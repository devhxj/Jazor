using System.Collections.Immutable;
using System.Collections.Generic;
using System.Linq;
using Jazor.RazorVue.Artifacts;
using Jazor.RazorVue.Descriptor;
using Jazor.RazorVue.RenderTree;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Operations;

namespace Jazor.RazorVue.Lowering;

internal sealed partial class RazorVueExpressionEmitter
{
    private string EmitNode(RazorVueRenderNode node)
        => node switch
        {
            RazorVueElementNode element => EmitElementNode(element),
            RazorVueComponentNode component => EmitComponentNode(component),
            RazorVueTextNode text => ToJavaScriptString(text.Text),
            RazorVueExpressionNode expression => EmitExpression(expression.Expression),
            RazorVueSlotOutletNode slot => EmitSlotOutlet(slot),
            RazorVueConditionalNode conditional => "(" + EmitExpression(conditional.Condition) + " ? " +
                                                  EmitFragment(conditional.WhenTrue) + " : " +
                                                  EmitFragment(conditional.WhenFalse) + ")",
            RazorVueForEachNode loop => EmitLoop(loop),
            RazorVueForNode loop => EmitForLoop(loop),
            _ => throw new NotSupportedException($"Unsupported RazorVue render node: {node.GetType().Name}.")
        };

    private string EmitElementNode(RazorVueElementNode element)
        => EmitVNodeCall(
            ToJavaScriptString(element.TagName),
            EmitAttributesArgument(element.Attributes),
            EmitFragmentArgument(element.Children));

    private string EmitComponentNode(RazorVueComponentNode component)
    {
        _resolvedComponents.TryGetValue(component.ComponentName, out var descriptor);
        _componentSlotsByPublicName.TryGetValue(component.ComponentName, out var slotsByPublicName);

        // Library components only accept default child content when the stub
        // explicitly exposes ChildContent as part of the authoring contract.
        ValidateDefaultLibrarySlotUsage(component, descriptor, slotsByPublicName);
        ValidateDuplicateLibrarySlotUsage(component, descriptor, slotsByPublicName);

        var slotEntries = new List<string>();
        if (!component.Children.Children.IsDefaultOrEmpty)
            slotEntries.Add("default: () => " + EmitFragment(component.Children));
        AppendExplicitSlotTemplates(component, descriptor, slotsByPublicName, slotEntries);

        var attributes = EmitAttributesArgument(component.Attributes, component, slotEntries);
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
        ImmutableDictionary<string, VueSlotDescriptor>? slotsByPublicName)
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

        if (slotsByPublicName is not null &&
            slotsByPublicName.TryGetValue("ChildContent", out var defaultSlotDescriptor))
        {
            if (defaultSlotDescriptor.Parameters.IsDefaultOrEmpty)
                return;

            // Implicit child content cannot satisfy a typed slot contract because
            // the template has no callable surface to receive the slot context.
            throw CreateAuthoringIssue(
                RazorVueIssueCode.SlotContextMisuse,
                $"Child content parameter 'ChildContent' on component '{descriptor.Name}' expects a callable template that accepts '{DescribeSlotContext(defaultSlotDescriptor)}'.",
                origin);
        }

