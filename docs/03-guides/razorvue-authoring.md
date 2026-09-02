# RazorVue 作者指南：组件 C# 边界与构建诊断

> 本指南描述当前 RazorVue 的作者面（authoring surface）。Razor SDK/Roslyn 负责 Razor 绑定、C# 类型检查和语法诊断；RazorVue 消费 official Razor Source Generator 完成后的最终 `Compilation`，把 VNode-producing `BuildRenderTree` 和其可达的组件 C# 成员分别降低为 Vue render-function `.mjs`。

## 先判断代码位置

RazorVue 有两个执行域。手写 `RenderTreeBuilder` 不是第三条路径，它与 Razor 标记走同一套 direct-render 协议。相同的 C# 语法是否可用，首先取决于它是否正在产出 VNode。

| 作者代码位置 | 执行域 | 当前决策 |
| --- | --- | --- |
| Razor 标记、组件标签、静态/表达式属性、`@(...)` | direct render + compiler bridge | **Support with constraints**。RenderEmitter 组织 VNode，表达式仍交给 `Jazor.Compiler`。 |
| 会输出标记的 `@if`、`@foreach`、`@for`、`@while`、`@do while` 和 `@{ ... }` | direct render | **Support with constraints**。它们是 `BuildRenderTree` 协议的一部分，而不是普通 C# 方法体。 |
| `@code`、`.razor.cs` 中的字段、属性、普通 helper、事件处理器、生命周期方法 | component logic | **Support with constraints**。从 render/event/lifecycle 可达的成员由 `SemanticWalker` 降低；业务循环和分支应放在这里。 |
| 手写或生成的 `BuildRenderTree(RenderTreeBuilder)` | direct render | **Support with constraints**。必须遵守 frame、metadata、fragment 和循环协议，不能当作任意 C# statement lowering。 |

两个域共享 Razor/C# 的前置诊断和 `Jazor.Compiler` 的类型/member 支持切片，但不能互相借规则：component logic 中的普通循环控制流由 compiler 保留；direct-render 中，能绑定到当前 `for`/`foreach`/`while`/`do while` 且不跨越未关闭 frame 的普通 `break`/`continue` 也会保留，RenderEmitter 会切换到 imperative loop path。`goto`、无法投影的 labeled branch 和跨 open frame 的跳转仍失败。不要通过 `object`、字符串拼接或手写 JavaScript 绕过这个区分，否则会失去 C# 类型检查、source map 和确定性 artifact 生成。

<a id="final-compilation"></a>
## Final Compilation 与错误生命周期

RazorVue 的最终管线顺序是：组件发现 -> final Compilation binding -> member closure -> VueInject registry -> direct RenderTree lowering/compiler bridge -> Vue module framing -> `ModuleCatalog` C# source generation -> source-generator reporting。任何阶段失败都会停止受影响组件；存在错误时不会生成部分 `ModuleCatalog` 条目或模块声明。

诊断不是从异常文本猜出来的。内部使用 typed `RazorVueDiagnosticInfo` 传递 category、已渲染 detail、primary/additional locations 和 component identity；descriptor 集中拥有 ID、severity 和 HelpLink。mapped `.razor` span 优先于 generated `.razor.g.cs` span。独立组件的错误按稳定组件身份和位置排序，因此同一输入在并行构建中仍有相同输出。

### Diagnostic ID

