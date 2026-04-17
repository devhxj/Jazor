namespace Jazor.VueHost.Extensions;

internal sealed record ExtensionProviderInvocation(
    string ProviderName,
    string Capability,
    TimeSpan Duration,
    bool Succeeded,
    bool TimedOut,
    bool Skipped,
    string? ErrorMessage);

internal sealed record ExtensionProviderHealth(
    string ProviderName,
    string Capability,
    int SuccessCount,
    int FailureCount,
    int TimeoutCount,
    int SkippedCount,
    TimeSpan LastDuration,
    DateTimeOffset? LastSuccessAt,
    DateTimeOffset? LastFailureAt,
    string? LastErrorMessage);
