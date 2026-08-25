# RazorVue Blazor CLR 类型支持计划

> 状态：规划中。发布基线为 `v0.19.0`，开发审阅基线为 2026-08-24 的工作树。只有机器可核验 ledger、实现、目标 profile 证据和面向用户文档一致时，能力才能标记为 Support。
>
> 定位：这是 [RazorVue 开发者体验完善路线图](./razorvue-developer-experience.md) 中浏览器运行时类型与服务的专项实施计划。它不试图把整个 ASP.NET Core/Blazor runtime 映射到 JavaScript。

> **范围决策（2026-08-25）**：本计划支持 Blazor framework 的 authoring/runtime contract，以及自定义组件和第三方组件库所需的 lowering/runtime primitive；不支持 `Microsoft.AspNetCore.Components` 提供的内置 UI 组件。`Router`、`RouteView`、`NavLink`、`DynamicComponent`、`ErrorBoundary`、`EditForm`、`Input*`、`AuthorizeView`、`Virtualize`、`QuickGrid` 等标签不属于本计划的产品契约。UI 组件层由现有的 TDesign、Vuetify、Element Plus 等绑定/组件库承担。标准组件若仍被识别到，最终应走稳定的 Reject/Guidance 诊断，不得静默生成“部分兼容”的 Vue 替代品。
> **RazorVue 组件入口**：可被本计划消费的组件类型必须可赋值给 `ComponentBase`、实现 `IVueComponent` 或其派生接口，并带有 `[ECMAScriptModule]` 或 `[VueLibraryComponent]` 导入描述。`ComponentBase`/`IComponent` 本身仍属于 framework primitive；仅有 `ComponentBase` 但没有 Vue marker 的 Microsoft 内置 UI 组件不进入本计划。

本计划采用“一步到位的归属、分切片的实现”：Blazor framework 的类型扩展声明从第一版就属于 `ECMAScript.Blazor`，不先在 `Jazor.CLR` 建一套再迁移；需要实际浏览器行为的 runtime module/helper 仍直接属于 `Jazor.CLR`。S0–S6 以及后续框架级切片仍按各自的 reference、browser、package 和 profile 门禁逐步交付，不等于一次性实现全部 Blazor API，也不为内置 UI 组件建立兼容路线。

## 1. 目标与范围

RazorVue 的目标是让 Blazor framework 的作者面在浏览器中保持可观察行为，而不是复制 server renderer、circuit、内置组件库或完整 CLR 对象模型。本计划只覆盖自定义组件 C# 逻辑确实需要消费、且能够在浏览器中建立稳定 carrier 与行为合同的 framework 类型：

- official Razor Source Generator 生成形状、`ComponentBase` 生命周期、参数/fragment/event callback 等自定义组件 lowering primitive；
- 导航拦截所需的 `ValueTask`、`LocationChangingContext` 和关联注销/取消协议；
- DOM 事件参数对象：原生 DOM event 作为 carrier，由 `ECMAScript.Blazor`（随 `Jazor.Vue` 交付）声明 Blazor 成员投影，实际 helper/module 仍由 `Jazor.CLR` 承载；
- `ElementReference` 的浏览器操作；
- 受控的 JS interop 对象与回调协议；
- 浏览器认证状态 provider 的 API（不包含 `AuthorizeView` 或其他认证 UI 组件）。

以下内容不因名称属于 Blazor 就进入本计划的内置组件兼容范围：`Router`、`RouteView`、`LayoutView`、`NavLink`、`DynamicComponent`、`ErrorBoundary`、`EditForm`、`InputBase<T>`/`Input*`、`ValidationMessage`、`AuthorizeView`、`CascadingAuthenticationState`、`Virtualize<TItem>`、`QuickGrid<TGridItem>`、`SectionOutlet`/`SectionContent` 以及 `InputFile`/`IBrowserFile` 组件组合。`ComponentBase`、`EventCallback`、`RenderFragment`、`ParameterView`、`RenderTreeBuilder` 等 framework primitive 仍可由 `Jazor.RazorVue` 的 current-component lowering、render emitter 或运行时桥接消费，但这不等于承诺对应标准组件标签。

### 1.1 发布基线与开发基线

| 范围 | `v0.19.0` 发布状态 | 当前开发基线 | 本计划中的位置 |
| --- | --- | --- | --- |
| `NavigationManager` 基础导航、`LocationChangedEventArgs`、`NotFoundEventArgs`、URL-backed `System.Uri` | `v0.19.0` 已包含对应成员/runtime，但更宽的 routing family 仍由 M5 ledger 标记为 InProof | 沿用既有 runtime，并增加导航取消能力所需的 host 状态 | S1 的基础，不重复实现 |
| `System.Threading.Tasks.Task`、`Task<TResult>` | 已有 Promise carrier 与受控成员面 | 不因新切片自动扩大 Task API | S1/S3/S5 只复用已批准路径 |
| `RegisterLocationChangingHandler(...)`、`LocationChangingContext` | 未发布 | CLR mapping、navigation state machine、compiler tests 和 official Razor SG + Deno runtime tests 已落地 | **InProof**：缺标准 Blazor reference oracle、真实浏览器与 package consumer 证据 |
| 非泛型 `ValueTask` | 未发布 | 导航 handler 所需最小 Promise carrier、metadata/runtime/compiler tests 已落地 | **InProof**：只作为已批准 async 路径的依赖，不代表完整 `ValueTask` 支持 |
| `CancellationToken` / `CancellationTokenSource` / `CancellationTokenRegistration` | 未发布 | `AbortSignal` / `AbortController` / inferred nominal carrier 及对应成员族已落地 | **InProof**：只承诺已验收的取消切片 |
| `MouseEventArgs`、`KeyboardEventArgs`、`FocusEventArgs` | Razor SG 可绑定 handler；首批 DOM carrier 与只读 getter mapping 已落地 | **InProof**：mapping declaration、source-root 和 compiler regression 已有；reference/browser/package 证据仍待补齐 | S2：核心事件 |
| `ChangeEventArgs` | Razor SG 可绑定 handler；`Value` 已通过 listener 边界 capture 与 CLR `WeakMap` helper 投影 | **InProof**：mapping、runtime module、compiler wrapper 与 official Razor SG typed handler 回归已落地；真实 BrowserSmoke 与 package consumer 证据仍待补齐 | S2：核心事件 |
| `@ref` capture / `ElementReference.FocusAsync` | capture 已由 render emitter 支持；`ElementReference` 已映射为 `HTMLElement`，两个 `FocusAsync` overload 已接入 `HTMLElement.Focus` | **InProof**：mapping/compiler/official Razor SG 证据已落地，真实 browser 与 package consumer 仍待补齐 | S3：只补受控 DOM 操作 |
| `IJSRuntime` / `IJSObjectReference` | 属性注入 framing 可存在，但没有默认可执行 interop contract | M5 ledger 仍为 Guidance；没有 typed identifier/module contract | S5：先做可行性与合同裁决，不预先承诺 Support |
| `AuthenticationStateProvider` | 没有默认 browser provider | 通用 browser service injection 不等于认证 UI 已实现；只评估 provider/state API | S6：认证状态 API 垂直切片，排除 `AuthorizeView` |
| `EditContext`、`FieldIdentifier`、`ValidationMessageStore`、`InputFile` | 不在本框架计划中；已有标准输入适配器属于遗留实验/组件兼容工作，不构成 Support | 由 TDesign/Vuetify/Element Plus 的强类型表单组件或独立组件兼容路线承担 | Out of scope；不安排 S7/S8 实现 |

开发基线上的实现事实、发布状态和支持决策必须分开记录：白名单存在不等于 Support，Deno runtime 通过也不等于 BrowserSmoke；反过来，已经落地的 runtime/test 也不能继续写成“无映射”。源码证据应引用稳定的类型、成员和测试名，不引用易漂移的 `WhiteList.cs.Generate.cs` 行号。

当前还有一处需要在 S0 解决的状态不一致：`README.md` 与 `CHANGELOG.md` 的 Unreleased 段已把 location-changing navigation 描述为支持，而 `RazorVueM5CapabilityLedger` 只有覆盖更宽 routing family 的 `P1-navigation-router` row，仍为 InProof；仓库内也没有 location-changing 切片的 BrowserSmoke/PackageConsumer 证据。进入发布前必须二选一：为该切片建立/关联独立 ledger row，补齐证据并提升为 Support；或把面向用户声明改成 InProof/尚未发布。不能直接把整个宽泛 routing row 提升为 Support，也不能由本计划文字单方面覆盖另一个事实源。

### 1.2 支持等级与运行 profile

本计划沿用 M5 的两组正交概念。第一组是**支持决策**：

| 等级 | 含义 |
| --- | --- |
| Direct Support | 标准 C# API 直接映射，浏览器行为已被证明。 |
| Compatibility Adapter | 作者源码不变，RazorVue runtime 吸收浏览器和 Blazor 的实现差异。 |
| Guided Adaptation | 无法保真，但存在明确、强类型的浏览器替代。 |
| Reject | 无稳定浏览器语义或会破坏确定性，必须在使用点失败。 |

