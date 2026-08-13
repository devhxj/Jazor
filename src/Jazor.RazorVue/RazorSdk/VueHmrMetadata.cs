namespace Jazor.RazorVue.RazorSdk;

/// <summary>Classifies whether a module update can preserve Vue instance state. 描述热更新边界。</summary>
internal enum VueHmrBoundaryKind
{
    Unknown,
    TemplateOnly,
    LogicSafe,
    FullReloadRequired
}

/// <summary>Serializes compiler classifications to the stable development-client wire protocol.</summary>
internal static class VueHmrBoundaryKindExtensions
{
    public static string ToWireValue(this VueHmrBoundaryKind value)
        // Keep these values protocol-stable: the dev client must not infer Razor semantics
        // from framed JavaScript. wire 值是 compiler 与 dev client 的固定边界。
        => value switch
        {
            VueHmrBoundaryKind.TemplateOnly => "template-only",
            VueHmrBoundaryKind.LogicSafe => "logic-safe",
            VueHmrBoundaryKind.FullReloadRequired => "full-reload-required",
            _ => "unknown"
        };
}

/// <summary>
/// Stable, compiler-owned identity and change partitions for one Vue render module.
/// Descriptor/template/logic hash 分开保存，让 dev client 按已分析的边界决定保留状态或完整刷新。
/// </summary>
internal sealed record VueHmrMetadata(
    string ModuleId,
    string DescriptorHash,
    string TemplateHash,
    string LogicHash,
    VueHmrBoundaryKind BoundaryKind);
