# RazorVue 作者面诊断与支持决策路线图

> 状态：D0-D5 已实现并通过当前主线回归（2026-08-17）。范围是 official Razor Source Generator 生成的 C# -> RazorVue direct-render `.mjs`，以及把最终 Compilation 失败报告给作者的完整生成管线。

## 目标与契约

Razor SDK/Roslyn 负责 Razor 绑定、C# 类型和语法语义；RazorVue 只消费 final Compilation 中可解释的 `BuildRenderTree(RenderTreeBuilder)` 协议，再由 `Jazor.Compiler` 降低 C# 表达式。两层职责不能互相复制，也不能把 RazorVue 的限制伪装成 Razor 语法错误。

一个 SDK 已接受、但落在 RazorVue 支持切片之外的作者形状只有三种可接受结果：

| 结果 | 验收定义 | D0-D5 状态 |
| --- | --- | --- |
| `Support` | lowering 有确定性回归；涉及 reactive state 的形状有真实 ECMAScript Proxy 运行时证明 | 已完成，见 D1 |
| `Reject` | 稳定 ID、作者源位置、原因、HelpLink 和可执行替代 | 已完成，见 D2/D3 |
| 静默劣化 | 生成成功但运行时必然失败、错误被吞或行为错误 | A 级已清零；新风险必须先登记 |

**final Compilation pipeline 是唯一正确性裁决者。** 候选发现、generated-C# binding、member closure、VueInject、compiler bridge、RenderTree lowering、Vue module framing、catalog 生成和 hook reporting 都属于同一错误边界。只要存在错误，就不生成部分 artifact catalog 或部分 `.mjs` 声明。

## 交付状态

| 阶段 | 决策/交付物 | 状态 |
| --- | --- | --- |
| D0 | typed diagnostic carrier、mapped location、稳定聚合和 no-partial-catalog | **Complete** |
| D1 | member class 进入 Vue Proxy 的 proxy-safe lowering | **Complete / Support** |
| D2 | 作者可达失败按 Support/Reject 分类，覆盖 59 个历史 direct-render guard | **Complete** |
| D3 | `JAZORVGA021`-`026` descriptor、HelpLink 和 source contract | **Complete** |
| D4 | generated-code early analyzer spike 的交付决策 | **No-go；作者源码 compatibility analyzer 由 M5 另行交付** |
| D5 | 作者指南、preview SDK/Roslyn/Razor 升级门禁和回归入口 | **Complete** |

已知作者可达列表没有遗留 `Investigate`。未来新增边界必须先在 D2 ledger 登记，再选择 `Support` 或 `Reject`；不能以“暂时不报错”作为第三种状态。

## D0：Final-pipeline diagnostic transport

`src/Jazor.RazorVue/Generation/RazorVueDiagnosticInfo.cs` 提供 typed `RazorVueDiagnosticInfo`、category、已渲染 detail、primary/additional locations 和 component identity。ID、severity 与 HelpLink 由 `Diagnostics` 的 descriptor 集中定义。`RazorVueDiagnosticException` 只负责跨现有 exception-shaped API 搬运该 carrier；生产 reporting 不解析异常文本，也不读取 `Exception.Data`。

来自 `Jazor.Compiler` 的 `OperationTransformationException`、`SyntaxNodeTransformationException` 与 `SymbolTransformationException` 统一归为 `CompilerBridge` 并保留其 typed source location；未包装的 `RenderEmitter` `OperationTransformationException` 则仍归属于 `DirectRender`，因为它表示 `RenderTreeBuilder` 协议而非 C# 语义桥接失败。

已接入的边界：`GeneratedCSharpBinder`、`MemberClosureBuilder`、`VueInjectRegistry`、`RenderEmitter`、`VueModuleBuilder`、`RazorTailOutput`、`InitializeHookInstaller` 和 `RazorVueGenerator`。旧的 string `TryEmit`/binding overload 只保留给现有 compiler-facing compatibility tests，production path 使用 typed overload。

