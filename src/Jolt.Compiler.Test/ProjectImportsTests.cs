using Jazor.Npm;
using Jolt.Npm;
using Jolt.Project;

namespace Jolt.Compiler.Test;

[TestClass]
public sealed class ProjectImportsTests
{
    [TestMethod]
    public async Task LoadSources_ReusesUnchangedParsesAcrossDiskAndEditorSnapshots()
    {
        var directory = Directory.CreateDirectory(Path.Combine(AppContext.BaseDirectory, "jolt-sources-" + Guid.NewGuid()));
        try
        {
            var page = Path.Combine(directory.FullName, "Page.jazor");
            var card = Path.Combine(directory.FullName, "Card.jazor");
            await File.WriteAllTextAsync(page, "<p>A</p>");
            await File.WriteAllTextAsync(card, "<b />");
            var initial = await JoltProjectLoader.LoadSourcesAsync(directory.FullName, null, CancellationToken.None);
            var overlay = new Dictionary<string, string> { [page] = "<p>B</p>" };
            var edited = await JoltProjectLoader.LoadSourcesAsync(directory.FullName, overlay, CancellationToken.None, initial.Sources);
            var before = JoltProjectImports.ParseCold(initial.Sources.Reverse().ToArray(), CancellationToken.None).Documents;
            CollectionAssert.AreEqual(new[] { "Card.jazor", "Page.jazor" }, before.Select(document => document.Path).ToArray());
            var after = JoltProjectImports.Parse(edited.Sources).Documents;
            Assert.AreSame(before.Single(doc => doc.Path == "Card.jazor"), after.Single(doc => doc.Path == "Card.jazor"));
            Assert.AreEqual(overlay[page], after.Single(doc => doc.Path == "Page.jazor").Source);

            var stamp = File.GetLastWriteTimeUtc(page);
            await File.WriteAllTextAsync(page, "<p>C</p>");
            File.SetLastWriteTimeUtc(page, stamp);
            File.Move(card, Path.Combine(directory.FullName, "Row.jazor"));
            var disk = await JoltProjectLoader.LoadSourcesAsync(directory.FullName, null, CancellationToken.None, edited.Sources);
            CollectionAssert.AreEqual(new[] { "Page.jazor", "Row.jazor" }, disk.Sources.Select(source => source.Path).ToArray());
            Assert.AreEqual("<p>C</p>", JoltProjectImports.Parse(disk.Sources).Documents[0].Source);
        }
        finally { directory.Delete(recursive: true); }
    }

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
    public void PortableSnapshot_RebasesDefinitionAndRealpathsWithoutChangingDocumentation()
    {
        const string json = """
            {"definition":{"path":"producer/types.d.ts","start":12,"length":6,
            "documentation":{"parameters":{"modulePath":"producer/example", "realpaths":"producer/example", "inputs":"producer/example"}}},
            "realpaths":{"producer/second":"producer/types.d.ts","producer/first":"producer/index.d.ts"},
            "inputs":["producer/types.d.ts",null]}
            """;
        var snapshot = System.Text.Json.Nodes.JsonNode.Parse(json)!;
        var definition = snapshot["definition"]!;
        var documentation = definition["documentation"];
        var rebased = NpmValueMetadataProvider.RebaseSnapshot(snapshot, value =>
            value.StartsWith("producer/", StringComparison.Ordinal) ? "consumer/" + value["producer/".Length..] : null);
        Assert.AreSame(snapshot, rebased);
        Assert.AreSame(documentation, definition["documentation"]);
        Assert.AreEqual("producer/example", (string?)documentation!["parameters"]!["modulePath"]);
        Assert.AreEqual("producer/example", (string?)documentation["parameters"]!["realpaths"]);
        Assert.AreEqual("producer/example", (string?)documentation["parameters"]!["inputs"]);
        Assert.AreEqual("consumer/types.d.ts", (string?)definition["path"]);
        Assert.AreEqual(12, (int)definition["start"]!);
        Assert.AreEqual(6, (int)definition["length"]!);
        Assert.AreEqual("""{"consumer/second":"consumer/types.d.ts","consumer/first":"consumer/index.d.ts"}""",
            snapshot["realpaths"]!.ToJsonString());
        Assert.AreEqual("""["consumer/types.d.ts",null]""", snapshot["inputs"]!.ToJsonString());
    }

