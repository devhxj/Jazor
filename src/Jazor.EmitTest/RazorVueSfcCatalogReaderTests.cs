using Basic.Reference.Assemblies;
using Jazor.Emit;
using Jazor.RazorVue.Analysis;
using Jazor.RazorVue.Artifacts;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.AspNetCore.Components;
using System.Reflection;

namespace Jazor.EmitTest;

[TestClass]
public sealed class RazorVueSfcCatalogReaderTests
{
    [TestMethod]
    public void RazorVueSfcCatalogReader_ReadsGeneratedCatalogFromAssembly()
    {
        var assembly = CompileCatalogAssembly(
            "RazorVue.Sfc.Reader.Tests",
            DefaultGeneratedArtifactMembers);

        var catalog = RazorVueSfcCatalogReader.TryRead(assembly);

        Assert.IsNotNull(catalog);
        Assert.AreEqual("RazorVue.Sfc.Reader.Tests", catalog.AssemblyName);
        Assert.HasCount(1, catalog.Artifacts);

        var artifact = catalog.Artifacts[0];
        Assert.AreEqual("CounterCard", artifact.ComponentName);
        Assert.AreEqual("components/counter-card.vue", artifact.RelativeSfcPath);
        Assert.AreEqual("<div>{{ value }}</div>", artifact.TemplateText);
        Assert.AreEqual("const value = 1;", artifact.ScriptSetupText);
        Assert.AreEqual("style-hash", artifact.Identity.StyleHash);
        Assert.AreEqual(RazorVueSfcOriginKindRecord.Style, artifact.StyleBlocks[0].SourceOrigins[0].OriginKind);
        Assert.AreEqual("category", artifact.CustomBlocks[0].Attributes[0].Name);
        Assert.AreEqual(RazorVueSfcOriginKindRecord.CustomBlock, artifact.CustomBlocks[0].SourceOrigins[0].OriginKind);
        CollectionAssert.AreEquivalent(new[] { "vue", "./button.mjs" }, artifact.Imports.ToArray());
        CollectionAssert.AreEquivalent(new[] { "vuetify/styles" }, artifact.Styles.ToArray());
        CollectionAssert.AreEquivalent(new[] { "vuetify" }, artifact.PluginRequirements.ToArray());
        Assert.HasCount(5, artifact.SourceOrigins);
    }

    [TestMethod]
    public void RazorVueSfcCatalogReader_ThrowsWhenStyleBlocksIsNull()
    {
        var assembly = CompileCatalogAssembly(
            "RazorVue.Sfc.Reader.NullStyleBlocks",
            """
            public string ComponentName => "CounterCard";
            public string RelativeSfcPath => "components/counter-card.vue";
            public string SfcText => "<template><div>{{ value }}</div></template>";
            public GeneratedTemplateBlock TemplateBlock => new GeneratedTemplateBlock();
            public GeneratedScriptSetupBlock ScriptSetupBlock => new GeneratedScriptSetupBlock();
            public GeneratedStyleBlock[]? StyleBlocks => null;
            public GeneratedCustomBlock[] CustomBlocks => new[] { new GeneratedCustomBlock() };
            public string[] Imports => new[] { "vue" };
            public string[] Styles => new[] { "vuetify/styles" };
            public string[] PluginRequirements => new[] { "vuetify" };
            public GeneratedIdentity Identity => new GeneratedIdentity();
            public GeneratedHints Hints => new GeneratedHints();
            public GeneratedOrigin[] SourceOrigins => new[] { new GeneratedOrigin(GeneratedOriginKind.Component, "Counter.razor", 0, 128, "components/counter-card.vue", 0, 128) };
            """);

        var exception = Assert.ThrowsExactly<InvalidOperationException>(() => RazorVueSfcCatalogReader.TryRead(assembly));
        StringAssert.Contains(exception.Message, "StyleBlocks");
    }

