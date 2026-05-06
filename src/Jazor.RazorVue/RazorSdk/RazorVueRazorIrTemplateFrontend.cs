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
using Microsoft.AspNetCore.Razor.Language;
using Microsoft.AspNetCore.Razor.Language.Intermediate;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Operations;

namespace Jazor.RazorVue.RazorSdk;

internal sealed class RazorVueRazorIrTemplateFrontend : IRazorVueTemplateFrontend
{
    private readonly RazorVueRazorCodeDocumentProvider _provider = new();

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
        if (!_provider.TryCreate(context, snapshot, out var handle))
            return false;

        renderTree = CreateRenderTreeCore(context, snapshot, handle);
        return true;
    }

    internal static RazorVueSourceOrigin? CreateSourceOrigin(SourceSpan? sourceSpan, RazorVueOriginKind originKind)
    {
        if (sourceSpan is null || string.IsNullOrWhiteSpace(sourceSpan.Value.FilePath))
            return null;

        return new RazorVueSourceOrigin(
            OriginKind: originKind,
            SourceFilePath: sourceSpan.Value.FilePath,
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

    private static RazorVueRenderFragment CreateRenderTreeCore(
        Jazor.RazorVue.RazorVueCompilationContext context,
        RazorVueSemanticSnapshot snapshot,
        RazorVueRazorCodeDocumentHandle handle)
    {
        var resolver = new RazorVueRazorIrOperationResolver(context, snapshot, handle);
        var converter = new Converter(context, snapshot, resolver);
        return converter.Convert(handle.DocumentNode);
    }

    private sealed class Converter(
        Jazor.RazorVue.RazorVueCompilationContext context,
        RazorVueSemanticSnapshot snapshot,
        RazorVueRazorIrOperationResolver resolver)
    {
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

        public RazorVueRenderFragment Convert(DocumentIntermediateNode document)
        {
            if (document is null)
                throw new ArgumentNullException(nameof(document));

            return ConvertLooseNodes(document.Children, insideTemplate: false);
        }

        private RazorVueRenderFragment ConvertLooseNodes(IEnumerable<IntermediateNode> nodes, bool insideTemplate)
        {
            var builder = ImmutableArray.CreateBuilder<RazorVueRenderNode>();

            foreach (var node in nodes)
            {
                switch (node)
                {
                    case MarkupElementIntermediateNode element:
                        builder.Add(ConvertElement(element));
                        break;
                    case ComponentIntermediateNode component:
                        builder.Add(ConvertComponent(component));
                        break;
                    case HtmlContentIntermediateNode html:
                        AppendHtmlContent(builder, html);
                        break;
                    case CSharpExpressionIntermediateNode expression:
                        builder.Add(ConvertExpressionOrSlotOutlet(expression));
                        break;
                    case CSharpCodeIntermediateNode code when insideTemplate:
                        throw CreateUnsupportedNodeException(code, "CSharpCodeIntermediateNode");
                    case DocumentIntermediateNode document:
                        builder.AddRange(ConvertLooseNodes(document.Children, insideTemplate).Children);
                        break;
                    case NamespaceDeclarationIntermediateNode namespaceDeclaration:
                        builder.AddRange(ConvertLooseNodes(namespaceDeclaration.Children, insideTemplate).Children);
                        break;
                    case ClassDeclarationIntermediateNode classDeclaration:
                        builder.AddRange(ConvertLooseNodes(classDeclaration.Children, insideTemplate).Children);
                        break;
                    case MethodDeclarationIntermediateNode methodDeclaration:
                        builder.AddRange(ConvertMethodDeclaration(methodDeclaration).Children);
                        break;
                    case MarkupBlockIntermediateNode markupBlock:
                        AppendMarkupBlock(builder, markupBlock);
                        break;
                    case TagHelperBodyIntermediateNode tagHelperBody:
                        builder.AddRange(ConvertLooseNodes(tagHelperBody.Children, insideTemplate: true).Children);
                        break;
                    case CSharpCodeIntermediateNode:
                    case FieldDeclarationIntermediateNode:
                    case PropertyDeclarationIntermediateNode:
                    case UsingDirectiveIntermediateNode:
                    case DirectiveIntermediateNode:
                    case MalformedDirectiveIntermediateNode:
                    case ExtensionIntermediateNode:
                        break;
                    case TagHelperIntermediateNode tagHelper:
                        throw CreateUnsupportedNodeException(tagHelper, "TagHelperIntermediateNode");
                    default:
                        if (node.Children.Count > 0)
                        {
                            builder.AddRange(ConvertLooseNodes(node.Children, insideTemplate).Children);
                            break;
                        }

                        break;
                }
            }

            return new RazorVueRenderFragment(builder.ToImmutable());
        }

        private RazorVueRenderFragment ConvertMethodDeclaration(MethodDeclarationIntermediateNode node)
        {
            if (node is null)
                throw new ArgumentNullException(nameof(node));

            if (!string.Equals(GetMethodDeclarationName(node), "BuildRenderTree", StringComparison.Ordinal))
                return ConvertLooseNodes(node.Children, insideTemplate: false);

            return ConvertTemplateMethodBody(node.Children);
        }

        private RazorVueRenderFragment ConvertTemplateMethodBody(IEnumerable<IntermediateNode> nodes)
        {
            var bufferedNodes = nodes.ToList();
            if (bufferedNodes.Count == 0)
                return RazorVueRenderFragment.Empty;

            var builder = ImmutableArray.CreateBuilder<RazorVueRenderNode>();
            var index = 0;
            while (index < bufferedNodes.Count)
            {
                var node = bufferedNodes[index];
                if (TryConvertConditional(bufferedNodes, ref index, out var conditionalNode))
                {
                    builder.Add(conditionalNode);
                    continue;
                }

                if (TryConvertForEach(bufferedNodes, ref index, out var loopNode))
                {
                    builder.Add(loopNode);
                    continue;
                }

                switch (node)
                {
                    case MarkupElementIntermediateNode element:
                        builder.Add(ConvertElement(element));
                        index++;
                        break;
                    case ComponentIntermediateNode component:
                        builder.Add(ConvertComponent(component));
                        index++;
                        break;
                    case HtmlContentIntermediateNode html:
                        AppendHtmlContent(builder, html);
                        index++;
                        break;
                    case CSharpExpressionIntermediateNode expression:
                        builder.Add(ConvertExpressionOrSlotOutlet(expression));
                        index++;
                        break;
                    case MarkupBlockIntermediateNode markupBlock:
                        AppendMarkupBlock(builder, markupBlock);
                        index++;
                        break;
                    case TagHelperBodyIntermediateNode tagHelperBody:
                        builder.AddRange(ConvertLooseNodes(tagHelperBody.Children, insideTemplate: true).Children);
                        index++;
                        break;
                    case CSharpCodeIntermediateNode:
                    case FieldDeclarationIntermediateNode:
                    case PropertyDeclarationIntermediateNode:
                    case UsingDirectiveIntermediateNode:
                    case DirectiveIntermediateNode:
                    case MalformedDirectiveIntermediateNode:
                    case ExtensionIntermediateNode:
                        index++;
                        break;
                    case TagHelperIntermediateNode tagHelper:
                        throw CreateUnsupportedNodeException(tagHelper, "TagHelperIntermediateNode");
                    default:
                        if (node.Children.Count > 0)
                        {
                            builder.AddRange(ConvertLooseNodes(node.Children, insideTemplate: true).Children);
                        }

                        index++;
                        break;
                }
            }

            return new RazorVueRenderFragment(builder.ToImmutable());
        }

        private RazorVueElementNode ConvertElement(MarkupElementIntermediateNode node)
        {
            RejectElementExtensions(node.Captures, "ReferenceCaptureIntermediateNode");
            RejectElementExtensions(node.SetKeys, "SetKeyIntermediateNode");

            var attributes = node.Attributes
                .Select(ConvertHtmlAttribute)
                .ToImmutableArray();
            var children = ConvertLooseNodes(node.Body, insideTemplate: true);

            return new RazorVueElementNode(
                node.TagName,
                attributes,
                children,
                CreateOrigins(node.Source));
        }

        private RazorVueComponentNode ConvertComponent(ComponentIntermediateNode node)
        {
            RejectComponentExtensions(node.Captures, "ReferenceCaptureIntermediateNode");
            RejectComponentExtensions(node.SetKeys, "SetKeyIntermediateNode");
            RejectComponentExtensions(node.Splats, "SplatIntermediateNode");

            var attributes = ImmutableArray.CreateBuilder<RazorVueAttributeNode>();
            foreach (var attribute in node.Attributes)
            {
                if (attribute.IsDesignTimePropertyAccessHelper)
                    continue;
                if (attribute.IsSynthesized && attribute.Source is null)
                    continue;

                attributes.Add(ConvertComponentAttribute(attribute));
            }

            var children = ImmutableArray.CreateBuilder<RazorVueRenderNode>();
            foreach (var childContent in node.ChildContents)
            {
                if (childContent.IsParameterized)
                    throw CreateUnsupportedNodeException(childContent, "parameterized ComponentChildContentIntermediateNode");

                if (!string.Equals(childContent.AttributeName, "ChildContent", StringComparison.Ordinal))
                {
                    throw CreateUnsupportedNodeException(
                        childContent,
                        $"named ComponentChildContentIntermediateNode '{childContent.AttributeName}'");
                }

                children.AddRange(ConvertLooseNodes(childContent.Children, insideTemplate: true).Children);
            }

            return new RazorVueComponentNode(
                GetComponentName(node),
                NormalizeTypeName(node.TypeName),
                string.IsNullOrWhiteSpace(node.TagName) ? GetComponentName(node) : node.TagName,
                attributes.ToImmutable(),
                new RazorVueRenderFragment(children.ToImmutable()),
                CreateOrigins(node.Source is null ? node.StartTagSpan : node.Source));
        }

        private RazorVueExpressionNode ConvertExpression(CSharpExpressionIntermediateNode node)
        {
            var sourceSpan = GetRequiredSourceSpan(node, "CSharpExpressionIntermediateNode");
            var operation = _resolver.ResolveRequiredOperation(sourceSpan, "Razor IR body expression");
            return new RazorVueExpressionNode(operation, CreateOrigins(sourceSpan));
        }

        private void AppendHtmlContent(
            ImmutableArray<RazorVueRenderNode>.Builder builder,
            HtmlContentIntermediateNode node)
        {
            var text = GetNodeText(node);
            if (string.IsNullOrEmpty(text))
                return;

            builder.Add(new RazorVueTextNode(text, CreateOrigins(GetBestSourceSpan(node))));
        }

        private void AppendMarkupBlock(
            ImmutableArray<RazorVueRenderNode>.Builder builder,
            MarkupBlockIntermediateNode node)
        {
            if (node.Children.Count > 0)
            {
                builder.AddRange(ConvertLooseNodes(node.Children, insideTemplate: true).Children);
                return;
            }

            var markup = GetNodeText(node);
            if (string.IsNullOrEmpty(markup))
                markup = GetNodeContent(node);
            if (string.IsNullOrEmpty(markup))
                return;

            builder.AddRange(ParseStaticMarkupFragment(markup, CreateOrigins(GetBestSourceSpan(node))));
        }

        private RazorVueAttributeNode ConvertHtmlAttribute(HtmlAttributeIntermediateNode node)
        {
            if (node.AttributeNameExpression is not null)
                throw CreateUnsupportedNodeException(node, "dynamic HtmlAttributeIntermediateNode.AttributeNameExpression");

            var value = ResolveAttributeValue(node.AttributeName, node.Children, node.Source);
            return new RazorVueAttributeNode(
                node.AttributeName,
                value,
                CreateOrigins(node.Source));
        }

        private RazorVueAttributeNode ConvertComponentAttribute(ComponentAttributeIntermediateNode node)
        {
            var value = ResolveAttributeValue(node.AttributeName, node.Children, node.Source);
            return new RazorVueAttributeNode(
                node.AttributeName,
                value,
                CreateOrigins(node.Source));
        }

        private IOperation? ResolveAttributeValue(
            string attributeName,
            IntermediateNodeCollection children,
            SourceSpan? fallbackSource)
        {
            if (children.Count == 0)
                return fallbackSource is null
                    ? null
                    : _resolver.ResolveRequiredOperation(fallbackSource, $"Razor IR attribute '{attributeName}'");

            if (children.Count != 1)
            {
                throw CreateUnsupportedAttributeException(
                    fallbackSource,
                    $"RazorVue Razor IR frontend does not yet support mixed attribute content for '{attributeName}' in component '{_snapshot.Descriptor.FullName}'.");
            }

            return children[0] switch
            {
                HtmlContentIntermediateNode html => ResolveLiteralAttributeValue(html),
                CSharpExpressionIntermediateNode expressionNode => _resolver.ResolveRequiredOperation(
                    GetRequiredSourceSpan(expressionNode, $"CSharpExpression value for attribute '{attributeName}'"),
                    $"Razor IR expression attribute '{attributeName}'"),
                CSharpExpressionAttributeValueIntermediateNode expression => _resolver.ResolveRequiredOperation(
                    GetRequiredSourceSpan(expression, $"CSharpExpressionAttributeValue for attribute '{attributeName}'"),
                    $"Razor IR expression attribute '{attributeName}'"),
                CSharpCodeAttributeValueIntermediateNode code => throw CreateUnsupportedNodeException(
                    code,
                    $"CSharpCodeAttributeValueIntermediateNode '{attributeName}'"),
                HtmlAttributeValueIntermediateNode htmlValue => ResolveLiteralAttributeValue(htmlValue),
                _ => throw CreateUnsupportedNodeException(children[0], $"{children[0].GetType().Name} '{attributeName}'")
            };
        }

        private IOperation ResolveLiteralAttributeValue(IntermediateNode node)
        {
            var text = GetNodeText(node);
            if (string.IsNullOrEmpty(text))
                text = GetNodeContent(node);

            return CreateLiteralStringOperation(NormalizeLiteralAttributeText(text));
        }

        private void RejectElementExtensions<TNode>(IEnumerable<TNode> nodes, string detail)
            where TNode : IntermediateNode
        {
            var first = nodes.FirstOrDefault();
            if (first is not null)
                throw CreateUnsupportedNodeException(first, detail);
        }

        private void RejectComponentExtensions<TNode>(IEnumerable<TNode> nodes, string detail)
            where TNode : IntermediateNode
        {
            var first = nodes.FirstOrDefault();
            if (first is not null)
                throw CreateUnsupportedNodeException(first, detail);
        }

        private RazorVueCompilationIssueException CreateUnsupportedNodeException(IntermediateNode node, string detail)
            => CreateUnsupportedAttributeException(
                node.Source,
                $"RazorVue Razor IR frontend does not yet support {detail} in component '{_snapshot.Descriptor.FullName}'.");

        private RazorVueCompilationIssueException CreateUnsupportedAttributeException(SourceSpan? sourceSpan, string message)
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
        {
            if (!SymbolEqualityComparer.Default.Equals(symbol.ContainingType, _snapshot.ComponentSymbol))
                return false;

            return instance is null ||
                   Jazor.RazorVue.RazorVueOperationNormalizer.Unwrap(instance) is IInstanceReferenceOperation;
        }

        private RazorVueRenderNode ConvertExpressionOrSlotOutlet(CSharpExpressionIntermediateNode node)
        {
            var expression = ConvertExpression(node);
            if (TryResolveSlotOutlet(expression.Expression, out var slotName))
                return new RazorVueSlotOutletNode(slotName, null, expression.Origins);

            return expression;
        }

        private bool TryConvertConditional(
            IReadOnlyList<IntermediateNode> nodes,
            ref int index,
            out RazorVueConditionalNode conditionalNode)
        {
            conditionalNode = default!;
            if (nodes[index] is not CSharpCodeIntermediateNode codeNode)
                return false;

            var controlText = GetNodeText(codeNode);
            if (!TryExtractControlHeader(controlText, "if", out var conditionText))
                return false;

            var sourceSpan = GetRequiredSourceSpan(codeNode, "CSharpCodeIntermediateNode if header");
            var condition = _resolver.ResolveRequiredOperation(sourceSpan, "Razor IR conditional expression");
            var bodyStart = index + 1;
            var bodyEnd = FindClosingCodeNodeIndex(nodes, bodyStart, conditionText, sourceSpan, isForEach: false);
            if (bodyEnd < bodyStart)
                throw CreateUnsupportedNodeException(codeNode, "unterminated if CSharpCodeIntermediateNode");

            var whenTrue = ConvertTemplateMethodBody(nodes.Skip(bodyStart).Take(bodyEnd - bodyStart));
            var whenFalse = RazorVueRenderFragment.Empty;

            index = bodyEnd + 1;
            conditionalNode = new RazorVueConditionalNode(
                condition,
                whenTrue,
                whenFalse,
                CreateOrigins(sourceSpan));
            return true;
        }

        private bool TryConvertForEach(
            IReadOnlyList<IntermediateNode> nodes,
            ref int index,
            out RazorVueForEachNode loopNode)
        {
            loopNode = default!;
            if (nodes[index] is not CSharpCodeIntermediateNode codeNode)
                return false;

            var controlText = GetNodeText(codeNode);
            if (!TryExtractControlHeader(controlText, "foreach", out _))
                return false;

            var sourceSpan = GetRequiredSourceSpan(codeNode, "CSharpCodeIntermediateNode foreach header");
            var (itemName, sourceOperation) = ResolveForEachHeader(codeNode, sourceSpan);
            var bodyStart = index + 1;
            var bodyEnd = FindClosingCodeNodeIndex(nodes, bodyStart, itemName, sourceSpan, isForEach: true);
            if (bodyEnd < bodyStart)
                throw CreateUnsupportedNodeException(codeNode, "unterminated foreach CSharpCodeIntermediateNode");

            var body = ConvertTemplateMethodBody(nodes.Skip(bodyStart).Take(bodyEnd - bodyStart));
            index = bodyEnd + 1;
            loopNode = new RazorVueForEachNode(
                itemName,
                sourceOperation,
                body,
                CreateOrigins(sourceSpan));
            return true;
        }

        private (string ItemName, IOperation SourceOperation) ResolveForEachHeader(
            CSharpCodeIntermediateNode codeNode,
            SourceSpan sourceSpan)
        {
            var operation = _resolver.ResolveRequiredOperation(sourceSpan, "Razor IR foreach expression");
            var current = Jazor.RazorVue.RazorVueOperationNormalizer.Unwrap(operation);
            if (current is not IForEachLoopOperation foreachLoop)
            {
                throw CreateUnsupportedAttributeException(
                    sourceSpan,
                    $"RazorVue Razor IR frontend expected a foreach loop operation in component '{_snapshot.Descriptor.FullName}'.");
            }

            return (
                foreachLoop.Locals.Length > 0 ? foreachLoop.Locals[0].Name : "item",
                foreachLoop.Collection);
        }

        private int FindClosingCodeNodeIndex(
            IReadOnlyList<IntermediateNode> nodes,
            int startIndex,
            string detail,
            SourceSpan sourceSpan,
            bool isForEach)
        {
            for (var candidateIndex = startIndex; candidateIndex < nodes.Count; candidateIndex++)
            {
                if (nodes[candidateIndex] is not CSharpCodeIntermediateNode closingNode)
                    continue;

                var text = GetNodeText(closingNode).Trim();
                if (!IsClosingControlBlock(text))
                    continue;

                if (candidateIndex != nodes.Count - 1)
                {
                    var trailingSignificantNode = nodes.Skip(candidateIndex + 1).FirstOrDefault(static node =>
                        node is not HtmlContentIntermediateNode html || !string.IsNullOrWhiteSpace(GetNodeText(html)));
                    if (trailingSignificantNode is not null)
                    {
                        throw CreateUnsupportedAttributeException(
                            sourceSpan,
                            $"RazorVue Razor IR frontend does not yet support additional control-flow siblings after {(isForEach ? "foreach" : "if")} '{detail}' in component '{_snapshot.Descriptor.FullName}'.");
                    }
                }

                return candidateIndex;
            }

            return -1;
        }

        private static bool TryExtractControlHeader(string text, string keyword, out string payload)
        {
            payload = string.Empty;
            if (string.IsNullOrWhiteSpace(text))
                return false;

            var trimmed = text.TrimStart();
            if (!trimmed.StartsWith(keyword + " ", StringComparison.Ordinal) &&
                !trimmed.StartsWith(keyword + "(", StringComparison.Ordinal))
            {
                return false;
            }

            payload = trimmed;
            return true;
        }

        private static bool IsClosingControlBlock(string text)
            => string.Equals(text, "}", StringComparison.Ordinal);

        private static string GetComponentName(ComponentIntermediateNode node)
        {
            var normalizedTypeName = NormalizeTypeName(node.TypeName);
            if (!string.IsNullOrWhiteSpace(normalizedTypeName))
            {
                var lastDot = normalizedTypeName.LastIndexOf('.');
                return lastDot >= 0 ? normalizedTypeName.Substring(lastDot + 1) : normalizedTypeName;
            }

            return string.IsNullOrWhiteSpace(node.TagName) ? "Component" : node.TagName;
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

        private static SourceSpan? GetBestSourceSpan(IntermediateNode node)
        {
            foreach (var token in EnumerateTokens(node))
            {
                if (token.Source is not null)
                    return token.Source;
            }

            return node.Source;
        }

        private static SourceSpan GetRequiredSourceSpan(IntermediateNode node, string detail)
            => GetBestSourceSpan(node)
               ?? throw new InvalidOperationException($"The Razor IR node '{detail}' did not expose a source span.");

        private static string GetNodeText(IntermediateNode node)
            => string.Concat(EnumerateTokens(node).Select(static token => token.Content));

        private static string GetNodeContent(IntermediateNode node)
        {
            var property = node.GetType().GetProperty("Content");
            return property?.GetValue(node) as string ?? string.Empty;
        }

        private static string? GetMethodDeclarationName(MethodDeclarationIntermediateNode node)
        {
            var nodeType = node.GetType();
            var property = nodeType.GetProperty("MethodName") ?? nodeType.GetProperty("Name");
            return property?.GetValue(node) as string;
        }

        private static IEnumerable<IntermediateToken> EnumerateTokens(IntermediateNode node)
        {
            foreach (var child in node.Children)
            {
                if (child is IntermediateToken token)
                {
                    yield return token;
                    continue;
                }

                foreach (var nested in EnumerateTokens(child))
                    yield return nested;
            }
        }

        private static ImmutableArray<RazorVueSourceOrigin> CreateOrigins(SourceSpan? sourceSpan)
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

        private ImmutableArray<RazorVueRenderNode> ParseStaticMarkupFragment(
            string markup,
            ImmutableArray<RazorVueSourceOrigin> origins)
        {
            var roots = ImmutableArray.CreateBuilder<RazorVueRenderNode>();
            var openElements = new Stack<StaticElementBuilder>();
            var index = 0;

            while (index < markup.Length)
            {
                if (markup[index] != '<')
                {
                    var textStart = index;
                    while (index < markup.Length && markup[index] != '<')
                        index++;

                    AppendStaticText(markup.Substring(textStart, index - textStart));
                    continue;
                }

                if (StartsWith(markup, index, "<!--"))
                {
                    var commentEnd = markup.IndexOf("-->", index, StringComparison.Ordinal);
                    if (commentEnd < 0)
                        throw CreateUnsupportedAttributeException(null, $"RazorVue Razor IR frontend could not parse static markup block '{markup}' in component '{_snapshot.Descriptor.FullName}'.");

                    index = commentEnd + 3;
                    continue;
                }

                if (index + 1 < markup.Length && markup[index + 1] == '/')
                {
                    index += 2;
                    SkipWhitespace(markup, ref index);
                    var tagName = ReadName(markup, ref index);
                    SkipWhitespace(markup, ref index);
                    Expect(markup, ref index, '>');

                    if (openElements.Count == 0)
                    {
                        throw CreateUnsupportedAttributeException(null, $"RazorVue Razor IR frontend found an unmatched closing tag '</{tagName}>' in component '{_snapshot.Descriptor.FullName}'.");
                    }

                    var element = openElements.Pop();
                    if (!string.Equals(element.TagName, tagName, StringComparison.OrdinalIgnoreCase))
                    {
                        throw CreateUnsupportedAttributeException(null, $"RazorVue Razor IR frontend found a mismatched closing tag '</{tagName}>' for '<{element.TagName}>' in component '{_snapshot.Descriptor.FullName}'.");
                    }

                    AddNode(element.Build());
                    continue;
                }

                index++;
                SkipWhitespace(markup, ref index);
                var startTagName = ReadName(markup, ref index);
                var attributes = ImmutableArray.CreateBuilder<RazorVueAttributeNode>();
                var selfClosing = false;

                while (index < markup.Length)
                {
                    SkipWhitespace(markup, ref index);
                    if (index >= markup.Length)
                        break;

                    if (markup[index] == '>')
                    {
                        index++;
                        break;
                    }

                    if (markup[index] == '/' && index + 1 < markup.Length && markup[index + 1] == '>')
                    {
                        selfClosing = true;
                        index += 2;
                        break;
                    }

                    var attributeName = ReadName(markup, ref index);
                    SkipWhitespace(markup, ref index);

                    IOperation? attributeValue = null;
                    if (index < markup.Length && markup[index] == '=')
                    {
                        index++;
                        SkipWhitespace(markup, ref index);
                        attributeValue = CreateLiteralStringOperation(ReadAttributeValue(markup, ref index));
                    }

                    attributes.Add(new RazorVueAttributeNode(attributeName, attributeValue, origins));
                }

                var builder = new StaticElementBuilder(startTagName, attributes.ToImmutable(), origins);
                if (selfClosing || VoidElementNames.Contains(startTagName))
                {
                    AddNode(builder.Build());
                    continue;
                }

                openElements.Push(builder);
            }

            if (openElements.Count > 0)
            {
                var unclosed = openElements.Peek();
                throw CreateUnsupportedAttributeException(null, $"RazorVue Razor IR frontend found an unclosed static tag '<{unclosed.TagName}>' in component '{_snapshot.Descriptor.FullName}'.");
            }

            return roots.ToImmutable();

            void AppendStaticText(string text)
            {
                if (string.IsNullOrEmpty(text))
                    return;

                AddNode(new RazorVueTextNode(text, origins));
            }

            void AddNode(RazorVueRenderNode node)
            {
                if (openElements.Count == 0)
                {
                    roots.Add(node);
                    return;
                }

                openElements.Peek().Children.Add(node);
            }
        }

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

        private static void SkipWhitespace(string text, ref int index)
        {
            while (index < text.Length && char.IsWhiteSpace(text[index]))
                index++;
        }

        private static string ReadName(string text, ref int index)
        {
            var start = index;
            while (index < text.Length)
            {
                var current = text[index];
                if (char.IsWhiteSpace(current) || current is '=' or '/' or '>')
                    break;

                index++;
            }

            if (index == start)
                throw new InvalidOperationException("Expected a markup name token.");

            return text.Substring(start, index - start);
        }

        private static string ReadAttributeValue(string text, ref int index)
        {
            if (index >= text.Length)
                return string.Empty;

            var quote = text[index];
            if (quote is '"' or '\'')
            {
                index++;
                var start = index;
                while (index < text.Length && text[index] != quote)
                    index++;

                var value = text.Substring(start, index - start);
                if (index < text.Length)
                    index++;

                return value;
            }

            var unquotedStart = index;
            while (index < text.Length)
            {
                var current = text[index];
                if (char.IsWhiteSpace(current) || current is '/' or '>')
                    break;

                index++;
            }

            return text.Substring(unquotedStart, index - unquotedStart);
        }

        private static bool StartsWith(string text, int index, string value)
            => index >= 0 &&
               index + value.Length <= text.Length &&
               string.Compare(text, index, value, 0, value.Length, StringComparison.Ordinal) == 0;

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

        private static void Expect(string text, ref int index, char expected)
        {
            if (index >= text.Length || text[index] != expected)
                throw new InvalidOperationException($"Expected '{expected}' in static markup.");

            index++;
        }

        private sealed class StaticElementBuilder(
            string tagName,
            ImmutableArray<RazorVueAttributeNode> attributes,
            ImmutableArray<RazorVueSourceOrigin> origins)
        {
            public string TagName { get; } = tagName;
            public ImmutableArray<RazorVueAttributeNode> Attributes { get; } = attributes;
            public ImmutableArray<RazorVueSourceOrigin> Origins { get; } = origins;
            public ImmutableArray<RazorVueRenderNode>.Builder Children { get; } = ImmutableArray.CreateBuilder<RazorVueRenderNode>();

            public RazorVueElementNode Build()
                => new(
                    TagName,
                    Attributes,
                    new RazorVueRenderFragment(Children.ToImmutable()),
                    Origins);
        }
    }
}
