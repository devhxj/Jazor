# Jazor.Common

> 定位：编译器、分析器、Emit 与 Razor-to-Vue 共用的依赖型实现层。

`Jazor.Common` 承接需要跨项目共享、但不应污染 `ECMAScript.*` 最小契约面的实现代码。格式化、稳定命名与 SourceMap 支持统一在此维护；Razor-to-Vue 的组件绑定和生成职责仍归其专属项目。

## 职责

- 提供统一符号格式化与稳定 hash 命名能力 `Format`。
- 承载共享 SourceMap 模型与写出辅助。

## 命名空间

- `Jazor.Common`
- `Jazor.Common.SourceMaps`

## 边界

- 本项目可以依赖 `System.Text.Json` 等共享实现需要的包。
- 最小契约仍归 `ECMAScript.Contract`，例如 `JazorAttribute`、`Op` 与 `IUIComponent`。
- 本项目不拥有 ECMAScript AST、编译器入口、Razor SDK 桥接或组件专属 manifest 模型。

## 关键区域

- `Format.cs`：统一 `SymbolDisplayFormat` 与稳定 hash 命名。
- `SourceMaps/`：SourceMap 模型与 writer。

## 相关文档

- [ECMAScript.Contract](../ECMAScript.Contract/README.md)
- [Jazor.Emit](../Jazor.Emit/README.md)
- [编译器架构](../../docs/02-architecture/compiler.md)
