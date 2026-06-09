# RazorVue Playground 支持缺口状态（2026-06-09）

本文是 RazorVue / Playground 的当前边界状态页，不记录逐次修复日志。单次修复过程、focused run 输出和完整历史细节应回到测试名、提交记录、PR 描述和实现代码中追溯。

本文只回答四类问题：

- 当前哪些形态仍必须 fail-fast。
- 当前哪些形态只能保守输出 render-function `.vue`。
- 哪些能力已经进入支持面，不再作为缺口追踪。
- 支持面变化后应从哪里验证。

## 维护规则

- “当前缺口”只放仍有效的限制、降级条件或 fail-fast 条件。
- 缺口关闭后，从“当前缺口”移除；只有它仍帮助理解边界时，才在“已固化能力摘要”保留一行。
- 不在本文追加逐条 focused run 日志。需要证明时，补测试、引用测试名，或在 PR / commit 中记录命令输出。
- 无法证明求值顺序、副作用次数、slot/component metadata、HMR identity、frame identity/depth 或 Vue runtime 语义等价时，继续显式 fail-fast，或保留 render-function `.vue`。

## 不可突破的边界

- RazorVue 的主输出合同是最终 `.vue` / render-function artifact，不引入 wrapper-JS 中间协议。
- C# 表达式、成员、类型引用、CLR helper、import/reference 语义继续走 `Jazor.Compiler` / `SemanticWalker`。RazorVue 只负责 Vue artifact framing、Razor frontend 还原和组件/slot 描述符桥接。
- RazorVue 的 `.razor` 输入必须保持官方 Razor Source Generator 可编译，不能引入只有 RazorVue 自己懂的参数或类型形态。
- Analyzer 可以更早、更严；compiler / RazorVue lowering 仍必须在实际 runtime-sensitive 使用点 fail-fast。

## 支持策略总览

| 领域 | 稳定主线 | 保守策略 | 必须 fail-fast 的核心边界 |
|------|----------|----------|----------------------------|
| Render / Template | 已实现的 root-level 与 mixed/nested template-safe 子集 | 不能证明 template-safe 时保留 render-function `.vue` | async render、`goto`、需要真实 exception/dispose/lock 语义、mutation/byref/loop control 等不可模板编码语义 |
| Template code block / raw markup | template local、受控 assignment / increment / decrement、静态可证明 raw markup | 动态 raw HTML 不降级为普通安全 HTML | 运行时生成 HTML、执行型元素/属性/directive、后续可观察写入 carrier |
| Component / `System.Type` | 可源码证明的静态 `typeof(IVueComponent)` 目标 | 普通 type-token 表达式交回 compiler | 动态 `System.Type` 组件、把 `System.Type` 当普通内容/条件/key/loop source |
| Fragment / Slot carrier | inline template、source-stable carrier、受控 fragment factory 转发链 | 任意 delegate dataflow 不进入 slot 还原路径 | 递归 factory、副作用 dataflow、无法还原匿名模板 body、`ref` / `out` 转发 |
| Render helper / open frame | 只读 captured value、受控 open-frame replay、同 artifact helper class | 无法保持 frame identity/depth 时失败 | caller-owned frame 漂移、跨 helper 未闭合 frame、关闭/重开 caller frame、region 逃逸、写回/逃逸 byref |
| Setup / lifecycle / render control | no-op helper、受控 emit/watch、受控 `ShouldRender` gate | 无源码 base override 或不可证明 delegate identity 时 `FullReloadRequired` 或失败 | async helper、任意外部 invocation、mutation、真实 exception payload、不可证明 delegate escape |
| DOM / route / consumer build | descriptor-aware emits、SFC bridge、常见 route template bridge、colocated consumer handoff | 无法诚实映射的 route 形态拒绝 | 字符串 `on*` 当事件、SFC default export 直接当 authored module、runner 缺失静默跳过 |

## 当前缺口

### Render / Template

