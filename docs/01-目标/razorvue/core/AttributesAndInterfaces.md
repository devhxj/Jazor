# RazorVue 核心属性与接口


## 1. 文档定位

本文档详细说明 RazorVue 核心属性系统和接口设计，这些类型构成了 RazorVue 组件声明式元数据和编译器扩展点的基础。

**核心职责**：
- 提供库组件的声明式元数据系统（Attributes）
- 定义组件基础类型契约（Interfaces）
- 提供编译器扩展点接口（Extensibility）

**源文件位置**：
- 属性定义：`src/ECMAScript.VueContract/*.cs`
- 接口定义：`src/ECMAScript.Vue3/*.cs`
- 扩展性接口：`src/Jazor.RazorVue/Extensibility/` 目录

## 2. 核心类型

### 2.1 VueLibrary 属性系统

RazorVue 提供了一套完整的声明式属性系统，用于描述外部 Vue 库组件的元数据。这些属性允许编译器理解第三方 Vue 组件的契约，而无需实际的 JavaScript 实现。

#### 2.1.1 VueLibraryComponentAttribute

**文件**：`src/ECMAScript.VueContract/VueLibraryComponentAttribute.cs`

**用途**：标记类为 Vue 库组件桩（stub），声明其导入路径和导出名称。

**构造参数**：
- `importSpecifier`（string）：JavaScript 模块导入路径，例如 `"vuetify/lib/components/VBtn/VBtn.mjs"` 或 `"@vueuse/core"`
- `exportName`（string）：模块导出名称，例如 `"VBtn"` 或 `"useScroll"`

**使用示例**：
```csharp
[VueLibraryComponent("vuetify/lib/components/VBtn/VBtn.mjs", "VBtn")]
[VueLibraryComponent("@vueuse/core", "useScroll")]
public sealed class VBtn : IVueLibraryComponent
{
}
```

**特性约束**：
- `AttributeTargets.Class`：仅用于类
- `AllowMultiple = false`：每个类只能应用一次
- `Inherited = false`：不被派生类继承

#### 2.1.2 VueLibraryStyleAttribute

**文件**：`src/ECMAScript.VueContract/VueLibraryStyleAttribute.cs`

**用途**：声明库组件的 CSS 样式依赖。

**构造参数**：
- `styleSpecifier`（string）：样式文件路径，例如 `"vuetify/lib/components/VBtn/VBtn.css"`

**使用示例**：
```csharp
[VueLibraryComponent("vuetify/lib/components/VBtn/VBtn.mjs", "VBtn")]
[VueLibraryStyle("vuetify/lib/components/VBtn/VBtn.css")]
public sealed class VBtn : IVueLibraryComponent
{
}
```

**特性约束**：
- `AllowMultiple = true`：组件可能依赖多个样式文件

#### 2.1.3 VueLibraryPluginRequirementAttribute

**文件**：`src/ECMAScript.VueContract/VueLibraryPluginRequirementAttribute.cs`

**用途**：声明库组件对 Vue 插件的依赖，例如 Vuetify 需要注册插件才能正常工作。

**构造参数**：
- `requirementId`（string）：插件标识符，例如 `"vuetify"` 或 `"vue-router"`

**使用示例**：
```csharp
[VueLibraryComponent("vuetify/lib/components/VBtn/VBtn.mjs", "VBtn")]
[VueLibraryPluginRequirement("vuetify")]
public sealed class VBtn : IVueLibraryComponent
{
}
```

**特性约束**：
- `AllowMultiple = true`：组件可能依赖多个插件

#### 2.1.4 VuePropAttribute

**文件**：`src/ECMAScript.VueContract/VuePropAttribute.cs`

**用途**：声明库组件的 props（属性），包括其类型、是否必填、是否支持 v-model 等。

**构造参数**：
- `publicName`（string）：Vue 组件的 prop 名称，例如 `"color"` 或 `"model-value"`

