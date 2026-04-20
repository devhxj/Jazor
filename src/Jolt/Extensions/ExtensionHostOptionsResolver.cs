using Jolt.DevServer;
using System.Text.Json;

namespace Jolt.Extensions;

internal static class ExtensionHostOptionsResolver
{
    public static ExtensionHostOptions Resolve(
        string[] args,
        string rootDirectory,
        JazorConfig? config)
    {
        ArgumentNullException.ThrowIfNull(args);

        var normalizedRoot = Path.GetFullPath(string.IsNullOrWhiteSpace(rootDirectory)
            ? Directory.GetCurrentDirectory()
            : rootDirectory);

        var configExtensions = config?.Extensions;
        var enabled = configExtensions?.Enabled ?? true;
        var allowExternalDirectory = configExtensions?.AllowExternalDirectory ?? false;
        var directory = string.IsNullOrWhiteSpace(configExtensions?.Directory)
            ? ".jazor/extensions"
            : configExtensions!.Directory!;
        var disabledIds = ToSet(configExtensions?.Disabled);
        var trustedIds = ToSet(configExtensions?.Trusted);
        var trustedPublicKeys = ToDictionary(configExtensions?.TrustedPublicKeys);
        var trustKeysFile = configExtensions?.TrustKeysFile;
        var requireAssemblyHash = configExtensions?.RequireAssemblyHash ?? true;
        var enforceProviderPermissions = configExtensions?.EnforceProviderPermissions ?? true;
        var requireManifestSignature = configExtensions?.RequireManifestSignature ?? true;
        var requireProcessIsolation = configExtensions?.RequireProcessIsolation ?? false;
        var maxIoCapability = ResolveIoCapability(
                configExtensions?.MaxIoCapability,
                "extensions.maxIoCapability")
            ?? ExtensionHostOptions.IoCapabilityRead;
        var maxNetworkCapability = ResolveNetworkCapability(
                configExtensions?.MaxNetworkCapability,
                "extensions.maxNetworkCapability")
            ?? ExtensionHostOptions.NetworkCapabilityLoopback;
        var loadLogFile = configExtensions?.LoadLogFile;
        var loadEventRetention = configExtensions?.LoadEventRetention ?? 200;
        var providerLogFile = configExtensions?.ProviderLogFile;
        var providerEventRetention = configExtensions?.ProviderEventRetention ?? 500;

        if (TryGetOptionValue(args, "--extensions-enabled", out var enabledValue))
        {
            if (!TryParseBoolean(enabledValue, out var enabledOverride))
            {
                throw CreateInvalidBooleanOptionException("--extensions-enabled", enabledValue);
            }

            enabled = enabledOverride;
        }

        if (TryGetOptionValue(args, "--extensions-dir", out var directoryOverride)
            && !string.IsNullOrWhiteSpace(directoryOverride))
        {
            directory = directoryOverride;
        }

        if (TryGetOptionValue(args, "--extensions-disabled", out var disabledOverride))
        {
            disabledIds = ToSet(ParseList(disabledOverride));
        }

        if (TryGetOptionValue(args, "--extensions-trusted", out var trustedOverride))
        {
            trustedIds = ToSet(ParseList(trustedOverride));
        }

        if (TryGetOptionValue(args, "--extensions-trusted-public-keys", out var trustedPublicKeysOverride))
        {
            trustedPublicKeys = ParseTrustedPublicKeysInline(trustedPublicKeysOverride);
        }

        if (TryGetOptionValue(args, "--extensions-trust-keys-file", out var trustKeysFileOverride)
            && !string.IsNullOrWhiteSpace(trustKeysFileOverride))
        {
            trustKeysFile = trustKeysFileOverride;
        }

        if (TryGetOptionValue(args, "--extensions-allow-external", out var allowExternalOverrideValue))
        {
            if (!TryParseBoolean(allowExternalOverrideValue, out var allowExternalOverride))
            {
                throw CreateInvalidBooleanOptionException("--extensions-allow-external", allowExternalOverrideValue);
            }

            allowExternalDirectory = allowExternalOverride;
        }

        if (TryGetOptionValue(args, "--extensions-require-hash", out var requireHashOverrideValue))
        {
            if (!TryParseBoolean(requireHashOverrideValue, out var requireHashOverride))
            {
                throw CreateInvalidBooleanOptionException("--extensions-require-hash", requireHashOverrideValue);
            }

            requireAssemblyHash = requireHashOverride;
        }

        if (TryGetOptionValue(args, "--extensions-enforce-provider-permissions", out var enforcePermissionsOverrideValue))
        {
            if (!TryParseBoolean(enforcePermissionsOverrideValue, out var enforcePermissionsOverride))
            {
                throw CreateInvalidBooleanOptionException("--extensions-enforce-provider-permissions", enforcePermissionsOverrideValue);
            }

            enforceProviderPermissions = enforcePermissionsOverride;
        }

        if (TryGetOptionValue(args, "--extensions-require-signature", out var requireSignatureOverrideValue))
        {
            if (!TryParseBoolean(requireSignatureOverrideValue, out var requireSignatureOverride))
            {
                throw CreateInvalidBooleanOptionException("--extensions-require-signature", requireSignatureOverrideValue);
            }

            requireManifestSignature = requireSignatureOverride;
        }

        if (TryGetOptionValue(args, "--extensions-require-process-isolation", out var requireProcessIsolationOverrideValue))
        {
            if (!TryParseBoolean(requireProcessIsolationOverrideValue, out var requireProcessIsolationOverride))
            {
                throw CreateInvalidBooleanOptionException("--extensions-require-process-isolation", requireProcessIsolationOverrideValue);
            }

            requireProcessIsolation = requireProcessIsolationOverride;
        }

        if (TryGetOptionValue(args, "--extensions-max-io-capability", out var maxIoCapabilityOverride))
        {
            maxIoCapability = ResolveIoCapability(maxIoCapabilityOverride, "--extensions-max-io-capability")
                ?? maxIoCapability;
        }

        if (TryGetOptionValue(args, "--extensions-max-network-capability", out var maxNetworkCapabilityOverride))
        {
            maxNetworkCapability = ResolveNetworkCapability(maxNetworkCapabilityOverride, "--extensions-max-network-capability")
                ?? maxNetworkCapability;
        }

        if (TryGetOptionValue(args, "--extensions-load-log-file", out var loadLogFileOverride)
            && !string.IsNullOrWhiteSpace(loadLogFileOverride))
        {
            loadLogFile = loadLogFileOverride;
        }

        if (TryGetOptionValue(args, "--extensions-load-event-retention", out var loadEventRetentionOverride))
        {
            if (!int.TryParse(loadEventRetentionOverride, out var parsedRetention))
            {
                throw new InvalidOperationException(
                    $"Invalid value '{loadEventRetentionOverride}' for '--extensions-load-event-retention'. " +
                    "Expected an integer.");
            }

            loadEventRetention = parsedRetention;
        }

        if (TryGetOptionValue(args, "--extensions-provider-log-file", out var providerLogFileOverride)
            && !string.IsNullOrWhiteSpace(providerLogFileOverride))
        {
            providerLogFile = providerLogFileOverride;
        }

        if (TryGetOptionValue(args, "--extensions-provider-event-retention", out var providerEventRetentionOverride))
        {
            if (!int.TryParse(providerEventRetentionOverride, out var parsedProviderRetention))
            {
                throw new InvalidOperationException(
                    $"Invalid value '{providerEventRetentionOverride}' for '--extensions-provider-event-retention'. " +
                    "Expected an integer.");
            }

            providerEventRetention = parsedProviderRetention;
        }

        var extensionDirectoryPath = Path.IsPathRooted(directory)
            ? Path.GetFullPath(directory)
            : Path.GetFullPath(Path.Combine(normalizedRoot, directory));
        if (!allowExternalDirectory && !IsPathInsideRoot(normalizedRoot, extensionDirectoryPath))
        {
            throw new InvalidOperationException(
                $"Extensions directory '{extensionDirectoryPath}' must be inside root directory '{normalizedRoot}'. " +
                "Set '--extensions-allow-external=true' to opt in.");
        }

        trustedPublicKeys = MergeTrustedPublicKeys(
            trustedPublicKeys,
            LoadTrustedPublicKeysFromFile(normalizedRoot, trustKeysFile));

        var loadLogFilePath = ResolveOptionalPath(normalizedRoot, loadLogFile);
        var providerLogFilePath = ResolveOptionalPath(normalizedRoot, providerLogFile);

        return new ExtensionHostOptions
        {
            RootDirectory = normalizedRoot,
            Enabled = enabled,
            ExtensionsDirectory = extensionDirectoryPath,
            AllowExternalDirectory = allowExternalDirectory,
            DisabledExtensionIds = disabledIds,
            TrustedExtensionIds = trustedIds,
            TrustedPublicKeys = trustedPublicKeys,
            RequireAssemblyHash = requireAssemblyHash,
            EnforceProviderPermissions = enforceProviderPermissions,
            RequireManifestSignature = requireManifestSignature,
            RequireProcessIsolation = requireProcessIsolation,
            MaxIoCapability = maxIoCapability,
            MaxNetworkCapability = maxNetworkCapability,
            LoadLogFilePath = loadLogFilePath,
            LoadEventRetention = Math.Clamp(loadEventRetention, 0, 10_000),
            ProviderLogFilePath = providerLogFilePath,
            ProviderEventRetention = Math.Clamp(providerEventRetention, 0, 100_000)
        };
    }

