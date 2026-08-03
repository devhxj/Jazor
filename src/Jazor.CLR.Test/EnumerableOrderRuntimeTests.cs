using System.Text;
using DenoHost.Core;

namespace Jazor.CLR.Test;

[TestClass]
public sealed class EnumerableOrderRuntimeTests
{
    private const string EnumerableModulePath = "System/Linq/EnumerableModule.js";

    [TestMethod]
    public async Task OrderExports_UseDefaultComparerWithoutMutatingSourceOnDenoHost()
    {
        var order = GetExportName("static System.Linq.Enumerable.Order<T>(System.Collections.Generic.IEnumerable<T>)");
        var orderDescending = GetExportName("static System.Linq.Enumerable.OrderDescending<T>(System.Collections.Generic.IEnumerable<T>)");
        var root = Path.Combine(Path.GetTempPath(), "jazor-enumerable-order-" + Guid.NewGuid().ToString("N"));
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
            var testPath = Path.Combine(root, "order.test.mjs");
            await File.WriteAllTextAsync(
                testPath,
                $$"""
                import { {{order}}, {{orderDescending}} } from "./System/Linq/EnumerableModule.js";

                Deno.test("Order and OrderDescending use the default comparer without source mutation", () => {
                  const source = [2, 7, 4, 1];
                  const ascending = {{order}}(source);
                  const descending = {{orderDescending}}(source);
                  if (ascending.join(",") !== "1,2,4,7")
                    throw new Error(`Order result was ${ascending.join(",")}`);
                  if (descending.join(",") !== "7,4,2,1")
                    throw new Error(`OrderDescending result was ${descending.join(",")}`);
                  if (source.join(",") !== "2,7,4,1")
                    throw new Error(`Order mutated source: ${source.join(",")}`);
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
