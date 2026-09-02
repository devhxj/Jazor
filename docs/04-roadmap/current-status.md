# 当前状态

> 定位：当前产品范围与验证入口，不是某次构建或审计的历史快照。

## 核心平台

Jazor 的当前核心是受控 C# -> ECMAScript 转换：Roslyn `IOperation` 进入 `Jazor.Compiler`，生成 ESTree 和确定性 ECMAScript 模块，随后由 `Jazor.Emit` 物化或打包。宿主 API 通过 CLR/ECMAScript 映射和白名单定义，未支持的运行时语义在使用点明确失败。

类库资源与 Emit 物化已经完成一次性收敛，且只有两种 carrier：JS resource library 使用
`manifest.json + dist/**`，纯 Jazor library 使用程序集内的
`Jazor.Generated.ModuleCatalog/ECMAScriptCode`。分析/生成阶段只把纯 Jazor 模块写入
`ModuleCatalog`；最终 `Exe`/`WinExe` 宿主的 MSBuild target 在 `Build` 后调用 Emit，直接将选中
闭包物化到 `JazorDir`。中间类库不 Emit、不重编译上游 catalog；工具资格不传递，资源 manifest
和模块依赖会传递。NuGet `build/` 仅用于直接 tooling activation，`buildTransitive/` 只携带
manifest locator，analyzer 依赖不使用自动 `analyzers/dotnet/cs` 资产。源码和 NuGet 的 A -> B ->
Console 回归均已覆盖这一边界。

当前核心持续维护的能力包括模块发射、导入收集、source origin、与所属 module 关联的 source map、CLR 映射、静态分析与 Emit 交付。CLR 集合映射目前包含 `Queue<T>` / `Stack<T>` 的核心切片：构造、`Count`、入队/入栈、出队/出栈、查看、Try 操作、`Contains`、`Clear` 与 `ToArray`；容量管理及未列入白名单的长尾成员仍保持明确的不支持边界。详细边界见 [编译器](../02-architecture/compiler.md) 和 [产物管线](../02-architecture/artifact-pipeline.md)。

## 框架集成

当前已实现的框架集成是 Razor-to-Vue。它以官方 Razor Source Generator 完成后的最终 `Compilation` 为输入，通过 `Jazor.RazorVue` 完成组件绑定和 Vue framing，并复用核心编译器降低 C# 语义。

Blazor framework mapping 的 S2 核心事件切片（Mouse/Keyboard/Focus/Change）已达到 browser-interactive `Support`：前三类 getter 使用原生 DOM carrier，`ChangeEventArgs.Value` 使用 `Jazor.CLR` 的一次性 listener capture 与 `WeakMap` 投影；`RazorSgOfficialCoreDomEventRuntimeTests` 以 Blazor `EventHandlers`/`ChangeEventArgsReader` 作为 reference metadata/value-shaping oracle，official Razor SG、Deno、真实 browser smoke、bundle source map 和事件特定 isolated Release package consumer 均已通过。该切片覆盖 method group/lambda typed callbacks、native mouse/keyboard/focus handlers、direct `@bind` 与 typed `onchange` 共存、checkbox/multiple-select shaping，以及 DOM value 改变后的 async continuation snapshot；constructor/setter/identity、synthetic payload 和 file input 继续拒绝。S3 的 `@ref`/`ElementReference.FocusAsync` browser-interactive 子集已达到 Support：`ElementReference` 由 `Jazor.CLR` alias 为 `HTMLElement`，两个 overload 通过 generated CLR Import helper 保留 completed `ValueTask`/Promise、`preventScroll` payload，以及 default/unmounted ref 的 framework failure；official SG/Deno、真实 Release browser 与 isolated NuGet consumer 均已通过。该声明不包括 SSR/prerender、server renderer identity 或任意 DOM methods。S4 的 Pointer/Wheel/Drag/Clipboard/Touch/Error/Progress 七组事件已达到 browser-interactive `Support`：`Jazor.CLR` generator modules/docs、compiler、official Razor SG/Deno、Blazor `EventHandlers` reference metadata、真实 BrowserSmoke 和 isolated Release package consumer 均已通过；`long`/`unsigned long` 来源字段使用 JavaScript `Number`，`long long`/`unsigned long long` 来源字段按当前项目 carrier 决策使用 `BigInt`，TouchList 只在 CLR 属性访问时惰性 `Array.from(...)`。声明仅覆盖 getter-only native event projection；未批准 setter/constructor、synthetic payload、files/items 和 TouchList 非 getter 操作继续拒绝，SSR/prerender 不声明。统一 Release 包边界仍由 `SdkIntegrationTests.Build_LocalReleasePackages_CoreAndVueConsumers_RespectBlazorClrPackageBoundary` 验证：核心 `Jazor` 消费者不携带 `ECMAScript.Blazor`/ASP.NET Core framework，`Jazor + Jazor.Vue` 消费者能加载 Razor/Vue 作者面。Blazor 内置 UI 组件仍明确不在产品入口内。

