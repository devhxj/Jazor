# Jazor.RazorVue

> Status: active reference
> Positioning: shared RazorVue semantic, Razor SDK bridge, and host protocol layer used by analyzer, emit, Jolt, and library-component packages.

`Jazor.RazorVue` 不再只是“库模式 lowering 项目”。在当前结构下，它承接整条 RazorVue lane 需要跨 `Jazor.Analyzer`、`Jazor.Emit`、`Jolt`、`ECMAScript.Vuetify` 共享的代码：核心语义、Razor SDK 桥接、artifact/catalog 模型，以及 RazorVue/Jolt 的宿主协议 DTO。

## Responsibilities

- 提供 RazorVue 入口分类、descriptor、render tree、canonical model、lowering 与 catalog。
- 提供 `RazorCodeDocument` / Razor IR 获取、文档定位与 template frontend 选择。
- 提供 legacy render artifact 与 design-time SFC artifact 的共享模型。
- 提供 `Documents/` 与 `Protocol/` 下的 RazorVue/Jolt 宿主协议 DTO。
- 产出目标是最终 `.vue` / render-function artifact；RazorVue 不是中间 wrapper JS 管线。

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
- Razor Source Generator semantic frontend 只接收带 `RazorSourceGeneratorDocument` 的 semantic snapshot；无 SG 文档的 helper component / partial snapshot 不能遮蔽真实 Razor IR canonicalization error。

## `@key` Support

- RazorVue 现已将 vnode `key` 作为一等语义处理，不会把它退化成普通 HTML / component attribute。
- 手写 `BuildRenderTree` authoring 支持 `RenderTreeBuilder.SetKey(...)`，会在 render tree、canonical model、H lowering、SFC template lowering 中保留节点键。
- Razor SDK / Razor IR authoring 支持 Razor `@key`。
- 对官方 Razor Source Generator 当前会把 component `@key="Id"` 编成 `AddComponentParameter(..., "@key", "Id")` 的形态，RazorVue 会基于原始 Razor 源片段与生成调用位次恢复 C# 表达式语义，确保 `<Child @key="Id" />` 仍然按属性访问降为 `props.id`，而不是错误地固定成字符串 `"Id"`。

## DOM Event Attribute Support

- HTML element 上的 Blazor DOM event attribute 现在作为一等 HTML event 处理，而不是退化成普通 `onclick` 字符串属性。
- handwritten `BuildRenderTree` 支持 `builder.AddAttribute(..., "onclick", EventCallback/delegate)`，会 lower 为 Vue event prop/template event：
  - H/render-function 输出使用 `onClick`
  - SFC template 输出使用 `@click`
- Razor IR / `.razor` authored template 也支持 HTML element DOM event directive，例如 `<button @onclick="OnClick" @onclick:preventDefault="true" @onclick:stopPropagation="StopClick">`；frontend 会从 Razor SDK 生成的 `AddAttribute(...)` / `AddEvent...Attribute(...)` 调用或 raw markup fallback 中恢复 Roslyn `IOperation`，再交给既有 EventCallback / method-reference lowering，而不是在 RazorVue 内拼接 handler JS。
- 当 Razor SDK 把 `.razor` event directive 保留在 raw `AddMarkupContent(...)` 中时，RazorVue 会用当前组件 partial probe 重新绑定表达式，并且 downstream setup/member 分析使用该 `IOperation` 自带的 `SemanticModel.Compilation`；这保证 inline lambda handler 里的 CLR 类型、当前组件 member 引用、`Count++` 这类写入语义仍走 `Jazor.Compiler` / `SemanticWalker`，不会混用原 snapshot compilation 或手写 JS。
- `WebRenderTreeBuilderExtensions.AddEventPreventDefaultAttribute(...)` 与 `AddEventStopPropagationAttribute(...)` 已在 declarative 和 imperative render path 中进入正式支持。
- 静态 `true` 修饰符在 SFC template 中会优先输出 Vue 原生 modifier，例如 `@click.prevent.stop`；静态 `false` 不输出 modifier，并且在 handwritten `BuildRenderTree` 中遵循“最后一次调用胜出”，可清除前面设置过的 modifier。
- 动态 bool modifier 不会在 `script setup` 初始化时冻结值；SFC 会在事件 handler 内读取当前表达式，例如 `props.preventClick` 或 template-local `localPrevent`，H/render-function 则通过稳定 wrapper 保留事件触发时的条件判断。
- imperative render bridge 支持 `setEventModifier(...)` / `setEventModifiers(...)`，会规范化 Blazor event key（例如 `onclick` -> `onClick`），包装 handler，并在 modifier 被清除时还原到原始 handler，避免重复 wrapper 或残留旧 modifier。
- 该能力只覆盖 HTML DOM event。组件 emits 仍沿 descriptor-aware component event 路线处理，不与 DOM event modifier 合并。

## Mixed Attribute Support

- Razor IR frontend 现已支持 mixed HTML attribute content，不再只接受“单个 attribute value child”或“全部静态 literal child”。
- 当前已覆盖的 authoring 形态包括：
  - `class="todo-card @Title"`
  - `class="todo-card @(Title?.Trim() ?? "untitled")"`
  - 其他按 Razor IR 拆成“静态 literal child + C# expression/code child”的同类 attribute
- 这类 mixed attribute 会在前端重建为真实 C# 运行时表达式，再交给 Roslyn 解析；因此 downstream canonical model / H lowering / SFC lowering 会继续看到正常的 `IBinaryOperation`、条件/null 合并等语义，而不是被退化成字符串拼接文本。
- 当表达式语义要求单次求值保护时，最终 JS lowering 允许输出 IIFE / 临时变量，而不是强制追求最短文本形式。
- 仍保留显式失败边界：如果某个 mixed attribute child 既不是可证明静态 literal，也不是可重建的 Razor expression/code 节点，frontend 会直接报 unsupported，而不会静默生成不可靠代码。

## Static Markup Support

- handwritten `BuildRenderTree` 现已支持静态 markup subtree，只要内容可安全解析为静态 HTML subtree。
- 当前已覆盖的 authoring 形态包括：
  - `builder.AddMarkupContent(0, "<section class=\"hero\"><span>safe</span><p>ok</p></section>");`
  - `const string markup = "<section class=\"hero\"><span>safe</span><p>ok</p></section>"; builder.AddMarkupContent(0, markup);`
  - `string markup = "<section class=\"hero\"><span>safe</span><p>ok</p></section>"; builder.AddMarkupContent(0, markup);`
  - `string markup; markup = "<section class=\"hero\"><span>safe</span><p>ok</p></section>"; builder.AddMarkupContent(0, markup);`
  - 只读 expression-bodied property / getter-only property / `readonly` field 承载同类静态 string markup，再由 `AddMarkupContent(...)` 消费
  - private settable property / private 非 `readonly` field 承载同类静态 string markup，只要源码中可证明不存在后续写入，也可由 `AddMarkupContent(...)` 消费
  - `builder.AddContent(0, (MarkupString)"<section class=\"hero\"><span>safe</span><p>ok</p></section>");`
  - `builder.AddContent(0, new MarkupString("<section class=\"hero\"><span>safe</span><p>ok</p></section>"));`
  - `MarkupString markup = (MarkupString)"<section class=\"hero\"><span>safe</span><p>ok</p></section>"; builder.AddContent(0, markup);`
  - `MarkupString markup; markup = (MarkupString)"<section class=\"hero\"><span>safe</span><p>ok</p></section>"; builder.AddContent(0, markup);`
  - 只读 expression-bodied property / getter-only property / `readonly` field 承载同类静态 `MarkupString`，再由 `AddContent(...)` 消费
  - private settable property / private 非 `readonly` field 承载同类静态 `MarkupString`，只要源码中可证明不存在后续写入，也可由 `AddContent(...)` 消费
  - Razor authored 模板表达式中的同类静态 `MarkupString`：`@((MarkupString)"<section ...>...</section>")`、`@(new MarkupString("<section ...>...</section>"))`、以及局部/受控成员 carrier `@markup` / `@HeroMarkup`
  - imperative body 内与上述等价的静态 `MarkupString` direct/carrier 形态，也会直接 lower 为静态 `h(...)` subtree，而不是残留 `MarkupString` authoring 语义
  - imperative body 内与上述等价的静态 `AddMarkupContent(...)` string direct/carrier 形态，也会直接 lower 为静态 `h(...)` subtree，而不是残留 raw markup helper 语义
  - 含标准静态 attribute、嵌套 element、void element、自闭合 element、普通文本与 HTML comment 的同类静态片段
