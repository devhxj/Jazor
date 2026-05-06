using Jazor.Emit;

namespace Jazor.EmitTest;

[TestClass]
public sealed class ClrRuntimeCatalogReaderTests
{
    [TestMethod]
    public void CatalogReader_TryRead_UsesECMAScriptDedicatedCatalogTypeName()
    {
        var catalogType = typeof(ECMAScript.Number).Assembly.GetType("ECMAScript.Catalog", throwOnError: false, ignoreCase: false);

        Assert.IsNotNull(catalogType);
        Assert.IsNull(typeof(ECMAScript.Number).Assembly.GetType("Jazor.Generated.ModuleCatalog", throwOnError: false, ignoreCase: false));
    }

    [TestMethod]
    public void CatalogReader_TryRead_ReadsClrRuntimeModules_FromEcmascriptAssembly()
    {
        var assembly = typeof(ECMAScript.Number).Assembly;

        var modules = CatalogReader.TryRead(assembly);

        Assert.IsNotNull(modules);
        Assert.IsTrue(modules.Count >= 30, $"Expected at least 30 CLR runtime modules, but found {modules.Count}.");

        AssertContainsModule(modules, "System/RuntimeModule.js");
        AssertContainsModule(modules, "System/StringModule.js");
        AssertContainsModule(modules, "System/DecimalModule.js");
        AssertContainsModule(modules, "System/Globalization/CultureInfoModule.js");
    }

    [TestMethod]
    public void ModuleCollector_Collect_ReadsClrRuntimeModules_FromEcmascriptAssemblyCatalog()
    {
        var assemblyPath = typeof(ECMAScript.Number).Assembly.Location;
        var loadContext = new EmitLoadContext(assemblyPath);
        var collector = new ModuleCollector(loadContext);
        collector.AddAssembly(assemblyPath);

        var result = collector.Collect(failOnPathConflict: true);

        Assert.IsTrue(result.IsSuccess, result.Error ?? string.Empty);
        Assert.AreEqual(1, result.AssemblyCount);
        Assert.AreEqual(1, result.CatalogCount);
        Assert.AreEqual(0, result.RazorVueCatalogCount);

        AssertContainsModule(result.Modules, "System/RuntimeModule.js");
        AssertContainsModule(result.Modules, "System/StringModule.js");
        AssertContainsModule(result.Modules, "System/DecimalModule.js");
        AssertContainsModule(result.Modules, "System/Globalization/CultureInfoModule.js");
    }

    [TestMethod]
    public void CatalogReader_TryRead_ExportsClrImportMembers_AndModuleNamespaceObjects()
    {
        var assembly = typeof(ECMAScript.Number).Assembly;

        var modules = CatalogReader.TryRead(assembly);

        Assert.IsNotNull(modules);

        var stringModule = modules.Single(module => string.Equals(module.RelativePath, "System/StringModule.js", StringComparison.OrdinalIgnoreCase));
        StringAssert.Contains(stringModule.Content, "export function _5ad63706a889c294");
        StringAssert.Contains(stringModule.Content, "export const StringModule = {");

        var runtimeModule = modules.Single(module => string.Equals(module.RelativePath, "System/RuntimeModule.js", StringComparison.OrdinalIgnoreCase));
        StringAssert.Contains(runtimeModule.Content, "export const RuntimeModule = {");
        Assert.IsFalse(runtimeModule.Content.Contains("from \"System/RuntimeModule.js\"", StringComparison.Ordinal), runtimeModule.Content);
        Assert.IsFalse(runtimeModule.Content.Contains("import {", StringComparison.Ordinal), runtimeModule.Content);
        StringAssert.Contains(runtimeModule.Content, "this.items = materializeArray(collection, ");
        StringAssert.Contains(runtimeModule.Content, "return new JQueue;");

        var byteModule = modules.Single(module => string.Equals(module.RelativePath, "System/ByteModule.js", StringComparison.OrdinalIgnoreCase));
        StringAssert.Contains(byteModule.Content, "export function _8719e4b3055c5188");
        StringAssert.Contains(byteModule.Content, "export const ByteModule = {");

        var comparerModule = modules.Single(module => string.Equals(module.RelativePath, "System/Collections/Generic/ComparerT1Module.js", StringComparison.OrdinalIgnoreCase));
        StringAssert.Contains(comparerModule.Content, "export const ComparerT1Module = {");
        StringAssert.Contains(comparerModule.Content, "ensureComparerInstance,");
        StringAssert.Contains(comparerModule.Content, "compareCore,");
    }

    [TestMethod]
    public void CatalogReader_TryRead_StringIndexerGetter_UsesCharAt_WithoutSelfRecursiveImport()
    {
        var assembly = typeof(ECMAScript.Number).Assembly;

        var modules = CatalogReader.TryRead(assembly);

        Assert.IsNotNull(modules);

        var stringModule = modules.Single(module => string.Equals(module.RelativePath, "System/StringModule.js", StringComparison.OrdinalIgnoreCase));
        StringAssert.Contains(stringModule.Content, "return instance.charAt(index);");
        Assert.IsFalse(stringModule.Content.Contains("return String.fromCharCode(i$8578349aab59a79b(instance, index));", StringComparison.Ordinal));
    }

    [TestMethod]
    public void CatalogReader_TryRead_ClrBigIntStatics_InlineToNativeLiterals()
    {
        var assembly = typeof(ECMAScript.Number).Assembly;

        var modules = CatalogReader.TryRead(assembly);

        Assert.IsNotNull(modules);

        var allContent = string.Join("\n", modules.Select(static module => module.Content));
        Assert.IsFalse(allContent.Contains("BigInt.zero", StringComparison.Ordinal), "CLR runtime catalog still contains invalid BigInt.zero access.");
        Assert.IsFalse(allContent.Contains("BigInt.one", StringComparison.Ordinal), "CLR runtime catalog still contains invalid BigInt.one access.");
        Assert.IsFalse(allContent.Contains("BigInt.minusOne", StringComparison.Ordinal), "CLR runtime catalog still contains invalid BigInt.minusOne access.");

        var dateTimeModule = modules.Single(module => string.Equals(module.RelativePath, "System/DateTimeModule.js", StringComparison.OrdinalIgnoreCase));
        StringAssert.Contains(dateTimeModule.Content, "function get_ZeroTicks() {\n  return 0n;\n}");
    }

    private static void AssertContainsModule(IReadOnlyList<EmitModuleRecord> modules, string relativePath)
    {
        var module = modules.SingleOrDefault(module => string.Equals(module.RelativePath, relativePath, StringComparison.OrdinalIgnoreCase));

        Assert.IsNotNull(module, $"Expected CLR runtime catalog to contain '{relativePath}'.");
        Assert.AreEqual("ECMAScript", module.AssemblyName);
        Assert.IsFalse(string.IsNullOrWhiteSpace(module.TypeName), $"Expected '{relativePath}' to have a type name.");
        Assert.IsFalse(string.IsNullOrWhiteSpace(module.Content), $"Expected '{relativePath}' to have emitted module content.");
        Assert.IsFalse(string.IsNullOrWhiteSpace(module.Hash), $"Expected '{relativePath}' to have a content hash.");
    }
}
