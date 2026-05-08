using System.Collections;
using System.Collections.Immutable;

namespace Jazor.Analyzer.RazorVue.Generation;

internal static class RazorSourceGeneratorBridge
{
    public static bool TryReadCarrier(
        object codeDocument,
        out BridgedCarrier? carrier,
        out string? failure)
    {
        if (codeDocument is null)
            throw new ArgumentNullException(nameof(codeDocument));

        carrier = null;
        failure = null;

        var source = GetPropertyValue(codeDocument, "Source");
        if (source is null)
        {
            failure = "RazorCodeDocument.Source was not available.";
            return false;
        }

        var documentPath = ReadSourcePath(source);
        if (string.IsNullOrWhiteSpace(documentPath))
        {
            failure = "RazorCodeDocument.Source.FilePath was not available.";
            return false;
        }

        var documentText = ReadSourceText(source);
        var imports = ImmutableArray<BridgedImport>.Empty;
        if (GetPropertyValue(codeDocument, "Imports") is IEnumerable importEntries)
        {
            var builder = ImmutableArray.CreateBuilder<BridgedImport>();
            foreach (var import in importEntries)
            {
                if (import is null)
                    continue;

                var importPath = ReadSourcePath(import);
                if (string.IsNullOrWhiteSpace(importPath))
                    continue;

                builder.Add(new BridgedImport(importPath!, ReadSourceText(import)));
            }

            imports = builder.ToImmutable();
        }

        carrier = new BridgedCarrier(documentPath!, documentText, imports);
        return true;
    }

    private static string? ReadSourcePath(object sourceDocument)
        => GetPropertyValue(sourceDocument, "FilePath") as string
           ?? GetPropertyValue(sourceDocument, "RelativePath") as string;

    private static string ReadSourceText(object sourceDocument)
    {
        var textValue = GetPropertyValue(sourceDocument, "Text");
        if (textValue is not null)
            return textValue.ToString() ?? string.Empty;

        var lengthValue = GetPropertyValue(sourceDocument, "Length");
        if (lengthValue is not int length || length <= 0)
            return string.Empty;

        var buffer = new char[length];
        var copyTo = sourceDocument.GetType().GetMethod(
            "CopyTo",
            System.Reflection.BindingFlags.Instance |
            System.Reflection.BindingFlags.Public |
            System.Reflection.BindingFlags.NonPublic,
            binder: null,
            types: [typeof(int), typeof(char[]), typeof(int), typeof(int)],
            modifiers: null);
        copyTo?.Invoke(sourceDocument, [0, buffer, 0, length]);
        return new string(buffer);
    }

    private static object? GetPropertyValue(object value, string name)
        => value.GetType()
            .GetProperty(
                name,
                System.Reflection.BindingFlags.Instance |
                System.Reflection.BindingFlags.Public |
                System.Reflection.BindingFlags.NonPublic)
            ?.GetValue(value);

    internal sealed record BridgedCarrier(
        string DocumentPath,
        string DocumentText,
        ImmutableArray<BridgedImport> Imports);

    internal sealed record BridgedImport(
        string Path,
        string Text);
}
