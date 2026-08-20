using System.Collections.Immutable;
using Jazor.RazorVue.Authoring;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

namespace Jazor.RazorVue.Sg.Test;

[TestClass]
public sealed class RazorVueCompatibilityAnalyzerTests
{
    [TestMethod]
    public async Task InjectedDbContextProperty_ReportsAtAuthoredInjectAttribute()
    {
        var diagnostics = await AnalyzeAsync(
            new SourceFile(
                "Pages/Orders.razor.cs",
                """
                using Microsoft.AspNetCore.Components;

                namespace Microsoft.EntityFrameworkCore
                {
                    public abstract class DbContext;
                }

                namespace Demo.Pages
                {
                    public sealed class AppDbContext : Microsoft.EntityFrameworkCore.DbContext;

                    public sealed class Orders : ComponentBase
                    {
                        [Inject]
                        public AppDbContext Database { get; set; } = null!;
                    }
                }
                """));

        var diagnostic = AssertSingleDbContextDiagnostic(diagnostics);
        Assert.AreEqual("Pages/Orders.razor.cs", diagnostic.Location.GetLineSpan().Path);
        Assert.AreEqual(13, diagnostic.Location.GetLineSpan().StartLinePosition.Line);
        StringAssert.Contains(diagnostic.GetMessage(), "Demo.Pages.AppDbContext", StringComparison.Ordinal);
    }

    [TestMethod]
    public async Task RazorInjectDirective_ReportsAtOriginalRazorTypeSpan()
    {
        var diagnostics = await AnalyzeAsync(
            new SourceFile(
                "Data/AppDbContext.cs",
                """
                namespace Microsoft.EntityFrameworkCore
                {
                    public abstract class DbContext;
                }

                namespace Demo.Data
                {
                    public sealed class AppDbContext : Microsoft.EntityFrameworkCore.DbContext;
                }
                """),
            additionalFiles:
            [new InMemoryAdditionalText(
                "Pages/Orders.razor",
                """
                @inject Demo.Data.AppDbContext Database
                <p>Orders</p>
                """)]);

        var diagnostic = AssertSingleDbContextDiagnostic(diagnostics);
        Assert.AreEqual("Pages/Orders.razor", diagnostic.Location.GetLineSpan().Path);
        Assert.AreEqual(0, diagnostic.Location.GetLineSpan().StartLinePosition.Line);
        Assert.AreEqual(8, diagnostic.Location.GetLineSpan().StartLinePosition.Character);
        Assert.AreEqual(30, diagnostic.Location.GetLineSpan().EndLinePosition.Character);
    }

    [TestMethod]
    public async Task RazorInjectDirective_ResolvesUniqueSimpleSourceType()
    {
        var diagnostics = await AnalyzeAsync(
            new SourceFile(
                "Data/AppDbContext.cs",
                """
                namespace Microsoft.EntityFrameworkCore
                {
                    public abstract class DbContext;
                }

                namespace Demo.Data
                {
                    public sealed class AppDbContext : Microsoft.EntityFrameworkCore.DbContext;
                }
                """),
            additionalFiles:
            [new InMemoryAdditionalText(
                "Pages/Orders.razor",
                "@inject AppDbContext Database")]);

        var diagnostic = AssertSingleDbContextDiagnostic(diagnostics);
        Assert.AreEqual("Pages/Orders.razor", diagnostic.Location.GetLineSpan().Path);
    }

