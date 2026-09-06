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
    public async Task InjectedIJSRuntime_RemainsQuietUntilUsageSiteValidation()
    {
        var diagnostics = await AnalyzeAsync(
            new SourceFile(
                "Pages/Interop.razor.cs",
                """
                using Microsoft.AspNetCore.Components;

                namespace Microsoft.JSInterop
                {
                    public interface IJSRuntime;
                }

                namespace Demo.Pages
                {
                    public sealed class Interop : ComponentBase
                    {
                        [Inject]
                        public Microsoft.JSInterop.IJSRuntime Runtime { get; set; } = null!;
                    }
                }
                """));

        Assert.IsFalse(diagnostics.Any(static item => item.Id == "JAZORVCA007"),
            string.Join(Environment.NewLine, diagnostics));
    }

    [TestMethod]
    public async Task InjectedAuthenticationStateProvider_ReportsMissingExplicitBrowserProvider()
    {
        var diagnostics = await AnalyzeAsync(
            new SourceFile(
                "Pages/AuthState.razor.cs",
                """
                using Microsoft.AspNetCore.Components;
                using Microsoft.AspNetCore.Components.Authorization;

                namespace Demo.Pages
                {
                    public sealed class AuthState : ComponentBase
                    {
                        [Inject]
                        public AuthenticationStateProvider Provider { get; set; } = null!;
                    }
                }
                """));

        var diagnostic = diagnostics.Single(static item => item.Id == "JAZORVCA007");
        Assert.AreEqual("Pages/AuthState.razor.cs", diagnostic.Location.GetLineSpan().Path);
        StringAssert.Contains(diagnostic.GetMessage(), "AuthenticationStateProvider", StringComparison.Ordinal);
        StringAssert.Contains(diagnostic.GetMessage(), "Register a typed browser adapter", StringComparison.Ordinal);
    }

    [TestMethod]
    public async Task RazorAuthenticationStateProviderDirective_ReportsAtAuthoredTypeSpan()
    {
        var diagnostics = await AnalyzeAsync(
            new SourceFile(
                "Pages/AuthState.razor.cs",
                """
                using Microsoft.AspNetCore.Components;
                using Microsoft.AspNetCore.Components.Authorization;

                namespace Demo.Pages
                {
                    public sealed class AuthState : ComponentBase
                    {
                    }
                }
                """),
            additionalFiles:
            [
                new InMemoryAdditionalText(
                    "Pages/AuthState.razor",
                    "@inject Microsoft.AspNetCore.Components.Authorization.AuthenticationStateProvider Provider\n<p>auth</p>")
            ]);

        var diagnostic = diagnostics.Single(static item => item.Id == "JAZORVCA007");
        Assert.AreEqual("Pages/AuthState.razor", diagnostic.Location.GetLineSpan().Path);
        StringAssert.Contains(diagnostic.GetMessage(), "AuthenticationStateProvider", StringComparison.Ordinal);
    }

    [TestMethod]
    public async Task InjectedPersistentComponentState_ReportsExplicitSsrHandoffBoundary()
    {
        var diagnostics = await AnalyzeAsync(
            new SourceFile(
                "Pages/State.razor.cs",
                """
                using Microsoft.AspNetCore.Components;

                namespace Demo.Pages
                {
                    public sealed class State : ComponentBase
                    {
                        [Inject]
                        public PersistentComponentState StateStore { get; set; } = null!;
                    }
                }
                """));

        var diagnostic = diagnostics.Single(static item => item.Id == "JAZORVCA011");
        Assert.AreEqual("Pages/State.razor.cs", diagnostic.Location.GetLineSpan().Path);
        StringAssert.Contains(diagnostic.GetMessage(), "PersistentComponentState", StringComparison.Ordinal);
        StringAssert.Contains(diagnostic.GetMessage(), "typed endpoint/bootstrap payload", StringComparison.Ordinal);
    }

    [TestMethod]
    public async Task PersistentStateProperty_ReportsAtAuthoredAttribute()
    {
        var diagnostics = await AnalyzeAsync(
            new SourceFile(
                "Pages/State.razor.cs",
                """
                using Microsoft.AspNetCore.Components;

                namespace Demo.Pages
                {
                    public sealed class State : ComponentBase
                    {
                        [PersistentState]
                        public string? Snapshot { get; set; }
                    }
                }
                """));

        var diagnostic = diagnostics.Single(static item => item.Id == "JAZORVCA011");
        Assert.AreEqual("Pages/State.razor.cs", diagnostic.Location.GetLineSpan().Path);
        Assert.IsTrue(diagnostic.Location.GetLineSpan().StartLinePosition.Character > 0);
        StringAssert.Contains(diagnostic.GetMessage(), "PersistentState", StringComparison.Ordinal);
    }

    [TestMethod]
    public async Task StreamRenderingAttribute_ReportsAtAuthoredComponentAttribute()
    {
        var diagnostics = await AnalyzeAsync(
            new SourceFile(
                "Pages/Streaming.razor.cs",
                """
                using Microsoft.AspNetCore.Components;

                namespace Demo.Pages
                {
                    [StreamRendering(true)]
                    public sealed class Streaming : ComponentBase;
                }
                """));

        var diagnostic = diagnostics.Single(static item => item.Id == "JAZORVCA012");
        StringAssert.Contains(diagnostic.GetMessage(), "StreamRendering", StringComparison.Ordinal);
        Assert.AreEqual("Pages/Streaming.razor.cs", diagnostic.Location.GetLineSpan().Path);
    }

    [TestMethod]
    public async Task SupplyParameterFromFormProperty_ReportsExplicitSsrHandoffBoundary()
    {
        var diagnostics = await AnalyzeAsync(
            new SourceFile(
                "Pages/FormState.razor.cs",
                """
                using Microsoft.AspNetCore.Components;

                namespace Demo.Pages
                {
                    public sealed class FormState : ComponentBase
                    {
                        [SupplyParameterFromForm]
                        public string? Value { get; set; }
                    }
                }
                """));

        var diagnostic = diagnostics.Single(static item => item.Id == "JAZORVCA011");
        Assert.AreEqual("Pages/FormState.razor.cs", diagnostic.Location.GetLineSpan().Path);
        StringAssert.Contains(diagnostic.GetMessage(), "SupplyParameterFromForm", StringComparison.Ordinal);
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
    public async Task StandardBlazorComponentTag_ReportsUnsupportedBuiltInUi()
    {
        var diagnostics = await AnalyzeAsync(
            new SourceFile("Pages/Dynamic.razor.cs", "public sealed class Dynamic;"),
            additionalFiles:
            [new InMemoryAdditionalText(
                "Pages/Dynamic.razor",
                "<DynamicComponent Type=\"typeof(object)\" />")]);

        var diagnostic = diagnostics.Single(static item => item.Id == "JAZORVCA010");
        StringAssert.Contains(diagnostic.GetMessage(), "DynamicComponent", StringComparison.Ordinal);
    }

    [TestMethod]
    public async Task StandardGenericComponentTag_ReportsUnsupportedBuiltInUi()
    {
        var diagnostics = await AnalyzeAsync(
            new SourceFile("Pages/Form.razor.cs", "public sealed class Form;"),
            additionalFiles:
            [new InMemoryAdditionalText(
                "Pages/Form.razor",
                "<InputText @bind-Value=\"Name\" />")]);

        var diagnostic = diagnostics.Single(static item => item.Id == "JAZORVCA010");
        StringAssert.Contains(diagnostic.GetMessage(), "InputText", StringComparison.Ordinal);
    }

    [TestMethod]
    public async Task RemainingStandardComponentTags_ReportUnsupportedBuiltInUi()
    {
        var diagnostics = await AnalyzeAsync(
            new SourceFile("Pages/Standard.razor.cs", "public sealed class Standard;"),
            additionalFiles:
            [new InMemoryAdditionalText(
                "Pages/Standard.razor",
                """
                @using Microsoft.AspNetCore.Components.Authorization
                @using Microsoft.AspNetCore.Components
                @using Microsoft.AspNetCore.Components.Forms
                @using Microsoft.AspNetCore.Components.Routing

                <AuthorizeRouteView />
                <CascadingAuthenticationState />
                <DataAnnotationsValidator />
                <NavigationLock />
                <FocusOnNavigate />
                <PageTitle>Orders</PageTitle>
                <ImportMap />
                <AntiforgeryToken />
                <FormMappingScope Name="scope"><span>content</span></FormMappingScope>
                <CacheView><span>content</span></CacheView>
                <ConfigureBrowser />
                <ResourcePreloader />
                <DisplayName For="@(() => Name)" />
                <InputHidden />
                <Label For="@(() => Name)">Name</Label>
                <EnvironmentView Include="Development"><span>content</span></EnvironmentView>
                <BasePath />
                """)]);

        var messages = diagnostics
            .Where(static item => item.Id == "JAZORVCA010")
            .Select(static item => item.GetMessage())
            .ToArray();

        Assert.HasCount(17, messages);
        foreach (var componentName in new[]
        {
            "AuthorizeRouteView",
            "CascadingAuthenticationState",
            "DataAnnotationsValidator",
            "NavigationLock",
            "FocusOnNavigate",
            "PageTitle",
            "ImportMap",
            "AntiforgeryToken",
            "FormMappingScope"
            ,"CacheView"
            ,"ConfigureBrowser"
            ,"ResourcePreloader"
            ,"DisplayName"
            ,"InputHidden"
            ,"Label"
            ,"EnvironmentView"
            ,"BasePath"
        })
        {
            Assert.IsTrue(
                messages.Any(message => message.Contains(componentName, StringComparison.Ordinal)),
                componentName);
        }
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
    public async Task RazorAuthoringScanner_HandlesDirectiveClassificationMarkupBoundariesAndComponentContracts()
    {
        var diagnostics = await AnalyzeAsync(
            new SourceFile(
                "Pages/Scanner.razor.cs",
                """
                using System;

                namespace Microsoft.AspNetCore.Components
                {
                    [AttributeUsage(AttributeTargets.Property)]
                    public sealed class InjectAttribute : Attribute;

                    [AttributeUsage(AttributeTargets.Property)]
                    public sealed class CascadingParameterAttribute : Attribute;

                    public abstract class ComponentBase;

                    public interface IComponent;

                    public class NavigationManager;

                    public interface IComponentActivator;

                    public struct ParameterView
                    {
                        public bool TryGetValue<T>(string name, out T value)
                        {
                            value = default!;
                            return false;
                        }

                        public object ToDictionary() => new object();

                        public Enumerator GetEnumerator() => default;

                        public struct Enumerator
                        {
                            public object Current => new object();

                            public bool MoveNext() => false;
                        }
                    }
                }

                namespace Microsoft.AspNetCore.Http
                {
                    public abstract class HttpContext;

                    public interface IHttpContextAccessor;
                }

                namespace Microsoft.EntityFrameworkCore
                {
                    public abstract class DbContext;
                }

                namespace Microsoft.AspNetCore.Components.Forms
                {
                    public sealed class InputFile;

                    public abstract class InputBase<T>;
                }

                namespace Demo
                {
                    public sealed class Db : Microsoft.EntityFrameworkCore.DbContext;

                    public sealed class DerivedContext : Microsoft.AspNetCore.Http.HttpContext;

                    public sealed class Accessor : Microsoft.AspNetCore.Http.IHttpContextAccessor;

                    public sealed class BrowserAdapter : Microsoft.AspNetCore.Components.NavigationManager;

                    public sealed class BrowserUnavailable : Microsoft.AspNetCore.Components.IComponentActivator;

                    public sealed class OtherParameterView
                    {
                        public bool TryGetValue<T>(string name, out T value)
                        {
                            value = default!;
                            return false;
                        }
                    }

                    public sealed class BranchComponent : Microsoft.AspNetCore.Components.ComponentBase
                    {
                        private Microsoft.AspNetCore.Components.ParameterView Values { get; set; }

                        [Microsoft.AspNetCore.Components.Inject]
                        public Db Database { get; set; } = null!;

                        [Microsoft.AspNetCore.Components.Inject]
                        public BrowserAdapter Adapter { get; set; } = null!;

                        [Microsoft.AspNetCore.Components.Inject]
                        public BrowserAdapter ReadOnlyAdapter { get; } = null!;

                        [Microsoft.AspNetCore.Components.Inject]
                        public BrowserAdapter InitOnlyAdapter { get; init; } = null!;

                        [Microsoft.AspNetCore.Components.Inject]
                        public static BrowserAdapter StaticAdapter { get; set; } = null!;

                        [Microsoft.AspNetCore.Components.Inject]
                        public DerivedContext ServerContext { get; set; } = null!;

                        [Microsoft.AspNetCore.Components.Inject]
                        public Accessor ServerAccessor { get; set; } = null!;

                        [Microsoft.AspNetCore.Components.Inject]
                        public BrowserUnavailable Unavailable { get; set; } = null!;

                        [Microsoft.AspNetCore.Components.CascadingParameter]
                        public string InitOnlyCascade { get; init; } = string.Empty;

                        [Microsoft.AspNetCore.Components.CascadingParameter]
                        public static string StaticCascade { get; set; } = string.Empty;

                        private void Inspect()
                        {
                            Values.TryGetValue<string>("name", out _);
                            Values.ToDictionary();
                            foreach (var value in (Values))
                            {
                            }
                        }
                    }

                    public sealed class ContractOnly : Microsoft.AspNetCore.Components.IComponent
                    {
                        [Microsoft.AspNetCore.Components.Inject]
                        public BrowserAdapter ReadOnlyAdapter { get; } = null!;
                    }

                    public sealed class NotAComponent
                    {
                        private Microsoft.AspNetCore.Components.ParameterView Values { get; set; }

                        [Microsoft.AspNetCore.Components.Inject]
                        public BrowserAdapter ReadOnlyAdapter { get; } = null!;

                        private void Inspect()
                        {
                            Values.TryGetValue<string>("name", out _);
                            foreach (var value in Values)
                            {
                            }
                        }
                    }

                    public sealed class SameNameComponent : Microsoft.AspNetCore.Components.ComponentBase
                    {
                        private OtherParameterView Values { get; set; } = new();

                        private void Inspect()
                            => Values.TryGetValue<string>("name", out _);
                    }
                }

                namespace Demo.First
                {
                    public sealed class Duplicate : Microsoft.EntityFrameworkCore.DbContext;
                }

                namespace Demo.Second
                {
                    public sealed class Duplicate : Microsoft.EntityFrameworkCore.DbContext;
                }
                """),
            additionalFiles:
            [
                new InMemoryAdditionalText(
                    "Pages/Scanner.razor",
                    """
                       @inject Demo.Db QualifiedDatabase
                    @inject global::Demo.Db GlobalDatabase
                    @inject Db SimpleDatabase
                    @inject\tDemo.Db TabDatabase
                    @inject Demo.DerivedContext ServerContext
                    @inject Demo.Accessor ServerAccessor
                    @inject Demo.BrowserAdapter BrowserAdapter
                    @inject Demo.BrowserUnavailable BrowserUnavailable
                    @inject Duplicate AmbiguousDatabase
                    @inject Demo.Missing UnknownDatabase
                    @inject System.Collections.Generic.List<Demo.Db> GenericDatabase
                    @inject Db[] ArrayDatabase
                    @inject
                    @injectX Demo.Db NotADirective
                    <InputFile />
                    <Demo.InputFile />
                    <InputBase />
                    <div title="<InputFile />" data='<InputFile />'>content</div>
                    @{ var inline = "<InputFile />"; }
                    @code { var quoted = "{ <InputFile /> }"; }
                    @functions { var escaped = "\\\"<InputFile />"; }
                    </InputFile>
                    <!InputFile>
                    <?InputFile>
                    <1InputFile>
                    """),
                new InMemoryAdditionalText("Pages/Comment.razor", "<!-- <InputFile /> -->\n@* <InputFile /> *@"),
                new InMemoryAdditionalText("Pages/UnterminatedHtmlComment.razor", "<!-- <InputFile />"),
                new InMemoryAdditionalText("Pages/UnterminatedRazorComment.razor", "@* <InputFile />")
            ],
            references: RazorSgTestHost.CreateMetadataReferences()
                .Where(static reference =>
                    !Path.GetFileName(reference.Display ?? string.Empty)
                        .StartsWith("Microsoft.AspNetCore", StringComparison.OrdinalIgnoreCase))
                .ToArray());

        Assert.HasCount(4, diagnostics.Where(static item => item.Id == "JAZORVCA001"));
        Assert.HasCount(4, diagnostics.Where(static item => item.Id == "JAZORVCA002"));
        Assert.HasCount(1, diagnostics.Where(static item => item.Id == "JAZORVCA003"));
        Assert.HasCount(1, diagnostics.Where(static item => item.Id == "JAZORVCA004"));
        Assert.HasCount(1, diagnostics.Where(static item => item.Id == "JAZORVCA005"));
        Assert.HasCount(4, diagnostics.Where(static item => item.Id == "JAZORVCA006"));
        Assert.HasCount(2, diagnostics.Where(static item => item.Id == "JAZORVCA007"));
        Assert.HasCount(2, diagnostics.Where(static item => item.Id == "JAZORVCA008"));
        Assert.HasCount(3, diagnostics.Where(static item => item.Id == "JAZORVCA010"));
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
        Assert.AreEqual("JAZORVCA011", RazorVueCompatibilityAnalyzer.SsrStateHandoffUnavailable.Id);
        Assert.AreEqual("JAZORVCA012", RazorVueCompatibilityAnalyzer.StreamRenderingUnavailable.Id);
        Assert.IsTrue(RazorVueCompatibilityAnalyzer.SsrStateHandoffUnavailable.HelpLinkUri.EndsWith(
            "#ssr-state-handoff",
            StringComparison.Ordinal));
        Assert.AreEqual("JAZORVCA008", RazorVueCompatibilityAnalyzer.CascadingParameterUnsupported.Id);
        Assert.IsTrue(RazorVueCompatibilityAnalyzer.CascadingParameterUnsupported.HelpLinkUri.EndsWith(
            "#cascading-parameters",
            StringComparison.Ordinal));
        Assert.AreEqual("JAZORVCA009", RazorVueCompatibilityAnalyzer.RouteDirectiveRequiresHostAdapter.Id);
        Assert.AreEqual("JAZORVCA010", RazorVueCompatibilityAnalyzer.BlazorComponentAdapterUnavailable.Id);
        Assert.HasCount(11, new RazorVueCompatibilityAnalyzer().SupportedDiagnostics);
    }

    [TestMethod]
    public void RazorDirectiveScanners_KeepWhitespaceAndMarkupBoundariesExplicit()
    {
        var source = SourceText.From(
            """
               @page /leading
            @page	/tab
            @page
            @pageX /not-a-page
            @page/without-space

            @inject Demo.Service Service
            @inject	Demo.OtherService Other
            @inject
            @injectX Demo.Ignored Ignored
            """);

        var pages = InvokeScanner("EnumeratePageDirectives", source)
            .Select(value => GetScannerProperty<string>(value, "RouteText"))
            .ToArray();
        CollectionAssert.AreEqual(new[] { "/leading", "/tab" }, pages);

        var injections = InvokeScanner("EnumerateInjectDirectives", source)
            .Select(value => GetScannerProperty<string>(value, "TypeName"))
            .ToArray();
        CollectionAssert.AreEqual(new[] { "Demo.Service", "Demo.OtherService" }, injections);

        var tags = InvokeScanner(
                "EnumerateRazorComponentTags",
                SourceText.From(
                    """
                    <InputFile />
                    <Demo.InputFile />
                    <InputBase />
                    <!-- <InputFile /> -->
                    @* <InputFile /> *@
                    @code { var ignored = "<InputFile />"; }
                    <div title="<InputFile />"></div>
                    </InputFile>
                    <!InputFile>
                    <?InputFile>
                    <1InputFile>
                    """))
            .Select(value => GetScannerProperty<string>(value, "TagName"))
            .ToArray();
        CollectionAssert.AreEqual(new[] { "InputFile", "InputFile", "InputBase" }, tags);
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

    private static IEnumerable<object> InvokeScanner(string methodName, SourceText source)
    {
        var method = typeof(RazorVueCompatibilityAnalyzer)
            .GetMethods(System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)
            .Single(candidate => candidate.Name == methodName && candidate.GetParameters().Length == 1);
        return ((System.Collections.IEnumerable)method.Invoke(null, [source])!)
            .Cast<object>();
    }

    private static T GetScannerProperty<T>(object value, string propertyName)
        => (T)value.GetType().GetProperty(propertyName)!.GetValue(value)!;

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
