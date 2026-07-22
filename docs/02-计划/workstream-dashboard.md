# Jazor 工作流总览

> Status: 活跃计划
> Updated: 2026-07-06
> Positioning: 仓库级恢复入口，用于查看各工作流当前阶段、依赖顺序与下一步行动。
> Note: 更窄的 active plan 可以覆盖这里的具体执行切片，但不应反向改写这里对工作流边界和先后依赖的判断。
> Mainline pivot: 前端 authoring/runtime 主线切换为 Jazor Component Runtime；RazorVue、Jolt 和 CSX 不再定义主要组件执行模型。

## 快速导航

| 工作流 | 当前阶段 | 下一步行动 | 状态文档 |
|--------|---------|-----------|---------|
| Compiler 主线 | 接近稳定 | 巩固 output closure、import closure、host seam | [详情](../03-完成/compiler/status.md) |
| Jazor Component Runtime | 当前前端 authoring/runtime 主线 | 锁定 ASP.NET Core Components 基线，完成 `[ECMAScriptModule]` 入口分类、Phase 0 兼容矩阵与 Phase 1 Counter 闭环 | [计划](./jazor-component-runtime-plan-2026-07-06.md) |
| ECMAScript.Vue3 / RazorVue 旁路线 | 旁路参考，不承载主线组件执行模型 | 只保留 Vue artifact、slot、SFC 经验供 Runtime 评估复用；不继续扩大为主线 | [详情](../03-完成/ecmascript.vue3/status.md) |
| ECMAScript.Pinia 外部库线 | 初始落地完成，进入增量补齐 | 补 setup-store helpers 设计、继续沉淀外部库模板与独立测试治理 | [详情](../03-完成/ecmascript.pinia/status.md) |
| Emit / Materialisation | Runtime 主线承接层 | 显式化 component `.mjs`、runtime manifest、sourcemap 与 bundle 物化职责 | [详情](../03-完成/emit/status.md) |
| Jolt | 开发期宿主旁路 | Runtime 合同稳定前不承载新的组件执行模型；后续只评估 LSP/DevServer/HMR/build/debug 集成 | [详情](../03-完成/jolt/status.md) |
| Jazor CSX Frontend | 暂停为旁路评估线 | 不与 Razor Component Runtime 并行竞争主线；除非另有决策，不推进新的 authoring frontend | [详情](./csx/CSX.Frontend.ImplementationPlan.md) |
| SourceMap | 局部活跃（narrow lane） | 继续补齐调试消费链路与精度提升 | [详情](../03-完成/sourcemap/status.md) |

## 依赖顺序与并行策略

**依赖顺序**（上游必须先稳定）：

1. Compiler mainline stabilisation
2. Jazor Component Runtime Phase 0 baseline, `[ECMAScriptModule]` entry classification, and compatibility matrix
3. Component Runtime Phase 1 Counter closure, including compiler host mapping and browser runtime
4. Emit / host materialisation consolidation for component `.mjs` and runtime manifest
5. Jolt integration planning only after Runtime contracts are stable
6. SourceMap partial rollout for active runtime consumers
7. Broader SourceMap programme
8. Ongoing documentation governance

**允许的并行**：
- Documentation governance 可以持续运行
- ECMAScript.Vue3 / RazorVue 旁路线只能作为 Vue artifact 参考并行推进（前提是不新增主线依赖或 Vue 命名特路）
- ECMAScript.Pinia 外部库线可与 compiler 主线并行推进（前提是不新增 Pinia 命名特路）
- Jazor Component Runtime Phase 0 文档、fixture、兼容矩阵可与 compiler 稳定化并行推进
- Narrow SourceMap 工作可以和 emit / Runtime artifact mapping 并行推进
- Emit 可以作为活跃依赖层持续演进（compiler 作为上游基础）

**不允许的并行**：
- Broad SourceMap 扩张不能超过 compiler / emit 稳定性
- RazorVue、Jolt 或 CSX 不能绕过 Jazor Component Runtime 再定义新的主线组件执行模型
- Runtime authoring 广度不能超过 `[ECMAScriptModule]` 入口分类、Razor SG、compiler host mapping、runtime surface、DOM renderer 合同闭环

## 详细工作流说明

### Compiler 主线

**当前状态**：主干接近稳定，当前重点在主线闭环和边界收敛

`Jazor.Compiler` 仍然是当前仓库里最成熟的主干资产。编译器主链路已经接近稳定主干，当前工作重点不是重做架构，而是维持主线闭环、控制边界扩张、给外围能力提供稳定依赖面。

