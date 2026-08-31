# RazorVue “零摩擦”执行计划

> 日期：2026-08-31
> 状态：执行计划，不是 Support 声明。每项能力仍以 `RazorVueM5CapabilityLedger` 和本文件的验收门禁为准。
> 目标范围：标准 Blazor **自定义组件作者面** + TDesign 等第三方 typed component library；Microsoft/Blazor 内置 UI 组件不在本计划支持范围内。

## 0. 结论先行

规范化和交付链路已经达到可用基线：Emit 回归为 `180/180`，Razor Source Generator 回归为
`4907/4907`；JazorAdmin 的 Release 本地 NuGet consumer（native 与 VueInject）均为 0 warning/0
error，最终 Edge mount smoke 也已通过。`vue-data-ui` 的 71 个入口相对 ESM 闭包已经补齐。

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
| `@page`/`@layout`/route metadata 与页面状态 | `InProof` | route catalog 输出与 Deno 场景 | 真实浏览器路由切换、not-found、query refresh 和错误/重试流程 |
| `ParameterView`/`SetParametersAsync` | `InProof` | sparse/alias/slot/queue adapter 测试 | 完整 snapshot、异常传播、browser/SSR consumer |
| browser `[Inject]` property adapter | `InProof` | provider、lifecycle、missing-provider 测试 | lifetime、嵌套实例、browser/package/SSR 证明；constructor injection 保持 Reject |
| cascading values | `InProof` | typed/named/nested Deno 测试 | `IsFixed`、更新传播、scope 与 browser/package 证明 |
| `NavigationManager` 与 LocationChanging | `InProof` | URI/navigate 基础映射与 CLR whitelist | reference oracle、真实 browser、Release consumer；popstate/hashchange cancellation 未承诺 |
| Mouse/Keyboard/Focus/Change | `InProof` | CLR mapping、official SG 部分切片、Deno | 完整 handler 矩阵、browser、事件专用 package consumer |
| Pointer/Wheel/Drag/Clipboard/Touch/Error/Progress | `InProof` | generator、CLR、official SG、Deno 最小垂直切片 | 每组 reference/browser/package 证明；构造器/setter/files 等保持拒绝 |
| `@ref`/`ElementReference.FocusAsync` | `InProof` | VNode ref 与 mapping 测试 | 空/未挂载语义、真实 browser、Release consumer |
| TDesign typed API | 组件覆盖已较完整；作者体验 `InProof` | 118 runtime component binding、泛型 table cell 测试 | 自然 Razor 的 generic/non-generic、slot、union、bind、required 和命名冲突矩阵 |
| package/artifact/HMR | 交付基线已可靠 | Emit `180/180`、JazorAdmin package/browser gate、ESM closure | 每个新增 framework slice 仍需独立 consumer；SSR feature proof 不可借用总 gate |
| authentication state、SSR state/form handoff | `Planned` | 无行为证据 | 需要版本化 host protocol；内置认证/表单组件仍不在范围 |

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
| ZF-P0-01 | TDesign 页面需要 `AdminInput`/`AdminForm` 等薄桥接，作者被迫接触 callback 转发和 cast。当前消费者可运行，但桥接必要性未裁决。 | Binding/API；`ECMAScript.TDesign` + `Jazor.RazorVue` | 建立同一页面的 native TDesign 与 bridge 对照；先修公共 prop/event contract，再删除可替代 wrapper。依赖官方 SG binding。 | 新增 natural-authoring fixture：无手写 builder/cast 即完成 CRUD；Release browser smoke 与 package consumer 通过。目标：`Support`。 |
| ZF-P0-02 | `TPrimaryTable<T>` 的泛型/非泛型入口、typed cell/row slot、`RenderFragment<T>` 和 EventCallback 在 Razor 中仍可能需要固定 wrapper。 | Binding/API + RazorVue component binding | 固定 TypeInference、开放泛型、slot context、delegate variance 和跨模块 selector 的行为矩阵；不以 `object` 放宽契约。依赖 P0 generic/fragment 已有 lowering。 | 覆盖 table columns、typed slots、empty/loading、row event、bind、required 参数的 authored + official SG + Deno + browser 用例；目标：`Support` 或有明确 `Guidance`。 |
| ZF-P0-03 | 同一 JS prop 的 C# 后缀（如 `LoadingValue`/`LoadingContent`）和 union branch assignment 增加记忆负担。 | Binding/API；`ECMAScript.TDesign` | 做命名/union API review：优先不冲突的强类型命名、native union 或 tagged fallback；保留 Razor SG 可绑定性和兼容成员，不增加弱类型 escape hatch。 | 生成 contract 报告；常见 Button/Table/Form/Modal 写法无需 cast/`From`；breaking change 按 MINOR 和迁移说明发布。目标：`Support`。 |
| ZF-P0-04 | `@page`、layout、query/route 参数和 loading/error/retry 页面尚缺完整浏览器闭环，ledger 为 `InProof`。 | Compiler/runtime；RazorVue route host | 完成 route catalog -> host state -> component activation 的单一协议；明确 not-found、重复导航、query refresh 和取消行为。Router/RouteView/LayoutView/NavLink 不进入实现。 | route browser journey、official SG、module/source-map、package consumer；SSR 适用时补 hydration。目标：`Support`/`Compatibility Adapter`。 |
| ZF-P0-05 | 作者源码诊断已覆盖 D0-D5，但新失败仍可能在 generated C# 才暴露，或 analyzer/final pipeline 重复报告。 | Diagnostics；`Jazor.RazorVue` + analyzer | 为每个新边界先登记 ledger；author-source 高置信诊断与 final Compilation 单次裁决分层，保留 `JAZORVGA020`-`026` 稳定分类。 | source location、dedupe、无 partial ModuleCatalog、HelpLink 和最小替代回归；目标：Direct Support 零噪音，其他形状首建可解释。 |
| ZF-P0-06 | 没有独立、最小、Blazor-first 作者样例来证明“不需要 JazorAdmin 经验”。 | Evidence/delivery；`samples/RazorVue.Authoring`（待建） | 建立 Todo/CRUD 页面：TDesign table/form/dialog、typed slot、`@bind`、async callback、DI、route；样例不得使用内置组件或历史 bridge。依赖 ZF-P0-01/02 的 API review。 | clean checkout -> local package -> Release browser smoke；记录首次成功构建时间和作者源内部符号计数。目标：样例只消费已达 `Support` 的能力。 |

