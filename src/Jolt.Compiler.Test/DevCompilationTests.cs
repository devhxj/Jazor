using Jazor.Npm;
using Jolt.Npm;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Jolt.Compiler.Test;

[TestClass]
public class DevCompilationTests
{
    [TestMethod]
    [DataRow(false)]
    [DataRow(true)]
    public void SourceMapReferencingChangedGeneratedSourcePreventsReuse(bool typeScriptOnly)
    {
        const string ownMap = """
            {"version":3,"sources":["Other.jazor"],"names":[],"mappings":""}
            """;
        const string crossMap = """
            {"version":3,"sources":["Other.jazor","../Pages/Main.jazor.g.cs"],"names":[],"mappings":""}
            """;
        var module = new JoltModule("Other", "Other.mjs", "", typeScriptOnly ? ownMap : crossMap)
            { TypeScriptSourceMap = typeScriptOnly ? crossMap : null };
        Assert.IsTrue(JoltDevCompilation.ReferencesSource(module, "Pages/Main.jazor"));
        Assert.IsFalse(JoltDevCompilation.ReferencesSource(module, "Pages/Missing.jazor"));
    }

    [TestMethod]
    public async Task RouterStaticEditsReuseModulesAndRouteContractChangesMatchFreshBuild()
    {
        var session = new JoltDevCompilation();
        var options = new JoltCompileOptions { Router = true };
        JoltSource[] sources = [
            new("Page.jazor", """
                @page "/items/{id:int}"
                @layout MainLayout
                <p>Page @Id</p>
                @code { [Parameter] public int Id { get; set; } }
                """),
            new("MainLayout.jazor", "<main><h1>Title</h1><slot /></main>"),
        ];
        var initial = await CompileAndCompare();
        Assert.IsNotNull(initial.RouterModule);
        Replace(0, "Page ", "New Page ");
        var pageEdit = await CompileAndCompare();
        Assert.AreSame(initial.Modules.Single(module => module.Name == "MainLayout"),
            pageEdit.Modules.Single(module => module.Name == "MainLayout"));
        Assert.AreEqual(initial.RouterModule, pageEdit.RouterModule);
        Replace(1, "Title", "Updated title");
        var layoutEdit = await CompileAndCompare();
        Assert.AreSame(pageEdit.Modules.Single(module => module.Name == "Page"),
            layoutEdit.Modules.Single(module => module.Name == "Page"));
        Assert.AreEqual(initial.RouterModule, layoutEdit.RouterModule);

        Replace(0, "/items/", "/products/");
        var routeEdit = await CompileAndCompare();
        Assert.IsNotNull(routeEdit.RouterModule);
        StringAssert.Contains(routeEdit.RouterModule, "/products/:id(");
        Replace(0, "int Id", "string Id");
        await CompileAndCompare(expectError: true);
        Replace(0, "string Id", "int Id");
        Replace(1, "<slot />", "");
        await CompileAndCompare(expectError: true);

        void Replace(int index, string before, string after) => sources[index] = sources[index] with
        {
            Content = sources[index].Content.Replace(before, after, StringComparison.Ordinal),
        };

        async Task<JoltCompileResult> CompileAndCompare(bool expectError = false)
        {
            var edited = await session.CompileAsync(sources, options);
            var fresh = await JoltProjectCompiler.CompileAsync(sources, options);
            if (expectError)
                Assert.IsTrue(edited.Diagnostics.Any(diagnostic => diagnostic.Id == "JOLT1043"), string.Join("\n", edited.Diagnostics));
            else
                Assert.IsEmpty(edited.Diagnostics, string.Join("\n", edited.Diagnostics));
            CollectionAssert.AreEqual(fresh.Diagnostics.Select(diagnostic => diagnostic.ToString()).ToArray(),
                edited.Diagnostics.Select(diagnostic => diagnostic.ToString()).ToArray());
            CollectionAssert.AreEqual(fresh.Modules.ToArray(), edited.Modules.ToArray());
            Assert.AreEqual(fresh.RouterModule, edited.RouterModule);
            return edited;
        }
    }

