# RazorVue 核心枚举类型


## 1. 文档定位

本文档详细说明 RazorVue 核心子系统中的所有枚举类型，这些枚举构成了组件分类、状态跟踪、错误诊断和源映射的基础。

**核心职责**：
- 组件入口分类（`RazorVueEntryKind`）
- 组件来源和特性标记（`VueComponentSourceKind`, `VueComponentFlags`）
- 属性和事件的语义分类（`VuePropKind`, `VueEmitKind`）
- HMR 边界控制（`HmrBoundaryKind`）
- 源代码映射质量（`RazorVueOriginKind`, `RazorVueMappingQuality`, `RazorVueOriginProvenance`）
- 编译问题和组件解析状态（`RazorVueIssueCode`, `RazorVueIssueSeverity`, `VueComponentResolutionStatus`）

**源文件位置**：
- `src/Jazor.RazorVue/RazorVueEntryKind.cs`
- `src/Jazor.RazorVue/Descriptor/VueComponentDescriptor.cs`
- `src/Jazor.RazorVue/Descriptor/VuePropDescriptor.cs`
- `src/Jazor.RazorVue/Descriptor/VueEmitDescriptor.cs`
- `src/Jazor.RazorVue/Artifacts/VueCompiledArtifact.cs`
- `src/Jazor.RazorVue/Artifacts/RazorVueSourceOrigin.cs`
- `src/Jazor.RazorVue/Descriptor/RazorVueCompilationIssue.cs`
- `src/Jazor.RazorVue/Descriptor/VueComponentResolutionResult.cs`

## 2. 核心类型

### 2.1 条目分类枚举

#### RazorVueEntryKind

**文件**：`src/Jazor.RazorVue/RazorVueEntryKind.cs`

**用途**：分类 Roslyn 编译中的类型符号，确定其是否为 RazorVue 组件或模块。

**枚举值**：
```csharp
public enum RazorVueEntryKind
{
    None,              // 非 RazorVue 条目（普通 C# 类）
    StaticModule,      // 静态模块（不包含模板的纯逻辑模块）
    RazorVueComponent, // RazorVue 组件（包含模板 + 逻辑的完整组件）
    Invalid            // 无效条目（无法分类或存在错误）
}
```

**使用场景**：
- `IRazorSemanticFrontend.ClassifyEntry()` 返回值
- 决定编译器如何处理给定的类型符号
- 过滤非 RazorVue 类型以优化性能

**分类逻辑**：
```csharp
// 简化的分类逻辑
public RazorVueEntryKind ClassifyEntry(INamedTypeSymbol symbol)
{
    // 1. 检查是否实现 IVueComponent
    if (!symbol.ImplementsInterface<IVueComponent>())
        return RazorVueEntryKind.None;

    // 2. 检查是否为库组件桩
    if (symbol.ImplementsInterface<IVueLibraryComponent>())
        return RazorVueEntryKind.Invalid; // 库组件不编译

    // 3. 检查是否包含 Razor 模板
    var hasTemplate = symbol.HasRazorTemplate();
    if (!hasTemplate)
        return RazorVueEntryKind.StaticModule;

    // 4. 完整的 RazorVue 组件
    return RazorVueEntryKind.RazorVueComponent;
}
```

### 2.2 组件分类枚举

#### VueComponentSourceKind

**文件**：`src/Jazor.RazorVue/Descriptor/VueComponentDescriptor.cs`

**用途**：标识 Vue 组件的来源，区分用户组件、内置组件和库组件。

**枚举值**：
```csharp
public enum VueComponentSourceKind
{
    UserComponent,    // 用户定义的组件（.razor 文件）
    Intrinsic,        // 内置组件（如 <template>, <component>, <slot>）
    LibraryComponent  // 库组件（如 Vuetify 的 VBtn）
}
```

**使用场景**：
- `VueComponentDescriptor.SourceKind` 字段
- 决定组件解析策略
- 影响 IDE 自动完成和诊断行为

