using System.Text;
using DenoHost.Core;

namespace Jazor.CLR.Test;

[TestClass]
public sealed class EnumerableIndexRuntimeTests
{
    private const string EnumerableModulePath = "System/Linq/EnumerableModule.js";

    [TestMethod]
    public async Task IndexExport_PreservesSourceOrderAndNamedTupleShapeOnDenoHost()
    {
        var index = GetExportName("static System.Linq.Enumerable.Index<TSource>(System.Collections.Generic.IEnumerable<TSource>)");
        var root = Path.Combine(Path.GetTempPath(), "jazor-enumerable-index-" + Guid.NewGuid().ToString("N"));
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
            await File.WriteAllTextAsync(configPath, "{ \"imports\": { \"System/\": \"./System/\" } }", new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
            var testPath = Path.Combine(root, "index.test.mjs");
            await File.WriteAllTextAsync(
                testPath,
                $$"""
                import { {{index}} } from "./System/Linq/EnumerableModule.js";

                Deno.test("Enumerable.Index preserves source order and tuple names", () => {
                  const trace = [];
                  function* source() {
                    for (const value of [7, 3, 9]) {
                      trace.push(value);
                      yield value;
                    }
                  }

                  const indexed = {{index}}(source());
                  if (trace.join(",") !== "7,3,9")
                    throw new Error(`Index did not enumerate source in order: ${trace.join(",")}`);
                  if (indexed.length !== 3 || indexed[0].Index !== 0 || indexed[0].Item !== 7 || indexed[2].Index !== 2 || indexed[2].Item !== 9)
                    throw new Error(`Index tuple shape drifted: ${JSON.stringify(indexed)}`);
                  if ({{index}}([]).length !== 0)
                    throw new Error("Index must materialize an empty source as an empty Array");

                  let rejected = false;
                  try {
                    {{index}}(null);
                  } catch (error) {
                    rejected = String(error).includes("ArgumentNullException");
                  }
                  if (!rejected)
                    throw new Error("Index must reject a null source");
                });
                """,
                new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));

            using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(30));
            await Deno.Execute(new DenoExecuteBaseOptions { WorkingDirectory = root }, ["test", "--config", configPath, "--quiet", "--allow-read", testPath], timeout.Token);
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
