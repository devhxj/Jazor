namespace Jazor.Npm;

public static class NpmExportSelection
{
    public static async Task<IReadOnlyList<string>> FindModulesAsync(string npmNamespace,
        IEnumerable<string> packages, Func<string, Task<IReadOnlyList<string>>> listSpecifiers,
        bool validatePackageRoot = false)
    {
        var matches = new List<string>();
        foreach (var package in packages)
        {
            var prefix = NpmNames.NamespaceFor(package);
            if (npmNamespace == prefix && !validatePackageRoot)
                matches.Add(package);
            else if (npmNamespace == prefix || npmNamespace.StartsWith(prefix + ".", StringComparison.Ordinal))
                matches.AddRange((await listSpecifiers(package)).Where(specifier => NpmNames.NamespaceFor(specifier) == npmNamespace));
        }
        return matches.Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal).ToArray();
    }

    public static IReadOnlyList<(string Export, string? Suffix)> FindMembers(ILookup<string, string> index, string name)
    {
        var direct = index[name].Select(export => (export, (string?)null)).ToArray();
        if (direct.Length != 0)
            return direct;

        // Prefixes only request metadata. The projection must confirm the managed suffix.
        var candidates = new List<(string, string?)>();
        for (var cut = 1; cut < name.Length; cut++)
            if (char.IsUpper(name[cut]))
                candidates.AddRange(index[name[..cut]].Select(export => (export, (string?)name[cut..])));
        return candidates;
    }

    public static bool Matches(NpmValueExport value, string? suffix) =>
        suffix is null || value.AttachedMemberType is not null && (value.AttachedMemberSuffix ?? "Members") == suffix ||
        value.Signatures.Any(signature => signature.ManagedNameSuffix == suffix);

    internal static string ReserveAttachedMemberSuffix(string name, HashSet<string> occupied)
    {
        var suffix = "Members";
        for (var number = 2; !occupied.Add(name + suffix); number++)
            suffix = "Members_" + number;
        return suffix;
    }
}