- 通用 `imperative body -> canonical <template>` 回流仍不支持。当前只接受已实现的 root-level、无副作用、template-safe 窄子集，以及混合模板树中可证明局部且 template-safe 的 `switch` / `lock` / null-default `using` / no-op `try/catch/finally` / label / `do while(false)` 子树。其它 imperative subtree 保守输出 render-function `.vue` 或 fail-fast。
- 真正 async render contract 不支持。`await`、`await foreach`、`await using`、`await using var` 不会生成 fire-and-forget async render。
- `goto` 不支持，因为当前 `Jazor.Compiler` 没有任意 jump control flow 的等价 JS lowering。
- 声明式 count-style `for` 只接受可归一到 `__jazorVueForRange(...)` 且不会改变 iterator 求值次数的单 iterator 形态。常量、参数、属性、局部静态 carrier 和其它 loop-invariant step 可进入声明式 `RazorVueForNode`；多 iterator、非加减 iterator、逐轮动态 invocation step 或 loop-local dependent step 保守进入 imperative loop / render-function。
- 需要 runtime-sensitive exception、真实 dispose、未知 lock target、loop control、mutation、byref、same-artifact helper type runtime declaration 或动态 raw markup 语义的 template recovery 不做静默擦除。

### Template Code Block / Raw Markup

- 声明式模板 code-block 不是通用语句执行模型。除 template local initializer、同一线性声明前缀内一次简单赋值、普通 assignment / increment / decrement 的 imperative render segment recovery、同一 code-block 内受控 callable local invocation，以及已定义结构化控制语句外，delegate/callable template state 不进入 canonical template node。
- 动态 raw markup 不支持运行时生成 HTML。`AddMarkupContent(...)`、`AddContent(..., MarkupString)` 和 Razor template `MarkupString` 只接受源码可分析且可证明为静态 HTML 的子集；静态 carrier / factory / `MarkupString` carrier 的 `+` 拼接仅在每个片段均可证明为静态时支持。
- raw execution 元素、inline `on*`、Vue/raw directive attribute、`srcdoc`、`v-html`、`formaction`、畸形 tag/attribute name，以及可执行 `javascript:` / `vbscript:` / `data:` URL 继续 fail-fast。
- 后续可观察写入的 `RenderFragment` / `MarkupString` / static-markup carrier 不支持；普通 setup `let` carrier 不放宽 source-stable 合同。

### Component / `System.Type`

- 动态 `System.Type` 组件不支持。当前只支持可源码证明最终等于 `typeof(IVueComponent)` 的静态目标，包括 direct `typeof(...)`、source-stable local/member carrier 和受控只读转发链。
- `System.Type` carrier 只能作为 `OpenComponent(Type)` 的静态组件目标使用，不能当普通 render content、attribute、key、condition 或 loop source。
- 普通 CLR/type-token 表达式归 `Jazor.Compiler` / `SemanticWalker` 处理，RazorVue 不提供第二套 type-token lowering。

### Fragment / Slot Carrier

- 任意 `RenderFragment` / delegate dataflow 不支持。只接受 inline template、source-stable local/member carrier、受支持 current-component/local function fragment factory，以及 getter / fragment factory block body 内返回值依赖链可证明只由 source-stable `RenderFragment` local carrier 组成的窄子集。
- current-component fragment factory 之间的非递归只读转发链支持 source-stable 多跳解析，并按调用点/转发点书写顺序保留 captured value scope；任意 delegate 返回/参数传递不进入该路径。
- local function fragment factory 之间的同作用域非递归转发链支持 source-stable 解析，并与 current-component factory 一样保留 captured value scope；跨作用域逃逸或任意 delegate dataflow 不进入该路径。
- recursive fragment factory、getter / fragment factory block body 中无关 local/语句或副作用 dataflow、无法静态还原匿名模板 body 的 callable、fragment factory `ref` / `out` 参数和 by-reference 转发/逃逸继续 fail-fast。
- `in` 只读值参数只在已支持的 captured value 读取场景成立；继续传入任意 by-reference invocation 不支持。

### Render Helper / Open Frame

