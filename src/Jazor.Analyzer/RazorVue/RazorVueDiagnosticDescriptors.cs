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

    public static readonly DiagnosticDescriptor UnknownParameter = new(
        id: "JAZORVUE007",
        title: "RazorVue parameter is unknown",
        messageFormat: "{0}",
        category: "RazorVue",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor InvalidBindTarget = new(
        id: "JAZORVUE008",
        title: "RazorVue bind target is invalid",
        messageFormat: "{0}",
        category: "RazorVue",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor UnknownSlot = new(
        id: "JAZORVUE009",
        title: "RazorVue child content parameter is unknown",
        messageFormat: "{0}",
        category: "RazorVue",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor SlotContextMisuse = new(
        id: "JAZORVUE010",
        title: "RazorVue child content parameter context is invalid",
        messageFormat: "{0}",
        category: "RazorVue",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor DuplicateSlotValue = new(
        id: "JAZORVUE011",
        title: "RazorVue child content parameter is assigned multiple times",
        messageFormat: "{0}",
        category: "RazorVue",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor InvalidLibraryComponentDeclaration = new(
        id: "JAZORVUE012",
        title: "RazorVue library component declaration is invalid",
        messageFormat: "{0}",
        category: "RazorVue",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor InvalidLibraryStyleDependencyDeclaration = new(
        id: "JAZORVUE013",
        title: "RazorVue library style dependency declaration is invalid",
        messageFormat: "{0}",
        category: "RazorVue",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor InvalidLibraryPluginRequirementDeclaration = new(
        id: "JAZORVUE014",
        title: "RazorVue library plugin requirement declaration is invalid",
        messageFormat: "{0}",
        category: "RazorVue",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);
}
