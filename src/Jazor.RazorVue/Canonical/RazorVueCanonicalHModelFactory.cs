using System.Collections.Immutable;
using System.Linq;
using Jazor.RazorVue.Artifacts;
using Jazor.RazorVue.Descriptor;
using Jazor.RazorVue.Extensibility;
using Jazor.RazorVue.Lowering;
using Jazor.RazorVue.RenderTree;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Operations;

namespace Jazor.RazorVue.Canonical;

internal sealed class RazorVueCanonicalHModelFactory
{
    private readonly IRazorVueTemplateFrontend _templateFrontend;
    private static readonly ImmutableHashSet<ILocalSymbol> EmptyLocalScope =
        ImmutableHashSet<ILocalSymbol>.Empty.WithComparer(SymbolEqualityComparer.Default);
    private static readonly ImmutableHashSet<IParameterSymbol> EmptyParameterScope =
        ImmutableHashSet<IParameterSymbol>.Empty.WithComparer(SymbolEqualityComparer.Default);

    public RazorVueCanonicalHModelFactory(IRazorVueTemplateFrontend templateFrontend)
    {
        _templateFrontend = templateFrontend ?? throw new ArgumentNullException(nameof(templateFrontend));
    }

    public RazorVueCanonicalHComponentModel Create(
        RazorVueCompilationContext context,
        RazorVueSemanticSnapshot snapshot)
    {
        if (context is null)
            throw new ArgumentNullException(nameof(context));
        if (snapshot is null)
            throw new ArgumentNullException(nameof(snapshot));

        var renderTree = _templateFrontend.CreateRenderTree(context, snapshot);
        return Create(context, snapshot, renderTree);
    }

    public RazorVueCanonicalHComponentModel Create(
        RazorVueCompilationContext context,
        RazorVueSemanticSnapshot snapshot,
        RazorVueRenderFragment renderTree)
    {
        if (context is null)
            throw new ArgumentNullException(nameof(context));
        if (snapshot is null)
            throw new ArgumentNullException(nameof(snapshot));

        var resolvedComponents = ResolveComponents(context, snapshot, renderTree);
        var expressionEmitter = CreateExpressionEmitter(snapshot, resolvedComponents);
        var template = CreateTemplateFragment(
            snapshot,
            expressionEmitter,
            resolvedComponents,
            renderTree,
            EmptyLocalScope,
            EmptyParameterScope);
        var compilerImports = expressionEmitter.FlushCompilerImports();
        var imports = BuildImports(resolvedComponents, compilerImports);
        var styles = BuildStyles(snapshot.Descriptor, resolvedComponents);
        var pluginRequirements = BuildPluginRequirements(snapshot.Descriptor, resolvedComponents);
        var sourceOrigins = snapshot.Origins.AddRange(expressionEmitter.CollectOrigins(renderTree));
        var hints = BuildHints(snapshot, renderTree);

        return new RazorVueCanonicalHComponentModel(
            ComponentName: snapshot.Descriptor.Name,
            ComponentFullName: snapshot.Descriptor.FullName,
            RelativeComponentPath: NormalizeRelativePath(snapshot.Descriptor.ImportSpecifier),
            Descriptor: snapshot.Descriptor,
            Imports: imports,
            CompilerImports: compilerImports,
            Styles: styles,
            PluginRequirements: pluginRequirements,
            Hints: hints,
            SourceOrigins: sourceOrigins,
            Template: template,
            Setup: new RazorVueCanonicalSetupModel(
                snapshot.Logic.Fields,
                snapshot.Logic.Methods,
                expressionEmitter.GetRequiredSetupFields(),
                expressionEmitter.GetRequiredSetupMethods(),
                snapshot.Lifecycle));
    }

    private static RazorVueCanonicalTemplateFragment CreateTemplateFragment(
        RazorVueSemanticSnapshot snapshot,
        RazorVueExpressionEmitter expressionEmitter,
        ImmutableDictionary<string, VueComponentDescriptor> resolvedComponents,
        RazorVueRenderFragment fragment,
        ImmutableHashSet<ILocalSymbol> allowedLocalSymbols,
        ImmutableHashSet<IParameterSymbol> allowedParameterSymbols)
    {
        if (fragment.Children.IsDefaultOrEmpty)
            return RazorVueCanonicalTemplateFragment.Empty;

        return new RazorVueCanonicalTemplateFragment(
            fragment.Children
                .Select(node => CreateTemplateNode(
                    snapshot,
                    expressionEmitter,
                    resolvedComponents,
                    node,
                    allowedLocalSymbols,
                    allowedParameterSymbols))
                .ToImmutableArray());
    }

