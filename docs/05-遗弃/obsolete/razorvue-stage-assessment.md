# RazorVue 阶段评估（2026-04-06）

> Status: current status snapshot
> Positioning: Workstream-specific status snapshot for the historical RazorVue lane.
> Note: this document is archived context; the current active development-time boundary is `Jolt`, with Deno as the only runtime host path and `Jolt --analysis-stdio` as the migration-time analysis process entrypoint. For current Jolt progress, read `docs/status/jolt-status.md`.

## 1. 评估范围

本次评估聚焦 RazorVue 当前已经进入主干的设计实现，不重新定义一套新架构，而是基于现有代码、测试和文档，回答三个问题：

1. RazorVue 现在采用什么实现思路。
2. 主链路已经推进到什么程度。
3. 还缺什么、下一步该优先补什么。

本次评估主要依据：

- `src/Jazor.RazorVue/RazorVue/RazorVuePipeline.cs`
- `src/Jazor.RazorVue/RazorVue/RazorVueCompilationContext.cs`
- `src/Jazor.RazorVue/RazorVue/Lowering/RazorVueArtifactFactory.cs`
- `src/Jazor.RazorVue/RazorVue/Lowering/RazorVueExpressionEmitter.cs`
- `src/Jazor.RazorVue.Analysis/RazorVueGenerator.cs`
- `src/Jazor.CompilerTest/RazorVuePipelineTests.cs`
- `src/Jazor.CompilerTest/ESGeneratorTests.cs`

## 2. 当前 RazorVue 的设计实现思路

### 2.1 总体定位

RazorVue 当前不是"在编译器里顺手多加一点 Vue 支持"，而是一条独立的 Vue-first 编译路径：

- Razor 继续承担作者侧模板入口。
- Vue 是实际运行时语义目标。
- Roslyn / analyser 负责发现与提取语义输入。
- RazorVue core 负责 descriptor、render tree、lowering、artifact shaping。
- 当前方向里，这部分宿主 / 构建所有权已经收拢到 `Jolt` 和其 Deno 运行时路径里。

### 2.2 当前真实分层

当前分层已经从早期的职责混杂，收敛为更清晰的结构：

- `Jazor.Compiler`
  - 保留通用编译基础设施与静态模块主线。
- `Jazor.Razor`
  - 保留 Razor 侧最薄基类语义。
- `Jazor.RazorVue`
  - 已经成为 RazorVue 核心层。
  - 当前承载 compilation context、semantic snapshot、descriptor、render tree、pipeline、lowering、artifact / catalog 等核心语义。
- `Jazor.RazorVue.Analysis`
  - 当前定位为薄 Roslyn host。
  - 主要负责 generator 入口接线与诊断投影，而不再拥有 RazorVue 核心语义。

### 2.3 Descriptor extraction design

当前 descriptor extraction 已经采用稳定模式：

- `VueComponentDescriptorFactory` 集中提取 component、prop、emit、slot、exposed member。
- `RazorVueEntryClassifier` 负责 entry point classification。
- `RazorVueCompilationSymbols` 负责符号常量管理。
- `RazorVueCompilationContext` 负责整体 compilation snapshot。

这套结构已经在主干验证过，不再是早期探索状态。

### 2.4 Render tree and lowering design

当前 render tree 和 lowering 已经形成稳定边界：

- `RazorVueArtifactFactory` 负责从 descriptor 构建完整 Vue SFC。
- `RazorVueExpressionEmitter` 负责 C# expression → Vue template expression lowering。
- Lowering 只负责语法适配，不重新定义组件语义。

这套边界已经明确：descriptor 是 semantic truth，render tree 是 syntax projection。

### 2.5 Current test coverage reality

当前测试覆盖已经集中在：

- `RazorVuePipelineTests.cs`
  - 覆盖完整 pipeline 流程。
  - 验证从 Razor-authored 输入到 bridge artifacts 产出的端到端转换。
- `ESGeneratorTests.cs`
  - 覆盖 expression lowering。
  - 验证 C# → JavaScript expression 转换正确性。

测试侧已经证明主链路可运行，当前不需要再从零搭测试框架。

## 3. 当前主链路推进程度

### 3.1 已经完成的核心块

- ✅ Descriptor extraction pipeline。
- ✅ Entry classification。
- ✅ Compilation context and semantic snapshot。
- ✅ Basic component lowering。
- ✅ Prop / emit / slot handling。
- ✅ Render tree construction。
- ✅ Expression lowering core。
- ✅ Vue bridge artifact assembly。
- ✅ End-to-end pipeline test。

