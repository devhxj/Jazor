namespace Jazor.RazorVue.RazorSdk;

/// <summary>
/// Compiler-owned HMR boundary values carried through the RazorVue catalog.
/// The host consumes the wire value and never reclassifies Razor semantics from emitted JS.
/// </summary>
internal enum VueHmrBoundaryKind
{
    Unknown,
    TemplateOnly,
    LogicSafe,
    FullReloadRequired
}

internal static class VueHmrBoundaryKindExtensions
{
    public static string ToWireValue(this VueHmrBoundaryKind value)
        => value switch
        {
            VueHmrBoundaryKind.TemplateOnly => "template-only",
            VueHmrBoundaryKind.LogicSafe => "logic-safe",
            VueHmrBoundaryKind.FullReloadRequired => "full-reload-required",
            _ => "unknown"
        };
}

/// <summary>Stable, compiler-owned identity and change partitions for one Vue render module.</summary>
internal sealed record VueHmrMetadata(
    string ModuleId,
    string DescriptorHash,
    string TemplateHash,
    string LogicHash,
    VueHmrBoundaryKind BoundaryKind);