    [TestMethod]
    public async Task GeneratedRazorPropertyAndBrowserCapableService_ProduceNoNoise()
    {
        var diagnostics = await AnalyzeAsync(
            new SourceFile(
                "Pages/Generated.razor.g.cs",
                """
                using Microsoft.AspNetCore.Components;

                namespace Microsoft.EntityFrameworkCore
                {
                    public abstract class DbContext;
                }

                namespace Demo.Pages
                {
                    public sealed class AppDbContext : Microsoft.EntityFrameworkCore.DbContext;

                    public sealed class Generated : ComponentBase
                    {
                        [Inject]
                        public AppDbContext Database { get; set; } = null!;
                    }
                }
                """),
            new SourceFile(
                "Pages/BrowserComponent.razor.cs",
                """
                using Microsoft.AspNetCore.Components;

                namespace Demo.Pages
                {
                    public sealed class BrowserClient;

                    public sealed class BrowserComponent : ComponentBase
                    {
                        [Inject]
                        public BrowserClient Client { get; set; } = null!;
                    }
                }
                """));

        Assert.IsFalse(diagnostics.Any(static diagnostic => diagnostic.Id == "JAZORVCA001"),
            string.Join(Environment.NewLine, diagnostics));
    }

    [TestMethod]
    public async Task InjectedHttpContext_ReportsServerOnlyServiceAtAuthoredAttribute()
    {
        var diagnostics = await AnalyzeAsync(
            new SourceFile(
                "Pages/Request.razor.cs",
                """
                using Microsoft.AspNetCore.Components;

                namespace Microsoft.AspNetCore.Http
                {
                    public abstract class HttpContext;
                }

                namespace Demo.Pages
                {
                    public sealed class Request : ComponentBase
                    {
                        [Inject]
                        public Microsoft.AspNetCore.Http.HttpContext Context { get; set; } = null!;
                    }
                }
                """));

        var diagnostic = diagnostics.Single(static item => item.Id == "JAZORVCA002");
        Assert.AreEqual("Pages/Request.razor.cs", diagnostic.Location.GetLineSpan().Path);
        StringAssert.Contains(diagnostic.GetMessage(), "HttpContext", StringComparison.Ordinal);
        StringAssert.Contains(diagnostic.GetMessage(), "typed endpoint client", StringComparison.Ordinal);
    }

    [TestMethod]
    public async Task InjectedBlazorHostServiceWithoutAdapter_ReportsAtAuthoredInjectAttribute()
    {
        var diagnostics = await AnalyzeAsync(
            new SourceFile(
                "Pages/Circuit.razor.cs",
                """
                using Microsoft.AspNetCore.Components;

                namespace Demo.Pages
                {
                    public sealed class Circuit : ComponentBase
                    {
                        [Inject]
                        public IComponentActivator Activator { get; set; } = null!;
                    }
                }
                """));

        var diagnostic = diagnostics.Single(static item => item.Id == "JAZORVCA007");
        Assert.AreEqual("Pages/Circuit.razor.cs", diagnostic.Location.GetLineSpan().Path);
        StringAssert.Contains(diagnostic.GetMessage(), "IComponentActivator", StringComparison.Ordinal);
        StringAssert.Contains(diagnostic.GetMessage(), "browser adapter", StringComparison.Ordinal);
    }

    [TestMethod]
    public async Task InjectedNavigationManager_RemainsQuietAsBrowserAdapter()
    {
        var diagnostics = await AnalyzeAsync(
            new SourceFile(
                "Pages/Navigation.razor.cs",
                """
                using Microsoft.AspNetCore.Components;

                namespace Demo.Pages
                {
                    public sealed class Navigation : ComponentBase
                    {
                        [Inject]
                        public NavigationManager Manager { get; set; } = null!;
                    }
                }
                """));

        Assert.IsFalse(diagnostics.Any(static item => item.Id == "JAZORVCA007"),
            string.Join(Environment.NewLine, diagnostics));
    }

    [TestMethod]
    public async Task CascadingParameter_ReportsAtAuthoredAttribute()
    {
        var diagnostics = await AnalyzeAsync(
            new SourceFile(
                "Pages/Theme.razor.cs",
                """
                using Microsoft.AspNetCore.Components;

                namespace Demo.Pages
                {
                    public sealed class Theme : ComponentBase
                    {
                        [CascadingParameter]
                        public string Name => string.Empty;
                    }
                }
                """));

        var diagnostic = diagnostics.Single(static item => item.Id == "JAZORVCA008");
        Assert.AreEqual("Pages/Theme.razor.cs", diagnostic.Location.GetLineSpan().Path);
        StringAssert.Contains(diagnostic.GetMessage(), "Name", StringComparison.Ordinal);
    }

