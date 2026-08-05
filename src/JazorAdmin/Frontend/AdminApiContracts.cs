// Carries transport status around the shared API DTOs consumed by RazorVue pages.
// 在 RazorVue 页面消费共享 API DTO 时，仅补充传输状态，不复制业务数据模型。
namespace JazorAdmin.Frontend;

public sealed record AdminApiOutcome
{
    public bool Ok { get; init; }

    public string? Error { get; init; }

    public object? Data { get; init; }
}
