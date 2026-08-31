using JazorAdmin.Features.Accounts;
using JazorAdmin.Features.Audit;
using JazorAdmin.Features.Identity;
using JazorAdmin.Features.Notifications;
using JazorAdmin.Features.Organizations;
using JazorAdmin.Features.Overview;
using JazorAdmin.Features.Scheduling;
using JazorAdmin.Features.Settings;
using JazorAdmin.Features.Sso;
using static ECMAScript.Global;

namespace JazorAdmin;

/// <summary>
/// Owns same-origin HTTP transport through browser WebIDL bindings.
/// 业务页面只依赖此边界，不维护手写 JavaScript bridge。
/// </summary>
[ECMAScriptModule("components/api-client.mjs")]
public static class ApiClient
{
    public static IPromise<ApiOutcome> GetSession()
        => Get("/api/auth/session");

    public static IPromise<ApiOutcome> GetCaptcha()
        => Get("/api/auth/captcha");

    public static IPromise<ApiOutcome> SignIn(string email, string password, string? captchaId = null, string? captchaAnswer = null)
        => Send("/api/auth/login", "POST", new LoginRequest(email, password, CaptchaId: captchaId, CaptchaAnswer: captchaAnswer));

    public static IPromise<ApiOutcome> SignOut()
        => Send("/api/auth/logout", "POST");

    public static IPromise<ApiOutcome> GetOrganizations()
        => Get("/api/organizations/");

    public static IPromise<ApiOutcome> GetOrganization(string organizationId)
        => Get("/api/organizations/" + organizationId);

    public static IPromise<ApiOutcome> GetMembers(string organizationId)
        => Get("/api/organizations/" + organizationId + "/members");

    public static IPromise<ApiOutcome> GetRoles(string organizationId)
        => Get("/api/organizations/" + organizationId + "/roles");

    public static IPromise<ApiOutcome> GetRoleGrants(string organizationId, string roleId)
        => Get("/api/organizations/" + organizationId + "/roles/" + roleId + "/grants");

    public static IPromise<ApiOutcome> GetAuthorizationResources(string organizationId)
        => Get("/api/organizations/" + organizationId + "/authorization-resources");

    public static IPromise<ApiOutcome> CreateOrganization(string code, string displayName)
        => Send("/api/organizations/", "POST", new CreateOrganizationRequest(code, displayName));

    public static IPromise<ApiOutcome> CreateChildOrganization(
        string organizationId,
        string code,
        string displayName)
        => Send(
            "/api/organizations/" + organizationId + "/children",
            "POST",
            new CreateOrganizationRequest(code, displayName));

    public static IPromise<ApiOutcome> CreateRole(
        string organizationId,
        string code,
        string displayName)
        => Send(
            "/api/organizations/" + organizationId + "/roles",
            "POST",
            new CreateOrganizationRoleRequest(code, displayName));

    public static IPromise<ApiOutcome> ReplaceRoleGrants(
        string organizationId,
        string roleId,
        string[] grantKeys)
    {
        var grants = new ResourceOperationSelection[grantKeys.Length];
        for (var index = 0; index < grantKeys.Length; index++)
        {
            var separator = grantKeys[index].IndexOf(':');
            grants[index] = new ResourceOperationSelection(
                grantKeys[index][..separator],
                grantKeys[index][(separator + 1)..]);
        }

        return Send(
            "/api/organizations/" + organizationId + "/roles/" + roleId + "/grants",
            "PUT",
            new UpdateRoleGrantsRequest(grants));
    }

    public static IPromise<ApiOutcome> CreateMember(
        string organizationId,
        string email,
        string[] roleIds)
        => Send(
            "/api/organizations/" + organizationId + "/members",
            "POST",
            new CreateOrganizationMemberRequest(email, roleIds));

    public static IPromise<ApiOutcome> ReplaceMemberRoles(
        string organizationId,
        string membershipId,
        string[] roleIds)
        => Send(
            "/api/organizations/" + organizationId + "/members/" + membershipId + "/roles",
            "PUT",
            new UpdateOrganizationMemberRolesRequest(roleIds));

