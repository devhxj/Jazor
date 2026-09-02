# Changelog

本文件按日期记录发布与面向用户的变更。它保留版本演进历史，不替代当前产品契约、测试结果或架构文档。

## 2026-09-01

### Unreleased

#### 新增与改进

- TDesign typed component authoring 已完成自然 Razor 的完整交付证明：泛型/非泛型组件、typed
  slots、`@bind`、union、required 参数和 attribute splat 可直接使用，不需要应用侧 bridge、cast
  或手写 `BuildRenderTree`。
- 独立 Release NuGet consumer 会物化 TDesign/Vue 资源并在真实 Edge 浏览器中完成表单输入和按钮
  交互；`ECMAScript.TDesign` 的作者指南与零摩擦路线图已同步这一 `Support` 边界。
- `NavigationManager.RegisterLocationChangingHandler(...)` 的同源内部 `NavigateTo` 子集已完成
  reference、official Razor SG、Deno、真实 HTTP-origin browser 和 isolated Release package consumer
  证明；`PreventNavigation`、异步 supersede/cancellation、query/hash、history state 与 registration
  dispose 可直接使用。`popstate`/`hashchange` cancellation、SSR/prerender route identity 和
  Microsoft Router/RouteView 等内置 UI 仍不在声明内。

## 2026-08-31

### Jazor 0.26.3

> 修复已发布资源包的依赖闭包与 NuGet 打包元数据处理。本版本按 `PATCH` 通道发布，所有
> Jazor/ECMAScript 包继续使用同一版本。

#### 修复

- `ECMAScript.ElementPlus` 现在声明 `ECMAScript.VueRoute` 依赖，公开的
  `RouteLocationRaw` 参数在独立 NuGet 消费方中可以正常还原，Vue Router 的资源 locator 也会
  沿依赖闭包传递。
- NuGet 描述中的分号不再被 `NuspecProperties` 误解析为属性分隔符，包含该字符的包简介可以
  稳定打包。
- NuGet dry-run 入口覆盖全部已发布包，且发布 artifact 的版本只从核心 `Jazor` 包解析，避免
  发布前漏检包或误把 `Jazor.Admin` / `Jazor.Vue` 名称当作版本。

### Jazor 0.26.0

> 类库 JavaScript 资源一次性固定为两种 carrier，并统一引用传递与最终宿主物化。本版本在
> `0.x` 阶段按 `MINOR` 通道发布，不提供旧 provider/catalog 的兼容读取或中间过渡格式。

#### 破坏性变更

- **类库 carrier 固定为两种**：已有 JavaScript 资源的类库使用包内
  `manifest.json + dist/**`；由 Jazor 编译 C# 的纯 Jazor 类库只生成程序集内的
  `Jazor.Generated.ModuleCatalog`。`ECMAScriptCode` 只是后者的语义称呼，不是第三种类型。
- **旧读取入口全部退休**：`RuntimeProviderCatalog`、`ArtifactCatalog`、独立 source-map catalog
  和目录 fallback 不再由 Emit 读取。`src/ECMAScript` 改为标准 JS resource library，CLR
  `System/**` 模块由其 manifest/dist 交付。
- **工具资格不传递**：编写 Jazor 模块或 RazorVue 组件的项目必须直接引用相应工具包；只消费
  上游类库的中间项目不会获得 analyzer、generator 或 Emit。只有最终 `Exe`/`WinExe` 宿主在
  Build 后调用一次 Emit。
- **NuGet tooling 资产隔离**：`build/` 只在直接引用时注册 Jazor/RazorVue analyzer 和 Emit；
  `buildTransitive/` 仅传播 JS resource manifest locator。analyzer 依赖移至
  `tools/net11.0/analyzers/`，不再以自动 `analyzers/dotnet/cs` 资产间接激活。

#### 新增与改进

- **统一依赖图**：ModuleCatalog 模块和 resource manifest entry 使用显式 module/package
  依赖、稳定 identity、路径、hash、source map 与所属资源校验；缺失依赖、错误 hash 和路径冲突
  在写出前确定性失败。
- **直接原子物化**：Emit 从最终宿主的程序集闭包读取 ModuleCatalog，并从传递的 manifest
  locator 读取 JS resource，解析选中闭包后直接原子替换 `JazorDir`。Debug、Release、SSR 和
  HMR 只是同一闭包的输出投影，不构成新的类库 carrier。
- **引用链一致性**：ProjectReference 与 NuGet 的 A -> B -> Console 链使用相同的资源发现、
  去重和物化规则；中间类库不重编译上游 catalog，也不产生输出目录。

#### 迁移

- 从 `0.25.0` 或更早版本升级时，必须 lockstep 升级全部 Jazor/ECMAScript 包并重新构建类库与
  最终宿主。依赖旧 provider、artifact 或并列 source-map catalog 的自定义集成必须改为上述
  两种 carrier；本版本没有兼容 reader 或格式转换器。

## 2026-08-28

### Jazor 0.25.0

> 类库 artifact/resource 依赖与 CLR runtime provider 统一为一张可验证的 artifact graph。本版本在 `0.x` 阶段按 `MINOR` 通道发布，并明确切换到新的 provider 载体，不提供旧 catalog 的兼容读取。

#### 破坏性变更

- **CLR provider 载体切换**：`Jazor.Emit` 现在只读取标准 `Jazor.Artifacts.RuntimeProviderCatalog`。旧 `ECMAScript.Catalog` 不再读取或 fallback；从 `0.24.0` 或更早版本升级时，必须 lockstep 升级所有 Jazor 包并重新构建类库 provider/catalog。
- **工具引用边界收紧**：定义 ECMAScript 模块或 RazorVue 组件的类库必须直接引用对应工具包；只消费上游类库的中间项目不因传递 catalog 增加 `Jazor`/`Jazor.Vue`。封装包对工具依赖使用 `exclude="Build,Analyzers"`，最终宿主仍需直接引用并配置 Emit。

#### 新增与改进

- **统一 provider 读取**：inline content 与 embedded resource 经过同一 schema、路径、哈希、source map、asset、import-map 和依赖校验，归一化为相同的模块记录。
- **按真实入口裁剪闭包**：CLR/runtime provider 只有在应用生成模块实际导入时才激活，并沿声明的依赖闭包传播；未使用的 provider 模块不会污染最终输出。
- **统一类库资源传播**：生成模块、CLR runtime、组件 ESM/CSS、许可证和 source map 通过声明的 artifact graph 到达最终宿主；冲突路径、缺失依赖和不一致元数据会在 Emit 阶段确定性失败。

### Jazor 0.24.0

> Blazor CLR 模块生成与归属边界收敛，并交付扩展 DOM 事件参数的最小垂直切片。本版本按 `MINOR` 通道发布；尚未完成真实浏览器、reference oracle 与独立 package consumer 证据的能力继续标记为 `InProof`。

#### 新增

- **统一 Blazor CLR 模块管线**：受支持的 Blazor framework 类型先由 `Jazor.CLR.Generator` 从真实 reference symbol 生成初始 module/doc，再由 `Jazor.CLR` 持有 mapping、carrier 与 runtime helper；`Jazor.Compiler` 的静态 whitelist 与既有 `ECMAScript.Catalog` 继续由各自 owner 生成/物化。
- **扩展 DOM 事件参数切片**：Pointer、Wheel、Drag/DataTransfer、Clipboard、Touch、Error 和 Progress 的原生事件只读 getter 已接入 `Jazor.CLR`，TouchList 在属性访问时惰性转换为数组 carrier；未批准的构造器、setter、合成 payload、文件/items 和 TouchList 非 getter 操作继续明确拒绝。
- **发布包边界验证**：核心 `Jazor` consumer 不携带 `ECMAScript.Blazor` 或 ASP.NET Core framework；`Jazor + Jazor.Vue` consumer 保留 Razor/Vue authoring payload，并由独立 Release package consumer 回归验证。

#### 边界与迁移

