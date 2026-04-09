namespace Jazor.VueHost.Frontend.Deno.Hosting;

internal static class DenoFrontendHostOptionsParser
{
    private const string EnableEnvironmentVariable = "JAZOR_VUEHOST_DENO_ENABLE";
    private const string CommandEnvironmentVariable = "JAZOR_VUEHOST_DENO_COMMAND";
    private const string WorkingDirectoryEnvironmentVariable = "JAZOR_VUEHOST_DENO_WORKDIR";
    private const string ArgumentsEnvironmentVariable = "JAZOR_VUEHOST_DENO_ARGS";

    public static DenoFrontendHostOptions Parse(string[] args)
        => Parse(args, baseDirectory: null);

    internal static DenoFrontendHostOptions Parse(string[] args, string? baseDirectory)
    {
        ArgumentNullException.ThrowIfNull(args);

        var enabled = ReadBoolean(Environment.GetEnvironmentVariable(EnableEnvironmentVariable));
        var executableOverride = Environment.GetEnvironmentVariable(CommandEnvironmentVariable);
        var workingDirectory = Environment.GetEnvironmentVariable(WorkingDirectoryEnvironmentVariable);
        var arguments = ParseArgumentList(Environment.GetEnvironmentVariable(ArgumentsEnvironmentVariable)).ToList();
        var workerPath = DenoRuntimeAssetResolver.ResolveWorkerPath(baseDirectory);

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
            }
        }

        if (arguments.Count == 0)
        {
            arguments.AddRange(CreateDefaultArguments(workerPath));
        }

        return new DenoFrontendHostOptions
        {
            Enabled = enabled,
            ExecutablePath = string.IsNullOrWhiteSpace(executableOverride)
                ? DenoRuntimeAssetResolver.ResolveBundledExecutablePath(baseDirectory)
                : executableOverride,
            HasExplicitExecutableOverride = !string.IsNullOrWhiteSpace(executableOverride),
            WorkerScriptPath = workerPath,
            Arguments = arguments.ToArray(),
            WorkingDirectory = DenoRuntimeAssetResolver.ResolveWorkingDirectory(workingDirectory, workerPath),
            IgnoreStartupFailure = true
        };
    }

    private static IEnumerable<string> CreateDefaultArguments(string workerPath)
    {
        return
        [
            "run",
            "--quiet",
            workerPath
        ];
    }

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
}
