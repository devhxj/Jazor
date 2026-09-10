# Jazor.EmitTest

> 定位：`Jazor.Emit` 模块物化、SourceMap 和生产 bundle 的回归测试项目。

## 覆盖范围

- 基于已验证资源闭包的 bundle 生成与根程序集 export 保留。
- CLR runtime manifest、程序集 `ModuleCatalog` 与引用资源包收集。
- 临时 bundle workspace 中的跨模块 import 改写。
- 静态模块、chained bundle 与 writer source map 行为。
- `ECMAScript.Style` 的 debug 物化、source map、manifest 与 release bundle。
- Emit 只消费 `ModuleCatalog` 和 `manifest.json + dist/**` 两种输入的契约守护。

测试直接检查生成的 JavaScript 输出，不只检查进程退出码。

## 运行

```bash
dotnet test src/Jazor.EmitTest/Jazor.EmitTest.csproj
```

仓库统一入口：

```bash
dotnet run --file scripts/csharp/test-dotnet.cs -- --project emit
```

本地 NuGet、外部 Razor SG、Deno 和真实浏览器消费者矩阵属于发布验收 lane：

```bash
dotnet run --file scripts/csharp/test-dotnet.cs -- --project emit-consumer
```

## 说明

测试使用独立临时工作区并在结束后清理。SSR 和 runtime smoke 使用打包的 `DenoHost` runtime；还原期间出现 `NU1900` 漏洞源警告不阻断已成功完成的测试。

`SdkIntegrationTests` 标记为 `Consumer`。普通 `emit` lane 会跳过这组昂贵的包消费者测试，保持模块、SourceMap、宿主、SSR 和 artifact 契约快速反馈；`emit-consumer` 执行完整消费者矩阵。发布工作流仍必须运行 SPA/SSR 消费者门禁。

## 相关文档

- [Jazor.Emit](../Jazor.Emit/README.md)
- [开发与测试](../../docs/03-guides/development-and-testing.md)
## Test execution

Emit tests use class-level parallelism: independent test classes may run concurrently,
while methods in one fixture run in source order. Several fixtures build local packages
or start Deno/browser processes and share repository build outputs; method-level
parallelism can race those outputs and make the testhost appear hung behind a locked
assembly. Use a focused `--filter` for browser or package-consumer lanes when diagnosing
an individual failure.
