# Jazor.RazorVue

> Status: active reference
> Positioning: official Razor Source Generator final-document adapter, binder, component selector, and compiler seam adapter.

`Jazor.RazorVue` 在转型分支中只负责消费 official Razor SG tail output 提供的最终生成文档。它把 generated C# 绑定到 hook compilation 的同一派生链，并为后续 `BuildRenderTree` lowering 提供 Roslyn symbol 与 `IOperation` 输入。

## Responsibilities

- `RazorSgFinalDocumentAdapter` 从 tail callback 接收 `RazorCodeDocument`、`RazorCSharpDocument` 和 generated hint name，读取稳定文档路径、最终 generated C# 与 source mappings。
- `RazorSgGeneratedCSharpBinder` 优先复用 hook compilation 中的当前 generated tree；缺失时只向同一 compilation 派生链批量加入当前 generated tree，再绑定组件类型与 `BuildRenderTree(RenderTreeBuilder)` block operation。
- `RazorSgComponentCandidateSelector` 发现并选择当前 compilation 中需要 tail output 的 RazorVue 组件，拒绝缺失、重复或歧义匹配。
- `RazorSgComponentMemberClosureBuilder` 只作为 SG 侧薄适配器：选择 `BuildRenderTree` / supported lifecycle roots，复用 `Jazor.Compiler.CurrentComponentMemberClosure`，并为后续 lowering 提供 `AstConverterOptions(RazorVueRuntime, MemberFilter, CurrentComponentSemanticWalkerHost)`。
- `RazorSgVueComponentModuleBuilder` 提供最小内存 `.mjs` framing：将 compiler 产出的可达方法包入 `defineComponent` / `setup(props)` / reactive state / render-context 调用，并生成 deterministic component id、relative path、content hash 与 `.mjs.map` payload；source map 首切片已串联 wrapper map、compiler origin map 与 Razor SG source mappings，避免停在 `.razor.g.cs`。
- `RazorSourceGeneratorTailOutput` 将 successful binding 后的 `.mjs` artifact 编码为版本化 `Jazor.Generated.VueRenderCatalog` carrier；`Jazor.Emit` 在 build 成功后负责写盘。
- 适配、文档排序、重复检测和组件选择必须保持确定性，并在输入不完整时返回可定位失败。

## Boundaries

- production 输入只来自 official Razor SG 的最终生成文档及 callback compilation。
- 不在 analyzer 内私自运行 Razor SG，不回读 `.razor` 文件，不从零创建 compilation，也不维护第二条 HostOutput/fallback 生成主线。
- `RazorCodeDocument` 反射必须保留在 final-document adapter 边界，用于跨 Razor SDK load context 读取最终 C# 文档与 source mappings；它不遍历 Razor DR/IR 节点。
- 本项目不再拥有 Razor-to-SFC frontend、catalog/artifact lowering、RazorVue authoring analyzer 或 Jolt/Vue RPC 协议。
- `Jazor.Analyzer` 负责 generator 宿主、受控 tail hook 注册、兼容性校验和诊断。
- 后续 render-function lowering 通过 `Jazor.Compiler` 的正式翻译入口消费这里提供的 Roslyn 语义，不在适配层手拼 JavaScript 或 Acornima AST。
- `Runtime/` 下的 render-context v1 JavaScript 作为 `@jazor/vue-runtime` 资产嵌入程序集；它负责 frame stack、VNode materialization、Vue runtime framing，以及将 RenderTreeBuilder event attribute（如 `onclick` / `@onclick`）规范化为 Vue `h()` handler prop（如 `onClick`）。
- `.mjs`、source map、manifest 和 bundle 的物化属于 `Jazor.Emit`。

## Current Layout

- `RazorSdk/RazorSgFinalDocumentAdapter.cs`: official SG final-document callback 输入适配。
- `RazorSdk/RazorSgGeneratedCSharpBinder.cs`: generated C# compilation 派生与 `BuildRenderTree` 语义绑定。
- `RazorSdk/RazorSgComponentCandidateSelector.cs`: RazorVue 组件发现、筛选与唯一匹配。
- `RazorSdk/RazorSgComponentMemberClosureBuilder.cs`: SG binding -> compiler current-component closure/options 适配。
- `RazorSdk/RazorSgVueComponentModuleBuilder.cs`: compiler output -> minimal in-memory Vue render-function module framing。
- `Runtime/render-context-core.mjs`: framework-neutral render-context v1 stack and validation logic.
- `Runtime/render-context.mjs`: Vue-facing render-context entrypoint and protocol version export.

## Verification

```powershell
dotnet build src/Jazor.RazorVue/Jazor.RazorVue.csproj --no-restore -v minimal
dotnet build src/Jazor.Analyzer/Jazor.Analyzer.csproj --no-restore -v minimal
dotnet test src/Jazor.RazorVue.Sg.Test/Jazor.RazorVue.Sg.Test.csproj --no-restore --filter "FullyQualifiedName~RazorVueLegacySourceRetirementTests"
dotnet test src/Jazor.RazorVue.Sg.Test/Jazor.RazorVue.Sg.Test.csproj --no-restore --filter 'ComponentMemberClosure|FinalDocument|GeneratedCSharpBinder' -v minimal /nr:false /p:UseSharedCompilation=false
dotnet test src/Jazor.RazorVue.Sg.Test/Jazor.RazorVue.Sg.Test.csproj --no-restore --filter 'BuildVueComponentModule_SourceMapChainsCompilerSegmentsToOriginalRazor|BuildVueComponentModule_UsesCSharpDefaultsForUninitializedState|BuildVueComponentModule_EmitsSetupScopedRenderFunction|BuildVueComponentModule_RuntimeKeepsStateInitializerOnceAndStableHandler' -v minimal /nr:false /p:UseSharedCompilation=false
```

## Read Next

- [Razor SG Final-Document G0 决策记录](../../docs/02-计划/RazorSgFinalDocument.G0.DecisionRecord.md)
- [Jazor 架构转型开发计划](../../docs/02-计划/Jazor%20架构转型开发计划.md)
