using ECMAScript.WebIDL.Generator;

namespace ECMAScript.WebIDL.GeneratorTest;

[TestClass]
public sealed class GeneratorOptionsTests
{
    [TestMethod]
    public void Parse_ExplicitOfflineCollection_PreservesPathsAndProseChoice()
    {
        var layout = RepositoryLayout.Discover(AppContext.BaseDirectory);
        var options = GeneratorOptions.Parse(
            ["--node-project", layout.DefaultNodeProjectPath, "--skip-spec-prose", "--out", ".tmp/webidl-output"], layout);

        Assert.AreEqual(layout.DefaultNodeProjectPath, options.NodeProjectPath);
        Assert.IsTrue(options.SkipSpecificationProse);
        Assert.AreEqual(Path.GetFullPath(".tmp/webidl-output"), options.OutputDirectory);
        Assert.IsNull(options.InputInventoryPath);
    }

    [TestMethod]
    public void Parse_RetiredDenoConfig_FailsWithMigrationInstruction()
    {
        var layout = RepositoryLayout.Discover(AppContext.BaseDirectory);
        var exception = Assert.ThrowsExactly<ArgumentException>(() =>
            GeneratorOptions.Parse(["--deno-config", "deno.json"], layout));
        StringAssert.Contains(exception.Message, "--node-project");
    }
}
