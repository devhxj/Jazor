# RazorVueEntryClassifier - 组件入口分类器

**文件路径**: `src/Jazor.RazorVue/Discovery/RazorVueEntryClassifier.cs`

## 为什么需要

RazorVue 系统需要从编译中识别出哪些类型是有效的 RazorVue 组件入口。由于 `[ECMAScriptModule]` 特性既用于传统静态模块，也用于 RazorVue 组件，因此需要一个分类器来区分不同的入口类型，并为语义提取阶段提供生命周期和逻辑成员发现能力。

## 实现思路

### 核心职责

`RazorVueEntryClassifier` 是一个 `internal static` 类，提供两大功能：

1. **入口分类**：将带有 `[ECMAScriptModule]` 的类型分类为 `None`/`StaticModule`/`RazorVueComponent`/`Invalid`
2. **成员发现**：查找组件的生命周期方法和逻辑成员（方法/字段）

### 分类逻辑

#### 1. `HasECMAScriptModuleAttribute`

检测类型是否带有 `[ECMAScriptModule]` 特性：

```csharp
public static bool HasECMAScriptModuleAttribute(INamedTypeSymbol symbol, RazorVueCompilationSymbols symbols)
    => symbol.GetAttributes().Any(attribute => Comparer.Equals(attribute.AttributeClass, symbols.ECMAScriptModuleAttribute));
```

#### 2. `Classify` 分类方法

分类决策树：

```csharp
public static RazorVueEntryKind Classify(INamedTypeSymbol symbol, RazorVueCompilationSymbols symbols)
{
    // 1. 必须带有 [ECMAScriptModule] 特性
    if (!HasECMAScriptModuleAttribute(symbol, symbols))
        return RazorVueEntryKind.None;

    // 2. 静态类 = 传统静态模块路径
    if (symbol.IsStatic)
        return RazorVueEntryKind.StaticModule;

    // 3. 必须继承自 ComponentBase
    if (!DerivesFrom(symbol, symbols.ComponentBase))
        return RazorVueEntryKind.None;

    // 4. 必须继承自 JazorComponent，否则为无效
    return DerivesFrom(symbol, symbols.JazorComponent)
        ? RazorVueEntryKind.RazorVueComponent
        : RazorVueEntryKind.Invalid;
}
```

**分类结果**：

| 条件 | 结果 | 说明 |
|------|------|------|
| 无 `[ECMAScriptModule]` | `None` | 不是 RazorVue 入口 |
| 静态类 | `StaticModule` | 传统静态模块，保持现有路径 |
| 非静态 + 继承 `ComponentBase` + 继承 `JazorComponent` | `RazorVueComponent` | 有效的 RazorVue 组件 |
| 非静态 + 继承 `ComponentBase` + 不继承 `JazorComponent` | `Invalid` | 无效的组件配置 |

#### 3. `IsDirectComponentBaseEntry`

检测是否是直接继承 `ComponentBase` 的入口（非间接继承）：

```csharp
public static bool IsDirectComponentBaseEntry(INamedTypeSymbol symbol, RazorVueCompilationSymbols symbols)
    => HasECMAScriptModuleAttribute(symbol, symbols) &&
       !symbol.IsStatic &&
       Comparer.Equals(symbol.BaseType?.OriginalDefinition, symbols.ComponentBase);
```

**用途**：区分直接继承和间接继承的组件，影响语义提取策略。

#### 4. `IsInRazorVueScope`

检测嵌套类型是否在 RazorVue 组件作用域内：

```csharp
public static bool IsInRazorVueScope(INamedTypeSymbol symbol, RazorVueCompilationSymbols symbols)
{
    for (var current = symbol; current is not null; current = current.ContainingType)
    {
        var entryKind = Classify(current, symbols);
        if (entryKind is RazorVueEntryKind.RazorVueComponent or RazorVueEntryKind.Invalid)
            return true;
    }

    return false;
}
```

**作用域规则**：
- 向上遍历 `ContainingType` 链
- 如果任何父级是 `RazorVueComponent` 或 `Invalid`，则在作用域内
- 用于判断嵌套类型是否应参与组件语义提取

#### 5. `IsLibraryComponent`

检测是否是库组件（`IVueLibraryComponent` 派生）：

```csharp
public static bool IsLibraryComponent(INamedTypeSymbol symbol, RazorVueCompilationSymbols symbols)
{
    if (symbols.VueLibraryComponent is null || symbol.IsStatic || symbol.IsAbstract)
        return false;

    return !Comparer.Equals(symbol.OriginalDefinition, symbols.VueLibraryComponent) &&
           DerivesFrom(symbol, symbols.VueLibraryComponent);
}
```

**排除条件**：
- `VueLibraryComponent` 符号未定义
- 静态类或抽象类
- 是 `IVueLibraryComponent` 接口本身

### 生命周期方法发现

