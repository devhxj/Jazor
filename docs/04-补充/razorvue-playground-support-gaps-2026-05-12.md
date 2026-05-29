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
- 声明式 count-style `for` 只接受可归一到 `__jazorVueForRange(...)` 的单 iterator 形态。多 iterator、非加减 iterator、逐轮动态 step 或 loop-local dependent step 不进入声明式 `RazorVueForNode`。
- 需要 runtime-sensitive exception、dispose、lock、loop control、mutation、byref、same-artifact helper type runtime declaration 或动态 raw markup 语义的 template recovery 不做静默擦除。

### Template Code Block / Markup

- 声明式模板 code-block 不是通用语句执行模型。除 template local initializer、同一线性声明前缀内一次简单赋值和已定义结构化控制语句外，普通赋值、递增/递减、delegate/callable template state 不进入 canonical template node。
- 动态 raw markup 不支持。`AddMarkupContent(...)`、`AddContent(..., MarkupString)` 和 Razor template `MarkupString` 只接受源码可分析且可证明为静态 HTML 的子集。
- raw execution 元素、inline `on*`、Vue/raw directive attribute、`srcdoc`、`v-html`、`formaction`、畸形 tag/attribute name，以及可执行 `javascript:` / `vbscript:` / `data:` URL 继续 fail-fast。
- 后续可观察写入的 `RenderFragment` / `MarkupString` / static-markup carrier 不支持；普通 setup `let` carrier 不放宽 source-stable 合同。

### Component / `System.Type`

- 动态 `System.Type` 组件不支持。当前只支持可源码证明最终等于 `typeof(IVueComponent)` 的静态目标，包括 direct `typeof(...)`、source-stable local/member carrier 和受控只读转发链。
- `System.Type` carrier 只能作为 `OpenComponent(Type)` 的静态组件目标使用，不能当普通 render content、attribute、key、condition 或 loop source。
- 普通 CLR/type-token 表达式归 `Jazor.Compiler` / `SemanticWalker` 处理，RazorVue 不提供第二套 type-token lowering。

### Fragment / Slot Carrier

- 任意 `RenderFragment` / delegate dataflow 不支持。只接受 inline template、source-stable local/member carrier、受支持 current-component/local function fragment factory。
- recursive fragment factory、getter/dataflow 推理、无法静态还原匿名模板 body 的 callable、fragment factory `ref` / `out` 参数和 by-reference 转发/逃逸继续 fail-fast。
- `in` 只读值参数只在已支持的 captured value 读取场景成立；继续传入任意 by-reference invocation 不支持。

### Render Helper / Open Frame

- render helper 的 `ref` / `out` 参数不支持；`RenderTreeBuilder` 参数必须保持 by-value。
- recursive render helper 不支持。
- caller-owned open frame helper 只支持受控 replay：attribute/key/spread mutation、slot/default-slot assignment、ambient child emission、helper-local 平衡 `OpenRegion` / `CloseRegion`。跨 helper 留未闭合 frame、关闭/重开 caller-owned frame、active frame 漂移或 region 逃逸继续 fail-fast。
- 同文件 helper class lowering 只接受同步、源码可分析、同 artifact module 内的非泛型非 record runtime class。generic helper class、helper component 和普通模块级 static nested class 导出策略继续 fail-fast。

### Setup / Lifecycle / Render Control

- setup/lifecycle lowering 不是通用执行模型。普通 lifecycle no-op helper 仅接受当前组件 private 同步 helper、无副作用实参、普通按值参数和受控 `params` 展开；`async` helper、`Task` / `ValueTask`、`ref` / `out` / `in`、外部 invocation、未知实例 method payload 或超出 `SemanticWalker` statement lowering 的 body 继续失败。
- `SetParametersAsync` 只支持 no-op、base pass-through 和受控 emit/watch 序列、分支、guard-return、`try/catch/finally` recovery/cleanup 子集。额外 mutation、非 emit loop / pattern switch、任意外部 invocation、真实 exception payload 读取或一般方法体不支持。
- `ShouldRender` 只支持已定义的 no-op、base pass-through、单表达式、受控 control flow、受控 delegate carrier / compare / null-check 和同步异常分支。`await foreach`、无正常 `return bool` 的纯 `throw`、mutation、delegate escape、跨 member / 外部 callable 传参返回、外部引用程序集无源码 base override 继续 fail-fast 或 `FullReloadRequired`。

### DOM Event / Route / Consumer Build

- 任意 `on*` 字符串 attribute 不会被当成 DOM event；需要 event callback/delegate-like value 或 Razor/RenderTree event modifier metadata。
- component emit modifier 不与 HTML DOM event modifier 共用路径，组件 emits 继续按 descriptor-aware component event lowering。
- `.vue` default export/import 不作为 Jazor authored module 的编译器边界；SFC default component 仍通过 `razorvue-sfc-bridge` 转成 named export/import。
- RazorVue library mode 的 colocated `consumer` 是同一 ASP.NET Core 项目内的前端消费构建层，不是第二个 runtime host。`JazorConsumerRoot` 已设置但 runner 缺失时必须由 MSBuild target fail-fast。
- route template -> Vue Router bridge 继续拒绝无法诚实映射的长尾形态：optional separator 参数位置非法、多层 composite/mixed segment、未知自定义 constraint，以及无法表达为“Vue Router path regex + generated metadata 二次校验”的 constraint 组合。

## 已移除缺口摘要

- Body-level imperative render 主线已覆盖常见 `return`、loop、`switch`、`lock`、`try/catch/finally`、`using` / `using declaration`、无 `goto` labeled statement、局部 mutation、静态 markup / `MarkupString`。
- Root-level canonical `<template>` recovery 已覆盖受控 `switch`、guard-return、`try/finally` / 空 recovery、`lock(this)` / 受控 readonly object gate、no-op label、null/default `using`、null/default leading `using declaration`、`do while(false)` 和 `while(false)` 子集。
- Component parameter descriptor、current-component slot forwarding、builder-style `RenderFragment` / `RenderFragment<T>` slot callback、nested component metadata/import 已进入正式路径。
- Razor IR mixed attribute、lowercase `class` / `style` fallthrough、DOM event modifier、static markup、typed/untyped `RenderFragment` carrier、fragment factory、template local、setup/lifecycle helper 受控 payload、动态 `ShouldRender` cached render gate 已进入支持面。
- RazorVue library-mode colocated consumer build / publish handoff、runner 缺失 fail-fast、SFC bridge default-to-named import/export 和常见 route template bridge 已有回归覆盖。

## 验证入口

当前快照的依据应落在测试和实现中，而不是继续扩写本文。涉及 RazorVue 支持面变更时，按风险选择以下入口：

- `dotnet test src/Jazor.RazorVue.Test/Jazor.RazorVue.Test.csproj`
- `dotnet test src/Jazor.RazorVue.RazorIr.Test/Jazor.RazorVue.RazorIr.Test.csproj`
- `dotnet test src/Jazor.EmitTest/Jazor.EmitTest.csproj`
- `git diff --check`

最近一次记录的 focused 验证在 2026-05-29 覆盖 RazorVue、Razor IR、Emit 相关边界，并通过 `git diff --check`。后续以实际命令输出为准。

## 下一步

- 执行 RazorVue / Razor IR / Emit 的宽验证后，将本文标记为当前快照已验证。
- 后续新发现的缺口只补充到“当前缺口”；已完成过程留在测试名、PR/commit 描述和 git 历史中。
