using System.Collections.Immutable;
using System.Collections.Generic;
using System.Linq;
using Jazor.RazorVue;
using Jazor.RazorVue.Artifacts;
using Jazor.RazorVue.Descriptor;
using Jazor.RazorVue.RenderTree;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Operations;

namespace Jazor.RazorVue.Lowering;

internal sealed partial class RazorVueExpressionEmitter
{
    private static readonly ImmutableHashSet<ILocalSymbol> EmptyLocalScope =
        ImmutableHashSet<ILocalSymbol>.Empty.WithComparer(SymbolEqualityComparer.Default);
    private static readonly ImmutableHashSet<IParameterSymbol> EmptyParameterScope =
        ImmutableHashSet<IParameterSymbol>.Empty.WithComparer(SymbolEqualityComparer.Default);

    private string EmitNode(RazorVueRenderNode node)
        => EmitNode(node, EmptyLocalScope, EmptyParameterScope);

    private string EmitNode(
        RazorVueRenderNode node,
        ImmutableHashSet<ILocalSymbol> allowedLocalSymbols,
        ImmutableHashSet<IParameterSymbol> allowedParameterSymbols)
        => node switch
        {
            RazorVueElementNode element => EmitElementNode(element, allowedLocalSymbols, allowedParameterSymbols),
            RazorVueComponentNode component => EmitComponentNode(component, allowedLocalSymbols, allowedParameterSymbols),
            RazorVueTextNode text => ToJavaScriptString(text.Text),
            RazorVueExpressionNode expression => EmitScopedExpression(expression.Expression, allowedLocalSymbols, allowedParameterSymbols),
            RazorVueSlotOutletNode slot => EmitSlotOutlet(slot, allowedLocalSymbols, allowedParameterSymbols),
            RazorVueConditionalNode conditional => "(" + EmitScopedExpression(conditional.Condition, allowedLocalSymbols, allowedParameterSymbols) + " ? " +
                                                  EmitFragment(conditional.WhenTrue, allowedLocalSymbols, allowedParameterSymbols) + " : " +
                                                  EmitFragment(conditional.WhenFalse, allowedLocalSymbols, allowedParameterSymbols) + ")",
            RazorVueForEachNode loop => EmitLoop(loop, allowedLocalSymbols, allowedParameterSymbols),
            RazorVueForNode loop => EmitForLoop(loop, allowedLocalSymbols, allowedParameterSymbols),
            _ => throw new NotSupportedException($"Unsupported RazorVue render node: {node.GetType().Name}.")
        };

    private string EmitElementNode(
        RazorVueElementNode element,
        ImmutableHashSet<ILocalSymbol> allowedLocalSymbols,
        ImmutableHashSet<IParameterSymbol> allowedParameterSymbols)
        => EmitVNodeCall(
            ToJavaScriptString(element.TagName),
            EmitAttributesArgument(element.Attributes, allowedLocalSymbols, allowedParameterSymbols),
            EmitFragmentArgument(element.Children, allowedLocalSymbols, allowedParameterSymbols));

    private string EmitComponentNode(
        RazorVueComponentNode component,
        ImmutableHashSet<ILocalSymbol> allowedLocalSymbols,
        ImmutableHashSet<IParameterSymbol> allowedParameterSymbols)
    {
        _resolvedComponents.TryGetValue(component.ComponentName, out var descriptor);
        _componentSlotsByPublicName.TryGetValue(component.ComponentName, out var slotsByPublicName);
        _componentEmitDescriptorsByRazorAlias.TryGetValue(component.ComponentName, out var emitDescriptorsByAlias);

        // Library components only accept default child content when the stub
        // explicitly exposes ChildContent as part of the authoring contract.
        ValidateComponentAuthoringAttributes(component, descriptor, slotsByPublicName, emitDescriptorsByAlias);
        ValidateDefaultLibrarySlotUsage(component, descriptor, descriptor?.Slots ?? ImmutableArray<VueSlotDescriptor>.Empty);
        ValidateDuplicateLibrarySlotUsage(component, descriptor, descriptor?.Slots ?? ImmutableArray<VueSlotDescriptor>.Empty);

        var slotEntries = new List<string>();
        if (!component.Children.Children.IsDefaultOrEmpty)
            slotEntries.Add(EmitImplicitDefaultSlotEntry(component, descriptor, allowedLocalSymbols, allowedParameterSymbols));
        AppendExplicitSlotTemplates(component, descriptor, slotsByPublicName, slotEntries, allowedLocalSymbols, allowedParameterSymbols);

        var attributes = EmitAttributesArgument(component.Attributes, component, slotEntries, allowedLocalSymbols, allowedParameterSymbols);
        var slots = slotEntries.Count == 0
            ? OptionalJsArgument.Missing
            : new OptionalJsArgument("{ " + string.Join(", ", slotEntries) + " }", true);

        return EmitVNodeCall(
            ResolveComponentReference(component),
            attributes,
            slots);
    }

