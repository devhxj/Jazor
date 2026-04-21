# RazorVue Artifact Factory

## 概述

**RazorVueArtifactFactory** 是 RazorVue 降级器的核心实现，负责将 Razor 组件的语义快照（`RazorVueSemanticSnapshot`）和渲染树（`RazorVueRenderFragment`）转换为完整的 Vue 单文件组件（SFC）模块代码。

**核心文件**: `src/Jazor.RazorVue/Lowering/RazorVueArtifactFactory.cs`

## 核心职责

1. **组件解析**: 解析渲染树中的组件引用，构建组件映射表
2. **依赖分析**: 收集导入、样式、插件需求
3. **代码生成**: 生成完整的 Vue SFC 模块代码
4. **身份计算**: 计算组件的 HMR（热模块替换）哈希
5. **边界分类**: 确定组件的 HMR 边界类型

## 架构设计

### 1. IRazorVueArtifactLowerer 接口实现

```csharp
internal sealed partial class RazorVueArtifactFactory : IRazorVueArtifactLowerer
{
    public VueCompiledArtifact Lower(RazorVueCompilationContext context, RazorVueSemanticSnapshot snapshot)
    {
        var renderTree = _renderTreeExtractor.Extract(context, snapshot);
        return CreateCore(context, snapshot, renderTree);
    }
}
```

**两个重载**:
- `Lower(context, snapshot)`: 完整降级（包含渲染树提取）
- `Lower(snapshot)`: 语义降级（仅处理生命周期和逻辑）

### 2. 创建核心：CreateCore 方法

```csharp
private static VueCompiledArtifact CreateCore(
    RazorVueCompilationContext? context,
    RazorVueSemanticSnapshot snapshot,
    RazorVueRenderFragment renderTree)
{
    var descriptor = snapshot.Descriptor;
    var resolvedComponents = context is null
        ? ImmutableDictionary<string, VueComponentDescriptor>.Empty
        : ResolveComponents(context, snapshot, renderTree);
    var componentReferences = BuildComponentReferences(resolvedComponents);
    var componentEmitsByRazorAlias = BuildComponentEmitsByRazorAlias(resolvedComponents);
    var expressionEmitter = new RazorVueExpressionEmitter(
        snapshot,
        componentReferences,
        resolvedComponents,
        componentEmitsByRazorAlias);
    var moduleCode = BuildModuleCode(snapshot, renderTree, expressionEmitter, resolvedComponents);
    var sourceOrigins = snapshot.Origins.AddRange(expressionEmitter.CollectOrigins(renderTree));

    return new VueCompiledArtifact(
        ComponentName: descriptor.Name,
        RelativeModulePath: relativeModulePath,
        ModuleCode: moduleCode,
        Imports: BuildImports(resolvedComponents),
        Styles: BuildStyles(descriptor, resolvedComponents),
        PluginRequirements: BuildPluginRequirements(descriptor, resolvedComponents),
        Identity: BuildIdentity(context, snapshot, renderTree, expressionEmitter, relativeModulePath),
        Hints: BuildHints(moduleCode),
        SourceOrigins: sourceOrigins);
}
```

**处理流程**:
1. **组件解析**: 解析渲染树中的子组件引用
2. **表达式发射器**: 创建表达式翻译器
3. **模块代码生成**: 生成 Vue SFC 模块
4. **身份计算**: 计算 HMR 哈希
5. **元数据收集**: 收集导入、样式、插件需求

## 组件解析（ComponentResolver）

### 1. ResolveComponents 方法

**文件**: `src/Jazor.RazorVue/Lowering/RazorVueArtifactFactory.ComponentResolver.cs`

