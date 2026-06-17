# RazorVue 支持矩阵（2026-06-17）

本文是 RazorVue library-mode 的**支持 / 不支持矩阵**，按领域给出当前支持面、保守降级条件和 fail-fast 边界，并指向佐证的测试名 / 文件。它与以下文档互为引用，不重复记录逐次修复日志：

- `docs/04-补充/razorvue-playground-support-gaps-2026-05-12.md` — 当前缺口状态页（fail-fast / 降级 / 已固化能力的动态边界）。
- `src/Jazor.RazorVue/README.md` — authoring 合同、lowering 规则、各领域支持说明。
- `docs/03-完成/razorvue/completion-analysis.md` — 完成度与生产就绪评审。

## 约定

- ✅ 支持：已进入正式支持面，有回归测试佐证。
- 🟡 保守降级：不能证明 template-safe 时降级为 render-function `.vue`，或按 HMR 边界降级。
- ❌ fail-fast：显式报 `JAZORVGA0xx` / `JAZORVUE0xx` Error 诊断，不静默擦除。
- 测试佐证列给出代表性测试名或测试类；完整覆盖以测试代码为准。

## 1. Razor 语法 / 模板

| 形态 | 状态 | 边界说明 | 测试佐证 |
|------|------|----------|----------|
| root-level 静态 markup / 插值 / 表达式 | ✅ | — | `BuildRenderTreeTemplateFrontendTests`、`RazorVueRazorIrTemplateFrontendTests.CreateRenderTree_ForMarkupAndInterpolation_*` |
| `@if` / `@else` / `@else if` | ✅ | — | `CreateRenderTree_ForIfAndForeach_*`、`RazorVue_SfcArtifactFactory_LowersConditionalStaticAddMarkupContent_*` |
| `@foreach` / `@for`（count-style 可归一） | ✅ | count-style `for` 仅接受可归一到 `__jazorVueForRange(...)` 的单 iterator 形态；多 iterator / 副作用 step 保守进入 imperative loop / render-function | `RazorVue_Pipeline_WithDynamicAddAssignStep_*`、count-style `for` 回归 |
| `@switch`（template-safe） | ✅ | 常量 / 单值 / 多 label / pattern-local condition-only + 显式 `break` | `RazorVue_SfcArtifactFactory_WithSimpleConstantSwitch*`、`WithGuardedConstantPatternSwitch*` |
| `@lock(this)` / 受控 readonly object gate | ✅ | template-safe 回流 | `WithReadonlyObjectGateLockStatement*` |
| `@using`（null/default）/ using declaration | ✅ | null/default `using` 回流；真实 disposable `using` 保守 render-function | `WithRootDefaultUsingDeclaration*`、`WithNullUsingDeclaration*` |
| `@try` / `@catch` / `@finally`（no-op / 空 recovery） | ✅ | no-op `try/finally`、空 recovery `try/catch/finally` 回流；catch payload / 真实 exception 保守 render-function | `WithRootTryFinally*`、`WithRootTryCatch*` |
| `goto` | ❌ | `Jazor.Compiler` 无等价 jump lowering；`goto case` / `goto default` 同样 fail-fast | `WithSimpleConstantSwitchGotoCase_ThrowsStructuralIssue`、async/jump 矩阵 |
| `await` / `await foreach` / `await using` | ❌ | 不生成 fire-and-forget async render；`.razor` 形态停在官方 Razor SG `CS4033` | `WithContext_ForRootTemplateCodeBlockWithAwaitExpression_FailsAtOfficialRazorSourceGeneratorCompilation`、async/jump 矩阵 |
| imperative body → canonical `<template>` 回流 | 🟡 | 仅接受已实现的 template-safe 窄子集；其它保守 render-function 或 fail-fast | 见 `razorvue-playground-support-gaps` “Render / Template” 当前缺口 |

## 2. 生命周期 / Setup 逻辑

