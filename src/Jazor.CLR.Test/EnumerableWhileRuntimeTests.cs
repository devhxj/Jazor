using System.Text;
using DenoHost.Core;

namespace Jazor.CLR.Test;

[TestClass]
public sealed class EnumerableWhileRuntimeTests
{
    private const string EnumerableModulePath = "System/Linq/EnumerableModule.js";

    [TestMethod]
    public async Task SkipTakeWhileExports_PreservePredicateAndIteratorTerminationOnDenoHost()
    {
        var skipWhile = GetExportName("static System.Linq.Enumerable.SkipWhile<TSource>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, bool>)");
        var skipWhileAt = GetExportName("static System.Linq.Enumerable.SkipWhile<TSource>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, int, bool>)");
        var takeWhile = GetExportName("static System.Linq.Enumerable.TakeWhile<TSource>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, bool>)");
        var takeWhileAt = GetExportName("static System.Linq.Enumerable.TakeWhile<TSource>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, int, bool>)");
        var root = Path.Combine(Path.GetTempPath(), "jazor-enumerable-while-" + Guid.NewGuid().ToString("N"));
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
            var testPath = Path.Combine(root, "while-operators.test.mjs");
            await File.WriteAllTextAsync(
                testPath,
                $$"""
                import { {{skipWhile}}, {{skipWhileAt}}, {{takeWhile}}, {{takeWhileAt}} } from "./System/Linq/EnumerableModule.js";

                function assertArray(actual, expected, name) {
                  if (actual.length !== expected.length || actual.some((value, index) => value !== expected[index]))
                    throw new Error(`${name}: ${JSON.stringify(actual)} did not match ${JSON.stringify(expected)}`);
                }

                function tracked(values, trace) {
                  return {
                    [Symbol.iterator]() {
                      let index = 0;
                      return {
                        next() {
                          if (index === values.length) {
                            trace.push("source:done");
                            return { done: true };
                          }
                          const value = values[index++];
                          trace.push(`source:${value}`);
                          return { value, done: false };
                        },
                        return() {
                          trace.push("source:return");
                          return { done: true };
                        }
                      };
                    }
                  };
                }

                Deno.test("SkipWhile and TakeWhile preserve predicate stop and iterator close behavior", () => {
                  let trace = [];
                  assertArray({{skipWhile}}(tracked([2, 4, 1, 6], trace), value => {
                    trace.push(`predicate:${value}`);
                    return value % 2 === 0;
                  }), [1, 6], "SkipWhile");
                  if (trace.join(",") !== "source:2,predicate:2,source:4,predicate:4,source:1,predicate:1,source:6,source:done")
                    throw new Error(`SkipWhile traversal was ${trace.join(",")}`);

                  trace = [];
                  assertArray({{takeWhile}}(tracked([2, 4, 1, 6], trace), value => {
                    trace.push(`predicate:${value}`);
                    return value % 2 === 0;
                  }), [2, 4], "TakeWhile");
                  if (trace.join(",") !== "source:2,predicate:2,source:4,predicate:4,source:1,predicate:1,source:return")
                    throw new Error(`TakeWhile traversal was ${trace.join(",")}`);

                  trace = [];
                  assertArray({{skipWhileAt}}(tracked([10, 20, 30], trace), (value, index) => {
                    trace.push(`predicate:${value}:${index}`);
                    return index < 1;
                  }), [20, 30], "SkipWhile indexed");
                  if (trace.join(",") !== "source:10,predicate:10:0,source:20,predicate:20:1,source:30,source:done")
                    throw new Error(`indexed SkipWhile traversal was ${trace.join(",")}`);

                  trace = [];
                  assertArray({{takeWhileAt}}(tracked([10, 20, 30, 40], trace), (value, index) => {
                    trace.push(`predicate:${value}:${index}`);
                    return index < 2;
                  }), [10, 20], "TakeWhile indexed");
                  if (trace.join(",") !== "source:10,predicate:10:0,source:20,predicate:20:1,source:30,predicate:30:2,source:return")
                    throw new Error(`indexed TakeWhile traversal was ${trace.join(",")}`);

                  const original = [2, 4, 1, 6];
                  {{skipWhile}}(original, value => value % 2 === 0);
                  {{takeWhile}}(original, value => value % 2 === 0);
                  if (original.join(",") !== "2,4,1,6")
                    throw new Error("while operators mutated their source array");
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
