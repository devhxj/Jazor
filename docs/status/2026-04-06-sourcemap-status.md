# SourceMap 状态（2026-04-06）

> Status: current status snapshot
> Positioning: Repository-level status bridge for the SourceMap program.

## 总结

SourceMap 当前不能再用一句“deferred”概括。

更准确地说：

- broad SourceMap program 仍然偏保守
- 但 RazorVue 相关 bundle chaining 已进入 narrower active lane
- repo-level 文档需要同时表达这两层现实，不能二选一

## 当前依据

- [SourceMap.Overview.md](../../src/Jazor.Compiler/doc/SourceMap.Overview.md)
- [SourceMap.ImplementationChecklist.md](../../src/Jazor.Compiler/doc/SourceMap.ImplementationChecklist.md)
- [2026-04-06-razorvue-sourcemap-bundle-chaining-implementation.md](../superpowers/plans/2026-04-06-razorvue-sourcemap-bundle-chaining-implementation.md)

## 当前状态判断

### 1. broad program remains conservative

当前仍不应把 SourceMap 描述成“全线开工”。

Broad program 依然强调：

- compiler main path stability first
- module-level map first
- broad rollout should not outrun upstream stability

### 2. narrower active lane is already real

当前已经存在一个 narrower active lane：

- RazorVue emitted module sourcemap
- writer / manifest evolution
- bundle chaining continuation

因此更准确的状态是：

- broad program: conservative
- narrow lane: active

### 3. SourceMap is now a coordination lane, not only a future note

SourceMap 当前已经和以下工作流直接耦合：

- compiler artifact/source-origin shape
- emit writer / manifest / bundler evolution
- RazorVue host-facing materialization

这意味着 repo-level 文档必须显式桥接它，而不能只把它留在局部专题入口里。

## 当前推荐入口

如果你准备继续 SourceMap 相关工作，建议按这个顺序：

1. [工作流状态面板](./2026-04-06-project-workstream-dashboard.md)
2. 本文档
3. [sourcemap-execution-bridge.md](../plans/sourcemap-execution-bridge.md)
4. [SourceMap.Overview.md](../../src/Jazor.Compiler/doc/SourceMap.Overview.md)

## 当前执行重点

当前 SourceMap 更适合按两层理解：

1. broad program guidance
2. narrower active rollout for current consumers

## 当前缺口

- repo-level SourceMap 状态入口刚建立，还没有形成长期稳定阅读习惯
- broad program 与 narrow lane 的边界需要持续维护
- SourceMap 仍容易被误写成“全 deferred”或“全 active”
