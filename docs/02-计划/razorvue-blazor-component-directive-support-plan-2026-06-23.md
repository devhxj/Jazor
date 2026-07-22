# RazorVue Blazor Component 指令支持开发计划（2026-06-23）

## 目标

把 RazorVue 的 Blazor component directive support 从“高覆盖但分散在领域矩阵和测试名里”收口成可维护的工程合同：

- 补齐高频 component authoring 缺口，优先是 HTML element `@bind` family。
- 将 `@typeparam`、`@attribute`、`@implements`、`@inherits` 等 metadata / type-system 指令的边界写清楚并用测试锁住。
- 对不能诚实表达为 `.vue` artifact 的 Blazor host/runtime-only 指令给出一致 fail-fast 或文档化不适用结论。
- 保持 RazorVue 是 `.vue` artifact producer，不引入 wrapper-JS marker protocol，不绕过 `Jazor.Compiler` / `SemanticWalker` 翻译 C# 语义。

## 输入材料

- `docs/04-补充/razorvue-blazor-component-directive-support-inventory-2026-06-23.md`
- `docs/04-补充/razorvue-support-matrix-2026-06-17.md`
- `src/Jazor.RazorVue/README.md`
- `src/Jazor.RazorVue.RazorIr.Test/RazorVueRazorIrTemplateFrontendTests.cs`
- `src/Jazor.RazorVue/RazorSdk/RazorVueRazorIrTemplateFrontend.cs`

## 阶段 0：基线与可回归矩阵

**目标：** 先把官方指令 family、RazorVue 支持状态、预期 diagnostic 变成可测试资产。

### Task 0.1：新增 directive support matrix 测试类

**Acceptance criteria:**

- [x] `src/Jazor.RazorVue.RazorIr.Test` 新增 `RazorVueRazorDirectiveSupportMatrixTests` 或等价测试类。
- [x] 每个 inventory 状态类别至少有一个“支持 / 受控支持 / 非目标 / 不支持”的 smoke fixture。
- [x] 非目标指令不以“编译失败就算覆盖”为唯一证据，能在 RazorVue 层给出可读边界时必须断言 diagnostic。

**Verification:**

- [x] `dotnet test src/Jazor.RazorVue.RazorIr.Test/Jazor.RazorVue.RazorIr.Test.csproj --filter "FullyQualifiedName~RazorDirectiveSupportMatrix" -v minimal`

**Status 2026-06-23:** `RazorVueRazorDirectiveSupportMatrixTests` 已建立支持、受控支持、非目标、不支持四类 smoke coverage。`@layout` / `@inject` 透明通过但不产生 Vue artifact 语义，raw `@formname` 类 host/runtime-only directive attribute 在 RazorVue 层 fail-fast，async render 停在官方 Razor SG `CS4033` 边界。

**Likely files:**

- `src/Jazor.RazorVue.RazorIr.Test/RazorVueRazorDirectiveSupportMatrixTests.cs`
- `docs/04-补充/razorvue-blazor-component-directive-support-inventory-2026-06-23.md`

### Task 0.2：建立文档同步检查

**Acceptance criteria:**

- [x] support inventory 与 support matrix 中的状态枚举一致：支持、受控支持、非目标、不支持。
- [x] 文档明确完成度只统计 Blazor component authoring 目标域，不包含 Razor MVC / Razor Pages。
- [x] `docs/04-补充/README.md` 和 `docs/02-计划/README.md` 均包含新文档索引。

**Verification:**

- [x] `rg -n "razorvue-blazor-component-directive-support" docs`
- [x] 对 inventory / plan 文档执行相对时间关键词扫描，结果只允许为空。

## 阶段 1：`@bind` family 收口

**目标：** 把最影响日常组件 authoring 的 binding 支持从“component bind 已稳定、element bind 已进入 value-style 子集”继续收口到完整、可诊断的工程合同。

### Task 1.1：HTML element `@bind` 结构化采集

**Acceptance criteria:**

