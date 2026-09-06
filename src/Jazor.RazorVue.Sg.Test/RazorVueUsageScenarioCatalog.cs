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

    /// <summary>Version of the generated CLR module contract consumed by this entry.</summary>
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
            "P0-vue-module-integrity",
            "Final generated Vue module lexical bindings, imports, declarations, and explicitly allowed ECMAScript globals",
            RazorVueCapabilityPriority.P0,
            RazorVueCapabilityDecision.DirectSupport,
            RazorVueCapabilityStatus.Support,
            "Jazor.RazorVue VueModuleBuilder",
            "JAZORVGA026",
            "VueModuleIntegrityValidatorTests; RazorSgOfficialModuleIntegrityRuntimeTests; samples/RazorVue.Authoring/verify-smoke.cs; samples/JazorAdmin/verify-smoke.cs",
            RazorVueCapabilityEvidence.AuthorSource |
            RazorVueCapabilityEvidence.OfficialRazorSourceGenerator |
            RazorVueCapabilityEvidence.ModuleArtifact,
            "The final ESTree composition rejects unknown free identifiers before serialization. Property keys, member properties, labels, binding declarations, and the narrow ECMAScript/browser global set are handled structurally; browser/package consumer smoke remains indirect through the existing Authoring and JazorAdmin gates.")
        {
            TargetProfiles = "Official Razor SG final compilation and generated Vue module artifact; browser/package evidence is inherited from existing consumer gates",
            Carrier = "Acornima ESTree lexical scope analysis",
            ImplementationPath = "VueModuleBuilder final AST composition -> VueModuleIntegrityValidator -> JAZORVGA026",
            ContributionContractVersion = "vue-module-integrity/v1",
            Dependencies = "Jazor.RazorVue direct-render framing; compiler/direct-render/import declarations",
            ExcludedSurface = "Regex .mjs scanning, broad global allow-lists, runtime fallbacks, and independent browser/package semantic claims"
        },
        new(
            "P0-tdesign-typed-authoring",
            "Native TDesign generic/non-generic components, typed form rules/validation/reset/submit, typed callbacks, attribute splats, union value branches, named slots, and required table parameters without application bridge components",
            RazorVueCapabilityPriority.P0,
            RazorVueCapabilityDecision.DirectSupport,
            RazorVueCapabilityStatus.Support,
            "ECMAScript.TDesign contract + RazorVue component binding",
            null,
            "RazorSgOfficialTDesignNaturalAuthoringRuntimeTests; RazorSgOfficialTDesignTableCellRuntimeTests; samples/RazorVue.Authoring/verify-smoke.cs (typed rules, validation, reset, and async submit); SdkIntegrationTests.Build_LocalReleasePackages_WithExternalNativeTDesignRazorConsumer_MountsAndInteractsInRealBrowser",
            RazorVueCapabilityEvidence.AuthorSource |
            RazorVueCapabilityEvidence.OfficialRazorSourceGenerator |
            RazorVueCapabilityEvidence.ModuleArtifact |
            RazorVueCapabilityEvidence.DenoRuntime |
            RazorVueCapabilityEvidence.BrowserSmoke |
            RazorVueCapabilityEvidence.PackageConsumer,
            "Natural TDesign form/control/dialog/table authoring, including typed rules, validation/reset callbacks, and async submit state, is covered through official SG, module/Deno runtime, the Authoring Release browser journey, and an isolated Release NuGet consumer mounted and interacted with in a real Edge browser. The contract excludes application bridge components and Microsoft built-in Blazor UI components.")
        {
            TargetProfiles = "Compiler authoring, Deno runtime, real Edge browser, and isolated Release package consumer",
            Carrier = "TDesign component contracts, native erased unions, and Vue named slots",
            ImplementationPath = "ECMAScript.TDesign generated Parameter/ECMAScriptName contracts -> official Razor SG -> RenderEmitter/VueModuleBuilder",
            Dependencies = "ECMAScript.TDesign resource manifest; Jazor.RazorVue direct-render pipeline",
            ExcludedSurface = "Application bridge components, object/cast escape hatches, and Microsoft built-in Blazor UI components"
        },
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
            "P1-complex-lifecycle",
            "Async lifecycle rejection/cancellation, repeated render, and async disposal/unmount races in browser; initial async lifecycle wait/preserve during SSR hydration",
            RazorVueCapabilityPriority.P1,
            RazorVueCapabilityDecision.CompatibilityAdapter,
            RazorVueCapabilityStatus.Support,
            "VueModuleBuilder setup lifecycle bridge",
            null,
            "RazorSgOfficialComplexLifecycleRuntimeTests.BuildComponent_AsyncInitializationFailureReachesNextRender; RazorSgOfficialComplexLifecycleRuntimeTests.BuildComponent_CanceledParameterLifecycleAfterUnmountDoesNotInvalidate; RazorSgOfficialComplexLifecycleRuntimeTests.BuildComponent_QueuedParameterLifecycleDoesNotStartAfterUnmount; RazorSgOfficialComplexLifecycleRuntimeTests.BuildComponent_StaleParameterLifecycleFailureStillReachesNextRender; RazorSgOfficialComplexLifecycleRuntimeTests.BuildComponent_RepeatedRenderDoesNotRepeatAfterRenderAsyncHook; RazorSgOfficialComplexLifecycleRuntimeTests.BuildComponent_AsyncLifecycleCompletionAfterAsyncUnmountIsIgnored; SdkIntegrationTests.Build_LocalReleasePackages_WithExternalComplexLifecycleRazorConsumer_ProvesAsyncRacesInRealBrowser; scripts/csharp/verify-windows-ssr-release.cs (TodoList OnInitializedAsync SSR/hydration lifecycle marker)",
            RazorVueCapabilityEvidence.AuthorSource |
            RazorVueCapabilityEvidence.OfficialRazorSourceGenerator |
            RazorVueCapabilityEvidence.ModuleArtifact |
            RazorVueCapabilityEvidence.DenoRuntime |
            RazorVueCapabilityEvidence.BrowserSmoke |
            RazorVueCapabilityEvidence.PackageConsumer |
            RazorVueCapabilityEvidence.SsrHydration,
            "Setup-local failure capture, generation checks, and disposed guards cover the browser lifecycle matrix in official SG, Deno, and an isolated Release package consumer; the Windows SSR Release consumer proves the initial OnInitializedAsync completion is awaited before server HTML and preserved through hydration. Full SSR/prerender lifecycle identity, SSR rejection/cancellation, and hydration side-effect parity remain explicitly excluded.")
        {
            TargetProfiles = "Compiler authoring, official Razor SG, module artifact, Deno runtime, real browser, isolated Release package consumer, and Windows SSR Release consumer (initial async lifecycle/hydration subset)",
            Carrier = "Setup-local lifecycle failure state plus Promise/Vue lifecycle hooks",
            ImplementationPath = "VueModuleBuilder setup framing; Promise observation with next-render failure propagation; generation/disposed guards",
            Dependencies = "Task/ValueTask Promise carriers; Vue onMounted/onUpdated/onUnmounted hooks; StateHasChanged invalidation",
            ExcludedSurface = "Full SSR/prerender lifecycle identity; SSR rejection/cancellation behavior; hydration side-effect parity"
        },
        new(
            "P0-route-layout-page-state",
            "@page, @layout, route parameters, not-found, push/replace history, LocationChanged, and normal loading/error/retry page workflows (without Microsoft built-in Router/RouteView/LayoutView/NavLink tags)",
            RazorVueCapabilityPriority.P0,
            RazorVueCapabilityDecision.CompatibilityAdapter,
            RazorVueCapabilityStatus.Support,
            "RazorVue route host",
            null,
            "RazorSourceGeneratorTailOutputTests.RouteCatalog_EmitsDeterministicPageLayoutAndQueryMappings; RazorSgNavigationRuntimeTests; samples/RazorVue.Authoring/verify-smoke.cs (isolated Release package route, history, and LocationChanged journey)",
            RazorVueCapabilityEvidence.AuthorSource |
            RazorVueCapabilityEvidence.OfficialRazorSourceGenerator |
            RazorVueCapabilityEvidence.ModuleArtifact |
            RazorVueCapabilityEvidence.DenoRuntime |
            RazorVueCapabilityEvidence.BrowserSmoke |
            RazorVueCapabilityEvidence.PackageConsumer,
            "The app-owned route catalog covers page/layout activation, typed route/query refresh, not-found, push/replace history, HistoryEntryState, and LocationChanged through an isolated Release package consumer. Microsoft Router/RouteView/LayoutView/NavLink tags and LocationChanging cancellation stay outside this P0 contract.")
        {
            TargetProfiles = "Compiler authoring, Deno runtime, real Chrome/Chromium/Edge browser, and isolated Release package consumer; SSR/prerender not claimed",
            Carrier = "Generated route catalog + application-owned Vue route host + browser NavigationManager",
            ImplementationPath = "Official Razor SG -> generated route catalog -> @jazor/vue-runtime/blazor-routing.mjs -> application-owned Vue framing",
            Dependencies = "Jazor.RazorVue route catalog lowering; Jazor.CLR NavigationManager browser adapter; Jazor.Emit Release bundle/source-map materialization",
            ExcludedSurface = "Microsoft Router/RouteView/LayoutView/NavLink tags; LocationChanging cancellation; server circuit and SSR/prerender route identity"
        },
        new(
            "P1-parameter-view",
            "SetParametersAsync(ParameterView) sparse/default/slot/queue overlay and initial async SSR hydration handoff",
            RazorVueCapabilityPriority.P1,
            RazorVueCapabilityDecision.CompatibilityAdapter,
            RazorVueCapabilityStatus.Support,
            "MemberClosureBuilder + VueModuleBuilder ParameterView adapter",
            null,
            "RazorSgSetParametersAsyncRuntimeTests (sparse, alias, slot, SetParameterProperties, queue); RazorSgBlazorReferenceOracleTests.BlazorReferenceParameterView_SetsKnownValuesAndPreservesSparseDefaults; RazorSgBlazorReferenceOracleTests.BlazorReferenceParameterView_RejectsUnknownParameterNames; MemberClosureBuilderContractTests.TryBuild_AcceptsParameterViewRuntimeEntryPoint; SdkIntegrationTests.Build_LocalReleasePackages_WithExternalFrameworkPrimitivesRazorConsumer_ProvesInjectionCascadingAndParameterViewInRealBrowser; scripts/csharp/verify-windows-ssr-release.cs (TodoList ParameterView SSR/hydration consumer)",
            RazorVueCapabilityEvidence.AuthorSource |
            RazorVueCapabilityEvidence.OfficialRazorSourceGenerator |
            RazorVueCapabilityEvidence.ModuleArtifact |
            RazorVueCapabilityEvidence.DenoRuntime |
            RazorVueCapabilityEvidence.BrowserSmoke |
            RazorVueCapabilityEvidence.PackageConsumer |
            RazorVueCapabilityEvidence.SsrHydration,
            "The adapter preserves CLR defaults before base application, sparse source-name overlay, explicit undefined, RenderFragment slots, lifecycle order, and queued updates. An isolated Release package/browser proof covers parameter replacement and lifecycle ordering, while the Windows SSR Release consumer proves an official SG component receives serialized props and completes its initial async SetParametersAsync task before server HTML and hydration. Full snapshot/reference parity, cancellation depth, and authored SSR exception coverage remain explicitly excluded."),
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
            "[Inject]/@inject writable property activation for browser-capable services with serialized SSR hydration handoff",
            RazorVueCapabilityPriority.P1,
            RazorVueCapabilityDecision.CompatibilityAdapter,
            RazorVueCapabilityStatus.Support,
            "VueInjectRegistry + VueModuleBuilder component activation adapter",
            null,
            "RazorSgInjectedServiceRuntimeTests (provider, lifecycle order, missing-provider failure); RazorSgBlazorReferenceOracleTests.BlazorReferenceInjectActivator_ResolvesPropertyAndReportsMissingProvider; SdkIntegrationTests.Build_LocalReleasePackages_WithExternalFrameworkPrimitivesRazorConsumer_ProvesInjectionCascadingAndParameterViewInRealBrowser; JazorSsrHostingTests.JazorSsrRenderer_AppliesRequestProvidersToServerComponent; scripts/csharp/verify-windows-ssr-release.cs (TodoBrowserService server provider + hydration)",
            RazorVueCapabilityEvidence.AuthorSource |
            RazorVueCapabilityEvidence.OfficialRazorSourceGenerator |
            RazorVueCapabilityEvidence.ModuleArtifact |
            RazorVueCapabilityEvidence.DenoRuntime |
            RazorVueCapabilityEvidence.BrowserSmoke |
            RazorVueCapabilityEvidence.PackageConsumer |
            RazorVueCapabilityEvidence.SsrHydration,
            "Nested and recreated components have isolated Release package/browser proof for writable property activation; the SSR runner and hydration document carry the same serialized application providers. A single explicit constructor with ordinary reference-type service parameters is also lowered through the existing typed provider key; provider lifetime/reference parity and SSR update side effects remain explicitly excluded."),
        new(
            "P1-cascading-values",
            "CascadingValue, [CascadingParameter], named/nested override, browser updates, and initial SSR hydration cascade",
            RazorVueCapabilityPriority.P1,
            RazorVueCapabilityDecision.CompatibilityAdapter,
            RazorVueCapabilityStatus.Support,
            "Cascading adapter + VueModuleBuilder lifecycle bridge",
            "JAZORVCA008",
            "RazorSgCascadingValueRuntimeTests; RazorSgBlazorReferenceOracleTests.BlazorReferenceCascadingValue_MatchesNameAndTypeAndPublishesCurrentValue; RazorVueCompatibilityAnalyzerTests.WritableCascadingParameter_IsHandledByBrowserAdapterWithoutDiagnostic; SdkIntegrationTests.Build_LocalReleasePackages_WithExternalFrameworkPrimitivesRazorConsumer_ProvesInjectionCascadingAndParameterViewInRealBrowser; scripts/csharp/verify-windows-ssr-release.cs (TodoList named cascade SSR/hydration consumer)",
            RazorVueCapabilityEvidence.AuthorSource |
            RazorVueCapabilityEvidence.OfficialRazorSourceGenerator |
            RazorVueCapabilityEvidence.ModuleArtifact |
            RazorVueCapabilityEvidence.DenoRuntime |
            RazorVueCapabilityEvidence.BrowserSmoke |
            RazorVueCapabilityEvidence.PackageConsumer |
            RazorVueCapabilityEvidence.SsrHydration,
            "Typed/named provider lookup, nested Vue scope behavior, IsFixed, same-value behavior, disposal, and update propagation have isolated Release package/browser proof; the Windows SSR Release consumer also proves a named cascade reaches the child in server HTML and remains present through hydration. Full reference parity and SSR update/hydration side-effect behavior are explicitly excluded."),
        new(
            "P1-navigation-router",
            "NavigationManager and query/route parameter refresh through the RazorVue route catalog with internal push/replace browser history and LocationChanged (Microsoft Router/RouteView/LayoutView/NavLink tags excluded)",
            RazorVueCapabilityPriority.P1,
            RazorVueCapabilityDecision.CompatibilityAdapter,
            RazorVueCapabilityStatus.Support,
            "RazorVue route catalog + blazor-routing.mjs",
            null,
            "RazorSgNavigationRuntimeTests.NavigationManager_UsesBrowserAdapterForUriAndNavigateTo; RazorSgBlazorReferenceOracleTests.BlazorReferenceNavigationManager_ReportsOptionsLocationEventsAndCancellation; RazorTailOutputTests.RouteCatalog_EmitsDeterministicPageAndLayoutEntries; samples/RazorVue.Authoring/verify-smoke.cs (PathBase push/replace, HistoryEntryState, history length, and LocationChanged); SdkIntegrationTests.Build_LocalReleasePackages_WithExternalNavigationLocationChangingRazorConsumer_ProvesInternalCancellationInRealBrowser",
            RazorVueCapabilityEvidence.AuthorSource |
            RazorVueCapabilityEvidence.OfficialRazorSourceGenerator |
            RazorVueCapabilityEvidence.ModuleArtifact |
            RazorVueCapabilityEvidence.DenoRuntime |
            RazorVueCapabilityEvidence.BrowserSmoke |
            RazorVueCapabilityEvidence.PackageConsumer,
            "The route catalog and internal NavigationManager history subset have reference, official SG, Deno, real browser, and isolated Release package evidence for typed route/query refresh, not-found, push/replace/pop history, HistoryEntryState, LocationChanged, and LocationChanging cancellation. External URI, forceLoad, popstate/hashchange cancellation, server circuit, SSR/prerender route identity, and standard Router/RouteView/LayoutView/NavLink composition remain explicitly excluded."),
        new(
            "P0-blazor-clr-mapping-package",
            "First-party Jazor.CLR generated Blazor mapping package boundary; ECMAScript.Blazor remains an optional authoring projection payload",
            RazorVueCapabilityPriority.P0,
            RazorVueCapabilityDecision.CompatibilityAdapter,
            RazorVueCapabilityStatus.Support,
            "Jazor.CLR.Generator + Jazor.CLR module/doc + Jazor.Compiler.Generator + Jazor.Vue packaging",
            null,
            "BlazorClrGeneratorOutputTests; BlazorClrWhitelistTests; BlazorClrMappingTests; ProductionRazorCompilerReferenceTests; Jazor.EmitTest.SdkIntegrationTests.Build_LocalReleasePackages_CoreAndVueConsumers_RespectBlazorClrPackageBoundary",
            RazorVueCapabilityEvidence.AuthorSource |
            RazorVueCapabilityEvidence.ModuleArtifact |
            RazorVueCapabilityEvidence.PackageConsumer,
            "The first-party static contribution, generated whitelist, and core/Vue Release package boundary are proven. A general third-party per-compilation provider protocol is explicitly excluded.")
        {
            TargetProfiles = "Compiler authoring; static Release package mapping payload",
            Carrier = "Generated Jazor.CLR adapters and generated whitelist entries",
            ImplementationPath = "Jazor.CLR generated Alias/Inline/Import/Allowed modules consumed by Jazor.Compiler.Generator; static CLR mapping ownership remains in Jazor.CLR",
            ContributionContractVersion = "generated-clr-module/v1",
            Dependencies = "Jazor.CLR module/doc source; Jazor.Compiler.Generator; Jazor.Vue lib/net11.0 authoring payload; Microsoft.AspNetCore.App generator reference",
            ExcludedSurface = "Dynamic third-party mapping discovery; duplicate provider registries; ECMAScript.Blazor mapping contribution; Blazor assets in core Jazor package"
        },
        new(
            "P1-blazor-clr-navigation-location-changing",
            "NavigationManager.RegisterLocationChangingHandler(Func<LocationChangingContext, ValueTask>), LocationChangingContext, CancellationToken, and IDisposable registration",
            RazorVueCapabilityPriority.P1,
            RazorVueCapabilityDecision.CompatibilityAdapter,
            RazorVueCapabilityStatus.Support,
            "Jazor.CLR NavigationManagerModule + RazorVue route host",
            null,
            "RazorSgNavigationRuntimeTests; RazorSgBlazorReferenceOracleTests.BlazorReferenceNavigationManager_SupersedesPendingLocationChangingDispatch; ClrRuntimeNavigationScenarios; NavigationManagerClrWhitelistTests; SdkIntegrationTests.Build_LocalReleasePackages_WithExternalNavigationLocationChangingRazorConsumer_ProvesInternalCancellationInRealBrowser",
            RazorVueCapabilityEvidence.AuthorSource |
            RazorVueCapabilityEvidence.OfficialRazorSourceGenerator |
            RazorVueCapabilityEvidence.ModuleArtifact |
            RazorVueCapabilityEvidence.DenoRuntime |
            RazorVueCapabilityEvidence.BrowserSmoke |
            RazorVueCapabilityEvidence.PackageConsumer,
            "The browser-interactive internal NavigateTo subset is covered by the Blazor reference oracle, official SG, Deno, and an isolated Release package consumer in a real HTTP-origin browser: PreventNavigation, async supersede/cancellation, query/hash, history state, and registration disposal. popstate/hashchange cancellation, server circuit identity, and SSR/prerender remain unclaimed.")
        {
            TargetProfiles = "Browser interactive; SSR/prerender not claimed",
            Carrier = "Promise/AbortSignal + module-private navigation host WeakMap",
            ImplementationPath = "Jazor.CLR generated mapping and C# Import modules; RazorVue host framing",
            ContributionContractVersion = "generated-clr-module/v1",
            Dependencies = "Task/ValueTask + CancellationToken carriers; NavigationManager route host",
            ExcludedSurface = "Router/RouteView/LayoutView/NavLink tags; popstate/hashchange cancellation; server circuit identity; SSR/prerender route identity"
        },
        new(
            "P1-blazor-clr-core-dom-events",
            "MouseEventArgs, KeyboardEventArgs, FocusEventArgs getter projections and ChangeEventArgs.Value event-time capture",
            RazorVueCapabilityPriority.P1,
            RazorVueCapabilityDecision.CompatibilityAdapter,
            RazorVueCapabilityStatus.Support,
            "Jazor.CLR event modules + Jazor.RazorVue RenderEmitter",
            null,
            "BlazorClrMappingTests; BlazorClrWhitelistTests; RazorSgOfficialCoreDomEventRuntimeTests.BlazorReferenceEventRegistry_MapsCoreDomNamesToTypedArguments; RazorSgOfficialCoreDomEventRuntimeTests.BlazorReferenceChangeEventReader_ShapesStringBooleanAndStringArrayValues; RazorSgOfficialCoreDomEventRuntimeTests.BuildComponent_OfficialRazorCoreDomTypedHandlers_ReadNativeMouseKeyboardFocusEventsOnDenoHost; RazorSgOfficialCoreDomEventRuntimeTests.BuildComponent_OfficialRazorChangeHandlers_CaptureEventTimeValueAndKeepBindDirectOnDenoHost; ClrRuntimeChangeEventArgsScenarios; SdkIntegrationTests.Build_LocalReleasePackages_WithExternalCoreDomEventsRazorConsumer_HandlesNativeEventsInRealBrowser",
            RazorVueCapabilityEvidence.AuthorSource |
            RazorVueCapabilityEvidence.OfficialRazorSourceGenerator |
            RazorVueCapabilityEvidence.ModuleArtifact |
            RazorVueCapabilityEvidence.DenoRuntime |
            RazorVueCapabilityEvidence.BrowserSmoke |
            RazorVueCapabilityEvidence.PackageConsumer,
            "Mouse/Keyboard/Focus native getter handlers, typed lambda/method-group callbacks, direct @bind coexistence, checkbox and multiple-select shaping, and event-time async capture are covered through official SG, Deno, an isolated Release NuGet consumer, and a real browser. Constructor/setter/identity and file input remain rejected.")
        {
            TargetProfiles = "Compiler authoring, Deno runtime, real browser, and isolated Release package consumer; SSR/prerender not claimed",
            Carrier = "MouseEvent/KeyboardEvent/FocusEvent; JazorEvent + WeakMap for ChangeEventArgs",
            ImplementationPath = "Jazor.CLR generated Alias/Inline/Import modules; one typed onchange capture wrapper in RenderEmitter",
            ContributionContractVersion = "generated-clr-module/v1",
            Dependencies = "WebIDL event carriers; Jazor.CLR WeakMap/Array runtime; EventCallback framing",
            ExcludedSurface = "Synthetic EventArgs construction, setters, runtime identity/type tests, InputFile/IBrowserFile"
        },
        new(
            "P1-blazor-clr-element-reference",
            "@ref ElementReference capture and ElementReferenceExtensions.FocusAsync overloads",
            RazorVueCapabilityPriority.P1,
            RazorVueCapabilityDecision.DirectSupport,
            RazorVueCapabilityStatus.Support,
            "Jazor.CLR ElementReference modules + Jazor.RazorVue VNode ref framing",
            null,
            "BlazorClrMappingTests; BlazorClrWhitelistTests; RazorSgOfficialReferenceAuthoringTests.BuildComponent_OfficialRazorElementReferenceFocus_UsesDomCarrierMapping; RazorSgOfficialReferenceAuthoringTests.BuildComponent_OfficialRazorElementReferenceFocus_PreservesMountAndUnmountFailureContractOnDenoHost; SdkIntegrationTests.Build_LocalReleasePackages_WithExternalElementReferenceRazorConsumer_FocusesAndHandlesUnmountInRealBrowser",
            RazorVueCapabilityEvidence.AuthorSource |
            RazorVueCapabilityEvidence.OfficialRazorSourceGenerator |
            RazorVueCapabilityEvidence.ModuleArtifact |
            RazorVueCapabilityEvidence.DenoRuntime |
            RazorVueCapabilityEvidence.BrowserSmoke |
            RazorVueCapabilityEvidence.PackageConsumer,
            "The @ref-captured element is focused through both FocusAsync overloads in an isolated Release NuGet consumer and a real browser; empty and unmounted refs preserve the framework failure contract. SSR/prerender is intentionally not claimed.")
        {
            TargetProfiles = "Compiler authoring, Deno runtime, real browser, and isolated Release package consumer; SSR/prerender not claimed",
            Carrier = "HTMLElement captured by Vue ref callback",
            ImplementationPath = "Jazor.CLR generated Alias(ElementReference -> HTMLElement) + Import(ElementReferenceExtensions focus helper) consumed by Jazor.RazorVue VNode ref framing",
            ContributionContractVersion = "generated-clr-module/v1",
            Dependencies = "HTMLElement/FocusOptions WebIDL; generated CLR Import resource; ValueTask/Promise carrier; Vue ref lifecycle",
            ExcludedSurface = "new ElementReference, Id/Context server identity, arbitrary DOM methods"
        },
        new(
            "P2-blazor-clr-pointer-events",
            "PointerEventArgs DOM-origin getter projection",
            RazorVueCapabilityPriority.P2,
            RazorVueCapabilityDecision.DirectSupport,
            RazorVueCapabilityStatus.Support,
            "Jazor.CLR PointerEventArgsModule + official Razor SG event framing",
            null,
            "BlazorClrMappingTests; BlazorClrWhitelistTests; BlazorClrGeneratorOutputTests; RazorSgOfficialExtendedDomEventRuntimeTests.BlazorReferenceEventRegistry_MapsExtendedDomNamesToTypedArguments; RazorSgOfficialExtendedDomEventRuntimeTests.BuildComponent_OfficialRazorPointerAndWheelHandlers_ReadNativeEventCarriersOnDenoHost; SdkIntegrationTests.Build_LocalReleasePackages_WithExternalExtendedDomEventsRazorConsumer_HandlesNativeEventsInRealBrowser",
            RazorVueCapabilityEvidence.AuthorSource |
            RazorVueCapabilityEvidence.OfficialRazorSourceGenerator |
            RazorVueCapabilityEvidence.ModuleArtifact |
            RazorVueCapabilityEvidence.DenoRuntime |
            RazorVueCapabilityEvidence.BrowserSmoke |
            RazorVueCapabilityEvidence.PackageConsumer,
            "The getter-only pointer slice is covered by Blazor EventHandlers metadata, official SG, Deno, and one isolated Release package/browser consumer shared by the seven extended event groups; synthetic construction and setters remain rejected.")
        {
            TargetProfiles = "Compiler authoring, Deno runtime, real browser, and isolated Release package consumer; SSR/prerender not claimed",
            Carrier = "PointerEvent native event object",
            ImplementationPath = "Jazor.CLR.Generator PointerEventArgs scaffold copied to Jazor.CLR; Alias(PointerEvent) + Inline native getters",
            ContributionContractVersion = "generated-clr-module/v1",
            Dependencies = "Core MouseEvent carrier; PointerEvent WebIDL Number fields; typed EventCallback framing",
            ExcludedSurface = "PointerEventArgs construction/setters and runtime identity checks"
        },
        new(
            "P2-blazor-clr-wheel-events",
            "WheelEventArgs DOM-origin getter projection",
            RazorVueCapabilityPriority.P2,
            RazorVueCapabilityDecision.DirectSupport,
            RazorVueCapabilityStatus.Support,
            "Jazor.CLR WheelEventArgsModule + official Razor SG event framing",
            null,
            "BlazorClrMappingTests; BlazorClrWhitelistTests; BlazorClrGeneratorOutputTests; RazorSgOfficialExtendedDomEventRuntimeTests.BlazorReferenceEventRegistry_MapsExtendedDomNamesToTypedArguments; RazorSgOfficialExtendedDomEventRuntimeTests.BuildComponent_OfficialRazorPointerAndWheelHandlers_ReadNativeEventCarriersOnDenoHost; SdkIntegrationTests.Build_LocalReleasePackages_WithExternalExtendedDomEventsRazorConsumer_HandlesNativeEventsInRealBrowser",
            RazorVueCapabilityEvidence.AuthorSource |
            RazorVueCapabilityEvidence.OfficialRazorSourceGenerator |
            RazorVueCapabilityEvidence.ModuleArtifact |
            RazorVueCapabilityEvidence.DenoRuntime |
            RazorVueCapabilityEvidence.BrowserSmoke |
            RazorVueCapabilityEvidence.PackageConsumer,
            "The getter-only wheel slice is covered by Blazor EventHandlers metadata, official SG, Deno, and one isolated Release package/browser consumer shared by the seven extended event groups; synthetic construction and setters remain rejected.")
        {
            TargetProfiles = "Compiler authoring, Deno runtime, real browser, and isolated Release package consumer; SSR/prerender not claimed",
            Carrier = "WheelEvent native event object",
            ImplementationPath = "Jazor.CLR.Generator WheelEventArgs scaffold copied to Jazor.CLR; Alias(WheelEvent) + Inline native getters",
            ContributionContractVersion = "generated-clr-module/v1",
            Dependencies = "Core MouseEvent carrier; WheelEvent WebIDL Number fields; typed EventCallback framing",
            ExcludedSurface = "WheelEventArgs construction/setters and runtime identity checks"
        },
        new(
            "P2-blazor-clr-drag-events",
            "DragEventArgs.DataTransfer and the stable read-only DataTransfer fields",
            RazorVueCapabilityPriority.P2,
            RazorVueCapabilityDecision.DirectSupport,
            RazorVueCapabilityStatus.Support,
            "Jazor.CLR DragEventArgs/DataTransfer modules + official Razor SG event framing",
            null,
            "BlazorClrMappingTests; BlazorClrWhitelistTests; BlazorClrGeneratorOutputTests; RazorSgOfficialExtendedDomEventRuntimeTests.BlazorReferenceEventRegistry_MapsExtendedDomNamesToTypedArguments; RazorSgOfficialExtendedDomEventRuntimeTests.BuildComponent_OfficialRazorDragAndClipboardHandlers_ReadNativeEventCarriersOnDenoHost; SdkIntegrationTests.Build_LocalReleasePackages_WithExternalExtendedDomEventsRazorConsumer_HandlesNativeEventsInRealBrowser",
            RazorVueCapabilityEvidence.AuthorSource |
            RazorVueCapabilityEvidence.OfficialRazorSourceGenerator |
            RazorVueCapabilityEvidence.ModuleArtifact |
            RazorVueCapabilityEvidence.DenoRuntime |
            RazorVueCapabilityEvidence.BrowserSmoke |
            RazorVueCapabilityEvidence.PackageConsumer,
            "The read-only drag/DataTransfer slice is covered by Blazor EventHandlers metadata, official SG, Deno, and one isolated Release package/browser consumer shared by the seven extended event groups; synthetic construction, setters, files, and item payloads remain rejected.")
        {
            TargetProfiles = "Compiler authoring, Deno runtime, real browser, and isolated Release package consumer; SSR/prerender not claimed",
            Carrier = "DragEvent and DataTransfer native browser objects",
            ImplementationPath = "Jazor.CLR.Generator DragEventArgs/DataTransfer scaffolds copied to Jazor.CLR; Alias + Inline native getters",
            ContributionContractVersion = "generated-clr-module/v1",
            Dependencies = "MouseEvent/DragEvent/DataTransfer WebIDL carriers; typed EventCallback framing",
            ExcludedSurface = "DataTransfer files/items, synthetic DragEvent/DataTransfer construction, and all mutations"
        },
        new(
            "P2-blazor-clr-clipboard-events",
            "ClipboardEventArgs.Type DOM-origin getter projection",
            RazorVueCapabilityPriority.P2,
            RazorVueCapabilityDecision.DirectSupport,
            RazorVueCapabilityStatus.Support,
            "Jazor.CLR ClipboardEventArgsModule + official Razor SG event framing",
            null,
            "BlazorClrMappingTests; BlazorClrWhitelistTests; BlazorClrGeneratorOutputTests; RazorSgOfficialExtendedDomEventRuntimeTests.BlazorReferenceEventRegistry_MapsExtendedDomNamesToTypedArguments; RazorSgOfficialExtendedDomEventRuntimeTests.BuildComponent_OfficialRazorDragAndClipboardHandlers_ReadNativeEventCarriersOnDenoHost; SdkIntegrationTests.Build_LocalReleasePackages_WithExternalExtendedDomEventsRazorConsumer_HandlesNativeEventsInRealBrowser",
            RazorVueCapabilityEvidence.AuthorSource |
            RazorVueCapabilityEvidence.OfficialRazorSourceGenerator |
            RazorVueCapabilityEvidence.ModuleArtifact |
            RazorVueCapabilityEvidence.DenoRuntime |
            RazorVueCapabilityEvidence.BrowserSmoke |
            RazorVueCapabilityEvidence.PackageConsumer,
            "The clipboard event type getter is covered by Blazor EventHandlers metadata, official SG, Deno, and one isolated Release package/browser consumer shared by the seven extended event groups; clipboard payload APIs and synthetic construction remain rejected.")
        {
            TargetProfiles = "Compiler authoring, Deno runtime, real browser, and isolated Release package consumer; SSR/prerender not claimed",
            Carrier = "ClipboardEvent native event object",
            ImplementationPath = "Jazor.CLR.Generator ClipboardEventArgs scaffold copied to Jazor.CLR; Alias(ClipboardEvent) + Inline native getter",
            ContributionContractVersion = "generated-clr-module/v1",
            Dependencies = "ClipboardEvent WebIDL carrier; typed EventCallback framing",
            ExcludedSurface = "clipboard payload permissions, arbitrary clipboard APIs, and synthetic ClipboardEventArgs construction"
        },
        new(
            "P2-blazor-clr-touch-events",
            "TouchEventArgs modifier/getter projection and TouchPoint collection access",
            RazorVueCapabilityPriority.P2,
            RazorVueCapabilityDecision.DirectSupport,
            RazorVueCapabilityStatus.Support,
            "Jazor.CLR TouchEventArgs/TouchPoint modules + official Razor SG event framing",
            null,
            "BlazorClrMappingTests; BlazorClrWhitelistTests; BlazorClrGeneratorOutputTests; RazorSgOfficialExtendedDomEventRuntimeTests.BlazorReferenceEventRegistry_MapsExtendedDomNamesToTypedArguments; RazorSgOfficialExtendedDomEventRuntimeTests.BuildComponent_OfficialRazorTouchErrorAndProgressHandlers_ReadNativeEventCarriersOnDenoHost; SdkIntegrationTests.Build_LocalReleasePackages_WithExternalExtendedDomEventsRazorConsumer_HandlesNativeEventsInRealBrowser",
            RazorVueCapabilityEvidence.AuthorSource |
            RazorVueCapabilityEvidence.OfficialRazorSourceGenerator |
            RazorVueCapabilityEvidence.ModuleArtifact |
            RazorVueCapabilityEvidence.DenoRuntime |
            RazorVueCapabilityEvidence.BrowserSmoke |
            RazorVueCapabilityEvidence.PackageConsumer,
            "The getter-only touch slice is covered by Blazor EventHandlers metadata, official SG, Deno, and one isolated Release package/browser consumer shared by the seven extended event groups; synthetic construction, setters, and non-DOM TouchList operations remain rejected.")
        {
            TargetProfiles = "Compiler authoring, Deno runtime, real browser, and isolated Release package consumer; SSR/prerender not claimed",
            Carrier = "TouchEvent/Touch native browser objects; Array.from conversion at Touches/TargetTouches/ChangedTouches property access",
            ImplementationPath = "Jazor.CLR.Generator TouchEventArgs/TouchPoint scaffolds copied to Jazor.CLR; Alias(TouchEvent/Touch) + Inline native getters and lazy Array.from",
            ContributionContractVersion = "generated-clr-module/v1",
            Dependencies = "TouchEvent/Touch WebIDL carriers; Array.from; typed EventCallback framing",
            ExcludedSurface = "TouchEventArgs/TouchPoint construction, setters, TouchList mutators, and synthetic event payloads"
        },
        new(
            "P2-blazor-clr-error-events",
            "ErrorEventArgs DOM-origin getter projection",
            RazorVueCapabilityPriority.P2,
            RazorVueCapabilityDecision.DirectSupport,
            RazorVueCapabilityStatus.Support,
            "Jazor.CLR ErrorEventArgs module + official Razor SG event framing",
            null,
            "BlazorClrMappingTests; BlazorClrWhitelistTests; BlazorClrGeneratorOutputTests; RazorSgOfficialExtendedDomEventRuntimeTests.BlazorReferenceEventRegistry_MapsExtendedDomNamesToTypedArguments; RazorSgOfficialExtendedDomEventRuntimeTests.BuildComponent_OfficialRazorTouchErrorAndProgressHandlers_ReadNativeEventCarriersOnDenoHost; SdkIntegrationTests.Build_LocalReleasePackages_WithExternalExtendedDomEventsRazorConsumer_HandlesNativeEventsInRealBrowser",
            RazorVueCapabilityEvidence.AuthorSource |
            RazorVueCapabilityEvidence.OfficialRazorSourceGenerator |
            RazorVueCapabilityEvidence.ModuleArtifact |
            RazorVueCapabilityEvidence.DenoRuntime |
            RazorVueCapabilityEvidence.BrowserSmoke |
            RazorVueCapabilityEvidence.PackageConsumer,
            "The error event getter slice is covered by Blazor EventHandlers metadata, official SG, Deno, and one isolated Release package/browser consumer shared by the seven extended event groups; synthetic construction and setters remain rejected.")
        {
            TargetProfiles = "Compiler authoring, Deno runtime, real browser, and isolated Release package consumer; SSR/prerender not claimed",
            Carrier = "ErrorEvent native browser object",
            ImplementationPath = "Jazor.CLR.Generator ErrorEventArgs scaffold copied to Jazor.CLR; Alias(ErrorEvent) + Inline native getters",
            ContributionContractVersion = "generated-clr-module/v1",
            Dependencies = "ErrorEvent WebIDL carrier; typed EventCallback framing",
            ExcludedSurface = "ErrorEventArgs construction/setters, runtime identity checks, and non-DOM error dispatch"
        },
        new(
            "P2-blazor-clr-progress-events",
            "ProgressEventArgs DOM-origin getter projection",
            RazorVueCapabilityPriority.P2,
            RazorVueCapabilityDecision.DirectSupport,
            RazorVueCapabilityStatus.Support,
            "Jazor.CLR ProgressEventArgs module + official Razor SG event framing",
            null,
            "BlazorClrMappingTests; BlazorClrWhitelistTests; BlazorClrGeneratorOutputTests; RazorSgOfficialExtendedDomEventRuntimeTests.BlazorReferenceEventRegistry_MapsExtendedDomNamesToTypedArguments; RazorSgOfficialExtendedDomEventRuntimeTests.BuildComponent_OfficialRazorTouchErrorAndProgressHandlers_ReadNativeEventCarriersOnDenoHost; SdkIntegrationTests.Build_LocalReleasePackages_WithExternalExtendedDomEventsRazorConsumer_HandlesNativeEventsInRealBrowser",
            RazorVueCapabilityEvidence.AuthorSource |
            RazorVueCapabilityEvidence.OfficialRazorSourceGenerator |
            RazorVueCapabilityEvidence.ModuleArtifact |
            RazorVueCapabilityEvidence.DenoRuntime |
            RazorVueCapabilityEvidence.BrowserSmoke |
            RazorVueCapabilityEvidence.PackageConsumer,
            "The progress event getter slice is covered by Blazor EventHandlers metadata, official SG, Deno, and one isolated Release package/browser consumer shared by the seven extended event groups; synthetic construction and setters remain rejected.")
        {
            TargetProfiles = "Compiler authoring, Deno runtime, real browser, and isolated Release package consumer; SSR/prerender not claimed",
            Carrier = "ProgressEvent native browser object",
            ImplementationPath = "Jazor.CLR.Generator ProgressEventArgs scaffold copied to Jazor.CLR; Alias(ProgressEvent) + Inline native getters",
            ContributionContractVersion = "generated-clr-module/v1",
            Dependencies = "ProgressEvent WebIDL carrier; typed EventCallback framing",
            ExcludedSurface = "ProgressEventArgs construction/setters, runtime identity checks, and upload/download orchestration"
        },
        new(
            "P1-standard-blazor-component-adapters",
            "Microsoft built-in UI components: DynamicComponent, ImportMap, EditForm, AntiforgeryToken, FormMappingScope, ErrorBoundary, Router, RouteView, LayoutView, NavLink, NavigationLock, FocusOnNavigate, PageTitle, HeadContent, HeadOutlet, AuthorizeView/AuthorizeRouteView, CascadingAuthenticationState, DataAnnotationsValidator, Input*, Virtualize, QuickGrid, and SectionContent/SectionOutlet",
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
            "P1-authentication-browser-contract",
            "Typed browser authentication snapshot/provider with versioned SSR endpoint envelope; AuthenticationStateProvider and AuthorizeView remain outside the contract",
            RazorVueCapabilityPriority.P1,
            RazorVueCapabilityDecision.GuidedAdaptation,
            RazorVueCapabilityStatus.Support,
            "Jazor.AspNetCore typed state + @jazor/vue-runtime/authentication.mjs",
            null,
            "Jazor.EmitTest.JazorSsrHostingTests.JazorSsrRenderer_AddsTypedAuthenticationAsReservedProvider; Jazor.EmitTest.JazorSsrHostingTests.JazorAuthenticationEnvelope_CreateProducesVersionedEndpointContract; RazorVueAuthenticationRuntimeTests; scripts/csharp/verify-windows-ssr-release.cs",
            RazorVueCapabilityEvidence.AuthorSource |
            RazorVueCapabilityEvidence.ModuleArtifact |
            RazorVueCapabilityEvidence.DenoRuntime |
            RazorVueCapabilityEvidence.SsrHydration,
            "The closed Anonymous/Authenticated/Expired/Forbidden snapshot and explicit endpoint provider are supported. Endpoint authorization remains the security boundary; AuthenticationStateProvider, AuthorizeView, token storage, and implicit authorization composition remain outside the contract."),
        new(
            "P2-js-runtime",
            "IJSRuntime/IJSObjectReference/IJSInProcessRuntime/JSInvokable Blazor JS interop facades",
            RazorVueCapabilityPriority.P2,
            RazorVueCapabilityDecision.Reject,
            RazorVueCapabilityStatus.Reject,
            "Jazor.Compiler usage-site validation + RazorVue final Compilation",
            "JAZORVGA022",
            "RazorVueCompatibilityAnalyzerTests.InjectedIJSRuntime_RemainsQuietUntilUsageSiteValidation; RazorSourceGeneratorBootstrapPatchTests.DriverCompletionHook_CompilerBridgeFailure_ReportsMappedAuthorDiagnostic",
            RazorVueCapabilityEvidence.AuthorSource |
            RazorVueCapabilityEvidence.OfficialRazorSourceGenerator,
            "The existing unwhitelisted-type/member boundary rejects these facades at their actual use site. Jazor already emits typed ECMAScript modules, so IJSRuntime string invocation, dynamic import, object-array marshaling, runtime registries, DotNetObjectReference, and JSInvokable are intentionally unsupported; use a typed ECMAScript/WebIDL/module binding instead."),
        new(
            "P2-ssr-state-and-forms",
            "PersistentComponentState, SupplyParameterFromForm, antiforgery, enhanced post, and hydration state",
            RazorVueCapabilityPriority.P2,
            RazorVueCapabilityDecision.GuidedAdaptation,
            RazorVueCapabilityStatus.Guidance,
            "RazorVueCompatibilityAnalyzer + explicit endpoint/bootstrap contract",
            "JAZORVCA011",
            "RazorVueCompatibilityAnalyzerTests.InjectedPersistentComponentState_ReportsExplicitSsrHandoffBoundary; RazorVueCompatibilityAnalyzerTests.PersistentStateProperty_ReportsAtAuthoredAttribute; RazorVueCompatibilityAnalyzerTests.SupplyParameterFromFormProperty_ReportsExplicitSsrHandoffBoundary; JazorSsrHostingTests.JazorSsrRenderer_RejectsDuplicateProviderKeysBeforeStartingWorker",
            RazorVueCapabilityEvidence.AuthorSource,
            "PersistentComponentState, PersistentState, and SupplyParameterFromForm are diagnosed at authored source until a versioned SSR/hydration protocol exists. Use a typed endpoint/bootstrap payload; FormName, antiforgery, enhanced post, and built-in EditForm/Input protocols remain outside this plan."),
        new(
            "P2-advanced-rendering",
            "ImportMap, AntiforgeryToken, FormMappingScope, Virtualize, QuickGrid, SectionOutlet/SectionContent and other Microsoft Blazor built-in UI components; StreamRendering, localization, and complex validation remain separate semantic questions",
            RazorVueCapabilityPriority.P2,
            RazorVueCapabilityDecision.Reject,
            RazorVueCapabilityStatus.Reject,
            "RazorVueCompatibilityAnalyzer + RenderEmitter",
            "JAZORVGA021",
            "RazorVueCompatibilityAnalyzerTests.StandardGenericComponentTag_ReportsUnsupportedBuiltInUi; RazorSgStandardBlazorComponentRuntimeTests.RemainingStandardBlazorComponents_AreRejectedAsBuiltInUi",
            RazorVueCapabilityEvidence.AuthorSource |
            RazorVueCapabilityEvidence.OfficialRazorSourceGenerator,
            "Microsoft Blazor built-in UI components are outside the RazorVue component contract. StreamRendering, localization, and complex validation are not component entries and require separate typed semantic decisions."),
        new(
            "P1-parameterized-activation",
            "Single-constructor reference-type service activation; primary constructors, overload selectors, this(...), and base(args) remain unsupported",
            RazorVueCapabilityPriority.P1,
            RazorVueCapabilityDecision.CompatibilityAdapter,
            RazorVueCapabilityStatus.Support,
            "MemberClosureBuilder + component activation adapter",
            "JAZORVGA024",
            "RazorSgInjectedServiceRuntimeTests.BuildComponent_ActivatesSingleReferenceTypeConstructorFromProvider; ComponentInitializationLowererContractTests.Build_LowersReferenceServiceConstructorThroughTypedVueInject; SdkIntegrationTests.Build_LocalReleasePackages_WithExternalFrameworkPrimitivesRazorConsumer_ProvesInjectionCascadingAndParameterViewInRealBrowser; MemberClosureBuilderContractTests.TryBuild_RejectsUnsupportedSourceConstructorActivationProtocols",
            RazorVueCapabilityEvidence.AuthorSource |
            RazorVueCapabilityEvidence.OfficialRazorSourceGenerator |
            RazorVueCapabilityEvidence.ModuleArtifact |
            RazorVueCapabilityEvidence.DenoRuntime |
            RazorVueCapabilityEvidence.BrowserSmoke |
            RazorVueCapabilityEvidence.PackageConsumer,
            "The supported slice uses one Roslyn-bound constructor and existing jazor:service:<type> providers; unsupported activation shapes remain diagnosed at member closure."),
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
            "Existing RazorVue Release package consumer, SSR rendering, and hydration delivery contract",
            RazorVueCapabilityPriority.P2,
            RazorVueCapabilityDecision.CompatibilityAdapter,
            RazorVueCapabilityStatus.Support,
            "Jazor.Emit + ASP.NET Core host",
            null,
            "verify-windows-ssr-release.cs",
            RazorVueCapabilityEvidence.AuthorSource |
            RazorVueCapabilityEvidence.OfficialRazorSourceGenerator |
            RazorVueCapabilityEvidence.ModuleArtifact |
            RazorVueCapabilityEvidence.BrowserSmoke |
            RazorVueCapabilityEvidence.SsrHydration |
            RazorVueCapabilityEvidence.PackageConsumer,
            "The Windows SSR Release consumer proves packaged DenoHost rendering, deployment-root asset closure, serialized provider/parameter handoff, and browser hydration. Feature-specific semantic claims still require their own ledger evidence.")
        {
            TargetProfiles = "Windows Release package consumer; SSR server render and browser hydration",
            Carrier = "Packaged DenoHost SSR runner + serialized provider/parameter envelope + hydration ESM graph",
            ImplementationPath = "Jazor.Emit package closure -> Jazor.AspNetCore SSR host -> DenoHost render -> browser hydration",
            ContributionContractVersion = "ssr-package-consumer/v1",
            Dependencies = "Jazor.Emit Release materialization; Jazor.AspNetCore SSR runner; Vue server renderer; selected resource manifest closure",
            ExcludedSurface = "Feature-specific SSR semantics not independently proven; authentication/claims state; PersistentComponentState; enhanced form protocol"
        },
        new(
            "consumer-jazor-admin",
            "JazorAdmin pages as real P0/component-binding consumer regression",
            RazorVueCapabilityPriority.P0,
            RazorVueCapabilityDecision.DirectSupport,
            RazorVueCapabilityStatus.Support,
            "samples/JazorAdmin",
            null,
            "samples/JazorAdmin/verify-smoke.cs (Release local package + browser mount)",
            RazorVueCapabilityEvidence.AuthorSource |
            RazorVueCapabilityEvidence.ModuleArtifact |
            RazorVueCapabilityEvidence.BrowserSmoke |
            RazorVueCapabilityEvidence.PackageConsumer,
            "Consumer delivery is proven with direct typed TDesign authoring; this row does not promote independent framework capabilities without their own semantic evidence.")
    ];
}
