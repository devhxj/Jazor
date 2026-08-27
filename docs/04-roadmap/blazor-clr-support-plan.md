# RazorVue Blazor CLR 类型支持计划

> 状态：规划中。发布基线为 `v0.19.0`，开发审阅基线为 2026-08-27 的工作树。只有机器可核验 ledger、实现、目标 profile 证据和面向用户文档一致时，能力才能标记为 Support。
>
> 定位：这是 [RazorVue 开发者体验完善路线图](./razorvue-developer-experience.md) 中浏览器运行时类型与服务的专项实施计划。它不试图把整个 ASP.NET Core/Blazor runtime 映射到 JavaScript。

> **范围决策（2026-08-25）**：本计划支持 Blazor framework 的 authoring/runtime contract，以及自定义组件和第三方组件库所需的 lowering/runtime primitive；不支持 `Microsoft.AspNetCore.Components` 提供的内置 UI 组件。`Router`、`RouteView`、`NavLink`、`DynamicComponent`、`ErrorBoundary`、`EditForm`、`Input*`、`AuthorizeView`、`Virtualize`、`QuickGrid` 等标签不属于本计划的产品契约。UI 组件层由现有的 TDesign、Vuetify、Element Plus 等绑定/组件库承担。标准组件若仍被识别到，最终应走稳定的 Reject/Guidance 诊断，不得静默生成“部分兼容”的 Vue 替代品。
> **RazorVue 组件入口**：可被本计划消费的组件类型必须可赋值给 `ComponentBase`、实现 `IVueComponent` 或其派生接口，并带有 `[ECMAScriptModule]` 或 `[ECMAScript(import, Transform.Component, exportName)]` 导入描述。应用/本地组件通常使用 `[ECMAScriptModule]`，第三方库包装组件使用 `Transform.Component`；后者是静态 ESM 元数据，不是 JS interop。`ComponentBase`/`IComponent` 本身仍属于 framework primitive；RazorVue 只拥有组件身份识别和产品级 lowering，其获准执行的原始 CLR type/member surface 同样必须先生成并复制到 `Jazor.CLR`。仅有 `ComponentBase` 但没有 Vue marker 的 Microsoft 内置 UI 组件不进入本计划。
> **CLR 模块边界（2026-08-27）**：任何进入支持面的 Blazor CLR 类型都必须先由 `Jazor.CLR.Generator` 从真实 ASP.NET Core reference symbol 生成初始 module/doc，再把选定骨架复制到 `src/Jazor.CLR/module/` 与 `src/Jazor.CLR/doc/` 完善实现。原始 CLR type/member key、`[Jazor]`/`Op.*` 声明与实际 runtime module/helper 的源码 owner 均为 `Jazor.CLR`；生成的 `WhiteList.cs.Generate.cs` 仍由 `Jazor.Compiler` 持有，`ECMAScript.Catalog` 仍保留在 `ECMAScript`，不得在 `ECMAScript.Blazor` 维护第二份映射。
> **ECMAScript.Blazor 边界（2026-08-27）**：`ECMAScript.Blazor` 只提供与 `ECMAScript/internal/Math.cs` 同类的标准 ECMAScript 模拟/投影扩展，可使用 `[ECMAScript]`、`[ECMAScript("specifier")]`、`[ECMAScriptInline]` 与 `Description` 等公开元数据；不得使用 `[Jazor]`、`Op.*`、`[ECMAScriptModule]`，不得成为 whitelist source-root 或 CLR runtime provider。没有额外作者扩展需求时，不为维持程序集存在而复制 CLR mapping。

本计划采用“统一生成入口、分切片完善实现”：先由模块生成器冻结真实 CLR surface，再在 `Jazor.CLR` 中选择 `Discard`、`Allowed`、`Alias`、`Inline`、`Import` 或 `Compile` 并补齐需要的 C# runtime 行为。S0–S5 以及后续框架级切片仍按各自的 reference、browser、package 和 profile 门禁逐步交付，不等于一次性实现全部 Blazor API，也不为内置 UI 组件建立兼容路线。

## 1. 目标与范围

RazorVue 的目标是让 Blazor framework 的作者面在浏览器中保持可观察行为，而不是复制 server renderer、circuit、内置组件库或完整 CLR 对象模型。本计划只覆盖自定义组件 C# 逻辑确实需要消费、且能够在浏览器中建立稳定 carrier 与行为合同的 framework 类型：

- official Razor Source Generator 生成形状、`ComponentBase` 生命周期、参数/fragment/event callback 等自定义组件 lowering primitive；
- 导航拦截所需的 `ValueTask`、`LocationChangingContext` 和关联注销/取消协议；
- DOM 事件参数对象：原生 DOM event 作为 carrier；对应 Blazor CLR 类型与 getter 先生成 `Jazor.CLR` 模块，再在该模块内完成 alias/inline/import 与必要 helper；
- `ElementReference` 的浏览器操作；
- 浏览器认证状态 provider 的 API（不包含 `AuthorizeView` 或其他认证 UI 组件）。

以下内容不因名称属于 Blazor 就进入本计划的内置组件兼容范围：`Router`、`RouteView`、`LayoutView`、`NavLink`、`NavigationLock`、`FocusOnNavigate`、`PageTitle`、`HeadContent`、`HeadOutlet`、`DynamicComponent`、`ErrorBoundary`、`EditForm`、`DataAnnotationsValidator`、`InputBase<T>`/`Input*`、`ValidationMessage`、`AuthorizeView`、`AuthorizeRouteView`、`CascadingAuthenticationState`、`Virtualize<TItem>`、`QuickGrid<TGridItem>`、`SectionOutlet`/`SectionContent` 以及 `InputFile`/`IBrowserFile` 组件组合。`ComponentBase`、`EventCallback`、`RenderFragment`、`ParameterView`、`RenderTreeBuilder` 等 framework primitive 仍可由 `Jazor.RazorVue` 的 current-component lowering、render emitter 或运行时桥接消费；其中任何进入 runtime-sensitive lowering 的 CLR type/member 都必须先经过模块生成器并由 `Jazor.CLR` 持有 mapping，RazorVue hook 只负责最终产品投影。这不等于承诺对应标准组件标签。

### 1.1 发布基线与开发基线

