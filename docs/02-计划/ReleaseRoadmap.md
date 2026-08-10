# Jazor 发布路线图

> 状态：当前主线规划。最后确认：2026-08-10。

本文定义 Jazor 从现有生产技术路线基线到正式 `1.0` 的版本门槛。版本号代表可复核的产品成熟度，不按零散功能或单次修复递增。

## 版本阶段

| 版本 | 核心门槛 | HMR、调试与性能工作 |
| --- | --- | --- |
| `0.3` | 当前 RazorVue 生产技术路线基线：官方 Razor SG -> Roslyn `IOperation` -> Vue render-function `.mjs`、包消费、Emit/MSBuild、WebIDL 和默认导入主线可用。 | 尚不以完整 HMR、调试或性能收口作为发布门槛。 |
| `0.4` | RazorVue 分支覆盖率达到 `90%`。 | 保持现有架构预留，不宣称运行时 HMR 已完成。 |
| `0.5` | RazorVue 分支覆盖率达到 `96%`。 | 保持可追溯的 SourceMap、组件身份和变更分类契约。 |
| `0.6` | 其他 Vue 封装的逐项公开绑定契约审计覆盖率达到 `80%`，且对应测试 lane 全部通过。 | 开始 HMR、调试和性能调优实施，不把“开始”误写成完成。 |
| `0.7` | 其他 Vue 封装的逐项公开绑定契约审计覆盖率达到 `90%`。 | 已验证由编译器证明的仅模板更新、显式处理器和完整刷新回退；性能仍以带警告的可复现基线记录。 |
| `0.8` | 其他 Vue 封装的逐项公开绑定契约审计覆盖率达到 `96%`。 | 收敛 HMR、调试和性能遗留问题，为应用验证提供稳定开发链路。 |
| `0.9` | JazorAdmin 初版完成：同一 ASP.NET Core 宿主承载 RazorVue 前端、Web API、OpenIddict SSO、组织机构、账户、资源/操作授权和配置中心，并完成集成与浏览器验证。 | JazorAdmin 纳入 HMR、调试和性能验证范围。 |
| `1.0` | Jazor 与 JazorAdmin 均彻底完成。 | 性能调优不存在已知发布阻塞，HMR 与调试链路从源码变更到浏览器诊断的实际工作流全部闭合并有自动化证据。 |

## 口径与规则

- `0.4` 与 `0.5` 的“覆盖率”明确指 RazorVue 分支覆盖率；不得以行覆盖率或历史快照替代。
- `0.6` 至 `0.8` 的 Vue 封装覆盖要求按受支持的独立封装项目逐项验收，不以多个项目的加权平均掩盖短板。每个版本开始前，应在对应测试门禁中明确具体度量口径。
- `0.6` 的门禁由 `scripts/csharp/verify-vue-binding-coverage.cs` 固化：对 Vue3、Vuetify、Element Plus、TDesign、Pinia、Pinia Testing 和 Vue Router 分别枚举公开类型、构造函数、方法、属性、字段与事件，审计可寻址的签名及绑定元数据；每个 test lane 还必须达到其最低测试数且无失败。该口径不把 `extern`/元数据封装没有可执行 IL 的 Coverlet 行覆盖误当成测试覆盖率。
- HMR、调试和性能工作从 `0.6` 开始进入主动实施。`0.6` 到 `0.9` 的阶段性结果只能证明已覆盖的场景；只有 `1.0` 可以声明开发体验与性能路线完成。
- `1.0` 的性能结论必须来自可复现的基准、回归阈值和真实应用验证，而不是主观的“感觉足够快”。
- `0.3` 至 `0.8` 的 JazorAdmin 开发、集成测试和 smoke 必须消费当前源码打出的本地 NuGet 包，不能用远程包替代当前源码验证。`0.8` 完成后，从 `0.9` 开始再把公开 NuGet 源消费纳入正式验收。
- `0.1.x` 是路线确立前的历史补丁线。后续公开里程碑从 `0.3.0` 开始，`0.2` 不单独占用版本阶段。
- 正式 NuGet 上传只通过 `.github/workflows/nuget-publish-ref.yml` 执行：推送 `v*` 标签或手动 `workflow_dispatch` 会在 GitHub Actions 中打包、上传 nuget.org / GitHub Packages 并创建或更新 GitHub Release。本地 `scripts/csharp/publish-nuget.cs` 仅用于 `--skip-push` 打包验证，不应把本机 `NUGET_API_KEY` 作为发布前提。