    [TestMethod]
    public void NpmValidation_ReadsPackageSelectorsAgainInANewValidation()
    {
        var project = Directory.CreateDirectory(Path.Combine(AppContext.BaseDirectory, "jolt-manifest-validation-" + Guid.NewGuid()));
        try
        {
            var manifest = Path.Combine(project.FullName, "package.json");
            File.WriteAllText(manifest, """{"type":"module"}""");
            var timestamp = File.GetLastWriteTimeUtc(manifest);
            var validation = new NpmValueMetadataProvider.Validation();
            var original = validation.ReadPackageFingerprints(manifest);
            Assert.AreEqual(original.Full, validation.ReadFingerprint(manifest));

            File.WriteAllText(manifest, """{"type":"commonjs"}""");
            File.SetLastWriteTimeUtc(manifest, timestamp);
            Assert.AreEqual(original, validation.ReadPackageFingerprints(manifest));
            var refreshed = new NpmValueMetadataProvider.Validation().ReadPackageFingerprints(manifest);
            Assert.AreNotEqual(original.Full, refreshed.Full);
            Assert.AreNotEqual(original.Selectors, refreshed.Selectors);

            validation.Clear();
            Assert.AreEqual(refreshed, validation.ReadPackageFingerprints(manifest));
            Assert.AreEqual(refreshed.Full, validation.ReadFingerprint(manifest));
        }
        finally
        {
            project.Delete(recursive: true);
        }
    }

