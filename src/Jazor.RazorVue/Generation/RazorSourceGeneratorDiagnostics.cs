using Microsoft.CodeAnalysis;

namespace Jazor.RazorVue.Generation;

internal static class RazorSourceGeneratorDiagnostics
{
    internal static readonly DiagnosticDescriptor RazorSgTailOutputFailed = new(
        id: "JAZORVGA020",
        title: "RazorVue final Compilation output failed",
        messageFormat: "RazorVue final Compilation output failed: {0}",
        category: "Jazor.RazorVue",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);
}
