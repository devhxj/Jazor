# RazorVueGenerator 设计文档

## 1. 概述

**RazorVueGenerator** 是 RazorVue 线路的 Roslyn Source Generator，负责在编译时发现组件候选、执行 pipeline、生成 `RazorVueCatalog` 代码。它遵循"薄宿主"原则：仅处理 Roslyn 集成和诊断，RazorVue 语义逻辑位于 `Jazor.RazorVue` 项目中。

**文件位置**: `src/Jazor.Analyzer/RazorVue/Generation/RazorVueGenerator.cs`

**特性**: `[Generator]`, `IIncrementalGenerator`

## 2. 诊断系统

### 2.1 诊断描述符列表

Generator 定义了 14 个诊断描述符，覆盖所有 RazorVue 编译问题：

| ID | 标题 | RazorVueIssueCode | 默认严重性 |
|----|------|-------------------|-----------|
| JAZORVGA001 | RazorVue catalog generation failed | (通用) | Error |
| JAZORVGA002 | RazorVue component not found | ComponentNotFound | Error |
| JAZORVGA003 | RazorVue component name is ambiguous | AmbiguousComponentName | Error |
| JAZORVGA004 | RazorVue component name collides with intrinsic | ReservedIntrinsicNameCollision | Error |
| JAZORVGA005 | RazorVue lifecycle lowering is unsupported | UnsupportedLifecycleLowering | Error |
| JAZORVGA006 | RazorVue setup logic lowering is unsupported | UnsupportedSetupLogicLowering | Error |
| JAZORVGA007 | RazorVue parameter is unknown | UnknownParameter | Error |
| JAZORVGA008 | RazorVue bind target is invalid | InvalidBindTarget | Error |
| JAZORVGA009 | RazorVue child content parameter is unknown | UnknownSlot | Error |
| JAZORVGA010 | RazorVue child content parameter context is invalid | SlotContextMisuse | Error |
| JAZORVGA011 | RazorVue child content parameter is assigned multiple times | DuplicateSlotValue | Error |
| JAZORVGA012 | RazorVue library component declaration is invalid | InvalidLibraryComponentDeclaration | Error |
| JAZORVGA013 | RazorVue library style dependency declaration is invalid | InvalidLibraryStyleDependencyDeclaration | Error |
| JAZORVGA014 | RazorVue library plugin requirement declaration is invalid | InvalidLibraryPluginRequirementDeclaration | Error |

### 2.2 诊断描述符定义示例

```csharp
private static readonly DiagnosticDescriptor RazorVueGenerationFailed = new(
    id: "JAZORVGA001",
    title: "RazorVue catalog generation failed",
    messageFormat: "Failed to generate RazorVue catalog for '{0}': {1}",
    category: "Jazor.RazorVue.Analysis",
    defaultSeverity: DiagnosticSeverity.Error,
    isEnabledByDefault: true);

private static readonly DiagnosticDescriptor RazorVueComponentNotFound = new(
    id: "JAZORVGA002",
    title: "RazorVue component not found",
    messageFormat: "{0}",
    category: "Jazor.RazorVue.Analysis",
    defaultSeverity: DiagnosticSeverity.Error,
    isEnabledByDefault: true);
```

**设计原则**:
- 所有诊断属于 `Jazor.RazorVue.Analysis` 类别
- 默认启用（`isEnabledByDefault: true`）
- 错误严重性（`DiagnosticSeverity.Error`）阻止编译
- 消息格式支持参数化（`{0}`, `{1}`）

## 3. 候选组件发现

### 3.1 增量发现管道

Generator 使用增量生成器管道发现候选组件：

```csharp
public void Initialize(IncrementalGeneratorInitializationContext context)
{
    var componentCandidates = context.SyntaxProvider.ForAttributeWithMetadataName(
        fullyQualifiedMetadataName: "ECMAScript.ECMAScriptModuleAttribute",
        predicate: static (node, _) => node is ClassDeclarationSyntax,
        transform: static (syntaxContext, _) => CreateCandidate(syntaxContext))
        .Where(static candidate => candidate is not null);

    var combined = context.CompilationProvider.Combine(componentCandidates.Collect());
    context.RegisterSourceOutput(combined, static (outputContext, source) =>
    {
        var (compilation, candidates) = source;
        EmitRazorVueCatalog(outputContext, compilation, candidates);
    });
}
```

