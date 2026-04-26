# RazorVue 设计

本文档定义跨 Jazor 编译器、Razor 基质、Vue 创作库和 RazorVue 分析层的 RazorVue 设计。

它主要是架构文档。
它不试图逐行镜像每个当前实现细节。

当前仓库已经跨越了 RazorVue 的核心主管道里程碑，包括：

- 入口发现和分析器拆分
- `JAZORVUE001`、`JAZORVUE002`、`JAZORVUE004`、`JAZORVUE005` 和 `JAZORVUE006` 的 Roslyn 入口/误用诊断
- 描述符提取
- `RazorVueCatalog` 生成
- emit/清单物化
- 基于 `RazorVueCompilationContext -> RazorVueSemanticSnapshot -> RazorVuePipeline -> RazorVueArtifactFactory` 的真实 Vue 渲染函数发送车道
- 经过验证的组件节点降低，涵盖组件节点、props、事件/监听器连线和默认/命名/作用域插槽流
- `if` / `foreach` 的最小结构降低
- `OnInitialized*`、`OnParametersSet*` 和 `OnAfterRender*` 的生命周期安全子集降低
- 工件标识/哈希塑造和基本 HMR 边界分类

仓库尚未跨越第一阶段关闭里程碑。
不支持的提取/降低形状仍会在广义情况下回退到通用 `JAZORVGA001` 诊断表面，但瘦 `Jazor.RazorVue.Analysis` 主机路径现在为 `JAZORVGA002`（组件未找到）、`JAZORVGA003`（歧义短组件名称）、`JAZORVGA004`（保留内置名称冲突）和 `JAZORVGA005`（不支持的生命周期降低）投影结构化编译器问题。

本文档的存在是为了：

1. 定义 RazorVue 解决的问题
2. 定义 RazorVue 应该和不应该在哪些层中存在
3. 在实现开始之前修复职责
4. 为后续 `DenoHost` 集成提供稳定的契约

> 延伸阅读：[RazorVue.DecisionSummary.md](./RazorVue.DecisionSummary.md) | [RazorVue.HardRules.md](./RazorVue.HardRules.md) | [RazorVue.ImplementationChecklist.md](../../../02-计划/jolt/razorvue-implementation/RazorVue.ImplementationChecklist.md) | [RazorVue.Pitfalls.md](./RazorVue.Pitfalls.md) | [RazorVue.Authoring.ProductDefinition.md](./RazorVue.Authoring.ProductDefinition.md) | [RazorVue.LibraryAuthoring.Design.md](./RazorVue.LibraryAuthoring.Design.md)

## 目录

