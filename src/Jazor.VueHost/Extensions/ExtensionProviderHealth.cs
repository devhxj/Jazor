namespace Jazor.VueHost.Extensions;

internal sealed record ExtensionProviderInvocation(
    string ProviderName,
    string Capability,
    TimeSpan Duration,
    bool Succeeded,
    bool TimedOut,
    string? ErrorMessage);

internal sealed record ExtensionProviderHealth(
    string ProviderName,
    string Capability,
    int SuccessCount,
    int FailureCount,
    int TimeoutCount,
    TimeSpan LastDuration,
    DateTimeOffset? LastSuccessAt,
    DateTimeOffset? LastFailureAt,
    string? LastErrorMessage);
