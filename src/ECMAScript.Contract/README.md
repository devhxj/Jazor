# ECMAScript.Contract

> Status: active reference
> Positioning: dependency-free contract assembly for the smallest shared declaration surface.

`ECMAScript.Contract` 只保留编译器链路里最小、最稳定、最不应该携带依赖的契约。它的目标不是承载功能实现，而是把必须被多个消费者共享的最低层声明固定下来，同时避免把 Roslyn、JSON、emit、RazorVue 等依赖带进 `ECMAScript.*` 命名空间。

## Responsibilities

- 定义 producer 侧白名单声明原语 `JazorAttribute`。
- 定义白名单操作词汇 `Op`。
- 提供最小 UI/Razor 标记契约 `IUIComponent`。
- 提供 record-like host contract 的缺省成员推导原语 `PropsAttribute`。
- 提供 setup-based emit contract 的缺省成员推导原语 `EmitsAttribute`。
- 维持零外部依赖。

## Boundaries

- `ECMAScript.Contract` 不承载 SourceMap、emit 共享模型、宿主协议 DTO 或 RazorVue 语义实现。
- 这些共享实现统一放在 `Jazor.Common`。
- `JazorAttribute` 和 `Op` 当前都是 `internal`，通过 `InternalsVisibleTo` 在仓库内共享，不作为广义公共 API 扩散。
- `Op.Compile` 仍是编译器拥有语义的保留入口；consumer 侧现在可拿到 symbol / context / origin operation，以完成 import 绑定和 usage-site 诊断，但不应替代正常的 `Alias` / `Inline` / `Import` 建模。
- `PropsAttribute` / `EmitsAttribute` 直接就是声明侧原语，不再通过额外的统一 contract 元模型再包一层。
- `PropsAttribute` 通过 `TypeArgumentIndex` 声明 props 名称推导来源的泛型类型参数位置，默认是第 `0` 个类型参数。
- `EmitsAttribute` 通过 `SourceMemberName` 声明 emits 推导来源的 setup 成员名，默认是 `Setup`。

## Key Files

- `JazorAttribute.cs`: 白名单 producer 侧声明特性。
- `Op.cs`: 共享的声明端操作枚举。
- `IUIComponent.cs`: 最小 UI 组件标记契约。
- `PropsAttribute.cs`: 仅允许标在属性上；默认声明“从第 0 个泛型类型实参公共属性名推导”。
- `EmitsAttribute.cs`: 仅允许标在属性上；默认声明“从 `Setup` 成员中的稳定 emit 调用推导”。
- `GlobalUsings.cs`, `IsExternalInit.cs`: 低层共享辅助。

## Read Next

- [../Jazor.Common/README.md](../Jazor.Common/README.md)
- [../Jazor.Compiler.Generator/README.md](../Jazor.Compiler.Generator/README.md)
- [../Jazor.CLR/README.md](../Jazor.CLR/README.md)
