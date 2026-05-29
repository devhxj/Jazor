# RazorVue Playground 支持缺口状态（2026-05-29）

本文只记录当前 RazorVue / Playground 仍需明确边界、保守降级或 fail-fast 的能力缺口。历史流水账、单次修复细节和完整命令输出不放在本文中，必要时通过 git 历史、测试名和实现代码追溯。

## 维护规则

- 新增条目必须描述当前仍有效的限制，而不是记录单次修复过程。
- 一个缺口关闭后，从“当前缺口”移除；只有当它仍帮助理解边界时，才在“已移除缺口摘要”保留一行。
- 测试命令只保留验证入口，不逐条追加 focused run 日志。
- 如果无法证明求值顺序、副作用次数、slot/component metadata、HMR identity 或 Vue runtime 语义等价，继续显式 fail-fast 或保留 render-function `.vue`。

## 硬边界

- RazorVue 的主输出合同是最终 `.vue` / render-function artifact，不引入 wrapper-JS 中间协议。
- C# 表达式、成员、类型引用、CLR helper、import/reference 语义继续走 `Jazor.Compiler` / `SemanticWalker`。RazorVue 只负责 Vue artifact framing、Razor frontend 还原和组件/slot 描述符桥接。
- RazorVue 的 `.razor` 输入必须保持官方 Razor Source Generator 可编译，不能引入只有 RazorVue 自己懂的参数或类型形态。
- analyzer 可以更早、更严；compiler / RazorVue lowering 仍必须在实际 runtime-sensitive 使用点 fail-fast。

## 当前缺口

### Render / Template

- 通用 `imperative body -> canonical <template>` 回流仍不支持。当前只接受已实现的 root-level、无副作用、template-safe 窄子集；其它 imperative subtree 保守输出 render-function `.vue` 或 fail-fast。
- 真正 async render contract 不支持。`await`、`await foreach`、`await using`、`await using var` 不会生成 fire-and-forget async render。
- `goto` 不支持，因为当前 `Jazor.Compiler` 没有任意 jump control flow 的等价 JS lowering。
- 声明式 count-style `for` 只接受可归一到 `__jazorVueForRange(...)` 且不会改变 iterator 求值次数的单 iterator 形态。常量、参数、属性、局部静态 carrier 和其它可按进入 range helper 前一次求值表达的 loop-invariant step 可进入声明式 `RazorVueForNode`；多 iterator、非加减 iterator、逐轮动态 invocation step 或 loop-local dependent step 保守进入 imperative loop/render-function。
- 需要 runtime-sensitive exception、dispose、lock、loop control、mutation、byref、same-artifact helper type runtime declaration 或动态 raw markup 语义的 template recovery 不做静默擦除。

### Template Code Block / Markup

- 声明式模板 code-block 不是通用语句执行模型。除 template local initializer、同一线性声明前缀内一次简单赋值、普通 assignment / increment / decrement 的 imperative render segment recovery、同一 code-block 内受控 callable local invocation 的 imperative render segment recovery，以及已定义结构化控制语句外，delegate/callable template state 不进入 canonical template node。
- 动态 raw markup 不支持拼接或运行时生成 HTML。`AddMarkupContent(...)`、`AddContent(..., MarkupString)` 和 Razor template `MarkupString` 只接受源码可分析且可证明为静态 HTML 的子集；运行时条件仅可在多个各自可证明安全的静态 raw-markup 分支之间选择。
- raw execution 元素、inline `on*`、Vue/raw directive attribute、`srcdoc`、`v-html`、`formaction`、畸形 tag/attribute name，以及可执行 `javascript:` / `vbscript:` / `data:` URL 继续 fail-fast。
- 后续可观察写入的 `RenderFragment` / `MarkupString` / static-markup carrier 不支持；普通 setup `let` carrier 不放宽 source-stable 合同。

### Component / `System.Type`

- 动态 `System.Type` 组件不支持。当前只支持可源码证明最终等于 `typeof(IVueComponent)` 的静态目标，包括 direct `typeof(...)`、source-stable local/member carrier 和受控只读转发链。
- `System.Type` carrier 只能作为 `OpenComponent(Type)` 的静态组件目标使用，不能当普通 render content、attribute、key、condition 或 loop source。
- 普通 CLR/type-token 表达式归 `Jazor.Compiler` / `SemanticWalker` 处理，RazorVue 不提供第二套 type-token lowering。

