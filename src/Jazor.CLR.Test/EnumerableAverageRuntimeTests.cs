using System.Text;
using DenoHost.Core;

namespace Jazor.CLR.Test;

[TestClass]
public sealed class EnumerableAverageRuntimeTests
{
    private const string ModulePath = "System/Linq/EnumerableModule.js";

    [TestMethod]
    public async Task AverageExports_PreserveNumericCarrierPrecisionAndEmptySourceContractsOnDenoHost()
    {
        var names = new[]
        {
            GetExportName("static System.Linq.Enumerable.Average(System.Collections.Generic.IEnumerable<int>)"),
            GetExportName("static System.Linq.Enumerable.Average(System.Collections.Generic.IEnumerable<long>)"),
            GetExportName("static System.Linq.Enumerable.Average(System.Collections.Generic.IEnumerable<float>)"),
            GetExportName("static System.Linq.Enumerable.Average(System.Collections.Generic.IEnumerable<double>)"),
            GetExportName("static System.Linq.Enumerable.Average(System.Collections.Generic.IEnumerable<decimal>)")
        };
        var root = Path.Combine(Path.GetTempPath(), "jazor-enumerable-average-" + Guid.NewGuid().ToString("N"));
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
            var testPath = Path.Combine(root, "average.test.mjs");
            await File.WriteAllTextAsync(
                testPath,
                $$"""
                import { {{string.Join(", ", names)}} } from "./System/Linq/EnumerableModule.js";

                Deno.test("Enumerable.Average preserves numeric carrier contracts", () => {
                  const [averageIntOperation, averageInt64Operation, averageSingleOperation, averageDoubleOperation, averageDecimalOperation] = [{{string.Join(", ", names)}}];
                  if (averageIntOperation([1, 2]) !== 1.5 || averageIntOperation([2147483647, 2147483647]) !== 2147483647)
                    throw new Error("Int32 Average must use the Int64 accumulator contract");
                  if (averageInt64Operation([7n, -2n, 4n]) !== 3)
                    throw new Error("Int64 Average must convert the checked integral result to Number");
                  if (averageSingleOperation([0.1, 0.2]) !== Math.fround((0.1 + 0.2) / 2) || !Number.isNaN(averageSingleOperation([1, NaN])))
                    throw new Error("Single Average must round once after wide accumulation and propagate NaN");
                  if (averageDoubleOperation([0.1, 0.2]) !== (0.1 + 0.2) / 2 || !Number.isNaN(averageDoubleOperation([1, NaN])))
                    throw new Error("Double Average precision or NaN behavior drifted");
                  if (averageDecimalOperation(["3.25", "-1.50"]) !== "0.875")
                    throw new Error("Decimal Average must use the decimal carrier");

                  for (const [operation, values, expected] of [
                    [averageIntOperation, [], "InvalidOperationException"],
                    [averageInt64Operation, [], "InvalidOperationException"],
                    [averageSingleOperation, [], "InvalidOperationException"],
                    [averageDoubleOperation, [], "InvalidOperationException"],
                    [averageDecimalOperation, [], "InvalidOperationException"],
                    [averageInt64Operation, [9223372036854775807n, 9223372036854775807n], "OverflowException"],
                    [averageDecimalOperation, ["79228162514264337593543950335", "79228162514264337593543950335"], "OverflowException"]
                  ]) {
                    let rejected = false;
                    try { operation(values); } catch (error) { rejected = String(error).includes(expected); }
                    if (!rejected) throw new Error(`Average must reject ${expected}`);
                  }

                  let nullRejected = false;
                  try { averageIntOperation(null); } catch (error) { nullRejected = String(error).includes("ArgumentNullException"); }
                  if (!nullRejected) throw new Error("Average must reject a null source");
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
