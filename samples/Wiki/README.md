# Wiki

> 定位：使用 Jazor 和 ASP.NET Core 构建文档站点的端到端示例。

Wiki 使用 `ECMAScript.Vue` 的 C# `H()` authoring、`ECMAScript.Style` 生成样式、Jazor module 输出和 ASP.NET Core static hosting 实现文档站点。它用于验证站点壳、路由、产物、SEO 元数据和浏览器交互，不是独立的产品线或可编辑 CMS。

## 结构

- `Wiki.csproj` 与 `Program.cs`：ASP.NET Core host、静态资源与路由 fallback。
- `WikiHomeModule*.cs`：页面目录、站点壳、导航、TOC 与文章内容的 C# module authoring。
- `AppModule.cs` 与 `WikiStyleSheet.cs`：浏览器 bootstrap 和真实页面使用的 `ECMAScript.Style` 模块。
- `wwwroot/`：站点资源和 vendored Vue runtime；`jazor/`：Jazor 生成产物。
- `scripts/csharp/wiki-*.cs`：本地构建、预览、smoke 和浏览器验证入口。

## 构建与预览

在仓库根目录执行：

```bash
dotnet run --file scripts/csharp/wiki-build-local.cs
dotnet run --file scripts/csharp/wiki-serve.cs -- --build
```

预览地址由脚本输出；默认可访问根路径和 `/search`。需要 production-shape 预览时：

```bash
dotnet run --file scripts/csharp/wiki-serve.cs -- --publish
```

生成模块位于 `samples/Wiki/jazor/`；发布时复制到 `<publish>/jazor/`，浏览器地址保持 `/jazor/*`。Wiki 默认 `JazorMode=debug`，发布入口会显式传递 `-p:JazorMode=release`，使生产预览实际加载 `bundle.js`。

## 验证

```bash
dotnet run --file scripts/csharp/wiki-verify-smoke.cs -- --build
dotnet run --file scripts/csharp/wiki-verify-browser.cs -- --build-local
dotnet run --file scripts/csharp/wiki-verify-smoke.cs -- --publish --path-base /docs
dotnet run --file scripts/csharp/wiki-verify-browser.cs -- --publish --path-base /docs
dotnet run --file scripts/csharp/verify-windows-spa-release.cs -- --path-base /docs
```

smoke 验证路由、Debug/Release 产物、首屏元数据、robots/sitemap 和静态资源。浏览器验证还覆盖 SPA 路由、搜索、hash、持久化 shell 状态、移动端抽屉以及 `ECMAScript.Style` 生成 class 与受管理样式标签；该入口需要 Node.js 和 Microsoft Edge。Windows release consumer 验证会打包并只使用本地 NuGet 包恢复同一份 Wiki。

部署与 PathBase 配置见 [DEPLOY.md](./DEPLOY.md)。

## 相关文档

- [ECMAScript.Vue](../../src/ECMAScript.Vue/README.md)
- [产物管线](../../docs/02-architecture/artifact-pipeline.md)
- [示例总览](../../docs/03-guides/examples.md)