**可选构造参数**：
- `kind`（VuePropKind）：prop 的语义类型，默认为 `VuePropKind.Normal`

**可设置属性**：
- `Name`（string?）：内部 C# 名称，如果未设置则使用 `publicName`
- `Required`（bool）：是否为必填属性，默认为 `false`
- `AcceptsBinding`（bool）：是否支持 v-model 绑定，默认为 `false`
- `DefaultExpression`（string?）：默认值的 JavaScript 表达式，例如 `"'primary'"` 或 `"undefined"`

**VuePropKind 枚举值**：
- `Normal`：普通属性
- `Model`：v-model 绑定属性（如 `model-value`）
- `HtmlLike`：类 HTML 属性（如 `disabled`、`checked`）
- `LibrarySpecific`：库特定语义属性

**使用示例**：
```csharp
[VueLibraryComponent("vuetify/lib/components/VBtn/VBtn.mjs", "VBtn")]
[VueProp("color", Required = false, DefaultExpression = "'primary'")]
[VueProp("model-value", VuePropKind.Model, AcceptsBinding = true)]
[VueProp("disabled", VuePropKind.HtmlLike)]
public sealed class VBtn : IVueLibraryComponent
{
}
```

**特性约束**：
- `AllowMultiple = true`：组件可以有多个 props

#### 2.1.5 VueLibraryEmitAttribute

**文件**：`src/ECMAScript.VueContract/VueLibraryEmitAttribute.cs`

**用途**：声明库组件的 emits（事件），包括事件名称和负载类型。

**构造参数**：
- `razorAlias`（string）：Razor 中的方法别名，例如 `"OnClick"` 对应 `@onclick`

**可选构造参数**：
- `kind`（VueEmitKind）：事件的语义类型，默认为 `VueEmitKind.Normal`

**可设置属性**：
- `Name`（string?）：Vue 事件名称，如果未设置则从 `razorAlias` 转换（如 `OnClick` → `click`）
- `PayloadTypeName`（string?）：事件负载的类型名称，例如 `"MouseEventArgs"` 或 `"string"`

**VueEmitKind 枚举值**：
- `Normal`：普通事件
- `ModelUpdate`：v-model 更新事件（如 `update:modelValue`）
- `LifecycleLike`：生命周期类事件
- `LibrarySpecific`：库特定语义事件

**使用示例**：
```csharp
[VueLibraryComponent("vuetify/lib/components/VBtn/VBtn.mjs", "VBtn")]
[VueLibraryEmit("click")]
[VueLibraryEmit("update:modelValue", VueEmitKind.ModelUpdate, Name = "update:model-value", PayloadTypeName = "string")]
public sealed class VBtn : IVueLibraryComponent
{
}
```

**特性约束**：
- `AllowMultiple = true`：组件可以有多个 emits

#### 2.1.6 VueSlotAttribute

**文件**：`src/ECMAScript.VueContract/VueSlotAttribute.cs`

**用途**：声明库组件的 slots（插槽），包括插槽名称、是否为默认插槽、作用域上下文等。

**构造参数**：
- `publicName`（string）：Vue 插槽名称，例如 `"default"` 或 `"activator"`

**可设置属性**：
- `Name`（string?）：内部 C# 名称，如果未设置则使用 `publicName`
- `IsDefault`（bool）：是否为默认插槽，默认为 `false`
- `Required`（bool）：是否为必填插槽，默认为 `false`
- `ContextTypeName`（string?）：作用域插槽的上下文类型名称
- `ContextParameterName`（string）：上下文参数名称，默认为 `"context"`

**使用示例**：
```csharp
[VueLibraryComponent("vuetify/lib/components/VDialog/VDialog.mjs", "VDialog")]
[VueSlot("default", IsDefault = true)]
[VueSlot("activator", ContextTypeName = "ActivatorContext", ContextParameterName = "contextProps")]
public sealed class VDialog : IVueLibraryComponent
{
}
```