    public static IPromise<ApiOutcome> GetAccounts()
        => Get("/api/accounts/");

    public static IPromise<ApiOutcome> CreateAccount(
        string email,
        string displayName,
        string password,
        bool platformAdministrator)
        => Send(
            "/api/accounts/",
            "POST",
            new CreateAccountRequest(email, displayName, password, platformAdministrator));

    public static IPromise<ApiOutcome> SetAccountEnabled(string userId, bool enabled)
        => Send("/api/accounts/" + userId + "/enabled", "PUT", new UpdateAccountStateRequest(enabled));

    public static IPromise<ApiOutcome> ResetAccountPassword(string userId, string password)
        => Send("/api/accounts/" + userId + "/password", "PUT", new ResetAccountPasswordRequest(password));

    public static IPromise<ApiOutcome> GetApps()
        => Get("/api/sso/applications");

    public static IPromise<ApiOutcome> CreateApp(AppCreate request)
        => Send("/api/sso/applications", "POST", request);

    public static IPromise<ApiOutcome> UpdateApp(string id, AppUpdate request)
        => Send("/api/sso/applications/" + id, "PUT", request);

    public static IPromise<ApiOutcome> DeleteApp(string id)
        => Send("/api/sso/applications/" + id, "DELETE");

    public static IPromise<ApiOutcome> RotateAppSecret(string id)
        => Send("/api/sso/applications/" + id + "/secret", "POST");

    public static IPromise<ApiOutcome> GetScopes()
        => Get("/api/sso/scopes");

    public static IPromise<ApiOutcome> CreateScope(ScopeCreate request)
        => Send("/api/sso/scopes", "POST", request);

    public static IPromise<ApiOutcome> UpdateScope(string id, ScopeUpdate request)
        => Send("/api/sso/scopes/" + id, "PUT", request);

    public static IPromise<ApiOutcome> DeleteScope(string id)
        => Send("/api/sso/scopes/" + id, "DELETE");

    public static IPromise<ApiOutcome> GetAuthorizations()
        => Get("/api/sso/authorizations");

    public static IPromise<ApiOutcome> RevokeAuthorization(string id)
        => Send("/api/sso/authorizations/" + id + "/revoke", "POST");

    public static IPromise<ApiOutcome> GetTokens()
        => Get("/api/sso/tokens");

    public static IPromise<ApiOutcome> RevokeToken(string id)
        => Send("/api/sso/tokens/" + id + "/revoke", "POST");

    public static IPromise<ApiOutcome> GetSettings()
        => Get("/api/settings/");

    public static IPromise<ApiOutcome> CreateSetting(SettingCreate request)
        => Send("/api/settings/", "POST", request);

    public static IPromise<ApiOutcome> UpdateSetting(string key, SettingUpdate request)
        => Send("/api/settings/" + key, "PUT", request);

    public static IPromise<ApiOutcome> DeleteSetting(string key)
        => Send("/api/settings/" + key, "DELETE");

    public static IPromise<ApiOutcome> GetSchedules()
        => Get("/api/schedules/");

    public static IPromise<ApiOutcome> UpdateSchedule(string key, ScheduleUpdate request)
        => Send("/api/schedules/" + key, "PUT", request);

    public static IPromise<ApiOutcome> TriggerSchedule(string key)
        => Send("/api/schedules/" + key + "/run", "POST");

    public static IPromise<ApiOutcome> GetScheduleRuns(string key)
        => Get("/api/schedules/" + key + "/runs");

    public static IPromise<ApiOutcome> GetNotifications()
        => Get("/api/notifications/");

    public static IPromise<ApiOutcome> GetOverview()
        => Get("/api/overview/");

    public static IPromise<ApiOutcome> GetAudit(
        string? from,
        string? to,
        string? actor,
        string? target,
        string? action)
    {
        var path = "/api/audit/";
        path = AppendQuery(path, "from", from);
        path = AppendQuery(path, "to", to);
        path = AppendQuery(path, "actor", actor);
        path = AppendQuery(path, "object", target);
        path = AppendQuery(path, "action", action);
        return Get(path);
    }

