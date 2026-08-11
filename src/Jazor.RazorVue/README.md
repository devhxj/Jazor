# Jazor.RazorVue

> 定位：官方 Razor Source Generator 到 Vue render-function `.mjs` 的当前集成实现。

`Jazor.RazorVue` 只消费 official Razor SG 完成后的 Roslyn `Compilation` 和 generated C#。它将 `BuildRenderTree` 相关 `IOperation` 通过 `Jazor.Compiler` 降低为 Vue render-function artifact，不将 Razor DR/IR、SFC 或中间 marker protocol 作为回退路径。

## 职责

- 选择 RazorVue 组件并绑定最终 generated C# 中的组件类型与 `BuildRenderTree` operation。
- 建立当前组件成员闭包，并通过 `AstConverterOptions`、`SemanticWalkerHost` 和模块策略进入核心 compiler。
- 在 `RazorSdk/Lowering/` 中处理 RenderTreeBuilder、current-component state、children-to-slot 和 Vue runtime bridge 等产品特有 framing。
- 构建确定的 Vue module、source map 与 `Jazor.Generated.VueRenderCatalog` carrier，交由 `Jazor.Emit` 写入 `.mjs`、map 与 manifest。

## 边界

- Razor 参数、必填参数和参数类型由 Razor/C# compiler 验证，本项目不重复实现这些检查。
- C# 表达式、成员和函数语义必须经 `Jazor.Compiler` translation hook 处理；RazorVue 只可为 compiler 不拥有的 Vue artifact framing 直接构造 AST。
- `Jazor.Analyzer` 提供 Razor SG hook/bootstrap；本项目不拥有 authoring analyzer、SFC frontend 或宿主 RPC。
- DOM `@bind`、`EventCallback`、slot 和 lifecycle 的支持范围以当前 official SG 输入与回归测试为准；不支持的协议必须给出可定位诊断。

## 代码结构

| 位置 | 责任 |
| --- | --- |
| `Generation/` | generator hook、入口与诊断 |
| `RazorSdk/GeneratedCSharpBinder.cs` | final generated C# 和 operation 绑定 |
| `RazorSdk/ComponentSelector.cs` | 组件发现与选择 |
| `RazorSdk/MemberClosureBuilder.cs` | 组件成员闭包和 compiler 选项 |
| `RazorSdk/Lowering/` | RazorVue 特有的 render-function lowering |
| `RazorSdk/Catalog/` | ASP.NET Components mapping catalog |
| `RazorSdk/VueModuleBuilder.cs` | Vue module framing 与 source map 组装 |

## 验证

```bash
dotnet build src/Jazor.RazorVue/Jazor.RazorVue.csproj --no-restore -v minimal
dotnet test src/Jazor.RazorVue.Sg.Test/Jazor.RazorVue.Sg.Test.csproj --no-restore --filter 'ComponentMemberClosure|FinalDocument|GeneratedCSharpBinder' -v minimal /nr:false /p:UseSharedCompilation=false
```

## 相关文档

- [Jazor.Vue](../Jazor.Vue/README.md)
- [Razor-to-Vue](../../docs/02-architecture/razor-to-vue.md)
- [产物管线](../../docs/02-architecture/artifact-pipeline.md)
