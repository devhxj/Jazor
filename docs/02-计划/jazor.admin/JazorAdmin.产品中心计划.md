# JazorAdmin 产品中心计划

> Status: 当前实施基线
> Updated: 2026-08-07
> Positioning: `samples/JazorAdmin/` 是 Jazor 的正式参考管理产品，验证 RazorVue、ASP.NET Core、Identity 与本地资源包在真实后台工作流中的组合，不是演示性质的页面集合。

## 目标

JazorAdmin 的一层导航固定为三类独立管理中心：

1. **SSO 中心**：OpenIddict 应用、Scope、授权和令牌的完整生命周期管理。
2. **配置中心**：平台级键值配置的创建、更新、删除与类型校验。
3. **任务中心**：受控运维任务的计划、启停、人工执行和执行历史查看。

组织、成员、资源操作授权和平台账号继续作为通用管理能力，与三中心协同但不混入其路由或 API 前缀。

## 产品约束

- SSO API 统一在 `/api/sso/*`，页面统一在 `/sso/*`；不保留旧的泛化配置路由。
- 通用配置 API 为 `/api/settings/*`，页面为 `/settings`，支持 `text`、`boolean`、`number` 和 `json`。
- 任务 API 为 `/api/schedules/*`，页面为 `/schedules`。
- 样式唯一前缀是 `ja-*`，应用源码、`Jazor.Admin` 壳层、生成模块与 smoke 选择器必须一致，不允许新旧前缀混用。
- 前端依赖通过 NuGet 包内的 manifest 资源物化，不能依赖 CDN、项目 `node_modules` 或单独的前端包管理安装步骤。

## 任务中心方案

调度内核选择 Quartz.NET，不实现自定义 Cron 解析、计时循环、触发队列、misfire 或并发控制。

| 职责 | 所有者 |
|------|--------|
| Cron 解析、触发时间、misfire、同一任务互斥、手动触发 | Quartz.NET |
| 任务目录、显示名称、默认计划、启停状态、执行历史 | JazorAdmin 数据模型 |
| 业务任务的依赖解析与执行结果 | ASP.NET Core DI 与 `IManagedTask` |
| 管理操作与展示 | JazorAdmin API 和 RazorVue 页面 |

任务目录是封闭集合。管理员只能调整已注册任务的 Cron 与状态，或发起一次人工运行；不能在后台提交脚本、类型名、程序集名或任意可执行负载。当前目录包含 `openid-prune`，按 14 天保留期清理 OpenIddict token，再清理失去关联的 authorization。

运行元数据由 `Schedule` 与 `ScheduleRun` 保存。`Schedule` 是管理页面的计划视图，`ScheduleRun` 用于展示最近执行结果，不替代 Quartz 的运行时触发状态。

## 验收基线

### SSO 中心

- `/sso/applications`、`/sso/scopes`、`/sso/authorizations`、`/sso/tokens` 可由平台管理员访问。
- 应用支持交互式、机器与 API 配置；Secret 仅在创建或轮换时返回一次。
- Scope 资源、授权撤销和令牌撤销均通过受保护 API 生效。

### 配置中心

- 平台管理员可创建、编辑和删除配置项。
- API 在写入前校验键、分组、标签、值长度与所选值类型。
- 配置项按分组与键稳定排序，JSON 保持作者输入的有效文本。

### 任务中心

- Quartz 接受合法 Cron，拒绝非法表达式。
- 禁用任务时不保留计划触发器；人工运行仍可用于运维验证。
- 同一任务不会并发执行，运行成功或失败均留下开始、结束、触发来源和结果。
- SQLite 默认部署下仍能按最近执行时间读取历史，不改变对外时间戳的 UTC offset 表达。

### 验证命令

```bash
dotnet test samples/JazorAdmin.Test/JazorAdmin.Test.csproj -c Release
dotnet run --no-launch-profile --file samples/JazorAdmin/verify-smoke.cs -- --configuration Release
```

集成测试覆盖平台管理员权限、配置类型校验与 CRUD、Quartz Cron 校验、暂停和人工触发。浏览器 smoke 验证本地资源包、生成 manifest、三中心导航、配置创建及任务执行历史。

## 后续扩展

新增运维任务时只增加明确的 `IManagedTask` 实现和目录记录，并补齐对应的权限、API、执行历史与 smoke 验证。不得把任务中心扩展为通用脚本运行器，也不得绕过 Quartz 自行实现调度语义。
