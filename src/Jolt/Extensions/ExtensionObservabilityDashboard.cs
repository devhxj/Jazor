namespace Jolt.Extensions;

internal sealed record ExtensionObservabilityDashboard(
    IReadOnlyList<ExtensionLoadHealth> LoadHealth,
    IReadOnlyList<ExtensionProviderHealth> ProviderHealth,
    IReadOnlyList<ExtensionLoadInvocation> RecentLoadEvents,
    IReadOnlyList<ExtensionProviderInvocationSnapshot> RecentProviderEvents,
    DateTimeOffset GeneratedAt);
