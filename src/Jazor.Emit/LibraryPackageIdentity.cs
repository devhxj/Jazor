namespace Jazor.Emit;

/// <summary>
/// Normalizes logical npm/JSR package names to the package identity materialized by Deno.
/// JSR dependencies are installed through npm compatibility names (<c>@jsr/scope__name</c>),
/// while binding metadata and authored imports keep the original JSR specifier.
/// </summary>
internal static class LibraryPackageIdentity
{
    public static string GetCanonicalName(LibraryPackageReference reference)
    {
        ArgumentNullException.ThrowIfNull(reference);
        if (!string.Equals(reference.Source, "jsr", StringComparison.Ordinal))
            return reference.Name;

        if (TryParseJsrSpecifier(reference.Version, out var jsrPackage, out _))
            return EncodeJsrPackageName(jsrPackage);

        return EncodeJsrPackageName(GetAuthoredName(reference));
    }

    public static string GetDependencySpecifier(LibraryPackageReference reference)
    {
        ArgumentNullException.ThrowIfNull(reference);
        if (!string.Equals(reference.Source, "jsr", StringComparison.Ordinal))
            return reference.Version;

        if (reference.Version.StartsWith("jsr:", StringComparison.OrdinalIgnoreCase))
            return reference.Version;

        return "jsr:" + GetAuthoredName(reference) + "@" + reference.Version;
    }

    public static string GetCanonicalSpecifier(LibraryPackageReference reference, string specifier)
    {
        ArgumentNullException.ThrowIfNull(reference);
        ArgumentException.ThrowIfNullOrWhiteSpace(specifier);

        var canonicalName = GetCanonicalName(reference);
        if (string.Equals(reference.Source, "jsr", StringComparison.Ordinal))
        {
            var authoredName = GetAuthoredName(reference);
            if (string.Equals(specifier, authoredName, StringComparison.Ordinal))
                return canonicalName;
            if (specifier.StartsWith(authoredName + "/", StringComparison.Ordinal))
                return canonicalName + specifier[authoredName.Length..];

            // Metadata can already use the canonical key while the version keeps the authored
            // jsr: identity. Accept both forms so callers can normalize imports idempotently.
            if (string.Equals(specifier, reference.Name, StringComparison.Ordinal))
                return canonicalName;
            if (specifier.StartsWith(reference.Name + "/", StringComparison.Ordinal))
                return canonicalName + specifier[reference.Name.Length..];
        }

        return specifier;
    }

    public static string GetAuthoredName(LibraryPackageReference reference)
    {
        ArgumentNullException.ThrowIfNull(reference);
        if (!string.Equals(reference.Source, "jsr", StringComparison.Ordinal))
            return reference.Name;

        if (!string.IsNullOrWhiteSpace(reference.AuthoredName))
            return reference.AuthoredName!;

        return TryParseJsrSpecifier(reference.Version, out var jsrPackage, out _)
            ? jsrPackage
            : DecodeCanonicalName(reference.Name);
    }

    public static string ResolveAuthoredName(string declaredName, string source, string version, string? authoredName)
    {
        if (!string.IsNullOrWhiteSpace(authoredName))
            return authoredName.Trim();
        if (!string.Equals(source, "jsr", StringComparison.Ordinal))
            return declaredName;
        if (TryParseJsrSpecifier(version, out var jsrPackage, out _))
            return jsrPackage;
        return DecodeCanonicalName(declaredName);
    }

    public static bool TryGetReference(
        LibraryAssets libraries,
        string specifier,
        out LibraryPackageReference reference)
    {
        ArgumentNullException.ThrowIfNull(libraries);
        ArgumentException.ThrowIfNullOrWhiteSpace(specifier);

        if (libraries.PackageReferences.TryGetValue(specifier, out reference!))
            return true;

        var packageName = GetPackageName(specifier);
        if (libraries.PackageReferences.TryGetValue(packageName, out reference!))
            return true;

        foreach (var candidate in libraries.PackageReferences.Values
                     .OrderBy(static value => value.Name, StringComparer.Ordinal))
        {
            if (string.Equals(candidate.CanonicalName, packageName, StringComparison.Ordinal))
            {
                reference = candidate;
                return true;
            }
        }

        reference = null!;
        return false;
    }

    public static string GetPackageName(string specifier)
    {
        var segments = specifier.Split('/', StringSplitOptions.RemoveEmptyEntries);
        var count = specifier.StartsWith('@', StringComparison.Ordinal) ? 2 : 1;
        return segments.Length >= count
            ? string.Join('/', segments.Take(count))
            : specifier;
    }

    private static string EncodeJsrPackageName(string packageName)
    {
        var normalized = packageName.Trim();
        if (normalized.StartsWith("@jsr/", StringComparison.Ordinal))
            return normalized;

        if (normalized.StartsWith('@', StringComparison.Ordinal))
            normalized = normalized[1..];

        return "@jsr/" + normalized.Replace("/", "__", StringComparison.Ordinal);
    }

    private static string DecodeCanonicalName(string packageName)
    {
        var normalized = packageName.Trim();
        if (!normalized.StartsWith("@jsr/", StringComparison.Ordinal))
            return normalized;

        var encoded = normalized[5..];
        var separator = encoded.IndexOf("__", StringComparison.Ordinal);
        return separator > 0 && separator + 2 < encoded.Length
            ? "@" + encoded[..separator] + "/" + encoded[(separator + 2)..]
            : normalized;
    }

    private static bool TryParseJsrSpecifier(
        string value,
        out string packageName,
        out string version)
    {
        packageName = string.Empty;
        version = string.Empty;
        if (!value.StartsWith("jsr:", StringComparison.OrdinalIgnoreCase))
            return false;

        var authored = value[4..];
        var slash = authored.IndexOf('/', StringComparison.Ordinal);
        var separator = slash < 0
            ? authored.IndexOf('@', StringComparison.Ordinal)
            : authored.IndexOf('@', slash + 1);
        if (separator <= 0)
            return false;

        packageName = authored[..separator];
        version = authored[(separator + 1)..];
        return packageName.Length > 0 && version.Length > 0;
    }
}
