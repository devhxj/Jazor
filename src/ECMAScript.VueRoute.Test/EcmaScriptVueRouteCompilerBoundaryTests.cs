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
            using System;
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
            MetadataReference.CreateFromFile(typeof(ECMAScript.Vue.IVueComponent).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(ECMAScript.Vue).Assembly.Location),
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
    public async Task VueRoute_RouterMatcherSurface_Compiles_WithStronglyTypedMatcherLocations_AndRouteManipulation()
    {
        var code = """
            using System;
            using ECMAScript;
            using static ECMAScript.VueRoute;

            public static class TestClass
            {
                public static MatcherLocation BuildMatcherResult(RawRouteComponent component)
                {
                    var root = new RouteRecordSingleView
                    {
                        Path = "/users/:id",
                        Name = "user",
                        Component = component
                    };
                    var matcher = CreateRouterMatcher(
                    [
                        root
                    ], new PathParserOptions
                    {
                        Strict = true,
                        Sensitive = false,
                        End = true
                    });

                    var current = matcher.Resolve(
                        new MatcherLocationAsPath
                        {
                            Path = "/users/current"
                        },
                        new MatcherLocation
                        {
                        });

                    var resolved = matcher.Resolve(
                        new MatcherLocationAsName
                        {
                            Name = "user",
                            Params = new RouteParams
                            {
                                { "id", "42" }
                            }
                        },
                        current);

                    var remove = matcher.AddRoute(new RouteRecordRedirect
                    {
                        Path = "/legacy",
                        Redirect = "/users/42"
                    });
                    remove();
                    matcher.GetRoutes();
                    matcher.GetRecordMatcher("user");

                    return resolved;
                }
            }
            """;

        var (classSymbol, semanticModel) = CompileAndGetSymbol(
            code,
            "TestClass",
            MetadataReference.CreateFromFile(typeof(ECMAScript.Contract.IUIComponent).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(ECMAScript.Vue.IVueComponent).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(ECMAScript.Vue).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(ECMAScript.VueRoute).Assembly.Location));
        var converter = new AstConverter(classSymbol, semanticModel);

        var module = await converter.Convert(CancellationToken.None);
        var script = module?.ToKnRECMAScript();

        Assert.IsNotNull(script);
        StringAssert.Contains(script, "let matcher = createRouterMatcher([");
        StringAssert.Contains(script, "strict: true");
        StringAssert.Contains(script, "sensitive: false");
        StringAssert.Contains(script, "end: true");
        StringAssert.Contains(script, "let current = matcher.resolve({ path: \"/users/current\" }, {});");
        StringAssert.Contains(script, "let resolved = matcher.resolve({ name: \"user\", params: { id: \"42\" } }, current);");
        StringAssert.Contains(script, "let remove = matcher.addRoute({ path: \"/legacy\", redirect: \"/users/42\" });");
        StringAssert.Contains(script, "remove();");
        StringAssert.Contains(script, "matcher.getRoutes();");
        StringAssert.Contains(script, "matcher.getRecordMatcher(\"user\");");
        StringAssert.Contains(script, "return resolved;");
    }

    [TestMethod]
    public async Task VueRoute_PathParserSurface_Compiles_WithTypedParamsRoundTrip()
    {
        var code = """
            using ECMAScript;

            public static class TestClass
            {
                public static string BuildPath(PathParser parser)
                {
                    var parsed = parser.Parse("/users/42");
                    if (parsed is null)
                        return "";

                    var cloned = new RouteParams
                    {
                        { "id", parsed["id"] ?? "" }
                    };
                    return parser.Stringify(cloned);
                }
            }
            """;

        var (classSymbol, semanticModel) = CompileAndGetSymbol(
            code,
            "TestClass",
            MetadataReference.CreateFromFile(typeof(ECMAScript.Contract.IUIComponent).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(ECMAScript.Vue).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(ECMAScript.VueRoute).Assembly.Location));
        var converter = new AstConverter(classSymbol, semanticModel);

        var module = await converter.Convert(CancellationToken.None);
        var script = module?.ToKnRECMAScript();

        Assert.IsNotNull(script);
        StringAssert.Contains(script, "let parsed = parser.parse(\"/users/42\");");
        StringAssert.Contains(script, "if (parsed == null)");
        StringAssert.Contains(script, "let cloned = { id: parsed[\"id\"] ?? \"\" };");
        StringAssert.Contains(script, "return parser.stringify(cloned);");
    }

    [TestMethod]
    public async Task VueRoute_RedirectCallbacks_Compile_AgainstRouteLocationSurface_AndGuardsCanReturnError()
    {
        var code = """
            using System;
            using ECMAScript;
            using static ECMAScript.VueRoute;

            public static class TestClass
            {
                public static RouteRecordRedirect BuildRedirect()
                {
                    return new RouteRecordRedirect
                    {
                        Path = "/legacy",
                        Redirect = new RouteRedirectCallback((to, from) => new RouteLocationAsPath
                        {
                            Path = to.Path + from.FullPath,
                            Hash = to.Hash,
                            Replace = to.Replace,
                            Force = to.Force,
                            State = to.State
                        })
                    };
                }

                public static NavigationGuardReturn? GuardWithError(RouteLocationNormalized to, RouteLocationNormalizedLoaded from)
                {
                    return new Error(to.FullPath + from.FullPath);
                }
            }
            """;

        var (classSymbol, semanticModel) = CompileAndGetSymbol(
            code,
            "TestClass",
            MetadataReference.CreateFromFile(typeof(ECMAScript.Contract.IUIComponent).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(ECMAScript.Vue.IVueComponent).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(ECMAScript.Vue).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(ECMAScript.VueRoute).Assembly.Location));
        var converter = new AstConverter(classSymbol, semanticModel);

        var module = await converter.Convert(CancellationToken.None);
        var script = module?.ToKnRECMAScript();

        Assert.IsNotNull(script);
        StringAssert.Contains(script, "redirect: (to, from) => {");
        StringAssert.Contains(script, "path: to.path + from.fullPath");
        StringAssert.Contains(script, "hash: to.hash");
        StringAssert.Contains(script, "replace: to.replace");
        StringAssert.Contains(script, "force: to.force");
        StringAssert.Contains(script, "state: to.state");
        StringAssert.Contains(script, "return new Error(to.fullPath + from.fullPath);");
    }

    [TestMethod]
    public async Task VueRoute_OfficialNamedRawPathRawAndRedirectOption_Surfaces_Compile_WithoutWeakFallbacks()
    {
        var code = """
            using ECMAScript;

            public static class TestClass
            {
                public static RouteLocationNamedRaw BuildNamed()
                {
                    return new RouteLocationNamedRaw
                    {
                        Name = "user",
                        Params = new RouteParamsRaw
                        {
                            { "id", "42" }
                        },
                        Query = new LocationQueryRaw
                        {
                            { "tab", "profile" }
                        },
                        Hash = "#bio",
                        Replace = true
                    };
                }

                public static RouteLocationPathRaw BuildPath()
                {
                    return new RouteLocationPathRaw
                    {
                        Path = "/users/42",
                        Query = new LocationQueryRaw
                        {
                            { "from", "search" }
                        },
                        Hash = "#top",
                        Force = true
                    };
                }

                public static RouteRecordRedirect BuildRedirect()
                {
                    RouteRecordRedirectOption redirect = new RouteLocationNamedRaw
                    {
                        Name = "user",
                        Params = new RouteParamsRaw
                        {
                            { "id", "7" }
                        }
                    };

                    return new RouteRecordRedirect
                    {
                        Path = "/legacy",
                        Redirect = redirect
                    };
                }
            }
            """;

        var (classSymbol, semanticModel) = CompileAndGetSymbol(
            code,
            "TestClass",
            MetadataReference.CreateFromFile(typeof(ECMAScript.Contract.IUIComponent).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(ECMAScript.Vue).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(ECMAScript.VueRoute).Assembly.Location));
        var converter = new AstConverter(classSymbol, semanticModel);

        var module = await converter.Convert(CancellationToken.None);
        var script = module?.ToKnRECMAScript();

        Assert.IsNotNull(script);
        StringAssert.Contains(script, "name: \"user\"");
        StringAssert.Contains(script, "params: { id: \"42\" }");
        StringAssert.Contains(script, "query: { tab: \"profile\" }");
        StringAssert.Contains(script, "hash: \"#bio\"");
        StringAssert.Contains(script, "replace: true");
        StringAssert.Contains(script, "path: \"/users/42\"");
        StringAssert.Contains(script, "query: { from: \"search\" }");
        StringAssert.Contains(script, "hash: \"#top\"");
        StringAssert.Contains(script, "force: true");
        StringAssert.Contains(script, "let redirect = { name: \"user\", params: { id: \"7\" } };");
        StringAssert.Contains(script, "return { path: \"/legacy\", redirect: redirect };");
    }

    [TestMethod]
    public async Task VueRoute_IntermediateLocationContracts_Compile_AsReusableStronglyTypedAuthoringShapes()
    {
        var code = """
            using ECMAScript;

            public static class TestClass
            {
                public static LocationAsRelativeRaw BuildRelative()
                {
                    return new RouteLocationAsRelative
                    {
                        Name = "user",
                        Params = new RouteParamsRaw
                        {
                            { "id", "42" }
                        },
                        Query = new LocationQueryRaw
                        {
                            { "from", "search" }
                        },
                        Hash = "#top"
                    };
                }

                public static RouteLocationPathRawBase BuildPath()
                {
                    return new RouteLocationPathRaw
                    {
                        Path = "/users/42",
                        Query = new LocationQueryRaw
                        {
                            { "preview", "1" }
                        },
                        Hash = "#section",
                        Replace = true
                    };
                }
            }
            """;

        var (classSymbol, semanticModel) = CompileAndGetSymbol(
            code,
            "TestClass",
            MetadataReference.CreateFromFile(typeof(ECMAScript.Contract.IUIComponent).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(ECMAScript.Vue).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(ECMAScript.VueRoute).Assembly.Location));
        var converter = new AstConverter(classSymbol, semanticModel);

        var module = await converter.Convert(CancellationToken.None);
        var script = module?.ToKnRECMAScript();

        Assert.IsNotNull(script);
        StringAssert.Contains(script, "export function BuildRelative()");
        StringAssert.Contains(script, "name: \"user\"");
        StringAssert.Contains(script, "params: { id: \"42\" }");
        StringAssert.Contains(script, "query: { from: \"search\" }");
        StringAssert.Contains(script, "hash: \"#top\"");
        StringAssert.Contains(script, "export function BuildPath()");
        StringAssert.Contains(script, "path: \"/users/42\"");
        StringAssert.Contains(script, "query: { preview: \"1\" }");
        StringAssert.Contains(script, "hash: \"#section\"");
        StringAssert.Contains(script, "replace: true");
    }

    [TestMethod]
    public async Task VueRoute_LegacyGuardNext_Compiles_WithParameterlessNextCall()
    {
        var code = """
            using ECMAScript;
            using static ECMAScript.VueRoute;

            public static class TestClass
            {
                public static RouteRecordSingleView BuildRecord(RawRouteComponent component)
                {
                    var beforeEnter = new LegacyRouteNavigationGuard((to, from, next) =>
                    {
                        next();
                        return true;
                    });
                    NavigationGuardHandler handler = beforeEnter;
                    RouteRecordBeforeEnter routeBeforeEnter = handler;

                    return new RouteRecordSingleView
                    {
                        Path = "/users",
                        Component = component,
                        BeforeEnter = routeBeforeEnter
                    };
                }
            }
            """;

        var (classSymbol, semanticModel) = CompileAndGetSymbol(
            code,
            "TestClass",
            MetadataReference.CreateFromFile(typeof(ECMAScript.Contract.IUIComponent).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(ECMAScript.Vue).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(ECMAScript.VueRoute).Assembly.Location));
        var converter = new AstConverter(classSymbol, semanticModel);

        var module = await converter.Convert(CancellationToken.None);
        var script = module?.ToKnRECMAScript();

        Assert.IsNotNull(script);
        StringAssert.Contains(script, "let beforeEnter = (to, from, next) => {");
        StringAssert.Contains(script, "let routeBeforeEnter = handler;");
        StringAssert.Contains(script, "next(null);");
        StringAssert.Contains(script, "return true;");
        StringAssert.Contains(script, "beforeEnter: routeBeforeEnter");
    }

    [TestMethod]
    public async Task VueRoute_LegacyGuardNext_Compiles_WithCallbackArgumentAuthoring()
    {
        var code = """
            using ECMAScript;
            using static ECMAScript.VueRoute;

            public static class TestClass
            {
                public static RouteRecordSingleView BuildRecord(RawRouteComponent component)
                {
                    return new RouteRecordSingleView
                    {
                        Path = "/users",
                        Component = component,
                        BeforeEnter = RouteRecordBeforeEnter.From((RouteLocationNormalized to, RouteLocationNormalizedLoaded from, NavigationGuardNext next) =>
                        {
                            next(NavigationGuardNextArgument.From((Vue.VueComponentPublicInstance instance) =>
                            {
                                _ = instance;
                            }));
                            return true;
                        })
                    };
                }
            }
            """;

        var (classSymbol, semanticModel) = CompileAndGetSymbol(
            code,
            "TestClass",
            MetadataReference.CreateFromFile(typeof(ECMAScript.Contract.IUIComponent).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(ECMAScript.Vue).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(ECMAScript.VueRoute).Assembly.Location));
        var converter = new AstConverter(classSymbol, semanticModel);

        var module = await converter.Convert(CancellationToken.None);
        var script = module?.ToKnRECMAScript();

        Assert.IsNotNull(script);
        StringAssert.Contains(script, "beforeEnter: (to, from, next) => {");
        StringAssert.Contains(script, "next(instance => {");
        StringAssert.Contains(script, "return true;");
    }

    [TestMethod]
    public async Task VueRoute_NavigationGuardAuthoring_Compiles_WithExplicitMethodOverloads_AndPropertyFactories()
    {
        var code = """
            using System;
            using ECMAScript;
            using static ECMAScript.VueRoute;

            public static class TestClass
            {
                public static RouteRecordSingleView BuildRecord(RawRouteComponent component)
                {
                    var sync = NavigationGuardHandler.From((RouteLocationNormalized to, RouteLocationNormalizedLoaded from) => true);
                    var asyncGuard = NavigationGuardHandler.From((RouteLocationNormalized to, RouteLocationNormalizedLoaded from) => Promise<NavigationGuardReturn?>.Resolve(true));
                    var legacy = NavigationGuardHandler.From((RouteLocationNormalized to, RouteLocationNormalizedLoaded from, NavigationGuardNext next) =>
                    {
                        next();
                        return true;
                    });
                    var legacyAsync = NavigationGuardHandler.From((RouteLocationNormalized to, RouteLocationNormalizedLoaded from, NavigationGuardNext next) =>
                    {
                        next();
                        return Promise<NavigationGuardReturn?>.Resolve(true);
                    });

                    return new RouteRecordSingleView
                    {
                        Path = "/users",
                        Component = component,
                        BeforeEnter = RouteRecordBeforeEnter.From((RouteLocationNormalized to, RouteLocationNormalizedLoaded from) => true)
                    };
                }

                public static Action ConfigureRouter(Router router)
                {
                    var stop1 = router.BeforeEach((RouteLocationNormalized to, RouteLocationNormalizedLoaded from) => true);
                    var stop2 = router.BeforeResolve((RouteLocationNormalized to, RouteLocationNormalizedLoaded from) => Promise<NavigationGuardReturn?>.Resolve(true));
                    OnBeforeRouteLeave((RouteLocationNormalized to, RouteLocationNormalizedLoaded from, NavigationGuardNext next) =>
                    {
                        next();
                        return true;
                    });
                    OnBeforeRouteUpdate((RouteLocationNormalized to, RouteLocationNormalizedLoaded from, NavigationGuardNext next) =>
                    {
                        next();
                        return Promise<NavigationGuardReturn?>.Resolve(true);
                    });

                    return stop1;
                }
            }
            """;

        var (classSymbol, semanticModel) = CompileAndGetSymbol(
            code,
            "TestClass",
            MetadataReference.CreateFromFile(typeof(ECMAScript.Contract.IUIComponent).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(ECMAScript.Vue).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(ECMAScript.VueRoute).Assembly.Location));
        var converter = new AstConverter(classSymbol, semanticModel);

        var module = await converter.Convert(CancellationToken.None);
        var script = module?.ToKnRECMAScript();

        Assert.IsNotNull(script);
        StringAssert.Contains(script, "let sync = (to, from) => {");
        StringAssert.Contains(script, "let asyncGuard = (to, from) => {");
        StringAssert.Contains(script, "return Promise.resolve(true);");
        StringAssert.Contains(script, "let legacy = (to, from, next) => {");
        StringAssert.Contains(script, "next(null);");
        StringAssert.Contains(script, "let legacyAsync = (to, from, next) => {");
        StringAssert.Contains(script, "beforeEnter: (to, from) => {");
        StringAssert.Contains(script, "let stop1 = router.beforeEach((to, from) => {");
        StringAssert.Contains(script, "let stop2 = router.beforeResolve((to, from) => {");
        StringAssert.Contains(script, "onBeforeRouteLeave((to, from, next) => {");
        StringAssert.Contains(script, "onBeforeRouteUpdate((to, from, next) => {");
    }

    [TestMethod]
    public async Task VueRoute_BeforeEnterArrayAuthoring_Compiles_WithTypedGuardArrayFactories()
    {
        var code = """
            using ECMAScript;
            using static ECMAScript.VueRoute;

            public static class TestClass
            {
                public static RouteRecordSingleView BuildRecord(RawRouteComponent component)
                {
                    var syncGuards = new RouteNavigationGuard[]
                    {
                        (RouteLocationNormalized to, RouteLocationNormalizedLoaded from) => true,
                        (RouteLocationNormalized to, RouteLocationNormalizedLoaded from) => new RouteLocationAsPath
                        {
                            Path = "/login",
                            Hash = to.Hash
                        }
                    };
                    var legacyGuards = new LegacyRouteNavigationGuard[]
                    {
                        (RouteLocationNormalized to, RouteLocationNormalizedLoaded from, NavigationGuardNext next) =>
                        {
                            next();
                            return true;
                        }
                    };
                    var legacyBeforeEnter = RouteRecordBeforeEnter.From(legacyGuards);

                    return new RouteRecordSingleView
                    {
                        Path = "/users",
                        Component = component,
                        BeforeEnter = RouteRecordBeforeEnter.From(syncGuards)
                    };
                }
            }
            """;

        var (classSymbol, semanticModel) = CompileAndGetSymbol(
            code,
            "TestClass",
            MetadataReference.CreateFromFile(typeof(ECMAScript.Contract.IUIComponent).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(ECMAScript.Vue).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(ECMAScript.VueRoute).Assembly.Location));
        var converter = new AstConverter(classSymbol, semanticModel);

        var module = await converter.Convert(CancellationToken.None);
        var script = module?.ToKnRECMAScript();

        Assert.IsNotNull(script);
        StringAssert.Contains(script, "let syncGuards = [(to, from) => {");
        StringAssert.Contains(script, "return true;");
        StringAssert.Contains(script, "path: \"/login\"");
        StringAssert.Contains(script, "hash: to.hash");
        StringAssert.Contains(script, "let legacyGuards = [(to, from, next) => {");
        StringAssert.Contains(script, "next(null);");
        StringAssert.Contains(script, "let legacyBeforeEnter = legacyGuards;");
        StringAssert.Contains(script, "beforeEnter: syncGuards");
    }

    [TestMethod]
    public async Task VueRoute_RoutePropsAndRedirectAuthoring_Compiles_WithExplicitFactories_AndStronglyTypedNamedPropEntries()
    {
        var code = """
            using System.ComponentModel;
            using ECMAScript;
            using static ECMAScript.VueRoute;

            public sealed record TestViewProps : ECMAScript.Vue.VueProps
            {
                [Description("@#featured")]
                public bool Featured { get; init; }
            }

            public static class TestClass
            {
                public static RouterOptions BuildOptions(RouterHistory history, RawRouteComponent component)
                {
                    var staticProps = new TestViewProps
                    {
                        Featured = true
                    };
                    RouteRecordPropsResolver propsResolver = (RouteLocationNormalized to) => new TestViewProps
                    {
                        Featured = to.Name != null
                    };
                    RouteRedirectCallback redirectCallback = (RouteLocation to, RouteLocationNormalizedLoaded from) => new RouteLocationAsPath
                    {
                        Path = to.Path + from.FullPath,
                        Hash = to.Hash,
                        Replace = true
                    };

                    return new RouterOptions
                    {
                        History = history,
                        Routes =
                        [
                            new RouteRecordSingleView
                            {
                                Path = "/users/:id",
                                Component = component,
                                Props = propsResolver
                            },
                            new RouteRecordMultipleViews
                            {
                                Path = "/dashboard",
                                Components = new RawRouteComponents
                                {
                                    ["default"] = component
                                },
                                Props = RouteRecordNamedViewProps.From(new RouteNamedProps
                                {
                                    { "default", true },
                                    { "sidebar", staticProps },
                                    { "footer", (RouteLocationNormalized to) => new TestViewProps { Featured = to.Path != "" } }
                                })
                            },
                            new RouteRecordRedirect
                            {
                                Path = "/legacy",
                                Redirect = redirectCallback
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
            MetadataReference.CreateFromFile(typeof(ECMAScript.Vue).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(ECMAScript.VueRoute).Assembly.Location));
        var converter = new AstConverter(classSymbol, semanticModel);

        var module = await converter.Convert(CancellationToken.None);
        var script = module?.ToKnRECMAScript();

        Assert.IsNotNull(script);
        StringAssert.Contains(script, "let staticProps = {");
        StringAssert.Contains(script, "featured: true");
        StringAssert.Contains(script, "let propsResolver = to => {");
        StringAssert.Contains(script, "featured: to.name !== null");
        StringAssert.Contains(script, "props: propsResolver");
        StringAssert.Contains(script, "default: true");
        StringAssert.Contains(script, "sidebar: staticProps");
        StringAssert.Contains(script, "footer: to => {");
        StringAssert.Contains(script, "featured: to.path !== \"\"");
        StringAssert.Contains(script, "let redirectCallback = (to, from) => {");
        StringAssert.Contains(script, "redirect: redirectCallback");
        StringAssert.Contains(script, "path: to.path + from.fullPath");
        StringAssert.Contains(script, "hash: to.hash");
        StringAssert.Contains(script, "replace: true");
    }

    [TestMethod]
    public async Task VueRoute_RouteShellRecords_Compile_WithoutRequiringNamedViewPayloads()
    {
        var code = """
            using ECMAScript;
            using static ECMAScript.Vue;

            public static class TestClass
            {
                public static RouterOptions BuildOptions(RouterHistory history)
                {
                    return new RouterOptions
                    {
                        History = history,
                        Sensitive = true,
                        Strict = true,
                        End = false,
                        Routes =
                        [
                            new RouteRecordSingleViewWithChildren
                            {
                                Path = "/users",
                                Component = RawRouteComponent.From(DefineComponent(new VueComponentOptions
                                {
                                    Setup = () => null
                                })),
                                Children =
                                [
                                    new RouteRecordSingleView
                                    {
                                        Path = ":id",
                                        Component = RawRouteComponent.From(DefineComponent(new VueComponentOptions
                                        {
                                            Setup = () => null
                                        }))
                                    }
                                ]
                            },
                            new RouteRecordMultipleViewsWithChildren
                            {
                                Path = "/admin",
                                Children =
                                [
                                    new RouteRecordRedirect
                                    {
                                        Path = "",
                                        Redirect = "/admin/home"
                                    }
                                ]
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
            MetadataReference.CreateFromFile(typeof(ECMAScript.Vue.IVueComponent).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(ECMAScript.Vue).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(ECMAScript.VueRoute).Assembly.Location));
        var converter = new AstConverter(classSymbol, semanticModel);

        var module = await converter.Convert(CancellationToken.None);
        var script = module?.ToKnRECMAScript();

        Assert.IsNotNull(script);
        StringAssert.Contains(script, "sensitive: true");
        StringAssert.Contains(script, "strict: true");
        StringAssert.Contains(script, "end: false");
        StringAssert.Contains(script, "path: \"/users\"");
        StringAssert.Contains(script, "component: defineComponent({");
        StringAssert.Contains(script, "children: [{ path: \":id\", component: defineComponent({");
        StringAssert.Contains(script, "path: \"/admin\"");
        StringAssert.Contains(script, "children: [{ path: \"\", redirect: \"/admin/home\" }]");
    }

    [TestMethod]
    public async Task VueRoute_MultipleViewRecordProps_Compile_WithGlobalBooleanContract()
    {
        var code = """
            using ECMAScript;

            public static class TestClass
            {
                public static RouteRecordMultipleViews BuildRecord(RawRouteComponent component)
                {
                    var components = new RawRouteComponents
                    {
                        ["default"] = component,
                        ["sidebar"] = component
                    };

                    return new RouteRecordMultipleViews
                    {
                        Path = "/dashboard",
                        Components = components,
                        Props = true
                    };
                }
            }
            """;

        var (classSymbol, semanticModel) = CompileAndGetSymbol(
            code,
            "TestClass",
            MetadataReference.CreateFromFile(typeof(ECMAScript.Contract.IUIComponent).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(ECMAScript.Vue).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(ECMAScript.VueRoute).Assembly.Location));
        var converter = new AstConverter(classSymbol, semanticModel);

        var module = await converter.Convert(CancellationToken.None);
        var script = module?.ToKnRECMAScript();

        Assert.IsNotNull(script);
        StringAssert.Contains(script, "default: component");
        StringAssert.Contains(script, "sidebar: component");
        StringAssert.Contains(script, "props: true");
    }

    [TestMethod]
    public async Task VueRoute_LazyRouteComponents_Compile_WithExplicitLoaderFactories()
    {
        var code = """
            using ECMAScript;

            public static class TestClass
            {
                public static RouteRecordSingleView BuildLazyRecord()
                {
                    ECMAScript.Vue.IVueComponent component = null!;
                    return new RouteRecordSingleView
                    {
                        Path = "/lazy",
                        Component = RawRouteComponent.From(() => Promise<ECMAScript.Vue.IVueComponent>.Resolve(component))
                    };
                }

                public static RouteComponent BuildTypedLoader()
                {
                    ECMAScript.Vue.IVueComponent component = null!;
                    return RouteComponent.From(() => Promise<ECMAScript.Vue.IVueComponent>.Resolve(component));
                }
            }
            """;

        var (classSymbol, semanticModel) = CompileAndGetSymbol(
            code,
            "TestClass",
            MetadataReference.CreateFromFile(typeof(ECMAScript.Contract.IUIComponent).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(ECMAScript.Vue.IVueComponent).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(ECMAScript.Vue).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(ECMAScript.VueRoute).Assembly.Location));
        var converter = new AstConverter(classSymbol, semanticModel);

        var module = await converter.Convert(CancellationToken.None);
        var script = module?.ToKnRECMAScript();

        Assert.IsNotNull(script);
        StringAssert.Contains(script, "let component = null;");
        StringAssert.Contains(script, "component: () => {");
        StringAssert.Contains(script, "return Promise.resolve(component);");
        StringAssert.Contains(script, "export function BuildTypedLoader()");
        StringAssert.Contains(script, "return () => {");
    }

    [TestMethod]
    public void VueRoute_RouteComponentAuthoring_AllowsDirectNativeUnionBranchAssignment()
    {
        var code = """
            using ECMAScript;

            public static class TestClass
            {
                public static RouteRecordSingleView BuildSingle(ECMAScript.Vue.IVueComponent component)
                {
                    return new RouteRecordSingleView
                    {
                        Path = "/users",
                        Component = component
                    };
                }

                public static RouteComponent BuildLoaded(ECMAScript.Vue.IVueComponent component)
                {
                    return component;
                }
            }
            """;

        var compilation = CSharpCompilation.Create(
            "ECMAScript.VueRoute.Test.Assembly.DirectComponentAssignment",
            BuildSyntaxTrees(code),
            BuildCompilationReferences(new[]
            {
                MetadataReference.CreateFromFile(typeof(ECMAScript.Contract.IUIComponent).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(ECMAScript.Vue.IVueComponent).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(ECMAScript.Vue).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(ECMAScript.VueRoute).Assembly.Location)
            }),
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        var diagnostics = compilation.GetDiagnostics()
            .Where(static diagnostic => diagnostic.Severity == DiagnosticSeverity.Error)
            .ToArray();

        Assert.AreEqual(0, diagnostics.Length, string.Join(Environment.NewLine, diagnostics.Select(static diagnostic => diagnostic.ToString())));
    }

    [TestMethod]
    public async Task VueRoute_RawRouteComponents_CollectionInitializer_Compiles_WithLazyLoaderEntries()
    {
        var code = """
            using ECMAScript;

            public static class TestClass
            {
                public static RawRouteComponents BuildComponents()
                {
                    ECMAScript.Vue.IVueComponent component = null!;
                    return new RawRouteComponents
                    {
                        { "default", () => Promise<ECMAScript.Vue.IVueComponent>.Resolve(component) },
                        { "sidebar", () => Promise<ECMAScript.Vue.IVueComponent>.Resolve(component) }
                    };
                }
            }
            """;

        var (classSymbol, semanticModel) = CompileAndGetSymbol(
            code,
            "TestClass",
            MetadataReference.CreateFromFile(typeof(ECMAScript.Contract.IUIComponent).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(ECMAScript.Vue.IVueComponent).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(ECMAScript.Vue).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(ECMAScript.VueRoute).Assembly.Location));
        var converter = new AstConverter(classSymbol, semanticModel);

        var module = await converter.Convert(CancellationToken.None);
        var script = module?.ToKnRECMAScript();

        Assert.IsNotNull(script);
        StringAssert.Contains(script, "default: () => {");
        StringAssert.Contains(script, "sidebar: () => {");
        StringAssert.Contains(script, "return Promise.resolve(component);");
    }

    [TestMethod]
    public async Task VueRoute_RawRouteComponents_CollectionInitializer_Compiles_WithDirectComponentEntries()
    {
        var code = """
            using ECMAScript;

            public static class TestClass
            {
                public static RawRouteComponents BuildComponents(ECMAScript.Vue.IVueComponent component)
                {
                    return new RawRouteComponents
                    {
                        { "default", component },
                        { "sidebar", component }
                    };
                }
            }
            """;

        var (classSymbol, semanticModel) = CompileAndGetSymbol(
            code,
            "TestClass",
            MetadataReference.CreateFromFile(typeof(ECMAScript.Contract.IUIComponent).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(ECMAScript.Vue.IVueComponent).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(ECMAScript.Vue).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(ECMAScript.VueRoute).Assembly.Location));
        var converter = new AstConverter(classSymbol, semanticModel);

        var module = await converter.Convert(CancellationToken.None);
        var script = module?.ToKnRECMAScript();

        Assert.IsNotNull(script);
        StringAssert.Contains(script, "default: component");
        StringAssert.Contains(script, "sidebar: component");
    }

    [TestMethod]
    public async Task VueRoute_RawRouteComponent_ImplicitConversion_FromRouteComponent_PreservesLazyLoaderBranch()
    {
        var code = """
            using ECMAScript;

            public static class TestClass
            {
                public static RawRouteComponent BuildRawLoader()
                {
                    ECMAScript.Vue.IVueComponent component = null!;
                    RouteComponent typed = RouteComponent.From(() => Promise<ECMAScript.Vue.IVueComponent>.Resolve(component));
                    RawRouteComponent raw = typed;
                    return raw;
                }
            }
            """;

        var (classSymbol, semanticModel) = CompileAndGetSymbol(
            code,
            "TestClass",
            MetadataReference.CreateFromFile(typeof(ECMAScript.Contract.IUIComponent).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(ECMAScript.Vue.IVueComponent).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(ECMAScript.Vue).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(ECMAScript.VueRoute).Assembly.Location));
        var converter = new AstConverter(classSymbol, semanticModel);

        var module = await converter.Convert(CancellationToken.None);
        var script = module?.ToKnRECMAScript();

        Assert.IsNotNull(script);
        StringAssert.Contains(script, "let typed = () => {");
        StringAssert.Contains(script, "return Promise.resolve(component);");
        StringAssert.Contains(script, "let raw = typed;");
        StringAssert.Contains(script, "return raw;");
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
            MetadataReference.CreateFromFile(typeof(ECMAScript.Vue).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(ECMAScript.VueRoute).Assembly.Location));
        var converter = new AstConverter(classSymbol, semanticModel);

        var module = await converter.Convert(CancellationToken.None);
        var script = module?.ToKnRECMAScript();

        Assert.IsNotNull(script);
        StringAssert.Contains(script, "useRouter");
        StringAssert.Contains(script, "currentRoute.value.path");
    }

    [TestMethod]
    public async Task VueRoute_RouterRecordProxyExternProperty_CompilesThroughHostFallback()
    {
        var code = """
            using ECMAScript;
            using static ECMAScript.VueRoute;

            public static class TestClass
            {
                public static string CurrentPath(Router router)
                {
                    return router.CurrentRoute.Value.Path;
                }
            }
            """;

        var (classSymbol, semanticModel) = CompileAndGetSymbol(
            code,
            "TestClass",
            MetadataReference.CreateFromFile(typeof(ECMAScript.Contract.IUIComponent).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(ECMAScript.Vue).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(ECMAScript.VueRoute).Assembly.Location));
        var converter = new AstConverter(classSymbol, semanticModel);

        var module = await converter.Convert(CancellationToken.None);
        var script = module?.ToKnRECMAScript();

        Assert.IsNotNull(script);
        StringAssert.Contains(script, "router.currentRoute.value.path");
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
            MetadataReference.CreateFromFile(typeof(ECMAScript.Vue).Assembly.Location),
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
    public async Task VueRoute_LoadRouteLocation_Compile_WithRouteLocationBaseSurface()
    {
        var code = """
            using ECMAScript;
            using static ECMAScript.VueRoute;

            public static class TestClass
            {
                public static IPromise<RouteLocationNormalizedLoaded> Load(RouteLocation location, RouteLocationNormalized normalized)
                {
                    var fromLocation = LoadRouteLocation(location);
                    var fromNormalized = LoadRouteLocation(normalized);
                    return fromLocation;
                }
            }
            """;

        var (classSymbol, semanticModel) = CompileAndGetSymbol(
            code,
            "TestClass",
            MetadataReference.CreateFromFile(typeof(ECMAScript.Contract.IUIComponent).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(ECMAScript.Vue).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(ECMAScript.VueRoute).Assembly.Location));
        var converter = new AstConverter(classSymbol, semanticModel);

        var module = await converter.Convert(CancellationToken.None);
        var script = module?.ToKnRECMAScript();

        Assert.IsNotNull(script);
        StringAssert.Contains(script, "let fromLocation = loadRouteLocation(location);");
        StringAssert.Contains(script, "let fromNormalized = loadRouteLocation(normalized);");
        StringAssert.Contains(script, "return fromLocation;");
    }

    [TestMethod]
    public async Task VueRoute_QueryContracts_Compile_WithNullAndMixedArrayPayloads()
    {
        var code = """
            using ECMAScript;
            using static ECMAScript.VueRoute;

            public static class TestClass
            {
                public static string BuildQuery()
                {
                    var raw = new LocationQueryRaw
                    {
                        ["page"] = (Number)1,
                        ["empty"] = null,
                        ["tags"] = new LocationQueryValueRaw?[] { "a", null, (Number)3 }
                    };
                    var parsed = ParseQuery("?flag&name=han");

                    return StringifyQuery(raw) + parsed["flag"] + parsed["name"];
                }
            }
            """;

        var (classSymbol, semanticModel) = CompileAndGetSymbol(
            code,
            "TestClass",
            MetadataReference.CreateFromFile(typeof(ECMAScript.Contract.IUIComponent).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(ECMAScript.Vue).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(ECMAScript.VueRoute).Assembly.Location));
        var converter = new AstConverter(classSymbol, semanticModel);

        var module = await converter.Convert(CancellationToken.None);
        var script = module?.ToKnRECMAScript();

        Assert.IsNotNull(script);
        StringAssert.Contains(script, "empty: null");
        StringAssert.Contains(script, "tags: [\"a\", null, 3]");
        StringAssert.Contains(script, "parseQuery");
        StringAssert.Contains(script, "stringifyQuery");
    }

    [TestMethod]
    public async Task VueRoute_QueryContracts_Compile_WithUndefinedNullAndMixedArrayPayloads()
    {
        var code = """
            using ECMAScript;
            using static ECMAScript.Global;
            using static ECMAScript.VueRoute;

            public static class TestClass
            {
                public static string BuildQuery()
                {
                    var raw = new LocationQueryRaw
                    {
                        ["drop"] = Undefined<LocationQueryValueRaw?>(),
                        ["page"] = (Number)1,
                        ["tags"] = new LocationQueryValueRaw?[] { "a", Undefined<LocationQueryValueRaw?>(), null, (Number)3 }
                    };
                    var collectionQuery = new LocationQueryRaw
                    {
                        { "drop", Undefined<LocationQueryValueRaw?>() },
                        { "page", (Number)1 }
                    };

                    return StringifyQuery(raw) + StringifyQuery(collectionQuery);
                }
            }
            """;

        var (classSymbol, semanticModel) = CompileAndGetSymbol(
            code,
            "TestClass",
            MetadataReference.CreateFromFile(typeof(ECMAScript.Contract.IUIComponent).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(ECMAScript.Vue).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(ECMAScript.VueRoute).Assembly.Location));
        var converter = new AstConverter(classSymbol, semanticModel);

        var module = await converter.Convert(CancellationToken.None);
        var script = module?.ToKnRECMAScript();

        Assert.IsNotNull(script);
        StringAssert.Contains(script, "let raw = {");
        StringAssert.Contains(script, "drop: undefined,");
        StringAssert.Contains(script, "page: 1,");
        StringAssert.Contains(script, "tags: [\"a\", undefined, null, 3]");
        StringAssert.Contains(script, "let collectionQuery = { drop: undefined, page: 1 };");
        StringAssert.Contains(script, "stringifyQuery(raw)");
        StringAssert.Contains(script, "stringifyQuery(collectionQuery)");
    }

    [TestMethod]
    public async Task VueRoute_QueryContracts_Compile_WithDirectNullableStringArrayAuthoring()
    {
        var code = """
            using ECMAScript;
            using static ECMAScript.VueRoute;

            public static class TestClass
            {
                public static string BuildQuery()
                {
                    LocationQueryValue normalized = new string?[] { "a", null, "b" };
                    LocationQueryValueRaw rawValue = new string?[] { "x", null, "y" };
                    var raw = new LocationQueryRaw
                    {
                        ["tags"] = new string?[] { "a", null, "b" },
                        ["filter"] = rawValue
                    };

                    return StringifyQuery(raw) + normalized.AsArray;
                }
            }
            """;

        var (classSymbol, semanticModel) = CompileAndGetSymbol(
            code,
            "TestClass",
            MetadataReference.CreateFromFile(typeof(ECMAScript.Contract.IUIComponent).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(ECMAScript.Vue).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(ECMAScript.VueRoute).Assembly.Location));
        var converter = new AstConverter(classSymbol, semanticModel);

        var module = await converter.Convert(CancellationToken.None);
        var script = module?.ToKnRECMAScript();

        Assert.IsNotNull(script);
        StringAssert.Contains(script, "let normalized = [\"a\", null, \"b\"];");
        StringAssert.Contains(script, "let rawValue = [\"x\", null, \"y\"];");
        StringAssert.Contains(script, "tags: [\"a\", null, \"b\"]");
        StringAssert.Contains(script, "filter: rawValue");
        StringAssert.Contains(script, "stringifyQuery(raw)");
    }

    [TestMethod]
    public async Task VueRoute_ErasedValueUnionContractProjection_GeneratesNativeValue()
    {
        var code = """
            using ECMAScript;

            public static class TestClass
            {
                public static object? ReadHistoryStateValue(HistoryStateValue value)
                    => value.Value;

                public static object? ReadRouteParamRaw(RouteParamRaw value)
                    => value.Value;

                public static object? ReadLocationQueryValue(LocationQueryValue value)
                    => value.Value;

                public static object? ReadLocationQueryValueRaw(LocationQueryValueRaw value)
                    => value.Value;

                public static object? ReadScrollPositionTarget(ScrollPositionTarget value)
                    => value.Value;

                public static object? ReadRouterViewDepthValue(RouterViewDepthValue value)
                    => value.Value;

                public static object? ReadRouterScrollResult(RouterScrollResult value)
                    => value.Value;

                public static object? ReadRouterScrollHandler(RouterScrollHandler value)
                    => value.Value;

                public static object? ReadRouteRecordRaw(RouteRecordRaw value)
                    => value.Value;
            }
            """;

        var (classSymbol, semanticModel) = CompileAndGetSymbol(
            code,
            "TestClass",
            MetadataReference.CreateFromFile(typeof(ECMAScript.Contract.IUIComponent).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(ECMAScript.Vue.IVueComponent).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(ECMAScript.Vue).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(ECMAScript.VueRoute).Assembly.Location));
        var converter = new AstConverter(classSymbol, semanticModel);

        var module = await converter.Convert(CancellationToken.None);
        var script = module?.ToKnRECMAScript();

        Assert.IsNotNull(script);
        StringAssert.Contains(script, "export function ReadHistoryStateValue(value)");
        StringAssert.Contains(script, "export function ReadRouteParamRaw(value)");
        StringAssert.Contains(script, "export function ReadLocationQueryValue(value)");
        StringAssert.Contains(script, "export function ReadLocationQueryValueRaw(value)");
        StringAssert.Contains(script, "export function ReadScrollPositionTarget(value)");
        StringAssert.Contains(script, "export function ReadRouterViewDepthValue(value)");
        StringAssert.Contains(script, "export function ReadRouterScrollResult(value)");
        StringAssert.Contains(script, "export function ReadRouterScrollHandler(value)");
        StringAssert.Contains(script, "export function ReadRouteRecordRaw(value)");
        Assert.AreEqual(9, CountOccurrences(script, "return value;"));
    }

    [TestMethod]
    public async Task VueRoute_RouteMeta_Compile_WithNullableRecursiveValues_AndPropertyKeyAuthoring()
    {
        var code = """
            using ECMAScript;
            using static ECMAScript.Global;

            public static class TestClass
            {
                public static RouteRecordSingleView BuildRecord(RawRouteComponent component)
                {
                    var featureKey = Symbol.For("feature");
                    var nested = new RouteMeta
                    {
                        ["section"] = "admin",
                        ["fallback"] = null,
                        ["missing"] = Undefined<RouteMetaValue?>()
                    };
                    var tags = new RouteMetaValue?[] { "admin", null, Undefined<RouteMetaValue?>(), nested };
                    var meta = new RouteMeta
                    {
                        ["requiresAuth"] = true,
                        ["tags"] = tags,
                        ["layout"] = nested,
                        [(Number)7] = "priority",
                        [featureKey] = false
                    };
                    var collectionMeta = new RouteMeta
                    {
                        { "nullable", null },
                        { "missing", Undefined<RouteMetaValue?>() },
                        { (Number)8, "tenant" },
                        { featureKey, nested }
                    };

                    return new RouteRecordSingleView
                    {
                        Path = "/users",
                        Component = component,
                        Meta = new RouteMeta
                        {
                            ["primary"] = meta,
                            ["secondary"] = collectionMeta
                        }
                    };
                }
            }
            """;

        var (classSymbol, semanticModel) = CompileAndGetSymbol(
            code,
            "TestClass",
            MetadataReference.CreateFromFile(typeof(ECMAScript.Contract.IUIComponent).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(ECMAScript.Vue).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(ECMAScript.VueRoute).Assembly.Location));
        var converter = new AstConverter(classSymbol, semanticModel);

        var module = await converter.Convert(CancellationToken.None);
        var script = module?.ToKnRECMAScript();

        Assert.IsNotNull(script);
        StringAssert.Contains(script, "let featureKey = Symbol.for(\"feature\");");
        StringAssert.Contains(script, "let nested = {");
        StringAssert.Contains(script, "section: \"admin\"");
        StringAssert.Contains(script, "fallback: null");
        StringAssert.Contains(script, "missing: undefined");
        StringAssert.Contains(script, "let tags = [\"admin\", null, undefined, nested];");
        StringAssert.Contains(script, "let meta = {");
        StringAssert.Contains(script, "requiresAuth: true");
        StringAssert.Contains(script, "tags: tags");
        StringAssert.Contains(script, "layout: nested");
        StringAssert.Contains(script, "7: \"priority\"");
        StringAssert.Contains(script, "[featureKey]: false");
        StringAssert.Contains(script, "let collectionMeta = {");
        StringAssert.Contains(script, "nullable: null");
        StringAssert.Contains(script, "missing: undefined");
        StringAssert.Contains(script, "8: \"tenant\"");
        StringAssert.Contains(script, "[featureKey]: nested");
        StringAssert.Contains(script, "meta: { primary: meta, secondary: collectionMeta }");
    }

    [TestMethod]
    public async Task VueRoute_RouteMeta_Compiles_WithCallbackAuthoring_InCollectionAndIndexerForms()
    {
        var code = """
            using System;
            using ECMAScript;

            public static class TestClass
            {
                public static RouteMeta BuildMeta(Symbol featureKey)
                {
                    var callbackMeta = new RouteMeta
                    {
                        { "onEnter", () => { } },
                        { (Number)9, () => { } },
                        { featureKey, () => { } }
                    };

                    callbackMeta["onLeave"] = RouteMetaValue.From(() => { });

                    return callbackMeta;
                }
            }
            """;

        var (classSymbol, semanticModel) = CompileAndGetSymbol(
            code,
            "TestClass",
            MetadataReference.CreateFromFile(typeof(ECMAScript.Contract.IUIComponent).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(ECMAScript.Vue).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(ECMAScript.VueRoute).Assembly.Location));
        var converter = new AstConverter(classSymbol, semanticModel);

        var module = await converter.Convert(CancellationToken.None);
        var script = module?.ToKnRECMAScript();

        Assert.IsNotNull(script);
        StringAssert.Contains(script, "let callbackMeta = {");
        StringAssert.Contains(script, "onEnter: () => {");
        StringAssert.Contains(script, "9: () => {");
        StringAssert.Contains(script, "[featureKey]: () => {");
        StringAssert.Contains(script, "callbackMeta[\"onLeave\"] = () => {");
        StringAssert.Contains(script, "return callbackMeta;");
    }

    [TestMethod]
    public async Task VueRoute_RouteParamsRaw_Compile_WithMixedScalarArrays_AndExplicitNullSingleValue()
    {
        var code = """
            using ECMAScript;
            using static ECMAScript.VueRoute;

            public static class TestClass
            {
                public static RouteLocationAsRelative BuildLocation()
                {
                    var rawParams = new RouteParamsRaw
                    {
                        ["id"] = (Number)7,
                        ["slug"] = null,
                        ["segments"] = new RouteParamRaw[] { "users", (Number)42 }
                    };

                    return new RouteLocationAsRelative
                    {
                        Name = "user-detail",
                        Params = rawParams
                    };
                }
            }
            """;

        var (classSymbol, semanticModel) = CompileAndGetSymbol(
            code,
            "TestClass",
            MetadataReference.CreateFromFile(typeof(ECMAScript.Contract.IUIComponent).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(ECMAScript.Vue).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(ECMAScript.VueRoute).Assembly.Location));
        var converter = new AstConverter(classSymbol, semanticModel);

        var module = await converter.Convert(CancellationToken.None);
        var script = module?.ToKnRECMAScript();

        Assert.IsNotNull(script);
        StringAssert.Contains(script, "let rawParams = {");
        StringAssert.Contains(script, "id: 7");
        StringAssert.Contains(script, "slug: null");
        StringAssert.Contains(script, "segments: [\"users\", 42]");
        StringAssert.Contains(script, "return { name: \"user-detail\", params: rawParams };");
    }

    [TestMethod]
    public async Task VueRoute_RouterOnError_Compile_WithExplicitStronglyTypedOverloads()
    {
        var code = """
            using ECMAScript;
            using static ECMAScript.VueRoute;

            public static class TestClass
            {
                public static string RegisterHandlers(Router router, IObject payload)
                {
                    var stopError = router.OnError((Error error, RouteLocationNormalized to, RouteLocationNormalizedLoaded from) =>
                    {
                        _ = error.Message;
                        _ = to.FullPath;
                        _ = from.FullPath;
                    });
                    var stopFailure = router.OnError((NavigationFailure error, RouteLocationNormalized to, RouteLocationNormalizedLoaded from) =>
                    {
                        _ = error.Type;
                        _ = error.To.Path;
                        _ = error.From.Path;
                    });
                    var stopRedirect = router.OnError((NavigationRedirectError error, RouteLocationNormalized to, RouteLocationNormalizedLoaded from) =>
                    {
                        _ = error.Type;
                        _ = error.To.AsString;
                        _ = error.To.AsPath?.Path;
                        _ = error.From.Path;
                    });
                    var stopString = router.OnError((string error, RouteLocationNormalized to, RouteLocationNormalizedLoaded from) =>
                    {
                        _ = error.Length;
                        _ = to.Hash;
                        _ = from.Hash;
                    });
                    var stopNumber = router.OnError((Number error, RouteLocationNormalized to, RouteLocationNormalizedLoaded from) =>
                    {
                        _ = error.ToString();
                    });
                    var stopBoolean = router.OnError((bool error, RouteLocationNormalized to, RouteLocationNormalizedLoaded from) =>
                    {
                        _ = error;
                    });
                    var stopBigInt = router.OnError((BigInt error, RouteLocationNormalized to, RouteLocationNormalizedLoaded from) =>
                    {
                        _ = error.ToString();
                    });
                    var stopSymbol = router.OnError((Symbol error, RouteLocationNormalized to, RouteLocationNormalizedLoaded from) =>
                    {
                        _ = error.Description;
                    });
                    var stopObject = router.OnError((IObject error, RouteLocationNormalized to, RouteLocationNormalizedLoaded from) =>
                    {
                        _ = error["code"];
                        _ = payload["fallback"];
                    });
                    var stopArray = router.OnError((Array<RouterErrorValue?> error, RouteLocationNormalized to, RouteLocationNormalizedLoaded from) =>
                    {
                        var first = error[(Number)0];
                        _ = first?.AsError?.Message;
                        _ = first?.AsString;
                        _ = first?.AsObject?["detail"];
                        _ = first?.AsArray?[(Number)0];
                    });

                    return "ok";
                }
            }
            """;

        var (classSymbol, semanticModel) = CompileAndGetSymbol(
            code,
            "TestClass",
            MetadataReference.CreateFromFile(typeof(ECMAScript.Contract.IUIComponent).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(ECMAScript.Vue).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(ECMAScript.VueRoute).Assembly.Location));
        var converter = new AstConverter(classSymbol, semanticModel);

        var module = await converter.Convert(CancellationToken.None);
        var script = module?.ToKnRECMAScript();

        Assert.IsNotNull(script);
        StringAssert.Contains(script, "let stopError = router.onError((error, to, from) => {");
        StringAssert.Contains(script, "error.message");
        StringAssert.Contains(script, "error.type");
        StringAssert.Contains(script, "error.to.path");
        StringAssert.Contains(script, "error.from.path");
        StringAssert.Contains(script, "let stopRedirect = router.onError((error, to, from) => {");
        StringAssert.Contains(script, "error.to");
        StringAssert.Contains(script, "error.length");
        StringAssert.Contains(script, "error.description");
        StringAssert.Contains(script, "error[\"code\"]");
        StringAssert.Contains(script, "payload[\"fallback\"]");
        StringAssert.Contains(script, "let first = error[0];");
        StringAssert.Contains(script, "first?.message");
        StringAssert.Contains(script, "first?.[\"detail\"]");
        StringAssert.Contains(script, "first?.[0]");
        StringAssert.Contains(script, "return \"ok\";");
    }

    [TestMethod]
    public async Task VueRoute_RouterBeforeEach_AsyncGuard_Compiles_ToBeforeEach_NotBeforeResolve()
    {
        var code = """
            using ECMAScript;

            public static class TestClass
            {
                public static string Register(Router router)
                {
                    var stop = router.BeforeEach((RouteLocationNormalized to, RouteLocationNormalizedLoaded from) =>
                    {
                        _ = to.FullPath;
                        _ = from.FullPath;
                        return Promise<NavigationGuardReturn?>.Resolve(true);
                    });

                    stop();
                    return "ok";
                }
            }
            """;

        var (classSymbol, semanticModel) = CompileAndGetSymbol(
            code,
            "TestClass",
            MetadataReference.CreateFromFile(typeof(ECMAScript.Contract.IUIComponent).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(ECMAScript.Vue).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(ECMAScript.VueRoute).Assembly.Location));
        var converter = new AstConverter(classSymbol, semanticModel);

        var module = await converter.Convert(CancellationToken.None);
        var script = module?.ToKnRECMAScript();

        Assert.IsNotNull(script);
        StringAssert.Contains(script, "let stop = router.beforeEach((to, from) => {");
        Assert.IsFalse(script.Contains("router.beforeResolve((to, from) => {"), script);
        StringAssert.Contains(script, "return Promise.resolve(true);");
        StringAssert.Contains(script, "stop();");
        StringAssert.Contains(script, "return \"ok\";");
    }

    [TestMethod]
    public void VueRoute_RouterOnError_Compile_RejectsImplicitLambdaWhenOverloadIsAmbiguous()
    {
        var code = """
            using ECMAScript;
            using static ECMAScript.VueRoute;

            public static class TestClass
            {
                public static System.Action RegisterHandler(Router router)
                {
                    return router.OnError((error, to, from) =>
                    {
                        _ = to.FullPath;
                        _ = from.FullPath;
                    });
                }
            }
            """;

        var compilation = CSharpCompilation.Create(
            "ECMAScript.VueRoute.Test.Assembly.AmbiguousOnError",
            BuildSyntaxTrees(code),
            BuildCompilationReferences(new[]
            {
                MetadataReference.CreateFromFile(typeof(ECMAScript.Contract.IUIComponent).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(ECMAScript.Vue).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(ECMAScript.VueRoute).Assembly.Location)
            }),
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        var diagnostics = compilation.GetDiagnostics()
            .Where(static diagnostic => diagnostic.Severity == DiagnosticSeverity.Error)
            .ToArray();

        Assert.IsTrue(diagnostics.Any(static diagnostic => diagnostic.Id == "CS0121"),
            string.Join(Environment.NewLine, diagnostics.Select(static diagnostic => diagnostic.ToString())));
    }

    [TestMethod]
    public void VueRoute_LegacyNextGuardSurface_Compile_EmitsObsoleteWarnings_ButStaysCompatible()
    {
        var code = """
            using ECMAScript;
            using static ECMAScript.VueRoute;

            public static class TestClass
            {
                public static void Register(Router router, RawRouteComponent component)
                {
                    var legacy = new LegacyRouteNavigationGuard((RouteLocationNormalized to, RouteLocationNormalizedLoaded from, NavigationGuardNext next) =>
                    {
                        next(NavigationGuardNextArgument.From((Vue.VueComponentPublicInstance instance) =>
                        {
                            _ = instance;
                        }));
                        return true;
                    });

                    router.BeforeEach(legacy);
                    router.BeforeResolve((RouteLocationNormalized to, RouteLocationNormalizedLoaded from, NavigationGuardNext next) =>
                    {
                        next();
                        return true;
                    });

                    _ = new RouteRecordSingleView
                    {
                        Path = "/users",
                        Component = component,
                        BeforeEnter = RouteRecordBeforeEnter.From(legacy)
                    };
                }
            }
            """;

        var compilation = CSharpCompilation.Create(
            "ECMAScript.VueRoute.Test.Assembly.LegacyNextWarnings",
            BuildSyntaxTrees(code),
            BuildCompilationReferences(new[]
            {
                MetadataReference.CreateFromFile(typeof(ECMAScript.Contract.IUIComponent).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(ECMAScript.Vue).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(ECMAScript.VueRoute).Assembly.Location)
            }),
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        var errors = compilation.GetDiagnostics()
            .Where(static diagnostic => diagnostic.Severity == DiagnosticSeverity.Error)
            .ToArray();
        var warnings = compilation.GetDiagnostics()
            .Where(static diagnostic => diagnostic.Severity == DiagnosticSeverity.Warning)
            .ToArray();

        Assert.IsFalse(errors.Length > 0, string.Join(Environment.NewLine, errors.Select(static diagnostic => diagnostic.ToString())));
        Assert.IsTrue(warnings.Any(static diagnostic => diagnostic.Id == "CS0618"),
            string.Join(Environment.NewLine, warnings.Select(static diagnostic => diagnostic.ToString())));
        Assert.IsTrue(warnings.Any(static diagnostic => diagnostic.GetMessage().Contains("return-based navigation guards", StringComparison.Ordinal)));
        Assert.IsTrue(warnings.Any(static diagnostic => diagnostic.GetMessage().Contains("beforeRouteEnter", StringComparison.Ordinal)));
    }

    [TestMethod]
    public void VueRoute_EndOptionSurface_Compile_EmitsObsoleteWarnings_ButStaysCompatible()
    {
        var code = """
            using ECMAScript;
            using static ECMAScript.VueRoute;

            public static class TestClass
            {
                public static RouterOptions BuildOptions(RouterHistory history, RawRouteComponent component)
                {
                    return new RouterOptions
                    {
                        History = history,
                        End = true,
                        Routes =
                        [
                            new RouteRecordSingleView
                            {
                                Path = "/users",
                                Component = component,
                                End = false
                            }
                        ],
                        ScrollBehavior = false ? null : default
                    };
                }

                public static RouterMatcher BuildMatcher()
                {
                    return CreateRouterMatcher(
                    [
                        new RouteRecordRedirect
                        {
                            Path = "/",
                            Redirect = "/home"
                        }
                    ],
                    new PathParserOptions
                    {
                        Strict = true,
                        End = true
                    });
                }
            }
            """;

        var compilation = CSharpCompilation.Create(
            "ECMAScript.VueRoute.Test.Assembly.EndWarnings",
            BuildSyntaxTrees(code),
            BuildCompilationReferences(new[]
            {
                MetadataReference.CreateFromFile(typeof(ECMAScript.Contract.IUIComponent).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(ECMAScript.Vue.IVueComponent).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(ECMAScript.Vue).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(ECMAScript.VueRoute).Assembly.Location)
            }),
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        var errors = compilation.GetDiagnostics()
            .Where(static diagnostic => diagnostic.Severity == DiagnosticSeverity.Error)
            .ToArray();
        var warnings = compilation.GetDiagnostics()
            .Where(static diagnostic => diagnostic.Severity == DiagnosticSeverity.Warning)
            .ToArray();

        Assert.IsFalse(errors.Length > 0, string.Join(Environment.NewLine, errors.Select(static diagnostic => diagnostic.ToString())));
        Assert.IsTrue(warnings.Any(static diagnostic => diagnostic.Id == "CS0618"),
            string.Join(Environment.NewLine, warnings.Select(static diagnostic => diagnostic.ToString())));
        Assert.IsTrue(warnings.Any(static diagnostic => diagnostic.GetMessage().Contains("always true", StringComparison.OrdinalIgnoreCase)),
            string.Join(Environment.NewLine, warnings.Select(static diagnostic => diagnostic.ToString())));
    }

    [TestMethod]
    public async Task VueRoute_LocationQueryRaw_AndHistoryState_Compile_WithNumericLiteralKeys()
    {
        var code = """
            using ECMAScript;
            using static ECMAScript.VueRoute;

            public static class TestClass
            {
                public static string BuildStateAndQuery()
                {
                    var query = new LocationQueryRaw
                    {
                        [(Number)1] = "page",
                        [(Number)2] = (Number)7
                    };
                    var state = new HistoryState
                    {
                        [(Number)10] = "root",
                        [(Number)11] = (Number)99
                    };

                    return StringifyQuery(query) + state[(Number)10] + state[(Number)11];
                }
            }
            """;

        var (classSymbol, semanticModel) = CompileAndGetSymbol(
            code,
            "TestClass",
            MetadataReference.CreateFromFile(typeof(ECMAScript.Contract.IUIComponent).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(ECMAScript.Vue).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(ECMAScript.VueRoute).Assembly.Location));
        var converter = new AstConverter(classSymbol, semanticModel);

        var module = await converter.Convert(CancellationToken.None);
        var script = module?.ToKnRECMAScript();

        Assert.IsNotNull(script);
        StringAssert.Contains(script, "let query = { 1: \"page\", 2: 7 };");
        StringAssert.Contains(script, "let state = { 10: \"root\", 11: 99 };");
        StringAssert.Contains(script, "stringifyQuery(query)");
        StringAssert.Contains(script, "state[10]");
        StringAssert.Contains(script, "state[11]");
    }

    [TestMethod]
    public async Task VueRoute_LocationQueryRaw_Compile_WithCollectionInitializerNullEntries()
    {
        var code = """
            using ECMAScript;
            using static ECMAScript.VueRoute;

            public static class TestClass
            {
                public static string BuildQuery()
                {
                    var query = new LocationQueryRaw
                    {
                        { "flag", null },
                        { "page", (Number)1 }
                    };

                    return StringifyQuery(query);
                }
            }
            """;

        var (classSymbol, semanticModel) = CompileAndGetSymbol(
            code,
            "TestClass",
            MetadataReference.CreateFromFile(typeof(ECMAScript.Contract.IUIComponent).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(ECMAScript.Vue).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(ECMAScript.VueRoute).Assembly.Location));
        var converter = new AstConverter(classSymbol, semanticModel);

        var module = await converter.Convert(CancellationToken.None);
        var script = module?.ToKnRECMAScript();

        Assert.IsNotNull(script);
        StringAssert.Contains(script, "let query = { flag: null, page: 1 };");
        StringAssert.Contains(script, "stringifyQuery(query)");
    }

    [TestMethod]
    public async Task VueRoute_HistoryState_Compile_WithCollectionInitializerNullEntries()
    {
        var code = """
            using ECMAScript;

            public static class TestClass
            {
                public static string BuildState()
                {
                    var state = new HistoryState
                    {
                        { "source", "router" },
                        { "empty", null }
                    };

                    return "ok";
                }
            }
            """;

        var (classSymbol, semanticModel) = CompileAndGetSymbol(
            code,
            "TestClass",
            MetadataReference.CreateFromFile(typeof(ECMAScript.Contract.IUIComponent).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(ECMAScript.Vue).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(ECMAScript.VueRoute).Assembly.Location));
        var converter = new AstConverter(classSymbol, semanticModel);

        var module = await converter.Convert(CancellationToken.None);
        var script = module?.ToKnRECMAScript();

        Assert.IsNotNull(script);
        StringAssert.Contains(script, "let state = { source: \"router\", empty: null };");
    }

    [TestMethod]
    public async Task VueRoute_HistoryState_Compile_WithCollectionInitializerNumericKeys()
    {
        var code = """
            using ECMAScript;

            public static class TestClass
            {
                public static string BuildState()
                {
                    var state = new HistoryState
                    {
                        { (Number)7, (Number)9 }
                    };

                    return "ok";
                }
            }
            """;

        var (classSymbol, semanticModel) = CompileAndGetSymbol(
            code,
            "TestClass",
            MetadataReference.CreateFromFile(typeof(ECMAScript.Contract.IUIComponent).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(ECMAScript.Vue).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(ECMAScript.VueRoute).Assembly.Location));
        var converter = new AstConverter(classSymbol, semanticModel);

        var module = await converter.Convert(CancellationToken.None);
        var script = module?.ToKnRECMAScript();

        Assert.IsNotNull(script);
        StringAssert.Contains(script, "let state = { 7: 9 };");
    }

    [TestMethod]
    public async Task VueRoute_RouteParamsRaw_Compile_WithCollectionInitializerNullEntries()
    {
        var code = """
            using ECMAScript;

            public static class TestClass
            {
                public static string BuildParams()
                {
                    var paramsRaw = new RouteParamsRaw
                    {
                        { "slug", null },
                        { "segments", new RouteParamRaw[] { "users", (Number)42 } }
                    };

                    return "ok";
                }
            }
            """;

        var (classSymbol, semanticModel) = CompileAndGetSymbol(
            code,
            "TestClass",
            MetadataReference.CreateFromFile(typeof(ECMAScript.Contract.IUIComponent).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(ECMAScript.Vue).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(ECMAScript.VueRoute).Assembly.Location));
        var converter = new AstConverter(classSymbol, semanticModel);

        var module = await converter.Convert(CancellationToken.None);
        var script = module?.ToKnRECMAScript();

        Assert.IsNotNull(script);
        StringAssert.Contains(script, "let paramsRaw = { slug: null, segments: [\"users\", 42] };");
    }

    [TestMethod]
    public async Task VueRoute_RouteComponentInstanceMap_Compile_WithCollectionInitializerNullEntries()
    {
        var code = """
            using ECMAScript;

            public static class TestClass
            {
                public static RouteComponentInstanceMap BuildInstances()
                {
                    var instances = new RouteComponentInstanceMap
                    {
                        { "default", null }
                    };

                    return instances;
                }
            }
            """;

        var (classSymbol, semanticModel) = CompileAndGetSymbol(
            code,
            "TestClass",
            MetadataReference.CreateFromFile(typeof(ECMAScript.Contract.IUIComponent).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(ECMAScript.Vue).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(ECMAScript.VueRoute).Assembly.Location));
        var converter = new AstConverter(classSymbol, semanticModel);

        var module = await converter.Convert(CancellationToken.None);
        var script = module?.ToKnRECMAScript();

        Assert.IsNotNull(script);
        StringAssert.Contains(script, "let instances = { default: null };");
        StringAssert.Contains(script, "return instances;");
    }

    [TestMethod]
    public async Task VueRoute_UseLinkMaybeRefContracts_Compile_WithReactiveTargets_AndStringEnumProps()
    {
        var code = """
            using ECMAScript;
            using static ECMAScript.Vue;
            using static ECMAScript.VueRoute;

            public static class TestClass
            {
                public static IPromise<RouteNavigationResult?> BuildLink()
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
            MetadataReference.CreateFromFile(typeof(ECMAScript.Vue).Assembly.Location),
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
    public async Task VueRoute_UseLinkMaybeRefContracts_Compile_WithDirectReadonlyRefAssignments()
    {
        var code = """
            using ECMAScript;
            using static ECMAScript.Vue;
            using static ECMAScript.VueRoute;

            public static class TestClass
            {
                public static IPromise<RouteNavigationResult?> BuildLink()
                {
                    var toReadonly = ToRef(() => new RouteLocationAsRelative
                    {
                        Name = "users"
                    });
                    var replaceReadonly = Computed(() => true);
                    var link = UseLink(new UseLinkOptions
                    {
                        To = toReadonly,
                        Replace = replaceReadonly,
                        ViewTransition = true
                    });

                    return link.Navigate();
                }
            }
            """;

        var (classSymbol, semanticModel) = CompileAndGetSymbol(
            code,
            "TestClass",
            MetadataReference.CreateFromFile(typeof(ECMAScript.Contract.IUIComponent).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(ECMAScript.Vue).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(ECMAScript.VueRoute).Assembly.Location));
        var converter = new AstConverter(classSymbol, semanticModel);

        var module = await converter.Convert(CancellationToken.None);
        var script = module?.ToKnRECMAScript();

        Assert.IsNotNull(script);
        StringAssert.Contains(script, "let toReadonly = toRef(() => {");
        StringAssert.Contains(script, "return { name: \"users\" };");
        StringAssert.Contains(script, "let replaceReadonly = computed(() => {");
        StringAssert.Contains(script, "let link = useLink({");
        StringAssert.Contains(script, "to: toReadonly,");
        StringAssert.Contains(script, "replace: replaceReadonly,");
        StringAssert.Contains(script, "return link.navigate();");
    }

    [TestMethod]
    public async Task VueRoute_UseLinkMaybeRefContracts_Compile_WithDirectWritableRefAssignments()
    {
        var code = """
            using ECMAScript;
            using static ECMAScript.Vue;
            using static ECMAScript.VueRoute;

            public static class TestClass
            {
                public static IPromise<RouteNavigationResult?> BuildLink()
                {
                    var toRef = Ref(new RouteLocationAsRelative
                    {
                        Name = "users"
                    });
                    var replaceRef = Ref(true);
                    var link = UseLink(new UseLinkOptions
                    {
                        To = toRef,
                        Replace = replaceRef,
                        ViewTransition = true
                    });

                    return link.Navigate();
                }
            }
            """;

        var (classSymbol, semanticModel) = CompileAndGetSymbol(
            code,
            "TestClass",
            MetadataReference.CreateFromFile(typeof(ECMAScript.Contract.IUIComponent).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(ECMAScript.Vue).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(ECMAScript.VueRoute).Assembly.Location));
        var converter = new AstConverter(classSymbol, semanticModel);

        var module = await converter.Convert(CancellationToken.None);
        var script = module?.ToKnRECMAScript();

        Assert.IsNotNull(script);
        StringAssert.Contains(script, "let toRef = ref({ name: \"users\" });");
        StringAssert.Contains(script, "let replaceRef = ref(true);");
        StringAssert.Contains(script, "let link = useLink({");
        StringAssert.Contains(script, "to: toRef,");
        StringAssert.Contains(script, "replace: replaceRef,");
        StringAssert.Contains(script, "viewTransition: true");
        StringAssert.Contains(script, "return link.navigate();");
    }

    [TestMethod]
    public async Task VueRoute_StronglyTypedDelegateVariables_Compile_WithoutFrom_WhenCSharpAlreadyExpressesTheContract()
    {
        var code = """
            using ECMAScript;
            using static ECMAScript.VueRoute;

            public sealed record TestViewProps : ECMAScript.Vue.VueProps
            {
            }

            public static class TestClass
            {
                public static RouterOptions BuildOptions(RouterHistory history, RawRouteComponent component)
                {
                    RouteRecordPropsResolver propsResolver = (RouteLocationNormalized to) => new TestViewProps();
                    RouteRedirectCallback redirectCallback = (RouteLocation to, RouteLocationNormalizedLoaded from) => "/home";
                    RouterScrollBehavior scrollBehavior = (RouteLocationNormalized to, RouteLocationNormalizedLoaded from, ScrollPositionNormalized? savedPosition) => false;

                    return new RouterOptions
                    {
                        History = history,
                        Routes =
                        [
                            new RouteRecordSingleView
                            {
                                Path = "/users",
                                Component = component,
                                Props = propsResolver
                            },
                            new RouteRecordRedirect
                            {
                                Path = "/legacy",
                                Redirect = redirectCallback
                            }
                        ],
                        ScrollBehavior = scrollBehavior
                    };
                }

                public static bool RunLegacyNext(NavigationGuardNext next)
                {
                    NavigationGuardNextCallback callback = (Vue.VueComponentPublicInstance instance) =>
                    {
                        _ = instance;
                    };
                    next(callback);
                    return true;
                }
            }
            """;

        var (classSymbol, semanticModel) = CompileAndGetSymbol(
            code,
            "TestClass",
            MetadataReference.CreateFromFile(typeof(ECMAScript.Contract.IUIComponent).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(ECMAScript.Vue).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(ECMAScript.VueRoute).Assembly.Location));
        var converter = new AstConverter(classSymbol, semanticModel);

        var module = await converter.Convert(CancellationToken.None);
        var script = module?.ToKnRECMAScript();

        Assert.IsNotNull(script);
        StringAssert.Contains(script, "props: propsResolver");
        StringAssert.Contains(script, "redirect: redirectCallback");
        StringAssert.Contains(script, "scrollBehavior: scrollBehavior");
        StringAssert.Contains(script, "next(callback);");
    }

    [TestMethod]
    public void VueRoute_DirectLambdaUnionAssignments_Reject_WhenCSharpCannotBind_AndRequireExplicitFactories()
    {
        var code = """
            using ECMAScript;
            using static ECMAScript.VueRoute;

            public sealed record TestViewProps : ECMAScript.Vue.VueProps
            {
            }

            public static class TestClass
            {
                public static RouterOptions BuildOptions(RouterHistory history, RawRouteComponent component)
                {
                    NavigationGuardNext next = null!;

                    next((Vue.VueComponentPublicInstance instance) =>
                    {
                        _ = instance;
                    });

                    return new RouterOptions
                    {
                        History = history,
                        Routes =
                        [
                            new RouteRecordSingleView
                            {
                                Path = "/users",
                                Component = component,
                                Props = (RouteLocationNormalized to) => new TestViewProps()
                            },
                            new RouteRecordRedirect
                            {
                                Path = "/legacy",
                                Redirect = (RouteLocation to, RouteLocationNormalizedLoaded from) => "/home"
                            }
                        ],
                        ScrollBehavior = (RouteLocationNormalized to, RouteLocationNormalizedLoaded from, ScrollPositionNormalized? savedPosition) => false
                    };
                }
            }
            """;

        var compilation = CSharpCompilation.Create(
            "ECMAScript.VueRoute.Test.Assembly.DirectLambdaUnionAssignments",
            BuildSyntaxTrees(code),
            BuildCompilationReferences(new[]
            {
                MetadataReference.CreateFromFile(typeof(ECMAScript.Contract.IUIComponent).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(ECMAScript.Vue).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(ECMAScript.VueRoute).Assembly.Location)
            }),
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        var diagnostics = compilation.GetDiagnostics()
            .Where(static diagnostic => diagnostic.Severity == DiagnosticSeverity.Error)
            .ToArray();

        Assert.IsTrue(diagnostics.Any(static diagnostic =>
                diagnostic.Id is "CS1660" or "CS1503" or "CS0029"),
            string.Join(Environment.NewLine, diagnostics.Select(static diagnostic => diagnostic.ToString())));
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
            MetadataReference.CreateFromFile(typeof(ECMAScript.Vue).Assembly.Location),
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
    public async Task VueRoute_SlotCallbacks_Compile_WithVNodeArrayReturns_AndCurrentRouteRefValue()
    {
        var code = """
            using ECMAScript;
            using static ECMAScript.Vue;
            using static ECMAScript.VueRoute;

            public static class TestClass
            {
                public static RouterLinkSlots BuildLinkSlots()
                {
                    return new RouterLinkSlots
                    {
                        Default = scope => new IVNode[]
                        {
                            H("a", new VueObject
                            {
                                Href = scope.Href
                            }, new IVNode[]
                            {
                                H("span", scope.Route.Path)
                            })
                        }
                    };
                }

                public static RouterViewSlots BuildViewSlots()
                {
                    return new RouterViewSlots
                    {
                        Default = scope => new IVNode[]
                        {
                            H("section", new IVNode[]
                            {
                                H("div", scope.Route.Path)
                            })
                        }
                    };
                }

                public static string ReadCurrentRoute(Router router)
                {
                    return router.CurrentRoute.Value.FullPath;
                }
            }
            """;

        var (classSymbol, semanticModel) = CompileAndGetSymbol(
            code,
            "TestClass",
            MetadataReference.CreateFromFile(typeof(ECMAScript.Contract.IUIComponent).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(ECMAScript.Vue.IVueComponent).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(ECMAScript.Vue).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(ECMAScript.VueRoute).Assembly.Location));
        var converter = new AstConverter(classSymbol, semanticModel);

        var module = await converter.Convert(CancellationToken.None);
        var script = module?.ToKnRECMAScript();

        Assert.IsNotNull(script);
        StringAssert.Contains(script, "default: scope => {");
        StringAssert.Contains(script, "return [h(\"a\"");
        StringAssert.Contains(script, "href: scope.href");
        StringAssert.Contains(script, "scope.route.path");
        StringAssert.Contains(script, "return [h(\"section\"");
        StringAssert.Contains(script, "router.currentRoute.value.fullPath");
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
            MetadataReference.CreateFromFile(typeof(ECMAScript.Vue).Assembly.Location),
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

    [TestMethod]
    public async Task VueRoute_RouterHistory_Compile_WithOptionalStateOverloads()
    {
        var code = """
            using ECMAScript;

            public static class TestClass
            {
                public static string ConfigureHistory(RouterHistory history)
                {
                    history.Push("/users");
                    history.Replace("/users/7");

                    return history.CreateHref("/users/7");
                }
            }
            """;

        var (classSymbol, semanticModel) = CompileAndGetSymbol(
            code,
            "TestClass",
            MetadataReference.CreateFromFile(typeof(ECMAScript.Contract.IUIComponent).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(ECMAScript.Vue).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(ECMAScript.VueRoute).Assembly.Location));
        var converter = new AstConverter(classSymbol, semanticModel);

        var module = await converter.Convert(CancellationToken.None);
        var script = module?.ToKnRECMAScript();

        Assert.IsNotNull(script);
        StringAssert.Contains(script, "history.push(\"/users\");");
        StringAssert.Contains(script, "history.replace(\"/users/7\");");
        StringAssert.Contains(script, "return history.createHref(\"/users/7\");");
    }

    [TestMethod]
    public async Task VueRoute_ReactiveRefContracts_Compile_WithExplicitComputedAndShallowRefTypes()
    {
        var code = """
            using ECMAScript;
            using static ECMAScript.Vue;
            using static ECMAScript.VueRoute;

            public static class TestClass
            {
                public static string ReadRefs(Router router)
                {
                    VueComputedRef<bool> computedFlag = Computed(() => true);
                    VueComputedRef<RouteLocationAsRelative> computedLocation = ToRef(() => new RouteLocationAsRelative
                    {
                        Name = "users"
                    });
                    VueShallowRef<RouteLocationNormalizedLoaded> currentRoute = router.CurrentRoute;
                    var link = UseLink(new UseLinkOptions
                    {
                        To = computedLocation,
                        Replace = computedFlag
                    });

                    TriggerRef(currentRoute);
                    return currentRoute.Value.Path + link.Href.Value + link.IsActive.Value + link.IsExactActive.Value + link.Route.Value.Href;
                }
            }
            """;

        var (classSymbol, semanticModel) = CompileAndGetSymbol(
            code,
            "TestClass",
            MetadataReference.CreateFromFile(typeof(ECMAScript.Contract.IUIComponent).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(ECMAScript.Vue).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(ECMAScript.VueRoute).Assembly.Location));
        var converter = new AstConverter(classSymbol, semanticModel);

        var module = await converter.Convert(CancellationToken.None);
        var script = module?.ToKnRECMAScript();

        Assert.IsNotNull(script);
        StringAssert.Contains(script, "let computedFlag = computed(() => {");
        StringAssert.Contains(script, "return true;");
        StringAssert.Contains(script, "let computedLocation = toRef(() => {");
        StringAssert.Contains(script, "return { name: \"users\" };");
        StringAssert.Contains(script, "let currentRoute = router.currentRoute;");
        StringAssert.Contains(script, "let link = useLink({");
        StringAssert.Contains(script, "to: computedLocation,");
        StringAssert.Contains(script, "replace: computedFlag");
        StringAssert.Contains(script, "triggerRef(currentRoute);");
        StringAssert.Contains(script, "currentRoute.value.path");
        StringAssert.Contains(script, "link.href.value");
        StringAssert.Contains(script, "link.isActive.value");
        StringAssert.Contains(script, "link.isExactActive.value");
        StringAssert.Contains(script, "link.route.value.href");
    }

    [TestMethod]
    public async Task VueRoute_InjectionAndReactiveContracts_Compile_WithTypedInjectionKeysAndLoadRouteLocation()
    {
        var code = """
            using ECMAScript;
            using static ECMAScript.Vue;
            using static ECMAScript.VueRoute;

            public static class TestClass
            {
                public static string Build(
                    Router router,
                    RouteLocation routeLocation,
                    RouteLocationNormalized normalized,
                    VueShallowRef<RouteLocationNormalizedLoaded> routeRef)
                {
                    VueComputedRef<RouteRecordNormalized?> matched = Computed(() => normalized.Matched[0]);

                    Provide(VueRoute.RouterKey, router);
                    Provide(VueRoute.RouteLocationKey, UseRoute());
                    Provide(VueRoute.RouterViewLocationKey, routeRef);
                    Provide(VueRoute.MatchedRouteKey, matched);
                    Provide(VueRoute.ViewDepthKey, 1);

                    var injectedRouter = Inject(VueRoute.RouterKey)!;
                    var injectedRoute = Inject(VueRoute.RouteLocationKey)!;
                    var injectedRouteRef = Inject(VueRoute.RouterViewLocationKey)!;
                    var injectedMatched = Inject(VueRoute.MatchedRouteKey)!;
                    var injectedDepth = Inject(VueRoute.ViewDepthKey)!;

                    var loadedFromLocation = LoadRouteLocation(routeLocation);
                    var loadedFromNormalized = LoadRouteLocation(normalized);
                    var link = UseLink(new UseLinkOptions
                    {
                        To = ToRef(() => new RouteLocationAsRelative
                        {
                            Name = injectedRoute.Name!
                        }),
                        Replace = Computed(() => true)
                    });

                    TriggerRef(routeRef);
                    return injectedRouter.CurrentRoute.Value.Path
                        + injectedRoute.Path
                        + injectedRouteRef.Value.Path
                        + injectedMatched.Value!.Path
                        + injectedDepth.AsNumber!.ToString()
                        + link.Href.Value
                        + link.Route.Value.Href
                        + loadedFromLocation.ToString()
                        + loadedFromNormalized.ToString();
                }
            }
            """;

        var (classSymbol, semanticModel) = CompileAndGetSymbol(
            code,
            "TestClass",
            MetadataReference.CreateFromFile(typeof(ECMAScript.Contract.IUIComponent).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(ECMAScript.Vue).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(ECMAScript.VueRoute).Assembly.Location));
        var converter = new AstConverter(classSymbol, semanticModel);

        var module = await converter.Convert(CancellationToken.None);
        var script = module?.ToKnRECMAScript();

        Assert.IsNotNull(script);
        StringAssert.Contains(script, "let matched = computed(() => {");
        StringAssert.Contains(script, "provide(routerKey, router);");
        StringAssert.Contains(script, "provide(routeLocationKey, useRoute());");
        StringAssert.Contains(script, "provide(routerViewLocationKey, routeRef);");
        StringAssert.Contains(script, "provide(matchedRouteKey, matched);");
        StringAssert.Contains(script, "provide(viewDepthKey, 1);");
        StringAssert.Contains(script, "let injectedRouter = inject(routerKey);");
        StringAssert.Contains(script, "let injectedRoute = inject(routeLocationKey);");
        StringAssert.Contains(script, "let injectedRouteRef = inject(routerViewLocationKey);");
        StringAssert.Contains(script, "let injectedMatched = inject(matchedRouteKey);");
        StringAssert.Contains(script, "let injectedDepth = inject(viewDepthKey);");
        StringAssert.Contains(script, "let loadedFromLocation = loadRouteLocation(routeLocation);");
        StringAssert.Contains(script, "let loadedFromNormalized = loadRouteLocation(normalized);");
        StringAssert.Contains(script, "let link = useLink({");
        StringAssert.Contains(script, "triggerRef(routeRef);");
        StringAssert.Contains(script, "injectedRouter.currentRoute.value.path");
        StringAssert.Contains(script, "injectedRoute.path");
        StringAssert.Contains(script, "injectedRouteRef.value.path");
        StringAssert.Contains(script, "injectedMatched.value.path");
        StringAssert.Contains(script, "injectedDepth.toString()");
        StringAssert.Contains(script, "link.href.value");
        StringAssert.Contains(script, "link.route.value.href");
    }

    [TestMethod]
    public async Task VueRoute_HistoryState_Compile_WithDirectTypedArrayAuthoring_ForRecursiveStateValues()
    {
        var code = """
            using ECMAScript;

            public static class TestClass
            {
                public static string BuildState()
                {
                    var state = new HistoryState
                    {
                        ["tags"] = new string?[] { "users", null, "detail" },
                        ["flags"] = new bool?[] { true, false, null },
                        ["steps"] = new Number?[] { (Number)1, null, (Number)2 },
                        ["trail"] = new HistoryState?[]
                        {
                            new HistoryState
                            {
                                ["kind"] = "root"
                            },
                            null,
                            new HistoryState
                            {
                                ["kind"] = "leaf",
                                ["visible"] = true
                            }
                        }
                    };

                    return "ok";
                }
            }
            """;

        var (classSymbol, semanticModel) = CompileAndGetSymbol(
            code,
            "TestClass",
            MetadataReference.CreateFromFile(typeof(ECMAScript.Contract.IUIComponent).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(ECMAScript.Vue).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(ECMAScript.VueRoute).Assembly.Location));
        var converter = new AstConverter(classSymbol, semanticModel);

        var module = await converter.Convert(CancellationToken.None);
        var script = module?.ToKnRECMAScript();

        Assert.IsNotNull(script);
        StringAssert.Contains(script, "let state = {");
        StringAssert.Contains(script, "tags: [\"users\", null, \"detail\"],");
        StringAssert.Contains(script, "flags: [true, false, null],");
        StringAssert.Contains(script, "steps: [1, null, 2],");
        StringAssert.Contains(script, "trail: [{ kind: \"root\" }, null, { kind: \"leaf\", visible: true }]");
        StringAssert.Contains(script, "return \"ok\";");
    }

    [TestMethod]
    public async Task VueRoute_ScrollBehavior_Compile_WithSelectorAndDomElementTargets()
    {
        var code = """
            using ECMAScript;
            using static ECMAScript.VueRoute;

            public static class TestClass
            {
                public static RouterOptions BuildOptions(RouterHistory history, Element panel)
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
                        ],
                        ScrollBehavior = RouterScrollHandler.From((RouteLocationNormalized to, RouteLocationNormalizedLoaded from, ScrollPositionNormalized? savedPosition) =>
                        {
                            if (savedPosition is not null)
                                return (RouterScrollResult)savedPosition;

                            var selectorTarget = new ScrollPositionElement
                            {
                                El = "#app",
                                Top = 12,
                                Behavior = ScrollBehavior.Smooth
                            };
                            var elementTarget = new ScrollPositionElement
                            {
                                El = panel,
                                Left = 4
                            };

                            return to.Hash != ""
                                ? (RouterScrollResult)selectorTarget
                                : (RouterScrollResult)elementTarget;
                        })
                    };
                }
            }
            """;

        var (classSymbol, semanticModel) = CompileAndGetSymbol(
            code,
            "TestClass",
            MetadataReference.CreateFromFile(typeof(ECMAScript.Contract.IUIComponent).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(ECMAScript.Vue).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(ECMAScript.VueRoute).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(Element).Assembly.Location));
        var converter = new AstConverter(classSymbol, semanticModel);

        var module = await converter.Convert(CancellationToken.None);
        var script = module?.ToKnRECMAScript();

        Assert.IsNotNull(script);
        StringAssert.Contains(script, "scrollBehavior: (to, from, savedPosition) => {");
        StringAssert.Contains(script, "if (!(savedPosition == null))");
        StringAssert.Contains(script, "return savedPosition;");
        StringAssert.Contains(script, "let selectorTarget = {");
        StringAssert.Contains(script, "el: \"#app\"");
        StringAssert.Contains(script, "top: 12");
        StringAssert.Contains(script, "behavior: \"smooth\"");
        StringAssert.Contains(script, "let elementTarget = {");
        StringAssert.Contains(script, "el: panel");
        StringAssert.Contains(script, "left: 4");
        StringAssert.Contains(script, "return to.hash !== \"\" ? selectorTarget : elementTarget;");
    }

    [TestMethod]
    public void VueRoute_RouteRecordAuthoring_Compile_FailsForIllegalMutuallyExclusiveCombinations()
    {
        var code = """
            using ECMAScript;

            public static class TestClass
            {
                public static void Build(RawRouteComponent component)
                {
                    _ = new RouteRecordSingleView
                    {
                        Path = "/users",
                        Component = component,
                        Redirect = "/home"
                    };

                    _ = new RouteRecordMultipleViews
                    {
                        Path = "/dashboard",
                        Components = new RawRouteComponents
                        {
                            ["default"] = component
                        },
                        Children =
                        [
                            new RouteRecordRedirect
                            {
                                Path = "legacy",
                                Redirect = "/dashboard/home"
                            }
                        ]
                    };

                    _ = new RouteRecordRedirect
                    {
                        Path = "/legacy",
                        Redirect = "/home",
                        Component = component
                    };
                }
            }
            """;

        var compilation = CSharpCompilation.Create(
            "ECMAScript.VueRoute.Test.Assembly.RouteRecordMutualExclusion",
            BuildSyntaxTrees(code),
            BuildCompilationReferences(new[]
            {
                MetadataReference.CreateFromFile(typeof(ECMAScript.Contract.IUIComponent).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(ECMAScript.Vue).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(ECMAScript.VueRoute).Assembly.Location)
            }),
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        var diagnostics = compilation.GetDiagnostics()
            .Where(static diagnostic => diagnostic.Severity == DiagnosticSeverity.Error)
            .ToArray();

        Assert.IsTrue(diagnostics.Any(static diagnostic => diagnostic.Id == "CS0117"),
            string.Join(Environment.NewLine, diagnostics.Select(static diagnostic => diagnostic.ToString())));
        Assert.IsTrue(diagnostics.Any(static diagnostic => diagnostic.GetMessage().Contains("Redirect", StringComparison.Ordinal)),
            string.Join(Environment.NewLine, diagnostics.Select(static diagnostic => diagnostic.ToString())));
        Assert.IsTrue(diagnostics.Any(static diagnostic => diagnostic.GetMessage().Contains("Children", StringComparison.Ordinal)),
            string.Join(Environment.NewLine, diagnostics.Select(static diagnostic => diagnostic.ToString())));
        Assert.IsTrue(diagnostics.Any(static diagnostic => diagnostic.GetMessage().Contains("Component", StringComparison.Ordinal)),
            string.Join(Environment.NewLine, diagnostics.Select(static diagnostic => diagnostic.ToString())));
    }

    private static MetadataReference[] BuildCompilationReferences(IEnumerable<MetadataReference>? additionalReferences = null)
    {
        var references = CurrentRuntimeReferences().ToList();
        references.Add(MetadataReference.CreateFromFile(typeof(Number).Assembly.Location));
        if (additionalReferences is not null)
            references.AddRange(additionalReferences);

        return references.ToArray();
    }

    private static IEnumerable<MetadataReference> CurrentRuntimeReferences()
    {
        var trustedPlatformAssemblies = (string?)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES");
        if (string.IsNullOrWhiteSpace(trustedPlatformAssemblies))
            return Net110.References.All.Cast<MetadataReference>();

        return trustedPlatformAssemblies
            .Split(Path.PathSeparator)
            .Where(static path => !string.IsNullOrWhiteSpace(path))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Select(static path => MetadataReference.CreateFromFile(path));
    }

    private static SyntaxTree[] BuildSyntaxTrees(string code)
        => new[] { CSharpSyntaxTree.ParseText(code, CSharpParseOptions.Default.WithLanguageVersion(LanguageVersion.Preview)) };

    private static int CountOccurrences(string text, string value)
    {
        var count = 0;
        var index = 0;
        while ((index = text.IndexOf(value, index, StringComparison.Ordinal)) >= 0)
        {
            count++;
            index += value.Length;
        }

        return count;
    }

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
