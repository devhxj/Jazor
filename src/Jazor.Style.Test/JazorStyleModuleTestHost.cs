using System.Collections;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;

namespace Jazor.Style.Tests;

internal static class JazorStyleModuleTestHost
{
    public static ModuleRecord GetRuntimeModule()
    {
        var assembly = typeof(Css).Assembly;
        var catalogType = assembly.GetType("Jazor.Generated.ModuleCatalog", throwOnError: false, ignoreCase: false)
            ?? throw new InvalidOperationException("Jazor.Style module catalog was not generated.");
        var getModules = catalogType.GetMethod("GetModules", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
            ?? throw new InvalidOperationException("Jazor.Style module catalog has no GetModules method.");
        var modules = ((IEnumerable?)getModules.Invoke(null, null))?.Cast<object>().ToArray()
            ?? throw new InvalidOperationException("Jazor.Style module catalog returned null.");
        var module = modules.Single();

        return new ModuleRecord(
            ReadProperty(module, "TypeName"),
            ReadProperty(module, "RelativePath"),
            ReadProperty(module, "Content"));
    }

    public static async Task<ProcessResult> RunDenoAsync(string runnerSource)
    {
        var root = Path.Combine(Path.GetTempPath(), "jazor-css-test-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        try
        {
            var module = GetRuntimeModule();
            var runtimePath = Path.Combine(root, "runtime.mjs");
            var runnerPath = Path.Combine(root, "runner.mjs");
            await File.WriteAllTextAsync(runtimePath, module.Content, new UTF8Encoding(false));
            await File.WriteAllTextAsync(runnerPath, runnerSource, new UTF8Encoding(false));

            var startInfo = new ProcessStartInfo
            {
                FileName = ResolveDenoPath(),
                WorkingDirectory = root,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                StandardOutputEncoding = Encoding.UTF8,
                StandardErrorEncoding = Encoding.UTF8,
                UseShellExecute = false,
                CreateNoWindow = true
            };
            startInfo.ArgumentList.Add("run");
            startInfo.ArgumentList.Add("--no-config");
            startInfo.ArgumentList.Add("runner.mjs");

            using var process = Process.Start(startInfo)
                ?? throw new InvalidOperationException("Deno process could not be started.");
            var standardOutput = process.StandardOutput.ReadToEndAsync();
            var standardError = process.StandardError.ReadToEndAsync();
            await process.WaitForExitAsync().WaitAsync(TimeSpan.FromSeconds(30));
            return new ProcessResult(process.ExitCode, await standardOutput, await standardError);
        }
        finally
        {
            if (Directory.Exists(root))
                Directory.Delete(root, recursive: true);
        }
    }

    private static string ResolveDenoPath()
    {
        var runtimeIdentifier = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
            ? "win-x64"
            : RuntimeInformation.IsOSPlatform(OSPlatform.Linux)
                ? "linux-x64"
                : RuntimeInformation.ProcessArchitecture == Architecture.Arm64
                    ? "osx-arm64"
                    : "osx-x64";
        var executableName = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "deno.exe" : "deno";
        var path = Path.Combine(AppContext.BaseDirectory, "runtimes", runtimeIdentifier, "native", executableName);
        return File.Exists(path)
            ? path
            : throw new FileNotFoundException("Bundled Deno runtime was not found.", path);
    }

    private static string ReadProperty(object instance, string name)
    {
        var property = instance.GetType().GetProperty(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
            ?? throw new InvalidOperationException("Catalog module has no '" + name + "' property.");
        return (string?)property.GetValue(instance) ?? string.Empty;
    }

    internal sealed record ModuleRecord(string TypeName, string RelativePath, string Content);

    internal sealed record ProcessResult(int ExitCode, string StandardOutput, string StandardError);
}