**特征对比**：

| 来源 | 示例 | 编译产物 | 元数据来源 |
|------|------|---------|-----------|
| `UserComponent` | `MyComponent.razor` | .mjs 模块 | 编译时生成 |
| `Intrinsic` | `<template>`, `<slot>` | 无（内联处理） | 编译器内置 |
| `LibraryComponent` | `VBtn` (Vuetify) | 无（仅引用） | `VueLibrary*Attribute` |

#### VueComponentFlags

**文件**：`src/Jazor.RazorVue/Descriptor/VueComponentDescriptor.cs`

**用途**：标记组件的特殊能力和行为约束，使用 `[Flags]` 特性支持位组合。

**枚举值**：
```csharp
[Flags]
public enum VueComponentFlags
{
    None = 0,                    // 无特殊标志
    SupportsModelValue = 1,      // 支持 v-model（model-value prop + update:modelValue 事件）
    SupportsMultipleModels = 2,  // 支持多个 v-model 绑定（如 v-model:title 和 v-model:content）
    RequiresExplicitChildren = 4,// 需要显式子组件（不支持默认插槽）
    IsDynamicSafe = 8,           // 可安全用于动态组件（<component :is="...">）
    IsFormControl = 16          // 是表单控件（参与表单提交）
}
```

**使用场景**：
- `VueComponentDescriptor.Flags` 字段
- `VueLibraryComponentFlagsAttribute` 构造参数
- 编译器验证和 IDE 诊断

**标志组合示例**：
```csharp
// 文本输入框：支持 v-model，是表单控件
VueComponentFlags.SupportsModelValue | VueComponentFlags.IsFormControl
// = 1 | 16 = 17

// 自定义选择器：支持多个 v-model，需要显式子组件
VueComponentFlags.SupportsMultipleModels | VueComponentFlags.RequiresExplicitChildren
// = 2 | 4 = 6

// 通用包装器：动态安全，需要显式子组件
VueComponentFlags.IsDynamicSafe | VueComponentFlags.RequiresExplicitChildren
// = 8 | 4 = 12
```

**验证规则**：
- `SupportsModelValue` 和 `SupportsMultipleModels` 互斥（一个组件不能同时支持两者）
- `RequiresExplicitChildren` 意味着没有默认插槽
- `IsFormControl` 通常隐含 `SupportsModelValue`

### 2.3 属性和事件语义枚举

#### VuePropKind

**文件**：`src/Jazor.RazorVue/Descriptor/VuePropDescriptor.cs`

**用途**：分类 Vue 组件属性（props）的语义类型，指导编译器如何处理绑定和验证。

**枚举值**：
```csharp
public enum VuePropKind
{
    Normal,         // 普通属性（如 color, size）
    Model,          // v-model 绑定属性（如 model-value, title）
    HtmlLike,       // 类 HTML 属性（如 disabled, checked, hidden）
    LibrarySpecific // 库特定语义属性（由库定义特殊行为的属性）
}
```

**使用场景**：
- `VuePropDescriptor.Kind` 字段
- `VueLibraryPropAttribute` 构造参数
- 编译器绑定策略选择

**语义差异**：

| Kind | 示例 | 绑定方式 | 类型处理 |
|------|------|---------|---------|
| `Normal` | `color="primary"` | `:color="'primary'"` 或 `color="primary"` | 根据类型转换 |
| `Model` | `v-model="text"` | `:model-value="text" @update:model-value="text = $event"` | 双向绑定 |
| `HtmlLike` | `disabled` | `:disabled="true"` 或 `disabled` | 布尔值，可省略值 |
| `LibrarySpecific` | `ripple`（Vuetify）| 库定义 | 库特定逻辑 |

#### VueEmitKind

**文件**：`src/Jazor.RazorVue/Descriptor/VueEmitDescriptor.cs`

