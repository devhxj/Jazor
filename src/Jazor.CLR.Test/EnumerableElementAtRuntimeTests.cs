using System.Text;
using DenoHost.Core;

namespace Jazor.CLR.Test;

[TestClass]
public sealed class EnumerableElementAtRuntimeTests
{
    private const string EnumerableModulePath = "System/Linq/EnumerableModule.js";

    [TestMethod]
    public async Task ElementAtExports_PreserveFromStartAndFromEndTraversalOnDenoHost()
    {
        var elementAt = GetExportName("static System.Linq.Enumerable.ElementAt<TSource>(System.Collections.Generic.IEnumerable<TSource>, int)");
        var elementAtIndex = GetExportName("static System.Linq.Enumerable.ElementAt<TSource>(System.Collections.Generic.IEnumerable<TSource>, System.Index)");
        var fromStart = GetExportName("static System.Index.FromStart(int)", "System/IndexModule.js");
        var fromEnd = GetExportName("static System.Index.FromEnd(int)", "System/IndexModule.js");
        var root = Path.Combine(Path.GetTempPath(), "jazor-enumerable-element-at-" + Guid.NewGuid().ToString("N"));
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
            var testPath = Path.Combine(root, "element-at.test.mjs");
            await File.WriteAllTextAsync(
                testPath,
                $$"""
                import { {{elementAt}}, {{elementAtIndex}} } from "./System/Linq/EnumerableModule.js";
                import { {{fromStart}}, {{fromEnd}} } from "./System/IndexModule.js";

                Deno.test("ElementAt stops enumeration at the target index", () => {
                  const trace = [];
                  function* source(values) {
                    for (const value of values) {
                      trace.push(value);
                      yield value;
                    }
                  }

                  const result = {{elementAt}}(source([2, 7, 9]), 1);
                  if (result !== 7)
                    throw new Error(`ElementAt result was ${result}`);
                  if (trace.join(",") !== "2,7")
                    throw new Error(`ElementAt enumerated beyond its bound index: ${trace.join(",")}`);

                  const original = [2, 7, 9];
                  {{elementAt}}(original, 1);
                  if (original.join(",") !== "2,7,9")
                    throw new Error(`ElementAt mutated source: ${original.join(",")}`);

                  for (const [sourceValue, index, expectedMessage] of [[original, -1, "index is less than zero"], [original, 3, "index is out of range."]]) {
                    let error = null;
                    try {
                      {{elementAt}}(sourceValue, index);
                    } catch (caught) {
                      error = caught;
                    }
                    if (!(error instanceof Error) || !error.message.includes(expectedMessage))
                      throw new Error(`ElementAt index ${index} error was ${String(error)}`);
                  }

                  trace.length = 0;
                  const fromStartResult = {{elementAtIndex}}(source([2, 7, 9]), {{fromStart}}(1));
                  if (fromStartResult !== 7 || trace.join(",") !== "2,7")
                    throw new Error(`ElementAt(Index) from-start traversal was ${fromStartResult}/${trace.join(",")}`);

                  trace.length = 0;
                  const fromEndResult = {{elementAtIndex}}(source([2, 7, 9]), {{fromEnd}}(2));
                  if (fromEndResult !== 7 || trace.join(",") !== "2,7,9")
                    throw new Error(`ElementAt(Index) from-end traversal was ${fromEndResult}/${trace.join(",")}`);

                  for (const index of [{{fromEnd}}(0), {{fromStart}}(3), {{fromEnd}}(4)]) {
                    let error = null;
                    try {
                      {{elementAtIndex}}(original, index);
                    } catch (caught) {
                      error = caught;
                    }
                    if (!(error instanceof Error) || !error.message.includes("index is out of range."))
                      throw new Error(`ElementAt(Index) boundary error was ${String(error)}`);
                  }
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
        => GetExportName(member, EnumerableModulePath);

    private static string GetExportName(string member, string modulePath)
    {
        var mapping = ClrRuntimeMappingCatalog.GetImport(member);
        Assert.AreEqual(modulePath, mapping.ModulePath, member);
        return mapping.ExportName;
    }
}