### 3.2 当前正在推进的块

- ⏳ Helper composition（已经进主干，正在做两层级联验证）。
- ⏳ Layering refinement（已经进主干，正在收口 phase-one 边界）。
- ⏳ Lifecycle safe subset（已经进主干，正在补齐边界案例）。

### 3.3 当前明确的缺口

- ❌ Library component authoring stub and descriptor extraction。
- ❌ Library component registry and discovery。
- ❌ Strong typed slot context lowering for library components。
- ❌ Event / binding closure for library components。
- ❌ Host plugin requirement declaration。
- ❌ Design-time diagnostics for common authoring errors。

## 4. 下一步行动

### Phase-one closure（优先级最高）

**目标**：完成 RazorVue 最小可用路径的闭环

**具体行动**：

1. **完成 layering 实现**
   - 收口 phase-one 边界
   - 验证两层级联 helper composition
   - 后续执行参考已并入：[RazorVue.ImplementationSkeleton.md](../../02-计划/jolt/razorvue-implementation/RazorVue.ImplementationSkeleton.md)

2. **完成 lifecycle safe subset 实现**
   - 补齐边界案例
   - 确保生命周期语义在 Vue 侧有安全映射
   - 后续执行参考已并入：[RazorVue.ImplementationChecklist.md](../../02-计划/jolt/razorvue-implementation/RazorVue.ImplementationChecklist.md)

**验证标准**：
- Phase-one 核心场景全部有测试覆盖
- 边界行为明确且稳定
- 文档和代码保持同步

### Authoring lane 收口（次优先级）

**目标**：让 library component authoring 进入可执行状态

**具体行动**：

1. **从 PR1 开始执行 authoring roadmap**
   - Library metadata extraction
   - Default library discovery
   - First Vuetify package
   - 后续执行参考已并入：[RazorVue.Authoring.ProductDefinition.md](../../01-目标/razorvue/design/RazorVue.Authoring.ProductDefinition.md)

2. **跨过 mid-authoring review gate**
   - 确认 stub-as-truth-source 仍然成立
   - 确认 lowering 保持通用
   - 确认 package-specific branches 没有渗透进核心

**验证标准**：
- Library components 在 registry 中可见
- 第一批 Vuetify 组件可用
- Authoring 体验保持 Blazor-like

### SourceMap / bundle chaining（并行推进）

**目标**：支撑 RazorVue 的 host-facing 需求

**具体行动**：
- 完成 RazorVue bundle chaining 实现
- 让 writer / manifest / bundler 演进就位
- 后续执行参考已并入：[Phase 2: 编译管道统一 + Source Map](../../02-计划/jolt/phase2-sourcemap.md)

## 5. 深度文档

- [RazorVue.Overview.md](../../01-目标/razorvue/design/RazorVue.Overview.md)
- [RazorVue.Design.md](../../01-目标/razorvue/design/RazorVue.Design.md)
- [RazorVue.ComponentDescriptorSpec.md](../../01-目标/razorvue/design/RazorVue.ComponentDescriptorSpec.md)
- [RazorVue.DenoHostContract.md](../../01-目标/razorvue/design/RazorVue.DenoHostContract.md)
- [RazorVue.ImplementationChecklist.md](../../02-计划/jolt/razorvue-implementation/RazorVue.ImplementationChecklist.md)

## 6. 当前风险

### 6.1 Authoring 扩张可能分叉核心语义

风险信号：
- Package-specific lowering branches 出现在 `Jazor.RazorVue` core。
- Descriptor extraction 开始为特定包增加特殊逻辑。

缓解措施：
- 保持 stub-as-truth-source 原则
- 保持 lowering generic 性质
- 在 mid-authoring gate 进行强制检查

### 6.2 Phase-one 和 authoring 并行推进的理解成本

风险信号：
- Phase-one closure 还没完成，authoring 已经开始推进。
- 文档如果写不清楚，容易误读成"所有 RazorVue 工作都是那个阶段的 active execution"。

缓解措施：
- 明确区分 phase-one closure 和 authoring lane
- 在 dashboard 中显式标注优先级
- 保持 gates 约束

### 6.3 Test coverage 仍然集中在主链路，边界案例覆盖不足

风险信号：
- Helper composition 边界案例可能不够。
- Lifecycle safe subset 的边界行为可能存在未覆盖场景。

缓解措施：
- Phase-one closure 必须包含边界案例测试
- 持续补充边缘场景
- 保持测试和文档同步