    private void ValidateDefaultLibrarySlotUsage(
        RazorVueComponentNode component,
        VueComponentDescriptor? descriptor,
        ImmutableArray<VueSlotDescriptor> slots)
    {
        var hasDefaultChildren = !component.Children.Children.IsDefaultOrEmpty;
        if (descriptor is null ||
            descriptor.SourceKind != VueComponentSourceKind.LibraryComponent ||
            !hasDefaultChildren)
        {
            return;
        }

        var origin = CollectOrigins(component.Children).FirstOrDefault() ??
                     component.Origins.FirstOrDefault();

        if (VueSlotResolver.TryResolve(slots, "ChildContent", out var defaultSlot))
        {
            return;
        }

        throw CreateAuthoringIssue(
            RazorVueIssueCode.UnknownSlot,
            $"Component '{descriptor.Name}' does not declare a child content parameter named 'ChildContent'.",
            origin);
    }

    private string EmitImplicitDefaultSlotEntry(
        RazorVueComponentNode component,
        VueComponentDescriptor? descriptor,
        ImmutableHashSet<ILocalSymbol> allowedLocalSymbols,
        ImmutableHashSet<IParameterSymbol> allowedParameterSymbols)
    {
        var prefix = "default: () => ";
        if (descriptor is not null &&
            VueSlotResolver.TryResolve(descriptor.Slots, "ChildContent", out var defaultSlot) &&
            !defaultSlot.Descriptor.Parameters.IsDefaultOrEmpty)
        {
            var slotParameterName = RazorVueSlotParameterNames.CreateImplicitDefaultSlotParameterName(
                allowedLocalSymbols,
                allowedParameterSymbols);
            prefix = "default: (" + slotParameterName + ") => ";
        }

        return prefix + EmitFragment(component.Children, allowedLocalSymbols, allowedParameterSymbols);
    }

    private void ValidateDuplicateLibrarySlotUsage(
        RazorVueComponentNode component,
        VueComponentDescriptor? descriptor,
        ImmutableArray<VueSlotDescriptor> slots)
    {
        if (descriptor is null ||
            descriptor.SourceKind != VueComponentSourceKind.LibraryComponent ||
            slots.IsDefaultOrEmpty)
        {
            return;
        }

        // Library slots are single-assignment authoring contracts. A duplicate
        // slot input would otherwise collapse into duplicate Vue slot keys.
        var assignedSlots = new HashSet<string>(StringComparer.Ordinal);
        if (!component.Children.Children.IsDefaultOrEmpty &&
            VueSlotResolver.TryResolve(slots, "ChildContent", out var childContentSlot))
        {
            assignedSlots.Add(childContentSlot.SlotName);
        }

        foreach (var slotTemplate in component.SlotTemplates)
        {
            if (!VueSlotResolver.TryResolve(slots, slotTemplate.PublicName, out var slot))
                continue;

            if (!assignedSlots.Add(slot.SlotName))
            {
                throw CreateAuthoringIssue(
                    RazorVueIssueCode.DuplicateSlotValue,
                    $"Component '{descriptor.Name}' receives child content parameter '{slotTemplate.PublicName}' more than once.",
                    slotTemplate.Origins.IsDefaultOrEmpty ? null : slotTemplate.Origins[0]);
            }
        }

        foreach (var attributeEntry in component.Attributes)
        {
            if (attributeEntry is not RazorVueAttributeNode attribute)
                continue;

            if (!VueSlotResolver.TryResolve(slots, attribute.Name, out var slot))
                continue;

            if (assignedSlots.Add(slot.SlotName))
                continue;

            throw CreateAuthoringIssue(
                RazorVueIssueCode.DuplicateSlotValue,
                $"Component '{descriptor.Name}' receives child content parameter '{attribute.Name}' more than once.",
                attribute);
        }
    }