- [x] Razor IR frontend 能从官方 Razor SDK 输出或生成源码中恢复 `<input @bind="Title" />` 的 value expression 与 update expression。
- [x] 不通过手写 JS 字符串拼接更新 C# member；表达式读取与写入必须通过现有 Roslyn / SemanticWalker 路径可验证地建模。
- [x] 2026-06-23 观察到的 raw `@bind` attribute 行为被替换为结构化 node 或明确中间 carrier。

**Verification:**

- [x] `dotnet test src/Jazor.RazorVue.RazorIr.Test/Jazor.RazorVue.RazorIr.Test.csproj --filter "FullyQualifiedName~ElementBind" -v minimal`

**Status 2026-06-23:** 已完成普通 string value-style HTML element `@bind` 结构化采集。2026-06-23 支持 `<input @bind="Title" />`，并在 artifact 中生成 `value` prop 与 `onChange` handler。更新表达式通过 `EventCallback.Factory.CreateBinder(...)` 的 Roslyn `IOperation` 建模，再由 existing emitter 归一化为 Vue emit/update callback；不是在 RazorVue frontend 内直接拼接 member assignment JS。

**Likely files:**

- `src/Jazor.RazorVue/RazorSdk/RazorVueRazorIrTemplateFrontend.cs`
- `src/Jazor.RazorVue.RazorIr.Test/RazorVueRazorIrTemplateFrontendTests.cs`
- `src/Jazor.RazorVue/Canonical/RazorVueCanonicalHModelFactory.cs`

### Task 1.2：HTML element bind lowering

**Acceptance criteria:**

- [x] `<input @bind="Value" />` lower 成 Vue artifact 可表达的 `value` + update handler，保留 C# evaluation order。
- [x] `textarea`、`select`、checkbox / boolean input 至少有明确支持矩阵：支持的形态生成 artifact，不支持的形态 fail-fast。
- [x] 支持输出覆盖 2026-06-23 Razor IR 路径的 render-function fallback mode。

**Verification:**

- [x] `dotnet test src/Jazor.RazorVue.RazorIr.Test/Jazor.RazorVue.RazorIr.Test.csproj --filter "FullyQualifiedName~ElementBind" -v minimal`
- [x] `dotnet test src/Jazor.RazorVue.Test/Jazor.RazorVue.Test.csproj --filter "FullyQualifiedName~Bind" -v minimal`

**Status 2026-06-23:** Phase 1 已完成 string value-style `input`、`textarea`、`select`。checkbox / radio / file、非 string target 均已 RazorVue fail-fast。`ElementBind` focused suite 13/13 通过，`RazorVue.RazorIr` `Bind` focused suite 21/21 通过，`Jazor.RazorVue.Test` `Bind` focused suite 43/43 通过。

**Likely files:**

- `src/Jazor.RazorVue/Canonical/RazorVueCanonicalHModelFactory.cs`
- `src/Jazor.RazorVue/Lowering/RazorVueSfcArtifactFactory.cs`
- `src/Jazor.RazorVue.Test/RazorVueSfcArtifactFactoryTests.cs`

### Task 1.3：Advanced bind modifier 边界

**Acceptance criteria:**

- [x] `@bind:event` 支持至少一个高频 DOM event，例如 `oninput`。
- [x] `@bind:get` / `@bind:set` / `@bind:after` 有明确设计：支持或 fail-fast，不能保留 raw directive attribute。
- [x] `@bind:format` / culture 在 component 和 element 两条路径都有统一结论；未实现前给出官方 Razor SG boundary 或 RazorVue diagnostic。

**Verification:**

- [x] `dotnet test src/Jazor.RazorVue.RazorIr.Test/Jazor.RazorVue.RazorIr.Test.csproj --filter "FullyQualifiedName~ElementBind" -v minimal`
- [x] `dotnet test src/Jazor.RazorVue.RazorIr.Test/Jazor.RazorVue.RazorIr.Test.csproj --filter "FullyQualifiedName~Bind" -v minimal`

