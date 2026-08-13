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
        isEnabledByDefault: true);
}
