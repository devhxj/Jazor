using Jazor.RazorVue.RazorSdk;

namespace Jazor.RazorVue.Sg.Test;

[TestClass]
public sealed class VueHmrMetadataTests
{
    [TestMethod]
    public void BoundaryKindWireValues_RemainStableAcrossCatalogConsumers()
    {
        Assert.AreEqual("unknown", VueHmrBoundaryKind.Unknown.ToWireValue());
        Assert.AreEqual("template-only", VueHmrBoundaryKind.TemplateOnly.ToWireValue());
        Assert.AreEqual("logic-safe", VueHmrBoundaryKind.LogicSafe.ToWireValue());
        Assert.AreEqual("full-reload-required", VueHmrBoundaryKind.FullReloadRequired.ToWireValue());
    }
}