### Fragment / Slot Carrier

- 任意 `RenderFragment` / delegate dataflow 不支持。只接受 inline template、source-stable local/member carrier、受支持 current-component/local function fragment factory，以及 getter 内返回值依赖链可证明只由 source-stable `RenderFragment` local carrier 组成的窄子集。
- current-component fragment factory 之间的非递归只读转发链支持 source-stable 多跳解析，并按调用点/转发点书写顺序保留 captured value scope；任意 delegate 返回/参数传递不进入该路径。
- local function fragment factory 之间的同作用域非递归转发链支持 source-stable 解析，并与 current-component factory 一样保留 captured value scope；跨作用域逃逸或任意 delegate dataflow 不进入该路径。
- recursive fragment factory、getter 中无关 local/语句或副作用 dataflow、无法静态还原匿名模板 body 的 callable、fragment factory `ref` / `out` 参数和 by-reference 转发/逃逸继续 fail-fast。
- `in` 只读值参数只在已支持的 captured value 读取场景成立；继续传入任意 by-reference invocation 不支持。

### Render Helper / Open Frame

- render helper 非 builder `ref` 参数仅支持可证明只读的 captured value 读取子集：实参必须是 C# 可寻址值，helper body 不得 assign / increment / decrement、不得通过任何 by-reference invocation 转发，且不提供 caller writeback。`out` 参数、`ref` 写回/逃逸和 by-reference forwarding 继续 fail-fast；`RenderTreeBuilder` 参数必须保持 by-value。
- recursive render helper 不支持。
- caller-owned open frame helper 只支持受控 replay：attribute/key/spread mutation、slot/default-slot assignment、ambient child emission、helper-local 平衡 `OpenRegion` / `CloseRegion`。跨 helper 留未闭合 frame、关闭/重开 caller-owned frame、active frame 漂移或 region 逃逸继续 fail-fast。
- 同文件 helper class lowering 只接受同步、源码可分析、同 artifact module 内的普通 runtime class、static nested helper class，以及 erased value-only generic helper class。generic helper 的静态泛型状态、`typeof(T)` / `new T()` / type-pattern 等 runtime type-parameter 语义继续 fail-fast。helper component 只能通过 `OpenComponent` / component reference 路径渲染；`new Component()` 当普通对象使用继续 fail-fast。

### Setup / Lifecycle / Render Control

- setup/lifecycle lowering 不是通用执行模型。普通 lifecycle no-op helper 仅接受当前组件 private 同步 helper、无副作用实参、普通按值参数、受控只读 `in` 值参数和受控 `params` 展开；`async` helper、`Task` / `ValueTask`、`ref` / `out`、`in` 参数继续 by-reference forwarding、外部 invocation、未知实例 method payload 或超出 `SemanticWalker` statement lowering 的 body 继续失败。
- `SetParametersAsync` 只支持 no-op、base pass-through 和受控 emit/watch 序列、分支、guard-return、普通 `switch`、无 pattern-local 的 pattern switch、含至少一次受支持 callback emit 的受控 loop、`try/catch/finally` recovery/cleanup 子集。额外 mutation、非 emit loop、`await foreach`、声明 pattern-local 并让 case body 依赖其绑定的 pattern switch、任意外部 invocation、真实 exception payload 读取或一般方法体不支持。
- `ShouldRender` 只支持已定义的 no-op、base pass-through、单表达式、受控 control flow、受控 delegate carrier / compare / null-check、同步异常分支和纯同步 `throw` 终止方法体。`await foreach`、mutation、delegate escape、跨 member / 外部 callable 传参返回、外部引用程序集无源码 base override 继续 fail-fast 或 `FullReloadRequired`。

### DOM Event / Route / Consumer Build