    [TestMethod]
    public void RazorVueSfcCatalogReader_ReadsGeneratedCatalogFromRealGeneratorAssembly()
    {
        var compilation = CreateRazorVueCompilation(
            "RazorVue.Sfc.Reader.Integration.Tests",
            """
            using System;
            using ECMAScript.VueContract;
            using static ECMAScript.Vue3;
            using Microsoft.AspNetCore.Components;
            using Microsoft.AspNetCore.Components.Rendering;

            namespace ECMAScript
            {
                [AttributeUsage(AttributeTargets.Class, Inherited = false)]
                public sealed class ECMAScriptModuleAttribute : Attribute
                {
                    public ECMAScriptModuleAttribute() { }
                    public ECMAScriptModuleAttribute(string import) { }
                }
            }

            namespace Demo.Components
            {
                [ECMAScript.ECMAScriptModule("./components/counter-card")]
                public class CounterCard : ComponentBase, IVueComponent
                {
                    [Parameter]
                    public string? Title { get; set; }

                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        builder.OpenElement(0, "section");
                        builder.AddContent(1, Title);
                        builder.CloseElement();
                    }
                }
            }
            """,
            "CounterCard.razor");

        var assembly = CompileRazorVueGeneratedAssembly(compilation, "sfc");

        var catalog = RazorVueSfcCatalogReader.TryRead(assembly);

        Assert.IsNotNull(catalog);
        Assert.AreEqual("RazorVue.Sfc.Reader.Integration.Tests", catalog.AssemblyName);
        Assert.HasCount(1, catalog.Artifacts);
        Assert.AreEqual("components/counter-card.vue", catalog.Artifacts[0].RelativeSfcPath);
        StringAssert.Contains(catalog.Artifacts[0].SfcText, "<template>");
        StringAssert.Contains(catalog.Artifacts[0].SfcText, "<script setup lang=\"ts\">");
    }

    private const string DefaultGeneratedArtifactMembers =
        """
        public string ComponentName => "CounterCard";
        public string RelativeSfcPath => "components/counter-card.vue";
        public string SfcText => "<template><div>{{ value }}</div></template>\n<script setup lang=\"ts\">\nconst value = 1;\n</script>\n<style scoped>\n.card { color: red; }\n</style>";
        public GeneratedTemplateBlock TemplateBlock => new GeneratedTemplateBlock();
        public GeneratedScriptSetupBlock ScriptSetupBlock => new GeneratedScriptSetupBlock();
        public GeneratedStyleBlock[] StyleBlocks => new[] { new GeneratedStyleBlock() };
        public GeneratedCustomBlock[] CustomBlocks => new[] { new GeneratedCustomBlock() };
        public string[] Imports => new[] { "vue", "./button.mjs" };
        public string[] Styles => new[] { "vuetify/styles" };
        public string[] PluginRequirements => new[] { "vuetify" };
        public GeneratedIdentity Identity => new GeneratedIdentity();
        public GeneratedHints Hints => new GeneratedHints();
        public GeneratedOrigin[] SourceOrigins => new[]
        {
            new GeneratedOrigin(GeneratedOriginKind.Component, "Counter.razor", 0, 128, "components/counter-card.vue", 0, 128),
            new GeneratedOrigin(GeneratedOriginKind.Template, "Counter.razor", 0, 48, "components/counter-card.vue", 0, 48),
            new GeneratedOrigin(GeneratedOriginKind.Logic, "Counter.razor", 49, 40, "components/counter-card.vue", 49, 40),
            new GeneratedOrigin(GeneratedOriginKind.Style, "Counter.razor.css", 90, 24, "components/counter-card.vue", 90, 24),
            new GeneratedOrigin(GeneratedOriginKind.CustomBlock, "Counter.razor", 115, 10, "components/counter-card.vue", 115, 10)
        };
        """;