### 3.2 候选组件创建

```csharp
private static ModuleCandidate? CreateCandidate(GeneratorAttributeSyntaxContext context)
{
    if (context.TargetNode is not ClassDeclarationSyntax)
        return null;

    if (context.TargetSymbol is not INamedTypeSymbol classSymbol)
        return null;

    return new ModuleCandidate(classSymbol, context.TargetNode.GetLocation());
}

private sealed record ModuleCandidate(
    INamedTypeSymbol ClassSymbol,
    Location Location);
```

**发现条件**:
- 目标节点是 `ClassDeclarationSyntax`（类声明）
- 目标符号是 `INamedTypeSymbol`（命名类型符号）
- 类标记了 `[ECMAScriptModule]` 特性

**记录数据**:
- `ClassSymbol`: 候选组件的类型符号
- `Location`: 候选组件在源代码中的位置（用于诊断定位）

## 4. 代码生成流程

### 4.1 EmitRazorVueCatalog 主流程

```csharp
private static void EmitRazorVueCatalog(
    SourceProductionContext context,
    Compilation compilation,
    ImmutableArray<ModuleCandidate?> candidates)
{
    // 1. 上下文验证
    var razorVueContext = RazorVueCompilationContext.TryCreate(compilation);
    if (razorVueContext is null)
        return;

    if (!candidates.Any(static candidate => candidate is not null))
        return;

    var candidate = candidates.FirstOrDefault(static candidate => candidate is not null);

    try
    {
        // 2. 库组件发现（验证阶段）
        _ = razorVueContext.DiscoverLibraryComponents();

        // 3. 执行 pipeline
        var catalog = new RazorVuePipeline(
            RazorVueRazorDocumentSemanticFrontend.Instance,
            RazorVuePreferredTemplateFrontend.Instance).Execute(compilation);
        if (catalog.Artifacts.IsDefaultOrEmpty)
            return;

        // 4. 生成代码
        context.AddSource("Jazor.Generated.RazorVueCatalog.g.cs", BuildRazorVueCatalogSource(catalog));
    }
    catch (RazorVueCompilationIssueException issueException)
    {
        context.ReportDiagnostic(CreateCompilationIssueDiagnostic(issueException, candidate));
    }
    catch (NotSupportedException ex) when (TryCreateUnsupportedSetupLogicIssueException(ex, candidate, out var issueException))
    {
        context.ReportDiagnostic(CreateCompilationIssueDiagnostic(issueException!, candidate));
    }
    catch (global::System.Exception ex)
    {
        var location = candidate?.Location ?? Location.None;
        var typeName = candidate?.ClassSymbol.ToDisplayString() ?? (compilation.AssemblyName ?? "Jazor.Assembly");
        context.ReportDiagnostic(Diagnostic.Create(
            RazorVueGenerationFailed,
            location,
            typeName,
            ex.Message));
    }
}
```

### 4.2 流程分解

#### 步骤 1: 上下文验证

```csharp
var razorVueContext = RazorVueCompilationContext.TryCreate(compilation);
if (razorVueContext is null)
    return;

if (!candidates.Any(static candidate => candidate is not null))
    return;
```

**早期退出条件**:
- RazorVueCompilationContext 创建失败（项目不是 RazorVue 项目）
- 没有候选组件（没有标记 `[ECMAScriptModule]` 的类）

**设计决策**: 静默返回而非抛出异常，避免在非 RazorVue 项目中产生噪音。

#### 步骤 2: 库组件发现（验证阶段）

```csharp
_ = razorVueContext.DiscoverLibraryComponents();
```

**目的**:
- 在任何消费组件解析库组件之前，先验证仅描述符的库桩
- 确保库组件声明有效（JAZORVGA012-014）
- 与分析器的验证保持一致

**失败处理**: 抛出 `RazorVueCompilationIssueException`，映射到相应的诊断。

#### 步骤 3: 执行 Pipeline

```csharp
var catalog = new RazorVuePipeline(RazorVuePreferredTemplateFrontend.Instance).Execute(compilation);
if (catalog.Artifacts.IsDefaultOrEmpty)
    return;
```

**职责委托**: Generator 不直接处理语义提取和 artifact 降级，而是委托给 `RazorVuePipeline`，同时由宿主显式决定模板前端策略。

**空结果处理**: 如果没有生成 artifacts，静默返回（不生成空文件）。

