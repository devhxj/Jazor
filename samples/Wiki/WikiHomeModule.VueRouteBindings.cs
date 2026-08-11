// WikiHomeModule.VueRouteBindings.cs - VueRoute 绑定 / VueRoute Bindings
using ECMAScript;
using static ECMAScript.Vue;

namespace Wiki;

public static partial class WikiHomeModule
{
    // 构建 VueRoute 绑定页面主体 / Build the VueRoute bindings page body
    private static IVNode VueRouteBindingsBody()
        => H("div", new VueObject { Class = "doc-body" },
        [
            PageSection("why-vueroute-exists", "为什么存在 VueRoute 绑定",
            [
                H("p", "`ECMAScript.VueRoute` 的存在使 Vue Router 4 可以通过与 `ECMAScript.Vue` 相同的类型化 C# 宿主绑定模型编写，而非依赖临时字符串导入或编译器特殊情况。"),
                H("ul",
                [
                    H("li", "保持 `vue-router` 导入显式且局限于绑定库。"),
                    H("li", "暴露真实应用代码最先需要的高频编写表面。"),
                    H("li", "让编译器、Emit、包和消费者测试验证集成，而无需将路由器语义硬编码到编译器中。")
                ])
            ]),
            PageSection("current-surface", "当前表面",
            [
                H("p", "当前项目刻意覆盖大多数 Jazor 应用需要的路由编写的第一个切片。"),
                CodeBlock("当前 `ECMAScript.VueRoute` 范围", """
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
                H("p", "该范围已涵盖路由创建、历史创建、路由读取访问、常见导航调用和组件级路由器入口点。")
            ]),
            PageSection("authoring-boundary", "编写边界",
            [
                H("p", "该库刻意作为宿主绑定表面，而非隐藏框架策略的地方。"),
                H("div", new VueObject { Class = "check-grid" },
                [
                    CheckCard("仅绑定", "该项目将官方 Vue Router API 根映射到类型化的 C# 名称和宿主记录。"),
                    CheckCard("无编译器特殊处理", "`Jazor.Compiler` 仍将 VueRoute 视为普通的外部宿主绑定库。"),
                    CheckCard("分层验证", "结构、代理表面、编译器边界行为和 nupkg 消费在独立的测试层中验证。")
                ]),
                Callout("实用规则", "如果路由器特性只能通过教会编译器关于 `vue-router` 的知识来工作，绑定设计就退化了。")
            ]),
            PageSection("verification-path", "验证路径",
            [
                H("p", "VueRoute 作为一等项目接入，其回归覆盖刻意从 `Jazor.CompilerTest` 中拆分出来。"),
                CodeBlock("当前验证链", """
src/ECMAScript.VueRoute.Test/
  EcmaScriptVueRouteLayoutGuardTests.cs
  EcmaScriptVueRouteProxyTests.cs
  EcmaScriptVueRouteCompilerBoundaryTests.cs

dotnet run --file ./scripts/csharp/test-dotnet.cs -- --project vueroute
src/Jazor.EmitTest/SdkIntegrationTests.cs
src/Jazor/Jazor.csproj
"""),
                H("ul",
                [
                    H("li", "独立测试项目锁定结构、反射表面和编译器边界行为。"),
                    H("li", "Emit 集成测试证明本地打包的 `Jazor` 包可以在消费者项目中恢复、构建和发射 Vue Router 导入。"),
                    H("li", "`ECMAScript.VueRoute` 作为独立前端库发布，消费方在引用 `Jazor` 之外还需要显式安装它。")
                ])
            ]),
            PageSection("where-to-extend-next", "下一步扩展方向",
            [
                H("p", "添加应遵循公共路径优先规则：在追求长尾 TypeScript 精度之前，先加强公共路由编写路径。"),
                RouteCardGrid([ProjectLinesPath, RazorVueLibraryModePath, ImportEmitContractPath, TestingVerificationPath])
            ])
        ]);
}
