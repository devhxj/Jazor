# RazorVue Razor SG Tail Injection Guidance

> Status: Accepted implementation guidance
> Date: 2026-05-08
> Scope: RazorVue 正式 SG 接入阶段，如何把 RazorVue SFC catalog/artifact 生成逻辑挂到当前 SDK Razor Source Generator 主线末尾。

本文记录当前源码阅读与 focused tests 后锁定的最优实现方向。

结论是：**官方 Razor SG 原样运行，通过受控 IL 尾部注入复用官方增量数据流，额外注册 RazorVue source output。**

这不是 wrapper，不是私跑第二遍 Razor SG，也不是在 `HostOutput` 回调里直接产源码。

## 1. 决策摘要

采用受控 IL 尾部注入作为 RazorVue 正式 SG 接入主方案：

1. 保留官方 `RazorSourceGenerator` 的 `.razor.g.cs` 生成逻辑。
2. 保留官方 `HostOutputs -> RazorGeneratorResult` 发布逻辑。
3. 在官方 Razor SG 已经形成最终 Razor/C# document 的增量数据流之后，额外注册一个 RazorVue source output。
4. 新增 source output 负责生成 RazorVue SFC artifact 与 catalog source。
5. RazorVue 后续消费侧继续保持“官方 Razor SG 内存结果输入”模型：generated C# / Roslyn operation 作为语义基线，Razor IR 作为 `.razor` SFC 增强信息，不回读 `.razor` 原文。

概念形态如下：

```csharp
official RazorSourceGenerator.Initialize(context)
{
    var csharpDocuments = ...;

    context.RegisterImplementationSourceOutput(csharpDocuments, officialRazorEmit);

    // RazorVue injected output. It reuses the official SG data flow and emits RazorVue sources.
    context.RegisterImplementationSourceOutput(csharpDocuments, razorVueEmit);

    context.RegisterHostOutput(hostOutputs, officialHostOutput);
}
```

## 2. 为什么这是当前最优解

相比 wrapper / fork / nested run，这条路线对用户项目的 Razor 编译行为侵入最小：

1. 不替换 SDK Razor SG 主线。
2. 不重复执行 Razor SG。
3. 不改变官方 `.razor.g.cs` 的生成内容、hint name、诊断、缓存和设计时行为。
4. 不要求用户显式改写 Razor 文件或额外声明中间产物。
5. 不把 `.razor` 原文读取、`AdditionalText.GetText()` 或 path 回读升级为生产契约。

相比只读 `HostOutput`，这条路线能真正把 RazorVue generated sources 放进当前 compilation：

1. `HostOutput` 是宿主输出，不是源码输出。
2. `HostOutput` 适合作为 IR 结果锚点和验证点。
3. RazorVue catalog/artifact 必须通过 `SourceProductionContext.AddSource(...)` 所在的 source output 通道产出。
4. 因此正式实现应注入并列 source output，而不是试图在 `HostOutput` 回调内完成源码生成。

## 3. 已验证事实

当前仓库已通过 focused tests 证明以下事实：

1. SDK `RazorSourceGenerator` 单轮运行可以同时产出标准 Razor generated source 和 `HostOutputs`。
2. `HostOutputs` 中存在 internal `RazorGeneratorResult`。
3. 可以通过受控 bridge 从 `RazorGeneratorResult.GetCodeDocument(string physicalPath)` 取回对应 `RazorCodeDocument`。
4. Roslyn 同一轮 generator 之间不能看到彼此新生成的 partial / attribute。
5. 第二轮 compilation 才能看到上一轮 generator 输出。
6. 在真实 `dotnet build` 编译进程内，`Jazor.Analyzer` 这类 analyzer assembly 的模块初始化器会早于 `Microsoft.CodeAnalysis.Razor.Compiler` 程序集装载事件执行，而我们自己的 generator `Initialize(...)` 已经晚于 Razor compiler assembly 装载。
7. 外部构建中官方 Razor SG 传出的 `RazorCodeDocument` / `RazorCSharpDocument` 可能与 analyzer 依赖中的同名 Razor 类型存在 assembly-load-context 身份隔离；生产侧不得强转这些对象。
8. `Jazor.Analyzer` 与 `Jazor.RazorVue` 生产代码均已移除对 `Microsoft.CodeAnalysis.Razor.Compiler.dll` 的编译/复制依赖；正式桥接通过反射读取官方内存对象并投影为 Jazor 自有中立 IR DTO。

对应测试：