第二组是**交付状态**，与 `RazorVueCapabilityStatus` 对齐：

| 状态 | 含义 |
| --- | --- |
| Planned | 已有范围与 owner，但实现或证据尚未开始。 |
| InProof | 已有部分实现/证据；任何缺失的 reference、browser、package 或 profile 证据都会停留在此状态。 |
| Support | 实现已合并，目标 profile 的证据链完整，ledger 与当前文档一致。是否已随 NuGet 发布另由 tag/CHANGELOG 记录。 |
| Guidance | 选择强类型替代路径，不宣称标准 API 直接可执行。 |
| Reject | 在作者源或最终使用点稳定失败。 |

`RazorVueCapabilityEvidence` 已有 `AuthorSource`、`OfficialRazorSourceGenerator`、`ModuleArtifact`、`DenoRuntime`、`BrowserSmoke`、`SsrHydration` 和 `PackageConsumer`。每个切片只声明适用证据，但 Browser interactive Support 至少需要前五项与 PackageConsumer；Deno 不能冒充真实浏览器。

本文章节中的 `P0`–`P3` 是本计划的路线优先级带，不等同于 `RazorVueCapabilityPriority` enum 或 M5 ledger 的 `P*-...` row（当前 enum 只到 P2）；`S0`–`S8` 才是可独立验收的交付切片。两套编号不能互相推导。一个计划优先级带可以包含多个切片（例如 P1 包含 S2/S3），切片状态仍以 §11.1 和关联 ledger 为准。

每个切片必须分别标明适用 profile：

| Profile | 规则 |
| --- | --- |
| Browser interactive | 本计划的主目标；必须有真实浏览器回归。 |
| SSR/prerender + hydration | 仅在切片显式完成 payload、一次性副作用和 hydration 时序证明后支持。 |
| Interactive Server / server-hosted reference | 只作为 Blazor 行为 oracle 和 API 迁移参考，不扩大 RazorVue 浏览器支持范围。 |
| Static/non-interactive render | 仅支持没有交互副作用的页面输出；事件和交互指令必须有明确诊断或对应的 render-mode adapter，不能静默输出看似可点击但没有处理器的 DOM。 |

## 2. 所有权与实现约束

```text
标准 Razor/C# 作者代码
  -> official Razor Source Generator
  -> Jazor.RazorVue: 识别 Vue/DOM/组件边界，建立 runtime bridge
  -> Jazor.Compiler / SemanticWalker: C# 调用、成员、类型、导入和失败裁决
  -> Jazor.CLR: CLR runtime module/helper 的 C# 实现与物化
  -> ECMAScript.Blazor（随 Jazor.Vue 交付）: Blazor framework 成员到浏览器 carrier 的 mapping declaration
  -> Jazor.Emit: .mjs、source map、manifest、bundle 物化
```

| 层 | 本计划中的责任 | 禁止的做法 |
| --- | --- | --- |
| `ECMAScript.Blazor` | Blazor 专属 CLR 类型/成员的 `Alias`、`Inline`、必要的 `Compile` mapping declaration；声明使用哪个原生 carrier 和成员路径 | 放 runtime module/helper、事件捕获状态机或手写 `.mjs`；用普通扩展方法冒充原始 framework getter 的 lowering |
| `Jazor.CLR` | Blazor 与 BCL 共用 carrier，以及确有复杂行为的 Blazor C# runtime module/helper；由现有 catalog/Emit 管道物化模块 | 把 mapping declaration 复制到第二处，或把 helper 改成手写 `.mjs` |
| `Jazor.Compiler` / `SemanticWalker` | 所有 C# 表达式、调用、成员访问、导入收集和使用点失败 | 对未映射外部成员静默发射原始 JavaScript |
| `Jazor.RazorVue` | Vue listener 的原生 event 传递、`@ref` 生命周期、Vue `provide`/`inject`、组件/路由/表单 framing | 为每种 `EventArgs` 手工构造 payload，用手拼 JS 替代 C# 成员/函数语义 lowering，或把导航、认证、表单状态机新增到既有 hand-written runtime `.mjs` |
| `Jazor.Emit` | 产物和 runtime closure 物化 | 在 RazorVue 中直接写入文件或绕过 manifest |

所有类型必须以完整垂直切片交付。一个类型仅有白名单 key、一个空对象 Alias，或仅能通过 Razor 编译，都不构成 Support。

### 2.1 映射包可以拆分，但不能只靠普通扩展方法

从长期归属看，把 Blazor 专属的 `ChangeEventArgs`、`MouseEventArgs`、`KeyboardEventArgs`、`FocusEventArgs` 等**类型映射声明**放进独立的 `ECMAScript.Blazor` 是更好的边界。它与 `ECMAScript.Vue`、`ECMAScript.Pinia` 同属 ECMAScript host binding 层：提供把 framework 类型投影到原生 browser carrier 的强类型声明和适配器；实际 CLR runtime module/helper 仍由 `Jazor.CLR` 作为唯一 owner 提供，基础 `Jazor` 保持框架无关。交付上，`ECMAScript.Blazor` 是独立项目/程序集，但作为 `Jazor.Vue` NuGet 的 payload（或由它锁定同版本的传递依赖）带入最终消费项目；外部契约固定为“安装 `Jazor.Vue` 才获得这组映射”，用户不需要另行引用或复制映射源码。`Jazor` 核心包不得包含 `ECMAScript.Blazor` 程序集或其 Blazor reference 资产，也不应因此引入 Blazor framework 依赖。这样既保持项目边界，又避免把同一份声明复制进 `Jazor.Vue` 源码。

交付拓扑必须固定为下表，不能把“独立项目”误解为“核心包自动安装”：

| 项目/程序集 | 负责内容 | NuGet 交付位置 | 不应发生的事 |
| --- | --- | --- | --- |
| `Jazor` | 框架无关的 compiler、Emit、基础 contract/runtime，以及现有 `Jazor.CLR` runtime modules | `Jazor` 包 | 不包含 `ECMAScript.Blazor` DLL 或其 mapping declarations；已有 `Jazor.CLR` runtime 不因拆分而复制 |
| `ECMAScript.Blazor` | Blazor framework type -> browser carrier 的 mapping contribution 与 adapter declaration | 独立源码/程序集；打入 `Jazor.Vue` NuGet payload，或由 `Jazor.Vue` 以同版本传递依赖带入 | 不放 runtime module/helper，不把 mapping 源码复制到 `Jazor.Vue`，不维护第二份 member table |
| `Jazor.CLR` | 实际 CLR runtime module、共用 carrier、必要的 Blazor 专属 helper | `Jazor` 核心包已有的 runtime source/catalog；按模块闭包物化 | 不把 Blazor mapping declaration 再复制一份 |
| `Jazor.Vue` | RazorVue analyzer、build-transitive 注册和 Vue listener/component framing | `Jazor.Vue` 包；负责带入上面的 `ECMAScript.Blazor` 资产 | 不让 `Jazor.Vue` 私自维护第二份 Blazor member 表或手写 `.mjs` 状态机 |

在当前静态 whitelist 架构下，`Jazor.Vue` 带入程序集本身仍不足以改变 `Jazor.Compiler`；必须先落地 §2.1 的 contribution contract，或由 `Jazor.Vue` 的 analyzer 在 compilation 内显式合并该 provider，不能把映射反向塞回 `Jazor` 包。

目标归属按“framework mapping 与宿主 framing 分离”固定：

| 能力 | `ECMAScript.Blazor` mapping owner | runtime/helper 与宿主 framing |
| --- | --- | --- |
| S1 导航类型与 location-changing 成员 | Blazor framework type/member mapping declaration | `NavigationManager`、`LocationChangingContext`、handler/registration、取消与 commit helper 仍在 `Jazor.CLR`；Router mount/unmount framing 在 `Jazor.RazorVue` |
| S2/S4 DOM `EventArgs` | framework getter alias 与 native carrier mapping；`ChangeEventArgs` 只在 capture contract 完成后增加原始 getter declaration | Vue listener 注册和一次 capture 调用在 `Jazor.RazorVue`；`WeakMap`/值转换与 event helper 在 `Jazor.CLR` |
| S3 `ElementReference` | Blazor extension member 到 WebIDL DOM 操作的 mapping | `@ref` 生命周期和 VNode ref callback 在 `Jazor.RazorVue` |
| S5–S6 interop 与认证状态 API | Blazor framework API 的 typed mapping、carrier 与 provider contract | Vue provide/inject、host registration 和 SSR handoff 分别由 `Jazor.RazorVue`/`Jazor.Emit` 负责；不实现标准表单/路由/认证 UI 组件 |

因此“随 `Jazor.Vue` 安装”只改变交付入口，不改变 owner：`Jazor.Vue` 不复制这些声明，`ECMAScript.Blazor` 也不承担 Razor renderer、Vue 组件实现或 runtime module。

`ECMAScript.Blazor` 对 `Microsoft.AspNetCore.Components*` 的引用必须限制在 provider 自身的兼容性/分析上下文；不能为了加载它而把 Blazor server/runtime 依赖塞进 `Jazor` 核心包。缺少匹配的 ASP.NET Core reference version 时，应在 compilation/package restore 阶段给出明确版本诊断，而不是退化成 `object` 或静默跳过 mapping。

