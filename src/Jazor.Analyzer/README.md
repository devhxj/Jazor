# Jazor.Analyzer

> Status: active reference
> Positioning: Roslyn analyzer and Razor SG hook host for whitelist validation.

`Jazor.Analyzer` 承载 `ECMAScript` / `Jazor.Compiler` 主线的静态诊断，以及 RazorVue 使用的 Razor SG hook/bootstrap 生成器。它不再承载旧 RazorVue authoring 诊断、IR/SFC 语义模型或 Vue 分析 RPC 宿主。

## Responsibilities

- 对 `ECMAScript` 标注代码执行白名单类型/成员诊断。
- 在 erased positions（泛型实参、数组元素、局部推断、集合表达式目标等）做更早、更严格的入口诊断。
- 为 RazorVue 注册 Razor SG hook/bootstrap，并保留 final-document 输入不可用时的 fail-fast 诊断。

## Architectural Boundaries

- `Jazor.Analyzer` 可以比 `Jazor.Compiler` 更严格，但编译器仍是最终 runtime-sensitive 验证层。
- `ECMAScript.Contract` 提供最小契约，如 `Op` 和 `JazorAttribute`。
- `Jazor.Common` 提供 `Format` 与 `SourceMaps` 等真正通用实现。
- `Jazor.RazorVue` 提供 Razor SDK final-document 绑定边界；RazorVue generator 的历史命名空间 `Jazor.RazorVue.Analysis` 仍由本程序集承载。

## Current Layout

- `Analyzer.cs`: ECMAScript 主线静态分析器。
- `RazorVue/Generation/*.cs`: Razor SG hook/bootstrap 与 final-document 输入诊断。
- `AnalyzerReleases.*.md`: 分析器规则发布记录。

## Diagnostic Surface

- `JAZOR001`: 不支持的类型/成员进入 ECMAScript 编译域。
- `JAZOR002`: 共享 runtime alias 造成的类型过滤歧义。

## Build and Test

```powershell
dotnet build src/Jazor.Analyzer/Jazor.Analyzer.csproj
dotnet test src/Jazor.RazorVue.Sg.Test/Jazor.RazorVue.Sg.Test.csproj
dotnet test src/Jazor.CompilerTest/Jazor.CompilerTest.csproj
```

## Read Next

- [../Jazor.Compiler/README.md](../Jazor.Compiler/README.md)
- [../Jazor.Common/README.md](../Jazor.Common/README.md)
- [../../docs/01-目标/analyzer/README.md](../../docs/01-目标/analyzer/README.md)
