# RazorVue Block Code Execution Model

> Status: In Progress
> Updated: 2026-05-19
> Scope: `src/Jazor.RazorVue` full block-code support architecture

## 1. Why This Document Exists

RazorVue 当前对模板 authoring 的核心优势，是能把一大类 Razor / `BuildRenderTree` 输入收敛成稳定、可解释、可验证的声明式模板语义：

- element / component / text / expression
- slot / template
- template-scoped local
- `if`
- `foreach`
- count-style `for`

这条路线对正常 UI authoring 非常有效，但它本质上仍然是：

- 一个声明式模板树恢复器
- 一个受控切片的 code-block 结构化器

它不是完整的 C# block statement 执行模型。

随着真实项目进入复杂业务区，用户会自然写出：

- `while`
- `switch`
- `try/catch/finally`
- 局部变量多次写入
- 赋值后再使用
- helper 内复杂控制流
- code-block 中前后混排的多段业务逻辑

如果继续沿着“遇到一种语句就补一个前端特判”的方式推进，RazorVue 会逐渐变成：

- handwritten `BuildRenderTree` 一套 statement 特判
- Razor IR frontend 再复制一套 statement 特判
- SFC / H lowering 再对每种特殊节点各写一套语义

这条路线最终不可维护，也无法保证两条前端与多条 lowering 车道长期一致。

因此，从 2026-05-18 开始，RazorVue 对“完整 block code 支持”的设计正式收敛为：

- **声明式模板通道**
- **命令式渲染通道**

而不是继续无上限扩张现有模板树节点。

## 2. Problem Statement

### 2.1 Current Strength

当前系统擅长的是：

- 识别可 lift 的模板结构
- 保留模板局部作用域
- 保留 slot/context 语义
- 用 `RazorVueExpressionEmitter` 和 canonical model 生成高质量 H / SFC 输出

### 2.2 Current Boundary

当前系统不擅长，也不应该继续伪装擅长的是：

- 任意 block statement 的逐语句解释
- 任意局部状态机
- 任意控制流逃逸分析
- 任意异常路径恢复
- 任意“builder API 调用序列”与声明式 template 之间的双向自由转换

### 2.3 Core Design Tension

完整 block code 的需求，和当前 template-lift 架构之间的根本张力是：

- 一边是 Vue template / SFC / canonical template tree 要求的声明式结构
- 一边是 C# block code 天然允许的命令式执行语义

真正的问题不是“还少几个节点类型”，而是：

- 是否承认命令式渲染是一等语义
- 如何在不污染现有模板通道的前提下，引入它

## 3. Design Goals

完整 block-code 支持必须满足以下目标：

1. 不破坏现有声明式模板主通道的清晰度与可维护性。
2. 不要求 handwritten `BuildRenderTree` 与 Razor IR frontend 各自维护一套越来越大的 statement 特判矩阵。
3. 对复杂业务 block 提供一条正式、稳定、可测试的命令式渲染通道。
4. 保持 fail-fast 原则：不能正确 lower 的 block，仍然必须稳定失败，不能 silent fallback。
5. 支持分阶段落地：先 correctness，再细化局部 promotion，再优化 SFC 体验。

## 4. Non-Goals

本设计明确不追求：

1. 把所有 C# block code 都重新 lift 回 `<template>`。
2. 在第一阶段就实现任意 block 的最小化局部 promotion。
3. 在第一阶段就要求 handwritten frontend、Razor IR frontend、H lowering、SFC lowering 都达到最优输出。
4. 把 RazorVue 变成通用 CLR UI 解释器。

## 5. Two-Channel Architecture

### 5.1 Declarative Template Channel

保留现有主通道，不改变其核心定位：

- 输入：可结构化的模板 authoring
- 中间形状：`RazorVueRenderFragment` + 现有声明式节点
- 输出：
  - canonical model
  - H render lowering
  - SFC template lowering

这条通道继续优先承接：

- markup / element / component
- expression
- slot / template
- template-scoped local
- `if`
- `foreach`
- count-style `for`
- 已支持的 helper / fragment carrier / current-component forwarding 语义

### 5.2 Imperative Render Channel

新增正式的“命令式渲染通道”，用于承接无法继续安全 template-lift 的 block code。

该通道的职责不是把 block 重新翻译成声明式节点，而是承认：

- 这是一段命令式渲染程序
- 它需要按 C# block code 的执行语义运行
- 它的主要输出目标是 render-function / imperative render program，而不是 template string

该通道的典型输入包括：

- `while`
- `switch`
- `try/catch/finally`
- 赋值后再读的局部变量
- 多次 mutation 的局部状态
- 无法继续以现有 template-scope node 表达的复杂 helper body
- 需要完整 block statement 语义的 code-block

## 6. Promotion Rule

### 6.1 Promotion Instead of Special-Casing

未来当前端遇到不再属于声明式通道的 block 结构时，不再默认做“加一个新模板节点类型”。