| 形态 | 状态 | 边界说明 | 测试佐证 |
|------|------|----------|----------|
| 普通 lifecycle no-op helper（private 同步、无副作用实参、按值 / 受控只读 `in` / 受控 `params`） | ✅ | — | `RazorVueRazorIrLifecycleBoundaryTests`、`ESGeneratorTests` no-op helper 切片 |
| `SetParametersAsync` no-op / base pass-through | ✅ | — | `RazorVue_Pipeline_ClassifiesTemplateOnlyBoundaryForBaseOnlySetParametersAsync*` |
| `SetParametersAsync` 受控 emit-watch（序列 / 分支 / guard-return / `switch` / 无 pattern-local pattern switch / 含至少一次受支持 callback emit 的受控 loop 含 `await foreach` / `try/catch/finally` recovery-cleanup） | ✅ | lower 成 `watch(() => [props.x], async () => { ... })`，按源码顺序保留 emit 顺序 | `RazorVue_Pipeline_LowersBaseThen*SetParametersAsync*`（69 个）、`RazorVue_SfcArtifactFactory_LowersSetParametersAsync*`（4 个） |
| `SetParametersAsync` mutation / 非 emit loop / 声明 pattern-local 并让 case body 依赖 / 任意外部 invocation / 真实 exception payload | ❌ | `FullReloadBoundary` 或 fail-fast | `RazorVue_Pipeline_ClassifiesFullReloadBoundaryFor*SetParametersAsync*` |
| `ShouldRender` no-op / base pass-through / 单表达式 / 受控 control flow / 受控 delegate carrier / local function delegate identity-return / 同源条件分支 / 只读本地别名链 / 必返回嵌套 block alias-return / 同源 `switch` / trailing-return switch / 同源 `try/catch` identity-return / 同步异常分支 / 纯同步 `throw` 终止 | ✅ | lower 成 cached render gate | `RazorVue_Pipeline_Lowers*ShouldRenderConditionIntoCachedRenderGate`（95 个）、`RazorVue_SfcArtifactFactory_With*ShouldRender_*`（7 个） |
| `ShouldRender` `await foreach` / mutation / 任意 delegate escape / 跨 member / 外部 callable 传参返回 / 外部引用程序集无源码 base override | ❌ | `FullReloadRequired` 或 fail-fast | `RazorVue_Pipeline_ClassifiesFullReloadBoundaryFor*ShouldRender*` |
| `async` helper / `Task` / `ValueTask` / `ref` / `out` / `in` by-reference forwarding / 外部 invocation / 未知实例 method payload | ❌ | analyzer / generator / pipeline 三层一致 fail-fast | `RazorVueRazorIrLifecycleBoundaryTests` by-reference 切片 |

## 3. Slot / 子内容

| 形态 | 状态 | 边界说明 | 测试佐证 |
|------|------|----------|----------|
| inline template | ✅ | — | `CreateRenderTree_ForRenderFragmentPropertyExpression_*` |
| source-stable local / member `RenderFragment` / `RenderFragment<T>` carrier | ✅ | 只读 / `readonly` / 可证明无后续写入的 private carrier | `RazorVue_SfcArtifactFactory_LowersAnalyzableCurrentComponentRenderFragment*Carrier*` |
| current-component / local function fragment factory（zero-arg / parameterized / named / `in` / `params` / omitted optional） | ✅ | 非递归；captured value 一次求值；按调用点 / 转发点顺序保留 scope | `RazorVue_SfcArtifactFactory_LowersParameterizedCurrentComponentRenderFragmentFactory*`、`LowersZeroArgument*` |
| getter / factory block body 返回链（由 source-stable `RenderFragment` local carrier 组成） | ✅ | — | `LowersCurrentComponentRenderFragmentFactoryBlockBodyReturningSourceStableLocalChain*` |
| current-component / local function factory 多跳非递归转发链 | ✅ | 保留 captured value scope | `LowersMultiHopCurrentComponentRenderFragmentFactoryForwarding*`、`ForDirectTypedRenderFragmentLocalFunctionFactoryForwarding*` |
| `in` 只读值参数（作为 captured value 读取） | ✅ | 仅已支持的 captured value 读取场景 | `ForDirectRenderFragmentFactoryExpressionWithInParameter_*` |
| recursive fragment factory | ❌ | fail-fast | `WithRecursiveLocalRenderFragmentFactoryMethodForwarding_ThrowsCanonicalizationFailed`、`ForRecursiveDirectTypedRenderFragment*FactoryForwarding_*` |
| 任意 delegate dataflow / delegate invocation / 无法静态还原匿名模板 body 的 callable | ❌ | fail-fast | `WithRenderFragmentFactoryViaDelegateDataflow_*`、`ForDirectRenderFragmentFactoryBlockBodyReturningDelegateInvocation_*` |
| `ref` / `out` 参数 / by-reference forwarding / escape | ❌ | fail-fast | `RazorVueFragmentSlotCarrierBoundaryTests` ref/out 矩阵、`RazorVueRazorIrFragmentSlotCarrierBoundaryTests` |
| `in` 继续传入任意 by-reference invocation | ❌ | fail-fast | `ForDirectRenderFragmentFactoryExpressionForwardingInParameterByReference_*` |
| default slot / named typed slot | ✅ | — | `LowersDefaultSlotForwarding*`、`LowersInlineNamedSlotTemplate*`、`LowersCallableScopedSlotForwarding*` |
| 未知 slot / slot context 误用 / 重复 slot / missing slot | ❌ | `JAZORVUE009/010/011/015` | `WithNonCallableScopedSlotAttribute_ThrowsSlotContextMisuse`、`WithImplicitLibraryDefaultSlotOnComponentWithoutChildContent_ThrowsUnknownSlot` |

