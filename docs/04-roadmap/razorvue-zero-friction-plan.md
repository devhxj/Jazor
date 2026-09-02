# RazorVue “零摩擦”执行计划

> 日期：2026-08-31
> 状态：执行计划，不是 Support 声明。每项能力仍以 `RazorVueM5CapabilityLedger` 和本文件的验收门禁为准。
> 目标范围：标准 Blazor **自定义组件作者面** + TDesign 等第三方 typed component library；Microsoft/Blazor 内置 UI 组件不在本计划支持范围内。

## 0. 结论先行

规范化和交付链路已经达到可用基线：Emit 回归为 `185/185`，Razor Source Generator 回归为
`4936/4936`；JazorAdmin 的 Release 本地 NuGet consumer（native 与 VueInject）均为 0 warning/0
error，最终 Edge mount smoke 也已通过。`vue-data-ui` 的 71 个入口相对 ESM 闭包已经补齐。
真实浏览器证据固定保留三条互补入口：.NET `BrowserSmokeTestHelper` 负责可重复的
Edge/Chrome/Chromium 自动门禁，Playwright CLI 负责固定 Chrome/Chromium 会话的快照和交互，
`agent-browser` 负责独立 CDP session 的复核；任一入口都不能单独替代 package 或语义证据。

这说明“分号、包复制、资源闭包和基本 TDesign 页面运行”不再是主要风险，但还不能把 RazorVue
称为“目标范围内零摩擦”。剩余摩擦集中在三类：

1. **Compiler/runtime gap**：ParameterView 完整协议、组件 activation/DI 生命周期、cascading
   更新、导航状态、异步 lifecycle 和 DOM/reference 的真实浏览器语义证据不完整。
2. **Binding/API gap**：TDesign 泛型/非泛型组件、typed slot、EventCallback、`@bind`、union、
   required parameter 的自然 Razor 写法还没有经过一套独立矩阵；JazorAdmin 中的桥接、cast 和
   手写 `BuildRenderTree` 是否仍必要，也没有逐项裁决。
3. **Evidence/delivery gap**：若干切片只有 source/official SG/module/Deno 证明，缺真实 browser、
   Blazor reference oracle、隔离的 Release package consumer，或适用的 SSR/hydration 证明。

本计划先消除作者可感知的摩擦，再补齐语义和交付证据；不会通过“生成成功”或单页 workaround
把未证明能力标成 Support。

## 1. “零摩擦”的可验收定义

对目标范围内的页面/业务组件作者，以下条件必须同时成立：

- 只写标准 Razor/C#：`ComponentBase`、`[Parameter]`、生命周期、`EventCallback`、
  `RenderFragment`、`@bind`、`@key`、`@ref` 和浏览器可用的 typed service；组件库使用 TDesign
  的强类型参数、slot 和事件契约。
- 不需要学习 `RenderTreeBuilder`、VNode frame、Vue AST、module marker、import alias、
  中间 JS 协议或 generated C# 的内部细节。组件库维护者可以使用这些内部层，页面作者不可以被迫使用。
- 首次构建就能在作者源码位置给出可执行的诊断；不存在“生成成功、浏览器才发现
  `undefined`/错误 import/错误 lifecycle”的 runtime-first 失败。
- 同一份作者源码在 official Razor Source Generator、Roslyn semantic lowering、最终 `.mjs`、
  真实浏览器和 Release package consumer 中保持可观察行为一致；启用 SSR 时再增加 hydration/state
  证明。
- 业务页面中 RazorVue 专用符号、手写 JS、手写 builder、无意的 `object`/cast 和应用侧薄桥接数量为零，
  或有明确的组件库内部理由并记录在 API review 中。

### 1.1 支持结果和证据等级

| 决策 | 含义 | 可接受状态 | 最低证据 |
| --- | --- | --- | --- |
| Direct Support | 标准写法直接 lowering，作者无额外适配 | `Support` | source + official SG + module/AST + runtime；触及时加 browser/package/SSR |
| Compatibility Adapter | 作者保留标准写法，由框架 adapter 提供浏览器等价行为 | `InProof` 或 `Support` | adapter 行为矩阵 + 上述全部适用证据 |
| Guided Adaptation | 不能保真但有稳定、短小、类型安全的替代 | `Guidance` | authored-source 诊断、HelpLink、最小替代 fixture |
| Reject | 目标运行时无法安全/确定地表达 | `Reject` | 稳定诊断、无 partial artifact、可复现失败 |
| Planned | 尚未实现或没有行为协议 | `Planned` | 只能出现在明确 backlog，不能被样例 workaround 覆盖 |

`InProof` 不是“基本可用”的同义词；在完成对应门禁前不得对外宣称 Support。

## 2. 产品范围与明确不做项

### 2.1 本计划支持

