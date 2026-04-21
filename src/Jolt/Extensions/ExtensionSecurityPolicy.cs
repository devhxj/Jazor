using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace Jolt.Extensions;

/// <summary>
/// Validates manifest-declared extension capabilities and builds the Jolt-enforced runtime policy.
/// Process isolation here means a separate worker process plus request-surface checks, not an OS sandbox.
/// </summary>
internal static class ExtensionSecurityPolicy
{
    private const int IoRankNone = 0;
    private const int IoRankRead = 1;
    private const int IoRankReadWrite = 2;

    private const int NetworkRankNone = 0;
    private const int NetworkRankLoopback = 1;
    private const int NetworkRankInternet = 2;

    private static readonly IReadOnlyDictionary<Type, string> ProviderCapabilityByInterface = new Dictionary<Type, string>
    {
        [typeof(ILspDiagnosticProvider)] = ExtensionCapabilityNames.Diagnostic,
        [typeof(ILspCodeActionProvider)] = ExtensionCapabilityNames.CodeAction,
        [typeof(ILspHoverProvider)] = ExtensionCapabilityNames.Hover,
        [typeof(ILspCompletionProvider)] = ExtensionCapabilityNames.Completion,
        [typeof(ILspDocumentSymbolProvider)] = ExtensionCapabilityNames.DocumentSymbol,
        [typeof(ILspSignatureHelpProvider)] = ExtensionCapabilityNames.SignatureHelp,
        [typeof(ILspInlayHintProvider)] = ExtensionCapabilityNames.InlayHint,
        [typeof(ILspWorkspaceSymbolProvider)] = ExtensionCapabilityNames.WorkspaceSymbol,
        [typeof(ILspFoldingRangeProvider)] = ExtensionCapabilityNames.FoldingRange,
        [typeof(ILspReferenceProvider)] = ExtensionCapabilityNames.References,
        [typeof(ILspRenameProvider)] = ExtensionCapabilityNames.Rename
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
        return IsProviderPermissionSatisfied(providedCapabilities, manifest, out reason);
    }