`RazorSgBlazorReferenceOracleTests` 是当前 framework primitive 行为对照的权威 reference fixture，覆盖 `ParameterView` sparse/default/unknown-name、property injection、named cascading value 和 `NavigationManager` options/location-changing/cancellation；它只验证 ASP.NET Core reference assemblies 的观察结果，不把 Blazor runtime 引入 RazorVue。`NavigationManager.RegisterLocationChangingHandler` 的同源内部 `NavigateTo` 子集已达到 browser-interactive `Support`：reference oracle、official SG、module/Deno、真实 HTTP-origin browser 和 isolated Release package consumer 共同覆盖 `PreventNavigation`、query/hash/history state、异步 supersede/cancellation 与 registration dispose；`popstate`/`hashchange` cancellation、server circuit 和 SSR/prerender route identity 仍不声明。复杂 lifecycle 仍单独保持 `InProof`：`RazorSgOfficialComplexLifecycleRuntimeTests` 通过 official Razor SG、最终 module artifact 和 Deno 覆盖 async initialization rejection、unmount 后 cancellation、queued parameter lifecycle suppression、stale rejection propagation、重复 render 的 `OnAfterRenderAsync` 调用次数，以及 async disposal race；`SdkIntegrationTests.Build_LocalReleasePackages_WithExternalComplexLifecycleRazorConsumer_ProvesAsyncRacesInRealBrowser` 已补齐 isolated Release package 与真实 browser consumer 证据，SSR/prerender 和 hydration 副作用证据仍未宣称。

RazorVue 的生产输出是 direct Vue render-function `.mjs`。当前已接受的 static hoist、精确 static VNode cardinality、按需 raw-markup runtime、child-level block tree（dynamic string text、mixed text 与 nested block）、保守 patch flags、setup-instance handler cache、已证明安全的 `foreach` `renderList` / keyed-unkeyed fragment、stable/dynamic slot 精确 lowering、direct string/boolean DOM bind，以及 shallow parameter lifecycle watch 边界见 [RazorVue Direct Render 性能评审](./razorvue-direct-h-performance.md)。production Vue gate 已确认 keyed reorder identity、slot 分支更新、direct bind patch，以及 scalar/reference parameter replacement；同一 prop 引用内部的 nested mutation 不定义为新参数赋值。RazorVue 模块生成在保持 stable discovery/order 的前提下有界并行；资源类库按 `manifest.json + dist` 的 selected entry/dependency closure 物化，纯 Jazor 模块按 `ModuleCatalog` 记录物化，browser 不复制无关 SSR/devtools entry，SSR 则显式保留 `vue` / `@vue/server-renderer` 与 runner。完整边界、实测口径和未启用的 cache/preload 策略见 [RazorVue 极致性能路线图](./razorvue-extreme-performance.md)。

作者 C# 面已覆盖两层：Razor 标记和手写 `BuildRenderTree` 共用 direct-render 协议；`@code`/`.razor.cs` 的可达 helper、handler、lifecycle、source-base dispatch、parameterless constructor replay 和 static module members 走 component-logic/module framing。含普通 `break`/`continue` 的 `for`、`foreach`、`while`、`do while` 会使用 imperative loop lowering 保留控制流；`goto`、无法投影的 labeled branch、跨 open frame branch 和参数化 component activation 仍是明确边界。`SetParametersAsync(ParameterView)`、browser `[Inject]` property activation 与 named/nested cascading values 已补齐 isolated Release package + real-browser framework-primitives consumer proof（含参数替换、嵌套/重建实例、`IsFixed`、same-value、dispose 与生命周期顺序），但三项仍保持 `InProof`，因为完整异常/reference/SSR 语义尚未宣称。完整矩阵见 [RazorVue 作者指南](../03-guides/razorvue-authoring.md)。

