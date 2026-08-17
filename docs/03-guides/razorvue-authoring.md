# RazorVue 作者指南：支持切片与构建诊断

> 本指南描述当前 RazorVue 的作者面（authoring surface）。Razor SDK/Roslyn 负责 Razor 绑定、C# 类型检查和语法诊断；RazorVue 只处理 official Razor Source Generator 生成的最终 C# `BuildRenderTree`，并输出 Vue render-function `.mjs`。

## 快速判断

| 形状 | 当前决策 | 说明 |
| --- | --- | --- |
| 普通 Razor 标记、静态属性、表达式属性和已覆盖的 `@if`/`@foreach` | **Support** | 由 official Razor SG 生成 C#，再进入 final Compilation lowering。 |
| `for`、`while`、`do while` 的已建模 RenderTree 内容 | **Support** | 循环体必须落在 direct-render 支持子集内。render 体内的 `break`/`continue`（含普通形式）按 `JAZORVGA021` 拒绝；普通 `break`/`continue` 只在成员方法的纯 C# 逻辑中由编译器层保留语义。 |
| labeled `break`/`continue`、`goto` | **Reject** | 当前 Roslyn operation contract 无法稳定提供作者 label，不能错误地降成无标签跳转。请改写为条件状态或拆分渲染方法。 |
| member class 实例进入 reactive state | **Support** | RazorVue 使用 `$jazor$private$...` ordinary property storage，避免 Vue Proxy receiver 触发 JavaScript `#private` brand failure。 |
| Vue prop 同一引用内部的 nested mutation | **Support with Vue semantics** | Vue props 默认 shallow reactive；替换引用会触发参数更新，但组件不会把同一引用内部变更承诺为新的参数赋值。 |
| 不在支持切片内的 RenderTree 协议或宿主表达式 | **Reject** | final Compilation 报稳定的 `JAZORVGA021+`，保留作者源位置和替代指引。 |

不要通过 `object`、字符串拼接或手写 JavaScript 绕过这些边界。这样会失去 Razor/C# 类型检查、source map 和确定性 artifact 生成。

<a id="final-compilation"></a>
## Final Compilation 与错误生命周期

RazorVue 的最终管线顺序是：组件发现 -> final Compilation binding -> member closure -> VueInject registry -> direct RenderTree lowering/compiler bridge -> Vue module framing -> artifact catalog -> source-generator reporting。任何阶段失败都会停止受影响组件；存在错误时不会生成部分 catalog 或部分模块声明。

诊断不是从异常文本猜出来的。内部使用 typed `RazorVueDiagnosticInfo` 传递 category、已渲染 detail、primary/additional locations 和 component identity；descriptor 集中拥有 ID、severity 和 HelpLink。mapped `.razor` span 优先于 generated `.razor.g.cs` span。独立组件的错误按稳定组件身份和位置排序，因此同一输入在并行构建中仍有相同输出。

### Diagnostic ID

| ID | 所属边界 | HelpLink 锚点 | 常见动作 |
| --- | --- | --- | --- |
| `JAZORVGA020` | bootstrap、未知或未分类 internal failure | `#final-compilation` | 保留完整构建日志并提交最小复现；它不代表已知作者规则。 |
| `JAZORVGA021` | direct RenderTree 协议/形状 | `#direct-render` | 按 direct-render 章节改写标记或 builder 形状。 |
| `JAZORVGA022` | C# expression/compiler bridge | `#compiler-boundary` | 让表达式使用已有 whitelist/host contract，或改用受支持的值形状。 |
| `JAZORVGA023` | component binding | `#component-binding` | 检查 `BuildRenderTree`、组件模块声明和 component parameter。 |
| `JAZORVGA024` | member closure | `#member-closure` | 缩小可达成员，确保成员类型和访问方式在 compiler 支持范围。 |
| `JAZORVGA025` | `[VueInject]` declaration | `#vue-inject` | 修正 container/implementation contract 和重复声明。 |
| `JAZORVGA026` | Vue module/import/framing | `#vue-module` | 修正模块路径、导出名、import collision 或 runtime helper contract。 |