#### 步骤 4: 生成代码

```csharp
context.AddSource("Jazor.Generated.RazorVueCatalog.g.cs", BuildRazorVueCatalogSource(catalog));
```

**输出文件名**: `Jazor.Generated.RazorVueCatalog.g.cs`
- `.g.cs` 后缀表示生成的代码
- 放置在 `Jazor.Generated` 命名空间

## 5. 错误处理与诊断映射

### 5.1 RazorVueCompilationIssueException 映射

```csharp
private static Diagnostic CreateCompilationIssueDiagnostic(
    RazorVueCompilationIssueException issueException,
    ModuleCandidate? candidate)
{
    var descriptor = issueException.Issue.Code switch
    {
        RazorVueIssueCode.ComponentNotFound => RazorVueComponentNotFound,
        RazorVueIssueCode.AmbiguousComponentName => RazorVueAmbiguousComponentName,
        RazorVueIssueCode.ReservedIntrinsicNameCollision => RazorVueReservedIntrinsicNameCollision,
        RazorVueIssueCode.UnsupportedLifecycleLowering => RazorVueUnsupportedLifecycleLowering,
        RazorVueIssueCode.UnsupportedSetupLogicLowering => RazorVueUnsupportedSetupLogicLowering,
        RazorVueIssueCode.InvalidLibraryComponentDeclaration => RazorVueInvalidLibraryComponentDeclaration,
        RazorVueIssueCode.InvalidLibraryStyleDependencyDeclaration => RazorVueInvalidLibraryStyleDependencyDeclaration,
        RazorVueIssueCode.InvalidLibraryPluginRequirementDeclaration => RazorVueInvalidLibraryPluginRequirementDeclaration,
        RazorVueIssueCode.UnknownParameter => RazorVueUnknownParameter,
        RazorVueIssueCode.InvalidBindTarget => RazorVueInvalidBindTarget,
        RazorVueIssueCode.UnknownSlot => RazorVueUnknownSlot,
        RazorVueIssueCode.SlotContextMisuse => RazorVueSlotContextMisuse,
        RazorVueIssueCode.DuplicateSlotValue => RazorVueDuplicateSlotValue,
        _ => RazorVueGenerationFailed
    };
    var location = TryCreateLocation(issueException.Origin) ?? candidate?.Location ?? Location.None;
    return Diagnostic.Create(descriptor, location, issueException.Issue.Message);
}
```

**映射规则**:
- `RazorVueIssueCode` 枚举值直接映射到 `DiagnosticDescriptor`
- 未知的 issue code 映射到 `RazorVueGenerationFailed`（JAZORVGA001）
- 位置优先级：`issueException.Origin` → `candidate.Location` → `Location.None`

### 5.2 NotSupportedException 特殊处理

```csharp
catch (NotSupportedException ex) when (
    TryCreateUnsupportedSetupLogicIssueException(ex, candidate, out var issueException))
{
    context.ReportDiagnostic(CreateCompilationIssueDiagnostic(issueException!, candidate));
}
```

**场景**: 某些组件方法（如 `Setup` 逻辑）降级失败时抛出 `NotSupportedException`。

**启发式提取**:
```csharp
private static bool TryCreateUnsupportedSetupLogicIssueException(
    NotSupportedException exception,
    ModuleCandidate? candidate,
    out RazorVueCompilationIssueException? issueException)
{
    issueException = null;
    if (candidate?.ClassSymbol is null)
        return false;

    var message = exception.Message;
    if (string.IsNullOrWhiteSpace(message) || !message.Contains("component method", StringComparison.Ordinal))
        return false;

    var methodName = ExtractQuotedIdentifier(message);
    if (string.IsNullOrWhiteSpace(methodName))
        return false;

    var method = candidate.ClassSymbol.GetMembers(methodName!)
        .OfType<IMethodSymbol>()
        .FirstOrDefault(static member => !member.IsStatic);
    if (method is null)
        return false;

    var originLocation = method.Locations.FirstOrDefault(static location => location.IsInSource);
    var origin = originLocation is null
        ? null
        : RazorVueSourceOrigin.FromLocation(originLocation, RazorVueOriginKind.Logic);
    var issue = new RazorVueCompilationIssue(
        RazorVueIssueCode.UnsupportedSetupLogicLowering,
        RazorVueIssueSeverity.Error,
        $"RazorVue setup lowering does not support method '{method.Name}' in component '{method.ContainingType.ToDisplayString()}'.",
        ImmutableArray<string>.Empty);
    issueException = new RazorVueCompilationIssueException(issue, method.ContainingType.ToDisplayString(), origin);
    return true;
}
```

