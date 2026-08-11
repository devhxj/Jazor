# JazorAdmin

> 定位：消费 `Jazor.Admin` 的管理应用示例与 Razor-to-Vue 集成验证项目。

JazorAdmin 展示应用如何在 `Jazor.Admin` 提供的公共容器契约上组合自己的导航、页面、TDesign 实现和业务能力。它是示例应用，不是 `Jazor.Admin` 库，也不把自身的认证、SSO、任务中心或部署配置提升为库契约。

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

## 验证

```bash
dotnet run --no-launch-profile --file samples/JazorAdmin/verify-smoke.cs -- --configuration Release
```

smoke 会基于当前仓库打包本地依赖，验证 Razor SG 模块、manifest、容器替换、路由与浏览器交互。生成的验证产物写入 `.tmp/sample-smoke/JazorAdmin/`。

品牌图标的再生成和一致性检查：

```bash
dotnet run --file scripts/csharp/generate-jazoradmin-brand-assets.cs -- --check
```

## 示例边界

- `Jazor.Admin` 提供应用框架和强类型模型；JazorAdmin 选择 TDesign、页面结构和领域功能。
- 应用在一个 ASP.NET Core host 中组合 RazorVue、Web API、Identity 和 OpenIddict。
- 应用样式由 `ECMAScript.Style` 和项目自身的 `ja-*` 命名空间管理，不依赖外部 CDN。

## 相关文档

- [Jazor.Admin](../../src/Jazor.Admin/README.md)
- [管理壳架构](../../docs/02-architecture/admin-shell.md)
- [示例总览](../../docs/03-guides/examples.md)
