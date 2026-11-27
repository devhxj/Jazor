namespace Jero.Web.Infrastructure.Http;

/// <summary>
/// 客户端请求结果。<see cref="Data"/> 对应服务端响应的 result 字段。
/// </summary>
public sealed record ApiOutcome
{
    /// <summary>请求是否成功。有响应体时要求 HTTP 成功且业务码为 200。</summary>
    public bool Ok { get; init; }

    /// <summary>
    /// 服务端响应体中的 <c>code</c>。无正文的 204 响应使用 HTTP 状态码。
    /// </summary>
    public int Code { get; init; }

    /// <summary>请求失败时显示的错误说明。</summary>
    public string? Error { get; init; }

    public object? Data { get; init; }
}
