# Vue 组件描述符工厂（Descriptor Factory）

## 为什么需要

组件描述符工厂负责从 C# 类型符号生成 Vue 组件描述符。它是 RazorVue 编译时分析的核心引擎，自动扫描组件参数、推断类型、分类特征，并生成完整的组件元数据。

工厂模式解决了以下关键问题：

1. **自动参数发现**：遍历继承链，收集所有 `[Parameter]` 属性
2. **智能类型分类**：将参数自动分类为 prop/emit/slot
3. **双向绑定推断**：从 `Foo` + `FooChanged` 模式推断 v-model 支持
4. **库组件元数据**：支持通过特性覆盖默认行为
5. **命名约定转换**：自动处理 PascalCase 到 camelCase 转换
6. **容器契约投影**：支持容器契约组件与具体实现组件的编译期关联

## 实现思路

### 核心方法

工厂类位于 `src/Jazor.RazorVue/Descriptor/VueComponentDescriptorFactory.cs`，提供两个静态入口点：

#### 1. 用户组件创建

```csharp
public static VueComponentDescriptor Create(
    RazorVueComponentCandidate candidate,
    RazorVueCompilationContext context)
{
    return CreateDescriptor(
        candidate.ComponentSymbol,
        context.Symbols,
        VueComponentSourceKind.UserComponent,
        GetUserImportSpecifier(candidate.ComponentSymbol, context.Symbols),
        "default",
        GetContainerContractFullName(candidate.ComponentSymbol, context.Symbols),
        [],
        []);
}
```

**特点**：
- 从 `RazorVueComponentCandidate` 提取组件符号
- 自动生成导入路径（基于程序集和命名空间）
- 导出名称固定为 `"default"`
- 无样式依赖和插件需求

#### 2. 库组件创建

```csharp
public static VueComponentDescriptor CreateLibraryComponent(
    INamedTypeSymbol componentSymbol,
    RazorVueCompilationContext context)
{
    if (componentSymbol is null)
        throw new ArgumentNullException(nameof(componentSymbol));
    if (context is null)
        throw new ArgumentNullException(nameof(context));

    var symbols = context.Symbols;
    var metadata = GetLibraryMetadata(componentSymbol, symbols);
    return CreateDescriptor(
        componentSymbol,
        symbols,
        VueComponentSourceKind.LibraryComponent,
        metadata.ImportSpecifier,
        metadata.ExportName,
        GetContainerContractFullName(componentSymbol, symbols),
        metadata.StyleDependencies,
        metadata.PluginRequirements);
}
```

**特点**：
- 必须声明 `[VueLibraryComponent]` 特性
- 支持元数据覆盖（prop/emit/slot 名称、类型、标志）
- 支持样式依赖和插件需求声明
- 更严格的验证规则

### 参数发现算法

#### 遍历继承链

```csharp
private static ImmutableArray<IPropertySymbol> GetParameterProperties(
    INamedTypeSymbol componentSymbol,
    RazorVueCompilationSymbols symbols)
{
    if (symbols.ParameterAttribute is null)
        return [];

    var builder = ImmutableArray.CreateBuilder<IPropertySymbol>();
    var seenNames = new HashSet<string>(StringComparer.Ordinal);

    // 从当前类型向基类遍历
    for (var current = componentSymbol; current is not null; current = current.BaseType)
    {
        foreach (var member in current.GetMembers())
        {
            // 只处理实例属性，跳过静态和重复名称
            if (member is not IPropertySymbol property ||
                property.IsStatic ||
                !seenNames.Add(property.Name))
                continue;

            // 检查是否标记 [Parameter]
            if (property.GetAttributes()
                .Any(attribute => Comparer.Equals(
                    attribute.AttributeClass,
                    symbols.ParameterAttribute)))
            {
                builder.Add(property);
            }
        }
    }

    return builder.ToImmutable();
}
```

**特点**：
- 去重处理：派生类覆盖基类同名属性
- 顺序保证：按继承顺序（派生类在前）
- 性能优化：使用 `HashSet` 快速去重

### 类型分类逻辑

#### 1. RenderFragment → Slot

