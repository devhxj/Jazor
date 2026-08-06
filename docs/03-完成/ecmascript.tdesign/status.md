# ECMAScript.TDesign 完成审计（2026-08-06）

> Status: 当前完成状态
> Scope: `src/ECMAScript.TDesign/` 对 `tdesign-vue-next@1.20.5` 的强类型 authoring surface、随包资源与生成验证。

## 交付边界

- npm registry 的 `latest` 为 `tdesign-vue-next@1.20.5`；本仓库的版本化上游快照位于 `src/ECMAScript.Vue.Generator/upstream/tdesign-vue-next/1.20.5`。
- `components.json` 记录 120 个文档条目，生成链将别名归并为 118 个唯一运行时组件；`contracts.json`、`bindings.json` 与 `TBasic.g.cs`、`TComponents.cs`、`TRegistry.cs` 由同一冻结输入生成。
- 每个组件的 prop 使用 `[Parameter]`，仅在原始 Vue 名称不同才使用 `[ECMAScriptName]`；slot 使用 `RenderFragment` / `RenderFragment<T>`；event 使用 `EventCallback` / `EventCallback<T>`，异常原始事件名使用 `VueLibraryEmit`。
- authoring contract 不引入额外的 prop/slot 元数据特性。
- 公共契约不使用 `object` 或 `VueValue` 作为 prop、slot 或 event 的回退。`AdditionalAttributes` 是 Razor 的未匹配属性透传入口；带重叠分支的 union 使用带标签的 `IUnion` 形式以保持精确投影。

## 资源自包含

- NuGet 包包含 `dist/tdesign.mjs`、`dist/tdesign.css`、MIT `licenses/LICENSE` 和 `manifest.json`。
- TDesign ESM 的唯一 bare import 是 `vue`。`manifest.json` 以 `requires.vue3 = ^3.5.0` 声明依赖，`ECMAScript.Vue3` 同样随 NuGet 交付浏览器 ESM；`LibraryMaterializer` 按 manifest 把两者物化到生成应用的本地 `vendor/` 路径。
- TDesign CSS 不包含远程 URL。应用消费者不需要 Node.js、npm、Deno、CDN 或 `node_modules`。
- `ECMAScript.TDesign.nuspec` 将根 `manifest.json`、`dist/**` 和 `licenses/**` 打入 `jazor/tdesign-vue-next`，`buildTransitive/ECMAScript.TDesign.targets` 将其 manifest 注册给 Jazor materialization。

## 可重复生成

维护者通过独立的 `ECMAScript.Vue.Generator` 项目更新或验证绑定。Tree-sitter 仅在维护者生成时解析冻结的 TypeScript 声明，不进入消费者运行时或构建依赖。

```text
dotnet run --project src/ECMAScript.Vue.Generator -- tdesign snapshot --check
dotnet run --project src/ECMAScript.Vue.Generator -- tdesign bindings --check
dotnet run --project src/ECMAScript.Vue.Generator -- tdesign components --report
dotnet run --project src/ECMAScript.Vue.Generator -- tdesign components --check
```

当前检查结果：上游快照有效，940 个导出的 TypeScript 声明已索引，120/120 个文档组件均找到 Props 定义，118/118 个运行时组件均生成并映射到 authoring surface。

## 回归守护

`EcmaScriptVueProxyTests` 以冻结的 contracts、bindings 和真实 `tdesign.mjs` 为基线，验证：

- 文档条目、运行时 export、`TComponents`、`TComponentRegistry` 与 authoring type 一一对应；
- prop、slot、event 均使用现行协议，且退役特性不存在；
- `Pick` / `Omit`、接口继承和 union 精确投影不会退化；
- 所有公开 TDesign 契约保持强类型。

## 验证基线

本轮已通过：

- 三段 TDesign 生成器的 `--check`，以及 component generator 的 `--report`；
- `dotnet pack src/ECMAScript.TDesign/ECMAScript.TDesign.csproj -c Release`，并直接检查 nupkg 包含 DLL、targets、manifest、ESM、CSS 和许可证；
- 真实 nupkg 回归：TDesign manifest 只声明包内资源和 `vue3` 依赖，ESM 的唯一 bare import 为 `vue`，CSS 不含远程 URL；
- `dotnet test src/Jazor.CompilerTest/Jazor.CompilerTest.csproj --no-build`：10,315 通过；
- `dotnet test src/Jazor.RazorVue.Sg.Test/Jazor.RazorVue.Sg.Test.csproj --no-restore`：4,491 通过；
- `dotnet test src/Jazor.EmitTest/Jazor.EmitTest.csproj --no-build`：137 通过，1 个显式跳过的 Netpack toolchain 场景。该套件包含本地 NuGet 消费者、资源物化和真实浏览器 smoke。
