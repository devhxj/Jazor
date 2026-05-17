# Jazor.RazorVue

> Status: active reference
> Positioning: shared RazorVue semantic, Razor SDK bridge, and host protocol layer used by analyzer, emit, Jolt, and library-component packages.

`Jazor.RazorVue` 不再只是“库模式 lowering 项目”。在当前结构下，它承接整条 RazorVue lane 需要跨 `Jazor.Analyzer`、`Jazor.Emit`、`Jolt`、`ECMAScript.Vuetify` 共享的代码：核心语义、Razor SDK 桥接、artifact/catalog 模型，以及 RazorVue/Jolt 的宿主协议 DTO。

## Responsibilities

- 提供 RazorVue 入口分类、descriptor、render tree、canonical model、lowering 与 catalog。
- 提供 `RazorCodeDocument` / Razor IR 获取、文档定位与 template frontend 选择。
- 提供 legacy render artifact 与 design-time SFC artifact 的共享模型。
- 提供 `Documents/` 与 `Protocol/` 下的 RazorVue/Jolt 宿主协议 DTO。

## Boundaries

- `Jazor.Analyzer` 负责 Roslyn analyzer / incremental generator 宿主与 RazorVue authoring diagnostics。
- `Jazor.Emit` 负责 `.mjs` / `.vue` / manifest / source map 的物化与 bundling。
- `Jolt` 负责 LSP、DevServer、进程管理、工作区与运行时宿主编排。
- `Jazor.Common` 只保留真正通用的 `Format` 与 `SourceMaps`，不再拥有 RazorVue/Jolt 协议 DTO。
- 用户直接 authoring 的 `IVueComponent` / `IVueLibraryComponent` canonical 类型保持在 `ECMAScript.Vue3`；`VueLibrary*` 标记类型以及 `VuePropKind` / `VueEmitKind` / `VueComponentFlags` 归属 `ECMAScript.VueContract` / `ECMAScript.VueContract.Descriptor`，`Jazor.RazorVue` 直接消费这组正式合同，不保留旧位置回退。
- RazorVue 的 consumer authoring 合同是显式按需导入，而不是由 `Jazor` NuGet 包自动注入 marker alias。
- 组件文件如需直接使用 `IVueComponent` / `IVueLibraryComponent` 简名，应显式声明 `using static ECMAScript.Vue3;`；完整 Vue3 API 亦同。

## Current Layout

- `Discovery/`: 入口分类与候选发现。
- `Descriptor/`: props / emits / slots / registry / resolution。
- `RenderTree/`: 手写 `BuildRenderTree` authoring 前端。
- `RazorSdk/`: `RazorCodeDocument` / Razor IR 主前端与文档定位。
- `Canonical/`, `Sfc/`, `Lowering/`, `Artifacts/`, `Emit/`: shared artifact/model pipeline。
- `Documents/`, `Protocol/`: Jolt 与分析宿主共享文档/RPC 契约。

## Template Frontend Rule

- Razor 生成组件优先走 `RazorCodeDocument` / Razor IR。
- 只有源码中显式手写的 `BuildRenderTree` 组件才允许走 `BuildRenderTree` 前端。
- 对于 Razor 生成组件，如果既没有可绑定的 Razor 文档又不是手写 `BuildRenderTree` authoring，应显式失败，而不是静默回退。

## `@key` Support

- RazorVue 现已将 vnode `key` 作为一等语义处理，不会把它退化成普通 HTML / component attribute。
- 手写 `BuildRenderTree` authoring 支持 `RenderTreeBuilder.SetKey(...)`，会在 render tree、canonical model、H lowering、SFC template lowering 中保留节点键。
- Razor SDK / Razor IR authoring 支持 Razor `@key`。
- 对官方 Razor Source Generator 当前会把 component `@key="Id"` 编成 `AddComponentParameter(..., "@key", "Id")` 的形态，RazorVue 会基于原始 Razor 源片段与生成调用位次恢复 C# 表达式语义，确保 `<Child @key="Id" />` 仍然按属性访问降为 `props.id`，而不是错误地固定成字符串 `"Id"`。

## Runtime Naming Contract

- C# authoring surface 继续使用正常的 `PascalCase` 成员名，例如 `Title`、`IsDone`、`ModelValue`。
- 进入 Vue runtime/template 边界后，RazorVue 统一按 JavaScript/Vue 约定输出 `camelCase` 访问名，例如 `props.modelValue`、`item.title`、`item.isDone`、`context.isActive`。
- 该规则同时适用于：
  - 组件 props 的 `props.*` 访问
  - typed slot/scoped slot 的上下文对象成员
  - Razor IR / handwritten `BuildRenderTree` 两条 frontend 路线
- `script setup` SFC 输出统一保留：
  - `const __jazorRawProps = defineProps<...>();`
  - `const props = __jazorRawProps;`
- 当组件参数存在默认值代理时，`props` 可能升级为 `new Proxy(__jazorRawProps, ...)`，但 `__jazorRawProps` 仍是稳定底层绑定名。

## Template-Scoped Locals

