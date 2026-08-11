# ECMAScript.Contract

> 定位：编译器链路中零外部依赖的最小共享契约程序集。

`ECMAScript.Contract` 只定义多个消费者必须共享且不应携带实现依赖的声明。它不承载 Roslyn、JSON、Emit 或 Razor-to-Vue 实现，以保持 `ECMAScript.*` 命名空间的最低依赖面稳定。

## 职责

- 定义白名单声明原语 `JazorAttribute` 与操作词汇 `Op`。
- 提供最小 UI/Razor 标记契约 `IUIComponent`。
- 提供 record-like host contract 的推导原语 `PropsAttribute` 与 `EmitsAttribute`。
- 保持零外部依赖，供编译器、生成器和宿主绑定共同引用。

## 边界

- `JazorAttribute` 与 `Op` 是仓库内部契约，通过 `InternalsVisibleTo` 共享，不扩展为宽泛公共 API。
- `Op.Compile` 是编译器持有语义的保留入口；consumer 不应以它替代 `Alias`、`Inline` 或 `Import` 建模。
- SourceMap、宿主协议 DTO 与其他共享实现位于 `Jazor.Common`，Razor-to-Vue 行为位于其专属项目。

## 关键文件

- `JazorAttribute.cs`：白名单 producer 声明特性。
- `Op.cs`：共享操作枚举。
- `IUIComponent.cs`：最小 UI 组件标记契约。
- `PropsAttribute.cs`、`EmitsAttribute.cs`：authoring 推导元数据。

## 相关文档

- [Jazor.Common](../Jazor.Common/README.md)
- [Jazor.Compiler.Generator](../Jazor.Compiler.Generator/README.md)
- [平台与绑定](../../docs/02-architecture/platform-and-bindings.md)