### P1：补齐高频 framework primitive 的语义闭环

| ID | 用户摩擦 / 当前状态 | 类型与 owner | 实施方向和依赖 | 验收与退出状态 |
| --- | --- | --- | --- | --- |
| ZF-P1-01 | `SetParametersAsync(ParameterView)` 已能处理部分 sparse/slot/queue 场景，但任意 snapshot/异常行为仍不完整。 | Compiler/runtime；ParameterView adapter | 固定 CLR default -> base apply -> overlay -> lifecycle -> queued update 的顺序；只实现已定义的成员，`TryGetValue`/枚举/`ToDictionary` 继续 Guidance。 | `RazorSgSetParametersAsyncRuntimeTests` 扩展异常、取消、重复更新和跨实例用例；browser + SSR package consumer；目标：`Support`。 |
| ZF-P1-02 | `[Inject]` browser service 可运行，但 provider lifetime、缺失 provider、嵌套组件和重渲染边界尚缺交付证明。 | Runtime/host；`VueInjectRegistry` + `VueModuleBuilder` | 明确实例级 registration、初始化/Dispose 顺序和 async callback 竞态；constructor injection、primary constructor、`this(...)`、`base(args)` 保持 `Reject`。 | official SG + Deno + browser + isolated Release package + SSR（适用时）；目标：`Support`。 |
| ZF-P1-03 | cascading 参数已有 Deno adapter，但 nested override、named provider、`IsFixed` 和更新传播未完成。 | Runtime；cascading adapter | 以 Vue scope 建立 typed/named provider；一次更新只触发必要 consumer；不可写属性继续 `JAZORVCA008`。 | nested scope、replacement、same-reference、dispose、browser/package 回归；目标：`Support`。 |
| ZF-P1-04 | `NavigationManager` 基础 URI/navigate 可用，LocationChanging 与浏览器 history 语义仍为 `InProof`。 | CLR/runtime + route host | 依照 reference oracle 定义 handler 顺序、取消、registration dispose、rapid navigation；不承诺 server circuit 或 popstate/hashchange 取消，除非另有协议。 | reference oracle 对照、真实 Edge history/hash/query journey、isolated package consumer；目标：声明支持的子集 `Support`，未实现部分稳定 `Guidance/Reject`。 |
| ZF-P1-05 | Mouse/Keyboard/Focus/Change 事件及 typed callback 还缺完整 browser/package 证明。 | CLR mapping + RenderEmitter | 保持 native event carrier；`ChangeEventArgs.Value` 只在事件时间 capture，不引入 synthetic Args 构造或 setter。 | 每个事件的 authored/official SG/module/Deno/browser/package fixture；目标：`Support`。 |
| ZF-P1-06 | `@ref`/`ElementReference.FocusAsync` 已有 mapping，但空 ref、未挂载和卸载时行为未裁决。 | CLR mapping + Vue ref framing | 固定 ref callback 生命周期和 Promise/ValueTask 失败语义；不扩展为任意 DOM 方法或 `new ElementReference`。 | mount/update/unmount/empty ref 的真实 browser + package consumer；目标：`Support`。 |
| ZF-P1-07 | 普通 lifecycle 已 Support，异步异常、取消、重复 render、Dispose race 在复杂组件中仍可能产生隐性差异。 | Runtime；`VueModuleBuilder`/host | 用 generation-aware guard 和明确 queue 顺序处理可观察副作用；不增加未要求的全局防御式重试。 | lifecycle reference fixture、rapid update/unmount browser journey、SSR 不重复副作用；目标：`Support`。 |

