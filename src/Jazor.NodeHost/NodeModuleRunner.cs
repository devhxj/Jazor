using System.Text.Json;

namespace Jazor.NodeHost;

internal static class NodeModuleRunner
{
    public static async Task<string> RunAsync(string projectDirectory, string modulePath,
        IReadOnlyList<string>? arguments = null, CancellationToken cancellationToken = default,
        IReadOnlyDictionary<string, string>? imports = null, IReadOnlyDictionary<string, string>? embeddedModules = null)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var packagePath = Path.Combine(projectDirectory, "package.json");
        if (!File.Exists(packagePath))
            await File.WriteAllTextAsync(packagePath, "{\"private\":true,\"type\":\"module\"}", cancellationToken);
        using var environment = new NodeEnvironment(projectDirectory);
        var result = await NodeToolchain.Bridge.InvokeAsync(environment, projectDirectory, "executeModule", cancellationToken,
            JsonSerializer.Serialize(new { projectDirectory, modulePath, arguments = arguments ?? [], imports, embeddedModules }));
        cancellationToken.ThrowIfCancellationRequested();
        using var document = JsonDocument.Parse(result);
        return document.RootElement.GetProperty("output").GetString()!;
    }
}
