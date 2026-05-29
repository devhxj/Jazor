using System.Collections.Immutable;
using System.Text;
using Jazor.RazorVue.Canonical;
using Jazor.RazorVue.Descriptor;
using Jazor.RazorVue.RenderTree;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Operations;

namespace Jazor.RazorVue.Lowering;

internal static class RazorVueDescriptorIdentityShapeBuilder
{
    public static string BuildForRenderTree(
        VueComponentDescriptor ownerDescriptor,
        INamedTypeSymbol ownerComponentSymbol,
        Compilation compilation,
        RazorVueRenderFragment renderTree,
        ImmutableDictionary<string, VueComponentDescriptor> resolvedComponents)
    {
        var builder = CreateBaseDescriptorShape(ownerDescriptor);
        AppendRenderTreeKeyShape(builder, renderTree);
        AppendResolvedComponentRuntimeShape(builder, CollectFromRenderTree(ownerComponentSymbol, compilation, renderTree, resolvedComponents));
        return builder.ToString();
    }

    public static string BuildForCanonicalTemplate(
        VueComponentDescriptor ownerDescriptor,
        RazorVueCanonicalTemplateFragment template)
    {
        var builder = CreateBaseDescriptorShape(ownerDescriptor);
        AppendCanonicalKeyShape(builder, template);
        AppendResolvedComponentRuntimeShape(builder, CollectFromCanonicalTemplate(template));
        return builder.ToString();
    }

    private static void AppendRenderTreeKeyShape(StringBuilder builder, RazorVueRenderFragment fragment)
    {
        if (fragment.Children.IsDefaultOrEmpty)
            return;

        foreach (var child in fragment.Children)
            AppendRenderTreeKeyShape(builder, child);
    }

    private static void AppendRenderTreeKeyShape(StringBuilder builder, RazorVueRenderNode node)
    {
        switch (node)
        {
            case RazorVueElementNode element:
                if (element.Key is not null)
                    builder.AppendLine("render:key:" + element.Key.Expression.Syntax.ToString());
                AppendRenderTreeKeyShape(builder, element.Children);
                break;
            case RazorVueComponentNode component:
                if (component.Key is not null)
                    builder.AppendLine("render:key:" + component.Key.Expression.Syntax.ToString());
                AppendRenderTreeKeyShape(builder, component.Children);
                foreach (var slotTemplate in component.SlotTemplates)
                    AppendRenderTreeKeyShape(builder, slotTemplate.Children);
                foreach (var implicitDefaultSlotAssignment in component.ImplicitDefaultSlotAssignments)
                {
                    builder.AppendLine("render:implicit-default-slot");
                    AppendRenderTreeKeyShape(builder, implicitDefaultSlotAssignment.Children);
                }
                break;
            case RazorVueLocalDeclarationNode localDeclaration:
                builder.AppendLine("render:local:" + localDeclaration.LocalSymbol.Name + "=" + localDeclaration.Initializer.Syntax.ToString());
                break;
            case RazorVueTemplateScopeNode templateScope:
                builder.AppendLine("render:scope:" + templateScope.ScopeName + "=" + templateScope.Initializer.Syntax.ToString());
                AppendRenderTreeKeyShape(builder, templateScope.Children);
                break;
            case RazorVueConditionalNode conditional:
                AppendRenderTreeKeyShape(builder, conditional.WhenTrue);
                AppendRenderTreeKeyShape(builder, conditional.WhenFalse);
                break;
            case RazorVueRecoveredSwitchConditionalNode conditional:
                builder.AppendLine("render:recovered-switch-if:" + conditional.ConditionExpressionText);
                AppendRenderTreeKeyShape(builder, conditional.WhenTrue);
                AppendRenderTreeKeyShape(builder, conditional.WhenFalse);
                break;
            case RazorVueForEachNode loop:
                AppendRenderTreeKeyShape(builder, loop.Body);
                break;
            case RazorVueForNode loop:
                AppendRenderTreeKeyShape(builder, loop.Body);
                break;
            case RazorVueImperativeBlockNode imperative:
                builder.AppendLine("render:imperative:" + imperative.Kind + "|" + string.Join("||", imperative.Operations.Select(static operation => operation.Syntax.ToString())));
                break;
        }
    }

    private static void AppendCanonicalKeyShape(StringBuilder builder, RazorVueCanonicalTemplateFragment fragment)
    {
        if (fragment.Children.IsDefaultOrEmpty)
            return;

        foreach (var child in fragment.Children)
            AppendCanonicalKeyShape(builder, child);
    }

