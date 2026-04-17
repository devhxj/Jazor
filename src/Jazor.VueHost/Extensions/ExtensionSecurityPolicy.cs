using System.Security.Cryptography;

namespace Jazor.VueHost.Extensions;

internal static class ExtensionSecurityPolicy
{
    private static readonly IReadOnlyDictionary<Type, string> ProviderCapabilityByInterface = new Dictionary<Type, string>
    {
        [typeof(ILspDiagnosticProvider)] = "diagnostic",
        [typeof(ILspCodeActionProvider)] = "codeAction",
        [typeof(ILspHoverProvider)] = "hover",
        [typeof(ILspCompletionProvider)] = "completion",
        [typeof(ILspDocumentSymbolProvider)] = "documentSymbol",
        [typeof(ILspSignatureHelpProvider)] = "signatureHelp",
        [typeof(ILspInlayHintProvider)] = "inlayHint",
        [typeof(ILspWorkspaceSymbolProvider)] = "workspaceSymbol",
        [typeof(ILspFoldingRangeProvider)] = "foldingRange",
        [typeof(ILspReferenceProvider)] = "references",
        [typeof(ILspRenameProvider)] = "rename"
    };

    public static IReadOnlySet<string> GetProvidedCapabilities(IExtension extension)
    {
        ArgumentNullException.ThrowIfNull(extension);
        return GetProvidedCapabilities(extension.GetType());
    }

    public static IReadOnlySet<string> GetProvidedCapabilities(Type extensionType)
    {
        ArgumentNullException.ThrowIfNull(extensionType);
        var interfaces = extensionType.GetInterfaces();
        var set = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var providerInterface in ProviderCapabilityByInterface.Keys)
        {
            if (interfaces.Contains(providerInterface))
            {
                set.Add(ProviderCapabilityByInterface[providerInterface]);
            }
        }

        return set;
    }

    public static IReadOnlySet<string> NormalizeAllowedCapabilities(ExtensionManifest manifest)
    {
        ArgumentNullException.ThrowIfNull(manifest);

        var normalized = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var rawCapability in manifest.Permissions?.Providers ?? Array.Empty<string>())
        {
            var capability = rawCapability?.Trim();
            if (string.IsNullOrWhiteSpace(capability))
            {
                continue;
            }

            if (IsKnownCapability(capability))
            {
                normalized.Add(capability);
            }
        }

        return normalized;
    }

    public static bool IsKnownCapability(string capability)
        => ProviderCapabilityByInterface.Values.Contains(capability, StringComparer.OrdinalIgnoreCase);

    public static bool IsProviderPermissionSatisfied(
        Type extensionType,
        ExtensionManifest manifest,
        out string? reason)
    {
        ArgumentNullException.ThrowIfNull(extensionType);
        ArgumentNullException.ThrowIfNull(manifest);

        var providedCapabilities = GetProvidedCapabilities(extensionType);
        if (providedCapabilities.Count == 0)
        {
            reason = null;
            return true;
        }

        var allowedCapabilities = NormalizeAllowedCapabilities(manifest);
        var deniedCapabilities = providedCapabilities
            .Where(capability => !allowedCapabilities.Contains(capability))
            .Order(StringComparer.OrdinalIgnoreCase)
            .ToArray();
        if (deniedCapabilities.Length == 0)
        {
            reason = null;
            return true;
        }

        reason = $"provider capability denied: {string.Join(", ", deniedCapabilities)}";
        return false;
    }

    public static bool IsAssemblyHashSatisfied(
        string assemblyPath,
        string expectedSha256)
    {
        if (string.IsNullOrWhiteSpace(assemblyPath) || string.IsNullOrWhiteSpace(expectedSha256))
        {
            return false;
        }

        var normalizedExpected = NormalizeSha256(expectedSha256);
        if (normalizedExpected.Length == 0)
        {
            return false;
        }

        var computed = ComputeSha256Hex(assemblyPath);
        return string.Equals(
            computed,
            normalizedExpected,
            StringComparison.OrdinalIgnoreCase);
    }

    public static string NormalizeSha256(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        var normalized = value.Trim();
        if (normalized.StartsWith("sha256:", StringComparison.OrdinalIgnoreCase))
        {
            normalized = normalized["sha256:".Length..];
        }

        normalized = normalized
            .Replace(" ", string.Empty, StringComparison.Ordinal)
            .Replace("-", string.Empty, StringComparison.Ordinal);

        return normalized.ToUpperInvariant();
    }

    private static string ComputeSha256Hex(string filePath)
    {
        var bytes = File.ReadAllBytes(filePath);
        return Convert.ToHexString(SHA256.HashData(bytes));
    }
}
