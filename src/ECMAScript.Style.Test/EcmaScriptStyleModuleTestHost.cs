using System.Collections;
using Jazor.NodeHost;
using System.Reflection;

using System.Text;

namespace ECMAScript.Style.Tests;

internal static class EcmaScriptStyleModuleTestHost
{
    public static ModuleRecord GetRuntimeModule()
    {
        var assembly = typeof(css).Assembly;
        var catalogType = assembly.GetType("Jazor.Generated.ModuleCatalog", throwOnError: false, ignoreCase: false)
            ?? throw new InvalidOperationException("ECMAScript.Style module catalog was not generated.");
        var getModules = catalogType.GetMethod("GetModules", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
            ?? throw new InvalidOperationException("ECMAScript.Style module catalog has no GetModules method.");
        var modules = ((IEnumerable?)getModules.Invoke(null, null))?.Cast<object>().ToArray()
            ?? throw new InvalidOperationException("ECMAScript.Style module catalog returned null.");
        var module = modules.Single();

        return new ModuleRecord(
            ReadProperty(module, "TypeName"),
            ReadProperty(module, "RelativePath"),
            ReadProperty(module, "Content"));
    }

    public static async Task<string> RunNodeAsync(string runnerSource)
    {
        var root = Path.Combine(RepositoryTemp.Root, "ecmascript-style-test-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        try
        {
            var module = GetRuntimeModule();
            var runtimePath = Path.Combine(root, "runtime.mjs");
            var runnerPath = Path.Combine(root, "runner.mjs");
            await global::System.IO.File.WriteAllTextAsync(runtimePath, module.Content, new UTF8Encoding(false));
            await global::System.IO.File.WriteAllTextAsync(runnerPath, runnerSource, new UTF8Encoding(false));

            return await NodeModuleRunner.RunAsync(root, runnerPath);
        }
        finally
        {
            if (Directory.Exists(root))
                Directory.Delete(root, recursive: true);
        }
    }

    private static string ReadProperty(object instance, string name)
    {
        var property = instance.GetType().GetProperty(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
            ?? throw new InvalidOperationException("Catalog module has no '" + name + "' property.");
        return (string?)property.GetValue(instance) ?? string.Empty;
    }

    internal sealed record ModuleRecord(string TypeName, string RelativePath, string Content);


}
