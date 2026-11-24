#!/usr/bin/env dotnet run
#:property AssemblyName=Jazor.NodeApiVerification
#:property CETCompat=false
#:property PublishAot=false
#:project ../../src/Jazor.NodeHost/Jazor.NodeHost.csproj

using System.Collections;
using System.Diagnostics;
using System.IO.Compression;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text.Json;
using System.Xml.Linq;
using Jazor.NodeHost;

if (args.Length > 1)
    throw new ArgumentException("Usage: verify-jazor-npm-projection.cs [Jazor.nupkg or Jazor.Emit build output directory]");

var scripts = Path.GetDirectoryName(ScriptPath())!;
var repo = Path.GetFullPath(Path.Combine(scripts, "..", ".."));
var input = args.Length == 1 ? Path.GetFullPath(args[0]) : null;
var packaged = Path.GetExtension(input)?.Equals(".nupkg", StringComparison.OrdinalIgnoreCase) == true;
var analyzerDirectory = input ?? Path.Combine(repo, "src", "Jazor.Emit", "bin", "Debug", "net11.0");
var store = Path.GetFullPath(Environment.GetEnvironmentVariable("JazorPnpmStoreDirectory")
    ?? Path.Combine(repo, ".tmp", "nodeapi-store"));
foreach (var file in new[] { "Jazor.Compiler.dll", "ECMAScript.dll", "ECMAScript.Contract.dll" })
    if (!packaged && !File.Exists(Path.Combine(analyzerDirectory, file)))
        throw new FileNotFoundException($"Use a compiler tool build containing {file}.", Path.Combine(analyzerDirectory, file));

