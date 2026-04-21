# RazorVue 编译问题（Compilation Issues）

## 为什么需要

RazorVue 编译问题系统提供了一套结构化的诊断机制，用于捕获和报告编译时错误、警告和建议。这套系统确保开发者在编译时就能发现组件定义和使用中的问题，而不是等到运行时才暴露。

编译问题系统解决了以下关键问题：

1. **结构化诊断**：统一的错误格式，便于工具链处理
2. **精确定位**：关联具体的组件和源代码位置
3. **错误分类**：区分不同类型的错误（未找到、歧义、冲突等）
4. **上下文信息**：提供相关的组件名称和建议
5. **异常集成**：与编译器异常系统集成

## 实现思路

### 核心数据结构

#### 1. RazorVueCompilationIssue

位于 `src/Jazor.RazorVue/Descriptor/RazorVueCompilationIssue.cs`：

```csharp
public sealed record RazorVueCompilationIssue(
    RazorVueIssueCode Code,
    RazorVueIssueSeverity Severity,
    string Message,
    ImmutableArray<string> RelatedComponentNames);
```

**字段说明**：

- `Code`：错误代码枚举值
- `Severity`：严重程度（目前只有 Error）
- `Message`：人类可读的错误消息
- `RelatedComponentNames`：相关的组件完全限定名列表（用于歧义引用等场景）

#### 2. RazorVueIssueCode

错误代码枚举，定义了 14 种编译问题：

```csharp
public enum RazorVueIssueCode
{
    // 组件解析错误（3 个）
    ComponentNotFound,                    // 未找到组件
    AmbiguousComponentName,               // 歧义引用
    ReservedIntrinsicNameCollision,       // 与内置组件名冲突

    // 生命周期和逻辑降低错误（2 个）
    UnsupportedLifecycleLowering,         // 不支持的生命周期方法
    UnsupportedSetupLogicLowering,        // 不支持的 setup 逻辑

    // 库组件元数据错误（4 个）
    InvalidLibraryComponentDeclaration,              // 无效的库组件声明
    InvalidLibraryStyleDependencyDeclaration,        // 无效的样式依赖声明
    InvalidLibraryPluginRequirementDeclaration,      // 无效的插件需求声明
    UnknownParameter,                                // 未知参数

    // 参数绑定错误（2 个）
    InvalidBindTarget,                   // 无效的绑定目标
    UnknownSlot,                         // 未知插槽

    // 插槽错误（3 个）
    SlotContextMisuse,                   // 插槽上下文误用
    DuplicateSlotValue                   // 重复的插槽值
}
```

#### 3. RazorVueIssueSeverity

严重程度枚举：

```csharp
public enum RazorVueIssueSeverity
{
    Error  // 目前只支持错误级别
}
```

**未来扩展**：可能添加 `Warning`、`Info`、`Suggestion` 等级别

### 异常类型

#### RazorVueCompilationIssueException

位于 `src/Jazor.RazorVue/Descriptor/RazorVueCompilationIssueException.cs`：

```csharp
public sealed class RazorVueCompilationIssueException : Exception
{
    public RazorVueCompilationIssueException(
        RazorVueCompilationIssue issue,
        string ownerComponentFullName,
        RazorVueSourceOrigin? origin)
        : base(issue?.Message)
    {
        Issue = issue ?? throw new ArgumentNullException(nameof(issue));
        OwnerComponentFullName = ownerComponentFullName ?? string.Empty;
        Origin = origin;
    }

    public RazorVueCompilationIssue Issue { get; }
    public string OwnerComponentFullName { get; }
    public RazorVueSourceOrigin? Origin { get; }
}
```

**字段说明**：

- `Issue`：诊断问题记录
- `OwnerComponentFullName`：拥有此问题的组件完全限定名
- `Origin`：源代码位置信息（文件路径、行号、列号等）