    private static RazorVueCanonicalTemplateNode CreateTemplateNode(
        RazorVueSemanticSnapshot snapshot,
        RazorVueExpressionEmitter expressionEmitter,
        ImmutableDictionary<string, VueComponentDescriptor> resolvedComponents,
        RazorVueRenderNode node,
        ImmutableHashSet<ILocalSymbol> allowedLocalSymbols,
        ImmutableHashSet<IParameterSymbol> allowedParameterSymbols)
        => node switch
        {
            RazorVueElementNode element => new RazorVueCanonicalElementNode(
                TagName: element.TagName,
                Attributes: CreateHtmlAttributeBindings(snapshot, expressionEmitter, element.Attributes, allowedLocalSymbols, allowedParameterSymbols),
                Children: CreateTemplateFragment(snapshot, expressionEmitter, resolvedComponents, element.Children, allowedLocalSymbols, allowedParameterSymbols),
                TemplateEncodability: RazorVueTemplateEncodability.DirectTemplate,
                SideEffectClassification: RazorVueSideEffectClassification.None,
                SourceOrigins: element.Origins),
            RazorVueComponentNode component => CreateComponentNode(
                snapshot,
                expressionEmitter,
                resolvedComponents,
                component,
                allowedLocalSymbols,
                allowedParameterSymbols),
            RazorVueTextNode text => new RazorVueCanonicalTextNode(
                Text: text.Text,
                TemplateEncodability: RazorVueTemplateEncodability.DirectTemplate,
                SideEffectClassification: RazorVueSideEffectClassification.None,
                SourceOrigins: text.Origins),
            RazorVueExpressionNode expression => CreateInterpolationNode(
                snapshot,
                expressionEmitter,
                expression,
                allowedLocalSymbols,
                allowedParameterSymbols),
            RazorVueUnsupportedTemplateNode unsupported => throw CreateUnsupportedExpressionException(
                snapshot,
                unsupported.Origins,
                unsupported.Message),
            RazorVueSlotOutletNode slot => new RazorVueCanonicalSlotOutletNode(
                SlotName: slot.SlotName,
                ArgumentExpressionText: slot.Argument is null
                    ? null
                    : EmitTemplateExpression(snapshot, expressionEmitter, slot.Argument, allowedLocalSymbols, allowedParameterSymbols),
                BindingKind: ClassifyBindingKind(snapshot, slot.Argument),
                TemplateEncodability: ClassifyTemplateEncodability(slot.Argument),
                SideEffectClassification: ClassifySideEffects(slot.Argument),
                SourceOrigins: slot.Origins),
            RazorVueConditionalNode conditional => new RazorVueCanonicalConditionalNode(
                ConditionExpressionText: EmitTemplateExpression(
                    snapshot,
                    expressionEmitter,
                    conditional.Condition,
                    allowedLocalSymbols,
                    allowedParameterSymbols),
                BindingKind: ClassifyBindingKind(snapshot, conditional.Condition),
                WhenTrue: CreateTemplateFragment(
                    snapshot,
                    expressionEmitter,
                    resolvedComponents,
                    conditional.WhenTrue,
                    allowedLocalSymbols,
                    allowedParameterSymbols),
                WhenFalse: CreateTemplateFragment(
                    snapshot,
                    expressionEmitter,
                    resolvedComponents,
                    conditional.WhenFalse,
                    allowedLocalSymbols,
                    allowedParameterSymbols),
                TemplateEncodability: RazorVueTemplateEncodability.TemplateViaSetupBinding,
                SideEffectClassification: ClassifySideEffects(conditional.Condition),
                SourceOrigins: conditional.Origins),
            RazorVueForEachNode loop => new RazorVueCanonicalForEachNode(
                ItemName: loop.ItemName,
                SourceExpressionText: EmitTemplateExpression(
                    snapshot,
                    expressionEmitter,
                    loop.Source,
                    allowedLocalSymbols,
                    allowedParameterSymbols),
                BindingKind: ClassifyBindingKind(snapshot, loop.Source),
                Body: CreateTemplateFragment(
                    snapshot,
                    expressionEmitter,
                    resolvedComponents,
                    loop.Body,
                    RazorVueTemplateExpressionScopeValidator.AddIfPresent(allowedLocalSymbols, loop.ItemSymbol),
                    allowedParameterSymbols),
                TemplateEncodability: RazorVueTemplateEncodability.TemplateViaSetupBinding,
                SideEffectClassification: ClassifySideEffects(loop.Source),
                SourceOrigins: loop.Origins),
            RazorVueForNode loop => new RazorVueCanonicalForNode(
                VariableName: loop.VariableName,
                InitialValueExpressionText: EmitTemplateExpression(
                    snapshot,
                    expressionEmitter,
                    loop.InitialValue,
                    allowedLocalSymbols,
                    allowedParameterSymbols),
                InitialValueBindingKind: ClassifyBindingKind(snapshot, loop.InitialValue),
                ConditionKind: loop.ConditionKind,
                LimitValueExpressionText: EmitTemplateExpression(
                    snapshot,
                    expressionEmitter,
                    loop.LimitValue,
                    allowedLocalSymbols,
                    allowedParameterSymbols),
                LimitValueBindingKind: ClassifyBindingKind(snapshot, loop.LimitValue),
                StepKind: loop.StepKind,
                StepValueExpressionText: loop.StepValue is null
                    ? null
                    : EmitTemplateExpression(snapshot, expressionEmitter, loop.StepValue, allowedLocalSymbols, allowedParameterSymbols),
                StepValueBindingKind: ClassifyBindingKind(snapshot, loop.StepValue),
                Body: CreateTemplateFragment(
                    snapshot,
                    expressionEmitter,
                    resolvedComponents,
                    loop.Body,
                    RazorVueTemplateExpressionScopeValidator.AddIfPresent(allowedLocalSymbols, loop.VariableSymbol),
                    allowedParameterSymbols),
                TemplateEncodability: RazorVueTemplateEncodability.TemplateViaSetupBinding,
                SideEffectClassification: CombineSideEffectClassifications(
                    ClassifySideEffects(loop.InitialValue),
                    ClassifySideEffects(loop.LimitValue),
                    ClassifySideEffects(loop.StepValue)),
                SourceOrigins: loop.Origins),
            _ => throw CreateUnsupportedCanonicalizationException(snapshot, node.GetType().Name, node.Origins)
        };

    private static RazorVueCanonicalComponentNode CreateComponentNode(
        RazorVueSemanticSnapshot snapshot,
        RazorVueExpressionEmitter expressionEmitter,
        ImmutableDictionary<string, VueComponentDescriptor> resolvedComponents,
        RazorVueComponentNode component,
        ImmutableHashSet<ILocalSymbol> allowedLocalSymbols,
        ImmutableHashSet<IParameterSymbol> allowedParameterSymbols)
    {
        resolvedComponents.TryGetValue(component.ComponentName, out var resolvedDescriptor);
        if (resolvedDescriptor is null)
            throw CreateUnsupportedCanonicalizationException(snapshot, component.ComponentName, component.Origins);

        ValidateDefaultLibrarySlotUsage(snapshot, resolvedDescriptor, component);
        ValidateDuplicateLibrarySlotUsage(snapshot, resolvedDescriptor, component);

        var slotBindings = CreateComponentSlotBindings(
            snapshot,
            expressionEmitter,
            component,
            resolvedDescriptor,
            allowedLocalSymbols,
            allowedParameterSymbols);
        slotBindings = slotBindings.AddRange(CreateComponentSlotTemplateBindings(
            snapshot,
            expressionEmitter,
            resolvedComponents,
            component,
            resolvedDescriptor,
            allowedLocalSymbols,
            allowedParameterSymbols));
        if (!component.Children.Children.IsDefaultOrEmpty)
        {
            slotBindings = slotBindings.Add(new RazorVueCanonicalSlotBinding(
                SlotName: "default",
                IsDefault: true,
                ParameterName: null,
                ValueKind: RazorVueCanonicalSlotValueKind.None,
                ValueExpressionText: null,
                ForwardedSlotName: null,
                BindingKind: RazorVueExpressionBindingKind.None,
                TemplateEncodability: RazorVueTemplateEncodability.DirectTemplate,
                Children: CreateTemplateFragment(
                    snapshot,
                    expressionEmitter,
                    resolvedComponents,
                    component.Children,
                    allowedLocalSymbols,
                    allowedParameterSymbols),
                SourceOrigins: component.Children.Children
                    .SelectMany(static child => child.Origins)
                    .ToImmutableArray()));
        }

        return new RazorVueCanonicalComponentNode(
            ComponentName: component.ComponentName,
            ComponentFullName: component.ComponentFullName,
            ResolutionName: component.ResolutionName,
            ResolvedDescriptor: resolvedDescriptor,
            Attributes: CreateComponentAttributeBindings(
                snapshot,
                expressionEmitter,
                component,
                resolvedDescriptor,
                allowedLocalSymbols,
                allowedParameterSymbols),
            Slots: slotBindings,
            Children: RazorVueCanonicalTemplateFragment.Empty,
            TemplateEncodability: RazorVueTemplateEncodability.DirectTemplate,
            SideEffectClassification: RazorVueSideEffectClassification.None,
            SourceOrigins: component.Origins);
    }

    private static void ValidateDefaultLibrarySlotUsage(
        RazorVueSemanticSnapshot snapshot,
        VueComponentDescriptor descriptor,
        RazorVueComponentNode component)
    {
        if (descriptor.SourceKind != VueComponentSourceKind.LibraryComponent ||
            component.Children.Children.IsDefaultOrEmpty)
        {
            return;
        }

        var origin = component.Children.Children
            .SelectMany(static child => child.Origins)
            .FirstOrDefault() ?? component.Origins.FirstOrDefault();

        if (VueSlotResolver.TryResolve(descriptor.Slots, "ChildContent", out var defaultSlot))
        {
            if (defaultSlot.Descriptor.Parameters.IsDefaultOrEmpty)
                return;

            throw CreateSlotContextMisuseException(
                snapshot,
                descriptor,
                defaultSlot.Descriptor,
                "ChildContent",
                origin);
        }

        throw CreateUnknownSlotException(snapshot, descriptor, "ChildContent", origin);
    }

