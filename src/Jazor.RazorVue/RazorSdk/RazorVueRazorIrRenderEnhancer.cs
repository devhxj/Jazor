using System.Collections.Immutable;
using Jazor.RazorVue.Artifacts;
using Jazor.RazorVue.RenderTree;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Operations;

namespace Jazor.RazorVue.RazorSdk;

internal static class RazorVueRazorIrRenderEnhancer
{
    public static bool TryEnhance(
        RazorVueRenderFragment baselineRenderTree,
        RazorVueRenderFragment razorIrRenderTree,
        out RazorVueRenderFragment enhancedRenderTree)
    {
        if (baselineRenderTree is null)
            throw new ArgumentNullException(nameof(baselineRenderTree));
        if (razorIrRenderTree is null)
            throw new ArgumentNullException(nameof(razorIrRenderTree));

        if (!TryEnhanceFragment(baselineRenderTree, razorIrRenderTree, out enhancedRenderTree))
        {
            enhancedRenderTree = baselineRenderTree;
            return false;
        }

        return !ReferenceEquals(enhancedRenderTree, baselineRenderTree);
    }

    private static bool TryEnhanceFragment(
        RazorVueRenderFragment baseline,
        RazorVueRenderFragment razorIr,
        out RazorVueRenderFragment enhanced)
    {
        enhanced = baseline;
        if (baseline.Children.Length != razorIr.Children.Length)
            return false;

        if (baseline.Children.Length == 0)
            return true;

        var changed = false;
        var builder = ImmutableArray.CreateBuilder<RazorVueRenderNode>(baseline.Children.Length);
        for (var index = 0; index < baseline.Children.Length; index++)
        {
            if (!TryEnhanceNode(baseline.Children[index], razorIr.Children[index], out var enhancedNode))
                return false;

            changed |= !ReferenceEquals(enhancedNode, baseline.Children[index]);
            builder.Add(enhancedNode);
        }

        enhanced = changed
            ? new RazorVueRenderFragment(builder.MoveToImmutable())
            : baseline;
        return true;
    }

    private static bool TryEnhanceNode(
        RazorVueRenderNode baseline,
        RazorVueRenderNode razorIr,
        out RazorVueRenderNode enhanced)
    {
        enhanced = baseline;
        if (baseline.GetType() != razorIr.GetType())
            return false;

        return baseline switch
        {
            RazorVueElementNode baselineElement when razorIr is RazorVueElementNode razorIrElement =>
                TryEnhanceElement(baselineElement, razorIrElement, out enhanced),
            RazorVueComponentNode baselineComponent when razorIr is RazorVueComponentNode razorIrComponent =>
                TryEnhanceComponent(baselineComponent, razorIrComponent, out enhanced),
            RazorVueTextNode baselineText when razorIr is RazorVueTextNode razorIrText =>
                TryEnhanceText(baselineText, razorIrText, out enhanced),
            RazorVueExpressionNode baselineExpression when razorIr is RazorVueExpressionNode razorIrExpression =>
                TryEnhanceExpression(baselineExpression, razorIrExpression, out enhanced),
            RazorVueLocalDeclarationNode baselineLocal when razorIr is RazorVueLocalDeclarationNode razorIrLocal =>
                TryEnhanceLocalDeclaration(baselineLocal, razorIrLocal, out enhanced),
            RazorVueTemplateScopeNode baselineScope when razorIr is RazorVueTemplateScopeNode razorIrScope =>
                TryEnhanceTemplateScope(baselineScope, razorIrScope, out enhanced),
            RazorVueUnsupportedTemplateNode baselineUnsupported when razorIr is RazorVueUnsupportedTemplateNode razorIrUnsupported =>
                TryEnhanceUnsupported(baselineUnsupported, razorIrUnsupported, out enhanced),
            RazorVueSlotOutletNode baselineSlot when razorIr is RazorVueSlotOutletNode razorIrSlot =>
                TryEnhanceSlotOutlet(baselineSlot, razorIrSlot, out enhanced),
            RazorVueConditionalNode baselineConditional when razorIr is RazorVueConditionalNode razorIrConditional =>
                TryEnhanceConditional(baselineConditional, razorIrConditional, out enhanced),
            RazorVueForEachNode baselineForEach when razorIr is RazorVueForEachNode razorIrForEach =>
                TryEnhanceForEach(baselineForEach, razorIrForEach, out enhanced),
            RazorVueForNode baselineFor when razorIr is RazorVueForNode razorIrFor =>
                TryEnhanceFor(baselineFor, razorIrFor, out enhanced),
            RazorVueImperativeBlockNode baselineImperative when razorIr is RazorVueImperativeBlockNode razorIrImperative =>
                TryEnhanceImperative(baselineImperative, razorIrImperative, out enhanced),
            _ => false
        };
    }

