using System.Text;
using DenoHost.Core;

namespace Jazor.CLR.Test;

[TestClass]
public sealed class EnumerableDistinctByRuntimeTests
{
    private const string EnumerableModulePath = "System/Linq/EnumerableModule.js";

    [TestMethod]
    public async Task DistinctByExport_PreservesFirstKeysAndDefaultEqualityOnDenoHost()
    {
        var distinctBy = GetExportName("static System.Linq.Enumerable.DistinctBy<TSource, TKey>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, TKey>)");
        var root = Path.Combine(Path.GetTempPath(), "jazor-enumerable-distinct-by-" + Guid.NewGuid().ToString("N"));
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
            var testPath = Path.Combine(root, "distinct-by.test.mjs");
            await File.WriteAllTextAsync(
                testPath,
                $$"""
                import { {{distinctBy}} } from "./System/Linq/EnumerableModule.js";

                Deno.test("DistinctBy evaluates each key once and uses CLR default equality", () => {
                  const source = [2, 7, 4, 9, 3];
                  const keyTrace = [];
                  const result = {{distinctBy}}(source, value => {
                    keyTrace.push(value);
                    return value % 2;
                  });
                  if (result.join(",") !== "2,7")
                    throw new Error(`DistinctBy result was ${result.join(",")}`);
                  if (keyTrace.join(",") !== "2,7,4,9,3")
                    throw new Error(`DistinctBy selector invocation trace was ${keyTrace.join(",")}`);
                  if (source.join(",") !== "2,7,4,9,3")
                    throw new Error(`DistinctBy mutated source: ${source.join(",")}`);

                  const equalityResult = {{distinctBy}}([Number.NaN, 0, -0, Number.NaN], value => value);
                  if (equalityResult.length !== 2 || !Number.isNaN(equalityResult[0]) || equalityResult[1] !== 0)
                    throw new Error(`DistinctBy equality result was ${equalityResult.map(String).join(",")}`);
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
