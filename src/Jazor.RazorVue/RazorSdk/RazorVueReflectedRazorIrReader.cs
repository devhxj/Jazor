using System.Collections;
using System.Collections.Immutable;
using System.Reflection;

namespace Jazor.RazorVue.RazorSdk;

internal static class RazorVueReflectedRazorIrReader
{
    private const BindingFlags InstanceFlags =
        BindingFlags.Instance |
        BindingFlags.Public |
        BindingFlags.NonPublic;

    public static bool TryCreateDocument(
        string hintName,
        object codeDocument,
        object csharpDocument,
        out RazorVueRazorSourceGeneratorDocument document,
        out string? failure)
    {
        document = default!;
        failure = null;

        if (codeDocument is null)
        {
            failure = "RazorCodeDocument was null.";
            return false;
        }

        if (csharpDocument is null)
        {
            failure = "RazorCSharpDocument was null.";
            return false;
        }

        if (!HasFullName(codeDocument, "Microsoft.AspNetCore.Razor.Language.RazorCodeDocument"))
        {
            failure = "HostOutput Item2 was not Microsoft.AspNetCore.Razor.Language.RazorCodeDocument.";
            return false;
        }

        if (!HasFullName(csharpDocument, "Microsoft.AspNetCore.Razor.Language.RazorCSharpDocument"))
        {
            failure = "HostOutput Item3 was not Microsoft.AspNetCore.Razor.Language.RazorCSharpDocument.";
            return false;
        }

        if (!TryGetSourceDocument(codeDocument, "Source", hintName, out var primaryDocument, out failure))
            return false;

        var imports = ReadImportDocuments(codeDocument);
        if (!TryReadCSharpText(csharpDocument, out var csharpText, out failure))
            return false;

        var sourceMappings = ReadSourceMappings(csharpDocument);
        if (!TryGetDocumentNode(codeDocument, out var rawDocumentNode, out failure))
            return false;

        document = new RazorVueRazorSourceGeneratorDocument(
            hintName,
            primaryDocument,
            imports,
            csharpText,
            sourceMappings,
            ConvertNode(rawDocumentNode));
        return true;
    }

    private static bool TryGetDocumentNode(
        object codeDocument,
        out object documentNode,
        out string? failure)
    {
        documentNode = default!;
        failure = null;

        var method = codeDocument.GetType().GetMethod("GetDocumentNode", InstanceFlags);
        if (method is null)
        {
            failure = "RazorCodeDocument.GetDocumentNode() was not found.";
            return false;
        }

        var value = method.Invoke(codeDocument, null);
        if (value is null ||
            !HasFullName(value, "Microsoft.AspNetCore.Razor.Language.Intermediate.DocumentIntermediateNode"))
        {
            failure = "RazorCodeDocument.GetDocumentNode() returned null or a non-document IR node.";
            return false;
        }

        documentNode = value;
        return true;
    }

    private static bool TryGetSourceDocument(
        object value,
        string propertyName,
        string fallbackPath,
        out Jazor.RazorVue.RazorVueRazorDocument document,
        out string? failure)
    {
        document = default!;
        failure = null;

        var source = GetPropertyValue(value, propertyName);
        if (source is null)
        {
            failure = "RazorCodeDocument." + propertyName + " was not available.";
            return false;
        }

        var path = ReadSourcePath(source);
        if (string.IsNullOrWhiteSpace(path))
            path = fallbackPath;

        var text = ReadSourceText(source);
        if (text is null)
        {
            failure = "Razor source document text was not readable.";
            return false;
        }

        document = new Jazor.RazorVue.RazorVueRazorDocument(
            path!,
            Microsoft.CodeAnalysis.Text.SourceText.From(text));
        return true;
    }

    private static ImmutableArray<Jazor.RazorVue.RazorVueRazorDocument> ReadImportDocuments(object codeDocument)
    {
        if (GetPropertyValue(codeDocument, "Imports") is not IEnumerable imports)
            return ImmutableArray<Jazor.RazorVue.RazorVueRazorDocument>.Empty;

        var builder = ImmutableArray.CreateBuilder<Jazor.RazorVue.RazorVueRazorDocument>();
        foreach (var import in imports)
        {
            if (import is null)
                continue;

            var path = ReadSourcePath(import);
            var text = ReadSourceText(import);
            if (string.IsNullOrWhiteSpace(path) || text is null)
                continue;

            builder.Add(new Jazor.RazorVue.RazorVueRazorDocument(
                path!,
                Microsoft.CodeAnalysis.Text.SourceText.From(text)));
        }

        return builder.ToImmutable();
    }

    private static bool TryReadCSharpText(
        object csharpDocument,
        out Microsoft.CodeAnalysis.Text.SourceText text,
        out string? failure)
    {
        text = default!;
        failure = null;

        var textValue = GetPropertyValue(csharpDocument, "Text");
        if (textValue is null)
        {
            failure = "RazorCSharpDocument.Text was not available.";
            return false;
        }

        text = Microsoft.CodeAnalysis.Text.SourceText.From(textValue.ToString() ?? string.Empty);
        return true;
    }

