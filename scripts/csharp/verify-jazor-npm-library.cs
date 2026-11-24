#!/usr/bin/env dotnet run
#:property AssemblyName=Jazor.NodeApiVerification
#:property CETCompat=false
#:property PublishAot=false
#:project ../../src/Jazor.NodeHost/Jazor.NodeHost.csproj

using System.Collections;
using System.Diagnostics;
using System.IO.Compression;
using System.Reflection;
using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text.Json;
using System.Xml.Linq;
using Jazor.NodeHost;

if (args is not [var artifact])
    throw new ArgumentException("Usage: verify-jazor-npm-library.cs <Jazor.nupkg or Jazor.Emit build output directory>");

var input = Path.GetFullPath(artifact);
var packaged = Path.GetExtension(input).Equals(".nupkg", StringComparison.OrdinalIgnoreCase);
var scripts = Path.GetDirectoryName(ScriptPath())!;
var repo = Path.GetFullPath(Path.Combine(scripts, "..", ".."));
var store = Environment.GetEnvironmentVariable("JazorPnpmStoreDirectory") ?? Path.Combine(repo, ".tmp", "nodeapi-store");
var fixture = Path.Combine(scripts, "fixtures", "jazor-npm-library");
var scratch = Directory.CreateDirectory(Path.Combine(repo, ".tmp", $"jazor-npm-library-{Guid.NewGuid():N}")).FullName;
using var deadline = new CancellationTokenSource(TimeSpan.FromMinutes(2));
var stop = deadline.Token;
Console.WriteLine($"Ordinary C# npm library fixture: {scratch}");
foreach (var file in Directory.EnumerateFiles(fixture, "*", SearchOption.AllDirectories))
{
    var destination = Path.Combine(scratch, Path.GetRelativePath(fixture, file));
    Directory.CreateDirectory(Path.GetDirectoryName(destination)!);
    File.Copy(file, destination);
}

string? packageVersion = null;
if (packaged)
{
    using var archive = ZipFile.OpenRead(input);
    using var spec = archive.GetEntry("Jazor.nuspec")!.Open();
    var nuspec = XElement.Load(spec);
    var ns = nuspec.Name.Namespace;
    packageVersion = nuspec.Element(ns + "metadata")!.Element(ns + "version")!.Value;
    new XElement("configuration",
        new XElement("packageSources", new XElement("clear"),
            Source("candidate", Path.GetDirectoryName(input)!), Source("nuget.org", "https://api.nuget.org/v3/index.json")),
        new XElement("packageSourceMapping", Map("candidate", "Jazor"), Map("nuget.org", "*")))
        .Save(Path.Combine(scratch, "NuGet.Config"));

    static XElement Source(string key, string value) => new("add", new XAttribute("key", key), new XAttribute("value", value));
    static XElement Map(string key, string pattern) => new("packageSource", new XAttribute("key", key),
        new XElement("package", new XAttribute("pattern", pattern)));
}
WriteProject("CellLibrary");
WriteProject("CellConsumer", projectReferences: ["CellLibrary"]);
await BuildAsync("CellConsumer");
if (packaged)
{
    using var assets = JsonDocument.Parse(await File.ReadAllTextAsync(Path.Combine(scratch, "CellConsumer", "obj", "project.assets.json"), stop));
    var restored = assets.RootElement.GetProperty("libraries").GetProperty("Jazor/" + packageVersion).GetProperty("sha512").GetString();
    using var package = File.OpenRead(input);
    if (restored != Convert.ToBase64String(await SHA512.HashDataAsync(package, stop)))
        throw new InvalidOperationException("The fixture restored a different Jazor package with the same version.");
}

await WriteModuleAsync("CellLibrary", "cell-library.mjs", "wire-cells");
await WriteModuleAsync("CellConsumer", "cell-consumer.mjs", "./cell-library.mjs", "cell-ops", "wire-cells");
WriteProject("CellProducerA");
WriteProject("CellProducerB");
await BuildAsync("CellProducerA");
await BuildAsync("CellProducerB");
(string Assembly, string Alias)[] independentProducers = [("CellProducerA", "First"), ("CellProducerB", "Second")];
const string ambiguous = "GlobalCellConsumer";
const string ambiguousSource = """
    public static class GlobalCellConsumer
    {
        public static void Read() => Npm.WireCells.Exports.MakeCell("ambiguous");
    }
    """;
