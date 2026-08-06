using ECMAScript;
using JazorAdmin.Features.Accounts;
using JazorAdmin.Features.Configuration;
using JazorAdmin.Features.Identity;
using JazorAdmin.Features.Organizations;
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

    public static IPromise<ApiOutcome> SignIn(string email, string password)
        => Send("/api/auth/login", "POST", new LoginRequest(email, password));

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
        => Get("/api/configuration/applications");

    public static IPromise<ApiOutcome> CreateApp(AppCreate request)
        => Send("/api/configuration/applications", "POST", request);

    public static IPromise<ApiOutcome> UpdateApp(string id, AppUpdate request)
        => Send("/api/configuration/applications/" + id, "PUT", request);

    public static IPromise<ApiOutcome> DeleteApp(string id)
        => Send("/api/configuration/applications/" + id, "DELETE");

    public static IPromise<ApiOutcome> RotateAppSecret(string id)
        => Send("/api/configuration/applications/" + id + "/secret", "POST");

    public static IPromise<ApiOutcome> GetScopes()
        => Get("/api/configuration/scopes");

    public static IPromise<ApiOutcome> CreateScope(ScopeCreate request)
        => Send("/api/configuration/scopes", "POST", request);

    public static IPromise<ApiOutcome> UpdateScope(string id, ScopeUpdate request)
        => Send("/api/configuration/scopes/" + id, "PUT", request);

    public static IPromise<ApiOutcome> DeleteScope(string id)
        => Send("/api/configuration/scopes/" + id, "DELETE");

    public static IPromise<ApiOutcome> GetAuthorizations()
        => Get("/api/configuration/authorizations");

    public static IPromise<ApiOutcome> RevokeAuthorization(string id)
        => Send("/api/configuration/authorizations/" + id + "/revoke", "POST");

    public static IPromise<ApiOutcome> GetTokens()
        => Get("/api/configuration/tokens");

    public static IPromise<ApiOutcome> RevokeToken(string id)
        => Send("/api/configuration/tokens/" + id + "/revoke", "POST");

    public static SessionResponse ToSession(object data)
        => new(
            ReadString(data, "userId"),
            ReadString(data, "email"),
            ReadString(data, "displayName"),
            ReadStringArray(Reflect.Get(data, "roles")),
            ReadOrganizationSummaries(Reflect.Get(data, "organizations")));

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
                BooleanFn(Reflect.Get(value, "enabled")),
                BooleanFn(Reflect.Get(value, "platformAdministrator")));
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

    private static IPromise<ApiOutcome> Get(string path)
        => Send(path, "GET");

    private static IPromise<ApiOutcome> Send(string path, string method, object? body = null)
    {
        var json = body is null ? null : JSON.Stringify(body);
        HeadersInit? headers = json is null
            ? (HeadersInit?)null
            : (HeadersInit)new string[][] { new[] { "content-type", "application/json" } };
        BodyInit? requestBody = json is null
            ? (BodyInit?)null
            : (BodyInit)(XMLHttpRequestBodyInit)json;
        var options = new RequestInit(
            Method: method,
            Headers: headers,
            Body: requestBody,
            Credentials: RequestCredentials.SameOrigin);

        return Promise<Response>.Resolve(ECMAScript.Global.Window.Fetch(path, options)).Then(ReadResponse);
    }

    private static IPromise<ApiOutcome> ReadResponse(Response response)
    {
        // Empty and authorization responses intentionally skip JSON parsing. ASP.NET Core commonly
        // returns no body for those statuses, while other errors can still expose ProblemDetails.
        // 204/401/403 响应不解析 JSON；其余错误保留 ProblemDetails 文本供页面呈现。
        if (response.Status is 204 or 401 or 403)
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
            BooleanFn(Reflect.Get(value, "requirePkce")),
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
            result[index] = StringFn(values[index]);
        return result;
    }

    private static string ReadString(object value, string key)
        => StringFn(Reflect.Get(value, key));

    private static string? ReadOptionalString(object? value)
        => value is null ? null : StringFn(value);
}