**启发式规则**:
- 异常消息包含 "component method" 字符串
- 从单引号中提取方法名：`'MethodName'`
- 在候选类中查找非静态方法
- 使用方法位置作为诊断位置

### 5.3 Location 创建

```csharp
private static Location? TryCreateLocation(RazorVueSourceOrigin? origin)
{
    if (origin is null || string.IsNullOrWhiteSpace(origin.SourceFilePath))
        return null;

    var startLine = Math.Max(origin.StartLine - 1, 0);
    var startColumn = Math.Max(origin.StartColumn - 1, 0);
    var start = new LinePosition(startLine, startColumn);
    var end = new LinePosition(startLine, startColumn + Math.Max(origin.SourceSpanLength, 1));
    return Location.Create(
        origin.SourceFilePath,
        new TextSpan(Math.Max(origin.SourceSpanStart, 0), Math.Max(origin.SourceSpanLength, 0)),
        new LinePositionSpan(start, end));
}
```

**坐标转换**:
- Roslyn `LinePosition` 从 0 开始，RazorVue 从 1 开始，需要减 1
- 确保非负值（`Math.Max(0, ...)`）
- 使用 `Location.Create()` 创建精确的诊断位置

## 6. 生成代码结构

### 6.1 命名空间与类声明

```csharp
// <auto-generated/>
#nullable enable
namespace Jazor.Generated
{
    [global::System.Runtime.CompilerServices.CompilerGenerated]
    public static partial class RazorVueCatalog
    {
        public static string AssemblyName { get; } = "MyAssembly";

        public static global::System.Collections.IEnumerable GetArtifacts()
        {
            return _artifacts;
        }

        // ... 嵌套类型和 artifacts 数组
    }
}
```

**关键特性**:
- `<auto-generated/>` 注释标识生成的代码
- `#nullable enable` 启用可空引用类型
- `[CompilerGenerated]` 特性标记编译器生成的代码
- `partial class` 允许手动扩展（虽然当前没有其他部分）
- `static class` 所有成员都是静态的

### 6.2 嵌套类型：GeneratedArtifact

```csharp
[global::System.Runtime.CompilerServices.CompilerGenerated]
private sealed class GeneratedArtifact
{
    public GeneratedArtifact(
        string componentName,
        string relativeModulePath,
        string moduleCode,
        string[] imports,
        string[] styles,
        string[] pluginRequirements,
        GeneratedIdentity identity,
        GeneratedHints hints,
        GeneratedOrigin[] sourceOrigins)
    {
        ComponentName = componentName;
        RelativeModulePath = relativeModulePath;
        ModuleCode = moduleCode;
        Imports = imports;
        Styles = styles;
        PluginRequirements = pluginRequirements;
        Identity = identity;
        Hints = hints;
        SourceOrigins = sourceOrigins;
    }

    public string ComponentName { get; }
    public string RelativeModulePath { get; }
    public string ModuleCode { get; }
    public string[] Imports { get; }
    public string[] Styles { get; }
    public string[] PluginRequirements { get; }
    public GeneratedIdentity Identity { get; }
    public GeneratedHints Hints { get; }
    public GeneratedOrigin[] SourceOrigins { get; }
}
```

**属性映射**:
- `ComponentName`: 组件名称（如 `VBtn`）
- `RelativeModulePath`: 模块路径（如 `vbtn.js`）
- `ModuleCode`: 完整的 JavaScript 模块源码
- `Imports`: 导入的模块路径数组
- `Styles`: 样式依赖数组
- `PluginRequirements`: 插件需求数组（如 `"vuetify"`）
- `Identity`: 组件身份信息（ID、哈希等）
- `Hints`: 优化提示（SSR、Hydration 等）
- `SourceOrigins`: 源码映射信息数组

### 6.3 嵌套类型：GeneratedIdentity

