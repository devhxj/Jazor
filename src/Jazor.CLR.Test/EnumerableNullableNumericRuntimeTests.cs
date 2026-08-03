using System.Text;
using DenoHost.Core;

namespace Jazor.CLR.Test;

[TestClass]
public sealed class EnumerableNullableNumericRuntimeTests
{
    private const string ModulePath = "System/Linq/EnumerableModule.js";

    [TestMethod]
    public async Task NullableNumericTerminalExports_PreserveCarrierAndEmptySequenceContractsOnDenoHost()
    {
        var sumNames = new[]
        {
            GetExportName("static System.Linq.Enumerable.Sum(System.Collections.Generic.IEnumerable<int?>)"),
            GetExportName("static System.Linq.Enumerable.Sum(System.Collections.Generic.IEnumerable<long?>)"),
            GetExportName("static System.Linq.Enumerable.Sum(System.Collections.Generic.IEnumerable<float?>)"),
            GetExportName("static System.Linq.Enumerable.Sum(System.Collections.Generic.IEnumerable<double?>)"),
            GetExportName("static System.Linq.Enumerable.Sum(System.Collections.Generic.IEnumerable<decimal?>)")
        };
        var averageNames = new[]
        {
            GetExportName("static System.Linq.Enumerable.Average(System.Collections.Generic.IEnumerable<int?>)"),
            GetExportName("static System.Linq.Enumerable.Average(System.Collections.Generic.IEnumerable<long?>)"),
            GetExportName("static System.Linq.Enumerable.Average(System.Collections.Generic.IEnumerable<float?>)"),
            GetExportName("static System.Linq.Enumerable.Average(System.Collections.Generic.IEnumerable<double?>)"),
            GetExportName("static System.Linq.Enumerable.Average(System.Collections.Generic.IEnumerable<decimal?>)")
        };
        var root = Path.Combine(Path.GetTempPath(), "jazor-enumerable-nullable-numeric-" + Guid.NewGuid().ToString("N"));
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
            var testPath = Path.Combine(root, "nullable-numeric.test.mjs");
            await File.WriteAllTextAsync(
                testPath,
                $$"""
                import { {{string.Join(", ", sumNames.Concat(averageNames))}} } from "./System/Linq/EnumerableModule.js";

                Deno.test("Enumerable nullable numeric terminals preserve CLR contracts", () => {
                  const [sumInt, sumInt64, sumSingle, sumDouble, sumDecimal] = [{{string.Join(", ", sumNames)}}];
                  const [averageInt, averageInt64, averageSingle, averageDouble, averageDecimal] = [{{string.Join(", ", averageNames)}}];
                  const integerSource = [null, 1, null, 2];
                  if (sumInt(integerSource) !== 3 || averageInt(integerSource) !== 1.5 || sumInt([null, null]) !== 0 || averageInt([null, null]) !== null)
                    throw new Error("Nullable Int32 terminals must ignore null and preserve zero/null empty contracts");
                  if (JSON.stringify(integerSource) !== "[null,1,null,2]")
                    throw new Error("Nullable terminals must not mutate their source");
                  if (sumInt64([null, 7n, -2n]) !== 5n || averageInt64([null, 7n, -2n]) !== 2.5 || sumInt64([null, null]) !== 0n || averageInt64([null, null]) !== null)
                    throw new Error("Nullable Int64 terminals lost their BigInt or empty-sequence contracts");
                  if (sumSingle([null, 0.1, 0.2]) !== Math.fround(0.1 + 0.2) || averageSingle([null, 1, 2]) !== Math.fround(1.5) || averageSingle([null, null]) !== null)
                    throw new Error("Nullable Single terminals must round after wide accumulation");
                  if (!Number.isNaN(sumDouble([null, 1, NaN])) || !Number.isNaN(averageDouble([null, 1, NaN])) || averageDouble([null, null]) !== null)
                    throw new Error("Nullable Double terminals must preserve NaN and empty-sequence behavior");
                  if (sumDecimal(["3.25", null, "-1.50"]) !== "1.75" || averageDecimal(["3.25", null, "-1.50"]) !== "0.875" || sumDecimal([null, null]) !== "0" || averageDecimal([null, null]) !== null)
                    throw new Error("Nullable Decimal terminals must retain the exact decimal carrier");

                  for (const operation of [sumInt, sumInt64, sumSingle, sumDouble, sumDecimal, averageInt, averageInt64, averageSingle, averageDouble, averageDecimal]) {
                    let rejected = false;
                    try { operation(null); } catch (error) { rejected = String(error).includes("ArgumentNullException"); }
                    if (!rejected) throw new Error("Nullable numeric terminal must reject a null source");
                  }
                  for (const [operation, values, errorName] of [
                    [sumInt, [null, 2147483647, 1], "OverflowException"],
                    [sumInt64, [null, 9223372036854775807n, 1n], "OverflowException"],
                    [averageInt64, [null, 9223372036854775807n, 9223372036854775807n], "OverflowException"],
                    [sumDecimal, [null, "79228162514264337593543950335", "1"], "OverflowException"],
                    [averageDecimal, [null, "79228162514264337593543950335", "79228162514264337593543950335"], "OverflowException"]
                  ]) {
                    let rejected = false;
                    try { operation(values); } catch (error) { rejected = String(error).includes(errorName); }
                    if (!rejected) throw new Error(`Nullable numeric terminal must reject ${errorName}`);
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
