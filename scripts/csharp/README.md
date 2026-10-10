# C# 脚本

> 定位：仓库构建、验证和可复用诊断的单文件 C# 入口。

仓库自动化统一使用 `dotnet run --file` 运行本目录下的 C# 文件。需要反射、Roslyn、元数据检查或复杂参数编排时，应新增或扩展这里的脚本，而不是添加仓库自有的 PowerShell 脚本。

## 使用方式

在仓库根目录执行：

```bash
dotnet run --file scripts/csharp/<script-name>.cs -- [arguments]
```

脚本应保持确定性，并明确其输出目录、外部进程和是否会修改构建产物。一次性输入可置于 `.tmp/`；可复用的检查应保留在本目录。

## 主要入口

| 脚本 | 用途 |
| --- | --- |
| `test-dotnet.cs` | 构建一次并运行当前主测试 lane，支持 `--project <name>` 聚焦项目（例如 `dataui`、`lucide`） |
| `inspect-public-api.cs` | 从宿主、Jazor.Admin、Jazor/Jazor.Vue 和 ECMAScript 生态公开程序集生成稳定排序的 API 快照；可用 `--configuration` 指定构建位置 |
| `compare-public-api.cs` | 将当前公开 API 快照与候选基线逐行比较，并在报告中分别给出顶层声明与成员声明数量；缺失基线默认失败，只有本地首次建基线可显式传 `--allow-missing-baseline`，删除或签名变化时失败 |
| `verify-compiler-coverage.cs` | 执行编译器测试和正式覆盖率门槛 |
| `verify-razorvue-coverage.cs` | 执行 RazorVue 覆盖率门槛 |
| `verify-vue-binding-coverage.cs` | 审核 Vue 生态 binding 的公开契约覆盖 |
| `run-quality-gate.cs` | 执行单项覆盖率门禁；可用 `--output-directory` 将报告归档到发布候选目录 |
| `verify-vue-binding-contracts.cs` | 统一校验 Element Plus、Vuetify、TDesign 的生成快照、原始文档与资源 manifest；传入 `--report <path>` 会生成 schema `1.0` 的 JSON 报告及同名 Markdown 摘要，记录组件/export/prop/event/slot inventory 与 fingerprint；配合 `--baseline <report.json> --fail-on-baseline-drift` 可阻断检测到的 inventory 漂移，仓库基线位于 `docs/04-roadmap/binding-contract-baseline.json` |
| `verify-binding-documentation.cs` | 校验绑定库公开声明、枚举值、XML 输出、上游快照和 nuspec 文档文件；支持 `--configuration`、`--no-build`、`--baseline`、`--library` 与仓库内 `--output-directory` |
| `benchmark-razorvue-build.cs` | 测量 RazorVue clean、incremental、HMR 和 Release；保留原始日志，解析编译阶段，报告中位数、最大值和最慢样本；支持 `--diagnostics`、`--build-observations`，并统计模块/map/manifest/资源体积 |
| `write-toolchain-matrix.cs` | 记录当前 commit、global.json SDK、.NET CLI、Node.js、Chrome 和操作系统版本到 Markdown，供兼容矩阵与门禁日志引用 |
| `verify-development-hmr.cs` | 验证开发模式的 HMR artifact 和浏览器路径 |
| `wiki-import-docs.cs`、`wiki-build-local.cs`、`wiki-serve.cs`、`wiki-verify-*.cs`、`wiki-export-static.cs` | 导入 `docs/`、构建、预览、验证与静态导出 Jazor 官方网站；机器生成的 `*.snapshot.md` 默认只作为仓库证据，不进入浏览器目录 |
| `verify-windows-spa-release.cs` | 打包本地 NuGet 后，以隔离 Wiki 消费者完成 Windows Release publish 和 Chrome 浏览器验证 |
| `verify-windows-ssr-release.cs` | 打包本地 NuGet 后，以隔离 RazorVue TodoList 消费者完成 `JazorSSR=true` Release publish、Deno SSR HTML、发布目录资源解析与 Chrome hydration 交互验证 |
| `verify-typed-bootstrap.cs` | 启动应用自有 typed bootstrap endpoint，验证 GET、过期版本 409、校验失败 422、客户端草稿保留和成功提交；支持 `--report` 生成可归档证据；不注册 Jazor runtime 服务 |
| `inspect-razorvue-chain.cs` | 检查 `.razor` → Razor SG generated C# → render-function module → source map 链路；支持文本、JSON、版本化 remediation 报告（`--report`）和 IDE 可消费的 SARIF（`--sarif`）输出，并在断链时返回非零退出码 |
| `verify-razorvue-diagnostics.cs` | 在隔离 fixture 中持续验证链路检查器的成功报告、`JAZORVGA020`/`JAZORVGA026` 失败诊断、HelpLink、SARIF 和 remediation 协议，并归档摘要 |
| `generate-jazoradmin-brand-assets.cs` | 再生成或检查 JazorAdmin 本地品牌图标 |
| `publish-nuget.cs` | 本地打包 NuGet 验证；正式发布只走 tag 触发的 NuGet 工作流，本地必须 `--skip-push`；脚本不探测环境变量中的 `NUGET_API_KEY` |
| `prepare-local-candidate.cs` | 复用打包入口准备唯一版本的本地候选、隔离 bin/obj、本地源配置和包/源码身份报告；默认消费者六包，可指定包或 restore 已选同版本的消费者 |
| `release-notes.cs` | 为 tag 输出发布说明：优先取 CHANGELOG 对应版本章节，否则按 tag 区间提交生成 |
| `verify-release-notes.cs` | 在发布前确认目标 tag 存在带日期的 `CHANGELOG.md` 版本章节和非空双语说明 |
| `verify-release-candidate.cs` | 按 1.0 冻结规则串联 release notes、Release build、API 兼容性、覆盖率、主线测试、绑定 contract、绑定 XML 文档、RazorVue diagnostics、typed bootstrap、NuGet 包和 SPA/SSR consumer，并归档统一报告；支持 `--only` 局部复核 |

覆盖率脚本默认把 TRX、Cobertura 和同轮临时结果写入仓库根目录的
`test/coverage/<gate>/`；`test/` 是本地生成目录，不应把测试产物写到仓库外的盘符根目录。

具体参数和验证范围以脚本开头的说明及相应项目 README 为准。

## 相关文档

- [开发与测试](../../docs/03-guides/development-and-testing.md)
- [示例总览](../../docs/03-guides/examples.md)
