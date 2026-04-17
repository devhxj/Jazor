using Jazor.VueHost.DevServer;

namespace Jazor.VueHost.Extensions;

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

        if (TryGetOptionValue(args, "--extensions-enabled", out var enabledValue)
            && TryParseBoolean(enabledValue, out var enabledOverride))
        {
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

        if (TryGetOptionValue(args, "--extensions-allow-external", out var allowExternalOverrideValue)
            && TryParseBoolean(allowExternalOverrideValue, out var allowExternalOverride))
        {
            allowExternalDirectory = allowExternalOverride;
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

        return new ExtensionHostOptions
        {
            RootDirectory = normalizedRoot,
            Enabled = enabled,
            ExtensionsDirectory = extensionDirectoryPath,
            AllowExternalDirectory = allowExternalDirectory,
            DisabledExtensionIds = disabledIds
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
}
