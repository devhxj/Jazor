# Jazor 工作流总览

> 最后更新：2026-04-07
> 作用：这是恢复工作的唯一入口，告诉你每个工作流现在在哪个阶段、下一步该做什么。

## 快速导航

| 工作流 | 当前阶段 | 下一步行动 | 状态文档 |
|--------|---------|-----------|---------|
| Compiler 主线 | 接近稳定 | 压实 output closure、import closure、host seam | [详情](./status/compiler-mainline-status.md) |
| Emit / Materialisation | 持续承接 | 显式化 materialisation / sourcemap 承接职责 | [详情](./status/emit-host-materialization-status.md) |
| RazorVue | 活跃执行中 | Phase-one closure 和 authoring lane 收口 | [详情](./status/razorvue-stage-assessment.md) |
| SourceMap | 局部活跃 | RazorVue bundle chaining 实现 | [详情](./status/sourcemap-status.md) |

## 依赖顺序与并行策略

**依赖顺序**（上游必须先稳定）：

1. Compiler mainline stabilisation
2. Emit / host materialisation consolidation
3. RazorVue phase-one closure
4. RazorVue authoring lane execution
5. SourceMap partial rollout for active consumers
6. Broader SourceMap programme
7. Ongoing documentation governance

**允许的并行**：
- Documentation governance 可以持续运行
- Narrow SourceMap 工作可以和 RazorVue / emit 集成并行推进
- Emit 可以作为活跃依赖层持续演进（compiler 作为上游基础）

**不允许的并行**：
- Broad SourceMap 扩张不能超过 compiler / emit 稳定性
- Authoring 广度不能超过 RazorVue phase-one closure

## 详细工作流说明

### Compiler 主线

**当前状态**：主干接近稳定，当前重点在主线闭环和边界收敛

`Jazor.Compiler` 仍然是当前仓库里最成熟的主干资产。编译器主链路已经接近稳定主干，当前工作重点不是重做架构，而是维持主线闭环、控制边界扩张、给外围能力提供稳定依赖面。

**下一步行动**：

1. **Output closure**
   - 压实 `ESGenerator -> catalog -> output` 闭环
   - 避免测试链路和真实输出链路继续分裂
   - 参考：[TransformationRoadmap.md](../src/Jazor.Compiler/doc/TransformationRoadmap.md)

2. **Import closure**
   - 让 import 从收集阶段进到稳定落盘阶段
   - 保持 import 命名、去重和顺序稳定

3. **Host semantics seam**
   - 稳定 `Inline` / `Compile` 分工
   - 莫让宿主语义扩张又跑回来破坏 compiler 主线边界
   - 参考：[InlineAstTemplateSpec.md](../src/Jazor.Compiler/doc/InlineAstTemplateSpec.md)

**深度文档**：
- [Compiler Architecture Bridge](./architecture/compiler/README.md)
- [Jazor.Compiler 文档索引](../src/Jazor.Compiler/doc/README.md)

---

### Emit / Host Materialisation

**当前状态**：承担 catalog、manifest、materialisation 以及 sourcemap/output 承接职责，仓库级入口已经补齐第一层桥接

Emit 不是单独的大专题，但确实是多个工作流共同的承接层。它负责把上游 compiler / RazorVue 的产出落盘成 host-facing artefact。

**下一步行动**：

1. **显式化 materialisation / sourcemap 承接职责**
   - 让 manifest / catalog / writer 的职责更清晰
   - 保持模块 README 和 repo-level bridge 同步

2. **维持 emit test 和真实输出链路的一致性**
   - 避免测试和真实执行继续分裂

**深度文档**：
- [Modules Bridge](./architecture/modules/README.md)
- [Jazor.Emit README](../src/Jazor.Emit/README.md)
- [Jazor.Emit Docs](../src/Jazor.Emit/doc/README.md)

---

### RazorVue

**当前状态**：主链路已进主干，正在做 phase-one closure 和 authoring lane 收口

RazorVue 当前不是"在编译器里顺手多加一点 Vue 支持"，而是一条独立的 Vue-first 编译路径。主链路已经从早期的职责混杂，收敛为更清晰的结构。

**下一步行动**：

1. **Phase-one closure**
   - 完成 layering 实现
   - 完成 lifecycle safe subset 实现
   - 参考：[2026-04-05-razorvue-layering-implementation.md](./superpowers/plans/2026-04-05-razorvue-layering-implementation.md)

2. **Authoring lane 收口**
   - 从 PR1 开始执行 authoring roadmap
   - 参考：[2026-04-06-razorvue-v1-authoring-roadmap.md](./superpowers/plans/2026-04-06-razorvue-v1-authoring-roadmap.md)

