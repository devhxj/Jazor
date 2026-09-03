# JazorAdmin

> 定位：生产级管理应用参考。面向中小型团队，可整体照抄的登录中心门户、资源管理与生产页面范式。

JazorAdmin 在一个 ASP.NET Core 宿主中组合 RazorVue UI、Web API、ASP.NET Core Identity、OpenIddict SSO、EF Core 与 Quartz，消费 `Jazor.Admin` 提供的管理壳契约。它的三个能力分区：

- **门户工作台**：登录（图形验证码）、锁屏、consent、审计驱动的总览指标，以及已注册下游应用入口。
- **身份与访问**：组织机构（结构/成员）、角色与资源操作授权、平台账号、SSO 中心（OpenIddict 应用/作用域/授权/令牌）。
- **平台运营**：强类型配置中心、Quartz 任务调度、页头通知中心、统一操作审计日志。

所有管理页共享统一页面范式（页头 → 筛选/工具条 → 内容 → 反馈）。应用边界见[管理壳架构](../../docs/02-architecture/admin-shell.md)，当前平台能力与后续优先级见[当前状态](../../docs/04-roadmap/current-status.md)和[下一阶段](../../docs/04-roadmap/next-development.md)。

边界规矩：平台 IAM 只管身份与平台资源；下游应用的业务权限归下游应用，只消费 OIDC claims，不注册为本应用的 resource operations。

## 运行

在仓库根目录执行：

```bash
dotnet run --project samples/JazorAdmin/JazorAdmin.csproj
```

访问启动日志输出的 `/login` 地址。空的开发数据存储会创建默认管理员：`admin@jazor.local` / `JazorAdmin123!`。此账户仅用于本地开发初始化；已有账户的密码不会被启动过程重置。

首次启动前可通过 user secrets 指定初始管理员：

```bash
dotnet user-secrets set --project samples/JazorAdmin/JazorAdmin.csproj "JazorAdmin:Bootstrap:Email" "admin@example.test"
dotnet user-secrets set --project samples/JazorAdmin/JazorAdmin.csproj "JazorAdmin:Bootstrap:Password" "ChangeThisAdmin123!"
dotnet user-secrets set --project samples/JazorAdmin/JazorAdmin.csproj "JazorAdmin:Bootstrap:DisplayName" "Platform Administrator"
```

部署时必须从部署环境的 secret store 提供初始管理员和 OpenIddict callback URL；不要使用开发默认值。

## 下游 OIDC 演示客户端

[`JazorAdmin.DemoClient`](../JazorAdmin.DemoClient/README.md) 是独立宿主的 confidential RazorVue 客户端。它使用授权码 + PKCE，调用 Bearer 保护的平台 API，并通过前端回调完成单点登出。管理员需要和下游应用配置同一个 `JazorAdmin:DemoClient:ClientSecret`；此值绝不能写入 `appsettings*.json` 或提交到仓库。

默认 HTTPS 开发端口为 JazorAdmin `49732` 和 DemoClient `49734`。完整 user-secrets 键、定制 redirect URI 与启动顺序见 DemoClient README；先启动 JazorAdmin，随后启动 DemoClient，并从 `https://localhost:49734` 发起登录。

## 验证

```bash
dotnet run --no-launch-profile --file samples/JazorAdmin/verify-smoke.cs -- --configuration Release
dotnet run --no-launch-profile --file samples/JazorAdmin.DemoClient/verify-smoke.cs -- --configuration Release
dotnet test samples/JazorAdmin.Test/JazorAdmin.Test.csproj
```

管理端 smoke 会基于当前仓库打包本地依赖，验证 Razor SG 模块、manifest、容器替换、分区导航、审计筛选与浏览器交互。DemoClient smoke 额外验证 CAPTCHA 登录、授权码 + PKCE、Bearer API、令牌审计和单点登出。生成的验证产物写入 `.tmp/sample-smoke/JazorAdmin/`。

## 当前 authoring 观察

账户管理页的创建账户和重置密码流程已直接使用 `TForm<T>`：页面状态分别由
`AccountDraft`/`PasswordDraft` 承载，字段使用 `@bind-Value`，规则使用
`TFormRules<T>`，提交与重置使用 typed `OnSubmit`/`OnReset`。服务端仍只接收现有的
`CreateAccountRequest`/密码 request，表单模型不会泄漏到 API 边界。

下一步实现要点：

- 账户、组织和 SSO 页面的大量表格操作列仍在 `.razor.cs` 中通过
  `RenderFragment`/`RenderTreeBuilder` 组合；先建立独立 typed column/slot fixture，再决定
  是否需要 binding API 调整。
- 其他页面仍有 `TForm<TJsonObject>` 加手写 `Value`/`OnChange` 状态；应逐页迁移并验证错误保留、
  异步提交和重置语义，不恢复通用 sample-local bridge。
- Microsoft `EditForm`、`InputBase<T>`、认证状态和 SSR form handoff 仍按作者指南保持
  Guidance/Reject，不以本页迁移扩大支持范围。

品牌图标的再生成和一致性检查：

```bash
dotnet run --file scripts/csharp/generate-jazoradmin-brand-assets.cs -- --check
```

## 示例边界

- `Jazor.Admin` 提供应用框架和强类型模型；JazorAdmin 选择 TDesign 组件、页面结构和领域功能。
- 管理页直接使用 `TTable<T>`、`TForm<T>`、`TInput<string>`、`TSwitch<bool>` 和 `TRadioGroup<string>` 等 typed TDesign 组件；账户页已采用 typed draft/bind/rules/submit/reset，不依赖 sample-local 控件桥接。
- 应用样式由 `ECMAScript.Style` 和项目自身的 `ja-*` 命名空间管理，不依赖外部 CDN。
- 上游 TDesign Starter 模板复刻页已退役；TDesign 绑定覆盖率由 binding-contract 审计门禁保证，不在本示例重复维护。

## 相关文档

- [当前状态](../../docs/04-roadmap/current-status.md)
- [下一阶段](../../docs/04-roadmap/next-development.md)
- [Jazor.Admin](../../src/Jazor.Admin/README.md)
- [管理壳架构](../../docs/02-architecture/admin-shell.md)
- [示例总览](../../docs/03-guides/examples.md)