- 标准 Blazor 自定义组件作者面：组件参数与生命周期、事件回调、fragment/templated component、
  `@bind`、`@key`、`@ref`、可写 `[Inject]` browser service、普通 cascading 和 route metadata。
- TDesign Vue Next 的 typed component API，以及 Vuetify、Element Plus 和应用自定义的
  `ComponentBase + IVueComponent` 组件。
- 为上述作者面所需的 CLR/browser primitive mapping。runtime-sensitive 类型仍由
  `Jazor.CLR.Generator -> Jazor.CLR -> Jazor.Compiler` 负责，RazorVue 只做最终产品投影。

### 2.2 Microsoft/Blazor 内置 UI 组件：永久范围外

以下内容不是本计划的待实现项目，不设 P0/P1/P2 里程碑，也不因历史 adapter 或样例代码而恢复兼容：

| 内置组件/协议 | 结果 | 作者替代 |
| --- | --- | --- |
| `Router`、`RouteView`、`LayoutView`、`NavLink`、`NavigationLock`、`FocusOnNavigate` | `Reject` | RazorVue route catalog/host、应用自定义导航组件或 TDesign 组件 |
| `DynamicComponent` | `Reject` | 静态导入的 typed component 或应用自定义的显式分支 |
| `ErrorBoundary` | `Reject` | 应用自有 error boundary contract 或组件库组件 |
| `EditForm`、`InputBase<T>`、`Input*`、`ValidationMessage`、`DataAnnotationsValidator`、`InputFile` | `Reject` | TDesign/Vuetify/Element Plus 表单组件或应用自定义 typed form |
| `AuthorizeView`、`AuthorizeRouteView`、`CascadingAuthenticationState` | `Reject` | 认证状态 provider + endpoint 授权 + 应用自定义视图 |
| `Virtualize<TItem>`、`QuickGrid<TGridItem>` | `Reject` | 组件库/应用自有虚拟列表或表格 |
| `PageTitle`、`HeadContent`、`HeadOutlet`、`SectionContent`、`SectionOutlet` | `Reject` | RazorVue host/SSR 明确的 head/section contract |

这些标签被识别时必须在作者源或 final pipeline 稳定报告 `JAZORVGA021`/对应 guidance；不得静默
生成一个“看起来能跑”但语义不同的 Vue 替代品。这里的“Reject”只针对内置组件入口，不限制
`ComponentBase`、`EventCallback`、`RenderFragment`、`ParameterView`、`NavigationManager`
等 framework primitive 在自定义组件 lowering 中的逐项支持。

### 2.3 其他固定边界

- `IJSRuntime`、`IJSObjectReference`、`IJSInProcessRuntime`、`JSInvokable` 及字符串调用/动态
  import/marshalling facade 固定 `Reject`；使用 typed ECMAScript/WebIDL/module binding。
- `DbContext`、`HttpContext`、ASP.NET host environment、Identity manager 等 server-only service
  固定 `Reject`；使用 endpoint 和 typed browser client。
- 不以 `object?`、开放泛型或隐式 JS fallback 扩大公共 API；union、overload 和 `From(...)` 只在
  C# 类型系统无法自然表达时采用。

## 3. 当前基线与未闭合项