await File.WriteAllTextAsync(Path.Combine(scratch, ambiguous + ".cs"), ambiguousSource, stop);
WriteProject(ambiguous, aliases: independentProducers.Select(reference => (reference.Assembly, "global")).ToArray());
var ambiguityReport = Path.Combine(scratch, "ambiguous.sarif");
await BuildAsync(ambiguous, ambiguityReport);
using (var ambiguity = JsonDocument.Parse(await File.ReadAllTextAsync(ambiguityReport, stop)))
{
    var error = ambiguity.RootElement.GetProperty("runs")[0].GetProperty("results").EnumerateArray()
        .Single(result => result.GetProperty("level").GetString() == "error");
    var location = error.GetProperty("locations")[0].GetProperty("resultFile");
    var span = location.GetProperty("region");
    var line = ambiguousSource.Split('\n')[span.GetProperty("startLine").GetInt32() - 1];
    var token = line[(span.GetProperty("startColumn").GetInt32() - 1)..(span.GetProperty("endColumn").GetInt32() - 1)];
    var message = error.GetProperty("message").GetString()!;
    if (error.GetProperty("ruleId").GetString() != "CS0433" || token != "Npm.WireCells.Exports" ||
        !location.GetProperty("uri").GetString()!.EndsWith(ambiguous + ".cs", StringComparison.Ordinal) ||
        !message.Contains("CellProducerA", StringComparison.Ordinal) || !message.Contains("CellProducerB", StringComparison.Ordinal))
        throw new InvalidOperationException($"Expected CS0433 at Exports naming both producers: {error}");
}
Console.WriteLine("Roslyn rejected the globally referenced duplicate npm facade at the call (CS0433).");
WriteProject("AliasedCellConsumer", aliases: independentProducers);
await BuildAsync("AliasedCellConsumer");

await WriteModuleAsync("CellProducerA", "cell-producer-a.mjs", "wire-cells");
await WriteModuleAsync("CellProducerB", "cell-producer-b.mjs", "wire-cells", "cell-ops");
var aliased = await WriteModuleAsync("AliasedCellConsumer", "aliased-cell-consumer.mjs",
    "wire-cells", "./cell-producer-a.mjs", "./cell-producer-b.mjs");
if (!aliased.Contains("cellWeights", StringComparison.Ordinal))
    throw new InvalidOperationException($"Expected the local projection to import cellWeights.{Environment.NewLine}{aliased}");

WriteProject("CellContracts");
WriteProject("SharedCellProducerA", projectReferences: ["CellContracts"]);
WriteProject("SharedCellProducerB", projectReferences: ["CellContracts"]);
WriteProject("SharedCellConsumer", projectReferences: ["SharedCellProducerA", "SharedCellProducerB"]);
await BuildAsync("SharedCellConsumer");
ExpectContracts();
ExpectContractOwner("SharedCellProducerA");
ExpectContractOwner("SharedCellProducerB");
ExpectContractOwner("SharedCellConsumer");
await WriteModuleAsync("SharedCellProducerA", "shared-cell-producer-a.mjs", "wire-cells");
await WriteModuleAsync("SharedCellProducerB", "shared-cell-producer-b.mjs", "wire-cells");
await WriteModuleAsync("SharedCellConsumer", "shared-cell-consumer.mjs",
    "./shared-cell-producer-a.mjs", "./shared-cell-producer-b.mjs");
Console.WriteLine(await NodeModuleRunner.RunAsync(scratch, Path.Combine(scratch, "verify.mjs"), cancellationToken: stop));

// Both producers independently project Cell<T>. CLR keeps their assembly identities
// even when the npm declaration, generated C# name and JavaScript shape are identical.
const string mismatch = "CrossAssemblyMismatch";
const string wrongArgument = """First::CellProducerA.Create("foreign")""";
const string mismatchSource = """
    extern alias First;
    extern alias Second;
    public static class CrossAssemblyMismatch
    {
        public static string WrongOwner() => Second::CellProducerB.Label(First::CellProducerA.Create("foreign"));
    }
    """;
await File.WriteAllTextAsync(Path.Combine(scratch, mismatch + ".cs"), mismatchSource, stop);
WriteProject(mismatch, aliases: independentProducers);
var sarif = Path.Combine(scratch, "mismatch.sarif");
await BuildAsync(mismatch, sarif);
using var report = JsonDocument.Parse(await File.ReadAllTextAsync(sarif, stop));
var errors = report.RootElement.GetProperty("runs")[0].GetProperty("results").EnumerateArray()
    .Where(result => result.GetProperty("level").GetString() == "error").ToArray();
if (errors.Length != 1 || errors[0].GetProperty("ruleId").GetString() != "CS1503")
    throw new InvalidOperationException($"Expected a single CS1503 for the cross-assembly record argument: {string.Join(", ", errors)}");
