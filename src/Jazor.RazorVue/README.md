# Jazor.RazorVue

> Status: active reference
> Positioning: shared RazorVue semantic, Razor SDK bridge, and host protocol layer used by analyzer, emit, Jolt, and library-component packages.

`Jazor.RazorVue` 不再只是“库模式 lowering 项目”。在当前结构下，它承接整条 RazorVue lane 需要跨 `Jazor.Analyzer`、`Jazor.Emit`、`Jolt`、`ECMAScript.Vuetify` 共享的代码：核心语义、Razor SDK 桥接、artifact/catalog 模型，以及 RazorVue/Jolt 的宿主协议 DTO。

## Responsibilities

- 提供 RazorVue 入口分类、descriptor、render tree、canonical model、lowering 与 catalog。
- 提供 `RazorCodeDocument` / Razor IR 获取、文档定位与 template frontend 选择。
- 提供 legacy render artifact 与 design-time SFC artifact 的共享模型。
- 提供 `Documents/` 与 `Protocol/` 下的 RazorVue/Jolt 宿主协议 DTO。

## Boundaries

- `Jazor.Analyzer` 负责 Roslyn analyzer / incremental generator 宿主与 RazorVue authoring diagnostics。
- `Jazor.Emit` 负责 `.mjs` / `.vue` / manifest / source map 的物化与 bundling。
- `Jolt` 负责 LSP、DevServer、进程管理、工作区与运行时宿主编排。
- `Jazor.Common` 只保留真正通用的 `Format` 与 `SourceMaps`，不再拥有 RazorVue/Jolt 协议 DTO。
- `IVueComponent` / `IVueLibraryComponent`、`VueLibrary*` authoring 标记类型以及 `VuePropKind` / `VueEmitKind` / `VueComponentFlags` 已经下沉到 `ECMAScript.VueContract` 项目；实际代码命名空间统一为 `ECMAScript.VueContract` / `ECMAScript.VueContract.Descriptor`，`Jazor.RazorVue` 直接消费这组合同，不保留旧命名空间转发层。

## Current Layout

- `Discovery/`: 入口分类与候选发现。
- `Descriptor/`: props / emits / slots / registry / resolution。
- `RenderTree/`: 手写 `BuildRenderTree` authoring 前端。
- `RazorSdk/`: `RazorCodeDocument` / Razor IR 主前端与文档定位。
- `Canonical/`, `Sfc/`, `Lowering/`, `Artifacts/`, `Emit/`: shared artifact/model pipeline。
- `Documents/`, `Protocol/`: Jolt 与分析宿主共享文档/RPC 契约。

## Template Frontend Rule

- Razor 生成组件优先走 `RazorCodeDocument` / Razor IR。
- 只有源码中显式手写的 `BuildRenderTree` 组件才允许走 `BuildRenderTree` 前端。
- 对于 Razor 生成组件，如果既没有可绑定的 Razor 文档又不是手写 `BuildRenderTree` authoring，应显式失败，而不是静默回退。

## Verification

```powershell
dotnet build src/Jazor.RazorVue/Jazor.RazorVue.csproj
dotnet test src/Jazor.RazorVue.Test/Jazor.RazorVue.Test.csproj
dotnet test src/Jazor.RazorVue.RazorIr.Test/Jazor.RazorVue.RazorIr.Test.csproj
```

## Read Next

- [../Jazor.Analyzer/README.md](../Jazor.Analyzer/README.md)
- [../Jazor.Emit/README.md](../Jazor.Emit/README.md)
- [../../docs/01-目标/razorvue/README.md](../../docs/01-目标/razorvue/README.md)