    private string EmitSlotOutlet(
        RazorVueSlotOutletNode slot,
        ImmutableHashSet<ILocalSymbol> allowedLocalSymbols,
        ImmutableHashSet<IParameterSymbol> allowedParameterSymbols)
    {
        if (slot.Argument is null)
            return "slots." + slot.SlotName + " ? slots." + slot.SlotName + "() : null";

        return "slots." + slot.SlotName + " ? slots." + slot.SlotName + "(" + EmitScopedExpression(slot.Argument, allowedLocalSymbols, allowedParameterSymbols) + ") : null";
    }

    private string EmitLoop(
        RazorVueForEachNode loop,
        ImmutableHashSet<ILocalSymbol> allowedLocalSymbols,
        ImmutableHashSet<IParameterSymbol> allowedParameterSymbols)
        => EmitScopedExpression(loop.Source, allowedLocalSymbols, allowedParameterSymbols) + ".map((" + loop.ItemName + ") => " +
           EmitFragment(
               loop.Body,
               RazorVueTemplateExpressionScopeValidator.AddIfPresent(allowedLocalSymbols, loop.ItemSymbol),
               allowedParameterSymbols) + ")";

    private string EmitForLoop(
        RazorVueForNode loop,
        ImmutableHashSet<ILocalSymbol> allowedLocalSymbols,
        ImmutableHashSet<IParameterSymbol> allowedParameterSymbols)
        => EmitForRangeInvocation(loop, allowedLocalSymbols, allowedParameterSymbols) + ".map((" + loop.VariableName + ") => " +
           EmitFragment(
               loop.Body,
               RazorVueTemplateExpressionScopeValidator.AddIfPresent(allowedLocalSymbols, loop.VariableSymbol),
               allowedParameterSymbols) + ")";

    private string EmitForRangeInvocation(
        RazorVueForNode loop,
        ImmutableHashSet<ILocalSymbol> allowedLocalSymbols,
        ImmutableHashSet<IParameterSymbol> allowedParameterSymbols)
        => "__jazorVueForRange(" +
           EmitScopedExpression(loop.InitialValue, allowedLocalSymbols, allowedParameterSymbols) + ", " +
           EmitScopedExpression(loop.LimitValue, allowedLocalSymbols, allowedParameterSymbols) + ", " +
           ToJavaScriptString(RazorVueForLoopLoweringSupport.GetForConditionOperator(loop.ConditionKind)) + ", " +
           ToJavaScriptString(RazorVueForLoopLoweringSupport.GetForStepOperator(loop.StepKind)) + ", " +
           (loop.StepValue is null ? "null" : EmitScopedExpression(loop.StepValue, allowedLocalSymbols, allowedParameterSymbols)) + ")";

    private static string EmitVNodeCall(
        string target,
        OptionalJsArgument props,
        OptionalJsArgument children)
    {
        if (!props.HasValue && !children.HasValue)
            return "h(" + target + ")";

        if (props.HasValue && !children.HasValue)
            return "h(" + target + ", " + props.Expression + ")";

        if (!props.HasValue)
            return "h(" + target + ", null, " + children.Expression + ")";

        return "h(" + target + ", " + props.Expression + ", " + children.Expression + ")";
    }

    private OptionalJsArgument EmitAttributesArgument(
        ImmutableArray<RazorVueAttributeEntry> attributes,
        ImmutableHashSet<ILocalSymbol> allowedLocalSymbols,
        ImmutableHashSet<IParameterSymbol> allowedParameterSymbols)
    {
        if (attributes.IsDefaultOrEmpty)
            return OptionalJsArgument.Missing;

        var segments = new List<string>();
        var objectEntries = new List<string>();
        var containsSpread = false;
        foreach (var attributeEntry in attributes)
        {
            switch (attributeEntry)
            {
                case RazorVueAttributeNode attribute:
                    objectEntries.Add(ToJavaScriptString(attribute.Name) + ": " + (attribute.Value is null ? "true" : EmitScopedExpression(attribute.Value!, allowedLocalSymbols, allowedParameterSymbols)));
                    break;
                case RazorVueAttributeSpreadNode spread:
                    containsSpread = true;
                    FlushObjectEntries(segments, objectEntries);
                    segments.Add(EmitScopedExpression(spread.Expression, allowedLocalSymbols, allowedParameterSymbols));
                    break;
            }
        }

        FlushObjectEntries(segments, objectEntries);
        return BuildPropsArgument(segments, containsSpread);
    }

