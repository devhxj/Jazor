using System.Diagnostics;
using Jazor.Vue;
using Jazor.VueHost.DevServer;
using Jazor.VueHost.Frontend.Deno.Hosting;

namespace Jazor.VueHost.Build;

/// <summary>
/// Orchestrates the production build pipeline.
/// Coordinates compilation, esbuild bundling, and post-processing.
/// </summary>
internal sealed class BuildOrchestrator
{
    /// <summary>
    /// Executes a production build.
    /// </summary>
    public async Task<BuildResult> BuildAsync(
        BuildOptions options,
        CancellationToken cancellationToken = default)
    {
        var stopwatch = Stopwatch.StartNew();
        var diagnostics = new List<BuildDiagnostic>();

        try
        {
            // 1. Create build context
            using var context = new BuildContext(options, cancellationToken);

            // 2. Ensure output directory exists and is clean
            if (Directory.Exists(context.OutDirectory))
            {
                Directory.Delete(context.OutDirectory, recursive: true);
            }

            Directory.CreateDirectory(context.OutDirectory);

            // 3. Start Deno frontend host for Vue SFC compilation
            await using var denoHost = CreateDenoHost(options.RootDirectory);
            await denoHost.StartAsync(cancellationToken);

            try
            {
                // 4. Create the compilation pipeline
                var moduleResolver = new ModuleResolver(options.RootDirectory);
                var frontendCompiler = new DenoFrontendModuleCompiler(denoHost);
                var compiler = new OnDemandCompiler(
                    new JazorVueParser(),
                    new JazorVueCompiler(),
                    frontendCompiler,
                    new CompilationCache(),
                    new DependencyGraph(moduleResolver),
                    moduleResolver);

                // 5. Start build server
                var buildServer = new BuildServer(context, compiler);
                await buildServer.StartAsync(cancellationToken);
                context.BuildServerPort = buildServer.Port;

                try
                {
                    // 6. Generate esbuild plugin
                    var pluginGenerator = new EsbuildPluginGenerator(context);
                    var pluginPath = await pluginGenerator.GenerateAsync();

                    // 7. Run esbuild
                    var esbuildRunner = new EsbuildRunner(context);
                    var esbuildResult = await esbuildRunner.RunAsync(pluginPath, cancellationToken);

                    if (!esbuildResult.Success)
                    {
                        stopwatch.Stop();
                        return new BuildResult
                        {
                            Success = false,
                            Diagnostics = esbuildResult.Errors,
                            Duration = stopwatch.Elapsed
                        };
                    }

                    // 8. Copy static assets from public/
                    var staticAssetHandler = new StaticAssetHandler(context);
                    var staticAssets = await staticAssetHandler.CopyPublicAssetsAsync(cancellationToken);

                    // 9. Process esbuild output
                    var assetProcessor = new AssetProcessor(context);
                    var assets = await assetProcessor.ProcessAsync(esbuildResult, cancellationToken);

                    // 10. Generate production index.html
                    await GenerateHtmlAsync(context, assets, cancellationToken);

                    stopwatch.Stop();

                    return new BuildResult
                    {
                        Success = true,
                        OutDirectory = context.OutDirectory,
                        Chunks = assets.Chunks,
                        CssAssets = assets.CssAssets,
                        StaticAssets = staticAssets,
                        Diagnostics = [.. diagnostics, .. context.Diagnostics],
                        Duration = stopwatch.Elapsed,
                        TotalSize = assets.TotalSize
                    };
                }
                finally
                {
                    await buildServer.StopAsync();
                }
            }
            finally
            {
                await denoHost.StopAsync(cancellationToken);
            }
        }
        catch (OperationCanceledException)
        {
            stopwatch.Stop();
            return new BuildResult
            {
                Success = false,
                Diagnostics = [new BuildDiagnostic
                {
                    Severity = DiagnosticSeverity.Error,
                    Message = "Build was cancelled."
                }],
                Duration = stopwatch.Elapsed
            };
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            return new BuildResult
            {
                Success = false,
                Diagnostics = [new BuildDiagnostic
                {
                    Severity = DiagnosticSeverity.Error,
                    Message = ex.Message
                }],
                Duration = stopwatch.Elapsed
            };
        }
    }

    /// <summary>
    /// Generates the production index.html in the output directory.
    /// </summary>
    private static async Task GenerateHtmlAsync(
        BuildContext context,
        ProcessedAssets assets,
        CancellationToken cancellationToken)
    {
        var htmlPath = Path.Combine(context.RootDirectory, "index.html");
        if (!File.Exists(htmlPath))
        {
            context.Diagnostics.Add(new BuildDiagnostic
            {
                Severity = DiagnosticSeverity.Warning,
                Message = "index.html not found in project root. Skipping HTML generation."
            });
            return;
        }

        var html = await File.ReadAllTextAsync(htmlPath, cancellationToken);

        // Remove dev-mode script references
        html = HtmlTransformer.RemoveDevScriptRefs(html);

        // Inject production CSS links
        foreach (var css in assets.CssAssets)
        {
            html = HtmlTransformer.InjectCss(html, "/" + css.FilePath);
        }

        // Inject production script (first entry chunk)
        var entryChunk = assets.Chunks.FirstOrDefault(c => c.IsEntry)
            ?? assets.Chunks.FirstOrDefault();

        if (entryChunk is not null)
        {
            html = HtmlTransformer.InjectScript(html, "/" + entryChunk.FilePath);
        }
        else
        {
            context.Diagnostics.Add(new BuildDiagnostic
            {
                Severity = DiagnosticSeverity.Warning,
                Message = "No entry chunk found in esbuild output."
            });
        }

        // Write to dist/index.html
        var outPath = Path.Combine(context.OutDirectory, "index.html");
        await File.WriteAllTextAsync(outPath, html, cancellationToken);
    }

    /// <summary>
    /// Creates a DenoVolarHost for the build pipeline.
    /// </summary>
    private static DenoVolarHost CreateDenoHost(string rootDirectory)
    {
        var options = new DenoVolarHostOptions
        {
            Enabled = true,
            ExecutablePath = DenoRuntimeAssetResolver.ResolveBundledExecutablePath(),
            WorkerScriptPath = DenoRuntimeAssetResolver.ResolveWorkerPath(),
            CacheDirectory = DenoRuntimeAssetResolver.ResolveCacheDirectory(),
            WorkingDirectory = DenoRuntimeAssetResolver.ResolveWorkingDirectory(
                null,
                DenoRuntimeAssetResolver.ResolveWorkerPath()),
            IgnoreStartupFailure = false
        };

        return new DenoVolarHost(options);
    }
}
