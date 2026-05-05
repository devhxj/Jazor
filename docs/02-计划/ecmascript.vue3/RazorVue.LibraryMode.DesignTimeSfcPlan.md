# RazorVue 库模式 Design-Time SFC 方案

> Status: 活跃计划
> Updated: 2026-05-05
> Positioning: 基于 `ECMAScript.Vue3` Phase 2（Razor -> `H(...)`）canonical contract，定义 RazorVue 库模式如何把既有 canonical 语义 materialize 成 design-time `.vue` SFC artifact。
> Scope: 只覆盖 RazorVue 库模式；不覆盖 Jolt Phase 3；不引入 render fallback；不把 `.vue` authoring 或 Volar/LSP 虚拟文档作为前提。

## 摘要

这份方案不是新的模板语义设计，也不是独立于 Phase 2 的第二套 Vue lowering。

- Phase 2 继续负责定义 Razor authoring 到 canonical `H(...)` / setup / lifecycle 语义。
- 本方案只负责把这套既有 canonical 语义 materialize 为 design-time `.vue` SFC artifact。
- 任何绕过 Phase 2 contract 直接从 `BuildRenderTree`“拼 SFC”的路径都视为错误方向。

## 0. 一句话决策

RazorVue 库模式改为：

- **在 Roslyn design-time compilation 中生成每组件 `.vue` SFC artifact**
- **在 build/emit 阶段只物化该 artifact**
- **Phase 2 的 canonical `Razor -> H(...)` 语义层成为 SFC 生成的唯一真相源**
- **不允许 render fallback**

## 1. 背景与问题

当前库模式的主链路仍然是：

`Compilation -> SemanticSnapshot -> RenderTree -> Vue ESM artifact -> Emit`

它的优点是：

- 语义提取时机稳定；
- 可直接复用 Roslyn `Compilation`、Razor 生成的 `BuildRenderTree` 与现有 catalog 机制；
- build 和 emit 的职责清晰。

但这条线与当前目标不再一致：

1. Vue 运行时与构建生态对 `.vue` SFC 有专门优化与稳定消费路径。
2. 库模式的主工件不应再被 `DenoHost` 的当前消费能力限制。
3. 如果 SFC 是主工件，就必须在**唯一拿得到 Razor 生成语义**的时间窗口内完成语义生成。
4. 这个窗口不是 build 后 `Jazor.Emit`，而是 Roslyn design-time / compilation-time。

因此，库模式需要从“发送 `defineComponent + render` 的 ESM artifact”切换到“发送 `.vue` SFC artifact”，并保持：

- compiler/analysis 负责语义生成；
- emit 负责文件物化；
- bundler/host 负责后段消费；
- 不引入第二套语义 lowering。

## 2. 与 ECMAScript.Vue3 Phase 2 的关系

本方案不是新增独立工作流，而是 **Phase 2: Razor -> `H(...)` canonical lowering** 的下游收口。

Phase 2 已经建立或正在建立的 contract 包括：

- `H(...)` canonical 分类；
- 最小 arity；
- 无多余 `null` 占位；
- component direct-child default-slot sugar；
- typed slot contract；
- single-evaluation / evaluation-order 语义；
- object-literal / attrs / props / slots / emits 的统一诊断边界。

本方案要求：

1. RazorVue SFC 生成必须**先通过** Phase 2 canonical `H(...)` 语义层。
2. `.vue` SFC emitter 不得直接消费原始 `BuildRenderTree` 细节。
3. 任何不落入 canonical `H(...)` contract 的路径必须诊断失败，不得绕过 Phase 2 直接“拼出一份看起来能跑的 Vue”。

换句话说：

- Phase 2 负责定义 **Razor authoring 到 Vue canonical semantics**。
- 本方案负责定义 **canonical semantics 到 SFC artifact**。

## 3. 目标

### 3.1 主目标

