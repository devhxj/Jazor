# RazorVue Blazor-first 兼容与开发者体验路线图

> 状态：实施中。M5-0 ledger、author-source compatibility rules、ParameterView adapter、browser `[Inject]` property adapter、typed cascading adapter、route catalog/host、NavigationManager 基础适配器与 P0 module-integrity proof 已落地；应用自有 route-host 的 `@page`/`@layout`/typed route-query/browser-history 子集已达到 `Support`，`NavigationManager` 同源内部 `NavigateTo` 的 LocationChanging 子集也已由 reference、official SG、Deno、真实 HTTP-origin browser 和 isolated Release package consumer 证明。JazorAdmin 与 RazorVue.Authoring 的规范化 Release package/browser consumer gate 也已通过。剩余的是 framework 行为深度、replace/LocationChanged/更宽的 routing、auth state、各 feature 的 SSR/package/browser consumer proof 及未承诺的 P1/P2 能力；`popstate`/`hashchange` cancellation、SSR/prerender route identity 仍未声明。标准 Blazor 内置 UI 组件和 Blazor JS interop 不属于当前产品契约。JazorAdmin M1-M4 完成后的下一阶段，代号 M5。
>
> 目标：开发者按标准 Blazor 的 Razor、组件生命周期、参数、事件、服务和 framework API 使用习惯编写自定义组件；UI 由 TDesign、Vuetify、Element Plus 等组件库提供。RazorVue 应优先自动提供 framework 等价行为；无法保持等价时，由源码分析诊断在作者代码处说明原因、影响和替代，而不是要求开发者预先学习 lowering、Vue 或 generated C#。

> **范围决策（2026-08-25）**：支持 Blazor framework，不承诺 `Microsoft.AspNetCore.Components` 内置 UI 组件。`Router`、`RouteView`、`NavLink`、`DynamicComponent`、`ErrorBoundary`、`EditForm`、`Input*`、`AuthorizeView`、`Virtualize`、`QuickGrid` 等标准标签不进入 M5 Support；识别到未纳入组件路线的标准标签时，应给出稳定 Reject/Guidance。现有适配器代码保留为历史/实验实现，不能作为产品支持证据或继续扩大承诺。

## 1. 产品决策

M5 的目标从“定义一个容易学习的 RazorVue 子集”提升为“尽量完整覆盖 Blazor framework 作者面，并让 UI 组件库承担组件体验”。这不等同于把任意 .NET server runtime 或 Microsoft 内置 UI 组件带进浏览器，也不等同于静默改变代码行为。

兼容性采用以下优先级：

1. 同一份标准 Blazor 写法可以直接编译、运行，并保留可观察行为。
2. 若 Blazor framework 抽象可以由浏览器运行时等价实现，RazorVue 自动提供 adapter，作者仍使用原来的 Razor/C# 形状；UI 组件标签由组件库提供明确的 typed contract。
3. 若无法等价，源码分析诊断在准确位置给出一个可复制的迁移；作者不需要先阅读内部限制文档。
4. 只有不能在浏览器保真、也没有安全明确替代的能力才保持 Reject，并必须有稳定诊断、HelpLink 和最小替代。

页面作者不应为了正常业务功能理解 RenderTreeBuilder、VNode frame、fragment closure、泛型擦除、Vue module、import alias 或 generated Razor C#。这些知识只属于组件库作者、binding 维护者和 RazorVue 维护者。

兼容层的默认策略是保留 Blazor 的 framework 作者面：页面继续使用标准 Razor 指令、`ComponentBase` 生命周期、参数、事件、服务和 framework API。UI 组件使用 TDesign、Vuetify、Element Plus 等库的自然 API，不把 Microsoft 内置组件标签当成隐式兼容层。adapter、registry 和 host registration 属于框架或组件库交付物；除非某个服务确实需要应用宿主选择，否则不把它们变成页面作者必须记忆的新调用约定。内部 ledger、实现原则和完整矩阵用于维护者验收，不是开发者开始写页面的前置阅读。

### 1.1 M5 评审结论与修正

原先以“RazorVue 可用子集 + 页面侧桥接”为中心的规划不满足新目标。主要问题和修正如下：

| 评审发现 | 风险 | 修正后的决策 |
| --- | --- | --- |
| 把 RenderTreeBuilder、Vue module、fragment closure 等内部概念暴露给页面作者 | 开发者必须先学习转译器，普通 Blazor 代码无法直接迁移 | 页面作者合同固定为标准 Blazor Razor/C#；内部 lowering 只由平台和组件库承担 |
| 以“能编译/有 `.mjs`”判定支持 | 可能留下未定义标识符、错误生命周期或异步竞态 | Support 必须同时通过 source、语义、browser、artifact 和适用 SSR/package 证据 |
| 只有 final generated-C# 诊断 | 失败位置落在 generated code，作者只能靠试错理解边界 | 增加只分析 authored source 的 compatibility analyzer；final Compilation 继续负责最终协议裁决 |
| 适配器只有名称，没有行为合同和失败等级 | “已规划”容易被误读为“已兼容”，运行时 null/undefined 变成隐性回退 | 所有能力统一标为 Direct Support、Compatibility Adapter、Guided Adaptation 或 Reject，并附行为矩阵和证据 |
| 以 JazorAdmin 页面 workaround 证明平台能力 | 样例可能掩盖编译器缺陷，公共 API 被单页需求污染 | JazorAdmin 只做真实消费者和回归样本；重复摩擦先做 API review，再决定是否上移 |
| 兼容清单偏重当前热门组件，缺少完整 Blazor 作者面盘点 | 高价值但未列出的指令、生命周期、表单和平台服务会形成盲区 | 增加作者面分类和 ledger 必填字段；未登记能力不能宣称 Support |
| “尽量完整”没有环境边界 | 容易被理解为任意 server API 都能在浏览器运行 | 明确 server-only、动态执行和不可确定 runtime Type 的 Reject/Guided Adaptation 边界 |

因此，M5 不是新增一套 RazorVue 写法，而是逐项消除现有转译限制：能自动投影的由平台完成，不能投影的由分析在源码位置解释，无法保真的才明确拒绝。

### 1.2 两类作者都保持标准 Blazor 形状

