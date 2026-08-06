# Jazor.RazorVue

> Status: active reference
> Positioning: official Razor SG -> `IOperation` -> ESTree -> Vue render-function artifact integration.

`Jazor.RazorVue` 在转型分支中只消费 official Razor SG 完成后的最终 `Compilation`。它直接绑定其中的 generated C#，并为后续 `BuildRenderTree` lowering 提供 Roslyn symbol 与 `IOperation` 输入。

## Responsibilities

- `GeneratedCSharpBinder` 直接复用最终 compilation 中的官方 generated tree，绑定组件类型与 `BuildRenderTree(RenderTreeBuilder)` block operation。
- `ComponentSelector` 发现并选择最终 compilation 中的 RazorVue 组件。
- `MemberClosureBuilder` 选择 `BuildRenderTree` / supported lifecycle roots，并使用本项目的 `CurrentComponentMemberClosure` 建立可达成员闭包；它以 `AstConverterOptions(Standard, MemberFilter, VueSemanticWalkerHost, VueModulePolicy)` 进入核心转译器。
- `RazorSdk/Lowering/` 拥有 current-component、RenderTreeBuilder、children-to-slot、组件 state 默认值和 module projection；`VueSemanticWalkerHost` 以组合而非继承 `SemanticWalker` 的方式协调这些 projection。
- `RazorSdk/Catalog/` 拥有 EventCallback、RenderTreeBuilder 和 WebRenderTreeBuilderExtensions 的 ASP.NET Components `Op.Allowed` 声明；`Jazor.Compiler.Generator` 从该明确目录生成 compiler 消费的统一 whitelist。
- `VueModuleBuilder` 提供最小内存 `.mjs` framing：将 compiler 产出的可达方法包入 `defineComponent` / `setup(props)` / reactive state / render-context 调用，并生成 deterministic component id、relative path、content hash 与 `.mjs.map` payload；source map 首切片已串联 wrapper map、compiler origin map 与 Razor SG source mappings，避免停在 `.razor.g.cs`。
- `RazorTailOutput` 将 successful binding 后的 `.mjs` artifact 编码为版本化 `Jazor.Generated.VueRenderCatalog` carrier，并在 driver 返回前追加为 syntax tree；`Jazor.Emit` 在 build 成功后负责写盘。
- 组件选择和 artifact 排序必须保持确定性，并在绑定失败时返回可定位诊断。

## Boundaries

- production 输入只来自 `GeneratorDriver.RunGeneratorsAndUpdateCompilation` 返回的最终 compilation。
- 不在 analyzer 内私自运行 Razor SG，不回读 `.razor` 文件，不从零创建或二次解析 generated C# compilation，也不维护 HostOutput 生成主线。
- 本项目不拥有 Razor-to-SFC frontend、RazorVue authoring analyzer 或宿主 RPC 协议。
- `Generation/` 负责 generator driver 完成边界的 hook、入口和诊断；它与 lowering 编译到同一个 `Jazor.RazorVue` analyzer 程序集，但保持独立的内部职责边界。
- 后续 render-function lowering 通过 `Jazor.Compiler` 的正式翻译入口消费这里提供的 Roslyn 语义；C# expression/member/function 语义不在适配层手拼 JavaScript 或 Acornima AST。
- direct-render 的 `foreach` 解构绑定复用 `SemanticWalker.BuildForEachLoopBinding`：tuple、`KeyValuePair<TKey,TValue>` 与 structural record 按核心编译器的运行时 shape 生成 mapper 参数；未声明稳定结构的自定义 `Deconstruct` 保持显式不支持。
- production artifact 是 Vue render-function `.mjs`，不回退为 SFC、render-context marker protocol、`scope.buildRenderTree(builder)` 或 `builder.finish()`。
- DOM `@bind` 只接受当前组件 state 的直接赋值；官方 Razor SDK 为 `@bind:after` 和 `@bind:set` 生成的 `RuntimeHelpers.CreateInferredBindSetter<T>(Action<T>, T)` 或 `RuntimeHelpers.CreateInferredBindSetter<T>(Func<T, Task>, T)` 是唯一额外支持的 binder protocol，lambda 与当前组件单参方法组仍交由 compiler lowering 以保原始调用顺序。任意手写多语句 `CreateBinder` handler 不作为兼容 fallback。
- 当前组件的 `EventCallback` 与 `EventCallback<T>` 参数会映射为可选 Vue listener prop；`InvokeAsync` 保留传参和 await 完成顺序，未传 listener 时正常完成。
- `.mjs`、source map、manifest 和 bundle 的物化属于 `Jazor.Emit`。

## Current Layout

- `Generation/RazorVueGenerator.cs`: analyzer generator 入口与 driver hook 初始化。
- `Generation/RazorTailOutput.cs`: final compilation -> render catalog carrier。
- `RazorSdk/GeneratedCSharpBinder.cs`: final generated C# 与 `BuildRenderTree` 语义绑定。
- `RazorSdk/ComponentSelector.cs`: RazorVue 组件发现、筛选与唯一匹配。
- `RazorSdk/MemberClosureBuilder.cs`: SG binding -> compiler current-component closure/options 适配。
- `RazorSdk/Lowering/`: RazorVue product host、member closure、state defaults 与 direct render lowering。
- `RazorSdk/Catalog/`: RazorVue-owned ASP.NET Components whitelist catalog 与同目录文档。
- `RazorSdk/VueModuleBuilder.cs`: compiler output -> minimal in-memory Vue render-function module framing。

## Verification

```powershell
dotnet build src/Jazor.RazorVue/Jazor.RazorVue.csproj --no-restore -v minimal
dotnet build src/Jazor.Analyzer/Jazor.Analyzer.csproj --no-restore -v minimal
dotnet test src/Jazor.RazorVue.Sg.Test/Jazor.RazorVue.Sg.Test.csproj --no-restore --filter 'ComponentMemberClosure|FinalDocument|GeneratedCSharpBinder' -v minimal /nr:false /p:UseSharedCompilation=false
dotnet test src/Jazor.RazorVue.Sg.Test/Jazor.RazorVue.Sg.Test.csproj --no-restore --filter 'BuildVueComponentModule_SourceMapChainsCompilerSegmentsToOriginalRazor|BuildVueComponentModule_UsesCSharpDefaultsForUninitializedState|BuildVueComponentModule_EmitsSetupScopedRenderFunction|BuildVueComponentModule_RuntimeKeepsStateInitializerOnceAndStableHandler' -v minimal /nr:false /p:UseSharedCompilation=false
```

## Read Next

- [Razor SG Final-Document G0 决策记录](../../docs/02-计划/RazorSgFinalDocument.G0.DecisionRecord.md)
- [Jazor 架构转型开发计划](../../docs/02-计划/Jazor%20架构转型开发计划.md)
