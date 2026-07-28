# Jazor SourceMap 实施清单

> Status: 活跃计划
> Positioning: compiler SourceMap rollout 与 refinement 的主执行清单。
> Note: 当前基线已经存在；本页负责组织后续扩展与收敛动作，具体阶段成果应回写状态页或正文。

## 1. 文档定位

本文档是 [SourceMap.Design.md](../../01-目标/compiler/sourcemap/SourceMap.Design.md) 的配套实施清单。

目标不是重复设计讨论，而是把 sourcemap 后续 rollout 与 refinement 拆成可执行步骤。

当前关联入口：

- [SourceMap 状态（2026-04-06）](../../03-完成/sourcemap/status.md)
- [SourceMap 目标与设计](../../01-目标/compiler/sourcemap/SourceMap.Design.md)
- [RazorVue 工具链计划](../razorvue-transition/04-工具链.md)

说明：

- 原先分散在不同路线下的 sourcemap / bundle chaining 计划不再作为独立入口维护；
- 当前应以 SourceMap 设计、状态页、`Jazor.Emit` 物化契约和 RazorVue 工具链计划为准。

## 2. 当前基线与启动前门槛

当前 compiler/emit 链路里，下面这些基础能力已经存在：

1. `SourceOrigin` 与 `WithOrigin*` helper
2. `ToKnRECMAScriptWithSourceMap(...)`
3. `ESGenerator` catalog 中的 `SourceMapContent`
4. `Jazor.Emit` 对 `.mjs.map` 与 `sourceMappingURL` 的输出
5. `SemanticWalkerSourceOriginTest`、`SemanticWalkerSourceMapEmissionTest`、`ESGeneratorSourceMapCatalogTest`、`StaticModuleSourceMapTests` 等回归测试

因此当前问题已经不是“能不能开始 sourcemap 实现”，而是：

- 哪些范围已经具备可用基线
- 哪些范围仍然只适合在 narrower active lane 中推进
- 哪些稳定性契约还需要继续巩固

如果要继续扩大 sourcemap 覆盖面，仍建议先确认以下条件稳定：

1. tuple lowering 主路径稳定
2. deconstruct / pattern / collection expression 主路径稳定
3. `ToKnRECMAScript()` 的输出格式短期内不会频繁变化
4. `ESGenerator -> catalog -> Emit` 主链路字段结构基本稳定

如果这些前提重新进入频繁变动期，应先稳住 compiler 主体与输出契约，再继续扩 sourcemap 覆盖。

## 3. 第一阶段范围

第一阶段只做：

1. 模块级 `.mjs.map`
2. 普通表达式、引用、赋值、调用、return 的 mapping
3. tuple / deconstruct 的 mapping
4. emit 落盘与 `sourceMappingURL`

第一阶段不做：

1. bundle map chaining
2. token 级极致 mapping
3. 所有语法点一次性全覆盖

### 3.1 当前“全节点 sourcemap”口径（active lane）

当前执行层里的“全节点”定义为：

1. 对 `SemanticWalker` 中“已支持且会产出 `Node`”的 `Visit*` 操作，`Visit(operation, ...)` 返回的根节点必须带 `SourceOrigin`
2. sourcemap 只对可输出的语义节点做保证，不对未支持语法点做伪映射

明确不纳入“全节点”保证的范围：

1. `SemanticWalker.cs.NotSupport.cs` 中明确 `NotSupport` 的操作（抛错或返回 `null`）
2. 由父节点统一处理、子节点自身不产出独立 AST 节点的场景（例如 `IDefaultCaseClauseOperation`）
3. `IAttributeOperation` 当前未产出 decorator 节点的路径（返回 `null`）

形态依赖（条件覆盖）：

1. `IImplicitIndexerReferenceOperation` 是否出现受 Roslyn operation tree 形态影响
2. 测试策略为“出现即强断言 SourceOrigin；未出现则通过 `IArrayElementReferenceOperation` 路径兜底验证”

对应回归测试入口：

