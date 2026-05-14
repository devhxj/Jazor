# Vue 组件描述符（Component Descriptor）

## 为什么需要

Vue 组件描述符是 RazorVue 编译时分析的核心数据结构，负责将 C# 组件类型转换为 Vue 组件的完整元数据表示。它桥接了 Razor 组件模型和 Vue 组件模型，使得编译器能够正确生成 Vue 组件代码。

描述符系统解决了以下关键问题：

1. **组件识别与分类**：区分用户组件、内置组件和库组件
2. **参数映射**：将 Razor `[Parameter]` 属性映射到 Vue props/emits/slots
3. **生命周期检测**：识别组件使用的生命周期钩子
4. **命名转换**：处理 C# PascalCase 到 Vue camelCase 的命名约定
5. **依赖声明**：跟踪样式依赖和插件需求
6. **容器抽象投影**：在不新增 source kind 的前提下承载 container contract 归属

## 实现思路

### 核心数据结构

组件描述符系统位于 `src/Jazor.RazorVue/Descriptor/VueComponentDescriptor.cs`，由以下核心 record 类型组成：

#### 1. VueComponentDescriptor

主描述符，包含组件的完整元数据：

```csharp
public sealed record VueComponentDescriptor(
    string Name,                                    // 组件短名称（如 "MyComponent"）
    string FullName,                                // 完全限定名（如 "App.Components.MyComponent"）
    VueComponentSourceKind SourceKind,              // 组件来源类型
    string ResolutionNamespace,                     // 解析命名空间
    string ImportSpecifier,                         // 导入说明符（模块路径）
    string ExportName,                              // 导出名称
    string? ContainerContractFullName,              // 容器契约完全限定名
    ImmutableArray<VuePropDescriptor> Props,        // 属性列表
    ImmutableArray<VueEmitDescriptor> Emits,        // 事件列表
    ImmutableArray<VueSlotDescriptor> Slots,        // 插槽列表
    ImmutableArray<string> StyleDependencies,       // 样式依赖
    ImmutableArray<string> PluginRequirements,      // 插件需求
    VueComponentFlags Flags);                       // 组件标志
```

**组件来源类型**：

```csharp
public enum VueComponentSourceKind
{
    UserComponent,      // 用户定义组件（从 RazorVueComponentCandidate 生成）
    Intrinsic,          // Vue 内置组件（Teleport, Transition, KeepAlive, Suspense）
    LibraryComponent    // 库组件（从 [VueLibraryComponent] 声明生成）
}
```

**组件标志**：

```csharp
[Flags]
public enum VueComponentFlags
{
    None = 0,
    SupportsModelValue = 1,         // 支持单向数据流（v-model）
    SupportsMultipleModels = 2,     // 支持多个 v-model
    RequiresExplicitChildren = 4,   // 需要显式子节点
    IsDynamicSafe = 8,             // 动态组件安全
    IsFormControl = 16             // 表单控件
}
```

#### 2. VuePropDescriptor

属性描述符，描述 Vue props：

```csharp
public sealed record VuePropDescriptor(
    string Name,                    // prop 名称（camelCase）
    string PublicName,              // 公共名称（C# PascalCase）
    string TypeName,                // 类型名称（编译时字符串）
    bool Required,                  // 是否必需
    bool AcceptsBinding,            // 是否接受双向绑定
    string? DefaultExpression,      // 默认值表达式
    VuePropKind Kind,               // prop 类型
    bool CaptureUnmatchedValues);   // 是否是任意属性汇聚槽
```

**Prop 类型**：

```csharp
public enum VuePropKind
{
    Normal,              // 普通 prop
    Model,               // v-model prop
    HtmlLike,            // 类 HTML 属性（如 checked, disabled）
    LibrarySpecific      // 库特定 prop
}
```

#### 3. VueEmitDescriptor

事件描述符，描述 Vue emits：

```csharp
public sealed record VueEmitDescriptor(
    string Name,                    // emit 名称（如 "update:value", "click"）
    string PayloadTypeName,         // 载荷类型名称
    string? RazorAlias,             // Razor 别名（如 "OnValueChanged"）
    VueEmitKind Kind);              // emit 类型
```

**Emit 类型**：

```csharp
public enum VueEmitKind
{
    Normal,              // 普通 emit
    ModelUpdate,         // v-model 更新 emit（update:xxx）
    LifecycleLike,       // 类生命周期 emit
    LibrarySpecific      // 库特定 emit
}
```

#### 4. VueSlotDescriptor

插槽描述符，描述 Vue slots：

