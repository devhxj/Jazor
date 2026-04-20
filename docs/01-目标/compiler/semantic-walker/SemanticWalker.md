# `SemanticWalker`

## 定位

`SemanticWalker` 是 Jazor 编译器里负责“语义级 lowering”的核心组件。

它把 Roslyn 的 `IOperation` 树转换成 Acornima ESTree 节点，属于整条转换链路里最靠近“C# 语义”同时也最直接决定“最终 JS shape”的一层。

对应代码：

- `src/Jazor.Compiler/core/SemanticWalker.cs`
- `src/Jazor.Compiler/core/SemanticWalker.cs.*.cs`

如果只看职责，可以把它理解成：

```text
C# IOperation
    -> SemanticWalker
    -> ESTree
    -> JavaScript writer
```

## 当前职责边界

`SemanticWalker` 负责的是“语义到 AST”的转换，不负责：

- 模块级成员拆解
- 源码输出文件组织
- 最终 JavaScript 文本写出
- 完整 sourcemap 产物生成

这些职责分别落在：

- `AstConverter`
- `ESGenerator`
- 后续 writer / emit 路径

所以判断一个问题该不该进 `SemanticWalker`，核心看它是不是：

- 某个 `IOperation` 应该变成什么 AST
- 某个 C# 语义糖应该如何 lower
- 某个 runtime host/member 在表达式里应如何落地

## 核心设计原则

### 1. 优先基于语义，而不是语法文本

`SemanticWalker` 的主要输入是 `IOperation`，不是裸语法节点。

这意味着：

- 它优先信 Roslyn 的语义绑定结果
- 语法节点主要用来补局部信息或处理特殊回退路径
- 类型别名、隐式转换、方法绑定、常量语义都尽量在语义层解决

这也是为什么像“静态运行时宿主选择”这类问题，最终要靠 `SemanticModel` 和类型关系，而不能只看调用点文本。

### 2. 目标是结果等价，不是逐步模拟 CLR

当前设计不追求在 JS 里重建一整套 CLR 运行时。

更明确地说，它通常不会主动引入：

- 新的包装宿主
- 新的桥接对象层
- 额外的运行时协议类型

只要能在当前映射边界内稳定得到结果等价，`SemanticWalker` 会优先选择更直接的 lowering。

### 3. 优先让输出贴近真实 JS 运行时 shape

这条原则在手写映射和 `Reference` 语法域里尤其重要。

例如：

- `Console.WriteLine` 最终是 `console.log`
- `Bytes.Of(...)` 不会把 C# 类型别名泄漏到 JS
- tuple 会 lower 成普通对象，而不是额外 tuple runtime type

目标是尽量减少 C# / JS 的割裂，而不是为编译器内部方便引入新的名字体系。

### 4. 对复杂 lowering，优先保持求值顺序正确

例如：

- `ref` / `out`
- tuple 解构 / swap
- 模式匹配缓存
- 条件访问

这些场景里，当前策略通常是先插入临时变量，再构造最终表达式，优先保证：

- 副作用次数正确
- 求值顺序正确
- 结果值正确

## 上下文模型

`SemanticWalker` 不是单纯的“递归访问器”，它还依赖显式上下文传递。

关键对象是：

- `Sense`
- `SenseArgument`

它们负责描述当前转换处于什么语义环境，例如：

- 普通右值
- 左值
- 属性初始化器
- `out` 参数
- 模式匹配输入
- 作用域边界

`SenseArgument` 还承担两类跨节点收集工作：

- 变量声明收集
- import specifier 收集

这也是当前很多 lowering 可以避免“向上回看父节点”的基础。

## 分文件组织

当前 `SemanticWalker` 按语法域拆成多个 partial 文件。

| 文件 | 当前职责 |
|------|----------|
| `SemanticWalker.cs` | 主入口、通用 `Translate`、基础类型映射、统一调度 |
| `SemanticWalker.cs.Pattern.cs` | 模式匹配 lowering |
| `SemanticWalker.cs.Reference.cs` | 引用、调用、索引、运行时宿主修正 |
| `SemanticWalker.cs.Loop.cs` | `for` / `foreach` / `while` |
| `SemanticWalker.cs.Switch.cs` | `switch` 与模式 `switch` |
| `SemanticWalker.cs.String.cs` | 插值字符串 |
| `SemanticWalker.cs.TryCatch.cs` | `try/catch/finally` |
| `SemanticWalker.cs.Creation.cs` | 对象、数组、匿名对象、初始化器 |
| `SemanticWalker.cs.Tuple.cs` | tuple、解构、projection、比较 |
| `SemanticWalker.cs.Declaration.cs` | 变量与局部函数相关声明 |
| `SemanticWalker.cs.Ordinary.cs` | 常规表达式、一元/二元/赋值 |
| `SemanticWalker.cs.Invalid.cs` | `IInvalidOperation` 回退处理 |
| `SemanticWalker.cs.NotSupport.cs` | 明确拒绝或报错的语义节点 |
| `SemanticWalker.cs.WhiteList.cs` | 白名单消费与模板映射 |
| `SemanticWalker.cs.Generate.cs` | 生成器产物 |

