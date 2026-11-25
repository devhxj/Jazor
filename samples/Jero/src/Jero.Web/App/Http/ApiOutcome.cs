namespace Jero.Web.App.Http;

/// <summary>
/// 客户端请求结果。<see cref="Data"/> 对应服务端响应的 result 字段。
/// </summary>
public sealed record ApiOutcome
{
    public bool Ok { get; init; }

    /// <summary>
    /// Backend <c>code</c>, which uses HTTP status semantics (200/400/401/403/500).
    /// </summary>
    public int Code { get; init; }

    public string? Error { get; init; }

    public object? Data { get; init; }
}
