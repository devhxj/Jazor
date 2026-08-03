using System.Text;
using DenoHost.Core;

namespace Jazor.CLR.Test;

[TestClass]
public sealed class EnumerableNumericSelectorRuntimeTests
{
    private const string ModulePath = "System/Linq/EnumerableModule.js";

    [TestMethod]
    public async Task NumericSelectorExports_PreserveCarrierOrderAndFailureContractsOnDenoHost()
    {
        var names = new[]
        {
            GetExportName("static System.Linq.Enumerable.Sum<TSource>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, int>)"),
            GetExportName("static System.Linq.Enumerable.Sum<TSource>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, long>)"),
            GetExportName("static System.Linq.Enumerable.Sum<TSource>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, float>)"),
            GetExportName("static System.Linq.Enumerable.Sum<TSource>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, double>)"),
            GetExportName("static System.Linq.Enumerable.Sum<TSource>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, decimal>)"),
            GetExportName("static System.Linq.Enumerable.Average<TSource>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, int>)"),
            GetExportName("static System.Linq.Enumerable.Average<TSource>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, long>)"),
            GetExportName("static System.Linq.Enumerable.Average<TSource>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, float>)"),
            GetExportName("static System.Linq.Enumerable.Average<TSource>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, double>)"),
            GetExportName("static System.Linq.Enumerable.Average<TSource>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, decimal>)"),
            GetExportName("static System.Linq.Enumerable.Min<TSource>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, int>)"),
            GetExportName("static System.Linq.Enumerable.Min<TSource>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, long>)"),
            GetExportName("static System.Linq.Enumerable.Min<TSource>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, float>)"),
            GetExportName("static System.Linq.Enumerable.Min<TSource>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, double>)"),
            GetExportName("static System.Linq.Enumerable.Min<TSource>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, decimal>)"),
            GetExportName("static System.Linq.Enumerable.Max<TSource>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, int>)"),
            GetExportName("static System.Linq.Enumerable.Max<TSource>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, long>)"),
            GetExportName("static System.Linq.Enumerable.Max<TSource>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, float>)"),
            GetExportName("static System.Linq.Enumerable.Max<TSource>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, double>)"),
            GetExportName("static System.Linq.Enumerable.Max<TSource>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, decimal>)")
        };
        var root = Path.Combine(Path.GetTempPath(), "jazor-enumerable-numeric-selector-" + Guid.NewGuid().ToString("N"));
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
            var testPath = Path.Combine(root, "numeric-selector.test.mjs");
            await File.WriteAllTextAsync(
                testPath,
                $$"""
                import { {{string.Join(", ", names)}} } from "./System/Linq/EnumerableModule.js";

                Deno.test("Enumerable numeric selectors preserve carrier and source order", () => {
                  const [sumIntByOperation, sumInt64ByOperation, sumSingleByOperation, sumDoubleByOperation, sumDecimalByOperation, averageIntByOperation, averageInt64ByOperation, averageSingleByOperation, averageDoubleByOperation, averageDecimalByOperation, minIntByOperation, minInt64ByOperation, minSingleByOperation, minDoubleByOperation, minDecimalByOperation, maxIntByOperation, maxInt64ByOperation, maxSingleByOperation, maxDoubleByOperation, maxDecimalByOperation] = [{{string.Join(", ", names)}}];
                  const entries = [{ value: 1 }, { value: 2 }];
                  const sumTrace = [];
                  const sumInt = sumIntByOperation(entries, entry => {
                    sumTrace.push(entry.value);
                    return entry.value * 2;
                  });
                  if (sumInt !== 6 || sumTrace.join(",") !== "1,2")
                    throw new Error("Int32 Sum selector must run once per source item in order");
                  if (sumInt64ByOperation(entries, entry => BigInt(entry.value)) !== 3n)
                    throw new Error("Int64 Sum selector must preserve BigInt carrier");
                  if (sumSingleByOperation(entries, entry => entry.value / 10) !== Math.fround(0.1 + 0.2))
                    throw new Error("Single Sum selector precision drifted");
                  if (sumDoubleByOperation(entries, entry => entry.value / 10) !== 0.1 + 0.2)
                    throw new Error("Double Sum selector precision drifted");

                  const decimals = [{ value: "1.25" }, { value: "2.50" }];
                  if (sumDecimalByOperation(decimals, entry => entry.value) !== "3.75")
                    throw new Error("Decimal Sum selector must preserve decimal addition");

                  const averageTrace = [];
                  const averageInt = averageIntByOperation(entries, entry => {
                    averageTrace.push(entry.value);
                    return entry.value * 2;
                  });
                  if (averageInt !== 3 || averageTrace.join(",") !== "1,2")
                    throw new Error("Int32 Average selector must run once per source item in order");
                  if (averageInt64ByOperation(entries, entry => BigInt(entry.value)) !== 1.5)
                    throw new Error("Int64 Average selector carrier drifted");
                  if (averageSingleByOperation(entries, entry => entry.value / 10) !== Math.fround((0.1 + 0.2) / 2))
                    throw new Error("Single Average selector precision drifted");
                  if (averageDoubleByOperation(entries, entry => entry.value / 10) !== (0.1 + 0.2) / 2)
                    throw new Error("Double Average selector precision drifted");
                  if (averageDecimalByOperation(decimals, entry => entry.value) !== "1.875")
                    throw new Error("Decimal Average selector must preserve decimal division");

                  const minTrace = [];
                  const minInt = minIntByOperation(entries, entry => {
                    minTrace.push(entry.value);
                    return entry.value * 2;
                  });
                  if (minInt !== 2 || minTrace.join(",") !== "1,2")
                    throw new Error("Int32 Min selector must run once per source item in order");
                  if (minInt64ByOperation(entries, entry => BigInt(entry.value)) !== 1n)
                    throw new Error("Int64 Min selector must preserve BigInt carrier");
                  if (minSingleByOperation(entries, entry => entry.value / 10) !== 0.1)
                    throw new Error("Single Min selector must return the selected carrier");
                  if (minDoubleByOperation(entries, entry => entry.value / 10) !== 0.1)
                    throw new Error("Double Min selector precision drifted");
                  if (!Number.isNaN(minDoubleByOperation([{ value: 1 }, { value: NaN }], entry => entry.value)))
                    throw new Error("Double Min selector must preserve NaN propagation");
                  if (minDecimalByOperation([{ value: "10" }, { value: "2" }], entry => entry.value) !== "2")
                    throw new Error("Decimal Min selector must use numeric comparison");

                  if (maxIntByOperation(entries, entry => entry.value * 2) !== 4)
                    throw new Error("Int32 Max selector result drifted");
                  if (maxInt64ByOperation(entries, entry => BigInt(entry.value)) !== 2n)
                    throw new Error("Int64 Max selector must preserve BigInt carrier");
                  if (maxSingleByOperation(entries, entry => entry.value / 10) !== 0.2)
                    throw new Error("Single Max selector must return the selected carrier");
                  if (maxDoubleByOperation(entries, entry => entry.value / 10) !== 0.2)
                    throw new Error("Double Max selector precision drifted");
                  if (maxDoubleByOperation([{ value: NaN }, { value: 1 }], entry => entry.value) !== 1)
                    throw new Error("Double Max selector must skip NaN when a numeric value exists");
                  if (maxDecimalByOperation([{ value: "10" }, { value: "2" }], entry => entry.value) !== "10")
                    throw new Error("Decimal Max selector must use numeric comparison");

                  for (const [operation, source, selector] of [
                    [sumIntByOperation, null, entry => entry.value],
                    [sumIntByOperation, entries, null],
                    [averageIntByOperation, null, entry => entry.value],
                    [averageIntByOperation, entries, null],
                    [minIntByOperation, null, entry => entry.value],
                    [minIntByOperation, entries, null],
                    [maxIntByOperation, null, entry => entry.value],
                    [maxIntByOperation, entries, null]
                  ]) {
                    let rejected = false;
                    try { operation(source, selector); } catch (error) { rejected = String(error).includes("ArgumentNullException"); }
                    if (!rejected) throw new Error("Numeric selector terminals must reject null source and selector");
                  }

                  if (entries.map(entry => entry.value).join(",") !== "1,2")
                    throw new Error("Numeric selector terminals must not mutate their source");
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
