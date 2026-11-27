using Jero.Web.Infrastructure.Storage;
using static ECMAScript.Global;

namespace Jero.Web.Infrastructure.Http;

/// <summary>
/// 发送同源 API 请求，附加当前会话令牌，并把服务端 AdminResult 解包为页面使用的 ApiOutcome。
/// </summary>
[ECMAScriptModule("components/api-client.js")]
public static class BrowserApiClient
{
    /// <summary>
    /// 同源请求使用的 Bearer 访问令牌，由浏览器会话存储跨页面重载保存。
    /// </summary>
    public static string? AuthToken
    {
        get => BrowserSessionStore.AccessToken;
        set => BrowserSessionStore.AccessToken = value;
    }

    /// <summary>
    /// 显式续期请求放在 <c>x-authorization</c> 请求头中的令牌。
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
    /// 使用当前令牌调用续期接口。仅当调用成功且响应包含业务数据时，才从响应体读取两项令牌并写回存储。
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

        // WebIDL 可选成员没有值时需省略，保留无请求体、无请求头等不同调用形式。
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
        // 先处理响应头中的续期令牌。普通响应只覆盖头中实际提供的令牌，401 则清除会话。
        var renewedToken = response.Headers.Get("access-token");
        var renewedRefreshToken = response.Headers.Get("x-access-token");

        // 服务端用 invalid_token 通知客户端退出或会话失效，收到时清除本地令牌。
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

        // 204 响应没有正文，直接构造结果，避免调用 JSON 解析。
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
