// WikiHomeModule.RazorVueLibraryMode.cs - RazorVue 库模式 / RazorVue Library Mode
using ECMAScript;
using static ECMAScript.Vue3;

namespace Wiki;

public static partial class WikiHomeModule
{
    // 构建 RazorVue 库模式页面主体 / Build the RazorVue library mode page body
    private static IVNode RazorVueLibraryModeBody()
        => H("div", new VueObject { Class = "doc-body" },
        [
            PageSection("why-razorvue", "为什么存在 RazorVue",
            [
                H("p", "并非每个项目都需要完整的开发宿主。RazorVue 存在是为了构建时 Razor 到 JavaScript 编译，输出是库制品而非实时应用外壳。"),
                H("ul",
                [
                    H("li", "在 `dotnet build` 期间编译 Razor 组件。"),
                    H("li", "交付库制品，不依赖已退役的 Jolt host。"),
                    H("li", "与仓库其余部分共享编译器、分析器、Emit 和源起源基础。")
                ])
            ]),
            PageSection("physical-split", "物理拆分",
            [
                H("p", "外部 RazorVue 命名保持稳定，但物理源码刻意按关注点拆分。"),
                CodeBlock("当前物理所有权", """
src/Jazor.RazorVue/
src/Jazor.RazorVue/RazorSdk/
src/Jazor.Analyzer/RazorVue/
src/ECMAScript.Vuetify/
src/ECMAScript.Contract/
"""),
                H("p", "这种拆分保持 RazorVue 核心语义、Razor SDK 桥接、Roslyn 宿主行为和库组件绑定不会漂移到一个项目中。")
            ]),
            PageSection("build-time-flow", "构建时流程",
            [
                H("p", "RazorVue 的契约是构建时制品生成，而非长时间运行的应用宿主。"),
                H("div", new VueObject { Class = "check-grid" },
                [
                    CheckCard("Razor 语义", "从 Razor 编写中提取组件含义。"),
                    CheckCard("制品生成", "生成目录、编译的 JS 模块和源起源。"),
                    CheckCard("Emit 物化", "将稳定制品传递到下游，使 Emit 可以写入浏览器就绪的输出。")
                ])
            ]),
            PageSection("when-to-choose-library-mode", "何时选择库模式",
            [
                H("p", "当用户故事是包创建、可复用组件或无需完整工作区宿主的构建时集成时，选择 RazorVue。"),
                RouteCardGrid([ProjectLinesPath, CompilerOverviewPath, HFunctionAuthoringPath])
            ])
        ]);
}