- `ECMAScript.Blazor` 现在只提供标准 ECMAScript 模拟/投影扩展，不再贡献 `[Jazor]` mapping、CLR whitelist 或 runtime module；Blazor CLR mapping 由 `Jazor.CLR` 唯一持有。需要 Blazor 类型支持的应用无需复制 mapping 源码或注册 provider。
- RazorVue 仍保留 `ComponentBase`、`EventCallback`、`RenderTreeBuilder` 和 current-component 等产品 hook，但这些 hook 的获准 CLR surface 也必须经过模块生成器并在 `Jazor.CLR` 声明；组件入口仍要求 `ComponentBase + IVueComponent + ECMAScript` 导入描述。
- S5 认证状态/provider 不实现且不进入本版本；`IJSRuntime` 家族继续是 Reject。浏览器 API 应使用强类型 ECMAScript/WebIDL binding，内置 Blazor UI、表单、文件和认证 UI 不因本次 CLR 切片而获得支持。

## 2026-08-26

### Jazor 0.23.0

> ECMAScript 外部绑定元数据收敛为统一协议。本版本删除旧组件 Attribute 公共 API，按 `MINOR` 通道发布。

#### 新增

- **统一 ECMAScript binding 协议**：`ECMAScriptAttribute` 现在通过 `Transform.Allow`、`Transform.Import` 和 `Transform.Component` 区分环境宿主、普通外部 ESM binding 与组件 binding，并支持组件 default/named export。
- **保留外部 ESM specifier**：bare、相对、root-relative 与 URL specifier 原样进入模块 import，不补 `.mjs` 或改写 `.js`；Windows 盘符和 UNC 磁盘路径会明确失败。

#### 破坏性变更

- 删除 `LibraryComponentAttribute` 与 `VueLibraryComponentAttribute`，不保留 obsolete 兼容层。组件代理应从 `[VueLibraryComponent("package", "Export")]` 迁移到 `[ECMAScript("package", Transform.Component, "Export")]`；省略第三个参数表示 default export。

#### 边界

- RazorVue 组件仍必须同时满足 `ComponentBase + IVueComponent`，统一 Attribute 不能绕过组件身份约束。Blazor JS interop 继续保持现有 Reject 边界，不属于本协议。

## 2026-08-25

### Jazor 0.22.0

> RazorVue 组件入口约束统一，并交付首批 Blazor CLR 映射能力。本版本按 `MINOR` 通道发布；未完成真实浏览器 profile 证据的切片仍按路线图保持 `InProof`，不扩大为完整 Blazor UI 兼容。

#### 新增

- **统一 RazorVue 组件契约**：组件必须继承 `ComponentBase` 或其派生类，实现 `IVueComponent` 或其派生接口，并声明 `[ECMAScriptModule]` 或 `[VueLibraryComponent]` 导入描述。Vuetify、TDesign、Element Plus、VueDataUi、VuIcons 和 VueRoute 的生成/绑定组件沿用该契约。
- **Blazor CLR 映射首批切片**：`Jazor.Vue` 携带 `ECMAScript.Blazor` 映射，支持 DOM 事件参数的强类型只读投影、`ChangeEventArgs.Value` 的事件时刻捕获，以及 `ElementReference.FocusAsync` 的受控 DOM 焦点操作。

#### 边界与兼容性

- **内置 Blazor UI 组件不在产品契约内**：`Router`、`RouteView`、`NavLink`、`DynamicComponent`、`ErrorBoundary`、`EditForm`、`Input*`、`AuthorizeView` 等标签现在稳定报告 `JAZORVGA021`。请使用自定义 `ComponentBase + IVueComponent` 组件或现有的 TDesign/Vuetify/Element Plus 组件库。
- `ECMAScript.Blazor` 仍只随 `Jazor.Vue` 交付；核心 `Jazor` 包保持框架无关。事件参数构造、setter、runtime identity、`InputFile`/`IBrowserFile` 以及完整内置表单/认证 UI 仍不支持。

### Jazor 0.21.0

> CLR 运行时语义加固与泛型集合能力补齐。本版本包含新的 `Queue<T>` / `Stack<T>` 支持面，按 `MINOR` 通道发布。

#### 新增

- **`Queue<T>` 与 `Stack<T>` 核心操作**：新增构造、`Count`、入队/入栈、出队/出栈、`Peek`、`TryPeek`、`TryDequeue`、`TryPop`、`Contains`、`Clear` 与 `ToArray` 的受支持运行时映射。

#### 修复与一致性改进

- **数值语义**：修正 `long` / `ulong` 十进制解析、`IsPositive(0)`、窄整型最小值 `Abs` 溢出、负数奇次根、`ExpM1` / `Log*P1` 精度、`IsPow2` 边界判断，以及 decimal 超范围指数输入。
- **集合与数组**：修正只读数组视图对全部原型突变方法的拦截；`Array.Clear(array)` 现在清除元素但保持数组长度；集合比较器、哈希、批量操作及 `Queue<T>` / `Stack<T>` 的载体行为保持一致。
- **字符串与日期时间**：无分隔符 `Split` 保留 .NET 的连续空白空条目；`DateTime` carrier 的时区转换与 `GetTicks(Date)` 保持一致。

#### 兼容性

- 未引入包名、程序集边界或现有公共 API 的破坏性变更；既有代码可直接升级，新增集合成员按需使用即可。

## 2026-08-24

### Jazor 0.20.0

- **Breaking package boundary:** `Jazor` is now framework-neutral and can be used for ordinary C# -> ECMAScript libraries without Vue payloads. Vue authoring, Razor-to-Vue integration, Vue bindings, and local Vue browser runtime assets are delivered by the explicit `Jazor.Vue` package; Vue ecosystem packages now depend on the same-version `Jazor.Vue` package.
- `ECMAScript.Blazor` is introduced as the first-party Blazor framework-to-browser mapping contribution. Its initial Mouse/Keyboard/Focus event getter declarations ship as a `Jazor.Vue` payload; the `Jazor` package does not install that assembly or add a Blazor framework reference. Runtime modules/helpers remain owned by `Jazor.CLR`.
- RazorVue browser navigation now supports `NavigationManager.RegisterLocationChangingHandler(...)`. Handlers receive `LocationChangingContext`, can call `PreventNavigation()`, and observe cancellation when a later navigation supersedes an in-flight handler. The browser runtime now provides the required `ValueTask`, `CancellationToken`, `CancellationTokenSource`, and `CancellationTokenRegistration` slice.
- **Breaking:** ECMAScript property-key and property-descriptor authoring types are now named `JazorPropertyKey` and `JazorPropertyDescriptor`. Update source references from `JPropertyKey` and `JSPropertyDescriptor`; generated JavaScript property-key and descriptor behavior is unchanged.
- Browser lowering now rejects exception families without a stable supported JavaScript identity, including `InvalidOperationException` and `DivideByZeroException`. Use the supported `System.Exception` surface where a general browser error is intended.

## 2026-08-21

### Jazor 0.19.0

- RazorVue browser navigation now supports `NavigationManager.OnNotFound` subscriptions and `NavigationManager.NotFound()` dispatch, including a browser-side `NotFoundEventArgs.Path` payload for component callbacks.
- `NavigationManager.ToAbsoluteUri(...)` now returns a URL-backed `System.Uri` value. RazorVue components can use the supported absolute URI, path, query, fragment, host, authority, scheme, port, and path-and-query members without losing the resolved browser URL.
- Packaged Wiki SPA consumers now materialize their documentation catalog before publish, so detached release verification retains the generated documentation routes.

## 2026-08-20

### Jazor 0.18.0

- ECMAScript browser bindings now use `JazorFile` and `JazorWindow` as their C# authoring names for the browser `File` and `Window` interfaces. Generated JavaScript ABI names remain `File` and `Window`; update source references from the retired `Files` and `Window` names.
- The ECMAScript host now exposes `Global.IsUndefined(value)` for strict JavaScript `undefined` checks. It remains distinct from C# null checks, so authors can distinguish an omitted browser value from an explicit `null`.
- RazorVue browser navigation now supports `NavigationManager.Refresh(false)` and reliably appends or replaces query parameters through `NavigationManagerExtensions`.
- RazorVue continues to lower `base.StateHasChanged()` and `base.InvokeAsync(...)` as component lifecycle protocol calls while leaving unrelated framework base members on the normal CLR whitelist path.
- The RazorVue acceptance gate temporarily requires 94% branch coverage while its active integration work continues; the core compiler gate remains at 97%.

