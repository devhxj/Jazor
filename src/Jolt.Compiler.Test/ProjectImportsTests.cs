using Jazor.Npm;
using Jolt.Npm;
using Jolt.Project;

namespace Jolt.Compiler.Test;

[TestClass]
public sealed class ProjectImportsTests
{
    [TestMethod]
    public void Analyze_ReusesParsedSourcesAndReparsesEditedRecords()
    {
        var source = new JoltSource("Page.jazor", "<div>Original</div>");
        var document = JoltProjectImports.Parse([source]).Documents.Single();
        Assert.AreSame(document, JoltProjectCompiler.Analyze([source]).Documents.Single().Document);
        var edited = source with { Content = "<div>Edited</div>" };
        var editedDocument = JoltProjectCompiler.Analyze([edited]).Documents.Single().Document;
        Assert.AreEqual(edited.Content, editedDocument.Source);
        Assert.AreSame(editedDocument, JoltProjectImports.Parse([edited]).Documents.Single());
        var renamed = source with { Path = "Pages/Other.jazor" };
        Assert.AreEqual(renamed.Path, JoltProjectImports.Parse([renamed]).Documents.Single().Path);
        Assert.AreSame(document, JoltProjectImports.Parse([source]).Documents.Single());
    }

    [TestMethod]
    public async Task Compile_EmitsOnlyComponents()
    {
        var result = await JoltProjectCompiler.CompileAsync([
            new("_Imports.jazor", "@using System.Text"),
            new("Pages/Page.jazor", "<div />"),
        ]);
        Assert.IsEmpty(result.Diagnostics, string.Join(Environment.NewLine, result.Diagnostics));
        Assert.AreEqual("Pages/Page.mjs", result.Modules.Single().Path);
    }

    [TestMethod]
    public void Analyze_CascadesImportsInDirectoryOrderAndKeepsThemScoped()
    {
        var definition = new NpmDefinition("index.d.ts", 0, 1);
        var npm = new NpmVueComponent("reka-ui", "CollapsibleRoot", "reka-ui", "1.0.0", "index.mjs", "index.d.ts", definition,
            new Dictionary<string, NpmVueProp>(), new Dictionary<string, NpmVueEvent>(), new Dictionary<string, NpmVueSlot>());
        var result = JoltProjectCompiler.Analyze([
            new("_Imports.jazor", "@using System.Text"),
            new("Area/_Imports.jazor", "@using Npm.RekaUi\n@using App.Widgets"),
            new("Area/Child/_Imports.jazor", "@using System.Threading"),
            new("Area/Child/Editor.jazor", """
                @using System.IO
                <div><Panel /><CollapsibleRoot /></div>
                @code { private StringBuilder Builder = new(); }
                """),
            new("Widgets/Panel.jazor", "<div />"),
            new("Elsewhere/Other.jazor", "<div />"),
        ], new(NpmComponents: [npm]));
        Assert.IsEmpty(result.Diagnostics, string.Join(Environment.NewLine, result.Diagnostics));
        Assert.HasCount(3, result.Documents);
        var editor = result.Documents.Single(document => document.Document.Path == "Area/Child/Editor.jazor");
        CollectionAssert.AreEqual(new[] { "System.Text", "Npm.RekaUi", "App.Widgets", "System.Threading", "System.IO" },
            editor.Imports.ToArray());
        Assert.IsTrue(editor.LocalComponents.Any(component => component.Name == "Panel"));
        var other = result.Documents.Single(document => document.Document.Path == "Elsewhere/Other.jazor");
        CollectionAssert.AreEqual(new[] { "System.Text" }, other.Imports.ToArray());
        Assert.IsFalse(other.LocalComponents.Any(component => component.Name == "Panel"));
    }

    [TestMethod]
    public void Analyze_ReportsInheritedErrorsOnceAtTheImportFile()
    {
        const string imports = "\n\n@using System.MissingJoltNamespace";
        var result = JoltProjectCompiler.Analyze([
            new("Pages/_Imports.jazor", imports),
            new("Pages/First.jazor", "<div />"),
            new("Pages/Nested/Second.jazor", "<div />"),
        ]);
        var diagnostic = result.Diagnostics.Single();
        Assert.AreEqual("CS0234", diagnostic.Id);
        var source = diagnostic.Location.GetMappedLineSpan();
        Assert.AreEqual("Pages/_Imports.jazor", source.Path);
        Assert.AreEqual(2, source.StartLinePosition.Line);
        Assert.AreEqual("MissingJoltNamespace", imports.Substring(diagnostic.Location.SourceSpan.Start, diagnostic.Location.SourceSpan.Length));
    }

