using System.Text;
using DenoHost.Core;

namespace Jazor.CLR.Test;

[TestClass]
public sealed class EnumerableConcatRuntimeTests
{
    private const string EnumerableModulePath = "System/Linq/EnumerableModule.js";

    [TestMethod]
    public async Task ConcatExport_PreservesFirstThenSecondEnumerationOrderOnDenoHost()
    {
        var concat = GetExportName("static System.Linq.Enumerable.Concat<TSource>(System.Collections.Generic.IEnumerable<TSource>, System.Collections.Generic.IEnumerable<TSource>)");
        var root = Path.Combine(Path.GetTempPath(), "jazor-enumerable-concat-" + Guid.NewGuid().ToString("N"));
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
            var testPath = Path.Combine(root, "concat.test.mjs");
            await File.WriteAllTextAsync(
                testPath,
                $$"""
                import { {{concat}} } from "./System/Linq/EnumerableModule.js";

                Deno.test("Concat enumerates first before second without mutating either source", () => {
                  const trace = [];
                  function* source(name, values) {
                    for (const value of values) {
                      trace.push(`${name}:${value}`);
                      yield value;
                    }
                  }

                  const result = {{concat}}(source("first", [2, 7]), source("second", [3, 9]));
                  if (result.join(",") !== "2,7,3,9")
                    throw new Error(`Concat result was ${result.join(",")}`);
                  if (trace.join(",") !== "first:2,first:7,second:3,second:9")
                    throw new Error(`Concat enumeration order was ${trace.join(",")}`);

                  const first = [2, 7];
                  const second = [3, 9];
                  {{concat}}(first, second);
                  if (first.join(",") !== "2,7" || second.join(",") !== "3,9")
                    throw new Error("Concat mutated an input source");
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
