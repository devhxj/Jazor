using Jazor.RazorVue.Generation;

namespace Jazor.RazorVue.Sg.Test;

[TestClass]
public sealed class InitializeNativeHookSelfTest
{
    [TestMethod]
    public void TryValidateCurrentPlatform_ValidatesNativeInstallInvokeAndRestoreProtocol()
    {
        var supported = InitializeNativeHook.IsCurrentPlatformSupported(out var supportedFailure);
        var validated = InitializeNativeHook.TryValidateCurrentPlatform(out var validationFailure);

        Assert.AreEqual(supported, validated, validationFailure);
        if (supported)
        {
            Assert.IsTrue(validated, validationFailure);
            Assert.AreEqual(string.Empty, supportedFailure);
            Assert.AreEqual(string.Empty, validationFailure);
            return;
        }

        Assert.IsFalse(validated, validationFailure);
        StringAssert.Contains(validationFailure, supportedFailure, StringComparison.Ordinal);
    }
}
