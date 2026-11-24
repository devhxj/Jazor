#:property CETCompat=false
#:property PublishAot=false
#:property AssemblyName=Jazor.NodeApiVerification
#:project ../../src/Jazor.Npm/Jazor.Npm.csproj

using Jazor.Npm;
using System.Diagnostics;

var store = Environment.GetEnvironmentVariable("JazorPnpmStoreDirectory") ?? Path.GetFullPath(".tmp/nodeapi-store");
if (args is ["--read", var workerDocument, var workerKind])
{
    await Read(new NpmValueMetadataProvider(store), workerDocument, workerKind);
    return;
}

var project = Directory.CreateDirectory(Path.GetFullPath($".tmp/jolt-npm-cache-{Guid.NewGuid():N}"));
try
{
    var fixture = Directory.CreateDirectory(Path.Combine(project.FullName, "fixture")).FullName;
    File.WriteAllText(Path.Combine(project.FullName, "package.json"),
        """{"private":true,"dependencies":{"cache-fixture":"link:./fixture"}}""");
    const string manifest = """{"name":"cache-fixture","version":"1.0.0","type":"module","exports":{"types":"./index.d.ts","import":"./index.js"}}""";
    File.WriteAllText(Path.Combine(fixture, "package.json"), manifest);
    File.WriteAllText(Path.Combine(fixture, "index.js"), "export const answer = 42;");
    File.WriteAllText(Path.Combine(fixture, "index.d.ts"), "export declare const answer: number;");
    File.WriteAllText(Path.Combine(fixture, "alternate.js"), "export const answer = 'updated';");
    File.WriteAllText(Path.Combine(fixture, "alternate.d.ts"), "export { answer } from './dep.js';");
    File.WriteAllText(Path.Combine(fixture, "dep.d.ts"), "export declare const answer: string;");
    var document = Path.Combine(project.FullName, "Probe.cs");
    var cache = Path.Combine(project.FullName, ".jazor", "cache", "npm-metadata");

    await Read(new NpmValueMetadataProvider(store), document, "number");
    var files = Directory.GetFiles(cache, "*.json");
    Require(files.Length == 3);
    var marker = new DateTime(2020, 1, 1, 0, 0, 0, DateTimeKind.Utc);
    foreach (var file in files)
        File.SetLastWriteTimeUtc(file, marker);
    var start = new System.Diagnostics.ProcessStartInfo(Environment.ProcessPath!) { UseShellExecute = false };
    start.ArgumentList.Add("--read");
    start.ArgumentList.Add(document);
    start.ArgumentList.Add("number");
    using var worker = System.Diagnostics.Process.Start(start)!;
    await worker.WaitForExitAsync();
    Require(worker.ExitCode == 0);
    Require(files.All(file => File.GetLastWriteTimeUtc(file) == marker));

    var restore = new NpmValueMetadataProvider(store);
    var manifestFile = Path.Combine(project.FullName, "package.json");
    var lockFile = Path.Combine(project.FullName, "pnpm-lock.yaml");
    Require((await restore.RestoreAsync(project.FullName, store, default)).Contains("\"status\":\"skipped\""));
    Require((await restore.RestoreAsync(project.FullName, store, default)).Length == 0);
    await File.AppendAllTextAsync(manifestFile, "\n");
    Require((await restore.RestoreAsync(project.FullName, store, default)).Contains("\"status\":\"skipped\""));
    Require((await restore.RestoreAsync(project.FullName, store, default)).Length == 0);
    await File.AppendAllTextAsync(lockFile, "\n");
    Require((await restore.RestoreAsync(project.FullName, store, default)).Contains("\"status\":\"completed\""));
    try
    {
        await restore.RestoreAsync(project.FullName, store, new CancellationToken(true));
        throw new InvalidOperationException("A canceled restore returned successfully.");
    }
    catch (OperationCanceledException)
    {
    }

    var installed = Path.Combine(project.FullName, "node_modules", "cache-fixture");
    File.WriteAllText(Path.Combine(installed, "package.json"),
        manifest.Replace("./index", "./alternate", StringComparison.Ordinal));
    await Read(new NpmValueMetadataProvider(store), document, "string");
    Require(files.All(file => File.GetLastWriteTimeUtc(file) != marker));
    File.WriteAllText(Path.Combine(installed, "dep.ts"), "export declare const answer: boolean;");
    await Read(new NpmValueMetadataProvider(store), document, "boolean");
    File.Delete(Path.Combine(installed, "dep.ts"));
    await Read(new NpmValueMetadataProvider(store), document, "string");
    var provider = new NpmValueMetadataProvider(store);
    var linkedModules = Directory.CreateDirectory(Path.Combine(installed, "node_modules"));
    var dependencyLink = Path.Combine(linkedModules.FullName, "cache-dependency");
    foreach (var kind in new[] { "string", "boolean" })
    {
        var target = Directory.CreateDirectory(Path.Combine(project.FullName, kind)).FullName;
        File.WriteAllText(Path.Combine(target, "package.json"), """{"name":"cache-dependency","type":"module","types":"./index.d.ts"}""");
        File.WriteAllText(Path.Combine(target, "index.d.ts"), "export { answer } from './value.js';");
        File.WriteAllText(Path.Combine(target, "value.d.ts"), $"export declare const answer: {kind};");
    }
    File.WriteAllText(Path.Combine(installed, "alternate.d.ts"), "export { answer } from 'cache-dependency';");
    await CreateDirectoryLink(dependencyLink, Path.Combine(project.FullName, "string"));
    await Read(provider, document, "string");
    Directory.Delete(dependencyLink);
    await CreateDirectoryLink(dependencyLink, Path.Combine(project.FullName, "boolean"));
    await Read(new NpmValueMetadataProvider(store), document, "boolean");
    await Read(provider, document, "boolean");

    var requested = new[] { "answer", "later" };
    var missing = await provider.ReadValueProjectionsAsync(document, "cache-fixture", requested);
    Require(missing.Errors.ContainsKey("later"));
    Require(missing.Values is [{ ExportName: "answer", ConstantType.Kind: "boolean" }]);
    File.AppendAllText(Path.Combine(project.FullName, "boolean", "value.d.ts"),
        "\nexport declare const later: number;");
    File.WriteAllText(Path.Combine(installed, "alternate.d.ts"), "export { answer, later } from 'cache-dependency';");
    File.WriteAllText(Path.Combine(project.FullName, "boolean", "index.d.ts"), "export { answer, later } from './value.js';");
    foreach (var reader in new[] { new NpmValueMetadataProvider(store), provider })
    {
        var recovered = await reader.ReadValueProjectionsAsync(document, "cache-fixture", requested);
        Require(recovered.Errors.Count == 0);
        Require(recovered.Values is [
            { ExportName: "answer", ConstantType.Kind: "boolean" },
            { ExportName: "later", ConstantType.Kind: "number" },
        ]);
    }
    Console.WriteLine("Verified cross-process npm metadata cache reuse and invalidation on export, type, and linked dependency changes.");
}
finally
{
    project.Delete(recursive: true);
}