| 领域 | 当前状态 | 已有证据 | 未闭合原因 |
| --- | --- | --- | --- |
| 普通 markup、泛型、fragment/slot、control flow、bind/event、参数 lifecycle | `Support` | official SG、module、Deno 回归；JazorAdmin 消费者 | 仍需保持独立自然写法矩阵，避免桥接掩盖回归 |
| 复杂 async lifecycle（rejection、cancellation、重复 render、async disposal race） | `InProof` | `RazorSgOfficialComplexLifecycleRuntimeTests`：official SG、module artifact、Deno；isolated Release package consumer + real browser smoke；Windows SSR Release consumer 已证明初始 `OnInitializedAsync` 完成后才序列化 HTML 并在 hydration 后保留状态；`RazorSgBlazorReferenceOracleTests` 作为 framework primitive 对照 | 完整 SSR/prerender lifecycle identity、rejection/cancellation 与 hydration 副作用证据仍未闭合；不得因 browser/package 通过升级为 `Support` |
| `@page`/`@layout`/route metadata 与页面状态 | `InProof` | route catalog 输出与 Deno 场景 | 真实浏览器路由切换、not-found、query refresh 和错误/重试流程 |
| `ParameterView`/`SetParametersAsync` | `InProof` | sparse/alias/slot/queue adapter 测试；isolated Release package/browser consumer 已覆盖参数替换与 lifecycle ordering；Windows SSR Release consumer 已证明 serialized props 在 server HTML/hydration 中经 `SetParametersAsync` 应用并等待初始 async task | 完整 snapshot、取消深度、作者 SSR 异常传播与更深 reference parity |
| browser `[Inject]` property adapter | `InProof` | provider、lifecycle、missing-provider 测试；isolated Release package/browser consumer 已覆盖嵌套与 recreated component activation；SSR runner 与 hydration document 通过同一 serialized application provider 传递并由 TodoList Release consumer 验证 | provider lifetime、完整 reference parity、SSR 更新/副作用语义；constructor injection 保持 Reject |
| cascading values | `InProof` | typed/named/nested Deno 测试；isolated Release package/browser consumer 已覆盖 nested override、`IsFixed`、same-value、dispose 与 update propagation；Windows SSR Release consumer 已证明 named cascade 在 server HTML 与 hydration 中传递到子组件 | 完整 reference parity、SSR 更新传播与 hydration 副作用证据 |
| `NavigationManager` 与 LocationChanging | `Support`（Compatibility Adapter，内部导航子集） | URI/navigate 基础映射与 CLR whitelist；reference oracle、official SG、Deno、真实 HTTP-origin browser、Release consumer | 仅同一 base URI 的内部 `NavigateTo` 承诺 handler、取消、query/hash/history state 和 dispose；popstate/hashchange cancellation、SSR/prerender 与 server circuit 不承诺 |
| Mouse/Keyboard/Focus/Change | `Support`（Compatibility Adapter） | CLR mapping、Blazor reference metadata/value oracle、official SG、Deno、真实 browser、bundle source map、isolated Release package consumer | 仅 constructor/setter/identity、synthetic payload、file input 与 SSR/prerender 不在声明内 |
| Pointer/Wheel/Drag/Clipboard/Touch/Error/Progress | `Support`（browser interactive） | generator、CLR、Blazor `EventHandlers` reference metadata、official SG、Deno、真实 BrowserSmoke、isolated Release package consumer | getter-only native projection 已闭合；构造器/setter/files/items、synthetic payload 与非 getter TouchList 操作保持拒绝，SSR/prerender 不声明 |
| `@ref`/`ElementReference.FocusAsync` | `Support`（Direct Support） | VNode ref 与 mapping、official SG/Deno、真实 browser、isolated Release consumer | 仅承诺 browser interactive；SSR/prerender 不在本切片声明内 |
| TDesign typed API | `Support`（Direct Support） | 118 runtime component binding、自然 Razor generic/non-generic、slot、union、bind、required 矩阵；isolated Release consumer + real Edge smoke | 后续组件扩展仍需按同一四层证据门禁；不扩大到内置 Blazor UI |
| package/artifact/HMR | 交付基线已可靠 | Emit `185/185`、JazorAdmin package/browser gate、ESM closure | 每个新增 framework slice 仍需独立 consumer；SSR feature proof 不可借用总 gate |
| authentication state | `Guidance` | authored `[Inject]`/`@inject` 诊断 `JAZORVCA007`；`AuthorizeView`/`AuthorizeRouteView` 保持 `Reject` | 尚无版本化 browser provider、claims/SSR handoff；endpoint authorization 是安全边界 |
| SSR state/form handoff | `Guidance` | authored `PersistentComponentState`、`[PersistentState]`、`[SupplyParameterFromForm]` 诊断 `JAZORVCA011` | 尚无版本化 payload/form protocol；使用 typed endpoint/bootstrap payload，内置表单协议仍不在范围 |

## 4. 缺口归因规则

同一个用户症状只能有一个主 owner，避免把 binding 问题错误地修成 compiler 特例：

| 观察到的症状 | 先查什么 | 允许的修复位置 |
| --- | --- | --- |
| 官方 SG 生成的 C# 已经表达了语义，但 `.mjs` 顺序/值/生命周期错误 | usage-site operation、`SemanticWalker`、RenderEmitter | `Jazor.Compiler`/`Jazor.RazorVue`；补 source + AST/module + runtime 回归 |
| `.mjs` 正确但 Razor 参数无法自然绑定、泛型/union/slot 需要 cast/桥接 | TDesign contract、Razor SG binding、公开 C# 类型 | `ECMAScript.TDesign` 或公共 authoring contract；只有证据表明语义缺失时才改 compiler |
| 本地 Deno 正常，Release/browser/package 或 SSR 失败 | manifest、selected closure、consumer isolation、host | `Jazor.Emit`/packaging/host；不得把 package 缺口藏到页面 workaround |
| 只在单个 JazorAdmin 页面需要一段手写 builder | 先做 API review 和最小自然 Razor fixture | 优先公共 binding/compiler；确认只服务领域语义后才保留 sample-local wrapper |

## 5. 可执行 backlog

状态字段沿用 `RazorVueM5CapabilityLedger`；新 ID 是本计划的执行跟踪号，不替代 ledger ID。

### P0：先让常见页面自然可写