        throw CreateAuthoringIssue(
            RazorVueIssueCode.UnknownSlot,
            $"Component '{descriptor.Name}' does not declare a child content parameter named 'ChildContent'.",
            origin);
    }

    private void ValidateDuplicateLibrarySlotUsage(
        RazorVueComponentNode component,
        VueComponentDescriptor? descriptor,
        ImmutableDictionary<string, VueSlotDescriptor>? slotsByPublicName)
    {
        if (descriptor is null ||
            descriptor.SourceKind != VueComponentSourceKind.LibraryComponent ||
            slotsByPublicName is null)
        {
            return;
        }

        // Library slots are single-assignment authoring contracts. A duplicate
        // slot input would otherwise collapse into duplicate Vue slot keys.
        var assignedSlots = new HashSet<string>(StringComparer.Ordinal);
        if (!component.Children.Children.IsDefaultOrEmpty &&
            slotsByPublicName.ContainsKey("ChildContent"))
        {
            assignedSlots.Add("ChildContent");
        }

        foreach (var slotTemplate in component.SlotTemplates)
        {
            if (!assignedSlots.Add(slotTemplate.PublicName))
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

            if (!slotsByPublicName.ContainsKey(attribute.Name))
                continue;

            if (assignedSlots.Add(attribute.Name))
                continue;

            throw CreateAuthoringIssue(
                RazorVueIssueCode.DuplicateSlotValue,
                $"Component '{descriptor.Name}' receives child content parameter '{attribute.Name}' more than once.",
                attribute);
        }
    }

    private string EmitSlotOutlet(RazorVueSlotOutletNode slot)
    {
        if (slot.Argument is null)
            return "slots." + slot.SlotName + " ? slots." + slot.SlotName + "() : null";

        return "slots." + slot.SlotName + " ? slots." + slot.SlotName + "(" + EmitExpression(slot.Argument) + ") : null";
    }

    private string EmitLoop(RazorVueForEachNode loop)
        => EmitExpression(loop.Source) + ".map((" + loop.ItemName + ") => " + EmitFragment(loop.Body) + ")";

    private string EmitForLoop(RazorVueForNode loop)
        => EmitForRangeInvocation(loop) + ".map((" + loop.VariableName + ") => " + EmitFragment(loop.Body) + ")";

    private string EmitForRangeInvocation(RazorVueForNode loop)
        => "__jazorVueForRange(" +
           EmitExpression(loop.InitialValue) + ", " +
           EmitExpression(loop.LimitValue) + ", " +
           ToJavaScriptString(RazorVueForLoopLoweringSupport.GetForConditionOperator(loop.ConditionKind)) + ", " +
           ToJavaScriptString(RazorVueForLoopLoweringSupport.GetForStepOperator(loop.StepKind)) + ", " +
           (loop.StepValue is null ? "null" : EmitExpression(loop.StepValue)) + ")";

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
            return "h(" + target + ", " + children.Expression + ")";

        return "h(" + target + ", " + props.Expression + ", " + children.Expression + ")";
    }

    private OptionalJsArgument EmitAttributesArgument(ImmutableArray<RazorVueAttributeEntry> attributes)
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
                    objectEntries.Add(ToJavaScriptString(attribute.Name) + ": " + (attribute.Value is null ? "true" : EmitExpression(attribute.Value!)));
                    break;
                case RazorVueAttributeSpreadNode spread:
                    containsSpread = true;
                    FlushObjectEntries(segments, objectEntries);
                    segments.Add(EmitExpression(spread.Expression));
                    break;
            }
        }

        FlushObjectEntries(segments, objectEntries);
        return BuildPropsArgument(segments, containsSpread);
    }

    private OptionalJsArgument EmitAttributesArgument(
        ImmutableArray<RazorVueAttributeEntry> attributes,
        RazorVueComponentNode component,
        List<string> slotEntries)
    {
        if (attributes.IsDefaultOrEmpty)
            return OptionalJsArgument.Missing;

        _componentEmitsByRazorAlias.TryGetValue(component.ComponentName, out var emitsByAlias);
        _componentEmitDescriptorsByRazorAlias.TryGetValue(component.ComponentName, out var emitDescriptorsByAlias);
        _componentPropsByPublicName.TryGetValue(component.ComponentName, out var propsByPublicName);
        _componentSlotsByPublicName.TryGetValue(component.ComponentName, out var slotsByPublicName);
        _resolvedComponents.TryGetValue(component.ComponentName, out var resolvedDescriptor);

        // Library stubs are explicit authoring contracts, so invalid parameters
        // should fail at compile-time instead of silently falling through to attrs.
        ValidateComponentAuthoringAttributes(component, propsByPublicName, slotsByPublicName, emitDescriptorsByAlias);

        var segments = new List<string>();
        var objectEntries = new List<string>();
        var containsSpread = false;
        foreach (var attributeEntry in attributes)
        {
            if (attributeEntry is RazorVueAttributeSpreadNode spread)
            {
                containsSpread = true;
                FlushObjectEntries(segments, objectEntries);
                ValidateComponentSpreadTarget(component, resolvedDescriptor, propsByPublicName, spread);
                segments.Add(EmitExpression(spread.Expression));
                continue;
            }

            var attribute = (RazorVueAttributeNode)attributeEntry;
            if (slotsByPublicName is not null &&
                slotsByPublicName.TryGetValue(attribute.Name, out var slotDescriptor))
            {
                if (attribute.Value is null)
                {
                    throw CreateAuthoringIssue(
                        RazorVueIssueCode.MissingSlotValue,
                        $"Child content parameter '{attribute.Name}' on component '{GetComponentDisplayName(component)}' must be assigned a value.",
                        attribute);
                }

                var slotName = slotDescriptor.Name;
                var slotExpression = EmitExpression(attribute.Value!);
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
                    slotEntries.Add(slotName + ": () => " + slotExpression);
                }
                else
                {
                    // Preserve the declared slot context name so generated authoring
                    // code matches the library contract instead of hard-coding "context".
                    var slotParameterName = slotDescriptor.Parameters[0].Name;
                    slotEntries.Add(slotName + ": (" + slotParameterName + ") => " + slotExpression + "(" + slotParameterName + ")");
                }

                continue;
            }

            var name = attribute.Name;
            if (emitsByAlias is not null && emitsByAlias.TryGetValue(name, out var vueEventName))
                name = vueEventName;
            else if (propsByPublicName is not null && propsByPublicName.TryGetValue(name, out var propDescriptor))
                name = propDescriptor.Name;

            objectEntries.Add(ToJavaScriptString(name) + ": " + (attribute.Value is null ? "true" : EmitExpression(attribute.Value!)));
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
        ImmutableDictionary<string, VuePropDescriptor>? propsByPublicName,
        ImmutableDictionary<string, VueSlotDescriptor>? slotsByPublicName,
        ImmutableDictionary<string, VueEmitDescriptor>? emitsByAlias)
    {
        if (!_resolvedComponents.TryGetValue(component.ComponentName, out var descriptor) ||
            descriptor.SourceKind != VueComponentSourceKind.LibraryComponent ||
            component.Attributes.IsDefaultOrEmpty)
        {
            return;
        }

        var attributeNames = component.Attributes
            .OfType<RazorVueAttributeNode>()
            .Select(static attribute => attribute.Name)
            .ToImmutableHashSet(StringComparer.Ordinal);

        ValidateInvalidBindTargets(component, descriptor, propsByPublicName, emitsByAlias, attributeNames);

        foreach (var attributeEntry in component.Attributes)
        {
            if (attributeEntry is RazorVueAttributeSpreadNode spread)
            {
                ValidateComponentSpreadTarget(component, descriptor, propsByPublicName, spread);
                continue;
            }

            var attribute = (RazorVueAttributeNode)attributeEntry;
            if (slotsByPublicName is not null &&
                slotsByPublicName.TryGetValue(attribute.Name, out var slotDescriptor))
            {
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

            if (propsByPublicName is not null && propsByPublicName.ContainsKey(attribute.Name))
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

            throw CreateAuthoringIssue(
                RazorVueIssueCode.UnknownParameter,
                $"Component '{descriptor.Name}' does not declare a parameter named '{attribute.Name}'.",
                attribute);
        }

        foreach (var slotTemplate in component.SlotTemplates)
        {
            if (slotsByPublicName is null ||
                !slotsByPublicName.TryGetValue(slotTemplate.PublicName, out var slotDescriptor))
            {
                throw CreateAuthoringIssue(
                    RazorVueIssueCode.UnknownSlot,
                    $"Component '{descriptor.Name}' does not declare a child content parameter named '{slotTemplate.PublicName}'.",
                    slotTemplate.Origins.IsDefaultOrEmpty ? null : slotTemplate.Origins[0]);
            }

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

    private void ValidateInvalidBindTargets(
        RazorVueComponentNode component,
        VueComponentDescriptor descriptor,
        ImmutableDictionary<string, VuePropDescriptor>? propsByPublicName,
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

            var hasBindableProp = propsByPublicName is not null &&
                                  propsByPublicName.TryGetValue(parameterName, out var propDescriptor) &&
                                  propDescriptor.AcceptsBinding;
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
        ImmutableDictionary<string, VuePropDescriptor>? propsByPublicName,
        RazorVueAttributeSpreadNode spread)
    {
        var captureUnmatchedValueProps = propsByPublicName?.Values
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
        List<string> slotEntries)
    {
        if (component.SlotTemplates.IsDefaultOrEmpty)
            return;

        foreach (var slotTemplate in component.SlotTemplates)
        {
            if (descriptor is not null &&
                slotsByPublicName is not null &&
                slotsByPublicName.TryGetValue(slotTemplate.PublicName, out var slotDescriptor))
            {
                if (slotDescriptor.Parameters.IsDefaultOrEmpty)
                {
                    slotEntries.Add(slotDescriptor.Name + ": () => " + EmitFragment(slotTemplate.Children));
                }
                else
                {
                    var slotParameterName = slotTemplate.ParameterName ?? slotDescriptor.Parameters[0].Name;
                    slotEntries.Add(slotDescriptor.Name + ": (" + slotParameterName + ") => " + EmitFragment(slotTemplate.Children));
                }

                continue;
            }

            var slotName = string.Equals(slotTemplate.PublicName, "ChildContent", StringComparison.Ordinal)
                ? "default"
                : char.ToLowerInvariant(slotTemplate.PublicName[0]) + slotTemplate.PublicName.Substring(1);
            if (string.IsNullOrWhiteSpace(slotTemplate.ParameterName))
                slotEntries.Add(slotName + ": () => " + EmitFragment(slotTemplate.Children));
            else
                slotEntries.Add(slotName + ": (" + slotTemplate.ParameterName + ") => " + EmitFragment(slotTemplate.Children));
        }
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