**用途**：
- 在编译时抛出，中断编译过程
- 携带完整的诊断信息供 IDE 和构建工具显示
- 保留源代码位置，支持"跳转到错误"功能

### 诊断工厂

#### RazorVueResolutionIssueFactory

位于 `src/Jazor.RazorVue/Descriptor/RazorVueResolutionIssueFactory.cs`，根据解析状态创建诊断：

```csharp
internal static class RazorVueResolutionIssueFactory
{
    public static ImmutableArray<RazorVueCompilationIssue> Create(
        VueComponentResolutionStatus status,
        string componentName,
        ImmutableArray<VueComponentDescriptor> candidates)
    {
        switch (status)
        {
            case VueComponentResolutionStatus.NotFound:
                return
                [
                    new RazorVueCompilationIssue(
                        RazorVueIssueCode.ComponentNotFound,
                        RazorVueIssueSeverity.Error,
                        $"Component '{componentName}' is not visible in the current RazorVue resolution scope.",
                        [])
                ];

            case VueComponentResolutionStatus.Ambiguous:
                return
                [
                    new RazorVueCompilationIssue(
                        RazorVueIssueCode.AmbiguousComponentName,
                        RazorVueIssueSeverity.Error,
                        $"Component name '{componentName}' is ambiguous. Use a fully-qualified component name.",
                        GetRelatedComponentNames(candidates))
                ];

            case VueComponentResolutionStatus.ReservedIntrinsicName:
                return
                [
                    new RazorVueCompilationIssue(
                        RazorVueIssueCode.ReservedIntrinsicNameCollision,
                        RazorVueIssueSeverity.Error,
                        $"Component name '{componentName}' collides with a reserved intrinsic Vue component name.",
                        GetRelatedComponentNames(candidates))
                ];

            default:
                return [];
        }
    }

    private static ImmutableArray<string> GetRelatedComponentNames(
        ImmutableArray<VueComponentDescriptor> candidates)
        => candidates.IsDefaultOrEmpty
            ? []
            : candidates
                .Select(static candidate => candidate.FullName)
                .Distinct(StringComparer.Ordinal)
                .ToImmutableArray();
}
```

### 错误代码详解

#### 1. 组件解析错误

##### ComponentNotFound

**场景**：引用的组件在当前作用域中不存在

```razor
<MyUnknownButton />  <!-- 错误：MyUnknownButton 不存在 -->
```

**诊断信息**：

```csharp
new RazorVueCompilationIssue(
    RazorVueIssueCode.ComponentNotFound,
    RazorVueIssueSeverity.Error,
    "Component 'MyUnknownButton' is not visible in the current RazorVue resolution scope.",
    [])
```

**解决方案**：
- 检查组件名称拼写
- 添加 `using` 导入组件的命名空间
- 使用完全限定名（如 `App.Components.MyButton`）

##### AmbiguousComponentName

**场景**：多个组件同名且都可见

```razor
@* 当前命名空间：App.Components *@
@* using 导入：App.Shared, ThirdParty.Lib *@

<Button />  <!-- 错误：App.Shared.Button 和 ThirdParty.Lib.Button 都可见 -->
```

**诊断信息**：

```csharp
new RazorVueCompilationIssue(
    RazorVueIssueCode.AmbiguousComponentName,
    RazorVueIssueSeverity.Error,
    "Component name 'Button' is ambiguous. Use a fully-qualified component name.",
    ["App.Shared.Button", "ThirdParty.Lib.Button"])
```

**解决方案**：
- 使用完全限定名：`<App.Shared.Button />` 或 `<ThirdParty.Lib.Button />`
- 移除不必要的 `using` 导入

##### ReservedIntrinsicNameCollision

**场景**：用户组件或库组件与内置组件同名

```csharp
// 用户组件
public class Teleport : ComponentBase
{
    // ...
}
```

```razor
<Teleport />  <!-- 错误：与内置组件 Teleport 冲突 -->
```

**诊断信息**：

