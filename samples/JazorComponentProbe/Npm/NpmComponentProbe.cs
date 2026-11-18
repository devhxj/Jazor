using Jazor.NodeHost;

namespace Jazor.ComponentProbe.Npm;

public static class NpmComponentProbe
{
    public static async Task<string> ReadAsync(string projectDirectory, string packageName, string exportName,
        CancellationToken cancellationToken = default)
    {
        projectDirectory = Path.GetFullPath(projectDirectory);
        var repository = new DirectoryInfo(Directory.GetCurrentDirectory());
        while (repository is not null && !File.Exists(Path.Combine(repository.FullName, "Jazor.slnx")))
            repository = repository.Parent;
        var store = Environment.GetEnvironmentVariable("JazorPnpmStoreDirectory") ??
            Path.Combine(repository?.FullName ?? Directory.GetCurrentDirectory(), ".tmp", "nodeapi-store");
        await NodeToolchain.RestoreAsync(projectDirectory, store, cancellationToken, Console.Error);

        // Resolve both the runtime import and its peers from this consumer.
        var module = Path.Combine(projectDirectory, ".jazor", $"component-probe-{Guid.NewGuid():N}.mjs");
        try
        {
            using var resource = typeof(NpmComponentProbe).Assembly.GetManifestResourceStream("Jazor.ComponentProbe.Npm.probe.mjs")!;
            await using (var output = File.Create(module))
                await resource.CopyToAsync(output, cancellationToken);
            return await NodeModuleRunner.RunAsync(projectDirectory, module, [packageName, exportName], cancellationToken);
        }
        finally
        {
            File.Delete(module);
        }
    }
}
