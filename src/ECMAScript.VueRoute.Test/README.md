# ECMAScript.VueRoute.Test

> 定位：`ECMAScript.VueRoute` 的独立 binding 回归测试项目。

该项目维护 Vue Router public binding surface 的契约，不把框架特定 fixture 混入 `Jazor.CompilerTest`。

## 覆盖范围

- 项目布局、打包和仓库测试入口。
- runtime proxy 的公开成员和强类型 authoring surface。
- union、maybe-ref、注入 key、命名视图、route props 与 navigation guard 的 C# 表达能力。
- binding 与 `Jazor.Compiler` lowering 边界的一致性。

当前测试入口为 `EcmaScriptVueRouteLayoutGuardTests`、`EcmaScriptVueRouteProxyTests` 和 `EcmaScriptVueRouteCompilerBoundaryTests`。具体 API 覆盖以这些测试和 `ECMAScript.VueRoute` 源码为准。

## 运行

```bash
dotnet test src/ECMAScript.VueRoute.Test/ECMAScript.VueRoute.Test.csproj
dotnet test src/ECMAScript.VueRoute.Test/ECMAScript.VueRoute.Test.csproj --settings src/ECMAScript.VueRoute.Test/coverlet.runsettings
dotnet run --file scripts/csharp/test-dotnet.cs -- --project vueroute
```

## 相关文档

- [ECMAScript.VueRoute](../ECMAScript.VueRoute/README.md)
- [平台与绑定](../../docs/02-architecture/platform-and-bindings.md)
- [开发与测试](../../docs/03-guides/development-and-testing.md)
