# Jazor SourceMap 设计方案

## 1. 文档定位

本文档用于给出 Jazor 当前编译器的 sourcemap 设计方案。

它最初来自实现前设计阶段，但当前已经不只是“纯前置设计文档”。
仓库主线里已有 sourcemap baseline，因此这份文档现在同时承担两件事：

1. 明确 sourcemap 应该解决什么问题。
2. 明确 sourcemap 不应该在哪一层实现。
3. 为后续继续扩展覆盖面与稳定性提供一致的分层约束。

本文档以当前代码结构为约束，尤其考虑：

- `AstConverter` 负责模块级转换
- `SemanticWalker` 负责 `IOperation -> Acornima AST`
- 最终 JavaScript 文本由 `ToKnRECMAScript()` / `ToECMAScript()` 输出
- 编译器内部存在大量 lowering，例如 tuple、pattern、deconstruct、with、collection expression 等

## 2. 设计结论

### 2.1 总结版

Jazor 的 sourcemap 应该按三层实现：

1. 源来源标注层
   在生成 Acornima AST 时，把 JS AST 节点绑定回对应的 C# 源位置。
2. 映射构建层
   在 JavaScript writer 输出文本时，把“生成位置 -> 原始位置”编码成标准 source map。
3. 发射集成层
   在 emit 阶段把 `.mjs`、`.mjs.map` 和 `sourceMappingURL` 一起写出。

### 2.2 核心原则

- sourcemap 服务的是“源级调试体验”，不是“还原 lowered JS 结构”
- 一个 C# 源节点映射到多个 JS 片段是允许的
- 编译器插入的临时变量和胶水节点不应主导调试体验
- 不在 `SemanticWalker` 里直接拼 VLQ，也不在 emit 阶段反推 AST 映射

## 3. 目标与非目标

### 3.1 目标

第一阶段 sourcemap 应满足：

1. 浏览器 DevTools 能正确把断点、异常位置和单步回到 C# 源码。
2. tuple / deconstruct / pattern 这类 lowering 后的代码仍保持可用调试体验。
3. sourcemap 不改变现有 lowering 结果，也不引入新的运行时结构。
4. 实现落点与现有 `AstConverter -> SemanticWalker -> ToJavaScript -> Emit` 链路兼容。

### 3.2 非目标

第一阶段不追求：

1. token 级极致精度
2. bundle 级 sourcemap chaining
3. 从已输出 JS 文本反推 source map
4. 让所有 synthetic 节点都可见
5. 让 sourcemap 参与语义判断或 lowering 决策

## 4. 为什么这份设计文档在 baseline 落地后仍然重要

当前编译器仍在持续完善 tuple、pattern、reference、creation 等 lowering 逻辑。

这份设计文档最初要解决的问题是：如果 sourcemap 在 lowering 未稳定前就接入，会立刻遇到两个问题：

1. lowering 形状会继续变
   任何 JS AST 结构变化都会连带修改 mapping 行为和测试断言。
2. sourcemap 会放大未稳定路径的维护成本
   一个语法点一旦既要修 lowering，又要修 map，很容易把主问题掩盖掉。

这也是当时更合理的顺序：

1. 先把编译器主链路和主要 lowering 行为稳定下来
2. 再基于稳定 AST 输出模型挂 sourcemap

现在 baseline 已经落地，但本文档仍然需要保留，因为后续每次扩展新的语法域覆盖时，仍然会反复遇到同一类边界问题：source-origin 应挂在哪、synthetic 该如何处理、哪些事情不能下沉到 emit 反推。

## 5. 现有代码结构约束

### 5.1 当前输出链路

当前主链路大致如下：

```text
Roslyn IOperation
    -> SemanticWalker
    -> Acornima AST
    -> ToKnRECMAScript / ToECMAScript
    -> ESGenerator catalog
    -> Emit 落盘
```

### 5.2 当前 baseline 已落地的事实

当前仓库中已经存在这些 sourcemap 主链路事实：

- `SourceOrigin` 已作为来源模型接入 compiler 主链路
- writer 侧已能生成 source map 内容
- `ESGenerator` 已能记录 `SourceMapRelativePath`、`SourceMapContent`、`MapHash`
- `Jazor.Emit` 已能写出 `.mjs`、`.mjs.map` 与 `sourceMappingURL`

