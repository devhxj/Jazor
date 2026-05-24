# RazorVue Playground 支持缺口记录（2026-05-12）

## 背景

`src/Playground` 是一个真实案例，不是演示玩具。它按如下路线落地：

- 单 ASP.NET Core 项目作为唯一运行时宿主
- RazorVue library mode 产出 `.vue` SFC
- consumer 使用 DenoHost 路线消费生成产物
- UI 技术栈为 `Vuetify + Pinia + Vue Router`

这个过程暴露出若干当前不支持点或高摩擦点，需要明确记录，作为后续能力提升项。

## 0. 已完成：block-code Phase 1 架构收敛

### 当前状态

RazorVue 对复杂 block code 的路线已不再是“前端继续堆 statement 特判”，而是正式收敛到：

- 声明式模板通道
- 命令式渲染通道

本轮已落地的 Phase 1 能力是：

- render tree 层新增 `RazorVueImperativeBlockNode` / `RazorVueImperativeBlockKind`
- handwritten `BuildRenderTree` frontend 与 Razor IR frontend 共享 body-level promotion 规则
- handwritten `BuildRenderTree` frontend 与 Razor IR frontend 现已共享 local segment promotion 规则：声明式 siblings 保留原 render tree，只把真正命中的 imperative segment 提升为 `RazorVueImperativeBlockNode`
- complex body 会被提升为命令式 render block，而不是继续被前端拆成越来越多的伪声明式节点
- `.mjs` / H artifact 已具备 body-level imperative render bridge
- `.vue` / SFC artifact 现已具备 render-function SFC 承载；imperative body 不再在 SFC lowering 阶段被显式拒绝
- imperative runtime vocabulary 现已统一切换到 render-context 模型：最终 `.mjs` / render-function `.vue` 产物使用 `__jazorRenderContext`、`enterElement/leaveElement`、`append`、`setComponentParameter`、`finish`，不再暴露 Razor `RenderTreeBuilder` helper 名称
- 首段真实 imperative render 承载已覆盖：提前 `return`、`while` / `do-while`、带 `break` / `continue` 的 `for` / `foreach`、`switch`、`lock`、`try/catch/finally`、`using` / `using declaration`、无 `goto` 的 labeled statement、局部 mutation、imperative body 内静态 `AddMarkupContent(...)` / `AddContent(..., MarkupString)`。`goto` 仍保持显式 unsupported，因为 `Jazor.Compiler` 也不提供等价 JS lowering
- mixed declarative + imperative render tree 现已统一按“mixed render-function path”处理：任意位置只要出现 imperative node，最终 `.vue` 产物统一切到 render-function 模型，而不是试图局部回到 template canonical subtree
- mixed render-function path 现在会保留可声明式表达的 sibling vnode：普通 count-style `for` / `foreach` body、条件分支、template scope、attribute/key/event scoped replay 仍优先发射为 `h(...)` / `__jazorVueForRange(...)` / `__jazorVueMergeAttributes(...)` 表达式；只有 scoped replay 内真正需要 frame/slot/child 回放的操作才进入 render-context bridge
- `.mjs` module builder 与 render-function `.vue` builder 现在都会扫描 imperative render body 中的 helper invocation；因此 mixed imperative body 里的声明式 attribute spread 会自动注入 `__jazorVueMergeAttributes(...)` helper，不再只扫描 declarative root render expression
- imperative render 中引用同文件 artifact module 内的同步 helper class 现已走 compiler-owned runtime class lowering：`new TestDisposable()`、`typeof(TestDisposable)`、`TestDisposable.StaticMember`、`static class StaticHelpers { ... }`、`IDisposable GetDisposable() => new TestDisposable()`、组件内嵌 helper class、helper class 字段 initializer 中递归创建其他同模块 helper class，以及 helper method 内触发 CLR whitelist `Import` 的调用，都会通过 `AstConverter.ConvertRuntimeClass(...)` / `SemanticWalker` 的 module-local declared-name context 生成最终 class/function/import 形状，而不是在 RazorVue 内拼 carrier wrapper 或手写 JS 字符串。helper type 发现现在也会通过 compiler-owned type-reference notification 记账，因此只消耗类型名或静态成员引用的 helper 同样会被保留；静态 helper class 由 `AstConverter` 发射为带 `static` field/method 的 JS class，普通模块级 static nested class 导出策略仍保持原有 fail-fast，不被这条 RazorVue runtime helper 通道放宽
- mixed imperative segment 现在会保留同一 `BuildRenderTree` body 内被命令式片段实际调用的前置 local function declaration：例如“声明式 header + `void AppendLine(...)` + `while` 调用 + 声明式 footer”的形态会只把 `AppendLine` declaration、必要局部和 `while` 放入同一个 `RazorVueImperativeBlockNode`，header/footer 仍保持声明式 sibling。local function 声明与调用仍由 `SemanticWalker.VisitLocalFunction(...)` / invocation lowering 生成最终 JS，不在 RazorVue planner 中拼装函数体或改写调用名；未被命令式 segment 引用的 local function helper 仍可继续走原有声明式 render-helper canonical path。local function dependency expansion 现已递归进入已纳入 segment 的 local function body，因此 `AppendLine(...)` 内再调用 `FormatLine(...)` 这类 transitive local helper 时，会把两者声明一并纳入同一个 imperative segment
- mixed imperative segment 现在正式覆盖 tuple deconstruction declaration/assignment，例如 `var pair = (...); var (label, suffix) = pair; while (...) { ... label ... } <footer>@label</footer>` 这类“解构声明 + 命令式 loop + 后续 sibling 读取”会整体进入同一个 imperative segment。RazorVue 不实现 tuple/CLR deconstruction 语义，只通过 `RazorVueOperationLocalCollector` 保持 segment 边界、可见局部与段内声明局部隔离，最终 `let label, suffix; label = pair...` 的声明 hoist、tuple field projection 和 assignment 仍由 `SemanticWalker` / `Jazor.Compiler` lowering

### 仍未完成

当前仍未完成的主要缺口是：

- imperative body 的 canonical template path：未完成
- 真正 async imperative render contract：未完成；当前同步 `.mjs` / render-function `.vue` 主线对 `await`、`await foreach`、`await using` / `await using var` 均显式失败
- 更复杂控制流下的进一步覆盖仍需继续扩大，但 `while` / `do-while`、`for` / `foreach` 的 `break` / `continue`、`switch` / `lock` / `try-catch/finally`、无 `goto` 的 labeled statement 已进入正式 imperative render runtime 主线

其中 `imperative AddComponentParameter(...)` 这一项本轮已完成第一阶段正式支持：

- imperative component frame 会携带 resolved component descriptor metadata
- `AddComponentParameter(...)` 不再一律退化成普通 prop 赋值
- imperative 路径现已按 descriptor 区分：
  - prop runtime name 映射
  - emit alias/runtime handler name 映射
  - slot/template 参数映射
- builder-style `RenderFragment` / `RenderFragment<T>` 组件参数现已在 imperative render bridge 中物化为 Vue slot callback
- 该 slot callback 运行时也已统一切到 render-context 承载；nested slot fragment 不再泄露 Razor builder 词汇
- current-component slot forwarding 现已在 imperative 路径保留 slot 语义，不再错误降级为 raw prop
- imperative body 中真实使用到的 injected/resolved component prop / emit / slot runtime shape，现已进入 descriptor identity/runtime-usage 收集；descriptor hash / HMR 边界推导不再忽略 imperative `AddComponentParameter(...)`、slot forwarding 或 slot builder 内嵌套组件
- Razor IR root template `@{ ... }` promotion 后的 imperative current-component slot forwarding 现已与 handwritten `BuildRenderTree` 对齐，不再在 SG/IR 路径退化成普通 `slots.xxx ?? null` 值传递
- Razor IR typed child-content / typed slot template body 中，“局部 immutable cache + imperative statement” 现已进入正式支持：局部声明后接 `while` / `do-while` / 带 `break` / `continue` 的 `for` / `foreach` / `switch` / `try-catch-finally` / `using` / `using declaration` / `lock`，以及需要 method/local imperative tail 语义的 `return` / `throw` / mutation，都会稳定提升为 `RazorVueImperativeBlockNode`，并复用现有 imperative render bridge
- Razor IR typed child-content / typed slot template body 中，“无初始化器声明 + 同一线性局部声明前缀内一次简单赋值”的窄模式也已正式锁定；例如 `string? decorated; decorated = item;` 与 `string? decorated; var revision = 0; decorated = item;` 都会与声明点初始化等价地进入 template-scoped local 契约，而不会被误判成一般赋值语句执行模型
- Razor IR typed child-content / typed slot template body 中，standalone imperative `@{ ... }` 语句块也已开始走正式 imperative render 主线；即使前面没有任何局部声明，像 `@{ while (Show) { <p>@item</p>; break; } }` 这类纯 imperative body 也不会再因为缺少 local 前缀而退回 `unbound template CSharpCodeIntermediateNode`
- 其中 typed slot/template body 的一个关键语义已锁定：一旦局部声明后的后续片段命中 imperative tail，frontend 会把“命中点到同一 slot body 末尾”的剩余片段整体收进同一个 imperative tail，而不是在命令式片段后再恢复 declarative sibling；这样才能正确保留 `return` / `throw` / mutation 对后续模板节点的可见性/终止语义

因此该缺口后续剩余的不是“完全不支持 imperative slot”，而是：

- imperative body 的 canonical template path：未完成
- 更复杂控制流下的进一步覆盖：持续推进中

因此这项工作当前应理解为：

- 架构与中间语义已收敛
- 双前端已对齐
- body-level imperative H/render-function 主线已覆盖 `return` / `while` / `do-while` / `for` / `foreach` 中的 `break` / `continue` / `switch` / `lock` / `try-catch-finally` / `using` / `using declaration` / 无 `goto` labeled statement / mutation / constant markup / tuple deconstruction declaration-assignment
- mixed render-function 组合层只负责 Vue artifact framing：条件/循环/template-scope 内含 imperative fragment 时会生成嵌套 render-context IIFE 或声明式 `map(...)` 组合，但条件、循环边界表达式、loop source、attribute spread 表达式、local function declaration/call、helper class、CLR import、tuple deconstruction 与 member/type semantics 仍交给 `EmitScopedExpression` / `SemanticWalker` / `Jazor.Compiler`
- `using` / `using declaration` 中的同模块 helper resource 支持保持在同步 render contract 内；helper class 声明插入在 Vue imports / compiler CLR imports 之后、`export default defineComponent(...)` 之前，依赖导入与声明名由 compiler module context 统一去重/绑定。该通道只收源码可分析的同 artifact module helper class（含只通过类型名/静态成员使用的 static helper class），不会把带 `[ECMAScript]` / `[ECMAScriptModule]` 标记的 host/component type 当作 helper class 扁平化
- 下一阶段重点转向 canonical template path 与更复杂语句族扩面

### 当前保护

- `src/Jazor.RazorVue/RenderTree/RazorVueImperativeRenderPromotionAnalyzer.cs`
- `src/Jazor.RazorVue/RenderTree/RazorVueOperationLocalCollector.cs`
- `src/Jazor.RazorVue/RenderTree/RazorVueRenderTreeExtractor.cs`
- `src/Jazor.RazorVue/RazorSdk/RazorVueRazorIrTemplateFrontend.cs`
- `src/Jazor.RazorVue/Canonical/RazorVueCanonicalHModelFactory.cs`
- `src/Jazor.RazorVue/Lowering/RazorVueExpressionEmitter.ComponentAuthoring.cs`
- `src/Jazor.RazorVue/Lowering/RazorVueExpressionEmitter.ImperativeMixedRender.cs`
- `src/Jazor.RazorVue/Lowering/RazorVueSfcArtifactFactory.cs`
- `src/Jazor.RazorVue/Lowering/RazorVueImperativeSfcModuleBuilder.cs`
- `src/Jazor.RazorVue/RenderTree/RazorVueOpenNodeReplayHelper.cs`
- `src/Jazor.RazorVue.Test/BuildRenderTreeTemplateFrontendTests.cs`
- `src/Jazor.RazorVue.Test/RazorVuePipelineTests.cs`
- `src/Jazor.RazorVue.Test/RazorVueSfcArtifactFactoryTests.cs`
- `src/Jazor.RazorVue.RazorIr.Test/RazorVueRazorIrTemplateFrontendTests.cs`
- `src/Jazor.RazorVue.RazorIr.Test/RazorVueTemplateFrontendParityTests.cs`

## 1. RazorVue SFC default export 不能进入 Jazor 编译器边界

### 现象

Vue SFC 生态默认以 `default export` 表达组件，但 Jazor authored C# module 路线明确不支持：

- default export emit
- default import consume

这个边界是刻意保留的，不计划通过扩展 Jazor 编译器来支持 default export/import。

### 当前影响

`.vue` 不能作为 Jazor authored module 直接消费的模块边界。真实项目需要一个 build-time bridge，把 Vue SFC 的 default component 语义转换成 Jazor 可接受的 named export/import 语义。

### 当前落地方式

`Playground` 采用：

- ASP.NET Core 项目开发/测试阶段输出根 `jazor/*.vue`
- 发布阶段将根 `jazor` 物化到 `wwwroot/jazor`
- `Jazor.Emit razorvue-consumer-entry` 读取 manifest 和 `.vue`，生成 browser/SSR entry modules
- Deno pipeline 打包生成的 browser entry 到 `wwwroot/jazor/client-entry.*`
- `Jazor.Emit razorvue-sfc-bridge` 编译 `.vue` 后输出 named-export bridge module，例如 `export { _sfc_main as PlaygroundCatalogPage }`
- consumer 入口和组件间引用都使用 named import，例如 `import { PlaygroundCatalogPage } from "./pages/playground-catalog-page.mjs"`

### 当前保护

- SFC named-export bridge 已收敛为 `Jazor.Emit` 的官方 host-facing build target，而不是 Playground 私有 workaround。
- `src/Jazor.Emit/Deno/razorvue-sfc-bridge.ts` 负责 Vue SFC 编译、default export 转 named export、相对 `.vue` import 转 `.mjs` named import、CSS 输出，以及 browser/SSR 模式差异。
- `src/Jazor.Emit/RazorVueSfcBridgeCompiler.cs` 通过 DenoHost 在隔离 workspace 中执行 bridge，避免依赖调用方目录中的 `deno.json` 或全局 Deno。
- `src/Jazor.EmitTest/RazorVueSfcBridgeCompilerTests.cs` 覆盖 named export 输出、相对 `.vue` default import 改写、SSR 模式不注入 CSS import、非法 component export name 和 manifest 缺失错误。
- `Playground` consumer 只调用官方 consumer entry 生成命令，并通过 `JAZOR_EMIT_TOOL_PATH` 在 MSBuild 中复用当前 `Jazor.Emit.dll`，避免维护本地 SFC 编译和 manifest/entry 拼接副本。

若未来 authored Jazor module 需要引用 RazorVue 组件，也应引用 bridge module 的 named export，而不是直接引用 `.vue` default export。

## 2. RazorVue Razor IR frontend 对某些静态 HTML attribute 形态仍然脆弱

### 现象

真实案例中遇到：

- 静态多 token `class` 值在 Razor IR frontend 中被识别为 mixed attribute content
- 从而触发 `ResolveAttributeValue(...)` 路径拒绝

典型现象是本来语义上完全静态的：

```razor
class="playground-page playground-page--catalog"
```

该问题已在 Razor IR frontend 中修复：当 attribute value 的多个 Razor IR child 都能证明为静态 literal 时，前端会按 Razor IR 的 `Prefix`/token 内容拼接成一个静态字符串，而不是直接判为 mixed content。

### 当前影响

真实项目不再需要把纯静态 class 设计从多 token 写法改成单 token 规避，例如：

- `playground-page playground-page--catalog`
- 不再需要改成 `playground-page-catalog`

该问题已继续扩展修复：Razor IR frontend 现在不仅支持“多 child 但全部静态 literal”的 attribute，也支持“静态 literal + Razor 表达式/代码块”混写的动态 attribute，例如：

```razor
class="todo-card @Title"
class="todo-card @(Title?.Trim() ?? "untitled")"
```

这类 mixed attribute 会被还原为真实运行时表达式，而不是错误降级为静态字符串。

### 相关代码

- `src/Jazor.RazorVue/RazorSdk/RazorVueRazorIrTemplateFrontend.cs`
- `src/Jazor.RazorVue/RazorSdk/RazorVueReflectedRazorIrReader.cs`
- `src/Jazor.RazorVue.RazorIr.Test/RazorVueRazorIrTemplateFrontendTests.cs`

### 当前落地方式

- `RazorVueReflectedRazorIrReader` 读取 Razor IR attribute value 的 `Prefix`/`Suffix` 元数据。
- `ResolveAttributeValue(...)` 在多 child 场景下先尝试静态 literal 拼接。
- 当多 child 中包含 C# expression / code attribute value 时，会按 child 顺序重建字符串拼接表达式，并优先通过 source-span / `BuildRenderTree` builder attribute 位点回写为 Roslyn `IOperation`。
- 复杂表达式若 lowering 为单次求值保护的 IIFE / 临时变量，也按真实 Roslyn 语义保留，不强行改写成更“短”的 JS。
- 剩余明确边界是：如果某个 mixed child 既不能判为静态 literal，也不能提取为可重建的 Razor 表达式节点，前端仍会显式失败，而不是静默生成不可靠代码。

## 3. library component 上原样 authoring `class=` / `style=` 已落地稳定契约

### 现象

按设计，带 `[Parameter(CaptureUnmatchedValues = true)]` 的 library component 应该支持 fallthrough attributes。

仓库中已有测试和文档也说明：

- `class`
- `style`
- `data-*`
- `aria-*`

应当可以透传。

`Playground` 的真实 authoring 过程中曾经遇到：在组件标签上写 lowercase raw attribute：

```razor
<VChip class="playground-category-chip" ... />
```

会被官方 Razor Source Generator 绑定到组件的 `Class` 参数，而不是作为 unmatched fallthrough attribute 进入 `AdditionalAttributes`。由于 `Class` 参数类型是 `VueClassValue?`，HTML-style 字符串 literal 不能按 Razor SG 规则直接绑定到该非字符串参数，最终会生成错误的 C#。

### 当前落地方式

该问题已在 authoring surface 层修复：

- 组件标签上的 lowercase `class` / `style` 走 `AdditionalAttributes` fallthrough，保持 Razor SG 原生可编译。
- 强类型 C# authoring 入口统一使用 `CssClass` / `CssStyle`，通过 `[VueProp(..., Name = "class" / "style")]` 映射到 Vue runtime prop。
- 不再在 top-level Vuetify authoring component 上暴露 `[Parameter] Class` / `[Parameter] Style`，避免与 Razor SG 的 lowercase attribute 绑定规则冲突。

