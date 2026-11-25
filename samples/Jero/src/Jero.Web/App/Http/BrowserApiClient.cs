using static ECMAScript.Global;
using Jero.Web.App.Auth;

namespace Jero.Web.App.Http;

/// <summary>
/// 发送同源 API 请求，附加当前会话令牌，并把服务端 AdminResult 解包为页面使用的 ApiOutcome。
/// </summary>
[ECMAScriptModule("components/api-client.js")]
public static class BrowserApiClient
{
    /// <summary>
    /// Bearer token provided by the application after a successful sign-in.
    /// The browser session store persists it across full page reloads.
    /// </summary>
    public static string? AuthToken
    {
        get => BrowserSessionStore.AccessToken;
        set => BrowserSessionStore.AccessToken = value;
    }

    /// <summary>
    /// Refresh token used by <see cref="Refresh"/>.
    /// </summary>
    public static string? RefreshToken
    {
        get => BrowserSessionStore.RefreshToken;
        set => BrowserSessionStore.RefreshToken = value;
    }

    public static IPromise<ApiOutcome> Get(string path) => Send(path, "GET");

    public static IPromise<ApiOutcome> Post(string path, object? body = null) => Send(path, "POST", body);

    public static IPromise<ApiOutcome> Put(string path, object? body = null) => Send(path, "PUT", body);

    public static IPromise<ApiOutcome> Delete(string path) => Send(path, "DELETE");

    /// <summary>
    /// Exchanges the current tokens for a new access token.
    ///
    /// The refresh endpoint returns both tokens in its JSON result. Persist the pair together.
    /// </summary>
    public static IPromise<ApiOutcome> Refresh()
    {
        var headers = new string[][]
        {
            new[] { "authorization", "Bearer " + AuthToken },
            new[] { "x-authorization", "Bearer " + RefreshToken }
        };

        var init = new RequestInit(
            Method: "GET",
            Headers: (HeadersInit)headers,
            Credentials: RequestCredentials.SameOrigin);

        return SendRequest("/api/sysAuth/refreshToken?accessToken=" + EncodeURIComponent(AuthToken ?? ""), init)
            .Then(outcome =>
            {
                if (!outcome.Ok || outcome.Data is null)
                    return outcome;

                BrowserSessionStore.SetTokens(
                    ReadOptionalString(Reflect.Get(outcome.Data, "accessToken")),
                    ReadOptionalString(Reflect.Get(outcome.Data, "refreshToken")));
                return outcome;
            });
    }

    private static IPromise<ApiOutcome> Send(string path, string method, object? body = null)
    {
        var json = body is null ? null : JSON.Stringify(body);
        var token = AuthToken;
        var hasToken = !string.IsNullOrEmpty(token);

        // Optional WebIDL dictionary members must be absent, not null.
        if (json is null && !hasToken)
            return SendRequest(path, new RequestInit(Method: method, Credentials: RequestCredentials.SameOrigin));

        if (json is null)
            return SendRequest(path, new RequestInit(
                Method: method,
                Headers: AuthHeaders(token!),
                Credentials: RequestCredentials.SameOrigin));

        if (!hasToken)
            return SendRequest(path, new RequestInit(
                Method: method,
                Headers: JsonHeaders(),
                Body: Body(json),
                Credentials: RequestCredentials.SameOrigin));

        return SendRequest(path, new RequestInit(
            Method: method,
            Headers: JsonAndAuthHeaders(token!),
            Body: Body(json),
            Credentials: RequestCredentials.SameOrigin));
    }

    private static IPromise<ApiOutcome> SendRequest(string path, RequestInit init)
        => Promise<Response>.Resolve(ECMAScript.Global.Window.Fetch(path, init)).Then(ReadResponse);

    private static HeadersInit AuthHeaders(string token)
        => (HeadersInit)new string[][] { new[] { "authorization", "Bearer " + token } };

    private static HeadersInit JsonHeaders()
        => (HeadersInit)new string[][] { new[] { "content-type", "application/json" } };

    private static HeadersInit JsonAndAuthHeaders(string token)
        => (HeadersInit)new string[][]
        {
            new[] { "content-type", "application/json" },
            new[] { "authorization", "Bearer " + token }
        };

    private static BodyInit Body(string json) => (BodyInit)(XMLHttpRequestBodyInit)json;

    private static IPromise<ApiOutcome> ReadResponse(Response response)
    {
        // Jero's JWT handler can renew either token through response headers.
        var renewedToken = response.Headers.Get("access-token");
        var renewedRefreshToken = response.Headers.Get("x-access-token");

        // The backend uses this sentinel on logout and invalid sessions. Match the legacy
        // client's semantics: never persist the sentinel as if it were a usable JWT.
        if (renewedToken == "invalid_token")
        {
            BrowserSessionStore.Clear();
        }
        else
        {
            if (!string.IsNullOrEmpty(renewedToken))
                AuthToken = renewedToken;
            if (!string.IsNullOrEmpty(renewedRefreshToken))
                RefreshToken = renewedRefreshToken;
        }

        if (response.Status == 401)
            BrowserSessionStore.Clear();

        // 204 is the only Jero response without a body; every other status ships the envelope.
        if (response.Status == 204)
            return Promise<ApiOutcome>.Resolve(new ApiOutcome
            {
                Ok = response.Ok,
                Code = response.Status,
                Error = response.Ok ? null : "请求失败，状态码 " + response.Status + "。"
            });

        return Promise<object>.Resolve(response.Json()).Then(envelope =>
        {
            var code = (int)NumberValue(Reflect.Get(envelope, "code"));
            var ok = response.Ok && code == 200;

            return new ApiOutcome
            {
                Ok = ok,
                Code = code,
                Data = ok ? Reflect.Get(envelope, "result") : null,
                Error = ok ? null : ReadError(envelope, code)
            };
        });
    }

    private static string ReadError(object envelope, int code)
    {
        var message = ReadOptionalString(Reflect.Get(envelope, "message"));
        return string.IsNullOrWhiteSpace(message)
            ? "请求失败，状态码 " + code + "。"
            : message!;
    }

    private static string? ReadOptionalString(object? value) => value is null ? null : StringValue(value);
}
