using System;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;

namespace Jazor.Common;

/// <summary>Canonical SHA-256 contract used by generated artifacts and resource manifests.</summary>
public static class ArtifactHash
{
    public const int Sha256HexLength = 64;

    public static string ComputeSha256(string content)
    {
        if (content is null)
            throw new ArgumentNullException(nameof(content));

        return ComputeSha256(Encoding.UTF8.GetBytes(content));
    }

    public static string ComputeSha256(byte[] content)
    {
        if (content is null)
            throw new ArgumentNullException(nameof(content));

        using var sha256 = SHA256.Create();
        var bytes = sha256.ComputeHash(content);
        var builder = new StringBuilder(bytes.Length * 2);
        foreach (var value in bytes)
            builder.Append(value.ToString("x2", CultureInfo.InvariantCulture));
        return builder.ToString();
    }

    public static string RequireSha256(string value, string description)
    {
        if (string.IsNullOrWhiteSpace(value) || value.Length != Sha256HexLength)
            throw new InvalidOperationException($"{description} must be a {Sha256HexLength}-character lowercase SHA-256 hex value.");

        foreach (var character in value)
        {
            if ((character < '0' || character > '9') &&
                (character < 'a' || character > 'f'))
            {
                throw new InvalidOperationException($"{description} must be a {Sha256HexLength}-character lowercase SHA-256 hex value.");
            }
        }

        return value;
    }
}
