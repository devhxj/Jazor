# SourceMap 执行桥接

> Status: active plan
> Positioning: Repository-level execution bridge for the SourceMap program and its current narrower active lane.

## 目标

把 SourceMap 从“局部专题已存在，但 repo-level 进入方式不清楚”桥接成一个可执行入口。

## 当前双层结构

### 1. Broad program

Broad program 负责：

- module-level map as the stable first layer
- long-lived SourceMap rules and implementation order
- broad rollout constraints

入口：

- [SourceMap 状态（2026-04-06）](../status/2026-04-06-sourcemap-status.md)
- [SourceMap.Overview.md](../../src/Jazor.Compiler/doc/SourceMap.Overview.md)
- [SourceMap.ImplementationChecklist.md](../../src/Jazor.Compiler/doc/SourceMap.ImplementationChecklist.md)

### 2. Narrower active lane

Narrower active lane 负责：

- RazorVue emitted module sourcemap
- writer / manifest extension
- bundle chaining continuation

入口：

- [2026-04-06-razorvue-sourcemap-bundle-chaining-implementation.md](../superpowers/plans/2026-04-06-razorvue-sourcemap-bundle-chaining-implementation.md)

## 当前依赖关系

SourceMap 当前依赖以下上游边界：

1. compiler artifact/source-origin shape
2. emit writer / manifest / bundler evolution
3. RazorVue current host-facing carrier path

因此它不应被看成单独孤立的 lane。

## 建议阅读顺序

1. [SourceMap 状态（2026-04-06）](../status/2026-04-06-sourcemap-status.md)
2. 本文档
3. [SourceMap.Overview.md](../../src/Jazor.Compiler/doc/SourceMap.Overview.md)
4. 再根据具体任务进入 broad program 或 narrower active lane

## 当前非目标

- 不把 broader SourceMap program 误写成 fully active
- 不让 narrower active lane 覆盖 broad program 规则
- 不在 repo-level bridge 中重复局部专题细节

## 下一步维护要求

如果 SourceMap 的 broad program 或 narrower active lane 状态发生变化，至少同步更新：

1. [SourceMap 状态（2026-04-06）](../status/2026-04-06-sourcemap-status.md)
2. [项目执行导航](./project-execution-index.md)
3. [Jazor Project Program Roadmap](./project-program-roadmap.md)