**Status 2026-06-23:** `@bind:event="oninput"` 已支持。孤立 `@bind:event`、element `@bind:format`、`@bind:get` / `@bind:set` / `@bind:after` 均已 fail-fast，不保留 raw directive attribute。

## 阶段 2：`@typeparam` 与 generic component 合同

**目标：** 让 generic component authoring 的支持状态从“可由 C# 编译但合同不稳定”变成明确的产品边界。

### Task 2.1：定义 generic component artifact contract

**Acceptance criteria:**

- [x] 文档说明 generic arguments 在 RazorVue 中何时只是 compile-time annotation，何时会影响 descriptor / slot / parameter shape。
- [x] 禁止把 runtime `typeof(T)`、`default(T)`、`new T()`、`is T` 等 CLR generic runtime 语义偷渡进 Vue artifact。
- [x] 合同同时满足官方 Razor SG 生成 `.razor.g.cs` 能编译，以及 RazorVue lowering 能识别组件 descriptor。

**Verification:**

- [x] `dotnet test src/Jazor.RazorVue.RazorIr.Test/Jazor.RazorVue.RazorIr.Test.csproj --filter "FullyQualifiedName~Generic" -v minimal`

**Status 2026-06-23:** Phase 2 generic component artifact contract 已落地。`@typeparam` 组件可与官方 Razor SG 生成 `.razor.g.cs` 对齐；Razor SG document map 会识别泛型生成类名。RazorVue component node 的 `ComponentName` / resolution key 使用开放组件名，例如 `GenericList`，`ComponentFullName` / descriptor 保留开放泛型形状，例如 `Demo.Pages.GenericList<TItem>`。泛型参数在 Vue artifact 中默认是 compile-time annotation；`RenderFragment<T>` descriptor、prop/slot type name 可保留 `TItem` 这种类型形状，但不生成 runtime generic metadata。

**Likely files:**

- `src/Jazor.RazorVue/Descriptor/VueComponentDescriptorFactory.cs`
- `src/Jazor.RazorVue.RazorIr.Test/RazorVueRazorIrTemplateFrontendTests.cs`
- `docs/04-补充/razorvue-blazor-component-directive-support-inventory-2026-06-23.md`
- `src/Jazor.RazorVue/RazorSdk/RazorVueRazorDocumentSemanticFrontend.cs`
- `src/Jazor.RazorVue/RazorSdk/RazorVueRazorIrTemplateFrontend.cs`
- `src/Jazor.RazorVue.RazorIr.Test/RazorVueRazorIrGenericComponentDirectiveTests.cs`

### Task 2.2：typed slot / generic parameter 回归

**Acceptance criteria:**

- [x] `RenderFragment<T>`、generic child component、typed slot context 三类场景都有成功 fixture。
- [x] generic component 不尝试生成 runtime closed descriptor；descriptor 使用开放泛型合同，typed slot context 的闭合成员语义来自官方 Razor SG / Roslyn `IOperation`。
- [x] runtime generic type-parameter semantics fail-fast，diagnostic 指向 component 和 type parameter。

**Verification:**

- [x] `dotnet test src/Jazor.RazorVue.RazorIr.Test/Jazor.RazorVue.RazorIr.Test.csproj --filter "FullyQualifiedName~TypedSlot|FullyQualifiedName~Generic" -v minimal`

**Status 2026-06-23:** 新增 `RazorVueRazorIrGenericComponentDirectiveTests` 覆盖 current generic component `@typeparam`、generic child component + typed slot、generic descriptor shape、runtime `typeof(TValue)` fail-fast。`Generic` focused suite 6/6 通过，`TypedSlot|Generic` focused suite 9/9 通过。

## 阶段 3：metadata 指令语义与诊断

**目标：** 将 `@attribute`、`@implements`、`@inherits` 从“依赖 C# 编译自然存在”推进到用户可读的 RazorVue 支持合同。

### Task 3.1：`@attribute` artifact-relevant 白名单