1. 把 `.vue` SFC 设为 RazorVue 库模式的主工件。
2. 把 design-time compilation 设为 SFC 语义生成的唯一窗口。
3. 保持 build/emit 只做物化，不重复解释语义。
4. 用 per-component 增量 topology 支撑稳定、高频、低抖动的 design-time 生成。
5. 用 block-aware artifact contract 支撑后续 diff、hash、source map 和 host 集成。

### 3.2 质量目标

1. 不引入 render fallback。
2. 不引入第二套和 Phase 2 平行的模板语义。
3. 不把 `.vue` 物理文件写入 design-time generator 副作用路径。
4. 不把 bundler/host 约束反向注入 Phase 2 canonical 语义。

## 4. 非目标

以下事项不属于本方案范围：

- `.vue` 源文件 authoring；
- Volar/LSP 虚拟文档要求；
- Jolt Phase 3 lane/worker/projection 协同；
- SSR renderer、hydration runtime、custom renderer；
- `<script setup>` compiler macros 的用户 authoring surface；
- build 后再做第二轮 RazorVue 语义恢复。

## 5. 核心决策

### 5.1 SFC 是主工件，不是投影工件

RazorVue 库模式的主发射格式改为 `.vue` SFC。

旧的 `ModuleCode` 不是主真相源，也不再作为 first-class artifact 保留。

### 5.2 design-time compilation 是唯一语义生成窗口

SFC 语义生成放在 Roslyn incremental generator 驱动的 compilation-time。

原因：

- 只有此时同时拥有 `Compilation`、Razor 生成代码、symbol、descriptor、origin 信息；
- `Jazor.Emit` 当前定位是 materialisation，不应接管语言语义；
- 如果 build 再重新恢复语义，将引入第二套 lowering 和一致性风险。

### 5.3 Phase 2 canonical `H(...)` 模型是唯一真相源

SFC emitter 只接受 canonical `H(...)` 语义模型输入。

禁止路径：

- `BuildRenderTree -> 直接 template string`
- `RenderTree -> render fallback -> 包装成 SFC`
- `Emit 阶段反向从文本推回模板结构`

### 5.4 build 只负责物化，不负责再 lowering

build/emit 从程序集里的 generated catalog 读取 `VueSfcArtifact`，并将其写到：

- `obj/.../*.vue`
- output 目录
- package artifact

由后段 host/bundler 决定如何消费。

### 5.5 不允许 render fallback

若某段 Razor authoring 无法无损落入：

- Phase 2 canonical `H(...)` contract，或
- template/script-setup 可表达 contract

则必须直接产生诊断并中止该组件的 SFC artifact 生成。

## 6. 总体流水线

新主链路固定为：

`Compilation`
`-> RazorVueCompilationContext`
`-> RazorVueSemanticSnapshot`
`-> RazorVueRenderFragment`
`-> RazorVueCanonicalHComponentModel`
`-> RazorVueSfcSemanticModel`
`-> VueSfcArtifact`
`-> generated C# catalog carrier`
`-> Jazor.Emit materialisation`
`-> .vue files / manifest`

## 7. design-time 触发与增量拓扑

### 7.1 触发源

design-time SFC 生成由 `RazorVueGenerator : IIncrementalGenerator` 触发。

失效源包括：

- 组件 C# 源变更；
- Razor 生成的 `BuildRenderTree` 变更；
- `[ECMAScriptModule]` 候选集合变化；
- 引用程序集和库组件 metadata 变化；
- import/usings/namespace 变化；
- 影响 descriptor 解析的 attribute 或 base-type 变化。

### 7.2 拓扑要求

现有 `CompilationProvider.Combine(componentCandidates.Collect())` 不足以承载大体积 SFC artifact。

新的 topology 约束：

1. **候选发现增量化**
   - 继续使用 `ForAttributeWithMetadataName(...)`
   - 输出稳定的 component candidate key

2. **library registry 单次共享**
   - 每次 compilation 只生成一次 library descriptor registry
   - 所有 component artifact 共享

