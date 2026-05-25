using System.Collections.Immutable;
using System.Linq;
using Jazor.Compiler;
using Jazor.RazorVue;
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
        var imperativeRootProgram = TryCreateImperativeRootProgram(snapshot, renderTree, expressionEmitter);
        var template = imperativeRootProgram is null
            ? CreateTemplateFragment(
                snapshot,
                expressionEmitter,
                resolvedComponents,
                renderTree,
                EmptyLocalScope,
                EmptyParameterScope)
            : RazorVueCanonicalTemplateFragment.Empty;
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
            ImperativeRootProgram: imperativeRootProgram,
            Setup: new RazorVueCanonicalSetupModel(
                snapshot.Logic.Properties,
                snapshot.Logic.Fields,
                snapshot.Logic.Methods,
                expressionEmitter.GetRequiredSetupProperties(),
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

        var currentLocalScope = allowedLocalSymbols;
        var currentParameterScope = allowedParameterSymbols;
        var builder = ImmutableArray.CreateBuilder<RazorVueCanonicalTemplateNode>(fragment.Children.Length);
        foreach (var node in fragment.Children)
        {
            builder.Add(CreateTemplateNode(
                snapshot,
                expressionEmitter,
                resolvedComponents,
                node,
                currentLocalScope,
                currentParameterScope));

            if (node is RazorVueLocalDeclarationNode localDeclaration)
                currentLocalScope = RazorVueTemplateExpressionScopeValidator.AddIfPresent(currentLocalScope, localDeclaration.LocalSymbol);
        }

        return new RazorVueCanonicalTemplateFragment(builder.ToImmutable());
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
                Key: CreateNodeKey(snapshot, expressionEmitter, element.Key, allowedLocalSymbols, allowedParameterSymbols),
                Attributes: CreateHtmlAttributeBindings(snapshot, expressionEmitter, element.Attributes, allowedLocalSymbols, allowedParameterSymbols),
                Children: CreateTemplateFragment(snapshot, expressionEmitter, resolvedComponents, element.Children, allowedLocalSymbols, allowedParameterSymbols),
                TemplateEncodability: RazorVueTemplateEncodability.DirectTemplate,
                TemplateExpressionSafety: RazorVueTemplateExpressionSafety.DirectTemplateSafe,
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
                TemplateExpressionSafety: RazorVueTemplateExpressionSafety.DirectTemplateSafe,
                SideEffectClassification: RazorVueSideEffectClassification.None,
                SourceOrigins: text.Origins),
            RazorVueExpressionNode expression => CreateInterpolationNode(
                snapshot,
                expressionEmitter,
                expression,
                allowedLocalSymbols,
                allowedParameterSymbols),
            RazorVueLocalDeclarationNode localDeclaration => new RazorVueCanonicalLocalDeclarationNode(
                LocalName: localDeclaration.LocalSymbol.Name,
                InitializerExpressionText: EmitTemplateExpression(
                    snapshot,
                    expressionEmitter,
                    localDeclaration.Initializer,
                    allowedLocalSymbols,
                    allowedParameterSymbols),
                BindingKind: ClassifyBindingKind(snapshot, localDeclaration.Initializer),
                TemplateEncodability: ClassifyTemplateEncodability(localDeclaration.Initializer),
                TemplateExpressionSafety: ClassifyTemplateExpressionSafety(snapshot, localDeclaration.Initializer),
                SideEffectClassification: ClassifySideEffects(localDeclaration.Initializer),
                SourceOrigins: localDeclaration.Origins),
            RazorVueTemplateScopeNode templateScope => new RazorVueCanonicalTemplateScopeNode(
                ScopeName: templateScope.ScopeName,
                InitializerExpressionText: EmitTemplateExpression(
                    snapshot,
                    expressionEmitter,
                    templateScope.Initializer,
                    allowedLocalSymbols,
                    allowedParameterSymbols),
                BindingKind: ClassifyBindingKind(snapshot, templateScope.Initializer),
                Children: CreateTemplateFragment(
                    snapshot,
                    expressionEmitter,
                    resolvedComponents,
                    templateScope.Children,
                    allowedLocalSymbols,
                    RazorVueTemplateExpressionScopeValidator.AddIfPresent(allowedParameterSymbols, templateScope.ScopeParameterSymbol)),
                TemplateEncodability: ClassifyTemplateEncodability(templateScope.Initializer),
                TemplateExpressionSafety: ClassifyTemplateExpressionSafety(snapshot, templateScope.Initializer),
                SideEffectClassification: ClassifySideEffects(templateScope.Initializer),
                SourceOrigins: templateScope.Origins),
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
                TemplateExpressionSafety: ClassifyTemplateExpressionSafety(snapshot, slot.Argument),
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
                TemplateExpressionSafety: ClassifyTemplateExpressionSafety(snapshot, conditional.Condition),
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
                TemplateExpressionSafety: ClassifyTemplateExpressionSafety(snapshot, loop.Source),
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
                InitialValueTemplateExpressionSafety: ClassifyTemplateExpressionSafety(snapshot, loop.InitialValue),
                InitialValueSideEffectClassification: ClassifySideEffects(loop.InitialValue),
                ConditionKind: loop.ConditionKind,
                LimitValueExpressionText: EmitTemplateExpression(
                    snapshot,
                    expressionEmitter,
                    loop.LimitValue,
                    allowedLocalSymbols,
                    allowedParameterSymbols),
                LimitValueBindingKind: ClassifyBindingKind(snapshot, loop.LimitValue),
                LimitValueTemplateExpressionSafety: ClassifyTemplateExpressionSafety(snapshot, loop.LimitValue),
                LimitValueSideEffectClassification: ClassifySideEffects(loop.LimitValue),
                StepKind: loop.StepKind,
                StepValueExpressionText: loop.StepValue is null
                    ? null
                    : EmitTemplateExpression(snapshot, expressionEmitter, loop.StepValue, allowedLocalSymbols, allowedParameterSymbols),
                StepValueBindingKind: ClassifyBindingKind(snapshot, loop.StepValue),
                StepValueTemplateExpressionSafety: ClassifyTemplateExpressionSafety(snapshot, loop.StepValue),
                StepValueSideEffectClassification: ClassifySideEffects(loop.StepValue),
                Body: CreateTemplateFragment(
                    snapshot,
                    expressionEmitter,
                    resolvedComponents,
                    loop.Body,
                    RazorVueTemplateExpressionScopeValidator.AddIfPresent(allowedLocalSymbols, loop.VariableSymbol),
                    allowedParameterSymbols),
                TemplateEncodability: RazorVueTemplateEncodability.TemplateViaSetupBinding,
                TemplateExpressionSafety: CombineTemplateExpressionSafety(
                    ClassifyTemplateExpressionSafety(snapshot, loop.InitialValue),
                    ClassifyTemplateExpressionSafety(snapshot, loop.LimitValue),
                    ClassifyTemplateExpressionSafety(snapshot, loop.StepValue)),
                SideEffectClassification: CombineSideEffectClassifications(
                    ClassifySideEffects(loop.InitialValue),
                    ClassifySideEffects(loop.LimitValue),
                    ClassifySideEffects(loop.StepValue)),
                SourceOrigins: loop.Origins),
            RazorVueImperativeBlockNode imperative => throw CreateUnsupportedNestedImperativeRenderException(snapshot, imperative),
            _ => throw CreateUnsupportedCanonicalizationException(snapshot, node.GetType().Name, node.Origins)
        };

    private static RazorVueCanonicalImperativeRootProgram? TryCreateImperativeRootProgram(
        RazorVueSemanticSnapshot snapshot,
        RazorVueRenderFragment renderTree,
        RazorVueExpressionEmitter expressionEmitter)
    {
        if (RequiresRenderFunctionForShouldRenderGate(snapshot, expressionEmitter))
        {
            return new RazorVueCanonicalImperativeRootProgram(
                RazorVueImperativeBlockKind.MethodBody,
                renderTree,
                IsRootOnly: false,
                snapshot.Origins.AddRange(renderTree.Children.SelectMany(static child => child.Origins)).Distinct().ToImmutableArray());
        }

        if (RequiresImperativeScopedReplay(renderTree))
        {
            return new RazorVueCanonicalImperativeRootProgram(
                RazorVueImperativeBlockKind.MethodBody,
                renderTree,
                IsRootOnly: false,
                renderTree.Children.SelectMany(static child => child.Origins).Distinct().ToImmutableArray());
        }

        var imperativeNodes = EnumerateImperativeNodes(renderTree).ToImmutableArray();
        if (imperativeNodes.IsDefaultOrEmpty)
            return null;

        var isRootOnly = renderTree.Children.Length == 1 &&
                         renderTree.Children[0] is RazorVueImperativeBlockNode;
        var kind = isRootOnly
            ? ((RazorVueImperativeBlockNode)renderTree.Children[0]).Kind
            : imperativeNodes[0].Kind;

        return new RazorVueCanonicalImperativeRootProgram(
            kind,
            renderTree,
            isRootOnly,
            imperativeNodes.SelectMany(static imperative => imperative.Origins).Distinct().ToImmutableArray());
    }

    private static bool RequiresImperativeScopedReplay(RazorVueRenderFragment fragment)
    {
        if (fragment.Children.IsDefaultOrEmpty)
            return false;

        foreach (var child in fragment.Children)
        {
            if (RequiresImperativeScopedReplay(child))
                return true;
        }

        return false;
    }

    private static bool RequiresRenderFunctionForShouldRenderGate(
        RazorVueSemanticSnapshot snapshot,
        RazorVueExpressionEmitter expressionEmitter)
        => snapshot.Lifecycle.HasShouldRender &&
           RazorVueSetupAndLifecycleLoweringSupport.DescribeShouldRenderShape(snapshot, expressionEmitter)
               .StartsWith("condition:", StringComparison.Ordinal);

    private static bool RequiresImperativeScopedReplay(RazorVueRenderNode node)
        => node switch
        {
            RazorVueElementNode element =>
                RazorVueOpenNodeReplayHelper.RequiresImperativeScopedReplay(element.ReplayOperations) ||
                RequiresImperativeScopedReplay(element.Children),
            RazorVueComponentNode component =>
                RazorVueOpenNodeReplayHelper.RequiresImperativeScopedReplay(component.ReplayOperations) ||
                RequiresImperativeScopedReplay(component.Children) ||
                RequiresImperativeScopedReplay(component.AmbientDefaultSlotChildren) ||
                component.SlotTemplates.Any(static slot => RequiresImperativeScopedReplay(slot.Children)) ||
                component.ImplicitDefaultSlotAssignments.Any(static assignment => RequiresImperativeScopedReplay(assignment.Children)),
            RazorVueTemplateScopeNode templateScope => RequiresImperativeScopedReplay(templateScope.Children),
            RazorVueConditionalNode conditional => RequiresImperativeScopedReplay(conditional.WhenTrue) || RequiresImperativeScopedReplay(conditional.WhenFalse),
            RazorVueForEachNode loop => RequiresImperativeScopedReplay(loop.Body),
            RazorVueForNode loop => RequiresImperativeScopedReplay(loop.Body),
            _ => false
        };

    private static IEnumerable<RazorVueImperativeBlockNode> EnumerateImperativeNodes(RazorVueRenderFragment fragment)
    {
        if (fragment.Children.IsDefaultOrEmpty)
            yield break;

        foreach (var child in fragment.Children)
        {
            switch (child)
            {
                case RazorVueImperativeBlockNode imperative:
                    yield return imperative;
                    break;
                case RazorVueElementNode element:
                    foreach (var nested in EnumerateImperativeNodes(element.Children))
                        yield return nested;
                    break;
                case RazorVueComponentNode component:
                    foreach (var nested in EnumerateImperativeNodes(component.Children))
                        yield return nested;
                    foreach (var nested in EnumerateImperativeNodes(component.AmbientDefaultSlotChildren))
                        yield return nested;
                    foreach (var slotTemplate in component.SlotTemplates)
                    {
                        foreach (var nested in EnumerateImperativeNodes(slotTemplate.Children))
                            yield return nested;
                    }

                    foreach (var assignment in component.ImplicitDefaultSlotAssignments)
                    {
                        foreach (var nested in EnumerateImperativeNodes(assignment.Children))
                            yield return nested;
                    }

                    break;
                case RazorVueConditionalNode conditional:
                    foreach (var nested in EnumerateImperativeNodes(conditional.WhenTrue))
                        yield return nested;
                    foreach (var nested in EnumerateImperativeNodes(conditional.WhenFalse))
                        yield return nested;
                    break;
                case RazorVueTemplateScopeNode templateScope:
                    foreach (var nested in EnumerateImperativeNodes(templateScope.Children))
                        yield return nested;
                    break;
                case RazorVueForEachNode loop:
                    foreach (var nested in EnumerateImperativeNodes(loop.Body))
                        yield return nested;
                    break;
                case RazorVueForNode loop:
                    foreach (var nested in EnumerateImperativeNodes(loop.Body))
                        yield return nested;
                    break;
            }
        }
    }

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
        if (HasAnyDefaultSlotContent(component))
        {
            var defaultSlotParameterName = TryGetDefaultSlotParameterName(
                resolvedDescriptor,
                component,
                allowedLocalSymbols,
                allowedParameterSymbols);
            slotBindings = slotBindings.Add(new RazorVueCanonicalSlotBinding(
                SlotName: "default",
                IsDefault: true,
                ParameterName: defaultSlotParameterName,
                ValueKind: RazorVueCanonicalSlotValueKind.None,
                ValueExpressionText: null,
                ForwardedSlotName: null,
                BindingKind: RazorVueExpressionBindingKind.None,
                TemplateEncodability: RazorVueTemplateEncodability.DirectTemplate,
                TemplateExpressionSafety: RazorVueTemplateExpressionSafety.DirectTemplateSafe,
                SideEffectClassification: RazorVueSideEffectClassification.None,
                Children: CreateTemplateFragment(
                    snapshot,
                    expressionEmitter,
                    resolvedComponents,
                    GetDefaultSlotFragment(component),
                    allowedLocalSymbols,
                    allowedParameterSymbols),
                SourceOrigins: GetDefaultSlotFragment(component).Children
                    .SelectMany(static child => child.Origins)
                    .ToImmutableArray()));
        }

        return new RazorVueCanonicalComponentNode(
            ComponentName: component.ComponentName,
            ComponentFullName: component.ComponentFullName,
            ResolutionName: component.ResolutionName,
            ResolvedDescriptor: resolvedDescriptor,
            Key: CreateNodeKey(snapshot, expressionEmitter, component.Key, allowedLocalSymbols, allowedParameterSymbols),
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
            TemplateExpressionSafety: RazorVueTemplateExpressionSafety.DirectTemplateSafe,
            SideEffectClassification: RazorVueSideEffectClassification.None,
            SourceOrigins: component.Origins);
    }

    private static RazorVueCanonicalNodeKey? CreateNodeKey(
        RazorVueSemanticSnapshot snapshot,
        RazorVueExpressionEmitter expressionEmitter,
        RazorVueNodeKey? key,
        ImmutableHashSet<ILocalSymbol> allowedLocalSymbols,
        ImmutableHashSet<IParameterSymbol> allowedParameterSymbols)
    {
        if (key is null)
            return null;

        return new RazorVueCanonicalNodeKey(
            ExpressionText: key.CapturedBindings.IsDefaultOrEmpty
                ? EmitTemplateExpression(snapshot, expressionEmitter, key.Expression, allowedLocalSymbols, allowedParameterSymbols)
                : expressionEmitter.EmitCapturedTemplateExpression(key.Expression, key.CapturedBindings, allowedLocalSymbols, allowedParameterSymbols),
            BindingKind: key.CapturedBindings.IsDefaultOrEmpty
                ? ClassifyBindingKind(snapshot, key.Expression)
                : RazorVueExpressionBindingKind.RuntimeExpression,
            TemplateEncodability: key.CapturedBindings.IsDefaultOrEmpty
                ? ClassifyTemplateEncodability(key.Expression)
                : RazorVueTemplateEncodability.TemplateViaSetupBinding,
            TemplateExpressionSafety: key.CapturedBindings.IsDefaultOrEmpty
                ? ClassifyTemplateExpressionSafety(snapshot, key.Expression)
                : RazorVueTemplateExpressionSafety.RequiresSetupBinding,
            SideEffectClassification: key.CapturedBindings.IsDefaultOrEmpty
                ? ClassifySideEffects(key.Expression)
                : RazorVueSideEffectClassification.SingleEvaluationRequired,
            SourceOrigins: key.Origins);
    }

    private static void ValidateDefaultLibrarySlotUsage(
        RazorVueSemanticSnapshot snapshot,
        VueComponentDescriptor descriptor,
        RazorVueComponentNode component)
    {
        if (descriptor.SourceKind != VueComponentSourceKind.LibraryComponent ||
            !HasAnyDefaultSlotAssignment(component))
        {
            return;
        }

        var origin = GetDefaultSlotFragment(component).Children
            .SelectMany(static child => child.Origins)
            .FirstOrDefault() ??
            component.ImplicitDefaultSlotAssignments.SelectMany(static assignment => assignment.Origins).FirstOrDefault() ??
            component.AmbientDefaultSlotChildren.Children.SelectMany(static child => child.Origins).FirstOrDefault() ??
            component.Origins.FirstOrDefault();

        if (VueSlotResolver.TryResolve(descriptor.Slots, "ChildContent", out var defaultSlot))
        {
            return;
        }

        throw CreateUnknownSlotException(snapshot, descriptor, "ChildContent", origin);
    }

    private static string? TryGetDefaultSlotParameterName(
        VueComponentDescriptor descriptor,
        RazorVueComponentNode component,
        ImmutableHashSet<ILocalSymbol> allowedLocalSymbols,
        ImmutableHashSet<IParameterSymbol> allowedParameterSymbols)
        => VueSlotResolver.TryResolve(descriptor.Slots, "ChildContent", out var defaultSlot) &&
           !defaultSlot.Descriptor.Parameters.IsDefaultOrEmpty
            ? RazorVueSlotParameterNames.CreateImplicitDefaultSlotParameterName(
                defaultSlot.Descriptor.Parameters[0].Name,
                allowedLocalSymbols,
                allowedParameterSymbols)
            : null;

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
        if (HasAnyDefaultSlotAssignment(component) &&
            VueSlotResolver.TryResolve(descriptor.Slots, "ChildContent", out var childContentSlot))
        {
            var defaultSlotAssignmentCount = GetDefaultSlotAssignmentCount(component);
            if (defaultSlotAssignmentCount > 1)
            {
                throw CreateDuplicateSlotValueException(
                    snapshot,
                    descriptor,
                    "ChildContent",
                    GetSecondDefaultSlotAssignmentOrigin(snapshot, component));
            }

            assignedSlots.Add(childContentSlot.SlotName);
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

    private static RazorVueSourceOrigin? GetSecondDefaultSlotAssignmentOrigin(
        RazorVueSemanticSnapshot snapshot,
        RazorVueComponentNode component)
    {
        if (HasAmbientDefaultSlotContent(component) && component.ImplicitDefaultSlotAssignments.Length > 0)
        {
            return component.ImplicitDefaultSlotAssignments[0].Origins.IsDefaultOrEmpty
                ? snapshot.Origins.FirstOrDefault()
                : component.ImplicitDefaultSlotAssignments[0].Origins[0];
        }

        if (component.ImplicitDefaultSlotAssignments.Length > 1)
        {
            return component.ImplicitDefaultSlotAssignments[1].Origins.IsDefaultOrEmpty
                ? snapshot.Origins.FirstOrDefault()
                : component.ImplicitDefaultSlotAssignments[1].Origins[0];
        }

        return component.AmbientDefaultSlotChildren.Children
            .SelectMany(static child => child.Origins)
            .FirstOrDefault() ?? snapshot.Origins.FirstOrDefault();
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
            TemplateExpressionSafety: ClassifyTemplateExpressionSafety(snapshot, expression.Expression),
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
                    var isHtmlEvent = IsElementDomEventAttribute(attribute, out var eventName);
                    var eventModifiers = CreateCanonicalEventModifiers(
                        snapshot,
                        expressionEmitter,
                        attribute.EventModifiers,
                        allowedLocalSymbols,
                        allowedParameterSymbols);
                    var templateEncodability = attribute.CapturedBindings.IsDefaultOrEmpty
                        ? ClassifyTemplateEncodability(attribute.Value)
                        : RazorVueTemplateEncodability.TemplateViaSetupBinding;
                    var templateExpressionSafety = attribute.CapturedBindings.IsDefaultOrEmpty
                        ? ClassifyTemplateExpressionSafety(snapshot, attribute.Value)
                        : RazorVueTemplateExpressionSafety.RequiresSetupBinding;
                    var sideEffectClassification = attribute.CapturedBindings.IsDefaultOrEmpty
                        ? ClassifySideEffects(attribute.Value)
                        : RazorVueSideEffectClassification.SingleEvaluationRequired;
                    builder.Add(new RazorVueCanonicalAttributeBinding(
                        Name: isHtmlEvent ? eventName : attribute.Name,
                        ExpressionText: attribute.Value is null
                            ? null
                            : attribute.CapturedBindings.IsDefaultOrEmpty
                                ? EmitTemplateExpression(snapshot, expressionEmitter, attribute.Value, allowedLocalSymbols, allowedParameterSymbols)
                                : expressionEmitter.EmitCapturedTemplateExpression(attribute.Value, attribute.CapturedBindings, allowedLocalSymbols, allowedParameterSymbols),
                        LiteralValueKind: ClassifyLiteralValueKind(attribute.Value),
                        AttributeKind: isHtmlEvent
                            ? RazorVueCanonicalAttributeKind.HtmlEvent
                            : RazorVueCanonicalAttributeKind.HtmlAttribute,
                        BindingKind: attribute.CapturedBindings.IsDefaultOrEmpty
                            ? ClassifyBindingKind(snapshot, attribute.Value)
                            : RazorVueExpressionBindingKind.RuntimeExpression,
                        TemplateEncodability: CombineTemplateEncodability(templateEncodability, eventModifiers.TemplateEncodability),
                        TemplateExpressionSafety: CombineTemplateExpressionSafety(
                            templateExpressionSafety,
                            eventModifiers.TemplateExpressionSafety),
                        SideEffectClassification: CombineSideEffectClassifications(
                            sideEffectClassification,
                            eventModifiers.SideEffectClassification),
                        EventModifiers: eventModifiers,
                        SourceOrigins: attribute.Origins));
                    break;
                case RazorVueAttributeSpreadNode spread:
                    builder.Add(new RazorVueCanonicalAttributeSpreadBinding(
                        ExpressionText: spread.CapturedBindings.IsDefaultOrEmpty
                            ? EmitTemplateExpression(
                                snapshot,
                                expressionEmitter,
                                spread.Expression,
                                allowedLocalSymbols,
                                allowedParameterSymbols)
                            : expressionEmitter.EmitCapturedTemplateExpression(spread.Expression, spread.CapturedBindings, allowedLocalSymbols, allowedParameterSymbols),
                        BindingKind: spread.CapturedBindings.IsDefaultOrEmpty
                            ? ClassifyBindingKind(snapshot, spread.Expression)
                            : RazorVueExpressionBindingKind.RuntimeExpression,
                        TemplateEncodability: spread.CapturedBindings.IsDefaultOrEmpty
                            ? ClassifyTemplateEncodability(spread.Expression)
                            : RazorVueTemplateEncodability.TemplateViaSetupBinding,
                        TemplateExpressionSafety: spread.CapturedBindings.IsDefaultOrEmpty
                            ? ClassifyTemplateExpressionSafety(snapshot, spread.Expression)
                            : RazorVueTemplateExpressionSafety.RequiresSetupBinding,
                        SideEffectClassification: spread.CapturedBindings.IsDefaultOrEmpty
                            ? ClassifySideEffects(spread.Expression)
                            : RazorVueSideEffectClassification.SingleEvaluationRequired,
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

        var emitsByAlias = BuildEmitsByAlias(descriptor);
        var unmatchedValuesProp = GetCaptureUnmatchedValuesProp(snapshot, descriptor, component);
        ValidateInvalidBindTargets(snapshot, descriptor, component, emitsByAlias);
        ValidateDuplicateMappedComponentAttributes(snapshot, descriptor, component, emitsByAlias);
        var builder = ImmutableArray.CreateBuilder<RazorVueCanonicalAttributeEntry>();

        foreach (var attributeEntry in component.Attributes)
        {
            if (attributeEntry is RazorVueAttributeSpreadNode spread)
            {
                if (unmatchedValuesProp is null)
                    throw CreateUnsupportedComponentSpreadException(snapshot, descriptor, spread);

                builder.Add(new RazorVueCanonicalAttributeSpreadBinding(
                    ExpressionText: spread.CapturedBindings.IsDefaultOrEmpty
                        ? EmitTemplateExpression(
                            snapshot,
                            expressionEmitter,
                            spread.Expression,
                            allowedLocalSymbols,
                            allowedParameterSymbols)
                        : expressionEmitter.EmitCapturedTemplateExpression(spread.Expression, spread.CapturedBindings, allowedLocalSymbols, allowedParameterSymbols),
                    BindingKind: spread.CapturedBindings.IsDefaultOrEmpty
                        ? ClassifyBindingKind(snapshot, spread.Expression)
                        : RazorVueExpressionBindingKind.RuntimeExpression,
                    TemplateEncodability: spread.CapturedBindings.IsDefaultOrEmpty
                        ? ClassifyTemplateEncodability(spread.Expression)
                        : RazorVueTemplateEncodability.TemplateViaSetupBinding,
                    TemplateExpressionSafety: spread.CapturedBindings.IsDefaultOrEmpty
                        ? ClassifyTemplateExpressionSafety(snapshot, spread.Expression)
                        : RazorVueTemplateExpressionSafety.RequiresSetupBinding,
                    SideEffectClassification: spread.CapturedBindings.IsDefaultOrEmpty
                        ? ClassifySideEffects(spread.Expression)
                        : RazorVueSideEffectClassification.SingleEvaluationRequired,
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
                        : attribute.CapturedBindings.IsDefaultOrEmpty
                            ? EmitTemplateExpression(snapshot, expressionEmitter, attribute.Value, allowedLocalSymbols, allowedParameterSymbols)
                            : expressionEmitter.EmitCapturedTemplateExpression(attribute.Value, attribute.CapturedBindings, allowedLocalSymbols, allowedParameterSymbols),
                    LiteralValueKind: ClassifyLiteralValueKind(attribute.Value),
                    AttributeKind: RazorVueCanonicalAttributeKind.ComponentEvent,
                    BindingKind: attribute.CapturedBindings.IsDefaultOrEmpty
                        ? ClassifyBindingKind(snapshot, attribute.Value)
                        : RazorVueExpressionBindingKind.RuntimeExpression,
                    TemplateEncodability: attribute.CapturedBindings.IsDefaultOrEmpty
                        ? ClassifyTemplateEncodability(attribute.Value)
                        : RazorVueTemplateEncodability.TemplateViaSetupBinding,
                    TemplateExpressionSafety: attribute.CapturedBindings.IsDefaultOrEmpty
                        ? ClassifyTemplateExpressionSafety(snapshot, attribute.Value)
                        : RazorVueTemplateExpressionSafety.RequiresSetupBinding,
                    SideEffectClassification: attribute.CapturedBindings.IsDefaultOrEmpty
                        ? ClassifySideEffects(attribute.Value)
                        : RazorVueSideEffectClassification.SingleEvaluationRequired,
                    EventModifiers: RazorVueCanonicalEventModifiers.Empty,
                    SourceOrigins: attribute.Origins));
                continue;
            }

            if (VuePropResolver.TryResolve(descriptor.Props, attribute.Name, out var prop))
            {
                builder.Add(new RazorVueCanonicalAttributeBinding(
                    Name: prop.PropName,
                    ExpressionText: attribute.Value is null
                        ? null
                        : attribute.CapturedBindings.IsDefaultOrEmpty
                            ? EmitTemplateExpression(snapshot, expressionEmitter, attribute.Value, allowedLocalSymbols, allowedParameterSymbols)
                            : expressionEmitter.EmitCapturedTemplateExpression(attribute.Value, attribute.CapturedBindings, allowedLocalSymbols, allowedParameterSymbols),
                    LiteralValueKind: ClassifyLiteralValueKind(attribute.Value),
                    AttributeKind: RazorVueCanonicalAttributeKind.ComponentProp,
                    BindingKind: attribute.CapturedBindings.IsDefaultOrEmpty
                        ? ClassifyBindingKind(snapshot, attribute.Value)
                        : RazorVueExpressionBindingKind.RuntimeExpression,
                    TemplateEncodability: attribute.CapturedBindings.IsDefaultOrEmpty
                        ? ClassifyTemplateEncodability(attribute.Value)
                        : RazorVueTemplateEncodability.TemplateViaSetupBinding,
                    TemplateExpressionSafety: attribute.CapturedBindings.IsDefaultOrEmpty
                        ? ClassifyTemplateExpressionSafety(snapshot, attribute.Value)
                        : RazorVueTemplateExpressionSafety.RequiresSetupBinding,
                    SideEffectClassification: attribute.CapturedBindings.IsDefaultOrEmpty
                        ? ClassifySideEffects(attribute.Value)
                        : RazorVueSideEffectClassification.SingleEvaluationRequired,
                    EventModifiers: RazorVueCanonicalEventModifiers.Empty,
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
                        : attribute.CapturedBindings.IsDefaultOrEmpty
                            ? EmitTemplateExpression(snapshot, expressionEmitter, attribute.Value, allowedLocalSymbols, allowedParameterSymbols)
                            : expressionEmitter.EmitCapturedTemplateExpression(attribute.Value, attribute.CapturedBindings, allowedLocalSymbols, allowedParameterSymbols),
                    LiteralValueKind: ClassifyLiteralValueKind(attribute.Value),
                    AttributeKind: RazorVueCanonicalAttributeKind.ComponentFallthroughAttribute,
                    BindingKind: attribute.CapturedBindings.IsDefaultOrEmpty
                        ? ClassifyBindingKind(snapshot, attribute.Value)
                        : RazorVueExpressionBindingKind.RuntimeExpression,
                    TemplateEncodability: attribute.CapturedBindings.IsDefaultOrEmpty
                        ? ClassifyTemplateEncodability(attribute.Value)
                        : RazorVueTemplateEncodability.TemplateViaSetupBinding,
                    TemplateExpressionSafety: attribute.CapturedBindings.IsDefaultOrEmpty
                        ? ClassifyTemplateExpressionSafety(snapshot, attribute.Value)
                        : RazorVueTemplateExpressionSafety.RequiresSetupBinding,
                    SideEffectClassification: attribute.CapturedBindings.IsDefaultOrEmpty
                        ? ClassifySideEffects(attribute.Value)
                        : RazorVueSideEffectClassification.SingleEvaluationRequired,
                    EventModifiers: RazorVueCanonicalEventModifiers.Empty,
                    SourceOrigins: attribute.Origins));
                continue;
            }

            throw CreateUnknownComponentAttributeException(snapshot, descriptor, attribute);
        }

        return builder.ToImmutable();
    }

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

    private static RazorVueCanonicalEventModifiers CreateCanonicalEventModifiers(
        RazorVueSemanticSnapshot snapshot,
        RazorVueExpressionEmitter expressionEmitter,
        RazorVueEventModifiers modifiers,
        ImmutableHashSet<ILocalSymbol> allowedLocalSymbols,
        ImmutableHashSet<IParameterSymbol> allowedParameterSymbols)
    {
        if (!modifiers.HasAny)
            return RazorVueCanonicalEventModifiers.Empty;

        return new RazorVueCanonicalEventModifiers(
            PreventDefaultExpressionText: EmitEventModifierExpression(
                snapshot,
                expressionEmitter,
                modifiers.PreventDefault,
                allowedLocalSymbols,
                allowedParameterSymbols),
            StopPropagationExpressionText: EmitEventModifierExpression(
                snapshot,
                expressionEmitter,
                modifiers.StopPropagation,
                allowedLocalSymbols,
                allowedParameterSymbols),
            TemplateEncodability: CombineTemplateEncodability(
                ClassifyEventModifierTemplateEncodability(modifiers.PreventDefault),
                ClassifyEventModifierTemplateEncodability(modifiers.StopPropagation)),
            TemplateExpressionSafety: CombineTemplateExpressionSafety(
                ClassifyEventModifierTemplateExpressionSafety(snapshot, modifiers.PreventDefault),
                ClassifyEventModifierTemplateExpressionSafety(snapshot, modifiers.StopPropagation)),
            SideEffectClassification: CombineSideEffectClassifications(
                ClassifyEventModifierSideEffects(modifiers.PreventDefault),
                ClassifyEventModifierSideEffects(modifiers.StopPropagation)));
    }

    private static string? EmitEventModifierExpression(
        RazorVueSemanticSnapshot snapshot,
        RazorVueExpressionEmitter expressionEmitter,
        RazorVueEventModifierBinding? binding,
        ImmutableHashSet<ILocalSymbol> allowedLocalSymbols,
        ImmutableHashSet<IParameterSymbol> allowedParameterSymbols)
    {
        if (binding is null)
            return null;

        return binding.CapturedBindings.IsDefaultOrEmpty
            ? EmitTemplateExpression(snapshot, expressionEmitter, binding.Value, allowedLocalSymbols, allowedParameterSymbols)
            : expressionEmitter.EmitCapturedTemplateExpression(binding.Value, binding.CapturedBindings, allowedLocalSymbols, allowedParameterSymbols);
    }

    private static RazorVueTemplateEncodability ClassifyEventModifierTemplateEncodability(RazorVueEventModifierBinding? binding)
        => binding is null
            ? RazorVueTemplateEncodability.DirectTemplate
            : binding.CapturedBindings.IsDefaultOrEmpty
                ? ClassifyTemplateEncodability(binding.Value)
                : RazorVueTemplateEncodability.TemplateViaSetupBinding;

    private static RazorVueTemplateExpressionSafety ClassifyEventModifierTemplateExpressionSafety(
        RazorVueSemanticSnapshot snapshot,
        RazorVueEventModifierBinding? binding)
        => binding is null
            ? RazorVueTemplateExpressionSafety.DirectTemplateSafe
            : binding.CapturedBindings.IsDefaultOrEmpty
                ? ClassifyTemplateExpressionSafety(snapshot, binding.Value)
                : RazorVueTemplateExpressionSafety.RequiresSetupBinding;

    private static RazorVueSideEffectClassification ClassifyEventModifierSideEffects(RazorVueEventModifierBinding? binding)
        => binding is null
            ? RazorVueSideEffectClassification.None
            : binding.CapturedBindings.IsDefaultOrEmpty
                ? ClassifySideEffects(binding.Value)
                : RazorVueSideEffectClassification.SingleEvaluationRequired;

    private static void ValidateInvalidBindTargets(
        RazorVueSemanticSnapshot snapshot,
        VueComponentDescriptor descriptor,
        RazorVueComponentNode component,
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

            var hasBindableProp = VuePropResolver.TryResolve(descriptor.Props, parameterName, out var prop) &&
                                  prop.Descriptor.AcceptsBinding;
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
        ImmutableDictionary<string, VueEmitDescriptor> emitsByAlias)
    {
        var mappedAttributes = new Dictionary<string, RazorVueAttributeNode>(StringComparer.Ordinal);
        foreach (var attributeEntry in component.Attributes)
        {
            if (attributeEntry is not RazorVueAttributeNode attribute)
                continue;

            if (VuePropResolver.TryResolve(descriptor.Props, attribute.Name, out var prop))
            {
                ValidateUniqueMappedAttribute(
                    snapshot,
                    descriptor,
                    mappedAttributes,
                    "prop:" + prop.PropName,
                    "Vue prop",
                    prop.PropName,
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
                TemplateExpressionSafety: ClassifySlotTemplateExpressionSafety(snapshot, attribute.Value, valueKind),
                SideEffectClassification: ClassifySlotSideEffects(attribute.Value, valueKind),
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
                        new RazorVueAttributeNode(
                            slotTemplate.PublicName,
                            null,
                            ImmutableArray<RazorVueCapturedValueBinding>.Empty,
                            slotTemplate.Origins));
                }
            }
            else if (string.IsNullOrWhiteSpace(slotTemplate.ParameterName))
            {
                throw CreateSlotContextMisuseException(
                    snapshot,
                    slotDescriptor,
                    new RazorVueAttributeNode(
                        slotTemplate.PublicName,
                        null,
                        ImmutableArray<RazorVueCapturedValueBinding>.Empty,
                        slotTemplate.Origins));
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
                TemplateExpressionSafety: RazorVueTemplateExpressionSafety.DirectTemplateSafe,
                SideEffectClassification: RazorVueSideEffectClassification.None,
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

    private static RazorVueTemplateExpressionSafety ClassifySlotTemplateExpressionSafety(
        RazorVueSemanticSnapshot snapshot,
        IOperation? operation,
        RazorVueCanonicalSlotValueKind valueKind)
        => valueKind == RazorVueCanonicalSlotValueKind.ForwardedSlot
            ? RazorVueTemplateExpressionSafety.DirectTemplateSafe
            : ClassifyTemplateExpressionSafety(snapshot, operation);

    private static RazorVueSideEffectClassification ClassifySlotSideEffects(
        IOperation? operation,
        RazorVueCanonicalSlotValueKind valueKind)
        => valueKind == RazorVueCanonicalSlotValueKind.ForwardedSlot
            ? RazorVueSideEffectClassification.None
            : ClassifySideEffects(operation);

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

    private static RazorVueCompilationIssueException CreateUnsupportedNestedImperativeRenderException(
        RazorVueSemanticSnapshot snapshot,
        RazorVueImperativeBlockNode imperative)
    {
        var issue = new RazorVueCompilationIssue(
            RazorVueIssueCode.CanonicalizationFailed,
            RazorVueIssueSeverity.Error,
            $"RazorVue canonical template lowering does not support nested imperative render block '{imperative.Kind}' inside template canonicalization for component '{snapshot.Descriptor.FullName}'. Promote the enclosing body to one imperative root program instead.",
            ImmutableArray<string>.Empty);
        var origin = imperative.Origins.IsDefaultOrEmpty ? snapshot.Origins.FirstOrDefault() : imperative.Origins[0];
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

        if (TryGetStableTemplateScalarLiteral(current, out _))
            return RazorVueExpressionBindingKind.Literal;

        return current switch
        {
            ILiteralOperation => RazorVueExpressionBindingKind.Literal,
            IDefaultValueOperation => RazorVueExpressionBindingKind.Literal,
            IParameterReferenceOperation => RazorVueExpressionBindingKind.LocalReference,
            ILocalReferenceOperation => RazorVueExpressionBindingKind.LocalReference,
            IPropertyReferenceOperation property when IsCurrentVuePropProperty(snapshot, property) => RazorVueExpressionBindingKind.PropsReference,
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

        if (TryGetStableTemplateScalarLiteral(current, out var literal))
            return ClassifyStableTemplateScalarLiteralKind(literal);

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

        if (TryGetStableTemplateScalarLiteral(current, out _))
            return RazorVueTemplateEncodability.DirectTemplate;

        return current switch
        {
            ILiteralOperation => RazorVueTemplateEncodability.DirectTemplate,
            IDefaultValueOperation => RazorVueTemplateEncodability.DirectTemplate,
            INameOfOperation => RazorVueTemplateEncodability.DirectTemplate,
            IPropertyReferenceOperation => RazorVueTemplateEncodability.DirectTemplate,
            IFieldReferenceOperation => RazorVueTemplateEncodability.TemplateViaSetupBinding,
            ILocalReferenceOperation => RazorVueTemplateEncodability.TemplateViaSetupBinding,
            IParameterReferenceOperation => RazorVueTemplateEncodability.TemplateViaSetupBinding,
            IArrayCreationOperation => RazorVueTemplateEncodability.TemplateViaSetupBinding,
            IBinaryOperation => RazorVueTemplateEncodability.TemplateViaSetupBinding,
            IUnaryOperation => RazorVueTemplateEncodability.TemplateViaSetupBinding,
            IInvocationOperation => RazorVueTemplateEncodability.TemplateViaSetupBinding,
            IInterpolatedStringOperation => RazorVueTemplateEncodability.TemplateViaSetupBinding,
            IConditionalOperation => RazorVueTemplateEncodability.TemplateViaSetupBinding,
            _ => RazorVueTemplateEncodability.NotTemplateEncodable
        };
    }

    private static RazorVueTemplateEncodability CombineTemplateEncodability(
        params RazorVueTemplateEncodability[] classifications)
    {
        if (classifications.Any(static item => item == RazorVueTemplateEncodability.NotTemplateEncodable))
            return RazorVueTemplateEncodability.NotTemplateEncodable;

        if (classifications.Any(static item => item == RazorVueTemplateEncodability.TemplateViaSetupBinding))
            return RazorVueTemplateEncodability.TemplateViaSetupBinding;

        return RazorVueTemplateEncodability.DirectTemplate;
    }

    private static RazorVueTemplateExpressionSafety ClassifyTemplateExpressionSafety(
        RazorVueSemanticSnapshot snapshot,
        IOperation? operation)
    {
        var current = Unwrap(operation);
        if (current is null)
            return RazorVueTemplateExpressionSafety.DirectTemplateSafe;

        if (TryGetStableTemplateScalarLiteral(current, out _))
            return RazorVueTemplateExpressionSafety.DirectTemplateSafe;

        return current switch
        {
            ILiteralOperation => RazorVueTemplateExpressionSafety.DirectTemplateSafe,
            IDefaultValueOperation => RazorVueTemplateExpressionSafety.DirectTemplateSafe,
            INameOfOperation => RazorVueTemplateExpressionSafety.DirectTemplateSafe,
            IPropertyReferenceOperation property => ClassifyPropertyTemplateExpressionSafety(snapshot, property),
            IBinaryOperation binary => CombineTemplateExpressionSafety(
                ClassifyTemplateExpressionSafety(snapshot, binary.LeftOperand),
                ClassifyTemplateExpressionSafety(snapshot, binary.RightOperand)),
            IUnaryOperation unary => ClassifyTemplateExpressionSafety(snapshot, unary.Operand),
            IConditionalOperation conditional => conditional.WhenTrue is null || conditional.WhenFalse is null
                ? RazorVueTemplateExpressionSafety.NotTemplateSafe
                : CombineTemplateExpressionSafety(
                    ClassifyTemplateExpressionSafety(snapshot, conditional.Condition),
                    ClassifyTemplateExpressionSafety(snapshot, conditional.WhenTrue),
                    ClassifyTemplateExpressionSafety(snapshot, conditional.WhenFalse)),
            IInterpolatedStringOperation interpolated => CombineTemplateExpressionSafety(
                interpolated.Parts
                    .Select(part => ClassifyInterpolatedStringPartTemplateExpressionSafety(snapshot, part))
                    .ToArray()),
            IInterpolationOperation interpolation => interpolation.FormatString is null
                ? ClassifyTemplateExpressionSafety(snapshot, interpolation.Expression)
                : RazorVueTemplateExpressionSafety.NotTemplateSafe,
            IInterpolatedStringTextOperation => RazorVueTemplateExpressionSafety.DirectTemplateSafe,
            ILocalReferenceOperation => RazorVueTemplateExpressionSafety.RequiresSetupBinding,
            IParameterReferenceOperation => RazorVueTemplateExpressionSafety.RequiresSetupBinding,
            IFieldReferenceOperation => RazorVueTemplateExpressionSafety.RequiresSetupBinding,
            IArrayCreationOperation => RazorVueTemplateExpressionSafety.RequiresSetupBinding,
            IInvocationOperation => RazorVueTemplateExpressionSafety.RequiresSetupBinding,
            IDelegateCreationOperation => RazorVueTemplateExpressionSafety.RequiresSetupBinding,
            IAnonymousFunctionOperation => RazorVueTemplateExpressionSafety.RequiresSetupBinding,
            IObjectCreationOperation => RazorVueTemplateExpressionSafety.RequiresSetupBinding,
            IMethodReferenceOperation method when IsCurrentComponentMember(snapshot, method.Method, method.Instance) =>
                RazorVueTemplateExpressionSafety.RequiresSetupBinding,
            _ => RazorVueTemplateExpressionSafety.NotTemplateSafe
        };
    }

    private static RazorVueTemplateExpressionSafety ClassifyInterpolatedStringPartTemplateExpressionSafety(
        RazorVueSemanticSnapshot snapshot,
        IOperation part)
        => part switch
        {
            IInterpolatedStringTextOperation => RazorVueTemplateExpressionSafety.DirectTemplateSafe,
            IInterpolationOperation interpolation => interpolation.FormatString is null
                ? ClassifyTemplateExpressionSafety(snapshot, interpolation.Expression)
                : RazorVueTemplateExpressionSafety.NotTemplateSafe,
            _ => RazorVueTemplateExpressionSafety.NotTemplateSafe
        };

    private static RazorVueTemplateExpressionSafety ClassifyPropertyTemplateExpressionSafety(
        RazorVueSemanticSnapshot snapshot,
        IPropertyReferenceOperation property)
    {
        if (property.Arguments.Length != 0 || property.Property.IsStatic || property.Property.IsIndexer)
            return RazorVueTemplateExpressionSafety.RequiresSetupBinding;

        if (IsCurrentVuePropProperty(snapshot, property))
            return RazorVueTemplateExpressionSafety.DirectTemplateSafe;

        if (IsCurrentComponentMember(snapshot, property.Property, property.Instance))
            return RazorVueTemplateExpressionSafety.RequiresSetupBinding;

        var instance = Unwrap(property.Instance);
        if (instance is null)
            return RazorVueTemplateExpressionSafety.NotTemplateSafe;

        return ClassifyTemplateExpressionSafety(snapshot, instance);
    }

    private static RazorVueSideEffectClassification ClassifySideEffects(IOperation? operation)
    {
        var current = Unwrap(operation);
        if (current is null)
            return RazorVueSideEffectClassification.None;

        if (TryGetStableTemplateScalarLiteral(current, out _))
            return RazorVueSideEffectClassification.None;

        if (ClassifyPureTemplateExpressionSideEffects(current) == RazorVueSideEffectClassification.None)
            return RazorVueSideEffectClassification.None;

        return current switch
        {
            ILiteralOperation => RazorVueSideEffectClassification.None,
            IDefaultValueOperation => RazorVueSideEffectClassification.None,
            IPropertyReferenceOperation property => ClassifyPropertySideEffects(property),
            ILocalReferenceOperation => RazorVueSideEffectClassification.None,
            IParameterReferenceOperation => RazorVueSideEffectClassification.None,
            IFieldReferenceOperation => RazorVueSideEffectClassification.SingleEvaluationRequired,
            IArrayCreationOperation => RazorVueSideEffectClassification.SingleEvaluationRequired,
            IBinaryOperation => RazorVueSideEffectClassification.SingleEvaluationRequired,
            IUnaryOperation => RazorVueSideEffectClassification.SingleEvaluationRequired,
            IConditionalOperation => RazorVueSideEffectClassification.SingleEvaluationRequired,
            IInterpolatedStringOperation => RazorVueSideEffectClassification.SingleEvaluationRequired,
            IInvocationOperation => RazorVueSideEffectClassification.RepeatedEvaluationRisk,
            _ => RazorVueSideEffectClassification.RepeatedEvaluationRisk
        };
    }

    private static RazorVueSideEffectClassification ClassifyPureTemplateExpressionSideEffects(IOperation operation)
    {
        var current = Unwrap(operation);
        if (current is null)
            return RazorVueSideEffectClassification.None;

        if (TryGetStableTemplateScalarLiteral(current, out _))
            return RazorVueSideEffectClassification.None;

        return current switch
        {
            IInstanceReferenceOperation => RazorVueSideEffectClassification.None,
            ILiteralOperation => RazorVueSideEffectClassification.None,
            IDefaultValueOperation => RazorVueSideEffectClassification.None,
            IPropertyReferenceOperation property => ClassifyPurePropertyReferenceSideEffects(property),
            IBinaryOperation binary => CombineSideEffectClassifications(
                ClassifyPureTemplateExpressionSideEffects(binary.LeftOperand),
                ClassifyPureTemplateExpressionSideEffects(binary.RightOperand)),
            IUnaryOperation unary => ClassifyPureTemplateExpressionSideEffects(unary.Operand),
            IConditionalOperation conditional => CombineSideEffectClassifications(
                ClassifyPureTemplateExpressionSideEffects(conditional.Condition),
                conditional.WhenTrue is null ? RazorVueSideEffectClassification.RepeatedEvaluationRisk : ClassifyPureTemplateExpressionSideEffects(conditional.WhenTrue),
                conditional.WhenFalse is null ? RazorVueSideEffectClassification.RepeatedEvaluationRisk : ClassifyPureTemplateExpressionSideEffects(conditional.WhenFalse)),
            IInterpolatedStringOperation interpolated => CombineSideEffectClassifications(
                interpolated.Parts.Select(static part => part switch
                {
                    IInterpolatedStringTextOperation => RazorVueSideEffectClassification.None,
                    IInterpolationOperation interpolation => interpolation.FormatString is null
                        ? ClassifyPureTemplateExpressionSideEffects(interpolation.Expression)
                        : RazorVueSideEffectClassification.RepeatedEvaluationRisk,
                    _ => RazorVueSideEffectClassification.RepeatedEvaluationRisk
                }).ToArray()),
            INameOfOperation => RazorVueSideEffectClassification.None,
            _ => RazorVueSideEffectClassification.RepeatedEvaluationRisk
        };
    }

    private static RazorVueSideEffectClassification ClassifyPurePropertyReferenceSideEffects(IPropertyReferenceOperation property)
    {
        if (property.Arguments.Length != 0 || property.Property.IsStatic || property.Property.IsIndexer)
            return RazorVueSideEffectClassification.RepeatedEvaluationRisk;

        return property.Instance is null
            ? RazorVueSideEffectClassification.None
            : ClassifyPureTemplateExpressionSideEffects(property.Instance);
    }

    private static RazorVueSideEffectClassification ClassifyPropertySideEffects(IPropertyReferenceOperation property)
    {
        if (property.Arguments.Length != 0 || property.Property.IsStatic || property.Property.IsIndexer)
            return RazorVueSideEffectClassification.RepeatedEvaluationRisk;

        return property.Instance is null
            ? RazorVueSideEffectClassification.None
            : ClassifySideEffects(property.Instance);
    }

    private static RazorVueTemplateExpressionSafety CombineTemplateExpressionSafety(
        params RazorVueTemplateExpressionSafety[] classifications)
    {
        if (classifications.Any(static item => item == RazorVueTemplateExpressionSafety.NotTemplateSafe))
            return RazorVueTemplateExpressionSafety.NotTemplateSafe;

        if (classifications.Any(static item => item == RazorVueTemplateExpressionSafety.RequiresSetupBinding))
            return RazorVueTemplateExpressionSafety.RequiresSetupBinding;

        return RazorVueTemplateExpressionSafety.DirectTemplateSafe;
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

    private static RazorVueLiteralValueKind ClassifyStableTemplateScalarLiteralKind(object? literal)
        => literal switch
        {
            null => RazorVueLiteralValueKind.Null,
            string => RazorVueLiteralValueKind.String,
            char => RazorVueLiteralValueKind.String,
            bool => RazorVueLiteralValueKind.Boolean,
            sbyte or byte or short or ushort or int or uint or long or ulong or float or double or decimal => RazorVueLiteralValueKind.Number,
            _ => RazorVueLiteralValueKind.Other
        };

    private static bool TryGetStableTemplateScalarLiteral(IOperation operation, out object? literal)
    {
        literal = null;
        var current = Unwrap(operation);
        if (current is null)
            return false;

        if (current is ILiteralOperation literalOperation && literalOperation.ConstantValue.HasValue)
        {
            literal = literalOperation.ConstantValue.Value;
            return true;
        }

        if (current is IDefaultValueOperation defaultValue && IsNullDefaultValue(defaultValue))
        {
            literal = null;
            return true;
        }

        if (current is not IFieldReferenceOperation fieldReference)
            return false;

        var field = fieldReference.Field;
        if (!field.HasConstantValue)
            return false;

        if (field.ContainingType?.TypeKind == TypeKind.Enum &&
            Util.IsStringEnumType(field.ContainingType))
        {
            if (!TryGetStringEnumLiteralText(field, out var enumLiteral))
                return false;

            literal = enumLiteral;
            return true;
        }

        literal = field.ConstantValue;
        return ClassifyStableTemplateScalarLiteralKind(literal) is not RazorVueLiteralValueKind.Other;
    }

    private static bool TryGetStringEnumLiteralText(IFieldSymbol symbol, out string? literalText)
    {
        literalText = null;
        if (symbol.ContainingType?.TypeKind != TypeKind.Enum ||
            !Util.IsStringEnumType(symbol.ContainingType))
        {
            return false;
        }

        foreach (var attribute in symbol.GetAttributes())
        {
            if (attribute.ConstructorArguments.Length == 0)
                continue;

            if (attribute.AttributeClass?.Name == "ECMAScriptNameAttribute")
            {
                literalText = attribute.ConstructorArguments[0].Value?.ToString() ?? string.Empty;
                return true;
            }

            if (attribute.AttributeClass?.Name != "DescriptionAttribute")
                continue;

            var description = attribute.ConstructorArguments[0].Value?.ToString()?.Trim();
            if (description?.StartsWith("@#", StringComparison.Ordinal) != true)
                continue;

            literalText = description.Substring(2);
            return true;
        }

        literalText = Util.GetSymbolConfigName(symbol) ?? symbol.Name;
        return true;
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

    private static bool IsCurrentVuePropProperty(RazorVueSemanticSnapshot snapshot, IPropertyReferenceOperation property)
        => IsCurrentParameterProperty(snapshot, property) &&
           snapshot.Descriptor.Props.Any(prop => string.Equals(prop.PublicName, property.Property.Name, StringComparison.Ordinal));

    private static bool IsCurrentComponentMember(
        RazorVueSemanticSnapshot snapshot,
        ISymbol symbol,
        IOperation? instance)
        => RazorVueSymbolIdentity.IsCurrentComponentMember(
            snapshot.ComponentSymbol,
            symbol,
            instance,
            Unwrap);

    private static bool IsRenderFragmentLike(IOperation operation)
        => RazorVueRenderFragmentTypeHelper.IsRenderFragmentType(Unwrap(operation)?.Type);

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
