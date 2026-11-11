using Acornima;
using Jazor.NodeHost;

namespace Jazor.Emit;

internal static class NodePackageRestorer
{
    public static async Task RestoreAndCheckAsync(string workspaceRoot,
        IReadOnlyList<string> entryPaths, CancellationToken cancellationToken, TextWriter? progress = null,
        string? storeDirectory = null)
    {
        await NodeToolchain.RestoreAsync(workspaceRoot,
            storeDirectory ?? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Jazor", "pnpm-store"),
            cancellationToken, progress).ConfigureAwait(false);

        // Syntax checks must never execute module initializers. Package graph
        // resolution stays with Vite; authored C# semantics stay with Roslyn.
        foreach (var path in entryPaths.Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal))
        {
            cancellationToken.ThrowIfCancellationRequested();
            new Parser().ParseModule(await File.ReadAllTextAsync(path, cancellationToken).ConfigureAwait(false));
        }
        progress?.WriteLine($"[Jazor NodeApi] syntax: checked {entryPaths.Count} authored modules.");
        try
        {
            await NodeToolchain.CheckAsync(workspaceRoot, entryPaths, cancellationToken, progress).ConfigureAwait(false);
        }
        catch (Microsoft.JavaScript.NodeApi.JSException exception)
        {
            throw new InvalidOperationException("Jazor module dependency check failed. " + exception.Message, exception);
        }
    }
}
