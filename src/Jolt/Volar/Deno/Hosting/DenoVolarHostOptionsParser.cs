namespace Jolt.Volar.Deno.Hosting;

internal static class DenoVolarHostOptionsParser
{
    private const string EnableEnvironmentVariable = "JOLT_DENO_ENABLE";
    private const string CommandEnvironmentVariable = "JOLT_DENO_COMMAND";
    private const string WorkingDirectoryEnvironmentVariable = "JOLT_DENO_WORKDIR";
    private const string ArgumentsEnvironmentVariable = "JOLT_DENO_ARGS";
    private const string RequestTimeoutEnvironmentVariable = "JOLT_DENO_REQUEST_TIMEOUT_MS";
    private static readonly string[] DefaultAllowedEnvironmentVariables =
    [
        "__MINIMATCH_TESTING_PLATFORM__",
        "BABEL_TYPES_8_BREAKING",
        "DENO_DIR",
        "JOLT_*",
        "LANG",
        "NODE_DEBUG",
        "NODE_ENV",
        "NODE_INSPECTOR_IPC",
        "NO_COLOR",
        "TSC_*",
        "VSCODE_INSPECTOR_OPTIONS",
        "VSCODE_NLS_CONFIG",
        "XDG_RUNTIME_DIR"
    ];

    public static DenoVolarHostOptions Parse(string[] args)
        => Parse(args, baseDirectory: null);

    internal static DenoVolarHostOptions Parse(string[] args, string? baseDirectory)
    {
        ArgumentNullException.ThrowIfNull(args);

        var enabled = true;
        if (ReadBoolean(Environment.GetEnvironmentVariable(EnableEnvironmentVariable)))
        {
            enabled = true;
        }
        var executableOverride = Environment.GetEnvironmentVariable(CommandEnvironmentVariable);
        var workingDirectory = Environment.GetEnvironmentVariable(WorkingDirectoryEnvironmentVariable);
        var arguments = ParseArgumentList(Environment.GetEnvironmentVariable(ArgumentsEnvironmentVariable)).ToList();
        var requestTimeout = ParseRequestTimeoutMilliseconds(
            Environment.GetEnvironmentVariable(RequestTimeoutEnvironmentVariable),
            RequestTimeoutEnvironmentVariable);
        var workerPath = DenoRuntimeAssetResolver.ResolveWorkerPath(baseDirectory);
        var cacheDirectory = DenoRuntimeAssetResolver.ResolveCacheDirectory(baseDirectory);
        var workspaceRoots = new List<string>();

        foreach (var arg in args)
        {
            if (string.Equals(arg, "--deno-worker", StringComparison.OrdinalIgnoreCase))
            {
                enabled = true;
                continue;
            }

            if (string.Equals(arg, "--no-deno-worker", StringComparison.OrdinalIgnoreCase))
            {
                enabled = false;
                continue;
            }

            if (TryGetOptionValue(arg, "--deno-command", out var commandValue))
            {
                executableOverride = commandValue;
                continue;
            }

            if (TryGetOptionValue(arg, "--deno-working-directory", out var workingDirectoryValue))
            {
                workingDirectory = workingDirectoryValue;
                continue;
            }

            if (TryGetOptionValue(arg, "--deno-arg", out var argumentValue)
                && !string.IsNullOrWhiteSpace(argumentValue))
            {
                arguments.Add(argumentValue);
                continue;
            }

            if (TryGetOptionValue(arg, "--deno-request-timeout-ms", out var requestTimeoutValue))
            {
                requestTimeout = ParseRequestTimeoutMilliseconds(requestTimeoutValue, "--deno-request-timeout-ms");
                continue;
            }

            if (TryGetOptionValue(arg, "--dev-root", out var devRootValue)
                && !string.IsNullOrWhiteSpace(devRootValue))
            {
                workspaceRoots.Add(devRootValue);
            }
        }

        var resolvedWorkingDirectory = DenoRuntimeAssetResolver.ResolveWorkingDirectory(workingDirectory, workerPath);

        if (arguments.Count == 0)
        {
            arguments.AddRange(CreateDefaultArguments(
                workerPath,
                cacheDirectory,
                resolvedWorkingDirectory,
                workspaceRoots));
        }

        return new DenoVolarHostOptions
        {
            Enabled = enabled,
            ExecutablePath = string.IsNullOrWhiteSpace(executableOverride)
                ? DenoRuntimeAssetResolver.ResolveBundledExecutablePath(baseDirectory)
                : executableOverride,
            HasExplicitExecutableOverride = !string.IsNullOrWhiteSpace(executableOverride),
            WorkerScriptPath = workerPath,
            CacheDirectory = cacheDirectory,
            Arguments = arguments.ToArray(),
            WorkingDirectory = resolvedWorkingDirectory,
            IgnoreStartupFailure = true,
            RequestTimeout = requestTimeout
        };
    }

