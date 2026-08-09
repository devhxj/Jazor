using Jazor.RazorVue.RazorSdk;

namespace Jazor.RazorVue.Sg.Test;

[TestClass]
public sealed class VueDescriptorNamingTests
{
    [TestMethod]
    public void ToListenerPropertyName_PreservesOnlyCanonicalListenerPrefixes()
    {
        Assert.AreEqual("onClick", VueDescriptorNaming.ToListenerPropertyName("click"));
        Assert.AreEqual("onClick", VueDescriptorNaming.ToListenerPropertyName("onClick"));
        Assert.AreEqual("onOn", VueDescriptorNaming.ToListenerPropertyName("on"));
        Assert.AreEqual("onOn1", VueDescriptorNaming.ToListenerPropertyName("on1"));
    }
}
