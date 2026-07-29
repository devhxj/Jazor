namespace Jazor.CLR.Test;

[TestClass]
public sealed class ClrRuntimeCatalogTests
{
    public static IEnumerable<TestDataRow<string>> Modules
        => ClrRuntimeCatalog.All.Select(static module => new TestDataRow<string>(module.RelativePath)
        {
            DisplayName = module.RelativePath
        });

    [TestMethod]
    public void Catalog_HasUniqueModuleIdentityAndPaths()
    {
        var modules = ClrRuntimeCatalog.All;

        Assert.IsNotEmpty(modules);
        Assert.HasCount(modules.Count, modules.Select(static module => module.TypeName).Distinct(StringComparer.Ordinal));
        Assert.HasCount(modules.Count, modules.Select(static module => module.Id).Distinct(StringComparer.Ordinal));
        Assert.HasCount(modules.Count, modules.Select(static module => module.RelativePath).Distinct(StringComparer.Ordinal));
        Assert.IsTrue(modules.All(static module => module.AssemblyName == "ECMAScript"));
        Assert.IsTrue(modules.All(static module => module.Id == module.TypeName));
    }

    [TestMethod]
    [DynamicData(nameof(Modules))]
    public void CatalogModule_HashMatchesUtf8Content(string modulePath)
    {
        var module = ClrRuntimeCatalog.Get(modulePath);
        Assert.AreEqual(64, module.Hash.Length);
        Assert.AreEqual(module.ComputeHash(), module.Hash);
    }

    [TestMethod]
    [DynamicData(nameof(Modules))]
    public void CatalogModule_IsParseableEcmaScript(string modulePath)
    {
        var module = ClrRuntimeCatalog.Get(modulePath);
        var program = module.Parse();

        Assert.IsNotNull(program);
        Assert.IsNotEmpty(program.Body);
    }

    [TestMethod]
    [DynamicData(nameof(Modules))]
    public void CatalogModule_ImportsResolveWithinCatalog(string modulePath)
    {
        var module = ClrRuntimeCatalog.Get(modulePath);

        foreach (var importedPath in module.GetImportedModulePaths())
            Assert.AreEqual(importedPath, ClrRuntimeCatalog.Get(importedPath).RelativePath);
    }

    [TestMethod]
    [DynamicData(nameof(Modules))]
    public void CatalogModule_DoesNotImportItself(string modulePath)
    {
        var module = ClrRuntimeCatalog.Get(modulePath);

        Assert.DoesNotContain(modulePath, module.GetImportedModulePaths());
    }

    [TestMethod]
    [DynamicData(nameof(Modules))]
    public void CatalogModule_NamedImportsHavePublishedTargetBindings(string modulePath)
    {
        var module = ClrRuntimeCatalog.Get(modulePath);

        foreach (var import in module.GetNamedImports())
        {
            var target = ClrRuntimeCatalog.Get(import.ModulePath);
            Assert.Contains(
                import.ImportedName,
                target.GetExportedNames(),
                $"{modulePath} imports missing binding '{import.ImportedName}' from {import.ModulePath}.");
        }
    }
}
