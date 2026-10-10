# 1.0 公共 API 冻结审查

> 当前预览版本：[1.0.0-preview.9（2026-10-10）](https://github.com/devhxj/Jazor/releases/tag/v1.0.0-preview.9)。下文 preview.1 指首次冻结候选里程碑；版本变化见 [CHANGELOG](../../CHANGELOG.md)。

> preview.9 包含 `ElStringDatePicker`、`JazorFrontendUrls.GetStylesheets`、Pagination 的两个 `EventCallback<Number>`，以及数值转换和直接 union 标量投影。机器 snapshot 按本次预览 API 更新；回调与投影的迁移说明见 [CHANGELOG](../../CHANGELOG.md)。

> 状态：正式 `1.0` 尚未冻结。preview.6 已完成统一 frontend 候选契约重置；preview.8 的候选快照已按开发者反馈审查新增绑定、原生文件事件 extension 与 `DOMTokenList` 签名修正。preview.8 的 Release 构建、API 比较、完整质量与本地包发布消费者门禁已通过；正式 1.0 冻结仍按下文清单执行。

## 审查结论

frontend 宿主面在 preview.6 按新的单一契约重新收敛；preview.8 增补 Startup/framework 宿主重载、Element Plus 强类型交互和浏览器绑定修正，preview.9 补齐数值与 union 作者面，公开面与迁移说明见[1.0 公共 API 基线清单](./public-api-baseline.md)。机器候选快照保存在 `public-api-baseline.snapshot.md`；正式 1.0 前仍需在候选 ref 上重新生成并通过兼容性检查，以及当前声明范围内的质量门禁和发布消费者门禁，才能最终冻结。

本次审查采用以下稳定决策：

preview.5 在 2026-09-28 发布时的机器快照为 `76105` 条（`8948` 个顶层声明、`67157` 个成员声明），当时比较新增 `0`、删除 `0`。该结果属于已发布候选的历史证据；当前 frontend API 的删除和新增是明确批准的候选重置。重置后的基线为 `76133` 条（`8950` 个顶层声明、`67183` 个成员声明），对该新基线重新生成的比较报告新增 `0`、删除 `0`。迁移说明和消费者门禁仍必须随下一候选一并保留。

2026-10-09 的 preview.8 API 审查最终基线为 `76564` 条（`9017` 个顶层声明、`67547` 个成员声明），相对原 `76133` 条基线新增 `445` 个去重签名、改变 `14` 个去重签名。新增面来自 Element Plus typed variants/services、Number 转换、IWindow.LiveLocation、三个 frontend 宿主重载和既有 CLR 事件/载荷 extension；14 个改变签名为 WebIDL token 属性修正为 `DOMTokenList`。`DOMTokenList` 和 `ElTableColumn.ChildContent` scoped slot 的迁移已写入基线清单与 CHANGELOG。最终 Release 程序集比较为 `76564` 对 `76564`、新增 `0`、删除 `0`，完整质量与 23 包的 SPA/SSR 消费者门禁通过。

preview.9 的机器候选快照按 Release 程序集刷新；本轮相对已审查的 frontend 候选基线只新增强类型数值隐式转换，没有删除既有签名。直接标量投影保留旧路径，投影不匹配返回 null；可赋值重叠或无法区分的结构化对象分支需要准确的显式宿主映射。API 计数与本地门禁快照保留在[演进记录](../05-history/evolution.md#2026-10-10-preview9-作者体验与发布准备)，正式发布 ref 继续由 tag workflow 验证。

文件事件 extension 随 `Jazor.Vue` 的 `ECMAScript.Vue` 程序集交付，属于既有 C# 事件类型的额外宿主映射；`FileList` / `FileRef` 保持 WebIDL 生成。对应 CLR adapter 是编译映射实现，不增加独立公共作者 API。

同一隔离构建通过 Element Plus、Vuetify 与 TDesign 的五项生成检查；三套 upstream inventory 与 `binding-contract-baseline.json` 的 fingerprint 均保持一致，因此未刷新绑定 inventory 基线。作者侧的 typed projections/services 由公共 API 快照记录，不作为上游组件 inventory 新增项。

- NuGet 包名保持现状，所有发布包继续 lockstep 版本；`Jazor` 是核心宿主包，`Jazor.Vue` 是 Razor-to-Vue opt-in 包，`Jazor.Admin` 是管理壳包。
- ASP.NET Core 底层静态资源与 SSR 位于 `Jazor.AspNetCore`；统一 frontend orchestration 与 reload 位于 `Jazor.AspNetCore.Dev`。两套程序集继续随同一个 `Jazor` 包交付，不新增宿主 NuGet 包。
- 扩展方法采用 PascalCase 的 `AddJazor*` / `UseJazor*` 形式，缩写按现有语义固定为 `Ssr`；迁移文档使用 `AddJazorSsr`。
- 配置模型保持 `Jazor*Options` 命名，并以只读集合、强类型路径和显式委托表达扩展点；不新增 `object` 或字符串字典式总配置入口。
- `JazorWebApplication.CreateBuilder`、`IJazorSsrRenderer`、SSR 请求/结果记录类型属于宿主集成契约，必须纳入 API 兼容性检查，不视为内部实现。

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
- `JazorViteServerOptions`：server origin、Deno task、是否启动、启动与停止超时。
- `JazorReloadOptions`：client/WebSocket path、观察路径、HMR mapping、节流/轮询/心跳与 HTML 注入开关。

集合属性继续使用可变 `IList<T>` 是当前配置 authoring 约定；如果未来改为不可变或替换为 options binding，需要单独设计迁移路径。所有默认值必须在测试中断言，尤其是 `/jazor`、`entry.js`、`dist/bundle.js`、`http://127.0.0.1:5173`、`/@jazor/client`、`/@jazor/reload`、安全头、探针文件和缓存策略。

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
5. 在 CHANGELOG 的 `1.0.0` 条目中写明冻结日期、迁移说明（如无迁移则明确写“无已知迁移”）和全部门禁链接。
6. 只有以上证据全部成功，本文状态才改为“已冻结”，再决定是否创建 `v1.0.0` tag。

候选 ref 可通过手动 `Release Candidate Verification` workflow，或本地运行以下单一入口完成同一顺序的验收：

```bash
dotnet run --file scripts/csharp/verify-release-candidate.cs -- --tag v1.0.0-preview.9
```

脚本会在 `artifacts/release-candidate/<tag>/` 归档每阶段日志、API 快照、兼容性报告、typed bootstrap 报告、包文件和最终 `report.md`；任一阶段失败都会以非零退出码结束。`--only build,public-api-compatibility` 这类按实际阶段名的筛选只适用于局部复核，正式候选必须运行完整序列。

preview.8 本地完整主线验证包括 Compiler `10728/10728`、CLR `5092/5092`、RazorVue SG `5035/5035`、Emit `221/221`，覆盖率、绑定文档与 23 包的 SPA/SSR 消费者矩阵均已通过；复现入口和范围见[反馈验收](../04-roadmap/preview8-developer-feedback.md)。tag workflow 在上传 NuGet 前会针对发布 ref 再次执行门禁。

机器快照可在构建后生成：

```bash
dotnet run --file scripts/csharp/inspect-public-api.cs -- --output artifacts/api/public-api.md
```

发布候选应保存该文件，并与上一候选快照进行稳定排序后的差异比较。

机器比较使用：

```bash
dotnet run --file scripts/csharp/compare-public-api.cs -- --current artifacts/api/public-api.md --baseline artifacts/api/public-api-baseline.md --output artifacts/api/public-api-compatibility.md
```

首次建立候选快照时可以省略 `--baseline`，或显式传 `--allow-missing-baseline`，脚本会生成“baseline missing”报告；这不代表兼容性已经通过。CI 不传该选项，基线缺失会阻断门禁。存在基线时，新增 API 会列在报告中，删除或签名变化会使命令失败。快照覆盖宿主、开发 reload、Jazor.Admin、Jazor/Jazor.Vue 以及 P3-A/B/C、Monaco 和其他 ECMAScript 绑定程序集；仅作为包内实现载体的 Compiler、Common、Contract、RazorVue 和 Blazor 程序集不计入作者 API 冻结面。

## 变更规则

冻结后，新增 API 进入 MINOR，修复行为保持 PATCH；删除、重命名、签名改变、包/命名空间迁移和序列化协议改变进入 MAJOR。用户可见 API 变更同步更新文档、测试和 CHANGELOG。