这里的“扩展”应理解为**适配器声明的扩展**，而不是面向作者的普通 C# extension method：

- `MouseEventArgs.ClientX` 等属性在作者源码中仍绑定到 framework 类型原有的 getter symbol。普通扩展方法只能增加另一个可调用方法，不能替换这个 getter 的 lowering；适配器必须以 `[Jazor]` 记录原始 CLR member key，并声明 WebIDL receiver、`Alias`/`Inline`/`Import` 行为。
- `ChangeEventArgs.Value` 更不能只加一个 `GetValue()` 扩展方法。原生 `Event` 没有顶层 `value`，且 async handler 恢复前 target 可能已经变化；它需要在 listener 边界捕获一次并以 `WeakMap` 保存事件时刻的值，再由原始 `Value.get` 映射读取。这个 capture hook 是事件协议的一部分，不是普通扩展方法能隐式提供的能力。
- 因此 adapter 可以在包内使用静态扩展风格的 C# helper 来组织实现，但不能把扩展方法名称当作 public Blazor API，也不能借助运行时字符串 registry、payload wrapper 或 hand-written `.mjs` 来“补映射”。

一个正式的 `ECMAScript.Blazor` mapping contribution 至少应同时拥有：

1. `[Jazor]` CLR type/member declarations，以及稳定的生成 mapping catalog（type alias、member `Op`/path、必要的 compiler hook）；
2. 对应的 runtime/helper 若需要复杂行为，必须在 `Jazor.CLR` 的 `[ECMAScriptModule]` C# 源码中实现，并由现有 catalog/Emit 管道物化；
3. build-transitive 注册，使 compiler/RazorVue 能在**当前 compilation**发现 mapping contribution；runtime artifact、manifest closure 和 import-map 仍沿用 `Jazor.CLR` provider；
4. Jazor/compiler contract version、ASP.NET Core target compatibility、provider id 和冲突诊断；重复 key 必须确定性合并或在编译期失败，不能由程序集加载顺序决定结果；
5. CLR metadata、compiler emission、official Razor Source Generator、Deno、真实 browser 以及 isolated Release package consumer 的证据。

建议的职责切分是：`ECMAScript.Blazor` 声明 framework-to-browser 的 native carrier/adapter surface，`Jazor.CLR` 承载跨 Blazor 与 BCL 共用的 CLR 语义和所有实际 runtime module/helper，`Jazor.RazorVue` 只负责 listener/callback framing 和 Vue 生命周期。`ChangeEventArgs` 的 event-time capture 若只服务 Blazor，应由 `ECMAScript.Blazor` 声明原始 getter contract，由 `Jazor.CLR` 实现 C# capture/helper；若以后被两个以上 host 复用，再上提为 `Jazor.CLR` 的通用 primitive。这样“类型映射库”不会变成第二个 Razor renderer，也不会把 Vue 特定逻辑倒灌进 ECMAScript binding。

当前仓库只有 runtime 物化一侧接近这个形态：`Jazor.Emit` 可以从 `JazorArtifactProviderAssembly`/runtime catalog 读取外部 provider。编译器白名单仍由 `Jazor.Compiler.Generator` 的固定源码根生成到 `WhiteList.cs.Generate.cs`，`SemanticWalker` 消费的是这份静态快照；仅把一个 NuGet 程序集作为 `Jazor.Vue` 依赖，并不会自动扩展 compiler mapping。还要注意，当前 `JazorAttribute` 是 `ECMAScript.Contract` 中的内部契约，只对仓库内列出的 friend assembly 可见；这允许第一方 `ECMAScript.Blazor` 先走显式 source-root 集成，却不等于任意外部包可以直接引用该 attribute。要支持真正的可插拔包，必须新增**每次 compilation 注入、不可变、可排序的 mapping-contribution contract**（必要时提供窄的 public schema），而不是把内部 attribute 粗暴公开、反射扫描程序集、修改进程全局白名单或在运行时查字符串。

`ECMAScript.Blazor` 的第一方落地可以先走显式集成，而不等待第三方插件协议：新增 `src/ECMAScript.Blazor` 项目和测试，加入 solution、`Jazor.Vue` 的 package build inputs 与 compiler source-root 生成输入；`Jazor.Vue` 负责把程序集作为 `lib/net11.0` payload 带入。runtime module/helper 不在这个项目中复制，继续从 `Jazor.CLR` 的 catalog/module closure 进入产物。这样能复用现有 `ECMAScript.Vue` 的绑定项目形态，同时把“项目作为 `Jazor.Vue` 的 payload 安装”与“任意外部程序集都能动态贡献 mapping”明确区分开。验收必须同时证明 mapping entry、`Jazor.CLR` runtime module closure 和 RazorVue analyzer 来自同一版本，不能只把 DLL 放入 `analyzers` 目录。

本计划不设置 `Jazor.CLR` 的 Blazor mapping 过渡期，也不设置双 owner。S0 直接创建 `src/ECMAScript.Blazor` 项目、测试和 source-root 集成；S1/S2 等切片的 mapping declaration 从第一版就归该项目，实际 module/helper、catalog 和 runtime artifact 仍归 `Jazor.CLR`。现有 `Jazor.CLR` 导航模块继续保留，不搬迁、不复制；它们是 runtime 行为的唯一来源。`Jazor.CLR` 同时保留 `Task`/`ValueTask`/`CancellationToken` 等跨 framework 共用 carrier 和 helper。

compiler contribution contract、generator merge、package build inputs 和 runtime provider 可以按依赖顺序实现，但它们是同一 mapping owner 的内部实施阶段，不构成“先 Jazor.CLR、后 ECMAScript.Blazor”的产品路径。runtime provider 的实现位置固定为 `Jazor.CLR`，不会因此形成过渡期或第二套实现；这样既能一步到位确定边界，也不会为了追求一次提交完成所有 S1–S8 而跳过各切片的 reference/browser/package 门禁。

每个切片还必须遵守以下不变量：

1. 保持求值顺序、副作用次数、异常传播和 async 完成时机；不能以“生成的 JS 更短”为由改变行为。
2. 浏览器 carrier 是实现细节，不可把它误宣称为完整 CLR runtime identity；无法可靠判定的 `is`/`as`/`typeof` 必须显式失败。已知精度边界是“精确到 carrier，而非唯一 CLR 类型”，泛型实参也会擦除；详见 [hardening plan](./clr-runtime-hardening-plan.md) §1 与 R7。
3. 不引入任意字符串执行、开放 `object` 参数、动态 import 或服务器 API fallback。
4. CLR whitelist 源变更后必须重新运行 `Jazor.Compiler.Generator`，并提交生成的 `WhiteList.cs.Generate.cs`。
5. 新能力改变消费者可使用的 API 面，应按 [发版与版本规则](../03-guides/release-and-versioning.md) 进入 `MINOR`，而不是 PATCH。
6. 实现路径按以下顺序选择并记录原因：C# 类型系统与既有 WebIDL binding、`[Jazor]` 声明和 whitelist、JS 原生语义已经正确时的 `Op.Allowed`、短 `Alias`/`Inline`、C# 编写的 `[ECMAScriptModule]` `Import` helper，最后才是确有上下文或 AST 级协议需要的 compiler `Compile`。不能跳过前一层而直接新增 runtime glue。
7. Blazor 专属 runtime 行为从第一版就以 C# 写入 `Jazor.CLR` module，并由现有 catalog/Emit 管道编译为产物；`ECMAScript.Blazor` 只提供对应的 mapping declaration。不得新增 hand-written `.mjs`。现有 RazorVue `.mjs` 只保留 Vue 生命周期、渲染 framing 和到 `Jazor.CLR` 模块入口的薄转发，不承载新增状态机或成员语义。
8. 内部对象布局遵循 [CLR Runtime 健壮性与性能强化计划](./clr-runtime-hardening-plan.md)：需要 object overload/type test 的值保留推断得到的 nominal carrier，真实 browser 值使用原生 carrier，无身份 host state 使用所属模块的 plain object/closure/`WeakMap`，擦除集合使用原生 `Map`/`Array`/`Set`。不得以 `__jazorType` 或平行 tag 协议补回身份；任何生产入口重建 nominal carrier 时必须调用 CLR-owned 构造/helper。

## 3. P0：导航拦截与异步 carrier

### 3.1 交付目标

让组件可使用标准 `NavigationManager.RegisterLocationChangingHandler(Func<LocationChangingContext, ValueTask>)` 阻止或观察内部导航，并得到与浏览器 history 交互一致的注销和异步行为。开发基线的实现目前只在同一 base URI 的 `NavigateTo` 内部路径上运行 location-changing handler；`popstate`/`hashchange` 目前只触发 `LocationChanged`，不执行可取消的 handler。该 API 虽已有 mapping/runtime/test 实现，但在完成 §12 的 reference、browser、package 证据并裁决 back/forward 边界前仍保持 InProof。

