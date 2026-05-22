using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Jazor.RazorVue.Artifacts;
using Jazor.RazorVue.Descriptor;
using Jazor.RazorVue.Extensibility;
using Jazor.RazorVue.RenderTree;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Operations;
using Microsoft.CodeAnalysis.Text;
using static Jazor.RazorVue.RazorVueOperationNormalizer;

namespace Jazor.RazorVue.RazorSdk;

internal sealed class RazorVueRazorIrTemplateFrontend : IRazorVueTemplateFrontend
{
    public string Name => "Jazor.RazorVue.RazorSdk.RazorVueRazorIrTemplateFrontend";

    public RazorVueRenderFragment CreateRenderTree(
        Jazor.RazorVue.RazorVueCompilationContext context,
        RazorVueSemanticSnapshot snapshot)
    {
        if (context is null)
            throw new ArgumentNullException(nameof(context));
        if (snapshot is null)
            throw new ArgumentNullException(nameof(snapshot));

        if (!TryCreateRenderTree(context, snapshot, out var renderTree))
        {
            throw new InvalidOperationException(
                $"RazorVue Razor IR frontend requires a bound Razor document for component '{snapshot.Descriptor.FullName}'.");
        }

        return renderTree;
    }

    internal bool TryCreateRenderTree(
        Jazor.RazorVue.RazorVueCompilationContext context,
        RazorVueSemanticSnapshot snapshot,
        out RazorVueRenderFragment renderTree)
    {
        if (context is null)
            throw new ArgumentNullException(nameof(context));
        if (snapshot is null)
            throw new ArgumentNullException(nameof(snapshot));

        renderTree = RazorVueRenderFragment.Empty;
        if (snapshot.RazorSourceGeneratorDocument is not { } sourceGeneratorDocument)
            return false;

        renderTree = CreateRenderTreeCore(context, snapshot, sourceGeneratorDocument);
        return true;
    }

    private static RazorVueRenderFragment CreateRenderTreeCore(
        Jazor.RazorVue.RazorVueCompilationContext context,
        RazorVueSemanticSnapshot snapshot,
        RazorVueRazorSourceGeneratorDocument document)
    {
        var resolver = new RazorVueRazorIrOperationResolver(context, snapshot, document);
        var converter = new Converter(context, snapshot, resolver);
        return converter.Convert(document.DocumentNode);
    }

    internal static RazorVueSourceOrigin? CreateSourceOrigin(RazorVueRazorSourceSpan? sourceSpan, RazorVueOriginKind originKind)
    {
        if (sourceSpan is null || string.IsNullOrWhiteSpace(sourceSpan.Value.FilePath))
            return null;

        var sourceFilePath = sourceSpan.Value.FilePath!;
        return new RazorVueSourceOrigin(
            OriginKind: originKind,
            SourceFilePath: sourceFilePath,
            SourceSpanStart: sourceSpan.Value.AbsoluteIndex,
            SourceSpanLength: sourceSpan.Value.Length,
            StartLine: sourceSpan.Value.LineIndex + 1,
            StartColumn: sourceSpan.Value.CharacterIndex + 1,
            GeneratedFilePath: null,
            GeneratedSpanStart: null,
            GeneratedSpanLength: null,
            MappingQuality: RazorVueMappingQuality.ExactSource,
            Provenance: RazorVueOriginProvenance.RazorSourceMap);
    }