```csharp
if (IsRenderFragment(property.Type, symbols))
{
    if (propOverride is not null)
    {
        throw CreateInvalidLibraryComponentDeclarationException(
            componentSymbol,
            $"Library component '{FormatFullName(componentSymbol)}' can only apply [VueProp] to regular [Parameter] properties. '{property.Name}' is a slot parameter.");
    }

    slots.Add(CreateSlotDescriptor(property, symbols, slotOverride));
    continue;
}
```

**检测方法**：

```csharp
private static bool IsRenderFragment(
    ITypeSymbol typeSymbol,
    RazorVueCompilationSymbols symbols)
    => typeSymbol is INamedTypeSymbol namedType &&
       ((symbols.RenderFragment is not null &&
         Comparer.Equals(namedType.OriginalDefinition, symbols.RenderFragment)) ||
        (symbols.RenderFragmentOfT is not null &&
         Comparer.Equals(namedType.OriginalDefinition, symbols.RenderFragmentOfT)));
```

#### 2. EventCallback → Emit

```csharp
if (IsEventCallback(property.Type, symbols))
{
    if (propOverride is not null)
    {
        throw CreateInvalidLibraryComponentDeclarationException(
            componentSymbol,
            $"Library component '{FormatFullName(componentSymbol)}' can only apply [VueProp] to regular [Parameter] properties. '{property.Name}' is an event callback parameter.");
    }

    continue;  // 稍后在 emit 循环中处理
}
```

**检测方法**：

```csharp
private static bool IsEventCallback(
    ITypeSymbol typeSymbol,
    RazorVueCompilationSymbols symbols)
    => typeSymbol is INamedTypeSymbol namedType &&
       ((symbols.EventCallback is not null &&
         Comparer.Equals(namedType.OriginalDefinition, symbols.EventCallback)) ||
        (symbols.EventCallbackOfT is not null &&
         Comparer.Equals(namedType.OriginalDefinition, symbols.EventCallbackOfT)));
```

#### 3. 其他 → Prop

```csharp
var publicName = property.Name;
var inferredAcceptsBinding = bindPairs.Contains(publicName);
var acceptsBinding = propOverride?.AcceptsBinding ?? inferredAcceptsBinding;
var kind = propOverride is not null && propOverride.HasKindOverride
    ? propOverride.Kind
    : acceptsBinding
        ? VuePropKind.Model
        : VuePropKind.Normal;

props.Add(new VuePropDescriptor(
    Name: propOverride?.Name ?? ToLowerCamelCase(publicName),
    PublicName: publicName,
    TypeName: FormatTypeName(property.Type),
    Required: propOverride?.Required ?? false,
    AcceptsBinding: acceptsBinding,
    DefaultExpression: propOverride?.DefaultExpression,
    Kind: kind));
```

### 双向绑定推断

#### Foo + FooChanged 模式检测

```csharp
private static HashSet<string> GetBindableParameterNames(
    ImmutableArray<IPropertySymbol> parameterProperties,
    RazorVueCompilationSymbols symbols)
{
    var parameterNames = new HashSet<string>(
        parameterProperties.Select(static property => property.Name),
        StringComparer.Ordinal);

    var builder = new HashSet<string>(StringComparer.Ordinal);

    foreach (var property in parameterProperties)
    {
        // 检查是否为 EventCallback 且名称以 "Changed" 结尾
        if (!IsEventCallback(property.Type, symbols) ||
            !property.Name.EndsWith("Changed", StringComparison.Ordinal))
        {
            continue;
        }

        // 提取基础名称（ValueChanged → Value）
        var parameterName = property.Name.Substring(
            0,
            property.Name.Length - "Changed".Length);

        // 检查是否存在对应的参数
        if (parameterNames.Contains(parameterName))
            builder.Add(parameterName);
    }

    return builder;
}
```

**推断示例**：

```csharp
// C# 代码
[Parameter] public string Value { get; set; }
[Parameter] public EventCallback<string> ValueChanged { get; set; }

// 推断结果
bindPairs.Contains("Value")  // true
AcceptsBinding: true         // Value 被标记为可绑定
Kind: VuePropKind.Model      // Value 被分类为 Model
```