## 4. Bind / 事件 / `@key`

| 形态 | 状态 | 边界说明 | 测试佐证 |
|------|------|----------|----------|
| `@bind`（component parameter + `ValueChanged`） | ✅ | descriptor-aware | `RazorVue_SfcArtifactFactory_LowersRazorGeneratedEventCallbackFactoryWrapper_*`、bind 回归 |
| `@key` | ✅ | 常量 / 表达式 | `CreateRenderTree_ForAtKeyAttributes_*` |
| DOM event `@onclick` 等（static / dynamic / merged modifier） | ✅ | event modifier metadata 装饰已确认的 EventCallback / delegate-like DOM event | `LowersElementDomEventWithModifiers*`、`LowersElementDomEventWithDynamicModifier*`、`LowersMergedElementDomEventWithDynamicModifier*` |
| `@onclick:preventDefault` / `:stopPropagation`（常量 `true`） | ✅ | emit modifier gate | `ForElementDomEventWithModifiers_*` |
| `@onclick:preventDefault="false"`（常量 `false` / cleared） | ✅ | template 路径编译期 no-op（不记录 modifier、裸 handler）；pipeline 保留 runtime `setEventModifier(..., false)` replay — 有意分层差异 | `CreateRenderTree_ForElementDomEventWithClearedModifier_*`、`RazorVueSfcArtifactFactory_WithRazorIrTemplateFrontend_LowersElementDomEventWithClearedModifier` |
| `on*` 字符串 attribute（非 EventCallback） | ✅ | 普通字符串属性，不当 DOM event | `WithStringOnAttribute_EmitsPlainHtmlAttribute`、`WithStringOnAttributeAndEventModifier_EmitsPlainHtmlAttribute` |
| component emit modifier 与 HTML DOM event modifier 共用路径 | ❌ | 组件 emits 按 descriptor-aware component event lowering，不与 DOM event modifier 共路径 | `WithComponentEventModifier_ThrowsStructuralIssue` |

## 5. `OpenComponent(Type)` / `System.Type` carrier

| 形态 | 状态 | 边界说明 | 测试佐证 |
|------|------|----------|----------|
| `OpenComponent<T>`（泛型） | ✅ | — | component lowering 回归 |
| direct `typeof(IVueComponent)` | ✅ | — | `CreateRenderTree_WithOpenComponentUsingTypeOf_*`、`RazorVue_SfcArtifactFactory_LowersOpenComponentDirectTypeOf_*` |
| source-stable local `System.Type` carrier | ✅ | declaration-initialized / immediately-assigned | `WithOpenComponentUsingTypeOfLocalCarrier_*`、`LowersOpenComponentTypeOfLocalCarrier*` |
| source-stable member（property / readonly field）carrier | ✅ | — | `WithOpenComponentUsingTypeOfPropertyCarrier_*`、`LowersOpenComponentTypeOfPropertyCarrier*`、`LowersOpenComponentTypeOfReadonlyFieldCarrier*` |
| 只读转发链（local←member / member→member） | ✅ | 非递归 | `WithOpenComponentUsingTypeOfLocalForwardedFromMemberCarrier_*`、`WithOpenComponentUsingTypeOfMemberForwardingChain_*`、`LowersOpenComponentTypeOfMemberForwardingChain*` |
| 动态 `System.Type` 组件 / `typeof(非 IVueComponent)` | ❌ | `JAZORVGA002` component-not-found | `WithOpenComponentTypeOfLocalCarrierForNonComponent_ThrowsComponentNotFound` |
| `System.Type` 当普通 content / attribute / key / condition / loop source | ❌ | `ThrowIfComponentTypeCarrierUsedAsRuntimeValue` → `CanonicalizationFailed` | `CreateRenderTree_WithComponentTypeLocalCarrierUsedAs{Attribute,Content,Key,Condition,LoopSource}_*`、IR frontend 同名矩阵 |
| carrier 后续可观察写入（branch/loop/ref escape） | ❌ | source-stable 合同 fail-fast | `WithReassignedOpenComponentTypeOf{Local,Member}Carrier_*`、`WithBranchAssigned*`、`WithRefEscaped*` |