    [TestMethod]
    public async Task StaticEditWithChangedSharedCSharpRecompilesOtherModule()
    {
        var session = new JoltDevCompilation();
        JoltSource[] sources = [new("Main.jazor", "<p>Log in</p>"), new("Other.jazor", "<p>Other</p>")];
        var options = new JoltCompileOptions { CSharpSources = [new("Shared.cs", "internal static class Shared { public const int Value = 1; }")] };
        var initial = await session.CompileAsync(sources, options);
        sources[0] = sources[0] with { Content = "<p>Welcome</p>" };
        options = options with { CSharpSources = [options.CSharpSources[0] with { Content = "internal static class Shared { public const int Value = 2; }" }] };
        var edited = await session.CompileAsync(sources, options);
        Assert.AreNotSame(initial.Modules.Single(module => module.Name == "Other"), edited.Modules.Single(module => module.Name == "Other"));
    }

    [TestMethod]
    public async Task StaticComponentAttributeEditReusesUnchangedModule()
    {
        var session = new JoltDevCompilation();
        var options = new JoltCompileOptions();
        JoltSource[] original = [new("Main.jazor", "<Card Title=\"Log in\" />"),
            new("Card.jazor", "<h1>@Title</h1>\n@code { [Parameter] public string Title { get; set; } = \"\"; }")];
        JoltSource[] edited = [original[0] with { Content = "<Card Title=\"Sign in now\" />" }, original[1]];
        var initial = await session.CompileAsync(original, options);
        var originalCardTree = session.Snapshot!.Compilation.SyntaxTrees.Single(tree => tree.FilePath == "Card.jazor.g.cs");
        var incremental = await session.CompileAsync(edited, options);
        Assert.AreSame(originalCardTree, session.Snapshot!.Compilation.SyntaxTrees.Single(tree => tree.FilePath == "Card.jazor.g.cs"));
        var fresh = await JoltProjectCompiler.CompileAsync(edited, options);
        Assert.AreEqual(0, incremental.Diagnostics.Count, string.Join("\n", incremental.Diagnostics));
        Assert.AreSame(initial.Modules.Single(module => module.Name == "Card"), incremental.Modules.Single(module => module.Name == "Card"));
        Assert.AreNotSame(initial.Modules.Single(module => module.Name == "Main"), incremental.Modules.Single(module => module.Name == "Main"));
        for (var i = 0; i < fresh.Modules.Count; i++)
        {
            Assert.AreEqual(fresh.Modules[i].Content, incremental.Modules[i].Content);
            Assert.AreEqual(fresh.Modules[i].SourceMap, incremental.Modules[i].SourceMap);
            Assert.AreEqual(fresh.Modules[i].TypeScriptDeclaration, incremental.Modules[i].TypeScriptDeclaration);
            Assert.AreEqual(fresh.Modules[i].TypeScriptSourceMap, incremental.Modules[i].TypeScriptSourceMap);
        }

        var invalid = await session.CompileAsync([edited[0] with { Content = "<Card Missing=\"value\" />" }, edited[1]], options);
        Assert.IsEmpty(invalid.Modules);
        var repair = await session.CompileAsync([edited[0] with { Content = "<Card Title=\"Welcome\" />" }, edited[1]], options);
        Assert.AreSame(incremental.Modules.Single(module => module.Name == "Card"), repair.Modules.Single(module => module.Name == "Card"));
        var removed = await session.CompileAsync([edited[1]], options);
        Assert.HasCount(1, removed.Modules);
        Assert.AreNotSame(repair.Modules.Single(module => module.Name == "Card"), removed.Modules[0]);
    }

    [TestMethod]
    public async Task InvalidStaticPropValueReportsFreshDiagnosticAndKeepsBaseline()
    {
        var session = new JoltDevCompilation();
        var options = new JoltCompileOptions();
        JoltSource[] sources = [new("Main.jazor", "<Card Count=\"42\" />"),
            new("Card.jazor", "<p>@Count</p>\n@code { [Parameter] public int Count { get; set; } }")];
        var initial = await session.CompileAsync(sources, options);
        Assert.IsEmpty(initial.Diagnostics, string.Join("\n", initial.Diagnostics));
        var baseline = session.Snapshot;
        Assert.IsNotNull(baseline);
        sources[0] = sources[0] with { Content = "<Card Count=\"invalid\" />" };
        var rejected = await session.CompileAsync(sources, options);
        var fresh = await JoltProjectCompiler.CompileAsync(sources, options);
        Assert.IsEmpty(rejected.Modules);
        Assert.IsNotEmpty(fresh.Diagnostics);
        CollectionAssert.AreEqual(fresh.Diagnostics.Select(diagnostic => diagnostic.ToString()).ToArray(),
            rejected.Diagnostics.Select(diagnostic => diagnostic.ToString()).ToArray());
        Assert.AreSame(baseline, session.Snapshot);

        sources[0] = sources[0] with { Content = "<Card Count=\"43\" />" };
        var repaired = await session.CompileAsync(sources, options);
        Assert.IsEmpty(repaired.Diagnostics, string.Join("\n", repaired.Diagnostics));
        Assert.AreSame(initial.Modules.Single(module => module.Name == "Card"), repaired.Modules.Single(module => module.Name == "Card"));
    }