| 作者角色 | 默认合同 | 额外知识的处理方式 |
| --- | --- | --- |
| 页面/业务组件作者 | 只使用标准 Razor、`ComponentBase`、参数、事件、服务和表单 API | adapter 自动吸收宿主差异；不可兼容形状由 authored-source analyzer 在源码位置解释 |
| 组件库作者 | 使用标准 Blazor 组件契约、templated component 和必要的 `BuildRenderTree` 形状 | 平台提供 library fixture、source-mapped diagnostic 和模块/运行时证明；不得要求作者拼接 Vue AST、模块字符串或内部 marker protocol |

两类作者都不以“先学 RazorVue lowering”作为完成条件。只有维护 Jazor 本身的人员才需要阅读 compiler/Emit 内部协议；这些内部文档不应成为公共 authoring guide 的前置章节。

## 2. 什么是“尽量完整兼容”

### 2.1 兼容性不是只看编译

一个能力只有同时满足以下条件才可以宣称 Support：

| 维度 | 要求 |
| --- | --- |
| 源码 | 标准 Razor/C# 写法经 official Razor SG 正常绑定和类型检查 |
| 语义 | 求值顺序、副作用次数、异常、生命周期、参数更新和回调时机与目标 Blazor 行为一致 |
| 响应式 | 数据、参数、cascading value、表单和异步结果在实际浏览器中更新正确 |
| 交付 | debug、Release bundle、package consumer 和适用的 SSR/hydration 路径结果一致 |
| 诊断 | 不支持或无法保真的形状在首次 build 失败，不遗留 console error、未定义标识符或部分 artifact |

### 2.2 作者结果分级

| 结果 | 含义 | 作者体验 |
| --- | --- | --- |
| Direct Support | 标准 Blazor 形状直接映射，行为已证明等价 | 无 RazorVue 专属认知或诊断 |
| Compatibility Adapter | 标准 Blazor API 由浏览器 adapter 实现，作者源码保持不变或只有官方推荐的普通配置 | 无手写 Vue/builder；仅在行为不能完全等价时给出说明 |
| Guided Adaptation | 原 API 无法在浏览器保真，但存在强类型替代 | 分析诊断指出源码位置、差异和一段最小替代 |
| Reject | 无可靠浏览器语义或会破坏确定性/安全性 | 构建失败，提供稳定 ID、HelpLink 和替代方向 |

Direct Support 和 Compatibility Adapter 是 M5 的主交付。Guided Adaptation 不是把责任推回给作者，而是把不可避免的环境差异转化为一次性、就地、可行动的信息。

### 2.3 Blazor 参考行为作为兼容 oracle

“等价”不能只凭生成代码目测。每个 P0/P1 能力建立最小 authored fixture，并在对应的标准 Blazor reference host 与 RazorVue browser host 中记录同一组可观察事件：初始参数、生命周期顺序、render 次数、DOM/属性、事件回调、异常、dispose、导航和表单验证结果。比较器只比较作者可观察行为，不要求两边生成相同的 DOM 内部结构或 JavaScript 形状。

reference host 只作为测试 oracle，不作为浏览器 bundle 的运行时依赖。对 SSR、hydration、认证和 server-only service，分别记录环境差异并把差异归入 Compatibility Adapter、Guided Adaptation 或 Reject；不能用“reference host 能运行”替代浏览器可执行性证明。

### 2.4 运行模型 profile

每个 ledger 条目必须标注适用的运行模型，不能用一个“Blazor 兼容”标签掩盖环境差异：

| Profile | M5 目标 | 处理原则 |
| --- | --- | --- |
| Browser interactive（Vue/browser bundle） | P0 全部、已证明的 P1/P2 进入 Direct Support 或 Compatibility Adapter | 依赖必须可编译进浏览器或由 typed endpoint/module adapter 提供；server-only service 在作者源代码处诊断 |
| SSR/prerender + hydration | 对已承诺能力保持一次性副作用、可序列化 state 和 hydration 后交互一致 | 明确 payload/version/lifetime；不能把服务器实例或隐式全局缓存带到客户端 |
| Interactive Server / server-hosted reference | 作为 Blazor 行为 oracle 和 API 迁移参考，不自动扩大浏览器 bundle 支持 | server-only 能力可以在 reference host 中存在，但必须在 RazorVue profile 单独标为 Adapter、Guidance 或 Reject |
| Static/non-interactive render | 支持无交互副作用的页面输出；交互指令和事件必须有明确诊断或对应 render-mode adapter | 不静默生成看似可点击但没有事件处理的 DOM |

作者 API 和组件参数保持跨 profile 一致；差异通过 host registration、adapter 或源码诊断表达。新增功能必须先声明 profile，再进入 P0/P1/P2 ledger 和完成门禁。

## 3. 兼容性范围和优先级

### 3.1 P0：普通页面必须自然工作

P0 是“开发者不应额外学习 RazorVue”的最低完成线：

| Blazor 作者面 | M5 目标 | 主要证明 |
| --- | --- | --- |
| 标记、组件组合、`@typeparam` 泛型组件、templated component、`RenderFragment`/`RenderFragment<T>` | Direct Support | official Razor SG、slot/runtime、模块闭包 |
| `@page`、`@layout`、route parameter、not-found 的普通页面声明 | Direct Support 或 Compatibility Adapter | route matching、layout composition、参数更新和浏览器导航 |
| `@if`、`@foreach`、`@for`、`@while`、条件 attribute、attribute splat、`@key`、`@ref` | Direct Support | DOM 更新、identity、source map |
| DOM/component `@bind`（含 `@bind:get/set/after/event/format`）、`EventCallback`/`EventCallback<T>`、async handler、event modifier | Direct Support | 用户输入、异常/await、culture/format、回调次数 |
| [Parameter]、参数替换、OnParametersSet 和 StateHasChanged | Direct Support | parent-child 更新、单次 render、旧请求竞争 |
| 字段、属性、helper、普通 C# 控制流、record/union 值 | Direct Support | compiler semantics、真实页面业务流 |
| 加载、空、错误、提交、删除确认、重试 | Direct Support 或页面级 adapter | Release browser user journey |
| 常见泛型 UI binding、typed cell/slot、value callback | Compatibility Adapter | 无双重 cast、无 builder 的 Razor 调用 |

