// Applies the local schema and seeds authorization metadata plus the first-party OpenIddict client.
// 应用本地数据库架构，并初始化授权元数据和第一方 OpenIddict 客户端。
using JazorAdmin.Authentication;
using JazorAdmin.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using OpenIddict.Abstractions;

namespace JazorAdmin.Data;

public sealed class JazorAdminDatabaseInitializer(
    IServiceScopeFactory scopeFactory,
    IOptions<JazorAdminOpenIddictOptions> openIddictOptions,
    IOptions<BootstrapOptions> bootstrapOptions,
    IHostEnvironment environment,
    ILogger<JazorAdminDatabaseInitializer> logger) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await using var scope = scopeFactory.CreateAsyncScope();
        var database = scope.ServiceProvider.GetRequiredService<JazorAdminDbContext>();
        await database.Database.MigrateAsync(cancellationToken);
        var roles = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        await SeedPlatformRoleAsync(roles);
        await SeedBootstrapAdministratorAsync(scope.ServiceProvider.GetRequiredService<UserManager<JazorAdminUser>>());
        await SeedAuthorizationCatalogAsync(database, cancellationToken);
        await SeedSpaClientAsync(
            scope.ServiceProvider.GetRequiredService<IOpenIddictApplicationManager>(),
            cancellationToken);

        logger.LogInformation("JazorAdmin identity and authorization store is ready.");
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    private static async Task SeedPlatformRoleAsync(RoleManager<IdentityRole> roles)
    {
        if (await roles.RoleExistsAsync(JazorAdminRoles.PlatformAdministrator))
            return;

        var result = await roles.CreateAsync(new IdentityRole(JazorAdminRoles.PlatformAdministrator));
        if (!result.Succeeded)
            throw new InvalidOperationException("Unable to create the platform administrator role.");
    }

    private async Task SeedBootstrapAdministratorAsync(UserManager<JazorAdminUser> users)
    {
        var platformAdministrators = await users.GetUsersInRoleAsync(JazorAdminRoles.PlatformAdministrator);
        if (platformAdministrators.Count > 0)
            return;

        var options = bootstrapOptions.Value;
        var hasEmail = !string.IsNullOrWhiteSpace(options.Email);
        var hasPassword = !string.IsNullOrWhiteSpace(options.Password);
        if (!hasEmail && !hasPassword && environment.IsEnvironment("Testing"))
            return;

        if (!hasEmail || !hasPassword)
        {
            throw new InvalidOperationException(
                "No platform administrator exists. Configure JazorAdmin:Bootstrap:Email and " +
                "JazorAdmin:Bootstrap:Password before starting the application.");
        }

        var user = await users.FindByEmailAsync(options.Email!);
        if (user is null)
        {
            user = new JazorAdminUser
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

        if (!await users.IsInRoleAsync(user, JazorAdminRoles.PlatformAdministrator))
        {
            var result = await users.AddToRoleAsync(user, JazorAdminRoles.PlatformAdministrator);
            if (!result.Succeeded)
                throw new InvalidOperationException("Unable to grant the bootstrap administrator role: " +
                    string.Join(", ", result.Errors.Select(error => error.Description)));
        }

        logger.LogInformation("Bootstrap platform administrator '{Email}' is ready.", user.Email);
    }

    private static async Task SeedAuthorizationCatalogAsync(
        JazorAdminDbContext database,
        CancellationToken cancellationToken)
    {
        var definitions = new[]
        {
            (JazorAdminResources.Organizations, "Organizations"),
            (JazorAdminResources.Authorization, "Authorization")
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

            foreach (var operation in new[] { JazorAdminOperations.Read, JazorAdminOperations.Manage })
            {
                if (await database.AuthorizationOperations.FindAsync([key, operation], cancellationToken) is not null)
                    continue;

                database.AuthorizationOperations.Add(new AuthorizationOperation
                {
                    ResourceKey = key,
                    Key = operation,
                    DisplayName = operation == JazorAdminOperations.Read ? "Read" : "Manage"
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
            OpenIddictConstants.Permissions.Prefixes.Scope + JazorAdminScopes.Api
        ]);
        descriptor.Requirements.Add(OpenIddictConstants.Requirements.Features.ProofKeyForCodeExchange);

        if (application is null)
            await applications.CreateAsync(descriptor, cancellationToken);
        else
            await applications.UpdateAsync(application, descriptor, cancellationToken);
    }
}
