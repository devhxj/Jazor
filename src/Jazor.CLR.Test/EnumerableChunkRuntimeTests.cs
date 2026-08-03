using System.Text;
using DenoHost.Core;

namespace Jazor.CLR.Test;

[TestClass]
public sealed class EnumerableChunkRuntimeTests
{
    private const string EnumerableModulePath = "System/Linq/EnumerableModule.js";

    [TestMethod]
    public async Task ChunkExport_PreservesSourceOrderAndCreatesIndependentChunksOnDenoHost()
    {
        var chunk = GetExportName("static System.Linq.Enumerable.Chunk<TSource>(System.Collections.Generic.IEnumerable<TSource>, int)");
        var root = Path.Combine(Path.GetTempPath(), "jazor-enumerable-chunk-" + Guid.NewGuid().ToString("N"));
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
            var testPath = Path.Combine(root, "chunk.test.mjs");
            await File.WriteAllTextAsync(
                testPath,
                $$"""
                import { {{chunk}} } from "./System/Linq/EnumerableModule.js";

                Deno.test("Chunk preserves enumeration order and independent chunk carriers", () => {
                  const trace = [];
                  function* source(values) {
                    for (const value of values) {
                      trace.push(value);
                      yield value;
                    }
                  }

                  const result = {{chunk}}(source([2, 7, 3, 9, 4]), 2);
                  if (result.map(values => values.join(",")).join("|") !== "2,7|3,9|4")
                    throw new Error(`Chunk result was ${result.map(values => values.join(",")).join("|")}`);
                  if (trace.join(",") !== "2,7,3,9,4")
                    throw new Error(`Chunk enumeration trace was ${trace.join(",")}`);

                  result[0].push(99);
                  if (result[1].join(",") !== "3,9")
                    throw new Error(`Chunk carriers were not independent: ${result[1].join(",")}`);

                  const original = [2, 7, 3, 9, 4];
                  {{chunk}}(original, 2);
                  if (original.join(",") !== "2,7,3,9,4")
                    throw new Error(`Chunk mutated source: ${original.join(",")}`);
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
