namespace Jazor.VueHost.Frontend.Deno.Hosting;

public sealed class DenoFrontendHostOptions
{
    public bool Enabled { get; init; }

    public string Command { get; init; } = "deno";

    public string[] Arguments { get; init; } = [];

    public string? WorkingDirectory { get; init; }

    public bool IgnoreStartupFailure { get; init; } = true;
}