P0 的业务页面不得依赖 RenderTreeBuilder、ECMAScriptModule、手写 JavaScript、object 逃生参数或 generated C#。

### 3.2 P1：Blazor framework 组件模型兼容

P1 消除开发者从普通 Blazor 迁移时最常见的认知断点：

| 当前边界或摩擦 | M5 目标 | 预期实现方向 |
| --- | --- | --- |
| SetParametersAsync(ParameterView) | Compatibility Adapter 或 Direct Support | 提供真实 ParameterView snapshot、参数应用顺序和 lifecycle 调度；不能把它伪装成普通 props watch |
| `@inject`/`[Inject]` property 和 constructor injection | Compatibility Adapter | 浏览器 service catalog 与 Vue provide/inject；仅可执行的服务可以被激活 |
| CascadingValue 和 [CascadingParameter] | Compatibility Adapter | typed provide/inject、名称/类型匹配、更新传播、嵌套覆盖和生命周期回归 |
| NavigationManager | Compatibility Adapter | 通过宿主导航实现 URI、NavigateTo、location changed 的明确子集；不承诺 `Router`/`NavLink` 标签 |
| route catalog、`@page`、route/query 参数与页面 host framing | Compatibility Adapter | 生成页面 route metadata 和 host navigation contract；`Router`、`RouteView`、`LayoutView`、`NavLink` 等内置组件标签不在本计划中 |
| `[SupplyParameterFromQuery]`、route parameter、query 更新 | Compatibility Adapter | 由 router/URI adapter 向标准 parameter surface 提供值，并回归 back/forward、编码、nullable 和更新时机 |
| `@ref`、Dispose、Error propagation 和自定义 fragment/slot | Direct Support 或 Compatibility Adapter | 仅覆盖 framework/lowering primitive；`ErrorBoundary`、`DynamicComponent` 等内置组件标签不在本计划中 |
| `Router`、`RouteView`、`LayoutView`、`NavLink`、`DynamicComponent`、`ErrorBoundary` | Reject / separate component roadmap | 不生成 partial Vue substitute；使用应用自定义组件或第三方组件库的 typed contract |
| `EditForm`、`InputBase`、`Input*`、`ValidationMessage`、`InputFile` | Reject / separate component roadmap | 表单与文件 UI 由 TDesign、Vuetify、Element Plus 或应用自定义组件承担 |

P1 的目标是让自定义组件的 framework 代码保持原样。内置 UI 组件标签即使在历史 runtime 中存在 adapter，也不能提升为 M5 Support；若某个标签没有独立组件路线，应在作者源/最终使用点稳定失败。

### 3.3 P2：平台服务和高级兼容

P2 继续扩大 Blazor 兼容，但每项先完成可行性设计和行为证明：

| 能力 | 候选方向 | 不可接受的做法 |
| --- | --- | --- |
| AuthenticationStateProvider、claims/auth state API | 浏览器认证状态 provider 加服务端 endpoint/SSR payload 交接 | 不以 `AuthorizeView`/`AuthorizeRouteView` 隐藏 UI 代替 endpoint 授权；内置认证 UI 不在本计划 |
| IJSRuntime、IJSObjectReference、IJSInProcessRuntime、JSInvokable | **Reject**；Jazor 已通过强类型 ECMAScript/WebIDL/module binding 直接生成浏览器 import，不再提供 Blazor 的 string invocation facade | `InvokeAsync`/`InvokeVoidAsync`、动态 import、`object[]` 编组、runtime registry 和程序集扫描；作者改用实际 API 的强类型 binding |
| PersistentComponentState、prerender data | SSR payload 和 hydration state handoff | 客户端与服务器重复副作用或无版本的全局缓存 |
| `[SupplyParameterFromForm]`、`FormName`、`AntiforgeryToken`、enhanced form post | 独立 SSR/endpoint 或组件路线 | 本框架计划不实现内置表单组件的 server form protocol |
| `Virtualize<TItem>`、`QuickGrid<TGridItem>`、`SectionOutlet`/`SectionContent`、`StreamRendering` | 独立性能/SSR/组件路线 | 不以普通列表或空占位静默替代内置组件 |
| query、fragment、history state 等超出基础 route parameter 的 URI 状态 | Vue Router adapter 与 Razor 组件参数同步 | 页面作者手写 router glue |
| DataAnnotations 和复杂表单验证 | 编译时 descriptor 或明确绑定 contract | 运行时反射扫描整个程序集 |
| localization、culture、`IStringLocalizer`/资源访问 | 编译时资源索引或显式 host locale adapter | 把服务器资源程序集或当前线程状态隐式复制到浏览器 |
| 参数化组件 constructor、this/base chaining | 仅在 activation、DI lifetime、base 初始化顺序可完整投影时支持 | 忽略 constructor argument 或把参数当 constructor argument |

P2 不预设全部都会变成 Direct Support。每项要么实现可保真的 adapter，要么进入 Guided Adaptation；`IJSRuntime` 家族固定为 Reject。不会长期留下“能编译但运行时未知”的灰区。

### 3.4 仍需明确拒绝的底线

以下能力默认不以“完整兼容”为名静默放行：

- 需要真实 server process、filesystem、thread、socket、数据库上下文或任意服务器 DI service 的代码；
- 无法静态确定的动态 Type、动态 import、任意 JavaScript text execution；
- `IJSRuntime`、`IJSObjectReference`、`IJSInProcessRuntime`、`DotNetObjectReference<T>` 与 `JSInvokable` 的通用 interop facade；应改用强类型 ECMAScript/WebIDL/module binding；
- 跨 render frame 的 goto、不可投影的 labeled branch，或破坏 Razor render protocol 的控制流；
- 无法确定来源且可能递归的 RenderFragment factory；
- 不能建立稳定使用点语义的反射、运行时代码生成和裸 object 逃生 API。

这些形状需要由分析诊断识别并说明浏览器边界，而不是让页面作者通过试错了解。

### 3.5 Blazor 作者面盘点

为避免“列了几个组件就称为完整兼容”，M5 ledger 必须覆盖下列作者面分类。分类是审计入口，不要求页面作者逐项学习；每项的最终状态以 ledger 和源码诊断为准。