位置规则：mapped `.razor` span 优先，其次 authored `.razor.cs`/symbol span，最后才是 generated C# span；无法归属作者输入的 bootstrap/internal failure 使用 `Location.None`。独立组件按 component identity、mapped path/span、diagnostic ID 稳定排序；组件在第一个不可靠状态后停止，避免 cascade。

D0 的 official Razor SG + patched `GeneratorDriver` 回归覆盖：

- RenderTree protocol failure -> mapped `.razor` location；
- `Jazor.Compiler`/`SemanticWalker` bridge failure -> mapped location；
- component/member/module/registry declaration failure；
- bootstrap/internal fallback -> `JAZORVGA020` + `Location.None`；
- 两个独立错误组件的稳定聚合和“无部分 catalog”。

## D1：Reactive member-class 风险闭环

### 决策：Support

问题是 JavaScript `#private` field 的 brand check 针对 Proxy receiver 会失败。把 member class 改成 record 或只拒绝某一种直接赋值模式都不能覆盖字段、auto-property、primary-constructor capture 和 event storage 的完整可达面。

RazorVue 的 member-closure profile 现在使用 `RuntimeClassPrivateStorage.ProxySafeMangledProperties`：

- private field、auto-property backing field、primary-constructor capture 和 field-like event storage 统一使用 `$jazor$private$...` ordinary property；
- storage 名称与普通成员命名空间做确定性冲突检查；
- class identity、继承、getter/setter、求值顺序和事件更新协议保持不变；
- `AstConverter` 标准 profile 仍可使用 JavaScript private fields，proxy-safe 只由 RazorVue profile 选择。

真实证明入口：

- `RazorSgOfficialNestedRuntimeClassClosureRuntimeTests.BuildComponent_OfficialRazorReactiveNestedRuntimeClass_UsesProxySafePrivateStorage` 使用 official Razor SG 和递归 `Proxy`，验证初始 render、事件更新和 Proxy `set` trap；
- `AstConverterReachableBranchClosureTests.Convert_ProxySafeRuntimeClassStorage_CoversInheritanceAutoPropertiesPrimaryCaptureAndEvents` 覆盖静态 AST 形状；
- 发布 consumer gate 继续验证 packaged SSR、部署路径和 Edge hydration。文档只宣称已运行的 Deno Proxy 证明，不把静态 snapshot 冒充浏览器语义证明。

组件参数仍遵守 Vue 的 shallow-prop contract：替换 prop 引用会触发更新；同一引用内部的 nested mutation 不定义为新的参数赋值保证。该语义写入作者指南，不增加误报率高的硬诊断。

## D2：作者可达失败分类 ledger

下表保留 `RenderEmitter.cs` 的 59 个历史 `throw Unsupported` call-site 审计索引（其中 `2463/2465` 已升级为 typed compiler-bridge sentinel）。行号是审计索引，不是对外协议；对外协议是 category、ID、source location 和 HelpLink。普通 `break`/`continue` 在可识别 loop 中已经升级为 Support，不再属于“direct-render 一律拒绝”的旧结论；表中的 control-flow Reject 只描述仍未形成稳定 lowering 协议的形状。

