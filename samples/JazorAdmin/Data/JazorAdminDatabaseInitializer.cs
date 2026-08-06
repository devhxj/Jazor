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
    ILogger<JazorAdminDatabaseInitializer> logger) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await using var scope = scopeFactory.CreateAsyncScope();
        var database = scope.ServiceProvider.GetRequiredService<JazorAdminDbContext>();
        await database.Database.MigrateAsync(cancellationToken);
        await SeedPlatformRoleAsync(scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>());
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
        if (await applications.FindByClientIdAsync(options.ClientId, cancellationToken) is not null)
            return;

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

        await applications.CreateAsync(descriptor, cancellationToken);
    }
}