**特性约束**：
- `AllowMultiple = true`：组件可以有多个 slots

#### 2.1.7 VueLibraryComponentFlagsAttribute

**文件**：`src/ECMAScript.VueContract/VueLibraryComponentFlagsAttribute.cs`

**用途**：声明库组件的特殊标志，如是否支持 v-model、是否为表单控件等。

**构造参数**：
- `flags`（VueComponentFlags）：组件标志位枚举

**VueComponentFlags 枚举值**（[Flags]）：
- `None = 0`：无特殊标志
- `SupportsModelValue = 1`：支持 v-model（model-value prop + update:modelValue 事件）
- `SupportsMultipleModels = 2`：支持多个 v-model 绑定
- `RequiresExplicitChildren = 4`：需要显式子组件（不支持默认插槽）
- `IsDynamicSafe = 8`：可安全用于动态组件（`<component :is="...">`）
- `IsFormControl = 16`：是表单控件（参与表单提交）

**使用示例**：
```csharp
[VueLibraryComponent("vuetify/lib/components/VTextField/VTextField.mjs", "VTextField")]
[VueLibraryComponentFlags(VueComponentFlags.SupportsModelValue | VueComponentFlags.IsFormControl)]
public sealed class VTextField : IVueLibraryComponent
{
}
```

**特性约束**：
- `AllowMultiple = false`：每个类只能应用一次

### 2.2 基础接口

#### 2.2.1 IVueComponent

**文件**：`src/Jazor.RazorVue/IVueComponent.cs`

**用途**：RazorVue 的基础组件类型，同时所在程序集也是 RazorVue 核心语义的归属层。

**继承关系**：
```csharp
public interface IVueComponent : IJazorComponent
{
}
```

**设计意图**：
- `IJazorComponent`：Jazor 编译器基础契约
- `IVueComponent`：空标记接口，用于识别 Vue 组件类型
- 所在程序集（`Jazor.RazorVue`）承载核心语义实现

**架构分层说明**：
> Vue authoring surface 与 RazorVue descriptor/lowering/pipeline 属于同一个产品核心，
> 而 Roslyn generator 入口只是在 Analysis 层做薄接线，不再承载核心实现。

#### 2.2.2 IVueLibraryComponent

**文件**：`src/Jazor.RazorVue/IVueLibraryComponent.cs`

**用途**：外部 Vue 库桩（stub）的基础类型，参与 RazorVue descriptor/registry 管道，但不成为正常的编译 RazorVue 组件条目。

**继承关系**：
```csharp
public interface IVueLibraryComponent : IVueComponent
{
}
```

**使用场景**：
- 标记第三方 Vue 组件的 C# 桩类型
- 这些类型不编译为实际的 JavaScript 模块
- 仅用于提供组件元数据供编译器和 LSP 使用

**使用示例**：
```csharp
[VueLibraryComponent("vuetify/lib/components/VBtn/VBtn.mjs", "VBtn")]
public sealed class VBtn : IVueLibraryComponent
{
    // 无需实现，仅作为元数据载体
}
```

#### 2.2.3 IVueContainerComponent

**文件**：`src/ECMAScript.VueContract/IVueContainerComponent.cs`

**用途**：标记一个 authored 组件是“容器契约组件”。它表达的是抽象槽位，而不是最终必须发射的具体 Vue 组件实现。

**继承关系**：
```csharp
public interface IVueContainerComponent : IUIComponent
{
}
```

**设计意图**：
- 让上层框架先声明稳定的 authoring contract
- 不把 authoring 面直接耦合到 Element Plus / Vuetify / TDesign 某个具体库
- 容器是否被替换、替换成谁，交由编译期注入机制决定

**使用示例**：
```csharp
[ECMAScriptModule("./containers/nav-shell")]
public sealed class NavShell : ComponentBase, IVueComponent, IVueContainerComponent
{
    [Parameter]
    public string? Title { get; set; }
}
```

