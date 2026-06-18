using System.Collections.Immutable;

namespace Jazor.RazorVue.RenderTree;

internal static class RazorVueOpenNodeReplayHelper
{
    public static bool ContainsScopedReplay(RazorVueRenderFragment fragment)
    {
        if (fragment.Children.IsDefaultOrEmpty)
            return false;

        foreach (var child in fragment.Children)
        {
            if (ContainsScopedReplay(child))
                return true;
        }

        return false;
    }

    public static bool ContainsScopedReplay(RazorVueRenderNode node)
        => node switch
        {
            RazorVueElementNode element => HasScopedReplayOperations(element.ReplayOperations) || ContainsScopedReplay(element.Children),
            RazorVueComponentNode component =>
                HasScopedReplayOperations(component.ReplayOperations) ||
                ContainsScopedReplay(component.Children) ||
                ContainsScopedReplay(component.AmbientDefaultSlotChildren) ||
                component.SlotTemplates.Any(static slot => ContainsScopedReplay(slot.Children)) ||
                component.ImplicitDefaultSlotAssignments.Any(static assignment => ContainsScopedReplay(assignment.Children)),
            RazorVueTemplateScopeNode templateScope => ContainsScopedReplay(templateScope.Children),
            RazorVueConditionalNode conditional => ContainsScopedReplay(conditional.WhenTrue) || ContainsScopedReplay(conditional.WhenFalse),
            RazorVueRecoveredSwitchConditionalNode conditional => ContainsScopedReplay(conditional.WhenTrue) || ContainsScopedReplay(conditional.WhenFalse),
            RazorVueForEachNode loop => ContainsScopedReplay(loop.Body),
            RazorVueForNode loop => ContainsScopedReplay(loop.Body),
            _ => false
        };

    public static bool HasScopedReplayOperations(ImmutableArray<RazorVueOpenNodeReplayOperation> operations)
    {
        if (operations.IsDefaultOrEmpty)
            return false;

        foreach (var operation in operations)
        {
            if (operation is RazorVueOpenNodeScopedReplayOperation scopedOperation)
            {
                if (!scopedOperation.CapturedBindings.IsDefaultOrEmpty || HasScopedReplayOperations(scopedOperation.Operations))
                    return true;
            }
        }

        return false;
    }

    public static bool RequiresImperativeScopedReplay(ImmutableArray<RazorVueOpenNodeReplayOperation> operations)
    {
        if (operations.IsDefaultOrEmpty)
            return false;

        foreach (var operation in operations)
        {
            if (RequiresImperativeScopedReplay(operation, isScopedReplay: false))
                return true;
        }

        return false;
    }

    private static bool RequiresImperativeScopedReplay(
        RazorVueOpenNodeReplayOperation operation,
        bool isScopedReplay)
        => operation switch
        {
            RazorVueOpenNodeAttributeReplayOperation => false,
            RazorVueOpenNodeEventModifierReplayOperation => false,
            RazorVueOpenNodeLocalDeclarationReplayOperation => true,
            RazorVueOpenNodeImperativeReplayOperation => true,
            RazorVueOpenNodeConditionalReplayOperation => true,
            RazorVueOpenNodeSwitchReplayOperation => true,
            RazorVueOpenNodeKeyReplayOperation => false,
            RazorVueOpenNodeElementReferenceReplayOperation => false,
            RazorVueOpenNodeScopedReplayOperation scopedOperation => scopedOperation.Operations.Any(static child =>
                child is RazorVueOpenNodeEventModifierReplayOperation or RazorVueOpenNodeLocalDeclarationReplayOperation or RazorVueOpenNodeImperativeReplayOperation or RazorVueOpenNodeConditionalReplayOperation or RazorVueOpenNodeSwitchReplayOperation ||
                RequiresImperativeScopedReplay(child, isScopedReplay: true)),
            _ => isScopedReplay
        };
}