因此现在更准确的说法不是“要不要新建一条 sourcemap 产物链路”，而是：

- baseline 链路已经存在
- 后续重点是扩大语法域覆盖、锁定文本与映射一致性、继续巩固 compiler / emit 契约

## 6. 总体架构

### 6.1 分层

#### A. Source Origin 层

职责：

- 在 AST 节点上保存其来源 C# 位置
- 不负责编码 sourcemap
- 不负责落盘

建议落点：

- `SemanticWalker`
- `AstConverter`
- Acornima `Node.UserData`

#### B. SourceMap Builder 层

职责：

- 在输出 JavaScript 文本时读取节点来源
- 记录 generated line/column 与 original line/column 的对应关系
- 生成标准 source map v3 JSON

建议落点：

- `Jazor.Compiler` 内部新增 writer 扩展或 source map builder

#### C. Emit Integration 层

职责：

- 写 `.mjs`
- 写 `.mjs.map`
- 在 `.mjs` 末尾补 `//# sourceMappingURL=...`

建议落点：

- `ESGenerator` 扩展 catalog 结构
- `Jazor.Emit` 扩展读取和写出逻辑

### 6.2 不建议的实现方式

以下方式不建议采用：

1. 在 `SemanticWalker` 内直接拼 source map `mappings`
2. 在 emit 阶段仅根据最终 JS 文本回推 source map
3. 用注释占位方式模拟 mapping
4. 让 sourcemap 逻辑渗入 tuple / pattern 具体语义判断

原因是这些方案都会混淆“语义 lowering”和“调试映射”两层职责。

## 7. Source Origin 模型

### 7.1 建议的数据结构

```csharp
internal sealed record SourceOrigin(
    string SourcePath,
    int StartLine,
    int StartColumn,
    int EndLine,
    int EndColumn,
    string? Name = null,
    bool IsSynthetic = false);
```

说明：

- 行列建议统一使用 0-based
- `Name` 可选，第一阶段可不强依赖
- `IsSynthetic` 用于标记编译器插入节点

### 7.2 建议挂载位置

建议直接使用 Acornima `Node.UserData`：

- 不改 AST 类型体系
- 不增加运行时依赖
- 可在 writer 输出阶段直接读取

### 7.3 设计边界

`SourceOrigin` 只描述“这个 JS 节点来自哪里”，不描述：

- sourcemap segment 编码状态
- 生成后行列
- bundler 信息
- runtime 含义

它必须保持薄、稳定、与 emit 解耦。

## 8. 哪些节点需要挂 Source Origin

### 8.1 第一优先级

这些节点应优先挂 origin：

- `Literal`
- `Identifier`
- `MemberExpression`
- `CallExpression`
- `AssignmentExpression`
- `VariableDeclarator`
- `ReturnStatement`
- `ExpressionStatement`
- 控制流入口节点

### 8.2 第二优先级

这些节点在 lowering 稳定后补：

- `ObjectExpression`
- `ArrayExpression`
- `ConditionalExpression`
- `SwitchExpression` lowering 结果
- `With` lowering 结果
- `CollectionExpression` lowering 结果

### 8.3 不应主导调试的节点

这些节点通常应标记为 synthetic，或弱化映射：

- 编译器插入的 temp `Identifier`
- 为避免重复求值插入的缓存赋值
- lowering glue 的 `SequenceExpression` 辅助项
- import rewrite 产生的中间节点
- bundle 阶段的入口拼接文件

## 9. 对 lowering 的统一规则

这是整个 sourcemap 设计里最重要的一条。

### 9.1 允许一源多目标

一个 C# 源节点映射到多个 JS 片段是合法且正常的。

典型例子：

- tuple swap
- deconstruction
- recursive pattern
- collection expression lowering

### 9.2 真实语义片段与 synthetic 片段分离

每次 lowering 后的生成节点，要分成两类：

1. 用户可感知语义片段
   应尽量保留源来源。
2. 编译器胶水片段
   应标 synthetic，避免污染调试体验。

