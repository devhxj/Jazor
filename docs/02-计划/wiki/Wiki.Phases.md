# Wiki 阶段计划

> Status: Phase 1–5 已完成
> Updated: 2026-05-08
> Positioning: `src/Wiki/` 当前是已完成的生产级文档站，基于 C# + `ECMAScript.Vue3` H 函数 authoring 构建，包含 24 个注册页面、全文搜索、导航壳层、完整验证管线、本地化依赖、CI 自动化、部署契约，以及 ASP.NET Core `PathBase` / 开发重载链路收口。

## 1. 当前定位

`src/Wiki/` 是已完成的真实文档站 MVP：

- 用 C# + `ECMAScript.Vue3` 的 `H(...)` authoring 构建全部 24 个页面；
- 通过 `JazorEmit` 在构建时把模块发射到 `wwwroot/jazor`；
- 由静态 `index.html` + import map 在浏览器里启动 Vue runtime；
- 完整 SPA 路由、导航壳层、全文搜索、响应式布局、暗色/亮色主题。

当前首要目标：

1. 作为 Jazor / RazorVue / Jolt 的产品与工程文档站点；
2. 证明 H-function authoring 可以承载生产级页面结构；
3. 为后续 productization 和内容扩展提供稳定基线。

## 2. 当前基线（已完成）

当前已具备的完整文档站闭环：

- ASP.NET Core 静态宿主、`/health`、host-rendered HTML shell fallback；
- `AppModule.cs` 负责 Vue bootstrap；
- `WikiHomeModule.cs` 及 26 个 partial 文件负责站点壳层和 24 个页面内容；
- `WikiCatalogGuard.cs` 启动校验全部路由目录完整性；
- `Wiki.csproj` 已开启 `JazorEmit`，输出到 `wwwroot/jazor`；
- `main.mjs`、`components/wiki-home.mjs` 与 manifest（48 模块）已稳定产出；
- `serve.ps1` / `build-local.ps1` / `verify-smoke.ps1` / `verify-browser.ps1` 构建与验证闭环；
- `Wiki:PathBase` / `Wiki__PathBase` 子路径部署支持已打通；
- 开发时 injected client、reload websocket、静态资源 URL、discovery documents、SourceMap 服务链路都已在根路径和 `/docs` 子路径下通过验证。

因此 `src/Wiki/` 当前应视为：

- **Phase 1–5 complete**
- **Docs-site MVP**
- **Verified (HTTP smoke + headless browser, root-path + PathBase)**

## 3. 阶段总路线

建议把 `src/Wiki/` 拆成五个阶段，避免“样例站”和“产品 wiki”混成一条线，并把传统 ASP.NET Core 宿主硬化单独收口。

### Phase 0: Baseline（已完成）

目标：

- 跑通最小宿主、emit、boot、页面渲染闭环。

当前状态：

- 已完成。

### Phase 1: Sample 收口（已完成）

目标：

- 把”能跑的 demo”收口成”稳定样例”。

当前状态：

- 已完成。README 定位已更新，smoke verification 已覆盖 build、产物、`/health`、首页入口。

### Phase 2: Authoring Showcase 增强（已完成）

目标：

- 让 wiki 成为更强的 Jazor / Vue3 authoring showcase。

当前状态：

- 已完成。24 个页面覆盖 element、props、events、component、reactivity，以及独立外部绑定文档面；H 函数成为生产级标准。

### Phase 3: Real Wiki MVP（已完成）

目标：

- 从 single-page sample 演进成真实文档站 MVP。

当前状态：

- 已完成。24 个注册页面（Foundation 11 + Engineering 10 + Operations 3），三栏导航壳层，SPA 路由，全文搜索，Glossary，FAQ，Troubleshooting，Topic Index，阅读进度条，session scroll memory，暗色/亮色主题，移动端响应式，无障碍，SEO 元数据，WikiCatalogGuard 启动校验，HTTP smoke + headless Edge CDP 浏览器验证。

### Phase 4: Productization（已完成）

目标：

- 把 MVP 收口成可维护、可验证、可部署的站点。

当前状态：

- 已完成。Vue 3 已本地化到 `wwwroot/vendor/`，站点完全离线可用；GitHub Actions CI workflow 在 PR/push 时自动运行 smoke + publish-smoke 验证；`DEPLOY.md` 文档化了发布命令、目录结构契约、关键不变量和回滚程序；`verify-smoke.ps1` 包含漂移检测和 vendor 断言。

