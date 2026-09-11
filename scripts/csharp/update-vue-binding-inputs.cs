#!/usr/bin/env dotnet run

using System.Formats.Tar;
using System.Diagnostics;
using System.IO.Compression;
using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Nodes;

// Refresh only the inputs and shipped assets. Run the binding generators afterwards
// so reviewed C# type projections are never replaced by guessed TypeScript mappings.
// 仅更新输入与资源；组件契约仍由各自生成器生成并检查。
if (args.Length != 2)
    throw new ArgumentException("Usage: update-vue-binding-inputs.cs -- <vuetify|element-plus|tdesign-vue-next> <version>");
var package = args[0];
var version = args[1];
var project = package switch
{
    "vuetify" => "ECMAScript.Vuetify",
    "element-plus" => "ECMAScript.ElementPlus",
    "tdesign-vue-next" => "ECMAScript.TDesign",
    _ => throw new ArgumentException("Unsupported binding package: " + package)
};
var root = Directory.GetCurrentDirectory();
if (!File.Exists(Path.Combine(root, "Jazor.slnx")))
    throw new InvalidOperationException("Run from the repository root.");
var projectRoot = Path.Combine(root, "src", project);
var manifestPath = Path.Combine(projectRoot, "manifest.json");
var manifest = JsonNode.Parse(File.ReadAllText(manifestPath))!;
var previousVersion = manifest["version"]!.GetValue<string>();
var upstream = Path.Combine(root, "src", "ECMAScript.Vue.Generator", "upstream", package);
var previousSnapshot = Path.Combine(upstream, previousVersion);
var snapshot = Path.Combine(upstream, version);
var cache = Path.Combine(root, ".tmp", "packages", $"{package}-{version}.tgz");
Directory.CreateDirectory(Path.GetDirectoryName(cache)!);

using var client = new HttpClient { Timeout = TimeSpan.FromMinutes(5) };
using var metadata = JsonDocument.Parse(await client.GetStringAsync($"https://registry.npmjs.org/{package}/{version}"));
if (metadata.RootElement.GetProperty("version").GetString() != version)
    throw new InvalidOperationException("Registry returned a different package version.");
var distribution = metadata.RootElement.GetProperty("dist");
var integrity = distribution.GetProperty("integrity").GetString()!;
if (!integrity.StartsWith("sha512-", StringComparison.Ordinal))
    throw new InvalidOperationException("Expected npm SHA-512 integrity metadata.");
var expectedHash = integrity[7..];
if (!File.Exists(cache) || HashArchive(cache) != expectedHash)
{
    Console.WriteLine($"Downloading {package}@{version}");
    await using (var input = await client.GetStreamAsync(distribution.GetProperty("tarball").GetString()!))
    await using (var output = File.Create(cache))
        await input.CopyToAsync(output);
}
if (HashArchive(cache) != expectedHash)
    throw new InvalidOperationException("Downloaded archive does not match npm integrity metadata.");

var files = new Dictionary<string, string>(StringComparer.Ordinal);
if (package != "tdesign-vue-next")
{
    foreach (var file in Directory.EnumerateFiles(previousSnapshot, "*", SearchOption.AllDirectories))
    {
        var relative = Path.GetRelativePath(previousSnapshot, file).Replace('\\', '/');
        if (relative != "contracts.json")
            files.Add(package == "vuetify" && relative == "web-types.json" ? "dist/json/web-types.json" : relative, Path.Combine(snapshot, relative));
    }
}
var assets = package switch
{
    "vuetify" => new[] { ("dist/vuetify.esm.js", "dist/vuetify.esm.js"), ("dist/vuetify-labs.esm.js", "dist/vuetify-labs.esm.js"), ("dist/vuetify.min.css", "dist/vuetify.min.css"), ("LICENSE.md", "licenses/LICENSE.md") },
    "element-plus" => [("dist/index.full.min.mjs", "dist/index.full.min.mjs"), ("dist/index.css", "dist/index.css"), ("LICENSE", "licenses/LICENSE")],
    _ => [("dist/tdesign.css", "dist/tdesign.css"), ("LICENSE", "licenses/LICENSE")]
};
foreach (var (source, destination) in assets)
    files.Add(source, Path.Combine(projectRoot, destination));

