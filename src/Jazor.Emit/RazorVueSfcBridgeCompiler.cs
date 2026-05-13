using DenoHost.Core;
using System.Text;
using System.Text.Json;

namespace Jazor.Emit;

internal sealed class RazorVueSfcBridgeCompiler
{
    private const string BridgeScriptRelativePath = "Deno/razorvue-sfc-bridge.ts";
    private static readonly UTF8Encoding Utf8WithoutBom = new(encoderShouldEmitUTF8Identifier: false);
    private static readonly JsonSerializerOptions DenoConfigSerializerOptions = new()
    {
        WriteIndented = true
    };

    public async Task<RazorVueSfcBridgeResult> CompileAsync(RazorVueSfcBridgeOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        var manifestPath = string.IsNullOrWhiteSpace(options.ManifestPath)
            ? Path.Combine(options.HostJazorRoot, "jazor-manifest-razorvue.json")
            : options.ManifestPath;

        if (!Directory.Exists(options.HostJazorRoot))
            return RazorVueSfcBridgeResult.Fail(6, $"RazorVue host output root was not found: '{options.HostJazorRoot}'.");

        if (!File.Exists(manifestPath))
            return RazorVueSfcBridgeResult.Fail(7, $"RazorVue manifest was not found: '{manifestPath}'.");

        var bridgeScriptPath = ResolveBridgeScriptPath();
        if (!File.Exists(bridgeScriptPath))
            return RazorVueSfcBridgeResult.Fail(8, $"RazorVue SFC bridge script was not found: '{bridgeScriptPath}'.");

        if (options.Clean && IsSameOrAncestorOf(options.OutputDirectory, options.HostJazorRoot))
            return RazorVueSfcBridgeResult.Fail(
                11,
                $"RazorVue SFC bridge output directory '{options.OutputDirectory}' cannot be the host output root or one of its parent directories when --clean is true.");

        Directory.CreateDirectory(options.OutputDirectory);
        var denoWorkspace = Path.Combine(
            Path.GetTempPath(),
            "Jazor.Emit",
            "RazorVueSfcBridge",
            Guid.NewGuid().ToString("N"));
        var resultPath = Path.Combine(
            options.OutputDirectory,
            $"razorvue-sfc-bridge.{GetModeValue(options.Mode)}.json");

        try
        {
            Directory.CreateDirectory(denoWorkspace);
            await WriteDenoConfigAsync(denoWorkspace);
            var workspaceBridgeScriptPath = Path.Combine(denoWorkspace, Path.GetFileName(bridgeScriptPath));
            File.Copy(bridgeScriptPath, workspaceBridgeScriptPath, overwrite: true);

            var args = new[]
            {
                "run",
                "-A",
                workspaceBridgeScriptPath,
                "--host-root",
                options.HostJazorRoot,
                "--manifest",
                manifestPath,
                "--out",
                options.OutputDirectory,
                "--mode",
                GetModeValue(options.Mode),
                "--production",
                options.Production ? "true" : "false",
                "--clean",
                options.Clean ? "true" : "false",
                "--write-result",
                resultPath
            };

            await Deno.Execute(
                new DenoExecuteBaseOptions
                {
                    WorkingDirectory = denoWorkspace
                },
                args);

            if (!File.Exists(resultPath))
                return RazorVueSfcBridgeResult.Fail(12, $"RazorVue SFC bridge result file was not written: '{resultPath}'.");

            return RazorVueSfcBridgeResult.Success(resultPath);
        }
        catch (Exception ex)
        {
            return RazorVueSfcBridgeResult.Fail(9, ex.ToString());
        }
        finally
        {
            TryDeleteDirectory(denoWorkspace);
        }
    }

    private static async Task WriteDenoConfigAsync(string denoWorkspace)
    {
        var denoConfigPath = Path.Combine(denoWorkspace, "deno.json");
        var denoConfig = JsonSerializer.Serialize(
            new
            {
                nodeModulesDir = "auto"
            },
            DenoConfigSerializerOptions);

        await File.WriteAllTextAsync(denoConfigPath, denoConfig, Utf8WithoutBom);
    }

    private static void TryDeleteDirectory(string path)
    {
        try
        {
            if (Directory.Exists(path))
                Directory.Delete(path, recursive: true);
        }
        catch
        {
        }
    }

    private static bool IsSameOrAncestorOf(string candidateDirectory, string targetDirectory)
    {
        var candidate = EnsureTrailingDirectorySeparator(Path.GetFullPath(candidateDirectory));
        var target = EnsureTrailingDirectorySeparator(Path.GetFullPath(targetDirectory));
        return target.StartsWith(candidate, StringComparison.OrdinalIgnoreCase);
    }

    private static string EnsureTrailingDirectorySeparator(string path)
        => path.EndsWith(Path.DirectorySeparatorChar) || path.EndsWith(Path.AltDirectorySeparatorChar)
            ? path
            : path + Path.DirectorySeparatorChar;

    internal static string ResolveBridgeScriptPath()
    {
        var baseDirectory = AppContext.BaseDirectory;
        var candidates = new[]
        {
            Path.Combine(baseDirectory, BridgeScriptRelativePath),
            Path.Combine(baseDirectory, "Deno", "razorvue-sfc-bridge.ts"),
            Path.GetFullPath(Path.Combine(baseDirectory, "..", "..", "..", BridgeScriptRelativePath)),
            Path.GetFullPath(Path.Combine(baseDirectory, "..", "..", "..", "..", "src", "Jazor.Emit", BridgeScriptRelativePath))
        };

        return candidates.FirstOrDefault(File.Exists) ?? candidates[0];
    }

    private static string GetModeValue(RazorVueSfcBridgeMode mode)
        => mode switch
        {
            RazorVueSfcBridgeMode.Browser => "browser",
            RazorVueSfcBridgeMode.Ssr => "ssr",
            _ => throw new ArgumentOutOfRangeException(nameof(mode), mode, "Unsupported RazorVue SFC bridge mode.")
        };
}

internal sealed record RazorVueSfcBridgeResult(
    bool IsSuccess,
    int ExitCode,
    string? ResultPath,
    string? Error)
{
    public static RazorVueSfcBridgeResult Success(string resultPath)
        => new(true, 0, resultPath, null);

    public static RazorVueSfcBridgeResult Fail(int exitCode, string error)
        => new(false, exitCode, null, error);
}
