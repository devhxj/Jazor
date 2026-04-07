using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Jazor.Emit;

internal sealed class RazorVueHostAssetWriter
{
    private static readonly UTF8Encoding Utf8WithoutBom = new(encoderShouldEmitUTF8Identifier: false);
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public void Sync(string bundlePath, RazorVueManifestModel? manifest)
    {
        var cssPath = GetCssPath(bundlePath);
        var hostPath = GetHostContractPath(bundlePath);

        if (manifest is null)
        {
            DeleteIfExists(cssPath);
            DeleteIfExists(hostPath);
            return;
        }

        var styles = manifest.Styles ?? [];
        var pluginRequirements = manifest.PluginRequirements ?? [];

        if (styles.Count == 0)
        {
            DeleteIfExists(cssPath);
        }
        else
        {
            var css = BuildCss(styles);
            File.WriteAllText(cssPath, css, Utf8WithoutBom);
        }

        var contract = new RazorVueHostContractModel(
            manifest.AssemblyName,
            manifest.GeneratedAtUtc,
            Path.GetFileName(bundlePath) ?? string.Empty,
            styles,
            pluginRequirements,
            manifest.Modules
                .Select(static module => new RazorVueHostContractModule(
                    module.ComponentName,
                    module.RelativeModulePath,
                    module.Styles,
                    module.PluginRequirements))
                .ToList());
        File.WriteAllText(hostPath, JsonSerializer.Serialize(contract, JsonOptions), Utf8WithoutBom);
    }

    public static string GetCssPath(string bundlePath)
        => Path.Combine(
            Path.GetDirectoryName(bundlePath) ?? string.Empty,
            Path.GetFileNameWithoutExtension(bundlePath) + ".razorvue.css");

    public static string GetHostContractPath(string bundlePath)
        => Path.Combine(
            Path.GetDirectoryName(bundlePath) ?? string.Empty,
            Path.GetFileNameWithoutExtension(bundlePath) + ".razorvue.host.json");

    private static string BuildCss(IReadOnlyList<string> styles)
    {
        // Preserve manifest ordering so host-side style entrypoints stay deterministic.
        var lines = styles
            .Select(static style => $"@import \"{style}\";")
            .ToArray();
        return string.Join(Environment.NewLine, lines) + Environment.NewLine;
    }

    private static void DeleteIfExists(string path)
    {
        if (File.Exists(path))
            File.Delete(path);
    }
}

internal sealed record RazorVueHostContractModel(
    string AssemblyName,
    DateTime GeneratedAtUtc,
    string BundleFile,
    IReadOnlyList<string> Styles,
    IReadOnlyList<string> PluginRequirements,
    IReadOnlyList<RazorVueHostContractModule> Modules);

internal sealed record RazorVueHostContractModule(
    string ComponentName,
    string RelativeModulePath,
    IReadOnlyList<string> Styles,
    IReadOnlyList<string> PluginRequirements);
