using System.Text;
using DenoHost.Core;

namespace Jazor.CLR.Test;

[TestClass]
public sealed class EnumerableSumRuntimeTests
{
    private const string ModulePath = "System/Linq/EnumerableModule.js";

    [TestMethod]
    public async Task SumExports_PreserveNumericCarrierPrecisionAndOverflowContractsOnDenoHost()
    {
        var names = new[]
        {
            GetExportName("static System.Linq.Enumerable.Sum(System.Collections.Generic.IEnumerable<int>)"),
            GetExportName("static System.Linq.Enumerable.Sum(System.Collections.Generic.IEnumerable<long>)"),
            GetExportName("static System.Linq.Enumerable.Sum(System.Collections.Generic.IEnumerable<float>)"),
            GetExportName("static System.Linq.Enumerable.Sum(System.Collections.Generic.IEnumerable<double>)"),
            GetExportName("static System.Linq.Enumerable.Sum(System.Collections.Generic.IEnumerable<decimal>)")
        };
        var root = Path.Combine(Path.GetTempPath(), "jazor-enumerable-sum-" + Guid.NewGuid().ToString("N"));
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
            var testPath = Path.Combine(root, "sum.test.mjs");
            await File.WriteAllTextAsync(
                testPath,
                $$"""
                import { {{string.Join(", ", names)}} } from "./System/Linq/EnumerableModule.js";

                Deno.test("Enumerable.Sum preserves numeric carrier contracts", () => {
                  const [sumIntOperation, sumInt64Operation, sumSingleOperation, sumDoubleOperation, sumDecimalOperation] = [{{string.Join(", ", names)}}];
                  if (sumIntOperation([7, -2, 4]) !== 9 || sumIntOperation([]) !== 0)
                    throw new Error("Int32 Sum normal or empty behavior drifted");
                  if (sumInt64Operation([7n, -2n, 4n]) !== 9n || sumInt64Operation([]) !== 0n)
                    throw new Error("Int64 Sum must preserve BigInt values");
                  if (sumSingleOperation([0.1, 0.2]) !== Math.fround(0.1 + 0.2) || !Number.isNaN(sumSingleOperation([1, NaN])))
                    throw new Error("Single Sum must round once after wide accumulation and propagate NaN");
                  if (sumDoubleOperation([0.1, 0.2]) !== 0.1 + 0.2 || !Number.isNaN(sumDoubleOperation([1, NaN])))
                    throw new Error("Double Sum precision or NaN behavior drifted");
                  if (sumDecimalOperation([]) !== "0" || sumDecimalOperation(["3.25", "-1.50"]) !== "1.75")
                    throw new Error("Decimal Sum must preserve the decimal carrier");

                  for (const [operation, values] of [
                    [sumIntOperation, [2147483647, 1]],
                    [sumInt64Operation, [9223372036854775807n, 1n]],
                    [sumDecimalOperation, ["79228162514264337593543950335", "1"]]
                  ]) {
                    let overflowed = false;
                    try { operation(values); } catch (error) { overflowed = String(error).includes("OverflowException"); }
                    if (!overflowed) throw new Error("Integral and decimal Sum must reject overflow");
                  }

                  let nullRejected = false;
                  try { sumIntOperation(null); } catch (error) { nullRejected = String(error).includes("ArgumentNullException"); }
                  if (!nullRejected) throw new Error("Sum must reject a null source");
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
