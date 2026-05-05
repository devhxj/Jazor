# Wiki 阶段计划

> Status: Phase 1–3 已完成，Phase 4 待启动
> Updated: 2026-05-05
> Positioning: `src/Wiki/` 当前是已完成的真实文档站 MVP，基于 C# + `ECMAScript.Vue3` H 函数 authoring 构建，包含 23 个注册页面、全文搜索、导航壳层和完整验证管线。

## 1. 当前定位

`src/Wiki/` 是已完成的真实文档站 MVP：

- 用 C# + `ECMAScript.Vue3` 的 `H(...)` authoring 构建全部 23 个页面；
- 通过 `JazorEmit` 在构建时把模块发射到 `wwwroot/jazor`；
- 由静态 `index.html` + import map 在浏览器里启动 Vue runtime；
- 完整 SPA 路由、导航壳层、全文搜索、响应式布局、暗色/亮色主题。

当前首要目标：

1. 作为 Jazor / RazorVue / Jolt 的产品与工程文档站点；
2. 证明 H-function authoring 可以承载生产级页面结构；
3. 为后续 productization 和内容扩展提供稳定基线。

## 2. 当前基线（已完成）

当前已具备的完整文档站闭环：

- ASP.NET Core 静态宿主、`/health`、`MapFallbackToFile` 前端路由；
- `AppModule.cs` 负责 Vue bootstrap；
- `WikiHomeModule.cs` 及 25 个 partial 文件负责站点壳层和 23 个页面内容；
- `WikiCatalogGuard.cs` 启动校验全部路由目录完整性；
- `Wiki.csproj` 已开启 `JazorEmit`，输出到 `wwwroot/jazor`；
- `main.mjs`、`components/wiki-home.mjs` 与 manifest（48 模块）已稳定产出；
- `serve.ps1` / `build-local.ps1` / `verify-smoke.ps1` / `verify-browser.ps1` 构建与验证闭环。

因此 `src/Wiki/` 当前应视为：

- **Phase 1–3 complete**
- **Docs-site MVP**
- **Verified (HTTP smoke + headless browser)**

## 3. 阶段总路线

建议把 `src/Wiki/` 拆成四个阶段，避免“样例站”和“产品 wiki”混成一条线。

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

- 已完成。23 个页面覆盖 element、props、events、component、reactivity 等全部 authoring 面；H 函数成为生产级标准。

### Phase 3: Real Wiki MVP（已完成）

目标：

- 从 single-page sample 演进成真实文档站 MVP。

当前状态：

- 已完成。23 个注册页面（Foundation 11 + Engineering 9 + Operations 3），三栏导航壳层，SPA 路由，全文搜索，Glossary，FAQ，Troubleshooting，Topic Index，阅读进度条，session scroll memory，暗色/亮色主题，移动端响应式，无障碍，SEO 元数据，WikiCatalogGuard 启动校验，HTTP smoke + headless Edge CDP 浏览器验证。

### Phase 4: Productization（待启动）

目标：

- 把 MVP 收口成可维护、可验证、可部署的站点。

范围：

- 依赖策略、构建验证、部署约束、运维边界。

验收标准：

- CDN 依赖有明确策略：继续使用、锁版本、或提供本地 fallback；
- 有最小自动化 smoke check；
- 构建、静态资源、入口模块、部署目录结构有稳定约束；
- 文档更新和 sample 更新不会无声漂移。

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

Phase 1–3 已全部完成。最合理的下一步是 **Phase 4: Productization**。

推荐切片如下：

### Task 1: CDN 依赖策略

目标：

- 锁定或本地化 CDN 依赖，消除运行时外部依赖不确定性。

验收标准：

- Vue 3 CDN 资源锁版本或提供本地 fallback；
- import map 有明确策略文档。

### Task 2: 持续集成与自动化

目标：

- 将 smoke/browser verification 纳入 CI 管线。

验收标准：

- PR 触发自动 build + smoke 验证；
- 文档更新与 sample 更新有 drift 检测。

### Task 3: 部署约束与运维边界

目标：

- 稳定构建、静态资源、入口模块和部署目录结构。

验收标准：

- 发布流程有文档化的步骤和回滚方案；
- 构建产物结构有稳定约束，不允许无声漂移。

## 6. 进入条件

进入 Phase 4 前至少满足：

- Phase 1–3 已闭环（当前已满足）；
- 已有可用的多页面 MVP（当前已满足：23 个页面）；
- 已有最小自动化验证（当前已满足：HTTP smoke + headless browser）；
- 依赖与部署策略已定（待确认）。

## 7. 非当前目标

在 Phase 4 中，以下事项不应混入：

- 后台内容管理、用户系统、权限、评论、编辑器；
- 引入 markdown 管线或数据库（除非产品决策明确）；
- 为 Wiki 反向新增 sample-only compiler 特路。

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
