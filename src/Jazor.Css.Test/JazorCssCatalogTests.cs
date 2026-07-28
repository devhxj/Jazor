namespace Jazor.Css.Tests;

[TestClass]
public sealed class JazorCssCatalogTests
{
    [TestMethod]
    public void Build_RuntimeModule_EmitsAssemblyCatalog()
    {
        var module = JazorCssModuleTestHost.GetRuntimeModule();
        Assert.AreEqual("Jazor.Css/runtime.mjs", module.RelativePath);
        Assert.AreEqual("Jazor.Css.Css", module.TypeName);

        var content = module.Content;
        StringAssert.Contains(content, "export");
        StringAssert.Contains(content, "export function css(");
        StringAssert.Contains(content, "export function keyframes(");
        StringAssert.Contains(content, "export function global(");
        StringAssert.Contains(content, "export function extract(");
        StringAssert.Contains(content, "export function configure(");
    }
}
