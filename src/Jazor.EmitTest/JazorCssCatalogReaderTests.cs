using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Jazor.Emit;

namespace Jazor.EmitTest;

[TestClass]
public sealed class JazorCssCatalogReaderTests
{
    [TestMethod]
    public void CatalogReader_TryRead_ReadsJazorCssRuntimeWithSourceMap()
    {
        var assembly = typeof(global::Jazor.Css.Css).Assembly;

        var modules = CatalogReader.TryRead(assembly);

        Assert.IsNotNull(modules);
        var module = modules.Single();
        Assert.AreEqual("Jazor.Css", module.AssemblyName);
        Assert.AreEqual("Jazor.Css.Css", module.TypeName);
        Assert.AreEqual("Jazor.Css/runtime.mjs", module.RelativePath);
        StringAssert.Contains(module.Content, "export function css(");
        StringAssert.Contains(module.Content, "export function createContext(");
        StringAssert.Contains(module.Content, "export function classIn(");
        StringAssert.Contains(module.Content, "export function atRuleIn(");
        StringAssert.Contains(module.Content, "export function snapshotFrom(");
        Assert.HasCount(64, module.Hash);
        Assert.AreEqual("Jazor.Css/runtime.mjs.map", module.SourceMapRelativePath);
        Assert.HasCount(64, module.MapHash!);
        Assert.AreEqual(ComputeHash(module.Content), module.Hash);
        Assert.AreEqual(ComputeHash(module.SourceMapContent!), module.MapHash);

        using var sourceMap = JsonDocument.Parse(module.SourceMapContent!);
        Assert.AreEqual(3, sourceMap.RootElement.GetProperty("version").GetInt32());
        Assert.AreEqual("Jazor.Css/runtime.mjs", sourceMap.RootElement.GetProperty("file").GetString());
    }

    [TestMethod]
    public void ModuleCollector_Collect_ReadsJazorCssRuntimeFromReferencedAssembly()
    {
        var assemblyPath = typeof(global::Jazor.Css.Css).Assembly.Location;
        var loadContext = new EmitLoadContext(assemblyPath);
        var collector = new ModuleCollector(loadContext);
        collector.AddAssembly(assemblyPath);

        var result = collector.Collect(failOnPathConflict: true);

        Assert.IsTrue(result.IsSuccess, result.Error ?? string.Empty);
        Assert.AreEqual(1, result.AssemblyCount);
        Assert.AreEqual(1, result.CatalogCount);
        var module = result.Modules.Single();
        Assert.AreEqual("Jazor.Css/runtime.mjs", module.RelativePath);
        Assert.IsNotNull(module.SourceMapContent);
    }

    private static string ComputeHash(string content)
        => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(content)));
}
