using System.Collections.Concurrent;
using System.Diagnostics;
using System.Text.Json;
using System.Xml.Linq;
using Jolt.Compiler;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.Extensions.FileSystemGlobbing;

namespace Jolt.Project;

public static partial class JoltProjectLoader
{
    private static readonly StringComparer DotNetPaths = OperatingSystem.IsWindows()
        ? StringComparer.OrdinalIgnoreCase : StringComparer.Ordinal;
    private static readonly ConcurrentDictionary<string, (DateTime Timestamp, long Size, MetadataReference Reference)> DotNetReferences =
        new(DotNetPaths);
    private static readonly ConcurrentDictionary<string, string[]> EvaluatedProjects = new(DotNetPaths);

    private static async Task<DotNetInputs?> ReadDotNetProjectAsync(string root,
        IReadOnlyDictionary<string, string>? overlays,
        bool evaluateDotNetProject, CancellationToken cancellationToken)
    {
        var projects = Directory.GetFiles(root, "*.csproj");
        if (projects.Length == 0)
            return null;
        var inputPath = Path.Combine(root, ".jazor", "project.inputs");
        var inputs = File.Exists(inputPath) ? await File.ReadAllLinesAsync(inputPath, cancellationToken) : [];
        var projectPath = inputs.FirstOrDefault(line => line.StartsWith("project|", StringComparison.Ordinal))?[8..];
        if (!projects.Contains(projectPath, DotNetPaths))
        {
            if (projects.Length != 1)
                return null;
            projectPath = projects[0];
            inputs = [];
        }

        if (evaluateDotNetProject)
        {
            var evaluated = await EvaluateDotNetProjectAsync(projectPath!, resolveReferences: true, cancellationToken);
            if (evaluated is not null)
                EvaluatedProjects[projectPath!] = evaluated;
            else if (await EvaluateDotNetProjectAsync(projectPath!, resolveReferences: false, cancellationToken) is { } evaluatedSources)
                EvaluatedProjects[projectPath!] = evaluatedSources;
        }
        if (EvaluatedProjects.TryGetValue(projectPath!, out var current))
            inputs = current;
        var project = XDocument.Load(projectPath!);
        var rootNamespace = InputValues("namespace").FirstOrDefault() ?? Property("RootNamespace") ??
            Path.GetFileNameWithoutExtension(projectPath)!.Replace(' ', '_').Replace('-', '_');
        var paths = new HashSet<string>(DotNetPaths);
        var references = new List<MetadataReference>();
        var globalUsings = new List<string>();
        var excludedDirectories = new HashSet<string>(DotNetPaths);
        foreach (var path in InputValues("output").Concat(new[] { Property("JazorDir"), Property("BaseIntermediateOutputPath"),
            Property("IntermediateOutputPath"), Property("BaseOutputPath"), Environment.GetEnvironmentVariable("JAZOR_GENERATED_ROOT") }))
        {
            if (string.IsNullOrWhiteSpace(path) || path.Contains("$(", StringComparison.Ordinal))
                continue;
            var directory = Path.TrimEndingDirectorySeparator(Path.GetFullPath(path, root));
            if (!DotNetPaths.Equals(root, directory) && IsProjectRelative(Relative(directory)))
                excludedDirectories.Add(directory);
        }
        Matcher? matcher = null;
        if (inputs.Length != 0)
        {
            // The SDK's npm projection and Jolt's projection can declare the same
            // partial exports. The latter owns these names in the Jolt compilation.
            var projectName = Path.GetFileNameWithoutExtension(projectPath);
            paths.UnionWith(InputValues("source").Where(path =>
                Path.GetFileName(path) != projectName + ".GlobalUsings.g.cs" &&
                Path.GetFileName(path) != projectName + ".npm.g.cs"));
            foreach (var value in InputValues("using"))
            {
                var parts = value.Split('|');
                if (parts.Length == 3)
                    AddUsing(parts[0], parts[1], parts[2]);
            }
            var platform = new HashSet<string>(((string)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES")!)
                .Split(Path.PathSeparator).Select(Path.GetFileName)!, DotNetPaths);
            foreach (var path in InputValues("reference").Distinct(DotNetPaths))
            {
                if (platform.Contains(Path.GetFileName(path)))
                    continue;
                var file = new FileInfo(path);
                if (!file.Exists)
                    continue;
                var timestamp = file.LastWriteTimeUtc;
                var size = file.Length;
                var entry = DotNetReferences.AddOrUpdate(path,
                    _ => (timestamp, size, MetadataReference.CreateFromFile(path)),
                    (_, previous) => previous.Timestamp == timestamp && previous.Size == size ? previous :
                        (timestamp, size, MetadataReference.CreateFromFile(path)));
                references.Add(entry.Reference);
            }
        }
        else
        {
            // Before the first build, support conventional SDK projects without running MSBuild in the editor.
            if (Property("EnableDefaultCompileItems") != "false" && Property("EnableDefaultItems") != "false")
            {
                matcher = new Matcher(OperatingSystem.IsWindows() ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal);
                matcher.AddInclude("**/*.cs");
                foreach (var item in project.Descendants("Compile").SelectMany(element => ((string?)element.Attribute("Remove") ?? "").Split(';')))
                    if (item.Length > 0)
                        matcher.AddExclude(item.Replace('\\', '/'));
            }
            foreach (var item in project.Descendants("Compile").SelectMany(element => ((string?)element.Attribute("Include") ?? "").Split(';')))
                if (item.Length > 0 && !item.Contains('*') && !item.Contains("$("))
                    paths.Add(Path.GetFullPath(item, root));
            if (Property("ImplicitUsings") is "enable" or "true")
            {
                foreach (var ns in new[] { "System", "System.Collections.Generic", "System.IO", "System.Linq",
                    "System.Net.Http", "System.Threading", "System.Threading.Tasks" })
                    AddUsing(ns);
            }
            foreach (var item in project.Descendants("Using"))
                if ((string?)item.Attribute("Include") is { } ns)
                    AddUsing(ns, (string?)item.Attribute("Alias"), (string?)item.Attribute("Static"));
        }
        var explicitPaths = new HashSet<string>(paths.Select(Relative), DotNetPaths);
        var selection = new CSharpSourceSelection(root, explicitPaths, matcher);
        if (matcher is not null)
            foreach (var file in Discover(root, csharp: true, stopAtNestedProjects: true, excludedDirectories: excludedDirectories))
                if (selection.Includes(file))
                    paths.Add(file);
        if (overlays is not null)
            foreach (var path in overlays.Keys)
                if (selection.Includes(path) && !IsExcluded(path, excludedDirectories))
                    paths.Add(path);
        var sources = new List<JoltSource>(paths.Count + 1);
        foreach (var path in paths.Order(StringComparer.Ordinal))
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (overlays?.TryGetValue(path, out var text) == true)
                sources.Add(new(path, text));
            else if (File.Exists(path))
                sources.Add(new(path, await File.ReadAllTextAsync(path, cancellationToken)));
        }
        if (globalUsings.Count != 0)
            sources.Add(new(Path.Combine(root, ".jazor", "project.usings.g.cs"), string.Join('\n', globalUsings)));
        return new(rootNamespace, sources, references, selection,
            InputValues("define").Where(SyntaxFacts.IsValidIdentifier).Distinct(StringComparer.Ordinal).ToArray(), excludedDirectories);

        IEnumerable<string> InputValues(string prefix) => inputs.Where(line => line.StartsWith(prefix + "|", StringComparison.Ordinal))
            .Select(line => line[(prefix.Length + 1)..]);
        string? Property(string name) => project.Descendants(name).LastOrDefault(element =>
            element.Attribute("Condition") is null && element.Parent?.Attribute("Condition") is null)?.Value.Trim();
        string Relative(string path) => Path.GetRelativePath(root, path).Replace('\\', '/');
        void AddUsing(string ns, string? alias = null, string? isStatic = null)
        {
            if (!SyntaxFactory.ParseName(ns).ContainsDiagnostics)
                globalUsings.Add($"global using {(string.Equals(isStatic, "true", StringComparison.OrdinalIgnoreCase) ? "static " : "")}{(string.IsNullOrEmpty(alias) ? "" : alias + " = ")}{ns};");
        }
    }