**下一步行动**：

1. **Output closure**
   - 巩固 `ESGenerator -> catalog -> output` 闭环
   - 避免测试链路和真实输出链路继续分裂
   - 参考：[TransformationRoadmap.md](./compiler/TransformationRoadmap.md)

2. **Import closure**
   - 让 import 从收集阶段进到稳定落盘阶段
   - 保持 import 命名、去重和顺序稳定

3. **Host semantics seam**
   - 稳定 `Inline` / `Compile` 分工
   - 避免宿主语义扩张又跑回来破坏 compiler 主线边界
   - 参考：[InlineAstTemplateSpec.md](../01-目标/compiler/InlineAstTemplateSpec.md)

**深度文档**：
- [Compiler Architecture Bridge](../01-目标/compiler/architecture.md)
- [Jazor.Compiler 文档索引](../01-目标/compiler/README.md)

---

### Jazor Component Runtime

**当前状态**：当前前端 authoring/runtime 主线。它取代 RazorVue/Jolt/CSX 作为主要组件执行模型。

Jazor Component Runtime 只处理显式标注 `[ECMAScriptModule]` 的 Razor component。它以官方 Razor Source Generator 产物为输入，以 ASP.NET Core Components 固定版本源码为兼容规范，由 Jazor.Compiler 编译 opt-in 组件类并由 `@jazor/runtime` 在浏览器 ES module 环境执行 Razor render tree。

**下一步行动**：

1. **Phase 0: 基线与矩阵**
   - 锁定 ASP.NET Core Components upstream tag
   - 明确 `[ECMAScriptModule]` 入口分类，区分静态模块、Runtime 组件和非入口组件
   - 建立 P0-P4 兼容矩阵、runtime public surface、compiler host mapping 表
   - 选定 Counter、父子组件、ChildContent、列表、表单雏形 fixtures

2. **Phase 1: Counter 闭环**
   - 实现最小 `ComponentBase`、`RenderTreeBuilder`、`RenderTreeFrame`、render queue 和 DOM mount
   - 支持外部宿主基类继承与稳定 runtime import
   - 不改 `.razor` 源码运行按钮点击更新

3. **Phase 2+: 组件模型与生态兼容**
   - 按计划推进参数、`EventCallback`、`RenderFragment`、生命周期、diff、级联参数、表单、路由和 JS interop
   - 每个 unsupported 项都必须 fail-fast 并进入兼容矩阵

**深度文档**：
- [Jazor Component Runtime 工程计划](./jazor-component-runtime-plan-2026-07-06.md)

---

### ECMAScript.Vue3 / RazorVue 旁路线

**当前状态**：旁路参考线，不再承载主要组件执行模型。

`ECMAScript.Vue3` 与 `RazorVue` 保留外部库、Vue artifact、slot/SFC、设计期生成经验的参考价值，但不再定义 Jazor 的主线前端 runtime。后续工作只能作为可选互操作或历史材料推进，不能反向要求 compiler 或 runtime 为 Vue/SFC 协议重塑核心边界。

**下一步行动**：

1. **冻结主线扩张**
   - 不继续把 RazorVue 推进为默认组件执行模型
   - 不为 Vue SFC/slot 协议新增 compiler 主线特路

2. **保留参考材料**
   - 保留 H/slot/SFC lowering、design-time artifact 和测试治理经验
   - Runtime 需要 Vue interop 时，再以显式兼容任务评估复用

**深度文档**：
- [ECMAScript.Vue3 目标索引](../01-目标/ecmascript.vue3/README.md)
- [ECMAScript.Vue3 计划索引](./ecmascript.vue3/README.md)
- [ECMAScript.Vue3 状态快照](../03-完成/ecmascript.vue3/status.md)

---

### Emit / Host Materialisation

**当前状态**：承担 catalog、manifest、materialisation 以及 sourcemap/output 承接职责，仓库级入口已经补齐第一层桥接

Emit 不是单独的大专题，但确实是多个工作流共同的承接层。主线切换后，它优先负责把 compiler / component runtime 的产出落盘成 host-facing artefact。

**下一步行动**：

1. **显式化 materialisation / sourcemap 承接职责**
   - 让 manifest / catalog / writer 的职责更清晰
   - 保持模块 README 和 repo-level bridge 同步

2. **维持 emit test 和真实输出链路的一致性**
   - 避免测试和真实执行继续分裂

**深度文档**：
- [Modules Bridge](../01-目标/jolt/modules-bridge.md)
- [Jazor.Emit README](../../src/Jazor.Emit/README.md)
- [Emit.Pipeline.Overview.md](../01-目标/compiler/emit/Emit.Pipeline.Overview.md)

