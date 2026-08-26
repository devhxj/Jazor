namespace Jazor.RazorVue.Sg.Test;

public enum RazorVueUsageScenarioFamily
{
    ContentValue,
    ElementAttributeValue,
    AttributeOverwrite,
    ComponentParameterValue,
    KeyExpression,
    ComponentOpenForm,
    RegionBoundary,
    EmptyAndNullOutput,
    ElementEventCallback,
    ElementBind,
    EventModifier,
    ElementReference,
    ComponentReference,
    DefaultSlot,
    NamedSlot,
    ScopedSlot,
    NestedElement,
    RepeatedComponentImport,
    HelperComposition,
    FragmentComposition,
    LocalPrelude,
    ConditionalComposition,
    ForeachCollection,
    DescriptorMapping,
    MissingAttributeTarget,
    InvalidUpdatesTarget,
    UpdatesAfterChild,
    InvalidEventModifierTarget,
    InvalidNamedEvent,
    InvalidRenderMode,
    ReferenceAfterChild,
    InvalidMetadataTiming
}

public enum RazorVueUsageScenarioArea
{
    Authoring,
    Callback,
    Template,
    Composition,
    Compiler,
    Diagnostic
}

public enum RazorVueUsageScenarioExpectation
{
    Emission,
    Diagnostic
}

public readonly record struct RazorVueUsageScenarioId(
    RazorVueUsageScenarioFamily Family,
    int Shape);

public sealed record RazorVueUsageScenarioDefinition(
    RazorVueUsageScenarioId Id,
    RazorVueUsageScenarioArea Area,
    RazorVueUsageScenarioExpectation Expectation,
    string Description);

internal static class RazorVueUsageScenarioCatalog
{
    public static IReadOnlyList<RazorVueUsageScenarioDefinition> All { get; } = CreateDefinitions();

