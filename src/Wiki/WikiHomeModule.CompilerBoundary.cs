// WikiHomeModule.CompilerBoundary.cs - 编译器支持边界 / Compiler Support Boundary
using ECMAScript;
using static ECMAScript.Vue3;

namespace Wiki;

public static partial class WikiHomeModule
{
    // 构建编译器边界页面主体 / Build the compiler boundary page body
    private static IVNode CompilerBoundaryBody()
        => H("div", new VueObject { Class = "doc-body" },
        [
            PageSection("controlled-domain", "受控输入域",
            [
                H("p", "`Jazor.Compiler` 不是通用 CLR-to-JS 编译器。支持的契约是一个受控输入域，其中 Roslyn `IOperation` 语义、宿主映射和确定性发射都比假装每个 .NET 运行时形状都能存在于 JavaScript 中更重要。"),
                H("ul",
                [
                    H("li", "首要目标：保留使用点可观察行为。"),
                    H("li", "第二目标：保持宿主语义边界显式且可审查。"),
                    H("li", "第三目标：保持发射的导入、名称和源锚点具有确定性。")
                ])
            ]),
            PageSection("behavior-priority", "行为优先级",
            [
                H("p", "当 Jazor 无法保留完整的 CLR 运行时身份时，它遵循显式的优先级顺序，而非临时妥协。"),
                H("div", new VueObject { Class = "check-grid" },
                [
                    CheckCard("1. 求值顺序", "不要为了使 JS 看起来更干净而复制或重排副作用。"),
                    CheckCard("2. 副作用次数", "改变执行次数的 Lowering 通常是错误的，即使最终值看起来正确。"),
                    CheckCard("3. 最终结果", "值、分支结果和可见状态优先于运行时结构保真度。"),
                    CheckCard("4. 使用点语义", "元组、记录和基于协议的特性可以在使用点行为保持正确时擦除运行时身份。")
                ])
            ]),
            PageSection("support-boundary", "支持边界",
            [
                H("p", "支持在运行时敏感的使用点决定，而非仅通过类型名是否出现在源码中。这就是为什么擦除的泛型位置可以被容忍，而具体运行时物化仍然会失败。"),
                H("ul",
                [
                    H("li", "通常允许：`List<Unsupported>`、`Task<Unsupported>`、`Dictionary<TKey, Unsupported>` 和类似的擦除位置。"),
                    H("li", "通常拒绝：`new Unsupported()`、运行时敏感的 `default(Unsupported)`，以及对不受支持的外部类型的直接静态或实例成员访问。"),
                    H("li", "不受支持的运行时敏感行为的默认策略：快速失败并显式诊断，而非静默的原始 JS 回退。")
                ])
            ]),
            PageSection("stabilized-routes", "已稳定的语义路由",
            [
                H("p", "几个语言路由不再是探索性的。贡献者应将它们视为活跃契约。"),
                H("ul",
                [
                    H("li", "元组：擦除值组合 Lowering；保留投影、解构和比较行为，而非 `System.ValueTuple` 运行时身份。"),
                    H("li", "Ref/out：调用方/被调用方协议模拟，保留顺序和写回语义。"),
                    H("li", "枚举：声明擦除加使用点常量 Lowering。"),
                    H("li", "接口：仅分析和宿主查找契约；无运行时声明发射。"),
                    H("li", "导入/Emit 链：`SemanticWalker` 收集导入，`AstConverter` 发射稳定模块头，`Jazor.Emit` 物化文件。")
                ])
            ]),
            PageSection("practical-reading", "实用阅读顺序",
            [
                H("p", "扩展编译器支持时，从活跃的原理文档开始，而非历史通过率快照。"),
                CodeBlock("推荐来源", """
src/Jazor.Compiler/ImplementationPrinciples.md
docs/03-完成/compiler/status.md
src/Jazor.Compiler/README.md
src/Jazor.CompilerTest/README.md
"""),
                Callout("工作规则", "如果一个提议的编译器改动弱化了支持边界或引入了输出不稳定性，即使它使一个狭窄的用例通过，也可能不是正确的扩展路线。")
            ])
        ]);
}
