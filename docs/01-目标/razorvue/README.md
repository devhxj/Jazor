# RazorVue 目标

> Status: active reference
> Positioning: 当前转型分支的 Razor-to-Vue 目标入口。

RazorVue 当前只有一条生产主线：

```text
official Razor SG generated C#
    -> Roslyn IOperation
    -> Jazor.Compiler / SemanticWalker
    -> Vue render-function .mjs
```

## 当前边界

- production 输入是 official Razor Source Generator 的最终 generated C# 文档和 hook callback compilation 派生链。
- `Jazor.RazorVue` 只保留 final-document adapter、generated C# binder、component candidate selector，以及 Vue runtime framing。
- C# 表达式、成员访问、函数调用、临时变量、import 收集和 RenderTreeBuilder 语义 lowering 必须走 `Jazor.Compiler` / `SemanticWalker`。
- Razor 组件输出合同是 `.mjs`、`.mjs.map` 和版本化 manifest；不再生成 Vue SFC artifact。
- 手写 `.vue` 只通过后续 `DynamicVueComponent<TProps>` 做单向互操作，不是 Razor lowering fallback。

## 已退役边界

- Razor DR/IR reader/frontend/DTO。
- Razor-to-template/SFC lowering 与 SFC catalog/artifact factory。
- Jolt、`.jazor`、Jolt LSP/DAP/debug/dev-server 协议。
- wrapper marker 或模板/slot transport fallback。
- 旧 Playground 演示宿主。

旧设计碎片不再保留在活跃目标目录中；需要历史对照时使用 Git 基线 `d68aecbb00b23aa35735c9a269b2e987c7815b05` 或转型计划中列出的历史材料入口。

## 当前实现落点

| 路径 | 职责 |
|------|------|
| `src/Jazor.RazorVue/RazorSdk/` | official Razor SG final-document adapter、generated C# binder、component selector |
| `src/Jazor.RazorVue/Runtime/` | `@jazor/vue-runtime` render-context v1 runtime assets |
| `src/Jazor.Analyzer/` | Razor SG hook/bootstrap、compatibility guard 和 analyzer diagnostics |
| `src/Jazor.Compiler/` | Roslyn `IOperation` lowering 与后续 RenderTreeBuilder Compile hooks |
| `src/Jazor.Emit/` | `.mjs`、source map、manifest、bundle 和 runtime asset 物化 |

## 验证入口

```powershell
dotnet run --file scripts/csharp/test-dotnet.cs -- --project razor-sg
dotnet run --file scripts/csharp/test-dotnet.cs -- --project emit
dotnet run --file scripts/csharp/test-dotnet.cs -- --project render-context
```

## Read Next

- [Jazor 架构转型开发计划](../../02-计划/Jazor%20架构转型开发计划.md)
- [Razor SG Final-Document G0 决策记录](../../02-计划/RazorSgFinalDocument.G0.DecisionRecord.md)
- [Jazor.RazorVue project README](../../../src/Jazor.RazorVue/README.md)
