# ECMAScript.Contract

> Status: active reference
> Positioning: dependency-free contract assembly for the smallest shared declaration surface.

`ECMAScript.Contract` 只保留编译器链路里最小、最稳定、最不应该携带依赖的契约。它的目标不是承载功能实现，而是把必须被多个消费者共享的最低层声明固定下来，同时避免把 Roslyn、JSON、emit、RazorVue 等依赖带进 `ECMAScript.*` 命名空间。

## Responsibilities

- 定义 producer 侧白名单声明原语 `JazorAttribute`。
- 定义白名单操作词汇 `Op`。
- 提供最小 UI/Razor 标记契约 `IUIComponent`。
- 提供 record-like host contract 的缺省成员推导原语 `PropsAttribute`。
- 维持零外部依赖。

## Boundaries

- `ECMAScript.Contract` 不承载 SourceMap、emit 共享模型、Vue/Jolt 协议 DTO、RazorVue 语义实现。
- 这些共享实现统一放在 `Jazor.Common`。
- `JazorAttribute` 和 `Op` 当前都是 `internal`，通过 `InternalsVisibleTo` 在仓库内共享，不作为广义公共 API 扩散。
- `PropsAttribute` 也是内部 contract primitive，供 `ECMAScript` 声明、`Jazor.Compiler` 消费；它不是面向业务侧的运行时 API。

## Key Files

- `JazorAttribute.cs`: 白名单 producer 侧声明特性。
- `Op.cs`: 共享的声明端操作枚举。
- `IUIComponent.cs`: 最小 UI 组件标记契约。
- `PropsAttribute.cs`: 从第一个泛型类型实参公共属性名补写 record literal 成员的声明原语。
- `GlobalUsings.cs`, `IsExternalInit.cs`: 低层共享辅助。

## Read Next

- [../Jazor.Common/README.md](../Jazor.Common/README.md)
- [../Jazor.Compiler.Generator/README.md](../Jazor.Compiler.Generator/README.md)
- [../Jazor.CLR/README.md](../Jazor.CLR/README.md)