    private static void AppendCanonicalKeyShape(StringBuilder builder, RazorVueCanonicalTemplateNode node)
    {
        switch (node)
        {
            case RazorVueCanonicalElementNode element:
                if (element.Key is not null)
                    builder.AppendLine("canonical:key:" + element.Key.ExpressionText);
                AppendCanonicalKeyShape(builder, element.Children);
                break;
            case RazorVueCanonicalComponentNode component:
                if (component.Key is not null)
                    builder.AppendLine("canonical:key:" + component.Key.ExpressionText);
                AppendCanonicalKeyShape(builder, component.Children);
                foreach (var slot in component.Slots)
                    AppendCanonicalKeyShape(builder, slot.Children);
                break;
            case RazorVueCanonicalLocalDeclarationNode localDeclaration:
                builder.AppendLine("canonical:local:" + localDeclaration.LocalName + "=" + localDeclaration.InitializerExpressionText);
                break;
            case RazorVueCanonicalTemplateScopeNode templateScope:
                builder.AppendLine("canonical:scope:" + templateScope.ScopeName + "=" + templateScope.InitializerExpressionText);
                AppendCanonicalKeyShape(builder, templateScope.Children);
                break;
            case RazorVueCanonicalConditionalNode conditional:
                AppendCanonicalKeyShape(builder, conditional.WhenTrue);
                AppendCanonicalKeyShape(builder, conditional.WhenFalse);
                break;
            case RazorVueCanonicalForEachNode loop:
                AppendCanonicalKeyShape(builder, loop.Body);
                break;
            case RazorVueCanonicalForNode loop:
                AppendCanonicalKeyShape(builder, loop.Body);
                break;
        }
    }

    private static StringBuilder CreateBaseDescriptorShape(VueComponentDescriptor descriptor)
    {
        var builder = new StringBuilder();
        builder.AppendLine(descriptor.FullName);
        builder.AppendLine(descriptor.SourceKind.ToString());
        builder.AppendLine(descriptor.ImportSpecifier);
        builder.AppendLine(descriptor.ExportName);
        foreach (var routeTemplate in descriptor.RouteTemplates)
            builder.AppendLine("route:" + routeTemplate);
        builder.AppendLine("flags:" + descriptor.Flags);
        foreach (var prop in descriptor.Props.OrderBy(static item => item.PublicName, StringComparer.Ordinal))
        {
            builder.AppendLine(
                prop.PublicName + "|" +
                prop.Name + "|" +
                prop.TypeName + "|" +
                prop.Required + "|" +
                prop.AcceptsBinding + "|" +
                (prop.DefaultExpression ?? string.Empty) + "|" +
                prop.DefaultSource + "|" +
                prop.Kind + "|" +
                prop.CaptureUnmatchedValues);
        }

        foreach (var emit in descriptor.Emits.OrderBy(static item => item.RazorAlias, StringComparer.Ordinal))
            builder.AppendLine(emit.RazorAlias + "|" + emit.Name + "|" + emit.PayloadTypeName + "|" + emit.Kind);

        foreach (var slot in descriptor.Slots.OrderBy(static item => item.Name, StringComparer.Ordinal))
        {
            builder.AppendLine(
                slot.PublicName + "|" +
                slot.Name + "|" +
                (slot.NamePattern ?? string.Empty) + "|" +
                slot.PatternOnly + "|" +
                slot.IsDefault + "|" +
                slot.Required + "|" +
                string.Join(",", slot.Parameters.Select(static parameter => parameter.Name + ":" + parameter.TypeName)));
        }

        foreach (var pluginRequirement in descriptor.PluginRequirements.OrderBy(static item => item, StringComparer.Ordinal))
            builder.AppendLine("plugin:" + pluginRequirement);

        return builder;
    }

    private static void AppendResolvedComponentRuntimeShape(
        StringBuilder builder,
        ImmutableArray<ResolvedComponentRuntimeUsage> referencedComponents)
    {
        foreach (var component in referencedComponents)
        {
            builder.AppendLine("resolved:" + component.ComponentKey);
            builder.AppendLine("resolved:sourceKind=" + component.Descriptor.SourceKind);
            builder.AppendLine("resolved:import=" + component.Descriptor.ImportSpecifier);
            builder.AppendLine("resolved:export=" + component.Descriptor.ExportName);
            builder.AppendLine("resolved:container=" + (component.Descriptor.ContainerContractFullName ?? string.Empty));
            builder.AppendLine("resolved:flags=" + component.Descriptor.Flags);

            foreach (var usedProp in component.UsedProps.OrderBy(static item => item.PublicName, StringComparer.Ordinal))
            {
                builder.AppendLine(
                    "resolved:prop:" +
                    usedProp.PublicName + "|" +
                    usedProp.Name + "|" +
                    usedProp.TypeName + "|" +
                    usedProp.Required + "|" +
                    usedProp.AcceptsBinding + "|" +
                    (usedProp.DefaultExpression ?? string.Empty) + "|" +
                    usedProp.DefaultSource + "|" +
                    usedProp.Kind + "|" +
                    usedProp.CaptureUnmatchedValues);
            }

            foreach (var usedEmit in component.UsedEmits.OrderBy(static item => item.RazorAlias, StringComparer.Ordinal))
                builder.AppendLine("resolved:emit:" + (usedEmit.RazorAlias ?? string.Empty) + "|" + usedEmit.Name + "|" + usedEmit.PayloadTypeName + "|" + usedEmit.Kind);

            foreach (var usedSlot in component.UsedSlots.OrderBy(static item => item.PublicName, StringComparer.Ordinal))
            {
                builder.AppendLine(
                    "resolved:slot:" +
                    usedSlot.PublicName + "|" +
                    usedSlot.Name + "|" +
                    (usedSlot.NamePattern ?? string.Empty) + "|" +
                    usedSlot.PatternOnly + "|" +
                    usedSlot.IsDefault + "|" +
                    usedSlot.Required + "|" +
                    string.Join(",", usedSlot.Parameters.Select(static parameter => parameter.Name + ":" + parameter.TypeName)));
            }

            foreach (var style in component.Descriptor.StyleDependencies.OrderBy(static item => item, StringComparer.Ordinal))
                builder.AppendLine("resolved:style:" + style);
            foreach (var pluginRequirement in component.Descriptor.PluginRequirements.OrderBy(static item => item, StringComparer.Ordinal))
                builder.AppendLine("resolved:plugin:" + pluginRequirement);
        }
    }

