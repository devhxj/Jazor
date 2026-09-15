# RazorVue 作者指南：组件 C# 边界与构建诊断

> 本指南描述当前 RazorVue 的作者面（authoring surface）。Razor SDK/Roslyn 负责 Razor 绑定、C# 类型检查和语法诊断；RazorVue 消费 official Razor Source Generator 完成后的最终 `Compilation`，分别降低 VNode-producing `BuildRenderTree` 和其可达的组件 C# 成员，产出 Vue render-function `.mjs`。

## 先判断代码位置

RazorVue 有两个执行域。手写 `RenderTreeBuilder` 与 Razor 标记共同遵循 direct-render 协议。C# 语法的适用规则由其是否产出 VNode 决定。

| 作者代码位置 | 执行域 | 当前决策 |
| --- | --- | --- |
| Razor 标记、组件标签、静态/表达式属性、`@(...)` | direct render + compiler bridge | **Support with constraints**。RenderEmitter 组织 VNode，表达式仍由 `Jazor.Compiler` 降低。 |
| 会输出标记的 `@if`、`@foreach`、`@for`、`@while`、`@do while` 和 `@{ ... }` | direct render | **Support with constraints**。它们遵循 `BuildRenderTree` 协议。 |
| `@code`、`.razor.cs` 中的字段、属性、普通 helper、事件处理器、生命周期方法 | component logic | **Support with constraints**。从 render/event/lifecycle 可达的成员由 `SemanticWalker` 降低；业务循环和分支应放在这里。 |
| 手写或生成的 `BuildRenderTree(RenderTreeBuilder)` | direct render | **Support with constraints**。遵循 frame、metadata、fragment 和循环协议。 |

两个域共享 Razor/C# 的前置诊断和 `Jazor.Compiler` 的类型/member 支持切片，分别适用各自规则：component logic 中的普通循环控制流由 compiler 保留；direct-render 中，绑定当前 `for`/`foreach`/`while`/`do while` 且位于已关闭 frame 之后的普通 `break`/`continue` 进入 imperative loop path。`goto`、labeled branch 和跨 open frame 跳转通过 `JAZORVGA021` 说明当前范围。运行时语义通过强类型 C#、host mapping 和 compiler 入口表达。

<a id="final-compilation"></a>
## Final Compilation 与错误生命周期

RazorVue 的最终管线顺序是：组件发现 -> final Compilation binding -> member closure -> VueInject registry -> direct RenderTree lowering/compiler bridge -> Vue module framing -> `ModuleCatalog` C# source generation -> source-generator reporting。诊断阶段停止受影响组件，Emit 维持完整 `ModuleCatalog` 条目和模块声明。

诊断由 typed `RazorVueDiagnosticInfo` 传递 category、已渲染 detail、primary/additional locations 和 component identity；descriptor 集中拥有 ID、severity 和 HelpLink。mapped `.razor` span 优先于 generated `.razor.g.cs` span。独立组件的错误按稳定组件身份和位置排序，因此同一输入在并行构建中保持相同输出。

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
| `JAZORVCA001` | authored `[Inject]`/`@inject` 使用了 `DbContext` | `#browser-services` | 数据访问移到 typed endpoint；组件继续注入 browser client。 |
| `JAZORVCA002` | authored `[Inject]`/`@inject` 使用了 server-only ASP.NET/Identity service | `#browser-services` | request/identity 操作移到 server endpoint。 |
| `JAZORVCA003`-`005` | authored `ParameterView` 的未物化枚举/查找操作 | `#parameter-lifecycle` | 使用声明的 typed `[Parameter]` 属性。 |
| `JAZORVCA006` | authored `[Inject]` property 未采用 writable auto-property | `#browser-services` | 改为普通 `get; set;` 属性。 |
| `JAZORVCA007` | 已知 Blazor host service 缺少 browser adapter | `#browser-services` | 注册 typed browser adapter，或让操作在 endpoint 完成；页面继续使用标准 `[Inject]`。 |
| `JAZORVCA008` | `[CascadingParameter]` 缺少可激活的 writable auto-property 形状 | `#cascading-parameters` | 改为普通 `get; set;` 属性；标准 `CascadingValue`/命名级联由 browser adapter 自动处理。 |
| `JAZORVCA009` | 保留的旧 route-host descriptor（当前生成 route catalog 的 `@page` 不触发） | `#routing` | 正常页面无需注册 route host；若未来使用未覆盖的 host profile，按诊断给出的宿主配置处理。 |
| `JAZORVCA010` | 标准组件 adapter descriptor（不在当前产品契约中的 Microsoft 内置组件） | `#component-adapters` | 使用自定义组件或 TDesign/Vuetify/Element Plus 的 typed contract；不要依赖历史/实验 adapter。 |
| `JAZORVCA011` | `PersistentComponentState`、`[PersistentState]` 或 `[SupplyParameterFromForm]` 需要版本化 SSR/form handoff | `#ssr-state-handoff` | 使用显式版本化的 typed endpoint/bootstrap payload。 |