    public static bool IsProviderPermissionSatisfied(
        IReadOnlySet<string> providedCapabilities,
        ExtensionManifest manifest,
        out string? reason)
    {
        ArgumentNullException.ThrowIfNull(providedCapabilities);
        ArgumentNullException.ThrowIfNull(manifest);

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

    public static bool IsSandboxPermissionSatisfied(
        ExtensionManifest manifest,
        ExtensionHostOptions options,
        string rootDirectory,
        string extensionDirectory,
        out string? reason)
    {
        ArgumentNullException.ThrowIfNull(manifest);
        ArgumentNullException.ThrowIfNull(options);

        if (!ValidateProcessIsolationRequirement(manifest, options, out reason))
        {
            return false;
        }

        if (!ValidateIoCapability(manifest, options, rootDirectory, extensionDirectory, out reason))
        {
            return false;
        }

        if (!ValidateNetworkCapability(manifest, options, out reason))
        {
            return false;
        }

        reason = null;
        return true;
    }

    public static ExtensionSandboxProfile CreateRuntimeSandboxProfile(
        ExtensionManifest manifest,
        string rootDirectory,
        string extensionDirectory)
    {
        ArgumentNullException.ThrowIfNull(manifest);
        if (string.IsNullOrWhiteSpace(rootDirectory))
        {
            throw new ArgumentException("Value cannot be null or whitespace.", nameof(rootDirectory));
        }

        if (string.IsNullOrWhiteSpace(extensionDirectory))
        {
            throw new ArgumentException("Value cannot be null or whitespace.", nameof(extensionDirectory));
        }

        var normalizedRootDirectory = Path.GetFullPath(rootDirectory);
        var normalizedExtensionDirectory = Path.GetFullPath(extensionDirectory);
        var ioPermission = manifest.Permissions?.Io;
        var networkPermission = manifest.Permissions?.Network;

        var ioCapability = NormalizeIoCapability(ioPermission?.Level) ?? ExtensionHostOptions.IoCapabilityNone;
        var networkCapability = NormalizeNetworkCapability(networkPermission?.Level) ?? ExtensionHostOptions.NetworkCapabilityNone;

        var readRoots = ResolvePermissionPathsForRuntime(
            normalizedRootDirectory,
            normalizedExtensionDirectory,
            NormalizePermissionPaths(ioPermission?.ReadRoots));
        var writeRoots = ResolvePermissionPathsForRuntime(
            normalizedRootDirectory,
            normalizedExtensionDirectory,
            NormalizePermissionPaths(ioPermission?.WriteRoots));

        if (string.Equals(ioCapability, ExtensionHostOptions.IoCapabilityNone, StringComparison.OrdinalIgnoreCase))
        {
            readRoots = Array.Empty<string>();
            writeRoots = Array.Empty<string>();
        }
        else if (string.Equals(ioCapability, ExtensionHostOptions.IoCapabilityRead, StringComparison.OrdinalIgnoreCase))
        {
            if (writeRoots.Length > 0)
            {
                throw new InvalidOperationException("io level 'read' cannot declare writeRoots.");
            }

            if (readRoots.Length == 0)
            {
                readRoots =
                [
                    normalizedRootDirectory,
                    normalizedExtensionDirectory
                ];
            }
        }
        else if (string.Equals(ioCapability, ExtensionHostOptions.IoCapabilityReadWrite, StringComparison.OrdinalIgnoreCase))
        {
            if (readRoots.Length == 0)
            {
                readRoots =
                [
                    normalizedRootDirectory,
                    normalizedExtensionDirectory
                ];
            }

            if (writeRoots.Length == 0)
            {
                writeRoots = [normalizedExtensionDirectory];
            }
        }

        var allowedHosts = NormalizeHosts(networkPermission?.AllowedHosts);
        if (string.Equals(networkCapability, ExtensionHostOptions.NetworkCapabilityNone, StringComparison.OrdinalIgnoreCase))
        {
            allowedHosts = Array.Empty<string>();
        }
        else if (string.Equals(networkCapability, ExtensionHostOptions.NetworkCapabilityLoopback, StringComparison.OrdinalIgnoreCase)
                 && allowedHosts.Length == 0)
        {
            allowedHosts =
            [
                "localhost",
                "127.0.0.1",
                "::1"
            ];
        }

        // This profile constrains document/edit/network data that flows through Jolt request handlers.
        // It does not revoke the worker process's ambient OS access outside those mediated surfaces.
        return new ExtensionSandboxProfile
        {
            IoCapability = ioCapability,
            NetworkCapability = networkCapability,
            ReadRoots = readRoots,
            WriteRoots = writeRoots,
            AllowedHosts = allowedHosts
        };
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

        payload["processIsolation"] = manifest.Permissions?.ProcessIsolation ?? false;
        payload["io"] = NormalizeIoPermissionForPayload(manifest.Permissions?.Io);
        payload["network"] = NormalizeNetworkPermissionForPayload(manifest.Permissions?.Network);
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
        using var stream = File.OpenRead(filePath);
        using var sha256 = SHA256.Create();
        return Convert.ToHexString(sha256.ComputeHash(stream));
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

    private static bool ValidateProcessIsolationRequirement(
        ExtensionManifest manifest,
        ExtensionHostOptions options,
        out string? reason)
    {
        if (RequiresCapabilityBoundSandbox(manifest)
            && manifest.Permissions?.ProcessIsolation != true)
        {
            reason = "a separate worker process is required when io/network capabilities are declared";
            return false;
        }

        if (options.RequireProcessIsolation
            && manifest.Permissions?.ProcessIsolation != true)
        {
            reason = "a separate worker process is required by host policy";
            return false;
        }

        reason = null;
        return true;
    }

    private static bool RequiresCapabilityBoundSandbox(ExtensionManifest manifest)
    {
        var permissions = manifest.Permissions;
        if (permissions is null)
        {
            return false;
        }

        var ioLevel = NormalizeIoCapability(permissions.Io?.Level) ?? ExtensionHostOptions.IoCapabilityNone;
        var networkLevel = NormalizeNetworkCapability(permissions.Network?.Level) ?? ExtensionHostOptions.NetworkCapabilityNone;
        if (!string.Equals(ioLevel, ExtensionHostOptions.IoCapabilityNone, StringComparison.OrdinalIgnoreCase)
            || !string.Equals(networkLevel, ExtensionHostOptions.NetworkCapabilityNone, StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        var readRoots = NormalizePermissionPaths(permissions.Io?.ReadRoots);
        var writeRoots = NormalizePermissionPaths(permissions.Io?.WriteRoots);
        var allowedHosts = NormalizeHosts(permissions.Network?.AllowedHosts);
        return readRoots.Length > 0
            || writeRoots.Length > 0
            || allowedHosts.Length > 0;
    }

    private static bool ValidateIoCapability(
        ExtensionManifest manifest,
        ExtensionHostOptions options,
        string rootDirectory,
        string extensionDirectory,
        out string? reason)
    {
        var ioPermission = manifest.Permissions?.Io;
        var normalizedIoLevel = NormalizeIoCapability(ioPermission?.Level);
        if (!string.IsNullOrWhiteSpace(ioPermission?.Level) && normalizedIoLevel is null)
        {
            reason = $"unsupported io capability '{ioPermission?.Level}'";
            return false;
        }

        var ioLevel = normalizedIoLevel ?? ExtensionHostOptions.IoCapabilityNone;
        if (!TryGetIoCapabilityRank(ioLevel, out var requestedRank))
        {
            reason = $"unsupported io capability '{ioPermission?.Level}'";
            return false;
        }

        if (!TryGetIoCapabilityRank(options.MaxIoCapability, out var maxRank))
        {
            reason = $"unsupported host max io capability '{options.MaxIoCapability}'";
            return false;
        }

        if (requestedRank > maxRank)
        {
            reason = $"io capability '{ioLevel}' exceeds host max '{options.MaxIoCapability}'";
            return false;
        }

        var readRoots = NormalizePermissionPaths(ioPermission?.ReadRoots);
        var writeRoots = NormalizePermissionPaths(ioPermission?.WriteRoots);
        if (requestedRank == IoRankNone && (readRoots.Length > 0 || writeRoots.Length > 0))
        {
            reason = "io level 'none' cannot declare readRoots/writeRoots";
            return false;
        }

        if (requestedRank == IoRankRead && writeRoots.Length > 0)
        {
            reason = "io level 'read' cannot declare writeRoots";
            return false;
        }

        foreach (var root in readRoots.Concat(writeRoots))
        {
            if (!TryResolvePermissionPath(rootDirectory, extensionDirectory, root, out _, out reason))
            {
                return false;
            }
        }

        reason = null;
        return true;
    }

    private static string[] ResolvePermissionPathsForRuntime(
        string rootDirectory,
        string extensionDirectory,
        IReadOnlyList<string> rawPaths)
    {
        if (rawPaths.Count == 0)
        {
            return Array.Empty<string>();
        }

        var resolved = new List<string>(rawPaths.Count);
        foreach (var rawPath in rawPaths)
        {
            if (!TryResolvePermissionPath(rootDirectory, extensionDirectory, rawPath, out var resolvedPath, out var reason))
            {
                throw new InvalidOperationException(reason ?? $"Invalid io permission path '{rawPath}'.");
            }

            resolved.Add(resolvedPath);
        }

        return resolved
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Order(StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    private static bool ValidateNetworkCapability(
        ExtensionManifest manifest,
        ExtensionHostOptions options,
        out string? reason)
    {
        var networkPermission = manifest.Permissions?.Network;
        var normalizedNetworkLevel = NormalizeNetworkCapability(networkPermission?.Level);
        if (!string.IsNullOrWhiteSpace(networkPermission?.Level) && normalizedNetworkLevel is null)
        {
            reason = $"unsupported network capability '{networkPermission?.Level}'";
            return false;
        }

        var networkLevel = normalizedNetworkLevel ?? ExtensionHostOptions.NetworkCapabilityNone;
        if (!TryGetNetworkCapabilityRank(networkLevel, out var requestedRank))
        {
            reason = $"unsupported network capability '{networkPermission?.Level}'";
            return false;
        }

        if (!TryGetNetworkCapabilityRank(options.MaxNetworkCapability, out var maxRank))
        {
            reason = $"unsupported host max network capability '{options.MaxNetworkCapability}'";
            return false;
        }

        if (requestedRank > maxRank)
        {
            reason = $"network capability '{networkLevel}' exceeds host max '{options.MaxNetworkCapability}'";
            return false;
        }

        var allowedHosts = NormalizeHosts(networkPermission?.AllowedHosts);
        if (requestedRank == NetworkRankNone && allowedHosts.Length > 0)
        {
            reason = "network level 'none' cannot declare allowedHosts";
            return false;
        }

        foreach (var host in allowedHosts)
        {
            if (Uri.CheckHostName(host) == UriHostNameType.Unknown && !string.Equals(host, "*", StringComparison.Ordinal))
            {
                reason = $"invalid network host '{host}'";
                return false;
            }

            if (requestedRank == NetworkRankLoopback
                && !IsLoopbackHost(host))
            {
                reason = $"network level 'loopback' does not allow host '{host}'";
                return false;
            }
        }

        reason = null;
        return true;
    }

    private static object NormalizeIoPermissionForPayload(ExtensionIoPermissionManifest? io)
    {
        var level = NormalizeIoCapability(io?.Level) ?? ExtensionHostOptions.IoCapabilityNone;
        return new SortedDictionary<string, object?>(StringComparer.Ordinal)
        {
            ["level"] = level,
            ["readRoots"] = NormalizePermissionPaths(io?.ReadRoots),
            ["writeRoots"] = NormalizePermissionPaths(io?.WriteRoots)
        };
    }

    private static object NormalizeNetworkPermissionForPayload(ExtensionNetworkPermissionManifest? network)
    {
        var level = NormalizeNetworkCapability(network?.Level) ?? ExtensionHostOptions.NetworkCapabilityNone;
        return new SortedDictionary<string, object?>(StringComparer.Ordinal)
        {
            ["level"] = level,
            ["allowedHosts"] = NormalizeHosts(network?.AllowedHosts)
        };
    }

    private static string[] NormalizePermissionPaths(string[]? paths)
    {
        if (paths is null)
        {
            return Array.Empty<string>();
        }

        return paths
            .Where(static item => !string.IsNullOrWhiteSpace(item))
            .Select(static item => item.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Order(StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    private static string[] NormalizeHosts(string[]? hosts)
    {
        if (hosts is null)
        {
            return Array.Empty<string>();
        }

        return hosts
            .Where(static item => !string.IsNullOrWhiteSpace(item))
            .Select(static item => item.Trim().ToLowerInvariant())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Order(StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    private static string? NormalizeIoCapability(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
        {
            return null;
        }

        return raw.Trim().ToLowerInvariant() switch
        {
            "none" => ExtensionHostOptions.IoCapabilityNone,
            "read" => ExtensionHostOptions.IoCapabilityRead,
            "readwrite" or "read-write" or "read_write" => ExtensionHostOptions.IoCapabilityReadWrite,
            _ => null
        };
    }

    private static string? NormalizeNetworkCapability(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
        {
            return null;
        }

        return raw.Trim().ToLowerInvariant() switch
        {
            "none" => ExtensionHostOptions.NetworkCapabilityNone,
            "loopback" => ExtensionHostOptions.NetworkCapabilityLoopback,
            "internet" => ExtensionHostOptions.NetworkCapabilityInternet,
            _ => null
        };
    }

    private static bool TryGetIoCapabilityRank(string capability, out int rank)
    {
        switch (capability)
        {
            case ExtensionHostOptions.IoCapabilityNone:
                rank = IoRankNone;
                return true;
            case ExtensionHostOptions.IoCapabilityRead:
                rank = IoRankRead;
                return true;
            case ExtensionHostOptions.IoCapabilityReadWrite:
                rank = IoRankReadWrite;
                return true;
            default:
                rank = default;
                return false;
        }
    }

    private static bool TryGetNetworkCapabilityRank(string capability, out int rank)
    {
        switch (capability)
        {
            case ExtensionHostOptions.NetworkCapabilityNone:
                rank = NetworkRankNone;
                return true;
            case ExtensionHostOptions.NetworkCapabilityLoopback:
                rank = NetworkRankLoopback;
                return true;
            case ExtensionHostOptions.NetworkCapabilityInternet:
                rank = NetworkRankInternet;
                return true;
            default:
                rank = default;
                return false;
        }
    }

    private static bool IsLoopbackHost(string host)
    {
        return string.Equals(host, "localhost", StringComparison.OrdinalIgnoreCase)
            || string.Equals(host, "127.0.0.1", StringComparison.OrdinalIgnoreCase)
            || string.Equals(host, "::1", StringComparison.OrdinalIgnoreCase);
    }

    private static bool TryResolvePermissionPath(
        string rootDirectory,
        string extensionDirectory,
        string rawPath,
        out string resolvedPath,
        out string? reason)
    {
        var candidate = Path.IsPathRooted(rawPath)
            ? Path.GetFullPath(rawPath)
            : Path.GetFullPath(Path.Combine(extensionDirectory, rawPath));
        var normalizedExtensionDirectory = Path.GetFullPath(extensionDirectory);
        var normalizedRootDirectory = Path.GetFullPath(rootDirectory);

        var insideExtension = IsPathInsideDirectory(normalizedExtensionDirectory, candidate);
        var insideRoot = IsPathInsideDirectory(normalizedRootDirectory, candidate);
        if (!insideExtension && !insideRoot)
        {
            resolvedPath = string.Empty;
            reason = $"io permission path '{rawPath}' escapes extension/root boundary";
            return false;
        }

        resolvedPath = candidate;
        reason = null;
        return true;
    }

    private static bool IsPathInsideDirectory(string directoryPath, string candidatePath)
    {
        var relativePath = Path.GetRelativePath(directoryPath, candidatePath);
        return !string.IsNullOrWhiteSpace(relativePath)
            && !string.Equals(relativePath, "..", StringComparison.Ordinal)
            && !relativePath.StartsWith(".." + Path.DirectorySeparatorChar, StringComparison.Ordinal)
            && !relativePath.StartsWith(".." + Path.AltDirectorySeparatorChar, StringComparison.Ordinal)
            && !Path.IsPathRooted(relativePath);
    }
}