认证与 SSR 状态交接目前采用明确 `Guidance` 边界：缺少显式 typed browser provider 的 `AuthenticationStateProvider` 注入在作者源报告 `JAZORVCA007`，`AuthorizeView`/`AuthorizeRouteView`/`CascadingAuthenticationState` 仍是固定 Reject；`PersistentComponentState`、`[PersistentState]` 与 `[SupplyParameterFromForm]` 在作者源报告 `JAZORVCA011`，要求使用 typed endpoint/bootstrap payload。当前没有版本化 claims/state envelope、hydration checksum 或 enhanced form protocol，因此这些形状不会被误标为 Support，也不会静默生成浏览器运行时 fallback。

应用自有 RazorVue route host 的 `@page`、`@layout`、typed route/query 参数、not-found、query refresh 和 browser history 已达到 `Support`（Compatibility Adapter）：official Razor SG、module/source map、Deno 与 `samples/RazorVue.Authoring` 的 isolated Release package browser journey 均已覆盖。该 route-host 声明不引入 Microsoft `Router`/`RouteView`/`LayoutView`/`NavLink` 标签，也不声明 SSR/prerender route identity；`LocationChanging` 的内部 `NavigateTo` 子集由独立的 `P1-blazor-clr-navigation-location-changing` ledger row 负责，popstate/hashchange cancellation 仍不在范围内。

组件身份约束已统一为：必须直接或间接继承 `ComponentBase`，实现 `IVueComponent` 或其派生接口，并声明 `[ECMAScriptModule]` 或 `[ECMAScript(import, Transform.Component, exportName)]` 导入描述。Microsoft Blazor 内置 UI 组件（如 `Router`、`DynamicComponent`、`ErrorBoundary`、`EditForm`、`Input*`）不满足该产品入口，识别到后稳定报告 Reject；UI 组件由 TDesign、Vuetify、Element Plus 或应用自定义组件承担。

RazorVue 作者面现已使用 final Compilation typed diagnostics：`JAZORVGA021`-`026` 分别覆盖 direct-render、compiler bridge、component binding、member closure、VueInject 和 Vue module；`JAZORVGA020` 收缩为 bootstrap/未分类内部失败。mapped `.razor` location、稳定多组件聚合和错误时无 partial `ModuleCatalog`/module 由官方 Razor SG 回归覆盖。member class 进入 Vue Proxy 时使用 proxy-safe mangled storage；支持切片、拒绝边界和升级门禁见 [RazorVue 作者指南](../03-guides/razorvue-authoring.md) 与 [作者面诊断路线图](./razorvue-authoring-diagnostics.md)。

Vue 3、Vue Router、Pinia、Vue Devtools、Vue Data UI、Vu Icons、UI 库绑定、`ECMAScript.Style`、`Jazor.Admin` 和 ASP.NET Core SSR 都围绕核心平台或当前 RazorVue 集成提供能力；Vue authoring、Vue bindings 和 Vue runtime 由 `Jazor.Vue` 交付，不改变 Jazor 的框架无关核心定位。Blazor framework CLR mapping 由 `Jazor.CLR.Generator` 生成并由 `Jazor.CLR` 唯一持有，生成的 runtime JavaScript 进入 `ECMAScript/manifest.json + dist/**`；`ECMAScript.Blazor` 仅作为随 `Jazor.Vue` 交付的可选标准 ECMAScript 模拟/投影扩展，不贡献 whitelist 或 runtime resource。`ECMAScript.VueDataUi` 完整映射 `vue-data-ui` 3.23.4 的 71 个公开 `vue-ui-*` entry，保留 typed dataset/config authoring；最终宿主按资源 manifest 的 selected entry/dependency closure 物化 runtime。`ECMAScript.VuIcons` 映射 `vu-icons` 1.5.4 的 1,821 个静态 wrapper；静态使用按单图标 entry 物化，`VuIcon` 动态名称使用则由显式 package-entry closure 物化全量 catalog。

