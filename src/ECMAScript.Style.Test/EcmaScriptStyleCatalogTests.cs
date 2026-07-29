namespace ECMAScript.Style.Tests;

[TestClass]
public sealed class EcmaScriptStyleCatalogTests
{
    [TestMethod]
    public void Build_RuntimeModule_EmitsAssemblyCatalog()
    {
        var module = EcmaScriptStyleModuleTestHost.GetRuntimeModule();
        Assert.AreEqual("style.mjs", module.RelativePath);
        Assert.AreEqual("ECMAScript.Style.css", module.TypeName);

        var content = module.Content;
        StringAssert.Contains(content, "export");
        StringAssert.Contains(content, " as style };");
        StringAssert.Contains(content, "export function keyframes(");
        StringAssert.Contains(content, "export function global(");
        StringAssert.Contains(content, "export function extract(");
        StringAssert.Contains(content, "export function configure(");
    }
}
