# Wiki 状态（2026-05-08）

> Status: 当前状态快照（PathBase / 调试链路收口后更新）
> Positioning: `src/Wiki/` 的仓库级状态页
> Scope: ASP.NET Core 传统宿主、子路径部署、开发时全页重载、首响应元数据、静态资产与 SourceMap 可达性

## 结论

`Wiki` 当前已经从“可运行的 docs-site MVP”推进到**可以正式按 ASP.NET Core 传统站点模式部署的生产候选状态**。

这次收口后，可以明确做出的工程声明是：

- 根路径部署和反向代理子路径部署（例如 `/docs`）都已形成真实主路径。
- 开发时注入脚本、`@jazor/reload` websocket、静态资源 URL、首响应 metadata、`robots.txt`、`sitemap.xml` 在 `PathBase` 下都已校正。
- `main.mjs` 与 `components/wiki-home.mjs` 的 `sourceMapURL` 以及实际 `.mjs.map` 服务链路已被浏览器级验证覆盖。
- 当前 `Wiki` 的调试/热更新模型是 **ASP.NET Core / `dotnet watch` 热重载 + 浏览器全页 reload**，而不是 `Wiki` 自己实现 HMR。

如果目标问题是“当前 `Wiki` 这条传统 ASP.NET Core 宿主线，调试模式、开发重载、SourceMap、子路径部署能否按生产标准继续推进”，当前答案是**可以**。

## 当前生产边界

当前生产边界应明确表述为：

- `Wiki` 是构建在 ASP.NET Core 上的传统 web 宿主，不是 `Jolt`。
- `Wiki` 的浏览器端 Jazor 产物来自编译时 emit，运行时通过 `/jazor/*` 静态资产加载。
- 开发期编辑 C# 时，主循环依赖 ASP.NET Core / `dotnet watch` 的 hot reload 或进程重启；Jazor 浏览器模块侧通过开发重载服务触发整页刷新。
- `Wiki` 不承载 `.jazor` authoring、开发服务器 HMR、LSP、DAP/CDP 调试宿主等 `Jolt` 职责。

因此当前不应做出的声明包括：

- 不应宣称 `Wiki` 自身提供 HMR。
- 不应把 `Wiki` 的开发重载误写成 `Jolt` dev server 能力。
- 不应把 `Jazor.Emit` 描述成开发服务器或热更新宿主；它仍然只负责 emit/materialisation。

## 本轮收口内容

### 1. 子路径部署主路径已落地

- `Program.cs` 新增 `Wiki:PathBase` / `Wiki__PathBase` 读取，并在静态资源、HTML shell、discovery documents 前统一 `UsePathBase(...)`。
- `WikiHostShell.cs` 改为 host 渲染 `wwwroot/index.html`，把入口模块、import map、favicon、CSS、canonical URL 和 path base 作为 token 在首响应阶段写入。
- `Wiki` 站内路由从此明确区分：
  - 逻辑文档路径：`/guides/...`、`/engineering/...`、`/search`
  - 浏览器外部路径：`PathBase + 逻辑文档路径`

### 2. 开发重载链路与 PathBase 对齐

- `Jazor.AspNetCore.Dev` 下的开发注入脚本、HTML 注入、reload middleware、reload service 已统一支持 path base 感知。
- 开发脚本从 `<html data-wiki-path-base="...">` 读取宿主前缀，再拼接 websocket URL，避免 `/docs` 下仍错误连接根路径 `@jazor/reload`。
- 结果是：开发环境下的 injected client、reload websocket、静态模块地址与页面当前外部 URL 前缀保持一致。

### 3. 首响应 metadata 与发现文档已补齐宿主正确性

- HTML shell 在首响应阶段即输出 route-correct 的 `<title>`、description、canonical、Open Graph、Twitter、robots。
- `/search` 仍保持 utility surface 边界：可访问、可分享查询，但 `noindex, nofollow` 且不进入 `sitemap.xml`。
- 未知路由仍返回可恢复 shell，但维持 HTTP 404 和 `X-Robots-Tag: noindex, nofollow`。
- `robots.txt`、`sitemap.xml`、静态资产 URL 在 `PathBase` 下都已对齐外部可访问地址。