    public static SessionResponse ToSession(object data)
        => new(
            ReadString(data, "userId"),
            ReadString(data, "email"),
            ReadString(data, "displayName"),
            ReadStringArray(Reflect.Get(data, "roles")),
            ReadOrganizationSummaries(Reflect.Get(data, "organizations")));

    public static CaptchaChallengeResponse ToCaptchaChallenge(object data)
        => new(
            ReadString(data, "id"),
            ReadString(data, "imageUrl"));

    public static OrganizationDetailResponse ToOrganizationDetail(object data)
        => new(
            ReadString(data, "id"),
            ReadString(data, "code"),
            ReadString(data, "displayName"),
            ReadOptionalString(Reflect.Get(data, "parentId")),
            ReadOrganizationSummaries(Reflect.Get(data, "children")));

    public static OrganizationSummary[] ToOrganizationSummaries(object? data)
        => ReadOrganizationSummaries(data);

    public static OrganizationRoleResponse[] ToRoles(object? data)
    {
        var values = ReadArray(data);
        var roles = new OrganizationRoleResponse[values.Length];
        for (var index = 0; index < values.Length; index++)
        {
            var value = values[index]!;
            roles[index] = new OrganizationRoleResponse(
                ReadString(value, "id"),
                ReadString(value, "code"),
                ReadString(value, "displayName"));
        }

        return roles;
    }

    public static ResourceOperationResponse[] ToResourceOperations(object? data)
    {
        var values = ReadArray(data);
        var operations = new ResourceOperationResponse[values.Length];
        for (var index = 0; index < values.Length; index++)
        {
            var value = values[index]!;
            operations[index] = new ResourceOperationResponse(
                ReadString(value, "resource"),
                ReadString(value, "operation"),
                ReadString(value, "displayName"));
        }

        return operations;
    }

    public static string[] ToGrantKeys(object? data)
    {
        var values = ReadArray(data);
        var grants = new string[values.Length];
        for (var index = 0; index < values.Length; index++)
        {
            var value = values[index]!;
            grants[index] = ReadString(value, "resource") + ":" + ReadString(value, "operation");
        }

        return grants;
    }

    public static OrganizationMemberResponse[] ToMembers(object? data)
    {
        var values = ReadArray(data);
        var members = new OrganizationMemberResponse[values.Length];
        for (var index = 0; index < values.Length; index++)
        {
            var value = values[index]!;
            members[index] = new OrganizationMemberResponse(
                ReadString(value, "membershipId"),
                ReadString(value, "userId"),
                ReadString(value, "email"),
                ReadString(value, "displayName"),
                ToRoles(Reflect.Get(value, "roles")));
        }

        return members;
    }

    public static AccountResponse[] ToAccounts(object? data)
    {
        var values = ReadArray(data);
        var accounts = new AccountResponse[values.Length];
        for (var index = 0; index < values.Length; index++)
        {
            var value = values[index]!;
            accounts[index] = new AccountResponse(
                ReadString(value, "id"),
                ReadString(value, "email"),
                ReadString(value, "displayName"),
                BooleanValue(Reflect.Get(value, "enabled")),
                BooleanValue(Reflect.Get(value, "platformAdministrator")));
        }

        return accounts;
    }

    public static AppView[] ToApps(object? data)
    {
        var values = ReadArray(data);
        var applications = new AppView[values.Length];
        for (var index = 0; index < values.Length; index++)
            applications[index] = ReadApp(values[index]!);

        return applications;
    }

    public static AppSaved ToAppSaved(object data)
        => new(
            ReadApp(Reflect.Get(data, "app")!),
            ReadOptionalString(Reflect.Get(data, "secret")));

    public static string ReadSecret(object data)
        => ReadString(data, "secret");