    [TestMethod]
    public async Task LoadAsync_UsesUnsavedImportsAndReportsNpmFailuresAtTheirSource()
    {
        var directory = Directory.CreateDirectory(Path.Combine(AppContext.BaseDirectory, "jolt-imports-" + Guid.NewGuid()));
        try
        {
            var pages = directory.CreateSubdirectory("Pages");
            var imports = Path.Combine(pages.FullName, "_Imports.jazor");
            await File.WriteAllTextAsync(imports, "@using System.Text");
            await File.WriteAllTextAsync(Path.Combine(pages.FullName, "First.jazor"), "<div />");
            await File.WriteAllTextAsync(Path.Combine(pages.FullName, "Second.jazor"), "<div />");
            const string overlay = "\n@using Npm.Missing";
            var workspace = await JoltProjectLoader.LoadAsync(directory.FullName, new Dictionary<string, string> { [imports] = overlay });
            var diagnostic = workspace.Diagnostics.Single();
            Assert.AreEqual("JOLT1001", diagnostic.Id);
            var line = diagnostic.Location.GetMappedLineSpan();
            Assert.AreEqual("Pages/_Imports.jazor", line.Path);
            Assert.AreEqual(1, line.StartLinePosition.Line);
            Assert.AreEqual("Npm.Missing", overlay.Substring(diagnostic.Location.SourceSpan.Start, diagnostic.Location.SourceSpan.Length));
        }
        finally
        {
            directory.Delete(recursive: true);
        }
    }

    [TestMethod]
    public async Task LoadAsync_ReportsAmbiguousNpmNamespaceForDisjointPackageExports()
    {
        var directory = Directory.CreateDirectory(Path.Combine(AppContext.BaseDirectory, "jolt-npm-imports-" + Guid.NewGuid()));
        try
        {
            foreach (var (name, export) in new[] { ("wire-cells", "read"), ("wire_cells", "write") })
            {
                var package = directory.CreateSubdirectory(name);
                await File.WriteAllTextAsync(Path.Combine(package.FullName, "package.json"), $$$$"""
                    {"name":"{{{{name}}}}","version":"1.0.0","type":"module","exports":{".":{"types":"./index.d.ts","default":"./index.js"}}}
                    """);
                await File.WriteAllTextAsync(Path.Combine(package.FullName, "index.d.ts"),
                    $"export declare function {export}(): string;");
                await File.WriteAllTextAsync(Path.Combine(package.FullName, "index.js"),
                    $"export function {export}() {{ return '{export}'; }}");
            }
            await File.WriteAllTextAsync(Path.Combine(directory.FullName, "package.json"), """
                {"private":true,"dependencies":{"wire_cells":"link:./wire_cells","wire-cells":"link:./wire-cells"}}
                """);
            await File.WriteAllTextAsync(Path.Combine(directory.FullName, "pnpm-lock.yaml"), """
                lockfileVersion: '9.0'
                settings:
                  autoInstallPeers: true
                  excludeLinksFromLockfile: false
                importers:
                  .:
                    dependencies:
                      wire-cells:
                        specifier: link:./wire-cells
                        version: link:wire-cells
                      wire_cells:
                        specifier: link:./wire_cells
                        version: link:wire_cells
                """);
            var document = Path.Combine(directory.FullName, "Page.jazor");
            await File.WriteAllTextAsync(document, "@using Cells = Npm.WireCells.Exports\n<p>@Cells.Read()</p>");

            const string npmNamespace = "Npm.WireCells";
            var provider = new NpmValueMetadataProvider();
            var matches = await NpmExportSelection.FindModulesAsync(npmNamespace, ["wire_cells", "wire-cells"],
                package => provider.ListSpecifiersAsync(document, package, npmNamespace), validatePackageRoot: true);
            CollectionAssert.AreEqual(new[] { "wire-cells", "wire_cells" }, matches.ToArray());

            var workspace = await JoltProjectLoader.LoadAsync(directory.FullName);
            var diagnostic = workspace.Diagnostics.Single();
            Assert.AreEqual("JOLT1001", diagnostic.Id);
            StringAssert.Contains(diagnostic.GetMessage(),
                "Npm.WireCells matches multiple npm specifiers: wire-cells, wire_cells.");
        }
        finally
        {
            directory.Delete(recursive: true);
        }
    }

    [TestMethod]
    public void Parse_DistinguishesNpmValueAliasesFromOrdinaryUsingDirectives()
    {
        var imports = JoltProjectImports.Parse([new("Page.jazor", """
            @using Text = global::Npm.ValueFixture.Text.Exports;
            @using Npm.ValueFixture.Text
            @using Local = System.Text
            @using static System.Math
            """)]);
        var directives = imports.ImportsFor(imports.Documents.Single()).ToArray();
        Assert.HasCount(4, directives);
        Assert.AreEqual("Text", directives[0].Alias);
        Assert.AreEqual("Npm.ValueFixture.Text", directives[0].NpmNamespace);
        Assert.IsTrue(directives[0].IsNpmValue);
        Assert.AreEqual(directives[0].NpmNamespace, directives[1].NpmNamespace);
        Assert.IsFalse(directives[1].IsNpmValue);
        Assert.IsFalse(directives[2].IsNpm);
        Assert.IsFalse(directives[3].IsNpm);
    }

