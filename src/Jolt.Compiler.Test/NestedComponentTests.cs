using System.Text.Json;

namespace Jolt.Compiler.Test;

[TestClass]
public sealed class NestedComponentTests
{
    [TestMethod]
    public async Task Compile_DirectoryNamespacesResolveSameNamesAndKeepIndependentArtifacts()
    {
        JoltSource[] sources =
        [
            new("App.jazor", "@using App.First\n<Badge />"),
            new("First/Badge.jazor", "<p>First</p>"),
            new("Second/Badge.jazor", "<p>Second</p>"),
            new("Second/Page.jazor", "<Badge />"),
            new("Third/Page.jazor", "@using App.Second\n<Badge />"),
        ];
        var result = await JoltProjectCompiler.CompileAsync(sources);
        Assert.AreEqual(0, result.Diagnostics.Count, string.Join(Environment.NewLine, result.Diagnostics));
        var modules = result.Modules.ToDictionary(module => module.Path);
        StringAssert.Contains(modules["App.mjs"].Content, "from \"./First/Badge.mjs\"");
        StringAssert.Contains(modules["Second/Page.mjs"].Content, "from \"./Badge.mjs\"");
        StringAssert.Contains(modules["Third/Page.mjs"].Content, "from \"../Second/Badge.mjs\"");

        foreach (var folder in new[] { "First", "Second" })
        {
            var badge = modules[$"{folder}/Badge.mjs"];
            StringAssert.Contains(badge.Content, "sourceMappingURL=Badge.mjs.map");
            using var map = JsonDocument.Parse(badge.SourceMap);
            Assert.AreEqual($"{folder}/Badge.jazor", map.RootElement.GetProperty("sources")[0].GetString());
            Assert.AreEqual($"<p>{folder}</p>", map.RootElement.GetProperty("sourcesContent")[0].GetString());
        }

        sources[1] = sources[1] with { Content = "<p>Edited</p>" };
        var rebuilt = await JoltProjectCompiler.CompileAsync(sources);
        Assert.AreEqual(0, rebuilt.Diagnostics.Count, string.Join(Environment.NewLine, rebuilt.Diagnostics));
        CollectionAssert.AreEqual(new[] { "First/Badge.mjs" },
            rebuilt.Modules.Where(module => module != modules[module.Path]).Select(module => module.Path).ToArray());
    }

    [TestMethod]
    public async Task Compile_ReportsAmbiguousComponentWhenBothNamespacesAreImported()
    {
        var result = await JoltProjectCompiler.CompileAsync([
            new("App.jazor", "@using App.First\n@using App.Second\n<Badge />"),
            new("First/Badge.jazor", "<p>First</p>"),
            new("Second/Badge.jazor", "<p>Second</p>"),
        ]);
        Assert.IsTrue(result.Diagnostics.Any(diagnostic => diagnostic.Id == "JOLT1002"));
        Assert.AreEqual(0, result.Modules.Count);
    }
}