1. `src/Jazor.RazorVue.RazorIr.Test/RazorSourceGeneratorHostOutputTests.cs`
2. `src/Jazor.RazorVue.RazorIr.Test/RazorSourceGeneratorCarrierBridgeTests.cs`
3. `src/Jazor.RazorVue.RazorIr.Test/RoslynGeneratorVisibilityTests.cs`
4. `src/Jazor.RazorVue.RazorIr.Test/RazorSourceGeneratorLoadTimingTests.cs`
5. `src/Jazor.RazorVue.RazorIr.Test/RazorVueReflectedRazorIrReaderTests.cs`
6. `src/Jazor.RazorVue.RazorIr.Test/ProductionRazorCompilerReferenceTests.cs`

这些事实共同排除了 companion generator 同轮消费官方 Razor SG 输出的方案，也排除了 production nested run。
同时它们把“抢在官方 Razor SG 前执行挂钩”的可用早期入口收敛为：**analyzer assembly 装载期的模块初始化/等价早期静态入口**，而不是我们自己的 generator `Initialize(...)`。

## 4. HostOutput 与类型边界的定位

`HostOutput` 在本方案中的定位是：

1. 证明官方 Razor SG 末端已经拥有完整 Razor generation result。
2. 作为定位末端数据流和验证 bridge 的锚点。
3. 作为调试和 future host integration 的观察通道。

`HostOutput` 不是：

1. RazorVue catalog/artifact 的源码产出通道。
2. generator 之间共享数据的正式通道。
3. 绕过 source output 的替代编译输入。

实现时可以利用 `RazorGeneratorResult` / `RazorCodeDocument` 的 shape 作为数据确认点，但 RazorVue generated sources 进入 compilation 必须走 source output。

当前实现还固定了一个重要加载边界：

1. `Jazor.Analyzer` 只做 Roslyn 宿主、hook、`_outputNodes` 扫描和 tuple/object shape 读取。
2. `Jazor.Analyzer` 不引用、不复制 `Microsoft.CodeAnalysis.Razor.Compiler.dll`，避免制造第二份 Razor 类型身份。
3. 官方 SG 对象先按 full name 与 tuple field shape 识别，再以 `object` 传给 RazorVue bridge。
4. `Jazor.RazorVue` 生产代码同样不引用、不复制 `Microsoft.CodeAnalysis.Razor.Compiler.dll`，避免把 Razor SDK internal 类型身份固化为 Jazor 的发布契约。
5. 正式 bridge 通过 `RazorVueReflectedRazorIrReader` 读取官方内存对象的 shape，并投影到 `RazorVueRazorSourceGeneratorDocument` / `RazorVueRazorIrNode` / `RazorVueRazorSourceSpan` 等 Jazor 自有 DTO。
6. 生产路径不强转 `RazorCodeDocument`，不重建本地 `RazorCodeDocument`，不私跑 Razor SG；测试项目可以引用 SDK Razor Compiler 生成真实 IR，用于验证反射投影的正确性。

## 5. IL 注入边界

IL 注入必须保持窄边界：

1. 只 patch `Microsoft.NET.Sdk.Razor.SourceGenerators.RazorSourceGenerator.Initialize(...)` 的末端注册逻辑。
2. 只新增一个 RazorVue source output 注册。
3. 不改写官方 Razor source output delegate。
4. 不改写官方 `HostOutput` delegate。
5. 不修改官方 generated C# 文本。
6. 不修改 Razor parser、Razor engine passes、tag helper discovery 或 document classifier。

注入点优先选择已经形成最终 `csharpDocuments` 的位置。

但注入动作本身的安装时机必须更早：

1. 不能依赖我们自己的 generator `Initialize(...)` 再去安装 patch。
2. 当前 focused test 已证实 generator `Initialize(...)` 时 `Microsoft.CodeAnalysis.Razor.Compiler` 已经装载。
3. 因此 patch 安装入口必须前移到 `Jazor.Analyzer` analyzer assembly 装载期，例如模块初始化器或等价的最早静态引导点。

如果当前 SDK 版本下无法稳定拿到 `csharpDocuments` 数据流，则该 SDK 版本应 fail-fast，而不是退回 nested run、classic codegen 或 `.razor` 原文重建。

## 6. RazorVue source output 职责

注入的 RazorVue source output 只做以下事情：

