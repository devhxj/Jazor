using System.Collections.Immutable;
using System.Globalization;
using System.Reflection;
using Jazor.RazorVue.Generation;
using Jazor.RazorVue.RazorSdk;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;
using Microsoft.CodeAnalysis.Text;

namespace Jazor.RazorVue.Sg.Test;

[TestClass]
public sealed class RazorVueRouteCatalogBuilderTests
{
    [TestMethod]
    public void Build_ProjectsSortedRouteLayoutRouteAndQueryContracts()
    {
        var fixture = CreateFixture();
        var orders = fixture.GetComponent("Orders");
        var plain = fixture.GetComponent("Plain");
        var shell = fixture.GetComponent("Shell");
        var artifacts = ImmutableArray.Create(
            CreateArtifact(orders.ComponentSymbol, "Pages/Orders.mjs"),
            CreateArtifact(plain.ComponentSymbol, "Pages/Plain.mjs"),
            CreateArtifact(shell.ComponentSymbol, "Layouts/Shell.mjs"));

        var catalog = RazorVueRouteCatalogBuilder.Build(fixture.Binding, artifacts);

        Assert.AreEqual(RazorVueRouteCatalogBuilder.RelativePath, catalog.RelativePath);
        Assert.AreEqual(VueHmrBoundaryKind.FullReloadRequired, catalog.Hmr.BoundaryKind);
        Assert.AreEqual(catalog.ContentHash, catalog.Hmr.DescriptorHash);
        Assert.IsEmpty(catalog.PackageImports);
        Assert.IsEmpty(catalog.Assets);
        StringAssert.Contains(catalog.ModuleText, "import routeComponent0", StringComparison.Ordinal);
        StringAssert.Contains(catalog.ModuleText, "Orders.mjs", StringComparison.Ordinal);
        StringAssert.Contains(catalog.ModuleText, "Shell.mjs", StringComparison.Ordinal);
        Assert.IsFalse(catalog.ModuleText.Contains("Plain.mjs", StringComparison.Ordinal), catalog.ModuleText);
        StringAssert.Contains(catalog.ModuleText, "template: \"/orders/{id:int}/{name?}/{*catchAll}/{missing}\"", StringComparison.Ordinal);
        StringAssert.Contains(catalog.ModuleText, "template: \"/orders/{id}/{ID}\"", StringComparison.Ordinal);
        StringAssert.Contains(catalog.ModuleText, "{ name: \"id\", prop: \"Id\", kind: \"number\" }", StringComparison.Ordinal);
        StringAssert.Contains(catalog.ModuleText, "{ name: \"name\", prop: \"Name\", kind: \"string\" }", StringComparison.Ordinal);
        StringAssert.Contains(catalog.ModuleText, "{ name: \"catchAll\", prop: \"CatchAll\", kind: \"string\" }", StringComparison.Ordinal);
        Assert.IsFalse(catalog.ModuleText.Contains("\"missing\"", StringComparison.Ordinal), catalog.ModuleText);
        StringAssert.Contains(catalog.ModuleText, "{ name: \"Active\", prop: \"Active\", kind: \"boolean\" }", StringComparison.Ordinal);
        StringAssert.Contains(catalog.ModuleText, "{ name: \"page-index\", prop: \"Page\", kind: \"number\" }", StringComparison.Ordinal);
        StringAssert.Contains(catalog.ModuleText, "{ name: \"Status\", prop: \"Status\", kind: \"number\" }", StringComparison.Ordinal);
        StringAssert.Contains(catalog.ModuleText, "{ name: \"term\", prop: \"Term\", kind: \"string\" }", StringComparison.Ordinal);

        var firstTemplate = catalog.ModuleText.IndexOf("/orders/{id:int}", StringComparison.Ordinal);
        var secondTemplate = catalog.ModuleText.IndexOf("/orders/{id}/{ID}", StringComparison.Ordinal);
        Assert.IsTrue(firstTemplate >= 0 && secondTemplate >= 0 && firstTemplate < secondTemplate, catalog.ModuleText);
    }

