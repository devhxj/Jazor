# 项目执行导航

> Status: active plan
> Positioning: Repository-level execution bridge for the current Jazor workstreams.

## 目标

本文档负责回答三个问题：

1. 当前项目有哪些活跃工作流
2. 应该先看哪份状态文档，再看哪份计划文档
3. 仓库级执行导航如何桥接到子系统局部文档集

如需看跨工作流依赖顺序和 gate，而不是单纯入口导航，请继续看：

- [project-program-roadmap.md](./project-program-roadmap.md)

## 当前活跃工作流

### 1. Compiler 主线稳定化

- 当前状态：主干接近稳定，更多是主线闭环与边界收敛
- 状态文档：
  - [2026-04-06-compiler-mainline-status.md](../status/2026-04-06-compiler-mainline-status.md)
- 执行桥接：
  - [compiler-mainline-execution-bridge.md](./compiler-mainline-execution-bridge.md)
- 深度文档：
  - [Compiler Architecture Bridge](../architecture/compiler/README.md)
  - [Jazor.Compiler 文档索引](../../src/Jazor.Compiler/doc/README.md)

### 2. RazorVue 主线收口

- 当前状态：主链路已进入主干，正在做 phase-one closure 与 authoring lane 收口
- 状态文档：
  - [2026-04-06-razorvue-stage-assessment.md](../status/2026-04-06-razorvue-stage-assessment.md)
- 执行桥接：
  - [razorvue-execution-bridge.md](./razorvue-execution-bridge.md)
- 活跃计划：
  - [2026-04-05-razorvue-layering-implementation.md](../superpowers/plans/2026-04-05-razorvue-layering-implementation.md)
  - [2026-04-05-razorvue-lifecycle-safe-subset-implementation.md](../superpowers/plans/2026-04-05-razorvue-lifecycle-safe-subset-implementation.md)
  - [2026-04-06-razorvue-v1-authoring-roadmap.md](../superpowers/plans/2026-04-06-razorvue-v1-authoring-roadmap.md)
  - [2026-04-06-razorvue-v1-authoring-pr-breakdown.md](../superpowers/plans/2026-04-06-razorvue-v1-authoring-pr-breakdown.md)
- 深度文档：
  - [RazorVue.Overview.md](../../src/Jazor.Compiler/doc/RazorVue.Overview.md)

### 3. SourceMap / bundle chaining lane

- 当前状态：通用 sourcemap 大计划仍偏保守，但 RazorVue 相关 bundle chaining 已进入活跃执行
- 状态文档：
  - [2026-04-06-sourcemap-status.md](../status/2026-04-06-sourcemap-status.md)
- 执行桥接：
  - [sourcemap-execution-bridge.md](./sourcemap-execution-bridge.md)
- 活跃计划：
  - [2026-04-06-razorvue-sourcemap-bundle-chaining-implementation.md](../superpowers/plans/2026-04-06-razorvue-sourcemap-bundle-chaining-implementation.md)
- 深度文档：
  - [SourceMap.Overview.md](../../src/Jazor.Compiler/doc/SourceMap.Overview.md)

### 4. Emit / host-facing materialization

- 当前状态：承担 catalog、manifest、materialization 以及 sourcemap/output 承接职责，仓库级入口已补齐第一层桥接
- 状态文档：
  - [2026-04-06-emit-host-materialization-status.md](../status/2026-04-06-emit-host-materialization-status.md)
- 执行桥接：
  - [emit-materialization-execution-bridge.md](./emit-materialization-execution-bridge.md)
- 深度文档：
  - [Modules Bridge](../architecture/modules/README.md)
  - [Jazor.Emit README](../../src/Jazor.Emit/README.md)
  - [Jazor.Emit Docs](../../src/Jazor.Emit/doc/README.md)
  - [Jazor.EmitTest README](../../src/Jazor.EmitTest/README.md)
  - [Jazor.Compiler 文档索引](../../src/Jazor.Compiler/doc/README.md)

## 建议阅读顺序

如果你是在恢复项目工作，建议按以下顺序：

1. [docs/README.md](../README.md)
2. [2026-04-06-project-workstream-dashboard.md](../status/2026-04-06-project-workstream-dashboard.md)
3. 本文档
4. [project-program-roadmap.md](./project-program-roadmap.md)
5. [docs/architecture/README.md](../architecture/README.md)
6. 再按具体工作流进入子系统文档

## 仓库级与子系统级的分工

- `docs/status/` 负责当前状态快照
- `docs/plans/` 负责仓库级执行导航
- `docs/superpowers/plans/` 保留更细的执行计划与工作流拆分
- `src/Jazor.Compiler/doc/` 保留 compiler / RazorVue / SourceMap 深度设计与专题总览

规则：

- 仓库级文档负责“告诉你先去哪里”
- 子系统文档负责“把该领域讲清楚”
- 不把成熟局部文档集强行搬回 `docs/`

## 当前主要风险

- repo-level `plans` 入口需要持续跟随更细的执行计划演进
- 状态文档和执行计划需要继续保持同步更新
- SourceMap 相关文档存在“总体 deferred”与“局部 active lane”并存的理解成本

## 后续维护要求

新增活跃工作流时，至少同步更新：

1. `docs/status/README.md`
2. `docs/plans/README.md`
3. 本文档

这样仓库级阅读路径才不会再次失真。
