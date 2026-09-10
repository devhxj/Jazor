# 1.0 公共 API 冻结审查

> 状态：冻结前审查已建立，尚未授权发布 `1.0.0`。本文是 1.0 发布前的契约基线；任何新增、删除、重命名或签名改变都必须先更新本文、测试和 CHANGELOG。

## 审查结论

当前 API 可以进入冻结候选阶段，暂不需要为了形式统一而重命名现有公开类型或扩展方法。公开面清单见[1.0 公共 API 基线清单](./public-api-baseline.md)。冻结前仍必须完成一次针对发布 ref 的机器 API 兼容性检查，并以全部质量门禁和发布消费者门禁的成功结果作为最终放行条件。

本次审查采用以下稳定决策：

- NuGet 包名保持现状，所有发布包继续 lockstep 版本；`Jazor` 是核心宿主包，`Jazor.Vue` 是 Razor-to-Vue opt-in 包，`Jazor.Admin` 是管理壳包。
- ASP.NET Core 公共命名空间保持 `Jazor.AspNetCore`；开发期 reload 公共命名空间保持 `Jazor.AspNetCore.Dev`。不把开发期 API 混入生产宿主命名空间。
- 扩展方法保留 PascalCase 的 `AddJazor*` / `UseJazor*` 形式，缩写按现有语义固定为 `Ssr`，不再引入 `SSR` 别名。迁移文档不再假设存在 `AddJazorSSR`。
- 配置模型保持 `Jazor*Options` 命名，并以只读集合、强类型路径和显式委托表达扩展点；不新增 `object` 或字符串字典式总配置入口。
- `JazorWebApplication.CreateBuilder`、`IJazorSsrRenderer`、SSR 请求/结果记录类型属于宿主集成契约，必须纳入 API 兼容性检查，而不是视为内部实现。

## 包名与命名空间基线

| 包 | 稳定用途 | 公开命名空间 | 冻结意见 |
| --- | --- | --- | --- |
| `Jazor` | 编译器、Emit、MSBuild 与 ASP.NET Core 生产宿主集成 | `Jazor.*`、`Jazor.AspNetCore` | 保持 |
| `Jazor.Vue` | 官方 Razor Source Generator 到 Vue render-function 的 opt-in 集成 | `Jazor.RazorVue`、`ECMAScript.Vue*` | 保持 |
| `Jazor.AspNetCore.Dev`（随核心发行资产提供） | Development 环境 reload/HMR | `Jazor.AspNetCore.Dev` | 保持独立命名空间 |
| `Jazor.Admin` | UI 库无关的管理壳与 RazorVue 组件 | `Jazor.Admin` | 保持 |
| `ECMAScript.*` | ECMAScript、Vue 生态与样式绑定 | 各包现有命名空间 | 按各自 binding 审查，不在本表重命名 |

包名、程序集名、RootNamespace 和 README 安装示例必须保持一致。任何未来拆分 `Jazor.AspNetCore` 包或改变资源 carrier 都属于破坏性变更，不能作为 1.0 后的 PATCH/MINOR 顺手处理。

## ASP.NET Core 扩展面

生产宿主扩展位于 `Jazor.AspNetCore`：

| API 家族 | 当前公开入口 | 1.0 决策 |
| --- | --- | --- |
| Builder | `JazorWebApplication.CreateBuilder(string[] args, CallerFilePath)` | 保持；`CallerFilePath` 是 source/publish content-root 解析契约 |
| Host | `UseJazorHost`, `UseJazorSecurityHeaders`, `UseJazorAssets` | 保持；组合顺序和默认值属于行为契约 |
| Assets | `UseJazorStaticFiles`, `UseJazorArtifacts`, `UseJazorSpaFallback` | 保持；404、PathBase、Accept 头和缓存行为须继续由宿主测试锁定 |
| SSR | `AddJazorSsr`, `UseJazorSsr` | 保持 `Ssr` 拼写；不提供大小写别名 |

开发宿主扩展位于 `Jazor.AspNetCore.Dev`：

