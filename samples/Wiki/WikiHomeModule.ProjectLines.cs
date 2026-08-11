// WikiHomeModule.ProjectLines.cs - 项目线路 / Project Lines
using ECMAScript;
using static ECMAScript.Vue;

namespace Wiki;

public static partial class WikiHomeModule
{
    // 构建项目线路页面主体 / Build the project lines page body
    private static IVNode ProjectLinesBody()
        => H("div", new VueObject { Class = "doc-body" },
        [
            PageSection("two-lines", "当前主线与历史边界",
            [
                H("p", "当前转型分支只有一条 Razor-to-Vue 主线：官方 Razor Source Generator 生成 C#，Roslyn 提供语义，后续输出 Vue render-function `.mjs`。Jolt 仅作为可追溯的历史边界保留。"),
                H("div", new VueObject { Class = "check-grid" },
                [
                    CheckCard("Razor-to-Vue 转型主线", "只消费 SG generated C# 与 Roslyn `IOperation`，生成唯一的 render-function `.mjs` artifact。"),
                    CheckCard("Jolt（已退役）", "旧 `.jazor` LSP、DAP、DevServer/HMR 与 build host 已从当前项目图删除，只能从 Git 历史恢复。")
                ])
            ]),
            PageSection("choose-a-path", "当前开发入口",
            [
                H("p", "新工作默认进入转型主线，不再在 RazorVue 与 Jolt 之间做产品选择。"),
                H("ul",
                [
                    H("li", "组件语义从官方 Razor SG 的 generated C# 绑定结果进入编译器。"),
                    H("li", "Razor 与 `.vue` 编辑体验分别交给原生语言服务，不重建 Jolt 多 lane 协调。"),
                    H("li", "若必须维护旧 Jolt 行为，请使用固定 baseline，而不是在当前分支增加兼容层。")
                ])
            ]),
            PageSection("shared-core", "共享核心",
            [
                H("p", "转型主线继续复用通用编译器和 Emit 基础，但不复用旧 Jolt frontend、DTO、协议或状态机。"),
                CodeBlock("共享模块", """
src/Jazor.Compiler/
src/Jazor.Emit/
src/Jazor.Common/
src/Jazor.Name/
src/Jazor.Analyzer/
"""),
                H("p", "编译器边界、运行时目录和 Emit 契约仍是当前主线的共同基础。")
            ]),
            PageSection("where-to-read-next", "推荐后续阅读",
            [
                H("p", "继续阅读当前 RazorVue 边界、编译器契约；Jolt 页面仅用于历史追溯。"),
                RouteCardGrid([RazorVueLibraryModePath, JoltHostPath, CompilerOverviewPath])
            ])
        ]);
}
