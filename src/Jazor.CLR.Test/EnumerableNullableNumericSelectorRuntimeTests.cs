using System.Text;
using DenoHost.Core;

namespace Jazor.CLR.Test;

[TestClass]
public sealed class EnumerableNullableNumericSelectorRuntimeTests
{
    private const string ModulePath = "System/Linq/EnumerableModule.js";

    [TestMethod]
    public async Task NullableNumericSelectorExports_PreserveSingleEvaluationAndCarrierContractsOnDenoHost()
    {
        var names = new[]
        {
            GetExportName("static System.Linq.Enumerable.Sum<TSource>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, int?>)"),
            GetExportName("static System.Linq.Enumerable.Sum<TSource>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, long?>)"),
            GetExportName("static System.Linq.Enumerable.Sum<TSource>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, float?>)"),
            GetExportName("static System.Linq.Enumerable.Sum<TSource>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, double?>)"),
            GetExportName("static System.Linq.Enumerable.Sum<TSource>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, decimal?>)"),
            GetExportName("static System.Linq.Enumerable.Average<TSource>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, int?>)"),
            GetExportName("static System.Linq.Enumerable.Average<TSource>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, long?>)"),
            GetExportName("static System.Linq.Enumerable.Average<TSource>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, float?>)"),
            GetExportName("static System.Linq.Enumerable.Average<TSource>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, double?>)"),
            GetExportName("static System.Linq.Enumerable.Average<TSource>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, decimal?>)"),
            GetExportName("static System.Linq.Enumerable.Min<TSource>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, int?>)"),
            GetExportName("static System.Linq.Enumerable.Min<TSource>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, long?>)"),
            GetExportName("static System.Linq.Enumerable.Min<TSource>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, float?>)"),
            GetExportName("static System.Linq.Enumerable.Min<TSource>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, double?>)"),
            GetExportName("static System.Linq.Enumerable.Min<TSource>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, decimal?>)"),
            GetExportName("static System.Linq.Enumerable.Max<TSource>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, int?>)"),
            GetExportName("static System.Linq.Enumerable.Max<TSource>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, long?>)"),
            GetExportName("static System.Linq.Enumerable.Max<TSource>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, float?>)"),
            GetExportName("static System.Linq.Enumerable.Max<TSource>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, double?>)"),
            GetExportName("static System.Linq.Enumerable.Max<TSource>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, decimal?>)")
        };
        var root = Path.Combine(Path.GetTempPath(), "jazor-enumerable-nullable-numeric-selector-" + Guid.NewGuid().ToString("N"));
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
            var testPath = Path.Combine(root, "nullable-numeric-selector.test.mjs");
            await File.WriteAllTextAsync(
                testPath,
                $$"""
                import { {{string.Join(", ", names)}} } from "./System/Linq/EnumerableModule.js";

                Deno.test("Enumerable nullable numeric selector terminals preserve CLR contracts", () => {
                  const [sumInt, sumInt64, sumSingle, sumDouble, sumDecimal, averageInt, averageInt64, averageSingle, averageDouble, averageDecimal, minInt, minInt64, minSingle, minDouble, minDecimal, maxInt, maxInt64, maxSingle, maxDouble, maxDecimal] = [{{string.Join(", ", names)}}];
                  const entries = [
                    { id: 1, int32: 1, int64: 1n, single: 1.5, double: 1.5, decimal: "1.5" },
                    { id: 2, int32: null, int64: null, single: null, double: null, decimal: null },
                    { id: 3, int32: 2, int64: 2n, single: 2.5, double: 2.5, decimal: "2.5" }
                  ];
                  const operations = [
                    [sumInt, entry => entry.int32], [sumInt64, entry => entry.int64], [sumSingle, entry => entry.single], [sumDouble, entry => entry.double], [sumDecimal, entry => entry.decimal],
                    [averageInt, entry => entry.int32], [averageInt64, entry => entry.int64], [averageSingle, entry => entry.single], [averageDouble, entry => entry.double], [averageDecimal, entry => entry.decimal],
                    [minInt, entry => entry.int32], [minInt64, entry => entry.int64], [minSingle, entry => entry.single], [minDouble, entry => entry.double], [minDecimal, entry => entry.decimal],
                    [maxInt, entry => entry.int32], [maxInt64, entry => entry.int64], [maxSingle, entry => entry.single], [maxDouble, entry => entry.double], [maxDecimal, entry => entry.decimal]
                  ];
                  for (const [operation, selector] of operations) {
                    const trace = [];
                    operation(entries, entry => {
                      trace.push(entry.id);
                      return selector(entry);
                    });
                    if (trace.join(",") !== "1,2,3")
                      throw new Error("Nullable numeric selector must run once in source order for every item");
                  }

                  if (sumInt(entries, entry => entry.int32) !== 3 || sumInt64(entries, entry => entry.int64) !== 3n || sumSingle(entries, entry => entry.single) !== 4 || sumDouble(entries, entry => entry.double) !== 4 || sumDecimal(entries, entry => entry.decimal) !== "4.0")
                    throw new Error("Nullable selector Sum carrier or null filtering drifted");
                  if (averageInt(entries, entry => entry.int32) !== 1.5 || averageInt64(entries, entry => entry.int64) !== 1.5 || averageSingle(entries, entry => entry.single) !== 2 || averageDouble(entries, entry => entry.double) !== 2 || averageDecimal(entries, entry => entry.decimal) !== "2")
                    throw new Error("Nullable selector Average carrier or null filtering drifted");
                  if (minInt(entries, entry => entry.int32) !== 1 || minInt64(entries, entry => entry.int64) !== 1n || minSingle(entries, entry => entry.single) !== 1.5 || minDouble(entries, entry => entry.double) !== 1.5 || minDecimal(entries, entry => entry.decimal) !== "1.5")
                    throw new Error("Nullable selector Min carrier contract drifted");
                  if (maxInt(entries, entry => entry.int32) !== 2 || maxInt64(entries, entry => entry.int64) !== 2n || maxSingle(entries, entry => entry.single) !== 2.5 || maxDouble(entries, entry => entry.double) !== 2.5 || maxDecimal(entries, entry => entry.decimal) !== "2.5")
                    throw new Error("Nullable selector Max carrier contract drifted");
                  if (sumInt(entries, () => null) !== 0 || averageDecimal(entries, () => null) !== null || minInt(entries, () => null) !== null || maxDecimal(entries, () => null) !== null)
                    throw new Error("Nullable selector terminals must preserve zero/null empty contracts");
                  if (!Number.isNaN(minDouble(entries, entry => entry.id === 3 ? Number.NaN : entry.double)) || maxDouble(entries, entry => entry.id === 1 ? Number.NaN : entry.double) !== 2.5)
                    throw new Error("Nullable selector Min/Max NaN behavior drifted");
                  for (const operation of operations.map(([operation]) => operation)) {
                    let rejected = false;
                    try { operation(entries, null); } catch (error) { rejected = String(error).includes("ArgumentNullException"); }
                    if (!rejected) throw new Error("Nullable numeric selector terminal must reject a null selector");
                  }
                  let rejected = false;
                  try { sumInt(null, entry => entry.int32); } catch (error) { rejected = String(error).includes("ArgumentNullException"); }
                  if (!rejected) throw new Error("Nullable numeric selector terminal must reject a null source");
                  rejected = false;
                  try { sumInt([{ value: 2147483647 }, { value: 1 }], entry => entry.value); } catch (error) { rejected = String(error).includes("OverflowException"); }
                  if (!rejected) throw new Error("Nullable Int32 selector Sum must check overflow");
                  rejected = false;
                  try { averageInt64([{ value: 9223372036854775807n }, { value: 9223372036854775807n }], entry => entry.value); } catch (error) { rejected = String(error).includes("OverflowException"); }
                  if (!rejected) throw new Error("Nullable Int64 selector Average must check overflow");
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