    private static bool TryEnhanceElement(
        RazorVueElementNode baseline,
        RazorVueElementNode razorIr,
        out RazorVueRenderNode enhanced)
    {
        enhanced = baseline;
        if (!string.Equals(baseline.TagName, razorIr.TagName, StringComparison.Ordinal) ||
            !TryEnhanceKey(baseline.Key, razorIr.Key, out var enhancedKey) ||
            !TryEnhanceAttributes(baseline.Attributes, razorIr.Attributes, out var enhancedAttributes) ||
            !TryEnhanceFragment(baseline.Children, razorIr.Children, out var enhancedChildren))
        {
            return false;
        }

        var origins = SelectOrigins(baseline.Origins, razorIr.Origins);
        enhanced = ReferenceEquals(enhancedChildren, baseline.Children) &&
                   ReferenceEquals(enhancedKey, baseline.Key) &&
                   enhancedAttributes.Equals(baseline.Attributes) &&
                   origins.Equals(baseline.Origins)
            ? baseline
            : baseline with
            {
                Key = enhancedKey,
                Attributes = enhancedAttributes,
                Children = enhancedChildren,
                Origins = origins
            };
        return true;
    }

    private static bool TryEnhanceComponent(
        RazorVueComponentNode baseline,
        RazorVueComponentNode razorIr,
        out RazorVueRenderNode enhanced)
    {
        enhanced = baseline;
        if (!string.Equals(baseline.ComponentName, razorIr.ComponentName, StringComparison.Ordinal) ||
            !string.Equals(baseline.ComponentFullName, razorIr.ComponentFullName, StringComparison.Ordinal) ||
            !ComponentResolutionNamesMatch(baseline, razorIr) ||
            !TryEnhanceKey(baseline.Key, razorIr.Key, out var enhancedKey) ||
            !TryEnhanceAttributes(baseline.Attributes, razorIr.Attributes, out var enhancedAttributes) ||
            !TryEnhanceSlotTemplates(baseline.SlotTemplates, razorIr.SlotTemplates, out var enhancedSlotTemplates) ||
            !TryEnhanceImplicitDefaultSlotAssignments(
                baseline.ImplicitDefaultSlotAssignments,
                razorIr.ImplicitDefaultSlotAssignments,
                out var enhancedImplicitDefaultSlotAssignments) ||
            !TryEnhanceFragment(baseline.AmbientDefaultSlotChildren, razorIr.AmbientDefaultSlotChildren, out var enhancedAmbientDefaultSlotChildren) ||
            !TryEnhanceFragment(baseline.Children, razorIr.Children, out var enhancedChildren))
        {
            return false;
        }

        var origins = SelectOrigins(baseline.Origins, razorIr.Origins);
        enhanced = ReferenceEquals(enhancedKey, baseline.Key) &&
                   enhancedAttributes.Equals(baseline.Attributes) &&
                   enhancedSlotTemplates.Equals(baseline.SlotTemplates) &&
                   enhancedImplicitDefaultSlotAssignments.Equals(baseline.ImplicitDefaultSlotAssignments) &&
                   ReferenceEquals(enhancedAmbientDefaultSlotChildren, baseline.AmbientDefaultSlotChildren) &&
                   ReferenceEquals(enhancedChildren, baseline.Children) &&
                   origins.Equals(baseline.Origins)
            ? baseline
            : baseline with
            {
                Key = enhancedKey,
                Attributes = enhancedAttributes,
                SlotTemplates = enhancedSlotTemplates,
                ImplicitDefaultSlotAssignments = enhancedImplicitDefaultSlotAssignments,
                AmbientDefaultSlotChildren = enhancedAmbientDefaultSlotChildren,
                Children = enhancedChildren,
                Origins = origins
        };
        return true;
    }