| ID | 用户摩擦 / 当前状态 | 类型与 owner | 实施方向和依赖 | 验收与退出状态 |
| --- | --- | --- | --- | --- |
| ZF-P0-01 | TDesign 页面需要 `AdminInput`/`AdminForm` 等薄桥接，作者被迫接触 callback 转发和 cast。 | Binding/API；`ECMAScript.TDesign` + `Jazor.RazorVue` | 已完成 native TDesign 与 bridge 对照；公共 prop/event contract 和 direct render 路径已收敛。 | natural-authoring fixture 无手写 builder/cast 完成 CRUD 交互；Release browser smoke 与 isolated package consumer 通过。目标：`Support`（完成）。 |
| ZF-P0-02 | `TPrimaryTable<T>` 的泛型/非泛型入口、typed cell/row slot、`RenderFragment<T>` 和 EventCallback 在 Razor 中仍可能需要固定 wrapper。 | Binding/API + RazorVue component binding | TypeInference、开放泛型、slot context、delegate variance 和跨模块 selector 已纳入矩阵；不以 `object` 放宽契约。 | table columns、typed slots、empty/loading、bind、required 参数已通过 authored + official SG + Deno + real Edge + Release consumer；目标：`Support`（完成）。 |
| ZF-P0-03 | 同一 JS prop 的 C# 后缀（如 `LoadingValue`/`LoadingContent`）和 union branch assignment 增加记忆负担。 | Binding/API；`ECMAScript.TDesign` | 命名/union API review 已完成：采用稳定后缀、native union 或 tagged fallback，并保留 Razor SG 可绑定性；不增加弱类型 escape hatch。 | Button/Table/Form/Dialog 常见写法无需 cast/`From`；breaking change 按 MINOR 和迁移说明发布。目标：`Support`（完成）。 |
| ZF-P0-04 | `@page`、layout、typed query/route 参数、not-found 和应用自有页面状态已经完成浏览器闭环，ledger 为 `Support`（Compatibility Adapter）。 | Compiler/runtime；RazorVue route host | route catalog -> host state -> component activation 保持单一协议；Release consumer 已验证 highlighted -> standard query refresh、返回 board、三次 browser back 和 not-found。Router/RouteView/LayoutView/NavLink 与 LocationChanging cancellation 不进入实现。 | `RazorSourceGeneratorTailOutputTests`、`RazorSgNavigationRuntimeTests` 和 `samples/RazorVue.Authoring/verify-smoke.cs` 已覆盖 official SG、module/source-map、Deno、isolated Release package、真实 Chrome/Chromium/Edge browser journey 与 console errors=`0`；SSR/prerender 未声明。目标：`Support`（完成）。 |
| ZF-P0-05 | 作者源码诊断已覆盖 D0-D5，但新失败仍可能在 generated C# 才暴露，或 analyzer/final pipeline 重复报告。 | Diagnostics；`Jazor.RazorVue` + analyzer | 为每个新边界先登记 ledger；author-source 高置信诊断与 final Compilation 单次裁决分层，保留 `JAZORVGA020`-`026` 稳定分类。 | source location、dedupe、无 partial ModuleCatalog、HelpLink 和最小替代回归；目标：Direct Support 零噪音，其他形状首建可解释。 |
| ZF-P0-06 | 独立、最小、Blazor-first 作者样例已建立，证明不需要 JazorAdmin 经验即可完成常见 CRUD 页面。 | Evidence/delivery；`samples/RazorVue.Authoring` | 已建立 Todo/CRUD 页面：TDesign table/form/dialog、typed slot、`@bind`、async callback、DI、route；样例不使用内置组件、历史 bridge、手写 builder 或应用侧 cast。 | 已通过：`dotnet run --file samples/RazorVue.Authoring/build-local.cs -- --source-only --configuration Debug --work-root .tmp/authoring-local-build-debug`（首次成功构建 50.52s；0 warning、0 error，作者源 internal symbols=`0`）；`dotnet run --file samples/RazorVue.Authoring/verify-smoke.cs -- --configuration Release --work-root .tmp/authoring-local-build --package-output .tmp/nupkg-sample/RazorVue.Authoring`（Release/package pipeline 99.66s；isolated Release package consumer、Jazor/Vue/TDesign/Style 本地包、bundle/source map/vendor closure、无 `node_modules`、真实浏览器 mount/dialog/input/submit/route 交互通过，console errors=`0`）。目标：样例只消费已达 `Support` 的能力；`Support`（完成）。 |

### P1：补齐高频 framework primitive 的语义闭环