### 容器契约投影

描述符工厂现在还负责把容器抽象投影到 `VueComponentDescriptor.ContainerContractFullName`，但不会引入新的 `SourceKind`。

原因是容器不是“第四类组件”，而是 user component / library component 之上的一层抽象语义：

- authored 容器组件本身仍然是 `UserComponent`
- 来自 Element Plus / Vuetify / TDesign 的实现组件仍然是 `LibraryComponent`
- 是否发生替换，由后续装配级 `[VueInject]` 决定

工厂只做契约归属识别：

```csharp
private static string? GetContainerContractFullName(
    INamedTypeSymbol componentSymbol,
    RazorVueCompilationSymbols symbols)
{
    if (symbols.IVueContainerComponent is not null &&
        componentSymbol.AllInterfaces.Any(candidate => Comparer.Equals(candidate.OriginalDefinition, symbols.IVueContainerComponent)))
    {
        return FormatFullName(componentSymbol);
    }

    if (symbols.IVueContainerImplementation is null)
        return null;

    foreach (var interfaceSymbol in componentSymbol.AllInterfaces)
    {
        if (!Comparer.Equals(interfaceSymbol.OriginalDefinition, symbols.IVueContainerImplementation))
            continue;

        if (interfaceSymbol.TypeArguments.Length != 1 || interfaceSymbol.TypeArguments[0] is not INamedTypeSymbol contractType)
            continue;

        return FormatFullName(contractType);
    }

    return null;
}
```

语义规则：

1. `IVueContainerComponent`
   - 当前组件就是容器契约
   - `ContainerContractFullName = 当前组件 FullName`

2. `IVueContainerImplementation<TContainer>`
   - 当前组件是某个容器契约的实现
   - `ContainerContractFullName = TContainer.FullName`

3. 其他组件
   - `ContainerContractFullName = null`

这样后续的 `VueInjectRegistry` 就能在组件解析完成后，把 authored 容器组件替换成装配配置的具体实现，而不需要在描述符阶段硬编码任何具体组件库。

### 容器注入后的 merged descriptor 规则

`VueComponentDescriptorFactory` 只负责把容器归属投影到 `ContainerContractFullName`，  
真正的注入发生在 `VueInjectRegistry.ResolveImplementation(...)`。

这里有一个容易误用的点：

- **注入不是直接返回 implementation descriptor**
- **注入返回的是 contract + implementation 合成后的 merged descriptor**

当前合成规则：

| 字段 | 来源 |
|------|------|
| `Name` | contract |
| `FullName` | contract |
| `ResolutionNamespace` | contract |
| `RouteTemplates` | contract |
| `Flags` | contract |
| `SourceKind` | implementation |
| `ImportSpecifier` | implementation |
| `ExportName` | implementation |
| `StyleDependencies` | implementation |
| `PluginRequirements` | implementation |
| `Props` | 按 `PublicName` 配对，contract 保留 authoring 语义，implementation 提供 runtime `Name` |
| `Emits` | 按 `RazorAlias` 配对，contract 保留 authoring 语义，implementation 提供 runtime `Name` |
| `Slots` | 按 `PublicName` 配对，contract 保留 authoring 语义，implementation 提供 runtime `Name` |

这样设计的原因很直接：

1. authoring contract 必须稳定，不能因为底层库切换而改变上层 authoring 面。
2. runtime import / export / style / plugin 依赖必须来自真实 implementation。
3. prop / emit / slot 描述符同时承载 authoring 名和 runtime 名，所以不能整组沿用 contract 或 implementation，必须成员级别合成。

### 容器注入兼容性验证

`VueInjectRegistry` 在返回 merged descriptor 之前会做 contract/implementation 一致性校验。

当前校验方式是“按 authoring 键配对，再检查 implementation 是否能替代 contract”：

- `Props`：按 `PublicName`
- `Emits`：按 `RazorAlias`
- `Slots`：按 `PublicName`

校验目标：

- implementation 不能缺少 contract 已公开的成员
- implementation 不能把 contract 已公开的 authoring 保证收窄
- 运行时命名允许变化，但 authoring 语义不允许漂移

