# Wiki 阶段计划

> Status: 活跃计划
> Updated: 2026-05-03
> Positioning: `src/Wiki/` 当前是 `jazor.wiki` 的 library-mode sample，不是内容管理系统；本计划用于区分 sample 闭环、authoring showcase 增强，以及未来若要演进为真实 wiki 时的边界与顺序。

## 1. 当前定位

`src/Wiki/` 当前不是“真正的 wiki 产品”，而是一个可运行的样例站：

- 用 C# + `ECMAScript.Vue3` 的 `H(...)` authoring 构建页面；
- 通过 `JazorEmit` 在构建时把模块发射到 `wwwroot/jazor`；
- 由静态 `index.html` + import map 在浏览器里启动 Vue runtime；
- 页面内提供一个浏览器侧的快速 C# -> JS preview，用于 authoring feedback。

这意味着它当前的首要目标不是文章管理、搜索、编辑和权限，而是：

1. 稳定展示 Jazor 的 library-mode authoring 体验；
2. 为 `ECMAScript.Vue3` / compiler / emit 提供一个真实 sample；
3. 为后续 authoring ergonomics 提供反馈载体。

## 2. 当前基线（已完成）

当前已经具备的最小闭环：

- ASP.NET Core 静态宿主、默认页与 `/health`；
- `AppModule.cs` 负责 Vue + Vuetify bootstrap；
- `WikiHomeModule.cs` 负责页面 render 与 playground 逻辑；
- `Wiki.csproj` 已开启 `JazorEmit`，输出到 `wwwroot/jazor`；
- `main.mjs`、`components/wiki-home.mjs` 与 manifest 已能稳定产出；
- `serve.ps1` / `build-local.ps1` 可用于本地构建和预览。

因此 `src/Wiki/` 当前应视为：

- **Baseline complete**
- **Sample-grade**
- **Not product-grade**

## 3. 阶段总路线

建议把 `src/Wiki/` 拆成四个阶段，避免“样例站”和“产品 wiki”混成一条线。

### Phase 0: Baseline（已完成）

目标：

- 跑通最小宿主、emit、boot、页面渲染、playground 预览闭环。

当前状态：

- 已完成。

验收标准：

- `dotnet build src/Wiki/Wiki.csproj` 成功；
- `wwwroot/jazor/main.mjs` 与 `components/wiki-home.mjs` 存在；
- 本地运行后 `/health` 返回 200；
- `index.html` 能加载 import map 和 `./jazor/main.mjs`。

### Phase 1: Sample 收口

目标：

- 把“能跑的 demo”收口成“稳定样例”。

范围：

- 文档、脚本、产物、命名、定位说明、基本 smoke verification。

验收标准：

- README 与实际实现一致，不再出现旧命名或错误定位；
- 明确说明 preview 是 browser-side fast preview，而不是 compiler-accurate output；
- `build-local.ps1` / `serve.ps1` 使用路径、前置条件和失败提示稳定；
- 增加最小 smoke verification，覆盖 build、产物存在、`/health`、首页入口；
- sample 的外部依赖说明完整，尤其是 CDN import map 与 Vuetify 依赖边界。

### Phase 2: Authoring Showcase 增强

目标：

- 让 `jazor.wiki` 成为更强的 Jazor / Vue3 authoring showcase，而不是只展示一个静态页面。

范围：

- 更多示例场景、更强的对照信息、更明确的 authoring feedback 面板。

验收标准：

- Playground 至少支持 3 到 5 个代表性示例场景切换；
- 页面能展示更多“发射结果”信息，而不只是简单字符串替换结果；
- sample 覆盖常见 authoring 面：element、props、events、component、slot、basic reactivity；
- 页面结构仍保持 sample 可维护性，不把它做成第二个编译器前端。

### Phase 3: Real Wiki MVP

目标：

- 若产品方向成立，把它从 single-page sample 演进成真正的文档站 / wiki MVP。

范围：

- 内容源、导航、路由、索引页、文档页、基础信息架构。

验收标准：

- 先明确内容源策略：静态 markdown、JSON、或 C# 模块三选一；
- 至少支持多页面内容组织，而不是单页；
- 支持导航、目录、上一页/下一页、索引页；
- sample/authoring 展示区与文档内容区职责清楚，不相互污染；
- 在进入本阶段前，先明确“文档站”还是“可编辑 wiki”，不要混用目标。

### Phase 4: Productization

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

按当前仓库状态，最合理的下一步是 **Phase 1: Sample 收口**。

推荐切片如下：

### Task 1: README 与定位对齐

目标：

- 修正文档中的命名漂移与定位歧义。

验收标准：

- `ECMAScript.Vue` 之类旧表述统一为当前真实命名；
- 明确 sample、playground、fast preview、library-mode 的边界；
- 构建与预览命令和现状一致。

### Task 2: 增加 Wiki smoke verification

目标：

- 让 Wiki 不再只靠人工打开页面验证。

验收标准：

- 至少覆盖 build 成功；
- 至少覆盖 emitted module 存在；
- 至少覆盖本地启动后 `/health` 返回 200；
- 至少覆盖首页存在 `#app` 和 `./jazor/main.mjs` 引用。

### Task 3: 脚本与失败提示收口

目标：

- 让 `build-local.ps1` / `serve.ps1` 的使用路径更稳定。

验收标准：

- 缺失 emitted module 时有清晰提示；
- `-Build` / `-BuildLocal` 的行为边界清楚；
- Dry-run / 本地预览的输出说明完整。

## 6. 进入条件

进入 Phase 2 前至少满足：

- Phase 1 的 README、脚本、smoke verification 已闭环；
- `src/Wiki/` 的定位仍明确为 sample；
- 不需要为了 showcase 扩张新的 compiler 特路。

进入 Phase 3 前至少满足：

- 已明确 `src/Wiki/` 不再只是 sample；
- 已选定内容源与路由策略；
- 已接受页面结构和维护目标会发生变化。

进入 Phase 4 前至少满足：

- 已存在可用的多页面 MVP；
- 已有最小自动化验证；
- 依赖与部署策略已定。

## 7. 非当前目标

在 Phase 1 / Phase 2 中，以下事项不应混入：

- 后台内容管理；
- 用户、权限、评论、编辑器；
- 把 browser-side preview 伪装成真实编译结果；
- 因 Wiki 需求反向要求 compiler 新增 sample-only 特路；
- 在没有产品决策前，把 sample 机械扩张成“半个 wiki 产品”。

## 8. 参考

- `src/Wiki/README.md`
- `src/Wiki/Wiki.csproj`
- `src/Wiki/Program.cs`
- `src/Wiki/AppModule.cs`
- `src/Wiki/WikiHomeModule.cs`
- `src/Wiki/build-local.ps1`
- `src/Wiki/serve.ps1`