- 该能力在 `BuildRenderTree` frontend 与 Razor IR frontend 之间复用了同一个静态标记解析器，因此两条路径对静态 HTML 片段的 element/text/attribute 还原语义保持一致。
- `AddMarkupContent(...)`、`AddContent(..., MarkupString)` 与 Razor 模板中的 `@MarkupStringExpression` 仍刻意收窄为“编译期可证明为静态 HTML”的子集；允许的 string/`MarkupString` carrier 只限源码可分析、静态可追溯的局部、源码可分析的 current-component / local function static-markup factory 返回值，以及只读成员或“private mutable + 可证明无后续写入”的受控成员形态。对 handwritten `BuildRenderTree` 而言，静态 `AddMarkupContent(...)` 的 string local 现已正式覆盖 `const`、普通 declaration initializer，以及“先声明、再在同一线性局部声明前缀内完成一次简单赋值”的 source-stable 窄模式；`MarkupString` local 也在 handwritten `BuildRenderTree` 与 Razor IR authored template 两条路径上支持同一 declaration-prefix 合同，并允许在赋值前穿过 sibling local declarations。`CreateMarkup()`、`CreateMarkup(Title)`、省略 optional 参数的 `CreateMarkup()`，以及 `CreateMarkup(Title, "suffix")` 这类按 Roslyn 绑定为单数组形参的 `params` 调用，只要返回值本身仍可还原为静态 string / `MarkupString`，现在都可直接被上述静态-markup 消费点接受，并贯通 declarative / imperative / pipeline / SFC 主线。对带普通按值参数、omitted optional default 或 `params` 单数组绑定的 factory，RazorVue 都会保留调用点实参的左到右求值顺序，并把实参与形参绑定为 captured scope，再包裹最终静态 subtree，而不是通过“看起来是静态返回值”把调用直接擦掉。一旦这些 local 或 factory-backed carrier 后续依赖更宽 dataflow、重赋值、`ref/out`/`in`、实参与形参无法按当前合同绑定、或其他不可静态证明的变异，就会显式拒绝，而不是沿第一次赋值静默恢复旧静态 subtree。动态 markup、运行时拼接 markup、运行时构造的 `MarkupString` 或需要执行脚本语义的 raw HTML 当前仍不支持。
- imperative render / setup lowering 里引用同文件 helper class 时，不再走 carrier wrapper 或手写 JS 拼装；同模块同步 helper 由 `AstConverter.ConvertRuntimeClass(...)` 直出为 class declaration，再由 RazorVue module context 统一收集 helper 依赖、CLR/compiler import 绑定和模块内声明名，最后插入到 Vue/compiler imports 之后、`export default defineComponent(...)` 之前。该支持面覆盖组件内嵌 helper class、只通过 `typeof(...)` / 静态成员使用的 static helper class、helper class 递归创建其他同模块 helper class，以及 helper method 内的 CLR whitelist `Import` 调用；带 `[ECMAScript]` / `[ECMAScriptModule]` 标记的 host/component type 不会被当作 helper class 扁平化。
- 上述静态-markup 解析链现在也明确覆盖“factory-backed member/local carrier 再消费”的组合形态：例如 `private string HeroMarkup => CreateMarkup(); builder.AddMarkupContent(..., HeroMarkup);`、`MarkupString markup; markup = CreateMarkup(); builder.AddContent(..., markup);`，以及 `builder.AddMarkupContent(..., CreateMarkup(Title));` / `builder.AddContent(..., CreateMarkup(Title));` 这类带普通按值参数但返回静态 markup 的 factory 消费，都会沿同一 source-stable/static-factory 解析链继续追踪到最终静态 markup 值，并在需要时保留调用点 captured-binding scope 后再贯通 render tree / pipeline / SFC / imperative bridge；不会在 property/local 这一层错误丢失 factory 返回值解析上下文，也不会把有参数的静态 factory 错误退回 unsupported。imperative bridge 中的 `MarkupString` local 仍是值 carrier：声明/赋值语句继续通过 `SemanticWalker` 的 host rewrite 发射为静态 markup 值，不会被伪装成 `() => h(...)` thunk，也不会把 static-markup factory 错误纳入 setup helper dependency graph。

## Razor IR Template Locals

- Razor IR frontend 现已支持模板内局部 `@{ ... }` code-block 的受控生产切片：带初始化器的不可变局部声明，以及“声明后在同一线性局部声明前缀内完成一次简单赋值”的等价不可变局部声明，再加上 Razor IR boundary code node 驱动的顺序控制语句恢复。
- 当前已覆盖的 authoring 形态包括：
  - `@{ var localTitle = Title; }`
  - `@{ string? localTitle; localTitle = Title; }`
  - `@{ var decorated = item + "!"; }` 这类 loop body 内局部缓存/别名
  - `<ItemTemplate Context="item">@{ var decorated = item + "!"; } <p>@decorated</p></ItemTemplate>` 这类 typed child-content/slot body 内局部缓存/别名
  - `@{ RenderFragment<string> template = item => @<p>@item</p>; } <LayoutCard ItemTemplate="template" />` 这类 Razor IR template code-block 内局部 typed `RenderFragment<T>` carrier 赋给组件 typed slot/template 参数
  - `@{ RenderFragment<string> template = item => @<p>@item</p>; if (Show) { <section>tail</section> } } <LayoutCard ItemTemplate="template" />`
  - `@{ RenderFragment<string> template = item => @<p>@item</p>; foreach (var tag in Tags!) { <section>@tag</section> } } <LayoutCard ItemTemplate="template" />`
  - `@{ RenderFragment<string> template = item => @<p>@item</p>; for (var i = 0; i < Count; i++) { <section>@i</section> } } <LayoutCard ItemTemplate="template" />`
  - `@{ var localTitle = Title; if (Show) { <section>@localTitle</section> } }`
  - `@{ var localTitle = Title; if (Show) { <section>@localTitle</section> } else { <p>hidden</p> } }`
  - `@{ var localTitle = Title; if (ShowPrimary) { <section>@localTitle</section> } if (ShowSecondary) { <p>secondary</p> } }`
  - `@{ var prefix = Title; if (ShowPrimary) { <section>@prefix</section> } foreach (var item in Items!) { <p>@prefix @item</p> } }`
  - `@{ var prefix = Title; foreach (var item in Items!) { <p>@prefix @item</p> } if (ShowTail) { <section>@prefix</section> } }`
  - `@{ var prefix = Title; foreach (var item in Items!) { <p>@prefix @item</p> } }`
  - `@{ var prefix = Title; for (var i = 0; i < Count; i++) { <p>@prefix @i</p> } if (ShowTail) { <section>@prefix</section> } }`
  - `@{ var prefix = Title; for (var i = 0; i < Count; i++) { <p>@prefix @i</p> } }`