### 9.3 外层与子层同时保留

一般建议：

- lowering 的根节点保留父级语句来源
- 关键子表达式尽量保留更细粒度来源

这样可以兼顾：

- 断点定位到整句
- 单步进入关键子表达式时仍能回到更细位置

## 10. tuple / deconstruct 的 sourcemap 规则

tuple 是 Jazor 当前 lowering 最典型的场景，也是 sourcemap 设计的标尺。

### 10.1 tuple 的调试目标

tuple 在 Jazor 中是语法糖，不是新 runtime 类型。

因此 sourcemap 的目标不是强调：

- 运行时对象 key
- 中间 projection 结构
- 临时变量

而是尽量让开发者感觉自己仍在调试 tuple 表达式。

### 10.2 tuple literal

例如：

```csharp
(name: "John", age: 30)
```

lower 后即使输出成：

```js
{ name: "John", age: 30 }
```

映射也应优先保留：

- 整个 `ObjectExpression` 对应整个 tuple 表达式
- 每个 value 节点对应各自 tuple element 表达式

第一阶段不要求 object key 自身拥有高精度独立映射。

### 10.3 tuple remap

例如：

```csharp
(string name, int age) source = ("John", 30);
(string first, int years) target = source;
```

lower 后可能为：

```js
let target = { first: source.name, years: source.age };
```

建议映射规则：

- 外层 projection 根节点映射到原右值 tuple 表达式
- `source.name` / `source.age` 这类读取表达式映射到对应源 tuple element
- synthetic projection glue 不应抢占主要断点位置

### 10.4 deconstruct / swap

例如：

```csharp
(a, b) = (b, a);
```

可能 lower 为：

```js
v$0 = b, v$1 = a, a = v$0, b = v$1;
```

建议映射规则：

- 整个 `SequenceExpression` 映射到整条赋值语句
- `b` 对应右侧第一个源元素
- `a` 对应右侧第二个源元素
- 左侧 `a = ...` 对应左侧第一个目标元素
- 左侧 `b = ...` 对应左侧第二个目标元素
- `v$0` / `v$1` 与缓存赋值标记为 synthetic

这样调试体验会聚焦在用户变量，而不是编译器临时变量。

## 11. pattern / with / collection lowering 的规则

这些语法点虽然形状不同，但 sourcemap 原则与 tuple 相同。

### 11.1 pattern

- pattern 的判定表达式映射回原 pattern 输入
- lowering 后插入的 guard / helper / cache 节点标 synthetic
- 各分支关键比较节点尽量映射到原 pattern 子表达式

### 11.2 with

- 整体对象复制映射回 `with` 语句本体
- 每个成员更新表达式映射到对应 member assignment
- 中间 clone glue 标 synthetic

### 11.3 collection expression

- 外层 collection literal 映射回原 collection expression
- 每个元素映射回对应源元素
- spread、tuple remap、内部缓存按各自来源处理

## 12. SourceMap Builder 设计

### 12.1 最小实现目标

第一阶段不要求 token 级全覆盖，建议仅做“节点开始位置映射”。

也就是：

- 输出某个节点时
- 如果该节点有非 synthetic `SourceOrigin`
- 就记录一条 generated position -> original position

这已经足够支持大部分调试需求。

### 12.2 Builder 应维护的状态

最小状态建议包括：

- `generatedLine`
- `generatedColumn`
- `sources`
- `names`
- 按行存储的 segments
- 上一条 segment 的 source/name/original/generated 状态

### 12.3 编码格式

最终输出使用标准 source map v3：

- `version`
- `file`
- `sources`
- `sourcesContent`
- `names`
- `mappings`

### 12.4 `names`

`names` 是增强项，不是第一阶段主目标。

第一阶段可以：

- 只在 `Identifier` / `MemberExpression` / `CallExpression` 中适度写入
- 或直接留空

不应为了 `names` 复杂化整个主链路。

## 13. 输出与落盘设计

### 13.1 编译器产物建议

建议引入并行 artifact 概念：

```csharp
internal sealed record GeneratedJavaScriptArtifact(
    string Content,
    string? SourceMapContent,
    string JsHash,
    string? MapHash);
```

