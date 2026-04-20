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