    private static bool ComponentResolutionNamesMatch(
        RazorVueComponentNode baseline,
        RazorVueComponentNode razorIr)
    {
        if (string.Equals(baseline.ResolutionName, razorIr.ResolutionName, StringComparison.Ordinal))
            return true;

        var baselineResolutionName = NormalizeResolutionName(baseline.ResolutionName);
        var razorIrResolutionName = NormalizeResolutionName(razorIr.ResolutionName);
        var componentFullName = NormalizeResolutionName(baseline.ComponentFullName);
        return string.Equals(baselineResolutionName, componentFullName, StringComparison.Ordinal) &&
               string.Equals(razorIrResolutionName, baseline.ComponentName, StringComparison.Ordinal);
    }

    private static string NormalizeResolutionName(string value)
        => value.StartsWith("global::", StringComparison.Ordinal)
            ? value.Substring("global::".Length)
            : value;

    private static bool TryEnhanceText(
        RazorVueTextNode baseline,
        RazorVueTextNode razorIr,
        out RazorVueRenderNode enhanced)
    {
        enhanced = baseline;
        if (!string.Equals(baseline.Text, razorIr.Text, StringComparison.Ordinal))
            return false;

        var origins = SelectOrigins(baseline.Origins, razorIr.Origins);
        enhanced = origins.Equals(baseline.Origins) ? baseline : baseline with { Origins = origins };
        return true;
    }

    private static bool TryEnhanceExpression(
        RazorVueExpressionNode baseline,
        RazorVueExpressionNode razorIr,
        out RazorVueRenderNode enhanced)
    {
        enhanced = baseline;
        if (!OperationsMatch(baseline.Expression, razorIr.Expression))
            return false;

        var origins = SelectOrigins(baseline.Origins, razorIr.Origins);
        enhanced = origins.Equals(baseline.Origins) ? baseline : baseline with { Origins = origins };
        return true;
    }

    private static bool TryEnhanceLocalDeclaration(
        RazorVueLocalDeclarationNode baseline,
        RazorVueLocalDeclarationNode razorIr,
        out RazorVueRenderNode enhanced)
    {
        enhanced = baseline;
        if (!SymbolsMatch(baseline.LocalSymbol, razorIr.LocalSymbol) ||
            !OperationsMatch(baseline.Initializer, razorIr.Initializer))
        {
            return false;
        }

        var origins = SelectOrigins(baseline.Origins, razorIr.Origins);
        enhanced = origins.Equals(baseline.Origins) ? baseline : baseline with { Origins = origins };
        return true;
    }

    private static bool TryEnhanceTemplateScope(
        RazorVueTemplateScopeNode baseline,
        RazorVueTemplateScopeNode razorIr,
        out RazorVueRenderNode enhanced)
    {
        enhanced = baseline;
        if (!string.Equals(baseline.ScopeName, razorIr.ScopeName, StringComparison.Ordinal) ||
            !SymbolsMatch(baseline.ScopeParameterSymbol, razorIr.ScopeParameterSymbol) ||
            !OperationsMatch(baseline.Initializer, razorIr.Initializer) ||
            !TryEnhanceFragment(baseline.Children, razorIr.Children, out var enhancedChildren))
        {
            return false;
        }

        var origins = SelectOrigins(baseline.Origins, razorIr.Origins);
        enhanced = ReferenceEquals(enhancedChildren, baseline.Children) && origins.Equals(baseline.Origins)
            ? baseline
            : baseline with
            {
                Children = enhancedChildren,
                Origins = origins
            };
        return true;
    }