1. 读取官方 Razor SG 已产生的 generated C#、document / IR 内存对象。
2. 按 full name、属性名、集合 shape 和必要的 node type name 进行反射投影。
3. 生成 Jazor 自有的中立 `RazorVueRazorSourceGeneratorDocument`，包含 primary/import source、generated C# text、source mappings 和 IR node tree。
4. 允许静态 Razor 文档没有 source mappings；generated span 的 `FilePath` 必须按 nullable 处理。
5. 运行 RazorVue SFC pipeline：先使用 Roslyn/`BuildRenderTree` 组件语义基线，再应用可用 Razor IR 增强，生成 artifact source 与 `Jazor.Generated.RazorVueCatalog.g.cs`。
6. 生成明确、稳定、可诊断的 hint name。
7. 对无法识别或不支持的 Razor IR shape 产出诊断或保留语义基线，不伪造增强结果。

它不得做以下事情：

1. 重新读取 `.razor` 文件作为语义输入。
2. 基于文件系统或 `AdditionalText.GetText()` 重新运行 `RazorProjectEngine.Process(...)` 作为生产主线。
3. 从 `BuildRenderTree` 反推 Razor 原文或伪造 Razor IR。官方 SG 后已绑定的 `BuildRenderTree` / Roslyn `IOperation` 仍是组件语义基线。
4. 静默降级到空 artifact/catalog 或伪 artifact/catalog。
5. 改变官方 Razor component class 的 public surface。

不再保留“生产侧引用 Razor Compiler 后重建本地 `RazorCodeDocument`”的例外。跨越 analyzer/Razor SG load-context 类型身份隔离的唯一正式方式是反射读取官方内存对象并投影到 Jazor 自有中立 IR DTO。

## 7. 版本与失败策略

由于这是 internal / IL 层面的尾部注入，必须把技术风险显式收口：

1. 绑定 `RazorSourceGenerator.Initialize(...)` 的 IL 指纹和 declared method surface。
2. 把 assembly path / version / MVID 仅作为测试观测信息，不作为正式兼容门。
3. 自有 native `Initialize` hook 安装前必须先运行同一套兼容校验；校验失败时不 patch 官方 Razor SG，并记录 bootstrap failure。
4. 校验失败时，在 RazorVue 启用场景下给出明确诊断并停止 RazorVue tail output 生成。
5. RazorVue 未启用时，不注入、不影响普通 Razor 项目。
6. SDK 升级必须先更新指纹和 focused tests，再允许进入生产路径。

当前实现用 `RazorSourceGeneratorInitializeHookInstaller.ValidateAssemblyForPatch(...)` 将 guard 接入 patch 前置路径。`Assembly.Location`、assembly version 和 MVID 仍只作为 trace/排查信息；动态或特殊 load context 下没有 assembly path 不应单独导致兼容失败。

RazorVue SG integration 启用后，且当前 compilation 存在 RazorVue component candidate 时，tail output 的输入异常必须 fail-fast：

1. 读不懂官方 Razor SG output shape。
2. 未收到任何 Razor SG document。
3. 只收到 suppressed document。
4. bridge 无法把官方对象投影为 Jazor 中立 IR。

这些场景必须报告 `JAZORVGA020`，不能只写 test trace 或静默不产 catalog/artifact。
如果当前 compilation 没有 RazorVue component candidate，则 tail output 允许 no-op；这覆盖“项目引用/启用 RazorVue 包但当前编译单元没有 RazorVue 组件”的合法场景，不能误伤普通 Razor 或非组件项目。

普通 `RazorVueGenerator` 在 integration 启用后只负责诊断守门和让路：

1. 如果 bootstrap patch 已失败，报告 `JAZORVGA019`，并带出 bootstrap failure。
2. 如果当前 generator context 已注册 tail output，普通 generator 不再产 catalog/artifact，也不重复跑兼容 probe。
3. 如果只是进程级历史状态显示 tail output 曾经注册，但当前 generator context 没有注册证据，不能让路，必须继续按未接管处理。
4. 如果当前 compilation 需要 RazorVue tail output，但当前 generator context 没有 tail output 注册且 SDK shape 兼容，报告 `JAZORVGA018`。
5. 如果 Roslyn 当前 `IncrementalGeneratorInitializationContext` 的 output-node state 取不到，则无法证明“当前 context 已接管”，必须报告 `JAZORVGA019`，不能退化成进程级历史状态判断。
6. 如果当前 compilation 不需要 RazorVue tail output，不报告 integration 诊断。

Hook 注册去重也必须按当前 generator context + source node 组合处理，不能只按进程级 source node 去重。否则 compiler server 同进程多项目/多 driver run 可能让后续 context 被历史注册短路。

Hook 扫描新输出节点时必须优先挂载官方 `RegisterImplementationSourceOutput(...)` 对应的 Razor/C# document 数据流，并在 trace 中记录 `TailOutputRegistrationKind = "implementation-source-output"`。只有无法定位 implementation source-output 时，才允许退到 `HostOutput` 形状作为兜底观测路径；这类情况应被视为 SDK shape 偏离，需要通过 focused test 重新评估。

