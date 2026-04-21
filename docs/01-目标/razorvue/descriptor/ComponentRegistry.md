# Vue 组件注册表（Component Registry）

## 为什么需要

Vue 组件注册表是 RazorVue 编译时解析的核心引擎，负责管理所有可用的 Vue 组件（用户组件、库组件、内置组件），并提供组件名称解析服务。它实现了类似 C# 命名空间可见性的解析规则，确保组件引用的正确性和唯一性。

注册表解决了以下关键问题：

1. **组件索引**：按名称、全名、解析命名空间建立多级索引
2. **可见性判断**：基于当前命名空间和 using 导入判断组件可见性
3. **冲突检测**：发现名称冲突和保留字冲突
4. **优先级处理**：内置组件 > 用户组件 > 库组件
5. **诊断生成**：为解析失败创建详细的错误信息

## 实现思路

### 核心数据结构

注册表类位于 `src/Jazor.RazorVue/Descriptor/VueComponentRegistry.cs`：

```csharp
public sealed class VueComponentRegistry
{
    private static readonly ImmutableArray<VueComponentDescriptor> IntrinsicComponents =
        VueIntrinsicComponentDescriptors.All;

    public ImmutableArray<VueComponentDescriptor> Components { get; }

    public ImmutableDictionary<string, ImmutableArray<VueComponentDescriptor>> ComponentsByName { get; }

    public ImmutableDictionary<string, VueComponentDescriptor> ComponentsByFullName { get; }

    public ImmutableDictionary<string, ImmutableArray<VueComponentDescriptor>> ComponentsByResolutionNamespace { get; }
}
```

**索引说明**：

- `Components`：所有组件的扁平列表
- `ComponentsByName`：按短名称索引（可能有多个同名组件）
- `ComponentsByFullName`：按完全限定名索引（唯一）
- `ComponentsByResolutionNamespace`：按解析命名空间索引

### 注册表创建

#### 1. 从语义快照创建

```csharp
public static VueComponentRegistry Create(
    ImmutableArray<RazorVueSemanticSnapshot> userSnapshots,
    ImmutableArray<VueComponentDescriptor> libraryComponents = default)
{
    var userComponents = userSnapshots.IsDefault
        ? ImmutableArray<VueComponentDescriptor>.Empty
        : userSnapshots.Select(static snapshot => snapshot.Descriptor).ToImmutableArray();

    return Create(userComponents, libraryComponents);
}
```

**使用场景**：编译时从所有用户组件快照创建全局注册表

#### 2. 从描述符集合创建

```csharp
public static VueComponentRegistry Create(
    ImmutableArray<VueComponentDescriptor> userComponents,
    ImmutableArray<VueComponentDescriptor> libraryComponents = default)
{
    var allComponents = ImmutableArray.CreateBuilder<VueComponentDescriptor>();

    // 按优先级添加：内置 > 用户 > 库
    AddRange(allComponents, IntrinsicComponents);
    AddRange(allComponents, userComponents);
    AddRange(allComponents, libraryComponents);

    // 按名称索引
    var byName = allComponents
        .GroupBy(static descriptor => descriptor.Name, StringComparer.Ordinal)
        .ToImmutableDictionary(
            static group => group.Key,
            static group => group.ToImmutableArray(),
            StringComparer.Ordinal);

    // 按全名索引
    var byFullName = ImmutableDictionary.CreateBuilder<string, VueComponentDescriptor>(StringComparer.Ordinal);
    foreach (var component in allComponents)
        byFullName[component.FullName] = component;

    // 按命名空间索引
    var byResolutionNamespace = allComponents
        .GroupBy(static descriptor => descriptor.ResolutionNamespace ?? string.Empty, StringComparer.Ordinal)
        .ToImmutableDictionary(
            static group => group.Key,
            static group => group.ToImmutableArray(),
            StringComparer.Ordinal);

    return new VueComponentRegistry(
        allComponents.ToImmutable(),
        byName,
        byFullName.ToImmutable(),
        byResolutionNamespace);
}
```

**特点**：
- 三级优先级：内置组件 > 用户组件 > 库组件
- 所有索引使用 `StringComparer.Ordinal`（区分大小写）
- 空命名空间映射为 `string.Empty`

### 组件解析逻辑

#### Resolve 方法

