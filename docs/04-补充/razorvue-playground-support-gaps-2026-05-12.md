# RazorVue Playground 支持缺口状态（2026-05-25）

## 目的

本文只记录当前仍不支持、仍需保守 fail-fast、或仍需产品化补齐的 RazorVue / Playground 能力边界。历史过程、阶段流水账和已完成细节不再放在本文中，必要时通过 git 历史追溯。

当前解释规则：

- RazorVue 的主输出合同是最终 `.vue` / render-function artifact，不是 wrapper-JS 中间协议。
- C# 表达式、成员、类型引用、CLR helper、import/reference 语义必须继续走 `Jazor.Compiler` / `SemanticWalker`，RazorVue 只负责 Vue artifact framing、Razor frontend 还原和组件/slot 描述符桥接。
- 不确定能否保持求值顺序、副作用次数、slot/component metadata、HMR identity 或 Vue runtime 语义时，继续显式 fail-fast。

## 当前仍不支持

### 1. Render / Template 模型

- `imperative body -> canonical <template>` 路径仍未落地。只要 render tree 任意位置包含 imperative node，SFC 输出统一切到 render-function `.vue`，不会尝试把 mixed imperative subtree 回流到 canonical template subtree。
- 同步 render artifact 仍不支持真正 async imperative render contract。`await`、`await foreach`、`await using`、`await using var` 在 render path 中继续显式失败；当前不会生成 fire-and-forget async render 或破坏 Vue `render()` 同步返回时序。
- `goto` 仍不支持。当前 `Jazor.Compiler` 没有等价 JS lowering，RazorVue 不会静默擦除跳转语义。
- 声明式 count-style `for` 只接受可归一到现有 `__jazorVueForRange(...)` 合同的单 iterator 形态。多 iterator、非加减式 iterator、`i = Next(i)`、`i = i * step`、逐轮动态步进协议不作为声明式 `RazorVueForNode` 支持；若代码本身能由同步 imperative render 主线诚实承载，则走 render-function，否则失败。
- 声明式模板 code-block 通道不是通用语句执行模型。除带 initializer 的 template local、同一线性局部声明前缀内一次简单赋值、以及已定义的结构化控制语句外，任意赋值、递增/递减、delegate/callable template state 不会被塞进 canonical template node。

### 2. Component / `System.Type` 边界

- 动态 `System.Type` 组件不支持。当前只支持可源码证明最终等于 `typeof(IVueComponent)` 的静态目标，包括 direct `typeof(...)`、source-stable local carrier、source-stable current-component member carrier 和受控只读转发链。
- `System.Type` carrier 后续可观察写入、运行时 `Type` dataflow、非组件 `typeof(...)` 作为组件目标继续失败。
- 组件 `System.Type` carrier 只能作为 `OpenComponent(Type)` 的静态组件目标使用，不能当普通 render content、attribute、key、condition 或 loop source。
- 普通 CLR/type-token 表达式仍归 `Jazor.Compiler` / `SemanticWalker` 处理，例如 `typeof(TestDisposable).Name`；RazorVue 不提供另一套 type-token lowering。

### 3. Markup / Fragment / Slot Carrier

- 动态 raw markup 不支持。`AddMarkupContent(...)`、`AddContent(..., MarkupString)` 和 Razor template 中的 `MarkupString` 只接受源码可分析且可证明为静态 HTML 的子集；运行时拼接 markup、运行时构造 `MarkupString`、脚本/raw HTML 执行语义继续失败。
- 任意 `RenderFragment` / delegate dataflow 不支持。只接受 inline template、source-stable local/member carrier、受支持 current-component/local function fragment factory 这类可静态追踪形态。
- 后续可观察写入的 `RenderFragment` / `MarkupString` / static-markup carrier 不支持；普通 setup `let` carrier 支持不会放宽这些 source-stable 合同。
- recursive fragment factory、无法静态还原匿名模板 body 的 callable、需要任意 getter/dataflow 推理的 fragment member carrier 不支持。

### 4. Render Helper / Open Frame 协议

- render helper 的 `ref` / `out` / `in` 参数不支持。
- recursive render helper 不支持。
- caller-owned open frame helper 只支持受控 replay：attribute/key/spread mutation、slot/default-slot assignment、ambient child emission、helper-local 平衡 `OpenRegion` / `CloseRegion`。跨 helper 留下未闭合 frame、关闭/重开 caller-owned frame、改变最终 active frame、region 逃逸/不平衡、需要跨 helper 推断 frame shape 的协议继续失败。
- 同文件 helper class lowering 只接受同步、源码可分析、同 artifact module 内 runtime class。泛型 helper class、record、helper component、ECMAScript host type 不会被该通道扁平化；普通模块级 static nested class 导出策略仍保持 fail-fast。

