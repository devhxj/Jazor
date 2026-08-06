// WikiHomeModule.CompilerOverview.cs - 编译器概览 / Compiler Overview
using ECMAScript;
using static ECMAScript.Vue3;

namespace Wiki;

public static partial class WikiHomeModule
{
    // 构建编译器概览页面主体 / Build the compiler overview page body
    private static IVNode CompilerOverviewBody()
        => H("div", new VueObject { Class = "doc-body" },
        [
            PageSection("what-it-is", "它是什么",
            [
                H("p", "`Jazor.Compiler` 是本仓库的核心 C# 到 JavaScript 编译器。它不旨在重建任意 CLR 运行时身份到 JavaScript 中。它旨在在受控域内保留使用点可观察行为。"),
                H("ul",
                [
                    H("li", "主要语义输入：Roslyn `IOperation`。"),
                    H("li", "主要中间表示：Acornima ESTree。"),
                    H("li", "主要输出契约：稳定的 AST、导入、名称、源起源和下游 Emit 载体。")
                ])
            ]),
            PageSection("core-pipeline", "核心管线",
            [
                H("p", "活跃管线刻意分层，使宿主映射、Lowering 和文件物化不会坍缩为一个模糊的阶段。"),
                H("div", new VueObject { Class = "check-grid" },
                [
                    CheckCard("SemanticWalker", "执行从 `IOperation` 到 ESTree 片段的表达式和语句 Lowering。"),
                    CheckCard("AstConverter", "构建模块级声明、类成员、导入和顶层形状。"),
                    CheckCard("Jazor.Emit", "物化 `.mjs`、`.mjs.map`、manifest 文件和面向 Bundle 的输出。")
                ])
            ]),
            PageSection("hard-contracts", "硬性契约",
            [
                H("p", "几个曾经是探索性的路由现在已足够稳定，贡献者应将它们视为工程契约。"),
                H("ul",
                [
                    H("li", "元组和记录路由保留使用点行为，而非 CLR 运行时身份。"),
                    H("li", "接口仅作为契约存在；不发射运行时声明。"),
                    H("li", "导入发现和模块头生成现在是稳定的主流行为，而非可选的后续工作。"),
                    H("li", "不受支持的运行时敏感行为应显式失败，而非降级为原始 JavaScript。")
                ])
            ]),
            PageSection("read-this-next", "推荐后续阅读",
            [
                H("p", "当需要比本概览更窄的规则时，使用边界和接缝页面。"),
                RouteCardGrid([CompilerBoundaryPath, HostSemanticSeamsPath, ImportEmitContractPath, RuntimeCatalogPath])
            ])
        ]);
}