| 失败族与 call-site | 决策 | ID / HelpLink | owner | source kind | 作者源码提前判断 | 作者替代与最小回归 |
| --- | --- | --- | --- | --- | --- | --- |
| ordinary loop `break`/`continue`，目标绑定到当前 `for`/`foreach`/`while`/`do while` 且 branch 前 frame 已关闭 | `Support` | 无诊断；imperative loop path | final Compilation / RenderEmitter | authored 或 mapped | No；需要最终 operation/frame 状态 | 保持 render segment 完整；`RazorSgOfficialLoopBranches` 与 handwritten builder runtime 回归 |
| remaining operation/control-flow shape：`397,430,453,506,529,849,916,943,980,1042,1051,1154,1260,1281` | `Reject` | `JAZORVGA021` / `#direct-render` | final Compilation / RenderEmitter | authored 或 mapped | No；依赖最终 operation/frame 状态 | 把计算移到 frame 外，使用已支持 loop/content segment；failure matrix + official loop tests |
| frame stack、close order、属性/children 和 metadata timing：`304,1122,1330,1399,1711,1745,1789,1793,1803,2037,2041,2048,2057,2060,2078,2083,2095,2102,2209,2295,4780` | `Reject` | `JAZORVGA021` / `#direct-render` | final Compilation / RenderEmitter | authored 或 mapped | No；需要完整协议状态 | 调整 builder 顺序、frame target 和 fragment source；coverage/boundary matrix |
| compile-time names、overload 和 component resolution：`719,725,1441,1581,1715,1748,2316,4082,4617,4648` | `Reject` | `JAZORVGA021` / `#direct-render` | final Compilation / RenderEmitter | authored 或 mapped | No；避免与 Razor SDK 规则漂移 | 静态名称、generic component 或 `typeof(T)`；direct-render matrix |
| RenderFragment/slot source、递归和 helper closure：`732,1692,2115,2130,2157,2504,2821,2842,2885,3004,3281,3361` | `Reject` | `JAZORVGA021` / `#direct-render`；compiler access failure 另为 `022` | final Compilation + compiler bridge | authored 或 mapped | No；来源解析依赖最终 closure | 使用 inline/local/helper/component-slot source；fragment boundary matrix |
| AST/compiler bridge sentinel：`2463,2465` 以及 `SemanticWalker` typed transformation failures | `Reject` | `JAZORVGA022` / `#compiler-boundary` | `Jazor.Compiler` + RazorVue wrapper | authored、mapped 或 generated fallback | No；不在 RazorVue 重复 whitelist/host 语义 | 使用受支持 CLR/host API；compiler bridge and mapped-location tests |

上述条目继续覆盖历史 59 个 direct-render guard 的审计范围；Support 行明确标出已经升级的普通 loop branch。其它作者可达边界按 pipeline owner 分类：

| 边界 | 决策 | ID | 位置/替代 |
| --- | --- | --- | --- |
| final component binding、缺失/不可绑定 `BuildRenderTree` | `Reject` | `JAZORVGA023` | component symbol 或 generated declaration location；修正组件形状和模块声明 |
| member closure、导出名或可达成员冲突 | `Reject` | `JAZORVGA024` | member/symbol location；缩小可达成员或使用受支持类型 |
| source component/base parameterless constructor replay、static module lifetime | `Support` | 无诊断；artifact/setup contract | constructors 与 state initializer 走 base-to-derived；static storage 不进入 setup state |
| primary-constructor 参数、参数化 activation、`this(...)`、`base(args)` | `Reject` | `JAZORVGA024` | 改用 `[Parameter]`/VueInject 或无参 constructor/lifecycle；不能把 constructor 参数隐式当作 props |
| `SetParametersAsync(ParameterView)` 标准 override | `Compatibility Adapter`（当前 In proof） | 无 authored reject；未支持的 ParameterView 成员使用 `JAZORVCA003+` | 保持标准 Blazor 入口；adapter 负责 snapshot、sparse overlay 和 lifecycle 顺序 |
| `[VueInject]` container/implementation/重复声明 | `Reject` | `JAZORVGA025` | attribute/contract location；修正声明，不加 runtime fallback |
| Vue module/import/helper/framing | `Reject` | `JAZORVGA026` | module/import location；使用稳定 module/package contract |
| bootstrap、未知、未分类内部不变量 | `Reject`（内部） | `JAZORVGA020` | 可能为 `Location.None`；提交最小复现，不把它当作新的作者规则 |

所有已知失败都已选择 `Support` 或 `Reject`；没有把低频长尾伪装成永久 `Investigate`。高频需求若要升级为 Support，必须先补语义和真实运行时回归。

## D3：分类 final diagnostics

`src/Jazor.RazorVue/Generation/Diagnostics.cs` 的公开 descriptor 契约如下：