| API 家族 | 当前公开入口 | 1.0 决策 |
| --- | --- | --- |
| Reload | `AddJazorReload`, `UseJazorReload` | 保持；生产环境必须 no-op，Development 才启用 transport |

扩展方法的返回类型继续使用 ASP.NET Core 的 `IServiceCollection` / `IApplicationBuilder`，以支持标准链式启动代码。空参数、无效路径、未注册服务和错误环境的行为必须保持显式异常或 no-op，不能增加静默 fallback。

## 配置模型基线

以下类型和成员属于 1.0 配置契约：

- `JazorHostOptions`：`SecurityHeaders`、`Assets`。
- `JazorSecurityHeaderOptions`：安全响应头属性和 `AdditionalHeaders`。
- `JazorAssetOptions`：默认文件、web root、artifact graph、探针路径、缓存前缀和 artifact 配置委托。
- `JazorArtifactOptions`：`RequestPath`、`RootPath`、`DirectoryName`、探针集合、缓存前缀、响应回调和 miss 行为。
- `JazorSpaFallbackOptions`：排除前缀、允许后缀和 HTML Accept 约束。
- `JazorSsrOptions`：artifact root、request path、mount element 和 worker 数量。
- `JazorReloadOptions`：client/WebSocket path、观察路径、HMR mapping、节流/轮询/心跳与 HTML 注入开关。

集合属性继续使用可变 `IList<T>` 是当前配置 authoring 约定；如果未来改为不可变或替换为 options binding，需要单独设计迁移路径。所有默认值必须在测试中断言，尤其是 `/jazor`、`/@jazor/client`、`/@jazor/reload`、安全头、探针文件和缓存策略。

## SSR 数据模型

以下记录和接口属于跨请求/浏览器边界，必须按协议冻结：

- `IJazorSsrRenderer`
- `JazorSsrProvider`
- `JazorAuthenticationState`、`JazorAuthenticationStatus`、`JazorAuthenticationEnvelope`
- `JazorSsrRequest`、`JazorSsrStateEnvelope`、`JazorSsrRenderResult`

SSR envelope 的 schema/version、provider key、认证保留 key、错误传播、hydration 占位和资源闭包必须与 `jazor-ssr-state` v1 证据保持一致。任何字段删除、类型收窄或序列化名称改变都视为破坏性变更。

## 冻结前必做清单

1. 在发布候选 ref 上生成公共 API 基线，覆盖上述两个 ASP.NET Core 命名空间、`Jazor.Admin` 和所有 lockstep 包的新增/删除/签名变更。
2. 对每个 `AddJazor*` / `UseJazor*` 入口保留至少一个源码消费者测试，并验证默认配置、链式返回值、异常/ no-op 行为。
3. 运行当前状态页列出的主线测试、编译器/RazorVue/Vue binding 覆盖率门禁，以及 Windows SPA/SSR 发布消费者门禁。
4. 检查 README、安装指南、示例、路线图和 CHANGELOG 使用的包名、命名空间、扩展方法拼写完全一致。
5. 在 CHANGELOG 的 `1.0.0-rc.1` 或 `1.0.0` 条目中写明冻结日期、迁移说明（如无迁移则明确写“无已知迁移”）和全部门禁链接。
6. 只有以上证据全部成功，才把本文状态改为“已冻结”，再决定是否创建 `v1.0.0` tag。

当前验证记录：快速 Emit 套件 `154/154` 通过；`emit-consumer` 消费者矩阵 `47/47` 通过。两者必须在发布候选 ref 上分别执行，不能只运行快速 lane。

机器快照可在构建后生成：

```bash
dotnet run --file scripts/csharp/inspect-public-api.cs -- --output artifacts/api/public-api.md
```

发布候选应保存该文件，并与上一候选快照进行稳定排序后的差异比较。

## 变更规则

冻结后，新增 API 进入 MINOR，修复行为保持 PATCH；删除、重命名、签名改变、包/命名空间迁移和序列化协议改变进入 MAJOR。文档修订不能掩盖 API 变更，所有用户可见契约必须同时更新测试和 CHANGELOG。
