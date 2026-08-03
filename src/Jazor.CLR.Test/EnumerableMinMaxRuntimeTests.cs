using System.Text;
using DenoHost.Core;

namespace Jazor.CLR.Test;

[TestClass]
public sealed class EnumerableMinMaxRuntimeTests
{
    private const string ModulePath = "System/Linq/EnumerableModule.js";

    [TestMethod]
    public async Task MinMaxExports_PreserveNumericCarrierAndNaNContractsOnDenoHost()
    {
        var names = new[]
        {
            GetExportName("static System.Linq.Enumerable.Min(System.Collections.Generic.IEnumerable<int>)"),
            GetExportName("static System.Linq.Enumerable.Max(System.Collections.Generic.IEnumerable<int>)"),
            GetExportName("static System.Linq.Enumerable.Min(System.Collections.Generic.IEnumerable<long>)"),
            GetExportName("static System.Linq.Enumerable.Max(System.Collections.Generic.IEnumerable<long>)"),
            GetExportName("static System.Linq.Enumerable.Min(System.Collections.Generic.IEnumerable<float>)"),
            GetExportName("static System.Linq.Enumerable.Max(System.Collections.Generic.IEnumerable<float>)"),
            GetExportName("static System.Linq.Enumerable.Min(System.Collections.Generic.IEnumerable<double>)"),
            GetExportName("static System.Linq.Enumerable.Max(System.Collections.Generic.IEnumerable<double>)"),
            GetExportName("static System.Linq.Enumerable.Min(System.Collections.Generic.IEnumerable<decimal>)"),
            GetExportName("static System.Linq.Enumerable.Max(System.Collections.Generic.IEnumerable<decimal>)")
        };
        var root = Path.Combine(Path.GetTempPath(), "jazor-enumerable-min-max-" + Guid.NewGuid().ToString("N"));
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
            var testPath = Path.Combine(root, "min-max.test.mjs");
            await File.WriteAllTextAsync(
                testPath,
                $$"""
                import { {{string.Join(", ", names)}} } from "./System/Linq/EnumerableModule.js";

                Deno.test("Enumerable.Min and Max preserve numeric carriers and NaN behavior", () => {
                  const [minIntOperation, maxIntOperation, minInt64Operation, maxInt64Operation, minSingleOperation, maxSingleOperation, minDoubleOperation, maxDoubleOperation, minDecimalOperation, maxDecimalOperation] = [{{string.Join(", ", names)}}];
                  if (minIntOperation([7, -2, 4]) !== -2 || maxIntOperation([7, -2, 4]) !== 7)
                    throw new Error("Int32 Min/Max drifted");
                  if (minInt64Operation([7n, -2n, 4n]) !== -2n || maxInt64Operation([7n, -2n, 4n]) !== 7n)
                    throw new Error("Int64 Min/Max must preserve BigInt values");
                  if (!Number.isNaN(minSingleOperation([1, NaN])) || maxSingleOperation([NaN, 1]) !== 1)
                    throw new Error("Single NaN Min/Max behavior drifted");
                  if (!Number.isNaN(minDoubleOperation([1, NaN])) || maxDoubleOperation([1, NaN]) !== 1)
                    throw new Error("Double NaN Min/Max behavior drifted");
                  if (minDecimalOperation(["3.25", "-1.50"]) !== "-1.50" || maxDecimalOperation(["3.25", "-1.50"]) !== "3.25" || minDecimalOperation(["10", "2"]) !== "2" || maxDecimalOperation(["10", "2"]) !== "10")
                    throw new Error("Decimal Min/Max must use decimal comparison instead of string ordering");
                  for (const operation of [minIntOperation, maxIntOperation, minInt64Operation, maxInt64Operation, minSingleOperation, maxSingleOperation, minDoubleOperation, maxDoubleOperation, minDecimalOperation, maxDecimalOperation]) {
                    let rejected = false;
                    try { operation([]); } catch (error) { rejected = String(error).includes("InvalidOperationException"); }
                    if (!rejected) throw new Error("Min/Max must reject an empty non-nullable source");
                  }
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
        Assert.AreEqual(ModulePath, mapping.ModulePath, member);
        return mapping.ExportName;
    }
}