```csharp
private static ImmutableDictionary<string, VueComponentDescriptor> ResolveComponents(
    RazorVueCompilationContext context,
    RazorVueSemanticSnapshot snapshot,
    RazorVueRenderFragment renderTree)
{
    var components = CollectComponents(renderTree);
    if (components.Count == 0)
        return ImmutableDictionary<string, VueComponentDescriptor>.Empty;

    var registry = context.CreateComponentRegistry();
    var resolutionContext = new VueComponentResolutionContext(
        snapshot.Descriptor.ResolutionNamespace,
        snapshot.ImportedNamespaces);
    var builder = ImmutableDictionary.CreateBuilder<string, VueComponentDescriptor>(StringComparer.Ordinal);

    foreach (var component in components)
    {
        var result = ResolveComponentDescriptor(registry, resolutionContext, component);
        if (result.Status != VueComponentResolutionStatus.Resolved || result.Descriptor is null)
            throw CreateResolutionIssueException(result, snapshot.Descriptor.FullName, component);

        builder[component.ComponentName] = result.Descriptor;
    }

    return builder.ToImmutable();
}
```

**关键步骤**:
1. **收集组件**: 遍历渲染树，提取所有 `RazorVueComponentNode`
2. **创建注册表**: 使用上下文创建组件注册表
3. **解析组件**: 逐个解析组件引用
4. **错误处理**: 解析失败时抛出 `RazorVueCompilationIssueException`

### 2. CollectComponents 方法

```csharp
private static HashSet<RazorVueComponentNode> CollectComponents(RazorVueRenderFragment fragment)
{
    var result = new HashSet<RazorVueComponentNode>();
    foreach (var child in fragment.Children)
        CollectComponents(child, result);
    return result;
}

private static void CollectComponents(RazorVueRenderNode node, HashSet<RazorVueComponentNode> components)
{
    switch (node)
    {
        case RazorVueComponentNode component:
            components.Add(component);
            foreach (var child in component.Children.Children)
                CollectComponents(child, components);
            break;
        case RazorVueElementNode element:
            foreach (var child in element.Children.Children)
                CollectComponents(child, components);
            break;
        case RazorVueConditionalNode conditional:
            foreach (var child in conditional.WhenTrue.Children)
                CollectComponents(child, components);
            foreach (var child in conditional.WhenFalse.Children)
                CollectComponents(child, components);
            break;
        case RazorVueForEachNode loop:
            foreach (var child in loop.Body.Children)
                CollectComponents(child, components);
            break;
    }
}
```

**深度优先遍历**: 递归遍历所有节点类型，收集组件节点。

### 3. 组件引用映射

```csharp
private static ImmutableDictionary<string, string> BuildComponentReferences(
    ImmutableDictionary<string, VueComponentDescriptor> resolvedComponents)
{
    if (resolvedComponents.IsEmpty)
        return ImmutableDictionary<string, string>.Empty;

    var builder = ImmutableDictionary.CreateBuilder<string, string>(StringComparer.Ordinal);
    foreach (var item in resolvedComponents)
    {
        if (string.Equals(item.Value.ImportSpecifier, "vue", StringComparison.Ordinal))
            builder[item.Key] = item.Value.ExportName;  // 内置组件直接使用导出名
        else
            builder[item.Key] = CreateComponentAlias(item.Key);  // 其他组件使用别名
    }

    return builder.ToImmutable();
}

private static string CreateComponentAlias(string componentName)
    => componentName + "Component";
```

**映射规则**:
- Vue 内置组件（`Teleport`, `Suspense`, `KeepAlive`）→ 直接使用导出名
- 其他组件 → 使用 `{ComponentName}Component` 别名

### 4. 事件处理程序映射

