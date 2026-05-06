# Jazor.Analyzer

> Status: active reference
> Positioning: Roslyn analyzer and incremental-generator host for whitelist validation and RazorVue compile-time analysis.

`Jazor.Analyzer` 不是单纯的“旧白名单分析器”。它现在同时承载两类能力：一类是 `ECMAScript` / `Jazor.Compiler` 主线的静态诊断，另一类是 RazorVue 的编译时分析、增量生成器和兼容分析宿主。

## Responsibilities

- 对 `ECMAScript` 标注代码执行白名单类型/成员诊断。
- 在 erased positions（泛型实参、数组元素、局部推断、集合表达式目标等）做更早、更严格的入口诊断。
- 为 RazorVue 承载 authoring 诊断与增量生成器。
- 提供 RazorVue 兼容分析 RPC 的进程内/stdio 宿主实现。

## Architectural Boundaries

- `Jazor.Analyzer` 可以比 `Jazor.Compiler` 更严格，但编译器仍是最终 runtime-sensitive 验证层。
- `ECMAScript.Contract` 提供最小契约，如 `Op` 和 `JazorAttribute`。
- `Jazor.Common` 提供 `Format` 与 `SourceMaps` 等真正通用实现。
- `Jazor.RazorVue` 提供 RazorVue 共享语义、Razor SDK 桥接、宿主协议 DTO 和 emit/shared artifact 模型。
- RazorVue 对外命名空间仍保留为 `Jazor.RazorVue.Analysis`，但物理程序集已经并入 `Jazor.Analyzer`。

## Current Layout

- `Analyzer.cs`: ECMAScript 主线静态分析器。
- `RazorVue/Diagnostics/*.cs`: RazorVue authoring 诊断、descriptor 和 symbol helper。
- `RazorVue/Generation/*.cs`: RazorVue 增量生成器。
- `VueHost/*.cs`: `Jazor.Vue` 兼容分析 RPC、进程内分析运行时与序列化。
- `AnalyzerReleases.*.md`: 分析器规则发布记录。

## Diagnostic Surface

- `JAZOR001`: 不支持的类型/成员进入 ECMAScript 编译域。
- `JAZOR002`: 共享 runtime alias 造成的类型过滤歧义。
- `JAZORVUE*`: RazorVue authoring 规则与旧指令迁移诊断。
- `JAZORVGA*`: RazorVue catalog 生成与 library component 约束诊断。

## Build and Test

```powershell
dotnet build src/Jazor.Analyzer/Jazor.Analyzer.csproj
dotnet test src/Jazor.RazorVue.Test/Jazor.RazorVue.Test.csproj
dotnet test src/Jazor.CompilerTest/Jazor.CompilerTest.csproj
```

## Read Next

- [../Jazor.Compiler/README.md](../Jazor.Compiler/README.md)
- [../Jazor.Common/README.md](../Jazor.Common/README.md)
- [../../docs/01-目标/analyzer/README.md](../../docs/01-目标/analyzer/README.md)
