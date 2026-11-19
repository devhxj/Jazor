# 公共作者 API 与冻结基线

本文是当前公开作者面的人工可读索引，覆盖宿主、SSR、开发 reload 与重点绑定契约。正式冻结基线保存在机器快照中，当前完整签名和差异以实际发布候选程序集的 API 兼容性工具结果为准。

## 包与命名空间

| 包 | 稳定程序集/命名空间 |
| --- | --- |
| `Jazor` | `Jazor`, `Jazor.AspNetCore`, `Jazor.AspNetCore.Dev` |

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

- `JazorFrontendExtensions.AddJazorFrontend(WebApplicationBuilder, Action<JazorFrontendOptions>?)`
- `JazorFrontendExtensions.AddJazorFrontend(IServiceCollection, Action<JazorFrontendOptions>?)`
- `JazorFrontendExtensions.UseJazorPathBase(WebApplication)` / `UseJazorPathBase(IApplicationBuilder)`
- `JazorFrontendExtensions.UseJazorFrontend(WebApplication, Action<JazorHostOptions>?)` / `UseJazorFrontend(IApplicationBuilder, Action<JazorHostOptions>?)`
- `JazorFrontendOptions`
- `JazorViteServerOptions`
- `JazorFrontendUrls`
- `JazorReloadExtensions.AddJazorReload`
- `JazorReloadExtensions.UseJazorReload`
- `JazorReloadOptions`
- `JazorHmrMapping`

frontend API 在 Development 通过 NodeApi 管理 Vite 生命周期并代理 HTTP/HMR，其他环境托管 Release 产物。`UseJazorReload` 是独立的低层 reload/HMR 能力，在非 Development 环境保持 no-op。1.0.0 冻结快照仍保留为兼容性比较基线；Vite/SSR `TaskName` 删除按维护者指定的 1.1.0 工具链重构例外发布，迁移步骤见[发版规则](./release-and-versioning.md)。`public-api-reviewed-removals.snapshot.md` 只放行已确认签名，报告仍列出删除，旧冻结快照保持不变。

## ECMAScript 绑定命名

ECMAScript 浏览器绑定保留浏览器标准运行时名称；当标准名称与 C# 常用类型冲突时使用 `Ref` 后缀。涉及旧名称的迁移对照如下：

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

## 浏览器集合与组件绑定契约

浏览器标准接口仍由 WebIDL 生成；`extension` 只给已有 C# 类型追加宿主映射。`IWindow.LiveLocation` 是此类扩展，返回原生 `LocationRef`；`WindowRef.Location` 继续是 WebIDL 属性，`IWindow` / `WindowProxy` 的既有 `Location` 契约继续保留。

以下 14 个属性从错误的 `List<string>` 修正为 WebIDL `DOMTokenList`：

| 类型 | 属性 |
| --- | --- |
| `Element` | `ClassList`, `Part` |
| `HTMLAnchorElement`, `HTMLAreaElement`, `HTMLFormElement`, `SVGAElement` | `RelList` |
| `HTMLFencedFrameElement`, `HTMLIFrameElement` | `Sandbox` |
| `HTMLLinkElement` | `Blocking`, `RelList`, `Sizes` |
| `HTMLOutputElement` | `HtmlFor` |
| `HTMLScriptElement`, `HTMLStyleElement` | `Blocking` |

迁移时把显式 `List<string>` 变量改为 `DOMTokenList` 或 `var`，把 `.Count` 改为 `.Length`，按索引读取改为 `.GetItem(index)`；添加、删除和切换 token 使用 `.Add(...)`、`.Remove(...)`、`.Toggle(...)`。这些操作直接映射浏览器的 live token list，不再模拟 CLR 列表。

`Number` 新增到 `long` / `ulong` 的显式转换；它们进入 BigInt 值域，不能恢复源 Number 已丢失的整数精度。精确 ID 的 JSON 策略见 [Browser Interop](./browser-interop.md)。

## 数值与 union 作者面

嵌套 union 保留原分支路径，并增加由声明生成的直接标量投影。包含 `Number` 分支的 union 同时提供对应数值类型的强类型隐式转换，可在 C# 中直接传入整数变量。参数和回调提示来自实际声明，不增加 `object` 或开放泛型兜底。

Union 投影在不匹配时返回 null，并只求值一次。无法恢复精确标签的可赋值分支、无法区分的结构化对象分支明确失败；原始路由需要已解析路径时使用 `router.Resolve(raw).Path`。完整写法与限制见[类型与宿主标注](./type-semantics.md)及 [CHANGELOG](../../CHANGELOG.md)。

## 冻结规则

本清单的新增、删除或重命名同步更新 [公共 API 兼容性](./public-api-freeze.md)、测试和 `CHANGELOG.md`。发布候选从构建后的程序集生成机器可比较的签名快照，其差异作为目标版本的兼容性证据；机器兼容性检查提供正式比对结果。