```csharp
new RazorVueCompilationIssue(
    RazorVueIssueCode.ReservedIntrinsicNameCollision,
    RazorVueIssueSeverity.Error,
    "Component name 'Teleport' collides with a reserved intrinsic Vue component name.",
    ["ECMAScript.UI.Vue.Teleport", "App.Components.Teleport"])
```

**解决方案**：
- 重命名用户组件
- 使用完全限定名引用用户组件：`<App.Components.Teleport />`

#### 2. 生命周期和逻辑降低错误

##### UnsupportedLifecycleLowering

**场景**：使用了不支持的生命周期方法

```csharp
@code {
    // 错误：RazorVue 不支持此方法
    protected override Task OnAfterRenderAsync(bool firstRender)
    {
        return base.OnAfterRenderAsync(firstRender);
    }
}
```

**诊断信息**：

```csharp
new RazorVueCompilationIssue(
    RazorVueIssueCode.UnsupportedLifecycleLowering,
    RazorVueIssueSeverity.Error,
    "Lifecycle method 'OnAfterRenderAsync' is not supported in RazorVue lowering.",
    [])
```

**解决方案**：
- 参考 RazorVue 文档，使用支持的生命周期方法
- 使用 Vue Composition API 的 `onMounted`、`onUpdated` 等钩子

##### UnsupportedSetupLogicLowering

**场景**：组件中包含无法降低到 Vue setup 的逻辑

```csharp
@code {
    // 错误：不支持的字段类型
    private IServiceProvider serviceProvider;  // 无法序列化到 JavaScript
}
```

**诊断信息**：

```csharp
new RazorVueCompilationIssue(
    RazorVueIssueCode.UnsupportedSetupLogicLowering,
    RazorVueIssueSeverity.Error,
    "Field 'serviceProvider' of type 'IServiceProvider' cannot be lowered to Vue setup logic.",
    [])
```

**解决方案**：
- 避免使用 .NET 特定类型（如 `IServiceProvider`、`HttpContext`）
- 使用简单类型（string、int、bool、自定义 class 等）

#### 3. 库组件元数据错误

##### InvalidLibraryComponentDeclaration

**场景**：库组件声明不完整或错误

```csharp
// 错误：缺少必需的 [VueLibraryComponent] 特性
public partial class MyLibButton : ComponentBase
{
    // ...
}

// 错误：参数不完整
[VueLibraryComponent("vuetify")]  // 缺少 exportName
public partial class VBtn : ComponentBase
{
    // ...
}
```

**诊断信息**：

```csharp
new RazorVueCompilationIssue(
    RazorVueIssueCode.InvalidLibraryComponentDeclaration,
    RazorVueIssueSeverity.Error,
    "Library component 'ThirdParty.Vuetify.VBtn' must declare [VueLibraryComponent(importSpecifier, exportName)].",
    [])
```

**解决方案**：
- 声明完整的 `[VueLibraryComponent(importSpecifier, exportName)]` 特性
- 确保参数类型正确（string，非空）

##### InvalidLibraryStyleDependencyDeclaration

**场景**：样式依赖声明错误

```csharp
[VueLibraryStyle()]  // 错误：缺少样式说明符
[VueLibraryStyle(123)]  // 错误：参数类型错误
public partial class VBtn : ComponentBase
{
    // ...
}
```

**诊断信息**：

```csharp
new RazorVueCompilationIssue(
    RazorVueIssueCode.InvalidLibraryStyleDependencyDeclaration,
    RazorVueIssueSeverity.Error,
    "Library component 'ThirdParty.Vuetify.VBtn' has an invalid [VueLibraryStyle(styleSpecifier)] declaration.",
    [])
```

**解决方案**：
- 使用字符串字面量：`[VueLibraryStyle("vuetify/lib/components/VBtn/VBtn.css")]`

##### InvalidLibraryPluginRequirementDeclaration

**场景**：插件需求声明错误