- **Breaking:** JazorAdmin retires the 22 upstream TDesign Starter replica/result routes and their branded result assets, and adds an audit trail plus a `JazorAdmin.DemoClient` sample. Migrate bookmarks and demos to the portal, IAM, and platform-operations routes documented by the JazorAdmin reference application; the retained shell, appearance drawer, and dashboard are not replacement route aliases.
- RazorVue member closure now overlays `ParameterView` state: an effective `SetParametersAsync` override participates in component initialization, and parameter auto-property storage is retained even before render code reads it, so missing parameters keep their CLR default or previous value.
- New RazorVue compatibility analyzers `JAZORVCA001`/`JAZORVCA002` report injected `DbContext` and other server-only ASP.NET services in RazorVue browser components at the authored source location instead of failing during artifact generation.
- Jazor now has an official website deployed to GitHub Pages from the repository `docs/` tree: a docs-driven generated route catalog, static full-route export, and a Material 3 (Sober web components) responsive shell with overlay navigation drawers on mobile.

- RazorVue now lowers official Razor Source Generator generic component type-inference helpers and open generic `OpenComponent<T>` calls. Generic type parameters remain erased at runtime, and generated artifacts no longer leak render-builder symbols from nested fragments.
- Reachable `RenderFragment` properties and methods are retained when ordinary component members reference them, preventing generated modules from silently calling an omitted helper.
- Final RazorVue artifact generation validates the final compilation before artifact fan-out, so invalid Vue injection metadata reports one mapped diagnostic without leaving a partial module catalog.
- The C#-to-JavaScript compiler now supports indexed nested initializers such as `Items[0] = { ... }` while preserving the enclosing initializer target and evaluation order.
- Checked-in sample manifests use portable root assembly identity instead of developer-specific absolute assembly paths.

## 2026-08-18

### Jazor 0.16.3

- Compiler and RazorVue acceptance gates now require at least 97% branch coverage, with targeted boundary regressions covering the supported lowering and component-generation paths.

## 2026-08-17

### Jazor 0.16.2

- RazorVue components now replay field/property initialization and parameterless source constructors from base to derived setup order, keep static component members at module lifetime, and route supported source-base lifecycle/dispose dispatch through the established Vue hooks.
- Direct RazorVue render lowering now preserves ordinary `break` and `continue` in verified `for`, `foreach`, `while`, and `do while` content segments. Labeled branches, branches across open RenderTreeBuilder frames, and `goto` remain explicit diagnostic boundaries.
- RazorVue component inheritance now retains required base declarations locally while the most-derived declaration owns a conflicting public ES module export name, avoiding invalid duplicate exports without changing source-level dispatch.

### Jazor 0.16.1

- RazorVue direct lowering now preserves the parent instance context for dynamic slot descriptors, retains explicit loop-item keys when scoped lowering cannot use `renderList`, and imports `createStaticVNode` only when retained module hoists require it.
- Supported C# collection initializers now retain their declared or configured `Add` runtime name, preventing an empty JavaScript member call for current-module and mapped runtime types.

### Jazor 0.16.0

- RazorVue now reports final-compilation failures through stable `JAZORVGA020`-`026` diagnostics. Direct-render, compiler bridge, component binding, member closure, VueInject, and Vue module failures point back to mapped `.razor` or authored C# locations where available, include actionable documentation links, and suppress partial artifact catalogs when a component generation fails.
- Runtime member classes captured by RazorVue components are now safe to use through Vue deep Proxies. Private fields, auto-property backing storage, primary-constructor capture, and field-like event storage use collision-checked internal properties while preserving class, inheritance, and event behavior.
- RazorVue explicitly rejects labeled `break` and `continue` shapes that have no verified lowering contract, including future Roslyn branch representations, instead of silently changing control flow.
- Release validation now publishes an isolated `JazorSSR=true` RazorVue TodoList consumer from the generated NuGet packages, verifies packaged SSR output and PathBase resource resolution, then confirms Edge hydration restores interaction.

## 2026-08-15

### Jazor 0.15.0

- `ECMAScript.Pinia` now ships Pinia 4.0.3 and `ECMAScript.Pinia.Testing` ships `@pinia/testing` 2.0.1. Installing a Pinia root with `app.Use(pinia)` automatically registers its development panel with the Vue Devtools browser extension; production Pinia output omits that Devtools dependency closure. The retired `TestingOptions.WritableComputed` option has been removed because Testing 2 applies its writable-computed behavior internally.
- `ECMAScript.Vue.Devtools` is now an independent NuGet binding for the public `@vue/devtools-api` 8.1.5 plugin-authoring API. It covers typed plugin descriptors and settings, custom inspectors, timelines, component hooks, custom tabs/commands, and Devtools connection callbacks. The package reuses Jazor's local Vue Devtools runtime closure without exposing `@vue/devtools-kit` or altering Pinia 4's automatic Devtools registration.
- `ECMAScript.VueDataUi` now exposes all 71 public `vue-data-ui` 3.23.4 entries as typed RazorVue components. Components keep per-entry ESM imports, and PDF-capable entries use a browser-ready local `jspdf` ESM runtime instead of a bare dependency import.
- `ECMAScript.VuIcons` is a new binding for all 1,821 Vue 3 wrappers in `vu-icons` 1.5.4. Known `Vu*` components materialize only their own SVG module; the typed `VuIcon` dynamic-name bridge intentionally brings the full catalog when runtime selection requires it.
- RazorVue direct render lowering now supports official Razor `@for`, `@while`, and `@do while` alongside `@foreach`. It preserves C# loop-local closure behavior, body/condition evaluation order, and keyed/unkeyed dynamic Fragment metadata. `break` and `continue` remain explicit unsupported control-flow boundaries in this direct-render slice.
- RazorVue generated modules now retain collision-safe aliases for nested static member imports and preserve conditional branch vnode shapes without synthetic wrappers around opaque roots.
- `Jazor.Admin` and the JazorAdmin sample add overview and notification modules, improve scheduling query support, and refresh the administration shell, navigation, account, access-control, organization, settings, and SSO surfaces.

## 2026-08-14

### Jazor 0.14.0

- RazorVue direct render modules now generate Vue child block trees for a single dynamic string child, static-plus-dynamic text, and nested stable elements. These artifacts use Vue `TEXT` patch flags and `createTextVNode` only where the generated Razor C# proves the text surface, reducing ordinary child traversal during updates without changing C# evaluation, formatting, slot, loop, or raw-markup behavior.
- Conditional content, slots, render sequences, dynamic raw markup, and ordinary component children intentionally retain Vue's full `h(...)` children diff until their own stability and closure contracts are proven. Production Vue browser and SSR verification now exercises the new child block and text patch shapes alongside static markup and hydration coverage.
- Proven simple Razor `foreach` loops now lower to Vue `renderList` inside `openBlock(true)` fragments. Explicit `@key` emits `KEYED_FRAGMENT (128)`, unkeyed loops emit `UNKEYED_FRAGMENT (256)`, and the path deliberately retains Vue's original collection protocol instead of converting object/iterable/range sources through `Array.from(source ?? [])`. Production Vue verification confirms keyed DOM identity during reorder and rejects an accidental stable-fragment flag for unkeyed lists.
- RazorVue now classifies component slots instead of marking every slot dynamic. Fixed authored, named, and scoped slots use `withCtx` with `_: 1`; conditional, forwarded, nullable, and non-stable-scope slots use `createSlots({ _: 2 }, descriptors)` with `DYNAMIC_SLOTS (1024)`. Slot components retain a Vue block boundary, branch-specific functions and keys are preserved, and production Vue verification covers stable parent updates plus conditional DOM replacement.
- Roslyn-proven direct `string` value and `bool` checked binds now emit a single DOM event arrow instead of the generic event/value discriminator and rest-forwarding adapter. Complex setters, modifiers, method groups, bind callbacks, and conversion-bearing binders remain on the semantic-preserving general path.
- `OnParametersSet` and `OnParametersSetAsync` now watch a stable shallow projection of declared Vue props rather than recursively traversing the props object graph. Scalar changes and reference replacement trigger the lifecycle; nested mutation of the same reference intentionally does not. Async serialization, stale-generation suppression, rerender behavior, and initial invocation are unchanged.
- ASP.NET Core SSR now uses a bounded persistent DenoHost worker pool with a line-delimited stdin/stdout protocol. The artifact manifest, SSR import map, and packaged runner identify an ESM generation; new generations retire old workers without interrupting in-flight renders. Per-request temporary JSON and process startup are removed, while cancellation discards the leased worker, crashes recover on demand, concurrency stays within `WorkerCount`, and application disposal drains renders before stopping processes.
- RazorVue artifact generation now builds independent component artifacts through a bounded worker pool while preserving stable discovery, output order, diagnostics, module text, and source maps. Larger component sets can therefore use available build concurrency without making HMR or generated output non-deterministic.
- Release artifact materialization now follows the generated application's actual package imports and each package's declared runtime-file closure. Browser output no longer copies unrelated Vue server-renderer or devtools files; SSR explicitly carries its Vue/server-renderer graph. This reduces publish and deployment footprint without claiming a network saving for modules a browser never requested.
- Jazor-owned CLR runtime modules now materialize from the application's reachable runtime-import closure instead of copying an unused full catalog. Applications retain their required runtime behavior while release artifacts avoid unrelated CLR modules.

