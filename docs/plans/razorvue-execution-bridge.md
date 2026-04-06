# RazorVue 执行桥接

> Status: active plan
> Positioning: Repository-level execution bridge for the current RazorVue workstreams.

## 目标

把 RazorVue 从“repo-level 有状态页、局部有大文档集、执行计划分散在多个文件”桥接成一个更清晰的进入点。

本文档不解释 RazorVue 内部设计，只回答：

1. 当前 RazorVue 有哪几条执行分支
2. 应该先从哪份状态文档进入
3. 什么时候进入 phase-one closure，什么时候进入 authoring lane

## 当前 RazorVue 分支

### 1. Phase-one closure

当前关注：

- layering 收口
- lifecycle safe subset 收口
- 保持主链路 generic lowering 与 host-facing carrier continuity

主要入口：

- [RazorVue 阶段评估（2026-04-06）](../status/2026-04-06-razorvue-stage-assessment.md)
- [2026-04-05-razorvue-layering-implementation.md](../superpowers/plans/2026-04-05-razorvue-layering-implementation.md)
- [2026-04-05-razorvue-lifecycle-safe-subset-implementation.md](../superpowers/plans/2026-04-05-razorvue-lifecycle-safe-subset-implementation.md)

### 2. Authoring lane

当前关注：

- C#-first authoring model
- library authoring contract
- Vuetify first package
- staged PR execution

主要入口：

- [2026-04-06-razorvue-v1-authoring-roadmap.md](../superpowers/plans/2026-04-06-razorvue-v1-authoring-roadmap.md)
- [2026-04-06-razorvue-v1-authoring-pr-breakdown.md](../superpowers/plans/2026-04-06-razorvue-v1-authoring-pr-breakdown.md)

### 3. 相邻但不应混写的 lane

与 RazorVue 强相关，但不应直接写成 RazorVue 子专题：

- [SourceMap 执行桥接](./sourcemap-execution-bridge.md)
- [Emit / Materialization 执行桥接](./emit-materialization-execution-bridge.md)

这些是相邻依赖 lane，不是 RazorVue 内部执行分支。

## 主要入口

### Repo-level 状态入口

- [RazorVue 阶段评估（2026-04-06）](../status/2026-04-06-razorvue-stage-assessment.md)

### Repo-level 执行入口

- [项目执行导航](./project-execution-index.md)
- [Jazor Project Program Roadmap](./project-program-roadmap.md)
- 本文档

### 局部深文档入口

- [RazorVue.Overview.md](../../src/Jazor.Compiler/doc/RazorVue.Overview.md)

## 建议阅读顺序

如果你要继续 RazorVue 工作，建议按这个顺序：

1. [RazorVue 阶段评估（2026-04-06）](../status/2026-04-06-razorvue-stage-assessment.md)
2. [RazorVue.Overview.md](../../src/Jazor.Compiler/doc/RazorVue.Overview.md)
3. 判断当前任务属于：
   - phase-one closure
   - authoring lane
4. 再进入对应执行计划

## 当前非目标

- 不重复 RazorVue 架构、descriptor、HMR、DenoHost 契约等深度内容
- 不替代 `RazorVue.Overview.md`
- 不替代 authoring PR breakdown
- 不把 broad SourceMap program 吞进 RazorVue lane

## 下一步维护要求

如果 RazorVue 的执行分支发生变化，至少同步更新：

1. [RazorVue 阶段评估（2026-04-06）](../status/2026-04-06-razorvue-stage-assessment.md)
2. [项目执行导航](./project-execution-index.md)
3. 本文档
