// WikiHomeModule.Glossary.cs - 术语表 / Glossary
using ECMAScript;
using static ECMAScript.Vue;

namespace Wiki;

public static partial class WikiHomeModule
{
    // 构建术语表页面主体 / Build the glossary page body
    private static IVNode GlossaryBody()
        => H("div", new VueObject { Class = "doc-body" },
        [
            PageSection("compiler-terms", "编译器术语",
            [
                H("ul",
                [
                    H("li", [H("strong", "SemanticWalker"), H("span", " 将 Roslyn `IOperation` 树 Lowering 为 ESTree，同时保留使用点可观察行为。")]),
                    H("li", [H("strong", "AstConverter"), H("span", " 负责模块级 AST 组装、导入声明物化和最终结构规划。")]),
                    H("li", [H("strong", "Source origin"), H("span", " 将生成的 JavaScript 锚定回编写的 C#，使 Source Map 和调试工具保持可信。")]),
                    H("li", [H("strong", "WhiteList"), H("span", " 是编译器支持的外部运行时能力表面，从声明源生成。")])
                ])
            ]),
            PageSection("runtime-terms", "运行时术语",
            [
                H("ul",
                [
                    H("li", [H("strong", "Import map"), H("span", " 告诉浏览器 `System/*` 等模块说明符在运行时如何解析。")]),
                    H("li", [H("strong", "CLR 目录"), H("span", " 是发射的面向浏览器的模块集，以显式 ESM 文件支持 CLR 辅助函数导入。")]),
                    H("li", [H("strong", "Alias / Inline / Import / Compile"), H("span", " 是映射外部成员时使用的有序宿主语义接缝。")])
                ])
            ]),
            PageSection("host-terms", "宿主与工作流术语",
            [
                H("ul",
                [
                    H("li", [H("strong", "RazorVue"), H("span", " 是构建时库模式，用于将 Razor 组件编译为 JS 制品。")]),
                    H("li", [H("strong", "Jolt（历史）"), H("span", " 是已从转型分支退役的 `.jazor` 开发时宿主；当前只保留 baseline 与设计追溯入口。")]),
                    H("li", [H("strong", "Route catalog"), H("span", " 是单注册表面，驱动文档元数据、导航、TOC 锚点和相关页面流。")]),
                    H("li", [H("strong", "冒烟验证 (Smoke)"), H("span", " 是快速操作检查，证明发射的 Wiki 外壳和宿主路由端到端仍然有效。")])
                ]),
                Callout("实用规则", "如果一个术语在编译器、Emit 和宿主关注点之间被重载，应记录其所属边界，而非依赖部落知识。")
            ])
        ]);
}
