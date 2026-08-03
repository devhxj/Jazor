using System.Text;
using DenoHost.Core;

namespace Jazor.CLR.Test;

[TestClass]
public sealed class EnumerableLookupRuntimeTests
{
    [TestMethod]
    public async Task ToLookupExports_PreserveGroupingAndLookupContractsOnDenoHost()
    {
        var toLookup = GetExportName("static System.Linq.Enumerable.ToLookup<TSource, TKey>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, TKey>)");
        var toLookupElement = GetExportName("static System.Linq.Enumerable.ToLookup<TSource, TKey, TElement>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, TKey>, System.Func<TSource, TElement>)");
        var lookupCount = GetExportName("System.Linq.ILookup<TKey, TElement>.Count.get");
        var lookupContains = GetExportName("System.Linq.ILookup<TKey, TElement>.Contains(TKey)");
        var lookupGet = GetExportName("System.Linq.ILookup<TKey, TElement>.this[TKey].get");
        var groupingKey = ClrRuntimeMappingCatalog.GetImport("System.Linq.IGrouping<TKey, TElement>.Key.get").ExportName;
        var root = Path.Combine(Path.GetTempPath(), "jazor-enumerable-lookup-" + Guid.NewGuid().ToString("N"));
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
            var testPath = Path.Combine(root, "lookup.test.mjs");
            await File.WriteAllTextAsync(
                testPath,
                $$"""
                import { {{toLookup}}, {{toLookupElement}}, {{lookupCount}}, {{lookupContains}}, {{lookupGet}} } from "./System/Linq/EnumerableModule.js";
                import { {{groupingKey}} } from "./System/Linq/GroupingT2Module.js";

                Deno.test("Enumerable.ToLookup preserves CLR grouping and lookup behavior", () => {
                  const source = [1, 2, 3, 4];
                  const trace = [];
                  const lookup = {{toLookup}}(source, value => {
                    trace.push(value);
                    return value % 2;
                  });
                  if (trace.join(",") !== "1,2,3,4" || {{lookupCount}}(lookup) !== 2)
                    throw new Error("ToLookup key selector or group count drifted");
                  if (JSON.stringify({{lookupGet}}(lookup, 1)) !== "[1,3]" || JSON.stringify({{lookupGet}}(lookup, 0)) !== "[2,4]")
                    throw new Error("ToLookup indexer did not retain group members");
                  if (!{{lookupContains}}(lookup, 1) || {{lookupContains}}(lookup, 3) || {{lookupGet}}(lookup, 3).length !== 0)
                    throw new Error("ToLookup Contains/indexer missing-key behavior drifted");
                  if ({{groupingKey}}({{lookupGet}}(lookup, 1)) !== 1)
                    throw new Error("ToLookup must preserve grouping key metadata");

                  const projected = {{toLookupElement}}(source, value => value % 2, value => value * 10);
                  if (JSON.stringify({{lookupGet}}(projected, 1)) !== "[10,30]" || JSON.stringify({{lookupGet}}(projected, 0)) !== "[20,40]")
                    throw new Error("ToLookup element selector projection drifted");

                  const numeric = {{toLookup}}([Number.NaN, -0, 0, Number.NaN], value => value);
                  if ({{lookupCount}}(numeric) !== 2 || !{{lookupContains}}(numeric, Number.NaN) || !{{lookupContains}}(numeric, 0))
                    throw new Error("ToLookup must reuse CLR NaN and signed-zero equality");
                  if ({{lookupGet}}(numeric, Number.NaN).length !== 2 || {{lookupGet}}(numeric, 0).length !== 2)
                    throw new Error("ToLookup numeric groups are incomplete");

                  let rejected = false;
                  try { {{toLookup}}(null, value => value); } catch (error) { rejected = String(error).includes("ArgumentNullException"); }
                  if (!rejected) throw new Error("ToLookup must reject null source");
                  rejected = false;
                  try { {{toLookup}}(source, null); } catch (error) { rejected = String(error).includes("ArgumentNullException"); }
                  if (!rejected) throw new Error("ToLookup must reject null key selector");
                  rejected = false;
                  try { {{lookupCount}}(null); } catch (error) { rejected = String(error).includes("NullReferenceException"); }
                  if (!rejected) throw new Error("ILookup members must reject null instance");
                  if (JSON.stringify(source) !== "[1,2,3,4]")
                    throw new Error("ToLookup must not mutate source");
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
        Assert.AreEqual("System/Linq/EnumerableModule.js", mapping.ModulePath, member);
        return mapping.ExportName;
    }
}