### 13.2 `ESGenerator`

当前 `ESGenerator` 已经能同时生成：

- `Content`
- `SourceMapRelativePath`
- `SourceMapContent`
- `JsHash`
- `MapHash`

这样 sourcemap 不会在从 AST 进入 catalog 这一层时丢失。后续重点是保持这些字段的稳定消费契约，而不是再次讨论是否需要它们。

### 13.3 `Emit`

当前 emit 阶段已经负责：

1. 写 `.mjs`
2. 写 `.mjs.map`
3. 在 `.mjs` 末尾追加：

```js
//# sourceMappingURL=xxx.mjs.map
```

后续仍要持续锁定：

- `sourceMappingURL` 使用相对文件名
- hash 逻辑要明确 JS 与 map 是否分离计算

## 14. `sources` 与 `sourcesContent`

### 14.1 `sources`

建议最终输出时使用 repo-relative 或逻辑相对路径，不直接暴露本机绝对路径。

不要在 `SemanticWalker` 层过早做路径规范化，应在 builder 最终输出阶段统一处理。

### 14.2 `sourcesContent`

建议第一阶段就写入 `sourcesContent`。

原因：

- DevTools 不依赖源码文件实际存在于磁盘
- 调试体验更稳定
- sourcemap 产物自包含

## 15. bundler 范围

### 15.1 第一阶段不处理 map chaining

`ModuleBundler` 当前已经承担：

- import rewrite
- bundle 入口组装
- Deno bundle

如果第一阶段再引入 bundle map chaining，会把复杂度一下抬高。

### 15.2 建议阶段化

建议分两阶段：

1. 先做好模块级 sourcemap
2. 后续若确实需要 bundle 级调试，再处理 module map -> bundle map 合并

第一阶段不要因为 bundle map 把主链路拖慢。

## 16. 后续推进顺序建议

在 baseline 已落地的前提下，建议按以下顺序继续推进：

1. 继续锁定 `SourceOrigin` 与 helper 的传播规则
2. 给更多普通语法域和高风险 lowering 补 origin / mapping 覆盖
3. 继续锁定 `ToKnRECMAScriptWithSourceMap(...)` 与普通 writer 的文本一致性
4. 继续锁定 `ESGenerator` catalog 与 `Jazor.Emit` 物化契约
5. 最后再评估 bundler map / chaining 是否需要进入更广主线

## 17. 测试策略

### 17.1 第一批测试目标

先验证“链路存在且结构正确”，而不是覆盖所有语法点。

建议至少包括：

1. 简单赋值有有效 mapping
2. 调用表达式有有效 mapping
3. tuple literal 有有效 mapping
4. deconstruct / swap 中 temp 节点不主导 mapping
5. `.mjs` 末尾存在 `sourceMappingURL`
6. `.mjs.map` JSON 结构合法

### 17.2 第二批测试目标

等编译器 lowering 稳定后，再扩展：

- nested tuple remap
- pattern
- with
- collection expression
- object initializer
- array creation

## 18. 风险与取舍

### 18.1 主要风险

1. lowering 结构尚未完全稳定
2. synthetic 节点过多时，容易污染调试体验
3. writer 扩展点若不足，可能需要自行接管 JS 输出层

### 18.2 当前取舍

当前明确选择：

- 先保证 baseline 稳定，不急于过度扩张覆盖面
- broad compiler contract 先做模块级 map，不默认要求 bundle map chaining
- 先做节点级映射，不做 token 级极限精度
- sourcemap 不反向影响 lowering 语义

## 19. 最终结论

Jazor 的 sourcemap 不应被理解为“给输出 JS 附带一个额外文件”，而应被理解为：

1. 编译期对源来源的稳定建模
2. 输出期对这些来源的标准化编码
3. 发射期对 `.mjs/.map` 产物的一致落盘

在当前仓库结构下，更合理的路径是：

- 承认 sourcemap baseline 已经进入主链路
- 继续按本文档约束去扩大覆盖面并巩固稳定契约

这样既不会把 sourcemap 再写回成“未实现”，也能避免后续扩展重新耦合进不受控的 lowering 细节里。