Razor SDK/Roslyn 的 `RZ****`、`CS****` 诊断仍由对应工具报告；RazorVue 不复制这些检查。

<a id="browser-services"></a>
## Browser Services

页面和组件采用标准 Blazor 的 `[Inject]` 或 `@inject` 写法请求服务。RazorVue 将可在浏览器执行的 client/service adapter 解析到组件运行时；数据库上下文和服务器进程能力由 endpoint 提供。

| ID | 触发形状 | 最小动作 |
| --- | --- | --- |
| `JAZORVCA001` | `[Inject]` 或 `@inject` 的服务是 `DbContext` 或其派生类型 | 数据访问放在 server endpoint，组件注入对应的强类型 browser client。无需改用 Vue、RenderTreeBuilder 或手写 JavaScript。 |
| `JAZORVCA002` | `[Inject]` 或 `@inject` 的服务是 `HttpContext`/`IHttpContextAccessor`、ASP.NET host environment 或 ASP.NET Identity manager 等 server-only 类型 | 读取/写入动作移到 server endpoint，组件只注入强类型 browser client；request、response 或 server identity manager 不应视为浏览器服务。 |
| `JAZORVCA006` | `[Inject]` 属性未采用可写 auto-property（readonly、`init`、custom setter 或 static） | 改为普通的 `get; set;` 属性；服务由宿主 provider 注册。 |
| `JAZORVCA007` | `IComponentActivator`、circuit/protected-storage 等已知 Blazor host service 缺少 browser adapter | 注册 typed browser adapter，或在 server endpoint 封装服务调用。 |

浏览器可执行服务的 `[Inject]`/`@inject` 属性在 component 初始化和生命周期回调前自动解析。provider key、生命周期和 provider 解析错误由宿主 adapter 负责；页面作者使用标准 Blazor 注入语法。启用 SSR 时，宿主可在 `JazorSsrRequest.Providers` 中携带字符串 key 与 JSON value，runner 和 hydration 注册同一组 application providers；例如 `jazor:service:MyApp.BrowserClient`。当前支持单一显式 constructor 的普通引用类型服务参数，该参数使用同一 provider key；primary constructor、多构造函数、`this(...)`、`base(args)` 和值类型服务通过 `JAZORVGA024` 说明当前 activation 范围。

Blazor JS interop 使用独立的运行时模型。`IJSRuntime` 的 identifier string、`object[]` 参数编组、动态 import 和 runtime dispatcher 缺少 Jazor 静态 import 与模块依赖契约，因此实际类型或成员使用点通过现有 compiler/final Compilation diagnostic 说明。JavaScript 能力通过已有的强类型 `ECMAScript`/WebIDL binding，或静态模块 API 的强类型 binding declaration 进入模块；调用、导入和资源闭包由 `Jazor.Compiler` 与 `Jazor.Emit` 统一处理。

RazorVue 组件按独立入口契约判断：类型可赋值给 `ComponentBase`、实现 `IVueComponent`（包括泛型或派生 marker），并声明 `[ECMAScriptModule("...")]` 或 `[ECMAScript("package", Transform.Component, "Export")]`。其中 `Transform.Component` 描述静态库组件 import；它与 `IVueComponent` 共同构成当前组件类型契约。

这些规则检查作者的 `.razor`、`.razor.cs` 和普通 C# component source；final Compilation 使用 `JAZORVGA020`-`026` 系列诊断。其他服务在具备可静态证明的 browser contract 后进入 authoring 诊断；最终 lowering 由 final diagnostic 报告。

<a id="cascading-parameters"></a>
### Cascading 参数

`CascadingValue<T>` 与 `[CascadingParameter]` 已由 browser adapter 物化。页面作者使用标准 Blazor 写法，覆盖按类型或 `Name` 匹配、嵌套 provider 的最近值覆盖、显式 `null`、provider 缺失时的属性默认值，以及 `IsFixed` 对后续更新的语义。provider 更新依次同步级联属性、运行 `OnParametersSet`/`OnParametersSetAsync` 并请求渲染。Windows SSR Release consumer 还证明 named cascade 在 server HTML 中传给真实子组件，并在 hydration 后保持；完整 reference parity、SSR 更新传播与 hydration side-effect parity 归属后续范围。

