// Exercises the in-host Identity, OpenIddict, organization, and operation-authorization integration boundary.
// 验证同宿主 Identity、OpenIddict、组织机构与操作授权的集成边界。
using System.Buffers.Binary;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using JazorAdmin.Authorization;
using JazorAdmin.Data;
using JazorAdmin.Features.Accounts;
using JazorAdmin.Features.Sso;
using JazorAdmin.Features.Scheduling;
using JazorAdmin.Features.Settings;
using JazorAdmin.Features.Identity;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace JazorAdmin.Test;

[TestClass]
[DoNotParallelize]
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
                     "/sso/applications",
                     "/sso/scopes",
                     "/sso/authorizations",
                     "/sso/tokens",
                     "/settings",
                     "/schedules"
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
            StringAssert.Contains(document, "href=\"/brand/jazor-mark.svg\"", path);
            StringAssert.Contains(document, "href=\"/favicon.ico\"", path);
        }

        using var apiRequest = new HttpRequestMessage(HttpMethod.Get, "/api/not-found");
        apiRequest.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("text/html"));
        using var apiResponse = await client.SendAsync(apiRequest);

        Assert.AreEqual(HttpStatusCode.NotFound, apiResponse.StatusCode);

        using var logoResponse = await client.GetAsync("/brand/jazor-mark.svg");
        Assert.AreEqual(HttpStatusCode.OK, logoResponse.StatusCode);
        Assert.AreEqual("image/svg+xml", logoResponse.Content.Headers.ContentType?.MediaType);

        using var artworkResponse = await client.GetAsync("/brand/login-art.webp");
        Assert.AreEqual(HttpStatusCode.OK, artworkResponse.StatusCode);
        Assert.AreEqual("image/webp", artworkResponse.Content.Headers.ContentType?.MediaType);

        using var iconResponse = await client.GetAsync("/favicon.ico");
        Assert.AreEqual(HttpStatusCode.OK, iconResponse.StatusCode);
        Assert.IsNotNull(iconResponse.Content.Headers.ContentType?.MediaType);
        var icon = await iconResponse.Content.ReadAsByteArrayAsync();
        AssertIcoSizes(icon, [16, 32, 48, 64]);
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
    public async Task SignIn_WhenAnonymousCaptchaIsInvalid_ReturnsBadRequest()
    {
        await using var factory = new JazorAdminFactory();
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });

        using var response = await client.PostAsJsonAsync(
            "/api/auth/login",
            new LoginRequest("missing@example.test", "InvalidPassword123!", CaptchaId: "missing", CaptchaAnswer: "ABCD"));

        Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [TestMethod]
    public async Task BootstrapAdministrator_CreatesFirstPlatformAdminAndAllowsSignIn()
    {
        const string email = "bootstrap@example.test";
        const string password = "BootstrapAdmin123!";
        await using var factory = new JazorAdminFactory(email, password);
        using var client = await factory.CreateAuthenticatedClientAsync(email, password);

        var session = await client.GetFromJsonAsync<SessionResponse>("/api/auth/session");

        Assert.IsNotNull(session);
        Assert.AreEqual(email, session.Email);
        CollectionAssert.Contains(session.Roles, JazorAdminRoles.PlatformAdministrator);
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

        using var createdScope = await administratorClient.PostAsJsonAsync("/api/sso/scopes", new
        {
            name = "reports",
            displayName = "Reporting access",
            description = "Read reports from the reporting API.",
            resources = new[] { "reports-api" }
        });
        Assert.AreEqual(HttpStatusCode.Created, createdScope.StatusCode);
        var scope = await createdScope.Content.ReadFromJsonAsync<ScopeView>();
        Assert.IsNotNull(scope);
        CollectionAssert.Contains(scope.Resources.ToArray(), "reports-api");

        using var updatedScope = await administratorClient.PutAsJsonAsync("/api/sso/scopes/" + scope.Id, new
        {
            displayName = "Reporting API access",
            description = "Updated scope description.",
            resources = new[] { "reports-api", "audit-api" }
        });
        Assert.AreEqual(HttpStatusCode.OK, updatedScope.StatusCode);
        var savedScope = await updatedScope.Content.ReadFromJsonAsync<ScopeView>();
        Assert.IsNotNull(savedScope);
        Assert.AreEqual("Updated scope description.", savedScope.Description);
        CollectionAssert.Contains(savedScope.Resources.ToArray(), "audit-api");

        using var createdMachine = await administratorClient.PostAsJsonAsync("/api/sso/applications", new
        {
            clientId = "reports-worker",
            displayName = "Reports worker",
            applicationType = "web",
            clientType = "confidential",
            consentType = "implicit",
            requirePkce = false,
            redirectUris = Array.Empty<string>(),
            postLogoutRedirectUris = Array.Empty<string>(),
            endpoints = new[] { "token", "revocation" },
            grantTypes = new[] { "client_credentials" },
            responseTypes = Array.Empty<string>(),
            scopes = new[] { "reports" }
        });
        Assert.AreEqual(HttpStatusCode.Created, createdMachine.StatusCode);
        var machine = await createdMachine.Content.ReadFromJsonAsync<AppSaved>();
        Assert.IsNotNull(machine);
        Assert.AreEqual("machine", machine.App.Profile);
        Assert.IsFalse(string.IsNullOrWhiteSpace(machine.Secret));

        using var updatedMachine = await administratorClient.PutAsJsonAsync(
            "/api/sso/applications/" + machine.App.Id,
            new
            {
                displayName = "Reports background worker",
                applicationType = "web",
                clientType = "confidential",
                consentType = "implicit",
                requirePkce = false,
                redirectUris = Array.Empty<string>(),
                postLogoutRedirectUris = Array.Empty<string>(),
                endpoints = new[] { "token", "revocation" },
                grantTypes = new[] { "client_credentials" },
                responseTypes = Array.Empty<string>(),
                scopes = new[] { "reports" }
            });
        Assert.AreEqual(HttpStatusCode.OK, updatedMachine.StatusCode);
        var savedMachine = await updatedMachine.Content.ReadFromJsonAsync<AppSaved>();
        Assert.IsNotNull(savedMachine);
        Assert.AreEqual("Reports background worker", savedMachine.App.DisplayName);
        Assert.IsNull(savedMachine.Secret, "Normal edits must not replace an existing client secret.");

        var accessToken = await ExchangeClientCredentialsAsync(machine.Secret!);

        using var createdApi = await administratorClient.PostAsJsonAsync("/api/sso/applications", new
        {
            clientId = "reports-api",
            displayName = "Reports API",
            applicationType = "web",
            clientType = "confidential",
            consentType = "implicit",
            requirePkce = false,
            redirectUris = Array.Empty<string>(),
            postLogoutRedirectUris = Array.Empty<string>(),
            endpoints = new[] { "introspection" },
            grantTypes = Array.Empty<string>(),
            responseTypes = Array.Empty<string>(),
            scopes = Array.Empty<string>()
        });
        Assert.AreEqual(HttpStatusCode.Created, createdApi.StatusCode);
        var api = await createdApi.Content.ReadFromJsonAsync<AppSaved>();
        Assert.IsNotNull(api);
        Assert.AreEqual("api", api.App.Profile);

        using var introspection = await administratorClient.PostAsync("/connect/introspect", new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["client_id"] = "reports-api",
            ["client_secret"] = api.Secret!,
            ["token"] = accessToken
        }));
        Assert.AreEqual(HttpStatusCode.OK, introspection.StatusCode);
        using (var payload = JsonDocument.Parse(await introspection.Content.ReadAsStringAsync()))
            Assert.IsTrue(payload.RootElement.GetProperty("active").GetBoolean());

        using var rotated = await administratorClient.PostAsync(
            "/api/sso/applications/" + machine.App.Id + "/secret",
            content: null);
        Assert.AreEqual(HttpStatusCode.OK, rotated.StatusCode);
        var secret = await rotated.Content.ReadFromJsonAsync<SecretView>();
        Assert.IsNotNull(secret);
        Assert.AreNotEqual(machine.Secret, secret.Secret);

        using var oldSecretExchange = await administratorClient.PostAsync("/connect/token", ClientCredentialsForm(machine.Secret!));
        Assert.AreEqual(HttpStatusCode.Unauthorized, oldSecretExchange.StatusCode);
        _ = await ExchangeClientCredentialsAsync(secret.Secret);

        using var applicationList = await administratorClient.GetAsync("/api/sso/applications");
        var applications = await applicationList.Content.ReadFromJsonAsync<AppView[]>();
        Assert.IsNotNull(applications);
        Assert.IsTrue(applications.Any(item => item.ClientId == "reports-worker"));
        Assert.IsTrue(applications.Any(item => item.ClientId == "reports-api"));

        using var deletedApi = await administratorClient.DeleteAsync("/api/sso/applications/" + api.App.Id);
        Assert.AreEqual(HttpStatusCode.NoContent, deletedApi.StatusCode);
        using var deletedMachine = await administratorClient.DeleteAsync("/api/sso/applications/" + machine.App.Id);
        Assert.AreEqual(HttpStatusCode.NoContent, deletedMachine.StatusCode);
        using var deletedScope = await administratorClient.DeleteAsync("/api/sso/scopes/" + scope.Id);
        Assert.AreEqual(HttpStatusCode.NoContent, deletedScope.StatusCode);

        async Task<string> ExchangeClientCredentialsAsync(string clientSecret)
        {
            using var response = await administratorClient.PostAsync("/connect/token", ClientCredentialsForm(clientSecret));
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode, await response.Content.ReadAsStringAsync());
            using var payload = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
            return payload.RootElement.GetProperty("access_token").GetString()!;
        }

        static FormUrlEncodedContent ClientCredentialsForm(string clientSecret)
            => new(new Dictionary<string, string>
            {
                ["client_id"] = "reports-worker",
                ["client_secret"] = clientSecret,
                ["grant_type"] = "client_credentials",
                ["scope"] = "reports"
            });
    }

    [TestMethod]
    public async Task PlatformAdministration_ManagesTypedSettingsAndQuartzSchedules()
    {
        await using var factory = new JazorAdminFactory();
        var administrator = await factory.CreateUserAsync("centers@example.test", platformAdministrator: true);
        var operatorUser = await factory.CreateUserAsync("centers-member@example.test", platformAdministrator: false);
        using var administratorClient = await factory.CreateAuthenticatedClientAsync(administrator.Email, administrator.Password);
        using var operatorClient = await factory.CreateAuthenticatedClientAsync(operatorUser.Email, operatorUser.Password);

        Assert.AreEqual(HttpStatusCode.Forbidden, (await operatorClient.GetAsync("/api/settings/")).StatusCode);
        Assert.AreEqual(HttpStatusCode.Forbidden, (await operatorClient.GetAsync("/api/schedules/")).StatusCode);

        using var created = await administratorClient.PostAsJsonAsync("/api/settings/", new
        {
            key = "feature.audit.enabled",
            group = "feature",
            label = "Audit log",
            description = "Enables the audit log pipeline.",
            kind = "boolean",
            value = "true"
        });
        Assert.AreEqual(HttpStatusCode.Created, created.StatusCode);
        var setting = await created.Content.ReadFromJsonAsync<SettingView>();
        Assert.IsNotNull(setting);
        Assert.AreEqual("boolean", setting.Kind);

        using var invalidSetting = await administratorClient.PostAsJsonAsync("/api/settings/", new
        {
            key = "feature.audit.invalid",
            group = "feature",
            label = "Invalid audit log",
            kind = "boolean",
            value = "enabled"
        });
        Assert.AreEqual(HttpStatusCode.BadRequest, invalidSetting.StatusCode);

        using var updated = await administratorClient.PutAsJsonAsync("/api/settings/feature.audit.enabled", new
        {
            group = "feature",
            label = "Audit log",
            description = "Enables and configures the audit log pipeline.",
            kind = "json",
            value = "{\"retentionDays\":30}"
        });
        Assert.AreEqual(HttpStatusCode.OK, updated.StatusCode);
        var saved = await updated.Content.ReadFromJsonAsync<SettingView>();
        Assert.IsNotNull(saved);
        Assert.AreEqual("json", saved.Kind);

        using var listed = await administratorClient.GetAsync("/api/settings/");
        var settings = await listed.Content.ReadFromJsonAsync<SettingView[]>();
        Assert.IsNotNull(settings);
        Assert.AreEqual("{\"retentionDays\":30}", settings.Single(item => item.Key == "feature.audit.enabled").Value);

        using var deleted = await administratorClient.DeleteAsync("/api/settings/feature.audit.enabled");
        Assert.AreEqual(HttpStatusCode.NoContent, deleted.StatusCode);

        using var scheduleList = await administratorClient.GetAsync("/api/schedules/");
        Assert.AreEqual(HttpStatusCode.OK, scheduleList.StatusCode);
        var schedules = await scheduleList.Content.ReadFromJsonAsync<ScheduleView[]>();
        Assert.IsNotNull(schedules);
        var schedule = schedules.Single(item => item.Key == "openid-prune");
        Assert.IsTrue(schedule.Enabled);

        using var invalidCron = await administratorClient.PutAsJsonAsync("/api/schedules/openid-prune", new
        {
            cron = "not a cron",
            enabled = true
        });
        Assert.AreEqual(HttpStatusCode.BadRequest, invalidCron.StatusCode);

        using var paused = await administratorClient.PutAsJsonAsync("/api/schedules/openid-prune", new
        {
            cron = "0 0 3 * * ?",
            enabled = false
        });
        Assert.AreEqual(HttpStatusCode.OK, paused.StatusCode);
        var pausedSchedule = await paused.Content.ReadFromJsonAsync<ScheduleView>();
        Assert.IsNotNull(pausedSchedule);
        Assert.IsFalse(pausedSchedule.Enabled);
        Assert.IsNull(pausedSchedule.NextRunAt);

        using var triggered = await administratorClient.PostAsync("/api/schedules/openid-prune/run", content: null);
        Assert.AreEqual(HttpStatusCode.Accepted, triggered.StatusCode);
        var run = await WaitForRunAsync(administratorClient, "openid-prune");
        Assert.AreEqual("manual", run.Trigger);
        Assert.AreEqual("succeeded", run.Status);
        StringAssert.Contains(run.Message, "Pruned");
    }

    [TestMethod]
    public async Task ExplicitConsent_RendersConfirmationAndCreatesPermanentAuthorization()
    {
        await using var factory = new JazorAdminFactory();
        var user = await factory.CreateUserAsync("consent@example.test", platformAdministrator: true);
        using var client = await factory.CreateAuthenticatedClientAsync(
            user.Email,
            user.Password,
            allowAutoRedirect: false);

        using var created = await client.PostAsJsonAsync("/api/sso/applications", new
        {
            clientId = "consent-web",
            displayName = "Consent web application",
            applicationType = "web",
            clientType = "public",
            consentType = "explicit",
            requirePkce = false,
            redirectUris = new[] { "http://localhost/consent/callback" },
            postLogoutRedirectUris = Array.Empty<string>(),
            endpoints = new[] { "authorization", "token" },
            grantTypes = new[] { "authorization_code" },
            responseTypes = new[] { "code" },
            scopes = new[] { "openid" }
        });
        Assert.AreEqual(HttpStatusCode.Created, created.StatusCode);

        var authorizationPath = "/connect/authorize?client_id=consent-web"
                                + "&response_type=code"
                                + "&redirect_uri=" + Uri.EscapeDataString("http://localhost/consent/callback")
                                + "&scope=openid"
                                + "&state=consent-test";
        using var confirmation = await client.GetAsync(authorizationPath);
        Assert.AreEqual(HttpStatusCode.OK, confirmation.StatusCode);
        var confirmationHtml = await confirmation.Content.ReadAsStringAsync();
        StringAssert.Contains(confirmationHtml, "data-access-page=\"consent\"");
        StringAssert.Contains(confirmationHtml, "Consent web application");
        StringAssert.Contains(confirmationHtml, "name=\"client_id\" value=\"consent-web\"");
        StringAssert.Contains(confirmationHtml, "name=\"state\" value=\"consent-test\"");

        using var accepted = await client.PostAsync(
            "/connect/authorize",
            new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["client_id"] = "consent-web",
                ["response_type"] = "code",
                ["redirect_uri"] = "http://localhost/consent/callback",
                ["scope"] = "openid",
                ["state"] = "consent-test",
                ["decision"] = "accept"
            }));
        Assert.AreEqual(HttpStatusCode.Redirect, accepted.StatusCode);
        Assert.IsFalse(string.IsNullOrWhiteSpace(GetQueryValue(accepted.Headers.Location, "code")));

        using var authorizationsResponse = await client.GetAsync("/api/sso/authorizations");
        var authorizations = await authorizationsResponse.Content.ReadFromJsonAsync<AuthorizationView[]>();
        Assert.IsNotNull(authorizations);
        Assert.IsTrue(authorizations.Any(value =>
            value.ClientId == "consent-web" && value.Type == "permanent" && value.Status == "valid"));

        using var reused = await client.GetAsync(authorizationPath);
        Assert.AreEqual(HttpStatusCode.Redirect, reused.StatusCode);
    }

    [TestMethod]
    public async Task SystematicConsent_RequiresConfirmationForEachAuthorization()
    {
        await using var factory = new JazorAdminFactory();
        var user = await factory.CreateUserAsync("systematic@example.test", platformAdministrator: true);
        using var client = await factory.CreateAuthenticatedClientAsync(
            user.Email,
            user.Password,
            allowAutoRedirect: false);

        using var created = await client.PostAsJsonAsync("/api/sso/applications", new
        {
            clientId = "systematic-web",
            displayName = "Systematic web application",
            applicationType = "web",
            clientType = "public",
            consentType = "systematic",
            requirePkce = false,
            redirectUris = new[] { "http://localhost/systematic/callback" },
            postLogoutRedirectUris = Array.Empty<string>(),
            endpoints = new[] { "authorization", "token" },
            grantTypes = new[] { "authorization_code" },
            responseTypes = new[] { "code" },
            scopes = new[] { "openid" }
        });
        Assert.AreEqual(HttpStatusCode.Created, created.StatusCode);

        var authorizationPath = "/connect/authorize?client_id=systematic-web"
                                + "&response_type=code"
                                + "&redirect_uri=" + Uri.EscapeDataString("http://localhost/systematic/callback")
                                + "&scope=openid";
        using var confirmation = await client.GetAsync(authorizationPath);
        Assert.AreEqual(HttpStatusCode.OK, confirmation.StatusCode);

        using var accepted = await client.PostAsync(
            "/connect/authorize",
            new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["client_id"] = "systematic-web",
                ["response_type"] = "code",
                ["redirect_uri"] = "http://localhost/systematic/callback",
                ["scope"] = "openid",
                ["decision"] = "accept"
            }));
        Assert.AreEqual(HttpStatusCode.Redirect, accepted.StatusCode);

        using var repeated = await client.GetAsync(authorizationPath);
        Assert.AreEqual(HttpStatusCode.OK, repeated.StatusCode);
        StringAssert.Contains(await repeated.Content.ReadAsStringAsync(), "data-access-page=\"consent\"");

        using var authorizationsResponse = await client.GetAsync("/api/sso/authorizations");
        var authorizations = await authorizationsResponse.Content.ReadFromJsonAsync<AuthorizationView[]>();
        Assert.IsNotNull(authorizations);
        Assert.IsTrue(authorizations.Any(value =>
            value.ClientId == "systematic-web" && value.Type == "ad-hoc" && value.Status == "valid"));
    }

    [TestMethod]
    public async Task AuthorizationCodeWithPkce_ExchangesForAccessAndRefreshTokens()
    {
        await using var factory = new JazorAdminFactory();
        var user = await factory.CreateUserAsync("sso@example.test", platformAdministrator: true);
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

        using var authorizationsResponse = await client.GetAsync("/api/sso/authorizations");
        Assert.AreEqual(HttpStatusCode.OK, authorizationsResponse.StatusCode);
        var authorizations = await authorizationsResponse.Content.ReadFromJsonAsync<AuthorizationView[]>();
        Assert.IsNotNull(authorizations);
        var authorizationRecord = authorizations.First(item => item.ClientId == "jazoradmin-spa" && item.Status == "valid");

        using var tokensResponse = await client.GetAsync("/api/sso/tokens");
        Assert.AreEqual(HttpStatusCode.OK, tokensResponse.StatusCode);
        var tokens = await tokensResponse.Content.ReadFromJsonAsync<TokenView[]>();
        Assert.IsNotNull(tokens);
        var accessTokenRecord = tokens.First(item =>
            item.ClientId == "jazoradmin-spa" &&
            item.Type == "access_token" &&
            item.Status == "valid");

        using var revokedToken = await client.PostAsync("/api/sso/tokens/" + accessTokenRecord.Id + "/revoke", content: null);
        Assert.AreEqual(HttpStatusCode.NoContent, revokedToken.StatusCode);
        using var revokedAuthorization = await client.PostAsync("/api/sso/authorizations/" + authorizationRecord.Id + "/revoke", content: null);
        Assert.AreEqual(HttpStatusCode.NoContent, revokedAuthorization.StatusCode);

        using var updatedTokensResponse = await client.GetAsync("/api/sso/tokens");
        var updatedTokens = await updatedTokensResponse.Content.ReadFromJsonAsync<TokenView[]>();
        Assert.IsNotNull(updatedTokens);
        Assert.AreEqual("revoked", updatedTokens.Single(item => item.Id == accessTokenRecord.Id).Status);

        using var updatedAuthorizationsResponse = await client.GetAsync("/api/sso/authorizations");
        var updatedAuthorizations = await updatedAuthorizationsResponse.Content.ReadFromJsonAsync<AuthorizationView[]>();
        Assert.IsNotNull(updatedAuthorizations);
        Assert.AreEqual("revoked", updatedAuthorizations.Single(item => item.Id == authorizationRecord.Id).Status);
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

    private static void AssertIcoSizes(byte[] icon, int[] expectedSizes)
    {
        Assert.IsTrue(icon.Length >= 6 + (expectedSizes.Length * 16));
        Assert.AreEqual((ushort)0, BinaryPrimitives.ReadUInt16LittleEndian(icon));
        Assert.AreEqual((ushort)1, BinaryPrimitives.ReadUInt16LittleEndian(icon.AsSpan(2)));
        Assert.AreEqual((ushort)expectedSizes.Length, BinaryPrimitives.ReadUInt16LittleEndian(icon.AsSpan(4)));

        for (var index = 0; index < expectedSizes.Length; index++)
        {
            var entry = icon.AsSpan(6 + (index * 16), 16);
            Assert.AreEqual((byte)expectedSizes[index], entry[0]);
            Assert.AreEqual((byte)expectedSizes[index], entry[1]);
            Assert.AreEqual((ushort)32, BinaryPrimitives.ReadUInt16LittleEndian(entry[6..]));
            var imageLength = BinaryPrimitives.ReadInt32LittleEndian(entry[8..]);
            var imageOffset = BinaryPrimitives.ReadInt32LittleEndian(entry[12..]);
            Assert.IsTrue(imageLength > 40);
            Assert.IsTrue(imageOffset >= 6 + (expectedSizes.Length * 16));
            Assert.IsTrue(imageOffset + imageLength <= icon.Length);
        }
    }

    private static string Base64Url(byte[] value)
        => Convert.ToBase64String(value).TrimEnd('=').Replace('+', '-').Replace('/', '_');

    private static async Task<ScheduleRunView> WaitForRunAsync(HttpClient client, string key)
    {
        for (var attempt = 0; attempt < 50; attempt++)
        {
            using var response = await client.GetAsync("/api/schedules/" + key + "/runs");
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            var runs = await response.Content.ReadFromJsonAsync<ScheduleRunView[]>();
            var run = runs?.FirstOrDefault();
            if (run is not null && run.Status != "running")
                return run;

            await Task.Delay(100);
        }

        Assert.Fail("Quartz did not finish the manual task run in time.");
        return null!;
    }

    private sealed class JazorAdminFactory : WebApplicationFactory<Program>, IAsyncDisposable
    {
        private readonly string databasePath = Path.Combine(Path.GetTempPath(), "jazoradmin-test-" + Guid.NewGuid() + ".db");
        private readonly string? bootstrapEmail;
        private readonly string? bootstrapPassword;

        public JazorAdminFactory(string? bootstrapEmail = null, string? bootstrapPassword = null)
        {
            this.bootstrapEmail = bootstrapEmail;
            this.bootstrapPassword = bootstrapPassword;
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            // Quartz DI owns a process-global log provider. Reset it before each short-lived test host starts,
            // otherwise the next host can observe the previous host's disposed ILoggerFactory.
            // Quartz DI 使用进程级日志 Provider；短生命周期测试宿主启动前必须重置，避免访问上一个宿主
            // 已释放的 ILoggerFactory。该全局限制也是此测试类不能并行执行的原因。
            Quartz.Logging.LogContext.SetCurrentLogProvider(
                Microsoft.Extensions.Logging.Abstractions.NullLoggerFactory.Instance);

            builder.UseEnvironment("Testing");
            builder.ConfigureAppConfiguration(configuration =>
            {
                var values = new Dictionary<string, string?>
                {
                    // File-backed SQLite catches relational translation issues, while disabling pooling lets
                    // each isolated test release and remove its database deterministically.
                    // 使用文件 SQLite 验证关系型查询；关闭连接池确保隔离数据库可被确定性清理。
                    ["ConnectionStrings:JazorAdmin"] = "Data Source=" + databasePath + ";Pooling=False",
                    ["JazorAdmin:OpenIddict:ClientId"] = "jazoradmin-spa",
                    ["JazorAdmin:OpenIddict:RedirectUris:0"] = "http://localhost/auth/callback",
                    ["JazorAdmin:OpenIddict:PostLogoutRedirectUris:0"] = "http://localhost/login"
                };
                if (bootstrapEmail is not null || bootstrapPassword is not null)
                {
                    values["JazorAdmin:Bootstrap:Email"] = bootstrapEmail;
                    values["JazorAdmin:Bootstrap:Password"] = bootstrapPassword;
                }

                configuration.AddInMemoryCollection(values);
            });
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
            using var challengeResponse = await client.GetAsync("/api/auth/captcha");
            Assert.AreEqual(HttpStatusCode.OK, challengeResponse.StatusCode);
            var challenge = await challengeResponse.Content.ReadFromJsonAsync<CaptchaChallengeResponse>();
            Assert.IsNotNull(challenge);

            using var imageResponse = await client.GetAsync(challenge.ImageUrl);
            Assert.AreEqual(HttpStatusCode.OK, imageResponse.StatusCode);
            var image = await imageResponse.Content.ReadAsStringAsync();
            var answer = string.Concat(Regex.Matches(image, "<text[^>]*>([A-Z0-9])</text>").Select(match => match.Groups[1].Value));
            Assert.AreEqual(4, answer.Length);

            using var response = await client.PostAsJsonAsync(
                "/api/auth/login",
                new LoginRequest(email, password, CaptchaId: challenge.Id, CaptchaAnswer: answer));
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
