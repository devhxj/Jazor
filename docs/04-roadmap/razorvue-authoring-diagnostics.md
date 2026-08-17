# RazorVue 作者面诊断与支持决策路线图

> 状态：D0-D5 已实现并通过当前主线回归（2026-08-16）。范围是 official Razor Source Generator 生成的 C# -> RazorVue direct-render `.mjs`，以及把最终 Compilation 失败报告给作者的完整生成管线。

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
| D4 | early analyzer spike 的交付决策 | **No-go，明确不交付** |
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

下表把 `RenderEmitter.cs` 的 59 个历史 `throw Unsupported` call-site（其中 `2463/2465` 已升级为 typed compiler-bridge sentinel）分成可维护的失败族。行号是审计索引，不是对外协议；对外协议是 category、ID、source location 和 HelpLink。

| 失败族与 call-site | 决策 | ID / HelpLink | owner | source kind | early analyzer | 作者替代与最小回归 |
| --- | --- | --- | --- | --- | --- | --- |
| operation/control-flow shape：`397,430,453,506,529,849,916,943,980,1042,1051,1154,1260,1281` | `Reject` | `JAZORVGA021` / `#direct-render` | final Compilation / RenderEmitter | authored 或 mapped | No；依赖最终 operation/frame 状态 | 把计算移到 frame 外，使用已支持 loop/content segment；failure matrix + official loop tests |
| frame stack、close order、属性/children 和 metadata timing：`304,1122,1330,1399,1711,1745,1789,1793,1803,2037,2041,2048,2057,2060,2078,2083,2095,2102,2209,2295,4780` | `Reject` | `JAZORVGA021` / `#direct-render` | final Compilation / RenderEmitter | authored 或 mapped | No；需要完整协议状态 | 调整 builder 顺序、frame target 和 fragment source；coverage/boundary matrix |
| compile-time names、overload 和 component resolution：`719,725,1441,1581,1715,1748,2316,4082,4617,4648` | `Reject` | `JAZORVGA021` / `#direct-render` | final Compilation / RenderEmitter | authored 或 mapped | No；避免与 Razor SDK 规则漂移 | 静态名称、generic component 或 `typeof(T)`；direct-render matrix |
| RenderFragment/slot source、递归和 helper closure：`732,1692,2115,2130,2157,2504,2821,2842,2885,3004,3281,3361` | `Reject` | `JAZORVGA021` / `#direct-render`；compiler access failure 另为 `022` | final Compilation + compiler bridge | authored 或 mapped | No；来源解析依赖最终 closure | 使用 inline/local/helper/component-slot source；fragment boundary matrix |
| AST/compiler bridge sentinel：`2463,2465` 以及 `SemanticWalker` typed transformation failures | `Reject` | `JAZORVGA022` / `#compiler-boundary` | `Jazor.Compiler` + RazorVue wrapper | authored、mapped 或 generated fallback | No；不在 RazorVue 重复 whitelist/host 语义 | 使用受支持 CLR/host API；compiler bridge and mapped-location tests |

上述五行合计覆盖 59 个历史 direct-render guard。其它作者可达边界按 pipeline owner 分类：

| 边界 | 决策 | ID | 位置/替代 |
| --- | --- | --- | --- |
| final component binding、缺失/不可绑定 `BuildRenderTree` | `Reject` | `JAZORVGA023` | component symbol 或 generated declaration location；修正组件形状和模块声明 |
| member closure、导出名或可达成员冲突 | `Reject` | `JAZORVGA024` | member/symbol location；缩小可达成员或使用受支持类型 |
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

## D4：early analyzer 决策

### No-go：不交付 RazorVue 专属 early analyzer

spike 结论是 final Compilation 已经能在同一次 GeneratorDriver completion 中得到准确的 mapped `.razor` 位置、稳定聚合和唯一 build error；现有 `Jazor.Analyzer` 还必须保持 `GeneratedCodeAnalysisFlags.None`，它的作用域不是 Razor SG RenderTree 协议。再添加 generated-code analyzer 会重复同一错误、引入 RazorVue/ComponentSelector 范围漂移，并不能提前解决 frame stack 或 fragment source 的最终状态问题。

因此：

- 不修改 `Jazor.Analyzer` 的 generated-code 契约；
- 不留下半启用的 `JAZORVGA` analyzer descriptor、`AnalyzerReleases` 或 warning；
- `RazorVueAnalyzerScopeTests` 继续作为否决回归：通用 analyzer 不污染 RazorVue 组件面，final pipeline 仍报告唯一 error；
- 未来只有在实测 IDE 延迟、位置和零重复收益都优于 final diagnostic 时，才重新开一个独立 spike。

## D5：作者指南与 SDK 升级门禁

作者指南已落在 [`docs/03-guides/razorvue-authoring.md`](../03-guides/razorvue-authoring.md)，并包含所有 descriptor HelpLink 锚点：`final-compilation`、`direct-render`、`compiler-boundary`、`component-binding`、`member-closure`、`vue-inject`、`vue-module`。指南覆盖 direct-render 替代写法、ordinary/labeled branch、Proxy-safe class、Vue shallow props 和 native union 参数。

每次 .NET/Roslyn/Razor SDK preview 升级必须重新运行以下 gates，不能只依赖旧 snapshot：

1. `SemanticWalkerOrdinaryTest`：ordinary/labeled `IBranchOperation`、`BranchKind`、labeled syntax 可见性和显式拒绝；
2. `RazorSgOfficialForLoopRuntimeTests`：official `for`、`while`、`do while` 的生成与运行时行为；
3. `RazorSgOfficialNativeUnionParameterAuthoringTests`：native union component parameter 经 official Razor SG 编译、绑定并进入最终模块；
4. `RazorSourceGeneratorBootstrapPatchTests`：mapped diagnostic、HelpLink 和错误时无 catalog；
5. `RazorSgOfficialNestedRuntimeClassClosureRuntimeTests`：deep Proxy 下 member-class 的更新和 storage 语义；
6. `dotnet test` 的 Razor SG、Compiler、Emit 主线，以及 Windows SSR consumer gate 的 packaged DenoHost/Edge hydration 验证。

新作者可达边界的合入门禁是同一组交付物：Support 需要语义/运行时回归，Reject 需要 category/ID、作者位置、HelpLink、稳定参数和本指南条目；任何生成成功但行为未知的形状都不能合入。

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