这种拆分不是按“代码量”划分，而是按“lowering 语义域”划分。

## 当前主线能力

从现状看，`SemanticWalker` 主要承担下面几条主线能力。

### 1. 普通表达式与控制流 lowering

包括：

- 一元/二元表达式
- 赋值
- 循环
- `switch`
- `try/catch/finally`
- 条件访问

这部分构成最基础的 C# 到 JS AST 转换面。

### 2. 引用与运行时宿主对齐

这是 `Reference` 语法域负责的重点：

- 字段/属性/方法/索引访问
- ECMAScript 运行时宿主归一化
- 静态成员最终宿主选择
- 导入式宿主成员
- `ref` / `out` 调用回写

相关文档：

- [SemanticWalker.Reference.md](./SemanticWalker.Reference.md)
- [RuntimeStaticHostResolution.md](./RuntimeStaticHostResolution.md)

### 3. 创建与初始化器 lowering

包括：

- 对象创建
- 数组创建
- 匿名对象
- 对象初始化器
- 集合初始化器

这部分通常直接决定最终 JS 字面量或构造表达式的 shape。

### 4. tuple lowering

tuple 在当前设计里被视为“编译期语法糖”，不是新的运行时类型设计问题。

所以相关逻辑主要处理：

- tuple 字面量
- tuple projection
- 解构
- 比较
- swap

相关文档：

- [SemanticWalker.Tuple.md](./SemanticWalker.Tuple.md)

### 5. 白名单和宿主 API 映射消费

`SemanticWalker` 会消费由 `WhiteList` 和生成器提供的宿主规则，包括：

- `Alias`
- `Inline`
- `Import`
- 运行时宿主别名

这让“外部 API 映射”不会散落到每个 lowering 分支里。

## 与 `AstConverter` 的分工

一个常见混淆点是：`AstConverter` 和 `SemanticWalker` 都在“转换”。

两者分工实际上很清楚：

### `AstConverter` 负责

- 模块类拆解
- 顶层 `function` / `class` / `let` 组织
- 导出结构

### `SemanticWalker` 负责

- 方法体、表达式体、初始化器内部的语义 lowering
- `IOperation -> ESTree`

如果一个问题关心的是“模块里应该导出什么”，优先看 `AstConverter`。

如果一个问题关心的是“方法体里一段 C# 应该变成什么 JS AST”，优先看 `SemanticWalker`。

## 与白名单的关系

白名单不直接等于 `SemanticWalker`，但 `SemanticWalker` 是白名单规则最主要的消费方。

大致关系：

```text
宿主映射标注
    -> 生成 WhiteList / Compile 支撑
    -> SemanticWalker 在引用、调用、创建等路径消费
```

`SemanticWalker` 自己不负责声明这些宿主规则，但负责在具体语义节点上把规则落地。

## 与 sourcemap 的关系

当前 sourcemap 方案已经独立建档，但它高度依赖 `SemanticWalker` 的 lowering 稳定性。

原因很直接：

- `SemanticWalker` 会拆分一个源节点到多个 JS 片段
- 会插入临时变量
- 会重排部分结构
- 会显式构造 projection / 回写 / 缓存表达式

所以 sourcemap 不能脱离 `SemanticWalker` 的 lowering 形态单独设计。

相关文档：

- [SourceMap.DecisionSummary.md](./SourceMap.DecisionSummary.md)
- [SourceMap.Design.md](./SourceMap.Design.md)

## 当前边界

`SemanticWalker` 当前并不承诺这些事情：

- 完整 CLR 运行时仿真
- 对所有 C# 语法无条件支持
- 不插入任何临时变量
- 生成最短或最漂亮的 JS

它当前的优先级更接近：

1. 结果等价
2. 运行时 shape 尽量正确
3. 规则统一、可维护
4. 输出风格与后续优化可再改进

## 推荐阅读顺序

如果要从总览进入细节，建议顺序是：

1. [SyntaxTransformationPipeline.md](./SyntaxTransformationPipeline.md)
2. [SemanticWalker.Reference.md](./SemanticWalker.Reference.md)
3. [SemanticWalker.Tuple.md](./SemanticWalker.Tuple.md)
4. [SemanticWalker.WhiteList.md](./SemanticWalker.WhiteList.md)
5. [RuntimeStaticHostResolution.md](./RuntimeStaticHostResolution.md)

## 相关文档

- [SyntaxTransformationPipeline.md](./SyntaxTransformationPipeline.md)
- [SemanticWalker.Reference.md](./SemanticWalker.Reference.md)
- [SemanticWalker.Tuple.md](./SemanticWalker.Tuple.md)
- [SemanticWalker.WhiteList.md](./SemanticWalker.WhiteList.md)
- [WalkerExtensionSpec.md](./WalkerExtensionSpec.md)
- [InlineAstTemplateSpec.md](./InlineAstTemplateSpec.md)
