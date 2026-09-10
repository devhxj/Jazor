# 1.0 公共 API 基线清单

本文是冻结审查的人工可读基线。它覆盖 1.0 最容易被应用直接引用的宿主、SSR、开发 reload 和 `Jazor.Admin` 类型；编译器与生态绑定的完整签名仍以发布候选程序集的 API 兼容性工具结果为准。机器快照会排除 Razor 生成的下划线前缀辅助类型，例如 `_Imports`。

## 包与命名空间

| 包 | 稳定程序集/命名空间 |
| --- | --- |
| `Jazor` | `Jazor`, `Jazor.AspNetCore` |
| `Jazor.Vue` | `Jazor.RazorVue`, `ECMAScript.Vue*` |
| `Jazor.Admin` | `Jazor.Admin` |

## `Jazor.AspNetCore`

### 扩展入口

- `JazorWebApplication.CreateBuilder(string[] args, CallerFilePath)`
- `JazorExtensions.UseJazorHost`
- `JazorExtensions.UseJazorSecurityHeaders`
- `JazorExtensions.UseJazorStaticFiles`
- `JazorExtensions.UseJazorArtifacts`
- `JazorExtensions.UseJazorAssets`
- `JazorExtensions.UseJazorSpaFallback`
- `JazorSsrExtensions.AddJazorSsr`
- `JazorSsrExtensions.UseJazorSsr`

### 配置与协议类型

- `JazorHostOptions`
- `JazorSecurityHeaderOptions`
- `JazorAssetOptions`
- `JazorArtifactOptions`
- `JazorSpaFallbackOptions`
- `JazorSsrOptions`
- `IJazorSsrRenderer`
- `JazorSsrProvider`
- `JazorAuthenticationState`
- `JazorAuthenticationStatus`
- `JazorAuthenticationEnvelope`
- `JazorSsrRequest`
- `JazorSsrStateEnvelope`
- `JazorSsrRenderResult`

## `Jazor.AspNetCore.Dev`

- `JazorReloadExtensions.AddJazorReload`
- `JazorReloadExtensions.UseJazorReload`
- `JazorReloadOptions`
- `JazorHmrMapping`

该命名空间只用于 Development reload/HMR。生产应用不应依赖其运行时服务，`UseJazorReload` 在非 Development 环境保持 no-op。

## `Jazor.Admin`

- `AdminRouteDefinition`、`AdminRouteCatalog`
- `AdminLayoutState`
- `AdminLayoutMode`、`AdminThemeMode`
- `AdminPageActionKind`、`AdminPageAction`
- `AdminBreadcrumbItem`
- `AdminNavItem`、`AdminNavItemsCollectionBuilder`

`Jazor.Admin` 的页面、表单、表格和认证业务模型仍属于应用层，不进入公共壳 API 基线。

## ECMAScript 绑定命名

ECMAScript 浏览器绑定保留浏览器标准运行时名称；当标准名称与 C# 常用类型冲突时使用 `Ref` 后缀。当前冻结前迁移表如下：

| 旧名称 | 新名称 |
| --- | --- |
| `JazorFile` | `FileRef` |
| `JazorDocument` | `DocumentRef` |
| `JazorWindow` | `WindowRef` |
| `JazorHistory` | `HistoryRef` |
| `JazorEvent` | `EventRef` |
| `JazorLocation` | `LocationRef` |
| `JazorPropertyKey` | `PropertyKeyRef` |
| `JazorPropertyDescriptor` | `PropertyDescriptorRef` |

这些名称只影响 C# 作者侧 API；生成的 JavaScript/WebIDL ABI 名称保持浏览器标准名称不变。

## 冻结规则

本清单的新增、删除或重命名必须同步更新 [1.0 公共 API 冻结审查](./public-api-freeze.md)、测试和 `CHANGELOG.md`。发布候选时应从构建后的程序集生成机器可比较的签名快照，并将差异作为 1.0 放行证据；本文件不能替代机器兼容性检查。
