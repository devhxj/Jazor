# Jazor 工作流状态面板（2026-04-06）

> Status: current status snapshot
> Positioning: Repository-level status bridge across active workstreams.

## 总览

当前项目更适合按"工作流"来理解，不太适合简单按"项目名"一刀切：

- Compiler 主线：接近稳定主干
- RazorVue：执行推进中
- SourceMap / bundle chaining：从纯文档保留转进局部活跃实现
- Emit / host materialisation：持续承接，仓库级入口已经补齐第一层
- 文档治理：进入持续治理期

## 工作流状态

| 工作流 | 当前状态 | 最近依据 | 下一门槛 |
|---|---|---|---|
| Compiler 主线 | 接近稳定主干 | [2026-04-06-compiler-mainline-status.md](./2026-04-06-compiler-mainline-status.md) | 继续压缩输出闭环、import closure 和 host seam |
| RazorVue 主线 | active execution | [2026-04-06-razorvue-stage-assessment.md](./2026-04-06-razorvue-stage-assessment.md) | phase-one closure 和 authoring lane 收口 |
| RazorVue authoring lane | active planning | [2026-04-06-razorvue-v1-authoring-roadmap.md](../superpowers/plans/2026-04-06-razorvue-v1-authoring-roadmap.md) | 从 `PR1` 开始执行 |
| SourceMap / bundle chaining | active partial rollout | [2026-04-06-sourcemap-status.md](./2026-04-06-sourcemap-status.md) | 继续维持 broad programme 和 narrow lane 的边界 |
| Emit / host materialisation | active dependency lane | [2026-04-06-emit-host-materialization-status.md](./2026-04-06-emit-host-materialization-status.md) | 继续显式化 materialisation / sourcemap 承接职责 |
| 文档治理 | active governance | [2026-04-04-project-stage-assessment.md](./2026-04-04-project-stage-assessment.md) | 保持状态、计划和桥接索引持续同步 |

## 关键判断

### 1. 当前最成熟的是 compiler 主线，不是整个仓库所有方向一盘都成熟了

这就决定了 repo-level 文档必须分清：

- 稳定参考
- 当前执行
- 未来保留

### 2. RazorVue 已经不是"概念探索"阶段了，而是持续实现工作流

所以它需要三层东西同时在场：

- 状态评估
- 仓库级执行入口
- 子系统深度文档

### 3. SourceMap 当前不是一句"全部 deferred"就说得清的

更准确的说法是：

- 通用 sourcemap 大计划仍然偏保守
- 但 RazorVue 相关 bundle chaining 已经进了当前执行层

这一点如果不写明，状态文档和执行计划就容易互相打架。

### 4. Emit 是当前执行体系里的关键依赖层，当前入口已经具备第一层闭环

它不是一个单独的大专题，但确实是多个工作流共同的承接层。

## 当前推荐入口

如果你是维护者，准备继续推进项目，建议按这条路走：

1. [2026-04-04-project-stage-assessment.md](./2026-04-04-project-stage-assessment.md)
2. 本文档
3. [项目执行导航](../plans/project-execution-index.md)
4. [Project Program Roadmap](../plans/project-program-roadmap.md)
5. [docs/architecture/README.md](../architecture/README.md)

## 当前缺口

- compiler / emit 的 repo-level 入口已经补齐第一层，但还需要持续维护桥接和局部 README 的同步
- `docs/plans/README.md` 还要继续保持 repo-level active-plan index 的准确性
- module-level operational docs 的第一层桥接已经补齐，后头重点转为保持模块 README 和 repo-level bridge 同步
