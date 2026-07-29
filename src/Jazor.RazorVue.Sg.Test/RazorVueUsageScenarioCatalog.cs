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