3. **per-component artifact 生成**
   - 每个候选组件独立生成 `VueSfcArtifact`
   - 任何单组件改动不得强制重发其它组件的大文本 artifact

4. **small index catalog**
   - 单独生成一个小型 index catalog
   - 只负责聚合 `GetArtifacts()`
   - 不承载大段 SFC 文本

### 7.3 生成文件形状

generator 应生成两类 carrier：

1. `Jazor.Generated.RazorVue.Artifact_<stable-id>.g.cs`
   - 持有单个 `VueSfcArtifact`
   - 持有 block text、hash、origin、manifest-facing metadata

2. `Jazor.Generated.RazorVueCatalog.g.cs`
   - 提供 `AssemblyName`
   - 提供 `GetArtifacts()`
   - 只聚合各组件 artifact provider

## 8. Canonical H 模型合同

### 8.1 定位

`RazorVueCanonicalHComponentModel` 是 RazorVue SFC 生成前的唯一语义边界。

它不是：

- 原始 `BuildRenderTree` 调用流；
- 直接文本模板；
- host/bundler 特定结构。

它是与手写 `H(...)` authoring 等价的稳定语义结构。

### 8.2 组成

每个 component model 至少包含：

- component identity
- props contract
- emits contract
- slots contract
- imports
- style requirements
- plugin requirements
- setup declarations
- lifecycle bindings
- template root tree
- source origins

### 8.3 template root tree 节点类型

节点类型固定为：

- `ElementNode`
- `ComponentNode`
- `TextNode`
- `InterpolationNode`
- `ConditionalNode`
- `ForEachNode`
- `SlotOutletNode`
- `TemplateFragmentNode`

节点上必须携带：

- canonical props/attrs/bindings
- children / slots
- origin anchors
- side-effect classification
- template-encodability classification

### 8.4 语义不变量

所有 canonical 节点必须满足：

1. 求值顺序稳定。
2. 单次求值约束稳定。
3. `null`/missing/optional 语义已在 canonical 层收口，不再把 ambiguity 留给 emitter。
4. default-slot sugar、typed slot、props+children 边界已在 canonical 层决议完成。
5. 任何需要 runtime helper 才可表达的行为必须显式挂入 setup semantic model，而不是留到 SFC 拼接阶段再猜。

## 9. SFC 语义模型合同

### 9.1 定位

`RazorVueSfcSemanticModel` 是 canonical `H(...)` 模型到 `.vue` 文本之间的桥接层。

它负责：

- 判断哪些结构可进入 `<template>`
- 判断哪些声明必须进入 `<script setup>`
- 组织 style/plugin/import block
- 计算 block 级 hash / source-origin boundary

### 9.2 block 结构

SFC semantic model 至少包含：

- `TemplateBlockModel`
- `ScriptSetupBlockModel`
- `StyleBlockModel[]`
- `CustomBlockModel[]`（当前可为空，但 contract 预留）

### 9.3 template encodability 分类

所有 canonical 节点/表达式必须先经过分类：

- `DirectTemplate`
  - 可直接编码为 template 节点/attribute/interpolation
- `TemplateViaSetupBinding`
  - 需要先提升为 setup 变量、`computed`、helper 或 method，再由 template 引用
- `NotTemplateEncodable`
  - 不能无损表达；必须诊断失败

不允许未分类节点直接进入 emitter。

### 9.4 `<script setup>` 承担的内容

`<script setup>` 负责：

- imports
- `defineProps`
- `defineEmits`
- lifted helper bindings
- `computed`
- methods
- watch / lifecycle registration
- slot helper or local component aliases

它不负责重新定义模板语义。

## 10. `VueSfcArtifact` 数据契约

`VueSfcArtifact` 必须替换现有 `VueCompiledArtifact` / emit-side `ModuleCode` contract。

最小字段集：

- `ComponentName`
- `RelativeSfcPath`
- `SfcText`
- `TemplateText`
- `ScriptSetupText`
- `StyleBlocks`
- `Imports`
- `Styles`
- `PluginRequirements`
- `Identity`
- `SourceOrigins`

