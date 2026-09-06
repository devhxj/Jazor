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
- 完整 browser history 语义、SSR/prerender route identity 和完整 hydration 副作用 parity 仍不声明支持；当前 history 子集只覆盖已验证的 `popstate`/`hashchange` handler、取消恢复、竞态和释放协议。

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

## P0/P1 已闭环切片

截至 2026-09-06，以下范式工作已经有实现、回归和可重复入口：

| 切片 | 当前证据 |
| --- | --- |
| P0：失败诊断与生成稳定性 | RazorVue SG 测试 `4948/4948` 通过；诊断路径排序具有 Ordinal tie-breaker；生成失败不留下 partial artifact。 |
| P0：质量门禁 | `dotnet run --file scripts/csharp/verify-razorvue-coverage.cs -- --no-restore --no-build`：行覆盖率 `14054/14431 = 97.39%`，分支覆盖率 `6056/6442 = 94.01%`。 |
| P0：Debug/HMR/SPA/SSR 交付 | 2026-09-06 实跑通过：`verify-smoke.cs --configuration Release`（101.53s，含 PathBase 浏览器旅程）、`verify-development-hmr.cs`、`verify-windows-spa-release.cs -- --path-base /docs`、`verify-windows-ssr-release.cs -- --path-base /todo`；[范式证据矩阵](../02-architecture/razorvue-paradigm.md#p0p1-验收证据入口) 固定复现命令。 |
| P1：响应式与生命周期 | 参数队列、异步 lifecycle、slot、`@key`、卸载竞态和 SSR 首屏等待由 `RazorSgOfficial*RuntimeTests`、`RazorSgComponentMemberClosureTests` 及消费端脚本覆盖；完整 CLR reference parity 仍是边界。P1-A 已补齐版本化 SSR state envelope（schema/version/props/providers）及错误校验；P1-B 已提供 `JazorAuthenticationState`、`JazorAuthenticationEnvelope` 和显式 browser provider，覆盖匿名、登录、刷新、过期、403、登出、错误保持及请求竞态；P1-C 已支持单一显式构造函数的普通引用类型服务参数（既有 provider key + Vue inject），其余复杂 activation 仍按 Guidance/Reject 处理。P1-D 已支持 history 事件的 handler 协议、取消恢复、竞态和 dispose；不宣称服务器 circuit 或完整 Blazor UI parity。 |
| P1：范式级调试 | `dotnet run --file scripts/csharp/inspect-razorvue-chain.cs -- --source ... --generated ... --artifact ... --map ... --json` 输出 source → generated → module → map 链路并在映射断裂时失败。 |
| P1：中型应用基线 | `scripts/csharp/benchmark-razorvue-g2.cs` 已建立 plain-text、counter、keyed-list-100、static-vnode 的 render/update 吞吐和 gzip 基线；后续优化必须使用同一参数重测。 |
| P2：Element Plus typed binding 切片 | `elementplus --check` 通过（111 components、2 directives）；官方 Razor SG + Deno 回归覆盖 `ElButton`/`ElInput` 的 enum prop、click、双向绑定、slot、splat 和 import；Release package consumer 断言 Element Plus ESM/CSS 资源闭包，真实浏览器 smoke 可读取发布资源。现有协议足够，本轮没有新增 wrapper-JS 或弱类型协议。 |
| P2：协议边界与评估 | `jazor-ssr-state` v1 现在拒绝重复 provider key；`IJSRuntime` 家族保持 `JAZORVGA022` Reject 并有作者面回归；所有当前 SDK Microsoft Blazor 内置 UI 组件统一由 `JAZORVGA021` Reject（包括 `CacheView`、`ConfigureBrowser`、`ImportMap`、`ResourcePreloader`、`AntiforgeryToken`、`FormMappingScope`、`DisplayName<T>`、`InputHidden`、`Label<T>`、`EnvironmentView`、`Virtualize`、`QuickGrid`、Section）；`StreamRendering` 由 `JAZORVCA012` Reject；localization 与复杂 validation 仍是独立 Guidance；运行时 benchmark 基线已记录，未宣称优化收益。 | [P2 执行计划](./p2-plan.md) |
| P0：组件绑定统一门禁 | `dotnet run --file scripts/csharp/verify-vue-binding-contracts.cs` 依次执行 Element Plus、Vuetify、TDesign 的生成检查，并验证三套 manifest 与上游快照版本、README 原始注释来源保持一致。 |

这些记录是当前实现的验收快照，不等同于扩大支持边界。新增能力仍须同时更新实现、测试、作者指南和适用 consumer 证据。
