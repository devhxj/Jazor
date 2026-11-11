using System.Reflection;
using System.Security.Cryptography;
using System.Text.Json;

namespace Jazor.NodeHost;

internal static class NodeToolchain
{
    private static readonly string BridgeSource = ReadBridge();
    private static readonly string BridgeHash = Convert.ToHexString(SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(BridgeSource)));

    public static async Task<string> RestoreAsync(string projectDirectory, string storeDirectory,
        CancellationToken cancellationToken, TextWriter? progress = null)
    {
        var bootstrap = await PnpmBootstrap.EnsureAsync(storeDirectory, cancellationToken);
        cancellationToken.ThrowIfCancellationRequested();
        using var environment = new NodeEnvironment(projectDirectory, progress);
        var result = await environment.InvokeAsync(MaterializeBridge(projectDirectory), "restore",
            JsonSerializer.Serialize(new { projectDirectory, storeDirectory, bootstrap, engineVersion = PnpmBootstrap.Version }));
        progress?.WriteLine("[Jazor NodeApi] " + result);
        cancellationToken.ThrowIfCancellationRequested();
        return result;
    }

    public static string MaterializeBridge(string projectDirectory)
    {
        var root = Path.Combine(projectDirectory, ".jazor", "nodeapi", BridgeHash);
        Directory.CreateDirectory(root);
        var path = Path.Combine(root, "toolchain.cjs");
        if (!File.Exists(path))
            File.WriteAllText(path, BridgeSource);
        return path;
    }

    public static Task<string> RenderAsync(NodeEnvironment environment, string projectDirectory,
        string request, CancellationToken cancellationToken)
        => environment.InvokeSourceAsync(BridgeSource,
            Path.Combine(projectDirectory, ".jazor", "nodeapi", BridgeHash, "toolchain.cjs"),
            "render", cancellationToken, request);

    public static async Task<string> BuildAsync(string projectDirectory, bool ssr,
        CancellationToken cancellationToken, TextWriter? progress = null)
    {
        cancellationToken.ThrowIfCancellationRequested();
        using var environment = new NodeEnvironment(projectDirectory, progress);
        var result = await environment.InvokeAsync(MaterializeBridge(projectDirectory), "build",
            JsonSerializer.Serialize(new { projectDirectory, ssr }));
        progress?.WriteLine("[Jazor NodeApi] " + result);
        cancellationToken.ThrowIfCancellationRequested();
        return result;
    }

    public static async Task<string> CheckAsync(string projectDirectory, IReadOnlyList<string> entryPaths,
        CancellationToken cancellationToken, TextWriter? progress = null)
    {
        cancellationToken.ThrowIfCancellationRequested();
        using var environment = new NodeEnvironment(projectDirectory, progress);
        var result = await environment.InvokeAsync(MaterializeBridge(projectDirectory), "check",
            JsonSerializer.Serialize(new { projectDirectory, entryPaths }));
        progress?.WriteLine("[Jazor NodeApi] " + result);
        cancellationToken.ThrowIfCancellationRequested();
        return result;
    }

    private static string ReadBridge()
    {
        using var stream = typeof(NodeToolchain).Assembly.GetManifestResourceStream("Jazor.NodeHost.toolchain.cjs")!;
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }
}