```csharp
[global::System.Runtime.CompilerServices.CompilerGenerated]
private sealed class GeneratedIdentity
{
    public GeneratedIdentity(
        string componentId,
        string moduleId,
        string descriptorHash,
        string templateHash,
        string logicHash,
        GeneratedHmrBoundaryKind hmrBoundaryKind)
    {
        ComponentId = componentId;
        ModuleId = moduleId;
        DescriptorHash = descriptorHash;
        TemplateHash = templateHash;
        LogicHash = logicHash;
        HmrBoundaryKind = hmrBoundaryKind;
    }

    public string ComponentId { get; }
    public string ModuleId { get; }
    public string DescriptorHash { get; }
    public string TemplateHash { get; }
    public string LogicHash { get; }
    public GeneratedHmrBoundaryKind HmrBoundaryKind { get; }
}
```

**HMR 边界类型**:
```csharp
private enum GeneratedHmrBoundaryKind
{
    Unknown,           // 未知边界
    TemplateOnly,      // 仅模板热更新
    LogicSafe,         // 逻辑安全热更新
    FullReloadRequired // 需要完全重载
}
```

### 6.4 嵌套类型：GeneratedHints

```csharp
[global::System.Runtime.CompilerServices.CompilerGenerated]
private sealed class GeneratedHints
{
    public GeneratedHints(
        bool requiresVueRuntime,
        bool requiresHydration,
        bool supportsSsr,
        bool usesTeleport,
        bool usesSuspense,
        bool usesKeepAlive)
    {
        RequiresVueRuntime = requiresVueRuntime;
        RequiresHydration = requiresHydration;
        SupportsSsr = supportsSsr;
        UsesTeleport = usesTeleport;
        UsesSuspense = usesSuspense;
        UsesKeepAlive = usesKeepAlive;
    }

    public bool RequiresVueRuntime { get; }
    public bool RequiresHydration { get; }
    public bool SupportsSsr { get; }
    public bool UsesTeleport { get; }
    public bool UsesSuspense { get; }
    public bool UsesKeepAlive { get; }
}
```

**优化提示用途**:
- `RequiresVueRuntime`: 是否需要 Vue 运行时
- `RequiresHydration`: 是否需要水合（SSR）
- `SupportsSsr`: 是否支持服务端渲染
- `UsesTeleport`: 是否使用 Teleport 组件
- `UsesSuspense`: 是否使用 Suspense 组件
- `UsesKeepAlive`: 是否使用 KeepAlive 组件

### 6.5 嵌套类型：GeneratedOrigin

```csharp
[global::System.Runtime.CompilerServices.CompilerGenerated]
private sealed class GeneratedOrigin
{
    public GeneratedOrigin(
        string sourceFilePath,
        int sourceSpanStart,
        int sourceSpanLength,
        string? generatedFilePath,
        int? generatedSpanStart,
        int? generatedSpanLength,
        int startLine,
        int startColumn,
        GeneratedMappingQuality mappingQuality,
        GeneratedOriginProvenance provenance)
    {
        SourceFilePath = sourceFilePath;
        SourceSpanStart = sourceSpanStart;
        SourceSpanLength = sourceSpanLength;
        GeneratedFilePath = generatedFilePath;
        GeneratedSpanStart = generatedSpanStart;
        GeneratedSpanLength = generatedSpanLength;
        StartLine = startLine;
        StartColumn = startColumn;
        MappingQuality = mappingQuality;
        Provenance = provenance;
    }

    public string SourceFilePath { get; }
    public int SourceSpanStart { get; }
    public int SourceSpanLength { get; }
    public string? GeneratedFilePath { get; }
    public int? GeneratedSpanStart { get; }
    public int? GeneratedSpanLength { get; }
    public int StartLine { get; }
    public int StartColumn { get; }
    public GeneratedMappingQuality MappingQuality { get; }
    public GeneratedOriginProvenance Provenance { get; }
}
```

**映射质量**:
```csharp
private enum GeneratedMappingQuality
{
    ExactSource,         // 精确源码映射
    MappedFromGenerated, // 从生成代码映射
    GeneratedOnly        // 仅生成代码
}
```

**溯源类型**:
```csharp
private enum GeneratedOriginProvenance
{
    RazorSourceMap,           // Razor 源码映射
    GeneratedSyntaxLocation,  // 生成语法位置
    GeneratedFallback         // 生成回退位置
}
```

### 6.6 Artifacts 数组生成