    private static void ValidateDuplicateLibrarySlotUsage(
        RazorVueSemanticSnapshot snapshot,
        VueComponentDescriptor descriptor,
        RazorVueComponentNode component)
    {
        if (descriptor.SourceKind != VueComponentSourceKind.LibraryComponent)
            return;

        if (descriptor.Slots.IsDefaultOrEmpty)
            return;

        var assignedSlots = new HashSet<string>(StringComparer.Ordinal);
        if (!component.Children.Children.IsDefaultOrEmpty &&
            VueSlotResolver.TryResolve(descriptor.Slots, "ChildContent", out _))
        {
            assignedSlots.Add("ChildContent");
        }

        foreach (var slotTemplate in component.SlotTemplates)
        {
            if (!VueSlotResolver.TryResolve(descriptor.Slots, slotTemplate.PublicName, out var slot))
                continue;

            if (assignedSlots.Add(slot.SlotName))
                continue;

            throw CreateDuplicateSlotValueException(
                snapshot,
                descriptor,
                slotTemplate.PublicName,
                slotTemplate.Origins.IsDefaultOrEmpty ? snapshot.Origins.FirstOrDefault() : slotTemplate.Origins[0]);
        }

        foreach (var attributeEntry in component.Attributes)
        {
            if (attributeEntry is not RazorVueAttributeNode attribute ||
                !VueSlotResolver.TryResolve(descriptor.Slots, attribute.Name, out var slot))
            {
                continue;
            }

            if (assignedSlots.Add(slot.SlotName))
                continue;

            throw CreateDuplicateSlotValueException(
                snapshot,
                descriptor,
                attribute.Name,
                attribute.Origins.IsDefaultOrEmpty ? snapshot.Origins.FirstOrDefault() : attribute.Origins[0]);
        }
    }

    private static RazorVueCanonicalInterpolationNode CreateInterpolationNode(
        RazorVueSemanticSnapshot snapshot,
        RazorVueExpressionEmitter expressionEmitter,
        RazorVueExpressionNode expression,
        ImmutableHashSet<ILocalSymbol> allowedLocalSymbols,
        ImmutableHashSet<IParameterSymbol> allowedParameterSymbols)
    {
        var expressionText = EmitTemplateExpression(
            snapshot,
            expressionEmitter,
            expression.Expression,
            allowedLocalSymbols,
            allowedParameterSymbols);
        return new RazorVueCanonicalInterpolationNode(
            ExpressionText: expressionText,
            BindingKind: ClassifyBindingKind(snapshot, expression.Expression),
            TemplateEncodability: ClassifyTemplateEncodability(expression.Expression),
            SideEffectClassification: ClassifySideEffects(expression.Expression),
            SourceOrigins: expression.Origins);
    }

    private static ImmutableArray<RazorVueCanonicalAttributeEntry> CreateHtmlAttributeBindings(
        RazorVueSemanticSnapshot snapshot,
        RazorVueExpressionEmitter expressionEmitter,
        ImmutableArray<RazorVueAttributeEntry> attributes,
        ImmutableHashSet<ILocalSymbol> allowedLocalSymbols,
        ImmutableHashSet<IParameterSymbol> allowedParameterSymbols)
    {
        if (attributes.IsDefaultOrEmpty)
            return ImmutableArray<RazorVueCanonicalAttributeEntry>.Empty;

        var builder = ImmutableArray.CreateBuilder<RazorVueCanonicalAttributeEntry>();
        foreach (var attributeEntry in attributes)
        {
            switch (attributeEntry)
            {
                case RazorVueAttributeNode attribute:
                    builder.Add(new RazorVueCanonicalAttributeBinding(
                        Name: attribute.Name,
                        ExpressionText: attribute.Value is null
                            ? null
                            : EmitTemplateExpression(snapshot, expressionEmitter, attribute.Value, allowedLocalSymbols, allowedParameterSymbols),
                        LiteralValueKind: ClassifyLiteralValueKind(attribute.Value),
                        AttributeKind: RazorVueCanonicalAttributeKind.HtmlAttribute,
                        BindingKind: ClassifyBindingKind(snapshot, attribute.Value),
                        TemplateEncodability: ClassifyTemplateEncodability(attribute.Value),
                        SourceOrigins: attribute.Origins));
                    break;
                case RazorVueAttributeSpreadNode spread:
                    builder.Add(new RazorVueCanonicalAttributeSpreadBinding(
                        ExpressionText: EmitTemplateExpression(
                            snapshot,
                            expressionEmitter,
                            spread.Expression,
                            allowedLocalSymbols,
                            allowedParameterSymbols),
                        BindingKind: ClassifyBindingKind(snapshot, spread.Expression),
                        TemplateEncodability: ClassifyTemplateEncodability(spread.Expression),
                        SourceOrigins: spread.Origins));
                    break;
                default:
                    throw CreateUnsupportedCanonicalizationException(snapshot, attributeEntry.GetType().Name, attributeEntry.Origins);
            }
        }

        return builder.ToImmutable();
    }