    private static Assembly CompileCatalogAssembly(string assemblyName, string artifactMembers)
    {
        var source = $$$"""
        using System;

        namespace Jazor.Generated
        {
            internal static class RazorVueCatalog
            {
                internal static string AssemblyName => "{{{assemblyName}}}";

                internal static System.Collections.IEnumerable GetArtifacts()
                    => new object[] { new GeneratedArtifact() };

                private sealed class GeneratedArtifact
                {
        {{{artifactMembers}}}
                }

                private sealed class GeneratedIdentity
                {
                    public string ComponentId => "Demo.Components.CounterCard";
                    public string ModuleId => "components/counter-card.vue";
                    public string DescriptorHash => "descriptor-hash";
                    public string TemplateHash => "template-hash";
                    public string LogicHash => "logic-hash";
                    public string StyleHash => "style-hash";
                    public GeneratedHmrBoundaryKind HmrBoundaryKind => GeneratedHmrBoundaryKind.LogicSafe;
                }

                private sealed class GeneratedHints
                {
                    public bool RequiresVueRuntime => true;
                    public bool RequiresHydration => false;
                    public bool SupportsSsr => true;
                    public bool UsesTeleport => false;
                    public bool UsesSuspense => false;
                    public bool UsesKeepAlive => false;
                }

                private sealed class GeneratedTemplateBlock
                {
                    public string Text => "<div>{{ value }}</div>";
                    public GeneratedOrigin[] SourceOrigins => new[]
                    {
                        new GeneratedOrigin(GeneratedOriginKind.Template, "Counter.razor", 0, 48, "components/counter-card.vue", 0, 48)
                    };
                }

                private sealed class GeneratedScriptSetupBlock
                {
                    public string Text => "const value = 1;";
                    public string Language => "ts";
                    public GeneratedOrigin[] SourceOrigins => new[]
                    {
                        new GeneratedOrigin(GeneratedOriginKind.Logic, "Counter.razor", 49, 40, "components/counter-card.vue", 49, 40)
                    };
                }

                private sealed class GeneratedStyleBlock
                {
                    public string Text => ".card { color: red; }";
                    public bool IsScoped => true;
                    public string? ModuleName => null;
                    public string Language => "css";
                    public string SourceFilePath => "Counter.razor.css";
                    public GeneratedOrigin[] SourceOrigins => new[]
                    {
                        new GeneratedOrigin(GeneratedOriginKind.Style, "Counter.razor.css", 90, 24, "components/counter-card.vue", 90, 24)
                    };
                }

                private sealed class GeneratedCustomBlock
                {
                    public string Name => "docs";
                    public string Text => "{ \"category\": \"demo\" }";
                    public string Language => "json";
                    public GeneratedAttribute[] Attributes => new[]
                    {
                        new GeneratedAttribute("category", "demo")
                    };
                    public string SourceFilePath => "Counter.razor";
                    public GeneratedOrigin[] SourceOrigins => new[]
                    {
                        new GeneratedOrigin(GeneratedOriginKind.CustomBlock, "Counter.razor", 115, 10, "components/counter-card.vue", 115, 10)
                    };
                }

                private sealed class GeneratedAttribute
                {
                    public GeneratedAttribute(string name, string? value)
                    {
                        Name = name;
                        Value = value;
                    }

                    public string Name { get; }

                    public string? Value { get; }
                }

                private sealed class GeneratedOrigin
                {
                    public GeneratedOrigin(
                        GeneratedOriginKind originKind,
                        string sourceFilePath,
                        int sourceSpanStart,
                        int sourceSpanLength,
                        string? generatedFilePath,
                        int? generatedSpanStart,
                        int? generatedSpanLength)
                    {
                        OriginKind = originKind;
                        SourceFilePath = sourceFilePath;
                        SourceSpanStart = sourceSpanStart;
                        SourceSpanLength = sourceSpanLength;
                        GeneratedFilePath = generatedFilePath;
                        GeneratedSpanStart = generatedSpanStart;
                        GeneratedSpanLength = generatedSpanLength;
                    }

                    public GeneratedOriginKind OriginKind { get; }

                    public string SourceFilePath { get; }

                    public int SourceSpanStart { get; }

                    public int SourceSpanLength { get; }

                    public string? GeneratedFilePath { get; }

                    public int? GeneratedSpanStart { get; }

                    public int? GeneratedSpanLength { get; }

                    public int StartLine => 1;

                    public int StartColumn => 1;

                    public GeneratedMappingQuality MappingQuality => GeneratedMappingQuality.MappedFromGenerated;

                    public GeneratedOriginProvenance Provenance => GeneratedOriginProvenance.GeneratedSyntaxLocation;
                }

                private enum GeneratedHmrBoundaryKind
                {
                    Unknown,
                    TemplateOnly,
                    LogicSafe,
                    FullReloadRequired
                }

                private enum GeneratedOriginKind
                {
                    Component,
                    Descriptor,
                    Template,
                    Logic,
                    GeneratedRender,
                    Style,
                    CustomBlock
                }

                private enum GeneratedMappingQuality
                {
                    ExactSource,
                    MappedFromGenerated,
                    GeneratedOnly
                }

                private enum GeneratedOriginProvenance
                {
                    RazorSourceMap,
                    GeneratedSyntaxLocation,
                    GeneratedFallback
                }
            }
        }
        """;

        return CompileAssembly(CreateCompilation(assemblyName, source, $"{assemblyName}.g.cs"));
    }

