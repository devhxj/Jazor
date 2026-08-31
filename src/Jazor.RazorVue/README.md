# Jazor.RazorVue

> 定位：官方 Razor Source Generator 到 Vue render-function `.mjs` 的当前集成实现。

`Jazor.RazorVue` 只消费 official Razor SG 完成后的 Roslyn `Compilation` 和 generated C#。它将 `BuildRenderTree` 相关 `IOperation` 通过 `Jazor.Compiler` 降低为 Vue render-function artifact，不将 Razor DR/IR、SFC 或中间 marker protocol 作为回退路径。

## 职责

- 选择 RazorVue 组件并绑定最终 generated C# 中的组件类型与 `BuildRenderTree` operation。
- 建立当前组件成员闭包，并通过 `AstConverterOptions`、`SemanticWalkerHost` 和模块策略进入核心 compiler。
- 在 `RazorSdk/Lowering/` 中处理 current-component state、children-to-slot 等产品特有 projection；`RenderEmitter` 直接将 RenderTreeBuilder operation 组织为 Vue VNode AST。
- 构建确定的 Vue module、source map 并写入 `Jazor.Generated.ModuleCatalog`，交由 `Jazor.Emit` 统一物化为 `.mjs`、map 和输出 projections。
- Vue runtime helper 直接从 `vue` 导入；Vue HMR payload 仍由本项目生成和解释。

## 组件契约

RazorVue 将所有进入组件 lowering 的类型统一视为 RazorVue 组件。组件类型必须同时满足：

- 可赋值给 `Microsoft.AspNetCore.Components.ComponentBase`，包括通过自定义基类间接继承；
- 实现 `ECMAScript.Vue.IVueComponent` 或其派生接口；
- 声明组件导入描述：本地/应用组件使用 `[ECMAScriptModule("...")]`，第三方库代理使用 `[ECMAScript("package", Transform.Component, "Export")]`。

`IVueComponent<TProps>`、`IVueComponent<TProps, TSlots>` 和组件库自己的派生 marker 都满足第二项。两个导入描述同时出现时，`[ECMAScriptModule]` 优先。HTML 元素、`RenderFragment` 和普通 C# 类型不属于组件类型，不需要该 marker；Microsoft Blazor 内置 UI 组件即使继承 `ComponentBase`，因没有 `IVueComponent` 契约仍应稳定 Reject。

## 边界

- Razor 参数、必填参数和参数类型由 Razor/C# compiler 验证，本项目不重复实现这些检查。
- C# 表达式、成员和函数语义必须经 `Jazor.Compiler` translation hook 处理；RazorVue 只可为 compiler 不拥有的 Vue artifact framing 直接构造 AST。
- Razor 标记与手写 `BuildRenderTree` 都属于受约束的 direct-render 协议；`@code`/`.razor.cs` 的可达 helper、事件和生命周期成员属于 component logic。两者的完整 Support/constraint/reject 矩阵见 [RazorVue 作者指南](../../docs/03-guides/razorvue-authoring.md)。
- `Jazor.Analyzer` 提供 Razor SG hook/bootstrap；本项目不拥有 authoring analyzer、SFC frontend 或宿主 RPC。
- 第三方库组件使用框架中性的 `[ECMAScript(import, Transform.Component, exportName)]`；RazorVue 只在类型同时满足 `ComponentBase + IVueComponent` 时解释该 import，不把 JS interop 或其他框架包装器纳入 Vue render-function lowering。
- DOM `@bind`、`EventCallback`、slot、source-base lifecycle/Dispose、parameterless constructor replay 和 direct-render imperative loop 的支持范围以当前 official SG 输入与回归测试为准；不支持的协议必须给出可定位诊断。

## 代码结构

| 位置 | 责任 |
| --- | --- |
| `Generation/` | generator hook、入口与诊断 |
| `RazorSdk/GeneratedCSharpBinder.cs` | final generated C# 和 operation 绑定 |
| `RazorSdk/ComponentSelector.cs` | 组件发现与选择 |
| `RazorSdk/MemberClosureBuilder.cs` | 组件成员闭包和 compiler 选项 |
| `RazorSdk/Lowering/` | RazorVue 特有的 render-function lowering、ASP.NET Components product hooks 与 Vue projection framing |
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
