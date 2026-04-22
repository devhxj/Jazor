namespace Jolt.Extensions;

internal static class ExtensionWorkerMethodNames
{
    public const string Bootstrap = "extension/bootstrap";
    public const string Invoke = "extension/invoke";
    public const string Shutdown = "extension/shutdown";
}

internal static class ExtensionCapabilityNames
{
    public const string Diagnostic = "diagnostic";
    public const string CodeAction = "codeAction";
    public const string Hover = "hover";
    public const string Completion = "completion";
    public const string DocumentSymbol = "documentSymbol";
    public const string SignatureHelp = "signatureHelp";
    public const string InlayHint = "inlayHint";
    public const string WorkspaceSymbol = "workspaceSymbol";
    public const string FoldingRange = "foldingRange";
    public const string References = "references";
    public const string Rename = "rename";
}

internal static class ExtensionWorkerErrorCodes
{
    public const string InvalidRequest = "invalid_request";
    public const string InvalidParams = "invalid_params";
    public const string NotBootstrapped = "not_bootstrapped";
    public const string UnsupportedMethod = "unsupported_method";
    public const string UnsupportedCapability = "unsupported_capability";
    public const string ProviderNotImplemented = "provider_not_implemented";
    public const string SandboxViolation = "sandbox_violation";
    public const string InternalError = "internal_error";
}

internal sealed record ExtensionWorkerRequestEnvelope(
    int Id,
    string Method,
    object? Params);

internal sealed record ExtensionWorkerResponseEnvelope(
    int Id,
    object? Result,
    ExtensionWorkerError? Error);

internal sealed record ExtensionWorkerError(
    string Code,
    string Message);

internal sealed record ExtensionWorkerBootstrapRequest(
    string RootDirectory,
    string ExtensionDirectory,
    string AssemblyPath,
    string ExtensionTypeName,
    IReadOnlyDictionary<string, string>? Settings,
    ExtensionSandboxProfile? SandboxProfile);

internal static class ExtensionWorkerHostSettingNames
{
    public const string BootstrapTimeoutMilliseconds = "__joltHost.bootstrapTimeoutMs";
    public const string InvokeTimeoutMilliseconds = "__joltHost.invokeTimeoutMs";
    public const string WorkerRestartWindowMilliseconds = "__joltHost.workerRestartWindowMs";
    public const string WorkerMaxRestarts = "__joltHost.workerMaxRestarts";
    public const string WorkerRestartBaseDelayMilliseconds = "__joltHost.workerRestartBaseDelayMs";
}

internal static class ExtensionWorkerHostSettingResolver
{
    public static TimeSpan ResolvePositiveDurationFromMilliseconds(
        IReadOnlyDictionary<string, string>? settings,
        string settingName,
        string environmentVariableName,
        TimeSpan defaultValue)
    {
        if (TryGetPositiveInt32(settings, settingName, out var milliseconds))
        {
            return TimeSpan.FromMilliseconds(milliseconds);
        }

        var configuredValue = Environment.GetEnvironmentVariable(environmentVariableName);
        return int.TryParse(configuredValue, out var environmentMilliseconds) && environmentMilliseconds > 0
            ? TimeSpan.FromMilliseconds(environmentMilliseconds)
            : defaultValue;
    }

    public static int ResolvePositiveInt32(
        IReadOnlyDictionary<string, string>? settings,
        string settingName,
        string environmentVariableName,
        int defaultValue)
    {
        if (TryGetPositiveInt32(settings, settingName, out var parsed))
        {
            return parsed;
        }

        var configuredValue = Environment.GetEnvironmentVariable(environmentVariableName);
        return int.TryParse(configuredValue, out var environmentParsed) && environmentParsed > 0
            ? environmentParsed
            : defaultValue;
    }

    private static bool TryGetPositiveInt32(
        IReadOnlyDictionary<string, string>? settings,
        string settingName,
        out int value)
    {
        value = 0;
        if (!TryGetSettingValue(settings, settingName, out var rawValue))
        {
            return false;
        }

        return int.TryParse(rawValue, out value) && value > 0;
    }

    private static bool TryGetSettingValue(
        IReadOnlyDictionary<string, string>? settings,
        string settingName,
        out string? value)
    {
        value = null;
        if (settings is null || settings.Count == 0)
        {
            return false;
        }

        if (settings.TryGetValue(settingName, out value))
        {
            return !string.IsNullOrWhiteSpace(value);
        }

        foreach (var pair in settings)
        {
            if (!string.Equals(pair.Key, settingName, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            value = pair.Value;
            return !string.IsNullOrWhiteSpace(value);
        }

        return false;
    }
}

internal sealed record ExtensionWorkerBootstrapResponse(
    ExtensionMetadata Metadata,
    IReadOnlyList<ExtensionWorkerProviderDescriptor> Providers);

internal sealed record ExtensionWorkerProviderDescriptor(
    string Capability,
    string Name,
    int Priority);

internal sealed record ExtensionWorkerInvokeRequest(
    string Capability,
    object? Context);
