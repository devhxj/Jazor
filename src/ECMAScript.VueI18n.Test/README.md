# ECMAScript.VueI18n.Test

> 定位：`ECMAScript.VueI18n` 的独立 binding 回归测试项目。

该项目维护 vue-i18n public binding surface 的契约，不把第三方库 fixture 混入 `Jazor.CompilerTest`。

## 覆盖范围

- manifest schema 2、`@intlify/*` 多包闭包与全部 vendored 文件哈希（`VueI18nManifestTests`）。
- entry 的 package 依赖边（`@intlify/core-base`、`@intlify/shared`、`vue`、`@vue/devtools-api`）、inventory fingerprint 复算与上游导出 drift 检查。
- Composer 表面：locale/fallbackLocale 可写引用、messages、`t`/`te`/`tm`/`d` 入口（`VueI18nManifestTests`、`VueI18nCompilerBoundaryTests`）。
- binding 与 `Jazor.Compiler` lowering 边界的一致性：`createI18n`/`useI18n` 入口、字符串枚举作用域、选项对象字面量与 locale 切换（`VueI18nCompilerBoundaryTests`）。
- `Jazor.EmitTest` 另行验证真实 materialization 闭包（`Materialize_P3BClosures_ResolveTheirEntryGraphs`）。

具体 API 覆盖以这些测试和 `ECMAScript.VueI18n` 源码为准。

## 运行

```bash
dotnet test src/ECMAScript.VueI18n.Test/ECMAScript.VueI18n.Test.csproj
dotnet run --file scripts/csharp/test-dotnet.cs -- --project vue-i18n
```
