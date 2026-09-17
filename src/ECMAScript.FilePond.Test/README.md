# ECMAScript.FilePond.Test

> 定位：`ECMAScript.FilePond` 的独立 binding 回归测试项目。

该项目维护 FilePond public binding surface 的契约，不把第三方库 fixture 混入 `Jazor.CompilerTest`。

## 覆盖范围

- manifest schema 2、核心+适配器双入口闭包、全部 vendored 文件哈希与 styles 声明；upstream 导出 drift；核心与实例 API 形状；默认导出组件代理；浏览器安全（无 process.env）；编译器 emission。
- `Jazor.EmitTest` 另行验证真实 materialization 闭包（`Materialize_P3CComponentClosures_ResolveTheirEntryGraphs`）。

具体 API 覆盖以这些测试和 `ECMAScript.FilePond` 源码为准。

## 运行

```bash
dotnet test src/ECMAScript.FilePond.Test/ECMAScript.FilePond.Test.csproj
dotnet run --file scripts/csharp/test-dotnet.cs -- --project file-pond
```