1. `src/Jazor.CompilerTest/SemanticWalkerSourceOriginTest.cs`
2. `src/Jazor.CompilerTest/SemanticWalkerSourceMapEmissionTest.cs`
3. `src/Jazor.CompilerTest/ESGeneratorSourceMapCatalogTest.cs`
4. `src/Jazor.EmitTest/StaticModuleSourceMapTests.cs`

## 4. 历史第一阶段实现顺序（大体已落地）

这部分保留为“已落地基线的大致实施顺序”，方便后续理解当前代码为什么分层成现在这样。

### Step 1. 新增基础类型

当时新增的基础类型包括：

- `SourceOrigin`
- `GeneratedJavaScriptArtifact`
- `GeneratedSourceMap` 或等价 builder 输出模型

目标：

- 固定 sourcemap 的最小内部数据结构

完成标准：

- 类型定义稳定
- 不接入现有链路也能独立编译

### Step 2. 在 `SemanticWalker` 加 helper

当时接入的 helper 包括：

- `CreateOrigin(...)`
- `WithOrigin(...)`
- `WithOriginIfMissing(...)`
- `WithSyntheticOrigin(...)`

目标：

- 为 AST 节点挂来源，不改 lowering 语义

完成标准：

- helper 独立可用
- 初始接入阶段还未大规模铺开到所有 `Visit`

### Step 3. 接入第一批 `Visit`

优先接入：

- `VisitLiteral`
- `VisitLocalReference`
- `VisitParameterReference`
- `VisitFieldReference`
- `VisitPropertyReference`
- `VisitInvocation`
- `VisitReturn`
- `VisitSimpleAssignment`

目标：

- 先让普通表达式主链路具备 origin 基线

完成标准：

- 这批节点生成的 AST 可稳定读到 `SourceOrigin`

### Step 4. 接入 tuple / deconstruct

优先接入：

- `VisitTuple`
- `TranslateTupleForTarget(...)`
- `VisitDeconstructionAssignment`

目标：

- 固定 lowering 热点的映射原则

完成标准：

- tuple projection 根节点有来源
- deconstruct 中真实赋值与 synthetic temp 区分清楚

### Step 5. 新增 JS 输出接口

当时新增的输出接口包括：

- `ToKnRECMAScriptWithSourceMap(...)`
- 如有需要，新增 `ToECMAScriptWithSourceMap(...)`

目标：

- 不破坏原有 `ToKnRECMAScript()` 的情况下，并行输出 JS 与 map

完成标准：

- 输入 AST，能得到 `Content + SourceMapContent`

### Step 6. 扩 `ESGenerator`

目标：

- 让 catalog 能携带 map 信息，而不是只存 JS 内容

完成标准：

- `GeneratedModuleInfo` 增加 map 相关字段
- catalog 生成源码包含这些字段

### Step 7. 扩 `Jazor.Emit`

目标：

- `CatalogReader` 能读 map
- `ModuleWriter` 能写 `.mjs.map`

完成标准：

- 模块输出目录存在 `.mjs` 和对应 `.mjs.map`
- `.mjs` 尾部有 `sourceMappingURL`

### Step 8. 扩测试

目标：

- 用最小测试锁住 sourcemap 主链路

完成标准：

- 编译器侧与 emit 侧至少各有一组回归测试

## 5. 当前剩余动作

在已落地基线之上，当前更需要继续推进的是：

1. 扩大覆盖面时继续遵守 source-origin / synthetic 节点边界
2. 巩固 `ESGenerator -> catalog -> Emit` 的真实输出一致性
3. 把 narrower active lane 中已验证的策略有节制地回灌到广义清单
4. 持续锁定 tuple / deconstruct / pattern / conditional access 等 lowering 热点的调试体验
5. 避免在 sourcemap 扩展过程中反向破坏已有 lowering 契约

## 6. 文件级实施清单

### 6.1 `Jazor.Compiler`

建议关注文件：