    private static ImmutableArray<ResolvedComponentRuntimeUsage> CollectFromRenderTree(
        INamedTypeSymbol ownerComponentSymbol,
        Compilation compilation,
        RazorVueRenderFragment renderTree,
        ImmutableDictionary<string, VueComponentDescriptor> resolvedComponents)
    {
        if (resolvedComponents.IsEmpty)
            return ImmutableArray<ResolvedComponentRuntimeUsage>.Empty;

        var usageByComponent = new Dictionary<string, RuntimeUsageBuilder>(StringComparer.Ordinal);
        CollectFromRenderTree(ownerComponentSymbol, compilation, renderTree, resolvedComponents, usageByComponent);
        return BuildResult(usageByComponent);
    }

    private static void CollectFromRenderTree(
        INamedTypeSymbol ownerComponentSymbol,
        Compilation compilation,
        RazorVueRenderFragment fragment,
        ImmutableDictionary<string, VueComponentDescriptor> resolvedComponents,
        Dictionary<string, RuntimeUsageBuilder> usageByComponent)
    {
        if (fragment.Children.IsDefaultOrEmpty)
            return;

        foreach (var child in fragment.Children)
            CollectFromRenderTree(ownerComponentSymbol, compilation, child, resolvedComponents, usageByComponent);
    }

    private static void CollectFromRenderTree(
        INamedTypeSymbol ownerComponentSymbol,
        Compilation compilation,
        RazorVueRenderNode node,
        ImmutableDictionary<string, VueComponentDescriptor> resolvedComponents,
        Dictionary<string, RuntimeUsageBuilder> usageByComponent)
    {
        switch (node)
        {
            case RazorVueElementNode element:
                CollectFromRenderTree(ownerComponentSymbol, compilation, element.Children, resolvedComponents, usageByComponent);
                break;
            case RazorVueComponentNode component:
                CollectFromRenderTreeComponent(component, resolvedComponents, usageByComponent);
                CollectFromRenderTree(ownerComponentSymbol, compilation, component.Children, resolvedComponents, usageByComponent);
                foreach (var slotTemplate in component.SlotTemplates)
                    CollectFromRenderTree(ownerComponentSymbol, compilation, slotTemplate.Children, resolvedComponents, usageByComponent);
                foreach (var implicitDefaultSlotAssignment in component.ImplicitDefaultSlotAssignments)
                    CollectFromRenderTree(ownerComponentSymbol, compilation, implicitDefaultSlotAssignment.Children, resolvedComponents, usageByComponent);
                break;
            case RazorVueLocalDeclarationNode:
                break;
            case RazorVueTemplateScopeNode templateScope:
                CollectFromRenderTree(ownerComponentSymbol, compilation, templateScope.Children, resolvedComponents, usageByComponent);
                break;
            case RazorVueConditionalNode conditional:
                CollectFromRenderTree(ownerComponentSymbol, compilation, conditional.WhenTrue, resolvedComponents, usageByComponent);
                CollectFromRenderTree(ownerComponentSymbol, compilation, conditional.WhenFalse, resolvedComponents, usageByComponent);
                break;
            case RazorVueRecoveredSwitchConditionalNode conditional:
                CollectFromRenderTree(ownerComponentSymbol, compilation, conditional.WhenTrue, resolvedComponents, usageByComponent);
                CollectFromRenderTree(ownerComponentSymbol, compilation, conditional.WhenFalse, resolvedComponents, usageByComponent);
                break;
            case RazorVueForEachNode loop:
                CollectFromRenderTree(ownerComponentSymbol, compilation, loop.Body, resolvedComponents, usageByComponent);
                break;
            case RazorVueForNode loop:
                CollectFromRenderTree(ownerComponentSymbol, compilation, loop.Body, resolvedComponents, usageByComponent);
                break;
            case RazorVueImperativeBlockNode imperative:
                CollectFromImperativeRenderTree(ownerComponentSymbol, compilation, imperative, resolvedComponents, usageByComponent);
                break;
        }
    }

    private static void CollectFromImperativeRenderTree(
        INamedTypeSymbol ownerComponentSymbol,
        Compilation compilation,
        RazorVueImperativeBlockNode imperative,
        ImmutableDictionary<string, VueComponentDescriptor> resolvedComponents,
        Dictionary<string, RuntimeUsageBuilder> usageByComponent)
    {
        var collector = new ImperativeRuntimeUsageCollector(ownerComponentSymbol, compilation, resolvedComponents, usageByComponent);
        collector.Collect(imperative.Operations, imperative.VisibleParameters);
    }

