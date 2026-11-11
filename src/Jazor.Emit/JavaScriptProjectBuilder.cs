using Jazor.NodeHost;
using System.Text.Json;

namespace Jazor.Emit;

/// <summary>Builds the restored project through the in-process Vite API.</summary>
internal sealed class JavaScriptProjectBuilder
{
    /// <remarks>Build runs through the in-process Vite API. Shell scripts are not executed.</remarks>
    public async Task<ToolchainResult> BuildAsync(
        string projectRoot,
        string? denoExecutablePath = null,
        CancellationToken cancellationToken = default,
        string taskName = "build",
        TextWriter? progress = null)
    {
        try
        {
            if (taskName is not ("build" or "build:ssr"))
                throw new InvalidOperationException($"NodeApi build does not execute shell task '{taskName}'. Configure Vite through its project config.");
            using var package = JsonDocument.Parse(await File.ReadAllTextAsync(Path.Combine(projectRoot, "package.json"), cancellationToken));
            var script = package.RootElement.GetProperty("scripts").GetProperty(taskName).GetString();
            var expected = taskName == "build" ? "vite build" : "vite build --config " + ViteProjectWriter.SsrConfigFileName;
            if (script != expected)
                throw new InvalidOperationException($"NodeApi cannot execute authored shell script '{script}'. Move build customization into the Vite config and use '{expected}'.");
            await NodeToolchain.BuildAsync(projectRoot, taskName == "build:ssr", cancellationToken, progress).ConfigureAwait(false);
            return ToolchainResult.Success(0);
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            progress?.WriteLine("[Jazor NodeApi] build: failed. " + exception.Message);
            return ToolchainResult.Fail(1, "JAZOR_TOOLCHAIN_BUILD_FAILED", exception.ToString());
        }
    }
}
