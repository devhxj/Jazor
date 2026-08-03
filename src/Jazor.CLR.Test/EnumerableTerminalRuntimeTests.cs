using System.Text;
using DenoHost.Core;

namespace Jazor.CLR.Test;

[TestClass]
public sealed class EnumerableTerminalRuntimeTests
{
    private const string EnumerableModulePath = "System/Linq/EnumerableModule.js";

    [TestMethod]
    public async Task FirstLastAndSingleExports_PreserveTerminalEnumerationOrderOnDenoHost()
    {
        var first = GetExportName("static System.Linq.Enumerable.First<TSource>(System.Collections.Generic.IEnumerable<TSource>)");
        var firstWhere = GetExportName("static System.Linq.Enumerable.First<TSource>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, bool>)");
        var last = GetExportName("static System.Linq.Enumerable.Last<TSource>(System.Collections.Generic.IEnumerable<TSource>)");
        var lastWhere = GetExportName("static System.Linq.Enumerable.Last<TSource>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, bool>)");
        var single = GetExportName("static System.Linq.Enumerable.Single<TSource>(System.Collections.Generic.IEnumerable<TSource>)");
        var singleWhere = GetExportName("static System.Linq.Enumerable.Single<TSource>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, bool>)");
        var root = Path.Combine(Path.GetTempPath(), "jazor-enumerable-terminal-" + Guid.NewGuid().ToString("N"));
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
            var testPath = Path.Combine(root, "terminal.test.mjs");
            await File.WriteAllTextAsync(
                testPath,
                $$"""
                import { {{first}}, {{firstWhere}}, {{last}}, {{lastWhere}}, {{single}}, {{singleWhere}} } from "./System/Linq/EnumerableModule.js";

                Deno.test("First, Last, and Single preserve terminal enumeration order", () => {
                  const source = [3, 1, 4, 2];
                  if ({{first}}(source) !== 3)
                    throw new Error("First did not return the first source item");
                  if ({{last}}(source) !== 2)
                    throw new Error("Last did not return the final source item");

                  const firstTrace = [];
                  const firstMatch = {{firstWhere}}(source, value => {
                    firstTrace.push(value);
                    return value % 2 === 0;
                  });
                  if (firstMatch !== 4 || firstTrace.join(",") !== "3,1,4")
                    throw new Error(`First predicate did not short-circuit: ${firstMatch} / ${firstTrace.join(",")}`);

                  const lastTrace = [];
                  const lastMatch = {{lastWhere}}(source, value => {
                    lastTrace.push(value);
                    return value % 2 === 0;
                  });
                  if (lastMatch !== 2 || lastTrace.join(",") !== "3,1,4,2")
                    throw new Error(`Last predicate did not visit the whole source: ${lastMatch} / ${lastTrace.join(",")}`);
                  if (source.join(",") !== "3,1,4,2")
                    throw new Error(`terminal operators mutated source: ${source.join(",")}`);

                  if ({{single}}([7]) !== 7)
                    throw new Error("Single did not return the only source item");

                  const singleTrace = [];
                  const singleMatch = {{singleWhere}}([1, 2, 3], value => {
                    singleTrace.push(value);
                    return value === 2;
                  });
                  if (singleMatch !== 2 || singleTrace.join(",") !== "1,2,3")
                    throw new Error(`Single predicate did not verify the complete source: ${singleMatch} / ${singleTrace.join(",")}`);

                  const duplicateTrace = [];
                  let rejected = false;
                  try {
                    {{singleWhere}}([1, 2, 4, 6], value => {
                      duplicateTrace.push(value);
                      return value % 2 === 0;
                    });
                  } catch (error) {
                    rejected = String(error).includes("more than one matching element");
                  }
                  if (!rejected || duplicateTrace.join(",") !== "1,2,4")
                    throw new Error(`Single predicate did not fail at the second match: ${duplicateTrace.join(",")}`);
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