    private static void CollectFromRenderTreeComponent(
        RazorVueComponentNode component,
        ImmutableDictionary<string, VueComponentDescriptor> resolvedComponents,
        Dictionary<string, RuntimeUsageBuilder> usageByComponent)
    {
        if (!resolvedComponents.TryGetValue(component.ComponentName, out var descriptor))
            return;

        var usage = GetOrAddUsage(usageByComponent, component.ComponentName, descriptor);
        foreach (var attributeEntry in component.Attributes)
        {
            if (attributeEntry is RazorVueAttributeSpreadNode)
            {
                usage.MarkAllPropsAndEmits();
                continue;
            }

            var attribute = (RazorVueAttributeNode)attributeEntry;
            if (VueSlotResolver.TryResolve(descriptor.Slots, attribute.Name, out var slotResolution))
            {
                usage.MarkSlot(slotResolution.Descriptor);
                continue;
            }

            if (VuePropResolver.TryResolve(descriptor.Props, attribute.Name, out var propResolution))
            {
                usage.MarkProp(propResolution.Descriptor);
                continue;
            }

            if (!string.IsNullOrWhiteSpace(attribute.Name) &&
                descriptor.Emits.Any(emit => string.Equals(emit.RazorAlias, attribute.Name, StringComparison.Ordinal)))
            {
                usage.MarkEmitByAlias(attribute.Name);
            }
        }

        if (!component.ImplicitDefaultSlotAssignments.IsDefaultOrEmpty &&
            VueSlotResolver.TryResolve(descriptor.Slots, "ChildContent", out var implicitDefaultSlot))
        {
            usage.MarkSlot(implicitDefaultSlot.Descriptor);
        }

        foreach (var slotTemplate in component.SlotTemplates)
        {
            if (VueSlotResolver.TryResolve(descriptor.Slots, slotTemplate.PublicName, out var slotResolution))
                usage.MarkSlot(slotResolution.Descriptor);
        }
    }

    private static ImmutableArray<ResolvedComponentRuntimeUsage> CollectFromCanonicalTemplate(
        RazorVueCanonicalTemplateFragment template)
    {
        var usageByComponent = new Dictionary<string, RuntimeUsageBuilder>(StringComparer.Ordinal);
        CollectFromCanonicalTemplate(template, usageByComponent);
        return BuildResult(usageByComponent);
    }

    private static void CollectFromCanonicalTemplate(
        RazorVueCanonicalTemplateFragment fragment,
        Dictionary<string, RuntimeUsageBuilder> usageByComponent)
    {
        if (fragment.Children.IsDefaultOrEmpty)
            return;

        foreach (var child in fragment.Children)
            CollectFromCanonicalTemplate(child, usageByComponent);
    }

    private static void CollectFromCanonicalTemplate(
        RazorVueCanonicalTemplateNode node,
        Dictionary<string, RuntimeUsageBuilder> usageByComponent)
    {
        switch (node)
        {
            case RazorVueCanonicalElementNode element:
                CollectFromCanonicalTemplate(element.Children, usageByComponent);
                break;
            case RazorVueCanonicalComponentNode component:
                CollectFromCanonicalComponent(component, usageByComponent);
                CollectFromCanonicalTemplate(component.Children, usageByComponent);
                foreach (var slot in component.Slots)
                    CollectFromCanonicalTemplate(slot.Children, usageByComponent);
                break;
            case RazorVueCanonicalLocalDeclarationNode:
                break;
            case RazorVueCanonicalTemplateScopeNode templateScope:
                CollectFromCanonicalTemplate(templateScope.Children, usageByComponent);
                break;
            case RazorVueCanonicalConditionalNode conditional:
                CollectFromCanonicalTemplate(conditional.WhenTrue, usageByComponent);
                CollectFromCanonicalTemplate(conditional.WhenFalse, usageByComponent);
                break;
            case RazorVueCanonicalForEachNode loop:
                CollectFromCanonicalTemplate(loop.Body, usageByComponent);
                break;
            case RazorVueCanonicalForNode loop:
                CollectFromCanonicalTemplate(loop.Body, usageByComponent);
                break;
        }
    }

    private static void CollectFromCanonicalComponent(
        RazorVueCanonicalComponentNode component,
        Dictionary<string, RuntimeUsageBuilder> usageByComponent)
    {
        if (component.ResolvedDescriptor is null)
            return;

        var descriptor = component.ResolvedDescriptor;
        var usage = GetOrAddUsage(usageByComponent, component.ComponentFullName, descriptor);

        foreach (var attributeEntry in component.Attributes)
        {
            if (attributeEntry is RazorVueCanonicalAttributeSpreadBinding)
            {
                usage.MarkAllPropsAndEmits();
                continue;
            }

            var attribute = (RazorVueCanonicalAttributeBinding)attributeEntry;
            switch (attribute.AttributeKind)
            {
                case RazorVueCanonicalAttributeKind.ComponentProp:
                    if (TryResolvePropByRuntimeName(descriptor, attribute.Name, out var prop))
                        usage.MarkProp(prop);
                    break;
                case RazorVueCanonicalAttributeKind.ComponentEvent:
                    if (TryResolveEmitByRuntimeName(descriptor, attribute.Name, out var emit))
                        usage.MarkEmit(emit);
                    break;
            }
        }

        foreach (var slot in component.Slots)
        {
            if (TryResolveSlotByRuntimeName(descriptor, slot.SlotName, out var resolvedSlot))
                usage.MarkSlot(resolvedSlot);
        }
    }

