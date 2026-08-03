using System.Text;
using DenoHost.Core;

namespace Jazor.CLR.Test;

[TestClass]
public sealed class EnumerableTakeRangeRuntimeTests
{
    [TestMethod]
    public async Task TakeRangeExport_PreservesRangeBoundariesAndMaterializedSourceContractOnDenoHost()
    {
        var takeRange = GetExportName(
            "static System.Linq.Enumerable.Take<TSource>(System.Collections.Generic.IEnumerable<TSource>, System.Range)",
            "System/Linq/EnumerableModule.js");
        var fromStart = GetExportName("static System.Index.FromStart(int)", "System/IndexModule.js");
        var fromEnd = GetExportName("static System.Index.FromEnd(int)", "System/IndexModule.js");
        var createRange = GetExportName("System.Range.Range(System.Index, System.Index)", "System/RangeModule.js");
        var allRange = GetExportName("static System.Range.All.get", "System/RangeModule.js");
        var root = Path.Combine(Path.GetTempPath(), "jazor-enumerable-take-range-" + Guid.NewGuid().ToString("N"));
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
            var testPath = Path.Combine(root, "take-range.test.mjs");
            await File.WriteAllTextAsync(
                testPath,
                $$"""
                import { {{takeRange}} } from "./System/Linq/EnumerableModule.js";
                import { {{fromStart}}, {{fromEnd}} } from "./System/IndexModule.js";
                import { {{createRange}}, {{allRange}} } from "./System/RangeModule.js";

                Deno.test("Enumerable.Take(Range) preserves closed Range semantics", () => {
                  const source = [1, 2, 3, 4, 5];
                  const middle = {{takeRange}}(source, {{createRange}}({{fromStart}}(1), {{fromEnd}}(1)));
                  if (JSON.stringify(middle) !== "[2,3,4]")
                    throw new Error(`unexpected middle range: ${JSON.stringify(middle)}`);
                  if (JSON.stringify({{takeRange}}(source, {{createRange}}({{fromStart}}(0), {{fromStart}}(2)))) !== "[1,2]")
                    throw new Error("from-start prefix range drifted");
                  if (JSON.stringify({{takeRange}}(source, {{createRange}}({{fromEnd}}(2), {{fromEnd}}(0)))) !== "[4,5]")
                    throw new Error("from-end suffix range drifted");
                  if (JSON.stringify({{takeRange}}(source, {{allRange}}())) !== "[1,2,3,4,5]")
                    throw new Error("Range.All must retain every source item");

                  middle[0] = 99;
                  if (JSON.stringify(source) !== "[1,2,3,4,5]")
                    throw new Error("Take(Range) must return a new materialized Array");
                  let rejected = false;
                  try { {{takeRange}}(source, {{createRange}}({{fromEnd}}(1), {{fromStart}}(1))); } catch (error) { rejected = String(error).includes("ArgumentOutOfRangeException"); }
                  if (!rejected) throw new Error("inverted Range must fail through GetOffsetAndLength");
                  rejected = false;
                  try { {{takeRange}}(null, {{allRange}}()); } catch (error) { rejected = String(error).includes("ArgumentNullException"); }
                  if (!rejected) throw new Error("null source must fail before range projection");
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

    private static string GetExportName(string member, string modulePath)
    {
        var mapping = ClrRuntimeMappingCatalog.GetImport(member);
        Assert.AreEqual(modulePath, mapping.ModulePath, member);
        return mapping.ExportName;
    }
}