失败时禁止自动回退到：

1. `.razor` 原文回读。
2. `AdditionalText.GetText()` 重建文档。
3. `BuildRenderTree` 反推 Razor 原文或伪造 Razor IR。
4. classic Razor codegen。
5. production nested Razor SG run。

## 8. 打包与启用原则

用户侧默认风险控制原则：

1. 只有 RazorVue 显式启用时才激活尾部注入。
2. 不启用 RazorVue 的项目应看到与原 SDK Razor SG 一致的行为。
3. NuGet 包应把注入逻辑收口在 `Jazor.Analyzer` analyzer/generator 载体内。
4. 不向用户暴露额外 build task 作为主入口。
5. 不要求用户手动引用 Razor compiler internals。
6. analyzer 载体不得打包 `Microsoft.CodeAnalysis.Razor.Compiler.dll`；该程序集由当前 SDK Razor SG 主线提供。
7. `Jazor.RazorVue` 生产程序集不得强引用 `Microsoft.CodeAnalysis.Razor.Compiler.dll` 或 `Microsoft.AspNetCore.Razor.Utilities.Shared.dll`。
8. 旧 `Jazor.RazorVue.RazorExtension` 专用桥接项目已删除，不再作为发布或实验入口保留。

如果实现需要随包携带辅助 IL patcher 或 bridge assembly，它们必须只服务于 `Jazor.Analyzer` 的 RazorVue SG 接入，不扩大为通用 Razor 替换层，且不得重新引入 Razor Compiler 强引用。

## 9. 验收标准

正式实现完成前必须满足：

1. 官方 `.razor.g.cs` 仍正常生成。
2. RazorVue catalog/artifact source 在同一 generator run 中生成并进入 final compilation。
3. RazorVue 消费侧能从官方 Razor SG 内存结果完成组件语义基线构建，并用 Razor IR 模型增强 SFC 输出。
4. 普通 Razor 项目未启用 RazorVue 时不产生额外 source、diagnostic 或 build 行为变化；启用但没有 RazorVue component candidate 时也不得因为缺少 RazorVue artifact 输入误报。
5. SDK 指纹不匹配时 diagnostic 清晰，可定位到 RazorVue SG injection 版本不匹配。
6. focused tests 覆盖成功注入、未启用 no-op、指纹不匹配 fail-fast、RazorVue source 产出和官方 generated source parity。
7. `dotnet pack src/Jazor/Jazor.csproj -c Release -v minimal` 成功，且 `.nupkg` 的 analyzer/lib payload 不包含 Razor Compiler / Razor Utilities Shared / Harmony / MonoMod / Detour。
8. `ProductionRazorCompilerReferenceTests` 保证生产项目和旧桥接项目不会回退到 Razor Compiler 强引用路线。
9. `RazorSourceGeneratorCompatibilityProbeTests` 覆盖 unsupported SDK shape patch 前拒绝。
10. `RazorSourceGeneratorTailOutputTests` 覆盖 enabled tail output 在有 RazorVue candidate 时输入缺失/不可读报 `JAZORVGA020`，在无 candidate 时 no-op。
11. `ESGeneratorTests` 覆盖 integration 启用后 tail 未注册报 `JAZORVGA018`、bootstrap patch 失败报 `JAZORVGA019`、当前 context 已注册 tail 时普通 generator 不误报也不接管输出、只有进程级历史注册但当前 context 未注册时仍报 `JAZORVGA018`、当前 context key 不可用时报 `JAZORVGA019`。
12. `RazorSourceGeneratorBootstrapPatchTests` 覆盖真实外部构建 trace 中 `TailOutputRegisteredForCurrentContext=true` 且 `TailOutputRegistrationKind="implementation-source-output"`。
13. `Jazor.EmitTest` RazorVue 过滤切片必须全绿，当前最新验证为 45/45 通过。
14. `CreateLocalPackage_IncludesRazorVueAuthoringAssets` 必须同时断言当前 analyzer payload 完整、且 `.nupkg` 中不存在 Razor Compiler / Razor Utilities Shared / Harmony / MonoMod / Detour payload。
15. `samples/RazorVue.TodoList/build-local.cs` 必须通过本地 pack 的 `Jazor` / `ECMAScript.Vuetify` 包完成 host rebuild，并生成 SFC artifact、manifest、host requirements module 和 sidecar。
16. `samples/RazorVue.TodoList/Todo.Host/consumer` 必须通过 `dotnet run --file .\scripts\run-deno.cs -- task build`，证明生成 `.vue` 可被纯 Deno SFC 预编译 + `deno bundle` production build 消费。
17. `Build_LocalPackages_WithExternalRazorSgSfcConsumer_EmitsVueSfcArtifacts` 必须通过，证明独立临时 consumer 可只通过 NuGet 包、官方 Razor SG 和 `.razor` authoring 生成 SFC artifact。
18. `samples/RazorVue.TodoList/Todo.Host/consumer` 必须通过 `dotnet run --file .\scripts\run-deno.cs -- task smoke:ssr`，证明生成 `.vue` 至少可经纯 Deno 预编译、Vue server renderer 和 Vuetify plugin 做 runtime render，不出现 Vue prop 类型 warning。
19. SFC component prop lowering 必须保留类型语义：字符串 literal 可输出静态属性，Boolean / numeric / null / other 非字符串 literal 必须输出 Vue bound prop，避免把 library component props 统一降成字符串。
20. `Build_LocalPackages_WithExternalRazorSgSfcConsumer_PureDenoPipeline_PassesInIsolatedWorkspace` 与 `Build_LocalPackages_RazorVueTodoListSample_PureDenoPipeline_PassesInIsolatedWorkspace` 必须通过，覆盖独立外部纯 Deno consumer、TodoList sample consumer、`deno bundle`、`Deno.bundle()`、SSR smoke 和真实浏览器 smoke。

