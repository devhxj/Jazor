using Microsoft.CodeAnalysis;

namespace Jazor.RazorVue.Generation;

/// <summary>Owns diagnostics emitted by the final-compilation generation boundary.</summary>
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