**用途**：分类 Vue 组件事件（emits）的语义类型，指导事件名称转换和负载类型推断。

**枚举值**：
```csharp
public enum VueEmitKind
{
    Normal,         // 普通事件（如 click, submit）
    ModelUpdate,    // v-model 更新事件（如 update:modelValue）
    LifecycleLike,  // 生命周期类事件（如 mounted, updated）
    LibrarySpecific // 库特定语义事件（库定义的特殊事件）
}
```

**使用场景**：
- `VueEmitDescriptor.Kind` 字段
- `VueLibraryEmitAttribute` 构造参数
- 事件名称自动转换规则

**命名转换规则**：

| Kind | Razor 别名 | Vue 事件名 | 示例 |
|------|-----------|-----------|------|
| `Normal` | `OnClick` | `click` | `<button @click="handler">` |
| `Normal` | `OnSubmit` | `submit` | `<form @submit="handler">` |
| `ModelUpdate` | `OnUpdateModelValue` | `update:modelValue` | `<input @update:model-value="handler">` |
| `LifecycleLike` | `OnMounted` | `mounted` | 组件生命周期钩子 |
| `LibrarySpecific` | `OnResize`（Vuetify） | `resize` | 库定义 |

### 2.4 HMR 边界枚举

#### HmrBoundaryKind

**文件**：`src/Jazor.RazorVue/Artifacts/VueCompiledArtifact.cs`

**用途**：定义热模块替换（HMR）的边界类型，决定编辑后需要重新加载的范围。

**枚举值**：
```csharp
public enum HmrBoundaryKind
{
    Unknown,            // 未知边界（保守策略，完全重载）
    TemplateOnly,       // 仅模板变更（支持 HMR）
    LogicSafe,          // 逻辑安全变更（支持 HMR，保留状态）
    FullReloadRequired  // 需要完全重载（不支持 HMR）
}
```

**使用场景**：
- `VueArtifactIdentity.HmrBoundaryKind` 字段
- HMR 运行时决定更新策略
- 开发服务器决定刷新范围

**HMR 策略**：

| Kind | 模板变更 | 逻辑变更 | 状态保留 | 页面刷新 |
|------|---------|---------|---------|---------|
| `Unknown` | 完全重载 | 完全重载 | 否 | 是 |
| `TemplateOnly` | 热替换 | 完全重载 | 否 | 否 |
| `LogicSafe` | 热替换 | 热替换 | 是 | 否 |
| `FullReloadRequired` | 完全重载 | 完全重载 | 否 | 是 |

**分类逻辑**：
```csharp
public HmrBoundaryKind DetermineHmrBoundary(RazorVueSemanticSnapshot snapshot)
{
    // 1. 检查是否有不安全的生命周期钩子
    if (snapshot.HasUnsafeLifecycleHooks)
        return HmrBoundaryKind.FullReloadRequired;

    // 2. 检查是否有状态变更
    if (snapshot.HasStateFields)
        return HmrBoundaryKind.LogicSafe;

    // 3. 仅模板变更
    return HmrBoundaryKind.TemplateOnly;
}
```

### 2.5 源映射枚举

#### RazorVueOriginKind

**文件**：`src/Jazor.RazorVue/Artifacts/RazorVueSourceOrigin.cs`

**用途**：标识源代码起源的类型，用于 Source Map 和调试信息。

**枚举值**：
```csharp
public enum RazorVueOriginKind
{
    Component,         // 组件定义（.razor 文件）
    Descriptor,        // 描述符生成（编译时元数据）
    Template,          // 模板部分（HTML/Razor 语法）
    Logic,             // 逻辑部分（C# 代码块）
    GeneratedRender    // 生成的渲染代码（编译器输出）
}
```

**使用场景**：
- `RazorVueSourceOrigin.OriginKind` 字段
- Source Map 生成
- 调试器源映射

