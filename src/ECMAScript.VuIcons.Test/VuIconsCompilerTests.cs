namespace ECMAScript.VuIcons.Test;

[TestClass]
public sealed class VuIconsCompilerTests
{
    [TestMethod]
    public async Task Convert_IconNameEnum_LowersExactUpstreamTokens()
    {
        var script = await VuIconsTestCompiler.ConvertModuleAsync(
            """
            using ECMAScript;
            using ECMAScript.VuIcons;

            namespace Demo
            {
                [ECMAScriptModule("icons/names.mjs")]
                public static class IconNames
                {
                    public static VuIconName User() => VuIconName.User;
                    public static VuIconName NumberedArrow() => VuIconName.ArrowDown01;
                    public static VuIconName ThreeDimensionalAxis() => VuIconName.Axis3d;
                }
            }
            """,
            "IconNames");

        Assert.IsNotNull(script);
        StringAssert.Contains(script, "return \"user\";", StringComparison.Ordinal);
        StringAssert.Contains(script, "return \"arrow-down-0-1\";", StringComparison.Ordinal);
        StringAssert.Contains(script, "return \"axis-3d\";", StringComparison.Ordinal);
    }
}
