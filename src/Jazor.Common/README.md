# Jazor.Common

> Status: active reference
> Positioning: dependency-bearing shared implementation layer for compiler, analyzer, emit, Jolt, and RazorVue.

`Jazor.Common` 承接那些需要跨多个项目共享、但又不应该污染 `ECMAScript.*` 依赖面的实现代码。迁移之后，原先分散在 `Jazor.Name`、`ECMAScript.Internal`、独立 RazorVue 项目中的共享实现，已经统一收敛到这里。

## Responsibilities

- 提供统一的符号格式化与稳定哈希命名能力 `Format`。
- 承载共享的 RazorVue 语义、发现、描述符、lowering 与 artifact 模型。
- 承载共享的 SourceMap 模型与写出辅助。
- 承载 emit 侧共享 manifest 模型与序列化辅助。
- 承载 Vue/Jolt 的文档、RPC、协议 DTO。

## Public Namespace Layout

- `Jazor.Common`
- `Jazor.Common.Emit`
- `Jazor.Common.SourceMaps`
- `Jazor.Common.VueContracts.Documents`
- `Jazor.Common.VueContracts.Protocol`
- `Jazor.RazorVue*`（RazorVue 对外命名空间仍保留在这里，物理项目不再单独存在）

## Boundaries

- `Jazor.Common` 允许依赖 Roslyn、`System.Text.Json` 等共享实现需要的包。
- 最小契约层仍然属于 `ECMAScript.Contract`，例如 `JazorAttribute`、`Op`、`IUIComponent`。
- `Jazor.Common` 不承载 ECMAScript AST 核心定义，也不直接拥有编译器入口点。

## Key Areas

- `Format.cs`: 统一的 `SymbolDisplayFormat` 与稳定 hash 命名。
- `RazorVue/`: RazorVue 共享语义与 authoring/lowering 支持。
- `SourceMaps/`: SourceMap 模型与 writer。
- `Emit/`: RazorVue manifest 与 emit 共享模型。
- `VueContracts/`: Jolt 与分析链路共享协议 DTO。

## Read Next

- [../ECMAScript.Contract/README.md](../ECMAScript.Contract/README.md)
- [../ECMAScript.Vuetify/README.md](../ECMAScript.Vuetify/README.md)
- [../Jolt/README.md](../Jolt/README.md)