**Source Map 结构**：
```javascript
{
  "version": 3,
  "sources": ["MyComponent.razor"],
  "names": ["render", "buildRenderMode"],
  "mappings": "...",
  "originKinds": ["Component", "Template", "Logic", "GeneratedRender"]
}
```

#### RazorVueMappingQuality

**文件**：`src/Jazor.RazorVue/Artifacts/RazorVueSourceOrigin.cs`

**用途**：描述源映射的质量和精度，影响调试体验。

**枚举值**：
```csharp
public enum RazorVueMappingQuality
{
    ExactSource,           // 精确映射（1:1 对应）
    MappedFromGenerated,   // 从生成代码映射（可能有偏移）
    GeneratedOnly          // 仅生成代码（无源映射）
}
```

**使用场景**：
- `RazorVueSourceOrigin.MappingQuality` 字段
- 调试器决定断点行为
- IDE 决定是否支持"转到定义"

**质量差异**：

| Quality | 断点精度 | 变量检查 | 源码导航 |
|---------|---------|---------|---------|
| `ExactSource` | 完全精确 | 完全支持 | 完全支持 |
| `MappedFromGenerated` | 近似 | 部分支持 | 部分支持 |
| `GeneratedOnly` | 不支持 | 不支持 | 不支持 |

#### RazorVueOriginProvenance

**文件**：`src/Jazor.RazorVue/Artifacts/RazorVueSourceOrigin.cs`

**用途**：标识源位置信息的来源证明，用于验证映射的可信度。

**枚举值**：
```csharp
public enum RazorVueOriginProvenance
{
    RazorSourceMap,           // Razor Source Map（官方映射）
    GeneratedSyntaxLocation,  // 生成语法位置（编译器提供）
    GeneratedFallback         // 生成回退（最佳猜测）
}
```

**使用场景**：
- `RazorVueSourceOrigin.Provenance` 字段
- 诊断工具验证映射质量
- 调试器决定信任级别

**证明层级**：
```
RazorSourceMap（最可信）
    ↓
GeneratedSyntaxLocation（编译器生成，可信）
    ↓
GeneratedFallback（最佳猜测，可能不准确）
```

### 2.6 编译问题枚举

#### RazorVueIssueCode

**文件**：`src/Jazor.RazorVue/Descriptor/RazorVueCompilationIssue.cs`

**用途**：定义所有 RazorVue 编译时问题的错误代码，用于错误报告和诊断。

**枚举值**：
```csharp
public enum RazorVueIssueCode
{
    ComponentNotFound,                      // 组件未找到（404）
    AmbiguousComponentName,                 // 组件名称冲突（多个匹配）
    ReservedIntrinsicNameCollision,         // 与保留内置名称冲突
    UnsupportedLifecycleLowering,           // 不支持的生命周期钩子降级
    UnsupportedSetupLogicLowering,          // 不支持的 setup 逻辑降级
    InvalidLibraryComponentDeclaration,     // 无效的库组件声明
    InvalidLibraryStyleDependencyDeclaration, // 无效的库样式依赖声明
    InvalidLibraryPluginRequirementDeclaration, // 无效的库插件依赖声明
    UnknownParameter,                       // 未知参数（prop 或 attribute）
    InvalidBindTarget,                      // 无效的 v-bind 目标
    UnknownSlot,                            // 未知插槽
    SlotContextMisuse,                      // 插槽上下文使用错误
    DuplicateSlotValue                      // 重复的插槽值
}
```

**使用场景**：
- `RazorVueCompilationIssue.Code` 字段
- 编译器错误报告
- IDE 诊断显示

**错误分类**：

| Code | 严重性 | 示例场景 |
|------|-------|---------|
| `ComponentNotFound` | Error | `<UnknownComponent />` |
| `AmbiguousComponentName` | Error | 两个命名空间都有 `MyButton` |
| `ReservedIntrinsicNameCollision` | Error | 用户定义 `<template>` 组件 |
| `UnsupportedLifecycleLowering` | Error | 使用了不兼容的生命周期钩子 |
| `UnknownParameter` | Error | `<Button unknown="value" />` |
| `InvalidBindTarget` | Error | `v-bind="undefined"` |
| `UnknownSlot` | Error | `<MyComponent><UnknownSlot /></MyComponent>` |