    private sealed record DotNetInputs(string Namespace, IReadOnlyList<JoltSource> Sources,
        IReadOnlyList<MetadataReference> References, CSharpSourceSelection Selection, IReadOnlyList<string> Defines,
        IReadOnlySet<string> ExcludedDirectories);

    internal static async Task<string> GetGeneratedRootAsync(string directory, CancellationToken cancellationToken)
    {
        // A direct tool invocation honors MSBuild imports and conditions even before a build.
        var projects = Directory.GetFiles(directory, "*.csproj");
        string? project = projects.Length == 1 ? projects[0] : null;
        if (projects.Length > 1)
        {
            var snapshot = Path.Combine(directory, ".jazor", "project.inputs");
            if (File.Exists(snapshot))
            {
                var owner = File.ReadLines(snapshot).FirstOrDefault(line => line.StartsWith("project|", StringComparison.Ordinal))?[8..];
                project = projects.FirstOrDefault(path => DotNetPaths.Equals(path, owner));
            }
        }
        if (project is not null)
        {
            var result = await QueryMsBuildAsync(project, ["-getProperty:JazorDir,MSBuildProjectFullPath"], cancellationToken);
            if (result.ExitCode != 0)
                throw new InvalidOperationException($"Could not evaluate JazorDir for '{project}': {result.Error}{result.Output}");
            using var document = JsonDocument.Parse(result.Output);
            var configured = document.RootElement.GetProperty("Properties").GetProperty("JazorDir").GetString();
            if (!string.IsNullOrEmpty(configured))
                return Path.GetFullPath(configured, directory);
        }
        return Path.Combine(directory, Jazor.Common.JazorArtifactDefaults.DirectoryName);
    }

