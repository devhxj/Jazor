namespace Jazor.Style.Tests;

[TestClass]
public sealed class JazorStyleCatalogTests
{
    [TestMethod]
    public void Build_RuntimeModule_EmitsAssemblyCatalog()
    {
        var module = JazorStyleModuleTestHost.GetRuntimeModule();
        Assert.AreEqual("Jazor.Style/runtime.mjs", module.RelativePath);
        Assert.AreEqual("Jazor.Style.Css", module.TypeName);

        var content = module.Content;
        StringAssert.Contains(content, "export");
        StringAssert.Contains(content, "export function css(");
        StringAssert.Contains(content, "export function keyframes(");
        StringAssert.Contains(content, "export function global(");
        StringAssert.Contains(content, "export function extract(");
        StringAssert.Contains(content, "export function configure(");
    }
}