当前严格要求的字段包括：

- prop: `TypeName` / `Required` / `AcceptsBinding` / `CaptureUnmatchedValues` / `Kind`
- emit: `PayloadTypeName` / `Kind`
- slot: `PatternOnly` / `IsDefault` / `Required` / `NamePattern` / context 参数数量 / context 参数类型
- component flags: `Flags`

另外，类型名比较做了一个很窄的规范化，只处理 `string` / `System.String` 这类 Roslyn 展示差异，不会把真正不兼容的类型放过。

### 注入触发边界

注入只在“当前解析到的 descriptor 就是容器 contract 本体”时触发。

也就是说：

- authored `<NavShell />` 会根据 `[VueInject]` 解析到具体实现
- authored `<ElementPlusNavShell />` 不会再被反向重写

这条规则保证 implementation 组件自身仍然可以被显式引用、诊断和测试，不会因为容器注入而产生隐式递归替换。

### 库组件元数据

#### 1. 组件级别元数据

```csharp
[VueLibraryComponent("vuetify", "VBtn")]
[VueLibraryComponentFlags(SupportsModelValue | IsFormControl)]
[VueLibraryStyle("vuetify/lib/components/VBtn/VBtn.css")]
[VueLibraryPluginRequirement("vuetify")]
public partial class VBtn : ComponentBase
{
    // ...
}
```

**元数据提取**：

```csharp
private static LibraryComponentMetadata GetLibraryMetadata(
    INamedTypeSymbol componentSymbol,
    RazorVueCompilationSymbols symbols)
{
    if (symbols.VueLibraryComponentAttribute is null)
        throw new InvalidOperationException(
            "VueLibraryComponentAttribute could not be resolved from the compilation.");

    var componentAttribute = componentSymbol.GetAttributes()
        .FirstOrDefault(attribute => Comparer.Equals(
            attribute.AttributeClass,
            symbols.VueLibraryComponentAttribute));

    if (componentAttribute is null ||
        componentAttribute.ConstructorArguments.Length < 2 ||
        componentAttribute.ConstructorArguments[0].Value is not string importSpecifier ||
        string.IsNullOrWhiteSpace(importSpecifier) ||
        componentAttribute.ConstructorArguments[1].Value is not string exportName ||
        string.IsNullOrWhiteSpace(exportName))
    {
        throw CreateInvalidLibraryComponentDeclarationException(
            componentSymbol,
            $"Library component '{FormatFullName(componentSymbol)}' must declare [VueLibraryComponent(importSpecifier, exportName)].");
    }

    var styleDependencies = GetLibraryStyleDependencies(componentSymbol, symbols);
    var pluginRequirements = GetLibraryPluginRequirements(componentSymbol, symbols);

    return new LibraryComponentMetadata(
        importSpecifier.Trim(),
        exportName.Trim(),
        styleDependencies,
        pluginRequirements);
}
```

#### 2. 属性级别元数据

```csharp
[VueProp("color", Required = true)]
[Parameter] public string Color { get; set; }

[VueProp("model-value", AcceptsBinding = true, Kind = VuePropKind.Model)]
[Parameter] public object Value { get; set; }

[VueProp("density", DefaultExpression = "'comfortable'")]
[Parameter] public string? Density { get; set; }
```

**元数据提取**：