### 5. Setup / Lifecycle

- setup/lifecycle lowering 不是通用执行模型。`async` helper、`Task` / `ValueTask` 返回 helper、`ref` / `out` / `in` 参数、一般外部 invocation、未知实例 method-call payload、超出当前 `SemanticWalker` statement lowering 支持面的 helper body 继续失败。
- `SetParametersAsync` 只支持 no-op、直达 `ComponentBase.SetParametersAsync(...)` 的 pass-through、以及受控 base-pass-through + 单个受支持 `InvokeAsync(...)` emit。重复 emit、额外 mutation、控制流或更一般方法体不支持。
- `ShouldRender` 只支持 `return true;` 和可源码解析到受支持 base 链的 pass-through。动态条件例如 `return Value > 0;` 仍会进入 unsupported / `FullReloadRequired`。
- base pass-through 若最终落到外部引用程序集里的无源码 override，继续显式失败，不乐观当成 no-op。
- `Task` 返回 lifecycle 的 bare `default` 不再视为 no-op；`Task.CompletedTask` 才是受支持空实现。non-generic `ValueTask` 的 `default` 仍按其独立 no-op 合同处理。

### 6. DOM Event / Component Emit

- 任意 `on*` 字符串 attribute 不会被当成 DOM event。当前需要 event callback/delegate-like value 或 Razor/RenderTree event modifier metadata。
- component emit modifier 不与 HTML DOM event modifier 共用路径。组件 emits 继续按 descriptor-aware component event lowering 处理。

### 7. Route / Consumer / Build 产品化

- `.vue` default export/import 不是 Jazor authored module 的编译器边界。需要通过 `razorvue-sfc-bridge` 把 SFC default component 转成 named export/import；不计划在 Jazor 编译器里放开 default export/import。
- RazorVue library mode 仍需要 consumer 构建层。Playground 的 colocated `consumer` 是单 ASP.NET Core 项目里的前端消费构建层，不是第二个 runtime host；标准 ASP.NET Core + RazorVue library-mode 模板和约定仍需继续产品化。
- route template -> Vue Router bridge 仍拒绝两类无法诚实映射的形态：composite/mixed segment 内 default value，例如 `post-{id=42}`；带 optional separator 的 composite/mixed segment，例如 `/files/{filename}.{ext?}`。普通 literal、`{parameter}`、`{parameter?}`、whole-segment default value、受控 constraint、catch-all 等已不再属于该缺口。

## 已从缺口移除的主要能力

- body-level imperative render 主线已覆盖 `return`、`while` / `do-while`、带 `break` / `continue` 的 `for` / `foreach`、`switch`、`lock`、`try/catch/finally`、`using` / `using declaration`、无 `goto` labeled statement、局部 mutation、静态 markup / `MarkupString`。
- mixed declarative + imperative render-function 路径已能保留可声明式表达的 sibling vnode，只在需要 frame/slot/child replay 时进入 render-context bridge。
- imperative `AddComponentParameter(...)` 已按 descriptor 区分 prop / emit / slot；current-component slot forwarding、builder-style `RenderFragment` / `RenderFragment<T>` slot callback、nested component metadata/import 已进入正式路径。
- 静态 `OpenComponent(Type)` 及 source-stable local/current-component member `System.Type` carrier 已支持；这不是动态组件支持。
- Razor IR mixed attribute、lowercase `class` / `style` fallthrough、`CssClass` / `CssStyle` 强类型映射、DOM event modifier、static markup、typed/untyped `RenderFragment` carrier、fragment factory、template local、setup helper/lifecycle 受控 payload 已进入支持面。
- unified `jazor-manifest.json` component projection、SFC bridge named export、consumer route metadata、ASP.NET Core host 默认静态资源/SPA fallback 主线已收敛，旧第二 manifest 和 Playground 私有 matcher 不再是当前契约。

## 验证记录

最近一次相关实现验证记录：

- `dotnet test src/Jazor.RazorVue.Test/Jazor.RazorVue.Test.csproj -v minimal`：1373 通过，0 失败。
- `dotnet test src/Jazor.RazorVue.RazorIr.Test/Jazor.RazorVue.RazorIr.Test.csproj -v minimal`：486 通过，0 失败。
- 本次更新为文档压缩与边界重写；后续代码变更仍需按改动范围重新运行 focused / full RazorVue 测试。
