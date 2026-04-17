using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

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

    public static bool IsManifestSignatureSatisfied(
        ExtensionManifest manifest,
        IReadOnlyDictionary<string, string> trustedPublicKeys,
        out string? reason)
    {
        ArgumentNullException.ThrowIfNull(manifest);
        ArgumentNullException.ThrowIfNull(trustedPublicKeys);

        var signature = manifest.Signature;
        if (signature is null)
        {
            reason = "manifest signature is missing";
            return false;
        }

        var keyId = signature.KeyId?.Trim();
        var algorithm = signature.Algorithm?.Trim();
        var signatureValue = signature.Value?.Trim();
        if (string.IsNullOrWhiteSpace(keyId))
        {
            reason = "manifest signature keyId is missing";
            return false;
        }

        if (string.IsNullOrWhiteSpace(algorithm))
        {
            reason = "manifest signature algorithm is missing";
            return false;
        }

        if (!string.Equals(algorithm, "RS256", StringComparison.OrdinalIgnoreCase)
            && !string.Equals(algorithm, "RSA-SHA256", StringComparison.OrdinalIgnoreCase))
        {
            reason = $"unsupported manifest signature algorithm '{algorithm}'";
            return false;
        }

        if (string.IsNullOrWhiteSpace(signatureValue))
        {
            reason = "manifest signature value is missing";
            return false;
        }

        if (!trustedPublicKeys.TryGetValue(keyId, out var trustedPublicKeyValue)
            || string.IsNullOrWhiteSpace(trustedPublicKeyValue))
        {
            reason = $"trusted public key '{keyId}' is not configured";
            return false;
        }

        if (!TryDecodeSignatureValue(signatureValue, out var signatureBytes))
        {
            reason = "manifest signature value is not valid base64/base64url";
            return false;
        }

        if (!TryImportTrustedPublicKey(trustedPublicKeyValue, out var rsa))
        {
            reason = $"trusted public key '{keyId}' cannot be parsed";
            return false;
        }

        using (rsa)
        {
            var payload = BuildManifestSignaturePayload(manifest);
            var payloadBytes = Encoding.UTF8.GetBytes(payload);
            var verified = rsa.VerifyData(
                payloadBytes,
                signatureBytes,
                HashAlgorithmName.SHA256,
                RSASignaturePadding.Pkcs1);
            if (!verified)
            {
                reason = "manifest signature verification failed";
                return false;
            }
        }

        reason = null;
        return true;
    }

    public static string BuildManifestSignaturePayload(ExtensionManifest manifest)
    {
        ArgumentNullException.ThrowIfNull(manifest);

        var payload = new SortedDictionary<string, object?>(StringComparer.Ordinal)
        {
            ["id"] = manifest.Id?.Trim() ?? string.Empty,
            ["assembly"] = manifest.Assembly?.Trim() ?? string.Empty,
            ["assemblySha256"] = NormalizeSha256(manifest.AssemblySha256 ?? string.Empty),
            ["type"] = manifest.Type?.Trim() ?? string.Empty
        };

        var providers = (manifest.Permissions?.Providers ?? Array.Empty<string>())
            .Where(static capability => !string.IsNullOrWhiteSpace(capability))
            .Select(static capability => capability.Trim())
            .Order(StringComparer.OrdinalIgnoreCase)
            .ToArray();
        payload["providers"] = providers;

        payload["settings"] = NormalizeSettingsForPayload(manifest.Settings);

        return JsonSerializer.Serialize(payload);
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

    private static object NormalizeSettingsForPayload(Dictionary<string, JsonElement>? settings)
    {
        if (settings is null || settings.Count == 0)
        {
            return new SortedDictionary<string, object?>(StringComparer.Ordinal);
        }

        var normalized = new SortedDictionary<string, object?>(StringComparer.Ordinal);
        foreach (var pair in settings.OrderBy(static item => item.Key, StringComparer.Ordinal))
        {
            normalized[pair.Key] = pair.Value.ValueKind switch
            {
                JsonValueKind.String => pair.Value.GetString(),
                JsonValueKind.Number => pair.Value.GetRawText(),
                JsonValueKind.True => true,
                JsonValueKind.False => false,
                JsonValueKind.Null => null,
                _ => pair.Value.GetRawText()
            };
        }

        return normalized;
    }

    private static bool TryDecodeSignatureValue(string signatureValue, out byte[] signatureBytes)
    {
        try
        {
            signatureBytes = Convert.FromBase64String(signatureValue);
            return true;
        }
        catch (FormatException)
        {
            try
            {
                signatureBytes = Base64UrlDecode(signatureValue);
                return true;
            }
            catch (FormatException)
            {
                signatureBytes = [];
                return false;
            }
        }
    }

    private static bool TryImportTrustedPublicKey(string trustedPublicKeyValue, out RSA rsa)
    {
        rsa = RSA.Create();
        try
        {
            if (trustedPublicKeyValue.Contains("BEGIN PUBLIC KEY", StringComparison.Ordinal))
            {
                rsa.ImportFromPem(trustedPublicKeyValue);
                return true;
            }

            var keyBytes = Convert.FromBase64String(trustedPublicKeyValue);
            rsa.ImportSubjectPublicKeyInfo(keyBytes, out _);
            return true;
        }
        catch (CryptographicException)
        {
            rsa.Dispose();
            rsa = RSA.Create();
            return false;
        }
        catch (FormatException)
        {
            rsa.Dispose();
            rsa = RSA.Create();
            return false;
        }
    }

    private static byte[] Base64UrlDecode(string value)
    {
        var normalized = value
            .Replace("-", "+", StringComparison.Ordinal)
            .Replace("_", "/", StringComparison.Ordinal);
        var padding = normalized.Length % 4;
        if (padding > 0)
        {
            normalized = normalized.PadRight(normalized.Length + (4 - padding), '=');
        }

        return Convert.FromBase64String(normalized);
    }
}