    private static IEnumerable<string> CreateDefaultArguments(
        string workerPath,
        string cacheDirectory,
        string? workingDirectory,
        IReadOnlyList<string> workspaceRoots)
    {
        yield return "run";
        yield return "--quiet";
        if (DenoRuntimeAssetResolver.HasReadyWorkerDependencies(workerPath, cacheDirectory))
        {
            yield return "--cached-only";
        }

        yield return "--allow-env=" + string.Join(",", DefaultAllowedEnvironmentVariables);
        yield return "--allow-read=" + string.Join(
            ",",
            GetDefaultAllowedReadPaths(workerPath, cacheDirectory, workingDirectory, workspaceRoots)
                .Select(EscapePermissionPath));
        yield return workerPath;
    }

    private static IEnumerable<string> GetDefaultAllowedReadPaths(
        string workerPath,
        string cacheDirectory,
        string? workingDirectory,
        IReadOnlyList<string> workspaceRoots)
    {
        var allowedPaths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        AddAllowedPath(allowedPaths, Path.GetDirectoryName(workerPath));
        AddAllowedPath(allowedPaths, cacheDirectory);
        AddAllowedPath(allowedPaths, workingDirectory);

        foreach (var workspaceRoot in workspaceRoots)
        {
            AddAllowedPath(allowedPaths, workspaceRoot);
        }

        if (workspaceRoots.Count == 0)
        {
            AddAllowedPath(allowedPaths, Directory.GetCurrentDirectory());
        }

        return allowedPaths.Order(StringComparer.OrdinalIgnoreCase);
    }

    private static void AddAllowedPath(HashSet<string> allowedPaths, string? path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return;
        }

        allowedPaths.Add(Path.GetFullPath(path));
    }

    private static string EscapePermissionPath(string path)
        => path.Replace(",", ",,", StringComparison.Ordinal);

    private static bool TryGetOptionValue(string arg, string optionName, out string value)
    {
        var prefix = optionName + "=";
        if (arg.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
        {
            value = arg[prefix.Length..];
            return true;
        }

        value = string.Empty;
        return false;
    }

    private static bool ReadBoolean(string? value)
        => string.Equals(value, "1", StringComparison.Ordinal)
            || string.Equals(value, "true", StringComparison.OrdinalIgnoreCase)
            || string.Equals(value, "yes", StringComparison.OrdinalIgnoreCase)
            || string.Equals(value, "on", StringComparison.OrdinalIgnoreCase);

    private static IEnumerable<string> ParseArgumentList(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            yield break;
        }

        foreach (var entry in value.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            if (!string.IsNullOrWhiteSpace(entry))
            {
                yield return entry;
            }
        }
    }

    private static TimeSpan? ParseRequestTimeoutMilliseconds(string? rawValue, string source)
    {
        if (string.IsNullOrWhiteSpace(rawValue))
        {
            return DenoVolarHostOptionsDefaults.RequestTimeout;
        }

        if (!int.TryParse(rawValue, out var milliseconds))
        {
            throw new InvalidOperationException(
                $"Invalid value '{rawValue}' for '{source}'. Expected a non-negative integer timeout in milliseconds.");
        }

        if (milliseconds < 0)
        {
            throw new InvalidOperationException(
                $"Invalid value '{rawValue}' for '{source}'. Expected a non-negative integer timeout in milliseconds.");
        }

        return milliseconds == 0
            ? null
            : TimeSpan.FromMilliseconds(milliseconds);
    }

    private static class DenoVolarHostOptionsDefaults
    {
        public static TimeSpan? RequestTimeout => new DenoVolarHostOptions().RequestTimeout;
    }
}
