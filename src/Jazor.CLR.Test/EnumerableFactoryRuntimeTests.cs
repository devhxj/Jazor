using System.Text;
using DenoHost.Core;

namespace Jazor.CLR.Test;

[TestClass]
public sealed class EnumerableFactoryRuntimeTests
{
    private const string EnumerableModulePath = "System/Linq/EnumerableModule.js";

    [TestMethod]
    public async Task FactoryExports_PreserveRangeRepeatAndSourceIdentityOnDenoHost()
    {
        var empty = GetExportName("static System.Linq.Enumerable.Empty<TResult>()");
        var range = GetExportName("static System.Linq.Enumerable.Range(int, int)");
        var repeat = GetExportName("static System.Linq.Enumerable.Repeat<TResult>(TResult, int)");
        var asEnumerable = GetExportName("static System.Linq.Enumerable.AsEnumerable<TSource>(System.Collections.Generic.IEnumerable<TSource>)");
        var sequence = GetExportName("static System.Linq.Enumerable.Sequence<T>(T, T, T)");
        var root = Path.Combine(Path.GetTempPath(), "jazor-enumerable-factory-" + Guid.NewGuid().ToString("N"));
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
            var testPath = Path.Combine(root, "factory.test.mjs");
            await File.WriteAllTextAsync(
                testPath,
                $$"""
                import { {{empty}}, {{range}}, {{repeat}}, {{asEnumerable}}, {{sequence}} } from "./System/Linq/EnumerableModule.js";

                function assertArray(actual, expected, name) {
                  if (actual.length !== expected.length || actual.some((value, index) => value !== expected[index]))
                    throw new Error(`${name}: ${JSON.stringify(actual)} did not match ${JSON.stringify(expected)}`);
                }

                Deno.test("Enumerable factories preserve materialized values and source identity", () => {
                  const emptyValues = {{empty}}();
                  assertArray(emptyValues, [], "Empty");
                  if (emptyValues === {{empty}}())
                    throw new Error("Empty must not expose a mutable shared Array carrier");

                  assertArray({{range}}(-2, 3), [-2, -1, 0], "Range negative start");
                  assertArray({{range}}(2147483647, 0), [], "Range zero count at Int32.MaxValue");
                  let threw = false;
                  try {
                    {{range}}(2147483647, 2);
                  } catch (error) {
                    threw = String(error).includes("ArgumentOutOfRangeException");
                  }
                  if (!threw)
                    throw new Error("Range must reject values beyond Int32.MaxValue");

                  const marker = { id: 7 };
                  const repeated = {{repeat}}(marker, 3);
                  if (repeated.length !== 3 || repeated.some(value => value !== marker))
                    throw new Error("Repeat must preserve the element reference in every slot");
                  if ({{repeat}}(marker, 0).length !== 0)
                    throw new Error("Repeat(0) must return an empty Array");

                  const source = [2, 7, 9];
                  if ({{asEnumerable}}(source) !== source)
                    throw new Error("AsEnumerable must return the original enumerable carrier");
                  if ({{asEnumerable}}(null) !== null)
                    throw new Error("AsEnumerable must preserve a null input without adding a guard");

                  const first = { id: 1 };
                  const second = { id: 2 };
                  const third = { id: 3 };
                  const sequenceValues = {{sequence}}(first, second, third);
                  if (sequenceValues.length !== 3 || sequenceValues[0] !== first || sequenceValues[1] !== second || sequenceValues[2] !== third)
                    throw new Error("Sequence must preserve source argument order and references");
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