## 2026-08-13

### Jazor 0.13.0

- Windows SPA release publishing now has a real package-consumer gate. It packages `Jazor`, `Jazor.Vue`, and `ECMAScript.Style` locally, restores an isolated copy of Wiki without source-project references, publishes it with `JazorMode=release`, verifies the `<publish>/jazor/` release layout, and drives Microsoft Edge through a `/docs` PathBase. The tag publishing workflow runs this gate before any NuGet upload.
- Wiki is now a live `ECMAScript.Vue` `H()` and `ECMAScript.Style` consumer. Its H-function authoring page renders a generated `ecs-*` class and the CSS runtime's managed `#ecmascript-style` element; debug and release browser gates assert the generated selector, shorthand CSS, source maps, routing, and production HMR boundary in a real browser.
- Wiki publish and preview commands now explicitly select release output and serve `bundle.js` in production. Its custom HTML shell supplies the generated `style.mjs`, `components/`, and `System/` import-map namespaces under a PathBase, so emitted modules resolve consistently and absent generated assets remain HTTP 404s instead of silently becoming SPA HTML.
- `JazorWebApplication.CreateBuilder()` now ignores empty copied `bin/.../jazor/` directories and selects a ready artifact graph. Debug and `dotnet watch` hosts therefore keep serving the project-root `jazor/` output instead of an empty copied directory.
- Params-array preservation now recognizes `PreserveAttribute`. CSS shorthand calls such as `padding(px(4), px(8))` retain both values and emit `padding:4px 8px;`; generated CSS naming and the public `style.mjs` export ABI remain unchanged.
- Double and single decimal rounding now compares the original IEEE-754 payload instead of a lossy JavaScript decimal multiplication. `Round(2.675, 2)` and equivalent `Math.Round` calls preserve .NET's rounding side, while exact values and midpoint modes retain their original results.

## 2026-08-12

### Jazor 0.12.0

- Generated Jazor artifacts now default to the project-root `jazor/` directory instead of `wwwroot/jazor/`. `debug` continues to emit modules, source maps, and `jazor-manifest.json`; `release` continues to emit the browser bundle, and SSR release builds retain the raw module graph they require.
- `dotnet publish` now copies the generated graph explicitly to `<publish>/jazor/` for both Web SDK and non-Web SDK hosts. `UseJazorHost()` mounts the project or publish `jazor/` directory at the existing browser URL `/jazor/*` before ordinary web-root files, so a stale `wwwroot/jazor/` directory cannot shadow generated assets. There is intentionally no `wwwroot/jazor/` fallback: move checked-in or custom artifacts to the new root, or configure an explicit artifact root.
- Development reload is now configured through `AddJazorReload()` and `UseJazorReload()`. Its defaults observe both `jazor/` and `wwwroot/`, map `jazor/` to `/jazor`, and keep generated artifacts out of MSBuild's normal watch inputs to avoid rebuild loops. Use `dotnet watch run` as the development entry point when Jazor artifacts should rebuild and refresh; the reload service observes the emitted graph after the build completes.
- ASP.NET Core authoring APIs now use concise, consistent names: `AddJazorSSR` / `UseJazorSSR` become `AddJazorSsr` / `UseJazorSsr`; `AddJazorDevelopmentReload` / `UseJazorDevelopmentReload` become `AddJazorReload` / `UseJazorReload`; `UseJazorDevelopmentAssets` and `UseJazorWebAssets` become `UseJazorArtifacts` and `UseJazorAssets`. Their option models are likewise shortened to `JazorArtifactOptions`, `JazorAssetOptions`, `JazorSsrOptions`, `JazorReloadOptions`, and `JazorHmrMapping`. Host, SSR, and reload extension implementation types are now `JazorExtensions`, `JazorSsrExtensions`, and `JazorReloadExtensions` rather than receiver-type-heavy names.
- The browser-facing HMR ABI is unchanged: `/@jazor/client`, `/@jazor/reload`, `JazorHmr`, module-update messages, and the generated `/jazor/*` URLs remain stable. Existing browser bootstraps do not need a casing or protocol migration.
- `ECMAScript.Style` uses CSS-facing `lower_snake_case` for generated declaration properties, `css` facade members and tokens, and rule members such as `additional` and `children`. CSS models and configuration records such as `CssRule`, `CssDeclarations`, `CssAtRule`, `CssShadow`, `CssChild`, and `CssOptions` remain PascalCase. A CSS declaration such as `background_color` serializes as `background-color`, while generated WebIDL members retain their native DOM spelling such as `backgroundColor`; no implicit casing bridge is introduced at that API boundary. Generated CSS and the `style.mjs` export ABI remain unchanged.
- `Jazor.Analyzer` now reports contradictory `Description("@#...")` and `ECMAScriptName` metadata, plus duplicate final JavaScript names in module exports, generated runtime classes, and structural-record object keys.
- External component wrappers now share the framework-neutral `LibraryComponentAttribute` contract. `Jazor.Analyzer` no longer special-cases Vue metadata, while `VueLibraryComponentAttribute` remains a compatible Vue-specific derived attribute.
- Local package builds now use NuGet's standard `$version$` nuspec token, keeping custom-nuspec package output discovery compatible with the current .NET 11 preview SDK.

### Jazor 0.11.0

- Element Plus bindings now ship the local `2.14.4` runtime, stylesheet, license, manifest, and regenerated authoring contracts. The update includes typed `ElTransfer.VirtualScroll` and `ElTransfer.ItemSize` parameters.
- `Jazor.Analyzer` now reports unsupported concrete external types earlier when they appear in generic containers, ECMAScript interface or delegate signatures, and runtime type filters such as `is`, pattern matching, `switch`, and `catch`.

## 2026-08-11