#### 2.2.4 IVueContainerImplementation<TContainer>

**文件**：`src/ECMAScript.VueContract/IVueContainerImplementation.cs`

**用途**：标记一个具体组件是某个容器契约的编译期实现。

**接口定义**：
```csharp
public interface IVueContainerImplementation<TContainer>
    where TContainer : class, IVueContainerComponent
{
}
```

**设计意图**：
- 用泛型把“这个实现对应哪个 contract”明确写进类型系统
- 让编译器在读取 `[VueInject]` 时做一致性校验
- 避免仅靠命名约定或外部表驱动造成漂移

**使用示例**：
```csharp
[VueLibraryComponent("element-plus", "ElMenu")]
public sealed class ElementPlusNavShell
    : ComponentBase,
      IVueLibraryComponent,
      IVueContainerImplementation<NavShell>
{
    [Parameter]
    public string? Title { get; set; }
}
```

### 2.2.5 VueInjectAttribute

**文件**：`src/ECMAScript.VueContract/VueInjectAttribute.cs`

**用途**：在装配级声明“容器契约组件应注入为哪个具体实现组件”。

**构造参数**：
- `contractComponentType`（Type）：容器契约组件类型
- `implementationComponentType`（Type）：具体实现组件类型

**接口定义**：
```csharp
[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true, Inherited = false)]
public sealed class VueInjectAttribute : Attribute
{
    public VueInjectAttribute(Type contractComponentType, Type implementationComponentType) { ... }
}
```

**使用示例**：
```csharp
[assembly: VueInject(
    typeof(Demo.Containers.NavShell),
    typeof(Demo.Implementations.ElementPlusNavShell))]
```

**编译期约束**：
- 同一个容器契约不能重复声明多个 `[VueInject]`
- implementation 必须在当前 RazorVue 组件注册表中可见
- implementation 必须实现 `IVueContainerImplementation<TContainer>`
- `TContainer` 必须与 `[VueInject]` 的 contract 参数一致
- 容器注入只在“当前解析到的组件就是容器契约本体”时生效；直接引用某个具体实现组件时，不会再次经过注入重写

### 2.2.6 容器注入后的描述符合成规则

容器机制不是简单地“把 contract descriptor 整个替换成 implementation descriptor”。  
如果直接整体替换，authoring 面会泄漏为 Element Plus / Vuetify / TDesign 的具体库细节，容器抽象会立即失效。

RazorVue 当前采用 **merged descriptor** 规则：

- **保留 contract authoring identity**
  - `Name`
  - `FullName`
  - `ResolutionNamespace`
  - `RouteTemplates`
  - `Flags`
- **切换到 implementation runtime surface**
  - `SourceKind`
  - `ImportSpecifier`
  - `ExportName`
  - `StyleDependencies`
  - `PluginRequirements`
- **成员级别逐项合成**
  - `Props`：按 `PublicName` 配对，authoring 语义保留 contract，运行时 prop 名 `Name` 取 implementation
  - `Emits`：按 `RazorAlias` 配对，authoring 语义保留 contract，运行时 emit 名 `Name` 取 implementation
  - `Slots`：按 `PublicName` 配对，authoring 语义保留 contract，运行时 slot 名 `Name` 取 implementation

这意味着：

1. 上层 authoring 永远面对稳定的容器 contract。
2. 具体组件库差异只体现在 runtime prop / event / slot 名称与导入依赖上。
3. 容器实现切换不会把上层框架重新耦合到某个具体库的 authoring metadata。

### 2.2.7 容器 contract 与 implementation 的兼容性规则

`[VueInject]` 不只是声明映射，还会触发编译期兼容性验证。  
当前验证目标是：**implementation 不能收窄 contract 已公开的 authoring 保证。**

当前规则如下：

