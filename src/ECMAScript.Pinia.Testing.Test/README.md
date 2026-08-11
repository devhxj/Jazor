# ECMAScript.Pinia.Testing.Test

> 定位：`ECMAScript.Pinia.Testing` 的独立回归测试项目。

## 覆盖范围

- `@pinia/testing` 项目布局与仓库接线守护。
- `createTestingPinia()`、`TestingOptions`、`TestingInitialState`、spy factory 与 `stubActions` 的 authoring contract。
- `ProjectPlugin(...)`、`ProjectStubActionPredicate(...)` 与 `ProjectStubActions(...)` 的类型投影。
- `Jazor.Compiler` lowering，确保 binding 能生成正确的 `@pinia/testing` import。

## 运行

```bash
dotnet test src/ECMAScript.Pinia.Testing.Test/ECMAScript.Pinia.Testing.Test.csproj
```

采集覆盖率：

```bash
dotnet test src/ECMAScript.Pinia.Testing.Test/ECMAScript.Pinia.Testing.Test.csproj --settings src/ECMAScript.Pinia.Testing.Test/coverlet.runsettings
```

仓库统一入口：

```bash
dotnet run --file scripts/csharp/test-dotnet.cs -- --project pinia-testing
```

## 相关文档

- [ECMAScript.Pinia.Testing](../ECMAScript.Pinia.Testing/README.md)
- [开发与测试](../../docs/03-guides/development-and-testing.md)
