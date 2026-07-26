# Razor 基础层（历史边界）

> Status: historical boundary note
> Positioning: 独立 Razor 基础项目已不在当前项目图中；当前转型分支通过 official Razor Source Generator 结果进入生产链路。

## 为什么需要

Jazor 仍需要接入 Razor 语法，但当前生产输入已经收敛为 official Razor SG generated C#。独立 `Jazor.Razor` / `Jazor.Compiler.Razor` 项目不参与当前转型分支；Razor 相关生产逻辑由 `Jazor.Analyzer` 的受控 SG hook 和 `Jazor.RazorVue` 的 final-document adapter 承接。

## 解决什么问题

1. **固定生产输入**：Razor 语义只从 official SG generated C# 进入，不读取 Razor DR/IR。
2. **避免边界塌陷**：Razor SDK hook、generated C# binding、compiler lowering 分别归属 `Jazor.Analyzer`、`Jazor.RazorVue`、`Jazor.Compiler`。
3. **保留历史可追溯性**：旧 Razor 基础层设计只作为历史边界说明，不再指导当前实现。

## 当前分工

| 路径 | 职责 |
|------|------|
| `src/Jazor.Analyzer/` | 注册受控 Razor SG tail hook、兼容性校验和诊断 |
| `src/Jazor.RazorVue/RazorSdk/` | 读取 official SG final document、绑定 generated C#、选择组件候选 |
| `src/Jazor.Compiler/` | 基于 Roslyn `IOperation` 做 C# 语义 lowering；后续承接 RenderTreeBuilder Compile hooks |

## 当前链路

```text
.razor / .razor.cs
     ↓ official Razor Source Generator
generated .razor.g.cs
     ↓ Jazor.RazorVue generated C# binder
BuildRenderTree IOperation
     ↓ Jazor.Compiler / SemanticWalker
Vue render-function .mjs
```

这里没有 Razor DR/IR fallback、Razor-to-SFC 输出或 Jolt 投影路径。旧线路需要通过 Git 基线 `d68aecbb00b23aa35735c9a269b2e987c7815b05` 查看。
