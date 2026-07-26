# Jolt 开发时宿主（历史）

> Status: 历史资料
> Retirement: Jolt 已在 `3ee18679fbdf43c13e05d7bfac8857ddcebd19f9` 从转型分支退役。
> Baseline: 源码、目录和运行命令仅对应 `d68aecbb00b23aa35735c9a269b2e987c7815b05`；本页不描述当前架构。

## 历史动机

当时的 RazorVue 库模式只解决“构建时生成产物”，Jolt 曾作为编辑器、预览、HMR、生产构建、源码级调试和工作区导航的开发时宿主。

## 历史设计原则

1. **`.jazor` 是第一作者文档**：保持 Razor-first，不把虚拟 `.vue` 当作公开 authoring 面。
2. **工作区是图，不是孤立文件**：邻近 `.vue`、`.ts`、`.js`、`.css`、`.html` 都参与同一工作区图。
3. **三条 native lane，各自保真**：Jazor、Roslyn、Volar 各自返回本地语义，跨 lane 补充由协调层完成。
4. **Deno 是唯一前端运行时路径**：不再把 Bun/Vite 作为长期目标。

## 历史核心子系统

| 子系统 | 目录 | 作用 |
|------|------|------|
| Jazor Core | `Jazor/` | `.jazor` 解析、投影与前端上下文派生 |
| LSP | `Lsp/` | session、lane、routing、coordination |
| Roslyn In-Proc | `Roslyn/InProc/` | `.cs` / `.jazor` 代码区 C# 语义 |
| Razor In-Proc | `Razor/InProc/` | 设计时代码投影与 Razor SDK 桥接 |
| Volar / Deno | `Volar/Deno/` | Vue/TS/CSS/HTML 语义 worker |
| DevServer | `DevServer/` | 预览、按需编译、HMR |
| Build | `Build/` | 构建、CSS、资产、import map |
| RPC / Services / Workspace | `Rpc/`、`Services/`、`Workspace/` | 工作区、协议、热更新规划与 host 协调 |

## 历史运行模式

| 模式 | 用途 |
|------|------|
| `--stdio` | stdio RPC |
| `--lsp` | LSP 服务 |
| `--dev` | 开发服务器 + HMR |
| `--build` | 生产构建 |
| `--analysis-stdio` | 迁移兼容分析模式 |
| `--inspect-razor-toolset` | 检查 Razor SDK 工具集 |
| `--probe-inproc-razor=<path>` | 进程内 Razor 投影探针 |

## 当时与 RazorVue 的关系

| 维度 | Jolt | RazorVue |
|------|------|----------|
| 目标 | 开发时宿主 | 编译时库模式 |
| 作者文档 | `.jazor` + 邻近工作区文档 | Razor 组件 |
| 输出关注点 | LSP、预览、HMR、Build、Debug | artifact、catalog、generator |
| 共享基础 | Compiler、RazorVue 共享语义、SourceMap、协议 DTO | 同左 |

## 进一步阅读

- [architecture.md](architecture.md)
- [protocol/Contracts.md](protocol/Contracts.md)
- [workspace/Resolver.md](workspace/Resolver.md)
- [lsp/ThreeLaneArchitecture.md](lsp/ThreeLaneArchitecture.md)
- [devserver/Hmr.md](devserver/Hmr.md)
- [build/Pipeline.md](build/Pipeline.md)
- [testing/TestTopology.md](testing/TestTopology.md)