**深度文档**：
- [RazorVue.Overview.md](../src/Jazor.Compiler/doc/RazorVue.Overview.md)
- [RazorVue.Design.md](../src/Jazor.Compiler/doc/RazorVue.Design.md)

---

### SourceMap

**当前状态**：通用 sourcemap 大计划偏保守，但 RazorVue 相关 bundle chaining 已进入活跃执行

SourceMap 当前不能再用一句"deferred"概括了。更准确地说：broad SourceMap programme 仍然偏保守，但 RazorVue 相关 bundle chaining 已进入 narrower active lane。

**下一步行动**：

1. **Narrow active lane**
   - 完成 RazorVue bundle chaining 实现
   - 让 writer / manifest / bundler 演进就位
   - 参考：[2026-04-06-razorvue-sourcemap-bundle-chaining-implementation.md](./superpowers/plans/2026-04-06-razorvue-sourcemap-bundle-chaining-implementation.md)

2. **维持 broad programme 和 narrow lane 的边界**
   - 莫让 narrow lane 的活跃掩盖了 broad programme 的保守基调

**深度文档**：
- [SourceMap.Overview.md](../src/Jazor.Compiler/doc/SourceMap.Overview.md)
- [SourceMap.ImplementationChecklist.md](../src/Jazor.Compiler/doc/SourceMap.ImplementationChecklist.md)

---

## 执行门槛（Gates）

### Gate A. Compiler 稳定性优先于下游扩张

下游工作流不能强制要求重新设计 compiler 主路径。必须保持：
- Compiler 局部文档继续作为权威来源
- 核心转换边界对下游假设足够稳定

### Gate B. Emit / materialisation 桥接优先于下游闭环声明

如果 host-facing 交接仍然不清楚，就不能声称下游工作流已经闭环。这个门槛对 RazorVue artifact / manifest 流程和 SourceMap writer / bundle chaining 流程都很重要。

### Gate C. RazorVue 最小路径优先于 authoring 广度

Authoring 扩张应该建立在已闭环的最小 RazorVue 路径上，而不是绕过它。

### Gate D. RazorVue authoring 不能分叉核心语义

在跨过 mid-authoring review gate 之前，必须确认：
- stub-as-truth-source 仍然成立
- lowering 保持通用
- 特定包的分支没有渗透进核心

### Gate E. SourceMap 局部推广只能在稳定上游载体上

Narrow SourceMap slice 可以更早推进，前提是：
- artifact / source-origin 形态已经可用
- emit 侧演进已经显式化
- slice 保持比 broad SourceMap programme 更窄

### Gate F. Broad SourceMap programme 保持保守

不能因为某个 narrow lane 活跃就把 broad SourceMap programme 当成"全线活跃"。

### Gate G. 阶段变化时文档更新是强制性的

当任何工作流改变阶段时，至少更新：
1. 相关的 repo-level 状态快照
2. 本文档（如果依赖顺序或 gates 改变）

---

## 停止条件

当以下任一情况出现时，暂停扩张：

- 下游工作开始强制要求上游重新设计
- Repo-level 状态和计划文档偏离实际执行
- 局部活跃 lane 和 broad-programme 文档矛盾
- Repo-level bridge 开始复制子系统局部权威

---

## 执行级详细计划

当前执行级详细计划保存在 `docs/superpowers/plans/`：

- [2026-04-05-razorvue-layering-implementation.md](./superpowers/plans/2026-04-05-razorvue-layering-implementation.md)
- [2026-04-05-razorvue-lifecycle-safe-subset-implementation.md](./superpowers/plans/2026-04-05-razorvue-lifecycle-safe-subset-implementation.md)
- [2026-04-06-razorvue-helper-composition-implementation.md](./superpowers/plans/2026-04-06-razorvue-helper-composition-implementation.md)
- [2026-04-06-razorvue-setup-side-logic-implementation.md](./superpowers/plans/2026-04-06-razorvue-setup-side-logic-implementation.md)
- [2026-04-06-razorvue-sourcemap-bundle-chaining-implementation.md](./superpowers/plans/2026-04-06-razorvue-sourcemap-bundle-chaining-implementation.md)
- [2026-04-06-razorvue-v1-authoring-roadmap.md](./superpowers/plans/2026-04-06-razorvue-v1-authoring-roadmap.md)
- [2026-04-06-razorvue-v1-authoring-pr-breakdown.md](./superpowers/plans/2026-04-06-razorvue-v1-authoring-pr-breakdown.md)
- [2026-04-06-readme-refresh.md](./superpowers/plans/2026-04-06-readme-refresh.md)

---

## 维护规则

本文档应该保持简短。不要把它变成：

- 第二个 docs hub
- 子系统 checklist
- 更详细的替代局部设计文档的版本

如果某个变更只影响一个工作流的内部设计，更新那个工作流的文档而不是扩展本文档。