#### RazorVueIssueSeverity

**文件**：`src/Jazor.RazorVue/Descriptor/RazorVueCompilationIssue.cs`

**用途**：定义问题的严重性级别，当前仅支持 Error。

**枚举值**：
```csharp
public enum RazorVueIssueSeverity
{
    Error    // 错误（阻止编译）
}
```

**未来扩展计划**：
- `Warning`：警告（不阻止编译，但建议修复）
- `Info`：信息（提示性消息）
- `Hint`：提示（代码建议）

**使用场景**：
- `RazorVueCompilationIssue.Severity` 字段
- IDE 决定错误显示样式
- 编译器决定是否继续编译

### 2.7 组件解析状态枚举

#### VueComponentResolutionStatus

**文件**：`src/Jazor.RazorVue/Descriptor/VueComponentResolutionResult.cs`

**用途**：表示组件解析操作的结果状态，用于错误处理和诊断。

**枚举值**：
```csharp
public enum VueComponentResolutionStatus
{
    Resolved,               // 解析成功（找到唯一组件）
    NotFound,               // 未找到（组件不存在）
    Ambiguous,              // 歧义（多个匹配的组件）
    ReservedIntrinsicName   // 与保留内置名称冲突
}
```

**使用场景**：
- `VueComponentResolutionResult.Status` 字段
- 组件注册表查询结果
- IDE 自动完成和导航

**状态转换流程**：
```
开始查询
    ↓
扫描所有命名空间
    ↓
找到候选组件？
    ├─ 否 → NotFound
    ├─ 是，1个 → Resolved
    └─ 是，多个 → 检查歧义
                  ├─ 可解析 → Resolved
                  ├─ 歧义 → Ambiguous
                  └─ 内置名称冲突 → ReservedIntrinsicName
```

**错误示例**：

```csharp
// NotFound: 组件不存在
<UnknownButton />

// Ambiguous: 两个命名空间都有 Button
@using Namespace1
@using Namespace2
<Button />  // Namespace1.Button 和 Namespace2.Button 都存在

// ReservedIntrinsicName: 用户定义与内置名称冲突
<template>...</template>  // 用户定义了 template 组件
```

## 3. 核心算法

### 3.1 条目分类算法

**输入**：`INamedTypeSymbol symbol`
**输出**：`RazorVueEntryKind`

```csharp
public RazorVueEntryKind ClassifyEntry(INamedTypeSymbol symbol)
{
    // 1. 基础过滤：必须实现 IVueComponent
    if (!symbol.AllInterfaces.Any(i => i.Name == "IVueComponent"))
        return RazorVueEntryKind.None;

    // 2. 库组件桩不编译
    if (symbol.AllInterfaces.Any(i => i.Name == "IVueLibraryComponent"))
        return RazorVueEntryKind.Invalid;

    // 3. 检查是否包含 Razor 模板
    var syntaxReferences = symbol.DeclaringSyntaxReferences;
    var hasRazorTemplate = syntaxReferences.Any(sr =>
        sr.GetSyntax() is RazorSyntax);

    if (!hasRazorTemplate)
        return RazorVueEntryKind.StaticModule;

    // 4. 完整的 RazorVue 组件
    return RazorVueEntryKind.RazorVueComponent;
}
```

### 3.2 组件标志验证算法

**输入**：`VueComponentFlags flags`
**输出**：`ImmutableArray<RazorVueCompilationIssue>`