    private static bool TryEnhanceUnsupported(
        RazorVueUnsupportedTemplateNode baseline,
        RazorVueUnsupportedTemplateNode razorIr,
        out RazorVueRenderNode enhanced)
    {
        enhanced = baseline;
        if (!string.Equals(baseline.Message, razorIr.Message, StringComparison.Ordinal))
            return false;

        var origins = SelectOrigins(baseline.Origins, razorIr.Origins);
        enhanced = origins.Equals(baseline.Origins) ? baseline : baseline with { Origins = origins };
        return true;
    }

    private static bool TryEnhanceSlotOutlet(
        RazorVueSlotOutletNode baseline,
        RazorVueSlotOutletNode razorIr,
        out RazorVueRenderNode enhanced)
    {
        enhanced = baseline;
        if (!string.Equals(baseline.SlotName, razorIr.SlotName, StringComparison.Ordinal) ||
            !OperationsMatch(baseline.Argument, razorIr.Argument))
        {
            return false;
        }

        var origins = SelectOrigins(baseline.Origins, razorIr.Origins);
        enhanced = origins.Equals(baseline.Origins) ? baseline : baseline with { Origins = origins };
        return true;
    }

    private static bool TryEnhanceConditional(
        RazorVueConditionalNode baseline,
        RazorVueConditionalNode razorIr,
        out RazorVueRenderNode enhanced)
    {
        enhanced = baseline;
        if (!OperationsMatch(baseline.Condition, razorIr.Condition) ||
            !TryEnhanceFragment(baseline.WhenTrue, razorIr.WhenTrue, out var enhancedWhenTrue) ||
            !TryEnhanceFragment(baseline.WhenFalse, razorIr.WhenFalse, out var enhancedWhenFalse))
        {
            return false;
        }

        var origins = SelectOrigins(baseline.Origins, razorIr.Origins);
        enhanced = ReferenceEquals(enhancedWhenTrue, baseline.WhenTrue) &&
                   ReferenceEquals(enhancedWhenFalse, baseline.WhenFalse) &&
                   origins.Equals(baseline.Origins)
            ? baseline
            : baseline with
            {
                WhenTrue = enhancedWhenTrue,
                WhenFalse = enhancedWhenFalse,
                Origins = origins
            };
        return true;
    }

    private static bool TryEnhanceForEach(
        RazorVueForEachNode baseline,
        RazorVueForEachNode razorIr,
        out RazorVueRenderNode enhanced)
    {
        enhanced = baseline;
        if (!string.Equals(baseline.ItemName, razorIr.ItemName, StringComparison.Ordinal) ||
            !SymbolsMatch(baseline.ItemSymbol, razorIr.ItemSymbol) ||
            !OperationsMatch(baseline.Source, razorIr.Source) ||
            !TryEnhanceFragment(baseline.Body, razorIr.Body, out var enhancedBody))
        {
            return false;
        }

        var origins = SelectOrigins(baseline.Origins, razorIr.Origins);
        enhanced = ReferenceEquals(enhancedBody, baseline.Body) && origins.Equals(baseline.Origins)
            ? baseline
            : baseline with
            {
                Body = enhancedBody,
                Origins = origins
            };
        return true;
    }