**Acceptance criteria:**

- [x] 支持文档列出 RazorVue 会解释的 attribute 类型，例如 module、parameter、descriptor、diagnostic 相关 attribute。
- [x] 对 RazorVue 不解释但 C# 合法的 attribute，保持透明编译，不误导成 artifact 行为。
- [x] 对看起来要求 host runtime 行为的 attribute，给出 diagnostic 或文档化 no-op。

**Verification:**

- [x] `dotnet test src/Jazor.RazorVue.RazorIr.Test/Jazor.RazorVue.RazorIr.Test.csproj --filter "FullyQualifiedName~MetadataDirective" -v minimal`

**Status 2026-06-23:** 新增 `RazorVueRazorIrMetadataDirectiveTests`，锁定 `@attribute [Route(...)]` 进入 route metadata、普通合法 C# attribute 透明保留在 Roslyn metadata 但不进入 `.vue` artifact。README / support inventory 已列出 RazorVue 解释的 artifact-relevant attribute surface。

### Task 3.2：`@inherits` / `@implements` 边界测试

**Acceptance criteria:**

- [x] `@inherits` 对源码可分析 base lifecycle / setup member 的支持有正向测试。
- [x] 外部无源码 base override、动态 lifecycle、不可分析 interface runtime check 有 fail-fast 或 FullReloadRequired 测试。
- [x] `@implements` 明确为 compile-time contract，不生成 Vue runtime interface shim。

**Verification:**

- [x] `dotnet test src/Jazor.RazorVue.RazorIr.Test/Jazor.RazorVue.RazorIr.Test.csproj --filter "FullyQualifiedName~MetadataDirective" -v minimal`

**Status 2026-06-23:** `@inherits` 已用源码可分析 base `[Parameter]` + `ShouldRender` fixture 锁定 descriptor/render/lifecycle lowering；外部无源码 base override 与动态 lifecycle 继续由既有 lifecycle/base boundary 回归覆盖为 FullReloadRequired / fail-fast。`@implements` 已锁定为 Roslyn-visible compile-time contract，不生成 Vue runtime interface shim。

## 阶段 4：Blazor Host/Runtime 指令边界

**目标：** 对 `.vue` artifact 无法诚实表达的 Blazor host/runtime-only 语义给出统一、可理解的反馈。

### Task 4.1：Blazor host/runtime-only 指令 diagnostic catalog

**Acceptance criteria:**

- [x] `@inject`、`@layout`、`@rendermode`、`@formname` 有明确状态：官方 Razor SG boundary、RazorVue diagnostic 或文档化不适用。
- [x] diagnostic 文案说明替代路径，例如 Vue component composition、Vue / host inject、slot composition。
- [x] 不生成 silent no-op artifact，除非文档明确该 directive 对 `.vue` artifact 没有可观察语义。
- [x] Razor MVC / Razor Pages 指令不进入此计划；若用户输入 `.cshtml` 语义，应明确拒绝为 RazorVue 目标外。

**Verification:**

- [x] `dotnet test src/Jazor.RazorVue.RazorIr.Test/Jazor.RazorVue.RazorIr.Test.csproj --filter "FullyQualifiedName~DirectiveSupportMatrix" -v minimal`

**Status 2026-06-23:** `@layout` / `@inject` / `@rendermode` 经官方 Razor SG metadata 路径透明通过且不进入 `.vue` artifact；raw `@formname` 类 directive attribute 在 RazorVue 层 fail-fast，diagnostic 指向 `.vue artifact generation` 边界并提示 Vue composition、Vue/host inject 或 host-side form integration 替代方向。Razor MVC / Razor Pages 继续明确排除。

**Likely files:**

- `src/Jazor.RazorVue/RazorSdk/RazorVueRazorIrTemplateFrontend.cs`
- `src/Jazor.RazorVue/Diagnostics/*`
- `src/Jazor.RazorVue.RazorIr.Test/RazorVueRazorDirectiveSupportMatrixTests.cs`

