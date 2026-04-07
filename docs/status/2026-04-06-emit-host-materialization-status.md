# Emit / Host Materialisation 状态（2026-04-06）

> Status: current status snapshot
> Positioning: Repository-level status bridge for emit, manifest, bundle, and host-facing materialisation work.

## 总结

`Emit` 不是当前仓库里最显眼的专题，但它已经是多个活跃工作流共同的承接层。

说得更直白点：

- compiler 负责生成 catalog / artifact / manifest-ready data
- emit 负责读取、物化、写出和 bundle 承接
- host-facing materialisation 已经是当前架构边界的一部分，不应该再被误读成只存在于测试侧

## 当前依据

- [src/Jazor.Emit/README.md](../../src/Jazor.Emit/README.md)
- [src/Jazor.Emit/doc/Emit.Pipeline.Overview.md](../../src/Jazor.Emit/doc/Emit.Pipeline.Overview.md)
- [src/Jazor.EmitTest/README.md](../../src/Jazor.EmitTest/README.md)
- [RazorVue.DenoHostContract.md](../../src/Jazor.Compiler/doc/RazorVue.DenoHostContract.md)
- [2026-04-06-razorvue-sourcemap-bundle-chaining-implementation.md](../superpowers/plans/2026-04-06-razorvue-sourcemap-bundle-chaining-implementation.md)

## 当前状态判断

### 1. Emit 是 active dependency lane

当前 emit 线承接的不是单一功能，而是一组跨工作流职责：

- catalog reading
- manifest persistence
- module / artifact writing
- bundler 和 host-facing output assembly
- RazorVue 和 SourceMap handoff continuation

### 2. 当前 repo-level 入口已经补齐第一层，但仍需要持续维护

当前仓库级导航里，emit 已经可以通过以下入口直接进入：

- `docs/status/2026-04-06-emit-host-materialization-status.md`
- `docs/plans/emit-materialization-execution-bridge.md`
- `docs/architecture/modules/README.md`
- `src/Jazor.Emit/README.md`
- `src/Jazor.Emit/doc/README.md`
- `src/Jazor.EmitTest/README.md`

这比之前只靠测试 README 和相邻专题间接暴露要强得多，但仍需要随着 emit 职责演进持续维护。

### 3. 当前最明显的活跃执行交点是 RazorVue 和 SourceMap

目前 emit 最直接的活跃执行关联是：

- RazorVue catalog / manifest materialisation
- SourceMap module map 和 bundle chaining 承接

所以 emit 当前应该被描述成"被多个 lane 依赖的活跃承接层"。

## 当前推荐入口

如果你准备处理 emit 或 host-facing materialisation 问题，建议按这个顺序：

1. [工作流状态面板](./2026-04-06-project-workstream-dashboard.md)
2. 本文档
3. [项目执行导航](../plans/project-execution-index.md)
4. [Jazor.Emit README](../../src/Jazor.Emit/README.md)
5. [Jazor.Emit Docs](../../src/Jazor.Emit/doc/README.md)
6. [Emit.Materialization.Overview.md](../../src/Jazor.Emit/doc/Emit.Materialization.Overview.md)
7. [Emit.BundleAndSourceMap.Overview.md](../../src/Jazor.Emit/doc/Emit.BundleAndSourceMap.Overview.md)
8. [Jazor.EmitTest README](../../src/Jazor.EmitTest/README.md)
9. 再进入相关上游专题：
   - [RazorVue.Overview.md](../../src/Jazor.Compiler/doc/RazorVue.Overview.md)
   - [SourceMap.Overview.md](../../src/Jazor.Compiler/doc/SourceMap.Overview.md)

## 当前执行重点

当前 emit 线更适合按以下职责块理解：

1. catalog reading
2. manifest persistence
3. writer / materialisation
4. bundler / host-facing output assembly
5. RazorVue / SourceMap handoff continuation

## 当前缺口

- emit 职责仍然和 RazorVue / SourceMap 紧密耦合，后续需要继续保持 bridge 和相邻 lane 同步
- 如果 SourceMap 或 host bundling 明显扩张，可能需要进一步细化 emit 内部文档分层