应改为执行 promotion：

- 若当前 block 可被单独封闭，则提升为一个命令式 block 节点
- 若当前 block 中存在会影响外层兄弟节点或整体控制流的语义，则向外继续提升
- 必要时提升到整个 render body

### 6.2 Promotion Levels

#### Level A: Local Block Promotion

适用条件：

- block 的 capture 边界可封闭
- block 外前后兄弟节点的顺序可稳定拼接
- block 不改变外层渲染协议边界

目标：

- 只把当前复杂区域切到命令式通道
- 其余部分仍保留声明式 template-lift

#### Level B: Method/Body Promotion

适用条件：

- block 内出现无法局部封闭的控制流
- `return` / 提前退出 / 复杂异常路径会影响后续兄弟节点
- builder 协议边界无法局部截断

目标：

- 直接把整个 render body 切到命令式通道

### 6.3 Important Constraint

promotion 是正式架构决策，不是隐式降级技巧。

也就是说：

- 它必须有明确节点形状
- 有明确 lowering 路径
- 有明确 issue / diagnostics / artifact identity 影响
- 有明确文档合同

## 7. New Intermediate Model

### 7.1 New Node Family

在 `RazorVueRenderTree` 层新增一类正式节点，用于表达命令式 block。

建议最小形状：

```csharp
internal sealed record RazorVueImperativeBlockNode(
    IOperation Operation,
    RazorVueImperativeBlockKind Kind,
    ImmutableArray<ILocalSymbol> VisibleLocals,
    ImmutableArray<IParameterSymbol> VisibleParameters,
    ImmutableArray<RazorVueSourceOrigin> Origins) : RazorVueRenderNode(Origins);
```

说明：

- `Operation` 保留原始 Roslyn block / statement 入口
- `Kind` 用于诊断和 lowering 路径选择
- `VisibleLocals` / `VisibleParameters` 明确 capture 边界
- 不再尝试把这类 block 展平成一串伪声明式子节点

### 7.2 New Kind Classification

建议至少区分：

- `LocalBlock`
- `LoopBlock`
- `SwitchBlock`
- `TryBlock`
- `MethodBody`

这些 kind 不是为了让前端继续按种类堆特判，而是为了：

- diagnostics 更可解释
- identity / HMR / future optimization 更稳定

## 8. Frontend Responsibilities

### 8.1 Handwritten `BuildRenderTree` Frontend

handwritten frontend 不再把“不属于 if/foreach/for 的每种语句”都视为永久 unsupported。

它的新职责是：

1. 尽量恢复声明式模板子集。
2. 一旦遇到复杂 block，决定 promotion 范围。
3. 产出命令式 block 节点，而不是继续扩 statement 特判。

### 8.2 Razor IR Frontend

Razor IR frontend 与 handwritten frontend 必须收敛到同一 promotion 语义。

它的新职责也是：

1. 尽量恢复声明式模板子集。
2. 一旦 block 不能继续稳定结构化，做同一套 promotion 决策。
3. 产出同一种命令式 block 节点，而不是独立发明另一套 IR-only node。

### 8.3 Hard Rule

handwritten 与 Razor IR 两条前端的职责应该是：

- **恢复 / promote**

而不是：

- **分别实现两套复杂 block 解释器**

## 9. Lowering Strategy

### 9.1 H / Render-Function Is the Primary Target

命令式 block 的第一目标输出是：

- H render function
- 或者更一般的 render-program lowering

它不是 template string，不应强行走 template-lift。

### 9.2 SFC Strategy

SFC 输出从此分为两类：

#### Class 1: Template-Liftable SFC

- 仍输出 `<template> + <script setup>`
- 继续是默认最佳体验

#### Class 2: Render-Function SFC

- 当 render tree 中存在命令式 block 节点时
- 组件输出可退化为 render-function SFC
- 仍然是正式支持，不应直接视为失败

也就是说：

- “不能 lift 到 `<template>`”
- 不等于
- “不能作为 RazorVue 组件支持”

### 9.3 Historical Temporary Rule

早期 Phase 1 允许过一段临时状态：

- H / `.mjs` artifact 支持命令式 block
- SFC artifact 对这类组件给出显式稳定诊断

该临时状态已结束。当前命令式 block 已进入正式 render-function SFC 主线，不再把“无法 lift 到 `<template>`”等同于“SFC artifact 失败”。

## 10. Runtime Contract

本设计默认现有 Jazor runtime 路径可承接命令式 block 的 render-program lowering。

因此本文件不重新定义基础 `RenderTreeBuilder` 运行时事实来源，而只要求：

1. 命令式 block lowering 必须基于现有可支持的 builder/runtime 语义。
2. 不允许在 RazorVue 内偷偷引入另一套与 `BuildRenderTree` 不一致的私有渲染协议。
3. 如果未来运行时合同仍需要增强，应作为 CLR/compiler/runtime 正式工作项处理，而不是前端特判回避。

