namespace JazorAdmin.DemoClient;

public sealed record PortalApiOutcome
{
    public bool Ok { get; init; }

    public bool Unauthorized { get; init; }

    public object? Data { get; init; }

    public string? Error { get; init; }
}