| Category | ID | title/HelpLink |
| --- | --- | --- |
| Internal | `JAZORVGA020` | final Compilation / `#final-compilation` |
| DirectRender | `JAZORVGA021` | direct render shape / `#direct-render` |
| CompilerBridge | `JAZORVGA022` | compiler bridge / `#compiler-boundary` |
| ComponentBinding | `JAZORVGA023` | component binding / `#component-binding` |
| MemberClosure | `JAZORVGA024` | member closure / `#member-closure` |
| VueInject | `JAZORVGA025` | VueInject declaration / `#vue-inject` |
| VueModule | `JAZORVGA026` | Vue module / `#vue-module` |

预期作者失败只能由 typed category 创建 descriptor；不存在依据 message text 选择 ID 的逻辑。`RazorVueDiagnosticDescriptorTests` 锁定每个 category 的 ID、severity 和 HelpLink；`RazorSgDirectRenderFailureMatrixTests` 对全量失败族断言 category、作者位置、detail 和替代锚点；bootstrap/tail-output tests 断言 mapped path、line/column、fallback 类型和 no-partial-catalog。

## D4：generated-code early analyzer 决策

### No-go：不交付 generated-code early analyzer

历史 spike 结论仍然有效，但它只针对把 Razor SG generated C# 再交给通用 `Jazor.Analyzer` 或另一个 generated-code analyzer：final Compilation 已经能在同一次 GeneratorDriver completion 中得到准确的 mapped `.razor` 位置、稳定聚合和唯一 build error；现有 `Jazor.Analyzer` 还必须保持 `GeneratedCodeAnalysisFlags.None`，它的作用域不是 Razor SG RenderTree 协议。重复分析 generated C# 会引入 RazorVue/ComponentSelector 范围漂移，也不能提前解决 frame stack 或 fragment source 的最终状态问题。

M5 的产品目标不同：为了让页面作者不必预先学习 RazorVue 的限制，另行交付**作者源码 compatibility analyzer**。它只读取作者写下的 `.razor`、`.razor.cs` 和普通 C# component source，在原始 span 上解释 server-only dependency、未注册 browser adapter 等可以高置信判断的环境差异；它不分析 `.razor.g.cs`，也不替代 final Compilation 对 RenderTree/closure/module 协议的裁决。该 analyzer 的详细边界、去重契约和阶段安排见 [`razorvue-developer-experience.md`](./razorvue-developer-experience.md)。

因此：

- 不修改 `Jazor.Analyzer` 的 generated-code 契约；
- 不把 `JAZORVGA020`-`026` 的 final-protocol descriptor 复制到作者源码 analyzer；两者使用独立的 compatibility ID 段和共享的去重语义；
- `RazorVueAnalyzerScopeTests` 继续保证通用 analyzer 不污染 RazorVue 组件面；新增 source-analyzer tests 保证诊断锚定作者文件，final pipeline 仍只对 RenderTree/closure/module 失败负责；
- analyzer 诊断本身不假定能够停止 source generator。无论 analyzer 是否已报错，final generator 都必须独立执行 no-partial-catalog 不变量；同一已知形状只能由互斥 owner 报告，或由统一 build/diagnostic aggregation 层按 rule key + source span 合并，不能假定 generator 能读取 analyzer 结果；
- 只有在新增作者源码规则无法证明位置、语义或零重复收益时，才将该规则留给 final diagnostic，而不是退回 generated-code analyzer。

## D5：作者指南与 SDK 升级门禁

作者指南已落在 [`docs/03-guides/razorvue-authoring.md`](../03-guides/razorvue-authoring.md)，并包含所有 descriptor HelpLink 锚点：`final-compilation`、`direct-render`、`compiler-boundary`、`component-binding`、`member-closure`、`vue-inject`、`vue-module`。指南覆盖 direct-render imperative loop、ordinary/labeled branch 边界、source constructor replay、static module lifetime、Proxy-safe class、Vue shallow props 和 native union 参数。

每次 .NET/Roslyn/Razor SDK preview 升级必须重新运行以下 gates，不能只依赖旧 snapshot：