    private OptionalJsArgument EmitAttributesArgument(
        ImmutableArray<RazorVueAttributeEntry> attributes,
        RazorVueComponentNode component,
        List<string> slotEntries,
        ImmutableHashSet<ILocalSymbol> allowedLocalSymbols,
        ImmutableHashSet<IParameterSymbol> allowedParameterSymbols)
    {
        if (attributes.IsDefaultOrEmpty)
            return OptionalJsArgument.Missing;

        _componentEmitsByRazorAlias.TryGetValue(component.ComponentName, out var emitsByAlias);
        _componentEmitDescriptorsByRazorAlias.TryGetValue(component.ComponentName, out var emitDescriptorsByAlias);
        _componentSlotsByPublicName.TryGetValue(component.ComponentName, out var slotsByPublicName);
        _resolvedComponents.TryGetValue(component.ComponentName, out var resolvedDescriptor);

        var segments = new List<string>();
        var objectEntries = new List<string>();
        var containsSpread = false;
        foreach (var attributeEntry in attributes)
        {
            if (attributeEntry is RazorVueAttributeSpreadNode spread)
            {
                containsSpread = true;
                FlushObjectEntries(segments, objectEntries);
                ValidateComponentSpreadTarget(component, resolvedDescriptor, spread);
                segments.Add(EmitScopedExpression(spread.Expression, allowedLocalSymbols, allowedParameterSymbols));
                continue;
            }

            var attribute = (RazorVueAttributeNode)attributeEntry;
            if (resolvedDescriptor is not null &&
                VueSlotResolver.TryResolve(resolvedDescriptor.Slots, attribute.Name, out var slot))
            {
                var slotDescriptor = slot.Descriptor;
                if (attribute.Value is null)
                {
                    throw CreateAuthoringIssue(
                        RazorVueIssueCode.MissingSlotValue,
                        $"Child content parameter '{attribute.Name}' on component '{GetComponentDisplayName(component)}' must be assigned a value.",
                        attribute);
                }

                var slotName = slot.SlotName;
                if (TryGetCurrentComponentSlotDescriptor(attribute.Value!, out var currentSlot))
                {
                    if (slotDescriptor.Parameters.IsDefaultOrEmpty)
                    {
                        slotEntries.Add(FormatObjectPropertyKey(slotName) + ": () => " + EmitCurrentComponentSlotInvocation(currentSlot));
                    }
                    else
                    {
                        var slotParameterName = slotDescriptor.Parameters[0].Name;
                        slotEntries.Add(FormatObjectPropertyKey(slotName) + ": (" + slotParameterName + ") => " + EmitCurrentComponentSlotInvocation(currentSlot, slotParameterName));
                    }
                }
                else
                {
                    var slotExpression = EmitScopedExpression(attribute.Value!, allowedLocalSymbols, allowedParameterSymbols);
                    if (!slotDescriptor.Parameters.IsDefaultOrEmpty &&
                        !IsCallableSlotValue(attribute.Value!))
                    {
                        throw CreateAuthoringIssue(
                            RazorVueIssueCode.SlotContextMisuse,
                            $"Child content parameter '{attribute.Name}' on component '{GetComponentDisplayName(component)}' expects a callable template that accepts '{DescribeSlotContext(slotDescriptor)}'.",
                            attribute);
                    }

                    if (slotDescriptor.Parameters.IsDefaultOrEmpty || !IsCallableSlotExpression(attribute.Value!))
                    {
                        slotEntries.Add(FormatObjectPropertyKey(slotName) + ": () => " + slotExpression);
                    }
                    else
                    {
                        // Preserve the declared slot context name so generated authoring
                        // code matches the library contract instead of hard-coding "context".
                        var slotParameterName = slotDescriptor.Parameters[0].Name;
                        slotEntries.Add(FormatObjectPropertyKey(slotName) + ": (" + slotParameterName + ") => " + slotExpression + "(" + slotParameterName + ")");
                    }
                }

                continue;
            }

            var name = attribute.Name;
            if (emitsByAlias is not null && emitsByAlias.TryGetValue(name, out var vueEventName))
                name = vueEventName;
            else if (resolvedDescriptor is not null &&
                     VuePropResolver.TryResolve(resolvedDescriptor.Props, name, out var prop))
                name = prop.PropName;

            objectEntries.Add(ToJavaScriptString(name) + ": " + (attribute.Value is null ? "true" : EmitScopedExpression(attribute.Value!, allowedLocalSymbols, allowedParameterSymbols)));
        }

        FlushObjectEntries(segments, objectEntries);
        return BuildPropsArgument(segments, containsSpread);
    }