| 类型/API | 目标支持面 | 明确边界 |
| --- | --- | --- |
| `System.Threading.Tasks.ValueTask` | 仅覆盖导航 handler 所需的无参/`Task` 包装、`CompletedTask`、`AsTask`、`Preserve`、awaiter/configure 路径 | 不承诺完整 `ValueTask` API、`ValueTask<T>`、精确 `Task`/`ValueTask` runtime 类型识别、相等性、`IValueTaskSource` 池化协议或所有状态查询成员 |
| `System.Threading.Tasks.ValueTask<T>` | 不属于本 P0 的必做项；只在有已批准的强类型返回 API 时单独设计 | 不能因未来 JS interop 需要而先做无约束泛型 Promise Alias |
| `LocationChangingContext` | `TargetLocation`、`HistoryEntryState`、`IsNavigationIntercepted`、`CancellationToken`、`PreventNavigation()` | 不能只构造普通对象后遗漏导航提交点读取取消结果，也不能把公开的 token getter 留成未映射成员 |
| `CancellationToken`、`CancellationTokenSource`、`CancellationTokenRegistration` | `LocationChangingContext.CancellationToken` 的可观察取消、注册和注销；快速重复导航、back/forward replay 需要时作为同一协议闭环交付 | 不把 token 仅映射为布尔字段；注册、注销和一次性取消必须构成闭环 |
| 返回的 `IDisposable` | `Dispose()` 取消当前 handler 注册，且重复 dispose 不造成额外副作用 | 不把 handler 注册伪装成 field-like event |

### 3.2 必须先确认的参考行为

实现前为下列问题建立标准 Blazor reference fixture；未知行为不能由当前 Vue runtime 猜测：

- 多个 handler 的调用顺序、同步副作用顺序、异步完成顺序和异常传播；
- 任一 handler 调用 `PreventNavigation()` 后，剩余 handler 与最终导航提交的行为；
- handler 在执行期间触发新导航时，旧 context 的取消与最终 URL；
- 被后续导航取代的 context 何时触发 `CancellationToken`，以及已注册回调与 handler completion 的先后顺序；
- `NavigateTo`、浏览器 back/forward、hash/history state、外部 URI 与 `forceLoad` 的差异；`NavLink` 等标准组件不作为本计划的 reference surface；
- 注册句柄 dispose 后的行为、组件 unmount 后是否仍保留 handler；
- `IsNavigationIntercepted` 和 `HistoryEntryState` 的实际值来源。

### 3.3 实施顺序

1. 以现有 `ValueTaskModule` 的 Promise carrier 为基线，核对并收敛非泛型 `ValueTask` 的最小可观察面；不可保真的身份/比较成员继续保留 `Op.Discard`，不得把该最小面误写成完整 `ValueTask` 支持。
2. 在 `Jazor.CLR` 的 C# `[ECMAScriptModule]` 中定义 `LocationChangingContext` runtime 成员和 `PreventNavigation()` helper；`ECMAScript.Blazor` 只记录 framework member 到该 carrier/helper 的 mapping。复杂 dispatch、取消和 commit 决策使用 `Jazor.CLR` 模块的 `Import`，不压缩进 Inline，也不新建手写 `.mjs`。
3. 当前实现的 navigation host 已使用 `Object.Create(null)` + module-private `WeakMap`，但 `popstate`/`hashchange` listener 仍由现有 routing host framing 注册和释放。该 framing 只服务 `NavigationManager`/页面 route catalog 的宿主集成；`Router`、`RouteView`、`NavLink` 标签不属于本计划。后续若抽取跨切片 lifetime primitive，必须先有两个以上真实消费者和独立生命周期回归；S1 不应把计划中的未来抽象写成已存在的实现。
4. `NavigationManagerModule` 继续拥有已落地的 handler registry、取消 controller、内部 dispatch 和 commit；handler/cancellation 主要状态放在 module-private `WeakMap`，当前 host invalidation 版本仍通过 host 上的内部 `__jazorNavigationVersion` 属性传递（它不是作者 API，也不应被描述成完整私有布局）。browser `History` 操作优先复用 WebIDL binding。`popstate`/`hashchange` 的 replay/restore 或明确 Guided Adaptation 尚未裁决，不能把它们写成当前已有的取消协议。认证等后续 framework 切片只能复用已证明的所有权/释放约定，不能复用或改写 navigation state。
5. `blazor-routing.mjs` 只负责创建 host、`provide`、页面 route catalog 的宿主 framing 以及当前 listener 的 mount/unmount；不得在此新增标准 Router/RouteView/NavLink 组件兼容协议。若未来把 listener owner 移到 CLR module，必须同步删除这里的注册并增加 browser regression，避免双重订阅。
6. 只有 browser、Release package 和适用 SSR/hydration 行为一致后，才把该切片从 **InProof** 提升为 **Support**；明确不可保真的成员仍保持 `Op.Discard`，不因切片升级而放行。

### 3.4 验收

- CLR metadata/runtime：`ValueTaskModuleWhitelistTests` 固定 default/Completed/Task wrapping、failure/cancellation factory 与 long-tail `Discard`；`ClrRuntimeNavigationScenarios` 和 Razor SG runtime 覆盖 `LocationChangingContext.CancellationToken` 的取消/注册/注销、handler 注册/注销、`PreventNavigation`、异常和重复 dispose。只有新增可观察 `ValueTask` runtime 行为时才补独立执行场景，不能把 metadata 模板断言写成 runtime proof。
- compiler emission：直接调用、`await`、返回 `ValueTask` 的 lambda、`IDisposable.Dispose()`；所有 import 名和 alias 稳定。
- Razor SG/browser：组件注册 handler 后完成允许导航、阻止同一 base URI 的内部导航、组件卸载和快速连续导航；另用 reference fixture 与真实浏览器单独裁决 back/forward、hash 变化和无法取消的 browser event 是 replay/restore 还是 Guided Adaptation。
- 实现归属（当前）：已落地的导航状态、内部 dispatch、取消和 commit 位于 C# `NavigationManagerModule` 的 opaque host 和私有 state；`popstate`/`hashchange` listener 的 mount/unmount 仍由 `blazor-routing.mjs` 的 Router framing 负责，browser replay/restore 尚不属于该模块的已证明能力。仓库尚未有跨切片通用的 C# lifetime primitive，因此不能把该抽象写成现状。若未来有第二个真实消费者再抽取 owner/cleanup primitive，必须同步删除 `.mjs` 中的重复注册并补独立 lifecycle/browser 回归；在此之前，S1 的验收应直接证明当前分裂边界只订阅一次且卸载可清理。RazorVue runtime 不得新增 hand-written `.mjs` 状态机。
- Release package：runtime module 进入真实 consumer 的 closure，且未使用该切片的应用不会被无条件物化。

## 4. P1：核心 DOM 事件参数

### 4.1 CLR-first：原生 carrier，不造 EventArgs payload

默认路径不在 RazorVue 重新组装 Blazor event object。Vue listener 本来就以真实 DOM event 调用 handler，而 `EventCallback.Factory.Create<T>` 的当前 lowering 已把 callback 变成编译后的 C# handler。因此首批事件的运行路径应保持为：

```text
Vue onClick/onKeydown
  -> native DOM Event
  -> compiler-lowered C# handler(event)
  -> ECMAScript.Blazor member mapping declaration
  -> event.clientX / event.key / event.type ...
```

`T` 仍由 Razor SG/Roslyn 用于 C# 绑定；JavaScript 调用点只传一次真实 event。除 `ChangeEventArgs` 的单点捕获外，这样不需要通用的 `RenderEmitter` 事件类型 descriptor 表、不需要 per-event listener wrapper，也不需要把 DOM object 复制成一个 PascalCase payload。

每个 DOM-origin Blazor `EventArgs` 类型由 `ECMAScript.Blazor` 声明为对应原生 carrier 的 `Op.Alias`（例如 `MouseEventArgs -> MouseEvent`），其 getter 再映射到 WebIDL event 的 camelCase 字段。注意 .NET reference surface 上这些属性大多同时有 setter；本计划首批只承诺 DOM handler 的 read projection，setter/constructor/合成 payload 必须明确保持 Reject，不能因为类型可写就声称 POCO 完整支持。adapter receiver 应使用已有的 `ECMAScript` WebIDL 类型，例如：

```csharp
[Jazor(Op.Alias, "Microsoft.AspNetCore.Components.Web.MouseEventArgs", "MouseEvent")]
internal static class MouseEventArgsExtensions
{
    [Jazor(Op.Inline,
        "Microsoft.AspNetCore.Components.Web.MouseEventArgs.ClientX.get",
        "__arg1.clientX")]
    internal static long ClientX(this MouseEventArgs instance) => instance.ClientX;
}
```

这里的 `MouseEvent` 仅是 CLR adapter 的实现签名，carrier 仍是浏览器给 Vue 的原生对象。现有 carrier inference 只会把 `Jazor.CLR` 内部 class 视为 inferred runtime value carrier；不能为了让 generator 推断 carrier 而额外包一层 `JMouseEvent`，因为真实 DOM event 不会是该包装类的实例。