### 10.1 `Identity` 字段

至少保留：

- `ComponentId`
- `ModuleId`
- `DescriptorHash`
- `TemplateHash`
- `LogicHash`
- `StyleHash`
- `HmrBoundaryKind`

说明：

- 即使库模式当前不以 HMR 为主，也必须保留 block/hash 粒度 contract；
- `StyleHash` 不能继续隐含在大文本 hash 里。

### 10.2 `StyleBlocks`

style block 不能简化成一个字符串数组。

每个 style block 至少需要：

- raw text
- `scoped`
- `module`
- `lang`
- optional external source
- origin anchors

### 10.3 `SourceOrigins`

必须继续保留 block-aware origin 信息，至少能支撑：

- template diagnostics remap
- script diagnostics remap
- style diagnostics remap
- future block-diff/update plan

## 11. 诊断边界

### 11.1 原则

不支持场景必须在 design-time generation 期间失败，不得降级为近似输出。

### 11.2 诊断分类

至少新增或重组以下诊断组：

- canonicalization failure
- unsupported template encoding
- unsupported setup extraction
- unsupported slot encoding
- unsupported lifecycle-to-script-setup lowering
- unsupported style block emission
- unsupported component/element ambiguity

### 11.3 诊断落点

诊断必须优先锚定：

1. Razor 源位置；
2. 若无 Razor 映射，则锚定 generated syntax location；
3. 最后才使用 generated fallback。

## 12. build / emit 承接方式

现有职责边界保留：

- generator 负责生成 C# catalog carrier
- `Jazor.Emit` 负责从程序集反射读取 catalog
- `Jazor.Emit` 负责物化 `.vue` 和 sidecar manifest

需要修改：

1. `RazorVueCatalogReader`
   - 从读取 `ModuleCode` 改为读取 `SfcText` 与 block metadata

2. `RazorVueModuleWriter`
   - 从写 `.mjs` 改为写 `.vue`
   - 继续写 `.map` / `.origins.json` / manifest

3. `RazorVueManifestFactory`
   - manifest 的 module entry 改为 `.vue` artifact contract

原则：

- emit 不做语义恢复
- emit 不重新判断 template 是否有效
- emit 只负责 deterministic materialisation

## 13. 测试与验收

### 13.1 设计时生成回归

新增测试必须覆盖：

- 单组件变更只重生成单组件 artifact
- library descriptor 变化能正确失效依赖组件
- index catalog 在稳定排序下可重复生成

### 13.2 语义一致性回归

必须证明：

- Razor -> canonical `H(...)` 与手写 `H(...)` 的 Phase 2 contract 一致
- template/script-setup 拆分不改变求值顺序
- default-slot sugar / typed slot / props/attrs/emits contract 不漂移

### 13.3 emit 物化回归

必须覆盖：

- 从程序集 catalog 读取 `VueSfcArtifact`
- 写出 `.vue`
- 写出 `.map`
- 写出 `.origins.json`
- 写出 RazorVue manifest

### 13.4 验收标准

只有以下条件同时满足，方案才算完成：

1. design-time compilation 可稳定产生 per-component SFC artifact；
2. 主工件已切换为 `.vue` SFC；
3. 无 render fallback；
4. build/emit 不再做第二套 RazorVue lowering；
5. Razor -> canonical `H(...)` -> SFC 的语义路径有聚焦回归守护；
6. 诊断、hash、origin、manifest contract 已同步切换到 SFC。

## 14. 推荐执行顺序

1. 固定 `VueSfcArtifact` / emit contract
2. 建立 `RazorVueCanonicalHComponentModel`
3. 建立 `RazorVueSfcSemanticModel`
4. 改 generator 为 per-component artifact topology
5. 改 catalog reader / emit writer / manifest
6. 补齐 diagnostics 与 source-origin mapping
7. 做 end-to-end materialisation 回归

## 15. 风险与控制

