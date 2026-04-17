namespace Jazor.VueHost.Extensions;

internal static class ExtensionLoadStatus
{
    public const string Loaded = "loaded";

    public const string Rejected = "rejected";

    public const string Failed = "failed";
}

internal sealed record ExtensionLoadInvocation(
    string ExtensionId,
    string Source,
    string ExtensionDirectory,
    string? ManifestPath,
    string? AssemblyPath,
    string Status,
    string Reason,
    DateTimeOffset Timestamp);

internal sealed record ExtensionLoadHealth(
    string ExtensionId,
    string Source,
    int LoadedCount,
    int RejectedCount,
    int FailedCount,
    int AttemptCount,
    DateTimeOffset? LastAttemptAt,
    DateTimeOffset? LastLoadedAt,
    DateTimeOffset? LastRejectedAt,
    DateTimeOffset? LastFailedAt,
    string? LastReason,
    string? LastManifestPath,
    string? LastAssemblyPath,
    string? LastExtensionDirectory);
