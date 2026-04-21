# Vue 组件描述符（Component Descriptor）

## 为什么需要

Vue 组件描述符是 RazorVue 编译时分析的核心数据结构，负责将 C# 组件类型转换为 Vue 组件的完整元数据表示。它桥接了 Razor 组件模型和 Vue 组件模型，使得编译器能够正确生成 Vue 组件代码。

描述符系统解决了以下关键问题：

1. **组件识别与分类**：区分用户组件、内置组件和库组件
2. **参数映射**：将 Razor `[Parameter]` 属性映射到 Vue props/emits/slots
3. **生命周期检测**：识别组件使用的生命周期钩子
4. **命名转换**：处理 C# PascalCase 到 Vue camelCase 的命名约定
5. **依赖声明**：跟踪样式依赖和插件需求

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
    VuePropKind Kind);              // prop 类型
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
    bool IsDefault,                 // 是否为默认插槽
    ImmutableArray<VueSlotParameterDescriptor> Parameters,  // 插槽参数（作用域插槽）
    bool Required);                 // 是否必需
```

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
**最后更新**：2026-04-21
**文档版本**：v1.0