因此第一版明确保持以下边界：构造器和 setter 为 `Op.Discard`，`is`、`as`、`typeof(EventArgsType)` 也不提供 runtime identity。事件参数是传入 handler 的只读投影，不是可由作者构造、修改或进行 CLR 身份判断的 POCO。未来只有出现具体作者场景并有 reference fixture 时，才评估以 CLR sidecar 实现某个可观察写入语义；不预先建立通用 overlay/proxy。

`MouseEventArgs`、`KeyboardEventArgs` 和 `FocusEventArgs` 的 DOM-origin callback 路径的**目标决策**是 Direct Support，包含标准 DOM attribute 和把同一个 native event 原样向上转发的组件 adapter；S2 在证据补齐前仍是 Planned。`ChangeEventArgs` 的**目标决策**是 Compatibility Adapter，因为它必须在事件时刻保存 value；它不改变另外三类的直接映射方向。普通组件 `EventCallback<T>.InvokeAsync(...)` 可以携带任意自定义值；当它使用 `new MouseEventArgs(...)`、成员初始化或其他合成 event object 时，不能由 native DOM carrier 自动实现，首版在构造/调用使用点拒绝。需要合成参数的组件必须作为单独的 component-emits 切片，显式定义其 CLR creator/carrier 与生命周期，不能借用 DOM 映射悄悄放行。

### 4.2 第一组类型与映射面

| 类型 | 目标决策 | 原生 DOM carrier | 首批 CLR getter alias | RazorVue 工作 |
| --- | --- | --- | --- | --- |
| `Microsoft.AspNetCore.Components.Web.MouseEventArgs` | 目标：Direct Support | `MouseEvent` | `Detail`、`ScreenX/Y`、`ClientX/Y`、`OffsetX/Y`、`PageX/Y`、`MovementX/Y`、`Button`、`Buttons`、`CtrlKey`、`ShiftKey`、`AltKey`、`MetaKey`、`Type` | 无 wrapper，原样传 event。 |
| `Microsoft.AspNetCore.Components.Web.KeyboardEventArgs` | 目标：Direct Support | `KeyboardEvent` | `Key`、`Code`、`Location`、`Repeat`、`CtrlKey`、`ShiftKey`、`AltKey`、`MetaKey`、`Type`、`IsComposing` | 无 wrapper，原样传 event。 |
| `Microsoft.AspNetCore.Components.Web.FocusEventArgs` | 目标：Direct Support | `FocusEvent` | `Type` | 无 wrapper，原样传 event。 |
| `Microsoft.AspNetCore.Components.ChangeEventArgs` | 目标：Compatibility Adapter | `JazorEvent` | `Value`，通过 CLR helper 读取已捕获的 change value | 只在 typed `ChangeEventArgs` handler 上调用一次 capture helper；见下一节。 |

数值在 JavaScript 中统一是 `Number`，因此 `int`/`long`/`float`/`double` 的不同不会要求 payload 转换；C# 的静态签名和 Razor SG 继续负责作者侧类型检查。首批 read surface 覆盖这些类型的全部公开实例 getter。未列出的 setter、构造器和 runtime identity 不是遗漏，而是显式不支持的语义边界。

### 4.3 `ChangeEventArgs.Value`：唯一需要事件边界捕获的核心例外

原生 `Event` 没有顶层 `value`，而且 `event.target.value` 在 async handler 恢复前可能已经被用户后续输入改变。仅把 `Value.get` 映射成一次延迟的 `event.target.value` 读取会失去 Blazor 的事件时刻语义。

这里保留一个极小、CLR-owned 的 bridge，而不是构造 `ChangeEventArgs` payload：

```text
onChange: event => handler(captureChangeEvent(event))
                         |  returns the same native Event
                         |  stores the event-time value in a WeakMap

ChangeEventArgs.Value.get -> getChangeEventValue(event)
```

`captureChangeEvent` 与 `getChangeEventValue` 位于 C# 编写的 `Jazor.CLR` 事件模块（使用 `[ECMAScriptModule]`）；`ECMAScript.Blazor` 只声明 `ChangeEventArgs.Value.get` 对应的 mapping contract。实现复用 `WeakMap` 模式和 `HTMLInputElement`/`HTMLSelectElement` 等 WebIDL receiver，在 C# 控制流中完成输入、checkbox 与 select 的值塑形。RazorVue 只根据 Roslyn 已绑定的 `EventCallback<ChangeEventArgs>` 保留这一次调用，不了解字段形状，也不复制 object 或新增 `.mjs` helper。这是唯一一个类型定向的 listener 钩子，不是可扩展为通用 descriptor 表的协议。首批捕获规则必须用 Blazor reference fixture 固化：普通 input/textarea/select 为 string、checkbox 为 bool、`select[multiple]` 为 string array；file input 不借用此通道，进入后续 `InputFileChangeEventArgs`/`IBrowserFile` 切片。`@bind` 的直接赋值路径继续使用已有 value/checked 提取，不因支持 typed change handler 而创建 EventArgs carrier。

### 4.4 实施与验收

1. 在 `ECMAScript.Blazor` 增加每个已批准类型的原生 `Op.Alias` 和 getter adapter；`MouseEvent`、`KeyboardEvent`、`FocusEvent` 等采用现有 WebIDL receiver 类型，所有 constructor/setter 明确留为 `Op.Discard`。`KeyboardEventArgs.Location` 是 `float`，`MouseEventArgs.Detail/Button/Buttons` 是 `long`，不要在文档或 adapter 中按“全是 double”假设类型。
2. 运行统一的 source-root generator，并在 `ECMAScript.Blazor` metadata 测试中断言 type alias、getter key、Op/path；若涉及 runtime helper，则在 `Jazor.CLR.Test` 断言 helper/module 行为。mapping library 不生成第二份 runtime catalog，`Jazor.CLR` 仍是 runtime owner。
3. 只为 `ChangeEventArgs` 增加由 `Jazor.CLR` 所有的 C# `Import` helper 与 RazorVue 的一次 capture 调用；`ECMAScript.Blazor` 只记录原始 getter mapping，不得引入泛化 event descriptor、payload class、每种事件各自的 listener wrapper 或 hand-written `.mjs` event helper。
4. 在 `Jazor.CompilerTest` 覆盖 C# property access 的 emission、未支持 setter/constructor/identity 的稳定失败，以及 import alias 的稳定性。
5. 在 official Razor SG/Deno fixture 覆盖 method group、lambda、async handler、原样转发 native event 的组件 `EventCallback<T>`、`preventDefault`、`stopPropagation`、`@bind` 与 typed `@onchange` 共存；随后再加入真实 browser fixture。浏览器测试必须证明 async continuation 读取到的是触发时的 `ChangeEventArgs.Value`，而不是之后修改的 DOM value；合成 `new EventArgs` 路径必须稳定失败。capture 调用插入后，source map 仍须指向作者 handler，而不是 CLR helper 或 listener bridge 内部。

## 5. P1：元素引用与焦点

`@ref` capture 已是 render emitter 的职责：VNode 的 ref callback 在元素创建/更新/卸载时把真实 DOM element 写入当前组件 state。它不需要也不应重新变成 RenderTree 或 renderer CLR 模块。

| API | 计划 | 边界 |
| --- | --- | --- |
| `ElementReference` | 将由 `@ref` 捕获得到的真实 DOM element 视为内部 carrier | 不支持用 `new ElementReference(...)` 伪造浏览器节点，也不承诺 `Id`/`Context` 的 server renderer 身份。 |
| `ElementReferenceExtensions.FocusAsync(ElementReference)` | 优先以短 `Inline` 调用 WebIDL `HTMLElement.Focus()`，并返回已完成的 `ValueTask`/Promise carrier | 仅处理由 `@ref` 捕获的真实 DOM element；不伪造 server renderer 身份。 |
| `ElementReferenceExtensions.FocusAsync(ElementReference, bool preventScroll)` | 优先以短 `Inline` 调用 `HTMLElement.Focus(FocusOptions)`，其中 `FocusOptions.PreventScroll` 由标准 bool overload 提供 | 不通过宽松 options `object` 替代标准 bool overload；公开签名已由当前 reference surface 核对。 |

这两个 extension 成员首先走 `SemanticWalker -> Inline`：调用既有 WebIDL `HTMLElement.Focus(FocusOptions?)` binding，并复用已有 `ValueTask`/Promise carrier。只有出现短模板无法保持的可观察协议时，才升级为 C# 编写的 `[ECMAScriptModule]` `Import` helper；不为两个 overload 预先建立 runtime module。DOM node 生命周期和 Vue ref framing 仍由 RazorVue 处理。`scrollIntoView`、selection、measurement 等非标准 `ElementReference` API 应走已有或新增的强类型 WebIDL binding，不应借此把任意 DOM 方法塞进 CLR 模块。

验收覆盖同一元素重新渲染、条件卸载、组件 unmount、`OnAfterRenderAsync` 调用时机、`preventScroll`、短 Inline 的 `ValueTask` emission；若因已证明的复杂行为升级为 `Import`，再验证其 Release bundle closure。

## 6. P2：扩展 DOM 事件族