```csharp
[VueLibraryPluginRequirement()]  // 错误：缺少需求 ID
[VueLibraryPluginRequirement(123)]  // 错误：参数类型错误
public partial class VDataTable : ComponentBase
{
    // ...
}
```

**诊断信息**：

```csharp
new RazorVueCompilationIssue(
    RazorVueIssueCode.InvalidLibraryPluginRequirementDeclaration,
    RazorVueIssueSeverity.Error,
    "Library component 'ThirdParty.Vuetify.VDataTable' has an invalid [VueLibraryPluginRequirement(requirementId)] declaration.",
    [])
```

**解决方案**：
- 使用字符串字面量：`[VueLibraryPluginRequirement("vuetify")]`

##### UnknownParameter

**场景**：使用了组件中不存在的参数

```razor
@* 组件定义：只有 Color 和 Size 参数 *@
<VBtn Unknown="value" />  <!-- 错误：Unknown 不存在 -->
```

**诊断信息**：

```csharp
new RazorVueCompilationIssue(
    RazorVueIssueCode.UnknownParameter,
    RazorVueIssueSeverity.Error,
    "Component 'ThirdParty.Vuetify.VBtn' does not have a parameter named 'Unknown'.",
    ["ThirdParty.Vuetify.VBtn"])
```

**解决方案**：
- 检查参数名称拼写
- 查看组件文档确认可用参数

#### 4. 参数绑定错误

##### InvalidBindTarget

**场景**：尝试绑定到不支持双向绑定的参数

```razor
@* 组件定义：Text 参数未声明 AcceptsBinding *@
<VTextBox @bind-Text="textValue" />  <!-- 错误：Text 不支持绑定 -->
```

**诊断信息**：

```csharp
new RazorVueCompilationIssue(
    RazorVueIssueCode.InvalidBindTarget,
    RazorVueIssueSeverity.Error,
    "Parameter 'Text' on component 'App.Components.VTextBox' does not accept two-way binding.",
    ["App.Components.VTextBox"])
```

**解决方案**：
- 移除 `@bind-` 前缀，使用单向绑定：`Text="@textValue"`
- 或在组件中添加对应的 `TextChanged` 参数

#### 5. 插槽错误

##### UnknownSlot

**场景**：使用了组件中不存在的插槽

```razor
@* 组件定义：只有 default 和 header 插槽 *@
<VCard>
    <Footer>  <!-- 错误：Footer 插槽不存在 -->
        <p>Footer content</p>
    </Footer>
</VCard>
```

**诊断信息**：

```csharp
new RazorVueCompilationIssue(
    RazorVueIssueCode.UnknownSlot,
    RazorVueIssueSeverity.Error,
    "Component 'App.Components.VCard' does not have a slot named 'Footer'.",
    ["App.Components.VCard"])
```

**解决方案**：
- 检查插槽名称拼写
- 查看组件文档确认可用插槽

##### SlotContextMisuse

**场景**：在非作用域插槽中使用插槽参数

```razor
@* 组件定义：Items 是 RenderFragment<ItemContext> *@
<VCard>
    <Items>  <!-- 错误：应该使用 @context="@item" 语法 -->
        <div>@item.Name</div>
    </Items>
</VCard>
```

**诊断信息**：

```csharp
new RazorVueCompilationIssue(
    RazorVueIssueCode.SlotContextMisuse,
    RazorVueIssueSeverity.Error,
    "Slot 'Items' on component 'App.Components.VCard' requires a context parameter. Use '@context=\"@item\"' syntax.",
    ["App.Components.VCard"])
```

**解决方案**：
- 使用正确的插槽上下文语法：`<Items @context="@item">`

##### DuplicateSlotValue

**场景**：为单个插槽多次提供值

```razor
<VCard>
    <Header>
        <h1>First Header</h1>
    </Header>
    <Header>  <!-- 错误：Header 插槽已有值 -->
        <h2>Second Header</h2>
    </Header>
</VCard>
```