| ID | 用户摩擦 / 当前状态 | 类型与 owner | 实施方向和依赖 | 验收与退出状态 |
| --- | --- | --- | --- | --- |
| ZF-P1-01 | `SetParametersAsync(ParameterView)` 已能处理 sparse/slot/queue 场景；完整 snapshot/异常行为仍不完整。 | Compiler/runtime；ParameterView adapter | 固定 CLR default -> base apply -> overlay -> lifecycle -> queued update 的顺序；只实现已定义的成员，`TryGetValue`/枚举/`ToDictionary` 继续 Guidance。 | `RazorSgSetParametersAsyncRuntimeTests` 与 isolated Release package/browser consumer 已覆盖生命周期顺序、参数替换和队列；`verify-windows-ssr-release.cs` 已在 isolated Release NuGet consumer 中证明 serialized props、首次 async 参数任务、server HTML 与 hydration 后交互；仍需异常、取消深度与更深 reference parity；目标：`Support`。 |
| ZF-P1-02 | `[Inject]` browser service 已有 property activation 的 package/browser 证明；provider lifetime 与完整 activation 语义仍未闭合。 | Runtime/host；`VueInjectRegistry` + `VueModuleBuilder` | 明确实例级 registration、初始化/Dispose 顺序和 async callback 竞态；SSR 请求携带 JSON provider envelope，runner 与 hydration 使用同一 application provider；constructor injection、primary constructor、`this(...)`、`base(args)` 保持 `Reject`。 | official SG + Deno + isolated Release package/browser 已覆盖嵌套与 recreated component；`JazorSsrHostingTests` 与 Windows SSR Release consumer 已证明 server provider activation 及 hydration provider handoff；仍需 provider lifetime、reference parity 与 SSR 更新/副作用语义；目标：`Support`。 |
| ZF-P1-03 | cascading 参数已有 nested/named/update 的 package/browser 证明；完整 reference/SSR parity 仍未闭合。 | Runtime；cascading adapter | 以 Vue scope 建立 typed/named provider；一次更新只触发必要 consumer；不可写属性继续 `JAZORVCA008`。 | `RazorSgCascadingValueRuntimeTests` 与 isolated Release package/browser 已覆盖 nested override、replacement、same-value、`IsFixed`、dispose；`verify-windows-ssr-release.cs` 已证明 named cascade 在 server HTML/hydration 中到达子组件；仍需 reference parity、SSR 更新传播与 hydration 副作用证据；目标：`Support`。 |
| ZF-P1-04 | `NavigationManager` 基础 URI/navigate 与 LocationChanging 的声明子集已闭环。 | CLR/runtime + route host | 依照 reference oracle 固定 handler 顺序、取消、registration dispose、rapid navigation；只在同一 base URI 的内部 `NavigateTo` 上运行，不承诺 server circuit 或 popstate/hashchange 取消。 | `RazorSgNavigationRuntimeTests`、`RazorSgBlazorReferenceOracleTests` 与 `SdkIntegrationTests.Build_LocalReleasePackages_WithExternalNavigationLocationChangingRazorConsumer_ProvesInternalCancellationInRealBrowser` 已覆盖 official SG、module、Deno、真实 HTTP-origin browser、query/hash/history state、rapid supersede 和 isolated Release package；目标：声明支持的子集 `Support`，未实现部分稳定 `Guidance/Reject`。 |
| ZF-P1-05 | Mouse/Keyboard/Focus/Change 事件及 typed callback 已完成 browser-interactive 闭环。 | CLR mapping + RenderEmitter | 保持 native event carrier；`ChangeEventArgs.Value` 只在事件时间 capture，不引入 synthetic Args 构造或 setter。 | `RazorSgOfficialCoreDomEventRuntimeTests`、`SdkIntegrationTests.Build_LocalReleasePackages_WithExternalCoreDomEventsRazorConsumer_HandlesNativeEventsInRealBrowser` 已覆盖 reference metadata/value shaping、official SG、Deno、真实 DOM dispatch、source map 与 isolated Release package；目标：`Support`（完成）。SSR/prerender 不声明。 |
| ZF-P1-06 | `@ref`/`ElementReference.FocusAsync` 已完成 browser interactive 闭环。 | CLR mapping + Vue ref framing | `Jazor.CLR` Import helper 固定 ref callback 生命周期、Promise/ValueTask 完成语义与 framework failure；不扩展为任意 DOM 方法或 `new ElementReference`。 | mount/update/unmount/empty ref 的真实 browser + isolated Release package consumer 已通过；目标：`Support`（完成）。SSR/prerender 不声明。 |
| ZF-P1-07 | 普通 lifecycle 已 Support；复杂 async lifecycle 的 rejection、取消、重复 render、Dispose race 已完成 official SG/module/Deno 与 isolated Release package/real-browser proof，ledger 仍为 `InProof`。 | Runtime；`VueModuleBuilder`/host | 用 setup-local failure capture、generation-aware guard 和明确 queue 顺序处理可观察副作用；不增加未要求的全局防御式重试。 | `RazorSgOfficialComplexLifecycleRuntimeTests` 已覆盖六种复杂场景（含 queued-after-unmount suppression 与 stale rejection propagation），`SdkIntegrationTests.Build_LocalReleasePackages_WithExternalComplexLifecycleRazorConsumer_ProvesAsyncRacesInRealBrowser` 已覆盖真实 browser/package consumer；`verify-windows-ssr-release.cs` 进一步证明初始 `OnInitializedAsync` 完成后才输出 server HTML，并在 hydration 后保持标记；仍需完整 SSR/prerender identity、rejection/cancellation 与 hydration 副作用证据，完成后才可目标 `Support`。 |