- Generated WebIDL bindings now expose the browser `File` interface as `ECMAScript.Files`, avoiding an authoring collision with `System.IO.File`. Its JavaScript ABI remains `File` through the generated `Description("@#File")` mapping; C# callers should update `ECMAScript.File` references to `ECMAScript.Files`.
- `ECMAScript.Style` now refreshes its WebRef grammar catalog to 817 properties and exposes modern anchor-positioning and intrinsic-sizing syntax through dedicated typed domains. `anchor()`, `anchor-size()`, `calc-size()`, and `fit-content()` remain constrained to the CSS properties that accept them instead of falling back to an untyped string or generic CSS value.
- `ECMAScript.WebIDL.Generator` now refreshes its WebRef sources and emits developer-facing XML documentation from source-authored W3C/WHATWG specification prose, exact definition anchors, and available specification examples. Unverified or unrelated prose is omitted rather than synthesized.
- `Jazor.Emit` now consumes framework-neutral `Jazor.Generated.ArtifactCatalog` and `Jazor.Artifacts.RuntimeProviderCatalog` contracts. Vue runtime resources, internal import-map entries, and Vue HMR details are supplied by RazorVue; assemblies built against the retired `Jazor.Generated.VueRenderCatalog` contract must be rebuilt.
- Vue 3 core bindings are now published as `ECMAScript.Vue`, with the C# authoring host renamed from `Vue3` to `Vue`. The `vue3` runtime library ID remains stable so existing manifests and emitted asset paths continue to resolve.
- Jazor 0.8.4 reorganizes the documentation center into stable overview, architecture, guide, roadmap, and history sections. Current installation, quick-start, package configuration, module documentation, and example entry points now use one consistent Chinese-first format; superseded exploration material is condensed into a single evolution record.
- The RazorVue release gate now verifies 4,689 official Razor Source Generator scenarios with 97.63% line coverage and 96.03% branch coverage, including source-text carrier and logical-to-physical Razor path resolution boundaries.
- Development reload now registers generated RazorVue components with Vue's HMR runtime. Compiler-proven template-only updates reload the affected component in place and preserve parent component state; unavailable Vue HMR support, failed module imports, descriptor or logic changes, and other unproven boundaries still use a full-page reload.
- RazorVue debug modules now declare their external source map and embed the authored Razor text as `sourcesContent`. Browser DevTools can open the originating `.razor` source directly from generated render-function code without an additional source-file HTTP route.
- ASP.NET Core applications can now opt into Vue SSR. The generated artifact graph includes local Vue server-renderer assets, import maps, styles, server HTML, and browser hydration without application `node_modules`, a CDN, or remote imports. DenoHost is the SSR runtime executor, while Netpack produces browser bundles; Jint is not part of the supported execution path.
- Pinia sample applications now retain authored C# member names and declare Pinia's lowercase protocol keys explicitly where the runtime requires them, so generated browser modules and test workflows remain compatible with the explicit naming contract.
- Current Pinia and Vue Router samples now validate Netpack release bundles separately from their DenoHost-backed runtime smoke paths. Retired generated-SFC/Deno-bundle sample fixtures are removed from the active product tree.

## 2026-08-10

- Jazor 0.8.0 makes ECMAScript name resolution explicit. Unmapped C# symbols retain their authored names, while JavaScript ABI differences are declared per member with `Description("@#...")` or `ECMAScriptName`; RazorVue no longer infers prop, listener, or slot names from casing or Vue conventions.
- JazorAdmin now restores saved appearance preferences before its first render, so returning after changing theme or layout preferences no longer interrupts the session transition.
- Vuetify bindings are updated to 4.1.8. Vue3, Vuetify, Element Plus, TDesign, Pinia, Pinia Testing, and Vue Router each complete the public binding-contract audit at 100%, exceeding the 0.8 per-package 96% gate.
- The release baseline rechecks 10,318 compiler scenarios at 98.91% line and 96.01% branch coverage, plus 4,684 official Razor Source Generator scenarios at 97.57% line and 96.00% branch coverage.
- Jazor 0.7 raises the independent public Vue binding-contract audit gate to 90%. Vue3, Vuetify, Element Plus, TDesign, Pinia, Pinia Testing, and Vue Router pass every currently audited contract unit and their corresponding test lanes.
- Development reload can now dynamically import a compiler-proven template-only module update and pass it to a consumer-registered `JazorHmr.accept(moduleId, handler)` callback. Missing handlers, failed imports, descriptor or logic changes, and all other unproven boundaries fall back to a full-page reload. This does not automatically replace Vue instances or preserve component state.
- The release baseline now includes a real-browser HMR workflow: a manifest template diff reaches the browser over WebSocket, loads a cache-busted module, and invokes the registered handler. The G2 performance report remains a measured baseline with its gzip and retired-line comparison warnings visible.
- Jazor 0.6 now verifies every supported Vue binding package independently. Vue3, Vuetify, Element Plus, TDesign, Pinia, Pinia Testing, and Vue Router all meet the 80% public binding-contract audit gate, while their corresponding test lanes complete without failures.
- Development reload now offers a first opt-in `.mjs` module-update path: capable clients receive a cancellable `jazor:module-update` browser event, while older clients, unhandled updates, and non-module changes continue through full-page reload. This is a controlled first HMR phase and does not claim dynamic import or state preservation.
- RazorVue G2 now records a repeatable release performance baseline from an external official Razor SG consumer, generated modules and source maps, Node measurements, and a real-browser heap/timer lane. Any threshold warnings and unavailable retired-line comparison remain visible in the report.
- Jazor 0.5 raises the RazorVue quality gate to 96% branch coverage: 4,675 official Razor Source Generator scenarios now pass with 97.71% line coverage and 96.01% branch coverage.
- RazorVue direct-render and generated-C# binding regressions now cover metadata-only render methods, generic scoped content, helper fragment propagation, import boundaries, and render-frame ordering without weakening the official Razor SG input contract.

## 2026-08-09

- Jazor 0.4 now meets its RazorVue release gate: all 4,613 official Razor Source Generator scenarios pass, with 90.01% branch coverage. Local NuGet package consumers also pass the Deno execution and real-browser Counter smoke paths.
- The embedded DenoHost runtime is updated to 2.9.5. Deno bundling and the explicit Deno and Netpack production-toolchain checks remain verified against the current source.
- JazorAdmin now reproduces the TDesign Starter administration shell with Tencent Cloud's rounded theme, compact page spacing, medium-size route tabs, and an administrator-only appearance configurator fixed at the bottom center. Administrators can switch theme, brand color, layout, menu split, sidebar behavior, header, breadcrumb, footer, tabs, and menu-collapse preferences from the same global panel.
- The mixed navigation shell now keeps the IconBar as its primary navigation, fixes the selected primary module title in a 64px secondary-rail header, and scrolls only the secondary menu. Collapsing removes the secondary rail completely and lets the content reclaim its width; the logo, header toggle, content gutters, and mobile layout remain aligned without horizontal overflow.
- IconBar quick actions now open from a breathing circular trigger into four square actions distributed at 22.5-degree intervals on a 90-degree arc. The shell also restores route-aware multi-page tabs and keeps the login artwork visually continuous with its form surface.
- `ECMAScript.Style` now models common Starter layout values such as gradients, gaps, radii, flex shorthand, grid lines, and background sizes with typed CSS value domains, and `important(value)` preserves the original value domain. TDesign multi-argument events expose their first runtime Vue argument as the generated `EventCallback<T>` payload, matching the component runtime contract.

## 2026-08-07

- JazorAdmin now creates a development-only bootstrap platform administrator when its store has no platform administrator: `admin@jazor.local` / `JazorAdmin123!`. Production and staging deployments must supply the first administrator and exact OpenIddict callback origins from their deployment configuration; `localhost` is confined to development settings. Startup fails with an actionable configuration error rather than exposing an unusable login or SSO path. Bootstrap creates or promotes only the first administrator and never resets an existing password.
- JazorAdmin now has an original Jinsha sunbird-inspired local mark for login, shell, consent, and browser-tab branding. The mark ships as a scalable SVG plus a 16/32/48/64px ICO fallback, without a CDN, custom font, or runtime image dependency.
- JazorAdmin login now uses a local cyan-green ink/mineral landscape behind an Aero-style glass form. Anonymous password sign-in also requires a server-issued, one-time four-character image captcha valid for three minutes; the login artwork and captcha have no external image, font, or CDN dependency.
- `ECMAScript.Style` now models border shorthand and filters as typed value grammars. `px(1) | solid | var("--border")` produces a `CssBorder` accepted only by border properties; `blur`, `grayscale`, `saturate`, and `filters(...)` cover common filter values. Named border widths/styles are composable token types because C# enums cannot implement `|`; closed non-composite keyword domains remain string enums. `!important` remains declaration-level through `important(...)` rather than becoming a value token.
- JazorAdmin now covers the active OpenIddict administration surface: applications, scopes, authorizations, and tokens. Application profiles model interactive, machine, and API clients with public/confidential client types, one-time secret creation and rotation, consent and PKCE, URI and scope assignment, plus endpoint, grant, and response permissions. Scope resources determine token audiences; authorization and token records can be inspected and revoked.
- Explicit and systematic consent requests now render a local confirmation page that posts every validated OAuth parameter as form data, which preserves authorization-code, state, PKCE, nonce, prompt, and custom request values across the browser consent step.
- JazorAdmin's local-package browser smoke now exercises Machine/API registration, one-time secret display, Scope edits, authorization/token revocation, and mobile application-page viewport containment. It also catches RazorVue output that would use ECMAScript reserved words as local function names.
- JazorAdmin now has three independent operating centers: SSO management, typed configuration, and task scheduling. The configuration center supports text, boolean, number, and JSON values. The task center uses Quartz.NET for Cron triggers, misfire handling, and single-task concurrency; administrators can enable, pause, manually run, and inspect catalogued tasks, without submitting arbitrary executable code. The initial task safely prunes expired OpenIddict records.
- JazorAdmin uses the concise `ja-*` CSS namespace consistently across its shell, pages, generated modules, and browser verification.
- Vue Router and Pinia development builds now resolve their Vue Devtools API from a local, licensed Vue3 package resource. Browser development no longer requires a CDN, `node_modules`, or an unresolved bare module import.
- WebIDL string-enum generation now preserves the source wire token in `Description`, rather than deriving it from the PascalCase C# member. This fixes `RequestCredentials.SameOrigin` to emit `"same-origin"` and corrects the same defect for all generated enum values. JazorAdmin's API client also omits unset `headers` and `body` members instead of passing invalid explicit `null` values. Its isolated local-package smoke asserts both production paths, and its test project explicitly identifies itself as a test project so isolated `dotnet test` runs cannot silently skip API coverage.