var site = errors[0].GetProperty("locations")[0].GetProperty("resultFile");
var region = site.GetProperty("region");
var failingLine = mismatchSource.Split('\n')[region.GetProperty("startLine").GetInt32() - 1];
if (!site.GetProperty("uri").GetString()!.EndsWith(mismatch + ".cs", StringComparison.Ordinal) ||
    failingLine[(region.GetProperty("startColumn").GetInt32() - 1)..(region.GetProperty("endColumn").GetInt32() - 1)] != wrongArgument)
    throw new InvalidOperationException("CS1503 did not point to the argument produced by the other DLL.");
Console.WriteLine("Roslyn rejected the independently declared record at the typed DLL boundary (CS1503).");
Directory.Delete(scratch, recursive: true);

void WriteProject(string name, string[]? projectReferences = null, (string Assembly, string Alias)[]? aliases = null)
{
    var directory = Directory.CreateDirectory(Path.Combine(scratch, name)).FullName;
    new XElement("Project", new XAttribute("Sdk", "Microsoft.NET.Sdk"),
        new XElement("PropertyGroup",
            new XElement("TargetFramework", "net11.0"),
            new XElement("LangVersion", "preview"),
            new XElement("Nullable", "enable"),
            new XElement("EnableDefaultCompileItems", "false"),
            new XElement("TreatWarningsAsErrors", "true"),
            new XElement("JazorPnpmStoreDirectory", store)),
        new XElement("ItemGroup",
            new XElement("Compile", new XAttribute("Include", $"../{name}.cs")),
            aliases?.Select(reference => new XElement("Reference", new XAttribute("Include", reference.Assembly),
                new XElement("HintPath", AssemblyPath(reference.Assembly)),
                new XElement("Aliases", reference.Alias))),
            projectReferences?.Select(project => new XElement("ProjectReference",
                new XAttribute("Include", $"../{project}/{project}.csproj")))),
        packaged ? new XElement("ItemGroup", new XElement("PackageReference",
            new XAttribute("Include", "Jazor"), new XAttribute("Version", "[" + packageVersion + "]"))) :
        new XElement("ItemGroup",
            Reference("ECMAScript"), Reference("ECMAScript.Contract"),
            new XElement("ProjectReference",
                new XAttribute("Include", Path.Combine(repo, "src", "Jazor.Emit", "Jazor.Emit.csproj")),
                new XAttribute("ReferenceOutputAssembly", "false")),
            new XElement("Analyzer", new XAttribute("Include", string.Join(';',
                new[] { "Jazor.Compiler.dll", "Jazor.Common.dll", "ECMAScript.Contract.dll", "Acornima*.dll", "ICSharpCode.Decompiler.dll" }
                    .Select(file => Path.Combine(input, file)))))),
        packaged ? null : new XElement("Import", new XAttribute("Project", Path.Combine(repo, "src", "Jazor", "build", "Jazor.targets"))))
        .Save(Path.Combine(directory, name + ".csproj"));
}

XElement Reference(string name) => new("Reference", new XAttribute("Include", name),
    new XAttribute("HintPath", Path.Combine(input, name + ".dll")));

string ReadModule(string project, string modulePath)
{
    var assembly = Assembly.Load(File.ReadAllBytes(AssemblyPath(project)));
    var unexpected = assembly.GetReferencedAssemblies().Select(reference => reference.Name!)
        .Where(name => name.StartsWith("ECMAScript.Vue", StringComparison.Ordinal) ||
                       name == "Jolt" || name.StartsWith("Jolt.", StringComparison.Ordinal))
        .ToArray();
    if (unexpected.Length != 0)
        throw new InvalidOperationException($"{project} references unexpected assemblies: {string.Join(", ", unexpected)}.");
    var catalog = assembly.GetType("Jazor.Generated.ModuleCatalog", throwOnError: true)!;
    var modules = (IEnumerable)catalog.GetMethod("GetModules", BindingFlags.NonPublic | BindingFlags.Static)!.Invoke(null, null)!;
    var module = modules.Cast<object>().Single(item =>
        Equals(item.GetType().GetProperty("RelativePath")!.GetValue(item), modulePath));
    return (string)module.GetType().GetProperty("Content")!.GetValue(module)!;
}

string AssemblyPath(string project) => Path.Combine(scratch, project, "bin", "Debug", "net11.0", project + ".dll");

