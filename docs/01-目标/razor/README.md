# Razor 输入边界

> 当前生产入口：官方 Razor Source Generator 完成后的 Roslyn `Compilation`

## 定位

Jazor 不自行实现 Razor 语法解析器，也不把 Razor 中间表示作为 Razor-to-Vue 的生产输入。项目只消费官方 Razor Source Generator 已完成的生成 C#，从而将 Razor SDK 版本相关的内部文档模型隔离在官方工具链内部。

## 包与职责

| 组件 | 职责 |
| --- | --- |
| `Jazor.Vue` | 显式安装 generator-driver Hook |
| `Jazor.RazorVue.Generator` | 在最终 `Compilation` 上发现组件并生成 catalog |
| `Jazor.RazorVue` | 绑定生成 C#、选择组件并构造 Vue 产物 |
| `Jazor.Compiler` | 基于 Roslyn `IOperation` 完成 C# 语义降低 |
| `Jazor.Emit` | 物化模块、源映射、manifest 和 bundle |

`Jazor` 单独引用不会安装 Razor Hook。`Jazor.Vue` 是 Razor-to-Vue 的唯一公开 opt-in 入口。

## 当前链路

```text
.razor / .razor.cs
        -> 官方 Razor Source Generator
        -> generated .razor.g.cs
        -> GeneratorDriver 完成后的 Compilation
        -> BuildRenderTree Operation 绑定
        -> Jazor.Compiler / SemanticWalker
        -> Vue render-function .mjs
        -> Jazor.Emit
```

## 非目标边界

以下内容不属于当前 Razor-to-Vue 生产路径：

- `EnableRazorHostOutputs`；
- `RazorCodeDocument`、`RazorCSharpDocument` 和 Razor DR/IR；
- 对生成 C# 的二次解析；
- Razor-to-SFC 输出和模板协议回退；
- 编辑器协议、开发服务器或热更新协议。

## 设计原则

1. 官方 Razor SG 负责 Razor 语法、参数绑定和组件契约诊断。
2. RazorVue 只绑定最终生成 C# 中可验证的组件语义，不重复实现 Razor 语义检查。
3. C# 表达式、成员访问、函数调用、导入收集和临时变量必须通过 `Jazor.Compiler` 的翻译入口完成。
4. RazorVue 只负责组件边界、Vue render-function framing 和运行时资源封装。