**诊断信息**：

```csharp
new RazorVueCompilationIssue(
    RazorVueIssueCode.DuplicateSlotValue,
    RazorVueIssueSeverity.Error,
    "Slot 'Header' on component 'App.Components.VCard' has multiple values. Only one value is allowed.",
    ["App.Components.VCard"])
```

**解决方案**：
- 移除重复的插槽值
- 或使用不同名称的插槽

### 错误报告示例

#### 单个错误

```csharp
// 解析失败
var result = registry.Resolve("UnknownButton", context);

// 结果
result.Status  // NotFound
result.Issues.Length  // 1
result.Issues[0].Code  // ComponentNotFound
result.Issues[0].Message  // "Component 'UnknownButton' is not visible in the current RazorVue resolution scope."
result.Issues[0].Severity  // Error
result.Issues[0].RelatedComponentNames  // []
```

#### 多个相关错误

```csharp
// 歧义引用
var result = registry.Resolve("Button", context);

// 结果
result.Status  // Ambiguous
result.Issues.Length  // 1
result.Issues[0].Code  // AmbiguousComponentName
result.Issues[0].RelatedComponentNames.Length  // 2
result.Issues[0].RelatedComponentNames[0]  // "App.Shared.Button"
result.Issues[0].RelatedComponentNames[1]  // "ThirdParty.Lib.Button"
```

#### 异常抛出

```csharp
// 库组件元数据错误
try
{
    var descriptor = VueComponentDescriptorFactory.CreateLibraryComponent(
        componentSymbol,
        context);
}
catch (RazorVueCompilationIssueException ex)
{
    Console.WriteLine($"Error: {ex.Issue.Message}");
    Console.WriteLine($"Component: {ex.OwnerComponentFullName}");
    Console.WriteLine($"Location: {ex.Origin?.FilePath}:{ex.Origin?.Line}:{ex.Origin?.Column}");
}
```

**输出**：

```
Error: Library component 'ThirdParty.Vuetify.VBtn' must declare [VueLibraryComponent(importSpecifier, exportName)].
Component: ThirdParty.Vuetify.VBtn
Location: src/Vuetify/VBtn.cs:10:2
```

## 设计权衡

### 为什么只有 Error 级别

目前 `RazorVueIssueSeverity` 只定义了 `Error` 级别，原因：

1. **早期阶段**：RazorVue 仍在快速迭代，优先保证正确性
2. **简单性**：减少编译器的复杂度
3. **明确的失败**：所有问题都需要开发者处理

**未来扩展**：可能添加 `Warning`（如过时 API）、`Info`（如性能建议）、`Suggestion`（如代码风格）

### 为什么使用 Record 类型

使用 C# record 类型而不是 class：

1. **不可变性**：诊断信息一旦创建不应修改
2. **值语义**：相同内容的诊断相等
3. **简洁性**：主构造器语法更简洁
4. **模式匹配**：便于 switch 表达式

## 文件位置

- **诊断记录**：`src/Jazor.RazorVue/Descriptor/RazorVueCompilationIssue.cs`
- **异常类型**：`src/Jazor.RazorVue/Descriptor/RazorVueCompilationIssueException.cs`
- **诊断工厂**：`src/Jazor.RazorVue/Descriptor/RazorVueResolutionIssueFactory.cs`

## 相关文档

- **组件描述符**：`docs/01-目标/razorvue/descriptor/ComponentDescriptor.md`
- **组件注册表**：`docs/01-目标/razorvue/descriptor/ComponentRegistry.md`
- **描述符工厂**：`docs/01-目标/razorvue/descriptor/DescriptorFactory.md`
- **内置组件**：`docs/01-目标/razorvue/descriptor/IntrinsicComponents.md`

---

**维护者**：developerhan
**最后更新**：2026-04-21
**文档版本**：v1.0
