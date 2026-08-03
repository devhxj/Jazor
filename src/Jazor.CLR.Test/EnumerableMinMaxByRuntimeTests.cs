using System.Text;
using DenoHost.Core;

namespace Jazor.CLR.Test;

[TestClass]
public sealed class EnumerableMinMaxByRuntimeTests
{
    private const string EnumerableModulePath = "System/Linq/EnumerableModule.js";

    [TestMethod]
    public async Task MinByAndMaxByExports_EvaluateEachSelectorOnceAndPreserveFirstTieOnDenoHost()
    {
        var minBy = GetExportName("static System.Linq.Enumerable.MinBy<TSource, TKey>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, TKey>)");
        var maxBy = GetExportName("static System.Linq.Enumerable.MaxBy<TSource, TKey>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, TKey>)");
        var root = Path.Combine(Path.GetTempPath(), "jazor-enumerable-min-max-by-" + Guid.NewGuid().ToString("N"));
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
            var testPath = Path.Combine(root, "min-max-by.test.mjs");
            await File.WriteAllTextAsync(
                testPath,
                $$"""
                import { {{minBy}}, {{maxBy}} } from "./System/Linq/EnumerableModule.js";

                Deno.test("MinBy and MaxBy preserve selector order and first ties", () => {
                  const source = [22, 15, 35, 12];
                  const minTrace = [];
                  const minimum = {{minBy}}(source, value => {
                    minTrace.push(value);
                    return value % 10;
                  });
                  if (minimum !== 22 || minTrace.join(",") !== "22,15,35,12")
                    throw new Error(`MinBy result/trace was ${minimum} / ${minTrace.join(",")}`);

                  const maxTrace = [];
                  const maximum = {{maxBy}}(source, value => {
                    maxTrace.push(value);
                    return value % 10;
                  });
                  if (maximum !== 15 || maxTrace.join(",") !== "22,15,35,12")
                    throw new Error(`MaxBy result/trace was ${maximum} / ${maxTrace.join(",")}`);
                  if (source.join(",") !== "22,15,35,12")
                    throw new Error(`MinBy or MaxBy mutated source: ${source.join(",")}`);
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