因此真实项目可以恢复自然写法：

```razor
<VChip class="playground-category-chip" ... />
```

需要强类型表达式时则使用：

```razor
<VChip CssClass='@("playground-category-chip")' CssStyle='@("margin-inline: 1rem")' ... />
```

`CssClass` / `CssStyle` 仍会输出到 Vue 的 `class` / `style`，不会改变运行时语义。

### 相关参考

- `src/Jazor.RazorVue/Lowering/RazorVueCaptureUnmatchedAttributePolicy.cs`
- `src/ECMAScript.Vuetify/README.md`
- `src/Jazor.RazorVue.RazorIr.Test/RazorVueRazorIrTemplateFrontendTests.cs`
- `src/Jazor.RazorVue.Test/VuetifyAuthoringSurfaceTests.cs`
- `src/Jazor.RazorVue.Test/RazorVuePipelineTests.cs`
- `src/Jazor.RazorVue.Test/RazorVueSfcArtifactFactoryTests.cs`

### 当前保护

- Razor IR frontend 覆盖 raw `class=` fallthrough 与 `CssClass` / `CssStyle` 强类型映射。
- Vuetify authoring surface 测试禁止组件参数重新暴露 `Class` / `Style`。
- Playground 已使用自然 raw `class="playground-category-chip"` 作为真实集成验证点。

## 3.1 handwritten `BuildRenderTree` 的 typed `RenderFragment` carrier 支持已扩到受控 member 子集

### 现象

真实 authoring 中不仅会出现 inline typed fragment，也会出现：

- 局部 `RenderFragment<T>` 变量 carrier
- 当前组件只读 property / `readonly` field carrier
- 一个只读 member 再转发到另一个只读 member

如果这条路径不支持，真实组件很容易因为模板抽取方式稍有整理就回退成“RenderFragment shape 不可 canonicalize”。

### 当前落地方式

handwritten `BuildRenderTree` 当前已支持以下 typed `RenderFragment` carrier 进入：

- `builder.AddContent(sequence, RenderFragment<T>, value)`
- 组件 typed slot/template 参数，例如 `builder.AddAttribute(..., "ItemTemplate", template)`

受支持的 current-component member carrier 形态为：

- expression-bodied 只读 property
- 声明点 initializer 的 getter-only auto-property
- getter body 只有单个 `return` 的 getter-only property
- `readonly` field initializer
- 声明点 initializer 的 private settable auto-property，只要源码中不存在后续写入
- private 非 `readonly` field initializer，只要源码中不存在后续写入
- 上述只读 member 之间的有限转发链，只要最终仍能静态追到源码可分析匿名模板

### 当前保护

- slot outlet / slot forwarding source 仍只认 `[Parameter] RenderFragment...` property。
- 当前已支持：
  - 默认 slot / named slot outlet
  - 当前组件 `[Parameter] RenderFragment?` -> 子组件默认/未参数化 slot forwarding
  - 当前组件 `[Parameter] RenderFragment<T>?` -> 子组件 typed/scoped slot forwarding，并保留目标 slot 的 context 参数名
- 仍不会把普通 current-component member 静默当成 slot source。
- private settable property / private 非 `readonly` field 只有在“声明点初始化 + 源码可证明无后续写入”这个窄子集内才支持；一旦出现后续重赋值、`ref/out` 写入或其他可观察写入，仍明确不支持。
- 这条限制只约束 `RenderFragment` / `RenderFragment<T>` member carrier 的 source-stable 追踪合同。普通 setup value carrier 已另行扩展为 setup `let`：private mutable field / private-setter auto-property 可以存在 later writes，并可由模板、helper、lifecycle payload 与 imperative render body 读取。
- 需要任意 getter/dataflow 推理的 member carrier 仍明确不支持。
- current-component member carrier 一旦出现自引用或环引用，会显式失败；不会递归展开到栈溢出或产生不稳定结果。
- `src/Jazor.RazorVue.Test/BuildRenderTreeTemplateFrontendTests.cs`
- `src/Jazor.RazorVue.Test/RazorVueCanonicalSfcSemanticTests.cs`
- `src/Jazor.RazorVue.Test/RazorVuePipelineTests.cs`
- `src/Jazor.RazorVue.Test/RazorVueSfcArtifactFactoryTests.cs`
- `src/Jazor.RazorVue.RazorIr.Test/RazorVueRazorIrTemplateFrontendTests.cs`
- `src/Jazor.RazorVue.RazorIr.Test/RazorVueTemplateFrontendParityTests.cs`
  已同时覆盖：
  - 局部 carrier
  - 当前组件只读 property / `readonly` field carrier
  - 当前组件“无后续写入”的 private settable property / private 非 `readonly` field carrier
  - 只读 member 链式转发
  - 后续重赋值 fail-fast
  - 自引用 / 环引用 fail-fast

## 3.2 已完成：count-style `for` 等价步进赋值形态扩面

### 现象

此前 RazorVue 对声明式 count-style `for` 的步进子集只稳定接受：

- `i++`
- `i--`
- `i += Step`
- `i -= Step`

但真实 authoring 里，用户也很自然会写：

```csharp
for (var i = Start; i <= Count; i = i + Step)
{
    ...
}
```

或：

```csharp
for (var i = Start; i >= Count; i = i - Step)
{
    ...
}
```

这两类写法与 `+=` / `-=` 在当前 RazorVue 的 count-style loop 语义里本质等价；如果继续拒绝，会让 authoring contract 显得偶然且脆弱。

### 当前落地方式

该缺口已在共享 `RazorVueForLoopAnalyzer` 中收口：

- `ISimpleAssignmentOperation` 形态的 iterator 现会尝试识别：
  - `i = i + step`
  - `i = step + i`
  - `i = i - step`
- 命中后会规范化回现有：
  - `RazorVueForStepKind.AddAssign`
  - `RazorVueForStepKind.SubtractAssign`
- 后续 canonical / H / SFC lowering 仍完全复用现有 `__jazorVueForRange(...)` 路径，不新增新的 runtime helper、manifest 语义或 Vue 产物协议

### 当前支持边界

- 支持：`for (var i = Start; i <= Count; i = i + Step)`
- 支持：`for (var i = Start; i <= Count; i = Step + i)`
- 支持：`for (var i = Start; i >= Count; i = i - Step)`
- 支持：`for (var i = Start; i <= Count; i += GetStep())`
- 支持：`for (var i = Start; i <= Count; i = i + GetStep())`
- 仍支持：`i++` / `i--` / `i += Step` / `i -= Step`
- 支持边界不是“step 必须是常量/字段/参数”，而是“iterator 结构必须仍能归一到现有 count-style `++` / `--` / `+= expr` / `-= expr` / `i = i +/- expr` 契约”；其中 `expr` 可以是运行时方法调用，但仍按“进入 `__jazorVueForRange(...)` 前单次求值”处理，而不是逐轮重新求值
- 不支持作为声明式 count-style `RazorVueForNode`：多 iterator 表达式
- 不支持作为声明式 count-style `RazorVueForNode`：`i = i * Step`、`i = Next(i)` 这类更宽泛 iterator 协议解释
- 不支持：需要改变现有 “start / limit / step 先求值，再进入 range helper” 契约的逐轮动态步进模型

### 2026-05-23 补充校准

本轮继续收紧了“count-style 不支持”与“整体不支持 `for`”之间的边界。此前像：

```csharp
for (var index = 0; index < Count; index++, total++)
{
    builder.OpenElement(0, "section");
    builder.AddContent(1, index);
    builder.AddContent(2, total);
    builder.CloseElement();
}
```

这类 authored form 会在 frontend 里过早触发 `RazorVueForLoopAnalyzer` 的 count-style failure，直接报 unsupported。当前实现已改为：

- 若 `for` 能归一到现有 count-style 契约，仍优先走声明式 `RazorVueForNode` / `__jazorVueForRange(...)`
- 若 `for` 不能归一到 count-style，但本身仍可由现有同步 imperative render artifact contract 诚实承载，则不再直接失败，而是切到 `RazorVueImperativeBlockNode` / render-context imperative bridge / render-function `.vue` 主线

因此当前真正仍不支持的是：

- 需要新的逐轮动态步进 runtime 协议的 `for`
- 需要 async imperative render contract 的 `for`
- 其他超出当前同步 imperative render 主线的循环执行模型

同一类 frontend 过早拒绝缺口本轮也顺手收掉了一格 direct Razor IR `@foreach`：

- 之前 direct `@foreach` 主要只在 body 仍可结构化时稳定走 `RazorVueForEachNode`
- 一旦循环体里出现 `break` / `continue` 这类需要 imperative 语义的 authored form，就可能在 Razor IR frontend 结构化阶段过早掉回 unsupported
- 当前已改为与非 count-style `for` 同一原则：若 `@foreach` body 仍可结构化，继续 declarative；若 body 需要 imperative 语义，则直接切到 `RazorVueImperativeBlockNode` / render-context imperative bridge / render-function `.vue` 主线，而不是继续把“声明式 frontend 失败”误当成“整体不支持 foreach”

### 当前保护

- `src/Jazor.RazorVue/RenderTree/RazorVueForLoopAnalyzer.cs`
- `src/Jazor.RazorVue.Test/RazorVueCanonicalSfcSemanticTests.cs`
- `src/Jazor.RazorVue.Test/RazorVuePipelineTests.cs`
- `src/Jazor.RazorVue.Test/RazorVueSfcArtifactFactoryTests.cs`
- `src/Jazor.RazorVue.RazorIr.Test/RazorVueRazorIrTemplateFrontendTests.cs`
- `src/Jazor.RazorVue.RazorIr.Test/RazorVueTemplateFrontendParityTests.cs`

### 2026-05-21 补充校准

本轮再次核对后确认：`i += GetStep()` 与 `i = i + GetStep()` 这类 authored form 早已可达当前主线，并不是实际未支持项，之前的“`i = i + GetStep()` 不支持”属于文档边界过窄。

- `RazorVueForLoopAnalyzer` 本身对 `+= expr` 与 `i = i + expr` 的 `expr` 没有限定为常量/字段/参数
- pipeline `.mjs` 会直接把该步进表达式作为 `__jazorVueForRange(..., stepValue)` 的第 5 个参数传入
- canonical/SFC 语义会把这类实例方法调用步进识别为 `RequiresSetupBinding + RepeatedEvaluationRisk`，并在 SFC 产物里提升为 setup/computed binding，继续保留单次求值合同
- 因此当前真正的边界应理解为“支持单次求值的动态步进表达式，不支持逐轮动态步进协议”

### Razor IR 对齐状态

此前 Razor IR frontend 在这条路径上只会解析“carrier 引用递归”或“本地 inline template node”，对 `readonly field` / 只读 property 自身直接以匿名 typed fragment 初始化的 current-component member carrier 存在漏判，导致：

```razor
@{
    RenderFragment<int> template = _template;
}

<LayoutCard ItemTemplate="template" />

@code {
    private readonly RenderFragment<int> _template = item => @<span>@item</span>;
}
```

会被误报为 unsupported。

该缺口现已修复：Razor IR carrier initializer 解析已与 handwritten `BuildRenderTree` 路线对齐，统一支持：

- current-component 只读 property / `readonly` field 直接承载匿名 `RenderFragment` / `RenderFragment<T>` 初始化器
- 只读 member 转发链
- 受支持 fragment factory 返回值先落入 local/member carrier 再消费
- local `RenderFragment` / `RenderFragment<T>` 的“先声明、再在同一线性局部声明前缀内完成一次简单赋值” source-stable 窄模式
  - 上述 immediate-assignment 右侧现已回归锁定：inline Razor template、本组件受支持 member carrier、以及受支持 fragment factory 返回值三类都会继续进入结构化 slot/template carrier，而不会误退回 imperative tail
- 自引用 / 环引用 fail-fast

同时语义边界也已锁定：

- typed slot/template 的 context 参数保留在 slot/template 自身的 `ParameterName` / `ParameterSymbol`
- 只有存在额外 captured 普通值参数或当前组件值时，才会在 children 外再包一层 `TemplateScopeNode`
- 如果 carrier 本身只是 `item => ...` 这类直接 typed template，则 slot/template children 直接是结构化 element/expression 节点，不会额外生成 scope wrapper
- 若 local carrier 不能在同一线性局部声明前缀内完成这一次简单赋值，或在初始化后再次出现可观察写入，则继续沿 source-stable 合同显式 fail-fast

### 补充：implicit default slot assignment 与参数名策略

本轮又补齐了 default slot 的两个稳定合同：

- `RazorVueComponentNode` 不再只靠 `Children` 猜测 default slot 是否被赋值，而是显式区分：
  - `AmbientDefaultSlotChildren`
  - `ImplicitDefaultSlotAssignments`
- duplicate default slot 检测、unknown default slot 校验，以及 handwritten / Razor IR 两条 frontend 的 default-slot 计数现在都基于这份显式模型，而不是依赖扁平化后的 children 长度推断

同时，typed implicit default slot 的参数名策略也已统一：

- 优先保留库 slot contract 声明的参数名，例如 `context`
- 只有发生当前作用域命名冲突时，才回退为 `__jazorSlotContext*`

这条策略现已在 canonical / H / SFC 三条链路一致，不再出现 H 用 `context` 而 SFC 用 `__jazorSlotContext` 的分裂。

## 4. 单项目 library-mode 实际上仍需要 consumer 构建层

### 现象

从产品形态上看，`Playground` 已经满足“不要拆成 app 和 host”的要求，因为运行时只有一个 .NET 项目。

但从构建链角度，仍然必须存在一个 consumer 层去：

- 读取 RazorVue manifest
- 编译 `.vue`
- 组装 `Pinia` / `Vue Router` / `Vuetify`
- 输出浏览器 bundle

### 当前影响

“单项目”目前能做到的是：

- 单 .NET 项目
- 同仓库内 colocated consumer
- MSBuild target 自动调用 Deno consumer 构建浏览器资产

而不是“完全不需要任何前端 consumer”。

### 当前保护

- `Jazor.Emit razorvue-consumer-entry` 已成为官方 build-time consumer entry 生成入口。
- 该命令统一负责读取 `jazor-manifest.json` 中的 RazorVue component metadata、选择组件、调用 SFC named-export bridge、生成 browser/SSR bridge modules，以及写出 `client-entry.mjs` / `ssr-entry.mjs` / `vue-feature-flags.mjs`。
- 组件选择使用显式 `--component Alias=selector` 契约，selector 支持 `id:`、`name:`、`path:`；模糊匹配会失败并要求显式 selector，避免真实项目在组件重名时静默选错。
- consumer runtime 不再需要解析 manifest 或知道 `.vue` default export 转换细节，只接收 `razorVueConsumerComponents` 和 `razorVueHostRequirements`。
- `src/Jazor.EmitTest/RazorVueConsumerEntryCompilerTests.cs` 覆盖 browser/SSR entry 生成、CLI 参数解析、组件选择歧义错误和 clean 模式误删保护。
- `Playground` 的 `consumer/scripts/lib/pipeline.ts` 已改为调用官方 entry 生成命令，私有脚本只保留最终 Deno bundle、HTML dist 和 smoke verification。

### 后续提升方向

- 已收敛为 `Jazor` SDK/MSBuild contract：项目通过 `JazorConsumerRoot`、`JazorConsumerRunScriptPath`、`JazorConsumerBuildTask`、`JazorConsumerBrowserAssetRoot` 等声明式属性启用 colocated consumer build，不再手写项目私有 `Exec` target。
- `src/Jazor/buildTransitive/Jazor.targets` 现已官方提供 consumer build 与 publish materialization 组合能力；`Playground` 只保留配置，`SdkIntegrationTests` 覆盖 package consumer 场景下的 build/publish merge 行为。
- 后续仍需要提供 ASP.NET Core + RazorVue library mode 标准模板，明确 colocated consumer 目录、runtime entry、bundle 输出和 publish 合并策略。

## 5. ASP.NET Core fallback 不能使用 catch-all endpoint 抢占静态文件

### 现象

早期宿主验证时，`/assets/client-entry.js` 文件存在于 `wwwroot/assets`，但返回 404。后续按 Playground 的 `/jazor/*` 统一资源边界改为 `wwwroot/jazor/client-entry.*`，但该中间件顺序问题仍然成立。

根因是 endpoint routing 中的：

```csharp
app.MapMethods("/{**path}", ["GET", "HEAD"], ...)
```

会先为 `/assets/*` 选择 catch-all endpoint，导致 `StaticFileMiddleware` 因已有 endpoint 而不处理请求，最终落入 fallback 逻辑。

### 当前落地方式

`Playground` 已改为使用 `Jazor.AspNetCore` 官方 `UseJazorSpaFallback(...)` middleware 处理 HTML shell fallback：

- 先通过 `UseJazorWebAssets(...)` 挂载标准静态资源和开发期 `/jazor/*` 资产
- 再挂载 `UseJazorSpaFallback("/index.html")`
- 不使用 `MapMethods("/{**path}", ...)`、`MapFallbackToFile(...)` 等 endpoint catch-all 作为 SPA fallback

### 当前保护

`UseJazorSpaFallback(...)` 的默认行为面向生产宿主安全边界收窄：

- 只处理 `GET` / `HEAD`
- 默认要求 `Accept` 包含 `text/html` 或 `application/xhtml+xml`
- 所有带文件扩展名的路径都不 fallback，避免缺失静态文件被改写成 HTML
- 默认排除 `/api`、`/assets`、`/health`、`/jazor`
- 支持通过 `JazorSpaFallbackOptions.ExcludedPathPrefixes` 添加项目自定义排除前缀
- 在调用后续 pipeline 后只对未被 endpoint 选中、未开始响应、最终仍为 404 的导航请求写入 shell
- endpoint 自己返回的 404 保持 404，不会被 SPA shell 覆盖

### 后续提升方向

- 将 `UseJazorSpaFallback(...)` 纳入 ASP.NET Core + RazorVue library mode 标准模板
- Wiki 等已有站点后续可以按同一契约迁移，减少宿主私有 fallback 分类逻辑

## 6. 发布内容根不能固定到源码路径

### 现象

为了让 `dotnet run --project ...` 从仓库根启动时找到源码 `wwwroot`，一开始使用 `CallerFilePath` 固定 `ContentRootPath`。

该方式在发布包中有风险：发布后如果仍在同一台机器运行，宿主可能继续指向源码目录，而不是发布目录。

### 当前落地方式

`Playground` 已改为使用 `Jazor.AspNetCore` 官方 `JazorWebApplication.CreateBuilder(args)` 创建宿主。该 helper 的内容根解析策略是：

- 若 `AppContext.BaseDirectory/wwwroot` 存在，优先使用发布/输出目录
- 否则回退到 `Program.cs` 所在源码目录

