# RazorVue — 库模式

> Status: 活跃参考
> Positioning: RazorVue 库模式设计入口，描述当前编译时 Razor-to-JS 路线及其物理拆分后的责任边界。
> Note: RazorVue 的对外命名空间仍保留 `Jazor.RazorVue` / `Jazor.RazorVue.Analysis`，但物理项目已经收束为 `Jazor.RazorVue`（其中 `RazorSdk/` 目录承载 Razor SDK 桥接）、`Jazor.Analyzer`、`ECMAScript.Vuetify`。

> 对应源码：`src/Jazor.RazorVue/`、`src/Jazor.Analyzer/RazorVue/`、`src/ECMAScript.Vuetify/`

## 为什么需要

不是所有项目都需要完整开发宿主、LSP、HMR 或独立前端进程。很多场景只需要在 `dotnet build` 时把 Razor 组件编译成 JavaScript 模块，然后作为普通类库产物发布。RazorVue 就是这条编译时库模式。

## 解决什么问题

1. **编译时 Razor-to-JS**：在构建阶段完成组件语义提取、描述符解析和 artifact 生成。
2. **NuGet 式集成**：调用方不必引入完整 Jolt 宿主。
3. **库组件 authoring**：支持通过 `VueLibrary*` 元数据声明外部 Vue/Vuetify 组件。
4. **稳定共享语义**：让 Analyzer、Compiler、Emit、Jolt 共用同一套 RazorVue 中间模型。

## 当前物理分工

### `src/Jazor.RazorVue/`

共享 RazorVue 核心语义层：

- 入口分类与编译上下文
- 描述符、注册表、render tree
- lowering、artifact、catalog、source origin
- `JazorVueCompiler` / `JazorVueParser`

### `src/Jazor.RazorVue/RazorSdk/`

Razor SDK 桥接层：

- `RazorCodeDocument` / IR 获取
- Razor 组件与 `_Imports.razor` 文档定位
- Razor 生成组件优先走 `RazorCodeDocument` / IR，手写 `BuildRenderTree` 仅作为显式 authoring 路径

### `src/Jazor.Analyzer/RazorVue/`

Roslyn 宿主层：

- authoring 诊断
- 增量生成器
- 兼容分析 RPC 与进程内分析运行时
- 目录内再分为 `Diagnostics/` 与 `Generation/`；`Jazor.Vue` 兼容分析宿主单独放在 `src/Jazor.Analyzer/VueHost/`

### `src/ECMAScript.Vuetify/`

库组件层：

- Vuetify 绑定类型
- `VBtn`、`VCard`、`VDialog` 等 authoring 组件桩
- `VueLibraryComponent` / style / plugin requirement 元数据消费面

### `src/ECMAScript.Contract/`

最小基础契约层：

- `IUIComponent`
- `JazorAttribute`
- `Op`

### `src/Jazor.Common/`

共享通用设施层：

- `SourceMaps`
- `VueContracts`
- 通用 emit / 协议辅助

这里不再承载 RazorVue core 语义本体。

## 当前编译链路

```text
Razor 组件
    ↓ Razor 语义前端
RazorVueCompilationContext
    ↓ 描述符解析 / render tree / lowering
RazorVueCatalog + VueCompiledArtifact
    ↓ Jazor.Emit（按需物化）
JavaScript 模块 / RazorVue manifest / source maps
```

## 与 Jolt 的关系

| 维度 | RazorVue（库模式） | Jolt（开发时宿主） |
|------|-------------------|---------------------|
| 触发方式 | Source Generator / 编译时 | 独立进程 |
| 作者文档 | Razor 组件 | `.jazor` + 邻近 `.vue/.ts/.css` |
| 目标 | 构建产物 | 编辑、预览、构建、调试 |
| 热更新 | 无 | 有 |
| 共享点 | 编译器、RazorVue 共享语义、SourceMap、协议 DTO | 同左 |

## 设计文档

- [architecture.md](architecture.md)
- [design/](design/)

`design/` 中的文档描述的是 RazorVue 语义和规则本身，不要求和当前物理项目名一一对应。