```csharp
public sealed record VueSlotDescriptor(
    string Name,                    // slot 名称（如 "default", "header"）
    string PublicName,              // 公共名称（C# 属性名，如 "ChildContent", "Header"）
    string? NamePattern,            // 动态 slot 名称模式（如 "item.${string}"）
    bool PatternOnly,               // 是否只允许模式命中
    bool IsDefault,                 // 是否为默认插槽
    ImmutableArray<VueSlotParameterDescriptor> Parameters,  // 插槽参数（作用域插槽）
    bool Required);                 // 是否必需
```

#### 容器契约归属字段

`ContainerContractFullName` 表示当前 descriptor 隶属于哪个容器 contract。

- authored 容器 contract 本体：
  - `ContainerContractFullName == FullName`
- 容器 implementation：
  - `ContainerContractFullName == 对应 contract FullName`
- 普通组件：
  - `ContainerContractFullName == null`

它不是新的组件来源类型，也不单独改变解析路径。  
真正的实现替换发生在 `VueInjectRegistry` 读取装配级 `[VueInject]` 之后。

#### 容器 inject 的 merged descriptor 运行规则

当 `IVueContainerComponent` contract 命中装配级 `[VueInject]` 时，RazorVue 不会直接把 contract descriptor 整体替换成 implementation descriptor，而是创建一个 merged descriptor。

合并后的字段归属必须固定如下：

- contract 侧保留 authoring 身份：
  - `Name`
  - `FullName`
  - `ResolutionNamespace`
  - `ContainerContractFullName`
  - `RouteTemplates`
  - `Flags`
  - props / emits / slots 的 authoring 语义
- implementation 侧提供 runtime 依赖面：
  - `SourceKind`
  - `ImportSpecifier`
  - `ExportName`
  - `StyleDependencies`
  - `PluginRequirements`
  - props / emits / slots 的 runtime `Name`

这意味着：

1. C# authoring 仍然只面向 container contract 的公共成员名，例如 `Title`、`ValueChanged`、`Header`
2. 生成的 Vue SFC / module 产物必须使用 implementation 的 runtime 名称，例如：
   - prop `Title -> menuTitle`
   - model prop `Value -> modelValue`
   - emit `ValueChanged -> update:modelValue`
   - slot `Header -> header`
3. contract 不能被 implementation 偷偷收窄。任何 prop / emit / slot / flags 不兼容都必须在 inject 解析阶段失败，而不是带着不一致继续生成产物

这是容器抽象成立的基础约束：authoring surface 和 runtime surface 分层存在，但两者通过 merged descriptor 精确拼接，而不是相互泄漏或整体覆盖。

#### 容器 inject 的 analyzer 前置校验规则

`[VueInject]` 不是只有在某个页面实际引用 container contract 时才允许失败。

从生产约束看，assembly 级 inject 声明本身就是 public authoring contract，因此 analyzer 必须在 compilation 阶段主动完成两类校验：

1. 注册级校验
   - 同一 contract 不能声明多个 implementation
   - implementation 必须能在当前 component registry 中解析到
   - implementation 必须声明匹配的 `IVueContainerImplementation<TContract>`
2. 兼容性校验
   - 即使当前编译里没有任何 usage site 引用该 contract，也必须校验：
     - props
     - emits
     - slots
     - flags

这样可以保证：

- 错误的 `[VueInject]` 声明不会因为“当前还没人用到”而潜伏进主干
- analyzer、generator、lowering 使用同一套 inject 兼容性语义
- container contract 的 authoring 面在提交前就能得到稳定、确定的诊断

#### 容器 inject 的 artifact identity / HMR 分类规则

container inject 不只是影响最终生成代码，它还必须显式进入 owner artifact 的 identity 计算。

当前约束是：

1. compiled pipeline 的 `DescriptorHash` 不能只由 owner 自身 authored descriptor 决定
2. 当模板里实际引用了某个 container contract，并且该 contract 通过 `[VueInject]` 解析到 implementation 时：
   - owner artifact 的 `DescriptorHash` 必须额外纳入该 resolved runtime surface
   - 至少包含当前模板真实消费到的 runtime prop / emit / slot 名称，以及该 resolved component 的 import / export / source kind / flags / styles / plugins
3. `TemplateHash` 与 `LogicHash` 仍然只表达模板结构和逻辑 lowering 本身

这样做的原因不是“让 hash 更敏感”，而是保证 hot-update 分类语义正确：

- 如果 injected runtime prop / emit / slot 名称发生变化，上层组件的 host-facing runtime contract 实际已经变化
- 这类变化必须被 `RazorVueManifestDiffer` 识别为：
  - `DescriptorHash` drift
  - reason: `Public component descriptor changed.`
- 不能退化为：
  - 只有 `ContentHash` 变化
  - reason: `Module content changed outside split hash classification.`

这里有一个重要边界：

- 进入 owner `DescriptorHash` 的不是“整个 registry 的所有 resolved component descriptor”
- 而是“当前 render tree 实际引用到、并且当前模板真实消费到的 runtime surface”