| 风险 | 影响 | 控制方式 |
|------|------|---------|
| 继续绕过 Phase 2 contract 直接拼 SFC | 高 | 强制引入 canonical H model 边界，禁止 RenderTree 直出 |
| generator 仍以单大 catalog 承载所有组件文本 | 高 | 改为 per-component carrier + small index |
| build 重新做 RazorVue lowering | 高 | 在 emit contract 中只保留 SFC artifact，不保留可诱导重 lowering 的旧主路径 |
| 为了“能跑”而引入 render fallback | 高 | 把 fallback 设为显式非目标；不支持即诊断 |
| template/script-setup 拆分破坏求值语义 | 高 | 在 SFC semantic model 阶段显式做 template-encodability 分类与 single-evaluation 守护 |
| host/bundler 约束反向污染 canonical 语义 | 中 | host 只消费 artifact，不参与 Phase 2 canonical 化 |

## 16. 当前过渡状态（2026-05-04）

当前仓库已经进入“先固定 SFC artifact/emit contract，再回切 compiler 主链”的过渡阶段。

### 16.1 已落地

1. `VueSfcArtifact` / `VueSfcArtifactIdentity` / block-aware source-origin contract 已建立。
2. `Jazor.Emit` 已能从程序集 catalog 读取 `VueSfcArtifact`，并物化 `.vue`、manifest 与 sidecar metadata。
3. manifest/diff/update-plan 已引入 `StyleHash`，可区分 style-only 变化。
4. `RazorVueCanonicalHComponentModel` / `RazorVueSfcSemanticModel` 已建立，SFC lane 不再直接消费原始 `BuildRenderTree` 细节。
5. SFC lowerer 已直接消费 canonical model，并与 legacy lane 共享 setup/lifecycle lowering seam。
6. 组件 import/path/binding 闭环已打通：用户组件 `.mjs -> .vue` 规范化、library component named import、Vue intrinsic no-import 都已有真实文本与 metadata 回归。
7. `TemplateViaSetupBinding` 已形成真实消费闭环；template 侧通过显式 binding-site identity 消费 lifted binding，不再依赖遍历顺序偶然一致。
8. typed slot outlet argument 已进入 canonical/SFC contract，可生成 `<slot ... :value="...">`。
9. 组件级 `LocalReference` 在无法安全提升时已 fail-fast，避免生成“字符串可写出、Vue 无法消费”的非法 `.vue`。
10. 相关 emit/test 回归已建立，证明“程序集 catalog -> `.vue` materialisation”这条后段链路可工作。

### 16.2 仍在过渡

1. generator 的默认输出模式仍是 `legacy`；SFC 仍通过显式 output-mode 切换启用。
2. mixed legacy/SFC catalog 仍被禁止；当前过渡态不是“双主工件并存”，而是显式选择其中一条 lane。
3. `RazorVuePipeline` / generator 主路径仍保留 legacy `defineComponent + render` 作为过渡车道，尚未把 SFC 设为默认主发射格式。
4. 增量 topology 仍偏向 `CompilationProvider.Combine(componentCandidates.Collect())`，per-component carrier 的 design-time 抖动控制尚未完全收口。
5. child component 的 callable scoped slot forwarding 在 SFC lane 中仍需以显式 canonical/semantic contract 收口；当前不能把它视为已完成 parity。

### 16.3 下一执行切片

推荐按以下顺序推进，不要跳步硬切：

1. 收口 child component callable scoped slot forwarding
   - 新增 canonical/SFC 显式 contract，区分 callable forwarding、普通 slot value、typed slot misuse
   - SFC template/script 必须保持与 legacy lane 同一调用语义

2. 切换 generator/catalog source emission
   - 从大一统 legacy catalog 切到 per-component SFC carrier + small index catalog

3. 再做 topology 细化
   - 收敛 per-component invalidation
   - 控制大文本 artifact 的 design-time 抖动

4. 最后再做默认 output-mode 切换
   - 在 parity 和 topology 都收口之后，再把 SFC 从显式开关切到库模式默认主工件