1. `SemanticWalkerOrdinaryTest`：ordinary/labeled `IBranchOperation`、`BranchKind`、labeled syntax 可见性和显式拒绝；
2. `RazorSgOfficialForLoopRuntimeTests`：official `for`、`while`、`do while` 的生成与运行时行为，以及普通 branch 的 imperative loop path；
3. `RazorSgOfficialNativeUnionParameterAuthoringTests`：native union component parameter 经 official Razor SG 编译、绑定并进入最终模块；
4. `BootstrapPatchTests`（文件 `RazorSourceGeneratorBootstrapPatchTests.cs`）：mapped diagnostic、HelpLink 和错误时无 catalog；
5. `RazorSgOfficialNestedRuntimeClassClosureRuntimeTests`：deep Proxy 下 member-class 的更新和 storage 语义；
6. `RazorSgOfficialRuntimeAuthoringTests`：source base lifecycle/dispose、parameterless constructor replay、static module lifetime 和复杂 `@code` control flow；
7. `dotnet test` 的 Razor SG、Compiler、Emit 主线，以及 Windows SSR consumer gate 的 packaged DenoHost/Edge hydration 验证。

新作者可达边界的合入门禁是同一组交付物：Support 需要语义/运行时回归，Reject 需要 category/ID、作者位置、HelpLink、稳定参数和本指南条目；任何生成成功但行为未知的形状都不能合入。

## D6：新增待裁决边界登记（2026-08-18，来自 JazorAdmin M2 设计系统实测）

以下三个形状均为**编译通过、产物必然运行时失败**的 A 级静默劣化，按本文契约必须先登记再裁决。三者都在 JazorAdmin 重写中被真实触发，当前以作者侧替代绕过；浏览器 smoke 是唯一拦截线。

| 编号 | 形状 | 现状（实测） | 决策候选 | JazorAdmin 已落地替代 |
| --- | --- | --- | --- | --- |
| F1 泛型组件 TypeInference 辅助 | `.razor` 标记使用泛型组件（如 `<AdminTable Data=... Columns=...>`，T 由实参推断）。official SG 生成 `TypeInference.CreateX_0<T>` 静态辅助，体内为开放式 `OpenComponent<AdminTable<T>>` | 根渲染体和宿主组件子 RenderFragment 均按同一 helper 内联路径处理；构造泛型方法的参数绑定到 `OriginalDefinition`，因此嵌套 slot 使用当前 fragment builder，最终组件类型参数擦除且不泄漏 `__builder` | **Support**。继续保持单次求值顺序；只有 helper body 本身包含未建模 RenderTree 协议时才报告对应 direct-render 诊断 | 泛型组件可直接使用，闭式 wrapper 仍可作为需要固定 API/模块名时的可选写法 |
| F2 authored BuildRenderTree 中的开放泛型实例化 | 桥接基类（如 `AdminTable<T>`）的 `BuildRenderTree` 含 `builder.OpenComponent<TTable<T>>(0)`，T 为类自身泛型参数；最终组件可能是 `AdminTable<SettingView>` 这样的闭式派生类型 | `ResolveOpenComponentType` 沿用组件导入和类型参数擦除路径；direct-render 入口与 helper/slot 入口均把构造方法参数绑定到 `OriginalDefinition`，开放类型不会作为 JS 标识符发出，闭式 `TForm<TJsonObject>`/`TTable<ClosedRow>` 同样按 `createBlock` 翻译 | **Support**。类型参数仅是编译期注解；组件本身必须满足 `ComponentBase` + `IVueComponent`（或派生接口），并有 `[ECMAScriptModule]`/`[ECMAScript(..., Transform.Component, ...)]` 描述，运行时敏感的动态 `Type` 仍按 `JAZORVGA021` | 基类可保留泛型 BuildRenderTree；闭式 wrapper 仅在业务需要固定类型入口时使用 |
| F3 被非渲染方法引用的 RenderFragment 成员遭静默裁剪 | `private RenderFragment<...> XxxCell => context => builder => {...}`（属性或方法形态），由普通计算成员（列定义集合初始化器）引用 `Cell = XxxCell()` | member closure 会从任意已发射成员继续遍历，RenderFragment 返回方法/属性不再被过宽的 direct-template 过滤器裁剪；`Columns()` 与 `XxxCell` 同时进入模块并保持确定性 | **Support**。可达性闭包的契约是“从任一已发射组件成员可达即保留”，不区分是否直接来自渲染树。若未来某种片段语法仍无法 lowering，必须报告 typed `JAZORVGA024`，禁止生成未定义标识符 | 集合初始化器内联 double-cast lambda 仍可用；独立 RenderFragment 成员现在也可由普通 helper 安全引用 |

