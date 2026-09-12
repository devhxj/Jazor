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
| `test-dotnet.cs` | 构建一次并运行当前主测试 lane，支持 `--project <name>` 聚焦项目（例如 `dataui`、`vu-icons`） |
| `inspect-public-api.cs` | 从宿主、Jazor.Admin、Jazor/Jazor.Vue 和 ECMAScript 生态公开程序集生成稳定排序的 API 快照；可用 `--configuration` 指定构建位置 |
| `compare-public-api.cs` | 将当前公开 API 快照与候选基线逐行比较；缺失基线默认失败，只有本地首次建基线可显式传 `--allow-missing-baseline`，删除或签名变化时失败 |
| `verify-compiler-coverage.cs` | 执行编译器测试和正式覆盖率门槛 |
| `verify-razorvue-coverage.cs` | 执行 RazorVue 覆盖率门槛 |
| `verify-vue-binding-coverage.cs` | 审核 Vue 生态 binding 的公开契约覆盖 |
| `run-quality-gate.cs` | 执行单项覆盖率门禁；可用 `--output-directory` 将报告归档到发布候选目录 |
| `verify-vue-binding-contracts.cs` | 统一校验 Element Plus、Vuetify、TDesign 的生成快照、原始文档与资源 manifest |
| `benchmark-razorvue-build.cs` | 测量 RazorVue clean、incremental、HMR 和 Release 构建时间；clean/incremental 还在 JSON/Markdown 基线中记录生成模块与 `.mjs`/source map 数量、原始与逐文件 gzip 字节数、manifest/完整 Emit 输出体积及增量产物内容变化；不把产物未变化解释为内部缓存命中 |
| `write-toolchain-matrix.cs` | 记录当前 commit、global.json SDK、.NET CLI、Node.js、Chrome 和操作系统版本到 Markdown，供兼容矩阵与门禁日志引用 |
| `verify-development-hmr.cs` | 验证开发模式的 HMR artifact 和浏览器路径 |
| `wiki-import-docs.cs`、`wiki-build-local.cs`、`wiki-serve.cs`、`wiki-verify-*.cs`、`wiki-export-static.cs` | 导入 `docs/`、构建、预览、验证与静态导出 Jazor 官方网站 |
| `verify-windows-spa-release.cs` | 打包本地 NuGet 后，以隔离 Wiki 消费者完成 Windows Release publish 和 Chrome 浏览器验证 |
| `verify-windows-ssr-release.cs` | 打包本地 NuGet 后，以隔离 RazorVue TodoList 消费者完成 `JazorSSR=true` Release publish、Deno SSR HTML、发布目录资源解析与 Chrome hydration 交互验证 |
| `verify-typed-bootstrap.cs` | 启动应用自有 typed bootstrap endpoint，验证 GET、过期版本 409、校验失败 422、客户端草稿保留和成功提交；支持 `--report` 生成可归档证据；不注册 Jazor runtime 服务 |
| `inspect-razorvue-chain.cs` | 检查 `.razor` → Razor SG generated C# → render-function module → source map 链路；支持文本、JSON 和 IDE 可消费的 SARIF（`--sarif`）输出，并在断链时返回非零退出码 |
| `generate-jazoradmin-brand-assets.cs` | 再生成或检查 JazorAdmin 本地品牌图标 |
| `publish-nuget.cs` | 本地打包 NuGet 验证；正式发布只走 tag 触发的 NuGet 工作流，本地必须 `--skip-push`；脚本不探测环境变量中的 `NUGET_API_KEY` |
| `release-notes.cs` | 为 tag 输出发布说明：优先取 CHANGELOG 对应版本章节，否则按 tag 区间提交生成 |
| `verify-release-notes.cs` | 在发布前确认目标 tag 存在带日期的 `CHANGELOG.md` 版本章节和非空双语说明 |
| `verify-release-candidate.cs` | 按 1.0 冻结规则串联 release notes、Release build、API 兼容性、覆盖率、主线测试、绑定、typed bootstrap、NuGet 包和 SPA/SSR consumer，并归档统一报告；支持 `--only` 局部复核 |

覆盖率脚本默认把 TRX、Cobertura 和同轮临时结果写入仓库根目录的
`test/coverage/<gate>/`；`test/` 是本地生成目录，不应把测试产物写到仓库外的盘符根目录。

具体参数和验证范围以脚本开头的说明及相应项目 README 为准。

## 相关文档

- [开发与测试](../../docs/03-guides/development-and-testing.md)
- [示例总览](../../docs/03-guides/examples.md)
