# RazorVueCompilationSymbols - 编译符号表

**文件路径**: `src/Jazor.RazorVue/RazorVueCompilationSymbols.cs`

## 为什么需要

RazorVue 编译时分析需要访问大量类型符号（如 `ComponentBase`、`IUIComponent`、`IVueComponent`、`ParameterAttribute` 等）。这些符号分布在不同的命名空间和程序集中，需要统一的符号表提供以下功能：

1. **集中管理**：一次性解析所有需要的类型符号
2. **带回退解析**：支持过渡期的元数据名称变更（如 `Jazor.Razor.JazorComponent` → `Jazor.Compiler.Razor.JazorComponent`）
3. **可选性处理**：区分必需符号和可选符号（如库组件相关特性）
4. **类型安全**：使用 `record` 提供编译时类型检查

## 实现思路

### 符号结构

`RazorVueCompilationSymbols` 是一个内部 record，集中保存当前编译所需的类型符号：

#### 必需符号（不可为 null）

| 符号名 | 类型 | 用途 | 元数据名称 |
|--------|------|------|-----------|
| `ECMAScriptModuleAttribute` | `INamedTypeSymbol` | 检测 ECMAScript 模块入口 | `ECMAScript.ECMAScriptModuleAttribute` |
| `JazorComponentMarker` | `INamedTypeSymbol` | 检测 RazorVue 组件 authoring marker | `ECMAScript.Contract.IUIComponent` |
| `VueComponentMarker` | `INamedTypeSymbol` | 检测 Vue 组件 authoring marker | `ECMAScript.Vue3+IVueComponent` |
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
| `VueLibraryComponent` | `INamedTypeSymbol?` | 库组件 authoring 基类/marker | `Jazor.RazorVue.VueLibraryComponent` |
| `IVueLibraryComponent` | `INamedTypeSymbol?` | 库组件接口 marker | `ECMAScript.Vue3+IVueLibraryComponent` |
| `VueLibraryComponentAttribute` | `INamedTypeSymbol?` | 库组件特性 | `ECMAScript.VueContract.VueLibraryComponentAttribute` |
| `VueLibraryStyleAttribute` | `INamedTypeSymbol?` | 库组件样式特性 | `ECMAScript.VueContract.VueLibraryStyleAttribute` |
| `VueLibraryPluginRequirementAttribute` | `INamedTypeSymbol?` | 库组件插件依赖特性 | `ECMAScript.VueContract.VueLibraryPluginRequirementAttribute` |
| `VuePropAttribute` | `INamedTypeSymbol?` | 通用 prop 元数据特性 | `ECMAScript.VueContract.VuePropAttribute` |
| `VueLibraryEmitAttribute` | `INamedTypeSymbol?` | 库组件事件特性 | `ECMAScript.VueContract.VueLibraryEmitAttribute` |
| `VueSlotAttribute` | `INamedTypeSymbol?` | 通用 slot 元数据特性 | `ECMAScript.VueContract.VueSlotAttribute` |
| `VueLibraryComponentFlagsAttribute` | `INamedTypeSymbol?` | 库组件标志特性 | `ECMAScript.VueContract.VueLibraryComponentFlagsAttribute` |
| `IVueContainerComponent` | `INamedTypeSymbol?` | 容器组件契约接口 | `ECMAScript.VueContract.IVueContainerComponent` |
| `IVueContainerImplementation` | `INamedTypeSymbol?` | 容器实现组件接口 | `ECMAScript.VueContract.IVueContainerImplementation\`1` |
| `VueInjectAttribute` | `INamedTypeSymbol?` | 装配级容器注入声明 | `ECMAScript.VueContract.VueInjectAttribute` |

**可选性处理**：这些符号解析失败不会终止编译流程，但会禁用相关功能（如库组件支持）。

### 工厂方法

#### `TryCreate(Compilation)`