因此 `Program.cs` 不再手写 `WebRootPath`、`PhysicalFileProvider` 或 `CallerFilePath` 内容根解析，普通 `wwwroot` 静态文件继续交给 ASP.NET Core 默认 web root 机制处理。

### 后续提升方向

- 已完成（2026-05-14 本轮）：`JazorWebApplication.ResolveContentRootPath(...)` 已由 `JazorAspNetCoreHostingTests` 独立锁定“发布/输出目录存在 `wwwroot` 时优先使用 `AppContext.BaseDirectory`，否则回退源码目录”的双分支语义，避免后续把内容根解析悄悄漂回固定源码路径。

## 7. 浏览器 bundle 与 RazorVue emit 复用 `/jazor/*` 需要明确合并语义

### 现象

`Playground` 需要让最终浏览器 bundle 也直接落在 `wwwroot/jazor`，而不是单独使用 `wwwroot/assets`。这会让发布目录中同一个 `/jazor/*` 路径同时包含：

- 根 `jazor` emit 复制出的 manifest、SFC 和 CLR runtime modules
- Deno consumer 生成的 `client-entry.js`、`client-entry.css` 与 sourcemap

### 当前落地方式

- 本地 build 阶段：根 `jazor` 仍是 RazorVue emit 源，`wwwroot/jazor` 只承载浏览器 bundle。
- 本地宿主阶段：`UseJazorWebAssets(...)` 先服务 `wwwroot` 静态文件，再服务根 `jazor` development assets，确保 `/jazor/client-entry.*` 和 `/jazor/jazor-manifest.json` 都可访问。
- publish 阶段：先清空发布 `wwwroot/jazor`，复制根 `jazor` emit，再复制 `wwwroot/jazor/client-entry.*`，最终发布包只从 `wwwroot/jazor` 服务 `/jazor/*`。
- consumer 中间 build root 默认按进程隔离为 `.deno-build/pid-*`，避免 `smoke:ssr`、`smoke:browser` 或 CI 并行任务互相清理同一目录。

### 后续提升方向

- RazorVue / ASP.NET Core 集成层已提供官方 publish 合并 target：`JazorPublishMaterializeEnabled=true` 负责将开发输出根物化到发布 `wwwroot/jazor`，`JazorPublishConsumerBrowserAssets` 负责把 colocated consumer browser bundle 合并到同一路径并清理影子目录。
- `UseJazorWebAssets(...)` 后续可以继续扩展为更完整的 RazorVue library-mode 宿主模板入口。

## 8. 已完成：统一 manifest 与宿主 API 收敛

### 目标

`Playground` 和 `Wiki` 只是两个当前验证项目，真实项目可能同时包含普通 Jazor H 函数模块、RazorVue H 组件、RazorVue SFC 组件、浏览器 bundle、SSR bridge 和自定义 host shell。

因此不能继续通过项目私有约定、文件名分裂或样例特化 option 来区分产物类型。需要把输出契约收敛为一个默认可运行、可组合、可扩展的标准宿主模型：

- 默认配置即可启动标准 Jazor 输出
- 高级场景通过 option 扩展，不要求每个项目手写必需配置
- `Playground` 与 `Wiki` 使用同一组 `Jazor.AspNetCore` helper
- manifest 只保留一个公开文件名：`jazor-manifest.json`
- 组件语义写入 manifest module metadata，而不是另起 `jazor-manifest-razorvue.json`

### 已确认的 manifest 契约

统一 manifest 文件名固定为：

```text
jazor-manifest.json
```

旧文件名废除，不再作为默认探测、默认输出或公开文档入口：

```text
jazor-manifest-razorvue.json
```

`Modules` 中每个 module 使用两层判别：

- `kind` 表示实际产物文件形态，当前取值为 `mjs` 或 `vue`
- `component.model` 表示组件 authoring/runtime 模型，当前取值为 `h` 或 `sfc`
- 没有 `component` 的 `mjs` 是普通 Jazor/ECMAScript module，不应被当作 RazorVue component

约定示例：

```json
{
  "kind": "mjs",
  "relativePath": "components/wiki-home.mjs"
}
```

```json
{
  "kind": "mjs",
  "relativePath": "components/counter-card.mjs",
  "component": {
    "model": "h"
  }
}
```

```json
{
  "kind": "vue",
  "relativePath": "components/counter-card.vue",
  "component": {
    "model": "sfc"
  }
}
```

`kind` 不再使用 `ecmascript` / `razorvue` 这类来源或技术线命名。文件形态用 `mjs` / `vue`，组件模型用 `h` / `sfc`，避免把普通 H 函数模块、H 组件和 SFC 组件混在同一个维度。

### Emit 侧工作项

- 修正 `ManifestModel`，将当前半迁移状态中的 `RazorVue` metadata 改为通用 `Component` metadata。
- 增加 `ManifestComponentModel.H` / `ManifestComponentModel.Sfc` 常量，并保持 `ManifestModuleKind.Mjs` / `ManifestModuleKind.Vue` 只表达文件形态。
- `ModuleWriter` 写普通 `.mjs` manifest 时必须保留已有 component entries，clean 只清理自己负责的普通 module，不能误删 RazorVue component manifest 项。
- `RazorVueModuleWriter` 写 H 组件时合并到统一 `jazor-manifest.json`，产物为 `kind = "mjs"`、`component.model = "h"`。
- `RazorVueSfcModuleWriter` 写 SFC 组件时合并到统一 `jazor-manifest.json`，产物为 `kind = "vue"`、`component.model = "sfc"`。
- `ModuleBundler` 不再读取 `RazorVueModuleWriter.GetManifestPath(...)` 产生的第二 manifest，而是从统一 manifest 投影 RazorVue component metadata。
- 已完成（2026-05-13 本轮）：`ModuleCollector` 允许同一 emit run 同时收集 RazorVue H catalog 与 SFC catalog，并保留“单程序集只能暴露一种 catalog shape”保护；跨模型 `componentId` / `relativePath` 冲突会显式失败。
- 已完成（2026-05-13 本轮）：`Jazor.Emit` 主流程不再在 H 与 SFC 之间二选一；clean/write 会分别处理 H 与 SFC，并在最终统一 manifest 基础上收敛生成单一 `__jazor/razorvue-host.mjs`。
- 已完成（2026-05-13 本轮）：`RazorVueModuleWriter` / `RazorVueSfcModuleWriter` 的宿主 metadata 生成逻辑收敛为共享 `RazorVueHostRequirementsModuleWriter`，避免 mixed 场景下 host requirements 被最后一个 writer 覆盖。
- 已完成（2026-05-13 本轮）：`ModuleBundler` 仅将非 component entries 视为 bundle 输入模块，RazorVue host requirements 则直接从统一 manifest 生成，不再依赖输入目录中预先存在的 `__jazor/razorvue-host.mjs`。
- 已完成（2026-05-14 本轮）：`ModuleWriter` 在统一 manifest 下已收敛为“普通 `.mjs` 仅保留未被同路径 plain module 接管的 component entries”；当 plain module 接管旧 RazorVue H 路径时，会同步移除旧 `component` metadata 与 `.origins.json` sidecar，避免后续 RazorVue clean 把新 plain module 误删。
- 已完成（2026-05-14 本轮）：MSBuild target 已移除公开的 `JazorRazorVueManifestPath` 默认值和 `jazor-manifest-razorvue.json` 引用；bundle 前快照统一收敛为 `JazorPreviousManifestSnapshotPath -> previous-jazor-manifest.json`，并由 `Jazor.targets` 直接传给 `bundle --previous-manifest`。
- 已完成（2026-05-14 本轮）：`razorvue-diff`、bundle update plan 与 host asset sidecar 现已统一从 `jazor-manifest.json` 的 component projection 读取输入；`ModuleBundlerTests` / `RazorVueManifestDifferTests` / `SdkIntegrationTests` 已锁定“projection missing / invalid”语义，避免回退到第二 manifest 契约。
- 已完成（2026-05-24 本轮）：`RazorVueManifestSerializer.Load(...)` / `TryLoad(...)` 直接支持从统一 manifest 的 `modules[].component` 投影到现有 `RazorVueManifestModel`，并可通过 `componentModel = "h"` / `"sfc"` 过滤 H 与 SFC component entries。plain `mjs` module 不参与 projection；投影时会校验 `component.model` 只能是 `h` / `sfc`，校验 `h -> kind:mjs`、`sfc -> kind:vue`，归一化 route/style/plugin requirement，并把缺失、空、rooted、drive-qualified 或 `..` 逃逸的 module path / sidecar path 报为 `Invalid`。该保护同时覆盖统一 manifest 的 `relativePath` / `sourceMapPath` / `component.originMapPath`，以及 legacy RazorVue manifest 的 `RelativeModulePath` / `SourceMapPath` / `OriginMapPath`；`modules: null`、`modules: [null]`、缺失必需 assembly/type/component identity 字段等 malformed module shape 会稳定返回 `Invalid`，避免未知 component 模型、不完整 component identity 或不安全 artifact path 污染 diff、bundle、SFC bridge 或 consumer entry。

### Consumer 与 SFC bridge 工作项

- `razorvue-consumer-entry` 默认 manifest 路径改为 `<hostJazorRoot>/jazor-manifest.json`。
- `razorvue-sfc-bridge` 默认 manifest 路径改为 `<hostJazorRoot>/jazor-manifest.json`。
- Deno bridge 读取统一 manifest shape，并只处理 `kind = "vue"` 且 `component.model = "sfc"` 的 module。
- consumer entry 组件选择逻辑只在 component entries 中匹配，普通 `mjs` module 不参与 `id:` / `name:` / `path:` component selector。
- consumer entry 生成 `razorVueConsumerRoutes`，其数据源是 selected component 的 `routeTemplates`；Playground 这类 consumer runtime 不再把 `router.js` 中的手写 path table 作为路由真相源。
- 已完成（2026-05-21 本轮）：`razorvue-consumer-entry` 的 route template -> Vue Router path 转换现已从“只支持 pure literal / pure parameter segment”的窄子集，扩到一条更真实的生产契约：支持 pure literal、`{parameter}`、`{parameter?}`、whole-segment default value（如 `{id=42}` / `{id:int=42}`）、受控 constraint（如 `{id:int}` / `{slug:alpha}` / `length(...)` / `regex(...)`）、不含 optional separator 的 mixed/composite segment（如 `post-{id}`、`post-{id:int}`），以及 catch-all（如 `{*path}`）。实现不再手写 Razor route token 字符串拆分，而是复用 ASP.NET Core 官方 `TemplateParser` 解析 route template，再转换到 Vue Router path regex 形状。
- 当前仍显式拒绝两类 route 形态：
  - default value 出现在 composite/mixed segment 内部，例如 `post-{id=42}`
  - 带 optional separator 的 composite/mixed segment，例如 `/files/{filename}.{ext?}`
  原因不是 parser 不会读，而是经真实 `vue-router` matcher / href-generation probe 校准后确认：这两类形态在当前 consumer/runtime 契约下无法诚实承载 ASP.NET Core 的参数提取与 URL 生成语义；继续 fail-fast 比“看起来能跑”的假支持更符合生产标准。
- 已完成（2026-05-21 本轮）：Playground consumer runtime 已不再维护独立的简化 path matcher / 手写 `:id` href 拼接规则；anchor interception 的 client-route 判定与 route href 生成现在统一复用 `vue-router` matcher 语义，并在其外层叠加 generated route metadata 中的 default-parameter contract。这样 generated `razorVueConsumerRoutes` 一旦使用 constrained/composite/catch-all/default-valued path，就不会出现“consumer entry 已支持、Playground 点击导航/生成 href 仍按旧窄规则误判”的生产漂移。
- 已完成（2026-05-21 本轮）：Playground consumer view-model 形状也已与当前 RazorVue 生成组件实际读取的 prop casing 对齐。consumer `view-models.js` 不再把 SSR/browser runtime 继续暴露 PascalCase DTO 样式对象，而是显式投影为生成产物当前消费的 camelCase shape，避免 `props.model.TotalExamples` / `DetailHref` 与生成组件实际读取的 `props.model.totalExamples` / `detailHref` 再次漂移。
- 错误信息应继续明确区分“manifest 不存在”“manifest 没有组件”“selector 无匹配”“selector 匹配多个组件”，不能因为统一 manifest 降低诊断质量。
- Playground colocated `consumer` 目录继续保留；它是单 .NET 项目中的前端消费构建层，不是第二个运行时 host。命名上使用 `consumer`，不再使用 `playground-consumer` 这类项目特化名称。
- 已完成（2026-05-13 本轮）：`razorvue-consumer-entry` 在 mixed H/SFC 场景下按 `component.model` 分流，H 组件直接 default import host `.mjs`，SFC 组件才进入 bridge。
- 已完成（2026-05-13 本轮）：`razorvue-consumer-entry` 只把“被选择的 SFC 组件集合”传给 `razorvue-sfc-bridge`，不再因为 manifest 中未选中的坏 SFC 而整体失败。
- 已完成（2026-05-13 本轮）：`razorvue-sfc-bridge` 支持显式 entry module path 过滤，并保留相对 `.vue` 依赖闭包编译，保证选中的 SFC 组件间引用仍能稳定工作。
- 已完成（2026-05-14 本轮）：统一 manifest 的 RazorVue component projection 诊断已细分为“manifest 不存在”“manifest 存在但没有 RazorVue component entries”“selector 无匹配”“selector 匹配多个组件”；`razorvue-diff` 的缺失原因也改为统一 Jazor manifest projection 语义，避免继续泄漏废弃的第二 manifest 公共契约。
- 已完成（2026-05-14 本轮）：`Playground` consumer runtime 已移除缺失 `routeDefinitions` 时的 legacy 手写路由回退；运行时现在必须消费 `razorvue-consumer-entry` 生成的 `razorVueConsumerRoutes`，确保 Razor `@page -> unified manifest routeTemplates -> consumer runtime` 是唯一路由真相源。
- 已完成（2026-05-14 本轮）：sample / pure Deno consumer runtime 已与官方 `razorvue-consumer-entry` 的三参调用契约对齐；运行时会把第 3 个参数识别为 route metadata，而不是误当成 `app.mount(...)` selector，修复了浏览器 smoke 中的 `parent.insertBefore is not a function` 挂载错误。
- 已完成（2026-05-14 本轮）：SDK colocated consumer 模板运行时也已对齐同一三参契约；`Publish_LocalJazorPackage_WebSdkHost_WithColocatedConsumer_UsesSdkConsumerBuildAndUnifiedJazorPublishRoot` 现会回归锁定 `razorVueConsumerRoutes` 调用与 `Array.isArray(routesOrSelector)` 兼容逻辑，防止模板再次漂移回旧 selector-only 签名。
- 已完成（2026-05-14 本轮）：sample 与 external pure Deno 的 SSR runtime export 也已显式声明并透传第 3 个 `razorVueConsumerRoutes` 参数，避免“浏览器 runtime 已升级、SSR runtime 仍停留在旧双参签名”的模板契约漂移。
- 已完成（2026-05-14 本轮）：Playground consumer 的 browser/SSR runtime 入口已显式校验 `CatalogPage` / `DetailPage` 必需组件导出；当 selector 配置或生成入口退化时，会在入口层给出稳定错误，而不是等到更深层渲染/SSR 过程里以模糊异常失败。

### ASP.NET Core 宿主工作项

- `UseJazorWebAssets()` 默认只依赖标准 `jazor-manifest.json` 即可挂载开发期 `/jazor/*` 输出。
- `DevelopmentEntryModuleRelativePath = "jazor-manifest-razorvue.json"` 这类样例必需配置应移除；高级项目仍可通过 option 覆盖 readiness probe。
- `JazorDevelopmentAssetOptions` 默认探测列表移除旧 manifest 文件名，避免继续把废弃文件作为隐式契约。
- `wwwroot` 静态文件使用 ASP.NET Core 默认 web root 机制，不在 Playground/Wiki 私有代码里手写特殊处理。
- `JazorWebApplication.CreateBuilder(args)` 作为源码运行与发布运行的内容根 helper，可供 Playground 和 Wiki 共用。
- `UseJazorHost(...)` 作为默认宿主入口，统一挂载通用安全头、标准静态文件、source map content type、开发期 Jazor 输出；项目仅在需要时覆盖站点特有 cache/header 策略。
- `UseJazorWebAssets(...)` 继续作为更细粒度的低层挂载 API 存在，但不再要求 Playground/Wiki 这类标准宿主手写组合样板。
- `UseJazorSpaFallback(...)` 继续负责 SPA navigation fallback；Wiki 如果需要 SEO shell 和 discovery document，可以保留项目特定 HTML shell 逻辑，但不应复制静态资源挂载和 Jazor output 探测逻辑。
- `UseJazorSpaFallback("/index.html")` 这类静态页面回退应作为官方宿主 API 提供：标准 SPA 宿主可以直接复用 `wwwroot/index.html`，而不必总是手写 `HttpContext -> WriteHtmlAsync` 委托。
- 已完成（2026-05-13 本轮）：`JazorDevelopmentAssetOptions` / `JazorWebAssetOptions` 默认探测仅保留 `jazor-manifest.json`，`Playground` smoke 不再显式依赖旧 manifest 文件名是否 404。
- 已完成（2026-05-13 本轮）：Wiki 的 `/vendor/*` 长缓存策略已从项目私有 `OnPrepareResponse` delegate 收敛为 `UseJazorHost(...).WebAssets.ImmutableCachePathPrefixes` 声明式 option；标准宿主不再手写基础静态资源 header 逻辑。
- 已完成（2026-05-14 本轮）：`Playground` 与 `samples/RazorVue.TodoList/Todo.Host` 均已收敛到 `UseJazorHost()` + `UseJazorSpaFallback("/index.html")` 默认宿主契约；`JazorAspNetCoreHostingTests` 新增默认单宿主组合回归，防止样例重新退回私有 `SendFileAsync` fallback。
- 已完成（2026-05-14 本轮）：开发期输出探针的公共宿主 API 已收敛为 `DevelopmentOutputProbeRelativePath` / `DevelopmentOutputProbeRelativePaths`，默认即为统一 `jazor-manifest.json`；旧 `EntryModuleRelativePath` 仅保留显式弃用的兼容别名，不再作为推荐语义。
- 已完成（2026-05-14 本轮）：高层 `UseJazorHost(...).WebAssets` 入口现已补齐 `DevelopmentOutputProbeRelativePath` 单值配置，与低层 `UseJazorDevelopmentAssets(...)` 保持同一公共 probe 语义；`JazorAspNetCoreHostingTests` 已锁定该高层入口不会退回到只能手改 list 的半收敛状态。

### Playground / Wiki 一致性工作项