```csharp
private static readonly GeneratedArtifact[] _artifacts = new GeneratedArtifact[]
{
    new GeneratedArtifact(
        componentName: "VBtn",
        relativeModulePath: "vbtn.js",
        moduleCode: "export default { ... }",
        imports: new string[] { "vue" },
        styles: new string[] { "vuetify/styles" },
        pluginRequirements: new string[] { "vuetify" },
        identity: new GeneratedIdentity(
            componentId: "vbtn",
            moduleId: "vbtn-module",
            descriptorHash: "abc123",
            templateHash: "def456",
            logicHash: "ghi789",
            hmrBoundaryKind: GeneratedHmrBoundaryKind.LogicSafe),
        hints: new GeneratedHints(
            requiresVueRuntime: true,
            requiresHydration: false,
            supportsSsr: true,
            usesTeleport: false,
            usesSuspense: false,
            usesKeepAlive: false),
        sourceOrigins: new GeneratedOrigin[] {
            new GeneratedOrigin(
                sourceFilePath: "VBtn.razor",
                sourceSpanStart: 0,
                sourceSpanLength: 100,
                generatedFilePath: null,
                generatedSpanStart: null,
                generatedSpanLength: null,
                startLine: 1,
                startColumn: 1,
                mappingQuality: GeneratedMappingQuality.ExactSource,
                provenance: GeneratedOriginProvenance.RazorSourceMap)
        }),
    // ... 更多 artifacts
};
```

## 7. 辅助方法

### 7.1 字符串转义

```csharp
private static string EscapeCSharpString(string value)
{
    var builder = new StringBuilder((value ?? string.Empty).Length + 2);
    builder.Append('"');
    foreach (var ch in value ?? string.Empty)
    {
        builder.Append(ch switch
        {
            '\\' => "\\\\",
            '"' => "\\\"",
            '\0' => "\\0",
            '\a' => "\\a",
            '\b' => "\\b",
            '\f' => "\\f",
            '\n' => "\\n",
            '\r' => "\\r",
            '\t' => "\\t",
            '\v' => "\\v",
            _ => ch.ToString()
        });
    }

    builder.Append('"');
    return builder.ToString();
}
```

**转义字符**:
- 反斜杠：`\\`
- 双引号：`\"`
- 控制字符：`\0`, `\a`, `\b`, `\f`, `\n`, `\r`, `\t`, `\v`

### 7.2 数组字面量构建

```csharp
private static string BuildStringArrayLiteral(ImmutableArray<string> values)
{
    if (values.IsDefaultOrEmpty)
        return "new string[0]";

    var builder = new StringBuilder("new string[] { ");
    for (var i = 0; i < values.Length; i++)
    {
        if (i > 0)
            builder.Append(", ");
        builder.Append(EscapeCSharpString(values[i]));
    }

    builder.Append(" }");
    return builder.ToString();
}
```

**示例**:
- 空：`new string[0]`
- 非空：`new string[] { "vue", "vuetify/components" }`

### 7.3 Origins 数组构建

```csharp
private static string BuildOriginsArrayLiteral(ImmutableArray<RazorVueSourceOrigin> origins)
{
    if (origins.IsDefaultOrEmpty)
        return "new GeneratedOrigin[0]";

    var builder = new StringBuilder();
    builder.AppendLine("new GeneratedOrigin[]");
    builder.AppendLine("                {");
    foreach (var origin in origins)
    {
        builder.AppendLine("                    new GeneratedOrigin(");
        builder.Append("                        sourceFilePath: ").Append(EscapeCSharpString(origin.SourceFilePath)).AppendLine(",");
        builder.Append("                        sourceSpanStart: ").Append(origin.SourceSpanStart).AppendLine(",");
        // ... 更多属性
        builder.Append("                        provenance: GeneratedOriginProvenance.").Append(origin.Provenance).AppendLine("),");
    }

    builder.Append("                }");
    return builder.ToString();
}
```

### 7.4 布尔和可空整数转换

```csharp
private static string ToCSharpBool(bool value)
    => value ? "true" : "false";

private static string ToNullableCSharpInt(int? value)
    => value?.ToString() ?? "null";

private static string EscapeNullableCSharpString(string? value)
    => value is null ? "null" : EscapeCSharpString(value);
```

## 8. 增量生成优化

### 8.1 增量管道

Generator 使用 Roslyn 的增量生成器 API：

