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
        private int _elementKeyOrdinal;
        private int _componentKeyOrdinal;

        public RazorVueRenderFragment Convert(RazorVueRazorIrNode document)
        {
            if (document is null)
                throw new ArgumentNullException(nameof(document));

            return ConvertLooseNodes(document.Children, insideTemplate: false);
        }

        private RazorVueRenderFragment ConvertLooseNodes(IEnumerable<RazorVueRazorIrNode> nodes, bool insideTemplate)
        {
            if (insideTemplate)
                return ConvertTemplateMethodBody(nodes);

            var builder = ImmutableArray.CreateBuilder<RazorVueRenderNode>();

            foreach (var node in nodes)
            {
                switch (node.Kind)
                {
                    case RazorVueRazorIrNodeKind.MarkupElement:
                        builder.Add(ConvertElement(node));
                        break;
                    case RazorVueRazorIrNodeKind.Component:
                        builder.Add(ConvertComponent(node));
                        break;
                    case RazorVueRazorIrNodeKind.HtmlContent:
                        AppendHtmlContent(builder, node);
                        break;
                    case RazorVueRazorIrNodeKind.CSharpExpression:
                        builder.Add(ConvertExpressionOrSlotOutlet(node));
                        break;
                    case RazorVueRazorIrNodeKind.Document:
                        builder.AddRange(ConvertLooseNodes(node.Children, insideTemplate).Children);
                        break;
                    case RazorVueRazorIrNodeKind.NamespaceDeclaration:
                        builder.AddRange(ConvertLooseNodes(node.Children, insideTemplate).Children);
                        break;
                    case RazorVueRazorIrNodeKind.ClassDeclaration:
                        builder.AddRange(ConvertLooseNodes(node.Children, insideTemplate).Children);
                        break;
                    case RazorVueRazorIrNodeKind.MethodDeclaration:
                        builder.AddRange(ConvertMethodDeclaration(node).Children);
                        break;
                    case RazorVueRazorIrNodeKind.MarkupBlock:
                        AppendMarkupBlock(builder, node);
                        break;
                    case RazorVueRazorIrNodeKind.TagHelperBody:
                        builder.AddRange(ConvertTemplateMethodBody(node.Children).Children);
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
                        if (node.Children.Length > 0)
                        {
                            builder.AddRange(ConvertLooseNodes(node.Children, insideTemplate).Children);
                            break;
                        }

                        break;
                }
            }

            return new RazorVueRenderFragment(builder.ToImmutable());
        }

        private RazorVueRenderFragment ConvertMethodDeclaration(RazorVueRazorIrNode node)
        {
            if (node is null)
                throw new ArgumentNullException(nameof(node));

            if (!string.Equals(GetMethodDeclarationName(node), "BuildRenderTree", StringComparison.Ordinal))
                return ConvertLooseNodes(node.Children, insideTemplate: false);

            return ConvertTemplateMethodBody(node.Children);
        }

        private RazorVueRenderFragment ConvertTemplateMethodBody(IEnumerable<RazorVueRazorIrNode> nodes)
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

                if (TryConvertFor(bufferedNodes, ref index, out var forNode))
                {
                    builder.Add(forNode);
                    continue;
                }

                switch (node.Kind)
                {
                    case RazorVueRazorIrNodeKind.MarkupElement:
                        builder.Add(ConvertElement(node));
                        index++;
                        break;
                    case RazorVueRazorIrNodeKind.Component:
                        builder.Add(ConvertComponent(node));
                        index++;
                        break;
                    case RazorVueRazorIrNodeKind.HtmlContent:
                        AppendHtmlContent(builder, node);
                        index++;
                        break;
                    case RazorVueRazorIrNodeKind.CSharpExpression:
                        builder.Add(ConvertExpressionOrSlotOutlet(node));
                        index++;
                        break;
                    case RazorVueRazorIrNodeKind.MarkupBlock:
                        AppendMarkupBlock(builder, node);
                        index++;
                        break;
                    case RazorVueRazorIrNodeKind.TagHelperBody:
                        builder.AddRange(ConvertTemplateMethodBody(node.Children).Children);
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
                            builder.AddRange(ConvertTemplateMethodBody(node.Children).Children);
                        }

                        index++;
                        break;
                }
            }

            return new RazorVueRenderFragment(builder.ToImmutable());
        }

        private RazorVueElementNode ConvertElement(RazorVueRazorIrNode node)
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
            var children = ConvertTemplateMethodBody(node.BodyOrEmpty);

            return new RazorVueElementNode(
                node.TagName ?? string.Empty,
                key,
                attributes.ToImmutable(),
                children,
                CreateOrigins(node.Source));
        }

        private RazorVueComponentNode ConvertComponent(RazorVueRazorIrNode node)
        {
            RejectComponentExtensions(node.CapturesOrEmpty, "ReferenceCaptureIntermediateNode");

            var key = ResolveComponentKey(node);
            var attributes = ImmutableArray.CreateBuilder<RazorVueAttributeEntry>();
            foreach (var attribute in node.AttributesOrEmpty)
            {
                if (attribute.IsDesignTimePropertyAccessHelper)
                    continue;
                if (attribute.IsSynthesized && attribute.Source is null)
                    continue;
                if (IsKeyAttribute(attribute))
                    continue;

                attributes.Add(ConvertComponentAttribute(attribute));
            }
            foreach (var splat in node.SplatsOrEmpty)
                attributes.Add(ConvertSplatAttribute(splat));

            var children = ImmutableArray.CreateBuilder<RazorVueRenderNode>();
            var slotTemplates = ImmutableArray.CreateBuilder<RazorVueComponentSlotTemplateNode>();
            foreach (var childContent in node.ChildContentsOrEmpty)
            {
                var slotFragment = ConvertTemplateMethodBody(childContent.Children);
                var attributeName = childContent.AttributeName ?? string.Empty;
                if (string.Equals(attributeName, "ChildContent", StringComparison.Ordinal))
                {
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
                new RazorVueRenderFragment(children.ToImmutable()),
                CreateOrigins(node.Source is null ? node.StartTagSpan : node.Source));
        }

        private RazorVueExpressionNode ConvertExpression(RazorVueRazorIrNode node)
        {
            var sourceSpan = GetRequiredSourceSpan(node, "CSharpExpressionIntermediateNode");
            var operation = _resolver.ResolveRequiredOperation(sourceSpan, "Razor IR body expression");
            return new RazorVueExpressionNode(operation, CreateOrigins(sourceSpan));
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
            RazorVueRazorIrNode node)
        {
            if (node.Children.Length > 0)
            {
                builder.AddRange(ConvertTemplateMethodBody(node.Children).Children);
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
            var value = ResolveAttributeValue(attributeName, node.Children, node.Source);
            return new RazorVueAttributeNode(
                attributeName,
                value,
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
                    CreateOrigins(node.Source));
            }

            return ConvertHtmlAttribute(node);
        }

        private RazorVueAttributeNode ConvertComponentAttribute(RazorVueRazorIrNode node)
        {
            var attributeName = node.AttributeName ?? string.Empty;
            var value = ResolveAttributeValue(attributeName, node.Children, node.Source, node);
            return new RazorVueAttributeNode(
                attributeName,
                value,
                CreateOrigins(node.Source));
        }

        private RazorVueAttributeSpreadNode ConvertSplatAttribute(RazorVueRazorIrNode node)
        {
            var sourceSpan = GetRequiredSourceSpan(node, "SplatIntermediateNode");
            var operation = _resolver.ResolveRequiredOperation(sourceSpan, "Razor IR splat attribute");
            return new RazorVueAttributeSpreadNode(
                operation,
                CreateOrigins(node.Source));
        }

        private IOperation? ResolveAttributeValue(
            string attributeName,
            ImmutableArray<RazorVueRazorIrNode> children,
            RazorVueRazorSourceSpan? fallbackSource,
            RazorVueRazorIrNode? ownerNode = null)
        {
            if (children.Length == 0)
                return fallbackSource is null
                    ? null
                    : _resolver.ResolveRequiredOperation(fallbackSource, $"Razor IR attribute '{attributeName}'");

            if (children.Length != 1)
            {
                if (TryResolveStaticLiteralAttributeValue(children, out var literalValue))
                    return CreateLiteralStringOperation(literalValue);

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
                RazorVueRazorIrNodeKind.CSharpCodeAttributeValue => throw CreateUnsupportedNodeException(
                    children[0],
                    $"CSharpCodeAttributeValueIntermediateNode '{attributeName}'"),
                RazorVueRazorIrNodeKind.HtmlAttributeValue => CreateLiteralStringOperation(
                    NormalizeLiteralAttributeText(ResolveHtmlAttributeValueText(children[0], includePrefix: false))),
                _ => throw CreateUnsupportedNodeException(children[0], $"{children[0].RuntimeTypeName} '{attributeName}'")
            };
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
                    CreateOrigins(GetBestSourceSpan(child) ?? sourceSpan));
            }

            return new RazorVueNodeKey(
                    _resolver.ResolveRequiredOperation(sourceSpan, "Razor IR @key expression"),
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

        private RazorVueRenderNode ConvertExpressionOrSlotOutlet(RazorVueRazorIrNode node)
        {
            var expression = ConvertExpression(node);
            if (TryResolveSlotOutlet(expression.Expression, out var slotName))
                return new RazorVueSlotOutletNode(slotName, null, expression.Origins);

            return expression;
        }

        private bool TryConvertConditional(
            IReadOnlyList<RazorVueRazorIrNode> nodes,
            ref int index,
            out RazorVueConditionalNode conditionalNode)
        {
            conditionalNode = default!;
            if (!IsTemplateCodeNode(nodes[index]))
                return false;

            var codeNode = nodes[index];
            var codeText = GetNodeText(codeNode);
            var normalizedCodeText = NormalizeTemplateCodeText(codeText);
            if (!StartsWithControlKeyword(codeText, "if") &&
                !IsElseIfBoundaryCodeNode(normalizedCodeText))
                return false;

            var sourceSpan = GetRequiredSourceSpan(codeNode, "CSharpCodeIntermediateNode if header");
            if (!_resolver.TryResolveConditional(sourceSpan, out var resolvedConditional))
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
            else if (resolvedConditional.WhenFalseRange is not null)
            {
                (whenTrueNodes, whenFalseNodes) = SplitConditionalBranchesByStructure(coveredNodes, sourceSpan);
            }
            else
            {
                whenTrueNodes = SliceNodesByRange(coveredNodes, resolvedConditional.WhenTrueRange, sourceSpan, "if-true", trimLeadingControlNode: false);
                whenFalseNodes = [];
            }

            var whenTrue = ConvertTemplateMethodBody(whenTrueNodes);
            var whenFalse = ConvertTemplateMethodBody(whenFalseNodes);

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
            out RazorVueForEachNode loopNode)
        {
            loopNode = default!;
            if (!IsTemplateCodeNode(nodes[index]))
                return false;

            var codeNode = nodes[index];
            if (!StartsWithControlKeyword(GetNodeText(codeNode), "foreach"))
                return false;

            var sourceSpan = GetRequiredSourceSpan(codeNode, "CSharpCodeIntermediateNode foreach header");
            if (!_resolver.TryResolveForEach(sourceSpan, out var resolvedLoop))
                return false;

            var bodyEnd = FindControlStatementEndIndex(
                nodes,
                index,
                resolvedLoop.BodyRange,
                sourceSpan,
                "foreach");
            var coveredNodes = nodes.Skip(index + 1).Take(bodyEnd - index - 1).ToList();
            var bodyNodes = SliceNodesByRange(coveredNodes, resolvedLoop.BodyRange, sourceSpan, "foreach-body", trimLeadingControlNode: false);
            var body = ConvertTemplateMethodBody(bodyNodes);
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
            out RazorVueForNode loopNode)
        {
            loopNode = default!;
            if (!IsTemplateCodeNode(nodes[index]))
                return false;

            var codeNode = nodes[index];
            if (!StartsWithControlKeyword(GetNodeText(codeNode), "for"))
                return false;

            var sourceSpan = GetRequiredSourceSpan(codeNode, "CSharpCodeIntermediateNode for header");
            if (!_resolver.TryResolveFor(sourceSpan, out var resolvedLoop))
                return false;

            var analysis = RazorVueForLoopAnalyzer.AnalyzeRequired(
                resolvedLoop.Operation,
                Jazor.RazorVue.RazorVueOperationNormalizer.Unwrap,
                _snapshot.Descriptor.FullName);
            var bodyEnd = FindControlStatementEndIndex(
                nodes,
                index,
                resolvedLoop.BodyRange,
                sourceSpan,
                "for");
            var coveredNodes = nodes.Skip(index + 1).Take(bodyEnd - index - 1).ToList();
            var bodyNodes = SliceNodesByRange(coveredNodes, resolvedLoop.BodyRange, sourceSpan, "for-body", trimLeadingControlNode: false);
            var body = ConvertTemplateMethodBody(bodyNodes);
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

        private int FindControlStatementEndIndex(
            IReadOnlyList<RazorVueRazorIrNode> nodes,
            int startIndex,
            RazorVueRazorIrOperationResolver.SourceRange coveredRange,
            RazorVueRazorSourceSpan sourceSpan,
            string detail)
        {
            var matchingEndIndex = -1;
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
            }

            if (matchingEndIndex < 0)
            {
                throw CreateUnsupportedAttributeException(
                    sourceSpan,
                    $"RazorVue Razor IR frontend could not determine the template extent of {detail} in component '{_snapshot.Descriptor.FullName}'.");
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
                    continue;

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

        private static bool IsElseBoundaryCodeNode(string normalized)
            => string.Equals(normalized, "else", StringComparison.Ordinal) ||
               string.Equals(normalized, "else{", StringComparison.Ordinal) ||
               string.Equals(normalized, "}else{", StringComparison.Ordinal);

        private static bool IsElseIfBoundaryCodeNode(string normalized)
            => normalized.StartsWith("elseif(", StringComparison.Ordinal) ||
               normalized.StartsWith("}elseif(", StringComparison.Ordinal);

        private static bool IsPureClosingCodeNode(string normalized)
            => string.Equals(normalized, "}", StringComparison.Ordinal);

        private static string NormalizeTemplateCodeText(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return string.Empty;

            return new string(text.Where(static character => !char.IsWhiteSpace(character)).ToArray());
        }

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
                var attributes = ImmutableArray.CreateBuilder<RazorVueAttributeEntry>();
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
            ImmutableArray<RazorVueAttributeEntry> attributes,
            ImmutableArray<RazorVueSourceOrigin> origins)
        {
            public string TagName { get; } = tagName;
            public ImmutableArray<RazorVueAttributeEntry> Attributes { get; } = attributes;
            public ImmutableArray<RazorVueSourceOrigin> Origins { get; } = origins;
            public ImmutableArray<RazorVueRenderNode>.Builder Children { get; } = ImmutableArray.CreateBuilder<RazorVueRenderNode>();

            public RazorVueElementNode Build()
                => new(
                    TagName,
                    Key: null,
                    Attributes,
                    new RazorVueRenderFragment(Children.ToImmutable()),
                    Origins);
        }
    }
}
