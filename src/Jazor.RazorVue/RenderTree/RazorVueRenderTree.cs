using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Jazor.RazorVue.Artifacts;

namespace Jazor.RazorVue.RenderTree;

internal sealed record RazorVueRenderFragment(
    ImmutableArray<RazorVueRenderNode> Children)
{
    public static RazorVueRenderFragment Empty { get; } =
        new([]);
}

internal abstract record RazorVueRenderNode(
    ImmutableArray<RazorVueSourceOrigin> Origins);

internal sealed record RazorVueNodeKey(
    IOperation Expression,
    ImmutableArray<RazorVueSourceOrigin> Origins);

internal sealed record RazorVueElementNode(
    string TagName,
    RazorVueNodeKey? Key,
    ImmutableArray<RazorVueAttributeEntry> Attributes,
    RazorVueRenderFragment Children,
    ImmutableArray<RazorVueSourceOrigin> Origins) : RazorVueRenderNode(Origins);

internal sealed record RazorVueComponentNode(
    string ComponentName,
    string ComponentFullName,
    string ResolutionName,
    RazorVueNodeKey? Key,
    ImmutableArray<RazorVueAttributeEntry> Attributes,
    ImmutableArray<RazorVueComponentSlotTemplateNode> SlotTemplates,
    RazorVueRenderFragment Children,
    ImmutableArray<RazorVueSourceOrigin> Origins) : RazorVueRenderNode(Origins);

internal sealed record RazorVueComponentSlotTemplateNode(
    string PublicName,
    string SlotName,
    string? ParameterName,
    IParameterSymbol? ParameterSymbol,
    RazorVueRenderFragment Children,
    ImmutableArray<RazorVueSourceOrigin> Origins);

internal sealed record RazorVueTextNode(
    string Text,
    ImmutableArray<RazorVueSourceOrigin> Origins) : RazorVueRenderNode(Origins);

internal sealed record RazorVueExpressionNode(
    IOperation Expression,
    ImmutableArray<RazorVueSourceOrigin> Origins) : RazorVueRenderNode(Origins);

internal sealed record RazorVueUnsupportedTemplateNode(
    string Message,
    ImmutableArray<RazorVueSourceOrigin> Origins) : RazorVueRenderNode(Origins);

internal sealed record RazorVueSlotOutletNode(
    string SlotName,
    IOperation? Argument,
    ImmutableArray<RazorVueSourceOrigin> Origins) : RazorVueRenderNode(Origins);

internal sealed record RazorVueConditionalNode(
    IOperation Condition,
    RazorVueRenderFragment WhenTrue,
    RazorVueRenderFragment WhenFalse,
    ImmutableArray<RazorVueSourceOrigin> Origins) : RazorVueRenderNode(Origins);

internal sealed record RazorVueForEachNode(
    string ItemName,
    ILocalSymbol? ItemSymbol,
    IOperation Source,
    RazorVueRenderFragment Body,
    ImmutableArray<RazorVueSourceOrigin> Origins) : RazorVueRenderNode(Origins);

internal sealed record RazorVueForNode(
    string VariableName,
    ILocalSymbol? VariableSymbol,
    IOperation InitialValue,
    RazorVueForConditionKind ConditionKind,
    IOperation LimitValue,
    RazorVueForStepKind StepKind,
    IOperation? StepValue,
    RazorVueRenderFragment Body,
    ImmutableArray<RazorVueSourceOrigin> Origins) : RazorVueRenderNode(Origins);

internal abstract record RazorVueAttributeEntry(
    ImmutableArray<RazorVueSourceOrigin> Origins);

internal sealed record RazorVueAttributeNode(
    string Name,
    IOperation? Value,
    ImmutableArray<RazorVueSourceOrigin> Origins) : RazorVueAttributeEntry(Origins);

internal sealed record RazorVueAttributeSpreadNode(
    IOperation Expression,
    ImmutableArray<RazorVueSourceOrigin> Origins) : RazorVueAttributeEntry(Origins);

internal enum RazorVueForConditionKind
{
    LessThan,
    LessThanOrEqual,
    GreaterThan,
    GreaterThanOrEqual
}

internal enum RazorVueForStepKind
{
    Increment,
    Decrement,
    AddAssign,
    SubtractAssign
}
