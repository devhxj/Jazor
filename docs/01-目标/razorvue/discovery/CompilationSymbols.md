# RazorVueCompilationSymbols - 编译符号表

**文件路径**: `src/Jazor.RazorVue/RazorVueCompilationSymbols.cs`

## 为什么需要

RazorVue 编译时分析需要访问大量类型符号（如 `ComponentBase`、`JazorComponent`、`ParameterAttribute` 等）。这些符号分布在不同的命名空间和程序集中，需要统一的符号表提供以下功能：

1. **集中管理**：一次性解析所有需要的类型符号
2. **带回退解析**：支持过渡期的元数据名称变更（如 `Jazor.Razor.JazorComponent` → `Jazor.Compiler.Razor.JazorComponent`）
3. **可选性处理**：区分必需符号和可选符号（如库组件相关特性）
4. **类型安全**：使用 `record` 提供编译时类型检查

## 实现思路

### 符号结构

`RazorVueCompilationSymbols` 是一个 `public record`，包含 18 个类型符号：

#### 必需符号（不可为 null）

| 符号名 | 类型 | 用途 | 元数据名称 |
|--------|------|------|-----------|
| `ECMAScriptModuleAttribute` | `INamedTypeSymbol` | 检测 ECMAScript 模块入口 | `ECMAScript.ECMAScriptModuleAttribute` |
| `JazorComponent` | `INamedTypeSymbol` | 检测 RazorVue 组件基类 | `Jazor.Razor.JazorComponent`（回退到 `Jazor.Compiler.Razor.JazorComponent`） |
| `VueComponent` | `INamedTypeSymbol` | Vue 组件描述符基类 | `Jazor.RazorVue.VueComponent`（回退到 `Jazor.Compiler.RazorVue.VueComponent`） |
| `ComponentBase` | `INamedTypeSymbol` | ASP.NET Components 基类 | `Microsoft.AspNetCore.Components.ComponentBase` |

**必需性验证**：如果这 4 个符号任何一个解析失败，`TryCreate` 返回 `null`，整个 RazorVue 编译流程终止。

#### 可选符号（可为 null）

| 符号名 | 类型 | 用途 | 元数据名称 |
|--------|------|------|-----------|
| `ParameterAttribute` | `INamedTypeSymbol?` | 检测组件参数特性 | `Microsoft.AspNetCore.Components.ParameterAttribute` |
| `ParameterView` | `INamedTypeSymbol?` | 参数视图类型 | `Microsoft.AspNetCore.Components.ParameterView` |
| `EventCallback` | `INamedTypeSymbol?` | 事件回调类型 | `Microsoft.AspNetCore.Components.EventCallback` |
| `EventCallbackOfT` | `INamedTypeSymbol?` | 泛型事件回调类型 | `Microsoft.AspNetCore.Components.EventCallback`1` |
| `RenderFragment` | `INamedTypeSymbol?` | 渲染片段委托类型 | `Microsoft.AspNetCore.Components.RenderFragment` |
| `RenderFragmentOfT` | `INamedTypeSymbol?` | 泛型渲染片段委托类型 | `Microsoft.AspNetCore.Components.RenderFragment`1` |
| `VueLibraryComponent` | `INamedTypeSymbol?` | 库组件接口 | `Jazor.RazorVue.VueLibraryComponent` |
| `VueLibraryComponentAttribute` | `INamedTypeSymbol?` | 库组件特性 | `Jazor.RazorVue.VueLibraryComponentAttribute` |
| `VueLibraryStyleAttribute` | `INamedTypeSymbol?` | 库组件样式特性 | `Jazor.RazorVue.VueLibraryStyleAttribute` |
| `VueLibraryPluginRequirementAttribute` | `INamedTypeSymbol?` | 库组件插件依赖特性 | `Jazor.RazorVue.VueLibraryPluginRequirementAttribute` |
| `VueLibraryPropAttribute` | `INamedTypeSymbol?` | 库组件属性特性 | `Jazor.RazorVue.VueLibraryPropAttribute` |
| `VueLibraryEmitAttribute` | `INamedTypeSymbol?` | 库组件事件特性 | `Jazor.RazorVue.VueLibraryEmitAttribute` |
| `VueLibrarySlotAttribute` | `INamedTypeSymbol?` | 库组件插槽特性 | `Jazor.RazorVue.VueLibrarySlotAttribute` |
| `VueLibraryComponentFlagsAttribute` | `INamedTypeSymbol?` | 库组件标志特性 | `Jazor.RazorVue.VueLibraryComponentFlagsAttribute` |

**可选性处理**：这些符号解析失败不会终止编译流程，但会禁用相关功能（如库组件支持）。

### 工厂方法

#### `TryCreate(Compilation)`

