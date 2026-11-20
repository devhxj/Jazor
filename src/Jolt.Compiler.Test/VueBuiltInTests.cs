namespace Jolt.Compiler.Test;

[TestClass]
public sealed class VueBuiltInTests
{
    [TestMethod]
    public async Task Compile_ImportsBuiltInsWithTheirVueChildrenProtocols()
    {
        var result = await JoltProjectCompiler.CompileAsync([new("App.jazor", """
            <Teleport To="#overlays" Disabled="@Disabled">
                <p>Moved</p>
            </Teleport>
            <Transition Name="fade" Mode="out-in">
                <template #default>
                    <p>Animated</p>
                </template>
            </Transition>
            @code {
                private bool Disabled;
            }
            """)]);
        Assert.AreEqual(0, result.Diagnostics.Count, string.Join(Environment.NewLine, result.Diagnostics));
        var js = result.Modules.Single().Content;
        StringAssert.Contains(js, "Teleport as $joltComponent0");
        StringAssert.Contains(js, "Transition as $joltComponent1");
        StringAssert.Contains(js, "$joltH($joltComponent0,");
        StringAssert.Contains(js, "$joltH($joltComponent1,");
        StringAssert.Contains(js, "\"#overlays\"");
        StringAssert.Contains(js, "\"out-in\"");
        StringAssert.Contains(js, "{ \"default\": () =>");
    }

    [TestMethod]
    public async Task Compile_BindsExistingHostTypesForVueBuiltInProps()
    {
        var result = await JoltProjectCompiler.CompileAsync([new("App.jazor", """
            @using ECMAScript
            <Teleport To="@Target"><span>Moved</span></Teleport>
            <Transition Mode="@Mode"><span>Animated</span></Transition>
            @code {
                private Vue.VueTeleportTarget Target = "#overlays";
                private Vue.VueTransitionMode Mode = Vue.VueTransitionMode.InOut;
            }
            """)]);
        Assert.AreEqual(0, result.Diagnostics.Count, string.Join(Environment.NewLine, result.Diagnostics));
    }

    [TestMethod]
    [DataRow("<Teleport />", "JOLT1005")]
    [DataRow("<Teleport To=\"@(42)\" />", "CS1503")]
    [DataRow("<Teleport To=\"#overlays\" Disabled=\"yes\" />", "CS1503")]
    [DataRow("<Teleport To=\"#overlays\"><template #default><p /></template></Teleport>", "JOLT1007")]
    [DataRow("<Transition Mode=\"together\"><p /></Transition>", "JOLT1006")]
    [DataRow("<Transition><p /><div /></Transition>", "JOLT1039")]
    public async Task Compile_ReportsInvalidVueBuiltInUsage(string source, string diagnostic)
    {
        var result = await JoltProjectCompiler.CompileAsync([new("App.jazor", source)]);
        Assert.IsTrue(result.Diagnostics.Any(item => item.Id == diagnostic), string.Join(Environment.NewLine, result.Diagnostics));
        Assert.AreEqual(0, result.Modules.Count);
    }

    [TestMethod]
    public async Task Compile_VisibleLocalComponentTakesPrecedenceOverBuiltIn()
    {
        var result = await JoltProjectCompiler.CompileAsync([
            new("App.jazor", "<Teleport />"),
            new("Teleport.jazor", "<p>Local</p>"),
        ]);
        Assert.AreEqual(0, result.Diagnostics.Count, string.Join(Environment.NewLine, result.Diagnostics));
        StringAssert.Contains(result.Modules.Single(module => module.Path == "App.mjs").Content, "from \"./Teleport.mjs\"");
    }
}
