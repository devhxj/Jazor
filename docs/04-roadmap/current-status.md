# 当前状态

> 本页给出今天可以被项目依赖的产品契约，以及可以重复执行的验证入口。计划、一次性实施过程和历史构建数字，不构成当前能力。

审视这里的每项状态，只需要一个问题：今天能否据此设计、编写和交付。答案来自实现、测试与真实消费者证据；愿景、阶段性进展或一次成功的构建，都不足以作为依据。

## 已交付的核心能力

以下能力已经形成实现、验证与交付三者一致的产品契约。

| 能力 | 当前范围 | 详细入口 |
| --- | --- | --- |
| C# 到 ECMAScript | 受支持的 Roslyn `IOperation` 经 `Jazor.Compiler` 降低为 ESTree 和确定性 ECMAScript 模块；导入、临时名、source origin、source map 与宿主映射由编译主线统一负责。 | [编译器](../02-architecture/compiler.md) |
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

以下内容是当前已经明确的产品边界。在新的实现与证据足以改变契约前，它们保持显式失败或明确拒绝：

- Jazor 并非完整 CLR，也不支持任意未映射的 .NET 类型、成员或运行时身份。
- Microsoft/Blazor 内置 UI 组件，例如 `Router`、`RouteView`、`EditForm`、`Input*`、`AuthorizeView` 和 `DynamicComponent`，不作为 RazorVue 的组件入口；UI 层由应用自定义组件或已声明的第三方 binding 提供。
- `IJSRuntime` 字符串互操作、仅服务器端服务、未经版本化协议的认证状态、`PersistentComponentState`、`[PersistentState]` 与 enhanced form handoff 不会被静默模拟。
- 完整 browser history 语义、SSR/prerender route identity 和完整 hydration 副作用 parity 仍不声明支持；当前 history 子集只覆盖已验证的 `popstate`/`hashchange` handler、取消恢复、竞态和释放协议。

后续目标、责任归属与升级门槛见[下一阶段](./next-development.md)。

## 1.0 冻结前状态

公共 API 冻结审查和机器候选快照已建立，详见[1.0 公共 API 冻结审查](../03-guides/public-api-freeze.md)。`1.0.0-preview.1` 已作为首个冻结候选发布；当前包名、命名空间、`AddJazor*` / `UseJazor*` 扩展面和配置模型没有计划中的重命名；`verify-release-candidate.cs` 与手动 `Release Candidate Verification` workflow 已提供统一候选验收入口，在发布候选 ref 上重新生成并通过 API 兼容性检查、全部质量门禁、SPA/SSR 消费者门禁和 CHANGELOG 证据前，正式 `1.0.0` 仍不标记为可发布。冻结状态必须由一组可追溯的门禁结果和对应 `1.0.0` CHANGELOG 条目共同确认。

2026-09-14 本地完整解决方案构建和主线 Release 测试通过，公共 API 快照与 `docs/03-guides/public-api-baseline.snapshot.md` 比较结果为 `76108` 对 `76108`，新增 `0`、删除 `0`。Compiler `10711/10711`、CLR `5089/5089`、Razor SG `4982/4982`、Emit `202/202` 及其余生态测试均无失败。完整 Release Candidate 门禁仍是正式冻结的复核入口，但发布认证扩展、长期多版本矩阵和性能趋势采样属于 1.0 之后的运营质量工作，不作为核心 API/功能发布阻塞。

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