```csharp
public static RazorVueCompilationSymbols? TryCreate(Compilation compilation)
{
    // 1. 解析必需符号
    var ecmaScriptModuleAttribute = compilation.GetTypeByMetadataName("ECMAScript.ECMAScriptModuleAttribute");
    var jazorComponent = GetTypeByMetadataName(
        compilation,
        "Jazor.Razor.JazorComponent",
        "Jazor.Compiler.Razor.JazorComponent");
    var vueComponent = GetTypeByMetadataName(
        compilation,
        "Jazor.RazorVue.VueComponent",
        "Jazor.Compiler.RazorVue.VueComponent");
    var componentBase = compilation.GetTypeByMetadataName("Microsoft.AspNetCore.Components.ComponentBase");

    // 2. 验证必需符号
    if (ecmaScriptModuleAttribute is null ||
        jazorComponent is null ||
        vueComponent is null ||
        componentBase is null)
    {
        return null;
    }

    // 3. 解析可选符号
    var parameterAttribute = compilation.GetTypeByMetadataName("Microsoft.AspNetCore.Components.ParameterAttribute");
    var parameterView = compilation.GetTypeByMetadataName("Microsoft.AspNetCore.Components.ParameterView");
    var eventCallback = compilation.GetTypeByMetadataName("Microsoft.AspNetCore.Components.EventCallback");
    var eventCallbackOfT = compilation.GetTypeByMetadataName("Microsoft.AspNetCore.Components.EventCallback`1");
    var renderFragment = compilation.GetTypeByMetadataName("Microsoft.AspNetCore.Components.RenderFragment");
    var renderFragmentOfT = compilation.GetTypeByMetadataName("Microsoft.AspNetCore.Components.RenderFragment`1");
    var vueLibraryComponent = compilation.GetTypeByMetadataName("Jazor.RazorVue.VueLibraryComponent");
    var vueLibraryComponentAttribute = compilation.GetTypeByMetadataName("Jazor.RazorVue.VueLibraryComponentAttribute");
    var vueLibraryStyleAttribute = compilation.GetTypeByMetadataName("Jazor.RazorVue.VueLibraryStyleAttribute");
    var vueLibraryPluginRequirementAttribute = compilation.GetTypeByMetadataName("Jazor.RazorVue.VueLibraryPluginRequirementAttribute");
    var vueLibraryPropAttribute = compilation.GetTypeByMetadataName("Jazor.RazorVue.VueLibraryPropAttribute");
    var vueLibraryEmitAttribute = compilation.GetTypeByMetadataName("Jazor.RazorVue.VueLibraryEmitAttribute");
    var vueLibrarySlotAttribute = compilation.GetTypeByMetadataName("Jazor.RazorVue.VueLibrarySlotAttribute");
    var vueLibraryComponentFlagsAttribute = compilation.GetTypeByMetadataName("Jazor.RazorVue.VueLibraryComponentFlagsAttribute");

    // 4. 构建符号表
    return new RazorVueCompilationSymbols(
        ecmaScriptModuleAttribute,
        jazorComponent,
        vueComponent,
        componentBase,
        parameterAttribute,
        parameterView,
        eventCallback,
        eventCallbackOfT,
        renderFragment,
        renderFragmentOfT,
        vueLibraryComponent,
        vueLibraryComponentAttribute,
        vueLibraryStyleAttribute,
        vueLibraryPluginRequirementAttribute,
        vueLibraryPropAttribute,
        vueLibraryEmitAttribute,
        vueLibrarySlotAttribute,
        vueLibraryComponentFlagsAttribute);
}
```

**关键特性**：
1. **必需符号验证**：4 个必需符号全部解析成功后才继续
2. **可选符号容错**：可选符号解析失败不影响符号表创建
3. **回退元数据名**：支持过渡期的多个候选元数据名称

### 回退元数据名解析

#### `GetTypeByMetadataName(Compilation, params string[])`

```csharp
private static INamedTypeSymbol? GetTypeByMetadataName(Compilation compilation, params string[] metadataNames)
{
    foreach (var metadataName in metadataNames)
    {
        var symbol = compilation.GetTypeByMetadataName(metadataName);
        if (symbol is not null)
            return symbol;
    }

    return null;
}
```

**回退策略**：

| 符号 | 主名称 | 回退名称 | 原因 |
|------|--------|----------|------|
| `JazorComponent` | `Jazor.Razor.JazorComponent` | `Jazor.Compiler.Razor.JazorComponent` | 命名空间重构 |
| `VueComponent` | `Jazor.RazorVue.VueComponent` | `Jazor.Compiler.RazorVue.VueComponent` | 命名空间重构 |

**设计原因**：
- 支持测试代码和中间分支使用旧命名空间
- 向后兼容：旧的测试输入仍能加载
- 渐进式迁移：新旧命名空间共存期间的平滑过渡

**注释说明**：
```csharp
// Prefer the final public runtime libraries but keep transitional
// fallbacks so older test inputs and intermediate branches still load.
```

## 符号用途映射

### 组件入口检测

| 符号 | 用途 | 使用位置 |
|------|------|----------|
| `ECMAScriptModuleAttribute` | 检测 `[ECMAScriptModule]` 特性 | `EntryClassifier.HasECMAScriptModuleAttribute` |
| `JazorComponent` | 检测 RazorVue 组件继承 | `EntryClassifier.Classify` |
| `ComponentBase` | 检测 ASP.NET Components 基类 | `EntryClassifier.Classify` |
| `VueComponent` | Vue 组件描述符基类 | `VueComponentDescriptorFactory` |

### 参数系统

| 符号 | 用途 | 使用位置 |
|------|------|----------|
| `ParameterAttribute` | 检测 `[Parameter]` 特性 | `VueComponentDescriptorFactory.ExtractComponentParameters` |
| `ParameterView` | `SetParametersAsync` 参数类型 | `EntryClassifier.FindSetParametersAsyncMethod` |
| `EventCallback` | 事件回调类型检测 | `VueComponentDescriptorFactory.ExtractComponentParameters` |
| `EventCallbackOfT` | 泛型事件回调类型检测 | `VueComponentDescriptorFactory.ExtractComponentParameters` |

### 渲染系统

| 符号 | 用途 | 使用位置 |
|------|------|----------|
| `RenderFragment` | `RenderTreeBuilder` 委托类型 | `VueComponentDescriptorFactory` |
| `RenderFragmentOfT` | 泛型渲染片段委托类型 | `VueComponentDescriptorFactory` |

### 库组件系统

| 符号 | 用途 | 使用位置 |
|------|------|----------|
| `VueLibraryComponent` | 检测库组件接口 | `EntryClassifier.IsLibraryComponent` |
| `VueLibraryComponentAttribute` | 检测库组件特性 | `VueComponentDescriptorFactory.CreateLibraryComponent` |
| `VueLibraryStyleAttribute` | 检测样式依赖特性 | `VueComponentDescriptorFactory.ExtractLibraryStyleRequirements` |
| `VueLibraryPluginRequirementAttribute` | 检测插件依赖特性 | `VueComponentDescriptorFactory.ExtractLibraryPluginRequirements` |
| `VueLibraryPropAttribute` | 检测库组件属性特性 | `VueComponentDescriptorFactory.ExtractLibraryComponentProps` |
| `VueLibraryEmitAttribute` | 检测库组件事件特性 | `VueComponentDescriptorFactory.ExtractLibraryComponentEmits` |
| `VueLibrarySlotAttribute` | 检测库组件插槽特性 | `VueComponentDescriptorFactory.ExtractLibraryComponentSlots` |
| `VueLibraryComponentFlagsAttribute` | 检测库组件标志特性 | `VueComponentDescriptorFactory.ExtractLibraryComponentFlags` |

## 设计权衡

### 1. 必需 vs 可选符号

**必需符号设计理由**：
- `ECMAScriptModuleAttribute`：无此特性则不是 RazorVue 入口
- `JazorComponent`：无此基类则不是 RazorVue 组件
- `VueComponent`：无此基类则无法生成 Vue 组件描述符
- `ComponentBase`：无此基类则不是 ASP.NET Components 组件

**可选符号设计理由**：
- 参数系统符号：可能在不使用参数的组件中不需要
- 库组件符号：仅在使用库组件功能时需要
- 渲染片段符号：可能在仅 JavaScript 转译场景中不需要

### 2. 回退元数据名策略

**优点**：
- 支持渐进式重构
- 旧测试用例无需修改
- 多分支协同开发

**缺点**：
- 增加符号解析复杂度
- 可能掩盖命名不一致问题

**缓解措施**：
- 注释明确说明回退原因
- 优先使用最终公共命名空间
- 测试覆盖新旧两种命名空间

### 3. record 类型选择

**优点**：
- `with` 表达式支持（虽然当前未使用）
- 基于值的相等性（虽然当前未使用）
- 简洁的构造语法

**潜在改进**：
- 考虑使用 `class` + 只读属性，避免不需要的 `Equals`/`GetHashCode` 语义
- 使用 `init` 属性提供更灵活的构造

## 使用示例

### 创建符号表

```csharp
var compilation = ...; // 获取 Roslyn Compilation
var symbols = RazorVueCompilationSymbols.TryCreate(compilation);

if (symbols is null)
{
    // 必需符号缺失，无法进行 RazorVue 编译
    Console.WriteLine("Required RazorVue symbols not found");
    return;
}
```

### 使用符号表进行分类

```csharp
var symbols = RazorVueCompilationSymbols.TryCreate(compilation);
var entryKind = RazorVueEntryClassifier.Classify(myComponentSymbol, symbols);

if (entryKind == RazorVueEntryKind.RazorVueComponent)
{
    // 处理 RazorVue 组件
}
```

### 检查可选符号可用性

```csharp
var symbols = RazorVueCompilationSymbols.TryCreate(compilation);

if (symbols.VueLibraryComponent is not null)
{
    // 库组件功能可用
    var libraryComponents = context.DiscoverLibraryComponents();
}
else
{
    // 库组件功能不可用，使用降级策略
    Console.WriteLine("Vue library component support is not available");
}
```

## 相关文件

- `src/Jazor.RazorVue/Discovery/RazorVueEntryClassifier.cs` - 使用符号表进行分类
- `src/Jazor.RazorVue/RazorVueCompilationContext.cs` - 持有符号表实例
- `src/Jazor.RazorVue/Descriptor/VueComponentDescriptorFactory.cs` - 使用符号表提取组件描述符