    [TestMethod]
    public void Build_ReportsMissingPageOrLayoutArtifactsAndHelperContractsStayDeterministic()
    {
        var fixture = CreateFixture();
        var orders = fixture.GetComponent("Orders");
        var shell = fixture.GetComponent("Shell");

        var missingPage = Assert.Throws<InvalidOperationException>(() =>
            RazorVueRouteCatalogBuilder.Build(fixture.Binding, ImmutableArray<VueModuleArtifact>.Empty));
        StringAssert.Contains(missingPage.Message, "has no generated component artifact", StringComparison.Ordinal);

        var missingLayout = Assert.Throws<InvalidOperationException>(() =>
            RazorVueRouteCatalogBuilder.Build(
                fixture.Binding,
                ImmutableArray.Create(CreateArtifact(orders.ComponentSymbol, "Pages/Orders.mjs"))));
        StringAssert.Contains(missingLayout.Message, "layout", StringComparison.Ordinal);
        StringAssert.Contains(missingLayout.Message, "has no generated component artifact", StringComparison.Ordinal);

        Assert.AreEqual("./same.mjs", Invoke<string>("GetRelativeImport", "@jazor/vue-runtime/routes.mjs", "@jazor/vue-runtime/same.mjs"));
        Assert.AreEqual("../../Pages/Orders.mjs", Invoke<string>("GetRelativeImport", "@jazor/vue-runtime/routes.mjs", "Pages/Orders.mjs"));
        Assert.AreEqual("\"quote\\\" slash\\\\ newline\\n tab\\t control\\u001f\"", Invoke<string>("JavaScriptString", "quote\" slash\\ newline\n tab\t control\u001f"));
        Assert.IsTrue(Invoke<string>("ComputeContentHash", "route-content").All(static value => value is >= '0' and <= '9' or >= 'a' and <= 'f'));

        var intType = fixture.Binding.Compilation.GetSpecialType(SpecialType.System_Int32);
        var nullableInt = fixture.Binding.Compilation.GetSpecialType(SpecialType.System_Nullable_T).Construct(intType);
        var boolType = fixture.Binding.Compilation.GetSpecialType(SpecialType.System_Boolean);
        var stringType = fixture.Binding.Compilation.GetSpecialType(SpecialType.System_String);
        var status = fixture.Binding.Compilation.GetTypeByMetadataName("RouteCatalogContracts.Status");
        Assert.IsNotNull(status);
        Assert.AreEqual("number", Invoke<string>("GetRouteValueKind", intType));
        Assert.AreEqual("number", Invoke<string>("GetRouteValueKind", nullableInt));
        Assert.AreEqual("boolean", Invoke<string>("GetRouteValueKind", boolType));
        Assert.AreEqual("number", Invoke<string>("GetRouteValueKind", status!));
        Assert.AreEqual("string", Invoke<string>("GetRouteValueKind", stringType));

        AssertRouteComparerContracts(orders.ComponentSymbol, shell.ComponentSymbol);
    }

    private static void AssertRouteComparerContracts(INamedTypeSymbol orders, INamedTypeSymbol shell)
    {
        var owner = typeof(RazorVueRouteCatalogBuilder);
        var definitionType = owner.GetNestedType("RouteDefinition", BindingFlags.NonPublic)!;
        var parameterType = owner.GetNestedType("RouteParameter", BindingFlags.NonPublic)!;
        var comparerType = owner.GetNestedType("RouteDefinitionComparer", BindingFlags.NonPublic)!;
        var emptyParameters = Activator.CreateInstance(typeof(ImmutableArray<>).MakeGenericType(parameterType))!;
        var ordersArtifact = CreateArtifact(orders, "Pages/Orders.mjs");
        var shellArtifact = CreateArtifact(shell, "Layouts/Shell.mjs");
        var alpha = Activator.CreateInstance(
            definitionType,
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
            binder: null,
            ["/alpha", ordersArtifact, null, emptyParameters, emptyParameters],
            CultureInfo.InvariantCulture)!;
        var beta = Activator.CreateInstance(
            definitionType,
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
            binder: null,
            ["/beta", ordersArtifact, null, emptyParameters, emptyParameters],
            CultureInfo.InvariantCulture)!;
        var alphaOtherComponent = Activator.CreateInstance(
            definitionType,
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
            binder: null,
            ["/alpha", shellArtifact, null, emptyParameters, emptyParameters],
            CultureInfo.InvariantCulture)!;
        var comparer = comparerType.GetField("Instance", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)!.GetValue(null)!;
        var compare = comparerType.GetMethod("Compare", BindingFlags.Instance | BindingFlags.Public)!;

        Assert.AreEqual(0, (int)compare.Invoke(comparer, [alpha, alpha])!);
        Assert.IsTrue((int)compare.Invoke(comparer, [null, alpha])! < 0);
        Assert.IsTrue((int)compare.Invoke(comparer, [alpha, null])! > 0);
        Assert.IsTrue((int)compare.Invoke(comparer, [alpha, beta])! < 0);
        Assert.AreNotEqual(0, (int)compare.Invoke(comparer, [alpha, alphaOtherComponent])!);
    }