    [TestMethod]
    public async Task WritableCascadingParameter_IsHandledByBrowserAdapterWithoutDiagnostic()
    {
        var diagnostics = await AnalyzeAsync(
            new SourceFile(
                "Pages/Theme.razor.cs",
                """
                using Microsoft.AspNetCore.Components;

                namespace Demo.Pages
                {
                    public sealed class Theme : ComponentBase
                    {
                        [CascadingParameter]
                        public string Name { get; set; } = "default";
                    }
                }
                """));

        Assert.IsFalse(
            diagnostics.Any(static item => item.Id == "JAZORVCA008"),
            string.Join(Environment.NewLine, diagnostics));
    }

    [TestMethod]
    public async Task RazorPageDirective_RemainsQuietWhenRouteCatalogOwnsThePage()
    {
        var diagnostics = await AnalyzeAsync(
            new SourceFile("Pages/Orders.razor.cs", "public sealed class Orders;"),
            additionalFiles:
            [new InMemoryAdditionalText(
                "Pages/Orders.razor",
                "@page \"/orders/{id:int}\"\n<h1>Orders</h1>")]);

        Assert.IsFalse(diagnostics.Any(static item => item.Id == "JAZORVCA009"),
            string.Join(Environment.NewLine, diagnostics));
    }

    [TestMethod]
    public async Task StandardBlazorComponentTag_RemainsQuietWhenAdapterIsRegistered()
    {
        var diagnostics = await AnalyzeAsync(
            new SourceFile("Pages/Dynamic.razor.cs", "public sealed class Dynamic;"),
            additionalFiles:
            [new InMemoryAdditionalText(
                "Pages/Dynamic.razor",
                "<DynamicComponent Type=\"typeof(object)\" />")]);

        Assert.IsFalse(diagnostics.Any(static item => item.Id == "JAZORVCA010"),
            string.Join(Environment.NewLine, diagnostics));
    }

    [TestMethod]
    public async Task StandardGenericComponentTag_RemainsQuietWhenInputAdapterIsRegistered()
    {
        var diagnostics = await AnalyzeAsync(
            new SourceFile("Pages/Form.razor.cs", "public sealed class Form;"),
            additionalFiles:
            [new InMemoryAdditionalText(
                "Pages/Form.razor",
                "<InputText @bind-Value=\"Name\" />")]);

        Assert.IsFalse(diagnostics.Any(static item => item.Id == "JAZORVCA010"),
            string.Join(Environment.NewLine, diagnostics));
    }

    [TestMethod]
    public async Task StandardComponentTagInsideCommentsOrAttributeText_DoesNotReport()
    {
        var diagnostics = await AnalyzeAsync(
            new SourceFile("Pages/Markup.razor.cs", "public sealed class Markup;"),
            additionalFiles:
            [new InMemoryAdditionalText(
                "Pages/Markup.razor",
                "<!-- <DynamicComponent /> -->\n<div title=\"<EditForm>\">plain</div>\n@* <Router /> *@")]);

        Assert.IsFalse(diagnostics.Any(static item => item.Id == "JAZORVCA010"),
            string.Join(Environment.NewLine, diagnostics));
    }

