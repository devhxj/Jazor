# Emit / Host Materialisation 状态（2026-04-06）

> Status: 当前状态快照
> Positioning: 仓库级状态快照，涵盖 emit、manifest、bundle 和面向宿主的物化工作
> Note: 当前面向 Vue 的宿主路径是 `Jolt`；本页描述的是为其提供支撑的 emit/materialisation 层

## 总结

`Emit` 不是当前仓库里最显眼的专题，但它已经是多个活跃工作流共同的承接层。

说得更直白点：

- compiler 负责生成 catalog / artifact / manifest-ready data
- emit 负责读取、物化、写出和 bundle 承接
- host-facing materialisation 已经是当前 `Jolt` / Deno 架构边界的一部分，不应该再被误读成只存在于测试侧

## 当前状态判断

### 1. Emit 是 active dependency lane

当前 emit 线承接的不是单一功能，而是一组跨工作流职责：

- catalog reading
- manifest persistence
- module / artifact writing
- bundler 和 host-facing output assembly
- Jolt 和 SourceMap handoff continuation

### 2. 当前 repo-level 入口已经补齐第一层，但仍需要持续维护

当前仓库级导航里，emit 已经可以通过以下入口直接进入：

- `docs/03-完成/emit/status.md`（本文件）
- `docs/01-目标/jolt/modules-bridge.md`
- `src/Jazor.Emit/README.md`
- `docs/01-目标/compiler/emit/Emit.Pipeline.Overview.md`
- `src/Jazor.EmitTest/README.md`

这比之前只靠测试 README 和相邻专题间接暴露要强得多，但仍需要随着 emit 职责演进持续维护。

### 3. 当前最明显的活跃执行交点是 Jolt 和 SourceMap

目前 emit 最直接的活跃执行关联是：

- Jolt catalog / manifest materialisation
- SourceMap module map 和 bundle chaining 承接

所以 emit 当前应该被描述成"被多个 lane 依赖的活跃承接层"。

## 下一步行动

### 1. 显式化 materialisation / sourcemap 承接职责

**目标**：让 emit 在整体架构中的职责更清晰

**具体行动**：
- 明确 catalog reading 和 manifest persistence 的边界
- 保持模块 README 和 repo-level bridge 同步
- 让 writer / bundler 的演进方向和上游需求对齐

**参考文档**：
- [Jazor.Emit README](../../../src/Jazor.Emit/README.md)
- [Emit.Materialization.Overview.md](../../01-目标/compiler/emit/Emit.Materialization.Overview.md)

### 2. 维持 emit test 和真实输出链路的一致性

**目标**：避免测试和真实执行继续分裂

**具体行动**：
- 确保 emit test 覆盖真实的 materialisation 场景
- 让测试链路和构建产物路径保持一致

**参考文档**：
- [Jazor.EmitTest README](../../../src/Jazor.EmitTest/README.md)

### 3. 支撑 RazorVue 和 SourceMap 的活跃需求

**目标**：确保 emit 能够稳定承接上游工作流

**具体行动**：
- 跟进 RazorVue catalog / manifest materialisation 需求
- 跟进 SourceMap module map 和 bundle chaining 需求

**参考文档**：
- [Emit.BundleAndSourceMap.Overview.md](../../01-目标/compiler/emit/Emit.BundleAndSourceMap.Overview.md)
- [RazorVue.DenoHostContract.md](../../01-目标/razorvue/design/RazorVue.DenoHostContract.md)

## 深度文档

- [Modules Bridge](../../01-目标/jolt/modules-bridge.md)
- [Jazor.Emit README](../../../src/Jazor.Emit/README.md)
- [Emit.Pipeline.Overview.md](../../01-目标/compiler/emit/Emit.Pipeline.Overview.md)
- [Emit.Materialization.Overview.md](../../01-目标/compiler/emit/Emit.Materialization.Overview.md)
- [Jazor.EmitTest README](../../../src/Jazor.EmitTest/README.md)

## 当前缺口

- Emit 职责仍然和 RazorVue / SourceMap 紧密耦合，后续需要继续保持 bridge 和相邻 lane 同步
- 如果 SourceMap 或 host bundling 明显扩张，可能需要进一步细化 emit 内部文档分层
