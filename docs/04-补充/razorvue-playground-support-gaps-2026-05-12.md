# RazorVue Playground 支持缺口状态（2026-05-26）

本文只记录当前仍不支持、仍需保守 fail-fast、或仍需产品化补齐的 RazorVue / Playground 能力边界。历史过程、阶段流水账和已完成细节不放在本文中，必要时通过 git 历史追溯。

## 边界规则

- RazorVue 的主输出合同是最终 `.vue` / render-function artifact，不是 wrapper-JS 中间协议。
- C# 表达式、成员、类型引用、CLR helper、import/reference 语义必须继续走 `Jazor.Compiler` / `SemanticWalker`。RazorVue 只负责 Vue artifact framing、Razor frontend 还原和组件/slot 描述符桥接。
- RazorVue 的 `.razor` 输入必须保持官方 Razor Source Generator 可编译；不能引入只有 RazorVue 自己懂、但官方 `.razor.g.cs` 无法绑定的参数或类型形态。
- 不确定能否保持求值顺序、副作用次数、slot/component metadata、HMR identity 或 Vue runtime 语义时，继续显式 fail-fast。

## 当前仍不支持

### 1. Render / Template

- `imperative body -> canonical <template>` 回流仍不支持。render tree 任意位置包含 imperative node 时，SFC 输出统一切到 render-function `.vue`，不会尝试把 mixed imperative subtree 还原成 canonical template subtree。
- 真正 async render contract 不支持。`await`、`await foreach`、`await using`、`await using var` 在 render path 中继续显式失败，不生成 fire-and-forget async render。
- `goto` 不支持。当前 `Jazor.Compiler` 没有等价 JS lowering，RazorVue 不会静默擦除跳转语义。
- 声明式 count-style `for` 只接受可归一到 `__jazorVueForRange(...)` 的单 iterator 形态。多 iterator、非加减式 iterator、`i = Next(i)`、`i = i * step`、逐轮动态步进协议不作为声明式 `RazorVueForNode` 支持；如果代码能由同步 imperative render 主线诚实承载，则走 render-function，否则失败。

### 2. Template Code Block / Markup

- 声明式模板 code-block 不是通用语句执行模型。除带 initializer 的 template local、同一线性局部声明前缀内一次简单赋值、以及已定义的结构化控制语句外，任意赋值、递增/递减、delegate/callable template state 不会被塞进 canonical template node。
- 动态 raw markup 不支持。`AddMarkupContent(...)`、`AddContent(..., MarkupString)` 和 Razor template 中的 `MarkupString` 只接受源码可分析且可证明为静态 HTML 的子集；运行时拼接 markup、运行时构造 `MarkupString`、脚本/raw HTML 执行语义继续失败。
- 后续可观察写入的 `RenderFragment` / `MarkupString` / static-markup carrier 不支持；普通 setup `let` carrier 支持不会放宽这些 source-stable 合同。

### 3. Component / `System.Type`

- 动态 `System.Type` 组件不支持。当前只支持可源码证明最终等于 `typeof(IVueComponent)` 的静态目标，包括 direct `typeof(...)`、source-stable local carrier、source-stable current-component member carrier 和受控只读转发链。
- `System.Type` carrier 后续可观察写入、运行时 `Type` dataflow、非组件 `typeof(...)` 作为组件目标继续失败。
- 组件 `System.Type` carrier 只能作为 `OpenComponent(Type)` 的静态组件目标使用，不能当普通 render content、attribute、key、condition 或 loop source。
- 普通 CLR/type-token 表达式仍归 `Jazor.Compiler` / `SemanticWalker` 处理，例如 `typeof(TestDisposable).Name`；RazorVue 不提供另一套 type-token lowering。

### 4. Fragment / Slot Carrier

- 任意 `RenderFragment` / delegate dataflow 不支持。只接受 inline template、source-stable local/member carrier、受支持 current-component/local function fragment factory 这类可静态追踪形态。
- recursive fragment factory、无法静态还原匿名模板 body 的 callable、需要任意 getter/dataflow 推理的 fragment member carrier 不支持。
- fragment factory 的 `ref` / `out` 参数和 by-reference 转发/逃逸不支持。`in` 只读值参数已支持按 captured value 读取，但继续传入任意 `ref` / `out` / `in` by-reference invocation 会显式失败。

### 5. Render Helper / Open Frame

- render helper 的 `ref` / `out` 参数不支持；`RenderTreeBuilder` 参数也必须保持普通 by-value。非 builder 的 `in` 只读值参数已支持源码可分析读取，但把 `in` 参数继续转发到任意 by-reference invocation、写回协议或逃逸协议仍会显式失败。
- recursive render helper 不支持。
- caller-owned open frame helper 只支持受控 replay：attribute/key/spread mutation、slot/default-slot assignment、ambient child emission、helper-local 平衡 `OpenRegion` / `CloseRegion`。跨 helper 留下未闭合 frame、关闭/重开 caller-owned frame、改变最终 active frame、region 逃逸/不平衡、需要跨 helper 推断 frame shape 的协议继续失败。
- 同文件 helper class lowering 只接受同步、源码可分析、同 artifact module 内 runtime class。泛型 helper class、record、helper component、ECMAScript host type 不会被该通道扁平化；普通模块级 static nested class 导出策略仍保持 fail-fast。

### 6. Setup / Lifecycle / Render Control

