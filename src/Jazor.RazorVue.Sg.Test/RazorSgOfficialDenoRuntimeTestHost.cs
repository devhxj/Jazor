using System.Diagnostics;

namespace Jazor.RazorVue.Sg.Test;

/// <summary>
/// Runs a generated official RazorVue artifact with the DenoHost runtime packaged by Jazor.
/// </summary>
/// <remarks>
/// Official Razor authoring tests normally assert the generated module text. This host is
/// intentionally narrow: use it only when the observed behavior depends on JS evaluation,
/// such as an event callback seeing state assigned by a binding handler.
/// </remarks>
internal static class RazorSgOfficialDenoRuntimeTestHost
{
    public static async Task RunModuleTestAsync(
        string moduleRelativePath,
        string moduleText,
        string testFileName,
        string testSource,
        IReadOnlyDictionary<string, string>? supportingModules = null)
    {
        var root = Path.Combine(
            Path.GetTempPath(),
            "Jazor.RazorVue.Sg.Test",
            Guid.NewGuid().ToString("N"));
        try
        {
            WriteFile(Path.Combine(root, moduleRelativePath), moduleText);
            if (supportingModules is not null)
            {
                foreach (var module in supportingModules)
                    WriteFile(Path.Combine(root, module.Key), module.Value);
            }
            WriteFile(
                Path.Combine(root, "package.json"),
                """{"type":"module"}""");
            WriteFile(
                Path.Combine(root, "node_modules", "vue", "package.json"),
                """{"type":"module","exports":"./index.mjs"}""");
            WriteFile(
                Path.Combine(root, "node_modules", "vue", "index.mjs"),
                """
                export function defineComponent(options) {
                    return options;
                }

                export function reactive(value) {
                    return value;
                }

                export function createStaticVNode(html, count) {
                    return { name: "__static", props: { html, count }, children: html };
                }

                export function mergeProps(...sources) {
                    return Object.assign({}, ...sources.filter(source => source != null));
                }

                export function h(name, props, children) {
                    return { name, props, children };
                }
                """);

            var testFile = Path.Combine(root, testFileName);
            WriteFile(testFile, testSource);
            await RunDenoTestAsync(testFile, root);
        }
        finally
        {
            if (Directory.Exists(root))
                Directory.Delete(root, recursive: true);
        }
    }

    private static void WriteFile(string path, string content)
    {
        var directory = Path.GetDirectoryName(path)
            ?? throw new InvalidOperationException($"Could not resolve parent directory for '{path}'.");
        Directory.CreateDirectory(directory);
        File.WriteAllText(path, content);
    }

    private static async Task RunDenoTestAsync(string testFile, string workingDirectory)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = ResolveBundledDenoExecutable(),
            RedirectStandardError = true,
            RedirectStandardOutput = true,
            UseShellExecute = false,
            WorkingDirectory = workingDirectory
        };
        startInfo.ArgumentList.Add("test");
        startInfo.ArgumentList.Add("--quiet");
        startInfo.ArgumentList.Add("--allow-all");
        startInfo.ArgumentList.Add(testFile);
        startInfo.Environment["DENO_DIR"] = Path.Combine(workingDirectory, ".deno-cache");

        using var process = Process.Start(startInfo)
            ?? throw new InvalidOperationException("Failed to start the bundled DenoHost runtime.");
        var standardOutput = process.StandardOutput.ReadToEndAsync();
        var standardError = process.StandardError.ReadToEndAsync();
        await process.WaitForExitAsync();

        if (process.ExitCode == 0)
            return;

        Assert.Fail(
            "Bundled DenoHost runtime test failed." + Environment.NewLine +
            await standardOutput + Environment.NewLine +
            await standardError);
    }

    private static string ResolveBundledDenoExecutable()
    {
        var root = FindRepositoryRoot();
        var executableName = OperatingSystem.IsWindows() ? "deno.exe" : "deno";
        var emitBuildRoot = Path.Combine(root, "src", "Jazor.Emit", "bin");
        if (!Directory.Exists(emitBuildRoot))
        {
            throw new FileNotFoundException(
                "Jazor.Emit build output is required for official RazorVue DenoHost tests. Build src/Jazor.Emit first.",
                emitBuildRoot);
        }

        var candidate = Directory.EnumerateFiles(emitBuildRoot, executableName, SearchOption.AllDirectories)
            .Where(path => path.Contains(
                Path.DirectorySeparatorChar + "runtimes" + Path.DirectorySeparatorChar,
                StringComparison.OrdinalIgnoreCase))
            .OrderByDescending(path => path.Contains(
                Path.DirectorySeparatorChar + "net11.0" + Path.DirectorySeparatorChar,
                StringComparison.OrdinalIgnoreCase))
            .ThenByDescending(File.GetLastWriteTimeUtc)
            .FirstOrDefault();

        return candidate ?? throw new FileNotFoundException(
            "Bundled DenoHost runtime was not found. Build src/Jazor.Emit so its runtime assets are restored.",
            emitBuildRoot);
    }

    private static string FindRepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            if (File.Exists(Path.Combine(directory.FullName, "Jazor.slnx")))
                return directory.FullName;

            directory = directory.Parent;
        }

        throw new DirectoryNotFoundException("Could not locate the Jazor repository root from the test output directory.");
    }
}