## 当前定位

当前已验证的 Compiler 基线为 `10318/10318` 个场景通过、`98.91%` 行覆盖率和 `96.01%` 分支覆盖率，不再作为后续发布版本的独立门槛。RazorVue `0.5` 门禁已于 2026-08-10 重新验证：`4684/4684` 个官方 Razor SG 场景通过，行覆盖率为 `97.57%`，分支覆盖率为 `96.00%`。

`0.6` 门禁也已于 2026-08-10 重新验证：`verify-vue-binding-coverage.cs` 中的 Compiler、Pinia、Pinia Testing 和 Vue Router test lane 分别为 `10318/10318`、`68/68`、`39/39`、`102/102` 通过；Vue3 `1304/1304`、Vuetify `5076/5076`、Element Plus `3611/3611`、TDesign `13251/13251`、Pinia `314/314`、Pinia Testing `65/65`、Vue Router `766/766` 个公开绑定契约单位均完成审计。精确发布提交还在 clean worktree 中以 `publish-nuget.cs -- --skip-push --package-version 0.6.0` 成功打包十个默认公开包。`.mjs` 文件变更现可向声明 `module-update` capability 的开发客户端发送可取消事件，其他客户端和其他变更仍使用完整刷新。RazorVue G2 首份可复现报告已记录官方 Razor SG 外部包消费者、生成物、Node 和浏览器数据；其中性能阈值警告和旧线同协议缺失仍是后续阶段工作，不构成“性能已经完成”的声明。

`0.7` 门禁已于 2026-08-10 重新验证：上述同一组 test lane 全部通过，且 Vue3 `1304/1304`、Vuetify `5076/5076`、Element Plus `3611/3611`、TDesign `13251/13251`、Pinia `314/314`、Pinia Testing `65/65`、Vue Router `766/766` 个公开绑定契约单位均为 `100%`，高于逐项 `90%` 门槛。开发宿主会比较相邻 manifest：仅当 `ComponentId`/`ModuleId`、描述符和逻辑哈希保持不变且模板哈希变化时，才发送 `module-update`；浏览器必须通过 `JazorHmr.accept(moduleId, handler)` 显式接管动态导入后的模块，未注册处理器、导入失败或所有其他变更均完整刷新。`verify-development-hmr.cs` 已在真实浏览器中验证该流程；它不自动替换 Vue 实例，也不声明状态保留。G2 的发布报告仍为 `baseline-recorded-with-warnings`，生成物 gzip 比率、Node 比率和旧线同协议缺失保持为后续性能工作。主线下一门槛为 `0.8` 的逐项 `96%` Vue 封装契约审计覆盖率。

`0.8` 门禁已于 2026-08-10 重新验证：`verify-vue-binding-coverage.cs` 的 Compiler、Pinia、Pinia Testing 和 Vue Router test lane 分别为 `10318/10318`、`68/68`、`39/39`、`102/102` 通过；Vue3 `1307/1307`、Vuetify `5076/5076`、Element Plus `3611/3611`、TDesign `13251/13251`、Pinia `314/314`、Pinia Testing `65/65`、Vue Router `766/766` 个公开绑定契约单位均为 `100%`，高于逐项 `96%` 门槛。完整 `test-dotnet.cs` 还验证了 CLR、Style、Pinia、Pinia Testing、Vue Router、RazorVue、Emit 与 render-context lane 均无失败；`publish-nuget.cs --skip-push --package-version 0.8.0` 成功打包十个默认公开包。`0.8` 由此达到既定的独立 Vue binding 契约门槛；HMR、调试与性能的已知后续工作仍按路线图继续收口。

达到任一版本门槛前，必须重新运行对应的当前测试、覆盖率报告、包消费验证和必要的浏览器验证；历史状态文档中的通过记录不能替代本次证据。