    public static ScopeView[] ToScopes(object? data)
    {
        var values = ReadArray(data);
        var scopes = new ScopeView[values.Length];
        for (var index = 0; index < values.Length; index++)
        {
            var value = values[index]!;
            scopes[index] = new ScopeView(
                ReadString(value, "id"),
                ReadString(value, "name"),
                ReadString(value, "displayName"),
                ReadOptionalString(Reflect.Get(value, "description")),
                ReadStringArray(Reflect.Get(value, "resources")));
        }

        return scopes;
    }

    public static AuthorizationView[] ToAuthorizations(object? data)
    {
        var values = ReadArray(data);
        var authorizations = new AuthorizationView[values.Length];
        for (var index = 0; index < values.Length; index++)
        {
            var value = values[index]!;
            authorizations[index] = new AuthorizationView(
                ReadString(value, "id"),
                ReadOptionalString(Reflect.Get(value, "applicationId")),
                ReadOptionalString(Reflect.Get(value, "clientId")),
                ReadOptionalString(Reflect.Get(value, "subject")),
                ReadString(value, "status"),
                ReadString(value, "type"),
                ReadStringArray(Reflect.Get(value, "scopes")),
                ReadOptionalString(Reflect.Get(value, "createdAt")));
        }

        return authorizations;
    }

    public static TokenView[] ToTokens(object? data)
    {
        var values = ReadArray(data);
        var tokens = new TokenView[values.Length];
        for (var index = 0; index < values.Length; index++)
        {
            var value = values[index]!;
            tokens[index] = new TokenView(
                ReadString(value, "id"),
                ReadOptionalString(Reflect.Get(value, "applicationId")),
                ReadOptionalString(Reflect.Get(value, "clientId")),
                ReadOptionalString(Reflect.Get(value, "authorizationId")),
                ReadOptionalString(Reflect.Get(value, "subject")),
                ReadString(value, "status"),
                ReadString(value, "type"),
                ReadOptionalString(Reflect.Get(value, "createdAt")),
                ReadOptionalString(Reflect.Get(value, "expiresAt")),
                ReadOptionalString(Reflect.Get(value, "redeemedAt")));
        }

        return tokens;
    }

    public static SettingView[] ToSettings(object? data)
    {
        var values = ReadArray(data);
        var settings = new SettingView[values.Length];
        for (var index = 0; index < values.Length; index++)
        {
            var value = values[index]!;
            settings[index] = new SettingView(
                ReadString(value, "key"),
                ReadString(value, "group"),
                ReadString(value, "label"),
                ReadOptionalString(Reflect.Get(value, "description")),
                ReadString(value, "kind"),
                ReadString(value, "value"),
                ReadString(value, "updatedAt"));
        }

        return settings;
    }

    public static ScheduleView[] ToSchedules(object? data)
    {
        var values = ReadArray(data);
        var schedules = new ScheduleView[values.Length];
        for (var index = 0; index < values.Length; index++)
        {
            var value = values[index]!;
            schedules[index] = new ScheduleView(
                ReadString(value, "key"),
                ReadString(value, "name"),
                ReadString(value, "description"),
                ReadString(value, "cron"),
                BooleanValue(Reflect.Get(value, "enabled")),
                ReadOptionalString(Reflect.Get(value, "nextRunAt")),
                ReadOptionalString(Reflect.Get(value, "lastRunAt")),
                ReadOptionalString(Reflect.Get(value, "lastStatus")),
                ReadOptionalString(Reflect.Get(value, "lastMessage")));
        }

        return schedules;
    }

    public static ScheduleRunView[] ToScheduleRuns(object? data)
    {
        var values = ReadArray(data);
        var runs = new ScheduleRunView[values.Length];
        for (var index = 0; index < values.Length; index++)
        {
            var value = values[index]!;
            runs[index] = new ScheduleRunView(
                ReadString(value, "id"),
                ReadString(value, "trigger"),
                ReadString(value, "status"),
                ReadString(value, "startedAt"),
                ReadOptionalString(Reflect.Get(value, "finishedAt")),
                ReadOptionalString(Reflect.Get(value, "message")));
        }

        return runs;
    }

