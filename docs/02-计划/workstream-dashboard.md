# Jazor 工作流总览

> Status: 活跃计划
> Updated: 2026-05-05
> Positioning: 仓库级恢复入口，用于查看各工作流当前阶段、依赖顺序与下一步行动。
> Note: 更窄的 active plan 可以覆盖这里的具体执行切片，但不应反向改写这里对工作流边界和先后依赖的判断。

## 快速导航

| 工作流 | 当前阶段 | 下一步行动 | 状态文档 |
|--------|---------|-----------|---------|
| Compiler 主线 | 接近稳定 | 巩固 output closure、import closure、host seam | [详情](../03-完成/compiler/status.md) |
| ECMAScript.Vue3 外部库线 | 三阶段：Phase 1 完成，Phase 2 收口中 | 收敛 `H(...)` 规范层、在已固定的 slot contract 上扩 parity 覆盖、把 RazorVue 库模式切到 design-time SFC artifact | [详情](../03-完成/ecmascript.vue3/status.md) |
| ECMAScript.Pinia 外部库线 | 初始落地完成，进入增量补齐 | 补 setup-store helpers 设计、继续沉淀外部库模板与独立测试治理 | [详情](../03-完成/ecmascript.pinia/status.md) |
| Emit / Materialisation | 持续承接 | 显式化 materialisation / sourcemap 承接职责 | [详情](../03-完成/emit/status.md) |
| Jolt | Phase 1-6 收口中 | 调试闭环（CDP）、构建收口、Phase 7 扩展系统启动门槛 | [详情](../03-完成/jolt/status.md) |
| SourceMap | 局部活跃（narrow lane） | 继续补齐调试消费链路与精度提升 | [详情](../03-完成/sourcemap/status.md) |

## 依赖顺序与并行策略

**依赖顺序**（上游必须先稳定）：

1. Compiler mainline stabilisation
2. Emit / host materialisation consolidation
3. Jolt phase-one closure
4. Jolt authoring lane execution
5. SourceMap partial rollout for active consumers
6. Broader SourceMap programme
7. Ongoing documentation governance

**允许的并行**：
- Documentation governance 可以持续运行
- ECMAScript.Vue3 外部库线可与 compiler 主线并行推进（前提是不新增 Vue 命名特路）
- ECMAScript.Pinia 外部库线可与 compiler 主线并行推进（前提是不新增 Pinia 命名特路）
- Narrow SourceMap 工作可以和 Jolt / emit 集成并行推进
- Emit 可以作为活跃依赖层持续演进（compiler 作为上游基础）

**不允许的并行**：
- Broad SourceMap 扩张不能超过 compiler / emit 稳定性
- Authoring 广度不能超过 Jolt phase-one closure

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

### ECMAScript.Vue3 外部库线

**当前状态**：三阶段路线已明确，当前位于 Phase 2 收口阶段（Phase 1 已完成收口）

`ECMAScript.Vue3` 已从 `ECMAScript` 内核文档与模块布局独立出来，当前重点是维持外部库边界并避免 compiler 名称耦合回流。

**三阶段路线**：

1. **Phase 1: H 函数映射**
   - 目标：建立 `H(...)` / `VueObject` / slot sugar 的通用 contract 路线，去除 Vue 名称硬编码依赖。
   - 状态：已完成结构收口，转入持续硬化。

2. **Phase 2: Razor -> H**
   - 目标：把 Razor authoring 映射到稳定 `H(...)` 规范层，避免在 compiler 中扩张 Vue 专项分支。
   - 状态：收口中（当前主线）。
   - RazorVue 库模式的当前 Phase 2 下游目标：在 Roslyn design-time compilation 中生成 `.vue` SFC artifact，由 emit 只负责物化；当前剩余重点是 slot parity、generator per-component carrier topology 与默认 output 切换。

3. **Phase 3: Jolt**
   - 目标：在 Jolt 内完成工程化 authoring、调试与构建协同，但保持 Vue3 作为外部库边界。
   - 状态：规划中。

**下一步行动（Phase 2 主线）**：

1. **Razor -> H -> SFC 主线收口**
   - canonical `H(...)` 与 SFC semantic 主边界已落地
   - default slot forwarding 与 typed slot misuse 的核心边界已固定
   - 下一步扩更多 slot/scoping parity，并推进 generator per-component carrier topology 与默认 output-mode 切换
   - 再将 RazorVue 库模式切到 design-time SFC artifact 默认主工件
   - 不引入 render fallback

2. **覆盖矩阵驱动补齐**
   - 按 `vue3-api-coverage-matrix` 的优先级持续补齐 API
   - 仅走 record/overload/delegate/attribute 等公共语义路径

3. **host contract 收口**
   - 继续降低 compiler 对 Vue 名称耦合
   - 保持 `ChildrenToSlotIntrinsic` 与通用 contract 路线

4. **分层治理模板化**
   - 将 `Api/` + `Types/` + 壳文件约束抽象为外部库可复用模板

**深度文档**：
- [ECMAScript.Vue3 目标索引](../01-目标/ecmascript.vue3/README.md)
- [ECMAScript.Vue3 计划索引](./ecmascript.vue3/README.md)
- [ECMAScript.Vue3 状态快照](../03-完成/ecmascript.vue3/status.md)

---

### Emit / Host Materialisation

**当前状态**：承担 catalog、manifest、materialisation 以及 sourcemap/output 承接职责，仓库级入口已经补齐第一层桥接

Emit 不是单独的大专题，但确实是多个工作流共同的承接层。它负责把上游 compiler / Jolt 的产出落盘成 host-facing artefact。

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

**当前状态**：主链路已进入“能力收口与补齐”阶段，Phase 1/2/3 主路径已落地，Phase 4/5/6 持续推进，Phase 7 仍在规划。

`Jolt` 当前是 `.jazor` 的唯一开发时边界。In-proc Razor/Roslyn、Deno frontend worker、LSP bridge/coordinator、Dev Server/HMR、SourceMap 管线和 build lane 已形成活跃主路径。

**下一步行动**：

1. **调试闭环补齐（Phase 4）**
   - 从 DAP fallback 推进到 CDP 实浏览器闭环
   - 将 SourceMap 服务完整接入断点/调用栈映射路径

2. **构建与 LSP 收口（Phase 5/6）**
   - 继续巩固 manifest/css/js/source map 产物一致性
   - 收敛跨 lane supplement 边界，避免“伪造语义结果”

3. **扩展系统启动门槛（Phase 7）**
   - 先引入最小 provider 抽象（diagnostic/code-action）
   - 在不破坏当前主线稳定性的前提下逐步扩展

**深度文档**：
- [Jolt 状态快照](../03-完成/jolt/status.md)

---

### SourceMap

**当前状态**：broad programme 仍偏保守；narrow lane（Jolt / Deno）已形成可运行链路并进入收口阶段。

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

如果 host-facing 交接仍然不清楚，就不能声称下游工作流已经闭环。这个门槛对 Jolt artifact / manifest 流程和 SourceMap writer / bundle chaining 流程都很重要。

### Gate C. Jolt 最小路径优先于 authoring 广度

Authoring 扩张应该建立在已闭环的最小 Jolt 路径上，而不是绕过它。

### Gate D. Jolt authoring 不能分叉核心语义

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

---

## 维护规则

本文档应该保持简短。不要把它变成：

- 第二个 docs hub
- 子系统 checklist
- 更详细的替代局部设计文档的版本

如果某个变更只影响一个工作流的内部设计，更新那个工作流的文档而不是扩展本文档。
