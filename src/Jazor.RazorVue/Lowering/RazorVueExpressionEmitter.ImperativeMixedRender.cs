using System.Collections.Immutable;
using System.Text;
using Jazor.RazorVue.Descriptor;
using Jazor.RazorVue.RenderTree;
using Microsoft.CodeAnalysis;

namespace Jazor.RazorVue.Lowering;

internal sealed partial class RazorVueExpressionEmitter
{
    private int _imperativeScratchOrdinal;

    private string EmitImperativeFragmentStatements(
        RazorVueRenderFragment fragment,
        string builderAlias,
        ImmutableHashSet<ILocalSymbol> allowedLocalSymbols,
        ImmutableHashSet<IParameterSymbol> allowedParameterSymbols)
    {
        if (fragment.Children.IsDefaultOrEmpty)
            return string.Empty;

        var statements = new List<string>();
        var currentLocalScope = allowedLocalSymbols;
        var currentParameterScope = allowedParameterSymbols;

        foreach (var child in fragment.Children)
        {
            switch (child)
            {
                case RazorVueImperativeBlockNode imperative:
                    statements.Add(EmitImperativeBlockBody(imperative, builderAlias));
                    break;
                case RazorVueLocalDeclarationNode localDeclaration:
                    statements.Add(
                        "const " + localDeclaration.LocalSymbol.Name + " = " +
                        EmitScopedExpression(localDeclaration.Initializer, currentLocalScope, currentParameterScope) + ";");
                    currentLocalScope = RazorVueTemplateExpressionScopeValidator.AddIfPresent(currentLocalScope, localDeclaration.LocalSymbol);
                    break;
                default:
                    statements.Add(
                        builderAlias + ".AddContent(" +
                        EmitImperativeCompatibleNodeValue(child, currentLocalScope, currentParameterScope) +
                        ");");
                    break;
            }
        }

        return string.Join("\n", statements.Where(static statement => !string.IsNullOrWhiteSpace(statement)));
    }

    private string EmitImperativeCompatibleNodeValue(
        RazorVueRenderNode node,
        ImmutableHashSet<ILocalSymbol> allowedLocalSymbols,
        ImmutableHashSet<IParameterSymbol> allowedParameterSymbols)
    {
        if (!ContainsImperativeInNode(node))
            return EmitNode(node, allowedLocalSymbols, allowedParameterSymbols);

        return node switch
        {
            RazorVueElementNode element => EmitVNodeCall(
                ToJavaScriptString(element.TagName),
                EmitAttributesArgument(element.Attributes, element.Key, allowedLocalSymbols, allowedParameterSymbols),
                EmitImperativeCompatibleFragmentArgument(element.Children, allowedLocalSymbols, allowedParameterSymbols)),
            RazorVueComponentNode component => EmitImperativeCompatibleComponentNode(component, allowedLocalSymbols, allowedParameterSymbols),
            RazorVueConditionalNode conditional => EmitImperativeCompatibleFragmentExpression(
                new RazorVueRenderFragment([conditional]),
                allowedLocalSymbols,
                allowedParameterSymbols),
            RazorVueForEachNode loop => EmitImperativeCompatibleFragmentExpression(
                new RazorVueRenderFragment([loop]),
                allowedLocalSymbols,
                allowedParameterSymbols),
            RazorVueForNode loop => EmitImperativeCompatibleFragmentExpression(
                new RazorVueRenderFragment([loop]),
                allowedLocalSymbols,
                allowedParameterSymbols),
            RazorVueTemplateScopeNode templateScope => EmitImperativeCompatibleFragmentExpression(
                new RazorVueRenderFragment([templateScope]),
                allowedLocalSymbols,
                allowedParameterSymbols),
            RazorVueImperativeBlockNode imperative => EmitImperativeCompatibleFragmentExpression(
                new RazorVueRenderFragment([imperative]),
                allowedLocalSymbols,
                allowedParameterSymbols),
            _ => EmitNode(node, allowedLocalSymbols, allowedParameterSymbols)
        };
    }

