namespace JazorAdmin;

/// <summary>
/// Carries transport status around shared API DTOs consumed by RazorVue pages.
/// 仅补充传输状态，不复制业务数据模型。
/// </summary>
public sealed record ApiOutcome
{
    public bool Ok { get; init; }

    public string? Error { get; init; }

    public object? Data { get; init; }
}
