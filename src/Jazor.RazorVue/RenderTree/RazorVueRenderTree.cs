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
    ImmutableArray<RazorVueCapturedValueBinding> CapturedBindings,
    ImmutableArray<RazorVueSourceOrigin> Origins);

internal sealed record RazorVueElementNode(
    string TagName,
    RazorVueNodeKey? Key,
    ImmutableArray<RazorVueAttributeEntry> Attributes,
    RazorVueRenderFragment Children,
    ImmutableArray<RazorVueSourceOrigin> Origins) : RazorVueRenderNode(Origins)
{
    public ImmutableArray<RazorVueOpenNodeReplayOperation> ReplayOperations { get; init; } =
        ImmutableArray<RazorVueOpenNodeReplayOperation>.Empty;
}

internal sealed record RazorVueComponentNode(
    string ComponentName,
    string ComponentFullName,
    string ResolutionName,
    RazorVueNodeKey? Key,
    ImmutableArray<RazorVueAttributeEntry> Attributes,
    ImmutableArray<RazorVueComponentSlotTemplateNode> SlotTemplates,
    ImmutableArray<RazorVueImplicitDefaultSlotAssignmentNode> ImplicitDefaultSlotAssignments,
    RazorVueRenderFragment AmbientDefaultSlotChildren,
    RazorVueRenderFragment Children,
    ImmutableArray<RazorVueSourceOrigin> Origins) : RazorVueRenderNode(Origins)
{
    public ImmutableArray<RazorVueOpenNodeReplayOperation> ReplayOperations { get; init; } =
        ImmutableArray<RazorVueOpenNodeReplayOperation>.Empty;
}

internal sealed record RazorVueImplicitDefaultSlotAssignmentNode(
    RazorVueRenderFragment Children,
    ImmutableArray<RazorVueSourceOrigin> Origins);

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

internal sealed record RazorVueLocalDeclarationNode(
    ILocalSymbol LocalSymbol,
    IOperation Initializer,
    ImmutableArray<RazorVueSourceOrigin> Origins) : RazorVueRenderNode(Origins);

internal sealed record RazorVueTemplateScopeNode(
    string ScopeName,
    IParameterSymbol? ScopeParameterSymbol,
    IOperation Initializer,
    RazorVueRenderFragment Children,
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

internal sealed record RazorVueImperativeBlockNode(
    ImmutableArray<IOperation> Operations,
    RazorVueImperativeBlockKind Kind,
    ImmutableArray<ILocalSymbol> VisibleLocals,
    ImmutableArray<IParameterSymbol> VisibleParameters,
    ImmutableArray<RazorVueSourceOrigin> Origins) : RazorVueRenderNode(Origins);

internal abstract record RazorVueAttributeEntry(
    ImmutableArray<RazorVueSourceOrigin> Origins);

internal sealed record RazorVueAttributeNode(
    string Name,
    IOperation? Value,
    ImmutableArray<RazorVueCapturedValueBinding> CapturedBindings,
    ImmutableArray<RazorVueSourceOrigin> Origins) : RazorVueAttributeEntry(Origins);

internal sealed record RazorVueAttributeSpreadNode(
    IOperation Expression,
    ImmutableArray<RazorVueCapturedValueBinding> CapturedBindings,
    ImmutableArray<RazorVueSourceOrigin> Origins) : RazorVueAttributeEntry(Origins);

internal abstract record RazorVueOpenNodeReplayOperation(
    ImmutableArray<RazorVueSourceOrigin> Origins);

internal sealed record RazorVueOpenNodeAttributeReplayOperation(
    RazorVueAttributeEntry Attribute,
    ImmutableArray<RazorVueSourceOrigin> Origins) : RazorVueOpenNodeReplayOperation(Origins);

internal sealed record RazorVueOpenNodeKeyReplayOperation(
    RazorVueNodeKey? Key,
    bool KeyAssigned,
    ImmutableArray<RazorVueSourceOrigin> Origins) : RazorVueOpenNodeReplayOperation(Origins);

internal sealed record RazorVueOpenNodeSlotTemplateReplayOperation(
    RazorVueComponentSlotTemplateNode SlotTemplate,
    ImmutableArray<RazorVueSourceOrigin> Origins) : RazorVueOpenNodeReplayOperation(Origins);

internal sealed record RazorVueOpenNodeImplicitDefaultSlotAssignmentReplayOperation(
    RazorVueImplicitDefaultSlotAssignmentNode Assignment,
    ImmutableArray<RazorVueSourceOrigin> Origins) : RazorVueOpenNodeReplayOperation(Origins);

internal sealed record RazorVueOpenNodeAmbientDefaultSlotChildReplayOperation(
    RazorVueRenderNode Child,
    ImmutableArray<RazorVueSourceOrigin> Origins) : RazorVueOpenNodeReplayOperation(Origins);

internal sealed record RazorVueOpenNodeAmbientDefaultSlotFragmentReplayOperation(
    RazorVueRenderFragment Children,
    ImmutableArray<RazorVueSourceOrigin> Origins) : RazorVueOpenNodeReplayOperation(Origins);

internal sealed record RazorVueOpenNodeChildReplayOperation(
    RazorVueRenderNode Child,
    ImmutableArray<RazorVueSourceOrigin> Origins) : RazorVueOpenNodeReplayOperation(Origins);

internal sealed record RazorVueOpenNodeScopedReplayOperation(
    ImmutableArray<RazorVueCapturedValueBinding> CapturedBindings,
    ImmutableArray<RazorVueOpenNodeReplayOperation> Operations,
    ImmutableArray<RazorVueSourceOrigin> Origins) : RazorVueOpenNodeReplayOperation(Origins);

internal sealed record RazorVueCapturedValueBinding(
    IParameterSymbol ParameterSymbol,
    IOperation Initializer);

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

internal enum RazorVueImperativeBlockKind
{
    LocalBlock,
    LoopBlock,
    SwitchBlock,
    LockBlock,
    TryBlock,
    MethodBody
}