    private static bool TryEnhanceFor(
        RazorVueForNode baseline,
        RazorVueForNode razorIr,
        out RazorVueRenderNode enhanced)
    {
        enhanced = baseline;
        if (!string.Equals(baseline.VariableName, razorIr.VariableName, StringComparison.Ordinal) ||
            !SymbolsMatch(baseline.VariableSymbol, razorIr.VariableSymbol) ||
            !OperationsMatch(baseline.InitialValue, razorIr.InitialValue) ||
            baseline.ConditionKind != razorIr.ConditionKind ||
            !OperationsMatch(baseline.LimitValue, razorIr.LimitValue) ||
            baseline.StepKind != razorIr.StepKind ||
            !OperationsMatch(baseline.StepValue, razorIr.StepValue) ||
            !TryEnhanceFragment(baseline.Body, razorIr.Body, out var enhancedBody))
        {
            return false;
        }

        var origins = SelectOrigins(baseline.Origins, razorIr.Origins);
        enhanced = ReferenceEquals(enhancedBody, baseline.Body) && origins.Equals(baseline.Origins)
            ? baseline
            : baseline with
            {
                Body = enhancedBody,
                Origins = origins
            };
        return true;
    }

    private static bool TryEnhanceImperative(
        RazorVueImperativeBlockNode baseline,
        RazorVueImperativeBlockNode razorIr,
        out RazorVueRenderNode enhanced)
    {
        enhanced = baseline;
        if (baseline.Kind != razorIr.Kind ||
            !OperationsMatch(baseline.Operations, razorIr.Operations) ||
            !SymbolsMatch(baseline.VisibleLocals, razorIr.VisibleLocals) ||
            !SymbolsMatch(baseline.VisibleParameters, razorIr.VisibleParameters))
        {
            return false;
        }

        var origins = SelectOrigins(baseline.Origins, razorIr.Origins);
        enhanced = origins.Equals(baseline.Origins) ? baseline : baseline with { Origins = origins };
        return true;
    }

    private static bool TryEnhanceKey(
        RazorVueNodeKey? baseline,
        RazorVueNodeKey? razorIr,
        out RazorVueNodeKey? enhanced)
    {
        enhanced = baseline;
        if (baseline is null || razorIr is null)
            return baseline is null && razorIr is null;

        if (!TryEnhanceKeyOperation(baseline.Expression, razorIr.Expression, out var enhancedExpression) ||
            !CapturedBindingsMatch(baseline.CapturedBindings, razorIr.CapturedBindings))
        {
            return false;
        }

        var origins = SelectOrigins(baseline.Origins, razorIr.Origins);
        enhanced = ReferenceEquals(enhancedExpression, baseline.Expression) && origins.Equals(baseline.Origins)
            ? baseline
            : baseline with
            {
                Expression = enhancedExpression,
                Origins = origins
            };
        return true;
    }

    private static bool TryEnhanceKeyOperation(
        IOperation baseline,
        IOperation razorIr,
        out IOperation enhanced)
    {
        enhanced = baseline;
        if (OperationsMatch(baseline, razorIr))
            return true;

        if (IsStringLiteralOperation(baseline))
        {
            enhanced = razorIr;
            return true;
        }

        return false;
    }

    private static bool TryEnhanceAttributes(
        ImmutableArray<RazorVueAttributeEntry> baseline,
        ImmutableArray<RazorVueAttributeEntry> razorIr,
        out ImmutableArray<RazorVueAttributeEntry> enhanced)
    {
        enhanced = baseline;
        if (baseline.Length != razorIr.Length)
            return false;

        if (baseline.Length == 0)
            return true;

        var changed = false;
        var builder = ImmutableArray.CreateBuilder<RazorVueAttributeEntry>(baseline.Length);
        for (var index = 0; index < baseline.Length; index++)
        {
            if (!TryEnhanceAttribute(baseline[index], razorIr[index], out var enhancedAttribute))
                return false;

            changed |= !ReferenceEquals(enhancedAttribute, baseline[index]);
            builder.Add(enhancedAttribute);
        }

        enhanced = changed ? builder.MoveToImmutable() : baseline;
        return true;
    }

