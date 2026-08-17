# 当前状态

> 定位：当前产品范围与验证入口，不是某次构建或审计的历史快照。

## 核心平台

Jazor 的当前核心是受控 C# -> ECMAScript 转换：Roslyn `IOperation` 进入 `Jazor.Compiler`，生成 ESTree 和确定性 ECMAScript 模块，随后由 `Jazor.Emit` 物化或打包。宿主 API 通过 CLR/ECMAScript 映射和白名单定义，未支持的运行时语义在使用点明确失败。

当前核心持续维护的能力包括模块发射、导入收集、source origin、source map carrier、CLR 映射、静态分析与 Emit 交付。详细边界见 [编译器](../02-architecture/compiler.md) 和 [产物管线](../02-architecture/artifact-pipeline.md)。

## 框架集成

当前已实现的框架集成是 Razor-to-Vue。它以官方 Razor Source Generator 完成后的最终 `Compilation` 为输入，通过 `Jazor.RazorVue` 完成组件绑定和 Vue framing，并复用核心编译器降低 C# 语义。

RazorVue 的生产输出是 direct Vue render-function `.mjs`。当前已接受的 static hoist、精确 static VNode cardinality、按需 raw-markup runtime、child-level block tree（dynamic string text、mixed text 与 nested block）、保守 patch flags、setup-instance handler cache、已证明安全的 `foreach` `renderList` / keyed-unkeyed fragment、stable/dynamic slot 精确 lowering、direct string/boolean DOM bind，以及 shallow parameter lifecycle watch 边界见 [RazorVue Direct Render 性能评审](./razorvue-direct-h-performance.md)。production Vue gate 已确认 keyed reorder identity、slot 分支更新、direct bind patch，以及 scalar/reference parameter replacement；同一 prop 引用内部的 nested mutation 不定义为新参数赋值。artifact generation 在保持 stable discovery/order 的前提下有界并行，release library assets 由 generated `PackageImports` 和 package-declared closure 按需物化；browser 不复制无关 SSR/devtools entry，SSR 则显式保留 `vue` / `@vue/server-renderer`。完整边界、实测口径和未启用的 cache/preload 策略见 [RazorVue 极致性能路线图](./razorvue-extreme-performance.md)。

作者 C# 面已覆盖两层：Razor 标记和手写 `BuildRenderTree` 共用 direct-render 协议；`@code`/`.razor.cs` 的可达 helper、handler、lifecycle、source-base dispatch、parameterless constructor replay 和 static module members 走 component-logic/module framing。含普通 `break`/`continue` 的 `for`、`foreach`、`while`、`do while` 会使用 imperative loop lowering 保留控制流；`goto`、无法投影的 labeled branch、跨 open frame branch、参数化 component activation 和 `SetParametersAsync` 仍是明确边界。完整矩阵见 [RazorVue 作者指南](../03-guides/razorvue-authoring.md)。

RazorVue 作者面现已使用 final Compilation typed diagnostics：`JAZORVGA021`-`026` 分别覆盖 direct-render、compiler bridge、component binding、member closure、VueInject 和 Vue module；`JAZORVGA020` 收缩为 bootstrap/未分类内部失败。mapped `.razor` location、稳定多组件聚合和错误时无 partial catalog 由官方 Razor SG 回归覆盖。member class 进入 Vue Proxy 时使用 proxy-safe mangled storage；支持切片、拒绝边界和升级门禁见 [RazorVue 作者指南](../03-guides/razorvue-authoring.md) 与 [作者面诊断路线图](./razorvue-authoring-diagnostics.md)。

Vue 3、Vue Router、Pinia、Vue Devtools、Vue Data UI、Vu Icons、UI 库绑定、`ECMAScript.Style`、`Jazor.Admin` 和 ASP.NET Core SSR 都围绕核心平台或当前 RazorVue 集成提供能力；它们不改变 Jazor 的框架无关核心定位。`ECMAScript.VueDataUi` 完整映射 `vue-data-ui` 3.23.4 的 71 个公开 `vue-ui-*` entry，保留 typed dataset/config authoring，并由 package manifest 按实际 import 物化 runtime closure。`ECMAScript.VuIcons` 映射 `vu-icons` 1.5.4 的 1,821 个静态 wrapper；静态使用按单图标 entry 物化，`VuIcon` 动态名称使用则显式物化全量 catalog。

`Jazor.React`、`Jazor.RazorReact` 等未来方向尚未构成已接受的产品范围或公开 API。任何新框架集成必须遵守 [框架集成层](../02-architecture/framework-integrations.md) 的边界。

## 交付与 SSR

`JazorMode=debug` 生成模块、source map 与 manifest；`JazorMode=release` 通过 Netpack 生成浏览器 bundle。library package ESM 保持 external，但只物化应用实际 import 的 entry 与 manifest 声明的依赖/relative-file closure；这减少发布目录和部署复制量，不将未请求的文件错误计入首屏网络收益。启用 `JazorSSR=true` 的 ASP.NET Core 应用可使用本地 Vue SSR 与 hydration，DenoHost 负责服务器模块执行，Netpack 只负责浏览器构建。SSR 使用有界、generation-aware persistent Deno worker pool；manifest/import-map 变化会轮换 ESM generation，取消、crash、并发上限与应用关闭均有真实进程回归。SSR 发布链路由独立 NuGet 消费者门禁覆盖：本地打包后以 `JazorSSR=true` 发布 RazorVue TodoList，验证 `jazor/ssr` 图、packaged DenoHost 渲染的服务端 HTML、PathBase 部署资源解析与 Edge hydration 交互恢复。

配置方法见 [安装与配置](../03-guides/installation-and-configuration.md)。

## 质量门槛与验证

| 范围 | 门槛 | 入口 |
| --- | --- | --- |
| 核心编译器 | 至少 10,000 个通过场景、98% 行覆盖率、96% 分支覆盖率 | `dotnet run --file scripts/csharp/verify-compiler-coverage.cs` |
| Razor-to-Vue | 至少 4,000 个通过场景、90% 行覆盖率、96% 分支覆盖率 | `dotnet run --file scripts/csharp/verify-razorvue-coverage.cs` |
| Vue 绑定 | 每个目标至少 90% 已审计公共绑定契约 | `dotnet run --file scripts/csharp/verify-vue-binding-coverage.cs` |
| 全仓库主线 | 当前 compiler、CLR、Devtools、Vue Data UI、Vu Icons、Pinia、VueRoute、Razor SG、Emit 测试 lane | `dotnet run --file scripts/csharp/test-dotnet.cs` |
| Windows SPA 发布消费者 | 本地 NuGet 包、Release bundle、`/docs` PathBase 与 Edge 真实浏览器交互 | `dotnet run --file scripts/csharp/verify-windows-spa-release.cs -- --path-base /docs` |
| Windows SSR 发布消费者 | 本地 NuGet 包、`JazorSSR=true` Release publish、packaged DenoHost SSR HTML、部署资源解析与 Edge hydration 交互 | `dotnet run --file scripts/csharp/verify-windows-ssr-release.cs -- --path-base /todo` |

门槛描述的是可复现的验收规则。需要引用某一时点的实际结果时，应运行对应命令或查看 [CHANGELOG.md](../../CHANGELOG.md) 的发布记录，而不是依赖已删除的历史报告。
