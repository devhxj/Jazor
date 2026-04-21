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
            _ => throw new NotSupportedException($"Unsupported RazorVue render node: {node.GetType().Name}.")
        };

    private string EmitElementNode(RazorVueElementNode element)
        => "h(" + ToJavaScriptString(element.TagName) + ", " +
           EmitAttributes(element.Attributes) + ", " +
           EmitFragment(element.Children) + ")";

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

        var attributes = EmitAttributes(component.Attributes, component, slotEntries);
        var slots = slotEntries.Count == 0
            ? "null"
            : "{ " + string.Join(", ", slotEntries) + " }";

        return "h(" + ResolveComponentReference(component) + ", " + attributes + ", " + slots + ")";
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

        foreach (var attribute in component.Attributes)
        {
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

    private string EmitAttributes(ImmutableArray<RazorVueAttributeNode> attributes)
    {
        if (attributes.IsDefaultOrEmpty)
            return "null";

        var entries = attributes.Select(attribute =>
            ToJavaScriptString(attribute.Name) + ": " + (attribute.Value is null ? "true" : EmitExpression(attribute.Value!)));
        return "{ " + string.Join(", ", entries) + " }";
    }

    private string EmitAttributes(
        ImmutableArray<RazorVueAttributeNode> attributes,
        RazorVueComponentNode component,
        List<string> slotEntries)
    {
        if (attributes.IsDefaultOrEmpty)
            return "null";

        _componentEmitsByRazorAlias.TryGetValue(component.ComponentName, out var emitsByAlias);
        _componentEmitDescriptorsByRazorAlias.TryGetValue(component.ComponentName, out var emitDescriptorsByAlias);
        _componentPropsByPublicName.TryGetValue(component.ComponentName, out var propsByPublicName);
        _componentSlotsByPublicName.TryGetValue(component.ComponentName, out var slotsByPublicName);

        // Library stubs are explicit authoring contracts, so invalid parameters
        // should fail at compile-time instead of silently falling through to attrs.
        ValidateComponentAuthoringAttributes(component, propsByPublicName, slotsByPublicName, emitDescriptorsByAlias);

        var entries = new List<string>();
        foreach (var attribute in attributes)
        {
            if (slotsByPublicName is not null &&
                slotsByPublicName.TryGetValue(attribute.Name, out var slotDescriptor) &&
                attribute.Value is not null)
            {
                var slotName = slotDescriptor.Name;
                var slotExpression = EmitExpression(attribute.Value!);
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

            entries.Add(ToJavaScriptString(name) + ": " + (attribute.Value is null ? "true" : EmitExpression(attribute.Value!)));
        }

        return entries.Count == 0
            ? "null"
            : "{ " + string.Join(", ", entries) + " }";
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
            .Select(static attribute => attribute.Name)
            .ToImmutableHashSet(StringComparer.Ordinal);

        ValidateInvalidBindTargets(component, descriptor, propsByPublicName, emitsByAlias, attributeNames);

        foreach (var attribute in component.Attributes)
        {
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
    }

    private void ValidateInvalidBindTargets(
        RazorVueComponentNode component,
        VueComponentDescriptor descriptor,
        ImmutableDictionary<string, VuePropDescriptor>? propsByPublicName,
        ImmutableDictionary<string, VueEmitDescriptor>? emitsByAlias,
        ImmutableHashSet<string> attributeNames)
    {
        foreach (var attribute in component.Attributes)
        {
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
}
