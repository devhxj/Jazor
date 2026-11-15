# 公共 API 兼容性与冻结基线

1.0.0 的公开作者 API 已冻结，机器基线保存在 `public-api-baseline.snapshot.md`。每次发布从实际 Release 程序集生成当前快照，与冻结基线比较；新增、删除和签名改变保留在报告中。不能把过去刷新候选基线后的“0 diff”描述为当前发布结论。

当前版本和升级入口见[安装与配置](./installation-and-configuration.md)。历史候选重置、预览版 API 数字与固定验证快照见[历史演进](../05-history/evolution.md)。基线文件不随普通文档整理改写。

## 稳定决策

- 包名保持现状，公开包 lockstep；核心、Vue opt-in、管理壳与生态 binding 分别承担各自契约。
- ASP.NET Core 生产宿主与开发宿主程序集继续随 `Jazor` 交付，不因模块分层增加宿主 NuGet 包。
- 扩展入口保留 PascalCase 的 `AddJazor*` / `UseJazor*` 和既有 `Ssr` 拼写；配置保持明确类型、只读集合和显式委托。
- 当前作者表面见[人工可读基线](./public-api-baseline.md)，权威比较范围和工具如下。历史签名及已批准移除仍由机器记录保存。
- 新增进入 MINOR，修复进入 PATCH，正式版后的破坏性改变进入 MAJOR；1.1.0/1.2.0 的单次例外见[版本规则](./release-and-versioning.md)。


## 包名与命名空间基线

| 包 | 稳定用途 | 公开命名空间 | 冻结意见 |
| --- | --- | --- | --- |
| `Jazor` | 编译器、Emit、MSBuild 与 ASP.NET Core 生产宿主集成 | `Jazor.*`、`Jazor.AspNetCore` | 保持 |
| `Jazor.Vue` | 官方 Razor Source Generator 到 Vue render-function 的 opt-in 集成 | `Jazor.RazorVue`、`ECMAScript.Vue*` | 保持 |
| `Jazor.AspNetCore.Dev`（`Jazor` 包内程序集） | 统一 frontend orchestration 与 Development reload/HMR | `Jazor.AspNetCore.Dev` | 不新增独立 NuGet 包 |
| `Jazor.Admin` | UI 库无关的管理壳与 RazorVue 组件 | `Jazor.Admin` | 保持 |
| `ECMAScript.*` | ECMAScript、Vue 生态与样式绑定 | 各包现有命名空间 | 按各自 binding 审查，不在本表重命名 |

包名、程序集名、RootNamespace 和 README 安装示例保持一致。未来拆分 `Jazor.AspNetCore` 包或改变资源 carrier 进入 MAJOR 版本通道。

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
| Frontend | `AddJazorFrontend`, `UseJazorPathBase`, `UseJazorFrontend`, `JazorFrontendUrls` | 统一项目目录、PathBase、Vite 启停/代理与 Release 托管；替代已删除的 Vite proxy API |
| Reload | `AddJazorReload`, `UseJazorReload` | 保持；生产环境必须 no-op，Development 才启用 transport |

统一 frontend 扩展同时支持 `WebApplicationBuilder` / `WebApplication` 与 `IServiceCollection` / `IApplicationBuilder`，让 Startup 或框架提供的 builder 直接完成注册和中间件组合；对应重载共享实现，并返回原 builder。底层 SSR/reload 扩展继续使用现有 ASP.NET Core 链式返回类型。空参数、无效路径、未注册服务和错误环境通过显式异常或 no-op 表达处理结果。

## 配置模型基线

以下类型和成员属于 1.0 配置契约：

- `JazorHostOptions`：`SecurityHeaders`、`Assets`。
- `JazorSecurityHeaderOptions`：安全响应头属性和 `AdditionalHeaders`。
- `JazorAssetOptions`：默认文件、web root、artifact graph、探针路径、缓存前缀和 artifact 配置委托。
- `JazorArtifactOptions`：`RequestPath`、`RootPath`、`DirectoryName`、探针集合、缓存前缀、响应回调和 miss 行为。
- `JazorSpaFallbackOptions`：排除前缀、允许后缀和 HTML Accept 约束。
- `JazorSsrOptions`：artifact root、request path、mount element 和 worker 数量。
- `JazorFrontendOptions`：request path、PathBase、项目根、Development/Release 入口和 `Vite` 子配置。
- `JazorViteServerOptions`：server origin、Vite API 配置、是否启动、启动与停止超时。
- `JazorReloadOptions`：client/WebSocket path、观察路径、HMR mapping、节流/轮询/心跳与 HTML 注入开关。

以上记录 1.0.0 的冻结表面。1.1.0 直接调用 Vite 与 SSR API，移除 Vite/SSR 的 `TaskName`；维护者指定该底层重构使用 MINOR，迁移步骤见 CHANGELOG。已审阅签名由 `public-api-reviewed-removals.snapshot.md` 显式列出，既有稳定 tag 和冻结快照保持不变。