### P2：扩大真实生产覆盖，但不扩大内置组件范围

| ID | 用户摩擦 / 当前状态 | 类型与 owner | 实施方向和依赖 | 验收与退出状态 |
| --- | --- | --- | --- | --- |
| ZF-P2-01 | Pointer/Wheel/Drag/Clipboard/Touch/Error/Progress 七组 getter-only 事件已完成 browser-interactive Support。 | CLR mapping；`Jazor.CLR` + generator | 保持 native carrier 与属性访问时的 TouchList `Array.from(...)`；构造器、setter、files/items、非 getter TouchList 操作继续拒绝。 | `RazorSgOfficialExtendedDomEventRuntimeTests` 已覆盖 reference metadata、official SG/Deno；`SdkIntegrationTests.Build_LocalReleasePackages_WithExternalExtendedDomEventsRazorConsumer_HandlesNativeEventsInRealBrowser` 已覆盖真实浏览器与 isolated Release package；目标：`Support`（完成），SSR/prerender 不声明。 |
| ZF-P2-02 | 认证状态尚未有 browser provider 与服务端 endpoint/claims handoff；ledger 已明确为 `Guidance`。 | Host/runtime；ASP.NET Core + RazorVue | 保持 `AuthenticationStateProvider` 缺失 provider 的 authored `JAZORVCA007` 诊断；不实现 `AuthorizeView` 等内置组件，不把 UI 隐藏当授权。 | authored source 能在首次构建给出 typed provider/endpoint 替代，匿名/过期/403 由 endpoint 测试覆盖后再升级；当前目标：稳定 `Guidance`，不伪称 `Compatibility Adapter`。 |
| ZF-P2-03 | `PersistentComponentState`、hydration state 和 SSR/增强 post handoff 尚无版本化协议；ledger 已明确为 `Guidance`。 | SSR/host；`Jazor.AspNetCore` + `Jazor.Emit` | `JAZORVCA011` 在 authored property/attribute 注入点阻断未定义 handoff；使用 typed endpoint/bootstrap payload；内置 `EditForm`/antiforgery/enhanced form protocol 不纳入，表单 UI 由 TDesign 等承担。 | authored source 首建稳定诊断、无 partial artifact，并有最小 typed endpoint 替代 fixture；版本化 state protocol、重复提交/失配 hydration 证据完成后才考虑 `Support`。 |
| ZF-P2-04 | 组件数量、slot 深度和高频更新增长后，作者不应为性能手动改写 API。 | Runtime/perf；`RenderEmitter` + host | 以真实样例 benchmark 决定 block/patch/handler cache；不为“看起来像 Vue”引入破坏语义的优化。 | P0/P1 authoring sample 与 JazorAdmin 的 render/update/SSR 指标、无行为回归；目标：性能门禁稳定，未达标项不阻塞语义 Support 但必须有 issue。 |

## 6. 分阶段执行顺序与门禁

### Phase 0：冻结合同与基线

交付物：

- 把本文件、`RazorVueM5CapabilityLedger`、作者诊断 ID 和四层证据字段作为同一份 backlog；新能力没有 ledger 行不得宣称 Support。
- 创建 natural-authoring baseline（优先从 JazorAdmin 抽取最小页面，不复制其 bridge），记录：手写
  `BuildRenderTree` 数量、应用侧 cast/bridge 数量、首次构建时间、浏览器 console error 数量。
- 固定 clean checkout、Release package consumer、Edge/Chrome/Chromium browser 和适用 SSR 的可复现命令。

门禁：文档/ledger 状态一致；现有 Emit `185/185`、Razor SG `4936/4936` 和 JazorAdmin package/browser
基线可重跑；无工作区意外改动。

### Phase 1：TDesign authoring friction

交付物：

- 先做 API review，再决定每个 bridge 是删除、上移到 TDesign binding、修复 RazorVue binding，还是保留为领域语义 wrapper。
- 覆盖 Button/Input/Form/Table/Modal 等高频组件的自然 Razor：generic/non-generic、typed slot、
  `@bind`、async EventCallback、union、required parameter、attribute splat 和命名冲突。
- 将通过的写法加入独立 authoring sample；不把页面 workaround 写成公共 API。

门禁：official SG 编译、semantic/module snapshot、Deno、真实 Chrome/Chromium（必要时 Edge）browser、isolated Release package consumer
全部通过；组件库内部之外的 RazorVue-specific symbol/cast/builder 为零。

