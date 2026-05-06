# ECMAScript.Pinia.Testing

`ECMAScript.Pinia.Testing` 是独立于 `ECMAScript.Pinia` 主包的 `@pinia/testing` 绑定线，用于承载测试期 Pinia root / spy / initial-state authoring contract。

## Responsibilities

- 提供 `createTestingPinia()` 的 C# host binding。
- 提供 `TestingOptions`、`TestingInitialState`、`PiniaTestingSpyFactory` 等测试包契约。
- 保持 `@pinia/testing` 与 `pinia` 主包边界分离，避免把测试专用 surface 混进生产主包。

## Boundaries

- 不把 `@pinia/testing` API 合并进 `src/ECMAScript.Pinia/`。
- 不尝试镜像测试框架（Vitest/Jest）自身 spy 类型；只保留 Pinia 侧配置契约。
- 当前聚焦 `createTestingPinia()` 主路径与高频 options，不内建测试 runner 适配层。

## Layout

- `PiniaTesting.cs`
  - 模块入口，只保留导入标记和委托声明。
- `Api/PiniaTesting.Api.cs`
  - `createTestingPinia(...)` 入口。
- `Types/PiniaTesting.Types.cs`
  - `TestingOptions`、`TestingInitialState` 等测试专用运行时形状。