    [TestMethod]
    public async Task AuthoredComponentWithFrameworkComponentName_DoesNotProduceFrameworkAdapterNoise()
    {
        var diagnostics = await AnalyzeAsync(
            new SourceFile(
                "Pages/DynamicComponent.razor.cs",
                """
                using Microsoft.AspNetCore.Components;

                namespace Demo.Pages
                {
                    public sealed class DynamicComponent : ComponentBase;
                }
                """),
            additionalFiles:
            [new InMemoryAdditionalText(
                "Pages/Host.razor",
                "<DynamicComponent />")]);

        Assert.IsFalse(diagnostics.Any(static item => item.Id == "JAZORVCA010"),
            string.Join(Environment.NewLine, diagnostics));
    }

    [TestMethod]
    public async Task InjectedBrowserServiceWithReadOnlyProperty_ReportsAtAuthoredInjectAttribute()
    {
        var diagnostics = await AnalyzeAsync(
            new SourceFile(
                "Pages/ReadOnlyService.razor.cs",
                """
                using Microsoft.AspNetCore.Components;

                namespace Demo.Pages
                {
                    public sealed class BrowserClient;

                    public sealed class ReadOnlyService : ComponentBase
                    {
                        [Inject]
                        public BrowserClient Client { get; } = null!;
                    }
                }
                """));

        var diagnostic = diagnostics.Single(static item => item.Id == "JAZORVCA006");
        Assert.AreEqual("Pages/ReadOnlyService.razor.cs", diagnostic.Location.GetLineSpan().Path);
        StringAssert.Contains(diagnostic.GetMessage(), "writable auto-property", StringComparison.Ordinal);
        StringAssert.Contains(diagnostic.GetMessage(), "Demo.Pages.BrowserClient", StringComparison.Ordinal);
    }

    [TestMethod]
    public async Task InjectedBrowserServiceWithCustomSetter_ReportsAtAuthoredInjectAttribute()
    {
        var diagnostics = await AnalyzeAsync(
            new SourceFile(
                "Pages/CustomSetterService.razor.cs",
                """
                using Microsoft.AspNetCore.Components;

                namespace Demo.Pages
                {
                    public sealed class BrowserClient;

                    public sealed class CustomSetterService : ComponentBase
                    {
                        private BrowserClient client = null!;

                        [Inject]
                        public BrowserClient Client
                        {
                            get => client;
                            set => client = value;
                        }
                    }
                }
                """));

        var diagnostic = diagnostics.Single(static item => item.Id == "JAZORVCA006");
        Assert.AreEqual("Pages/CustomSetterService.razor.cs", diagnostic.Location.GetLineSpan().Path);
    }

    [TestMethod]
    public async Task InjectedPropertyShapeRule_StillRunsWithoutOptionalServerMetadata()
    {
        var diagnostics = await AnalyzeAsync(
            new SourceFile(
                "Pages/Minimal.razor.cs",
                """
                using System;

                namespace Microsoft.AspNetCore.Components
                {
                    public abstract class ComponentBase;

                    [AttributeUsage(AttributeTargets.Property)]
                    public sealed class InjectAttribute : Attribute;
                }

                namespace Demo.Pages
                {
                    public sealed class Client;

                    public sealed class Minimal : Microsoft.AspNetCore.Components.ComponentBase
                    {
                        [Microsoft.AspNetCore.Components.Inject]
                        public Client Service { get; } = null!;
                    }
                }
                """),
            references: RazorSgTestHost.CreateMetadataReferences()
                .Where(static reference =>
                    !Path.GetFileName(reference.Display ?? string.Empty)
                        .StartsWith("Microsoft.AspNetCore", StringComparison.OrdinalIgnoreCase))
                .ToArray());

        var diagnostic = diagnostics.Single(static item => item.Id == "JAZORVCA006");
        Assert.AreEqual("Pages/Minimal.razor.cs", diagnostic.Location.GetLineSpan().Path);
    }

