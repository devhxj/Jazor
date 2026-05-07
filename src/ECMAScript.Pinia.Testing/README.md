# ECMAScript.Pinia.Testing

`ECMAScript.Pinia.Testing` 是独立于 `ECMAScript.Pinia` 主包的 `@pinia/testing` 绑定线，用于承载测试期 Pinia root / spy / initial-state authoring contract。

## Responsibilities

- 提供 `createTestingPinia()` 的 C# host binding。
- 提供 `TestingOptions`、`TestingOptions<TDelegate>`、`TestingOptions<TDelegate, TStore>`、`TestingInitialState`、`PiniaTestingSpyFactory`、`TestingStubActions.From(...)`、`TestingStubActions<TStore>.From(...)`、`ProjectPlugin(...)`、`ProjectStubActionPredicate(...)`、`ProjectStubActions(...)` 等测试包契约。
- 保持 `@pinia/testing` 与 `pinia` 主包边界分离，避免把测试专用 surface 混进生产主包。

## Boundaries

- 不把 `@pinia/testing` API 合并进 `src/ECMAScript.Pinia/`。
- 不尝试镜像测试框架（Vitest/Jest）自身 spy 类型；只保留 Pinia 侧配置契约。
- 当调用方需要保留一个具体 delegate 签名时，使用 `TestingOptions<TDelegate>` 显式收口 `createSpy` authoring；runtime 仍保持同一个 `createSpy` object-form 配置字段。
- 当同一个 testing root 既需要显式 `createSpy` delegate 形状，又需要显式 `stubActions` store 投影时，使用 `TestingOptions<TDelegate, TStore>`；它只增强 compile-time authoring，不改变 runtime options object shape。
- 当对象初始化器里的 `StubActions = ...` 需要稳定承接 predicate / named-list / boolean 分支时，使用 `TestingStubActions.From(...)` 或 `TestingStubActions<TStore>.From(...)`；这和 `VueRoute` 的 union factory 模式一致，避免依赖多跳隐式转换。
- 当调用方需要把 `stubActions` predicate 收口到一个显式 store 投影时，使用 `ProjectStubActionPredicate<TStore>(...)`；该 helper 只做类型级 identity 投影，不创建新的 runtime predicate 函数对象。
- 当调用方希望把 typed `stubActions` predicate 直接放进 typed options 的 union surface 时，使用 `ProjectStubActions<TStore>(...)`；该 helper 同样只做类型级 identity 投影，不创建新的 runtime wrapper。
- 当测试根需要复用主包里的 typed / projected Pinia plugin 时，使用 `ProjectPlugin(...)` 把 typed plugin callback 显式投影到 `TestingOptions.Plugins` 所需的 untyped `PiniaPlugin` 形状；该 helper 只做类型级 identity 投影，不创建新的 runtime plugin 函数对象。
- 当前聚焦 `createTestingPinia()` 主路径与高频 options，不内建测试 runner 适配层。

## Layout

- `PiniaTesting.cs`
  - 模块入口，只保留导入标记和委托声明。
- `Api/PiniaTesting.Api.cs`
  - `createTestingPinia(...)`、`ProjectPlugin(...)`、`ProjectStubActionPredicate(...)`、`ProjectStubActions(...)` 入口。
- `Types/PiniaTesting.Types.cs`
  - `TestingOptions`、`TestingOptions<TDelegate>`、`TestingOptions<TDelegate, TStore>`、`TestingStubActions`、`TestingStubActions<TStore>`、`TestingInitialState` 等测试专用运行时形状。
