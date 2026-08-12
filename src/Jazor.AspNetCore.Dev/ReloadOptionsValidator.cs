using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace Jazor.AspNetCore.Dev;

/// <summary>Validates reload endpoints, timings, and artifact mappings at host startup.</summary>
internal sealed class ReloadOptionsValidator : IValidateOptions<JazorReloadOptions>
{
    public ValidateOptionsResult Validate(string? name, JazorReloadOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        var failures = new List<string>();
        ValidatePath(options.ClientScriptPath, nameof(options.ClientScriptPath), failures);
        ValidatePath(options.WebSocketPath, nameof(options.WebSocketPath), failures);

        if (string.Equals(options.ClientScriptPath.Value, options.WebSocketPath.Value, StringComparison.Ordinal))
        {
            failures.Add("Jazor reload client script path and WebSocket path must be different.");
        }

        if (options.DebounceInterval <= TimeSpan.Zero)
        {
            failures.Add("Jazor reload debounce interval must be greater than zero.");
        }

        if (options.PollingInterval <= TimeSpan.Zero)
        {
            failures.Add("Jazor reload polling interval must be greater than zero.");
        }

        if (options.KeepAliveInterval <= TimeSpan.Zero)
        {
            failures.Add("Jazor reload WebSocket keep-alive interval must be greater than zero.");
        }

        if (options.WatchPaths.Any(static path => string.IsNullOrWhiteSpace(path)))
        {
            failures.Add("Jazor reload watch paths cannot contain null, empty, or whitespace entries.");
        }

        foreach (var mapping in options.HmrMappings)
        {
            if (mapping is null)
            {
                failures.Add("Jazor reload HMR mappings cannot contain null entries.");
                continue;
            }

            if (string.IsNullOrWhiteSpace(mapping.ArtifactRootPath))
            {
                failures.Add("Jazor reload HMR artifact root paths cannot be null, empty, or whitespace.");
            }

            ValidatePath(mapping.RequestPath, "HmrMappings.RequestPath", failures);
        }

        return failures.Count == 0
            ? ValidateOptionsResult.Success
            : ValidateOptionsResult.Fail(failures);
    }

    private static void ValidatePath(PathString path, string optionName, List<string> failures)
    {
        if (!path.HasValue || string.IsNullOrWhiteSpace(path.Value))
        {
            failures.Add($"Jazor reload {optionName} is required.");
            return;
        }

        if (!path.Value.StartsWith('/'))
        {
            failures.Add($"Jazor reload {optionName} must start with '/'.");
        }

        if (string.Equals(path.Value, "/", StringComparison.Ordinal))
        {
            failures.Add($"Jazor reload {optionName} cannot be the application root.");
        }
    }
}