```csharp
private static ImmutableDictionary<string, LibraryPropOverride> GetLibraryPropOverrides(
    INamedTypeSymbol componentSymbol,
    RazorVueCompilationSymbols symbols,
    ImmutableDictionary<string, IPropertySymbol> parameterLookup)
{
    if (symbols.VuePropAttribute is null)
        return ImmutableDictionary<string, LibraryPropOverride>.Empty
            .WithComparers(StringComparer.Ordinal);

    var builder = ImmutableDictionary.CreateBuilder<string, LibraryPropOverride>(
        StringComparer.Ordinal);

    foreach (var attribute in componentSymbol.GetAttributes())
    {
        if (!Comparer.Equals(attribute.AttributeClass, symbols.VuePropAttribute))
            continue;

        var publicName = GetRequiredConstructorStringArgument(
            attribute,
            0,
            componentSymbol,
            "VueProp");

        var property = GetRequiredParameter(
            componentSymbol,
            parameterLookup,
            publicName,
            "VueProp");

        // 验证：只能应用于普通 [Parameter] 属性
        if (IsEventCallback(property.Type, symbols) ||
            IsRenderFragment(property.Type, symbols))
        {
            throw CreateInvalidLibraryComponentDeclarationException(
                componentSymbol,
                $"Library component '{FormatFullName(componentSymbol)}' can only apply [VueProp] to regular [Parameter] properties. '{publicName}' is not a prop parameter.");
        }

        // 验证：防止重复声明
        if (builder.ContainsKey(publicName))
        {
            throw CreateInvalidLibraryComponentDeclarationException(
                componentSymbol,
                $"Library component '{FormatFullName(componentSymbol)}' declares duplicate [VueProp] metadata for '{publicName}'.");
        }

        builder[publicName] = new LibraryPropOverride(
            GetOptionalNamedStringArgument(attribute, "Name", componentSymbol, "VueProp"),
            GetOptionalNamedBoolArgument(attribute, "Required"),
            GetOptionalNamedBoolArgument(attribute, "AcceptsBinding"),
            GetOptionalNamedStringArgument(attribute, "DefaultExpression", componentSymbol, "VueProp"),
            attribute.ConstructorArguments.Length >= 2 && attribute.ConstructorArguments[1].Value is int propKind
                ? (VuePropKind)propKind
                : VuePropKind.Normal,
            attribute.ConstructorArguments.Length >= 2);
    }

    return builder.ToImmutable();
}
```

#### 3. 事件级别元数据

```csharp
[VueLibraryEmit("click", Kind = VueEmitKind.LibrarySpecific)]
[Parameter] public EventCallback<MouseEventArgs> OnClick { get; set; }

[VueLibraryEmit("update:modelValue", "string", Kind = VueEmitKind.ModelUpdate)]
[Parameter] public EventCallback<string> ValueChanged { get; set; }
```

**元数据提取**：

```csharp
private static ImmutableDictionary<string, LibraryEmitOverride> GetLibraryEmitOverrides(
    INamedTypeSymbol componentSymbol,
    RazorVueCompilationSymbols symbols,
    ImmutableDictionary<string, IPropertySymbol> parameterLookup)
{
    if (symbols.VueLibraryEmitAttribute is null)
        return ImmutableDictionary<string, LibraryEmitOverride>.Empty
            .WithComparers(StringComparer.Ordinal);

    var builder = ImmutableDictionary.CreateBuilder<string, LibraryEmitOverride>(
        StringComparer.Ordinal);

    foreach (var attribute in componentSymbol.GetAttributes())
    {
        if (!Comparer.Equals(attribute.AttributeClass, symbols.VueLibraryEmitAttribute))
            continue;

        var razorAlias = GetRequiredConstructorStringArgument(
            attribute,
            0,
            componentSymbol,
            "VueLibraryEmit");

        var property = GetRequiredParameter(
            componentSymbol,
            parameterLookup,
            razorAlias,
            "VueLibraryEmit");

        // 验证：只能应用于 EventCallback
        if (!IsEventCallback(property.Type, symbols))
        {
            throw CreateInvalidLibraryComponentDeclarationException(
                componentSymbol,
                $"Library component '{FormatFullName(componentSymbol)}' can only apply [VueLibraryEmit] to EventCallback parameters. '{razorAlias}' is not an event callback parameter.");
        }

        // 验证：防止重复声明
        if (builder.ContainsKey(razorAlias))
        {
            throw CreateInvalidLibraryComponentDeclarationException(
                componentSymbol,
                $"Library component '{FormatFullName(componentSymbol)}' declares duplicate [VueLibraryEmit] metadata for '{razorAlias}'.");
        }

        builder[razorAlias] = new LibraryEmitOverride(
            GetOptionalNamedStringArgument(attribute, "Name", componentSymbol, "VueLibraryEmit"),
            GetOptionalNamedStringArgument(attribute, "PayloadTypeName", componentSymbol, "VueLibraryEmit"),
            attribute.ConstructorArguments.Length >= 2 && attribute.ConstructorArguments[1].Value is int emitKind
                ? (VueEmitKind)emitKind
                : VueEmitKind.Normal,
            attribute.ConstructorArguments.Length >= 2);
    }

    return builder.ToImmutable();
}
```

