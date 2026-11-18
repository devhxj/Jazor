using Jolt.Syntax;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Classification;
using Microsoft.CodeAnalysis.Text;

namespace Jolt.LanguageServer;

internal enum TokenKind
{
    Class, Struct, Interface, Enum, Delegate, TypeParameter, Namespace,
    Method, Property, Field, Event, Parameter, Local, Constant, EnumMember,
}

internal static class SemanticTokens
{
    // VS applies the same theme classifications as its C# editor. Other LSP clients use standard kinds.
    internal static string[] Legend(bool visualStudio) => visualStudio
        ? ["class name", "struct name", "interface name", "enum name", "delegate name", "type parameter name",
            "namespace name", "method name", "property name", "field name", "event name", "parameter name",
            "local name", "constant name", "enum member name"]
        : ["class", "struct", "interface", "enum", "type", "typeParameter", "namespace", "method",
            "property", "variable", "event", "parameter", "enumMember"];

    internal static int Index(TokenKind kind, bool visualStudio)
    {
        if (visualStudio)
            return (int)kind;
        return kind switch
        {
            TokenKind.Local or TokenKind.Constant => (int)TokenKind.Field,
            TokenKind.EnumMember => (int)TokenKind.Local,
            _ => (int)kind,
        };
    }

    internal static TokenKind? Kind(string classification) => classification switch
    {
        ClassificationTypeNames.ClassName or ClassificationTypeNames.RecordClassName => TokenKind.Class,
        ClassificationTypeNames.StructName or ClassificationTypeNames.RecordStructName => TokenKind.Struct,
        ClassificationTypeNames.InterfaceName => TokenKind.Interface,
        ClassificationTypeNames.EnumName => TokenKind.Enum,
        ClassificationTypeNames.DelegateName => TokenKind.Delegate,
        ClassificationTypeNames.TypeParameterName => TokenKind.TypeParameter,
        ClassificationTypeNames.NamespaceName => TokenKind.Namespace,
        ClassificationTypeNames.MethodName or ClassificationTypeNames.ExtensionMethodName => TokenKind.Method,
        ClassificationTypeNames.PropertyName => TokenKind.Property,
        ClassificationTypeNames.FieldName => TokenKind.Field,
        ClassificationTypeNames.EventName => TokenKind.Event,
        ClassificationTypeNames.ParameterName => TokenKind.Parameter,
        ClassificationTypeNames.LocalName => TokenKind.Local,
        ClassificationTypeNames.ConstantName => TokenKind.Constant,
        ClassificationTypeNames.EnumMemberName => TokenKind.EnumMember,
        _ => null,
    };
}

internal sealed partial class LanguageFeatures
{
    internal async Task<object> ClassifyAsync(TextSpan range, bool visualStudio, CancellationToken cancellationToken)
    {
        var tokens = new List<(TextSpan Span, TokenKind Kind)>();
        using var classifierWorkspace = new AdhocWorkspace(Host.Value);
        var document = await CreateRoslynDocumentAsync(classifierWorkspace, cancellationToken);
        foreach (var mapping in projection.Spans)
        {
            if (mapping.Source.Intersection(range) is not { Length: > 0 } requested)
                continue;
            var generated = new TextSpan(mapping.Generated.Start + requested.Start - mapping.Source.Start, requested.Length);
            foreach (var classified in await Classifier.GetClassifiedSpansAsync(document, generated, cancellationToken))
            {
                // A recovered C# token can cross a projection seam. Only classify complete author tokens.
                if (SemanticTokens.Kind(classified.ClassificationType) is not { } kind ||
                    !mapping.Generated.Contains(classified.TextSpan))
                    continue;
                var span = new TextSpan(mapping.Source.Start + classified.TextSpan.Start - mapping.Generated.Start, classified.TextSpan.Length);
                Add(span, kind);
            }
        }

        foreach (var element in Elements(projection.Document.Nodes))
        {
            var local = LocalComponent(element.Name);
            var component = Component(element.Name);
            if (local is null && component is null)
                continue;

            Add(element.NameSpan, TokenKind.Class);
            var closingTag = "</" + element.Name;
            var elementText = projection.Document.Source.AsSpan(element.Span.Start, element.Span.Length).TrimEnd();
            if (elementText.EndsWith(">", StringComparison.Ordinal) && elementText.LastIndexOf('<') is var closingStart &&
                closingStart >= 0 && elementText[closingStart..].StartsWith(closingTag, StringComparison.Ordinal) &&
                elementText[(closingStart + closingTag.Length)..^1].Trim().IsEmpty)
                Add(new TextSpan(element.Span.Start + closingStart + 2, element.Name.Length), TokenKind.Class);

            foreach (var attribute in element.Attributes)
            {
                if (component is not null && component.Events.Keys.Any(name => MatchesEvent(attribute.Name, name)))
                    Add(attribute.NameSpan, TokenKind.Event);
                else if (local?.Properties.Any(property => property.Name.Equals(attribute.Name, StringComparison.OrdinalIgnoreCase)) == true ||
                    component?.Props.Keys.Any(name => MatchesProp(attribute.Name, name)) == true)
                    Add(attribute.NameSpan, TokenKind.Property);
            }
        }

        var data = new List<int>(tokens.Count * 5);
        var previousLine = 0;
        var previousColumn = 0;
        var previousEnd = 0;
        foreach (var (span, kind) in tokens.OrderBy(token => token.Span.Start))
        {
            var position = _text.Lines.GetLinePositionSpan(span);
            if (span.Start < previousEnd || position.Start.Line != position.End.Line)
                continue;
            var line = position.Start.Line;
            var column = position.Start.Character;
            data.AddRange([line - previousLine, line == previousLine ? column - previousColumn : column, span.Length,
                SemanticTokens.Index(kind, visualStudio), 0]);
            previousEnd = span.End;
            previousLine = line;
            previousColumn = column;
        }
        return new { data = data.ToArray() };

        void Add(TextSpan span, TokenKind kind)
        {
            if (span.Length != 0 && range.Contains(span))
                tokens.Add((span, kind));
        }
    }
}
