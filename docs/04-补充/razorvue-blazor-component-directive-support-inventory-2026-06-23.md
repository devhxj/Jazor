# RazorVue Blazor Component 指令特性支持盘点（2026-06-23）

本文从 Blazor component 官方指令特性视角盘点 RazorVue 的支持状态。它补充的是“指令层横向视图”，不替代领域级矩阵：

- `docs/04-补充/razorvue-support-matrix-2026-06-17.md` - 按 RazorVue 领域给出支持、降级和 fail-fast 边界。
- `src/Jazor.RazorVue/README.md` - authoring 合同、lowering 规则和运行时边界。
- `docs/03-完成/razorvue/completion-analysis.md` - 生产就绪状态与 consumer 链路评审。

## 来源与统计口径

官方指令分类参考 Microsoft Learn 的 Blazor component 文档：

- Razor components: <https://learn.microsoft.com/en-us/aspnet/core/blazor/components/?view=aspnetcore-10.0>
- Blazor data binding: <https://learn.microsoft.com/en-us/aspnet/core/blazor/components/data-binding?view=aspnetcore-10.0>
- Blazor event handling: <https://learn.microsoft.com/en-us/aspnet/core/blazor/components/event-handling?view=aspnetcore-10.0>
- Blazor splat attributes: <https://learn.microsoft.com/en-us/aspnet/core/blazor/components/splat-attributes-and-arbitrary-parameters?view=aspnetcore-10.0>
- Blazor generic type support: <https://learn.microsoft.com/en-us/aspnet/core/blazor/components/generic-type-support?view=aspnetcore-10.0>
- Blazor lifecycle: <https://learn.microsoft.com/en-us/aspnet/core/blazor/components/lifecycle?view=aspnetcore-10.0>
- Blazor element/component relationships: <https://learn.microsoft.com/en-us/aspnet/core/blazor/components/element-component-model-relationships?view=aspnetcore-10.0>

统计采用 feature family 粒度，而不是逐语法 token 粒度。例如 `@onclick`、`@onchange` 等归为 DOM event directive family；`@bind:get`、`@bind:set`、`@bind:after`、`@bind:event`、`@bind:format` 归入 `@bind` family 的扩展面。

RazorVue 的目标域是 Blazor `.razor` component authoring 到 `.vue` artifact 生成。Razor MVC / Razor Pages 不属于盘点范围，也不进入完成度分母。Blazor host/runtime-only 语义如果无法诚实表达为 `.vue` artifact，则单独记录为“不纳入完成度分母的边界”。

## 完成度统计

| 统计域 | 完整支持 | 受控 / 部分支持 | 目标域内不支持 | 加权完成度 |
|--------|----------|-----------------|----------------|------------|
| Blazor component authoring 目标域（22 项） | 15 | 7 | 0 | 约 85% |

加权规则：完整支持计 1，受控 / 部分支持计 0.5，目标域内不支持计 0。Blazor host/runtime-only 指令、Razor MVC、Razor Pages 均不计入该完成度。

工程判断：

- RazorVue 已具备较高的 Blazor component authoring 可用性，尤其是 template、component parameter、slot、route、event、`@key`、`@attributes`、element `@ref`、受控 lifecycle / setup 逻辑。
- 剩余扩展空间集中在 `@bind` advanced modifier 的更宽支持面；directive metadata 与 host/runtime-only 指令边界已形成可测试合同。
- RazorVue 不应被描述成 Blazor runtime clone。它是 `.vue` artifact producer，宿主语义必须转成 Vue artifact 能诚实表达的形状。

## Blazor Component Authoring 目标域矩阵