- **Props**
  - implementation 必须包含 contract 的每个 `PublicName`
  - `TypeName` 必须兼容（当前按规范化后的类型名相等比较）
  - `Required` 必须一致
  - `AcceptsBinding` 必须一致
  - `CaptureUnmatchedValues` 必须一致
  - `Kind` 必须一致
- **Emits**
  - implementation 必须包含 contract 的每个 `RazorAlias`
  - `PayloadTypeName` 必须兼容
  - `Kind` 必须一致
  - 运行时 `Name` 可以不同，最终以 implementation 为准
- **Slots**
  - implementation 必须包含 contract 的每个 `PublicName`
  - `PatternOnly` / `IsDefault` / `Required` / `NamePattern` 必须一致
  - slot context 参数个数必须一致
  - 每个 slot context 参数 `TypeName` 必须兼容
  - 运行时 `Name` 可以不同，最终以 implementation 为准
- **Flags**
  - 当前 `VueComponentFlags` 仍属于 authoring contract 面，所以要求一致

这里有一个刻意保守的设计：

- 对于还没有拆成独立 authoring/runtime 两套字段的元数据，RazorVue 当前优先要求严格一致
- 只有已经明确分层的运行时命名（prop name / emit name / slot name）才允许 implementation 覆盖

这样可以保证容器抽象先稳定，再逐步扩展更细粒度的兼容规则，而不是提前放松后再靠例外修补。

这组规则让容器机制更接近“编译期依赖注入”，但仍保持静态、可验证、可诊断。

### 2.3 扩展性接口

#### 2.3.1 IRazorSemanticFrontend

**文件**：`src/Jazor.RazorVue/Extensibility/IRazorSemanticFrontend.cs`

**用途**：定义编译器编排与 Razor 特定语义提取之间的窄缝（narrow seam）。长期目标是让 Razor 拥有的项目实现此契约，而无需 Jazor.Compiler 永久拥有每个前端细节。

**接口定义**：
```csharp
public interface IRazorSemanticFrontend
{
    string Name { get; }

    bool CanHandle(Compilation compilation);

    RazorVueEntryKind ClassifyEntry(Compilation compilation, INamedTypeSymbol symbol);

    ImmutableArray<RazorVueSemanticSnapshot> CreateSemanticSnapshots(Compilation compilation);
}
```

**方法说明**：

- **Name**：前端名称，用于标识和调试（例如 `"Jazor.Compiler.DefaultRazorFrontend"`）

- **CanHandle**：判断是否可以处理给定的 Roslyn 编译
  - 返回 `true` 表示此前端可以处理该编译
  - 通常检查是否引用了必要的程序集或存在特定的类型

- **ClassifyEntry**：对给定符号进行条目分类
  - 返回 `RazorVueEntryKind` 枚举值：
    - `None`：非 RazorVue 条目
    - `StaticModule`：静态模块
    - `RazorVueComponent`：RazorVue 组件
    - `Invalid`：无效条目

- **CreateSemanticSnapshots**：从编译中创建语义快照集合
  - 返回 `ImmutableArray<RazorVueSemanticSnapshot>`
  - 每个快照代表一个组件或模块的语义信息

**设计意图**：
- 允许 Razor 团队实现自己的语义前端
- Jazor.Compiler 通过此接口与 Razor 语义解耦
- 未来可支持多个前端（例如 Razor、Blazor、或其他模板引擎）

#### 2.3.2 IRazorVueArtifactLowerer

**文件**：`src/Jazor.RazorVue/Extensibility/IRazorVueArtifactLowerer.cs`

**用途**：将编译器拥有的语义快照转换为 Vue 工件。保持此契约明确可防止管道退化为直接字符串生成。

**接口定义**：
```csharp
public interface IRazorVueArtifactLowerer
{
    VueCompiledArtifact Lower(RazorVueCompilationContext context, RazorVueSemanticSnapshot snapshot);

    VueCompiledArtifact Lower(RazorVueSemanticSnapshot snapshot);
}
```