```csharp
private static ImmutableDictionary<string, ImmutableDictionary<string, string>> BuildComponentEmitsByRazorAlias(
    ImmutableDictionary<string, VueComponentDescriptor> resolvedComponents)
{
    var builder = ImmutableDictionary.CreateBuilder<string, ImmutableDictionary<string, string>>(StringComparer.Ordinal);
    foreach (var item in resolvedComponents)
    {
        var emitsBuilder = ImmutableDictionary.CreateBuilder<string, string>(StringComparer.Ordinal);
        foreach (var emit in item.Value.Emits)
        {
            if (!string.IsNullOrWhiteSpace(emit.RazorAlias))
                emitsBuilder[emit.RazorAlias!] = ToVueEventHandlerName(emit.Name);
        }

        builder[item.Key] = emitsBuilder.ToImmutable();
    }

    return builder.ToImmutable();
}

private static string ToVueEventHandlerName(string eventName)
{
    if (IsVueEventHandlerName(eventName))
        return eventName;

    return "on" + char.ToUpperInvariant(eventName[0]) + eventName.Substring(1);
}

private static bool IsVueEventHandlerName(string eventName)
{
    if (!eventName.StartsWith("on", StringComparison.Ordinal) || eventName.Length <= 2)
        return false;

    var marker = eventName[2];
    return char.IsUpper(marker) || marker == ':';
}
```

**转换规则**:
- `ValueChanged` → `onUpdate:valueChanged`
- `OnClick` → `onClick`
- 已符合 Vue 规范的名称保持不变

## 导入和样式构建（ImportStyleBuilder）

### 1. 导入构建

**文件**: `src/Jazor.RazorVue/Lowering/RazorVueArtifactFactory.ImportStyleBuilder.cs`

```csharp
private static ImmutableArray<string> BuildImports(ImmutableDictionary<string, VueComponentDescriptor> resolvedComponents)
{
    if (resolvedComponents.IsEmpty)
        return ImmutableArray.Create("vue");

    return ImmutableArray.Create("vue").AddRange(
        resolvedComponents.Values
            .Select(descriptor => descriptor.ImportSpecifier)
            .Where(importSpecifier => !string.Equals(importSpecifier, "vue", StringComparison.Ordinal))
            .Distinct(StringComparer.Ordinal));
}
```

**结果示例**:
```javascript
["vue", "./components/Button.mjs", "./components/Card.mjs"]
```

### 2. 样式构建

```csharp
private static ImmutableArray<string> BuildStyles(
    VueComponentDescriptor descriptor,
    ImmutableDictionary<string, VueComponentDescriptor> resolvedComponents)
{
    var builder = ImmutableArray.CreateBuilder<string>();
    var seen = new HashSet<string>(StringComparer.Ordinal);

    // 当前组件的样式
    foreach (var style in descriptor.StyleDependencies)
    {
        if (!string.IsNullOrWhiteSpace(style) && seen.Add(style))
            builder.Add(style);
    }

    // 递归收集子组件的样式
    foreach (var component in resolvedComponents.Values)
    {
        foreach (var style in component.StyleDependencies)
        {
            if (!string.IsNullOrWhiteSpace(style) && seen.Add(style))
                builder.Add(style);
        }
    }

    return builder.ToImmutable();
}
```

**去重策略**: 使用 `HashSet<string>` 确保样式文件唯一。

### 3. 插件需求构建

```csharp
private static ImmutableArray<string> BuildPluginRequirements(
    VueComponentDescriptor descriptor,
    ImmutableDictionary<string, VueComponentDescriptor> resolvedComponents)
{
    var builder = ImmutableArray.CreateBuilder<string>();
    var seen = new HashSet<string>(StringComparer.Ordinal);

    // 当前组件的插件需求
    foreach (var requirement in descriptor.PluginRequirements)
    {
        if (!string.IsNullOrWhiteSpace(requirement) && seen.Add(requirement))
            builder.Add(requirement);
    }

    // 递归收集子组件的插件需求
    foreach (var component in resolvedComponents.Values)
    {
        foreach (var requirement in component.PluginRequirements)
        {
            if (!string.IsNullOrWhiteSpace(requirement) && seen.Add(requirement))
                builder.Add(requirement);
        }
    }

    return builder.ToImmutable();
}
```