1.2.0 类型语义调整将 `ErrorOptions`、`PropertyDescriptorRef` 和 WebIDL callback literal 从 class 迁移为 record，并清理许可用途的标注。该版本按维护者批准的 MINOR 例外交付；破坏性迁移说明与发布门禁仍适用，冻结快照继续报告差异。迁移路径见[类型指南](./type-semantics.md)、[版本规则例外](./release-and-versioning.md#120-类型语义调整例外)与 CHANGELOG。

当前候选的机器签名比较沿用既有已审阅删除记录后通过；该检查没有完整表达 class→record 的继承和对象模型变化，因此仍按上述破坏性迁移处理。

集合属性继续使用可变 `IList<T>` 是当前配置 authoring 约定；如果未来改为不可变或替换为 options binding，需要单独设计迁移路径。所有默认值必须在测试中断言，尤其是 `/jazor`、`entry.js`、`dist/bundle.js`、`http://127.0.0.1:5173`、`/@jazor/client`、`/@jazor/reload`、安全头、探针文件和缓存策略。

## SSR 数据模型

以下记录和接口属于跨请求/浏览器边界，必须按协议冻结：

- `IJazorSsrRenderer`
- `JazorSsrProvider`
- `JazorAuthenticationState`、`JazorAuthenticationStatus`、`JazorAuthenticationEnvelope`
- `JazorSsrRequest`、`JazorSsrStateEnvelope`、`JazorSsrRenderResult`

SSR envelope 的 schema/version、provider key、认证保留 key、错误传播、hydration 占位和资源闭包必须与 `jazor-ssr-state` v1 证据保持一致。任何字段删除、类型收窄或序列化名称改变都视为破坏性变更。

## 每次发布的兼容性验证

1. 在发布候选 ref 上生成当前 API 快照，与保留的正式冻结基线比较，覆盖上述两个 ASP.NET Core 命名空间、`Jazor.Admin` 和所有受审查 lockstep 作者程序集。
2. 对每个 `AddJazor*` / `UseJazor*` 入口保留至少一个源码消费者测试，并验证默认配置、链式返回值、异常/ no-op 行为。
3. 运行当前状态页列出的主线测试、编译器/RazorVue/Vue binding 覆盖率门禁，以及 Windows SPA/SSR 发布消费者门禁。
4. 检查 README、安装指南、示例、路线图和 CHANGELOG 使用的包名、命名空间、扩展方法拼写完全一致。
5. 在 CHANGELOG 的目标版本条目中记录变更与迁移；保留 API 差异、批准记录和门禁证据。
6. 所有候选检查通过后，创建目标版本 tag，由正式工作流重新验证并上传。

候选 ref 可通过手动 `Release Candidate Verification` workflow，或本地运行以下单一入口完成同一顺序的验收：

```bash
dotnet run --file scripts/csharp/verify-release-candidate.cs -- --tag v1.4.1
```

脚本会在 `artifacts/release-candidate/<tag>/` 归档每阶段日志、API 快照、兼容性报告、typed bootstrap 报告、包文件和最终 `report.md`；任一阶段失败都会以非零退出码结束。`--only build,public-api-compatibility` 这类按实际阶段名的筛选只适用于局部复核，正式候选必须运行完整序列。

发布候选和正式 tag workflow 均针对被测 ref 执行当前质量与消费者门禁。固定历史结果见[演进记录](../05-history/evolution.md)。

机器快照可在构建后生成：

```bash
dotnet run --file scripts/csharp/inspect-public-api.cs -- --output artifacts/api/public-api.md
```

发布候选应保存该文件，并与正式冻结基线进行稳定排序后的差异比较；不通过重置基线隐藏删除或签名变化。

机器比较使用：

```bash
dotnet run --file scripts/csharp/compare-public-api.cs -- --current artifacts/api/public-api.md --baseline docs/03-guides/public-api-baseline.snapshot.md --approved-removals docs/03-guides/public-api-reviewed-removals.snapshot.md --output artifacts/api/public-api-compatibility.md
```

首次建立候选快照时可以省略 `--baseline`，或显式传 `--allow-missing-baseline`，脚本会生成“baseline missing”报告；这不代表兼容性已经通过。CI 不传该选项，基线缺失会阻断门禁。存在基线时，新增 API 会列在报告中，未审阅的删除或签名变化会使命令失败。1.1.0 的 TaskName 删除通过显式 `--approved-removals docs/03-guides/public-api-reviewed-removals.snapshot.md` 放行，报告仍保留差异。快照覆盖宿主、开发 reload、Jazor.Admin、Jazor/Jazor.Vue 以及 P3-A/B/C、Monaco 和其他 ECMAScript 绑定程序集；仅作为包内实现载体的 Compiler、Common、Contract、RazorVue 和 Blazor 程序集不计入作者 API 冻结面。

## 变更规则

冻结后，新增 API 进入 MINOR，修复行为保持 PATCH；删除、重命名、签名改变、包/命名空间迁移和序列化协议改变进入 MAJOR。用户可见 API 变更同步更新文档、测试和 CHANGELOG。
