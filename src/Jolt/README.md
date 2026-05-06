# Jolt

> Status: active development-time boundary
> Positioning: standalone `.NET 10` host for `.jazor` authoring, workspace coordination, Deno/Volar frontend semantics, preview, build, and debug.

`Jolt` 是当前唯一活跃的 `.jazor` 开发时宿主边界。

## Authoring Model

- `.jazor` 是第一作者文档，保持 Razor-first。
- `.vue`、`.ts`、`.js`、`.css`、`.html` 作为同一工作区图中的邻近文档参与语义与导航。
- `@module` 是当前规范的 authoring 指令。
- `@import` / `@vueimport` / `@jsimport` 是历史兼容路径，应通过诊断和 quick-fix 迁回当前语法。
- 虚拟 `.vue` / `.cs` 工件可以存在，但它们只是实现细节，不是公开 authoring 边界。

## Responsibilities

- `.jazor` 工作区与文档生命周期管理。
- Jazor / Roslyn / Volar 三条语义 lane 的路由与聚合。
- DevServer、HMR、build、source map 与 preview/debug 配套能力。
- Deno 托管的 Volar/TypeScript 前端语义。
- 主机侧 RPC、LSP、bridge coordination 与 workspace resolver。

## Boundaries

- `Jolt` 不拥有编译器 lowering 规则；那属于 `Jazor.Compiler`。
- RazorVue 共享语义与宿主协议 DTO 不在 `Jolt` 内定义，而是在 `Jazor.RazorVue`。
- transport-based analysis 仅保留为迁移兼容路径，不再是目标架构。

## Runtime Modes

| 模式 | 说明 |
|------|------|
| 默认启动 | 输出启动信息与 `jolt/getHostInfo` envelope |
| `--stdio` | stdio RPC 模式；stdin 重定向时也会自动进入 |
| `--lsp` | LSP 模式 |
| `--dev` | 开发服务器 + HMR |
| `--build` | 生产构建 |
| `--analysis-stdio` | 旧分析路径兼容模式 |
| `--inspect-razor-toolset` | 检查 Razor SDK toolset |
| `--probe-inproc-razor=<path>` | 进程内 Razor 投影探针 |

## Current Layout

- `Analysis/`: 分析客户端抽象与兼容 transport。
- `Build/`: build orchestrator、CSS pipeline、资产与 import map。
- `DevServer/`: 按需编译、HTTP、HMR、模块解析。
- `Jazor/`: `.jazor` 解析与投影服务。
- `Lsp/`: session、lane、routing、coordination。
- `Razor/InProc/`: 进程内 Razor 设计时代码投影。
- `Roslyn/InProc/`: 进程内 Roslyn 语义服务。
- `Rpc/`: Jolt RPC dispatcher、processor、serializer。
- `Services/`: `JoltService`、前端上下文与热更新规划。
- `Volar/Deno/`: Deno worker、协议、缓存与宿主。
- `Workspace/`: 工作区存储、邻近解析与边界发现。

## Shared Contracts

Jolt 现在不再在 `Protocol/Contracts/` 下自定义共享 DTO。共享契约统一位于：

- `src/Jazor.RazorVue/Documents/`
- `src/Jazor.RazorVue/Protocol/`

其中包括：

- `DocumentVersion`
- `TextSpan`
- `TextChange`
- `DocumentSnapshot`
- `HostInfo`
- `RpcMessages`
- `JoltRpcMethodNames`
- `ProtocolJsonSerializer`

## Read Next

- [../../docs/01-目标/jolt/README.md](../../docs/01-目标/jolt/README.md)
- [../../docs/03-完成/jolt/status.md](../../docs/03-完成/jolt/status.md)
- [../Jazor.Common/README.md](../Jazor.Common/README.md)