### Phase 5: ASP.NET Core Hosting Hardening（已完成）

目标：

- 让 `Wiki` 从“站点可运行”推进到“传统 ASP.NET Core 宿主可按生产标准部署和调试”。

当前状态：

- 已完成。`Wiki:PathBase` / `Wiki__PathBase` 已支持反向代理子路径部署；`wwwroot/index.html` 已改为 host-rendered token shell；开发重载注入脚本和 websocket 已支持 path base；`verify-smoke.ps1` 与 `verify-browser.ps1` 已覆盖根路径和 `/docs` 子路径；浏览器验证已覆盖 `main.mjs` / `wiki-home.mjs` 的 `sourceMapURL` 与 `.mjs.map` 服务链路。

## 4. 推荐推进路径

### 路径 A：长期作为 sample

如果 `src/Wiki/` 的长期目标仍然是样例站，推荐顺序是：

1. Phase 1
2. Phase 2

不要直接做 Phase 3/4，因为那会把 sample 的边界打散。

### 路径 B：转成真正的文档站 / wiki

如果目标是产品化内容站，推荐顺序是：

1. 先做完 Phase 1
2. 明确产品目标与内容源
3. 直接进入 Phase 3
4. MVP 稳定后再做 Phase 4

不要在当前单页 playground 上持续堆产品能力，否则会形成“既不是好 sample，也不是好站点”的中间态。

## 5. 当前推荐动作

Phase 1–5 已全部完成。当前不再建议把 `Wiki` 当成“待 productize 的 sample”；更合理的定位是把它作为已收口的传统宿主参考实现持续维护。

推荐切片如下：

### Task 1: 自动化补齐子路径发布验证

目标：

- 把 `/docs` 子路径的 publish smoke/browser 验证固定进入仓库级自动化。

验收标准：

- CI 稳定运行 `verify-smoke.ps1 -Publish -PathBase /docs`；
- CI 稳定运行 `verify-browser.ps1 -Publish -PathBase /docs`。

### Task 2: 复用 ASP.NET Core 宿主基元

目标：

- 让未来 ASP.NET Core 宿主消费者复用 `Jazor.AspNetCore` / `Jazor.AspNetCore.Dev`，避免各项目重复实现 path-base shell 与开发重载拼接。

验收标准：

- 新宿主复用公共 middleware / helper；
- 不再出现项目内重复的 reload client/path-base 注入实现。

### Task 3: 宿主线职责边界文档化维持

目标：

- 持续防止 `Wiki` / `Jolt` / `Jazor.Emit` 三者职责重新混淆。

验收标准：

- `Wiki` 文档继续明确“不做 HMR、不做 LSP/dev host”；
- 新增宿主相关状态页或 ADR 时保持边界措辞一致。

## 6. 进入条件

Phase 5 的进入条件已经全部满足并完成收口：

- Phase 1–3 已闭环（当前已满足）；
- 已有可用的多页面 MVP（当前已满足：24 个页面）；
- 已有最小自动化验证（当前已满足：HTTP smoke + headless browser）；
- 依赖与部署策略已定（当前已满足）；
- ASP.NET Core 子路径部署与开发重载链路已验证（当前已满足）。

## 7. 非当前目标

在当前 `Wiki` 宿主线中，以下事项不应混入：

- 后台内容管理、用户系统、权限、评论、编辑器；
- 引入 markdown 管线或数据库（除非产品决策明确）；
- 为 Wiki 反向新增 sample-only compiler 特路；
- 在 `Wiki` 内实现 HMR、LSP、`.jazor` 开发时宿主或 `Jolt` 级调试职责；
- 把 `Jazor.Emit` 演进成开发服务器。

## 8. 参考

- `src/Wiki/README.md`
- `src/Wiki/Wiki.csproj`
- `src/Wiki/Program.cs`
- `src/Wiki/AppModule.cs`
- `src/Wiki/WikiHomeModule.cs`（主壳层，2014 行）
- `src/Wiki/WikiHomeModule.RouteContract.cs`（路由目录，1108 行）
- `src/Wiki/WikiHomeModule.Elements.cs`（元素工具，413 行）
- `src/Wiki/WikiCatalogGuard.cs`（启动校验）
- `src/Wiki/wwwroot/site.css`（样式，1377 行）
- `src/Wiki/build-local.ps1`
- `src/Wiki/serve.ps1`
- `src/Wiki/verify-smoke.ps1`
- `src/Wiki/verify-browser.ps1`
- `docs/03-完成/wiki/status.md`
