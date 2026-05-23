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
                    var initializerExpression = TryEmitImperativeRenderFragmentLocalDeclarationInitializer(localDeclaration.Initializer, currentParameterScope)
                        ?? EmitScopedExpression(localDeclaration.Initializer, currentLocalScope, currentParameterScope);
                    statements.Add(
                        "const " + localDeclaration.LocalSymbol.Name + " = " +
                        initializerExpression + ";");
                    currentLocalScope = RazorVueTemplateExpressionScopeValidator.AddIfPresent(currentLocalScope, localDeclaration.LocalSymbol);
                    break;
                default:
                    if (TryEmitImperativeCompatibleNodeStatements(
                            child,
                            builderAlias,
                            currentLocalScope,
                            currentParameterScope,
                            out var nodeStatements))
                    {
                        statements.Add(nodeStatements);
                    }
                    else
                    {
                        statements.Add(
                            builderAlias + ".append(" +
                            EmitImperativeCompatibleNodeValue(child, currentLocalScope, currentParameterScope) +
                            ");");
                    }
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
            RazorVueElementNode element => EmitImperativeCompatibleElementNode(element, allowedLocalSymbols, allowedParameterSymbols),
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

    private string EmitImperativeCompatibleElementNode(
        RazorVueElementNode element,
        ImmutableHashSet<ILocalSymbol> allowedLocalSymbols,
        ImmutableHashSet<IParameterSymbol> allowedParameterSymbols)
    {
        if (!RazorVueOpenNodeReplayHelper.HasScopedReplayOperations(element.ReplayOperations))
        {
            return EmitVNodeCall(
                ToJavaScriptString(element.TagName),
                EmitAttributesArgument(element.Attributes, element.Key, allowedLocalSymbols, allowedParameterSymbols),
                EmitImperativeCompatibleFragmentArgument(element.Children, allowedLocalSymbols, allowedParameterSymbols));
        }

        return EmitImperativeCompatibleOpenElementReplayExpression(
            element.TagName,
            element.ReplayOperations,
            allowedLocalSymbols,
            allowedParameterSymbols);
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

        if (RazorVueOpenNodeReplayHelper.HasScopedReplayOperations(component.ReplayOperations))
        {
            return EmitImperativeCompatibleOpenComponentReplayExpression(
                component,
                descriptor,
                slotsByPublicName,
                emitDescriptorsByAlias,
                allowedLocalSymbols,
                allowedParameterSymbols);
        }

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

        var nestedBuilderAlias = AllocateImperativeScratchName("Context");
        var builder = new StringBuilder();
        builder.Append("(() => {\n");
        builder.Append("const ").Append(nestedBuilderAlias).Append(" = __jazorCreateRenderContext(h);\n");
        var body = EmitImperativeFragmentStatements(fragment, nestedBuilderAlias, allowedLocalSymbols, allowedParameterSymbols);
        if (!string.IsNullOrWhiteSpace(body))
        {
            builder.Append(body);
            if (!body.EndsWith("\n", StringComparison.Ordinal))
                builder.Append('\n');
        }

        builder.Append("return ").Append(nestedBuilderAlias).Append(".finish();\n");
        builder.Append("})()");
        return builder.ToString();
    }

    private string EmitImperativeCompatibleOpenElementReplayExpression(
        string tagName,
        ImmutableArray<RazorVueOpenNodeReplayOperation> replayOperations,
        ImmutableHashSet<ILocalSymbol> allowedLocalSymbols,
        ImmutableHashSet<IParameterSymbol> allowedParameterSymbols)
    {
        var nestedBuilderAlias = AllocateImperativeScratchName("Context");
        var builder = new StringBuilder();
        builder.Append("(() => {\n");
        builder.Append("const ").Append(nestedBuilderAlias).Append(" = __jazorCreateRenderContext(h);\n");
        builder.Append(nestedBuilderAlias).Append(".enterElement(").Append(ToJavaScriptString(tagName)).Append(");\n");
        AppendReplayOperationsStatements(
            builder,
            replayOperations,
            nestedBuilderAlias,
            component: null,
            descriptor: null,
            slotsByPublicName: null,
            emitDescriptorsByAlias: null,
            allowedLocalSymbols,
            allowedParameterSymbols);
        builder.Append(nestedBuilderAlias).Append(".leaveElement();\n");
        builder.Append("return ").Append(nestedBuilderAlias).Append(".finish();\n");
        builder.Append("})()");
        return builder.ToString();
    }

    private string EmitImperativeCompatibleOpenComponentReplayExpression(
        RazorVueComponentNode component,
        VueComponentDescriptor? descriptor,
        ImmutableDictionary<string, VueSlotDescriptor>? slotsByPublicName,
        ImmutableDictionary<string, VueEmitDescriptor>? emitDescriptorsByAlias,
        ImmutableHashSet<ILocalSymbol> allowedLocalSymbols,
        ImmutableHashSet<IParameterSymbol> allowedParameterSymbols)
    {
        _ = slotsByPublicName;
        _ = emitDescriptorsByAlias;

        var nestedBuilderAlias = AllocateImperativeScratchName("Context");
        var builder = new StringBuilder();
        builder.Append("(() => {\n");
        builder.Append("const ").Append(nestedBuilderAlias).Append(" = __jazorCreateRenderContext(h);\n");
        builder.Append(nestedBuilderAlias)
            .Append(".enterComponent(")
            .Append(ResolveComponentReference(component))
            .Append(", ")
            .Append(RazorVueArtifactFactory.CreateImperativeComponentMetadataAlias(component.ComponentName))
            .Append(");\n");
        AppendReplayOperationsStatements(
            builder,
            component.ReplayOperations,
            nestedBuilderAlias,
            component,
            descriptor,
            _componentSlotsByPublicName.TryGetValue(component.ComponentName, out var resolvedSlots) ? resolvedSlots : null,
            _componentEmitDescriptorsByRazorAlias.TryGetValue(component.ComponentName, out var resolvedEmits) ? resolvedEmits : null,
            allowedLocalSymbols,
            allowedParameterSymbols);
        builder.Append(nestedBuilderAlias).Append(".leaveComponent();\n");
        builder.Append("return ").Append(nestedBuilderAlias).Append(".finish();\n");
        builder.Append("})()");
        return builder.ToString();
    }

    private bool TryEmitImperativeCompatibleNodeStatements(
        RazorVueRenderNode node,
        string builderAlias,
        ImmutableHashSet<ILocalSymbol> allowedLocalSymbols,
        ImmutableHashSet<IParameterSymbol> allowedParameterSymbols,
        out string statements)
    {
        statements = string.Empty;
        switch (node)
        {
            case RazorVueElementNode element
                when RazorVueOpenNodeReplayHelper.HasScopedReplayOperations(element.ReplayOperations):
                var elementBuilder = new StringBuilder();
                elementBuilder.Append(builderAlias).Append(".enterElement(").Append(ToJavaScriptString(element.TagName)).AppendLine(");");
                AppendReplayOperationsStatements(
                    elementBuilder,
                    element.ReplayOperations,
                    builderAlias,
                    component: null,
                    descriptor: null,
                    slotsByPublicName: null,
                    emitDescriptorsByAlias: null,
                    allowedLocalSymbols,
                    allowedParameterSymbols);
                elementBuilder.Append(builderAlias).Append(".leaveElement();");
                statements = elementBuilder.ToString();
                return true;
            case RazorVueComponentNode component
                when RazorVueOpenNodeReplayHelper.HasScopedReplayOperations(component.ReplayOperations):
                _resolvedComponents.TryGetValue(component.ComponentName, out var descriptor);
                _componentSlotsByPublicName.TryGetValue(component.ComponentName, out var slotsByPublicName);
                _componentEmitDescriptorsByRazorAlias.TryGetValue(component.ComponentName, out var emitDescriptorsByAlias);
                ValidateComponentAuthoringAttributes(component, descriptor, slotsByPublicName, emitDescriptorsByAlias);
                ValidateDefaultLibrarySlotUsage(component, descriptor, descriptor?.Slots ?? ImmutableArray<VueSlotDescriptor>.Empty);
                ValidateDuplicateLibrarySlotUsage(component, descriptor, descriptor?.Slots ?? ImmutableArray<VueSlotDescriptor>.Empty);

                var componentBuilder = new StringBuilder();
                componentBuilder.Append(builderAlias)
                    .Append(".enterComponent(")
                    .Append(ResolveComponentReference(component))
                    .Append(", ")
                    .Append(RazorVueArtifactFactory.CreateImperativeComponentMetadataAlias(component.ComponentName))
                    .AppendLine(");");
                AppendReplayOperationsStatements(
                    componentBuilder,
                    component.ReplayOperations,
                    builderAlias,
                    component,
                    descriptor,
                    slotsByPublicName,
                    emitDescriptorsByAlias,
                    allowedLocalSymbols,
                    allowedParameterSymbols);
                componentBuilder.Append(builderAlias).Append(".leaveComponent();");
                statements = componentBuilder.ToString();
                return true;
            default:
                return false;
        }
    }

    private void AppendReplayOperationsStatements(
        StringBuilder builder,
        ImmutableArray<RazorVueOpenNodeReplayOperation> operations,
        string builderAlias,
        RazorVueComponentNode? component,
        VueComponentDescriptor? descriptor,
        ImmutableDictionary<string, VueSlotDescriptor>? slotsByPublicName,
        ImmutableDictionary<string, VueEmitDescriptor>? emitDescriptorsByAlias,
        ImmutableHashSet<ILocalSymbol> allowedLocalSymbols,
        ImmutableHashSet<IParameterSymbol> allowedParameterSymbols)
    {
        if (operations.IsDefaultOrEmpty)
            return;

        foreach (var operation in operations)
        {
            AppendReplayOperationStatement(
                builder,
                operation,
                builderAlias,
                component,
                descriptor,
                slotsByPublicName,
                emitDescriptorsByAlias,
                allowedLocalSymbols,
                allowedParameterSymbols);
        }
    }

    private void AppendReplayOperationStatement(
        StringBuilder builder,
        RazorVueOpenNodeReplayOperation operation,
        string builderAlias,
        RazorVueComponentNode? component,
        VueComponentDescriptor? descriptor,
        ImmutableDictionary<string, VueSlotDescriptor>? slotsByPublicName,
        ImmutableDictionary<string, VueEmitDescriptor>? emitDescriptorsByAlias,
        ImmutableHashSet<ILocalSymbol> allowedLocalSymbols,
        ImmutableHashSet<IParameterSymbol> allowedParameterSymbols)
    {
        switch (operation)
        {
            case RazorVueOpenNodeAttributeReplayOperation attributeOperation:
                builder.Append(EmitReplayAttributeStatement(
                    builderAlias,
                    attributeOperation.Attribute,
                    component,
                    descriptor,
                    slotsByPublicName,
                    emitDescriptorsByAlias,
                    allowedLocalSymbols,
                    allowedParameterSymbols));
                builder.Append('\n');
                break;
            case RazorVueOpenNodeKeyReplayOperation keyOperation:
                if (!keyOperation.KeyAssigned)
                    break;

                builder.Append(builderAlias)
                    .Append(".setKey(")
                    .Append(keyOperation.Key is null
                        ? "null"
                        : EmitCapturedScopedExpression(
                            keyOperation.Key.Expression,
                            keyOperation.Key.CapturedBindings,
                            allowedLocalSymbols,
                            allowedParameterSymbols))
                    .AppendLine(");");
                break;
            case RazorVueOpenNodeSlotTemplateReplayOperation slotTemplateOperation:
                if (component is null)
                    throw new InvalidOperationException("Slot template replay requires an open component frame.");

                builder.Append(EmitReplayComponentSlotTemplateStatement(
                    builderAlias,
                    component,
                    descriptor,
                    slotTemplateOperation.SlotTemplate,
                    allowedLocalSymbols,
                    allowedParameterSymbols));
                builder.Append('\n');
                break;
            case RazorVueOpenNodeImplicitDefaultSlotAssignmentReplayOperation assignmentOperation:
                if (component is null)
                    throw new InvalidOperationException("Implicit default slot replay requires an open component frame.");

                builder.Append(EmitReplayComponentDefaultSlotStatement(
                    builderAlias,
                    component,
                    descriptor,
                    assignmentOperation.Assignment.Children,
                    allowedLocalSymbols,
                    allowedParameterSymbols));
                builder.Append('\n');
                break;
            case RazorVueOpenNodeAmbientDefaultSlotChildReplayOperation ambientChildOperation:
                if (TryEmitImperativeCompatibleNodeStatements(
                        ambientChildOperation.Child,
                        builderAlias,
                        allowedLocalSymbols,
                        allowedParameterSymbols,
                        out var ambientChildStatements))
                {
                    builder.Append(ambientChildStatements);
                    if (!ambientChildStatements.EndsWith("\n", StringComparison.Ordinal))
                        builder.Append('\n');
                }
                else
                {
                    builder.Append(builderAlias)
                        .Append(".append(")
                        .Append(EmitImperativeCompatibleNodeValue(
                            ambientChildOperation.Child,
                            allowedLocalSymbols,
                            allowedParameterSymbols))
                        .AppendLine(");");
                }
                break;
            case RazorVueOpenNodeAmbientDefaultSlotFragmentReplayOperation ambientFragmentOperation:
                if (component is null)
                    throw new InvalidOperationException("Ambient default slot fragment replay requires an open component frame.");

                builder.Append(EmitReplayComponentDefaultSlotStatement(
                    builderAlias,
                    component,
                    descriptor,
                    ambientFragmentOperation.Children,
                    allowedLocalSymbols,
                    allowedParameterSymbols));
                builder.Append('\n');
                break;
            case RazorVueOpenNodeChildReplayOperation childOperation:
                if (TryEmitImperativeCompatibleNodeStatements(
                        childOperation.Child,
                        builderAlias,
                        allowedLocalSymbols,
                        allowedParameterSymbols,
                        out var childStatements))
                {
                    builder.Append(childStatements);
                    if (!childStatements.EndsWith("\n", StringComparison.Ordinal))
                        builder.Append('\n');
                }
                else
                {
                    builder.Append(builderAlias)
                        .Append(".append(")
                        .Append(EmitImperativeCompatibleNodeValue(
                            childOperation.Child,
                            allowedLocalSymbols,
                            allowedParameterSymbols))
                        .AppendLine(");");
                }
                break;
            case RazorVueOpenNodeScopedReplayOperation scopedOperation:
                var parameterAliases = new Dictionary<IParameterSymbol, string>(SymbolEqualityComparer.Default);
                foreach (var binding in scopedOperation.CapturedBindings)
                    parameterAliases[binding.ParameterSymbol] = binding.ParameterSymbol.Name;
                var scopedExpression = WithScopedParameterAliases(
                    parameterAliases,
                    () =>
                    {
                        var nestedBuilder = new StringBuilder();
                        AppendReplayOperationsStatements(
                            nestedBuilder,
                            scopedOperation.Operations,
                            builderAlias,
                            component,
                            descriptor,
                            slotsByPublicName,
                            emitDescriptorsByAlias,
                            allowedLocalSymbols,
                            allowedParameterSymbols.Union(scopedOperation.CapturedBindings.Select(static binding => binding.ParameterSymbol)));
                        return nestedBuilder.ToString();
                    });

                var wrappedBody = scopedExpression;
                for (var index = scopedOperation.CapturedBindings.Length - 1; index >= 0; index--)
                {
                    var binding = scopedOperation.CapturedBindings[index];
                    var initializer = EmitScopedExpression(binding.Initializer, allowedLocalSymbols, allowedParameterSymbols);
                    wrappedBody =
                        "((" + binding.ParameterSymbol.Name + ") => {\n" +
                        wrappedBody +
                        "})(" + initializer + ");\n";
                }

                builder.Append(wrappedBody);
                break;
        }
    }

    private string EmitReplayAttributeStatement(
        string builderAlias,
        RazorVueAttributeEntry attributeEntry,
        RazorVueComponentNode? component,
        VueComponentDescriptor? descriptor,
        ImmutableDictionary<string, VueSlotDescriptor>? slotsByPublicName,
        ImmutableDictionary<string, VueEmitDescriptor>? emitDescriptorsByAlias,
        ImmutableHashSet<ILocalSymbol> allowedLocalSymbols,
        ImmutableHashSet<IParameterSymbol> allowedParameterSymbols)
    {
        if (attributeEntry is RazorVueAttributeSpreadNode spread)
        {
            if (component is not null)
                ValidateComponentSpreadTarget(component, descriptor, spread);

            return builderAlias + ".mergeAttributes(" +
                   EmitCapturedScopedExpression(spread.Expression, spread.CapturedBindings, allowedLocalSymbols, allowedParameterSymbols) +
                   ");";
        }

        var attribute = (RazorVueAttributeNode)attributeEntry;
        var valueExpression = attribute.Value is null
            ? "true"
            : EmitCapturedScopedExpression(attribute.Value, attribute.CapturedBindings, allowedLocalSymbols, allowedParameterSymbols);

        if (component is null)
            return builderAlias + ".setAttribute(" + ToJavaScriptString(attribute.Name) + ", " + valueExpression + ");";

        if (descriptor is not null &&
            VueSlotResolver.TryResolve(descriptor.Slots, attribute.Name, out var slot))
        {
            if (attribute.Value is null)
            {
                throw CreateAuthoringIssue(
                    RazorVueIssueCode.MissingSlotValue,
                    $"Child content parameter '{attribute.Name}' on component '{GetComponentDisplayName(component)}' must be assigned a value.",
                    attribute);
            }

            return builderAlias + ".setComponentParameter(" +
                   ToJavaScriptString(attribute.Name) + ", " +
                   EmitReplayComponentSlotParameterValue(
                       component,
                       slot,
                       attribute,
                       allowedLocalSymbols,
                       allowedParameterSymbols) +
                   ");";
        }

        var runtimeName = attribute.Name;
        if (_componentEmitsByRazorAlias.TryGetValue(component.ComponentName, out var emitsByAlias) &&
            emitsByAlias.TryGetValue(runtimeName, out var vueEventName))
        {
            runtimeName = vueEventName;
        }
        else if (descriptor is not null &&
                 VuePropResolver.TryResolve(descriptor.Props, runtimeName, out var prop))
        {
            runtimeName = prop.PropName;
        }

        _ = slotsByPublicName;
        _ = emitDescriptorsByAlias;
        return builderAlias + ".setComponentParameter(" + ToJavaScriptString(runtimeName) + ", " + valueExpression + ");";
    }

    private string EmitReplayComponentSlotParameterValue(
        RazorVueComponentNode component,
        VueSlotResolution slot,
        RazorVueAttributeNode attribute,
        ImmutableHashSet<ILocalSymbol> allowedLocalSymbols,
        ImmutableHashSet<IParameterSymbol> allowedParameterSymbols)
    {
        var slotDescriptor = slot.Descriptor;
        if (TryGetCurrentComponentSlotDescriptor(attribute.Value!, out var currentSlot))
        {
            return "__jazorCreateSlotReference(" +
                   EmitCurrentComponentSlotReference(currentSlot) + ", " +
                   (currentSlot.Parameters.IsDefaultOrEmpty ? "false" : "true") + ")";
        }

        if (!slotDescriptor.Parameters.IsDefaultOrEmpty &&
            !IsCallableSlotValue(attribute.Value!))
        {
            throw CreateAuthoringIssue(
                RazorVueIssueCode.SlotContextMisuse,
                $"Child content parameter '{attribute.Name}' on component '{GetComponentDisplayName(component)}' expects a callable template that accepts '{DescribeSlotContext(slotDescriptor)}'.",
                attribute);
        }

        var slotExpression = EmitCapturedScopedExpression(
            attribute.Value!,
            attribute.CapturedBindings,
            allowedLocalSymbols,
            allowedParameterSymbols);

        if (slotDescriptor.Parameters.IsDefaultOrEmpty || !IsCallableSlotExpression(attribute.Value!))
            return "() => " + NormalizeArrowFunctionExpressionBody(slotExpression);

        var slotParameterName = slotDescriptor.Parameters[0].Name;
        return "(" + slotParameterName + ") => " + NormalizeArrowFunctionExpressionBody(slotExpression + "(" + slotParameterName + ")");
    }

    private string EmitReplayComponentSlotTemplateStatement(
        string builderAlias,
        RazorVueComponentNode component,
        VueComponentDescriptor? descriptor,
        RazorVueComponentSlotTemplateNode slotTemplate,
        ImmutableHashSet<ILocalSymbol> allowedLocalSymbols,
        ImmutableHashSet<IParameterSymbol> allowedParameterSymbols)
    {
        string parameterName;
        string valueExpression;

        if (descriptor is not null &&
            VueSlotResolver.TryResolve(descriptor.Slots, slotTemplate.PublicName, out var slot))
        {
            var slotDescriptor = slot.Descriptor;
            if (slotDescriptor.Parameters.IsDefaultOrEmpty)
            {
                parameterName = string.Empty;
                valueExpression = "() => " + EmitImperativeCompatibleFragmentExpression(
                    slotTemplate.Children,
                    allowedLocalSymbols,
                    allowedParameterSymbols);
            }
            else
            {
                parameterName = slotTemplate.ParameterName ?? slotDescriptor.Parameters[0].Name;
                valueExpression = "(" + parameterName + ") => " + EmitImperativeCompatibleFragmentExpression(
                    slotTemplate.Children,
                    allowedLocalSymbols,
                    RazorVueTemplateExpressionScopeValidator.AddIfPresent(allowedParameterSymbols, slotTemplate.ParameterSymbol));
            }
        }
        else
        {
            var slotName = string.Equals(slotTemplate.PublicName, "ChildContent", StringComparison.Ordinal)
                ? "default"
                : char.ToLowerInvariant(slotTemplate.PublicName[0]) + slotTemplate.PublicName.Substring(1);
            parameterName = slotTemplate.ParameterName ?? string.Empty;
            valueExpression = string.IsNullOrWhiteSpace(parameterName)
                ? "() => " + EmitImperativeCompatibleFragmentExpression(slotTemplate.Children, allowedLocalSymbols, allowedParameterSymbols)
                : "(" + parameterName + ") => " + EmitImperativeCompatibleFragmentExpression(
                    slotTemplate.Children,
                    allowedLocalSymbols,
                    RazorVueTemplateExpressionScopeValidator.AddIfPresent(allowedParameterSymbols, slotTemplate.ParameterSymbol));
            return builderAlias + ".setComponentParameter(" + ToJavaScriptString(slotName) + ", " + valueExpression + ");";
        }

        return builderAlias + ".setComponentParameter(" + ToJavaScriptString(slotTemplate.PublicName) + ", " + valueExpression + ");";
    }

    private string EmitReplayComponentDefaultSlotStatement(
        string builderAlias,
        RazorVueComponentNode component,
        VueComponentDescriptor? descriptor,
        RazorVueRenderFragment defaultSlotFragment,
        ImmutableHashSet<ILocalSymbol> allowedLocalSymbols,
        ImmutableHashSet<IParameterSymbol> allowedParameterSymbols)
    {
        string valueExpression;
        if (descriptor is not null &&
            VueSlotResolver.TryResolve(descriptor.Slots, "ChildContent", out var defaultSlot) &&
            !defaultSlot.Descriptor.Parameters.IsDefaultOrEmpty)
        {
            var slotParameterName = RazorVueSlotParameterNames.CreateImplicitDefaultSlotParameterName(
                defaultSlot.Descriptor.Parameters[0].Name,
                allowedLocalSymbols,
                allowedParameterSymbols);
            if (TryGetSingleCurrentComponentDefaultSlot(defaultSlotFragment, out var currentSlot))
            {
                valueExpression = "(" + slotParameterName + ") => " + EmitCurrentComponentSlotInvocation(currentSlot, slotParameterName);
            }
            else
            {
                valueExpression = "(" + slotParameterName + ") => " + EmitImperativeCompatibleFragmentExpression(
                    defaultSlotFragment,
                    allowedLocalSymbols,
                    allowedParameterSymbols);
            }
        }
        else if (TryGetSingleCurrentComponentDefaultSlot(defaultSlotFragment, out var currentSlot))
        {
            valueExpression = "() => " + EmitCurrentComponentSlotInvocation(currentSlot);
        }
        else
        {
            valueExpression = "() => " + EmitImperativeCompatibleFragmentExpression(
                defaultSlotFragment,
                allowedLocalSymbols,
                allowedParameterSymbols);
        }

        _ = component;
        return builderAlias + ".setComponentParameter(\"ChildContent\", " + valueExpression + ");";
    }

    private bool ContainsImperativeInNode(RazorVueRenderNode node)
        => node switch
        {
            RazorVueImperativeBlockNode => true,
            RazorVueElementNode element =>
                RazorVueOpenNodeReplayHelper.HasScopedReplayOperations(element.ReplayOperations) ||
                ContainsImperativeRenderBodyCore(element.Children),
            RazorVueComponentNode component =>
                RazorVueOpenNodeReplayHelper.HasScopedReplayOperations(component.ReplayOperations) ||
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
