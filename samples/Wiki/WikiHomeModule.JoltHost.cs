// WikiHomeModule.JoltHost.cs - Jolt 历史宿主 / Retired Jolt Host
using ECMAScript;
using static ECMAScript.Vue3;

namespace Wiki;

public static partial class WikiHomeModule
{
    // 构建 Jolt 历史页面主体 / Build the retired Jolt host page body
    private static IVNode JoltHostBody()
        => H("div", new VueObject { Class = "doc-body" },
        [
            PageSection("why-jolt", "退役结论",
            [
                H("p", "Jolt 已从转型分支退役。当前分支不再提供 `.jazor` authoring、Jolt LSP/DAP、DevServer/HMR、debug 或 build host，也不为新项目保留兼容入口。"),
                H("ul",
                [
                    H("li", "转型分支只消费官方 Razor Source Generator 的 generated C# 与 Roslyn 语义。"),
                    H("li", "Razor 和手写 `.vue` 分别使用各自的原生语言服务。"),
                    H("li", "旧线路通过固定 baseline 和 Git 历史保留，不复制到当前生产项目图。")
                ])
            ]),
            PageSection("subsystems", "历史能力范围",
            [
                H("p", "退役前的 Jolt 曾协调以下开发时能力。它们只用于理解历史设计，不代表当前分支仍有对应实现。"),
                H("div", new VueObject { Class = "check-grid" },
                [
                    CheckCard("`.jazor` 投影", "解析第一作者文档并派生 Razor、Roslyn 与 Vue 虚拟文档。"),
                    CheckCard("LSP / DAP", "协调语言请求、source map 与调试协议。"),
                    CheckCard("DevServer / HMR", "提供预览、模块更新、CSS 与静态资源处理。"),
                    CheckCard("Build / Deno", "组织生产构建、Volar worker、import map 与 bundle。")
                ])
            ]),
            PageSection("run-modes", "历史恢复入口",
            [
                H("p", "需要维护、对照或追溯旧线路时，请从固定 baseline 或原分支恢复。不要在转型分支重新引入已删除的 Jolt 项目和协议。"),
                CodeBlock("固定历史边界", """
baseline commit: d68aecbb00b23aa35735c9a269b2e987c7815b05
retirement commit: 3ee18679fbdf43c13e05d7bfac8857ddcebd19f9
G0 evidence: docs/02-计划/RazorSgFinalDocument.G0.DecisionRecord.md
""")
            ]),
            PageSection("when-to-choose-jolt", "新项目如何选择",
            [
                H("p", "新项目不可再选择 Jolt。请沿当前 Razor-to-Vue render-function 主线工作；若需求属于旧 `.jazor` host，只能在旧线路维护，不能把其协议带回转型分支。"),
                RouteCardGrid([ProjectLinesPath, RazorVueLibraryModePath, GettingStartedPath])
            ])
        ]);
}