    private string EmitImperativeCompatibleComponentNode(
        RazorVueComponentNode component,
        ImmutableHashSet<ILocalSymbol> allowedLocalSymbols,
        ImmutableHashSet<IParameterSymbol> allowedParameterSymbols)
    {
        _resolvedComponents.TryGetValue(component.ComponentName, out var descriptor);
        _componentSlotsByPublicName.TryGetValue(component.ComponentName, out var slotsByPublicName);
        _componentEmitDescriptorsByRazorAlias.TryGetValue(component.ComponentName, out var emitDescriptorsByAlias);

        ValidateComponentAuthoringAttributes(component, descriptor, slotsByPublicName, emitDescriptorsByAlias);
        ValidateDefaultLibrarySlotUsage(component, descriptor, descriptor?.Slots ?? ImmutableArray<VueSlotDescriptor>.Empty);
        ValidateDuplicateLibrarySlotUsage(component, descriptor, descriptor?.Slots ?? ImmutableArray<VueSlotDescriptor>.Empty);

        var slotEntries = new List<string>();
        if (HasAnyDefaultSlotContent(component))
            slotEntries.Add(EmitImperativeCompatibleDefaultSlotEntry(component, descriptor, allowedLocalSymbols, allowedParameterSymbols));
        AppendImperativeCompatibleExplicitSlotTemplates(component, descriptor, slotEntries, allowedLocalSymbols, allowedParameterSymbols);

        var attributes = EmitAttributesArgument(component.Attributes, component, slotEntries, allowedLocalSymbols, allowedParameterSymbols);
        var slots = slotEntries.Count == 0
            ? OptionalJsArgument.Missing
            : new OptionalJsArgument("{ " + string.Join(", ", slotEntries) + " }", true);

        return EmitVNodeCall(
            ResolveComponentReference(component),
            ApplyNodeKey(attributes, component.Key, allowedLocalSymbols, allowedParameterSymbols),
            slots);
    }

    private string EmitImperativeCompatibleDefaultSlotEntry(
        RazorVueComponentNode component,
        VueComponentDescriptor? descriptor,
        ImmutableHashSet<ILocalSymbol> allowedLocalSymbols,
        ImmutableHashSet<IParameterSymbol> allowedParameterSymbols)
    {
        var defaultSlotFragment = GetDefaultSlotFragment(component);
        var prefix = "default: () => ";
        if (descriptor is not null &&
            VueSlotResolver.TryResolve(descriptor.Slots, "ChildContent", out var defaultSlot) &&
            !defaultSlot.Descriptor.Parameters.IsDefaultOrEmpty)
        {
            var slotParameterName = RazorVueSlotParameterNames.CreateImplicitDefaultSlotParameterName(
                defaultSlot.Descriptor.Parameters[0].Name,
                allowedLocalSymbols,
                allowedParameterSymbols);
            if (TryGetSingleCurrentComponentDefaultSlot(defaultSlotFragment, out var currentSlot))
                return "default: (" + slotParameterName + ") => " + EmitCurrentComponentSlotInvocation(currentSlot, slotParameterName);

            prefix = "default: (" + slotParameterName + ") => ";
        }

        return prefix + EmitImperativeCompatibleFragmentExpression(defaultSlotFragment, allowedLocalSymbols, allowedParameterSymbols);
    }