    private static RuntimeUsageBuilder GetOrAddUsage(
        Dictionary<string, RuntimeUsageBuilder> usageByComponent,
        string componentKey,
        VueComponentDescriptor descriptor)
    {
        if (usageByComponent.TryGetValue(componentKey, out var usage))
            return usage;

        usage = new RuntimeUsageBuilder(componentKey, descriptor);
        usageByComponent.Add(componentKey, usage);
        return usage;
    }

    private static ImmutableArray<ResolvedComponentRuntimeUsage> BuildResult(Dictionary<string, RuntimeUsageBuilder> usageByComponent)
        => usageByComponent
            .OrderBy(static pair => pair.Key, StringComparer.Ordinal)
            .Select(static pair => pair.Value.Build())
            .ToImmutableArray();

    private static bool TryGetCurrentComponentSlotForwarding(IOperation? operation, out string slotName)
    {
        slotName = string.Empty;
        if (RazorVueOperationNormalizer.Unwrap(operation) is not IPropertyReferenceOperation propertyReference ||
            propertyReference.Instance is not IInstanceReferenceOperation)
        {
            return false;
        }

        slotName = propertyReference.Property.Name;
        return !string.IsNullOrWhiteSpace(slotName);
    }

    private static bool IsRenderTreeBuilderType(ITypeSymbol? typeSymbol)
        => string.Equals(
            typeSymbol?.ToDisplayString(),
            "Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder",
            StringComparison.Ordinal);

    private static bool IsRenderFragmentType(ITypeSymbol? typeSymbol)
        => RazorVueRenderFragmentTypeHelper.IsUntypedRenderFragmentType(typeSymbol);

    private static bool IsTypedRenderFragmentType(ITypeSymbol? typeSymbol)
        => RazorVueRenderFragmentTypeHelper.IsParameterizedRenderFragmentType(typeSymbol);

    private static bool TryGetAnonymousFunction(IOperation? operation, out IAnonymousFunctionOperation anonymousFunction)
    {
        anonymousFunction = default!;
        var current = RazorVueOperationNormalizer.Unwrap(operation);
        while (current is not null)
        {
            switch (current)
            {
                case IAnonymousFunctionOperation direct:
                    anonymousFunction = direct;
                    return true;
                case IDelegateCreationOperation delegateCreation:
                    current = RazorVueOperationNormalizer.Unwrap(delegateCreation.Target);
                    continue;
                case IConversionOperation conversion when conversion.IsImplicit:
                    current = RazorVueOperationNormalizer.Unwrap(conversion.Operand);
                    continue;
                default:
                    return false;
            }
        }

        return false;
    }

    private static bool TryGetSingleBuilderParameter(
        IAnonymousFunctionOperation anonymousFunction,
        out IParameterSymbol builderParameter)
    {
        builderParameter = anonymousFunction.Symbol.Parameters.FirstOrDefault(
            static parameter => IsRenderTreeBuilderType(parameter.Type))!;
        return builderParameter is not null && anonymousFunction.Symbol.Parameters.Length == 1;
    }

    private static bool TryGetTypedBuilderTemplate(
        IOperation? operation,
        out IAnonymousFunctionOperation outerAnonymousFunction,
        out IAnonymousFunctionOperation builderAnonymousFunction)
    {
        outerAnonymousFunction = default!;
        builderAnonymousFunction = default!;
        if (!TryGetAnonymousFunction(operation, out outerAnonymousFunction) ||
            outerAnonymousFunction.Symbol.Parameters.Length != 1)
        {
            return false;
        }

        if (!TryGetReturnedAnonymousFunction(outerAnonymousFunction.Body, out builderAnonymousFunction))
            return false;

        return TryGetSingleBuilderParameter(builderAnonymousFunction, out _);
    }

    private static bool TryGetReturnedAnonymousFunction(
        IOperation? body,
        out IAnonymousFunctionOperation anonymousFunction)
    {
        anonymousFunction = default!;
        switch (RazorVueOperationNormalizer.Unwrap(body))
        {
            case IAnonymousFunctionOperation direct:
                anonymousFunction = direct;
                return true;
            case IBlockOperation block when TryGetSingleReturnedValue(block, out var returnValue):
                return TryGetAnonymousFunction(returnValue, out anonymousFunction);
            case IReturnOperation returnOperation when returnOperation.ReturnedValue is not null:
                return TryGetAnonymousFunction(returnOperation.ReturnedValue, out anonymousFunction);
            default:
                return false;
        }
    }

    private static bool TryGetSingleReturnedValue(IBlockOperation block, out IOperation? returnedValue)
    {
        returnedValue = null;
        if (block.Operations.Length != 1 ||
            block.Operations[0] is not IReturnOperation returnOperation)
        {
            return false;
        }

        returnedValue = RazorVueOperationNormalizer.Unwrap(returnOperation.ReturnedValue);
        return returnedValue is not null;
    }

    private static bool TryResolvePropByRuntimeName(
        VueComponentDescriptor descriptor,
        string runtimeName,
        out VuePropDescriptor prop)
    {
        foreach (var candidate in descriptor.Props)
        {
            if (string.Equals(candidate.Name, runtimeName, StringComparison.Ordinal))
            {
                prop = candidate;
                return true;
            }
        }

        prop = default!;
        return false;
    }