```csharp
public VueComponentResolutionResult Resolve(
    string componentName,
    VueComponentResolutionContext context)
{
    if (componentName is null)
        throw new ArgumentNullException(nameof(componentName));
    if (context is null)
        throw new ArgumentNullException(nameof(context));

    // 1. 完全限定名解析（带命名空间的名称）
    if (componentName.IndexOf('.') >= 0)
    {
        return ComponentsByFullName.TryGetValue(componentName, out var exactDescriptor)
            ? VueComponentResolutionResult.Resolved(componentName, exactDescriptor)
            : VueComponentResolutionResult.NotFound(componentName);
    }

    // 2. 短名称解析
    if (!ComponentsByName.TryGetValue(componentName, out var candidates))
        return VueComponentResolutionResult.NotFound(componentName);

    // 3. 分离内置组件和可见组件
    var intrinsicMatches = candidates
        .Where(static descriptor => descriptor.SourceKind == VueComponentSourceKind.Intrinsic)
        .ToImmutableArray();

    var visibleMatches = candidates
        .Where(descriptor => descriptor.SourceKind != VueComponentSourceKind.Intrinsic && IsVisible(descriptor, context))
        .ToImmutableArray();

    // 4. 处理内置组件冲突
    if (intrinsicMatches.Length > 0)
    {
        // 内置组件名称是保留字。如果可见组件与内置组件同名，
        // 必须报告冲突而不是静默隐藏内置组件。
        return visibleMatches.Length > 0
            ? VueComponentResolutionResult.ReservedIntrinsicName(
                componentName,
                intrinsicMatches.AddRange(visibleMatches))
            : VueComponentResolutionResult.Resolved(componentName, intrinsicMatches[0]);
    }

    // 5. 未找到可见组件
    if (visibleMatches.Length == 0)
        return VueComponentResolutionResult.NotFound(componentName);

    // 6. 唯一匹配
    if (visibleMatches.Length == 1)
        return VueComponentResolutionResult.Resolved(componentName, visibleMatches[0]);

    // 7. 歧义引用
    return VueComponentResolutionResult.Ambiguous(componentName, visibleMatches);
}
```

### 可见性判断

#### IsVisible 方法

```csharp
private static bool IsVisible(
    VueComponentDescriptor descriptor,
    VueComponentResolutionContext context)
{
    // 1. 当前命名空间的组件总是可见
    if (string.Equals(descriptor.ResolutionNamespace, context.CurrentNamespace, StringComparison.Ordinal))
        return true;

    // 2. 通过 using 导入的命名空间可见
    return context.Imports.Contains(descriptor.ResolutionNamespace, StringComparer.Ordinal);
}
```

**可见性规则**：

```csharp
// 解析上下文
var context = new VueComponentResolutionContext(
    currentNamespace: "App.Components",
    imports: ["App.Shared", "ThirdParty.Lib"]);

// 示例 1：当前命名空间
// App.Components.MyButton → 可见

// 示例 2：using 导入
// App.Shared.Button → 可见
// ThirdParty.Lib.Button → 可见

// 示例 3：不可见
// App.Models.Button → 不可见（未导入）
// Internal.Utils.Button → 不可见（未导入）
```

### 解析结果类型

#### VueComponentResolutionResult

```csharp
public sealed class VueComponentResolutionResult
{
    public VueComponentResolutionStatus Status { get; }
    public string ComponentName { get; }
    public VueComponentDescriptor? Descriptor { get; }
    public ImmutableArray<VueComponentDescriptor> Candidates { get; }
    public ImmutableArray<RazorVueCompilationIssue> Issues { get; }
}
```

#### 解析状态

```csharp
public enum VueComponentResolutionStatus
{
    Resolved,              // 成功解析
    NotFound,              // 未找到组件
    Ambiguous,             // 歧义引用
    ReservedIntrinsicName  // 与内置组件名冲突
}
```

#### 状态工厂方法

```csharp
// 成功解析
public static VueComponentResolutionResult Resolved(
    string componentName,
    VueComponentDescriptor descriptor)
    => new(
        VueComponentResolutionStatus.Resolved,
        componentName,
        descriptor,
        [descriptor],
        []);

// 未找到
public static VueComponentResolutionResult NotFound(string componentName)
    => new(
        VueComponentResolutionStatus.NotFound,
        componentName,
        null,
        [],
        RazorVueResolutionIssueFactory.Create(
            VueComponentResolutionStatus.NotFound,
            componentName,
            []));

// 歧义引用
public static VueComponentResolutionResult Ambiguous(
    string componentName,
    ImmutableArray<VueComponentDescriptor> candidates)
    => new(
        VueComponentResolutionStatus.Ambiguous,
        componentName,
        null,
        candidates,
        RazorVueResolutionIssueFactory.Create(
            VueComponentResolutionStatus.Ambiguous,
            componentName,
            candidates));

// 保留字冲突
public static VueComponentResolutionResult ReservedIntrinsicName(
    string componentName,
    ImmutableArray<VueComponentDescriptor> candidates)
    => new(
        VueComponentResolutionStatus.ReservedIntrinsicName,
        componentName,
        null,
        candidates,
        RazorVueResolutionIssueFactory.Create(
            VueComponentResolutionStatus.ReservedIntrinsicName,
            componentName,
            candidates));
```

### 解析上下文

#### VueComponentResolutionContext

