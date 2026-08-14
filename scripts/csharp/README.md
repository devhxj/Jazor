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
| `test-dotnet.cs` | 构建一次并运行当前主测试 lane，支持 `--project <name>` 聚焦项目（例如 `dataui`） |
| `verify-compiler-coverage.cs` | 执行编译器测试和正式覆盖率门槛 |
| `verify-razorvue-coverage.cs` | 执行 RazorVue 覆盖率门槛 |
| `verify-vue-binding-coverage.cs` | 审核 Vue 生态 binding 的公开契约覆盖 |
| `verify-development-hmr.cs` | 验证开发模式的 HMR artifact 和浏览器路径 |
| `wiki-build-local.cs`、`wiki-serve.cs`、`wiki-verify-*.cs` | 构建、预览与验证 Wiki 示例 |
| `verify-windows-spa-release.cs` | 打包本地 NuGet 后，以隔离 Wiki 消费者完成 Windows Release publish 和 Edge 浏览器验证 |
| `generate-jazoradmin-brand-assets.cs` | 再生成或检查 JazorAdmin 本地品牌图标 |

具体参数和验证范围以脚本开头的说明及相应项目 README 为准。

## 相关文档

- [开发与测试](../../docs/03-guides/development-and-testing.md)
- [示例总览](../../docs/03-guides/examples.md)