**用途**: 标识组件需要的 Vue 插件（如 `router`, `pinia`）。

### 4. 组件导入代码生成

```csharp
private static void AppendComponentImports(StringBuilder builder, ImmutableDictionary<string, VueComponentDescriptor> resolvedComponents)
{
    var groups = resolvedComponents
        .Where(pair => !string.Equals(pair.Value.ImportSpecifier, "vue", StringComparison.Ordinal))
        .OrderBy(pair => pair.Key, StringComparer.Ordinal)
        .GroupBy(pair => pair.Value.ImportSpecifier, StringComparer.Ordinal);

    foreach (var group in groups)
    {
        AppendGroupedComponentImports(builder, group.Key, group.ToImmutableArray());
    }
}

private static void AppendGroupedComponentImports(
    StringBuilder builder,
    string importSpecifier,
    ImmutableArray<KeyValuePair<string, VueComponentDescriptor>> components)
{
    // 命名导入（库组件的非默认导出）
    var namedImports = components
        .Where(item => item.Value.SourceKind == VueComponentSourceKind.LibraryComponent &&
                      !string.Equals(item.Value.ExportName, "default", StringComparison.Ordinal))
        .Select(item => item.Value.ExportName + " as " + CreateComponentAlias(item.Key))
        .ToImmutableArray();

    // 默认导入
    foreach (var item in components)
    {
        if (item.Value.SourceKind == VueComponentSourceKind.LibraryComponent &&
            !string.Equals(item.Value.ExportName, "default", StringComparison.Ordinal))
        {
            continue;
        }

        AppendDefaultComponentImport(builder, item.Key, importSpecifier);
    }

    // 命名导入聚合
    if (!namedImports.IsDefaultOrEmpty)
    {
        builder.Append("import { ");
        builder.Append(string.Join(", ", namedImports));
        builder.Append(" } from ");
        builder.Append(ToJavaScriptString(importSpecifier));
        builder.AppendLine(";");
    }
}
```

**生成示例**:
```javascript
import ButtonComponent from "./Button.mjs";
import CardComponent from "./Card.mjs";
import { VBtn as VBtnComponent, VCard as VCardComponent } from "vuetify";
```

## HMR 身份计算

### 1. BuildIdentity 方法

```csharp
private static VueArtifactIdentity BuildIdentity(
    RazorVueCompilationContext? context,
    RazorVueSemanticSnapshot snapshot,
    RazorVueRenderFragment renderTree,
    RazorVueExpressionEmitter expressionEmitter,
    string relativeModulePath)
{
    var descriptor = snapshot.Descriptor;
    var descriptorShape = BuildDescriptorShape(descriptor);
    var templateShape = expressionEmitter.DescribeFragment(renderTree);
    var logicShape = BuildLogicShape(context, snapshot, renderTree, expressionEmitter);
    var boundaryKind = ClassifyHmrBoundary(renderTree, snapshot);

    return new VueArtifactIdentity(
        ComponentId: descriptor.FullName,
        ModuleId: relativeModulePath,
        DescriptorHash: ComputeSha256Hex(descriptorShape),
        TemplateHash: ComputeSha256Hex(templateShape),
        LogicHash: ComputeSha256Hex(logicShape),
        HmrBoundaryKind: boundaryKind);
}
```

**四个哈希**:
1. **DescriptorHash**: 组件描述符的哈希（Props、Emits、Slots）
2. **TemplateHash**: 模板结构的哈希
3. **LogicHash**: 生命周期和逻辑方法的哈希
4. **HmrBoundaryKind**: HMR 边界分类

### 2. SHA256 哈希计算

```csharp
private static string ComputeSha256Hex(string content)
{
    using var sha = SHA256.Create();
    var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(content ?? string.Empty));
    var builder = new StringBuilder(bytes.Length * 2);
    foreach (var item in bytes)
        builder.Append(item.ToString("X2"));
    return builder.ToString();
}
```