| ID | 所属边界 | HelpLink 锚点 | 常见动作 |
| --- | --- | --- | --- |
| `JAZORVGA020` | bootstrap、未知或未分类 internal failure | `#final-compilation` | 保留完整构建日志并提交最小复现；它不代表已知作者规则。 |
| `JAZORVGA021` | direct RenderTree 协议/形状 | `#direct-render` | 按 direct-render 章节改写标记或 builder 形状。 |
| `JAZORVGA022` | C# expression/compiler bridge | `#compiler-boundary` | 让表达式使用已有 whitelist/host contract，或改用受支持的值形状。 |
| `JAZORVGA023` | component binding | `#component-binding` | 检查 `BuildRenderTree`、组件模块声明和 component parameter。 |
| `JAZORVGA024` | member closure | `#member-closure` | 检查可达成员、constructor activation、lifecycle dispatch 和导出名；按本指南的 Support/Reject 子集调整源码。 |
| `JAZORVGA025` | `[VueInject]` declaration | `#vue-inject` | 修正 container/implementation contract 和重复声明。 |
| `JAZORVGA026` | Vue module/import/framing | `#vue-module` | 修正模块路径、导出名、import collision 或 runtime helper contract。 |
| `JAZORVCA001` | authored `[Inject]`/`@inject` 使用了 `DbContext` | `#browser-services` | 把数据访问移到 typed endpoint；组件继续注入 browser client。 |
| `JAZORVCA002` | authored `[Inject]`/`@inject` 使用了 server-only ASP.NET/Identity service | `#browser-services` | 把 request/identity 操作移到 server endpoint。 |
| `JAZORVCA003`-`005` | authored `ParameterView` 的未物化枚举/查找操作 | `#parameter-lifecycle` | 使用声明的 typed `[Parameter]` 属性。 |
| `JAZORVCA006` | authored `[Inject]` property 不是 writable auto-property | `#browser-services` | 改为普通 `get; set;` 属性。 |
| `JAZORVCA007` | 已知 Blazor host service 没有 browser adapter | `#browser-services` | 注册 typed browser adapter，或把操作移到 endpoint；页面仍写标准 `[Inject]`。 |
| `JAZORVCA008` | `[CascadingParameter]` 不是可激活的 writable auto-property | `#cascading-parameters` | 改为普通 `get; set;` 属性；标准 `CascadingValue`/命名级联由 browser adapter 自动处理。 |
| `JAZORVCA009` | 保留的旧 route-host descriptor（当前生成 route catalog 的 `@page` 不触发） | `#routing` | 正常页面无需注册 route host；若未来使用未覆盖的 host profile，按诊断给出的宿主配置处理。 |
| `JAZORVCA010` | 标准组件 adapter descriptor（不在当前产品契约中的 Microsoft 内置组件） | `#component-adapters` | 使用自定义组件或 TDesign/Vuetify/Element Plus 的 typed contract；不要依赖历史/实验 adapter。 |
| `JAZORVCA011` | `PersistentComponentState`、`[PersistentState]` 或 `[SupplyParameterFromForm]` 需要未定义的 SSR/form handoff | `#ssr-state-handoff` | 使用显式版本化的 typed endpoint/bootstrap payload；当前不支持隐式 hydration state、antiforgery 或 enhanced form protocol。 |

Razor SDK/Roslyn 的 `RZ****`、`CS****` 诊断仍由对应工具报告；RazorVue 不复制这些检查。

<a id="browser-services"></a>
## Browser Services

页面和组件仍按标准 Blazor 的 `[Inject]` 或 `@inject` 写法请求服务。RazorVue 只会把可在浏览器执行的 client/service adapter 放入组件运行时，不会把数据库上下文或服务器进程能力复制到 bundle。

| ID | 触发形状 | 最小动作 |
| --- | --- | --- |
| `JAZORVCA001` | `[Inject]` 或 `@inject` 的服务是 `DbContext` 或其派生类型 | 将数据访问放在 server endpoint，组件注入对应的强类型 browser client。无需改用 Vue、RenderTreeBuilder 或手写 JavaScript。 |
| `JAZORVCA002` | `[Inject]` 或 `@inject` 的服务是 `HttpContext`/`IHttpContextAccessor`、ASP.NET host environment 或 ASP.NET Identity manager 等 server-only 类型 | 将读取/写入动作移到 server endpoint，组件只注入强类型 browser client；不要把 request、response 或 server identity manager 当作浏览器服务。 |
| `JAZORVCA006` | `[Inject]` 属性不是可写 auto-property（readonly、`init`、custom setter 或 static） | 改为普通的 `get; set;` 属性；服务由宿主 provider 注册，页面不需要写 `provide`/`inject` 或 Vue glue。 |
| `JAZORVCA007` | `IComponentActivator`、circuit/protected-storage 等已知 Blazor host service 没有 browser adapter | 注册 typed browser adapter，或将服务调用封装到 server endpoint；不要在页面中自行调用 Vue provide/inject。 |

浏览器可执行服务的 `[Inject]`/`@inject` 属性会在 component 初始化和生命周期回调前自动解析。provider key、生命周期和缺失 provider 错误由宿主 adapter 负责；页面作者继续使用标准 Blazor 注入语法。当前 adapter 的服务入口是属性注入，参数化 constructor activation 仍按 `JAZORVGA024` 明确拒绝。

Blazor JS interop 不属于 browser service adapter。`IJSRuntime` 的 identifier string、`object[]` 参数编组、动态 import 和 runtime dispatcher 会绕开 Jazor 的静态 import 与模块依赖约束，因此在实际类型或成员使用点由现有 compiler/final Compilation 不支持诊断拒绝；这里不新增专用 analyzer 规则或兼容层。需要 JavaScript 能力时，使用已有的强类型 `ECMAScript`/WebIDL binding，或为静态模块 API 添加同样强类型的 binding declaration；调用、导入和资源闭包仍由 `Jazor.Compiler` 与 `Jazor.Emit` 统一处理。