- 这类 Razor code-block 会被还原为现有的 `RazorVueLocalDeclarationNode`，因此 downstream canonical model / H lowering / SFC lowering 会沿用 handwritten `BuildRenderTree` 已有的 template-scoped local 语义，而不是走另一套特殊分支。
- 对于 Razor IR template code-block 内部的局部 `RenderFragment` / `RenderFragment<T>` carrier，frontend 现在会按 handwritten `BuildRenderTree` 既有 contract 直接吸收为后续 `AddContent` / 组件 slot 参数可消费的结构化 template carrier，而不是把该 carrier 继续保留为根级 `RazorVueLocalDeclarationNode`。
- 当前该局部 carrier 不仅支持“初始化器本身就是 inline Razor template”，也支持“先声明、再在同一线性局部声明前缀内完成一次简单赋值”的 source-stable 窄模式；这条 declaration-prefix 路线现在已正式覆盖 inline Razor template、本组件受支持 `RenderFragment` member carrier，以及受支持 fragment factory 调用结果。额外捕获值会继续被保留为外层 template scope，而不会被错误扁平化。
- 这条局部 `RenderFragment` / `RenderFragment<T>` carrier 现在也能跨过后续 imperative tail：例如 local carrier 后接 `while` / conditional `return`，再把同一 carrier 赋给组件 typed slot/template 参数时，frontend 会把需要命令式执行的片段提升为 `RazorVueImperativeBlockNode`，同时保留 carrier/local 可见性与后续 sibling 的真实求值顺序。对“先声明、再立即赋值”的 carrier，如果 tail 后仍有 local 读取，segment planner 会把声明前缀并入同一 imperative render segment，并通过 `SemanticWalkerHost.RewriteSimpleAssignmentPreorder` 把简单赋值交给既有 slot-factory lowering，而不是在 RazorVue 内拼接一套私有 JS delegate 协议。
- Razor IR template code-block 里的 local function fragment factory 声明现在也已并入同一局部 carrier 合同；例如 `@{ RenderFragment<int> template = CreateTemplate(Title); RenderFragment<int> CreateTemplate(string? title) => item => @<span>@title @item</span>; }` 会像当前组件 `@code` factory 一样保留 `title` captured scope 与内层 `item` typed scope，而不会把 local function 自身的 `@<...>` 模板体泄漏成根级 render node。
- 对 untyped `RenderFragment` 而言，Razor IR frontend 现在也支持 direct expression consumption：`@Template`、`@template` 这类当前组件 member / source-stable local carrier 直接出现在 Razor 表达式位时，会还原为结构化 render subtree，而不是退化成普通表达式节点、重复输出模板体，或误落入 imperative tail。
- 这条 untyped direct expression 路径同样覆盖“直接调用 fragment factory 并立即消费返回值”的 authored 语法：既支持当前组件 `@code` / member method，也支持 template code-block 内 local function factory；例如 `@CreateTemplate(Title)`、`@CreateTemplate()`、`@CreateTemplate(subtitle: Subtitle, title: Title)`，以及 `@{ RenderFragment CreateTemplate(string? title) => @<section><span>@title</span><p>ok</p></section>; } @CreateTemplate(Title)`。factory 的普通 captured 参数会继续保留为外层 scope，再在其内层直接物化最终 render subtree，而不会退化成普通 invocation expression；named argument out-of-order 也会按调用点求值顺序保留外层 scope 包裹顺序。
- 对 typed `RenderFragment<T>` 而言，Razor IR frontend 现在也支持 direct invocation consumption：`@Template(42)`、`@template(42)` 这类当前组件 member / source-stable local carrier 直接出现在 Razor 表达式位时，会还原为结构化 `RazorVueTemplateScopeNode`，并继续保留外层 captured-value scope，而不是退化成普通 invocation 表达式或在后续 canonical/SFC 阶段触发 unsupported member/property 失败。
- 这条 typed direct invocation 路径同样覆盖“直接调用 fragment factory 再立即消费返回值”的 authored 语法：既支持当前组件 `@code` / member method，也支持 template code-block 内 local function factory；例如 `@CreateTemplate(Title)(42)`、`@CreateTemplate()(42)`、`@CreateTemplate(subtitle: Subtitle, title: Title)(42)`，以及 `@{ RenderFragment<int> CreateTemplate(string? title) => item => @<span>@title @item</span>; } @CreateTemplate(Title)(42)`。factory 的普通 captured 参数仍会保留为外层 scope，typed slot context 参数则继续落到内层 `RazorVueTemplateScopeNode`，named argument out-of-order 也会按调用点求值顺序保留外层 scope 包裹顺序。
- 对 typed slot outlet 而言，Razor authored direct invocation 现在也已对齐 handwritten `BuildRenderTree` 语义：`@Header(Count + 1)` 这类当前组件 `[Parameter] RenderFragment<T>?` slot source 会直接还原为带 argument 的 `RazorVueSlotOutletNode`，最终 lower 为 `<slot name="header" :value="(props.count + 1)" />`，而不会退化成普通插值表达式。
- 对 typed `RenderFragment<T>` / typed slot 而言，slot context 参数仍保留在 slot/template 自身的 `ParameterName` / `ParameterSymbol` 上；只有 factory/member carrier 额外捕获了普通值参数或当前组件值时，才会在模板 children 外层再包裹 `RazorVueTemplateScopeNode`。如果 carrier 本身只是 `item => ...` 这类直接 typed template，则 children 会直接是结构化 element/expression 节点，而不会平白新增一层 scope。
- typed slot/template-local 的“先声明、再在同一线性局部声明前缀内完成一次简单赋值”窄模式现在也已在 Razor IR 路线正式锁定；例如 `string? decorated; decorated = item;` 与 `string? decorated; var revision = 0; decorated = item;` 都会和声明点初始化一样被还原为稳定的 template-scoped local，而不会退化成一般赋值语句执行模型。
- typed child-content / typed slot template body 中，纯 imperative code-block 也已开始按正式通道接入；像 `@{ while (Show) { <p>@item</p>; break; } }`、`@{ do { <p>@item</p>; break; } while (Show); }`、`@{ foreach (var value in Items!) { if (value < 0) { break; } <p>@item @value</p> } }`、`@{ foreach (var value in Items!) { if (value == SkipValue) { continue; } <p>@item @value</p> } }`、`@{ for (var index = 0; index < Count; index++) { if (index >= StopIndex) { break; } <p>@item @index</p> } }`、`@{ for (var index = 0; index < Count; index++) { if (index == SkipIndex) { continue; } <p>@item @index</p> } }`、`@{ using (CreateDisposable()) { <section>@item</section> } }`、`@{ lock (_gate) { <section>@item</section> } }` 这类“没有任何前置局部声明”的 standalone imperative body，不再因为缺少 local 前缀而卡在 `unbound template CSharpCodeIntermediateNode`，而是直接提升为 `RazorVueImperativeBlockNode` 并复用现有 imperative render bridge。direct Razor control-block 形态也对齐进入这条路线；例如 typed slot body 内 `@while (...) { ... }` 会 lower 成 scoped slot callback 里的 imperative render IIFE，`@if (Hide) { return; } <p>@item</p>` 会保留提前返回对 tail markup 的可见性。
- 这条“模板 code-block 结构化恢复”路线只覆盖声明式模板子集。对于更复杂的 block code，RazorVue 后续会按 `docs/01-目标/razorvue/design/RazorVue.BlockCode.ExecutionModel.md` 收敛为正式的命令式渲染通道，而不是继续无上限扩张前端特判矩阵。
- 当前支持边界刻意收窄为“局部声明优先、且只进入受支持的顺序控制语句”的模板 code-block：
  - 每个局部要么在声明点提供 initializer，要么严格匹配“声明后在同一线性局部声明前缀内完成一次简单赋值”
  - initializer 只能捕获当前可见的 template local、loop local、typed slot context parameter 或合法模板表达式
  - 支持局部声明后进入 `if` / `if-else` / `foreach` / count-style `for`
  - 支持 Razor IR 把 `}` 与下一个 `if` / `foreach` / `for` header 线性化到同一 `CSharpCodeIntermediateNode` 时的顺序恢复，包括 `if -> if`、`if -> foreach`、`foreach -> if`、`for -> if`
  - 支持一个更窄的 Razor IR local `RenderFragment` carrier 子集：既可以是声明点初始化，也可以严格匹配“声明后在同一线性局部声明前缀内完成一次简单赋值”；允许在这次赋值前继续出现 sibling local declarations。初始化器只能是 inline Razor template、当前组件受支持 `RenderFragment` member carrier，或受支持 fragment factory 调用结果。若最终模板体对应 Razor SDK 暴露的 `TemplateIntermediateNode`，该 carrier 之后同一 code-block 仍可继续进入受支持的 `if` / `foreach` / `for` 顺序控制；若该 local 后续再次出现可观察写入，则会按同一 source-stable 合同显式 fail-fast
