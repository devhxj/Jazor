using System.Diagnostics;
using System.Text.Json;

namespace Jazor.EmitTest;

[TestClass]
public sealed class BuildBenchmarkReportTests
{
    [TestMethod]
    public async Task ReleaseReport_SeparatesSharedAndLazyAssets_AndRejectsIncompleteOrWritableInputs()
    {
        var repo = FindRepositoryRoot();
        var root = Path.Combine(RepositoryTemp.Root, "Jazor.BuildReportTest", Guid.NewGuid().ToString("N"));
        var dist = Path.Combine(root, "dist");
        Directory.CreateDirectory(dist);
        var contents = new Dictionary<string, string>
        {
            ["manifest.json"] = """
                {
                  "entry.js":{"file":"bundle.js","isEntry":true,"imports":["shared"],"dynamicImports":["lazy"]},
                  "shared":{"file":"shared.js","imports":["entry.js"],"css":["shared.css"]},
                  "lazy":{"file":"lazy.js","imports":["shared"],"css":["shared.css","lazy.css"],"assets":["image.svg"]}
                }
                """,
            ["bundle.js"] = new string('x', 4096), ["shared.js"] = "shared", ["shared.css"] = "shared css",
            ["lazy.js"] = "lazy", ["lazy.css"] = "lazy css", ["image.svg"] = "<svg/>",
            ["bundle.js.map"] = "{}", ["unreferenced.txt"] = "other"
        };
        try
        {
            foreach (var (path, content) in contents)
                await File.WriteAllTextAsync(Path.Combine(dist, path), content);
            var reportPath = Path.Combine(root, "report.json");
            var result = await RunAsync(reportPath);
            Assert.AreEqual(0, result.ExitCode, result.Output);
            using var report = JsonDocument.Parse(await File.ReadAllTextAsync(reportPath));
            var files = report.RootElement.GetProperty("Measurements")[0].GetProperty("ReleaseArtifact").GetProperty("Files");
            var byPath = files.EnumerateArray().ToDictionary(file => file.GetProperty("Path").GetString()!);
            Assert.AreEqual(contents.Count, byPath.Count);
            Assert.AreEqual("entry", byPath["bundle.js"].GetProperty("Group").GetString());
            Assert.AreEqual("static-dependency", byPath["shared.js"].GetProperty("Group").GetString());
            Assert.AreEqual("static-dependency", byPath["shared.css"].GetProperty("Group").GetString());
            foreach (var path in new[] { "lazy.js", "lazy.css", "image.svg" })
                Assert.AreEqual("lazy", byPath[path].GetProperty("Group").GetString());
            Assert.AreEqual("source-map", byPath["bundle.js.map"].GetProperty("Group").GetString());
            Assert.AreEqual("other", byPath["unreferenced.txt"].GetProperty("Group").GetString());
            Assert.IsTrue(byPath["bundle.js"].GetProperty("GzipBytes").GetInt64() < 4096);
            Assert.AreEqual(contents.Count, Directory.GetFiles(dist).Length);
            foreach (var (path, content) in contents)
                Assert.AreEqual(content, await File.ReadAllTextAsync(Path.Combine(dist, path)));

            // Reporting into dist would mutate the input (or overwrite its manifest).
            var rejectedOutput = await RunAsync(Path.Combine(dist, "manifest.json"));
            Assert.AreNotEqual(0, rejectedOutput.ExitCode);
            StringAssert.Contains(rejectedOutput.Output, "outside the read-only");
            Assert.AreEqual(contents["manifest.json"], await File.ReadAllTextAsync(Path.Combine(dist, "manifest.json")));

            File.Delete(Path.Combine(dist, "image.svg"));
            var incomplete = await RunAsync(Path.Combine(root, "incomplete.json"));
            Assert.AreNotEqual(0, incomplete.ExitCode);
            StringAssert.Contains(incomplete.Output, "Release manifest resource is missing: image.svg");
            Assert.IsFalse(File.Exists(Path.Combine(root, "incomplete.json")));
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }

        async Task<(int ExitCode, string Output)> RunAsync(string output)
        {
            using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(60));
            using var process = new Process
            {
                StartInfo = new ProcessStartInfo("dotnet")
                {
                    WorkingDirectory = repo, UseShellExecute = false, CreateNoWindow = true,
                    RedirectStandardOutput = true, RedirectStandardError = true
                }
            };
            foreach (var argument in new[] { "run", "--file", Path.Combine(repo, "scripts", "csharp", "benchmark-razorvue-build.cs"),
                         "--", "--release-artifacts", dist, "--out", output })
                process.StartInfo.ArgumentList.Add(argument);
            process.Start();
            var stdout = process.StandardOutput.ReadToEndAsync();
            var stderr = process.StandardError.ReadToEndAsync();
            try
            {
                await process.WaitForExitAsync(timeout.Token);
                return (process.ExitCode, await stdout + await stderr);
            }
            finally
            {
                if (!process.HasExited)
                {
                    process.Kill(entireProcessTree: true);
                    await process.WaitForExitAsync();
                }
            }
        }
    }

    private static string FindRepositoryRoot()
    {
        for (var directory = new DirectoryInfo(AppContext.BaseDirectory); directory is not null; directory = directory.Parent)
            if (File.Exists(Path.Combine(directory.FullName, "Jazor.slnx")))
                return directory.FullName;
        throw new DirectoryNotFoundException("Jazor.slnx was not found.");
    }
}
