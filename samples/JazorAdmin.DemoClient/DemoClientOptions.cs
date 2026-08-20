namespace JazorAdmin.DemoClient;

public sealed class DemoClientOptions
{
    public const string SectionName = "JazorAdminDemo";

    public string Authority { get; init; } = string.Empty;

    public string ClientId { get; init; } = "jazoradmin-demo-client";

    public string ClientSecret { get; init; } = string.Empty;
}
