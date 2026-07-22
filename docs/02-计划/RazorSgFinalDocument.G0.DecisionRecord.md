# Razor SG Final-Document G0 决策记录

> 状态：Accepted - G0 passed
>
> 日期：2026-07-22
>
> 范围：`feature/razor-sg-render-function` 转型分支的 Razor 输入边界

## 背景

旧 RazorVue 主线通过受控 Razor Source Generator tail hook 取得
`RazorCodeDocument`、`RazorCSharpDocument` 和 Razor IR，再把 IR 增强到 SFC
产物。这条路线把 SDK internal shape、IR 投影、模板/SFC lowering 和旧 Jolt
合同耦合在一起，扩展成本高，且不能成为新的 render-function 主线的边界。

转型分支保留受控 tail hook，但只把官方 Razor SG 已生成的 C# 文档当作组件
语义输入。组件语义从绑定后的 Roslyn `Compilation` 中取得，由
`BuildRenderTree(RenderTreeBuilder)` 的 `IOperation` 驱动后续 lowering。

基线为 `d68aecbb00b23aa35735c9a269b2e987c7815b05`，旧线路由 Git 历史和原有
分支保留，不迁入本分支的兼容层。

## 决定

1. production tail 只注册已 fingerprint 的 Razor SG implementation
   `SourceOutputNode`；`HostOutput` 仅允许用于 compatibility probe 和诊断，
   不得作为第二条 lowering 或 fallback 路径。
2. adapter 只投影 `HintName`、稳定 document identity、generated C#、source
   mappings 和 callback `Compilation`。它不得向 lowering 暴露
   `RazorCodeDocument`、`RazorCSharpDocument`、`DocumentIntermediateNode` 或任何
   Razor IR node。
3. adapter 不得调用 `GetDocumentNode()`，不得读取或重新解析 `.razor` 原文，
   不得 nested-run Razor SG，也不得从零创建 `CSharpCompilation`。
4. generated C# binder 以 hook callback 的 `Compilation` 为唯一基底。所有当前
   generated trees 已精确存在时直接复用该对象；否则在同一不可变派生链上按稳定
   顺序一次性补充缺失 trees。identity/hash 冲突、stale tree 或无法绑定都
   fail-fast。
5. candidate 从 SG document 映射的 generated tree 中确定。手写 `.cs`
   `BuildRenderTree` 不由 tail hook claim。
6. G0 只证明 SG-result 到 `BuildRenderTree IOperation` 的输入链。render runtime、
   `.mjs` carrier、Deno、HMR、SFC interop 和旧线路删除均不得在 G0 前开始。

## 明确不做

- 不维护 Razor-to-SFC 与 render-function 的双输出。
- 不把 Razor IR 当作 generated C# 的补偿输入。
- 不恢复 `.jazor`、Jolt LSP/DAP 或旧 Jolt 协议兼容。
- 不因 callback compilation 尚未含 generated tree 而重跑 Razor SG；只允许补入
  同一轮已收到的 generated C# text。

## 后果

- SDK internal object shape 仍被限制在 hook adapter，因而必须由 fingerprint 和
  focused compatibility tests 保护。
- callback compilation 可能不是 driver output-applied final compilation；这不是
  错误。G0 将 evidence 记录为 `ReusedHookCompilation` 或
  `DerivedHookCompilation`。
- 旧 IR/SFC 管线在 G0 之前仅保留为未调用的历史参考。通过 G0 后再在独立清理
  提交中删除，不迁移其 DTO、frontend 或协议。

## 通过条件

G0 仅在以下事实同时被自动化验证时成立：

- official Razor SG 单次运行；
- final generated C#、source identity 和 mappings 可从唯一 production source
  读取；
- hook compilation 可复用或仅通过一次 `AddSyntaxTrees(...)` 派生；
- 每个 current generated tree 在 bound compilation 中恰好一份；
- `BuildRenderTree` 的 `IBlockOperation` 可稳定定位；
- 独立 package consumer 的 clean 和 incremental build 均通过。

任何一项失败都停止后续 G1-G5，而不是回退到 IR、SFC 或 second generator run。

## Gate Evidence

G0 于 2026-07-22 通过。当前锁定的 SDK 为
`11.0.100-preview.5.26302.115`，Razor compiler assembly version 为 `10.4.0.0`，
`RazorSourceGenerator.Initialize` 指纹为 IL length `1375`、SHA-256
`D26899291FF7BD908E842A14E3AFE5DBB1CE7376AB667C7733EBF25820FBBDC6`。

当前 package consumer host 的实际 binding mode 是
`DerivedHookCompilation`：hook callback compilation 不含本轮 generated tree，binder
仅对该 immutable compilation 调用一次 `AddSyntaxTrees(...)`。evidence Schema 2
记录 document content hash、`BuildRenderTree` operation inventory/hash、tree counts
和禁止路径标志；它不包含外部 consumer 的绝对路径。

自动化证据：

- `dotnet test src/Jazor.RazorVue.Sg.Test/Jazor.RazorVue.Sg.Test.csproj --filter "FullyQualifiedName~RazorSgFinalDocumentBindingTests" --no-restore`
  覆盖 reused/derived binder、current tree reconciliation 和 final-document evidence。
- `dotnet test src/Jazor.RazorVue.Sg.Test/Jazor.RazorVue.Sg.Test.csproj --filter "FullyQualifiedName~ExternalBuild_BootstrapHook_BindsOfficialGeneratedCSharpThroughImplementationSourceOutput" --no-restore`
  在独立 Razor SDK consumer 中验证 implementation source-output hook、fingerprint、trace 和 official generated C#。
- `dotnet test src/Jazor.EmitTest/Jazor.EmitTest.csproj --filter "FullyQualifiedName~Build_LocalPackages_WithExternalRazorSgG0Consumer_ReconcilesFinalDocumentsAcrossIncrementalBuilds" --no-restore`
  通过本地 NuGet 包运行 clean restore、首次 build、无改动 incremental build 与 Razor
  修改后的 build，断言每轮只有一个 generated input、mode 为 `DerivedHookCompilation`、
  legacy catalog/SFC artifact 不出现且 content hash / operation inventory 会更新。

G0 的通过只解除 Task 0.5 的前置条件；它不表示 render-function runtime、artifact
carrier、toolchain 或旧线路清理已经完成。

## 相关文档

- [Jazor 架构转型开发计划](./Jazor%20架构转型开发计划.md)
- [旧 IR 注入决策记录](./jolt/razorvue-implementation/RazorVue.RazorSg.MainlineIrInjection.DecisionRecord.md)