    private static bool TryEnhanceAttribute(
        RazorVueAttributeEntry baseline,
        RazorVueAttributeEntry razorIr,
        out RazorVueAttributeEntry enhanced)
    {
        enhanced = baseline;
        if (baseline.GetType() != razorIr.GetType())
            return false;

        switch (baseline)
        {
            case RazorVueAttributeNode baselineAttribute when razorIr is RazorVueAttributeNode razorIrAttribute:
            {
                if (!string.Equals(baselineAttribute.Name, razorIrAttribute.Name, StringComparison.Ordinal) ||
                    !OperationsMatch(baselineAttribute.Value, razorIrAttribute.Value) ||
                    !CapturedBindingsMatch(baselineAttribute.CapturedBindings, razorIrAttribute.CapturedBindings) ||
                    !EventModifiersMatch(baselineAttribute.EventModifiers, razorIrAttribute.EventModifiers, out var enhancedModifiers))
                {
                    return false;
                }

                var origins = SelectOrigins(baselineAttribute.Origins, razorIrAttribute.Origins);
                enhanced = ReferenceEquals(enhancedModifiers, baselineAttribute.EventModifiers) && origins.Equals(baselineAttribute.Origins)
                    ? baselineAttribute
                    : baselineAttribute with
                    {
                        Origins = origins,
                        EventModifiers = enhancedModifiers
                    };
                return true;
            }
            case RazorVueAttributeSpreadNode baselineSpread when razorIr is RazorVueAttributeSpreadNode razorIrSpread:
            {
                if (!OperationsMatch(baselineSpread.Expression, razorIrSpread.Expression) ||
                    !CapturedBindingsMatch(baselineSpread.CapturedBindings, razorIrSpread.CapturedBindings))
                {
                    return false;
                }

                var origins = SelectOrigins(baselineSpread.Origins, razorIrSpread.Origins);
                enhanced = origins.Equals(baselineSpread.Origins) ? baselineSpread : baselineSpread with { Origins = origins };
                return true;
            }
            default:
                return false;
        }
    }

    private static bool TryEnhanceSlotTemplates(
        ImmutableArray<RazorVueComponentSlotTemplateNode> baseline,
        ImmutableArray<RazorVueComponentSlotTemplateNode> razorIr,
        out ImmutableArray<RazorVueComponentSlotTemplateNode> enhanced)
    {
        enhanced = baseline;
        if (baseline.Length != razorIr.Length)
            return false;

        if (baseline.Length == 0)
            return true;

        var changed = false;
        var builder = ImmutableArray.CreateBuilder<RazorVueComponentSlotTemplateNode>(baseline.Length);
        for (var index = 0; index < baseline.Length; index++)
        {
            var baselineSlot = baseline[index];
            var razorIrSlot = razorIr[index];
            if (!string.Equals(baselineSlot.PublicName, razorIrSlot.PublicName, StringComparison.Ordinal) ||
                !string.Equals(baselineSlot.SlotName, razorIrSlot.SlotName, StringComparison.Ordinal) ||
                !string.Equals(baselineSlot.ParameterName, razorIrSlot.ParameterName, StringComparison.Ordinal) ||
                !SymbolsMatch(baselineSlot.ParameterSymbol, razorIrSlot.ParameterSymbol) ||
                !TryEnhanceFragment(baselineSlot.Children, razorIrSlot.Children, out var enhancedChildren))
            {
                return false;
            }

            var origins = SelectOrigins(baselineSlot.Origins, razorIrSlot.Origins);
            var enhancedSlot = ReferenceEquals(enhancedChildren, baselineSlot.Children) && origins.Equals(baselineSlot.Origins)
                ? baselineSlot
                : baselineSlot with
                {
                    Children = enhancedChildren,
                    Origins = origins
                };
            changed |= !ReferenceEquals(enhancedSlot, baselineSlot);
            builder.Add(enhancedSlot);
        }

        enhanced = changed ? builder.MoveToImmutable() : baseline;
        return true;
    }