不要把这条 Reject 理解为拒绝所有带 Blazor 形状的组件。组件类型仍按独立的 RazorVue 入口契约判断：必须可赋值给 `ComponentBase`、实现 `IVueComponent`（包括泛型或派生 marker），并声明 `[ECMAScriptModule("...")]` 或 `[ECMAScript("package", Transform.Component, "Export")]`。其中 `Transform.Component` 是静态库组件 import 描述；它与 `IVueComponent` 共同构成受支持的组件类型，不是 `IJSRuntime` 兼容层。

这些规则只检查作者的 `.razor`、`.razor.cs` 和普通 C# component source；它们不分析 Razor SG 生成的 C#，因此不会复制 `JAZORVGA020`-`026` 的 final Compilation 诊断。其他服务在尚未能够静态证明其 browser contract 前不产生猜测性 warning；最终 lowering 失败仍由既有、唯一的 final diagnostic 报告。

<a id="cascading-parameters"></a>
### Cascading 参数

`CascadingValue<T>` 与 `[CascadingParameter]` 已由 browser adapter 物化。页面作者继续使用标准 Blazor 写法，支持按类型或 `Name` 匹配、嵌套 provider 的最近值覆盖、显式 `null`、无 provider 时保留属性默认值，以及 `IsFixed` 对后续更新的语义。provider 更新会先同步级联属性，再运行 `OnParametersSet`/`OnParametersSetAsync`，最后请求渲染；不会只更新 DOM 而跳过生命周期。

`JAZORVCA008` 只针对无法由 adapter 激活的属性形状（readonly、`init`、custom setter 或 static）。改为普通 writable auto-property 即可；不需要写 Vue `provide`/`inject`，也不需要增加 RazorVue 专属参数类型。

<a id="routing"></a>
### 路由

`@page`、`@layout`、route parameter 和 `[SupplyParameterFromQuery]` 会由 official Razor SG symbols 自动生成稳定 route catalog；`NavigationManager` 和应用自有页面 host framing 可以消费该 contract。`Router`、`RouteView`、`LayoutView`、`NavLink` 等 Microsoft 内置 UI 组件没有 RazorVue 组件入口，不属于当前产品契约；页面应使用应用自定义 host 或明确的 Vue Router/组件库 contract。当前已 Support 的 route-host 子集覆盖初始匹配、layout composition、query/route prop 映射、pushState、popstate、query refresh、not-found 和 browser history；该子集已由 isolated Release package 的真实浏览器 journey 验证。`replaceState`、LocationChanged 订阅、复杂 constraint/fragment/history state 和 SSR/hydration route identity 仍在对应 M5 proof 阶段。

`NavigationManager.RegisterLocationChangingHandler(...)` 的 browser-interactive 支持范围是同一 base URI 的内部 `NavigateTo`：handler 可以读取 `TargetLocation`、`HistoryEntryState` 和 `CancellationToken`，调用 `PreventNavigation()`，通过 `CancellationToken.Register` 观察被后续导航 supersede 的取消，并在返回的 `IDisposable` dispose 后停止拦截。该子集已通过 Blazor reference oracle、official Razor SG、Deno、真实 HTTP-origin browser 和 isolated Release package consumer；`forceLoad`、外部 URI、`popstate`/`hashchange` cancellation、server circuit 与 SSR/prerender route identity 不在声明内。`NavigationOptions` 在 C# 中使用 `Microsoft.AspNetCore.Components.NavigationOptions`，避免与 `ECMAScript.NavigationOptions` 绑定产生歧义；后者对应 Web Platform Navigation API 的 options dictionary，不应重命名。

<a id="component-adapters"></a>
### 标准组件适配器

Microsoft.AspNetCore.Components 提供的 `DynamicComponent`、`ErrorBoundary`、`EditForm`、`Input*` 等内置 UI 组件不属于 RazorVue 组件入口；只有满足 `ComponentBase` + `IVueComponent`（或派生接口）并声明导入描述的自定义/第三方组件才会进入 lowering。工作树中仍可能存在历史/实验 adapter 和对应测试，它们不构成产品支持证据；页面应改用自定义组件或 TDesign/Vuetify/Element Plus 的 typed contract，避免依赖运行时替代。

<a id="component-logic"></a>
## Component C# Logic

`@code` 与 `.razor.cs` 共同组成同一个 partial component。RazorVue 从 `BuildRenderTree`、已识别的生命周期入口和其捕获的 handler 开始收集闭包，再沿当前组件及其源码基类的成员调用展开。未被该闭包引用的成员不会进入模块；这不是按 public/private 或文件位置全量转译 C# 类型。