    private OptionalJsArgument BuildPropsArgument(List<string> segments, bool containsSpread)
    {
        if (segments.Count == 0)
            return OptionalJsArgument.Missing;

        if (!containsSpread && segments.Count == 1)
            return new OptionalJsArgument(segments[0], true);

        return new OptionalJsArgument(RazorVueAttributeMergeHelper.BuildInvocation(segments), true);
    }

    private static void FlushObjectEntries(List<string> segments, List<string> objectEntries)
    {
        if (objectEntries.Count == 0)
            return;

        segments.Add("{ " + string.Join(", ", objectEntries) + " }");
        objectEntries.Clear();
    }

    private void ValidateComponentAuthoringAttributes(
        RazorVueComponentNode component,
        VueComponentDescriptor? resolvedDescriptor,
        ImmutableDictionary<string, VueSlotDescriptor>? slotsByPublicName,
        ImmutableDictionary<string, VueEmitDescriptor>? emitsByAlias)
    {
        if (resolvedDescriptor is not { SourceKind: VueComponentSourceKind.LibraryComponent } descriptor)
        {
            return;
        }

        var attributeNames = component.Attributes
            .OfType<RazorVueAttributeNode>()
            .Select(static attribute => attribute.Name)
            .ToImmutableHashSet(StringComparer.Ordinal);
        var slots = descriptor.Slots;

        ValidateInvalidBindTargets(component, descriptor, emitsByAlias, attributeNames);
        ValidateDuplicateMappedComponentAttributes(component, descriptor, emitsByAlias);

        foreach (var attributeEntry in component.Attributes)
        {
            if (attributeEntry is RazorVueAttributeSpreadNode spread)
            {
                ValidateComponentSpreadTarget(component, descriptor, spread);
                continue;
            }

            var attribute = (RazorVueAttributeNode)attributeEntry;
            if (VueSlotResolver.TryResolve(slots, attribute.Name, out var slot))
            {
                var slotDescriptor = slot.Descriptor;
                if (!slotDescriptor.Parameters.IsDefaultOrEmpty &&
                    attribute.Value is not null &&
                    !IsCallableSlotExpression(attribute.Value))
                {
                    throw CreateAuthoringIssue(
                        RazorVueIssueCode.SlotContextMisuse,
                        $"Child content parameter '{attribute.Name}' on component '{descriptor.Name}' expects a callable template that accepts '{DescribeSlotContext(slotDescriptor)}'.",
                        attribute);
                }

                continue;
            }

            if (VuePropResolver.TryResolve(descriptor.Props, attribute.Name, out _))
                continue;

            if (emitsByAlias is not null && emitsByAlias.ContainsKey(attribute.Name))
                continue;

            if (attribute.Value is not null && IsRenderFragmentLike(attribute.Value))
            {
                throw CreateAuthoringIssue(
                    RazorVueIssueCode.UnknownSlot,
                    $"Component '{descriptor.Name}' does not declare a child content parameter named '{attribute.Name}'.",
                    attribute);
            }

            if (HasCaptureUnmatchedValuesProp(descriptor) &&
                RazorVueCaptureUnmatchedAttributePolicy.CanCaptureExplicitAttribute(attribute.Name))
            {
                continue;
            }

            throw CreateAuthoringIssue(
                RazorVueIssueCode.UnknownParameter,
                $"Component '{descriptor.Name}' does not declare a parameter named '{attribute.Name}'.",
                attribute);
        }

        foreach (var slotTemplate in component.SlotTemplates)
        {
            if (!VueSlotResolver.TryResolve(slots, slotTemplate.PublicName, out var slot))
            {
                throw CreateAuthoringIssue(
                    RazorVueIssueCode.UnknownSlot,
                    $"Component '{descriptor.Name}' does not declare a child content parameter named '{slotTemplate.PublicName}'.",
                    slotTemplate.Origins.IsDefaultOrEmpty ? null : slotTemplate.Origins[0]);
            }

            var slotDescriptor = slot.Descriptor;
            if (slotDescriptor.Parameters.IsDefaultOrEmpty)
            {
                if (!string.IsNullOrWhiteSpace(slotTemplate.ParameterName))
                {
                    throw CreateAuthoringIssue(
                        RazorVueIssueCode.SlotContextMisuse,
                        $"Child content parameter '{slotTemplate.PublicName}' on component '{descriptor.Name}' does not accept a slot context parameter.",
                        slotTemplate.Origins.IsDefaultOrEmpty ? null : slotTemplate.Origins[0]);
                }
            }
            else if (string.IsNullOrWhiteSpace(slotTemplate.ParameterName))
            {
                throw CreateAuthoringIssue(
                    RazorVueIssueCode.SlotContextMisuse,
                    $"Child content parameter '{slotTemplate.PublicName}' on component '{descriptor.Name}' expects a callable template that accepts '{DescribeSlotContext(slotDescriptor)}'.",
                    slotTemplate.Origins.IsDefaultOrEmpty ? null : slotTemplate.Origins[0]);
            }
        }
    }