    private static ImmutableArray<RazorVueCanonicalAttributeEntry> CreateComponentAttributeBindings(
        RazorVueSemanticSnapshot snapshot,
        RazorVueExpressionEmitter expressionEmitter,
        RazorVueComponentNode component,
        VueComponentDescriptor descriptor,
        ImmutableHashSet<ILocalSymbol> allowedLocalSymbols,
        ImmutableHashSet<IParameterSymbol> allowedParameterSymbols)
    {
        if (component.Attributes.IsDefaultOrEmpty)
            return ImmutableArray<RazorVueCanonicalAttributeEntry>.Empty;

        var propsByName = BuildPropsByName(descriptor);
        var emitsByAlias = BuildEmitsByAlias(descriptor);
        var unmatchedValuesProp = GetCaptureUnmatchedValuesProp(snapshot, descriptor, component);
        ValidateInvalidBindTargets(snapshot, descriptor, component, propsByName, emitsByAlias);
        ValidateDuplicateMappedComponentAttributes(snapshot, descriptor, component, propsByName, emitsByAlias);
        var builder = ImmutableArray.CreateBuilder<RazorVueCanonicalAttributeEntry>();

        foreach (var attributeEntry in component.Attributes)
        {
            if (attributeEntry is RazorVueAttributeSpreadNode spread)
            {
                if (unmatchedValuesProp is null)
                    throw CreateUnsupportedComponentSpreadException(snapshot, descriptor, spread);

                builder.Add(new RazorVueCanonicalAttributeSpreadBinding(
                    ExpressionText: EmitTemplateExpression(
                        snapshot,
                        expressionEmitter,
                        spread.Expression,
                        allowedLocalSymbols,
                        allowedParameterSymbols),
                    BindingKind: ClassifyBindingKind(snapshot, spread.Expression),
                    TemplateEncodability: ClassifyTemplateEncodability(spread.Expression),
                    SourceOrigins: spread.Origins));
                continue;
            }

            if (attributeEntry is not RazorVueAttributeNode attribute)
                throw CreateUnsupportedCanonicalizationException(snapshot, attributeEntry.GetType().Name, attributeEntry.Origins);

            if (VueSlotResolver.TryResolve(descriptor.Slots, attribute.Name, out _))
                continue;

            if (emitsByAlias.TryGetValue(attribute.Name, out var emitDescriptor))
            {
                builder.Add(new RazorVueCanonicalAttributeBinding(
                    Name: emitDescriptor.Name,
                    ExpressionText: attribute.Value is null
                        ? null
                        : EmitTemplateExpression(snapshot, expressionEmitter, attribute.Value, allowedLocalSymbols, allowedParameterSymbols),
                    LiteralValueKind: ClassifyLiteralValueKind(attribute.Value),
                    AttributeKind: RazorVueCanonicalAttributeKind.ComponentEvent,
                    BindingKind: ClassifyBindingKind(snapshot, attribute.Value),
                    TemplateEncodability: ClassifyTemplateEncodability(attribute.Value),
                    SourceOrigins: attribute.Origins));
                continue;
            }

            if (propsByName.TryGetValue(attribute.Name, out var propDescriptor))
            {
                builder.Add(new RazorVueCanonicalAttributeBinding(
                    Name: propDescriptor.Name,
                    ExpressionText: attribute.Value is null
                        ? null
                        : EmitTemplateExpression(snapshot, expressionEmitter, attribute.Value, allowedLocalSymbols, allowedParameterSymbols),
                    LiteralValueKind: ClassifyLiteralValueKind(attribute.Value),
                    AttributeKind: RazorVueCanonicalAttributeKind.ComponentProp,
                    BindingKind: ClassifyBindingKind(snapshot, attribute.Value),
                    TemplateEncodability: ClassifyTemplateEncodability(attribute.Value),
                    SourceOrigins: attribute.Origins));
                continue;
            }

            if (unmatchedValuesProp is not null &&
                RazorVueCaptureUnmatchedAttributePolicy.CanCaptureExplicitAttribute(attribute.Name))
            {
                builder.Add(new RazorVueCanonicalAttributeBinding(
                    Name: attribute.Name,
                    ExpressionText: attribute.Value is null
                        ? null
                        : EmitTemplateExpression(snapshot, expressionEmitter, attribute.Value, allowedLocalSymbols, allowedParameterSymbols),
                    LiteralValueKind: ClassifyLiteralValueKind(attribute.Value),
                    AttributeKind: RazorVueCanonicalAttributeKind.ComponentFallthroughAttribute,
                    BindingKind: ClassifyBindingKind(snapshot, attribute.Value),
                    TemplateEncodability: ClassifyTemplateEncodability(attribute.Value),
                    SourceOrigins: attribute.Origins));
                continue;
            }

            throw CreateUnknownComponentAttributeException(snapshot, descriptor, attribute);
        }

        return builder.ToImmutable();
    }

    private static void ValidateInvalidBindTargets(
        RazorVueSemanticSnapshot snapshot,
        VueComponentDescriptor descriptor,
        RazorVueComponentNode component,
        ImmutableDictionary<string, VuePropDescriptor> propsByName,
        ImmutableDictionary<string, VueEmitDescriptor> emitsByAlias)
    {
        var attributeNames = component.Attributes
            .OfType<RazorVueAttributeNode>()
            .Select(static attribute => attribute.Name)
            .ToImmutableHashSet(StringComparer.Ordinal);

        foreach (var attributeEntry in component.Attributes)
        {
            if (attributeEntry is not RazorVueAttributeNode attribute)
                continue;

            if (!TryGetBindTargetName(attribute.Name, out var parameterName) ||
                !attributeNames.Contains(parameterName))
            {
                continue;
            }

            var hasBindableProp = propsByName.TryGetValue(parameterName, out var propDescriptor) &&
                                  propDescriptor.AcceptsBinding;
            var hasModelUpdateEmit = emitsByAlias.TryGetValue(attribute.Name, out var emitDescriptor) &&
                                     emitDescriptor.Kind == VueEmitKind.ModelUpdate;

            if (hasBindableProp && hasModelUpdateEmit)
                continue;

            throw CreateInvalidBindTargetException(snapshot, descriptor, parameterName, attribute);
        }
    }

    private static void ValidateDuplicateMappedComponentAttributes(
        RazorVueSemanticSnapshot snapshot,
        VueComponentDescriptor descriptor,
        RazorVueComponentNode component,
        ImmutableDictionary<string, VuePropDescriptor> propsByName,
        ImmutableDictionary<string, VueEmitDescriptor> emitsByAlias)
    {
        var mappedAttributes = new Dictionary<string, RazorVueAttributeNode>(StringComparer.Ordinal);
        foreach (var attributeEntry in component.Attributes)
        {
            if (attributeEntry is not RazorVueAttributeNode attribute)
                continue;

            if (propsByName.TryGetValue(attribute.Name, out var propDescriptor))
            {
                ValidateUniqueMappedAttribute(
                    snapshot,
                    descriptor,
                    mappedAttributes,
                    "prop:" + propDescriptor.Name,
                    "Vue prop",
                    propDescriptor.Name,
                    attribute);
                continue;
            }

            if (emitsByAlias.TryGetValue(attribute.Name, out var emitDescriptor))
            {
                ValidateUniqueMappedAttribute(
                    snapshot,
                    descriptor,
                    mappedAttributes,
                    "emit:" + emitDescriptor.Name,
                    "Vue event",
                    emitDescriptor.Name,
                    attribute);
            }
        }
    }

    private static void ValidateUniqueMappedAttribute(
        RazorVueSemanticSnapshot snapshot,
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
        throw CreateDuplicateMappedComponentAttributeException(
            snapshot,
            descriptor,
            firstAttribute,
            attribute,
            mappedKind,
            mappedName);
    }