| 作者面分类 | 必须盘点的标准形状 | 兼容判定重点 |
| --- | --- | --- |
| Razor 文件指令 | `@page`、`@layout`、`@inherits`、`@implements`、`@using`、`@inject`、`@typeparam`/`[CascadingTypeParameter]`、`@attribute`、`@namespace`、`@preservewhitespace`、`@rendermode` | official Razor SG 绑定、泛型级联、基类/接口契约、静态与交互 render mode 的环境边界 |
| 组件契约 | `IComponent`、`ComponentBase`、`IHandleEvent`、`IHandleAfterRender`、`IDisposable`、`IAsyncDisposable`、`OwningComponentBase` | activation、DI lifetime、事件调度、dispose 顺序；server-only owner 必须有明确替代或 Reject |
| 渲染与组合 | `RenderFragment`、`RenderFragment<T>`、`CascadingValue<T>`/`CascadingValueSource<T>` 及自定义 templated component | 单次求值、稳定 identity、fragment/slot 闭包和级联更新；`DynamicComponent`、`ErrorBoundary`、`Virtualize`、`QuickGrid`、`Section*` 内置组件另行处理 |
| 状态与生命周期 | `[Parameter]`、`[CascadingParameter]`、`[SupplyParameterFromQuery]`、`[SupplyParameterFromForm]`、`ParameterView`、`SetParametersAsync`、`OnInitialized*`、`OnParametersSet*`、`OnAfterRender*`、`ShouldRender`、`StateHasChanged`、`InvokeAsync` | 参数覆盖顺序、异步竞态、异常传播、render gate、SSR/hydration 一次性副作用 |
| 路由与文档头 | `NavigationManager`、`NavigationLock`、`FocusOnNavigate`、`PageTitle`、`HeadContent`、`HeadOutlet`、query/fragment/history state，以及生成的 route metadata | 浏览器 history、URI 状态和 host registration；`Router`、`RouteView`、`AuthorizeRouteView`、`LayoutView`、`NavLink` 等内置组件不在本计划 |
| 表单与验证 | 自定义组件的 binding/event contract | 由 TDesign/Vuetify/Element Plus 或独立路线定义 parse/format、validation、file input；`EditForm`/`Input*`/`ValidationMessage` 等内置组件不在本计划 |
| 平台服务 | `AuthenticationStateProvider`、claims/auth state、`PersistentComponentState`、`HttpClient` 和可执行应用服务 | 浏览器可执行性、认证与 endpoint 强制授权、SSR payload 版本和 service lifetime；浏览器 API/第三方模块通过 compiler-owned typed ECMAScript/WebIDL binding，不是 Blazor service；`AuthorizeView` 不在本计划 |

`@rendermode`、`OwningComponentBase`、`Virtualize`、`QuickGrid`、自定义 `InputBase<T>` 和 Blazor JS interop 家族不因出现在盘点表中而自动获得 Support；其中 JS interop 是固定 Reject，作者应改用显式的强类型 binding。内置 UI 组件标签默认走 Reject/Guidance 或独立组件路线。这样既保持 Blazor framework 作者面的完整视野，也不把“列入盘点”误写成“已经实现”。

## 4. 分析诊断优先

### 4.1 诊断模型

开发者不应先读完整兼容矩阵再开始工作。M5 新增 RazorVue authored-source compatibility analyzer，专门分析作者写下的 .razor、.razor.cs 和普通 C# component member，不分析 Razor SG generated C#。

它与现有最终 Compilation 诊断分工如下：

| 层 | 输入 | 职责 | 是否生成 artifact |
| --- | --- | --- | --- |
| Compatibility analyzer | 作者 `.razor`、`.razor.cs` 和普通 C# source；可使用配对 symbol 信息，但不遍历 Razor SG generated C# | 高置信识别 Blazor API、server-only dependency、已知不兼容形状和可机械替代 | 在作者源码处先解释；规则与 final generator 采用互斥 ownership 或共享聚合键；是否生成 artifact 仍由 final generator 的 no-partial 不变量决定 |
 | final Compilation generator | official Razor SG final Compilation | 最终 RenderTree protocol、generic helper、member closure、module framing 和 compiler bridge 的唯一裁决 | 有错误时无 partial descriptor/module |
| runtime smoke | 真实 browser/SSR host | 验证静态分析不能证明的响应式、生命周期、DOM、hydration 和异步行为 | 不作为首次作者错误发现机制 |

当前 `Jazor.Analyzer` 保持 generic ECMAScript contract 和 `GeneratedCodeAnalysisFlags.None`。RazorVue compatibility analyzer 是独立、作者源代码范围明确的 analyzer，不把 generated-code analyzer 的旧 no-go 结论推翻为重复报错。

这里的“独立”是职责和输入边界独立，不要求页面作者安装或配置第二套编程模型。实现时必须先固定 `.razor`/code-behind source 的获取、Razor 文档与 C# symbol 的配对及 source span 映射；默认以 RazorVue 专属 analyzer package/assembly 随 `Jazor.RazorVue` 交付，不扩展通用 `Jazor.Analyzer` 的 generated-code 作用域，具体 assembly 拆分在 M5-0 source-acquisition spike 后锁定。若 Roslyn analyzer 扩展点无法直接提供某种 Razor 语义，则由 RazorVue 自己的 authoring analysis hook 产出同一套 descriptor，而不是退回分析 generated C#。由于 analyzer 通常不能读取 source generator 的诊断，去重必须靠规则 ownership 不重叠，或由统一 build/diagnostic aggregation 层按 rule key + source span 合并；不能假定 analyzer 能停止 generator。final generator 始终独立对实际 lowering 结果作最终裁决并保证错误时没有 partial artifact。

### 4.2 诊断体验契约

诊断只在需要作者行动时出现。Direct Support 不产生“你正在使用兼容模式”的噪音。

每条 Compatibility Diagnostic 必须包含：

1. 作者源码的准确 span，而不是 generated variable 或 builder invocation。
2. 使用的 Blazor API/形状和它在浏览器中无法等价的具体原因。
3. 自动 adapter 是否可用；如果不可用，给出最小强类型替代或 host 配置步骤。
4. 稳定 diagnostic ID、HelpLink 和适用的代码修复。
5. 与最终 `JAZORVGA020`-`026` 的去重关系；通过互斥 ownership 或统一聚合层保证不能同时报两个同源错误。

