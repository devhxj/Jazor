using System.Text;
using DenoHost.Core;

namespace Jazor.CLR.Test;

[TestClass]
public sealed class EnumerableSelectManyRuntimeTests
{
    private const string EnumerableModulePath = "System/Linq/EnumerableModule.js";

    [TestMethod]
    public async Task SelectManyExports_PreserveSelectorEvaluationOrderOnDenoHost()
    {
        var collectionSelector = GetExportName(
            "static System.Linq.Enumerable.SelectMany<TSource, TResult>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, System.Collections.Generic.IEnumerable<TResult>>)");
        var indexedCollectionSelector = GetExportName(
            "static System.Linq.Enumerable.SelectMany<TSource, TResult>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, int, System.Collections.Generic.IEnumerable<TResult>>)");
        var resultSelector = GetExportName(
            "static System.Linq.Enumerable.SelectMany<TSource, TCollection, TResult>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, System.Collections.Generic.IEnumerable<TCollection>>, System.Func<TSource, TCollection, TResult>)");
        var root = Path.Combine(Path.GetTempPath(), "jazor-enumerable-select-many-" + Guid.NewGuid().ToString("N"));
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
            var testPath = Path.Combine(root, "select-many.test.mjs");
            await File.WriteAllTextAsync(
                testPath,
                $$"""
                import { {{collectionSelector}}, {{indexedCollectionSelector}}, {{resultSelector}} } from "./System/Linq/EnumerableModule.js";

                Deno.test("SelectMany preserves outer/inner and selector evaluation order", () => {
                  const trace = [];
                  const flattened = {{collectionSelector}}([2, 3], outer => {
                    trace.push(`collection:${outer}`);
                    return [outer, outer * 10];
                  });
                  if (JSON.stringify(flattened) !== "[2,20,3,30]")
                    throw new Error(`unexpected flattened values: ${JSON.stringify(flattened)}`);

                  const indexed = {{indexedCollectionSelector}}([10, 20], (outer, index) => {
                    trace.push(`indexed:${outer}:${index}`);
                    return [outer + index];
                  });
                  if (JSON.stringify(indexed) !== "[10,21]")
                    throw new Error(`unexpected indexed values: ${JSON.stringify(indexed)}`);

                  const projected = {{resultSelector}}([1, 2], outer => {
                    trace.push(`nested:${outer}`);
                    return [outer, outer * 10];
                  }, (outer, inner) => {
                    trace.push(`result:${outer}:${inner}`);
                    return outer * 100 + inner;
                  });
                  if (JSON.stringify(projected) !== "[101,110,202,220]")
                    throw new Error(`unexpected projected values: ${JSON.stringify(projected)}`);

                  const expectedTrace = "collection:2,collection:3,indexed:10:0,indexed:20:1,nested:1,result:1:1,result:1:10,nested:2,result:2:2,result:2:20";
                  if (trace.join(",") !== expectedTrace)
                    throw new Error(`unexpected selector order: ${trace.join(",")}`);
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

    private static string GetExportName(string member)
    {
        var mapping = ClrRuntimeMappingCatalog.GetImport(member);
        Assert.AreEqual(EnumerableModulePath, mapping.ModulePath, member);
        return mapping.ExportName;
    }
}