`JAZORVCA008` 针对缺少 adapter 激活条件的属性形状（readonly、`init`、custom setter 或 static）。普通 writable auto-property 提供兼容形状；服务交接由 browser adapter 完成。

<a id="routing"></a>
### 路由

`@page`、`@layout`、route parameter 和 `[SupplyParameterFromQuery]` 由 official Razor SG symbols 自动生成稳定 route catalog；`NavigationManager` 和应用自有页面 host framing 消费该 contract。页面使用应用自定义 host 或明确的 Vue Router/组件库 contract。当前 Support 的 route-host 子集覆盖初始匹配、layout composition、query/route prop 映射、pushState、replaceState、popstate、query refresh、not-found、browser history 和 `LocationChanged` 事件；该子集由 official SG、Deno、HTTP-origin browser 以及 isolated Release package consumer 共同验证。复杂 constraint/fragment/history 序列化和 SSR/hydration route identity 归属扩展范围；`LocationChanging` 的取消范围按对应 ledger 条目裁决。

`NavigationManager.RegisterLocationChangingHandler(...)` 的 browser-interactive 支持范围是同一 base URI 的内部 `NavigateTo`：handler 可以读取 `TargetLocation`、`HistoryEntryState` 和 `CancellationToken`，调用 `PreventNavigation()`，通过 `CancellationToken.Register` 观察被后续导航 supersede 的取消，并在返回的 `IDisposable` dispose 后停止拦截。该子集已通过 Blazor reference oracle、official Razor SG、Deno、真实 HTTP-origin browser 和 isolated Release package consumer；`forceLoad`、外部 URI、`popstate`/`hashchange` cancellation、server circuit 与 SSR/prerender route identity 不在声明内。`NavigationOptions` 在 C# 中使用 `Microsoft.AspNetCore.Components.NavigationOptions`，避免与 `ECMAScript.NavigationOptions` 绑定产生歧义；后者对应 Web Platform Navigation API 的 options dictionary，不应重命名。

<a id="component-adapters"></a>
### 标准组件适配器

Microsoft.AspNetCore.Components 提供的 `DynamicComponent`、`CacheView`、`ConfigureBrowser`、`ImportMap`、`ResourcePreloader`、`BasePath`、`ErrorBoundary`、`EditForm`、`AntiforgeryToken`、`FormMappingScope`、`DisplayName<T>`、`InputHidden`、`Label<T>`、`EnvironmentView`、`Input*`、`Virtualize`、`QuickGrid`、`SectionContent` 和 `SectionOutlet` 等内置 UI 组件不属于 RazorVue 组件入口；只有满足 `ComponentBase` + `IVueComponent`（或派生接口）并声明导入描述的自定义/第三方组件才会进入 lowering。工作树中仍可能存在历史/实验 adapter 和对应测试，它们不构成产品支持证据；页面应改用自定义组件或 TDesign/Vuetify/Element Plus 的 typed contract，避免依赖运行时替代。

Element Plus 组件按普通 Razor 组件使用：`ElButtonType` 等 enum 保持受限字符串域，`ElInput.ModelValue` 使用 `VueStringNumberValue`，`@bind-ModelValue` 连接 `onUpdate:modelValue`，`ChildContent`/命名 `RenderFragment` 映射 default 或命名 slot，`class`、`style` 与 `@attributes` 走基类的 typed/splat contract。示例：

```razor
@using ECMAScript.ElementPlus

<ElButton Type="ElButtonType.Primary" OnClick="Save">Save</ElButton>
<ElInput @bind-ModelValue="Name" Placeholder="Name" />
```

该切片通过 C# 参数、事件和 slot 类型表达组件契约；新增 Element Plus 组件依次通过官方 Razor SG、模块运行时、package consumer 和浏览器资源验收。

<a id="component-logic"></a>
## Component C# Logic

`@code` 与 `.razor.cs` 共同组成同一个 partial component。RazorVue 从 `BuildRenderTree`、已识别的生命周期入口和其捕获的 handler 开始收集闭包，再沿当前组件及其源码基类的成员调用展开。模块收集可达成员，降低范围由可达关系确定。