    private static IReadOnlyList<RazorVueUsageScenarioDefinition> CreateDefinitions()
    {
        ScenarioFamilyDefinition[] families =
        [
            new(
                RazorVueUsageScenarioFamily.ContentValue,
                RazorVueUsageScenarioArea.Authoring,
                RazorVueUsageScenarioExpectation.Emission,
                [
                    "literal text content",
                    "parameter-derived text content",
                    "numeric expression content",
                    "conditional boolean content",
                    "nullable content among sibling roots",
                    "MarkupString static content",
                    "array-length expression content",
                    "inline RenderFragment content"
                ]),
            new(
                RazorVueUsageScenarioFamily.ElementAttributeValue,
                RazorVueUsageScenarioArea.Authoring,
                RazorVueUsageScenarioExpectation.Emission,
                [
                    "string element attribute",
                    "numeric element attribute",
                    "boolean element attribute",
                    "null element attribute",
                    "conditional element attribute value",
                    "minimized boolean element attribute",
                    "indexed nullable element attribute",
                    "interpolated element attribute"
                ]),
            new(
                RazorVueUsageScenarioFamily.AttributeOverwrite,
                RazorVueUsageScenarioArea.Authoring,
                RazorVueUsageScenarioExpectation.Emission,
                [
                    "duplicate element attributes",
                    "SetAttributeValue element overwrite",
                    "three ordered element attributes",
                    "conditional then explicit attribute",
                    "explicit then conditional attribute",
                    "expression overwrite on element",
                    "duplicate component parameters",
                    "SetAttributeValue component overwrite"
                ]),
            new(
                RazorVueUsageScenarioFamily.ComponentParameterValue,
                RazorVueUsageScenarioArea.Authoring,
                RazorVueUsageScenarioExpectation.Emission,
                [
                    "descriptor-renamed string prop",
                    "numeric component prop expression",
                    "boolean component prop expression",
                    "nullable model prop",
                    "interpolated component prop",
                    "array-length component prop",
                    "relational component prop",
                    "parameter-forwarded model prop"
                ]),
            new(
                RazorVueUsageScenarioFamily.KeyExpression,
                RazorVueUsageScenarioArea.Authoring,
                RazorVueUsageScenarioExpectation.Emission,
                [
                    "literal element key",
                    "numeric element key",
                    "conditional element key",
                    "literal component key",
                    "parameter component key",
                    "foreach element key",
                    "foreach component key",
                    "local alias element key"
                ]),
            new(
                RazorVueUsageScenarioFamily.ComponentOpenForm,
                RazorVueUsageScenarioArea.Authoring,
                RazorVueUsageScenarioExpectation.Emission,
                [
                    "generic OpenComponent",
                    "typeof OpenComponent",
                    "local typeof OpenComponent",
                    "local typeof component across conditional branches",
                    "generic component with props",
                    "typeof component with props",
                    "local-type component in foreach",
                    "component opened by render helper"
                ]),
            new(
                RazorVueUsageScenarioFamily.RegionBoundary,
                RazorVueUsageScenarioArea.Authoring,
                RazorVueUsageScenarioExpectation.Emission,
                [
                    "empty region",
                    "single-child region",
                    "multi-child region",
                    "nested region with empty child",
                    "element inside region",
                    "empty region inside element",
                    "conditional region content",
                    "foreach region content"
                ]),
            new(
                RazorVueUsageScenarioFamily.EmptyAndNullOutput,
                RazorVueUsageScenarioArea.Authoring,
                RazorVueUsageScenarioExpectation.Emission,
                [
                    "dispose-only render",
                    "explicit null content",
                    "empty region output",
                    "empty fragment output",
                    "terminating conditional output",
                    "Clear removes prior text output",
                    "Clear removes prior static output",
                    "empty conditional output"
                ]),
            new(
                RazorVueUsageScenarioFamily.ElementEventCallback,
                RazorVueUsageScenarioArea.Callback,
                RazorVueUsageScenarioExpectation.Emission,
                [
                    "EventCallback method handler",
                    "EventCallback lambda handler",
                    "Action method handler",
                    "typed string callback",
                    "typed change-event callback",
                    "typed callback assignment lambda",
                    "typed mouse-event callback",
                    "state-mutating callback lambda"
                ]),
            new(
                RazorVueUsageScenarioFamily.ElementBind,
                RazorVueUsageScenarioArea.Callback,
                RazorVueUsageScenarioExpectation.Emission,
                [
                    "input value bound on input",
                    "input value bound on change",
                    "textarea value binding",
                    "select value binding",
                    "checkbox checked binding",
                    "numeric value binding",
                    "nullable value binding",
                    "manual callback with updates metadata"
                ]),
            new(
                RazorVueUsageScenarioFamily.EventModifier,
                RazorVueUsageScenarioArea.Callback,
                RazorVueUsageScenarioExpectation.Emission,
                [
                    "enabled prevent-default modifier",
                    "enabled stop-propagation modifier",
                    "combined enabled modifiers",
                    "disabled prevent-default modifier",
                    "disabled stop-propagation modifier",
                    "dynamic prevent-default modifier",
                    "dynamic stop-propagation modifier",
                    "combined dynamic modifiers"
                ]),
            new(
                RazorVueUsageScenarioFamily.ElementReference,
                RazorVueUsageScenarioArea.Callback,
                RazorVueUsageScenarioExpectation.Emission,
                [
                    "element reference field assignment",
                    "element reference method handler",
                    "multiple element reference captures",
                    "keyed element reference",
                    "attributed element reference",
                    "bound element reference",
                    "conditional element references",
                    "repeated element references"
                ]),
            new(
                RazorVueUsageScenarioFamily.ComponentReference,
                RazorVueUsageScenarioArea.Callback,
                RazorVueUsageScenarioExpectation.Emission,
                [
                    "component reference field assignment",
                    "component reference method handler",
                    "multiple component reference captures",
                    "keyed component reference",
                    "parameterized component reference",
                    "typeof component reference",
                    "conditional component references",
                    "repeated component references"
                ]),
            new(
                RazorVueUsageScenarioFamily.DefaultSlot,
                RazorVueUsageScenarioArea.Template,
                RazorVueUsageScenarioExpectation.Emission,
                [
                    "default slot text",
                    "default slot element",
                    "default slot sibling roots",
                    "default slot static markup",
                    "default slot conditional content",
                    "default slot repeated content",
                    "default slot returned by helper",
                    "default slot returned by property"
                ]),
            new(
                RazorVueUsageScenarioFamily.NamedSlot,
                RazorVueUsageScenarioArea.Template,
                RazorVueUsageScenarioExpectation.Emission,
                [
                    "named slot text",
                    "named slot element",
                    "named slot sibling roots",
                    "named slot static markup",
                    "named slot conditional content",
                    "named slot repeated content",
                    "named slot returned by helper",
                    "named slot returned by property"
                ]),
            new(
                RazorVueUsageScenarioFamily.ScopedSlot,
                RazorVueUsageScenarioArea.Template,
                RazorVueUsageScenarioExpectation.Emission,
                [
                    "scoped slot text projection",
                    "scoped slot element projection",
                    "scoped slot sibling roots",
                    "scoped slot conditional projection",
                    "scoped slot with outer parameter",
                    "scoped slot repeated projection",
                    "scoped slot returned by helper",
                    "scoped slot returned by property"
                ]),
            new(
                RazorVueUsageScenarioFamily.NestedElement,
                RazorVueUsageScenarioArea.Composition,
                RazorVueUsageScenarioExpectation.Emission,
                [
                    "element with nested child",
                    "three-level nested elements",
                    "element with sibling children",
                    "element containing component",
                    "component slot containing element",
                    "element containing static markup",
                    "element containing region",
                    "element containing repeated children"
                ]),
            new(
                RazorVueUsageScenarioFamily.RepeatedComponentImport,
                RazorVueUsageScenarioArea.Compiler,
                RazorVueUsageScenarioExpectation.Emission,
                [
                    "sibling repeated components",
                    "conditional repeated component",
                    "repeated component collection",
                    "fragment and root component reuse",
                    "helper component reuse",
                    "nested slot component reuse",
                    "generic and typeof component reuse",
                    "keyed component reuse"
                ]),
            new(
                RazorVueUsageScenarioFamily.HelperComposition,
                RazorVueUsageScenarioArea.Composition,
                RazorVueUsageScenarioExpectation.Emission,
                [
                    "helper text composition",
                    "helper element composition",
                    "helper component composition",
                    "helper region composition",
                    "helper conditional composition",
                    "helper collection composition",
                    "nested helper composition",
                    "helper local prelude composition"
                ]),
            new(
                RazorVueUsageScenarioFamily.FragmentComposition,
                RazorVueUsageScenarioArea.Composition,
                RazorVueUsageScenarioExpectation.Emission,
                [
                    "fragment text composition",
                    "fragment element composition",
                    "fragment sibling roots",
                    "fragment static markup",
                    "fragment conditional content",
                    "fragment collection content",
                    "nested fragment invocation",
                    "conditional fragment selection"
                ]),
            new(
                RazorVueUsageScenarioFamily.LocalPrelude,
                RazorVueUsageScenarioArea.Compiler,
                RazorVueUsageScenarioExpectation.Emission,
                [
                    "string local prelude",
                    "numeric local prelude",
                    "boolean local prelude",
                    "collection alias prelude",
                    "nullable local prelude",
                    "markup local prelude",
                    "component parameter local prelude",
                    "local prelude after prior output"
                ]),
            new(
                RazorVueUsageScenarioFamily.ConditionalComposition,
                RazorVueUsageScenarioArea.Composition,
                RazorVueUsageScenarioExpectation.Emission,
                [
                    "conditional text composition",
                    "conditional element composition",
                    "conditional component composition",
                    "conditional empty branch",
                    "conditional attribute composition",
                    "conditional slot composition",
                    "nested conditional composition",
                    "conditional fragment composition"
                ]),
            new(
                RazorVueUsageScenarioFamily.ForeachCollection,
                RazorVueUsageScenarioArea.Compiler,
                RazorVueUsageScenarioExpectation.Emission,
                [
                    "string collection content",
                    "numeric collection content",
                    "nullable collection content",
                    "local collection alias",
                    "collection element projection",
                    "nested collection projection",
                    "collection component projection",
                    "collection fragment projection"
                ]),
            new(
                RazorVueUsageScenarioFamily.DescriptorMapping,
                RazorVueUsageScenarioArea.Compiler,
                RazorVueUsageScenarioExpectation.Emission,
                [
                    "renamed prop descriptor",
                    "model prop descriptor",
                    "event descriptor",
                    "model-update event descriptor",
                    "default slot descriptor",
                    "named slot descriptor",
                    "scoped slot descriptor",
                    "combined descriptor mapping"
                ]),
            new(
                RazorVueUsageScenarioFamily.MissingAttributeTarget,
                RazorVueUsageScenarioArea.Diagnostic,
                RazorVueUsageScenarioExpectation.Diagnostic,
                [
                    "attribute without open frame",
                    "component parameter without component",
                    "multiple attributes without open frame",
                    "attribute update without open frame",
                    "attribute after closed element",
                    "parameter after closed component",
                    "attribute on region frame",
                    "component parameter on element frame"
                ]),
            new(
                RazorVueUsageScenarioFamily.InvalidUpdatesTarget,
                RazorVueUsageScenarioArea.Diagnostic,
                RazorVueUsageScenarioExpectation.Diagnostic,
                [
                    "updates metadata without frame",
                    "updates metadata on component",
                    "updates metadata on region",
                    "updates metadata after closed element",
                    "updates metadata after closed component",
                    "updates metadata after clear",
                    "updates metadata on nested region",
                    "updates metadata on nested component"
                ]),
            new(
                RazorVueUsageScenarioFamily.UpdatesAfterChild,
                RazorVueUsageScenarioArea.Diagnostic,
                RazorVueUsageScenarioExpectation.Diagnostic,
                [
                    "updates metadata after text child",
                    "updates metadata after markup child",
                    "updates metadata after element child",
                    "updates metadata after region child",
                    "updates metadata after component child",
                    "updates metadata after fragment child",
                    "updates metadata after conditional child",
                    "updates metadata after referenced child"
                ]),
            new(
                RazorVueUsageScenarioFamily.InvalidEventModifierTarget,
                RazorVueUsageScenarioArea.Diagnostic,
                RazorVueUsageScenarioExpectation.Diagnostic,
                [
                    "event modifier without frame",
                    "event modifier on component",
                    "event modifier on region",
                    "event modifier after closed element",
                    "event modifier after text child",
                    "event modifier after element child",
                    "event modifier on nested component",
                    "event modifier after fragment child"
                ]),
            new(
                RazorVueUsageScenarioFamily.InvalidNamedEvent,
                RazorVueUsageScenarioArea.Diagnostic,
                RazorVueUsageScenarioExpectation.Diagnostic,
                [
                    "named event without frame",
                    "named event on component",
                    "named event after child",
                    "dynamic source event name",
                    "dynamic assigned event name",
                    "empty source event name",
                    "blank assigned event name",
                    "conditional assigned event name"
                ]),
            new(
                RazorVueUsageScenarioFamily.InvalidRenderMode,
                RazorVueUsageScenarioArea.Diagnostic,
                RazorVueUsageScenarioExpectation.Diagnostic,
                [
                    "render mode without frame",
                    "render mode on element",
                    "render mode on region",
                    "render mode after closed component",
                    "render mode after text child",
                    "render mode after element child",
                    "render mode on nested element",
                    "render mode after root output"
                ]),
            new(
                RazorVueUsageScenarioFamily.ReferenceAfterChild,
                RazorVueUsageScenarioArea.Diagnostic,
                RazorVueUsageScenarioExpectation.Diagnostic,
                [
                    "element reference after text child",
                    "element reference after markup child",
                    "element reference after element child",
                    "element reference after region child",
                    "component reference after text child",
                    "component reference after element child",
                    "element reference after fragment child",
                    "component reference after component child"
                ]),
            new(
                RazorVueUsageScenarioFamily.InvalidMetadataTiming,
                RazorVueUsageScenarioArea.Diagnostic,
                RazorVueUsageScenarioExpectation.Diagnostic,
                [
                    "key metadata after child",
                    "attribute update after child",
                    "named event metadata after child",
                    "render mode metadata after child",
                    "multiple attributes after child",
                    "attribute metadata after child",
                    "updates metadata after child",
                    "event modifier metadata after child"
                ])
        ];

        var definitions = new List<RazorVueUsageScenarioDefinition>(families.Length * 8);
        foreach (var family in families)
        {
            if (family.ShapeDescriptions.Length != 8)
                throw new InvalidOperationException(family.Family + " must define exactly eight usage shapes.");

            for (var shape = 0; shape < family.ShapeDescriptions.Length; shape++)
            {
                definitions.Add(new RazorVueUsageScenarioDefinition(
                    new RazorVueUsageScenarioId(family.Family, shape),
                    family.Area,
                    family.Expectation,
                    family.ShapeDescriptions[shape]));
            }
        }

        return definitions;
    }

