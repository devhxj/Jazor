namespace Jazor.Style.Tests;

[TestClass]
public sealed class JazorStyleCatalogTests
{
    [TestMethod]
    public void Build_RuntimeModule_EmitsAssemblyCatalog()
    {
        var module = JazorStyleModuleTestHost.GetRuntimeModule();
        Assert.AreEqual("jazorStyle.mjs", module.RelativePath);
        Assert.AreEqual("Jazor.Style.css", module.TypeName);

        var content = module.Content;
        StringAssert.Contains(content, "export");
        StringAssert.Contains(content, " as style };");
        StringAssert.Contains(content, "export function keyframes(");
        StringAssert.Contains(content, "export function global(");
        StringAssert.Contains(content, "export function extract(");
        StringAssert.Contains(content, "export function configure(");
    }
}
