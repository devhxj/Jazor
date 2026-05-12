# Playground

`Playground` 是仓库中的真实生产化案例，用来验证 `razorvue + vuetify + pinia + vueroute + aspnetcore + DenoHost` 在非 `Jolt`、非 `Vite`、单 ASP.NET Core 项目路线下的可行性。

## 结构

- `Playground/Playground`：唯一的 .NET Web 项目，负责 API、静态资源和 HTML shell。
- `playground-consumer`：Deno consumer，消费 `wwwroot/jazor` 中的 RazorVue SFC 产物，输出浏览器与 SSR 产物。

这不是双宿主方案。运行时只有一个 ASP.NET Core 项目；consumer 只是同仓库内的前端构建管线。

## 关键约束

- 不使用 `Jolt`
- 不使用 `Vite`
- 使用 `DenoHost` / bundled `deno.exe`
- 生成页面来自 RazorVue SFC emit
- 最终浏览器资产写入 `Playground/wwwroot/assets`

## 运行方式

1. 构建宿主并生成 RazorVue 产物：

```powershell
dotnet build src/Playground/Playground/Playground.csproj -v minimal
```

2. 运行 consumer 打包：

```powershell
dotnet run --file src/Playground/playground-consumer/scripts/run-deno.cs -- task build
```

3. 启动宿主：

```powershell
dotnet run --project src/Playground/Playground/Playground.csproj
```

默认页面：

- `/`：catalog
- `/examples/{id}`：detail

## 验证命令

```powershell
dotnet run --file src/Playground/playground-consumer/scripts/run-deno.cs -- task smoke:ssr
dotnet run --file src/Playground/playground-consumer/scripts/run-deno.cs -- task build
dotnet run --file src/Playground/playground-consumer/scripts/run-deno.cs -- task smoke:browser
```

## 当前真实边界

本案例暴露出若干 RazorVue 当前支持边界，已单独记录到：

- `docs/04-补充/razorvue-playground-support-gaps-2026-05-12.md`

这些点不是 `Playground` 私有问题，而是后续 RazorVue library-mode 生产化需要继续抬升的能力面。