这样可以避免未被当前模板使用的 implementation 细节漂移，错误地扩大为 owner artifact 的 descriptor drift。

**插槽参数描述符**（作用域插槽）：

```csharp
public sealed record VueSlotParameterDescriptor(
    string Name,                    // 参数名称（如 "context"）
    string TypeName);               // 参数类型名称
```

#### 5. VueLifecycleDescriptor

生命周期钩子描述符，记录组件使用的生命周期方法：

```csharp
public sealed record VueLifecycleDescriptor(
    bool HasOnInitialized,              // OnInitialized
    bool HasOnInitializedAsync,         // OnInitializedAsync
    bool HasOnParametersSet,            // OnParametersSet
    bool HasOnParametersSetAsync,       // OnParametersSetAsync
    bool HasOnAfterRender,              // OnAfterRender
    bool HasOnAfterRenderAsync,         // OnAfterRenderAsync
    bool HasShouldRender,               // ShouldRender
    bool HasSetParametersAsync,         // SetParametersAsync
    bool HasDispose,                    // Dispose
    bool HasDisposeAsync);              // DisposeAsync
```

**便捷属性**：

```csharp
public bool HasAnyHook
    => HasOnInitialized || HasOnInitializedAsync ||
       HasOnParametersSet || HasOnParametersSetAsync ||
       HasOnAfterRender || HasOnAfterRenderAsync ||
       HasShouldRender || HasSetParametersAsync ||
       HasDispose || HasDisposeAsync;
```

#### 6. VueLogicDescriptor

组件逻辑描述符，描述组件中的方法和字段：

```csharp
public sealed record VueLogicDescriptor(
    ImmutableArray<VueLogicFieldDescriptor> Fields,    // 字段列表
    ImmutableArray<VueLogicMethodDescriptor> Methods); // 方法列表

public static VueLogicDescriptor Empty { get; } = new(
    ImmutableArray<VueLogicFieldDescriptor>.Empty,
    ImmutableArray<VueLogicMethodDescriptor>.Empty);
```

**逻辑方法描述符**：

```csharp
public sealed record VueLogicMethodDescriptor(
    string Name,                    // 方法名称
    int Arity,                      // 参数数量
    bool IsAsync,                   // 是否异步
    IMethodSymbol MethodSymbol);    // 编译时符号
```

**逻辑字段描述符**：

```csharp
public sealed record VueLogicFieldDescriptor(
    string Name,                    // 字段名称
    bool IsReadOnly,                // 是否只读
    IFieldSymbol FieldSymbol);      // 编译时符号
```

### 类型推断规则

#### 1. RenderFragment → Slot

```csharp
// C# 代码
[Parameter] public RenderFragment ChildContent { get; set; }

// 转换结果
VueSlotDescriptor(
    Name: "default",
    PublicName: "ChildContent",
    IsDefault: true,
    Parameters: [],
    Required: false)
```

#### 2. RenderFragment<T> → Scoped Slot

```csharp
// C# 代码
[Parameter] public RenderFragment<ItemContext> Items { get; set; }

// 转换结果
VueSlotDescriptor(
    Name: "items",
    PublicName: "Items",
    IsDefault: false,
    Parameters: [
        new VueSlotParameterDescriptor(
            Name: "context",
            TypeName: "ItemContext")
    ],
    Required: false)
```

#### 3. EventCallback → Emit

```csharp
// C# 代码
[Parameter] public EventCallback OnClick { get; set; }

// 转换结果
VueEmitDescriptor(
    Name: "click",              // "On" + Click → click
    PayloadTypeName: "void",
    RazorAlias: "OnClick",
    Kind: VueEmitKind.Normal)
```

#### 4. EventCallback<T> → Typed Emit

```csharp
// C# 代码
[Parameter] public EventCallback<MouseEventArgs> OnClick { get; set; }

// 转换结果
VueEmitDescriptor(
    Name: "click",
    PayloadTypeName: "MouseEventArgs",
    RazorAlias: "OnClick",
    Kind: VueEmitKind.Normal)
```

#### 5. Foo + FooChanged → v-model

```csharp
// C# 代码
[Parameter] public string Value { get; set; }
[Parameter] public EventCallback<string> ValueChanged { get; set; }

// Value 转换结果
VuePropDescriptor(
    Name: "value",
    PublicName: "Value",
    TypeName: "string",
    Required: false,
    AcceptsBinding: true,          // 检测到 ValueChanged
    DefaultExpression: null,
    Kind: VuePropKind.Model)       // 标记为 Model

// ValueChanged 转换结果
VueEmitDescriptor(
    Name: "update:value",          // FooChanged → update:foo
    PayloadTypeName: "string",
    RazorAlias: "ValueChanged",
    Kind: VueEmitKind.ModelUpdate)
```

