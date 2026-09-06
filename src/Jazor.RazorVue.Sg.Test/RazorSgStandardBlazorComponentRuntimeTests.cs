namespace Jazor.RazorVue.Sg.Test;

[TestClass]
public sealed class RazorSgStandardBlazorComponentRuntimeTests
{
    [TestMethod]
    public Task DynamicComponent_IsRejectedAsBuiltInUi()
        => AssertBuiltInRejectedAsync(
            "DynamicHost",
            "<DynamicComponent Type=\"@typeof(object)\" />",
            """
            namespace Demo.Pages;

            [ECMAScriptModule("./components/dynamic-host")]
            public partial class DynamicHost : ComponentBase, IVueComponent;
            """,
            "Microsoft.AspNetCore.Components.DynamicComponent");

    [TestMethod]
    public Task EditFormAndInputText_AreRejectedAsBuiltInUi()
        => AssertBuiltInRejectedAsync(
            "FormHost",
            """
            @using Microsoft.AspNetCore.Components.Forms

            <EditForm Model="@model">
                <InputText @bind-Value="Name" />
            </EditForm>
            """,
            """
            namespace Demo.Pages;

            [ECMAScriptModule("./components/form-host")]
            public partial class FormHost : ComponentBase, IVueComponent
            {
                private object model = new();
                private string Name { get; set; } = "initial";
            }
            """,
            "Microsoft.AspNetCore.Components.Forms.EditForm");

    [TestMethod]
    public Task Router_IsRejectedAsBuiltInUi()
        => AssertBuiltInRejectedAsync(
            "AppRouter",
            """
            @using Microsoft.AspNetCore.Components.Routing

            <Router AppAssembly="@typeof(Program).Assembly" />
            """,
            """
            namespace Demo.Pages;

            public sealed class Program;

            [ECMAScriptModule("./components/app-router")]
            public partial class AppRouter : ComponentBase, IVueComponent;
            """,
            "Microsoft.AspNetCore.Components.Routing.Router");

    [TestMethod]
    public Task TypedInput_IsRejectedAsBuiltInUi()
        => AssertBuiltInRejectedAsync(
            "TypedInput",
            """
            @using Microsoft.AspNetCore.Components.Forms

            <InputNumber TValue="int" @bind-Value="Count" />
            """,
            """
            namespace Demo.Pages;

            [ECMAScriptModule("./components/typed-input")]
            public partial class TypedInput : ComponentBase, IVueComponent
            {
                private int Count { get; set; }
            }
            """,
            "Microsoft.AspNetCore.Components.Forms.InputNumber<int>");

    [TestMethod]
    public Task ErrorBoundary_IsRejectedAsBuiltInUi()
        => AssertBuiltInRejectedAsync(
            "ErrorBoundaryHost",
            """
            @using Microsoft.AspNetCore.Components.Web

            <ErrorBoundary />
            """,
            """
            namespace Demo.Pages;

            [ECMAScriptModule("./components/error-boundary-host")]
            public partial class ErrorBoundaryHost : ComponentBase, IVueComponent;
            """,
            "Microsoft.AspNetCore.Components.Web.ErrorBoundary");

    [TestMethod]
    public Task QuickGrid_IsRejectedAsBuiltInUi()
        => AssertBuiltInRejectedAsync(
            "QuickGridHost",
            """
            @using Microsoft.AspNetCore.Components.QuickGrid

            <QuickGrid TGridItem="int" Items="@Array.Empty<int>()" />
            """,
            """
            namespace Microsoft.AspNetCore.Components.QuickGrid
            {
                public class QuickGrid<TGridItem> : ComponentBase
                {
                    [Parameter]
                    public IEnumerable<TGridItem>? Items { get; set; }
                }
            }

            namespace Demo.Pages
            {
                [ECMAScriptModule("./components/quick-grid-host")]
                public partial class QuickGridHost : ComponentBase, IVueComponent;
            }
            """,
            "Microsoft.AspNetCore.Components.QuickGrid.QuickGrid<int>");

