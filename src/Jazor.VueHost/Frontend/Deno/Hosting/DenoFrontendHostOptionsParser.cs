namespace Jazor.VueHost.Frontend.Deno.Hosting;

internal static class DenoFrontendHostOptionsParser
{
    private const string EnableEnvironmentVariable = "JAZOR_VUEHOST_DENO_ENABLE";
    private const string CommandEnvironmentVariable = "JAZOR_VUEHOST_DENO_COMMAND";
    private const string WorkingDirectoryEnvironmentVariable = "JAZOR_VUEHOST_DENO_WORKDIR";
    private const string ArgumentsEnvironmentVariable = "JAZOR_VUEHOST_DENO_ARGS";

    public static DenoFrontendHostOptions Parse(string[] args)
    {
        ArgumentNullException.ThrowIfNull(args);

        var enabled = ReadBoolean(Environment.GetEnvironmentVariable(EnableEnvironmentVariable));
        var command = Environment.GetEnvironmentVariable(CommandEnvironmentVariable);
        var workingDirectory = Environment.GetEnvironmentVariable(WorkingDirectoryEnvironmentVariable);
        var arguments = ParseArgumentList(Environment.GetEnvironmentVariable(ArgumentsEnvironmentVariable)).ToList();

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
                command = commandValue;
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
            arguments.AddRange(CreateDefaultArguments());
        }

        return new DenoFrontendHostOptions
        {
            Enabled = enabled,
            Command = string.IsNullOrWhiteSpace(command) ? "deno" : command,
            Arguments = arguments.ToArray(),
            WorkingDirectory = string.IsNullOrWhiteSpace(workingDirectory) ? null : workingDirectory,
            IgnoreStartupFailure = true
        };
    }

    private static IEnumerable<string> CreateDefaultArguments()
    {
        var workerPath = Path.GetFullPath(Path.Combine(
            AppContext.BaseDirectory,
            "..",
            "..",
            "..",
            "Frontend",
            "Deno",
            "Worker",
            "frontend-worker.ts"));

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