    private static bool TryResolveEmitByRuntimeName(
        VueComponentDescriptor descriptor,
        string runtimeName,
        out VueEmitDescriptor emit)
    {
        foreach (var candidate in descriptor.Emits)
        {
            if (string.Equals(candidate.Name, runtimeName, StringComparison.Ordinal))
            {
                emit = candidate;
                return true;
            }
        }

        emit = default!;
        return false;
    }

    private static bool TryResolveSlotByRuntimeName(
        VueComponentDescriptor descriptor,
        string runtimeName,
        out VueSlotDescriptor slot)
    {
        foreach (var candidate in descriptor.Slots)
        {
            if (string.Equals(candidate.Name, runtimeName, StringComparison.Ordinal))
            {
                slot = candidate;
                return true;
            }
        }

        if (VueSlotResolver.TryResolve(descriptor.Slots, runtimeName, out var resolution))
        {
            slot = resolution.Descriptor;
            return true;
        }

        slot = default!;
        return false;
    }

    private sealed record ResolvedComponentRuntimeUsage(
        string ComponentKey,
        VueComponentDescriptor Descriptor,
        ImmutableArray<VuePropDescriptor> UsedProps,
        ImmutableArray<VueEmitDescriptor> UsedEmits,
        ImmutableArray<VueSlotDescriptor> UsedSlots);

    private sealed class ImperativeRuntimeUsageCollector
    {
        private readonly INamedTypeSymbol _ownerComponentSymbol;
        private readonly Compilation _compilation;
        private readonly ImmutableDictionary<string, VueComponentDescriptor> _resolvedComponents;
        private readonly Dictionary<string, RuntimeUsageBuilder> _usageByComponent;
        private readonly Dictionary<IParameterSymbol, BuilderState> _builderStates = new(SymbolEqualityComparer.Default);
        private readonly Dictionary<ILocalSymbol, BuilderState> _builderAliases = new(SymbolEqualityComparer.Default);

        public ImperativeRuntimeUsageCollector(
            INamedTypeSymbol ownerComponentSymbol,
            Compilation compilation,
            ImmutableDictionary<string, VueComponentDescriptor> resolvedComponents,
            Dictionary<string, RuntimeUsageBuilder> usageByComponent)
        {
            _ownerComponentSymbol = ownerComponentSymbol;
            _compilation = compilation;
            _resolvedComponents = resolvedComponents;
            _usageByComponent = usageByComponent;
        }

        public void Collect(
            IEnumerable<IOperation> operations,
            ImmutableArray<IParameterSymbol> visibleParameters)
        {
            foreach (var parameter in visibleParameters)
            {
                if (IsRenderTreeBuilderType(parameter.Type))
                    _builderStates[parameter] = new BuilderState();
            }

            foreach (var operation in operations)
                Visit(operation);
        }

        private void Visit(IOperation? operation)
        {
            var current = RazorVueOperationNormalizer.Unwrap(operation);
            if (current is null)
                return;

            switch (current)
            {
                case IBlockOperation block:
                    foreach (var child in block.Operations)
                        Visit(child);
                    break;
                case IVariableDeclarationGroupOperation declarationGroup:
                    VisitVariableDeclarationGroup(declarationGroup);
                    break;
                case IExpressionStatementOperation expressionStatement:
                    Visit(expressionStatement.Operation);
                    break;
                case ISimpleAssignmentOperation assignment:
                    VisitSimpleAssignment(assignment);
                    break;
                case IInvocationOperation invocation:
                    VisitInvocation(invocation);
                    break;
                case IConditionalOperation conditional:
                    Visit(conditional.WhenTrue);
                    Visit(conditional.WhenFalse);
                    break;
                case IWhileLoopOperation whileLoop:
                    Visit(whileLoop.Body);
                    break;
                case IForLoopOperation forLoop:
                    foreach (var before in forLoop.Before)
                        Visit(before);
                    Visit(forLoop.Body);
                    foreach (var atLoopBottom in forLoop.AtLoopBottom)
                        Visit(atLoopBottom);
                    break;
                case IForEachLoopOperation forEachLoop:
                    Visit(forEachLoop.Body);
                    break;
                case ISwitchOperation switchOperation:
                    foreach (var @case in switchOperation.Cases)
                    {
                        foreach (var statement in @case.Body)
                            Visit(statement);
                    }
                    break;
                case ITryOperation tryOperation:
                    Visit(tryOperation.Body);
                    foreach (var @catch in tryOperation.Catches)
                        Visit(@catch.Handler);
                    Visit(tryOperation.Finally);
                    break;
                case IReturnOperation returnOperation:
                    Visit(returnOperation.ReturnedValue);
                    break;
                case IAnonymousFunctionOperation:
                    break;
                default:
                    foreach (var child in current.ChildOperations)
                        Visit(child);
                    break;
            }
        }

        private void VisitVariableDeclarationGroup(IVariableDeclarationGroupOperation declarationGroup)
        {
            foreach (var declaration in declarationGroup.Declarations)
            {
                foreach (var declarator in declaration.Declarators)
                {
                    Visit(declarator.Initializer?.Value);
                    if (TryResolveBuilderState(declarator.Initializer?.Value, out var state))
                        _builderAliases[declarator.Symbol] = state;
                }
            }
        }