## 11. Diagnostics Contract

### 11.1 Current Problem

当前大量复杂 block authoring 失败时，用户只会看到：

- `CanonicalizationFailed`

这对长期架构演进不够精确。

### 11.2 New Direction

后续应逐步把“复杂 block 未能进入正式命令式通道”的失败与“普通模板 canonicalization 失败”区分开。

建议未来新增更精确的 issue code，例如：

- `UnsupportedBlockExecutionModel`
- `UnsupportedImperativeRenderLowering`

第一阶段如果为了降低改动面，仍沿用 `CanonicalizationFailed`，也必须：

- 在 message 中明确说明是 block-code / imperative render promotion 失败
- 避免再给出“Only canonicalizable for/foreach/if are supported”这类过时描述

## 12. Identity / HMR / Artifact Consequences

命令式 block 是正式语义，而不是脏 fallback，因此必须进入 artifact identity。

后续 identity 规则应满足：

1. 含命令式 block 的组件，其 logic/template identity 必须稳定反映 block 存在。
2. HMR boundary 至少不能错误标成纯 template-only。
3. 含命令式 block 的组件，默认至少应视为 `LogicSafe`，必要时升级到 `FullReloadRequired`。

## 13. Phase Plan

### Phase 1: Architecture Lock-In

当前状态：

- 已完成文档落地
- 已完成 promotion 规则收敛
- 已完成正式命令式 block 节点与 lowering 入口落地
- 已完成首段 body-level imperative H/render-function 承载

当前交付：

- 本文档
- `RazorVueImperativeBlockNode` / `RazorVueImperativeBlockKind`
- handwritten `BuildRenderTree` frontend body-level promotion
- Razor IR frontend body-level promotion
- `.mjs` / H artifact 的 body-level imperative render bridge
- README / support-gap 文档对齐

### Phase 2: Body-Level Imperative Fallback

当前状态：

- 已进入进行中阶段，并已完成第一段生产切片
- 当前不追求局部最优切分
- 当前已支持“复杂 block 存在时，整段 render body 走命令式通道”
- 当前 render 目标以 H / `.mjs` artifact 为主

优点：

- correctness 优先
- 最容易保证 handwritten / Razor IR 行为一致

缺点：

- 会损失一部分 template-lift 机会

当前已验证的 production slice 包括：

1. `return` 提前退出的 body-level imperative render
2. `while`
3. `switch`
4. `lock`
5. `try/catch/finally`
6. `using` / `using declaration`
7. 局部 mutation 后继续渲染
8. imperative body 内常量 `AddMarkupContent(...)` 静态 subtree 复用

当前仍保留的边界包括：

1. canonical template path 不接受 imperative body
2. `await using`
3. `lock` 的 CLR monitor / cross-thread 互斥语义
4. 更复杂资源生命周期与异常控制流仍需继续扩面

这是当前生产主线的真实落地点。

### Phase 3: Local Promotion

目标：

- 从整段 body fallback 收敛到局部 block promotion
- 复杂块外的兄弟节点仍保留声明式模板通道

### Phase 4: Render-Function SFC

目标：

- 含命令式 block 的组件也能稳定生成正式 SFC artifact
- 不再要求“一旦有命令式 block 就只能走 H artifact”

## 14. Initial Supported Scope Recommendation

第一批应优先纳入命令式通道的真实高价值 block：

1. `while`
2. `switch`
3. 局部变量赋值后继续参与渲染
4. 同一 block 中多段复杂业务语句后再渲染

第一批不建议优先做：

1. `await using`
2. async render body
3. 更细粒度局部 promotion 与优化
4. `lock` 的更强语义模拟（若未来确有真实需求）

原因不是这些永远不支持，而是：

- 第一批先建立稳定通道
- 再逐步扩张复杂控制流

## 15. Hard Rules

1. 不再继续把复杂 block 支持建立在持续膨胀的 `PendingTemplateControlNode`/statement 特判矩阵上。
2. handwritten frontend 与 Razor IR frontend 必须共享同一 promotion 语义，而不是分别发展。
3. 命令式 block 是正式中间语义，不是 undocumented fallback。
4. 不能稳定 lower 的命令式 block 必须显式失败，不能 silent degrade。
5. 新增支持面必须同时更新：
   - 架构文档
   - README 支持边界
   - support-gap 记录
   - focused tests

## 16. Immediate Next Step

紧接本设计之后的第一实现切片应是：

1. 在 render tree 层引入正式命令式 block 节点。
2. 两条前端先实现 body-level promotion。
3. 先把 `while` 和“局部赋值后继续渲染”的一条真实业务路径接入命令式通道。
4. 以 focused tests 证明：
   - 不再报旧式 canonicalization message
   - 两条前端 promotion 行为一致
   - H artifact 能稳定承载该路径