#### 4. 插槽级别元数据

```csharp
[VueSlot("default", IsDefault = true)]
[Parameter] public RenderFragment ChildContent { get; set; }

[VueSlot("items", ContextTypeName = "ItemContext", ContextParameterName = "item")]
[Parameter] public RenderFragment<ItemContext> Items { get; set; }
```

**元数据提取**：

```csharp
private static ImmutableDictionary<string, LibrarySlotOverride> GetLibrarySlotOverrides(
    INamedTypeSymbol componentSymbol,
    RazorVueCompilationSymbols symbols,
    ImmutableDictionary<string, IPropertySymbol> parameterLookup)
{
    if (symbols.VueSlotAttribute is null)
        return ImmutableDictionary<string, LibrarySlotOverride>.Empty
            .WithComparers(StringComparer.Ordinal);

    var builder = ImmutableDictionary.CreateBuilder<string, LibrarySlotOverride>(
        StringComparer.Ordinal);

    foreach (var attribute in componentSymbol.GetAttributes())
    {
        if (!Comparer.Equals(attribute.AttributeClass, symbols.VueSlotAttribute))
            continue;

        var publicName = GetRequiredConstructorStringArgument(
            attribute,
            0,
            componentSymbol,
            "VueSlot");

        var property = GetRequiredParameter(
            componentSymbol,
            parameterLookup,
            publicName,
            "VueSlot");

        // 验证：只能应用于 RenderFragment
        if (!IsRenderFragment(property.Type, symbols))
        {
            throw CreateInvalidLibraryComponentDeclarationException(
                componentSymbol,
                $"Library component '{FormatFullName(componentSymbol)}' can only apply [VueSlot] to RenderFragment parameters. '{publicName}' is not a slot parameter.");
        }

        // 验证：防止重复声明
        if (builder.ContainsKey(publicName))
        {
            throw CreateInvalidLibraryComponentDeclarationException(
                componentSymbol,
                $"Library component '{FormatFullName(componentSymbol)}' declares duplicate [VueSlot] metadata for '{publicName}'.");
        }

        var slotName = GetOptionalNamedStringArgument(attribute, "Name", componentSymbol, "VueSlot");
        var isDefault = GetOptionalNamedBoolArgument(attribute, "IsDefault");

        // 验证：默认插槽必须命名为 "default"
        if (isDefault == true && slotName is not null &&
            !string.Equals(slotName, "default", StringComparison.Ordinal))
        {
            throw CreateInvalidLibraryComponentDeclarationException(
                componentSymbol,
                $"Library component '{FormatFullName(componentSymbol)}' must use slot name 'default' when [VueSlot] marks '{publicName}' as the default slot.");
        }

        var contextTypeName = GetOptionalNamedStringArgument(
            attribute,
            "ContextTypeName",
            componentSymbol,
            "VueSlot");

        // 验证：只有 RenderFragment<T> 才能声明上下文类型
        if (contextTypeName is not null && !IsTypedRenderFragment(property.Type, symbols))
        {
            throw CreateInvalidLibraryComponentDeclarationException(
                componentSymbol,
                $"Library component '{FormatFullName(componentSymbol)}' can only declare an explicit slot context type for RenderFragment<T> parameters. '{publicName}' is not typed child content.");
        }

        var contextParameterName = contextTypeName is null
            ? null
            : GetOptionalNamedStringArgument(attribute, "ContextParameterName", componentSymbol, "VueSlot")
                ?? "context";

        builder[publicName] = new LibrarySlotOverride(
            slotName,
            isDefault,
            GetOptionalNamedBoolArgument(attribute, "Required"),
            contextTypeName,
            contextParameterName);
    }

    return builder.ToImmutable();
}
```

