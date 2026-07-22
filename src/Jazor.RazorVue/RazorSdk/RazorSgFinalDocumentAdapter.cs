using System.Collections;
using System.Collections.Immutable;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace Jazor.RazorVue.RazorSdk;

// SDK Razor types stay inside this adapter because analyzers may load them in a separate context.
internal static class RazorSgFinalDocumentAdapter
{
    private const BindingFlags InstanceFlags =
        BindingFlags.Instance |
        BindingFlags.Public |
        BindingFlags.NonPublic;

    private const string RazorCodeDocumentTypeName = "Microsoft.AspNetCore.Razor.Language.RazorCodeDocument";
    private const string RazorCSharpDocumentTypeName = "Microsoft.AspNetCore.Razor.Language.RazorCSharpDocument";

    public static bool TryCreateBatch(
        Compilation hookCompilation,
        ImmutableArray<RazorSgTailDocumentInput> inputs,
        out RazorSgTailBatch? batch,
        out string? failure)
    {
        if (hookCompilation is null)
            throw new ArgumentNullException(nameof(hookCompilation));

        batch = null;
        failure = null;
        if (inputs.IsDefaultOrEmpty)
        {
            failure = "The Razor SG final-document adapter did not receive any generated documents.";
            return false;
        }

        var documents = ImmutableArray.CreateBuilder<RazorSgGeneratedDocument>(inputs.Length);
        var identities = new HashSet<RazorSgGeneratedDocumentIdentity>();
        var hintNames = new HashSet<string>(StringComparer.Ordinal);
        foreach (var input in inputs)
        {
            if (!TryCreateDocument(input, out var document, out failure))
                return false;

            if (!hintNames.Add(document!.HintName))
            {
                failure = "The Razor SG final-document batch contained duplicate hint name '" +
                          document.HintName +
                          "'.";
                return false;
            }

            if (!identities.Add(document.Identity))
            {
                failure = "The Razor SG final-document batch contained duplicate document identity '" +
                          document.Identity.SourcePath +
                          "' / '" +
                          document.Identity.HintName +
                          "'.";
                return false;
            }

            documents.Add(document);
        }

        batch = new RazorSgTailBatch(hookCompilation, documents.ToImmutable());
        return true;
    }

    private static bool TryCreateDocument(
        RazorSgTailDocumentInput input,
        out RazorSgGeneratedDocument? document,
        out string? failure)
    {
        document = null;
        failure = null;
        if (string.IsNullOrWhiteSpace(input.HintName))
        {
            failure = "The Razor SG final-document input had an empty hint name.";
            return false;
        }

        if (input.CodeDocument is null || !HasTypeInBaseChain(input.CodeDocument, RazorCodeDocumentTypeName))
        {
            failure = "The Razor SG final-document input did not contain a compatible RazorCodeDocument.";
            return false;
        }

        if (input.CSharpDocument is null || !HasTypeInBaseChain(input.CSharpDocument, RazorCSharpDocumentTypeName))
        {
            failure = "The Razor SG final-document input did not contain a compatible RazorCSharpDocument.";
            return false;
        }

        if (!TryReadSourcePath(input.CodeDocument, out var sourcePath, out failure) ||
            !TryReadGeneratedCSharp(input.CSharpDocument, out var generatedCSharp, out failure))
        {
            return false;
        }

        if (!TryReadSourceMappings(input.CSharpDocument, out var mappings, out failure))
            return false;

        document = new RazorSgGeneratedDocument(
            input.HintName,
            sourcePath!,
            generatedCSharp!,
            mappings);
        return true;
    }

    private static bool TryReadSourcePath(object codeDocument, out string? sourcePath, out string? failure)
    {
        sourcePath = null;
        failure = null;
        var source = GetPropertyValue(codeDocument, "Source");
        if (source is null)
        {
            failure = "RazorCodeDocument.Source was not available from the Razor SG final-document input.";
            return false;
        }

        sourcePath = ReadString(source, "FilePath") ?? ReadString(source, "RelativePath");
        if (string.IsNullOrWhiteSpace(sourcePath))
        {
            failure = "RazorCodeDocument.Source did not provide a stable document path.";
            return false;
        }

        return true;
    }

    private static bool TryReadGeneratedCSharp(
        object csharpDocument,
        out SourceText? generatedCSharp,
        out string? failure)
    {
        generatedCSharp = null;
        failure = null;
        var text = GetPropertyValue(csharpDocument, "Text");
        if (text is null)
        {
            failure = "RazorCSharpDocument.Text was not available from the Razor SG final-document input.";
            return false;
        }

        generatedCSharp = text as SourceText ?? SourceText.From(text.ToString() ?? string.Empty, Encoding.UTF8);
        return true;
    }

