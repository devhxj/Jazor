# Jazor 工作流状态面板（2026-04-06）

> Status: current status snapshot
> Positioning: Repository-level status bridge across active workstreams.

## 总览

当前项目更适合按“工作流”而不是按“项目名”理解：

- Compiler 主线：接近稳定主干
- RazorVue：执行推进中
- SourceMap / bundle chaining：从纯文档保留转入局部活跃实现
- Emit / host materialization：持续承接，仓库级入口已补齐第一层
- 文档治理：进入持续治理期

## 工作流状态

| 工作流 | 当前状态 | 最近依据 | 下一门槛 |
|---|---|---|---|
| Compiler 主线 | 接近稳定主干 | [2026-04-06-compiler-mainline-status.md](./2026-04-06-compiler-mainline-status.md) | 继续压缩输出闭环、import closure 与 host seam |
| RazorVue 主线 | active execution | [2026-04-06-razorvue-stage-assessment.md](./2026-04-06-razorvue-stage-assessment.md) | phase-one closure 与 authoring lane 收口 |
| RazorVue authoring lane | active planning | [2026-04-06-razorvue-v1-authoring-roadmap.md](../superpowers/plans/2026-04-06-razorvue-v1-authoring-roadmap.md) | 从 `PR1` 开始执行 |
| SourceMap / bundle chaining | active partial rollout | [2026-04-06-sourcemap-status.md](./2026-04-06-sourcemap-status.md) | 继续维持 broad program 与 narrow lane 的边界 |
| Emit / host materialization | active dependency lane | [2026-04-06-emit-host-materialization-status.md](./2026-04-06-emit-host-materialization-status.md) | 继续显式化 materialization / sourcemap 承接职责 |
| 文档治理 | active governance | [2026-04-04-project-stage-assessment.md](./2026-04-04-project-stage-assessment.md) | 保持状态、计划、桥接索引持续同步 |

## 关键判断

### 1. 当前最成熟的是 compiler 主线，不是整个仓库所有方向

这决定了 repo-level 文档必须区分：

- 稳定参考
- 当前执行
- 未来保留

### 2. RazorVue 已经不再是“概念探索”，而是持续实现工作流

因此它需要：

- 状态评估
- 仓库级执行入口
- 子系统深度文档

三层同时存在。

### 3. SourceMap 当前不是简单的“全部 deferred”

更准确的说法是：

- 通用 sourcemap 大计划仍偏保守
- 但 RazorVue 相关 bundle chaining 已进入当前执行层

如果不显式写出这一点，状态文档会继续和执行计划冲突。

### 4. Emit 是当前执行体系中的关键依赖层，当前入口已经具备第一层闭环

它不是单独的大专题，却是当前多个工作流的共同承接层。

## 当前推荐入口

如果你是维护者并准备继续推进项目：

1. [2026-04-04-project-stage-assessment.md](./2026-04-04-project-stage-assessment.md)
2. 本文档
3. [项目执行导航](../plans/project-execution-index.md)
4. [Project Program Roadmap](../plans/project-program-roadmap.md)
5. [docs/architecture/README.md](../architecture/README.md)

## 当前缺口

- compiler / emit 的 repo-level 入口已补齐第一层，但仍需要持续维护桥接与局部 README 的同步
- `docs/plans/README.md` 需要继续保持 repo-level active-plan index 的准确性
- module-level operational docs 第一层桥接已补齐，后续重点转为保持模块 README 与 repo-level bridge 同步