using (var archive = File.OpenRead(cache))
using (var gzip = new GZipStream(archive, CompressionMode.Decompress))
using (var reader = new TarReader(gzip))
{
    while (reader.GetNextEntry() is { } entry)
    {
        if (!entry.Name.StartsWith("package/", StringComparison.Ordinal) || entry.DataStream is null ||
            !files.Remove(entry.Name[8..], out var destination))
            continue;
        Directory.CreateDirectory(Path.GetDirectoryName(destination)!);
        using var output = File.Create(destination);
        entry.DataStream.CopyTo(output);
    }
}
if (files.Count > 0)
    throw new InvalidOperationException("Missing upstream inputs: " + string.Join(", ", files.Keys));
if (package == "tdesign-vue-next")
{
    // The upstream dist bundle is UMD. Bundle the ESM entry with Vue external so
    // the consumer shares its existing Vue instance. Lock transitive inputs for reproducibility.
    // dist 为 UMD；必须从 ESM 入口打包并外置 Vue，避免应用产生第二份 Vue 实例。
    var buildRoot = Path.Combine(root, "src", "ECMAScript.Vue.Generator", "tdesign-runtime");
    var buildPackage = JsonNode.Parse(File.ReadAllText(Path.Combine(buildRoot, "package.json")))!;
    if (buildPackage["dependencies"]![package]!.GetValue<string>() != version)
        throw new InvalidOperationException("Update tdesign-runtime/package.json and package-lock.json to the requested version first.");
    await RunAsync(OperatingSystem.IsWindows() ? "cmd.exe" : "npm", buildRoot,
        OperatingSystem.IsWindows() ? ["/d", "/c", "npm.cmd", "ci", "--ignore-scripts", "--no-audit", "--no-fund"] : ["ci", "--ignore-scripts", "--no-audit", "--no-fund"]);
    await RunAsync("node", buildRoot,
        ["node_modules/esbuild/bin/esbuild", "node_modules/tdesign-vue-next/es/index.mjs", "--bundle", "--format=esm", "--platform=browser", "--target=es2022", "--external:vue", "--charset=utf8", "--outfile=" + Path.Combine(projectRoot, "dist", "tdesign.mjs")]);
}
if (package == "vuetify")
{
    var contracts = File.ReadAllText(Path.Combine(previousSnapshot, "contracts.json"));
    File.WriteAllText(Path.Combine(snapshot, "contracts.json"), contracts.Replace(
        $"\"upstreamVersion\": \"{previousVersion}\"", $"\"upstreamVersion\": \"{version}\"", StringComparison.Ordinal));
}
manifest["version"] = version;
RefreshHashes(manifest);
File.WriteAllText(manifestPath, manifest.ToJsonString(new JsonSerializerOptions { WriteIndented = true }) + "\n");
Console.WriteLine($"Updated {package}: {previousVersion} -> {version}. Run the generators and review contract drift.");

static string HashArchive(string path)
{
    using var stream = File.OpenRead(path);
    return Convert.ToBase64String(SHA512.HashData(stream));
}

static async Task RunAsync(string executable, string workingDirectory, string[] arguments)
{
    var start = new ProcessStartInfo(executable) { WorkingDirectory = workingDirectory, UseShellExecute = false, CreateNoWindow = true };
    foreach (var argument in arguments) start.ArgumentList.Add(argument);
    using var process = Process.Start(start) ?? throw new InvalidOperationException("Could not start " + executable);
    await process.WaitForExitAsync();
    if (process.ExitCode != 0) throw new InvalidOperationException(executable + " failed: " + process.ExitCode);
}

void RefreshHashes(JsonNode node)
{
    if (node is JsonObject obj)
    {
        foreach (var (key, pathKey) in new[] { ("hash", "path"), ("developmentHash", "development"), ("productionHash", "production") })
            if (obj.ContainsKey(key))
                obj[key] = Convert.ToHexStringLower(SHA256.HashData(File.ReadAllBytes(Path.Combine(projectRoot, obj[pathKey]!.GetValue<string>()))));
        foreach (var child in obj.ToArray())
            if (child.Value is not null) RefreshHashes(child.Value);
    }
    else if (node is JsonArray array)
        foreach (var child in array)
            if (child is not null) RefreshHashes(child);
}