    private static async Task<string[]?> EvaluateDotNetProjectAsync(string project, bool resolveReferences, CancellationToken cancellationToken)
    {
        // Only evaluate compiler inputs and resolve existing references. Running Build here
        // would couple editor requests to backend method bodies and deployment targets.
        var arguments = new List<string> {
            "-getProperty:RootNamespace,DefineConstants,JazorDir,BaseIntermediateOutputPath,IntermediateOutputPath,BaseOutputPath",
            "-getItem:Compile,Using,ReferencePath",
            "-p:BuildProjectReferences=false", "-p:DesignTimeBuild=true" };
        if (resolveReferences)
            arguments.Add("-target:ResolveReferences");
        var result = await QueryMsBuildAsync(project, arguments, cancellationToken);
        if (result.ExitCode != 0)
            return null;
        using var document = JsonDocument.Parse(result.Output);
        var properties = document.RootElement.GetProperty("Properties");
        var items = document.RootElement.GetProperty("Items");
        var lines = new List<string> { "project|" + project, "namespace|" + properties.GetProperty("RootNamespace").GetString() };
        lines.AddRange(new[] { "JazorDir", "BaseIntermediateOutputPath", "IntermediateOutputPath", "BaseOutputPath" }
            .Select(name => "output|" + properties.GetProperty(name).GetString()));
        lines.AddRange((properties.GetProperty("DefineConstants").GetString() ?? "").Split(';', StringSplitOptions.RemoveEmptyEntries)
            .Select(value => "define|" + value));
        lines.AddRange(items.GetProperty("Compile").EnumerateArray().Select(item => "source|" + Value(item, "FullPath")));
        lines.AddRange(items.GetProperty("Using").EnumerateArray().Select(item =>
            $"using|{Value(item, "Identity")}|{Value(item, "Alias")}|{Value(item, "Static")}"));
        lines.AddRange(items.GetProperty("ReferencePath").EnumerateArray().Select(item => "reference|" + Value(item, "FullPath")));
        return lines.ToArray();

        static string? Value(JsonElement item, string name) => item.TryGetProperty(name, out var value) ? value.GetString() : null;
    }

    private static async Task<(int ExitCode, string Output, string Error)> QueryMsBuildAsync(string project,
        IReadOnlyList<string> arguments, CancellationToken cancellationToken)
    {
        var start = new ProcessStartInfo(Environment.GetEnvironmentVariable("DOTNET_HOST_PATH") ?? "dotnet")
        {
            WorkingDirectory = Path.GetDirectoryName(project)!,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
        };
        foreach (var argument in new[] { "msbuild", project, "-nologo", "-verbosity:quiet", "-nodeReuse:false" }.Concat(arguments))
            start.ArgumentList.Add(argument);
        Process? launched;
        try { launched = Process.Start(start); }
        catch (System.ComponentModel.Win32Exception error) { return (-1, "", error.Message); }
        using var process = launched!;
        using var cancellation = cancellationToken.Register(() =>
        {
            try { process.Kill(entireProcessTree: true); }
            catch (InvalidOperationException) { }
        });
        var output = process.StandardOutput.ReadToEndAsync(cancellationToken);
        var errors = process.StandardError.ReadToEndAsync(cancellationToken);
        await process.WaitForExitAsync(cancellationToken);
        return (process.ExitCode, await output, await errors);
    }
}
