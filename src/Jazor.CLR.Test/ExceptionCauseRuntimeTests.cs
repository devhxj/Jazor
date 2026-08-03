using System.Text;
using DenoHost.Core;

namespace Jazor.CLR.Test;

[TestClass]
public sealed class ExceptionCauseRuntimeTests
{
    private const string ModulePath = "System/ExceptionModule.js";

    [TestMethod]
    public async Task CauseExports_PreserveInnerIdentityAndRootTraversalOnDenoHost()
    {
        var create = GetExportName("System.Exception.Exception(string, System.Exception)");
        var getInner = GetExportName("System.Exception.InnerException.get");
        var getBase = GetExportName("virtual System.Exception.GetBaseException()");
        var getHelpLink = GetExportName("virtual System.Exception.HelpLink.get");
        var setHelpLink = GetExportName("virtual System.Exception.HelpLink.set");
        var root = Path.Combine(Path.GetTempPath(), "jazor-exception-cause-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);

        try
        {
            foreach (var module in ClrRuntimeCatalog.All)
            {
                var outputPath = Path.Combine(root, module.RelativePath.Replace('/', Path.DirectorySeparatorChar));
                Directory.CreateDirectory(Path.GetDirectoryName(outputPath)!);
                await File.WriteAllTextAsync(
                    outputPath,
                    module.Content,
                    new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
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
            var testPath = Path.Combine(root, "exception-cause.test.mjs");
            await File.WriteAllTextAsync(
                testPath,
                $$"""
                import { {{create}}, {{getInner}}, {{getBase}}, {{getHelpLink}}, {{setHelpLink}} } from "./System/ExceptionModule.js";

                Deno.test("Exception cause chain preserves CLR-facing identity", () => {
                  const leaf = new Error("leaf");
                  const middle = {{create}}("middle", leaf);
                  const outer = {{create}}("outer", middle);

                  if (outer.message !== "outer" || outer.cause !== middle)
                    throw new Error("Exception constructor did not preserve message/cause");
                  if ({{getInner}}(outer) !== middle || {{getInner}}(leaf) !== null)
                    throw new Error("InnerException did not preserve nullable cause identity");
                  if ({{getBase}}(outer) !== leaf || {{getBase}}(leaf) !== leaf)
                    throw new Error("GetBaseException did not traverse to the root cause");

                  const withoutInner = {{create}}("plain", null);
                  if ({{getInner}}(withoutInner) !== null || {{getBase}}(withoutInner) !== withoutInner)
                    throw new Error("Null inner exception changed the root exception");

                  const metadata = new Error("metadata");
                  if ({{getHelpLink}}(metadata) !== null)
                    throw new Error("HelpLink must default to null");
                  {{setHelpLink}}(metadata, "https://example.test/help");
                  if ({{getHelpLink}}(metadata) !== "https://example.test/help")
                    throw new Error("HelpLink assignment was not retained by its exception");
                  {{setHelpLink}}(metadata, null);
                  if ({{getHelpLink}}(metadata) !== null)
                    throw new Error("HelpLink must retain an explicit null assignment");

                  for (const operation of [{{getInner}}, {{getBase}}, {{getHelpLink}}]) {
                    let rejected = false;
                    try {
                      operation(null);
                    } catch (error) {
                      rejected = String(error).includes("NullReferenceException");
                    }
                    if (!rejected)
                      throw new Error("Exception instance member must reject a null receiver");
                  }

                  let setterRejected = false;
                  try {
                    {{setHelpLink}}(null, "https://example.test/help");
                  } catch (error) {
                    setterRejected = String(error).includes("NullReferenceException");
                  }
                  if (!setterRejected)
                    throw new Error("Exception HelpLink setter must reject a null receiver");
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
        Assert.AreEqual(ModulePath, mapping.ModulePath, member);
        return mapping.ExportName;
    }
}
