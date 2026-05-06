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

    public RazorVueCanonicalHModelFactory()
        : this(BuildRenderTreeTemplateFrontend.Instance)
    {
    }

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
        var imports = BuildImports(resolvedComponents);
        var styles = BuildStyles(snapshot.Descriptor, resolvedComponents);
        var pluginRequirements = BuildPluginRequirements(snapshot.Descriptor, resolvedComponents);
        var expressionEmitter = CreateExpressionEmitter(snapshot, resolvedComponents);
        var template = CreateTemplateFragment(snapshot, expressionEmitter, resolvedComponents, renderTree);
        var sourceOrigins = snapshot.Origins.AddRange(expressionEmitter.CollectOrigins(renderTree));
        var hints = BuildHints(snapshot, renderTree);

        return new RazorVueCanonicalHComponentModel(
            ComponentName: snapshot.Descriptor.Name,
            ComponentFullName: snapshot.Descriptor.FullName,
            RelativeComponentPath: NormalizeRelativePath(snapshot.Descriptor.ImportSpecifier),
            Descriptor: snapshot.Descriptor,
            Imports: imports,
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
        RazorVueRenderFragment fragment)
    {
        if (fragment.Children.IsDefaultOrEmpty)
            return RazorVueCanonicalTemplateFragment.Empty;

        return new RazorVueCanonicalTemplateFragment(
            fragment.Children
                .Select(node => CreateTemplateNode(snapshot, expressionEmitter, resolvedComponents, node))
                .ToImmutableArray());
    }

    private static RazorVueCanonicalTemplateNode CreateTemplateNode(
        RazorVueSemanticSnapshot snapshot,
        RazorVueExpressionEmitter expressionEmitter,
        ImmutableDictionary<string, VueComponentDescriptor> resolvedComponents,
        RazorVueRenderNode node)
        => node switch
        {
            RazorVueElementNode element => new RazorVueCanonicalElementNode(
                TagName: element.TagName,
                Attributes: CreateHtmlAttributeBindings(snapshot, expressionEmitter, element.Attributes),
                Children: CreateTemplateFragment(snapshot, expressionEmitter, resolvedComponents, element.Children),
                TemplateEncodability: RazorVueTemplateEncodability.DirectTemplate,
                SideEffectClassification: RazorVueSideEffectClassification.None,
                SourceOrigins: element.Origins),
            RazorVueComponentNode component => CreateComponentNode(snapshot, expressionEmitter, resolvedComponents, component),
            RazorVueTextNode text => new RazorVueCanonicalTextNode(
                Text: text.Text,
                TemplateEncodability: RazorVueTemplateEncodability.DirectTemplate,
                SideEffectClassification: RazorVueSideEffectClassification.None,
                SourceOrigins: text.Origins),
            RazorVueExpressionNode expression => CreateInterpolationNode(snapshot, expressionEmitter, expression),
            RazorVueSlotOutletNode slot => new RazorVueCanonicalSlotOutletNode(
                SlotName: slot.SlotName,
                ArgumentExpressionText: slot.Argument is null ? null : EmitTemplateExpression(snapshot, expressionEmitter, slot.Argument),
                BindingKind: ClassifyBindingKind(snapshot, slot.Argument),
                TemplateEncodability: ClassifyTemplateEncodability(slot.Argument),
                SideEffectClassification: ClassifySideEffects(slot.Argument),
                SourceOrigins: slot.Origins),
            RazorVueConditionalNode conditional => new RazorVueCanonicalConditionalNode(
                ConditionExpressionText: EmitTemplateExpression(snapshot, expressionEmitter, conditional.Condition),
                BindingKind: ClassifyBindingKind(snapshot, conditional.Condition),
                WhenTrue: CreateTemplateFragment(snapshot, expressionEmitter, resolvedComponents, conditional.WhenTrue),
                WhenFalse: CreateTemplateFragment(snapshot, expressionEmitter, resolvedComponents, conditional.WhenFalse),
                TemplateEncodability: RazorVueTemplateEncodability.TemplateViaSetupBinding,
                SideEffectClassification: ClassifySideEffects(conditional.Condition),
                SourceOrigins: conditional.Origins),
            RazorVueForEachNode loop => new RazorVueCanonicalForEachNode(
                ItemName: loop.ItemName,
                SourceExpressionText: EmitTemplateExpression(snapshot, expressionEmitter, loop.Source),
                BindingKind: ClassifyBindingKind(snapshot, loop.Source),
                Body: CreateTemplateFragment(snapshot, expressionEmitter, resolvedComponents, loop.Body),
                TemplateEncodability: RazorVueTemplateEncodability.TemplateViaSetupBinding,
                SideEffectClassification: ClassifySideEffects(loop.Source),
                SourceOrigins: loop.Origins),
            _ => throw CreateUnsupportedCanonicalizationException(snapshot, node.GetType().Name, node.Origins)
        };

    private static RazorVueCanonicalComponentNode CreateComponentNode(
        RazorVueSemanticSnapshot snapshot,
        RazorVueExpressionEmitter expressionEmitter,
        ImmutableDictionary<string, VueComponentDescriptor> resolvedComponents,
        RazorVueComponentNode component)
    {
        resolvedComponents.TryGetValue(component.ComponentName, out var resolvedDescriptor);
        if (resolvedDescriptor is null)
            throw CreateUnsupportedCanonicalizationException(snapshot, component.ComponentName, component.Origins);

        var slotBindings = CreateComponentSlotBindings(snapshot, expressionEmitter, component, resolvedDescriptor);
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
                SourceOrigins: component.Children.Children
                    .SelectMany(static child => child.Origins)
                    .ToImmutableArray()));
        }

        return new RazorVueCanonicalComponentNode(
            ComponentName: component.ComponentName,
            ComponentFullName: component.ComponentFullName,
            ResolutionName: component.ResolutionName,
            ResolvedDescriptor: resolvedDescriptor,
            Attributes: CreateComponentAttributeBindings(snapshot, expressionEmitter, component, resolvedDescriptor),
            Slots: slotBindings,
            Children: CreateTemplateFragment(snapshot, expressionEmitter, resolvedComponents, component.Children),
            TemplateEncodability: RazorVueTemplateEncodability.DirectTemplate,
            SideEffectClassification: RazorVueSideEffectClassification.None,
            SourceOrigins: component.Origins);
    }

    private static RazorVueCanonicalInterpolationNode CreateInterpolationNode(
        RazorVueSemanticSnapshot snapshot,
        RazorVueExpressionEmitter expressionEmitter,
        RazorVueExpressionNode expression)
    {
        var expressionText = EmitTemplateExpression(snapshot, expressionEmitter, expression.Expression);
        return new RazorVueCanonicalInterpolationNode(
            ExpressionText: expressionText,
            BindingKind: ClassifyBindingKind(snapshot, expression.Expression),
            TemplateEncodability: ClassifyTemplateEncodability(expression.Expression),
            SideEffectClassification: ClassifySideEffects(expression.Expression),
            SourceOrigins: expression.Origins);
    }

    private static ImmutableArray<RazorVueCanonicalAttributeBinding> CreateHtmlAttributeBindings(
        RazorVueSemanticSnapshot snapshot,
        RazorVueExpressionEmitter expressionEmitter,
        ImmutableArray<RazorVueAttributeNode> attributes)
    {
        if (attributes.IsDefaultOrEmpty)
            return ImmutableArray<RazorVueCanonicalAttributeBinding>.Empty;

        return attributes.Select(attribute => new RazorVueCanonicalAttributeBinding(
                Name: attribute.Name,
                ExpressionText: attribute.Value is null ? null : EmitTemplateExpression(snapshot, expressionEmitter, attribute.Value),
                AttributeKind: RazorVueCanonicalAttributeKind.HtmlAttribute,
                BindingKind: ClassifyBindingKind(snapshot, attribute.Value),
                TemplateEncodability: ClassifyTemplateEncodability(attribute.Value),
                SourceOrigins: attribute.Origins))
            .ToImmutableArray();
    }

    private static ImmutableArray<RazorVueCanonicalAttributeBinding> CreateComponentAttributeBindings(
        RazorVueSemanticSnapshot snapshot,
        RazorVueExpressionEmitter expressionEmitter,
        RazorVueComponentNode component,
        VueComponentDescriptor descriptor)
    {
        if (component.Attributes.IsDefaultOrEmpty)
            return ImmutableArray<RazorVueCanonicalAttributeBinding>.Empty;

        var propsByName = BuildPropsByName(descriptor);
        var emitsByAlias = BuildEmitsByAlias(descriptor);
        var slotsByPublicName = BuildSlotsByPublicName(descriptor);
        var builder = ImmutableArray.CreateBuilder<RazorVueCanonicalAttributeBinding>();

        foreach (var attribute in component.Attributes)
        {
            if (slotsByPublicName.ContainsKey(attribute.Name))
                continue;

            if (emitsByAlias.TryGetValue(attribute.Name, out var emitDescriptor))
            {
                builder.Add(new RazorVueCanonicalAttributeBinding(
                    Name: emitDescriptor.Name,
                    ExpressionText: attribute.Value is null ? null : EmitTemplateExpression(snapshot, expressionEmitter, attribute.Value),
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
                    ExpressionText: attribute.Value is null ? null : EmitTemplateExpression(snapshot, expressionEmitter, attribute.Value),
                    AttributeKind: RazorVueCanonicalAttributeKind.ComponentProp,
                    BindingKind: ClassifyBindingKind(snapshot, attribute.Value),
                    TemplateEncodability: ClassifyTemplateEncodability(attribute.Value),
                    SourceOrigins: attribute.Origins));
                continue;
            }

            throw CreateUnknownComponentAttributeException(snapshot, descriptor, attribute);
        }

        return builder.ToImmutable();
    }

    private static ImmutableArray<RazorVueCanonicalSlotBinding> CreateComponentSlotBindings(
        RazorVueSemanticSnapshot snapshot,
        RazorVueExpressionEmitter expressionEmitter,
        RazorVueComponentNode component,
        VueComponentDescriptor descriptor)
    {
        if (component.Attributes.IsDefaultOrEmpty)
            return ImmutableArray<RazorVueCanonicalSlotBinding>.Empty;

        var slotsByPublicName = BuildSlotsByPublicName(descriptor);
        var builder = ImmutableArray.CreateBuilder<RazorVueCanonicalSlotBinding>();

        foreach (var attribute in component.Attributes)
        {
            if (!slotsByPublicName.TryGetValue(attribute.Name, out var slotDescriptor))
                continue;

            if (attribute.Value is not null &&
                !slotDescriptor.Parameters.IsDefaultOrEmpty &&
                !IsCallableSlotValue(snapshot, attribute.Value))
            {
                throw CreateSlotContextMisuseException(snapshot, slotDescriptor, attribute);
            }

            var valueKind = ClassifySlotValueKind(snapshot, attribute.Value, slotDescriptor);
            var valueExpressionText = valueKind == RazorVueCanonicalSlotValueKind.ForwardedSlot || attribute.Value is null
                ? null
                : EmitTemplateExpression(snapshot, expressionEmitter, attribute.Value);
            var forwardedSlotName = valueKind == RazorVueCanonicalSlotValueKind.ForwardedSlot
                ? GetForwardedSlotName(snapshot, attribute.Value!, slotDescriptor)
                : null;

            builder.Add(new RazorVueCanonicalSlotBinding(
                SlotName: slotDescriptor.Name,
                IsDefault: slotDescriptor.IsDefault,
                ParameterName: slotDescriptor.Parameters.IsDefaultOrEmpty ? null : slotDescriptor.Parameters[0].Name,
                ValueKind: valueKind,
                ValueExpressionText: valueExpressionText,
                ForwardedSlotName: forwardedSlotName,
                BindingKind: ClassifySlotBindingKind(snapshot, attribute.Value, valueKind),
                TemplateEncodability: ClassifySlotTemplateEncodability(attribute.Value, valueKind),
                SourceOrigins: attribute.Origins));
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
        var issue = new RazorVueCompilationIssue(
            RazorVueIssueCode.SlotContextMisuse,
            RazorVueIssueSeverity.Error,
            $"Child content parameter '{attribute.Name}' on component '{snapshot.Descriptor.Name}' expects a callable template that accepts '{DescribeSlotContext(slotDescriptor)}'.",
            ImmutableArray<string>.Empty);
        return new RazorVueCompilationIssueException(
            issue,
            snapshot.Descriptor.FullName,
            attribute.Origins.IsDefaultOrEmpty ? snapshot.Origins.FirstOrDefault() : attribute.Origins[0]);
    }

    private static string DescribeSlotContext(VueSlotDescriptor slotDescriptor)
        => string.Join(", ", slotDescriptor.Parameters.Select(static parameter => parameter.TypeName));

    private static string EmitTemplateExpression(
        RazorVueSemanticSnapshot snapshot,
        RazorVueExpressionEmitter expressionEmitter,
        IOperation expression)
    {
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

    private static bool IsCurrentParameterProperty(RazorVueSemanticSnapshot snapshot, IPropertyReferenceOperation property)
    {
        for (var current = snapshot.ComponentSymbol; current is not null; current = current.BaseType)
        {
            if (SymbolEqualityComparer.Default.Equals(property.Property.ContainingType, current))
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

    private static ImmutableDictionary<string, VueSlotDescriptor> BuildSlotsByPublicName(VueComponentDescriptor descriptor)
        => descriptor.Slots.ToImmutableDictionary(static slot => slot.PublicName, static slot => slot, StringComparer.Ordinal);

    private static IOperation? Unwrap(IOperation? operation)
        => RazorVueOperationNormalizer.Unwrap(operation);

    private static ImmutableDictionary<string, VueComponentDescriptor> ResolveComponents(
        RazorVueCompilationContext context,
        RazorVueSemanticSnapshot snapshot,
        RazorVueRenderFragment renderTree)
        => RazorVueArtifactFactory.ResolveComponentsForCanonicalization(context, snapshot, renderTree);

    private static ImmutableArray<string> BuildImports(ImmutableDictionary<string, VueComponentDescriptor> resolvedComponents)
        => RazorVueArtifactFactory.BuildImportsForCanonicalization(resolvedComponents);

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