核心编译器、Razor-to-Vue 与 Vue 绑定覆盖率门禁由 `.github/workflows/quality-gates.yml` 在相关 pull request 和 main 分支变更中执行，并作为 tag 与手动 NuGet 发布的前置任务。每个门禁保存 TRX、Cobertura（适用时）、文本日志和 Markdown 摘要，关键指标写入 GitHub Actions 摘要。复现方式与报告保留期见[发版与版本规则](../03-guides/release-and-versioning.md#发版门禁)。

这些门槛是对产品声明的验收规则。某次发布的实际结果应查看对应 CI、运行命令或[CHANGELOG.md](../../CHANGELOG.md)；本页不固化历史快照。

## P0/P1 已闭环切片

截至 2026-09-14，以下范式工作已经有实现、回归和可重复入口：

| 切片 | 当前证据 |
| --- | --- |
| P0：失败诊断与生成稳定性 | RazorVue SG 测试 `4982/4982` 通过（由 `dotnet test src/Jazor.RazorVue.Sg.Test/Jazor.RazorVue.Sg.Test.csproj` 当前运行确认）；诊断路径排序具有 Ordinal tie-breaker；生成失败不留下 partial artifact。RazorVue.Authoring 已通过 typed `TPrimaryTableColCell<TaskRow>`、typed `OnRowClick`、组合 `TopContent` slot 及 package/Release/Chrome smoke。 |
| P0：质量门禁 | 2026-09-14 当前 HEAD 独立回归：Compiler `10711/10711`、Razor SG `4982/4982`、Emit `202/202`；覆盖率门禁基线仍为 Compiler 行/分支 `99.42%/97.15%`、Razor SG `97.53%/94.30%`。 |
| P0：Debug/HMR/SPA/SSR 交付 | 2026-09-14 候选验证提交 `1fcde72e` 以 `v0.60.0` 参数完成完整 Release Candidate；随后仅追加了路线图、benchmark 采样策略和 authoring 样例验证提交，未改动 RC 主链路。RC 阶段包含 release notes、Release build、API 兼容性、Compiler/RazorVue/Vue binding coverage、binding baseline、diagnostics、typed bootstrap、主线测试、13 个 NuGet 包、package shape、`/docs` Chrome SPA 与 `/todo` Chrome SSR consumer；归档证据见 `.tmp/rc-95-full/report.md`（CI 会归档到 `artifacts/release-candidate/`）。长期 `wiki-verify` 与 `quality-gates` 额外上传 `toolchain-matrix.md`，关联实际 SDK、Node、Chrome、操作系统和 commit。Wiki browser 与 publish-browser 使用 Node 22（浏览器脚本依赖 Node 22 WebSocket 客户端）。 |
| P1：响应式与生命周期 | 参数队列、异步 lifecycle、slot、`@key`、卸载竞态和 SSR 首屏等待由 `RazorSgOfficial*RuntimeTests`、`RazorSgComponentMemberClosureTests` 及消费端脚本覆盖；完整 CLR reference parity 仍是边界。P1-A 已补齐版本化 SSR state envelope（schema/version/props/providers）及错误校验；P1-B 已提供 `JazorAuthenticationState`、`JazorAuthenticationEnvelope` 和显式 browser provider，覆盖匿名、登录、刷新、过期、403、登出、错误保持及请求竞态；P1-C 已支持单一显式构造函数的普通引用类型服务参数（既有 provider key + Vue inject），其余复杂 activation 仍按 Guidance/Reject 处理。P1-D 已支持 history 事件的 handler 协议、取消恢复、竞态和 dispose；typed bootstrap 的应用自有 DTO、409 版本刷新、422 校验失败和草稿保留已纳入 `quality-gates` 的长期 consumer job，并上传可归档报告；不宣称服务器 circuit 或完整 Blazor UI parity。 |
| P1：范式级调试 | `inspect-razorvue-chain.cs` 输出 source → generated → module → map 链路，支持文本、JSON、SARIF 2.1.0 和 schema `1.0` remediation 报告；`verify-razorvue-diagnostics.cs` 在 Quality Gates 中持续验证成功链路、source map 断链和 generated C# 缺失三条路径，并在两种机器输出中保留 `JAZORVGA020`/`JAZORVGA026`、HelpLink、作者源位置和最小替代建议。2026-09-11 使用 `RazorVue.TodoList/TodoApp.razor` 实际产物验证通过（generated C# 1095 行、module 86 行、source map 映射成功）。 |
| P1：中型应用基线 | `scripts/csharp/benchmark-razorvue-g2.cs` 已建立 plain-text、counter、keyed-list-100、static-vnode 的 render/update 吞吐和 gzip 基线；`benchmark-razorvue-build.cs` 另记录 clean/incremental/HMR/Release 耗时，且对 clean/incremental 记录生成模块/source map 数量、原始与 gzip 体积及增量产物变化，并生成 JSON/Markdown 摘要；`verify-sample-regression-matrix.cs` 在定时/手动 Quality Gates 串行运行 `RazorVue.Authoring` 与 `JazorAdmin` Release smoke，归档逐场景状态、耗时、命令和提交上下文；2026-09-14 runtime 复核已完成，counter 与 keyed-list-100 的差异已量化；Quality Gates 已切换为每周 3 轮 clean/incremental 采样，后续优化必须使用同一参数重测且不得据此宣称收益。2026-09-14 提交 `07ff87db` 的完整 Chrome 矩阵再次通过：`RazorVue.Authoring` 与 `JazorAdmin` 均为 `passed`，未跳过浏览器，使用 .NET SDK `11.0.100-rc.1.26425.128`、Node `v24.14.1`、Chrome `152.0.7977.83`。 |
| P2：Element Plus typed binding 切片 | `elementplus --check` 通过（111 components、2 directives）；官方 Razor SG + Deno 回归覆盖 `ElButton`/`ElInput` 的 enum prop、click、双向绑定、slot、splat 和 import；Release package consumer 断言 Element Plus ESM/CSS 资源闭包，真实浏览器 smoke 可读取发布资源。现有协议足够，本轮没有新增 wrapper-JS 或弱类型协议。 |
| P2：协议边界与评估 | `jazor-ssr-state` v1 拒绝空白/重复 provider key 和认证保留 key 冲突；浏览器 hydration 在首个异步导入前占用 mount，重复入口立即失败，导入/挂载失败保留失败状态；真实浏览器覆盖无效 envelope、并发入口和组件导入/挂载错误，SSR hosting 共 23 项通过；`IJSRuntime` 家族保持 `JAZORVGA022` Reject 并有作者面回归；所有当前 SDK Microsoft Blazor 内置 UI 组件统一由 `JAZORVGA021` Reject（包括 `CacheView`、`ConfigureBrowser`、`ImportMap`、`ResourcePreloader`、`AntiforgeryToken`、`FormMappingScope`、`DisplayName<T>`、`InputHidden`、`Label<T>`、`EnvironmentView`、`Virtualize`、`QuickGrid`、Section）；`StreamRendering` 由 `JAZORVCA012` Reject；localization 与复杂 validation 已分别记录为 Guidance 及 typed 替代路径；运行时 benchmark 基线已记录，未宣称优化收益。 | [P2 执行计划](./p2-plan.md) |
| P0：组件绑定统一门禁 | `dotnet run --file scripts/csharp/verify-vue-binding-contracts.cs` 依次执行 Element Plus、Vuetify、TDesign 的生成检查，并验证三套 manifest 与上游快照版本、README 原始注释来源保持一致；`--report` 生成 schema `1.0` JSON/Markdown 审阅证据，包含组件/export/prop/event/slot inventory、成员集合、类型/描述集合、fingerprint 和 baseline diff，Markdown 摘要展示受控成员明细，Quality Gates 与 release candidate 均归档并可阻断漂移。 |

这些记录是当前实现的验收快照，不等同于扩大支持边界。新增能力仍须同时更新实现、测试、作者指南和适用 consumer 证据。