| 形状 | 当前决策 | 作者约束 |
| --- | --- | --- |
| 非参数 instance field、auto-property 及其 initializer | **Support** | 可达成员会物化为 Vue reactive state。用 field/property initializer 表达默认值；它会进入 setup 状态初始化。 |
| `[Parameter]` property | **Support with Vue semantics** | 映射为 Vue props，不应在组件内赋值。props 是 shallow reactive：替换引用会触发参数处理，同一引用内部 mutation 不构成新的参数赋值承诺。 |
| 有 getter body 的非参数 property | **Support** | 作为 computed member 降低；getter 中用到的成员同样必须在 compiler 支持切片内。 |
| 普通 instance/static helper、事件处理器、相互调用的可达方法 | **Support with constraints** | 只有 render、handler、lifecycle 或另一可达成员能到达的方法会发射。业务计算应收敛在这些方法中，不宜放进 VNode-producing block。 |
| 普通方法中的 local、赋值、条件、`for`/`foreach`/`while`/`do while`、非标签 `break`/`continue`、return、switch expression | **Support with compiler constraints** | 这些是 component logic，不受 direct-render 的 straight-line loop 限制。每个表达式、类型和 member 仍需可由 `Jazor.Compiler` 降低；官方运行时回归覆盖了 `@code` handler 中的 `foreach`、`break`、`continue`、helper 调用和 switch expression。 |
| `goto`、labeled branch | **Reject** | 当前 compiler 通过 `JAZORVGA021` 说明 imperative/render fragment 边界；使用显式条件、返回或拆分 helper。普通 loop `break`/`continue` 适用当前 loop 规则。 |
| `async` handler、`Task`/`ValueTask` lifecycle、`EventCallback` | **Support with constraints** | 使用已有 CLR/host mapping。`EventCallback` 的 listener await 语义与 Razor SG 绑定形状一起覆盖；Windows SSR Release consumer 已证明初始 `OnInitializedAsync` 完成后序列化 HTML 并在 hydration 后保留状态。复杂 rejection/cancellation/disposal race 的浏览器语义已覆盖；完整 SSR/prerender identity、SSR rejection/cancellation 与 hydration side-effect parity 归属扩展范围。 |
| `SetParametersAsync(ParameterView)` override | **Compatibility Adapter (Support)** | 保持标准 Blazor 写法。RazorVue 为该入口建立 per-instance ParameterView snapshot，按 source parameter name 应用 sparse overlay，再按 Blazor 顺序调用 `OnInitialized*`/`OnParametersSet*`；异步更新串行排队。Windows SSR Release consumer 也证明 serialized props 会在 server HTML/hydration 入口等待初始参数任务。完整 snapshot/reference parity、取消深度和 SSR 异常明确排除；未支持的 `ParameterView` API 会在作者源码处给出兼容性诊断。 |
| 源码基类声明的 lifecycle/dispose entry point，或间接 lifecycle override | **Support with CLR dispatch constraints** | 当前组件及其源码基类属于同一 member closure；按真实 virtual/interface dispatch 选择最派生实现，再在 Vue setup/unmount 中执行。外部已编译基类仍不进入源码 closure。 |
| static field、static auto-property、可达 static helper/accessor | **Support with module lifetime** | static storage、helper 和可达 nested runtime class 在 artifact module 作用域只初始化一次，不进入每个 setup 的 `reactive` state。它们不构成组件实例隔离状态。 |
| 嵌套 non-record runtime class | **Support with constraints** | 可达的创建和成员会进入闭包。实例可能被 Vue deep Proxy 包装，因此 private storage 会降为 `$jazor$private$...` ordinary property；不要在作者代码中依赖该实现名。 |
| record、interface | **Support with compiler semantics** | record 是 structural lowering，interface 是编译期 contract；两者不等同于可随意保留 CLR runtime identity 的对象模型。 |
| 无参显式组件/源码基类实例构造函数、默认 `base()` 链、constructor body | **Support with constraints** | setup 先建立 CLR default state，再按 base-to-derived 执行 field/property initializer 与 constructor body；constructor 中的普通 C# 仍需 compiler 支持。 |
| primary-constructor 参数、`this(...)`、`base(args)`、值类型/泛型/`ref`/`out`/`in`/`params` 构造参数、多构造函数 | **Reject (`JAZORVGA024`)** | 这些形状需要独立的静态 activation selector；使用 `[Parameter]`/VueInject，或将初始化收敛到无参 constructor/lifecycle。 |
| 单一显式构造函数 + 普通引用类型服务参数 | **Support with constraints** | 参数按现有 `jazor:service:<type>` provider key 通过 Vue `inject()` 解析；先完成 base-to-derived initializer/constructor replay，再进入 property injection 和 lifecycle。服务必须由宿主显式提供，缺失 provider 会在 setup 激活时失败。 |
| 当前组件 indexer | **Reject** | 当前 state/props projection 使用普通方法或显式集合成员。 |

### 推荐的复杂逻辑形状

