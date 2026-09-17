# ECMAScript.VeeValidate.Test

> 定位：`ECMAScript.VeeValidate` 的独立 binding 回归测试项目。

该项目维护 vee-validate public binding surface 的契约，不把第三方库 fixture 混入 `Jazor.CompilerTest`。

## 覆盖范围

- manifest schema 2、模块闭包与全部 vendored 文件哈希（`VeeValidateManifestTests`）。
- `inventory.json` fingerprint 复算与上游 barrel 导出 drift 检查。
- runtime proxy 的 import 宿主、强类型 authoring surface 与泛型返回形状（`VeeValidateProxyTests`）。
- binding 与 `Jazor.Compiler` lowering 边界的一致性：表单/字段组合式函数、选项对象字面量、状态读取引用与可写写入入口（`VeeValidateCompilerBoundaryTests`）。
- `Jazor.EmitTest` 另行验证真实 materialization 闭包（`Materialize_P3BClosures_ResolveTheirEntryGraphs`）。

具体 API 覆盖以这些测试和 `ECMAScript.VeeValidate` 源码为准。

## 运行

```bash
dotnet test src/ECMAScript.VeeValidate.Test/ECMAScript.VeeValidate.Test.csproj
dotnet run --file scripts/csharp/test-dotnet.cs -- --project vee-validate
```
