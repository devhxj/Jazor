using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text.Json;
using Jazor.Common;
using Jazor.Emit;

namespace Jazor.EmitTest;

[TestClass]
public sealed class StandardPackageProjectTests
{
    [TestMethod]
    public async Task ProjectBuild_UsesAuthoredScriptAndOutputLayout_AndPropagatesFailure()
    {
        var root = Path.Combine(Path.GetTempPath(), "Jazor.ProjectBuildTest", Guid.NewGuid().ToString("N"));
        var deno = Path.Combine(AppContext.BaseDirectory, "runtimes", RuntimeInformation.RuntimeIdentifier,
            "native", OperatingSystem.IsWindows() ? "deno.exe" : "deno");
        Directory.CreateDirectory(root);
        try
        {
            File.WriteAllText(Path.Combine(root, "package.json"), """
                {"type":"module","scripts":{"build":"deno run --allow-write build.js"}}
                """);
            File.WriteAllText(Path.Combine(root, "build.js"), "await Deno.writeTextFile('custom-output.js', 'export const built = true;');");
            var builder = new JavaScriptProjectBuilder();
            var built = await builder.BuildAsync(root, deno);
            Assert.IsTrue(built.IsSuccess, built.Diagnostic?.Message);
            Assert.IsTrue(File.Exists(Path.Combine(root, "custom-output.js")));
            Assert.IsFalse(Directory.Exists(Path.Combine(root, "dist")));
            File.WriteAllText(Path.Combine(root, "build.js"), "console.error('authored build failed'); Deno.exit(7);");
            var failed = await builder.BuildAsync(root, deno);
            Assert.IsFalse(failed.IsSuccess);
            Assert.AreEqual(7, failed.ExitCode);
            StringAssert.Contains(failed.Diagnostic!.Message, "authored build failed");
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    [TestMethod]
    public void Regeneration_PreservesAuthoredBuildToolAndUnmanagedDependencies()
    {
        var root = Path.Combine(Path.GetTempPath(), "Jazor.EmitTest", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        try
        {
            File.WriteAllText(Path.Combine(root, "package.json"), """
                {"name":"custom-project","type":"module",
                 "scripts":{"build":"custom-tool build","dev":"custom-tool serve","ssr":"deno run custom-server.js"},
                 "devDependencies":{"custom-tool":"1.0.0"},
                 "dependencies":{"authored":"2.0.0","retired":"1.0.0"},
                 "jazor":{"packages":{"retired":{"source":"npm","version":"1.0.0"}}}}
                """);
            var libraries = new LibraryAssets(
                new Dictionary<string, string>(), new Dictionary<string, string>(),
                [], [], [], [], [], [],
                new Dictionary<string, LibraryPackageReference> { ["generated"] = new("generated", "3.0.0", "npm") });
            LibraryPackageWriter.WritePackageProject(root, libraries, hasSsrEntry: true);
            LibraryPackageWriter.WritePackageProject(root, libraries, hasSsrEntry: true);
            using var package = JsonDocument.Parse(File.ReadAllText(Path.Combine(root, "package.json")));
            Assert.AreEqual("custom-project", package.RootElement.GetProperty("name").GetString());
            Assert.AreEqual("custom-tool build", package.RootElement.GetProperty("scripts").GetProperty("build").GetString());
            Assert.AreEqual("deno run custom-server.js", package.RootElement.GetProperty("scripts").GetProperty("ssr").GetString());
            Assert.AreEqual("1.0.0", package.RootElement.GetProperty("devDependencies").GetProperty("custom-tool").GetString());
            Assert.IsFalse(package.RootElement.GetProperty("devDependencies").TryGetProperty("vite", out _));
            Assert.AreEqual("2.0.0", package.RootElement.GetProperty("dependencies").GetProperty("authored").GetString());
            Assert.AreEqual("3.0.0", package.RootElement.GetProperty("dependencies").GetProperty("generated").GetString());
            Assert.IsFalse(package.RootElement.GetProperty("dependencies").TryGetProperty("retired", out _));
            Assert.IsFalse(File.Exists(Path.Combine(root, "vite.config.js")));
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    [TestMethod]
    public async Task NpmAndJsr_DeclaredKeysResolveThroughDenoAndViteWithoutPrivateManifests()
    {
        var root = Path.Combine(Path.GetTempPath(), "Jazor.EmitTest", "standard-project", Guid.NewGuid().ToString("N"));
        var deno = Path.Combine(AppContext.BaseDirectory, "runtimes", RuntimeInformation.RuntimeIdentifier,
            "native", OperatingSystem.IsWindows() ? "deno.exe" : "deno");
        Assert.IsTrue(File.Exists(deno), "The test must use the Deno runtime from its own build output.");
        using var timeout = new CancellationTokenSource(TimeSpan.FromMinutes(2));
        Directory.CreateDirectory(root);
        try
        {
            const string source = """
                import { addDays } from "date-fns/addDays";
                import { join } from "@jsr/std__path/posix";
                console.log(join("npm", String(addDays(new Date(2020, 0, 1), 2).getDate())));
                """;
            var appPath = Path.Combine(root, "app.js");
            await File.WriteAllTextAsync(appPath, source, timeout.Token);
            var libraries = new LibraryAssets(
                new Dictionary<string, string>(), new Dictionary<string, string>(),
                [], [], [], [], [], [],
                new Dictionary<string, LibraryPackageReference>
                {
                    ["date-fns"] = new("date-fns", "4.1.0", "npm"),
                    ["@jsr/std__path"] = new("@jsr/std__path", "jsr:@std/path@1.0.8", "jsr")
                });
            LibraryPackageWriter.WritePackageProject(root, libraries);
            var entries = ProjectEntryWriter.Write(root, ["app.js"]);
            using var package = JsonDocument.Parse(await File.ReadAllTextAsync(Path.Combine(root, "package.json"), timeout.Token));
            var dependencies = package.RootElement.GetProperty("dependencies");
            Assert.AreEqual("jsr:@std/path@1.0.8", dependencies.GetProperty("@jsr/std__path").GetString());
            Assert.AreEqual("4.1.0", dependencies.GetProperty("date-fns").GetString());

            await DenoPackageRestorer.RestoreAndCheckAsync(root, deno, entries, libraries, timeout.Token);
            var lockPath = Path.Combine(root, "deno.lock");
            var locked = await File.ReadAllTextAsync(lockPath, timeout.Token);
            await DenoPackageRestorer.RestoreAndCheckAsync(root, deno, entries, libraries, timeout.Token);
            Assert.AreEqual(locked, await File.ReadAllTextAsync(lockPath, timeout.Token));
            Assert.AreEqual("npm/3", await RunDenoAsync(deno, root, "entry.js", timeout.Token));

            var result = await new JavaScriptProjectBuilder().BuildAsync(root, deno, timeout.Token);
            Assert.IsTrue(result.IsSuccess, result.Diagnostic?.Message);
            Assert.AreEqual("npm/3", await RunDenoAsync(deno, root, "dist/bundle.js", timeout.Token));

            // Enabling SSR later must not require Emit to overwrite the authored Vite config.
            var config = await File.ReadAllTextAsync(Path.Combine(root, "vite.config.js"), timeout.Token);
            await File.WriteAllTextAsync(Path.Combine(root, "hydration.js"), "export const hydrate = () => 'ready';", timeout.Token);
            LibraryPackageWriter.WritePackageProject(root, libraries, hasSsrEntry: true);
            Assert.AreEqual(config, await File.ReadAllTextAsync(Path.Combine(root, "vite.config.js"), timeout.Token));
            var withHydration = await new JavaScriptProjectBuilder().BuildAsync(root, deno, timeout.Token);
            Assert.IsTrue(withHydration.IsSuccess, withHydration.Diagnostic?.Message);
            Assert.IsTrue(File.Exists(Path.Combine(root, "dist", "hydration.js")));

            // A patch version is a new identity, even when its JSR package and major are unchanged.
            var upgradedReferences = libraries.PackageReferences.ToDictionary(pair => pair.Key, pair => pair.Value);
            upgradedReferences["@jsr/std__path"] = new("@jsr/std__path", "jsr:@std/path@1.0.9", "jsr");
            var upgradedLibraries = libraries with { PackageReferences = upgradedReferences };
            LibraryPackageWriter.WritePackageProject(root, upgradedLibraries);
            await DenoPackageRestorer.RestoreAndCheckAsync(root, deno, entries, upgradedLibraries, timeout.Token);
            var upgradedLock = await File.ReadAllTextAsync(lockPath, timeout.Token);
            Assert.AreNotEqual(locked, upgradedLock);
            StringAssert.Contains(upgradedLock, "npm:@jsr/std__path@1.0.9");
            Assert.AreEqual("npm/3", await RunDenoAsync(deno, root, "entry.js", timeout.Token));
            Assert.IsFalse(File.Exists(Path.Combine(root, "importmap.json")));
            Assert.IsFalse(File.Exists(Path.Combine(root, "jazor-manifest.json")));
            Assert.IsFalse(File.Exists(Path.Combine(root, ".jazor-deno-check-importmap.json")));
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    private static async Task<string> RunDenoAsync(string deno, string root, string entry, CancellationToken cancellationToken)
    {
        var start = new ProcessStartInfo(deno)
        {
            WorkingDirectory = root,
            UseShellExecute = false,
            CreateNoWindow = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true
        };
        foreach (var argument in new[] { "run", "--no-config", "--no-remote", "--frozen-lockfile", "--node-modules-dir=manual", entry })
            start.ArgumentList.Add(argument);
        using var process = Process.Start(start)!;
        var stdout = process.StandardOutput.ReadToEndAsync(cancellationToken);
        var stderr = process.StandardError.ReadToEndAsync(cancellationToken);
        try
        {
            await process.WaitForExitAsync(cancellationToken);
            Assert.AreEqual(0, process.ExitCode, await stderr);
            return (await stdout).Trim();
        }
        finally
        {
            if (!process.HasExited)
            {
                process.Kill(entireProcessTree: true);
                await process.WaitForExitAsync(CancellationToken.None);
            }
        }
    }
}
