# RazorVue 阶段评估（2026-04-06）

> Status: current status snapshot
> Positioning: Workstream-specific status snapshot for the active RazorVue lane.

## 1. 评估范围

本次评估聚焦 RazorVue 当前已经进入主干的设计实现，不重新定义一套新架构，而是基于现有代码、测试与文档，回答三个问题：

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

RazorVue 当前不是“在编译器里顺手多加一点 Vue 支持”，而是一条独立的 Vue-first 编译路径：

- Razor 继续承担作者侧模板入口。
- Vue 是实际运行时语义目标。
- Roslyn/analyzer 负责发现与提取语义输入。
- RazorVue core 负责 descriptor、render tree、lowering、artifact shaping。
- `DenoHost` 仍然保留后续宿主/构建所有权。

### 2.2 当前真实分层

当前分层已经从早期的职责混杂，收敛为更清晰的结构：

- `Jazor.Compiler`
  - 保留通用编译基础设施与静态模块主线。
- `Jazor.Razor`
  - 保留 Razor 侧最薄基类语义。
- `Jazor.RazorVue`
  - 已经成为 RazorVue 核心层。
  - 当前承载 compilation context、semantic snapshot、descriptor、render tree、pipeline、lowering、artifact/catalog 等核心语义。
- `Jazor.RazorVue.Analysis`
  - 当前定位为薄 Roslyn host。
  - 主要负责 generator 入口接线与诊断投影，而不再拥有 RazorVue 核心语义。

这点在代码与测试上都已经有明确体现，而不再只是设计意图。

### 2.3 当前主链路

当前主链路已经清晰稳定为：

`RazorVueCompilationContext -> RazorVueSemanticSnapshot -> RazorVuePipeline -> RazorVueArtifactFactory -> RazorVueCatalog`

各阶段职责大致如下：

1. `RazorVueCompilationContext`
   - 从 `Compilation` 建立 RazorVue 视角的共享上下文。
   - 负责组件发现、入口分类、生命周期方法识别、snapshot 构建。
2. `RazorVueSemanticSnapshot`
   - 作为 lowering 前的编译期语义载体。
   - 聚合 descriptor、lifecycle、logic、source origins、imported namespaces 等信息。
3. `RazorVuePipeline`
   - 负责串接 semantic frontend、artifact lowerer 与 catalog builder。
   - 当前已明确归属 `Jazor.RazorVue`，不再归属于 Analysis 宿主层。
4. `RazorVueArtifactFactory`
   - 负责从 snapshot + render tree 生成最终 Vue artifact。
   - 包含模块代码拼装、lifecycle lowering、identity/hash shaping、HMR boundary 初步分类。
5. `RazorVueCatalog`
   - 作为 host-facing compiler-owned carrier，供 generator/emit 阶段继续物化。

### 2.4 当前 lowering 方向

当前产物模型已经固定为标准 Vue ESM：

- `defineComponent`
- `setup(props, { emit, slots, expose, attrs })`
- `return () => h(...)`

这意味着当前实现不是把 Razor 组件硬映射成 Blazor runtime clone，而是明确投影到 Vue Composition API/Render Function 语义。

## 3. 当前已完成的能力

### 3.1 入口与分流

已完成：

- `[ECMAScriptModule]` 下的静态模块路径与 RazorVue 组件路径分流。
- `JazorComponent` / `VueComponent` 继承关系约束。
- Roslyn analyzer 对常见误用进行前置诊断。

### 3.2 组件语义提取

已完成：

- props 提取
- emits 提取
- slots 提取
- 基础 bind/model 相关语义收集
- imported namespaces 与 source origin 的基础保留

这说明当前 RazorVue 已经不只是“能认出组件”，而是能拿到 Vue lowering 需要的核心 contract。

### 3.3 RenderTree -> Vue render function 主链路

已完成并已有测试覆盖：

- HTML element lowering
- 子组件节点 lowering
- props 透传
- emit/listener 映射
- 默认 slot
- named slot
- scoped slot
- 基础 `if`
- 基础 `foreach`

因此，`BuildRenderTree` 到 Vue `h(...)` 的第一条可用主链路已经闭合，不再只是设计稿或空壳实现。

### 3.4 Lifecycle safe subset lowering

这一部分是当前最关键的新进展之一。

已完成的安全子集包括：

- `OnInitialized` / `OnInitializedAsync`
  - lower 到 `onMounted(...)`
- `OnParametersSet` / `OnParametersSetAsync`
  - lower 到 `watch(() => [props...], ..., { immediate: true })`
- `OnAfterRender` / `OnAfterRenderAsync`
  - lower 到 `onMounted(...) + onUpdated(...)`
  - 对 `firstRender` 做显式桥接

这里的重点不是“所有 lifecycle 都支持了”，而是：

- 当前支持的是能稳定映射到 Vue closure/hook/watch 语义的安全子集。
- 当前不再接受“生命周期存在就生成一个空 hook 壳”的模糊行为。
- 超界时走明确诊断，而不是假装成功。

### 3.5 结构化诊断面

当前除了通用 fallback `JAZORVGA001` 外，已存在更具体的诊断面：

- `JAZORVGA002`：组件未找到
- `JAZORVGA003`：短名组件歧义
- `JAZORVGA004`：与保留 intrinsic 名冲突
- `JAZORVGA005`：lifecycle lowering 超出当前安全子集

这意味着当前主链路的失败面，已经开始从“统统归为 catalog generation failed”向“按问题类型归位”演进。

### 3.6 Artifact / identity / HMR reservation

已完成：