- `Playground/Program.cs` 精简为 builder、服务注册、安全头、`UseJazorWebAssets(...)`、SPA fallback、API endpoint，不再包含 manifest 文件名或 Jazor output 细节。
- `Playground/Program.cs` 现已进一步收敛为 builder、服务注册、`UseJazorHost()`、SPA fallback、API endpoint，不再维护项目私有静态资产/安全头样板。
- `Wiki/Program.cs` 迁移到同一组 `Jazor.AspNetCore` helper，避免与 Playground 使用不同 API 设计。
- Wiki 可保留 host-rendered HTML shell、robots/sitemap、路径基址和目录完整性校验；这些是站点语义，不是 Jazor web asset 基础设施。
- 两个项目都应以默认配置跑起来，差异只体现在项目语义 option，而不是基础 Jazor host 契约。

### 验收标准

- 全仓库不再有默认输出、默认探测、测试断言或 smoke 脚本依赖 `jazor-manifest-razorvue.json`。
- `jazor-manifest.json` 同时覆盖普通 `mjs` module、H component module、SFC `vue` module。
- manifest clean 不误删不同 writer 负责的 module entries 或文件。
- SFC bridge、consumer entry、bundle、update plan 都从统一 manifest 工作。
- Playground smoke 访问 `/jazor/jazor-manifest.json` 成功，且不需要显式配置旧 manifest 路径。
- Wiki 和 Playground 使用一致的 ASP.NET Core helper API。
- 相关测试覆盖 manifest schema、merge/clean、consumer selection、SFC bridge filtering、ASP.NET Core default hosting。

### 当前状态

该项现已完成并由回归锁定：

- 统一 manifest 公开契约已稳定为 `jazor-manifest.json`
- `jazor-manifest-razorvue.json` 仅作为文档中的废弃历史名称保留，不再参与默认输出、默认探测或默认宿主运行
- `UseJazorDevelopmentAssets()` 默认探针与 `UseJazorHost()` 默认宿主契约现都显式拒绝把旧文件名当成 development readiness probe
- `Playground` / `Wiki` 已收敛到同一组 `Jazor.AspNetCore` helper，只在站点语义层保留差异化 option

## 9. 当前处理结论

这些问题没有阻断 `Playground` 落地，但都属于真实生产标准下必须正视的能力边界。

建议优先级：

1. default import/export 与 SFC bridge
2. library-mode 单项目 consumer pipeline 的 SDK/MSBuild/template 封装
3. ASP.NET Core + RazorVue library mode 标准模板
4. `/jazor/*` 多来源合并能力官方化

已落地并由回归保护的项：

- Razor IR 对纯静态多 token attribute 的稳定接受
- library component raw `class=` / `style=` fallthrough authoring 体验修复
- RazorVue SFC named-export bridge 官方化
- RazorVue consumer entry generation 官方化切片
- colocated consumer MSBuild build/publish contract 官方化
- ASP.NET Core SPA fallback/static-file 官方 helper
- ASP.NET Core 源码/发布双形态 content root helper
- `/jazor/*` 本地 webroot bundle + development emit 标准挂载 helper

## 10. 已完成：handwritten `BuildRenderTree` 模板局部变量支持

### 现象

此前 handwritten `BuildRenderTree` 中只允许 `RenderTreeBuilder` 别名局部变量。真实 authoring 中常见的模板内局部缓存/别名声明会失败，例如：

```csharp
var localTitle = Title;
```

```csharp
foreach (var item in Items!)
{
    var decorated = item + "!";
    builder.OpenElement(0, "span");
    builder.AddContent(1, decorated);
    builder.CloseElement();
}
```

```csharp
builder.AddAttribute(1, nameof(ChildCard.ItemTemplate), (RenderFragment<int>)((item) => (slotBuilder) =>
{
    var decorated = item + 1;
    slotBuilder.OpenElement(2, "span");
    slotBuilder.AddContent(3, decorated);
    slotBuilder.CloseElement();
}));
```

### 当前落地方式

该缺口已在 RazorVue handwritten `BuildRenderTree` frontend 与后续 lowering 链路中收口：

- render tree 增加 template-scoped local declaration 节点
- render tree 增加局部 template scope 节点，用于“立即调用的 typed fragment”
- canonical model 显式保留“声明后生效”的局部作用域顺序
- H lowering 使用片段级局部作用域/IIFE 保证单次求值与节点顺序
- SFC lowering 使用局部 template scope wrapper 保留同一顺序语义

因此以下场景现已稳定支持：

- 顶层片段局部值缓存/别名
- `for` / `foreach` body 中基于迭代变量的局部缓存
- typed slot template 中基于 slot 参数的局部缓存
- `AddContent(sequence, RenderFragment<T>, value)` 这种“立即调用 typed fragment + 实参”的局部模板作用域

### 当前支持边界

支持边界仍然刻意收窄为“带初始化器的不可变模板局部声明/局部模板作用域”：

- 支持：`var decorated = item + "!";`
- 支持：`string? localTitle; localTitle = Title;`，以及 `string? localTitle; var revision = 0; localTitle = Title;` 这种“声明后在同一线性局部声明前缀内完成一次简单赋值”的窄模式；仍按不可变 template local 处理
- 支持：`builder.AddContent(0, (RenderFragment<int>)(item => itemBuilder => { ... }), 42);`
- 不支持：声明后进入普通表达式语句、控制流、嵌套块、局部写入逃逸等，不再属于“同一线性局部声明前缀内一次简单赋值”的其他变体
- 不支持：`++` / `--` / 其他写入型模板局部状态
- 不支持：把模板局部声明当作匿名函数/委托状态载体继续扩散
- 不支持：把任意 delegate 值、动态 callable、外部 fragment 变量都放宽成可立即模板执行的 `AddContent(RenderFragment<T>, value)` 形态；当前要求源码 inline 且可分析

### 当前保护

- `src/Jazor.RazorVue.Test/BuildRenderTreeTemplateFrontendTests.cs`
- `src/Jazor.RazorVue.Test/RazorVueCanonicalSfcSemanticTests.cs`
- `src/Jazor.RazorVue.Test/RazorVueSfcArtifactFactoryTests.cs`
- `src/Jazor.RazorVue.Test/RazorVuePipelineTests.cs`

当前回归同时覆盖：

- 顶层局部声明成功
- 顶层“先声明后紧邻赋值”成功
- loop body 局部声明成功
- typed slot template 局部声明成功
- typed `AddContent(RenderFragment<T>, value)` 模板作用域成功
- “无初始化器但不能在同一线性局部声明前缀内完成一次简单赋值”的变体仍明确失败

在 Razor IR frontend 路线下，这一批 support gap 也继续向“复杂 code-block 的顺序控制恢复”扩展并已锁定：

- `local + if + if`
- `local + if + foreach`
- `local + foreach + if`
- `local + for + if`

这些组合都已覆盖 Razor IR inventory / operation resolver / frontend / parity / pipeline 回归。当前 contract 仍然是“显式支持的顺序控制恢复子集”，不是放宽成任意语句执行模型。

## 11. 已完成：handwritten `BuildRenderTree` render helper 额外值参数支持

### 现象

此前 handwritten `BuildRenderTree` 对当前组件/local render helper 只支持“单个 `RenderTreeBuilder` 参数”形态。下面这些真实 authoring 都会失败：

```csharp
protected override void BuildRenderTree(RenderTreeBuilder builder)
{
    RenderBody(builder, Title);
}

private void RenderBody(RenderTreeBuilder builder, string? title)
{
    builder.OpenElement(0, "section");
    builder.AddContent(1, title);
    builder.CloseElement();
}
```

```csharp
protected override void BuildRenderTree(RenderTreeBuilder builder)
{
    void RenderBody(RenderTreeBuilder localBuilder, string? title)
    {
        localBuilder.OpenElement(0, "section");
        localBuilder.AddContent(1, title);
        localBuilder.CloseElement();
    }

    RenderBody(builder, Title);
}
```

如果简单把 helper 参数直接替换成调用点实参，会破坏单次求值、副作用顺序和参数作用域边界；如果继续沿用共享 open-frame 解析，又会把“依赖调用方已打开节点/component frame”的 helper 一并放开，边界不安全。

### 当前落地方式

该缺口已在 RazorVue handwritten `BuildRenderTree` frontend 与后续 lowering 链路中收口：

- current-component/local render helper 现支持“恰好一个 `RenderTreeBuilder` 参数 + 额外普通按值参数”
- helper body 在 extra-parameter 场景下按独立片段解析，避免把调用方 open-frame 状态隐式透传进 helper
- render tree / canonical model 使用局部 template scope node 显式保留“helper 形参 <- 调用点实参”绑定
- H lowering 将 helper 参数编码为一次性立即调用作用域，保证单次求值与参数不泄漏
- SFC lowering 将 helper 参数编码为局部 `<template v-for="(...) in [...]">` scope wrapper，并修正了根级 template-scope close-tag 重复输出
- template-scoped local declaration 现在也允许在 helper body 中基于 helper 参数建立局部缓存/别名

因此以下场景现已稳定支持：

- `RenderBody(builder, Title)` 这种当前组件 helper 参数绑定
- `void RenderBody(RenderTreeBuilder localBuilder, string? title) { ... }` + `RenderBody(builder, Title)` 这种 local function helper 参数绑定
- `RenderBody(builder, Title, Subtitle)` / `void RenderBody(RenderTreeBuilder localBuilder, string? title, string? subtitle) { ... }` 这类 multiple extra parameter 绑定
- `RenderBody(title: Title, builder: builder)` / `RenderBody(title: Title, localBuilder: builder)` 这类 named argument 绑定
- `RenderBody(builder)` + helper optional default value 绑定
- helper body 中对参数的 element child / interpolation 使用
- helper body 中基于参数的模板局部缓存/别名
- helper body 中“额外参数 -> 模板局部缓存/别名 -> 后续节点引用”这类组合 authoring
- `for` / `foreach` body 中“loop 变量 -> helper 额外参数 -> helper 内模板局部缓存/别名 -> 后续节点引用”这类组合 authoring
- canonical / H / SFC 三条 lowering 链路对 helper 参数作用域的一致保留
- helper body 在“调用方已打开 element/component frame”场景下，对 caller-owned node 的受控 mutation：
  - `AddAttribute(...)`
  - `SetKey(...)`
  - `AddMultipleAttributes(...)`
- 上述 caller-owned mutation 不会把 helper 形参提前改写成调用点表达式，而是保留“helper 形参引用 + 节点级 captured binding”合同，再由 canonical / H / SFC 统一保证单次求值与正确作用域

### 当前支持边界

支持边界仍然刻意收窄为“源码可分析、按值参数、要么 helper 自身可独立 canonicalize，要么 helper 只在 caller-owned open node 上执行受控 mutation 协议”的 render helper：

- 支持：`private void RenderBody(RenderTreeBuilder builder, string? title) { ... }`
- 支持：`void RenderBody(RenderTreeBuilder localBuilder, string? title) { ... }`
- 支持：`private void RenderBody(RenderTreeBuilder builder, string? title, string? subtitle) { ... }`
- 支持：named argument / builder 参数不在第一个位置，只要调用点参数与声明一一对应
- 支持：省略 optional parameter 且默认值可安全投影到当前 template/canonical 边界
- 支持：`params` 数组参数，只要 Roslyn 调用绑定仍是“单个数组形参 <- 单个数组实参”语义；expanded/empty `params` 调用会按正常数组创建结果进入 RazorVue，而不会被额外拍平
- 支持：多个额外值参数按调用点实参求值顺序形成嵌套 template scope / 嵌套 IIFE，同时保持 helper 形参与实参的正确绑定；named argument 打乱声明顺序时不会退化成错误重排
- 支持：`params` 数组实参在 canonical/H 路径保留为数组表达式；SFC 路径若模板侧不能直接内联该数组初始化，会提升为 setup binding（例如 `__jazor$0 = computed(() => [props.title, "suffix"])`）后再进入局部 `<template v-for>` scope wrapper
- 支持：`for` / `foreach` body 中使用 loop 变量调用 helper 时，loop 变量可作为 helper 实参稳定进入后续 helper parameter scope；不会因为 loop/template scope 叠加而丢失绑定或错误提升
- 支持：调用方已打开 element/component frame 时，extra-parameter helper 只做 caller-owned node mutation 的窄子集：
  - `AddAttribute(...)`
  - `SetKey(...)`
  - `AddMultipleAttributes(...)`
- 支持：调用方已打开 element/component frame 时，extra-parameter helper 在保持同一 caller-owned open node/frame 的前提下，混合执行：
  - 上述 caller-owned mutation
  - 继续向同一 open node 追加 child emission，例如在 helper 内 `OpenElement(...) ... CloseElement()` 后回到原 caller-owned frame
- 支持：这类 mixed mutation + child emission 走 invocation-scoped replay contract，先在 render tree 中保留“helper 形参引用 + captured binding + ordered replay operations”，再由 lowering 统一保证单次求值；不会把同一个 helper 实参在多个 attribute/child 位置重复内联求值
- 支持：上述 caller-owned helper 若目标 frame 是 component，当前 replay contract 也已正式覆盖 default-slot 语义，而不是再把 default-slot subtree 错当成普通 `children`：
  - `AddAttribute(..., "ChildContent", ...)` / `AddComponentParameter(..., "ChildContent", ...)` 形成的 implicit default-slot assignment，会在 replay 中保留为显式 default-slot assignment，再由 imperative render path 发射成 `setComponentParameter("ChildContent", () => ...)`
  - helper 在 caller-owned component frame 上直接 `OpenElement(...)` / `AddContent(...)` 落下的 ambient default-slot child，也会在 replay 归一化阶段折叠为“一个 default-slot fragment”，而不是回放成普通 `append(...)`
  - 因此 component caller-owned helper 的 default-slot 与 ambient child 现在都保留 RazorVue 自己的 slot contract；不会再出现 render tree 结构是 default slot，但最终 render-function 产物把它们误 materialize 到 component raw children 的语义漂移
- 支持：上述 caller-owned helper 若目标 frame 是 component，当前 replay contract 也已正式覆盖命名 slot / typed slot 参数，而不是只覆盖 default slot：
  - `AddAttribute(..., "Header", ...)` / `AddComponentParameter(..., "Header", ...)` 这类命名 slot 赋值，会保留为显式 slot template replay，并在 render-function path 发射成 `setComponentParameter("Header", () => ...)`
  - `AddAttribute(..., "ItemTemplate", ...)` / `AddComponentParameter(..., "ItemTemplate", ...)` 这类 typed/scoped slot 赋值，会保留 slot context 参数与 helper captured-value scope，并在 render-function path 发射成 `setComponentParameter("ItemTemplate", (item) => ...)`
  - 这条 contract 现在同时锁定 current-component helper 与 `BuildRenderTree` local function helper；不会再出现“default slot 正确 replay，但 named/typed slot 回退成普通 prop/children”的层间漂移
- 支持：若 caller-owned component helper 做的不是 inline slot/template materialization，而是“当前组件 `[Parameter] RenderFragment...` -> 子组件 slot 参数”的 slot forwarding，且该 forwarding 仍可诚实 canonicalize，则 RazorVue 会继续保持声明式 forwarded-slot lowering，而不是无谓切到 replay/render-function：
  - 例如 helper 内 `builder.AddAttribute(nameof(ListCard.ItemTemplate), ItemTemplate)` 仍会保持 typed forwarded-slot 语义，最终 lower 成 `itemTemplate: (context) => slots.itemTemplate ? slots.itemTemplate(context) : null`
  - helper 内 `builder.AddComponentParameter(nameof(NavShell.Header), Header)` 这类命名/scoped slot forwarding 也同样保持声明式 slot binding，不会因为 helper 抽取而退化成 imperative bridge 或普通 prop 值
  - 因此当前合同不是“component caller-owned helper 一旦碰到 slot 就统一走 render-function”，而是“只有 inline slot/template assignment 需要 replay 时才切 imperative；纯 forwarding 仍优先保持 canonical declarative path”
- 支持：上述 caller-owned mutation 在 render tree 中保留 helper 形参与 captured binding，H 路径会在安全的 identity case 直接折叠回调用点实参（例如 `"class": props.title`、`"key": props.title`），而 spread 继续走既有 `__jazorVueMergeAttributes(...)` 路径
- 支持：当 invocation-scoped replay 不能被 template/SFC template block 诚实表达时，canonical/H/SFC 会显式切到 render-function / imperative render path，而不是把 helper 参数作用域错误 hoist 到 template 外层
- 支持：attribute / key / event modifier 这类 scoped replay 本身不再触发 render-function / imperative bridge promotion；它们可以继续通过 captured expression、props merge helper 或 event wrapper 在 declarative path 中表达。只有 scoped replay 内出现 slot assignment、ambient child、default-slot fragment、child replay 等需要 open-frame 回放的结构性操作时，才会切到 render-function / imperative render path
- 支持：caller-owned helper 内部的 helper-local 平衡 `OpenRegion(...)` / `CloseRegion()`，只要 region 不逃逸、不改变进入 helper 时的 caller-owned open node/frame，且最终仍回到同一 caller-owned frame。该 region 是 Razor frame-shape 边界，不是 Vue artifact 必须保留的运行时节点；进入 invocation-scoped replay 后最终 `.mjs` / render-function `.vue` 可以把它擦除为同一个 child vnode replay，例如 element frame 上的 `append(h("span", null, title))`。当目标 frame 是 component 时，同样的 region 包裹 ambient child emission 会先归一化为 default-slot fragment replay，再由 render-function path 发射为 `setComponentParameter("ChildContent", () => ...)`，不会退化成普通 component children，同时继续保留 helper captured binding 与 child emission 顺序。
- 不支持：`ref` / `out` / `in`
- 不支持：caller-owned open node 协议中的 open/close/region/frame-shape 变更：
  - `OpenElement` / `OpenComponent`
  - `CloseElement` / `CloseComponent`
- 不支持：caller-owned helper 内部 region 逃逸、不平衡，或需要跨 helper 推断 caller-owned frame shape 的 region/open-frame rewrite
- 不支持：helper 改变最终 active caller-owned open frame，或结束时没有回到进入时的同一 open node/frame
- 不支持：超出当前 replay contract 的 caller-owned frame 协议变更，例如跨 helper 留下未闭合 frame、切换到其他 caller-owned open node 再返回、或需要 region/open-frame 结构推断的 shape rewrite
- 不支持：caller-owned mutation helper 结束后 frame depth 或 active open node 与进入时不一致；这类情况会显式 fail-fast，而不是静默猜测调用方协议
- 不支持：递归 render helper

### 当前保护

- `src/Jazor.RazorVue.Test/BuildRenderTreeTemplateFrontendTests.cs`
- `src/Jazor.RazorVue.Test/RazorVueCanonicalSfcSemanticTests.cs`
- `src/Jazor.RazorVue.Test/RazorVueSfcArtifactFactoryTests.cs`
- `src/Jazor.RazorVue.Test/RazorVuePipelineTests.cs`

当前回归同时覆盖：