- [SemanticWalker.cs](/D:/repository/own/jazor/Jazor/src/Jazor.Compiler/core/SemanticWalker.cs)
- [SemanticWalker.cs.Ordinary.cs](/D:/repository/own/jazor/Jazor/src/Jazor.Compiler/core/SemanticWalker.cs.Ordinary.cs)
- [SemanticWalker.cs.Reference.cs](/D:/repository/own/jazor/Jazor/src/Jazor.Compiler/core/SemanticWalker.cs.Reference.cs)
- [SemanticWalker.cs.Tuple.cs](/D:/repository/own/jazor/Jazor/src/Jazor.Compiler/core/SemanticWalker.cs.Tuple.cs)
- [Util.cs](/D:/repository/own/jazor/Jazor/src/Jazor.Compiler/Util.cs)
- [ESGenerator.cs](/D:/repository/own/jazor/Jazor/src/Jazor.Compiler/ESGenerator.cs)

### 6.2 `Jazor.Emit`

建议关注文件：

- [CatalogReader.cs](/D:/repository/own/jazor/Jazor/src/Jazor.Emit/CatalogReader.cs)
- [ModuleWriter.cs](/D:/repository/own/jazor/Jazor/src/Jazor.Emit/ModuleWriter.cs)
- [ModuleCollector.cs](/D:/repository/own/jazor/Jazor/src/Jazor.Emit/ModuleCollector.cs)
- [ManifestModel.cs](/D:/repository/own/jazor/Jazor/src/Jazor.Emit/ManifestModel.cs)

### 6.3 测试

建议继续新增或扩展：

- `Jazor.CompilerTest` 中的 sourcemap 结构测试
- `Jazor.EmitTest` 中的落盘测试

## 7. 第一批必测用例

建议至少覆盖这些场景：

1. 简单局部变量赋值
2. 成员访问 + 调用
3. `return` 表达式
4. tuple literal
5. tuple remap
6. deconstruction
7. swap
8. `.mjs.map` 落盘
9. `sourceMappingURL` 正确生成

## 8. synthetic 节点检查清单

实现时要专门检查这些节点是否错误进入主 mapping：

1. `GetUniqueName(...)` 生成的临时变量
2. tuple / pattern 的缓存赋值
3. lowering glue 的中间 sequence 片段
4. import rewrite 中间节点
5. bundle 入口文件内容

原则：

- synthetic 节点可以存在
- 但不能成为主要断点落点

## 9. 输出一致性检查清单

实现完成后，应逐项确认：

1. sourcemap 不改变现有 `.mjs` 语义
2. 不引入新的 runtime tuple / helper 类型
3. 不影响 tuple remap / deconstruct / pattern 的已有测试结果
4. 不破坏 `ESGenerator` catalog 的消费链路
5. 不改变 `ModuleBundler` 当前行为边界

## 10. 当前不默认纳入 broad contract 的项

这些能力当前仍建议保持为更窄 lane 或后续增量目标，不默认混入 broad compiler SourceMap contract：

1. bundle map chaining
2. `names` 精细化
3. token 级 mapping
4. 所有语法点全覆盖
5. sourcemap 性能优化

## 11. 验收标准

### 11.1 历史第一阶段基线验收口径

下面这组口径用于解释“为什么当前仓库可以被描述为 baseline 已落地”：

1. 模块级 `.mjs.map` 能稳定生成
2. DevTools 能把主要断点与异常位置映射回 C# 源
3. tuple / deconstruct 的调试体验不被 temp 节点主导
4. sourcemap 不改变现有 lowering 结果
5. 相关测试稳定通过

### 11.2 当前继续扩展时的验收口径

后续如果继续扩大覆盖面，至少还应同时满足：

1. 新增语法域不会破坏既有模块级 `.mjs.map` 稳定性
2. `SourceOrigin -> ESGenerator -> Emit` 链路的输出契约继续保持确定性
3. synthetic 节点与 temp 节点不会反客为主，覆盖主要断点落点
4. narrower active lane 中验证过的策略，只有在 broad contract 能稳定承受时才回灌
5. 新增覆盖面对应的回归测试与失败路径测试同步补齐

## 12. 结论

这份清单当前的作用不是从零启动 sourcemap，而是保证后续继续扩覆盖面时：

- 有明确边界
- 有固定顺序
- 不会在实现过程中重新讨论同一批设计问题

如果要讨论更大范围的 rollout 或 design 取舍，再回到 [SourceMap.Design.md](../../01-目标/compiler/sourcemap/SourceMap.Design.md) 看每一项背后的设计理由。