    private static bool HasCaptureUnmatchedValuesProp(VueComponentDescriptor? descriptor)
        => descriptor?.Props.Any(static prop => prop.CaptureUnmatchedValues) == true;

    private void ValidateDuplicateMappedComponentAttributes(
        RazorVueComponentNode component,
        VueComponentDescriptor descriptor,
        ImmutableDictionary<string, VueEmitDescriptor>? emitsByAlias)
    {
        var mappedAttributes = new Dictionary<string, RazorVueAttributeNode>(StringComparer.Ordinal);
        foreach (var attributeEntry in component.Attributes)
        {
            if (attributeEntry is not RazorVueAttributeNode attribute)
                continue;

            if (VuePropResolver.TryResolve(descriptor.Props, attribute.Name, out var prop))
            {
                ValidateUniqueMappedAttribute(
                    descriptor,
                    mappedAttributes,
                    "prop:" + prop.PropName,
                    "Vue prop",
                    prop.PropName,
                    attribute);
                continue;
            }

            if (emitsByAlias is not null &&
                emitsByAlias.TryGetValue(attribute.Name, out var emitDescriptor))
            {
                ValidateUniqueMappedAttribute(
                    descriptor,
                    mappedAttributes,
                    "emit:" + emitDescriptor.Name,
                    "Vue event",
                    emitDescriptor.Name,
                    attribute);
            }
        }
    }

    private void ValidateUniqueMappedAttribute(
        VueComponentDescriptor descriptor,
        Dictionary<string, RazorVueAttributeNode> mappedAttributes,
        string mappedKey,
        string mappedKind,
        string mappedName,
        RazorVueAttributeNode attribute)
    {
        if (!mappedAttributes.ContainsKey(mappedKey))
        {
            mappedAttributes.Add(mappedKey, attribute);
            return;
        }

        var firstAttribute = mappedAttributes[mappedKey];
        throw CreateAuthoringIssue(
            RazorVueIssueCode.UnknownParameter,
            $"Component '{descriptor.Name}' receives both '{firstAttribute.Name}' and '{attribute.Name}', but both map to {mappedKind} '{mappedName}'. Use only one authoring parameter for that target.",
            attribute);
    }

    private void ValidateInvalidBindTargets(
        RazorVueComponentNode component,
        VueComponentDescriptor descriptor,
        ImmutableDictionary<string, VueEmitDescriptor>? emitsByAlias,
        ImmutableHashSet<string> attributeNames)
    {
        foreach (var attributeEntry in component.Attributes)
        {
            if (attributeEntry is not RazorVueAttributeNode attribute)
                continue;

            if (!TryGetBindTargetName(attribute.Name, out var parameterName) ||
                !attributeNames.Contains(parameterName))
            {
                continue;
            }

            var hasBindableProp = VuePropResolver.TryResolve(descriptor.Props, parameterName, out var prop) &&
                                  prop.Descriptor.AcceptsBinding;
            var hasModelUpdateEmit = emitsByAlias is not null &&
                                     emitsByAlias.TryGetValue(attribute.Name, out var emitDescriptor) &&
                                     emitDescriptor.Kind == VueEmitKind.ModelUpdate;

            if (hasBindableProp && hasModelUpdateEmit)
                continue;

            throw CreateAuthoringIssue(
                RazorVueIssueCode.InvalidBindTarget,
                $"Component '{descriptor.Name}' does not support two-way binding for parameter '{parameterName}'.",
                attribute);
        }
    }

    private RazorVueCompilationIssueException CreateAuthoringIssue(
        RazorVueIssueCode code,
        string message,
        RazorVueAttributeNode attribute)
        => CreateAuthoringIssue(
            code,
            message,
            attribute.Origins.IsDefaultOrEmpty ? null : attribute.Origins[0]);