扩展事件沿用同一条 CLR-first 原则：Vue 继续传入原生 event，`ECMAScript.Blazor` 将 Blazor property getter 映射到 native carrier；若 live browser object 必须转换为 CLR 值契约，只能由 `Jazor.CLR` property helper 在该属性首次访问时完成，不能把物化前移到事件 listener。listener 层不得组装 payload 或为每个类型另建 normalizer。

| 类型组 | 原生 carrier / 依赖 | 交付要求 |
| --- | --- | --- |
| `PointerEventArgs` | `PointerEvent`，继承 `MouseEventArgs` getter slice | 以 getter alias 增加 pointer id、尺寸、压力、倾角、pointer type、primary。 |
| `WheelEventArgs` | `WheelEvent`，继承 `MouseEventArgs` getter slice | 以 getter alias 增加 `DeltaX`、`DeltaY`、`DeltaZ`、`DeltaMode`。 |
| `DragEventArgs`、`DataTransfer`、`DataTransferItem` | `DragEvent`、`DataTransfer` | `DragEventArgs.DataTransfer` 先映射为 native carrier；其 Blazor surface 单独建 CLR adapter，不能把不可用 DOM 方法或 File 对象伪装成普通 POCO。 |
| `ClipboardEventArgs` | `ClipboardEvent` | `Type` 可直接 alias；clipboard data 的权限/用户手势限制由 browser carrier 保持。 |
| `TouchEventArgs`、`TouchPoint` | `TouchEvent`、不可变 `TouchList`、`Touch` | `Touch` 以 CLR getter alias 投影为 `TouchPoint`；集合成员优先在属性首次访问时以短 `Inline` 的 `Array.from(...)` 转为 `TouchPoint[]`。`TouchList` 不可变，因此惰性转换仍读取同一事件值；不在 listener 时刻 materialize，也不预先新增 helper module。 |
| `ErrorEventArgs`、`ProgressEventArgs` | `ErrorEvent`、`ProgressEvent` | 公开成员都有稳定 native 来源时采用 getter alias；否则按成员拒绝。 |

当前 .NET 11 Blazor public reference surface 中没有独立的 `InputEventArgs` 或 `CompositionEventArgs` 类型。本计划不为不存在的 Blazor 契约创建自定义 CLR 类型；输入/组合事件先使用已存在的 `ChangeEventArgs` 或在未来 framework API 出现后重新评估。

扩展事件只在有真实 RazorVue 消费场景时推进。每个类型组独立成为一个 MINOR 能力切片，不因共享 WebIDL carrier 或 CLR helper 而自动获得 Support。

## 7. P2：受控 JS interop

### 7.1 为什么不能只添加接口 Alias

`IJSRuntime` 与 `IJSObjectReference` 的核心问题是“哪个 identifier 可以调用、它的参数/返回值如何编组、该模块由谁提供”，而不是接口在 JavaScript 中叫什么。当前组件注入机制能够按类型生成 Vue `inject(...)`，但内建 runtime 只有已声明的 browser service/provider；没有可执行的默认 JS interop contract。因此单独给接口加 `Object` Alias 会生成无法解析的服务或开放任意动态执行。当前 .NET 11 reference surface 还包括 `InvokeConstructorAsync`、`GetValueAsync` 和 `SetValueAsync`，不能只凭 `InvokeAsync` 两个 overload 宣称覆盖整个接口。

### 7.2 目标合同

| 类型/API | 目标支持面 | 前置条件 |
| --- | --- | --- |
| `IJSRuntime` / `IJSRuntime` extension surface | 只实现静态 contract 已声明的 invocation、constructor、get/set entry；每个 entry 独立记录是否返回 `Task`/`ValueTask` 和是否可取消 | identifier 为编译期可确定的 entry；参数和结果使用已声明的强类型投影，不把 reference surface 的 `object[]` 当成作者面 catch-all。 |
| 静态 module acquisition（若产品最终选择该 API） | 获取静态 module specifier 对应的强类型 `IJSObjectReference` | module specifier 必须在编译期可确定并进入 manifest；当前仓库没有可直接引用的 `[ModuleImport]` pipeline，不能把它写成既有能力。 |
| `IJSObjectReference` | 已声明模块/object 的受控 invocation、constructor/get/set 和 async dispose | import/module closure 进入 manifest，object lifetime 可追踪。 |
| `IJSInProcessRuntime` / `IJSInProcessObjectReference` | 仅对应静态 contract 中实际同步 browser binding 的调用 | 不把异步 Promise 假装同步返回。 |
| `DotNetObjectReference<T>`、`JSInvokableAttribute` | 已发现且可静态绑定的回调 entry | 不扫描程序集反射，不接受任意字符串回调。 |
| `ValueTask<TResult>` | 仅为已经批准的强类型 interop 返回路径新增 | 先定义 Promise carrier、类型投影与失败的 runtime identity 规则。 |

### 7.3 实施步骤

1. 将所谓 registry 定义为**编译期** typed module contract：宿主或绑定包以 `[Jazor]`/whitelist 声明 import specifier、模块入口、可调用成员、强类型参数/结果投影和同步/异步属性。它是编译与 emit metadata，不得生成运行时 JS `Map`、字符串 lookup 或通用 dispatcher。
2. 对 const identifier，`SemanticWalker` 的 `Compile` 在调用点解析该 contract，收集直接 `ImportSpecifier`，再由 `Jazor.Emit` 将实际模块纳入 manifest/closure。`IJSRuntime` 实例只作为复用现有 `provide`/`inject` 的注入 facade，不拥有成员查找。
3. 若未来引入 module-acquisition API，它仅接受静态 specifier；该 API 必须是同一强类型 contract 内的静态 C# module declaration，并复用既有 import collection。当前没有该 attribute pipeline 时不得在计划中假定它已存在。动态 import 继续拒绝。
4. 在 `ECMAScript.Blazor` mapping contribution 中声明已批准的接口成员和受控模块获取入口；object lifetime、dispose 或 callback dispatch 若不能用短 Inline 保真，使用 `Jazor.CLR` 中 C# 编写的 `[ECMAScriptModule]` `Import` helper 或必要的 `Compile` 协议，不使用泛型 `object[]` fallback 或 hand-written `.mjs` glue。Task/ValueTask 等共用 carrier 仍复用 `Jazor.CLR`，不在映射包中复制。
5. 在现有 authored-source compatibility analyzer/final Compilation diagnostic ownership 下，对未知 identifier、动态 identifier、未声明返回类型和 server-only interop 位置给出稳定诊断与强类型替代方向；不要在计划中暗示仓库已有独立 `Jazor.Analyzer.Test` 项目。
6. 最后再考虑 `DotNetObjectReference` 和 `JSInvokable`；它们需要 callback lifetime、实例释放和非反射发现协议，不能由 `InvokeAsync` 自动推导。

### 7.4 验收

- const identifier 必须在编译时解析为唯一的 whitelist/module contract entry 和直接 import；未知或动态调用没有 runtime `undefined`，也不留下运行时 registry lookup。
- 参数、返回值、异常、取消、静态模块获取、object dispose 和 module cache 有真实 browser 回归。
- 同步接口只在同步 registry entry 上可用；所有 Promise 路径在 C# 侧保持 `Task`/`ValueTask` 语义。
- Release package 只物化被 registry 使用的 module closure；SSR/hydration 另有明确 profile 证明。

## 8. P2：认证状态

认证不是把 `AuthenticationStateProvider` 映射为一个 JavaScript object 就完成。它需要浏览器可验证的状态来源、刷新通知、SSR handoff 以及与真实 endpoint 授权分离的契约。Blazor provider 的状态、订阅、刷新和 claims mapping 的 mapping declaration 归 `ECMAScript.Blazor`，实际 C# module 归 `Jazor.CLR`；RazorVue 只复用已有 `provide`/`inject`、cascade 和 component render framing，SSR payload 契约归 `Jazor.Emit`，不得另造 hand-written `.mjs` 认证状态协议。

| 类型/API | 目标支持面 | 边界 |
| --- | --- | --- |
| `AuthenticationStateProvider` | `GetAuthenticationStateAsync()` 与状态变更通知 | provider 必须由 host 注册；没有默认隐式 identity 服务。 |
| `AuthenticationState` | `User` 的最小可观察身份 carrier | 不宣称完整服务器 `ClaimsPrincipal` runtime 身份。 |
| `ClaimsPrincipal`、`ClaimsIdentity`、`Claim` | 仅为已批准的角色/claim 查询提供受控 carrier/member slice | 不引入任意 claims transformation、服务器 ticket 或安全决策 fallback；普通 service injection 不等于有默认身份来源。 |
| `CascadingAuthenticationState` | 不在本计划中 | 这是 Blazor 内置认证 UI/组合组件；第三方组件库或应用自定义组件可消费已注册的 provider/state contract。 |
| `AuthorizeView` / `AuthorizeRouteView` | 不在本计划中 | 这是 Blazor 内置认证 UI/路由组件；UI 隐藏本身也不构成 endpoint 授权。 |