    private static ImmutableArray<RazorVueCanonicalSlotBinding> CreateComponentSlotBindings(
        RazorVueSemanticSnapshot snapshot,
        RazorVueExpressionEmitter expressionEmitter,
        RazorVueComponentNode component,
        VueComponentDescriptor descriptor,
        ImmutableHashSet<ILocalSymbol> allowedLocalSymbols,
        ImmutableHashSet<IParameterSymbol> allowedParameterSymbols)
    {
        if (component.Attributes.IsDefaultOrEmpty)
            return ImmutableArray<RazorVueCanonicalSlotBinding>.Empty;

        var builder = ImmutableArray.CreateBuilder<RazorVueCanonicalSlotBinding>();

        foreach (var attributeEntry in component.Attributes)
        {
            if (attributeEntry is not RazorVueAttributeNode attribute)
                continue;

            if (!VueSlotResolver.TryResolve(descriptor.Slots, attribute.Name, out var slot))
                continue;

            var slotDescriptor = slot.Descriptor;
            if (attribute.Value is not null &&
                !slotDescriptor.Parameters.IsDefaultOrEmpty &&
                !IsCallableSlotValue(snapshot, attribute.Value))
            {
                throw CreateSlotContextMisuseException(snapshot, slotDescriptor, attribute);
            }

            var valueKind = ClassifySlotValueKind(snapshot, attribute.Value, slotDescriptor);
            var valueExpressionText = valueKind == RazorVueCanonicalSlotValueKind.ForwardedSlot || attribute.Value is null
                ? null
                : EmitTemplateExpression(snapshot, expressionEmitter, attribute.Value, allowedLocalSymbols, allowedParameterSymbols);
            var forwardedSlotName = valueKind == RazorVueCanonicalSlotValueKind.ForwardedSlot
                ? GetForwardedSlotName(snapshot, attribute.Value!, slotDescriptor)
                : null;

            builder.Add(new RazorVueCanonicalSlotBinding(
                SlotName: slot.SlotName,
                IsDefault: slotDescriptor.IsDefault,
                ParameterName: slotDescriptor.Parameters.IsDefaultOrEmpty ? null : slotDescriptor.Parameters[0].Name,
                ValueKind: valueKind,
                ValueExpressionText: valueExpressionText,
                ForwardedSlotName: forwardedSlotName,
                BindingKind: ClassifySlotBindingKind(snapshot, attribute.Value, valueKind),
                TemplateEncodability: ClassifySlotTemplateEncodability(attribute.Value, valueKind),
                Children: RazorVueCanonicalTemplateFragment.Empty,
                SourceOrigins: attribute.Origins));
        }

        return builder.ToImmutable();
    }

    private static ImmutableArray<RazorVueCanonicalSlotBinding> CreateComponentSlotTemplateBindings(
        RazorVueSemanticSnapshot snapshot,
        RazorVueExpressionEmitter expressionEmitter,
        ImmutableDictionary<string, VueComponentDescriptor> resolvedComponents,
        RazorVueComponentNode component,
        VueComponentDescriptor descriptor,
        ImmutableHashSet<ILocalSymbol> allowedLocalSymbols,
        ImmutableHashSet<IParameterSymbol> allowedParameterSymbols)
    {
        if (component.SlotTemplates.IsDefaultOrEmpty)
            return ImmutableArray<RazorVueCanonicalSlotBinding>.Empty;

        var assignedNames = new HashSet<string>(StringComparer.Ordinal);
        var builder = ImmutableArray.CreateBuilder<RazorVueCanonicalSlotBinding>();

        foreach (var slotTemplate in component.SlotTemplates)
        {
            if (!VueSlotResolver.TryResolve(descriptor.Slots, slotTemplate.PublicName, out var slot))
            {
                throw CreateUnknownSlotException(
                    snapshot,
                    descriptor,
                    slotTemplate.PublicName,
                    slotTemplate.Origins.IsDefaultOrEmpty ? snapshot.Origins.FirstOrDefault() : slotTemplate.Origins[0]);
            }

            if (!assignedNames.Add(slot.SlotName))
            {
                throw CreateDuplicateSlotValueException(
                    snapshot,
                    descriptor,
                    slotTemplate.PublicName,
                    slotTemplate.Origins.IsDefaultOrEmpty ? snapshot.Origins.FirstOrDefault() : slotTemplate.Origins[0]);
            }

            var slotDescriptor = slot.Descriptor;
            if (slotDescriptor.Parameters.IsDefaultOrEmpty)
            {
                if (!string.IsNullOrWhiteSpace(slotTemplate.ParameterName))
                {
                    throw CreateSlotContextMisuseException(
                        snapshot,
                        slotDescriptor,
                        new RazorVueAttributeNode(slotTemplate.PublicName, null, slotTemplate.Origins));
                }
            }
            else if (string.IsNullOrWhiteSpace(slotTemplate.ParameterName))
            {
                throw CreateSlotContextMisuseException(
                    snapshot,
                    slotDescriptor,
                    new RazorVueAttributeNode(slotTemplate.PublicName, null, slotTemplate.Origins));
            }

            var slotParameterScope = string.IsNullOrWhiteSpace(slotTemplate.ParameterName)
                ? allowedParameterSymbols
                : RazorVueTemplateExpressionScopeValidator.AddIfPresent(allowedParameterSymbols, slotTemplate.ParameterSymbol);
            builder.Add(new RazorVueCanonicalSlotBinding(
                SlotName: slot.SlotName,
                IsDefault: slotDescriptor.IsDefault,
                ParameterName: slotDescriptor.Parameters.IsDefaultOrEmpty ? null : slotTemplate.ParameterName,
                ValueKind: RazorVueCanonicalSlotValueKind.None,
                ValueExpressionText: null,
                ForwardedSlotName: null,
                BindingKind: RazorVueExpressionBindingKind.None,
                TemplateEncodability: RazorVueTemplateEncodability.DirectTemplate,
                Children: CreateTemplateFragment(
                    snapshot,
                    expressionEmitter,
                    resolvedComponents,
                    slotTemplate.Children,
                    allowedLocalSymbols,
                    slotParameterScope),
                SourceOrigins: slotTemplate.Origins));
        }

        return builder.ToImmutable();
    }

    private static RazorVueCanonicalSlotValueKind ClassifySlotValueKind(
        RazorVueSemanticSnapshot snapshot,
        IOperation? operation,
        VueSlotDescriptor slotDescriptor)
    {
        if (operation is null)
            return RazorVueCanonicalSlotValueKind.None;

        var current = Unwrap(operation);
        if (current is IPropertyReferenceOperation property &&
            TryGetCurrentComponentSlotDescriptor(snapshot, property, out var currentSlot))
        {
            return RazorVueCanonicalSlotValueKind.ForwardedSlot;
        }

        if (slotDescriptor.Parameters.IsDefaultOrEmpty)
            return RazorVueCanonicalSlotValueKind.ValueExpression;

        return RazorVueCanonicalSlotValueKind.ValueExpression;
    }

    private static RazorVueExpressionBindingKind ClassifySlotBindingKind(
        RazorVueSemanticSnapshot snapshot,
        IOperation? operation,
        RazorVueCanonicalSlotValueKind valueKind)
        => valueKind == RazorVueCanonicalSlotValueKind.ForwardedSlot
            ? RazorVueExpressionBindingKind.SlotReference
            : ClassifyBindingKind(snapshot, operation);

    private static RazorVueTemplateEncodability ClassifySlotTemplateEncodability(
        IOperation? operation,
        RazorVueCanonicalSlotValueKind valueKind)
        => valueKind == RazorVueCanonicalSlotValueKind.ForwardedSlot
            ? RazorVueTemplateEncodability.DirectTemplate
            : ClassifyTemplateEncodability(operation);

    private static string GetForwardedSlotName(
        RazorVueSemanticSnapshot snapshot,
        IOperation operation,
        VueSlotDescriptor slotDescriptor)
    {
        var current = Unwrap(operation);
        if (current is not IPropertyReferenceOperation property ||
            !TryGetCurrentComponentSlotDescriptor(snapshot, property, out var currentSlot))
        {
            throw CreateUnsupportedExpressionException(
                snapshot,
                operation,
                $"RazorVue SFC slot forwarding expected a current-component RenderFragment parameter for slot '{slotDescriptor.PublicName}'.");
        }

        return currentSlot.Name;
    }

