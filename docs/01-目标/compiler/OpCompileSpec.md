# `Op.Compile` 设计约定

前置阅读：[实现原则](../../../src/Jazor.Compiler/ImplementationPrinciples.md)

## 目录

- [定位](#定位)
- [当前事实](#当前事实)
- [目标职责](#目标职责)
- [当前主分发顺序](#当前主分发顺序)
- [`handler` / `args` 契约](#handler--args-契约)
- [返回语义](#返回语义)
- [与 `Inline` 的边界](#与-inline-的边界)
- [接线后仍需保持的约束](#接线后仍需保持的约束)
- [当前建议](#当前建议)
- [相关文档](#相关文档)

## 定位

以下讨论 `Op.Compile` 在 `Jazor.Compiler` 里的消费契约。

它回答的是：

- `Op.Compile` 现在到底接到了哪一步
- 后续应该按什么顺序参与白名单分发
- `Compile_*` 返回 `null`、返回表达式、抛异常各自代表什么
- `Compile` 和 `Inline` 的边界到底在哪里

相关代码主要在：

- `src/Jazor.Common/JazorAttribute.cs`
- `src/Jazor.Compiler/WhiteList.cs.Compile.cs`
- `src/Jazor.Compiler/core/SemanticWalker.cs`
- `src/Jazor.Compiler/core/SemanticWalker.cs.Generate.cs`
- `src/Jazor.Compiler/core/SemanticWalker.cs.WhiteList.cs`

## 当前事实

首先明确当前已存在的内容。

### 1. 标注入口已经固定

`[Jazor]` 的无参构造会生成 `Op.Compile`。

换言之，`Compile` 不是运行时动态配置，而是编译期静态声明。

### 2. Generator 基础设施已经存在

当前生成器已经会为 `Op.Compile` 条目额外生成两类产物：

- `WhiteList.cs.Compile.cs`
  生成 `Compile_*` 接口声明
- `SemanticWalker.cs.Generate.cs`
  生成“成员签名 -> `Compile_*` 方法”的装配字典

注意：

- `Op.Compile` 条目不会进入 `WhiteList.Members`
- 它走的是独立于 `Alias` / `Inline` / `Import` 的分发表

### 3. `SemanticWalker` 已持有分发表，并已接入主链路

`SemanticWalker` 构造函数里已经初始化 `_whiteListCompiles`，并调用 `Generate(...)` 完成装配。

当前 `GetWhiteListExpressionCore(...)` 会先尝试：

- `TryGetCompileExpression(...)`

只有 `Compile_*` 返回 `null` 时，才继续回落到：

- `Alias`
- `Inline`
- `Import`

所以状态是：

- `Compile_*` 方法和字典都在
- 主成员映射入口已经先尝试 `Compile`
- 但只有显式声明为 `Op.Compile` 的少数成员会命中这条路径

### 4. 当前 `Compile_*` 签名很窄

当前统一签名是：

```csharp
Expression? Compile_xxx(Expression? handler, Expression?[] args)
```

这意味着它现在天然缺少这些上下文：

- `SenseArgument`
- 原始 `IOperation`
- 稳定临时变量名来源
- 导入收集能力
- 声明提升能力
- sourcemap / `SourceOrigin` 级别的来源信息

这不是实现细节，而是当前 contract 的真实边界。

## 目标职责

`Op.Compile` 的职责不是“另一个更强的 Alias”。

它应该承担的是：

- 结构上不能安全写成 `Inline` 模板的宿主改写
- 需要精确控制求值顺序的表达式 lowering
- 需要根据参数形态决定最终 AST 的特殊映射

但在当前签名下，它更准确的定位应当是：

> 表达式级特殊编译钩子，而不是完整语义 lowering 子系统。

换言之，现阶段不应将其视为“可以承接任何复杂宿主语义”。

另外还要补两条 producer 侧约束：

> 能稳定用 `Inline` 表达的，不要升级成 `Import`。

> 能稳定用 `Inline` 或 `Import` 解决的，不要升级成 `Compile`。

原因是：

- `Inline` 仍属于编译期解糖
- `Import` 会引入额外模块实现、导入收集和运行时依赖
- `Compile` 属于编译器内部保留能力，应该只承接少数必须由编译器直接接管的内建例外
- 对 tuple、解构、普通表达式组合这类语法糖问题，优先目标应是“生成结果等价”，而不是过早下沉到运行时 helper

## 当前主分发顺序

当前主分发已经固定为以下顺序：

1. 先根据读/写语义解析真正的成员符号
2. 先尝试 `Op.Compile`
3. `Compile` 未处理时，再尝试 `Alias`
4. 再尝试 `Inline`
5. 再尝试 `Import`
6. 都未命中时，回到普通 lowering

写成一句话就是：

> `Compile -> Alias -> Inline -> Import -> normal lowering`

这样安排的原因是：

- `Compile` 是最强约束、最定制化的分支
- 它应该优先于模板和改名被询问
- 一旦复杂语义已经由 `Compile` 明确接管，就不应再让 `Inline` 或普通路径抢先落地

## `handler` / `args` 契约

当前实现已经把参数语义固定下来。

### `handler`

`handler` 表示成员访问宿主。

规则应为：

- 实例成员：传实例表达式
- 静态成员：传 `null`

### `args`

`args` 只表示显式调用参数，不包含实例宿主。

规则应为：

- 普通实例方法：只放方法参数
- 普通静态方法：放全部方法参数
- getter / 字段读取：空数组
- setter：只放被赋值的新值

这个约定必须和当前 `Inline` 路径区分开。

当前 `Inline` / `Import` 为了共用占位符布局，会把实例表达式并入参数数组前缀。

但 `Compile` 既然已经单独拥有 `handler` 参数，就不应该再把实例重复塞进 `args[0]`。

否则会出现两个问题：

1. 签名语义重复
2. 同一个 `Compile_*` 无法稳定区分“宿主”与“第一个真实参数”

## 返回语义

`Compile_*` 的返回语义建议固定如下。

### 返回表达式

表示：

- 当前 `Compile_*` 明确接管了该成员
- 返回值就是最终要使用的 AST 表达式
- 主分发立即停止，不再继续 `Alias` / `Inline` / `Import`

### 返回 `null`

表示：

- 该 `Compile_*` 选择放弃处理
- 允许主分发继续回落到后续链路

这里的 `null` 应被理解为：

> decline to handle

而不是“编译失败”。

### 抛异常

表示：

- 该 `Compile_*` 已经认领这条路径
- 但在生成过程中发现不能安全产出结果

这时不应静默 fallback。

原因是：

- 静默 fallback 会把“本应由 `Compile` 接管的复杂语义”重新丢回弱表达能力路径
- 最终更容易产出语义近似但错误的 AST

所以规则应当是：

> `throw` = claimed but failed，不再回退

## 与 `Inline` 的边界

这条边界必须写死，否则后面会反复漂移。

### 继续放在 `Inline`

适合：

- 结构稳定的纯表达式模板
- 不需要引入临时变量
- 不需要额外导入
- 不需要根据参数形状分支
- 不需要控制副作用顺序

补充原则：

- 只要能稳定写成 `Inline`，就不要为了“实现省事”退化成 `Import`

### 应升级到 `Compile`

适合：

- 不能稳定表达为单个模板表达式
- 同时也不适合作为运行时 helper 落到 `Import`
- 需要按参数结构选择不同 AST
- 需要精细控制访问顺序
- 且该语义本身属于编译器内部保留的内建改写，而不是普通库成员映射

### 仍然不适合当前 `Compile`

如果一个宿主改写需要这些能力：

- 新增 `var` / `let` 声明
- 合并 import specifier
- 依赖 `IOperation` 生成稳定临时名
- 记录或保留源位置信息
- 需要把 `throw` 分支编码成表达式约定，而不是普通语句级 `ThrowStatement`

那么它其实已经超出当前 `Compile(handler, args)` contract。

这种场景不应直接往现有 `Compile_*` 里硬塞，而应先扩展 hook contract。

### 才应该落到 `Import`

`Import` 应被视为最后手段。

更适合 `Import` 的场景是：

- 需要完整运行时实现
- 需要多条语句或可复用 helper 逻辑
- 需要异常、校验、循环、复杂状态处理
- 作为模块能力存在本身就比“编译期改写”更合适

所以 producer 侧的优先级应理解为：

> `Allowed/Alias -> Inline -> Import -> Compile`

这里的含义不是“`Compile` 比 `Import` 更强”。

真正的意思是：

- 先看能否直接映射
- 再看能否稳定模板化为 `Inline`
- 再看是否更适合作为运行时 helper 落到 `Import`
- 只有前面都不合适、且必须由编译器内部接管时，才保留给 `Compile`

consumer 侧主分发顺序仍然保持：

> `Compile -> Alias -> Inline -> Import -> normal lowering`

因为一旦 producer 已经明确把某个成员标成 `Compile`，consumer 就应优先信任这条内部特例分支，而不是再让普通链路抢先处理。

- 能用声明式模板解决的，不要引入模块
- 不能用模板但仍是编译期表达式改写的，再考虑 `Compile`
- 只有确实需要运行时实现时，才落 `Import`

## 接线后仍需保持的约束

`Compile` 虽然已经接进主链路，但下面几条约束仍然必须保持。

### 1. `Compile` 是否只做表达式级改写

当前答案仍然是“是”，所以当前签名还能成立。

如果答案是“否”，例如要：

- 引入临时变量
- 收集 import
- 参与 sourcemap 来源记录
- 统一表达“条件分支命中后直接抛异常”的表达式级协议

那就应先扩展签名，而不是继续往现有签名里堆特判。

### 2. fallback 语义是否统一

必须统一三种结果：

- 非 `null`：成功接管
- `null`：放弃处理，继续 fallback
- `throw`：失败并中止

这三者不能在不同调用点有不同解释。

### 3. 测试是否覆盖“decline”与“claim-fail”

`Op.Compile` 的测试不能只测 happy path。

至少应覆盖：

- 命中 compile 并返回表达式
- 命中 compile 但返回 `null`
- 命中 compile 后抛异常
- 未命中 compile，正常走 `Alias` / `Inline` / `Import`

## 当前建议

结合当前代码形态，更稳妥的推进顺序是：

1. 先把本文档中的分发顺序和返回语义固定下来
2. `GetWhiteListExpression(...)` 已接入 `_whiteListCompiles`，后续只补真实条目和回归测试
3. 第一阶段只允许 `Compile` 处理“自包含表达式级”改写
4. 真正需要 temp/import/source-origin 的场景，再升级 hook contract

这样做的优势在于：

- 不会把 `Compile` 过早宣传成“万能复杂 lowering 入口”
- 也不会继续把本该脱离模板的逻辑堆回 `Inline`

## 相关文档

- [WhiteList.md](./WhiteList.md)
- [SemanticWalker.WhiteList.md](./semantic-walker/SemanticWalker.WhiteList.md)
- [InlineAstTemplateSpec.md](./InlineAstTemplateSpec.md)
- [SyntaxTransformationPipeline.md](./SyntaxTransformationPipeline.md)
- [TransformationClosureChecklist.md](../../02-计划/compiler/TransformationClosureChecklist.md)