**方法说明**：

- **Lower(context, snapshot)**：完整的降低方法
  - `context`：RazorVue 编译上下文，包含所有全局状态
  - `snapshot`：单个组件的语义快照
  - 返回 `VueCompiledArtifact`：编译后的 Vue 工件

- **Lower(snapshot)**：简化重载
  - 仅使用快照进行降低
  - 内部可能使用默认或缓存的上下文

**设计意图**：
- 明确语义快照与生成代码之间的边界
- 允许不同的降低策略（例如生产构建 vs 开发构建）
- 防止管道退化为字符串拼接，保持 AST 级别的转换

#### 2.3.3 DefaultRazorSemanticFrontend

**文件**：`src/Jazor.RazorVue/Extensibility/DefaultRazorSemanticFrontend.cs`

**用途**：内置语义前端，从 Roslyn 编译中投影 RazorVue 快照。

**实现特性**：
- 单例模式：`public static DefaultRazorSemanticFrontend Instance { get; }`
- 内部类：不对外暴露，仅作为默认实现

**实现细节**：
```csharp
public bool CanHandle(Compilation compilation)
    => RazorVueCompilationContext.TryCreate(compilation) is not null;

public RazorVueEntryKind ClassifyEntry(Compilation compilation, INamedTypeSymbol symbol)
    => GetRequiredContext(compilation).ClassifyEntry(symbol);

public ImmutableArray<RazorVueSemanticSnapshot> CreateSemanticSnapshots(Compilation compilation)
    => GetRequiredContext(compilation).CreateSemanticSnapshots();
```

**设计权衡**：
> Keep a compiler-local fallback until the Razor project becomes the
> primary semantic frontend through a proven registration/loading path.

当前实现作为过渡方案：
- 未来 Razor 项目应成为主要语义前端
- 通过经过验证的注册/加载路径替代默认实现
- 当前保留编译器本地回退以确保向后兼容

## 3. 核心算法

### 3.1 属性元数据提取流程

1. **组件发现**：扫描程序集中所有实现 `IVueLibraryComponent` 的类型
2. **属性读取**：使用反射读取 `VueLibrary*Attribute` 属性
3. **描述符构建**：将属性数据转换为 `VueComponentDescriptor`
4. **注册表更新**：将描述符注册到 `VueComponentRegistry`

### 3.2 语义前端选择流程

1. **前端发现**：从服务定位器或配置获取所有已注册的 `IRazorSemanticFrontend` 实现
2. **能力检查**：调用 `CanHandle(compilation)` 检查每个前端
3. **前端选择**：选择第一个返回 `true` 的前端
4. **快照生成**：调用 `CreateSemanticSnapshots(compilation)` 生成语义快照

### 3.3 工件降低流程

1. **快照选择**：从编译结果中选择一个 `RazorVueSemanticSnapshot`
2. **降低器调用**：调用 `IRazorVueArtifactLowerer.Lower(snapshot)`
3. **代码生成**：降低器内部将语义信息转换为 Vue 组件代码
4. **工件构建**：返回 `VueCompiledArtifact`，包含代码、导入、样式等

## 4. 线程安全模型

### 4.1 属性读取
- **只读操作**：属性读取是线程安全的（.NET 属性元数据是不可变的）
- **无状态设计**：所有属性类都是无状态的

### 4.2 前端实现
- **单例模式**：`DefaultRazorSemanticFrontend.Instance` 是线程安全的单例
- **无状态方法**：所有方法都是纯函数，无共享状态

### 4.3 降低器
- **无状态接口**：`IRazorVueArtifactLowerer` 接口本身不包含状态
- **上下文传递**：所有必要状态通过方法参数传递

## 5. 错误处理

### 5.1 属性验证错误

**场景**：属性使用不当（例如无效的导入路径）

