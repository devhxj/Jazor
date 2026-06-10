using Jazor.RazorVue.Artifacts;
using Jazor.RazorVue.Descriptor;
using Jazor.RazorVue;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;
using System.Collections.Immutable;

namespace Jazor.RazorVue.RenderTree;

/// <summary>
/// Extracts a framework-agnostic RazorVue render tree from BuildRenderTree operations.
/// </summary>
internal sealed class RazorVueRenderTreeExtractor
{
    internal sealed record ParsedTemplateCarrier(
        string? ParameterName,
        IParameterSymbol? ParameterSymbol,
        RazorVueRenderFragment Children,
        ImmutableArray<CapturedValueBinding> CapturedBindings)
    {
        public static ParsedTemplateCarrier Create(
            string? parameterName,
            IParameterSymbol? parameterSymbol,
            RazorVueRenderFragment children)
            => new(
                parameterName,
                parameterSymbol,
                children,
                ImmutableArray<CapturedValueBinding>.Empty);
    }

    internal readonly record struct CapturedValueBinding(
        IParameterSymbol ParameterSymbol,
        IOperation Initializer);

    /// <summary>
    /// Converts BuildRenderTree syntax/operations into a <see cref="RazorVueRenderFragment"/>.
    /// </summary>
    public RazorVueRenderFragment Extract(RazorVueCompilationContext context, RazorVueSemanticSnapshot snapshot)
    {
        if (context is null)
            throw new ArgumentNullException(nameof(context));
        if (snapshot is null)
            throw new ArgumentNullException(nameof(snapshot));

        var method = snapshot.BuildRenderTreeMethod;
        if (method is null)
        {
            if (RequiresGeneratedRazorBaseline(snapshot))
                throw CreateMissingBuildRenderTreeIssue(snapshot, "did not resolve a BuildRenderTree method");

            return RazorVueRenderFragment.Empty;
        }

        var builderParameters = method.Parameters
            .Where(static parameter => string.Equals(parameter.Name, "builder", StringComparison.Ordinal) ||
                                       string.Equals(parameter.Type.Name, "RenderTreeBuilder", StringComparison.Ordinal))
            .ToImmutableHashSet<IParameterSymbol>(SymbolEqualityComparer.Default);

        foreach (var reference in method.DeclaringSyntaxReferences)
        {
            if (reference.GetSyntax() is not MethodDeclarationSyntax methodSyntax)
                continue;

            var model = context.Compilation.GetSemanticModel(methodSyntax.SyntaxTree);
            var operation = methodSyntax.Body is not null
                ? model.GetOperation(methodSyntax.Body)
                : methodSyntax.ExpressionBody is not null
                    ? model.GetOperation(methodSyntax.ExpressionBody.Expression)
                    : null;

            if (operation is IBlockOperation block)
            {
                if (RazorVueImperativeRenderSegmentationPlanner.TryPlanLocalSegments(block.Operations, out var segments))
                {
                    return new Parser(snapshot, context.Compilation, context.Symbols, builderParameters, allowTemplateScopedLocals: true)
                        .ParseWithImperativeSegments(block.Operations, segments);
                }

                if (RazorVueImperativeRenderPromotionAnalyzer.ShouldPromoteBody(block.Operations))
                {
                    return CreateImperativeBodyFragment(
                        block.Operations,
                        RazorVueImperativeRenderPromotionAnalyzer.ClassifyBodyKind(block.Operations),
                        builderParameters);
                }

                return new Parser(snapshot, context.Compilation, context.Symbols, builderParameters, allowTemplateScopedLocals: true).Parse(block.Operations);
            }

            if (operation is not null)
            {
                if (RazorVueImperativeRenderPromotionAnalyzer.ShouldPromoteBody([operation]))
                {
                    return CreateImperativeBodyFragment(
                        [operation],
                        RazorVueImperativeRenderPromotionAnalyzer.ClassifyBodyKind([operation]),
                        builderParameters);
                }

                return new Parser(snapshot, context.Compilation, context.Symbols, builderParameters, allowTemplateScopedLocals: true).Parse([operation]);
            }
        }

        throw CreateMissingBuildRenderTreeIssue(snapshot, "resolved BuildRenderTree but could not bind an analyzable method body");
    }

    private static RazorVueCompilationIssueException CreateMissingBuildRenderTreeIssue(
        RazorVueSemanticSnapshot snapshot,
        string reason)
    {
        var routeHint = snapshot.RazorSourceGeneratorDocument is not null
            ? "Razor SG tail input must include the official generated C# containing BuildRenderTree for this .razor component."
            : snapshot.BuildRenderTreeMethod is not null
                ? "BuildRenderTree must have a source-authored or generated C# body that Roslyn can bind."
                : "For .razor components, run RazorVue after Razor SG tail output has provided official generated C#; Razor IR may enhance the SFC but cannot replace the Roslyn/BuildRenderTree semantic baseline. Handwritten component authoring must provide a BuildRenderTree body.";
        var issue = new RazorVueCompilationIssue(
            RazorVueIssueCode.CanonicalizationFailed,
            RazorVueIssueSeverity.Error,
            $"RazorVue could not create a render semantic baseline for component '{snapshot.Descriptor.FullName}': {reason}. {routeHint}",
            ImmutableArray<string>.Empty);
        return new RazorVueCompilationIssueException(
            issue,
            snapshot.Descriptor.FullName,
            snapshot.Origins.FirstOrDefault());
    }

