# RazorVue Gate 与验收

> Parent: [Jazor 架构转型开发计划](../Jazor%20架构转型开发计划.md)
> Size rule: keep under 10KB.

## Gate 顺序

| Gate | 当前意义 | 阻断条件 |
|---|---|---|
| G0 | official Razor SG generated C# + hook compilation 可绑定为 `IOperation` | 需要 DR/IR、回读 Razor、nested SG、从零建 compilation |
| G1 | Counter `.razor -> .mjs -> browser` 真实链路 | 任一层只靠手工 fixture |
| G2-F | 功能 Gate：render surface、props/events/slots/bind、lifecycle 可用 | accepted generated-code shape 缺测试或输出半成品 |
| G2-P | 性能 Gate：功能闭环后采样与阈值判定 | 无 benchmark report、无 browser heap、无阈值 |
| G3 | Deno production + DynamicVueComponent mixed SFC | `.vue` graph/CSS/assets/map 不完整 |
| G4 | Deno dev/HMR + Netpack production lane | 工具链不消费统一 manifest |
| G5 | package consumer、sample、platform matrix、release docs | 依赖 repo-local bin 或缺 release evidence |

`G2-F` 必须先于 `G2-P`。当前阶段主攻 `G2-F`。

## 当前必跑 focused tests

按变更范围选择：

```text
dotnet test src/Jazor.CompilerTest/Jazor.CompilerTest.csproj --filter "SemanticWalkerRenderTreeBuilderHostTest|CurrentComponentSemanticWalkerHostTest"
dotnet test src/Jazor.RazorVue.Sg.Test/Jazor.RazorVue.Sg.Test.csproj --filter "RazorSgComponentMemberClosureTests"
dotnet run --file scripts/csharp/test-render-context.cs
dotnet test src/Jazor.EmitTest/Jazor.EmitTest.csproj --filter "VueRenderCatalog|ModuleWriter"
```

功能落地阶段不要求默认运行 benchmark。

## 完成定义

当前计划完成必须同时满足：

- official Razor SG 原样运行；
- production call graph 不含 Razor DR/IR、Razor 原文重建、nested Razor SG；
- `.razor` 组件只生成传统 Vue render-function `.mjs`、map、manifest；
- C# expression/member/function semantics 通过 `Jazor.Compiler` / `SemanticWalker`；
- RenderTreeBuilder 只走一套 render-context/VNode lowering；
- Deno production build 和 browser flow 通过；
- Netpack production build smoke 通过；
- package consumer 不依赖仓库本地工具输出；
- unsupported capability 有明确 diagnostic 或文档；
- 性能报告在功能闭环后补齐。

## 文档验收

- 活跃计划单文件不超过 10KB。
- 入口文件只保留结论、路线和索引。
- 长表格、状态快照、验证命令放分片。
- 禁止描述第二执行路线。