```csharp
public sealed class VueComponentResolutionContext
{
    public VueComponentResolutionContext(
        string currentNamespace,
        ImmutableArray<string> imports)
    {
        CurrentNamespace = currentNamespace ?? string.Empty;
        Imports = imports.IsDefault ? ImmutableArray<string>.Empty : imports;
    }

    public string CurrentNamespace { get; }
    public ImmutableArray<string> Imports { get; }

    public static VueComponentResolutionContext Create(
        string currentNamespace,
        params string[] imports)
        => new(
            currentNamespace,
            imports is null ? ImmutableArray<string>.Empty : ImmutableArray.Create(imports));
}
```

**用途**：
- `CurrentNamespace`：当前正在编译的组件所在的命名空间
- `Imports`：当前文件中 `using` 导入的命名空间列表

### 解析示例

#### 示例 1：完全限定名解析

```csharp
// 上下文
var context = new VueComponentResolutionContext("App.Components", []);

// 解析
var result = registry.Resolve("App.Shared.Button", context);

// 结果
result.Status  // Resolved
result.Descriptor.FullName  // "App.Shared.Button"
```

#### 示例 2：短名称解析（当前命名空间）

```csharp
// 上下文
var context = new VueComponentResolutionContext("App.Components", []);

// 注册表包含：App.Components.Button
var result = registry.Resolve("Button", context);

// 结果
result.Status  // Resolved
result.Descriptor.FullName  // "App.Components.Button"
```

#### 示例 3：短名称解析（using 导入）

```csharp
// 上下文
var context = new VueComponentResolutionContext("App.Components", ["App.Shared"]);

// 注册表包含：App.Shared.Button
var result = registry.Resolve("Button", context);

// 结果
result.Status  // Resolved
result.Descriptor.FullName  // "App.Shared.Button"
```

#### 示例 4：歧义引用

```csharp
// 上下文
var context = new VueComponentResolutionContext("App.Components", ["App.Shared", "ThirdParty.Lib"]);

// 注册表包含：App.Shared.Button 和 ThirdParty.Lib.Button
var result = registry.Resolve("Button", context);

// 结果
result.Status  // Ambiguous
result.Candidates.Length  // 2
result.Candidates[0].FullName  // "App.Shared.Button"
result.Candidates[1].FullName  // "ThirdParty.Lib.Button"
result.Issues[0].Code  // RazorVueIssueCode.AmbiguousComponentName
result.Issues[0].Message  // "Component name 'Button' is ambiguous. Use a fully-qualified component name."
```

#### 示例 5：内置组件优先

```csharp
// 上下文
var context = new VueComponentResolutionContext("App.Components", []);

// 注册表包含：ECMAScript.UI.Vue.Teleport（内置）和 App.Components.Teleport（用户）
var result = registry.Resolve("Teleport", context);

// 结果
result.Status  // ReservedIntrinsicName
result.Candidates.Length  // 2
result.Candidates[0].FullName  // "ECMAScript.UI.Vue.Teleport"
result.Candidates[1].FullName  // "App.Components.Teleport"
result.Issues[0].Code  // RazorVueIssueCode.ReservedIntrinsicNameCollision
result.Issues[0].Message  // "Component name 'Teleport' collides with a reserved intrinsic Vue component name."
```

#### 示例 6：未找到组件

```csharp
// 上下文
var context = new VueComponentResolutionContext("App.Components", []);

// 注册表不包含 UnknownButton
var result = registry.Resolve("UnknownButton", context);

// 结果
result.Status  // NotFound
result.Descriptor  // null
result.Issues[0].Code  // RazorVueIssueCode.ComponentNotFound
result.Issues[0].Message  // "Component 'UnknownButton' is not visible in the current RazorVue resolution scope."
```

### 诊断工厂

#### RazorVueResolutionIssueFactory

位于 `src/Jazor.RazorVue/Descriptor/RazorVueResolutionIssueFactory.cs`：

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

## 文件位置

- **注册表类**：`src/Jazor.RazorVue/Descriptor/VueComponentRegistry.cs`
- **解析结果**：`src/Jazor.RazorVue/Descriptor/VueComponentResolutionResult.cs`
- **解析上下文**：`src/Jazor.RazorVue/Descriptor/VueComponentResolutionContext.cs`
- **诊断工厂**：`src/Jazor.RazorVue/Descriptor/RazorVueResolutionIssueFactory.cs`

## 相关文档

- **组件描述符**：`docs/01-目标/razorvue/descriptor/ComponentDescriptor.md`
- **描述符工厂**：`docs/01-目标/razorvue/descriptor/DescriptorFactory.md`
- **内置组件**：`docs/01-目标/razorvue/descriptor/IntrinsicComponents.md`
- **编译问题**：`docs/01-目标/razorvue/descriptor/CompilationIssues.md`

---

**维护者**：developerhan
**最后更新**：2026-04-21
**文档版本**：v1.0
