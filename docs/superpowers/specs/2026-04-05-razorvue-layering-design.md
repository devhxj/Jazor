# RazorVue Layering Refactor Design

> Status: active reference
> Positioning: Execution-facing design reference for the RazorVue layering closure lane.

**Date:** 2026-04-05

## Goal

把 `Jazor.RazorVue` 收敛为真正的 RazorVue 核心层，把 `Jazor.RazorVue.Analysis` 收敛为薄 Roslyn 入口层，消除当前 Analysis 既当 generator host 又承载 RazorVue 核心语义的职责膨胀问题。

## Current Problem

当前仓库存在明显的层次失衡：

- `src/Jazor.RazorVue/` 目前几乎只包含 `VueComponent.cs`
- `src/Jazor.RazorVue.Analysis/` 已经承载了：
  - generator/analyzer 入口
  - compilation context
  - semantic snapshot
  - descriptor model
  - render-tree model/extractor
  - lowering
  - artifact/catalog shaping
  - resolution/issue model
  - pipeline orchestration

这意味着 `Jazor.RazorVue.Analysis` 实际上已经演变成 RazorVue 核心实现，而项目名却仍然表达“分析入口”。这会导致后续代码继续错误地增长在 Analysis 层里。

## Correct Ownership

### `Jazor.Razor`

职责保持不变：

- `JazorComponent`
- Razor 侧最薄基类语义

### `Jazor.RazorVue`

这是 RazorVue 核心层，应承载：

- `VueComponent`
- RazorVue 的 descriptor 模型
- render-tree 模型与提取逻辑
- lowering
- artifact/catalog 领域模型
- pipeline 核心
- resolution / issue model
- RazorVue 的核心编译语义

原则：**只要脱离 Roslyn generator 宿主以后依然成立，它就属于 `Jazor.RazorVue`。**

### `Jazor.RazorVue.Analysis`

这是薄入口层，只承载：

- `RazorVueGenerator`
- Roslyn analyzer/source-generator 入口接线
- 调用 `Jazor.RazorVue` 核心层
- 诊断投影和 generator 宿主 glue code

原则：**Analysis 不拥有 RazorVue 语义，只负责把 Roslyn 输入接到 RazorVue 核心层。**

### `Jazor.Compiler`

继续只承载 route-neutral compiler infrastructure：

- 通用编译基础设施
- 通用契约
- 不再继续吸收 RazorVue 专有 lowering / descriptor / render-tree 逻辑

## Required Code Movement

下面这些类型/目录应从 `Jazor.RazorVue.Analysis` 迁入 `Jazor.RazorVue`：

- `RazorVue/Artifacts/*`
- `RazorVue/Descriptor/*`
- `RazorVue/Discovery/*`
- `RazorVue/Extensibility/*`
- `RazorVue/Lowering/*`
- `RazorVue/RenderTree/*`
- `RazorVueCompilationContext.cs`
- `RazorVueCompilationSymbols.cs`
- `RazorVueComponentCandidate.cs`
- `RazorVueEntryKind.cs`
- `RazorVuePipeline.cs`

保留在 `Jazor.RazorVue.Analysis` 的核心代码应只剩：

- `RazorVueGenerator.cs`
- 必要的 generator 专用诊断/投影辅助（如果后续需要拆出小 helper）
- `AssemblyInfo.cs` / `GlobalUsings.cs` 等宿主文件

## Framework and Reference Direction

当前项目状态：

- `Jazor.RazorVue` 目标框架是 `net10.0`
- `Jazor.RazorVue.Analysis` 目标框架是 `netstandard2.0`
- `Jazor.Compiler.Generator` 等仓库内 generator 类项目已存在 `net10.0` 目标框架实践

为了让 `Analysis` 能薄化并直接引用 `Jazor.RazorVue`，本次重构采用：

- 将 `Jazor.RazorVue.Analysis` 调整到 `net10.0`
- 让 `Jazor.RazorVue.Analysis` 引用 `Jazor.RazorVue`
- 保持 `Jazor.RazorVue` 引用 `Jazor.Razor`
- 仅在确有需要时让 `Jazor.RazorVue` 引用 `Jazor.Compiler`

这样做的原因：

- 它符合“`Jazor.RazorVue` 是核心层”的用户约束
- 它避免引入新的 `Core` 项目稀释核心职责
- 它可以在当前仓库已有的 `net10.0` generator 生态内工作

## Hard Rules

1. `Jazor.RazorVue` 必须成为 RazorVue 核心实现的唯一归属层。
2. `Jazor.RazorVue.Analysis` 不能继续新增 descriptor/lowering/render-tree/pipeline 逻辑。
3. `Jazor.Compiler` 不能接收新的 RazorVue 专有实现代码。
4. `Jazor.RazorVue` 不得反向依赖 `Jazor.RazorVue.Analysis`。
5. 所有新注释都要写在 seam 附近，明确说明为什么 Analysis 只保留薄入口。

## Migration Strategy

### Stage 1: 物理搬迁核心代码

- 调整 `.csproj` 引用与 target framework
- 把核心类型从 `Analysis` 迁到 `RazorVue`
- 同步 namespaces / usings

### Stage 2: 瘦身 Analysis 入口

- 让 `RazorVueGenerator` 只做候选采集、调用 pipeline、投影诊断
- 删除 Analysis 内已经不该存在的核心实现副本

### Stage 3: 回归与文档同步

- 更新测试项目引用与 using
- 更新职责文档、设计文档、实施清单
- 运行定向编译与测试，确认分层重构没有破坏 lifecycle 主链路

## Success Criteria

本次重构完成的判定标准：

1. `Jazor.RazorVue` 实际拥有 RazorVue 核心类型，而不再只是 `VueComponent` 空壳。
2. `Jazor.RazorVue.Analysis` 代码量显著收缩为 generator 入口层。
3. `RazorVueGenerator` 仍能成功生成 catalog，并保留 `JAZORVGA001`~`JAZORVGA005` 诊断行为。
4. 现有 `RazorVuePipelineTests` 与 `ESGeneratorTests` 通过。
5. 文档中的项目职责描述与实际代码归属一致。