## 6. Render Helper / Open Frame（caller-owned replay）

> 该领域是 imperative-only 特性，Layer 2（`.razor`）不适用；实际为 Layer 1（frontend）/ Layer 3（SFC）/ Layer 4（pipeline）三层。

| 形态 | 状态 | 边界说明 | 测试佐证 |
|------|------|----------|----------|
| caller-owned attribute / key / spread / event-modifier / slot mutation / ambient child emission replay | ✅ | 无 captured value 走 template carrier；带 captured binding 走 render-function carrier | `RazorVueRenderHelperOpenFrameBoundaryTests` caller-owned 矩阵、`RazorVue_SfcArtifactFactory_WithCallerOwned*` |
| expression-bodied single-call helper / builder local alias / `this.` qualified helper | ✅ | — | `WithCallerOwnedExpressionBodiedHelper*`、`WithCallerOwnedBuilderAliasHelper*`、`WithCallerOwnedThisQualifiedHelper*` |
| `if/else` 双分支 / 嵌套组合 attribute+child / component attribute+default-slot / nested `if/else` replay | ✅ | — | `WithCallerOwnedIfElseHelper*` |
| guard-return / consecutive guard-return / single-branch / both-branches terminal-return replay | ✅ | — | `WithCallerOwnedGuardReturnHelper*`、`WithCallerOwnedTerminalReturnBranchMutation*` |
| frame-neutral caller-owned loop / `try/catch/finally` / `lock` / null-default `using` / using declaration scope mutation replay | ✅ | 不改变 frame depth | frame-neutral replay 矩阵 |
| `switch` replay（single-value / default / multiple-label / pattern-local condition-only / source-stable prelude guard / relational+guarded） | ✅ | `goto case` / `goto default` fail-fast | switch replay 矩阵 |
| generic value-erased helper attribute replay | ✅ | 静态泛型状态 OK | `WithCallerOwnedGenericHelperAttributeMutation*` |
| recursive render helper（current-component / local function）imperative materialization | ✅ | — | `WithTerminatingRecursiveRenderHelper*` |
| 只读 `ref` captured value（helper 非 builder `ref` 参数） | ✅ | 实参可寻址、helper body 不 assign/increment/decrement、不 by-ref 转发 | `WithReadOnlyRefParameterRenderHelper_*` |
| `out` 参数 / `ref` 写回 / by-reference forwarding | ❌ | fail-fast | `WithWritingRefParameterRenderHelper_*`、`WithOutParameterRenderHelper_*` |
| async helper（current-component method / local function；async void / `Task` / `ValueTask`） | ❌ | fail-fast | async helper 边界矩阵 |
| omitted optional parameter / named argument reshaping | ❌ | 声明顺序一致的显式命名调用仍可 replay；命名参数重排 / omitted optional default / 未读取 omitted optional fail-fast | optional/named helper 边界矩阵 |
| generic helper runtime type-parameter semantics（`typeof(T)` / `default(T)` / `new T()` / `is T` / declaration pattern；direct / 已调用 local function / 已调用 lambda body） | ❌ | fail-fast；未调用 local function / lambda body 不做过度扫描 | generic helper 边界矩阵 |
| active frame 漂移 / 跨 helper 未闭合 frame / 关闭重开 caller frame / component-frame DOM event modifier / region 逃逸 / recursive caller-owned mutation | ❌ | frame identity/depth 稳定性 fail-fast | frame 边界矩阵 |
| caller-owned loop / try/catch/finally / lock / using 改变 frame depth / try 内 `goto` / 真实 disposable `using` | ❌ | fail-fast | runtime control-flow 边界矩阵 |
| helper component via `new Component()` 当普通对象 | ❌ | helper component 只通过 `OpenComponent` / component reference 渲染 | `WithComponentHelperClassInImperativeRender_*` |