- render helper 非 builder `ref` 参数仅支持可证明只读的 captured value 读取子集：实参必须是 C# 可寻址值，helper body 不得 assign / increment / decrement、不得通过任何 by-reference invocation 转发，且不提供 caller writeback。`out` 参数、`ref` 写回/逃逸和 by-reference forwarding 继续 fail-fast；`RenderTreeBuilder` 参数必须保持 by-value。
- caller-owned open frame helper 只支持受控 replay：attribute/key/spread mutation、DOM event modifier mutation、slot/default-slot assignment、ambient child emission、helper-local 平衡 `OpenRegion` / `CloseRegion`，以及能保持 frame identity/depth 的简单条件、guard-return、consecutive guard-return、single-branch terminal-return 和 both-branches terminal-return replay。跨 helper 留未闭合 frame、关闭/重开 caller-owned frame、active frame 漂移或 region 逃逸继续 fail-fast。
- 同文件 helper class lowering 只接受同步、源码可分析、同 artifact module 内的普通 runtime class、static nested helper class，以及 erased value-only generic helper class。generic helper 的静态泛型状态、`typeof(T)` / `new T()` / type-pattern 等 runtime type-parameter 语义继续 fail-fast。helper component 只能通过 `OpenComponent` / component reference 路径渲染；`new Component()` 当普通对象使用继续 fail-fast。

### Setup / Lifecycle / Render Control

- setup/lifecycle lowering 不是通用执行模型。普通 lifecycle no-op helper 仅接受当前组件 private 同步 helper、无副作用实参、普通按值参数、受控只读 `in` 值参数和受控 `params` 展开；`async` helper、`Task` / `ValueTask`、`ref` / `out`、`in` 参数继续 by-reference forwarding、外部 invocation、未知实例 method payload 或超出 `SemanticWalker` statement lowering 的 body 继续失败。
- `SetParametersAsync` 只支持 no-op、base pass-through 和受控 emit/watch 序列、分支、guard-return、普通 `switch`、无 pattern-local 的 pattern switch、含至少一次受支持 callback emit 的受控 loop（含 `await foreach`）、`try/catch/finally` recovery/cleanup 子集。额外 mutation、非 emit loop、声明 pattern-local 并让 case body 依赖其绑定的 pattern switch、任意外部 invocation、真实 exception payload 读取或一般方法体不支持。
- `ShouldRender` 只支持已定义的 no-op、base pass-through、单表达式、受控 control flow、受控 delegate carrier / compare / null-check、local function delegate 参数 identity-return、同源条件分支 / 条件表达式 identity-return、只读本地别名链、必返回的嵌套 block alias-return、同源 `switch` / trailing-return switch identity-return 及同源 `try/catch` identity-return、同步异常分支和纯同步 `throw` 终止方法体。`await foreach`、mutation、任意 delegate escape、跨 member / 外部 callable 传参返回、外部引用程序集无源码 base override 继续 fail-fast 或 `FullReloadRequired`。

### DOM Event / Route / Consumer Build

- 任意 `on*` 字符串 attribute 不会被当成 DOM event；需要 event callback/delegate-like value 或 Razor/RenderTree event modifier metadata。
- component emit modifier 不与 HTML DOM event modifier 共用路径，组件 emits 继续按 descriptor-aware component event lowering。
- `.vue` default export/import 不作为 Jazor authored module 的编译器边界；SFC default component 仍通过 `razorvue-sfc-bridge` 转成 named export/import。
- RazorVue library mode 的 colocated `consumer` 是同一 ASP.NET Core 项目内的前端消费构建层，不是第二个 runtime host。`JazorConsumerRoot` 已设置但 runner 缺失时必须由 MSBuild target fail-fast。
- route template -> Vue Router bridge 继续拒绝无法诚实映射的长尾形态：optional separator 参数位置非法、需要多次 optional separator 展开的 composite/mixed segment、未知自定义 constraint，以及无法表达为“Vue Router path regex + generated metadata 二次校验”的 constraint 组合。普通多参数 composite/mixed segment 已由 Emit 回归固化。

## 已固化能力摘要

以下内容已经进入支持面，不再作为当前缺口逐条追踪。具体行为仍以实现和回归测试为准。

