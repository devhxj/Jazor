using System.Security.Claims;
using System.Text.Json.Serialization;

namespace Jazor.AspNetCore;

/// <summary>Describes one application-level Vue provider carried into SSR and hydration.</summary>
/// <param name="Key">Vue provide 的字符串键，必须非空且在同一请求中唯一（区分大小写）。</param>
/// <param name="Value">可由 System.Text.Json 序列化的 provider 值；将发送给浏览器。</param>
public sealed record JazorSsrProvider(
    [property: JsonPropertyName("key")] string Key,
    [property: JsonPropertyName("value")] object? Value);

/// <summary>Closed, host-owned authentication snapshot for one SSR request.</summary>
/// <param name="Status">宿主判定的认证状态。</param>
/// <param name="Subject">主体标识；匿名或无标识时可为 null。</param>
/// <param name="Claims">按 claim 类型分组的全部值；null 表示未提供。该快照会发送给浏览器。</param>
public sealed record JazorAuthenticationState(
    [property: JsonPropertyName("status")] JazorAuthenticationStatus Status,
    [property: JsonPropertyName("subject")] string? Subject = null,
    [property: JsonPropertyName("claims")] IReadOnlyDictionary<string, string[]>? Claims = null)
{
    /// <summary>Vue authentication provider 的保留键 jazor:auth-state。</summary>
    public const string ProviderKey = "jazor:auth-state";
    /// <summary>认证快照协议名 jazor-auth-state。</summary>
    public const string EnvelopeSchema = "jazor-auth-state";
    /// <summary>当前认证快照协议版本 1。</summary>
    public const int EnvelopeVersion = 1;

    /// <summary>从当前 principal 创建快照；未认证时返回 Anonymous，否则复制全部 claims，并优先用 NameIdentifier 作为 Subject。</summary>
    /// <param name="principal">当前请求的 ClaimsPrincipal；生成的全部 claims 将成为可序列化快照。</param>
    /// <returns>包含当前协议数据的快照。</returns>
    public static JazorAuthenticationState FromPrincipal(ClaimsPrincipal principal)
    {
        ArgumentNullException.ThrowIfNull(principal);
        var identity = principal.Identity;
        if (identity?.IsAuthenticated != true)
            return new(JazorAuthenticationStatus.Anonymous);

        var claims = principal.Claims
            .GroupBy(static claim => claim.Type, StringComparer.Ordinal)
            .ToDictionary(
                static group => group.Key,
                static group => group.Select(static claim => claim.Value).ToArray(),
                StringComparer.Ordinal);
        return new(
            JazorAuthenticationStatus.Authenticated,
            principal.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? identity.Name,
            claims);
    }
}

/// <summary>Versioned application endpoint result consumed by the browser authentication provider.</summary>
/// <param name="Schema">协议名称；建议使用 Create 工厂填入当前值。</param>
/// <param name="Version">协议版本；建议使用 Create 工厂填入当前值。</param>
/// <param name="State">当前认证快照。</param>
public sealed record JazorAuthenticationEnvelope(
    [property: JsonPropertyName("schema")] string Schema,
    [property: JsonPropertyName("version")] int Version,
    [property: JsonPropertyName("state")] JazorAuthenticationState State)
{
    /// <summary>用当前 schema/version 包装认证快照。</summary>
    /// <param name="state">需要交给浏览器的认证快照。</param>
    /// <returns>包含当前协议数据的快照。</returns>
    public static JazorAuthenticationEnvelope Create(JazorAuthenticationState state)
    {
        ArgumentNullException.ThrowIfNull(state);
        return new(JazorAuthenticationState.EnvelopeSchema, JazorAuthenticationState.EnvelopeVersion, state);
    }
}

/// <summary>宿主提供给浏览器的认证状态，以字符串序列化；不自动执行服务端认证或授权。</summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum JazorAuthenticationStatus
{
    /// <summary>当前请求没有已认证身份。</summary>
    Anonymous,
    /// <summary>宿主确认当前身份已认证。</summary>
    Authenticated,
    /// <summary>宿主声明身份已过期，需要重新认证。</summary>
    Expired,
    /// <summary>宿主声明当前身份不能访问目标资源。</summary>
    Forbidden
}

