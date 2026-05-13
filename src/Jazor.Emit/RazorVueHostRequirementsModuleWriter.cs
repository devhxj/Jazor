using System.Text;
using System.Text.Json;

namespace Jazor.Emit;

internal sealed class RazorVueHostRequirementsModuleWriter
{
    private const string HostRequirementsModuleRelativePath = "__jazor/razorvue-host.mjs";
    private static readonly UTF8Encoding Utf8WithoutBom = new(encoderShouldEmitUTF8Identifier: false);

    public WriteResult Sync(string outputDirectory, ManifestModel manifest)
    {
        ArgumentNullException.ThrowIfNull(manifest);

        Directory.CreateDirectory(outputDirectory);

        var hostRequirementsModulePath = GetHostRequirementsModulePath(outputDirectory);
        var razorVueManifest = manifest.ToRazorVueManifest();
        if (razorVueManifest.Modules.Count == 0)
        {
            if (!File.Exists(hostRequirementsModulePath))
                return WriteResult.Success(0, 0, 0);

            File.Delete(hostRequirementsModulePath);
            return WriteResult.Success(0, 0, 1);
        }

        var hostRequirementsDirectory = Path.GetDirectoryName(hostRequirementsModulePath);
        if (!string.IsNullOrWhiteSpace(hostRequirementsDirectory))
            Directory.CreateDirectory(hostRequirementsDirectory);

        var hostRequirementsCode = BuildHostRequirementsModule(razorVueManifest);
        if (File.Exists(hostRequirementsModulePath) &&
            string.Equals(File.ReadAllText(hostRequirementsModulePath), hostRequirementsCode, StringComparison.Ordinal))
        {
            return WriteResult.Success(0, 1, 0);
        }

        File.WriteAllText(hostRequirementsModulePath, hostRequirementsCode, Utf8WithoutBom);
        return WriteResult.Success(1, 0, 0);
    }

    public static string GetHostRequirementsModulePath(string outputDirectory)
        => GetTargetPath(EnsureDirectorySeparator(Path.GetFullPath(outputDirectory)), HostRequirementsModuleRelativePath);

    public static string BuildHostRequirementsModule(RazorVueManifestModel manifest)
    {
        ArgumentNullException.ThrowIfNull(manifest);

        var assemblyNameLiteral = JsonSerializer.Serialize(manifest.AssemblyName);
        var generatedAtUtcLiteral = JsonSerializer.Serialize(manifest.GeneratedAtUtc.ToString("O"));
        var stylesLiteral = BuildStringArrayLiteral(manifest.Styles ?? []);
        var modulesLiteral = BuildHostModulesLiteral(manifest.Modules);
        var pluginRequirementsLiteral = BuildStringArrayLiteral(manifest.PluginRequirements ?? []);

        return $$"""
        export const razorVueHostAssemblyName = {{assemblyNameLiteral}};
        export const razorVueHostGeneratedAtUtc = {{generatedAtUtcLiteral}};
        export const razorVueStyles = Object.freeze({{stylesLiteral}});
        export const razorVuePluginRequirements = Object.freeze({{pluginRequirementsLiteral}});
        export const razorVueHostModules = Object.freeze({{modulesLiteral}});
        export const razorVueHostRequirements = Object.freeze({
          assemblyName: razorVueHostAssemblyName,
          generatedAtUtc: razorVueHostGeneratedAtUtc,
          styles: razorVueStyles,
          pluginRequirements: razorVuePluginRequirements,
          modules: razorVueHostModules
        });
        """.ReplaceLineEndings("\n");
    }

    private static string GetTargetPath(string normalizedOutputDirectory, string relativePath)
    {
        var relativePlatformPath = relativePath.Replace('/', Path.DirectorySeparatorChar);
        var fullPath = Path.GetFullPath(Path.Combine(normalizedOutputDirectory, relativePlatformPath));
        if (!fullPath.StartsWith(normalizedOutputDirectory, StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException($"Refusing to write outside output directory: '{relativePath}'.");

        return fullPath;
    }

    private static string EnsureDirectorySeparator(string path)
        => path.EndsWith(Path.DirectorySeparatorChar) ? path : path + Path.DirectorySeparatorChar;

    private static string BuildHostModulesLiteral(IReadOnlyList<RazorVueManifestEntry> modules)
        => JsonSerializer.Serialize(
            modules.Select(static module => new
            {
                assemblyName = module.AssemblyName,
                componentId = module.ComponentId,
                moduleId = module.ModuleId,
                componentName = module.ComponentName,
                relativeModulePath = module.RelativeModulePath,
                sourceMapPath = module.SourceMapPath,
                originMapPath = module.OriginMapPath,
                styles = module.Styles,
                pluginRequirements = module.PluginRequirements,
                descriptorHash = module.DescriptorHash,
                templateHash = module.TemplateHash,
                logicHash = module.LogicHash,
                contentHash = module.ContentHash,
                styleHash = module.StyleHash,
                hmrBoundaryKind = module.HmrBoundaryKind,
                requiresHydration = module.RequiresHydration,
                supportsSsr = module.SupportsSsr
            }));

    private static string BuildStringArrayLiteral(IReadOnlyList<string> values)
        => "[" + string.Join(", ", values.Select(static value => JsonSerializer.Serialize(value))) + "]";
}
