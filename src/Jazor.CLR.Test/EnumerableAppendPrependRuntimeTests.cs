using System.Text;
using DenoHost.Core;

namespace Jazor.CLR.Test;

[TestClass]
public sealed class EnumerableAppendPrependRuntimeTests
{
    private const string EnumerableModulePath = "System/Linq/EnumerableModule.js";

    [TestMethod]
    public async Task AppendAndPrependExports_PreserveEnumerationAndSourceOrderOnDenoHost()
    {
        var append = GetExportName("static System.Linq.Enumerable.Append<TSource>(System.Collections.Generic.IEnumerable<TSource>, TSource)");
        var prepend = GetExportName("static System.Linq.Enumerable.Prepend<TSource>(System.Collections.Generic.IEnumerable<TSource>, TSource)");
        var root = Path.Combine(Path.GetTempPath(), "jazor-enumerable-append-prepend-" + Guid.NewGuid().ToString("N"));
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
            var testPath = Path.Combine(root, "append-prepend.test.mjs");
            await File.WriteAllTextAsync(
                testPath,
                $$"""
                import { {{append}}, {{prepend}} } from "./System/Linq/EnumerableModule.js";

                Deno.test("Append and Prepend preserve enumerable order without mutating sources", () => {
                  const trace = [];
                  function* source(name, values) {
                    for (const value of values) {
                      trace.push(`${name}:${value}`);
                      yield value;
                    }
                  }

                  const appended = {{append}}(source("append", [2, 7]), 9);
                  if (appended.join(",") !== "2,7,9")
                    throw new Error(`Append result was ${appended.join(",")}`);
                  if (trace.join(",") !== "append:2,append:7")
                    throw new Error(`Append source enumeration was ${trace.join(",")}`);

                  trace.length = 0;
                  const prepended = {{prepend}}(source("prepend", [2, 7]), 1);
                  if (prepended.join(",") !== "1,2,7")
                    throw new Error(`Prepend result was ${prepended.join(",")}`);
                  if (trace.join(",") !== "prepend:2,prepend:7")
                    throw new Error(`Prepend source enumeration was ${trace.join(",")}`);

                  const original = [2, 7];
                  {{append}}(original, 9);
                  {{prepend}}(original, 1);
                  if (original.join(",") !== "2,7")
                    throw new Error(`Append or Prepend mutated source: ${original.join(",")}`);
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
