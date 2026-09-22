namespace Jazor.Emit;

/// <summary>Runs the project's build script using its restored Deno package graph.</summary>
internal sealed class JavaScriptProjectBuilder
{
    /// <remarks>执行统一走 <see cref="DenoPackageRestorer.RunAsync"/> 的 DenoHost DenoProcess；
    /// <paramref name="denoExecutablePath"/> 保留为兼容入口，仅做存在性校验。</remarks>
    public async Task<ToolchainResult> BuildAsync(
        string projectRoot,
        string? denoExecutablePath = null,
        CancellationToken cancellationToken = default)
    {
        var result = await DenoPackageRestorer.RunAsync(
            DenoPackageRestorer.ResolveExecutable(denoExecutablePath),
            projectRoot,
            ["task", "build"],
            cancellationToken).ConfigureAwait(false);
        return result.Succeeded
            ? ToolchainResult.Success(0)
            : ToolchainResult.Fail(result.ExitCode, "JAZOR_TOOLCHAIN_BUILD_FAILED",
                (result.StandardError + Environment.NewLine + result.StandardOutput).Trim());
    }
}