    private sealed class Converter(
        Jazor.RazorVue.RazorVueCompilationContext context,
        RazorVueSemanticSnapshot snapshot,
        RazorVueRazorIrOperationResolver resolver)
    {
        private static readonly ImmutableHashSet<ILocalSymbol> EmptyLocalScope =
            ImmutableHashSet<ILocalSymbol>.Empty.WithComparer(SymbolEqualityComparer.Default);
        private static readonly ImmutableHashSet<IParameterSymbol> EmptyParameterScope =
            ImmutableHashSet<IParameterSymbol>.Empty.WithComparer(SymbolEqualityComparer.Default);
        private static readonly HashSet<string> VoidElementNames = new(StringComparer.OrdinalIgnoreCase)
        {
            "area",
            "base",
            "br",
            "col",
            "embed",
            "hr",
            "img",
            "input",
            "link",
            "meta",
            "param",
            "source",
            "track",
            "wbr"
        };

        private readonly Jazor.RazorVue.RazorVueCompilationContext _context = context;
        private readonly RazorVueSemanticSnapshot _snapshot = snapshot;
        private readonly RazorVueRazorIrOperationResolver _resolver = resolver;
        private readonly Dictionary<string, IOperation> _literalStringOperationCache = new(StringComparer.Ordinal);
        private readonly Dictionary<string, int> _elementAttributeOrdinals = new(StringComparer.Ordinal);
        private readonly Dictionary<string, int> _componentAttributeOrdinals = new(StringComparer.Ordinal);
        private readonly Dictionary<ILocalSymbol, IOperation> _localStaticMarkupCarriers =
            new(SymbolEqualityComparer.Default);
        private readonly Dictionary<ILocalSymbol, ParsedSlotTemplate> _localRenderFragmentCarriers =
            new(SymbolEqualityComparer.Default);
        private readonly Dictionary<ISymbol, ParsedSlotTemplate> _memberRenderFragmentCarriers =
            new(SymbolEqualityComparer.Default);
        private readonly Dictionary<IMethodSymbol, ParsedSlotTemplate> _factoryRenderFragmentCarriers =
            new(SymbolEqualityComparer.Default);
        private readonly HashSet<ISymbol> _activeRenderFragmentMembers =
            new(SymbolEqualityComparer.Default);
        private readonly HashSet<IMethodSymbol> _activeRenderFragmentFactories =
            new(SymbolEqualityComparer.Default);
        private readonly HashSet<ConsumedTemplateNodeKey> _consumedTemplateNodes = [];
        private bool _lastTemplateDeclarationScanEncounteredSupplementalDeclarations;
        private RazorVueRazorIrNode? _documentRoot;
        private int _elementKeyOrdinal;
        private int _componentKeyOrdinal;

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

        private readonly record struct RenderHelperValueBinding(
            IParameterSymbol ParameterSymbol,
            IOperation Initializer);

        private readonly record struct ConsumedTemplateNodeKey(
            string FilePath,
            int Start,
            int Length);

        public RazorVueRenderFragment Convert(RazorVueRazorIrNode document)
        {
            if (document is null)
                throw new ArgumentNullException(nameof(document));

            _documentRoot = document;
            return ConvertLooseNodes(document.Children, insideTemplate: false, allowImperativePromotion: false, EmptyLocalScope, EmptyParameterScope);
        }

        private RazorVueRenderFragment ConvertLooseNodes(
            IEnumerable<RazorVueRazorIrNode> nodes,
            bool insideTemplate,
            bool allowImperativePromotion,
            ImmutableHashSet<ILocalSymbol> allowedLocalSymbols,
            ImmutableHashSet<IParameterSymbol> allowedParameterSymbols)
        {
            if (insideTemplate)
                return ConvertTemplateMethodBody(nodes, allowImperativePromotion, allowedLocalSymbols, allowedParameterSymbols);

            var builder = ImmutableArray.CreateBuilder<RazorVueRenderNode>();

            foreach (var node in nodes)
            {
                if (IsConsumedTemplateExtensionNode(node) || IsTemplateIntermediateNode(node))
                    continue;

                switch (node.Kind)
                {
                    case RazorVueRazorIrNodeKind.MarkupElement:
                        builder.Add(ConvertElement(node, allowedLocalSymbols, allowedParameterSymbols));
                        break;
                    case RazorVueRazorIrNodeKind.Component:
                        builder.Add(ConvertComponent(node, allowedLocalSymbols, allowedParameterSymbols));
                        break;
                    case RazorVueRazorIrNodeKind.HtmlContent:
                        AppendHtmlContent(builder, node);
                        break;
                    case RazorVueRazorIrNodeKind.CSharpExpression:
                        if (TryConvertStaticMarkupExpression(node, out var staticMarkupNodes))
                        {
                            builder.AddRange(staticMarkupNodes);
                            break;
                        }

                        builder.AddRange(ConvertExpressionOrSlotOutlet(node).Children);
                        break;
                    case RazorVueRazorIrNodeKind.Document:
                        builder.AddRange(ConvertLooseNodes(node.Children, insideTemplate, allowImperativePromotion, allowedLocalSymbols, allowedParameterSymbols).Children);
                        break;
                    case RazorVueRazorIrNodeKind.NamespaceDeclaration:
                        builder.AddRange(ConvertLooseNodes(node.Children, insideTemplate, allowImperativePromotion, allowedLocalSymbols, allowedParameterSymbols).Children);
                        break;
                    case RazorVueRazorIrNodeKind.ClassDeclaration:
                        builder.AddRange(ConvertLooseNodes(node.Children, insideTemplate, allowImperativePromotion, allowedLocalSymbols, allowedParameterSymbols).Children);
                        break;
                    case RazorVueRazorIrNodeKind.MethodDeclaration:
                        builder.AddRange(ConvertMethodDeclaration(node, allowedLocalSymbols, allowedParameterSymbols).Children);
                        break;
                    case RazorVueRazorIrNodeKind.MarkupBlock:
                        AppendMarkupBlock(builder, node, allowedLocalSymbols, allowedParameterSymbols);
                        break;
                    case RazorVueRazorIrNodeKind.TagHelperBody:
                        builder.AddRange(ConvertTemplateMethodBody(node.Children, allowImperativePromotion: false, allowedLocalSymbols, allowedParameterSymbols).Children);
                        break;
                    case RazorVueRazorIrNodeKind.CSharpCode:
                    case RazorVueRazorIrNodeKind.FieldDeclaration:
                    case RazorVueRazorIrNodeKind.PropertyDeclaration:
                    case RazorVueRazorIrNodeKind.UsingDirective:
                    case RazorVueRazorIrNodeKind.Directive:
                    case RazorVueRazorIrNodeKind.MalformedDirective:
                    case RazorVueRazorIrNodeKind.Extension:
                        break;
                    case RazorVueRazorIrNodeKind.TagHelper:
                        throw CreateUnsupportedNodeException(node, "TagHelperIntermediateNode");
                    default:
                        if (IsConsumedTemplateExtensionNode(node))
                            break;

                        if (node.Children.Length > 0)
                        {
                            builder.AddRange(ConvertLooseNodes(node.Children, insideTemplate, allowImperativePromotion, allowedLocalSymbols, allowedParameterSymbols).Children);
                            break;
                        }

                        break;
                }
            }

            return new RazorVueRenderFragment(builder.ToImmutable());
        }

        private RazorVueRenderFragment ConvertMethodDeclaration(
            RazorVueRazorIrNode node,
            ImmutableHashSet<ILocalSymbol> allowedLocalSymbols,
            ImmutableHashSet<IParameterSymbol> allowedParameterSymbols)
        {
            if (node is null)
                throw new ArgumentNullException(nameof(node));

            if (!string.Equals(GetMethodDeclarationName(node), "BuildRenderTree", StringComparison.Ordinal))
                return ConvertLooseNodes(node.Children, insideTemplate: false, allowImperativePromotion: false, allowedLocalSymbols, allowedParameterSymbols);

            return ConvertTemplateMethodBody(node.Children, allowImperativePromotion: true, allowedLocalSymbols, allowedParameterSymbols);
        }

        private RazorVueRenderFragment ConvertTemplateMethodBody(
            IEnumerable<RazorVueRazorIrNode> nodes,
            bool allowImperativePromotion,
            ImmutableHashSet<ILocalSymbol>? allowedLocalSymbols = null,
            ImmutableHashSet<IParameterSymbol>? allowedParameterSymbols = null)
        {
            var bufferedNodes = nodes.ToList();
            if (bufferedNodes.Count == 0)
                return RazorVueRenderFragment.Empty;

            if (allowImperativePromotion &&
                TryPromoteTemplateMethodBodyToImperative(bufferedNodes, out var imperativeBody))
                return imperativeBody;

            var currentLocalScope = allowedLocalSymbols ?? EmptyLocalScope;
            var currentParameterScope = allowedParameterSymbols ?? EmptyParameterScope;
            PendingTemplateControlNode? pendingTemplateControlNode = null;
            var builder = ImmutableArray.CreateBuilder<RazorVueRenderNode>();
            var index = 0;
            while (index < bufferedNodes.Count)
            {
                var node = bufferedNodes[index];
                if (IsConsumedTemplateExtensionNode(node))
                {
                    index++;
                    continue;
                }

                if (TryConvertConditional(bufferedNodes, ref index, currentLocalScope, currentParameterScope, out var conditionalNode))
                {
                    builder.Add(conditionalNode);
                    continue;
                }

                if (TryConvertForEach(bufferedNodes, ref index, currentLocalScope, currentParameterScope, out var loopNode))
                {
                    builder.Add(loopNode);
                    continue;
                }

                if (TryConvertFor(bufferedNodes, ref index, currentLocalScope, currentParameterScope, out var forNode))
                {
                    builder.Add(forNode);
                    continue;
                }

                if (TryConvertTemplateLocalCodeBlock(
                        bufferedNodes,
                        ref index,
                        currentLocalScope,
                        currentParameterScope,
                        out var localDeclarations,
                        out var emittedPendingControlNode))
                {
                    foreach (var localDeclaration in localDeclarations)
                    {
                        builder.Add(localDeclaration);
                        currentLocalScope = currentLocalScope.Add(localDeclaration.LocalSymbol);
                    }

                    pendingTemplateControlNode = emittedPendingControlNode;
                    continue;
                }

                if (TryConvertPendingTemplateControlNode(
                        pendingTemplateControlNode,
                        bufferedNodes,
                        ref index,
                        currentLocalScope,
                        currentParameterScope,
                        out var pendingControlNode))
                {
                    builder.Add(pendingControlNode);
                    pendingTemplateControlNode = null;
                    continue;
                }

                switch (node.Kind)
                {
                    case RazorVueRazorIrNodeKind.MarkupElement:
                        builder.Add(ConvertElement(node, currentLocalScope, currentParameterScope));
                        index++;
                        break;
                    case RazorVueRazorIrNodeKind.Component:
                        builder.Add(ConvertComponent(node, currentLocalScope, currentParameterScope));
                        index++;
                        break;
                    case RazorVueRazorIrNodeKind.HtmlContent:
                        AppendHtmlContent(builder, node);
                        index++;
                        break;
                    case RazorVueRazorIrNodeKind.CSharpExpression:
                        if (TryConvertStaticMarkupExpression(node, out var staticMarkupNodes))
                        {
                            builder.AddRange(staticMarkupNodes);
                            index++;
                            break;
                        }

                        builder.AddRange(ConvertExpressionOrSlotOutlet(node).Children);
                        index++;
                        break;
                    case RazorVueRazorIrNodeKind.MarkupBlock:
                        AppendMarkupBlock(builder, node, currentLocalScope, currentParameterScope);
                        index++;
                        break;
                    case RazorVueRazorIrNodeKind.TagHelperBody:
                        builder.AddRange(ConvertTemplateMethodBody(node.Children, allowImperativePromotion: false, currentLocalScope, currentParameterScope).Children);
                        index++;
                        break;
                    case RazorVueRazorIrNodeKind.CSharpCode:
                        if (!IsIgnorableTemplateCodeNode(node))
                            throw CreateUnsupportedNodeException(node, "unbound template CSharpCodeIntermediateNode");
                        index++;
                        break;
                    case RazorVueRazorIrNodeKind.IntermediateToken when IsCSharpIntermediateToken(node):
                        if (!IsIgnorableTemplateCodeNode(node))
                            throw CreateUnsupportedNodeException(node, "unbound template CSharpIntermediateToken");
                        index++;
                        break;
                    case RazorVueRazorIrNodeKind.FieldDeclaration:
                    case RazorVueRazorIrNodeKind.PropertyDeclaration:
                    case RazorVueRazorIrNodeKind.UsingDirective:
                    case RazorVueRazorIrNodeKind.Directive:
                    case RazorVueRazorIrNodeKind.MalformedDirective:
                    case RazorVueRazorIrNodeKind.Extension:
                        index++;
                        break;
                    case RazorVueRazorIrNodeKind.TagHelper:
                        throw CreateUnsupportedNodeException(node, "TagHelperIntermediateNode");
                    default:
                        if (node.Children.Length > 0)
                        {
                            builder.AddRange(ConvertTemplateMethodBody(node.Children, allowImperativePromotion: false, currentLocalScope, currentParameterScope).Children);
                        }

                        index++;
                        break;
                }
            }

            return new RazorVueRenderFragment(builder.ToImmutable());
        }

        private bool TryPromoteTemplateMethodBodyToImperative(
            IReadOnlyList<RazorVueRazorIrNode> nodes,
            out RazorVueRenderFragment fragment)
        {
            fragment = RazorVueRenderFragment.Empty;
            if (!_resolver.TryResolveBuildRenderTreeBodyOperation(out var bodyOperation))
                return false;

            if (bodyOperation is IBlockOperation block)
            {
                if (RazorVueImperativeRenderSegmentationPlanner.TryPlanLocalSegments(block.Operations, out var segments) &&
                    TryCreateLocallyPromotedImperativeFragment(nodes, block.Operations, segments, out fragment))
                {
                    return true;
                }

                if (!RazorVueImperativeRenderPromotionAnalyzer.ShouldPromoteBody(block.Operations))
                    return false;

                fragment = CreateImperativeBodyFragment(
                    block.Operations,
                    RazorVueImperativeRenderPromotionAnalyzer.ClassifyBodyKind(block.Operations),
                    CreateImperativeOrigins(block.Operations, nodes));
                return true;
            }

            if (!RazorVueImperativeRenderPromotionAnalyzer.ShouldPromoteBody([bodyOperation]))
                return false;

            fragment = CreateImperativeBodyFragment(
                [bodyOperation],
                RazorVueImperativeRenderPromotionAnalyzer.ClassifyBodyKind([bodyOperation]),
                CreateImperativeOrigins([bodyOperation], nodes));
            return true;
        }

        private bool TryCreateLocallyPromotedImperativeFragment(
            IReadOnlyList<RazorVueRazorIrNode> nodes,
            IReadOnlyList<IOperation> operations,
            ImmutableArray<RazorVueImperativeRenderSegmentationPlanner.PlannedSegment> segments,
            out RazorVueRenderFragment fragment)
        {
            fragment = RazorVueRenderFragment.Empty;
            if (segments.IsDefaultOrEmpty)
                return false;

            var children = ImmutableArray.CreateBuilder<RazorVueRenderNode>();
            var currentLocalScope = EmptyLocalScope;
            var currentParameterScope = EmptyParameterScope;
            var orderedVisibleLocals = new List<ILocalSymbol>();
            var nextNodeIndex = 0;

            foreach (var segment in segments)
            {
                if (!TryResolveOperationSegmentSourceRange(operations, segment, out var segmentRange) ||
                    !TryFindSegmentNodeBounds(nodes, nextNodeIndex, segmentRange, out var segmentStartIndex, out var segmentEndExclusive))
                {
                    return false;
                }

                AppendConvertedTemplateSlice(
                    nodes,
                    nextNodeIndex,
                    segmentStartIndex,
                    children,
                    ref currentLocalScope,
                    currentParameterScope,
                    orderedVisibleLocals);

                var segmentOperations = operations
                    .Skip(segment.StartIndex)
                    .Take(segment.EndExclusive - segment.StartIndex)
                    .ToImmutableArray();
                var coveredNodes = nodes
                    .Skip(segmentStartIndex)
                    .Take(segmentEndExclusive - segmentStartIndex)
                    .ToArray();

                children.Add(CreateImperativeBlockNode(
                    segmentOperations,
                    segment.Kind,
                    CreateImperativeOrigins(segmentOperations, coveredNodes),
                    orderedVisibleLocals));
                nextNodeIndex = segmentEndExclusive;
            }

            AppendConvertedTemplateSlice(
                nodes,
                nextNodeIndex,
                nodes.Count,
                children,
                ref currentLocalScope,
                currentParameterScope,
                orderedVisibleLocals);

            fragment = new RazorVueRenderFragment(children.ToImmutable());
            return true;
        }

        private void AppendConvertedTemplateSlice(
            IReadOnlyList<RazorVueRazorIrNode> nodes,
            int startIndex,
            int endExclusive,
            ImmutableArray<RazorVueRenderNode>.Builder children,
            ref ImmutableHashSet<ILocalSymbol> currentLocalScope,
            ImmutableHashSet<IParameterSymbol> currentParameterScope,
            List<ILocalSymbol> orderedVisibleLocals)
        {
            if (endExclusive <= startIndex)
                return;

            var slice = nodes.Skip(startIndex).Take(endExclusive - startIndex).ToArray();
            if (slice.Length == 0)
                return;

            var fragment = ConvertTemplateMethodBody(
                slice,
                allowImperativePromotion: false,
                currentLocalScope,
                currentParameterScope);
            children.AddRange(fragment.Children);

            foreach (var localDeclaration in fragment.Children.OfType<RazorVueLocalDeclarationNode>())
            {
                if (currentLocalScope.Contains(localDeclaration.LocalSymbol))
                    continue;

                currentLocalScope = currentLocalScope.Add(localDeclaration.LocalSymbol);
                orderedVisibleLocals.Add(localDeclaration.LocalSymbol);
            }
        }

        private bool TryResolveOperationSegmentSourceRange(
            IReadOnlyList<IOperation> operations,
            RazorVueImperativeRenderSegmentationPlanner.PlannedSegment segment,
            out RazorVueRazorIrOperationResolver.SourceRange sourceRange)
        {
            sourceRange = default;
            var segmentOperations = operations
                .Skip(segment.StartIndex)
                .Take(segment.EndExclusive - segment.StartIndex)
                .ToArray();
            if (segmentOperations.Length == 0)
                return false;

            var mappedRanges = segmentOperations
                .Select(TryMapOperationToSourceRange)
                .Where(static range => range is not null)
                .Select(static range => range!.Value)
                .ToArray();
            if (mappedRanges.Length == 0)
                return false;

            var filePath = mappedRanges[0].FilePath;
            if (mappedRanges.Any(range => !PathsEqual(range.FilePath, filePath)))
                return false;

            sourceRange = new RazorVueRazorIrOperationResolver.SourceRange(
                filePath,
                mappedRanges.Min(static range => range.Start),
                mappedRanges.Max(static range => range.End));
            return true;
        }

        private RazorVueRazorIrOperationResolver.SourceRange? TryMapOperationToSourceRange(IOperation operation)
        {
            if (!_resolver.TryMapGeneratedOperationToOriginalSourceSpan(operation, out var sourceSpan) ||
                string.IsNullOrWhiteSpace(sourceSpan.FilePath))
            {
                return null;
            }

            return new RazorVueRazorIrOperationResolver.SourceRange(
                NormalizeComparablePath(sourceSpan.FilePath),
                sourceSpan.AbsoluteIndex,
                sourceSpan.AbsoluteIndex + sourceSpan.Length);
        }

        private static bool TryFindSegmentNodeBounds(
            IReadOnlyList<RazorVueRazorIrNode> nodes,
            int searchStartIndex,
            RazorVueRazorIrOperationResolver.SourceRange segmentRange,
            out int startIndex,
            out int endExclusive)
        {
            startIndex = -1;
            endExclusive = -1;

            for (var index = searchStartIndex; index < nodes.Count; index++)
            {
                var nodeRange = TryGetNodeSourceRange(nodes[index]);
                if (nodeRange is null ||
                    !PathsEqual(nodeRange.Value.FilePath, segmentRange.FilePath) ||
                    !RangesOverlap(nodeRange.Value, segmentRange))
                {
                    continue;
                }

                if (startIndex < 0)
                    startIndex = index;

                endExclusive = index + 1;
            }

            return startIndex >= 0;
        }

        private ImmutableArray<RazorVueSourceOrigin> CreateImperativeOrigins(
            IEnumerable<IOperation> operations,
            IReadOnlyList<RazorVueRazorIrNode> nodes)
        {
            var builder = ImmutableArray.CreateBuilder<RazorVueSourceOrigin>();
            foreach (var operation in operations)
            {
                if (!_resolver.TryMapGeneratedOperationToOriginalSourceSpan(operation, out var sourceSpan))
                    continue;

                var origin = CreateSourceOrigin(sourceSpan, RazorVueOriginKind.Template);
                if (origin is not null)
                    builder.Add(origin);
            }

            if (builder.Count > 0)
                return builder.ToImmutable();

            foreach (var node in nodes)
            {
                var origins = CreateOrigins(GetBestSourceSpan(node));
                if (!origins.IsDefaultOrEmpty)
                    return origins;
            }

            return ImmutableArray<RazorVueSourceOrigin>.Empty;
        }

        private static RazorVueRenderFragment CreateImperativeBodyFragment(
            IReadOnlyList<IOperation> operations,
            RazorVueImperativeBlockKind kind,
            ImmutableArray<RazorVueSourceOrigin> origins)
        {
            var visibleLocals = CollectVisibleLocals(operations);
            var visibleParameters = CollectVisibleParameters(operations);

            return new RazorVueRenderFragment(
            [
                new RazorVueImperativeBlockNode(
                    [.. operations],
                    kind,
                    visibleLocals,
                    visibleParameters,
                    origins)
            ]);
        }

        private static RazorVueImperativeBlockNode CreateImperativeBlockNode(
            ImmutableArray<IOperation> operations,
            RazorVueImperativeBlockKind kind,
            ImmutableArray<RazorVueSourceOrigin> origins,
            IEnumerable<ILocalSymbol> outerVisibleLocals)
            => new(
                operations,
                kind,
                CollectVisibleLocals(operations, outerVisibleLocals),
                CollectVisibleParameters(operations),
                origins);

        private static ImmutableArray<ILocalSymbol> CollectVisibleLocals(
            IEnumerable<IOperation> operations,
            IEnumerable<ILocalSymbol>? additionalVisibleLocals = null)
        {
            var builder = ImmutableArray.CreateBuilder<ILocalSymbol>();
            var seen = new HashSet<ILocalSymbol>(SymbolEqualityComparer.Default);

            if (additionalVisibleLocals is not null)
            {
                foreach (var local in additionalVisibleLocals)
                {
                    if (seen.Add(local))
                        builder.Add(local);
                }
            }

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

        private static ImmutableArray<IParameterSymbol> CollectVisibleParameters(IEnumerable<IOperation> operations)
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

        private RazorVueElementNode ConvertElement(
            RazorVueRazorIrNode node,
            ImmutableHashSet<ILocalSymbol> allowedLocalSymbols,
            ImmutableHashSet<IParameterSymbol> allowedParameterSymbols)
        {
            RejectElementExtensions(node.CapturesOrEmpty, "ReferenceCaptureIntermediateNode");

            var key = ResolveElementKey(node);
            var attributes = ImmutableArray.CreateBuilder<RazorVueAttributeEntry>();
            foreach (var attribute in node.AttributesOrEmpty)
            {
                if (IsKeyAttribute(attribute))
                    continue;

                attributes.Add(ConvertHtmlAttributeEntry(attribute));
            }
            foreach (var splat in node.BodyOrEmpty.Where(static child => child.Kind == RazorVueRazorIrNodeKind.Splat))
                attributes.Add(ConvertSplatAttribute(splat));
            var children = ConvertTemplateMethodBody(node.BodyOrEmpty, allowImperativePromotion: false, allowedLocalSymbols, allowedParameterSymbols);

            return new RazorVueElementNode(
                node.TagName ?? string.Empty,
                key,
                attributes.ToImmutable(),
                children,
                CreateOrigins(node.Source));
        }

        private RazorVueComponentNode ConvertComponent(
            RazorVueRazorIrNode node,
            ImmutableHashSet<ILocalSymbol> allowedLocalSymbols,
            ImmutableHashSet<IParameterSymbol> allowedParameterSymbols)
        {
            RejectComponentExtensions(node.CapturesOrEmpty, "ReferenceCaptureIntermediateNode");

            var key = ResolveComponentKey(node);
            var attributes = ImmutableArray.CreateBuilder<RazorVueAttributeEntry>();
            var children = ImmutableArray.CreateBuilder<RazorVueRenderNode>();
            var slotTemplates = ImmutableArray.CreateBuilder<RazorVueComponentSlotTemplateNode>();
            var implicitDefaultSlotAssignments = ImmutableArray.CreateBuilder<RazorVueImplicitDefaultSlotAssignmentNode>();
            foreach (var attribute in node.AttributesOrEmpty)
            {
                if (attribute.IsDesignTimePropertyAccessHelper)
                    continue;
                if (attribute.IsSynthesized && attribute.Source is null)
                    continue;
                if (IsKeyAttribute(attribute))
                    continue;

                var attributeName = attribute.AttributeName ?? string.Empty;
                var attributeOrigins = CreateOrigins(attribute.Source);
                var attributeValue = ResolveAttributeValue(
                    attributeName,
                    attribute.Children,
                    attribute.Source,
                    attribute,
                    builderMethodName: "AddComponentParameter",
                    builderAttributeOrdinal: GetNextComponentAttributeOrdinal(attributeName));
                if (TryConvertStoredLocalCarrierComponentSlotTemplate(
                        attributeName,
                        attribute.TypeName,
                        attributeValue,
                        attributeOrigins,
                        children,
                        slotTemplates))
                {
                    continue;
                }

                attributes.Add(new RazorVueAttributeNode(
                    attributeName,
                    attributeValue,
                    ImmutableArray<RazorVueCapturedValueBinding>.Empty,
                    attributeOrigins));
            }
            foreach (var splat in node.SplatsOrEmpty)
                attributes.Add(ConvertSplatAttribute(splat));
            foreach (var childContent in node.ChildContentsOrEmpty)
            {
                var slotFragment = ConvertTemplateMethodBody(childContent.Children, allowImperativePromotion: false, allowedLocalSymbols, allowedParameterSymbols);
                var attributeName = childContent.AttributeName ?? string.Empty;
                if (string.Equals(attributeName, "ChildContent", StringComparison.Ordinal))
                {
                    implicitDefaultSlotAssignments.Add(new RazorVueImplicitDefaultSlotAssignmentNode(
                        slotFragment,
                        CreateOrigins(childContent.Source ?? node.Source ?? node.StartTagSpan)));
                    children.AddRange(slotFragment.Children);
                    continue;
                }

                var slotName = ToLowerCamelCase(attributeName);
                slotTemplates.Add(new RazorVueComponentSlotTemplateNode(
                    PublicName: attributeName,
                    SlotName: slotName,
                    ParameterName: childContent.IsParameterized
                        ? childContent.ParameterName
                        : null,
                    ParameterSymbol: null,
                    Children: slotFragment,
                    Origins: CreateOrigins(childContent.Source ?? node.Source ?? node.StartTagSpan)));
            }

            return new RazorVueComponentNode(
                GetComponentName(node),
                NormalizeTypeName(node.TypeName),
                string.IsNullOrWhiteSpace(node.TagName) ? GetComponentName(node) : node.TagName!,
                key,
                attributes.ToImmutable(),
                slotTemplates.ToImmutable(),
                implicitDefaultSlotAssignments.ToImmutable(),
                new RazorVueRenderFragment(children.ToImmutable()),
                new RazorVueRenderFragment(children.ToImmutable()),
                CreateOrigins(node.Source is null ? node.StartTagSpan : node.Source));
        }

        private RazorVueExpressionNode ConvertExpression(RazorVueRazorIrNode node)
        {
            var sourceSpan = GetRequiredSourceSpan(node, "CSharpExpressionIntermediateNode");
            var operation = _resolver.ResolveRequiredOperation(sourceSpan, "Razor IR body expression");
            return new RazorVueExpressionNode(operation, CreateOrigins(sourceSpan));
        }

        private bool TryConvertStaticMarkupExpression(
            RazorVueRazorIrNode node,
            out ImmutableArray<RazorVueRenderNode> nodes)
        {
            nodes = ImmutableArray<RazorVueRenderNode>.Empty;
            var sourceSpan = GetRequiredSourceSpan(node, "CSharpExpressionIntermediateNode");
            var operation = _resolver.ResolveRequiredOperation(sourceSpan, "Razor IR body expression");
            if (TryResolveStaticMarkup(operation) is { } staticMarkup)
            {
                nodes = MaterializeStaticMarkupFragment(staticMarkup, CreateOrigins(sourceSpan));
                return true;
            }

            if (RazorVueStaticMarkupValueHelper.IsMarkupStringType(Unwrap(operation)?.Type))
            {
                throw CreateUnsupportedAttributeException(
                    sourceSpan,
                    $"RazorVue Razor IR frontend only supports compile-time provable static MarkupString template expressions in component '{_snapshot.Descriptor.FullName}'.");
            }

            return false;
        }

        private void AppendHtmlContent(
            ImmutableArray<RazorVueRenderNode>.Builder builder,
            RazorVueRazorIrNode node)
        {
            var text = GetNodeText(node);
            if (string.IsNullOrEmpty(text))
                return;

            builder.Add(new RazorVueTextNode(text, CreateOrigins(GetBestSourceSpan(node))));
        }

        private void AppendMarkupBlock(
            ImmutableArray<RazorVueRenderNode>.Builder builder,
            RazorVueRazorIrNode node,
            ImmutableHashSet<ILocalSymbol> allowedLocalSymbols,
            ImmutableHashSet<IParameterSymbol> allowedParameterSymbols)
        {
            if (node.Children.Length > 0)
            {
                builder.AddRange(ConvertTemplateMethodBody(node.Children, allowImperativePromotion: false, allowedLocalSymbols, allowedParameterSymbols).Children);
                return;
            }

            var markup = GetNodeText(node);
            if (string.IsNullOrEmpty(markup))
                markup = GetNodeContent(node);
            if (string.IsNullOrEmpty(markup))
                return;

            builder.AddRange(ParseStaticMarkupFragment(markup, CreateOrigins(GetBestSourceSpan(node))));
        }

        private RazorVueAttributeNode ConvertHtmlAttribute(RazorVueRazorIrNode node)
        {
            if (node.HasAttributeNameExpression)
                throw CreateUnsupportedNodeException(node, "dynamic HtmlAttributeIntermediateNode.AttributeNameExpression");

            var attributeName = node.AttributeName ?? string.Empty;
            var value = ResolveAttributeValue(
                attributeName,
                node.Children,
                node.Source,
                node,
                builderMethodName: "AddAttribute",
                builderAttributeOrdinal: GetNextElementAttributeOrdinal(attributeName));
            return new RazorVueAttributeNode(
                attributeName,
                value,
                ImmutableArray<RazorVueCapturedValueBinding>.Empty,
                CreateOrigins(node.Source));
        }

        private RazorVueAttributeEntry ConvertHtmlAttributeEntry(RazorVueRazorIrNode node)
        {
            var attributeName = node.AttributeName ?? string.Empty;
            if (string.Equals(attributeName, "@attributes", StringComparison.Ordinal))
            {
                var value = ResolveAttributeValue(attributeName, node.Children, node.Source)
                    ?? throw CreateUnsupportedAttributeException(
                        node.Source,
                        $"RazorVue Razor IR frontend requires an expression value for '@attributes' in component '{_snapshot.Descriptor.FullName}'.");
                return new RazorVueAttributeSpreadNode(
                    value,
                    ImmutableArray<RazorVueCapturedValueBinding>.Empty,
                    CreateOrigins(node.Source));
            }

            return ConvertHtmlAttribute(node);
        }

        private RazorVueAttributeNode ConvertComponentAttribute(RazorVueRazorIrNode node)
        {
            var attributeName = node.AttributeName ?? string.Empty;
            var value = ResolveAttributeValue(
                attributeName,
                node.Children,
                node.Source,
                node,
                builderMethodName: "AddComponentParameter",
                builderAttributeOrdinal: GetNextComponentAttributeOrdinal(attributeName));
            return new RazorVueAttributeNode(
                attributeName,
                value,
                ImmutableArray<RazorVueCapturedValueBinding>.Empty,
                CreateOrigins(node.Source));
        }

        private RazorVueAttributeSpreadNode ConvertSplatAttribute(RazorVueRazorIrNode node)
        {
            var sourceSpan = GetRequiredSourceSpan(node, "SplatIntermediateNode");
            var operation = _resolver.ResolveRequiredOperation(sourceSpan, "Razor IR splat attribute");
            return new RazorVueAttributeSpreadNode(
                operation,
                ImmutableArray<RazorVueCapturedValueBinding>.Empty,
                CreateOrigins(node.Source));
        }

        private IOperation? ResolveAttributeValue(
            string attributeName,
            ImmutableArray<RazorVueRazorIrNode> children,
            RazorVueRazorSourceSpan? fallbackSource,
            RazorVueRazorIrNode? ownerNode = null,
            string? builderMethodName = null,
            int builderAttributeOrdinal = -1)
        {
            if (children.Length == 0)
                return fallbackSource is null
                    ? null
                    : _resolver.ResolveRequiredOperation(fallbackSource, $"Razor IR attribute '{attributeName}'");

            if (children.Length != 1)
            {
                if (TryResolveStaticLiteralAttributeValue(children, out var literalValue))
                    return CreateLiteralStringOperation(literalValue);

                if (TryResolveMixedAttributeValue(
                        attributeName,
                        children,
                        fallbackSource,
                        ownerNode,
                        builderMethodName,
                        builderAttributeOrdinal,
                        out var mixedOperation))
                {
                    return mixedOperation;
                }

                throw CreateUnsupportedAttributeException(
                    fallbackSource,
                    $"RazorVue Razor IR frontend does not yet support mixed attribute content for '{attributeName}' in component '{_snapshot.Descriptor.FullName}'.");
            }

            return children[0].Kind switch
            {
                RazorVueRazorIrNodeKind.HtmlContent => ResolveLiteralAttributeValue(children[0]),
                RazorVueRazorIrNodeKind.CSharpExpression => ResolveExpressionAttributeOperation(
                    attributeName,
                    children[0],
                    fallbackSource,
                    ownerNode),
                RazorVueRazorIrNodeKind.CSharpExpressionAttributeValue => ResolveExpressionAttributeOperation(
                    attributeName,
                    children[0],
                    fallbackSource,
                    ownerNode),
                RazorVueRazorIrNodeKind.IntermediateToken when IsCSharpIntermediateToken(children[0]) => ResolveExpressionAttributeOperation(
                    attributeName,
                    children[0],
                    fallbackSource,
                    ownerNode),
                RazorVueRazorIrNodeKind.IntermediateToken => ResolveLiteralAttributeValue(children[0]),
                RazorVueRazorIrNodeKind.CSharpCodeAttributeValue => ResolveExpressionAttributeOperation(
                    attributeName,
                    children[0],
                    fallbackSource,
                    ownerNode),
                RazorVueRazorIrNodeKind.HtmlAttributeValue => CreateLiteralStringOperation(
                    NormalizeLiteralAttributeText(ResolveHtmlAttributeValueText(children[0], includePrefix: false))),
                _ => throw CreateUnsupportedNodeException(children[0], $"{children[0].RuntimeTypeName} '{attributeName}'")
            };
        }

        private bool TryResolveMixedAttributeValue(
            string attributeName,
            ImmutableArray<RazorVueRazorIrNode> children,
            RazorVueRazorSourceSpan? fallbackSource,
            RazorVueRazorIrNode? ownerNode,
            string? builderMethodName,
            int builderAttributeOrdinal,
            out IOperation operation)
        {
            operation = default!;
            if (!TryBuildMixedAttributeExpressionText(children, out var expressionText))
                return false;

            foreach (var sourceSpan in EnumerateMixedAttributeSourceSpans(children, ownerNode, fallbackSource))
            {
                if (_resolver.TryResolveRewrittenSourceExpression(expressionText, sourceSpan, out var rewrittenOperation))
                {
                    operation = rewrittenOperation;
                    return true;
                }
            }

            if (!string.IsNullOrWhiteSpace(builderMethodName) &&
                builderAttributeOrdinal >= 0 &&
                _resolver.TryResolveRewrittenBuilderAttributeValue(
                    builderMethodName!,
                    attributeName,
                    builderAttributeOrdinal,
                    expressionText,
                    out var builderRewrittenOperation))
            {
                operation = builderRewrittenOperation;
                return true;
            }

            if (_resolver.TryResolveGeneratedExpression(expressionText, fallbackSource, out var generatedOperation))
            {
                operation = generatedOperation;
                return true;
            }

            return false;
        }

        private IOperation ResolveExpressionAttributeOperation(
            string attributeName,
            RazorVueRazorIrNode expressionNode,
            RazorVueRazorSourceSpan? fallbackSource,
            RazorVueRazorIrNode? ownerNode)
        {
            var generatedExpressionText = GetNodeText(expressionNode);
            var sourceSpan = GetBestSourceSpan(expressionNode) ??
                             GetBestSourceSpan(ownerNode ?? expressionNode) ??
                             fallbackSource;
            if (_resolver.TryResolveGeneratedExpression(generatedExpressionText, sourceSpan, out var generatedExpressionOperation))
                return generatedExpressionOperation;

            if (sourceSpan is null)
            {
                throw new InvalidOperationException(
                    $"The Razor IR node 'expression attribute {attributeName}' did not expose a source span.");
            }

            return _resolver.ResolveRequiredOperation(
                sourceSpan,
                $"Razor IR expression attribute '{attributeName}'");
        }

        private IOperation ResolveLiteralAttributeValue(RazorVueRazorIrNode node)
        {
            var text = GetNodeText(node);
            if (string.IsNullOrEmpty(text))
                text = GetNodeContent(node);

            return CreateLiteralStringOperation(NormalizeLiteralAttributeText(text));
        }

        private bool TryResolveStaticLiteralAttributeValue(
            ImmutableArray<RazorVueRazorIrNode> children,
            out string value)
        {
            value = string.Empty;
            if (children.Length == 0)
                return false;

            var builder = new System.Text.StringBuilder();
            var isFirst = true;
            foreach (var child in children)
            {
                var part = ResolveStaticLiteralAttributeText(child, includePrefix: !isFirst);
                if (part is null)
                    return false;

                builder.Append(part);
                isFirst = false;
            }

            value = NormalizeLiteralAttributeText(builder.ToString());
            return true;
        }

        private static string? ResolveStaticLiteralAttributeText(
            RazorVueRazorIrNode node,
            bool includePrefix)
        {
            switch (node.Kind)
            {
                case RazorVueRazorIrNodeKind.HtmlContent:
                    return ResolveLiteralText(node);
                case RazorVueRazorIrNodeKind.HtmlAttributeValue:
                    return ResolveHtmlAttributeValueText(node, includePrefix);
                case RazorVueRazorIrNodeKind.IntermediateToken when !IsCSharpIntermediateToken(node):
                    return ResolveLiteralText(node);
                default:
                    return null;
            }
        }

        private static string ResolveHtmlAttributeValueText(
            RazorVueRazorIrNode node,
            bool includePrefix)
        {
            var text = ResolveLiteralText(node);
            return includePrefix && !string.IsNullOrEmpty(node.Prefix)
                ? node.Prefix + text
                : text;
        }

        private static string ResolveLiteralText(RazorVueRazorIrNode node)
        {
            var text = GetNodeText(node);
            return string.IsNullOrEmpty(text)
                ? GetNodeContent(node)
                : text;
        }

        private bool TryBuildMixedAttributeExpressionText(
            ImmutableArray<RazorVueRazorIrNode> children,
            out string expressionText)
        {
            expressionText = string.Empty;
            if (children.IsDefaultOrEmpty)
                return false;

            var parts = new List<string>(children.Length);
            var literalBuilder = new System.Text.StringBuilder();
            var isFirst = true;

            foreach (var child in children)
            {
                if (!TryBuildMixedAttributeExpressionPart(
                        child,
                        includePrefix: !isFirst,
                        literalBuilder,
                        parts,
                        out var part))
                {
                    return false;
                }

                if (part is not null)
                    parts.Add(part);

                isFirst = false;
            }

            FlushPendingLiteral(literalBuilder, parts);

            if (parts.Count == 0)
            {
                expressionText = SymbolDisplay.FormatLiteral(string.Empty, quote: true);
                return true;
            }

            expressionText = string.Join(" + ", parts);
            return true;
        }

        private bool TryBuildMixedAttributeExpressionPart(
            RazorVueRazorIrNode node,
            bool includePrefix,
            System.Text.StringBuilder literalBuilder,
            List<string> parts,
            out string part)
        {
            part = null!;

            switch (node.Kind)
            {
                case RazorVueRazorIrNodeKind.HtmlContent:
                case RazorVueRazorIrNodeKind.HtmlAttributeValue:
                case RazorVueRazorIrNodeKind.IntermediateToken when !IsCSharpIntermediateToken(node):
                {
                    var literalText = ResolveStaticLiteralAttributeText(node, includePrefix);
                    if (literalText is null)
                        return false;

                    literalBuilder.Append(NormalizeLiteralAttributeText(literalText));
                    return true;
                }

                case RazorVueRazorIrNodeKind.CSharpExpression:
                case RazorVueRazorIrNodeKind.CSharpExpressionAttributeValue:
                case RazorVueRazorIrNodeKind.CSharpCodeAttributeValue:
                case RazorVueRazorIrNodeKind.IntermediateToken when IsCSharpIntermediateToken(node):
                {
                    var text = GetNodeText(node);
                    if (string.IsNullOrWhiteSpace(text))
                        text = GetNodeContent(node);

                    var expressionText = ExtractAttributeExpressionText(text);
                    if (string.IsNullOrWhiteSpace(expressionText))
                        return false;

                    if (includePrefix && !string.IsNullOrEmpty(node.Prefix))
                        literalBuilder.Append(node.Prefix);

                    FlushPendingLiteral(literalBuilder, parts);
                    part = "(" + expressionText + ")";
                    return true;
                }

                default:
                    return false;
            }
        }

        private static string ExtractAttributeExpressionText(string text)
        {
            var trimmed = text.Trim();
            if (trimmed.Length == 0)
                return string.Empty;

            if (trimmed.StartsWith("@(", StringComparison.Ordinal) &&
                trimmed[trimmed.Length - 1] == ')')
            {
                return trimmed.Substring(2, trimmed.Length - 3).Trim();
            }

            if (trimmed[0] == '@')
            {
                trimmed = trimmed.Substring(1).TrimStart();
            }

            return trimmed;
        }

        private static void FlushPendingLiteral(System.Text.StringBuilder literalBuilder, List<string> parts)
        {
            if (literalBuilder.Length == 0)
                return;

            parts.Add(SymbolDisplay.FormatLiteral(literalBuilder.ToString(), quote: true));
            literalBuilder.Clear();
        }

        private IEnumerable<RazorVueRazorSourceSpan?> EnumerateMixedAttributeSourceSpans(
            ImmutableArray<RazorVueRazorIrNode> children,
            RazorVueRazorIrNode? ownerNode,
            RazorVueRazorSourceSpan? fallbackSource)
        {
            var seen = new HashSet<(string FilePath, int AbsoluteIndex, int Length)>();

            foreach (var child in children)
            {
                if (!IsMixedAttributeExpressionNode(child))
                    continue;

                var sourceSpan = TryGetBestSourceSpan(child);
                if (TryAddSourceSpan(sourceSpan, seen))
                    yield return sourceSpan;
            }

            var ownerSource = GetBestSourceSpan(ownerNode ?? children[0]);
            if (TryAddSourceSpan(ownerSource, seen))
                yield return ownerSource;

            if (TryAddSourceSpan(fallbackSource, seen))
                yield return fallbackSource;
        }

        private static bool IsMixedAttributeExpressionNode(RazorVueRazorIrNode node)
            => node.Kind == RazorVueRazorIrNodeKind.CSharpExpression ||
               node.Kind == RazorVueRazorIrNodeKind.CSharpExpressionAttributeValue ||
               node.Kind == RazorVueRazorIrNodeKind.CSharpCodeAttributeValue ||
               (node.Kind == RazorVueRazorIrNodeKind.IntermediateToken && IsCSharpIntermediateToken(node));

        private static bool TryAddSourceSpan(
            RazorVueRazorSourceSpan? sourceSpan,
            HashSet<(string FilePath, int AbsoluteIndex, int Length)> seen)
        {
            if (sourceSpan is null)
                return false;

            var key = (
                NormalizeComparablePath(sourceSpan.Value.FilePath),
                sourceSpan.Value.AbsoluteIndex,
                sourceSpan.Value.Length);
            return seen.Add(key);
        }

        private int GetNextElementAttributeOrdinal(string attributeName)
            => GetNextAttributeOrdinal(_elementAttributeOrdinals, attributeName);

        private int GetNextComponentAttributeOrdinal(string attributeName)
            => GetNextAttributeOrdinal(_componentAttributeOrdinals, attributeName);

        private static int GetNextAttributeOrdinal(Dictionary<string, int> ordinals, string attributeName)
        {
            if (!ordinals.TryGetValue(attributeName, out var ordinal))
                ordinal = 0;

            ordinals[attributeName] = ordinal + 1;
            return ordinal;
        }

        private void RejectElementExtensions(IEnumerable<RazorVueRazorIrNode> nodes, string detail)
        {
            var first = nodes.FirstOrDefault();
            if (first is not null)
                throw CreateUnsupportedNodeException(first, detail);
        }

        private void RejectComponentExtensions(IEnumerable<RazorVueRazorIrNode> nodes, string detail)
        {
            var first = nodes.FirstOrDefault();
            if (first is not null)
                throw CreateUnsupportedNodeException(first, detail);
        }

        private RazorVueNodeKey? ResolveSetKeyValue(ImmutableArray<RazorVueRazorIrNode> setKeys)
        {
            if (setKeys.IsDefaultOrEmpty)
                return null;

            if (setKeys.Length != 1)
            {
                throw CreateUnsupportedAttributeException(
                    GetBestSourceSpan(setKeys[0]),
                    $"RazorVue Razor IR frontend expected exactly one SetKeyIntermediateNode for component '{_snapshot.Descriptor.FullName}'.");
            }

            var setKeyNode = setKeys[0];
            var sourceSpan = GetBestSourceSpan(setKeyNode) ??
                             TryGetBestSourceSpan(setKeyNode.Children.FirstOrDefault()) ??
                             throw new InvalidOperationException("The Razor IR node 'SetKeyIntermediateNode' did not expose a source span.");

            if (setKeyNode.Children.Length == 1)
            {
                var child = setKeyNode.Children[0];
                var expression = child.Kind switch
                {
                    RazorVueRazorIrNodeKind.CSharpExpression => ConvertExpression(child).Expression,
                    RazorVueRazorIrNodeKind.CSharpExpressionAttributeValue => ResolveExpressionAttributeOperation(
                        "key",
                        child,
                        sourceSpan,
                        setKeyNode),
                    RazorVueRazorIrNodeKind.IntermediateToken when IsCSharpIntermediateToken(child) => ResolveExpressionAttributeOperation(
                        "key",
                        child,
                        sourceSpan,
                        setKeyNode),
                    RazorVueRazorIrNodeKind.HtmlContent => CreateLiteralStringOperation(ResolveLiteralText(child)),
                    RazorVueRazorIrNodeKind.IntermediateToken => CreateLiteralStringOperation(ResolveLiteralText(child)),
                    _ => _resolver.ResolveRequiredOperation(sourceSpan, "Razor IR @key expression")
                };

                return new RazorVueNodeKey(
                    expression,
                    ImmutableArray<RazorVueCapturedValueBinding>.Empty,
                    CreateOrigins(GetBestSourceSpan(child) ?? sourceSpan));
            }

            return new RazorVueNodeKey(
                    _resolver.ResolveRequiredOperation(sourceSpan, "Razor IR @key expression"),
                    ImmutableArray<RazorVueCapturedValueBinding>.Empty,
                    CreateOrigins(sourceSpan));
        }

        private RazorVueNodeKey? ResolveElementKey(RazorVueRazorIrNode node)
            => ResolveNodeKey(node.SetKeysOrEmpty, node.AttributesOrEmpty, isComponent: false);

        private RazorVueNodeKey? ResolveComponentKey(RazorVueRazorIrNode node)
            => ResolveNodeKey(node.SetKeysOrEmpty, node.AttributesOrEmpty, isComponent: true);

        private RazorVueNodeKey? ResolveNodeKey(
            ImmutableArray<RazorVueRazorIrNode> setKeys,
            ImmutableArray<RazorVueRazorIrNode> attributes,
            bool isComponent)
        {
            var keyFromSetKey = ResolveSetKeyValue(setKeys);
            if (keyFromSetKey is not null)
                return keyFromSetKey;

            foreach (var attribute in attributes)
            {
                if (!IsKeyAttribute(attribute))
                    continue;

                return ResolveKeyAttributeValue(attribute, isComponent);
            }

            return null;
        }

        private RazorVueNodeKey ResolveKeyAttributeValue(RazorVueRazorIrNode attribute, bool isComponent)
        {
            var sourceSpan = attribute.Source ?? GetBestSourceSpan(attribute);
            var value = ResolveKeyOperation(attribute, isComponent)
                        ?? throw CreateUnsupportedAttributeException(
                            sourceSpan,
                            $"RazorVue Razor IR frontend requires an expression or literal value for '@key' in component '{_snapshot.Descriptor.FullName}'.");

            return new RazorVueNodeKey(
                value,
                ImmutableArray<RazorVueCapturedValueBinding>.Empty,
                CreateOrigins(sourceSpan));
        }

        private IOperation? ResolveKeyOperation(RazorVueRazorIrNode attribute, bool isComponent)
        {
            var sourceExpressionOperation = TryResolveSourceKeyOperation(attribute, isComponent);
            if (sourceExpressionOperation is not null)
                return sourceExpressionOperation;

            var mappedOperation = TryResolveMappedKeyOperation(attribute);
            if (mappedOperation is not null)
                return mappedOperation;

            return ResolveAttributeValue("@key", attribute.Children, attribute.Source, attribute);
        }

        private IOperation? TryResolveMappedKeyOperation(RazorVueRazorIrNode attribute)
        {
            var sourceSpan = attribute.Source
                             ?? GetBestSourceSpan(attribute)
                             ?? TryGetBestSourceSpan(attribute.Children.FirstOrDefault());
            if (sourceSpan is null)
                return null;

            if (!_resolver.TryResolveOperation(sourceSpan, out var operation))
                return null;

            operation = Jazor.RazorVue.RazorVueOperationNormalizer.Unwrap(operation);
            return operation switch
            {
                IConversionOperation conversion => Jazor.RazorVue.RazorVueOperationNormalizer.Unwrap(conversion.Operand),
                IArgumentOperation argument => Jazor.RazorVue.RazorVueOperationNormalizer.Unwrap(argument.Value),
                _ => operation
            };
        }

        private IOperation? TryResolveSourceKeyOperation(RazorVueRazorIrNode attribute, bool isComponent)
        {
            var sourceSpan = attribute.Source
                             ?? GetBestSourceSpan(attribute);
            if (sourceSpan is null)
                return null;

            if (!TryReadSourceSpanText(sourceSpan.Value, out var sourceText))
                return null;

            if (!TryExtractKeyExpressionText(sourceText, out var expressionText, out var isStringLiteral))
                return null;

            if (isStringLiteral)
                return CreateLiteralStringOperation(UnquoteStringLiteral(expressionText));

            if (_resolver.TryResolveRewrittenSourceExpression(expressionText, sourceSpan, out var rewrittenOperation))
                return rewrittenOperation;

            var ordinal = isComponent ? _componentKeyOrdinal++ : _elementKeyOrdinal++;
            if (_resolver.TryResolveRewrittenBuilderAttributeValue(
                    isComponent ? "AddComponentParameter" : "AddAttribute",
                    "@key",
                    ordinal,
                    expressionText,
                    out var builderRewrittenOperation))
            {
                return builderRewrittenOperation;
            }

            if (_resolver.TryResolveGeneratedExpression(expressionText, sourceSpan, out var generatedOperation))
                return generatedOperation;

            return null;
        }

        private bool TryReadSourceSpanText(RazorVueRazorSourceSpan sourceSpan, out string text)
        {
            text = string.Empty;
            var primaryDocument = _snapshot.RazorSourceGeneratorDocument?.PrimaryDocument;
            if (primaryDocument is null)
                return false;

            if (!PathsEqual(primaryDocument.Path, sourceSpan.FilePath) &&
                !string.Equals(primaryDocument.NormalizedPath, Jazor.RazorVue.RazorVueRazorDocument.NormalizePath(sourceSpan.FilePath ?? string.Empty), StringComparison.Ordinal))
            {
                return false;
            }

            if (sourceSpan.AbsoluteIndex < 0 ||
                sourceSpan.Length <= 0 ||
                sourceSpan.AbsoluteIndex + sourceSpan.Length > primaryDocument.Text.Length)
            {
                return false;
            }

            text = primaryDocument.Text.ToString(TextSpan.FromBounds(sourceSpan.AbsoluteIndex, sourceSpan.AbsoluteIndex + sourceSpan.Length));
            return !string.IsNullOrWhiteSpace(text);
        }

        private static bool TryExtractKeyExpressionText(string sourceText, out string expressionText, out bool isStringLiteral)
        {
            expressionText = string.Empty;
            isStringLiteral = false;

            var equalsIndex = sourceText.IndexOf('=');
            var valueText = equalsIndex < 0
                ? sourceText.Trim()
                : sourceText.Substring(equalsIndex + 1).Trim();
            if (valueText.Length == 0)
                return false;

            if ((valueText[0] == '"' && valueText[valueText.Length - 1] == '"') ||
                (valueText[0] == '\'' && valueText[valueText.Length - 1] == '\''))
            {
                valueText = valueText.Substring(1, valueText.Length - 2);
            }

            valueText = valueText.Trim();
            if (valueText.Length == 0)
                return false;

            expressionText = valueText;
            isStringLiteral = IsQuotedStringLiteral(valueText);
            return true;
        }

        private static bool IsQuotedStringLiteral(string text)
            => text.Length >= 2 &&
               ((text[0] == '"' && text[text.Length - 1] == '"') ||
                (text[0] == '\'' && text[text.Length - 1] == '\''));

        private static string UnquoteStringLiteral(string text)
            => IsQuotedStringLiteral(text)
                ? text.Substring(1, text.Length - 2)
                : text;

        private static RazorVueRazorSourceSpan? TryGetBestSourceSpan(RazorVueRazorIrNode? node)
            => node is null ? null : GetBestSourceSpan(node);

        private static bool IsKeyAttribute(RazorVueRazorIrNode attribute)
            => string.Equals(attribute.AttributeName, "@key", StringComparison.Ordinal);

        private RazorVueCompilationIssueException CreateUnsupportedNodeException(RazorVueRazorIrNode node, string detail)
            => CreateUnsupportedAttributeException(
                GetBestSourceSpan(node),
                $"RazorVue Razor IR frontend does not yet support {detail} in component '{_snapshot.Descriptor.FullName}'.");

        private RazorVueCompilationIssueException CreateUnsupportedAttributeException(RazorVueRazorSourceSpan? sourceSpan, string message)
        {
            var issue = new RazorVueCompilationIssue(
                RazorVueIssueCode.CanonicalizationFailed,
                RazorVueIssueSeverity.Error,
                message,
                ImmutableArray<string>.Empty);
            return new RazorVueCompilationIssueException(
                issue,
                _snapshot.Descriptor.FullName,
                CreateSourceOrigin(sourceSpan, RazorVueOriginKind.Template));
        }

        private bool TryResolveSlotOutlet(IOperation operation, out string slotName)
        {
            slotName = string.Empty;
            var current = Jazor.RazorVue.RazorVueOperationNormalizer.Unwrap(operation);
            if (current is not IPropertyReferenceOperation propertyReference)
                return false;

            if (!IsCurrentComponentMember(propertyReference.Property, propertyReference.Instance))
                return false;

            if (!IsRenderFragment(propertyReference.Property.Type))
                return false;

            if (_context.Symbols.ParameterAttribute is null ||
                !propertyReference.Property.GetAttributes().Any(attribute =>
                    SymbolEqualityComparer.Default.Equals(attribute.AttributeClass, _context.Symbols.ParameterAttribute)))
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
               ((_context.Symbols.RenderFragment is not null &&
                 SymbolEqualityComparer.Default.Equals(namedType.OriginalDefinition, _context.Symbols.RenderFragment)) ||
                (_context.Symbols.RenderFragmentOfT is not null &&
                 SymbolEqualityComparer.Default.Equals(namedType.OriginalDefinition, _context.Symbols.RenderFragmentOfT)));

        private bool IsCurrentComponentMember(ISymbol symbol, IOperation? instance)
            => RazorVueSymbolIdentity.IsCurrentComponentMember(
                _snapshot.ComponentSymbol,
                symbol,
                instance,
                Jazor.RazorVue.RazorVueOperationNormalizer.Unwrap);

        private RazorVueRenderFragment ConvertExpressionOrSlotOutlet(RazorVueRazorIrNode node)
        {
            var expression = ConvertExpression(node);
            if (TryResolveSlotOutlet(expression.Expression, out var slotName))
            {
                return new RazorVueRenderFragment(
                [
                    new RazorVueSlotOutletNode(slotName, null, expression.Origins)
                ]);
            }

            if (TryConvertSlotOutletInvocationExpression(expression.Expression, expression.Origins, out var slotOutletInvocationFragment))
                return slotOutletInvocationFragment;

            if (TryConvertRenderFragmentExpression(node, expression.Expression, expression.Origins, out var renderFragment))
                return renderFragment;

            return new RazorVueRenderFragment([expression]);
        }

        private bool TryConvertRenderFragmentExpression(
            RazorVueRazorIrNode node,
            IOperation operation,
            ImmutableArray<RazorVueSourceOrigin> origins,
            out RazorVueRenderFragment fragment)
        {
            fragment = RazorVueRenderFragment.Empty;
            if (Unwrap(operation) is IInvocationOperation invocation &&
                TryConvertTypedRenderFragmentInvocation(invocation, origins, out fragment))
            {
                return true;
            }

            if (!TryResolveStoredSlotTemplate(operation, out var slotTemplate) ||
                !string.IsNullOrWhiteSpace(slotTemplate.ParameterName))
            {
                return false;
            }

            if (TryGetBestSourceSpan(node) is null)
                return false;

            fragment = MaterializeCapturedTemplateChildren(slotTemplate, origins);
            return true;
        }

        private bool TryConvertSlotOutletInvocationExpression(
            IOperation operation,
            ImmutableArray<RazorVueSourceOrigin> origins,
            out RazorVueRenderFragment fragment)
        {
            fragment = RazorVueRenderFragment.Empty;
            if (Unwrap(operation) is not IInvocationOperation invocation ||
                invocation.Arguments.Length != 1)
            {
                return false;
            }

            if (!TryResolveTypedSlotOutletInvocation(invocation, out var slotName, out var argument))
                return false;

            fragment = new RazorVueRenderFragment(
            [
                new RazorVueSlotOutletNode(
                    slotName,
                    argument,
                    origins)
            ]);
            return true;
        }

        private bool TryResolveTypedSlotOutletInvocation(
            IInvocationOperation invocation,
            out string slotName,
            out IOperation argument)
        {
            slotName = string.Empty;
            argument = default!;

            if (Unwrap(invocation.Instance) is not IPropertyReferenceOperation propertyReference)
                return false;

            if (!IsCurrentComponentMember(propertyReference.Property, propertyReference.Instance))
                return false;

            if (!IsTypedRenderFragment(propertyReference.Property.Type))
                return false;

            if (_context.Symbols.ParameterAttribute is null ||
                !propertyReference.Property.GetAttributes().Any(attribute =>
                    SymbolEqualityComparer.Default.Equals(attribute.AttributeClass, _context.Symbols.ParameterAttribute)))
            {
                return false;
            }

            var invocationArgument = Unwrap(invocation.Arguments[0].Value);
            if (invocationArgument is null || IsConstantNull(invocationArgument))
                return false;

            slotName = string.Equals(propertyReference.Property.Name, "ChildContent", StringComparison.Ordinal)
                ? "default"
                : ToLowerCamelCase(propertyReference.Property.Name);
            argument = invocationArgument;
            return true;
        }

        private bool TryConvertTypedRenderFragmentInvocation(
            IInvocationOperation invocation,
            ImmutableArray<RazorVueSourceOrigin> origins,
            out RazorVueRenderFragment fragment)
        {
            fragment = RazorVueRenderFragment.Empty;
            if (!TryResolveStoredSlotTemplate(invocation.Instance, out var slotTemplate) ||
                string.IsNullOrWhiteSpace(slotTemplate.ParameterName) ||
                slotTemplate.ParameterSymbol is null ||
                invocation.Arguments.Length != 1)
            {
                return false;
            }

            var initializer = Unwrap(invocation.Arguments[0].Value);
            if (initializer is null || IsConstantNull(initializer))
                return false;

            fragment = CreateTypedFragmentScope(slotTemplate, initializer, origins);
            return true;
        }

        private RazorVueRenderFragment CreateTypedFragmentScope(
            ParsedSlotTemplate slotTemplate,
            IOperation initializer,
            ImmutableArray<RazorVueSourceOrigin> origins)
        {
            var fragment = new RazorVueRenderFragment(
            [
                new RazorVueTemplateScopeNode(
                    ScopeName: slotTemplate.ParameterName!,
                    ScopeParameterSymbol: slotTemplate.ParameterSymbol,
                    Initializer: initializer,
                    Children: slotTemplate.Children,
                    Origins: origins)
            ]);

            return WrapCapturedTemplateScopes(fragment, slotTemplate.CapturedBindings, origins);
        }

        private bool IsTypedRenderFragment(ITypeSymbol typeSymbol)
            => typeSymbol is INamedTypeSymbol namedType &&
               _context.Symbols.RenderFragmentOfT is not null &&
               SymbolEqualityComparer.Default.Equals(namedType.OriginalDefinition, _context.Symbols.RenderFragmentOfT);

        private static bool IsConstantNull(IOperation? operation)
        {
            var current = Unwrap(operation);
            return current?.ConstantValue.HasValue == true &&
                   current.ConstantValue.Value is null;
        }

        private static IOperation? Unwrap(IOperation? operation)
            => RazorVueOperationNormalizer.Unwrap(operation);

        private bool TryConvertConditional(
            IReadOnlyList<RazorVueRazorIrNode> nodes,
            ref int index,
            ImmutableHashSet<ILocalSymbol> allowedLocalSymbols,
            ImmutableHashSet<IParameterSymbol> allowedParameterSymbols,
            out RazorVueConditionalNode conditionalNode)
        {
            conditionalNode = default!;
            if (!IsTemplateCodeNode(nodes[index]))
                return false;

            var codeNode = nodes[index];
            var codeText = GetNodeText(codeNode);
            var normalizedCodeText = NormalizeTemplateCodeText(codeText);
            var isBoundaryIfHeader = StartsWithBoundaryControlKeyword(normalizedCodeText, "if");
            if (!StartsWithControlKeyword(codeText, "if") &&
                !StartsWithBoundaryControlKeyword(normalizedCodeText, "if") &&
                !IsElseIfBoundaryCodeNode(normalizedCodeText))
                return false;

            var sourceSpan = GetRequiredControlSourceSpan(codeNode, "if");
            if (!_resolver.TryResolveConditional(sourceSpan, out var resolvedConditional))
                return false;

            if (RazorVueImperativeRenderPromotionAnalyzer.ShouldPromoteBody([resolvedConditional.Operation]))
                return false;

            var isElseIfHeader = IsElseIfBoundaryCodeNode(normalizedCodeText);
            var bodyEnd = isElseIfHeader
                ? nodes.Count
                : FindControlStatementEndIndex(
                    nodes,
                    index,
                    resolvedConditional.WhenFalseRange is null
                        ? resolvedConditional.WhenTrueRange
                        : resolvedConditional.StatementRange,
                    sourceSpan,
                    "if");
            var coveredNodes = isElseIfHeader
                ? nodes.Skip(index + 1).ToList()
                : nodes.Skip(index + 1).Take(bodyEnd - index - 1).ToList();
            List<RazorVueRazorIrNode> whenTrueNodes;
            List<RazorVueRazorIrNode> whenFalseNodes;
            if (isElseIfHeader)
            {
                whenTrueNodes = TakeLeadingBranchNodesUntilTopLevelElseBoundary(coveredNodes, sourceSpan, "if-true");
                whenFalseNodes = TryTakeTrailingNodesAfterTopLevelElseBoundary(coveredNodes, sourceSpan, "if-false");
            }
            else if (isBoundaryIfHeader)
            {
                bodyEnd = FindControlStatementEndIndex(
                    nodes,
                    index,
                    resolvedConditional.StatementRange,
                    sourceSpan,
                    "if");
                coveredNodes = nodes.Skip(index + 1).Take(bodyEnd - index - 1).ToList();
                if (resolvedConditional.WhenFalseRange is not null)
                {
                    (whenTrueNodes, whenFalseNodes) = SplitConditionalBranchesByStructure(coveredNodes, sourceSpan);
                }
                else
                {
                    whenTrueNodes = BindCoveredControlBodyNodes(coveredNodes, sourceSpan, "if-true");
                    whenFalseNodes = [];
                }
            }
            else if (resolvedConditional.WhenFalseRange is not null)
            {
                (whenTrueNodes, whenFalseNodes) = SplitConditionalBranchesByStructure(coveredNodes, sourceSpan);
            }
            else
            {
                whenTrueNodes = SliceNodesByRange(coveredNodes, resolvedConditional.WhenTrueRange, sourceSpan, "if-true", trimLeadingControlNode: false);
                whenFalseNodes = [];
            }

            var whenTrue = ConvertTemplateMethodBody(whenTrueNodes, allowImperativePromotion: false, allowedLocalSymbols, allowedParameterSymbols);
            var whenFalse = ConvertTemplateMethodBody(whenFalseNodes, allowImperativePromotion: false, allowedLocalSymbols, allowedParameterSymbols);

            index = bodyEnd;
            conditionalNode = new RazorVueConditionalNode(
                resolvedConditional.Operation.Condition,
                whenTrue,
                whenFalse,
                CreateOrigins(sourceSpan));
            return true;
        }

        private bool TryConvertForEach(
            IReadOnlyList<RazorVueRazorIrNode> nodes,
            ref int index,
            ImmutableHashSet<ILocalSymbol> allowedLocalSymbols,
            ImmutableHashSet<IParameterSymbol> allowedParameterSymbols,
            out RazorVueForEachNode loopNode)
        {
            loopNode = default!;
            if (!IsTemplateCodeNode(nodes[index]))
                return false;

            var codeNode = nodes[index];
            var codeText = GetNodeText(codeNode);
            var normalizedCodeText = NormalizeTemplateCodeText(codeText);
            var isBoundaryForEachHeader = StartsWithBoundaryControlKeyword(normalizedCodeText, "foreach");
            if (!StartsWithControlKeyword(codeText, "foreach") &&
                !StartsWithBoundaryControlKeyword(normalizedCodeText, "foreach"))
                return false;

            var sourceSpan = GetRequiredControlSourceSpan(codeNode, "foreach");
            if (!_resolver.TryResolveForEach(sourceSpan, out var resolvedLoop))
                return false;

            if (RazorVueImperativeRenderPromotionAnalyzer.ShouldPromoteBody([resolvedLoop.Operation]))
                return false;

            var bodyEnd = FindControlStatementEndIndex(
                nodes,
                index,
                isBoundaryForEachHeader ? resolvedLoop.StatementRange : resolvedLoop.BodyRange,
                sourceSpan,
                "foreach");
            var coveredNodes = nodes.Skip(index + 1).Take(bodyEnd - index - 1).ToList();
            var bodyNodes = isBoundaryForEachHeader
                ? BindCoveredControlBodyNodes(coveredNodes, sourceSpan, "foreach-body")
                : SliceNodesByRange(coveredNodes, resolvedLoop.BodyRange, sourceSpan, "foreach-body", trimLeadingControlNode: false);
            var body = ConvertTemplateMethodBody(
                bodyNodes,
                allowImperativePromotion: false,
                resolvedLoop.Operation.Locals.Length > 0
                    ? allowedLocalSymbols.Add(resolvedLoop.Operation.Locals[0])
                    : allowedLocalSymbols,
                allowedParameterSymbols);
            index = bodyEnd;
            loopNode = new RazorVueForEachNode(
                resolvedLoop.Operation.Locals.Length > 0 ? resolvedLoop.Operation.Locals[0].Name : "item",
                resolvedLoop.Operation.Locals.Length > 0 ? resolvedLoop.Operation.Locals[0] : null,
                Jazor.RazorVue.RazorVueOperationNormalizer.Unwrap(resolvedLoop.Operation.Collection) ?? resolvedLoop.Operation.Collection,
                body,
                CreateOrigins(sourceSpan));
            return true;
        }

        private bool TryConvertFor(
            IReadOnlyList<RazorVueRazorIrNode> nodes,
            ref int index,
            ImmutableHashSet<ILocalSymbol> allowedLocalSymbols,
            ImmutableHashSet<IParameterSymbol> allowedParameterSymbols,
            out RazorVueForNode loopNode)
        {
            loopNode = default!;
            if (!IsTemplateCodeNode(nodes[index]))
                return false;

            var codeNode = nodes[index];
            var codeText = GetNodeText(codeNode);
            var normalizedCodeText = NormalizeTemplateCodeText(codeText);
            var isBoundaryForHeader = StartsWithBoundaryControlKeyword(normalizedCodeText, "for");
            if (!StartsWithControlKeyword(codeText, "for") &&
                !StartsWithBoundaryControlKeyword(normalizedCodeText, "for"))
                return false;

            var sourceSpan = GetRequiredControlSourceSpan(codeNode, "for");
            if (!_resolver.TryResolveFor(sourceSpan, out var resolvedLoop))
                return false;

            if (RazorVueImperativeRenderPromotionAnalyzer.ShouldPromoteBody([resolvedLoop.Operation]))
                return false;

            var analysis = RazorVueForLoopAnalyzer.AnalyzeRequired(
                resolvedLoop.Operation,
                Jazor.RazorVue.RazorVueOperationNormalizer.Unwrap,
                _snapshot.Descriptor.FullName);
            var bodyEnd = FindControlStatementEndIndex(
                nodes,
                index,
                isBoundaryForHeader ? resolvedLoop.StatementRange : resolvedLoop.BodyRange,
                sourceSpan,
                "for");
            var coveredNodes = nodes.Skip(index + 1).Take(bodyEnd - index - 1).ToList();
            var bodyNodes = isBoundaryForHeader
                ? BindCoveredControlBodyNodes(coveredNodes, sourceSpan, "for-body")
                : SliceNodesByRange(coveredNodes, resolvedLoop.BodyRange, sourceSpan, "for-body", trimLeadingControlNode: false);
            var body = ConvertTemplateMethodBody(
                bodyNodes,
                allowImperativePromotion: false,
                resolvedLoop.Operation.Locals.Length > 0
                    ? allowedLocalSymbols.Add(resolvedLoop.Operation.Locals[0])
                    : allowedLocalSymbols,
                allowedParameterSymbols);
            index = bodyEnd;
            loopNode = new RazorVueForNode(
                analysis.VariableName,
                resolvedLoop.Operation.Locals.Length > 0 ? resolvedLoop.Operation.Locals[0] : null,
                analysis.InitialValue,
                analysis.ConditionKind,
                analysis.LimitValue,
                analysis.StepKind,
                analysis.StepValue,
                body,
                CreateOrigins(sourceSpan));
            return true;
        }

        private bool TryConvertTemplateLocalCodeBlock(
            IReadOnlyList<RazorVueRazorIrNode> nodes,
            ref int index,
            ImmutableHashSet<ILocalSymbol> allowedLocalSymbols,
            ImmutableHashSet<IParameterSymbol> allowedParameterSymbols,
            out ImmutableArray<RazorVueLocalDeclarationNode> localDeclarations,
            out PendingTemplateControlNode? pendingControlNode)
        {
            localDeclarations = ImmutableArray<RazorVueLocalDeclarationNode>.Empty;
            pendingControlNode = null;
            var node = nodes[index];
            if (node.Kind != RazorVueRazorIrNodeKind.CSharpCode)
                return false;

            if (IsIgnorableTemplateCodeNode(node))
                return false;

            var sourceSpan = GetRequiredSourceSpan(node, "CSharpCodeIntermediateNode template local code block");
            var resolvedOperation = _resolver.ResolveRequiredOperation(sourceSpan, "Razor IR template local code block");
            if (!TryCreateTemplateLocalDeclarations(
                    node,
                    resolvedOperation,
                    nodes,
                    index,
                    allowedLocalSymbols,
                    allowedParameterSymbols,
                    out localDeclarations,
                    out pendingControlNode,
                    out var consumedNodeCount))
            {
                return false;
            }

            index += consumedNodeCount;
            return true;
        }

        private bool TryConvertPendingTemplateControlNode(
            PendingTemplateControlNode? pendingTemplateControlNode,
            IReadOnlyList<RazorVueRazorIrNode> nodes,
            ref int index,
            ImmutableHashSet<ILocalSymbol> allowedLocalSymbols,
            ImmutableHashSet<IParameterSymbol> allowedParameterSymbols,
            out RazorVueRenderNode renderNode)
        {
            renderNode = default!;
            if (pendingTemplateControlNode is not { } pending)
                return false;

            switch (pending.Kind)
            {
                case PendingTemplateControlKind.Conditional:
                {
                    if (!_resolver.TryResolveConditional(pending.ConditionalOperation!, out var resolvedConditional))
                    {
                        throw CreateUnsupportedAttributeException(
                            GetBestSourceSpan(nodes[index]),
                            $"RazorVue Razor IR frontend could not map template code block conditional back to source ranges in component '{_snapshot.Descriptor.FullName}'.");
                    }

                    var bodyEnd = FindControlStatementEndIndex(
                        nodes,
                        index,
                        resolvedConditional.WhenFalseRange is null
                            ? resolvedConditional.WhenTrueRange
                            : resolvedConditional.StatementRange,
                        pending.SourceSpan,
                        "embedded if");
                    var coveredNodes = nodes.Skip(index).Take(bodyEnd - index).ToList();
                    List<RazorVueRazorIrNode> whenTrueNodes;
                    List<RazorVueRazorIrNode> whenFalseNodes;
                    if (resolvedConditional.WhenFalseRange is not null)
                    {
                        (whenTrueNodes, whenFalseNodes) = SplitConditionalBranchesByStructure(coveredNodes, pending.SourceSpan);
                    }
                    else
                    {
                        whenTrueNodes = BindCoveredControlBodyNodes(coveredNodes, pending.SourceSpan, "embedded if-true");
                        whenFalseNodes = [];
                    }

                    var whenTrue = ConvertTemplateMethodBody(whenTrueNodes, allowImperativePromotion: false, allowedLocalSymbols, allowedParameterSymbols);
                    var whenFalse = ConvertTemplateMethodBody(whenFalseNodes, allowImperativePromotion: false, allowedLocalSymbols, allowedParameterSymbols);

                    index = bodyEnd;
                    renderNode = new RazorVueConditionalNode(
                        resolvedConditional.Operation.Condition,
                        whenTrue,
                        whenFalse,
                        CreateOrigins(pending.SourceSpan));
                    return true;
                }
                case PendingTemplateControlKind.ForEach:
                {
                    if (!_resolver.TryResolveForEach(pending.ForEachOperation!, out var resolvedLoop))
                    {
                        throw CreateUnsupportedAttributeException(
                            GetBestSourceSpan(nodes[index]),
                            $"RazorVue Razor IR frontend could not map template code block foreach back to source ranges in component '{_snapshot.Descriptor.FullName}'.");
                    }

                    var bodyEnd = FindControlStatementEndIndex(
                        nodes,
                        index,
                        resolvedLoop.BodyRange,
                        pending.SourceSpan,
                        "embedded foreach");
                    var coveredNodes = nodes.Skip(index).Take(bodyEnd - index).ToList();
                    var bodyNodes = SliceNodesByRange(
                        coveredNodes,
                        resolvedLoop.BodyRange,
                        pending.SourceSpan,
                        "embedded foreach-body",
                        trimLeadingControlNode: false);
                    var body = ConvertTemplateMethodBody(
                        bodyNodes,
                        allowImperativePromotion: false,
                        resolvedLoop.Operation.Locals.Length > 0
                            ? allowedLocalSymbols.Add(resolvedLoop.Operation.Locals[0])
                            : allowedLocalSymbols,
                        allowedParameterSymbols);

                    index = bodyEnd;
                    renderNode = new RazorVueForEachNode(
                        resolvedLoop.Operation.Locals.Length > 0 ? resolvedLoop.Operation.Locals[0].Name : "item",
                        resolvedLoop.Operation.Locals.Length > 0 ? resolvedLoop.Operation.Locals[0] : null,
                        Unwrap(resolvedLoop.Operation.Collection) ?? resolvedLoop.Operation.Collection,
                        body,
                        CreateOrigins(pending.SourceSpan));
                    return true;
                }
                case PendingTemplateControlKind.For:
                {
                    if (!_resolver.TryResolveFor(pending.ForOperation!, out var resolvedLoop))
                    {
                        throw CreateUnsupportedAttributeException(
                            GetBestSourceSpan(nodes[index]),
                            $"RazorVue Razor IR frontend could not map template code block for-loop back to source ranges in component '{_snapshot.Descriptor.FullName}'.");
                    }

                    var analysis = RazorVueForLoopAnalyzer.AnalyzeRequired(
                        resolvedLoop.Operation,
                        Unwrap,
                        _snapshot.Descriptor.FullName);
                    var bodyEnd = FindControlStatementEndIndex(
                        nodes,
                        index,
                        resolvedLoop.BodyRange,
                        pending.SourceSpan,
                        "embedded for");
                    var coveredNodes = nodes.Skip(index).Take(bodyEnd - index).ToList();
                    var bodyNodes = SliceNodesByRange(
                        coveredNodes,
                        resolvedLoop.BodyRange,
                        pending.SourceSpan,
                        "embedded for-body",
                        trimLeadingControlNode: false);
                    var body = ConvertTemplateMethodBody(
                        bodyNodes,
                        allowImperativePromotion: false,
                        resolvedLoop.Operation.Locals.Length > 0
                            ? allowedLocalSymbols.Add(resolvedLoop.Operation.Locals[0])
                            : allowedLocalSymbols,
                        allowedParameterSymbols);

                    index = bodyEnd;
                    renderNode = new RazorVueForNode(
                        analysis.VariableName,
                        resolvedLoop.Operation.Locals.Length > 0 ? resolvedLoop.Operation.Locals[0] : null,
                        analysis.InitialValue,
                        analysis.ConditionKind,
                        analysis.LimitValue,
                        analysis.StepKind,
                        analysis.StepValue,
                        body,
                        CreateOrigins(pending.SourceSpan));
                    return true;
                }
                case PendingTemplateControlKind.Imperative:
                {
                    var operations = pending.ImperativeOperations!;
                    var bodyEnd = FindPendingImperativeEndIndex(nodes, index, operations, pending.SourceSpan);
                    var coveredNodes = bodyEnd > index
                        ? nodes.Skip(index).Take(bodyEnd - index).ToArray()
                        : [];

                    renderNode = CreateImperativeBlockNode(
                        operations,
                        pending.ImperativeKind!.Value,
                        CreateImperativeOrigins(operations, coveredNodes),
                        allowedLocalSymbols);
                    index = bodyEnd;
                    return true;
                }
                default:
                    return false;
            }
        }

        private bool TryCreateTemplateLocalDeclarations(
            RazorVueRazorIrNode codeNode,
            IOperation operation,
            IReadOnlyList<RazorVueRazorIrNode> nodes,
            int startIndex,
            ImmutableHashSet<ILocalSymbol> allowedLocalSymbols,
            ImmutableHashSet<IParameterSymbol> allowedParameterSymbols,
            out ImmutableArray<RazorVueLocalDeclarationNode> localDeclarations,
            out PendingTemplateControlNode? pendingControlNode,
            out int consumedNodeCount)
        {
            localDeclarations = ImmutableArray<RazorVueLocalDeclarationNode>.Empty;
            pendingControlNode = null;
            consumedNodeCount = 0;
            if (TryCreateStandaloneTemplateImperativeContinuation(
                    codeNode,
                    operation,
                    out pendingControlNode))
            {
                consumedNodeCount = 1;
                return true;
            }

            var encounteredRenderFragmentCarrier = false;
            var encounteredStaticMarkupCarrier = false;
            var declarators = ImmutableArray.CreateBuilder<IVariableDeclaratorOperation>();
            var immediateAssignedInitializers = new Dictionary<ILocalSymbol, IOperation>(SymbolEqualityComparer.Default);
            if (!TryCollectTemplateLocalDeclarators(codeNode, operation, declarators, immediateAssignedInitializers, out pendingControlNode))
                return false;
            var encounteredSupplementalTemplateDeclarations = _lastTemplateDeclarationScanEncounteredSupplementalDeclarations;

            var declarationBuilder = ImmutableArray.CreateBuilder<RazorVueLocalDeclarationNode>(declarators.Count);
            var currentLocalScope = allowedLocalSymbols;
            var continuationIndex = startIndex + 1;
            foreach (var declarator in declarators)
            {
                if (IsRenderTreeBuilderType(declarator.Symbol.Type))
                {
                    throw CreateUnsupportedAttributeException(
                        GetBestSourceSpan(codeNode),
                        $"RazorVue Razor IR frontend does not support RenderTreeBuilder local alias declarations in template code block for component '{_snapshot.Descriptor.FullName}'.");
                }

                if (IsRenderFragment(declarator.Symbol.Type))
                {
                    RegisterRenderFragmentLocalCarrier(
                        declarator,
                        currentLocalScope,
                        allowedParameterSymbols,
                        out var consumeFollowingTemplateNode);
                    if (consumeFollowingTemplateNode)
                    {
                        _ = ConsumeRequiredRenderFragmentCarrierTemplateNode(
                            nodes,
                            ref continuationIndex,
                            codeNode,
                            declarator);
                    }

                    ConsumeRenderFragmentCarrierContinuation(
                        nodes,
                        ref continuationIndex,
                        ref pendingControlNode);
                    encounteredRenderFragmentCarrier = true;
                    continue;
                }

                if (RazorVueStaticMarkupValueHelper.IsMarkupStringType(declarator.Symbol.Type))
                {
                    RegisterStaticMarkupLocalCarrier(declarator);
                    encounteredStaticMarkupCarrier = true;
                    continue;
                }

                if (declarator.Initializer?.Value is not { } initializer)
                {
                    if (immediateAssignedInitializers.TryGetValue(declarator.Symbol, out initializer))
                    {
                        // resolved from the same code block body
                    }
                    else if (!TryResolveImmediateAssignedInitializer(
                            declarator,
                            nodes,
                            ref continuationIndex,
                            codeNode,
                            out initializer))
                    {
                        throw CreateUnsupportedAttributeException(
                            GetBestSourceSpan(codeNode),
                            $"RazorVue template-scoped local '{declarator.Symbol.Name}' in component '{_snapshot.Descriptor.FullName}' requires an initializer.");
                    }
                }

                ValidateTemplateScopedInitializer(
                    declarator,
                    initializer,
                    currentLocalScope,
                    allowedParameterSymbols);
                declarationBuilder.Add(new RazorVueLocalDeclarationNode(
                    declarator.Symbol,
                    Unwrap(initializer) ?? initializer,
                    CreateOrigins(GetBestSourceSpan(codeNode))));
                currentLocalScope = currentLocalScope.Add(declarator.Symbol);
            }

            localDeclarations = declarationBuilder.ToImmutable();
            consumedNodeCount = Math.Max(1, continuationIndex - startIndex);
            return encounteredRenderFragmentCarrier ||
                   encounteredStaticMarkupCarrier ||
                   encounteredSupplementalTemplateDeclarations ||
                   localDeclarations.Length > 0 ||
                   pendingControlNode is not null ||
                   continuationIndex > startIndex + 1;
        }

        private bool TryCreateStandaloneTemplateImperativeContinuation(
            RazorVueRazorIrNode codeNode,
            IOperation operation,
            out PendingTemplateControlNode? pendingControlNode)
        {
            pendingControlNode = null;
            var unwrapped = Unwrap(operation);
            if (unwrapped is null)
                return false;

            return unwrapped switch
            {
                IBlockOperation blockOperation when
                    TryGetStandaloneTemplateImperativeStartOperation(blockOperation, out var blockStartOperation) &&
                    TryCreatePendingTemplateImperativeNode(codeNode, blockOperation.Operations, blockStartOperation, out pendingControlNode) => true,
                IConditionalOperation or
                IForEachLoopOperation or
                IForLoopOperation or
                IWhileLoopOperation or
                ISwitchOperation or
                ITryOperation or
                ILockOperation or
                IUsingOperation or
                IUsingDeclarationOperation or
                IReturnOperation or
                IThrowOperation or
                IAssignmentOperation or
                IIncrementOrDecrementOperation
                    when TryCreatePendingTemplateImperativeNode(codeNode, [unwrapped], unwrapped, out pendingControlNode) => true,
                IExpressionStatementOperation expressionStatement
                    when TryCreatePendingTemplateImperativeNode(codeNode, [expressionStatement], expressionStatement, out pendingControlNode) => true,
                _ => false
            };
        }

        private static bool TryGetStandaloneTemplateImperativeStartOperation(
            IBlockOperation blockOperation,
            out IOperation startOperation)
        {
            startOperation = default!;
            foreach (var operation in blockOperation.Operations)
            {
                var current = Unwrap(operation);
                if (current is null or IEmptyOperation)
                    continue;

                if (current is IVariableDeclarationGroupOperation or IVariableDeclarationOperation)
                    return false;

                startOperation = operation;
                return true;
            }

            return false;
        }

        private void RegisterStaticMarkupLocalCarrier(IVariableDeclaratorOperation declarator)
        {
            var initializer = TryGetSourceStableLocalMarkupStringInitializer(declarator.Symbol);

            if (initializer is null)
            {
                var hasLaterWrites = IsSourceStableLocalMarkupStringInitializerInvalidatedByLaterWrites(declarator.Symbol);
                var failureMessage =
                    declarator.Initializer?.Value is null
                        ? $"RazorVue MarkupString local '{declarator.Symbol.Name}' in component '{_snapshot.Descriptor.FullName}' must be assigned exactly once by the immediately following simple assignment statement and cannot be observed through later writes."
                        : $"RazorVue MarkupString local '{declarator.Symbol.Name}' in component '{_snapshot.Descriptor.FullName}' requires a compile-time provable static markup initializer.";
                if (hasLaterWrites)
                {
                    failureMessage =
                        declarator.Initializer?.Value is null
                            ? $"RazorVue MarkupString local '{declarator.Symbol.Name}' in component '{_snapshot.Descriptor.FullName}' must be assigned exactly once by the immediately following simple assignment statement and cannot be observed through later writes."
                            : $"RazorVue MarkupString local '{declarator.Symbol.Name}' in component '{_snapshot.Descriptor.FullName}' cannot be observed through later writes. Declaration-initialized local carriers must remain source-stable.";
                }

                throw CreateUnsupportedAttributeException(
                    CreateSourceSpanFromSyntax(declarator.Syntax),
                    failureMessage);
            }

            if (TryGetStaticMarkupValue(initializer) is null)
            {
                throw CreateUnsupportedAttributeException(
                    CreateSourceSpanFromSyntax(declarator.Syntax),
                    $"RazorVue MarkupString local '{declarator.Symbol.Name}' in component '{_snapshot.Descriptor.FullName}' must be initialized from compile-time provable static markup or a previously analyzable static MarkupString carrier.");
            }

            _localStaticMarkupCarriers[declarator.Symbol] = Unwrap(initializer) ?? initializer;
        }

        private bool TryResolveImmediateAssignedInitializer(
            IVariableDeclaratorOperation declarator,
            IReadOnlyList<RazorVueRazorIrNode> nodes,
            ref int continuationIndex,
            RazorVueRazorIrNode codeNode,
            out IOperation initializer)
        {
            initializer = default!;
            if (continuationIndex >= nodes.Count)
                return false;

            var nextNode = nodes[continuationIndex];
            if (nextNode.Kind != RazorVueRazorIrNodeKind.CSharpCode ||
                IsIgnorableTemplateCodeNode(nextNode))
            {
                return false;
            }

            var sourceSpan = GetRequiredSourceSpan(nextNode, "CSharpCodeIntermediateNode template local assignment code block");
            var operation = _resolver.ResolveRequiredOperation(sourceSpan, "Razor IR template local assignment code block");
            if (!TryExtractSimpleAssignmentToLocal(operation, declarator.Symbol, out initializer))
                return false;

            continuationIndex++;
            return true;
        }

        private static bool TryExtractSimpleAssignmentToLocal(
            IOperation operation,
            ILocalSymbol targetLocal,
            out IOperation initializer)
        {
            initializer = default!;
            switch (Unwrap(operation))
            {
                case IExpressionStatementOperation expressionStatement:
                    return TryExtractSimpleAssignmentToLocal(expressionStatement.Operation, targetLocal, out initializer);
                case ISimpleAssignmentOperation assignment
                    when assignment.Target is ILocalReferenceOperation localReference &&
                         SymbolEqualityComparer.Default.Equals(localReference.Local, targetLocal):
                    initializer = Unwrap(assignment.Value) ?? assignment.Value;
                    return true;
                case IBlockOperation block when block.Operations.Length == 1:
                    return TryExtractSimpleAssignmentToLocal(block.Operations[0], targetLocal, out initializer);
                default:
                    return false;
            }
        }

        private bool TryCollectTemplateLocalDeclarators(
            RazorVueRazorIrNode codeNode,
            IOperation operation,
            ImmutableArray<IVariableDeclaratorOperation>.Builder builder,
            Dictionary<ILocalSymbol, IOperation> immediateAssignedInitializers,
            out PendingTemplateControlNode? pendingControlNode)
        {
            _lastTemplateDeclarationScanEncounteredSupplementalDeclarations = false;
            pendingControlNode = null;
            switch (Unwrap(operation))
            {
                case IVariableDeclarationGroupOperation declarationGroup:
                    CollectDeclarators(declarationGroup.Declarations, builder);
                    return builder.Count > 0;
                case IVariableDeclarationOperation declarationOperation:
                    CollectDeclarators([declarationOperation], builder);
                    return builder.Count > 0;
                case ILocalFunctionOperation localFunctionOperation:
                    _lastTemplateDeclarationScanEncounteredSupplementalDeclarations = true;
                    MarkConsumedTemplateNodesForOperation(localFunctionOperation);
                    return true;
                case IBlockOperation blockOperation:
                    var encounteredNonDeclaration = false;
                    var pendingDeclarators = new HashSet<ILocalSymbol>(SymbolEqualityComparer.Default);
                    foreach (var child in blockOperation.Operations)
                    {
                        switch (Unwrap(child))
                        {
                            case null:
                            case IEmptyOperation:
                                continue;
                            case IVariableDeclarationGroupOperation childGroup:
                                if (encounteredNonDeclaration)
                                {
                                    throw CreateUnsupportedAttributeException(
                                        GetBestSourceSpan(codeNode),
                                        $"RazorVue Razor IR frontend only supports template code blocks whose immutable local declarations appear before any control-flow statement in component '{_snapshot.Descriptor.FullName}'.");
                                }

                                CollectDeclarators(childGroup.Declarations, builder);
                                RegisterPendingTemplateDeclarators(childGroup.Declarations, pendingDeclarators);
                                continue;
                            case IVariableDeclarationOperation childDeclaration:
                                if (encounteredNonDeclaration)
                                {
                                    throw CreateUnsupportedAttributeException(
                                        GetBestSourceSpan(codeNode),
                                        $"RazorVue Razor IR frontend only supports template code blocks whose immutable local declarations appear before any control-flow statement in component '{_snapshot.Descriptor.FullName}'.");
                                }

                                CollectDeclarators([childDeclaration], builder);
                                RegisterPendingTemplateDeclarators([childDeclaration], pendingDeclarators);
                                continue;
                            case ILocalFunctionOperation:
                                if (encounteredNonDeclaration)
                                {
                                    throw CreateUnsupportedAttributeException(
                                        GetBestSourceSpan(codeNode),
                                        $"RazorVue Razor IR frontend only supports template code blocks whose immutable local declarations appear before any control-flow statement in component '{_snapshot.Descriptor.FullName}'.");
                                }

                                _lastTemplateDeclarationScanEncounteredSupplementalDeclarations = true;
                                MarkConsumedTemplateNodesForOperation(child);
                                continue;
                            case IExpressionStatementOperation expressionStatement
                                when TryConsumePendingTemplateDeclarationAssignment(expressionStatement, pendingDeclarators, out var assignedLocal, out var assignedInitializer):
                                immediateAssignedInitializers[assignedLocal] = assignedInitializer;
                                continue;
                            case IConditionalOperation conditionalOperation
                                when builder.Count > 0:
                                if (!encounteredNonDeclaration &&
                                    pendingControlNode is null &&
                                    TryCreatePendingTemplateImperativeNode(codeNode, blockOperation.Operations, child, out pendingControlNode))
                                {
                                    encounteredNonDeclaration = true;
                                    continue;
                                }

                                if (!encounteredNonDeclaration &&
                                    pendingControlNode is null &&
                                    TryCreatePendingTemplateControlNode(codeNode, conditionalOperation, out pendingControlNode))
                                {
                                    encounteredNonDeclaration = true;
                                    continue;
                                }

                                encounteredNonDeclaration = true;
                                continue;
                            case IForEachLoopOperation forEachOperation
                                when builder.Count > 0:
                                if (!encounteredNonDeclaration &&
                                    pendingControlNode is null &&
                                    TryCreatePendingTemplateImperativeNode(codeNode, blockOperation.Operations, child, out pendingControlNode))
                                {
                                    encounteredNonDeclaration = true;
                                    continue;
                                }

                                if (!encounteredNonDeclaration &&
                                    pendingControlNode is null &&
                                    TryCreatePendingTemplateControlNode(codeNode, forEachOperation, out pendingControlNode))
                                {
                                    encounteredNonDeclaration = true;
                                    continue;
                                }

                                encounteredNonDeclaration = true;
                                continue;
                            case IForLoopOperation forOperation
                                when builder.Count > 0:
                                if (!encounteredNonDeclaration &&
                                    pendingControlNode is null &&
                                    TryCreatePendingTemplateImperativeNode(codeNode, blockOperation.Operations, child, out pendingControlNode))
                                {
                                    encounteredNonDeclaration = true;
                                    continue;
                                }

                                if (!encounteredNonDeclaration &&
                                    pendingControlNode is null &&
                                    TryCreatePendingTemplateControlNode(codeNode, forOperation, out pendingControlNode))
                                {
                                    encounteredNonDeclaration = true;
                                    continue;
                                }

                                encounteredNonDeclaration = true;
                                continue;
                            case IExpressionStatementOperation expressionStatement
                                when IsGeneratedTemplateBuilderStatement(codeNode, expressionStatement):
                                continue;
                            case IReturnOperation returnOperation
                                when IsGeneratedTemplateBuilderReturn(codeNode, returnOperation):
                                continue;
                            default:
                                if (builder.Count > 0 &&
                                    !encounteredNonDeclaration &&
                                    pendingControlNode is null &&
                                    TryCreatePendingTemplateImperativeNode(codeNode, blockOperation.Operations, child, out pendingControlNode))
                                {
                                    encounteredNonDeclaration = true;
                                    continue;
                                }

                                throw CreateUnsupportedAttributeException(
                                    GetBestSourceSpan(codeNode),
                                    $"RazorVue Razor IR frontend only supports template code blocks that contain immutable local declarations in component '{_snapshot.Descriptor.FullName}'.");
                        }
                    }

                    return builder.Count > 0 ||
                           pendingControlNode is not null ||
                           _lastTemplateDeclarationScanEncounteredSupplementalDeclarations;
                default:
                    return false;
            }
        }

        private void RegisterPendingTemplateDeclarators(
            IEnumerable<IVariableDeclarationOperation> declarations,
            HashSet<ILocalSymbol> pendingDeclarators)
        {
            foreach (var declaration in declarations)
            {
                foreach (var declarator in declaration.Declarators)
                {
                    if (declarator.Initializer?.Value is not null)
                        continue;

                    if (IsRenderTreeBuilderType(declarator.Symbol.Type))
                        continue;

                    pendingDeclarators.Add(declarator.Symbol);
                }
            }
        }

        private static bool TryConsumePendingTemplateDeclarationAssignment(
            IExpressionStatementOperation expressionStatement,
            HashSet<ILocalSymbol> pendingDeclarators,
            out ILocalSymbol localSymbol,
            out IOperation initializer)
        {
            localSymbol = default!;
            initializer = default!;
            if (pendingDeclarators.Count == 0 ||
                Unwrap(expressionStatement.Operation) is not ISimpleAssignmentOperation assignment ||
                assignment.Target is not ILocalReferenceOperation localReference)
            {
                return false;
            }

            if (!pendingDeclarators.Remove(localReference.Local))
                return false;

            localSymbol = localReference.Local;
            initializer = Unwrap(assignment.Value) ?? assignment.Value;
            return true;
        }

        private bool TryCreatePendingTemplateControlNode(
            RazorVueRazorIrNode codeNode,
            IConditionalOperation operation,
            out PendingTemplateControlNode? pendingControlNode)
        {
            pendingControlNode = null;
            var sourceSpan = GetRequiredSourceSpan(codeNode, "CSharpCodeIntermediateNode template conditional code block");
            if (!_resolver.TryResolveConditional(operation, out _))
                return false;

            pendingControlNode = PendingTemplateControlNode.CreateConditional(sourceSpan, operation);
            return true;
        }

        private bool TryCreatePendingTemplateControlNode(
            RazorVueRazorIrNode codeNode,
            IForEachLoopOperation operation,
            out PendingTemplateControlNode? pendingControlNode)
        {
            pendingControlNode = null;
            var sourceSpan = GetRequiredSourceSpan(codeNode, "CSharpCodeIntermediateNode template foreach code block");
            if (!_resolver.TryResolveForEach(operation, out _))
                return false;

            pendingControlNode = PendingTemplateControlNode.CreateForEach(sourceSpan, operation);
            return true;
        }

        private bool TryCreatePendingTemplateControlNode(
            RazorVueRazorIrNode codeNode,
            IForLoopOperation operation,
            out PendingTemplateControlNode? pendingControlNode)
        {
            pendingControlNode = null;
            var sourceSpan = GetRequiredSourceSpan(codeNode, "CSharpCodeIntermediateNode template for code block");
            if (!_resolver.TryResolveFor(operation, out _))
                return false;

            pendingControlNode = PendingTemplateControlNode.CreateFor(sourceSpan, operation);
            return true;
        }

        private bool TryCreatePendingTemplateImperativeNode(
            RazorVueRazorIrNode codeNode,
            ImmutableArray<IOperation> siblingOperations,
            IOperation startingOperation,
            out PendingTemplateControlNode? pendingControlNode)
        {
            pendingControlNode = null;
            var sourceSpan = GetRequiredSourceSpan(codeNode, "CSharpCodeIntermediateNode template imperative code block");
            var startIndex = -1;

            for (var index = 0; index < siblingOperations.Length; index++)
            {
                if (ReferenceEquals(siblingOperations[index], startingOperation))
                {
                    startIndex = index;
                    break;
                }
            }

            if (startIndex < 0)
                return false;

            var tailOperations = siblingOperations.Skip(startIndex).ToImmutableArray();
            if (tailOperations.All(operation => IsIgnorableGeneratedTemplateTailOperation(codeNode, operation)))
                return false;

            if (!RazorVueImperativeRenderPromotionAnalyzer.ShouldPromoteBody(tailOperations))
                return false;

            pendingControlNode = PendingTemplateControlNode.CreateImperative(
                sourceSpan,
                tailOperations,
                RazorVueImperativeRenderPromotionAnalyzer.ClassifyBodyKind(tailOperations));
            return true;
        }

        private bool IsIgnorableGeneratedTemplateTailOperation(
            RazorVueRazorIrNode codeNode,
            IOperation operation)
        {
            var current = Unwrap(operation);
            return current switch
            {
                null => true,
                IExpressionStatementOperation expressionStatement => IsGeneratedTemplateBuilderStatement(codeNode, expressionStatement),
                IReturnOperation returnOperation => IsGeneratedTemplateBuilderReturn(codeNode, returnOperation),
                IBlockOperation block => block.Operations.All(child => IsIgnorableGeneratedTemplateTailOperation(codeNode, child)),
                _ => false
            };
        }

        private static void CollectDeclarators(
            IEnumerable<IVariableDeclarationOperation> declarations,
            ImmutableArray<IVariableDeclaratorOperation>.Builder builder)
        {
            foreach (var declaration in declarations)
            {
                foreach (var declarator in declaration.Declarators)
                    builder.Add(declarator);
            }
        }

        private bool IsGeneratedTemplateBuilderStatement(
            RazorVueRazorIrNode codeNode,
            IExpressionStatementOperation expressionStatement)
        {
            if (expressionStatement.Operation is not IInvocationOperation invocation)
                return false;

            if (!IsRenderTreeBuilderInvocation(invocation))
                return false;

            var sourceSpan = CreateSourceSpanFromSyntax(expressionStatement.Syntax);
            if (sourceSpan is null)
                return true;

            return !IsWithinCodeNodeSourceRange(codeNode, sourceSpan.Value);
        }

        private bool IsGeneratedTemplateBuilderReturn(
            RazorVueRazorIrNode codeNode,
            IReturnOperation returnOperation)
        {
            var sourceSpan = CreateSourceSpanFromSyntax(returnOperation.Syntax);
            if (sourceSpan is null)
                return true;

            return !IsWithinCodeNodeSourceRange(codeNode, sourceSpan.Value);
        }

        private bool IsWithinCodeNodeSourceRange(
            RazorVueRazorIrNode codeNode,
            RazorVueRazorSourceSpan sourceSpan)
        {
            var codeRange = TryGetNodeSourceRange(codeNode);
            if (codeRange is null || string.IsNullOrWhiteSpace(sourceSpan.FilePath))
                return false;

            if (!PathsEqual(codeRange.Value.FilePath, sourceSpan.FilePath))
                return false;

            var candidateStart = sourceSpan.AbsoluteIndex;
            var candidateEnd = sourceSpan.AbsoluteIndex + sourceSpan.Length;
            return candidateStart >= codeRange.Value.Start &&
                   candidateEnd <= codeRange.Value.End;
        }

        private static bool IsRenderTreeBuilderInvocation(IInvocationOperation invocation)
        {
            var instance = Unwrap(invocation.Instance);
            return instance is IParameterReferenceOperation parameterReference &&
                   IsRenderTreeBuilderType(parameterReference.Parameter.Type);
        }

        private void ValidateTemplateScopedInitializer(
            IVariableDeclaratorOperation declarator,
            IOperation initializer,
            ImmutableHashSet<ILocalSymbol> allowedLocalSymbols,
            ImmutableHashSet<IParameterSymbol> allowedParameterSymbols)
        {
            foreach (var operation in EnumerateSelfAndDescendants(initializer))
            {
                switch (Unwrap(operation))
                {
                    case null:
                        continue;
                    case ILocalReferenceOperation localReference when !allowedLocalSymbols.Contains(localReference.Local):
                        throw CreateUnsupportedAttributeException(
                            CreateSourceSpanFromSyntax(declarator.Syntax),
                            $"RazorVue template-scoped local '{declarator.Symbol.Name}' in component '{_snapshot.Descriptor.FullName}' cannot capture unsupported local '{localReference.Local.Name}'. Only previously declared template locals and active loop locals are allowed.");
                    case IParameterReferenceOperation parameterReference when
                        !allowedParameterSymbols.Contains(parameterReference.Parameter) &&
                        !IsAnonymousFunctionParameter(parameterReference.Parameter):
                        throw CreateUnsupportedAttributeException(
                            CreateSourceSpanFromSyntax(declarator.Syntax),
                            $"RazorVue template-scoped local '{declarator.Symbol.Name}' in component '{_snapshot.Descriptor.FullName}' cannot capture unsupported parameter '{parameterReference.Parameter.Name}'.");
                    case IAnonymousFunctionOperation:
                    case IDelegateCreationOperation:
                    case IAssignmentOperation:
                    case IIncrementOrDecrementOperation:
                        throw CreateUnsupportedAttributeException(
                            CreateSourceSpanFromSyntax(declarator.Syntax),
                            $"RazorVue template-scoped local '{declarator.Symbol.Name}' in component '{_snapshot.Descriptor.FullName}' must be an immutable value/cache initializer without nested write or callable template state.");
                }
            }
        }

        private static IEnumerable<IOperation> EnumerateSelfAndDescendants(IOperation root)
        {
            yield return root;
            foreach (var descendant in root.Descendants())
                yield return descendant;
        }

        private static bool IsRenderTreeBuilderType(ITypeSymbol? typeSymbol)
            => string.Equals(typeSymbol?.Name, "RenderTreeBuilder", StringComparison.Ordinal);

        private bool IsAnonymousFunctionParameter(IParameterSymbol parameter)
            => parameter.ContainingSymbol is IMethodSymbol { MethodKind: MethodKind.LambdaMethod or MethodKind.AnonymousFunction };

        private bool TryConvertStoredLocalCarrierComponentSlotTemplate(
            string attributeName,
            string? attributeTypeName,
            IOperation? value,
            ImmutableArray<RazorVueSourceOrigin> origins,
            ImmutableArray<RazorVueRenderNode>.Builder children,
            ImmutableArray<RazorVueComponentSlotTemplateNode>.Builder slotTemplates)
        {
            if (string.IsNullOrWhiteSpace(attributeName) ||
                !IsRenderFragmentTypeName(attributeTypeName) ||
                !TryResolveStoredSlotTemplate(value, out var slotTemplate))
            {
                return false;
            }

            if (string.Equals(attributeName, "ChildContent", StringComparison.Ordinal) &&
                string.IsNullOrWhiteSpace(slotTemplate.ParameterName))
            {
                children.AddRange(MaterializeCapturedTemplateChildren(slotTemplate, origins).Children);
                return true;
            }

            slotTemplates.Add(new RazorVueComponentSlotTemplateNode(
                PublicName: attributeName,
                SlotName: string.Equals(attributeName, "ChildContent", StringComparison.Ordinal)
                    ? "default"
                    : ToLowerCamelCase(attributeName),
                ParameterName: slotTemplate.ParameterName,
                ParameterSymbol: slotTemplate.ParameterSymbol,
                Children: MaterializeCapturedTemplateChildren(slotTemplate, origins),
                Origins: origins));
            return true;
        }

        private bool TryResolveStoredSlotTemplate(IOperation? operation, out ParsedSlotTemplate slotTemplate)
        {
            slotTemplate = default;
            if (TryResolveStoredLocalSlotTemplate(operation, out slotTemplate))
                return true;

            if (TryResolveCurrentComponentMemberSlotTemplate(operation, out slotTemplate))
                return true;

            if (TryResolveCurrentComponentFragmentFactory(operation, out slotTemplate))
                return true;

            return false;
        }

        private void RegisterRenderFragmentLocalCarrier(
            IVariableDeclaratorOperation declarator,
            ImmutableHashSet<ILocalSymbol> allowedLocalSymbols,
            ImmutableHashSet<IParameterSymbol> allowedParameterSymbols,
            out bool consumeFollowingTemplateNode)
        {
            consumeFollowingTemplateNode = false;
            var initializer = TryGetSourceStableRenderFragmentInitializer(declarator.Symbol);
            if (initializer is null)
            {
                var hasLaterWrites =
                    RazorVueImperativeRenderFragmentCarrierHelper.IsSourceStableLocalRenderFragmentInitializerInvalidatedByLaterWrites(
                        _context.Compilation,
                        declarator.Symbol);
                var failureMessage =
                    declarator.Initializer?.Value is null
                        ? $"RazorVue RenderFragment local '{declarator.Symbol.Name}' in component '{_snapshot.Descriptor.FullName}' must be assigned exactly once by the immediately following simple assignment statement and cannot be observed through later writes."
                        : $"RazorVue RenderFragment local '{declarator.Symbol.Name}' in component '{_snapshot.Descriptor.FullName}' requires an analyzable initializer.";
                if (hasLaterWrites)
                {
                    failureMessage =
                        declarator.Initializer?.Value is null
                            ? $"RazorVue RenderFragment local '{declarator.Symbol.Name}' in component '{_snapshot.Descriptor.FullName}' must be assigned exactly once by the immediately following simple assignment statement and cannot be observed through later writes."
                            : $"RazorVue RenderFragment local '{declarator.Symbol.Name}' in component '{_snapshot.Descriptor.FullName}' cannot be observed through later writes. Declaration-initialized local carriers must remain source-stable.";
                }

                throw CreateUnsupportedAttributeException(
                    CreateSourceSpanFromSyntax(declarator.Syntax),
                    failureMessage);
            }

            if (!TryResolveRenderFragmentLocalCarrierTemplate(
                    declarator,
                    initializer,
                    allowedLocalSymbols,
                    allowedParameterSymbols,
                    out var slotTemplate,
                    out consumeFollowingTemplateNode))
            {
                throw CreateUnsupportedAttributeException(
                    CreateSourceSpanFromSyntax(declarator.Syntax),
                    $"RazorVue RenderFragment local '{declarator.Symbol.Name}' in component '{_snapshot.Descriptor.FullName}' must be initialized from an analyzable inline template, current-component RenderFragment member, or supported fragment factory.");
            }

            _localRenderFragmentCarriers[declarator.Symbol] = slotTemplate;
        }

        private RazorVueRazorIrNode ConsumeRequiredRenderFragmentCarrierTemplateNode(
            IReadOnlyList<RazorVueRazorIrNode> nodes,
            ref int continuationIndex,
            RazorVueRazorIrNode codeNode,
            IVariableDeclaratorOperation declarator)
        {
            if (continuationIndex >= nodes.Count ||
                !IsTemplateIntermediateNode(nodes[continuationIndex]))
            {
                throw CreateUnsupportedAttributeException(
                    GetBestSourceSpan(codeNode),
                    $"RazorVue RenderFragment local '{declarator.Symbol.Name}' in component '{_snapshot.Descriptor.FullName}' requires an inline Razor template body that the Razor IR frontend can bind.");
            }

            var templateNode = nodes[continuationIndex];
            continuationIndex++;
            return templateNode;
        }

        private bool TryResolveRenderFragmentLocalCarrierTemplate(
            IVariableDeclaratorOperation declarator,
            IOperation initializer,
            ImmutableHashSet<ILocalSymbol> allowedLocalSymbols,
            ImmutableHashSet<IParameterSymbol> allowedParameterSymbols,
            out ParsedSlotTemplate slotTemplate,
            out bool consumeFollowingTemplateNode)
        {
            slotTemplate = default;
            consumeFollowingTemplateNode = false;
            return TryResolveRenderFragmentCarrierInitializerTemplate(
                declarator,
                initializer,
                allowedLocalSymbols,
                allowedParameterSymbols,
                out slotTemplate,
                out consumeFollowingTemplateNode);
        }

        private bool TryResolveRenderFragmentCarrierInitializerTemplate(
            IVariableDeclaratorOperation? declarator,
            IOperation initializer,
            ImmutableHashSet<ILocalSymbol> allowedLocalSymbols,
            ImmutableHashSet<IParameterSymbol> allowedParameterSymbols,
            out ParsedSlotTemplate slotTemplate,
            out bool consumeFollowingTemplateNode)
        {
            slotTemplate = default;
            consumeFollowingTemplateNode = false;

            if (TryResolveRenderFragmentCarrierTemplateFromOperation(initializer, out slotTemplate))
                return true;

            if (TryResolveRenderFragmentCarrierTemplateFromBuilderOperations(
                    initializer,
                    allowedLocalSymbols,
                    allowedParameterSymbols,
                    out slotTemplate))
            {
                consumeFollowingTemplateNode = declarator is not null;
                return true;
            }

            return TryResolveInlineRenderFragmentCarrierTemplate(
                declarator,
                initializer,
                allowedLocalSymbols,
                allowedParameterSymbols,
                out slotTemplate,
                out consumeFollowingTemplateNode);
        }

        private bool TryResolveRenderFragmentCarrierTemplateFromBuilderOperations(
            IOperation initializer,
            ImmutableHashSet<ILocalSymbol> allowedLocalSymbols,
            ImmutableHashSet<IParameterSymbol> allowedParameterSymbols,
            out ParsedSlotTemplate slotTemplate)
        {
            slotTemplate = default;
            if (!RazorVueRenderTreeExtractor.TryParseTemplateCarrier(
                    _context,
                    _snapshot,
                    initializer,
                    _localRenderFragmentCarriers.Select(static pair =>
                        new KeyValuePair<ILocalSymbol, RazorVueRenderTreeExtractor.ParsedTemplateCarrier>(
                            pair.Key,
                            new RazorVueRenderTreeExtractor.ParsedTemplateCarrier(
                                pair.Value.ParameterName,
                                pair.Value.ParameterSymbol,
                                pair.Value.Children,
                                pair.Value.CapturedBindings
                                    .Select(static binding => new RazorVueRenderTreeExtractor.CapturedValueBinding(binding.ParameterSymbol, binding.Initializer))
                                    .ToImmutableArray()))),
                    _memberRenderFragmentCarriers.Select(static pair =>
                        new KeyValuePair<ISymbol, RazorVueRenderTreeExtractor.ParsedTemplateCarrier>(
                            pair.Key,
                            new RazorVueRenderTreeExtractor.ParsedTemplateCarrier(
                                pair.Value.ParameterName,
                                pair.Value.ParameterSymbol,
                                pair.Value.Children,
                                pair.Value.CapturedBindings
                                    .Select(static binding => new RazorVueRenderTreeExtractor.CapturedValueBinding(binding.ParameterSymbol, binding.Initializer))
                                    .ToImmutableArray()))),
                    _factoryRenderFragmentCarriers.Select(static pair =>
                        new KeyValuePair<IMethodSymbol, RazorVueRenderTreeExtractor.ParsedTemplateCarrier>(
                            pair.Key,
                            new RazorVueRenderTreeExtractor.ParsedTemplateCarrier(
                                pair.Value.ParameterName,
                                pair.Value.ParameterSymbol,
                                pair.Value.Children,
                                pair.Value.CapturedBindings
                                    .Select(static binding => new RazorVueRenderTreeExtractor.CapturedValueBinding(binding.ParameterSymbol, binding.Initializer))
                                    .ToImmutableArray()))),
                    _activeRenderFragmentMembers,
                    _activeRenderFragmentFactories,
                    allowedLocalSymbols,
                    allowedParameterSymbols,
                    out var parsedTemplate))
            {
                return false;
            }

            slotTemplate = new ParsedSlotTemplate(
                parsedTemplate.ParameterName,
                parsedTemplate.ParameterSymbol,
                parsedTemplate.Children,
                parsedTemplate.CapturedBindings
                    .Select(static binding => new RenderHelperValueBinding(binding.ParameterSymbol, binding.Initializer))
                    .ToImmutableArray());
            return true;
        }

        private bool TryResolveInlineRenderFragmentCarrierTemplate(
            IVariableDeclaratorOperation? declarator,
            IOperation initializer,
            ImmutableHashSet<ILocalSymbol> allowedLocalSymbols,
            ImmutableHashSet<IParameterSymbol> allowedParameterSymbols,
            out ParsedSlotTemplate slotTemplate,
            out bool consumeFollowingTemplateNode)
        {
            slotTemplate = default;
            consumeFollowingTemplateNode = false;
            if (!TryParseRenderFragmentCarrierSignature(
                    initializer,
                    out var parameterName,
                    out var parameterSymbol))
            {
                return false;
            }

            if (!TryFindTemplateNodeForOperation(initializer, out var templateNode))
                return false;

            consumeFollowingTemplateNode = true;
            MarkConsumedTemplateNode(templateNode);
            var templateParameterScope = parameterSymbol is null
                ? allowedParameterSymbols
                : allowedParameterSymbols.Add(parameterSymbol);
            var children = ConvertTemplateMethodBody(
                templateNode.Children,
                allowImperativePromotion: false,
                allowedLocalSymbols,
                templateParameterScope);
            slotTemplate = ParsedSlotTemplate.Create(
                parameterName,
                parameterSymbol,
                children);
            return true;
        }

        private bool TryFindTemplateNodeForOperation(IOperation operation, out RazorVueRazorIrNode templateNode)
        {
            templateNode = default!;
            if (_documentRoot is null)
                return false;

            if (!TryGetTemplateAnchorRange(operation, out var anchorRange))
                return false;

            var candidates = EnumerateNodes(_documentRoot)
                .Where(IsTemplateIntermediateNode)
                .Select(node => new
                {
                    Node = node,
                    Range = TryGetNodeSourceRange(node)
                })
                .Where(item => item.Range is not null &&
                               PathsEqual(item.Range.Value.FilePath, anchorRange.FilePath) &&
                               item.Range.Value.Start <= anchorRange.Start &&
                               item.Range.Value.End >= anchorRange.End)
                .OrderBy(item => item.Range!.Value.Length)
                .ThenBy(item => Math.Abs(item.Range!.Value.Start - anchorRange.Start))
                .ToArray();
            if (candidates.Length == 0)
            {
                candidates = EnumerateNodes(_documentRoot)
                    .Where(IsTemplateIntermediateNode)
                    .Select(node => new
                    {
                        Node = node,
                        Range = TryGetNodeSourceRange(node)
                    })
                    .Where(item => item.Range is not null &&
                                   PathsEqual(item.Range.Value.FilePath, anchorRange.FilePath) &&
                                   RangesOverlap(item.Range.Value, anchorRange))
                    .OrderBy(item => item.Range!.Value.Length)
                    .ThenBy(item => Math.Abs(item.Range!.Value.Start - anchorRange.Start))
                    .ToArray();
            }

            if (candidates.Length == 0)
            {
                candidates = EnumerateNodes(_documentRoot)
                    .Where(IsTemplateIntermediateNode)
                    .Select(node => new
                    {
                        Node = node,
                        Range = TryGetNodeSourceRange(node)
                    })
                    .Where(item => item.Range is not null &&
                                   PathsEqual(item.Range.Value.FilePath, anchorRange.FilePath) &&
                                   item.Range.Value.Start >= anchorRange.Start)
                    .OrderBy(item => item.Range!.Value.Start - anchorRange.Start)
                    .ThenBy(item => item.Range!.Value.Length)
                    .ToArray();
            }

            if (candidates.Length == 0)
                return false;

            templateNode = candidates[0].Node;
            return true;
        }

        private void MarkNearestTemplateNodeForOperation(IOperation operation)
        {
            if (_documentRoot is null)
                return;

            if (!TryGetTemplateAnchorRange(operation, out var anchorRange) &&
                !TryCreateSourceRangeFromSyntax(operation.Syntax, out anchorRange))
            {
                return;
            }

            var candidate = EnumerateNodes(_documentRoot)
                .Where(IsTemplateIntermediateNode)
                .Select(node => new
                {
                    Node = node,
                    Range = TryGetNodeSourceRange(node)
                })
                .Where(item => item.Range is not null &&
                               PathsEqual(item.Range.Value.FilePath, anchorRange.FilePath))
                .OrderBy(item =>
                {
                    var range = item.Range!.Value;
                    if (range.Start <= anchorRange.Start && range.End >= anchorRange.End)
                        return 0;

                    if (RangesOverlap(range, anchorRange))
                        return 1;

                    return 2;
                })
                .ThenBy(item =>
                {
                    var range = item.Range!.Value;
                    var distanceToStart = Math.Abs(range.Start - anchorRange.Start);
                    var distanceToEnd = Math.Abs(range.End - anchorRange.End);
                    return Math.Min(distanceToStart, distanceToEnd);
                })
                .ThenBy(item => item.Range!.Value.Length)
                .FirstOrDefault();

            if (candidate is not null)
                MarkConsumedTemplateNode(candidate.Node);
        }

        private static bool TryCreateSourceRangeFromSyntax(
            SyntaxNode? syntax,
            out RazorVueRazorIrOperationResolver.SourceRange sourceRange)
        {
            sourceRange = default;
            if (syntax is null)
                return false;

            if (CreateSourceSpanFromSyntax(syntax) is not { } sourceSpan ||
                string.IsNullOrWhiteSpace(sourceSpan.FilePath))
            {
                return false;
            }

            sourceRange = new RazorVueRazorIrOperationResolver.SourceRange(
                NormalizeComparablePath(sourceSpan.FilePath),
                sourceSpan.AbsoluteIndex,
                sourceSpan.AbsoluteIndex + sourceSpan.Length);
            return true;
        }

        private bool TryGetTemplateAnchorRange(
            IOperation operation,
            out RazorVueRazorIrOperationResolver.SourceRange anchorRange)
        {
            anchorRange = default;
            if (_resolver.TryMapGeneratedOperationToOriginalSourceSpan(operation, out var directSourceSpan) &&
                !string.IsNullOrWhiteSpace(directSourceSpan.FilePath))
            {
                anchorRange = new RazorVueRazorIrOperationResolver.SourceRange(
                    NormalizeComparablePath(directSourceSpan.FilePath),
                    directSourceSpan.AbsoluteIndex,
                    directSourceSpan.AbsoluteIndex + directSourceSpan.Length);
                return true;
            }

            var descendantRanges = EnumerateSelfAndDescendants(operation)
                .Select(current =>
                    _resolver.TryMapGeneratedOperationToOriginalSourceSpan(current, out var sourceSpan) &&
                    !string.IsNullOrWhiteSpace(sourceSpan.FilePath)
                        ? new RazorVueRazorIrOperationResolver.SourceRange(
                            NormalizeComparablePath(sourceSpan.FilePath),
                            sourceSpan.AbsoluteIndex,
                            sourceSpan.AbsoluteIndex + sourceSpan.Length)
                        : (RazorVueRazorIrOperationResolver.SourceRange?)null)
                .Where(static range => range is not null)
                .Select(static range => range!.Value)
                .ToArray();
            if (descendantRanges.Length == 0)
                return false;

            var filePath = descendantRanges[0].FilePath;
            var sameFileRanges = descendantRanges
                .Where(range => PathsEqual(range.FilePath, filePath))
                .ToArray();
            if (sameFileRanges.Length == 0)
                return false;

            anchorRange = new RazorVueRazorIrOperationResolver.SourceRange(
                filePath,
                sameFileRanges.Min(static range => range.Start),
                sameFileRanges.Max(static range => range.End));
            return true;
        }

        private void MarkConsumedTemplateNode(RazorVueRazorIrNode templateNode)
        {
            if (TryGetNodeSourceRange(templateNode) is not { } range)
                return;

            _consumedTemplateNodes.Add(new ConsumedTemplateNodeKey(
                range.FilePath,
                range.Start,
                range.Length));
        }

        private bool IsConsumedTemplateExtensionNode(RazorVueRazorIrNode node)
        {
            if (TryGetNodeSourceRange(node) is not { } range)
                return false;

            return _consumedTemplateNodes.Contains(new ConsumedTemplateNodeKey(
                range.FilePath,
                range.Start,
                range.Length));
        }

        private void MarkConsumedTemplateNodesForOperation(IOperation operation)
        {
            if (_documentRoot is null)
                return;

            if (!TryGetOperationCoverageRange(operation, out var sourceRange) &&
                !TryCreateSourceRangeFromSyntax(operation.Syntax, out sourceRange))
            {
                return;
            }

            var candidates = EnumerateNodes(_documentRoot)
                .Select(node => new
                {
                    Node = node,
                    Range = TryGetNodeSourceRange(node)
                })
                .Where(item => item.Range is not null &&
                               PathsEqual(item.Range.Value.FilePath, sourceRange.FilePath) &&
                               item.Range.Value.Start >= sourceRange.Start &&
                               item.Range.Value.End <= sourceRange.End)
                .ToArray();

            if (candidates.Length == 0)
            {
                candidates = EnumerateNodes(_documentRoot)
                    .Select(node => new
                    {
                        Node = node,
                        Range = TryGetNodeSourceRange(node)
                    })
                    .Where(item => item.Range is not null &&
                                   PathsEqual(item.Range.Value.FilePath, sourceRange.FilePath) &&
                                   RangesOverlap(item.Range.Value, sourceRange))
                    .ToArray();
            }

            foreach (var candidate in candidates)
                MarkConsumedTemplateNode(candidate.Node);
        }

        private bool TryGetOperationCoverageRange(
            IOperation operation,
            out RazorVueRazorIrOperationResolver.SourceRange sourceRange)
        {
            sourceRange = default;
            var ranges = ImmutableArray.CreateBuilder<RazorVueRazorIrOperationResolver.SourceRange>();

            if (_resolver.TryMapGeneratedOperationToOriginalSourceSpan(operation, out var directSourceSpan) &&
                !string.IsNullOrWhiteSpace(directSourceSpan.FilePath))
            {
                ranges.Add(new RazorVueRazorIrOperationResolver.SourceRange(
                    NormalizeComparablePath(directSourceSpan.FilePath),
                    directSourceSpan.AbsoluteIndex,
                    directSourceSpan.AbsoluteIndex + directSourceSpan.Length));
            }

            foreach (var current in EnumerateSelfAndDescendants(operation))
            {
                if (!_resolver.TryMapGeneratedOperationToOriginalSourceSpan(current, out var mappedSourceSpan) ||
                    string.IsNullOrWhiteSpace(mappedSourceSpan.FilePath))
                {
                    continue;
                }

                ranges.Add(new RazorVueRazorIrOperationResolver.SourceRange(
                    NormalizeComparablePath(mappedSourceSpan.FilePath),
                    mappedSourceSpan.AbsoluteIndex,
                    mappedSourceSpan.AbsoluteIndex + mappedSourceSpan.Length));
            }

            if (ranges.Count == 0)
                return false;

            var filePath = ranges[0].FilePath;
            var sameFileRanges = ranges
                .Where(range => PathsEqual(range.FilePath, filePath))
                .ToArray();
            if (sameFileRanges.Length == 0)
                return false;

            sourceRange = new RazorVueRazorIrOperationResolver.SourceRange(
                filePath,
                sameFileRanges.Min(static range => range.Start),
                sameFileRanges.Max(static range => range.End));
            return true;
        }

        private static IEnumerable<RazorVueRazorIrNode> EnumerateNodes(RazorVueRazorIrNode node)
        {
            yield return node;
            foreach (var child in node.Children)
            {
                foreach (var nested in EnumerateNodes(child))
                    yield return nested;
            }
        }

        private bool TryResolveRenderFragmentCarrierTemplateFromOperation(
            IOperation? operation,
            out ParsedSlotTemplate slotTemplate)
        {
            if (TryResolveStoredLocalSlotTemplate(operation, out slotTemplate))
                return true;

            if (TryResolveCurrentComponentMemberSlotTemplate(operation, out slotTemplate))
                return true;

            if (TryResolveCurrentComponentFragmentFactory(operation, out slotTemplate))
                return true;

            slotTemplate = default;
            return false;
        }

        private bool TryResolveStoredLocalSlotTemplate(IOperation? operation, out ParsedSlotTemplate slotTemplate)
        {
            slotTemplate = default;
            var current = Unwrap(operation);
            if (current is not ILocalReferenceOperation localReference)
                return false;

            return _localRenderFragmentCarriers.TryGetValue(localReference.Local, out slotTemplate);
        }

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

            if (!TryGetSupportedRenderFragmentFactorySignature(
                    invocation.TargetMethod,
                    out _,
                    out var failureMessage))
            {
                if (!IsRenderFragment(invocation.TargetMethod.ReturnType))
                    return false;

                throw CreateUnsupportedAttributeException(
                    CreateSourceSpanFromSyntax(invocation.Syntax),
                    failureMessage);
            }

            if (!TryGetRenderFragmentFactoryInvocationBindings(
                    invocation,
                    out var extraArgumentBindings,
                    out failureMessage))
            {
                throw CreateUnsupportedAttributeException(
                    CreateSourceSpanFromSyntax(invocation.Syntax),
                    failureMessage);
            }

            if (!TryResolveFactoryCarrier(invocation, requireZeroArguments: false, out var parsedFactoryTemplate))
                return false;

            slotTemplate = extraArgumentBindings.IsDefaultOrEmpty
                ? parsedFactoryTemplate
                : parsedFactoryTemplate.PrependCapturedBindings(extraArgumentBindings);
            return true;
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
                throw CreateUnsupportedAttributeException(
                    CreateSourceSpanFromSyntax(referenceOperation.Syntax),
                    $"RazorVue uses current-component RenderFragment member '{member.Name}' recursively; cyclic current-component RenderFragment member carriers are not supported in component '{_snapshot.Descriptor.FullName}'.");
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
            slotTemplate = default;
            if (requireZeroArguments &&
                (invocation.TargetMethod.Parameters.Length != 0 || invocation.Arguments.Length != 0))
            {
                return false;
            }

            var method = RazorVueMethodSymbolNormalizer.GetCanonicalMethod(invocation.TargetMethod);
            if (_factoryRenderFragmentCarriers.TryGetValue(method, out slotTemplate))
            {
                if (requireZeroArguments && !slotTemplate.CapturedBindings.IsDefaultOrEmpty)
                    return false;

                return true;
            }

            if (!_activeRenderFragmentFactories.Add(method))
            {
                throw CreateUnsupportedAttributeException(
                    CreateSourceSpanFromSyntax(invocation.Syntax),
                    $"RazorVue fragment factory method '{GetBuilderCallDisplayName(invocation)}' is recursive; recursive RenderFragment factory methods are not supported in component '{_snapshot.Descriptor.FullName}'.");
            }

            try
            {
                if (!TryCreateFactoryCarrier(invocation, out slotTemplate))
                    return false;

                _factoryRenderFragmentCarriers[method] = slotTemplate;
                if (requireZeroArguments && !slotTemplate.CapturedBindings.IsDefaultOrEmpty)
                    return false;

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
            if (!IsRenderFragment(member switch
                {
                    IPropertySymbol property => property.Type,
                    IFieldSymbol field => field.Type,
                    _ => null!
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

            return TryResolveRenderFragmentCarrierInitializerTemplate(
                declarator: null,
                initializer,
                EmptyLocalScope,
                EmptyParameterScope,
                out slotTemplate,
                out _);
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

                    return !RazorVueMemberWriteAnalysis.HasObservableWritesOutsideDeclarationInitializer(_context.Compilation, propertySymbol);
                case IFieldSymbol fieldSymbol:
                    if (fieldSymbol.IsReadOnly)
                        return true;

                    if (!RazorVueMemberWriteAnalysis.CanUseSourceStableMutableCarrierMember(fieldSymbol))
                        return false;

                    return !RazorVueMemberWriteAnalysis.HasObservableWritesOutsideDeclarationInitializer(_context.Compilation, fieldSymbol);
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
                if (!IsRenderFragment(method.ReturnType))
                    return false;

                throw CreateUnsupportedAttributeException(
                    CreateSourceSpanFromSyntax(invocation.Syntax),
                    failureMessage);
            }

            if (!TryGetRenderFragmentFactoryReturnedValue(invocation, out var returnedValue))
            {
                throw CreateUnsupportedAttributeException(
                    CreateSourceSpanFromSyntax(invocation.Syntax),
                    $"RazorVue fragment factory method '{GetBuilderCallDisplayName(invocation)}' must be source-authored with an analyzable return value in component '{_snapshot.Descriptor.FullName}'.");
            }

            if (!TryResolveRenderFragmentCarrierInitializerTemplate(
                    declarator: null,
                    initializer: returnedValue,
                    allowedLocalSymbols: EmptyLocalScope,
                    allowedParameterSymbols: EmptyParameterScope,
                    out var parsedFactoryTemplate,
                    out _))
            {
                throw CreateUnsupportedAttributeException(
                    CreateSourceSpanFromSyntax(invocation.Syntax),
                    $"RazorVue fragment factory method '{GetBuilderCallDisplayName(invocation)}' must return an analyzable RenderFragment template shape in component '{_snapshot.Descriptor.FullName}'.");
            }

            slotTemplate = parsedFactoryTemplate;
            return true;
        }

        private bool TryGetSupportedRenderFragmentFactorySignature(
            IMethodSymbol method,
            out ImmutableArray<IParameterSymbol> extraParameters,
            out string failureMessage)
        {
            extraParameters = ImmutableArray<IParameterSymbol>.Empty;
            failureMessage = string.Empty;
            if (!IsRenderFragment(method.ReturnType))
                return false;

            var helperDisplayName = method.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat);

            if (ContainsRenderTreeBuilderParameter(method))
            {
                failureMessage =
                    $"RazorVue fragment factory method '{helperDisplayName}' must not declare RenderTreeBuilder parameters in component '{_snapshot.Descriptor.FullName}'.";
                return false;
            }

            foreach (var parameter in method.Parameters)
            {
                if (parameter.RefKind != RefKind.None)
                {
                    var modifier = parameter.RefKind switch
                    {
                        RefKind.Ref => "ref",
                        RefKind.Out => "out",
                        RefKind.In => "in",
                        _ => parameter.RefKind.ToString().ToLowerInvariant()
                    };
                    failureMessage =
                        $"RazorVue fragment factory method '{helperDisplayName}' cannot declare '{modifier}' parameter '{parameter.Name}' in component '{_snapshot.Descriptor.FullName}'. Only ordinary by-value parameters are supported.";
                    return false;
                }
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
                    $"RazorVue fragment factory method '{GetBuilderCallDisplayName(invocation)}' must be invoked with arguments matching its declared signature in component '{_snapshot.Descriptor.FullName}'.";
                return false;
            }

            var boundParameters = new HashSet<IParameterSymbol>(SymbolEqualityComparer.Default);
            var bindingsBuilder = ImmutableArray.CreateBuilder<RenderHelperValueBinding>(invocation.Arguments.Length);
            foreach (var argument in invocation.Arguments)
            {
                if (argument.Parameter is not { } rawParameter)
                {
                    failureMessage =
                        $"RazorVue fragment factory method '{GetBuilderCallDisplayName(invocation)}' must use direct one-to-one argument binding in component '{_snapshot.Descriptor.FullName}'.";
                    return false;
                }

                var parameter = RazorVueMethodSymbolNormalizer.NormalizeParameter(invocation.TargetMethod, rawParameter);
                if (!boundParameters.Add(parameter))
                {
                    failureMessage =
                        $"RazorVue fragment factory method '{GetBuilderCallDisplayName(invocation)}' must use direct one-to-one argument binding in component '{_snapshot.Descriptor.FullName}'.";
                    return false;
                }

                var initializer = Unwrap(argument.Value);
                if (initializer is null)
                {
                    failureMessage =
                        $"RazorVue fragment factory method '{GetBuilderCallDisplayName(invocation)}' contains an unsupported argument value for parameter '{parameter.Name}' in component '{_snapshot.Descriptor.FullName}'.";
                    return false;
                }

                bindingsBuilder.Add(new RenderHelperValueBinding(parameter, initializer));
            }

            if (boundParameters.Count != invocation.TargetMethod.Parameters.Length)
            {
                failureMessage =
                    $"RazorVue fragment factory method '{GetBuilderCallDisplayName(invocation)}' must be invoked with arguments matching its declared signature in component '{_snapshot.Descriptor.FullName}'.";
                return false;
            }

            extraArgumentBindings = bindingsBuilder.ToImmutable();
            return true;
        }

        private bool TryGetRenderFragmentFactoryReturnedValue(
            IInvocationOperation invocation,
            out IOperation returnedValue)
        {
            returnedValue = default!;
            foreach (var syntaxReference in RazorVueMethodSymbolNormalizer.GetCanonicalMethod(invocation.TargetMethod).DeclaringSyntaxReferences)
            {
                var syntax = syntaxReference.GetSyntax();
                var semanticModel = _context.Compilation.GetSemanticModel(syntax.SyntaxTree);
                switch (syntax)
                {
                    case MethodDeclarationSyntax methodDeclaration:
                        if (methodDeclaration.ExpressionBody?.Expression is { } methodExpressionBody &&
                            RazorVuePropertyInitializerHelper.TryGetNormalizedOperation(semanticModel, methodExpressionBody, out var methodExpressionBodyOperation) &&
                            methodExpressionBodyOperation is not null)
                        {
                            returnedValue = methodExpressionBodyOperation;
                            return true;
                        }

                        if (methodDeclaration.Body is not null &&
                            semanticModel.GetOperation(methodDeclaration.Body) is IBlockOperation methodBlock &&
                            TryGetSingleReturnedValue(methodBlock, out var methodReturnValue) &&
                            methodReturnValue is not null)
                        {
                            returnedValue = methodReturnValue;
                            return true;
                        }

                        break;
                    case LocalFunctionStatementSyntax localFunction:
                        if (localFunction.ExpressionBody?.Expression is { } localExpressionBody &&
                            RazorVuePropertyInitializerHelper.TryGetNormalizedOperation(semanticModel, localExpressionBody, out var localExpressionBodyOperation) &&
                            localExpressionBodyOperation is not null)
                        {
                            returnedValue = localExpressionBodyOperation;
                            return true;
                        }

                        if (localFunction.Body is not null &&
                            semanticModel.GetOperation(localFunction.Body) is IBlockOperation localBlock &&
                            TryGetSingleReturnedValue(localBlock, out var localReturnValue) &&
                            localReturnValue is not null)
                        {
                            returnedValue = localReturnValue;
                            return true;
                        }

                        break;
                }
            }

            return false;
        }

        private IOperation? TryGetPropertyRenderFragmentInitializer(IPropertySymbol property)
        {
            foreach (var reference in property.DeclaringSyntaxReferences)
            {
                if (reference.GetSyntax() is not PropertyDeclarationSyntax declaration)
                    continue;

                var semanticModel = _context.Compilation.GetSemanticModel(declaration.SyntaxTree);
                if (RazorVuePropertyInitializerHelper.TryGetPropertyValueOperation(semanticModel, declaration, out var propertyOperation))
                    return propertyOperation;
            }

            return null;
        }

        private IOperation? TryGetSourceStableRenderFragmentInitializer(ILocalSymbol local)
            => RazorVueImperativeRenderFragmentCarrierHelper.TryGetSourceStableLocalRenderFragmentInitializer(
                _context.Compilation,
                local,
                out var initializer)
                ? initializer
                : null;

        private IOperation? TryGetFieldRenderFragmentInitializer(IFieldSymbol field)
        {
            foreach (var reference in field.DeclaringSyntaxReferences)
            {
                if (reference.GetSyntax() is not VariableDeclaratorSyntax declarator ||
                    declarator.Initializer?.Value is null)
                {
                    continue;
                }

                var semanticModel = _context.Compilation.GetSemanticModel(declarator.SyntaxTree);
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

        private void ConsumeRenderFragmentCarrierContinuation(
            IReadOnlyList<RazorVueRazorIrNode> nodes,
            ref int continuationIndex,
            ref PendingTemplateControlNode? pendingControlNode)
        {
            if (continuationIndex >= nodes.Count)
                return;

            var continuationNode = nodes[continuationIndex];
            if (!IsRenderFragmentCarrierContinuationCodeNode(continuationNode))
                return;

            var normalized = NormalizeTemplateCodeText(GetNodeText(continuationNode));
            var remainder = normalized.TrimStart(';');
            if (pendingControlNode is null &&
                TryCreatePendingTemplateControlNodeFromRenderFragmentContinuation(
                    continuationNode,
                    remainder,
                    out var emittedPendingControlNode))
            {
                pendingControlNode = emittedPendingControlNode;
            }

            continuationIndex++;
        }

        private bool TryCreatePendingTemplateControlNodeFromRenderFragmentContinuation(
            RazorVueRazorIrNode continuationNode,
            string normalizedContinuationRemainder,
            out PendingTemplateControlNode? pendingControlNode)
        {
            pendingControlNode = null;
            if (string.IsNullOrEmpty(normalizedContinuationRemainder) ||
                string.Equals(normalizedContinuationRemainder, "}", StringComparison.Ordinal))
            {
                return false;
            }

            if (normalizedContinuationRemainder.StartsWith("if(", StringComparison.Ordinal))
            {
                var sourceSpan = GetRequiredControlSourceSpan(continuationNode, "if");
                if (!_resolver.TryResolveConditional(sourceSpan, out var resolvedConditional))
                    return false;

                pendingControlNode = PendingTemplateControlNode.CreateConditional(sourceSpan, resolvedConditional.Operation);
                return true;
            }

            if (normalizedContinuationRemainder.StartsWith("foreach(", StringComparison.Ordinal))
            {
                var sourceSpan = GetRequiredControlSourceSpan(continuationNode, "foreach");
                if (!_resolver.TryResolveForEach(sourceSpan, out var resolvedLoop))
                    return false;

                pendingControlNode = PendingTemplateControlNode.CreateForEach(sourceSpan, resolvedLoop.Operation);
                return true;
            }

            if (normalizedContinuationRemainder.StartsWith("for(", StringComparison.Ordinal))
            {
                var sourceSpan = GetRequiredControlSourceSpan(continuationNode, "for");
                if (!_resolver.TryResolveFor(sourceSpan, out var resolvedLoop))
                    return false;

                pendingControlNode = PendingTemplateControlNode.CreateFor(sourceSpan, resolvedLoop.Operation);
                return true;
            }

            return false;
        }

        private bool TryParseRenderFragmentCarrierSignature(
            IOperation initializer,
            out string? parameterName,
            out IParameterSymbol? parameterSymbol)
        {
            parameterName = null;
            parameterSymbol = null;
            if (!TryGetAnonymousFunction(initializer, out var anonymousFunction))
                return false;

            if (TryGetSingleBuilderParameter(anonymousFunction, out _))
                return true;

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

            if (!TryGetAnonymousFunction(returnedBuilderFactory, out var builderAnonymousFunction) ||
                !TryGetSingleBuilderParameter(builderAnonymousFunction, out _))
            {
                return false;
            }

            parameterName = slotContextParameter.Name;
            parameterSymbol = slotContextParameter;
            return true;
        }

        private static RazorVueRenderFragment MaterializeCapturedTemplateChildren(
            ParsedSlotTemplate slotTemplate,
            ImmutableArray<RazorVueSourceOrigin> origins)
            => WrapCapturedTemplateScopes(slotTemplate.Children, slotTemplate.CapturedBindings, origins);

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

        private static bool TryGetSingleReturnedValue(IBlockOperation block, out IOperation? returnedValue)
        {
            returnedValue = null;
            if (block.Operations.Length != 1 ||
                block.Operations[0] is not IReturnOperation returnOperation)
            {
                return false;
            }

            returnedValue = Unwrap(returnOperation.ReturnedValue);
            return returnedValue is not null;
        }

        private static bool TryGetAnonymousFunction(IOperation? operation, out IAnonymousFunctionOperation anonymousFunction)
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

        private static string GetBuilderCallDisplayName(IInvocationOperation invocation)
            => invocation.TargetMethod.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat);

        private static bool TryGetSingleBuilderParameter(
            IAnonymousFunctionOperation anonymousFunction,
            out IParameterSymbol builderParameter)
        {
            builderParameter = default!;
            var builderParameters = anonymousFunction.Symbol.Parameters
                .Where(static parameter =>
                    string.Equals(parameter.Name, "builder", StringComparison.Ordinal) ||
                    string.Equals(parameter.Type.Name, "RenderTreeBuilder", StringComparison.Ordinal))
                .ToArray();
            if (builderParameters.Length != 1)
                return false;

            builderParameter = builderParameters[0];
            return true;
        }

        private static RazorVueRazorSourceSpan? CreateSourceSpanFromSyntax(SyntaxNode syntax)
        {
            var location = syntax.GetLocation();
            var lineSpan = location.GetLineSpan();
            var path = lineSpan.Path;
            if (string.IsNullOrWhiteSpace(path))
                return null;

            return new RazorVueRazorSourceSpan(
                FilePath: path,
                AbsoluteIndex: location.SourceSpan.Start,
                LineIndex: lineSpan.StartLinePosition.Line,
                CharacterIndex: lineSpan.StartLinePosition.Character,
                Length: location.SourceSpan.Length);
        }

        private int FindControlStatementEndIndex(
            IReadOnlyList<RazorVueRazorIrNode> nodes,
            int startIndex,
            RazorVueRazorIrOperationResolver.SourceRange coveredRange,
            RazorVueRazorSourceSpan sourceSpan,
            string detail)
        {
            var matchingEndIndex = -1;
            var lastMatchingNodeIndex = -1;
            for (var candidateIndex = startIndex; candidateIndex < nodes.Count; candidateIndex++)
            {
                var candidateRange = TryGetNodeSourceRange(nodes[candidateIndex]);
                if (candidateRange is null)
                    continue;

                if (!PathsEqual(candidateRange.Value.FilePath, coveredRange.FilePath))
                    continue;

                if (!RangesOverlap(candidateRange.Value, coveredRange))
                    continue;

                matchingEndIndex = candidateIndex + 1;
                lastMatchingNodeIndex = candidateIndex;
            }

            if (matchingEndIndex < 0)
            {
                throw CreateUnsupportedAttributeException(
                    sourceSpan,
                    $"RazorVue Razor IR frontend could not determine the template extent of {detail} in component '{_snapshot.Descriptor.FullName}'.");
            }

            if (lastMatchingNodeIndex > startIndex &&
                matchingEndIndex > startIndex &&
                IsTemplateCodeNode(nodes[lastMatchingNodeIndex]) &&
                IsBoundaryToNextControlCodeNode(NormalizeTemplateCodeText(GetNodeText(nodes[lastMatchingNodeIndex]))))
            {
                matchingEndIndex--;
            }

            return matchingEndIndex;
        }

        private int FindPendingImperativeEndIndex(
            IReadOnlyList<RazorVueRazorIrNode> nodes,
            int startIndex,
            IReadOnlyList<IOperation> operations,
            RazorVueRazorSourceSpan sourceSpan)
        {
            if (operations.Count == 0)
                return startIndex;

            var mappedRanges = operations
                .Select(TryMapOperationToSourceRange)
                .Where(static range => range is not null)
                .Select(static range => range!.Value)
                .ToArray();

            if (mappedRanges.Length == 0)
                return startIndex;

            var filePath = mappedRanges[0].FilePath;
            if (mappedRanges.Any(range => !PathsEqual(range.FilePath, filePath)))
            {
                throw CreateUnsupportedAttributeException(
                    sourceSpan,
                    $"RazorVue Razor IR frontend could not determine a single template extent for embedded imperative block in component '{_snapshot.Descriptor.FullName}'.");
            }

            var mergedRange = new RazorVueRazorIrOperationResolver.SourceRange(
                filePath,
                mappedRanges.Min(static range => range.Start),
                mappedRanges.Max(static range => range.End));

            var matchingEndIndex = startIndex;
            for (var candidateIndex = startIndex; candidateIndex < nodes.Count; candidateIndex++)
            {
                var candidateRange = TryGetNodeSourceRange(nodes[candidateIndex]);
                if (candidateRange is null)
                    continue;

                if (!PathsEqual(candidateRange.Value.FilePath, mergedRange.FilePath))
                    continue;

                if (!RangesOverlap(candidateRange.Value, mergedRange))
                    continue;

                matchingEndIndex = candidateIndex + 1;
            }

            return matchingEndIndex;
        }

        private List<RazorVueRazorIrNode> SliceNodesByRange(
            IReadOnlyList<RazorVueRazorIrNode> nodes,
            RazorVueRazorIrOperationResolver.SourceRange range,
            RazorVueRazorSourceSpan sourceSpan,
            string detail,
            bool trimLeadingControlNode)
        {
            var selected = new List<RazorVueRazorIrNode>();
            foreach (var node in nodes)
            {
                var nodeRange = TryGetNodeSourceRange(node);
                if (nodeRange is null)
                    continue;

                if (!PathsEqual(nodeRange.Value.FilePath, range.FilePath))
                    continue;

                if (nodeRange.Value.End <= range.Start || nodeRange.Value.Start >= range.End)
                {
                    if (!IsTemplateCodeNode(node))
                        continue;

                    var normalizedCodeText = NormalizeTemplateCodeText(GetNodeText(node));
                    if (!(IsElseBoundaryCodeNode(normalizedCodeText) ||
                          IsElseIfBoundaryCodeNode(normalizedCodeText) ||
                          IsBoundaryToNextControlCodeNode(normalizedCodeText)))
                    {
                        continue;
                    }
                }

                selected.Add(node);
            }

            while (selected.Count > 0 &&
                   IsTemplateCodeNode(selected[0]) &&
                   IsIgnorableTemplateCodeNode(selected[0]))
            {
                selected.RemoveAt(0);
            }

            while (selected.Count > 0 &&
                   IsTemplateCodeNode(selected[selected.Count - 1]) &&
                   IsIgnorableTemplateCodeNode(selected[selected.Count - 1]))
            {
                selected.RemoveAt(selected.Count - 1);
            }

            if (trimLeadingControlNode &&
                selected.Count > 0 &&
                IsTemplateCodeNode(selected[0]) &&
                !StartsWithControlKeyword(GetNodeText(selected[0]), "if") &&
                !StartsWithControlKeyword(GetNodeText(selected[0]), "foreach") &&
                !StartsWithControlKeyword(GetNodeText(selected[0]), "for"))
            {
                selected.RemoveAt(0);
            }

            if (selected.Count == 0)
            {
                throw CreateUnsupportedAttributeException(
                    sourceSpan,
                    $"RazorVue Razor IR frontend could not bind {detail} nodes in component '{_snapshot.Descriptor.FullName}'.");
            }

            return selected;
        }

        private List<RazorVueRazorIrNode> BindCoveredControlBodyNodes(
            IReadOnlyList<RazorVueRazorIrNode> coveredNodes,
            RazorVueRazorSourceSpan sourceSpan,
            string detail)
        {
            var selected = coveredNodes.ToList();
            TrimIgnorableBoundaryCodeNodes(selected);
            if (selected.Count == 0)
            {
                throw CreateUnsupportedAttributeException(
                    sourceSpan,
                    $"RazorVue Razor IR frontend could not bind {detail} nodes in component '{_snapshot.Descriptor.FullName}'.");
            }

            return selected;
        }

        private List<RazorVueRazorIrNode> TakeLeadingBranchNodesUntilTopLevelElseBoundary(
            IReadOnlyList<RazorVueRazorIrNode> nodes,
            RazorVueRazorSourceSpan sourceSpan,
            string detail)
        {
            var selected = new List<RazorVueRazorIrNode>();
            var nestedControlDepth = 0;

            foreach (var node in nodes)
            {
                if (IsTemplateCodeNode(node))
                {
                    var normalized = NormalizeTemplateCodeText(GetNodeText(node));
                    if (nestedControlDepth == 0 &&
                        (IsElseBoundaryCodeNode(normalized) || IsElseIfBoundaryCodeNode(normalized)))
                    {
                        break;
                    }

                    if (StartsWithControlKeyword(GetNodeText(node), "if") ||
                        StartsWithControlKeyword(GetNodeText(node), "foreach") ||
                        StartsWithControlKeyword(GetNodeText(node), "for"))
                    {
                        selected.Add(node);
                        nestedControlDepth++;
                        continue;
                    }

                    if (IsPureClosingCodeNode(normalized))
                    {
                        if (nestedControlDepth == 0)
                            continue;

                        selected.Add(node);
                        nestedControlDepth--;
                        continue;
                    }
                }

                selected.Add(node);
            }

            TrimIgnorableBoundaryCodeNodes(selected);
            if (selected.Count == 0)
            {
                throw CreateUnsupportedAttributeException(
                    sourceSpan,
                    $"RazorVue Razor IR frontend could not bind {detail} nodes in component '{_snapshot.Descriptor.FullName}'.");
            }

            return selected;
        }

        private List<RazorVueRazorIrNode> TryTakeTrailingNodesAfterTopLevelElseBoundary(
            IReadOnlyList<RazorVueRazorIrNode> nodes,
            RazorVueRazorSourceSpan sourceSpan,
            string detail)
        {
            var nestedControlDepth = 0;
            for (var index = 0; index < nodes.Count; index++)
            {
                if (!IsTemplateCodeNode(nodes[index]))
                    continue;

                var codeNode = nodes[index];
                var normalized = NormalizeTemplateCodeText(GetNodeText(codeNode));
                if (nestedControlDepth == 0 &&
                    (IsElseBoundaryCodeNode(normalized) || IsElseIfBoundaryCodeNode(normalized)))
                {
                    var selected = nodes.Skip(index).ToList();
                    TrimIgnorableBoundaryCodeNodes(selected);
                    return selected;
                }

                if (StartsWithControlKeyword(GetNodeText(codeNode), "if") ||
                    StartsWithControlKeyword(GetNodeText(codeNode), "foreach") ||
                    StartsWithControlKeyword(GetNodeText(codeNode), "for"))
                {
                    nestedControlDepth++;
                    continue;
                }

                if (IsPureClosingCodeNode(normalized) && nestedControlDepth > 0)
                {
                    nestedControlDepth--;
                }
            }

            return [];
        }

        private (List<RazorVueRazorIrNode> WhenTrue, List<RazorVueRazorIrNode> WhenFalse) SplitConditionalBranchesByStructure(
            IReadOnlyList<RazorVueRazorIrNode> nodes,
            RazorVueRazorSourceSpan sourceSpan)
        {
            var whenTrue = new List<RazorVueRazorIrNode>();
            var whenFalse = new List<RazorVueRazorIrNode>();
            var current = whenTrue;
            var nestedControlDepth = 0;
            var sawTopLevelElseBoundary = false;

            foreach (var node in nodes)
            {
                if (IsTemplateCodeNode(node))
                {
                    var normalized = NormalizeTemplateCodeText(GetNodeText(node));
                    if (IsElseIfBoundaryCodeNode(normalized))
                    {
                        if (nestedControlDepth == 0)
                        {
                            sawTopLevelElseBoundary = true;
                            current = whenFalse;
                            current.Add(node);
                            nestedControlDepth = 1;
                            continue;
                        }

                        current.Add(node);
                        continue;
                    }

                    if (IsElseBoundaryCodeNode(normalized))
                    {
                        if (nestedControlDepth == 0)
                        {
                            if (sawTopLevelElseBoundary)
                            {
                                throw CreateUnsupportedAttributeException(
                                    sourceSpan,
                                    $"RazorVue Razor IR frontend encountered multiple top-level else boundaries in component '{_snapshot.Descriptor.FullName}'.");
                            }

                            sawTopLevelElseBoundary = true;
                            current = whenFalse;
                            continue;
                        }

                        current.Add(node);
                        continue;
                    }

                    if (StartsWithControlKeyword(GetNodeText(node), "if") ||
                        StartsWithControlKeyword(GetNodeText(node), "foreach") ||
                        StartsWithControlKeyword(GetNodeText(node), "for"))
                    {
                        current.Add(node);
                        nestedControlDepth++;
                        continue;
                    }

                    if (IsPureClosingCodeNode(normalized))
                    {
                        if (nestedControlDepth == 0)
                            continue;

                        current.Add(node);
                        nestedControlDepth--;
                        continue;
                    }
                }

                current.Add(node);
            }

            TrimIgnorableBoundaryCodeNodes(whenTrue);
            TrimIgnorableBoundaryCodeNodes(whenFalse);

            if (!sawTopLevelElseBoundary || nestedControlDepth != 0)
            {
                throw CreateUnsupportedAttributeException(
                    sourceSpan,
                    $"RazorVue Razor IR frontend could not structurally bind if/else branches in component '{_snapshot.Descriptor.FullName}'.");
            }

            if (whenTrue.Count == 0 || whenFalse.Count == 0)
            {
                throw CreateUnsupportedAttributeException(
                    sourceSpan,
                    $"RazorVue Razor IR frontend could not structurally bind if/else branches in component '{_snapshot.Descriptor.FullName}'.");
            }

            return (whenTrue, whenFalse);
        }

        private static void TrimIgnorableBoundaryCodeNodes(List<RazorVueRazorIrNode> nodes)
        {
            while (nodes.Count > 0 &&
                   IsTemplateCodeNode(nodes[0]) &&
                   IsIgnorableTemplateCodeNode(nodes[0]))
            {
                nodes.RemoveAt(0);
            }

            while (nodes.Count > 0 &&
                   IsTemplateCodeNode(nodes[nodes.Count - 1]) &&
                   IsIgnorableTemplateCodeNode(nodes[nodes.Count - 1]))
            {
                nodes.RemoveAt(nodes.Count - 1);
            }
        }

        private static bool IsTemplateCodeNode(RazorVueRazorIrNode node)
            => node.Kind == RazorVueRazorIrNodeKind.CSharpCode ||
               (node.Kind == RazorVueRazorIrNodeKind.IntermediateToken &&
                IsCSharpIntermediateToken(node));

        private static bool IsCSharpIntermediateToken(RazorVueRazorIrNode node)
            => node.Kind == RazorVueRazorIrNodeKind.IntermediateToken &&
               node.RuntimeTypeName.EndsWith(".CSharpIntermediateToken", StringComparison.Ordinal);

        private static bool IsIgnorableTemplateCodeNode(RazorVueRazorIrNode node)
        {
            var normalized = NormalizeTemplateCodeText(GetNodeText(node));
            return normalized.Length == 0 ||
                   string.Equals(normalized, ";", StringComparison.Ordinal) ||
                   string.Equals(normalized, "{", StringComparison.Ordinal) ||
                   string.Equals(normalized, "}", StringComparison.Ordinal) ||
                   string.Equals(normalized, "else", StringComparison.Ordinal) ||
                   string.Equals(normalized, "else{", StringComparison.Ordinal) ||
                   string.Equals(normalized, "}else{", StringComparison.Ordinal);
        }

        private static bool StartsWithControlKeyword(string text, string keyword)
        {
            if (string.IsNullOrWhiteSpace(text))
                return false;

            var trimmed = text.TrimStart();
            return trimmed.StartsWith(keyword + " ", StringComparison.Ordinal) ||
                   trimmed.StartsWith(keyword + "(", StringComparison.Ordinal);
        }

        private static bool StartsWithBoundaryControlKeyword(string normalizedText, string keyword)
        {
            if (string.IsNullOrEmpty(normalizedText))
                return false;

            return normalizedText.StartsWith("}" + keyword + "(", StringComparison.Ordinal) ||
                   normalizedText.StartsWith("}" + keyword, StringComparison.Ordinal);
        }

        private static bool IsBoundaryToNextControlCodeNode(string normalizedText)
            => StartsWithBoundaryControlKeyword(normalizedText, "if") ||
               StartsWithBoundaryControlKeyword(normalizedText, "foreach") ||
               StartsWithBoundaryControlKeyword(normalizedText, "for");

        private static bool IsElseBoundaryCodeNode(string normalized)
            => string.Equals(normalized, "else", StringComparison.Ordinal) ||
               string.Equals(normalized, "else{", StringComparison.Ordinal) ||
               string.Equals(normalized, "}else{", StringComparison.Ordinal);

        private static bool IsElseIfBoundaryCodeNode(string normalized)
            => normalized.StartsWith("elseif(", StringComparison.Ordinal) ||
               normalized.StartsWith("}elseif(", StringComparison.Ordinal);

        private static bool IsPureClosingCodeNode(string normalized)
            => string.Equals(normalized, "}", StringComparison.Ordinal);

        private static bool IsTemplateIntermediateNode(RazorVueRazorIrNode node)
            => node.RuntimeTypeName.EndsWith(".TemplateIntermediateNode", StringComparison.Ordinal) ||
               node.RuntimeTypeName.EndsWith("TemplateIntermediateNode", StringComparison.Ordinal);

        private static bool IsRenderFragmentCarrierContinuationCodeNode(RazorVueRazorIrNode node)
        {
            if (node.Kind != RazorVueRazorIrNodeKind.CSharpCode)
                return false;

            var normalized = NormalizeTemplateCodeText(GetNodeText(node));
            if (!normalized.StartsWith(";", StringComparison.Ordinal))
                return false;

            var remainder = normalized.TrimStart(';');
            return remainder.Length == 0 ||
                   string.Equals(remainder, "}", StringComparison.Ordinal) ||
                   remainder.StartsWith("if(", StringComparison.Ordinal) ||
                   remainder.StartsWith("foreach(", StringComparison.Ordinal) ||
                   remainder.StartsWith("for(", StringComparison.Ordinal);
        }

        private static string NormalizeTemplateCodeText(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return string.Empty;

            return new string(text.Where(static character => !char.IsWhiteSpace(character)).ToArray());
        }

        private static bool IsRenderFragmentTypeName(string? typeName)
            => NormalizeTypeName(typeName).StartsWith(
                "Microsoft.AspNetCore.Components.RenderFragment",
                StringComparison.Ordinal);

        private static string GetComponentName(RazorVueRazorIrNode node)
        {
            var normalizedTypeName = NormalizeTypeName(node.TypeName);
            if (!string.IsNullOrWhiteSpace(normalizedTypeName))
            {
                var lastDot = normalizedTypeName.LastIndexOf('.');
                return lastDot >= 0 ? normalizedTypeName.Substring(lastDot + 1) : normalizedTypeName;
            }

            return string.IsNullOrWhiteSpace(node.TagName) ? "Component" : node.TagName!;
        }

        private static string NormalizeTypeName(string? typeName)
        {
            if (string.IsNullOrWhiteSpace(typeName))
                return string.Empty;

            var current = typeName!;
            return current.StartsWith("global::", StringComparison.Ordinal)
                ? current.Substring("global::".Length)
                : current;
        }

        private static RazorVueRazorSourceSpan? GetBestSourceSpan(RazorVueRazorIrNode node)
        {
            foreach (var token in EnumerateTokens(node))
            {
                if (token.Source is not null)
                    return token.Source;
            }

            return node.Source;
        }

        private static RazorVueRazorSourceSpan GetRequiredSourceSpan(RazorVueRazorIrNode node, string detail)
            => GetBestSourceSpan(node)
               ?? throw new InvalidOperationException($"The Razor IR node '{detail}' did not expose a source span.");

        private static RazorVueRazorSourceSpan GetRequiredControlSourceSpan(RazorVueRazorIrNode node, string keyword)
        {
            var sourceSpan = GetRequiredSourceSpan(node, $"CSharpCodeIntermediateNode {keyword} header");
            var text = GetNodeText(node);
            if (string.IsNullOrEmpty(text))
                return sourceSpan;

            var keywordIndex = FindControlKeywordIndex(text, keyword);
            if (keywordIndex <= 0)
                return sourceSpan;

            return sourceSpan with
            {
                AbsoluteIndex = sourceSpan.AbsoluteIndex + keywordIndex,
                CharacterIndex = sourceSpan.CharacterIndex + keywordIndex,
                Length = Math.Max(1, sourceSpan.Length - keywordIndex)
            };
        }

        private static int FindControlKeywordIndex(string text, string keyword)
        {
            var searchIndex = 0;
            while (searchIndex < text.Length)
            {
                var candidateIndex = text.IndexOf(keyword, searchIndex, StringComparison.Ordinal);
                if (candidateIndex < 0)
                    return -1;

                var previous = candidateIndex == 0 ? '\0' : text[candidateIndex - 1];
                var nextIndex = candidateIndex + keyword.Length;
                var next = nextIndex >= text.Length ? '\0' : text[nextIndex];
                var previousIsBoundary = candidateIndex == 0 || char.IsWhiteSpace(previous) || previous is '{' or '}' or ';';
                var nextIsBoundary = next == '\0' || char.IsWhiteSpace(next) || next == '(';
                if (previousIsBoundary && nextIsBoundary)
                    return candidateIndex;

                searchIndex = candidateIndex + keyword.Length;
            }

            return -1;
        }

        private static RazorVueRazorIrOperationResolver.SourceRange? TryGetNodeSourceRange(RazorVueRazorIrNode node)
        {
            var sourceSpan = GetBestSourceSpan(node);
            if (sourceSpan is null || string.IsNullOrWhiteSpace(sourceSpan.Value.FilePath))
                return null;

            return new RazorVueRazorIrOperationResolver.SourceRange(
                NormalizeComparablePath(sourceSpan.Value.FilePath),
                sourceSpan.Value.AbsoluteIndex,
                sourceSpan.Value.AbsoluteIndex + sourceSpan.Value.Length);
        }

        private static bool RangesOverlap(
            RazorVueRazorIrOperationResolver.SourceRange left,
            RazorVueRazorIrOperationResolver.SourceRange right)
            => left.Start < right.End && right.Start < left.End;

        private static string GetNodeText(RazorVueRazorIrNode node)
        {
            var text = string.Concat(EnumerateTokens(node).Select(static token => token.Content));
            return text.Length == 0 && node.Kind == RazorVueRazorIrNodeKind.IntermediateToken
                ? GetNodeContent(node)
                : text;
        }

        private static string GetNodeContent(RazorVueRazorIrNode node)
            => node.Content ?? string.Empty;

        private static bool PathsEqual(string? left, string? right)
            => PathComparer.Equals(NormalizeComparablePath(left), NormalizeComparablePath(right));

        private static string NormalizeComparablePath(string? path)
        {
            if (string.IsNullOrWhiteSpace(path))
                return string.Empty;

            try
            {
                return System.IO.Path.GetFullPath(path).Replace('\\', '/');
            }
            catch (ArgumentException)
            {
                return path!.Replace('\\', '/');
            }
            catch (System.IO.PathTooLongException)
            {
                return path!.Replace('\\', '/');
            }
            catch (NotSupportedException)
            {
                return path!.Replace('\\', '/');
            }
            catch (System.IO.IOException)
            {
                return path!.Replace('\\', '/');
            }
        }

        private static string? GetMethodDeclarationName(RazorVueRazorIrNode node)
            => node.MethodName;

        private static IEnumerable<RazorVueRazorIrToken> EnumerateTokens(RazorVueRazorIrNode node)
        {
            foreach (var token in node.Tokens)
                yield return token;

            foreach (var child in node.Children)
            {
                foreach (var nested in EnumerateTokens(child))
                    yield return nested;
            }
        }

        private static ImmutableArray<RazorVueSourceOrigin> CreateOrigins(RazorVueRazorSourceSpan? sourceSpan)
        {
            var origin = CreateSourceOrigin(sourceSpan, RazorVueOriginKind.Template);
            return origin is null
                ? ImmutableArray<RazorVueSourceOrigin>.Empty
                : ImmutableArray.Create(origin);
        }

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

        private static StringComparer PathComparer
            => System.IO.Path.DirectorySeparatorChar == '\\'
                ? StringComparer.OrdinalIgnoreCase
                : StringComparer.Ordinal;

        private ImmutableArray<RazorVueRenderNode> ParseStaticMarkupFragment(
            string markup,
            ImmutableArray<RazorVueSourceOrigin> origins)
            => RazorVueStaticMarkupParser.Parse(
                markup,
                origins,
                new RazorVueStaticMarkupParser.Dependencies(
                    CreateLiteralStringOperation,
                    detail => CreateUnsupportedAttributeException(
                        null,
                        $"RazorVue Razor IR frontend {detail} in component '{_snapshot.Descriptor.FullName}'.")));

        private ImmutableArray<RazorVueRenderNode> MaterializeStaticMarkupFragment(
            RazorVueStaticMarkupValueHelper.StaticMarkupResolution resolution,
            ImmutableArray<RazorVueSourceOrigin> origins)
        {
            var fragment = new RazorVueRenderFragment(ParseStaticMarkupFragment(resolution.Markup, origins));
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

            return fragment.Children;
        }

        private string? TryGetStaticMarkupValue(IOperation? operation)
            => RazorVueStaticMarkupValueHelper.TryGetStaticMarkupValue(
                operation,
                _context.Compilation,
                TryGetLocalMarkupStringInitializer,
                TryGetPropertyMarkupStringInitializer,
                TryGetFieldMarkupStringInitializer,
                TryGetStaticMarkupFactoryReturnedValue,
                IsSupportedStaticMarkupFactoryInvocation);

        private RazorVueStaticMarkupValueHelper.StaticMarkupResolution? TryResolveStaticMarkup(IOperation? operation)
            => RazorVueStaticMarkupValueHelper.TryResolveStaticMarkup(
                operation,
                _context.Compilation,
                TryGetLocalMarkupStringInitializer,
                TryGetPropertyMarkupStringInitializer,
                TryGetFieldMarkupStringInitializer,
                TryGetStaticMarkupFactoryReturnedValue,
                IsSupportedStaticMarkupFactoryInvocation);

        private IOperation? TryGetLocalMarkupStringInitializer(ILocalSymbol local)
        {
            if (_localStaticMarkupCarriers.TryGetValue(local, out var initializer))
                return initializer;

            return TryGetSourceStableLocalMarkupStringInitializer(local);
        }

        private IOperation? TryGetSourceStableLocalMarkupStringInitializer(ILocalSymbol local)
            => RazorVueSourceStableLocalInitializerHelper.TryGetSourceStableLocalInitializer(
                _context.Compilation,
                local,
                RazorVueStaticMarkupValueHelper.IsMarkupStringType,
                out var initializer)
                ? initializer
                : null;

        private bool IsSourceStableLocalMarkupStringInitializerInvalidatedByLaterWrites(ILocalSymbol local)
            => RazorVueSourceStableLocalInitializerHelper.IsSourceStableLocalInitializerInvalidatedByLaterWrites(
                _context.Compilation,
                local,
                RazorVueStaticMarkupValueHelper.IsMarkupStringType);

        private IOperation? TryGetPropertyMarkupStringInitializer(IPropertySymbol property)
        {
            foreach (var reference in property.DeclaringSyntaxReferences)
            {
                if (reference.GetSyntax() is not PropertyDeclarationSyntax declaration)
                    continue;

                var semanticModel = _context.Compilation.GetSemanticModel(declaration.SyntaxTree);
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

                var semanticModel = _context.Compilation.GetSemanticModel(declarator.SyntaxTree);
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

            var parseOptions = _context.Compilation.SyntaxTrees.FirstOrDefault()?.Options as CSharpParseOptions
                               ?? CSharpParseOptions.Default;
            var source = "file static class __RazorVueLiteralHolder { internal static object Value => "
                         + SymbolDisplay.FormatLiteral(value, quote: true)
                         + "; }";
            var syntaxTree = CSharpSyntaxTree.ParseText(source, parseOptions);
            var compilation = CSharpCompilation.Create(
                "__RazorVueLiteralHolder",
                [syntaxTree],
                _context.Compilation.References,
                new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
            var literal = syntaxTree.GetRoot()
                .DescendantNodes()
                .OfType<LiteralExpressionSyntax>()
                .Single();
            var operation = compilation.GetSemanticModel(syntaxTree).GetOperation(literal)
                            ?? throw new InvalidOperationException("Could not materialize a Roslyn literal operation for static Razor markup.");

            _literalStringOperationCache[value] = operation;
            return operation;
        }

        private static string NormalizeLiteralAttributeText(string text)
        {
            if (text.Length >= 2 &&
                ((text[0] == '"' && text[text.Length - 1] == '"') ||
                 (text[0] == '\'' && text[text.Length - 1] == '\'')))
            {
                return text.Substring(1, text.Length - 2);
            }

            return text;
        }

        private enum PendingTemplateControlKind
        {
            Conditional,
            ForEach,
            For,
            Imperative
        }

        private sealed record PendingTemplateControlNode(
            PendingTemplateControlKind Kind,
            RazorVueRazorSourceSpan SourceSpan,
            IConditionalOperation? ConditionalOperation,
            IForEachLoopOperation? ForEachOperation,
            IForLoopOperation? ForOperation,
            ImmutableArray<IOperation> ImperativeOperations,
            RazorVueImperativeBlockKind? ImperativeKind)
        {
            public static PendingTemplateControlNode CreateConditional(
                RazorVueRazorSourceSpan sourceSpan,
                IConditionalOperation operation)
                => new(PendingTemplateControlKind.Conditional, sourceSpan, operation, null, null, [], null);

            public static PendingTemplateControlNode CreateForEach(
                RazorVueRazorSourceSpan sourceSpan,
                IForEachLoopOperation operation)
                => new(PendingTemplateControlKind.ForEach, sourceSpan, null, operation, null, [], null);

            public static PendingTemplateControlNode CreateFor(
                RazorVueRazorSourceSpan sourceSpan,
                IForLoopOperation operation)
                => new(PendingTemplateControlKind.For, sourceSpan, null, null, operation, [], null);

            public static PendingTemplateControlNode CreateImperative(
                RazorVueRazorSourceSpan sourceSpan,
                ImmutableArray<IOperation> operations,
                RazorVueImperativeBlockKind kind)
                => new(PendingTemplateControlKind.Imperative, sourceSpan, null, null, null, operations, kind);
        }
    }
}