    private RazorVueCompilationIssueException CreateAuthoringIssue(
        RazorVueIssueCode code,
        string message,
        RazorVueSourceOrigin? origin)
    {
        var issue = new RazorVueCompilationIssue(
            code,
            RazorVueIssueSeverity.Error,
            message,
            ImmutableArray<string>.Empty);
        return new RazorVueCompilationIssueException(issue, _snapshot.Descriptor.FullName, origin);
    }

    private void ValidateComponentSpreadTarget(
        RazorVueComponentNode component,
        VueComponentDescriptor? descriptor,
        RazorVueAttributeSpreadNode spread)
    {
        var captureUnmatchedValueProps = descriptor?.Props
            .Where(static prop => prop.CaptureUnmatchedValues)
            .Distinct()
            .Take(2)
            .ToArray();
        if (captureUnmatchedValueProps is { Length: > 1 })
        {
            var duplicateSinkComponentDisplayName = descriptor?.Name ?? GetComponentDisplayName(component);
            throw CreateAuthoringIssue(
                RazorVueIssueCode.CanonicalizationFailed,
                $"Component '{duplicateSinkComponentDisplayName}' declares multiple [Parameter(CaptureUnmatchedValues = true)] sinks; RazorVue requires exactly one.",
                spread.Origins.IsDefaultOrEmpty ? null : spread.Origins[0]);
        }

        var captureUnmatchedValuesProp = captureUnmatchedValueProps is { Length: 1 }
            ? captureUnmatchedValueProps[0]
            : null;
        if (captureUnmatchedValuesProp is not null)
            return;

        var componentDisplayName = descriptor?.Name ?? GetComponentDisplayName(component);
        throw CreateAuthoringIssue(
            RazorVueIssueCode.UnknownParameter,
            $"Component '{componentDisplayName}' does not declare a [Parameter(CaptureUnmatchedValues = true)] sink for arbitrary attributes.",
            spread.Origins.IsDefaultOrEmpty ? null : spread.Origins[0]);
    }

    private static bool TryGetBindTargetName(string attributeName, out string parameterName)
    {
        parameterName = string.Empty;
        if (string.IsNullOrWhiteSpace(attributeName) ||
            !attributeName.EndsWith("Changed", StringComparison.Ordinal) ||
            attributeName.Length <= "Changed".Length)
        {
            return false;
        }

        parameterName = attributeName.Substring(0, attributeName.Length - "Changed".Length);
        return !string.IsNullOrWhiteSpace(parameterName);
    }

    private static string DescribeSlotContext(VueSlotDescriptor slotDescriptor)
        => string.Join(", ", slotDescriptor.Parameters.Select(static parameter => parameter.TypeName));

    private void AppendExplicitSlotTemplates(
        RazorVueComponentNode component,
        VueComponentDescriptor? descriptor,
        ImmutableDictionary<string, VueSlotDescriptor>? slotsByPublicName,
        List<string> slotEntries,
        ImmutableHashSet<ILocalSymbol> allowedLocalSymbols,
        ImmutableHashSet<IParameterSymbol> allowedParameterSymbols)
    {
        if (component.SlotTemplates.IsDefaultOrEmpty)
            return;

        foreach (var slotTemplate in component.SlotTemplates)
        {
            if (descriptor is not null &&
                VueSlotResolver.TryResolve(descriptor.Slots, slotTemplate.PublicName, out var slot))
            {
                var slotDescriptor = slot.Descriptor;
                if (slotDescriptor.Parameters.IsDefaultOrEmpty)
                {
                    slotEntries.Add(FormatObjectPropertyKey(slot.SlotName) + ": () => " + EmitFragment(slotTemplate.Children, allowedLocalSymbols, allowedParameterSymbols));
                }
                else
                {
                    var slotParameterName = slotTemplate.ParameterName ?? slotDescriptor.Parameters[0].Name;
                    slotEntries.Add(FormatObjectPropertyKey(slot.SlotName) + ": (" + slotParameterName + ") => " +
                                    EmitFragment(
                                        slotTemplate.Children,
                                        allowedLocalSymbols,
                                        RazorVueTemplateExpressionScopeValidator.AddIfPresent(allowedParameterSymbols, slotTemplate.ParameterSymbol)));
                }

                continue;
            }

            var slotName = string.Equals(slotTemplate.PublicName, "ChildContent", StringComparison.Ordinal)
                ? "default"
                : char.ToLowerInvariant(slotTemplate.PublicName[0]) + slotTemplate.PublicName.Substring(1);
            if (string.IsNullOrWhiteSpace(slotTemplate.ParameterName))
                slotEntries.Add(FormatObjectPropertyKey(slotName) + ": () => " + EmitFragment(slotTemplate.Children, allowedLocalSymbols, allowedParameterSymbols));
            else
                slotEntries.Add(FormatObjectPropertyKey(slotName) + ": (" + slotTemplate.ParameterName + ") => " +
                                EmitFragment(
                                    slotTemplate.Children,
                                    allowedLocalSymbols,
                                    RazorVueTemplateExpressionScopeValidator.AddIfPresent(allowedParameterSymbols, slotTemplate.ParameterSymbol)));
        }
    }