| 领域 | 已固化能力 |
|------|------------|
| Body-level imperative render | 常见 `return`、loop、`switch`、`lock`、`try/catch/finally`、`using` / using declaration、无 `goto` labeled statement、局部 mutation、静态 markup / `MarkupString`。 |
| Canonical template recovery | root-level 与 mixed/nested template-safe 子树中的受控 `switch`、guard-return、`try/finally` / 空 recovery、`lock(this)` / 受控 readonly object gate、null/default `using`、no-op label、`do while(false)` / `while(false)` 子集。 |
| Component / slot metadata | Component parameter descriptor、current-component slot forwarding、builder-style `RenderFragment` / `RenderFragment<T>` slot callback、nested component metadata/import。 |
| Razor IR frontend | mixed attribute、lowercase `class` / `style` fallthrough、DOM event modifier、typed/untyped `RenderFragment` carrier、fragment factory、template local、受控 setup/lifecycle helper payload、动态 `ShouldRender` cached render gate。 |
| Raw markup / `MarkupString` | 可证明静态的 carrier、条件静态分支、静态安全 `+` 拼接、显式 `MarkupString` 表达式入口；plain `string` 仍按普通 render content 处理。 |
| Fragment factory | getter / factory block body 中由 source-stable `RenderFragment` local carrier 组成的返回值依赖链，current-component 与 local function factory 的非递归多跳转发链。 |
| Render helper / open frame | 只读 `ref` captured value 子集、caller-owned attribute/child replay、DOM event modifier mutation、简单条件 replay、guard-return replay、consecutive guard-return nested replay、single-branch / both-branches terminal-return replay、current-component 与 local function recursive render helper imperative materialization。 |
| Helper class / component boundary | 同文件普通 class、static nested helper class、erased value-only generic helper class runtime 发射；helper component 继续只通过 `OpenComponent` / component reference 路径渲染。 |
| Setup / lifecycle / render control | 普通 lifecycle no-op helper 的只读 `in` 值参数、`SetParametersAsync` 受控 switch / pattern switch / loop emit-watch、`ShouldRender` 同源 delegate identity-return / alias-return / control-flow return 与纯同步 `throw`。 |
| Build / route bridge | RazorVue library-mode colocated consumer build / publish handoff、runner 缺失 fail-fast、SFC bridge default-to-named import/export、普通多参数 composite/mixed route template bridge。 |

## 验证入口

当前快照的依据应落在测试和实现中，而不是继续扩写本文。涉及 RazorVue 支持面变更时，按风险选择以下入口：

- `dotnet test src/Jazor.RazorVue.Test/Jazor.RazorVue.Test.csproj`
- `dotnet test src/Jazor.RazorVue.RazorIr.Test/Jazor.RazorVue.RazorIr.Test.csproj`
- `dotnet test src/Jazor.EmitTest/Jazor.EmitTest.csproj`
- `git diff --check`

最近状态锚点：

- 2026-06-09：focused 验证覆盖 Render Helper / Open Frame 的 caller-owned replay、DOM event modifier mutation、conditional replay、guard-return replay、consecutive guard-return nested replay、single-branch / both-branches terminal-return replay、ordinary conditional child boundary、recursive helper materialization、overload alias 去碰撞，以及 active frame 漂移、recursive caller-owned mutation、component frame DOM event modifier 等负向边界；`RazorVueRenderHelperOpenFrameBoundaryTests` 为 64/64 通过，wider caller-owned 过滤器为 104/104 通过。
- 2026-06-09：focused 验证覆盖 Razor IR / BuildRenderTree Fragment factory returned-value dependency、getter local-chain、`MarkupString` / static-markup 表达式入口收紧、later-write 负向边界。
- 2026-05-29：Emit 全套在 SDK integration 编排优化后为 196/196 通过；同日补充普通多参数 composite/mixed route bridge 回归。
- 2026-05-29：focused 验证覆盖 count-style `for`、Template code-block imperative segment、mixed/nested template-safe recovery、raw markup 静态条件/拼接、render helper 只读 `ref`、helper class、setup/lifecycle 和 `ShouldRender` 受控边界。

## 下一步

- 优先继续评估 Render Helper / Open Frame 的 caller-owned open frame 长尾边界。只有能证明 active frame identity、frame depth、attribute/slot/child replay 顺序和 captured value 求值次数不变的形态，才允许进入支持面。
- 跨 helper 未闭合 frame、关闭/重开 caller-owned frame、active frame 漂移、region 逃逸、component frame DOM event modifier 和 recursive caller-owned mutation 继续 fail-fast。
- 后续新发现的缺口只补充到“当前缺口”；完成过程留在测试名、PR/commit 描述和 git 历史中。