    private static bool TryGetOptionValue(
        string[] args,
        string optionName,
        out string value)
    {
        foreach (var arg in args)
        {
            if (arg.StartsWith(optionName + "=", StringComparison.OrdinalIgnoreCase))
            {
                value = arg[(optionName.Length + 1)..];
                return true;
            }
        }

        value = string.Empty;
        return false;
    }

    private static bool TryParseBoolean(string value, out bool result)
    {
        if (bool.TryParse(value, out result))
        {
            return true;
        }

        result = value.Trim().ToLowerInvariant() switch
        {
            "1" or "yes" or "on" => true,
            "0" or "no" or "off" => false,
            _ => default
        };

        return value.Trim().ToLowerInvariant() is "1" or "yes" or "on" or "0" or "no" or "off";
    }

    private static IReadOnlySet<string> ToSet(IEnumerable<string>? values)
    {
        var set = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        if (values is null)
        {
            return set;
        }

        foreach (var value in values)
        {
            if (!string.IsNullOrWhiteSpace(value))
            {
                set.Add(value.Trim());
            }
        }

        return set;
    }

    private static IReadOnlyDictionary<string, string> ToDictionary(IReadOnlyDictionary<string, string>? values)
    {
        var map = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        if (values is null)
        {
            return map;
        }

        foreach (var pair in values)
        {
            var key = pair.Key?.Trim();
            var value = pair.Value?.Trim();
            if (string.IsNullOrWhiteSpace(key) || string.IsNullOrWhiteSpace(value))
            {
                continue;
            }

            map[key] = value;
        }

        return map;
    }