    private static bool IsCallableSlotValue(RazorVueSemanticSnapshot snapshot, IOperation operation)
    {
        var current = Unwrap(operation);
        if (current?.Type?.TypeKind == TypeKind.Delegate)
            return true;

        if (current is IPropertyReferenceOperation property &&
            TryGetCurrentComponentSlotDescriptor(snapshot, property, out var currentSlot))
        {
            return !currentSlot.Parameters.IsDefaultOrEmpty;
        }

        return false;
    }

    private static bool TryGetCurrentComponentSlotDescriptor(
        RazorVueSemanticSnapshot snapshot,
        IPropertyReferenceOperation property,
        out VueSlotDescriptor slotDescriptor)
    {
        slotDescriptor = default!;
        if (!IsCurrentParameterProperty(snapshot, property))
            return false;

        foreach (var slot in snapshot.Descriptor.Slots)
        {
            if (string.Equals(slot.PublicName, property.Property.Name, StringComparison.Ordinal))
            {
                slotDescriptor = slot;
                return true;
            }
        }

        return false;
    }

    private static RazorVueCompilationIssueException CreateSlotContextMisuseException(
        RazorVueSemanticSnapshot snapshot,
        VueSlotDescriptor slotDescriptor,
        RazorVueAttributeNode attribute)
    {
        return CreateSlotContextMisuseException(
            snapshot,
            snapshot.Descriptor,
            slotDescriptor,
            attribute.Name,
            attribute.Origins.IsDefaultOrEmpty ? snapshot.Origins.FirstOrDefault() : attribute.Origins[0]);
    }

    private static RazorVueCompilationIssueException CreateSlotContextMisuseException(
        RazorVueSemanticSnapshot snapshot,
        VueComponentDescriptor descriptor,
        VueSlotDescriptor slotDescriptor,
        string publicName,
        RazorVueSourceOrigin? origin)
    {
        var issue = new RazorVueCompilationIssue(
            RazorVueIssueCode.SlotContextMisuse,
            RazorVueIssueSeverity.Error,
            $"Child content parameter '{publicName}' on component '{descriptor.Name}' expects a callable template that accepts '{DescribeSlotContext(slotDescriptor)}'.",
            ImmutableArray<string>.Empty);
        return new RazorVueCompilationIssueException(issue, snapshot.Descriptor.FullName, origin);
    }

    private static RazorVueCompilationIssueException CreateDuplicateSlotValueException(
        RazorVueSemanticSnapshot snapshot,
        VueComponentDescriptor descriptor,
        string publicName,
        RazorVueSourceOrigin? origin)
    {
        var issue = new RazorVueCompilationIssue(
            RazorVueIssueCode.DuplicateSlotValue,
            RazorVueIssueSeverity.Error,
            $"Component '{descriptor.Name}' receives child content parameter '{publicName}' more than once.",
            ImmutableArray<string>.Empty);
        return new RazorVueCompilationIssueException(issue, snapshot.Descriptor.FullName, origin);
    }

    private static string DescribeSlotContext(VueSlotDescriptor slotDescriptor)
        => string.Join(", ", slotDescriptor.Parameters.Select(static parameter => parameter.TypeName));

    private static string EmitTemplateExpression(
        RazorVueSemanticSnapshot snapshot,
        RazorVueExpressionEmitter expressionEmitter,
        IOperation expression,
        ImmutableHashSet<ILocalSymbol> allowedLocalSymbols,
        ImmutableHashSet<IParameterSymbol> allowedParameterSymbols)
    {
        RazorVueTemplateExpressionScopeValidator.Validate(snapshot, expression, allowedLocalSymbols, allowedParameterSymbols);
        try
        {
            return expressionEmitter.EmitTemplateExpression(expression);
        }
        catch (NotSupportedException ex)
        {
            throw CreateUnsupportedExpressionException(snapshot, expression, ex.Message);
        }
    }

    private static RazorVueCompilationIssueException CreateUnsupportedExpressionException(
        RazorVueSemanticSnapshot snapshot,
        IOperation expression,
        string message)
    {
        var origin = expression.Syntax is null
            ? snapshot.Origins.FirstOrDefault()
            : RazorVueSourceOrigin.FromLocation(expression.Syntax.GetLocation(), RazorVueOriginKind.Template);
        var issue = new RazorVueCompilationIssue(
            RazorVueIssueCode.UnsupportedTemplateEncoding,
            RazorVueIssueSeverity.Error,
            message,
            ImmutableArray<string>.Empty);
        return new RazorVueCompilationIssueException(issue, snapshot.Descriptor.FullName, origin);
    }

    private static RazorVueCompilationIssueException CreateUnsupportedExpressionException(
        RazorVueSemanticSnapshot snapshot,
        ImmutableArray<RazorVueSourceOrigin> origins,
        string message)
    {
        var issue = new RazorVueCompilationIssue(
            RazorVueIssueCode.UnsupportedTemplateEncoding,
            RazorVueIssueSeverity.Error,
            message,
            ImmutableArray<string>.Empty);
        return new RazorVueCompilationIssueException(
            issue,
            snapshot.Descriptor.FullName,
            origins.IsDefaultOrEmpty ? snapshot.Origins.FirstOrDefault() : origins[0]);
    }

    private static RazorVueCompilationIssueException CreateUnsupportedCanonicalizationException(
        RazorVueSemanticSnapshot snapshot,
        string detail,
        ImmutableArray<RazorVueSourceOrigin> origins)
    {
        var issue = new RazorVueCompilationIssue(
            RazorVueIssueCode.CanonicalizationFailed,
            RazorVueIssueSeverity.Error,
            $"RazorVue canonicalization does not support render node '{detail}' in component '{snapshot.Descriptor.FullName}'.",
            ImmutableArray<string>.Empty);
        return new RazorVueCompilationIssueException(issue, snapshot.Descriptor.FullName, origins.IsDefaultOrEmpty ? snapshot.Origins.FirstOrDefault() : origins[0]);
    }


    private static RazorVueCompilationIssueException CreateUnknownComponentAttributeException(
        RazorVueSemanticSnapshot snapshot,
        VueComponentDescriptor descriptor,
        RazorVueAttributeNode attribute)
    {
        var issueCode = attribute.Value is not null && IsRenderFragmentLike(attribute.Value)
            ? RazorVueIssueCode.UnknownSlot
            : RazorVueIssueCode.UnknownParameter;
        var message = issueCode == RazorVueIssueCode.UnknownSlot
            ? $"Component '{descriptor.Name}' does not declare a child content parameter named '{attribute.Name}'."
            : $"Component '{descriptor.Name}' does not declare a parameter named '{attribute.Name}'.";
        var issue = new RazorVueCompilationIssue(
            issueCode,
            RazorVueIssueSeverity.Error,
            message,
            ImmutableArray<string>.Empty);
        return new RazorVueCompilationIssueException(issue, snapshot.Descriptor.FullName, attribute.Origins.IsDefaultOrEmpty ? snapshot.Origins.FirstOrDefault() : attribute.Origins[0]);
    }

