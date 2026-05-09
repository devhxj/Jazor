# Jolt — 全功能开发模式

> 对应源码：`src/Jolt/`

## 为什么需要

库模式的 RazorVue 只解决"构建时生成产物"。应用开发还需要编辑器智能、预览、HMR、生产构建、源码级调试与工作区级导航。`Jolt` 就是这一整套开发时宿主。

## 设计原则

1. **`.jazor` 是第一作者文档**：保持 Razor-first，不把虚拟 `.vue` 当作公开 authoring 面。
2. **工作区是图，不是孤立文件**：邻近 `.vue`、`.ts`、`.js`、`.css`、`.html` 都参与同一工作区图。
3. **三条 native lane，各自保真**：Jazor、Roslyn、Volar 各自返回本地语义，跨 lane 补充由协调层完成。
4. **Deno 是唯一前端运行时路径**：不再把 Bun/Vite 作为长期目标。

## 核心子系统

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

## 运行模式

| 模式 | 用途 |
|------|------|
| `--stdio` | stdio RPC |
| `--lsp` | LSP 服务 |
| `--dev` | 开发服务器 + HMR |
| `--build` | 生产构建 |
| `--analysis-stdio` | 迁移兼容分析模式 |
| `--inspect-razor-toolset` | 检查 Razor SDK 工具集 |
| `--probe-inproc-razor=<path>` | 进程内 Razor 投影探针 |

## 与 RazorVue 的关系

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