    private static bool TryEnhanceImplicitDefaultSlotAssignments(
        ImmutableArray<RazorVueImplicitDefaultSlotAssignmentNode> baseline,
        ImmutableArray<RazorVueImplicitDefaultSlotAssignmentNode> razorIr,
        out ImmutableArray<RazorVueImplicitDefaultSlotAssignmentNode> enhanced)
    {
        enhanced = baseline;
        if (baseline.Length != razorIr.Length)
            return false;

        if (baseline.Length == 0)
            return true;

        var changed = false;
        var builder = ImmutableArray.CreateBuilder<RazorVueImplicitDefaultSlotAssignmentNode>(baseline.Length);
        for (var index = 0; index < baseline.Length; index++)
        {
            var baselineAssignment = baseline[index];
            var razorIrAssignment = razorIr[index];
            if (!TryEnhanceFragment(baselineAssignment.Children, razorIrAssignment.Children, out var enhancedChildren))
                return false;

            var origins = SelectOrigins(baselineAssignment.Origins, razorIrAssignment.Origins);
            var enhancedAssignment = ReferenceEquals(enhancedChildren, baselineAssignment.Children) && origins.Equals(baselineAssignment.Origins)
                ? baselineAssignment
                : baselineAssignment with
                {
                    Children = enhancedChildren,
                    Origins = origins
                };
            changed |= !ReferenceEquals(enhancedAssignment, baselineAssignment);
            builder.Add(enhancedAssignment);
        }

        enhanced = changed ? builder.MoveToImmutable() : baseline;
        return true;
    }

    private static bool EventModifiersMatch(
        RazorVueEventModifiers baseline,
        RazorVueEventModifiers razorIr,
        out RazorVueEventModifiers enhanced)
    {
        enhanced = baseline;
        if (!EventModifierBindingMatches(baseline.PreventDefault, razorIr.PreventDefault, out var enhancedPreventDefault) ||
            !EventModifierBindingMatches(baseline.StopPropagation, razorIr.StopPropagation, out var enhancedStopPropagation))
        {
            return false;
        }

        enhanced = ReferenceEquals(enhancedPreventDefault, baseline.PreventDefault) &&
                   ReferenceEquals(enhancedStopPropagation, baseline.StopPropagation)
            ? baseline
            : baseline with
            {
                PreventDefault = enhancedPreventDefault,
                StopPropagation = enhancedStopPropagation
            };
        return true;
    }

    private static bool EventModifierBindingMatches(
        RazorVueEventModifierBinding? baseline,
        RazorVueEventModifierBinding? razorIr,
        out RazorVueEventModifierBinding? enhanced)
    {
        enhanced = baseline;
        if (baseline is null || razorIr is null)
            return baseline is null && razorIr is null;

        if (!OperationsMatch(baseline.Value, razorIr.Value) ||
            !CapturedBindingsMatch(baseline.CapturedBindings, razorIr.CapturedBindings))
        {
            return false;
        }

        var origins = SelectOrigins(baseline.Origins, razorIr.Origins);
        enhanced = origins.Equals(baseline.Origins) ? baseline : baseline with { Origins = origins };
        return true;
    }

    private static bool CapturedBindingsMatch(
        ImmutableArray<RazorVueCapturedValueBinding> baseline,
        ImmutableArray<RazorVueCapturedValueBinding> razorIr)
    {
        if (baseline.Length != razorIr.Length)
            return false;

        for (var index = 0; index < baseline.Length; index++)
        {
            if (!SymbolsMatch(baseline[index].ParameterSymbol, razorIr[index].ParameterSymbol) ||
                !OperationsMatch(baseline[index].Initializer, razorIr[index].Initializer))
            {
                return false;
            }
        }

        return true;
    }

    private static bool OperationsMatch(ImmutableArray<IOperation> baseline, ImmutableArray<IOperation> razorIr)
    {
        if (baseline.Length != razorIr.Length)
            return false;

        for (var index = 0; index < baseline.Length; index++)
        {
            if (!OperationsMatch(baseline[index], razorIr[index]))
                return false;
        }

        return true;
    }