- frontend 产出 helper 参数 template scope node
- frontend 产出 current-component / local function helper 参数 template scope node
- frontend / canonical / H / SFC 对 multiple extra parameter 的嵌套作用域与调用点实参求值顺序保持一致
- frontend / canonical / H / SFC 对“helper 参数作用域 + helper body 内模板局部声明”组合语义保持一致
- frontend / canonical / H / SFC 对“loop scope + helper 参数作用域 + helper body 内模板局部声明”组合语义保持一致
- frontend / pipeline 对 caller-owned `AddAttribute` / `SetKey` / `AddMultipleAttributes` helper mutation 保持一致
- frontend / canonical / SFC / pipeline 对 caller-owned helper 的“attribute mutation + child emission”保持一致
- frontend / canonical / SFC / pipeline 对 caller-owned component helper 的 implicit default-slot assignment 与 ambient default-slot child 保持一致，render-function path 会统一 materialize 成 `ChildContent` slot callback，而不是普通 component children
- frontend / canonical / SFC / pipeline 对 caller-owned component helper 的 named slot / typed slot assignment 保持一致，render-function path 会统一 materialize 成对应 slot callback，并保留 scoped-slot context 参数
- current-component helper 与 `BuildRenderTree` local function helper 在 caller-owned component named/typed slot assignment 上保持 parity；local-function 路径不会退回 prop 赋值或丢失 helper captured bindings
- current-component helper 在 caller-owned component slot forwarding 上保持“能 declarative 就 declarative”的合同：typed/named slot forwarding 不会仅因 helper 抽取就被误提升为 imperative root program
- invocation-scoped replay 命中时，canonical / SFC 会切到 render-function，而不是继续错误尝试 template lowering
- attribute/key/event scoped replay 不会误触发 imperative promotion；`AddMultipleAttributes(...)` 在 declarative 和 mixed imperative sibling 中继续走 `__jazorVueMergeAttributes(...)`，并由 module/SFC builder 按实际 body 引用注入 helper
- caller-owned helper 内部平衡 `OpenRegion` / `CloseRegion` 已锁定为“frontend 接受并归一化为 child replay/default-slot fragment replay，canonical/SFC/pipeline 进入 render-function，并在最终 Vue artifact 中擦除 helper-local region frame”的合同；element child 与 component ambient default-slot child 两条路径均有回归保护
- canonical model 保留 `title <- props.title` 绑定
- named argument 绑定稳定工作
- omitted optional default value 绑定稳定工作
- H lowering 输出 helper 立即调用作用域
- SFC lowering 输出局部 template scope wrapper，且不再重复闭合 `</template>`
- caller-owned mutation helper 若试图 close/open caller frame 或留下不平衡 frame，会继续明确失败

## 13. handwritten `AddContent(RenderFragment<T>, value)` 的 typed fragment carrier 需要稳定边界

### 现象

此前 handwritten `BuildRenderTree` 对 typed fragment 只稳定支持“调用点直接内联匿名模板”：

```csharp
builder.AddContent(0, (RenderFragment<int>)(item => itemBuilder =>
{
    itemBuilder.AddContent(1, item);
}), 42);
```

但真实 authoring 很自然会写成局部 carrier：

```csharp
RenderFragment<int> template = item => itemBuilder =>
{
    itemBuilder.AddContent(1, item);
};

builder.AddContent(0, template, 42);
```

旧实现会把 `template` 误判为普通 template-scoped local，然后因为“callable template state”保护在声明阶段直接失败。

### 当前落地方式

该缺口已在 handwritten `BuildRenderTree` extractor 中收口：

- `RenderFragment` / `RenderFragment<T>` 局部变量会先按“局部 fragment carrier”单独识别，而不是落入普通 template-scoped local 规则
- carrier 只接受源码可分析 initializer：
  - inline anonymous fragment
  - 或引用先前已解析的本地 fragment carrier
- `AddContent(sequence, RenderFragment<T>, value)`、slot template 等后续解析会优先消费该 carrier 映射
- 普通 template-scoped local 仍保持“不允许 callable template state”保护，不会因为这次支持而被整体放宽

### 当前支持边界

- 支持：inline typed fragment
- 支持：同一可分析作用域内、初始化即为可分析匿名模板的局部 `RenderFragment<T>` carrier
- 支持：该局部 carrier 既可用于 `AddContent(sequence, RenderFragment<T>, value)`，也可用于组件 typed slot/template 参数
- 支持：current-component 上的只读 expression-bodied property、声明点 initializer 的 getter-only auto-property、单返回 getter property、以及 `readonly` field 形式的 `RenderFragment<T>` carrier，只要其初始化器仍可静态还原到匿名模板
- 支持：声明点 initializer 的 private settable property / private 非 `readonly` field 形式的 `RenderFragment<T>` carrier，只要源码中不存在后续重赋值、`ref/out` 写入或其他可观察写入
- 支持：上述局部 / current-component 受控 carrier 也可以把“受支持的 current-component method / local function fragment factory 调用结果”作为初始化器承载；例如 `RenderFragment<int> template = CreateTemplate(Title);`、只读 property 返回 `CreateTemplate(Title)`、或 `readonly` field 持有该结果
- 支持：current-component method / local function 的零参数 fragment factory，只要返回值本身仍可静态还原到匿名模板
- 支持：current-component method / local function 的“普通按值参数 fragment factory”可直接用于：
  - `builder.AddContent(0, CreateTemplate(Title), 42);`
  - 组件 typed slot/template 参数，例如 `builder.AddAttribute(1, "ItemTemplate", CreateTemplate(Title));`
- 支持：Razor authored template expression 里的 direct untyped factory consumption；受支持的 current-component method / local function fragment factory 返回 `RenderFragment` 时，可以直接写成 `@CreateTemplate(Title)`、`@CreateTemplate()`、`@CreateTemplate(subtitle: Subtitle, title: Title)`，并保持“外层 captured 参数 scope + 内层结构化 render subtree”的同一语义；named argument out-of-order 仍按调用点求值顺序保留外层 scope 包裹顺序
- 支持：generic current-component method / local function fragment factory，只要调用点已经被 Roslyn 绑定为具体构造方法实例，且返回模板形状本身仍可静态还原；其缓存语义与 non-generic 一致，仍是“缓存源码定义模板骨架，调用点单独绑定 captured 参数”
- 支持：current-component method / local function 的 `params` 数组 fragment factory；expanded / empty `params` 调用会保留为单个数组绑定结果，而不是在 RazorVue 前端自行拍平成多个逻辑形参
- 支持：带参数 fragment factory 的调用结果即使先经过上述受控 carrier，再用于 `AddContent(...)` 或组件 typed slot/template 参数，frontend / canonical / H / SFC 四层也会保持与直接调用点一致的作用域语义
- 支持：带参数 fragment factory 在 frontend / canonical / H / SFC 中保留额外参数局部作用域；named argument 打乱书写顺序时，仍按调用点左到右求值顺序保留外层 scope 包裹，而不是按形参声明顺序重排求值
- 支持：同一个带参 fragment factory 在同一组件内被多个不同调用点重复使用时，RazorVue 只复用模板骨架，不复用某一次调用点的 captured 值绑定；`CreateTemplate(Title)` 与 `CreateTemplate(Subtitle)` 这类多次调用会各自保留独立外层 scope
- 支持：若带参 fragment factory 的 captured 参数是数组创建（典型如 `params` expanded call），SFC 路径会在需要时把该数组初始化提升为 setup binding，再通过局部 template scope wrapper 消费，以避免模板内重复构造
- 支持：generic render helper 也进入同一受支持子集，只要调用点已绑定到具体构造方法实例，且 helper 仍满足现有 builder 协议与 self-contained fragment 约束
- 支持：frontend / canonical / H / SFC 对局部 carrier 与 inline 形态保持相同 lowering 结果
- 支持：当前组件 slot outlet / slot forwarding 仅从 `[Parameter] RenderFragment...` 属性识别
- 支持：当前组件 `[Parameter] RenderFragment?` 转发到子组件默认/未参数化 slot
- 支持：当前组件 `[Parameter] RenderFragment<T>?` 转发到子组件 typed/scoped slot，frontend / canonical / H / SFC 四层保留 forwarded-slot 语义与目标 slot context 参数名
- 不支持：任意 delegate 值流分析
- 不支持：递归 fragment factory
- 不支持：动态重赋值后的 carrier
- 不支持：无法静态还原到匿名模板 body 的 callable 形态

### 当前保护

- `src/Jazor.RazorVue.Test/BuildRenderTreeTemplateFrontendTests.cs`
- `src/Jazor.RazorVue.Test/RazorVueCanonicalSfcSemanticTests.cs`
- `src/Jazor.RazorVue.Test/RazorVueSfcArtifactFactoryTests.cs`
- `src/Jazor.RazorVue.Test/RazorVuePipelineTests.cs`

当前回归同时锁定：

- typed fragment local carrier frontend 产出 template scope node
- canonical model 与 inline typed fragment 保持相同 `item <- 42` 作用域语义
- H lowering 与 inline typed fragment 保持相同立即调用输出
- SFC lowering 与 inline typed fragment 保持相同局部 template scope wrapper
- current-component / local zero-argument fragment factory 与 inline typed fragment 保持相同作用域与 lowering 结果
- direct `AddContent(...)` 与组件 typed slot/template 参数上的 parameterized fragment factory 均已支持，并在 frontend / canonical / H / SFC 四层保持一致作用域语义
- parameterized fragment factory 结果即使先通过局部 carrier、只读 property 或 `readonly` field 承载，再被 direct `AddContent(...)` 或组件 typed slot/template 参数消费，四层仍保持“外层 captured-value scope、内层 typed fragment scope”的一致顺序
- parameterized fragment factory 的 named-argument 路径已锁定为“按调用点左到右求值顺序保留嵌套 scope”，避免回退成按形参声明顺序重排
- parameterized fragment factory 的“同方法多调用点”路径已锁定为“仅缓存模板骨架，不缓存调用点 captured binding”；Razor IR frontend 不会再把第一次调用的 captured 参数错误复用到后续不同调用点
- 当前组件 `[Parameter] RenderFragment...` 作为 slot forwarding source 时，默认 slot 与 typed/scoped slot 两条路径都已锁定回归；typed forwarding 会按目标子组件 slot contract 保留 context 参数名，而不是退化成普通值表达式或错误的无参 slot template
- private settable property / private 非 `readonly` field carrier 的“声明点初始化 + 无后续写入”窄子集已在 handwritten / Razor IR / pipeline / SFC 四层锁定；一旦源码里出现后续重赋值，仍显式失败
- recursive fragment factory 仍显式失败

## 14. 已完成：handwritten `BuildRenderTree` 静态 `AddMarkupContent(...)` 标记支持

### 现象

此前 handwritten `BuildRenderTree` 对下面这种真实 authoring 会直接失败：

```csharp
builder.AddMarkupContent(0, "<section class=\"hero\"><span>safe</span><p>ok</p></section>");
```

这类片段本质上是静态 HTML subtree，并不需要执行任意 raw HTML/runtime script 语义；如果完全拒绝，会迫使真实项目把原本清晰的静态 markup 改写回繁琐的 `OpenElement` / `AddAttribute` / `AddContent` 序列。

### 当前落地方式

该缺口已在 RazorVue handwritten `BuildRenderTree` frontend 与 imperative render lowering 主线中收口：

- `AddMarkupContent(...)` 在第二参数可证明为常量字符串时，会交给共享静态标记解析器还原为 render tree subtree
- 当前已继续扩展为“编译期可证明静态 markup carrier”子集：
  - `const string markup = "<section ...>...</section>"; builder.AddMarkupContent(..., markup);`
  - `string markup = "<section ...>...</section>"; builder.AddMarkupContent(..., markup);`
  - `string markup; markup = "<section ...>...</section>"; builder.AddMarkupContent(..., markup);`
  - current-component / local function factory 返回同类静态 string markup，再由 `AddMarkupContent(...)` 直接消费，例如 `builder.AddMarkupContent(..., CreateMarkup());`、`builder.AddMarkupContent(..., CreateMarkup(Title));`
  - 受控 member/local carrier 再转发同类 static-markup factory 返回值，例如 `private string HeroMarkup => CreateMarkup(); builder.AddMarkupContent(..., HeroMarkup);`
  - 只读 expression-bodied property / getter-only property / `readonly` field 承载同类静态 string markup，再由 `AddMarkupContent(...)` 消费
  - private settable property / private 非 `readonly` field 承载同类静态 string markup，只要源码中可证明不存在后续写入，也可由 `AddMarkupContent(...)` 消费
- imperative body 内同样的静态 string direct/local/readonly-member carrier authoring，以及源码可分析 static-markup factory 返回值，也会直接 lower 为 `h(...)` subtree；若 factory 带普通按值参数，则会保留调用点实参求值顺序并通过 captured scope/IIFE 包裹最终静态 subtree
- 共享静态标记解析器同时被 Razor IR frontend 复用，保证静态 HTML 片段在两条 frontend 路径上的 element/text/attribute 还原语义一致
- 还原出的 subtree 继续走现有 canonical / H / SFC lowering 链路，不额外引入 raw-html 特判分支
- 若 `AddMarkupContent(...)` 内容不是编译期可证明静态 markup，则 declarative 路径显式报 `CanonicalizationFailed`，imperative 路径显式报 `UnsupportedImperativeRenderLowering`；不会再静默当成普通 string text 或 raw helper 调用放行

因此以下场景现已稳定支持：

- 常量 `AddMarkupContent(...)` 静态 element subtree
- local/readonly-member `AddMarkupContent(...)` static carrier subtree
- 静态 attribute、嵌套 element、void element/self-closing element
- 静态文本节点与 HTML comment 跳过

### 当前支持边界

- 支持：`builder.AddMarkupContent(0, "<section class=\"hero\"><span>safe</span></section>");`
- 支持：local `const` string、普通 declaration-initialized string、以及“先声明、再在同一线性局部声明前缀内完成一次简单赋值”的 source-stable string local，再由 `AddMarkupContent(...)` 消费
- 支持：current-component / local function factory 返回编译期可证明静态 string markup，再由 `AddMarkupContent(...)` 直接消费；普通按值参数、omitted optional default，以及按 Roslyn 绑定为单数组形参的 `params` 调用都支持，只要返回 markup 仍可静态证明
- 支持：受控 property/field/local carrier 再转发上述 static-markup factory 返回值，只要中间 carrier 仍满足同一 source-stable / controlled-member 合同
- 支持：只读 property / `readonly` field 承载的编译期可证明静态 markup
- 支持：private settable property / private 非 `readonly` field 承载的编译期可证明静态 markup，只要源码中不存在后续重赋值、`ref/out` 写入或其他可观察写入
- 支持：imperative body 内同样的静态 string direct/local/readonly-member carrier authoring，以及源码可分析 static-markup factory 返回值；带普通按值参数、omitted optional default 或 `params` 单数组绑定时都保留调用点 captured-binding 求值顺序
- 不支持：上述 string local / member carrier 在初始化后再次出现可观察写入；这类场景继续按 source-stable 合同显式 fail-fast，而不是静默沿第一次赋值恢复旧静态 subtree
- 不支持：运行时拼接/动态 markup
- 不支持：`ref/out/in`、实参与形参无法按当前合同绑定、或返回值本身已不再可静态证明的 static-markup factory
- 不支持：需要当作任意 raw HTML script 语义执行的内容
- 不支持：结构非法、闭合不匹配或无法解析的静态 markup；这类输入仍显式失败，而不是静默降级成字符串注入

### 当前保护

- `src/Jazor.RazorVue/RenderTree/RazorVueStaticMarkupParser.cs`
- `src/Jazor.RazorVue/RenderTree/RazorVueRenderTreeExtractor.cs`
- `src/Jazor.RazorVue/RazorSdk/RazorVueRazorIrTemplateFrontend.cs`
- `src/Jazor.RazorVue.Test/BuildRenderTreeTemplateFrontendTests.cs`
- `src/Jazor.RazorVue.Test/RazorVuePipelineTests.cs`
- `src/Jazor.RazorVue.Test/RazorVueSfcArtifactFactoryTests.cs`

当前回归同时锁定：

- declarative `BuildRenderTree` 中常量 `AddMarkupContent(...)`
- declarative `BuildRenderTree` 中 local/readonly-member `AddMarkupContent(...)` carrier
- declarative `BuildRenderTree` 中 declaration-initialized / immediate-assignment string local `AddMarkupContent(...)` carrier
- declarative `BuildRenderTree` 中 current-component static-markup factory（含普通按值参数 captured-binding 保真）
- declarative / imperative 中“private mutable + 无后续写入”的 static markup member carrier
- declarative `BuildRenderTree` 中动态 `AddMarkupContent(...)` carrier fail-fast
- declarative `BuildRenderTree` 中 immediate-assignment string local 后续再次写入的 source-stable fail-fast
- imperative render bridge 中静态 `AddMarkupContent(...)` direct/local/readonly-member carrier
- imperative render bridge 中 current-component static-markup factory（含普通按值参数 captured-binding / IIFE 保真）
- imperative render bridge 中 declaration-initialized / immediate-assignment string local `AddMarkupContent(...)` carrier
- imperative render bridge 中动态 `AddMarkupContent(...)` carrier fail-fast

## 15. 已完成：静态 `MarkupString` `AddContent(...)` 静态标记支持

### 现象

此前 handwritten `BuildRenderTree` 对下面这类真实 authoring 仍会失败：

```csharp
builder.AddContent(0, (MarkupString)"<section class=\"hero\"><span>safe</span></section>");
builder.AddContent(0, new MarkupString("<section class=\"hero\"><span>safe</span></section>"));
```

这些写法在 Blazor authoring 中很常见，本质上仍是“编译期可证明的静态 markup subtree”。如果这条路径继续失败，用户就必须把同一段静态 HTML 改写回 `AddMarkupContent(...)` 或更底层的 `OpenElement`/`CloseElement` 序列，authoring contract 不稳定。

### 当前落地方式

该缺口已在 RazorVue handwritten `BuildRenderTree` frontend 与 imperative render lowering 主线中共同收口：

- 新增共享静态 markup value helper，只接受“编译期可证明为静态 markup”的 `MarkupString` 形态
- 当前已覆盖：
  - `(MarkupString)"<section ...>...</section>"`
  - `new MarkupString("<section ...>...</section>")`
- 当前已继续覆盖 Razor authored template expression 路径：
  - `@((MarkupString)"<section ...>...</section>")`
  - `@(new MarkupString("<section ...>...</section>"))`
