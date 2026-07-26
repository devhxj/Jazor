# 工具与集成

> 当前对应源码：`src/Jazor/`
> 历史说明：Jolt VS Code client 和旧 `Jazor.Test` 不在当前项目图中；相关源码仅见 Git 基线 `d68aecbb00b23aa35735c9a269b2e987c7815b05`。

## 为什么需要

编译器和运行时写好后，用户还需要：能一键安装的 NuGet 包、能实时看到代码效果的 IDE 集成、能验证整体流程的集成测试。工具与集成层把所有组件打包成用户可直接使用的产品形态。

## 解决什么问题

### Jazor（NuGet 包）

将运行时、分析器、源码生成器、发射工具、MSBuild 集成打包为单一 NuGet 包：

- 用户只需 `dotnet add package Jazor` 即可获得完整工具链
- 包含 Analyzer（编译时检查）、Compiler.Generator（白名单生成）、Emit（产物输出）
- MSBuild 集成让构建流程自动触发 C#→JS 编译

### Jolt VS Code 集成（历史）

当前分支不提供 Jolt LSP 或 VS Code client。历史基线曾包含以下集成：

- 启动 `Jolt --language-server` 作为 LSP 后端
- 提供语法高亮、智能补全、错误诊断、Go to Definition 等 IDE 功能
- 支持 `.jazor` 文件类型关联

### Jazor.Test（历史集成测试 / 手动实验）

历史基线中的主集成测试和手动实验平台：

- 通过 Roslyn 编译 C# 代码并送入 Jazor 编译管线，验证端到端转换
- `NamedPipeClient`：实现 .NET 宿主与 JS/Vue 运行时之间的二进制命名管道通信协议
- `JazorExtractorTask`：MSBuild Task，从编译后的程序集中提取生成的代码用于白名单/特殊编译处理

## 历史项目去向（基线快照）

以下描述只对应历史基线中的迁移判断；Jolt 自身也已从当前转型分支退役：

| 项目 | 原定用途 |
|------|---------|
| `Jazor.Vite` | 前端集成历史项（已由 Jolt 内置 Deno 管线替代） |
| `Jazor.Vue` | Vue 核心抽象（已合并到 RazorVue/Jolt） |
| `Jazor.Vue.Analysis` | Vue 模板分析（已合并到 RazorVue.Analysis） |
| `Jazor.Vue.Analysis.Host` | 分析驱动进程（已合并到 Jolt） |
| `Jazor.Vue.Analysis.Runtime` | 分析运行时（已合并到 Jolt） |
| `Jazor.VueContracts` | 通信契约（已合并到 Jolt RPC 层） |