#### 6. `FindBuildRenderTreeMethod`

查找 `BuildRenderTree` 方法：

```csharp
public static IMethodSymbol? FindBuildRenderTreeMethod(INamedTypeSymbol symbol)
    => FindHierarchyMethod(symbol, "BuildRenderTree", static method =>
        method.Parameters.Length == 1 &&
        method.MethodKind == MethodKind.Ordinary);
```

**匹配条件**：
- 方法名：`BuildRenderTree`
- 参数数量：1 个
- 方法类型：普通方法（非静态、非构造函数等）

#### 7. 初始化生命周期

```csharp
public static IMethodSymbol? FindOnInitializedMethod(INamedTypeSymbol symbol)
    => FindOrdinaryMethod(symbol, "OnInitialized", parameterCount: 0);

public static IMethodSymbol? FindOnInitializedAsyncMethod(INamedTypeSymbol symbol)
    => FindOrdinaryMethod(symbol, "OnInitializedAsync", parameterCount: 0);
```

#### 8. 参数设置生命周期

```csharp
public static IMethodSymbol? FindOnParametersSetMethod(INamedTypeSymbol symbol)
    => FindOrdinaryMethod(symbol, "OnParametersSet", parameterCount: 0);

public static IMethodSymbol? FindOnParametersSetAsyncMethod(INamedTypeSymbol symbol)
    => FindOrdinaryMethod(symbol, "OnParametersSetAsync", parameterCount: 0);
```

#### 9. 渲染后生命周期

```csharp
public static IMethodSymbol? FindOnAfterRenderMethod(INamedTypeSymbol symbol)
    => FindOrdinaryMethod(symbol, "OnAfterRender", parameterCount: 1);

public static IMethodSymbol? FindOnAfterRenderAsyncMethod(INamedTypeSymbol symbol)
    => FindOrdinaryMethod(symbol, "OnAfterRenderAsync", parameterCount: 1);
```

#### 10. 渲染控制

```csharp
public static IMethodSymbol? FindShouldRenderMethod(INamedTypeSymbol symbol)
    => FindOrdinaryMethod(symbol, "ShouldRender", parameterCount: 0);

public static IMethodSymbol? FindSetParametersAsyncMethod(INamedTypeSymbol symbol)
    => FindOrdinaryMethod(symbol, "SetParametersAsync", parameterCount: 1);
```

#### 11. 资源清理

```csharp
public static IMethodSymbol? FindDisposeMethod(INamedTypeSymbol symbol)
    => FindOrdinaryMethod(symbol, "Dispose", parameterCount: 0);

public static IMethodSymbol? FindDisposeAsyncMethod(INamedTypeSymbol symbol)
    => FindOrdinaryMethod(symbol, "DisposeAsync", parameterCount: 0);
```

### 逻辑成员发现

#### 12. `FindLogicMethods`

查找所有逻辑方法（排除生命周期方法）：

```csharp
public static ImmutableArray<IMethodSymbol> FindLogicMethods(INamedTypeSymbol symbol)
{
    var builder = ImmutableArray.CreateBuilder<IMethodSymbol>();
    var seenSignatures = new HashSet<string>(StringComparer.Ordinal);

    for (var current = symbol; current is not null; current = current.BaseType)
    {
        foreach (var method in current.GetMembers().OfType<IMethodSymbol>())
        {
            // 排除静态方法
            if (method.MethodKind != MethodKind.Ordinary || method.IsStatic)
                continue;

            // 排除非源码成员（如框架生成的方法）
            if (!method.Locations.Any(static location => location.IsInSource))
                continue;

            // 排除生命周期方法
            if (method.Name is "BuildRenderTree" or "OnInitialized" or "OnInitializedAsync"
                or "OnParametersSet" or "OnParametersSetAsync" or "OnAfterRender"
                or "OnAfterRenderAsync" or "ShouldRender" or "SetParametersAsync"
                or "Dispose" or "DisposeAsync")
                continue;

            // 去重：使用签名去重
            var signature = method.ToDisplayString(SymbolDisplayFormat.CSharpErrorMessageFormat);
            if (!seenSignatures.Add(signature))
                continue;

            builder.Add(method);
        }
    }

    return builder.ToImmutable();
}
```

**排除规则**：
1. 静态方法
2. 非源码成员（从元数据或引用程序集继承）
3. 生命周期方法（11 个已知方法名）
4. 重复签名（继承链中的重写方法）

**去重策略**：使用 `CSharpErrorMessageFormat` 签名去重，确保同一方法的不同重写版本只保留一个。

#### 13. `FindLogicFields`

查找所有逻辑字段：

