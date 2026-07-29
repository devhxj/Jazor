using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Jazor.Emit;

namespace Jazor.EmitTest;

[TestClass]
public sealed class EcmaScriptStyleCatalogReaderTests
{
    [TestMethod]
    public void CatalogReader_TryRead_ReadsEcmaScriptStyleRuntimeWithSourceMap()
    {
        var assembly = typeof(global::ECMAScript.Style.css).Assembly;

        var modules = CatalogReader.TryRead(assembly);

        Assert.IsNotNull(modules);
        var module = modules.Single();
        Assert.AreEqual("ECMAScript.Style", module.AssemblyName);
        Assert.AreEqual("ECMAScript.Style.css", module.TypeName);
        Assert.AreEqual("style.mjs", module.RelativePath);
        StringAssert.Contains(module.Content, " as style };");
        StringAssert.Contains(module.Content, " as context };");
        StringAssert.Contains(module.Content, "export function styleIn(");
        StringAssert.Contains(module.Content, "export function atRuleIn(");
        StringAssert.Contains(module.Content, "export function snapshotFrom(");
        Assert.HasCount(64, module.Hash);
        Assert.AreEqual("style.mjs.map", module.SourceMapRelativePath);
        Assert.HasCount(64, module.MapHash!);
        Assert.AreEqual(ComputeHash(module.Content), module.Hash);
        Assert.AreEqual(ComputeHash(module.SourceMapContent!), module.MapHash);

        using var sourceMap = JsonDocument.Parse(module.SourceMapContent!);
        Assert.AreEqual(3, sourceMap.RootElement.GetProperty("version").GetInt32());
        Assert.AreEqual("style.mjs", sourceMap.RootElement.GetProperty("file").GetString());
    }

    [TestMethod]
    public void ModuleCollector_Collect_ReadsEcmaScriptStyleRuntimeFromReferencedAssembly()
    {
        var assemblyPath = typeof(global::ECMAScript.Style.css).Assembly.Location;
        var loadContext = new EmitLoadContext(assemblyPath);
        var collector = new ModuleCollector(loadContext);
        collector.AddAssembly(assemblyPath);

        var result = collector.Collect(failOnPathConflict: true);

        Assert.IsTrue(result.IsSuccess, result.Error ?? string.Empty);
        Assert.AreEqual(1, result.AssemblyCount);
        Assert.AreEqual(1, result.CatalogCount);
        var module = result.Modules.Single();
        Assert.AreEqual("style.mjs", module.RelativePath);
        Assert.IsNotNull(module.SourceMapContent);
    }

    private static string ComputeHash(string content)
        => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(content)));
}