- 在 typed child-content / typed slot template body 中，局部声明后的 `while` / `do-while` / 带 `break` / `continue` 的 `for` / `foreach` / `switch` / `try-catch-finally` / `using` / `using declaration` / `lock` 以及需要 method/local imperative tail 语义的 `return` / `throw` / mutation，现已不再走声明式结构化节点扩张，而是稳定提升为 `RazorVueImperativeBlockNode` 并进入现有 imperative render 主线；standalone body 也已对齐覆盖 `foreach` 的 `break` / `continue` 与 `for` 的 `break` / `continue`；该路径与 handwritten `BuildRenderTree` imperative bridge 共享同一 lowering/runtime contract
- 这条 imperative template-body 支持现在不再要求“必须先出现 template local 声明”才能命中；如果 typed slot/template body 的 `@{ ... }` 本身就是纯 imperative 语句块，frontend 也会直接接入同一 imperative render 主线。
- 对 typed slot/template body 而言，一旦局部声明后的后续片段需要 imperative tail 语义，frontend 会把“该命中点到同一 slot body 尾部”的剩余节点统一收进同一个 imperative tail，而不是切成“局部 imperative + 后续再回 declarative sibling”的混合片段；这样才能正确保留 `return` / `throw` / mutation 对后续节点可见性的真实模板语义
- 对 Razor authored root template `@{ ... }` 而言，这条语义再收敛一步：若 code-block 同时包含 template local 声明与后续 imperative statement，并且后面还存在普通 root sibling，例如 `@{ var localTitle = Title; _count++; } <section>@localTitle @_count</section>`、`@{ var localTitle = Title; if (Hide) { return; } } <section>@localTitle</section>`、或 `@{ var localTitle = Title; var index = 0; while (index < Count) { <section>@localTitle @index</section>; index++; } } <footer>@localTitle @index</footer>`，当前实现都会把“local + imperative + 后续 sibling”整体提升为同一个 imperative render block / render-function `.vue` 产物，而不是像 typed slot/template body 那样保留“前缀 local declaration node + 尾部 imperative block”分裂结构；这样可以更稳地保留 root 级求值顺序与 local 可见性。
- 当前这一声明式结构化通道不支持局部声明后进入更一般的语句执行模型；除“声明后在同一线性局部声明前缀内完成一次简单赋值”外，其他赋值语句、递增/递减、delegate/callable template state、`switch` / `while` / `do-while` / `try-catch` / `using` / `lock` 仍不走此通道
- 上述更一般的 block code 不再被视为长期只能 fail-fast 的永久边界；其中 `while` / `do-while` / `switch` / `lock` / `try-catch/finally` / `using` / `using declaration` 已进入正式命令式渲染通道，其余语句族将继续沿这条通道扩面，而不是继续把它们塞回 `RazorVueConditionalNode` / `RazorVueForEachNode` / `RazorVueForNode`

## Lifecycle Support

- RazorVue 的 lifecycle/setup lowering 仍是受控子集，但当前已正式支持一条更完整的 `ShouldRender` 透传链契约。
- `ShouldRender` 当前接受的安全形态包括：
  - `return true;`
  - `return base.ShouldRender();`，当该 `base` 最终解析到 `ComponentBase.ShouldRender()` 时
  - `return base.ShouldRender();`，当该 `base` 最终递归解析到另一个同样受支持的 base `ShouldRender` 实现时，例如“派生类透传 -> 抽象基类 `return true;`”
- 这条支持是递归受控的，而不是“只要写了 `base.ShouldRender()` 就接受”：
  - 如果 base 链上的最终实现仍是动态条件（例如 `return Value > 0;`），RazorVue 仍会把整条链判为 unsupported，并继续落到 `FullReloadRequired`
  - 若 base 链形成循环、缺少源码、或方法体形状超出上述受控子集，也会继续显式失败
- 普通 lifecycle 的 base-pass-through 现在也对齐接受一个更窄的尾随 no-op 形态：例如 `await base.OnInitializedAsync(); return;` 这类“base 透传后只跟空返回”的方法体，会与纯 pass-through 一样被视为没有新增运行时行为，不再误退回 unsupported。
- lifecycle / no-op contract 本轮进一步按真实返回类型收紧并对齐：
  - `Task` 返回的 lifecycle 只接受真实 completed-task no-op，例如 `Task.CompletedTask`
  - non-generic `ValueTask` 返回的 lifecycle 额外接受 `default` / `default(ValueTask)` / `new ValueTask(...)` 包裹后的等价 no-op
  - `protected override Task OnInitializedAsync() => default;` 现在会在 analyzer / lowering / generator 三层一致视为 unsupported，因为 `default(Task)` 实际是 `null`，不是 no-op
- lifecycle payload 当前也已补齐一条更真实但仍受控的 setup 依赖路径：
  - `[Parameter]` property 仍可直接进入 payload lowering
  - current-component setup value member 现在也可作为 payload 原子参与 lowering，包括 getter-bodied property、source-stable declaration-initialized value-like property/field，以及 private mutable setup carrier auto-property/field
  - 这些 member 仍不是在 lifecycle lowering 内手拼 JS；RazorVue 会把它们先纳入同一 setup/property/field lowering 主线，再在 payload 里引用最终 setup binding / setup function
- 这条 lifecycle payload 扩面当前刻意只开放受控 current-component member 子集：
  - getter-bodied property 只接受 expression-bodied property、getter accessor 中单个 `return` 的 property，以及同一受控子集内的 getter 链
  - source-stable value member 仍只接受可静态还原 initializer 的只读/getter-only 形态；private mutable setup carrier 则必须是 private field 或 private-setter auto-property，会按 component setup `let` carrier 处理，允许后续写入存在
  - payload 的直接快路径仍只接受当前既有安全组合：literal / `null` / `firstRender` / unary / binary / conditional / interpolated string，以及由这些受支持原子拼成的表达式；超出该快路径但仍可由 `SemanticWalker` 诚实 lower 的受控形态，会进入下述 compiler-owned fallback，而不是在 RazorVue 内另写一套表达式语义
- lifecycle payload 当前也已补齐一条受控 current-component helper/method-call 路径：
  - 仅限当前组件内 helper/method
  - 调用点参数必须能按 Roslyn 绑定结果稳定落位；普通按值参数、named argument out-of-order、omitted optional default、以及 `params` 单数组形参均已支持，`ref` / `out` / `in` 仍显式 fail-fast
  - helper 本身必须继续满足 setup helper lowering 合同：同步、非 `Task`/`ValueTask` 返回、源码可分析；expression-bodied / single-return helper 继续走 expression lowering，普通 block-bodied helper body 走 `SemanticWalker.TranslateStatementSequence(...)` 的 compiler-owned statement lowering
  - helper body 中对 declaration-initialized property / field、getter-bodied property、以及其他同步 helper 的依赖，继续沿同一 setup/property/field/method lowering 主线递归展开
- `OnAfterRender*` 的 `firstRender` payload 现还补齐了一条 compiler-owned fallback：
  - 当 payload 实际引用 lifecycle `firstRender` 参数、且表达式形状仍落在当前受控子集内时，RazorVue 会把 `firstRender` 通过 `currentFirstRender` 别名交回 `EmitSetupExpression -> SemanticWalker -> Jazor.Compiler`
  - 这里不是在 RazorVue 内部继续手拼 CLR/成员/调用语义；RazorVue 只负责 after-render snapshot 协议与参数别名，具体表达式翻译仍归 `Jazor.Compiler`
  - 当前已锁定进入这条 fallback 的真实形态包括 `(bool)firstRender`、`object.Equals(firstRender, true)`、`object.Equals((bool)firstRender, true)`、`firstRender.Equals(true)`、`firstRender == true`、`bool? alias = firstRender; alias ?? false` 这一类 source-stable nullable-bool local carrier、`firstRender is true/false`、`firstRender is not true/false`、`firstRender is true or false`、`firstRender is true and not false`、`firstRender is bool`、`firstRender is object`、直接 against `firstRender` 的 declaration-pattern（例如 `firstRender is bool ready && ready`）、`firstRender switch { ... }`，以及继续满足 setup helper 合同的受控 helper-call payload（例如 `Normalize(firstRender)`）
  - 当前已锁定的典型发射结果包括：`object.Equals(firstRender, true)` / `object.Equals((bool)firstRender, true)` / `firstRender.Equals(true)` -> `currentFirstRender === true`，`firstRender == true` -> `(currentFirstRender === true)`，`alias ?? false` -> `currentFirstRender ?? false`，`firstRender is true` -> `currentFirstRender === true`，`firstRender is false` -> `currentFirstRender === false`，`firstRender is bool` -> `typeof currentFirstRender === "boolean"`，`firstRender is bool ready && ready` -> compiler-owned declaration-pattern lowering（包含 pattern local `ready` 的绑定与复用）
