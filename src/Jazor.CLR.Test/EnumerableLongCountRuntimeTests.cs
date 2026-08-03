using System.Text;
using DenoHost.Core;

namespace Jazor.CLR.Test;

[TestClass]
public sealed class EnumerableLongCountRuntimeTests
{
    private const string EnumerableModulePath = "System/Linq/EnumerableModule.js";

    [TestMethod]
    public async Task LongCountExports_PreserveBigIntResultPredicateOrderAndWidthBoundariesOnDenoHost()
    {
        var longCount = GetExportName("static System.Linq.Enumerable.LongCount<TSource>(System.Collections.Generic.IEnumerable<TSource>)");
        var longCountWhere = GetExportName("static System.Linq.Enumerable.LongCount<TSource>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, bool>)");
        var enumerableModule = ClrRuntimeCatalog.All.Single(module => module.RelativePath == EnumerableModulePath);
        StringAssert.Contains(enumerableModule.Content, "count === 2147483647", StringComparison.Ordinal);
        StringAssert.Contains(enumerableModule.Content, "9223372036854775807", StringComparison.Ordinal);

        var root = Path.Combine(Path.GetTempPath(), "jazor-enumerable-long-count-" + Guid.NewGuid().ToString("N"));
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
            var testPath = Path.Combine(root, "long-count.test.mjs");
            await File.WriteAllTextAsync(
                testPath,
                $$"""
                import { {{longCount}}, {{longCountWhere}} } from "./System/Linq/EnumerableModule.js";

                Deno.test("Enumerable.LongCount preserves BigInt width and predicate order", () => {
                  const source = [3, 1, 4, 2];
                  const all = {{longCount}}(source);
                  if (all !== 4n || typeof all !== "bigint")
                    throw new Error(`LongCount must return a BigInt carrier: ${all} / ${typeof all}`);

                  const trace = [];
                  const matches = {{longCountWhere}}(source, value => {
                    trace.push(value);
                    return value % 2 === 0;
                  });
                  if (matches !== 2n || trace.join(",") !== "3,1,4,2")
                    throw new Error(`LongCount predicate order drifted: ${matches} / ${trace.join(",")}`);
                  if (source.join(",") !== "3,1,4,2")
                    throw new Error(`LongCount must not mutate its source: ${source.join(",")}`);

                  let sourceRejected = false;
                  try {
                    {{longCount}}(null);
                  } catch (error) {
                    sourceRejected = String(error).includes("ArgumentNullException");
                  }
                  if (!sourceRejected)
                    throw new Error("LongCount must reject a null source");

                  let predicateRejected = false;
                  try {
                    {{longCountWhere}}([1], null);
                  } catch (error) {
                    predicateRejected = String(error).includes("ArgumentNullException");
                  }
                  if (!predicateRejected)
                    throw new Error("LongCount must reject a null predicate");
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