关键约束：

- 不要直接 `RenderTree -> template string`
- 不要先生成 render module 再包装成 SFC
- 不要让 `Jazor.Emit` 接手语义恢复

## 18. Phase 2 收口修订（2026-05-05）

在进入 design-time SFC 模式之后，真正的收口点不是“能生成 `.vue` 文件”，而是以下两条必须同时成立：

1. `<script setup>` 必须直接复用 RazorVue Phase 2 已有的 setup/lifecycle lowering 语义。
2. template 中出现的用户组件/库组件，必须在同一个 design-time artifact 中完成可消费的 import/binding/path 闭环。

### 18.1 setup/lifecycle 不能停留在占位生成

SFC lane 不允许出现：

- `undefined as any` 字段占位
- `throw new Error("...not connected yet")` 方法占位
- 仅按 bool flag 猜测导入 `onMounted/watch/onUpdated/onUnmounted`

正确做法是：

- 从真实 `RazorVueSemanticSnapshot` 出发
- 复用与 legacy lane 同一套 Roslyn method-body 分析
- 共用同一套 supported/no-op/unsupported 判定边界
- 对 `OnInitialized` / `OnParametersSet` / `SetParametersAsync` / `OnAfterRender` / `Dispose` 的 emit-style lowering 生成与 legacy lane 一致的 Vue hook 代码

换句话说：

- SFC lane 的 `<script setup>` 不是“另一份相似实现”
- 它只是 Phase 2 setup/lifecycle lowering 的另一种 materialization 形态

### 18.2 组件 import/path 必须按 SFC 消费模型闭环

当 canonical template 中出现子组件时，SFC lane 必须同时完成：

- template tag 名决议
- `<script setup>` import binding 决议
- artifact `Imports` metadata 决议
- 用户组件依赖从 `.mjs` 规范化到 `.vue`

具体规则：

- 用户组件：
  - template tag / local binding 使用稳定别名，例如 `ChildCardComponent`
  - import specifier 使用相对 `.vue` 路径
- library component：
  - 保持 package import specifier 不变
  - named export 直接映射到可在 template 中使用的本地 binding
- Vue intrinsic（例如 `Teleport`）：
  - 不生成额外 import

禁止出现：

- template 使用一个标签名，但 `<script setup>` 没有对应 binding
- artifact metadata 仍然宣称依赖 `./child-card.mjs`，而实际 SFC 文本已经切到 `./child-card.vue`
- 用 fully-qualified C# type name 泄露到 Vue local import alias

### 18.3 design-time 已打通，剩余重点转为 parity 守护

到这一阶段，库模式需要把“是否能在 design-time 触发 SFC 生成”的讨论视为已解决问题，后续重点转为：

- Phase 2 lowering parity
- SFC import/path/binding consistency
- block-aware manifest/hash/origin 保持与真实 SFC 文本一致
- 针对 field/method/lifecycle/component-import 的聚焦回归

### 18.4 `TemplateViaSetupBinding` 必须形成真实消费闭环

`TemplateViaSetupBinding` 不能只停留在：

- SFC semantic model 里“标记了一下”
- `<script setup>` 里声明了 `__jazorVueSfcBindingN`
- `<template>` 仍然直接输出原始 `ExpressionText`

这会造成两个问题：

1. single-evaluation 约束没有真正生效；
2. script/template 两侧的语义来源开始漂移，后续很难验证 parity。

因此，新的约束必须固定为：

- 所有被判定为 `TemplateViaSetupBinding` 且位于组件级 template scope 的表达式，
  - 必须先 materialize 为 `<script setup>` 中的稳定 binding；
  - 再由 `<template>` 使用该 binding 名称，而不是回退为原始表达式文本。
- 对于需要保持响应式的组件级 lifted expression，优先 materialize 为 `computed(() => expr)`。
- `v-if` / `v-for` / 顶层 interpolation / 顶层 attribute binding / 非局部 slot value expression，必须共用同一套提升顺序与模板消费顺序，避免 template/script 漂移。

