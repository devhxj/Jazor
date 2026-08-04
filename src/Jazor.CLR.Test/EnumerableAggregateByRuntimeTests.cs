using System.Text;
using DenoHost.Core;

namespace Jazor.CLR.Test;

[TestClass]
public sealed class EnumerableAggregateByRuntimeTests
{
    private const string EnumerableModulePath = "System/Linq/EnumerableModule.js";

    [TestMethod]
    public async Task AggregateByExports_PreserveComparerRepresentativeSeedProtocolAndEntryCarrierOnDenoHost()
    {
        var countBy = GetExportName("static System.Linq.Enumerable.CountBy<TSource, TKey>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, TKey>, System.Collections.Generic.IEqualityComparer<TKey>)");
        var aggregateBy = GetExportName("static System.Linq.Enumerable.AggregateBy<TSource, TKey, TAccumulate>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, TKey>, TAccumulate, System.Func<TAccumulate, TSource, TAccumulate>, System.Collections.Generic.IEqualityComparer<TKey>)");
        var aggregateByWithSeedSelector = GetExportName("static System.Linq.Enumerable.AggregateBy<TSource, TKey, TAccumulate>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, TKey>, System.Func<TKey, TAccumulate>, System.Func<TAccumulate, TSource, TAccumulate>, System.Collections.Generic.IEqualityComparer<TKey>)");
        var enumerableModule = ClrRuntimeCatalog.All.Single(module => module.RelativePath == EnumerableModulePath);
        StringAssert.Contains(enumerableModule.Content, "CountBy count exceeds Int32.MaxValue.", StringComparison.Ordinal);
        StringAssert.Contains(enumerableModule.Content, "count === 2147483647", StringComparison.Ordinal);

        var root = Path.Combine(Path.GetTempPath(), "jazor-enumerable-aggregate-by-" + Guid.NewGuid().ToString("N"));
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
            var testPath = Path.Combine(root, "aggregate-by.test.mjs");
            await File.WriteAllTextAsync(
                testPath,
                $$"""
                import { {{countBy}}, {{aggregateBy}}, {{aggregateByWithSeedSelector}} } from "./System/Linq/EnumerableModule.js";

                function parityComparer() {
                  return {
                    equals(left, right) {
                      return Math.abs(left % 2) === Math.abs(right % 2);
                    },
                    getHashCode(value) {
                      return Math.abs(value % 2);
                    }
                  };
                }

                function assertEntries(actual, expected, name) {
                  if (JSON.stringify(actual) !== JSON.stringify(expected))
                    throw new Error(`${name}: ${JSON.stringify(actual)} did not match ${JSON.stringify(expected)}`);
                  if (actual.some(entry => !Array.isArray(entry) || entry.length !== 2))
                    throw new Error(`${name}: KeyValuePair entries must stay two-slot Arrays`);
                }

                function assertArgumentNull(action, name) {
                  try {
                    action();
                  } catch (error) {
                    if (String(error).includes("ArgumentNullException"))
                      return;
                    throw new Error(`${name}: wrong error ${error}`);
                  }
                  throw new Error(`${name}: expected ArgumentNullException`);
                }

                Deno.test("CountBy and AggregateBy preserve CLR grouping protocol", () => {
                  const source = [3, 1, 2, 5, 4];
                  assertEntries({{countBy}}([1, 1, 2], value => value, null), [[1, 2], [2, 1]], "CountBy default comparer");
                  assertEntries({{aggregateBy}}([1, 1, 2], value => value, 1, (sum, value) => sum + value, null), [[1, 3], [2, 3]], "AggregateBy default comparer");
                  assertEntries({{aggregateByWithSeedSelector}}([1, 1, 2], value => value, key => key * 10, (sum, value) => sum + value, null), [[1, 12], [2, 22]], "AggregateBy key seed default comparer");

                  const countTrace = [];
                  const counts = {{countBy}}(source, value => {
                    countTrace.push(`key:${value}`);
                    return value;
                  }, parityComparer());
                  assertEntries(counts, [[3, 3], [2, 2]], "CountBy comparer result");
                  if (countTrace.join(",") !== "key:3,key:1,key:2,key:5,key:4")
                    throw new Error(`CountBy key selector order drifted: ${countTrace.join(",")}`);
                  if (source.join(",") !== "3,1,2,5,4")
                    throw new Error(`CountBy mutated source: ${source.join(",")}`);

                  const fixedTrace = [];
                  const fixed = {{aggregateBy}}(source, value => {
                    fixedTrace.push(`key:${value}`);
                    return value;
                  }, 10, (sum, value) => {
                    fixedTrace.push(`func:${sum}:${value}`);
                    return sum + value;
                  }, parityComparer());
                  assertEntries(fixed, [[3, 19], [2, 16]], "AggregateBy fixed seed result");
                  if (fixedTrace.join(",") !== "key:3,func:10:3,key:1,func:13:1,key:2,func:10:2,key:5,func:14:5,key:4,func:12:4")
                    throw new Error(`AggregateBy fixed seed protocol drifted: ${fixedTrace.join(",")}`);

                  const seedTrace = [];
                  const keySeed = {{aggregateByWithSeedSelector}}(source, value => {
                    seedTrace.push(`key:${value}`);
                    return value;
                  }, key => {
                    seedTrace.push(`seed:${key}`);
                    return key * 10;
                  }, (sum, value) => {
                    seedTrace.push(`func:${sum}:${value}`);
                    return sum + value;
                  }, parityComparer());
                  assertEntries(keySeed, [[3, 39], [2, 26]], "AggregateBy key seed result");
                  if (seedTrace.join(",") !== "key:3,seed:3,func:30:3,key:1,func:33:1,key:2,seed:2,func:20:2,key:5,func:34:5,key:4,func:22:4")
                    throw new Error(`AggregateBy key seed protocol drifted: ${seedTrace.join(",")}`);

                  assertArgumentNull(() => {{countBy}}(null, value => value, null), "CountBy source");
                  assertArgumentNull(() => {{countBy}}([], null, null), "CountBy key selector");
                  assertArgumentNull(() => {{aggregateBy}}(null, value => value, 0, (sum, value) => sum + value, null), "AggregateBy source");
                  assertArgumentNull(() => {{aggregateBy}}([], null, 0, (sum, value) => sum + value, null), "AggregateBy key selector");
                  assertArgumentNull(() => {{aggregateBy}}([], value => value, 0, null, null), "AggregateBy accumulator");
                  assertArgumentNull(() => {{aggregateByWithSeedSelector}}([], value => value, null, (sum, value) => sum + value, null), "AggregateBy seed selector");
                  assertArgumentNull(() => {{aggregateByWithSeedSelector}}([], value => value, value => value, null, null), "AggregateBy key seed accumulator");
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