Razor SDK/Roslyn 的 `RZ****`、`CS****` 诊断仍由对应工具报告；RazorVue 不复制这些检查。

<a id="direct-render"></a>
## Direct Render

Razor SG 生成的 builder 调用按顺序解释为 Vue VNode。以下约束是协议约束，不是普通 C# 语法错误：

- `OpenElement`、`OpenComponent`、`OpenRegion` 必须与对应 close 成对，且按栈顺序关闭；
- element/component 的属性、component parameter、splat 和 event metadata 必须在第一个 child 之前写入；
- tag、attribute、parameter、event modifier 和 bulk-attribute 名称必须是 compile-time string；
- `SetKey`、`SetUpdatesAttributeName`、reference capture 和 render-mode metadata 必须作用于正确的当前 frame；
- `OpenComponent` 使用 generic component type 或 `typeof(T)`，不能把运行时 `Type` 值当作动态组件类型；
- `RenderFragment`/slot 必须能解析为 inline、local、helper 或 component slot source；任意外部 factory、递归 render helper 和未闭合 fragment 会被拒绝；
- sequence 参数只允许无副作用表达式；sequence 不是运行时排序值，不要用 `NextSequence()` 之类调用填充它。

### 循环与分支

已支持的循环会生成 Vue fragment：`@foreach` 使用 `renderList`，普通 `@for`、`@while`、`@do while` 使用受控 fragment lowering。循环体必须包含可识别的 RenderTreeBuilder content segment；需要临时变量的 initializer/condition/update 或混杂未建模语句会得到 `JAZORVGA021`。

direct render 的 `BuildRenderTree` 是直线协议：循环体内出现任何 `break`/`continue`（普通或 labeled）或 `goto` 都会得到 `JAZORVGA021`。labeled `break`/`continue` 额外受 Roslyn operation contract 限制——作者 label 没有稳定的结构化投影，编译器层同样显式拒绝，绝不把 `break outer` 输出成无标签 `break`。普通 `break`/`continue` 只在组件成员方法（非 render 体）的纯 C# 逻辑中由编译器层保留语义。推荐使用布尔 guard、提前结束当前 render helper，或将目标循环拆成独立受支持的片段。

### 常见替代

| 失败写法 | 推荐写法 |
| --- | --- |
| `builder.OpenElement(0, tag)` | 使用静态标签，或为不同标签写显式 `@if` 分支。 |
| child 之后再 `AddAttribute`/`SetKey` | 在 open frame 后、任何 child 前设置 metadata。 |
| `AddContent(0, SomeFactory())` 返回未知 `RenderFragment` | 使用 inline fragment、已声明的 slot 或可分析的 helper。 |
| 在 frame 中声明未初始化 local | 先初始化，或把纯 C# 计算移到 frame 之前。 |
| 动态 `OpenComponent(0, type)` | 使用静态组件类型或显式组件分支。 |

<a id="compiler-boundary"></a>
## Compiler Boundary

RazorVue 不会在 direct-render 层重新实现 C# 成员、调用、转换或 whitelist 语义。表达式交给 `Jazor.Compiler`/`SemanticWalker`；当 operation 无法生成 JavaScript expression、访问了未支持的 external type/member，或 host mapping 失败时，报告 `JAZORVGA022`。

诊断位置来自 Roslyn operation/symbol 的原始 `Location`，再通过 mapped span 投影回 `.razor` 或作者 `.razor.cs`。不要依赖异常消息中的 generated C# 行号。泛型参数、数组元素和集合元素的类型在未进入运行时敏感 lowering 时保持 erased；真正的成员访问、构造、运行时类型检查才会在使用点拒绝。

<a id="component-binding"></a>
## Component Binding

组件必须能从 final Compilation 中解析出可绑定的 `BuildRenderTree(RenderTreeBuilder)` block。官方 Razor SG 生成的 component parameter、required parameter 和参数类型错误仍由 Razor SDK 负责；RazorVue 只报告它无法绑定或无法消费的最终形状。

组件模块应使用稳定的 `[ECMAScriptModule("...")]` 或已声明的 Vue library contract。组件引用、parameter 名称和 child content 必须与编译期 symbol 对齐；不要依赖运行时字符串查找组件。