### 18.5 模板局部作用域不能错误提升到 `<script setup>`

并非所有 `TemplateViaSetupBinding` 都能安全进入组件级 `<script setup>`。

以下名字属于 template-local scope：

- `v-for` 引入的迭代变量；
- typed slot / named slot template 引入的 slot 参数；
- 后续若支持的其它 template-local alias。

这些表达式即使在 canonical 层被分类为 `TemplateViaSetupBinding`，也不能机械提升到组件级 binding。否则会生成：

- 在 `<script setup>` 中引用未定义模板局部名的非法代码；
- 或者把本应逐项/逐 slot 求值的表达式错误外提。

因此 SFC semantic model 必须具备最小 scope awareness：

- 组件级 scope 的 `TemplateViaSetupBinding` 才允许进入 lifted binding 集合；
- template-local scope 内的表达式，要么保持 template 直出，
- 要么在未来进入更细粒度的 template helper / slot helper contract，
- 但不能偷渡成组件级 `<script setup>` binding。

当前 Phase 2 收口的最低验收线是：

1. control-flow 的 lifted binding 在 template 中已被真实消费；
2. 顶层 setup field/method 调用插值可通过 lifted computed binding 保持响应式闭环；
3. `v-for` 体内的 `item` 等 template-local 名称不会被错误提升到 `<script setup>`。

### 18.6 模板消费不能依赖“遍历顺序偶然一致”

当 lifted binding 数量继续增加到：

- interpolation
- `v-if`
- `v-for`
- attribute binding
- slot value expression
- slot outlet argument

如果 template 侧仍然靠“收集时的遍历顺序”和“发射时的遍历顺序”隐式对齐，就会出现两个问题：

1. 某一类节点新增后，旧节点的 binding 序号整体漂移；
2. semantic model 与 emitter 分别调整遍历逻辑时，问题只会在最终字符串阶段以偶发形式暴露。

因此，Phase 2 收口后的 contract 必须要求：

- SFC semantic model 为每一个 lifted template 消费位点生成显式 binding-site identity；
- template emitter 通过该 identity 查询 binding 名称，而不是线性 consume “下一个 binding”；
- 新增 template 节点类型时，必须同时补 binding-site 生成与消费回归。

这是 RazorVue SFC lane 的健壮性要求，不是单纯的实现偏好。

### 18.7 组件级 BuildRenderTree 局部变量当前必须 fail-fast

Razor `BuildRenderTree` 中的局部变量并不天然等价于 `<script setup>` 的组件级 binding。

示例：

- `var localTitle = Title; builder.AddContent(..., localTitle);`

如果把这类表达式静默提升到 `<script setup>`：

- 可能直接引用一个在 SFC 中根本不存在的组件局部名；
- 也可能把本应位于模板控制流内部的值错误外提；
- 最终生成“能编译字符串、不能正确消费”的非法 `.vue`。

在没有建立更完整的 template-local alias / helper contract 之前，当前规则应保持保守：

- 对组件级 `LocalReference` 且需要 `TemplateViaSetupBinding` 的场景，design-time 直接报 `UnsupportedTemplateEncoding`；
- 不允许为了“先生成点什么”而把这类表达式偷渡进 `<script setup>`。

后续如果要支持这类 authoring，必须显式设计：

- 哪些局部变量可等价映射为 setup binding；
- 哪些只能保留在 template-local scope；
- 它们与 single-evaluation / lifecycle / slot-scope 的交互规则。

### 18.8 typed slot outlet argument 也是 Phase 2 canonical contract 的一部分

typed slot contract 不能只覆盖：

- 组件作为 child component 时，把 `RenderFragment<T>` 作为 slot attribute 传给下游组件；

还必须覆盖当前组件自身的 slot outlet 读取场景，即：

- `builder.AddContent(sequence, SomeRenderFragmentOfT, argument)`

这一形态在 canonical/SFC lane 中必须收口为：

