using Microsoft.CodeAnalysis;

namespace Jazor.Vue.Analysis;

internal static class JazorVueDiagnosticDescriptors
{
    public static readonly DiagnosticDescriptor NamespaceImportInvokedAsFunction = new(
        id: "JAZORJV001",
        title: "Namespace import cannot be invoked",
        messageFormat: "JSImport namespace '{0}' cannot be invoked as a function",
        category: "Jazor.Vue",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor ComponentImportInvokedAsFunction = new(
        id: "JAZORJV002",
        title: "Vue component import cannot be invoked",
        messageFormat: "Vue component import '{0}' cannot be invoked as a function",
        category: "Jazor.Vue",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);
}
