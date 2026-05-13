# Playground

`Playground` 是仓库中的真实生产化案例，用来验证 `razorvue + vuetify + pinia + vueroute + aspnetcore + DenoHost` 在非 `Jolt`、非 `Vite`、单 ASP.NET Core 项目路线下的可行性。

## 结构

- `src/Playground`：唯一的 .NET Web 项目，负责 API、静态资源和 HTML shell。
- `src/Playground/jazor`：开发/测试阶段的 RazorVue SFC emit 目录，和 `Wiki` 保持同类模式。
- `src/Playground/wwwroot/jazor`：浏览器 bundle 的本地输出目录；发布时会与根 `jazor` emit 合并为最终 `/jazor/*` 静态目录。
- `src/Playground/consumer`：DenoHost consumer，消费根 `jazor` 中的 RazorVue SFC 产物，输出浏览器与 SSR 产物。

这不是双宿主方案。运行时只有一个 ASP.NET Core 项目；consumer 只是同仓库内的前端构建管线。

## 关键约束

- 不使用 `Jolt`
- 不使用 `Vite`
- 使用 `DenoHost.Core` + bundled runtime assets
- 生成页面来自 RazorVue SFC emit
- 浏览器资产写入 `src/Playground/wwwroot/jazor`
- 不使用 `.ps1`，验证与构建辅助都走 `dotnet run --file` 或 MSBuild target

## 构建与发布契约

`Playground` 参考 `Wiki` 的资源模型：

- 本地 build 生成 `src/Playground/jazor`
- 宿主使用 `JazorWebApplication.CreateBuilder()` 处理源码/发布双形态内容根
- 宿主开发时用 `UseJazorWebAssets()` 同时提供 `wwwroot` 静态资产和根 `jazor` development assets
- HTML shell fallback 使用 `UseJazorSpaFallback()`，只兜底无扩展名的 HTML 导航请求，不使用 endpoint catch-all
- consumer 在 `JazorEmit` 后自动运行，生成 `wwwroot/jazor/client-entry.*`
- publish 后只从发布目录的 `wwwroot/jazor` 提供 `/jazor/*`
- publish 阶段会把根 `jazor` emit 与 `wwwroot/jazor/client-entry.*` 合并到发布目录的 `wwwroot/jazor`
- 发布根目录不能出现影子 `jazor/` 目录
- consumer 的中间 build root 默认使用 `.deno-build/pid-*`，避免并行 smoke/build 互相清空目录；需要固定路径时可显式设置 `RAZORVUE_BUILD_ROOT`

## 运行方式

1. 构建宿主、生成 RazorVue 产物并打包浏览器资产：

```powershell
dotnet build src/Playground/Playground.csproj -v minimal
```

2. 启动宿主：

```powershell
dotnet run --project src/Playground/Playground.csproj
```

默认页面：

- `/`：catalog
- `/examples/{id}`：detail

## 验证命令

本地真实宿主 smoke：

```powershell
dotnet run --file scripts/csharp/playground-verify-smoke.cs -- --build
```

发布形态 smoke：

```powershell
dotnet run --file scripts/csharp/playground-verify-smoke.cs -- --publish
```

consumer focused smoke：

```powershell
dotnet run --file src/Playground/consumer/scripts/run-deno.cs -- task smoke:ssr
dotnet run --file src/Playground/consumer/scripts/run-deno.cs -- task build
dotnet run --file src/Playground/consumer/scripts/run-deno.cs -- task smoke:browser
```

## 当前真实边界

本案例暴露出若干 RazorVue 当前支持边界，已单独记录到：

- `docs/04-补充/razorvue-playground-support-gaps-2026-05-12.md`

这些点不是 `Playground` 私有问题，而是后续 RazorVue library-mode 生产化需要继续抬升的能力面。
