using System.Text;
using DenoHost.Core;

namespace Jazor.CLR.Test;

[TestClass]
public sealed class EnumerableTryGetNonEnumeratedCountRuntimeTests
{
    private const string EnumerableModulePath = "System/Linq/EnumerableModule.js";
    private const string Member = "static System.Linq.Enumerable.TryGetNonEnumeratedCount<TSource>(System.Collections.Generic.IEnumerable<TSource>, out int)";

    [TestMethod]
    public async Task TryGetNonEnumeratedCountExport_UsesArrayCarrierLengthWithoutIteratingOnDenoHost()
    {
        var exportName = GetExportName();
        var root = Path.Combine(Path.GetTempPath(), "jazor-enumerable-try-get-count-" + Guid.NewGuid().ToString("N"));
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
            var testPath = Path.Combine(root, "try-get-non-enumerated-count.test.mjs");
            await File.WriteAllTextAsync(
                testPath,
                $$"""
                import { {{exportName}} } from "./System/Linq/EnumerableModule.js";

                Deno.test("Enumerable.TryGetNonEnumeratedCount reads Array length without advancing its iterator", () => {
                  const source = [5, 8, 13];
                  source[Symbol.iterator] = () => {
                    throw new Error("TryGetNonEnumeratedCount must not enumerate source");
                  };

                  const result = {{exportName}}(source, -1);
                  if (!Array.isArray(result) || result[0] !== true || result[1] !== 3)
                    throw new Error(`unexpected known count result: ${JSON.stringify(result)}`);

                  const empty = {{exportName}}([], 99);
                  if (empty[0] !== true || empty[1] !== 0)
                    throw new Error(`unexpected empty result: ${JSON.stringify(empty)}`);

                  let rejected = false;
                  try {
                    {{exportName}}(null, 0);
                  } catch (error) {
                    rejected = String(error).includes("ArgumentNullException");
                  }
                  if (!rejected)
                    throw new Error("TryGetNonEnumeratedCount must reject a null source");
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

    private static string GetExportName()
    {
        var mapping = ClrRuntimeMappingCatalog.GetImport(Member);
        Assert.AreEqual(EnumerableModulePath, mapping.ModulePath, Member);
        return mapping.ExportName;
    }
}
