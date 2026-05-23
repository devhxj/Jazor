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
            RazorVueLocalDeclarationNode localDeclaration => throw new NotSupportedException(
                $"Template local declaration '{localDeclaration.LocalSymbol.Name}' must be lowered through fragment scope, not as a standalone vnode."),
            RazorVueTemplateScopeNode templateScope => EmitTemplateScopeNode(templateScope, allowedLocalSymbols, allowedParameterSymbols),
            RazorVueSlotOutletNode slot => EmitSlotOutlet(slot, allowedLocalSymbols, allowedParameterSymbols),
            RazorVueConditionalNode conditional => "(" + EmitScopedExpression(conditional.Condition, allowedLocalSymbols, allowedParameterSymbols) + " ? " +
                                                  EmitFragment(conditional.WhenTrue, allowedLocalSymbols, allowedParameterSymbols) + " : " +
                                                  EmitFragment(conditional.WhenFalse, allowedLocalSymbols, allowedParameterSymbols) + ")",
            RazorVueForEachNode loop => EmitLoop(loop, allowedLocalSymbols, allowedParameterSymbols),
            RazorVueForNode loop => EmitForLoop(loop, allowedLocalSymbols, allowedParameterSymbols),
            RazorVueImperativeBlockNode imperative => throw CreateAuthoringIssue(
                RazorVueIssueCode.CanonicalizationFailed,
                $"RazorVue H lowering has not yet materialized imperative render block '{imperative.Kind}' in component '{_snapshot.Descriptor.FullName}'.",
                imperative.Origins.IsDefaultOrEmpty ? _snapshot.Origins.FirstOrDefault() : imperative.Origins[0]),
            _ => throw new NotSupportedException($"Unsupported RazorVue render node: {node.GetType().Name}.")
        };

    private string EmitElementNode(
        RazorVueElementNode element,
        ImmutableHashSet<ILocalSymbol> allowedLocalSymbols,
        ImmutableHashSet<IParameterSymbol> allowedParameterSymbols)
        => EmitVNodeCall(
            ToJavaScriptString(element.TagName),
            EmitAttributesArgument(element.Attributes, element.Key, allowedLocalSymbols, allowedParameterSymbols),
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
        if (HasAnyDefaultSlotContent(component))
            slotEntries.Add(EmitImplicitDefaultSlotEntry(component, descriptor, allowedLocalSymbols, allowedParameterSymbols));
        AppendExplicitSlotTemplates(component, descriptor, slotsByPublicName, slotEntries, allowedLocalSymbols, allowedParameterSymbols);

        var attributes = EmitAttributesArgument(component.Attributes, component, slotEntries, allowedLocalSymbols, allowedParameterSymbols);
        var slots = slotEntries.Count == 0
            ? OptionalJsArgument.Missing
            : new OptionalJsArgument("{ " + string.Join(", ", slotEntries) + " }", true);

        return EmitVNodeCall(
            ResolveComponentReference(component),
            ApplyNodeKey(attributes, component.Key, allowedLocalSymbols, allowedParameterSymbols),
            slots);
    }

    private void ValidateDefaultLibrarySlotUsage(
        RazorVueComponentNode component,
        VueComponentDescriptor? descriptor,
        ImmutableArray<VueSlotDescriptor> slots)
    {
        var hasDefaultChildren = HasAnyDefaultSlotAssignment(component);
        if (descriptor is null ||
            descriptor.SourceKind != VueComponentSourceKind.LibraryComponent ||
            !hasDefaultChildren)
        {
            return;
        }

        var origin = CollectOrigins(GetDefaultSlotFragment(component)).FirstOrDefault() ??
                     component.ImplicitDefaultSlotAssignments.SelectMany(static assignment => assignment.Origins).FirstOrDefault() ??
                     component.AmbientDefaultSlotChildren.Children.SelectMany(static child => child.Origins).FirstOrDefault() ??
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
                defaultSlot.Descriptor.Parameters[0].Name,
                allowedLocalSymbols,
                allowedParameterSymbols);
            var implicitDefaultSlotFragment = GetDefaultSlotFragment(component);
            if (TryGetSingleCurrentComponentDefaultSlot(implicitDefaultSlotFragment, out var currentSlot))
            {
                return "default: (" + slotParameterName + ") => " + EmitCurrentComponentSlotInvocation(currentSlot, slotParameterName);
            }

            prefix = "default: (" + slotParameterName + ") => ";
        }

        return prefix + EmitFragment(GetDefaultSlotFragment(component), allowedLocalSymbols, allowedParameterSymbols);
    }

    private bool TryGetSingleCurrentComponentDefaultSlot(
        RazorVueRenderFragment fragment,
        out VueSlotDescriptor slotDescriptor)
    {
        slotDescriptor = default!;
        if (fragment.Children.Length != 1 ||
            fragment.Children[0] is not RazorVueSlotOutletNode slotOutlet ||
            slotOutlet.Argument is not null)
        {
            return false;
        }

        return TryResolveCurrentComponentSlotDescriptorBySlotName(slotOutlet.SlotName, out slotDescriptor);
    }

    private bool TryResolveCurrentComponentSlotDescriptorBySlotName(
        string slotName,
        out VueSlotDescriptor slotDescriptor)
    {
        foreach (var slot in _snapshot.Descriptor.Slots)
        {
            if (string.Equals(slot.Name, slotName, StringComparison.Ordinal))
            {
                slotDescriptor = slot;
                return true;
            }
        }

        slotDescriptor = default!;
        return false;
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
        if (HasAnyDefaultSlotAssignment(component) &&
            VueSlotResolver.TryResolve(slots, "ChildContent", out var childContentSlot))
        {
            var defaultSlotAssignmentCount = GetDefaultSlotAssignmentCount(component);
            if (defaultSlotAssignmentCount > 1)
            {
                throw CreateAuthoringIssue(
                    RazorVueIssueCode.DuplicateSlotValue,
                    $"Component '{descriptor.Name}' receives child content parameter 'ChildContent' more than once.",
                    GetSecondDefaultSlotAssignmentOrigin(component));
            }

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

    private static bool HasImplicitDefaultSlotAssignment(RazorVueComponentNode component)
        => !component.ImplicitDefaultSlotAssignments.IsDefaultOrEmpty;

    private static bool HasAmbientDefaultSlotContent(RazorVueComponentNode component)
        => !component.AmbientDefaultSlotChildren.Children.IsDefaultOrEmpty;

    private static bool HasAnyDefaultSlotContent(RazorVueComponentNode component)
        => HasImplicitDefaultSlotAssignment(component) || HasAmbientDefaultSlotContent(component);

    private static bool HasAnyDefaultSlotAssignment(RazorVueComponentNode component)
        => component.ImplicitDefaultSlotAssignments.Length > 0 || HasAmbientDefaultSlotContent(component);

    private static int GetDefaultSlotAssignmentCount(RazorVueComponentNode component)
        => component.ImplicitDefaultSlotAssignments.Length + (HasAmbientDefaultSlotContent(component) ? 1 : 0);

    private static RazorVueRenderFragment GetImplicitDefaultSlotFragment(RazorVueComponentNode component)
        => component.ImplicitDefaultSlotAssignments.IsDefaultOrEmpty
            ? component.Children
            : component.ImplicitDefaultSlotAssignments[0].Children;

    private static RazorVueRenderFragment GetDefaultSlotFragment(RazorVueComponentNode component)
        => !component.ImplicitDefaultSlotAssignments.IsDefaultOrEmpty
            ? component.ImplicitDefaultSlotAssignments[0].Children
            : component.AmbientDefaultSlotChildren;

    private static RazorVueSourceOrigin? GetSecondDefaultSlotAssignmentOrigin(RazorVueComponentNode component)
    {
        if (HasAmbientDefaultSlotContent(component) && component.ImplicitDefaultSlotAssignments.Length > 0)
        {
            return component.ImplicitDefaultSlotAssignments[0].Origins.IsDefaultOrEmpty
                ? null
                : component.ImplicitDefaultSlotAssignments[0].Origins[0];
        }

        if (component.ImplicitDefaultSlotAssignments.Length > 1)
        {
            return component.ImplicitDefaultSlotAssignments[1].Origins.IsDefaultOrEmpty
                ? null
                : component.ImplicitDefaultSlotAssignments[1].Origins[0];
        }

        return component.AmbientDefaultSlotChildren.Children
            .SelectMany(static child => child.Origins)
            .FirstOrDefault();
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
        RazorVueNodeKey? key,
        ImmutableHashSet<ILocalSymbol> allowedLocalSymbols,
        ImmutableHashSet<IParameterSymbol> allowedParameterSymbols)
    {
        if (attributes.IsDefaultOrEmpty)
            return ApplyNodeKey(OptionalJsArgument.Missing, key, allowedLocalSymbols, allowedParameterSymbols);

        var segments = new List<string>();
        var objectEntries = new List<string>();
        var containsSpread = false;
        foreach (var attributeEntry in attributes)
        {
            switch (attributeEntry)
            {
                case RazorVueAttributeNode attribute:
                    var name = GetElementAttributeRuntimeName(attribute);
                    var value = attribute.Value is null
                        ? "true"
                        : EmitCapturedScopedExpression(attribute.Value!, attribute.CapturedBindings, allowedLocalSymbols, allowedParameterSymbols);
                    if (attribute.Value is not null)
                    {
                        value = WrapElementEventHandler(
                            value,
                            attribute.EventModifiers,
                            allowedLocalSymbols,
                            allowedParameterSymbols,
                            preferIifeForStaticModifiers: false);
                    }

                    objectEntries.Add(ToJavaScriptString(name) + ": " + value);
                    break;
                case RazorVueAttributeSpreadNode spread:
                    containsSpread = true;
                    FlushObjectEntries(segments, objectEntries);
                    segments.Add(EmitCapturedScopedExpression(spread.Expression, spread.CapturedBindings, allowedLocalSymbols, allowedParameterSymbols));
                    break;
            }
        }

        FlushObjectEntries(segments, objectEntries);
        return ApplyNodeKey(BuildPropsArgument(segments, containsSpread), key, allowedLocalSymbols, allowedParameterSymbols);
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
                segments.Add(EmitCapturedScopedExpression(spread.Expression, spread.CapturedBindings, allowedLocalSymbols, allowedParameterSymbols));
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
                    var slotExpression = EmitCapturedScopedExpression(attribute.Value!, attribute.CapturedBindings, allowedLocalSymbols, allowedParameterSymbols);
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

            objectEntries.Add(ToJavaScriptString(name) + ": " + (attribute.Value is null
                ? "true"
                : EmitCapturedScopedExpression(attribute.Value!, attribute.CapturedBindings, allowedLocalSymbols, allowedParameterSymbols)));
        }

        FlushObjectEntries(segments, objectEntries);
        return BuildPropsArgument(segments, containsSpread);
    }

    private OptionalJsArgument ApplyNodeKey(
        OptionalJsArgument props,
        RazorVueNodeKey? key,
        ImmutableHashSet<ILocalSymbol> allowedLocalSymbols,
        ImmutableHashSet<IParameterSymbol> allowedParameterSymbols)
    {
        if (key is null)
            return props;

        var keyEntry = "{ \"key\": " + EmitCapturedScopedExpression(key.Expression, key.CapturedBindings, allowedLocalSymbols, allowedParameterSymbols) + " }";
        if (!props.HasValue)
            return new OptionalJsArgument(keyEntry, true);

        return new OptionalJsArgument(
            RazorVueAttributeMergeHelper.BuildInvocation([props.Expression, keyEntry]),
            true);
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

    private static string GetElementAttributeRuntimeName(RazorVueAttributeNode attribute)
        => IsElementDomEventAttribute(attribute, out var eventName)
            ? RazorVueDomEventName.ToVueHandlerPropName(eventName)
            : attribute.Name;

    private static bool IsElementDomEventAttribute(RazorVueAttributeNode attribute, out string eventName)
    {
        if (attribute.EventModifiers.HasAny)
            return RazorVueDomEventName.TryNormalizeBlazorEventAttributeName(attribute.Name, out eventName);

        if (attribute.Value is not null &&
            IsEventCallbackLike(attribute.Value) &&
            RazorVueDomEventName.TryNormalizeBlazorEventAttributeName(attribute.Name, out eventName))
        {
            return true;
        }

        eventName = string.Empty;
        return false;
    }

    private static bool IsEventCallbackLike(IOperation operation)
    {
        var type = Unwrap(operation)?.Type;
        if (type is INamedTypeSymbol namedType)
        {
            var originalDefinition = namedType.OriginalDefinition.ToDisplayString();
            return string.Equals(
                       originalDefinition,
                       "Microsoft.AspNetCore.Components.EventCallback",
                       StringComparison.Ordinal) ||
                   string.Equals(
                       originalDefinition,
                       "Microsoft.AspNetCore.Components.EventCallback<TValue>",
                       StringComparison.Ordinal);
        }

        return type?.TypeKind == TypeKind.Delegate;
    }

    private string WrapElementEventHandler(
        string handlerExpression,
        RazorVueEventModifiers modifiers,
        ImmutableHashSet<ILocalSymbol> allowedLocalSymbols,
        ImmutableHashSet<IParameterSymbol> allowedParameterSymbols,
        bool preferIifeForStaticModifiers)
    {
        if (!modifiers.HasAny)
            return handlerExpression;

        var preventDefaultExpression = EmitEventModifierValue(
            modifiers.PreventDefault,
            allowedLocalSymbols,
            allowedParameterSymbols);
        var stopPropagationExpression = EmitEventModifierValue(
            modifiers.StopPropagation,
            allowedLocalSymbols,
            allowedParameterSymbols);

        var requiresIife = preferIifeForStaticModifiers ||
            RequiresEventModifierAlias(preventDefaultExpression) ||
            RequiresEventModifierAlias(stopPropagationExpression);

        if (!requiresIife)
        {
            var statements = new List<string>();
            AppendEventModifierInvocationStatements(statements, preventDefaultExpression, stopPropagationExpression);
            statements.Add("return (" + handlerExpression + ")(__event);");
            return "(__event) => { " + string.Join(" ", statements) + " }";
        }

        var aliases = new List<string> { "const __jazorHandler = " + handlerExpression + ";" };
        var invocations = new List<string>();
        AppendEventModifierAlias(
            aliases,
            invocations,
            "__jazorPreventDefault",
            preventDefaultExpression,
            "__event?.preventDefault?.();");
        AppendEventModifierAlias(
            aliases,
            invocations,
            "__jazorStopPropagation",
            stopPropagationExpression,
            "__event?.stopPropagation?.();");

        return "(() => { " +
               string.Join(" ", aliases) +
               " return (__event) => { " +
               string.Join(" ", invocations) +
               " return __jazorHandler(__event); }; })()";
    }

    private string EmitEventModifierValue(
        RazorVueEventModifierBinding? binding,
        ImmutableHashSet<ILocalSymbol> allowedLocalSymbols,
        ImmutableHashSet<IParameterSymbol> allowedParameterSymbols)
        => binding is null
            ? "false"
            : EmitCapturedScopedExpression(
                binding.Value,
                binding.CapturedBindings,
                allowedLocalSymbols,
                allowedParameterSymbols);

    private static bool RequiresEventModifierAlias(string expression)
        => !string.Equals(expression, "true", StringComparison.Ordinal) &&
           !string.Equals(expression, "false", StringComparison.Ordinal);

    private static void AppendEventModifierInvocationStatements(
        List<string> statements,
        string preventDefaultExpression,
        string stopPropagationExpression)
    {
        AppendEventModifierInvocationStatement(
            statements,
            preventDefaultExpression,
            "__event?.preventDefault?.();");
        AppendEventModifierInvocationStatement(
            statements,
            stopPropagationExpression,
            "__event?.stopPropagation?.();");
    }

    private static void AppendEventModifierInvocationStatement(
        List<string> statements,
        string expression,
        string invocation)
    {
        if (string.Equals(expression, "false", StringComparison.Ordinal))
            return;

        if (string.Equals(expression, "true", StringComparison.Ordinal))
        {
            statements.Add(invocation);
            return;
        }

        statements.Add("if (" + expression + ") " + invocation);
    }

    private static void AppendEventModifierAlias(
        List<string> aliases,
        List<string> invocations,
        string alias,
        string expression,
        string invocation)
    {
        if (string.Equals(expression, "false", StringComparison.Ordinal))
            return;

        if (string.Equals(expression, "true", StringComparison.Ordinal))
        {
            invocations.Add(invocation);
            return;
        }

        aliases.Add("const " + alias + " = " + expression + ";");
        invocations.Add("if (" + alias + ") " + invocation);
    }

    private string EmitEventModifiersObject(
        RazorVueEventModifiers modifiers,
        ImmutableHashSet<ILocalSymbol> allowedLocalSymbols,
        ImmutableHashSet<IParameterSymbol> allowedParameterSymbols)
    {
        if (!modifiers.HasAny)
            return "{ preventDefault: false, stopPropagation: false }";

        return "{ preventDefault: " +
               EmitEventModifierValue(modifiers.PreventDefault, allowedLocalSymbols, allowedParameterSymbols) +
               ", stopPropagation: " +
               EmitEventModifierValue(modifiers.StopPropagation, allowedLocalSymbols, allowedParameterSymbols) +
               " }";
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

    private static string EmitCurrentComponentSlotReference(VueSlotDescriptor slotDescriptor)
    {
        var slotAccess = GetSlotAccessExpression(slotDescriptor.Name);
        return slotAccess + " ?? null";
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

    private string EmitCapturedScopedExpression(
        IOperation operation,
        ImmutableArray<RazorVueCapturedValueBinding> capturedBindings,
        ImmutableHashSet<ILocalSymbol> allowedLocalSymbols,
        ImmutableHashSet<IParameterSymbol> allowedParameterSymbols)
    {
        if (capturedBindings.IsDefaultOrEmpty)
            return EmitScopedExpression(operation, allowedLocalSymbols, allowedParameterSymbols);

        if (TryEmitDirectCapturedInitializer(operation, capturedBindings, allowedLocalSymbols, allowedParameterSymbols, out var directExpression))
            return directExpression;

        var aliasMap = new Dictionary<IParameterSymbol, string>(SymbolEqualityComparer.Default);
        foreach (var binding in capturedBindings)
            aliasMap[binding.ParameterSymbol] = binding.ParameterSymbol.Name;
        var capturedParameterScope = allowedParameterSymbols;
        foreach (var binding in capturedBindings)
            capturedParameterScope = RazorVueTemplateExpressionScopeValidator.AddIfPresent(capturedParameterScope, binding.ParameterSymbol);
        var expression = WithScopedParameterAliases(
            aliasMap,
            () => EmitScopedExpression(operation, allowedLocalSymbols, capturedParameterScope));

        for (var index = capturedBindings.Length - 1; index >= 0; index--)
        {
            var binding = capturedBindings[index];
            var initializer = EmitScopedExpression(binding.Initializer, allowedLocalSymbols, allowedParameterSymbols);
            expression = "((" + aliasMap[binding.ParameterSymbol] + ") => " + expression + ")(" + initializer + ")";
        }

        return expression;
    }

    private bool TryEmitDirectCapturedInitializer(
        IOperation operation,
        ImmutableArray<RazorVueCapturedValueBinding> capturedBindings,
        ImmutableHashSet<ILocalSymbol> allowedLocalSymbols,
        ImmutableHashSet<IParameterSymbol> allowedParameterSymbols,
        out string expression)
    {
        expression = string.Empty;
        if (capturedBindings.Length != 1 ||
            Unwrap(operation) is not IParameterReferenceOperation parameterReference ||
            !SymbolEqualityComparer.Default.Equals(parameterReference.Parameter, capturedBindings[0].ParameterSymbol))
        {
            return false;
        }

        expression = EmitScopedExpression(capturedBindings[0].Initializer, allowedLocalSymbols, allowedParameterSymbols);
        return true;
    }

    internal string EmitCapturedTemplateExpression(
        IOperation operation,
        ImmutableArray<RazorVueCapturedValueBinding> capturedBindings,
        ImmutableHashSet<ILocalSymbol> allowedLocalSymbols,
        ImmutableHashSet<IParameterSymbol> allowedParameterSymbols)
        => EmitCapturedScopedExpression(operation, capturedBindings, allowedLocalSymbols, allowedParameterSymbols);

    private string EmitFragmentWithTemplateLocals(
        RazorVueRenderFragment fragment,
        ImmutableHashSet<ILocalSymbol> allowedLocalSymbols,
        ImmutableHashSet<IParameterSymbol> allowedParameterSymbols)
    {
        var statements = new List<string>();
        var currentLocalScope = allowedLocalSymbols;
        var currentParameterScope = allowedParameterSymbols;
        statements.Add("const __jazorNodes = [];");

        foreach (var child in fragment.Children)
        {
            if (child is RazorVueLocalDeclarationNode localDeclaration)
            {
                statements.Add(
                    "const " + localDeclaration.LocalSymbol.Name + " = " +
                    EmitScopedExpression(localDeclaration.Initializer, currentLocalScope, currentParameterScope) + ";");
                currentLocalScope = RazorVueTemplateExpressionScopeValidator.AddIfPresent(currentLocalScope, localDeclaration.LocalSymbol);
                continue;
            }

            if (child is RazorVueTemplateScopeNode templateScope)
            {
                statements.Add("__jazorNodes.push(" + EmitTemplateScopeNode(templateScope, currentLocalScope, currentParameterScope) + ");");
                continue;
            }

            statements.Add("__jazorNodes.push(" + EmitNode(child, currentLocalScope, currentParameterScope) + ");");
        }

        statements.Add("return __jazorNodes.length === 0 ? null : (__jazorNodes.length === 1 ? __jazorNodes[0] : __jazorNodes);");
        return "(() => { " + string.Join(" ", statements) + " })()";
    }

    private static bool ContainsTemplateLocalDeclaration(RazorVueRenderFragment fragment)
        => !fragment.Children.IsDefaultOrEmpty &&
           fragment.Children.Any(static child => child is RazorVueLocalDeclarationNode);

    private string EmitTemplateScopeNode(
        RazorVueTemplateScopeNode templateScope,
        ImmutableHashSet<ILocalSymbol> allowedLocalSymbols,
        ImmutableHashSet<IParameterSymbol> allowedParameterSymbols)
    {
        var initializer = EmitScopedExpression(templateScope.Initializer, allowedLocalSymbols, allowedParameterSymbols);
        return "((" + templateScope.ScopeName + ") => " +
               EmitFragment(
                   templateScope.Children,
                   allowedLocalSymbols,
                   RazorVueTemplateExpressionScopeValidator.AddIfPresent(allowedParameterSymbols, templateScope.ScopeParameterSymbol)) +
                ")(" + initializer + ")";
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
        var current = UnwrapDelegateCarrier(operation) ?? Unwrap(operation);
        if (current is not IPropertyReferenceOperation propertyReference ||
            !IsCurrentComponentMember(propertyReference.Property, propertyReference.Instance))
        {
            return false;
        }

        return _slotsByPublicName.TryGetValue(propertyReference.Property.Name, out slotDescriptor);
    }

    private static bool IsRenderFragmentLike(IOperation operation)
        => RazorVueRenderFragmentTypeHelper.IsRenderFragmentType(Unwrap(operation)?.Type);

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