    [TestMethod]
    public async Task RemainingStandardBlazorComponents_AreRejectedAsBuiltInUi()
    {
        var cases = new[]
        {
            (
                Name: "NavigationLockHost",
                Markup: """
                    @using Microsoft.AspNetCore.Components.Routing

                    <NavigationLock />
                    """,
                ExpectedType: "Microsoft.AspNetCore.Components.Routing.NavigationLock"),
            (
                Name: "FocusOnNavigateHost",
                Markup: """
                    @using Microsoft.AspNetCore.Components.Routing

                    <FocusOnNavigate />
                    """,
                ExpectedType: "Microsoft.AspNetCore.Components.Routing.FocusOnNavigate"),
            (
                Name: "PageTitleHost",
                Markup: """
                    @using Microsoft.AspNetCore.Components.Web

                    <PageTitle>Orders</PageTitle>
                    """,
                ExpectedType: "Microsoft.AspNetCore.Components.Web.PageTitle"),
            (
                Name: "AuthorizeRouteViewHost",
                Markup: """
                    @using Microsoft.AspNetCore.Components.Authorization

                    <AuthorizeRouteView />
                    """,
                ExpectedType: "Microsoft.AspNetCore.Components.Authorization.AuthorizeRouteView"),
            (
                Name: "CascadingAuthenticationStateHost",
                Markup: """
                    @using Microsoft.AspNetCore.Components.Authorization

                    <CascadingAuthenticationState />
                    """,
                ExpectedType: "Microsoft.AspNetCore.Components.Authorization.CascadingAuthenticationState"),
            (
                Name: "DataAnnotationsValidatorHost",
                Markup: """
                    @using Microsoft.AspNetCore.Components.Forms

                    <DataAnnotationsValidator />
                    """,
                ExpectedType: "Microsoft.AspNetCore.Components.Forms.DataAnnotationsValidator")
            ,(
                Name: "VirtualizeHost",
                Markup: """
                    @using Microsoft.AspNetCore.Components.Web.Virtualization

                    <Virtualize Items="@Array.Empty<int>()" />
                    """,
                ExpectedType: "Microsoft.AspNetCore.Components.Web.Virtualization.Virtualize<TItem>")
            ,(
                Name: "SectionContentHost",
                Markup: """
                    @using Microsoft.AspNetCore.Components.Sections

                    <SectionContent SectionId="main">content</SectionContent>
                    """,
                ExpectedType: "Microsoft.AspNetCore.Components.Sections.SectionContent")
            ,(
                Name: "SectionOutletHost",
                Markup: """
                    @using Microsoft.AspNetCore.Components.Sections

                    <SectionOutlet SectionId="main" />
                    """,
                ExpectedType: "Microsoft.AspNetCore.Components.Sections.SectionOutlet")
        };

        foreach (var testCase in cases)
        {
            await AssertBuiltInRejectedAsync(
                testCase.Name,
                testCase.Markup,
                $$"""
                namespace Demo.Pages;

                [ECMAScriptModule("./components/{{testCase.Name.ToLowerInvariant()}}")]
                public partial class {{testCase.Name}} : ComponentBase, IVueComponent;
                """,
                testCase.ExpectedType);
        }
    }

    private static async Task AssertBuiltInRejectedAsync(
        string componentName,
        string documentText,
        string codeBehindSource,
        string expectedType)
    {
        var documentPath = RazorSgTestHost.GetTestDocumentPath(
            "Pages/" + componentName + ".razor");
        var diagnostics = await RazorSgOfficialAuthoringTestHost.GetGeneratorDiagnosticsAsync(
            documentPath,
            documentText,
            codeBehindSource,
            "Demo.Pages");

        var diagnostic = diagnostics.SingleOrDefault(item => item.Id == "JAZORVGA021");
        Assert.IsNotNull(
            diagnostic,
            "Expected JAZORVGA021. Actual diagnostics: " +
            string.Join(
                Environment.NewLine,
                diagnostics.Select(static item => item.Id + ": " + item.GetMessage())));
        StringAssert.Contains(diagnostic.GetMessage(), "built-in UI component", StringComparison.Ordinal, componentName);
        StringAssert.Contains(diagnostic.GetMessage(), expectedType, StringComparison.Ordinal, componentName);
        StringAssert.Contains(diagnostic.GetMessage(), "ComponentBase + IVueComponent", StringComparison.Ordinal, componentName);
    }
}
