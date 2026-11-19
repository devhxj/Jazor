# ECMAScript.Contract

> 定位：编译器链路中零外部依赖的最小共享契约程序集。

`ECMAScript.Contract` 只定义多个消费者必须共享且不应携带实现依赖的声明。它不承载 Roslyn、JSON或 Emit 实现，以保持 `ECMAScript.*` 命名空间的最低依赖面稳定。

## 职责

- 定义白名单声明原语 `JazorAttribute` 与操作词汇 `Op`。
- 提供最小 UI 标记契约 `IUIComponent`。
- 提供统一的 ECMAScript 宿主/外部 ESM/组件绑定元数据 `ECMAScriptAttribute`。
- 提供 record-like host contract 的推导原语 `PropsAttribute` 与 `EmitsAttribute`。
- 保持零外部依赖，供编译器、生成器和宿主绑定共同引用。

## 边界

- `JazorAttribute` 与 `Op` 是仓库内部契约，通过 `InternalsVisibleTo` 共享，不扩展为宽泛公共 API。
- `Op.Compile` 是编译器持有语义的保留入口；consumer 不应以它替代 `Alias`、`Inline` 或 `Import` 建模。
- `[ECMAScript]` 声明环境宿主，`[ECMAScript("specifier")]` 声明外部 ESM；最多一个参数，没有 `Transform` 或组件专用参数。导出名由名称机制提供，组件身份由框架 marker 确认。成员级 specifier 优先于类型级声明，包身份与资源闭包来自对应 binding metadata。
- SourceMap、宿主协议 DTO 与其他共享实现位于 `Jazor.Common`。

## 关键文件

- `attribute/JazorAttribute.cs`：白名单 producer 声明特性。
- `Op.cs`：共享操作枚举。
- `IUIComponent.cs`：最小 UI 组件标记契约。
- `attribute/ECMAScriptAttribute.cs`：环境宿主与外部 ESM 元数据。
- `attribute/PropsAttribute.cs`、`attribute/EmitsAttribute.cs`：authoring 推导元数据。

普通源码类型不依靠该特性取得编译许可；class、record 与宿主 union 的规则见[类型指南](../../docs/03-guides/type-semantics.md)。

## 相关文档

- [Jazor.Common](../Jazor.Common/README.md)
- [Jazor.Compiler.Generator](../Jazor.Compiler.Generator/README.md)
- [平台与绑定](../../docs/02-architecture/platform-and-bindings.md)