- lifecycle payload 的 compiler-owned fallback 现也不再局限于 `OnAfterRender*` / `firstRender`：普通 lifecycle 中的 source-stable local、local function 与 callable local payload 会先发射稳定 prelude alias，再把最终 payload 交回 `EmitSetupExpression -> SemanticWalker -> Jazor.Compiler`。例如 `var label = Prefix + Value; ValueChanged.InvokeAsync(label + "!")`、`string FormatLabel(int value) => "Count: " + value; ValueChanged.InvokeAsync(FormatLabel(Value))`、以及 `Func<int, int> increment = static value => value + 1; ValueChanged.InvokeAsync(increment(Value))` 会在 `onMounted(...)`、`watch(..., { immediate: true })` 等 hook body 内保留 local 单次求值与参数绑定，而不是在 RazorVue 内手拼 payload 表达式。
- 普通 lifecycle 中的局部名 `firstRender` 只是普通 source-stable local；`var firstRender = 1; ValueChanged.InvokeAsync(firstRender);` 在 `OnInitialized*` / `OnParametersSet*` 中不会触发 after-render `currentFirstRender` snapshot 协议。
- 普通 lifecycle 的 current-component helper / local function payload 参数绑定也复用同一 shared invocation binder：omitted optional default、按 Roslyn 绑定成单数组形参的 `params`、以及 named argument out-of-order 都会保持调用点左到右求值顺序，再按形参声明顺序落位。`params` 不被改造成 JavaScript rest/spread 协议；RazorVue 保留 Roslyn 已绑定的数组表达式，并交由 `SemanticWalker` lower。
- 对 `OnAfterRender*`，`firstRender` 使用检测会递归进入 source-stable local initializer、local function body 与 callable local initializer；local function 闭包捕获 `firstRender` 时仍会生成 `currentFirstRender` snapshot，并保持 async hook 中“先快照、再翻转”的协议。
- 这条 helper payload 扩面依然不是“任意执行模型放开”：
  - `async` helper、`Task` / `ValueTask` 返回 helper、`ref` / `out` / `in` 参数、越出当前 setup lowering / `SemanticWalker` 合同的 helper body、以及更宽的动态 member/dataflow 仍显式报 `UnsupportedLifecycleLowering` / analyzer `JAZORVUE005`
  - 一般外部 invocation、未知实例 method-call payload，也没有因为这轮扩面而被放宽
  - 同一个 lifecycle body 内重复 emit、额外 mutation、控制流或更一般的多语句执行模型仍不属于这条 payload fallback；这些场景需要走明确设计的 lifecycle imperative contract，当前继续 fail-fast
  - deeper object-construction/member-chain、property-pattern、依赖额外 source-stable object boxing/local carrier 的 declaration-pattern / pattern-var、null-conditional + coalesced 组合，以及更宽 dataflow 形状仍只在现有 compiler-owned lowering 已能诚实承载的受控路径内开放；这里不是把 lifecycle payload 放宽成任意执行模型
- module builder / SFC builder 现在也已对 lifecycle 发射顺序做了正式收口：
  - 先创建 lifecycle plan
  - 在 plan 创建阶段就预收集 lifecycle payload 触发的 setup property/field/method 依赖
  - 先发射这些 setup bindings/functions
  - 最后再发射 `watch(..., { immediate: true })`、`onMounted`、`onUpdated`、`onUnmounted` 等 lifecycle hooks
- 这条顺序不是代码风格调整，而是生产级语义要求：`OnParametersSet*` / `SetParametersAsync` 会 lower 到 `watch(..., { immediate: true })`，若 setup binding 晚于 watch 注册发射，会直接形成 TDZ / 初始化顺序风险。当前 pipeline / SFC artifact 都已锁定“setup 先于 immediate watch”这一合同。
- analyzer / lowering / generator 现也已对齐到同一 lifecycle 支持矩阵：analyzer 不再用旧的语法级近似规则猜测 lifecycle payload 是否可支持，而是基于同一 semantic snapshot 复用 lowering 侧的 support-shape 判定；因此 declaration-initialized property/field lifecycle payload 不会再出现“pipeline 接受但 analyzer 仍报 `JAZORVUE005`”的漂移。
- setup helper lowering 目前也已补齐“同步多级 helper 组合”和“普通 block-bodied helper”的真实支持面：只要 helper 仍是当前组件内、源码可分析、同步、且每一层都满足现有 setup lowering 合同，就不再受旧的 2 层深度人工限制；例如 `FormatOuter -> FormatMiddle -> FormatInner` 这类三层及以上同步 helper 链会继续递归收集并 lower 到同一 setup scope。block-bodied helper 不在 RazorVue 内手拼 JS statement，而是由 `RazorVueExpressionEmitter.EmitSetupStatementSequence(...)` 交给 `SemanticWalker.TranslateStatementSequence(...)` 完成 CLR-aware statement lowering、依赖收集和 import/reference 语义。
- setup-side logic 当前还正式补齐了一条 getter property 支持面：当前组件内、源码可分析、getter body 可收敛到“单表达式 / 单返回”的 property，现在可以像同步 helper 一样进入同一 setup scope；helper body 引用这类 property 时，RazorVue 会先把 property 发射为 setup function，再让后续表达式继续通过 `Jazor.Compiler` / `SemanticWalker` 完成 CLR-aware lowering，而不是在 RazorVue 内部手拼 JS 语义。
- setup-side logic 现也补齐了 value-like property / field 的受控支持面：当前组件内、声明点初始化且源码可证明保持 source-stable 的只读 property/field 会直接发射为 setup `const` binding；private mutable field 或 private-setter auto-property 会发射为 setup `let` carrier，即使源码中存在后续写入，也可被模板表达式、setup helper、lifecycle payload 与 imperative render body 引用。所有 initializer / helper body / render body 表达式仍继续通过 `RazorVueExpressionEmitter` / `SemanticWalker` / `Jazor.Compiler` 保持 CLR-aware lowering，而不是在 RazorVue 内部另写一套值替换逻辑。
- 这条 property 支持当前刻意只开放 getter-bodied 受控子集：
  - expression-bodied property
  - getter accessor 里单个 `return` 的 property
  - getter 间链式依赖，只要整条链最终仍落在同一受控子集内
- value-like property / field 当前开放两个不同合同，不能混淆：
  - getter-only auto-property 或 declaration initializer property
  - `readonly` field 或可证明 source-stable 的 declaration initializer member 会作为稳定 `const` binding
  - private mutable field / private-setter auto-property 会作为 setup `let` carrier，允许 later writes；没有 initializer 时会按 CLR 默认值发射，存在 initializer 但无法 lowering 时会 fail-fast
  - direct template expression、setup helper、lifecycle payload 与 imperative render body 引用走同一 setup binding 合同，不分裂成多套语义
- 这条扩面同样保持 fail-fast：
  - property 链一旦出现循环依赖，会在编译期直接报 `UnsupportedSetupLogicLowering`
  - setter 不是 private 的 mutable property、带自定义 getter/setter 的 mutable property、非 private mutable field、static/indexer/隐式成员、以及存在 initializer 但无法进入 compiler lowering 的 mutable setup carrier 会直接报 `UnsupportedSetupLogicLowering`
  - `RenderFragment` / `MarkupString` / static markup 这类需要 source-stable 追踪的 member carrier 仍不因为普通 setup `let` carrier 支持而放宽；它们一旦依赖后续可观察写入，继续按 source-stable 合同 fail-fast
  - 仍不支持需要模拟构造/任意写入时序语义的 property、`async` getter 语义模拟、以及超出当前单表达式 / 单返回受控子集的 getter
- 这条扩面仍然刻意保持 fail-fast：`async` helper、`Task` / `ValueTask` 返回值、`ref` / `out` / `in` 参数、或 helper body 超出当前 compiler-owned statement lowering 支持面时，RazorVue 仍会显式报 `UnsupportedSetupLogicLowering`，而不会因为外层 helper 可分析就静默放行。受支持 block-bodied helper 的 normalized body 也会进入 LogicHash，避免 HMR/manifest 逻辑指纹漏掉 helper body 变化。
- `SetParametersAsync` 也仍是受控子集：支持 no-op（含 expression-bodied `=> Task.CompletedTask` 一类空实现）、直达 `ComponentBase.SetParametersAsync(...)` 的 base pass-through，以及“base pass-through 后接 source-stable local / local function / callable local 前缀，再进入单个受支持 `InvokeAsync` emit”这类可稳定映射到一个 Vue `watch` 的形态；重复 emit、额外 mutation、控制流或更一般的方法体仍显式 unsupported。
- `SetParametersAsync` 的 no-op 也遵循同一条返回类型语义边界：`Task.CompletedTask` 仍接受，但 `=> default` 不再被误判成空实现。
- `SetParametersAsync` 的 base-chain 也保持保守边界：如果 pass-through 目标落到另一个源码可分析且同样受支持的 base 实现，会继续递归接受；但若最终落到外部无源码的 override（不是 `ComponentBase` 默认实现），RazorVue 会显式按 unsupported / `FullReloadRequired` 处理，而不会把未知参数赋值语义乐观当成 no-op。
- 上述 `SetParametersAsync` contract 现在已在 analyzer / lowering / generator 三层对齐；analyzer 会构建同一 semantic snapshot 并复用 lowering 侧 support-shape 判定，expression-bodied no-op 与 base+local-prefix emit 都不会再出现“pipeline 接受但 analyzer 仍报 `JAZORVUE006`”的漂移。