    private static ImmutableArray<RazorVueRazorSourceMapping> ReadSourceMappings(object csharpDocument)
    {
        foreach (var propertyName in new[]
                 {
                     "SourceMappingsSortedByOriginal",
                     "SourceMappingsSortedByGenerated",
                     "SourceMappings"
                 })
        {
            if (GetPropertyValue(csharpDocument, propertyName) is not IEnumerable mappings)
                continue;

            var builder = ImmutableArray.CreateBuilder<RazorVueRazorSourceMapping>();
            foreach (var mapping in mappings)
            {
                if (mapping is not null && TryReadSourceMapping(mapping, out var converted))
                    builder.Add(converted);
            }

            return builder.ToImmutable();
        }

        return ImmutableArray<RazorVueRazorSourceMapping>.Empty;
    }

    private static bool TryReadSourceMapping(
        object mapping,
        out RazorVueRazorSourceMapping converted)
    {
        converted = default;
        var originalSpan = GetPropertyValue(mapping, "OriginalSpan");
        var generatedSpan = GetPropertyValue(mapping, "GeneratedSpan");
        if (originalSpan is null || generatedSpan is null)
            return false;

        if (!TryReadSourceSpan(originalSpan, out var original) ||
            !TryReadSourceSpan(generatedSpan, out var generated))
        {
            return false;
        }

        converted = new RazorVueRazorSourceMapping(original, generated);
        return true;
    }

    private static RazorVueRazorIrNode ConvertNode(object node)
    {
        var kind = GetNodeKind(node);
        var typeName = node.GetType().FullName ?? node.GetType().Name;
        var children = ReadNodeCollection(node, "Children")
            .Select(ConvertNode)
            .ToImmutableArray();
        var tokens = children
            .Where(static child => child.Kind == RazorVueRazorIrNodeKind.IntermediateToken)
            .Select(static child => new RazorVueRazorIrToken(child.Content ?? string.Empty, child.Source))
            .ToImmutableArray();

        return new RazorVueRazorIrNode(
            kind,
            typeName,
            children,
            tokens,
            ReadSourceSpanProperty(node, "Source"),
            TagName: ReadString(node, "TagName"),
            TypeName: ReadString(node, "TypeName"),
            MethodName: ReadString(node, "MethodName") ?? ReadString(node, "Name"),
            AttributeName: ReadString(node, "AttributeName"),
            ParameterName: ReadString(node, "ParameterName"),
            IsParameterized: ReadBool(node, "IsParameterized"),
            IsDesignTimePropertyAccessHelper: ReadBool(node, "IsDesignTimePropertyAccessHelper"),
            IsSynthesized: ReadBool(node, "IsSynthesized"),
            HasAttributeNameExpression: GetPropertyValue(node, "AttributeNameExpression") is not null,
            Content: ReadString(node, "Content"),
            Prefix: ReadString(node, "Prefix"),
            Suffix: ReadString(node, "Suffix"),
            StartTagSpan: ReadSourceSpanProperty(node, "StartTagSpan"),
            Attributes: ReadNodeCollection(node, "Attributes").Select(ConvertNode).ToImmutableArray(),
            Body: ReadNodeCollection(node, "Body").Select(ConvertNode).ToImmutableArray(),
            Splats: ReadNodeCollection(node, "Splats").Select(ConvertNode).ToImmutableArray(),
            ChildContents: ReadNodeCollection(node, "ChildContents").Select(ConvertNode).ToImmutableArray(),
            Captures: ReadNodeCollection(node, "Captures").Select(ConvertNode).ToImmutableArray(),
            SetKeys: ReadNodeCollection(node, "SetKeys").Select(ConvertNode).ToImmutableArray());
    }

