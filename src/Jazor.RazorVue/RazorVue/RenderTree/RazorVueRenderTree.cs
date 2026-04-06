using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Jazor.RazorVue.Artifacts;

namespace Jazor.RazorVue.RenderTree;

internal sealed record RazorVueRenderFragment(
    ImmutableArray<RazorVueRenderNode> Children)
{
    public static RazorVueRenderFragment Empty { get; } =
        new(ImmutableArray<RazorVueRenderNode>.Empty);
}

internal abstract record RazorVueRenderNode(
    ImmutableArray<RazorVueSourceOrigin> Origins);

internal sealed record RazorVueElementNode(
    string TagName,
    ImmutableArray<RazorVueAttributeNode> Attributes,
    RazorVueRenderFragment Children,
    ImmutableArray<RazorVueSourceOrigin> Origins) : RazorVueRenderNode(Origins);

internal sealed record RazorVueComponentNode(
    string ComponentName,
    string ComponentFullName,
    string ResolutionName,
    ImmutableArray<RazorVueAttributeNode> Attributes,
    RazorVueRenderFragment Children,
    ImmutableArray<RazorVueSourceOrigin> Origins) : RazorVueRenderNode(Origins);

internal sealed record RazorVueTextNode(
    string Text,
    ImmutableArray<RazorVueSourceOrigin> Origins) : RazorVueRenderNode(Origins);

internal sealed record RazorVueExpressionNode(
    IOperation Expression,
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
    IOperation Source,
    RazorVueRenderFragment Body,
    ImmutableArray<RazorVueSourceOrigin> Origins) : RazorVueRenderNode(Origins);

internal sealed record RazorVueAttributeNode(
    string Name,
    IOperation? Value,
    ImmutableArray<RazorVueSourceOrigin> Origins);