- 任意 `on*` 字符串 attribute 不会被当成 DOM event；需要 event callback/delegate-like value 或 Razor/RenderTree event modifier metadata。
- component emit modifier 不与 HTML DOM event modifier 共用路径，组件 emits 继续按 descriptor-aware component event lowering。
- `.vue` default export/import 不作为 Jazor authored module 的编译器边界；SFC default component 仍通过 `razorvue-sfc-bridge` 转成 named export/import。
- RazorVue library mode 的 colocated `consumer` 是同一 ASP.NET Core 项目内的前端消费构建层，不是第二个 runtime host。`JazorConsumerRoot` 已设置但 runner 缺失时必须由 MSBuild target fail-fast。
- route template -> Vue Router bridge 继续拒绝无法诚实映射的长尾形态：optional separator 参数位置非法、需要多次 optional separator 展开的 composite/mixed segment、未知自定义 constraint，以及无法表达为“Vue Router path regex + generated metadata 二次校验”的 constraint 组合。普通多参数 composite/mixed segment 已由 Emit 回归固化。

## 已移除缺口摘要

- Body-level imperative render 主线已覆盖常见 `return`、loop、`switch`、`lock`、`try/catch/finally`、`using` / `using declaration`、无 `goto` labeled statement、局部 mutation、静态 markup / `MarkupString`。
- Root-level canonical `<template>` recovery 已覆盖受控 `switch`、guard-return、`try/finally` / 空 recovery、`lock(this)` / 受控 readonly object gate、no-op label、null/default `using`、null/default leading `using declaration`、`do while(false)` 和 `while(false)` 子集。
- Component parameter descriptor、current-component slot forwarding、builder-style `RenderFragment` / `RenderFragment<T>` slot callback、nested component metadata/import 已进入正式路径。
- Razor IR mixed attribute、lowercase `class` / `style` fallthrough、DOM event modifier、static markup、typed/untyped `RenderFragment` carrier、fragment factory、template local、setup/lifecycle helper 受控 payload、动态 `ShouldRender` cached render gate 已进入支持面。
- static markup / `MarkupString` 的普通 setup `let` member carrier 已覆盖可证明无后续写入的窄切片：private mutable string carrier 可通过 `(MarkupString)carrier` / `new MarkupString(carrier)` 继续还原为静态 HTML；后续可观察写入仍按 source-stable 合同 fail-fast。
- Template code-block 中普通 assignment、increment、decrement 已覆盖受控 imperative render segment recovery；tree / pipeline / SFC 回归锁定为 render-function `.vue`，不会伪装成 canonical template local。
- Template code-block 中同一 code-block 内定义并调用的 ordinary callable local 已覆盖受控 imperative render segment recovery；跨后续模板表达式的 delegate dataflow 仍保持 fail-fast。
- raw markup / `MarkupString` 已覆盖运行时条件选择静态分支的窄切片；每个分支仍经 static markup parser 校验，unsafe element / inline event / directive / executable URL 任一分支出现都会 fail-fast。
- RenderFragment getter/dataflow 已覆盖 getter body 中“仅声明/立即赋值返回值依赖链上的 source-stable `RenderFragment` local carrier，随后直接 return”的窄切片；getter 中额外语句、无关 local carrier、普通副作用或无法证明依赖链时仍 fail-fast。
- current-component fragment factory 多跳转发链已由回归固化：单 return / expression-bodied factory 之间可转发到最终 inline template，并保留 named argument out-of-order 的 captured scope 顺序；递归仍 fail-fast。
- local function fragment factory 同作用域转发链已由回归固化：可转发到 local/core inline template 并保留 captured scope；递归 local function factory 继续 fail-fast。
- render helper 非 builder `ref` 参数已覆盖只读 captured value 子集，并由 tree / SFC / pipeline 回归固化；`out`、写回、increment/decrement 和 by-reference 转发继续 fail-fast。
- 同文件 helper class lowering 已覆盖普通 class、static nested helper class 和 erased value-only generic helper class 的 runtime class 发射；record / struct / ECMAScript host data carrier 保持结构化降低，不发 same-artifact runtime class。
- helper component imperative render 边界已评估并由回归固化：`OpenComponent<T>` / `OpenComponent(Type)` 继续走 Vue component import / metadata / bridge 路径；`new Component()` 当普通对象、读取组件实例成员或把组件当 helper class 继续 fail-fast。
- 普通 lifecycle no-op helper 已覆盖只读 `in` 值参数，与 setup/lifecycle helper captured value 读取模型一致；把已有 `in` 参数继续传入任意 by-reference invocation 仍 fail-fast。
- `SetParametersAsync` 受控 `switch` / pattern switch emit/watch 序列已覆盖：普通 switch 保持 JS `switch`，pattern switch 对 discriminant 单次求值后输出有序 `if` / `else if` / `else`，声明 pattern-local 并让 case body 依赖绑定的形态仍 fail-fast。
- `SetParametersAsync` 受控 loop emit/watch 序列已覆盖：`foreach` / `for` / `while` 通过 compiler-owned statement lowering 输出到同一个 watcher，loop body 必须包含受支持 callback emit；非 emit loop、组件/参数 mutation 和 `await foreach` 仍 fail-fast。
- `ShouldRender` 纯同步 `throw` 方法体已覆盖 cached render gate；`try/finally` 中 `finally` 终止 throw 等改变正常返回协议的形态仍保持 fail-fast。
- RazorVue library-mode colocated consumer build / publish handoff、runner 缺失 fail-fast、SFC bridge default-to-named import/export 和常见 route template bridge 已有回归覆盖。