**特点**:
- 使用十六进制表示（大写）
- 固定长度（64 个字符）
- 内容为空时返回空字符串的哈希

### 3. 描述符形状构建

```csharp
private static string BuildDescriptorShape(VueComponentDescriptor descriptor)
{
    var descriptorShape = new StringBuilder();
    descriptorShape.AppendLine(descriptor.FullName);
    descriptorShape.AppendLine(descriptor.SourceKind.ToString());
    descriptorShape.AppendLine(descriptor.ImportSpecifier);
    descriptorShape.AppendLine(descriptor.ExportName);
    descriptorShape.AppendLine("flags:" + descriptor.Flags);

    // Props
    foreach (var prop in descriptor.Props.OrderBy(prop => prop.PublicName, StringComparer.Ordinal))
        descriptorShape.AppendLine(
            prop.PublicName + "|" +
            prop.Name + "|" +
            prop.TypeName + "|" +
            prop.Required + "|" +
            prop.AcceptsBinding + "|" +
            (prop.DefaultExpression ?? string.Empty) + "|" +
            prop.Kind);

    // Emits
    foreach (var emit in descriptor.Emits.OrderBy(emit => emit.RazorAlias, StringComparer.Ordinal))
        descriptorShape.AppendLine(emit.RazorAlias + "|" + emit.Name + "|" + emit.PayloadTypeName + "|" + emit.Kind);

    // Slots
    foreach (var slot in descriptor.Slots.OrderBy(slot => slot.Name, StringComparer.Ordinal))
        descriptorShape.AppendLine(
            slot.PublicName + "|" +
            slot.Name + "|" +
            slot.IsDefault + "|" +
            slot.Required + "|" +
            string.Join(",", slot.Parameters.Select(parameter => parameter.Name + ":" + parameter.TypeName)));

    // Plugin requirements
    foreach (var pluginRequirement in descriptor.PluginRequirements.OrderBy(requirement => requirement, StringComparer.Ordinal))
        descriptorShape.AppendLine("plugin:" + pluginRequirement);

    return descriptorShape.ToString();
}
```

**格式**: 每行一个条目，字段用 `|` 分隔。

### 4. 逻辑形状构建

```csharp
private static string BuildLogicShape(
    RazorVueCompilationContext? context,
    RazorVueSemanticSnapshot snapshot,
    RazorVueRenderFragment renderTree,
    RazorVueExpressionEmitter expressionEmitter)
{
    var descriptor = snapshot.Descriptor;
    var logicShape = new StringBuilder();

    // 生命周期方法
    logicShape.AppendLine("component:" + descriptor.FullName);
    logicShape.AppendLine("module:" + descriptor.ImportSpecifier);
    logicShape.AppendLine("lifecycle:onInitialized=" + DescribeLifecycleLoweringShape(snapshot, snapshot.OnInitializedMethod, false));
    logicShape.AppendLine("lifecycle:onInitializedAsync=" + DescribeLifecycleLoweringShape(snapshot, snapshot.OnInitializedAsyncMethod, false));
    logicShape.AppendLine("lifecycle:onParametersSet=" + DescribeLifecycleLoweringShape(snapshot, snapshot.OnParametersSetMethod, false));
    logicShape.AppendLine("lifecycle:onParametersSetAsync=" + DescribeLifecycleLoweringShape(snapshot, snapshot.OnParametersSetAsyncMethod, false));
    logicShape.AppendLine("lifecycle:setParametersAsync=" + DescribeSetParametersAsyncShape(snapshot, snapshot.SetParametersAsyncMethod));
    logicShape.AppendLine("lifecycle:onAfterRender=" + DescribeLifecycleLoweringShape(snapshot, snapshot.OnAfterRenderMethod, true));
    logicShape.AppendLine("lifecycle:onAfterRenderAsync=" + DescribeLifecycleLoweringShape(snapshot, snapshot.OnAfterRenderAsyncMethod, true));
    logicShape.AppendLine("lifecycle:shouldRender=" + DescribeShouldRenderShape(snapshot.Compilation, snapshot.ShouldRenderMethod));
    logicShape.AppendLine("lifecycle:dispose=" + DescribeLifecycleLoweringShape(snapshot, snapshot.DisposeMethod, false));
    logicShape.AppendLine("lifecycle:disposeAsync=" + DescribeLifecycleLoweringShape(snapshot, snapshot.DisposeAsyncMethod, false));

    // 字段
    foreach (var field in snapshot.Logic.Fields.OrderBy(field => field.Name, StringComparer.Ordinal))
        logicShape.AppendLine("field:" + field.Name + "|" + field.IsReadOnly + "|" + DescribeSetupFieldShape(field.FieldSymbol));

    // 方法
    foreach (var method in snapshot.Logic.Methods.OrderBy(method => method.Name, StringComparer.Ordinal).ThenBy(method => method.Arity))
        logicShape.AppendLine("logic:" + method.Name + "|" + method.Arity + "|" + method.IsAsync + "|" + DescribeSetupMethodShape(method.MethodSymbol));

    return logicShape.ToString();
}
```