## 阶段 5：文档、consumer smoke 与发布门槛

**目标：** 把实现结果回写到 support matrix，并证明 library-mode consumer 仍闭合。

### Task 5.1：文档同步

**Acceptance criteria:**

- [x] `razorvue-blazor-component-directive-support-inventory-2026-06-23.md` 的统计数与实现状态一致。
- [x] `razorvue-support-matrix-2026-06-17.md` 的 Bind / 事件 / metadata 行同步更新。
- [x] `src/Jazor.RazorVue/README.md` 只保留 authoring 合同和关键边界，不追加流水账。

**Verification:**

- [x] `rg -n "ElementBind|@bind|@typeparam|@attribute|@implements|@inherits" docs src/Jazor.RazorVue/README.md`

### Task 5.2：focused suite 与 consumer smoke

**Acceptance criteria:**

- [x] Razor IR directive matrix、RazorVue SFC artifact factory、pipeline bind 切片均通过。
- [x] 若改动影响 route / emit / package consumer，必须跑对应 `Jazor.EmitTest` RazorVue slice；本轮只补 directive matrix、metadata 边界、diagnostic 与文档，不改变 route / emit / package consumer 行为，因此该条件不适用。
- [x] 若改动影响 TodoList sample authoring，必须跑 sample pure Deno pipeline 对应测试。

**Verification:**

- [x] `dotnet test src/Jazor.RazorVue.RazorIr.Test/Jazor.RazorVue.RazorIr.Test.csproj --filter "FullyQualifiedName~DirectiveSupportMatrix|FullyQualifiedName~Bind|FullyQualifiedName~Generic|FullyQualifiedName~MetadataDirective" -v minimal`：34/34 passed。
- [x] `dotnet test src/Jazor.RazorVue.Test/Jazor.RazorVue.Test.csproj --filter "FullyQualifiedName~Bind|FullyQualifiedName~RazorVue_SfcArtifactFactory|FullyQualifiedName~RazorVue_Pipeline" -v minimal`：1178/1178 passed。
- [x] `dotnet test src/Jazor.EmitTest/Jazor.EmitTest.csproj --filter "FullyQualifiedName~RazorVue" -v minimal`：114/114 passed。

**Status 2026-06-23:** Phase 5 已完成当前 scope 的文档同步、focused suite 与 RazorVue consumer smoke 验收。

## 风险与约束

| 风险 | 影响 | 处理 |
|------|------|------|
| Razor SDK 对某些 directive 只暴露生成 C#，不暴露结构化 IR | 高 | 优先从官方生成调用恢复 Roslyn `IOperation`；必要时补 Razor IR host 配置，不手写 JS 语义。 |
| Element `@bind` setter 语义破坏 evaluation order | 高 | 将读取、转换、赋值建模为 compiler-owned lowering，增加 side-effect 顺序测试。 |
| `@typeparam` 被误实现成 runtime generic simulation | 中高 | 明确 generic argument 默认是 compile-time annotation；runtime-sensitive generic 语义保持 fail-fast。 |
| Blazor host/runtime-only 指令被用户误认为缺陷 | 中 | 统一 diagnostic 文案和文档解释，给出 Vue artifact 替代方案。 |
| 文档统计与测试漂移 | 中 | 阶段 0 的 directive matrix 测试作为每次状态更新的同步门槛。 |

## 执行顺序

1. 阶段 0 必须先做，避免 Phase 1-5 改动没有统一基线。
2. 阶段 1 是最高收益路径，完成后 Razor component 日常表单 authoring 覆盖率明显提升。
3. 阶段 2 和阶段 3 可并行，但都必须遵守 Razor SG binding 与 RazorVue `.vue` artifact 双边界。
4. 阶段 4 可与阶段 2 / 3 并行，由 diagnostic / docs 负责人推进；Razor MVC / Razor Pages 不纳入执行范围。
5. 阶段 5 在每个实现阶段结束时局部执行，最终再跑一次完整 focused suite。