### Phase 2：framework primitive

交付物：按 ZF-P1-01～07 顺序完成 ParameterView、DI、cascading、route/navigation、lifecycle；每项先
写行为矩阵和失败矩阵，再实现 lowering。

门禁：每项至少 source + official SG + module + runtime；adapter 触及浏览器或实例 lifetime 时必须有
真实 browser 和 package；SSR 形状必须有 hydration/副作用证明。

### Phase 3：DOM/reference/event 证据

交付物：Mouse/Keyboard/Focus/Change/ElementReference 与七类扩展事件的 browser-interactive 证据已完成；每组保留
carrier、允许 getter、明确拒绝的构造/setter/files/items 清单。

门禁：reference oracle 与真实 Chrome/Chromium（必要时 Edge）事件 journey 的可观察结果一致；package consumer 不依赖仓库源码或
未选择的资源入口；浏览器 console 无 error。七组扩展事件的当前 getter-only 子集已满足该门禁。

### Phase 4：SSR、认证与状态交接

交付物：先确定 endpoint/claims 和 serialized state 版本协议；在协议尚未存在时，认证与 SSR state/form
入口必须由 authored-source Guidance（`JAZORVCA007`/`JAZORVCA011`）明确阻断，并提供 typed endpoint/bootstrap
替代。协议完成后再实现 provider/hydration adapter；不借用 `AuthorizeView`、`EditForm` 或其他内置组件作为入口。

门禁：当前 Guidance 阶段要求诊断在 authored source 首次构建出现且无 partial artifact；升级为 adapter
前，匿名/认证/过期、SSR 首屏、hydration、刷新和状态失配必须均有 packaged consumer，服务端 endpoint
仍是授权事实来源。

### Phase 5：quickstart、包和发布质量门禁

交付物：

- `samples/RazorVue.Authoring` 与现有[快速开始指南](../03-guides/quick-start.md)对齐，并在该阶段补充
  RazorVue 专用作者入口；所有片段来自已通过测试的样例。
- 将 natural-authoring sample、JazorAdmin、TodoList 纳入同一套 Release/package/browser/SSR 入口，
  但每个 framework slice 仍保留独立证据。
- 记录性能、产物大小、模块闭包和首次成功构建指标；不把未选择的资源复制到发布目录。

门禁：clean checkout 能完成一个 CRUD/admin 页面；package consumer 0 warning/0 error；Chrome/Chromium（必要时 Edge）mount、
交互和适用 hydration 通过；失败在作者源或 final pipeline 明确出现。

## 7. 四层证据门禁

任何状态从 `InProof`/`Planned` 提升为 `Support`，都按下表逐层勾选；缺一层就保持原状态：

| 层 | 要证明的事实 | 典型入口 |
| --- | --- | --- |
| L1 作者源 / official SG | 标准 Razor/C# 能绑定，且生成 C# 形状稳定 | `src/Jazor.RazorVue.Sg.Test` authored fixtures、官方 SG 输出 |
| L2 语义 / artifact | `SemanticWalker`、RenderEmitter、imports、source map、module closure 保留求值顺序和 identity | `src/Jazor.CompilerTest`、`src/Jazor.EmitTest`、Razor SG module assertions |
| L3 reference / real browser | 与 Blazor reference oracle 和真实 Edge 可观察行为一致 | reference fixture、BrowserSmoke、console/network 检查 |
| L4 Release package / SSR | 从 NuGet/manifest 选定闭包可交付；适用时 SSR/hydration 一致 | isolated PackageConsumer、`verify-windows-spa-release.cs`、`verify-windows-ssr-release.cs` |

`Deno` 运行时测试是快速语义回归，不单独替代 L3/L4。静态 `.mjs` snapshot 也不能单独代表支持。

### 7.1 浏览器验证入口与职责

三条入口必须针对同一个已物化的 consumer harness（不能针对仓库源码目录拼接资源）：

| 入口 | 作用 | 约束 |
| --- | --- | --- |
| `.NET BrowserSmokeTestHelper` | CI/测试中的自动化 mount、事件和 DOM 断言 | 通过 `RAZORVUE_BROWSER_EXE`/`RAZORVUE_BROWSER_PATH` 固定可执行文件；默认按 Edge、Chrome、Chromium 顺序发现；临时 profile 与 harness 隔离 |
| Playwright CLI | 可复现的 Chrome/Chromium 快照、填充、点击、导航、截图和 console 检查 | 使用仓库 wrapper `C:/Users/hanxj/.codex/skills/playwright/scripts/playwright_cli.sh`；每次交互先 `snapshot`，DOM 变化后重新 snapshot；不把 `@playwright/test` spec 当作产品门禁 |
| `agent-browser` | 与 Playwright 隔离的 CDP session 复核，用于发现 session/焦点/真实 DOM 差异 | 使用独立 `--session` 和 Chrome engine；每次交互先 `snapshot -i`，只使用当前 ref；复核后记录 `console`/`errors`，不复用 Playwright 的 session state |