    private sealed record ScenarioFamilyDefinition(
        RazorVueUsageScenarioFamily Family,
        RazorVueUsageScenarioArea Area,
        RazorVueUsageScenarioExpectation Expectation,
        string[] ShapeDescriptions);
}

public enum RazorVueCapabilityPriority
{
    P0,
    P1,
    P2
}

public enum RazorVueCapabilityDecision
{
    DirectSupport,
    CompatibilityAdapter,
    GuidedAdaptation,
    Reject
}

public enum RazorVueCapabilityStatus
{
    Planned,
    InProof,
    Support,
    Guidance,
    Reject
}

[Flags]
public enum RazorVueCapabilityEvidence
{
    None = 0,
    AuthorSource = 1 << 0,
    OfficialRazorSourceGenerator = 1 << 1,
    ModuleArtifact = 1 << 2,
    DenoRuntime = 1 << 3,
    BrowserSmoke = 1 << 4,
    SsrHydration = 1 << 5,
    PackageConsumer = 1 << 6
}

/// <summary>
/// One auditable M5 authoring-surface decision. This is a maintainer ledger, not a
/// prerequisite API for page authors: supported rows stay normal Blazor Razor/C#.
/// M5 ledger 只用于维护者验收；页面作者仍只需写标准 Blazor Razor/C#。
/// </summary>
public sealed record RazorVueCapabilityLedgerEntry(
    string Id,
    string AuthoringShape,
    RazorVueCapabilityPriority Priority,
    RazorVueCapabilityDecision Decision,
    RazorVueCapabilityStatus Status,
    string Owner,
    string? DiagnosticId,
    string Fixture,
    RazorVueCapabilityEvidence Evidence,
    string Blocker)
{
    /// <summary>Browser/SSR profile explicitly covered by the entry.</summary>
    public string TargetProfiles { get; init; } = string.Empty;

    /// <summary>Runtime carrier used by the framework value or mapping.</summary>
    public string Carrier { get; init; } = string.Empty;

    /// <summary>Stable lowering owner and operation kind (WebIDL, Alias, Inline, Import, or Compile).</summary>
    public string ImplementationPath { get; init; } = string.Empty;

    /// <summary>Version of the mapping contribution contract consumed by this entry.</summary>
    public string ContributionContractVersion { get; init; } = string.Empty;

    /// <summary>Approved shared runtime/module dependencies.</summary>
    public string Dependencies { get; init; } = string.Empty;

    /// <summary>Authoring/runtime surface intentionally excluded from the entry.</summary>
    public string ExcludedSurface { get; init; } = string.Empty;
}