## 7. Static Markup / `MarkupString`

| 形态 | 状态 | 边界说明 | 测试佐证 |
|------|------|----------|----------|
| 常量 string / `new MarkupString(literal)` / `(MarkupString)literal` | ✅ | — | `LowersConstantAddMarkupContent`、`LowersConstantMarkupStringAddContent` |
| source-stable local / member（property / readonly field / 可证明无后续写入 private）carrier | ✅ | — | `LowersLocalAddMarkupContentCarrier*`、`LowersReadonlyAddMarkupContentPropertyCarrier*` |
| static-markup factory（current-component / local function；含 `params` / omitted optional / captured argument） | ✅ | 保留调用点求值顺序与 captured scope | `LowersCurrentComponentStaticMarkupFactoryMethod*`、`LowersMarkupStringFactoryMethodWithParamsParameter*` |
| 静态 `+` 拼接（每段可证明静态） | ✅ | `string + string` → `(MarkupString)` cast | `LowersConcatenatedStaticAddMarkupContentCarriers`、`LowersConcatenatedStaticMarkupStringExpression*` |
| 条件静态分支（`cond ? (MarkupString)"<a/>" : (MarkupString)"<b/>"`） | ✅ | lower 成 template conditional | `LowersConditionalStaticAddMarkupContent_ToTemplateConditional` |
| 动态 raw HTML / 运行时生成 `MarkupString` | ❌ | fail-fast | `WithDynamicAddMarkupContentCarrier_*`、`WithDynamicNewMarkupStringAddContent_*` |
| 执行型元素（`script`/`iframe`/`object`/...）/ 执行型属性（`on*`/`srcdoc`/`v-html`/`formaction`）/ Vue/raw directive / 畸形 tag/attribute name / `javascript:`/`vbscript:` / executable `data:` URL | ❌ | `RazorVueStaticMarkupParser` fail-fast | `WithScriptAddMarkupContent_*`、`WithInlineEventMarkupStringAddContent_*`、`WithSrcdoc*`、`WithFormAction*`、`WithExecutableUrl*`、`WithExecutableDataUri*`、`ForMalformedStaticMarkup*` |
| carrier 后续可观察写入 | ❌ | source-stable 合同 fail-fast | `WithMutatedConcatenatedAddMarkupContentCarrier_*`、`WithImmediatelyAssignedMarkupStringLocalCarrierThenReassigned_*` |

## 8. Component / Component Library

| 形态 | 状态 | 边界说明 | 测试佐证 |
|------|------|----------|----------|
| `ComponentBase, IVueComponent` + `[ECMAScriptModule]` 入口 | ✅ | authoring 合同：per-file `using static ECMAScript.Vue3;` | sample `TodoApp.razor.cs`、SDK 集成 `CreateRazorVueSampleProject` |
| `IVueLibraryComponent` 库组件（Vuetify / ElementPlus / TDesign / Vben） | ✅ | descriptor-aware | `VuetifyAuthoringSurfaceTests`、`ElementPlusAuthoringSurfaceTests`、`TDesignAuthoringSurfaceTests`、`ECMAScript.Vuetify.ComponentCoverageMatrix.md` |
| component parameter descriptor / nested component metadata / import | ✅ | — | `ImportsNestedUserAndLibraryComponents_IntoScriptSetup` |
| `IVueContainerComponent` inject | ✅ | container inject lowering | `VueContainerComponentInjectTests` |
| 直接 `ComponentBase` 入口 / 缺 `[ECMAScriptModule]` | ❌ | `JAZORVUE002` / `JAZORVGA017` | analyzer 诊断矩阵 |
| `StateHasChanged()` | 🟡 | `JAZORVUE004` Warning（不 fail）；Vue 是响应式驱动，调用被忽略为 no-op | `RazorVue_Misuse_StateHasChanged_ReportsJAZORVUE004_AsWarning` |
| 无效 bind / 未知 parameter / 未知 slot | ❌ | `JAZORVUE008/007/009` | analyzer 诊断矩阵 |

