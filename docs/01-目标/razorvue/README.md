# RazorVue — 库模式

> 对应源码：`src/Jazor.RazorVue/`、`src/Jazor.RazorVue.Analysis/`、`src/Jazor.RazorVue.Vuetify/`

## 为什么需要

不是所有场景都需要完整的开发服务器和 HMR。很多项目只需要在编译时把 Razor 组件转成 JavaScript，像使用普通 NuGet 库一样集成到现有构建流程中。RazorVue 就是这个"轻量版"——通过 Source Generator 在编译时完成一切，无需额外进程或开发服务器。

## 解决什么问题

1. **编译时转换**：Razor 组件在 `dotnet build` 时自动转换为 JavaScript，无需运行时或开发服务器
2. **零配置集成**：安装 NuGet 包即可，Source Generator 自动注册，不需要额外工具链
3. **Vuetify UI 库**：提供 35 个 Vuetify 3 组件的 C# 包装，用 Razor 语法编写 Material Design 界面
4. **库模式发布**：转换结果作为库的一部分输出，可被其他项目引用

## 大致实现思路

### 核心区别：不使用 .vue SFC

RazorVue 的核心设计选择是**不生成 .vue 单文件组件**。Razor 组件直接转换为纯 JavaScript/TypeScript 模块，跳过 Vue SFC 编译步骤：

```
Razor 组件 (.razor)
     ↓ Source Generator（编译时自动触发）
     ↓ Roslyn 分析 + 语义提取
     ↓ Jazor.RazorVue 核心语义转换
JavaScript 模块（纯 JS/TS，非 .vue SFC）
     ↓ 嵌入到程序集或输出到项目
作为 NuGet 库的一部分发布
```

### 三个子项目

**Jazor.RazorVue（核心语义）**
- 定义组件模型：属性映射、事件绑定、子内容插槽
- 处理 Razor 语法到 JavaScript 的语义转换
- 不依赖 .vue SFC 格式，直接输出 JS 模块

**Jazor.RazorVue.Analysis（编译时分析）**
- 薄 Roslyn 宿主，在编译时验证 RazorVue 组件的正确性
- 生成组件描述信息供 Source Generator 使用
- 输出诊断信息到 IDE

**Jazor.RazorVue.Vuetify（UI 组件库）**

Vuetify 3 组件的 C# 包装：

```csharp
[VueLibraryComponent("vuetify/components", "VBtn")]
[VueLibraryStyle("vuetify/styles")]
[VueLibraryPluginRequirement("vuetify")]
public partial class VBtn : VuetifyComponentBase
{
    [Parameter] public string? Text { get; set; }
    [Parameter] public bool Disabled { get; set; }
    [Parameter] public EventCallback OnClick { get; set; }
}
```

已包装 35 个组件：VBtn、VCard、VDialog、VDataTable、VTextField、VSelect、VTabs、VToolbar 等。

### 与 Jolt 的对比

| 维度 | RazorVue（库模式） | Jolt（全功能模式） |
|------|-------------------|---------------------|
| 触发方式 | Source Generator（编译时） | 独立进程（LSP + DevServer） |
| 输出格式 | 纯 JS/TS 模块 | .vue SFC + JS/CSS |
| 开发热更新 | 无（需要重新编译） | HMR（< 500ms） |
| 调试支持 | 无 | DAP + CDP 源码级调试 |
| LSP 智能提示 | 无（仅 Roslyn 分析） | 3-Lane 全语义（Jazor + Roslyn + Volar） |
| 适用场景 | 库开发、CI 构建 | 应用开发、实时预览 |

## 功能设计文档索引

以下是从代码实现反推编写的细粒度设计文档，覆盖 RazorVue 全部子系统：

### 整体架构
| 文档 | 覆盖范围 |
|------|---------|
| [architecture.md](architecture.md) | 整体架构：3 个项目、依赖图、Source Generator 管线、与 Jolt 对比 |

### 核心类型 (`core/`)
| 文档 | 覆盖范围 |
|------|---------|
| [AttributesAndInterfaces.md](core/AttributesAndInterfaces.md) | 6 个 VueLibrary 属性 + IVueComponent/IVueLibraryComponent + 扩展性接口 |
| [Enums.md](core/Enums.md) | 所有枚举：14 种类型（EntryKind, PropKind, EmitKind, Flags, HmrBoundary 等） |

