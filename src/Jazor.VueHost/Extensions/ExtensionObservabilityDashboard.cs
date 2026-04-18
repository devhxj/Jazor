namespace Jazor.VueHost.Extensions;

internal sealed record ExtensionObservabilityDashboard(
    IReadOnlyList<ExtensionLoadHealth> LoadHealth,
    IReadOnlyList<ExtensionProviderHealth> ProviderHealth,
    IReadOnlyList<ExtensionLoadInvocation> RecentLoadEvents,
    DateTimeOffset GeneratedAt);