void ExpectContracts()
{
    using var stream = File.OpenRead(AssemblyPath("CellContracts"));
    using var pe = new PEReader(stream);
    var metadata = pe.GetMetadataReader();
    bool HasName(StringHandle name, string value) => metadata.StringComparer.Equals(name, value);
    var types = metadata.TypeDefinitions.Select(metadata.GetTypeDefinition).ToArray();
    var shapes = types.Single(type => HasName(type.Namespace, "Jolt.Generated.Npm") && HasName(type.Name, "Exports"));
    var cell = shapes.GetNestedTypes().Select(metadata.GetTypeDefinition)
        .Single(type => HasName(type.Name, "Cell_A357BBB40AF5`1"));
    var exports = types.Single(type => HasName(type.Namespace, "Npm.WireCells") && HasName(type.Name, "Exports"));
    var publicMethods = exports.GetMethods().Select(metadata.GetMethodDefinition)
        .Where(method => (method.Attributes & MethodAttributes.MemberAccessMask) == MethodAttributes.Public)
        .Select(method => metadata.GetString(method.Name)).ToHashSet(StringComparer.Ordinal);
    if ((shapes.Attributes & TypeAttributes.VisibilityMask) != TypeAttributes.Public ||
        (cell.Attributes & TypeAttributes.VisibilityMask) != TypeAttributes.NestedPublic ||
        (exports.Attributes & TypeAttributes.VisibilityMask) != TypeAttributes.Public ||
        !publicMethods.SetEquals(["MakeCell", "ReadLabel"]) ||
        types.Any(type => HasName(type.Namespace, "Jazor.Generated") && HasName(type.Name, "ModuleCatalog")))
        throw new InvalidOperationException("CellContracts must expose the shared record and the two selected npm methods without emitting a module catalog.");
}

void ExpectContractOwner(string project)
{
    const string cellName = "Cell_A357BBB40AF5`1";
    using var stream = File.OpenRead(AssemblyPath(project));
    using var pe = new PEReader(stream);
    var metadata = pe.GetMetadataReader();
    bool HasName(StringHandle name, string value) => metadata.StringComparer.Equals(name, value);
    var cell = metadata.TypeReferences.Select(metadata.GetTypeReference).Single(type => HasName(type.Name, cellName));
    if (metadata.TypeDefinitions.Select(metadata.GetTypeDefinition).Any(type => HasName(type.Name, cellName)) ||
        cell.ResolutionScope.Kind != HandleKind.TypeReference)
        throw new InvalidOperationException($"{project} must reference the owner's nested Cell<T> type.");
    var exports = metadata.GetTypeReference((TypeReferenceHandle)cell.ResolutionScope);
    if (!HasName(exports.Name, "Exports") || !HasName(exports.Namespace, "Jolt.Generated.Npm") ||
        exports.ResolutionScope.Kind != HandleKind.AssemblyReference ||
        !HasName(metadata.GetAssemblyReference((AssemblyReferenceHandle)exports.ResolutionScope).Name, "CellContracts"))
        throw new InvalidOperationException($"{project} must resolve Cell<T> through the CellContracts AssemblyRef.");
}

static void ExpectImport(string content, string module)
{
    if (!content.Contains($"from \"{module}\"", StringComparison.Ordinal))
        throw new InvalidOperationException($"Expected to import {module}.{Environment.NewLine}{content}");
}

async Task<string> WriteModuleAsync(string project, string module, params string[] imports)
{
    var content = ReadModule(project, module);
    foreach (var import in imports)
        ExpectImport(content, import);
    await File.WriteAllTextAsync(Path.Combine(scratch, module), content, stop);
    return content;
}

async Task BuildAsync(string project, string? errorLog = null)
{
    var start = new ProcessStartInfo("dotnet")
    {
        WorkingDirectory = scratch,
        RedirectStandardOutput = true,
        RedirectStandardError = true,
        UseShellExecute = false,
    };
    foreach (var argument in new[] { "build", Path.Combine(project, project + ".csproj"), "--nologo" })
        start.ArgumentList.Add(argument);
    if (errorLog is not null)
        start.ArgumentList.Add("-p:ErrorLog=" + errorLog);
    using var process = Process.Start(start)!;
    var stdout = process.StandardOutput.ReadToEndAsync(stop);
    var stderr = process.StandardError.ReadToEndAsync(stop);
    try
    {
        await process.WaitForExitAsync(stop);
    }
    catch (OperationCanceledException)
    {
        if (!process.HasExited)
            process.Kill(entireProcessTree: true);
        throw;
    }
    var output = await stdout + await stderr;
    if (process.ExitCode != (errorLog is null ? 0 : 1))
        throw new InvalidOperationException($"{project} build returned {process.ExitCode}.{Environment.NewLine}{output}");
}

static string ScriptPath([CallerFilePath] string path = "") => path;