    private static bool RequiresGeneratedRazorBaseline(RazorVueSemanticSnapshot snapshot)
    {
        if (snapshot.RazorSourceGeneratorDocument is not null)
            return true;

        if (snapshot.RazorIrCarrier is not null)
            return false;

        foreach (var syntaxReference in snapshot.ComponentSymbol.DeclaringSyntaxReferences)
        {
            if (syntaxReference.GetSyntax() is not ClassDeclarationSyntax classDeclaration ||
                !classDeclaration.Modifiers.Any(SyntaxKind.PartialKeyword))
            {
                continue;
            }

            var path = classDeclaration.SyntaxTree.FilePath;
            if (path is not null &&
                path.EndsWith(".razor.cs", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }

    internal static bool TryParseTemplateCarrier(
        RazorVueCompilationContext context,
        RazorVueSemanticSnapshot snapshot,
        IOperation initializer,
        IEnumerable<KeyValuePair<ILocalSymbol, ParsedTemplateCarrier>>? localRenderFragmentCarriers,
        IEnumerable<KeyValuePair<ISymbol, ParsedTemplateCarrier>>? memberRenderFragmentCarriers,
        IEnumerable<KeyValuePair<IMethodSymbol, ParsedTemplateCarrier>>? factoryRenderFragmentCarriers,
        IEnumerable<ISymbol>? activeRenderFragmentMembers,
        IEnumerable<IMethodSymbol>? activeRenderFragmentFactories,
        IEnumerable<ILocalSymbol>? accessibleTemplateLocals,
        IEnumerable<IParameterSymbol>? accessibleTemplateParameters,
        out ParsedTemplateCarrier slotTemplate)
    {
        var adapter = ParserAdapter.Create(
            context,
            snapshot,
            localRenderFragmentCarriers,
            memberRenderFragmentCarriers,
            factoryRenderFragmentCarriers,
            activeRenderFragmentMembers,
            activeRenderFragmentFactories,
            accessibleTemplateLocals,
            accessibleTemplateParameters);

        return adapter.TryParseTemplateCarrier(initializer, out slotTemplate);
    }

    private static RazorVueRenderFragment CreateImperativeBodyFragment(
        IReadOnlyList<IOperation> operations,
        RazorVueImperativeBlockKind kind,
        ImmutableHashSet<IParameterSymbol> builderParameters)
    {
        var visibleLocals = CollectVisibleLocals(operations);
        var visibleParameters = CollectVisibleParameters(operations, builderParameters);

        return new RazorVueRenderFragment(
        [
            new RazorVueImperativeBlockNode(
                [.. operations],
                kind,
                visibleLocals,
                visibleParameters,
                CreateOriginsStatic(operations, RazorVueOriginKind.Template))
        ]);
    }

    private static ImmutableArray<ILocalSymbol> CollectVisibleLocals(IEnumerable<IOperation> operations)
    {
        var builder = ImmutableArray.CreateBuilder<ILocalSymbol>();
        var seen = new HashSet<ILocalSymbol>(SymbolEqualityComparer.Default);

        foreach (var operation in operations)
        {
            foreach (var candidate in EnumerateOperationAndDescendants(operation))
            {
                switch (candidate)
                {
                    case IVariableDeclarationGroupOperation declarationGroup:
                        foreach (var declaration in declarationGroup.Declarations)
                        {
                            foreach (var declarator in declaration.Declarators)
                            {
                                if (seen.Add(declarator.Symbol))
                                    builder.Add(declarator.Symbol);
                            }
                        }

                        break;
                    case IForEachLoopOperation foreachLoop:
                        foreach (var local in foreachLoop.Locals)
                        {
                            if (seen.Add(local))
                                builder.Add(local);
                        }

                        break;
                    case IForLoopOperation forLoop:
                        foreach (var local in forLoop.Locals)
                        {
                            if (seen.Add(local))
                                builder.Add(local);
                        }

                        break;
                    case IUsingDeclarationOperation usingDeclaration:
                        if (usingDeclaration.DeclarationGroup is null)
                            break;

                        foreach (var declaration in usingDeclaration.DeclarationGroup.Declarations)
                        {
                            foreach (var declarator in declaration.Declarators)
                            {
                                if (seen.Add(declarator.Symbol))
                                    builder.Add(declarator.Symbol);
                            }
                        }

                        break;
                    case ILocalReferenceOperation localReference:
                        if (seen.Add(localReference.Local))
                            builder.Add(localReference.Local);

                        break;
                }
            }
        }

        return builder.ToImmutable();
    }

    private static IEnumerable<IOperation> EnumerateOperationAndDescendants(IOperation operation)
    {
        yield return operation;
        foreach (var child in operation.ChildOperations)
        {
            if (child is null)
                continue;

            foreach (var nested in EnumerateOperationAndDescendants(child))
                yield return nested;
        }
    }

    private static ImmutableArray<IParameterSymbol> CollectVisibleParameters(
        IEnumerable<IOperation> operations,
        ImmutableHashSet<IParameterSymbol> fallbackParameters)
    {
        var builder = ImmutableArray.CreateBuilder<IParameterSymbol>();
        var seen = new HashSet<IParameterSymbol>(SymbolEqualityComparer.Default);

        foreach (var operation in operations)
        {
            foreach (var parameterReference in EnumerateOperationAndDescendants(operation).OfType<IParameterReferenceOperation>())
            {
                if (seen.Add(parameterReference.Parameter))
                    builder.Add(parameterReference.Parameter);
            }
        }

        if (builder.Count > 0)
            return builder.ToImmutable();

        return fallbackParameters.ToImmutableArray();
    }

    private static ImmutableArray<RazorVueSourceOrigin> CreateOriginsStatic(
        IEnumerable<IOperation> operations,
        RazorVueOriginKind originKind)
    {
        var builder = ImmutableArray.CreateBuilder<RazorVueSourceOrigin>();
        foreach (var operation in operations)
        {
            if (operation.Syntax is null)
                continue;

            builder.Add(RazorVueSourceOrigin.FromLocation(operation.Syntax.GetLocation(), originKind));
        }

        return builder.ToImmutable();
    }

    private readonly record struct ParsedSlotTemplate(
        string? ParameterName,
        IParameterSymbol? ParameterSymbol,
        RazorVueRenderFragment Children,
        ImmutableArray<RenderHelperValueBinding> CapturedBindings)
    {
        public static ParsedSlotTemplate Create(
            string? parameterName,
            IParameterSymbol? parameterSymbol,
            RazorVueRenderFragment children)
            => new(
                parameterName,
                parameterSymbol,
                children,
                ImmutableArray<RenderHelperValueBinding>.Empty);

        public ParsedSlotTemplate PrependCapturedBindings(ImmutableArray<RenderHelperValueBinding> capturedBindings)
        {
            if (capturedBindings.IsDefaultOrEmpty)
                return this;

            if (CapturedBindings.IsDefaultOrEmpty)
                return new ParsedSlotTemplate(ParameterName, ParameterSymbol, Children, capturedBindings);

            var builder = ImmutableArray.CreateBuilder<RenderHelperValueBinding>(capturedBindings.Length + CapturedBindings.Length);
            builder.AddRange(capturedBindings);
            builder.AddRange(CapturedBindings);
            return new ParsedSlotTemplate(ParameterName, ParameterSymbol, Children, builder.MoveToImmutable());
        }
    }

    private sealed class ParserAdapter(Parser parser)
    {
        public static ParserAdapter Create(
            RazorVueCompilationContext context,
            RazorVueSemanticSnapshot snapshot,
            IEnumerable<KeyValuePair<ILocalSymbol, ParsedTemplateCarrier>>? localRenderFragmentCarriers,
            IEnumerable<KeyValuePair<ISymbol, ParsedTemplateCarrier>>? memberRenderFragmentCarriers,
            IEnumerable<KeyValuePair<IMethodSymbol, ParsedTemplateCarrier>>? factoryRenderFragmentCarriers,
            IEnumerable<ISymbol>? activeRenderFragmentMembers,
            IEnumerable<IMethodSymbol>? activeRenderFragmentFactories,
            IEnumerable<ILocalSymbol>? accessibleTemplateLocals,
            IEnumerable<IParameterSymbol>? accessibleTemplateParameters)
        {
            var localDictionary = CreateLocalRenderFragmentCarrierDictionary(localRenderFragmentCarriers);
            var memberDictionary = CreateMemberRenderFragmentCarrierDictionary(memberRenderFragmentCarriers);
            var factoryDictionary = CreateFactoryRenderFragmentCarrierDictionary(factoryRenderFragmentCarriers);

            return new ParserAdapter(
                new Parser(
                    snapshot,
                    context.Compilation,
                    context.Symbols,
                    ImmutableHashSet<IParameterSymbol>.Empty.WithComparer(SymbolEqualityComparer.Default),
                    activeRenderFragmentMembers: activeRenderFragmentMembers,
                    activeRenderFragmentFactories: activeRenderFragmentFactories,
                    localRenderFragmentCarriers: localDictionary.Select(static pair => new RenderFragmentLocalCarrier(pair.Key, pair.Value)),
                    memberRenderFragmentCarriers: memberDictionary.Select(static pair => new RenderFragmentMemberCarrier(pair.Key, pair.Value)),
                    factoryRenderFragmentCarriers: factoryDictionary.Select(static pair => new RenderFragmentFactoryCarrier(pair.Key, pair.Value)),
                    accessibleTemplateLocals: accessibleTemplateLocals,
                    accessibleTemplateParameters: accessibleTemplateParameters,
                    allowTemplateScopedLocals: true));
        }

        public bool TryParseTemplateCarrier(IOperation initializer, out ParsedTemplateCarrier slotTemplate)
        {
            if (!parser.TryParseSlotTemplateForExternalConsumption(initializer, out var internalTemplate))
            {
                slotTemplate = default!;
                return false;
            }

            slotTemplate = ToExternal(internalTemplate);
            return true;
        }

        private ParsedTemplateCarrier ToExternal(ParsedSlotTemplate template)
            => new(
                template.ParameterName,
                template.ParameterSymbol,
                template.Children,
                template.CapturedBindings
                    .Select(static binding => new CapturedValueBinding(binding.ParameterSymbol, binding.Initializer))
                    .ToImmutableArray());

        private static Dictionary<ILocalSymbol, ParsedSlotTemplate> CreateLocalRenderFragmentCarrierDictionary(
            IEnumerable<KeyValuePair<ILocalSymbol, ParsedTemplateCarrier>>? carriers)
        {
            var dictionary = new Dictionary<ILocalSymbol, ParsedSlotTemplate>(SymbolEqualityComparer.Default);
            if (carriers is null)
                return dictionary;

            foreach (var carrier in carriers)
                dictionary[carrier.Key] = ToInternal(carrier.Value);

            return dictionary;
        }

        private static Dictionary<ISymbol, ParsedSlotTemplate> CreateMemberRenderFragmentCarrierDictionary(
            IEnumerable<KeyValuePair<ISymbol, ParsedTemplateCarrier>>? carriers)
        {
            var dictionary = new Dictionary<ISymbol, ParsedSlotTemplate>(SymbolEqualityComparer.Default);
            if (carriers is null)
                return dictionary;

            foreach (var carrier in carriers)
                dictionary[carrier.Key] = ToInternal(carrier.Value);

            return dictionary;
        }

        private static Dictionary<IMethodSymbol, ParsedSlotTemplate> CreateFactoryRenderFragmentCarrierDictionary(
            IEnumerable<KeyValuePair<IMethodSymbol, ParsedTemplateCarrier>>? carriers)
        {
            var dictionary = new Dictionary<IMethodSymbol, ParsedSlotTemplate>(SymbolEqualityComparer.Default);
            if (carriers is null)
                return dictionary;

            foreach (var carrier in carriers)
                dictionary[carrier.Key] = ToInternal(carrier.Value);

            return dictionary;
        }

        private static ParsedSlotTemplate ToInternal(ParsedTemplateCarrier template)
            => new(
                template.ParameterName,
                template.ParameterSymbol,
                template.Children,
                template.CapturedBindings
                    .Select(static binding => new RenderHelperValueBinding(binding.ParameterSymbol, binding.Initializer))
                    .ToImmutableArray());
    }

    private readonly record struct RenderFragmentLocalCarrier(
        ILocalSymbol LocalSymbol,
        ParsedSlotTemplate Template);

    private readonly record struct RenderFragmentMemberCarrier(
        ISymbol MemberSymbol,
        ParsedSlotTemplate Template);

    private readonly record struct RenderFragmentFactoryCarrier(
        IMethodSymbol MethodSymbol,
        ParsedSlotTemplate Template);

    private readonly record struct LocalFunctionDeclarationCarrier(
        IMethodSymbol MethodSymbol,
        ILocalFunctionOperation Declaration);

    private readonly record struct CallerOwnedReturnBranch(
        bool Returns,
        ImmutableArray<IOperation> BeforeReturn)
    {
        public static CallerOwnedReturnBranch DoesNotReturn { get; } =
            new(false, ImmutableArray<IOperation>.Empty);
    }

    private readonly record struct RenderHelperValueBinding(
        IParameterSymbol ParameterSymbol,
        IOperation Initializer);

    internal static RazorVueRenderFragment ParseSupportedOperations(
        RazorVueCompilationContext context,
        RazorVueSemanticSnapshot snapshot,
        IEnumerable<IOperation> operations,
        IEnumerable<ILocalSymbol>? accessibleTemplateLocals = null,
        IEnumerable<IParameterSymbol>? accessibleTemplateParameters = null)
    {
        if (context is null)
            throw new ArgumentNullException(nameof(context));
        if (snapshot is null)
            throw new ArgumentNullException(nameof(snapshot));
        if (operations is null)
            throw new ArgumentNullException(nameof(operations));

        var templateParameters = accessibleTemplateParameters?.ToImmutableArray() ?? ImmutableArray<IParameterSymbol>.Empty;
        var builderParameters = templateParameters
            .Where(static parameter => IsRenderTreeBuilderParameterType(parameter.Type))
            .ToImmutableHashSet<IParameterSymbol>(SymbolEqualityComparer.Default);
        var nonBuilderTemplateParameters = templateParameters
            .Where(static parameter => !IsRenderTreeBuilderParameterType(parameter.Type))
            .ToImmutableArray();
        return new Parser(
            snapshot,
            context.Compilation,
            context.Symbols,
            builderParameters,
            accessibleTemplateLocals: accessibleTemplateLocals,
            accessibleTemplateParameters: nonBuilderTemplateParameters,
            allowTemplateScopedLocals: true).Parse(operations);
    }

    private static bool IsRenderTreeBuilderParameterType(ITypeSymbol? typeSymbol)
        => string.Equals(
            typeSymbol?.ToDisplayString(),
            "Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder",
            StringComparison.Ordinal);

    private sealed class Parser(
        RazorVueSemanticSnapshot snapshot,
        Compilation compilation,
        RazorVueCompilationSymbols symbols,
        ImmutableHashSet<IParameterSymbol> builderParameters,
        IEnumerable<ILocalSymbol>? builderAliases = null,
        IEnumerable<IMethodSymbol>? activeRenderHelperMethods = null,
        IEnumerable<ISymbol>? activeRenderFragmentMembers = null,
        IEnumerable<IMethodSymbol>? activeRenderFragmentFactories = null,
        IEnumerable<RenderFragmentLocalCarrier>? localRenderFragmentCarriers = null,
        IEnumerable<RenderFragmentMemberCarrier>? memberRenderFragmentCarriers = null,
        IEnumerable<RenderFragmentFactoryCarrier>? factoryRenderFragmentCarriers = null,
        IEnumerable<LocalFunctionDeclarationCarrier>? localFunctionDeclarations = null,
        IEnumerable<ILocalSymbol>? accessibleTemplateLocals = null,
        IEnumerable<IParameterSymbol>? accessibleTemplateParameters = null,
        bool allowTemplateScopedLocals = false)
    {
        private readonly RazorVueSemanticSnapshot _snapshot = snapshot;
        private readonly Compilation _compilation = compilation;
        private readonly RazorVueCompilationSymbols _symbols = symbols;
        private ImmutableHashSet<IParameterSymbol> _builderParameters = builderParameters;
        private readonly HashSet<ILocalSymbol> _builderAliases = builderAliases is null
                ? [with(SymbolEqualityComparer.Default)]
                : new HashSet<ILocalSymbol>(builderAliases, SymbolEqualityComparer.Default);
        private readonly HashSet<IMethodSymbol> _activeRenderHelperMethods = activeRenderHelperMethods is null
                ? [with(SymbolEqualityComparer.Default)]
                : new HashSet<IMethodSymbol>(activeRenderHelperMethods, SymbolEqualityComparer.Default);
        private readonly HashSet<ISymbol> _activeRenderFragmentMembers = activeRenderFragmentMembers is null
                ? [with(SymbolEqualityComparer.Default)]
                : new HashSet<ISymbol>(activeRenderFragmentMembers, SymbolEqualityComparer.Default);
        private readonly HashSet<IMethodSymbol> _activeRenderFragmentFactories = activeRenderFragmentFactories is null
                ? [with(SymbolEqualityComparer.Default)]
                : new HashSet<IMethodSymbol>(activeRenderFragmentFactories, SymbolEqualityComparer.Default);
        private readonly Dictionary<ILocalSymbol, ParsedSlotTemplate> _localRenderFragmentCarriers = localRenderFragmentCarriers is null
                ? new Dictionary<ILocalSymbol, ParsedSlotTemplate>(SymbolEqualityComparer.Default)
                : CreateLocalRenderFragmentCarrierDictionary(localRenderFragmentCarriers);
        private readonly Dictionary<ILocalSymbol, IOperation> _sourceStableLocalRenderFragmentInitializers =
            new(SymbolEqualityComparer.Default);
        private readonly Dictionary<ILocalSymbol, IOperation> _sourceStableLocalStaticMarkupInitializers =
            new(SymbolEqualityComparer.Default);
        private readonly Dictionary<ILocalSymbol, IOperation> _sourceStableLocalComponentTypeInitializers =
            new(SymbolEqualityComparer.Default);
        private readonly Dictionary<ILocalSymbol, IOperation> _localStaticMarkupCarriers =
            new(SymbolEqualityComparer.Default);
        private readonly Dictionary<ISymbol, ParsedSlotTemplate> _memberRenderFragmentCarriers = memberRenderFragmentCarriers is null
                ? new Dictionary<ISymbol, ParsedSlotTemplate>(SymbolEqualityComparer.Default)
                : CreateMemberRenderFragmentCarrierDictionary(memberRenderFragmentCarriers);
        private readonly Dictionary<IMethodSymbol, ParsedSlotTemplate> _factoryRenderFragmentCarriers = factoryRenderFragmentCarriers is null
                ? new Dictionary<IMethodSymbol, ParsedSlotTemplate>(SymbolEqualityComparer.Default)
                : CreateFactoryRenderFragmentCarrierDictionary(factoryRenderFragmentCarriers);
        private readonly HashSet<ILocalSymbol> _accessibleTemplateLocals = accessibleTemplateLocals is null
                ? [with(SymbolEqualityComparer.Default)]
                : new HashSet<ILocalSymbol>(accessibleTemplateLocals, SymbolEqualityComparer.Default);
        private readonly HashSet<IParameterSymbol> _accessibleTemplateParameters = accessibleTemplateParameters is null
                ? [with(SymbolEqualityComparer.Default)]
                : new HashSet<IParameterSymbol>(accessibleTemplateParameters, SymbolEqualityComparer.Default);
        private readonly Dictionary<ILocalSymbol, PendingRenderFragmentLocalCarrierDeclaration> _pendingRenderFragmentLocalCarriers =
            new(SymbolEqualityComparer.Default);
        private readonly Dictionary<ILocalSymbol, PendingStaticMarkupLocalCarrierDeclaration> _pendingStaticMarkupLocalCarriers =
            new(SymbolEqualityComparer.Default);
        private readonly Dictionary<ILocalSymbol, PendingComponentTypeLocalCarrierDeclaration> _pendingComponentTypeLocalCarriers =
            new(SymbolEqualityComparer.Default);
        private readonly Dictionary<ILocalSymbol, PendingTemplateScopedDeclaration> _pendingTemplateScopedDeclarations =
            new(SymbolEqualityComparer.Default);
        private readonly Dictionary<IMethodSymbol, ILocalFunctionOperation> _localFunctionDeclarations = localFunctionDeclarations is null
                ? new Dictionary<IMethodSymbol, ILocalFunctionOperation>(SymbolEqualityComparer.Default)
                : CreateLocalFunctionDeclarationDictionary(localFunctionDeclarations);
        private readonly List<ILocalFunctionOperation> _localFunctionDeclarationOrder = localFunctionDeclarations is null
                ? []
                : [.. localFunctionDeclarations.Select(static carrier => carrier.Declaration)];
        private readonly Dictionary<string, IOperation> _literalStringOperationCache = new(StringComparer.Ordinal);
        private readonly List<RazorVueRenderNode> _rootChildren = [];
        private readonly Stack<OpenFrame> _openFrames = new();
        private ImmutableArray<RenderHelperValueBinding> _activeCapturedBindings = ImmutableArray<RenderHelperValueBinding>.Empty;
        private bool _allowCallerOwnedOpenNodeConditionalReplay;
        private readonly bool _allowTemplateScopedLocals = allowTemplateScopedLocals;

        public RazorVueRenderFragment Parse(IEnumerable<IOperation> operations)
        {
            var operationList = operations as IReadOnlyList<IOperation> ?? operations.ToArray();
            PrimeSourceStableLocalRenderFragmentInitializers(operationList);
            PrimeSourceStableLocalStaticMarkupInitializers(operationList);
            PrimeSourceStableLocalComponentTypeInitializers(operationList);
            PrimeLocalFunctionDeclarations(operationList);
            foreach (var operation in operationList)
                ParseOperation(operation);

            EnsureNoPendingImmediateAssignmentDeclarations();

            if (_openFrames.Count > 0)
                throw CreateStructuralIssueForUnclosedFrames();

            return new RazorVueRenderFragment(_rootChildren.ToImmutableArray());
        }

        public RazorVueRenderFragment ParseWithImperativeSegments(
            IReadOnlyList<IOperation> operations,
            ImmutableArray<RazorVueImperativeRenderSegmentationPlanner.PlannedSegment> segments)
        {
            PrimeSourceStableLocalRenderFragmentInitializers(operations);
            PrimeSourceStableLocalStaticMarkupInitializers(operations);
            PrimeSourceStableLocalComponentTypeInitializers(operations);
            PrimeLocalFunctionDeclarations(operations);
            var nextOperationIndex = 0;
            foreach (var segment in segments)
            {
                for (; nextOperationIndex < segment.StartIndex; nextOperationIndex++)
                    ParseOperation(operations[nextOperationIndex]);

                AddImperativeSegment(operations, segment);
                nextOperationIndex = segment.EndExclusive;
            }

            for (; nextOperationIndex < operations.Count; nextOperationIndex++)
                ParseOperation(operations[nextOperationIndex]);

            EnsureNoPendingImmediateAssignmentDeclarations();

            if (_openFrames.Count > 0)
                throw CreateStructuralIssueForUnclosedFrames();

            return new RazorVueRenderFragment(_rootChildren.ToImmutableArray());
        }

        private void ParseOperation(IOperation? operation)
        {
            var current = Unwrap(operation);
            if (current is null)
                return;

            if (HasPendingImmediateAssignmentDeclarations() &&
                !IsPendingImmediateAssignment(current) &&
                !IsPendingImmediateAssignmentContinuation(current))
            {
                ThrowPendingImmediateAssignmentRequiresImmediateAssignment(current);
            }

            switch (current)
            {
                case IExpressionStatementOperation expressionStatement:
                    ParseExpressionStatement(expressionStatement);
                    break;
                case IConditionalOperation conditional:
                    if (_allowCallerOwnedOpenNodeConditionalReplay &&
                        TryParseCallerOwnedOpenNodeConditionalReplay(conditional))
                    {
                        break;
                    }

                    ThrowIfComponentTypeCarrierUsedAsRuntimeValue(conditional.Condition, conditional);
                    AddNode(new RazorVueConditionalNode(
                        conditional.Condition,
                        ParseNestedBranch(conditional.WhenTrue),
                        ParseNestedBranch(conditional.WhenFalse),
                        CreateOrigins(current, RazorVueOriginKind.Template)));
                    break;
                case ISwitchOperation switchOperation
                    when _allowCallerOwnedOpenNodeConditionalReplay &&
                         TryParseCallerOwnedOpenNodeSwitchReplay(switchOperation):
                    break;
                case IForEachLoopOperation foreachLoop:
                    ThrowIfComponentTypeCarrierUsedAsRuntimeValue(foreachLoop.Collection, foreachLoop);
                    AddNode(new RazorVueForEachNode(
                        foreachLoop.Locals.Length > 0 ? foreachLoop.Locals[0].Name : "item",
                        foreachLoop.Locals.Length > 0 ? foreachLoop.Locals[0] : null,
                        foreachLoop.Collection,
                        ParseNestedBranch(foreachLoop.Body, foreachLoop.Locals),
                        CreateOrigins(current, RazorVueOriginKind.Template)));
                    break;
                case IForLoopOperation forLoop:
                    if (TryCreateForNode(forLoop, out var forNode))
                    {
                        AddNode(forNode);
                    }
                    else
                    {
                        AddNode(new RazorVueImperativeBlockNode(
                            [forLoop],
                            RazorVueImperativeBlockKind.LoopBlock,
                            CollectVisibleLocals([forLoop]),
                            CollectVisibleParameters([forLoop], _builderParameters),
                            CreateOrigins(forLoop, RazorVueOriginKind.Template)));
                    }
                    break;
                case IBlockOperation block:
                    foreach (var child in block.Operations)
                        ParseOperation(child);
                    break;
                case IVariableDeclarationGroupOperation variableDeclarationGroup:
                    ParseVariableDeclarationGroup(variableDeclarationGroup);
                    break;
                case IInvocationOperation invocation:
                    ParseOperationExpression(invocation);
                    break;
                case ILocalFunctionOperation:
                case IEmptyOperation:
                    break;
                case IReturnOperation { IsImplicit: true }:
                    break;
                case IReturnOperation returnOperation:
                    throw CreateStructuralIssue(
                        returnOperation,
                        $"BuildRenderTree does not support 'return' statements during RazorVue template extraction in component '{_snapshot.Descriptor.FullName}'. Move this control flow outside the render body or use the Razor IR frontend.");
                case ILoopOperation loop:
                    throw CreateStructuralIssue(
                        loop,
                        $"BuildRenderTree does not support loop statement '{GetOperationDisplay(loop)}' in component '{_snapshot.Descriptor.FullName}'. Only canonicalizable 'for' and 'foreach' loops are supported.");
                default:
                    throw CreateStructuralIssue(
                        current,
                        $"BuildRenderTree does not support statement '{GetOperationDisplay(current)}' ({current.Kind}) in component '{_snapshot.Descriptor.FullName}'.");
            }
        }

        private void AddImperativeSegment(
            IReadOnlyList<IOperation> operations,
            RazorVueImperativeRenderSegmentationPlanner.PlannedSegment segment)
        {
            var segmentOperations = operations
                .Skip(segment.StartIndex)
                .Take(segment.EndExclusive - segment.StartIndex)
                .ToImmutableArray();

            AddNode(new RazorVueImperativeBlockNode(
                segmentOperations,
                segment.Kind,
                CollectVisibleLocals(segmentOperations),
                CollectVisibleParameters(segmentOperations, _builderParameters),
                CreateOriginsStatic(segmentOperations, RazorVueOriginKind.Template)));
        }

        private void ParseExpressionStatement(IExpressionStatementOperation expressionStatement)
        {
            var statementOperation = Unwrap(expressionStatement.Operation);
            if (statementOperation is IInvocationOperation promotedInvocation &&
                TryGetCurrentComponentRenderHelperInvocationPromotionKind(promotedInvocation, out var promotedKind))
            {
                var promotedOperations = CreatePromotedRenderHelperOperations(expressionStatement, promotedInvocation);
                AddNode(new RazorVueImperativeBlockNode(
                    promotedOperations,
                    promotedKind,
                    CollectVisibleLocals(promotedOperations),
                    CollectVisibleParameters(promotedOperations, _builderParameters),
                    CreateOrigins(expressionStatement, RazorVueOriginKind.Template)));
                return;
            }

            if (statementOperation is ISimpleAssignmentOperation assignment)
            {
                if (TryRegisterBuilderAliasAssignment(assignment))
                    return;

                if (TryCompletePendingRenderFragmentLocalCarrier(assignment))
                    return;

                if (TryCompletePendingStaticMarkupLocalCarrier(assignment))
                    return;

                if (TryCompletePendingComponentTypeLocalCarrier(assignment))
                    return;

                if (TryCompletePendingTemplateScopedDeclaration(assignment))
                    return;

                throw CreateStructuralIssue(
                    assignment,
                    $"BuildRenderTree does not support assignment statement '{GetOperationDisplay(assignment)}' in component '{_snapshot.Descriptor.FullName}'. Only direct RenderTreeBuilder local alias assignments and the supported immediate-assignment local declaration patterns are allowed.");
            }

            if (statementOperation is not IInvocationOperation invocation)
            {
                throw CreateStructuralIssue(
                    statementOperation ?? expressionStatement,
                    $"BuildRenderTree does not support statement '{GetOperationDisplay(statementOperation ?? expressionStatement)}' in component '{_snapshot.Descriptor.FullName}'.");
            }

            ParseOperationExpression(invocation);
        }

        private bool TryGetCurrentComponentRenderHelperInvocationPromotionKind(
            IInvocationOperation invocation,
            out RazorVueImperativeBlockKind kind)
        {
            kind = default;
            if (_openFrames.Count > 0)
                return false;

            if (!IsCurrentComponentRenderHelperCandidate(invocation.TargetMethod, invocation.Instance))
                return false;

            if (!TryGetSupportedRenderHelperSignature(
                    invocation.TargetMethod,
                    out var builderParameter,
                    out _,
                    out _))
            {
                return false;
            }

            if (!TryGetRenderHelperInvocationBindings(
                    invocation,
                    builderParameter,
                    requireCallerOwnedReplaySafeBinding: false,
                    out var builderArgument,
                    out _,
                    out _))
            {
                return false;
            }

            if (!IsKnownBuilderReference(builderArgument.Value))
                return false;

            ImmutableArray<IOperation> operations;
            try
            {
                operations = GetRenderHelperOperations(invocation);
            }
            catch (RazorVueCompilationIssueException)
            {
                return false;
            }

            if (!RazorVueImperativeRenderPromotionAnalyzer.ShouldPromoteBody(operations))
                return false;

            ThrowIfReadOnlyRefParameterWritesOrEscapes(
                operations,
                GetReadOnlyRefParameters(invocation.TargetMethod),
                invocation);

            kind = RazorVueImperativeRenderPromotionAnalyzer.ClassifyBodyKind(operations);
            return true;
        }

        private ImmutableArray<IOperation> CreatePromotedRenderHelperOperations(
            IExpressionStatementOperation expressionStatement,
            IInvocationOperation invocation)
        {
            if (invocation.TargetMethod.MethodKind != MethodKind.LocalFunction)
                return [expressionStatement];

            var declarations = CollectReferencedLocalFunctionDeclarations(invocation);
            if (declarations.Length == 0)
                return [expressionStatement];

            var builder = ImmutableArray.CreateBuilder<IOperation>(declarations.Length + 1);
            builder.AddRange(declarations);
            builder.Add(expressionStatement);
            return builder.ToImmutable();
        }

        private ImmutableArray<IOperation> CollectReferencedLocalFunctionDeclarations(IInvocationOperation rootInvocation)
        {
            var required = new HashSet<IMethodSymbol>(SymbolEqualityComparer.Default);
            CollectReferencedLocalFunctions(rootInvocation, required);
            if (required.Count == 0)
                return ImmutableArray<IOperation>.Empty;

            var builder = ImmutableArray.CreateBuilder<IOperation>();
            var emitted = new HashSet<IMethodSymbol>(SymbolEqualityComparer.Default);
            var added = true;
            while (added)
            {
                added = false;
                foreach (var declaration in _localFunctionDeclarationOrder)
                {
                    var method = declaration.Symbol.OriginalDefinition;
                    if (!required.Contains(method) || !emitted.Add(method))
                        continue;

                    builder.Add(declaration);
                    added = true;
                    CollectReferencedLocalFunctions(declaration, required);
                }
            }

            return builder.ToImmutable();
        }

        private void CollectReferencedLocalFunctions(
            IOperation operation,
            HashSet<IMethodSymbol> referencedMethods)
        {
            foreach (var current in RazorVueOperationLocalCollector.EnumerateSelfAndDescendants(
                         operation,
                         includeLocalFunctionBodies: true))
            {
                if (current is IInvocationOperation { TargetMethod.MethodKind: MethodKind.LocalFunction } invocation)
                    referencedMethods.Add(invocation.TargetMethod.OriginalDefinition);
            }
        }

        private void ParseOperationExpression(IInvocationOperation invocation)
        {
            if (TryParseCurrentComponentRenderHelperInvocation(invocation))
                return;

            if (TryParseEventModifierInvocation(invocation))
                return;

            ThrowIfReadOnlyByRefParameterEscapes(invocation);

            if (!IsRenderTreeBuilderInvocation(invocation))
            {
                if (IsRenderTreeBuilderMethod(invocation.TargetMethod))
                {
                    throw CreateUnsupportedBuilderCall(
                        invocation,
                        $"BuildRenderTree call '{GetBuilderCallDisplayName(invocation)}' uses a RenderTreeBuilder receiver that RazorVue cannot track in component '{_snapshot.Descriptor.FullName}'. " +
                        "Use the active builder parameter or a direct local alias of that parameter.");
                }

                throw CreateStructuralIssue(
                    invocation,
                    $"BuildRenderTree does not support standalone invocation '{GetBuilderCallDisplayName(invocation)}' in component '{_snapshot.Descriptor.FullName}'. Only RenderTreeBuilder calls and supported render helpers may participate in RazorVue template extraction.");
            }

            switch (invocation.TargetMethod.Name)
            {
                case "OpenElement":
                    OpenElement(invocation);
                    break;
                case "CloseElement":
                    CloseCurrentNode(invocation, expectedComponent: false);
                    break;
                case "OpenComponent":
                    OpenComponent(invocation);
                    break;
                case "CloseComponent":
                    CloseCurrentNode(invocation, expectedComponent: true);
                    break;
                case "AddAttribute":
                    AddAttribute(invocation);
                    break;
                case "AddComponentParameter":
                    AddComponentParameter(invocation);
                    break;
                case "AddMultipleAttributes":
                    AddMultipleAttributes(invocation);
                    break;
                case "SetKey":
                    SetKey(invocation);
                    break;
                case "OpenRegion":
                    OpenRegion(invocation);
                    break;
                case "CloseRegion":
                    CloseRegion(invocation);
                    break;
                case "AddContent":
                    AddContent(invocation);
                    break;
                case "AddMarkupContent":
                    AddMarkupContent(invocation);
                    break;
                default:
                    throw CreateUnsupportedBuilderCall(
                        invocation,
                        $"RazorVue BuildRenderTree frontend does not support builder call '{GetBuilderCallDisplayName(invocation)}' in component '{_snapshot.Descriptor.FullName}'.");
            }
        }

        private void OpenElement(IInvocationOperation invocation)
        {
            var tagName = GetConstantStringArgument(invocation, 1);
            if (string.IsNullOrWhiteSpace(tagName))
            {
                throw CreateUnsupportedBuilderCall(
                    invocation,
                    $"BuildRenderTree call '{GetBuilderCallDisplayName(invocation)}' requires a constant element name in component '{_snapshot.Descriptor.FullName}'.");
            }

            _openFrames.Push(new ElementBuilder(tagName!, CreateOrigins(invocation, RazorVueOriginKind.Template)));
        }

        private void OpenComponent(IInvocationOperation invocation)
        {
            if (!TryResolveOpenComponent(invocation, out var componentType, out var resolutionName))
            {
                throw CreateUnsupportedBuilderCall(
                    invocation,
                    $"BuildRenderTree call '{GetBuilderCallDisplayName(invocation)}' must specify a concrete component type that RazorVue can resolve in component '{_snapshot.Descriptor.FullName}'.");
            }

            _openFrames.Push(new ComponentBuilder(
                componentType.Name,
                componentType.ToDisplayString(),
                resolutionName,
                componentType,
                CreateOrigins(invocation, RazorVueOriginKind.Template)));
        }

        private bool TryResolveOpenComponent(
            IInvocationOperation invocation,
            out INamedTypeSymbol componentType,
            out string resolutionName)
        {
            componentType = default!;
            resolutionName = string.Empty;

            if (invocation.TargetMethod.TypeArguments.Length == 1 &&
                invocation.TargetMethod.TypeArguments[0] is INamedTypeSymbol genericComponentType)
            {
                componentType = genericComponentType;
                resolutionName = GetGenericComponentResolutionName(invocation, componentType.ToDisplayString());
                return true;
            }

            if (RazorVueComponentTypeCarrierHelper.TryResolveComponentType(
                    _compilation,
                    _snapshot.ComponentSymbol,
                    GetInvocationArgument(invocation, 1),
                    out var explicitComponentType,
                    out var typeOfOperation))
            {
                componentType = explicitComponentType;
                resolutionName = typeOfOperation is null
                    ? componentType.Name
                    : GetTypeOfComponentResolutionName(typeOfOperation, componentType.ToDisplayString());
                return true;
            }

            if (RazorVueComponentTypeCarrierHelper.TryGetInvalidatedSourceStableComponentTypeMember(
                    _compilation,
                    _snapshot.ComponentSymbol,
                    GetInvocationArgument(invocation, 1),
                    out var memberCarrier))
            {
                throw CreateStructuralIssue(
                    invocation,
                    $"RazorVue System.Type member '{memberCarrier.Name}' in component '{_snapshot.Descriptor.FullName}' cannot be observed through later writes. OpenComponent(Type) carriers must remain source-stable.");
            }

            return false;
        }

        private static string GetGenericComponentResolutionName(IInvocationOperation invocation, string fallback)
        {
            if (invocation.Syntax is not InvocationExpressionSyntax invocationSyntax)
                return fallback;

            if (invocationSyntax.Expression is not MemberAccessExpressionSyntax { Name: GenericNameSyntax genericName })
                return fallback;

            if (genericName.TypeArgumentList.Arguments.Count != 1)
                return fallback;

            return genericName.TypeArgumentList.Arguments[0].ToString();
        }

        private static string GetTypeOfComponentResolutionName(ITypeOfOperation typeOfOperation, string fallback)
            => typeOfOperation.Syntax is TypeOfExpressionSyntax { Type: { } typeSyntax }
                ? typeSyntax.ToString()
                : fallback;

        private void OpenRegion(IInvocationOperation invocation)
            => _openFrames.Push(new RegionScope(CreateOrigins(invocation, RazorVueOriginKind.Template)));

        private void CloseRegion(IInvocationOperation invocation)
        {
            if (_openFrames.Count == 0)
            {
                throw CreateStructuralIssue(
                    invocation,
                    $"BuildRenderTree encountered 'CloseRegion' without a matching open region in component '{_snapshot.Descriptor.FullName}'.");
            }

            if (_openFrames.Peek() is not RegionScope)
            {
                throw CreateStructuralIssue(
                    invocation,
                    $"BuildRenderTree encountered 'CloseRegion' while the current open frame is {_openFrames.Peek().Describe()} in component '{_snapshot.Descriptor.FullName}'.");
            }

            _openFrames.Pop();
        }

        private void CloseCurrentNode(IInvocationOperation invocation, bool expectedComponent)
        {
            if (_openFrames.Count == 0)
                throw CreateStructuralIssue(
                    invocation,
                    $"BuildRenderTree encountered '{invocation.TargetMethod.Name}' without a matching open frame in component '{_snapshot.Descriptor.FullName}'.");

            if (_openFrames.Peek() is not OpenNodeBuilder current)
            {
                throw CreateStructuralIssue(
                    invocation,
                    $"BuildRenderTree encountered '{invocation.TargetMethod.Name}' while the current open frame is {_openFrames.Peek().Describe()} in component '{_snapshot.Descriptor.FullName}'.");
            }

            if (current is ComponentBuilder != expectedComponent)
            {
                throw CreateStructuralIssue(
                    invocation,
                    $"BuildRenderTree encountered '{invocation.TargetMethod.Name}' while the current open node is {current.Describe()} in component '{_snapshot.Descriptor.FullName}'.");
            }

            _openFrames.Pop();
            AddNode(current.Build());
        }

        private void AddAttribute(IInvocationOperation invocation)
        {
            var name = GetConstantStringArgument(invocation, 1);
            if (string.IsNullOrWhiteSpace(name))
            {
                throw CreateUnsupportedBuilderCall(
                    invocation,
                    $"BuildRenderTree call '{GetBuilderCallDisplayName(invocation)}' requires a constant attribute name in component '{_snapshot.Descriptor.FullName}'.");
            }

            var value = GetInvocationArgument(invocation, 2);
            ThrowIfComponentTypeCarrierUsedAsRuntimeValue(value, invocation);
            var currentNode = GetRequiredOpenNodeBuilder(invocation);
            if (string.Equals(name, "@key", StringComparison.Ordinal))
            {
                currentNode.SetKey(value, ToCapturedValueBindings(_activeCapturedBindings), CreateOrigins(invocation, RazorVueOriginKind.Template));
                return;
            }

            if (TryHandleComponentSlotValue(currentNode, name!, value, invocation))
                return;

            if (string.Equals(name, "ChildContent", StringComparison.Ordinal) &&
                TryParseChildContent(value, out var childContent))
            {
                currentNode.AddImplicitDefaultSlotAssignment(new RazorVueImplicitDefaultSlotAssignmentNode(
                    childContent,
                    CreateOrigins(invocation, RazorVueOriginKind.Template)));
                foreach (var child in childContent.Children)
                    currentNode.AddChild(child);
                return;
            }

            if (ShouldOmitElementAttribute(currentNode, value))
                return;

            currentNode.AddAttribute(new RazorVueAttributeNode(
                name!,
                value,
                ToCapturedValueBindings(_activeCapturedBindings),
                CreateOrigins(invocation, RazorVueOriginKind.Template))
            {
                EventModifiers = currentNode.GetEventModifiers(name!)
            });
        }

        private bool TryParseEventModifierInvocation(IInvocationOperation invocation)
        {
            if (!IsEventModifierInvocation(invocation.TargetMethod) ||
                invocation.Arguments.Length < 4)
            {
                return false;
            }

            var builderArgument = GetInvocationArgument(invocation, 0);
            if (!IsKnownBuilderReference(builderArgument))
            {
                throw CreateUnsupportedBuilderCall(
                    invocation,
                    $"BuildRenderTree call '{GetBuilderCallDisplayName(invocation)}' uses a RenderTreeBuilder receiver that RazorVue cannot track in component '{_snapshot.Descriptor.FullName}'. " +
                    "Use the active builder parameter or a direct local alias of that parameter.");
            }

            var eventHandlerName = GetConstantStringArgument(invocation, 2);
            if (string.IsNullOrWhiteSpace(eventHandlerName))
            {
                throw CreateUnsupportedBuilderCall(
                    invocation,
                    $"BuildRenderTree call '{GetBuilderCallDisplayName(invocation)}' requires a constant event handler name in component '{_snapshot.Descriptor.FullName}'.");
            }

            var value = GetInvocationArgument(invocation, 3);
            ThrowIfComponentTypeCarrierUsedAsRuntimeValue(value, invocation);
            if (value is null)
            {
                throw CreateUnsupportedBuilderCall(
                    invocation,
                    $"BuildRenderTree call '{GetBuilderCallDisplayName(invocation)}' requires an event modifier value in component '{_snapshot.Descriptor.FullName}'.");
            }

            var currentNode = GetRequiredOpenElementBuilder(invocation);
            currentNode.SetEventModifier(
                eventHandlerName!,
                invocation.TargetMethod.Name,
                new RazorVueEventModifierBinding(
                    value,
                    ToCapturedValueBindings(_activeCapturedBindings),
                    CreateOrigins(invocation, RazorVueOriginKind.Template)),
                CreateOrigins(invocation, RazorVueOriginKind.Template));
            return true;
        }

        private void AddComponentParameter(IInvocationOperation invocation)
        {
            var name = GetConstantStringArgument(invocation, 1);
            if (string.IsNullOrWhiteSpace(name))
            {
                throw CreateUnsupportedBuilderCall(
                    invocation,
                    $"BuildRenderTree call '{GetBuilderCallDisplayName(invocation)}' requires a constant parameter name in component '{_snapshot.Descriptor.FullName}'.");
            }

            var value = GetInvocationArgument(invocation, 2);
            ThrowIfComponentTypeCarrierUsedAsRuntimeValue(value, invocation);
            var currentNode = GetRequiredOpenComponentBuilder(invocation);
            if (string.Equals(name, "@key", StringComparison.Ordinal))
            {
                currentNode.SetKey(value, ToCapturedValueBindings(_activeCapturedBindings), CreateOrigins(invocation, RazorVueOriginKind.Template));
                return;
            }

            if (TryHandleComponentSlotValue(currentNode, name!, value, invocation))
                return;

            if (string.Equals(name, "ChildContent", StringComparison.Ordinal) &&
                TryParseChildContent(value, out var childContent))
            {
                currentNode.AddImplicitDefaultSlotAssignment(new RazorVueImplicitDefaultSlotAssignmentNode(
                    childContent,
                    CreateOrigins(invocation, RazorVueOriginKind.Template)));
                foreach (var child in childContent.Children)
                    currentNode.AddChild(child);
                return;
            }

            currentNode.AddAttribute(new RazorVueAttributeNode(
                name!,
                value,
                ToCapturedValueBindings(_activeCapturedBindings),
                CreateOrigins(invocation, RazorVueOriginKind.Template)));
        }

        private void AddMultipleAttributes(IInvocationOperation invocation)
        {
            var value = GetInvocationArgument(invocation, 1);
            ThrowIfComponentTypeCarrierUsedAsRuntimeValue(value, invocation);
            if (value is null || IsConstantNull(value))
                return;

            var currentNode = GetRequiredOpenNodeBuilder(invocation);
            currentNode.AddAttribute(new RazorVueAttributeSpreadNode(
                value,
                ToCapturedValueBindings(_activeCapturedBindings),
                CreateOrigins(invocation, RazorVueOriginKind.Template)));
        }

        private void SetKey(IInvocationOperation invocation)
        {
            var currentNode = GetRequiredOpenNodeBuilder(invocation);
            var key = GetInvocationArgument(invocation, 0);
            ThrowIfComponentTypeCarrierUsedAsRuntimeValue(key, invocation);
            currentNode.SetKey(key, ToCapturedValueBindings(_activeCapturedBindings), CreateOrigins(invocation, RazorVueOriginKind.Template));
        }

        private void AddContent(IInvocationOperation invocation)
        {
            var value = GetInvocationArgument(invocation, 1);
            ThrowIfComponentTypeCarrierUsedAsRuntimeValue(value, invocation);
            if (value is null || IsConstantNull(value))
                return;

            var origins = CreateOrigins(invocation, RazorVueOriginKind.Template);
            if (TryResolveSlotOutlet(value, out var slotName))
            {
                AddNode(new RazorVueSlotOutletNode(
                    slotName,
                    GetInvocationArgument(invocation, 2),
                    origins));
                return;
            }

            if (TryParseAddContentFragmentFactory(invocation, value, out var factoryFragment))
            {
                foreach (var child in factoryFragment.Children)
                    AddNode(child);
                return;
            }

            if (TryParseTypedAddContentTemplate(invocation, value, out var typedFragment))
            {
                foreach (var child in typedFragment.Children)
                    AddNode(child);
                return;
            }

            if (IsMarkupStringAddContent(invocation))
            {
                if (TryResolveStaticMarkupRender(value) is { } staticMarkup)
                {
                    AddStaticMarkupContent(invocation, staticMarkup);
                    return;
                }

                TryThrowInvalidatedStaticMarkupCarrier(value, "MarkupString AddContent(...)");
                throw CreateUnsupportedBuilderCall(
                    invocation,
                    $"BuildRenderTree call '{GetBuilderCallDisplayName(invocation)}' uses MarkupString content that is not compile-time provable static markup in component '{_snapshot.Descriptor.FullName}'. RazorVue only supports static MarkupString literals that can be canonicalized into a safe render subtree.");
            }

            if (TryParseAddContentRenderFragment(invocation, value, out var fragment))
            {
                foreach (var child in fragment.Children)
                    AddNode(child);
                return;
            }

            if (IsRenderFragmentAddContent(invocation))
            {
                throw CreateUnsupportedBuilderCall(
                    invocation,
                    $"BuildRenderTree call '{GetBuilderCallDisplayName(invocation)}' uses a RenderFragment shape that RazorVue cannot canonicalize in component '{_snapshot.Descriptor.FullName}'.");
            }

            if (TryGetConstantString(value) is string text)
            {
                AddNode(new RazorVueTextNode(text, origins));
                return;
            }

            AddNode(new RazorVueExpressionNode(value, origins));
        }

        private void ThrowIfComponentTypeCarrierUsedAsRuntimeValue(IOperation? operation, IOperation origin)
        {
            if (operation is null)
                return;

            foreach (var candidate in EnumerateSelfAndDescendants(operation))
            {
                switch (Unwrap(candidate))
                {
                    case ITypeOfOperation { TypeOperand: INamedTypeSymbol typeOperand }
                        when RazorVueComponentTypeCarrierHelper.IsVueComponentType(_compilation, typeOperand):
                        throw CreateStructuralIssue(
                            origin,
                            $"BuildRenderTree uses typeof(...) as a runtime value in component '{_snapshot.Descriptor.FullName}'. RazorVue only supports System.Type values as source-stable OpenComponent(Type) carriers.");
                    case ILocalReferenceOperation localReference
                        when RazorVueComponentTypeCarrierHelper.TryResolveSourceStableVueComponentTypeLocal(
                            _compilation,
                            _snapshot.ComponentSymbol,
                            localReference.Local,
                            out _):
                        throw CreateStructuralIssue(
                            origin,
                            $"BuildRenderTree uses System.Type local '{localReference.Local.Name}' as a runtime value in component '{_snapshot.Descriptor.FullName}'. RazorVue only supports System.Type locals as source-stable OpenComponent(Type) carriers.");
                    case { }
                        when RazorVueComponentTypeCarrierHelper.TryGetInvalidatedSourceStableComponentTypeMember(
                            _compilation,
                            _snapshot.ComponentSymbol,
                            candidate,
                            out var invalidatedMemberCarrier):
                        throw CreateStructuralIssue(
                            origin,
                            $"RazorVue System.Type member '{invalidatedMemberCarrier.Name}' in component '{_snapshot.Descriptor.FullName}' cannot be observed through later writes. OpenComponent(Type) carriers must remain source-stable.");
                    case IPropertyReferenceOperation or IFieldReferenceOperation
                        when RazorVueComponentTypeCarrierHelper.TryResolveSourceStableVueComponentTypeMember(
                            _compilation,
                            _snapshot.ComponentSymbol,
                            candidate,
                            out var memberCarrier,
                            out _):
                        throw CreateStructuralIssue(
                            origin,
                            $"BuildRenderTree uses System.Type member '{memberCarrier.Name}' as a runtime value in component '{_snapshot.Descriptor.FullName}'. RazorVue only supports System.Type members as source-stable OpenComponent(Type) carriers.");
                }
            }
        }

        private void AddMarkupContent(IInvocationOperation invocation)
        {
            var value = GetInvocationArgument(invocation, 1);
            if (value is null || IsConstantNull(value))
                return;

            if (TryResolveStaticMarkupRender(value) is not { } staticMarkup)
            {
                TryThrowInvalidatedStaticMarkupCarrier(value, "AddMarkupContent(...)");
                throw CreateUnsupportedBuilderCall(
                    invocation,
                    $"BuildRenderTree call '{GetBuilderCallDisplayName(invocation)}' uses AddMarkupContent(...) content that is not compile-time provable static markup in component '{_snapshot.Descriptor.FullName}'. RazorVue only supports static markup literals/carriers that can be canonicalized into a safe render subtree.");
            }

            AddStaticMarkupContent(invocation, staticMarkup);
        }

        private void AddStaticMarkupContent(
            IInvocationOperation invocation,
            RazorVueStaticMarkupValueHelper.StaticMarkupRenderResolution resolution)
        {
            var fragment = CreateStaticMarkupFragment(invocation, resolution);
            foreach (var child in fragment.Children)
                AddNode(child);
        }

        private RazorVueRenderFragment CreateStaticMarkupFragment(
            IInvocationOperation invocation,
            RazorVueStaticMarkupValueHelper.StaticMarkupRenderResolution resolution)
        {
            var origins = CreateOrigins(invocation, RazorVueOriginKind.Template);
            return resolution switch
            {
                RazorVueStaticMarkupValueHelper.StaticMarkupLiteralRenderResolution literal =>
                    CreateStaticMarkupFragment(invocation, literal.Resolution, origins),
                RazorVueStaticMarkupValueHelper.StaticMarkupConditionalRenderResolution conditional =>
                    new RazorVueRenderFragment(
                    [
                        new RazorVueConditionalNode(
                            conditional.Condition,
                            CreateStaticMarkupFragment(invocation, conditional.WhenTrue),
                            CreateStaticMarkupFragment(invocation, conditional.WhenFalse),
                            origins)
                    ]),
                _ => RazorVueRenderFragment.Empty
            };
        }

        private RazorVueRenderFragment CreateStaticMarkupFragment(
            IInvocationOperation invocation,
            RazorVueStaticMarkupValueHelper.StaticMarkupResolution resolution)
        {
            var origins = CreateOrigins(invocation, RazorVueOriginKind.Template);
            return CreateStaticMarkupFragment(invocation, resolution, origins);
        }

        private RazorVueRenderFragment CreateStaticMarkupFragment(
            IInvocationOperation invocation,
            RazorVueStaticMarkupValueHelper.StaticMarkupResolution resolution,
            ImmutableArray<RazorVueSourceOrigin> origins)
        {
            var fragment = ParseStaticMarkupFragment(
                resolution.Markup,
                origins,
                invocation);

            for (var index = resolution.CapturedBindings.Length - 1; index >= 0; index--)
            {
                var binding = resolution.CapturedBindings[index];
                fragment = new RazorVueRenderFragment(
                [
                    new RazorVueTemplateScopeNode(
                        ScopeName: binding.ParameterSymbol.Name,
                        ScopeParameterSymbol: binding.ParameterSymbol,
                        Initializer: binding.Initializer,
                        Children: fragment,
                        Origins: origins)
                ]);
            }

            return fragment;
        }

        private RazorVueRenderFragment ParseStaticMarkupFragment(
            string markup,
            ImmutableArray<RazorVueSourceOrigin> origins,
            IInvocationOperation invocation)
        {
            if (string.IsNullOrEmpty(markup))
                return RazorVueRenderFragment.Empty;

            var nodes = RazorVueStaticMarkupParser.Parse(
                markup,
                origins,
                new RazorVueStaticMarkupParser.Dependencies(
                    CreateLiteralStringOperation,
                    detail => CreateUnsupportedBuilderCall(
                        invocation,
                        $"BuildRenderTree call '{GetBuilderCallDisplayName(invocation)}' {detail} in component '{_snapshot.Descriptor.FullName}'.")));
            return new RazorVueRenderFragment(nodes);
        }

        private string? TryGetStaticMarkupString(IOperation? operation)
            => RazorVueStaticMarkupValueHelper.TryGetStaticMarkupValue(
                operation,
                _compilation,
                TryGetLocalMarkupStringInitializer,
                TryGetPropertyMarkupStringInitializer,
                TryGetFieldMarkupStringInitializer,
                TryGetStaticMarkupFactoryReturnedValue,
                IsSupportedStaticMarkupFactoryInvocation);

        private RazorVueStaticMarkupValueHelper.StaticMarkupResolution? TryResolveStaticMarkup(IOperation? operation)
            => RazorVueStaticMarkupValueHelper.TryResolveStaticMarkup(
                operation,
                _compilation,
                TryGetLocalMarkupStringInitializer,
                TryGetPropertyMarkupStringInitializer,
                TryGetFieldMarkupStringInitializer,
                TryGetStaticMarkupFactoryReturnedValue,
                IsSupportedStaticMarkupFactoryInvocation);

        private RazorVueStaticMarkupValueHelper.StaticMarkupRenderResolution? TryResolveStaticMarkupRender(IOperation? operation)
            => RazorVueStaticMarkupValueHelper.TryResolveStaticMarkupRender(
                operation,
                _compilation,
                TryGetLocalMarkupStringInitializer,
                TryGetPropertyMarkupStringInitializer,
                TryGetFieldMarkupStringInitializer,
                TryGetStaticMarkupFactoryReturnedValue,
                IsSupportedStaticMarkupFactoryInvocation);

        private void TryThrowInvalidatedStaticMarkupCarrier(IOperation? operation, string? api = null)
        {
            var apiPrefix = string.IsNullOrWhiteSpace(api)
                ? string.Empty
                : api + " ";

            if (Unwrap(operation) is not ILocalReferenceOperation localReference ||
                !RazorVueStaticMarkupValueHelper.IsStaticMarkupCarrierType(localReference.Local.Type))
            {
                if (RazorVueStaticMarkupValueHelper.TryGetInvalidatedSourceStableStaticMarkupMember(
                        operation,
                        _compilation,
                        out var member))
                {
                    var memberKind = GetStaticMarkupMemberCarrierKind(member);
                    throw CreateStructuralIssue(
                        operation!,
                        $"RazorVue {apiPrefix}{memberKind} member '{member.Name}' in component '{_snapshot.Descriptor.FullName}' cannot be observed through later writes. Static markup carriers must remain source-stable.");
                }

                return;
            }

            if (RazorVueSourceStableLocalInitializerHelper.IsSourceStableLocalInitializerInvalidatedByLaterWrites(
                    _compilation,
                    localReference.Local,
                    RazorVueStaticMarkupValueHelper.IsStaticMarkupCarrierType))
            {
                var carrierKind = RazorVueStaticMarkupValueHelper.IsMarkupStringType(localReference.Local.Type)
                    ? "MarkupString"
                    : "string";
                throw CreateStructuralIssue(
                    localReference,
                    $"RazorVue {apiPrefix}{carrierKind} local '{localReference.Local.Name}' in component '{_snapshot.Descriptor.FullName}' cannot be observed through later writes. Declaration-initialized local carriers must remain source-stable.");
            }

            if (RazorVueStaticMarkupValueHelper.TryGetInvalidatedSourceStableStaticMarkupMember(
                    operation,
                    _compilation,
                    out var memberCarrier))
            {
                var memberKind = GetStaticMarkupMemberCarrierKind(memberCarrier);
                throw CreateStructuralIssue(
                    operation!,
                    $"RazorVue {apiPrefix}{memberKind} member '{memberCarrier.Name}' in component '{_snapshot.Descriptor.FullName}' cannot be observed through later writes. Static markup carriers must remain source-stable.");
            }
        }

        private static string GetStaticMarkupMemberCarrierKind(ISymbol member)
        {
            var type = member switch
            {
                IPropertySymbol property => property.Type,
                IFieldSymbol field => field.Type,
                _ => null
            };
            return RazorVueStaticMarkupValueHelper.IsMarkupStringType(type)
                ? "MarkupString"
                : "string static-markup";
        }

        private RazorVueRenderFragment ParseNestedBranch(IOperation? operation)
        {
            var current = Unwrap(operation);
            if (current is null)
                return RazorVueRenderFragment.Empty;

            return current switch
            {
                IBlockOperation block => new Parser(
                    _snapshot,
                    _compilation,
                    _symbols,
                    _builderParameters,
                    _builderAliases,
                    _activeRenderHelperMethods,
                    _activeRenderFragmentMembers,
                    _activeRenderFragmentFactories,
                    GetLocalRenderFragmentCarrierSnapshot(),
                    GetMemberRenderFragmentCarrierSnapshot(),
                    GetFactoryRenderFragmentCarrierSnapshot(),
                    GetLocalFunctionDeclarationSnapshot(),
                    _accessibleTemplateLocals,
                    _accessibleTemplateParameters,
                    _allowTemplateScopedLocals).Parse(block.Operations),
                _ => new Parser(
                    _snapshot,
                    _compilation,
                    _symbols,
                    _builderParameters,
                    _builderAliases,
                    _activeRenderHelperMethods,
                    _activeRenderFragmentMembers,
                    _activeRenderFragmentFactories,
                    GetLocalRenderFragmentCarrierSnapshot(),
                    GetMemberRenderFragmentCarrierSnapshot(),
                    GetFactoryRenderFragmentCarrierSnapshot(),
                    GetLocalFunctionDeclarationSnapshot(),
                    _accessibleTemplateLocals,
                    _accessibleTemplateParameters,
                    _allowTemplateScopedLocals).Parse([current])
            };
        }

        private RazorVueRenderFragment ParseNestedBranch(
            IOperation? operation,
            IEnumerable<ILocalSymbol> additionalTemplateLocals)
        {
            var current = Unwrap(operation);
            if (current is null)
                return RazorVueRenderFragment.Empty;

            var mergedTemplateLocals = new HashSet<ILocalSymbol>(_accessibleTemplateLocals, SymbolEqualityComparer.Default);
            foreach (var local in additionalTemplateLocals)
                mergedTemplateLocals.Add(local);

            return current switch
            {
                IBlockOperation block => new Parser(
                    _snapshot,
                    _compilation,
                    _symbols,
                    _builderParameters,
                    _builderAliases,
                    _activeRenderHelperMethods,
                    _activeRenderFragmentMembers,
                    _activeRenderFragmentFactories,
                    GetLocalRenderFragmentCarrierSnapshot(),
                    GetMemberRenderFragmentCarrierSnapshot(),
                    GetFactoryRenderFragmentCarrierSnapshot(),
                    GetLocalFunctionDeclarationSnapshot(),
                    mergedTemplateLocals,
                    _accessibleTemplateParameters,
                    _allowTemplateScopedLocals).Parse(block.Operations),
                _ => new Parser(
                    _snapshot,
                    _compilation,
                    _symbols,
                    _builderParameters,
                    _builderAliases,
                    _activeRenderHelperMethods,
                    _activeRenderFragmentMembers,
                    _activeRenderFragmentFactories,
                    GetLocalRenderFragmentCarrierSnapshot(),
                    GetMemberRenderFragmentCarrierSnapshot(),
                    GetFactoryRenderFragmentCarrierSnapshot(),
                    GetLocalFunctionDeclarationSnapshot(),
                    mergedTemplateLocals,
                    _accessibleTemplateParameters,
                    _allowTemplateScopedLocals).Parse([current])
            };
        }

        private bool TryParseCallerOwnedOpenNodeConditionalReplay(IConditionalOperation conditional)
        {
            if (!TryGetNearestOpenNodeBuilder(out var currentNode) ||
                _openFrames.Count != 1)
            {
                return false;
            }

            ThrowIfComponentTypeCarrierUsedAsRuntimeValue(conditional.Condition, conditional);
            var whenTrue = ParseCallerOwnedOpenNodeConditionalReplayBranch(
                conditional,
                currentNode,
                conditional.WhenTrue);
            var whenFalse = ParseCallerOwnedOpenNodeConditionalReplayBranch(
                conditional,
                currentNode,
                conditional.WhenFalse);

            currentNode.AddReplayOperation(new RazorVueOpenNodeConditionalReplayOperation(
                conditional.Condition,
                whenTrue,
                whenFalse,
                CreateOrigins(conditional, RazorVueOriginKind.Template)));
            return true;
        }

        private bool TryParseCallerOwnedOpenNodeSwitchReplay(ISwitchOperation switchOperation)
        {
            if (!TryGetNearestOpenNodeBuilder(out var currentNode) ||
                _openFrames.Count != 1)
            {
                return false;
            }

            ThrowIfComponentTypeCarrierUsedAsRuntimeValue(switchOperation.Value, switchOperation);
            var sections = ImmutableArray.CreateBuilder<RazorVueOpenNodeSwitchReplaySection>(switchOperation.Cases.Length);
            foreach (var switchCase in switchOperation.Cases)
            {
                if (!TryParseCallerOwnedOpenNodeSwitchReplaySection(
                        switchOperation,
                        currentNode,
                        switchCase,
                        out var section))
                {
                    return false;
                }

                sections.Add(section);
            }

            if (sections.Count == 0)
                return false;

            currentNode.AddReplayOperation(new RazorVueOpenNodeSwitchReplayOperation(
                switchOperation.Value,
                sections.ToImmutable(),
                CreateOrigins(switchOperation, RazorVueOriginKind.Template)));
            return true;
        }

        private bool CanParseCallerOwnedOpenNodeSwitchReplayShape(ISwitchOperation switchOperation)
        {
            if (switchOperation.Cases.IsDefaultOrEmpty)
                return false;

            foreach (var switchCase in switchOperation.Cases)
            {
                if (switchCase.Clauses.IsDefaultOrEmpty)
                    return false;

                foreach (var clause in switchCase.Clauses)
                {
                    if (clause is IDefaultCaseClauseOperation or ISingleValueCaseClauseOperation)
                        continue;

                    if (clause is not IPatternCaseClauseOperation patternClause ||
                        !CanParseCallerOwnedOpenNodeSwitchPatternReplayLabelShape(patternClause))
                    {
                        return false;
                    }
                }

                var patternDeclaredLocals = CollectCallerOwnedSwitchPatternDeclaredLocals(switchCase.Clauses);
                if (!TryGetCallerOwnedSwitchCaseReplayOperations(switchCase.Body, out var replayBodyOperations) ||
                    ContainsCallerOwnedSwitchCasePatternDeclaredLocalReference(replayBodyOperations, patternDeclaredLocals))
                {
                    return false;
                }
            }

            return true;
        }

        private bool TryParseCallerOwnedOpenNodeSwitchReplaySection(
            ISwitchOperation switchOperation,
            OpenNodeBuilder currentNode,
            ISwitchCaseOperation switchCase,
            out RazorVueOpenNodeSwitchReplaySection section)
        {
            section = default!;
            var labels = ImmutableArray.CreateBuilder<RazorVueOpenNodeSwitchReplayLabel>(switchCase.Clauses.Length);
            foreach (var clause in switchCase.Clauses)
            {
                switch (clause)
                {
                    case IDefaultCaseClauseOperation:
                        labels.Add(new RazorVueOpenNodeSwitchReplayLabel(
                            RazorVueOpenNodeSwitchReplayLabelKind.Default,
                            Value: null));
                        break;
                    case ISingleValueCaseClauseOperation singleValueCase:
                        ThrowIfComponentTypeCarrierUsedAsRuntimeValue(singleValueCase.Value, singleValueCase);
                        labels.Add(new RazorVueOpenNodeSwitchReplayLabel(
                            RazorVueOpenNodeSwitchReplayLabelKind.Value,
                            singleValueCase.Value));
                        break;
                    case IPatternCaseClauseOperation patternCase
                        when CanParseCallerOwnedOpenNodeSwitchPatternReplayLabelShape(patternCase):
                        ThrowIfComponentTypeCarrierUsedAsRuntimeValue(patternCase.Pattern, patternCase);
                        if (patternCase.Guard is not null)
                            ThrowIfComponentTypeCarrierUsedAsRuntimeValue(patternCase.Guard, patternCase);

                        labels.Add(new RazorVueOpenNodeSwitchReplayLabel(
                            RazorVueOpenNodeSwitchReplayLabelKind.Condition,
                            patternCase));
                        break;
                    default:
                        return false;
                }
            }

            var patternDeclaredLocals = CollectCallerOwnedSwitchPatternDeclaredLocals(switchCase.Clauses);
            if (labels.Count == 0 ||
                labels.Any(static label => label.IsDefault) && labels.Count != 1 ||
                !TryGetCallerOwnedSwitchCaseReplayOperations(switchCase.Body, out var replayBodyOperations) ||
                ContainsCallerOwnedSwitchCasePatternDeclaredLocalReference(replayBodyOperations, patternDeclaredLocals))
            {
                return false;
            }

            var operations = ParseCallerOwnedOpenNodeConditionalReplayBranch(
                switchOperation,
                currentNode,
                replayBodyOperations,
                ImmutableArray<IOperation>.Empty);
            section = new RazorVueOpenNodeSwitchReplaySection(labels.ToImmutable(), operations);
            return true;
        }

        private bool CanParseCallerOwnedOpenNodeSwitchPatternReplayLabelShape(
            IPatternCaseClauseOperation patternClause)
        {
            var conditionLocals = CollectCallerOwnedSwitchPatternDeclaredLocals(patternClause);
            foreach (var local in _accessibleTemplateLocals)
                conditionLocals.Add(local);

            if (ContainsCallerOwnedSwitchPatternUnsupportedLocalReference(patternClause.Pattern, conditionLocals) ||
                ContainsCallerOwnedSwitchPatternUnsupportedLocalReference(patternClause.Guard, conditionLocals) ||
                ContainsCallerOwnedSwitchUnsupportedPatternConditionShape(patternClause.Pattern) ||
                ContainsCallerOwnedSwitchUnsupportedPatternConditionShape(patternClause.Guard))
            {
                return false;
            }

            return true;
        }

        private static bool ContainsCallerOwnedSwitchPatternUnsupportedLocalReference(
            IOperation? operation,
            ISet<ILocalSymbol> allowedLocals)
        {
            if (operation is null)
                return false;

            foreach (var descendant in RazorVueOperationLocalCollector.EnumerateSelfAndDescendants(operation))
            {
                if (descendant is ILocalReferenceOperation localReference &&
                    !allowedLocals.Contains(localReference.Local))
                {
                    return true;
                }
            }

            return false;
        }

        private static HashSet<ILocalSymbol> CollectCallerOwnedSwitchPatternDeclaredLocals(
            ImmutableArray<ICaseClauseOperation> clauses)
        {
            var locals = new HashSet<ILocalSymbol>(SymbolEqualityComparer.Default);
            foreach (var clause in clauses)
            {
                if (clause is IPatternCaseClauseOperation patternCase)
                    CollectCallerOwnedSwitchPatternDeclaredLocals(patternCase, locals);
            }

            return locals;
        }

        private static HashSet<ILocalSymbol> CollectCallerOwnedSwitchPatternDeclaredLocals(
            IPatternCaseClauseOperation patternClause)
        {
            var locals = new HashSet<ILocalSymbol>(SymbolEqualityComparer.Default);
            CollectCallerOwnedSwitchPatternDeclaredLocals(patternClause, locals);
            return locals;
        }

        private static void CollectCallerOwnedSwitchPatternDeclaredLocals(
            IPatternCaseClauseOperation patternClause,
            HashSet<ILocalSymbol> locals)
        {
            CollectCallerOwnedSwitchPatternDeclaredLocals(patternClause.Pattern, locals);
            CollectCallerOwnedSwitchPatternDeclaredLocals(patternClause.Guard, locals);
        }

        private static void CollectCallerOwnedSwitchPatternDeclaredLocals(
            IOperation? operation,
            HashSet<ILocalSymbol> locals)
        {
            if (operation is null)
                return;

            foreach (var descendant in RazorVueOperationLocalCollector.EnumerateSelfAndDescendants(operation))
            {
                switch (descendant)
                {
                    case IDeclarationPatternOperation { DeclaredSymbol: ILocalSymbol declarationLocal }:
                        locals.Add(declarationLocal);
                        break;
                    case IRecursivePatternOperation { DeclaredSymbol: ILocalSymbol recursiveLocal }:
                        locals.Add(recursiveLocal);
                        break;
                    case IListPatternOperation { DeclaredSymbol: ILocalSymbol listLocal }:
                        locals.Add(listLocal);
                        break;
                }
            }
        }

        private static bool ContainsCallerOwnedSwitchUnsupportedPatternConditionShape(IOperation? operation)
        {
            if (operation is null)
                return false;

            foreach (var descendant in RazorVueOperationLocalCollector.EnumerateSelfAndDescendants(operation))
            {
                switch (descendant)
                {
                    case IInvocationOperation:
                    case IAwaitOperation:
                    case IAssignmentOperation:
                    case IIncrementOrDecrementOperation:
                    case IThrowOperation:
                    case IAnonymousFunctionOperation:
                    case ILocalFunctionOperation:
                    case IVariableDeclaratorOperation:
                    case IDeclarationExpressionOperation:
                        return true;
                }

                if (descendant.Kind is OperationKind.FlowAnonymousFunction or OperationKind.FlowCapture)
                    return true;
            }

            return false;
        }

        private static bool ContainsCallerOwnedSwitchCasePatternDeclaredLocalReference(
            ImmutableArray<IOperation> operations,
            ISet<ILocalSymbol> patternDeclaredLocals)
        {
            if (operations.IsDefaultOrEmpty || patternDeclaredLocals.Count == 0)
                return false;

            foreach (var operation in operations)
            {
                foreach (var descendant in RazorVueOperationLocalCollector.EnumerateSelfAndDescendants(operation))
                {
                    if (descendant is ILocalReferenceOperation localReference &&
                        patternDeclaredLocals.Contains(localReference.Local))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private static bool TryGetCallerOwnedSwitchCaseReplayOperations(
            ImmutableArray<IOperation> body,
            out ImmutableArray<IOperation> replayBodyOperations)
        {
            replayBodyOperations = ImmutableArray<IOperation>.Empty;
            if (body.IsDefaultOrEmpty)
                return false;

            var normalizedBody = ImmutableArray.CreateBuilder<IOperation>(body.Length);
            foreach (var operation in body)
            {
                var current = Unwrap(operation);
                if (current is null)
                    continue;

                normalizedBody.Add(current);
            }

            if (normalizedBody.Count == 0 ||
                normalizedBody[normalizedBody.Count - 1] is not IBranchOperation { BranchKind: BranchKind.Break })
            {
                return false;
            }

            for (var index = 0; index < normalizedBody.Count - 1; index++)
            {
                if (ContainsCallerOwnedSwitchCaseBreak(normalizedBody[index]))
                    return false;
            }

            replayBodyOperations = normalizedBody.Take(normalizedBody.Count - 1).ToImmutableArray();
            return true;
        }

        private static bool ContainsCallerOwnedSwitchCaseBreak(IOperation operation)
            => RazorVueOperationLocalCollector.EnumerateSelfAndDescendants(operation)
                .Any(static current => current is IBranchOperation { BranchKind: BranchKind.Break });

        private bool TryParseCallerOwnedOpenNodeGuardReturnReplay(
            IReadOnlyList<IOperation> operations,
            int operationIndex)
        {
            if (!_allowCallerOwnedOpenNodeConditionalReplay ||
                operationIndex < 0 ||
                operationIndex >= operations.Count ||
                Unwrap(operations[operationIndex]) is not IConditionalOperation conditional ||
                !TryGetNearestOpenNodeBuilder(out var currentNode) ||
                _openFrames.Count != 1)
            {
                return false;
            }

            var whenTrue = ClassifyCallerOwnedReturnBranch(conditional.WhenTrue);
            var whenFalse = ClassifyCallerOwnedReturnBranch(conditional.WhenFalse);
            if (!whenTrue.Returns && !whenFalse.Returns)
                return false;

            ThrowIfComponentTypeCarrierUsedAsRuntimeValue(conditional.Condition, conditional);
            var tailOperations = operations
                .Skip(operationIndex + 1)
                .ToImmutableArray();

            var whenTrueReplay = whenTrue.Returns
                ? ParseCallerOwnedOpenNodeConditionalReplayBranch(
                    conditional,
                    currentNode,
                    whenTrue.BeforeReturn,
                    ImmutableArray<IOperation>.Empty)
                : ParseCallerOwnedOpenNodeConditionalReplayBranch(
                    conditional,
                    currentNode,
                    conditional.WhenTrue,
                    tailOperations);
            var whenFalseReplay = whenFalse.Returns
                ? ParseCallerOwnedOpenNodeConditionalReplayBranch(
                    conditional,
                    currentNode,
                    whenFalse.BeforeReturn,
                    ImmutableArray<IOperation>.Empty)
                : ParseCallerOwnedOpenNodeConditionalReplayBranch(
                    conditional,
                    currentNode,
                    conditional.WhenFalse,
                    tailOperations);

            currentNode.AddReplayOperation(new RazorVueOpenNodeConditionalReplayOperation(
                conditional.Condition,
                whenTrueReplay,
                whenFalseReplay,
                CreateOrigins(conditional, RazorVueOriginKind.Template)));
            return true;
        }

        private ImmutableArray<RazorVueOpenNodeReplayOperation> ParseCallerOwnedOpenNodeConditionalReplayBranch(
            IOperation branchOwner,
            OpenNodeBuilder currentNode,
            IOperation? branch)
            => ParseCallerOwnedOpenNodeConditionalReplayBranch(
                branchOwner,
                currentNode,
                branch,
                ImmutableArray<IOperation>.Empty);

        private ImmutableArray<RazorVueOpenNodeReplayOperation> ParseCallerOwnedOpenNodeConditionalReplayBranch(
            IOperation branchOwner,
            OpenNodeBuilder currentNode,
            IOperation? branch,
            ImmutableArray<IOperation> tailOperations)
            => ParseCallerOwnedOpenNodeConditionalReplayBranch(
                branchOwner,
                currentNode,
                GetBranchOperations(branch),
                tailOperations);

        private ImmutableArray<RazorVueOpenNodeReplayOperation> ParseCallerOwnedOpenNodeConditionalReplayBranch(
            IOperation branchOwner,
            OpenNodeBuilder currentNode,
            ImmutableArray<IOperation> branchOperations,
            ImmutableArray<IOperation> tailOperations)
        {
            if (branchOperations.IsDefaultOrEmpty && tailOperations.IsDefaultOrEmpty)
                return ImmutableArray<RazorVueOpenNodeReplayOperation>.Empty;

            var branchParser = CreateConditionalReplayBranchParser();
            var branchOpenNode = currentNode.CreateEmptyClone();
            branchParser._allowCallerOwnedOpenNodeConditionalReplay = true;
            branchParser._openFrames.Push(branchOpenNode);

            if (tailOperations.IsDefaultOrEmpty)
            {
                branchParser.ParseCallerOwnedOpenNodeMutationOperations(branchOperations);
            }
            else
            {
                var combinedOperations = ImmutableArray.CreateBuilder<IOperation>(branchOperations.Length + tailOperations.Length);
                combinedOperations.AddRange(branchOperations);
                combinedOperations.AddRange(tailOperations);
                branchParser.ParseCallerOwnedOpenNodeMutationOperations(combinedOperations.ToImmutable());
            }

            branchParser.EnsureNoPendingImmediateAssignmentDeclarations();
            branchParser.ValidateCallerOwnedOpenNodeMutationPostState(
                branchOwner,
                originalFrameDepth: 1,
                branchOpenNode);
            return branchOpenNode.CreateSnapshot().ReplayOperations;
        }

        private Parser CreateConditionalReplayBranchParser()
            => new(
                _snapshot,
                _compilation,
                _symbols,
                _builderParameters,
                _builderAliases,
                _activeRenderHelperMethods,
                _activeRenderFragmentMembers,
                _activeRenderFragmentFactories,
                GetLocalRenderFragmentCarrierSnapshot(),
                GetMemberRenderFragmentCarrierSnapshot(),
                GetFactoryRenderFragmentCarrierSnapshot(),
                GetLocalFunctionDeclarationSnapshot(),
                _accessibleTemplateLocals,
                _accessibleTemplateParameters,
                _allowTemplateScopedLocals);

        private void ParseCallerOwnedOpenNodeMutationOperations(IReadOnlyList<IOperation> operations)
        {
            for (var index = 0; index < operations.Count; index++)
            {
                if (TryParseCallerOwnedOpenNodeImperativeLoopReplay(operations, index, out var loopReplayOperationCount))
                {
                    index += loopReplayOperationCount - 1;
                    continue;
                }

                if (TryParseCallerOwnedOpenNodeImperativeTryReplay(operations[index]))
                    continue;

                if (TryParseCallerOwnedOpenNodeImperativeLockReplay(operations[index]))
                    continue;

                if (TryParseCallerOwnedOpenNodeImperativeUsingDeclarationReplay(operations, index, out var usingDeclarationReplayOperationCount))
                {
                    index += usingDeclarationReplayOperationCount - 1;
                    continue;
                }

                if (TryParseCallerOwnedOpenNodeImperativeUsingReplay(operations[index]))
                    continue;

                if (TryParseCallerOwnedOpenNodeLocalDeclarationReplay(operations, index))
                    continue;

                if (TryParseCallerOwnedOpenNodeGuardReturnReplay(operations, index))
                    return;

                if (Unwrap(operations[index]) is ISwitchOperation switchOperation &&
                    TryParseCallerOwnedOpenNodeSwitchReplay(switchOperation))
                {
                    continue;
                }

                ThrowIfUnsupportedCallerOwnedOpenNodeControlFlow(operations[index]);
                ParseOperation(operations[index]);
            }
        }

        private bool TryParseCallerOwnedOpenNodeLocalDeclarationReplay(
            IReadOnlyList<IOperation> operations,
            int operationIndex)
        {
            if (!_allowCallerOwnedOpenNodeConditionalReplay ||
                !TryGetNearestOpenNodeBuilder(out var currentNode) ||
                _openFrames.Count != 1 ||
                operationIndex < 0 ||
                operationIndex >= operations.Count ||
                Unwrap(operations[operationIndex]) is not IVariableDeclarationGroupOperation declarationGroup ||
                !HasCallerOwnedOpenNodeLocalDeclarationReplayConsumer(operations, operationIndex + 1))
            {
                return false;
            }

            foreach (var declaration in declarationGroup.Declarations)
            {
                foreach (var declarator in declaration.Declarators)
                {
                    if (IsRenderTreeBuilderType(declarator.Symbol.Type))
                        return false;

                    if (declarator.Initializer?.Value is not { } initializer)
                        return false;

                    if (IsCallerOwnedOpenNodeLocalInvalidatedByLaterWrites(declarator.Symbol))
                    {
                        throw CreateStructuralIssue(
                            declarator,
                            $"BuildRenderTree caller-owned open frame replay local '{declarator.Symbol.Name}' in component '{_snapshot.Descriptor.FullName}' cannot be observed through later writes. Caller-owned replay locals must remain source-stable to preserve replay order and captured value evaluation count.");
                    }

                    ValidateTemplateScopedInitializer(declarator, initializer);
                }
            }

            foreach (var declaration in declarationGroup.Declarations)
            {
                foreach (var declarator in declaration.Declarators)
                {
                    var initializer = declarator.Initializer!.Value;
                    currentNode.AddReplayOperation(new RazorVueOpenNodeLocalDeclarationReplayOperation(
                        declarator.Symbol,
                        initializer,
                        CreateOrigins(declarator, RazorVueOriginKind.Template)));
                    _accessibleTemplateLocals.Add(declarator.Symbol);
                }
            }

            return true;
        }

        private bool HasCallerOwnedOpenNodeLocalDeclarationReplayConsumer(
            IReadOnlyList<IOperation> operations,
            int startIndex)
        {
            for (var index = startIndex; index < operations.Count; index++)
            {
                switch (Unwrap(operations[index]))
                {
                    case null:
                    case IEmptyOperation:
                    case ILocalFunctionOperation:
                    case IVariableDeclarationGroupOperation:
                        continue;
                    case IConditionalOperation:
                    case ISwitchOperation:
                        return true;
                    case IExpressionStatementOperation expressionStatement
                        when Unwrap(expressionStatement.Operation) is IInvocationOperation invocation &&
                             IsRenderTreeBuilderInvocation(invocation):
                        return true;
                    default:
                        continue;
                }
            }

            return false;
        }

        private bool IsCallerOwnedOpenNodeLocalInvalidatedByLaterWrites(ILocalSymbol local)
            => RazorVueSourceStableLocalInitializerHelper.IsSourceStableLocalInitializerInvalidatedByLaterWrites(
                _compilation,
                local,
                static _ => true);

        private bool TryParseCallerOwnedOpenNodeImperativeLoopReplay(
            IReadOnlyList<IOperation> operations,
            int operationIndex,
            out int consumedOperationCount)
        {
            consumedOperationCount = 0;
            if (!_allowCallerOwnedOpenNodeConditionalReplay ||
                !TryGetNearestOpenNodeBuilder(out var currentNode) ||
                _openFrames.Count != 1 ||
                operationIndex < 0 ||
                operationIndex >= operations.Count)
            {
                return false;
            }

            var replayOperations = ImmutableArray.CreateBuilder<IOperation>();
            var scanIndex = operationIndex;
            while (scanIndex < operations.Count &&
                   Unwrap(operations[scanIndex]) is IVariableDeclarationGroupOperation declarationGroup)
            {
                if (!CanIncludeCallerOwnedOpenNodeImperativeLoopPrelude(declarationGroup))
                    return false;

                replayOperations.Add(operations[scanIndex]);
                scanIndex++;
            }

            if (scanIndex >= operations.Count ||
                Unwrap(operations[scanIndex]) is not ILoopOperation loopOperation)
            {
                return false;
            }

            replayOperations.Add(operations[scanIndex]);
            if (ContainsUnsupportedCallerOwnedOpenNodeImperativeLoopReplayOperation(loopOperation))
            {
                throw CreateStructuralIssue(
                    loopOperation,
                    $"BuildRenderTree caller-owned open frame replay does not support loop control flow '{GetOperationDisplay(loopOperation)}' in component '{_snapshot.Descriptor.FullName}'. Caller-owned loop replay must preserve active frame identity and frame depth; loops that open/close frames, jump, throw, dispose, lock, or use exception control flow require a dedicated lowering protocol.");
            }

            var replayOperationArray = replayOperations.ToImmutable();
            currentNode.AddReplayOperation(new RazorVueOpenNodeImperativeReplayOperation(
                replayOperationArray,
                RazorVueImperativeBlockKind.LoopBlock,
                CollectVisibleLocals(replayOperationArray),
                CollectVisibleParameters(replayOperationArray, _builderParameters),
                CreateOriginsStatic(replayOperationArray, RazorVueOriginKind.Template)));
            consumedOperationCount = scanIndex - operationIndex + 1;
            return true;
        }

        private bool CanIncludeCallerOwnedOpenNodeImperativeLoopPrelude(IVariableDeclarationGroupOperation declarationGroup)
        {
            foreach (var declaration in declarationGroup.Declarations)
            {
                foreach (var declarator in declaration.Declarators)
                {
                    if (IsRenderTreeBuilderType(declarator.Symbol.Type) ||
                        declarator.Initializer?.Value is null)
                    {
                        return false;
                    }

                    ValidateTemplateScopedInitializer(declarator, declarator.Initializer.Value);
                }
            }

            return true;
        }

        private static bool ContainsUnsupportedCallerOwnedOpenNodeImperativeLoopReplayOperation(IOperation operation)
        {
            foreach (var current in RazorVueOperationLocalCollector.EnumerateSelfAndDescendants(operation))
            {
                switch (current)
                {
                    case ITryOperation:
                    case IUsingOperation:
                    case IUsingDeclarationOperation:
                    case ILockOperation:
                    case IThrowOperation:
                    case IReturnOperation { IsImplicit: false }:
                    case IBranchOperation { BranchKind: BranchKind.GoTo }:
                    case ILabeledOperation:
                        return true;
                    case IInvocationOperation invocation
                        when IsFrameDepthChangingBuilderInvocation(invocation):
                        return true;
                }
            }

            return false;
        }

        private static bool IsFrameDepthChangingBuilderInvocation(IInvocationOperation invocation)
        {
            if (invocation.Instance is null)
                return false;

            if (!string.Equals(
                    invocation.Instance.Type?.ToDisplayString(),
                    "Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder",
                    StringComparison.Ordinal))
            {
                return false;
            }

            return invocation.TargetMethod.Name is
                "OpenElement" or
                "CloseElement" or
                "OpenComponent" or
                "CloseComponent" or
                "OpenRegion" or
                "CloseRegion";
        }

        private bool TryParseCallerOwnedOpenNodeImperativeTryReplay(IOperation operation)
        {
            if (!_allowCallerOwnedOpenNodeConditionalReplay ||
                !TryGetNearestOpenNodeBuilder(out var currentNode) ||
                _openFrames.Count != 1 ||
                Unwrap(operation) is not ITryOperation tryOperation)
            {
                return false;
            }

            if (ContainsUnsupportedCallerOwnedOpenNodeImperativeTryReplayOperation(tryOperation))
            {
                throw CreateStructuralIssue(
                    tryOperation,
                    $"BuildRenderTree caller-owned open frame replay does not support try/catch/finally control flow '{GetOperationDisplay(tryOperation)}' in component '{_snapshot.Descriptor.FullName}'. Caller-owned try replay must preserve active frame identity, frame depth, replay order, and captured value evaluation count; nested runtime control flow, jump control flow, and frame-depth mutation require a dedicated lowering protocol.");
            }

            currentNode.AddReplayOperation(new RazorVueOpenNodeImperativeReplayOperation(
                [tryOperation],
                RazorVueImperativeBlockKind.TryBlock,
                CollectVisibleLocals([tryOperation]),
                CollectVisibleParameters([tryOperation], _builderParameters),
                CreateOrigins(tryOperation, RazorVueOriginKind.Template)));
            return true;
        }

        private static bool ContainsUnsupportedCallerOwnedOpenNodeImperativeTryReplayOperation(ITryOperation operation)
        {
            foreach (var current in RazorVueOperationLocalCollector.EnumerateSelfAndDescendants(operation))
            {
                switch (current)
                {
                    case ITryOperation nestedTry when !ReferenceEquals(nestedTry, operation):
                    case IUsingOperation:
                    case IUsingDeclarationOperation:
                    case ILockOperation:
                    case IReturnOperation { IsImplicit: false }:
                    case IBranchOperation:
                    case ILabeledOperation:
                        return true;
                    case IInvocationOperation invocation
                        when IsFrameDepthChangingBuilderInvocation(invocation):
                        return true;
                }
            }

            return false;
        }

        private bool TryParseCallerOwnedOpenNodeImperativeLockReplay(IOperation operation)
        {
            if (!_allowCallerOwnedOpenNodeConditionalReplay ||
                !TryGetNearestOpenNodeBuilder(out var currentNode) ||
                _openFrames.Count != 1 ||
                Unwrap(operation) is not ILockOperation lockOperation)
            {
                return false;
            }

            if (ContainsUnsupportedCallerOwnedOpenNodeImperativeLockReplayOperation(lockOperation))
            {
                throw CreateStructuralIssue(
                    lockOperation,
                    $"BuildRenderTree caller-owned open frame replay does not support lock control flow '{GetOperationDisplay(lockOperation)}' in component '{_snapshot.Descriptor.FullName}'. Caller-owned lock replay must preserve active frame identity and frame depth; nested runtime control flow, throw/jump control flow, and frame-depth mutation require a dedicated lowering protocol.");
            }

            currentNode.AddReplayOperation(new RazorVueOpenNodeImperativeReplayOperation(
                [lockOperation],
                RazorVueImperativeBlockKind.LockBlock,
                CollectVisibleLocals([lockOperation]),
                CollectVisibleParameters([lockOperation], _builderParameters),
                CreateOrigins(lockOperation, RazorVueOriginKind.Template)));
            return true;
        }

        private static bool ContainsUnsupportedCallerOwnedOpenNodeImperativeLockReplayOperation(ILockOperation operation)
        {
            foreach (var current in RazorVueOperationLocalCollector.EnumerateSelfAndDescendants(operation))
            {
                switch (current)
                {
                    case ILoopOperation:
                    case ISwitchOperation:
                    case ITryOperation:
                    case IUsingOperation:
                    case IUsingDeclarationOperation:
                    case ILockOperation nestedLock when !ReferenceEquals(nestedLock, operation):
                    case IThrowOperation:
                    case IReturnOperation { IsImplicit: false }:
                    case IBranchOperation:
                    case ILabeledOperation:
                        return true;
                    case IInvocationOperation invocation
                        when IsFrameDepthChangingBuilderInvocation(invocation):
                        return true;
                }
            }

            return false;
        }

        private bool TryParseCallerOwnedOpenNodeImperativeUsingReplay(IOperation operation)
        {
            if (!_allowCallerOwnedOpenNodeConditionalReplay ||
                !TryGetNearestOpenNodeBuilder(out var currentNode) ||
                _openFrames.Count != 1 ||
                Unwrap(operation) is not IUsingOperation usingOperation)
            {
                return false;
            }

            if (!IsStableNullUsingResource(usingOperation.Resources) ||
                ContainsUnsupportedCallerOwnedOpenNodeImperativeUsingReplayOperation(usingOperation))
            {
                throw CreateStructuralIssue(
                    usingOperation,
                    $"BuildRenderTree caller-owned open frame replay does not support using control flow '{GetOperationDisplay(usingOperation)}' in component '{_snapshot.Descriptor.FullName}'. Caller-owned using replay is limited to null/default resources with frame-neutral mutation so active frame identity, frame depth, replay order, and captured value evaluation count stay unchanged; dispose-aware replay requires a dedicated lowering protocol.");
            }

            currentNode.AddReplayOperation(new RazorVueOpenNodeImperativeReplayOperation(
                [usingOperation],
                RazorVueImperativeBlockKind.TryBlock,
                CollectVisibleLocals([usingOperation]),
                CollectVisibleParameters([usingOperation], _builderParameters),
                CreateOrigins(usingOperation, RazorVueOriginKind.Template)));
            return true;
        }

        private bool TryParseCallerOwnedOpenNodeImperativeUsingDeclarationReplay(
            IReadOnlyList<IOperation> operations,
            int operationIndex,
            out int consumedOperationCount)
        {
            consumedOperationCount = 0;
            if (!_allowCallerOwnedOpenNodeConditionalReplay ||
                !TryGetNearestOpenNodeBuilder(out var currentNode) ||
                _openFrames.Count != 1 ||
                operationIndex < 0 ||
                operationIndex >= operations.Count ||
                Unwrap(operations[operationIndex]) is not IUsingDeclarationOperation usingDeclarationOperation)
            {
                return false;
            }

            var replayOperations = operations
                .Skip(operationIndex)
                .ToImmutableArray();

            if (!IsStableNullUsingDeclaration(usingDeclarationOperation) ||
                ContainsUnsupportedCallerOwnedOpenNodeImperativeUsingDeclarationReplayOperation(replayOperations))
            {
                throw CreateStructuralIssue(
                    usingDeclarationOperation,
                    $"BuildRenderTree caller-owned open frame replay does not support using declaration control flow '{GetOperationDisplay(usingDeclarationOperation)}' in component '{_snapshot.Descriptor.FullName}'. Caller-owned using declaration replay is limited to null/default resources with frame-neutral mutation through the declaration's full disposal scope so active frame identity, frame depth, replay order, and captured value evaluation count stay unchanged; dispose-aware replay requires a dedicated lowering protocol.");
            }

            currentNode.AddReplayOperation(new RazorVueOpenNodeImperativeReplayOperation(
                replayOperations,
                RazorVueImperativeBlockKind.TryBlock,
                CollectVisibleLocals(replayOperations),
                CollectVisibleParameters(replayOperations, _builderParameters),
                CreateOriginsStatic(replayOperations, RazorVueOriginKind.Template)));
            consumedOperationCount = operations.Count - operationIndex;
            return true;
        }

        private static bool ContainsUnsupportedCallerOwnedOpenNodeImperativeUsingReplayOperation(IUsingOperation operation)
        {
            if (operation.IsAsynchronous)
                return true;

            foreach (var current in RazorVueOperationLocalCollector.EnumerateSelfAndDescendants(operation))
            {
                switch (current)
                {
                    case IUsingOperation nestedUsing when !ReferenceEquals(nestedUsing, operation):
                    case IUsingDeclarationOperation:
                    case ILoopOperation:
                    case ISwitchOperation:
                    case ITryOperation:
                    case ILockOperation:
                    case IThrowOperation:
                    case IReturnOperation { IsImplicit: false }:
                    case IBranchOperation:
                    case ILabeledOperation:
                        return true;
                    case IInvocationOperation invocation
                        when IsFrameDepthChangingBuilderInvocation(invocation):
                        return true;
                }
            }

            return false;
        }

        private static bool ContainsUnsupportedCallerOwnedOpenNodeImperativeUsingDeclarationReplayOperation(
            ImmutableArray<IOperation> operations)
        {
            foreach (var operation in operations)
            {
                foreach (var current in RazorVueOperationLocalCollector.EnumerateSelfAndDescendants(operation))
                {
                    switch (current)
                    {
                        case IUsingDeclarationOperation usingDeclaration
                            when !IsStableNullUsingDeclaration(usingDeclaration):
                        case IUsingOperation:
                        case ILoopOperation:
                        case ISwitchOperation:
                        case ITryOperation:
                        case ILockOperation:
                        case IThrowOperation:
                        case IReturnOperation { IsImplicit: false }:
                        case IBranchOperation:
                        case ILabeledOperation:
                            return true;
                        case IInvocationOperation invocation
                            when IsFrameDepthChangingBuilderInvocation(invocation):
                            return true;
                    }
                }
            }

            return false;
        }

        private static bool IsStableNullUsingDeclaration(IUsingDeclarationOperation usingDeclaration)
        {
            if (usingDeclaration.IsAsynchronous ||
                usingDeclaration.DeclarationGroup is null)
            {
                return false;
            }

            foreach (var declaration in usingDeclaration.DeclarationGroup.Declarations)
            {
                foreach (var declarator in declaration.Declarators)
                {
                    if (declarator.Initializer?.Value is not { } initializer ||
                        !IsStableNullUsingResource(initializer))
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        private static bool IsStableNullUsingResource(IOperation? operation)
        {
            var current = operation;
            while (true)
            {
                current = Unwrap(current);
                if (current is IConversionOperation { OperatorMethod: null } conversion)
                {
                    current = conversion.Operand;
                    continue;
                }

                break;
            }

            return current switch
            {
                null => false,
                IDefaultValueOperation defaultValue => IsNullDefaultValue(defaultValue),
                _ => current.ConstantValue.HasValue && current.ConstantValue.Value is null
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

        private void ThrowIfUnsupportedCallerOwnedOpenNodeControlFlow(IOperation operation)
        {
            foreach (var current in RazorVueOperationLocalCollector.EnumerateSelfAndDescendants(operation))
            {
                switch (current)
                {
                    case ITryOperation tryOperation:
                        throw CreateStructuralIssue(
                            tryOperation,
                            $"BuildRenderTree caller-owned open frame replay does not support try/catch/finally control flow '{GetOperationDisplay(tryOperation)}' in component '{_snapshot.Descriptor.FullName}'. Caller-owned helper mutation must prove active frame identity, frame depth, replay order, and captured value evaluation count; runtime control-flow replay requires a dedicated lowering protocol.");
                    case IUsingOperation usingOperation:
                        throw CreateStructuralIssue(
                            usingOperation,
                            $"BuildRenderTree caller-owned open frame replay does not support using control flow '{GetOperationDisplay(usingOperation)}' in component '{_snapshot.Descriptor.FullName}'. Caller-owned helper mutation must prove active frame identity, frame depth, replay order, and captured value evaluation count; dispose-aware replay requires a dedicated lowering protocol.");
                    case IUsingDeclarationOperation usingDeclarationOperation:
                        throw CreateStructuralIssue(
                            usingDeclarationOperation,
                            $"BuildRenderTree caller-owned open frame replay does not support using declaration control flow '{GetOperationDisplay(usingDeclarationOperation)}' in component '{_snapshot.Descriptor.FullName}'. Caller-owned helper mutation must prove active frame identity, frame depth, replay order, and captured value evaluation count; dispose-aware replay requires a dedicated lowering protocol.");
                    case ILockOperation lockOperation:
                        throw CreateStructuralIssue(
                            lockOperation,
                            $"BuildRenderTree caller-owned open frame replay does not support lock control flow '{GetOperationDisplay(lockOperation)}' in component '{_snapshot.Descriptor.FullName}'. Caller-owned helper mutation must prove active frame identity, frame depth, replay order, and captured value evaluation count; lock-aware replay requires a dedicated lowering protocol.");
                    case ISwitchOperation switchOperation:
                        if (!CanParseCallerOwnedOpenNodeSwitchReplayShape(switchOperation))
                        {
                            throw CreateStructuralIssue(
                                switchOperation,
                                $"BuildRenderTree caller-owned open frame replay does not support switch control flow '{GetOperationDisplay(switchOperation)}' in component '{_snapshot.Descriptor.FullName}'. Caller-owned helper mutation must prove active frame identity, frame depth, replay order, and captured value evaluation count; switch replay requires a dedicated lowering protocol.");
                        }

                        break;
                    case ILoopOperation loopOperation:
                        throw CreateStructuralIssue(
                            loopOperation,
                            $"BuildRenderTree caller-owned open frame replay does not support loop control flow '{GetOperationDisplay(loopOperation)}' in component '{_snapshot.Descriptor.FullName}'. Caller-owned helper mutation must prove active frame identity, frame depth, replay order, and captured value evaluation count; loop replay requires a dedicated lowering protocol.");
                    case IBranchOperation { BranchKind: BranchKind.GoTo } branchOperation:
                        throw CreateStructuralIssue(
                            branchOperation,
                            $"BuildRenderTree caller-owned open frame replay does not support goto control flow '{GetOperationDisplay(branchOperation)}' in component '{_snapshot.Descriptor.FullName}'. Caller-owned helper mutation must prove active frame identity, frame depth, replay order, and captured value evaluation count; jump replay requires a dedicated lowering protocol.");
                    case ILabeledOperation labeledOperation:
                        throw CreateStructuralIssue(
                            labeledOperation,
                            $"BuildRenderTree caller-owned open frame replay does not support labeled control flow '{GetOperationDisplay(labeledOperation)}' in component '{_snapshot.Descriptor.FullName}'. Caller-owned helper mutation must prove active frame identity, frame depth, replay order, and captured value evaluation count; jump replay requires a dedicated lowering protocol.");
                    case IThrowOperation throwOperation:
                        throw CreateStructuralIssue(
                            throwOperation,
                            $"BuildRenderTree caller-owned open frame replay does not support throw control flow '{GetOperationDisplay(throwOperation)}' in component '{_snapshot.Descriptor.FullName}'. Caller-owned helper mutation must prove active frame identity, frame depth, replay order, and captured value evaluation count; exception-aware replay requires a dedicated lowering protocol.");
                }
            }
        }

        private static CallerOwnedReturnBranch ClassifyCallerOwnedReturnBranch(IOperation? branch)
        {
            var branchOperations = GetBranchOperations(branch);
            if (branchOperations.IsDefaultOrEmpty ||
                Unwrap(branchOperations[branchOperations.Length - 1]) is not IReturnOperation { IsImplicit: false, ReturnedValue: null })
            {
                return CallerOwnedReturnBranch.DoesNotReturn;
            }

            var beforeReturnBuilder = ImmutableArray.CreateBuilder<IOperation>(branchOperations.Length - 1);
            for (var index = 0; index < branchOperations.Length - 1; index++)
                beforeReturnBuilder.Add(branchOperations[index]);

            return new CallerOwnedReturnBranch(
                Returns: true,
                BeforeReturn: beforeReturnBuilder.ToImmutable());
        }

        private static ImmutableArray<IOperation> GetBranchOperations(IOperation? operation)
        {
            var current = Unwrap(operation);
            return current switch
            {
                null => ImmutableArray<IOperation>.Empty,
                IBlockOperation block => block.Operations,
                _ => ImmutableArray.Create(current)
            };
        }

        private bool TryCreateForNode(IForLoopOperation loop, out RazorVueForNode forNode)
        {
            forNode = default!;
            if (!RazorVueForLoopAnalyzer.TryAnalyze(loop, Unwrap, out var analyzedLoop))
                return false;

            ThrowIfComponentTypeCarrierUsedAsRuntimeValue(analyzedLoop.InitialValue, loop);
            ThrowIfComponentTypeCarrierUsedAsRuntimeValue(analyzedLoop.LimitValue, loop);
            ThrowIfComponentTypeCarrierUsedAsRuntimeValue(analyzedLoop.StepValue, loop);

            RazorVueForLoopAnalyzer.ValidateStaticLoopProgressIfProvable(
                loop,
                analyzedLoop,
                Unwrap,
                _snapshot.Descriptor.FullName);

            forNode = new RazorVueForNode(
                analyzedLoop.VariableName,
                loop.Locals.Length > 0 ? loop.Locals[0] : null,
                analyzedLoop.InitialValue,
                analyzedLoop.ConditionKind,
                analyzedLoop.LimitValue,
                analyzedLoop.StepKind,
                analyzedLoop.StepValue,
                ParseNestedBranch(loop.Body, loop.Locals),
                CreateOrigins(loop, RazorVueOriginKind.Template));
            return true;
        }

        private void AddNode(RazorVueRenderNode node)
        {
            if (TryGetNearestOpenNodeBuilder(out var currentNode))
            {
                if (currentNode is ComponentBuilder)
                    currentNode.AddAmbientDefaultSlotChild(node);
                currentNode.AddChild(node);
            }
            else
            {
                _rootChildren.Add(node);
            }
        }

        private bool IsRenderTreeBuilderInvocation(IInvocationOperation invocation)
        {
            if (_builderParameters.Count == 0)
                return false;

            return IsKnownBuilderReference(invocation.Instance);
        }

        private bool TryParseCurrentComponentRenderHelperInvocation(IInvocationOperation invocation)
        {
            if (!IsCurrentComponentRenderHelperCandidate(invocation.TargetMethod, invocation.Instance))
                return false;

            if (!TryGetSupportedRenderHelperSignature(
                    invocation.TargetMethod,
                    out var builderParameter,
                    out var extraParameters,
                    out var failureMessage))
            {
                throw CreateUnsupportedBuilderCall(
                    invocation,
                    failureMessage);
            }

            if (!TryGetRenderHelperInvocationBindings(
                    invocation,
                    builderParameter,
                    requireCallerOwnedReplaySafeBinding: _openFrames.Count > 0,
                    out var builderArgument,
                    out var extraArgumentBindings,
                    out failureMessage))
            {
                throw CreateUnsupportedBuilderCall(
                    invocation,
                    failureMessage);
            }

            if (!IsKnownBuilderReference(builderArgument.Value))
            {
                throw CreateUnsupportedBuilderCall(
                    invocation,
                    $"BuildRenderTree helper method '{GetBuilderCallDisplayName(invocation)}' must receive the active RenderTreeBuilder parameter or a direct local alias in component '{_snapshot.Descriptor.FullName}'.");
            }

            if (_openFrames.Count > 0 &&
                _openFrames.Peek() is OpenNodeBuilder originalOpenNode)
            {
                ParseRenderHelperBodyWithCallerOwnedOpenNodeMutation(
                    invocation,
                    builderParameter,
                    extraParameters,
                    extraArgumentBindings,
                    originalOpenNode);
                return true;
            }

            if (extraArgumentBindings.IsDefaultOrEmpty)
            {
                ParseRenderHelperBody(invocation, builderParameter);
                return true;
            }

            var fragment = ParseRenderHelperBodyAsScopedFragment(invocation, builderParameter, extraParameters, extraArgumentBindings);
            foreach (var child in fragment.Children)
                AddNode(child);

            return true;
        }

        private bool IsKnownBuilderReference(IOperation? operation)
        {
            return Unwrap(operation) switch
            {
                IParameterReferenceOperation parameterReference => _builderParameters.Contains(parameterReference.Parameter),
                ILocalReferenceOperation localReference => _builderAliases.Contains(localReference.Local),
                _ => false
            };
        }

        private void ParseVariableDeclarationGroup(IVariableDeclarationGroupOperation declarationGroup)
        {
            foreach (var declaration in declarationGroup.Declarations)
            {
                foreach (var declarator in declaration.Declarators)
                {
                    if (TryRegisterBuilderAliasDeclaration(declarator, out var failureMessage))
                        continue;

                    if (IsRenderFragmentType(declarator.Symbol.Type))
                    {
                        if (TryRegisterRenderFragmentLocalCarrier(declarator, out failureMessage))
                            continue;

                        throw CreateStructuralIssue(
                            declarator,
                            failureMessage);
                    }

                    if (TryRegisterStaticMarkupLocalCarrier(declarator, out failureMessage))
                        continue;

                    if (TryRegisterComponentTypeLocalCarrier(declarator, out failureMessage))
                        continue;

                    if (TryRegisterTemplateScopedDeclaration(declarator, out failureMessage))
                        continue;

                    throw CreateStructuralIssue(
                        declarator,
                        failureMessage);
                }
            }
        }

        private bool TryRegisterBuilderAliasDeclaration(
            IVariableDeclaratorOperation declarator,
            out string failureMessage)
        {
            failureMessage =
                $"BuildRenderTree does not support local variable declaration '{GetOperationDisplay(declarator)}' in component '{_snapshot.Descriptor.FullName}'. Only direct RenderTreeBuilder local alias declarations are supported.";

            if (!IsRenderTreeBuilderType(declarator.Symbol.Type))
                return false;

            var value = declarator.Initializer?.Value;
            if (!IsKnownBuilderReference(value))
            {
                failureMessage =
                    $"BuildRenderTree local alias '{declarator.Symbol.Name}' in component '{_snapshot.Descriptor.FullName}' must be initialized from the active RenderTreeBuilder parameter or a direct local alias. Other RenderTreeBuilder receivers cannot be tracked safely.";
                return false;
            }

            _builderAliases.Add(declarator.Symbol);
            return true;
        }

        private bool TryRegisterBuilderAliasAssignment(ISimpleAssignmentOperation assignment)
        {
            if (assignment.Target is not ILocalReferenceOperation localReference)
                return false;

            if (!IsRenderTreeBuilderType(localReference.Local.Type))
                return false;

            if (!IsKnownBuilderReference(assignment.Value))
                return false;

            _builderAliases.Add(localReference.Local);
            return true;
        }

        private bool TryRegisterRenderFragmentLocalCarrier(
            IVariableDeclaratorOperation declarator,
            out string failureMessage)
        {
            failureMessage =
                $"BuildRenderTree does not support local variable declaration '{GetOperationDisplay(declarator)}' in component '{_snapshot.Descriptor.FullName}'.";

            if (!IsRenderFragmentType(declarator.Symbol.Type))
                return false;

            if (declarator.Initializer?.Value is not { } initializer)
            {
                if (!_allowTemplateScopedLocals)
                {
                    failureMessage =
                        $"RazorVue RenderFragment local '{declarator.Symbol.Name}' in component '{_snapshot.Descriptor.FullName}' requires an analyzable initializer.";
                    return false;
                }

                _pendingRenderFragmentLocalCarriers[declarator.Symbol] =
                    new PendingRenderFragmentLocalCarrierDeclaration(declarator);
                return true;
            }

            var sourceStableInitializer = TryGetSourceStableRenderFragmentInitializer(declarator.Symbol);
            if (sourceStableInitializer is null &&
                RazorVueImperativeRenderFragmentCarrierHelper.IsSourceStableLocalRenderFragmentInitializerInvalidatedByLaterWrites(
                    _compilation,
                    declarator.Symbol))
            {
                failureMessage =
                    $"RazorVue RenderFragment local '{declarator.Symbol.Name}' in component '{_snapshot.Descriptor.FullName}' cannot be observed through later writes. Declaration-initialized local carriers must remain source-stable.";
                return false;
            }

            if (sourceStableInitializer is not null)
                initializer = sourceStableInitializer;

            if (!TryParseSlotTemplate(initializer, out var slotTemplate))
            {
                failureMessage =
                    $"RazorVue RenderFragment local '{declarator.Symbol.Name}' in component '{_snapshot.Descriptor.FullName}' must be initialized from an analyzable inline template, current-component RenderFragment member, or supported fragment factory.";
                return false;
            }

            _localRenderFragmentCarriers[declarator.Symbol] = slotTemplate;
            return true;
        }

        private bool TryRegisterStaticMarkupLocalCarrier(
            IVariableDeclaratorOperation declarator,
            out string failureMessage)
        {
            failureMessage =
                $"BuildRenderTree does not support local variable declaration '{GetOperationDisplay(declarator)}' in component '{_snapshot.Descriptor.FullName}'.";

            if (!RazorVueStaticMarkupValueHelper.IsMarkupStringType(declarator.Symbol.Type))
                return false;

            if (declarator.Initializer?.Value is null && _allowTemplateScopedLocals)
            {
                _pendingStaticMarkupLocalCarriers[declarator.Symbol] =
                    new PendingStaticMarkupLocalCarrierDeclaration(declarator);
                return true;
            }

            var initializer = TryGetSourceStableStaticMarkupInitializer(declarator.Symbol);
            if (initializer is null)
            {
                failureMessage =
                    RazorVueSourceStableLocalInitializerHelper.IsSourceStableLocalInitializerInvalidatedByLaterWrites(
                        _compilation,
                        declarator.Symbol,
                        RazorVueStaticMarkupValueHelper.IsMarkupStringType)
                        ? $"RazorVue MarkupString local '{declarator.Symbol.Name}' in component '{_snapshot.Descriptor.FullName}' cannot be observed through later writes. Declaration-initialized local carriers must remain source-stable."
                        : $"RazorVue MarkupString local '{declarator.Symbol.Name}' in component '{_snapshot.Descriptor.FullName}' requires a compile-time provable static markup initializer.";
                return true;
            }

            if (TryGetStaticMarkupString(initializer) is null)
            {
                failureMessage =
                    $"RazorVue MarkupString local '{declarator.Symbol.Name}' in component '{_snapshot.Descriptor.FullName}' must be initialized from compile-time provable static markup or a previously analyzable static MarkupString carrier.";
                return true;
            }

            _localStaticMarkupCarriers[declarator.Symbol] = initializer;
            return true;
        }

        private bool TryRegisterComponentTypeLocalCarrier(
            IVariableDeclaratorOperation declarator,
            out string failureMessage)
        {
            failureMessage =
                $"BuildRenderTree does not support local variable declaration '{GetOperationDisplay(declarator)}' in component '{_snapshot.Descriptor.FullName}'.";

            if (!RazorVueComponentTypeCarrierHelper.IsSystemType(declarator.Symbol.Type))
                return false;

            var initializer = TryGetSourceStableComponentTypeInitializer(declarator.Symbol);
            if (initializer is null)
            {
                if (declarator.Initializer?.Value is { } directInitializer &&
                    Unwrap(directInitializer) is ITypeOfOperation { TypeOperand: INamedTypeSymbol directTypeOperand } &&
                    RazorVueComponentTypeCarrierHelper.IsVueComponentType(_compilation, directTypeOperand))
                {
                    throw CreateStructuralIssue(
                        declarator,
                        RazorVueSourceStableLocalInitializerHelper.IsSourceStableLocalInitializerInvalidatedByLaterWrites(
                            _compilation,
                            declarator.Symbol,
                            RazorVueComponentTypeCarrierHelper.IsSystemType)
                            ? $"RazorVue System.Type local '{declarator.Symbol.Name}' in component '{_snapshot.Descriptor.FullName}' cannot be observed through later writes. OpenComponent(Type) carriers must remain source-stable."
                            : $"RazorVue System.Type local '{declarator.Symbol.Name}' in component '{_snapshot.Descriptor.FullName}' requires a compile-time provable typeof(component) initializer before it can be used as an OpenComponent(Type) carrier.");
                }

                return false;
            }

            if (!RazorVueComponentTypeCarrierHelper.TryResolveComponentType(
                    _compilation,
                    _snapshot.ComponentSymbol,
                    initializer,
                    out var componentType,
                    out _) ||
                !RazorVueComponentTypeCarrierHelper.IsVueComponentType(_compilation, componentType))
            {
                return false;
            }

            if (declarator.Initializer?.Value is null && _allowTemplateScopedLocals)
                _pendingComponentTypeLocalCarriers[declarator.Symbol] =
                    new PendingComponentTypeLocalCarrierDeclaration(declarator);

            return true;
        }

        private bool TryCompletePendingComponentTypeLocalCarrier(ISimpleAssignmentOperation assignment)
        {
            if (assignment.Target is not ILocalReferenceOperation localReference)
                return false;

            if (!_pendingComponentTypeLocalCarriers.TryGetValue(localReference.Local, out var pendingDeclaration))
                return false;

            _pendingComponentTypeLocalCarriers.Remove(localReference.Local);
            var initializer = TryGetSourceStableComponentTypeInitializer(pendingDeclaration.Declarator.Symbol);
            if (initializer is null)
            {
                throw CreateStructuralIssue(
                    assignment,
                    $"RazorVue System.Type local '{pendingDeclaration.Declarator.Symbol.Name}' in component '{_snapshot.Descriptor.FullName}' must be assigned exactly once within the same linear local-declaration prefix by a simple assignment statement and cannot be observed through later writes.");
            }

            if (!RazorVueComponentTypeCarrierHelper.TryResolveComponentType(
                    _compilation,
                    _snapshot.ComponentSymbol,
                    initializer,
                    out var componentType,
                    out _) ||
                !RazorVueComponentTypeCarrierHelper.IsVueComponentType(_compilation, componentType))
            {
                return false;
            }

            return true;
        }

        private bool TryCompletePendingStaticMarkupLocalCarrier(ISimpleAssignmentOperation assignment)
        {
            if (assignment.Target is not ILocalReferenceOperation localReference)
                return false;

            if (!_pendingStaticMarkupLocalCarriers.TryGetValue(localReference.Local, out var pendingDeclaration))
                return false;

            _pendingStaticMarkupLocalCarriers.Remove(localReference.Local);
            var initializer = TryGetSourceStableStaticMarkupInitializer(pendingDeclaration.Declarator.Symbol);
            if (initializer is null)
            {
                throw CreateStructuralIssue(
                    assignment,
                    $"RazorVue MarkupString local '{pendingDeclaration.Declarator.Symbol.Name}' in component '{_snapshot.Descriptor.FullName}' must be assigned exactly once within the same linear local-declaration prefix by a simple assignment statement and cannot be observed through later writes.");
            }

            if (TryGetStaticMarkupString(initializer) is null)
            {
                throw CreateStructuralIssue(
                    assignment,
                    $"RazorVue MarkupString local '{pendingDeclaration.Declarator.Symbol.Name}' in component '{_snapshot.Descriptor.FullName}' must be initialized from compile-time provable static markup or a previously analyzable static MarkupString carrier.");
            }

            _localStaticMarkupCarriers[pendingDeclaration.Declarator.Symbol] = initializer;
            return true;
        }

        private bool TryRegisterTemplateScopedDeclaration(
            IVariableDeclaratorOperation declarator,
            out string failureMessage)
        {
            failureMessage =
                $"BuildRenderTree does not support local variable declaration '{GetOperationDisplay(declarator)}' in component '{_snapshot.Descriptor.FullName}'.";

            if (!_allowTemplateScopedLocals)
                return false;

            if (IsRenderTreeBuilderType(declarator.Symbol.Type))
                return false;

            if (declarator.Initializer?.Value is not { } initializer)
            {
                if (IsRenderFragmentType(declarator.Symbol.Type))
                {
                    failureMessage =
                        $"RazorVue RenderFragment local '{declarator.Symbol.Name}' in component '{_snapshot.Descriptor.FullName}' requires an analyzable initializer.";
                    return false;
                }

                _pendingTemplateScopedDeclarations[declarator.Symbol] = new PendingTemplateScopedDeclaration(declarator);
                return true;
            }

            CommitTemplateScopedDeclaration(declarator, initializer);
            return true;
        }

        private bool TryCompletePendingTemplateScopedDeclaration(ISimpleAssignmentOperation assignment)
        {
            if (assignment.Target is not ILocalReferenceOperation localReference)
                return false;

            if (!_pendingTemplateScopedDeclarations.TryGetValue(localReference.Local, out var pendingDeclaration))
                return false;

            _pendingTemplateScopedDeclarations.Remove(localReference.Local);
            CommitTemplateScopedDeclaration(pendingDeclaration.Declarator, assignment.Value);
            return true;
        }

        private bool TryCompletePendingRenderFragmentLocalCarrier(ISimpleAssignmentOperation assignment)
        {
            if (assignment.Target is not ILocalReferenceOperation localReference)
                return false;

            if (!_pendingRenderFragmentLocalCarriers.TryGetValue(localReference.Local, out var pendingDeclaration))
                return false;

            _pendingRenderFragmentLocalCarriers.Remove(localReference.Local);
            var sourceStableInitializer = TryGetSourceStableRenderFragmentInitializer(pendingDeclaration.Declarator.Symbol);
            if (sourceStableInitializer is null)
            {
                throw CreateStructuralIssue(
                    assignment,
                    $"RazorVue RenderFragment local '{pendingDeclaration.Declarator.Symbol.Name}' in component '{_snapshot.Descriptor.FullName}' must be assigned exactly once within the same linear local-declaration prefix by a simple assignment statement and cannot be observed through later writes.");
            }

            var initializer = sourceStableInitializer;
            if (!TryParseSlotTemplate(initializer, out var slotTemplate))
            {
                throw CreateStructuralIssue(
                    assignment,
                    $"RazorVue RenderFragment local '{pendingDeclaration.Declarator.Symbol.Name}' in component '{_snapshot.Descriptor.FullName}' must be initialized from an analyzable inline template, current-component RenderFragment member, or supported fragment factory.");
            }

            _localRenderFragmentCarriers[pendingDeclaration.Declarator.Symbol] = slotTemplate;
            return true;
        }

        private void CommitTemplateScopedDeclaration(
            IVariableDeclaratorOperation declarator,
            IOperation initializer)
        {
            ValidateTemplateScopedInitializer(declarator, initializer);
            _accessibleTemplateLocals.Add(declarator.Symbol);
            AddNode(new RazorVueLocalDeclarationNode(
                declarator.Symbol,
                initializer,
                CreateOrigins(declarator, RazorVueOriginKind.Template)));
        }

        private void ValidateTemplateScopedInitializer(
            IVariableDeclaratorOperation declarator,
            IOperation initializer)
        {
            foreach (var operation in EnumerateSelfAndDescendants(initializer))
            {
                switch (Unwrap(operation))
                {
                    case null:
                        continue;
                    case ILocalReferenceOperation localReference when !_accessibleTemplateLocals.Contains(localReference.Local):
                        throw CreateStructuralIssue(
                            declarator,
                            $"RazorVue template-scoped local '{declarator.Symbol.Name}' in component '{_snapshot.Descriptor.FullName}' cannot capture unsupported local '{localReference.Local.Name}'. Only previously declared template locals and active slot/loop parameters are allowed.");
                    case IParameterReferenceOperation parameterReference when
                        !_builderParameters.Contains(parameterReference.Parameter) &&
                        !_accessibleTemplateParameters.Contains(parameterReference.Parameter) &&
                        !IsAnonymousFunctionParameter(parameterReference.Parameter):
                        throw CreateStructuralIssue(
                            declarator,
                            $"RazorVue template-scoped local '{declarator.Symbol.Name}' in component '{_snapshot.Descriptor.FullName}' cannot capture unsupported parameter '{parameterReference.Parameter.Name}'.");
                    case IAnonymousFunctionOperation:
                    case IDelegateCreationOperation:
                    case IAssignmentOperation:
                    case IIncrementOrDecrementOperation:
                        throw CreateStructuralIssue(
                            declarator,
                            $"RazorVue template-scoped local '{declarator.Symbol.Name}' in component '{_snapshot.Descriptor.FullName}' must be an immutable value/cache initializer without nested write or callable template state.");
                }
            }
        }

        private bool IsCurrentComponentRenderHelperCandidate(IMethodSymbol method, IOperation? instance)
        {
            if (!ContainsRenderTreeBuilderParameter(method))
                return false;

            if (method.MethodKind == MethodKind.LocalFunction)
                return instance is null && _localFunctionDeclarations.ContainsKey(method.OriginalDefinition);

            return IsCurrentComponentMethod(method, instance);
        }

        private void PrimeLocalFunctionDeclarations(IEnumerable<IOperation> operations)
        {
            foreach (var operation in operations)
            {
                if (Unwrap(operation) is ILocalFunctionOperation localFunction)
                    RegisterLocalFunctionDeclaration(localFunction);
            }
        }

        private void RegisterLocalFunctionDeclaration(ILocalFunctionOperation localFunction)
        {
            var method = localFunction.Symbol.OriginalDefinition;
            if (_localFunctionDeclarations.ContainsKey(method))
                return;

            _localFunctionDeclarations.Add(method, localFunction);
            _localFunctionDeclarationOrder.Add(localFunction);
        }

        private bool TryGetSupportedRenderHelperSignature(
            IMethodSymbol method,
            out IParameterSymbol builderParameter,
            out ImmutableArray<IParameterSymbol> extraParameters,
            out string failureMessage)
        {
            builderParameter = default!;
            extraParameters = ImmutableArray<IParameterSymbol>.Empty;
            failureMessage = string.Empty;
            var helperDisplayName = method.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat);

            if (method.IsAsync)
            {
                failureMessage =
                    $"BuildRenderTree helper method '{helperDisplayName}' must be synchronous in component '{_snapshot.Descriptor.FullName}'. Async render helpers are not supported because RazorVue cannot replay asynchronous continuation against caller-owned open frame semantics.";
                return false;
            }

            if (!method.ReturnsVoid)
            {
                failureMessage =
                    $"BuildRenderTree helper method '{helperDisplayName}' must return void in component '{_snapshot.Descriptor.FullName}'.";
                return false;
            }

            var builderParameters = method.Parameters
                .Where(static parameter => IsRenderTreeBuilderType(parameter.Type))
                .ToArray();

            if (builderParameters.Length != 1)
            {
                failureMessage =
                    $"BuildRenderTree helper method '{helperDisplayName}' must declare exactly one RenderTreeBuilder parameter in component '{_snapshot.Descriptor.FullName}'.";
                return false;
            }

            var selectedBuilderParameter = builderParameters[0];
            if (selectedBuilderParameter.RefKind != RefKind.None)
            {
                var modifier = GetRefKindModifier(selectedBuilderParameter.RefKind);
                failureMessage =
                    $"BuildRenderTree helper method '{helperDisplayName}' cannot declare '{modifier}' RenderTreeBuilder parameter '{selectedBuilderParameter.Name}' in component '{_snapshot.Descriptor.FullName}'. RenderTreeBuilder helper parameters must be ordinary by-value parameters.";
                return false;
            }

            foreach (var parameter in method.Parameters)
            {
                if (parameter.RefKind == RefKind.None)
                    continue;

                if (parameter.RefKind is RefKind.In or RefKind.Ref &&
                    !SymbolEqualityComparer.Default.Equals(parameter, selectedBuilderParameter))
                {
                    continue;
                }

                if (parameter.RefKind != RefKind.None)
                {
                    var modifier = GetRefKindModifier(parameter.RefKind);
                    failureMessage =
                        $"BuildRenderTree helper method '{helperDisplayName}' cannot declare '{modifier}' parameter '{parameter.Name}' in component '{_snapshot.Descriptor.FullName}'. Only ordinary by-value parameters, read-only 'in' value parameters, and read-only 'ref' value parameters with no writeback are supported.";
                    return false;
                }
            }

            builderParameter = RazorVueMethodSymbolNormalizer.NormalizeParameter(method, selectedBuilderParameter);
            extraParameters = method.Parameters
                .Where(parameter => !SymbolEqualityComparer.Default.Equals(parameter, selectedBuilderParameter))
                .Select(parameter => RazorVueMethodSymbolNormalizer.NormalizeParameter(method, parameter))
                .ToImmutableArray();
            return true;
        }

        private bool TryGetRenderHelperInvocationBindings(
            IInvocationOperation invocation,
            IParameterSymbol builderParameter,
            bool requireCallerOwnedReplaySafeBinding,
            out IArgumentOperation builderArgument,
            out ImmutableArray<RenderHelperValueBinding> extraArgumentBindings,
            out string failureMessage)
        {
            builderArgument = default!;
            extraArgumentBindings = ImmutableArray<RenderHelperValueBinding>.Empty;
            failureMessage = string.Empty;

            if (requireCallerOwnedReplaySafeBinding &&
                invocation.Arguments.Length != invocation.TargetMethod.Parameters.Length)
            {
                failureMessage =
                    $"BuildRenderTree helper method '{GetBuilderCallDisplayName(invocation)}' must be invoked with arguments matching its declared signature in component '{_snapshot.Descriptor.FullName}'. Omitted optional parameters and argument reshaping are not supported.";
                return false;
            }

            var boundParameters = new HashSet<IParameterSymbol>(SymbolEqualityComparer.Default);
            var extraBindingsBuilder = ImmutableArray.CreateBuilder<RenderHelperValueBinding>(Math.Max(invocation.Arguments.Length - 1, 0));
            IArgumentOperation? matchedBuilderArgument = null;
            var previousExplicitArgumentSourceStart = -1;
            var previousArgumentParameterOrdinal = -1;
            foreach (var argument in invocation.Arguments)
            {
                if (argument.Parameter is not { } rawParameter)
                {
                    failureMessage =
                        $"BuildRenderTree helper method '{GetBuilderCallDisplayName(invocation)}' must use direct one-to-one argument binding in component '{_snapshot.Descriptor.FullName}'.";
                    return false;
                }

                var parameter = RazorVueMethodSymbolNormalizer.NormalizeParameter(invocation.TargetMethod, rawParameter);
                if (requireCallerOwnedReplaySafeBinding &&
                    !IsSupportedCallerOwnedReplayRenderHelperArgumentKind(
                        invocation,
                        argument,
                        parameter,
                        ref previousExplicitArgumentSourceStart,
                        ref previousArgumentParameterOrdinal,
                        out failureMessage))
                {
                    return false;
                }

                if (!boundParameters.Add(parameter))
                {
                    failureMessage =
                        $"BuildRenderTree helper method '{GetBuilderCallDisplayName(invocation)}' must use direct one-to-one argument binding in component '{_snapshot.Descriptor.FullName}'.";
                    return false;
                }

                if (SymbolEqualityComparer.Default.Equals(parameter, builderParameter))
                {
                    matchedBuilderArgument = argument;
                    continue;
                }

                var initializer = Unwrap(argument.Value);
                if (initializer is null)
                {
                    failureMessage =
                        $"BuildRenderTree helper method '{GetBuilderCallDisplayName(invocation)}' contains an unsupported argument value for parameter '{parameter.Name}' in component '{_snapshot.Descriptor.FullName}'.";
                    return false;
                }

                extraBindingsBuilder.Add(new RenderHelperValueBinding(parameter, initializer));
            }

            if (matchedBuilderArgument is null ||
                (requireCallerOwnedReplaySafeBinding && boundParameters.Count != invocation.TargetMethod.Parameters.Length))
            {
                failureMessage =
                    $"BuildRenderTree helper method '{GetBuilderCallDisplayName(invocation)}' must be invoked with arguments matching its declared signature in component '{_snapshot.Descriptor.FullName}'.";
                return false;
            }

            builderArgument = matchedBuilderArgument;
            extraArgumentBindings = extraBindingsBuilder.ToImmutable();
            return true;
        }

        private bool IsSupportedCallerOwnedReplayRenderHelperArgumentKind(
            IInvocationOperation invocation,
            IArgumentOperation argument,
            IParameterSymbol normalizedParameter,
            ref int previousExplicitArgumentSourceStart,
            ref int previousArgumentParameterOrdinal,
            out string failureMessage)
        {
            failureMessage = string.Empty;

            switch (argument.ArgumentKind)
            {
                case ArgumentKind.Explicit:
                    if (!IsExplicitArgumentInSourceOrder(argument, ref previousExplicitArgumentSourceStart) ||
                        !IsArgumentInParameterSourceOrder(argument, ref previousArgumentParameterOrdinal))
                    {
                        failureMessage =
                            $"BuildRenderTree helper method '{GetBuilderCallDisplayName(invocation)}' must evaluate explicit arguments in source order for parameter '{normalizedParameter.Name}' in component '{_snapshot.Descriptor.FullName}'. Named argument reshaping that can change captured value evaluation order is not supported.";
                        return false;
                    }

                    break;
                case ArgumentKind.ParamArray:
                    if (!IsArgumentInParameterSourceOrder(argument, ref previousArgumentParameterOrdinal))
                    {
                        failureMessage =
                            $"BuildRenderTree helper method '{GetBuilderCallDisplayName(invocation)}' must evaluate params arguments in source order for parameter '{normalizedParameter.Name}' in component '{_snapshot.Descriptor.FullName}'. Params argument reshaping that can change captured value evaluation order is not supported.";
                        return false;
                    }

                    break;
                case ArgumentKind.DefaultValue:
                    failureMessage =
                        $"BuildRenderTree helper method '{GetBuilderCallDisplayName(invocation)}' cannot omit optional parameter '{normalizedParameter.Name}' in component '{_snapshot.Descriptor.FullName}'. Omitted optional parameters are compiler-synthesized default arguments and are not supported for render helper captured value replay.";
                    return false;
                default:
                    failureMessage =
                        $"BuildRenderTree helper method '{GetBuilderCallDisplayName(invocation)}' cannot bind {argument.ArgumentKind} argument for parameter '{normalizedParameter.Name}' in component '{_snapshot.Descriptor.FullName}'. Render helper parameters only support explicit one-to-one arguments and controlled params array expansion.";
                    return false;
            }

            var expectedRefKind = normalizedParameter.RefKind;
            if (expectedRefKind is RefKind.None or RefKind.In or RefKind.Ref)
                return true;

            var modifier = GetRefKindModifier(expectedRefKind);
            failureMessage =
                $"BuildRenderTree helper method '{GetBuilderCallDisplayName(invocation)}' cannot bind '{modifier}' argument for parameter '{normalizedParameter.Name}' in component '{_snapshot.Descriptor.FullName}'. Render helper parameters only support by-value binding, read-only 'in' value binding, and read-only 'ref' value binding with no writeback.";
            return false;
        }

        private static bool IsExplicitArgumentInSourceOrder(
            IArgumentOperation argument,
            ref int previousExplicitArgumentSourceStart)
        {
            var sourceStart = argument.Syntax.SpanStart;
            if (sourceStart < previousExplicitArgumentSourceStart)
                return false;

            previousExplicitArgumentSourceStart = sourceStart;
            return true;
        }

        private static bool IsArgumentInParameterSourceOrder(
            IArgumentOperation argument,
            ref int previousArgumentParameterOrdinal)
        {
            if (argument.Parameter is not { } parameter)
                return false;

            if (parameter.Ordinal < previousArgumentParameterOrdinal)
                return false;

            previousArgumentParameterOrdinal = parameter.Ordinal;
            return true;
        }

        private void ThrowIfReadOnlyByRefParameterEscapes(IOperation operation)
            => ThrowIfReadOnlyByRefParameterEscapes(
                operation,
                readOnlyByRefParameters: null,
                "BuildRenderTree helper parameter",
                "render helper parameters");

        private void ThrowIfReadOnlyRefParameterWritesOrEscapes(
            ImmutableArray<IOperation> operations,
            ImmutableHashSet<IParameterSymbol> readOnlyRefParameters,
            IInvocationOperation invocation)
        {
            if (readOnlyRefParameters.Count == 0)
                return;

            foreach (var operation in operations)
            {
                foreach (var current in EnumerateSelfAndDescendants(operation))
                {
                    var unwrapped = Unwrap(current);
                    if (unwrapped is null)
                        continue;

                    if (TryGetWrittenReadOnlyRefParameter(unwrapped, readOnlyRefParameters, out var writtenParameter))
                    {
                        throw CreateStructuralIssue(
                            current,
                            $"BuildRenderTree helper method '{GetBuilderCallDisplayName(invocation)}' declares 'ref' parameter '{writtenParameter.Name}' in component '{_snapshot.Descriptor.FullName}', but RazorVue can only lower 'ref' render helper parameters as read-only captured values. The helper body must not assign, increment, or otherwise require caller writeback.");
                    }

                    if (unwrapped is IArgumentOperation argument &&
                        argument.Parameter?.RefKind is RefKind.Ref or RefKind.Out or RefKind.In &&
                        TryGetReadOnlyRefParameterReference(argument.Value, readOnlyRefParameters, out var escapedParameter))
                    {
                        throw CreateStructuralIssue(
                            argument,
                            $"BuildRenderTree helper method '{GetBuilderCallDisplayName(invocation)}' declares 'ref' parameter '{escapedParameter.Name}' in component '{_snapshot.Descriptor.FullName}', but RazorVue can only lower 'ref' render helper parameters as read-only captured values. The parameter cannot be forwarded through a by-reference invocation because writeback and by-reference escape semantics are not supported.");
                    }
                }
            }
        }

        private void ThrowIfReadOnlyByRefParameterEscapes(
            IOperation operation,
            ImmutableHashSet<IParameterSymbol>? readOnlyByRefParameters,
            string parameterContext,
            string usageContext)
        {
            foreach (var current in EnumerateSelfAndDescendants(operation))
            {
                if (Unwrap(current) is not IArgumentOperation argument ||
                    argument.Parameter?.RefKind is not (RefKind.Ref or RefKind.Out or RefKind.In))
                {
                    continue;
                }

                if (!TryGetReadOnlyByRefParameterReference(
                        argument.Value,
                        readOnlyByRefParameters,
                        out var escapedParameter))
                {
                    continue;
                }

                throw CreateStructuralIssue(
                    argument,
                    $"{parameterContext} '{escapedParameter.Name}' in component '{_snapshot.Descriptor.FullName}' is a read-only 'in' value parameter and cannot be forwarded through a by-reference invocation. RazorVue only supports reading 'in' {usageContext} as values.");
            }
        }

        private void ThrowIfRenderFragmentFactoryReadOnlyByRefParameterEscapes(IInvocationOperation invocation)
        {
            var canonicalMethod = RazorVueMethodSymbolNormalizer.GetCanonicalMethod(invocation.TargetMethod);
            var readOnlyByRefParameters = canonicalMethod.Parameters
                .Where(static parameter => parameter.RefKind == RefKind.In)
                .ToImmutableHashSet<IParameterSymbol>(SymbolEqualityComparer.Default);
            if (readOnlyByRefParameters.Count == 0)
                return;

            foreach (var syntaxReference in canonicalMethod.DeclaringSyntaxReferences)
            {
                var syntax = syntaxReference.GetSyntax();
                var semanticModel = _compilation.GetSemanticModel(syntax.SyntaxTree);
                var operation = syntax switch
                {
                    MethodDeclarationSyntax methodDeclaration => methodDeclaration.Body is not null
                        ? semanticModel.GetOperation(methodDeclaration.Body)
                        : methodDeclaration.ExpressionBody is not null
                            ? semanticModel.GetOperation(methodDeclaration.ExpressionBody.Expression)
                            : null,
                    LocalFunctionStatementSyntax localFunction => localFunction.Body is not null
                        ? semanticModel.GetOperation(localFunction.Body)
                        : localFunction.ExpressionBody is not null
                            ? semanticModel.GetOperation(localFunction.ExpressionBody.Expression)
                            : null,
                    _ => null
                };

                if (operation is not null)
                {
                    ThrowIfReadOnlyByRefParameterEscapes(
                        operation,
                        readOnlyByRefParameters,
                        "BuildRenderTree fragment factory parameter",
                        "fragment factory parameters");
                }
            }
        }

        private static string GetRefKindModifier(RefKind refKind)
            => refKind switch
            {
                RefKind.Ref => "ref",
                RefKind.Out => "out",
                RefKind.In => "in",
                _ => refKind.ToString().ToLowerInvariant()
            };

        private static bool TryGetReadOnlyByRefParameterReference(
            IOperation? operation,
            ImmutableHashSet<IParameterSymbol>? readOnlyByRefParameters,
            out IParameterSymbol parameter)
        {
            parameter = default!;
            if (operation is null)
                return false;

            foreach (var current in EnumerateSelfAndDescendants(operation))
            {
                if (Unwrap(current) is IParameterReferenceOperation parameterReference &&
                    parameterReference.Parameter.RefKind == RefKind.In &&
                    (readOnlyByRefParameters is null ||
                     readOnlyByRefParameters.Contains(parameterReference.Parameter)))
                {
                    parameter = parameterReference.Parameter;
                    return true;
                }
            }

            return false;
        }

        private static bool TryGetReadOnlyRefParameterReference(
            IOperation? operation,
            ImmutableHashSet<IParameterSymbol> readOnlyRefParameters,
            out IParameterSymbol parameter)
        {
            parameter = default!;
            if (operation is null)
                return false;

            foreach (var current in EnumerateSelfAndDescendants(operation))
            {
                if (Unwrap(current) is IParameterReferenceOperation parameterReference &&
                    parameterReference.Parameter.RefKind == RefKind.Ref &&
                    readOnlyRefParameters.Contains(parameterReference.Parameter))
                {
                    parameter = parameterReference.Parameter;
                    return true;
                }
            }

            return false;
        }

        private static bool TryGetWrittenReadOnlyRefParameter(
            IOperation operation,
            ImmutableHashSet<IParameterSymbol> readOnlyRefParameters,
            out IParameterSymbol parameter)
        {
            parameter = default!;
            switch (operation)
            {
                case IAssignmentOperation assignment:
                    return TryGetReadOnlyRefParameterReference(
                        assignment.Target,
                        readOnlyRefParameters,
                        out parameter);
                case IIncrementOrDecrementOperation incrementOrDecrement:
                    return TryGetReadOnlyRefParameterReference(
                        incrementOrDecrement.Target,
                        readOnlyRefParameters,
                        out parameter);
            }

            return false;
        }

        private void ParseRenderHelperBody(IInvocationOperation invocation, IParameterSymbol builderParameter)
        {
            ExecuteRenderHelperBody(
                invocation,
                operations => ExecuteWithBuilderScope(
                    ImmutableHashSet.Create<IParameterSymbol>(SymbolEqualityComparer.Default, builderParameter),
                    () =>
                    {
                        foreach (var operation in operations)
                            ParseOperation(operation);
                    }));
        }

        private RazorVueRenderFragment ParseRenderHelperBodyAsScopedFragment(
            IInvocationOperation invocation,
            IParameterSymbol builderParameter,
            ImmutableArray<IParameterSymbol> extraParameters,
            ImmutableArray<RenderHelperValueBinding> extraArgumentBindings)
        {
            var fragment = ExecuteRenderHelperBody(
                invocation,
                operations =>
                {
                    var accessibleTemplateParameters = new HashSet<IParameterSymbol>(_accessibleTemplateParameters, SymbolEqualityComparer.Default);
                    foreach (var parameter in extraParameters)
                        accessibleTemplateParameters.Add(parameter);

                    try
                    {
                        return new Parser(
                            _snapshot,
                            _compilation,
                            _symbols,
                            ImmutableHashSet.Create<IParameterSymbol>(SymbolEqualityComparer.Default, builderParameter),
                            activeRenderHelperMethods: _activeRenderHelperMethods,
                            activeRenderFragmentMembers: _activeRenderFragmentMembers,
                            activeRenderFragmentFactories: _activeRenderFragmentFactories,
                            localRenderFragmentCarriers: GetLocalRenderFragmentCarrierSnapshot(),
                            memberRenderFragmentCarriers: GetMemberRenderFragmentCarrierSnapshot(),
                            factoryRenderFragmentCarriers: GetFactoryRenderFragmentCarrierSnapshot(),
                            localFunctionDeclarations: GetLocalFunctionDeclarationSnapshot(),
                            accessibleTemplateLocals: _accessibleTemplateLocals,
                            accessibleTemplateParameters: accessibleTemplateParameters,
                            allowTemplateScopedLocals: true)
                            .Parse(operations);
                    }
                    catch (RazorVueCompilationIssueException exception)
                    {
                        var message =
                            $"BuildRenderTree helper method '{GetBuilderCallDisplayName(invocation)}' declares extra value parameters and therefore must produce a self-contained fragment in component '{_snapshot.Descriptor.FullName}'. Inner helper body failure: {exception.Issue.Message}";
                        var origins = exception.Origin is { } origin
                            ? ImmutableArray.Create(origin)
                            : CreateOrigins(invocation, RazorVueOriginKind.Template);
                        throw CreateStructuralIssue(origins, message);
                    }
                });

            return WrapCapturedTemplateScopes(
                fragment,
                extraArgumentBindings,
                CreateOrigins(invocation, RazorVueOriginKind.Template));
        }

        private void ParseRenderHelperBodyWithCallerOwnedOpenNodeMutation(
            IInvocationOperation invocation,
            IParameterSymbol builderParameter,
            ImmutableArray<IParameterSymbol> extraParameters,
            ImmutableArray<RenderHelperValueBinding> extraArgumentBindings,
            OpenNodeBuilder originalOpenNode)
        {
            try
            {
                ExecuteRenderHelperBody(
                    invocation,
                    operations =>
                    {
                        var helperParser = CreateRenderHelperBodyParser(builderParameter);
                        var syntheticOpenNode = originalOpenNode.CreateEmptyClone();
                        helperParser.PrimeSourceStableLocalRenderFragmentInitializers(operations);
                        helperParser.PrimeSourceStableLocalStaticMarkupInitializers(operations);
                        helperParser.PrimeLocalFunctionDeclarations(operations);
                        helperParser._allowCallerOwnedOpenNodeConditionalReplay = true;
                        helperParser._openFrames.Push(syntheticOpenNode);

                        var helperAccessibleTemplateParameters = new HashSet<IParameterSymbol>(_accessibleTemplateParameters, SymbolEqualityComparer.Default);
                        foreach (var parameter in extraParameters)
                            helperAccessibleTemplateParameters.Add(parameter);

                        helperParser.ThrowIfCallerOwnedRenderHelperBodyContainsRecursiveInvocation(
                            operations,
                            invocation);

                        helperParser.ExecuteWithCapturedBindings(
                            extraArgumentBindings,
                            () => helperParser.ExecuteWithAccessibleTemplateParameters(
                                helperAccessibleTemplateParameters,
                                () => helperParser.ExecuteWithBuilderScope(
                                    ImmutableHashSet.Create<IParameterSymbol>(SymbolEqualityComparer.Default, builderParameter),
                                    () =>
                                    {
                                        helperParser.ParseCallerOwnedOpenNodeMutationOperations(operations);
                                    })));

                        helperParser.EnsureNoPendingImmediateAssignmentDeclarations();
                        helperParser.ValidateCallerOwnedOpenNodeMutationPostState(
                            invocation,
                            originalFrameDepth: 1,
                            syntheticOpenNode);

                        MergeCallerOwnedOpenNodeDelta(
                            originalOpenNode,
                            syntheticOpenNode,
                            extraArgumentBindings,
                            CreateOrigins(invocation, RazorVueOriginKind.Template));
                    });
            }
            catch (RazorVueCompilationIssueException exception)
            {
                var message =
                    $"BuildRenderTree helper method '{GetBuilderCallDisplayName(invocation)}' depends on caller-owned open frame semantics in component '{_snapshot.Descriptor.FullName}'. Inner helper body failure: {exception.Issue.Message}";
                var origins = exception.Origin is { } origin
                    ? ImmutableArray.Create(origin)
                    : CreateOrigins(invocation, RazorVueOriginKind.Template);
                throw CreateStructuralIssue(origins, message);
            }
        }

        private void ExecuteRenderHelperBody(
            IInvocationOperation invocation,
            Action<ImmutableArray<IOperation>> action)
            => ExecuteRenderHelperBody<object?>(
                invocation,
                operations =>
                {
                    action(operations);
                    return null;
                });

        private T ExecuteRenderHelperBody<T>(
            IInvocationOperation invocation,
            Func<ImmutableArray<IOperation>, T> action)
        {
            var canonicalMethod = RazorVueMethodSymbolNormalizer.GetCanonicalMethod(invocation.TargetMethod);
            if (!_activeRenderHelperMethods.Add(canonicalMethod))
            {
                throw CreateUnsupportedBuilderCall(
                    invocation,
                    $"BuildRenderTree helper method '{GetBuilderCallDisplayName(invocation)}' is recursive; RazorVue does not support recursive render helpers in component '{_snapshot.Descriptor.FullName}'.");
            }

            try
            {
                var operations = GetRenderHelperOperations(invocation);
                ThrowIfRuntimeSensitiveGenericRenderHelperTypeParameterUsage(operations, invocation);
                ThrowIfReadOnlyRefParameterWritesOrEscapes(
                    operations,
                    GetReadOnlyRefParameters(invocation.TargetMethod),
                    invocation);
                return action(operations);
            }
            finally
            {
                _activeRenderHelperMethods.Remove(canonicalMethod);
            }
        }

        private void ThrowIfRuntimeSensitiveGenericRenderHelperTypeParameterUsage(
            ImmutableArray<IOperation> operations,
            IInvocationOperation invocation)
        {
            var typeParameters = RazorVueMethodSymbolNormalizer
                .GetCanonicalMethod(invocation.TargetMethod)
                .TypeParameters
                .ToImmutableHashSet<ITypeParameterSymbol>(SymbolEqualityComparer.Default);
            if (typeParameters.Count == 0)
                return;

            var localFunctionDeclarations = new Dictionary<IMethodSymbol, ILocalFunctionOperation>(SymbolEqualityComparer.Default);
            var anonymousFunctionCarriers = new Dictionary<ILocalSymbol, List<IAnonymousFunctionOperation>>(SymbolEqualityComparer.Default);
            var visitedLocalFunctions = new HashSet<IMethodSymbol>(SymbolEqualityComparer.Default);
            var visitedAnonymousFunctions = new HashSet<IAnonymousFunctionOperation>();
            var pendingScopes = new Queue<ImmutableArray<IOperation>>();
            pendingScopes.Enqueue(operations);

            while (pendingScopes.Count > 0)
            {
                var scopeOperations = pendingScopes.Dequeue();
                RegisterLocalFunctionDeclarations(scopeOperations, localFunctionDeclarations);
                RegisterAnonymousFunctionCarriers(scopeOperations, anonymousFunctionCarriers);

                foreach (var operation in scopeOperations)
                {
                    foreach (var current in RazorVueOperationLocalCollector.EnumerateSelfAndDescendants(
                                 operation,
                                 includeLocalFunctionBodies: false,
                                 includeAnonymousFunctionBodies: false))
                    {
                        var unwrapped = Unwrap(current) ?? current;
                        switch (unwrapped)
                        {
                            case ITypeOfOperation typeOf
                                when IsRenderHelperTypeParameter(typeOf.TypeOperand, typeParameters, out var typeOfTypeParameter):
                                ThrowRuntimeSensitiveGenericRenderHelperTypeParameterUsage(
                                    typeOf,
                                    invocation,
                                    typeOfTypeParameter,
                                    "typeof(T)");
                                break;
                            case IDefaultValueOperation defaultValue
                                when IsRenderHelperTypeParameter(defaultValue.Type, typeParameters, out var defaultTypeParameter):
                                ThrowRuntimeSensitiveGenericRenderHelperTypeParameterUsage(
                                    defaultValue,
                                    invocation,
                                    defaultTypeParameter,
                                    "default(T)");
                                break;
                            case ITypeParameterObjectCreationOperation objectCreation:
                                if (IsRenderHelperTypeParameter(objectCreation.Type, typeParameters, out var objectCreationTypeParameter))
                                {
                                    ThrowRuntimeSensitiveGenericRenderHelperTypeParameterUsage(
                                        objectCreation,
                                        invocation,
                                        objectCreationTypeParameter,
                                        "new T()");
                                }

                                throw CreateStructuralIssue(
                                    objectCreation,
                                    $"BuildRenderTree helper method '{GetBuilderCallDisplayName(invocation)}' uses runtime generic type-parameter semantics '{GetOperationDisplay(objectCreation)}' in component '{_snapshot.Descriptor.FullName}'. Generic render helpers only support erased value-parameter usage; 'new T()' requires runtime type-parameter metadata and constructor metadata and is not supported.");
                            case IIsTypeOperation isType
                                when IsRenderHelperTypeParameter(isType.TypeOperand, typeParameters, out var isTypeTypeParameter):
                                ThrowRuntimeSensitiveGenericRenderHelperTypeParameterUsage(
                                    isType,
                                    invocation,
                                    isTypeTypeParameter,
                                    "is T");
                                break;
                            case ITypePatternOperation typePattern
                                when IsRenderHelperTypeParameter(typePattern.MatchedType, typeParameters, out var typePatternTypeParameter):
                                ThrowRuntimeSensitiveGenericRenderHelperTypeParameterUsage(
                                    typePattern,
                                    invocation,
                                    typePatternTypeParameter,
                                    "type pattern T");
                                break;
                            case IDeclarationPatternOperation declarationPattern
                                when IsRenderHelperTypeParameter(declarationPattern.MatchedType, typeParameters, out var declarationPatternTypeParameter):
                                ThrowRuntimeSensitiveGenericRenderHelperTypeParameterUsage(
                                    declarationPattern,
                                    invocation,
                                    declarationPatternTypeParameter,
                                    "type pattern T");
                                break;
                            case IInvocationOperation { TargetMethod.MethodKind: MethodKind.LocalFunction } localInvocation
                                when TryGetReachableLocalFunctionDeclaration(localInvocation.TargetMethod, localFunctionDeclarations, out var localFunction) &&
                                     visitedLocalFunctions.Add(localFunction.Symbol.OriginalDefinition):
                                var localFunctionOperations = GetLocalFunctionBodyOperations(localFunction);
                                if (!localFunctionOperations.IsDefaultOrEmpty)
                                    pendingScopes.Enqueue(localFunctionOperations);
                                break;
                            case IInvocationOperation anonymousInvocation
                                when TryGetInvokedAnonymousFunctions(anonymousInvocation, anonymousFunctionCarriers, out var anonymousFunctions):
                                foreach (var anonymousFunction in anonymousFunctions)
                                {
                                    if (!visitedAnonymousFunctions.Add(anonymousFunction))
                                        continue;

                                    var anonymousFunctionOperations = GetAnonymousFunctionBodyOperations(anonymousFunction);
                                    if (!anonymousFunctionOperations.IsDefaultOrEmpty)
                                        pendingScopes.Enqueue(anonymousFunctionOperations);
                                }

                                break;
                        }
                    }
                }
            }
        }

        private static void RegisterLocalFunctionDeclarations(
            ImmutableArray<IOperation> operations,
            Dictionary<IMethodSymbol, ILocalFunctionOperation> declarations)
        {
            foreach (var operation in operations)
            {
                foreach (var current in RazorVueOperationLocalCollector.EnumerateSelfAndDescendants(operation))
                {
                    if (Unwrap(current) is not ILocalFunctionOperation localFunction)
                        continue;

                    var method = localFunction.Symbol.OriginalDefinition;
                    if (!declarations.ContainsKey(method))
                        declarations.Add(method, localFunction);
                }
            }
        }

        private static void RegisterAnonymousFunctionCarriers(
            ImmutableArray<IOperation> operations,
            Dictionary<ILocalSymbol, List<IAnonymousFunctionOperation>> carriers)
        {
            foreach (var operation in operations)
            {
                foreach (var current in RazorVueOperationLocalCollector.EnumerateSelfAndDescendants(
                             operation,
                             includeLocalFunctionBodies: false,
                             includeAnonymousFunctionBodies: false))
                {
                    switch (Unwrap(current))
                    {
                        case IVariableDeclaratorOperation { Initializer.Value: { } initializer } declarator
                            when TryGetAnonymousFunction(initializer, out var anonymousFunction):
                            AddAnonymousFunctionCarrier(carriers, declarator.Symbol, anonymousFunction);
                            break;
                        case ISimpleAssignmentOperation assignment
                            when Unwrap(assignment.Target) is ILocalReferenceOperation localReference &&
                                 TryGetAnonymousFunction(assignment.Value, out var anonymousFunction):
                            AddAnonymousFunctionCarrier(carriers, localReference.Local, anonymousFunction);
                            break;
                    }
                }
            }
        }

        private static void AddAnonymousFunctionCarrier(
            Dictionary<ILocalSymbol, List<IAnonymousFunctionOperation>> carriers,
            ILocalSymbol local,
            IAnonymousFunctionOperation anonymousFunction)
        {
            if (!carriers.TryGetValue(local, out var functions))
            {
                functions = [];
                carriers.Add(local, functions);
            }

            if (!functions.Contains(anonymousFunction))
                functions.Add(anonymousFunction);
        }

        private static bool TryGetInvokedAnonymousFunctions(
            IInvocationOperation invocation,
            Dictionary<ILocalSymbol, List<IAnonymousFunctionOperation>> carriers,
            out ImmutableArray<IAnonymousFunctionOperation> anonymousFunctions)
        {
            var builder = ImmutableArray.CreateBuilder<IAnonymousFunctionOperation>();
            CollectInvokedAnonymousFunctions(invocation.Instance, carriers, builder);

            anonymousFunctions = builder.ToImmutable();
            return anonymousFunctions.Length > 0;
        }

        private static void CollectInvokedAnonymousFunctions(
            IOperation? operation,
            Dictionary<ILocalSymbol, List<IAnonymousFunctionOperation>> carriers,
            ImmutableArray<IAnonymousFunctionOperation>.Builder builder)
        {
            var current = UnwrapDelegateCarrier(operation);
            switch (current)
            {
                case IOperation candidate when TryGetAnonymousFunction(candidate, out var anonymousFunction):
                    builder.Add(anonymousFunction);
                    break;
                case IAnonymousFunctionOperation anonymousFunction:
                    builder.Add(anonymousFunction);
                    break;
                case ILocalReferenceOperation localReference
                    when carriers.TryGetValue(localReference.Local, out var anonymousFunctions):
                    builder.AddRange(anonymousFunctions);
                    break;
            }
        }

        private bool TryGetReachableLocalFunctionDeclaration(
            IMethodSymbol targetMethod,
            Dictionary<IMethodSymbol, ILocalFunctionOperation> declarations,
            out ILocalFunctionOperation localFunction)
        {
            if (declarations.TryGetValue(targetMethod.OriginalDefinition, out localFunction!))
                return true;

            foreach (var syntaxReference in targetMethod.OriginalDefinition.DeclaringSyntaxReferences)
            {
                if (syntaxReference.GetSyntax() is not LocalFunctionStatementSyntax localFunctionSyntax)
                    continue;

                var semanticModel = _compilation.GetSemanticModel(localFunctionSyntax.SyntaxTree);
                if (semanticModel.GetOperation(localFunctionSyntax) is not ILocalFunctionOperation operation)
                    continue;

                localFunction = operation;
                if (!declarations.ContainsKey(operation.Symbol.OriginalDefinition))
                    declarations.Add(operation.Symbol.OriginalDefinition, operation);
                return true;
            }

            localFunction = default!;
            return false;
        }

        private ImmutableArray<IOperation> GetLocalFunctionBodyOperations(ILocalFunctionOperation localFunction)
        {
            if (localFunction.Body is not null)
                return localFunction.Body.Operations;

            foreach (var syntaxReference in localFunction.Symbol.OriginalDefinition.DeclaringSyntaxReferences)
            {
                if (syntaxReference.GetSyntax() is not LocalFunctionStatementSyntax localFunctionSyntax)
                    continue;

                var semanticModel = _compilation.GetSemanticModel(localFunctionSyntax.SyntaxTree);
                var operation = localFunctionSyntax.Body is not null
                    ? semanticModel.GetOperation(localFunctionSyntax.Body)
                    : localFunctionSyntax.ExpressionBody is not null
                        ? semanticModel.GetOperation(localFunctionSyntax.ExpressionBody.Expression)
                        : null;

                if (operation is IBlockOperation block)
                    return block.Operations;

                if (TryGetOperationStatements(operation, out var statements))
                    return statements;
            }

            return ImmutableArray<IOperation>.Empty;
        }

        private static ImmutableArray<IOperation> GetAnonymousFunctionBodyOperations(
            IAnonymousFunctionOperation anonymousFunction)
        {
            var body = Unwrap(anonymousFunction.Body);
            if (body is null)
                return ImmutableArray<IOperation>.Empty;

            if (body is IBlockOperation block)
                return block.Operations;

            if (TryGetOperationStatements(body, out var statements))
                return statements;

            return ImmutableArray.Create(body);
        }

        private void ThrowRuntimeSensitiveGenericRenderHelperTypeParameterUsage(
            IOperation operation,
            IInvocationOperation invocation,
            ITypeParameterSymbol typeParameter,
            string usage)
            => throw CreateStructuralIssue(
                operation,
                $"BuildRenderTree helper method '{GetBuilderCallDisplayName(invocation)}' uses runtime generic type-parameter semantics '{GetOperationDisplay(operation)}' for type parameter '{typeParameter.Name}' in component '{_snapshot.Descriptor.FullName}'. Generic render helpers only support erased value-parameter usage; '{usage}' requires runtime type metadata and is not supported.");

        private static bool IsRenderHelperTypeParameter(
            ITypeSymbol? type,
            ImmutableHashSet<ITypeParameterSymbol> typeParameters,
            out ITypeParameterSymbol typeParameter)
        {
            typeParameter = default!;
            if (type is not ITypeParameterSymbol candidate)
                return false;

            foreach (var parameter in typeParameters)
            {
                if (SymbolEqualityComparer.Default.Equals(candidate, parameter) ||
                    SymbolEqualityComparer.Default.Equals(candidate.OriginalDefinition, parameter.OriginalDefinition))
                {
                    typeParameter = candidate;
                    return true;
                }
            }

            return false;
        }

        private void ThrowIfCallerOwnedRenderHelperBodyContainsRecursiveInvocation(
            ImmutableArray<IOperation> operations,
            IInvocationOperation rootInvocation)
        {
            foreach (var operation in operations)
            {
                foreach (var current in RazorVueOperationLocalCollector.EnumerateSelfAndDescendants(operation))
                {
                    if (Unwrap(current) is not IInvocationOperation invocation)
                        continue;

                    var canonicalMethod = RazorVueMethodSymbolNormalizer.GetCanonicalMethod(invocation.TargetMethod);
                    if (!_activeRenderHelperMethods.Contains(canonicalMethod))
                        continue;

                    throw CreateStructuralIssue(
                        invocation,
                        $"BuildRenderTree helper method '{GetBuilderCallDisplayName(rootInvocation)}' depends on caller-owned open frame mutation and is recursive in component '{_snapshot.Descriptor.FullName}'. Recursive caller-owned helper mutation cannot prove stable frame depth, active frame identity, or replay order.");
                }
            }
        }

        private ImmutableArray<IOperation> GetRenderHelperOperations(IInvocationOperation invocation)
        {
            foreach (var syntaxReference in RazorVueMethodSymbolNormalizer.GetCanonicalMethod(invocation.TargetMethod).DeclaringSyntaxReferences)
            {
                var syntax = syntaxReference.GetSyntax();
                var semanticModel = _compilation.GetSemanticModel(syntax.SyntaxTree);
                var operation = syntax switch
                {
                    MethodDeclarationSyntax methodDeclaration => methodDeclaration.Body is not null
                        ? semanticModel.GetOperation(methodDeclaration.Body)
                        : methodDeclaration.ExpressionBody is not null
                            ? semanticModel.GetOperation(methodDeclaration.ExpressionBody.Expression)
                            : null,
                    LocalFunctionStatementSyntax localFunction => localFunction.Body is not null
                        ? semanticModel.GetOperation(localFunction.Body)
                        : localFunction.ExpressionBody is not null
                            ? semanticModel.GetOperation(localFunction.ExpressionBody.Expression)
                            : null,
                    _ => null
                };

                if (operation is IBlockOperation block)
                    return block.Operations;

                if (TryGetOperationStatements(operation, out var statements))
                    return statements;
            }

            throw CreateUnsupportedBuilderCall(
                invocation,
                $"BuildRenderTree helper method '{GetBuilderCallDisplayName(invocation)}' must be source-authored with an analyzable body in component '{_snapshot.Descriptor.FullName}'.");
        }

        private static ImmutableHashSet<IParameterSymbol> GetReadOnlyRefParameters(IMethodSymbol method)
            => RazorVueMethodSymbolNormalizer.GetCanonicalMethod(method)
                .Parameters
                .Where(static parameter =>
                    parameter.RefKind == RefKind.Ref &&
                    !IsRenderTreeBuilderType(parameter.Type))
                .ToImmutableHashSet<IParameterSymbol>(SymbolEqualityComparer.Default);

        private void ExecuteWithBuilderScope(
            ImmutableHashSet<IParameterSymbol> builderParameters,
            Action action)
        {
            var previousBuilderParameters = _builderParameters;
            var previousBuilderAliases = _builderAliases.ToArray();

            _builderParameters = builderParameters;
            _builderAliases.Clear();

            try
            {
                action();
            }
            finally
            {
                _builderParameters = previousBuilderParameters;
                _builderAliases.Clear();
                foreach (var alias in previousBuilderAliases)
                    _builderAliases.Add(alias);
            }
        }

        private void ExecuteWithCapturedBindings(
            ImmutableArray<RenderHelperValueBinding> capturedBindings,
            Action action)
        {
            var previousCapturedBindings = _activeCapturedBindings;
            _activeCapturedBindings = capturedBindings;
            try
            {
                action();
            }
            finally
            {
                _activeCapturedBindings = previousCapturedBindings;
            }
        }

        private T ExecuteWithCapturedBindings<T>(
            ImmutableArray<RenderHelperValueBinding> capturedBindings,
            Func<T> action)
        {
            var previousCapturedBindings = _activeCapturedBindings;
            _activeCapturedBindings = capturedBindings;
            try
            {
                return action();
            }
            finally
            {
                _activeCapturedBindings = previousCapturedBindings;
            }
        }

        private void ExecuteWithAccessibleTemplateParameters(
            HashSet<IParameterSymbol> accessibleTemplateParameters,
            Action action)
        {
            var previousAccessibleTemplateParameters = _accessibleTemplateParameters.ToArray();
            _accessibleTemplateParameters.Clear();
            foreach (var parameter in accessibleTemplateParameters)
                _accessibleTemplateParameters.Add(parameter);

            try
            {
                action();
            }
            finally
            {
                _accessibleTemplateParameters.Clear();
                foreach (var parameter in previousAccessibleTemplateParameters)
                    _accessibleTemplateParameters.Add(parameter);
            }
        }

        public RazorVueRenderFragment ParseWithCapturedBindings(
            IEnumerable<IOperation> operations,
            ImmutableArray<RenderHelperValueBinding> capturedBindings)
        {
            return ExecuteWithCapturedBindings(
                capturedBindings,
                () => Parse(operations));
        }

        private void ValidateCallerOwnedOpenNodeMutationPostState(
            IOperation operation,
            int originalFrameDepth,
            OpenNodeBuilder originalOpenNode)
        {
            var operationDisplay = operation is IInvocationOperation invocation
                ? $"helper method '{GetBuilderCallDisplayName(invocation)}'"
                : $"operation '{GetOperationDisplay(operation)}'";

            if (_openFrames.Count != originalFrameDepth)
            {
                throw CreateStructuralIssue(
                    operation,
                    $"BuildRenderTree {operationDisplay} depends on caller-owned open frame mutation but leaves the frame stack unbalanced in component '{_snapshot.Descriptor.FullName}'. Caller-owned mutation must preserve the caller frame depth exactly.");
            }

            if (_openFrames.Peek() is not OpenNodeBuilder currentOpenNode ||
                !ReferenceEquals(currentOpenNode, originalOpenNode))
            {
                throw CreateStructuralIssue(
                    operation,
                    $"BuildRenderTree {operationDisplay} depends on caller-owned open frame mutation but changed the active caller-owned node in component '{_snapshot.Descriptor.FullName}'. Caller-owned mutation must return to the same open element/component frame.");
            }
        }

        private Parser CreateRenderHelperBodyParser(IParameterSymbol builderParameter)
            => new(
                _snapshot,
                _compilation,
                _symbols,
                ImmutableHashSet.Create<IParameterSymbol>(SymbolEqualityComparer.Default, builderParameter),
                activeRenderHelperMethods: _activeRenderHelperMethods,
                activeRenderFragmentMembers: _activeRenderFragmentMembers,
                activeRenderFragmentFactories: _activeRenderFragmentFactories,
                localRenderFragmentCarriers: GetLocalRenderFragmentCarrierSnapshot(),
                memberRenderFragmentCarriers: GetMemberRenderFragmentCarrierSnapshot(),
                factoryRenderFragmentCarriers: GetFactoryRenderFragmentCarrierSnapshot(),
                localFunctionDeclarations: GetLocalFunctionDeclarationSnapshot(),
                accessibleTemplateLocals: null,
                accessibleTemplateParameters: _accessibleTemplateParameters,
                allowTemplateScopedLocals: true);

        private void MergeCallerOwnedOpenNodeDelta(
            OpenNodeBuilder targetOpenNode,
            OpenNodeBuilder deltaOpenNode,
            ImmutableArray<RenderHelperValueBinding> extraArgumentBindings,
            ImmutableArray<RazorVueSourceOrigin> invocationOrigins)
        {
            var snapshot = deltaOpenNode.CreateSnapshot();
            if (snapshot.KeyAssigned)
                targetOpenNode.SetKeyWithoutReplay(snapshot.Key, snapshot.KeyAssigned);

            foreach (var attribute in snapshot.Attributes)
                targetOpenNode.AddAttributeWithoutReplay(attribute);

            foreach (var slotTemplate in snapshot.SlotTemplates)
                targetOpenNode.AddSlotTemplateWithoutReplay(slotTemplate);

            foreach (var assignment in snapshot.ImplicitDefaultSlotAssignments)
                targetOpenNode.AddImplicitDefaultSlotAssignmentWithoutReplay(assignment);

            foreach (var child in snapshot.AmbientDefaultSlotChildren.Children)
                targetOpenNode.AddAmbientDefaultSlotChildWithoutReplay(child);

            foreach (var child in snapshot.Children.Children)
                targetOpenNode.AddChildWithoutReplay(child);

            if (targetOpenNode is ElementBuilder targetElement)
            {
                foreach (var eventModifier in snapshot.ReplayOperations.OfType<RazorVueOpenNodeEventModifierReplayOperation>())
                    targetElement.ApplyEventModifierReplayWithoutReplay(eventModifier);
            }

            var replayOperations = NormalizeReplayOperationsForCallerOwnedOpenNode(targetOpenNode, snapshot);
            if (!extraArgumentBindings.IsDefaultOrEmpty)
            {
                replayOperations =
                [
                    new RazorVueOpenNodeScopedReplayOperation(
                        ToCapturedValueBindings(extraArgumentBindings),
                        StripCapturedBindings(replayOperations, extraArgumentBindings),
                        invocationOrigins)
                ];
            }

            targetOpenNode.AddReplayOperations(replayOperations);
        }

        private static ImmutableArray<RazorVueOpenNodeReplayOperation> NormalizeReplayOperationsForCallerOwnedOpenNode(
            OpenNodeBuilder targetOpenNode,
            OpenNodeSnapshot snapshot)
        {
            var operations = snapshot.ReplayOperations;
            if (operations.IsDefaultOrEmpty)
                return operations;

            if (targetOpenNode is not ComponentBuilder)
                return operations;

            return NormalizeComponentReplayOperations(operations, snapshot);
        }

        private static ImmutableArray<RazorVueOpenNodeReplayOperation> NormalizeComponentReplayOperations(
            ImmutableArray<RazorVueOpenNodeReplayOperation> operations,
            OpenNodeSnapshot snapshot)
        {
            if (operations.IsDefaultOrEmpty)
                return operations;

            var builder = ImmutableArray.CreateBuilder<RazorVueOpenNodeReplayOperation>(operations.Length + 1);
            var ambientDefaultSlotChildren = ImmutableArray.CreateBuilder<RazorVueRenderNode>();
            var implicitDefaultSlotAssignmentChildren = ImmutableArray.CreateBuilder<RazorVueRenderNode>();
            foreach (var operation in operations)
            {
                if (operation is RazorVueOpenNodeAmbientDefaultSlotChildReplayOperation ambientChildOperation)
                {
                    ambientDefaultSlotChildren.Add(ambientChildOperation.Child);
                    continue;
                }

                if (operation is RazorVueOpenNodeImplicitDefaultSlotAssignmentReplayOperation assignmentOperation)
                    implicitDefaultSlotAssignmentChildren.AddRange(assignmentOperation.Assignment.Children.Children);
            }

            foreach (var operation in operations)
            {
                switch (operation)
                {
                    case RazorVueOpenNodeConditionalReplayOperation conditionalOperation:
                        builder.Add(conditionalOperation with
                        {
                            WhenTrue = NormalizeComponentReplayOperations(conditionalOperation.WhenTrue, snapshot),
                            WhenFalse = NormalizeComponentReplayOperations(conditionalOperation.WhenFalse, snapshot)
                        });
                        break;
                    case RazorVueOpenNodeSwitchReplayOperation switchOperation:
                        builder.Add(switchOperation with
                        {
                            Sections = NormalizeComponentReplaySwitchSections(switchOperation.Sections, snapshot)
                        });
                        break;
                    case RazorVueOpenNodeScopedReplayOperation scopedOperation:
                        builder.Add(scopedOperation with
                        {
                            Operations = NormalizeComponentReplayOperations(scopedOperation.Operations, snapshot)
                        });
                        break;
                    case RazorVueOpenNodeAmbientDefaultSlotChildReplayOperation:
                        break;
                    case RazorVueOpenNodeChildReplayOperation childOperation
                        when ContainsReferenceNode(ambientDefaultSlotChildren, childOperation.Child) ||
                             ContainsReferenceNode(implicitDefaultSlotAssignmentChildren, childOperation.Child) ||
                             ContainsReferenceNode(snapshot.AmbientDefaultSlotChildren, childOperation.Child) ||
                             ContainsReferenceNode(snapshot.ImplicitDefaultSlotAssignments, childOperation.Child):
                        break;
                    default:
                        builder.Add(operation);
                        break;
                }
            }

            if (ambientDefaultSlotChildren.Count > 0)
            {
                builder.Add(new RazorVueOpenNodeAmbientDefaultSlotFragmentReplayOperation(
                    new RazorVueRenderFragment([.. ambientDefaultSlotChildren]),
                    ambientDefaultSlotChildren
                        .SelectMany(static child => child.Origins)
                        .ToImmutableArray()));
            }

            return builder.ToImmutable();
        }

        private static ImmutableArray<RazorVueOpenNodeSwitchReplaySection> NormalizeComponentReplaySwitchSections(
            ImmutableArray<RazorVueOpenNodeSwitchReplaySection> sections,
            OpenNodeSnapshot snapshot)
        {
            if (sections.IsDefaultOrEmpty)
                return sections;

            return [.. sections.Select(section => section with
            {
                Operations = NormalizeComponentReplayOperations(section.Operations, snapshot)
            })];
        }

        private static bool ContainsReferenceNode(
            RazorVueRenderFragment fragment,
            RazorVueRenderNode candidate)
        {
            if (fragment.Children.IsDefaultOrEmpty)
                return false;

            foreach (var child in fragment.Children)
            {
                if (ReferenceEquals(child, candidate))
                    return true;
            }

            return false;
        }

        private static bool ContainsReferenceNode(
            IEnumerable<RazorVueRenderNode> nodes,
            RazorVueRenderNode candidate)
        {
            foreach (var node in nodes)
            {
                if (ReferenceEquals(node, candidate))
                    return true;
            }

            return false;
        }

        private static bool ContainsReferenceNode(
            ImmutableArray<RazorVueImplicitDefaultSlotAssignmentNode> assignments,
            RazorVueRenderNode candidate)
        {
            if (assignments.IsDefaultOrEmpty)
                return false;

            foreach (var assignment in assignments)
            {
                if (ContainsReferenceNode(assignment.Children, candidate))
                    return true;
            }

            return false;
        }

        private static RazorVueRenderFragment StripCapturedBindings(
            RazorVueRenderFragment fragment,
            ImmutableArray<RenderHelperValueBinding> capturedBindings)
        {
            if (fragment.Children.IsDefaultOrEmpty || capturedBindings.IsDefaultOrEmpty)
                return fragment;

            return new RazorVueRenderFragment(
            [
                .. fragment.Children.Select(child => StripCapturedBindings(child, capturedBindings))
            ]);
        }

        private static RazorVueRenderNode StripCapturedBindings(
            RazorVueRenderNode node,
            ImmutableArray<RenderHelperValueBinding> capturedBindings)
            => node switch
            {
                RazorVueElementNode element => element with
                {
                    Key = StripCapturedBindings(element.Key, capturedBindings),
                    Attributes = [.. element.Attributes.Select(attribute => StripCapturedBindings(attribute, capturedBindings))],
                    Children = StripCapturedBindings(element.Children, capturedBindings)
                },
                RazorVueComponentNode component => component with
                {
                    Key = StripCapturedBindings(component.Key, capturedBindings),
                    Attributes = [.. component.Attributes.Select(attribute => StripCapturedBindings(attribute, capturedBindings))],
                    SlotTemplates = [.. component.SlotTemplates.Select(slotTemplate => slotTemplate with
                    {
                        Children = StripCapturedBindings(slotTemplate.Children, capturedBindings)
                    })],
                    ImplicitDefaultSlotAssignments = [.. component.ImplicitDefaultSlotAssignments.Select(assignment => assignment with
                    {
                        Children = StripCapturedBindings(assignment.Children, capturedBindings)
                    })],
                    AmbientDefaultSlotChildren = StripCapturedBindings(component.AmbientDefaultSlotChildren, capturedBindings),
                    Children = StripCapturedBindings(component.Children, capturedBindings)
                },
                RazorVueConditionalNode conditional => conditional with
                {
                    WhenTrue = StripCapturedBindings(conditional.WhenTrue, capturedBindings),
                    WhenFalse = StripCapturedBindings(conditional.WhenFalse, capturedBindings)
                },
                RazorVueRecoveredSwitchConditionalNode conditional => conditional with
                {
                    WhenTrue = StripCapturedBindings(conditional.WhenTrue, capturedBindings),
                    WhenFalse = StripCapturedBindings(conditional.WhenFalse, capturedBindings)
                },
                RazorVueForEachNode loop => loop with
                {
                    Body = StripCapturedBindings(loop.Body, capturedBindings)
                },
                RazorVueForNode loop => loop with
                {
                    Body = StripCapturedBindings(loop.Body, capturedBindings)
                },
                RazorVueTemplateScopeNode templateScope => templateScope with
                {
                    Children = StripCapturedBindings(templateScope.Children, capturedBindings)
                },
                _ => node
            };

        private static RazorVueAttributeEntry StripCapturedBindings(
            RazorVueAttributeEntry attributeEntry,
            ImmutableArray<RenderHelperValueBinding> capturedBindings)
            => attributeEntry switch
            {
                RazorVueAttributeNode attribute => attribute with
                {
                    CapturedBindings = FilterCapturedBindings(attribute.CapturedBindings, capturedBindings),
                    EventModifiers = StripCapturedBindings(attribute.EventModifiers, capturedBindings)
                },
                RazorVueAttributeSpreadNode spread => spread with
                {
                    CapturedBindings = FilterCapturedBindings(spread.CapturedBindings, capturedBindings)
                },
                _ => attributeEntry
            };

        private static RazorVueNodeKey? StripCapturedBindings(
            RazorVueNodeKey? key,
            ImmutableArray<RenderHelperValueBinding> capturedBindings)
        {
            if (key is null || capturedBindings.IsDefaultOrEmpty)
                return key;

            return key with
            {
                CapturedBindings = FilterCapturedBindings(key.CapturedBindings, capturedBindings)
            };
        }

        private static ImmutableArray<RazorVueOpenNodeReplayOperation> StripCapturedBindings(
            ImmutableArray<RazorVueOpenNodeReplayOperation> operations,
            ImmutableArray<RenderHelperValueBinding> capturedBindings)
        {
            if (operations.IsDefaultOrEmpty || capturedBindings.IsDefaultOrEmpty)
                return operations;

            return [.. operations.Select(operation => StripCapturedBindings(operation, capturedBindings))];
        }

        private static RazorVueOpenNodeReplayOperation StripCapturedBindings(
            RazorVueOpenNodeReplayOperation operation,
            ImmutableArray<RenderHelperValueBinding> capturedBindings)
            => operation switch
            {
                RazorVueOpenNodeAttributeReplayOperation attributeOperation => attributeOperation with
                {
                    Attribute = StripCapturedBindings(attributeOperation.Attribute, capturedBindings)
                },
                RazorVueOpenNodeEventModifierReplayOperation modifierOperation => modifierOperation with
                {
                    EventModifiers = StripCapturedBindings(modifierOperation.EventModifiers, capturedBindings)
                },
                RazorVueOpenNodeLocalDeclarationReplayOperation => operation,
                RazorVueOpenNodeImperativeReplayOperation => operation,
                RazorVueOpenNodeConditionalReplayOperation conditionalOperation => conditionalOperation with
                {
                    WhenTrue = StripCapturedBindings(conditionalOperation.WhenTrue, capturedBindings),
                    WhenFalse = StripCapturedBindings(conditionalOperation.WhenFalse, capturedBindings)
                },
                RazorVueOpenNodeSwitchReplayOperation switchOperation => switchOperation with
                {
                    Sections = StripCapturedBindings(switchOperation.Sections, capturedBindings)
                },
                RazorVueOpenNodeKeyReplayOperation keyOperation => keyOperation with
                {
                    Key = StripCapturedBindings(keyOperation.Key, capturedBindings)
                },
                RazorVueOpenNodeSlotTemplateReplayOperation slotTemplateOperation => slotTemplateOperation with
                {
                    SlotTemplate = slotTemplateOperation.SlotTemplate with
                    {
                        Children = StripCapturedBindings(slotTemplateOperation.SlotTemplate.Children, capturedBindings)
                    }
                },
                RazorVueOpenNodeImplicitDefaultSlotAssignmentReplayOperation assignmentOperation => assignmentOperation with
                {
                    Assignment = assignmentOperation.Assignment with
                    {
                        Children = StripCapturedBindings(assignmentOperation.Assignment.Children, capturedBindings)
                    }
                },
                RazorVueOpenNodeAmbientDefaultSlotChildReplayOperation ambientChildOperation => ambientChildOperation with
                {
                    Child = StripCapturedBindings(ambientChildOperation.Child, capturedBindings)
                },
                RazorVueOpenNodeAmbientDefaultSlotFragmentReplayOperation ambientFragmentOperation => ambientFragmentOperation with
                {
                    Children = StripCapturedBindings(ambientFragmentOperation.Children, capturedBindings)
                },
                RazorVueOpenNodeChildReplayOperation childOperation => childOperation with
                {
                    Child = StripCapturedBindings(childOperation.Child, capturedBindings)
                },
                RazorVueOpenNodeScopedReplayOperation scopedOperation => scopedOperation with
                {
                    CapturedBindings = FilterCapturedBindings(scopedOperation.CapturedBindings, capturedBindings),
                    Operations = StripCapturedBindings(scopedOperation.Operations, capturedBindings)
                },
                _ => operation
            };

        private static ImmutableArray<RazorVueOpenNodeSwitchReplaySection> StripCapturedBindings(
            ImmutableArray<RazorVueOpenNodeSwitchReplaySection> sections,
            ImmutableArray<RenderHelperValueBinding> capturedBindings)
        {
            if (sections.IsDefaultOrEmpty || capturedBindings.IsDefaultOrEmpty)
                return sections;

            return [.. sections.Select(section => section with
            {
                Operations = StripCapturedBindings(section.Operations, capturedBindings)
            })];
        }

        private static RazorVueEventModifiers StripCapturedBindings(
            RazorVueEventModifiers modifiers,
            ImmutableArray<RenderHelperValueBinding> capturedBindings)
        {
            if (!modifiers.HasAny || capturedBindings.IsDefaultOrEmpty)
                return modifiers;

            return modifiers with
            {
                PreventDefault = StripCapturedBindings(modifiers.PreventDefault, capturedBindings),
                StopPropagation = StripCapturedBindings(modifiers.StopPropagation, capturedBindings)
            };
        }

        private static RazorVueEventModifierBinding? StripCapturedBindings(
            RazorVueEventModifierBinding? binding,
            ImmutableArray<RenderHelperValueBinding> capturedBindings)
        {
            if (binding is null || capturedBindings.IsDefaultOrEmpty)
                return binding;

            return binding with
            {
                CapturedBindings = FilterCapturedBindings(binding.CapturedBindings, capturedBindings)
            };
        }

        private static ImmutableArray<RazorVueCapturedValueBinding> FilterCapturedBindings(
            ImmutableArray<RazorVueCapturedValueBinding> existingBindings,
            ImmutableArray<RenderHelperValueBinding> bindingsToStrip)
        {
            if (existingBindings.IsDefaultOrEmpty || bindingsToStrip.IsDefaultOrEmpty)
                return existingBindings;

            var bindingParameters = bindingsToStrip
                .Select(static binding => binding.ParameterSymbol)
                .ToImmutableHashSet(SymbolEqualityComparer.Default);
            return [.. existingBindings.Where(binding => !bindingParameters.Contains(binding.ParameterSymbol))];
        }


        private static IEnumerable<IOperation> EnumerateSelfAndDescendants(IOperation root)
        {
            yield return root;
            foreach (var descendant in root.Descendants())
                yield return descendant;
        }

        private bool TryResolveSlotOutlet(IOperation operation, out string slotName)
        {
            slotName = string.Empty;
            if (Unwrap(operation) is not IPropertyReferenceOperation propertyReference)
                return false;

            if (!IsCurrentComponentMember(propertyReference.Property, propertyReference.Instance))
                return false;

            if (!IsRenderFragment(propertyReference.Property.Type))
                return false;

            if (!propertyReference.Property.GetAttributes().Any(static attribute =>
                    string.Equals(
                        attribute.AttributeClass?.ToDisplayString(),
                        "Microsoft.AspNetCore.Components.ParameterAttribute",
                        StringComparison.Ordinal)))
            {
                return false;
            }

            slotName = string.Equals(propertyReference.Property.Name, "ChildContent", StringComparison.Ordinal)
                ? "default"
                : ToLowerCamelCase(propertyReference.Property.Name);
            return true;
        }

        private bool IsRenderFragment(ITypeSymbol typeSymbol)
            => typeSymbol is INamedTypeSymbol namedType &&
               ((_symbols.RenderFragment is not null && SymbolEqualityComparer.Default.Equals(namedType.OriginalDefinition, _symbols.RenderFragment)) ||
                (_symbols.RenderFragmentOfT is not null && SymbolEqualityComparer.Default.Equals(namedType.OriginalDefinition, _symbols.RenderFragmentOfT)));

        private bool IsCurrentComponentMethod(IMethodSymbol method, IOperation? instance)
        {
            for (var current = _snapshot.ComponentSymbol; current is not null; current = current.BaseType)
            {
                if (SymbolEqualityComparer.Default.Equals(method.ContainingType, current))
                    return instance is null || Unwrap(instance) is IInstanceReferenceOperation;
            }

            return false;
        }

        private static bool ContainsRenderTreeBuilderParameter(IMethodSymbol method)
            => method.Parameters.Any(static parameter => IsRenderTreeBuilderType(parameter.Type));

        private static bool IsRenderTreeBuilderMethod(IMethodSymbol method)
            => IsRenderTreeBuilderType(method.ContainingType);

        private static bool IsEventModifierInvocation(IMethodSymbol method)
            => (string.Equals(method.Name, "AddEventPreventDefaultAttribute", StringComparison.Ordinal) ||
                string.Equals(method.Name, "AddEventStopPropagationAttribute", StringComparison.Ordinal)) &&
               string.Equals(
                   method.ContainingType?.ToDisplayString(),
                   "Microsoft.AspNetCore.Components.Web.WebRenderTreeBuilderExtensions",
                   StringComparison.Ordinal);

        private static bool IsRenderTreeBuilderType(ITypeSymbol? typeSymbol)
            => string.Equals(
                typeSymbol?.ToDisplayString(),
                "Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder",
                StringComparison.Ordinal);

        private bool IsCurrentComponentMember(ISymbol symbol, IOperation? instance)
        {
            for (var current = _snapshot.ComponentSymbol; current is not null; current = current.BaseType)
            {
                if (SymbolEqualityComparer.Default.Equals(symbol.ContainingType, current))
                    return instance is null || Unwrap(instance) is IInstanceReferenceOperation;
            }

            return false;
        }

        private OpenNodeBuilder GetRequiredOpenNodeBuilder(IInvocationOperation invocation)
        {
            if (_openFrames.Count == 0)
            {
                throw CreateStructuralIssue(
                    invocation,
                    $"BuildRenderTree encountered '{invocation.TargetMethod.Name}' without an open element or component frame in component '{_snapshot.Descriptor.FullName}'.");
            }

            if (_openFrames.Peek() is not OpenNodeBuilder currentNode)
            {
                throw CreateStructuralIssue(
                    invocation,
                    $"BuildRenderTree encountered '{invocation.TargetMethod.Name}' while the current open frame is {_openFrames.Peek().Describe()} in component '{_snapshot.Descriptor.FullName}'.");
            }

            return currentNode;
        }

        private ComponentBuilder GetRequiredOpenComponentBuilder(IInvocationOperation invocation)
        {
            var currentNode = GetRequiredOpenNodeBuilder(invocation);
            if (currentNode is ComponentBuilder componentBuilder)
                return componentBuilder;

            throw CreateStructuralIssue(
                invocation,
                $"BuildRenderTree encountered '{invocation.TargetMethod.Name}' while the current open node is {currentNode.Describe()} in component '{_snapshot.Descriptor.FullName}'.");
        }

        private ElementBuilder GetRequiredOpenElementBuilder(IInvocationOperation invocation)
        {
            var currentNode = GetRequiredOpenNodeBuilder(invocation);
            if (currentNode is ElementBuilder elementBuilder)
                return elementBuilder;

            throw CreateStructuralIssue(
                invocation,
                $"BuildRenderTree encountered '{invocation.TargetMethod.Name}' while the current open node is {currentNode.Describe()} in component '{_snapshot.Descriptor.FullName}'. Event modifiers are only supported on HTML element frames.");
        }

        private bool TryGetNearestOpenNodeBuilder(out OpenNodeBuilder currentNode)
        {
            foreach (var frame in _openFrames)
            {
                if (frame is OpenNodeBuilder nodeBuilder)
                {
                    currentNode = nodeBuilder;
                    return true;
                }
            }

            currentNode = default!;
            return false;
        }

        private static IOperation? GetInvocationArgument(IInvocationOperation invocation, int index)
        {
            if (invocation.Arguments.Length <= index)
                return null;

            return Unwrap(invocation.Arguments[index].Value);
        }

        private static string? GetConstantStringArgument(IInvocationOperation invocation, int index)
            => TryGetConstantString(GetInvocationArgument(invocation, index));

        private static string? TryGetConstantString(IOperation? operation)
        {
            var current = Unwrap(operation);
            if (current?.ConstantValue.HasValue == true &&
                current.ConstantValue.Value is string text)
                return text;

            return null;
        }

        private IOperation? TryGetLocalMarkupStringInitializer(ILocalSymbol local)
            => _localStaticMarkupCarriers.TryGetValue(local, out var initializer)
                ? initializer
                : TryGetLocalStaticMarkupInitializer(local);

        private IOperation? TryGetLocalStaticMarkupInitializer(ILocalSymbol local)
        {
            if (TryGetSourceStableStaticMarkupInitializer(local) is { } sourceStableInitializer)
                return sourceStableInitializer;

            return RazorVueSourceStableLocalInitializerHelper.TryGetSourceStableLocalInitializer(
                _compilation,
                local,
                RazorVueStaticMarkupValueHelper.IsStaticMarkupCarrierType,
                out var initializer)
                ? initializer
                : null;
        }

        private IOperation? TryGetPropertyMarkupStringInitializer(IPropertySymbol property)
        {
            foreach (var reference in property.DeclaringSyntaxReferences)
            {
                if (reference.GetSyntax() is not PropertyDeclarationSyntax declaration)
                    continue;

                var semanticModel = _compilation.GetSemanticModel(declaration.SyntaxTree);
                if (RazorVuePropertyInitializerHelper.TryGetPropertyValueOperation(semanticModel, declaration, out var propertyOperation))
                    return propertyOperation;
            }

            return null;
        }

        private IOperation? TryGetFieldMarkupStringInitializer(IFieldSymbol field)
        {
            foreach (var reference in field.DeclaringSyntaxReferences)
            {
                if (reference.GetSyntax() is not VariableDeclaratorSyntax declarator ||
                    declarator.Initializer?.Value is null)
                {
                    continue;
                }

                var semanticModel = _compilation.GetSemanticModel(declarator.SyntaxTree);
                if (RazorVuePropertyInitializerHelper.TryGetNormalizedOperation(
                        semanticModel,
                        declarator.Initializer.Value,
                        out var initializerOperation))
                {
                    return initializerOperation;
                }
            }

            return null;
        }

        private bool IsSupportedStaticMarkupFactoryInvocation(IInvocationOperation invocation)
            => IsCurrentComponentMethod(invocation.TargetMethod, invocation.Instance);

        private IOperation? TryGetStaticMarkupFactoryReturnedValue(IInvocationOperation invocation)
            => TryGetRenderFragmentFactoryReturnedValue(invocation, out var returnedValue)
                ? returnedValue
                : null;

        private IOperation CreateLiteralStringOperation(string value)
        {
            if (_literalStringOperationCache.TryGetValue(value, out var cached))
                return cached;

            var parseOptions = _compilation.SyntaxTrees.FirstOrDefault()?.Options as CSharpParseOptions
                               ?? CSharpParseOptions.Default;
            var source = "file static class __RazorVueLiteralHolder { internal static object Value => "
                         + SymbolDisplay.FormatLiteral(value, quote: true)
                         + "; }";
            var syntaxTree = CSharpSyntaxTree.ParseText(source, parseOptions);
            var compilation = CSharpCompilation.Create(
                "__RazorVueLiteralHolder",
                [syntaxTree],
                _compilation.References,
                new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
            var literal = syntaxTree.GetRoot()
                .DescendantNodes()
                .OfType<LiteralExpressionSyntax>()
                .Single();
            var operation = compilation.GetSemanticModel(syntaxTree).GetOperation(literal)
                            ?? throw new InvalidOperationException("Could not materialize a Roslyn literal operation for static BuildRenderTree markup.");

            _literalStringOperationCache[value] = operation;
            return operation;
        }

        private static bool IsConstantNull(IOperation? operation)
        {
            var current = Unwrap(operation);
            return current?.ConstantValue.HasValue == true &&
                   current.ConstantValue.Value is null;
        }

        private static bool IsConstantFalse(IOperation? operation)
        {
            var current = Unwrap(operation);
            return current?.ConstantValue.HasValue == true &&
                   current.ConstantValue.Value is bool value &&
                   !value;
        }

        private static IOperation? Unwrap(IOperation? operation)
            => RazorVueOperationNormalizer.Unwrap(operation);

        private bool ShouldOmitElementAttribute(OpenNodeBuilder currentNode, IOperation? value)
        {
            if (currentNode is not ElementBuilder)
                return false;

            if (value is null)
                return false;

            var current = Unwrap(value);
            if (current is null)
                return false;

            if (IsConstantNull(current))
                return true;

            return current.ConstantValue.HasValue &&
                   current.ConstantValue.Value is bool boolValue &&
                   !boolValue;
        }

        private bool TryHandleComponentSlotValue(
            OpenNodeBuilder currentNode,
            string name,
            IOperation? value,
            IInvocationOperation invocation)
        {
            if (currentNode is not ComponentBuilder componentBuilder)
                return false;

            if (!TryParseSlotTemplate(value, out var slotTemplate))
            {
                if (IsDeclaredComponentSlot(componentBuilder.ComponentType, name) &&
                    value is not null &&
                    IsRenderFragmentLikeValue(value))
                {
                    throw CreateUnsupportedBuilderCall(
                        invocation,
                        $"BuildRenderTree call '{GetBuilderCallDisplayName(invocation)}' passes child content parameter '{name}' on component '{componentBuilder.ComponentFullName}' using a RenderFragment shape that RazorVue cannot canonicalize in component '{_snapshot.Descriptor.FullName}'.");
                }

                return false;
            }

            if (string.Equals(name, "ChildContent", StringComparison.Ordinal) &&
                string.IsNullOrWhiteSpace(slotTemplate.ParameterName))
            {
                var childContent = MaterializeCapturedTemplateChildren(
                    slotTemplate,
                    CreateOrigins(invocation, RazorVueOriginKind.Template));
                currentNode.AddImplicitDefaultSlotAssignment(new RazorVueImplicitDefaultSlotAssignmentNode(
                    childContent,
                    CreateOrigins(invocation, RazorVueOriginKind.Template)));
                foreach (var child in childContent.Children)
                    currentNode.AddChild(child);
                return true;
            }

            var slotOrigins = CreateOrigins(invocation, RazorVueOriginKind.Template);
            if (TryCreateCurrentComponentForwardedSlotAttribute(componentBuilder.ComponentType, name, value, slotOrigins, out var forwardedSlotAttribute))
            {
                currentNode.AddAttribute(forwardedSlotAttribute);
                return true;
            }

            currentNode.AddSlotTemplate(new RazorVueComponentSlotTemplateNode(
                PublicName: name,
                SlotName: string.Equals(name, "ChildContent", StringComparison.Ordinal)
                    ? "default"
                    : ToLowerCamelCase(name),
                ParameterName: slotTemplate.ParameterName,
                ParameterSymbol: slotTemplate.ParameterSymbol,
                Children: MaterializeCapturedTemplateChildren(
                    slotTemplate,
                    slotOrigins),
                Origins: slotOrigins));
            return true;
        }

        private static bool IsRenderFragmentLikeValue(IOperation operation)
            => RazorVueRenderFragmentTypeHelper.IsRenderFragmentType(Unwrap(operation)?.Type);

        private bool TryCreateCurrentComponentForwardedSlotAttribute(
            INamedTypeSymbol componentType,
            string parameterName,
            IOperation? value,
            ImmutableArray<RazorVueSourceOrigin> origins,
            out RazorVueAttributeNode attribute)
        {
            attribute = default!;
            var current = Unwrap(value);
            if (current is not IPropertyReferenceOperation propertyReference ||
                !TryResolveSlotOutlet(propertyReference, out _) ||
                !TryGetDeclaredComponentSlotProperty(componentType, parameterName, out var slotProperty) ||
                !IsParameterizedRenderFragmentType(slotProperty.Type))
            {
                return false;
            }

            attribute = new RazorVueAttributeNode(
                parameterName,
                propertyReference,
                ImmutableArray<RazorVueCapturedValueBinding>.Empty,
                origins);
            return true;
        }

        private bool TryParseChildContent(IOperation? operation, out RazorVueRenderFragment fragment)
        {
            fragment = RazorVueRenderFragment.Empty;
            if (!TryParseSlotTemplate(operation, out var slotTemplate))
                return false;

            if (!string.IsNullOrWhiteSpace(slotTemplate.ParameterName))
                return false;

            fragment = MaterializeCapturedTemplateChildren(
                slotTemplate,
                operation is null
                    ? ImmutableArray<RazorVueSourceOrigin>.Empty
                    : CreateOrigins(operation, RazorVueOriginKind.Template));
            return true;
        }

        private bool TryParseAddContentRenderFragment(
            IInvocationOperation invocation,
            IOperation value,
            out RazorVueRenderFragment fragment)
        {
            fragment = RazorVueRenderFragment.Empty;
            if (!IsRenderFragmentAddContent(invocation))
                return false;

            if (invocation.Arguments.Length != 2)
                return false;

            return TryParseChildContent(value, out fragment);
        }

        private bool TryParseAddContentFragmentFactory(
            IInvocationOperation addContentInvocation,
            IOperation value,
            out RazorVueRenderFragment fragment)
        {
            fragment = RazorVueRenderFragment.Empty;
            if (Unwrap(value) is not IInvocationOperation factoryInvocation)
                return false;

            if (!IsCurrentComponentMethod(factoryInvocation.TargetMethod, factoryInvocation.Instance) ||
                !IsRenderFragmentType(factoryInvocation.TargetMethod.ReturnType))
            {
                return false;
            }

            if (!TryGetSupportedRenderFragmentFactorySignature(
                    factoryInvocation.TargetMethod,
                    out _,
                    out var failureMessage))
            {
                throw CreateUnsupportedBuilderCall(factoryInvocation, failureMessage);
            }

            if (!TryGetRenderFragmentFactoryInvocationBindings(
                    factoryInvocation,
                    out var extraArgumentBindings,
                    out failureMessage))
            {
                throw CreateUnsupportedBuilderCall(factoryInvocation, failureMessage);
            }

            if (!TryResolveFactoryCarrier(factoryInvocation, requireZeroArguments: false, out var slotTemplate))
                return false;

            return TryCreateBoundAddContentFragment(
                addContentInvocation,
                factoryInvocation,
                slotTemplate,
                extraArgumentBindings,
                out fragment);
        }

        private bool TryParseSlotTemplateFragmentFactory(
            IInvocationOperation slotInvocation,
            IOperation value,
            out ParsedSlotTemplate slotTemplate)
        {
            slotTemplate = default;
            if (Unwrap(value) is not IInvocationOperation factoryInvocation)
                return false;

            if (!IsCurrentComponentMethod(factoryInvocation.TargetMethod, factoryInvocation.Instance) ||
                !IsRenderFragmentType(factoryInvocation.TargetMethod.ReturnType))
            {
                return false;
            }

            if (!TryGetSupportedRenderFragmentFactorySignature(
                    factoryInvocation.TargetMethod,
                    out _,
                    out var failureMessage))
            {
                throw CreateUnsupportedBuilderCall(factoryInvocation, failureMessage);
            }

            if (!TryGetRenderFragmentFactoryInvocationBindings(
                    factoryInvocation,
                    out var extraArgumentBindings,
                    out failureMessage))
            {
                throw CreateUnsupportedBuilderCall(factoryInvocation, failureMessage);
            }

            if (!TryResolveFactoryCarrier(factoryInvocation, requireZeroArguments: false, out var parsedFactoryTemplate))
                return false;

            if (extraArgumentBindings.IsDefaultOrEmpty)
            {
                slotTemplate = parsedFactoryTemplate;
                return true;
            }

            slotTemplate = parsedFactoryTemplate.PrependCapturedBindings(extraArgumentBindings);
            return true;
        }

        private bool TryCreateBoundAddContentFragment(
            IInvocationOperation addContentInvocation,
            IInvocationOperation factoryInvocation,
            ParsedSlotTemplate slotTemplate,
            ImmutableArray<RenderHelperValueBinding> extraArgumentBindings,
            out RazorVueRenderFragment fragment)
        {
            fragment = RazorVueRenderFragment.Empty;
            var invocationOrigins = CreateOrigins(factoryInvocation, RazorVueOriginKind.Template);
            if (IsTypedRenderFragmentAddContent(addContentInvocation))
            {
                if (addContentInvocation.Arguments.Length != 3 ||
                    string.IsNullOrWhiteSpace(slotTemplate.ParameterName) ||
                    slotTemplate.ParameterSymbol is null)
                {
                    return false;
                }

                var initializer = GetInvocationArgument(addContentInvocation, 2);
                if (initializer is null || IsConstantNull(initializer))
                    return false;

                fragment = CreateTypedFragmentScope(
                    addContentInvocation,
                    slotTemplate,
                    initializer);
                fragment = WrapCapturedTemplateScopes(fragment, extraArgumentBindings, invocationOrigins);
                return true;
            }

            if (!IsRenderFragmentAddContent(addContentInvocation) ||
                addContentInvocation.Arguments.Length != 2 ||
                !string.IsNullOrWhiteSpace(slotTemplate.ParameterName))
            {
                return false;
            }

            fragment = MaterializeCapturedTemplateChildren(slotTemplate, invocationOrigins);
            fragment = WrapCapturedTemplateScopes(fragment, extraArgumentBindings, invocationOrigins);
            return true;
        }

        private static RazorVueRenderFragment MaterializeCapturedTemplateChildren(
            ParsedSlotTemplate slotTemplate,
            ImmutableArray<RazorVueSourceOrigin> origins)
            => WrapCapturedTemplateScopes(slotTemplate.Children, slotTemplate.CapturedBindings, origins);

        private RazorVueRenderFragment CreateTypedFragmentScope(
            IInvocationOperation invocation,
            ParsedSlotTemplate slotTemplate,
            IOperation initializer)
        {
            var fragment = new RazorVueRenderFragment(
            [
                new RazorVueTemplateScopeNode(
                    ScopeName: slotTemplate.ParameterName!,
                    ScopeParameterSymbol: slotTemplate.ParameterSymbol,
                    Initializer: initializer,
                    Children: slotTemplate.Children,
                    Origins: CreateOrigins(invocation, RazorVueOriginKind.Template))
            ]);

            return WrapCapturedTemplateScopes(
                fragment,
                slotTemplate.CapturedBindings,
                CreateOrigins(invocation, RazorVueOriginKind.Template));
        }

        private static RazorVueRenderFragment WrapCapturedTemplateScopes(
            RazorVueRenderFragment fragment,
            ImmutableArray<RenderHelperValueBinding> extraArgumentBindings,
            ImmutableArray<RazorVueSourceOrigin> origins)
        {
            var wrappedFragment = fragment;
            for (var index = extraArgumentBindings.Length - 1; index >= 0; index--)
            {
                var binding = extraArgumentBindings[index];
                wrappedFragment = new RazorVueRenderFragment(
                [
                    new RazorVueTemplateScopeNode(
                        ScopeName: binding.ParameterSymbol.Name,
                        ScopeParameterSymbol: binding.ParameterSymbol,
                        Initializer: binding.Initializer,
                        Children: wrappedFragment,
                        Origins: origins)
                ]);
            }

            return wrappedFragment;
        }

        private bool TryParseTypedAddContentTemplate(
            IInvocationOperation invocation,
            IOperation value,
            out RazorVueRenderFragment fragment)
        {
            fragment = RazorVueRenderFragment.Empty;
            if (!IsTypedRenderFragmentAddContent(invocation))
                return false;

            if (invocation.Arguments.Length != 3)
                return false;

            if (!TryParseSlotTemplate(value, out var slotTemplate))
                return false;

            if (string.IsNullOrWhiteSpace(slotTemplate.ParameterName) ||
                slotTemplate.ParameterSymbol is null)
            {
                return false;
            }

            var initializer = GetInvocationArgument(invocation, 2);
            if (initializer is null || IsConstantNull(initializer))
                return false;

            fragment = CreateTypedFragmentScope(invocation, slotTemplate, initializer);
            return true;
        }

        private bool TryParseSlotTemplate(IOperation? operation, out ParsedSlotTemplate slotTemplate)
        {
            slotTemplate = default;
            if (TryParseSlotTemplateFragmentFactoryOperation(operation, out slotTemplate))
                return true;

            if (TryParseCurrentComponentSlotSource(operation, out slotTemplate))
                return true;

            if (TryResolveStoredSlotTemplate(operation, out slotTemplate))
                return true;

            if (TryResolveCurrentComponentMemberSlotTemplate(operation, out slotTemplate))
                return true;

            if (TryResolveCurrentComponentFragmentFactory(operation, out slotTemplate))
                return true;

            if (!TryGetAnonymousFunction(operation, out var anonymousFunction))
                return false;

            if (TryParseUntypedSlotTemplate(anonymousFunction, out slotTemplate))
                return true;

            if (TryParseTypedSlotTemplate(anonymousFunction, out slotTemplate))
                return true;

            return false;
        }

        internal bool TryParseSlotTemplateForExternalConsumption(IOperation? operation, out ParsedSlotTemplate slotTemplate)
            => TryParseSlotTemplate(operation, out slotTemplate);

        private bool TryParseCurrentComponentSlotSource(IOperation? operation, out ParsedSlotTemplate slotTemplate)
        {
            slotTemplate = default;
            var current = Unwrap(operation);
            if (current is null || !TryResolveSlotOutlet(current, out var slotName))
                return false;

            slotTemplate = ParsedSlotTemplate.Create(
                parameterName: null,
                parameterSymbol: null,
                children: new RazorVueRenderFragment(
                [
                    new RazorVueSlotOutletNode(
                        slotName,
                        null,
                        CreateOrigins(current, RazorVueOriginKind.Template))
                ]));
            return true;
        }

        private bool TryParseSlotTemplateFragmentFactoryOperation(IOperation? operation, out ParsedSlotTemplate slotTemplate)
        {
            slotTemplate = default;
            return Unwrap(operation) is IInvocationOperation invocation &&
                   TryParseSlotTemplateFragmentFactory(invocation, invocation, out slotTemplate);
        }

        private bool TryResolveStoredSlotTemplate(IOperation? operation, out ParsedSlotTemplate slotTemplate)
        {
            slotTemplate = default;
            if (Unwrap(operation) is not ILocalReferenceOperation localReference)
                return false;

            if (_localRenderFragmentCarriers.TryGetValue(localReference.Local, out slotTemplate))
                return true;

            var initializer = TryGetSourceStableRenderFragmentInitializer(localReference.Local);
            return initializer is not null && TryParseSlotTemplate(initializer, out slotTemplate);
        }

        private void PrimeSourceStableLocalRenderFragmentInitializers(IEnumerable<IOperation> operations)
        {
            _sourceStableLocalRenderFragmentInitializers.Clear();
            var buffered = operations as IReadOnlyList<IOperation> ?? operations.ToArray();
            if (buffered.Count == 0)
                return;

            foreach (var pair in RazorVueImperativeRenderFragmentCarrierHelper
                         .CollectSourceStableLocalRenderFragmentInitializers(_compilation, buffered))
            {
                _sourceStableLocalRenderFragmentInitializers[pair.Key] = pair.Value;
            }
        }

        private void PrimeSourceStableLocalStaticMarkupInitializers(IEnumerable<IOperation> operations)
        {
            _sourceStableLocalStaticMarkupInitializers.Clear();
            var buffered = operations as IReadOnlyList<IOperation> ?? operations.ToArray();
            if (buffered.Count == 0)
                return;

            foreach (var pair in RazorVueSourceStableLocalInitializerHelper.CollectSourceStableLocalInitializers(
                         _compilation,
                         buffered,
                         RazorVueStaticMarkupValueHelper.IsMarkupStringType))
            {
                _sourceStableLocalStaticMarkupInitializers[pair.Key] = pair.Value;
            }
        }

        private void PrimeSourceStableLocalComponentTypeInitializers(IEnumerable<IOperation> operations)
        {
            _sourceStableLocalComponentTypeInitializers.Clear();
            var buffered = operations as IReadOnlyList<IOperation> ?? operations.ToArray();
            if (buffered.Count == 0)
                return;

            foreach (var pair in RazorVueSourceStableLocalInitializerHelper.CollectSourceStableLocalInitializers(
                         _compilation,
                         buffered,
                         RazorVueComponentTypeCarrierHelper.IsSystemType))
            {
                _sourceStableLocalComponentTypeInitializers[pair.Key] = pair.Value;
            }
        }

        private IOperation? TryGetSourceStableRenderFragmentInitializer(ILocalSymbol local)
            => _sourceStableLocalRenderFragmentInitializers.TryGetValue(local, out var initializer)
                ? initializer
                : null;

        private IOperation? TryGetSourceStableStaticMarkupInitializer(ILocalSymbol local)
            => _sourceStableLocalStaticMarkupInitializers.TryGetValue(local, out var initializer)
                ? initializer
                : null;

        private IOperation? TryGetSourceStableComponentTypeInitializer(ILocalSymbol local)
            => _sourceStableLocalComponentTypeInitializers.TryGetValue(local, out var initializer)
                ? initializer
                : null;

        private bool TryResolveCurrentComponentMemberSlotTemplate(IOperation? operation, out ParsedSlotTemplate slotTemplate)
        {
            slotTemplate = default;
            var current = Unwrap(operation);
            switch (current)
            {
                case IPropertyReferenceOperation propertyReference
                    when IsCurrentComponentMember(propertyReference.Property, propertyReference.Instance):
                    return TryResolveMemberCarrier(propertyReference.Property, propertyReference, out slotTemplate);
                case IFieldReferenceOperation fieldReference
                    when IsCurrentComponentMember(fieldReference.Field, fieldReference.Instance):
                    return TryResolveMemberCarrier(fieldReference.Field, fieldReference, out slotTemplate);
                default:
                    return false;
            }
        }

        private bool TryResolveCurrentComponentFragmentFactory(IOperation? operation, out ParsedSlotTemplate slotTemplate)
        {
            slotTemplate = default;
            if (Unwrap(operation) is not IInvocationOperation invocation)
                return false;

            if (!IsCurrentComponentMethod(invocation.TargetMethod, invocation.Instance))
                return false;

            return TryResolveFactoryCarrier(invocation, requireZeroArguments: true, out slotTemplate);
        }

        private bool TryResolveMemberCarrier(
            ISymbol member,
            IOperation referenceOperation,
            out ParsedSlotTemplate slotTemplate)
        {
            if (_memberRenderFragmentCarriers.TryGetValue(member, out slotTemplate))
                return true;

            if (!_activeRenderFragmentMembers.Add(member))
            {
                throw CreateStructuralIssue(
                    referenceOperation,
                    $"BuildRenderTree uses current-component RenderFragment member '{member.Name}' recursively; RazorVue does not support cyclic current-component RenderFragment member carriers in component '{_snapshot.Descriptor.FullName}'.");
            }

            try
            {
                if (!TryCreateMemberCarrier(member, out slotTemplate))
                    return false;

                _memberRenderFragmentCarriers[member] = slotTemplate;
                return true;
            }
            finally
            {
                _activeRenderFragmentMembers.Remove(member);
            }
        }

        private bool TryResolveFactoryCarrier(
            IInvocationOperation invocation,
            bool requireZeroArguments,
            out ParsedSlotTemplate slotTemplate)
        {
            if (requireZeroArguments &&
                (invocation.TargetMethod.Parameters.Length != 0 || invocation.Arguments.Length != 0))
            {
                slotTemplate = default;
                return false;
            }

            var method = RazorVueMethodSymbolNormalizer.GetCanonicalMethod(invocation.TargetMethod);
            if (_factoryRenderFragmentCarriers.TryGetValue(method, out slotTemplate))
                return true;

            if (!_activeRenderFragmentFactories.Add(method))
            {
                throw CreateStructuralIssue(
                    invocation,
                    $"BuildRenderTree fragment factory method '{GetBuilderCallDisplayName(invocation)}' is recursive; RazorVue does not support recursive RenderFragment factory methods in component '{_snapshot.Descriptor.FullName}'.");
            }

            try
            {
                if (!TryCreateFactoryCarrier(invocation, out slotTemplate))
                    return false;

                _factoryRenderFragmentCarriers[method] = slotTemplate;
                return true;
            }
            finally
            {
                _activeRenderFragmentFactories.Remove(method);
            }
        }

        private bool TryCreateMemberCarrier(ISymbol member, out ParsedSlotTemplate slotTemplate)
        {
            slotTemplate = default;
            if (!IsRenderFragmentType(member switch
                {
                    IPropertySymbol property => property.Type,
                    IFieldSymbol field => field.Type,
                    _ => null
                }))
            {
                return false;
            }

            if (!IsSupportedCurrentComponentRenderFragmentCarrierMember(member))
                return false;

            IOperation? initializer = member switch
            {
                IPropertySymbol property => TryGetPropertyRenderFragmentInitializer(property),
                IFieldSymbol field => TryGetFieldRenderFragmentInitializer(field),
                _ => null
            };

            if (initializer is null)
                return false;

            return TryGetParsedSlotTemplateFromCarrierInitializer(initializer, out slotTemplate);
        }

        private bool IsSupportedCurrentComponentRenderFragmentCarrierMember(ISymbol member)
        {
            switch (member)
            {
                case IPropertySymbol propertySymbol:
                    if (propertySymbol.SetMethod is null)
                        return true;

                    if (!RazorVueMemberWriteAnalysis.CanUseSourceStableMutableCarrierMember(propertySymbol))
                        return false;

                    return !RazorVueMemberWriteAnalysis.HasObservableWritesOutsideDeclarationInitializer(_compilation, propertySymbol);
                case IFieldSymbol fieldSymbol:
                    if (fieldSymbol.IsReadOnly)
                        return true;

                    if (!RazorVueMemberWriteAnalysis.CanUseSourceStableMutableCarrierMember(fieldSymbol))
                        return false;

                    return !RazorVueMemberWriteAnalysis.HasObservableWritesOutsideDeclarationInitializer(_compilation, fieldSymbol);
                default:
                    return false;
            }
        }

        private bool TryCreateFactoryCarrier(
            IInvocationOperation invocation,
            out ParsedSlotTemplate slotTemplate)
        {
            slotTemplate = default;
            var method = invocation.TargetMethod;
            if (!TryGetSupportedRenderFragmentFactorySignature(
                    method,
                    out _,
                    out var failureMessage))
            {
                if (!IsRenderFragmentType(method.ReturnType))
                    return false;

                throw CreateUnsupportedBuilderCall(invocation, failureMessage);
            }

            ThrowIfRenderFragmentFactoryReadOnlyByRefParameterEscapes(invocation);

            if (!TryGetRenderFragmentFactoryReturnedValue(invocation, out var returnedValue))
            {
                throw CreateUnsupportedBuilderCall(
                    invocation,
                    $"BuildRenderTree fragment factory method '{GetBuilderCallDisplayName(invocation)}' must be source-authored with an analyzable return value in component '{_snapshot.Descriptor.FullName}'.");
            }

            if (!TryGetParsedSlotTemplateFromCarrierInitializer(returnedValue, out slotTemplate))
            {
                throw CreateUnsupportedBuilderCall(
                    invocation,
                    $"BuildRenderTree fragment factory method '{GetBuilderCallDisplayName(invocation)}' must return an analyzable RenderFragment template shape in component '{_snapshot.Descriptor.FullName}'.");
            }

            return true;
        }

        private bool TryGetSupportedRenderFragmentFactorySignature(
            IMethodSymbol method,
            out ImmutableArray<IParameterSymbol> extraParameters,
            out string failureMessage)
        {
            extraParameters = ImmutableArray<IParameterSymbol>.Empty;
            failureMessage = string.Empty;
            if (!IsRenderFragmentType(method.ReturnType))
                return false;

            var helperDisplayName = method.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat);

            if (ContainsRenderTreeBuilderParameter(method))
            {
                failureMessage =
                    $"BuildRenderTree fragment factory method '{helperDisplayName}' must not declare RenderTreeBuilder parameters in component '{_snapshot.Descriptor.FullName}'.";
                return false;
            }

            foreach (var parameter in method.Parameters)
            {
                if (parameter.RefKind is RefKind.None or RefKind.In)
                    continue;

                var modifier = GetRefKindModifier(parameter.RefKind);
                failureMessage =
                    $"BuildRenderTree fragment factory method '{helperDisplayName}' cannot declare '{modifier}' parameter '{parameter.Name}' in component '{_snapshot.Descriptor.FullName}'. Only ordinary by-value parameters and read-only 'in' value parameters are supported.";
                return false;
            }

            extraParameters = method.Parameters
                .Select(parameter => RazorVueMethodSymbolNormalizer.NormalizeParameter(method, parameter))
                .ToImmutableArray();
            return true;
        }

        private bool TryGetRenderFragmentFactoryInvocationBindings(
            IInvocationOperation invocation,
            out ImmutableArray<RenderHelperValueBinding> extraArgumentBindings,
            out string failureMessage)
        {
            extraArgumentBindings = ImmutableArray<RenderHelperValueBinding>.Empty;
            failureMessage = string.Empty;

            if (invocation.Arguments.Length != invocation.TargetMethod.Parameters.Length)
            {
                failureMessage =
                    $"BuildRenderTree fragment factory method '{GetBuilderCallDisplayName(invocation)}' must be invoked with arguments matching its declared signature in component '{_snapshot.Descriptor.FullName}'.";
                return false;
            }

            var boundParameters = new HashSet<IParameterSymbol>(SymbolEqualityComparer.Default);
            var extraBindingsBuilder = ImmutableArray.CreateBuilder<RenderHelperValueBinding>(invocation.Arguments.Length);
            foreach (var argument in invocation.Arguments)
            {
                if (argument.Parameter is not { } rawParameter)
                {
                    failureMessage =
                        $"BuildRenderTree fragment factory method '{GetBuilderCallDisplayName(invocation)}' must use direct one-to-one argument binding in component '{_snapshot.Descriptor.FullName}'.";
                    return false;
                }

                var parameter = RazorVueMethodSymbolNormalizer.NormalizeParameter(invocation.TargetMethod, rawParameter);
                if (!IsSupportedRenderFragmentFactoryArgumentKind(invocation, parameter, out failureMessage))
                    return false;

                if (!boundParameters.Add(parameter))
                {
                    failureMessage =
                        $"BuildRenderTree fragment factory method '{GetBuilderCallDisplayName(invocation)}' must use direct one-to-one argument binding in component '{_snapshot.Descriptor.FullName}'.";
                    return false;
                }

                var initializer = Unwrap(argument.Value);
                if (initializer is null)
                {
                    failureMessage =
                        $"BuildRenderTree fragment factory method '{GetBuilderCallDisplayName(invocation)}' contains an unsupported argument value for parameter '{parameter.Name}' in component '{_snapshot.Descriptor.FullName}'.";
                    return false;
                }

                extraBindingsBuilder.Add(new RenderHelperValueBinding(parameter, initializer));
            }

            if (boundParameters.Count != invocation.TargetMethod.Parameters.Length)
            {
                failureMessage =
                    $"BuildRenderTree fragment factory method '{GetBuilderCallDisplayName(invocation)}' must be invoked with arguments matching its declared signature in component '{_snapshot.Descriptor.FullName}'.";
                return false;
            }

            extraArgumentBindings = extraBindingsBuilder.ToImmutable();
            return true;
        }

        private bool IsSupportedRenderFragmentFactoryArgumentKind(
            IInvocationOperation invocation,
            IParameterSymbol normalizedParameter,
            out string failureMessage)
        {
            failureMessage = string.Empty;

            var expectedRefKind = normalizedParameter.RefKind;
            if (expectedRefKind is RefKind.None or RefKind.In)
                return true;

            var modifier = GetRefKindModifier(expectedRefKind);
            failureMessage =
                $"BuildRenderTree fragment factory method '{GetBuilderCallDisplayName(invocation)}' cannot bind '{modifier}' argument for parameter '{normalizedParameter.Name}' in component '{_snapshot.Descriptor.FullName}'. RenderFragment factory parameters only support by-value binding and read-only 'in' value binding.";
            return false;
        }

        private bool TryGetRenderFragmentFactoryReturnedValue(
            IInvocationOperation invocation,
            out IOperation returnedValue)
            => RazorVueImperativeRenderFragmentCarrierHelper.TryGetRenderFragmentFactoryReturnedValue(
                _compilation,
                invocation,
                out returnedValue);

        private IOperation? TryGetPropertyRenderFragmentInitializer(IPropertySymbol property)
            => RazorVueImperativeRenderFragmentCarrierHelper.TryGetPropertyRenderFragmentInitializer(
                _compilation,
                property);

        private IOperation? TryGetFieldRenderFragmentInitializer(IFieldSymbol field)
            => RazorVueImperativeRenderFragmentCarrierHelper.TryGetFieldRenderFragmentInitializer(
                _compilation,
                field);

        private bool TryGetParsedSlotTemplateFromCarrierInitializer(IOperation initializer, out ParsedSlotTemplate slotTemplate)
        {
            slotTemplate = default;
            return TryParseSlotTemplate(initializer, out slotTemplate);
        }

        private bool TryParseUntypedSlotTemplate(
            IAnonymousFunctionOperation anonymousFunction,
            out ParsedSlotTemplate slotTemplate)
        {
            slotTemplate = default;
            if (!TryGetSingleBuilderParameter(anonymousFunction, out _))
                return false;

            slotTemplate = ParsedSlotTemplate.Create(
                parameterName: null,
                parameterSymbol: null,
                children: ParseAnonymousFunctionBody(anonymousFunction));
            return true;
        }

        private bool TryParseTypedSlotTemplate(
            IAnonymousFunctionOperation anonymousFunction,
            out ParsedSlotTemplate slotTemplate)
        {
            slotTemplate = default;
            if (anonymousFunction.Symbol.Parameters.Length != 1)
                return false;

            var slotContextParameter = anonymousFunction.Symbol.Parameters[0];
            var body = anonymousFunction.Body;
            if (body is null)
                return false;

            IOperation? returnedBuilderFactory = null;
            switch (Unwrap(body))
            {
                case IAnonymousFunctionOperation nestedAnonymousFunction:
                    returnedBuilderFactory = nestedAnonymousFunction;
                    break;
                case IDelegateCreationOperation delegateCreation:
                    returnedBuilderFactory = delegateCreation;
                    break;
                case IBlockOperation block when TryGetSingleReturnedValue(block, out var returnValue):
                    returnedBuilderFactory = returnValue;
                    break;
                case IReturnOperation returnOperation when returnOperation.ReturnedValue is not null:
                    returnedBuilderFactory = returnOperation.ReturnedValue;
                    break;
            }

            if (!TryGetAnonymousFunction(returnedBuilderFactory, out var builderAnonymousFunction))
                return false;

            if (!TryGetSingleBuilderParameter(builderAnonymousFunction, out _))
                return false;

            slotTemplate = ParsedSlotTemplate.Create(
                parameterName: slotContextParameter.Name,
                parameterSymbol: slotContextParameter,
                children: ParseAnonymousFunctionBody(builderAnonymousFunction));
            return true;
        }

        private RazorVueRenderFragment ParseAnonymousFunctionBody(IAnonymousFunctionOperation anonymousFunction)
        {
            if (!TryGetBuilderParameters(anonymousFunction, out var builderParameters))
                return RazorVueRenderFragment.Empty;

            var body = anonymousFunction.Body;
            if (body is null)
                return RazorVueRenderFragment.Empty;

            if (body is IBlockOperation block)
                return new Parser(
                    _snapshot,
                    _compilation,
                    _symbols,
                    builderParameters,
                    activeRenderHelperMethods: _activeRenderHelperMethods,
                    activeRenderFragmentMembers: _activeRenderFragmentMembers,
                    activeRenderFragmentFactories: _activeRenderFragmentFactories,
                    localRenderFragmentCarriers: GetLocalRenderFragmentCarrierSnapshot(),
                    memberRenderFragmentCarriers: GetMemberRenderFragmentCarrierSnapshot(),
                    factoryRenderFragmentCarriers: GetFactoryRenderFragmentCarrierSnapshot(),
                    localFunctionDeclarations: GetLocalFunctionDeclarationSnapshot(),
                    accessibleTemplateLocals: _accessibleTemplateLocals,
                    accessibleTemplateParameters: _accessibleTemplateParameters,
                    allowTemplateScopedLocals: true).Parse(block.Operations);

            return new Parser(
                _snapshot,
                _compilation,
                _symbols,
                builderParameters,
                activeRenderHelperMethods: _activeRenderHelperMethods,
                activeRenderFragmentMembers: _activeRenderFragmentMembers,
                activeRenderFragmentFactories: _activeRenderFragmentFactories,
                localRenderFragmentCarriers: GetLocalRenderFragmentCarrierSnapshot(),
                memberRenderFragmentCarriers: GetMemberRenderFragmentCarrierSnapshot(),
                factoryRenderFragmentCarriers: GetFactoryRenderFragmentCarrierSnapshot(),
                localFunctionDeclarations: GetLocalFunctionDeclarationSnapshot(),
                accessibleTemplateLocals: _accessibleTemplateLocals,
                accessibleTemplateParameters: _accessibleTemplateParameters,
                allowTemplateScopedLocals: true).Parse([body]);
        }

        private static bool TryGetAnonymousFunction(
            IOperation? operation,
            out IAnonymousFunctionOperation anonymousFunction)
        {
            anonymousFunction = default!;
            var current = UnwrapDelegateCarrier(operation);
            switch (current)
            {
                case IAnonymousFunctionOperation directAnonymousFunction:
                    anonymousFunction = directAnonymousFunction;
                    return true;
                case IDelegateCreationOperation delegateCreation when UnwrapDelegateCarrier(delegateCreation.Target) is IAnonymousFunctionOperation targetAnonymousFunction:
                    anonymousFunction = targetAnonymousFunction;
                    return true;
                default:
                    return false;
            }
        }

        private static IOperation? UnwrapDelegateCarrier(IOperation? operation)
        {
            var current = Unwrap(operation);
            while (current is IConversionOperation conversion)
                current = Unwrap(conversion.Operand);
            return current;
        }

        private static bool TryGetBuilderParameters(
            IAnonymousFunctionOperation anonymousFunction,
            out ImmutableHashSet<IParameterSymbol> builderParameters)
        {
            builderParameters = anonymousFunction.Symbol.Parameters
                .Where(static parameter =>
                    string.Equals(parameter.Name, "builder", StringComparison.Ordinal) ||
                    string.Equals(parameter.Type.Name, "RenderTreeBuilder", StringComparison.Ordinal))
                .ToImmutableHashSet<IParameterSymbol>(SymbolEqualityComparer.Default);
            return builderParameters.Count > 0;
        }

        private static bool TryGetOperationStatements(
            IOperation? operation,
            out ImmutableArray<IOperation> statements)
        {
            statements = ImmutableArray<IOperation>.Empty;
            var current = Unwrap(operation);
            if (current is null)
                return false;

            if (current is IBlockOperation block)
            {
                statements = block.Operations;
                return true;
            }

            if (current is IInvocationOperation invocation)
            {
                statements = [invocation];
                return true;
            }

            return false;
        }

        private static bool TryGetSingleBuilderParameter(
            IAnonymousFunctionOperation anonymousFunction,
            out IParameterSymbol builderParameter)
        {
            builderParameter = default!;
            if (!TryGetBuilderParameters(anonymousFunction, out var builderParameters) ||
                builderParameters.Count != 1)
            {
                return false;
            }

            builderParameter = builderParameters.Single();
            return true;
        }

        private static bool TryGetSingleReturnedValue(IBlockOperation block, out IOperation? returnedValue)
        {
            returnedValue = null;
            if (block.Operations.Length != 1 ||
                block.Operations[0] is not IReturnOperation returnOperation)
            {
                return false;
            }

            returnedValue = returnOperation.ReturnedValue;
            return returnedValue is not null;
        }

        private static bool IsAnonymousFunctionParameter(IParameterSymbol parameter)
            => parameter.ContainingSymbol is IMethodSymbol { MethodKind: MethodKind.LambdaMethod or MethodKind.AnonymousFunction };

        private ImmutableArray<RenderFragmentLocalCarrier> GetLocalRenderFragmentCarrierSnapshot()
            => [.. _localRenderFragmentCarriers.Select(static pair => new RenderFragmentLocalCarrier(pair.Key, pair.Value))];

        private ImmutableArray<RenderFragmentMemberCarrier> GetMemberRenderFragmentCarrierSnapshot()
            => [.. _memberRenderFragmentCarriers.Select(static pair => new RenderFragmentMemberCarrier(pair.Key, pair.Value))];

        private ImmutableArray<RenderFragmentFactoryCarrier> GetFactoryRenderFragmentCarrierSnapshot()
            => [.. _factoryRenderFragmentCarriers.Select(static pair => new RenderFragmentFactoryCarrier(pair.Key, pair.Value))];

        private ImmutableArray<LocalFunctionDeclarationCarrier> GetLocalFunctionDeclarationSnapshot()
            => [.. _localFunctionDeclarationOrder.Select(declaration =>
                new LocalFunctionDeclarationCarrier(declaration.Symbol.OriginalDefinition, declaration))];

        private static Dictionary<ILocalSymbol, ParsedSlotTemplate> CreateLocalRenderFragmentCarrierDictionary(
            IEnumerable<RenderFragmentLocalCarrier> carriers)
        {
            var dictionary = new Dictionary<ILocalSymbol, ParsedSlotTemplate>(SymbolEqualityComparer.Default);
            foreach (var carrier in carriers)
                dictionary[carrier.LocalSymbol] = carrier.Template;

            return dictionary;
        }

        private static Dictionary<ISymbol, ParsedSlotTemplate> CreateMemberRenderFragmentCarrierDictionary(
            IEnumerable<RenderFragmentMemberCarrier> carriers)
        {
            var dictionary = new Dictionary<ISymbol, ParsedSlotTemplate>(SymbolEqualityComparer.Default);
            foreach (var carrier in carriers)
                dictionary[carrier.MemberSymbol] = carrier.Template;

            return dictionary;
        }

        private static Dictionary<IMethodSymbol, ParsedSlotTemplate> CreateFactoryRenderFragmentCarrierDictionary(
            IEnumerable<RenderFragmentFactoryCarrier> carriers)
        {
            var dictionary = new Dictionary<IMethodSymbol, ParsedSlotTemplate>(SymbolEqualityComparer.Default);
            foreach (var carrier in carriers)
                dictionary[carrier.MethodSymbol] = carrier.Template;

            return dictionary;
        }

        private static Dictionary<IMethodSymbol, ILocalFunctionOperation> CreateLocalFunctionDeclarationDictionary(
            IEnumerable<LocalFunctionDeclarationCarrier> carriers)
        {
            var dictionary = new Dictionary<IMethodSymbol, ILocalFunctionOperation>(SymbolEqualityComparer.Default);
            foreach (var carrier in carriers)
                dictionary[carrier.MethodSymbol] = carrier.Declaration;

            return dictionary;
        }

        private bool IsDeclaredComponentSlot(INamedTypeSymbol componentType, string parameterName)
            => TryGetDeclaredComponentSlotProperty(componentType, parameterName, out _);

        private bool TryGetDeclaredComponentSlotProperty(
            INamedTypeSymbol componentType,
            string parameterName,
            out IPropertySymbol property)
        {
            property = default!;
            if (_symbols.ParameterAttribute is null)
                return false;

            for (var current = componentType; current is not null; current = current.BaseType)
            {
                foreach (var member in current.GetMembers(parameterName))
                {
                    if (member is not IPropertySymbol candidateProperty ||
                        candidateProperty.IsStatic ||
                        !IsRenderFragmentType(candidateProperty.Type))
                    {
                        continue;
                    }

                    if (candidateProperty.GetAttributes().Any(attribute =>
                            SymbolEqualityComparer.Default.Equals(attribute.AttributeClass, _symbols.ParameterAttribute)))
                    {
                        property = candidateProperty;
                        return true;
                    }
                }
            }

            return false;
        }

        private bool IsRenderFragmentAddContent(IInvocationOperation invocation)
            => invocation.Arguments.Length >= 2 &&
               IsRenderFragmentType(invocation.TargetMethod.Parameters[1].Type);

        private bool IsTypedRenderFragmentAddContent(IInvocationOperation invocation)
            => invocation.Arguments.Length >= 3 &&
               IsRenderFragmentType(invocation.TargetMethod.Parameters[1].Type);

        private bool IsMarkupStringAddContent(IInvocationOperation invocation)
            => invocation.Arguments.Length >= 2 &&
               RazorVueStaticMarkupValueHelper.IsMarkupStringType(invocation.TargetMethod.Parameters[1].Type);

        private bool IsRenderFragmentType(ITypeSymbol? typeSymbol)
        {
            if (typeSymbol is null)
                return false;

            if (typeSymbol is INamedTypeSymbol namedType &&
                namedType.IsGenericType &&
                namedType.ConstructedFrom.SpecialType == SpecialType.System_Nullable_T)
            {
                typeSymbol = namedType.TypeArguments[0];
            }

            return IsRenderFragment(typeSymbol);
        }

        private bool IsParameterizedRenderFragmentType(ITypeSymbol? typeSymbol)
        {
            if (typeSymbol is null)
                return false;

            if (typeSymbol is INamedTypeSymbol namedType &&
                namedType.IsGenericType &&
                namedType.ConstructedFrom.SpecialType == SpecialType.System_Nullable_T)
            {
                typeSymbol = namedType.TypeArguments[0];
            }

            return typeSymbol is INamedTypeSymbol renderFragmentType &&
                   _symbols.RenderFragmentOfT is not null &&
                   SymbolEqualityComparer.Default.Equals(renderFragmentType.OriginalDefinition, _symbols.RenderFragmentOfT);
        }

        private RazorVueCompilationIssueException CreateStructuralIssue(
            IOperation operation,
            string message)
            => CreateStructuralIssue(
                operation.Syntax is null
                    ? _snapshot.Origins.FirstOrDefault() is { } origin ? ImmutableArray.Create(origin) : ImmutableArray<RazorVueSourceOrigin>.Empty
                    : CreateOrigins(operation, RazorVueOriginKind.Template),
                message);

        private RazorVueCompilationIssueException CreateStructuralIssueForUnclosedFrames()
        {
            var current = _openFrames.Peek();
            return CreateStructuralIssue(
                current.Origins,
                $"BuildRenderTree ended with {_openFrames.Count} unclosed frame(s); innermost open frame is {current.Describe()} in component '{_snapshot.Descriptor.FullName}'.");
        }

        private RazorVueCompilationIssueException CreateUnsupportedBuilderCall(
            IInvocationOperation invocation,
            string message)
            => CreateStructuralIssue(invocation, message);

        private static string GetBuilderCallDisplayName(IInvocationOperation invocation)
            => invocation.TargetMethod.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat);

        private static string GetOperationDisplay(IOperation operation)
        {
            var display = operation.Syntax?.ToString()?.Trim();
            return string.IsNullOrWhiteSpace(display)
                ? operation.Kind.ToString()
                : display!;
        }

        private sealed class RegionScope(ImmutableArray<RazorVueSourceOrigin> origins)
            : OpenFrame(origins)
        {
            public override string Describe()
                => "region";
        }

        private RazorVueCompilationIssueException CreateStructuralIssue(
            ImmutableArray<RazorVueSourceOrigin> origins,
            string message)
        {
            var issue = new RazorVueCompilationIssue(
                RazorVueIssueCode.CanonicalizationFailed,
                RazorVueIssueSeverity.Error,
                message,
                ImmutableArray<string>.Empty);
            return new RazorVueCompilationIssueException(
                issue,
                _snapshot.Descriptor.FullName,
                origins.IsDefaultOrEmpty ? _snapshot.Origins.FirstOrDefault() : origins[0]);
        }

        private static ImmutableArray<RazorVueSourceOrigin> CreateOrigins(IOperation operation, RazorVueOriginKind originKind)
            => operation.Syntax is null
                ? ImmutableArray<RazorVueSourceOrigin>.Empty
                : [RazorVueSourceOrigin.FromLocation(operation.Syntax.GetLocation(), originKind)];

        private static ImmutableArray<RazorVueCapturedValueBinding> ToCapturedValueBindings(
            ImmutableArray<RenderHelperValueBinding> bindings)
            => bindings.IsDefaultOrEmpty
                ? ImmutableArray<RazorVueCapturedValueBinding>.Empty
                : [.. bindings.Select(static binding => new RazorVueCapturedValueBinding(binding.ParameterSymbol, binding.Initializer))];

        private static string ToLowerCamelCase(string value)
        {
            if (string.IsNullOrEmpty(value))
                return value;

            if (value.Length == 1)
                return char.ToLowerInvariant(value[0]).ToString();

            if (char.IsUpper(value[0]) && char.IsUpper(value[1]))
                return value;

            return char.ToLowerInvariant(value[0]) + value.Substring(1);
        }

        private bool HasPendingImmediateAssignmentDeclarations()
            => _pendingRenderFragmentLocalCarriers.Count > 0 ||
               _pendingStaticMarkupLocalCarriers.Count > 0 ||
               _pendingComponentTypeLocalCarriers.Count > 0 ||
               _pendingTemplateScopedDeclarations.Count > 0;

        private bool IsPendingImmediateAssignment(IOperation operation)
        {
            if (Unwrap(operation) is not IExpressionStatementOperation expressionStatement ||
                Unwrap(expressionStatement.Operation) is not ISimpleAssignmentOperation assignment ||
                assignment.Target is not ILocalReferenceOperation localReference)
            {
                return false;
            }

            return _pendingRenderFragmentLocalCarriers.ContainsKey(localReference.Local) ||
                   _pendingStaticMarkupLocalCarriers.ContainsKey(localReference.Local) ||
                   _pendingComponentTypeLocalCarriers.ContainsKey(localReference.Local) ||
                   _pendingTemplateScopedDeclarations.ContainsKey(localReference.Local);
        }

        private bool IsPendingImmediateAssignmentContinuation(IOperation operation)
            => Unwrap(operation) switch
            {
                IVariableDeclarationGroupOperation declarationGroup => declarationGroup.Declarations.All(static declaration =>
                    declaration.Declarators.All(static declarator => !IsRenderTreeBuilderType(declarator.Symbol.Type))),
                IVariableDeclarationOperation declarationOperation => declarationOperation.Declarators.All(static declarator =>
                    !IsRenderTreeBuilderType(declarator.Symbol.Type)),
                _ => false
            };

        private void EnsureNoPendingImmediateAssignmentDeclarations()
        {
            if (!HasPendingImmediateAssignmentDeclarations())
                return;

            ThrowPendingImmediateAssignmentRequiresImmediateAssignment(null);
        }

        private void ThrowPendingImmediateAssignmentRequiresImmediateAssignment(IOperation? currentOperation)
        {
            string message;
            IOperation originOperation;
            if (_pendingRenderFragmentLocalCarriers.Count > 0)
            {
                var pendingDeclaration = _pendingRenderFragmentLocalCarriers.Values.First();
                originOperation = pendingDeclaration.Declarator;
                message =
                    $"RazorVue RenderFragment local '{pendingDeclaration.Declarator.Symbol.Name}' in component '{_snapshot.Descriptor.FullName}' must be assigned exactly once within the same linear local-declaration prefix by a simple assignment statement.";
            }
            else if (_pendingStaticMarkupLocalCarriers.Count > 0)
            {
                var pendingDeclaration = _pendingStaticMarkupLocalCarriers.Values.First();
                originOperation = pendingDeclaration.Declarator;
                message =
                    $"RazorVue MarkupString local '{pendingDeclaration.Declarator.Symbol.Name}' in component '{_snapshot.Descriptor.FullName}' must be assigned exactly once within the same linear local-declaration prefix by a simple assignment statement.";
            }
            else if (_pendingComponentTypeLocalCarriers.Count > 0)
            {
                var pendingDeclaration = _pendingComponentTypeLocalCarriers.Values.First();
                originOperation = pendingDeclaration.Declarator;
                message =
                    $"RazorVue System.Type local '{pendingDeclaration.Declarator.Symbol.Name}' in component '{_snapshot.Descriptor.FullName}' must be assigned exactly once within the same linear local-declaration prefix by a simple assignment statement.";
            }
            else
            {
                var pendingDeclaration = _pendingTemplateScopedDeclarations.Values.First();
                originOperation = pendingDeclaration.Declarator;
                message =
                    $"RazorVue template-scoped local '{pendingDeclaration.Declarator.Symbol.Name}' in component '{_snapshot.Descriptor.FullName}' must be assigned exactly once within the same linear local-declaration prefix by a simple assignment statement.";
            }

            throw CreateStructuralIssue(
                currentOperation ?? originOperation,
                message);
        }

        private readonly record struct PendingRenderFragmentLocalCarrierDeclaration(
            IVariableDeclaratorOperation Declarator);

        private readonly record struct PendingStaticMarkupLocalCarrierDeclaration(
            IVariableDeclaratorOperation Declarator);

        private readonly record struct PendingComponentTypeLocalCarrierDeclaration(
            IVariableDeclaratorOperation Declarator);

        private readonly record struct PendingTemplateScopedDeclaration(
            IVariableDeclaratorOperation Declarator);

    }

    private abstract class OpenFrame(ImmutableArray<RazorVueSourceOrigin> origins)
	{
		public ImmutableArray<RazorVueSourceOrigin> Origins { get; } = origins;

		public abstract string Describe();
    }

    private abstract class OpenNodeBuilder : OpenFrame
    {
        private RazorVueNodeKey? _key;
        private bool _keyAssigned;
        private readonly List<RazorVueAttributeEntry> _attributes = [];
        private readonly List<RazorVueComponentSlotTemplateNode> _slotTemplates = [];
        private readonly List<RazorVueImplicitDefaultSlotAssignmentNode> _implicitDefaultSlotAssignments = [];
        private readonly List<RazorVueRenderNode> _ambientDefaultSlotChildren = [];
        private readonly List<RazorVueRenderNode> _children = [];
        private readonly List<RazorVueOpenNodeReplayOperation> _replayOperations = [];

        protected OpenNodeBuilder(ImmutableArray<RazorVueSourceOrigin> origins)
            : base(origins)
        {
        }

        protected List<RazorVueAttributeEntry> MutableAttributes => _attributes;

        public void AddAttribute(RazorVueAttributeEntry attribute)
        {
            AddAttributeWithoutReplay(attribute);
            _replayOperations.Add(new RazorVueOpenNodeAttributeReplayOperation(attribute, attribute.Origins));
        }

        public void SetKey(
            IOperation? key,
            ImmutableArray<RazorVueCapturedValueBinding> capturedBindings,
            ImmutableArray<RazorVueSourceOrigin> origins)
        {
            _keyAssigned = true;
            _key = key is null ? null : new RazorVueNodeKey(key, capturedBindings, origins);
            _replayOperations.Add(new RazorVueOpenNodeKeyReplayOperation(_key, true, origins));
        }

        public void SetKey(
            RazorVueNodeKey? key,
            bool keyAssigned)
        {
            SetKeyWithoutReplay(key, keyAssigned);
            _replayOperations.Add(new RazorVueOpenNodeKeyReplayOperation(key, keyAssigned, key?.Origins ?? ImmutableArray<RazorVueSourceOrigin>.Empty));
        }

        public void AddSlotTemplate(RazorVueComponentSlotTemplateNode slotTemplate)
        {
            AddSlotTemplateWithoutReplay(slotTemplate);
            _replayOperations.Add(new RazorVueOpenNodeSlotTemplateReplayOperation(slotTemplate, slotTemplate.Origins));
        }

        public void AddImplicitDefaultSlotAssignment(RazorVueImplicitDefaultSlotAssignmentNode assignment)
        {
            AddImplicitDefaultSlotAssignmentWithoutReplay(assignment);
            _replayOperations.Add(new RazorVueOpenNodeImplicitDefaultSlotAssignmentReplayOperation(assignment, assignment.Origins));
        }

        public void AddAmbientDefaultSlotChild(RazorVueRenderNode child)
        {
            AddAmbientDefaultSlotChildWithoutReplay(child);
            _replayOperations.Add(new RazorVueOpenNodeAmbientDefaultSlotChildReplayOperation(child, child.Origins));
        }

        public void AddChild(RazorVueRenderNode child)
        {
            AddChildWithoutReplay(child);
            _replayOperations.Add(new RazorVueOpenNodeChildReplayOperation(child, child.Origins));
        }

        public void AddReplayOperation(RazorVueOpenNodeReplayOperation operation)
            => _replayOperations.Add(operation);

        public void AddReplayOperations(IEnumerable<RazorVueOpenNodeReplayOperation> operations)
            => _replayOperations.AddRange(operations);

        protected ImmutableArray<RazorVueAttributeEntry> BuildAttributes()
            => [.. _attributes];

        protected RazorVueNodeKey? BuildKey()
            => _key;

        protected ImmutableArray<RazorVueComponentSlotTemplateNode> BuildSlotTemplates()
            => [.. _slotTemplates];

        protected ImmutableArray<RazorVueImplicitDefaultSlotAssignmentNode> BuildImplicitDefaultSlotAssignments()
            => [.. _implicitDefaultSlotAssignments];

        protected RazorVueRenderFragment BuildAmbientDefaultSlotChildren()
            => new([.. _ambientDefaultSlotChildren]);

        protected RazorVueRenderFragment BuildChildren()
            => new([.. _children]);

        protected ImmutableArray<RazorVueOpenNodeReplayOperation> BuildReplayOperations()
            => [.. _replayOperations];

        public void AddAttributeWithoutReplay(RazorVueAttributeEntry attribute)
            => _attributes.Add(attribute);

        public virtual RazorVueEventModifiers GetEventModifiers(string eventHandlerName)
        {
            _ = eventHandlerName;
            return RazorVueEventModifiers.Empty;
        }

        public virtual void SetEventModifier(
            string eventHandlerName,
            string methodName,
            RazorVueEventModifierBinding binding,
            ImmutableArray<RazorVueSourceOrigin> origins)
        {
            _ = eventHandlerName;
            _ = methodName;
            _ = binding;
            _ = origins;
        }

        public void SetKeyWithoutReplay(
            RazorVueNodeKey? key,
            bool keyAssigned)
        {
            _keyAssigned = keyAssigned;
            _key = key;
        }

        public void AddSlotTemplateWithoutReplay(RazorVueComponentSlotTemplateNode slotTemplate)
            => _slotTemplates.Add(slotTemplate);

        public void AddImplicitDefaultSlotAssignmentWithoutReplay(RazorVueImplicitDefaultSlotAssignmentNode assignment)
            => _implicitDefaultSlotAssignments.Add(assignment);

        public void AddAmbientDefaultSlotChildWithoutReplay(RazorVueRenderNode child)
            => _ambientDefaultSlotChildren.Add(child);

        public void AddChildWithoutReplay(RazorVueRenderNode child)
            => _children.Add(child);

        public OpenNodeSnapshot CreateSnapshot()
            => new(
                Key: _key,
                KeyAssigned: _keyAssigned,
                Attributes: BuildAttributes(),
                SlotTemplates: BuildSlotTemplates(),
                ImplicitDefaultSlotAssignments: BuildImplicitDefaultSlotAssignments(),
                AmbientDefaultSlotChildren: BuildAmbientDefaultSlotChildren(),
                Children: BuildChildren(),
                ReplayOperations: BuildReplayOperations());

        public abstract override string Describe();

        public abstract OpenNodeBuilder CreateEmptyClone();

        public abstract RazorVueRenderNode Build();
    }

    private readonly record struct OpenNodeSnapshot(
        RazorVueNodeKey? Key,
        bool KeyAssigned,
        ImmutableArray<RazorVueAttributeEntry> Attributes,
        ImmutableArray<RazorVueComponentSlotTemplateNode> SlotTemplates,
        ImmutableArray<RazorVueImplicitDefaultSlotAssignmentNode> ImplicitDefaultSlotAssignments,
        RazorVueRenderFragment AmbientDefaultSlotChildren,
        RazorVueRenderFragment Children,
        ImmutableArray<RazorVueOpenNodeReplayOperation> ReplayOperations);

    private sealed class ElementBuilder(string tagName, ImmutableArray<RazorVueSourceOrigin> origins)
        : OpenNodeBuilder(origins)
    {
        private readonly Dictionary<string, RazorVueEventModifiers> _eventModifiersByHandlerName = new(StringComparer.Ordinal);

        private ElementBuilder(
            string tagName,
            ImmutableArray<RazorVueSourceOrigin> origins,
            IReadOnlyDictionary<string, RazorVueEventModifiers> eventModifiersByHandlerName)
            : this(tagName, origins)
        {
            foreach (var pair in eventModifiersByHandlerName)
                _eventModifiersByHandlerName.Add(pair.Key, pair.Value);
        }

        public override string Describe()
            => $"element <{tagName}>";

        public override OpenNodeBuilder CreateEmptyClone()
            => new ElementBuilder(tagName, Origins, _eventModifiersByHandlerName);

        public override RazorVueRenderNode Build()
            => new RazorVueElementNode(tagName, BuildKey(), BuildAttributes(), BuildChildren(), Origins)
            {
                ReplayOperations = BuildReplayOperations()
            };

        public override RazorVueEventModifiers GetEventModifiers(string eventHandlerName)
            => _eventModifiersByHandlerName.TryGetValue(eventHandlerName, out var modifiers)
                ? modifiers
                : RazorVueEventModifiers.Empty;

        public override void SetEventModifier(
            string eventHandlerName,
            string methodName,
            RazorVueEventModifierBinding binding,
            ImmutableArray<RazorVueSourceOrigin> origins)
        {
            var current = GetEventModifiers(eventHandlerName);
            var isConstantFalse = IsConstantFalseEventModifier(binding.Value);
            var updated = methodName switch
            {
                "AddEventPreventDefaultAttribute" => current with { PreventDefault = isConstantFalse ? null : binding },
                "AddEventStopPropagationAttribute" => current with { StopPropagation = isConstantFalse ? null : binding },
                _ => current
            };
            if (current == updated)
                return;

            if (updated.HasAny)
                _eventModifiersByHandlerName[eventHandlerName] = updated;
            else
                _eventModifiersByHandlerName.Remove(eventHandlerName);

            UpdateExistingAttribute(eventHandlerName, updated);
            AddReplayOperation(new RazorVueOpenNodeEventModifierReplayOperation(
                eventHandlerName,
                updated,
                origins));
        }

        public void ApplyEventModifierReplayWithoutReplay(RazorVueOpenNodeEventModifierReplayOperation operation)
            => ApplyEventModifiersWithoutReplay(operation.EventHandlerName, operation.EventModifiers);

        private void ApplyEventModifiersWithoutReplay(string eventHandlerName, RazorVueEventModifiers modifiers)
        {
            if (modifiers.HasAny)
                _eventModifiersByHandlerName[eventHandlerName] = modifiers;
            else
                _eventModifiersByHandlerName.Remove(eventHandlerName);

            UpdateExistingAttribute(eventHandlerName, modifiers);
        }

        private void UpdateExistingAttribute(string eventHandlerName, RazorVueEventModifiers modifiers)
        {
            var attributes = MutableAttributes;
            for (var index = attributes.Count - 1; index >= 0; index--)
            {
                if (attributes[index] is RazorVueAttributeNode attribute &&
                    string.Equals(attribute.Name, eventHandlerName, StringComparison.Ordinal))
                {
                    attributes[index] = attribute with { EventModifiers = modifiers };
                    return;
                }
            }
        }

        private static bool IsConstantFalseEventModifier(IOperation? operation)
        {
            var current = RazorVueOperationNormalizer.Unwrap(operation);
            return current?.ConstantValue.HasValue == true &&
                   current.ConstantValue.Value is bool value &&
                   !value;
        }
    }

    private sealed class ComponentBuilder(string componentName, string componentFullName, string resolutionName, INamedTypeSymbol componentType, ImmutableArray<RazorVueSourceOrigin> origins)
        : OpenNodeBuilder(origins)
    {
        public string ComponentFullName { get; } = componentFullName;

        public INamedTypeSymbol ComponentType { get; } = componentType;

        public override string Describe()
            => $"component '{ComponentFullName}'";

        public override OpenNodeBuilder CreateEmptyClone()
            => new ComponentBuilder(componentName, ComponentFullName, resolutionName, ComponentType, Origins);

        public override RazorVueRenderNode Build()
            => new RazorVueComponentNode(componentName, ComponentFullName, resolutionName, BuildKey(), BuildAttributes(), BuildSlotTemplates(), BuildImplicitDefaultSlotAssignments(), BuildAmbientDefaultSlotChildren(), BuildChildren(), Origins)
            {
                ReplayOperations = BuildReplayOperations()
            };
    }
}