- setup/lifecycle lowering 不是通用执行模型。`async` helper、`Task` / `ValueTask` 返回 helper、`ref` / `out` 参数、一般外部 invocation、未知实例 method-call payload、超出当前 `SemanticWalker` statement lowering 支持面的 helper body 继续失败。
- `SetParametersAsync` 只支持 no-op、直达 `ComponentBase.SetParametersAsync(...)` 的 pass-through、以及受控 base-pass-through + 线性有序 `InvokeAsync(...)` emit 序列。额外 mutation、控制流、任意外部 invocation 或更一般方法体不支持。
- `ShouldRender` 只支持已定义的 no-op / base pass-through / 单表达式 / 线性局部前缀 / 受控 `if` / 受控传统 `switch` / 受控声明 local 形态。loop / try、switch expression、pattern/guarded switch case、表达式内部 mutation、current-component property/field mutation、任意外部/member mutation、任意 invocation expression statement、局部 lambda/delegate state、嵌套局部函数、外部引用程序集无源码 base override 仍保守 `FullReloadRequired` 或 fail-fast。
- base pass-through 若最终落到外部引用程序集里的无源码 override，继续显式失败，不乐观当成 no-op。

### 7. DOM Event / Route / Consumer Build

- 任意 `on*` 字符串 attribute 不会被当成 DOM event。当前需要 event callback/delegate-like value 或 Razor/RenderTree event modifier metadata。
- component emit modifier 不与 HTML DOM event modifier 共用路径。组件 emits 继续按 descriptor-aware component event lowering 处理。
- `.vue` default export/import 不是 Jazor authored module 的编译器边界。需要通过 `razorvue-sfc-bridge` 把 SFC default component 转成 named export/import；不计划在 Jazor 编译器里放开 default export/import。
- RazorVue library mode 仍需要 consumer 构建层。Playground 的 colocated `consumer` 是单 ASP.NET Core 项目里的前端消费构建层，不是第二个 runtime host；标准 ASP.NET Core + RazorVue library-mode 模板和约定仍需继续产品化。
- route template -> Vue Router bridge 仍拒绝无法诚实映射的长尾形态：optional separator 参数不是 segment 尾部、没有紧邻前置 optional separator、需要多层组合展开的 composite/mixed segment，以及无法转换成“一个 Vue Router path regex + generated metadata 二次校验”的 constraint 组合。未知自定义 constraint、当前 ASP.NET Core 默认 `ConstraintMap` 不内置的 `date` 仍保持 fail-fast。

## 已从缺口移除

- Body-level imperative render 主线已覆盖 `return`、`while` / `do-while`、带 `break` / `continue` 的 `for` / `foreach`、`switch`、`lock`、`try/catch/finally`、`using` / `using declaration`、无 `goto` labeled statement、局部 mutation、静态 markup / `MarkupString`。
- Imperative `AddComponentParameter(...)` 已按 descriptor 区分 prop / emit / slot；current-component slot forwarding、builder-style `RenderFragment` / `RenderFragment<T>` slot callback、nested component metadata/import 已进入正式路径。
- 静态 `OpenComponent(Type)` 及 source-stable local/current-component member `System.Type` carrier 已支持；这不是动态组件支持。
- Razor IR mixed attribute、lowercase `class` / `style` fallthrough、DOM event modifier、static markup、typed/untyped `RenderFragment` carrier、fragment factory、template local、setup/lifecycle helper 受控 payload、动态 `ShouldRender` cached render gate 已进入支持面。
- Runtime naming contract 已收敛：C# authoring surface 继续使用 `PascalCase`，Vue runtime/template 边界统一输出 `camelCase`，例如 `props.modelValue`、`item.title`、`item.isDone`。
- 本轮补齐 `System.Collections.Generic.IReadOnlyCollection<T>` / `IReadOnlyList<T>` Array carrier：`IReadOnlyCollection<T>.Count`、`IReadOnlyList<T>[int]` 和 `IReadOnlyList<T> Items { get; set; } = []` 已可通过 compiler / CLR whitelist / RazorVue Razor SG SFC consumer 路径。
- 本轮外部 Razor SG SFC consumer 已恢复：官方 Razor SG tail 输出中的 `IReadOnlyList<T>` collection expression 可 lowering，SFC 输出和 pure Deno consumer 均按 camelCase DTO contract 运行。
- route bridge 已支持常见 literal、parameter、optional/default/composite 形态、catch-all，以及当前已实现的 built-in constraint metadata 二次校验；仅长尾不可诚实映射形态仍留在缺口中。

## 最新验证记录

- `dotnet run --file scripts/csharp/test-dotnet.cs`：通过。覆盖 `Jazor.CompilerTest` 1937、`Jazor.CLR.Test` 74、`ECMAScript.Pinia.Test` 67、`ECMAScript.Pinia.Testing.Test` 39、`ECMAScript.VueRoute.Test` 101、`Jazor.RazorVue.Test` 1464、`Jolt.Test` 778、`Jazor.EmitTest` 190。
- `dotnet test src/ECMAScript.Vben.Test/ECMAScript.Vben.Test.csproj -v minimal -m:1 -p:UseSharedCompilation=false`：123 通过，0 失败。
- `dotnet test src/Jazor.RazorVue.RazorIr.Test/Jazor.RazorVue.RazorIr.Test.csproj -v minimal -m:1 -p:UseSharedCompilation=false`：493 通过，0 失败。
- `dotnet test src/ECMAScript.WebIDL.GeneratorTest/ECMAScript.WebIDL.GeneratorTest.csproj -v minimal -m:1 -p:UseSharedCompilation=false`：27 通过，0 失败。