### 命名转换实现

#### PascalCase → camelCase

```csharp
private static string ToLowerCamelCase(string value)
{
    if (string.IsNullOrEmpty(value))
        return value;

    if (value.Length == 1)
        return char.ToLowerInvariant(value[0]).ToString();

    // 处理缩写词（HTTP, URL, API）
    if (char.IsUpper(value[0]) && char.IsUpper(value[1]))
        return value;

    // 常规情况（FirstName → firstName）
    return char.ToLowerInvariant(value[0]) + value.Substring(1);
}
```

**转换示例**：

| 输入 | 输出 |
|------|------|
| `FirstName` | `firstName` |
| `IsActive` | `isActive` |
| `HTTPClient` | `HTTPClient` |
| `DataURL` | `DataURL` |
| `OnClick` | `onClick` (中间值) |
| `XMLParser` | `XMLParser` |

#### Emit 名称特殊处理

```csharp
private static string ToEmitName(string propertyName)
{
    // "On" + Xxxx → xxxx（去除 On 前缀）
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

| 输入 | 输出 |
|------|------|
| `OnClick` | `click` |
| `OnDataBound` | `dataBound` |
| `OnValueChanged` | `valueChanged` (非 v-model) |
| `ValueChanged` | `valueChanged` (非 v-model) |
| `HandleClick` | `handleClick` |

### 导入路径生成

#### 用户组件

```csharp
private static string GetUserImportSpecifier(
    INamedTypeSymbol componentSymbol,
    RazorVueCompilationSymbols symbols)
{
    // 优先使用 [ECMAScriptModule] 特性声明的路径
    foreach (var attribute in componentSymbol.GetAttributes())
    {
        if (!Comparer.Equals(attribute.AttributeClass, symbols.ECMAScriptModuleAttribute))
            continue;

        if (attribute.ConstructorArguments.Length == 1 &&
            attribute.ConstructorArguments[0].Value is string importPath &&
            !string.IsNullOrWhiteSpace(importPath))
        {
            return NormalizeImportPath(importPath);
        }
    }

    // 默认路径：程序集/命名空间/文件名.mjs
    var assemblyName = componentSymbol.ContainingAssembly?.Name ?? "Jazor.Assembly";
    var namespaceName = GetResolutionNamespace(componentSymbol).Replace('.', '/');
    var fileName = $"{componentSymbol.Name}.mjs";

    return string.IsNullOrEmpty(namespaceName)
        ? $"{assemblyName}/{fileName}"
        : $"{assemblyName}/{namespaceName}/{fileName}";
}
```

**路径示例**：

```csharp
// [ECMAScriptModule("./components/Button")] → "./components/Button"

// App.Components.Button (无特性)
// → "Jazor.Assembly/App/Components/Button.mjs"

// Root.MyComponent (全局命名空间)
// → "Jazor.Assembly/MyComponent.mjs"
```

#### 路径规范化

```csharp
private static string NormalizeImportPath(string importPath)
{
    var normalized = importPath.Replace('\\', '/').Trim();

    // 确保有 .mjs 扩展名
    var extension = normalized.EndsWith(".js", StringComparison.OrdinalIgnoreCase) ||
                    normalized.EndsWith(".mjs", StringComparison.OrdinalIgnoreCase)
        ? normalized
        : $"{normalized}.mjs";

    return extension;
}
```

## 文件位置

- **工厂类**：`src/Jazor.RazorVue/Descriptor/VueComponentDescriptorFactory.cs`
- **容器注入注册表**：`src/Jazor.RazorVue/Descriptor/VueInjectRegistry.cs`

## 相关文档

- **组件描述符**：`docs/01-目标/razorvue/descriptor/ComponentDescriptor.md`
- **组件注册表**：`docs/01-目标/razorvue/descriptor/ComponentRegistry.md`
- **内置组件**：`docs/01-目标/razorvue/descriptor/IntrinsicComponents.md`
- **编译问题**：`docs/01-目标/razorvue/descriptor/CompilationIssues.md`

---

**维护者**：developerhan
**最后更新**：2026-05-14
**文档版本**：v1.1