/// <summary>
/// Blazor-first compatibility baseline. Keep an explicit entry for every major authoring
/// family so a missing proof never becomes an implied runtime fallback.
/// 每个作者面必须有明确决策和证据；未完成 adapter 不能因可以编译而被标记为 Support。
/// </summary>
internal static class RazorVueM5CapabilityLedger
{
    public static IReadOnlyList<RazorVueCapabilityLedgerEntry> All { get; } =
    [
        new(
            "P0-markup-composition",
            "Razor markup, component composition, regions, static markup, and normal child content",
            RazorVueCapabilityPriority.P0,
            RazorVueCapabilityDecision.DirectSupport,
            RazorVueCapabilityStatus.Support,
            "RenderEmitter + VueModuleBuilder",
            null,
            "RazorSgOfficialComponentCompositionAuthoringTests",
            RazorVueCapabilityEvidence.AuthorSource |
            RazorVueCapabilityEvidence.OfficialRazorSourceGenerator |
            RazorVueCapabilityEvidence.ModuleArtifact |
            RazorVueCapabilityEvidence.DenoRuntime,
            "Release browser/package proof remains owned by the consumer gate."),
        new(
            "P0-generic-templates-fragments",
            "Generic components, TypeInference helpers, RenderFragment, RenderFragment<T>, typed slots, and member-reachable fragments",
            RazorVueCapabilityPriority.P0,
            RazorVueCapabilityDecision.DirectSupport,
            RazorVueCapabilityStatus.Support,
            "RenderEmitter + MemberClosureBuilder",
            null,
            "RazorSgOfficialGenericComponentBindingRuntimeTests; RazorSgOfficialTDesignTableCellRuntimeTests",
            RazorVueCapabilityEvidence.AuthorSource |
            RazorVueCapabilityEvidence.OfficialRazorSourceGenerator |
            RazorVueCapabilityEvidence.ModuleArtifact |
            RazorVueCapabilityEvidence.DenoRuntime,
            "Dynamic runtime Type and unresolved fragment factories remain final-pipeline owned."),
        new(
            "P0-control-flow-attributes-identity",
            "Conditional/loop rendering, conditional attributes, attribute splat, @key, and @ref",
            RazorVueCapabilityPriority.P0,
            RazorVueCapabilityDecision.DirectSupport,
            RazorVueCapabilityStatus.Support,
            "RenderEmitter",
            null,
            "RazorSgDirectRenderAdvancedMatrixTests; RazorSgOfficialKeyAuthoringTests; RazorSgOfficialReferenceAuthoringTests",
            RazorVueCapabilityEvidence.AuthorSource |
            RazorVueCapabilityEvidence.OfficialRazorSourceGenerator |
            RazorVueCapabilityEvidence.ModuleArtifact |
            RazorVueCapabilityEvidence.DenoRuntime,
            "Cross-frame branch and unresolved builder protocol shapes remain final-pipeline rejects."),
        new(
            "P0-bind-events",
            "DOM/component @bind, EventCallback, async handlers, and event modifiers",
            RazorVueCapabilityPriority.P0,
            RazorVueCapabilityDecision.DirectSupport,
            RazorVueCapabilityStatus.Support,
            "RenderEmitter + CurrentComponentSemanticWalkerHost",
            null,
            "RazorSgOfficialBindAfterRuntimeTests; RazorSgOfficialEventModifierRuntimeTests",
            RazorVueCapabilityEvidence.AuthorSource |
            RazorVueCapabilityEvidence.OfficialRazorSourceGenerator |
            RazorVueCapabilityEvidence.ModuleArtifact |
            RazorVueCapabilityEvidence.DenoRuntime,
            "Browser user-journey proof is still required before a consumer-specific form workflow is promoted."),
        new(
            "P0-parameter-lifecycle",
            "[Parameter], replacement-driven OnParametersSet*, StateHasChanged, ShouldRender, and dispose",
            RazorVueCapabilityPriority.P0,
            RazorVueCapabilityDecision.DirectSupport,
            RazorVueCapabilityStatus.Support,
            "MemberClosureBuilder + VueModuleBuilder",
            null,
            "RazorSgParameterLifecycleWatchRuntimeTests; RazorSgOfficialExplicitLifecycleRuntimeTests",
            RazorVueCapabilityEvidence.AuthorSource |
            RazorVueCapabilityEvidence.OfficialRazorSourceGenerator |
            RazorVueCapabilityEvidence.ModuleArtifact |
            RazorVueCapabilityEvidence.DenoRuntime,
            "ParameterView/SetParametersAsync has a separate P1 entry because it needs a real snapshot protocol."),
        new(
            "P0-route-layout-page-state",
            "@page, @layout, route parameters, not-found, and normal loading/error/retry page workflows (without Microsoft built-in Router/RouteView/LayoutView/NavLink tags)",
            RazorVueCapabilityPriority.P0,
            RazorVueCapabilityDecision.CompatibilityAdapter,
            RazorVueCapabilityStatus.InProof,
            "RazorVue route host",
            null,
            "RazorSourceGeneratorTailOutputTests.RouteCatalog_EmitsDeterministicPageLayoutAndQueryMappings",
            RazorVueCapabilityEvidence.AuthorSource |
            RazorVueCapabilityEvidence.OfficialRazorSourceGenerator |
            RazorVueCapabilityEvidence.ModuleArtifact |
            RazorVueCapabilityEvidence.DenoRuntime,
            "The route catalog owns @page/@layout/query mapping; Microsoft Router/RouteView/LayoutView/NavLink tags are outside the product contract."),
        new(
            "P1-parameter-view",
            "SetParametersAsync(ParameterView), parameter snapshot, overlay order, and async error behavior",
            RazorVueCapabilityPriority.P1,
            RazorVueCapabilityDecision.CompatibilityAdapter,
            RazorVueCapabilityStatus.InProof,
            "MemberClosureBuilder + VueModuleBuilder ParameterView adapter",
            null,
            "RazorSgSetParametersAsyncRuntimeTests (sparse, alias, slot, SetParameterProperties, queue); MemberClosureBuilderContractTests.TryBuild_AcceptsParameterViewRuntimeEntryPoint",
            RazorVueCapabilityEvidence.AuthorSource |
            RazorVueCapabilityEvidence.OfficialRazorSourceGenerator |
            RazorVueCapabilityEvidence.ModuleArtifact |
            RazorVueCapabilityEvidence.DenoRuntime,
            "The adapter preserves CLR defaults before base application, sparse source-name overlay, explicit undefined, RenderFragment slots, lifecycle order, and queued updates; authored exception propagation and browser/SSR consumer proof remain before Support."),
        new(
            "P1-parameter-view-unsupported-members",
            "ParameterView.TryGetValue, enumeration, and ToDictionary used from authored component logic",
            RazorVueCapabilityPriority.P1,
            RazorVueCapabilityDecision.GuidedAdaptation,
            RazorVueCapabilityStatus.Guidance,
            "RazorVueCompatibilityAnalyzer",
            "JAZORVCA003/JAZORVCA004/JAZORVCA005",
            "RazorVueCompatibilityAnalyzerTests.ParameterViewTryGetValue_ReportsAtAuthoredInvocation; RazorVueCompatibilityAnalyzerTests.ParameterViewEnumeration_ReportsAtAuthoredCollection; RazorVueCompatibilityAnalyzerTests.ParameterViewToDictionary_ReportsAtAuthoredInvocation",
            RazorVueCapabilityEvidence.AuthorSource,
            "The standard snapshot entry point is supported, but arbitrary parameter-bag inspection is not materialized; typed [Parameter] properties are the direct authoring replacement."),
        new(
            "P1-server-only-injection",
            "[Inject]/@inject DbContext or derived database context",
            RazorVueCapabilityPriority.P1,
            RazorVueCapabilityDecision.Reject,
            RazorVueCapabilityStatus.Reject,
            "RazorVueCompatibilityAnalyzer",
            "JAZORVCA001",
            "RazorVueCompatibilityAnalyzerTests",
            RazorVueCapabilityEvidence.AuthorSource,
            "Use a typed endpoint client; DbContext cannot be materialized in a browser bundle."),
        new(
            "P1-server-only-aspnet-injection",
            "[Inject]/@inject HttpContext, IHttpContextAccessor, ASP.NET host environment, or Identity manager",
            RazorVueCapabilityPriority.P1,
            RazorVueCapabilityDecision.Reject,
            RazorVueCapabilityStatus.Reject,
            "RazorVueCompatibilityAnalyzer",
            "JAZORVCA002",
            "RazorVueCompatibilityAnalyzerTests.InjectedHttpContext_ReportsServerOnlyServiceAtAuthoredAttribute",
            RazorVueCapabilityEvidence.AuthorSource,
            "Move request/identity work to a server endpoint and inject a typed browser client."),
        new(
            "P1-injection-property-shape",
            "[Inject] properties with readonly, init-only, custom-setter, or static activation shape",
            RazorVueCapabilityPriority.P1,
            RazorVueCapabilityDecision.GuidedAdaptation,
            RazorVueCapabilityStatus.Guidance,
            "RazorVueCompatibilityAnalyzer + VueModuleBuilder activation",
            "JAZORVCA006",
            "RazorVueCompatibilityAnalyzerTests.InjectedBrowserServiceWithReadOnlyProperty_ReportsAtAuthoredInjectAttribute; RazorVueCompatibilityAnalyzerTests.InjectPropertyShapeRule_StillRunsWithoutOptionalServerMetadata",
            RazorVueCapabilityEvidence.AuthorSource,
            "Use a normal writable auto-property; the adapter assigns it after initialization and before lifecycle callbacks."),
        new(
            "P1-known-host-service-adapter-gap",
            "[Inject]/@inject known Blazor circuit, protected-storage, or host activation services without a browser adapter",
            RazorVueCapabilityPriority.P1,
            RazorVueCapabilityDecision.GuidedAdaptation,
            RazorVueCapabilityStatus.Guidance,
            "RazorVueCompatibilityAnalyzer",
            "JAZORVCA007",
            "RazorVueCompatibilityAnalyzerTests.InjectedBlazorHostServiceWithoutAdapter_ReportsAtAuthoredInjectAttribute",
            RazorVueCapabilityEvidence.AuthorSource,
            "Register a typed browser adapter or move the operation behind an endpoint; ordinary application services remain quiet when they have a valid writable property."),
        new(
            "P1-browser-service-injection",
            "[Inject]/@inject property activation for browser-capable services",
            RazorVueCapabilityPriority.P1,
            RazorVueCapabilityDecision.CompatibilityAdapter,
            RazorVueCapabilityStatus.InProof,
            "VueInjectRegistry + VueModuleBuilder component activation adapter",
            null,
            "RazorSgInjectedServiceRuntimeTests (provider, lifecycle order, missing-provider failure)",
            RazorVueCapabilityEvidence.AuthorSource |
            RazorVueCapabilityEvidence.OfficialRazorSourceGenerator |
            RazorVueCapabilityEvidence.ModuleArtifact |
            RazorVueCapabilityEvidence.DenoRuntime,
            "Constructor injection/parameterized activation remains an explicit JAZORVGA024 boundary; provider lifetime and browser/SSR consumer proof are still required before Support."),
        new(
            "P1-cascading-values",
            "CascadingValue, [CascadingParameter], named cascades, nested override, and updates",
            RazorVueCapabilityPriority.P1,
            RazorVueCapabilityDecision.CompatibilityAdapter,
            RazorVueCapabilityStatus.InProof,
            "Cascading adapter + VueModuleBuilder lifecycle bridge",
            "JAZORVCA008",
            "RazorSgCascadingValueRuntimeTests; RazorVueCompatibilityAnalyzerTests.WritableCascadingParameter_IsHandledByBrowserAdapterWithoutDiagnostic",
            RazorVueCapabilityEvidence.AuthorSource |
            RazorVueCapabilityEvidence.OfficialRazorSourceGenerator |
            RazorVueCapabilityEvidence.ModuleArtifact |
            RazorVueCapabilityEvidence.DenoRuntime,
            "Typed/named provider lookup, nested Vue scope behavior, IsFixed and browser/package consumer proof remain before Support."),
        new(
            "P1-navigation-router",
            "NavigationManager and query/route parameter refresh through the RazorVue route catalog (Microsoft Router/RouteView/LayoutView/NavLink tags excluded)",
            RazorVueCapabilityPriority.P1,
            RazorVueCapabilityDecision.CompatibilityAdapter,
            RazorVueCapabilityStatus.InProof,
            "RazorVue route catalog + blazor-routing.mjs",
            null,
            "RazorSgNavigationRuntimeTests.NavigationManager_UsesBrowserAdapterForUriAndNavigateTo; RazorTailOutputTests.RouteCatalog_EmitsDeterministicPageAndLayoutEntries",
            RazorVueCapabilityEvidence.AuthorSource |
            RazorVueCapabilityEvidence.OfficialRazorSourceGenerator |
            RazorVueCapabilityEvidence.ModuleArtifact |
            RazorVueCapabilityEvidence.DenoRuntime,
            "Router component matching and standard route-host composition are intentionally excluded; route catalog and NavigationManager consumer proof remain before Support."),
        new(
            "P0-blazor-clr-mapping-package",
            "ECMAScript.Blazor first-party mapping contribution delivered with Jazor.Vue and consumed by the static compiler source-root",
            RazorVueCapabilityPriority.P0,
            RazorVueCapabilityDecision.CompatibilityAdapter,
            RazorVueCapabilityStatus.InProof,
            "Jazor.Compiler.Generator + Jazor.Vue packaging + ECMAScript.Blazor",
            null,
            "EcmaScriptBlazorMappingTests; ProductionRazorCompilerReferenceTests; Jazor.EmitTest.SdkIntegrationTests.CreateLocalPackage_SeparatesSharedAndRazorVueAnalyzers; Jazor.EmitTest.SdkIntegrationTests.CreateLocalPackage_IncludesSelfContainedBrowserAssets",
            RazorVueCapabilityEvidence.AuthorSource |
            RazorVueCapabilityEvidence.ModuleArtifact |
            RazorVueCapabilityEvidence.PackageConsumer,
            "The first-party static contribution is proven; a general third-party per-compilation provider protocol is not claimed.")
        {
            TargetProfiles = "Compiler authoring; Browser interactive package payload",
            Carrier = "Provider assembly metadata and generated whitelist entries",
            ImplementationPath = "ECMAScript.Blazor Alias/Inline/Import declarations merged by Jazor.Compiler.Generator source-root",
            ContributionContractVersion = "static-source-root/v1",
            Dependencies = "Jazor.Vue lib/net11.0 payload; Jazor.CLR runtime catalog; Microsoft.AspNetCore.App reference",
            ExcludedSurface = "Dynamic third-party mapping discovery; duplicate provider registries; Blazor assets in core Jazor package"
        },
        new(
            "P1-blazor-clr-navigation-location-changing",
            "NavigationManager.RegisterLocationChangingHandler(Func<LocationChangingContext, ValueTask>), LocationChangingContext, CancellationToken, and IDisposable registration",
            RazorVueCapabilityPriority.P1,
            RazorVueCapabilityDecision.CompatibilityAdapter,
            RazorVueCapabilityStatus.InProof,
            "ECMAScript.Blazor mapping + Jazor.CLR NavigationManagerModule + RazorVue route host",
            null,
            "RazorSgNavigationRuntimeTests; ClrRuntimeNavigationScenarios; NavigationManagerCatalogWhitelistTests",
            RazorVueCapabilityEvidence.AuthorSource |
            RazorVueCapabilityEvidence.OfficialRazorSourceGenerator |
            RazorVueCapabilityEvidence.ModuleArtifact |
            RazorVueCapabilityEvidence.DenoRuntime,
            "Reference oracle, real BrowserSmoke, and isolated Release PackageConsumer are still required; popstate/hashchange cancellation remains unclaimed.")
        {
            TargetProfiles = "Browser interactive; SSR/prerender not claimed",
            Carrier = "Promise/AbortSignal + module-private navigation host WeakMap",
            ImplementationPath = "ECMAScript.Blazor mapping; Jazor.CLR C# Import modules; RazorVue host framing",
            ContributionContractVersion = "static-source-root/v1",
            Dependencies = "Task/ValueTask + CancellationToken carriers; NavigationManager route host",
            ExcludedSurface = "Router/RouteView/LayoutView/NavLink tags; popstate/hashchange cancellation; server circuit identity"
        },
        new(
            "P1-blazor-clr-core-dom-events",
            "MouseEventArgs, KeyboardEventArgs, FocusEventArgs getter projections and ChangeEventArgs.Value event-time capture",
            RazorVueCapabilityPriority.P1,
            RazorVueCapabilityDecision.CompatibilityAdapter,
            RazorVueCapabilityStatus.InProof,
            "ECMAScript.Blazor mappings + Jazor.RazorVue RenderEmitter + Jazor.CLR ChangeEventArgsModule",
            null,
            "EcmaScriptBlazorMappingTests; RazorSgOfficialBindingAuthoringTests.BuildComponent_OfficialRazorTypedChangeHandler_CapturesValueBeforeCallback; ClrRuntimeChangeEventArgsScenarios",
            RazorVueCapabilityEvidence.AuthorSource |
            RazorVueCapabilityEvidence.OfficialRazorSourceGenerator |
            RazorVueCapabilityEvidence.ModuleArtifact |
            RazorVueCapabilityEvidence.DenoRuntime,
            "Mouse/Keyboard/Focus official handler coverage, real BrowserSmoke, and isolated mapping PackageConsumer remain; constructor/setter/identity and file input stay rejected.")
        {
            TargetProfiles = "Browser interactive; SSR/prerender not claimed",
            Carrier = "MouseEvent/KeyboardEvent/FocusEvent; JazorEvent + WeakMap for ChangeEventArgs",
            ImplementationPath = "ECMAScript.Blazor Alias/Inline/Import; one typed onchange capture wrapper in RenderEmitter; Jazor.CLR C# Import helper",
            ContributionContractVersion = "static-source-root/v1",
            Dependencies = "WebIDL event carriers; Jazor.CLR WeakMap/Array runtime; EventCallback framing",
            ExcludedSurface = "Synthetic EventArgs construction, setters, runtime identity/type tests, InputFile/IBrowserFile"
        },
        new(
            "P1-blazor-clr-element-reference",
            "@ref ElementReference capture and ElementReferenceExtensions.FocusAsync overloads",
            RazorVueCapabilityPriority.P1,
            RazorVueCapabilityDecision.DirectSupport,
            RazorVueCapabilityStatus.InProof,
            "ECMAScript.Blazor mapping + Jazor.RazorVue VNode ref framing",
            null,
            "EcmaScriptBlazorMappingTests; RazorSgOfficialReferenceAuthoringTests.BuildComponent_OfficialRazorElementReferenceFocus_UsesDomCarrierMapping",
            RazorVueCapabilityEvidence.AuthorSource |
            RazorVueCapabilityEvidence.OfficialRazorSourceGenerator |
            RazorVueCapabilityEvidence.ModuleArtifact,
            "Real BrowserSmoke, isolated Release PackageConsumer, and empty/unmounted element behavior remain to be proved.")
        {
            TargetProfiles = "Browser interactive; SSR/prerender not claimed",
            Carrier = "HTMLElement captured by Vue ref callback",
            ImplementationPath = "ECMAScript.Blazor Alias(ElementReference -> HTMLElement) + Inline(HTMLElement.Focus)",
            ContributionContractVersion = "static-source-root/v1",
            Dependencies = "HTMLElement/FocusOptions WebIDL; ValueTask/Promise carrier; Vue ref lifecycle",
            ExcludedSurface = "new ElementReference, Id/Context server identity, arbitrary DOM methods"
        },
        new(
            "P2-blazor-clr-extended-dom-events",
            "Pointer/Wheel/Drag/Clipboard/Touch/Error/Progress EventArgs groups",
            RazorVueCapabilityPriority.P2,
            RazorVueCapabilityDecision.DirectSupport,
            RazorVueCapabilityStatus.Planned,
            "ECMAScript.Blazor mapping provider",
            null,
            "No fixture yet; each event group requires an independent official Razor/browser scenario",
            RazorVueCapabilityEvidence.None,
            "Do not infer support from the core event carrier; add one group only after a concrete authoring scenario and browser contract exist.")
        {
            TargetProfiles = "Browser interactive only after group-specific proof",
            Carrier = "PointerEvent/WheelEvent/DragEvent/ClipboardEvent/TouchEvent and group-specific WebIDL carriers",
            ImplementationPath = "Future ECMAScript.Blazor Alias/Inline or Jazor.CLR Import only when property conversion requires it",
            ContributionContractVersion = "static-source-root/v1",
            Dependencies = "Core DOM event mappings and group-specific WebIDL bindings",
            ExcludedSurface = "File payloads, permission-sensitive clipboard/file APIs, synthetic event construction"
        },
        new(
            "P1-standard-blazor-component-adapters",
            "Microsoft built-in UI components: DynamicComponent, EditForm, ErrorBoundary, Router, RouteView, LayoutView, NavLink, and Input*",
            RazorVueCapabilityPriority.P1,
            RazorVueCapabilityDecision.Reject,
            RazorVueCapabilityStatus.Reject,
            "RazorVueCompatibilityAnalyzer + RenderEmitter",
            "JAZORVGA021",
            "RazorVueCompatibilityAnalyzerTests.StandardBlazorComponentTag_ReportsUnsupportedBuiltInUi; RazorSgStandardBlazorComponentRuntimeTests",
            RazorVueCapabilityEvidence.AuthorSource |
            RazorVueCapabilityEvidence.OfficialRazorSourceGenerator,
            "Use ComponentBase + IVueComponent with [ECMAScriptModule] or [ECMAScript(import, Transform.Component, exportName)] metadata, or use a typed TDesign/Vuetify/Element Plus/custom component. Historical adapters are not product support."),
        new(
            "P1-dynamic-component",
            "DynamicComponent with statically discoverable component type and validated parameters",
            RazorVueCapabilityPriority.P1,
            RazorVueCapabilityDecision.Reject,
            RazorVueCapabilityStatus.Reject,
            "RazorVueCompatibilityAnalyzer + RenderEmitter",
            "JAZORVGA021",
            "RazorVueCompatibilityAnalyzerTests.StandardBlazorComponentTag_ReportsUnsupportedBuiltInUi; RazorSgStandardBlazorComponentRuntimeTests.DynamicComponent_IsRejectedAsBuiltInUi",
            RazorVueCapabilityEvidence.AuthorSource |
            RazorVueCapabilityEvidence.OfficialRazorSourceGenerator,
            "Dynamic runtime component selection is outside the product contract; use a statically imported ComponentBase + IVueComponent or a typed component-library component."),
        new(
            "P1-error-boundary",
            "ErrorBoundary with child render/update/unmount error semantics",
            RazorVueCapabilityPriority.P1,
            RazorVueCapabilityDecision.Reject,
            RazorVueCapabilityStatus.Reject,
            "RazorVueCompatibilityAnalyzer + RenderEmitter",
            "JAZORVGA021",
            "RazorSgStandardBlazorComponentRuntimeTests.ErrorBoundary_IsRejectedAsBuiltInUi",
            RazorVueCapabilityEvidence.AuthorSource |
            RazorVueCapabilityEvidence.OfficialRazorSourceGenerator,
            "Use an application-owned error boundary contract or a typed component-library component; the historical adapter is not a product entry point."),
        new(
            "P1-standard-forms",
            "EditForm, InputBase, built-in Input*, validation messages, and edit context",
            RazorVueCapabilityPriority.P1,
            RazorVueCapabilityDecision.Reject,
            RazorVueCapabilityStatus.Reject,
            "RazorVueCompatibilityAnalyzer + RenderEmitter",
            "JAZORVGA021",
            "RazorVueCompatibilityAnalyzerTests.StandardGenericComponentTag_ReportsUnsupportedBuiltInUi; RazorSgStandardBlazorComponentRuntimeTests.EditFormAndInputText_AreRejectedAsBuiltInUi",
            RazorVueCapabilityEvidence.AuthorSource |
            RazorVueCapabilityEvidence.OfficialRazorSourceGenerator,
            "Use typed TDesign/Vuetify/Element Plus form components or an application-owned ComponentBase + IVueComponent contract; EditContext/InputBase semantics are not lowered."),
        new(
            "P2-authentication",
            "AuthenticationStateProvider, AuthorizeView, and authorization-aware route composition",
            RazorVueCapabilityPriority.P2,
            RazorVueCapabilityDecision.CompatibilityAdapter,
            RazorVueCapabilityStatus.Planned,
            "Authentication state adapter + server endpoint contract",
            null,
            "JazorAdmin authenticated browser workflow",
            RazorVueCapabilityEvidence.None,
            "Browser state handoff and endpoint enforcement contract are not implemented."),
        new(
            "P2-js-runtime",
            "IJSRuntime/IJSObjectReference/IJSInProcessRuntime/JSInvokable Blazor JS interop facades",
            RazorVueCapabilityPriority.P2,
            RazorVueCapabilityDecision.Reject,
            RazorVueCapabilityStatus.Reject,
            "Jazor.Compiler usage-site validation + RazorVue final Compilation",
            "JAZORVGA022",
            "RazorSourceGeneratorBootstrapPatchTests.DriverCompletionHook_CompilerBridgeFailure_ReportsMappedAuthorDiagnostic",
            RazorVueCapabilityEvidence.AuthorSource |
            RazorVueCapabilityEvidence.OfficialRazorSourceGenerator,
            "The existing unwhitelisted-type/member boundary rejects these facades at their actual use site. Jazor already emits typed ECMAScript modules, so IJSRuntime string invocation, dynamic import, object-array marshaling, runtime registries, DotNetObjectReference, and JSInvokable are intentionally unsupported; use a typed ECMAScript/WebIDL/module binding instead."),
        new(
            "P2-ssr-state-and-forms",
            "PersistentComponentState, SupplyParameterFromForm, antiforgery, enhanced post, and hydration state",
            RazorVueCapabilityPriority.P2,
            RazorVueCapabilityDecision.CompatibilityAdapter,
            RazorVueCapabilityStatus.Planned,
            "SSR/hydration host adapter",
            null,
            "RazorVue SSR state/form fixture",
            RazorVueCapabilityEvidence.None,
            "No versioned state handoff or form-post adapter exists yet."),
        new(
            "P2-advanced-rendering",
            "Virtualize, QuickGrid, SectionOutlet/SectionContent, StreamRendering, localization, and complex validation",
            RazorVueCapabilityPriority.P2,
            RazorVueCapabilityDecision.GuidedAdaptation,
            RazorVueCapabilityStatus.Guidance,
            "Feature-specific adapter owners",
            null,
            "feature-specific feasibility fixture",
            RazorVueCapabilityEvidence.None,
            "Each feature needs a browser semantic proof before it can become an adapter."),
        new(
            "P2-parameterized-activation",
            "Primary constructors, constructor injection, this(...), and base(args) component activation",
            RazorVueCapabilityPriority.P2,
            RazorVueCapabilityDecision.Reject,
            RazorVueCapabilityStatus.Reject,
            "MemberClosureBuilder + component activation adapter",
            "JAZORVGA024",
            "MemberClosureBuilderContractTests.TryBuild_RejectsUnsupportedSourceConstructorActivationProtocols",
            RazorVueCapabilityEvidence.AuthorSource |
            RazorVueCapabilityEvidence.OfficialRazorSourceGenerator,
            "No activation/DI protocol can preserve base initialization order yet."),
        new(
            "crosscut-hmr",
            "Component HMR identity and template/logic boundary metadata",
            RazorVueCapabilityPriority.P0,
            RazorVueCapabilityDecision.DirectSupport,
            RazorVueCapabilityStatus.Support,
            "VueModuleBuilder",
            null,
            "VueHmrMetadataTests; RazorSgOfficialReleaseWorkflowRuntimeTests",
            RazorVueCapabilityEvidence.ModuleArtifact |
            RazorVueCapabilityEvidence.DenoRuntime,
            "Production browser HMR remains covered by the dev-host gate."),
        new(
            "crosscut-ssr-package",
            "Existing RazorVue package consumer, SSR rendering, and hydration delivery contract",
            RazorVueCapabilityPriority.P2,
            RazorVueCapabilityDecision.CompatibilityAdapter,
            RazorVueCapabilityStatus.InProof,
            "Jazor.Emit + ASP.NET Core host",
            null,
            "verify-windows-ssr-release.cs",
            RazorVueCapabilityEvidence.ModuleArtifact |
            RazorVueCapabilityEvidence.SsrHydration |
            RazorVueCapabilityEvidence.PackageConsumer,
            "M5 feature adapters must add their own SSR/hydration behavior proof."),
        new(
            "consumer-jazor-admin",
            "JazorAdmin pages as real P0/component-binding consumer regression",
            RazorVueCapabilityPriority.P0,
            RazorVueCapabilityDecision.DirectSupport,
            RazorVueCapabilityStatus.InProof,
            "samples/JazorAdmin",
            null,
            "samples/JazorAdmin/verify-smoke.cs",
            RazorVueCapabilityEvidence.AuthorSource |
            RazorVueCapabilityEvidence.ModuleArtifact |
            RazorVueCapabilityEvidence.BrowserSmoke,
            "Only capabilities with an independent platform proof may be promoted from this consumer evidence.")
    ];
}
