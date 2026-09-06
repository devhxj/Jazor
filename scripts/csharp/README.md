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
| `verify-compiler-coverage.cs` | 执行编译器测试和正式覆盖率门槛 |
| `verify-razorvue-coverage.cs` | 执行 RazorVue 覆盖率门槛 |
| `verify-vue-binding-coverage.cs` | 审核 Vue 生态 binding 的公开契约覆盖 |
| `verify-vue-binding-contracts.cs` | 统一校验 Element Plus、Vuetify、TDesign 的生成快照、原始文档与资源 manifest |
| `benchmark-razorvue-build.cs` | 测量 RazorVue clean、incremental、HMR 和 Release 构建时间并输出 JSON 基线 |
| `verify-development-hmr.cs` | 验证开发模式的 HMR artifact 和浏览器路径 |
| `wiki-import-docs.cs`、`wiki-build-local.cs`、`wiki-serve.cs`、`wiki-verify-*.cs`、`wiki-export-static.cs` | 导入 `docs/`、构建、预览、验证与静态导出 Jazor 官方网站 |
| `verify-windows-spa-release.cs` | 打包本地 NuGet 后，以隔离 Wiki 消费者完成 Windows Release publish 和 Edge 浏览器验证 |
| `verify-windows-ssr-release.cs` | 打包本地 NuGet 后，以隔离 RazorVue TodoList 消费者完成 `JazorSSR=true` Release publish、Deno SSR HTML、发布目录资源解析与 Edge hydration 交互验证 |
| `generate-jazoradmin-brand-assets.cs` | 再生成或检查 JazorAdmin 本地品牌图标 |
| `publish-nuget.cs` | 本地打包 NuGet 验证；正式发布只走 tag 触发的 NuGet 工作流，本地必须 `--skip-push` |
| `release-notes.cs` | 为 tag 输出发布说明：优先取 CHANGELOG 对应版本章节，否则按 tag 区间提交生成 |

覆盖率脚本默认把 TRX、Cobertura 和同轮临时结果写入仓库根目录的
`test/coverage/<gate>/`；`test/` 是本地生成目录，不应把测试产物写到仓库外的盘符根目录。

具体参数和验证范围以脚本开头的说明及相应项目 README 为准。

## 相关文档

- [开发与测试](../../docs/03-guides/development-and-testing.md)
- [示例总览](../../docs/03-guides/examples.md)