    [TestMethod]
    public async Task RazorInjectServerOnlyInterface_ReportsAtOriginalRazorTypeSpan()
    {
        var diagnostics = await AnalyzeAsync(
            new SourceFile(
                "Pages/Request.razor.cs",
                """
                namespace Microsoft.AspNetCore.Http
                {
                    public interface IHttpContextAccessor;
                }
                """),
            additionalFiles:
            [new InMemoryAdditionalText(
                "Pages/Request.razor",
                "@inject Microsoft.AspNetCore.Http.IHttpContextAccessor Accessor\n<p>Request</p>")]);

        var diagnostic = diagnostics.Single(static item => item.Id == "JAZORVCA002");
        Assert.AreEqual("Pages/Request.razor", diagnostic.Location.GetLineSpan().Path);
        Assert.AreEqual(8, diagnostic.Location.GetLineSpan().StartLinePosition.Character);
        StringAssert.Contains(diagnostic.GetMessage(), "IHttpContextAccessor", StringComparison.Ordinal);
    }

    [TestMethod]
    public async Task ParameterViewTryGetValue_ReportsAtAuthoredInvocation()
    {
        var diagnostics = await AnalyzeAsync(
            new SourceFile(
                "Pages/Parameters.razor.cs",
                """
                using Microsoft.AspNetCore.Components;

                namespace Demo.Pages
                {
                    public sealed class Parameters : ComponentBase
                    {
                        private ParameterView Values { get; set; }

                        private bool Read()
                            => Values.TryGetValue<string>("Name", out _);
                    }
                }
                """));

        var diagnostic = diagnostics.Single(static item => item.Id == "JAZORVCA003");
        Assert.AreEqual("Pages/Parameters.razor.cs", diagnostic.Location.GetLineSpan().Path);
        StringAssert.Contains(diagnostic.GetMessage(), "TryGetValue", StringComparison.Ordinal);
    }

    [TestMethod]
    public async Task ParameterViewEnumeration_ReportsAtAuthoredCollection()
    {
        var diagnostics = await AnalyzeAsync(
            new SourceFile(
                "Pages/Parameters.razor.cs",
                """
                using Microsoft.AspNetCore.Components;

                namespace Demo.Pages
                {
                    public sealed class Parameters : ComponentBase
                    {
                        private ParameterView Values { get; set; }

                        private void Read()
                        {
                            foreach (var value in Values)
                            {
                            }
                        }
                    }
                }
                """));

        var diagnostic = diagnostics.Single(static item => item.Id == "JAZORVCA004");
        Assert.AreEqual("Pages/Parameters.razor.cs", diagnostic.Location.GetLineSpan().Path);
        StringAssert.Contains(diagnostic.GetMessage(), "enumeration", StringComparison.OrdinalIgnoreCase);
    }

    [TestMethod]
    public async Task ParameterViewToDictionary_ReportsAtAuthoredInvocation()
    {
        var diagnostics = await AnalyzeAsync(
            new SourceFile(
                "Pages/Parameters.razor.cs",
                """
                using Microsoft.AspNetCore.Components;

                namespace Demo.Pages
                {
                    public sealed class Parameters : ComponentBase
                    {
                        private ParameterView Values { get; set; }

                        private object Read()
                            => Values.ToDictionary();
                    }
                }
                """));

        var diagnostic = diagnostics.Single(static item => item.Id == "JAZORVCA005");
        Assert.AreEqual("Pages/Parameters.razor.cs", diagnostic.Location.GetLineSpan().Path);
        StringAssert.Contains(diagnostic.GetMessage(), "ToDictionary", StringComparison.Ordinal);
    }

