# Jazor 官方网站

`samples/Wiki` 是 Jazor 的中文官方网站实现，部署地址为
`https://devhxj.github.io/Jazor/`。它仍然是完整的 ASP.NET Core + Jazor
宿主，用于本地开发、冒烟测试和生成静态 Pages 产物；网站正文不在此处维护。

## 内容来源

仓库 `docs/` 是唯一的正文来源。`Wiki.csproj` 在构建前运行
`scripts/csharp/wiki-import-docs.cs`，将 Markdown 解析为
`obj/wiki/WikiDocsContent.g.cs` 的纯数据目录，随后由 `WikiHomeModule.DocsPage.cs`
渲染为 Vue `H()` VNode。不要手动编辑 `obj/wiki/WikiDocsContent.g.cs`。

| Markdown 来源 | 网站路由 |
| --- | --- |
| `docs/README.md` | `/` |
| `docs/01-overview/` | `/overview` |
| `docs/02-architecture/` | `/architecture` |
| `docs/03-guides/` | `/guides` |
| `docs/04-roadmap/` | `/roadmap` |
| `docs/05-history/` | `/history` |

每个分组的 `README.md` 是分组落地页，其余 Markdown 文件按文件名映射到该分组下的路由。导入器同时生成标题、摘要、章节、正文块树、搜索语料、源文件、git 最后提交日期和同组相邻页关系。

## 本地开发

在仓库根目录执行：

```bash
dotnet run --file scripts/csharp/wiki-import-docs.cs -- --check
dotnet run --file scripts/csharp/wiki-serve.cs -- --build
```

开发宿主地址由脚本输出。生成的浏览器模块位于 `samples/Wiki/jazor/`，为本地构建产物，不提交到仓库。

## 静态导出

GitHub Pages 使用与发布宿主相同的首响应 HTML、元数据和路由边界。默认导出到已忽略的 `output/wiki/`，使用 `/Jazor` 路径前缀和 `https://devhxj.github.io` 站点源：

```bash
dotnet run --file scripts/csharp/wiki-export-static.cs
dotnet run --file scripts/csharp/wiki-export-static.cs -- --serve
```

第二条命令会在静态目录上启动本地预览服务；地址由脚本输出。导出器会校验每条文档路由、`/search`、`404.html`、`robots.txt`、`sitemap.xml`、canonical/OG URL 和所有资源文件。

## 验证

```bash
dotnet run --file scripts/csharp/wiki-verify-smoke.cs -- --build
dotnet run --file scripts/csharp/wiki-verify-browser.cs -- --build
dotnet run --file scripts/csharp/wiki-export-static.cs
dotnet run --file scripts/csharp/wiki-verify-smoke.cs -- --publish --path-base /docs
```

smoke 验证由生成目录驱动，自动覆盖当前 `docs/` 的所有页面。浏览器验证覆盖 SPA 路由、搜索、hash、持久化 shell 状态、移动端抽屉、source map 和 `ECMAScript.Style` 运行时；需要 Node.js 和 Google Chrome。

部署配置、Pages 工作流和自定义域名约定见 [DEPLOY.md](./DEPLOY.md)。

## 相关文档

- [产品概览](../../docs/01-overview/README.md)
- [产物管线](../../docs/02-architecture/artifact-pipeline.md)
- [开发与测试](../../docs/03-guides/development-and-testing.md)