    private static bool TryReadSourceMappings(
        object csharpDocument,
        out ImmutableArray<RazorSgSourceMapping> mappings,
        out string? failure)
    {
        mappings = ImmutableArray<RazorSgSourceMapping>.Empty;
        failure = null;
        foreach (var propertyName in new[]
                 {
                     "SourceMappingsSortedByOriginal",
                     "SourceMappingsSortedByGenerated",
                     "SourceMappings"
                 })
        {
            var value = GetPropertyValue(csharpDocument, propertyName);
            if (value is null)
                continue;

            if (value is not IEnumerable entries)
            {
                failure = "RazorCSharpDocument." + propertyName + " was not enumerable.";
                return false;
            }

            var builder = ImmutableArray.CreateBuilder<RazorSgSourceMapping>();
            foreach (var entry in entries)
            {
                if (entry is null)
                    continue;

                if (!TryReadSourceMapping(entry, out var mapping))
                {
                    failure = "A RazorCSharpDocument source mapping did not have readable original and generated spans.";
                    return false;
                }

                builder.Add(mapping);
            }

            mappings = builder.ToImmutable();
            return true;
        }

        failure = "RazorCSharpDocument did not expose a supported source-mappings collection.";
        return false;
    }

    private static bool TryReadSourceMapping(object value, out RazorSgSourceMapping mapping)
    {
        mapping = default;
        var original = GetPropertyValue(value, "OriginalSpan");
        var generated = GetPropertyValue(value, "GeneratedSpan");
        if (original is null || generated is null ||
            !TryReadSourceSpan(original, out var originalSpan) ||
            !TryReadSourceSpan(generated, out var generatedSpan))
        {
            return false;
        }

        mapping = new RazorSgSourceMapping(originalSpan, generatedSpan);
        return true;
    }

    private static bool TryReadSourceSpan(object value, out RazorSgSourceSpan span)
    {
        span = default;
        if (!TryReadInt(value, "AbsoluteIndex", out var absoluteIndex) ||
            !TryReadInt(value, "Length", out var length))
        {
            return false;
        }

        _ = TryReadInt(value, "LineIndex", out var lineIndex);
        _ = TryReadInt(value, "CharacterIndex", out var characterIndex);
        span = new RazorSgSourceSpan(
            ReadString(value, "FilePath"),
            absoluteIndex,
            length,
            lineIndex,
            characterIndex);
        return true;
    }

    private static bool HasTypeInBaseChain(object value, string expectedFullName)
    {
        for (var type = value.GetType(); type is not null; type = type.BaseType)
        {
            if (string.Equals(type.FullName, expectedFullName, StringComparison.Ordinal))
                return true;
        }

        return false;
    }

    private static object? GetPropertyValue(object value, string name)
        => value.GetType()
            .GetProperty(name, InstanceFlags)
            ?.GetValue(value);

    private static string? ReadString(object value, string name)
        => GetPropertyValue(value, name) as string;

    private static bool TryReadInt(object value, string name, out int number)
    {
        number = 0;
        return GetPropertyValue(value, name) is int valueAsInt && (number = valueAsInt) == valueAsInt;
    }
}

internal sealed record RazorSgTailDocumentInput(
    string HintName,
    object CodeDocument,
    object CSharpDocument);

internal sealed record RazorSgTailBatch(
    Compilation HookCompilation,
    ImmutableArray<RazorSgGeneratedDocument> Documents);

internal sealed record RazorSgGeneratedDocument(
    string HintName,
    string SourcePath,
    SourceText GeneratedCSharp,
    ImmutableArray<RazorSgSourceMapping> SourceMappings)
{
    public RazorSgGeneratedDocumentIdentity Identity { get; } = new(
        NormalizeIdentityPart(SourcePath),
        NormalizeIdentityPart(HintName));

    public string ContentHash { get; } = ComputeContentHash(GeneratedCSharp);

    private static string NormalizeIdentityPart(string value)
        => value.Replace('\\', '/');

    private static string ComputeContentHash(SourceText text)
    {
        using var sha256 = SHA256.Create();
        var hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(text.ToString()));
        var builder = new StringBuilder(hash.Length * 2);
        foreach (var item in hash)
            builder.Append(item.ToString("x2", System.Globalization.CultureInfo.InvariantCulture));

        return builder.ToString();
    }
}

internal sealed record RazorSgGeneratedDocumentIdentity(string SourcePath, string HintName);

internal readonly record struct RazorSgSourceSpan(
    string? FilePath,
    int AbsoluteIndex,
    int Length,
    int LineIndex,
    int CharacterIndex);

internal readonly record struct RazorSgSourceMapping(
    RazorSgSourceSpan OriginalSpan,
    RazorSgSourceSpan GeneratedSpan);
