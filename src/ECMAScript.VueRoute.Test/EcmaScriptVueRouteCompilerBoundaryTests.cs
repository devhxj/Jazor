using System.Threading;
using Acornima.Ast;
using Basic.Reference.Assemblies;
using ECMAScript;
using Jazor.Compiler;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ECMAScriptVueRouteTest;

[TestClass]
public sealed class EcmaScriptVueRouteCompilerBoundaryTests
{
    [TestMethod]
    public async Task VueRoute_HostTypes_AreRecognizedByCompiler_ForRouteObjectConstruction()
    {
        var code = """
            using ECMAScript;
            using static ECMAScript.VueRoute;

            public static class TestClass
            {
                public static RouteLocationAsPath BuildLocation()
                {
                    return new RouteLocationAsPath
                    {
                        Path = "/users",
                        Hash = "#list"
                    };
                }

                public static RouterOptions BuildOptions(RouterHistory history)
                {
                    return new RouterOptions
                    {
                        History = history,
                        Routes =
                        [
                            new RouteRecordRedirect
                            {
                                Path = "/",
                                Redirect = "/home"
                            }
                        ]
                    };
                }
            }
            """;

        var (classSymbol, semanticModel) = CompileAndGetSymbol(
            code,
            "TestClass",
            MetadataReference.CreateFromFile(typeof(ECMAScript.Contract.IUIComponent).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(ECMAScript.Vue3).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(ECMAScript.VueRoute).Assembly.Location));
        var converter = new AstConverter(classSymbol, semanticModel);

        var module = await converter.Convert(CancellationToken.None);
        var script = module?.ToKnRECMAScript();

        Assert.IsNotNull(script);
        StringAssert.Contains(script, "return { path: \"/users\", hash: \"#list\" };");
        StringAssert.Contains(script, "redirect: \"/home\"");
        StringAssert.Contains(script, "history: history");
    }

    [TestMethod]
    public async Task VueRoute_CurrentRoutePathAccess_CompilesThroughVueReadonlyRefValue()
    {
        var code = """
            using ECMAScript;
            using static ECMAScript.VueRoute;

            public static class TestClass
            {
                public static string CurrentPath()
                {
                    return VueRoute.UseRouter().CurrentRoute.Value.Path;
                }
            }
            """;

        var (classSymbol, semanticModel) = CompileAndGetSymbol(
            code,
            "TestClass",
            MetadataReference.CreateFromFile(typeof(ECMAScript.Contract.IUIComponent).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(ECMAScript.Vue3).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(ECMAScript.VueRoute).Assembly.Location));
        var converter = new AstConverter(classSymbol, semanticModel);

        var module = await converter.Convert(CancellationToken.None);
        var script = module?.ToKnRECMAScript();

        Assert.IsNotNull(script);
        StringAssert.Contains(script, "useRouter");
        StringAssert.Contains(script, "currentRoute.value.path");
    }

    [TestMethod]
    public async Task VueRoute_QueryHelpers_AndResolveOverload_Compile_WithTypedContracts()
    {
        var code = """
            using ECMAScript;
            using static ECMAScript.VueRoute;

            public static class TestClass
            {
                public static string BuildHref(Router router)
                {
                    var query = ParseQuery("?page=1&tags=a&tags=b");
                    var rawQuery = new LocationQueryRaw
                    {
                        ["page"] = (Number)1,
                        ["tags"] = new[] { "a", "b" }
                    };
                    var current = UseRoute();
                    var resolved = router.Resolve(new RouteLocationAsRelative
                    {
                        Name = "users",
                        Query = rawQuery
                    }, current);

                    return StringifyQuery(rawQuery) + resolved.Href + query["page"];
                }
            }
            """;

        var (classSymbol, semanticModel) = CompileAndGetSymbol(
            code,
            "TestClass",
            MetadataReference.CreateFromFile(typeof(ECMAScript.Contract.IUIComponent).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(ECMAScript.Vue3).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(ECMAScript.VueRoute).Assembly.Location));
        var converter = new AstConverter(classSymbol, semanticModel);

        var module = await converter.Convert(CancellationToken.None);
        var script = module?.ToKnRECMAScript();

        Assert.IsNotNull(script);
        StringAssert.Contains(script, "parseQuery");
        StringAssert.Contains(script, "stringifyQuery");
        StringAssert.Contains(script, "resolve({ name: \"users\"");
        StringAssert.Contains(script, "let rawQuery = { page: 1, tags: [\"a\", \"b\"] };");
        StringAssert.Contains(script, "query: rawQuery");
    }