<a id="member-closure"></a>
## Member Closure 与 Reactive Class

member closure 只物化组件 render 可达的字段、属性、方法、nested runtime class 和其依赖。无法确定成员导出名、类型或引用关系时报告 `JAZORVGA024`，而不是生成一个运行时才失败的空引用。

### Proxy-safe class storage

当 runtime member class 进入 Vue `reactive()` 或其他 deep Proxy，JavaScript private field 的 brand check 会针对 Proxy receiver 失败。RazorVue 在 Vue member-closure profile 中把非 public field、auto-property backing field、primary-constructor capture 和 field-like event storage 降为稳定的普通 mangled property，例如 `$jazor$private$...`。它仍保持 class identity、继承和访问顺序；该名称是实现细节，不应在作者代码中引用。

这项 Support 由 official Razor SG + deep Proxy Deno regression 覆盖。不要把 SSR runner 的显式失败或把 class 改成 record 当作通用修复。组件参数仍遵守 Vue 的 shallow-prop 语义：需要触发父子更新时替换整个引用，或使用明确的 Vue ref/reactive contract。

<a id="vue-inject"></a>
## VueInject

`[VueInject]` 是 compilation 级声明协议。注入角色必须引用命名 component type，container contract、implementation 和导出名必须满足当前 registry 规则，重复或冲突声明会报告 `JAZORVGA025`。修正声明本身，不要在组件里添加运行时 fallback；registry 失败时不会生成部分 catalog。

<a id="vue-module"></a>
## Vue Module 与 Union

`JAZORVGA026` 覆盖模块 framing、import alias、runtime helper 和 artifact materialization 失败。模块路径应稳定、可解析且与 package manifest 的实际 entry 一致；不要手工拼接 import 文本来绕过 `SemanticWalker` 的 import collection。

Vue host value domain 优先使用 C# native `union`，例如：

```csharp
[Parameter]
public Vue.VueBooleanStringValue Mode { get; set; } = true;
```

union 是 authoring/compile-time contract，运行时按其分支值擦除；保留 `AsX` projection 和正常赋值/隐式构造。官方 Razor SG 绑定也必须能编译该参数面；如果某个 union 形状不能被 Razor SG 合法绑定，应缩小为显式 overload 或强类型参数，而不是退化为 `object?`。

## 排查顺序

1. 先修复同一 compilation 中的 `CS****`/`RZ****`，它们可能使 generated C# 不完整。
2. 查看 `JAZORVGA` ID、mapped path/line/column 和 HelpLink；不要只复制异常末尾文本。
3. 按 ID 对照本指南章节，保留一个最小 `.razor` 或 `.razor.cs` 复现。
4. 确认失败时没有 `Jazor.Generated.ArtifactCatalog.g.cs` 或部分 `.mjs` 输出。
5. 若认为形状应该被支持，请同时提交：生成的 `BuildRenderTree` 形状、预期 Vue render-function、状态/SSR 语义和最小回归；不要先添加 silent fallback。

## 升级门禁

升级 .NET、Roslyn 或 Razor SDK preview 时，至少运行：

- `SemanticWalkerOrdinaryTest` 的 ordinary/labeled `IBranchOperation` 与 `BranchKind` gate；
- official Razor `for`、`while`、`do while` runtime tests；
- `RazorSgOfficialNativeUnionParameterAuthoringTests`，验证 native union 参数可由 Razor SG 绑定并进入最终模块；
- `RazorSourceGeneratorBootstrapPatchTests` 的 mapped diagnostic 和无 partial catalog gate；
- `RazorSgOfficialNestedRuntimeClassClosureRuntimeTests` 的 deep Proxy regression；
- `dotnet test src/Jazor.RazorVue.Sg.Test/Jazor.RazorVue.Sg.Test.csproj`、`dotnet test src/Jazor.CompilerTest/Jazor.CompilerTest.csproj`，以及发布/SSR consumer gate。

这些门禁检查的是 operation contract、作者位置和运行时语义；旧 snapshot 通过本身不足以证明 preview SDK 升级安全。
