# 发版与版本规则

> 面向：Jazor 仓库维护者与贡献者。规则适用于所有 NuGet 包的版本决策、发布门禁与 CHANGELOG 记录。

## 版本号语义

所有 NuGet 包 lockstep 使用同一版本号。版本号的唯一来源是 `vMAJOR.MINOR.PATCH` 格式的 git tag，由发布工作流注入打包脚本；不在 `.csproj` 中手写版本，各项目中的 `0.1.0` 默认值仅是占位。版本通道的决策发生在打 tag 时，commit message 不承载版本决策。

| 通道 | 允许的内容 | 示例 |
| --- | --- | --- |
| `MAJOR` | 破坏公共契约的变更；`1.0.0` 之后唯一允许破坏的通道 | 公共 API 删除或重命名、无回退的产物布局迁移 |
| `MINOR` | 新能力、新的支持面、公共 API 新增；`0.x` 阶段可包含破坏性变更，但必须在 CHANGELOG 标注迁移说明 | 新的 lowering 支持、新绑定组件、新包 |
| `PATCH` | 仅限修复：错误行为纠正、诊断改进、文档、依赖缺陷修复 | 发射输出错误、精度回归、HMR 回归 |

`PATCH` 明确不包含：新的 lowering 能力、新的绑定组件、新的公共 API、API 重命名。这些在 `0.x` 阶段归 `MINOR`，`1.0.0` 之后归 `MAJOR`（新能力仍归 `MINOR`）。判定原则是：消费者能否仅凭版本号判断升级风险——新能力改变了"能做什么"，必须进入 `MINOR` 通道。

## 1.0.0 出场条件

当前质量门禁、发布消费者门禁与绑定审计已达到 `1.0` 级验收强度，`0.x` 对外传达的"早期、勿用于生产"信号与实际状态不符。满足以下条件后，下一个 minor 位置发布 `1.0.0`；需要预热时可先发 `-rc.N` 预发布：

1. 完成[1.0 公共 API 冻结审查](./public-api-freeze.md)：ASP.NET Core 扩展面（`AddJazor*` / `UseJazor*` 族）、包名、命名空间、公共配置模型命名、SSR 数据模型和开发期 reload 边界。计划中的重命名全部在 `1.0.0` 之前完成，避免发布后立即进入 `2.0.0`。
2. 当时的全部质量门禁通过，门槛以[当前状态](../04-roadmap/current-status.md)的门槛表为准。
3. CHANGELOG 完整覆盖全部已发布版本，且本规则已生效。

`1.0.0` 发布之后，通道语义不再有例外：`MINOR` 只做新增，`PATCH` 只做修复，一切破坏性变更走 `MAJOR`。本节在 `1.0.0` 发布后收敛删除。

## 发版节奏

- 按语义触发，不按日历：一个完整能力切片完成并通过聚焦回归后发布 `MINOR`；已发布版本发现缺陷时发布 `PATCH`。
- 不为凑节奏拆分版本，同一主题的多个小能力可以合并进同一个 `MINOR`；也不强制"每个工作日一版"。
- 不跳号。若发生手工重置或跳号（例如历史上的 `v0.1.48` → `v0.3.0`），必须在 CHANGELOG 中留一句解释。

## 发版门禁

打 tag 前按改动触及面执行对应门禁；覆盖率与场景数量的具体门槛以[当前状态](../04-roadmap/current-status.md)的门槛表为准：

| 改动触及面 | 本地门禁 |
| --- | --- |
| 所有发布 | `dotnet run --file scripts/csharp/test-dotnet.cs` |
| 编译器、CLR 白名单 | `dotnet run --file scripts/csharp/verify-compiler-coverage.cs` |
| RazorVue | `dotnet run --file scripts/csharp/verify-razorvue-coverage.cs` |
| Vue 生态绑定包 | `dotnet run --file scripts/csharp/verify-vue-binding-coverage.cs` |

相关 PR 与 main 分支变更由 `Quality Gates` 工作流自动执行三项覆盖率门禁。标签发布和 `workflow_dispatch` 手动发布均对指定发布 ref 执行同一工作流，三项全部成功后才进入打包流程；任一失败、取消或跳过都会阻止发布任务。覆盖率使用与本地默认命令一致的 Debug 配置；各门禁在独立 runner 上运行，失败时其他门禁仍继续采集证据。

每个门禁的 TRX、Cobertura（编译器与 RazorVue）、日志和 Markdown 摘要作为 Actions artifact 保留 14 天，关键指标同时写入 job summary。Vue 绑定指标是公共契约审计率，不是运行时代码覆盖率。本地使用上表的三个单文件 C# 命令复现；需要相同日志和摘要时，在仓库根目录运行 `dotnet run --file scripts/csharp/run-quality-gate.cs -- compiler`（或 `razorvue` / `vue-bindings`），证据写入 `artifacts/quality/`。CI 从工作流提交读取报告入口，从指定发布 ref 读取门禁脚本和被测源码，因此手动验证旧标签不会因缺少新报告入口而改变被测代码。

SPA 与 SSR 发布消费者门禁由发布工作流在上传 NuGet 之前自动执行，本地无需重复运行；工作流门禁失败时不产生公开包。

## CHANGELOG 规则

- 一个版本一个 `### Jazor x.y.z` 独立章节并标注日期；同一天发布多个版本也必须分节，不得把多个版本号混入同一日期段落。
- 条目面向用户描述行为变化，不写内部实现流水账；破坏性变更必须写明迁移路径（例如 `AddJazorSSR` → `AddJazorSsr` 一类重命名应指明旧名与新名）。
- 内容在发版准备时写入；已发布版本的章节不再改写，勘误以追加条目方式补充。
- 未发布内容放在最新日期下的 `### 未发布 | Unreleased` 小节；发布前整理为中英双语的 `New Features`、`Improvements`、`Bug Fixes`、`Chores` 分类，条目末尾标注 GitHub 贡献者（`by @user`）。
- 中英两部分描述同一组变更，保持条目顺序和分类一致；`scripts/csharp/release-notes.cs` 会直接提取对应版本章节作为 GitHub Release 正文。

具体模板、分类定义、署名要求和发布前检查清单见[发布说明规范](./release-notes-format.md)。

## 发布机制备忘

- 官方发布唯一入口是 `.github/workflows/nuget-publish-ref.yml`：push `v*` tag 或 `workflow_dispatch` 触发，GitHub Actions 持有 trusted publishing 凭据。tag 名去掉 `v` 前缀即为包版本。
- 本地 `scripts/csharp/publish-nuget.cs` 仅用于打包验证，必须携带 `--skip-push`。
- 不要求、不探测本地 `NUGET_API_KEY`。

## 相关入口

- 质量门槛与验证入口：[当前状态](../04-roadmap/current-status.md)
- 测试与覆盖率命令：[开发与测试](./development-and-testing.md)
- 版本演进记录：仓库根目录 `CHANGELOG.md`