    [TestMethod]
    public async Task VueRoute_UseLinkMaybeRefContracts_Compile_WithReactiveTargets_AndStringEnumProps()
    {
        var code = """
            using ECMAScript;
            using static ECMAScript.Vue3;
            using static ECMAScript.VueRoute;

            public static class TestClass
            {
                public static IPromise<NavigationFailure?> BuildLink()
                {
                    var toRef = Ref(new RouteLocationAsRelative
                    {
                        Name = "users"
                    });
                    var replaceRef = Computed(() => true);
                    var link = UseLink(new UseLinkOptions
                    {
                        To = RouteLocationRawMaybeRef.From(toRef),
                        Replace = RouteBooleanMaybeRef.From(replaceRef),
                        ViewTransition = true
                    });
                    var props = new RouterLinkProps
                    {
                        To = "/users",
                        AriaCurrentValue = RouterLinkAriaCurrentValue.Location
                    };

                    return link.Navigate();
                }
            }
            """;

        var (classSymbol, semanticModel) = CompileAndGetSymbol(
            code,
            "TestClass",
            MetadataReference.CreateFromFile(typeof(ECMAScript.Contract.IUIComponent).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(ECMAScript.Vue3).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(ECMAScript.VueRoute).Assembly.Location));
        var converter = new AstConverter(classSymbol, semanticModel);

        var module = await converter.Convert(CancellationToken.None);
        var script = module?.ToKnRECMAScript();

        Assert.IsNotNull(script);
        StringAssert.Contains(script, "let toRef = ref({ name: \"users\" });");
        StringAssert.Contains(script, "let replaceRef = computed(() => {");
        StringAssert.Contains(script, "return true;");
        StringAssert.Contains(script, "let link = useLink({");
        StringAssert.Contains(script, "to: toRef,");
        StringAssert.Contains(script, "replace: replaceRef,");
        StringAssert.Contains(script, "viewTransition: true");
        StringAssert.Contains(script, "let props = { to: \"/users\", ariaCurrentValue: \"location\" };");
        StringAssert.Contains(script, "return link.navigate();");
    }

    [TestMethod]
    public async Task VueRoute_RouterLinkSlotScope_AndResolvedRouteContracts_Compile_WithCanonicalSurface()
    {
        var code = """
            using ECMAScript;
            using static ECMAScript.VueRoute;

            public static class TestClass
            {
                public static string ReadLinkScope(RouterLinkSlotScope scope, RouteLocationResolved resolved)
                {
                    var navigate = scope.Navigate();
                    return scope.Href + scope.Route.Path + scope.IsActive + scope.IsExactActive + resolved.Href + resolved.Replace + resolved.Force + resolved.State + navigate;
                }
            }
            """;

        var (classSymbol, semanticModel) = CompileAndGetSymbol(
            code,
            "TestClass",
            MetadataReference.CreateFromFile(typeof(ECMAScript.Contract.IUIComponent).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(ECMAScript.Vue3).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(ECMAScript.VueRoute).Assembly.Location));
        var converter = new AstConverter(classSymbol, semanticModel);

        var module = await converter.Convert(CancellationToken.None);
        var script = module?.ToKnRECMAScript();

        Assert.IsNotNull(script);
        StringAssert.Contains(script, "let navigate = scope.navigate(null);");
        StringAssert.Contains(script, "scope.href");
        StringAssert.Contains(script, "scope.route.path");
        StringAssert.Contains(script, "scope.isActive");
        StringAssert.Contains(script, "scope.isExactActive");
        StringAssert.Contains(script, "resolved.href");
        StringAssert.Contains(script, "resolved.replace");
        StringAssert.Contains(script, "resolved.force");
        StringAssert.Contains(script, "resolved.state");
    }