**关键逻辑**:
- 生命周期方法的"无操作"实现不会影响哈希
- 字段和方法的形状基于其初始值/表达式

## HMR 边界分类

### 1. ClassifyHmrBoundary 方法

```csharp
private static HmrBoundaryKind ClassifyHmrBoundary(
    RazorVueRenderFragment renderTree,
    RazorVueSemanticSnapshot snapshot)
{
    var descriptor = snapshot.Descriptor;

    // 无 Props/Emits/Slots → 需要完全重载
    if (descriptor.Props.Length == 0 && descriptor.Emits.Length == 0 && descriptor.Slots.Length == 0)
        return HmrBoundaryKind.FullReloadRequired;

    // 包含不支持的模板节点 → 需要完全重载
    if (HasUnsupportedTemplateNode(renderTree))
        return HmrBoundaryKind.FullReloadRequired;

    // ShouldRender 不受支持 → 需要完全重载
    if (snapshot.Lifecycle.HasShouldRender &&
        !AnalyzeShouldRender(snapshot.Compilation, snapshot.ShouldRenderMethod).IsSupported)
    {
        return HmrBoundaryKind.FullReloadRequired;
    }

    // SetParametersAsync 不受支持 → 需要完全重载
    if (snapshot.Lifecycle.HasSetParametersAsync &&
        !AnalyzeSetParametersAsync(snapshot, snapshot.SetParametersAsyncMethod).IsSupported)
    {
        return HmrBoundaryKind.FullReloadRequired;
    }

    // 有生命周期方法或逻辑 → 逻辑安全
    var hasSupportedLifecycleLowering = HasSupportedLifecycleLowering(snapshot, snapshot.OnInitializedMethod, false) ||
                                       HasSupportedLifecycleLowering(snapshot, snapshot.OnInitializedAsyncMethod, false) ||
                                       HasSupportedLifecycleLowering(snapshot, snapshot.OnParametersSetMethod, false) ||
                                       HasSupportedLifecycleLowering(snapshot, snapshot.OnParametersSetAsyncMethod, false) ||
                                       HasSupportedSetParametersAsyncLowering(snapshot) ||
                                       HasSupportedLifecycleLowering(snapshot, snapshot.OnAfterRenderMethod, true) ||
                                       HasSupportedLifecycleLowering(snapshot, snapshot.OnAfterRenderAsyncMethod, true) ||
                                       HasSupportedLifecycleLowering(snapshot, snapshot.DisposeMethod, false) ||
                                       HasSupportedLifecycleLowering(snapshot, snapshot.DisposeAsyncMethod, false);

    if (hasSupportedLifecycleLowering || snapshot.Logic.Fields.Length > 0 || snapshot.Logic.Methods.Length > 0)
        return HmrBoundaryKind.LogicSafe;

    // 纯模板变化 → 仅模板
    if (HasTemplateShape(renderTree))
        return HmrBoundaryKind.TemplateOnly;

    return HmrBoundaryKind.Unknown;
}
```

