using System.Collections.Concurrent;
using Jazor.VueHost.DevServer;

namespace Jazor.VueHost.Build;

internal sealed class BuildContext : IDisposable
{
    public string RootDirectory { get; }
    public string OutDirectory { get; }
    public BuildOptions Options { get; }

    public ConcurrentDictionary<string, CompilationArtifact> CompilationCache { get; } = new();

    public DependencyGraph DependencyGraph { get; }

    public List<BuildDiagnostic> Diagnostics { get; } = [];

    public int BuildServerPort { get; set; }

    public CancellationToken CancellationToken { get; }

    public BuildContext(BuildOptions options, CancellationToken cancellationToken = default)
    {
        Options = options;
        RootDirectory = options.RootDirectory;
        OutDirectory = Path.Combine(options.RootDirectory, options.OutDir);
        CancellationToken = cancellationToken;
        DependencyGraph = new DependencyGraph();
    }

    public void Dispose()
    {
        // Cleanup temporary files if needed
    }
}
