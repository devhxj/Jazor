// Applies the local schema and seeds authorization metadata plus the first-party OpenIddict client.
// 应用本地数据库架构，并初始化授权元数据和第一方 OpenIddict 客户端。
using JazorAdmin.Authentication;
using JazorAdmin.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using OpenIddict.Abstractions;

namespace JazorAdmin.Data;

public sealed class DatabaseInitializer(
    IServiceScopeFactory scopeFactory,
    IOptions<OpenIddictOptions> openIddictOptions,
    IOptions<BootstrapOptions> bootstrapOptions,
    IHostEnvironment environment,
    ILogger<DatabaseInitializer> logger) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await using var scope = scopeFactory.CreateAsyncScope();
        var database = scope.ServiceProvider.GetRequiredService<AdminDbContext>();
        await database.Database.MigrateAsync(cancellationToken);
        await BackfillScheduleRunUtcValuesAsync(database, cancellationToken);
        var roles = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        await SeedPlatformRoleAsync(roles);
        var bootstrapAdministrator = await SeedBootstrapAdministratorAsync(
            scope.ServiceProvider.GetRequiredService<UserManager<AdminUser>>());
        await SeedAuthorizationCatalogAsync(database, cancellationToken);
        await SeedDevelopmentWorkspaceAsync(database, bootstrapAdministrator, cancellationToken);
        await SeedSpaClientAsync(
            scope.ServiceProvider.GetRequiredService<IOpenIddictApplicationManager>(),
            cancellationToken);

        logger.LogInformation("JazorAdmin identity and authorization store is ready.");
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    private static async Task BackfillScheduleRunUtcValuesAsync(
        AdminDbContext database,
        CancellationToken cancellationToken)
    {
        const int BatchSize = 512;
        while (true)
        {
            var legacyRuns = await database.ScheduleRuns
                .Where(run => run.StartedAtUtc == null)
                .Take(BatchSize)
                .ToArrayAsync(cancellationToken);
            if (legacyRuns.Length == 0)
                return;

            // Materialize in bounded batches. The exact DateTimeOffset -> UTC conversion belongs to
            // .NET; SQLite's date helpers would round fractional seconds during a migration backfill.
            // 有界批次在 .NET 中完成精确 DateTimeOffset -> UTC 转换；SQLite 日期函数会在迁移回填
            // 时舍入小数秒，不能承担这个语义。
            foreach (var run in legacyRuns)
                run.NormalizeStartedAtUtc();
            await database.SaveChangesAsync(cancellationToken);
            // SaveChanges keeps accepted entities tracked. Clear completed batches so a long-lived
            // legacy history remains bounded in memory throughout the one-time startup backfill.
            // SaveChanges 后实体仍被跟踪；清理已完成批次，保证一次性历史回填在长任务历史下也保持
            // 有界内存。
            database.ChangeTracker.Clear();
        }
    }

    private static async Task SeedPlatformRoleAsync(RoleManager<IdentityRole> roles)
    {
        if (await roles.RoleExistsAsync(RoleKeys.PlatformAdministrator))
            return;

        var result = await roles.CreateAsync(new IdentityRole(RoleKeys.PlatformAdministrator));
        if (!result.Succeeded)
            throw new InvalidOperationException("Unable to create the platform administrator role.");
    }

    private async Task<AdminUser?> SeedBootstrapAdministratorAsync(UserManager<AdminUser> users)
    {
        var platformAdministrators = await users.GetUsersInRoleAsync(RoleKeys.PlatformAdministrator);
        if (platformAdministrators.Count > 0)
            return platformAdministrators[0];

        var options = bootstrapOptions.Value;
        var hasEmail = !string.IsNullOrWhiteSpace(options.Email);
        var hasPassword = !string.IsNullOrWhiteSpace(options.Password);
        if (!hasEmail && !hasPassword && environment.IsEnvironment("Testing"))
            return null;

        if (!hasEmail || !hasPassword)
        {
            throw new InvalidOperationException(
                "No platform administrator exists. Configure JazorAdmin:Bootstrap:Email and " +
                "JazorAdmin:Bootstrap:Password before starting the application.");
        }

        var user = await users.FindByEmailAsync(options.Email!);
        if (user is null)
        {
            user = new AdminUser
            {
                UserName = options.Email,
                Email = options.Email,
                DisplayName = options.DisplayName,
                EmailConfirmed = true,
                LockoutEnabled = true
            };
            var result = await users.CreateAsync(user, options.Password!);
            if (!result.Succeeded)
                throw new InvalidOperationException("Unable to create the bootstrap administrator: " +
                    string.Join(", ", result.Errors.Select(error => error.Description)));
        }

        if (!await users.IsInRoleAsync(user, RoleKeys.PlatformAdministrator))
        {
            var result = await users.AddToRoleAsync(user, RoleKeys.PlatformAdministrator);
            if (!result.Succeeded)
                throw new InvalidOperationException("Unable to grant the bootstrap administrator role: " +
                    string.Join(", ", result.Errors.Select(error => error.Description)));
        }

        logger.LogInformation("Bootstrap platform administrator '{Email}' is ready.", user.Email);
        return user;
    }

    private async Task SeedDevelopmentWorkspaceAsync(
        AdminDbContext database,
        AdminUser? administrator,
        CancellationToken cancellationToken)
    {
        // The usable first-run workspace is intentionally development-only. Production operators
        // configure their own tenant model; tests keep complete ownership of their data setup.
        if (!environment.IsDevelopment() || administrator is null ||
            await database.Organizations.AnyAsync(cancellationToken))
        {
            return;
        }

        var organization = new Organization
        {
            Code = "jazor",
            DisplayName = "Jazor Development"
        };
        var administratorRole = new OrganizationRole
        {
            OrganizationId = organization.Id,
            Code = "workspace-admin",
            DisplayName = "Workspace administrator"
        };
        var membership = new OrganizationMembership
        {
            OrganizationId = organization.Id,
            UserId = administrator.Id
        };

        database.Organizations.Add(organization);
        database.OrganizationRoles.Add(administratorRole);
        database.OrganizationMemberships.Add(membership);
        database.OrganizationMembershipRoles.Add(new OrganizationMembershipRole
        {
            MembershipId = membership.Id,
            RoleId = administratorRole.Id
        });

        foreach (var resource in new[] { ResourceKeys.Organizations, ResourceKeys.Authorization })
        {
            foreach (var operation in new[] { OperationKeys.Read, OperationKeys.Manage })
            {
                database.ResourceOperationGrants.Add(new ResourceOperationGrant
                {
                    RoleId = administratorRole.Id,
                    ResourceKey = resource,
                    OperationKey = operation
                });
            }
        }

        database.Settings.AddRange(
            new Setting
            {
                Key = "feature.audit.enabled",
                Group = "feature",
                Label = "Audit events",
                Description = "Records administrative changes in the development workspace.",
                Kind = "boolean",
                Value = "true",
                UpdatedAt = DateTimeOffset.UtcNow
            },
            new Setting
            {
                Key = "runtime.display-name",
                Group = "runtime",
                Label = "Application display name",
                Description = "The name presented by the development workspace.",
                Kind = "text",
                Value = "JazorAdmin",
                UpdatedAt = DateTimeOffset.UtcNow
            });

        await database.SaveChangesAsync(cancellationToken);
        logger.LogInformation("Seeded the development workspace for '{Email}'.", administrator.Email);
    }

    private static async Task SeedAuthorizationCatalogAsync(
        AdminDbContext database,
        CancellationToken cancellationToken)
    {
        var definitions = new[]
        {
            (ResourceKeys.Organizations, "Organizations"),
            (ResourceKeys.Authorization, "Authorization")
        };

        foreach (var (key, displayName) in definitions)
        {
            if (await database.AuthorizationResources.FindAsync([key], cancellationToken) is null)
            {
                database.AuthorizationResources.Add(new AuthorizationResource
                {
                    Key = key,
                    DisplayName = displayName
                });
            }

            foreach (var operation in new[] { OperationKeys.Read, OperationKeys.Manage })
            {
                if (await database.AuthorizationOperations.FindAsync([key, operation], cancellationToken) is not null)
                    continue;

                database.AuthorizationOperations.Add(new AuthorizationOperation
                {
                    ResourceKey = key,
                    Key = operation,
                    DisplayName = operation == OperationKeys.Read ? "Read" : "Manage"
                });
            }
        }

        await database.SaveChangesAsync(cancellationToken);
    }

    private async Task SeedSpaClientAsync(
        IOpenIddictApplicationManager applications,
        CancellationToken cancellationToken)
    {
        var options = openIddictOptions.Value;
        if (options.RedirectUris.Length == 0 || options.PostLogoutRedirectUris.Length == 0)
        {
            throw new InvalidOperationException(
                "Configure JazorAdmin:OpenIddict:RedirectUris and " +
                "JazorAdmin:OpenIddict:PostLogoutRedirectUris with the public application origin.");
        }

        var application = await applications.FindByClientIdAsync(options.ClientId, cancellationToken);
        var descriptor = new OpenIddictApplicationDescriptor
        {
            ClientId = options.ClientId,
            ClientType = OpenIddictConstants.ClientTypes.Public,
            ConsentType = OpenIddictConstants.ConsentTypes.Implicit,
            DisplayName = "JazorAdmin RazorVue"
        };
        descriptor.RedirectUris.UnionWith(options.RedirectUris.Select(static uri => new Uri(uri, UriKind.Absolute)));
        descriptor.PostLogoutRedirectUris.UnionWith(options.PostLogoutRedirectUris.Select(static uri => new Uri(uri, UriKind.Absolute)));
        descriptor.Permissions.UnionWith(
        [
            OpenIddictConstants.Permissions.Endpoints.Authorization,
            OpenIddictConstants.Permissions.Endpoints.EndSession,
            OpenIddictConstants.Permissions.Endpoints.Token,
            OpenIddictConstants.Permissions.GrantTypes.AuthorizationCode,
            OpenIddictConstants.Permissions.GrantTypes.RefreshToken,
            OpenIddictConstants.Permissions.ResponseTypes.Code,
            OpenIddictConstants.Permissions.Scopes.Email,
            OpenIddictConstants.Permissions.Scopes.Profile,
            OpenIddictConstants.Permissions.Scopes.Roles,
            OpenIddictConstants.Permissions.Prefixes.Scope + ScopeKeys.Api
        ]);
        descriptor.Requirements.Add(OpenIddictConstants.Requirements.Features.ProofKeyForCodeExchange);

        if (application is null)
            await applications.CreateAsync(descriptor, cancellationToken);
        else
            await applications.UpdateAsync(application, descriptor, cancellationToken);
    }
}