    [TestMethod]
    public async Task LoadAsync_BindsInheritedValueAliasesAndReportsUnsupportedUsesAtTheirSource()
    {
        var root = Directory.CreateDirectory(Path.Combine(AppContext.BaseDirectory, "jolt-value-imports-" + Guid.NewGuid()));
        try
        {
            var package = root.CreateSubdirectory("fixture");
            await File.WriteAllTextAsync(Path.Combine(root.FullName, "package.json"), """
                {"private":true,"dependencies":{"value-fixture":"file:./fixture"}}
                """);
            await File.WriteAllTextAsync(Path.Combine(package.FullName, "package.json"), """
                {"name":"value-fixture","version":"1.0.0","type":"module","exports":{"./text":{"types":"./text.d.ts","import":"./text.js"},"./*.js":"./public/*.js","./*":"./public/*.js"}}
                """);
            await File.WriteAllTextAsync(Path.Combine(package.FullName, "text.js"), """
                export default function (label) { return label; }
                export function title(label) { return label; }
                export function unsupported(value) { return value; }
                """);
            await File.WriteAllTextAsync(Path.Combine(package.FullName, "text.d.ts"), """
                export default function format(label: string): string;
                export function title(label: string): string;
                export function unsupported(value: any): any;
                """);
            var wildcard = package.CreateSubdirectory("public").CreateSubdirectory("editor");
            await File.WriteAllTextAsync(Path.Combine(wildcard.FullName, "editor.api.js"), "export function theme() { return 'dark'; }");
            await File.WriteAllTextAsync(Path.Combine(wildcard.FullName, "editor.api.d.ts"), "export declare function theme(): string;");
            var pages = root.CreateSubdirectory("Pages");
            await File.WriteAllTextAsync(Path.Combine(pages.FullName, "_Imports.jazor"), "@using Text = Npm.ValueFixture.Text.Exports");
            var page = Path.Combine(pages.FullName, "Page.jazor");
            const string valid = """
                @using Other = Npm.ValueFixture.Text.Exports;
                @using Wildcard = Npm.ValueFixture.Editor.EditorApiJs.Exports
                <p>@Text.Title("label") @Other.Default("another") @Wildcard.Theme()</p>
                @code {
                    private sealed class Local { public string Unsupported(string value) => value; }
                    private string Test(Local Text) => Text.Unsupported("shadowed");
                    private string Spelling = "Text.Unsupported()";
                }
                """;
            await File.WriteAllTextAsync(page, valid);

            // Windows keeps pnpm's native addon loaded until the test host exits. Keep the engine outside the disposable store.
            var engineCache = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Jolt", "pnpm-store");
            var provider = new NpmVueMetadataProvider(Path.Combine(root.FullName, ".pnpm-store"), engineCache);
            var workspace = await JoltProjectLoader.LoadAsync(root.FullName, provider);
            Assert.IsEmpty(workspace.Diagnostics, string.Join(Environment.NewLine, workspace.Diagnostics));
            CollectionAssert.AreEquivalent(new[] { "title", "default", "theme" }, workspace.Options.NpmValues.Select(value => value.ExportName).ToArray());
            Assert.AreEqual("value-fixture/editor/editor.api.js", workspace.Options.NpmValues.Single(value => value.ExportName == "theme").ModuleSpecifier);
            var analysis = JoltProjectCompiler.Analyze(workspace.Sources, workspace.Options);
            Assert.IsEmpty(analysis.Diagnostics, string.Join(Environment.NewLine, analysis.Diagnostics));

            const string invalid = "<p>@Text.Title(\"ok\") @Text.Unsupported(1) @Text.Unsupported(2)</p>";
            var unsupported = await JoltProjectLoader.LoadAsync(root.FullName, provider, new Dictionary<string, string> { [page] = invalid });
            Assert.IsTrue(unsupported.Options.NpmValues.Any(value => value.ExportName == "title"));
            Assert.AreEqual(2, unsupported.Diagnostics.Select(item => item.Location.SourceSpan).Distinct().Count());
            foreach (var diagnostic in unsupported.Diagnostics)
            {
                Assert.AreEqual("JOLT1001", diagnostic.Id);
                Assert.AreEqual("Pages/Page.jazor", diagnostic.Location.GetMappedLineSpan().Path);
                Assert.AreEqual("Unsupported", invalid.Substring(diagnostic.Location.SourceSpan.Start, diagnostic.Location.SourceSpan.Length));
                StringAssert.Contains(diagnostic.GetMessage(), "unsupported");
            }
        }
        finally
        {
            root.Delete(recursive: true);
        }
    }
}