    [TestMethod]
    public async Task AlternatingStaticEditsReuseUnchangedTreesAndModules()
    {
        var session = new JoltDevCompilation();
        var options = new JoltCompileOptions();
        JoltSource[] sources = [
            new("Main.jazor", """
                <p>Main</p>
                @code {
                    [Parameter] public string Title { get; set; } = "";
                    [Parameter] public Details? Data { get; set; }
                    public record Details(string Label);
                }
                """),
            new("Other.jazor", "<Main Title=\"Other\" /><CountCard Count=\"42\" />"),
            new("CountCard.jazor", "<p>Items: @Count</p>\n@code { [Parameter] public int Count { get; set; } }"),
        ];
        var previous = await session.CompileAsync(sources, options);
        Assert.IsEmpty(previous.Diagnostics, string.Join("\n", previous.Diagnostics));
        StringAssert.Contains(previous.Modules.Single(module => module.Name == "Main").TypeScriptDeclaration, "\"Label\": string");

        await Edit(0, "Main</p>", "Main\nlonger text</p>");
        var baseline = session.Snapshot;
        var validOther = sources[1];
        sources[1] = validOther with { Content = validOther.Content.Replace("42", "invalid", StringComparison.Ordinal) };
        var rejected = await session.CompileAsync(sources, options);
        var freshRejected = await JoltProjectCompiler.CompileAsync(sources, options);
        Assert.IsEmpty(rejected.Modules);
        Assert.IsNotEmpty(freshRejected.Diagnostics);
        CollectionAssert.AreEqual(freshRejected.Diagnostics.Select(diagnostic => diagnostic.ToString()).ToArray(),
            rejected.Diagnostics.Select(diagnostic => diagnostic.ToString()).ToArray());
        Assert.AreSame(baseline, session.Snapshot);
        sources[1] = validOther;
        await Edit(1, "42", "43");
        await Edit(2, "Items:", "Total:");
        await Edit(0, "longer text", "shorter");
        await Edit(1, "Other", "Updated");

        async Task Edit(int index, string before, string after)
        {
            var oldCompilation = session.Snapshot!.Compilation;
            var path = sources[index].Path;
            sources[index] = sources[index] with { Content = sources[index].Content.Replace(before, after, StringComparison.Ordinal) };
            var edited = await session.CompileAsync(sources, options);
            var fresh = await JoltProjectCompiler.CompileAsync(sources, options);
            Assert.IsEmpty(edited.Diagnostics, string.Join("\n", edited.Diagnostics));
            CollectionAssert.AreEqual(fresh.Modules.ToArray(), edited.Modules.ToArray());
            var newCompilation = session.Snapshot!.Compilation;
            foreach (var source in sources)
            {
                var treePath = source.Path + ".g.cs";
                var oldTree = oldCompilation.SyntaxTrees.Single(tree => tree.FilePath == treePath);
                var newTree = newCompilation.SyntaxTrees.Single(tree => tree.FilePath == treePath);
                var name = Path.GetFileNameWithoutExtension(source.Path);
                var oldModule = previous.Modules.Single(module => module.Name == name);
                var newModule = edited.Modules.Single(module => module.Name == name);
                if (source.Path == path)
                {
                    Assert.AreNotSame(oldTree, newTree);
                    Assert.AreNotSame(oldModule, newModule);
                }
                else
                {
                    Assert.AreSame(oldTree, newTree);
                    Assert.AreSame(oldModule, newModule);
                }
            }
            previous = edited;
        }
    }

