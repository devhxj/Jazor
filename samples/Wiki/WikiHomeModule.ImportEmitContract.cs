// WikiHomeModule.ImportEmitContract.cs - 导入与发射契约 / Import & Emit Contract
using ECMAScript;
using static ECMAScript.Vue;

namespace Wiki;

public static partial class WikiHomeModule
{
    // 构建导入与发射契约页面主体 / Build the import & emit contract page body
    private static IVNode ImportEmitContractBody()
        => H("div", new VueObject { Class = "doc-body" },
        [
            PageSection("boundary-split", "边界划分",
            [
                H("p", "编译器和发射器刻意是一个管线中的不同产品。`Jazor.Compiler` 拥有语义 Lowering 和模块形状输出；`Jazor.Emit` 拥有面向宿主的文件物化和打包。"),
                H("div", new VueObject { Class = "check-grid" },
                [
                    CheckCard("编译器侧", "构建 ESTree、模块文本、导入结构、源起源锚点，以及目录或 Source Map 载体。"),
                    CheckCard("发射器侧", "从程序集加载目录，写入 `.mjs` 和 manifest 文件，运行 Bundle 编排。"),
                    CheckCard("为何重要", "如果一个特性需要编译器逻辑秘密写入浏览器文件，或 Emit 逻辑发明 Lowering 语义，则该特性形状不正确。")
                ])
            ]),
            PageSection("import-mainline", "导入主线",
            [
                H("p", "导入路径已经是封闭的主线，而非松散的辅助调用集合。贡献者应保留该流程，而非绕过它。"),
                CodeBlock("导入流", """
SemanticWalker
  -> host mapping chooses Alias / Inline / Import / Compile
SenseArgument
  -> collects and flushes import specifiers
AstConverter
  -> merges, dedupes, orders, and emits ImportDeclaration headers
ESGenerator
  -> serializes the final module text and carriers
"""),
                H("ul",
                [
                    H("li", "`Op.Import` 在语义 Lowering 点发现，而非后续通过字符串重写合成。"),
                    H("li", "导入别名稳定性是契约的一部分，因此同一模块符号不应在单个模块内漂移到不同的本地名称。"),
                    H("li", "模块头排序和去重属于 `AstConverter`，而非后续的文件写入阶段。")
                ])
            ]),
            PageSection("layered-output", "分层输出契约",
            [
                H("p", "输出刻意分层。每个阶段拥有一个边界，混合它们会使管线更难推理和测试。"),
                CodeBlock("当前所有权", """
src/Jazor.Compiler/core/SemanticWalker.cs
src/Jazor.Compiler/SenseArgument.cs
src/Jazor.Compiler/AstConverter.cs
src/Jazor.Compiler/ESGenerator.cs
src/Jazor.Emit/ModuleCollector.cs
src/Jazor.Emit/ModuleWriter.cs
src/Jazor.Emit/NetpackBundler.cs
"""),
                H("ul",
                [
                    H("li", "`AstConverter` 拥有模块 AST 形状，而非文件系统输出。"),
                    H("li", "`ESGenerator` 拥有 JavaScript 文本加目录或 Source Map 载体，而非浏览器托管策略。"),
                    H("li", "`Jazor.Emit` 拥有 `.mjs`、`.mjs.map`、manifest 和 Bundle 物化，而非语言 Lowering。")
                ])
            ]),
            PageSection("host-materialization", "宿主物化",
            [
                H("p", "`Jazor.Emit` 从编译后的程序集和生成的目录工作。这保持宿主输出可重现，并将浏览器交付决策排除在 Lowering 核心之外。"),
                H("ul",
                [
                    H("li", "收集根程序集和引用程序集。"),
                    H("li", "读取 ECMAScript 模块目录和可选的 RazorVue 目录。"),
                    H("li", "写入模块文件、manifest 和 Source Map。"),
                    H("li", "可选通过 `DenoHost` 重写导入并打包。")
                ]),
                Callout("实用规则", "如果一个变更需要绕过目录直接从编译器 Lowering 写入浏览器文件，它可能跨越了错误的边界。")
            ]),
            PageSection("verification-signals", "验证信号",
            [
                H("p", "这个边界只有在正确层捕获回归时才有用。"),
                H("ul",
                [
                    H("li", "当导入收集、别名稳定性、源起源或载体形状变更时，使用编译器测试。"),
                    H("li", "当 manifest 输出、Bundle 重写或文件物化变更时，使用 Emit 测试。"),
                    H("li", "当浏览器提供的模块、路由资产或 import-map 预期变更时，使用 Wiki 冒烟测试。"),
                    H("li", "将 Source Map 和目录确定性视为生产契约，而非可选的调试附加。")
                ])
            ])
        ]);
}
