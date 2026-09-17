# ECMAScript.DateFns.Test

> 定位：`ECMAScript.DateFns` 的独立 binding 回归测试项目。

该项目维护 date-fns public binding surface 的契约，不把第三方库 fixture 混入 `Jazor.CompilerTest`。

## 覆盖范围

- manifest schema 2、模块闭包与全部 vendored 文件哈希（`DateFnsManifestTests`）。
- `inventory.json` fingerprint 复算、上游 barrel 导出 drift 检查与精选 locale 桥一致性。
- runtime proxy 的 import 宿主、强类型 authoring surface 与枚举值域（`DateFnsProxyTests`）。
- binding 与 `Jazor.Compiler` lowering 边界的一致性：命名导入、选项对象字面量、数值/字符串枚举、locale 导入与 Duration/Interval 组合（`DateFnsCompilerBoundaryTests`）。
- `Jazor.EmitTest` 另行验证真实 materialization 闭包（`Materialize_DateFnsClosure_IsSelfContainedAndResolvesBothEntries`）。

具体 API 覆盖以这些测试和 `ECMAScript.DateFns` 源码为准。

## 运行

```bash
dotnet test src/ECMAScript.DateFns.Test/ECMAScript.DateFns.Test.csproj
dotnet run --file scripts/csharp/test-dotnet.cs -- --project date-fns
```