    [TestMethod]
    public async Task VueRoute_HistoryState_AndRouterHistoryControls_Compile_WithTypedContracts()
    {
        var code = """
            using System;
            using ECMAScript;
            using static ECMAScript.VueRoute;

            public static class TestClass
            {
                public static string ConfigureHistory(RouterHistory history)
                {
                    var state = new HistoryState
                    {
                        ["page"] = "users",
                        ["id"] = (Number)7,
                        ["flags"] = new HistoryStateValue?[] { true, false, null },
                        ["nested"] = new HistoryState
                        {
                            ["source"] = "test"
                        }
                    };

                    history.Push("/users", state);
                    history.Replace("/users/7", state);
                    var stop = history.Listen((to, from, info) =>
                    {
                        var direction = info.Direction;
                        var delta = info.Delta;
                    });
                    history.Go((Number)(-1), false);

                    var location = new RouteLocationAsPath
                    {
                        Path = "/users/7",
                        State = state
                    };

                    return location.Path + history.Location + history.State + stop;
                }
            }
            """;

        var (classSymbol, semanticModel) = CompileAndGetSymbol(
            code,
            "TestClass",
            MetadataReference.CreateFromFile(typeof(ECMAScript.Contract.IUIComponent).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(ECMAScript.Vue3).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(ECMAScript.VueRoute).Assembly.Location));
        var converter = new AstConverter(classSymbol, semanticModel);

        var module = await converter.Convert(CancellationToken.None);
        var script = module?.ToKnRECMAScript();

        Assert.IsNotNull(script);
        StringAssert.Contains(script, "let state = {");
        StringAssert.Contains(script, "page: \"users\",");
        StringAssert.Contains(script, "id: 7,");
        StringAssert.Contains(script, "flags: [true, false, null],");
        StringAssert.Contains(script, "nested: { source: \"test\" }");
        StringAssert.Contains(script, "history.push(\"/users\", state);");
        StringAssert.Contains(script, "history.replace(\"/users/7\", state);");
        StringAssert.Contains(script, "let stop = history.listen((to, from, info) => {");
        StringAssert.Contains(script, "history.go(-1, false);");
        StringAssert.Contains(script, "state: state");
    }

    private static MetadataReference[] BuildCompilationReferences(IEnumerable<MetadataReference>? additionalReferences = null)
    {
        var references = Net100.References.All.Cast<MetadataReference>().ToList();
        references.Add(MetadataReference.CreateFromFile(typeof(Number).Assembly.Location));
        if (additionalReferences is not null)
            references.AddRange(additionalReferences);

        return references.ToArray();
    }

    private static SyntaxTree[] BuildSyntaxTrees(string code)
        => new[] { CSharpSyntaxTree.ParseText(code) };

    private static (INamedTypeSymbol, SemanticModel) CompileAndGetSymbol(
        string code,
        string className,
        params MetadataReference[] additionalReferences)
    {
        var compilation = CSharpCompilation.Create(
            "ECMAScript.VueRoute.Test.Assembly",
            BuildSyntaxTrees(code),
            BuildCompilationReferences(additionalReferences),
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        var diagnostics = compilation.GetDiagnostics()
            .Where(static diagnostic => diagnostic.Severity == DiagnosticSeverity.Error)
            .ToArray();
        Assert.IsFalse(diagnostics.Length > 0, string.Join(Environment.NewLine, diagnostics.Select(static diagnostic => diagnostic.ToString())));

        foreach (var syntaxTree in compilation.SyntaxTrees)
        {
            var semanticModel = compilation.GetSemanticModel(syntaxTree);
            var classDeclaration = syntaxTree.GetRoot()
                .DescendantNodes()
                .OfType<ClassDeclarationSyntax>()
                .FirstOrDefault(node => node.Identifier.Text == className);

            if (classDeclaration is null)
                continue;

            var classSymbol = semanticModel.GetDeclaredSymbol(classDeclaration);
            Assert.IsNotNull(classSymbol);
            return (classSymbol, semanticModel);
        }

        throw new InvalidOperationException($"Cannot locate class '{className}'.");
    }
}
