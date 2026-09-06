using System.Security.Cryptography;
using System.Text.Json;
using Jazor.RazorVue.RazorSdk;

namespace Jazor.RazorVue.Sg.Test;

/// <summary>
    /// Verifies raw-markup HTML facts and the Vue runtime JS-resource package contract.
    /// 覆盖 hydration cardinality 与 manifest/dist ABI，避免重新引入程序集 provider carrier。
/// </summary>
[TestClass]
public sealed class VueRawMarkupTests
{
    [TestMethod]
    [DataRow("", 0, false)]
    [DataRow("plain", 1, true)]
    [DataRow("<strong>one</strong>", 1, true)]
    [DataRow("<strong>one</strong><em>two</em>", 2, true)]
    [DataRow("text<span>two</span>", 2, true)]
    [DataRow("<!--lead--><span>two</span>", 2, false)]
    [DataRow("<template><span>nested</span></template><b>end</b>", 2, true)]
    [DataRow("<table><tr><td>cell</td></tr></table>", 1, true)]
    [DataRow("<svg><circle /></svg><math><mi>x</mi></math>", 2, true)]
    public void AnalyzeStatic_UsesHtmlFragmentCardinality(
        string markup,
        int expectedCount,
        bool expectedStaticHydration)
    {
        var result = VueRawMarkup.AnalyzeStatic(markup);

        Assert.AreEqual(expectedCount, result.NodeCount);
        Assert.AreEqual(expectedStaticHydration, result.CanHydrateAsStaticVNode);
    }

    [TestMethod]
    public void JsResourceManifest_ExposesVueRuntimeModulesAndDependencies()
    {
        var manifestPath = FindRepositoryFile("src", "Jazor.Vue", "manifest.json");
        var packageRoot = Path.GetDirectoryName(manifestPath)!;
        using var document = JsonDocument.Parse(File.ReadAllText(manifestPath));
        var root = document.RootElement;
        Assert.AreEqual(2, root.GetProperty("schemaVersion").GetInt32());
        Assert.AreEqual("jazor-vue-runtime", root.GetProperty("libraryId").GetString());

        var imports = root.GetProperty("imports");
        Assert.HasCount(4, imports.EnumerateObject());
        foreach (var name in new[]
                 {
                     "@jazor/vue-runtime/raw-markup.mjs",
                     "@jazor/vue-runtime/cascading.mjs",
                     "@jazor/vue-runtime/blazor-routing.mjs",
                     "@jazor/vue-runtime/authentication.mjs"
                 })
        {
            var entry = imports.GetProperty(name);
            Assert.AreEqual("module", entry.GetProperty("type").GetString());
            var production = entry.GetProperty("production").GetString()!;
            var sourcePath = Path.Combine(packageRoot, production.Replace('/', Path.DirectorySeparatorChar));
            Assert.IsTrue(File.Exists(sourcePath), sourcePath);
            var actualHash = Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(sourcePath))).ToLowerInvariant();
            Assert.AreEqual(entry.GetProperty("productionHash").GetString(), actualHash);
        }

        var routingPath = Path.Combine(
            packageRoot,
            imports.GetProperty("@jazor/vue-runtime/blazor-routing.mjs")
                .GetProperty("production").GetString()!
                .Replace('/', Path.DirectorySeparatorChar));
        var routing = File.ReadAllText(routingPath);
        StringAssert.Contains(routing, "@jazor/vue-runtime/routes.mjs");
        StringAssert.Contains(routing, "Microsoft/AspNetCore/Components/NavigationManagerModule.js");
        CollectionAssert.DoesNotContain(
            imports.GetProperty("@jazor/vue-runtime/blazor-routing.mjs")
                .GetProperty("productionDependencies")
                .EnumerateArray()
                .Select(static value => value.GetString())
                .ToArray(),
            "@jazor/vue-runtime/routes.mjs");

        CollectionAssert.Contains(
            root.GetProperty("requires").EnumerateObject().Select(static property => property.Name).ToArray(),
            "ecmascript");
        Assert.IsFalse(
            typeof(VueRawMarkup).Assembly.GetManifestResourceNames()
                .Any(static name => name.Contains("render-context", StringComparison.Ordinal)),
            "The retired render-context runtime must not return as a resource package asset.");
    }

    private static string FindRepositoryFile(params string[] segments)
    {
        for (var directory = new DirectoryInfo(AppContext.BaseDirectory); directory is not null; directory = directory.Parent)
        {
            var candidate = Path.Combine([directory.FullName, ..segments]);
            if (File.Exists(candidate))
                return candidate;
        }

        throw new FileNotFoundException($"Could not locate repository file '{Path.Combine(segments)}'.");
    }
}