### 4. SourceMap / 调试可达性已有真实浏览器证据

- 浏览器验证脚本已经检查 `/jazor/main.mjs` 与 `/jazor/components/wiki-home.mjs` 的 `sourceMapURL`。
- 对应 `.mjs.map` 文件可被实际服务，且能追溯到预期 C# 源文件。
- 这说明当前 `Wiki` 这条 ASP.NET Core 传统宿主线已经具备“浏览器可消费的 emitted module sourcemap”这一调试基础能力。

## 已验证证据

本轮已通过的 focused 验证包括：

```powershell
dotnet test src/Jazor.EmitTest/Jazor.EmitTest.csproj --filter 'FullyQualifiedName~JazorAspNetCoreDevelopmentReloadTests|FullyQualifiedName~JazorAspNetCoreHostingTests' -v minimal
```

```powershell
dotnet run --file .\scripts\csharp\wiki-verify-smoke.cs -- --build
dotnet run --file .\scripts\csharp\wiki-verify-browser.cs -- --build-local
```

```powershell
dotnet run --file .\scripts\csharp\wiki-verify-smoke.cs -- --build --path-base /docs
dotnet run --file .\scripts\csharp\wiki-verify-browser.cs -- --build-local --path-base /docs
```

以上验证已经覆盖：

- 开发模式 `/@jazor/client` 注入与 `@jazor/reload` websocket
- 根路径与 `/docs` 子路径下的 HTML shell、静态资源、SPA 导航和 discovery documents
- 首响应 metadata、search/noindex、404/noindex 契约
- 浏览器侧 SourceMap 可见性和 `.mjs.map` 服务链路

## 当前判断

当前更准确的状态不是“Wiki 只是一个 sample 站”，而是：

- 它已经是一个真实 docs-site MVP；
- 并且在 ASP.NET Core 传统宿主模式下，子路径部署、开发重载、静态资产寻址、首响应 SEO metadata、SourceMap 服务链路都已经形成生产候选主路径。

换句话说，`Wiki` 当前可以作为：

- Jazor / RazorVue / Jolt 的真实文档站；
- ASP.NET Core + Jazor emit 传统部署模式的 reference host；
- 后续 ASP.NET Core 线调试/部署规范的验证样板。

## 剩余边界与后续动作

当前仍需保持以下边界清晰：

- `Wiki` 不实现 HMR；如果未来要做模块级 HMR，应属于另一条开发时宿主线，而不是把复杂度塞回 `Wiki`。
- `Wiki` 不替代 ASP.NET Core 的 C# hot reload；C# 编辑循环应继续与 `dotnet watch` / 宿主重启协作。
- `Jazor.Emit` 不演进成 dev host；它继续保持 emit/materialisation 分层。

建议的后续动作是：

1. 把 `-Publish -PathBase /docs` 的 smoke/browser 验证固定纳入 repo 级自动化入口，而不只停留在本轮人工回归。
2. 如果后续还有新的 ASP.NET Core 宿主消费者，优先抽复用 `Jazor.AspNetCore` / `Jazor.AspNetCore.Dev`，不要在各站点重复拼接 shell/path-base 逻辑。
3. 继续把 `Wiki` 作为“传统宿主线”样板维护，而把 `.jazor` authoring、HMR、LSP、调试宿主扩展严格留在 `Jolt` 或未来专门宿主中。

## 相关入口

- `src/Wiki/README.md`
- `src/Wiki/DEPLOY.md`
- `src/Wiki/Program.cs`
- `src/Wiki/WikiHostShell.cs`
- `src/Wiki/WikiHomeModule.cs`
- `scripts/csharp/wiki-verify-smoke.cs`
- `scripts/csharp/wiki-verify-browser.cs`
- `src/Jazor.AspNetCore.Dev/`
- `src/Jazor.EmitTest/JazorAspNetCoreDevelopmentReloadTests.cs`
- `src/Jazor.EmitTest/JazorAspNetCoreHostingTests.cs`