    [TestMethod]
    public async Task StaticEditWithScopedSlotUsesFullBinding()
    {
        var session = new JoltDevCompilation();
        var options = new JoltCompileOptions();
        JoltSource[] sources = [new("Main.jazor", """
            <Table><Row Context="row"><p>Item @row.value.Length</p></Row></Table>
            """), new("Table.jazor", """
            <slot name="row" value="@Label" />
            @code { private string Label = "text"; }
            """)];
        var initial = await session.CompileAsync(sources, options);
        Assert.IsEmpty(initial.Diagnostics, string.Join("\n", initial.Diagnostics));
        Assert.IsNull(session.Snapshot);

        sources[0] = sources[0] with { Content = sources[0].Content.Replace("Item ", "Entry ", StringComparison.Ordinal) };
        var edited = await session.CompileAsync(sources, options);
        Assert.IsEmpty(edited.Diagnostics, string.Join("\n", edited.Diagnostics));
        Assert.IsNull(session.Snapshot);
        var fresh = await JoltProjectCompiler.CompileAsync(sources, options);
        CollectionAssert.AreEqual(fresh.Modules.ToArray(), edited.Modules.ToArray());
        Assert.AreNotSame(initial.Modules.Single(module => module.Name == "Table"), edited.Modules.Single(module => module.Name == "Table"));
    }

    [TestMethod]
    public async Task StaticMarkupLineInsertionKeepsAuthoredSourceMapsCurrent()
    {
        var session = new JoltDevCompilation();
        var options = new JoltCompileOptions();
        JoltSource[] sources = [new("Card.jazor", """
            <p>Hello</p>
            @code { [Parameter] public string Title { get; set; } = ""; }
            """), new("Other.jazor", "<p>Other</p>")];
        var initial = await session.CompileAsync(sources, options);
        sources[0] = sources[0] with { Content = sources[0].Content.Replace("Hello", "Hello\nthere", StringComparison.Ordinal) };
        var edited = await session.CompileAsync(sources, options);
        var fresh = await JoltProjectCompiler.CompileAsync(sources, options);
        Assert.IsEmpty(edited.Diagnostics, string.Join("\n", edited.Diagnostics));
        CollectionAssert.AreEqual(fresh.Modules.ToArray(), edited.Modules.ToArray());
        Assert.AreSame(initial.Modules.Single(module => module.Name == "Other"), edited.Modules.Single(module => module.Name == "Other"));
    }

    [TestMethod]
    public async Task StaticEditIgnoresRenderNamesInAuthoredNestedClasses()
    {
        var session = new JoltDevCompilation();
        var options = new JoltCompileOptions();
        JoltSource[] sources = [new("Main.jazor", """
            <p>Hello</p>
            @code {
                public record Container {
                    public class __StateMain {
                        public string __JoltRender() => "authored";
                    }
                }
            }
            """), new("Other.jazor", "<p>Other</p>")];
        var initial = await session.CompileAsync(sources, options);
        Assert.IsEmpty(initial.Diagnostics, string.Join("\n", initial.Diagnostics));
        sources[0] = sources[0] with { Content = sources[0].Content.Replace("Hello", "Welcome", StringComparison.Ordinal) };
        var edited = await session.CompileAsync(sources, options);
        var fresh = await JoltProjectCompiler.CompileAsync(sources, options);
        Assert.IsEmpty(edited.Diagnostics, string.Join("\n", edited.Diagnostics));
        CollectionAssert.AreEqual(fresh.Modules.ToArray(), edited.Modules.ToArray());
        Assert.AreSame(initial.Modules.Single(module => module.Name == "Other"), edited.Modules.Single(module => module.Name == "Other"));
    }

    [TestMethod]
    public async Task EquivalentVueMetadataReusesModulesButChangedMetadataDoesNot()
    {
        var session = new JoltDevCompilation();
        JoltSource[] sources = [new("Main.jazor", "<p>Log in</p>"), new("Other.jazor", "<p>Other</p>")];
        var initial = await session.CompileAsync(sources, Options("string"));
        sources[0] = sources[0] with { Content = "<p>Sign in</p>" };
        var edited = await session.CompileAsync(sources, Options("string"));
        Assert.AreSame(initial.Modules.Single(module => module.Name == "Other"), edited.Modules.Single(module => module.Name == "Other"));
        sources[0] = sources[0] with { Content = "<p>Welcome</p>" };
        var changed = await session.CompileAsync(sources, Options("boolean"));
        Assert.AreNotSame(edited.Modules.Single(module => module.Name == "Other"), changed.Modules.Single(module => module.Name == "Other"));

        static JoltCompileOptions Options(string type) => new()
        {
            VueComponents = [new NpmVueComponent("./Other.vue", "default", "", "", "Other.vue", "Other.vue",
                new NpmDefinition("Other.vue", 0, 1), new Dictionary<string, NpmVueProp> { ["flag"] = new(type, false, [], null) },
                new Dictionary<string, NpmVueEvent>(), new Dictionary<string, NpmVueSlot>())
                { LocalPath = "Other.vue", LocalNamespace = "Vue.Components" }],
        };
    }
}
