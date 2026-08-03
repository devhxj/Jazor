using System.Text;
using DenoHost.Core;

namespace Jazor.CLR.Test;

[TestClass]
public sealed class EnumerableSetRuntimeTests
{
    private const string EnumerableModulePath = "System/Linq/EnumerableModule.js";
    private const string MemoryExtensionsModulePath = "System/MemoryExtensionsModule.js";

    [TestMethod]
    public async Task SetOperatorExports_PreserveClrEqualityAndMaterializedEnumerationOrderOnDenoHost()
    {
        var distinct = GetExportName("static System.Linq.Enumerable.Distinct<TSource>(System.Collections.Generic.IEnumerable<TSource>)");
        var union = GetExportName("static System.Linq.Enumerable.Union<TSource>(System.Collections.Generic.IEnumerable<TSource>, System.Collections.Generic.IEnumerable<TSource>)");
        var except = GetExportName("static System.Linq.Enumerable.Except<TSource>(System.Collections.Generic.IEnumerable<TSource>, System.Collections.Generic.IEnumerable<TSource>)");
        var intersect = GetExportName("static System.Linq.Enumerable.Intersect<TSource>(System.Collections.Generic.IEnumerable<TSource>, System.Collections.Generic.IEnumerable<TSource>)");
        var contains = GetExportName("static System.Linq.Enumerable.Contains<TSource>(System.Collections.Generic.IEnumerable<TSource>, TSource)");
        var spanContains = GetExportName("static System.MemoryExtensions.Contains<T>(System.ReadOnlySpan<T>, T)", MemoryExtensionsModulePath);
        var root = Path.Combine(Path.GetTempPath(), "jazor-enumerable-set-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);

        try
        {
            foreach (var module in ClrRuntimeCatalog.All)
            {
                var outputPath = Path.Combine(root, module.RelativePath.Replace('/', Path.DirectorySeparatorChar));
                Directory.CreateDirectory(Path.GetDirectoryName(outputPath)!);
                await File.WriteAllTextAsync(outputPath, module.Content, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
            }

            var configPath = Path.Combine(root, "deno.json");
            await File.WriteAllTextAsync(
                configPath,
                """
                {
                  "imports": {
                    "System/": "./System/"
                  }
                }
                """,
                new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
            var testPath = Path.Combine(root, "set-operators.test.mjs");
            await File.WriteAllTextAsync(
                testPath,
                $$"""
                import { {{distinct}}, {{union}}, {{except}}, {{intersect}}, {{contains}} } from "./System/Linq/EnumerableModule.js";
                import { {{spanContains}} } from "./System/MemoryExtensionsModule.js";

                function assertArray(actual, expected, name) {
                  if (actual.length !== expected.length || actual.some((value, index) => !Object.is(value, expected[index])))
                    throw new Error(`${name}: ${JSON.stringify(actual)} did not match the expected CLR-order result`);
                }

                Deno.test("Enumerable set operators preserve CLR equality and enumeration order", () => {
                  assertArray({{distinct}}([Number.NaN, -0, 0, Number.NaN, 1]), [Number.NaN, -0, 1], "Distinct");
                  assertArray({{union}}([1, 2, 1], [2, 3, 1]), [1, 2, 3], "Union");
                  assertArray({{except}}([1, 2, 2, 3], [2, 4]), [1, 3], "Except");
                  assertArray({{intersect}}([3, 1, 2, 3, 2], [2, 3, 3]), [3, 2], "Intersect");
                  if (!{{contains}}([1, Number.NaN, 3], Number.NaN))
                    throw new Error("Contains must use EqualityComparer semantics for NaN");
                  if ({{contains}}([1, 2, 3], 4))
                    throw new Error("Contains returned true for an absent value");
                  if (!{{spanContains}}([1, Number.NaN, 3], Number.NaN))
                    throw new Error("MemoryExtensions.Contains must use EqualityComparer semantics for NaN");
                });
                """,
                new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));

            using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(30));
            await Deno.Execute(
                new DenoExecuteBaseOptions { WorkingDirectory = root },
                ["test", "--config", configPath, "--quiet", "--allow-read", testPath],
                timeout.Token);
        }
        finally
        {
            if (Directory.Exists(root))
                Directory.Delete(root, recursive: true);
        }
    }

    private static string GetExportName(string member, string modulePath = EnumerableModulePath)
    {
        var mapping = ClrRuntimeMappingCatalog.GetImport(member);
        Assert.AreEqual(modulePath, mapping.ModulePath, member);
        return mapping.ExportName;
    }
}
