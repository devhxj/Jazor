using ECMAScript;
using static ECMAScript.Vue3;

namespace Wiki;

public static partial class WikiHomeModule
{
    private static IVNode VueRouteBindingsBody()
        => H("div", new VueObject { Class = "doc-body" },
        [
            PageSection("why-vueroute-exists", "Why VueRoute bindings exist",
            [
                H("p", "`ECMAScript.VueRoute` exists so Vue Router 4 can be authored through the same typed C# host-binding model as `ECMAScript.Vue3`, instead of relying on ad-hoc string imports or compiler special cases."),
                H("ul",
                [
                    H("li", "Keep `vue-router` imports explicit and local to the binding library."),
                    H("li", "Expose the high-frequency authoring surface that real app code reaches first."),
                    H("li", "Let compiler, emit, package, and consumer tests validate the integration without hard-wiring router semantics into the compiler.")
                ])
            ]),
            PageSection("current-surface", "Current surface",
            [
                H("p", "The current project deliberately covers the first slice of route authoring that most Jazor apps need."),
                CodeBlock("Current `ECMAScript.VueRoute` scope", """
src/ECMAScript.VueRoute/
  VueRoute.cs
  Api/VueRoute.Api.cs
  Types/VueRoute.Types.cs
  Types/VueRoute.Types.Unions.cs

createRouter(...)
createWebHistory(...)
createWebHashHistory(...)
createMemoryHistory(...)
useRouter()
useRoute()
useLink(...)
RouterLink
RouterView
"""),
                H("p", "That scope already covers route creation, history creation, route-read access, common navigation calls, and component-level router entry points.")
            ]),
            PageSection("authoring-boundary", "Authoring boundary",
            [
                H("p", "The library is intentionally a host-binding surface, not a place to hide framework policy."),
                H("div", new VueObject { Class = "check-grid" },
                [
                    CheckCard("Binding only", "The project maps official Vue Router API roots into typed C# names and host records."),
                    CheckCard("No compiler carve-outs", "`Jazor.Compiler` still treats VueRoute like a normal external host-binding library."),
                    CheckCard("Layered verification", "Structure, proxy surface, compiler-boundary behavior, and nupkg consumption are verified in separate test layers.")
                ]),
                Callout("Practical rule", "If a router feature can only work by teaching the compiler about `vue-router`, the binding design regressed.")
            ]),
            PageSection("verification-path", "Verification path",
            [
                H("p", "VueRoute is wired as a first-class project, and its regression coverage is intentionally split out of `Jazor.CompilerTest`."),
                CodeBlock("Current verification chain", """
src/ECMAScript.VueRoute.Test/
  EcmaScriptVueRouteLayoutGuardTests.cs
  EcmaScriptVueRouteProxyTests.cs
  EcmaScriptVueRouteCompilerBoundaryTests.cs

scripts/test-dotnet.ps1 -Project vueroute
src/Jazor.EmitTest/SdkIntegrationTests.cs
src/Jazor/Jazor.csproj
"""),
                H("ul",
                [
                    H("li", "The standalone test project locks structure, reflection surface, and compiler-boundary behavior."),
                    H("li", "The emit integration test proves a local packed `Jazor` package can restore, build, and emit Vue Router imports in a consumer project."),
                    H("li", "Packaging wires `ECMAScript.VueRoute.dll` into the shipped `Jazor` package alongside the existing runtime libraries.")
                ])
            ]),
            PageSection("where-to-extend-next", "Where to extend next",
            [
                H("p", "Additions should follow the common-path-first rule: strengthen the public route authoring path before chasing long-tail TypeScript precision."),
                RouteCardGrid([ProjectLinesPath, RazorVueLibraryModePath, ImportEmitContractPath, TestingVerificationPath])
            ])
        ]);
}
