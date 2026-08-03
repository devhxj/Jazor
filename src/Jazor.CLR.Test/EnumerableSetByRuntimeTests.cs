using System.Text;
using DenoHost.Core;

namespace Jazor.CLR.Test;

[TestClass]
public sealed class EnumerableSetByRuntimeTests
{
    private const string EnumerableModulePath = "System/Linq/EnumerableModule.js";

    [TestMethod]
    public async Task SetByOperatorExports_PreserveKeyEqualityAndEnumerationOrderOnDenoHost()
    {
        var unionBy = GetExportName("static System.Linq.Enumerable.UnionBy<TSource, TKey>(System.Collections.Generic.IEnumerable<TSource>, System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, TKey>)");
        var exceptBy = GetExportName("static System.Linq.Enumerable.ExceptBy<TSource, TKey>(System.Collections.Generic.IEnumerable<TSource>, System.Collections.Generic.IEnumerable<TKey>, System.Func<TSource, TKey>)");
        var intersectBy = GetExportName("static System.Linq.Enumerable.IntersectBy<TSource, TKey>(System.Collections.Generic.IEnumerable<TSource>, System.Collections.Generic.IEnumerable<TKey>, System.Func<TSource, TKey>)");
        var root = Path.Combine(Path.GetTempPath(), "jazor-enumerable-set-by-" + Guid.NewGuid().ToString("N"));
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
            var testPath = Path.Combine(root, "set-by-operators.test.mjs");
            await File.WriteAllTextAsync(
                testPath,
                $$"""
                import { {{unionBy}}, {{exceptBy}}, {{intersectBy}} } from "./System/Linq/EnumerableModule.js";

                function assertArray(actual, expected, name) {
                  if (actual.length !== expected.length || actual.some((value, index) => !Object.is(value, expected[index])))
                    throw new Error(`${name}: ${JSON.stringify(actual)} did not match ${JSON.stringify(expected)}`);
                }

                function* tracked(label, values, trace) {
                  for (const value of values) {
                    trace.push(`${label}:${value}`);
                    yield value;
                  }
                }

                Deno.test("Enumerable key-set operators preserve CLR equality and traversal order", () => {
                  let trace = [];
                  const union = {{unionBy}}(
                    tracked("first", [1, 3, 2, 4], trace),
                    tracked("second", [5, 6], trace),
                    value => {
                      trace.push(`key:${value}`);
                      return value % 2;
                    });
                  assertArray(union, [1, 2], "UnionBy");
                  if (trace.join(",") !== "first:1,key:1,first:3,key:3,first:2,key:2,first:4,key:4,second:5,key:5,second:6,key:6")
                    throw new Error(`UnionBy traversal was ${trace.join(",")}`);

                  trace = [];
                  const except = {{exceptBy}}(
                    tracked("source", [1, 2, 3, 4], trace),
                    tracked("excluded", [0, 2], trace),
                    value => {
                      trace.push(`key:${value}`);
                      return value % 2;
                    });
                  assertArray(except, [1], "ExceptBy");
                  if (trace.join(",") !== "excluded:0,excluded:2,source:1,key:1,source:2,key:2,source:3,key:3,source:4,key:4")
                    throw new Error(`ExceptBy traversal was ${trace.join(",")}`);

                  trace = [];
                  const intersect = {{intersectBy}}(
                    tracked("source", [1, 2, 3, 4, 5], trace),
                    tracked("remaining", [0, 2], trace),
                    value => {
                      trace.push(`key:${value}`);
                      return value % 3;
                    });
                  assertArray(intersect, [2, 3], "IntersectBy");
                  if (trace.join(",") !== "remaining:0,remaining:2,source:1,key:1,source:2,key:2,source:3,key:3,source:4,key:4,source:5,key:5")
                    throw new Error(`IntersectBy traversal was ${trace.join(",")}`);

                  assertArray({{unionBy}}([Number.NaN, -0, 0, Number.NaN, 1], [1, 2], value => value), [Number.NaN, -0, 1, 2], "UnionBy numeric keys");
                  const first = [1, 2, 3];
                  const second = [4, 5];
                  {{unionBy}}(first, second, value => value % 2);
                  {{exceptBy}}(first, [0], value => value % 2);
                  {{intersectBy}}(first, [0], value => value % 2);
                  if (first.join(",") !== "1,2,3" || second.join(",") !== "4,5")
                    throw new Error("key-set operators mutated their source arrays");
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
