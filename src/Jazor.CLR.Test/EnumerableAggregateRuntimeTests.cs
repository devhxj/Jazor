using System.Text;
using DenoHost.Core;

namespace Jazor.CLR.Test;

[TestClass]
public sealed class EnumerableAggregateRuntimeTests
{
    private const string EnumerableModulePath = "System/Linq/EnumerableModule.js";

    [TestMethod]
    public async Task AggregateExports_PreserveAccumulatorAndResultSelectorOrderOnDenoHost()
    {
        var aggregate = GetExportName("static System.Linq.Enumerable.Aggregate<TSource>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, TSource, TSource>)");
        var aggregateWithSeed = GetExportName("static System.Linq.Enumerable.Aggregate<TSource, TAccumulate>(System.Collections.Generic.IEnumerable<TSource>, TAccumulate, System.Func<TAccumulate, TSource, TAccumulate>)");
        var aggregateWithResult = GetExportName("static System.Linq.Enumerable.Aggregate<TSource, TAccumulate, TResult>(System.Collections.Generic.IEnumerable<TSource>, TAccumulate, System.Func<TAccumulate, TSource, TAccumulate>, System.Func<TAccumulate, TResult>)");
        var root = Path.Combine(Path.GetTempPath(), "jazor-enumerable-aggregate-" + Guid.NewGuid().ToString("N"));
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
            var testPath = Path.Combine(root, "aggregate.test.mjs");
            await File.WriteAllTextAsync(
                testPath,
                $$"""
                import { {{aggregate}}, {{aggregateWithSeed}}, {{aggregateWithResult}} } from "./System/Linq/EnumerableModule.js";

                Deno.test("Aggregate preserves accumulator and result-selector order", () => {
                  const unseededTrace = [];
                  const unseeded = {{aggregate}}([2, 3, 4], (total, value) => {
                    unseededTrace.push(`${total}:${value}`);
                    return total + value;
                  });
                  if (unseeded !== 9 || unseededTrace.join(",") !== "2:3,5:4")
                    throw new Error(`unseeded aggregation drifted: ${unseeded} / ${unseededTrace.join(",")}`);

                  const seededTrace = [];
                  const seeded = {{aggregateWithSeed}}([2, 3, 4], 10, (total, value) => {
                    seededTrace.push(`${total}:${value}`);
                    return total + value;
                  });
                  if (seeded !== 19 || seededTrace.join(",") !== "10:2,12:3,15:4")
                    throw new Error(`seeded aggregation drifted: ${seeded} / ${seededTrace.join(",")}`);

                  const resultTrace = [];
                  const result = {{aggregateWithResult}}([], 10, () => {
                    throw new Error("empty source must not invoke accumulator");
                  }, total => {
                    resultTrace.push(total);
                    return total * 2;
                  });
                  if (result !== 20 || resultTrace.join(",") !== "10")
                    throw new Error(`result selector drifted: ${result} / ${resultTrace.join(",")}`);
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
