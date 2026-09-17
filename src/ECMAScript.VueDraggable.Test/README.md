# ECMAScript.VueDraggable.Test

> 定位：`ECMAScript.VueDraggable` 的独立 binding 回归测试项目。

该项目维护 vue-draggable-plus public binding surface 的契约，不把第三方库 fixture 混入 `Jazor.CompilerTest`。

## 覆盖范围

- manifest schema 2、单包闭包与全部 vendored 文件哈希；upstream 导出 drift；组件代理与泛型列表参数；SortableJS 选项字段名；编译器 emission（组合式函数调用与选项字面量）。
- `Jazor.EmitTest` 另行验证真实 materialization 闭包（`Materialize_P3CComponentClosures_ResolveTheirEntryGraphs`）。

具体 API 覆盖以这些测试和 `ECMAScript.VueDraggable` 源码为准。

## 运行

```bash
dotnet test src/ECMAScript.VueDraggable.Test/ECMAScript.VueDraggable.Test.csproj
dotnet run --file scripts/csharp/test-dotnet.cs -- --project vue-draggable
```