```csharp
var componentCandidates = context.SyntaxProvider.ForAttributeWithMetadataName(
    fullyQualifiedMetadataName: "ECMAScript.ECMAScriptModuleAttribute",
    predicate: static (node, _) => node is ClassDeclarationSyntax,
    transform: static (syntaxContext, _) => CreateCandidate(syntaxContext))
    .Where(static candidate => candidate is not null);

var combined = context.CompilationProvider.Combine(componentCandidates.Collect());
context.RegisterSourceOutput(combined, static (outputContext, source) => { ... });
```

**增量优化**:
- `ForAttributeWithMetadataName`: 仅处理带有特定特性的节点
- `predicate`: 快速过滤语法节点（在 `transform` 之前）
- `transform`: 创建轻量级的 `ModuleCandidate` 记录
- `Collect():` 缓存候选列表，避免重复计算

### 8.2 缓存策略

| 缓存级别 | 缓存内容 | 失效条件 |
|---------|---------|---------|
| 语法节点 | `ClassDeclarationSyntax` | 语法树变更 |
| 符号 | `INamedTypeSymbol` | 类型定义变更 |
| 候选列表 | `ImmutableArray<ModuleCandidate?>` | 特性使用变更 |
| 编译 | `Compilation` | 项目引用变更 |

## 9. 使用场景

### 9.1 标准 RazorVue 项目

```csharp
// MyComponent.razor
[ECMAScriptModule]
public class MyComponent : ComponentBase
{
    [Parameter] public string? Text { get; set; }
}
```

**生成结果**:
- 发现候选：`MyComponent`
- 执行 pipeline
- 生成 `Jazor.Generated.RazorVueCatalog.g.cs`

### 9.2 混合项目（RazorVue + 普通组件）

```csharp
// 只有标记 [ECMAScriptModule] 的组件会被发现
[ECMAScriptModule]
public class RazorVueComponent : ComponentBase { }

public class RegularComponent : ComponentBase { }
```

**生成结果**:
- 仅包含 `RazorVueComponent`
- `RegularComponent` 被忽略

### 9.3 多入口点项目

```csharp
// Entry1.razor
[ECMAScriptModule]
public class Entry1 : ComponentBase { }

// Entry2.razor
[ECMAScriptModule]
public class Entry2 : ComponentBase { }
```

**生成结果**:
- `Jazor.Generated.RazorVueCatalog.g.cs` 包含两个组件
- `_artifacts` 数组长度为 2

## 10. 错误场景

### 10.1 库组件声明无效

```csharp
[VueLibraryComponent("invalid/path", "")]
public class InvalidComponent : ComponentBase, IVueLibraryComponent { }
```

**诊断**:
```
JAZORVGA012: RazorVue library component declaration is invalid
```

### 10.2 组件名称冲突

```csharp
[ECMAScriptModule]
public class MyComponent : ComponentBase { }

[ECMAScriptModule]
public class MyComponent : ComponentBase { } // 重复名称
```

**诊断**:
```
JAZORVGA003: RazorVue component name is ambiguous
```

### 10.3 Setup 逻辑不支持

```csharp
[ECMAScriptModule]
public class MyComponent : ComponentBase
{
    protected override void Setup() // 不支持的方法
    {
        // ...
    }
}
```

**诊断**:
```
JAZORVGA006: RazorVue setup logic lowering is unsupported
RazorVue setup lowering does not support method 'Setup' in component 'MyComponent'.
```

## 11. 相关文件

| 文件 | 职责 |
|------|------|
| `src/Jazor.Analyzer/RazorVue/Generation/RazorVueGenerator.cs` | Generator 主类 |
| `src/Jazor.RazorVue/RazorVuePipeline.cs` | Pipeline 执行 |
| `src/Jazor.RazorVue/Artifacts/RazorVueCompilationContext.cs` | 编译上下文 |
| `src/Jazor.RazorVue/Artifacts/RazorVueCatalog.cs` | 目录数据结构 |
| `src/Jazor.RazorVue/Artifacts/VueCompiledArtifact.cs` | Artifact 数据结构 |
| `src/Jazor.RazorVue/Artifacts/RazorVueCompilationIssue.cs` | Issue 定义 |
| `src/Jazor.RazorVue/Artifacts/RazorVueSourceOrigin.cs` | 源码映射 |

---

**文档维护者**: developerhan
**最后更新**: 2026-04-21
**文档版本**: v1.0
