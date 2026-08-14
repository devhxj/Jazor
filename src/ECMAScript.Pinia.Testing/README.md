# ECMAScript.Pinia.Testing

> 定位：独立于 `ECMAScript.Pinia` 主包的 `@pinia/testing` 强类型 binding。

该项目承载测试期 Pinia root、spy 与 initial-state 的 authoring contract，避免把测试专用 API 混入生产运行时包。

## 职责

- 提供 `createTestingPinia()` 的 C# host binding。
- 提供 `TestingOptions`、`TestingInitialState`、`PiniaTestingSpyFactory` 与 `TestingStubActions` 契约。
- 提供 `ProjectPlugin(...)`、`ProjectStubActionPredicate(...)` 与 `ProjectStubActions(...)` 等显式类型投影 helper。
- `@pinia/testing` 2 已将 writable-computed plugin 设为内部固定行为，`TestingOptions` 不再公开已移除的 `WritableComputed` 选项。

## 边界

- 不镜像 Vitest、Jest 等测试框架的 spy 类型，只描述 Pinia 自身配置。
- 泛型 `TestingOptions<TDelegate, TStore>` 和投影 helper 只增强 C# authoring，不改变 JavaScript runtime options shape。
- 未覆盖的测试 runner 集成应由调用方提供，不在本包中建立兼容层。

## 代码结构

- `PiniaTesting.cs`：模块入口与委托声明。
- `Api/PiniaTesting.Api.cs`：`createTestingPinia(...)` 与投影入口。
- `Types/PiniaTesting.Types.cs`：options、state 与测试期 runtime shape。

## 相关文档

- [ECMAScript.Pinia](../ECMAScript.Pinia/README.md)
- [测试项目](../ECMAScript.Pinia.Testing.Test/README.md)
