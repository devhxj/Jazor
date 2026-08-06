// WikiHomeModule.HostSemanticSeams.cs - 宿主语义接缝 / Host Semantic Seams
using ECMAScript;
using static ECMAScript.Vue3;

namespace Wiki;

public static partial class WikiHomeModule
{
    // 构建宿主语义接缝页面主体 / Build the host semantic seams page body
    private static IVNode HostSemanticSeamsBody()
        => H("div", new VueObject { Class = "doc-body" },
        [
            PageSection("why-seams-exist", "为什么存在接缝",
            [
                H("p", "宿主语义不是任意 JavaScript 的逃生舱。它们是编译器 Lowering 和受支持的外部运行时行为之间的声明式接缝。"),
                H("ul",
                [
                    H("li", "`WhiteList` 声明哪些外部类型和成员受支持。"),
                    H("li", "消费者分派保持有序：`Allowed/Alias -> Inline -> Import -> Compile`。"),
                    H("li", "不受支持的运行时敏感行为应显式失败，而非降级为原始 JavaScript。")
                ])
            ]),
            PageSection("choose-the-right-seam", "选择正确的接缝",
            [
                H("p", "主要工程决策不是是否添加映射，而是哪个接缝拥有该行为。"),
                H("div", new VueObject { Class = "check-grid" },
                [
                    CheckCard("Alias", "用于稳定的名称重映射，如类型/运行时名称投影或明显的成员重命名情况如 `ToString -> toString`。"),
                    CheckCard("Inline", "用于简短、可读的表达式模板，具有稳定的本地语义且无复杂控制流。"),
                    CheckCard("Import", "用于共享辅助逻辑、重复守卫、非平凡分支，或作为显式运行时模块更清晰的行为。"),
                    CheckCard("Compile", "当宿主行为需要 AST 级构造、上下文敏感的 Lowering、临时变量、导入或协议感知结构时使用。")
                ])
            ]),
            PageSection("whitelist-contract", "WhiteList 契约",
            [
                H("p", "`WhiteList` 不仅是字符串替换表。它是编译器的正式宿主能力表面，从 `Jazor.CLR` 和相关映射中的源声明生成。"),
                CodeBlock("当前宿主映射来源", """
src/Jazor.CLR/module/*.cs
src/Jazor.Compiler/WhiteList.cs.Generate.cs
src/Jazor.Compiler.Generator/Program.cs
src/Jazor.Compiler/core/SemanticWalker.cs
"""),
                H("ul",
                [
                    H("li", "先修改声明源；不要手工编辑生成的白名单输出。"),
                    H("li", "保持生产者和消费者语义对齐，使同一 API 表面不会在 CLR 源和编译器分派之间漂移。"),
                    H("li", "将 `Op.Discard` 和显式不支持的情况视为产品边界标记，而非需要隐藏的临时尴尬。")
                ])
            ]),
            PageSection("inline-vs-compile", "Inline 与 Compile",
            [
                H("p", "一个常见的失败模式是将复杂行为留在 `Inline` 中太久。可读性门槛与语义门槛同样重要。"),
                H("ul",
                [
                    H("li", "当一个表达式保持简短、可审查且语义明显时，优先使用 `Inline`。"),
                    H("li", "当行为需要共享辅助代码或会变成长而多分支的模板时，升级到 `Import`。"),
                    H("li", "当宿主语义需要 AST 节点、表达式或语句重构或上下文 Lowering 决策时，升级到 `Compile`。"),
                    H("li", "如果行为应该是编译器固有规则，不要将公开编写语法糖推入临时 `[Jazor]` 编译钩子。")
                ]),
                Callout("实用规则", "如果审查者需要心智模拟占位符替换才能信任行为，接缝可能太弱了。")
            ]),
            PageSection("verification-surface", "验证表面",
            [
                H("p", "每个接缝变更都应同时证明映射元数据和发射行为。"),
                H("ul",
                [
                    H("li", "当类型别名或成员映射变更时，添加 CLR 白名单测试。"),
                    H("li", "当分派顺序、Inline 发射、Import 绑定或 Compile 钩子行为变更时，添加编译器测试。"),
                    H("li", "当变更影响浏览器提供的模块、import-map 假设或发射的文档外壳输出时，使用 Wiki 冒烟测试。"),
                    H("li", "当具体类型和接口表面代表一个运行时契约族时，保持它们对齐。")
                ])
            ])
        ]);
}
