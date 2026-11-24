using Jazor.Npm;
using Microsoft.CodeAnalysis.CSharp;

namespace Jolt.Compiler.Test;

[TestClass]
public sealed class NpmWorkspaceTests
{
    [TestMethod]
    public void Select_FollowsSemanticUseAndPrefersTheConsumerManifest()
    {
        var parent = Path.Combine(Path.GetTempPath(), "workspace-selection");
        var root = Path.Combine(parent, "consumer");
        var project = Path.Combine(root, "Consumer.csproj");
        var client = Path.Combine(root, "Client");
        var other = Path.Combine(root, "Other");
        var linked = Path.Combine(parent, "Linked");
        var options = new CSharpParseOptions(LanguageVersion.Preview);
        var sources = new[]
        {
            CSharpSyntaxTree.ParseText("global using Tool = Npm.Sample.Exports;", options, Path.Combine(root, "Imports.cs")),
            CSharpSyntaxTree.ParseText("class Page { void Render() => Tool.Show(); }", options, Path.Combine(client, "Page.cs")),
            CSharpSyntaxTree.ParseText("class Unused {}", options, Path.Combine(other, "Unused.cs")),
            CSharpSyntaxTree.ParseText("class Shared { void Render() => Tool.Show(); }", options, Path.Combine(linked, "Shared.cs")),
        };
        var requests = NpmSourceRequests.Discover(CSharpCompilation.Create("Test", sources), packages: null);
        var nestedManifest = Path.Combine(client, "package.json");
        var manifests = new HashSet<string>(NpmWorkspace.PathComparer)
        {
            nestedManifest, Path.Combine(other, "package.json"), Path.Combine(linked, "package.json"),
        };

        var selection = NpmWorkspace.Select(project, requests, manifests.Contains);
        Assert.AreEqual(nestedManifest, selection.Manifest);
        Assert.IsEmpty(selection.Conflicts);

        var parentManifest = Path.Combine(parent, "package.json");
        manifests.Add(parentManifest);
        Assert.AreEqual(parentManifest, NpmWorkspace.Select(project, requests, manifests.Contains).Manifest);
    }

    [TestMethod]
    public void Select_ReportsTheNearestWorkspaceAndSourceForEachConflict()
    {
        var root = Path.Combine(Path.GetTempPath(), "workspace-conflict");
        var project = Path.Combine(root, "Consumer.csproj");
        var client = Path.Combine(root, "Client");
        var nested = Path.Combine(client, "Nested");
        var other = Path.Combine(root, "Other");
        var first = CSharpSyntaxTree.ParseText("using static Npm.Sample.Exports; class Page { void Render() => Show(); }",
            path: Path.Combine(nested, "Page.cs"));
        var second = CSharpSyntaxTree.ParseText("class Other { void Render() => Npm.Sample.Exports.Show(); }",
            path: Path.Combine(other, "Other.cs"));
        var requests = NpmSourceRequests.Discover(CSharpCompilation.Create("Test", [first, second]), packages: null);
        var nestedManifest = Path.Combine(nested, "package.json");
        var otherManifest = Path.Combine(other, "package.json");
        var manifests = new HashSet<string>(NpmWorkspace.PathComparer)
        {
            Path.Combine(client, "package.json"), nestedManifest, otherManifest,
        };

        var selection = NpmWorkspace.Select(project, requests, manifests.Contains);
        Assert.IsNull(selection.Manifest);
        CollectionAssert.AreEquivalent(new[] { (nestedManifest, first.FilePath), (otherManifest, second.FilePath) },
            selection.Conflicts.Select(conflict => (conflict.Manifest, conflict.Site.Location.SourceTree!.FilePath)).ToArray());
    }
}
