# Jazor.Razor

> Status: active reference
> Positioning: the thinnest Razor-side marker layer in the Jazor stack.

`Jazor.Razor` 刻意保持很薄。它只定义最小的 Razor authoring 标记，不把编译器桥接、RazorVue 语义、Source Generator 规则或宿主逻辑耦合进来。

## Responsibilities

- 定义最小 Razor authoring 标记契约 `IJazorComponent`。
- 作为 Razor 基础层与更高层产品线之间的分界点。

## Boundaries

- `Jazor.Razor` 不承载 Razor 语义提取、RazorVue 描述符、lowering 或生成器。
- `Jazor.Compiler.Razor` 承载编译器侧的 `JazorComponent` 基类与 `RazorComponentSemanticFrontend`。
- `Jazor.Common/RazorVue` 承载 RazorVue 共享语义。
- `Jazor.Analyzer/RazorVue` 承载 RazorVue 的 Roslyn 分析与生成器宿主。

## Key File

- `JazorComponent.cs`: 当前文件里定义的是 `IJazorComponent` 最小标记接口。

## Read Next

- [../Jazor.Compiler.Razor/RazorComponentSemanticFrontend.cs](../Jazor.Compiler.Razor/RazorComponentSemanticFrontend.cs)
- [../../docs/01-目标/razor/README.md](../../docs/01-目标/razor/README.md)
- [../../docs/01-目标/razorvue/README.md](../../docs/01-目标/razorvue/README.md)