**边界类型**:
- **FullReloadRequired**: 需要完全重载（不安全的组件）
- **LogicSafe**: 逻辑安全（支持精确 HMR）
- **TemplateOnly**: 仅模板（可以热替换模板）
- **Unknown**: 未知状态

### 2. 模板形状检测

```csharp
private static bool HasTemplateShape(RazorVueRenderFragment fragment)
{
    if (fragment.Children.IsDefaultOrEmpty)
        return false;

    foreach (var child in fragment.Children)
    {
        switch (child)
        {
            case RazorVueElementNode:
            case RazorVueComponentNode:
            case RazorVueTextNode:
            case RazorVueExpressionNode:
            case RazorVueSlotOutletNode:
                return true;
            case RazorVueConditionalNode conditional:
                if (HasTemplateShape(conditional.WhenTrue) || HasTemplateShape(conditional.WhenFalse))
                    return true;
                break;
            case RazorVueForEachNode loop:
                if (HasTemplateShape(loop.Body))
                    return true;
                break;
        }
    }

    return false;
}
```

## 运行时提示构建

```csharp
private static VueRuntimeHints BuildHints(string moduleCode)
    => new(
        RequiresVueRuntime: true,
        RequiresHydration: false,
        SupportsSsr: true,
        UsesTeleport: moduleCode.Contains("Teleport", StringComparison.Ordinal),
        UsesSuspense: moduleCode.Contains("Suspense", StringComparison.Ordinal),
        UsesKeepAlive: moduleCode.Contains("KeepAlive", StringComparison.Ordinal));
```

**用途**: 为开发服务器和构建工具提供元数据。

## 路径规范化

```csharp
private static string NormalizeRelativePath(string relativePath)
{
    var normalized = relativePath.Replace('\\', '/').TrimStart('/');
    while (normalized.StartsWith("./", StringComparison.Ordinal))
        normalized = normalized.Substring(2);

    if (string.IsNullOrWhiteSpace(normalized))
        throw new InvalidOperationException("RazorVue artifact relative path cannot be empty.");

    if (!normalized.EndsWith(".js", StringComparison.OrdinalIgnoreCase) &&
        !normalized.EndsWith(".mjs", StringComparison.OrdinalIgnoreCase))
    {
        normalized += ".mjs";
    }

    return normalized;
}
```

**规则**:
- 反斜杠转换为正斜杠
- 移除前导 `./`
- 默认扩展名 `.mjs`

## 命名转换工具

```csharp
private static string ToLowerCamelCase(string value)
{
    if (string.IsNullOrEmpty(value))
        return value;

    if (value.Length == 1)
        return char.ToLowerInvariant(value[0]).ToString();

    if (char.IsUpper(value[0]) && char.IsUpper(value[1]))
        return value;  // 缩写词保持不变（如 "HTML"）

    return char.ToLowerInvariant(value[0]) + value.Substring(1);
}
```

**示例**:
- `MyComponent` → `myComponent`
- `HTMLContent` → `HTMLContent`（缩写词保护）
- `onClick` → `onClick`（已经是小驼峰）

## 相关文档

- **模块构建器**: `docs/01-目标/razorvue/lowering/ModuleBuilder.md`
- **表达式发射器**: `docs/01-目标/razorvue/lowering/ExpressionEmitter.md`
- **组件创作**: `docs/01-目标/razorvue/lowering/ComponentAuthoring.md`
- **生命周期降级**: `docs/01-目标/razorvue/lowering/LifecycleLowering.md`

---

**维护者**: developerhan
**最后更新**: 2026-04-21
**版本**: v1.0
