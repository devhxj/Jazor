namespace Jolt.Volar.Deno.Hosting;

public sealed class DenoVolarHostOptions
{
    private static readonly TimeSpan DefaultRequestTimeout = TimeSpan.FromSeconds(15);

    public bool Enabled { get; init; }

    public string ExecutablePath { get; init; } = string.Empty;

    public bool HasExplicitExecutableOverride { get; init; }

    public string WorkerScriptPath { get; init; } = string.Empty;

    public string CacheDirectory { get; init; } = string.Empty;

    public string[] Arguments { get; init; } = [];

    public string? WorkingDirectory { get; init; }

    public bool IgnoreStartupFailure { get; init; } = true;

    public TimeSpan? RequestTimeout { get; init; } = DefaultRequestTimeout;
}
