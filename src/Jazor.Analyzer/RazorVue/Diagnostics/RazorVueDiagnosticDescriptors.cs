using Microsoft.CodeAnalysis;

namespace Jazor.Analyzer;

internal static class RazorVueDiagnosticDescriptors
{
    public static readonly DiagnosticDescriptor InvalidEntryInheritance = new(
        id: "JAZORVUE001",
        title: "Invalid RazorVue component inheritance",
        messageFormat: "RazorVue entry '{0}' must inherit ComponentBase and implement IVueComponent",
        category: "RazorVue",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor DirectComponentBaseEntry = new(
        id: "JAZORVUE002",
        title: "Direct ComponentBase entry is not allowed",
        messageFormat: "RazorVue entry '{0}' must implement IVueComponent instead of using ComponentBase only",
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

    public static readonly DiagnosticDescriptor MissingSlotValue = new(
        id: "JAZORVUE015",
        title: "RazorVue child content parameter value is missing",
        messageFormat: "{0}",
        category: "RazorVue",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor InvalidComponentDeclaration = new(
        id: "JAZORVUE016",
        title: "RazorVue component declaration is invalid",
        messageFormat: "{0}",
        category: "RazorVue",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor InvalidContainerInjectDeclaration = new(
        id: "JAZORVUE018",
        title: "RazorVue container inject declaration is invalid",
        messageFormat: "{0}",
        category: "RazorVue",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);
}