    public static AuditEventView[] ToAuditEvents(object? data)
    {
        var values = ReadArray(data);
        var events = new AuditEventView[values.Length];
        for (var index = 0; index < values.Length; index++)
        {
            var value = values[index]!;
            events[index] = new AuditEventView(
                ReadString(value, "id"),
                ReadString(value, "occurredAt"),
                ReadOptionalString(Reflect.Get(value, "actorId")),
                ReadOptionalString(Reflect.Get(value, "actorName")),
                ReadString(value, "action"),
                ReadString(value, "objectType"),
                ReadString(value, "objectId"),
                ReadOptionalString(Reflect.Get(value, "summary")));
        }

        return events;
    }

    public static NotificationView[] ToNotifications(object? data)
    {
        var values = ReadArray(data);
        var notifications = new NotificationView[values.Length];
        for (var index = 0; index < values.Length; index++)
        {
            var value = values[index]!;
            notifications[index] = new NotificationView(
                ReadString(value, "id"),
                ReadString(value, "source"),
                ReadString(value, "title"),
                ReadString(value, "status"),
                ReadString(value, "startedAt"),
                ReadOptionalString(Reflect.Get(value, "message")));
        }

        return notifications;
    }

    public static OverviewView ToOverview(object data)
    {
        var runValues = ReadArray(Reflect.Get(data, "recentRuns"));
        var recentRuns = new OverviewDailyRunView[runValues.Length];
        for (var index = 0; index < runValues.Length; index++)
        {
            var run = runValues[index]!;
            recentRuns[index] = new OverviewDailyRunView(
                ReadString(run, "date"),
                ReadInt(Reflect.Get(run, "succeeded")),
                ReadInt(Reflect.Get(run, "failed")));
        }

        var auditValues = ReadArray(Reflect.Get(data, "recentAudit"));
        var recentAudit = new OverviewDailyAuditView[auditValues.Length];
        for (var index = 0; index < auditValues.Length; index++)
        {
            var audit = auditValues[index]!;
            recentAudit[index] = new OverviewDailyAuditView(
                ReadString(audit, "date"),
                ReadInt(Reflect.Get(audit, "signIns")),
                ReadInt(Reflect.Get(audit, "tokenIssuances")));
        }

        var portalValues = ReadArray(Reflect.Get(data, "portalApplications"));
        var portalApplications = new PortalApplicationView[portalValues.Length];
        for (var index = 0; index < portalValues.Length; index++)
        {
            var application = portalValues[index]!;
            portalApplications[index] = new PortalApplicationView(
                ReadString(application, "clientId"),
                ReadString(application, "displayName"),
                ReadString(application, "launchUri"));
        }

        return new OverviewView(
            ReadInt(Reflect.Get(data, "accounts")),
            ReadInt(Reflect.Get(data, "enabledAccounts")),
            ReadInt(Reflect.Get(data, "organizations")),
            ReadInt(Reflect.Get(data, "organizationRoles")),
            ReadInt(Reflect.Get(data, "platformRoles")),
            ReadInt(Reflect.Get(data, "applications")),
            ReadInt(Reflect.Get(data, "scopes")),
            ReadInt(Reflect.Get(data, "authorizations")),
            ReadInt(Reflect.Get(data, "tokens")),
            ReadInt(Reflect.Get(data, "settings")),
            ReadInt(Reflect.Get(data, "schedules")),
            ReadInt(Reflect.Get(data, "enabledSchedules")),
            recentRuns,
            ReadInt(Reflect.Get(data, "auditEvents")),
            ReadInt(Reflect.Get(data, "tokenIssuances")),
            recentAudit,
            portalApplications);
    }

    private static IPromise<ApiOutcome> Get(string path)
        => Send(path, "GET");

    private static string AppendQuery(string path, string key, string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return path;

        return path + (path.Contains("?") ? "&" : "?") + key + "=" + EncodeURIComponent(value.Trim());
    }

