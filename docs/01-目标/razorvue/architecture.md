# RazorVue 整体架构

> **状态**: 已实现  
> **最后更新**: 2026-04-21  
> **维护者**: developerhan

## 1. 文档定位

本文档是 RazorVue 技术线路的架构总览，解释其作为 **库模式** 的设计理念、项目组织、design-time Source Generator 管线流程，以及与 **Jolt 全功能模式** 的对比。

## 目录

- [1-文档定位](#1-文档定位)
- [2-项目结构](#2-项目结构)
- [3-依赖关系](#3-依赖关系)
- [4-source-generator-管线](#4-source-generator-管线)
- [5-核心设计原则](#5-核心设计原则)
- [6-与-jolt-全功能模式的对比](#6-与-jolt-全功能模式的对比)
- [7-文件组织](#7-文件组织)
- [8-关键依赖](#8-关键依赖)
- [9-扩展点](#9-扩展点)
- [10-诊断规则](#10-诊断规则)
- [11-后续工作](#11-后续工作)
- [12-相关文档](#12-相关文档)

**阅读路径**:
1. 新手入门: `docs/01-目标/razorvue/README.md` - 快速了解 RazorVue 是什么
2. 架构深入: 本文档 - 理解整体设计和管线流程
3. 实现细节: `src/Jazor.RazorVue/` 各子模块 - 查看具体实现代码

## 2. 项目结构

RazorVue 技术线路由 3 个核心项目组成，职责明确分离：

```
src/
├── Jazor.RazorVue/                    # 【核心语义】descriptor, render tree, lowering, artifacts, pipeline
├── Jazor.RazorVue.Analysis/           # 【Roslyn 接入】Source Generator + Analyzer 薄层
└── Jazor.RazorVue.Vuetify/            # 【组件桩】Vuetify 3 组件库桩 (35 个组件)
```

### 2.1 Jazor.RazorVue (核心语义)

**定位**: RazorVue 的核心语义归属层，包含 Vue-first 的组件模型、渲染树提取、Lowering 管线和编译产物。

**关键目录**:
- `Discovery/` - 组件发现与分类 (`RazorVueEntryClassifier`, `RazorVueComponentCandidate`)
- `Descriptor/` - Vue 组件描述符 (`VueComponentDescriptor`, `VuePropDescriptor`, `VueEmitDescriptor`, `VueSlotDescriptor`)
- `RenderTree/` - Razor 渲染树到 Vue 渲染树的转换 (`RazorVueRenderTreeExtractor`)
- `Lowering/` - 语义快照到 Vue 编译产物的 Lowering (`RazorVueArtifactFactory`, `RazorVueExpressionEmitter`)
- `Artifacts/` - 编译产物模型（legacy `VueCompiledArtifact` 与过渡中的 `VueSfcArtifact` / block-aware identity）
- `Extensibility/` - 扩展点接口 (`IRazorSemanticFrontend`, `IRazorVueArtifactLowerer`)
- `RazorVuePipeline.cs` - 管线编排入口
- `RazorVueCompilationContext.cs` - 编译上下文，共享符号和语义视图
- `IVueComponent.cs` - Vue 组件的基础接口 (继承 `IJazorComponent`)

**核心设计原则**:
> **Vue-first 语义**: RazorVue 不是简单的 Razor 语法转换，而是先把 Razor 组件模型收口到 canonical Vue semantics，再在库模式下于 design time 生成 `.vue` SFC artifact。当前代码仍保留 `defineComponent` + `setup` + `render` 的 legacy 过渡车道，但 slot contract 已要求 default slot forwarding 生成 nested slot template，`RenderFragment<T>` 的 non-callable misuse 统一 design-time fail-fast。

### 2.2 Jazor.RazorVue.Analysis (Roslyn 接入)

**定位**: 薄层 Roslyn Generator 和 Analyzer，只负责接线，不承载核心语义实现。

**文件**:
- `src/Jazor.Analyzer/RazorVue/Generation/RazorVueGenerator.cs` - 实现 `IIncrementalGenerator`，生成 `Jazor.Generated.RazorVueCatalog.g.cs`
- 14 个诊断规则 (JAZORVGA001-JAZORVGA014) - 组件发现、参数验证、Slot 上下文、库桩声明等

**工作流程**:
1. 通过 `[ECMAScriptModule]` 特性发现候选组件
2. 创建 `RazorVueCompilationContext` 上下文
3. 调用 `RazorVuePipeline` 执行核心语义处理
4. 生成 `RazorVueCatalog` 静态类，包含所有编译产物的序列化数据

### 2.3 Jazor.RazorVue.Vuetify (组件桩)

**定位**: Vuetify 3 组件库的 C# 桩代码，提供强类型的组件声明。

**示例** (`VBtn.cs`):
```csharp
[ECMAScriptModule]
public class VBtn : VueLibraryComponent<VBtn>
{
    [VueProp("string", acceptBinding: true)]
    public string? Color { get; set; }

    [VueProp("boolean")]
    public bool Disabled { get; set; }

    [VueSlot("default")]
    public RenderFragment? ChildContent { get; set; }
}
```

**覆盖组件**: 35 个 Vuetify 3 常用组件 (VBtn, VCard, VTextField, VSelect, VDataTable, 等等)

## 3. 依赖关系

### 3.1 项目级依赖图

```
┌─────────────────────────────────────────────────────────────┐
│                    Jazor.RazorVue.Vuetify                   │
│                    (Vuetify 组件桩)                         │
└────────────────────────────┬────────────────────────────────┘
                             │ ProjectReference
                             ▼
┌─────────────────────────────────────────────────────────────┐
│                    Jazor.RazorVue                           │
│              (核心语义: descriptor, lowering)               │
└─────┬───────────────────────────┬──────────────────────────┘
      │ ProjectReference           │ ProjectReference
      ▼                           ▼
┌─────────────┐          ┌─────────────────┐
│ Jazor.Razor │          │ Jazor.Compiler  │
│ (Razor 桥接)│          │ (C#→JS 编译器)  │
└─────────────┘          └─────────────────┘

┌─────────────────────────────────────────────────────────────┐
│               Jazor.RazorVue.Analysis                       │
│                  (Roslyn Generator)                         │
└─────┬───────────────────────────┬──────────────────────────┘
      │ ProjectReference           │ PackageReference
      ▼                           ▼
┌─────────────┐          ┌─────────────────┐
│Jazor.RazorVue│          │ Microsoft.CodeAnalysis.* │
│ (核心语义)  │          └─────────────────┘
└─────────────┘
```

### 3.2 关键依赖说明

| 依赖 | 用途 |
|------|------|
| `Jazor.Compiler` | C# 到 JavaScript 的核心编译器，提供 `AstConverter` 和 `SemanticWalker` |
| `Jazor.Razor` | Razor 基础层，提供 `IJazorComponent` 接口和 Razor 集成 |
| `Microsoft.CodeAnalysis.*` | Roslyn 编译器平台，用于符号分析和代码生成 |

## 4. Source Generator 管线

RazorVue 的编译流程采用 **增量式 Source Generator** 管线，从 C# 源代码到 compiler-owned Vue artifact 的完整转换：

### 4.1 管线阶段概览

```
┌─────────────────────────────────────────────────────────────┐
│ 1. ECMAScriptModule 发现 (RazorVueGenerator)               │
│    └─ ForAttributeWithMetadataName("ECMAScriptModule")    │
└────────────────────────┬────────────────────────────────────┘
                         ▼
┌─────────────────────────────────────────────────────────────┐
│ 2. CompilationContext 创建                                  │
│    └─ RazorVueCompilationSymbols.TryCreate(compilation)    │
│       ├─ 查找 ECMAScriptModuleAttribute                    │
│       ├─ 查找 ComponentBase                                │
│       ├─ 查找 IJazorComponent                              │
│       └─ 查找 VueLibraryComponent                          │
└────────────────────────┬────────────────────────────────────┘
                         ▼
┌─────────────────────────────────────────────────────────────┐
│ 3. SemanticSnapshot 提取 (RazorVueCompilationContext)       │
│    └─ DiscoverComponentCandidates()                        │
│       ├─ 枚举所有命名类型                                   │
│       ├─ RazorVueEntryClassifier.Classify()                 │
│       │  └─ [ECMAScriptModule] + 非静态 + 继承 IVueComponent │
│       └─ 收集生命周期方法和逻辑成员                         │
└────────────────────────┬────────────────────────────────────┘
                         ▼
┌─────────────────────────────────────────────────────────────┐
│ 4. Descriptor 创建 (VueComponentDescriptorFactory)          │
│    └─ Create(candidate, context)                           │
│       ├─ Props → VuePropDescriptor (从 [Parameter] 提取)   │
│       ├─ Emits → VueEmitDescriptor (从 EventCallback 提取) │
│       └─ Slots → VueSlotDescriptor (从 RenderFragment 提取；typed slot / default slot forwarding contract 在此后继续参与 canonical 判定)│
└────────────────────────┬────────────────────────────────────┘
                         ▼
┌─────────────────────────────────────────────────────────────┐
│ 5. RenderTree 提取 (RazorVueRenderTreeExtractor)           │
│    └─ Extract(context, snapshot)                           │
│       ├─ 解析 BuildRenderTree 方法体                       │
│       ├─ 将 OpenElement/CloseElement → RazorVueElementNode │
│       ├─ 将 AddAttribute → RazorVueAttributeNode           │
│       ├─ 将 AddContent → RazorVueTextNode/ExpressionNode   │
│       └─ 将 C# 控制流 → ConditionalNode/ForEachNode        │
└────────────────────────┬────────────────────────────────────┘
                         ▼
┌─────────────────────────────────────────────────────────────┐
│ 6. Artifact Lowering                                       │
│    └─ Lower(context, snapshot)                             │
│       ├─ 表达式发射 (RazorVueExpressionEmitter)            │
│       │  └─ C# 表达式 → JavaScript 表达式                  │
│       ├─ 组件解析 (ResolveComponents)                      │
│       │  └─ 子组件引用 → import 语句                       │
│       ├─ canonical / SFC 语义生成                          │
│       │  └─ Canonical H Model -> SFC Semantic Model        │
│       ├─ SFC artifact 生成                                 │
│       │  └─ VueSfcArtifact                                 │
│       └─ legacy render lane                                │
│          └─ 仅作为过渡车道保留                             │
└────────────────────────┬────────────────────────────────────┘
                         ▼
┌─────────────────────────────────────────────────────────────┐
│ 7. Catalog 构建 (RazorVueCatalogBuilder)                   │
│    └─ Build(assemblyName, artifacts)                       │
│       └─ 目标形状为 per-component carrier + small index    │
└────────────────────────┬────────────────────────────────────┘
                         ▼
┌─────────────────────────────────────────────────────────────┐
│ 8. 代码生成 (RazorVueGenerator.EmitRazorVueCatalog)        │
│    └─ 生成 per-component artifact carrier + index catalog  │
│       └─ 对外仍暴露稳定的 RazorVueCatalog 反射入口         │
└─────────────────────────────────────────────────────────────┘
```

### 4.2 核心数据流

```csharp
// 输入: C# 组件
[ECMAScriptModule]
public class MyComponent : IVueComponent
{
    [Parameter] public string Title { get; set; }
    
    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        builder.OpenElement(0, "div");
        builder.AddContent(1, Title);
        builder.CloseElement();
    }
}

// 输出: 当前 legacy Vue 模块（字符串形式嵌入生成的 C# 代码）
import { defineComponent, h } from 'vue';

export default defineComponent({
  name: 'MyComponent',
  props: {
    title: { type: String, required: false }
  },
  setup(props) {
    return () => h('div', props.title);
  }
});
```

上面的例子反映的是当前 legacy lowering 车道。RazorVue 库模式的 active target 已改为：在 design-time compilation 中生成 `.vue` SFC artifact，并在 build/emit 阶段只做物化。

## 5. 核心设计原则

### 5.1 Vue-first 语义

RazorVue 不是简单的 Razor 语法转换，而是将 **Razor 组件模型映射到 Vue 3 的 Composition API**：

| Razor 概念 | Vue 3 映射 |
|-----------|-----------|
| `IVueComponent` | `defineComponent` |
| `[Parameter]` | `props` 声明 |
| `EventCallback<T>` | `emits` 声明 |
| `RenderFragment` | `slots` |
| `BuildRenderTree` | `render` 函数 |
| 生命周期方法 | Vue 生命周期钩子 (`onMounted`, `onUpdated`, etc.) |
| `ref` 字段 | `setup` 中的 `ref`/`reactive` |

其中 `RenderFragment<T>` 的 Phase 2 下游 contract 不是单一“slot value”概念。
它至少要区分：

- 当前组件消费 typed slot outlet；
- 父组件把 callable `RenderFragment<T>` forwarding 给子组件；
- 非 callable 值误传给 typed slot 参数时的 fail-fast 边界。

库模式的 design-time SFC lane 必须显式保持这三者的差异，不能回退到 render fallback，也不能把 typed slot 退化成普通插值。

同时，默认 slot forwarding 也必须保持 forwarding 语义：

- `ChildContent -> Header/Footer/...` 这类 authoring 不能被当成普通值表达式；
- canonical / SFC lane 必须把它 materialize 为 nested slot template；
- 对无法无损表达的 typed slot misuse，统一以 `SlotContextMisuse` design-time fail-fast，而不是静默近似输出。

### 5.2 编译时转换

与 Jolt 全功能模式不同，RazorVue 在 **编译时** 完成所有转换：

| 特性 | RazorVue (库模式) | Jolt (全功能模式) |
|------|------------------|------------------|
| 转换时机 | 编译时 (Source Generator) | 运行时 (DevServer) |
| 输出形式 | design-time 生成 `.vue` SFC artifact（过渡期仍存在 legacy `.js` lane） | 内存中编译 + HMR |
| 开发体验 | 需要重新编译 | 即时热更新 |
| 部署方式 | 纯静态 JS 文件 | 需要 Jolt 服务器 |
| 调试支持 | 生成 SourceMap | 完整 DAP 调试 |
| 文件格式 | 无手写 `.vue` 输入；主工件目标为生成 `.vue` SFC | 支持 `.jazor` + `.vue` SFC |

### 5.3 语义保持

RazorVue 通过以下机制保证 C# 到 JavaScript 的语义等价：

1. **符号级分析**: 使用 Roslyn `ITypeSymbol` / `IMethodSymbol` 获取精确的类型信息
2. **操作树遍历**: 使用 `IOperation` 接口访问语义操作树，而非语法树
3. **表达式 Lowering**: `RazorVueExpressionEmitter` 将 C# 表达式转换为 JavaScript 表达式
4. **哈希校验**: `VueArtifactIdentity` 包含 `DescriptorHash` / `TemplateHash` / `LogicHash`，确保增量编译的正确性

### 5.4 HMR 支持

虽然 RazorVue 是编译时模式，但通过 `VueArtifactIdentity.HmrBoundaryKind` 支持 **未来 HMR 集成**：

| HmrBoundaryKind | 含义 |
|----------------|------|
| `TemplateOnly` | 仅模板变化，可安全热更新 |
| `LogicSafe` | 逻辑安全变化，支持热更新 |
| `FullReloadRequired` | 需要完全重新加载 |

## 6. 与 Jolt 全功能模式的对比

### 6.1 技术线路对比

| 维度 | RazorVue (库模式) | Jolt (全功能模式) |
|------|------------------|-------------------|
| **核心定位** | 编译时库，在 design time 生成 Vue SFC artifact | 运行时服务器，类似 Vite |
| **输入格式** | C# Razor 组件（无手写 `.vue` SFC 输入） | `.jazor` + `.vue` SFC |
| **转换时机** | 编译时 (Source Generator) | 运行时 (按需编译) |
| **输出形式** | `.vue` SFC artifact，由后段 bundler/host 消费 | 内存模块 + HMR WebSocket |
| **开发工具** | 无需额外工具，直接编译 | 需要 LSP Server + DevServer |
| **部署方式** | 无需 Jolt 服务器；由后段 bundler/host 处理 `.vue` | 需要 Jolt 服务器运行 |
| **调试支持** | 生成 SourceMap，断点映射到 C# | 完整 DAP 调试协议 |
| **热更新** | 需要重新编译 | 增量 HMR，毫秒级更新 |
| **适用场景** | 静态站点生成，组件库 | 开发环境，SPA 应用 |

### 6.2 适用场景建议

**选择 RazorVue (库模式) 当**:
- 你只需要组件库的编译时转换
- 你希望在 design time 生成 `.vue` SFC artifact，而无需 Jolt 服务器
- 你的应用部署在 CDN 或静态托管上
- 你可以接受重新编译的开发体验

**选择 Jolt (全功能模式) 当**:
- 你需要完整的开发环境 (LSP + HMR + Debug)
- 你需要支持 `.jazor` 和 `.vue` SFC 文件
- 你需要类似 Vite 的即时热更新体验
- 你正在构建复杂的 SPA 应用

## 7. 文件组织

### 7.1 源代码目录结构

```
src/Jazor.RazorVue/
├── Discovery/                      # 组件发现与分类
│   ├── RazorVueCompilationSymbols.cs    # 符号缓存
│   └── RazorVueEntryClassifier.cs       # 组件分类器
├── Descriptor/                     # Vue 组件描述符
│   ├── VueComponentDescriptor.cs
│   ├── VuePropDescriptor.cs
│   ├── VueEmitDescriptor.cs
│   └── VueSlotDescriptor.cs
├── RenderTree/                     # 渲染树模型
│   ├── RazorVueRenderNode.cs            # 节点基类
│   ├── RazorVueElementNode.cs
│   ├── RazorVueComponentNode.cs
│   ├── RazorVueTextNode.cs
│   ├── RazorVueExpressionNode.cs
│   ├── RazorVueConditionalNode.cs
│   ├── RazorVueForEachNode.cs
│   └── RazorVueRenderTreeExtractor.cs
├── Lowering/                       # 语义 Lowering
│   ├── RazorVueArtifactFactory.cs       # 主 Lowering 工厂
│   ├── RazorVueExpressionEmitter.cs     # 表达式发射器
│   └── IRazorVueArtifactLowerer.cs      # Lowering 接口
├── Artifacts/                      # 编译产物
│   ├── VueCompiledArtifact.cs
│   ├── VueArtifactIdentity.cs
│   ├── VueRuntimeHints.cs
│   └── RazorVueCatalog.cs
├── Extensibility/                  # 扩展点
│   ├── IRazorSemanticFrontend.cs
│   └── IRazorVueArtifactLowerer.cs
├── RazorVuePipeline.cs             # 管线入口
├── RazorVueCompilationContext.cs   # 编译上下文
└── IVueComponent.cs                # 组件基础接口

src/Jazor.RazorVue.Analysis/
└── src/Jazor.Analyzer/RazorVue/Generation/RazorVueGenerator.cs  # Source Generator

src/Jazor.RazorVue.Vuetify/
└── Components/                     # 35 个 Vuetify 组件桩
    ├── VBtn.cs
    ├── VCard.cs
    ├── VTextField.cs
    └── ...
```

### 7.2 生成的代码

Source Generator 的目标生成形状为：

```
$(GeneratedCode)/Jazor.Generated.RazorVue.Artifact_<stable-id>.g.cs
├── 单组件 VueSfcArtifact carrier
│   ├── SfcText / TemplateBlock / ScriptSetupBlock
│   ├── StyleBlocks / CustomBlocks
│   ├── Identity / Hints
│   └── SourceOrigins

$(GeneratedCode)/Jazor.Generated.RazorVueCatalog.g.cs
├── RazorVueCatalog 静态类
│   ├── AssemblyName
│   └── GetArtifacts()             # 只聚合 artifact provider
```

## 8. 关键依赖

### 8.1 NuGet 包依赖

| 包名 | 版本 | 用途 |
|------|------|------|
| `Microsoft.CodeAnalysis.CSharp` | $(CodeAnalysisVersion) | Roslyn C# 编译器平台 |
| `Microsoft.CodeAnalysis.Analyzers` | $(CodeAnalysisVersion) | 分析器基础包 |

### 8.2 项目引用

| 项目 | 用途 |
|------|------|
| `Jazor.Compiler` | C# 到 JavaScript 核心编译器 |
| `Jazor.Razor` | Razor 基础层桥接 |

### 8.3 框架依赖

| 框架 | 版本 | 用途 |
|------|------|------|
| `.NETStandard` | 2.0 | RazorVue 核心语义层 |
| `.NET` | 10.0 | Vuetify 组件桩 (需要 ASP.NET Core) |

## 9. 扩展点

RazorVue 提供以下扩展点，允许第三方定制：

### 9.1 IRazorSemanticFrontend

```csharp
public interface IRazorSemanticFrontend
{
    bool CanHandle(Compilation compilation);
    ImmutableArray<RazorVueSemanticSnapshot> CreateSemanticSnapshots(Compilation compilation);
}
```

**用途**: 自定义语义快照提取逻辑，支持非标准的组件发现方式。

### 9.2 IRazorVueArtifactLowerer / IRazorVueSfcArtifactLowerer

```csharp
public interface IRazorVueArtifactLowerer
{
    VueCompiledArtifact Lower(RazorVueCompilationContext context, RazorVueSemanticSnapshot snapshot);
    VueCompiledArtifact Lower(RazorVueSemanticSnapshot snapshot);
}

public interface IRazorVueSfcArtifactLowerer
{
    VueSfcArtifact Lower(RazorVueCompilationContext context, RazorVueSemanticSnapshot snapshot);
}
```

**用途**:
- `IRazorVueArtifactLowerer` 仍承载 legacy render module 过渡车道。
- `IRazorVueSfcArtifactLowerer` 承载库模式 design-time SFC 主线的 canonical-to-SFC materialisation。

## 10. 诊断规则

RazorVue 提供以下诊断规则（在 `src/Jazor.Analyzer/RazorVue/Generation/RazorVueGenerator.cs` 中定义）：

| 规则 ID | 标题 | 严重性 |
|---------|------|--------|
| JAZORVGA001 | RazorVue catalog generation failed | Error |
| JAZORVGA002 | RazorVue component not found | Error |
| JAZORVGA003 | RazorVue component name is ambiguous | Error |
| JAZORVGA004 | RazorVue component name collides with intrinsic | Error |
| JAZORVGA005 | RazorVue lifecycle lowering is unsupported | Error |
| JAZORVGA006 | RazorVue setup logic lowering is unsupported | Error |
| JAZORVGA007 | RazorVue parameter is unknown | Error |
| JAZORVGA008 | RazorVue bind target is invalid | Error |
| JAZORVGA009 | RazorVue child content parameter is unknown | Error |
| JAZORVGA010 | RazorVue child content parameter context is invalid | Error |
| JAZORVGA011 | RazorVue child content parameter is assigned multiple times | Error |
| JAZORVGA012 | RazorVue library component declaration is invalid | Error |
| JAZORVGA013 | RazorVue library style dependency declaration is invalid | Error |
| JAZORVGA014 | RazorVue library plugin requirement declaration is invalid | Error |

## 11. 后续工作

根据 `docs/02-计划/razorvue/` 中的规划，RazorVue 的后续工作包括：

- [ ] 完整的生命周期方法 Lowering (OnInitialized, OnParametersSet, OnAfterRender, etc.)
- [ ] setup 函数中的逻辑字段和方法 Lowering
- [ ] 更完整的表达式覆盖 (复杂 LINQ, 异步流, etc.)
- [ ] 优化 SourceMap 生成，支持精确的断点映射
- [ ] 支持更多 Vuetify 组件 (当前 35 个，目标 50+)

## 12. 相关文档

- **快速入门**: `docs/01-目标/razorvue/README.md`
- **编译器架构**: `docs/01-目标/compiler/ArchitectureOverview.Simplified.md`
- **Jolt 全功能模式**: `docs/01-目标/jolt/README.md`
- **SemanticWalker 转换规范**: `docs/01-目标/compiler/semantic-walker/`

---

**文档维护者**: developerhan  
**最后更新**: 2026-04-21  
**文档版本**: v1.0
