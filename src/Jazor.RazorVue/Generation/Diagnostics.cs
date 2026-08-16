using Microsoft.CodeAnalysis;

namespace Jazor.RazorVue.Generation;

/// <summary>
/// Owns diagnostics emitted by the final-compilation generation boundary.
/// 只报告 RazorVue 无法消费完成编译的失败，Razor/C# 原始诊断仍由 SDK 和 Roslyn 负责。
/// </summary>
internal static class Diagnostics
{
    internal static readonly DiagnosticDescriptor TailOutputFailed = new(
        id: "JAZORVGA020",
        title: "RazorVue final Compilation output failed",
        messageFormat: "RazorVue final Compilation output failed: {0}",
        category: "Jazor.RazorVue",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        helpLinkUri: HelpLink("final-compilation"));

    internal static readonly DiagnosticDescriptor DirectRenderUnsupported = Create(
        "JAZORVGA021",
        "RazorVue direct render shape is not supported",
        "RazorVue direct render shape is not supported: {0}",
        "direct-render");

    internal static readonly DiagnosticDescriptor CompilerBridgeUnsupported = Create(
        "JAZORVGA022",
        "RazorVue compiler bridge cannot lower this expression",
        "RazorVue compiler bridge cannot lower this expression: {0}",
        "compiler-boundary");

    internal static readonly DiagnosticDescriptor ComponentBindingFailed = Create(
        "JAZORVGA023",
        "RazorVue component render binding failed",
        "RazorVue component render binding failed: {0}",
        "component-binding");

    internal static readonly DiagnosticDescriptor MemberClosureFailed = Create(
        "JAZORVGA024",
        "RazorVue component member closure failed",
        "RazorVue component member closure failed: {0}",
        "member-closure");

    internal static readonly DiagnosticDescriptor VueInjectDeclarationInvalid = Create(
        "JAZORVGA025",
        "RazorVue VueInject declaration is invalid",
        "RazorVue VueInject declaration is invalid: {0}",
        "vue-inject");

    internal static readonly DiagnosticDescriptor VueModuleFailed = Create(
        "JAZORVGA026",
        "RazorVue Vue module generation failed",
        "RazorVue Vue module generation failed: {0}",
        "vue-module");

    internal static Diagnostic Create(RazorVueDiagnosticInfo info)
        => Diagnostic.Create(
            GetDescriptor(info.Category),
            info.PrimaryLocation,
            info.AdditionalLocations,
            properties: null,
            info.Message);

    internal static DiagnosticDescriptor GetDescriptor(RazorVueDiagnosticCategory category)
        => category switch
        {
            RazorVueDiagnosticCategory.DirectRender => DirectRenderUnsupported,
            RazorVueDiagnosticCategory.CompilerBridge => CompilerBridgeUnsupported,
            RazorVueDiagnosticCategory.ComponentBinding => ComponentBindingFailed,
            RazorVueDiagnosticCategory.MemberClosure => MemberClosureFailed,
            RazorVueDiagnosticCategory.VueInject => VueInjectDeclarationInvalid,
            RazorVueDiagnosticCategory.VueModule => VueModuleFailed,
            _ => TailOutputFailed
        };

    private static DiagnosticDescriptor Create(
        string id,
        string title,
        string messageFormat,
        string helpLinkKey)
        => new(
            id,
            title,
            messageFormat,
            "Jazor.RazorVue",
            DiagnosticSeverity.Error,
            isEnabledByDefault: true,
            helpLinkUri: HelpLink(helpLinkKey));

    private static string HelpLink(string anchor)
        => "https://github.com/devhxj/Jazor/blob/main/docs/03-guides/razorvue-authoring.md#" + anchor;
}