```csharp
public static RazorVueCompilationSymbols? TryCreate(Compilation compilation)
{
    // 1. 解析必需符号
    var ecmaScriptModuleAttribute = compilation.GetTypeByMetadataName("ECMAScript.ECMAScriptModuleAttribute");
    var jazorComponent = compilation.GetTypeByMetadataName("ECMAScript.Contract.IUIComponent");
    var vueComponent = compilation.GetTypeByMetadataName("ECMAScript.Vue3+IVueComponent");
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
    var iVueLibraryComponent = compilation.GetTypeByMetadataName("ECMAScript.Vue3+IVueLibraryComponent");
    var vueLibraryComponentAttribute = compilation.GetTypeByMetadataName("ECMAScript.VueContract.VueLibraryComponentAttribute");
    var vueLibraryStyleAttribute = compilation.GetTypeByMetadataName("ECMAScript.VueContract.VueLibraryStyleAttribute");
    var vueLibraryPluginRequirementAttribute = compilation.GetTypeByMetadataName("ECMAScript.VueContract.VueLibraryPluginRequirementAttribute");
    var vuePropAttribute = compilation.GetTypeByMetadataName("ECMAScript.VueContract.VuePropAttribute");
    var vueLibraryEmitAttribute = compilation.GetTypeByMetadataName("ECMAScript.VueContract.VueLibraryEmitAttribute");
    var vueSlotAttribute = compilation.GetTypeByMetadataName("ECMAScript.VueContract.VueSlotAttribute");
    var vueLibraryComponentFlagsAttribute = compilation.GetTypeByMetadataName("ECMAScript.VueContract.VueLibraryComponentFlagsAttribute");
    var iVueContainerComponent = compilation.GetTypeByMetadataName("ECMAScript.VueContract.IVueContainerComponent");
    var iVueContainerImplementation = compilation.GetTypeByMetadataName("ECMAScript.VueContract.IVueContainerImplementation`1");
    var vueInjectAttribute = compilation.GetTypeByMetadataName("ECMAScript.VueContract.VueInjectAttribute");

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
        iVueLibraryComponent,
        vueLibraryComponentAttribute,
        vueLibraryStyleAttribute,
        vueLibraryPluginRequirementAttribute,
        vuePropAttribute,
        vueLibraryEmitAttribute,
        vueSlotAttribute,
        vueLibraryComponentFlagsAttribute,
        iVueContainerComponent,
        iVueContainerImplementation,
        vueInjectAttribute);
}
```

**关键特性**：
1. **必需符号验证**：4 个必需符号全部解析成功后才继续
2. **可选符号容错**：可选符号解析失败不影响符号表创建
3. **组件边界去基类化**：当前 authoring 面通过 `IUIComponent` + `IVueComponent` marker 判定组件，不再依赖历史 `JazorComponent` / `VueComponent` 基类

## 符号用途映射

### 组件入口检测

| 符号 | 用途 | 使用位置 |
|------|------|----------|
| `ECMAScriptModuleAttribute` | 检测 `[ECMAScriptModule]` 特性 | `EntryClassifier.HasECMAScriptModuleAttribute` |
| `JazorComponentMarker` | 检测 RazorVue 组件 authoring marker | `EntryClassifier.Classify` |
| `ComponentBase` | 检测 ASP.NET Components 基类 | `EntryClassifier.Classify` |
| `VueComponentMarker` | 检测 Vue 组件 authoring marker | `EntryClassifier.Classify` |

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
| `VueLibraryComponent` | 检测库组件 authoring 基类 | `EntryClassifier.IsLibraryComponent` |
| `IVueLibraryComponent` | 检测库组件接口 marker | `EntryClassifier.IsLibraryComponent` |
| `VueLibraryComponentAttribute` | 检测库组件特性 | `VueComponentDescriptorFactory.CreateLibraryComponent` |
| `VueLibraryStyleAttribute` | 检测样式依赖特性 | `VueComponentDescriptorFactory.ExtractLibraryStyleRequirements` |
| `VueLibraryPluginRequirementAttribute` | 检测插件依赖特性 | `VueComponentDescriptorFactory.ExtractLibraryPluginRequirements` |
| `VuePropAttribute` | 检测库组件属性特性 | `VueComponentDescriptorFactory.ExtractLibraryComponentProps` |
| `VueLibraryEmitAttribute` | 检测库组件事件特性 | `VueComponentDescriptorFactory.ExtractLibraryComponentEmits` |
| `VueSlotAttribute` | 检测库组件插槽特性 | `VueComponentDescriptorFactory.ExtractLibraryComponentSlots` |
| `VueLibraryComponentFlagsAttribute` | 检测库组件标志特性 | `VueComponentDescriptorFactory.ExtractLibraryComponentFlags` |
| `IVueContainerComponent` | 检测 authored 容器契约组件 | `VueComponentDescriptorFactory.GetContainerContractFullName` |
| `IVueContainerImplementation` | 检测具体容器实现组件 | `VueComponentDescriptorFactory.GetContainerContractFullName` |
| `VueInjectAttribute` | 读取装配级容器注入映射 | `VueInjectRegistry.Resolve` |

### 容器注入机制

容器组件机制是 RazorVue 的一条独立抽象维度，不属于 `UserComponent / Intrinsic / LibraryComponent` 之外的新 source kind。

- `IVueContainerComponent`：标记 authored 组件是“容器契约”
- `IVueContainerImplementation<TContainer>`：标记某个具体组件实现了该容器契约
- `[assembly: VueInject(typeof(TContainer), typeof(TImplementation))]`：声明当前编译装配选择哪个实现参与最终编译

这组符号的职责边界如下：

1. `RazorVueCompilationSymbols` 负责发现契约接口和装配级注入声明。
2. `VueComponentDescriptorFactory` 负责把容器契约信息投影到 `VueComponentDescriptor.ContainerContractFullName`。
3. `VueInjectRegistry` 负责读取 `[VueInject]` 并校验：
   - 同一 contract 不能重复注入多个实现
   - implementation 必须在当前组件注册表中可见
   - implementation 必须声明匹配的 `IVueContainerImplementation<TContainer>`
4. `RazorVueArtifactFactory.ComponentResolver` 在组件解析完成后执行容器实现替换。

这个设计刻意避免引入 `ECMAScript.Vben.TDesign` 这类按库名耦合的框架层。Vben 之类上层只消费容器契约与注入机制，不定义新的解析路径。

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

### 2. 组件边界 marker 策略

**优点**：
- 组件 authoring 边界直接落在公共 contract 上
- 被引用程序集中的原生 `IVueComponent` 组件可直接参与发现
- 不再要求 consumer 继承历史兼容基类

**代价**：
- 文档和旧测试输入需要同步迁移到 marker 模型
- 设计期工具不能再假设存在单一运行时基类

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
