using System.Text;
using DenoHost.Core;

namespace Jazor.CLR.Test;

[TestClass]
public sealed class EnumerableGroupByRuntimeTests
{
    private const string EnumerableModulePath = "System/Linq/EnumerableModule.js";
    private const string GroupingModulePath = "System/Linq/GroupingT2Module.js";

    [TestMethod]
    public async Task GroupByExports_PreserveGroupingCarrierAndComparerSemanticsOnDenoHost()
    {
        var groupBy = GetExportName(
            "static System.Linq.Enumerable.GroupBy<TSource, TKey>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, TKey>)",
            EnumerableModulePath);
        var groupByElement = GetExportName(
            "static System.Linq.Enumerable.GroupBy<TSource, TKey, TElement>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, TKey>, System.Func<TSource, TElement>)",
            EnumerableModulePath);
        var groupByResult = GetExportName(
            "static System.Linq.Enumerable.GroupBy<TSource, TKey, TResult>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, TKey>, System.Func<TKey, System.Collections.Generic.IEnumerable<TSource>, TResult>)",
            EnumerableModulePath);
        var groupByElementResult = GetExportName(
            "static System.Linq.Enumerable.GroupBy<TSource, TKey, TElement, TResult>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, TKey>, System.Func<TSource, TElement>, System.Func<TKey, System.Collections.Generic.IEnumerable<TElement>, TResult>)",
            EnumerableModulePath);
        var groupingKey = GetExportName(
            "System.Linq.IGrouping<TKey, TElement>.Key.get",
            GroupingModulePath);
        var root = Path.Combine(Path.GetTempPath(), "jazor-enumerable-group-by-" + Guid.NewGuid().ToString("N"));
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
            var testPath = Path.Combine(root, "group-by.test.mjs");
            await File.WriteAllTextAsync(
                testPath,
                $$"""
                import { {{groupBy}}, {{groupByElement}}, {{groupByResult}}, {{groupByElementResult}} } from "./System/Linq/EnumerableModule.js";
                import { {{groupingKey}} } from "./System/Linq/GroupingT2Module.js";

                Deno.test("GroupBy preserves group order, carrier keys, and selector order", () => {
                  const trace = [];
                  const groups = {{groupBy}}([1, 2, 3, 4], value => {
                    trace.push(`key:${value}`);
                    return value % 2;
                  });
                  if (JSON.stringify(groups) !== "[[1,3],[2,4]]")
                    throw new Error(`unexpected group members: ${JSON.stringify(groups)}`);
                  if (JSON.stringify(groups.map({{groupingKey}})) !== "[1,0]")
                    throw new Error(`unexpected group keys: ${JSON.stringify(groups.map({{groupingKey}}))}`);

                  const projected = {{groupByElement}}([1, 2, 3], value => {
                    trace.push(`project-key:${value}`);
                    return value % 2;
                  }, value => {
                    trace.push(`element:${value}`);
                    return value * 10;
                  });
                  if (JSON.stringify(projected) !== "[[10,30],[20]]")
                    throw new Error(`unexpected projected groups: ${JSON.stringify(projected)}`);
                  if (trace.join(",") !== "key:1,key:2,key:3,key:4,project-key:1,element:1,project-key:2,element:2,project-key:3,element:3")
                    throw new Error(`unexpected selector order: ${trace.join(",")}`);

                  const numericGroups = {{groupBy}}([Number.NaN, -0, 0, Number.NaN], value => value);
                  if (numericGroups.length !== 2 || numericGroups[0].length !== 2 || numericGroups[1].length !== 2)
                    throw new Error(`unexpected numeric grouping shape: ${JSON.stringify(numericGroups)}`);
                  if (!Number.isNaN({{groupingKey}}(numericGroups[0])) || !Object.is({{groupingKey}}(numericGroups[1]), -0))
                    throw new Error("GroupBy did not preserve the first CLR-equivalent numeric keys");

                  const source = [1, 2, 3, 4];
                  const resultTrace = [];
                  const resultGroups = {{groupByResult}}(source, value => {
                    resultTrace.push(`key:${value}`);
                    return value % 2;
                  }, (key, group) => {
                    resultTrace.push(`result:${key}:${group.join(",")}`);
                    return key * 10 + group.length;
                  });
                  if (JSON.stringify(resultGroups) !== "[12,2]")
                    throw new Error(`unexpected GroupBy result projection: ${JSON.stringify(resultGroups)}`);
                  if (resultTrace.join(",") !== "key:1,key:2,key:3,key:4,result:1:1,3,result:0:2,4")
                    throw new Error(`GroupBy result selector order drifted: ${resultTrace.join(",")}`);

                  const elementResultTrace = [];
                  const projectedResults = {{groupByElementResult}}([1, 2, 3], value => {
                    elementResultTrace.push(`key:${value}`);
                    return value % 2;
                  }, value => {
                    elementResultTrace.push(`element:${value}`);
                    return value * 10;
                  }, (key, values) => {
                    elementResultTrace.push(`result:${key}:${values.join(",")}`);
                    return key * 100 + values.reduce((sum, value) => sum + value, 0);
                  });
                  if (JSON.stringify(projectedResults) !== "[140,20]")
                    throw new Error(`unexpected GroupBy element result projection: ${JSON.stringify(projectedResults)}`);
                  if (elementResultTrace.join(",") !== "key:1,element:1,key:2,element:2,key:3,element:3,result:1:10,30,result:0:20")
                    throw new Error(`GroupBy element result selector order drifted: ${elementResultTrace.join(",")}`);

                  if ({{groupByResult}}([], value => value, () => 1).length !== 0)
                    throw new Error("GroupBy result selector must not run for an empty source");
                  for (const [source, keySelector, resultSelector] of [
                    [null, value => value, () => 1],
                    [[1], null, () => 1],
                    [[1], value => value, null]
                  ]) {
                    let rejected = false;
                    try { {{groupByResult}}(source, keySelector, resultSelector); } catch (error) { rejected = String(error).includes("ArgumentNullException"); }
                    if (!rejected) throw new Error("GroupBy result selector overload must reject null arguments");
                  }
                  if (JSON.stringify(source) !== "[1,2,3,4]")
                    throw new Error("GroupBy result selector must not mutate source");
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

    private static string GetExportName(string member, string modulePath)
    {
        var mapping = ClrRuntimeMappingCatalog.GetImport(member);
        Assert.AreEqual(modulePath, mapping.ModulePath, member);
        return mapping.ExportName;
    }
}