using var deadline = new CancellationTokenSource(TimeSpan.FromMinutes(2));
var stop = deadline.Token;
var scratch = Directory.CreateDirectory(Path.Combine(repo, ".tmp", $"jazor-npm-projection-{Guid.NewGuid():N}"));
Console.WriteLine($"Ordinary C# fixture: {scratch.FullName}");
try
{
    string? packageVersion = null;
    if (packaged)
    {
        using var archive = ZipFile.OpenRead(input!);
        using var spec = archive.GetEntry("Jazor.nuspec")!.Open();
        var nuspec = XElement.Load(spec);
        var ns = nuspec.Name.Namespace;
        packageVersion = nuspec.Element(ns + "metadata")!.Element(ns + "version")!.Value;
        new XElement("configuration",
            new XElement("packageSources", new XElement("clear"),
                Source("candidate", Path.GetDirectoryName(input!)!), Source("nuget.org", "https://api.nuget.org/v3/index.json")),
            new XElement("packageSourceMapping", Map("candidate", "Jazor"), Map("nuget.org", "*")))
            .Save(Path.Combine(scratch.FullName, "NuGet.Config"));
    }

    var projectFile = Path.Combine(scratch.FullName, "Calendar.csproj");
    var project = new XElement("Project", new XAttribute("Sdk", "Microsoft.NET.Sdk"),
        new XElement("PropertyGroup",
            new XElement("TargetFramework", "net11.0"),
            new XElement("LangVersion", "preview"),
            new XElement("Nullable", "enable"),
            new XElement("DefineConstants", "$(DefineConstants);PROJECTION_ACTIVE"),
            new XElement("TreatWarningsAsErrors", "true"),
            new XElement("JazorPnpmStoreDirectory", store)),
        packaged ? new XElement("ItemGroup",
            new XElement("PackageReference", new XAttribute("Include", "Jazor"),
                new XAttribute("Version", $"[{packageVersion}]"))) : new XElement("ItemGroup",
            Reference("ECMAScript"), Reference("ECMAScript.Contract"),
            new XElement("ProjectReference",
                new XAttribute("Include", Path.Combine(repo, "src", "Jazor.Emit", "Jazor.Emit.csproj")),
                new XAttribute("ReferenceOutputAssembly", "false")),
            new XElement("Analyzer", new XAttribute("Include", string.Join(';',
                new[] { "Jazor.Compiler.dll", "Jazor.Common.dll", "ECMAScript.Contract.dll", "Acornima*.dll", "ICSharpCode.Decompiler.dll" }
                    .Select(file => Path.Combine(analyzerDirectory, file)))))),
        packaged ? null : new XElement("Import",
            new XAttribute("Project", Path.Combine(repo, "src", "Jazor", "build", "Jazor.targets"))));
    project.Save(projectFile);

    await File.WriteAllTextAsync(Path.Combine(scratch.FullName, "package.json"),
        """{"private":true,"type":"module","dependencies":{"date-fns":"4.4.0"}}""", stop);
    await File.WriteAllTextAsync(Path.Combine(scratch.FullName, "pnpm-lock.yaml"), """
        lockfileVersion: '9.0'

        settings:
          autoInstallPeers: true
          excludeLinksFromLockfile: false

        importers:
          .:
            dependencies:
              date-fns:
                specifier: 4.4.0
                version: 4.4.0

        packages:
          date-fns@4.4.0:
            resolution: {integrity: sha512-+1UMbeh68lH1SegH83CGWwpb6OHHbpSgr3+s5Eww5M4CAgswBpoWS0AjTOfEJ33HiYKz1hdj/KTFprzXHmq/6w==}

        snapshots:
          date-fns@4.4.0: {}
        """, stop);

    var sourceFile = Path.Combine(scratch.FullName, "Calendar.cs");
    const string validSource = """
        using System;
        using ECMAScript;
        using Dates = Npm.DateFns.Exports;
        #if PROJECTION_ACTIVE && NET11_0
        using static Npm.DateFns.Exports;
        #else
        using static Npm.DoesNotExist.Exports;
        #endif

        [ECMAScriptModule("calendar.mjs")]
        public static class Calendar
        {
            private sealed record Shift(Date Start, double Days);

            public static double Run()
            {
                var shift = new Shift(new Date(2024, 0, 31), 2);
                return Read(shift, value => Dates.AddDays(value.Start, value.Days).GetDate());
            }

            private static double Read(Shift shift, Func<Shift, double> calculate) => calculate(shift);
        }
        """;
    await File.WriteAllTextAsync(sourceFile, validSource, stop);
    var cold = await BuildAsync();
    if (cold.ExitCode != 0)
        throw new InvalidOperationException("Cold ordinary C# build failed." + Environment.NewLine + cold.Output);
    if (packaged)
    {
        using var assets = JsonDocument.Parse(await File.ReadAllTextAsync(Path.Combine(scratch.FullName, "obj", "project.assets.json"), stop));
        var restored = assets.RootElement.GetProperty("libraries").GetProperty("Jazor/" + packageVersion).GetProperty("sha512").GetString();
        using var package = File.OpenRead(input!);
        if (restored != Convert.ToBase64String(await SHA512.HashDataAsync(package, stop)))
            throw new InvalidOperationException("The fixture restored a different Jazor package with the same version.");
    }

    var projectionFile = Path.Combine(scratch.FullName, "obj", "Debug", "net11.0", "Calendar.npm.g.cs");
    var assemblyFile = Path.Combine(scratch.FullName, "bin", "Debug", "net11.0", "Calendar.dll");
    var requestFile = Path.Combine(Path.GetDirectoryName(projectionFile)!, "Calendar.npm.inputs");
    var stableFiles = new[] { projectionFile, requestFile, assemblyFile };
    var originalTimestamps = stableFiles.Select(File.GetLastWriteTimeUtc).ToArray();
    var unchanged = await BuildAsync("--no-restore");
    if (unchanged.ExitCode != 0 || !stableFiles.Select(File.GetLastWriteTimeUtc).SequenceEqual(originalTimestamps))
        throw new InvalidOperationException("An unchanged build rewrote the npm projection, SDK request or compiled assembly." + Environment.NewLine + unchanged.Output);

    var manifest = Path.Combine(scratch.FullName, "package.json");
    var backupManifest = manifest + ".saved";
    File.Move(manifest, backupManifest);
    var withoutManifest = await BuildAsync("--no-restore");
    if (withoutManifest.ExitCode == 0 || !withoutManifest.Output.Contains("CS0246", StringComparison.Ordinal))
        throw new InvalidOperationException("Removing the npm manifest must invalidate the prior projection and reject the unresolved namespace." +
            Environment.NewLine + withoutManifest.Output);
    File.Move(backupManifest, manifest);
    var restoredManifest = await BuildAsync("--no-restore");
    if (restoredManifest.ExitCode != 0 || !File.Exists(projectionFile))
        throw new InvalidOperationException("Restoring the npm manifest did not restore the projection." + Environment.NewLine + restoredManifest.Output);

    var nested = scratch.CreateSubdirectory("nested").FullName;
    var nestedProject = Path.Combine(nested, "Calendar.csproj");
    File.Copy(projectFile, nestedProject);
    File.Copy(sourceFile, Path.Combine(nested, "Calendar.cs"));
    var nestedManifest = Path.Combine(nested, "package.json");
    var inherited = await RunAsync(nested, ["build", nestedProject, "--nologo"]);
    if (inherited.ExitCode != 0)
        throw new InvalidOperationException("The nested project did not find the parent manifest." + Environment.NewLine + inherited.Output);
    await File.WriteAllTextAsync(nestedManifest, """{"private":true}""", stop);
    var shadowed = await RunAsync(nested, ["build", nestedProject, "--no-restore", "--nologo"]);
    if (shadowed.ExitCode == 0 || !shadowed.Output.Contains("CS0246", StringComparison.Ordinal))
        throw new InvalidOperationException("A nearer empty manifest must shadow the parent projection." + Environment.NewLine + shadowed.Output);
    File.Delete(nestedManifest);
    var fallback = await RunAsync(nested, ["build", nestedProject, "--no-restore", "--nologo"]);
    if (fallback.ExitCode != 0)
        throw new InvalidOperationException("Removing the shadowing manifest did not restore the parent projection." + Environment.NewLine + fallback.Output);
    Directory.Delete(nested, recursive: true);

    var invalidSource = validSource.Replace("value.Start, value.Days", "value.Start, true", StringComparison.Ordinal);
    await File.WriteAllTextAsync(sourceFile, invalidSource, stop);
    var rejected = await BuildAsync("--no-restore");
    if (rejected.ExitCode == 0 || !rejected.Output.Contains("Calendar.cs(", StringComparison.Ordinal)
        || !rejected.Output.Contains("CS1503", StringComparison.Ordinal))
        throw new InvalidOperationException("Expected a C# argument type diagnostic at the ordinary source call." + Environment.NewLine + rejected.Output);

    await File.WriteAllTextAsync(sourceFile, validSource, stop);
    var recovered = await BuildAsync("--no-restore");
    if (recovered.ExitCode != 0)
        throw new InvalidOperationException("Ordinary C# build did not recover after correcting the argument." + Environment.NewLine + recovered.Output);

    var emittedAssembly = File.ReadAllBytes(assemblyFile);
    var assembly = Assembly.Load(emittedAssembly);
    var catalog = assembly.GetType("Jazor.Generated.ModuleCatalog", throwOnError: true)!;
    var modules = (IEnumerable)catalog.GetMethod("GetModules", BindingFlags.NonPublic | BindingFlags.Static)!.Invoke(null, null)!;
    var module = modules.Cast<object>().Single(item =>
        Equals(item.GetType().GetProperty("RelativePath")!.GetValue(item), "calendar.mjs"));
    var content = (string)module.GetType().GetProperty("Content")!.GetValue(module)!;
    if (!content.Contains("from \"date-fns\"", StringComparison.Ordinal))
        throw new InvalidOperationException("Expected the emitted calendar module to import the real date-fns package." + Environment.NewLine + content);

    await File.WriteAllTextAsync(Path.Combine(scratch.FullName, "calendar.mjs"), content, stop);
    var verifier = Path.Combine(scratch.FullName, "verify.mjs");
    await File.WriteAllTextAsync(verifier, """
        import { Run } from './calendar.mjs';
        const actual = Run();
        if (actual !== 2) throw new Error(`Expected February 2 after shifting January 31 by two days, got ${actual}.`);
        console.log('Ordinary C# npm projection executed the record, callback and date-fns import through NodeApi.');
        """, stop);
    Console.WriteLine(await NodeModuleRunner.RunAsync(scratch.FullName, verifier, cancellationToken: stop));
    const string withoutProjection = """
        #if USE_NPM_DATES
        using static Npm.DateFns.Exports;
        #endif

        public static class Calendar
        {
        #if USE_NPM_DATES
            public static double Run() => AddDays(new ECMAScript.Date(2024, 0, 31), 2).GetDate();
        #else
            public static double Run() => 2;
        #endif
        }
        """;
    await File.WriteAllTextAsync(sourceFile, withoutProjection, stop);
    var excluded = await BuildAsync("--no-restore");
    if (excluded.ExitCode != 0 || File.Exists(projectionFile) ||
        excluded.Output.Contains("[Jazor NodeApi]", StringComparison.Ordinal))
        throw new InvalidOperationException("Disabling the last npm import should remove its projection without starting NodeApi." +
            Environment.NewLine + excluded.Output);

    var included = await BuildAsync("--no-restore", "-p:DefineConstants=USE_NPM_DATES");
    if (included.ExitCode != 0 || !File.Exists(projectionFile))
        throw new InvalidOperationException("Enabling the conditional npm import should regenerate its projection." +
            Environment.NewLine + included.Output);

    File.Delete(sourceFile);
    await File.WriteAllTextAsync(Path.Combine(scratch.FullName, "Plain.cs"),
        "public static class Plain { public const string Name = \"Npm\"; }", stop);
    await File.WriteAllTextAsync(Path.Combine(scratch.FullName, "package.json"), "{invalid}", stop);
    var removed = await BuildAsync("--no-restore", "-p:DefineConstants=USE_NPM_DATES");
    if (removed.ExitCode != 0 || File.Exists(projectionFile) || File.ReadAllBytes(assemblyFile).SequenceEqual(emittedAssembly) ||
        removed.Output.Contains("[Jazor NodeApi]", StringComparison.Ordinal))
        throw new InvalidOperationException("Removing the last npm import should delete its projection without starting NodeApi." +
            Environment.NewLine + removed.Output);

    var unchangedWithoutImports = await BuildAsync("--no-restore", "-v:n", "-p:DefineConstants=USE_NPM_DATES");
    var noImportsFile = Path.Combine(Path.GetDirectoryName(projectionFile)!, "Calendar.npm.no-imports");
    if (unchangedWithoutImports.ExitCode != 0 || !File.Exists(noImportsFile) ||
        unchangedWithoutImports.Output.Contains("--sdk-npm-inputs", StringComparison.Ordinal))
        throw new InvalidOperationException("An unchanged build without npm imports should skip the projection tool." +
            Environment.NewLine + unchangedWithoutImports.Output);

    Console.WriteLine("Unchanged, conditional and removed npm imports passed SDK build verification.");

    if (packaged)
    {
        var plain = scratch.CreateSubdirectory("plain").FullName;
        File.Delete(Path.Combine(scratch.FullName, "package.json"));
        File.Delete(Path.Combine(scratch.FullName, "pnpm-lock.yaml"));
        var plainProject = Path.Combine(plain, "Plain.csproj");
        new XElement("Project", new XAttribute("Sdk", "Microsoft.NET.Sdk"),
            new XElement("PropertyGroup", new XElement("TargetFramework", "net11.0"),
                new XElement("OutputType", "Exe"), new XElement("LangVersion", "preview"),
                new XElement("JazorPnpmStoreDirectory", Path.Combine(plain, "unused-store"))),
            new XElement("ItemGroup", new XElement("PackageReference", new XAttribute("Include", "Jazor"),
                new XAttribute("Version", $"[{packageVersion}]")))).Save(plainProject);
        await File.WriteAllTextAsync(Path.Combine(plain, "Program.cs"), """
            System.Console.WriteLine("Plain Jazor host started.");
            """, stop);
        var published = Path.Combine(plain, "published");
        foreach (var (label, command) in new[]
        {
            ("cold build", new[] { "build", plainProject, "--nologo", "-v:n" }),
            ("publish", new[] { "publish", plainProject, "--no-restore", "--nologo", "-v:n", "--output", published }),
            ("startup", new[] { Path.Combine(published, "Plain.dll") }),
        })
        {
            var result = await RunAsync(plain, command);
            if (result.ExitCode != 0 || result.Output.Contains("--sdk-npm-inputs", StringComparison.Ordinal) ||
                result.Output.Contains("[Jazor NodeApi]", StringComparison.Ordinal) ||
                label == "startup" && !result.Output.Contains("Plain Jazor host started.", StringComparison.Ordinal))
                throw new InvalidOperationException($"Manifest-free host failed {label}." + Environment.NewLine + result.Output);
        }
        if (new[] { "package.json", "pnpm-lock.yaml", "node_modules", "unused-store" }.Any(name => Path.Exists(Path.Combine(plain, name))) ||
            Directory.EnumerateFiles(plain, "*.npm.*", SearchOption.AllDirectories).Any())
            throw new InvalidOperationException("The manifest-free host created JavaScript tooling or SDK projection inputs or outputs.");
        Console.WriteLine("Manifest-free Jazor executable built, published and started without npm tooling or generated projections.");
    }
}
catch
{
    Console.Error.WriteLine($"Fixture preserved at {scratch.FullName}");
    throw;
}

scratch.Delete(recursive: true);

XElement Reference(string name) => new("Reference", new XAttribute("Include", name),
    new XAttribute("HintPath", Path.Combine(analyzerDirectory, name + ".dll")));

static XElement Source(string key, string value) => new("add", new XAttribute("key", key), new XAttribute("value", value));

static XElement Map(string key, string pattern) => new("packageSource", new XAttribute("key", key),
    new XElement("package", new XAttribute("pattern", pattern)));

Task<(int ExitCode, string Output)> BuildAsync(params string[] options)
    => RunAsync(scratch.FullName, new[] { "build", "Calendar.csproj", "--nologo" }.Concat(options));

async Task<(int ExitCode, string Output)> RunAsync(string directory, IEnumerable<string> arguments)
{
    var start = new ProcessStartInfo("dotnet")
    {
        WorkingDirectory = directory,
        RedirectStandardOutput = true,
        RedirectStandardError = true,
        UseShellExecute = false,
    };
    foreach (var argument in arguments)
        start.ArgumentList.Add(argument);
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
    return (process.ExitCode, await stdout + await stderr);
}

static string ScriptPath([CallerFilePath] string path = "") => path;
