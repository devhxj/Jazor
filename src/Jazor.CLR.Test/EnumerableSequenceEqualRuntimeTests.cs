using System.Text;
using DenoHost.Core;

namespace Jazor.CLR.Test;

[TestClass]
public sealed class EnumerableSequenceEqualRuntimeTests
{
    private const string EnumerableModulePath = "System/Linq/EnumerableModule.js";

    [TestMethod]
    public async Task SequenceEqualExport_PreservesSynchronousEqualityAndShortCircuitOrderOnDenoHost()
    {
        var sequenceEqual = GetExportName("static System.Linq.Enumerable.SequenceEqual<TSource>(System.Collections.Generic.IEnumerable<TSource>, System.Collections.Generic.IEnumerable<TSource>)");
        var root = Path.Combine(Path.GetTempPath(), "jazor-enumerable-sequence-equal-" + Guid.NewGuid().ToString("N"));
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
            var testPath = Path.Combine(root, "sequence-equal.test.mjs");
            await File.WriteAllTextAsync(
                testPath,
                $$"""
                import { {{sequenceEqual}} } from "./System/Linq/EnumerableModule.js";

                Deno.test("SequenceEqual compares aligned values and stops at the first mismatch", () => {
                  if (!{{sequenceEqual}}([Number.NaN, -0], [Number.NaN, 0]))
                    throw new Error("SequenceEqual did not use the default equality contract");

                  const trace = [];
                  const traceReads = (name, values) => new Proxy(values, {
                    get(target, property, receiver) {
                      if (/^\d+$/.test(String(property)))
                        trace.push(`${name}:${property}`);
                      return Reflect.get(target, property, receiver);
                    }
                  });
                  const first = traceReads("first", [3, 1, 4]);
                  const second = traceReads("second", [3, 2, 4]);
                  if ({{sequenceEqual}}(first, second))
                    throw new Error("SequenceEqual accepted a mismatched sequence");
                  if (trace.join(",") !== "first:0,second:0,first:1,second:1")
                    throw new Error(`SequenceEqual did not stop at the first mismatch: ${trace.join(",")}`);
                  if (first.join(",") !== "3,1,4" || second.join(",") !== "3,2,4")
                    throw new Error("SequenceEqual mutated an input sequence");
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
