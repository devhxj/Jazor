using System.Text;
using DenoHost.Core;

namespace Jazor.CLR.Test;

[TestClass]
public sealed class EnumerableJoinRuntimeTests
{
    private const string EnumerableModulePath = "System/Linq/EnumerableModule.js";

    [TestMethod]
    public async Task JoinExports_PreserveLookupAndSelectorEvaluationOrderOnDenoHost()
    {
        var join = GetExportName(
            "static System.Linq.Enumerable.Join<TOuter, TInner, TKey, TResult>(System.Collections.Generic.IEnumerable<TOuter>, System.Collections.Generic.IEnumerable<TInner>, System.Func<TOuter, TKey>, System.Func<TInner, TKey>, System.Func<TOuter, TInner, TResult>)");
        var groupJoin = GetExportName(
            "static System.Linq.Enumerable.GroupJoin<TOuter, TInner, TKey, TResult>(System.Collections.Generic.IEnumerable<TOuter>, System.Collections.Generic.IEnumerable<TInner>, System.Func<TOuter, TKey>, System.Func<TInner, TKey>, System.Func<TOuter, System.Collections.Generic.IEnumerable<TInner>, TResult>)");
        var root = Path.Combine(Path.GetTempPath(), "jazor-enumerable-join-" + Guid.NewGuid().ToString("N"));
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
            var testPath = Path.Combine(root, "join.test.mjs");
            await File.WriteAllTextAsync(
                testPath,
                $$"""
                import { {{join}}, {{groupJoin}} } from "./System/Linq/EnumerableModule.js";

                Deno.test("Join and GroupJoin preserve lookup, result, and numeric comparer semantics", () => {
                  const trace = [];
                  const joined = {{join}}([1, 2, 3], [10, 11, 12], outer => {
                    trace.push(`outer:${outer}`);
                    return outer % 2;
                  }, inner => {
                    trace.push(`inner:${inner}`);
                    return inner % 2;
                  }, (outer, inner) => {
                    trace.push(`result:${outer}:${inner}`);
                    return outer * 100 + inner;
                  });
                  if (JSON.stringify(joined) !== "[111,210,212,311]")
                    throw new Error(`unexpected join result: ${JSON.stringify(joined)}`);
                  if (trace.join(",") !== "inner:10,inner:11,inner:12,outer:1,result:1:11,outer:2,result:2:10,result:2:12,outer:3,result:3:11")
                    throw new Error(`unexpected join selector order: ${trace.join(",")}`);

                  const grouped = {{groupJoin}}([1, 2, 3], [10, 11, 12], outer => outer % 2, inner => inner % 2,
                    (outer, matches) => [outer, Array.from(matches)]);
                  if (JSON.stringify(grouped) !== "[[1,[11]],[2,[10,12]],[3,[11]]]")
                    throw new Error(`unexpected group join result: ${JSON.stringify(grouped)}`);

                  const numeric = {{join}}([Number.NaN, -0], [Number.NaN, 0], value => value, value => value,
                    (outer, inner) => [outer, inner]);
                  if (numeric.length !== 2)
                    throw new Error(`unexpected numeric join result: ${JSON.stringify(numeric)}`);
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
