# Jazor.Common

> Status: active reference
> Positioning: dependency-bearing shared implementation layer for compiler, analyzer, emit, and RazorVue.

`Jazor.Common` 承接需要跨多个项目共享、但不应污染 `ECMAScript.*` 依赖面的实现代码。原先分散在不同项目中的格式化、稳定命名和 SourceMap 支持已统一收敛到这里；RazorVue 的组件绑定与生成职责仍由相应项目负责。

## Responsibilities

- 提供统一的符号格式化与稳定哈希命名能力 `Format`。
- 承载共享的 SourceMap 模型与写出辅助。

## Public Namespace Layout

- `Jazor.Common`
- `Jazor.Common.SourceMaps`

## Boundaries

- `Jazor.Common` 允许依赖 `System.Text.Json` 等共享实现需要的包。
- 最小契约层仍然属于 `ECMAScript.Contract`，例如 `JazorAttribute`、`Op`、`IUIComponent`。
- `Jazor.Common` 不承载 ECMAScript AST 核心定义，也不直接拥有编译器入口点。
- `Jazor.Common` 不承载 RazorVue core、Razor SDK 桥接、宿主协议 DTO 或组件专属 manifest 模型。

## Key Areas

- `Format.cs`: 统一的 `SymbolDisplayFormat` 与稳定 hash 命名。
- `SourceMaps/`: SourceMap 模型与 writer。

## Read Next

- [../ECMAScript.Contract/README.md](../ECMAScript.Contract/README.md)
- [../ECMAScript.Vuetify/README.md](../ECMAScript.Vuetify/README.md)
- [../Jazor.Emit/README.md](../Jazor.Emit/README.md)