`2026-08-31` 的 JazorAdmin Release 本地 NuGet consumer gate 已通过：native 与 VueInject 两个 consumer 均完成 0 warning/0 error 构建，最终 browser mount smoke 通过；同时 `vue-data-ui` selected entry 的相对 ESM closure 已完整物化，Emit 资源回归 185/185 通过。这确认了规范化后的包与资源交付链路，但不替代逐项 Blazor framework capability 的语义证据，复杂页面的 bridge 摩擦和未完成 ledger 项仍保留。

面向“目标范围内零摩擦”的缺口、阶段、owner 和验收门禁见 [RazorVue “零摩擦”执行计划](./razorvue-zero-friction-plan.md)。Microsoft/Blazor 内置 UI 组件在该计划中明确保持范围外，不计入待实现能力。

`Jazor.React`、`Jazor.RazorReact` 等未来方向尚未构成已接受的产品范围或公开 API。任何新框架集成必须遵守 [框架集成层](../02-architecture/framework-integrations.md) 的边界。

`SetParametersAsync(ParameterView)` 还由 `scripts/csharp/verify-windows-ssr-release.cs` 的 isolated Release NuGet consumer 证明：serialized props 在 server HTML 生成前进入官方 SG 组件的 ParameterView，初始 async 参数任务完成后 hydration 恢复同一参数状态与交互；完整 snapshot/reference parity、取消深度和 authored SSR exception 仍保持 InProof。

## 交付与 SSR

`JazorMode=debug` 直接物化模块、source map、输出 manifest 与 import map；`JazorMode=release` 通过 Netpack 生成浏览器 bundle。资源类库来自各自 `manifest.json + dist/**`，纯 Jazor 模块来自引用程序集的 `ModuleCatalog`；同一依赖闭包决定选中资源，输出 manifest/import map 只是本次物化结果，不是类库发现入口。该选择减少发布目录和部署复制量，不将未请求的文件错误计入首屏网络收益。启用 `JazorSSR=true` 的 ASP.NET Core 应用可使用本地 Vue SSR 与 hydration，DenoHost 负责服务器模块执行，Netpack 只负责浏览器构建。SSR worker 可以在内部使用内容标识管理生命周期，但这不构成类库 carrier、输出目录 generation 或 URL pointer；SSR runner 由明确资源入口物化而非请求时写入。取消、crash、并发上限与应用关闭均有真实进程回归。SSR 发布链路由独立 NuGet 消费者门禁覆盖 runner、PathBase 组合后的 URL 与 Edge hydration。

配置方法见 [安装与配置](../03-guides/installation-and-configuration.md)。

## 质量门槛与验证

| 范围 | 门槛 | 入口 |
| --- | --- | --- |
| 核心编译器 | 至少 10,000 个通过场景、98% 行覆盖率、97% 分支覆盖率 | `dotnet run --file scripts/csharp/verify-compiler-coverage.cs` |
| Razor-to-Vue | 至少 4,000 个通过场景、90% 行覆盖率、94% 分支覆盖率 | `dotnet run --file scripts/csharp/verify-razorvue-coverage.cs` |
| Vue 绑定 | 每个目标至少 90% 已审计公共绑定契约 | `dotnet run --file scripts/csharp/verify-vue-binding-coverage.cs` |
| 全仓库主线 | 当前 compiler、CLR、Style、Devtools、Vue Data UI、Vu Icons、Pinia、Pinia.Testing、VueRoute、Razor SG、Emit 测试 lane | `dotnet run --file scripts/csharp/test-dotnet.cs` |
| Windows SPA 发布消费者 | 本地 NuGet 包、Release bundle、`/docs` PathBase 与 Edge 真实浏览器交互 | `dotnet run --file scripts/csharp/verify-windows-spa-release.cs -- --path-base /docs` |
| Windows SSR 发布消费者 | 本地 NuGet 包、`JazorSSR=true` Release publish、packaged DenoHost SSR HTML、部署资源解析与 Edge hydration 交互 | `dotnet run --file scripts/csharp/verify-windows-ssr-release.cs -- --path-base /todo` |

门槛描述的是可复现的验收规则。需要引用某一时点的实际结果时，应运行对应命令或查看 [CHANGELOG.md](../../CHANGELOG.md) 的发布记录，而不是依赖已删除的历史报告。