    [TestMethod]
    public void DescriptorContract_IsStableAndLinksToBrowserServicesGuidance()
    {
        var descriptor = RazorVueCompatibilityAnalyzer.BrowserIneligibleDbContext;

        Assert.AreEqual("JAZORVCA001", descriptor.Id);
        Assert.AreEqual(DiagnosticSeverity.Error, descriptor.DefaultSeverity);
        Assert.IsTrue(descriptor.HelpLinkUri.EndsWith("#browser-services", StringComparison.Ordinal));
        Assert.AreEqual("JAZORVCA002", RazorVueCompatibilityAnalyzer.BrowserIneligibleServerService.Id);
        Assert.AreEqual("JAZORVCA003", RazorVueCompatibilityAnalyzer.ParameterViewTryGetValueUnsupported.Id);
        Assert.AreEqual("JAZORVCA004", RazorVueCompatibilityAnalyzer.ParameterViewEnumerationUnsupported.Id);
        Assert.AreEqual("JAZORVCA005", RazorVueCompatibilityAnalyzer.ParameterViewToDictionaryUnsupported.Id);
        Assert.AreEqual("JAZORVCA006", RazorVueCompatibilityAnalyzer.InjectPropertyMustBeWritableAutoProperty.Id);
        Assert.AreEqual("JAZORVCA007", RazorVueCompatibilityAnalyzer.BrowserAdapterServiceUnavailable.Id);
        Assert.AreEqual("JAZORVCA008", RazorVueCompatibilityAnalyzer.CascadingParameterUnsupported.Id);
        Assert.IsTrue(RazorVueCompatibilityAnalyzer.CascadingParameterUnsupported.HelpLinkUri.EndsWith(
            "#cascading-parameters",
            StringComparison.Ordinal));
        Assert.AreEqual("JAZORVCA009", RazorVueCompatibilityAnalyzer.RouteDirectiveRequiresHostAdapter.Id);
        Assert.AreEqual("JAZORVCA010", RazorVueCompatibilityAnalyzer.BlazorComponentAdapterUnavailable.Id);
        Assert.HasCount(10, new RazorVueCompatibilityAnalyzer().SupportedDiagnostics);
    }

    private static RazorVueCompatibilityAnalyzerDiagnostic AssertSingleDbContextDiagnostic(
        ImmutableArray<Diagnostic> diagnostics)
    {
        var matches = diagnostics
            .Where(static diagnostic => diagnostic.Id == "JAZORVCA001")
            .ToArray();
        Assert.HasCount(1, matches, string.Join(Environment.NewLine, diagnostics));
        return new RazorVueCompatibilityAnalyzerDiagnostic(matches[0]);
    }

    private static async Task<ImmutableArray<Diagnostic>> AnalyzeAsync(
        SourceFile first,
        SourceFile? second = null,
        ImmutableArray<AdditionalText> additionalFiles = default,
        MetadataReference[]? references = null)
    {
        var sources = second is null ? [first] : new[] { first, second.Value };
        var compilation = CSharpCompilation.Create(
            assemblyName: "RazorVue.CompatibilityAnalyzer.Tests",
            syntaxTrees: sources.Select(source => CSharpSyntaxTree.ParseText(
                source.Text,
                new CSharpParseOptions(LanguageVersion.Preview),
                path: source.Path)),
            references: references ?? RazorSgTestHost.CreateMetadataReferences(),
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        var errors = RazorSgTestHost.GetCompilationErrors(compilation);
        Assert.IsEmpty(errors, string.Join(Environment.NewLine, errors));

        var options = new AnalyzerOptions(additionalFiles.IsDefault ? [] : additionalFiles);
        return await compilation
            .WithAnalyzers(
                ImmutableArray.Create<DiagnosticAnalyzer>(new RazorVueCompatibilityAnalyzer()),
                options)
            .GetAnalyzerDiagnosticsAsync();
    }

    private readonly record struct SourceFile(string Path, string Text);

    private sealed class InMemoryAdditionalText(string path, string text) : AdditionalText
    {
        private readonly SourceText _text = SourceText.From(text);

        public override string Path { get; } = path;

        public override SourceText GetText(CancellationToken cancellationToken = default)
            => _text;
    }

    private readonly record struct RazorVueCompatibilityAnalyzerDiagnostic(Diagnostic Value)
    {
        public Location Location => Value.Location;

        public string GetMessage() => Value.GetMessage();
    }
}