    [TestMethod]
    public async Task LoadAsync_BatchesUsedNpmComponentsAcrossDocumentsAndRefreshesDeclarations()
    {
        var root = Directory.CreateDirectory(Path.Combine(AppContext.BaseDirectory, "jolt-component-imports-" + Guid.NewGuid()));
        try
        {
            var package = root.CreateSubdirectory("fixture");
            await File.WriteAllTextAsync(Path.Combine(root.FullName, "package.json"), """
                {"private":true,"dependencies":{"component-fixture":"link:./fixture","vue":"3.5.30"}}
                """);
            await File.WriteAllTextAsync(Path.Combine(package.FullName, "package.json"), """
                {"name":"component-fixture","version":"1.0.0","type":"module","exports":{"types":"./index.d.ts","import":"./index.js"},"web-types":"./web-types.json"}
                """);
            var webTypes = Path.Combine(package.FullName, "web-types.json");
            const string metadata = """
                {"contributions":{"html":{"vue-components":[{
                    "name":"fixture-card","source":{"symbol":"Card"},"description":"Card from Web Types",
                    "props":[
                        {"name":"label","description":"Label from Web Types"},
                        {"name":"item-count","description":"Count from Web Types"}
                    ],"slots":[
                        {"name":"default","description":"Card contents"},
                        {"name":"detail"}
                    ]
                },{
                    "name":"fixture-dialog","source":{"symbol":"Dialog"},"description":"Dialog from Web Types"
                }]}}}
                """;
            await File.WriteAllTextAsync(webTypes, metadata);
            await File.WriteAllTextAsync(Path.Combine(package.FullName, "index.d.ts"), "export { Card, Dialog, Unused } from './types.js';");
            var declaration = Path.Combine(package.FullName, "types.d.ts");
            const string types = """
                type Component<Props, Slots = {}, Emit = {}> = new () => { $props: Props; $emit: Emit; $slots: Slots };
                export declare const Card: Component<{
                    /** Label from declarations. */
                    label: string;
                    itemCount?: number;
                }, { detail: (scope: { count: number }) => unknown }, (name: "click", ...args: any[]) => void>;
                /** Dialog from declarations. */
                export declare const Dialog: Component<{ open: boolean }>;
                export declare const Unused: Component<{}>;
                """;
            await File.WriteAllTextAsync(declaration, types);
            await File.WriteAllTextAsync(Path.Combine(package.FullName, "index.js"), "export const Card = {}, Dialog = {}, Unused = {};");
            await File.WriteAllTextAsync(Path.Combine(root.FullName, "_Imports.jazor"), "@using Npm.ComponentFixture");
            var pages = root.CreateSubdirectory("Pages");
            await File.WriteAllTextAsync(Path.Combine(pages.FullName, "First.jazor"), "<Card Label=\"hello\" />");
            var second = Path.Combine(pages.FullName, "Second.jazor");
            await File.WriteAllTextAsync(second, "<Card Label=\"again\" />");
            var overlays = new Dictionary<string, string> { [second] = "<Dialog Open=\"@true\" /><Card Label=\"again\" />" };
            var engineCache = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Jolt", "pnpm-store");
            var queries = 0;
            var provider = new NpmVueMetadataProvider(Path.Combine(root.FullName, ".pnpm-store"), engineCache)
            {
                ObserveQueryInputs = _ => queries++,
            };

            var sources = await JoltProjectLoader.LoadSourcesAsync(root.FullName, overlays, CancellationToken.None);
            var progress = new JoltNpmLoadProgress();
            var catalogAvailableAtContracts = false;
            provider.ObserveQueryInputs = _ =>
            {
                queries++;
                if (queries == 2)
                    catalogAvailableAtContracts = progress.Catalog.Task.IsCompletedSuccessfully &&
                        !progress.Components.Task.IsCompleted;
            };
            var workspace = await JoltProjectLoader.LoadNpmAsync(root.FullName, sources, provider, CancellationToken.None, progress);
            Assert.IsEmpty(workspace.Diagnostics, string.Join(Environment.NewLine, workspace.Diagnostics));
            Assert.AreEqual(2, queries, "The export catalog and all requested component contracts should each be read once.");
            Assert.IsTrue(catalogAvailableAtContracts, "The catalog should be available while component contracts are still loading.");
            Assert.AreSame(workspace.Exports, await progress.Catalog.Task);
            CollectionAssert.AreEqual(new[] { "Card", "Dialog", "Unused" }, workspace.Exports["component-fixture"].ToArray());
            var components = workspace.Options.NpmComponents!;
            CollectionAssert.AreEqual(components.ToArray(), (await progress.Components.Task).ToArray());
            CollectionAssert.AreEqual(new[] { "Card", "Dialog" }, components.Select(component => component.ExportName).ToArray());
            Assert.AreEqual("string", components[0].Props["label"].Type);
            Assert.AreEqual("boolean", components[1].Props["open"].Type);
            Assert.AreEqual("Card from Web Types", components[0].Definition?.Documentation?.Summary);
            Assert.AreEqual("Dialog from declarations.", components[1].Definition?.Documentation?.Summary);
            Assert.AreEqual("Label from declarations.", components[0].Props["label"].Definition?.Documentation?.Summary);
            Assert.AreEqual("Count from Web Types", components[0].Props["itemCount"].Definition?.Documentation?.Summary);
            Assert.AreEqual("types.d.ts", Path.GetFileName(components[0].Definition?.Path));
            Assert.AreEqual("types.d.ts", Path.GetFileName(components[0].Props["itemCount"].Definition?.Path));
            Assert.AreEqual("Card contents", components[0].Slots["default"].Definition?.Documentation?.Summary);
            var slotDefinition = components[0].Slots["default"].Definition!;
            Assert.AreEqual("web-types.json", Path.GetFileName(slotDefinition.Path));
            Assert.AreEqual("default", (await File.ReadAllTextAsync(slotDefinition.Path)).Substring(slotDefinition.Start, slotDefinition.Length));
            Assert.AreEqual("number", components[0].Slots["detail"].Signatures.Single().Single().Properties!["count"]);
            Assert.IsTrue(components[0].Events["click"].Signatures.Single().Single().UntypedRest);

            var webTypesTimestamp = File.GetLastWriteTimeUtc(webTypes);
            await File.WriteAllTextAsync(webTypes, metadata
                .Replace("Card contents", "Updated card contents", StringComparison.Ordinal)
                .Replace("Card from Web Types", "New description", StringComparison.Ordinal)
                .Replace("Count from Web Types", "New count description", StringComparison.Ordinal));
            File.SetLastWriteTimeUtc(webTypes, webTypesTimestamp);
            var updated = await provider.ReadComponentsAsync(second, "component-fixture", ["Card", "Dialog"]);
            Assert.AreEqual("Updated card contents", updated["Card"].Component!.Slots["default"].Definition?.Documentation?.Summary);
            Assert.AreEqual("New description", updated["Card"].Component!.Definition?.Documentation?.Summary);
            Assert.AreEqual("New count description", updated["Card"].Component!.Props["itemCount"].Definition?.Documentation?.Summary);

            var timestamp = File.GetLastWriteTimeUtc(declaration);
            await File.WriteAllTextAsync(declaration, types.Replace("label: string", "label: number", StringComparison.Ordinal));
            File.SetLastWriteTimeUtc(declaration, timestamp);
            queries = 0;
            var refreshed = await JoltProjectLoader.LoadAsync(root.FullName, provider, overlays);
            Assert.IsEmpty(refreshed.Diagnostics, string.Join(Environment.NewLine, refreshed.Diagnostics));
            Assert.AreEqual(2, queries);
            Assert.AreEqual("number", refreshed.Options.NpmComponents!.Single(component => component.ExportName == "Card").Props["label"].Type);

            // A newly present higher-priority source changes the declaration
            // graph even though the previously resolved file stays untouched.
            var source = Path.Combine(package.FullName, "types.ts");
            await File.WriteAllTextAsync(source, types.Replace("label: string", "label: boolean", StringComparison.Ordinal));
            var replaced = await JoltProjectLoader.LoadAsync(root.FullName, provider, overlays);
            Assert.IsEmpty(replaced.Diagnostics, string.Join(Environment.NewLine, replaced.Diagnostics));
            Assert.AreEqual("boolean", replaced.Options.NpmComponents!.Single(component => component.ExportName == "Card").Props["label"].Type);

            File.Delete(source);
            var removed = await JoltProjectLoader.LoadAsync(root.FullName, provider, overlays);
            Assert.IsEmpty(removed.Diagnostics, string.Join(Environment.NewLine, removed.Diagnostics));
            Assert.AreEqual("number", removed.Options.NpmComponents!.Single(component => component.ExportName == "Card").Props["label"].Type);
        }
        finally
        {
            root.Delete(recursive: true);
        }
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
            var progress = new JoltNpmLoadProgress();
            var componentsAvailableAtValues = false;
            var provider = new NpmVueMetadataProvider(Path.Combine(root.FullName, ".pnpm-store"), engineCache)
            {
                ObserveQueryInputs = _ => componentsAvailableAtValues = progress.Components.Task.IsCompletedSuccessfully,
            };
            var sources = await JoltProjectLoader.LoadSourcesAsync(root.FullName, null, CancellationToken.None);
            var workspace = await JoltProjectLoader.LoadNpmAsync(root.FullName, sources, provider, CancellationToken.None, progress);
            Assert.IsEmpty(workspace.Diagnostics, string.Join(Environment.NewLine, workspace.Diagnostics));
            Assert.IsTrue(componentsAvailableAtValues, "Value queries should begin after the component phase is available.");
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