推荐的本地复核顺序：先运行对应的 Release/package 测试生成 harness，再用 Playwright CLI 完成
快照和交互，最后用 `agent-browser` 以新 session 重复关键 journey。`favicon.ico` 等静态 harness
噪声必须与 bundle/runtime error 分开记录；只有页面错误、未处理 rejection、资源闭包错误或
可观察行为不一致才阻断 Support 升级。

## 8. 量化指标

| 指标 | 基线/目标 | 解释 |
| --- | --- | --- |
| clean checkout 首次成功构建 | 记录基线；目标不超过 quickstart 可接受的一次构建 | 不能依赖开发者预装 Node 或手工复制资源 |
| 作者源码中的 RazorVue 内部符号 | 目标为 0（组件库/平台内部除外） | 不出现 builder、Vue AST、marker protocol、手写 module/import |
| 应用侧手写 `BuildRenderTree`/桥接组件 | 以 Phase 0 基线为准，Phase 1 后逐项下降 | 只有领域语义 wrapper 可保留并需 API review 记录 |
| known runtime-first failure | 目标为 0 | 未支持形状在 source/final pipeline 首次构建明确失败 |
| Release package consumer | 0 warning / 0 error | 包、manifest、buildTransitive 和闭包一致 |
| Browser console error | 0 | 包含 mount、导航、事件、异步更新和 hydration |
| Support 状态 | P0 全部 `Support` 或有证据的 `Compatibility Adapter` | `InProof` 不算完成；Reject 不计入目标范围 |

## 9. 变更决策与升级条件

- 发现新摩擦时，先归因到 compiler/runtime、binding/API 或 evidence/delivery，再分配唯一 owner；不在
  RazorVue 里手工拼 AST/JS 来绕过 `Jazor.Compiler` 语义。
- 同一问题若只在一个页面出现，先做最小自然 Razor fixture 和 API review；不能用 JazorAdmin 的历史
  bridge 反推公共契约。
- 每个 Support 升级必须同时更新 ledger、实现、测试、作者指南和当前状态；只更新路线文字无效。
- 新公共 API 或 lowering 能力走 MINOR 版本；纯修复/文档走 PATCH；破坏性 TDesign 命名迁移必须有
  CHANGELOG 和迁移样例。
- 内置组件、IJSRuntime、server-only service 的 Reject 只有在产品范围正式改变且重新评审安全/语义
  协议时才可升级；本计划默认不升级它们。

## 10. 最终退出标准

“目标范围内零摩擦”达成需要同时满足：

1. P0 作者面全部为 `Support` 或有完整证据的 `Compatibility Adapter`；TDesign typed authoring
   与应用自有 route/page host 均已完成真实浏览器闭环。
2. ParameterView、browser DI、cascading、navigation、核心 DOM event、ElementReference 的高频子集
   通过四层证据；未覆盖子集在 authored source 有稳定 Guidance/Reject。
3. TDesign 常见管理页面不再需要不必要的 bridge、cast、手写 builder 或弱类型逃生；自然 Razor
   sample 与 JazorAdmin 的行为一致。
4. clean checkout quickstart 能完成一次 CRUD/admin journey，Release package、browser 和适用 SSR/
   hydration 一致，console error 为零。
5. analyzer 与 final pipeline 无重复/partial artifact；所有失败在作者能理解的位置出现。
6. Microsoft/Blazor 内置 UI 组件和 `IJSRuntime` 等固定 Reject 被清楚显示，但不影响上述目标范围的
   零摩擦结论。

## 11. 相关文档与验证入口

- [RazorVue 开发者体验路线图](./razorvue-developer-experience.md)：完整 M5 ledger、作者面和诊断决策。
- [RazorVue 作者面诊断路线图](./razorvue-authoring-diagnostics.md)：`JAZORVCA`/`JAZORVGA` 规则与
  source/final pipeline 边界。
- [Blazor CLR 类型支持计划](./blazor-clr-support-plan.md)：CLR mapping、DOM event、navigation、
  ElementReference 和明确 Reject 面。
- [JazorAdmin 生产级参考应用路线图](./admin-reference-app.md)：真实消费者定位；不以页面 workaround
  定义公共 API。
- [当前状态](./current-status.md)：已完成交付基线和可复现质量门槛。
- `dotnet test src/Jazor.RazorVue.Sg.Test/Jazor.RazorVue.Sg.Test.csproj`
- `dotnet test src/Jazor.EmitTest/Jazor.EmitTest.csproj`
- `dotnet run --file scripts/csharp/verify-windows-spa-release.cs -- --path-base /docs`
- `dotnet run --file scripts/csharp/verify-windows-ssr-release.cs -- --path-base /todo`