### P2：扩大真实生产覆盖，但不扩大内置组件范围

| ID | 用户摩擦 / 当前状态 | 类型与 owner | 实施方向和依赖 | 验收与退出状态 |
| --- | --- | --- | --- | --- |
| ZF-P2-01 | Pointer/Wheel/Drag/Clipboard/Touch/Error/Progress 已有最小垂直切片，仍缺逐组真实证据。 | CLR mapping；`Jazor.CLR` + generator | 按事件组补 reference/browser/package；构造器、setter、files/items、非 getter TouchList 操作继续拒绝。 | 每组独立 consumer、Edge smoke、Deno/reference 对照；目标：逐组 `Support` 或明确 `Guidance/Reject`。 |
| ZF-P2-02 | 认证状态尚未有浏览器 provider 与服务端 endpoint/claims handoff；ledger 为 `Planned`。 | Host/runtime；ASP.NET Core + RazorVue | 只定义 typed `AuthenticationStateProvider`/claims payload 和 endpoint enforcement；不实现 `AuthorizeView` 等内置组件，不把 UI 隐藏当授权。 | authenticated JazorAdmin journey、匿名/过期/刷新/403、SSR 到 hydration state 对照；目标：`Compatibility Adapter` 或明确不支持子集。 |
| ZF-P2-03 | `PersistentComponentState`、hydration state 和 SSR/增强 post handoff 尚无版本化协议；ledger 为 `Planned`。 | SSR/host；`Jazor.AspNetCore` + `Jazor.Emit` | 先实现可序列化、版本化的 state payload 和 hydration checksum；内置 `EditForm`/antiforgery/enhanced form protocol 不纳入，表单 UI 由 TDesign 等承担。 | packaged SSR HTML、Edge hydration、刷新/重复提交/状态失配 fixture；目标：适用形状 `Support`，其余 `Guidance/Reject`。 |
| ZF-P2-04 | 组件数量、slot 深度和高频更新增长后，作者不应为性能手动改写 API。 | Runtime/perf；`RenderEmitter` + host | 以真实样例 benchmark 决定 block/patch/handler cache；不为“看起来像 Vue”引入破坏语义的优化。 | P0/P1 authoring sample 与 JazorAdmin 的 render/update/SSR 指标、无行为回归；目标：性能门禁稳定，未达标项不阻塞语义 Support 但必须有 issue。 |