## 2026-08-06

- Jazor 0.3.2 corrects the first 0.3 package release: `ECMAScript.Pinia.Testing` now carries its `@pinia/testing` browser ESM, manifest, and MIT license; Pinia is updated to 3.0.4 to satisfy that upstream dependency. Manifest materialization now distinguishes application modules such as `host/app.mjs` from external library imports, so package consumers do not need npm or a CDN.
- Vue component-library authoring now relies on ordinary C# and Razor contracts for props, events, model updates, and slots. The retired `VueProp`, `VueSlot`, library marker, style/plugin declaration, component flags, and emit-kind APIs are removed instead of requiring authors to duplicate information already expressed by `[Parameter]`, `EventCallback<T>`, `RenderFragment`, and member-level names.
- RazorVue now infers every `X` plus `XChanged` model update and converts conventional `OnX` callbacks to kebab-case Vue event names. Vuetify callback properties consistently use `OnX`; explicit emit metadata remains only for raw names that cannot be reconstructed, such as colon events and `loadstart`.
- All 113 Vuetify erased-value domains now use native C# unions. Existing `AsX` projections, scalar and array assignments, and collection expressions remain available, while handwritten tag/state wrappers and redundant `From(...)` factories are removed.
- TDesign's 14 erased-value domains now use native C# unions, and ElementPlus uses native unions for 45 of 46 domains. The one required tagged contract preserves exact `File` versus `Blob` projections for upload callbacks; neither library requires `From(...)` factories.
- Element Plus and TDesign component authoring types now follow their component names as `El*` and `T*`. The `ElementPlus` / `TDesign` root hosts and `IElementPlusComponent` / `ITDesignComponent` marker interfaces retain the package identities for clear cross-library references.
- Vue binding packages now keep their browser modules, styles, and licenses as local manifest-owned resources. RazorVue materializes those package resources directly; component contracts no longer carry stylesheet URL metadata, and applications need no duplicate library stylesheet declaration.
- Element Plus, Vuetify, and TDesign binding maintenance now uses one reproducible generator with deterministic validation commands. Frozen upstream inputs are maintained only with that generator, while published binding packages retain their contracts and local runtime resources.
- Jazor 0.3.0 packages Vue 3, Vue Router, Pinia, Vuetify, Element Plus, and TDesign browser ESM, styles, licenses, and resource manifests locally. Applications build and bundle from restored NuGet resources without a project `node_modules`, CDN imports, npm downloads, or network access.
- JazorAdmin now follows the TDesign Starter navigation hierarchy with an independent IconBar for primary work areas and a scoped TDesign secondary menu. Both tiers share one route catalog; desktop collapse retains the 64px IconBar, and mobile reflows the IconBar and secondary menu above the work surface without horizontal overflow.
- TDesign string-literal props now emit their authored values such as `"light"`, `"text"`, and `"primary"` instead of numeric enum ordinals, so generated menus, buttons, layouts, and other components receive the runtime values their typed C# contracts represent.

## 2026-08-05

- ElementPlus binding generation now emits ordinary Razor parameters, standard `EditorRequired` metadata, and member-level `ECMAScriptName` only when a prop or slot differs from naming conventions. Regenerated components no longer contain `VueProp`, `VueSlot`, style, or plugin descriptor duplication.
- TDesign bindings now use inherited standard `class` / `style` / default-slot conventions and member-level `ECMAScriptName` for exceptional content, styling, and camelCase slot names. They no longer declare `VueProp` or `VueSlot` metadata.
- Vuetify authoring bindings no longer declare `VueProp` or `VueSlot` metadata. Exceptional prop names and dot-qualified slots now use the same member-level `ECMAScriptName` contract as ordinary compiler naming, while Razor property names and component usage remain unchanged.
- RazorVue component names now come from the effective `[Parameter]` symbol: member-level `ECMAScriptName` or `Description("@#...")` mappings override legacy class descriptors, derived `new [Parameter]` members replace hidden base parameters, and duplicate final Vue names fail explicitly. `VueProp` and `VueSlot` remain migration-only compatibility metadata until generated bindings are updated.
- Vuetify RazorVue bindings now favor ordinary C# and Razor contracts: `X` plus `XChanged` supplies two-way binding, `OnX` supplies ordinary listeners, and `ChildContent` / `DefaultContent`, `XContent`, and PascalCase named fragments supply Vue slots. Bare `Save`, `Load`, `Next`, `Prev`, `AfterEnter`, `AfterLeave`, and `Submit` callback parameters were renamed to their `OnX` forms. Only Vue names that C# cannot express, such as colon events and dot slots, retain explicit metadata.
- `VuetifyGridSpanValue` now uses the native C# union contract while retaining its boolean, number, string, and numeric assignment authoring forms.
- `VCalendar` date, allowed-date, and interval-format values now use native C# unions. Their `AsX` projections and scalar/array convenience conversions remain, while the redundant JavaScript `From(...)` factories are removed.
- `VColorPicker` mode, color, swatch, and swatch-collection values now use native C# unions. Collection expressions and existing scalar, numeric, and nested-array conversions remain available without inline `From(...)` factories.
- `VIconBtn` size-map and text values now use native C# unions, retaining collection expressions and boolean/numeric/string assignment forms without inline `From(...)` factories.
- `VSnackbarQueue` message lists and individual `string | options` messages now use native C# unions, preserving message-array authoring without inline `From(...)` factories.
- VCarousel vertical delimiters and VChip selected-class slot values now use native C# unions, retaining their boolean, enum, and string-array forms without inline `From(...)` factories.
- `VTimePicker` model values and allowed-unit array/resolver values now use native C# unions, retaining date, numeric-array, and callback assignment forms without inline `From(...)` factories.
- `VConfirmEdit` action collections and disabled values now use native C# unions, retaining collection expressions and `bool | actions` authoring forms without inline `From(...)` factories.
- `VDateInput` display-format values now use a native C# union, retaining both string and formatter-callback authoring forms without inline `From(...)` factories.
- `VDatePicker` weekday, multiple-selection, model, allowed-date, and active values now use native C# unions. Collection expressions and scalar, numeric, array, and resolver assignment forms remain available without inline `From(...)` factories.
- `ECMAScript.Style` now models `box-shadow` as typed C# data: compose one or more `CssShadow` records with `shadows(...)`, including optional blur, spread, color, inset, variables, `none`, and CSS-wide values. JazorAdmin themes and components now use the same typed shadow surface instead of raw shadow strings.
- JazorAdmin now validates the production application route in one ASP.NET Core host: RazorVue UI, Web API, Identity, OpenIddict SSO, organization and membership management, role-based resource-operation grants, platform accounts, and OpenID client/scope configuration. Its TDesign-inspired icon rail and scoped secondary navigation are authored with Razor and `ECMAScript.Style`, with no application-owned JavaScript, CSS, static `index.html`, or Blazor registration.
- CSS-in-JS keyframes now preserve `params` frames as one JavaScript array, and global selector validation accepts all legal CSS whitespace, including line breaks in readable selector lists.
- Jazor 0.1.48 build targets now exclude native runtime DLL assets before invoking the managed emit tool, so RazorVue builds work with dependencies such as SQLite that ship native `.dll` files.
- Jazor 0.1.47 allows external ECMAScript host proxies to consume a module's `default` export while keeping Jazor-authored module declarations on deterministic named exports.
- Generated WebIDL bindings now represent `ByteString` browser text as `string`, including Fetch, Headers, navigation preload, and XMLHttpRequest contracts.
- WebIDL bindings now distinguish WebCrypto's `BigInteger` byte-array typedef from the JavaScript `bigint` primitive, which maps to `System.Numerics.BigInteger` and retains the concise `AsBigInteger` union projection.
- Jazor 0.1.46 packages now ship the Acornima analyzer assemblies that match the compiler's 1.7.0 ABI, preventing runtime `MethodNotFoundException` failures during Razor compilation.
- Bound extension method groups now retain their receiver when used as delegates, including identifier receivers; generated callbacks preserve the original call target instead of losing instance context.
- Compound assignment, unsigned right shift, implicit derived constructors, property initialization, interpolation format intrinsics, and host-bound member dispatch now have focused Roslyn-operation regressions for their evaluation and runtime-shape contracts.
- Whitelist generation now rejects incomplete alias declarations at generation time, preventing a catalog entry with no usable runtime name.
- Compiler packages now use Acornima 1.7.0 while preserving the existing ESTree emission and parsing contracts.
- Imports whose public name collides with a declared or reserved module binding now receive a stable generated alias, and inherited generic static members retain their concrete runtime host.
- Interpolating `dynamic` values now reports the stable text-contract diagnostic instead of exposing an internal compiler exception.
- The compiler quality gate now verifies 10,297 genuine Roslyn `IOperation` scenarios at 98.94% line coverage and 96.01% branch coverage, satisfying its 10,000 / 98% / 96% release gate.