| 范围 | `v0.19.0` 发布状态 | 当前开发基线 | 本计划中的位置 |
| --- | --- | --- | --- |
| `NavigationManager` 基础导航、`LocationChangedEventArgs`、`NotFoundEventArgs`、URL-backed `System.Uri` | `v0.19.0` 已包含对应成员/runtime，但更宽的 routing family 仍由 M5 ledger 标记为 InProof | 沿用既有 runtime，并增加导航取消能力所需的 host 状态 | S1 的基础，不重复实现 |
| `System.Threading.Tasks.Task`、`Task<TResult>` | 已有 Promise carrier 与受控成员面 | 不因新切片自动扩大 Task API | S1/S3/S5 只复用已批准路径 |
| `RegisterLocationChangingHandler(...)`、`LocationChangingContext` | 未发布 | CLR mapping、navigation state machine、compiler tests 和 official Razor SG + Deno runtime tests 已落地 | **InProof**：缺标准 Blazor reference oracle、真实浏览器与 package consumer 证据 |
| 非泛型 `ValueTask` | 未发布 | 导航 handler 所需最小 Promise carrier、metadata/runtime/compiler tests 已落地 | **InProof**：只作为已批准 async 路径的依赖，不代表完整 `ValueTask` 支持 |
| `CancellationToken` / `CancellationTokenSource` / `CancellationTokenRegistration` | 未发布 | `AbortSignal` / `AbortController` / inferred nominal carrier 及对应成员族已落地 | **InProof**：只承诺已验收的取消切片 |
| `ComponentBase`、`EventCallback`/factory、`RenderTreeBuilder` product protocol | 未作为独立 CLR Support 切片发布 | RazorVue hook 已支持一组 official SG 调用形状；`Jazor.CLR` 已持有 generator 生成并完善的 framing-only `Op.Allowed` surface，RazorVue 只保留产品 hook | **InProof**：仍缺标准 reference oracle、真实 BrowserSmoke 和 isolated package consumer 证据 |
| `MouseEventArgs`、`KeyboardEventArgs`、`FocusEventArgs` | Razor SG 可绑定 handler；首批 DOM carrier 与只读 getter 已由 `Jazor.CLR` modules 提供 | `Jazor.CLR.Generator` 已生成 skeleton，选定 modules/docs 已复制并完善；getter 使用原生 DOM carrier，未批准 constructor/setter 保持 `Op.Discard` | **InProof**：仍缺完整 reference/browser/package 证据 |
| `ChangeEventArgs` | Razor SG 可绑定 handler；`Value` 已通过 listener 边界 capture 与 `Jazor.CLR` `WeakMap` helper 投影 | generator skeleton、`ChangeEventArgsModule` mapping/helper 与 RazorVue capture framing 已收敛到单一 CLR owner | **InProof**：仍缺真实 BrowserSmoke、reference oracle 和 isolated package consumer 证据 |
| `@ref` capture / `ElementReference.FocusAsync` | capture 已由 render emitter 支持；`Jazor.CLR` 已把 `ElementReference` 视为 `HTMLElement` 并接入两个 `FocusAsync` overload | `ElementReference`/extensions modules/docs 已由 generator skeleton 复制并完善；DOM lifecycle 仍由 RazorVue 负责 | **InProof**：仍缺真实 browser、Release PackageConsumer 及空/未挂载行为裁决 |
| `PointerEventArgs` | Razor SG 可绑定 `@onpointerdown`；只读 getter 直接读取原生 `PointerEvent`，WebIDL `long`/`int` 值保持 JavaScript `Number` | `Jazor.CLR.Generator` 已生成 skeleton，`PointerEventArgsModule`/doc 已复制并完善；setter 与构造器保持 `Op.Discard` | **InProof**：仍缺真实 BrowserSmoke、reference oracle 和 isolated package consumer；作者侧 `System.Int64` 的 `BigInt` 表示与该 DOM getter 的 `Number` carrier 分开处理 |
| `WheelEventArgs` | Razor SG 可绑定 `@onwheel`；`DeltaX/Y/Z/DeltaMode` getter 直接读取原生 `WheelEvent`，不在 listener 时刻 materialize | `Jazor.CLR.Generator` 已生成 skeleton，`WheelEventArgsModule`/doc 已复制并完善；setter 与构造器保持 `Op.Discard` | **InProof**：仍缺真实 BrowserSmoke、reference oracle 和 isolated package consumer；WebIDL `unsigned long DeltaMode` 是 `Number`，不是 `BigInt` |
| `DragEventArgs`、`DataTransfer` | Razor SG 可绑定 drag handler；`DataTransfer` 保持原生 browser carrier，首批只读字段直接读取 | `Jazor.CLR.Generator` 已生成 skeleton，`DragEventArgsModule`/`DataTransferModule`/doc 已复制并完善；files/items、setter 与构造器保持 `Op.Discard` | **InProof**：仍缺真实 BrowserSmoke、reference oracle 和 isolated package consumer；不承诺 `DataTransferItem`/File payload |
| `ClipboardEventArgs` | Razor SG 可绑定 clipboard handler；`Type` 直接读取原生 `ClipboardEvent` | `Jazor.CLR.Generator` 已生成 skeleton，`ClipboardEventArgsModule`/doc 已复制并完善；payload/权限 API、setter 与构造器保持 `Op.Discard` | **InProof**：仍缺真实 BrowserSmoke、reference oracle 和 isolated package consumer |
| `TouchEventArgs`、`TouchPoint` | Razor SG 可绑定 touch handler；TouchList 在属性访问时惰性 `Array.from(...)` 转成数组 carrier | `Jazor.CLR.Generator` 已生成 skeleton，`TouchEventArgsModule`/`TouchPointModule`/doc 已复制并完善；setter 与构造器保持 `Op.Discard` | **InProof**：仍缺真实 BrowserSmoke、reference oracle 和 isolated package consumer；不承诺 TouchList 非 getter 操作 |
| `ErrorEventArgs` | Razor SG 可绑定 error handler；公开 getter 直接读取原生 `ErrorEvent` | `Jazor.CLR.Generator` 已生成 skeleton，`ErrorEventArgsModule`/doc 已复制并完善；setter 与构造器保持 `Op.Discard` | **InProof**：仍缺真实 BrowserSmoke、reference oracle 和 isolated package consumer |
| `ProgressEventArgs` | Razor SG 可绑定 progress handler；公开 getter 直接读取原生 `ProgressEvent` | `Jazor.CLR.Generator` 已生成 skeleton，`ProgressEventArgsModule`/doc 已复制并完善；setter 与构造器保持 `Op.Discard` | **InProof**：仍缺真实 BrowserSmoke、reference oracle 和 isolated package consumer |
| `IJSRuntime`、`IJSObjectReference`、`IJSInProcessRuntime`、`JSInvokable` | 不进入本计划 | Jazor 的既有 typed ECMAScript/WebIDL/module binding 已直接表达浏览器 API；通用 identifier、`object[]` 和 runtime dispatcher 会削弱该边界 | **Reject**：compiler/final Compilation 在实际使用点诊断，作者改用强类型 binding；不安排 S5 interop 切片 |
| `AuthenticationStateProvider` | 没有默认 browser provider | 通用 browser service injection 不等于认证 UI 已实现；只评估 provider/state API | S5：认证状态 API 垂直切片，排除 `AuthorizeView` |
| `EditContext`、`FieldIdentifier`、`ValidationMessageStore`、`InputFile` | 不在本框架计划中；已有标准输入适配器属于遗留实验/组件兼容工作，不构成 Support | 由 TDesign/Vuetify/Element Plus 的强类型表单组件或独立组件兼容路线承担 | Out of scope；不安排后续切片 |

开发基线上的实现事实、发布状态和支持决策必须分开记录：生成 module/whitelist 存在不等于 Support，Deno runtime 通过也不等于 BrowserSmoke；反过来，已有行为测试也不能掩盖 mapping owner 错误。源码证据应引用稳定的类型、成员和测试名，不引用易漂移的 `WhiteList.cs.Generate.cs` 行号。

当前没有独立的 `Unreleased` 面向用户声明把 location-changing navigation 提升为 Support；`CHANGELOG.md` 中的相关文字属于已发布的历史 `Jazor 0.20.0` 章节，不能替代当前证据。`RazorVueM5CapabilityLedger` 的独立 `P1-blazor-clr-navigation-location-changing` row 仍为 InProof，仓库也没有该切片的 BrowserSmoke/PackageConsumer 证据。下一次发布前必须二选一：补齐该 row 的 reference、browser、package 证据并提升为 Support；或把新的面向用户声明写成 InProof/尚未发布。不能直接把整个宽泛 routing row 提升为 Support，也不能由本计划文字单方面覆盖另一个事实源。

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

本文章节中的 `P0`–`P3` 是本计划的路线优先级带，不等同于 `RazorVueCapabilityPriority` enum 或 M5 ledger 的 `P*-...` row（当前 enum 只到 P2）；`S0`–`S5` 才是可独立验收的交付切片。两套编号不能互相推导。一个计划优先级带可以包含多个切片（例如 P1 包含 S2/S3），切片状态仍以 §11.1 和关联 ledger 为准。

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
  -> Jazor.Emit: .mjs、source map、manifest、bundle 物化

Jazor.CLR.Generator
  -> 从真实 ASP.NET Core reference symbol 生成初始 module/doc
  -> 选定骨架复制到 Jazor.CLR，完善 [Jazor] mapping 与 C# runtime 实现
  -> Jazor.Compiler.Generator 生成 WhiteList 与 ECMAScript.Catalog
  -> SemanticWalker 按使用点收集确定性 runtime import

ECMAScript.Blazor（可选作者扩展）
  -> 标准 ECMAScript 特性与模拟/投影扩展
  -> 作为普通 compilation reference 被识别
  -> 不贡献 CLR member key、WhiteList 或 runtime module