---

### Jolt

**当前状态**：开发期宿主旁路。Runtime 合同稳定前，Jolt 不承载新的组件执行模型。

历史上 `Jolt` 是 `.jazor` 的开发时边界，包含 In-proc Razor/Roslyn、Deno frontend worker、LSP bridge/coordinator、Dev Server/HMR、SourceMap 管线和 build lane。主线切换后，这些实现保留工程参考价值；只有 Jazor Component Runtime 的输入、输出、manifest、source map 和 dev-server 合同稳定后，才以显式集成任务重新进入 Jolt。

**下一步行动**：

1. **保持冻结边界**
   - 不继续扩展既有 Jolt `.jazor` authoring lane
   - 不为 Runtime Phase 0/1 修改 Jolt runtime / LSP / Deno pipeline

2. **保留参考价值**
   - Runtime 可以读取 Jolt 的 manifest、SourceMap、DevServer、Deno worker 经验
   - 复用必须等 Runtime contract 稳定后以显式集成任务进行

**深度文档**：
- [Jolt 状态快照](../03-完成/jolt/status.md)

---

### Jazor CSX Frontend

**当前状态**：暂停为旁路评估线。

CSX 不再作为前端 authoring/runtime 主线推进。除非后续另有架构决策，它只保留为历史方案和潜在可选 frontend 设想；当前资源应优先投入 Razor Component Runtime 的官方 Razor SG 兼容路线。

**下一步行动**：

1. **暂停扩张**
   - 不推进新的 parser、IR、shadow C# 或 `.jsx` artifact 实现
   - 不与 Razor Component Runtime 并行竞争主线定位

2. **保留边界约束**
   - 如果未来重启，必须先写新的决策记录
   - 不能分叉核心 C# 语义或绕过 Jazor.Compiler lowering

**深度文档**：
- [CSX 目标](../01-目标/csx/README.md)
- [CSX 实施计划](./csx/CSX.Frontend.ImplementationPlan.md)

---

### SourceMap

**当前状态**：broad programme 仍偏保守；既有 Jolt / Deno narrow lane 保留参考价值，后续 active consumer 优先切向 Component Runtime。

SourceMap 当前不是“全 deferred”也不是“全 active”。更准确的状态是：构建/服务主链路已可用，调试消费与精度仍需持续补齐。

**下一步行动**：

1. **继续完善 narrow lane**
   - 补齐调试消费路径（断点/调用栈）对 SourceMap 服务的直接使用
   - 持续提升映射精度与链路稳定性

2. **维持 broad programme 和 narrow lane 的边界**
   - 保持“稳定上游优先”原则，不提前扩大 broad rollout 范围

**深度文档**：
- [SourceMap.Overview.md](../01-目标/compiler/sourcemap/SourceMap.Overview.md)
- [SourceMap.ImplementationChecklist.md](./compiler/SourceMap.ImplementationChecklist.md)

---

## 执行门槛（Gates）

### Gate A. Compiler 稳定性优先于下游扩张

下游工作流不能强制要求重新设计 compiler 主路径。必须保持：
- Compiler 局部文档继续作为权威来源
- 核心转换边界对下游假设足够稳定

### Gate B. Emit / materialisation 桥接优先于下游闭环声明

如果 host-facing 交接仍然不清楚，就不能声称下游工作流已经闭环。这个门槛对 Runtime component `.mjs`、manifest、SourceMap writer 和 bundle chaining 流程都很重要。

### Gate C. Runtime 合同优先于 authoring 广度

Razor Component authoring 扩张必须建立在 `[ECMAScriptModule]` opt-in 入口、官方 Razor SG、compiler host mapping、runtime public surface、render batch、DOM renderer 和 source map/diagnostic 合同闭环之上。

### Gate D. Runtime 不能分叉核心 Razor/C# 语义

只有标注 `[ECMAScriptModule]` 的 `.razor` 组件进入 Runtime 链路；源码必须继续走官方 Razor Source Generator；组件 C# 必须走 Roslyn semantic binding 和 `Jazor.Compiler` / `SemanticWalker` lowering。禁止通过字符串拼接、JS 近似表达式、第二套 Razor parser 或第二套 C# lowering 规则实现。

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

---

## 维护规则

本文档应该保持简短。不要把它变成：

- 第二个 docs hub
- 子系统 checklist
- 更详细的替代局部设计文档的版本

如果某个变更只影响一个工作流的内部设计，更新那个工作流的文档而不是扩展本文档。