## Count-Style `for` Support

- RazorVue 的声明式 count-style `for` 当前仍刻意收窄为“单一声明局部 + 直接比较条件 + 结构可识别的步进表达式”的子集。
- 当前已覆盖的 iterator authoring 形态包括：
  - `i++`
  - `i--`
  - `i += step`
  - `i -= step`
  - `i = i + step`
  - `i = step + i`
  - `i = i - step`
  - `i += GetStep()`
  - `i = i + GetStep()`
- 其中 `i = i + step` / `i = step + i` / `i = i - step` 会被规范化到与 `+=` / `-=` 相同的 count-style loop 语义，再继续进入 canonical / H / SFC lowering；不会额外引入新的 runtime 协议。
- `step` 不要求是编译期常量。只要 iterator 结构仍是当前受支持的 `++` / `--` / `+= expr` / `-= expr` / `i = i +/- expr` 子集，`expr` 可以是当前组件方法调用等运行时表达式；RazorVue 会保留“进入 range helper 前单次求值”的合同，并在 SFC 路径对需要单次求值保护的步进表达式提升 setup/computed binding。
- 上述动态步进表达式路径现在也已被 Razor IR / parity / pipeline / canonical / SFC 回归正式锁定，不再只停留在 analyzer 或 runtime helper 实现层面的隐式支持。
- 这条声明式 count-style 支持边界仍然不放宽到任意 iterator 表达式。多 iterator、非单变量协议、`i = i * step`、`i = Next(i)`、以及需要逐轮重新解释循环协议的形态，当前仍不会被当作 `RazorVueForNode` / `__jazorVueForRange(...)` 这条声明式 count-style 合同接受。
- 但这不再等同于“整体不支持 `for`”。对 handwritten `BuildRenderTree` 与 Razor IR root/template code-block 而言，只要这类 `for` 仍落在现有同步 imperative render artifact contract 内，它们现在会直接切到 `RazorVueImperativeBlockNode` / render-context imperative bridge / render-function `.vue` 主线。例如 `for (var index = 0; index < Count; index++, total++)` 这类多 iterator authored form，当前已不再因 count-style analyzer 失败而直接报 unsupported；它会按真实 JS `for (...)` 语义交给 `SemanticWalker` / imperative render lowering 承载。
- `foreach` 也遵循同一条“双通道”边界：当 direct Razor IR `@foreach` body 仍可结构化时，继续保留 `RazorVueForEachNode` / declarative lowering；当 body 已经需要 imperative 语义（例如循环内 `break` / `continue`），则不再因为 frontend 结构化失败而掉回 unsupported，而是直接切到 `RazorVueImperativeBlockNode` / render-context imperative bridge / render-function `.vue` 主线。这里扩的是 frontend fallback，不是新增 `foreach` runtime 协议。

## Imperative Block Phase 1

- `Jazor.RazorVue` 现已把命令式 render block 提升为正式 render-tree 语义：`RazorVueImperativeBlockNode` / `RazorVueImperativeBlockKind`。
- handwritten `BuildRenderTree` frontend 与 Razor IR frontend 现在共享 body-level promotion 规则；复杂 block body 不再继续通过两条前端各自增加 statement 特判来支持。
- 当前 Phase 1 已落地的是：
  - imperative body 建模
  - 双前端 promotion 对齐
  - handwritten `BuildRenderTree` 与 Razor IR `BuildRenderTree`/root template `@{ ... }` 的 local segment promotion 对齐；声明式 siblings 会保留 declarative render tree，只有命中的 imperative segment 进入 render-function lowering
  - typed child-content / typed slot template body 的 standalone imperative promotion 已覆盖 `if` 保持结构化节点、而 `do-while` / `for` / `foreach` / `while` / `switch` / `try-catch-finally` / `using` / `using declaration` / `lock` / `return` / `throw` / mutation 在需要 imperative 语义时稳定回退到统一 imperative render 主线，其中 `for` / `foreach` 的 `break` / `continue` 四类循环控制分支均已锁入回归
  - `.mjs` / H artifact 的 body-level imperative render bridge
  - `.vue` / SFC artifact 的 render-function 承载
  - imperative 产物 runtime vocabulary 现已统一为 render-context（`__jazorRenderContext`、`enterElement/leaveElement`、`append`、`setComponentParameter`、`finish`）；最终 Vue 产物不再暴露 Razor `RenderTreeBuilder` 语义
  - 首段真实 imperative render 承载：提前 `return`、`while` / `do-while`、带 `break` / `continue` 的 `for` / `foreach`、`switch`、`lock`、`try/catch/finally`、`using` / `using declaration`、无 `goto` 的 labeled statement、局部 mutation、imperative body 内静态 `AddMarkupContent(...)` / `AddContent(..., MarkupString)`。`goto` 继续显式 fail-fast，因为 `Jazor.Compiler` 也不提供等价 JS lowering
  - body-level imperative `OpenRegion(...)` / `CloseRegion()` 作为 frame-shape 边界保留，不生成 Vue vnode；render-context 会校验 close 时回到 open 时的 frame depth，并在 `finish()` 时拒绝未闭合 region
  - mixed render-function 组合层会保留可声明式表达的 sibling vnode：普通 count-style `for` / `foreach`、conditional、template scope、attribute/key/event scoped replay 仍优先发射为表达式；只有 scoped replay 内真正需要 frame/slot/child 回放时才进入 render-context bridge
  - mixed imperative body 中若声明式 sibling 使用 attribute spread，`.mjs` 与 render-function `.vue` builder 都会按 body dependency 注入 `__jazorVueMergeAttributes(...)` helper；表达式本身仍由 `SemanticWalker` / `Jazor.Compiler` lowering
  - mixed imperative segment 会保留同一 `BuildRenderTree` body 内被该 segment 实际调用的前置 local function declaration。例如“声明式 header + `void AppendLine(...)` + `while` 调用 + 声明式 footer”只会把 local function declaration、必要局部和 `while` 纳入同一个 `RazorVueImperativeBlockNode`，header/footer 仍保持声明式 sibling。local function declaration/call 的最终 JS 继续由 `SemanticWalker` 负责，planner 只维护 segment 依赖边界。该依赖扩展也会递归进入已纳入 segment 的 local function body，因此 `AppendLine(...)` 内再调用 `FormatLine(...)` 这类 transitive local helper 时，两者声明会一并保留
  - mixed imperative segment 现已覆盖 tuple deconstruction declaration/assignment；`var (label, suffix) = pair;` 后接 `while` 并被后续 sibling 读取时，会整体进入同一个 imperative segment。RazorVue 只用 `RazorVueOperationLocalCollector` 维护段内声明局部与外部可见 local alias 的边界，`let label, suffix` 的函数级声明、tuple field projection 与赋值仍由 `SemanticWalker` / `Jazor.Compiler` 生成
  - 上述承载同时覆盖 handwritten `BuildRenderTree` 与 Razor authored root template `@{ ... }` code-block，经 Razor IR frontend 提升后复用同一 imperative render 主线
  - canonical / SFC semantic 正式模型现已承载“单 imperative root program”，不再要求 SFC artifact factory 在入口层单独扫描 renderTree 决定是否旁路
  - template canonical subtree 的稳定显式边界
- 当 render tree 任意位置包含 imperative node 时，SFC 输出统一切到 render-function `.vue` 路径；不再尝试让 mixed imperative subtree 回流 template canonicalization
- 当前 Phase 1 仍未落地的是：
  - imperative body 继续结构化进入 template canonical subtree 的路径
  - 真正 async imperative render contract 的设计与产物承载；当前同步 `.mjs` / render-function `.vue` 主线对 `await`、`await foreach`、`await using` / `await using var` 均显式 fail-fast，而不是生成非法或 fire-and-forget async render
  - `lock` 当前已进入正式命令式渲染通道，但语义边界刻意收敛为 single-agent erased lock lowering：保留单次求值、空值失败、同步顺序与异常传播，不宣称 CLR monitor / cross-thread 互斥语义