- [0-项目拆分](#0-项目拆分)
- [1-目标](#1-目标)
- [2-非目标](#2-非目标)
- [3-定位](#3-定位)
- [4-入口模型](#4-入口模型)
- [5-基础类层次结构](#5-基础类层次结构)
- [6-为什么-razorvue-不解析-razor](#6-为什么-razorvue-不解析-razor)
- [7-编译时入口策略](#7-编译时入口策略)
- [8-为什么-razor-组件需要自己的降低](#8-为什么-razor-组件需要自己的降低)
- [9-高层管道](#9-高层管道)
- [10-组件契约提取](#10-组件契约提取)
- [11-逻辑提取](#11-逻辑提取)
- [12-渲染树提取](#12-渲染树提取)
- [13-模板语义](#13-模板语义)
- [14-生命周期语义](#14-生命周期语义)
- [15-vue-输出模型](#15-vue-输出模型)
- [16-生态系统扩展](#16-生态系统扩展)
- [17-工件和清单模型](#17-工件和清单模型)
- [18-hmr-和-sourcemap-保留](#18-hmr-和-sourcemap-保留)
- [19-denohost-边界](#19-denohost-边界)
- [20-第一阶段范围](#20-第一阶段范围)
- [21-设计结论](#21-设计结论)

## 0. 项目拆分

公共项目拆分为：

- `Jazor.Compiler`
  拥有编译器核心编排、共享契约、工件塑造、静态模块生成和扩展点
- `Jazor.Razor`
  拥有面向 Razor 的基础组件基质（`JazorComponent`）
- `Jazor.RazorVue`
  拥有面向 Vue 的创作基质（`VueComponent`）加上 RazorVue 核心语义车道：前端发现、生成的 Razor 分析、描述符提取、源出处、渲染树提取和 Vue 工件降低
- `Jazor.RazorVue.Analysis`
  拥有瘦 RazorVue 生成器/分析器面向主机入口：仅 Roslyn 连线和诊断投影到编译器管道

此拆分是有意的：

- 面向创作的运行时类型不属于编译器核心
- Vue 分析入口不属于 Vue 创作运行时
- 现有的静态模块路径与 RazorVue 分析增长隔离

## 1. 目标

RazorVue 的存在是让 Razor 组件进入 ECMAScript 前端编译域并成为 Vue 优先的组件模块。

目标链是：

`[ECMAScriptModule] Razor Component -> Vue component artifact -> DenoHost unified compile`

设计目标是：

- 保持 Razor 作为面向创作的模板语法
- 使用 Vue 作为真实运行时组件模型
- 保持构建所有权与 `DenoHost`
- 发送稳定的 Vue ESM 工件
- 保留足够的元数据以支持后续 HMR 和 sourcemap

## 2. 非目标

第一阶段明确不尝试执行以下操作：

1. 为 React/Vue/Svelte 构建跨框架 UI 抽象
2. 完全模拟 Blazor 运行时语义
3. 在编译器中构建新的打包器
4. 输出 `.vue` SFC 作为主要工件
5. 完全解决 SSR/hydration 策略
6. 完全支持所有 Razor 语法
7. 完全实现 sourcemap 或 HMR 运行时行为

## 3. 定位

RazorVue 是 Vue 优先。

这意味着：

- Razor 是模板前端
- Vue 是语义/运行时目标
- Blazor 熟悉性仅在改善创作采用时保留

这不是：

- 通用 UI 编译器
- Vue 之上的 Blazor 运行时克隆
- 隐藏的多框架抽象层

## 4. 入口模型

### 4.1 统一入口

`[ECMAScriptModule]` 仍然是进入前端编译的统一标记。

其含义从：

- "普通 ECMAScript 静态模块"

扩展为：

- "此类型参与 ECMAScript 前端输出"

### 4.2 入口后必需拆分

检测到 `[ECMAScriptModule]` 后，编译器必须拆分为两条路径。

#### 普通静态模块路径

输入：

- `static class`
- 当前 ECMAScript 模块契约

输出：

- 当前普通 ECMAScript 模块工件流

#### Razor 组件路径

输入：

- Razor 组件
- 继承 `JazorComponent`
- 标记有 `[ECMAScriptModule]`

输出：

- Vue 优先组件工件流

## 5. 基础类层次结构

层次结构固定为：

`ComponentBase -> JazorComponent -> VueComponent`

### 5.1 `JazorComponent`

`JazorComponent` 是 Jazor 前端编译的 Razor 组件标识基础。

职责：

- 定义"此组件属于 Jazor 前端管道"
- 托管共享的 Razor 组件契约边界
- 托管 Blazor 生命周期糖入口点

非职责：

- Vue 特定 API
- 运行时调度
- 构建主机逻辑
- 状态更新编排

### 5.2 `VueComponent`

`VueComponent` 是 Vue 优先创作基础类。

职责：

- 托管 Vue 优先创作 API
- 为分析器/降低提供稳定的符号表面
- 将 Vue 语义与通用 Razor 组件标识分离

预期的 API 表面包括：

- `Ref`
- `Reactive`
- `Computed`
- `Watch`
- `WatchEffect`
- `NextTick`
- `OnMounted`
- `OnUpdated`
- `OnUnmounted`
- `Emit`
- `Provide`
- `Inject`
- `Expose`

## 6. 为什么 RazorVue 不解析 `.razor`

第一阶段不引入自定义 `.razor` 解析器。

原因：

1. Razor 已经有工具链和生成代码模型。
2. 主要挑战是 Vue 降低，而不是重建 Razor 解析。
3. 并行 `.razor` 解析器将增加维护范围和语义漂移。
4. 当前实用的集成点是生成的 Razor 组件代码。

因此主要输入是：

- 组件符号
- 生成的 `BuildRenderTree(RenderTreeBuilder)` 操作
- 代码隐藏符号/操作数据

## 7. 编译时入口策略

### 7.1 不依赖源生成器排序

RazorVue 不得依赖源生成器排序。

具体来说，第一阶段车道不依赖：

- Razor 源生成器首先生成 C#
- 另一个源生成器然后可以消费其输出

### 7.2 分析器作为语义提取入口点

RazorVue 使用基于分析器的语义提取，并启用生成代码分析。

分析器负责：

- 发现有效的 RazorVue 组件
- 提取符号和操作
- 早期报告无效用法

### 7.2.1 分析器不是语义传输

分析器是验证和提取的语义入口点，
但它并非跨阶段传输机制。

这种区别必须保持显式，因为：

- 分析器报告诊断，它们不定义面向构建的工件载体
- 当前仓库已经有下游主机路径消费编译器拥有的模块元数据
- 让分析器语义泄漏到临时主机移交将立即创建隐藏耦合

### 7.2.2 必需的语义载体

阶段一需要在以下内容之间有显式编译器拥有的载体：

1. 语义提取
2. Vue 降低
3. 面向构建的物化

推荐的内部阶段是：

- `RazorVueSemanticSnapshot`
- `VueCompiledArtifact`
- `RazorVueCatalog` 或等效面向主机的载体

推荐职责拆分：

- 分析器验证并提取语义输入
- 编译器拥有的提取/降低构建 `RazorVueSemanticSnapshot`
- 降低将快照转换为 `VueCompiledArtifact`
- 后续面向构建的阶段物化 `RazorVueCatalog` 和清单/侧车
- `DenoHost` 仅消费物化的编译器拥有的输出

重要规则不是确切的类型名称。
它是管道必须具有显式语义载体，而不是：

- 在后续阶段重复重新分析
- 直接分析器到主机耦合
- 临时仅字符串移交

### 7.2.3 生产和消费表面

阶段一还必须定义每个载体在哪里生产和消费。

推荐实现边界：

1. 分析器仅作为诊断/发现运行
2. 编译器拥有的提取驱动器从最终编译视图构建 `RazorVueSemanticSnapshot`
3. 降低消费 `RazorVueSemanticSnapshot` 并生产 `VueCompiledArtifact`
4. 目录/物化阶段消费 `VueCompiledArtifact` 并发送 `RazorVueCatalog` 加上清单/侧车
5. `DenoHost` 仅消费物化的编译器拥有的输出

重要约束：

- `RazorVueSemanticSnapshot` 不得依赖隐藏的分析器状态
- `VueCompiledArtifact` 不得由 `DenoHost` 重建
- 编译器拥有的提取必须在生成的 Razor 组件代码已经可用的编译视图上运行

第一阶段不需要冻结驱动器的最终类名，
但它确实需要冻结此生产/消费拆分。

### 7.3 构建所有权保持与 `DenoHost`

编译器提取和发送元数据/工件。
`DenoHost` 执行后续统一构建。

这也暗示了具体的实现约束：

- 分析器是语义提取入口
- 分析器不是物理工件写入器
- 最终模块/清单元数据物化必须在后续编译器拥有的构建步骤或等效面向主机的发送阶段发生

### 7.4 与当前模块路径的迁移边界

仓库已经有一个工作的普通 ECMAScript 模块路径，基于生成的目录元数据。

阶段一 RazorVue 必须因此定义迁移边界，而不是假设绿地主机路径。

推荐规则：

- 为静态模块保留当前普通 `ModuleCatalog` 路径
- 添加并行 RazorVue 面向主机的目录或版本化超集载体
- 让下游主机/发送代码在过渡期间消费两者

在第一个 Vue 路径证明之前，不要要求阶段一 RazorVue 车道替换整个现有模块目录流。

## 8. 为什么 Razor 组件需要自己的降低

尽管分析器可以看到生成的 Razor 组件符号和操作，
Razor 模板通过 `BuildRenderTree(RenderTreeBuilder)` 构建器调用表示，而不是直接用户创作的方法体。

这意味着 RazorVue 不能安全地重用当前的普通静态模块降低路径。

RazorVue 需要自己的阶段：

1. 组件发现
2. 契约提取
3. 从 `BuildRenderTree` 渲染树提取
4. Vue 降低
5. 工件发送

## 9. 高层管道

推荐的 RazorVue 管道是：

1. `ComponentDiscovery`
2. `ContractExtraction`
3. `LogicExtraction`
4. `RenderTreeExtraction`
5. `VueLowering`
6. `ArtifactEmission`
7. `DenoHost`

每个阶段必须具有稳定的输入/输出，并且不得直接折衷为字符串生成。

## 10. 组件契约提取

每个 RazorVue 组件需要显式的面向 Vue 的契约模型。

该契约必须描述：

- 组件标识
- 导入/导出标识
- props
- emits
- 插槽
- 模型/绑定元数据
- 样式依赖
- 一小组标志

推荐结构是 `VueComponentDescriptor` 风格模型。

### 10.1 映射规则

- `[Parameter]` 普通属性 -> prop
- `EventCallback*` -> emit
- `RenderFragment` -> 默认或命名插槽
- `RenderFragment<T>` -> 作用域插槽
- `Foo + FooChanged` -> 绑定/模型元数据
- 显式 `Emit("...")` 用法 -> 附加 emit 契约信息

此契约必须在渲染树降低之前提取。

## 11. 逻辑提取

逻辑提取与渲染提取分离。

它涵盖：

- 字段
- 方法
- `Ref/Reactive/Computed`
- 生命周期糖
- `Emit`
- `Provide/Inject`
- `Expose`
- 观察者/效果

此阶段馈送 Vue `setup` 降低。

它不应尝试重建模板结构。

## 12. 渲染树提取

`BuildRenderTree` 不是发送的 Vue 代码。
它只是模板结构的生成的 Razor 表示。

RazorVue 因此需要最小的中间渲染树模型。

该模型应该至少捕获：

- 元素节点
- 组件节点
- 文本节点
- 表达式节点
- 条件节点
- 循环节点
- 属性节点
- 插槽内容节点

此提取阶段必须从构建器调用模式恢复稳定结构，如：

- `OpenElement`
- `CloseElement`
- `OpenComponent`
- `CloseComponent`
- `AddAttribute`
- `AddContent`

## 13. 模板语义

RazorVue 模板语义是 Vue 优先。

关键规则：

- 小写标签 -> HTML 元素
- 大写标签 -> Vue 组件
- `Teleport` / `Transition` / `KeepAlive` / `Suspense` -> 内置
- `@bind` -> Vue `v-model`
- `@ref` -> 模板引用
- `@key` -> vnode 键
- `RenderFragment*` -> Vue 插槽

这不是 Blazor 渲染树兼容性目标。

## 14. 生命周期语义

Vue 是真实的生命周期模型。

Blazor 生命周期成员仅保留为编译时糖。
当前实现不尝试在 Vue `setup()` 内重建完整的组件实例运行时。
相反，它只接受可以投影到 Vue 钩子或具有稳定闭包语义的观察者的生命周期形状。

### 14.1 当前支持的安全子集

当前实现的生命周期安全子集是：

- `OnInitialized` / `OnInitializedAsync` -> `onMounted(...)`
- `OnParametersSet` / `OnParametersSetAsync` -> `watch(() => [props...], ..., { immediate: true })`
- `OnAfterRender` / `OnAfterRenderAsync` -> `onMounted(...)` + `onUpdated(...)`

`OnAfterRender*` 还携带显式 `firstRender` 桥接。
生成的 Vue 代码在 emit 有效负载使用之前快照当前标志，然后重置共享标志，以便后续更新无法观察到陈旧的首次渲染状态。

### 14.2 当前降低边界

安全子集只接受降低为 EventCallback 驱动的 emit 桥接或等效无操作形状的生命周期体。
重要规则不是"生命周期存在"，而是"生命周期可以降低，而无需在 Vue setup 内假装完整的 Blazor 组件实例存在"。

这意味着当前车道有意拒绝依赖于以下内容的生命周期体：

- 任意实例字段或一般实例状态突变
- 支持的有效负载发送子集之外的辅助方法
- 无法映射到 Vue emit/watch/hook 计时的不支持的有效负载表达式
- `Dispose*`、`ShouldRender` 或 `SetParametersAsync` 的运行时等效处理

这些形状应该通过结构化诊断显式失败，而不是静默生成不正确的钩子。

### 14.3 延迟的生命周期表面

以下生命周期相关区域仍在当前主管道之外：

- `Dispose*` 运行时降低
- `ShouldRender` 运行时等效
- `SetParametersAsync` 运行时等效
- 超出 EventCallback 有效负载桥接子集的一般组件实例生命周期逻辑

目标是稳定的、可解释的行为，而不是完整的 Blazor 运行时等效。

## 15. Vue 输出模型

第一阶段输出固定为带有 `defineComponent + setup + render` 的标准 Vue ESM。

规范形状是：

```js
export default defineComponent({
  name: "...",
  props: { ... },
  emits: [ ... ],
  setup(props, { emit, slots, expose, attrs }) {
    return () => h(...);
  }
})
```

第一阶段不定位 `.vue` SFC。

## 16. 生态系统扩展

后续 Vue 生态系统包可以通过描述符/注册表风格集成扩展编译器。

示例：

- `ECMAScript.Vue.Vuetify`
- `ECMAScript.Vue.Router`
- `ECMAScript.Vue.Pinia`

它们的角色是：

- 组件描述符注册
- 附加导入/样式声明
- Vue 生态系统特定的创作辅助

它们不重新定义核心 RazorVue 管道。

## 17. 工件和清单模型

编译器不应将结果视为松散的 JS 字符串。

必须存在结构化工件，应该至少包括：

- 组件名称
- 相对模块路径
- 模块内容
- 导入依赖
- 样式依赖
- 内容哈希
- 运行时提示

`DenoHost` 消费的清单应从这些工件派生，而不是独立重建。

在第一阶段车道中，"工件发送"意味着两个不同的职责：

1. 语义/降低阶段生产结构化工件模型
2. 后续面向构建的发送阶段为 `DenoHost` 物化这些工件

这种区别很重要，因为分析器是语义提取的一部分，而不是最终文件写入主机。

## 18. HMR 和 SourceMap 保留

第一阶段不需要完整的 HMR 或 sourcemap 行为，
但管道必须保留所需的元数据。

### 18.1 源始保留

管道应该为以下内容保留源始元数据：

- 渲染树节点
- 组件逻辑绑定
- 生命周期绑定
- Vue 降低节点
- 工件输出锚点

至少，始类别应区分：

- `razor-template`
- `component-logic`
- `generated-render`

但始类别 alone 不够。

第一阶段应该将源始数据保留为结构化跨度或稳定引用，而不仅仅是标签。

推荐的最小源始条目形状：

```csharp
public sealed record RazorVueSourceOrigin(
    RazorVueOriginKind OriginKind,
    string SourceFilePath,
    int SourceSpanStart,
    int SourceSpanLength,
    int StartLine,
    int StartColumn,
    string? GeneratedFilePath,
    int? GeneratedSpanStart,
    int? GeneratedSpanLength,
    RazorVueMappingQuality MappingQuality);
```

推荐的映射质量值：

- `ExactSource`
- `MappedFromGenerated`
- `GeneratedOnly`

第一阶段不需要为每个节点完美的源恢复，
但它必须保留节点是否具有：

- 精确的 `.razor` 源标识
- 生成代码派生的映射
- 仅生成的回退标识

没有这种区别，后续 sourcemap 和 HMR 工作仍将需要重新设计。

### 18.1.1 源始出处策略

阶段一源始数据应该通过分层出处策略生成。

推荐顺序：

1. 在可用时使用 Razor 工具链/源映射数据
2. 否则使用绑定到 Razor 生成文件的生成 C# 语法/操作位置
3. 否则回退到仅生成始记录

这意味着阶段一车道不承诺为每个节点精确的 `.razor` 映射。
它确实承诺每个节点记录哪个出处层产生了其始数据。

### 18.2 HMR 保留

工件还应保留稳定的标识信息，如：

- `ComponentId`
- `ModuleId`
- `DescriptorHash`
- `TemplateHash`
- `LogicHash`
- `HmrBoundaryKind`

这些不是阶段一运行时功能，但它们是阶段一结构要求。

### 18.3 HMR/sourcemap 不得重新定义主降低

HMR 和 sourcemap 必须保持元数据扩展。

它们不应成为以下内容的主要驱动者：

- 组件语义设计
- 渲染树提取形状
- Vue 降低结构

## 19. `DenoHost` 边界

编译器职责：

- 语义提取
- Vue 组件生成
- 契约生成
- 工件生成
- 清单元数据生成

`DenoHost` 职责：

- 依赖解析
- 统一编译
- 打包
- 运行时集成
- 后续 HMR 和 sourcemap 主机行为

编译器和主机职责必须保持不同。

### 19.1 源始侧车是边界的一部分

编译器/主机边界应该允许源始数据跨越阶段，而无需强制 `DenoHost` 从最终 JS 文本反向工程。

阶段一推荐：

- 工件直接携带源始条目，或
- 工件引用编译器拥有的侧车，如 `*.jzrmap.json`

确切的文件格式可能会在 later 演变。
架构要求是源始数据保持编译器拥有和主机可消费。

## 20. 第一阶段范围

阶段一关闭只需要最小循环：

- RazorVue 组件发现
- `JazorComponent` / `VueComponent` 约束
- props/emits/slots 提取
- 最小渲染树恢复
- `@bind`、`@ref`、`@key`
- `if`、`foreach`
- 生命周期糖降低
- Vue 渲染函数 ESM 发送
- 工件 + 清单元数据生成
- `DenoHost` 消费路径

### 20.1 当前实现里程碑检查点

当前实现已经在这些领域跨越了核心主管道里程碑：

- RazorVue 组件发现
- `JazorComponent` / `VueComponent` 约束
- props / emits / slots 提取
- 带有结构化未找到/歧义/保留名称问题表面的组件解析
- 真实的 Vue ESM 工件发送
- 工件 + 清单元数据生成
- 面向 `DenoHost` 的发送侧主机移交形状
- 组件节点降低，带有 props、监听器和默认/命名/作用域插槽流
- `if` / `foreach` 的最小控制流降低
- `OnInitialized*`、`OnParametersSet*` 和 `OnAfterRender*` 的生命周期安全子集降低

当前实现在语义上仍然有意窄。
它只声称通过代码和测试证明的子集：

- 从 `BuildRenderTree` 重建的元素和组件节点
- 参数支持的模板表达式
- props / emits / slots / 绑定相邻连线
- 生命周期 EventCallback 有效负载桥接，包括 `watch(..., { immediate: true })` 和显式 `firstRender` 处理

当前实现尚未跨越以下方面的第一阶段关闭里程碑：

- 生命周期/EventCallback 安全子集之外的更广泛逻辑提取
- `setup` 内的完整组件实例语义
- `Dispose*`、`ShouldRender` 或 `SetParametersAsync` 运行时等效降低
- 全面的 Razor 语法覆盖
- 最终 sourcemap 输出
- 运行时 HMR 行为
- 最终 `DenoHost` 端到端验证

延迟的工作包括：

- 完整的生态系统集成
- 深度 SSR/hydration 策略
- 完整的 HMR 运行时
- 完整的 sourcemap 发送
- `.vue` SFC 输出
- 通用多框架抽象

## 21. 设计结论

RazorVue 不是"Razor 加上一点 Vue 支持"。

它是一个专用的 Vue 优先编译路径，它：

- 重用 Razor 作为创作语法
- 使用分析器而非生成的 Razor 组件代码
- 提取稳定的组件契约和渲染结构
- 发送标准 Vue ESM 工件
- 将统一构建所有权交给 `DenoHost`
