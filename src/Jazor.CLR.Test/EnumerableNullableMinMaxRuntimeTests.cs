using System.Text;
using DenoHost.Core;

namespace Jazor.CLR.Test;

[TestClass]
public sealed class EnumerableNullableMinMaxRuntimeTests
{
    private const string ModulePath = "System/Linq/EnumerableModule.js";

    [TestMethod]
    public async Task NullableMinMaxExports_PreserveCarrierNaNAndAllNullContractsOnDenoHost()
    {
        var names = new[]
        {
            GetExportName("static System.Linq.Enumerable.Min(System.Collections.Generic.IEnumerable<int?>)"),
            GetExportName("static System.Linq.Enumerable.Max(System.Collections.Generic.IEnumerable<int?>)"),
            GetExportName("static System.Linq.Enumerable.Min(System.Collections.Generic.IEnumerable<long?>)"),
            GetExportName("static System.Linq.Enumerable.Max(System.Collections.Generic.IEnumerable<long?>)"),
            GetExportName("static System.Linq.Enumerable.Min(System.Collections.Generic.IEnumerable<float?>)"),
            GetExportName("static System.Linq.Enumerable.Max(System.Collections.Generic.IEnumerable<float?>)"),
            GetExportName("static System.Linq.Enumerable.Min(System.Collections.Generic.IEnumerable<double?>)"),
            GetExportName("static System.Linq.Enumerable.Max(System.Collections.Generic.IEnumerable<double?>)"),
            GetExportName("static System.Linq.Enumerable.Min(System.Collections.Generic.IEnumerable<decimal?>)"),
            GetExportName("static System.Linq.Enumerable.Max(System.Collections.Generic.IEnumerable<decimal?>)")
        };
        var root = Path.Combine(Path.GetTempPath(), "jazor-enumerable-nullable-min-max-" + Guid.NewGuid().ToString("N"));
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
            var testPath = Path.Combine(root, "nullable-min-max.test.mjs");
            await File.WriteAllTextAsync(
                testPath,
                $$"""
                import { {{string.Join(", ", names)}} } from "./System/Linq/EnumerableModule.js";

                Deno.test("Enumerable nullable Min/Max preserves CLR numeric contracts", () => {
                  const [minInt, maxInt, minInt64, maxInt64, minSingle, maxSingle, minDouble, maxDouble, minDecimal, maxDecimal] = [{{string.Join(", ", names)}}];
                  const integerSource = [null, 7, -2, 4];
                  if (minInt(integerSource) !== -2 || maxInt(integerSource) !== 7 || minInt([null, null]) !== null || maxInt([null, null]) !== null)
                    throw new Error("Nullable Int32 Min/Max must ignore null and return null for all-null input");
                  if (JSON.stringify(integerSource) !== "[null,7,-2,4]")
                    throw new Error("Nullable Min/Max must not mutate their source");
                  if (minInt64([null, 7n, -2n, 4n]) !== -2n || maxInt64([null, 7n, -2n, 4n]) !== 7n || minInt64([null, null]) !== null || maxInt64([null, null]) !== null)
                    throw new Error("Nullable Int64 Min/Max lost BigInt or all-null contracts");
                  if (!Number.isNaN(minSingle([null, 1, NaN])) || maxSingle([null, NaN, 1]) !== 1 || !Number.isNaN(maxSingle([null, NaN, NaN])))
                    throw new Error("Nullable Single Min/Max NaN behavior drifted");
                  if (!Number.isNaN(minDouble([null, 1, NaN])) || maxDouble([null, NaN, 1]) !== 1 || minDouble([null, null]) !== null)
                    throw new Error("Nullable Double Min/Max NaN or all-null behavior drifted");
                  if (minDecimal(["10", null, "2"]) !== "2" || maxDecimal(["10", null, "2"]) !== "10" || minDecimal([null, null]) !== null || maxDecimal([null, null]) !== null)
                    throw new Error("Nullable Decimal Min/Max must use decimal ordering and nullable results");

                  for (const operation of [minInt, maxInt, minInt64, maxInt64, minSingle, maxSingle, minDouble, maxDouble, minDecimal, maxDecimal]) {
                    let rejected = false;
                    try { operation(null); } catch (error) { rejected = String(error).includes("ArgumentNullException"); }
                    if (!rejected) throw new Error("Nullable Min/Max must reject a null source");
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