/// <summary>Describes one server-rendered Vue root component and its serialized props.</summary>
/// <param name="ModulePath">相对产物根目录的组件 .js 路径，不接受外部 URL 或越出根目录的路径。</param>
/// <param name="Props">序列化到 SSR 和浏览器的 props，null 表示未提供；默认遵循 System.Text.Json 属性命名。</param>
/// <param name="Providers">Vue provide 快照；键必须非空且唯一。请求可用 null 表示无 provider。</param>
/// <param name="Authentication">可选认证快照；存在时自动提供 jazor:auth-state，不能在 Providers 重复占用该键。</param>
public sealed record JazorSsrRequest(
    string ModulePath,
    object? Props = null,
    IReadOnlyList<JazorSsrProvider>? Providers = null,
    JazorAuthenticationState? Authentication = null);

/// <summary>
/// Versioned state handoff shared by the SSR runner and browser hydration entry.
/// Props/providers remain strongly typed request inputs; this record is the transport envelope.
/// </summary>
/// <param name="Schema">协议名称；建议使用 Create 工厂填入当前值。</param>
/// <param name="Version">协议版本；建议使用 Create 工厂填入当前值。</param>
/// <param name="Props">序列化到 SSR 和浏览器的 props，null 表示未提供；默认遵循 System.Text.Json 属性命名。</param>
/// <param name="Providers">Vue provide 快照；键必须非空且唯一。请求可用 null 表示无 provider。</param>
/// <param name="Authentication">可选认证快照；存在时自动提供 jazor:auth-state，不能在 Providers 重复占用该键。</param>
public sealed record JazorSsrStateEnvelope(
    [property: JsonPropertyName("schema")] string Schema,
    [property: JsonPropertyName("version")] int Version,
    [property: JsonPropertyName("props")] object? Props,
    [property: JsonPropertyName("providers")] IReadOnlyList<JazorSsrProvider> Providers,
    [property: JsonPropertyName("authentication")] JazorAuthenticationState? Authentication)
{
    /// <summary>SSR hydration 状态协议名 jazor-ssr-state。</summary>
    public const string CurrentSchema = "jazor-ssr-state";
    /// <summary>当前 SSR hydration 协议版本 1。</summary>
    public const int CurrentVersion = 1;

    /// <summary>创建当前版本的 hydration 快照，并将 Authentication 加入保留的 Vue provider。</summary>
    /// <param name="request">根模块、props 和 provider 快照；HTTP 固定请求重载会跨请求复用此对象。</param>
    /// <returns>包含当前协议数据的快照。</returns>
    /// <exception cref="ArgumentException">Provider 键为空、重复，或与自动认证 provider 冲突。</exception>
    public static JazorSsrStateEnvelope Create(JazorSsrRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        var providers = (request.Providers ?? []).ToList();
        if (providers.Any(static provider => provider is null || string.IsNullOrWhiteSpace(provider.Key)))
            throw new ArgumentException("Jazor SSR providers must have non-empty keys.", nameof(request));

        if (providers
            .GroupBy(static provider => provider.Key, StringComparer.Ordinal)
            .Any(static group => group.Skip(1).Any()))
            throw new ArgumentException("Jazor SSR providers must have unique keys.", nameof(request));

        if (request.Authentication is not null &&
            providers.Any(static provider => string.Equals(provider.Key, JazorAuthenticationState.ProviderKey, StringComparison.Ordinal)))
            throw new ArgumentException(
                "Jazor SSR authentication provider key is reserved and cannot be supplied twice.",
                nameof(request));

        if (request.Authentication is not null)
            providers.Add(new JazorSsrProvider(JazorAuthenticationState.ProviderKey, request.Authentication));

        return new JazorSsrStateEnvelope(
            CurrentSchema,
            CurrentVersion,
            request.Props,
            providers,
            request.Authentication);
    }
}

/// <summary>Contains the HTML and serialized state produced by one SSR application instance.</summary>
/// <param name="ModulePath">相对产物根目录的组件 .js 路径，不接受外部 URL 或越出根目录的路径。</param>
/// <param name="Html">Vue 根组件 HTML 片段，不是完整 HTTP 文档。</param>
/// <param name="SerializedProps">渲染使用的 props JSON。</param>
/// <param name="SerializedProviders">渲染使用的 provider 数组 JSON，默认 []。</param>
/// <param name="SerializedState">用于 hydration 的版本化状态 JSON；完整渲染结果由 renderer 提供。</param>
public sealed record JazorSsrRenderResult(
    string ModulePath,
    string Html,
    string SerializedProps,
    string SerializedProviders = "[]",
    string SerializedState = "{}");