无法在浏览器中保真的形状必须使用阻止构建的 error；仅缺少可由宿主自动补齐的 registration 才可采用明确的 warning/error 规则，并在消息中说明是否能自动修复；Direct Support 不产生 warning 噪音。

 新 analyzer ID 使用独立的 RazorVue compatibility ID 段（暂建议 `JAZORVCA001+`，由 M5-0 锁定）和公开 release tracking。`JAZORVGA020`-`026` 继续表示 final Compilation 失败，不改变已发布的 ID、category、severity、mapped location 和无 partial descriptor/module 契约。

代码修复仅用于语义可证明的改写，例如添加缺失的浏览器 service registration、把已弃用兼容 helper 改为等价 API。涉及生命周期、服务 lifetime、动态组件或表单状态的迁移默认给出代码片段，不做危险的自动重写。

### 4.3 分析规则

| 规则族 | 触发例 | 诊断动作 |
| --- | --- | --- |
| Browser service eligibility | [Inject] 的服务含 server-only member、已知 host service 没有 adapter，或属性不是可激活的 writable auto-property | 指出不兼容成员及注册/endpoint client 替代；当前已落地 `JAZORVCA001`-`007` |
| Parameter lifecycle | SetParametersAsync、ParameterView、parameter mutation | 说明 adapter 支持状态；未实现时给出 OnParametersSet 或已定义兼容入口 |
| Cascading/DI | `[CascadingParameter]`、名称/类型冲突、无 provider、生命周期不匹配 | typed provider/inject adapter 已处理类型/名称匹配、最近值和生命周期；`JAZORVCA008` 只诊断不可写属性形状 |
| Forms/validation | 未支持的 validator、反射型 rule、不可序列化 field expression | 指出第三方 typed form contract 或明确 Reject；不把 `EditForm`/`Input*` adapter 当作 framework Support |
| Navigation/auth/JS | server authentication dependency、动态 JS invocation、尚未注册的 host capability | `NavigationManager`、route metadata 和 framework host contract 可被消费；认证 provider 缺失由 `JAZORVCA007` Guidance，`Router`/`RouteView`/`NavLink`/`AuthorizeView` 等内置组件标签不在本计划 |
| SSR state/form handoff | `PersistentComponentState`、`[PersistentState]`、`[SupplyParameterFromForm]` 或隐式 hydration payload | `JAZORVCA011` 在 authored property/attribute 位置阻断未定义协议；改用版本化 typed endpoint/bootstrap payload |
| Standard component adapter | 任意未纳入独立组件路线的 Microsoft 内置标签 | `JAZORVCA010` 作为稳定 Reject/Guidance descriptor；不得因历史 adapter 注册而静默放行或生成部分 Vue substitute |
| Render shape | 高置信 dynamic component、fragment recursion、跨 frame control flow | 能在作者源码判断时提前解释；不能确定时让 final Compilation 维持唯一诊断 |

分析器不能追踪不可靠的数据流、猜测动态 Type 来源，或重复 Razor SDK 的 RZ/CS 诊断。不能确定时保持静默，由 final Compilation 以精确 operation 状态决定。

兼容性诊断的消息必须按“这里用了什么、为什么当前宿主不能保真、框架已经能自动做什么、作者最小需要改什么”的顺序组织。HelpLink 用于背景和完整矩阵，不能成为完成普通页面的必读材料；对 Direct Support 形状保持零诊断噪音。

例如，页面直接注入 `DbContext` 时，诊断应落在 `@inject` 或 `[Inject]` 的作者 span：说明该服务依赖 server process，指出 RazorVue 不会把数据库上下文带进浏览器，给出 `HttpClient`/typed endpoint 或把操作移到 server endpoint 的最小替代，并链接到对应 profile；不应让作者先追踪 generated `BuildRenderTree`，也不应生成一个运行到浏览器才失败的 bundle。

### 4.4 诊断交付与配置契约

作者源码诊断必须随 RazorVue 的正常 package/buildTransitive 入口交付，在 IDE live analysis、`dotnet build`、CI 和 local package consumer 中使用同一套 descriptor、位置映射和去重规则。页面作者不需要额外安装 analyzer、手工添加 generated-code include 或打开专属 MSBuild target。

- analyzer assembly、规则版本和 `AnalyzerReleases` 随包 lockstep 发布；新规则先以明确的 preview/experimental 标记进入 release tracking，再升级为稳定诊断。
- error/warning severity、`.editorconfig` 分类和 suppression 行为必须有测试；禁止用 message text、生成文件路径或构建顺序决定 severity/ID。
- IDE 尚未提供 Razor source/symbol 配对信息时，规则保持静默并等待 final Compilation；不能退回 generated C# 位置或发出猜测性诊断。
- `dotnet build` 与 IDE 对同一 source fixture 必须产生相同 ID、severity、mapped span、HelpLink 和 artifact/no-artifact 结果；package consumer 再验证传递引用和 analyzer 不丢失。
- 代码修复只在语义可证明且可逆时提供；所有修复必须有 preview diff 和 compile/runtime 回归，不能自动把页面改写为 RazorVue 内部 API。

## 5. 运行时适配原则

### 5.1 组件与生命周期

Blazor 兼容实现按真实生命周期和可见行为设计，而不是按方法名匹配：

- 参数应用、SetParametersAsync、OnParametersSet 和 OnParametersSetAsync 必须有明确先后、异步串行、异常传播和过时 render 提交规则；generation guard 只能阻止过时 UI 提交，不能静默取消作者任务或改变其副作用；
- OnInitialized、OnAfterRender、ShouldRender、StateHasChanged、InvokeAsync、Dispose 和 DisposeAsync 必须在 mount/update/unmount 的正确时机运行；
- constructor、field/property initializer、base-to-derived 顺序和 injection 必须由可测试的 activation protocol 统一处理；
- 参数、cascading value 和 service 发生变化时，不能重复执行不该重复的 lifecycle，也不能漏掉需要执行的 lifecycle；
- SSR 与 hydration 不得重复一次性副作用；若需要 state handoff，必须由 P2 的显式 payload contract 完成。