- imperative component parameter bridge 现已进入 descriptor-aware 路线：
  - `OpenComponent(...)` 在 imperative render bridge 中会携带已解析组件 metadata，而不再把 `AddComponentParameter(...)` 一律当成原样 prop
  - imperative `AddComponentParameter(...)` 现已按目标组件 descriptor 正式区分 prop / emit / slot
  - current-component slot forwarding 现已在 imperative 路径保留 slot 语义，而不是退化成普通 prop 值
  - builder-style `RenderFragment` / `RenderFragment<T>` 组件参数现已在 imperative bridge 中物化为 Vue slot callback，并继续支持 nested component subtree
  - 上述 slot callback 最终也统一落到 render-context runtime，而不是继续把 nested fragment 暴露为 `RenderTreeBuilder` 风格 helper 名称
  - imperative body 中实际使用到的 injected/resolved component prop / emit / slot runtime shape 现已进入 descriptor identity/runtime-usage 收集；HMR/descriptor hash 不再忽略 imperative `AddComponentParameter(...)`、slot forwarding 或 slot builder 内嵌套组件
  - Razor IR root template `@{ ... }` promotion 后的 imperative 路径也已与 handwritten `BuildRenderTree` 对齐，current-component slot forwarding 不再在 SG/IR 路径退化成普通 slot 函数值
- 当前仍未落地的是：
  - imperative body 继续结构化进入 template canonical subtree 的路径
  - 真正 async imperative render contract 的设计与产物承载；当前同步 `.mjs` / render-function `.vue` 主线对 `await`、`await foreach`、`await using` / `await using var` 均显式 fail-fast，而不是生成非法或 fire-and-forget async render

## Default Slot Modeling

- `RazorVueComponentNode` 现在显式区分：
  - `AmbientDefaultSlotChildren`
  - `ImplicitDefaultSlotAssignments`
- 这让以下语义可以稳定对齐：
  - 普通组件标签体 default children 发射
  - library component default slot unknown-slot 校验
  - duplicate default slot 赋值检测
  - handwritten `BuildRenderTree` 与 Razor IR frontend 的 default-slot assignment 计数一致性
- typed implicit default slot 的参数名策略现已统一：
  - 优先保留库 slot contract 的参数名，例如 `context`
  - 若与当前可见局部/参数冲突，再回退为 `__jazorSlotContext*`

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
  - 必须在声明点提供 initializer，或严格匹配“声明后在同一线性局部声明前缀内完成一次简单赋值”
  - initializer 只能捕获当前可见的模板局部、slot/loop 参数或正常可编码表达式
  - 不支持除“声明后在同一线性局部声明前缀内完成一次简单赋值”以外的声明后再赋值、递增/递减、嵌套匿名函数/委托承载的模板状态写入
- 对于 `AddContent(sequence, RenderFragment<T>, value)`，当前支持源码可分析的 typed fragment：可以是 inline anonymous-function fragment，也可以是同一可分析作用域内、初始化即为该匿名模板的局部 `RenderFragment<T>` carrier；仍不把任意 delegate 值、属性承载或动态 callable 形态放宽为模板执行。
- 同一条“源码可分析的局部 `RenderFragment<T>` carrier”规则也适用于组件 typed slot/template 参数，例如 `builder.AddAttribute(1, "ItemTemplate", template);`。
- 对于 imperative bridge 中这类局部 typed `RenderFragment<T>` carrier，RazorVue 会直接把 carrier declarator 改写成最终 slot 函数：slot 函数自身创建独立 `__jazorCreateRenderContext(h)`、经 `Jazor.Compiler` / `SemanticWalker` 翻译其 builder body、最后返回 `.finish()`；不会再引入 `__jazorCreateContextualRenderSlot`、wrapper marker JS，或把 inner builder body 继续保留为 `item => __builder => ...` 形状。
- 对于 imperative bridge 中这类由当前组件 fragment factory 返回、再落入 local/member carrier 的 typed `RenderFragment<T>`，RazorVue 现在也直接 lower 为最终 slot callback：factory 的普通 captured 参数会以内联 alias 形式带入最终 callback body（例如 `Title -> props.title`），不会先生成外层 wrapper JS 再调用。
- 对于 handwritten `BuildRenderTree` 中这类局部 typed `RenderFragment<T>` carrier，如果它采用“先声明、再在同一线性局部声明前缀内完成一次简单赋值”的窄模式，当前 contract 也会继续跨过 mixed imperative segmentation 生效；即 declarative 前缀里完成 `template = CreateTemplate(Title);` 后，后续 imperative block 中再消费 `template` 仍会沿同一静态 carrier 链追到最终 slot callback lowering，而不会退回动态 delegate 值或丢失 nested component import / metadata。现在这条 contract 也允许 `RenderFragment<int> template; var revision = 0; template = CreateTemplate(Title);` 这类 sibling-local declaration 变体。
- 这条“先声明、再在同一线性局部声明前缀内完成一次简单赋值”的局部 carrier 合同现在也在 BuildRenderTree frontend / mixed imperative / pipeline 三条线上统一做了 fail-fast：一旦该 local 在后续再次出现可观察写入（重新赋值、递增/递减、`ref/out` 暴露等），RazorVue 会把它判定为不再 source-stable，而不是继续沿第一次赋值静默恢复旧模板。
- Razor IR frontend 现在也与这条 source-stable local carrier 合同对齐：`@{ RenderFragment<T> template; template = ...; } <Child ItemTemplate="template" />` 这类 authored template local 既可 lower 到 render tree / `.mjs` pipeline，也会在 non-immediate assignment 或 later writes 场景下显式 fail-fast。
- 这类 `RenderFragment` / `RenderFragment<T>` carrier 的类型识别使用 `OriginalDefinition.ToDisplayString(Jazor.Common.Format.NameFormat)` 的精确 delegate signature 合同；在该格式下，canonical 名称分别是 `Microsoft.AspNetCore.Components.RenderFragment(Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder)` 与 `Microsoft.AspNetCore.Components.RenderFragment<TValue>(TValue)`，不是裸类型名。
- 在 current-component member 层，当前还支持一个更窄的 carrier 子集：只读 expression-bodied property、声明点 initializer 的 getter-only auto-property、单返回 getter property、或 `readonly` field，只要其 `RenderFragment` / `RenderFragment<T>` 初始化器本身仍是源码可分析匿名模板，就可被 `AddContent` 与组件 typed slot/template 参数消费。
- 对 Razor authored template expression 而言，上述受控 current-component member / source-stable local carrier 现在也可直接作为 `@expr` 消费，包括 untyped `RenderFragment` 的 `@Template` / `@template`；这条路径与 slot outlet forwarding 分开处理，不会把普通 member 静默当成 `[Parameter]` slot source。
- 同一条 Razor authored template expression 路径现在也覆盖 direct untyped factory expression：受支持的 current-component method / local function fragment factory 返回 `RenderFragment` 时，可以直接写成 `@CreateTemplate(Title)`、`@CreateTemplate()`、`@CreateTemplate(subtitle: Subtitle, title: Title)`；factory 的 captured 参数会保留为外层 template scope，内层则直接落为结构化 render subtree，不会误退化成普通调用表达式，并且 named argument out-of-order 仍会保留调用点求值顺序。
- 同一条 untyped `@expr` 路径也覆盖“member/local carrier 由 fragment factory 支撑再直接消费”的 authored 形态；例如 `private RenderFragment Template => CreateTemplate(Title); @Template` 与 `RenderFragment template; template = CreateTemplate(Title); @template` 都会继续保留 factory captured scope，再直接物化结构化 render subtree。
- 同一条 Razor authored template expression 路径现在也覆盖 direct typed invocation：受控 current-component member / source-stable local `RenderFragment<T>` carrier 可以直接写成 `@Template(42)` / `@template(42)`，直接调用当前组件 fragment factory 返回值也可以写成 `@CreateTemplate(Title)(42)` / `@CreateTemplate()(42)` / `@CreateTemplate(subtitle: Subtitle, title: Title)(42)`；而当前组件 `[Parameter] RenderFragment<T>?` slot source 也可以直接写成 `@Header(Count + 1)`。这些路径会分别落到 typed fragment scope 与 typed slot outlet 语义，不会混淆成普通 invocation 表达式，并且 direct factory invocation 的 named argument out-of-order 仍会保留调用点求值顺序。
- 该子集现已继续扩到“声明点 initializer 的 private settable property / private 非 `readonly` field，但源码中不存在后续写入”的受控形态；只有在可证明未发生后续重赋值、`ref/out` 写入或其他可观察写入时，才会被视为稳定 member carrier。
- 上述受控 carrier 现在也允许把“当前组件方法 / local function 的受支持 fragment factory 调用结果”作为初始化器承载；例如局部 `RenderFragment<int> template = CreateTemplate(Title);`，或只读 property / `readonly` field 返回 `CreateTemplate(Title)`，同样可被 `AddContent` 与组件 typed slot/template 参数消费。
- 这条 factory/local/member carrier 路径现在共享同一条静态解析链：component resolution、imperative runtime-usage/descriptor identity 收集以及最终 slot callback lowering 都会沿同一个 carrier 追踪 current-component property/field、局部 carrier 和 fragment factory 返回值中的嵌套 builder body，因此 nested component import、metadata、slot runtime shape 与 HMR/identity 哈希不会再遗漏 factory-backed typed slot body。
- 上述 current-component member carrier 也支持有限的“只读 member 转发链”，例如一个只读 property 返回另一个只读 property / `readonly` field carrier；只要最终仍能静态追到源码可分析匿名模板，就会被接受。
- Razor IR frontend 现在也与 handwritten `BuildRenderTree` 对齐支持这组 current-component carrier 子集：局部 `RenderFragment` / `RenderFragment<T>` 可以从只读 property、`readonly` field、“声明点初始化且无后续写入”的 private settable property / private 非 `readonly` field、有限只读 member 转发链，或受支持 fragment factory 调用结果初始化，然后再赋给 `AddContent(...)` 或组件 typed slot/template 参数；自引用/环引用仍会显式 fail-fast。
- handwritten `BuildRenderTree` 当前还支持一个更窄的 fragment factory helper 子集：
- 当前组件方法或 local function 可以零参数返回 `RenderFragment` / `RenderFragment<T>`，只要其返回值本身仍能静态追到源码可分析匿名模板，就可被 `AddContent` 与组件 typed slot/template 参数消费。
  - 当前组件方法或 local function 也可以带普通按值参数返回 `RenderFragment` / `RenderFragment<T>`，支持两类直接调用点：
    - `builder.AddContent(0, CreateTemplate(Title), 42);`
    - `builder.AddAttribute(1, "ItemTemplate", CreateTemplate(Title));`
  - 这类 factory 调用结果也可以先落入受控 carrier，再由后续调用点消费，例如 `RenderFragment<int> template = CreateTemplate(Title); builder.AddContent(0, template, 42);` 或只读 property / `readonly` field 转发该结果。
  - 对于带参数 fragment factory，额外参数会通过嵌套 template scope / 嵌套 IIFE 保留单次求值与局部不泄漏语义；named argument 即使打乱调用书写顺序，也会继续按调用点左到右求值顺序保留作用域包裹，而不会退化成按形参声明顺序重排求值
  - 同一个带参 fragment factory 即使在同一组件内被多个不同调用点重复使用，RazorVue 也只缓存“模板骨架”，不会缓存某一次调用点的 captured 值绑定；`CreateTemplate(Title)` 与 `CreateTemplate(Subtitle)` 会各自保留自己的外层 scope，而不会互相污染
