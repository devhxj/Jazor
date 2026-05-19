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

- handwritten `BuildRenderTree` 现已支持常量 `AddMarkupContent(...)` 静态标记片段，只要内容可安全解析为静态 HTML subtree。
- 当前已覆盖的 authoring 形态包括：
  - `builder.AddMarkupContent(0, "<section class=\"hero\"><span>safe</span><p>ok</p></section>");`
  - 含标准静态 attribute、嵌套 element、void element、自闭合 element、普通文本与 HTML comment 的同类静态片段
- 该能力在 `BuildRenderTree` frontend 与 Razor IR frontend 之间复用了同一个静态标记解析器，因此两条路径对静态 HTML 片段的 element/text/attribute 还原语义保持一致。
- `AddMarkupContent(...)` 仍刻意收窄为“编译期可证明为常量字符串”的静态 HTML；动态 markup、运行时拼接 markup 或需要执行脚本语义的 raw HTML 当前仍不支持。

## Razor IR Template Locals

- Razor IR frontend 现已支持模板内局部 `@{ ... }` code-block 的受控生产切片：带初始化器的不可变局部声明，以及 Razor IR boundary code node 驱动的顺序控制语句恢复。
- 当前已覆盖的 authoring 形态包括：
  - `@{ var localTitle = Title; }`
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
- 当前该局部 carrier 不仅支持“初始化器本身就是 inline Razor template”，也支持更窄但真实可用的 current-component 只读 member carrier 与受支持 fragment factory 调用结果；额外捕获值会继续被保留为外层 template scope，而不会被错误扁平化。
- 对 typed `RenderFragment<T>` / typed slot 而言，slot context 参数仍保留在 slot/template 自身的 `ParameterName` / `ParameterSymbol` 上；只有 factory/member carrier 额外捕获了普通值参数或当前组件值时，才会在模板 children 外层再包裹 `RazorVueTemplateScopeNode`。如果 carrier 本身只是 `item => ...` 这类直接 typed template，则 children 会直接是结构化 element/expression 节点，而不会平白新增一层 scope。
- 这条“模板 code-block 结构化恢复”路线只覆盖声明式模板子集。对于更复杂的 block code，RazorVue 后续会按 `docs/01-目标/razorvue/design/RazorVue.BlockCode.ExecutionModel.md` 收敛为正式的命令式渲染通道，而不是继续无上限扩张前端特判矩阵。
- 当前支持边界刻意收窄为“局部声明优先、且只进入受支持的顺序控制语句”的模板 code-block：
  - 每个局部必须在声明点提供 initializer
  - initializer 只能捕获当前可见的 template local、loop local、typed slot context parameter 或合法模板表达式
  - 支持局部声明后进入 `if` / `if-else` / `foreach` / count-style `for`
  - 支持 Razor IR 把 `}` 与下一个 `if` / `foreach` / `for` header 线性化到同一 `CSharpCodeIntermediateNode` 时的顺序恢复，包括 `if -> if`、`if -> foreach`、`foreach -> if`、`for -> if`
  - 支持一个更窄的 Razor IR local `RenderFragment` carrier 子集：必须是声明点初始化；初始化器可以是 inline Razor template、当前组件只读 `RenderFragment` member carrier，或受支持 fragment factory 调用结果。若最终模板体对应 Razor SDK 暴露的 `TemplateIntermediateNode`，该 carrier 之后同一 code-block 仍可继续进入受支持的 `if` / `foreach` / `for` 顺序控制
  - 当前这一声明式结构化通道不支持局部声明后进入更一般的语句执行模型；例如赋值语句、递增/递减、delegate/callable template state、`switch` / `while` / `try-catch` / `using` / `lock`
  - 上述更一般的 block code 不再被视为长期只能 fail-fast 的永久边界；其中 `while` / `switch` / `lock` / `try-catch/finally` / `using` / `using declaration` 已进入正式命令式渲染通道，其余语句族将继续沿这条通道扩面，而不是继续把它们塞回 `RazorVueConditionalNode` / `RazorVueForEachNode` / `RazorVueForNode`

## Imperative Block Phase 1

- `Jazor.RazorVue` 现已把命令式 render block 提升为正式 render-tree 语义：`RazorVueImperativeBlockNode` / `RazorVueImperativeBlockKind`。
- handwritten `BuildRenderTree` frontend 与 Razor IR frontend 现在共享 body-level promotion 规则；复杂 block body 不再继续通过两条前端各自增加 statement 特判来支持。
- 当前 Phase 1 已落地的是：
  - imperative body 建模
  - 双前端 promotion 对齐
  - `.mjs` / H artifact 的 body-level imperative render bridge
  - `.vue` / SFC artifact 的 render-function 承载
  - 首段真实 imperative render 承载：提前 `return`、`while`、带 `break` / `continue` 的 `for` / `foreach`、`switch`、`lock`、`try/catch/finally`、`using` / `using declaration`、局部 mutation、imperative body 内常量 `AddMarkupContent(...)`
  - 上述承载同时覆盖 handwritten `BuildRenderTree` 与 Razor authored root template `@{ ... }` code-block，经 Razor IR frontend 提升后复用同一 imperative render 主线
  - canonical template path 的稳定显式边界
