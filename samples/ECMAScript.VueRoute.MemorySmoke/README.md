# ECMAScript.VueRoute.MemorySmoke

> 定位：Vue Router binding、生成模块和浏览器运行时的端到端 smoke 示例。

本示例以 C# 编写 Vue Router 4 route table、guard、link 和 router-view 组合，验证 `ECMAScript.VueRoute` 在 Jazor 输出链路中的真实消费方式。

## 结构

- `VueRoute.MemorySmoke.Host`：生成 router、components、tests 和 host 模块。
- `vueroute-consumer`：Deno consumer，加载 debug module 并验证 bundle 的运行时行为。
- `build-local.cs`：打包当前仓库的本地依赖并构建 host。
- `verify-smoke.cs`：生成隔离产物、检查 lowering、构建 bundle 并执行 runtime/DOM 测试。

## 构建与验证

在仓库根目录执行：

```bash
dotnet run --file samples/ECMAScript.VueRoute.MemorySmoke/build-local.cs
dotnet run --file samples/ECMAScript.VueRoute.MemorySmoke/verify-smoke.cs -- -Configuration Release
```

smoke 默认输出到仓库 `.tmp/sample-smoke/`，不依赖固定的项目根 `jazor/` 路径。

如需单独检查 consumer：

```bash
cd samples/ECMAScript.VueRoute.MemorySmoke/vueroute-consumer
deno task build
deno task test
```

## 相关文档

- [ECMAScript.VueRoute](../../src/ECMAScript.VueRoute/README.md)
- [VueRoute 测试](../../src/ECMAScript.VueRoute.Test/README.md)
- [示例总览](../../docs/03-guides/examples.md)