    private static RazorVueCompilationIssueException CreateUnknownSlotException(
        RazorVueSemanticSnapshot snapshot,
        VueComponentDescriptor descriptor,
        string publicName,
        RazorVueSourceOrigin? origin)
    {
        var issue = new RazorVueCompilationIssue(
            RazorVueIssueCode.UnknownSlot,
            RazorVueIssueSeverity.Error,
            $"Component '{descriptor.Name}' does not declare a child content parameter named '{publicName}'.",
            ImmutableArray<string>.Empty);
        return new RazorVueCompilationIssueException(issue, snapshot.Descriptor.FullName, origin);
    }

    private static RazorVueCompilationIssueException CreateInvalidBindTargetException(
        RazorVueSemanticSnapshot snapshot,
        VueComponentDescriptor descriptor,
        string parameterName,
        RazorVueAttributeNode attribute)
    {
        var issue = new RazorVueCompilationIssue(
            RazorVueIssueCode.InvalidBindTarget,
            RazorVueIssueSeverity.Error,
            $"Component '{descriptor.Name}' does not support two-way binding for parameter '{parameterName}'.",
            ImmutableArray<string>.Empty);
        return new RazorVueCompilationIssueException(
            issue,
            snapshot.Descriptor.FullName,
            attribute.Origins.IsDefaultOrEmpty ? snapshot.Origins.FirstOrDefault() : attribute.Origins[0]);
    }

    private static RazorVueCompilationIssueException CreateDuplicateMappedComponentAttributeException(
        RazorVueSemanticSnapshot snapshot,
        VueComponentDescriptor descriptor,
        RazorVueAttributeNode firstAttribute,
        RazorVueAttributeNode attribute,
        string mappedKind,
        string mappedName)
    {
        var issue = new RazorVueCompilationIssue(
            RazorVueIssueCode.UnknownParameter,
            RazorVueIssueSeverity.Error,
            $"Component '{descriptor.Name}' receives both '{firstAttribute.Name}' and '{attribute.Name}', but both map to {mappedKind} '{mappedName}'. Use only one authoring parameter for that target.",
            ImmutableArray<string>.Empty);
        return new RazorVueCompilationIssueException(
            issue,
            snapshot.Descriptor.FullName,
            attribute.Origins.IsDefaultOrEmpty ? snapshot.Origins.FirstOrDefault() : attribute.Origins[0]);
    }

    private static RazorVueExpressionBindingKind ClassifyBindingKind(
        RazorVueSemanticSnapshot snapshot,
        IOperation? operation)
    {
        var current = Unwrap(operation);
        if (current is null)
            return RazorVueExpressionBindingKind.None;

        return current switch
        {
            ILiteralOperation => RazorVueExpressionBindingKind.Literal,
            IDefaultValueOperation => RazorVueExpressionBindingKind.Literal,
            IParameterReferenceOperation => RazorVueExpressionBindingKind.LocalReference,
            ILocalReferenceOperation => RazorVueExpressionBindingKind.LocalReference,
            IPropertyReferenceOperation property when IsCurrentParameterProperty(snapshot, property) => RazorVueExpressionBindingKind.PropsReference,
            IFieldReferenceOperation => RazorVueExpressionBindingKind.SetupReference,
            IInvocationOperation => RazorVueExpressionBindingKind.RuntimeExpression,
            IBinaryOperation => RazorVueExpressionBindingKind.RuntimeExpression,
            IUnaryOperation => RazorVueExpressionBindingKind.RuntimeExpression,
            IInterpolatedStringOperation => RazorVueExpressionBindingKind.RuntimeExpression,
            IConditionalOperation => RazorVueExpressionBindingKind.RuntimeExpression,
            _ => RazorVueExpressionBindingKind.RuntimeExpression
        };
    }

    private static RazorVueLiteralValueKind ClassifyLiteralValueKind(IOperation? operation)
    {
        var current = Unwrap(operation);
        if (current is null)
            return RazorVueLiteralValueKind.None;

        if (current is IDefaultValueOperation defaultValue)
            return IsNullDefaultValue(defaultValue)
                ? RazorVueLiteralValueKind.Null
                : RazorVueLiteralValueKind.Other;

        if (current is not ILiteralOperation || current.ConstantValue.HasValue != true)
            return RazorVueLiteralValueKind.None;

        return current.ConstantValue.Value switch
        {
            null => RazorVueLiteralValueKind.Null,
            string => RazorVueLiteralValueKind.String,
            char => RazorVueLiteralValueKind.String,
            bool => RazorVueLiteralValueKind.Boolean,
            sbyte or byte or short or ushort or int or uint or long or ulong or float or double or decimal => RazorVueLiteralValueKind.Number,
            _ => RazorVueLiteralValueKind.Other
        };
    }

    private static bool IsNullDefaultValue(IDefaultValueOperation defaultValue)
    {
        var type = defaultValue.Type;
        if (type is null)
            return false;

        if (type.IsReferenceType)
            return true;

        return type is INamedTypeSymbol namedType &&
               namedType.OriginalDefinition.SpecialType == SpecialType.System_Nullable_T;
    }

    private static RazorVueTemplateEncodability ClassifyTemplateEncodability(IOperation? operation)
    {
        var current = Unwrap(operation);
        if (current is null)
            return RazorVueTemplateEncodability.DirectTemplate;

        return current switch
        {
            ILiteralOperation => RazorVueTemplateEncodability.DirectTemplate,
            IDefaultValueOperation => RazorVueTemplateEncodability.DirectTemplate,
            IPropertyReferenceOperation => RazorVueTemplateEncodability.DirectTemplate,
            IFieldReferenceOperation => RazorVueTemplateEncodability.TemplateViaSetupBinding,
            ILocalReferenceOperation => RazorVueTemplateEncodability.TemplateViaSetupBinding,
            IParameterReferenceOperation => RazorVueTemplateEncodability.TemplateViaSetupBinding,
            IBinaryOperation => RazorVueTemplateEncodability.TemplateViaSetupBinding,
            IUnaryOperation => RazorVueTemplateEncodability.TemplateViaSetupBinding,
            IInvocationOperation => RazorVueTemplateEncodability.TemplateViaSetupBinding,
            IInterpolatedStringOperation => RazorVueTemplateEncodability.TemplateViaSetupBinding,
            IConditionalOperation => RazorVueTemplateEncodability.TemplateViaSetupBinding,
            _ => RazorVueTemplateEncodability.NotTemplateEncodable
        };
    }

    private static RazorVueSideEffectClassification ClassifySideEffects(IOperation? operation)
    {
        var current = Unwrap(operation);
        if (current is null)
            return RazorVueSideEffectClassification.None;

        return current switch
        {
            ILiteralOperation => RazorVueSideEffectClassification.None,
            IDefaultValueOperation => RazorVueSideEffectClassification.None,
            IPropertyReferenceOperation => RazorVueSideEffectClassification.None,
            ILocalReferenceOperation => RazorVueSideEffectClassification.None,
            IParameterReferenceOperation => RazorVueSideEffectClassification.None,
            IFieldReferenceOperation => RazorVueSideEffectClassification.SingleEvaluationRequired,
            IBinaryOperation => RazorVueSideEffectClassification.SingleEvaluationRequired,
            IUnaryOperation => RazorVueSideEffectClassification.SingleEvaluationRequired,
            IConditionalOperation => RazorVueSideEffectClassification.SingleEvaluationRequired,
            IInterpolatedStringOperation => RazorVueSideEffectClassification.SingleEvaluationRequired,
            IInvocationOperation => RazorVueSideEffectClassification.RepeatedEvaluationRisk,
            _ => RazorVueSideEffectClassification.RepeatedEvaluationRisk
        };
    }