### 5.2 服务、导航和上下文

浏览器 service adapter 不是服务器 ServiceProvider 的伪副本。它应有显式的 service descriptor、作用域和 host registration：

- singleton、component scoped 和 request/SSR scoped 的可用范围必须区分；
- service constructor、[Inject] property、CascadingValue、NavigationManager 和 authentication state 必须由同一 activation/context 层处理；
- 任何 server-only service 在作者源代码处被诊断；不允许在浏览器 bundle 中留下无法运行的 DI call；
- adapter 可以在内部使用 Vue provide/inject 或 router，但这些实现细节不进入页面作者 API；
- API/endpoint client 保持强类型 C# contract，客户端不直接操作数据库、HttpContext 或 server service。

### 5.3 Framework binding、auth state 和 validation boundary

本路线只定义 framework-level binding/event/auth contracts：

- 自定义组件的 `Value`/`ValueChanged`、`EventCallback<T>`、model update 和 event 复杂性由 lowering 或组件库 adapter 吸收；
- `AuthenticationStateProvider`、claims/auth state、SSR handoff 和 endpoint authorization 保持分层；UI 隐藏不能替代服务端授权；
- `EditContext`、`InputBase<T>`、`EditForm`、`ValidationMessage`、`InputFile` 等 Microsoft 内置组件/表单协议不由 M5 实现，使用第三方 UI 库或独立组件路线；
- 需要反射型 validation、server form post 或 antiforgery 的形状在作者源得到明确 Guidance/Reject，不留 runtime-first 失败。

### 5.4 自定义 fragments 与组件库边界

`RenderFragment`、`RenderFragment<T>`、templated component、slot 和普通 member reachable fragment 仍由 RazorVue lowering 直接支持；不把 `DynamicComponent`、`ErrorBoundary` 等 Microsoft 内置标签作为其默认入口。自定义或第三方组件以稳定 type/parameter contract 注册，未知类型、参数或 fragment 由 analyzer/final Compilation 裁决；不以裸 string component name、反射搜索或未声明模块 import 实现动态组件。

## 6. M5 里程碑

### M5-0：Blazor compatibility ledger 与基线

目的：把“尽量完整”变成可管理的能力清单，而不是无限承诺。

Ledger 每一行至少包含：Blazor 作者形状、目标语义基线、P0/P1/P2 优先级、Direct Support/Compatibility Adapter/Guided Adaptation/Reject 决策、运行时 owner、source diagnostic rule、最小源码 fixture、browser/SSR/package 证据和当前阻塞依赖。状态只能是 `Planned`、`In proof`、`Support`、`Guidance` 或 `Reject`；`Investigate` 不能作为长期交付状态。

交付物：

- 在 src/Jazor.RazorVue.Sg.Test/RazorVueUsageScenarioCatalog.cs 中按第 3 节登记每项状态、owner、最小源码和证明层级；**已完成首版 `RazorVueM5CapabilityLedger`，后续实现必须同步更新对应行，而不是只改路线文字；**
- 将现有 F1/F2/F3、lifecycle、bind、slot、parameter、HMR、SSR、JazorAdmin 场景映射到 ledger；
- 为当前 Reject 建立候选表：升级为 Direct Support、Compatibility Adapter、Guided Adaptation 或保留 Reject；
- 定义 analyzer ID（暂建议 `JAZORVCA001+`）、HelpLink、诊断去重和 source/final pipeline 边界；**`JAZORVCA001`-`010` 已锁定为作者源码规则：001/002 是 server-only Reject，003-005 是未物化 ParameterView 操作，006 是注入属性形状，007 是已知 host service adapter 缺失，008 是 cascading 属性形状；009/010 descriptor 保留为未纳入产品路线的 host/standard-component guidance。`SetParametersAsync(ParameterView)`、标准 cascading provider 和基础 routing 属于 framework primitive；Microsoft 内置 UI 组件及其历史 compatibility adapter 不进入当前产品契约，`JAZORVGA024` 仅保留给真正未实现的 final activation/protocol 形状。**
- 完成作者 `.razor`/`.razor.cs` source acquisition、symbol 配对和原始 span 映射的可行性证明；
- 明确新的 samples/RazorVue.Authoring library、host、package-consumer、browser smoke 和临时资源所有权。

退出条件：没有“生成成功但还不知道运行时是否正确”的作者能力进入后续阶段。

阶段依赖固定为：M5-0 先锁定作者 source 与证据合同；M5-A 和 M5-B 可在合同稳定后并行；M5-C 依赖 P0 的 activation/render 语义；M5-D 依赖 adapter host 与 SSR 证据；M5-E 的可复制样例只消费已经达到 Support 的能力，Guidance 仅用于明确迁移案例，不以样例 workaround 把未证明能力伪装成完成。

### M5-A：作者源码 compatibility analyzer

目的：让额外认知在编辑/构建时由分析提供，而不是在文档中预习。

交付物：

- 实现独立 RazorVue compatibility analyzer，只看作者 source；现有 generic analyzer 继续保持 `GeneratedCodeAnalysisFlags.None`，任何新规则都不得通过分析 generated C# 取得“提前诊断”；**当前已完成 `JAZORVCA001`-`010` 的高置信服务、ParameterView、typed cascading、route 和标准组件边界，并由 source span 回归锁定。**
- 实现规则族、稳定 ID、HelpLink、去重和高置信边界；
- 为 server-only DI、注入属性形状、known ParameterView/DI/cascading/form/navigation/JS 边界提供具体替代；browser service provider 缺失由 runtime adapter 给出明确激活错误，不能退化为 `undefined`；
- 为已经存在 Direct Support 的常见写法确保零噪音；
- 保持 final generator 的 no-partial-descriptor 不变量：analyzer 先报不代表 generator 可以留下半成品；source analyzer 与 final RenderTree/closure/module 规则通过互斥 ownership 或共享聚合层去重，不能依赖 analyzer 结果改变 generator 行为；
- analyzer 必须增量、按文档有界执行，不读取全量 generated compilation、不访问网络、不依赖构建顺序；IDE 输入不完整时宁可静默并交给 final Compilation；
- 在 authoring guide 中将完整限制表改为诊断的背景资料，而不是页面开发前置阅读。