    private static T Invoke<T>(string methodName, params object?[] arguments)
    {
        var method = typeof(RazorVueRouteCatalogBuilder)
            .GetMethods(BindingFlags.Static | BindingFlags.NonPublic)
            .Single(candidate => candidate.Name == methodName && candidate.GetParameters().Length == arguments.Length);
        return (T)method.Invoke(null, arguments)!;
    }

    private static VueModuleArtifact CreateArtifact(INamedTypeSymbol symbol, string relativePath)
        => new(
            symbol.ToDisplayString(),
            relativePath,
            "export default {};\n",
            "content-hash:" + symbol.Name,
            relativePath + ".map",
            "{}",
            "map-hash:" + symbol.Name,
            ImmutableArray<string>.Empty,
            ImmutableArray<VueAsset>.Empty,
            new VueHmrMetadata(
                "test:" + symbol.Name,
                "descriptor",
                "template",
                "logic",
                VueHmrBoundaryKind.LogicSafe));

    private static RouteFixture CreateFixture()
    {
        const string source = """
            using Microsoft.AspNetCore.Components;
            using Microsoft.AspNetCore.Components.Rendering;

            namespace RouteCatalogContracts;

            public enum Status
            {
                Unknown,
                Ready
            }

            public sealed class Shell : ComponentBase
            {
                protected override void BuildRenderTree(RenderTreeBuilder builder) { }
            }

            public abstract class OrdersBase : ComponentBase
            {
                [Parameter]
                [SupplyParameterFromQuery(Name = "page-index")]
                public int? Page { get; set; }

                [Parameter]
                [SupplyParameterFromQuery]
                public bool Active { get; set; }

                [Parameter]
                [SupplyParameterFromQuery]
                public string BaseOnly { get; set; } = string.Empty;

                [Parameter]
                [SupplyParameterFromQuery(Name = "base-term")]
                public string Term { get; set; } = string.Empty;

                protected override void BuildRenderTree(RenderTreeBuilder builder) { }
            }

            [Route("/orders/{id:int}/{name?}/{*catchAll}/{missing}")]
            [Route("/orders/{id}/{ID}")]
            [Layout(typeof(Shell))]
            public sealed class Orders : OrdersBase
            {
                [Parameter] public int Id { get; set; }
                [Parameter] public string? Name { get; set; }
                [Parameter] public string? CatchAll { get; set; }

                [Parameter]
                [SupplyParameterFromQuery(Name = "term")]
                public new string Term { get; set; } = string.Empty;

                [Parameter]
                [SupplyParameterFromQuery]
                public Status Status { get; set; }

                protected override void BuildRenderTree(RenderTreeBuilder builder) { }
            }

            public sealed class Plain : ComponentBase
            {
                protected override void BuildRenderTree(RenderTreeBuilder builder) { }
            }
            """;
        var tree = CSharpSyntaxTree.ParseText(
            source,
            new CSharpParseOptions(LanguageVersion.Preview),
            path: "Pages/Routes.razor.g.cs");
        var compilation = CSharpCompilation.Create(
            "RazorVue.RouteCatalog.Contracts",
            [tree],
            RazorSgTestHost.CreateMetadataReferences(),
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        var errors = RazorSgTestHost.GetCompilationErrors(compilation);
        Assert.IsEmpty(errors, string.Join(Environment.NewLine, errors));

        var document = new GeneratedDocument(
            "Routes.razor.g.cs",
            "Pages/Routes.razor",
            SourceText.From(source),
            ImmutableArray<RazorSourceMap>.Empty);
        var model = compilation.GetSemanticModel(tree);
        var components = tree.GetRoot().DescendantNodes()
            .OfType<ClassDeclarationSyntax>()
            .Where(static declaration => declaration.Identifier.ValueText is "Orders" or "Plain" or "Shell")
            .Select(declaration =>
            {
                var symbol = model.GetDeclaredSymbol(declaration)!;
                var methodDeclaration = declaration.Members.OfType<MethodDeclarationSyntax>()
                    .Single(static method => method.Identifier.ValueText == "BuildRenderTree");
                var method = model.GetDeclaredSymbol(methodDeclaration)!;
                var body = model.GetOperation(methodDeclaration.Body!) as IBlockOperation;
                Assert.IsNotNull(body);
                return new BoundComponent(document, symbol, method, body!);
            })
            .ToImmutableArray();
        return new RouteFixture(
            new GeneratedCSharpBinding(compilation, ImmutableArray.Create(document), components),
            components.ToImmutableDictionary(static component => component.ComponentSymbol.Name, StringComparer.Ordinal));
    }

    private sealed record RouteFixture(
        GeneratedCSharpBinding Binding,
        ImmutableDictionary<string, BoundComponent> Components)
    {
        internal BoundComponent GetComponent(string name) => Components[name];
    }
}