    private static RazorVueSideEffectClassification CombineSideEffectClassifications(
        params RazorVueSideEffectClassification[] classifications)
    {
        if (classifications.Any(static item => item == RazorVueSideEffectClassification.RepeatedEvaluationRisk))
            return RazorVueSideEffectClassification.RepeatedEvaluationRisk;

        if (classifications.Any(static item => item == RazorVueSideEffectClassification.SingleEvaluationRequired))
            return RazorVueSideEffectClassification.SingleEvaluationRequired;

        return RazorVueSideEffectClassification.None;
    }

    private static bool IsCurrentParameterProperty(RazorVueSemanticSnapshot snapshot, IPropertyReferenceOperation property)
    {
        for (var current = snapshot.ComponentSymbol; current is not null; current = current.BaseType)
        {
            if (RazorVueSymbolIdentity.SameType(property.Property.ContainingType, current))
                return true;
        }

        return false;
    }

    private static bool IsRenderFragmentLike(IOperation operation)
    {
        var type = Unwrap(operation)?.Type as INamedTypeSymbol;
        if (type is null)
            return false;

        var metadataName = type.OriginalDefinition.ToDisplayString();
        return string.Equals(metadataName, "Microsoft.AspNetCore.Components.RenderFragment", StringComparison.Ordinal) ||
               string.Equals(metadataName, "Microsoft.AspNetCore.Components.RenderFragment<T>", StringComparison.Ordinal);
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

    private static ImmutableDictionary<string, VuePropDescriptor> BuildPropsByName(VueComponentDescriptor descriptor)
    {
        var builder = ImmutableDictionary.CreateBuilder<string, VuePropDescriptor>(StringComparer.OrdinalIgnoreCase);
        foreach (var prop in descriptor.Props)
        {
            if (!string.IsNullOrWhiteSpace(prop.PublicName))
                builder[prop.PublicName] = prop;
            if (!string.IsNullOrWhiteSpace(prop.Name))
                builder[prop.Name] = prop;
        }

        return builder.ToImmutable();
    }

    private static VuePropDescriptor? GetCaptureUnmatchedValuesProp(
        RazorVueSemanticSnapshot snapshot,
        VueComponentDescriptor descriptor,
        RazorVueComponentNode component)
    {
        var captureUnmatchedValueProps = descriptor.Props
            .Where(static prop => prop.CaptureUnmatchedValues)
            .Take(2)
            .ToArray();

        return captureUnmatchedValueProps.Length switch
        {
            0 => null,
            1 => captureUnmatchedValueProps[0],
            _ => throw CreateDuplicateCaptureUnmatchedValuesException(snapshot, descriptor, component)
        };
    }

    private static RazorVueCompilationIssueException CreateDuplicateCaptureUnmatchedValuesException(
        RazorVueSemanticSnapshot snapshot,
        VueComponentDescriptor descriptor,
        RazorVueComponentNode component)
    {
        var issue = new RazorVueCompilationIssue(
            RazorVueIssueCode.CanonicalizationFailed,
            RazorVueIssueSeverity.Error,
            $"Component '{descriptor.Name}' declares multiple [Parameter(CaptureUnmatchedValues = true)] sinks; RazorVue requires exactly one.",
            ImmutableArray<string>.Empty);
        return new RazorVueCompilationIssueException(
            issue,
            snapshot.Descriptor.FullName,
            component.Origins.IsDefaultOrEmpty ? snapshot.Origins.FirstOrDefault() : component.Origins[0]);
    }

    private static RazorVueCompilationIssueException CreateUnsupportedComponentSpreadException(
        RazorVueSemanticSnapshot snapshot,
        VueComponentDescriptor descriptor,
        RazorVueAttributeSpreadNode spread)
    {
        var issue = new RazorVueCompilationIssue(
            RazorVueIssueCode.UnknownParameter,
            RazorVueIssueSeverity.Error,
            $"Component '{descriptor.Name}' does not declare a [Parameter(CaptureUnmatchedValues = true)] sink for arbitrary attributes.",
            ImmutableArray<string>.Empty);
        return new RazorVueCompilationIssueException(
            issue,
            snapshot.Descriptor.FullName,
            spread.Origins.IsDefaultOrEmpty ? snapshot.Origins.FirstOrDefault() : spread.Origins[0]);
    }

    private static ImmutableDictionary<string, VueEmitDescriptor> BuildEmitsByAlias(VueComponentDescriptor descriptor)
    {
        var builder = ImmutableDictionary.CreateBuilder<string, VueEmitDescriptor>(StringComparer.Ordinal);
        foreach (var emit in descriptor.Emits)
        {
            if (!string.IsNullOrWhiteSpace(emit.RazorAlias))
                builder[emit.RazorAlias!] = emit;
        }

        return builder.ToImmutable();
    }

    private static IOperation? Unwrap(IOperation? operation)
        => RazorVueOperationNormalizer.Unwrap(operation);

    private static ImmutableDictionary<string, VueComponentDescriptor> ResolveComponents(
        RazorVueCompilationContext context,
        RazorVueSemanticSnapshot snapshot,
        RazorVueRenderFragment renderTree)
        => RazorVueArtifactFactory.ResolveComponentsForCanonicalization(context, snapshot, renderTree);

    private static ImmutableArray<string> BuildImports(
        ImmutableDictionary<string, VueComponentDescriptor> resolvedComponents,
        ImmutableArray<RazorVueCompilerImportBinding> compilerImports)
        => RazorVueArtifactFactory.BuildImportsForCanonicalization(resolvedComponents, compilerImports);

    private static ImmutableArray<string> BuildStyles(
        VueComponentDescriptor descriptor,
        ImmutableDictionary<string, VueComponentDescriptor> resolvedComponents)
        => RazorVueArtifactFactory.BuildStylesForCanonicalization(descriptor, resolvedComponents);

    private static ImmutableArray<string> BuildPluginRequirements(
        VueComponentDescriptor descriptor,
        ImmutableDictionary<string, VueComponentDescriptor> resolvedComponents)
        => RazorVueArtifactFactory.BuildPluginRequirementsForCanonicalization(descriptor, resolvedComponents);

    private static RazorVueExpressionEmitter CreateExpressionEmitter(
        RazorVueSemanticSnapshot snapshot,
        ImmutableDictionary<string, VueComponentDescriptor> resolvedComponents)
        => RazorVueArtifactFactory.CreateExpressionEmitterForCanonicalization(snapshot, resolvedComponents);

    private static VueRuntimeHints BuildHints(RazorVueSemanticSnapshot snapshot, RazorVueRenderFragment renderTree)
        => RazorVueArtifactFactory.BuildHintsForCanonicalization(snapshot, renderTree);

    private static string NormalizeRelativePath(string relativePath)
        => RazorVueArtifactFactory.NormalizeRelativePathForCanonicalization(relativePath);
}
