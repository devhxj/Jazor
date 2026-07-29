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