标记保持为声明 UI，分支、循环和计算放到 `@code` 或 `.razor.cs` 的普通成员中。下面的 handler 属于 component logic，不属于 direct render；其中的 `continue`/`break` 因而合法。

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

同一逻辑写入包围标记的 `@for` 或手写 `BuildRenderTree` 时，适用 [Direct Render](#direct-render) 约束：普通、绑定当前 loop 且位于已关闭 frame 之后的 `break`/`continue` 进入 lowering；复杂控制流通过 component logic helper 表达。

<a id="parameter-lifecycle"></a>
### 生命周期与组件运行时入口

RazorVue 按 Roslyn 的真实 override/interface 关系识别入口，同名普通方法保持普通方法语义。当前组件和源码基类的可达 hook 按 CLR 的 virtual/interface dispatch 解析到实际目标。`SetParametersAsync(ParameterView)` 使用 per-instance compatibility adapter；页面作者使用标准 Blazor 入口。初始 `OnInitializedAsync` 在 SSR server HTML 序列化前等待，hydration 后继续使用同一状态；复杂 lifecycle 的浏览器与首屏 SSR 子集已 Support，完整 SSR/prerender identity、rejection、cancellation 和 hydration side-effect parity 归属扩展范围。

| C# 入口 | Vue 映射与约束 |
| --- | --- |
| `OnInitialized` | 在 setup 中执行。 |
| `OnInitializedAsync` | setup 中调用，并在 Promise settle 后请求一次重新渲染；rejection 会在下一次 render 重新抛出。 |
| `OnParametersSet` | 初次 setup 执行，随后由 shallow props watch 触发。 |
| `OnParametersSetAsync` | 初次及 props 更新时串行执行；当前参数状态按最新一轮异步结果维持，rejection 进入下一次 render。 |
| `OnAfterRender(bool)` | 分别映射 Vue mounted/updated，参数为 `true`/`false`。 |
| `OnAfterRenderAsync(bool)` | 同样在 mounted/updated 调用；Vue 将 hook completion 与 render 调度分离，rejection 在下一次 render 重新抛出。 |
| `ShouldRender` | 作为 cached VNode 的 render gate。 |
| `IDisposable.Dispose` / `IAsyncDisposable.DisposeAsync` | 映射 unmount；Vue 在 unmount 时调用异步 dispose，并在卸载后维持组件失效状态。 |
| `StateHasChanged` | 仅当前组件 receiver 的调用有 runtime 支持；unmount 后调用会失败。 |
| `InvokeAsync(Action)` / `InvokeAsync(Func<Task>)` | 仅当前组件 receiver 的窄调用面有支持；unmount 后返回 rejected Promise。 |

### 标准 API 的环境边界

Razor SDK 提供 C# 与 Razor 编译能力；浏览器运行时通过已注册的 client/service adapter 执行服务。数据库上下文、请求上下文和服务器 host/Identity 服务由 `JAZORVCA001`/`JAZORVCA002` 在作者源码处提供诊断，缺少 adapter 的 Blazor host service 由 `JAZORVCA007` 说明。`AuthenticationStateProvider` 通过宿主注册的 typed browser provider 进入组件。`NavigationManager` 的基础属性注入由 browser service adapter 提供；同源内部 `NavigateTo` 覆盖 `ReplaceHistoryEntry`、`HistoryEntryState` 和 `LocationChanged` 订阅，`LocationChanging` 的内部取消子集及已验证的 `popstate`/`hashchange` history 取消恢复可直接使用。`IJSRuntime` 的实际调用或成员访问在使用点通过 compiler/final Compilation diagnostic 说明，JavaScript 能力使用强类型 ECMAScript/WebIDL binding。标准 cascading 和基础 route catalog 属于 framework primitive。TDesign 等组件库的 `TForm<T>`、`TFormItem`、typed `TInput<T>`、`Rules`、`OnValidate`、`OnReset` 和 `OnSubmit` 提供表单与校验 contract；`EditForm`、`Input*`、`ValidationMessage`、SSR/hydration 等形状通过对应 guidance 或 final Compilation 在作者映射位置说明。

`ParameterView` 的标准 `SetParametersAsync(ParameterView)` 入口由 compatibility adapter 支持；参数通过已声明的 typed properties 使用。枚举、`TryGetValue` 与 `ToDictionary` 由 `JAZORVCA003`、`JAZORVCA004`、`JAZORVCA005` 说明。`[VueInject]` 是组件库级 contract（见 [VueInject](#vue-inject)）。

<a id="ssr-state-handoff"></a>
### SSR 状态与表单交接

`PersistentComponentState`、`[PersistentState]` 和 `[SupplyParameterFromForm]` 依赖服务器 renderer、序列化 payload、请求边界和 hydration 时机。RazorVue 的 SSR host 提供 `jazor-ssr-state` v1 envelope（`schema`、`version`、`props`、`providers`），并在 Deno runner 与浏览器 hydration 入口校验版本、provider key 和 provider key 唯一性；空白 key、重复 key 以及认证保留 key 冲突产生显式诊断。浏览器 hydration 在首次异步导入前占用 mount；并发入口、组件导入和挂载错误保留可观察的失败状态。该协议定义 host-owned state transport。作者源中出现这些 API 时报告 `JAZORVCA011`，位置落在对应类型或 attribute 上。

`[StreamRendering]` 需要 renderer-owned 的流式 SSR 生命周期；RazorVue 通过 `JAZORVCA012` 说明该协议条件。增量数据使用显式 typed endpoint/bootstrap 协议并自行定义加载状态。

本地化保持应用所有权：使用应用自有 typed resource/locale contract 或 ECMAScript `Intl` binding；request-culture middleware、`IStringLocalizer` 注入、satellite-resource discovery 和 SSR locale handoff 由应用宿主协议提供。复杂校验使用 TDesign/Vuetify/Element Plus 的 typed rules/callbacks 或应用自有 validation contract；`EditContext`、`InputBase`、`DataAnnotationsValidator` 和完整服务器校验 parity 归属应用侧模型。

对于需要 SSR 首屏数据的页面，使用显式、强类型 endpoint 返回 bootstrap DTO，并在 `JazorSsrRequest.Props` 或 `Providers` 中交接；服务端是授权和数据事实来源。重复提交、过期 payload、状态失配和表单防伪由 endpoint/host 处理。认证状态使用显式 typed provider 与 `JazorAuthenticationEnvelope` endpoint 结果。单一显式构造函数的普通引用类型服务参数和已验证的 browser history 取消恢复属于当前有界 Support；复杂 activation、服务器 circuit、完整 history parity 和 hydration identity 按 [P1 执行计划](../04-roadmap/p1-plan.md) 使用 Guidance/Reject。

<a id="direct-render"></a>
## Direct Render

Razor SG 生成的 builder 调用按顺序解释为 Vue VNode；手写 `BuildRenderTree` 遵循相同模型。以下约束适用于产出 VNode 的代码，并定义 RenderTreeBuilder 协议：

| RenderTreeBuilder/statement 形状 | 当前决策 | 约束 |
| --- | --- | --- |
| `OpenElement`/`CloseElement`、`OpenComponent`/`CloseComponent`、`OpenRegion`/`CloseRegion` | **Support** | 形成严格 LIFO frame 栈；tag/component type 必须静态可分析。 |
| `AddContent`、`AddMarkupContent`、组件 child content | **Support with constraints** | content expression 走 compiler bridge；fragment/slot 必须有可分析来源。 |
| `AddAttribute`、`AddComponentParameter`、`AddMultipleAttributes`、event metadata | **Support with constraints** | 名称必须为 compile-time string，且必须位于该 frame 的第一个 child 前。 |
| `SetKey`、`SetUpdatesAttributeName`、element/component reference capture | **Support with constraints** | 作用于当前 frame，并位于 child 之前。 |
| invocation/expression statement、block、已初始化 local、`if`、已建模循环、direct `return` | **Support with constraints** | 组成 RenderEmitter 可识别的 render segment。 |
| 动态 tag/attribute/parameter 名称、运行时 component `Type`、未知外部 `RenderFragment` factory | **Reject (`JAZORVGA021`)** | 改用静态分支、显式组件类型或 inline/local/helper/slot fragment。 |
| 未初始化 local、frame 外 metadata、`goto`、labeled branch | **Reject (`JAZORVGA021`)** | 使用初始化、正确 frame 顺序或 [Component C# Logic](#component-logic) 中的普通 helper。普通 loop `break`/`continue` 见下方循环规则。 |

- `OpenElement`、`OpenComponent`、`OpenRegion` 与对应 close 成对，并按栈顺序关闭；
- `OpenComponent<T>` 支持开放泛型组件类型（例如 `OpenComponent<TTable<T>>()`）；类型参数作为编译期注解擦除，组件声明 `[ECMAScriptModule]` 或 `[ECMAScript(..., Transform.Component, ...)]` 描述。official Razor SG 生成的 `TypeInference.Create*_0<T>` 辅助在当前 fragment builder 作用域内内联，构造方法参数与方法体原始定义对齐，最终模块保持 builder 作用域封闭。运行时动态 `Type` 和静态信息不足的组件类型报告 `JAZORVGA021`。
- element/component 的属性、component parameter、splat 和 event metadata 位于第一个 child 之前；
- tag、attribute、parameter、event modifier 和 bulk-attribute 名称使用 compile-time string；
- `SetKey`、`SetUpdatesAttributeName`、reference capture 和 render-mode metadata 作用于正确的当前 frame；
- `OpenComponent` 使用 generic component type 或 `typeof(T)`；
- `RenderFragment`/slot 解析为 inline、local、helper 或 component slot source；外部 factory、递归 render helper 和未闭合 fragment 通过诊断说明范围；
- RenderFragment 属性/方法由已发射的普通 component member 引用时，随 member closure 一起保留。片段 body 缺少支持切片时，编译器报告 `JAZORVGA024`。
- sequence 参数使用无副作用表达式，并作为编译期顺序标记。

### 循环与分支

已支持的循环生成 Vue fragment：无控制流分支的 `@foreach` 使用 Vue `renderList` 快路径，普通 `@for`、`@while`、`@do while` 使用受控 fragment lowering。循环体包含普通 `break`/`continue` 时，进入同一 render pass 内的 imperative JS loop，并按源顺序写入已完成的 VNode segment 到结果数组；`foreach` collection 由 compiler 提供 string/iterable 语义。输出循环体形成可识别的 RenderTreeBuilder content segment；业务计算位于 component logic。需要 compiler 临时变量的 initializer/condition/update 或混杂未建模 statement 时报告 `JAZORVGA021`。

direct render 中普通 `break`/`continue` 绑定当前 loop 的结构化目标，且 branch 前已关闭 element/component/region frame；branch 在真实 JS loop 作用域中执行。跨 loop、`goto`、labeled branch 和 branch 时仍打开 frame 的形状报告 `JAZORVGA021`。复杂控制流通过 component logic helper 计算，再由 render 消费结果。

### 常见替代

| 失败写法 | 推荐写法 |
| --- | --- |
| `builder.OpenElement(0, tag)` | 使用静态标签，或为不同标签写显式 `@if` 分支。 |
| child 之后再 `AddAttribute`/`SetKey` | 在 open frame 后、任何 child 前设置 metadata。 |
| `AddContent(0, SomeFactory())` 返回未知 `RenderFragment` | 使用 inline fragment、已声明的 slot 或可分析的 helper。 |
| 在 frame 中声明未初始化 local | 先初始化，或让纯 C# 计算在 frame 之前完成。 |
| 动态 `OpenComponent(0, type)` | 使用静态组件类型或显式组件分支。 |

<a id="compiler-boundary"></a>
## Compiler Boundary

RazorVue 在 direct-render 层使用 `Jazor.Compiler`/`SemanticWalker` 的 C# 成员、调用、转换和 whitelist lowering。标记表达式和 component logic 都由该入口降低；operation 缺少 JavaScript expression/statement、external type/member mapping 或 host mapping 时，报告 `JAZORVGA022`。

`SemanticWalker` 覆盖具有明确 JavaScript/CLR host 语义的 compiler slice。值类型和被调用成员通过 whitelist/host mapping 确认运行时语义；BCL、反射、线程、文件/网络和 ASP.NET Core server API 通过各自宿主契约进入应用。诊断位置来自 Roslyn operation/symbol 的原始 `Location`，再通过 mapped span 投影回 `.razor` 或作者 `.razor.cs`。

泛型参数、数组元素和集合元素的类型在运行时敏感 lowering 之前保持 erased；成员访问、构造和运行时类型检查在使用点验证 host mapping。参数、返回值或 collection 保持可表达的强类型 C# surface，必要时使用已有 union/host value contract。

<a id="component-binding"></a>
## Component Binding

组件从 final Compilation 解析可绑定的 `BuildRenderTree(RenderTreeBuilder)` block，并满足 RazorVue 组件身份契约：可赋值给 `ComponentBase`，实现 `IVueComponent` 或其派生接口，且声明 `[ECMAScriptModule("...")]` 或 `[ECMAScript("package", Transform.Component, "Export")]` 导入描述。官方 Razor SG 负责 component parameter、required parameter 和参数类型诊断；RazorVue 报告缺少绑定或消费条件的最终形状。

组件模块使用稳定的 `[ECMAScriptModule("...")]` 或 `[ECMAScript("package", Transform.Component, "Export")]`。组件引用、parameter 名称和 child content 与编译期 symbol 对齐；组件入口通过 `IVueComponent` marker 和导入描述确定。

<a id="member-closure"></a>
## Member Closure 与 Reactive Class

member closure 物化 render、已支持 lifecycle、constructor replay 和捕获 handler 可达的字段、属性、方法、nested runtime class 及其依赖；当前组件的源码基类也视为同一作者成员面。成员导出名、类型或引用关系缺少确定性时报告 `JAZORVGA024`。

组件在 Vue setup 中采用结构化 state replay：先创建每个 state slot 的 CLR default，再按基类到派生类执行 source initializer 和 constructor body。单一显式 constructor 的普通引用类型服务参数从既有 Vue provider key 解析后传入；其他 activation 形状在 member closure 阶段通过 `JAZORVGA024` 说明。该 replay 保持普通 nested runtime class 的 class lowering；nested class 按自身 constructor protocol 执行。

### Proxy-safe class storage

当 runtime member class 进入 Vue `reactive()` 或其他 deep Proxy，JavaScript private field 的 brand check 会针对 Proxy receiver 失败。在 Vue member-closure profile 中，RazorVue 降级非 public field、auto-property backing field、primary-constructor capture 和 field-like event storage，得到稳定的普通 mangled property，例如 `$jazor$private$...`。它仍保持 class identity、继承和访问顺序；该名称是实现细节，不应在作者代码中引用。

这项 Support 由 official Razor SG + deep Proxy Deno regression 覆盖。组件参数遵守 Vue 的 shallow-prop 语义：触发父子更新时替换整个引用，或使用明确的 Vue ref/reactive contract。

<a id="vue-inject"></a>
## VueInject

`[VueInject]` 是 compilation 级声明协议。注入角色引用命名 component type，container contract、implementation 和导出名满足当前 registry 规则，重复或冲突声明报告 `JAZORVGA025`。registry 诊断维持完整 `ModuleCatalog` 条目。

<a id="vue-module"></a>
## Vue Module 与 Union

`JAZORVGA026` 覆盖模块 framing、import alias、runtime helper 和最终 Emit 物化失败。模块路径应稳定、可解析且与对应 JS resource manifest 的实际 package entry 一致；不要手工拼接 import 文本来绕过 `SemanticWalker` 的 import collection。

Vue host value domain 优先使用 C# native `union`，例如：

```csharp
[Parameter]
public Vue.VueBooleanStringValue Mode { get; set; } = true;
```

union 是 authoring/compile-time contract，运行时按其分支值擦除；保留 `AsX` projection 和正常赋值/隐式构造。官方 Razor SG 绑定使用可编译参数面；需要更窄作者体验时，使用显式 overload 或强类型参数。

## 排查顺序

1. 先修复同一 compilation 中的 `CS****`/`RZ****`，它们可能使 generated C# 不完整。
2. 查看 `JAZORVGA` ID、mapped path/line/column 和 HelpLink；不要只复制异常末尾文本。
3. 按 ID 对照本指南章节，保留一个最小 `.razor` 或 `.razor.cs` 复现。
4. 确认诊断后产物保持完整 `ModuleCatalog` 条目与 `.mjs` 输出。
5. 提议 Support 形状时，同时提交生成的 `BuildRenderTree` 形状、预期 Vue render-function、状态/SSR 语义和最小回归。

## 升级门禁

升级 .NET、Roslyn 或 Razor SDK preview 时，至少运行：

- `SemanticWalkerOrdinaryTest` 的 ordinary/labeled `IBranchOperation` 与 `BranchKind` gate；
- official Razor `for`、`while`、`do while` runtime tests；
- `RazorSgOfficialNativeUnionParameterAuthoringTests`，验证 native union 参数可由 Razor SG 绑定并进入最终模块；
- `BootstrapPatchTests`（文件 `RazorSourceGeneratorBootstrapPatchTests.cs`）的 mapped diagnostic 和无 partial `ModuleCatalog`/module gate；
- `RazorSgOfficialNestedRuntimeClassClosureRuntimeTests` 的 deep Proxy regression；
- `MemberClosureBuilderContractTests` 的显式 component/source-base constructor、runtime entry mapping diagnostic gate；
- `RazorSgOfficialRuntimeAuthoringTests.BuildComponent_OfficialRazorCodeBlock_ExecutesComplexComponentLogicOnDenoHost`，验证 `@code` 业务循环与 direct-render 循环边界；
- `dotnet test src/Jazor.RazorVue.Sg.Test/Jazor.RazorVue.Sg.Test.csproj`、`dotnet test src/Jazor.CompilerTest/Jazor.CompilerTest.csproj`，以及发布/SSR consumer gate。

这些门禁检查的是 operation contract、作者位置和运行时语义；旧 snapshot 通过本身不足以证明 preview SDK 升级安全。