        private void VisitSimpleAssignment(ISimpleAssignmentOperation assignment)
        {
            Visit(assignment.Value);
            if (assignment.Target is ILocalReferenceOperation localReference &&
                TryResolveBuilderState(assignment.Value, out var state))
            {
                _builderAliases[localReference.Local] = state;
            }
        }

        private void VisitInvocation(IInvocationOperation invocation)
        {
            if (!TryResolveBuilderState(invocation.Instance, out var builderState))
            {
                foreach (var argument in invocation.Arguments)
                    Visit(argument.Value);
                return;
            }

            switch (invocation.TargetMethod.Name)
            {
                case "OpenComponent":
                    VisitOpenComponent(invocation, builderState);
                    break;
                case "CloseComponent":
                    builderState.PopComponentFrame();
                    break;
                case "OpenElement":
                    builderState.PushNonComponentFrame();
                    break;
                case "CloseElement":
                    builderState.PopFrame();
                    break;
                case "OpenRegion":
                    builderState.PushNonComponentFrame();
                    break;
                case "CloseRegion":
                    builderState.PopFrame();
                    break;
                case "AddAttribute":
                case "AddComponentParameter":
                    VisitAddParameterInvocation(invocation, builderState);
                    break;
                case "AddMultipleAttributes":
                    VisitAddMultipleAttributesInvocation(builderState);
                    break;
                case "AddContent":
                    VisitAddContentInvocation(invocation);
                    break;
                default:
                    foreach (var argument in invocation.Arguments)
                        Visit(argument.Value);
                    break;
            }
        }

        private void VisitOpenComponent(IInvocationOperation invocation, BuilderState builderState)
        {
            foreach (var argument in invocation.Arguments)
                Visit(argument.Value);

            if (!TryResolveOpenComponent(invocation, out var componentKey, out var descriptor))
            {
                builderState.PushNonComponentFrame();
                return;
            }

            builderState.PushComponentFrame(GetOrAddUsage(_usageByComponent, componentKey, descriptor));
        }

        private void VisitAddParameterInvocation(IInvocationOperation invocation, BuilderState builderState)
        {
            if (!builderState.TryGetCurrentComponentUsage(out var usage))
                return;

            var name = TryGetConstantString(invocation.Arguments.ElementAtOrDefault(1)?.Value);
            if (string.IsNullOrWhiteSpace(name))
                return;

            var value = invocation.Arguments.ElementAtOrDefault(2)?.Value;
            if (VueSlotResolver.TryResolve(usage.Descriptor.Slots, name!, out var slotResolution))
            {
                usage.MarkSlot(slotResolution.Descriptor);
                VisitSlotValue(value);
                return;
            }

            if (VuePropResolver.TryResolve(usage.Descriptor.Props, name!, out var propResolution))
            {
                usage.MarkProp(propResolution.Descriptor);
                return;
            }

            if (!string.IsNullOrWhiteSpace(name))
                usage.MarkEmitByAlias(name!);
        }

        private void VisitAddMultipleAttributesInvocation(BuilderState builderState)
        {
            if (!builderState.TryGetCurrentComponentUsage(out var usage))
                return;

            usage.MarkAllPropsAndEmits();
        }

        private void VisitAddContentInvocation(IInvocationOperation invocation)
        {
            foreach (var argument in invocation.Arguments.Skip(1))
                VisitRenderFragmentValue(argument.Value);
        }

        private void VisitSlotValue(IOperation? operation)
        {
            if (TryGetCurrentComponentSlotForwarding(operation, out _))
                return;

            VisitRenderFragmentValue(operation);
        }

        private void VisitRenderFragmentValue(IOperation? operation)
        {
            var current = RazorVueOperationNormalizer.Unwrap(operation);
            if (current is null)
                return;

            if (RazorVueImperativeRenderFragmentCarrierHelper.TryEnumerateNestedImperativeRenderFragmentBodies(
                    _compilation,
                    _ownerComponentSymbol,
                    current,
                    RazorVueOperationNormalizer.Unwrap,
                    IsSourceStableMutableCarrierMember,
                    out var nestedBodies))
            {
                foreach (var body in nestedBodies)
                    Visit(body);

                return;
            }

            if (TryGetAnonymousFunction(current, out var anonymousFunction) &&
                TryGetSingleBuilderParameter(anonymousFunction, out var builderParameter))
            {
                CollectBuilderLambda(anonymousFunction, builderParameter);
                return;
            }

            if (TryGetTypedBuilderTemplate(current, out _, out var builderAnonymousFunction) &&
                TryGetSingleBuilderParameter(builderAnonymousFunction, out var typedBuilderParameter))
            {
                CollectBuilderLambda(builderAnonymousFunction, typedBuilderParameter);
                return;
            }

            Visit(current);
        }

        private void CollectBuilderLambda(
            IAnonymousFunctionOperation anonymousFunction,
            IParameterSymbol builderParameter)
        {
            var state = new BuilderState();
            _builderStates[builderParameter] = state;
            try
            {
                Visit(anonymousFunction.Body);
            }
            finally
            {
                _builderStates.Remove(builderParameter);
            }
        }

