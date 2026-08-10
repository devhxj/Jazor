using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace Jazor.AspNetCore.Dev;

internal sealed class JazorDevelopmentReloadOptionsValidator : IValidateOptions<JazorDevelopmentReloadOptions>
{
    public ValidateOptionsResult Validate(string? name, JazorDevelopmentReloadOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        var failures = new List<string>();
        ValidatePath(options.ClientScriptPath, nameof(options.ClientScriptPath), failures);
        ValidatePath(options.WebSocketPath, nameof(options.WebSocketPath), failures);

        if (string.Equals(options.ClientScriptPath.Value, options.WebSocketPath.Value, StringComparison.Ordinal))
        {
            failures.Add("Jazor development reload client script path and WebSocket path must be different.");
        }

        if (options.FileChangeDebounceInterval <= TimeSpan.Zero)
        {
            failures.Add("Jazor development reload debounce interval must be greater than zero.");
        }

        if (options.FileChangePollingInterval <= TimeSpan.Zero)
        {
            failures.Add("Jazor development reload polling interval must be greater than zero.");
        }

        if (options.WebSocketKeepAliveInterval <= TimeSpan.Zero)
        {
            failures.Add("Jazor development reload WebSocket keep-alive interval must be greater than zero.");
        }

        if (options.WatchRootPaths.Any(static path => string.IsNullOrWhiteSpace(path)))
        {
            failures.Add("Jazor development reload watch root paths cannot contain null, empty, or whitespace entries.");
        }

        foreach (var mapping in options.HmrModuleMappings)
        {
            if (mapping is null)
            {
                failures.Add("Jazor development reload HMR module mappings cannot contain null entries.");
                continue;
            }

            if (string.IsNullOrWhiteSpace(mapping.ArtifactRootPath))
            {
                failures.Add("Jazor development reload HMR artifact root paths cannot be null, empty, or whitespace.");
            }

            ValidatePath(mapping.RequestPath, "HmrModuleMappings.RequestPath", failures);
        }

        return failures.Count == 0
            ? ValidateOptionsResult.Success
            : ValidateOptionsResult.Fail(failures);
    }

    private static void ValidatePath(PathString path, string optionName, List<string> failures)
    {
        if (!path.HasValue || string.IsNullOrWhiteSpace(path.Value))
        {
            failures.Add($"Jazor development reload {optionName} is required.");
            return;
        }

        if (!path.Value.StartsWith('/'))
        {
            failures.Add($"Jazor development reload {optionName} must start with '/'.");
        }

        if (string.Equals(path.Value, "/", StringComparison.Ordinal))
        {
            failures.Add($"Jazor development reload {optionName} cannot be the application root.");
        }
    }
}