- handwritten `BuildRenderTree` 现已支持模板作用域内的局部值缓存/别名声明，例如：
  - 顶层片段中的 `var localTitle = Title;`
  - `foreach` / `for` body 中基于迭代变量的 `var decorated = item + "!";`
  - typed slot template 中基于 slot 参数的 `var decorated = item + 1;`
- handwritten `BuildRenderTree` 现已支持“立即调用的 typed fragment 模板作用域”形态，例如：
  - `builder.AddContent(0, (RenderFragment<int>)(item => itemBuilder => { ... }), 42);`
  - `RenderFragment<int> template = item => itemBuilder => { ... }; builder.AddContent(0, template, 42);`
- handwritten `BuildRenderTree` 现已支持当前组件/本地 render helper 的“`RenderTreeBuilder` + 额外普通值参数”形态，例如：
  - `RenderBody(builder, Title);`
  - `private void RenderBody(RenderTreeBuilder builder, string? title) { ... }`
  - `void RenderBody(RenderTreeBuilder localBuilder, string? title) { ... }`
  - `RenderBody(builder, Title, Subtitle);`
  - `private void RenderBody(RenderTreeBuilder builder, string? title, string? subtitle) { ... }`
  - `RenderBody(title: Title, builder: builder);`
  - `RenderBody(title: Title, localBuilder: builder);`
  - `private void RenderBody(RenderTreeBuilder builder, string? title = "fallback-title") { ... }`
  - `void RenderBody(RenderTreeBuilder localBuilder, string? title = "fallback-title") { ... }`
- 该能力会在 render tree、canonical model、H lowering、SFC template lowering 中保留顺序作用域语义：局部变量只对声明之后的同一片段后续节点生效。
- 对于立即调用的 typed fragment，模板参数只在该 fragment body 内可见，不会泄漏到后续兄弟节点。
- 对于带额外值参数的 render helper，helper 参数只在 helper body 内可见；H lowering 会编码为一次性立即调用作用域，SFC lowering 会编码为局部 template scope wrapper，从而保留单次求值与参数不外泄语义。
- 当 helper 存在多个额外值参数时，该作用域会按调用点实参求值顺序嵌套保留；当前 contract 会稳定编码为嵌套 template scope / 嵌套 IIFE，而不是把多个参数扁平替换进 helper body。
- helper body 内也允许继续基于这些额外参数声明 template-scoped local cache/alias；该组合会保留为“外层 helper parameter scope + 内层 local declaration scope”而不是被错误内联或泄漏到 helper 外部。
- 当带额外值参数的 helper 在 `for` / `foreach` body 中被调用时，循环变量同样可以作为 helper 实参参与嵌套作用域绑定；该组合会继续保留为“外层 loop scope + 中层 helper parameter scope + 内层 local declaration scope”。
- 当前支持边界刻意收窄为“带初始化器的不可变模板局部声明”：
  - 必须在声明点提供 initializer
  - initializer 只能捕获当前可见的模板局部、slot/loop 参数或正常可编码表达式
  - 不支持声明后再赋值、递增/递减、嵌套匿名函数/委托承载的模板状态写入
- 对于 `AddContent(sequence, RenderFragment<T>, value)`，当前支持源码可分析的 typed fragment：可以是 inline anonymous-function fragment，也可以是同一可分析作用域内、初始化即为该匿名模板的局部 `RenderFragment<T>` carrier；仍不把任意 delegate 值、属性承载或动态 callable 形态放宽为模板执行。
- 同一条“源码可分析的局部 `RenderFragment<T>` carrier”规则也适用于组件 typed slot/template 参数，例如 `builder.AddAttribute(1, "ItemTemplate", template);`。
- 对于带额外值参数的 render helper，当前只支持：
  - 恰好一个 `RenderTreeBuilder` 参数
  - 其余参数均为普通按值参数
  - 同时适用于当前组件方法与 `BuildRenderTree` 内 local function helper
  - 调用点参数与 helper 声明一一对应；支持 named argument，也支持安全可投影的 omitted optional default value
  - 多个额外参数会按调用点实参求值顺序形成嵌套局部作用域，同时保持每个 helper 形参绑定到其正确实参；即使 named argument 打乱声明顺序，也不会退化成按声明顺序重排求值
  - helper body 必须源码可分析且自身形成可独立 canonicalize 的片段；不支持依赖调用方已打开节点/component frame 的 attribute/key/close 协议
- 对 SFC 输出，模板局部声明会编码为局部 template scope wrapper，而不是泄漏为顶层 `script setup` 公共绑定。

## Verification

```powershell
dotnet build src/Jazor.RazorVue/Jazor.RazorVue.csproj
dotnet test src/Jazor.RazorVue.Test/Jazor.RazorVue.Test.csproj
dotnet test src/Jazor.RazorVue.RazorIr.Test/Jazor.RazorVue.RazorIr.Test.csproj
```

## Read Next

- [../Jazor.Analyzer/README.md](../Jazor.Analyzer/README.md)
- [../Jazor.Emit/README.md](../Jazor.Emit/README.md)
- [../../docs/01-目标/razorvue/README.md](../../docs/01-目标/razorvue/README.md)