- 当前 Phase 1 仍未落地的是：
  - imperative body 的 canonical template path
  - 真正 async imperative render contract 的设计与产物承载；当前同步 `.mjs` / render-function `.vue` 主线会对 `await using` 显式 fail-fast，而不是生成非法 async render
  - `lock` 当前已进入正式命令式渲染通道，但语义边界刻意收敛为 single-agent erased lock lowering：保留单次求值、空值失败、同步顺序与异常传播，不宣称 CLR monitor / cross-thread 互斥语义
- imperative component parameter bridge 现已进入 descriptor-aware 路线：
  - `OpenComponent(...)` 在 imperative render bridge 中会携带已解析组件 metadata，而不再把 `AddComponentParameter(...)` 一律当成原样 prop
  - imperative `AddComponentParameter(...)` 现已按目标组件 descriptor 正式区分 prop / emit / slot
  - current-component slot forwarding 现已在 imperative 路径保留 slot 语义，而不是退化成普通 prop 值
  - builder-style `RenderFragment` / `RenderFragment<T>` 组件参数现已在 imperative bridge 中物化为 Vue slot callback，并继续支持 nested component subtree
  - imperative body 中实际使用到的 injected/resolved component prop / emit / slot runtime shape 现已进入 descriptor identity/runtime-usage 收集；HMR/descriptor hash 不再忽略 imperative `AddComponentParameter(...)`、slot forwarding 或 slot builder 内嵌套组件
  - Razor IR root template `@{ ... }` promotion 后的 imperative 路径也已与 handwritten `BuildRenderTree` 对齐，current-component slot forwarding 不再在 SG/IR 路径退化成普通 slot 函数值
- 当前仍未落地的是：
  - imperative body 的 canonical template path
  - 真正 async imperative render contract 的设计与产物承载；当前同步 `.mjs` / render-function `.vue` 主线会对 `await using` 显式 fail-fast，而不是生成非法 async render

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
  - 必须在声明点提供 initializer
  - initializer 只能捕获当前可见的模板局部、slot/loop 参数或正常可编码表达式
  - 不支持声明后再赋值、递增/递减、嵌套匿名函数/委托承载的模板状态写入
- 对于 `AddContent(sequence, RenderFragment<T>, value)`，当前支持源码可分析的 typed fragment：可以是 inline anonymous-function fragment，也可以是同一可分析作用域内、初始化即为该匿名模板的局部 `RenderFragment<T>` carrier；仍不把任意 delegate 值、属性承载或动态 callable 形态放宽为模板执行。
- 同一条“源码可分析的局部 `RenderFragment<T>` carrier”规则也适用于组件 typed slot/template 参数，例如 `builder.AddAttribute(1, "ItemTemplate", template);`。
- 在 current-component member 层，当前还支持一个更窄的 carrier 子集：只读 expression-bodied property、声明点 initializer 的 getter-only auto-property、单返回 getter property、或 `readonly` field，只要其 `RenderFragment` / `RenderFragment<T>` 初始化器本身仍是源码可分析匿名模板，就可被 `AddContent` 与组件 typed slot/template 参数消费。
- 上述受控 carrier 现在也允许把“当前组件方法 / local function 的受支持 fragment factory 调用结果”作为初始化器承载；例如局部 `RenderFragment<int> template = CreateTemplate(Title);`，或只读 property / `readonly` field 返回 `CreateTemplate(Title)`，同样可被 `AddContent` 与组件 typed slot/template 参数消费。
- 上述 current-component member carrier 也支持有限的“只读 member 转发链”，例如一个只读 property 返回另一个只读 property / `readonly` field carrier；只要最终仍能静态追到源码可分析匿名模板，就会被接受。
- Razor IR frontend 现在也与 handwritten `BuildRenderTree` 对齐支持这组 current-component carrier 子集：局部 `RenderFragment` / `RenderFragment<T>` 可以从只读 property、`readonly` field、有限只读 member 转发链，或受支持 fragment factory 调用结果初始化，然后再赋给 `AddContent(...)` 或组件 typed slot/template 参数；自引用/环引用仍会显式 fail-fast。
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
- settable property、动态重赋值 field、以及需要任意 getter/dataflow 分析的 member carrier 当前仍明确不支持。
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
