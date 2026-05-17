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
- 用户直接 authoring 的 `IVueComponent` / `IVueLibraryComponent` canonical 类型保持在 `ECMAScript.Vue3`；`VueLibrary*` 标记类型以及 `VuePropKind` / `VueEmitKind` / `VueComponentFlags` 归属 `ECMAScript.VueContract` / `ECMAScript.VueContract.Descriptor`，`Jazor.RazorVue` 直接消费这组正式合同，不保留旧位置回退。
- RazorVue 的 consumer authoring 合同是显式按需导入，而不是由 `Jazor` NuGet 包自动注入 marker alias。
- 组件文件如需直接使用 `IVueComponent` / `IVueLibraryComponent` 简名，应显式声明 `using static ECMAScript.Vue3;`；完整 Vue3 API 亦同。

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

## `@key` Support

- RazorVue 现已将 vnode `key` 作为一等语义处理，不会把它退化成普通 HTML / component attribute。
- 手写 `BuildRenderTree` authoring 支持 `RenderTreeBuilder.SetKey(...)`，会在 render tree、canonical model、H lowering、SFC template lowering 中保留节点键。
- Razor SDK / Razor IR authoring 支持 Razor `@key`。
- 对官方 Razor Source Generator 当前会把 component `@key="Id"` 编成 `AddComponentParameter(..., "@key", "Id")` 的形态，RazorVue 会基于原始 Razor 源片段与生成调用位次恢复 C# 表达式语义，确保 `<Child @key="Id" />` 仍然按属性访问降为 `props.id`，而不是错误地固定成字符串 `"Id"`。

## Runtime Naming Contract

- C# authoring surface 继续使用正常的 `PascalCase` 成员名，例如 `Title`、`IsDone`、`ModelValue`。
- 进入 Vue runtime/template 边界后，RazorVue 统一按 JavaScript/Vue 约定输出 `camelCase` 访问名，例如 `props.modelValue`、`item.title`、`item.isDone`、`context.isActive`。
- 该规则同时适用于：
  - 组件 props 的 `props.*` 访问
  - typed slot/scoped slot 的上下文对象成员
  - Razor IR / handwritten `BuildRenderTree` 两条 frontend 路线
- `script setup` SFC 输出统一保留：
  - `const __jazorRawProps = defineProps<...>();`
  - `const props = __jazorRawProps;`
- 当组件参数存在默认值代理时，`props` 可能升级为 `new Proxy(__jazorRawProps, ...)`，但 `__jazorRawProps` 仍是稳定底层绑定名。

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
