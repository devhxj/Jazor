using System.Reflection;
using ECMAScript;
using ECMAScript.ElementPlus;
using ECMAScript.Vuetify;

namespace Jazor.CompilerTest;

[TestClass]
public sealed class ExactPairUnionContractTests
{
    [TestMethod]
    public void ComponentPairValues_RejectDirectArraysWithUnexpectedLength()
    {
        Assert.Throws<ArgumentException>(() => new ElTransferTextPair(["left"]));
        Assert.Throws<ArgumentException>(() => new VuetifyOverlayCoordinateTarget(new Number[1]));

        var transfer = new ElTransferTextPair(["left", "right"]);
        var target = new VuetifyOverlayCoordinateTarget(new Number[2]);

        Assert.AreEqual("left", transfer.First);
        Assert.AreEqual("right", transfer.Second);
        Assert.AreEqual(2, target.AsArray!.Length);
        Assert.IsNotNull(typeof(ElTransferTextPair).GetCustomAttribute<System.Runtime.CompilerServices.UnionAttribute>());
        Assert.IsNotNull(typeof(VuetifyOverlayCoordinateTarget).GetCustomAttribute<System.Runtime.CompilerServices.UnionAttribute>());
        Assert.IsTrue(typeof(System.Runtime.CompilerServices.IUnion).IsAssignableFrom(typeof(ElTransferTextPair)));
        Assert.IsTrue(typeof(System.Runtime.CompilerServices.IUnion).IsAssignableFrom(typeof(VuetifyOverlayCoordinateTarget)));
    }
}