退出条件：已知不可运行形状首先在 author source 被解释；无法静态确定的形状只由 final Compilation 报一次。

### M5-B：P0 标准 Razor 与组件语义闭环

目的：让普通页面的 Razor 写法不再暴露 lowering 细节。

交付物：

- 锁定泛型 TypeInference、开放泛型 OpenComponent、member reachable RenderFragment、typed slot、conditional fragment、@bind、attribute splat、event modifier、async callback 和 ref 的 Direct Support；
- 补齐 parameter replacement、lifecycle、render gate、error propagation、request race 和 selection/key identity 的 runtime proof；
- 在 VueModuleBuilder 最终 AST composition 边界实现模块完整性不变量，防止 builder leak、未定义 helper、缺失 import 和错误 alias；
- 建立 samples/RazorVue.Authoring 记录管理页面和 Release browser smoke；业务源码不出现 RazorVue 内部 API。

模块完整性审计只分析 Jazor 自己生成的 ESTree，绑定源为声明、参数、import local、runtime helper 与明确允许的 ECMAScript global。它必须区分 property key、member property、label、binding declaration 和自由引用，禁止 regex 扫描 .mjs 文本或宽泛 global allow-list。

退出条件：作者样例 P0 旅程在 official SG、debug、Release、真实浏览器下完成，且静态 module invariant 无漏报/误报回归。

### M5-C：P1 Blazor framework runtime primitives

目的：覆盖自定义组件所需的 Blazor framework primitive，不把 Microsoft 内置 UI 组件标签提升为产品支持。

交付物按实际语义依次推进：

1. ParameterView 和 SetParametersAsync 的参数快照、source-name sparse 覆盖链、slot/alias 传递、异步顺序和 error behavior。
2. browser service catalog、[Inject]/@inject property、activation lifetime 和 server-only service diagnostics；当前 property adapter 已进入 proof，constructor injection/parameterized activation 仍由 `JAZORVGA024` 明确拒绝，直到有完整 activation protocol。
3. CascadingValue、[CascadingParameter]、named cascade、嵌套 provider 覆盖与更新传播。
4. NavigationManager、自动生成 route catalog、route/query 参数更新和基础 history/popstate behavior；应用自有 route-host 子集已经由 official SG、Deno、真实浏览器和 isolated Release package 证明并进入 `Support`，同源内部 `NavigateTo` 的 LocationChanging cancellation 也已进入 `Support`（包含 PreventNavigation、异步 supersede/cancellation、query/hash、history state 和 registration dispose）。`replace`、LocationChanged 订阅、复杂 URI 状态、popstate/hashchange cancellation 与 SSR/prerender route identity 仍需独立补齐或保持边界。`Router`、`RouteView`、`LayoutView`、`NavLink` 标签不在本路线。
5. 自定义/第三方组件的静态 type token、registry、fragment/slot 和参数 descriptor contract；不以 Microsoft `DynamicComponent` 标签作为隐式 Vue 组件入口。
6. 自定义组件的 error propagation、`@ref`、dispose 和 render/update/unmount 组合；`ErrorBoundary` 标签和其历史 adapter 不在本路线。

每一项先写 adapter protocol 和行为矩阵，再改 lowering。不能将未实现的 adapter 标成 Support，也不能以运行时 null/undefined 作为回退。

退出条件：P1 ledger 中已承诺项全部具有 source、component runtime、browser 和适用 SSR 证明；未承诺项拥有 analyzer guidance。

### M5-D：P2 服务、状态和 SSR/hydration 兼容

目的：把复杂但高价值的 Blazor 代码迁移成可预测的浏览器应用。

交付物：

- authentication state provider、claims 和 host/endpoint contract；不以 `AuthorizeView`/`AuthorizeRouteView` 隐藏 UI 代替 endpoint 授权；
- 自定义组件的 typed binding/event contract；表单 UI、validation 和 file input 由 TDesign、Vuetify、Element Plus 或独立组件路线提供；
- IJSRuntime/IJSObjectReference/IJSInProcessRuntime/JSInvokable 的 compiler/final Compilation 使用点 Reject 诊断和强类型 ECMAScript/WebIDL binding 迁移说明；不建设 module registry 或 invocation adapter；
- prerender/SSR data handoff、PersistentComponentState 候选与 hydration 不重复副作用规则；
- route/not-found、query、error boundary 和 forms 的 SSR/hydration browser smoke。

退出条件：每个 P2 feature 都已实现并证明，或明确转为 Guided Adaptation；没有“文档暗示兼容、实际依赖服务器”的模糊状态。

### M5-E：组件 binding、样例、迁移和发布

目的：让开发者从第一天走 Blazor-first 路径，并安全升级。

交付物：

- 从 samples/JazorAdmin/AdminControls.cs 提炼重复但非自然的 binding authoring 摩擦；先形成 API review，再决定 sample-local、共享项目或公共 NuGet package；
- wrapper 只覆盖两个以上独立页面重复且原生 Razor 体验不自然的形状，保持 Value/ValueChanged、EventCallback<T>、Parameter、required/type contract 和 slot 类型；
- 新增 docs/03-guides/razorvue-quickstart.md，内容全部来自 samples/RazorVue.Authoring；更新 guides index 与完整 authoring guide 的入口；
- 将“零额外认知”作为 quickstart 的硬验收：从标准 Blazor 页面模板开始，除宿主必须选择的 API/base URL 等普通配置外，不要求页面作者记忆 RazorVue namespace、builder、module、特殊 parameter 类型或转译器术语；兼容 analyzer/runtime adapter 由正常 package 引用传递进入；
- 将已经验证的 bridge 迁移到 JazorAdmin 的重复页面，保留旧入口兼容期；
- 公共 API/新增 Support 走 MINOR，修复与文档走 PATCH，破坏性迁移按 release-and-versioning 规则记录。

退出条件：开发者可只跟 quickstart 完成记录管理页面；JazorAdmin 的复杂页面使用相同作者面；公共 package 改动由本地 package consumer 证明。

## 7. 验收矩阵

### 7.1 每项能力的最低证据

