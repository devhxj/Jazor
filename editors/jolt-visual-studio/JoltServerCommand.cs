using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Script.Serialization;

namespace Jolt.VisualStudio;

internal static class JoltServerCommand
{
    public static async Task<ProcessStartInfo> ResolveAsync(string directory, string? project, string? configuration)
    {
        project ??= FindProject(directory);
        if (project is not null)
        {
            var cwd = Path.GetDirectoryName(project)!;
            var query = Start("dotnet",
                $"msbuild \"{project}\" -nologo -v:q -t:_ResolveJoltTool -getProperty:_JoltHost,_JoltPackageSpec,_JoltRoot", cwd);
            if (!string.IsNullOrEmpty(configuration))
                query.Arguments += $" \"-p:Configuration={configuration}\"";
            query.RedirectStandardOutput = true;
            query.RedirectStandardError = true;
            using var msbuild = Process.Start(query) ?? throw new InvalidOperationException("Could not evaluate the project Jolt tool.");
            var outputTask = msbuild.StandardOutput.ReadToEndAsync();
            var errorTask = msbuild.StandardError.ReadToEndAsync();
            var output = await outputTask.ConfigureAwait(false);
            var errors = await errorTask.ConfigureAwait(false);
            msbuild.WaitForExit();
            if (msbuild.ExitCode == 0)
            {
                var properties = new JavaScriptSerializer().Deserialize<ProjectProperties>(output).Properties;
                ProcessStartInfo? selected = null;
                if (properties.TryGetValue("_JoltHost", out var appHost) && !string.IsNullOrWhiteSpace(appHost))
                    selected = Start(appHost, "lsp", cwd);
                else if (properties.TryGetValue("_JoltPackageSpec", out var package) && !string.IsNullOrWhiteSpace(package))
                    selected = Start("dotnet", $"tool exec \"{package}\" -v quiet -- lsp", cwd);
                if (selected is not null)
                {
                    if (properties.TryGetValue("_JoltRoot", out var root) && !string.IsNullOrWhiteSpace(root))
                        selected.EnvironmentVariables["JAZOR_GENERATED_ROOT"] = root;
                    return selected;
                }
            }
            else if (!(output + errors).Contains("MSB4057"))
            {
                // Honor failures in the selected tool instead of starting another Jolt version.
                throw new InvalidOperationException((output + errors).Trim());
            }
        }

        if (HasLocalTool(directory))
            return Start("dotnet", "tool run jolt -- lsp", directory);

        var cliHome = Environment.GetEnvironmentVariable("DOTNET_CLI_HOME")
            ?? Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        var tools = Path.Combine(cliHome, ".dotnet", "tools");
        var shim = Path.Combine(tools, "jolt.cmd");
        if (!File.Exists(shim))
            throw new FileNotFoundException("Restore the project-local Jazor.Jolt tool, or install it for a standalone Jolt workspace.");

        var result = Start(Path.Combine(Environment.SystemDirectory, "cmd.exe"), "/d /v:off /c %JOLT_EDITOR_SHIM% lsp", directory);
        result.EnvironmentVariables["JOLT_EDITOR_SHIM"] = "\"" + shim + "\"";
        return result;
    }

    private static ProcessStartInfo Start(string command, string arguments, string directory) => new(command, arguments)
    {
        WorkingDirectory = directory,
        UseShellExecute = false,
        CreateNoWindow = true,
    };

    private static string? FindProject(string directory)
    {
        for (var current = new DirectoryInfo(directory); current is not null; current = current.Parent)
        {
            var projects = current.GetFiles("*.csproj");
            if (projects.Length != 0)
                return projects.Length == 1 ? projects[0].FullName : null;
        }
        return null;
    }

    private static bool HasLocalTool(string directory)
    {
        var allowRoot = Environment.GetEnvironmentVariable("DOTNET_TOOLS_ALLOW_MANIFEST_IN_ROOT");
        var serializer = new JavaScriptSerializer();
        for (var current = new DirectoryInfo(directory); current is not null; current = current.Parent)
        {
            if (current.Parent is null && allowRoot != "1" && !string.Equals(allowRoot, "true", StringComparison.OrdinalIgnoreCase))
                break;
            foreach (var name in new[] { ".config/dotnet-tools.json", "dotnet-tools.json" })
            {
                var path = Path.Combine(current.FullName, name);
                if (!File.Exists(path)) continue;
                var manifest = serializer.Deserialize<ToolManifest>(File.ReadAllText(path));
                if (manifest.tools?.Values.Any(tool => tool.commands?.Contains("jolt") == true) == true) return true;
                if (manifest.isRoot) return false;
            }
        }
        return false;
    }

    private sealed class ProjectProperties
    {
        public Dictionary<string, string> Properties { get; set; } = new();
    }

    private sealed class ToolManifest
    {
        public Dictionary<string, Tool>? tools { get; set; }
        public bool isRoot { get; set; }
    }

    private sealed class Tool
    {
        public string[]? commands { get; set; }
    }
}