    private static IEnumerable<string> ParseList(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            yield break;
        }

        foreach (var item in value.Split([',', ';'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            if (!string.IsNullOrWhiteSpace(item))
            {
                yield return item;
            }
        }
    }

    private static bool IsPathInsideRoot(string rootDirectory, string candidatePath)
    {
        var relativePath = Path.GetRelativePath(rootDirectory, candidatePath);
        return !string.IsNullOrWhiteSpace(relativePath)
            && !relativePath.StartsWith("..", StringComparison.Ordinal)
            && !Path.IsPathRooted(relativePath);
    }

    private static IReadOnlyDictionary<string, string> ParseTrustedPublicKeysInline(string? value)
    {
        var map = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        if (string.IsNullOrWhiteSpace(value))
        {
            return map;
        }

        foreach (var entry in value.Split([';', ','], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            var separatorIndex = entry.IndexOf(':');
            if (separatorIndex <= 0 || separatorIndex == entry.Length - 1)
            {
                continue;
            }

            var keyId = entry[..separatorIndex].Trim();
            var publicKey = entry[(separatorIndex + 1)..].Trim();
            if (string.IsNullOrWhiteSpace(keyId) || string.IsNullOrWhiteSpace(publicKey))
            {
                continue;
            }

            map[keyId] = publicKey;
        }

        return map;
    }

    private static IReadOnlyDictionary<string, string> MergeTrustedPublicKeys(
        IReadOnlyDictionary<string, string> primary,
        IReadOnlyDictionary<string, string> secondary)
    {
        var merged = new Dictionary<string, string>(secondary, StringComparer.OrdinalIgnoreCase);
        foreach (var pair in primary)
        {
            merged[pair.Key] = pair.Value;
        }

        return merged;
    }

    private static IReadOnlyDictionary<string, string> LoadTrustedPublicKeysFromFile(
        string rootDirectory,
        string? trustKeysFile)
    {
        if (string.IsNullOrWhiteSpace(trustKeysFile))
        {
            return new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        }

        var trustFilePath = Path.IsPathRooted(trustKeysFile)
            ? Path.GetFullPath(trustKeysFile)
            : Path.GetFullPath(Path.Combine(rootDirectory, trustKeysFile));
        if (!File.Exists(trustFilePath))
        {
            throw new InvalidOperationException($"Trusted keys file '{trustFilePath}' does not exist.");
        }

        try
        {
            var parsed = JsonSerializer.Deserialize<Dictionary<string, string>>(
                File.ReadAllText(trustFilePath),
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
            return ToDictionary(parsed);
        }
        catch (JsonException exception)
        {
            throw new InvalidOperationException(
                $"Failed to parse trusted keys file '{trustFilePath}': {exception.Message}",
                exception);
        }
        catch (IOException exception)
        {
            throw new InvalidOperationException(
                $"Failed to parse trusted keys file '{trustFilePath}': {exception.Message}",
                exception);
        }
        catch (UnauthorizedAccessException exception)
        {
            throw new InvalidOperationException(
                $"Failed to parse trusted keys file '{trustFilePath}': {exception.Message}",
                exception);
        }
        catch (NotSupportedException exception)
        {
            throw new InvalidOperationException(
                $"Failed to parse trusted keys file '{trustFilePath}': {exception.Message}",
                exception);
        }
    }

    private static string? ResolveOptionalPath(string rootDirectory, string? configuredPath)
    {
        if (string.IsNullOrWhiteSpace(configuredPath))
        {
            return null;
        }

        return Path.IsPathRooted(configuredPath)
            ? Path.GetFullPath(configuredPath)
            : Path.GetFullPath(Path.Combine(rootDirectory, configuredPath));
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

    private static string? ResolveIoCapability(string? raw, string source)
    {
        if (string.IsNullOrWhiteSpace(raw))
        {
            return null;
        }

        var normalized = NormalizeIoCapability(raw);
        if (normalized is null)
        {
            throw new InvalidOperationException(
                $"Invalid value '{raw}' for '{source}'. Expected one of: none, read, readWrite.");
        }

        return normalized;
    }

    private static string? ResolveNetworkCapability(string? raw, string source)
    {
        if (string.IsNullOrWhiteSpace(raw))
        {
            return null;
        }

        var normalized = NormalizeNetworkCapability(raw);
        if (normalized is null)
        {
            throw new InvalidOperationException(
                $"Invalid value '{raw}' for '{source}'. Expected one of: none, loopback, internet.");
        }

        return normalized;
    }

    private static InvalidOperationException CreateInvalidBooleanOptionException(
        string optionName,
        string providedValue)
    {
        return new InvalidOperationException(
            $"Invalid value '{providedValue}' for '{optionName}'. " +
            "Expected boolean value: true/false/1/0/yes/no/on/off.");
    }
}
