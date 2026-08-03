using System.Text;
using DenoHost.Core;

namespace Jazor.CLR.Test;

[TestClass]
public sealed class EnumerableSkipTakeLastRuntimeTests
{
    private const string EnumerableModulePath = "System/Linq/EnumerableModule.js";

    [TestMethod]
    public async Task SkipLastAndTakeLastExports_PreserveTailBufferTraversalOnDenoHost()
    {
        var skipLast = GetExportName("static System.Linq.Enumerable.SkipLast<TSource>(System.Collections.Generic.IEnumerable<TSource>, int)");
        var takeLast = GetExportName("static System.Linq.Enumerable.TakeLast<TSource>(System.Collections.Generic.IEnumerable<TSource>, int)");
        var root = Path.Combine(Path.GetTempPath(), "jazor-enumerable-skip-take-last-" + Guid.NewGuid().ToString("N"));
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
            var testPath = Path.Combine(root, "skip-take-last.test.mjs");
            await File.WriteAllTextAsync(
                testPath,
                $$"""
                import { {{skipLast}}, {{takeLast}} } from "./System/Linq/EnumerableModule.js";

                Deno.test("SkipLast and TakeLast preserve bounded tail traversal", () => {
                  const trace = [];
                  function* source(values) {
                    for (const value of values) {
                      trace.push(value);
                      yield value;
                    }
                  }

                  const skipped = {{skipLast}}(source([2, 7, 3, 9]), 2);
                  if (skipped.join(",") !== "2,7")
                    throw new Error(`SkipLast result was ${skipped.join(",")}`);
                  if (trace.join(",") !== "2,7,3,9")
                    throw new Error(`SkipLast traversal was ${trace.join(",")}`);

                  trace.length = 0;
                  const taken = {{takeLast}}(source([2, 7, 3, 9]), 2);
                  if (taken.join(",") !== "3,9")
                    throw new Error(`TakeLast result was ${taken.join(",")}`);
                  if (trace.join(",") !== "2,7,3,9")
                    throw new Error(`TakeLast traversal was ${trace.join(",")}`);

                  trace.length = 0;
                  if ({{takeLast}}(source([2, 7]), 0).length !== 0 || trace.length !== 0)
                    throw new Error("TakeLast(0) must not enumerate an unknown source");

                  trace.length = 0;
                  const skipZero = {{skipLast}}(source([2, 7]), 0);
                  if (skipZero.join(",") !== "2,7" || trace.join(",") !== "2,7")
                    throw new Error(`SkipLast(0) traversal was ${skipZero.join(",")}/${trace.join(",")}`);

                  const original = [2, 7, 3, 9];
                  {{skipLast}}(original, 2);
                  {{takeLast}}(original, 2);
                  if (original.join(",") !== "2,7,3,9")
                    throw new Error(`tail operations mutated source: ${original.join(",")}`);
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

    private static string GetExportName(string member)
    {
        var mapping = ClrRuntimeMappingCatalog.GetImport(member);
        Assert.AreEqual(EnumerableModulePath, mapping.ModulePath, member);
        return mapping.ExportName;
    }
}
