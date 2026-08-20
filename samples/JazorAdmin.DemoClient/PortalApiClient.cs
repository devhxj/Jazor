using ECMAScript;
using static ECMAScript.Global;

namespace JazorAdmin.DemoClient;

[ECMAScriptModule("components/portal-api-client.mjs")]
public static class PortalApiClient
{
    public static IPromise<PortalApiOutcome> GetSession()
        => Get("/api/session");

    public static IPromise<PortalApiOutcome> GetOverview()
        => Get("/api/platform/overview");

    public static DemoSessionView ToSession(object data)
    {
        var roles = ReadArray(Reflect.Get(data, "roles"));
        var values = new string[roles.Length];
        for (var index = 0; index < roles.Length; index++)
            values[index] = StringFn(roles[index]);

        return new DemoSessionView(
            StringFn(Reflect.Get(data, "subject")),
            OptionalString(Reflect.Get(data, "name")),
            OptionalString(Reflect.Get(data, "email")),
            values,
            BooleanFn(Reflect.Get(data, "hasAccessToken")));
    }

    public static ProtectedOverviewView ToOverview(object data)
        => new(
            (int)NumberFn(Reflect.Get(data, "accounts")),
            (int)NumberFn(Reflect.Get(data, "applications")),
            (int)NumberFn(Reflect.Get(data, "tokens")),
            (int)NumberFn(Reflect.Get(data, "auditEvents")),
            (int)NumberFn(Reflect.Get(data, "tokenIssuances")));

    private static IPromise<PortalApiOutcome> Get(string path)
    {
        var options = new RequestInit(
            Method: "GET",
            Credentials: RequestCredentials.SameOrigin);
        return Promise<Response>.Resolve(ECMAScript.Global.Window.Fetch(path, options)).Then(ReadResponse);
    }

    private static IPromise<PortalApiOutcome> ReadResponse(Response response)
    {
        if (response.Status is 401 or 403 or 204)
        {
            return Promise<PortalApiOutcome>.Resolve(new PortalApiOutcome
            {
                Ok = response.Ok,
                Unauthorized = response.Status == 401,
                Error = response.Ok ? null : "Request failed with status " + response.Status + "."
            });
        }

        return Promise<object>.Resolve(response.Json()).Then(data => new PortalApiOutcome
        {
            Ok = response.Ok,
            Data = data,
            Error = response.Ok ? null : "Request failed with status " + response.Status + "."
        });
    }

    private static object?[] ReadArray(object? value)
        => value as object?[] ?? [];

    private static string? OptionalString(object? value)
        => value is null ? null : StringFn(value);
}