## 2026-08-04

- Standard interpolated strings now preserve C# null-to-empty conversion, `Boolean` text casing, numeric formatting, constant alignment, source-defined `ToString` dispatch, and single evaluation through compiler-owned ESTree lowering. Values without a stable runtime text contract now fail explicitly instead of inheriting JavaScript stringification.
- Source maps now keep an absolute source path when that source exactly equals the configured source root, avoiding a relative path that escapes the root directory.
- Compiler whitelist generation now publishes refreshed type and member mappings as one complete process-local catalog snapshot before CLR runtime modules are generated.
- The compiler quality gate now verifies 10,101 genuine Roslyn `IOperation` scenarios at 98.42% line coverage and 94.00% branch coverage. The RazorVue gate verifies 4,484 official Razor SG scenarios at 93.44% line coverage and 83.66% branch coverage.
- `DateTime` and `DateTimeOffset` now support their Gregorian-calendar constructor families, preserving calendar-null argument precedence, `DateTimeKind`, microseconds, and offset validation through the shared date carrier.
- `StringBuilder` now supports capacity-aware construction, `Capacity`, `MaxCapacity`, `EnsureCapacity`, string append and append-line paths, and content-based builder equality. Capacity growth preserves the .NET behavior where an allocation may briefly exceed `MaxCapacity` while already allocated space remains usable.
- `string.Intern` now runs through the string carrier contract, including null argument behavior; intern-table inspection remains an explicit boundary.
- Expression-tree and `IQueryable` lambda conversions now fail explicitly instead of being lowered as executable delegates, preserving the distinction from supported `Enumerable` callbacks.
- Generated runtime catalog assertions now track the shared char code-unit, comparer NaN ordering, and BigInt rotation helper contracts.
- Native `ECMAScript.Array`, `Set`, and `Map` now accept standard C# collection initializers; `Map` indexer and two-argument entries retain its typed `set` runtime behavior.
- Read-only collection, dictionary, and set construction now preserve their live-view and write-guard contracts instead of falling back to writable or snapshot carriers.
- Closures created inside a C# `for` loop now retain the loop control variable's single C# lifetime instead of inheriting JavaScript's per-iteration `let` binding behavior.
- `Nullable<T>.GetValueOrDefault(defaultValue)` now evaluates its receiver and explicit default argument eagerly from left to right before selecting the result, preserving fallback side effects even when the nullable contains a value.
- `Enumerable.Zip` now supports its three-source tuple overload alongside the existing two-source and result-selector forms, preserving source-order iterator creation and advancement, shortest-source termination, and reverse iterator closure.
- `Enumerable.CountBy` and both `AggregateBy` seed overloads now preserve comparer-aware grouping, first key representatives, insertion order, Int32 count bounds, and two-slot `KeyValuePair<TKey, TValue>` entries.
- Field-like instance events on generated non-record runtime member classes now preserve C# multicast subscription and removal semantics, invocation-list snapshots, method-group receiver identity, and conditional `Invoke` argument short-circuiting. Static, custom-accessor, virtual/override, by-reference, delegate-equality, and delegate-combination event forms remain explicit boundaries.
- Module methods, runtime member methods, and local functions using `yield` now generate JavaScript iterators; `async IAsyncEnumerable<T>` methods generate `async function*` while nested callback bodies remain isolated from the outer iterator shape.
- UTF-8 string literals now emit exact decoded UTF-8 byte sequences through the existing read-only span carrier, including escaped, raw, BMP, and supplementary-plane text.
- Lambda parameters with C# optional defaults now preserve omitted-call behavior at the generated JavaScript function boundary. By-reference lambda returns remain an explicit runtime boundary.
- Named arguments now retain C# source evaluation order while invoking Roslyn-bound parameter slots. `ref` and `out` array or member locations evaluate once and use the shared write-back protocol without reading an `out` location's prior value.

## 2026-08-03

- LINQ mappings now cover `Cast`, `OfType`, `TryGetNonEnumeratedCount`, comparer-aware `ToDictionary` and `ToHashSet`, and a broader set of selector, grouping, ordering, aggregation, and set operations through shared runtime helpers.
- CLR support now includes fixed-width integer and floating conversions, checked arithmetic, UTF-8 numeric parsing, Unicode character classification, deterministic scalar hash codes, and comparer-backed dictionary and set behavior.
- String span trimming and concatenation, line-ending replacement, joins and padding, one-dimensional `Array` parameter-index access, `ConditionalWeakTable` factory and clear operations, and `Exception` cause, `HelpLink`, and `Source` metadata now execute through generated runtime modules.
- `StringBuilder` fixed `float` and `double` append/insert overloads now reuse their corresponding numeric `ToString()` carrier semantics. Object/generic formatting, capacity, live views, CLR enumerators, and type/reflection protocols remain deliberate support boundaries.
- Read-only collection constructors and factories that require live views remain explicit support boundaries instead of returning writable or snapshot carriers.
- The compiler quality gate now verifies 8,265 scenarios at 96.26% line coverage and 90.02% branch coverage. The RazorVue quality gate verifies 4,482 official Razor SG scenarios at 93.44% line coverage and 83.68% branch coverage.

## 2026-08-02

- Nullable values and nested list patterns now preserve their C# null and single-evaluation semantics in generated JavaScript, including a stable failure for `Nullable<T>.Value` on an empty carrier.
- Official Razor components now retain static source-map paths, support optional `EventCallback` parameters with or without a listener, asynchronous `@bind:after` updates, and synchronous or asynchronous `@bind:set` method groups; their final render catalog generation remains stable across concurrent generator-driver use.
- Razor authoring errors reported by the official source generator now remain the sole diagnostic: RazorVue skips render-catalog generation for that invalid compilation instead of adding a secondary conversion failure.
- JazorAdmin now provides an expanded native RazorVue reference application with dashboard, release, audit, workspace, and settings flows, including controlled release-table selection and bulk actions.
- The compiler quality gate now verifies 8,158 scenarios at 96.43% line coverage and 90.71% branch coverage. The RazorVue quality gate verifies 4,472 official Razor SG scenarios at 93.44% line coverage and 83.67% branch coverage.

## 2026-08-01

