// WikiHomeModule.ProjectLines.cs - 项目线路 / Project Lines
using ECMAScript;
using static ECMAScript.Vue3;

namespace Wiki;

public static partial class WikiHomeModule
{
    // 构建项目线路页面主体 / Build the project lines page body
    private static IVNode ProjectLinesBody()
        => H("div", new VueObject { Class = "doc-body" },
        [
            PageSection("two-lines", "两条活跃线路",
            [
                H("p", "Jazor 现在有两条活跃的技术线路，它们解决不同的产品问题。读者应根据编写模式和运行时预期来选择，而非依据历史命名。"),
                H("div", new VueObject { Class = "check-grid" },
                [
                    CheckCard("RazorVue 库模式", "在 `dotnet build` 期间将 Razor 组件编译为 JavaScript 模块，并作为普通库制品交付。"),
                    CheckCard("Jolt 完整宿主", "当项目需要完整的开发时宿主时，使用 `.jazor` 编写，支持 LSP、预览、HMR、构建和调试。")
                ])
            ]),
            PageSection("choose-a-path", "选择正确的路径",
            [
                H("p", "最快的解题方式是判断你的问题属于构建时制品生成还是完整应用开发流程。"),
                H("ul",
                [
                    H("li", "当交付物是库或组件包，且编写表面可以保持在 Razor 组件内时，选择 RazorVue。"),
                    H("li", "当交付物需要实时应用宿主、工作区图谱和浏览器优先的开发循环时，选择 Jolt。"),
                    H("li", "将两条线路视为同一编译器、Emit 和源起源基础的消费者。")
                ])
            ]),
            PageSection("shared-core", "共享核心",
            [
                H("p", "两条线路在编写和宿主行为上分道扬镳，但刻意共享编译器和 Emit 基础，以避免语义漂移。"),
                CodeBlock("共享模块", """
src/Jazor.Compiler/
src/Jazor.Emit/
src/Jazor.Common/
src/Jazor.Name/
src/Jazor.Analyzer/
"""),
                H("p", "这正是编译器边界页面、运行时目录页面和 Emit 契约页面对两条线路都重要的原因。")
            ]),
            PageSection("where-to-read-next", "推荐后续阅读",
            [
                H("p", "使用下方的线路专用页面深入了解，而非将所有架构问题压入一个概览。"),
                RouteCardGrid([RazorVueLibraryModePath, JoltHostPath, CompilerOverviewPath])
            ])
        ]);
}
