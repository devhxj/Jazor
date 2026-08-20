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
    string Blocker);

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
            "@page, @layout, route parameters, not-found, and normal loading/error/retry page workflows",
            RazorVueCapabilityPriority.P0,
            RazorVueCapabilityDecision.CompatibilityAdapter,
            RazorVueCapabilityStatus.InProof,
            "RazorVue route host",
            null,
            "RazorSourceGeneratorTailOutputTests.RouteCatalog_EmitsDeterministicPageLayoutAndQueryMappings; RazorSgStandardBlazorComponentRuntimeTests.RouterRouteViewAndLayoutView_RenderThroughStandardAdapters",
            RazorVueCapabilityEvidence.AuthorSource |
            RazorVueCapabilityEvidence.OfficialRazorSourceGenerator |
            RazorVueCapabilityEvidence.ModuleArtifact |
            RazorVueCapabilityEvidence.DenoRuntime,
            "The route catalog and standard routing adapters own @page/@layout/query mapping; page authors do not write Vue Router glue."),
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
            "NavigationManager, Router, RouteView, LayoutView, NavLink, and query/route parameter refresh",
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
            "Router route matching, page/layout composition, history refresh, and query/route parameter browser proof remain before Support."),
        new(
            "P1-standard-blazor-component-adapters",
            "DynamicComponent, EditForm, ErrorBoundary, Router, RouteView, LayoutView, NavLink, and built-in input components",
            RazorVueCapabilityPriority.P1,
            RazorVueCapabilityDecision.CompatibilityAdapter,
            RazorVueCapabilityStatus.InProof,
            "RenderEmitter + blazor-components.mjs",
            null,
            "RazorVueCompatibilityAnalyzerTests.StandardBlazorComponentTag_RemainsQuietWhenAdapterIsRegistered; RazorSgStandardBlazorComponentRuntimeTests",
            RazorVueCapabilityEvidence.AuthorSource |
            RazorVueCapabilityEvidence.OfficialRazorSourceGenerator |
            RazorVueCapabilityEvidence.ModuleArtifact |
            RazorVueCapabilityEvidence.DenoRuntime,
            "Dynamic parameter validation, EditContext/validation semantics, and browser/package proof remain before Support."),
        new(
            "P1-dynamic-component",
            "DynamicComponent with statically discoverable component type and validated parameters",
            RazorVueCapabilityPriority.P1,
            RazorVueCapabilityDecision.CompatibilityAdapter,
            RazorVueCapabilityStatus.InProof,
            "RenderEmitter + blazor-components.mjs",
            null,
            "RazorSgStandardBlazorComponentRuntimeTests.DynamicComponent_UsesStaticComponentRegistryAdapter",
            RazorVueCapabilityEvidence.AuthorSource |
            RazorVueCapabilityEvidence.OfficialRazorSourceGenerator |
            RazorVueCapabilityEvidence.ModuleArtifact |
            RazorVueCapabilityEvidence.DenoRuntime,
            "Static type-token registry is supported; parameter descriptor validation and unknown external runtime types remain before Support."),
        new(
            "P1-error-boundary",
            "ErrorBoundary with child render/update/unmount error semantics",
            RazorVueCapabilityPriority.P1,
            RazorVueCapabilityDecision.CompatibilityAdapter,
            RazorVueCapabilityStatus.InProof,
            "blazor-components.mjs ErrorBoundary adapter",
            null,
            "RazorSgStandardBlazorComponentRuntimeTests; ErrorBoundary adapter module proof",
            RazorVueCapabilityEvidence.AuthorSource |
            RazorVueCapabilityEvidence.OfficialRazorSourceGenerator |
            RazorVueCapabilityEvidence.ModuleArtifact |
            RazorVueCapabilityEvidence.DenoRuntime,
            "Initial child/error slot capture is supported; Recover API, nested update/unmount coverage and SSR/browser proof remain before Support."),
        new(
            "P1-standard-forms",
            "EditForm, InputBase, built-in Input*, validation messages, and edit context",
            RazorVueCapabilityPriority.P1,
            RazorVueCapabilityDecision.CompatibilityAdapter,
            RazorVueCapabilityStatus.InProof,
            "blazor-components.mjs form/input adapters",
            null,
            "RazorSgStandardBlazorComponentRuntimeTests.EditFormAndInputText_KeepStandardBindingSurface",
            RazorVueCapabilityEvidence.AuthorSource |
            RazorVueCapabilityEvidence.OfficialRazorSourceGenerator |
            RazorVueCapabilityEvidence.ModuleArtifact |
            RazorVueCapabilityEvidence.DenoRuntime,
            "Common text/textarea/checkbox/number/date/select binding is materialized; InputBase, EditContext validation, culture/enum parsing, server errors and package/SSR proof remain before Support."),
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
            "IJSRuntime/IJSObjectReference/JSInvokable with a typed registered module contract",
            RazorVueCapabilityPriority.P2,
            RazorVueCapabilityDecision.GuidedAdaptation,
            RazorVueCapabilityStatus.Guidance,
            "Typed WebIDL/module registry",
            null,
            "typed module registry fixture",
            RazorVueCapabilityEvidence.None,
            "Arbitrary string invocation and dynamic import remain intentionally unsupported."),
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