## 容器注入后的 descriptor 语义

容器注入成功后，编译器消费的是 merged descriptor，而不是原始 implementation descriptor。

字段边界如下：

- **contract authoring identity**
  - `Name`
  - `FullName`
  - `ResolutionNamespace`
  - `RouteTemplates`
  - `Flags`
- **implementation runtime dependency surface**
  - `SourceKind`
  - `ImportSpecifier`
  - `ExportName`
  - `StyleDependencies`
  - `PluginRequirements`
- **成员级别合成**
  - `Props`：按 `PublicName` 配对，authoring 语义保留 contract，runtime `Name` 取 implementation
  - `Emits`：按 `RazorAlias` 配对，authoring 语义保留 contract，runtime `Name` 取 implementation
  - `Slots`：按 `PublicName` 配对，authoring 语义保留 contract，runtime `Name` 取 implementation

这个设计解决的是一个很具体的问题：

- 如果整体替换成 implementation descriptor，上层 authoring 面会泄漏为具体库的 props/emits/slots
- 如果整体保留 contract descriptor，最终生成的 runtime prop/event/slot 名又会错误

因此当前实现必须使用 merged descriptor。

## 容器 compatibility 约束

为了让 merged descriptor 可信，注入前会校验 contract/implementation 是否兼容。

当前规则：

- `Props`
  - implementation 必须包含每个 contract `PublicName`
  - `TypeName` / `Required` / `AcceptsBinding` / `CaptureUnmatchedValues` / `Kind` 必须兼容
- `Emits`
  - implementation 必须包含每个 contract `RazorAlias`
  - `PayloadTypeName` / `Kind` 必须兼容
- `Slots`
  - implementation 必须包含每个 contract `PublicName`
  - `PatternOnly` / `IsDefault` / `Required` / `NamePattern` 必须兼容
  - slot context 参数个数与参数类型必须兼容
- `Flags`
  - 当前要求一致

兼容性的核心原则是：

- 运行时命名可以替换
- authoring 语义不能收窄或漂移

另外，类型名比较做了一个窄范围规范化，只消除 `string` / `System.String` 这类展示差异，不放宽真实类型不兼容。

### 命名转换规则

#### 1. PascalCase → camelCase

```csharp
// 工具方法
private static string ToLowerCamelCase(string value)
{
    if (string.IsNullOrEmpty(value))
        return value;

    if (value.Length == 1)
        return char.ToLowerInvariant(value[0]).ToString();

    // 处理缩写词（如 HTTP, URL）
    if (char.IsUpper(value[0]) && char.IsUpper(value[1]))
        value;  // 保持不变（HTTP → HTTP）

    // 常规情况（FirstName → firstName）
    return char.ToLowerInvariant(value[0]) + value.Substring(1);
}
```

**转换示例**：

| C# 名称 | Vue 名称 |
|---------|----------|
| `FirstName` | `firstName` |
| `IsActive` | `isActive` |
| `HTTPClient` | `HTTPClient` |
| `OnDataBound` | `dataBound` (emit) |
| `ChildContent` | `default` (slot) |

#### 2. Emit 名称转换

```csharp
private static string ToEmitName(string propertyName)
{
    // "On" + Xxxx → xxxx
    if (propertyName.StartsWith("On", StringComparison.Ordinal) &&
        propertyName.Length > 2 &&
        char.IsUpper(propertyName[2]))
    {
        return ToLowerCamelCase(propertyName.Substring(2));
    }

    return ToLowerCamelCase(propertyName);
}
```

**转换示例**：

| C# 属性名 | Vue emit 名称 |
|-----------|---------------|
| `OnClick` | `click` |
| `OnDataBound` | `dataBound` |
| `OnValueChanged` | `valueChanged` (非 v-model) |
| `ValueChanged` (v-model) | `update:value` |

## 文件位置

- **主文件**：`src/Jazor.RazorVue/Descriptor/VueComponentDescriptor.cs`
- **属性描述符**：`src/Jazor.RazorVue/Descriptor/VuePropDescriptor.cs`
- **事件描述符**：`src/Jazor.RazorVue/Descriptor/VueEmitDescriptor.cs`
- **插槽描述符**：`src/Jazor.RazorVue/Descriptor/VueSlotDescriptor.cs`

## 相关文档

- **组件描述符工厂**：`docs/01-目标/razorvue/descriptor/DescriptorFactory.md`
- **组件注册表**：`docs/01-目标/razorvue/descriptor/ComponentRegistry.md`
- **内置组件**：`docs/01-目标/razorvue/descriptor/IntrinsicComponents.md`
- **编译问题**：`docs/01-目标/razorvue/descriptor/CompilationIssues.md`

---

**维护者**：developerhan
**最后更新**：2026-05-14
**文档版本**：v1.1