### 描述符系统 (`descriptor/`)
| 文档 | 覆盖范围 |
|------|---------|
| [ComponentDescriptor.md](descriptor/ComponentDescriptor.md) | VueComponentDescriptor + Prop/Emit/Slot/Lifecycle/Logic 子描述符 |
| [DescriptorFactory.md](descriptor/DescriptorFactory.md) | VueComponentDescriptorFactory：Roslyn 符号 -> 描述符，绑定推断，库组件元数据 |
| [ComponentRegistry.md](descriptor/ComponentRegistry.md) | VueComponentRegistry：三级索引，命名空间可见性，解析优先级 |
| [IntrinsicComponents.md](descriptor/IntrinsicComponents.md) | 4 个内置 Vue 组件：Teleport, Transition, KeepAlive, Suspense |
| [CompilationIssues.md](descriptor/CompilationIssues.md) | 14 个诊断代码，IssueException，ResolutionIssueFactory |

### 发现与分类 (`discovery/`)
| 文档 | 覆盖范围 |
|------|---------|
| [EntryClassifier.md](discovery/EntryClassifier.md) | RazorVueEntryClassifier：ECMAScriptModule 检测，入口分类，生命周期/逻辑发现 |
| [CompilationSymbols.md](discovery/CompilationSymbols.md) | RazorVueCompilationSymbols：必须/可选符号，回退元数据名解析 |
| [CompilationContext.md](discovery/CompilationContext.md) | RazorVueCompilationContext：组件发现，快照创建，注册表构建 |

### 制品系统 (`artifacts/`)
| 文档 | 覆盖范围 |
|------|---------|
| [CompiledArtifact.md](artifacts/CompiledArtifact.md) | VueCompiledArtifact + ArtifactIdentity + RuntimeHints + HmrBoundaryKind |
| [SemanticSnapshot.md](artifacts/SemanticSnapshot.md) | RazorVueSemanticSnapshot：组件语义模型（17 个字段） |
| [Catalog.md](artifacts/Catalog.md) | RazorVueCatalog + CatalogBuilder：路径规范化，排序 |
| [SourceOrigin.md](artifacts/SourceOrigin.md) | RazorVueSourceOrigin：源码映射，OriginKind，MappingQuality |

### 渲染树 (`render-tree/`)
| 文档 | 覆盖范围 |
|------|---------|
| [RenderTree.md](render-tree/RenderTree.md) | 框架无关渲染树模型：9 种节点类型（Element, Component, Text, Expression, Slot, Conditional, ForEach, Attribute） |
| [RenderTreeExtractor.md](render-tree/RenderTreeExtractor.md) | BuildRenderTree 解析：RenderTreeBuilder 调用识别，栈式树构建 |

### 降级系统 (`lowering/`)
| 文档 | 覆盖范围 |
|------|---------|
| [ArtifactFactory.md](lowering/ArtifactFactory.md) | RazorVueArtifactFactory：组件解析，SHA256 哈希，import/style 构建 |
| [ModuleBuilder.md](lowering/ModuleBuilder.md) | defineComponent 代码生成（~1100行）：setup + render 函数 |
| [ExpressionEmitter.md](lowering/ExpressionEmitter.md) | IOperation -> JS 表达式翻译：渲染/setup 双模式，运算符映射 |
| [ComponentAuthoring.md](lowering/ComponentAuthoring.md) | h() 调用生成：元素/组件/槽/循环，库组件编译时验证 |
| [LifecycleLowering.md](lowering/LifecycleLowering.md) | Blazor -> Vue hooks 映射：6 种生命周期，深度分析方法体 |

### 管线 (`pipeline/`)
| 文档 | 覆盖范围 |
|------|---------|
| [Pipeline.md](pipeline/Pipeline.md) | RazorVuePipeline：3 个 Execute 重载，完整管线编排 |
| [Generator.md](pipeline/Generator.md) | RazorVueGenerator：IIncrementalGenerator，14 个诊断，生成代码结构 |

### Vuetify (`vuetify/`)
| 文档 | 覆盖范围 |
|------|---------|
| [ComponentStubs.md](vuetify/ComponentStubs.md) | 38 个 Vuetify 组件桩：结构模式，属性/事件/槽分类，model binding |

## 设计文档

`design/` 目录下包含 RazorVue 的预实现设计决策、约束和实现规范（与上述实现文档互补）。
