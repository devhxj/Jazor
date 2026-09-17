# ECMAScript.VueQuery.Test

> 定位：`ECMAScript.VueQuery` 的独立 binding 回归测试项目。

该项目维护 vue-query public binding surface 的契约，不把第三方库 fixture 混入 `Jazor.CompilerTest`。

## 覆盖范围

- manifest schema 2、`@tanstack/*` 多包闭包（含 `vue-demi`）与全部 vendored 文件哈希（`VueQueryManifestTests`）。
- entry 的 package 依赖边（`@tanstack/query-core`、`@tanstack/match-sorter-utils`、`vue-demi`、`@vue/devtools-api`）、inventory fingerprint 复算与上游导出 drift 检查。
- 返回形状：query 的 loading/error/status/refetch 与 mutation 的 mutate/mutateAsync/reset（`VueQueryManifestTests`）。
- binding 与 `Jazor.Compiler` lowering 边界的一致性：query/mutation 组合式函数、query-key 联合数组、选项对象字面量与状态引用（`VueQueryCompilerBoundaryTests`）。
- `Jazor.EmitTest` 另行验证真实 materialization 闭包（`Materialize_P3BClosures_ResolveTheirEntryGraphs`）。

具体 API 覆盖以这些测试和 `ECMAScript.VueQuery` 源码为准。

## 运行

```bash
dotnet test src/ECMAScript.VueQuery.Test/ECMAScript.VueQuery.Test.csproj
dotnet run --file scripts/csharp/test-dotnet.cs -- --project vue-query
```