**处理**：在编译时通过 `RazorVueCompilationIssue` 报告
- `InvalidLibraryComponentDeclaration`
- `InvalidLibraryStyleDependencyDeclaration`
- `InvalidLibraryPluginRequirementDeclaration`

### 5.2 前端不可用

**场景**：没有找到能够处理编译的前端

**处理**：`DefaultRazorSemanticFrontend.GetRequiredContext()` 抛出 `InvalidOperationException`
```csharp
throw new InvalidOperationException(
    "The default Razor semantic frontend could not create a RazorVue compilation context.");
```

### 5.3 降低失败

**场景**：语义快照无法转换为有效代码

**处理**：通过 `RazorVueCompilationIssue` 报告具体错误

## 6. 配置选项

### 6.1 属性配置
所有配置通过声明式属性完成，无需运行时配置文件。

### 6.2 前端注册
当前使用硬编码单例 `DefaultRazorSemanticFrontend.Instance`。未来计划支持：
- 服务定位器模式
- 依赖注入容器
- 配置文件驱动的注册

### 6.3 降低器选择
当前降低器是管道内部的实现细节。未来计划支持：
- 策略模式选择降低器
- 构建配置（开发/生产）切换降低器

## 7. 与其他子系统的交互

### 7.1 与 Descriptor 子系统交互
- **属性 → 描述符**：`VueComponentDescriptorFactory` 将属性元数据转换为描述符
- **注册表**：`VueComponentRegistry` 存储所有组件描述符

### 7.2 与 Artifacts 子系统交互
- **前端 → 快照**：`IRazorSemanticFrontend.CreateSemanticSnapshots()` 返回 `RazorVueSemanticSnapshot`
- **降低器 → 工件**：`IRazorVueArtifactLowerer.Lower()` 返回 `VueCompiledArtifact`

### 7.3 与 Pipeline 子系统交互
- **编排**：`RazorVuePipeline` 协调前端和降低器的调用
- **上下文**：`RazorVueCompilationContext` 在管道各阶段间传递状态

### 7.4 与 LSP 交互
- **组件解析**：LSP 使用 `VueComponentDescriptor` 提供自动完成和类型检查
- **元数据访问**：通过 `IRazorSemanticFrontend` 访问组件元数据

## 8. 设计权衡

### 8.1 属性系统 vs Fluent API

**选择**：使用声明式属性系统

**理由**：
- 编译时验证
- 性能优越（反射读取 vs 运行时构建）
- 与 C# 生态系统集成良好
- 代码可读性高

**代价**：
- 灵活性较低（无法动态配置）
- 属性参数必须是编译时常量

### 8.2 紧密耦合 vs 解耦

**选择**：通过接口窄缝解耦

**理由**：
- 允许未来替代实现
- 测试友好（可 mock 接口）
- 职责分离（编译器 vs 前端 vs 降低器）

**代价**：
- 增加了抽象层
- 当前只有一种实现（`DefaultRazorSemanticFrontend`）

### 8.3 单例 vs 依赖注入

**选择**：当前使用单例，未来计划支持 DI

**理由**：
- 简单直接
- 无需外部 DI 容器
- 性能最优

**代价**：
- 测试时难以替换实现
- 未来扩展性受限

### 8.4 内置前端 vs 外部实现

**选择**：当前使用内置 `DefaultRazorSemanticFrontend`

**未来方向**：
> The long-term goal is for Razor-owned projects to implement this contract without forcing Jazor.Compiler to own every frontend detail forever.

**理由**：
- Razor 团队最了解 Razor 语义
- Jazor.Compiler 应保持通用性
- 明确的边界有利于维护

### 8.5 语义快照 vs 直接代码生成

**选择**：通过语义快照中间层

**理由**：
- 明确转换边界
- 支持多种降低策略
- 便于调试和诊断

**代价**：
- 增加了内存开销
- 需要维护中间表示

---

**文档维护者**：developerhan
**最后更新**：2026-04-21
**文档版本**：v1.0