- 当前已继续覆盖受控 carrier：
  - `MarkupString markup = (MarkupString)"<section ...>...</section>"; builder.AddContent(..., markup);`
  - `MarkupString markup; markup = (MarkupString)"<section ...>...</section>"; builder.AddContent(..., markup);`
  - current-component / local function factory 返回同类静态 `MarkupString`，再由 `AddContent(...)` 直接消费，例如 `builder.AddContent(..., CreateMarkup());`、`builder.AddContent(..., CreateMarkup(Title));`
  - 受控 member/local carrier 再转发同类 static-markup factory 返回值，例如 `MarkupString markup; markup = CreateMarkup(); builder.AddContent(..., markup);`
  - 只读 expression-bodied property / getter-only property / `readonly` field 承载同类静态 `MarkupString`，再由 `AddContent(...)` 消费
  - private settable property / private 非 `readonly` field 承载同类静态 `MarkupString`，只要源码中可证明不存在后续写入，也可由 `AddContent(...)` 消费
  - Razor authored template 中局部 / 受控成员 carrier 的 `@markup` / `@HeroMarkup` / `@_heroMarkup`
- 命中该子集时，会复用现有 `RazorVueStaticMarkupParser` 还原为正常 render tree subtree / imperative `h(...)` subtree
- 若 `MarkupString` 不是编译期可证明静态值，则继续显式失败，而不是伪装成已支持的 raw-html 注入

### 当前支持边界

- 支持：`builder.AddContent(0, (MarkupString)"<section class=\"hero\"><span>safe</span></section>");`
- 支持：`builder.AddContent(0, new MarkupString("<section class=\"hero\"><span>safe</span></section>"));`
- 支持：`MarkupString markup = (MarkupString)"<section class=\"hero\"><span>safe</span></section>"; builder.AddContent(0, markup);`
- 支持：`MarkupString markup; markup = (MarkupString)"<section class=\"hero\"><span>safe</span></section>"; builder.AddContent(0, markup);`
- 支持：current-component / local function factory 返回静态 `MarkupString`，再由 `AddContent(...)` 直接消费；普通按值参数、omitted optional default，以及按 Roslyn 绑定为单数组形参的 `params` 调用都支持，只要返回 markup 仍可静态证明
- 支持：受控 property/field/local carrier 再转发上述静态 `MarkupString` factory 返回值，只要中间 carrier 仍满足同一 source-stable / controlled-member 合同
- 支持：只读 property / `readonly` field 承载静态 `MarkupString`，再由 `AddContent(...)` 消费
- 支持：private settable property / private 非 `readonly` field 承载静态 `MarkupString`，只要源码中不存在后续重赋值、`ref/out` 写入或其他可观察写入，再由 `AddContent(...)` 消费
- 支持：Razor authored template expression 中等价的静态 `MarkupString` direct/local/受控-member carrier authoring，直接 canonicalize 为静态 subtree
- 支持：imperative body 内同样的静态 `MarkupString` direct/local/受控-member carrier authoring，以及源码可分析 factory 返回值，直接 lower 为 `h(...)` subtree；带普通按值参数、omitted optional default 或 `params` 单数组绑定时保留调用点 captured-binding 求值顺序
- 支持：handwritten `BuildRenderTree` 中 `MarkupString` local 的“先声明、再在同一线性局部声明前缀内完成一次简单赋值”窄模式；例如 `MarkupString markup; var revision = 0; markup = ...;` 也已纳入支持。若后续再次出现可观察写入，则沿同一 source-stable 合同 fail-fast
- 支持：Razor IR authored template `@{ MarkupString markup; markup = ...; } @markup` 这类 local carrier 也按同一 source-stable 合同接受，并贯通 render tree / `.mjs` pipeline / SFC artifact
- 不支持：imperative local `MarkupString` carrier 在声明后发生重赋值、`ref/out` 写入或其他不可静态证明变异
- 不支持：运行时拼接字符串后再转 `MarkupString`
- 不支持：`ref/out/in`、实参与形参无法按当前合同绑定、或返回值本身已不再可静态证明的 static-markup `MarkupString` factory
- 不支持：来自变量/调用结果/条件分支汇总的动态 `MarkupString`
- 不支持：任何需要保留任意 raw HTML/script 注入语义的场景

### 当前保护

- `src/Jazor.RazorVue/RazorVueStaticMarkupValueHelper.cs`
- `src/Jazor.RazorVue/RenderTree/RazorVueRenderTreeExtractor.cs`
- `src/Jazor.RazorVue/RazorSdk/RazorVueRazorIrTemplateFrontend.cs`
- `src/Jazor.RazorVue/Lowering/RazorVueExpressionEmitter.ImperativeRender.cs`
- `src/Jazor.RazorVue.Test/BuildRenderTreeTemplateFrontendTests.cs`
- `src/Jazor.RazorVue.Test/RazorVuePipelineTests.cs`
- `src/Jazor.RazorVue.Test/RazorVueSfcArtifactFactoryTests.cs`
- `src/Jazor.RazorVue.RazorIr.Test/RazorVueRazorIrTemplateFrontendTests.cs`
- `src/Jazor.RazorVue.RazorIr.Test/RazorVueTemplateFrontendParityTests.cs`

当前回归同时锁定：

- declarative `BuildRenderTree` 中 `(MarkupString)"..."`
- declarative `BuildRenderTree` 中 `new MarkupString("...")`
- declarative `BuildRenderTree` 中局部 `MarkupString` carrier
- declarative `BuildRenderTree` 中 current-component `MarkupString` factory（含普通按值参数 captured-binding 保真）
- declarative `BuildRenderTree` 中 `MarkupString` local 的“先声明、再在同一线性局部声明前缀内完成一次简单赋值”窄模式
- declarative `BuildRenderTree` 中只读 property / `readonly` field `MarkupString` carrier
- declarative / pipeline 中上述 local carrier 的后续重赋值 fail-fast
- Razor authored template expression 中静态 `MarkupString` direct/new/local/readonly-member carrier
- Razor authored template expression 中动态 `MarkupString` fail-fast
- imperative render bridge 中静态 `MarkupString` direct/local/readonly-member carrier
- imperative render bridge 中 mutated local `MarkupString` carrier fail-fast
- 最终产物不会残留 `MarkupString` authoring 语义，而是直接 lower 为静态 subtree

- frontend 产出静态 markup subtree
- H lowering 输出正常 `h(...)` 嵌套结构
- SFC lowering 输出正常 template subtree
- Razor IR frontend 与 handwritten `BuildRenderTree` frontend 复用同一静态标记解析器，不会在静态 HTML 片段语义上漂移

## 15. 已完成：Razor IR 模板局部 `@{ ... }` code-block 受控生产切片

### 现象

此前 Razor IR frontend 对模板内普通 `@{ ... }` code-block 没有稳定的结构化支持。像下面这种很常见的局部缓存/别名写法，会停留在 `CSharpCodeIntermediateNode` 层而无法进入现有 template local 语义：

```razor
@{
    var localTitle = Title;
}

<section>@localTitle</section>
```

这会让 Razor authored 组件和 handwritten `BuildRenderTree` 组件在“模板局部变量”能力上产生不必要分裂。

### 当前落地方式

该缺口已在 Razor IR frontend 中收口为受控生产切片：

- 模板内 `CSharpCodeIntermediateNode` 现在会尝试回映到 Roslyn 变量声明 operation
- 当 code-block 可证明包含“前置带初始化器的局部声明”时，会先还原为现有 `RazorVueLocalDeclarationNode`
- 若这些局部声明之后进入可结构化绑定的受支持控制语句，则同一 code-block 会继续还原出对应的 `RazorVueConditionalNode` / `RazorVueForEachNode` / `RazorVueForNode`
- 对 Razor IR 常见的 boundary 线性化形态，例如一个 `CSharpCodeIntermediateNode` 同时承载 `"}"` 与下一个 `if` / `foreach` / `for` header，frontend 现已能恢复后续顺序控制流，而不会把后一个 control 静默吞掉
- 还原后的节点继续走现有 canonical / H / SFC lowering，与 handwritten `BuildRenderTree` 的 template-scoped local 复用同一契约

因此以下场景现已稳定支持：

- 顶层模板中的 `@{ var localTitle = Title; }`
- loop body 中基于 loop local 的 `@{ var decorated = item + "!"; }`
- typed child-content / scoped slot body 中基于 slot context parameter 的 `@{ var decorated = item + "!"; }`
- `@{ RenderFragment<string> template = item => @<p>@item</p>; } <LayoutCard ItemTemplate="template" />`
- `@{ RenderFragment<string> template = item => @<p>@item</p>; if (Show) { <section>tail</section> } } <LayoutCard ItemTemplate="template" />`
- `@{ RenderFragment<string> template = item => @<p>@item</p>; foreach (var tag in Tags!) { <section>@tag</section> } } <LayoutCard ItemTemplate="template" />`
- `@{ RenderFragment<string> template = item => @<p>@item</p>; for (var i = 0; i < Count; i++) { <section>@i</section> } } <LayoutCard ItemTemplate="template" />`
- `@{ var localTitle = Title; if (Show) { <section>@localTitle</section> } }`
- `@{ var localTitle = Title; if (Show) { <section>@localTitle</section> } else { <p>hidden</p> } }`
- `@{ var localTitle = Title; if (ShowPrimary) { <section>@localTitle</section> } if (ShowSecondary) { <p>secondary</p> } }`
- `@{ var prefix = Title; if (ShowPrimary) { <section>@prefix</section> } foreach (var item in Items!) { <p>@prefix @item</p> } }`
- `@{ var prefix = Title; foreach (var item in Items!) { <p>@prefix @item</p> } }`
- `@{ var prefix = Title; for (var i = 0; i < Count; i++) { <p>@prefix @i</p> } }`

### 当前支持边界

支持边界刻意收窄为“局部声明优先、且只进入受支持顺序控制语句”的模板 code-block：