| 特性 / 指令 family | 状态 | 说明 | 代表证据 |
|--------------------|------|------|----------|
| Static markup、HTML attribute、Razor interpolation、表达式 | 支持 | 静态 template 与表达式 lowering 已进入主线。 | `src/Jazor.RazorVue/README.md`、`BuildRenderTreeTemplateFrontendTests`、`RazorVueRazorIrTemplateFrontendTests` |
| `@if` / `@else` / `@else if` | 支持 | template-safe 条件可回流 Vue template；复杂形态按既有边界进入 render-function 或 fail-fast。 | `docs/04-补充/razorvue-support-matrix-2026-06-17.md` 第 1 节 |
| `@foreach` / `@for` | 支持 | 同步 loop 支持；count-style `for` 有可归一子集；副作用 step 等进入保守边界。 | `RazorVue_Pipeline_WithDynamicAddAssignStep_*`、support matrix 第 1 节 |
| `@switch` | 支持 | 常量、单值、多 label、受控 pattern-local condition-only 子集已支持。 | `RazorVue_SfcArtifactFactory_WithSimpleConstantSwitch*`、support matrix 第 1 节 |
| Razor code block / local declaration / template-scoped local | 支持 | 受控本地声明、一次赋值 alias、template-scoped local carrier 已锁定。 | `src/Jazor.RazorVue/README.md` template-scoped local 段 |
| `@code` / `@functions` / code-behind partial | 支持 | 组件 C# surface 继续由官方 Razor SG / Roslyn 建模，RazorVue 读取语义快照。 | `src/Jazor.RazorVue/README.md` lifecycle / setup 段 |
| `@using` / `_Imports.razor` | 支持 | namespace import 和 component resolution 经官方 Razor / Roslyn 路径进入快照。 | `RazorVueRazorIrTemplateFrontendTests.CreateRenderTree_ForAtKeyAttributes_*` 使用 `importsText` |
| `@namespace` | 支持（透明） | 由 Razor SDK / Roslyn 负责生成类型命名空间；RazorVue 不另建命名空间语义。 | Razor SG carrier / aligned context 测试 |
| `@page` | 支持 | Component route templates 进入 artifact route metadata；consumer route 转换使用 ASP.NET Core `TemplateParser` 并保守映射到 Vue Router。 | `docs/03-完成/razorvue/completion-analysis.md` route 段、`RazorVueGeneratorRouteTests`、`RazorVueConsumerEntryCompilerTests` |
| `@attributes` | 支持 | HTML splat / component arbitrary attributes 进入 descriptor-aware attribute spread；无 sink 时 fail-fast。 | `RazorVueRazorIrTemplateFrontendTests`、`RazorVueCanonicalHModelFactory` arbitrary attributes 诊断 |
| `@key` | 支持 | HTML element 和 component `@key` 均恢复 C# 表达式语义。 | `RazorVueRazorIrTemplateFrontendTests.CreateRenderTree_ForAtKeyAttributes_ResolvesLiteralAndExpressionKeys` |
| DOM event directives，例如 `@onclick` | 支持 | 从 Razor SDK 生成调用或 raw markup fallback 恢复 `IOperation`，交给 EventCallback / method-reference lowering。 | `RazorVueRazorIrTemplateFrontendTests.CreateRenderTree_ForElementDomEventWithModifiers_PreservesModifierMetadata` |
| DOM event modifiers `:preventDefault` / `:stopPropagation` | 支持 | 常量和动态 modifier 已支持；常量 false 在 template 路径中编译期擦除。 | `RazorVueSfcArtifactFactory_WithRazorIrTemplateFrontend_LowersElementDomEventWithModifiers`、cleared modifier 测试 |
| Component `@bind-Value` | 支持 | 基于 `Value` / `ValueChanged` descriptor-aware bind pair 生成 value 和 callback。 | `CreateRenderTree_ForComponentBindAttribute_ProducesValueAndValueChangedAttributes` |
| RenderFragment / ChildContent / named slot / typed slot | 支持 | 默认 slot、命名 slot、typed scoped slot、source-stable fragment carrier 和受控 factory 已支持。 | `RazorVueRazorIrFragmentSlotCarrierBoundaryTests`、`RazorVue_SfcArtifactFactory_LowersInlineNamedSlotTemplate_*` |
| Lifecycle / setup methods | 受控支持 | `ShouldRender`、`SetParametersAsync`、`OnInitialized*`、`OnParametersSet*`、`OnAfterRender*` 等只接受明确受控子集。 | `src/Jazor.RazorVue/README.md` lifecycle 段、`RazorVueRazorIrLifecycleBoundaryTests` |
| `@bind` family 扩展 | 受控支持 | Component bind 已稳定；HTML element string value-style `@bind` 已从 raw directive attribute 收口为结构化 `value` + DOM event callback，默认 `onchange`，并支持 `input`、`textarea`、`select` 与 `@bind:event="oninput"`；checkbox / radio / file、非 string target、`@bind:format`、`@bind:get` / `@bind:set` / `@bind:after` 已 fail-fast 或停在官方 Razor SG 边界。 | `CreateRenderTree_ForElementBind_LowersToValueAndChangeHandler`、`RazorVuePipeline_WithRazorIrTemplateFrontend_ForTextareaElementBind_LowersToValueAndChangeHandler`、`RazorVuePipeline_WithRazorIrTemplateFrontend_ForSelectElementBind_LowersToValueAndChangeHandler`、`RazorVuePipeline_WithRazorIrTemplateFrontend_ForElementBindEventOnInput_LowersToInputHandler`、`RazorVuePipeline_WithRazorIrTemplateFrontend_ForElementBindFormat_ReportsUnsupportedDirective`、`CreateRenderTree_ForComponentBindFormatAttribute_RemainsOfficialRazorSgCompileBoundary` |
| `@ref` | 受控支持 | HTML element `@ref` lowering 为 Vue template ref；component `@ref` 明确 unsupported，因为 Vue component public instance ref 不能保持 Blazor component instance 语义。 | `CreateRenderTree_ForElementRefCapture_ProducesElementReferenceCapture`、`CreateRenderTree_ForComponentRefCapture_ThrowsCanonicalizationFailed`、`RazorVueRazorIrTemplateFrontend.cs` component ref error |
| `@inherits` | 受控支持 | C# 继承链参与语义快照；源码可分析 base parameter / setup member / lifecycle 子集可进入 descriptor、render 和 lifecycle lowering；外部无源码 override 或超出 lifecycle 合同的形态保持 FullReloadRequired / fail-fast。 | `RazorVueRazorIrMetadataDirectiveTests`、`src/Jazor.RazorVue/README.md` base-pass-through 与 setup base member 段 |
| `@implements` | 受控支持 | 作为 C# compile-time contract 保留；不会自动生成 Vue runtime interface shim，也不会把接口名泄露到 `.vue` artifact。 | `RazorVueRazorIrMetadataDirectiveTests`、编译器 interface-as-contract 约定 |
| `@attribute` | 受控支持 | 普通 C# metadata 可保留；RazorVue 只解释 artifact-relevant attribute，例如 route、module/import、parameter、library descriptor、diagnostic 相关形态。RazorVue 不解释的合法 C# attribute 透明编译，不产生 artifact 行为。 | `RazorVueRazorIrMetadataDirectiveTests`、`RazorVueDescriptorExtractionTests` |
| `@typeparam` / generic component authoring | 受控支持 | 泛型 C# 类型可参与官方 Razor SG / Roslyn 编译。RazorVue descriptor 与 component resolution 使用开放泛型形状，例如 `GenericList<TItem>`；泛型实参默认只是 Vue artifact 的 compile-time annotation。`RenderFragment<T>` slot descriptor 与 typed slot context 可保留类型形状并由 Roslyn 绑定闭合成员访问；runtime `typeof(T)`、`default(T)`、`new T()`、`is T` 等 CLR 泛型运行时语义 fail-fast。 | `RazorVueRazorIrGenericComponentDirectiveTests`、generic helper / component type carrier 边界测试 |