## 6. 分阶段执行顺序与门禁

### Phase 0：冻结合同与基线

交付物：

- 把本文件、`RazorVueM5CapabilityLedger`、作者诊断 ID 和四层证据字段作为同一份 backlog；新能力没有 ledger 行不得宣称 Support。
- 创建 natural-authoring baseline（优先从 JazorAdmin 抽取最小页面，不复制其 bridge），记录：手写
  `BuildRenderTree` 数量、应用侧 cast/bridge 数量、首次构建时间、浏览器 console error 数量。
- 固定 clean checkout、Release package consumer、Edge browser 和适用 SSR 的可复现命令。

门禁：文档/ledger 状态一致；现有 Emit `180/180`、Razor SG `4907/4907` 和 JazorAdmin package/browser
基线可重跑；无工作区意外改动。

### Phase 1：TDesign authoring friction

交付物：

- 先做 API review，再决定每个 bridge 是删除、上移到 TDesign binding、修复 RazorVue binding，还是保留为领域语义 wrapper。
- 覆盖 Button/Input/Form/Table/Modal 等高频组件的自然 Razor：generic/non-generic、typed slot、
  `@bind`、async EventCallback、union、required parameter、attribute splat 和命名冲突。
- 将通过的写法加入独立 authoring sample；不把页面 workaround 写成公共 API。

门禁：official SG 编译、semantic/module snapshot、Deno、Edge browser、isolated Release package consumer
全部通过；组件库内部之外的 RazorVue-specific symbol/cast/builder 为零。

### Phase 2：framework primitive

交付物：按 ZF-P1-01～07 顺序完成 ParameterView、DI、cascading、route/navigation、lifecycle；每项先
写行为矩阵和失败矩阵，再实现 lowering。

门禁：每项至少 source + official SG + module + runtime；adapter 触及浏览器或实例 lifetime 时必须有
真实 browser 和 package；SSR 形状必须有 hydration/副作用证明。

### Phase 3：DOM/reference/event 证据

交付物：先完成 Mouse/Keyboard/Focus/Change/ElementReference，再逐组推进七类扩展事件；每组保留
carrier、允许 getter、明确拒绝的构造/setter/files/items 清单。

门禁：reference oracle 与 Edge 真实事件 journey 的可观察结果一致；package consumer 不依赖仓库源码或
未选择的资源入口；浏览器 console 无 error。

### Phase 4：SSR、认证与状态交接

交付物：先确定 endpoint/claims 和 serialized state 版本协议，再实现 provider/hydration adapter；不
借用 `AuthorizeView`、`EditForm` 或其他内置组件作为入口。

门禁：匿名/认证/过期、SSR 首屏、hydration、刷新和状态失配均有 packaged consumer；服务端 endpoint
仍是授权事实来源。

### Phase 5：quickstart、包和发布质量门禁

交付物：

- `samples/RazorVue.Authoring` 与现有[快速开始指南](../03-guides/quick-start.md)对齐，并在该阶段补充
  RazorVue 专用作者入口；所有片段来自已通过测试的样例。
- 将 natural-authoring sample、JazorAdmin、TodoList 纳入同一套 Release/package/browser/SSR 入口，
  但每个 framework slice 仍保留独立证据。
- 记录性能、产物大小、模块闭包和首次成功构建指标；不把未选择的资源复制到发布目录。

门禁：clean checkout 能完成一个 CRUD/admin 页面；package consumer 0 warning/0 error；Edge mount、
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

1. P0 作者面全部为 `Support` 或有完整证据的 `Compatibility Adapter`；route/page 也有真实浏览器
   闭环。
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
