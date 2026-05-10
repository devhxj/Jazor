using Microsoft.CodeAnalysis;

namespace Jazor.Analyzer.RazorVue.Generation;

internal static class RazorSourceGeneratorDiagnostics
{
    internal static readonly DiagnosticDescriptor RazorSgTailOutputFailed = new(
        id: "JAZORVGA020",
        title: "RazorVue Razor SG tail output failed",
        messageFormat: "RazorVue Razor SG tail output failed: {0}",
        category: "Jazor.RazorVue.Analysis",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);
}
