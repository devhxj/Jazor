using System.Collections.Immutable;
using System.Security.Cryptography;
using System.Text;
using Microsoft.CodeAnalysis.Text;

namespace Jazor.RazorVue.RazorSdk;

internal sealed record RazorSgGeneratedDocument(
    string HintName,
    string SourcePath,
    SourceText GeneratedCSharp,
    ImmutableArray<RazorSgSourceMapping> SourceMappings)
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

internal readonly record struct RazorSgSourceSpan(
    string? FilePath,
    int AbsoluteIndex,
    int Length,
    int LineIndex,
    int CharacterIndex);

internal readonly record struct RazorSgSourceMapping(
    RazorSgSourceSpan OriginalSpan,
    RazorSgSourceSpan GeneratedSpan);