- 当前组件自身的 slot outlet / slot forwarding 源只认 `[Parameter] RenderFragment...` 属性。
- 该规则同时覆盖：
  - 默认 slot / named slot 的直接 outlet 使用
  - 当前组件 `[Parameter] RenderFragment?` 转发到子组件默认/未参数化 slot
  - 当前组件 `[Parameter] RenderFragment<T>?` 转发到子组件 typed/scoped slot，并保留子组件 slot contract 的上下文参数名
- 普通 current-component property / field 即使类型也是 `RenderFragment` / `RenderFragment<T>`，当前也不会被静默当成 slot source。
- `RenderFragment` / `RenderFragment<T>` member carrier 仍按 source-stable 合同处理：只读 / `readonly` member 与可证明无后续写入的 private carrier 可以进入静态 fragment 解析；后续重赋值、`ref/out` 写入，以及需要任意 getter/dataflow 分析的 member carrier 当前仍明确不支持。普通 setup `let` carrier 支持不会放宽 fragment/static-markup carrier 的 source-stable 要求。
- current-component member carrier 一旦形成自引用或环引用，会显式失败；RazorVue 不支持递归 current-component `RenderFragment` member carrier。
- fragment factory helper 当前支持源码可分析返回值；generic current-component method / local function 也可接受，但仅限 Roslyn 已绑定到具体构造方法实例、且返回模板形状本身仍可静态还原的子集。
  - direct `AddContent(...)` 路径支持零参数、普通按值参数，以及 `params` 数组参数 factory
  - 组件 typed slot/template 参数路径也支持零参数、普通按值参数，以及 `params` 数组参数 factory，但仍要求调用点直接传入当前组件方法 / local function factory
  - generic fragment factory 与 non-generic 走同一条缓存/作用域路径：RazorVue 按源码定义方法缓存模板骨架，按构造调用点绑定具体 captured 参数，不会把某一次 closed generic 调用的值污染到另一处调用点
  - `params` 在 RazorVue 中按“单个强类型数组形参绑定”处理，不扩展成 JavaScript 风格可变参数拍平协议
  - `ref` / `out` / `in` / recursive fragment factory 当前仍明确不支持
- 对于带额外值参数的 render helper，当前只支持：
  - 恰好一个 `RenderTreeBuilder` 参数
  - 其余参数为普通按值参数，或一个按 Roslyn 正常绑定为单个数组实参的 `params` 参数
  - generic render helper 也走同一条受支持子集，只要调用点已经被 Roslyn 绑定为具体构造方法实例，且 helper body 仍满足现有 self-contained fragment / builder 协议约束
  - 同时适用于当前组件方法与 `BuildRenderTree` 内 local function helper
  - 调用点参数与 helper 声明一一对应；支持 named argument，也支持安全可投影的 omitted optional default value
  - 多个额外参数会按调用点实参求值顺序形成嵌套局部作用域，同时保持每个 helper 形参绑定到其正确实参；即使 named argument 打乱声明顺序，也不会退化成按声明顺序重排求值
  - 对 `params` 参数，RazorVue 保留 Roslyn 的单数组绑定结果；canonical/H 会直接使用该数组表达式，SFC 会在需要时把数组初始化提升为 setup binding，再通过局部 template scope wrapper 消费
  - helper body 必须源码可分析；它可以自身形成可独立 canonicalize 的片段，也可以在调用方已打开 element/component frame 时执行受控 caller-owned replay
  - caller-owned replay 覆盖 attribute / key / spread mutation、slot/default-slot assignment、ambient child emission，以及 helper-local 平衡 `OpenRegion` / `CloseRegion` 包裹的 child emission
  - helper-local 平衡 region 会作为 Razor frame-shape 边界被 frontend 校验并在 replay 中归一化；element child replay 与 component ambient default-slot fragment replay 都会在最终 `.mjs` / render-function `.vue` 中擦除这类 region，不会无意义保留成 Vue runtime node，也不会把 component default-slot subtree 误发射为普通 children
  - 仍不支持 `ref` / `out` / `in` 参数、跨 helper 留下未闭合 frame、关闭/重开 caller-owned frame、region 逃逸/不平衡、改变最终 active caller-owned frame，或需要跨 helper 推断 frame shape 的协议
- 同文件 helper class 也可由 helper / using 路径触发递归 lowering，但仍必须是同步、源码可分析、同 artifact module 内的 runtime class；泛型、record、helper component 或 ECMAScript host type 不会进入该通道。static helper class 仅作为 RazorVue runtime helper class 支持，发射为带 `static` 成员的 JS class；普通模块级 static nested class 的导出策略仍保持 fail-fast。`await using` / `await using var` 仍显式 fail-fast，不进入同一 helper lowering 通道。
- 对 SFC 输出，模板局部声明会编码为局部 template scope wrapper，而不是泄漏为顶层 `script setup` 公共绑定。

## Verification

```powershell
dotnet build src/Jazor.RazorVue/Jazor.RazorVue.csproj
dotnet test src/Jazor.RazorVue.Test/Jazor.RazorVue.Test.csproj -p:UseSharedCompilation=false
dotnet test src/Jazor.RazorVue.RazorIr.Test/Jazor.RazorVue.RazorIr.Test.csproj -p:UseSharedCompilation=false
```

当前基线：

- `src/Jazor.RazorVue.Test`: `1304 / 1304` 通过
- `src/Jazor.RazorVue.RazorIr.Test`: `371 / 371` 通过

## Read Next

- [../Jazor.Analyzer/README.md](../Jazor.Analyzer/README.md)
- [../Jazor.Emit/README.md](../Jazor.Emit/README.md)
- [../../docs/01-目标/razorvue/README.md](../../docs/01-目标/razorvue/README.md)