```csharp
public static ImmutableArray<IFieldSymbol> FindLogicFields(INamedTypeSymbol symbol)
{
    var builder = ImmutableArray.CreateBuilder<IFieldSymbol>();
    var seenNames = new HashSet<string>(StringComparer.Ordinal);

    for (var current = symbol; current is not null; current = current.BaseType)
    {
        foreach (var field in current.GetMembers().OfType<IFieldSymbol>())
        {
            // 保守策略：排除静态字段和非源码字段
            if (field.IsStatic || !field.Locations.Any(static location => location.IsInSource))
                continue;

            // 排除属性关联字段（自动属性生成的 backing field）
            if (field.AssociatedSymbol is not null || field.IsImplicitlyDeclared)
                continue;

            // 去重：使用字段名去重
            if (!seenNames.Add(field.Name))
                continue;

            builder.Add(field);
        }
    }

    return builder.ToImmutable();
}
```

**排除规则**：
1. 静态字段
2. 非源码字段
3. 属性关联字段（`AssociatedSymbol != null`）
4. 隐式声明字段（`IsImplicitlyDeclared == true`，如闭包捕获字段）

**保守策略**：注释说明 "Keep setup-side field discovery conservative until lowering exists"，当前阶段保守处理，等待后续 lowering 阶段完善。

### 辅助方法

#### 14. `FindOrdinaryMethod`

通用普通方法查找器：

```csharp
private static IMethodSymbol? FindOrdinaryMethod(INamedTypeSymbol symbol, string methodName, int parameterCount)
    => FindHierarchyMethod(symbol, methodName, method =>
        method.MethodKind == MethodKind.Ordinary &&
        method.Parameters.Length == parameterCount);
```

#### 15. `FindHierarchyMethod`

继承链方法查找器：

```csharp
private static IMethodSymbol? FindHierarchyMethod(
    INamedTypeSymbol symbol,
    string methodName,
    Func<IMethodSymbol, bool> predicate)
{
    for (var current = symbol; current is not null; current = current.BaseType)
    {
        var method = current.GetMembers(methodName)
            .OfType<IMethodSymbol>()
            .FirstOrDefault(candidate =>
                !candidate.IsStatic &&
                candidate.Locations.Any(static location => location.IsInSource) &&
                predicate(candidate));
        if (method is not null)
            return method;
    }

    return null;
}
```

**查找策略**：
- 沿继承链向上查找
- 只查找源码中的方法（`IsInSource`）
- 只查找实例方法（非静态）
- 应用自定义谓词过滤

#### 16. `DerivesFrom`

类型继承关系检测：

```csharp
private static bool DerivesFrom(ITypeSymbol? symbol, INamedTypeSymbol baseType)
{
    for (var current = symbol as INamedTypeSymbol; current is not null; current = current.BaseType)
    {
        if (Comparer.Equals(current.OriginalDefinition, baseType))
            return true;
    }

    return false;
}
```

**关键点**：使用 `OriginalDefinition` 进行比较，避免泛型类型实例化导致的匹配失败。

## 设计权衡

### 1. 签名去重 vs 名称去重

- **方法**：使用完整签名去重，处理方法重载
- **字段**：使用名称去重，因为字段不支持重载

### 2. 保守的字段发现

当前字段发现策略保守，排除所有可能引起问题的字段（如自动属性 backing field），等待后续 lowering 阶段完善。

### 3. 源码位置检测

所有查找器都要求 `IsInSource == true`，排除从元数据或引用程序集继承的成员，确保只处理用户编写的代码。

## 使用示例

### 分类入口类型

```csharp
var symbols = RazorVueCompilationSymbols.TryCreate(compilation);
var entryKind = RazorVueEntryClassifier.Classify(myComponentSymbol, symbols);

switch (entryKind)
{
    case RazorVueEntryKind.RazorVueComponent:
        // 处理 RazorVue 组件
        break;
    case RazorVueEntryKind.StaticModule:
        // 处理静态模块
        break;
    case RazorVueEntryKind.Invalid:
        // 报告无效配置
        break;
}
```

### 发现生命周期方法

```csharp
var onInitialized = RazorVueEntryClassifier.FindOnInitializedMethod(componentSymbol);
var onAfterRender = RazorVueEntryClassifier.FindOnAfterRenderMethod(componentSymbol);
```

### 提取逻辑成员

```csharp
var logicMethods = RazorVueEntryClassifier.FindLogicMethods(componentSymbol);
var logicFields = RazorVueEntryClassifier.FindLogicFields(componentSymbol);

foreach (var method in logicMethods)
{
    Console.WriteLine($"Logic method: {method.Name}");
}
```

## 相关文件

- `src/Jazor.RazorVue/RazorVueCompilationSymbols.cs` - 编译符号提供者
- `src/Jazor.RazorVue/RazorVueCompilationContext.cs` - 编译上下文（使用分类器）
- `src/Jazor.RazorVue/RazorVueComponentCandidate.cs` - 组件候选（持有发现结果）
- `src/Jazor.RazorVue/RazorVueEntryKind.cs` - 入口类型枚举
