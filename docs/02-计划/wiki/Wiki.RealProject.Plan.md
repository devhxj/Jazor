# Implementation Plan: Wiki Real Project MVP

## Overview

本计划把 `samples/Wiki/` 从 sample 改造成真实文档站 MVP。第一阶段不追求 CMS，而是先完成真实文档站闭环：真实路由、多页面内容、导航壳层、目录、部署说明和可验证的构建入口。

## Architecture Decisions

- 内容源先采用 **code-first C# page modules**，因为用户已经明确要求把 H 函数作为生产 authoring 标准。
- 路由采用 **ASP.NET Core fallback + 前端路径解析**，支持真实 URL 和刷新进入。
- 页面正文采用 **显式 H 函数 sections**，而不是引入新的内容 DSL。
- 当前不引入编辑后台、持久化、用户系统或全文搜索服务。

## Task List

### Phase 1: Product Foundation

- [x] Task 1: 写入真实项目规格与计划
  - Acceptance: 产品目标、内容源、路由、边界、成功标准全部落盘
  - Verify: `docs/02-计划/wiki/` 出现可读 spec / plan 文件
  - Files: `docs/02-计划/wiki/*`

- [x] Task 2: 切换静态宿主到真实 docs 路由
  - Acceptance: 宿主支持 `/` 之外的文档路径直接进入
  - Verify: `dotnet run --file .\scripts\csharp\wiki-verify-smoke.cs -- --build-local`
  - Files: `samples/Wiki/Program.cs`, `scripts/csharp/wiki-verify-smoke.cs`

- [x] Task 3: 实现多页面 H-function docs shell
  - Acceptance:
    - 首页不再是 sample playground
    - 至少 4 个真实页面可访问
    - 存在左侧导航、正文区、右侧目录、上下页导航
  - Verify: `dotnet build .\samples\Wiki\Wiki.csproj`
  - Files: `samples/Wiki/WikiHomeModule*.cs`, `samples/Wiki/wwwroot/site.css`

### Checkpoint: Product Foundation

- [x] Build 成功
- [x] `/health`、`/`、`/guides/getting-started` 可访问
- [x] 页面已形成真实站点结构

### Phase 2: Project Positioning

- [x] Task 4: 更新 README 到真实项目视角
  - Acceptance: README 不再把 Wiki 描述成单页 sample，命令与路由说明正确
  - Verify: 人工审阅 README 与实现一致
  - Files: `samples/Wiki/README.md`

- [x] Task 5: 扩充 smoke verification 到真实 route
  - Acceptance: smoke 覆盖至少一个真实文档路由，并验证 emitted 文档内容标识
  - Verify: `dotnet run --file .\scripts\csharp\wiki-verify-smoke.cs -- --build-local`
  - Files: `scripts/csharp/wiki-verify-smoke.cs`

### Checkpoint: MVP Ready

- [x] Build / smoke 全部通过
- [x] 真实路由闭环已完成
- [x] README / spec / implementation 一致

## Risks and Mitigations

| Risk | Impact | Mitigation |
|------|--------|------------|
| H-function 页面内容过大，模块可读性下降 | Medium | 用 partial 文件拆分布局与内容 |
| 真实路径刷新进入失败 | High | 在宿主加 `MapFallbackToFile("index.html")`，并把 route 加入 smoke |
| 为了“像 wiki”而过早引入复杂内容系统 | High | 当前阶段限制为 code-first docs MVP |
| 产品定位与 sample 遗留内容混杂 | Medium | 首页直接切换为 docs shell，README 同步切换 |

## Open Questions

- 下一阶段是否保留 playground 为单独页面
- 是否需要独立 `Wiki.Test` 项目，还是继续以 smoke 为主
