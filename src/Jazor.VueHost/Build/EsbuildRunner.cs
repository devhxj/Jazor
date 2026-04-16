using System.Diagnostics;
using System.Text;
using System.Text.Json;

namespace Jazor.VueHost.Build;

/// <summary>
/// Manages the esbuild subprocess for production builds.
/// </summary>
internal sealed class EsbuildRunner
{
    private readonly BuildContext _context;

    public EsbuildRunner(BuildContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    /// <summary>
    /// Runs esbuild as a subprocess and returns the result.
    /// </summary>
    /// <param name="pluginPath">Path to the Jazor esbuild plugin.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Esbuild execution result.</returns>
    public async Task<EsbuildResult> RunAsync(string pluginPath, CancellationToken ct)
    {
        var esbuildPackageDirectory = EsbuildPackageResolver.ResolvePackageDirectory(_context.RootDirectory);
        if (esbuildPackageDirectory is null)
        {
            return new EsbuildResult
            {
                Success = false,
                Errors = new[]
                {
                    new BuildDiagnostic
                    {
                        Severity = DiagnosticSeverity.Error,
                        Message = "esbuild not found. Install it with: npm install -D esbuild"
                    }
                }
            };
        }

        string entryPointPath;
        try
        {
            entryPointPath = BuildEntryPointResolver.ResolveEntryPoint(_context.RootDirectory);
        }
        catch (InvalidOperationException ex)
        {
            return new EsbuildResult
            {
                Success = false,
                Errors = new[]
                {
                    new BuildDiagnostic
                    {
                        Severity = DiagnosticSeverity.Error,
                        Message = ex.Message
                    }
                }
            };
        }

        var configPath = await GenerateEsbuildConfigAsync(
            pluginPath,
            esbuildPackageDirectory,
            entryPointPath);

        var processStartInfo = new ProcessStartInfo
        {
            FileName = "node",
            Arguments = $"--experimental-vm-modules \"{configPath}\"",
            WorkingDirectory = _context.RootDirectory,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using var process = Process.Start(processStartInfo);
        if (process is null)
        {
            return new EsbuildResult
            {
                Success = false,
                Errors = new[]
                {
                    new BuildDiagnostic
                    {
                        Severity = DiagnosticSeverity.Error,
                        Message = "Failed to start esbuild process"
                    }
                }
            };
        }

        var stdoutBuilder = new StringBuilder();
        var stderrBuilder = new StringBuilder();

        var stdoutTask = Task.Run(() =>
        {
            while (!process.StandardOutput.EndOfStream && !ct.IsCancellationRequested)
            {
                var line = process.StandardOutput.ReadLine();
                if (line is not null)
                    stdoutBuilder.AppendLine(line);
            }
        }, ct);

        var stderrTask = Task.Run(() =>
        {
            while (!process.StandardError.EndOfStream && !ct.IsCancellationRequested)
            {
                var line = process.StandardError.ReadLine();
                if (line is not null)
                    stderrBuilder.AppendLine(line);
            }
        }, ct);

        await Task.WhenAll(stdoutTask, stderrTask);
        await process.WaitForExitAsync(ct);

        var stdout = stdoutBuilder.ToString();
        var stderr = stderrBuilder.ToString();

        // Check exit code
        if (process.ExitCode != 0)
        {
            return new EsbuildResult
            {
                Success = false,
                Errors = new[]
                {
                    new BuildDiagnostic
                    {
                        Severity = DiagnosticSeverity.Error,
                        Message = $"esbuild failed with exit code {process.ExitCode}{(string.IsNullOrEmpty(stderr) ? "" : $": {stderr}")}"
                    }
                }
            };
        }

        // Parse metafile JSON from stdout
        var metafileJson = ExtractMetafileJson(stdout);
        if (metafileJson is null)
        {
            return new EsbuildResult
            {
                Success = false,
                Errors = new[]
                {
                    new BuildDiagnostic
                    {
                        Severity = DiagnosticSeverity.Error,
                        Message = "esbuild did not output metafile JSON"
                    }
                }
            };
        }

        return new EsbuildResult
        {
            Success = true,
            MetafileJson = metafileJson
        };
    }

    private async Task<string> GenerateEsbuildConfigAsync(
        string pluginPath,
        string esbuildPackageDirectory,
        string entryPointPath)
    {
        var jazorDir = Path.Combine(_context.RootDirectory, ".jazor");
        Directory.CreateDirectory(jazorDir);

        var configPath = Path.Combine(jazorDir, "esbuild.config.mjs");

        var sourceMapOption = _context.Options.SourceMap switch
        {
            SourceMapOption.None => "false",
            SourceMapOption.Inline => "'inline'",
            SourceMapOption.External => "true",
            _ => "false"
        };

        var normalizedEntryPoint = Path.GetRelativePath(_context.RootDirectory, entryPointPath).Replace("\\", "/");
        var pluginFileUrl = new Uri(pluginPath).AbsoluteUri;
        var serializedEsbuildPackageDirectory = JsonSerializer.Serialize(esbuildPackageDirectory);

        var sb = new StringBuilder();

        sb.AppendLine("import { createRequire } from 'node:module';");
        sb.AppendLine($"import jazorPlugin from {JsonSerializer.Serialize(pluginFileUrl)};");
        sb.AppendLine("const require = createRequire(import.meta.url);");
        sb.AppendLine($"const esbuild = require({serializedEsbuildPackageDirectory});");
        sb.AppendLine();
        sb.AppendLine("try {");
        sb.AppendLine("    const result = await esbuild.build({");
        sb.AppendLine($"        entryPoints: [{JsonSerializer.Serialize(normalizedEntryPoint)}],");
        sb.AppendLine("        bundle: true,");
        sb.AppendLine($"        outdir: '{_context.Options.OutDir}',");
        sb.AppendLine($"        splitting: {_context.Options.CodeSplitting.ToString().ToLowerInvariant()},");
        sb.AppendLine("        format: 'esm',");
        sb.AppendLine($"        target: '{_context.Options.Target}',");
        sb.AppendLine($"        minify: {_context.Options.Minify.ToString().ToLowerInvariant()},");
        sb.AppendLine($"        sourcemap: {sourceMapOption},");
        sb.AppendLine("        metafile: true,");
        sb.AppendLine("        plugins: [jazorPlugin],");
        sb.AppendLine("        entryNames: 'assets/[name]-[hash]',");
        sb.AppendLine("        assetNames: 'assets/[name]-[hash]',");
        sb.AppendLine("        chunkNames: 'assets/[name]-[hash]',");
        sb.AppendLine("        publicPath: '/',");
        sb.AppendLine("        define: {");
        sb.AppendLine("            'process.env.NODE_ENV': '\"production\"'");
        sb.AppendLine("        },");
        sb.AppendLine("        logLevel: 'error'");
        sb.AppendLine("    });");
        sb.AppendLine();
        sb.AppendLine("    // Output metafile JSON to stdout for parsing");
        sb.AppendLine("    console.log('__JAZOR_METAFILE_START__');");
        sb.AppendLine("    console.log(JSON.stringify(result.metafile));");
        sb.AppendLine("    console.log('__JAZOR_METAFILE_END__');");
        sb.AppendLine("} catch (error) {");
        sb.AppendLine("    console.error(error);");
        sb.AppendLine("    process.exit(1);");
        sb.AppendLine("}");

        var configContent = sb.ToString();
        await File.WriteAllTextAsync(configPath, configContent, _context.CancellationToken);
        return configPath;
    }

    /// <summary>
    /// Extracts metafile JSON from esbuild stdout.
    /// </summary>
    private static string? ExtractMetafileJson(string stdout)
    {
        var startMarker = "__JAZOR_METAFILE_START__";
        var endMarker = "__JAZOR_METAFILE_END__";

        var startIndex = stdout.IndexOf(startMarker, StringComparison.Ordinal);
        if (startIndex < 0)
            return null;

        startIndex += startMarker.Length;

        var endIndex = stdout.IndexOf(endMarker, startIndex, StringComparison.Ordinal);
        if (endIndex < 0)
            return null;

        var json = stdout.Substring(startIndex, endIndex - startIndex).Trim();
        return string.IsNullOrEmpty(json) ? null : json;
    }
}

/// <summary>
/// Result of an esbuild execution.
/// </summary>
internal sealed class EsbuildResult
{
    public bool Success { get; init; }

    public IReadOnlyList<BuildDiagnostic> Errors { get; init; } = [];

    public string? MetafileJson { get; init; }
}
