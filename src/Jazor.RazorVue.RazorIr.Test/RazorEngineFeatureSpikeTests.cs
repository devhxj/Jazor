using System.Reflection;
using Jazor.RazorVue.RazorSdk;
using Microsoft.AspNetCore.Razor.Language;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace Jazor.RazorVue.RazorIr.Test;

[TestClass]
public sealed class RazorEngineFeatureSpikeTests
{
    public TestContext TestContext { get; set; } = default!;

    [TestMethod]
    public void CreateProjectEngine_MinimalHost_ExposesComponentRelatedFeatureTypes()
    {
        var projectEngine = RazorIrTestHost.CreateProjectEngine(@"D:\temp\FeatureProbe.jazor");
        var featureTypeNames = RazorIrTestHost.GetEngineFeatures(projectEngine)
            .Select(static feature => feature.GetType().FullName ?? feature.GetType().Name)
            .OrderBy(static name => name, StringComparer.Ordinal)
            .ToArray();

        foreach (var featureTypeName in featureTypeNames)
        {
            TestContext.WriteLine(featureTypeName);
        }

        Assert.IsTrue(featureTypeNames.Length > 0, "The Razor project engine did not expose any features.");
        Assert.IsTrue(
            featureTypeNames.Any(static name => name.Contains("Component", StringComparison.Ordinal)),
            "The minimal host did not expose any feature type whose name contains 'Component'.");
    }

    [TestMethod]
    public void ProcessDesignTime_ForBindDirectiveAttribute_RevealsWhetherBuiltInTagHelpersAreActive()
    {
        var codeDocument = RazorIrTestHost.CreateCodeDocument(
            @"D:\temp\InventoryBindDirective.jazor",
            """
            <input @bind="message" />

            @code {
                private string message = "hello";
            }
            """);

        var documentNode = RazorIrTestHost.GetDocumentNode(codeDocument);
        var tree = RazorIrTestHost.DumpIntermediateNodeTree(documentNode);

        TestContext.WriteLine(tree);

        StringAssert.Contains(tree, "input");
        Assert.IsTrue(
            tree.Contains("TagHelper", StringComparison.Ordinal) ||
            tree.Contains("@bind", StringComparison.Ordinal) ||
            tree.Contains("bind", StringComparison.OrdinalIgnoreCase),
            "The tree did not expose any obvious trace of @bind processing; re-check whether built-in component/tag-helper passes are active in this host.");
    }