    private static RazorVueRazorIrNodeKind GetNodeKind(object node)
    {
        var fullName = node.GetType().FullName ?? node.GetType().Name;
        var name = node.GetType().Name;
        return name switch
        {
            "DocumentIntermediateNode" => RazorVueRazorIrNodeKind.Document,
            "NamespaceDeclarationIntermediateNode" => RazorVueRazorIrNodeKind.NamespaceDeclaration,
            "ClassDeclarationIntermediateNode" => RazorVueRazorIrNodeKind.ClassDeclaration,
            "MethodDeclarationIntermediateNode" => RazorVueRazorIrNodeKind.MethodDeclaration,
            "MarkupElementIntermediateNode" => RazorVueRazorIrNodeKind.MarkupElement,
            "ComponentIntermediateNode" => RazorVueRazorIrNodeKind.Component,
            "HtmlContentIntermediateNode" => RazorVueRazorIrNodeKind.HtmlContent,
            "CSharpExpressionIntermediateNode" => RazorVueRazorIrNodeKind.CSharpExpression,
            "MarkupBlockIntermediateNode" => RazorVueRazorIrNodeKind.MarkupBlock,
            "TagHelperBodyIntermediateNode" => RazorVueRazorIrNodeKind.TagHelperBody,
            "CSharpCodeIntermediateNode" => RazorVueRazorIrNodeKind.CSharpCode,
            "FieldDeclarationIntermediateNode" => RazorVueRazorIrNodeKind.FieldDeclaration,
            "PropertyDeclarationIntermediateNode" => RazorVueRazorIrNodeKind.PropertyDeclaration,
            "UsingDirectiveIntermediateNode" => RazorVueRazorIrNodeKind.UsingDirective,
            "DirectiveIntermediateNode" => RazorVueRazorIrNodeKind.Directive,
            "MalformedDirectiveIntermediateNode" => RazorVueRazorIrNodeKind.MalformedDirective,
            "ExtensionIntermediateNode" => RazorVueRazorIrNodeKind.Extension,
            "TagHelperIntermediateNode" => RazorVueRazorIrNodeKind.TagHelper,
            "HtmlAttributeIntermediateNode" => RazorVueRazorIrNodeKind.HtmlAttribute,
            "ComponentAttributeIntermediateNode" => RazorVueRazorIrNodeKind.ComponentAttribute,
            "SplatIntermediateNode" => RazorVueRazorIrNodeKind.Splat,
            "CSharpExpressionAttributeValueIntermediateNode" => RazorVueRazorIrNodeKind.CSharpExpressionAttributeValue,
            "CSharpCodeAttributeValueIntermediateNode" => RazorVueRazorIrNodeKind.CSharpCodeAttributeValue,
            "HtmlAttributeValueIntermediateNode" => RazorVueRazorIrNodeKind.HtmlAttributeValue,
            _ when IsIntermediateTokenType(node.GetType()) => RazorVueRazorIrNodeKind.IntermediateToken,
            _ => RazorVueRazorIrNodeKind.Unknown
        };
    }

    private static bool IsIntermediateTokenType(Type type)
    {
        for (var current = type; current is not null; current = current.BaseType)
        {
            if (string.Equals(
                    current.FullName,
                    "Microsoft.AspNetCore.Razor.Language.Intermediate.IntermediateToken",
                    StringComparison.Ordinal))
            {
                return true;
            }
        }

        return false;
    }

    private static ImmutableArray<object> ReadNodeCollection(object node, string propertyName)
    {
        if (GetPropertyValue(node, propertyName) is not IEnumerable values)
            return ImmutableArray<object>.Empty;

        return values
            .Cast<object?>()
            .Where(static value => value is not null)
            .Cast<object>()
            .ToImmutableArray();
    }

    private static RazorVueRazorSourceSpan? ReadSourceSpanProperty(object node, string propertyName)
    {
        var value = GetPropertyValue(node, propertyName);
        return value is not null && TryReadSourceSpan(value, out var span)
            ? span
            : null;
    }

    private static bool TryReadSourceSpan(
        object span,
        out RazorVueRazorSourceSpan converted)
    {
        converted = default;
        var filePath = ReadString(span, "FilePath");

        if (!TryReadInt(span, "AbsoluteIndex", out var absoluteIndex) ||
            !TryReadInt(span, "Length", out var length))
        {
            return false;
        }

        TryReadInt(span, "LineIndex", out var lineIndex);
        TryReadInt(span, "CharacterIndex", out var characterIndex);
        converted = new RazorVueRazorSourceSpan(
            filePath,
            absoluteIndex,
            length,
            lineIndex,
            characterIndex);
        return true;
    }

    private static string? ReadSourcePath(object sourceDocument)
        => ReadString(sourceDocument, "FilePath")
           ?? ReadString(sourceDocument, "RelativePath");

    private static string? ReadSourceText(object sourceDocument)
    {
        var textValue = GetPropertyValue(sourceDocument, "Text");
        if (textValue is not null)
            return textValue.ToString() ?? string.Empty;

        if (!TryReadInt(sourceDocument, "Length", out var length) || length <= 0)
            return string.Empty;

        var copyTo = sourceDocument.GetType().GetMethod(
            "CopyTo",
            InstanceFlags,
            binder: null,
            types: [typeof(int), typeof(char[]), typeof(int), typeof(int)],
            modifiers: null);
        if (copyTo is null)
            return null;

        var buffer = new char[length];
        copyTo.Invoke(sourceDocument, [0, buffer, 0, length]);
        return new string(buffer);
    }

    private static bool HasFullName(object? value, string fullName)
        => string.Equals(value?.GetType().FullName, fullName, StringComparison.Ordinal);

    private static string? ReadString(object value, string propertyName)
        => GetPropertyValue(value, propertyName) as string;

    private static bool ReadBool(object value, string propertyName)
        => GetPropertyValue(value, propertyName) is bool boolValue && boolValue;

    private static bool TryReadInt(object value, string propertyName, out int result)
    {
        result = 0;
        if (GetPropertyValue(value, propertyName) is int intValue)
        {
            result = intValue;
            return true;
        }

        return false;
    }

    private static object? GetPropertyValue(object value, string name)
        => value.GetType()
            .GetProperty(name, InstanceFlags)
            ?.GetValue(value);
}