- Razor-to-Vue now keeps ASP.NET Components catalog declarations and Razor-specific lowering inside the `Jazor.Vue` product boundary while retaining explicit, typed compiler extension contracts for product integrations.
- Official Razor source-generator output continues to produce direct Vue render-function `.mjs` artifacts; SFC, render-context marker protocols, and generated-builder fallbacks are not part of the supported output path.
- Dynamic `@attributes` on Razor components now maps descriptor-owned C# parameter names for plain objects, `Map` values, and KeyValuePair-shaped sequences before Vue VNode props are created, while explicit `@bind` values retain precedence.
- The JazorAdmin reference application now verifies local package consumption, native and `VueInject` builds, generated artifacts, and browser mounting through the packaged Deno host.
- The compiler quality gate now verifies 8,113 focused regression scenarios with 96.42% line coverage and 90.78% branch coverage, above its 8,000 / 95% / 90% release thresholds.

## 2026-07-31

- Nested structural record deconstruction now reads configured and inherited property keys directly, preserves nested `var` declarations, and no longer depends on record `Deconstruct` methods that are not emitted at runtime.
- Runtime member classes now preserve expression-bodied and block-bodied `init` accessors as JavaScript setters, including C# `field`-backed properties, while bodyless automatic `init` accessors retain their getter-only runtime shape.
- Private runtime member-class fields now use the same JavaScript private name in declarations and instance or static references, including compiler-generated property backing fields.
- Object literals keyed by compile-time negative ECMAScript numbers now emit valid computed-property syntax while preserving the numeric value.

## 2026-07-30

- CLR mappings now support `Half` through the Number carrier and `Int128` / `UInt128` through fixed-width BigInt semantics, including parsing, comparison, numeric helpers, bit counting, rotation, 128-bit arithmetic wraparound, and runtime-checked division and remainder overflow behavior.
- Generic whitelist compatibility lookup now reuses indexed key shapes, preventing ordinary generic method calls from repeatedly scanning the full CLR member catalog during compilation.
- Inline-backed CLR and host calls now preserve C# receiver and argument evaluation order, eager timing, and single-evaluation semantics when templates repeat, omit, reorder, conditionally consume, or capture placeholders in deferred functions.
- Composite property, tuple, and nested list patterns now evaluate each member input once, preventing repeated C# or JavaScript getter side effects while preserving pattern order and results.
- Nested object initializers now complete their values before assigning a property or indexer, preserve mapped setter dispatch, evaluate computed targets once, and prevent setters from observing partially initialized objects.
- Unsupported standalone `System.Index` and `System.Range` values now produce explicit, source-located diagnostics that direct authors to contextual `^` and `..` indexer or slice usage.
- Source-map source content now resolves through exact normalized syntax-tree paths, so files with the same name in different directories retain their own `sourcesContent` instead of relying on ambiguous filename fallback.
- Mapped compound assignment and increment/decrement now evaluate direct member receivers and index keys once before the right-hand side, including side-effecting fields, properties, and ECMAScript indexers.

## 2026-07-29

- `ECMAScript.Style` is the independent ECMAScript ecosystem package for framework-neutral CSS-in-JS. Its sole public facade is the lowercase static class `css`; consumers may use qualified calls such as `css.style(...)` and `css.px(...)`, or direct `style(...)` and `px(...)` calls through a static using.
- The 705 generated CSS properties now use native C# union domains and nominal values for lengths, percentages, colors, times, display values, tracks, transforms, and related syntax. Cross-domain and implicit string assignments fail at compile time, while `raw(...)` remains the explicit path for future or unmodeled CSS.
- Typed units, variables, colors, grid functions, transforms, keywords, and `calc(...)` operators compose as ordinary C# expressions. Mixed length-percentage arithmetic remains distinct from pure lengths.
- Debug materialization now publishes the single root entry `style.mjs` and its source map. The stable `jazor-css:v1` naming, class/keyframe hashes, DOM framing, isolated contexts, Shadow DOM ownership, snapshots, hydration, and release Bundle behavior remain unchanged.
- `ECMAScript.Style` remains an independent opt-in package and adds no Style-specific build configuration; it uses only `JazorMode`, `JazorDir`, and `JazorTool`.
- Dynamic Razor event modifiers now preserve their boolean conditions, including repeated `preventDefault` and `stopPropagation` modifiers, instead of being treated as unconditionally enabled.
- RenderTreeBuilder helpers can compose output after root-level local declarations while declarations inside an open element or component frame remain explicitly rejected.
- CLR runtime modules now annotate imported hashed JavaScript helper declarations with their authored CLR member names, making packaged runtime output easier to inspect without changing runtime behavior.
- Nullable `GetValueOrDefault()` calls now emit the correct default for the underlying value type, including booleans, characters, 64-bit integers, and enums.
- Compiler-generated temporary names are stable across repository relocations and parallel Git worktrees instead of depending on absolute source paths.
- BigInt-mapped increment and decrement operations now preserve BigInt operands for locals and mapped indexers, including `Int128` and `UInt128` values.
- Generated modules now publish and import the `System.Guid` runtime implementation for parsing, formatting, equality, and hash-code operations.
- Character-to-number and number-to-character conversions preserve UTF-16 code units, and nested conditional-access initializers emit valid ECMAScript expressions.
- Custom interpolated-string handler additions now report a source-located compiler diagnostic instead of leaking an internal range exception; handler creation, addition, and append protocols remain explicitly unsupported.

## 2026-07-28

- RazorVue now consumes the final Roslyn compilation produced by the Razor Source Generator. It no longer requires Razor host outputs or reparsing generated C#.
- Jazor output is configured through `JazorMode`: `none` produces no files, `debug` produces modules and a manifest, and `release` produces the production bundle.
- The default output root is `wwwroot/jazor`; release builds write only `bundle.js` and `bundle.js.map` to that directory.
- Public Vue binding packages are compatible with .NET 11 Preview 6.
- Razor-to-Vue generation is now supplied by the explicit `Jazor.Vue` package, while `Jazor` owns the shared analyzer and compiler dependencies so generators are loaded once.
- `Jazor.Admin` provides UI-library-neutral admin-shell contracts and native RazorVue shell components for layout, navigation, breadcrumbs, page actions, controlled collapse, routing targets, and application-wide display state. Forms, tables, authentication fields, and concrete pages remain application-owned.
- `ECMAScript.Style` adds an independent opt-in, framework-neutral CSS-in-JS runtime with 705 generated Webref properties, deterministic class and keyframe names, nested selectors, media/supports rules, global styles, nonce-aware DOM injection, HMR-safe adoption, and non-destructive extraction. It uses the existing `JazorMode` debug/release pipeline and introduces no style-specific build properties.
- RazorVue component lowering now keeps compiler semantics on the Acornima AST path. Import rebasing, string literals, and slot sequence normalization no longer serialize and reparse JavaScript text, and forwarded, named, scoped, typed, and conditional slots preserve zero, one, or many child nodes.
- RazorVue component events support both synchronous delegates and asynchronous `Func<Task>` / `Func<TValue, Task>` handlers.
- Switch expressions now preserve guarded discard-arm semantics: `_ when condition` falls through to later arms when its guard is false.
- The JazorAdmin sample uses `Jazor.Admin` for application framing, routing, controlled sidebar collapse, live sidebar/top navigation modes, breadcrumbs, and page containment. Sample-owned pages cover login, lock screen, localized 404/500 recovery, global theme/language/grayscale controls, asynchronous tables, typed forms, action feedback, and a responsive TDesign implementation through packaged real-browser smoke verification on desktop and mobile viewports. A separately built companion application verifies assembly-level `VueInject` replacement, implementation prop/slot names, default imports, slot rendering, and event-driven state updates on the current `.mjs` pipeline.

## 2026-07-27

- RazorVue render-context now covers the core generated component semantics for render surface, component props, DOM and component events, slots, bind, lifecycle, references, metadata, and browser DOM behavior.
- RazorVue now has a direct VNode emitter for linear element/content/attribute output, static component props/listeners, regions, static markup, non-generic and typed scoped slots, bulk attrs, element key/value updates, DOM bind modifiers, named event metadata, and ref captures, with render-context retained as oracle/transition coverage.
- Production bundling now supports explicit Deno and Netpack lanes over the same manifest contract.
- Import-backed `.vue` SFC assets now flow from explicit component references into the manifest and production bundles without source-root scanning or a separate frontend asset API.
- External package consumers can build RazorVue output and Netpack bundles from the local `Jazor` NuGet package path without relying on repository-local tool binaries.
- RazorVue now has a G2 performance benchmark entrypoint that records runtime throughput, browser heap, generated artifact size, build timing, and release performance reports.
