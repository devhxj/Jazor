# ECMAScript.Pinia.Counter

> 定位：Pinia binding、生成模块、浏览器 bundle 和运行时验证的端到端示例。

本示例演示如何在 C# 中编写 Pinia option store，并将 Jazor 生成的模块交给 Vue、Pinia 和 Deno runtime 消费。它验证 binding 的真实使用路径，不定义 `ECMAScript.Pinia` 的公共 API。

## 结构

- `Pinia.Counter.Host`：Jazor host，生成 `stores/`、`components/`、`tests/` 和 `host/` 模块。
- `pinia-consumer`：Deno consumer，通过 import map 运行生成模块并检查 DOM 行为。
- `build-local.cs`：基于当前仓库打包本地 `Jazor` 后构建 host。
- `verify-smoke.cs`：构建 release bundle 并执行 Deno runtime/DOM smoke。

## 构建与验证

在仓库根目录执行：

```bash
dotnet run --file samples/ECMAScript.Pinia.Counter/build-local.cs
dotnet run --file samples/ECMAScript.Pinia.Counter/verify-smoke.cs -- -Configuration Release
```

常规构建产物位于 `Pinia.Counter.Host/jazor/`；smoke 会使用隔离的 `.tmp/` 输出，避免改动跟踪的示例文件。

如需单独检查前端 consumer：

```bash
cd samples/ECMAScript.Pinia.Counter/pinia-consumer
deno task build
deno task test
```

## 验证范围

- `createPinia()`、option store、`storeToRefs()`、Options API helpers 与自定义 plugin state。
- `@pinia/testing` 的 testing root、seed state 与 action stub authoring。
- hydration、HMR、multi-root isolation 与应用 unmount 后的 Pinia 清理。
- Jazor debug module、release bundle 和浏览器 DOM 的一致性。

## 相关文档

- [ECMAScript.Pinia](../../src/ECMAScript.Pinia/README.md)
- [ECMAScript.Pinia.Testing](../../src/ECMAScript.Pinia.Testing/README.md)
- [示例总览](../../docs/03-guides/examples.md)