- 支持：`@{ var localTitle = Title; }`
- 支持：一个 code-block 内多个连续局部声明，只要都带 initializer 且按声明顺序可作用域化
- 支持：一个 code-block 内局部声明可省略 initializer，但仅限“声明后在同一线性局部声明前缀内完成一次简单赋值”这一窄模式，且仍被还原为不可变 template local；允许在这次赋值前穿过 sibling local declarations
- 支持：initializer 捕获当前可见 template local、loop local 与 typed slot/scoped slot context parameter
- 支持：局部声明后进入 `if` / `if-else` / `foreach` / count-style `for`
- 支持：Razor IR 把 `}` 与下一个 `if` / `foreach` / `for` header 合并进同一 boundary code node 时，frontend 会继续恢复后续控制语句
- 支持：声明点初始化的局部 `RenderFragment` / `RenderFragment<T>` carrier；初始化器可以是 inline Razor template、当前组件受支持 `RenderFragment` member carrier，或受支持 fragment factory 调用结果。typed / inline template 仍优先走 Razor SDK `TemplateIntermediateNode` 结构恢复；untyped current-component member / source-stable local direct expression consumption 则允许走 shared builder-lambda parser fallback，因此不再要求 property initializer 自身必须直接带可映回 Razor 的 source mapping，且这两条路径都可继续进入后续组件 slot/template 参数或 direct `@expr` 消费
- 支持：同一 template code-block 里的 local function fragment factory 声明，只要仍位于局部声明前缀区域且返回模板形状本身可静态还原；例如 `@{ RenderFragment<int> template = CreateTemplate(Title); RenderFragment<int> CreateTemplate(string? title) => item => @<span>@title @item</span>; }` 会与 `@code` / current-component factory 走同一 captured-scope + typed-fragment-scope 语义，而不会把 local function 自身的 `@<...>` 模板体或尾随 `;` 泄漏成根级 render node
- 支持：Razor authored direct untyped expression consumption；受支持 current-component member / source-stable local `RenderFragment` carrier 可以直接写成 `@Template` / `@template`，直接调用 fragment factory 返回值也可以写成 `@CreateTemplate(Title)` / `@CreateTemplate()` / `@CreateTemplate(subtitle: Subtitle, title: Title)`；这里既支持当前组件 `@code` / member method，也支持同一 template code-block 里的 local function factory，并保持 direct expression subtree 与 captured scope 语义，named argument out-of-order 也保留调用点求值顺序
- 支持：上述 untyped direct `@expr` 还覆盖“member/local carrier 由 fragment factory 支撑再直接消费”的形态；例如 `private RenderFragment Template => CreateTemplate(Title); @Template` 与 `RenderFragment template; template = CreateTemplate(Title); @template` 都会继续保留 factory captured scope，再直接落到结构化 render subtree
- 支持：Razor authored direct typed invocation consumption；受控 current-component member / source-stable local `RenderFragment<T>` carrier 可以直接写成 `@Template(42)` / `@template(42)`，直接调用 fragment factory 返回值也可以写成 `@CreateTemplate(Title)(42)` / `@CreateTemplate()(42)` / `@CreateTemplate(subtitle: Subtitle, title: Title)(42)`；这里既支持当前组件 `@code` / member method，也支持同一 template code-block 里的 local function factory。当前组件 `[Parameter] RenderFragment<T>?` slot source 也可以直接写成 `@Header(Count + 1)`，并分别落到 typed fragment scope / typed slot outlet 语义
- 支持：Razor IR 局部 `RenderFragment` / `RenderFragment<T>` carrier 后接 imperative tail 再消费。例如 carrier 后接 `while` / conditional `return`，再把同一 carrier 赋给组件 typed slot/template 参数时，frontend 会提升必要片段为 `RazorVueImperativeBlockNode`，同时保留 carrier/local 可见性与后续 sibling 的真实求值顺序。对“先声明、再立即赋值”的 carrier，如果 tail 后仍有 local 读取，segment planner 会把声明前缀并入同一 imperative render segment，并通过 `SemanticWalkerHost.RewriteSimpleAssignmentPreorder` 把简单赋值交给既有 slot-factory lowering，而不是在 RazorVue 内拼接私有 JS delegate 协议
- 支持：typed child-content / typed slot body 的 direct Razor control-block 也进入正式 imperative render 主线；`@while (Show) { <p>@item</p>; break; }` 会 lower 成 scoped slot callback 中的 imperative render IIFE，`@if (Hide) { return; } <p>@item</p>` 会保留提前返回对后续 tail markup 的真实控制流
- 不支持：不是“声明后在同一线性局部声明前缀内完成一次简单赋值”的其他先声明后赋值形态，例如中间穿插普通表达式语句、条件/循环/try 等控制流、后续再次写入、或更宽 dataflow
- 当前这一“声明式模板 code-block 结构化恢复”通道不支持：局部声明后进入更一般的语句执行模型
- 当前这一“声明式模板 code-block 结构化恢复”通道不支持：赋值语句、递增/递减、delegate/callable template state
- 当前这一“声明式模板 code-block 结构化恢复”通道本身仍不负责：`switch` / `while` / `do-while` / `try-catch` / `using` / `lock` 等一般语句执行模型；其中 typed child-content / slot template body 内“局部声明后接 `while` / `do-while` / `switch` / `try-catch-finally` / `using` / `using declaration` / `lock` / `return` / `throw` / mutation`”以及“无前置局部声明的 standalone imperative `@{ ... }` body”都已改走 imperative render 主线，不再属于未支持缺口
- 2026-05-21 补充校准：Razor authored root template `@{ ... }` 下，“template local 声明 + imperative statement + 后续 root sibling”这条 authored form 也已确认可达当前主线，例如 `@{ var localTitle = Title; _count++; } <section>@localTitle @_count</section>`、`@{ var localTitle = Title; if (Hide) { return; } } <section>@localTitle</section>`、以及 `@{ var localTitle = Title; var index = 0; while (index < Count) { <section>@localTitle @index</section>; index++; } } <footer>@localTitle @index</footer>`。不过它的真实语义不是 typed child-content 那种“前缀 local declaration node + 尾部 imperative tail”，而是整个 root 片段统一提升为一个 imperative render block / render-function `.vue` 产物；这是当前实现刻意选择的 root 级单次求值与可见性保留策略，不应再被视为未支持缺口。
- 但这些 block code 不再按“无限期只能 fail-fast”处理：RazorVue 现已正式收敛到“声明式模板通道 + 命令式渲染通道”的双通道架构，后续将按 `docs/01-目标/razorvue/design/RazorVue.BlockCode.ExecutionModel.md` 进入命令式 block promotion / render-function lowering 路线，而不是继续在 Razor IR frontend 内无上限堆 statement 特判

补充说明：

- compiler block-code 主线中的 `using` statement / `using declaration` 已完成 lowering，统一收敛为 `try/finally`
- compiler block-code 主线中的 `lock` statement 已完成 lowering，当前 contract 收敛为 single-agent erased lock semantics：保留单次求值、空值失败、同步顺序与异常传播，不宣称 CLR monitor 语义
- 多 declarator 已支持逆序释放，且后续资源初始化抛错时会释放前面已成功获取的资源
- 资源表达式路径支持单次求值缓存，避免 `using (expr)` 重复执行副作用
- 源码具体 `Dispose()` 成员优先直发实例调用；接口/外部类型 fallback 则走 `System.IDisposable.Dispose()` helper
- RazorVue imperative render 主线现已贯通 handwritten `BuildRenderTree`、Razor IR root template `@{ ... }`、`.mjs` artifact 与 render-function SFC artifact，对 `using` / `using declaration` 统一复用 compiler lowering
- RazorVue imperative render 主线现已贯通 handwritten `BuildRenderTree`、Razor IR root template `@{ ... }`、`.mjs` artifact 与 render-function SFC artifact，对 `lock` 统一复用 compiler lowering
- compiler block-code 主线中的 `await using` 已完成 lowering，当前 contract 为：保留单次求值、异步释放顺序、multiple declarator 逆序释放，以及后续资源初始化抛错时对前序已获取资源的异步释放
- RazorVue imperative render 主线当前不放行 `await using` / `await using var`。保留这个边界的原因不是 compiler 不会 lower，而是当前 `.mjs` / render-function `.vue` artifact contract 仍是同步 render；在没有完整 async setup + Suspense 产物链前，生成 fire-and-forget async-disposal continuation 会破坏渲染完成时序
- 因此 `await`、`await foreach`、`await using (...) { ... }`、`await using var` 和嵌套 `await using` declaration 均由 handwritten `BuildRenderTree` 命令式主线显式报 `UnsupportedImperativeRenderLowering`，避免把 VNode 构建错误推到 Vue 同步 `render()` 返回之后
- Razor authored root template `@{ ... }` 场景下，`await using` 还受到上游 Razor SDK/`BuildRenderTree` 同步方法 contract 约束；这一路径在 Razor 生成阶段本身就不成立，不属于 RazorVue 单独漏支持
- 2026-05-23 同轮校准了 imperative bridge 中的 `MarkupString` local carrier 语义：`MarkupString markup; markup = CreateMarkup(); builder.AddContent(..., markup);` 这类 factory-backed local 现在仍按值承载一次静态 markup 字符串，再由消费点解析为最终 `h(...)` subtree；实现走 `SemanticWalkerHost` 的 declarator/assignment rewrite seam，不会把 `MarkupString` 伪装成 callable thunk（例如 `markup = () => h(...)` / `append(markup())`），也不会把 static-markup factory 错误拉入 setup helper lowering 图。动态或后续可观察写入的 `MarkupString` carrier 仍按 source-stable 合同 fail-fast

### 当前保护

- `src/Jazor.RazorVue/RazorSdk/RazorVueRazorIrTemplateFrontend.cs`
- `src/Jazor.RazorVue/RazorSdk/RazorVueRazorIrOperationResolver.cs`
- `src/Jazor.RazorVue.RazorIr.Test/RazorDocumentNodeInventoryTests.cs`
- `src/Jazor.RazorVue.RazorIr.Test/RazorVueRazorIrCompilerExpressionBridgeTests.cs`
- `src/Jazor.RazorVue.RazorIr.Test/RazorVueRazorIrTemplateFrontendTests.cs`

当前回归同时锁定：

- Razor IR inventory 确实产出模板 code-block 对应的 `CSharpCodeIntermediateNode`
- operation resolver 能把该 code-block 回映为变量声明 operation
- operation resolver 也能把“局部声明 + 控制语句”这类复杂 template code-block 回映为 block/conditional/loop operation
- frontend 产出 `RazorVueLocalDeclarationNode`，并在同一片段内继续产出对应 conditional / foreach / for node
- 对于局部 `RenderFragment` / `RenderFragment<T>` carrier，frontend 会按 handwritten `BuildRenderTree` 既有契约直接吸收为结构化 slot template carrier，而不是保留为根级 `RazorVueLocalDeclarationNode`
- 对于局部 `RenderFragment` / `RenderFragment<T>` carrier，如果它来自当前组件只读 member 或受支持 fragment factory，frontend 同样会保留“外层 captured-value scope + 内层 typed fragment scope”的结构化语义，而不是退化成动态 delegate 执行
- 对于同一 template code-block 里声明的 local function fragment factory，frontend 现也会按 operation 覆盖范围消费对应 Razor IR template node，而不是把 local function 内部匿名模板、声明残片或尾随 `;` 误当成未绑定根节点继续泄漏到 render tree
- 对于局部 `RenderFragment` / `RenderFragment<T>` carrier 后继续出现的 trailing `if` / `foreach` / `for`，frontend 会恢复为同一模板片段中的顺序控制节点，而不会把 control body 错误裸露为根节点
- 对于局部 `RenderFragment` / `RenderFragment<T>` carrier 后继续出现的 trailing `while` / conditional `return` / post-loop local use，frontend 会提升到 imperative render segment，并把 declaration/immediate-assignment 前缀纳入 segment 或作为可见 local 传播；不会再重复声明 `index`，也不会在后续 slot 参数消费时丢失 `template`
- direct `@Template` / `@template` 这类 untyped `RenderFragment` member/local expression 现已回归锁定：frontend 会直接还原结构化 subtree，而不会重复输出同一 template body、把普通 member 误判成 slot outlet，或把 immediate-assignment local 错误退回 imperative tail
- direct `@Template(42)` / `@template(42)` 这类 typed `RenderFragment<T>` member/local invocation，以及 direct `@CreateTemplate(Title)(42)` / `@CreateTemplate()(42)` / `@CreateTemplate(subtitle: Subtitle, title: Title)(42)` 这类 fragment factory 返回值的立即消费，现也已回归锁定：frontend 会直接还原为 typed fragment scope，并保留外层 captured-value scope，而不会退化成普通 invocation 表达式、错误进入 compiler expression bridge，或在 canonical/SFC 阶段触发 member/property unsupported；这里既覆盖当前组件 `@code` / member method，也覆盖同一 template code-block 里的 local function factory，named argument out-of-order 也会保留调用点求值顺序
- direct `@Header(Count + 1)` 这类 typed slot outlet invocation 现也已锁定：当前组件 `[Parameter] RenderFragment<T>?` slot source 会直接还原为带 argument 的 slot outlet，最终 lower 为 `<slot name="header" :value="(props.count + 1)" />`，而不会再变成普通插值表达式
- property initializer 若在 Razor SG 生成后落成 builder lambda 且整段 operation 本身没有直接 source mapping，当前实现会明确依赖 shared builder parser fallback；不会再把“direct source-map 缺失”误当成功能不支持
- frontend 对 `"}"` + 下一个 control header 共处同一 `CSharpCodeIntermediateNode` 的 boundary 形态具备稳定恢复能力
- H lowering 输出局部 `const`
- H lowering 对 `if` / `if-else` / `foreach` / count-style `for` 的后续节点保持与 handwritten `BuildRenderTree` 一致的作用域顺序
- SFC lowering 输出局部 `<template v-for="(...) in [...]">` scope wrapper
- typed child-content / scoped slot body 中的 template-local code-block 会忽略同一 generated block 内追加的 Razor builder 尾巴，不会误判成“混入任意语句执行模型”
- typed child-content / scoped slot body 中的 direct `@while` 与 direct `@if { return; } + tail markup` 也已锁定为正式 imperative render 路线，slot context 参数继续作为 `VisibleParameters` 进入 lowering，而不是退化成未绑定 local
- lifecycle 受控子集本轮也补齐了一条真实继承链缺口：`ShouldRender` 现在不仅支持 `return true;` 与直接透传 `ComponentBase.ShouldRender()`，还支持“派生类 `return base.ShouldRender();` -> 基类受支持 `ShouldRender` 实现”的递归安全 pass-through 链；例如抽象基类 `return true;`、派生组件再透传，当前 analyzer / generator / HMR 边界都已一致视为受支持，而“派生类透传 -> 基类动态条件 `return Value > 0;`”仍会显式保持 unsupported / FullReloadRequired
- setup-side helper lowering 本轮也完成了一次边界校准：旧的“helper 最多只支持 2 层组合”不再是实际 contract。当前同步 current-component helper 只要仍处在源码可分析、非 `Task`/`ValueTask` 返回、且 body 能被现有 setup lowering 合同承载的受控子集内，就会继续递归收集并 lower 到同一 setup scope；`FormatOuter -> FormatMiddle -> FormatInner` 这类三层及以上同步 helper 组合现已正式支持
- 随后 setup-side helper body 也从“单表达式 / 单返回”扩展到普通同步 block body：局部变量、条件 return、以及继续依赖 setup property/field/helper 的语句序列会通过 `RazorVueExpressionEmitter.EmitSetupStatementSequence(...) -> SemanticWalker.TranslateStatementSequence(...)` 进入 compiler-owned statement lowering，而不是在 RazorVue 里手写 JS statement 拼接
- setup-side current-component getter-bodied property 同轮也已从缺口转入正式支持面，但路线刻意保持 compiler-owned：RazorVue 只负责把 property body 解析成 Roslyn `IOperation`、发射 setup function 外壳、做依赖排序与循环检测；真正的表达式/CLR/type/import/reference 语义仍继续走 `RazorVueExpressionEmitter.EmitSetupExpression -> SemanticWalker -> Jazor.Compiler`，而不是在 RazorVue 内手拼一套私有 JS property 语义
- 当前已正式支持的 property 子集是：expression-bodied property、getter accessor 中单个 `return` 的 property，以及这些 getter property 之间仍落在同一受控子集内的链式依赖；helper body 引用它们时会稳定 lower 为 setup function 调用
- setup-side value-like member 合同已拆成两类：只读/getter-only/source-stable declaration initializer 继续发射为 setup `const`，private mutable field / private-setter auto-property 则发射为 setup `let` carrier；后者允许源码中存在 later writes，helper body、direct template expression、lifecycle payload 与 imperative render body 都共用同一 setup binding 主线
- private mutable setup carrier 会保留声明点 initializer；没有 initializer 时按 CLR 默认值发射；如果存在 initializer 但无法通过 `Jazor.Compiler` / `SemanticWalker` lower，则显式报 `UnsupportedSetupLogicLowering`
- 当前也已正式锁定 fail-fast 边界：getter property 链若形成循环依赖，会在编译期直接报 `UnsupportedSetupLogicLowering`；setter 不是 private 的 mutable property、带自定义 getter/setter 的 mutable property、非 private mutable field、static/indexer/隐式成员仍不作为 setup carrier 接受。`RenderFragment` / `MarkupString` / static markup 这类需要 source-stable 追踪的 member carrier 也没有因此放宽 later writes，仍按各自 source-stable 合同 fail-fast
- 这条扩面没有放宽到 async helper 或任意方法体：`async` helper、`Task` / `ValueTask` 返回 helper、`ref` / `out` / `in` 参数、以及超出当前 compiler-owned statement lowering 支持面的 helper 仍显式报 `UnsupportedSetupLogicLowering`。因此这里关闭的是“人工深度上限 + 普通同步 block body”缺口，不是把 setup-side logic 变成通用执行模型
- 同轮，`SetParametersAsync` 的 no-op 合同也补齐了一格此前遗漏的表达式体形态：`public override Task SetParametersAsync(ParameterView parameters) => Task.CompletedTask;` 现在会与 block-body no-op 一样被识别为“无运行时生命周期行为”，因此 pipeline / generator / HMR 边界都会保持 `TemplateOnly`，不会再因为只是 expression-bodied 空实现就误退回 unsupported
- 2026-05-23 继续补齐后，`SetParametersAsync` 的 base-pass-through emit 子集不再只能接受“base 后直接单个 `InvokeAsync(...)`”。当前还支持在 `await base.SetParametersAsync(parameters);` 后保留 source-stable local、local function、或 callable local 前缀，再把最终 payload 交给单个受支持 `InvokeAsync(...)`；payload 与前缀仍通过 `RazorVueExpressionEmitter` / `SemanticWalker` 做 compiler-owned lowering，而不是 RazorVue 手拼 JS。重复 emit、额外 mutation、控制流或任意方法体仍显式 fail-fast
- 同一批 lifecycle contract 还补齐了普通 lifecycle base-pass-through 的一个尾随 no-op 漏格：像 `await base.OnInitializedAsync(); return;` 这类“base 透传后只跟空返回”的方法体，现在会与纯 pass-through 一样保持 `TemplateOnly` / 原有受支持语义，而不会仅仅因为多了一个 no-op `return;` 就误退回 unsupported
- 同轮还收紧了一条 `SetParametersAsync` 的生产边界：如果派生组件只是 `return base.SetParametersAsync(parameters);`，但真正被透传的 base override 来自外部引用程序集、当前编译看不到源码，那么 RazorVue 现在不会再把这条链乐观当成安全 no-op/pass-through。当前只继续接受直达 `ComponentBase.SetParametersAsync(...)` 默认实现，或源码可分析且同样落在受支持子集内的 base 链；外部无源码 override 会显式退回 analyzer `JAZORVUE006` 与 generator/pipeline `FullReloadRequired`
- 随后又补齐了一条同类 contract 漂移：`SetParametersAsync` 的 expression-bodied no-op 之前已经在 pipeline / generator 层被识别为 `TemplateOnly`，但 analyzer 仍会误报 `JAZORVUE006`。当前 analyzer 已与 lowering 主线对齐接受这类 no-op，因此这一格现在不再是“运行时支持但编译期误报”的半支持状态
- 本轮又收紧并校准了一条更隐蔽的 no-op 边界：bare `default` 不再被一概视为 lifecycle 空实现。真实合同现在是“按目标返回类型判定”：
  - `Task` 返回 lifecycle / `SetParametersAsync` 只接受真实 completed-task 形态，例如 `Task.CompletedTask`
  - non-generic `ValueTask` 返回 lifecycle 继续接受 `default`、`default(ValueTask)`、`default(System.Threading.Tasks.ValueTask)`，以及 `new ValueTask(...)` 包裹后的等价 no-op
  - 因此 `protected override Task OnInitializedAsync() => default;` 现在会在 analyzer `JAZORVUE005`、pipeline lowering、以及 generator `JAZORVGA005` 三层一致回到 unsupported；而既有 `ValueTask DisposeAsync() => default;` / 继承 base 链上的同类 no-op 仍保持 `TemplateOnly`
- 这次修复不是把 analyzer 单独补一个字符串特判，而是把 analyzer / lowering / generator 的 no-op contract 重新收拢到同一条返回类型语义边界，避免后续再次出现“Task 被误放行”或“ValueTask 被误伤”的双向漂移
- lifecycle payload 本轮还补齐了一条真实使用路径：source-stable current-component value member 现在可以正式进入 lifecycle payload lowering，而不再只限 `[Parameter]` property。当前锁定的受支持原子包括 declaration-initialized value-like property、declaration-initialized field，以及 getter-bodied property；这些 member 会继续沿现有 setup/property/field lowering 主线发射为 setup binding / setup function，再由 lifecycle payload 引用，而不是在 RazorVue 内部新增一套私有 member 拼接语义
- 随后这条 lifecycle payload 路线又继续补齐到一条受控 current-component helper/method-call 子集：只要 helper 仍是当前组件内、源码可分析、同步、非 `Task`/`ValueTask` 返回，且 helper body 仍收敛在现有 setup helper lowering 合同内，lifecycle payload 现在也可以直接引用该 helper 调用；这里普通按值参数、named argument out-of-order、omitted optional default、以及按 Roslyn 绑定成单数组形参的 `params` 都已正式支持，并保持“按调用点源码顺序求值、按形参声明顺序落位”。helper 体对 declaration-initialized property/field、getter-bodied property、以及其他同步 helper 的依赖，会继续并入同一 setup 依赖图；block-bodied helper 也沿同一 setup 依赖图进入 lifecycle payload，并已锁定 setup function 先于 `watch(..., { immediate: true })` / `onMounted` / `onUpdated` 发射
- 这条扩面同样刻意不放宽到任意 helper 或方法执行模型：`async` helper、`Task` / `ValueTask` 返回 helper、`ref/out/in` 或其他无法按当前合同绑定的形参、越出当前 setup lowering 合同的 helper body、不能作为 setup carrier 或 source-stable carrier 的 mutable/later-written member、一般外部 invocation、以及更宽 dataflow 的 payload 仍显式 fail-fast。这里关闭的是“受控 helper-call payload 不能进入 lifecycle lowering”的缺口，不是把 lifecycle lowering 扩成 setup helper 的任意求值通道
- module builder / SFC builder 同轮还修掉了一条初始化顺序风险：lifecycle lowering 现已先创建 lifecycle plan，并在 plan 阶段预收集 payload 触发的 setup property/field/method 依赖；这些 setup bindings/functions 会先发射，随后才注册 `watch(..., { immediate: true })` 与其他 lifecycle hook。这样 `OnParametersSet*` / `SetParametersAsync` 的 immediate watch 不会在 setup binding 尚未声明时提前闭包引用它们
- 这条顺序修正不是“生成文本更好看”，而是生产级正确性要求：只要 lifecycle payload 依赖 setup member，就必须保证 setup 先于 hook/watch 注册，否则 `watch(..., { immediate: true })` 会形成 TDZ / 初始化顺序错误
- 同步 block-bodied setup helper 的 normalized method body 现也会进入 LogicHash / HMR 逻辑指纹；此前这种 helper 已能 lowering，但 shape 描述层仍会落成 `unsupported`，存在 helper body 变化不进入 logic hash 的风险
- analyzer / lowering / generator 的 lifecycle 支持矩阵本轮也重新对齐：analyzer 不再沿用旧的语法级近似规则猜 lifecycle payload 是否受支持，而是构建同一 semantic snapshot 并复用 lowering 侧的 support-shape 判定；因此 declaration-initialized property/field lifecycle payload 不再出现“runtime 已支持但 analyzer 先误报”的层间漂移
- 随后 `OnAfterRender*` 的 `firstRender` payload 又补齐了一条 compiler-owned fallback：当 payload 确实引用 `firstRender`、且表达式形状仍落在当前受控子集内时，RazorVue 会把 `firstRender` 别名成 `currentFirstRender`，再继续交回 `EmitSetupExpression -> SemanticWalker -> Jazor.Compiler`；RazorVue 自己只负责 after-render snapshot 协议，不新增另一套私有 CLR/call lowering
- 这条 fallback 当前已锁定的真实形态又继续扩到 `(bool)firstRender`、`object.Equals(firstRender, true)`、`object.Equals((bool)firstRender, true)`、`firstRender.Equals(true)`、`firstRender == true`、`bool? alias = firstRender; alias ?? false` 这一类 source-stable nullable-bool local carrier、`firstRender is true/false`、`firstRender is not true/false`、`firstRender is true or false`、`firstRender is true and not false`、`firstRender is bool`、`firstRender is object`、直接 against `firstRender` 的 declaration-pattern（例如 `firstRender is bool ready && ready`）、`firstRender switch { ... }`，以及继续满足 setup helper 合同的受控 helper-call payload，例如 `Normalize(firstRender)`；这些表达式最终都会 against `currentFirstRender` 发射，而不是直接逃逸到未快照的 lifecycle 参数，其中 `object.Equals(firstRender, true)` / `object.Equals((bool)firstRender, true)` / `firstRender.Equals(true)` 当前都会稳定 lower 为 `currentFirstRender === true`，`firstRender == true` 会稳定 lower 为 `(currentFirstRender === true)`，`alias ?? false` 会稳定 lower 为 `currentFirstRender ?? false`，而 declaration-pattern 也会继续走 compiler-owned pattern lowering，保留 pattern local 绑定
- 同一条 helper-call 路线在 2026-05-22 又补齐了一格此前真实存在的参数绑定漂移：`Normalize(suffix: "!", value: firstRender, prefix: "ready:")` 这类 named argument out-of-order lifecycle payload，现在会稳定发射为“外层按源码顺序 captured wrapper + 内层按形参顺序 helper 调用”，而不是再把 `invocation.Arguments` 直接 `string.Join(...)` 成错误的实参位次。`PreludeBindings` / `UsesFirstRender` 聚合也已沿同一 shared binder 补齐，因此子参数若触发 compiler-owned fallback，不会再在 helper-call wrapper 层把 `currentFirstRender` 或 source-stable prelude alias 丢失。
- 随后又补上了一段先前明确缺失的 structural deep-chain：`new ReadyEnvelope(new ReadyState(firstRender)).State.Value` 这类直接 structural source-data-carrier 深链，以及 `var readyEnvelopes = new List<ReadyEnvelope> { ... }; readyEnvelopes[1].State.Value` 这类 source-stable structural local/list carrier，现已通过 compiler-owned structural source-data-carrier lowering 支持；发射结果会擦成 object literal shape，并在需要时继续复用既有 CLR helper（例如 `List<T>.this[int].get`），而不是在 RazorVue 里临时拼一套 JS class/runtime
- 同一条 compiler-owned structural 路线随后又继续补齐到 helper-returned deep member-chain 与 structural property-pattern：`BuildEnvelope(firstRender).State.Value`、`new ReadyEnvelope(new ReadyState(firstRender)) is { State.Value: true }`、`BuildEnvelope(firstRender) is { State.Value: true }` 现在也已正式支持。其中 property-pattern 会保留 compiler-owned 单次求值 temp，避免 helper 或 object creation 被重复执行。
- 同一条主线本轮又补齐了三格先前仍被 support-gap 文档记成“未支持”的真实能力：`new ReadyState(firstRender).Value`、`new ReadyEnvelope { State = new ReadyState(firstRender) }.State.Value`、以及 `(firstRender, new ReadyState(firstRender)).Item2.Value`。前两者继续擦成 structural object literal deep-chain；tuple 这一格则继续遵守现有 compiler tuple runtime-shape 合同，字段名取当前静态视图，因此发射结果是 `.item2.value`，而不是 RazorVue 再私造一套 `Item2` 约定。
- 同一条 compiler-owned fallback 本轮继续纠正了 4 格旧 support-gap 记录：`firstRender.ToString().Length > 0` 这类 chained expression、`new ReadyEnvelope(new ReadyState(firstRender)).State.Value.Equals(true)` 这类 structural deep-member equals、`BuildReady(firstRender).Value.Equals(true)` 这类 helper-returned equals、以及 `(new ReadyEnvelope { State = new ReadyState(firstRender) }.State?.Value) ?? false` 这类 null-conditional + coalesced structural payload，现都已转入正式支持矩阵，而不是继续标成 fail-fast。
- `var pair = (firstRender, new ReadyState(firstRender)); var (_, readyState) = pair; readyState.Value` 这类 tuple deconstruction source-stable local payload 现已通过 focused 回归重新锁定为正式支持；路线不是 RazorVue 私造 tuple 投影，而是先把 source-stable declaration 前缀纳入 lifecycle prelude，再继续交回 `EmitSetupExpression -> SemanticWalker -> Jazor.Compiler` 完成 tuple / deconstruction lowering。
- 同一条 compiler-owned fallback 现也已正式覆盖 source-stable local function 与 local lambda / delegate local payload：例如 `bool NormalizeReady(bool value) => value; NormalizeReady(firstRender)`、`Func<bool, bool> normalizeReady = static value => value; normalizeReady(firstRender)`。RazorVue 只负责 prelude alias framing，真实 callable body / invocation 语义仍由编译器主链负责。
- 2026-05-23 本轮继续把这条 fallback 从 `firstRender` 专用路径扩到普通 lifecycle 的受控 local payload：`var label = Prefix + Value; ValueChanged.InvokeAsync(label + "!")` 这类 source-stable local、`string FormatLabel(int value) => "Count: " + value; ValueChanged.InvokeAsync(FormatLabel(Value))` 这类 local function payload，以及 `Func<int, int> increment = static value => value + 1; ValueChanged.InvokeAsync(increment(Value))` 这类 callable local payload，现在都会在 `OnInitialized*` / `OnParametersSet*` 等普通 hook body 内先发射稳定 prelude alias / local callable，再把最终 payload 交回 `EmitSetupExpression -> SemanticWalker -> Jazor.Compiler`。对 `OnAfterRender*`，`firstRender` 使用检测也会递归进入 source-stable local initializer、local function body 与 callable local initializer；local function 闭包捕获 `firstRender` 时仍会生成 `currentFirstRender` snapshot，不会漏发 snapshot alias 或在 async hook 中引用已翻转的 `firstRender`。
- 普通 lifecycle 中名为 `firstRender` 的局部变量现在按普通 source-stable local 处理，不再被旧测试/文档误标成 after-render 专用风险；只有真实 `OnAfterRender*` 参数才进入 `currentFirstRender` snapshot 协议。
- 同日，普通 lifecycle 的 helper / local function 调用参数绑定也已补齐到与 after-render `firstRender` payload 同一条 shared binder：omitted optional default、按 Roslyn 绑定成单数组形参的 `params`、以及 named argument out-of-order 都正式支持。这里不是在 RazorVue 内拼接 CLR 调用语义，而是沿 Roslyn `IInvocationOperation.Arguments` 的绑定结果保留调用点左到右求值顺序，再由 `EmitSetupExpression -> SemanticWalker -> Jazor.Compiler` 完成参数表达式 lowering。
- 这条扩面仍保持生产级保守边界：`async` local helper / local lambda、`Task` / `ValueTask` 返回 local helper、无法从源码稳定恢复初始化器或声明前缀的 callable local、`ref/out/in` 参数、一般外部 invocation、同一个 lifecycle body 内重复 emit / 额外 mutation / 控制流 / 多语句执行模型、依赖额外 source-stable object boxing/local carrier 的 declaration-pattern / pattern-var，以及更宽 dataflow 形状继续 fail-fast。这里开放的是“现有 compiler-owned lowering 真能诚实承载的受控 lifecycle payload 表达式”，不是继续靠 RazorVue 特判堆矩阵

## 16. 已完成：body-level imperative H/render-function 首段承载

### 现象

此前 RazorVue 虽然已经在架构上引入了命令式 block promotion，但真实 artifact 主线还停留在：

- render tree 能识别 `RazorVueImperativeBlockNode`
- canonical / SFC 能显式拒绝
- `.mjs` / H artifact 还不能正式承载复杂 block

这意味着 `while` / `do-while`、提前 `return`、局部 mutation 这类真实复杂 `BuildRenderTree` 业务虽然不再应该继续扩 statement 特判，但最终仍没有正式运行路径。

### 当前落地方式

该缺口现已在 RazorVue `.mjs` / H artifact 主线中收口为 body-level imperative render bridge：

- 当 render tree 根为单个 `RazorVueImperativeBlockNode` 时，module builder 会切换到 render-function 路径
- imperative body 继续复用现有 `SemanticWalker` 语句级 lowering，而不是在 RazorVue 内手写另一套 JS 控制流解释器
- `RenderTreeBuilder` API 在 imperative body 内会重写到本地 `__jazorCreateRenderContext(h)` bridge
- root imperative body 现已进入 canonical / SFC semantic 正式模型：canonical model 会携带 `ImperativeRootProgram`，SFC semantic model 会以 `RenderMode = RenderFunction` 正式表示该产物，不再要求 SFC artifact factory 在入口层额外扫描 renderTree 决定是否走旁路
- mixed declarative + imperative render body 会在同一个 render-function 中组合：声明式 sibling 继续以 `h(...)`、`__jazorVueForRange(...).map(...)`、条件表达式或 template-scope IIFE 表达，不需要 bridge 的 attribute/key/event scoped replay 不会把整棵 subtree 过度提升为 open-frame replay
- imperative render body 中若出现声明式 helper 调用，例如 attribute spread 生成的 `__jazorVueMergeAttributes(...)`，`.mjs` 与 render-function `.vue` builder 会按 body dependency 注入 helper；helper 注入不再只依赖 root declarative render expression
- `OpenRegion(...)` / `CloseRegion()` 在 body-level imperative render bridge 中作为 Razor frame-shape 边界保留，不 materialize 为 Vue vnode；bridge 会记录 region 打开时的 frame depth，要求 close 时回到同一 depth，并在 `finish()` 时拒绝未闭合 region，避免 region 跨 element/component frame 或静默泄漏
- bridge 当前已稳定承载：
  - `return` 提前退出
  - `while` / `do-while`
  - 局部 mutation 后继续渲染
  - 数组/多根节点追加
  - 平衡 `OpenRegion(...)` / `CloseRegion()`，作为 frame-shape 边界而不是 runtime node
  - imperative body 内常量 `AddMarkupContent(...)`，其静态 subtree 直接 lower 为 `h(...)` 表达式，而不是退化为 raw HTML 占位
- 该 body-level imperative 主线现在同时覆盖：
  - handwritten `BuildRenderTree`
  - Razor authored root template `@{ ... }` code-block，经 Razor IR frontend 提升为同一 `RazorVueImperativeBlockNode`

### 当前支持边界

- 支持：body-level imperative render root
- 支持：单 imperative root program 进入 canonical / SFC semantic 正式模型
- 支持：提前 `return`
- 支持：`while` / `do-while`
- 支持：带 `break` / `continue` 的 `for` / `foreach`
- 支持：局部赋值/递增后再渲染
- 支持：body-level imperative bridge 内平衡 `OpenRegion(...)` / `CloseRegion()`；region 本身不产生 Vue vnode，但 close 必须回到 open 时的 frame depth，`finish()` 会拒绝未闭合 region
- 支持：imperative body 内常量 `AddMarkupContent(...)`
- 支持：mixed imperative body 中的声明式条件/循环/template-scope 子表达式组合；不会再把同一个 conditional / loop 节点递归重新包装成自身 fragment 导致 stack overflow
- 支持：mixed imperative body 中声明式 sibling attribute spread 的 `__jazorVueMergeAttributes(...)` helper 注入，覆盖 `.mjs` 与 render-function `.vue`
- 支持：mixed imperative body 中被命令式片段直接或间接调用的 local function declaration 保留；planner 只扩展 segment dependency，local function body 与调用仍由 `SemanticWalker` lowering
- 支持：mixed imperative body 中的 tuple deconstruction declaration/assignment；段内声明局部不会再进入 RazorVue local alias map，函数级声明 hoist / tuple projection / assignment 由 `SemanticWalker` 完成
- 支持：`OpenElement` / `CloseElement` / `OpenComponent` / `CloseComponent` / `AddAttribute` / `AddMultipleAttributes` / `SetKey` 的 imperative bridge 基础协议
- 不支持：imperative body 继续结构化进入 template canonical subtree
- 不支持：`goto`；当前同步 imperative render bridge 会显式报 `UnsupportedImperativeRenderLowering`，不会把跳转语义静默擦除
- 不支持：动态 `AddMarkupContent(...)`

### 当前保护

- `src/Jazor.RazorVue/Lowering/RazorVueExpressionEmitter.ImperativeRender.cs`
- `src/Jazor.RazorVue/Lowering/RazorVueExpressionEmitter.ImperativeMixedRender.cs`
- `src/Jazor.RazorVue/Lowering/RazorVueArtifactFactory.ModuleBuilder.cs`
- `src/Jazor.RazorVue/Lowering/RazorVueImperativeSfcModuleBuilder.cs`
- `src/Jazor.RazorVue/RenderTree/RazorVueOperationLocalCollector.cs`
- `src/Jazor.RazorVue/RenderTree/RazorVueOpenNodeReplayHelper.cs`
- `src/Jazor.RazorVue.Test/RazorVuePipelineTests.cs`
- `src/Jazor.RazorVue.Test/RazorVueSfcArtifactFactoryTests.cs`

当前回归同时锁定：

- conditional return 通过 imperative render bridge
- `while` / `do-while` 通过 imperative render bridge
- `for` / `foreach` 中的 `break` / `continue` 通过 imperative render bridge
- 局部 mutation 通过 imperative render bridge
- 平衡 `OpenRegion(...)` / `CloseRegion()` 通过 `.mjs` 与 render-function `.vue` imperative bridge，且 bridge helper 会校验 frame-depth 与未闭合 region
- imperative body 内常量 `AddMarkupContent(...)` 直接发射 `h(...)` subtree，而不是残留 raw-html 占位
- mixed imperative body 中的 count-style `for` 继续输出 `__jazorVueForRange(...).map((i) => h(...))`，不会因普通 child replay 日志退化为 open-frame bridge
- mixed imperative body 中的 attribute spread sibling 会在 `.mjs` 与 render-function `.vue` 两条产物中注入 `__jazorVueMergeAttributes(...)` helper
- Razor authored root template code-block 中的提前 `return` / `while` / 局部 mutation 也走同一 imperative render bridge，而不是回退到另一条前端语义
- root imperative body 经 canonical / SFC semantic 正式模型进入 render-function `.vue` 产物，不再要求 artifact factory 入口层先扫描 renderTree 决定是否走特殊旁路

## 17. 已完成：HTML DOM event attribute 与 Blazor event modifier 支持

### 现象

此前 RazorVue 对 element 上的 Blazor DOM event attribute 仍存在两类裂缝：

- `builder.AddAttribute(..., "onclick", EventCallback/delegate)` 容易被当成普通 HTML attribute，而不是 Vue DOM event
- `AddEventPreventDefaultAttribute(...)` / `AddEventStopPropagationAttribute(...)` 没有进入 render tree / canonical / SFC / imperative bridge 的正式语义模型

这会导致真实 Razor authoring 中常见的 `@onclick:preventDefault` / `@onclick:stopPropagation` 等价输出无法稳定落到 Vue `.prevent` / `.stop` 或 render-function wrapper。

### 当前落地方式

本轮已把 HTML DOM event 与 event modifier 收口为正式模型：

- render tree `RazorVueAttributeNode` 携带 `RazorVueEventModifiers`
- `BuildRenderTree` frontend 识别 `WebRenderTreeBuilderExtensions.AddEventPreventDefaultAttribute(...)` 与 `AddEventStopPropagationAttribute(...)`
- Razor IR frontend 现在也覆盖 `.razor` authored HTML event directive：`@onclick` 会规范化为 element DOM event，`@onclick:preventDefault` / `@onclick:stopPropagation` 会合并进同一 `RazorVueEventModifiers`；当 Razor SDK 把这些 directive 暴露成 raw markup attribute 时，frontend 会回查生成的 `BuildRenderTree` 调用位次或在当前组件 partial probe 中重新绑定表达式，拿到真实 Roslyn `IOperation`
- raw markup fallback 的 probe 结果不会被当成字符串 JS 使用。它携带自己的 `SemanticModel.Compilation`，后续 setup/member 支持性分析会使用该 operation 所属 compilation，避免 `Count++` / 当前组件 member 引用这类 handler 语义因 symbol / syntax tree 身份不一致而绕过 `Jazor.Compiler` / `SemanticWalker`
- Blazor event key 统一经 `RazorVueDomEventName` 规范化，例如 `onclick` -> Vue template `@click` / H prop `onClick`
- canonical model 增加 `HtmlEvent` attribute kind，并把 modifier 表达式纳入 template encodability / safety / side-effect 分类
- SFC template 对静态 `true` modifier 输出 Vue 原生 `.prevent` / `.stop`
- 动态 bool modifier 在事件 handler 触发时求值，不在 `script setup` 初始化时冻结；因此 `props.preventClick` 或 template-local `localPrevent` 后续变化仍会影响事件行为
- H/render-function path 会生成稳定 handler wrapper，保留单次 handler 表达式求值与事件时 modifier 条件判断
- imperative render bridge 增加 `setEventModifier(...)` / `setEventModifiers(...)`，会规范化 event key、包装 DOM handler、避免重复 wrapper，并在 modifier 被清除时恢复原始 handler

### 当前支持边界

- 支持：HTML element 上的 `EventCallback` / delegate-like `onclick` 等 Blazor DOM event attribute
- 支持：Razor IR / `.razor` authored `<button @onclick="OnClick" @onclick:preventDefault="true" @onclick:stopPropagation="StopClick">`
- 支持：Razor IR / `.razor` authored inline lambda handler，例如 `<button @onclick="() => Count++" @onclick:preventDefault="PreventClick">`
- 支持：`AddEventPreventDefaultAttribute(builder, seq, "onclick", true/false/dynamicBool)`
- 支持：`AddEventStopPropagationAttribute(builder, seq, "onclick", true/false/dynamicBool)`
- 支持：静态 `true` 输出 SFC `.prevent` / `.stop`
- 支持：静态 `false` 不输出 modifier，并且 handwritten `BuildRenderTree` 中后续 `false` 会清除之前同一 event 的 modifier
- 支持：动态 bool modifier 在 SFC template event handler 内按事件触发时读取当前表达式，包含 root props 与 template-local scope
- 支持：imperative render bridge 中 event modifier 设置、清除和 handler rewrap
- 不支持把任意 `on*` 字符串属性都当成 event；当前需要 event callback/delegate-like value 或 modifier metadata
- 不支持把 component emit modifier 与 HTML DOM event modifier 混为一条路径；组件 emits 仍按 descriptor-aware component event lowering 处理

### 当前保护

- `src/Jazor.RazorVue/RazorVueDomEventName.cs`
- `src/Jazor.RazorVue/RenderTree/RazorVueRenderTree.cs`
- `src/Jazor.RazorVue/RenderTree/RazorVueRenderTreeExtractor.cs`
- `src/Jazor.RazorVue/Canonical/RazorVueCanonicalHModelFactory.cs`
- `src/Jazor.RazorVue/Sfc/RazorVueSfcSemanticModelFactory.cs`
- `src/Jazor.RazorVue/Lowering/RazorVueExpressionEmitter.ComponentAuthoring.cs`
- `src/Jazor.RazorVue/Lowering/RazorVueExpressionEmitter.ImperativeRender.cs`
- `src/Jazor.RazorVue/Lowering/RazorVueExpressionEmitter.ImperativeMixedRender.cs`
- `src/Jazor.RazorVue/Lowering/RazorVueArtifactFactory.ModuleBuilder.cs`
- `src/Jazor.RazorVue/Lowering/RazorVueSfcArtifactFactory.cs`
- `src/Jazor.RazorVue/RazorSdk/RazorVueRazorIrTemplateFrontend.cs`
- `src/Jazor.RazorVue/RazorSdk/RazorVueRazorIrOperationResolver.cs`
- `src/Jazor.RazorVue.Test/BuildRenderTreeTemplateFrontendTests.cs`
- `src/Jazor.RazorVue.Test/RazorVuePipelineTests.cs`
- `src/Jazor.RazorVue.Test/RazorVueSfcArtifactFactoryTests.cs`
- `src/Jazor.RazorVue.RazorIr.Test/RazorVueRazorIrCompilerExpressionBridgeTests.cs`
- `src/Jazor.RazorVue.RazorIr.Test/RazorVueRazorIrTemplateFrontendTests.cs`

当前 focused 回归已通过：

- `dotnet test src/Jazor.RazorVue.Test/Jazor.RazorVue.Test.csproj --filter 'FullyQualifiedName~ElementDomEventWithModifiers|FullyQualifiedName~FalseElementDomEventModifier|FullyQualifiedName~LaterFalseElementDomEventModifier|FullyQualifiedName~DynamicModifier|FullyQualifiedName~ImperativeElementDomEventModifier' -v minimal -p:UseSharedCompilation=false`
- `dotnet test src/Jazor.RazorVue.RazorIr.Test/Jazor.RazorVue.RazorIr.Test.csproj --filter 'FullyQualifiedName~ElementDomEvent' -v minimal -p:UseSharedCompilation=false`

最新结果：RazorVue DOM event focused 11 通过，0 失败，0 跳过；Razor IR DOM event focused 4 通过，0 失败，0 跳过。新增覆盖 Razor IR `.razor` authored `@onclick` / `@onclick:preventDefault` / `@onclick:stopPropagation` 到 render tree metadata 与 SFC `.vue` event handler lowering 的完整路径，并锁定 raw markup inline lambda handler 的 probe compilation ownership。
