# Spec: Wiki Real Project MVP

## Objective

把 `src/Wiki/` 从单页 sample 演进成一个真实可用的文档站 / wiki MVP，并明确采用 **C# + `ECMAScript.Vue3` + H 函数** 作为生产级前端 authoring 标准。

目标用户：

- 仓库维护者，需要一个真实站点承载 `Jazor` / `RazorVue` / `Jolt` 的产品与工程文档；
- 新接入工程师，需要稳定的导航、路由、索引与部署说明；
- 当前仓库自身，需要一个真实站点证明 H-function authoring 可以承载生产级页面结构，而不只是 sample。

本阶段成功标准：

- `Wiki` 不再是单页 playground，而是多页面 docs shell；
- 站点支持真实 URL 路由与刷新进入；
- 至少具备索引页、文档页、导航、目录、上一页/下一页；
- 页面主体由 H 函数 authoring 构建，而不是退回模板或 sample-only 特路；
- 构建、运行、smoke 验证能覆盖真实文档路由。

## Assumptions

1. 当前目标是 **文档站 / wiki MVP**，不是可编辑 CMS。
2. 内容源先采用 **C# 静态内容模块**，不在本阶段引入 markdown、数据库或后台管理。
3. 当前宿主继续是 ASP.NET Core 静态站点，依靠 `MapFallbackToFile("index.html")` 支持前端路由。
4. 页面 authoring 以 H 函数为主，不为了 wiki 需求新增 compiler sample-only 特路。

## Tech Stack

- Host: ASP.NET Core (`src/Wiki/Program.cs`)
- Frontend authoring: `ECMAScript.Vue3`
- UI bootstrap: `ECMAScript.Vuetify` runtime bootstrap retained
- Emit: `JazorEmit` -> `wwwroot/jazor`
- Delivery: static `index.html` + emitted ESM modules

## Commands

- Build: `dotnet build .\src\Wiki\Wiki.csproj`
- Local build with emit verification: `dotnet run --file .\scripts\csharp\wiki-build-local.cs`
- Local preview: `dotnet run --file .\scripts\csharp\wiki-serve.cs -- --build`
- Smoke verification: `dotnet run --file .\scripts\csharp\wiki-verify-smoke.cs -- --build-local`

## Project Structure

- `src/Wiki/Program.cs` -> 静态宿主、`/health`、前端路由 fallback
- `src/Wiki/AppModule.cs` -> Vue app bootstrap
- `src/Wiki/WikiHomeModule*.cs` -> H 函数站点壳层、页面内容与导航逻辑
- `src/Wiki/wwwroot/index.html` -> 静态入口与 import map
- `src/Wiki/wwwroot/site.css` -> 真实文档站样式
- `src/Wiki/README.md` -> 项目定位、运行方式、真实路由说明
- `docs/02-计划/wiki/` -> 活跃规格、计划与阶段说明

## Code Style

真实文档页继续用 typed H-function authoring，而不是退回字符串模板：

```csharp
private static IVNode PageSection(string id, string title, params IVNode[] content)
    => H("section", new VueObject { Id = id, Class = "doc-section" },
    [
        H("div", new VueObject { Class = "section-anchor" }, id),
        H("h2", title),
        H("div", new VueObject { Class = "section-body" }, content)
    ]);
```

约束：

- 页面结构优先可读性，不把内容塞成大字符串；
- 页面元数据和导航路径保持显式；
- 不引入“为了通用化而通用化”的内容 DSL；
- 每个切片保持可构建、可运行、可 smoke 验证。

## Testing Strategy

- 当前以 focused build + smoke verification 为主；
- smoke 必须覆盖：
  - build 成功；
  - emitted modules 存在；
  - `/health` 返回 200；
  - `/` 与至少一个真实文档路由返回 200；
  - emitted wiki component 中包含真实文档路由或页面标识；
- 后续若 Wiki 继续扩大，再补独立测试项目。

## Boundaries

- Always:
  - 保持 H 函数为页面 authoring 主路径
  - 保持真实路由可刷新进入
  - 保持 build/smoke 可重复运行
  - 保持页面内容和导航结构明确可维护

- Ask first:
  - 引入 markdown 管线
  - 引入数据库、搜索服务或用户系统
  - 切换依赖策略，例如完全移除 CDN 或新增前端构建链

- Never:
  - 把 browser-side demo preview 伪装成真实编译输出
  - 为 Wiki 反向新增 sample-only compiler 特路
  - 在未定义内容源策略前混入后台编辑、评论、权限

## Success Criteria

- 用户访问 `/` 能看到真实首页，而不是 sample playground
- 用户访问如 `/guides/getting-started` 的文档路由时能正常返回并加载
- 左侧导航、正文、右侧目录、上下页导航都已形成闭环
- README 和计划文档已切换到真实项目视角
- `wiki-verify-smoke.cs` 已覆盖至少一个真实 docs route

## Open Questions

- 第二阶段是否要把文档内容继续保持 code-first，还是引入 markdown 内容源
- 是否继续保留 playground 作为单独文档页，而不是首页主体验
- CDN 依赖是否在 productization 阶段切换为本地静态资源或锁定镜像
