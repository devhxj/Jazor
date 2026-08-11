# 当前状态

> 定位：当前产品范围与验证入口，不是某次构建或审计的历史快照。

## 核心平台

Jazor 的当前核心是受控 C# -> ECMAScript 转换：Roslyn `IOperation` 进入 `Jazor.Compiler`，生成 ESTree 和确定性 ECMAScript 模块，随后由 `Jazor.Emit` 物化或打包。宿主 API 通过 CLR/ECMAScript 映射和白名单定义，未支持的运行时语义在使用点明确失败。

当前核心持续维护的能力包括模块发射、导入收集、source origin、source map carrier、CLR 映射、静态分析与 Emit 交付。详细边界见 [编译器](../02-architecture/compiler.md) 和 [产物管线](../02-architecture/artifact-pipeline.md)。

## 框架集成

当前已实现的框架集成是 Razor-to-Vue。它以官方 Razor Source Generator 完成后的最终 `Compilation` 为输入，通过 `Jazor.RazorVue` 完成组件绑定和 Vue framing，并复用核心编译器降低 C# 语义。

Vue 3、Vue Router、Pinia、UI 库绑定、`ECMAScript.Style`、`Jazor.Admin` 和 ASP.NET Core SSR 都围绕核心平台或当前 RazorVue 集成提供能力；它们不改变 Jazor 的框架无关核心定位。

`Jazor.React`、`Jazor.RazorReact` 等未来方向尚未构成已接受的产品范围或公开 API。任何新框架集成必须遵守 [框架集成层](../02-architecture/framework-integrations.md) 的边界。

## 交付与 SSR

`JazorMode=debug` 生成模块、source map 与 manifest；`JazorMode=release` 通过 Netpack 生成浏览器 bundle。启用 `JazorSsrEnabled=true` 的 ASP.NET Core 应用可使用本地 Vue SSR 与 hydration，DenoHost 负责服务器模块执行，Netpack 只负责浏览器构建。

配置方法见 [安装与配置](../03-guides/installation-and-configuration.md)。

## 质量门槛与验证

| 范围 | 门槛 | 入口 |
| --- | --- | --- |
| 核心编译器 | 至少 10,000 个通过场景、98% 行覆盖率、96% 分支覆盖率 | `dotnet run --file scripts/csharp/verify-compiler-coverage.cs` |
| Razor-to-Vue | 至少 4,000 个通过场景、90% 行覆盖率、96% 分支覆盖率 | `dotnet run --file scripts/csharp/verify-razorvue-coverage.cs` |
| Vue 绑定 | 每个目标至少 90% 已审计公共绑定契约 | `dotnet run --file scripts/csharp/verify-vue-binding-coverage.cs` |
| 全仓库主线 | 当前 compiler、CLR、Pinia、VueRoute、Razor SG、Emit 测试 lane | `dotnet run --file scripts/csharp/test-dotnet.cs` |

门槛描述的是可复现的验收规则。需要引用某一时点的实际结果时，应运行对应命令或查看 [CHANGELOG.md](../../CHANGELOG.md) 的发布记录，而不是依赖已删除的历史报告。