    private static CSharpCompilation CreateCompilation(string assemblyName, string source, string sourcePath)
        => CSharpCompilation.Create(
            assemblyName: assemblyName,
            syntaxTrees: [CSharpSyntaxTree.ParseText(source, path: sourcePath)],
            references: Net110.References.All.Cast<MetadataReference>(),
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

    private static Assembly CompileAssembly(Compilation compilation)
    {
        using var stream = new MemoryStream();
        var emitResult = compilation.Emit(stream);
        Assert.IsTrue(emitResult.Success, string.Join("\n", emitResult.Diagnostics.Select(static diagnostic => diagnostic.ToString())));
        stream.Position = 0;
        return Assembly.Load(stream.ToArray());
    }

    private static CSharpCompilation CreateRazorVueCompilation(string assemblyName, string source, string sourcePath)
        => CSharpCompilation.Create(
            assemblyName,
            [
                CSharpSyntaxTree.ParseText(CreateRazorVueGlobalUsingsSource(), path: "__RazorVueGlobalUsings.g.cs"),
                CSharpSyntaxTree.ParseText(source, path: sourcePath)
            ],
            Net110.References.All.Cast<MetadataReference>().Concat([
                MetadataReference.CreateFromFile(typeof(ECMAScript.Contract.IUIComponent).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(ECMAScript.VueContract.VueLibraryComponentAttribute).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(ECMAScript.Vue3.IVueComponent).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(ComponentBase).Assembly.Location)
            ]),
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

    private static string CreateRazorVueGlobalUsingsSource()
        => """
           global using ECMAScript.VueContract;
           global using static ECMAScript.Vue3;
           """;

    private static Assembly CompileRazorVueGeneratedAssembly(Compilation compilation, string razorVueOutputMode)
    {
        GeneratorDriver driver = CSharpGeneratorDriver.Create(
            [new RazorVueGenerator().AsSourceGenerator()],
            additionalTexts: null,
            parseOptions: (CSharpParseOptions?)compilation.SyntaxTrees.FirstOrDefault()?.Options,
            optionsProvider: CreateAnalyzerConfigOptionsProvider(razorVueOutputMode),
            driverOptions: default);
        driver = driver.RunGeneratorsAndUpdateCompilation(compilation, out var outputCompilation, out _);

        var diagnostics = outputCompilation.GetDiagnostics()
            .Where(static diagnostic => diagnostic.Severity == DiagnosticSeverity.Error)
            .ToArray();
        Assert.AreEqual(0, diagnostics.Length, string.Join("\n", diagnostics.Select(static diagnostic => diagnostic.ToString())));

        return CompileAssembly(outputCompilation);
    }

    private static AnalyzerConfigOptionsProvider CreateAnalyzerConfigOptionsProvider(string razorVueOutputMode)
        => new TestAnalyzerConfigOptionsProvider(new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["build_property.JazorRazorVueOutputMode"] = razorVueOutputMode
        });

    private sealed class TestAnalyzerConfigOptionsProvider(IReadOnlyDictionary<string, string> globalOptions) : AnalyzerConfigOptionsProvider
    {
        private readonly AnalyzerConfigOptions _globalOptions = new TestAnalyzerConfigOptions(globalOptions);
        private static readonly AnalyzerConfigOptions EmptyOptions = new TestAnalyzerConfigOptions(new Dictionary<string, string>(StringComparer.Ordinal));

        public override AnalyzerConfigOptions GlobalOptions => _globalOptions;

        public override AnalyzerConfigOptions GetOptions(SyntaxTree tree)
            => EmptyOptions;

        public override AnalyzerConfigOptions GetOptions(AdditionalText textFile)
            => EmptyOptions;
    }

    private sealed class TestAnalyzerConfigOptions(IReadOnlyDictionary<string, string> values) : AnalyzerConfigOptions
    {
        private readonly IReadOnlyDictionary<string, string> _values = values;

        public override bool TryGetValue(string key, out string value)
            => _values.TryGetValue(key, out value!);
    }
}