## 验证入口

当前快照的依据应落在测试和实现中，而不是继续扩写本文。涉及 RazorVue 支持面变更时，按风险选择以下入口：

- `dotnet test src/Jazor.RazorVue.Test/Jazor.RazorVue.Test.csproj`
- `dotnet test src/Jazor.RazorVue.RazorIr.Test/Jazor.RazorVue.RazorIr.Test.csproj`
- `dotnet test src/Jazor.EmitTest/Jazor.EmitTest.csproj`
- `git diff --check`

最近一次记录的 focused 验证在 2026-05-29 覆盖 RazorVue、Razor IR、Emit 相关边界，并通过 `git diff --check`。2026-05-29 Emit 全套在 SDK integration 编排优化后为 196/196 通过，耗时 4m38；同日补充的多参数 composite/mixed route bridge 回归单测通过。2026-05-29 count-style `for` focused 验证覆盖静态 local step carrier 继续声明式、dynamic invocation step 降级 imperative loop；同日 MarkupString focused 验证覆盖 private mutable string static-markup carrier 经 `(MarkupString)` 转换的正负边界；Template code-block focused 验证覆盖普通 assignment / increment / decrement 和同块 callable local invocation 进入 imperative render segment；raw markup focused 验证覆盖条件选择静态安全分支及 unsafe branch fail-fast；Fragment/Slot Carrier focused 验证覆盖 getter 返回 source-stable `RenderFragment` local carrier 及 side-effect / unused-local 负边界，并覆盖 current-component/local function fragment factory 转发链 captured scope 顺序与递归 fail-fast；Render Helper focused 验证覆盖只读 `ref` captured value 子集、caller-owned attribute mutation、`out` / 写回 / by-reference 转发 fail-fast、erased generic helper class / static nested helper class runtime lowering 与 runtime type-parameter 语义 fail-fast，以及 helper component `OpenComponent` 正向和 `new Component()` fail-fast 边界；Setup / Lifecycle focused 验证覆盖普通 no-op lifecycle helper `in` 只读值参数正向和 by-reference forwarding 负边界，并覆盖 `SetParametersAsync` 普通 switch / pattern switch / loop emit/watch 正向、pattern-local / non-emit loop / mutation 负向边界；`ShouldRender` focused 验证覆盖纯同步 `throw` 正向和 `try/finally` terminal throw 负向边界。后续以实际命令输出为准。

## 下一步

- 下一项：继续推进 Setup / Lifecycle / Render Control 缺口，优先评估 `ShouldRender` 的 `await foreach` 和 delegate escape 边界；任何放宽都必须保持单次求值、副作用次数、cached render gate 和 `FullReloadRequired` 语义稳定。
- 后续新发现的缺口只补充到“当前缺口”；已完成过程留在测试名、PR/commit 描述和 git 历史中。