| 形状 | 当前决策 | 作者约束 |
| --- | --- | --- |
| 非参数 instance field、auto-property 及其 initializer | **Support** | 可达成员会物化为 Vue reactive state。用 field/property initializer 表达默认值；它会进入 setup 状态初始化。 |
| `[Parameter]` property | **Support with Vue semantics** | 映射为 Vue props，不应在组件内赋值。props 是 shallow reactive：替换引用会触发参数处理，同一引用内部 mutation 不构成新的参数赋值承诺。 |
| 有 getter body 的非参数 property | **Support** | 作为 computed member 降低；getter 中用到的成员同样必须在 compiler 支持切片内。 |
| 普通 instance/static helper、事件处理器、相互调用的可达方法 | **Support with constraints** | 只有 render、handler、lifecycle 或另一可达成员能到达的方法会发射。业务计算应收敛在这些方法中，而不是塞进 VNode-producing block。 |
| 普通方法中的 local、赋值、条件、`for`/`foreach`/`while`/`do while`、非标签 `break`/`continue`、return、switch expression | **Support with compiler constraints** | 这些是 component logic，不受 direct-render 的 straight-line loop 限制。每个表达式、类型和 member 仍需可由 `Jazor.Compiler` 降低；官方运行时回归覆盖了 `@code` handler 中的 `foreach`、`break`、`continue`、helper 调用和 switch expression。 |
| `goto`、无法投影的 labeled branch | **Reject** | 当前 compiler 没有跨 imperative/render fragment 边界的稳定目标协议；改为显式条件、返回或拆分 helper。普通 loop `break`/`continue` 不属于此行。 |
| `async` handler、`Task`/`ValueTask` lifecycle、`EventCallback` | **Support with constraints** | 仅使用已有 CLR/host mapping；不要把任意 .NET task/service API 当作浏览器运行时 API。`EventCallback` 的 listener await 语义与 Razor SG 绑定形状一起覆盖。 |
| `SetParametersAsync(ParameterView)` override | **Compatibility Adapter (In proof)** | 保持标准 Blazor 写法。RazorVue 为该入口建立 per-instance ParameterView snapshot，按 source parameter name 应用 sparse overlay，再按 Blazor 顺序调用 `OnInitialized*`/`OnParametersSet*`；异步更新串行排队。Windows SSR Release consumer 也证明 serialized props 会在 server HTML/hydration 入口等待初始参数任务；完整 snapshot/reference parity、取消深度和 SSR 异常仍未宣称。未支持的 `ParameterView` API 会在作者源码处给出兼容性诊断。 |
| 源码基类声明的 lifecycle/dispose entry point，或间接 lifecycle override | **Support with CLR dispatch constraints** | 当前组件及其源码基类属于同一 member closure；按真实 virtual/interface dispatch 选择最派生实现，再在 Vue setup/unmount 中执行。外部已编译基类仍不进入源码 closure。 |
| static field、static auto-property、可达 static helper/accessor | **Support with module lifetime** | static storage、helper 和可达 nested runtime class 在 artifact module 作用域只初始化一次，不进入每个 setup 的 `reactive` state。不要把它们当作组件实例隔离状态。 |
| 嵌套 non-record runtime class | **Support with constraints** | 可达的创建和成员会进入闭包。实例可能被 Vue deep Proxy 包装，因此 private storage 会降为 `$jazor$private$...` ordinary property；不要在作者代码中依赖该实现名。 |
| record、interface | **Support with compiler semantics** | record 是 structural lowering，interface 是编译期 contract；两者不等同于可随意保留 CLR runtime identity 的对象模型。 |
| 无参显式组件/源码基类实例构造函数、默认 `base()` 链、constructor body | **Support with constraints** | setup 先建立 CLR default state，再按 base-to-derived 执行 field/property initializer 与 constructor body；constructor 中的普通 C# 仍需 compiler 支持。 |
| primary-constructor 参数、参数化 activation、`this(...)`、`base(args)` | **Reject (`JAZORVGA024`)** | Vue setup 当前没有向 component activation 传递 CLR constructor 参数的协议；把输入改为 `[Parameter]`/VueInject，或把初始化收敛到无参 constructor/lifecycle。 |
| 当前组件 indexer | **Reject** | 当前 state/props projection 没有 indexer runtime protocol。改为普通方法或显式集合成员。 |

### 推荐的复杂逻辑形状

把标记保持为声明 UI，把分支、循环和计算放到 `@code` 或 `.razor.cs` 的普通成员中。下面的 handler 是 component logic，不是 direct render；其中的 `continue`/`break` 因而合法。

