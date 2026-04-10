namespace Jazor.VueHost.Frontend.Deno.Hosting;

public sealed class DenoVolarHostOptions
{
    public bool Enabled { get; init; }

    public string ExecutablePath { get; init; } = string.Empty;

    public bool HasExplicitExecutableOverride { get; init; }

    public string WorkerScriptPath { get; init; } = string.Empty;

    public string CacheDirectory { get; init; } = string.Empty;

    public string[] Arguments { get; init; } = [];

    public string? WorkingDirectory { get; init; }

    public bool IgnoreStartupFailure { get; init; } = true;
}