static async Task Read(NpmValueMetadataProvider provider, string document, string kind)
{
    Require((await provider.ListValueExportsAsync(document, "cache-fixture")) is ["answer"]);
    Require((await provider.ReadValueAsync(document, "cache-fixture", "answer")).ConstantType?.Kind == kind);
    Require((await provider.ReadValuesAsync(document, "cache-fixture", ["answer"])) is [{ ConstantType.Kind: var actual }] &&
        actual == kind);
}

static async Task CreateDirectoryLink(string link, string target)
{
    if (!OperatingSystem.IsWindows())
    {
        Directory.CreateSymbolicLink(link, target);
        return;
    }

    // The fixture creates both paths on one volume. A junction exercises realpath
    // invalidation without requiring Windows' symbolic-link privilege.
    var start = new ProcessStartInfo(Path.Combine(Environment.SystemDirectory, "cmd.exe"))
    {
        UseShellExecute = false,
        Arguments = "/d /v:off /c mklink /j \"%JAZOR_FIXTURE_LINK%\" \"%JAZOR_FIXTURE_TARGET%\"",
    };
    start.Environment["JAZOR_FIXTURE_LINK"] = link;
    start.Environment["JAZOR_FIXTURE_TARGET"] = target;
    using var process = Process.Start(start)!;
    await process.WaitForExitAsync();
    Require(process.ExitCode == 0);
    Require(string.Equals(Directory.ResolveLinkTarget(link, returnFinalTarget: true)?.FullName, target,
        StringComparison.OrdinalIgnoreCase));
}

static void Require(bool condition, [System.Runtime.CompilerServices.CallerArgumentExpression(nameof(condition))] string? check = null)
{
    if (!condition)
        throw new InvalidOperationException(check);
}