    private static bool OperationsMatch(IOperation? baseline, IOperation? razorIr)
    {
        if (baseline is null || razorIr is null)
            return baseline is null && razorIr is null;

        if (baseline.Kind != razorIr.Kind)
            return false;

        if (!SymbolEqualityComparer.Default.Equals(baseline.Type, razorIr.Type))
            return false;

        return string.Equals(
            NormalizeOperationSyntax(baseline),
            NormalizeOperationSyntax(razorIr),
            StringComparison.Ordinal);
    }

    private static bool IsStringLiteralOperation(IOperation operation)
        => operation.ConstantValue.HasValue &&
           operation.ConstantValue.Value is string;

    private static string NormalizeOperationSyntax(IOperation operation)
    {
        var syntaxText = operation.Syntax?.ToString() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(syntaxText))
            return string.Empty;

        return new string(syntaxText.Where(static character => !char.IsWhiteSpace(character)).ToArray());
    }

    private static bool SymbolsMatch(ImmutableArray<ILocalSymbol> baseline, ImmutableArray<ILocalSymbol> razorIr)
    {
        if (baseline.Length != razorIr.Length)
            return false;

        for (var index = 0; index < baseline.Length; index++)
        {
            if (!SymbolsMatch(baseline[index], razorIr[index]))
                return false;
        }

        return true;
    }

    private static bool SymbolsMatch(ImmutableArray<IParameterSymbol> baseline, ImmutableArray<IParameterSymbol> razorIr)
    {
        if (baseline.Length != razorIr.Length)
            return false;

        for (var index = 0; index < baseline.Length; index++)
        {
            if (!SymbolsMatch(baseline[index], razorIr[index]))
                return false;
        }

        return true;
    }

    private static bool SymbolsMatch(ISymbol? baseline, ISymbol? razorIr)
        => baseline is null || razorIr is null
            ? baseline is null && razorIr is null
            : SymbolEqualityComparer.Default.Equals(baseline, razorIr) ||
              (string.Equals(baseline.Name, razorIr.Name, StringComparison.Ordinal) &&
               SymbolEqualityComparer.Default.Equals(GetSymbolType(baseline), GetSymbolType(razorIr)));

    private static ITypeSymbol? GetSymbolType(ISymbol symbol)
        => symbol switch
        {
            ILocalSymbol local => local.Type,
            IParameterSymbol parameter => parameter.Type,
            IFieldSymbol field => field.Type,
            IPropertySymbol property => property.Type,
            IMethodSymbol method => method.ReturnType,
            _ => null
        };

    private static ImmutableArray<RazorVueSourceOrigin> SelectOrigins(
        ImmutableArray<RazorVueSourceOrigin> baseline,
        ImmutableArray<RazorVueSourceOrigin> razorIr)
    {
        if (razorIr.IsDefaultOrEmpty)
            return baseline;

        if (baseline.IsDefaultOrEmpty)
            return razorIr;

        return HasBetterOrigins(razorIr, baseline) ? razorIr : baseline;
    }

    private static bool HasBetterOrigins(
        ImmutableArray<RazorVueSourceOrigin> candidate,
        ImmutableArray<RazorVueSourceOrigin> current)
    {
        var candidateBest = candidate.Min(static origin => GetMappingQualityRank(origin.MappingQuality));
        var currentBest = current.Min(static origin => GetMappingQualityRank(origin.MappingQuality));
        return candidateBest < currentBest;
    }

    private static int GetMappingQualityRank(RazorVueMappingQuality quality)
        => quality switch
        {
            RazorVueMappingQuality.ExactSource => 0,
            RazorVueMappingQuality.MappedFromGenerated => 1,
            RazorVueMappingQuality.GeneratedOnly => 2,
            _ => 3
        };
}
