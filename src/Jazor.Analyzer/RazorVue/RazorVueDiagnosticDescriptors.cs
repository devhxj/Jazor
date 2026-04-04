using Microsoft.CodeAnalysis;

namespace Jazor.Analyzer;

internal static class RazorVueDiagnosticDescriptors
{
    public static readonly DiagnosticDescriptor InvalidEntryInheritance = new(
        id: "JAZORVUE001",
        title: "Invalid RazorVue component inheritance",
        messageFormat: "RazorVue entry '{0}' must inherit JazorComponent",
        category: "RazorVue",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor DirectComponentBaseEntry = new(
        id: "JAZORVUE002",
        title: "Direct ComponentBase entry is not allowed",
        messageFormat: "RazorVue entry '{0}' must inherit JazorComponent instead of ComponentBase directly",
        category: "RazorVue",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor StateHasChangedNotSupported = new(
        id: "JAZORVUE004",
        title: "StateHasChanged is not part of RazorVue semantics",
        messageFormat: "StateHasChanged is not part of RazorVue semantics",
        category: "RazorVue",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor ShouldRenderNotSupported = new(
        id: "JAZORVUE005",
        title: "ShouldRender is not part of RazorVue semantics",
        messageFormat: "ShouldRender is not part of RazorVue semantics",
        category: "RazorVue",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor SetParametersAsyncNotSupported = new(
        id: "JAZORVUE006",
        title: "SetParametersAsync is not part of RazorVue semantics",
        messageFormat: "SetParametersAsync is not part of RazorVue semantics",
        category: "RazorVue",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);
}
