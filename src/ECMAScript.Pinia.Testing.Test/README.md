# ECMAScript.Pinia.Testing.Test

`ECMAScript.Pinia.Testing.Test` 是 `src/ECMAScript.Pinia.Testing/` 的独立回归工程。

## Scope

- `@pinia/testing` 独立项目布局与仓库接线守护。
- `createTestingPinia()`、`TestingOptions`、`TestingInitialState`、spy factory contract 的反射验证。
- 编译边界验证，确保 `ECMAScript.Pinia.Testing` 能被 `Jazor.Compiler` 消费并正常降级为 `@pinia/testing` 导入。

## Run

```powershell
dotnet test src/ECMAScript.Pinia.Testing.Test/ECMAScript.Pinia.Testing.Test.csproj
```

带覆盖率：

```powershell
dotnet test src/ECMAScript.Pinia.Testing.Test/ECMAScript.Pinia.Testing.Test.csproj --settings src/ECMAScript.Pinia.Testing.Test/coverlet.runsettings
```

共享入口：

```powershell
pwsh ./scripts/test-dotnet.ps1 -Project pinia-testing
```