        private bool TryResolveBuilderState(IOperation? operation, out BuilderState builderState)
        {
            builderState = default!;
            switch (RazorVueOperationNormalizer.Unwrap(operation))
            {
                case IParameterReferenceOperation parameterReference when IsRenderTreeBuilderType(parameterReference.Parameter.Type):
                    return _builderStates.TryGetValue(parameterReference.Parameter, out builderState);
                case ILocalReferenceOperation localReference when IsRenderTreeBuilderType(localReference.Local.Type):
                    return _builderAliases.TryGetValue(localReference.Local, out builderState);
                default:
                    return false;
            }
        }

        private bool TryResolveOpenComponent(
            IInvocationOperation invocation,
            out string componentKey,
            out VueComponentDescriptor descriptor)
        {
            componentKey = string.Empty;
            descriptor = default!;

            INamedTypeSymbol? componentType = null;
            if (invocation.TargetMethod.TypeArguments.Length == 1 &&
                invocation.TargetMethod.TypeArguments[0] is INamedTypeSymbol genericComponentType)
            {
                componentType = genericComponentType;
            }
            else if (invocation.Arguments.Length >= 2 &&
                     invocation.SemanticModel?.Compilation is { } compilation &&
                     RazorVueComponentTypeCarrierHelper.TryResolveComponentType(
                         compilation,
                         _ownerComponentSymbol,
                         invocation.Arguments[1].Value,
                         out var explicitComponentType,
                         out _))
            {
                componentType = explicitComponentType;
            }

            if (componentType is null)
                return false;

            foreach (var pair in _resolvedComponents)
            {
                if (!string.Equals(pair.Value.FullName, componentType.ToDisplayString(), StringComparison.Ordinal))
                    continue;

                componentKey = pair.Key;
                descriptor = pair.Value;
                return true;
            }

            return false;
        }

        private static string? TryGetConstantString(IOperation? operation)
        {
            var current = RazorVueOperationNormalizer.Unwrap(operation);
            return current?.ConstantValue.HasValue == true &&
                   current.ConstantValue.Value is string text
                ? text
                : null;
        }

        private bool IsSourceStableMutableCarrierMember(Compilation compilation, ISymbol member)
            => RazorVueMemberWriteAnalysis.CanUseSourceStableMutableCarrierMember(member) &&
               !RazorVueMemberWriteAnalysis.HasObservableWritesOutsideDeclarationInitializer(compilation, member);

        private sealed class BuilderState
        {
            private readonly Stack<FrameState> _frames = new();

            public void PushComponentFrame(RuntimeUsageBuilder usage)
                => _frames.Push(new FrameState(usage));

            public void PushNonComponentFrame()
                => _frames.Push(new FrameState(null));

            public void PopComponentFrame()
            {
                if (_frames.Count == 0)
                    return;

                _frames.Pop();
            }

            public void PopFrame()
            {
                if (_frames.Count == 0)
                    return;

                _frames.Pop();
            }

            public bool TryGetCurrentComponentUsage(out RuntimeUsageBuilder usage)
            {
                usage = default!;
                if (_frames.Count == 0)
                    return false;

                var frame = _frames.Peek();
                if (frame.ComponentUsage is null)
                    return false;

                usage = frame.ComponentUsage;
                return true;
            }

            private sealed record FrameState(RuntimeUsageBuilder? ComponentUsage);
        }
    }

    private sealed class RuntimeUsageBuilder
    {
        private readonly Dictionary<string, VuePropDescriptor> _props = new(StringComparer.Ordinal);
        private readonly Dictionary<string, VueEmitDescriptor> _emits = new(StringComparer.Ordinal);
        private readonly Dictionary<string, VueSlotDescriptor> _slots = new(StringComparer.Ordinal);

        public RuntimeUsageBuilder(string componentKey, VueComponentDescriptor descriptor)
        {
            ComponentKey = componentKey;
            Descriptor = descriptor;
        }

        public string ComponentKey { get; }

        public VueComponentDescriptor Descriptor { get; }

        public void MarkAllPropsAndEmits()
        {
            foreach (var prop in Descriptor.Props)
                MarkProp(prop);
            foreach (var emit in Descriptor.Emits)
                MarkEmit(emit);
        }

        public void MarkProp(VuePropDescriptor descriptor)
            => _props[descriptor.PublicName] = descriptor;

        public void MarkEmitByAlias(string razorAlias)
        {
            foreach (var emit in Descriptor.Emits)
            {
                if (string.Equals(emit.RazorAlias, razorAlias, StringComparison.Ordinal))
                    MarkEmit(emit);
            }
        }

        public void MarkEmit(VueEmitDescriptor descriptor)
            => _emits[descriptor.RazorAlias ?? descriptor.Name] = descriptor;

        public void MarkSlot(VueSlotDescriptor descriptor)
            => _slots[descriptor.PublicName] = descriptor;

        public ResolvedComponentRuntimeUsage Build()
            => new(
                ComponentKey,
                Descriptor,
                _props.Values.ToImmutableArray(),
                _emits.Values.ToImmutableArray(),
                _slots.Values.ToImmutableArray());
    }
}