```csharp
public ImmutableArray<RazorVueCompilationIssue> ValidateFlags(VueComponentFlags flags)
{
    var issues = ImmutableArray.CreateBuilder<RazorVueCompilationIssue>();

    // 1. 检查互斥标志
    bool hasModelValue = flags.HasFlag(VueComponentFlags.SupportsModelValue);
    bool hasMultipleModels = flags.HasFlag(VueComponentFlags.SupportsMultipleModels);

    if (hasModelValue && hasMultipleModels)
    {
        issues.Add(new RazorVueCompilationIssue(
            RazorVueIssueCode.InvalidLibraryComponentDeclaration,
            RazorVueIssueSeverity.Error,
            "Component cannot have both SupportsModelValue and SupportsMultipleModels flags",
            ImmutableArray<string>.Empty));
    }

    // 2. 检查逻辑一致性
    bool isFormControl = flags.HasFlag(VueComponentFlags.IsFormControl);
    if (isFormControl && !hasModelValue && !hasMultipleModels)
    {
        issues.Add(new RazorVueCompilationIssue(
            RazorVueIssueCode.InvalidLibraryComponentDeclaration,
            RazorVueIssueSeverity.Error,
            "FormControl should have at least one model flag",
            ImmutableArray<string>.Empty));
    }

    return issues.ToImmutable();
}
```

### 3.3 HMR 边界决定算法

**输入**：`RazorVueSemanticSnapshot snapshot`
**输出**：`HmrBoundaryKind`

```csharp
public HmrBoundaryKind DetermineHmrBoundary(RazorVueSemanticSnapshot snapshot)
{
    // 1. 检查不安全的生命周期钩子
    var unsafeHooks = new[] { "OnInitialized", "OnParametersSet" };
    if (snapshot.LifecycleHooks.Any(h => unsafeHooks.Contains(h.Name)))
        return HmrBoundaryKind.FullReloadRequired;

    // 2. 检查状态字段
    if (snapshot.StateFields.Any())
        return HmrBoundaryKind.LogicSafe;

    // 3. 检查计算属性
    if (snapshot.ComputedProperties.Any())
        return HmrBoundaryKind.LogicSafe;

    // 4. 仅模板
    return HmrBoundaryKind.TemplateOnly;
}
```

### 3.4 组件解析算法

**输入**：`string componentName, ImmutableArray<VueComponentDescriptor> allDescriptors`
**输出**：`VueComponentResolutionResult`

```csharp
public VueComponentResolutionResult ResolveComponent(
    string componentName,
    ImmutableArray<VueComponentDescriptor> allDescriptors)
{
    // 1. 检查保留名称
    if (IsReservedIntrinsicName(componentName))
    {
        return VueComponentResolutionResult.ReservedIntrinsicName(
            componentName,
            allDescriptors.Where(d => d.Name == componentName).ToImmutableArray());
    }

    // 2. 查找所有匹配
    var candidates = allDescriptors
        .Where(d => d.Name == componentName)
        .ToImmutableArray();

    // 3. 未找到
    if (candidates.IsEmpty)
    {
        return VueComponentResolutionResult.NotFound(componentName);
    }

    // 4. 唯一匹配
    if (candidates.Length == 1)
    {
        return VueComponentResolutionResult.Resolved(componentName, candidates[0]);
    }

    // 5. 多个匹配（歧义）
    return VueComponentResolutionResult.Ambiguous(componentName, candidates);
}
```

## 4. 线程安全模型

### 4.1 枚举类型本身
所有枚举类型都是值类型，完全线程安全：
- 只读字段
- 不可变性
- 无共享状态

### 4.2 枚举使用场景
- **描述符字段**：`VueComponentDescriptor` 等记录类型是不可变的
- **问题报告**：`RazorVueCompilationIssue` 是不可变记录
- **解析结果**：`VueComponentResolutionResult` 是不可变的

## 5. 错误处理

### 5.1 无效枚举值
**场景**：从外部输入（如配置文件、网络）读取枚举值