## 9. Route / DOM / Consumer Build

| 形态 | 状态 | 边界说明 | 测试佐证 |
|------|------|----------|----------|
| 普通多参数 composite/mixed route segment | ✅ | Emit 回归固化 | `RazorVueGeneratorRouteTests`、Emit route bridge 回归 |
| 普通 catch-all / 单尾部 optional separator composite segment | ✅ | Emit 回归固化 | Emit route bridge 回归 |
| optional separator 参数位置非法 / 多层 optional separator composite/mixed segment / catch-all inline constraint / 未知自定义 constraint / constrained catch-all / 不可映射 constraint 组合 | ❌ | 停在 SDK Razor route parser / Emit fail-fast 边界 | route bridge fail-fast 回归 |
| `.vue` default export/import / default+named import / default re-export / render-function-only default export / selected-entry 相对 `.vue` 依赖 | ✅ | SFC bridge 生成 named-export `.mjs`，重写 default import/re-export | `RazorVueSfcBridgeCompilerTests`（11/11） |
| selected-entry 缺失 manifest 相对 `.vue` 依赖 | ❌ | fail-fast | `RazorVueSfcBridgeCompilerTests` |
| colocated consumer runner 缺失 | ❌ | MSBuild `<Error>` fail-fast，不产出 stale `wwwroot/jazor` 资产 | `Build_LocalJazorPackage_WebSdkHost_WithColocatedConsumer_MissingRunnerFailsFast` |

## 10. Source Map / HMR

| 形态 | 状态 | 边界说明 | 测试佐证 |
|------|------|----------|----------|
| source-origin / hash / HMR 元数据（`.vue.map`、`.origins.json`、manifest） | ✅ | 产物携带 | `Build_LocalJazorPackage_WithSourceReferencedRazorVueSample_EmitsRazorVueOutputs`、`_SecondBuildWritesUpdatePlan` |
| HMR 边界分类（TemplateOnly / LogicSafe / FullReloadRequired） | ✅ | `ShouldRender` / lifecycle / SetParametersAsync 边界分类 | `RazorVue_Pipeline_Classifies*Boundary*` 矩阵 |
| `.razor → .vue → bundled JS` sourcemap 真实浏览器调试链路 | 🟡 | origins sidecar 存在；真实浏览器调试体验未验收（见 Gate 4 / `completion-analysis.md`） | 待验收或文档化 |

## 11. SDK / Package / Emit

| 形态 | 状态 | 边界说明 | 测试佐证 |
|------|------|----------|----------|
| `Jazor` NuGet 包 payload（不含 Razor Compiler / Razor.Utilities.Shared / Harmony / MonoMod / Detour） | ✅ | payload guard | `CreateLocalPackage_IncludesRazorVueAuthoringAssets` |
| `ECMAScript.Vuetify` 包依赖 `Jazor` | ✅ | — | `CreateLocalPackage_IncludesVuetifyAuthoringPackage` |
| 独立外部 .NET + Deno consumer | ✅ | 闭合 | `Build_LocalPackages_WithExternalRazorSgSfcConsumer_PureDenoPipeline_PassesInIsolatedWorkspace`、`Build_LocalPackages_RazorVueTodoListSample_PureDenoPipeline_PassesInIsolatedWorkspace` |
| MSBuild `JazorEmit` / `JazorConsumerBuild` / publish materialize | ✅ | fail-fast on missing runner | `Build_LocalJazorPackage_*` / `Publish_LocalJazorPackage_*`（26/26） |
| DenoHost 签名校验（生产默认） | ✅ | `DENOHOST_ALLOW_CHECKSUM_BYPASS` 仅 break-glass，非生产模式 | DenoHost README；测试机 `deno.metadata.json` 签名不匹配是环境问题 |

## 维护规则

- 本矩阵与 `razorvue-playground-support-gaps-2026-05-12.md` 互引：动态边界变化先更新缺口页，再同步本矩阵对应行。
- 不在本文追加逐次修复日志；完成过程留在测试名、PR/commit 描述和 git 历史中。
- 测试佐证列只给代表性测试名 / 类，不穷举；完整覆盖以测试代码为准。
- 任一行状态变化时，必须同时确认对应测试仍绿。