## 10. 后续执行顺序

1. 复跑 `dotnet pack src/Jazor/Jazor.csproj -c Release -v minimal` 与包内容负向守卫，确保不携带 Razor Compiler / Razor Utilities Shared。
2. 将 pure Deno build、`Deno.bundle()`、SSR smoke 和真实浏览器 smoke 接入发布流水线，避免只停留在 focused integration test。
3. 扩展 Razor SG document 数据面到 imports、document identity 和后续模板所需结构。
4. 建立当前支持/unsupported 矩阵，明确 Razor 语法、生命周期、slot/bind/event、source map、HMR 的发布边界。
5. 验证 `.razor -> .vue -> bundled JS` sourcemap/source-origin 在真实浏览器调试中的闭环。

当前已通过的样例与前端工具链验证：

1. `dotnet run --file ./samples/RazorVue.TodoList/build-local.cs` 已通过，输出包含 `razorvueSfcArtifacts=2`。
2. `samples/RazorVue.TodoList/Todo.Host/wwwroot/jazor/components/todo-app.vue` 已包含完整 nested Vuetify template、component import 和 DTO 属性投影，并对 `VContainer` / `VCol` 等 Boolean / numeric props 输出 `:fluid="true"`、`:cols="12"` 这类 Vue bound props。
3. `cd samples/RazorVue.TodoList/Todo.Host/consumer && dotnet run --file .\scripts\run-deno.cs -- task build` 已通过，`deno bundle` production build 成功产出浏览器 JS/CSS。
4. `Build_LocalPackages_WithExternalRazorSgSfcConsumer_EmitsVueSfcArtifacts` 已通过，覆盖独立临时 `.razor` consumer、NuGet 包消费、官方 Razor SG integration、SFC manifest/source map/origins 输出。
5. `cd samples/RazorVue.TodoList/Todo.Host/consumer && dotnet run --file .\scripts\run-deno.cs -- task smoke:ssr` 已通过，SSR smoke 经纯 Deno 预编译加载生成 SFC、渲染 DTO 投影文本并验证 host requirements。
6. `cd samples/RazorVue.TodoList/Todo.Host/consumer && dotnet run --file .\scripts\run-deno.cs -- task smoke:browser` 已通过，真实浏览器 smoke 覆盖挂载、生成 CSS/JS、Vuetify `.v-application` root、关键文本、TodoList 交互、console warning/error、runtime exception 和 network failure。
7. `Build_LocalPackages_WithExternalRazorSgSfcConsumer_PureDenoPipeline_PassesInIsolatedWorkspace` 已通过，独立临时 `.NET + Razor SG + SFC` consumer 生成 `.vue` 后，由独立纯 Deno consumer 完成 SSR、bundle API、browser build 和 browser smoke。

## 11. 一句话结论

当前最优解是：**HostOutput 用作 Razor SG 末端 IR 结果锚点，受控 IL 尾部注入新增并列 source output，用同一条官方 Razor SG 数据流获取 generated C# / Roslyn 语义基线和 IR 增强信息，生成 RazorVue SFC artifact/catalog。**