    private void AppendImperativeCompatibleExplicitSlotTemplates(
        RazorVueComponentNode component,
        VueComponentDescriptor? descriptor,
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
                    slotEntries.Add(
                        FormatObjectPropertyKey(slot.SlotName) + ": () => " +
                        EmitImperativeCompatibleFragmentExpression(slotTemplate.Children, allowedLocalSymbols, allowedParameterSymbols));
                }
                else
                {
                    var slotParameterName = slotTemplate.ParameterName ?? slotDescriptor.Parameters[0].Name;
                    slotEntries.Add(
                        FormatObjectPropertyKey(slot.SlotName) + ": (" + slotParameterName + ") => " +
                        EmitImperativeCompatibleFragmentExpression(
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
            {
                slotEntries.Add(
                    FormatObjectPropertyKey(slotName) + ": () => " +
                    EmitImperativeCompatibleFragmentExpression(slotTemplate.Children, allowedLocalSymbols, allowedParameterSymbols));
            }
            else
            {
                slotEntries.Add(
                    FormatObjectPropertyKey(slotName) + ": (" + slotTemplate.ParameterName + ") => " +
                    EmitImperativeCompatibleFragmentExpression(
                        slotTemplate.Children,
                        allowedLocalSymbols,
                        RazorVueTemplateExpressionScopeValidator.AddIfPresent(allowedParameterSymbols, slotTemplate.ParameterSymbol)));
            }
        }
    }

    private OptionalJsArgument EmitImperativeCompatibleFragmentArgument(
        RazorVueRenderFragment fragment,
        ImmutableHashSet<ILocalSymbol> allowedLocalSymbols,
        ImmutableHashSet<IParameterSymbol> allowedParameterSymbols)
    {
        if (!ContainsImperativeRenderBodyCore(fragment))
            return EmitFragmentArgument(fragment, allowedLocalSymbols, allowedParameterSymbols);

        return new OptionalJsArgument(
            EmitImperativeCompatibleFragmentExpression(fragment, allowedLocalSymbols, allowedParameterSymbols),
            true);
    }

    private string EmitImperativeCompatibleFragmentExpression(
        RazorVueRenderFragment fragment,
        ImmutableHashSet<ILocalSymbol> allowedLocalSymbols,
        ImmutableHashSet<IParameterSymbol> allowedParameterSymbols)
    {
        if (!ContainsImperativeRenderBodyCore(fragment))
            return EmitFragment(fragment, allowedLocalSymbols, allowedParameterSymbols);

        var nestedBuilderAlias = AllocateImperativeScratchName("Builder");
        var builder = new StringBuilder();
        builder.Append("(() => {\n");
        builder.Append("const ").Append(nestedBuilderAlias).Append(" = __jazorCreateRenderTreeBuilder(h);\n");
        var body = EmitImperativeFragmentStatements(fragment, nestedBuilderAlias, allowedLocalSymbols, allowedParameterSymbols);
        if (!string.IsNullOrWhiteSpace(body))
        {
            builder.Append(body);
            if (!body.EndsWith("\n", StringComparison.Ordinal))
                builder.Append('\n');
        }

        builder.Append("return ").Append(nestedBuilderAlias).Append(".complete();\n");
        builder.Append("})()");
        return builder.ToString();
    }

    private bool ContainsImperativeInNode(RazorVueRenderNode node)
        => node switch
        {
            RazorVueImperativeBlockNode => true,
            RazorVueElementNode element => ContainsImperativeRenderBodyCore(element.Children),
            RazorVueComponentNode component =>
                ContainsImperativeRenderBodyCore(component.Children) ||
                ContainsImperativeRenderBodyCore(component.AmbientDefaultSlotChildren) ||
                component.SlotTemplates.Any(static slot => ContainsImperativeRenderBodyCore(slot.Children)) ||
                component.ImplicitDefaultSlotAssignments.Any(static assignment => ContainsImperativeRenderBodyCore(assignment.Children)),
            RazorVueConditionalNode conditional =>
                ContainsImperativeRenderBodyCore(conditional.WhenTrue) ||
                ContainsImperativeRenderBodyCore(conditional.WhenFalse),
            RazorVueTemplateScopeNode templateScope => ContainsImperativeRenderBodyCore(templateScope.Children),
            RazorVueForEachNode loop => ContainsImperativeRenderBodyCore(loop.Body),
            RazorVueForNode loop => ContainsImperativeRenderBodyCore(loop.Body),
            _ => false
        };

    private string AllocateImperativeScratchName(string suffix)
        => "__jazorImperative" + suffix + _imperativeScratchOrdinal++;
}