**处理**：
```csharp
public static bool TryParse<T>(string value, out T result) where T : struct
{
    if (Enum.TryParse<T>(value, true, out result))
    {
        if (Enum.IsDefined(typeof(T), result))
            return true;
    }
    result = default;
    return false;
}
```

### 5.2 枚举值组合验证
**场景**：`[Flags]` 枚举（如 `VueComponentFlags`）的无效组合

**处理**：参见 3.2 节的标志验证算法

### 5.3 问题报告
**场景**：编译时发现问题

**处理**：创建 `RazorVueCompilationIssue` 实例，包含：
- `Code`（`RazorVueIssueCode`）：错误类型
- `Severity`（`RazorVueIssueSeverity`）：严重性
- `Message`（string）：错误消息
- `RelatedComponentNames`（`ImmutableArray<string>`）：相关组件名称

## 6. 配置选项

### 6.1 枚举值扩展
所有枚举都是 `public` 的，允许未来添加新值：

```csharp
// 未来可能添加
public enum VuePropKind
{
    Normal,
    Model,
    HtmlLike,
    LibrarySpecific,
    Ref,        // 新增：ref 引用
    Slot,       // 新增：插槽属性
    // ...
}
```

### 6.2 兼容性保证
- 添加新枚举值不会破坏现有代码
- 删除或重命名枚举值是破坏性变更
- `[Flags]` 枚举的值应保持 2 的幂次方

## 7. 与其他子系统的交互

### 7.1 与 Descriptor 子系统交互
- **VueComponentDescriptor**：使用 `VueComponentSourceKind`, `VueComponentFlags`
- **VuePropDescriptor**：使用 `VuePropKind`
- **VueEmitDescriptor**：使用 `VueEmitKind`
- **VueComponentResolutionResult**：使用 `VueComponentResolutionStatus`

### 7.2 与 Artifacts 子系统交互
- **VueCompiledArtifact**：使用 `HmrBoundaryKind`
- **RazorVueSourceOrigin**：使用 `RazorVueOriginKind`, `RazorVueMappingQuality`, `RazorVueOriginProvenance`

### 7.3 与 Pipeline 子系统交互
- **RazorVueEntryKind**：用于条目分类
- **RazorVueIssueCode**：用于错误报告

### 7.4 与 LSP 交互
- **VueComponentResolutionStatus**：用于组件解析诊断
- **RazorVueIssueCode**：用于 IDE 错误提示
- **HmrBoundaryKind**：用于开发服务器 HMR 策略

## 8. 设计权衡

### 8.1 多个专用枚举 vs 统一枚举
**选择**：使用多个专用枚举（`VuePropKind`, `VueEmitKind` 等）

**理由**：
- 类型安全（避免混淆 props 和 emits）
- 语义清晰（每个枚举有明确的用途）
- 编译时检查

**代价**：
- 增加了类型数量
- 可能需要额外的转换逻辑

### 8.2 严重性级别单一 vs 多级别
**选择**：当前只有 `Error`

**理由**：
- 简化实现
- RazorVue 早期阶段，优先保证正确性

**未来方向**：
- 计划添加 `Warning`, `Info`, `Hint`
- 支持 LSP 诊断级别

### 8.3 标志位枚举 vs 独立布尔属性
**选择**：使用 `[Flags]` 枚举（`VueComponentFlags`）

**理由**：
- 紧凑存储（一个字段存储多个标志）
- 支持位运算（`flags | flag`, `flags & flag`）
- 易于扩展

**代价**：
- 需要验证标志组合（如互斥检查）
- 可读性略低于独立布尔属性

### 8.4 源映射质量三级 vs 两级
**选择**：三级（`ExactSource`, `MappedFromGenerated`, `GeneratedOnly`）

**理由**：
- 提供更细粒度的质量信息
- 支持不同的调试策略
- 未来可扩展到更多级别

**代价**：
- 增加了复杂度
- 需要在编译时追踪更多信息

---

**文档维护者**：developerhan
**最后更新**：2026-04-21
**文档版本**：v1.0