实施顺序：先确定 host 提供的 C# auth descriptor 来源和版本化 refresh 方式，再设计 claims carrier，然后在 CLR module 实现 provider/event。任何 cascading provider 或认证 UI 由第三方组件库/自定义组件自行组合，不在本计划新增标准组件 adapter。SSR profile 必须明确 payload 何时生成、何时失效、hydration 后是否重取；没有该协议时只支持 Browser interactive 或维持 Guided Adaptation。

验收至少包括 anonymous/authenticated 切换、role/claim 查询、provider refresh、组件 unmount、token/descriptor 过期后的可观察行为，以及 endpoint 授权不因任何 UI 组件而被错误宣称为已覆盖。

## 9. 内置表单、验证与文件组件（不属于本计划）

`EditForm`、`InputText`、`InputTextArea`、`InputCheckbox`、`InputNumber`、`InputDate`、`InputSelect`、`InputFile` 以及 `EditContext`/validation 组合均明确移出本框架计划。工作树中可能存在的标准组件 adapter、测试和 runtime module 是历史实验，不是产品支持证据；不得继续扩展，也不得在 release 文档中标记为 Support。表单交互由 TDesign、Vuetify、Element Plus 或应用自定义组件提供。

| 类型/API | 本计划处理 | 组件兼容路线 |
| --- | --- | --- |
| `EditContext` | 不处理 | 由第三方/自定义表单组件定义 model、字段变更和 validation contract。 |
| `FieldIdentifier` | 不处理 | 若组件库需要 field identity，由其公开强类型参数 contract。 |
| `ValidationMessageStore` | 不处理 | 由组件库或应用 validation service 提供。 |
| `InputBase<T>` 与 `Input*` | 不处理 | 不为 Microsoft 内置 input 标签生成 Vue 替代；使用 TDesign/Vuetify/Element Plus 的 typed binding。 |
| `ValidationMessage`、`ValidationSummary`、`DataAnnotationsValidator` | 不处理 | 组件库自行选择 validation descriptor/服务；不扫描整个程序集。 |
| `InputFileChangeEventArgs`、`IBrowserFile` | 不处理 | 文件选择/上传由第三方组件或显式 browser API contract 承担；不伪装成 `InputFile`。 |

本节不新增 `Jazor.CLR` module、`ECMAScript.Blazor` mapping 或 `RazorVue` 标准表单 adapter。若未来要兼容某个内置组件，必须另立组件兼容路线、独立 owner、独立 ledger 和独立版本决策。

`FormName`、`AntiforgeryToken`、`[SupplyParameterFromForm]` 和 enhanced form post 同样不进入本计划；它们属于 SSR/endpoint 或组件兼容路线。

本计划的验收只确认标准组件标签不会被误宣称为 framework Support，并最终能在作者源/使用点得到稳定 Reject 或 Guidance；不会用“表单标签能提交”作为 Blazor framework 证据。

## 10. 明确不进入本计划的类型

| 类型/领域 | 归属或处理方式 |
| --- | --- |
| `ComponentBase`、`IComponent`、`IHandleEvent`、`RenderHandle`、`Renderer`、`RenderTreeBuilder`、内部 RenderTree API | RazorVue component/runtime protocol；renderer/server 基础设施保持不支持。 |
| `EventCallback`、`RenderFragment`、`ParameterView`、component reference、slot | RazorVue lowering；只在已定义的 current-component/slot adapter 入口通过 compiler。 |
| `Router`、`RouteView`、`LayoutView`、`NavLink`、`NavigationLock`、`FocusOnNavigate`、`PageTitle`、`HeadContent`、`HeadOutlet` | 组件或 router adapter；不应因其参数使用 Blazor 类型而整体搬入 CLR。 |
| `AuthorizeView`、`AuthorizeRouteView`、`CascadingAuthenticationState` | 认证状态的消费者，属于 §8 的组件 adapter；不因可接受 `AuthenticationState` 而变成 CLR module。 |
| `Virtualize`、`QuickGrid`、`SectionContent`、`SectionOutlet`、`StreamRendering` | 独立的浏览器渲染/性能/SSR 项目，不能用占位 CLR types 冒充支持。 |
| `HttpClient`、`IHttpClientFactory` | 明确归 `Jazor.RazorVue` 的 browser endpoint-client / application-service adapter 与 authored-source diagnostic 线，不属于本 CLR 计划；没有已声明 endpoint contract 时为 Guided Adaptation 或 Reject，不能映射服务器 `HttpClient` 或隐式 credential 行为。 |
| `IStringLocalizer`、`IStringLocalizer<T>`、资源本地化 | 明确归 localization + SSR state-handoff adapter 与 authored-source diagnostic 线，不属于本 CLR 计划；在 culture、resource payload、fallback 与 hydration 未证明前为 Guided Adaptation 或 Reject。 |
| `ILogger`、`ILogger<T>` | 明确归浏览器 diagnostics / host logging adapter 与 authored-source diagnostic 线，不属于本 CLR 计划；不能把浏览器调用误宣称为服务器 logger，未注册 adapter 的注入或调用必须得到明确诊断。 |
| `HttpContext`、circuit/server service、`PersistentComponentState`、protected browser storage、数据库/Identity 管理服务 | server/SSR host 边界；没有浏览器等价 runtime。 |
| 反射、动态 Type、任意 JS text execution | 维持 Reject；无法通过“通用 object 映射”进入浏览器。 |

## 11. 实施顺序与依赖

| 顺序 | 可独立发布的能力切片 | 主要依赖 | 完成后允许宣称的支持 |
| --- | --- | --- | --- |
| S0 | API ledger、reference fixtures、diagnostic ownership、mapping package 归属裁决 | `src/ECMAScript.Blazor` 项目、首批 mapping test、generator source-root 与 `Jazor.Vue` package build inputs 已落地；现有 M5 ledger 已登记 5 个 Blazor CLR 条目，并由 `RazorVueM5CapabilityLedgerTests.Ledger_BlazorClrSlicesDeclareAuditableContractMetadata` 固定 carrier、profile、实现路径、依赖、排除面和 `static-source-root/v1` contract version。仍需标准 Blazor reference fixture、真正的 per-compilation contribution contract 和隔离 package consumer 编译证据。不要把第二套互相矛盾的 Support 状态塞进 catalog。标准 reference/runtime fixture 可以新增文件，但必须使用仓库现有 `RazorSgOfficialAuthoringTestHost` / `RazorSgOfficialDenoRuntimeTestHost` 入口 | mapping 包边界为 **InProof**，不据此提升具体 API 为 Support |
| S1 | 导航拦截：`ValueTask` + `LocationChangingContext` + 注册句柄 | 已落地的 `NavigationManager` 基础 runtime、`ValueTask`/cancellation 最小 carrier；仍需 reference/browser/package 证据 | 受限的内部导航拦截 |
| S2 | 核心事件：Change/Mouse/Keyboard/Focus | `ECMAScript.Blazor` member adapters + WebIDL carrier；仅 `ChangeEventArgs` 需要由 `Jazor.CLR` helper 实现的一次性 value capture；Task/ValueTask 等共用 carrier 由 `Jazor.CLR` 提供 | 强类型高频 DOM handler |
| S3 | `ElementReference.FocusAsync` | `@ref` lifecycle、`ValueTask` carrier、WebIDL `HTMLElement.Focus`/`FocusOptions` | 受控元素焦点 |
| S4 | Pointer/Wheel/Drag/Clipboard/Touch/Error/Progress 事件组 | S2、WebIDL/File carrier（按组） | 已完成组的强类型 handler |
| S5 | 编译期 typed module contract、`IJSRuntime` 首批 invocation 与静态模块获取 entry | S1 的 async carrier、host injection、现有 import collection、manifest closure | 已声明 identifier/module 的 interop |
| S6 | C# auth state/provider API | S5 的 host/provider 模式、auth descriptor contract | 浏览器认证状态 API；不包含 `CascadingAuthenticationState`/`AuthorizeView` |
| S7/S8 | 内置表单、验证和文件组件 | 不在本计划中 | 由 TDesign/Vuetify/Element Plus 或独立组件兼容路线承担 |

没有日历式发版目标。每个切片在标准语义 fixture、browser、package 及适用 profile 全部通过后，才进入下一次 MINOR；没有通过时保持计划状态或转为 Guided Adaptation/Reject。

### 11.1 落地状态（审阅基线）

状态只描述当前仓库事实，不替代 M5 ledger，也不把一次提交永久写成产品契约：