    private static IPromise<ApiOutcome> Send(string path, string method, object? body = null)
    {
        var json = body is null ? null : JSON.Stringify(body);
        if (json is null)
        {
            // Optional WebIDL dictionary members must be absent, not null.
            var emptyOptions = new RequestInit(
                Method: method,
                Credentials: RequestCredentials.SameOrigin);
            return Promise<Response>.Resolve(ECMAScript.Global.Window.Fetch(path, emptyOptions)).Then(ReadResponse);
        }

        var contentOptions = new RequestInit(
            Method: method,
            Headers: (HeadersInit)new string[][] { new[] { "content-type", "application/json" } },
            Body: (BodyInit)(XMLHttpRequestBodyInit)json,
            Credentials: RequestCredentials.SameOrigin);
        return Promise<Response>.Resolve(ECMAScript.Global.Window.Fetch(path, contentOptions)).Then(ReadResponse);
    }

    private static IPromise<ApiOutcome> ReadResponse(Response response)
    {
        // Empty and authorization responses intentionally skip JSON parsing. ASP.NET Core commonly
        // returns no body for those statuses, while other errors can still expose ProblemDetails.
        // 204/401/403 响应不解析 JSON；其余错误保留 ProblemDetails 文本供页面呈现。
        if (response.Status is 202 or 204 or 401 or 403)
        {
            return Promise<ApiOutcome>.Resolve(new ApiOutcome
            {
                Ok = response.Ok,
                Error = response.Ok ? null : "Request failed with status " + response.Status + "."
            });
        }

        return Promise<object>.Resolve(response.Json()).Then(data => new ApiOutcome
        {
            Ok = response.Ok,
            Data = data,
            Error = response.Ok ? null : ReadError(data, response.Status)
        });
    }

    private static string ReadError(object data, ushort status)
    {
        var detail = ReadOptionalString(Reflect.Get(data, "detail"));
        if (detail is not null)
            return detail;

        var title = ReadOptionalString(Reflect.Get(data, "title"));
        if (title is not null)
            return title;

        var message = ReadOptionalString(Reflect.Get(data, "message"));
        return message ?? "Request failed with status " + status + ".";
    }

    private static OrganizationSummary[] ReadOrganizationSummaries(object? data)
    {
        var values = ReadArray(data);
        var organizations = new OrganizationSummary[values.Length];
        for (var index = 0; index < values.Length; index++)
        {
            var value = values[index]!;
            organizations[index] = new OrganizationSummary(
                ReadString(value, "id"),
                ReadString(value, "code"),
                ReadString(value, "displayName"));
        }

        return organizations;
    }

    private static AppView ReadApp(object value)
        => new(
            ReadString(value, "id"),
            ReadString(value, "clientId"),
            ReadString(value, "displayName"),
            ReadString(value, "profile"),
            ReadString(value, "applicationType"),
            ReadString(value, "clientType"),
            ReadString(value, "consentType"),
            BooleanValue(Reflect.Get(value, "requirePkce")),
            ReadStringArray(Reflect.Get(value, "redirectUris")),
            ReadStringArray(Reflect.Get(value, "postLogoutRedirectUris")),
            ReadStringArray(Reflect.Get(value, "endpoints")),
            ReadStringArray(Reflect.Get(value, "grantTypes")),
            ReadStringArray(Reflect.Get(value, "responseTypes")),
            ReadStringArray(Reflect.Get(value, "scopes")));

    private static object?[] ReadArray(object? value)
        => value as object?[] ?? [];

    private static string[] ReadStringArray(object? value)
    {
        var values = ReadArray(value);
        var result = new string[values.Length];
        for (var index = 0; index < values.Length; index++)
            result[index] = StringValue(values[index]);
        return result;
    }

    private static string ReadString(object value, string key)
        => StringValue(Reflect.Get(value, key));

    private static string? ReadOptionalString(object? value)
        => value is null ? null : StringValue(value);

    private static int ReadInt(object? value)
        => (int)NumberValue(value);
}