## 不纳入完成度分母的 Blazor Host/Runtime 指令

| 指令 / feature | 状态 | 原因 / 推荐替代 |
|----------------|------|-----------------|
| Blazor DI `@inject` | 非目标 | 已测试为不产生 `.vue` artifact 语义。RazorVue 使用 Vue / host-facing inject、`[CascadingParameter]` / `CascadingValue` 等 artifact 可表达机制；不模拟 ASP.NET Core DI scope。 |
| `@layout` | 非目标 | 已测试为不产生 `.vue` artifact 语义。`.vue` artifact 中使用显式 Vue layout component composition。 |
| `@rendermode` | 非目标 | 已测试为不产生 `.vue` artifact 语义。Blazor SSR / interactive render mode 是 ASP.NET Core host runtime 语义，不属于 `.vue` artifact 合同。 |
| `@formname` | 非目标 | raw directive attribute 形态在 RazorVue 层 fail-fast，并提示 host/runtime-only 替代方向。Blazor SSR form post pipeline 不属于 RazorVue。 |
| Async render path：`await` / `await foreach` / `await using` in render | 不支持 | 不生成 fire-and-forget async render；`.razor` 形态可停在官方 Razor SG 编译错误或 RazorVue fail-fast。 |
| Component `@ref` | 不支持 | Vue component ref 与 Blazor component instance ref 语义不等价，已明确 fail-fast。 |

## 明确排除：Razor MVC / Razor Pages

RazorVue 不盘点 Razor MVC / Razor Pages 指令，不为 `.cshtml` view model、section、Tag Helper 或 page handler 语义建立兼容层。以下项目不进入 RazorVue 完成度分母，也不进入开发计划：

- MVC / Razor Pages `@model`
- MVC / Razor Pages `@section`
- MVC Tag Helper 指令：`@addTagHelper`、`@removeTagHelper`、`@tagHelperPrefix`
- Razor Pages host `@page` handler semantics

## 后续扩展项

| 项目 | 影响 | 优先级 |
|------|------|--------|
| `@bind:get` / `@bind:set` / `@bind:after` 等 advanced modifier 的真实 artifact lowering | 中 | P3 |
| checkbox / radio 等非 string HTML element bind 的类型化支持 | 中 | P3 |

## 维护要求

- 任一指令行状态变化时，同步更新本文和 `razorvue-support-matrix-2026-06-17.md` 的对应领域行。
- 新增支持必须补 Razor IR frontend 测试和 SFC artifact / pipeline 验证，不能只验证 handwritten `BuildRenderTree` 路线。
- 指令状态 smoke coverage 由 `src/Jazor.RazorVue.RazorIr.Test/RazorVueRazorDirectiveSupportMatrixTests.cs` 维护，至少覆盖支持、受控支持、非目标、不支持四类结果。
- 涉及 C# 表达式、member、import、type mapping 的 lowering 必须流经 `Jazor.Compiler` / `SemanticWalker` translation hooks。
- Blazor host/runtime-only 语义应 fail-fast 或明确文档化为不适用，不能静默生成貌似可用但语义漂移的 `.vue` artifact。
