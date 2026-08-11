# 编译器架构

> 适用范围：`Jazor.Compiler`、`Jazor.CLR`、`Jazor.Analyzer` 及相关生成器的协作边界。

## 定位

`Jazor.Compiler` 不是通用 CLR 运行时的替代品。它在受控输入域内将 Roslyn `IOperation` 转换为 Acornima ESTree，并在已声明的宿主能力范围内保持使用点可观察行为。

当完整运行时结构无法或不值得保留时，编译器按以下顺序维护语义：求值顺序、副作用次数、最终结果、使用点行为、运行时结构身份。为此引入稳定临时变量、`SequenceExpression` 或 IIFE 是允许的；通过原始 JavaScript 静默回退则不允许。

## 分层职责

| 组件 | 职责 |
| --- | --- |
| `Jazor.Analyzer` | 尽早诊断不支持的外部类型、成员和已知不合法的使用方式 |
| `AstConverter` | 处理模块、类型、成员、导出和模块级 AST |
| `SemanticWalker` | 处理方法体、表达式、控制流、模式匹配、引用与宿主成员调用 |
| `WhiteList` | 消费由宿主映射生成的 `Alias`、`Inline`、`Import` 和 `Compile` 规则 |
| `Jazor.Compiler.Generator` | 从声明的映射生成白名单和复杂 lowering 注册 |
| `Jazor.CLR` | 为可支持的 CLR API 提供 C# 声明与 JavaScript 实现 |

## 宿主映射

外部 API 不由名称猜测或隐式 JavaScript fallback 支持。`Jazor.CLR` 与 ECMAScript 绑定通过 `[Jazor(Op.*)]` 声明可用能力，生成器据此产生可消费的白名单。

| 映射 | 适用场景 |
| --- | --- |
| `Alias` | 稳定的符号名称映射 |
| `Inline` | 简短、无复杂控制流的表达式模板 |
| `Import` | 可复用 helper 或模块级依赖 |
| `Compile` | 需要上下文、协议或 AST 级构造的复杂语义 |

`Compile` 声明失败表示该宿主能力已认领但无法降低，不能继续尝试普通成员访问。白名单 key 保留作者声明或 Roslyn 原始定义生成的规范形式，不在写入时做私有改写。

## 支持边界

- 泛型参数和数组元素类型通常是擦除的编译期注释；只有其具体运行时语义被直接物化或访问时才要求支持。
- `tuple`、枚举和接口按已定义的擦除或协议规则降低，不生成 CLR 风格运行时对象。
- 继承、构造函数重载、`ref/out`、解构等能力只支持已端到端实现的子集；未实现的协议必须明确报错。
- 模块级导入、临时名和源映射来源属于编译器契约，输出必须可重复。

## 权威实现资料

实现路线、保真优先级和扩展判据位于 [Jazor.Compiler 实现原则](../../src/Jazor.Compiler/ImplementationPrinciples.md)。项目级 API 与测试入口见 [Jazor.Compiler README](../../src/Jazor.Compiler/README.md) 和 [Jazor.CompilerTest README](../../src/Jazor.CompilerTest/README.md)。