```

| 层 | 本计划中的责任 | 禁止的做法 |
| --- | --- | --- |
| `Jazor.CLR.Generator` | 从当前 .NET/ASP.NET Core reference symbol 生成完整初始 module/doc；保留 canonical member key，并以 `Op.Discard` 明确尚未支持的成员 | 手写目标 CLR 签名、哈希或参考文档；仅为复杂 helper 才生成模块而让 Alias/Inline 类型绕过统一入口 |
| `Jazor.CLR` | 所有获准支持的 Blazor CLR 类型/member mapping、carrier 选择、`[Jazor]`/`Op.*` 与 C# runtime module/helper；直接 Alias/Inline 类型也必须在这里拥有生成骨架 | 把原始 CLR key 或 whitelist mapping 放进 `ECMAScript.Blazor`；复制同一 mapping；用手写 `.mjs` 替代 C# module |
| `ECMAScript.Blazor` | 使用 `[ECMAScript]`、`[ECMAScript("specifier")]`、`[ECMAScriptInline]`、`Description` 等公开协议提供额外的模拟/投影扩展作者面 | 使用 `[Jazor]`、`Op.*`、`[ECMAScriptModule]`；进入 whitelist source-root；复制 framework 原始成员表；承载 runtime helper |
| `Jazor.Compiler` / `SemanticWalker` | 所有 C# 表达式、调用、成员访问、导入收集和使用点失败 | 对未映射外部成员静默发射原始 JavaScript |
| `Jazor.RazorVue` | Vue listener 的原生 event 传递、`@ref` 生命周期、Vue `provide`/`inject`、组件/路由/表单 framing，以及对已由 `Jazor.CLR` 放行 symbol 的产品级 translation hook | 持有 `[Jazor]`/`Op.*` CLR member catalog 或成为 whitelist source-root；为每种 `EventArgs` 手工构造 payload；用手拼 JS 替代 C# 成员/函数语义 lowering；把导航、认证、表单状态机新增到既有 hand-written runtime `.mjs` |
| `ECMAScript.Catalog` | 保持既有 CLR runtime catalog 载体；由 generator 从 `Jazor.CLR` module source 生成模块内容与依赖元数据 | 因拆分 Blazor binding 而迁移、复制或另建第二份 CLR catalog |
| `Jazor.Emit` | 读取 `ECMAScript.Catalog`，按真实 import closure 物化产物 | 在 RazorVue 中直接写入文件、重新解释 module 语义或绕过 manifest |

所有类型必须以完整垂直切片交付。这里也包括只在 RazorVue translation hook 中获得产品语义的 CLR 类型：hook 不是跳过模块生成器的例外。一个类型仅进入生成器、仅有 module/whitelist key、仅有空对象 Alias，或仅能通过 Razor 编译，都不构成 Support。

### 2.1 CLR 类型统一经过模块生成器

本计划中的“支持 Blazor CLR 类型”只允许一个 canonical 入口：`Jazor.CLR.Generator`。无论最终实现是原生 carrier alias、短 inline、import helper 还是 compiler hook，都不能跳过生成器直接手写 member key。统一流程如下：

1. 将目标 framework 类型加入生成器的显式类型集合，并补齐其 reference assembly、documentation provider 与初始 carrier map；不能用字符串猜测 reference surface。
2. 运行 `dotnet run --project src/Jazor.CLR.Generator/Jazor.CLR.Generator.csproj -- .tmp/clr-scaffold`，从 Roslyn symbol 生成初始 `module/*.cs` 与 `doc/*.md`。
3. 审核生成的完整成员面，把选定 module/doc 复制到 `src/Jazor.CLR/module/` 与 `src/Jazor.CLR/doc/`。保留生成的 canonical key、签名和默认 `Op.Discard`，只在已批准切片内修改 `Op`、carrier 与实现。
4. 直接 host property 可使用 `Alias`/`Inline`；需要复杂控制流、共享状态或 helper 时使用同一 `Jazor.CLR` module 中的 `Import`/C# 实现；只有 AST 上下文确实不可避免时才进入 `Compile`。若成员由 RazorVue 产品 hook 投影，`Jazor.CLR` module 仍负责 canonical key 与 `Op.Allowed`/`Op.Discard`，hook 负责最终 AST；这类 framing-only 成员不要求伪造无用的 runtime export。
5. 运行 `Jazor.Compiler.Generator`，由 `Jazor.CLR` module source 统一生成 `WhiteList.cs.Generate.cs` 和现有 `src/ECMAScript/Catalog.g.cs`。`ECMAScript.Catalog` 的类型名、程序集载体和 Emit 读取协议保持不变。

该流程既约束复杂 runtime 类型，也约束看似只需一行 Alias/Inline 的类型。模块骨架的意义不只是生成 JavaScript，而是冻结真实 CLR surface、明确未支持成员、避免手写 key 漂移，并让 mapping、runtime、文档和测试拥有同一 owner。

### 2.2 ECMAScript.Blazor 只提供补充投影扩展

`ECMAScript.Blazor` 与 `ECMAScript/internal/Math.cs` 属于同一类 authoring library：它可以用标准 ECMAScript 特性声明额外的模拟/投影扩展，但不是 Blazor CLR 类型支持的事实源。

- `MouseEventArgs.ClientX`、`ChangeEventArgs.Value` 等作者源码会绑定到 framework 原始 symbol；它们的支持必须来自 `Jazor.CLR` 生成 module 中的原始 member key，不能靠同名普通扩展成员替换。
- `ECMAScript.Blazor` 只有在确有额外作者 API 时才声明扩展；这些声明使用公开 ECMAScript 协议并按普通 compilation metadata 被读取，不建立 mapping-contribution schema、provider id 或动态 whitelist merge。
- `ECMAScript.Blazor` 不扫描进 `Jazor.Compiler.Generator` 的 whitelist source roots。现有 source-root、`[Jazor]` mapping 与 `[ECMAScriptModule]` 标注必须迁出并删除，不能保留双读或过渡 owner。
- `ChangeEventArgs` 等需要 helper 的原始 getter mapping 与 helper 应共同位于 `Jazor.CLR` module。RazorVue 只保留事件时刻 capture 的宿主 framing，不在投影库重复声明 getter key。

### 2.3 RazorVue 产品 hook 也不持有 CLR mapping

`Jazor.RazorVue/RazorSdk/Catalog` 中曾有的 `ComponentBaseCatalog`、`EventCallbackCatalog`、`RenderTreeBuilderCatalog` 与 `WebRenderTreeBuilderExtensionsCatalog` 是迁移前实现事实，不是目标所有权。它们已经删除；当前所有获准 CLR type/member key 均由 generator skeleton 完善后的 `Jazor.CLR` modules 持有。不能因为最终行为由 RazorVue hook 投影，就恢复第二条 whitelist producer 路径。

S0 已完成以下 ownership 收敛，后续切片沿用同一流程：

1. 已盘点并纳入首批由 RazorVue 产品 hook 实际调用的 `ComponentBase`、`EventCallback`/`EventCallback<TValue>`、`EventCallbackFactory`、`RenderTreeBuilder`、`WebRenderTreeBuilderExtensions`，并核对 `ParameterView`、`MarkupString` 的 runtime-sensitive 使用点。
2. 已将确认支持的类型加入 `Jazor.CLR.Generator`，生成完整 module/doc，再复制到 `Jazor.CLR`；类型 adapter 和产品 hook 接管的原始成员使用 generator 保留的 canonical key，获准 surface 为 `Op.Allowed`，其余 surface 为 `Op.Discard`。没有把旧手写 key 当作生成输入。
3. `Jazor.CLR` module 现持有 canonical type/member key 与 `Op.Allowed` 白名单面；`Allowed` 仍只表示 symbol 可以进入编译域，不等于通用 CLR runtime 实现。`Jazor.RazorVue` 继续持有 current-component、EventCallback、RenderTreeBuilder 等产品语义的 translation hook，不把 Vue framing 塞进 `jazor.clr` runtime。
4. 已删除 `Jazor.RazorVue/RazorSdk/Catalog` whitelist source-root 和旧 catalog 声明，metadata/key 测试已迁到 `Jazor.CLR.Test`；RazorVue SG 测试只验证 official SG 输入、hook emission、生命周期和失败路径。

仅在 Roslyn 中用于组件身份判定或擦除后的签名注解、从不被物化且没有成员进入 runtime-sensitive lowering 的类型，不构成 CLR runtime 支持声明，不需要创建占位 module。一旦支持其构造、成员访问、调用、类型判断或其他运行时语义，就必须触发上述 generator-first 流程。

交付拓扑固定为：

| 项目/程序集 | 负责内容 | NuGet/产物边界 |
| --- | --- | --- |
| `Jazor` | 框架无关 compiler、Emit、基础 contract/runtime，以及由 `Jazor.CLR` module 生成的静态 mapping/catalog | 不包含 `ECMAScript.Blazor` DLL 或 ASP.NET Core framework reference；未使用的 CLR module 由 closure DCE 排除 |
| `Jazor.CLR` | Blazor/BCL CLR type module、`[Jazor]` mapping、carrier 与 runtime helper 的唯一源码 owner | module 源进入既有 generator；不因 Blazor 支持另建 catalog |
| `ECMAScript` | 现有 `ECMAScript.Catalog` 载体与标准 ECMAScript host 类型 | catalog 由 generator 更新，结构和读取协议保持不变 |
| `ECMAScript.Blazor` | 可选的标准 ECMAScript 模拟/投影扩展 | 作为 `Jazor.Vue` 的 `lib/net11.0` payload；不贡献 whitelist/runtime artifact |
| `Jazor.Vue` | RazorVue analyzer、build-transitive 注册、Vue listener/component framing，并带入 `ECMAScript.Blazor` 作者扩展资产 | 不维护 Blazor CLR member 表或 runtime module；`Jazor.RazorVue` 的产品 hook 也不作为 whitelist producer |

`ECMAScript.Blazor` 对 `Microsoft.AspNetCore.Components*` 的引用只服务自身作者扩展的编译表面；不能因此把 Blazor server/runtime 依赖塞进 `Jazor` 核心包。`Jazor.CLR.Generator` 可以引用真实 ASP.NET Core reference symbol，但复制后的 `Jazor.CLR` module 必须继续使用现有 erased browser-facing adapter signature，不能让 `Jazor.CLR` 项目本身新增 ASP.NET Core framework reference。

每个切片还必须遵守以下不变量：

1. 保持求值顺序、副作用次数、异常传播和 async 完成时机；不能以“生成的 JS 更短”为由改变行为。
2. 浏览器 carrier 是实现细节，不可把它误宣称为完整 CLR runtime identity；无法可靠判定的 `is`/`as`/`typeof` 必须显式失败。已知精度边界是“精确到 carrier，而非唯一 CLR 类型”，泛型实参也会擦除；详见 [hardening plan](./clr-runtime-hardening-plan.md) §1 与 R7。
3. 不引入任意字符串执行、开放 `object` 参数、动态 import 或服务器 API fallback。
4. 新增支持类型时必须先运行 `Jazor.CLR.Generator` 生成并核对 module/doc；复制并完善 `Jazor.CLR` 源后，再运行 `Jazor.Compiler.Generator`，同时提交 `WhiteList.cs.Generate.cs` 与现有 `ECMAScript.Catalog` 生成结果。
5. 新能力改变消费者可使用的 API 面，应按 [发版与版本规则](../03-guides/release-and-versioning.md) 进入 `MINOR`，而不是 PATCH。
6. 实现路径按以下顺序选择并记录原因：模块生成器冻结 CLR surface、C# 类型系统与既有 WebIDL binding、JS 原生语义已经正确时的 `Op.Allowed`、短 `Alias`/`Inline`、同一 `Jazor.CLR` `[ECMAScriptModule]` 中的 `Import` helper，最后才是确有上下文或 AST 级协议需要的 compiler `Compile`。标准 ECMAScript 投影扩展只用于补充作者 API，不能替代原始 CLR member mapping。
7. Blazor 专属 mapping 与 runtime 行为从第一版就共同位于生成后完善的 `Jazor.CLR` module，并由现有 `ECMAScript.Catalog`/Emit 管道编译和物化；`ECMAScript.Blazor` 与 `Jazor.RazorVue` 都不持有对应 member key。不得新增 hand-written `.mjs`。现有 RazorVue `.mjs` 只保留 Vue 生命周期、渲染 framing 和到 `Jazor.CLR` 模块入口的薄转发，不承载新增状态机或成员语义。
8. 内部对象布局遵循 [CLR Runtime 健壮性与性能强化计划](./clr-runtime-hardening-plan.md)：需要 object overload/type test 的值保留推断得到的 nominal carrier，真实 browser 值使用原生 carrier，无身份 host state 使用所属模块的 plain object/closure/`WeakMap`，擦除集合使用原生 `Map`/`Array`/`Set`。不得以 `__jazorType` 或平行 tag 协议补回身份；任何生产入口重建 nominal carrier 时必须调用 CLR-owned 构造/helper。

## 3. P0：导航拦截与异步 carrier

### 3.1 交付目标

让组件可使用标准 `NavigationManager.RegisterLocationChangingHandler(Func<LocationChangingContext, ValueTask>)` 阻止或观察内部导航，并得到与浏览器 history 交互一致的注销和异步行为。开发基线的实现目前只在同一 base URI 的 `NavigateTo` 内部路径上运行 location-changing handler；`popstate`/`hashchange` 目前只触发 `LocationChanged`，不执行可取消的 handler。该 API 虽已有 mapping/runtime/test 实现，但在完成 §12 的 reference、browser、package 证据并裁决 back/forward 边界前仍保持 InProof。

| 类型/API | 目标支持面 | 明确边界 |
| --- | --- | --- |
| `System.Threading.Tasks.ValueTask` | 仅覆盖导航 handler 所需的无参/`Task` 包装、`CompletedTask`、`AsTask`、`Preserve`、awaiter/configure 路径 | 不承诺完整 `ValueTask` API、`ValueTask<T>`、精确 `Task`/`ValueTask` runtime 类型识别、相等性、`IValueTaskSource` 池化协议或所有状态查询成员 |
| `System.Threading.Tasks.ValueTask<T>` | 不属于本 P0 的必做项；只在有已批准的强类型返回 API 时单独设计 | 不能为尚无明确使用点的 API 预先建立无约束泛型 Promise Alias |
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
2. 以 `Jazor.CLR.Generator` 生成的 `LocationChangingContextModule` 为签名和 member key 基线，在同一 `Jazor.CLR` C# `[ECMAScriptModule]` 中完善 type/member mapping、runtime 成员和 `PreventNavigation()` helper。复杂 dispatch、取消和 commit 决策使用该模块的 `Import`，不压缩进 Inline，也不在 `ECMAScript.Blazor` 复制 mapping 或新建手写 `.mjs`。
3. 当前实现的 navigation host 已使用 `Object.Create(null)` + module-private `WeakMap`，但 `popstate`/`hashchange` listener 仍由现有 routing host framing 注册和释放。该 framing 只服务 `NavigationManager`/页面 route catalog 的宿主集成；`Router`、`RouteView`、`NavLink` 标签不属于本计划。后续若抽取跨切片 lifetime primitive，必须先有两个以上真实消费者和独立生命周期回归；S1 不应把计划中的未来抽象写成已存在的实现。
4. `NavigationManagerModule` 继续拥有已落地的 handler registry、取消 controller、内部 dispatch 和 commit；handler/cancellation 主要状态放在 module-private `WeakMap`，当前 host invalidation 版本仍通过 host 上的内部 `__jazorNavigationVersion` 属性传递（它不是作者 API，也不应被描述成完整私有布局）。browser `History` 操作优先复用 WebIDL binding。`popstate`/`hashchange` 的 replay/restore 或明确 Guided Adaptation 尚未裁决，不能把它们写成当前已有的取消协议。认证等后续 framework 切片只能复用已证明的所有权/释放约定，不能复用或改写 navigation state。
5. `blazor-routing.mjs` 只负责创建 host、`provide`、页面 route catalog 的宿主 framing 以及当前 listener 的 mount/unmount；不得在此新增标准 Router/RouteView/NavLink 组件兼容协议。若未来把 listener owner 移到 CLR module，必须同步删除这里的注册并增加 browser regression，避免双重订阅。
6. 只有 browser、Release package 和适用 SSR/hydration 行为一致后，才把该切片从 **InProof** 提升为 **Support**；明确不可保真的成员仍保持 `Op.Discard`，不因切片升级而放行。

### 3.4 验收

- CLR metadata/runtime：`ValueTaskModuleWhitelistTests` 固定 default/Completed/Task wrapping、failure/cancellation factory 与 long-tail `Discard`；`ClrRuntimeNavigationScenarios` 和 Razor SG runtime 覆盖 `LocationChangingContext.CancellationToken` 的取消/注册/注销、handler 注册/注销、`PreventNavigation`、异常和重复 dispose。只有新增可观察 `ValueTask` runtime 行为时才补独立执行场景，不能把 metadata 模板断言写成 runtime proof。
- compiler emission：直接调用、`await`、返回 `ValueTask` 的 lambda、`IDisposable.Dispose()`；所有 import 名和 alias 稳定。
- Razor SG/browser：组件注册 handler 后完成允许导航、阻止同一 base URI 的内部导航、组件卸载和快速连续导航；另用 reference fixture 与真实浏览器单独裁决 back/forward、hash 变化和无法取消的 browser event 是 replay/restore 还是 Guided Adaptation。
- 实现归属（当前）：已落地的导航状态、内部 dispatch、取消和 commit 位于 C# `NavigationManagerModule` 的 opaque host 和私有 state；`popstate`/`hashchange` listener 的 mount/unmount 仍由 `blazor-routing.mjs` 的 `createNavigationHost` framing 负责，browser replay/restore 尚不属于该模块的已证明能力。仓库尚未有跨切片通用的 C# lifetime primitive，因此不能把该抽象写成现状。若未来有第二个真实消费者再抽取 owner/cleanup primitive，必须同步删除这里的重复注册并补独立 lifecycle/browser 回归；在此之前，S1 的验收应直接证明当前分裂边界只订阅一次且卸载可清理。RazorVue runtime 不得新增 hand-written `.mjs` 状态机。
- Release package：runtime module 进入真实 consumer 的 closure，且未使用该切片的应用不会被无条件物化。

## 4. P1：核心 DOM 事件参数

### 4.1 CLR-first：原生 carrier，不造 EventArgs payload

默认路径不在 RazorVue 重新组装 Blazor event object。Vue listener 本来就以真实 DOM event 调用 handler，而 `EventCallback.Factory.Create<T>` 的当前 lowering 已把 callback 变成编译后的 C# handler。因此首批事件的运行路径应保持为：

```text
Vue onClick/onKeydown
  -> native DOM Event
  -> compiler-lowered C# handler(event)
  -> Jazor.CLR generated EventArgs module mapping
  -> event.clientX / event.key / event.type ...
```

`T` 仍由 Razor SG/Roslyn 用于 C# 绑定；JavaScript 调用点只传一次真实 event。除 `ChangeEventArgs` 的单点捕获外，这样不需要通用的 `RenderEmitter` 事件类型 descriptor 表、不需要 per-event listener wrapper，也不需要把 DOM object 复制成一个 PascalCase payload。

每个 DOM-origin Blazor `EventArgs` 类型都先加入 `Jazor.CLR.Generator` 的显式类型集合，并为 `Microsoft.AspNetCore.Components.Web` reference、documentation 与原生 carrier 补齐生成输入。生成的完整 module 默认保留未批准成员为 `Op.Discard`；复制到 `Jazor.CLR` 后，再把类型设为对应原生 carrier 的 `Op.Alias`（例如 `MouseEventArgs -> MouseEvent`），并把获准 getter 映射到 WebIDL event 的 camelCase 字段。注意 .NET reference surface 上这些属性大多同时有 setter；本计划首批只承诺 DOM handler 的 read projection，setter/constructor/合成 payload 必须明确保持 Reject，不能因为类型可写就声称 POCO 完整支持。

```text
Microsoft.AspNetCore.Components.Web.MouseEventArgs reference symbol
  -> Jazor.CLR.Generator
  -> .tmp/clr-scaffold/module/MouseEventArgsModule.cs
  -> src/Jazor.CLR/module/MouseEventArgsModule.cs
     type: Alias(MouseEventArgs -> MouseEvent)
     getter: Inline(ClientX.get -> __arg1.clientX)
     constructor/setter/未批准成员: Discard
```

这里的 `MouseEvent` 是生成器 type map 与 `Jazor.CLR` adapter signature 使用的原生 carrier，真实值仍是浏览器给 Vue 的 DOM 对象。现有 carrier inference 只会把 `Jazor.CLR` 内部 class 视为 inferred runtime value carrier；不能为了让 generator 推断 carrier 而额外包一层 `JMouseEvent`，因为真实 DOM event 不会是该包装类的实例。

因此第一版明确保持以下边界：构造器和 setter 为 `Op.Discard`，`is`、`as`、`typeof(EventArgsType)` 也不提供 runtime identity。事件参数是传入 handler 的只读投影，不是可由作者构造、修改或进行 CLR 身份判断的 POCO。未来只有出现具体作者场景并有 reference fixture 时，才评估以 CLR sidecar 实现某个可观察写入语义；不预先建立通用 overlay/proxy。

`MouseEventArgs`、`KeyboardEventArgs` 和 `FocusEventArgs` 的 DOM-origin callback 路径的**目标决策**是 Direct Support，包含标准 DOM attribute 和把同一个 native event 原样向上转发的组件 adapter；S2 在模块归属迁移和证据补齐前仍为 InProof。`ChangeEventArgs` 的**目标决策**是 Compatibility Adapter，因为它必须在事件时刻保存 value；它不改变另外三类的直接映射方向。普通组件 `EventCallback<T>.InvokeAsync(...)` 可以携带任意自定义值；当它使用 `new MouseEventArgs(...)`、成员初始化或其他合成 event object 时，不能由 native DOM carrier 自动实现，首版在构造/调用使用点拒绝。需要合成参数的组件必须作为单独的 component-emits 切片，显式定义其 CLR creator/carrier 与生命周期，不能借用 DOM 映射悄悄放行。

### 4.2 第一组类型与映射面

| 类型 | 目标决策 | 原生 DOM carrier | 首批 CLR getter alias | RazorVue 工作 |
| --- | --- | --- | --- | --- |
| `Microsoft.AspNetCore.Components.Web.MouseEventArgs` | 目标：Direct Support | `MouseEvent` | `Detail`、`ScreenX/Y`、`ClientX/Y`、`OffsetX/Y`、`PageX/Y`、`MovementX/Y`、`Button`、`Buttons`、`CtrlKey`、`ShiftKey`、`AltKey`、`MetaKey`、`Type` | 无 wrapper，原样传 event。 |
| `Microsoft.AspNetCore.Components.Web.KeyboardEventArgs` | 目标：Direct Support | `KeyboardEvent` | `Key`、`Code`、`Location`、`Repeat`、`CtrlKey`、`ShiftKey`、`AltKey`、`MetaKey`、`Type`、`IsComposing` | 无 wrapper，原样传 event。 |
| `Microsoft.AspNetCore.Components.Web.FocusEventArgs` | 目标：Direct Support | `FocusEvent` | `Type` | 无 wrapper，原样传 event。 |
| `Microsoft.AspNetCore.Components.ChangeEventArgs` | 目标：Compatibility Adapter | `JazorEvent` | `Value`，通过 CLR helper 读取已捕获的 change value | 只在 typed `ChangeEventArgs` handler 上调用一次 capture helper；见下一节。 |

数值 carrier 按来源区分，不能把 C# 宽度直接当成 JavaScript 类型：Web IDL `long`/`unsigned long` 对应 JavaScript `Number`；按本项目当前 WebIDL carrier 决策，`long long`/`unsigned long long` 对应 JavaScript `BigInt`（生成的 C# 类型为 `ECMAScript.BigInt`）。这与 Jazor CLR 对作者侧 `System.Int64`/`System.UInt64` 的 `BigInt` 表示是两条不同规则，不能互相推导。Blazor `MouseEventArgs` 的 CLR `long` 属性对应 Web IDL `MouseEvent` 的 `long` 字段，因此 adapter 明确使用 `Number` carrier；`ProgressEventArgs.Loaded/Total` 对应 `unsigned long long`，使用 `BigInt` carrier。C# 的静态签名和 Razor SG 继续负责作者侧类型检查。首批 read surface 覆盖这些类型的全部公开实例 getter。未列出的 setter、构造器和 runtime identity 不是遗漏，而是显式不支持的语义边界。

### 4.3 `ChangeEventArgs.Value`：唯一需要事件边界捕获的核心例外

原生 `Event` 没有顶层 `value`，而且 `event.target.value` 在 async handler 恢复前可能已经被用户后续输入改变。仅把 `Value.get` 映射成一次延迟的 `event.target.value` 读取会失去 Blazor 的事件时刻语义。

这里保留一个极小、CLR-owned 的 bridge，而不是构造 `ChangeEventArgs` payload：

```text
onChange: event => handler(captureChangeEvent(event))
                         |  returns the same native Event
                         |  stores the event-time value in a WeakMap

ChangeEventArgs.Value.get -> getChangeEventValue(event)
```

`ChangeEventArgs.Value.get` 的原始 CLR mapping、`captureChangeEvent` 与 `getChangeEventValue` 必须共同位于由生成骨架完善得到的 `Jazor.CLR.ChangeEventArgsModule`（使用 `[ECMAScriptModule]`）。实现复用 `WeakMap` 模式和 `HTMLInputElement`/`HTMLSelectElement` 等 WebIDL receiver，在 C# 控制流中完成输入、checkbox 与 select 的值塑形。RazorVue 只根据 Roslyn 已绑定的 `EventCallback<ChangeEventArgs>` 保留这一次调用，不了解字段形状，也不复制 object 或新增 `.mjs` helper。这是唯一一个类型定向的 listener 钩子，不是可扩展为通用 descriptor 表的协议。首批捕获规则必须用 Blazor reference fixture 固化：普通 input/textarea/select 为 string、checkbox 为 bool、`select[multiple]` 为 string array；file input 不借用此通道，进入后续 `InputFileChangeEventArgs`/`IBrowserFile` 切片。`@bind` 的直接赋值路径继续使用已有 value/checked 提取，不因支持 typed change handler 而创建 EventArgs carrier。

### 4.4 实施与验收

1. `MouseEventArgs`、`KeyboardEventArgs`、`FocusEventArgs` 与 `ChangeEventArgs` 已加入 `Jazor.CLR.Generator`，并已生成、核对和复制四组 module/doc。
2. `MouseEvent`、`KeyboardEvent`、`FocusEvent`、`JazorEvent` 的原生 carrier Alias 与获准 getter 已在 `Jazor.CLR` 完善；constructor/setter 保持生成的 `Op.Discard`。`MouseEventArgs.Detail/Button/Buttons` 的 CLR 属性可以是 `long`，但其对应 Web IDL `long` browser carrier 是 `Number`；不要把 source CLR 宽度直接当成 JavaScript carrier，也不要把所有数值一律改成 `double`。`KeyboardEventArgs.Location` 按其 reference/Web IDL 类型处理。
3. 将现有 `ChangeEventArgs` capture/helper 与生成的 `ChangeEventArgsModule` 合并为单一 owner；只为它保留 C# `Import` helper 与 RazorVue 的一次 capture 调用。不得引入泛化 event descriptor、payload class、每种事件各自的 listener wrapper 或 hand-written `.mjs` event helper。
4. 已删除 `ECMAScript.Blazor` 中对应的 `[Jazor]`、`[ECMAScriptModule]` 和重复 member table，并从 whitelist source roots 移除该项目；后续切片不得恢复双读或兼容 fallback。
5. 在 `Jazor.CLR.Test` 断言生成 module 的 type alias、完整 getter key、`Op`/path 与未批准成员 `Discard`；在 `Jazor.CompilerTest` 覆盖 C# property access emission、setter/constructor/identity 的稳定失败及 import alias 稳定性。
6. 在 official Razor SG/Deno fixture 覆盖 method group、lambda、async handler、原样转发 native event 的组件 `EventCallback<T>`、`preventDefault`、`stopPropagation`、`@bind` 与 typed `@onchange` 共存；随后再加入真实 browser fixture。浏览器测试必须证明 async continuation 读取到的是触发时的 `ChangeEventArgs.Value`，而不是之后修改的 DOM value；合成 `new EventArgs` 路径必须稳定失败。capture 调用插入后，source map 仍须指向作者 handler，而不是 CLR helper 或 listener bridge 内部。

## 5. P1：元素引用与焦点

`@ref` capture 已是 render emitter 的职责：VNode 的 ref callback 在元素创建/更新/卸载时把真实 DOM element 写入当前组件 state。它不需要也不应重新变成 RenderTree 或 renderer CLR 模块。

| API | 计划 | 边界 |
| --- | --- | --- |
| `ElementReference` | 将由 `@ref` 捕获得到的真实 DOM element 视为内部 carrier | 不支持用 `new ElementReference(...)` 伪造浏览器节点，也不承诺 `Id`/`Context` 的 server renderer 身份。 |
| `ElementReferenceExtensions.FocusAsync(ElementReference)` | 优先以短 `Inline` 调用 WebIDL `HTMLElement.Focus()`，并返回已完成的 `ValueTask`/Promise carrier | 仅处理由 `@ref` 捕获的真实 DOM element；不伪造 server renderer 身份。 |
| `ElementReferenceExtensions.FocusAsync(ElementReference, bool preventScroll)` | 优先以短 `Inline` 调用 `HTMLElement.Focus(FocusOptions)`，其中 `FocusOptions.PreventScroll` 由标准 bool overload 提供 | 不通过宽松 options `object` 替代标准 bool overload；公开签名已由当前 reference surface 核对。 |

`ElementReference` 与 framework 自带的 `ElementReferenceExtensions` 已加入 `Jazor.CLR.Generator`，分别生成并复制对应 module/doc。两个 `FocusAsync` overload 在 `Jazor.CLR` module 中走短 `Inline`，调用既有 WebIDL `HTMLElement.Focus(FocusOptions?)` binding，并复用已有 `ValueTask`/Promise carrier。只有出现短模板无法保持的可观察协议时，才在同一 `Jazor.CLR` module 中升级为 C# `Import` helper；生成 module 不等于必须为这两个 overload 增加非平凡 runtime export。DOM node 生命周期和 Vue ref framing 仍由 RazorVue 处理。`scrollIntoView`、selection、measurement 等非标准 `ElementReference` API 应走已有或新增的强类型 WebIDL binding，不应借此把任意 DOM 方法塞进 CLR 模块。

当前已证明两个类型来自模块生成器、module/doc 已进入 `Jazor.CLR`，旧 `ECMAScript.Blazor` `[Jazor]` mapping 已删除；剩余验收覆盖同一元素重新渲染、条件卸载、组件 unmount、`OnAfterRenderAsync` 调用时机、`preventScroll`、短 Inline 的 `ValueTask` emission，以及真实 browser 与 Release bundle closure。若因已证明的复杂行为升级为 `Import`，仍必须补对应的 closure 证据。

## 6. P2：扩展 DOM 事件族

扩展事件沿用同一条 CLR-first 原则：每个获准类型先进入 `Jazor.CLR.Generator`，再由复制到 `Jazor.CLR` 的 module 将 Blazor property getter 映射到 native carrier；若 live browser object 必须转换为 CLR 值契约，只能由同一 `Jazor.CLR` module 的 property helper 在该属性首次访问时完成，不能把物化前移到事件 listener。listener 层不得组装 payload 或为每个类型另建 normalizer。

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

## 7. JS interop：明确 Reject

`IJSRuntime` 解决的是 Blazor 应用在运行时按字符串调用另一段 JavaScript；Jazor 的产品路径已经是 C# 经 Roslyn lowering 直接生成确定性 ECMAScript module。因此再建立 `IJSRuntime` facade 会重复现有编译边界，并重新引入 identifier string、`object[]` 编组、动态 import、runtime registry 和无法静态追踪的模块闭包。

`IJSRuntime`、`IJSObjectReference`、`IJSInProcessRuntime`、`IJSInProcessObjectReference`、`DotNetObjectReference<T>` 和 `JSInvokableAttribute` 不进入任何 RazorVue/Blazor CLR 交付切片。这里不新增专用 analyzer 规则或兼容层；它们涉及的未白名单类型/member 在实际使用点由 `Jazor.Compiler` / final Compilation 通过既有 compiler diagnostic（通常为 `JAZORVGA022`）明确失败，不产生 `undefined`、动态 dispatcher 或 hand-written interop glue。

这条 Reject 只针对通用 JS interop facade，不否定受支持的 Blazor/Vue 组件入口。`ComponentBase + IVueComponent`（或派生接口）并带有 `[ECMAScript("package", Transform.Component, "Export")]` 或 `[ECMAScriptModule("./component")]` 的类型，是静态组件 import contract，可以正常进入 RazorVue lowering；`Transform.Component` 描述的是编译期 ESM binding，不是运行时字符串调用。

需要浏览器 API 或第三方 JavaScript 模块时，作者应使用已有的强类型 `ECMAScript`/WebIDL binding，或为实际静态模块 API 添加同样强类型的 binding declaration。该路径继续由 `Jazor.Compiler` 收集 import、由 `Jazor.Emit` 物化 manifest/closure；它不是 `IJSRuntime` 的兼容层，也不接受运行时字符串调用。

## 8. P2：认证状态

认证不是把 `AuthenticationStateProvider` 映射为一个 JavaScript object 就完成。它需要浏览器可验证的状态来源、刷新通知、SSR handoff 以及与真实 endpoint 授权分离的契约。所有获准的 provider/state/claims CLR 类型必须先由 `Jazor.CLR.Generator` 生成 module/doc，其 mapping、carrier 与实际 C# module 均归 `Jazor.CLR`；`ECMAScript.Blazor` 只在出现额外作者扩展需求时提供标准 ECMAScript 投影，不记录这些 framework member key。RazorVue 只复用已有 `provide`/`inject`、cascade 和 component render framing，SSR payload 契约归 `Jazor.Emit`，不得另造 hand-written `.mjs` 认证状态协议。

| 类型/API | 目标支持面 | 边界 |
| --- | --- | --- |
| `AuthenticationStateProvider` | `GetAuthenticationStateAsync()` 与状态变更通知 | provider 必须由 host 注册；没有默认隐式 identity 服务。 |
| `AuthenticationState` | `User` 的最小可观察身份 carrier | 不宣称完整服务器 `ClaimsPrincipal` runtime 身份。 |
| `ClaimsPrincipal`、`ClaimsIdentity`、`Claim` | 仅为已批准的角色/claim 查询提供受控 carrier/member slice | 不引入任意 claims transformation、服务器 ticket 或安全决策 fallback；普通 service injection 不等于有默认身份来源。 |
| `CascadingAuthenticationState` | 不在本计划中 | 这是 Blazor 内置认证 UI/组合组件；第三方组件库或应用自定义组件可消费已注册的 provider/state contract。 |
| `AuthorizeView` / `AuthorizeRouteView` | 不在本计划中 | 这是 Blazor 内置认证 UI/路由组件；UI 隐藏本身也不构成 endpoint 授权。 |

实施顺序：先确定 host 提供的 C# auth descriptor 来源和版本化 refresh 方式，再把获准 CLR 类型加入模块生成器并审核完整 reference surface，然后设计 claims carrier，并在生成后复制的 `Jazor.CLR` module 中实现 provider/event。任何 cascading provider 或认证 UI 由第三方组件库/自定义组件自行组合，不在本计划新增标准组件 adapter。SSR profile 必须明确 payload 何时生成、何时失效、hydration 后是否重取；没有该协议时只支持 Browser interactive 或维持 Guided Adaptation。

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

本节不把相关类型加入 `Jazor.CLR.Generator`，不新增 `Jazor.CLR` module、`ECMAScript.Blazor` 投影扩展或 `RazorVue` 标准表单 adapter。若未来要兼容某个内置组件，必须另立组件兼容路线、独立 owner、独立 ledger 和独立版本决策。

`FormName`、`AntiforgeryToken`、`[SupplyParameterFromForm]` 和 enhanced form post 同样不进入本计划；它们属于 SSR/endpoint 或组件兼容路线。

本计划的验收只确认标准组件标签不会被误宣称为 framework Support，并最终能在作者源/使用点得到稳定 Reject 或 Guidance；不会用“表单标签能提交”作为 Blazor framework 证据。

## 10. 明确不进入本计划的类型

`ComponentBase`、`EventCallback`、`RenderTreeBuilder`、`ParameterView` 等由 RazorVue 产品 hook 消费的 framework primitive 不列在下表；它们获准执行的 runtime-sensitive type/member surface 必须按 §2.3 先经生成器进入 `Jazor.CLR`，产品投影仍由 RazorVue 负责。未获准成员保留 `Op.Discard` 或使用点诊断，不能因同一类型有一个受支持切片就整体放行。

| 类型/领域 | 归属或处理方式 |
| --- | --- |
| `RenderHandle`、`Renderer` 与未获准的内部 RenderTree API | renderer/server 基础设施保持不支持；不得借 RazorVue 的 `RenderTreeBuilder` 产品 hook 放行。 |
| `Router`、`RouteView`、`LayoutView`、`NavLink`、`NavigationLock`、`FocusOnNavigate`、`PageTitle`、`HeadContent`、`HeadOutlet` | 组件或 router adapter；不应因其参数使用 Blazor 类型而整体搬入 CLR。 |
| `AuthorizeView`、`AuthorizeRouteView`、`CascadingAuthenticationState` | 认证状态的消费者，属于 §8 的组件 adapter；不因可接受 `AuthenticationState` 而变成 CLR module。 |
| `Virtualize`、`QuickGrid`、`SectionContent`、`SectionOutlet`、`StreamRendering` | 独立的浏览器渲染/性能/SSR 项目，不能用占位 CLR types 冒充支持。 |
| `IJSRuntime`、`IJSObjectReference`、`IJSInProcessRuntime`、`IJSInProcessObjectReference`、`DotNetObjectReference<T>`、`JSInvokableAttribute` | 明确 Reject；Jazor 已通过强类型 ECMAScript/WebIDL/module binding 生成浏览器模块，不再提供字符串 identifier 或 `object[]` interop facade。 |
| `HttpClient`、`IHttpClientFactory` | 明确归 `Jazor.RazorVue` 的 browser endpoint-client / application-service adapter 与 authored-source diagnostic 线，不属于本 CLR 计划；没有已声明 endpoint contract 时为 Guided Adaptation 或 Reject，不能映射服务器 `HttpClient` 或隐式 credential 行为。 |
| `IStringLocalizer`、`IStringLocalizer<T>`、资源本地化 | 明确归 localization + SSR state-handoff adapter 与 authored-source diagnostic 线，不属于本 CLR 计划；在 culture、resource payload、fallback 与 hydration 未证明前为 Guided Adaptation 或 Reject。 |
| `ILogger`、`ILogger<T>` | 明确归浏览器 diagnostics / host logging adapter 与 authored-source diagnostic 线，不属于本 CLR 计划；不能把浏览器调用误宣称为服务器 logger，未注册 adapter 的注入或调用必须得到明确诊断。 |
| `HttpContext`、circuit/server service、`PersistentComponentState`、protected browser storage、数据库/Identity 管理服务 | server/SSR host 边界；没有浏览器等价 runtime。 |
| 反射、动态 Type、任意 JS text execution | 维持 Reject；无法通过“通用 object 映射”进入浏览器。 |

## 11. 实施顺序与依赖

| 顺序 | 可独立发布的能力切片 | 主要依赖 | 完成后允许宣称的支持 |
| --- | --- | --- | --- |
| S0 | API ledger、reference fixtures、模块生成入口与 ownership 收敛 | `Jazor.CLR.Generator` 已从真实 ASP.NET Core symbol 生成导航、framework primitive、DOM EventArgs 与 ElementReference 的初始 module/doc；选定骨架已进入 `Jazor.CLR`，旧 `ECMAScript.Blazor` 与 `Jazor.RazorVue/RazorSdk/Catalog` whitelist source-root 已删除，metadata 测试已迁移。仍需标准 reference/runtime fixture 与隔离 package consumer，使用现有 `RazorSgOfficialAuthoringTestHost` / `RazorSgOfficialDenoRuntimeTestHost` 入口 | 统一 CLR module 边界为 **InProof**，不据此提升具体 API 为 Support |
| S1 | 导航拦截：`ValueTask` + `LocationChangingContext` + 注册句柄 | 已落地的 `NavigationManager` 基础 runtime、`ValueTask`/cancellation 最小 carrier；仍需 reference/browser/package 证据 | 受限的内部导航拦截 |
| S2 | 核心事件：Change/Mouse/Keyboard/Focus | 四个类型的 generator 输入、生成后复制的 `Jazor.CLR` modules 与 WebIDL carrier；仅 `ChangeEventArgs` 需要一次性 value capture helper；Task/ValueTask 等共用 carrier 继续由 `Jazor.CLR` 提供 | 强类型高频 DOM handler |
| S3 | `ElementReference.FocusAsync` | `ElementReference`/`ElementReferenceExtensions` generator modules、`@ref` lifecycle、`ValueTask` carrier、WebIDL `HTMLElement.Focus`/`FocusOptions` | 受控元素焦点 |
| S4 | Pointer/Wheel/Drag/Clipboard/Touch/Error/Progress 事件组 | 七组类型均已进入 generator，并由复制到 `Jazor.CLR` 的 modules/docs 完成 native carrier getter；Touch 集合只在属性访问时 `Array.from(...)` | 各组均为 **InProof** 的强类型 handler；仍需真实 BrowserSmoke、reference oracle 与 isolated package consumer 后才能宣称 Support |
| S5 | C# auth state/provider API | 获准 auth CLR 类型的 generator modules、host provider、auth descriptor contract、SSR handoff | 浏览器认证状态 API；不包含 `CascadingAuthenticationState`/`AuthorizeView` |
| - | 内置表单、验证和文件组件 | 不在本计划中 | 由 TDesign/Vuetify/Element Plus 或独立组件兼容路线承担 |

没有日历式发版目标。每个切片在标准语义 fixture、browser、package 及适用 profile 全部通过后，才进入下一次 MINOR；没有通过时保持计划状态或转为 Guided Adaptation/Reject。

### 11.1 落地状态（审阅基线）

状态只描述当前仓库事实，不替代 M5 ledger，也不把一次提交永久写成产品契约：

| 切片 | 当前状态 | 稳定证据 | 缺口/裁决 |
| --- | --- | --- | --- |
| S0 | **InProof（ownership 已收敛）** | `Jazor.CLR.Generator` 已覆盖 `NavigationManager`、navigation event/context、共用 async/cancellation 类型、RazorVue primitives、DOM EventArgs 与 ElementReference；`BlazorClrGeneratorOutputTests`、`BlazorClrWhitelistTests`、`RazorVueCatalogOwnershipTests`、`RazorVueM5CapabilityLedgerTests.Ledger_BlazorClrSlicesDeclareAuditableContractMetadata` 与 `SdkIntegrationTests.Build_LocalReleasePackages_CoreAndVueConsumers_RespectBlazorClrPackageBoundary` 固定生成输出、唯一 owner、key/Op、Release package 边界和核心/Vue 消费者资产分离 | 仍缺标准 reference oracle、完整 BrowserSmoke 与各能力切片的 isolated package consumer 证据；不新增 per-compilation mapping-contribution contract |
| S1 | **InProof** | CLR metadata: `NavigationManagerClrWhitelistTests`; CLR/compiler/runtime: `RazorSgNavigationRuntimeTests`（覆盖 URI/history、`LocationChanged`/`OnNotFound`、prevent/dispose/supersede）；模块实现：`NavigationManagerModule`、`ValueTaskModule`、cancellation modules；面向用户草稿：`README.md`/`CHANGELOG.md` Unreleased | 缺标准 Blazor reference oracle、真实 BrowserSmoke、Release PackageConsumer；在证据补齐或面向用户声明回退前，不标记 Support |
| S2 | **InProof（ownership 已收敛）** | M5 `P0-bind-events`、`BlazorClrMappingTests`、`BlazorClrWhitelistTests`、`Jazor.CLR.ChangeEventArgsModule`、RenderEmitter typed `onchange` wrapper 与 `RazorSgOfficialBindingAuthoringTests.BuildComponent_OfficialRazorTypedChangeHandler_CapturesValueBeforeCallback` 已证明 native getter emission 与 string/bool/multiple-select capture 行为 | 仍需完整 official Razor SG reference oracle、真实 BrowserSmoke 和 isolated mapping PackageConsumer；constructor/setter/identity、file input 与 synthetic payload 继续 Reject |
| S3 | **InProof（ownership 已收敛）** | `BlazorClrMappingTests`、`BlazorClrWhitelistTests` 与 official Razor SG 测试已证明 `ElementReference -> HTMLElement`、两个 `FocusAsync` overload、`@ref` 生命周期和 `OnAfterRenderAsync` lowering；对应 modules/docs 已由 generator skeleton 完善 | 仍需真实 BrowserSmoke、isolated Release PackageConsumer，以及空/未挂载 element behavior 裁决 |
| S4 | **InProof（全部七组）** | `PointerEventArgsModule`、`WheelEventArgsModule`、`DragEventArgsModule`/`DataTransferModule`、`ClipboardEventArgsModule`、`TouchEventArgsModule`/`TouchPointModule`、`ErrorEventArgsModule`、`ProgressEventArgsModule` 均由 `Jazor.CLR.Generator` 生成 skeleton 并复制到 `Jazor.CLR`；`BlazorClrWhitelistTests`、`BlazorClrGeneratorOutputTests`、`BlazorClrMappingTests` 与 `RazorSgOfficialExtendedDomEventRuntimeTests` 已证明 alias/getter、Number/BigInt carrier、native handler 与 Deno 行为 | 七组仍缺标准 reference oracle、真实 BrowserSmoke、isolated package consumer；setter/constructor、synthetic payload、DataTransfer files/items、TouchList 非 getter 操作继续 Reject |
| S5 | **Planned** | M5 row `P2-authentication` 仍为 Planned；尚未选择 generator type slice | 必须先解决 host descriptor、refresh、claims carrier 和 endpoint authorization 分离，再生成获准 modules；内置认证 UI 不在范围 |
| - | **Out of scope** | 内置表单、验证、文件与 Blazor JS interop 都没有本计划实现 | 不把现有标准 input/file adapter 测试当成 CLR framework proof；使用 typed ECMAScript/WebIDL binding，不新增 `IJSRuntime` 兼容层 |

## 12. 统一验收与发布清单

任一类型切片进入 Support 前，至少完成下列证据链：

1. **API ledger**：记录目标 framework 版本、类型/成员、profile、decision、status、carrier、依赖、明确排除项、生成 module/doc、实现路径及其选择理由和对应测试名。实现路径必须标明 `Jazor.CLR.Generator` 输入、WebIDL receiver、`Alias`/`Inline`、C# `Import` module 或 `Compile`。ledger 的唯一事实源应是 `RazorVueUsageScenarioCatalog.cs` 中现有 M5 owner 与其明确关联的 Blazor CLR 子项；不要因为本计划提出一个名字就预设 `RazorVueBlazorClrCapabilityLedger` 必须存在，也不要建立相互矛盾的第二套 Support 状态。新增条目必须同步更新现有 `RazorVueM5CapabilityLedgerTests` / `RazorVueSemanticMatrixInventoryTests`，若确实新增子表再增加对应的专门测试。
2. **模块生成与 CLR metadata/runtime**：断言目标类型已加入 `Jazor.CLR.Generator` 的真实 reference/type map，临时输出的完整 module/doc 可复现，选定骨架已复制到 `src/Jazor.CLR/module`/`doc`；在 `src/Jazor.CLR.Test` 断言 type alias、canonical member key、`Op`/path、未批准成员 `Discard` 与 helper 行为。由 RazorVue hook 投影的 framing-only 类型和成员也必须在这里分别断言 `Op.Allowed`，但不要求伪造 runtime export；其 observable emission 仍在 RazorVue SG 测试验证。新增 runtime helper 必须回链到同一 C# `Jazor.CLR` module，而非 `ECMAScript.Blazor`、`Jazor.RazorVue/RazorSdk/Catalog` 或 hand-written `.mjs`；随后运行 `Jazor.Compiler.Generator` 并核对 whitelist 与 `ECMAScript.Catalog`，确认 canonical key 和 catalog 载体没有被改写。
3. **编译器 emission**：在 `src/Jazor.CompilerTest` 覆盖直接调用、成员访问、异常路径、async/await、interface/extension dispatch（存在时）和稳定 import。
4. **official Razor SG 集成**：在 `src/Jazor.RazorVue.Sg.Test` 使用真实 `.razor` 作者写法，验证 generated C# binding、RazorVue lowering 和 mapped diagnostic。标准 Blazor 行为 oracle 与 RazorVue runtime/browser fixture 应分别有清晰 owner；当前仓库已有 `RazorSgNavigationRuntimeTests` 和 `RazorSgOfficialDenoRuntimeTestHost`，但没有名为 `RazorSgBlazorClrReferenceFixtureTests` 或 `RazorSgBlazorClrRuntimeTests` 的固定落点。新增 fixture 时可以采用这些名字，也可以扩展现有测试，但必须同步本表和 ledger，不能让不存在的文件名成为验收前提。
5. **真实浏览器**：验证 DOM、history、事件、生命周期、Promise/异常、unmount 和交互结果；不得只断言生成 `.mjs` 文本。若 RazorVue runtime 有改动，审查其仅为 framing/薄转发，新增领域状态和成员语义必须仍在 C# CLR module。
6. **交付**：至少确认 debug/release artifact；涉及 runtime import 的切片还要确认 isolated package consumer 的 closure。仅引用 `Jazor` 的 consumer 不应出现 `ECMAScript.Blazor` 资产或 ASP.NET Core framework reference，但 `Jazor.CLR` 生成的静态 mapping/catalog 仍按核心包既有方式提供且未使用 module 不得物化；引用 `Jazor` + `Jazor.Vue` 的 consumer 应获得 `ECMAScript.Blazor` 普通 authoring reference、RazorVue analyzer/reference 资产和既有 `jazor.clr` runtime catalog，无需 mapping contribution 注册、源码复制或第二 runtime provider。支持 SSR/hydration 时另行覆盖一次性副作用与状态 handoff。
7. **失败体验**：未支持的 member、动态值或 server-only 入口在作者源/实际使用点得到稳定诊断，绝不留下运行时 `undefined` 或部分 artifact。当前诊断证据由 `src/Jazor.CompilerTest`（compiler usage-site failure）和 `src/Jazor.RazorVue.Sg.Test`（official Razor SG/final Compilation 与 compatibility analyzer）承载；仓库没有独立 `src/Jazor.Analyzer.Test`，因此不要把 analyzer-only 覆盖写成既有事实。若未来切片确实要求 analyzer 独有规则，再单独建立测试项目并在 ledger 中声明 owner。
8. **模块文档**：每个获准 CLR 类型都必须由 `Jazor.CLR.Generator` 生成并同步 `src/Jazor.CLR/doc/<Module>.md`，无论最终使用 Alias、Inline 还是 Import；额外标准 ECMAScript 投影扩展才记录在 `src/ECMAScript.Blazor/README.md` 或其测试中。doc 与 module 源同 owner，落后的 doc 视为切片未完成；不要用一次性提交号或文件行号充当文档与实现的永久链接。
9. **质量门禁**：按改动触及面分别运行 `dotnet run --file scripts/csharp/test-dotnet.cs -- --project clr`、`--project compiler`、`--project razor-sg`（脚本每次只接受一个 project 值），并以 `verify-compiler-coverage.cs` / `verify-razorvue-coverage.cs` / `verify-vue-binding-coverage.cs` 复现 [当前状态](./current-status.md) 的门槛。切片不得以“新增场景很多”为由让覆盖率下降。

完成某个切片后，更新 [当前状态](./current-status.md)、作者指南和 CHANGELOG 的面向用户行为描述；已发布版本章节不回写。生成器输入、`Jazor.CLR` module/doc、生成 whitelist/catalog、RazorVue framing 和测试必须同一提交评审，避免出现“文档称支持但类型未经过生成器”“白名单已放行但没有浏览器 carrier”或“旧 `ECMAScript.Blazor`/`Jazor.RazorVue` catalog 仍形成双 owner”的灰区。

## 13. 决策门

每次新增类型前，维护者必须先回答：

1. 该类型是否有浏览器中的真实 carrier，还是只是在 server renderer 中存在？
2. 作者会在何处创建、接收、调用或比较该值？每个使用点是否可保真？
3. 这是 CLR member mapping、RazorVue bridge、host provider 还是 SSR handoff 问题？是否需要多个层共同完成？
4. 该类型是否已加入 `Jazor.CLR.Generator` 的显式输入，并从真实 reference symbol 生成完整 module/doc？如果没有，不得直接实现或手写 key。
5. 生成骨架是否已复制到 `Jazor.CLR`，并依次尝试 C# 类型系统、既有 WebIDL binding、`Op.Allowed`、短 `Alias`/`Inline`，而无需 `object`、动态 string 或额外 fallback？
6. 若前述路径不足，为什么必须在同一 `Jazor.CLR` C# `[ECMAScriptModule]` 中使用 `Import` helper 或 compiler `Compile`，且如何保持逻辑不落入 `ECMAScript.Blazor` 或 hand-written `.mjs`？
7. reference fixture 是否已经说明 async、异常、生命周期和取消顺序？
8. 若答案是否定的，最诚实的结果是 Guided Adaptation 还是 Reject？

只有这些问题都有可验证答案时，类型才进入实现；“ASP.NET Core reference assembly 中存在该类型”不是支持它的充分理由。