```razor
@using Microsoft.AspNetCore.Components.Web

<button type="button" @onclick="Process">@Status</button>

@code {
    private int[] samples = [2, -1, 3, 99];
    private string Status { get; set; } = "idle";

    private void Process()
    {
        var total = 0;
        foreach (var value in samples)
        {
            if (value < 0)
                continue;
            if (value > 10)
                break;

            total += Normalize(value);
        }

        Status = total switch
        {
            0 => "empty",
            > 4 => "large",
            _ => "small"
        };
    }

    private static int Normalize(int value) => value;
}
```

同一逻辑若写进包围标记的 `@for` 或手写 `BuildRenderTree`，就转入 [Direct Render](#direct-render) 约束：普通、绑定到当前 loop 且不跨 open frame 的 `break`/`continue` 可以保留，但其余复杂控制流不能直接照搬。

<a id="parameter-lifecycle"></a>
### 生命周期与组件运行时入口

RazorVue 按 Roslyn 的真实 override/interface 关系识别入口，不会因一个普通方法恰好同名就赋予 lifecycle 语义。当前组件和源码基类的可达 hook 会按 CLR 的 virtual/interface dispatch 解析到实际目标。`SetParametersAsync(ParameterView)` 使用 per-instance compatibility adapter；页面作者仍只写标准 Blazor 入口，不需要接触 Vue props 或 generated C#。

| C# 入口 | Vue 映射与约束 |
| --- | --- |
| `OnInitialized` | 在 setup 中执行。 |
| `OnInitializedAsync` | setup 中调用，并在 Promise settle 后请求一次重新渲染；rejection 会在下一次 render 重新抛出。 |
| `OnParametersSet` | 初次 setup 执行，随后由 shallow props watch 触发。 |
| `OnParametersSetAsync` | 初次及 props 更新时串行执行；旧一轮异步完成不能使新参数状态失效，rejection 会进入下一次 render。 |
| `OnAfterRender(bool)` | 分别映射 Vue mounted/updated，参数为 `true`/`false`。 |
| `OnAfterRenderAsync(bool)` | 同样在 mounted/updated 调用；Vue 不等待该 hook，成功完成不会自动请求重新渲染，rejection 会在下一次 render 重新抛出。 |
| `ShouldRender` | 作为 cached VNode 的 render gate。 |
| `IDisposable.Dispose` / `IAsyncDisposable.DisposeAsync` | 映射 unmount；异步 dispose 会调用但 Vue unmount hook 不等待它，卸载后续 completion 不再触发组件失效。 |
| `StateHasChanged` | 仅当前组件 receiver 的调用有 runtime 支持；unmount 后调用会失败。 |
| `InvokeAsync(Action)` / `InvokeAsync(Func<Task>)` | 仅当前组件 receiver 的窄调用面有支持；unmount 后返回 rejected Promise。 |

### 仍有明确环境边界的标准 API

Razor SDK 能编译不等于每个 ASP.NET Core runtime API 都能在浏览器执行。已注册、可执行的服务可直接使用标准 `[Inject]`/`@inject`；数据库上下文、请求上下文、服务器 host/Identity 服务由 `JAZORVCA001`/`JAZORVCA002` 在作者源码处拒绝，已知但没有 adapter 的 Blazor host service 由 `JAZORVCA007` 说明。当前 profile 没有隐式 `AuthenticationStateProvider`；除非宿主另行注册并证明 typed browser provider，否则该注入保持 Guidance。`NavigationManager` 的基础属性注入可由 browser service adapter 提供；同源内部 `NavigateTo` 的 LocationChanging 子集可直接使用，但 `popstate`/`hashchange` cancellation、server circuit 和 SSR/prerender route identity 不在声明内。`IJSRuntime` 家族没有 RazorVue 兼容层，实际调用或成员访问由现有 compiler/final Compilation 在使用点报告不支持，作者应改用强类型 ECMAScript/WebIDL binding。标准 cascading 和基础 route catalog 仍属于 framework primitive。Microsoft 内置 UI 组件不因存在历史 adapter 而进入产品契约；复杂 forms/validation、LocationChanged 订阅、SSR/hydration 和其它未证明能力不会静默生成运行时 `undefined`，而由对应 guidance 或 final Compilation 在作者映射位置说明。

`ParameterView` 的标准 `SetParametersAsync(ParameterView)` 入口已经由 compatibility adapter 支持；不要枚举参数、调用 `TryGetValue` 或 `ToDictionary`。这些未物化操作会分别报告 `JAZORVCA003`、`JAZORVCA004`、`JAZORVCA005`，建议改用已声明的 typed parameters。`[VueInject]` 仍是组件库级 contract（见 [VueInject](#vue-inject)），不是页面作者完成普通服务注入的前置知识。

<a id="ssr-state-handoff"></a>
### SSR 状态与表单交接

`PersistentComponentState`、`[PersistentState]` 和 `[SupplyParameterFromForm]` 依赖服务器 renderer、序列化 payload、请求边界和 hydration 时机。当前 RazorVue 没有版本化的 state envelope、checksum、失配处理或 enhanced form/antiforgery 协议，因此不会把这些 API 静默降级成浏览器全局状态。作者源中出现注入或特性时会报告 `JAZORVCA011`，位置落在对应类型或 attribute 上；这不是要求作者改写成 builder 或 Vue API。

对于需要 SSR 首屏数据的页面，使用显式、强类型 endpoint 返回 bootstrap DTO，并在组件初始化时消费该 DTO；服务端仍是授权和数据事实来源。重复提交、过期 payload、状态失配和表单防伪需要由 endpoint/host 自己处理。只有在未来版本化 payload 与 hydration 副作用矩阵完成后，才会把具体子集提升为 Compatibility Adapter 或 Support；内置 `EditForm`、`Input*`、`AuthorizeView` 等组件仍不属于本路线。

<a id="direct-render"></a>
## Direct Render

Razor SG 生成的 builder 调用按顺序解释为 Vue VNode；手写 `BuildRenderTree` 也完全相同。以下约束只适用于产出 VNode 的代码，它们是 RenderTreeBuilder 协议约束，不是对 `@code`/`.razor.cs` 任意 helper body 的通用 C# 禁令：

| RenderTreeBuilder/statement 形状 | 当前决策 | 约束 |
| --- | --- | --- |
| `OpenElement`/`CloseElement`、`OpenComponent`/`CloseComponent`、`OpenRegion`/`CloseRegion` | **Support** | 形成严格 LIFO frame 栈；tag/component type 必须静态可分析。 |
| `AddContent`、`AddMarkupContent`、组件 child content | **Support with constraints** | content expression 走 compiler bridge；fragment/slot 必须有可分析来源。 |
| `AddAttribute`、`AddComponentParameter`、`AddMultipleAttributes`、event metadata | **Support with constraints** | 名称必须为 compile-time string，且必须位于该 frame 的第一个 child 前。 |
| `SetKey`、`SetUpdatesAttributeName`、element/component reference capture | **Support with constraints** | 只能作用于正确的当前 frame，不能在 child 之后倒写 metadata。 |
| invocation/expression statement、block、已初始化 local、`if`、已建模循环、direct `return` | **Support with constraints** | 只能组成 RenderEmitter 可识别的 render segment；不是任意 statement-to-JavaScript 转译。 |
| 动态 tag/attribute/parameter 名称、运行时 component `Type`、未知外部 `RenderFragment` factory | **Reject (`JAZORVGA021`)** | 改用静态分支、显式组件类型或 inline/local/helper/slot fragment。 |
| 未初始化 local、frame 外 metadata、`goto`、无法投影的 labeled branch | **Reject (`JAZORVGA021`)** | 改用初始化、正确 frame 顺序或 [Component C# Logic](#component-logic) 中的普通 helper。普通 loop `break`/`continue` 见下方循环规则。 |

- `OpenElement`、`OpenComponent`、`OpenRegion` 必须与对应 close 成对，且按栈顺序关闭；
- `OpenComponent<T>` 支持开放泛型组件类型（例如 `OpenComponent<TTable<T>>()`）；类型参数只作为编译期注解擦除，组件仍必须有 `[ECMAScriptModule]` 或 `[ECMAScript(..., Transform.Component, ...)]` 描述。official Razor SG 生成的 `TypeInference.Create*_0<T>` 辅助会在当前 fragment builder 作用域内内联，构造方法参数与方法体原始定义对齐，不会把 `builder`/`__builder` 泄漏到最终模块。运行时动态 `Type`、无法静态确定的组件类型仍报告 `JAZORVGA021`。
- element/component 的属性、component parameter、splat 和 event metadata 必须在第一个 child 之前写入；
- tag、attribute、parameter、event modifier 和 bulk-attribute 名称必须是 compile-time string；
- `SetKey`、`SetUpdatesAttributeName`、reference capture 和 render-mode metadata 必须作用于正确的当前 frame；
- `OpenComponent` 使用 generic component type 或 `typeof(T)`，不能把运行时 `Type` 值当作动态组件类型；
- `RenderFragment`/slot 必须能解析为 inline、local、helper 或 component slot source；任意外部 factory、递归 render helper 和未闭合 fragment 会被拒绝；
- RenderFragment 属性/方法如果被任一已发射的普通 component member 引用，会随 member closure 一起保留；不要假设“只有渲染树直接调用的片段才会发射”。若片段 body 本身不在支持切片内，编译器必须报告 `JAZORVGA024`，不会静默裁剪成未定义名称。
- sequence 参数只允许无副作用表达式；sequence 不是运行时排序值，不要用 `NextSequence()` 之类调用填充它。

### 循环与分支

已支持的循环会生成 Vue fragment：无控制流分支的 `@foreach` 使用 Vue `renderList` 快路径，普通 `@for`、`@while`、`@do while` 使用受控 fragment lowering。循环体包含普通 `break`/`continue` 时，会切换为同一 render pass 内的 imperative JS loop，并按源顺序把已完成的 VNode segment 写入结果数组；`foreach` collection 仍由 compiler 提供 string/iterable 语义。需要输出的循环体必须形成可识别的 RenderTreeBuilder content segment；纯控制流 loop 虽可降低但不会产生 VNode，业务计算应放在 component logic。需要 compiler 临时变量的 initializer/condition/update 或混杂未建模 statement 会得到 `JAZORVGA021`。

direct render 中普通 `break`/`continue` 只支持绑定到当前 loop 的结构化目标，且 branch 前必须已经关闭 element/component/region frame；这保证 branch 在真实 JS loop 作用域中执行，不会错误地落入 vnode mapper/IIFE。跨 loop、`goto`、无法投影的 labeled branch，以及 branch 时仍打开 frame 的形状仍报告 `JAZORVGA021`。需要保留这类复杂控制流时，把计算移到 component logic helper，再让 render 只消费结果。

### 常见替代

| 失败写法 | 推荐写法 |
| --- | --- |
| `builder.OpenElement(0, tag)` | 使用静态标签，或为不同标签写显式 `@if` 分支。 |
| child 之后再 `AddAttribute`/`SetKey` | 在 open frame 后、任何 child 前设置 metadata。 |
| `AddContent(0, SomeFactory())` 返回未知 `RenderFragment` | 使用 inline fragment、已声明的 slot 或可分析的 helper。 |
| 在 frame 中声明未初始化 local | 先初始化，或把纯 C# 计算移到 frame 之前。 |
| 动态 `OpenComponent(0, type)` | 使用静态组件类型或显式组件分支。 |

<a id="compiler-boundary"></a>
## Compiler Boundary

RazorVue 不会在 direct-render 层重新实现 C# 成员、调用、转换或 whitelist 语义。标记表达式和 component logic 都交给 `Jazor.Compiler`/`SemanticWalker`；当 operation 无法生成 JavaScript expression/statement、访问了未支持的 external type/member，或 host mapping 失败时，报告 `JAZORVGA022`。

`SemanticWalker` 覆盖的是有明确 JavaScript/CLR host 语义的 compiler slice，不是任意 .NET runtime 的模拟器。先看值的类型和被调用的成员是否已有 whitelist/host mapping；不能因为代码位于 `@code` 就假定任意 BCL、反射、线程、文件/网络或 ASP.NET Core server API 都可执行。诊断位置来自 Roslyn operation/symbol 的原始 `Location`，再通过 mapped span 投影回 `.razor` 或作者 `.razor.cs`。不要依赖异常消息中的 generated C# 行号。

泛型参数、数组元素和集合元素的类型在未进入运行时敏感 lowering 时保持 erased；真正的成员访问、构造、运行时类型检查才会在使用点拒绝。不要把参数、返回值或 collection 退化为 `object?` 来躲避该检查：保留可表达的强类型 C# surface，必要时用已有 union/host value contract。

<a id="component-binding"></a>
## Component Binding

组件必须能从 final Compilation 中解析出可绑定的 `BuildRenderTree(RenderTreeBuilder)` block，并同时满足 RazorVue 组件身份契约：可赋值给 `ComponentBase`，实现 `IVueComponent` 或其派生接口，且声明 `[ECMAScriptModule("...")]` 或 `[ECMAScript("package", Transform.Component, "Export")]` 导入描述。官方 Razor SG 生成的 component parameter、required parameter 和参数类型错误仍由 Razor SDK 负责；RazorVue 只报告它无法绑定或无法消费的最终形状。

组件模块应使用稳定的 `[ECMAScriptModule("...")]` 或 `[ECMAScript("package", Transform.Component, "Export")]`。组件引用、parameter 名称和 child content 必须与编译期 symbol 对齐；不要依赖运行时字符串查找组件。标准 Blazor 内置 UI 组件没有 `IVueComponent` marker，不属于 RazorVue 组件入口。

<a id="member-closure"></a>
## Member Closure 与 Reactive Class

member closure 只物化 render、已支持 lifecycle、constructor replay 和捕获 handler 可达的字段、属性、方法、nested runtime class 及其依赖，并将当前组件的源码基类视为同一作者成员面。无法确定成员导出名、类型或引用关系时报告 `JAZORVGA024`，而不是生成一个运行时才失败的空引用。

组件本身不是由 Vue setup `new` 出来的 CLR object；因此 constructor 支持采用结构化 replay：先创建每个 state slot 的 CLR default，再按基类到派生类执行 source initializer 和无参 constructor body。该 replay 不改变普通 nested runtime class 的 class lowering；nested class 仍按其自身 constructor protocol 执行。

### Proxy-safe class storage

当 runtime member class 进入 Vue `reactive()` 或其他 deep Proxy，JavaScript private field 的 brand check 会针对 Proxy receiver 失败。RazorVue 在 Vue member-closure profile 中把非 public field、auto-property backing field、primary-constructor capture 和 field-like event storage 降为稳定的普通 mangled property，例如 `$jazor$private$...`。它仍保持 class identity、继承和访问顺序；该名称是实现细节，不应在作者代码中引用。

这项 Support 由 official Razor SG + deep Proxy Deno regression 覆盖。不要把 SSR runner 的显式失败或把 class 改成 record 当作通用修复。组件参数仍遵守 Vue 的 shallow-prop 语义：需要触发父子更新时替换整个引用，或使用明确的 Vue ref/reactive contract。

<a id="vue-inject"></a>
## VueInject

`[VueInject]` 是 compilation 级声明协议。注入角色必须引用命名 component type，container contract、implementation 和导出名必须满足当前 registry 规则，重复或冲突声明会报告 `JAZORVGA025`。修正声明本身，不要在组件里添加运行时 fallback；registry 失败时不会生成部分 `ModuleCatalog` 条目。

<a id="vue-module"></a>
## Vue Module 与 Union

`JAZORVGA026` 覆盖模块 framing、import alias、runtime helper 和最终 Emit 物化失败。模块路径应稳定、可解析且与对应 JS resource manifest 的实际 package entry 一致；不要手工拼接 import 文本来绕过 `SemanticWalker` 的 import collection。

Vue host value domain 优先使用 C# native `union`，例如：

```csharp
[Parameter]
public Vue.VueBooleanStringValue Mode { get; set; } = true;
```

union 是 authoring/compile-time contract，运行时按其分支值擦除；保留 `AsX` projection 和正常赋值/隐式构造。官方 Razor SG 绑定也必须能编译该参数面；如果某个 union 形状不能被 Razor SG 合法绑定，应缩小为显式 overload 或强类型参数，而不是退化为 `object?`。

## 排查顺序

1. 先修复同一 compilation 中的 `CS****`/`RZ****`，它们可能使 generated C# 不完整。
2. 查看 `JAZORVGA` ID、mapped path/line/column 和 HelpLink；不要只复制异常末尾文本。
3. 按 ID 对照本指南章节，保留一个最小 `.razor` 或 `.razor.cs` 复现。
4. 确认失败时没有部分 `ModuleCatalog` 条目；最终宿主 Emit 也不会留下半套 `.mjs` 输出。
5. 若认为形状应该被支持，请同时提交：生成的 `BuildRenderTree` 形状、预期 Vue render-function、状态/SSR 语义和最小回归；不要先添加 silent fallback。

## 升级门禁

升级 .NET、Roslyn 或 Razor SDK preview 时，至少运行：

- `SemanticWalkerOrdinaryTest` 的 ordinary/labeled `IBranchOperation` 与 `BranchKind` gate；
- official Razor `for`、`while`、`do while` runtime tests；
- `RazorSgOfficialNativeUnionParameterAuthoringTests`，验证 native union 参数可由 Razor SG 绑定并进入最终模块；
- `BootstrapPatchTests`（文件 `RazorSourceGeneratorBootstrapPatchTests.cs`）的 mapped diagnostic 和无 partial `ModuleCatalog`/module gate；
- `RazorSgOfficialNestedRuntimeClassClosureRuntimeTests` 的 deep Proxy regression；
- `MemberClosureBuilderContractTests` 的显式 component/source-base constructor、未映射 runtime entry 拒绝与 mapped diagnostic gate；
- `RazorSgOfficialRuntimeAuthoringTests.BuildComponent_OfficialRazorCodeBlock_ExecutesComplexComponentLogicOnDenoHost`，验证 `@code` 业务循环与 direct-render 循环边界；
- `dotnet test src/Jazor.RazorVue.Sg.Test/Jazor.RazorVue.Sg.Test.csproj`、`dotnet test src/Jazor.CompilerTest/Jazor.CompilerTest.csproj`，以及发布/SSR consumer gate。

这些门禁检查的是 operation contract、作者位置和运行时语义；旧 snapshot 通过本身不足以证明 preview SDK 升级安全。