- `RazorVueCanonicalSlotOutletNode`
- `ArgumentExpressionText`
- 若需要 single-evaluation / component-scope lifting，则走与普通 template binding 相同的 binding-site contract
- 最终在 SFC template 中生成 `<slot ... :value="...">`

禁止继续把这条路径留在“render module 能表达，但 SFC 还没承接”的半完成状态。

### 18.9 child component 的 callable scoped slot forwarding 也必须显式收口

legacy lane 已经证明过一个关键 authoring 语义：

- 当子组件声明 `RenderFragment<T>` slot parameter，
- 且父组件把自己接收到的 callable slot 原样传给子组件，
- 例如 `builder.AddAttribute(1, "ItemTemplate", ItemTemplate)`，
- lowering 语义不是“传一个普通值”，而是“保留一个可调用 slot forwarding contract”。

legacy render 车道当前的等价语义形状是：

- `itemTemplate: (context) => props.itemTemplate(context)`

SFC lane 不能把这一形态退化为：

- 普通插值文本；
- 非调用 slot 值；
- 或“看起来像 slot template，但实际上没有把 context 继续传下去”的近似输出。

因此，Phase 2 下游 contract 必须再区分三种 typed slot 路径：

1. 当前组件消费自身 slot outlet
2. 父组件把 callable `RenderFragment<T>` forwarding 给子组件
3. 非 callable 值被错误传给 `RenderFragment<T>` 参数

对应约束必须固定为：

- canonical model 不能只用 `ParameterName + ValueExpressionText` 含糊覆盖 callable forwarding；它必须能表达“这是一个需要保留调用语义的 scoped slot forwarding”。
- SFC semantic model 不能把 callable forwarding 当成普通 `slot value expression` 去做插值处理。
- SFC template emission 必须保留声明的 slot 参数名，并在 slot render site 精确转发一次调用语义。
- 这个 slot 参数名属于 template-local scope，不能被错误提升到组件级 `<script setup>` binding。
- 对非 callable 值传给 `RenderFragment<T>` 的场景，SFC lane 至少必须保证“不生成 invoked scoped slot 形状”；若不存在无损 SFC 投影，应 design-time fail-fast，而不是拼出近似模板。

下一组 focused regression 必须直接镜像 legacy 已有语义：

1. nested component typed slot forwarding
2. inherited typed slot forwarding
3. non-callable negative case

因此，后续实现顺序应调整为：

1. 继续把 legacy 与 SFC 共享的 lowering seam 固定下来
2. 先补 child component callable scoped slot forwarding 的 canonical/SFC contract 与回归
3. 扩 setup/lifecycle 支持面时优先扩共享 support，而不是只补 SFC 分支
4. 所有新增 SFC authoring 能力都必须同时验证：
   - generated `.vue` 文本可消费
   - artifact `Imports` / manifest metadata 与文本一致
   - unsupported 场景仍然 design-time fail-fast

## 17. 参考

- [ECMAScript.Vue3 Authoring 落地计划](./ECMAScript.Vue3.Authoring.ImplementationPlan.md)
- [ECMAScript.Vue3 剩余完善清单](./ECMAScript.Vue3.RemainingWorkChecklist.md)
- [ECMAScript.Vue3 当前状态](../../03-完成/ecmascript.vue3/status.md)
- [ECMAScript.Vue3 README](../../../src/ECMAScript.Vue3/README.md)
- [ECMAScript.Vue3 模块映射规则](../../01-目标/ecmascript.vue3/vue3-module-mapping-rules.md)
- [ECMAScript.Vue3 映射细节设计](../../01-目标/ecmascript.vue3/vue3-mapping-details.md)
- [RazorVue 设计总览](../../01-目标/razorvue/design/RazorVue.Design.md)
- [RazorVue 项目职责](../../01-目标/razorvue/design/RazorVue.ProjectResponsibilities.md)
- [RazorVue 渲染树设计](../../01-目标/razorvue/render-tree/RenderTree.md)
