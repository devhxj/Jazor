#!/usr/bin/env dotnet run

using System.Diagnostics;
using System.Text.Json;

var repoRoot = RequireRepositoryRoot();
var generatorProject = Path.Combine(repoRoot, "src", "ECMAScript.Vue.Generator", "ECMAScript.Vue.Generator.csproj");
var checks = new[]
{
    new Check("elementplus", ["elementplus", "--check"]),
    new Check("vuetify", ["vuetify", "--check"]),
    new Check("tdesign snapshot", ["tdesign", "snapshot", "--check"]),
    new Check("tdesign bindings", ["tdesign", "bindings", "--check"]),
    new Check("tdesign components", ["tdesign", "components", "--check"])
};

foreach (var check in checks)
{
    Console.WriteLine($"[binding-contract] {check.Name}");
    await RunDotNetAsync(generatorProject, check.Arguments, repoRoot);
}

var targets = new[]
{
    new BindingTarget("element-plus", "2.14.4", Path.Combine(repoRoot, "src", "ECMAScript.ElementPlus"), Path.Combine(repoRoot, "src", "ECMAScript.Vue.Generator", "upstream", "element-plus", "2.14.4"), "Element Plus"),
    new BindingTarget("vuetify", "4.1.8", Path.Combine(repoRoot, "src", "ECMAScript.Vuetify"), Path.Combine(repoRoot, "src", "ECMAScript.Vue.Generator", "upstream", "vuetify", "4.1.8"), "Vuetify"),
    new BindingTarget("tdesign-vue-next", "1.20.5", Path.Combine(repoRoot, "src", "ECMAScript.TDesign"), Path.Combine(repoRoot, "src", "ECMAScript.Vue.Generator", "upstream", "tdesign-vue-next", "1.20.5"), "TDesign")
};

foreach (var target in targets)
    VerifyTarget(target);

Console.WriteLine("Vue binding contract gate passed.");

static void VerifyTarget(BindingTarget target)
{
    var manifestPath = Path.Combine(target.ProjectDirectory, "manifest.json");
    var readmePath = Path.Combine(target.ProjectDirectory, "README.md");
    if (!File.Exists(manifestPath) || !File.Exists(readmePath) || !Directory.Exists(target.UpstreamDirectory))
        throw new InvalidOperationException($"{target.DisplayName}: project, README, manifest, or upstream snapshot is missing.");

    using var manifest = JsonDocument.Parse(File.ReadAllText(manifestPath));
    var root = manifest.RootElement;
    var libraryId = root.GetProperty("libraryId").GetString();
    var version = root.GetProperty("version").GetString();
    if (!string.Equals(libraryId, target.LibraryId, StringComparison.Ordinal) || !string.Equals(version, target.Version, StringComparison.Ordinal))
        throw new InvalidOperationException($"{target.DisplayName}: manifest identity is {libraryId}@{version}, expected {target.LibraryId}@{target.Version}.");

    var upstreamMetadata = Path.Combine(target.UpstreamDirectory, "package.json");
    if (!File.Exists(upstreamMetadata))
        throw new InvalidOperationException($"{target.DisplayName}: upstream package.json is missing.");

    using var package = JsonDocument.Parse(File.ReadAllText(upstreamMetadata));
    var upstreamVersion = package.RootElement.TryGetProperty("version", out var versionProperty) ? versionProperty.GetString() : null;
    if (!string.Equals(upstreamVersion, target.Version, StringComparison.Ordinal))
        throw new InvalidOperationException($"{target.DisplayName}: upstream snapshot version is {upstreamVersion}, expected {target.Version}.");

    var readme = File.ReadAllText(readmePath);
    if (!readme.Contains("注释", StringComparison.Ordinal) || !readme.Contains("manifest.json", StringComparison.Ordinal))
        throw new InvalidOperationException($"{target.DisplayName}: README must document original comments and manifest resource ownership.");

    Console.WriteLine($"  {target.DisplayName}: {target.LibraryId}@{target.Version}; source docs and manifest present");
}

static async Task RunDotNetAsync(string projectPath, IReadOnlyList<string> commandArguments, string workdir)
{
    using var process = new Process
    {
        StartInfo = new ProcessStartInfo
        {
            FileName = "dotnet", WorkingDirectory = workdir, UseShellExecute = false,
            RedirectStandardOutput = true, RedirectStandardError = true, CreateNoWindow = true
        }
    };
    process.StartInfo.ArgumentList.Add("run");
    process.StartInfo.ArgumentList.Add("--project");
    process.StartInfo.ArgumentList.Add(projectPath);
    process.StartInfo.ArgumentList.Add("--");
    foreach (var argument in commandArguments)
        process.StartInfo.ArgumentList.Add(argument);

    process.Start();
    var stdout = process.StandardOutput.ReadToEndAsync();
    var stderr = process.StandardError.ReadToEndAsync();
    await process.WaitForExitAsync();
    var output = await stdout;
    var error = await stderr;
    if (process.ExitCode == 0)
    {
        Console.Write(output);
        return;
    }

    throw new InvalidOperationException($"Binding generator check failed ({string.Join(' ', commandArguments)}).{Environment.NewLine}{output}{Environment.NewLine}{error}");
}

static string RequireRepositoryRoot()
{
    for (var directory = new DirectoryInfo(Directory.GetCurrentDirectory()); directory is not null; directory = directory.Parent)
        if (File.Exists(Path.Combine(directory.FullName, "Jazor.slnx")))
            return directory.FullName;
    throw new InvalidOperationException("Unable to locate Jazor.slnx.");
}

sealed record Check(string Name, IReadOnlyList<string> Arguments);
sealed record BindingTarget(string LibraryId, string Version, string ProjectDirectory, string UpstreamDirectory, string DisplayName);
