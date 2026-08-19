# Jazor 官方网站部署

Jazor 官方网站通过 GitHub Pages 发布，生产地址为
`https://devhxj.github.io/Jazor/`。静态文件不是从手写 HTML 复制而来，而是由发布版 Wiki 宿主逐条请求后导出，因此与 ASP.NET Core 首响应的 SEO、CSP 和路径行为保持一致。

## GitHub Pages 工作流

[`.github/workflows/wiki-pages.yml`](../../.github/workflows/wiki-pages.yml) 在 `main` 分支发生以下变更时运行：

- `docs/**`
- `samples/Wiki/**`
- `scripts/csharp/wiki-*.cs`
- 工作流自身

工作流以完整 git 历史检出，保证内容目录中的“最后更新”日期来自真实文件提交。它安装 `global.json` 指定的 SDK，执行静态导出，上传 `output/wiki/`，再由 GitHub Pages 部署。仓库 Settings 中将 Pages 的 Source 设为 `GitHub Actions` 后即可启用站点。

统一配置位于工作流顶层：

```yaml
Wiki__PathBase: /Jazor
Wiki__SiteOrigin: https://devhxj.github.io
```

站点的 canonical、Open Graph、Twitter、robots 和 sitemap URL 会组合为
`https://devhxj.github.io/Jazor/...`。未来迁移到自定义域名时，只需同步更新
`Wiki__SiteOrigin`、`Wiki__PathBase`（若需要）和 Pages 的 `CNAME`。

## 本地静态预览

默认导出与 Pages 使用相同的路径和站点源：

```bash
dotnet run --file scripts/csharp/wiki-export-static.cs
dotnet run --file scripts/csharp/wiki-export-static.cs -- --serve
```

导出器会发布 `samples/Wiki/Wiki.csproj`（`JazorMode=release`）、以配置的 PathBase 和 SiteOrigin 启动发布产物，并写出：

```text
output/wiki/
  index.html
  <route>/index.html
  404.html
  robots.txt
  sitemap.xml
  site.css
  favicon.svg
  jazor/
```

`--serve` 是静态目录预览，不是 ASP.NET Core 反向代理；它可用于确认 clean URL、`/Jazor` 前缀和 404 文件在 Pages 形态下正常工作。`output/wiki/` 是本地产物，不提交。

## 部署契约

- `docs/` 是页面正文的唯一来源；导出前会重新运行导入器。
- 每个注册文档路由导出为 `<route>/index.html`，根路径导出为 `index.html`。
- `/search` 是静态外壳页面，查询结果由浏览器运行时根据生成语料计算；它不进入 sitemap。
- 未注册路由导出为 `404.html`，保留可恢复的站点外壳并标注 `noindex, nofollow`。
- Pages 无法配置 ASP.NET Core 响应头，因此 HTML 同时包含与响应头 nonce 相同的 CSP `<meta http-equiv="Content-Security-Policy">`；ASP.NET Core 部署继续保留响应头。
- 导出校验拒绝 `localhost`、未解析模板 token、空文件和缺失路由，确保 Pages artifact 可独立发布。

ASP.NET Core 自托管仍支持 `Wiki__PathBase` 与 `Wiki__SiteOrigin` 配置；Pages 工作流只是该宿主的静态发布入口，不改变本地或反向代理部署方式。