### 转译器优化方向（按杠杆排序）

1. **产物引用完整性校验（回归保险）**：`.mjs` 生成后增加一次廉价静态检查——模块内自由标识符必须在模块定义/导入集合中。该检查用于捕获未来 lowering 回归，不替代 F1/F2 的源语义支持，也不引入运行时兜底。
2. **TypeInference 构造方法绑定**：继续覆盖根渲染体、普通 slot、嵌套 slot 和多层泛型参数推断；方法体操作树必须使用 `OriginalDefinition` 参数符号，组件类型参数按现有擦除契约处理。
3. **member closure 保留被引用成员**（F3 的 Support 路径）：从任意已发射方法可达的实例成员不得被裁剪；若片段 body 本身不在支持切片内，再报告 typed `JAZORVGA024`。
4. **union 的 lambda 工效**：delegate 分支 union 无法从 lambda 直接 target-type（CS1660 必须双重 cast）。属 C# 语言限制；可为含 delegate 分支的 union 生成 `Of(...)` 窄工厂缓解作者面（符合 From(...) 的窄桥接定位）。
5. **RenderTreeBuilder 子集文档化**：无 `AddMultipleParameters`；sequence 参数必须无副作用（`seq++` 已有显式拒绝，行为正确）；桥接组件须直线的约束建议进作者指南。

### 已验证的 Support 形状（建议补回归锁定）

- 闭式泛型桥接组件（`TForm<TJsonObject>` / `TTable<ClosedRow>`）在 code-behind BuildRenderTree 中完整翻译；
- 集合初始化器内联 RenderFragment lambda（含嵌套组件实例化、EventCallback、透传属性）；
- VueDataUi / VuIcons 库组件在页面渲染上下文直接实例化；
- 每类型桥接模块的 Emit 闭包与 manifest 自动收集（`admin-table-*.mjs` 全部按需物化）。

### 调试方法论备注

SG 生成的 `*_razor.g.cs` 是内存态产物；定位 F1/F2/F3 这类问题需要 `-p:EmitCompilerGeneratedFiles=true -p:CompilerGeneratedFilesOutputPath=obj/generated` 落盘后对比工作页与失败页的生成代码。建议写入贡献指南。

## 验证入口


| 范围 | 命令 |
| --- | --- |
| RazorVue final pipeline、分类、mapped location、official SG | `dotnet test src/Jazor.RazorVue.Sg.Test/Jazor.RazorVue.Sg.Test.csproj` |
| Compiler operation、branch 和 proxy-safe AST | `dotnet test src/Jazor.CompilerTest/Jazor.CompilerTest.csproj` |
| Emit/SSR artifact | `dotnet test src/Jazor.EmitTest/Jazor.EmitTest.csproj` |
| Windows SSR 发布消费者 | `dotnet run --file scripts/csharp/verify-windows-ssr-release.cs -- --path-base /todo` |
| 主线 | `dotnet run --file scripts/csharp/test-dotnet.cs` |

## 非目标

- 不支持任意 C#，也不把每个 `Unsupported` 自动转为新功能；
- 不重复 Razor SDK/Roslyn 已实施的作者期检查；
- 不为不支持形状发明 runtime fallback、防御性包装或静默降级；
- 不在 `Jazor.Analyzer` 中引入 RazorVue 所需的 generated-code 全局开关或跨方法数据流；
- 不把 Vue props 的一般 shallow-reactive 语义误诊断为 C# 或 RazorVue 编译错误。

当前 compiler lowering rationale 见 [`ImplementationPrinciples.md`](../../src/Jazor.Compiler/ImplementationPrinciples.md)，产品状态见 [`current-status.md`](./current-status.md)，历史背景只归档到 [`evolution.md`](../05-history/evolution.md)。
