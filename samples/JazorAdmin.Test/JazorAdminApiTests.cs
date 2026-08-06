// Exercises the in-host Identity, OpenIddict, organization, and operation-authorization integration boundary.
// 验证同宿主 Identity、OpenIddict、组织机构与操作授权的集成边界。
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using JazorAdmin.Authorization;
using JazorAdmin.Data;
using JazorAdmin.Features.Accounts;
using JazorAdmin.Features.Configuration;
using JazorAdmin.Features.Identity;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace JazorAdmin.Test;

[TestClass]
public sealed class JazorAdminApiTests
{
    [TestMethod]
    public async Task ApplicationShell_RootAndDeepNavigation_ReturnDynamicHtmlWithoutCapturingApiRoutes()
    {
        await using var factory = new JazorAdminFactory();
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });

        foreach (var path in new[]
                 {
                     "/",
                     "/organizations/structure",
                     "/accounts",
                     "/configuration/clients",
                     "/configuration/scopes"
                 })
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, path);
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("text/html"));
            using var response = await client.SendAsync(request);
            var document = await response.Content.ReadAsStringAsync();

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode, path);
            Assert.AreEqual("text/html", response.Content.Headers.ContentType?.MediaType, path);
            StringAssert.Contains(document, "<div id=\"app\"></div>", path);
            StringAssert.Contains(
                document,
                "src=\"/jazor/app.mjs\"",
                path);
        }

        using var apiRequest = new HttpRequestMessage(HttpMethod.Get, "/api/not-found");
        apiRequest.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("text/html"));
        using var apiResponse = await client.SendAsync(apiRequest);

        Assert.AreEqual(HttpStatusCode.NotFound, apiResponse.StatusCode);
    }

    [TestMethod]
    public async Task Session_WhenAnonymous_ReturnsUnauthorized()
    {
        await using var factory = new JazorAdminFactory();
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });

        var response = await client.GetAsync("/api/auth/session");

        Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [TestMethod]
    public async Task OrganizationGrant_WhenRevoked_StopsAccessImmediatelyAndKeepsOtherOrganizationsIsolated()
    {
        await using var factory = new JazorAdminFactory();
        var administrator = await factory.CreateUserAsync("admin@example.test", platformAdministrator: true);
        var operatorUser = await factory.CreateUserAsync("operator@example.test", platformAdministrator: false);
        using var administratorClient = await factory.CreateAuthenticatedClientAsync(administrator.Email, administrator.Password);

        var organizationId = await CreateOrganizationAsync(administratorClient, "clinical-west", "Clinical West");
        var isolatedOrganizationId = await CreateOrganizationAsync(administratorClient, "clinical-east", "Clinical East");
        Assert.AreNotEqual(organizationId, isolatedOrganizationId);
        var roleId = await CreateRoleAsync(administratorClient, organizationId, "organization-reader", "Organization reader");
        await ReplaceRoleGrantsAsync(administratorClient, organizationId, roleId, [
            new { resource = JazorAdminResources.Organizations, operation = JazorAdminOperations.Read }
        ]);
        await AddMemberAsync(administratorClient, organizationId, operatorUser.Email, [roleId]);

        using var operatorClient = await factory.CreateAuthenticatedClientAsync(operatorUser.Email, operatorUser.Password);
        var operatorSession = await operatorClient.GetFromJsonAsync<SessionResponse>("/api/auth/session");
        Assert.AreEqual(operatorUser.Email, operatorSession?.Email);
        CollectionAssert.DoesNotContain(
            (operatorSession?.Roles ?? []).ToArray(),
            JazorAdminRoles.PlatformAdministrator,
            "The operator client must not retain the administrator's Identity cookie.");
        var sessionOrganizationIds = operatorSession?.Organizations.Select(item => item.Id).ToArray() ?? [];
        Assert.IsTrue(
            sessionOrganizationIds.Contains(organizationId.ToString(), StringComparer.OrdinalIgnoreCase),
            "Session organization IDs: " + string.Join(", ", sessionOrganizationIds));
        Assert.IsFalse(
            sessionOrganizationIds.Contains(isolatedOrganizationId.ToString(), StringComparer.OrdinalIgnoreCase),
            "Session organization IDs: " + string.Join(", ", sessionOrganizationIds));
        Assert.AreEqual(HttpStatusCode.OK, (await operatorClient.GetAsync("/api/organizations/" + organizationId)).StatusCode);
        Assert.AreEqual(HttpStatusCode.Forbidden, (await operatorClient.GetAsync("/api/organizations/" + isolatedOrganizationId)).StatusCode);

        await ReplaceRoleGrantsAsync(administratorClient, organizationId, roleId, []);

        // The handler reads role grants for every request; a stale authentication cookie cannot retain access.
        // 处理器每次请求都读取角色授权，旧认证 Cookie 不能在授权撤销后继续访问。
        Assert.AreEqual(HttpStatusCode.Forbidden, (await operatorClient.GetAsync("/api/organizations/" + organizationId)).StatusCode);
    }

    [TestMethod]
    public async Task AuthorizationResources_ExposeOnlyImplementedAdministrationSurfaces()
    {
        await using var factory = new JazorAdminFactory();
        var administrator = await factory.CreateUserAsync("resources@example.test", platformAdministrator: true);
        using var client = await factory.CreateAuthenticatedClientAsync(administrator.Email, administrator.Password);
        var organizationId = await CreateOrganizationAsync(client, "resources-org", "Resources organization");

        using var response = await client.GetAsync("/api/organizations/" + organizationId + "/authorization-resources");
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var resourceOperations = document.RootElement.EnumerateArray()
            .Select(item => item.GetProperty("resource").GetString() + ":" + item.GetProperty("operation").GetString())
            .Order()
            .ToArray();

        CollectionAssert.AreEqual(
            new[]
            {
                "authorization:manage",
                "authorization:read",
                "organizations:manage",
                "organizations:read"
            },
            resourceOperations);
    }

    [TestMethod]
    public async Task PlatformAdministration_ManagesAccountsAndOpenIddictConfiguration()
    {
        await using var factory = new JazorAdminFactory();
        var administrator = await factory.CreateUserAsync("platform@example.test", platformAdministrator: true);
        var operatorUser = await factory.CreateUserAsync("member@example.test", platformAdministrator: false);
        using var administratorClient = await factory.CreateAuthenticatedClientAsync(administrator.Email, administrator.Password);
        using var operatorClient = await factory.CreateAuthenticatedClientAsync(operatorUser.Email, operatorUser.Password);

        using var denied = await operatorClient.GetAsync("/api/accounts/");
        Assert.AreEqual(HttpStatusCode.Forbidden, denied.StatusCode);

        using var createdAccount = await administratorClient.PostAsJsonAsync("/api/accounts/", new
        {
            email = "managed@example.test",
            displayName = "Managed account",
            password = "ManagedAccount123!",
            platformAdministrator = false
        });
        Assert.AreEqual(HttpStatusCode.Created, createdAccount.StatusCode);
        var account = await createdAccount.Content.ReadFromJsonAsync<AccountResponse>();
        Assert.IsNotNull(account);
        Assert.IsTrue(account.Enabled);
        Assert.IsFalse(account.PlatformAdministrator);

        using var disabledAccount = await administratorClient.PutAsJsonAsync(
            "/api/accounts/" + account.Id + "/enabled",
            new { enabled = false });
        Assert.AreEqual(HttpStatusCode.NoContent, disabledAccount.StatusCode);

        using var accountList = await administratorClient.GetAsync("/api/accounts/");
        var accounts = await accountList.Content.ReadFromJsonAsync<AccountResponse[]>();
        Assert.IsNotNull(accounts);
        Assert.IsFalse(accounts.Single(item => item.Id == account.Id).Enabled);

        using var createdScope = await administratorClient.PostAsJsonAsync("/api/configuration/scopes", new
        {
            name = "jazoradmin_reports",
            displayName = "Reporting API"
        });
        Assert.AreEqual(HttpStatusCode.Created, createdScope.StatusCode);
        var scope = await createdScope.Content.ReadFromJsonAsync<OpenIdScopeResponse>();
        Assert.IsNotNull(scope);
        CollectionAssert.Contains(scope.Resources.ToArray(), JazorAdminScopes.Api);

        using var createdClient = await administratorClient.PostAsJsonAsync("/api/configuration/clients", new
        {
            clientId = "reports-spa",
            displayName = "Reports SPA",
            redirectUris = new[] { "https://reports.example.test/auth/callback" },
            postLogoutRedirectUris = new[] { "https://reports.example.test/logout" },
            scopes = new[] { "jazoradmin_reports" }
        });
        Assert.AreEqual(HttpStatusCode.Created, createdClient.StatusCode);
        var client = await createdClient.Content.ReadFromJsonAsync<OpenIdClientResponse>();
        Assert.IsNotNull(client);
        CollectionAssert.Contains(client.Scopes.ToArray(), "jazoradmin_reports");

        using var clientList = await administratorClient.GetAsync("/api/configuration/clients");
        var clients = await clientList.Content.ReadFromJsonAsync<OpenIdClientResponse[]>();
        Assert.IsNotNull(clients);
        Assert.IsTrue(clients.Any(item => item.ClientId == "reports-spa"));
    }

    [TestMethod]
    public async Task AuthorizationCodeWithPkce_ExchangesForAccessAndRefreshTokens()
    {
        await using var factory = new JazorAdminFactory();
        var user = await factory.CreateUserAsync("sso@example.test", platformAdministrator: false);
        using var client = await factory.CreateAuthenticatedClientAsync(user.Email, user.Password, allowAutoRedirect: false);

        var discovery = await client.GetAsync("/.well-known/openid-configuration");
        Assert.AreEqual(HttpStatusCode.OK, discovery.StatusCode);

        const string verifier = "42k4yNdHICFuL_pniM.B2E9Iy-RGOrazoE06VBpdYM1HnJzi89sfFUix7drKs3b";
        var challenge = Base64Url(SHA256.HashData(Encoding.ASCII.GetBytes(verifier)));
        var authorizationPath = "/connect/authorize?client_id=jazoradmin-spa"
                                + "&response_type=code"
                                + "&redirect_uri=" + Uri.EscapeDataString("http://localhost/auth/callback")
                                + "&scope=" + Uri.EscapeDataString("openid profile offline_access jazoradmin_api")
                                + "&code_challenge_method=S256"
                                + "&code_challenge=" + Uri.EscapeDataString(challenge)
                                + "&state=integration-test";
        var authorization = await client.GetAsync(authorizationPath);

        Assert.AreEqual(HttpStatusCode.Redirect, authorization.StatusCode);
        var code = GetQueryValue(authorization.Headers.Location, "code");
        Assert.IsFalse(string.IsNullOrWhiteSpace(code));

        using var exchange = await client.PostAsync("/connect/token", new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["client_id"] = "jazoradmin-spa",
            ["grant_type"] = "authorization_code",
            ["redirect_uri"] = "http://localhost/auth/callback",
            ["code"] = code,
            ["code_verifier"] = verifier
        }));
        Assert.AreEqual(HttpStatusCode.OK, exchange.StatusCode);

        using var token = JsonDocument.Parse(await exchange.Content.ReadAsStringAsync());
        Assert.IsTrue(token.RootElement.TryGetProperty("access_token", out var accessToken));
        Assert.IsFalse(string.IsNullOrWhiteSpace(accessToken.GetString()));
        Assert.IsTrue(token.RootElement.TryGetProperty("refresh_token", out var refreshToken));
        Assert.IsFalse(string.IsNullOrWhiteSpace(refreshToken.GetString()));
    }

    private static async Task<Guid> CreateOrganizationAsync(HttpClient client, string code, string displayName)
    {
        using var response = await client.PostAsJsonAsync("/api/organizations", new { code, displayName });
        Assert.AreEqual(HttpStatusCode.Created, response.StatusCode);
        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        return document.RootElement.GetProperty("id").GetGuid();
    }

    private static async Task<Guid> CreateRoleAsync(HttpClient client, Guid organizationId, string code, string displayName)
    {
        using var response = await client.PostAsJsonAsync(
            "/api/organizations/" + organizationId + "/roles",
            new { code, displayName });
        Assert.AreEqual(HttpStatusCode.Created, response.StatusCode);
        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        return document.RootElement.GetProperty("id").GetGuid();
    }

    private static async Task AddMemberAsync(HttpClient client, Guid organizationId, string email, Guid[] roleIds)
    {
        using var response = await client.PostAsJsonAsync(
            "/api/organizations/" + organizationId + "/members",
            new { email, roleIds });
        Assert.AreEqual(HttpStatusCode.Created, response.StatusCode);
    }

    private static async Task ReplaceRoleGrantsAsync(HttpClient client, Guid organizationId, Guid roleId, object[] grants)
    {
        using var response = await client.PutAsJsonAsync(
            "/api/organizations/" + organizationId + "/roles/" + roleId + "/grants",
            new { grants });
        Assert.AreEqual(HttpStatusCode.NoContent, response.StatusCode);
    }

    private static string GetQueryValue(Uri? uri, string key)
    {
        Assert.IsNotNull(uri);
        foreach (var pair in uri.Query.TrimStart('?').Split('&', StringSplitOptions.RemoveEmptyEntries))
        {
            var separator = pair.IndexOf('=');
            if (separator > 0 && string.Equals(pair[..separator], key, StringComparison.Ordinal))
                return Uri.UnescapeDataString(pair[(separator + 1)..]);
        }

        Assert.Fail("Query value '" + key + "' was not found in the authorization redirect.");
        return string.Empty;
    }

    private static string Base64Url(byte[] value)
        => Convert.ToBase64String(value).TrimEnd('=').Replace('+', '-').Replace('/', '_');

    private sealed class JazorAdminFactory : WebApplicationFactory<Program>, IAsyncDisposable
    {
        private readonly string databasePath = Path.Combine(Path.GetTempPath(), "jazoradmin-test-" + Guid.NewGuid() + ".db");

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Testing");
            builder.ConfigureAppConfiguration(configuration => configuration.AddInMemoryCollection(new Dictionary<string, string?>
            {
                // File-backed SQLite catches relational translation issues, while disabling pooling lets
                // each isolated test release and remove its database deterministically.
                // 使用文件 SQLite 验证关系型查询；关闭连接池确保隔离数据库可被确定性清理。
                ["ConnectionStrings:JazorAdmin"] = "Data Source=" + databasePath + ";Pooling=False",
                ["JazorAdmin:OpenIddict:ClientId"] = "jazoradmin-spa",
                ["JazorAdmin:OpenIddict:RedirectUris:0"] = "http://localhost/auth/callback",
                ["JazorAdmin:OpenIddict:PostLogoutRedirectUris:0"] = "http://localhost/login"
            }));
        }

        public async Task<TestUser> CreateUserAsync(string email, bool platformAdministrator)
        {
            await using var scope = Services.CreateAsyncScope();
            var users = scope.ServiceProvider.GetRequiredService<UserManager<JazorAdminUser>>();
            var roles = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            if (platformAdministrator && !await roles.RoleExistsAsync(JazorAdminRoles.PlatformAdministrator))
            {
                var roleResult = await roles.CreateAsync(new IdentityRole(JazorAdminRoles.PlatformAdministrator));
                Assert.IsTrue(roleResult.Succeeded, string.Join(", ", roleResult.Errors.Select(error => error.Description)));
            }

            var user = new JazorAdminUser
            {
                UserName = email,
                Email = email,
                DisplayName = email.Split('@')[0],
                EmailConfirmed = true,
                LockoutEnabled = true
            };
            const string password = "JazorAdmin123!";
            var userResult = await users.CreateAsync(user, password);
            Assert.IsTrue(userResult.Succeeded, string.Join(", ", userResult.Errors.Select(error => error.Description)));
            if (platformAdministrator)
            {
                var roleResult = await users.AddToRoleAsync(user, JazorAdminRoles.PlatformAdministrator);
                Assert.IsTrue(roleResult.Succeeded, string.Join(", ", roleResult.Errors.Select(error => error.Description)));
            }

            return new TestUser(email, password);
        }

        public async Task<HttpClient> CreateAuthenticatedClientAsync(string email, string password, bool allowAutoRedirect = true)
        {
            var client = CreateClient(new WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = allowAutoRedirect,
                HandleCookies = true
            });
            using var response = await client.PostAsJsonAsync("/api/auth/login", new { email, password, rememberMe = false });
            Assert.AreEqual(HttpStatusCode.NoContent, response.StatusCode);
            return client;
        }

        public new async ValueTask DisposeAsync()
        {
            Dispose();
            await Task.Yield();
            if (File.Exists(databasePath))
                File.Delete(databasePath);
        }
    }

    private sealed record TestUser(string Email, string Password);
}