| 类型 | Analyzer | official Razor SG | 模块/Emit | 浏览器 | SSR/package |
| --- | --- | --- | --- | --- | --- |
| Direct Support | 对正常用法零噪音 | 必过 | import、source map、module invariant | 必过 | 触及时必过 |
| Compatibility Adapter | registration/error guidance | 必过 | adapter 物化和闭包完整 | 必过 | 触及 adapter lifetime/hydration 时必过 |
| Guided Adaptation | 必报且含替代 | 作者 source 可复现 | 无 partial artifact | 不依赖 runtime 发现 | 不适用 |
| Reject | 必报或 final pipeline 唯一报 | 可复现 | 无 partial artifact | 不依赖 runtime 发现 | 不适用 |

### 7.2 按触及面运行的门禁

| 触及面 | 必过验证 |
| --- | --- |
| RazorVue lowering、member closure、lifecycle、diagnostic | dotnet test src/Jazor.RazorVue.Sg.Test/Jazor.RazorVue.Sg.Test.csproj |
| authored-source analyzer | Jazor.Analyzer 或 RazorVue analyzer 的 source diagnostic suite，加 final pipeline 去重回归 |
| core C# semantics 或 CLR mapping | dotnet test src/Jazor.CompilerTest/Jazor.CompilerTest.csproj，必要时对应 CLR suite |
| module/manifest/import materialization | dotnet test src/Jazor.EmitTest/Jazor.EmitTest.csproj |
| samples/RazorVue.Authoring | Release browser smoke，覆盖 P0/P1/P2 已承诺旅程 |
| JazorAdmin bridge 或页面迁移 | dotnet test samples/JazorAdmin.Test/JazorAdmin.Test.csproj 和 JazorAdmin Release smoke |
| public package/buildTransitive | dotnet run --file scripts/csharp/test-dotnet.cs、RazorVue coverage gate、相应 local package consumer |
| SPA Release surface | dotnet run --file scripts/csharp/verify-windows-spa-release.cs -- --path-base /docs |
| SSR/hydration/package closure | dotnet run --file scripts/csharp/verify-windows-ssr-release.cs -- --path-base /todo |
| JazorAdmin DemoClient | 仅认证、门户、OIDC、shared host 或其 smoke infrastructure 受影响时运行 |

所有新 Support 必须在 source、AST/module、runtime 三层同时有回归；静态 emission snapshot 不能单独代表浏览器语义。

## 8. 完成定义

M5 完成时必须同时满足：

1. P0 全部为 Direct Support 或 Compatibility Adapter，作者样例的业务页面不含 RazorVue 内部 API、手写 JS、builder 或 object 逃生类型。
2. P1 的高频 Blazor API 已实现等价 adapter；尚未实现的 P1/P2 项由 authored-source analyzer 在首次 build 说明，不要求作者先读限制表。
 3. 任何无法静态确定的形状仍由 final Compilation 单次、mapped diagnostic 报告；没有重复诊断、partial descriptor/module、坏模块或 runtime-first failure。
4. samples/RazorVue.Authoring、JazorAdmin 迁移页面、Release/package consumer 和适用 SSR/hydration 的用户可观察行为一致。
5. quickstart 从 clean checkout 到 browser smoke 可复现，所有代码片段来自受测试样例。
6. 未参与 compiler 实现的人只按 quickstart 完成一次记录管理功能；出现的额外前置知识必须优先转化为 analyzer/diagnostic、自动 adapter 或更自然的 Blazor API。文档只解释背景，不得成为完成普通功能的前置知识。

## 9. 风险与控制

| 风险 | 控制 |
| --- | --- |
| “完整兼容”被误解为任意 server API 都可在浏览器运行 | P0/P1/P2 ledger 明确行为证明；server-only API 由 source analyzer 解释 |
| analyzer 与 final generator 重复报错 | author-source 高置信规则和 final operation 裁决分层；去重回归锁定 |
| 通用 JS interop 或 DI 引入弱类型、字符串调度或动态漏洞 | 明确拒绝 `IJSRuntime` 家族；浏览器 API 和模块只经 compiler-owned typed ECMAScript/WebIDL binding，禁止 arbitrary eval、open object fallback 和隐式 server access |
| lifecycle/SSR 有重复副作用 | lifecycle protocol、generation-aware async guard、SSR/hydration browser proof |
| bridge 演化成第二套不透明 UI framework | 只包装重复摩擦；原生 Razor 自然的 API 不加 wrapper；先 API review |
| 样例和真实消费者漂移 | 作者样例有 Release/package smoke；JazorAdmin 和 TodoList 各自保留独立责任 |
| Razor SDK 更新改变 generated shape | official SG fixture、authoring sample 和 analyzer/final pipeline 双层回归进入 SDK 升级门禁 |

## 10. 归属与文档层级

| 位置 | M5 责任 |
| --- | --- |
| src/Jazor.RazorVue | final Compilation、RenderEmitter、component activation、member closure、VueModuleBuilder、final diagnostics |
| RazorVue compatibility analyzer package/assembly（默认随 `Jazor.RazorVue` 传递交付） | authored source compatibility diagnostic；不分析 generated C#，不替代 final generator |
| src/Jazor.Compiler | 通用 C# semantics、host mapping、import、source origin；不承载 RazorVue 产品协议 |
| src/Jazor.RazorVue.Sg.Test | official SG、adapter behavior、diagnostic 去重、Deno/browser、source map |
| src/Jazor.EmitTest | artifact、manifest、package/import materialization |
| samples/RazorVue.Authoring | Blazor-first 页面作者合同样例和 browser smoke |
| samples/RazorVue.TodoList | HMR、PathBase、SSR/package consumer；保持小而稳定 |
| samples/JazorAdmin | 复杂业务和 binding adapter 的真实消费者，不定义通用领域 API |
| docs/03-guides/razorvue-quickstart.md | 页面作者第一入口 |
| docs/03-guides/razorvue-authoring.md | 完整兼容矩阵、诊断 HelpLink 和高级排查 |
| docs/04-roadmap/razorvue-authoring-diagnostics.md | final Compilation 决策、历史 ledger 和 SDK 升级门禁 |

M5 完成后，稳定兼容矩阵和作者入口收敛到当前状态与 guides；本文件只保留路线结论，实施过程进入历史演进。