    [TestMethod]
    public void ProcessDesignTime_ForBindDirectiveAttribute_CanInspectGeneratedCSharpAndMappedRazorPath()
    {
        const string documentPath = @"D:\repo\Demo\Pages\TodoApp.razor";
        const string documentText = """<input @bind="Title" />""";

        var parseOptions = new CSharpParseOptions(LanguageVersion.Preview);
        var baseCompilation = CSharpCompilation.Create(
            "RazorVue.RazorIr.BindPathProbe",
            syntaxTrees:
            [
                CSharpSyntaxTree.ParseText(
                    """
                    global using static ECMAScript.Vue3;
                    global using ECMAScript.VueContract;
                    global using Microsoft.AspNetCore.Components;
                    """,
                    options: parseOptions,
                    path: "RazorVueTestGlobalUsings.g.cs"),
                CSharpSyntaxTree.ParseText(
                    """
                    using System;

                    namespace ECMAScript
                    {
                        [AttributeUsage(AttributeTargets.Class, Inherited = false)]
                        public sealed class ECMAScriptModuleAttribute : Attribute
                        {
                            public ECMAScriptModuleAttribute() { }
                            public ECMAScriptModuleAttribute(string import) { }
                        }
                    }

                    namespace Demo.Pages
                    {
                        [ECMAScript.ECMAScriptModule("./components/todo-app")]
                        public partial class TodoApp : ComponentBase, IVueComponent
                        {
                            [Parameter]
                            public string? Title { get; set; }
                        }
                    }
                    """,
                    options: parseOptions,
                    path: "TodoApp.razor.cs")
            ],
            references: RazorIrTestHost.CreateMetadataReferences(),
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        var projectEngine = RazorVueRazorCodeDocumentProvider.CreateProjectEngine(
            documentPath,
            parseOptions,
            rootNamespace: "Demo.Pages");
        var tagHelpers = RazorVueRazorCodeDocumentProvider.DiscoverTagHelpers(projectEngine, baseCompilation);
        var codeDocument = projectEngine.Process(
            RazorVueRazorCodeDocumentProvider.CreateSourceDocument(
                new Jazor.RazorVue.RazorVueRazorDocument(documentPath, SourceText.From(documentText))),
            RazorFileKind.Component,
            [],
            tagHelpers.Length == 0 ? null : TagHelperCollection.Create(tagHelpers));
        var csharpDocument = RazorVueRazorCodeDocumentProvider.GetRequiredCSharpDocument(codeDocument);

        TestContext.WriteLine("Generated C#:");
        TestContext.WriteLine(csharpDocument.Text.ToString());

        var compilation = baseCompilation.AddSyntaxTrees(
            CSharpSyntaxTree.ParseText(
                csharpDocument.Text,
                options: parseOptions,
                path: Path.GetFileName(documentPath) + ".g.cs"));
        var context = Jazor.RazorVue.RazorVueCompilationContext.TryCreate(
            compilation,
            Jazor.RazorVue.RazorVueRazorDocumentSet.Create(
            [
                new Jazor.RazorVue.RazorVueRazorDocument(documentPath, SourceText.From(documentText))
            ]));
        Assert.IsNotNull(context);

        var snapshot = RazorVueRazorDocumentSemanticFrontend.Instance.CreateSemanticSnapshots(context).Single(static item => item.Descriptor.Name == "TodoApp");
        TestContext.WriteLine("Resolved RazorDocumentPath: " + (snapshot.RazorDocumentPath ?? "<null>"));

        Assert.IsNotNull(snapshot.BuildRenderTreeMethod);
        foreach (var syntaxReference in snapshot.BuildRenderTreeMethod.DeclaringSyntaxReferences)
        {
            if (syntaxReference.GetSyntax() is not MethodDeclarationSyntax methodSyntax)
                continue;

            foreach (var nodeOrToken in methodSyntax.Body!.DescendantNodesAndTokensAndSelf())
            {
                var mapped = nodeOrToken.GetLocation().GetMappedLineSpan();
                if (!mapped.HasMappedPath)
                    continue;

                TestContext.WriteLine(nodeOrToken.Kind() + " => " + mapped.Path);
            }
        }
    }

    [TestMethod]
    public void ProcessDesignTime_ForBindDirectiveAttribute_WithOfficialSourceGeneratorRegistration_CanInspectGeneratedCSharp()
    {
        const string documentPath = @"D:\repo\Demo\Pages\TodoApp.razor";
        const string documentText = """<input @bind="Title" />""";

        var parseOptions = new CSharpParseOptions(LanguageVersion.Preview);
        var baseCompilation = CSharpCompilation.Create(
            "RazorVue.RazorIr.BindPathProbe.OfficialRegistration",
            syntaxTrees:
            [
                CSharpSyntaxTree.ParseText(
                    """
                    global using static ECMAScript.Vue3;
                    global using ECMAScript.VueContract;
                    global using Microsoft.AspNetCore.Components;
                    """,
                    options: parseOptions,
                    path: "RazorVueTestGlobalUsings.g.cs"),
                CSharpSyntaxTree.ParseText(
                    """
                    using System;

                    namespace ECMAScript
                    {
                        [AttributeUsage(AttributeTargets.Class, Inherited = false)]
                        public sealed class ECMAScriptModuleAttribute : Attribute
                        {
                            public ECMAScriptModuleAttribute() { }
                            public ECMAScriptModuleAttribute(string import) { }
                        }
                    }

                    namespace Demo.Pages
                    {
                        [ECMAScript.ECMAScriptModule("./components/todo-app")]
                        public partial class TodoApp : ComponentBase, IVueComponent
                        {
                            [Parameter]
                            public string? Title { get; set; }
                        }
                    }
                    """,
                    options: parseOptions,
                    path: "TodoApp.razor.cs")
            ],
            references: RazorIrTestHost.CreateMetadataReferences(),
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        var projectEngine = RazorIrTestHost.CreateProjectEngineWithOfficialSourceGeneratorRegistration(documentPath);
        var sourceDocument = RazorSourceDocument.Create(documentText, documentPath);
        var tagHelpers = RazorIrTestHost.DiscoverTagHelpers(projectEngine, baseCompilation);
        var codeDocument = projectEngine.ProcessDesignTime(
            sourceDocument,
            RazorFileKind.Component,
            [],
            tagHelpers.Length == 0 ? null : TagHelperCollection.Create(tagHelpers));
        var csharpDocument = RazorVueRazorCodeDocumentProvider.GetRequiredCSharpDocument(codeDocument);

        TestContext.WriteLine("Official registration generated C#:");
        TestContext.WriteLine(csharpDocument.Text.ToString());
    }

    [TestMethod]
    public void ProcessDesignTime_ForComponentBindDirectiveAttribute_CanInspectGeneratedCSharpAndTree()
    {
        const string documentPath = @"D:\repo\Demo\Pages\TodoApp.razor";
        const string documentText = """<EditorCard @bind-Value="Title" />""";

        var parseOptions = new CSharpParseOptions(LanguageVersion.Preview);
        var baseCompilation = CSharpCompilation.Create(
            "RazorVue.RazorIr.ComponentBindProbe",
            syntaxTrees:
            [
                CSharpSyntaxTree.ParseText(
                    """
                    global using static ECMAScript.Vue3;
                    global using ECMAScript.VueContract;
                    global using Microsoft.AspNetCore.Components;
                    """,
                    options: parseOptions,
                    path: "RazorVueTestGlobalUsings.g.cs"),
                CSharpSyntaxTree.ParseText(
                    """
                    using System;

                    namespace ECMAScript
                    {
                        [AttributeUsage(AttributeTargets.Class, Inherited = false)]
                        public sealed class ECMAScriptModuleAttribute : Attribute
                        {
                            public ECMAScriptModuleAttribute() { }
                            public ECMAScriptModuleAttribute(string import) { }
                        }
                    }

                    namespace Demo.Pages
                    {
                        [ECMAScript.ECMAScriptModule("./components/editor-card")]
                        public partial class EditorCard : ComponentBase, IVueComponent
                        {
                            [Parameter]
                            public string? Value { get; set; }

                            [Parameter]
                            public EventCallback<string?> ValueChanged { get; set; }
                        }

                        [ECMAScript.ECMAScriptModule("./components/todo-app")]
                        public partial class TodoApp : ComponentBase, IVueComponent
                        {
                            [Parameter]
                            public string? Title { get; set; }
                        }
                    }
                    """,
                    options: parseOptions,
                    path: "TodoApp.razor.cs")
            ],
            references: RazorIrTestHost.CreateMetadataReferences(),
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        var projectEngine = RazorVueRazorCodeDocumentProvider.CreateProjectEngine(
            documentPath,
            parseOptions,
            rootNamespace: "Demo.Pages");
        var tagHelpers = RazorVueRazorCodeDocumentProvider.DiscoverTagHelpers(projectEngine, baseCompilation);
        var codeDocument = projectEngine.Process(
            RazorVueRazorCodeDocumentProvider.CreateSourceDocument(
                new Jazor.RazorVue.RazorVueRazorDocument(documentPath, SourceText.From(documentText))),
            RazorFileKind.Component,
            [],
            tagHelpers.Length == 0 ? null : TagHelperCollection.Create(tagHelpers));
        var csharpDocument = RazorVueRazorCodeDocumentProvider.GetRequiredCSharpDocument(codeDocument);
        var documentNode = RazorVueRazorCodeDocumentProvider.GetDocumentNode(codeDocument);

        TestContext.WriteLine("Component bind generated C#:");
        TestContext.WriteLine(csharpDocument.Text.ToString());
        TestContext.WriteLine("Component bind tree:");
        TestContext.WriteLine(RazorIrTestHost.DumpIntermediateNodeTree(documentNode));
    }

    [TestMethod]
    public void CreateProjectEngine_MinimalHost_CanDumpTagHelperFeatureAndDiscoveryMethodSurface()
    {
        var projectEngine = RazorIrTestHost.CreateProjectEngine(@"D:\temp\FeatureSurfaceProbe.jazor");
        var features = RazorIrTestHost.GetEngineFeatures(projectEngine);
        var tagHelperFeatures = features
            .Where(static feature => feature.GetType().FullName?.Contains("TagHelper", StringComparison.Ordinal) == true)
            .OrderBy(static feature => feature.GetType().FullName, StringComparer.Ordinal)
            .ToArray();

        Assert.IsTrue(tagHelperFeatures.Length > 0, "No tag-helper-related engine features were exposed.");

        foreach (var feature in tagHelperFeatures)
        {
            DumpFeatureSurface(feature);
        }
    }

    [TestMethod]
    public void CreateProjectEngine_MinimalHost_CanDumpTagHelperDiscoveryServicePrivateFieldSurface()
    {
        var projectEngine = RazorIrTestHost.CreateProjectEngine(@"D:\temp\FeatureSurfaceProbePrivate.jazor");
        var discoveryService = RazorIrTestHost.GetTagHelperDiscoveryService(projectEngine);
        var fieldSurface = RazorIrTestHost.DumpObjectFieldSurface(discoveryService);

        TestContext.WriteLine(fieldSurface);

        Assert.IsTrue(
            fieldSurface.Contains(Environment.NewLine + "  ", StringComparison.Ordinal),
            "The discovery service field surface was unexpectedly empty.");
    }

    [TestMethod]
    public void CreateProjectEngine_MinimalHost_CanInventoryTagHelperProducerFactoryTypes()
    {
        var projectEngine = RazorIrTestHost.CreateProjectEngine(@"D:\temp\ProducerFactoryInventory.jazor");
        var factoryTypeNames = RazorIrTestHost.GetTagHelperProducerFactoryTypeNames(projectEngine);

        foreach (var factoryTypeName in factoryTypeNames)
        {
            TestContext.WriteLine(factoryTypeName);
        }

        Assert.AreEqual(
            0,
            factoryTypeNames.Length,
            "The minimal host unexpectedly started exposing tag-helper producer factories. Re-check whether the discovery boundary test should now be upgraded.");
    }

    [TestMethod]
    public void ProcessDesignTime_WithCurrentIsolatedDiscovery_DoesNotProduceComponentTagHelpersYet()
    {
        var toolset = RazorSdkToolsetProbeResolver.Resolve();
        Assert.IsNotNull(toolset, "The independent Razor IR spike could not locate a usable Razor SDK toolset.");

        var loadedAssemblyPath = RazorIrTestHost.GetLoadedRazorCompilerAssemblyPath();
        var loadedAssemblyHash = RazorIrTestHost.ComputeFileSha256(loadedAssemblyPath);
        var resolvedSdkAssemblyHash = RazorIrTestHost.ComputeFileSha256(toolset.RazorSourceGeneratorPath);

        TestContext.WriteLine("Loaded compiler assembly: " + loadedAssemblyPath);
        TestContext.WriteLine("Resolved SDK assembly:   " + toolset.RazorSourceGeneratorPath);
        TestContext.WriteLine("Loaded hash:             " + loadedAssemblyHash);
        TestContext.WriteLine("Resolved SDK hash:       " + resolvedSdkAssemblyHash);

        Assert.AreEqual(
            resolvedSdkAssemblyHash,
            loadedAssemblyHash,
            "The minimal host is no longer running against the same Razor compiler binary as the resolved SDK source generator path. Re-check SDK alignment before interpreting the discovery result.");

        var projectEngine = RazorIrTestHost.CreateProjectEngine(@"D:\temp\ComponentAwareProbe.jazor");
        var compilation = RazorIrTestHost.CreateCompilation(
            assemblyName: "RazorIr.ComponentDiscovery.Spike",
            """
            using Microsoft.AspNetCore.Components;

            namespace Probe.Components;

            public sealed class CounterCard : ComponentBase
            {
                [Parameter]
                public string? Title { get; set; }

                [Parameter]
                public RenderFragment? ChildContent { get; set; }
            }
            """);
        var compilationErrors = RazorIrTestHost.GetCompilationErrors(compilation);
        foreach (var compilationError in compilationErrors)
        {
            TestContext.WriteLine("COMPILATION ERROR: " + compilationError);
        }

        Assert.AreEqual(
            0,
            compilationErrors.Length,
            "The Roslyn compilation used for tag-helper discovery must be error-free before interpreting discovery results.");

        var discoveredTagHelpers = RazorIrTestHost.DiscoverTagHelpers(projectEngine, compilation);
        foreach (var descriptor in discoveredTagHelpers)
        {
            TestContext.WriteLine(RazorIrTestHost.DescribeTagHelper(descriptor));
        }

        Assert.AreEqual(
            0,
            discoveredTagHelpers.Length,
            "The current isolated discovery spike unexpectedly started producing component tag helpers. Re-check whether the test should now be upgraded into a real component-aware host path.");

        var codeDocument = RazorIrTestHost.CreateCodeDocument(
            @"D:\temp\ComponentAwareProbe.jazor",
            """
            <CounterCard Title="@message">
                <p>Body</p>
            </CounterCard>

            @code {
                private string message = "hello";
            }
            """,
            importSources: [],
            tagHelpers: discoveredTagHelpers);
        var documentNode = RazorIrTestHost.GetDocumentNode(codeDocument);
        var tree = RazorIrTestHost.DumpIntermediateNodeTree(documentNode);

        TestContext.WriteLine(tree);

        StringAssert.Contains(tree, "MarkupElementIntermediateNode TagName=\"CounterCard\"");
        Assert.IsFalse(
            tree.Contains("TagHelper", StringComparison.Ordinal),
            "The isolated spike unexpectedly produced tag-helper-oriented nodes. Re-check whether the boundary test should be upgraded.");
    }

    [TestMethod]
    public void ProcessDesignTime_ComponentDiscoverySpike_CanCompareDiscoveryEntryPoints()
    {
        var projectEngine = RazorIrTestHost.CreateProjectEngine(@"D:\temp\ComponentAwareProbe2.jazor");
        var compilation = RazorIrTestHost.CreateCompilation(
            assemblyName: "RazorIr.ComponentDiscovery.Spike.Compare",
            """
            using Microsoft.AspNetCore.Components;

            namespace Probe.Components;

            public sealed class CounterCard : ComponentBase
            {
                [Parameter]
                public string? Title { get; set; }
            }
            """);

        var errors = RazorIrTestHost.GetCompilationErrors(compilation);
        foreach (var error in errors)
        {
            TestContext.WriteLine("COMPILATION ERROR: " + error);
        }

        Assert.AreEqual(0, errors.Length, "Discovery comparison requires a compilable Roslyn input.");

        var discoveredByGetTagHelpers = RazorIrTestHost.DiscoverTagHelpers(projectEngine, compilation);
        var discoveredByGetTagHelpersForCompilation = RazorIrTestHost.DiscoverTagHelpersForCompilation(projectEngine, compilation);

        TestContext.WriteLine("GetTagHelpers count: " + discoveredByGetTagHelpers.Length);
        foreach (var descriptor in discoveredByGetTagHelpers)
        {
            TestContext.WriteLine("  TH: " + RazorIrTestHost.DescribeTagHelper(descriptor));
        }

        TestContext.WriteLine("GetTagHelpersForCompilation count: " + discoveredByGetTagHelpersForCompilation.Length);
        foreach (var descriptor in discoveredByGetTagHelpersForCompilation)
        {
            TestContext.WriteLine("  THFC: " + RazorIrTestHost.DescribeTagHelper(descriptor));
        }
    }

    [TestMethod]
    public void ProcessDesignTime_ComponentDiscoverySpike_CanInspectDiscoveryOptionsAndProducerCounts()
    {
        var projectEngine = RazorIrTestHost.CreateProjectEngine(@"D:\temp\ComponentAwareProbe3.jazor");
        var compilation = RazorIrTestHost.CreateCompilation(
            assemblyName: "RazorIr.ComponentDiscovery.Spike.Options",
            """
            using Microsoft.AspNetCore.Components;

            namespace Probe.Components;

            public sealed class CounterCard : ComponentBase
            {
                [Parameter]
                public string? Title { get; set; }
            }
            """);

        var options = RazorIrTestHost.CreateDefaultTagHelperDiscoveryOptions(projectEngine);
        TestContext.WriteLine(RazorIrTestHost.DumpObjectSurface(options));

        var producers = RazorIrTestHost.GetTagHelperProducers(projectEngine, compilation);
        TestContext.WriteLine("Producer count: " + producers.Length);
        foreach (var producer in producers)
        {
            TestContext.WriteLine("  PRODUCER: " + producer.GetType().FullName);
        }
    }

    [TestMethod]
    public void ProcessDesignTime_ComponentDiscoverySpike_CanTryClassicTagHelperDiscovery()
    {
        var projectEngine = RazorIrTestHost.CreateProjectEngine(@"D:\temp\ClassicTagHelperProbe.cshtml");
        var compilation = RazorIrTestHost.CreateCompilation(
            assemblyName: "RazorIr.ClassicTagHelperDiscovery.Spike",
            """
            using System;
            using System.Threading.Tasks;
            using Microsoft.AspNetCore.Razor.TagHelpers;

            namespace Probe.TagHelpers;

            [HtmlTargetElement("demo-card")]
            public sealed class DemoCardTagHelper : TagHelper
            {
                public string? Title { get; set; }

                public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
                    => Task.CompletedTask;
            }
            """);

        var errors = RazorIrTestHost.GetCompilationErrors(compilation);
        foreach (var error in errors)
        {
            TestContext.WriteLine("COMPILATION ERROR: " + error);
        }

        Assert.AreEqual(0, errors.Length, "Classic tag-helper discovery comparison requires a compilable Roslyn input.");

        var discoveredByGetTagHelpers = RazorIrTestHost.DiscoverTagHelpers(projectEngine, compilation);
        var discoveredByGetTagHelpersForCompilation = RazorIrTestHost.DiscoverTagHelpersForCompilation(projectEngine, compilation);

        TestContext.WriteLine("Classic GetTagHelpers count: " + discoveredByGetTagHelpers.Length);
        foreach (var descriptor in discoveredByGetTagHelpers)
        {
            TestContext.WriteLine("  CLASSIC TH: " + RazorIrTestHost.DescribeTagHelper(descriptor));
        }

        TestContext.WriteLine("Classic GetTagHelpersForCompilation count: " + discoveredByGetTagHelpersForCompilation.Length);
        foreach (var descriptor in discoveredByGetTagHelpersForCompilation)
        {
            TestContext.WriteLine("  CLASSIC THFC: " + RazorIrTestHost.DescribeTagHelper(descriptor));
        }
    }

    [TestMethod]
    public void ProcessDesignTime_ComponentDiscoverySpike_CanCompareCurrentCompilationAndReferencedAssemblyDiscovery()
    {
        var projectEngine = RazorIrTestHost.CreateProjectEngine(@"D:\temp\ReferencedAssemblyProbe.jazor");

        var componentLibrary = RazorIrTestHost.CreateCompilation(
            assemblyName: "RazorIr.ComponentLibrary",
            """
            using Microsoft.AspNetCore.Components;

            namespace Referenced.Components;

            public sealed class CounterCard : ComponentBase
            {
                [Parameter]
                public string? Title { get; set; }
            }
            """);
        var componentLibraryErrors = RazorIrTestHost.GetCompilationErrors(componentLibrary);
        foreach (var error in componentLibraryErrors)
        {
            TestContext.WriteLine("COMPONENT LIB ERROR: " + error);
        }

        Assert.AreEqual(0, componentLibraryErrors.Length, "The referenced component library must compile before discovery comparison.");

        var hostCompilationWithReferencedComponent = CSharpCompilation.Create(
            assemblyName: "RazorIr.ComponentHost",
            syntaxTrees:
            [
                CSharpSyntaxTree.ParseText(
                    """
                    namespace Host;

                    public sealed class Marker
                    {
                    }
                    """)
            ],
            references: RazorIrTestHost.CreateMetadataReferences()
                .Append(RazorIrTestHost.EmitToMetadataReference(componentLibrary)),
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        var hostComponentErrors = RazorIrTestHost.GetCompilationErrors(hostCompilationWithReferencedComponent);
        foreach (var error in hostComponentErrors)
        {
            TestContext.WriteLine("COMPONENT HOST ERROR: " + error);
        }

        Assert.AreEqual(0, hostComponentErrors.Length, "The host compilation for referenced component discovery must compile.");

        var discoveredReferencedComponents = RazorIrTestHost.DiscoverTagHelpers(projectEngine, hostCompilationWithReferencedComponent);
        TestContext.WriteLine("Referenced component descriptors: " + discoveredReferencedComponents.Length);
        foreach (var descriptor in discoveredReferencedComponents)
        {
            TestContext.WriteLine("  REF COMPONENT: " + RazorIrTestHost.DescribeTagHelper(descriptor));
        }

        var classicTagHelperLibrary = RazorIrTestHost.CreateCompilation(
            assemblyName: "RazorIr.ClassicTagHelperLibrary",
            """
            using System.Threading.Tasks;
            using Microsoft.AspNetCore.Razor.TagHelpers;

            namespace Referenced.TagHelpers;

            [HtmlTargetElement("demo-card")]
            public sealed class DemoCardTagHelper : TagHelper
            {
                public string? Title { get; set; }

                public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
                    => Task.CompletedTask;
            }
            """);
        var classicLibraryErrors = RazorIrTestHost.GetCompilationErrors(classicTagHelperLibrary);
        foreach (var error in classicLibraryErrors)
        {
            TestContext.WriteLine("CLASSIC LIB ERROR: " + error);
        }

        Assert.AreEqual(0, classicLibraryErrors.Length, "The referenced classic tag-helper library must compile before discovery comparison.");

        var hostCompilationWithReferencedClassicTagHelper = CSharpCompilation.Create(
            assemblyName: "RazorIr.ClassicTagHelperHost",
            syntaxTrees:
            [
                CSharpSyntaxTree.ParseText(
                    """
                    namespace Host;

                    public sealed class Marker
                    {
                    }
                    """)
            ],
            references: RazorIrTestHost.CreateMetadataReferences()
                .Append(RazorIrTestHost.EmitToMetadataReference(classicTagHelperLibrary)),
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        var hostClassicErrors = RazorIrTestHost.GetCompilationErrors(hostCompilationWithReferencedClassicTagHelper);
        foreach (var error in hostClassicErrors)
        {
            TestContext.WriteLine("CLASSIC HOST ERROR: " + error);
        }

        Assert.AreEqual(0, hostClassicErrors.Length, "The host compilation for referenced classic tag-helper discovery must compile.");

        var discoveredReferencedClassicTagHelpers = RazorIrTestHost.DiscoverTagHelpers(projectEngine, hostCompilationWithReferencedClassicTagHelper);
        TestContext.WriteLine("Referenced classic tag-helper descriptors: " + discoveredReferencedClassicTagHelpers.Length);
        foreach (var descriptor in discoveredReferencedClassicTagHelpers)
        {
            TestContext.WriteLine("  REF CLASSIC: " + RazorIrTestHost.DescribeTagHelper(descriptor));
        }
    }

    [TestMethod]
    public void ProcessDesignTime_ComponentDiscoverySpike_CanInspectTryGetDiscovererResultForDifferentCompilationShapes()
    {
        var projectEngine = RazorIrTestHost.CreateProjectEngine(@"D:\temp\DiscovererProbe.jazor");

        var inlineComponentCompilation = RazorIrTestHost.CreateCompilation(
            assemblyName: "RazorIr.InlineComponentDiscovererProbe",
            """
            using Microsoft.AspNetCore.Components;

            namespace Probe.Components;

            public sealed class CounterCard : ComponentBase
            {
                [Parameter]
                public string? Title { get; set; }
            }
            """);

        var inlineErrors = RazorIrTestHost.GetCompilationErrors(inlineComponentCompilation);
        foreach (var error in inlineErrors)
        {
            TestContext.WriteLine("INLINE ERROR: " + error);
        }

        Assert.AreEqual(0, inlineErrors.Length, "Inline component discoverer probe requires a compilable Roslyn input.");

        var inlineTryGetDiscoverer = RazorIrTestHost.InvokeDiscoveryMethod(
            projectEngine,
            "TryGetDiscoverer",
            inlineComponentCompilation,
            null!);
        TestContext.WriteLine("Inline TryGetDiscoverer result: " + (inlineTryGetDiscoverer?.ToString() ?? "<null>"));

        var referencedComponentLibrary = RazorIrTestHost.CreateCompilation(
            assemblyName: "RazorIr.ReferencedComponentDiscovererLibrary",
            """
            using Microsoft.AspNetCore.Components;

            namespace Referenced.Components;

            public sealed class CounterCard : ComponentBase
            {
                [Parameter]
                public string? Title { get; set; }
            }
            """);
        Assert.AreEqual(0, RazorIrTestHost.GetCompilationErrors(referencedComponentLibrary).Length);

        var hostCompilation = CSharpCompilation.Create(
            assemblyName: "RazorIr.DiscovererHost",
            syntaxTrees:
            [
                CSharpSyntaxTree.ParseText(
                    """
                    namespace Host;

                    public sealed class Marker
                    {
                    }
                    """)
            ],
            references: RazorIrTestHost.CreateMetadataReferences()
                .Append(RazorIrTestHost.EmitToMetadataReference(referencedComponentLibrary)),
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        var hostErrors = RazorIrTestHost.GetCompilationErrors(hostCompilation);
        foreach (var error in hostErrors)
        {
            TestContext.WriteLine("HOST ERROR: " + error);
        }

        Assert.AreEqual(0, hostErrors.Length, "Referenced host discoverer probe requires a compilable Roslyn input.");

        var hostTryGetDiscoverer = RazorIrTestHost.InvokeDiscoveryMethod(
            projectEngine,
            "TryGetDiscoverer",
            hostCompilation,
            null!);
        TestContext.WriteLine("Referenced-host TryGetDiscoverer result: " + (hostTryGetDiscoverer?.ToString() ?? "<null>"));
    }

    [TestMethod]
    public void ProcessDesignTime_ComponentDiscoverySpike_CanInspectRequiredMetadataTypesForProducerFactories()
    {
        var componentCompilation = RazorIrTestHost.CreateCompilation(
            assemblyName: "RazorIr.RequiredMetadata.Component",
            """
            using Microsoft.AspNetCore.Components;

            namespace Probe.Components;

            public sealed class CounterCard : ComponentBase
            {
                [Parameter]
                public string? Title { get; set; }
            }
            """);
        Assert.AreEqual(0, RazorIrTestHost.GetCompilationErrors(componentCompilation).Length);

        DumpMetadataPresence(componentCompilation, "inline-component");

        var classicTagHelperCompilation = RazorIrTestHost.CreateCompilation(
            assemblyName: "RazorIr.RequiredMetadata.Classic",
            """
            using System.Threading.Tasks;
            using Microsoft.AspNetCore.Razor.TagHelpers;

            namespace Probe.TagHelpers;

            [HtmlTargetElement("demo-card")]
            public sealed class DemoCardTagHelper : TagHelper
            {
                public string? Title { get; set; }

                public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
                    => Task.CompletedTask;
            }
            """);
        Assert.AreEqual(0, RazorIrTestHost.GetCompilationErrors(classicTagHelperCompilation).Length);

        DumpMetadataPresence(classicTagHelperCompilation, "classic-taghelper");
    }

    [TestMethod]
    public void CreateProjectEngine_WithExplicitProducerFactories_ExposesFactoryInventory()
    {
        var projectEngine = RazorIrTestHost.CreateProjectEngineWithExplicitProducerFactories(@"D:\temp\ExplicitProducerFactories.jazor");
        var factoryTypeNames = RazorIrTestHost.GetTagHelperProducerFactoryTypeNames(projectEngine);

        foreach (var factoryTypeName in factoryTypeNames)
        {
            TestContext.WriteLine(factoryTypeName);
        }

        Assert.IsTrue(
            factoryTypeNames.Length > 0,
            "The explicit-producer-factory host did not expose any tag-helper producer factories.");
    }

    [TestMethod]
    public void CreateProjectEngine_WithOfficialCompilerFeatures_ExposesComponentProducerFactoryInventory()
    {
        var projectEngine = RazorIrTestHost.CreateProjectEngineWithOfficialCompilerFeatures(@"D:\temp\OfficialCompilerFeatures.jazor");
        var factoryTypeNames = RazorIrTestHost.GetTagHelperProducerFactoryTypeNames(projectEngine);

        foreach (var factoryTypeName in factoryTypeNames)
        {
            TestContext.WriteLine(factoryTypeName);
        }

        Assert.IsTrue(
            factoryTypeNames.Any(static name => name.Contains("ComponentTagHelperProducer+Factory", StringComparison.Ordinal)),
            "CompilerFeatures.Register(builder) did not expose the component producer factory from the official Razor compiler registration path.");
        Assert.IsFalse(
            factoryTypeNames.Any(static name => name.Contains("DefaultTagHelperProducer+Factory", StringComparison.Ordinal)),
            "CompilerFeatures.Register(builder) unexpectedly exposed the default MVC tag-helper producer. Re-check whether this test host still reflects the official compiler-features-only slice.");
    }

    [TestMethod]
    public void ProcessDesignTime_ComponentDiscoverySpike_WithExplicitProducerFactories_CanBuildDiscoverer()
    {
        var projectEngine = RazorIrTestHost.CreateProjectEngineWithExplicitProducerFactories(@"D:\temp\ExplicitDiscovererProbe.jazor");
        var compilation = RazorIrTestHost.CreateCompilation(
            assemblyName: "RazorIr.ExplicitDiscovererProbe",
            """
            using Microsoft.AspNetCore.Components;

            namespace Probe.Components;

            public sealed class CounterCard : ComponentBase
            {
                [Parameter]
                public string? Title { get; set; }
            }
            """);

        var errors = RazorIrTestHost.GetCompilationErrors(compilation);
        foreach (var error in errors)
        {
            TestContext.WriteLine("COMPILATION ERROR: " + error);
        }

        Assert.AreEqual(0, errors.Length, "Explicit producer factory discoverer probe requires a compilable Roslyn input.");

        var tryGetDiscovererResult = RazorIrTestHost.InvokeDiscoveryMethod(
            projectEngine,
            "TryGetDiscoverer",
            compilation,
            null!);

        TestContext.WriteLine("Explicit factory TryGetDiscoverer result: " + (tryGetDiscovererResult?.ToString() ?? "<null>"));

        Assert.AreEqual(
            true,
            tryGetDiscovererResult,
            "With the official producer factories injected, the discovery service still could not create a discoverer. Re-check whether more upstream SDK initialization is still missing.");
    }

    [TestMethod]
    public void ProcessDesignTime_ComponentDiscoverySpike_WithOfficialCompilerFeatures_CanBuildDiscoverer()
    {
        var projectEngine = RazorIrTestHost.CreateProjectEngineWithOfficialCompilerFeatures(@"D:\temp\OfficialCompilerFeaturesDiscovererProbe.jazor");
        var compilation = RazorIrTestHost.CreateCompilation(
            assemblyName: "RazorIr.OfficialCompilerFeaturesDiscovererProbe",
            """
            using Microsoft.AspNetCore.Components;

            namespace Probe.Components;

            public sealed class CounterCard : ComponentBase
            {
                [Parameter]
                public string? Title { get; set; }
            }
            """);

        var errors = RazorIrTestHost.GetCompilationErrors(compilation);
        foreach (var error in errors)
        {
            TestContext.WriteLine("COMPILATION ERROR: " + error);
        }

        Assert.AreEqual(0, errors.Length, "Official CompilerFeatures discoverer probe requires a compilable Roslyn input.");

        var tryGetDiscovererResult = RazorIrTestHost.InvokeDiscoveryMethod(
            projectEngine,
            "TryGetDiscoverer",
            compilation,
            null!);

        TestContext.WriteLine("Official CompilerFeatures TryGetDiscoverer result: " + (tryGetDiscovererResult?.ToString() ?? "<null>"));

        Assert.AreEqual(
            true,
            tryGetDiscovererResult,
            "With CompilerFeatures.Register(builder) applied, the discovery service still could not create a discoverer. Re-check whether the test host diverged from the official Razor registration slice.");
    }

    [TestMethod]
    public void CreateProjectEngine_WithOfficialSourceGeneratorRegistration_ExposesDefaultAndComponentProducerFactories()
    {
        var projectEngine = RazorIrTestHost.CreateProjectEngineWithOfficialSourceGeneratorRegistration(@"D:\temp\OfficialSourceGeneratorRegistration.jazor");
        var factoryTypeNames = RazorIrTestHost.GetTagHelperProducerFactoryTypeNames(projectEngine);

        foreach (var factoryTypeName in factoryTypeNames)
        {
            TestContext.WriteLine(factoryTypeName);
        }

        Assert.IsTrue(
            factoryTypeNames.Any(static name => name.Contains("DefaultTagHelperProducer+Factory", StringComparison.Ordinal)),
            "The source-generator-aligned registration path did not expose the default tag-helper producer from RazorExtensions.Register(builder).");
        Assert.IsTrue(
            factoryTypeNames.Any(static name => name.Contains("ComponentTagHelperProducer+Factory", StringComparison.Ordinal)),
            "The source-generator-aligned registration path did not expose the component producer from CompilerFeatures.Register(builder).");
    }

    [TestMethod]
    public void ProcessDesignTime_ComponentDiscoverySpike_WithOfficialSourceGeneratorRegistration_CanBuildDiscoverer()
    {
        var projectEngine = RazorIrTestHost.CreateProjectEngineWithOfficialSourceGeneratorRegistration(@"D:\temp\OfficialSourceGeneratorDiscovererProbe.jazor");
        var compilation = RazorIrTestHost.CreateCompilation(
            assemblyName: "RazorIr.OfficialSourceGeneratorDiscovererProbe",
            """
            using Microsoft.AspNetCore.Components;

            namespace Probe.Components;

            public sealed class CounterCard : ComponentBase
            {
                [Parameter]
                public string? Title { get; set; }
            }
            """);

        var errors = RazorIrTestHost.GetCompilationErrors(compilation);
        foreach (var error in errors)
        {
            TestContext.WriteLine("COMPILATION ERROR: " + error);
        }

        Assert.AreEqual(0, errors.Length, "Official source-generator discoverer probe requires a compilable Roslyn input.");

        var tryGetDiscovererResult = RazorIrTestHost.InvokeDiscoveryMethod(
            projectEngine,
            "TryGetDiscoverer",
            compilation,
            null!);

        TestContext.WriteLine("Official source-generator TryGetDiscoverer result: " + (tryGetDiscovererResult?.ToString() ?? "<null>"));

        Assert.AreEqual(
            true,
            tryGetDiscovererResult,
            "With the source-generator-aligned registration path applied, the discovery service still could not create a discoverer. Re-check whether more SDK context than the documented registration slice is still required.");
    }

    [TestMethod]
    public void ProcessDesignTime_ComponentDiscoverySpike_WithOfficialCompilerFeatures_CanCompareDiscoveryEntryPoints()
    {
        var projectEngine = RazorIrTestHost.CreateProjectEngineWithOfficialCompilerFeatures(@"D:\temp\OfficialCompilerFeaturesCompare.jazor");
        var compilation = RazorIrTestHost.CreateCompilation(
            assemblyName: "RazorIr.OfficialCompilerFeaturesCompare",
            """
            using Microsoft.AspNetCore.Components;

            namespace Probe.Components;

            public sealed class CounterCard : ComponentBase
            {
                [Parameter]
                public string? Title { get; set; }
            }
            """);

        var errors = RazorIrTestHost.GetCompilationErrors(compilation);
        foreach (var error in errors)
        {
            TestContext.WriteLine("COMPILATION ERROR: " + error);
        }

        Assert.AreEqual(0, errors.Length, "Official CompilerFeatures discovery comparison requires a compilable Roslyn input.");

        var discoveredByGetTagHelpers = RazorIrTestHost.DiscoverTagHelpers(projectEngine, compilation);
        var discoveredByGetTagHelpersForCompilation = RazorIrTestHost.DiscoverTagHelpersForCompilation(projectEngine, compilation);

        TestContext.WriteLine("Official CompilerFeatures GetTagHelpers count: " + discoveredByGetTagHelpers.Length);
        foreach (var descriptor in discoveredByGetTagHelpers)
        {
            TestContext.WriteLine("  OCF TH: " + RazorIrTestHost.DescribeTagHelper(descriptor));
        }

        TestContext.WriteLine("Official CompilerFeatures GetTagHelpersForCompilation count: " + discoveredByGetTagHelpersForCompilation.Length);
        foreach (var descriptor in discoveredByGetTagHelpersForCompilation)
        {
            TestContext.WriteLine("  OCF THFC: " + RazorIrTestHost.DescribeTagHelper(descriptor));
        }

        Assert.IsTrue(
            discoveredByGetTagHelpers.Any(static descriptor => RazorIrTestHost.DescribeTagHelper(descriptor).Contains("Probe.Components.CounterCard", StringComparison.Ordinal)),
            "CompilerFeatures.Register(builder) produced a discoverer, but GetTagHelpers(...) still did not return the probe component descriptor.");
        Assert.IsTrue(
            discoveredByGetTagHelpersForCompilation.Any(static descriptor => RazorIrTestHost.DescribeTagHelper(descriptor).Contains("Probe.Components.CounterCard", StringComparison.Ordinal)),
            "CompilerFeatures.Register(builder) produced a discoverer, but GetTagHelpersForCompilation(...) still did not return the probe component descriptor.");
    }

    [TestMethod]
    public void ProcessDesignTime_ComponentDiscoverySpike_WithOfficialSourceGeneratorRegistration_CanCompareComponentAndClassicDiscovery()
    {
        var projectEngine = RazorIrTestHost.CreateProjectEngineWithOfficialSourceGeneratorRegistration(@"D:\temp\OfficialSourceGeneratorCompare.jazor");

        var componentCompilation = RazorIrTestHost.CreateCompilation(
            assemblyName: "RazorIr.OfficialSourceGeneratorCompare.Component",
            """
            using Microsoft.AspNetCore.Components;

            namespace Probe.Components;

            public sealed class CounterCard : ComponentBase
            {
                [Parameter]
                public string? Title { get; set; }
            }
            """);
        Assert.AreEqual(0, RazorIrTestHost.GetCompilationErrors(componentCompilation).Length);

        var classicCompilation = RazorIrTestHost.CreateCompilation(
            assemblyName: "RazorIr.OfficialSourceGeneratorCompare.Classic",
            """
            using System.Threading.Tasks;
            using Microsoft.AspNetCore.Razor.TagHelpers;

            namespace Probe.TagHelpers;

            [HtmlTargetElement("demo-card")]
            public sealed class DemoCardTagHelper : TagHelper
            {
                public string? Title { get; set; }

                public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
                    => Task.CompletedTask;
            }
            """);
        Assert.AreEqual(0, RazorIrTestHost.GetCompilationErrors(classicCompilation).Length);

        var componentTagHelpers = RazorIrTestHost.DiscoverTagHelpers(projectEngine, componentCompilation);
        TestContext.WriteLine("Official source-generator component count: " + componentTagHelpers.Length);
        foreach (var descriptor in componentTagHelpers)
        {
            TestContext.WriteLine("  OSG COMPONENT: " + RazorIrTestHost.DescribeTagHelper(descriptor));
        }

        var classicTagHelpers = RazorIrTestHost.DiscoverTagHelpers(projectEngine, classicCompilation);
        TestContext.WriteLine("Official source-generator classic count: " + classicTagHelpers.Length);
        foreach (var descriptor in classicTagHelpers)
        {
            TestContext.WriteLine("  OSG CLASSIC: " + RazorIrTestHost.DescribeTagHelper(descriptor));
        }

        Assert.IsTrue(
            componentTagHelpers.Any(static descriptor => RazorIrTestHost.DescribeTagHelper(descriptor).Contains("Probe.Components.CounterCard", StringComparison.Ordinal)),
            "The source-generator-aligned registration path did not return the probe component descriptor.");
        Assert.IsTrue(
            classicTagHelpers.Any(static descriptor => RazorIrTestHost.DescribeTagHelper(descriptor).Contains("Probe.TagHelpers.DemoCardTagHelper", StringComparison.Ordinal)),
            "The source-generator-aligned registration path did not return the probe classic tag-helper descriptor.");
    }

    private void DumpFeatureSurface(object feature)
    {
        var featureType = feature.GetType();
        TestContext.WriteLine("FEATURE: " + (featureType.FullName ?? featureType.Name));

        foreach (var property in featureType
                     .GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                     .OrderBy(static property => property.Name, StringComparer.Ordinal))
        {
            TestContext.WriteLine(
                "  PROPERTY: "
                + property.PropertyType.FullName
                + " "
                + property.Name);
        }

        foreach (var method in featureType
                     .GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly)
                     .OrderBy(static method => method.Name, StringComparer.Ordinal))
        {
            if (method.IsSpecialName)
            {
                continue;
            }

            TestContext.WriteLine("  METHOD: " + FormatMethod(method));
        }
    }

    private static string FormatMethod(MethodInfo method)
    {
        var parameterText = string.Join(
            ", ",
            method.GetParameters().Select(static parameter =>
                (parameter.ParameterType.FullName ?? parameter.ParameterType.Name) + " " + parameter.Name));
        var returnTypeName = method.ReturnType.FullName ?? method.ReturnType.Name;
        return returnTypeName + " " + method.Name + "(" + parameterText + ")";
    }

    private void DumpMetadataPresence(Compilation compilation, string label)
    {
        var metadataNames = new[]
        {
            "Microsoft.AspNetCore.Razor.TagHelpers.ITagHelper",
            "Microsoft.AspNetCore.Components.IComponent",
            "Microsoft.AspNetCore.Components.BindConverter",
            "Microsoft.AspNetCore.Components.BindElementAttribute",
            "Microsoft.AspNetCore.Components.BindInputElementAttribute",
            "Microsoft.AspNetCore.Components.EventHandlerAttribute"
        };

        TestContext.WriteLine("Metadata inventory for " + label + ":");
        foreach (var metadataName in metadataNames)
        {
            var present = RazorIrTestHost.CompilationContainsMetadataType(compilation, metadataName);
            TestContext.WriteLine("  " + metadataName + " => " + present);
        }
    }
}
