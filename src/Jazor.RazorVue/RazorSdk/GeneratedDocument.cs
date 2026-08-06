using System.Collections.Immutable;
using System.Security.Cryptography;
using System.Text;
using Microsoft.CodeAnalysis.Text;

namespace Jazor.RazorVue.RazorSdk;

/// <summary>Generated C# text and source-map metadata for one Razor component document.</summary>
internal sealed record GeneratedDocument(
    string HintName,
    string SourcePath,
    SourceText GeneratedCSharp,
    ImmutableArray<RazorSourceMap> RazorSourceMaps)
{
    public string ContentHash { get; } = ComputeContentHash(GeneratedCSharp);

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

/// <summary>One source location retained from the generated C# document.</summary>
internal readonly record struct RazorSourceSpan(
    string? FilePath,
    int AbsoluteIndex,
    int Length,
    int LineIndex,
    int CharacterIndex);

/// <summary>Maps a generated span back to its authored Razor span.</summary>
internal readonly record struct RazorSourceMap(
    RazorSourceSpan OriginalSpan,
    RazorSourceSpan GeneratedSpan);