| 切片 | 当前状态 | 稳定证据 | 缺口/裁决 |
| --- | --- | --- | --- |
| S0 | **InProof** | `ECMAScript.Blazor` 项目已加入 solution、compiler generator source-root 与 `Jazor.Vue` package inputs；`EcmaScriptBlazorMappingTests` 固定首批 whitelist/emission；`RazorVueM5CapabilityLedgerTests.Ledger_BlazorClrSlicesDeclareAuditableContractMetadata` 固定 5 个 Blazor CLR 条目的审计元数据；`ProductionRazorCompilerReferenceTests` 与 `Jazor.EmitTest.SdkIntegrationTests.CreateLocalPackage_SeparatesSharedAndRazorVueAnalyzers`/`CreateLocalPackage_IncludesSelfContainedBrowserAssets` 固定 `Jazor`/`Jazor.Vue` 的程序集与 payload 边界 | 尚无标准 Blazor reference fixture、真正的 per-compilation contribution contract 或隔离 package consumer 编译；当前第一方 mapping 仍由静态 whitelist 显式合并，不把 package payload 存在误写成动态发现 |
| S1 | **InProof** | CLR metadata: `NavigationManagerCatalogWhitelistTests`; CLR/compiler/runtime: `RazorSgNavigationRuntimeTests`（覆盖 URI/history、`LocationChanged`/`OnNotFound`、prevent/dispose/supersede）；模块实现：`NavigationManagerModule`、`ValueTaskModule`、cancellation modules；面向用户草稿：`README.md`/`CHANGELOG.md` Unreleased | 缺标准 Blazor reference oracle、真实 BrowserSmoke、Release PackageConsumer；在证据补齐或面向用户声明回退前，不标记 Support |
| S2 | **InProof（Mouse/Keyboard/Focus/Change mapping）** | M5 `P0-bind-events` 已覆盖普通 DOM/EventCallback framing；`ECMAScript.Blazor.DomEventArgsExtensions` 与 `EcmaScriptBlazorMappingTests` 固定原生 carrier/getter；`ChangeEventArgsModule`、RenderEmitter typed `onchange` wrapper 与 `RazorSgOfficialBindingAuthoringTests.BuildComponent_OfficialRazorTypedChangeHandler_CapturesValueBeforeCallback` 已落地 string/bool/multiple-select capture | Mouse/Keyboard/Focus 的 official Razor SG handler、真实 BrowserSmoke 和 isolated mapping consumer 证据仍待补齐；file input、constructor/setter/合成 payload 保持 Reject |
| S3 | **InProof** | `ECMAScript.Blazor.ElementReferenceExtensions` 固定 `ElementReference -> HTMLElement` alias 与两个 `FocusAsync` `Inline` mapping；`EcmaScriptBlazorMappingTests` 覆盖 whitelist/compiler emission；`RazorSgOfficialReferenceAuthoringTests.BuildComponent_OfficialRazorElementReferenceFocus_UsesDomCarrierMapping` 覆盖 official Razor SG、`@ref` 生命周期和 `OnAfterRenderAsync` lowering | 仍缺真实 BrowserSmoke、isolated Release PackageConsumer，以及空/未挂载 element reference 的 browser 行为裁决；未完成前不标记 Support |
| S4 | **Planned** | 无扩展 event-args CLR mapping | 每个事件组独立验收，不因共享 WebIDL carrier 自动 Support |
| S5 | **Guidance** | M5 row `P2-js-runtime` 的 typed module registry 仍为 Guidance | 先完成静态 contract/manifest/import 设计；当前没有默认动态 interop runtime |
| S6 | **Planned** | M5 row `P2-authentication` 仍为 Planned | 必须先解决 host descriptor、refresh、claims carrier 和 endpoint authorization 分离；内置认证 UI 不在范围 |
| S7/S8 | **Out of scope** | 无本计划实现 | 不把现有标准 input/file adapter 测试当成 CLR framework proof；组件库或独立兼容路线另行负责 |

## 12. 统一验收与发布清单

任一类型切片进入 Support 前，至少完成下列证据链：

1. **API ledger**：记录目标 framework 版本、类型/成员、profile、decision、status、carrier、依赖、明确排除项、实现路径及其选择理由和对应测试名。实现路径必须标明 WebIDL receiver、`Alias`/`Inline`、C# `Import` module 或 `Compile`。ledger 的唯一事实源应是 `RazorVueUsageScenarioCatalog.cs` 中现有 M5 owner 与其明确关联的 Blazor CLR 子项；不要因为本计划提出一个名字就预设 `RazorVueBlazorClrCapabilityLedger` 必须存在，也不要建立相互矛盾的第二套 Support 状态。新增条目必须同步更新现有 `RazorVueM5CapabilityLedgerTests` / `RazorVueSemanticMatrixInventoryTests`，若确实新增子表再增加对应的专门测试。
2. **CLR metadata/runtime**：在 `ECMAScript.Blazor` mapping test 断言 type alias、member `Op` 和 key；在 `src/Jazor.CLR.Test` 断言实际 module path 与 helper 行为。新增 runtime helper 必须能回链到 C# `Jazor.CLR`/已批准 provider source，而非 hand-written `.mjs`；变更 whitelist 源或 mapping contribution 后运行对应的生成/合并门禁，并确认 canonical key 没有被重写。
3. **编译器 emission**：在 `src/Jazor.CompilerTest` 覆盖直接调用、成员访问、异常路径、async/await、interface/extension dispatch（存在时）和稳定 import。
4. **official Razor SG 集成**：在 `src/Jazor.RazorVue.Sg.Test` 使用真实 `.razor` 作者写法，验证 generated C# binding、RazorVue lowering 和 mapped diagnostic。标准 Blazor 行为 oracle 与 RazorVue runtime/browser fixture 应分别有清晰 owner；当前仓库已有 `RazorSgNavigationRuntimeTests` 和 `RazorSgOfficialDenoRuntimeTestHost`，但没有名为 `RazorSgBlazorClrReferenceFixtureTests` 或 `RazorSgBlazorClrRuntimeTests` 的固定落点。新增 fixture 时可以采用这些名字，也可以扩展现有测试，但必须同步本表和 ledger，不能让不存在的文件名成为验收前提。
5. **真实浏览器**：验证 DOM、history、事件、生命周期、Promise/异常、unmount 和交互结果；不得只断言生成 `.mjs` 文本。若 RazorVue runtime 有改动，审查其仅为 framing/薄转发，新增领域状态和成员语义必须仍在 C# CLR module。
6. **交付**：至少确认 debug/release artifact；涉及 runtime import 的切片还要确认 isolated package consumer 的 closure。针对映射包必须分别验证：仅引用 `Jazor` 的 consumer 不出现 `ECMAScript.Blazor` 资产；引用 `Jazor` + `Jazor.Vue` 的 consumer 能在同一 compilation 发现 mapping contribution、analyzer/reference 资产和 runtime provider，且无需复制源码或额外手工注册。支持 SSR/hydration 时另行覆盖一次性副作用与状态 handoff。
7. **失败体验**：未支持的 member、动态值或 server-only 入口在作者源/实际使用点得到稳定诊断，绝不留下运行时 `undefined` 或部分 artifact。当前诊断证据由 `src/Jazor.CompilerTest`（compiler usage-site failure）和 `src/Jazor.RazorVue.Sg.Test`（official Razor SG/final Compilation 与 compatibility analyzer）承载；仓库没有独立 `src/Jazor.Analyzer.Test`，因此不要把 analyzer-only 覆盖写成既有事实。若未来切片确实要求 analyzer 独有规则，再单独建立测试项目并在 ledger 中声明 owner。
8. **模块文档**：新增或改变 CLR runtime 成员行为时同步 `src/Jazor.CLR/doc/<Module>.md`；mapping-only declaration 的说明放在 `src/ECMAScript.Blazor/README.md` 或其测试中。doc 与运行时源码同目录维护，落后的 doc 视为切片未完成。文档生成/核对遵循 `Jazor.CLR` provider 的 generator 流程；不要用一次性提交号或文件行号充当文档与实现的永久链接。
9. **质量门禁**：按改动触及面分别运行 `dotnet run --file scripts/csharp/test-dotnet.cs -- --project clr`、`--project compiler`、`--project razor-sg`（脚本每次只接受一个 project 值），并以 `verify-compiler-coverage.cs` / `verify-razorvue-coverage.cs` / `verify-vue-binding-coverage.cs` 复现 [当前状态](./current-status.md) 的门槛。切片不得以“新增场景很多”为由让覆盖率下降。

完成某个切片后，更新 [当前状态](./current-status.md)、作者指南和 CHANGELOG 的面向用户行为描述；已发布版本章节不回写。文档、CLR mapping、RazorVue adapter 和测试必须同一提交评审，避免出现“文档称支持但 runtime 未注册”或“白名单已放行但没有浏览器 carrier”的灰区。

## 13. 决策门

每次新增类型前，维护者必须先回答：

1. 该类型是否有浏览器中的真实 carrier，还是只是在 server renderer 中存在？
2. 作者会在何处创建、接收、调用或比较该值？每个使用点是否可保真？
3. 这是 CLR member mapping、RazorVue bridge、host provider 还是 SSR handoff 问题？是否需要多个层共同完成？
4. 是否已依次尝试 C# 类型系统、既有 WebIDL binding、`[Jazor]`/whitelist 与短 `Alias`/`Inline`，而无需 `object`、动态 string 或额外 fallback？
5. 若前述路径不足，为什么必须使用 C# `[ECMAScriptModule]` `Import` helper 或 compiler `Compile`，且如何保持该逻辑不落入 hand-written `.mjs`？
6. reference fixture 是否已经说明 async、异常、生命周期和取消顺序？
7. 若答案是否定的，最诚实的结果是 Guided Adaptation 还是 Reject？

只有这些问题都有可验证答案时，类型才进入实现；“ASP.NET Core reference assembly 中存在该类型”不是支持它的充分理由。
