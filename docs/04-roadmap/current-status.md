# 当前状态

> 本页给出今天可以被项目依赖的产品契约，以及可以重复执行的验证入口。计划、一次性实施过程和历史构建数字，不构成当前能力。

审视这里的每项状态，只需要一个问题：今天能否据此设计、编写和交付。答案来自实现、测试与真实消费者证据，而不是愿景、阶段性进展或一次成功的构建。

## 已交付的核心能力

以下能力已经形成实现、验证与交付三者一致的产品契约。

| 能力 | 当前范围 | 详细入口 |
| --- | --- | --- |
| C# 到 ECMAScript | `Jazor.Compiler` 将受支持的 Roslyn `IOperation` 降低为 ESTree 和确定性 ECMAScript 模块；导入、临时名、source origin、source map 与宿主映射由编译主线统一负责。 | [编译器](../02-architecture/compiler.md) |
| 模块与资源交付 | 最终宿主只消费两类类库输入：JS resource library 的 `manifest.json + dist/**`，以及纯 Jazor library 的 `Jazor.Generated.ModuleCatalog`。`Jazor.Emit` 解析显式依赖闭包后物化 Debug、Release、SSR 或 HMR 输出。 | [类库资源与引用契约](../02-architecture/library-artifact-contract.md)、[产物管线](../02-architecture/artifact-pipeline.md) |
| Razor-to-Vue | 官方 Razor Source Generator 生成的最终 `Compilation` 经 `Jazor.RazorVue` 绑定为 Vue render-function `.mjs`；C# 表达式、成员和调用语义仍通过核心编译器 lowering。 | [Razor-to-Vue 架构](../02-architecture/razor-to-vue.md) |
| CLR 与外部 API | CLR/ECMAScript 映射、白名单和 runtime helper 共同定义受支持的运行时语义。未映射的类型或成员在使用点明确失败，不降级为原始 JavaScript。 | [编译器](../02-architecture/compiler.md) |

## RazorVue 作者面

RazorVue 的作者体验以官方 Razor SG、强类型组件契约和最终浏览器行为共同界定。

RazorVue 已覆盖自定义组件和已声明第三方组件 binding 的常用作者形态：组件组合、泛型组件、slot/fragment、`@bind`、事件、循环、生命周期、`@key`、`@ref`、可达的 `@code` / `.razor.cs` helper，以及 direct Vue render-function 产物。`ParameterView`、可写 `[Inject]` property、typed/named/nested cascading value、应用自有 route host 和限定的同源内部导航，只在已经证明的浏览器交互或首屏 SSR 子集内声明支持。

框架 primitive 的事件支持限于已验证的只读原生投影：核心 Mouse、Keyboard、Focus、Change，以及 Pointer、Wheel、Drag、Clipboard、Touch、Error、Progress 事件组；`ElementReference.FocusAsync` 也只覆盖已经证明的浏览器交互子集。复杂 lifecycle、导航取消、参数和 cascade 的完整 reference parity、SSR/prerender identity 仍不在声明范围内。

作者失败会通过 source compatibility analyzer 或 final Compilation diagnostics 回到 `.razor` / `.razor.cs` 位置；生成失败不会留下 partial module、`ModuleCatalog` 或 bundle。具体支持矩阵、诊断与替代写法见[RazorVue 作者指南](../03-guides/razorvue-authoring.md)。

## 生态与参考应用

生态绑定与参考应用服务于同一目标：让已声明的能力能够在真实项目中被自然地组合和验证。

- Vue 3、Vue Router、Pinia、Vue Devtools、Vue Data UI、Vu Icons、TDesign、Vuetify、Element Plus 与 `ECMAScript.Style` 共同构成 Jazor 核心之上的生态层；它们以强类型 binding 或资源库形式交付。
- `Jazor.Admin` 是 UI 库无关的管理壳库；`samples/JazorAdmin` 是它的生产级参考应用，以强类型 TDesign 组件实现当前 Starter 功能页面以及门户、IAM、运营场景。它验证编写体验、资源闭包与 Release browser 行为，不反向定义库 API。
- ASP.NET Core 宿主支持 `JazorMode=debug` 的模块、source map、import map 输出，以及 `JazorMode=release` 的浏览器 bundle。启用 `JazorSSR=true` 后，已声明范围内的 Vue SSR 与 hydration 使用同一显式资源闭包。

## 明确边界

以下内容不是“等待自动兼容”的缺口，而是当前已经明确的产品边界。它们将保持显式失败或明确拒绝，直到新的实现与证据足以改变契约：

- Jazor 不是完整 CLR，也不支持任意未映射的 .NET 类型、成员或运行时身份。
- Microsoft/Blazor 内置 UI 组件，例如 `Router`、`RouteView`、`EditForm`、`Input*`、`AuthorizeView` 和 `DynamicComponent`，不作为 RazorVue 的组件入口；UI 层由应用自定义组件或已声明的第三方 binding 提供。
- `IJSRuntime` 字符串互操作、仅服务器端服务、未经版本化协议的认证状态、`PersistentComponentState`、`[PersistentState]` 与 enhanced form handoff 不会被静默模拟。
- `popstate` / `hashchange` cancellation、完整 browser history 语义、SSR/prerender route identity 和完整 hydration 副作用 parity 仍不声明支持。

后续目标、责任归属与升级门槛见[下一阶段](./next-development.md)。

## 质量门槛与验证

质量门槛让每一项产品声明都能回到可执行的验收路径。

| 范围 | 门槛 | 入口 |
| --- | --- | --- |
| 核心编译器 | 至少 10,000 个通过场景、98% 行覆盖率、97% 分支覆盖率 | `dotnet run --file scripts/csharp/verify-compiler-coverage.cs` |
| Razor-to-Vue | 至少 4,000 个通过场景、90% 行覆盖率、94% 分支覆盖率 | `dotnet run --file scripts/csharp/verify-razorvue-coverage.cs` |
| Vue 绑定 | 每个目标至少 90% 已审计公共绑定契约 | `dotnet run --file scripts/csharp/verify-vue-binding-coverage.cs` |
| 仓库主线 | Compiler、CLR、Style、Devtools、Vue Data UI、Vu Icons、Pinia、Pinia.Testing、VueRoute、Razor SG、Emit 测试 lane | `dotnet run --file scripts/csharp/test-dotnet.cs` |
| Windows SPA 发布消费者 | 本地 NuGet 包、Release bundle、`/docs` PathBase 与真实浏览器交互 | `dotnet run --file scripts/csharp/verify-windows-spa-release.cs -- --path-base /docs` |
| Windows SSR 发布消费者 | 本地 NuGet 包、`JazorSSR=true` Release publish、SSR HTML、部署资源解析与 hydration | `dotnet run --file scripts/csharp/verify-windows-ssr-release.cs -- --path-base /todo` |

这些门槛是对产品声明的验收规则。某次发布的实际结果应查看对应 CI、运行命令或[CHANGELOG.md](../../CHANGELOG.md)，而不是把历史快照固化在本页。