- `DescriptorHash`
- `TemplateHash`
- `LogicHash`
- `HmrBoundaryKind`
- source origins 的基础保留

这部分仍然主要服务于未来 HMR/sourcemap，而不是表示运行时 HMR 已完成，但结构预留已经落地到 artifact 层。

## 4. 当前部分完成、但尚未闭合的能力

### 4.1 Logic extraction 仍然是窄子集

虽然当前已有 lifecycle/EventCallback bridge 这一条 logic lane，但整体 logic extraction 还远未完整。

当前尚未闭合的包括：

- 通用实例字段/方法语义
- 更广泛的 setup-side state lowering
- 通用 helper call / 深层成员链 / 复杂表达式 payload
- 完整组件实例语义在 Vue `setup()` 中的投影

### 4.2 Lifecycle 支持仍然是 safe subset，不是完整实例桥接

当前不应把 RazorVue 描述成“已经支持 lifecycle”。

更准确的说法是：

- 已支持 lifecycle safe subset lowering
- 尚未支持完整 lifecycle runtime equivalence

仍然未完成的主要包括：

- `Dispose*`
- `ShouldRender`
- `SetParametersAsync`
- 任意依赖实例状态与复杂成员访问的 lifecycle 方法体

### 4.3 DenoHost 终局集成尚未闭合

当前 artifact/catalog/emit 主线已经具备 host-facing shape，但“compiler + host 的最终闭环证据”还不充分。

更准确地说：

- compiler 侧 carrier 已经成形
- emit 侧物化也已有明确形状
- 但 end-to-end host 消费仍不应描述成完全收口

### 4.4 HMR / sourcemap 仍属结构预留

当前已有：

- identity 拆分
- boundary 分类
- source origin 保留

但没有完整落地：

- runtime HMR 行为
- final sourcemap 输出
- host/runtime 对这些 metadata 的完整消费闭环

## 5. 当前设计边界与主要取舍

### 5.1 当前没有引入“完整组件实例对象”中间层

这是当前实现的核心取舍之一。

当前 lifecycle/logic lowering 并没有先造一个“假的 Blazor 组件实例 runtime”，再把所有逻辑都挂进去；而是直接以 Vue `setup()` closure 为目标，允许那些可以安全投影的语义进入 lowering。

收益：

- 当前主链路更容易稳定。
- 不会因为伪造实例语义而快速失真。
- 可以让 diagnostics 清晰区分“支持的子集”和“未支持的语义”。

代价：

- 表达力仍然有限。
- 复杂 logic/lifecycle 暂时只能失败，而不能自动降级。

### 5.2 当前更重视“明确失败”，而不是“模糊成功”

从 lifecycle lowering 的实现与测试可以看出，当前设计明显偏向：

- 能保证语义边界时才 lowering
- 超出边界时给结构化诊断
- 不再生成误导性的空 hook 或貌似可运行的错误代码

这对当前阶段是对的，因为 RazorVue 仍在打磨主链路，不适合用 silent fallback 掩盖语义缺口。

## 6. 当前风险点

### 6.1 文档容易落后于代码

RazorVue 近期变动较快，尤其是：

- layering 收敛
- lifecycle safe subset lowering
- structured diagnostics

如果文档仍沿用旧表述，很容易出现两种误差：

1. 把已完成的能力继续写成“未来设计”。
2. 把尚未完成的能力误写成“已经支持”。

### 6.2 目前最容易被误判的点是 lifecycle 支持范围

外部阅读者最容易误解成：

- RazorVue 已经支持完整 Blazor lifecycle

但真实状态是：

- 已支持 EventCallback/emit 驱动的 lifecycle safe subset
- 尚未支持完整实例桥接与 runtime equivalence

### 6.3 逻辑能力与模板能力成熟度不对称

当前模板/render 主链路的成熟度已经明显高于通用 logic lowering。

这意味着下一阶段如果继续扩大表达能力，最需要警惕的是：

- 不要为了“支持更多逻辑”破坏现有稳定的 render/lifecycle 子集
- 不要让 `Jazor.RazorVue.Analysis` 再次回流承担核心语义

## 7. 建议的下一步

建议后续按“阶段收口里程碑”推进：

1. **里程碑 A：文档口径收口**
   - 让 Overview / Design / Checklist 与当前主链路对齐，并稳定使用同一阶段表述。
2. **里程碑 B：lifecycle safe subset 证据边界收口**
   - 优先补“哪些 payload expression 允许、哪些不允许”的清晰边界。
3. **里程碑 C：setup-side logic 最小闭环**
   - 先定义最小可闭环的下一批 setup-side logic，而不是一次性追求通用实例 lowering。
4. **里程碑 D：compiler-host 第一条 end-to-end 闭环证据**
   - 在 compiler-host 边界上继续保留 artifact/source-origin/identity 的稳定 contract，并补足最小 host 消费闭环证据，避免未来 HMR/sourcemap/DenoHost 集成时重新设计 carrier。

## 8. 结论

截至 2026-04-06，RazorVue 已经跨过“方向确认 / 设计起步”阶段，进入“主链路形成后的阶段收口”阶段：

- 分层已明显收敛
- descriptor/render/lowering/artifact 主链路已存在
- component/slot/props/emits 基础 lowering 已打通
- lifecycle safe subset 已进入可执行 lowering
- structured diagnostics 已开始替代单一 fallback

但它仍然没有跨过“phase-one 收口完成”这个里程碑。

更准确的阶段判断是：

**RazorVue 当前处于“核心主链路里程碑已达成，正在推进 phase-one 稳定收口里程碑”的阶段。**
