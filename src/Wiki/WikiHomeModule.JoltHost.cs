// WikiHomeModule.JoltHost.cs - Jolt 宿主 / Jolt Host
using ECMAScript;
using static ECMAScript.Vue3;

namespace Wiki;

public static partial class WikiHomeModule
{
    // 构建 Jolt 宿主页面主体 / Build the Jolt host page body
    private static IVNode JoltHostBody()
        => H("div", new VueObject { Class = "doc-body" },
        [
            PageSection("why-jolt", "为什么存在 Jolt",
            [
                H("p", "库模式发射不足以满足完整应用开发。Jolt 存在是为了围绕 `.jazor` 编写、工作区上下文、预览、构建和调试循环提供开发时宿主。"),
                H("ul",
                [
                    H("li", "`.jazor` 保持为一等编写表面。"),
                    H("li", "工作区被视为图谱，而非一堆孤立文件。"),
                    H("li", "Jazor、Roslyn 和 Volar 各自保持独立的语义通道。")
                ])
            ]),
            PageSection("subsystems", "子系统",
            [
                H("p", "Jolt 不是单一整体。它协调多个聚焦的子系统。"),
                H("div", new VueObject { Class = "check-grid" },
                [
                    CheckCard("Jazor 核心", "解析 `.jazor`，构建投影，派生前端上下文。"),
                    CheckCard("LSP", "跨 Jazor、Roslyn 和 Volar 通道路由请求。"),
                    CheckCard("DevServer 和 Build", "处理预览、HMR、生产构建、CSS、资源和 import map。"),
                    CheckCard("Volar / Deno", "提供 Vue、TypeScript、CSS 和 HTML 语义工作器。")
                ])
            ]),
            PageSection("run-modes", "运行模式",
            [
                H("p", "Jolt 为其需要拥有的工作流暴露不同的模式，而非一个通用的启动路径。"),
                CodeBlock("代表性模式", """
--stdio
--lsp
--dev
--build
--analysis-stdio
""")
            ]),
            PageSection("when-to-choose-jolt", "何时选择 Jolt",
            [
                H("p", "当任务主要关于应用开发人体工学而非库制品生成时，选择 Jolt。"),
                RouteCardGrid([ProjectLinesPath, RazorVueLibraryModePath, GettingStartedPath])
            ])
        ]);
}
