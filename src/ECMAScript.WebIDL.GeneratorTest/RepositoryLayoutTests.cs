using ECMAScript.WebIDL.Generator;

namespace ECMAScript.WebIDL.GeneratorTest;

[TestClass]
public sealed class RepositoryLayoutTests
{
    [TestMethod]
    public void Discover_RepositoryMarkerExistsUpTree_ReturnsRepositoryLayout()
    {
        foreach (var solutionFileName in new[] { "Jazor.slnx", "Jazor.sln" })
        {
            var tempDirectory = Directory.CreateTempSubdirectory("webidl-layout-test-");
            try
            {
                var repositoryRoot = Directory.CreateDirectory(Path.Combine(tempDirectory.FullName, "repo"));
                File.WriteAllText(Path.Combine(repositoryRoot.FullName, solutionFileName), "<Solution />");

                var baseDirectory = Directory.CreateDirectory(Path.Combine(
                    repositoryRoot.FullName,
                    "src",
                    "ECMAScript.WebIDL.Generator",
                    "bin",
                    "Debug",
                    "net10.0"));

                var layout = RepositoryLayout.Discover(baseDirectory.FullName);

                Assert.AreEqual(repositoryRoot.FullName, layout.RepositoryRoot);
                Assert.AreEqual(
                    Path.Combine(repositoryRoot.FullName, "src", "ECMAScript.WebIDL.Generator", "deno", "collect.ts"),
                    layout.DefaultWorkerPath);
                Assert.AreEqual(
                    Path.Combine(repositoryRoot.FullName, "src", "ECMAScript.WebIDL.Generator", "deno.json"),
                    layout.DefaultDenoConfigPath);
                Assert.AreEqual(
                    Path.Combine(repositoryRoot.FullName, "src", "ECMAScript", "webidl"),
                    layout.DefaultOutputDirectory);
            }
            finally
            {
                tempDirectory.Delete(recursive: true);
            }
        }
    }
}