    private static string FormatObjectPropertyKey(string name)
        => IsSimpleJavaScriptIdentifier(name) ? name : ToJavaScriptString(name);

    private static string EmitCurrentComponentSlotInvocation(VueSlotDescriptor slotDescriptor, string? argumentExpression = null)
    {
        var slotAccess = GetSlotAccessExpression(slotDescriptor.Name);
        return string.IsNullOrEmpty(argumentExpression)
            ? slotAccess + " ? " + slotAccess + "() : null"
            : slotAccess + " ? " + slotAccess + "(" + argumentExpression + ") : null";
    }

    private static string GetSlotAccessExpression(string slotName)
        => IsSimpleJavaScriptIdentifier(slotName)
            ? "slots." + slotName
            : "slots[" + ToJavaScriptString(slotName) + "]";

    private static bool IsSimpleJavaScriptIdentifier(string value)
    {
        if (string.IsNullOrEmpty(value) || !IsIdentifierStart(value[0]))
            return false;

        for (var index = 1; index < value.Length; index++)
        {
            if (!IsIdentifierPart(value[index]))
                return false;
        }

        return true;
    }

    private static bool IsIdentifierStart(char value)
        => value == '_' || value == '$' || char.IsLetter(value);

    private static bool IsIdentifierPart(char value)
        => IsIdentifierStart(value) || char.IsDigit(value);

    private string EmitScopedExpression(
        IOperation operation,
        ImmutableHashSet<ILocalSymbol> allowedLocalSymbols,
        ImmutableHashSet<IParameterSymbol> allowedParameterSymbols)
    {
        RazorVueTemplateExpressionScopeValidator.Validate(_snapshot, operation, allowedLocalSymbols, allowedParameterSymbols);
        return EmitExpression(operation);
    }

    private bool IsCallableSlotValue(IOperation operation)
    {
        if (IsCallableSlotExpression(operation))
            return true;

        return TryGetCurrentComponentSlotDescriptor(operation, out var currentSlot) &&
               !currentSlot.Parameters.IsDefaultOrEmpty;
    }

    private bool TryGetCurrentComponentSlotDescriptor(IOperation operation, out VueSlotDescriptor slotDescriptor)
    {
        slotDescriptor = default!;
        var current = Unwrap(operation);
        if (current is not IPropertyReferenceOperation propertyReference ||
            !IsCurrentComponentMember(propertyReference.Property, propertyReference.Instance))
        {
            return false;
        }

        return _slotsByPublicName.TryGetValue(propertyReference.Property.Name, out slotDescriptor);
    }

    private static bool IsRenderFragmentLike(IOperation operation)
    {
        if (Unwrap(operation)?.Type is not INamedTypeSymbol namedType)
            return false;

        var definition = namedType.OriginalDefinition;
        var metadataName = definition.ToDisplayString();
        return string.Equals(metadataName, "Microsoft.AspNetCore.Components.RenderFragment", StringComparison.Ordinal) ||
               string.Equals(metadataName, "Microsoft.AspNetCore.Components.RenderFragment<T>", StringComparison.Ordinal);
    }

    private string ResolveComponentReference(RazorVueComponentNode component)
    {
        if (_componentReferences.TryGetValue(component.ComponentName, out var reference))
            return reference;

        throw new NotSupportedException(
            $"RazorVue render could not resolve component node '{component.ComponentName}' in component '{_snapshot.Descriptor.FullName}'.");
    }

    private string GetComponentDisplayName(RazorVueComponentNode component)
        => _resolvedComponents.TryGetValue(component.ComponentName, out var descriptor)
            ? descriptor.Name
            : component.ComponentName;
}
