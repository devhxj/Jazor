# 三车道架构 (Three-Lane Architecture)

> Status: 活跃参考
> Positioning: Jolt LSP 的多语言服务提供层

## 1. 文档定位

本文档描述 Jolt LSP 的三车道架构，用于协调 Jazor、C# (Roslyn) 和 Vue (Volar) 三种语言服务的提供。

## 目录

- [1-文档定位](#1-文档定位)
- [2-核心类型](#2-核心类型)
- [3-jazorlaneservice](#3-jazorlaneservice)
- [4-roslynlaneservice](#4-roslynlaneservice)
- [5-volarlaneservice](#5-volarlaneservice)
- [6-核心算法](#6-核心算法)
- [7-线程安全模型](#7-线程安全模型)
- [8-错误处理](#8-错误处理)
- [9-配置选项](#9-配置选项)
- [10-与其他子系统的交互](#10-与其他子系统的交互)
- [11-设计权衡](#11-设计权衡)

**相关文件**：
- `src/Jolt/Lsp/Lanes/ILspLane.cs` (96行) - 车道接口定义
- `src/Jolt/Lsp/Lanes/JazorLaneService.cs` (103行) - Jazor 车道实现
- `src/Jolt/Lsp/Lanes/RoslynLaneService.cs` (434行) - C# 车道实现
- `src/Jolt/Lsp/Lanes/VolarLaneService.cs` (1594行) - Vue 车道实现

## 2. 核心类型

### 2.1 ILspLane 接口

**文件位置**：`src/Jolt/Lsp/Lanes/ILspLane.cs`

**职责**：定义所有语言服务提供者必须实现的契约

**核心方法**（14个）：

```csharp
public interface ILspLane
{
    LaneKind LaneKind { get; }

    // 诊断和语言特性
    ValueTask<IReadOnlyList<LspDiagnostic>> GetDiagnosticsAsync(
        DocumentSnapshot document, CancellationToken cancellationToken);

    ValueTask<LspHoverResult?> GetHoverAsync(
        DocumentSnapshot document, LspPosition position,
        ProjectionTarget projectionTarget, CancellationToken cancellationToken);

    ValueTask<IReadOnlyList<LspDocumentHighlight>> GetDocumentHighlightsAsync(
        DocumentSnapshot document, LspPosition position,
        ProjectionTarget projectionTarget, CancellationToken cancellationToken);

    ValueTask<IReadOnlyList<LspDocumentLink>> GetDocumentLinksAsync(
        DocumentSnapshot document, CancellationToken cancellationToken);

    // 补全和符号
    ValueTask<IReadOnlyList<LspCompletionItem>> GetCompletionItemsAsync(
        DocumentSnapshot document, LspPosition position,
        ProjectionTarget projectionTarget, CancellationToken cancellationToken);

    ValueTask<IReadOnlyList<LspDocumentSymbol>> GetDocumentSymbolsAsync(
        DocumentSnapshot document, CancellationToken cancellationToken);

    ValueTask<IReadOnlyList<LspSemanticToken>> GetSemanticTokensAsync(
        DocumentSnapshot document, CancellationToken cancellationToken);

    ValueTask<LspSignatureHelp?> GetSignatureHelpAsync(
        DocumentSnapshot document, LspPosition position,
        ProjectionTarget projectionTarget, CancellationToken cancellationToken);

    // 导航
    ValueTask<IReadOnlyList<LspLocation>> GetDefinitionAsync(
        DocumentSnapshot document, LspPosition position,
        ProjectionTarget projectionTarget, CancellationToken cancellationToken);

    ValueTask<IReadOnlyList<LspLocation>> GetImplementationAsync(
        DocumentSnapshot document, LspPosition position,
        ProjectionTarget projectionTarget, CancellationToken cancellationToken);

    ValueTask<IReadOnlyList<LspLocation>> GetReferencesAsync(
        DocumentSnapshot document, LspPosition position,
        bool includeDeclaration, ProjectionTarget projectionTarget,
        CancellationToken cancellationToken);

    // 重构
    ValueTask<LspWorkspaceEdit?> GetRenameAsync(
        DocumentSnapshot document, LspPosition position, string newName,
        ProjectionTarget projectionTarget, CancellationToken cancellationToken);

    ValueTask<IReadOnlyList<LspCodeAction>> GetCodeActionsAsync(
        DocumentSnapshot document, LspRange range,
        IReadOnlyList<LspDiagnostic> diagnostics,
        ProjectionTarget projectionTarget, CancellationToken cancellationToken);
}
```

**设计特点**：
- 所有方法返回 `ValueTask` 以减少异步开销
- 接受 `ProjectionTarget` 参数以支持投影文档
- 返回不可变集合以确保线程安全

## 3. JazorLaneService

**文件位置**：`src/Jolt/Lsp/Lanes/JazorLaneService.cs` (103行)

**职责**：提供 Jazor 特定的语言服务，委托给 `JazorLspDocumentService`

### 3.1 实现

```csharp
internal sealed class JazorLaneService : ILspLane
{
    private readonly JazorLspDocumentService _documentService;

    public JazorLaneService(JazorLspDocumentService documentService)
    {
        _documentService = documentService ?? throw new ArgumentNullException(nameof(documentService));
    }

    public LaneKind LaneKind => LaneKind.Jazor;

    // 所有方法直接委托给 _documentService
    public ValueTask<IReadOnlyList<LspDiagnostic>> GetDiagnosticsAsync(
        DocumentSnapshot document, CancellationToken cancellationToken)
        => _documentService.GetDiagnosticsAsync(document, cancellationToken);

    public ValueTask<LspHoverResult?> GetHoverAsync(
        DocumentSnapshot document, LspPosition position,
        ProjectionTarget projectionTarget, CancellationToken cancellationToken)
        => _documentService.GetHoverAsync(document, position, cancellationToken);

    // ... 其他方法类似
}
```

### 3.2 特性

**提供的能力**：
- **诊断**：Jazor 编译器诊断（语法错误、类型错误）
- **补全**：组件标签补全（基于工作区扫描）
- **定义**：跳转到组件定义（Vue 文件）
- **引用**：查找组件标签的所有使用
- **重命名**：跨文档重命名组件
- **文档链接**：`@module` 导入链接
- **语义高亮**：`<template>`、`@code`、`@module`、组件标签
- **文档符号**：Template 和 Code 区域符号

**不支持的能力**：
- SignatureHelp（返回 null）
- Implementation（返回空数组）

## 4. RoslynLaneService

**文件位置**：`src/Jolt/Lsp/Lanes/RoslynLaneService.cs` (434行)

**职责**：提供 C# 代码的语言服务，使用 `InProcRoslynCodeService`

### 4.1 实现

```csharp
internal sealed class RoslynLaneService : ILspLane
{
    private readonly IJoltWorkspaceStore _workspaceStore;
    private readonly InProcRoslynCodeService _inProcCodeService;

    public RoslynLaneService(
        IJoltWorkspaceStore workspaceStore,
        InProcRoslynCodeService? inProcCodeService = null)
    {
        _workspaceStore = workspaceStore ?? throw new ArgumentNullException(nameof(workspaceStore));
        _inProcCodeService = inProcCodeService ?? new InProcRoslynCodeService();
    }

    public LaneKind LaneKind => LaneKind.Roslyn;
}
```

### 4.2 工作区感知模式

**目的**：利用打开的文档提供更准确的结果

**实现**（以 `GetHoverAsync` 为例）：
```csharp
public async ValueTask<LspHoverResult?> GetHoverAsync(
    DocumentSnapshot document,
    LspPosition position,
    ProjectionTarget projectionTarget,
    CancellationToken cancellationToken)
{
    if (!IsCodeTarget(projectionTarget))
    {
        return null;
    }

    var openDocuments = await _workspaceStore.GetOpenDocumentsAsync(cancellationToken);
    var inProcResult = await _inProcCodeService.GetHoverAsync(
        document, position, openDocuments, cancellationToken);
    if (inProcResult is null)
    {
        // 回退到无工作区模式
        inProcResult = await _inProcCodeService.GetHoverAsync(
            document, position, cancellationToken);
    }

    return inProcResult;
}
```

**目标检查**：
```csharp
private static bool IsCodeTarget(ProjectionTarget projectionTarget)
    => projectionTarget.LaneKind == LaneKind.Roslyn
        || projectionTarget.RegionKind == DocumentRegionKind.Code;
```

### 4.3 额外能力

**TypeDefinition**（内部方法）：
```csharp
internal async ValueTask<IReadOnlyList<LspLocation>> GetTypeDefinitionAsync(
    DocumentSnapshot document,
    LspPosition position,
    CancellationToken cancellationToken)
{
    if (document.DocumentKind is not (DocumentKind.Jazor or DocumentKind.CSharp))
    {
        return Array.Empty<LspLocation>();
    }

    var openDocuments = await _workspaceStore.GetOpenDocumentsAsync(cancellationToken);
    var inProcResult = await _inProcCodeService.GetTypeDefinitionAsync(
        document, position, openDocuments, cancellationToken);
    if (inProcResult.Count == 0)
    {
        inProcResult = await _inProcCodeService.GetTypeDefinitionAsync(
            document, position, cancellationToken);
    }

    return inProcResult.Count > 0 ? inProcResult : Array.Empty<LspLocation>();
}
```

**CallHierarchy**（内部方法）：
- `PrepareCallHierarchyAsync`：准备调用层次结构
- `GetIncomingCallsAsync`：获取传入调用
- `GetOutgoingCallsAsync`：获取传出调用

**TypeHierarchy**（内部方法）：
- `PrepareTypeHierarchyAsync`：准备类型层次结构
- `GetTypeHierarchySuperTypesAsync`：获取父类型
- `GetTypeHierarchySubTypesAsync`：获取子类型

### 4.4 特性

**提供的能力**：
- **完整的 C# 语言服务**：诊断、补全、定义、引用、重命名、签名帮助等
- **工作区感知**：利用打开文档提供跨文件分析
- **语义分析**：完整的 C# 语义理解
- **层次结构**：类型和调用层次结构

**仅 Code 区域**：所有操作都需要 `IsCodeTarget` 检查

## 5. VolarLaneService

**文件位置**：`src/Jolt/Lsp/Lanes/VolarLaneService.cs` (1594行)

**职责**：提供 Vue/JavaScript/TypeScript 语言服务，使用 `IDenoVolarHost`

### 5.1 依赖

```csharp
internal sealed class VolarLaneService : ILspLane
{
    private readonly IFrontendContextProvider? _frontendContextProvider;
    private readonly IVirtualDocumentRegistry? _virtualDocumentRegistry;
    private readonly IDenoVolarHost? _denoVolarHost;
    private readonly MarkupComponentBridgeService _markupComponentBridge;

    public VolarLaneService(
        IJoltWorkspaceStore workspaceStore,
        IFrontendContextProvider? frontendContextProvider = null,
        IVirtualDocumentRegistry? virtualDocumentRegistry = null,
        IDenoVolarHost? denoVolarHost = null,
        MarkupComponentBridgeService? markupComponentBridge = null)
    {
        _frontendContextProvider = frontendContextProvider;
        _virtualDocumentRegistry = virtualDocumentRegistry;
        _denoVolarHost = denoVolarHost;
        _markupComponentBridge = markupComponentBridge ?? new MarkupComponentBridgeService(workspaceStore);
    }
}
```

### 5.2 投影文档解析

**文件位置**：`src/Jolt/Lsp/Lanes/VolarLaneService.cs:825-865`

**目的**：将源文档映射到投影的 Vue 文档

```csharp
private async ValueTask<VolarRequestDocument> ResolveFrontendDocumentAsync(
    DocumentSnapshot sourceDocument,
    ProjectionTarget? projectionTarget,
    CancellationToken cancellationToken)
{
    if (_virtualDocumentRegistry is not null)
    {
        VirtualDocument? projectedDocument = null;
        if (projectionTarget is not null
            && !string.IsNullOrWhiteSpace(projectionTarget.ProjectedDocumentPath))
        {
            projectedDocument = await _virtualDocumentRegistry.GetByProjectedDocumentAsync(
                projectionTarget.ProjectedDocumentPath,
                cancellationToken);
        }

        if (projectedDocument is null && sourceDocument.DocumentKind == DocumentKind.Jazor)
        {
            var virtualDocuments = await _virtualDocumentRegistry.GetBySourceDocumentAsync(
                sourceDocument.DocumentPath,
                cancellationToken);
            projectedDocument = FindPrimaryVueProjection(
                sourceDocument.DocumentPath,
                projectionTarget?.ProjectedDocumentPath,
                virtualDocuments);
        }

        if (projectedDocument is not null)
        {
            return new VolarRequestDocument(
                new DocumentSnapshot(
                    projectedDocument.Identity.ProjectedDocumentPath,
                    MapProjectedDocumentKind(projectedDocument.Identity.DocumentKind),
                    projectedDocument.Text,
                    projectedDocument.Version),
                projectedDocument.ProjectionMap);
        }

        return new VolarRequestDocument(sourceDocument, ProjectionMap: null);
    }
}
```

### 5.3 位置映射

**文件位置**：`src/Jolt/Lsp/Lanes/VolarLaneService.cs:969-1007`

**目的**：将投影 Vue 文档中的位置映射回原始 Jazor 源文档

```csharp
private static IReadOnlyList<LspLocation> MapLocations(
    DocumentSnapshot sourceDocument,
    VolarRequestDocument requestDocument,
    IReadOnlyList<LspLocation> locations)
{
    if (requestDocument.ProjectionMap is null)
    {
        return NormalizeLocationUris(locations);
    }

    var projectedUri = LspProtocolHelpers.ToDocumentUri(requestDocument.RequestDocument.DocumentPath);
    var projectedPath = NormalizePath(requestDocument.RequestDocument.DocumentPath);
    return NormalizeLocationUris(locations
        .Select(location =>
        {
            if (!LocationTargetsProjectedDocument(location.Uri, projectedUri, projectedPath))
            {
                return location;
            }

            if (!requestDocument.ProjectionMap.TryMapToOriginalRange(
                    requestDocument.RequestDocument.Text,
                    location.Range,
                    sourceDocument.Text,
                    out var sourceRange))
            {
                return null;
            }

            return new LspLocation
            {
                Uri = LspProtocolHelpers.ToDocumentUri(sourceDocument.DocumentPath),
                Range = sourceRange
            };
        })
        .Where(location => location is not null)
        .Cast<LspLocation>()
        .ToArray());
}
```

### 5.4 组件标签补全

**文件位置**：`src/Jolt/Lsp/Lanes/VolarLaneService.cs:117-169`

**目的**：提供 Vue 组件标签补全，包括工作区组件

```csharp
public async ValueTask<IReadOnlyList<LspCompletionItem>> GetCompletionItemsAsync(
    DocumentSnapshot document,
    LspPosition position,
    ProjectionTarget projectionTarget,
    CancellationToken cancellationToken)
{
    if (!IsTemplateTarget(projectionTarget))
    {
        return Array.Empty<LspCompletionItem>();
    }

    var frontendDocument = await ResolveFrontendDocumentAsync(document, projectionTarget, cancellationToken);
    var requestPosition = frontendDocument.MapPosition(position, projectionTarget.ProjectedPosition);
    var frontendContext = await GetVolarIntelliSenseContextAsync(document, cancellationToken);
    var items = new List<LspCompletionItem>();
    var seen = new HashSet<string>(StringComparer.Ordinal);

    // Volar 提供的补全
    foreach (var item in await TryGetDenoCompletionItemsAsync(
        frontendDocument.RequestDocument, requestPosition, frontendContext, cancellationToken))
    {
        if (seen.Add($"{item.Label}|{item.Kind}|{item.Detail}"))
        {
            items.Add(item);
        }
    }

    // 工作区组件补全
    if (CanUseWorkspaceGraph()
        && TryGetTagCompletionPrefix(document.Text, position, out var tagPrefix))
    {
        foreach (var component in await _markupComponentBridge.GetComponentSuggestionsAsync(
                 document.DocumentPath,
                 allowWorkspaceScan: document.DocumentKind == DocumentKind.Jazor,
                 cancellationToken))
        {
            if (!component.ComponentName.StartsWith(tagPrefix, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            var item = new LspCompletionItem
            {
                Label = component.ComponentName,
                Kind = 7,
                Detail = component.ImportPath,
                Documentation = $"Vue component available in the workspace graph at `{component.ImportPath}`."
            };
            if (seen.Add($"{item.Label}|{item.Kind}|{item.Detail}"))
            {
                items.Add(item);
            }
        }
    }

    return items;
}
```

### 5.5 诊断增强

**文件位置**：`src/Jolt/Lsp/Lanes/VolarLaneService.cs:1319-1391`

**目的**：检测未解析的组件标签并添加诊断

```csharp
private async ValueTask<IReadOnlyList<LspDiagnostic>> CreateUnresolvedMarkupComponentDiagnosticsAsync(
    DocumentSnapshot document,
    CancellationToken cancellationToken)
{
    var diagnostics = new List<LspDiagnostic>();
    foreach (Match match in JazorMarkupPatterns.ComponentTagPattern.Matches(document.Text))
    {
        cancellationToken.ThrowIfCancellationRequested();

        var group = match.Groups["name"];
        if (!group.Success)
        {
            continue;
        }

        var isResolvable = await _markupComponentBridge.ResolveComponentAsync(
                document.DocumentPath,
                group.Value,
                allowWorkspaceScan: document.DocumentKind == DocumentKind.Jazor,
                cancellationToken)
            is not null;
        if (isResolvable)
        {
            continue;
        }

        diagnostics.Add(new LspDiagnostic
        {
            Range = new LspRange
            {
                Start = LspProtocolHelpers.GetPosition(document.Text, group.Index),
                End = LspProtocolHelpers.GetPosition(document.Text, group.Index + group.Length)
            },
            Severity = DiagnosticSeverityWarning,
            Code = MissingTemplateImportDiagnosticCode,
            Source = "Jolt.Frontend",
            Message = $"Razor component '{group.Value}' could not be resolved to a nearby Vue file."
        });
    }

    return diagnostics;
}
```

### 5.6 失败快照

**文件位置**：`src/Jolt/Lsp/Lanes/VolarLaneService.cs:44-50, 1393-1443`

**目的**：记录 Deno 失败以供诊断

```csharp
private static void RecordDenoFailure(
    string operation,
    string documentPath,
    Exception exception)
{
    var timestamp = DateTimeOffset.UtcNow;
    var snapshot = DenoFailureSnapshots.AddOrUpdate(
        operation,
        static (op, state) => new DenoFailureSnapshot(
            Operation: op,
            FailureCount: 1,
            LastFailureAt: state.Timestamp,
            LastErrorType: state.Exception.GetType().FullName ?? state.Exception.GetType().Name,
            LastErrorMessage: state.Exception.Message),
        static (op, current, state) => current with
        {
            FailureCount = current.FailureCount + 1,
            LastFailureAt = state.Timestamp,
            LastErrorType = state.Exception.GetType().FullName ?? state.Exception.GetType().Name,
            LastErrorMessage = state.Exception.Message
        },
        (Timestamp: timestamp, Exception: exception));
    TrimDenoFailureSnapshots();

    var payload = new
    {
        eventType = "volarDenoLaneDegraded",
        operation,
        documentPath,
        failureCount = snapshot.FailureCount,
        errorType = snapshot.LastErrorType,
        message = snapshot.LastErrorMessage,
        timestamp = snapshot.LastFailureAt
    };
    Console.Error.WriteLine(JsonSerializer.Serialize(payload));
}
```

### 5.7 特性

**提供的能力**：
- **Vue 语言服务**：通过 Deno Volar Host 提供完整的 Vue 智能感知
- **投影映射**：将投影 Vue 文档结果映射回原始 Jazor 文档
- **组件标签补全**：包括工作区扫描的 Vue 组件
- **未解析组件诊断**：检测无法解析的组件标签
- **失败观察**：记录 Deno 失败以供调试

**仅 Template 区域**：大多数操作需要 `IsTemplateTarget` 检查

## 6. 核心算法

### 6.1 投影位置映射

**文件位置**：`src/Jolt/Lsp/Lanes/VolarLaneService.cs:1492-1500`

**算法**：
```csharp
private readonly record struct VolarRequestDocument(
    DocumentSnapshot RequestDocument,
    ProjectionMap? ProjectionMap)
{
    public LspPosition MapPosition(LspPosition sourcePosition, LspPosition? projectedPosition)
        => ProjectionMap is null
            ? sourcePosition
            : projectedPosition ?? sourcePosition;
}
```

**逻辑**：
- 如果没有投影映射：使用源位置
- 如果有投影映射：使用投影位置（从 `ProjectionTarget` 获取）

### 6.2 URI 规范化

**文件位置**：`src/Jolt/Lsp/Lanes/VolarLaneService.cs:1566-1592`

**目的**：确保文件 URI 的一致性（Windows 驱动器字母大小写）

```csharp
private static string NormalizeFileUri(string uri)
{
    if (!Uri.TryCreate(uri, UriKind.Absolute, out var parsed) || !parsed.IsFile)
    {
        return uri;
    }

    var localPath = parsed.LocalPath;
    if (localPath.Length >= 2 && localPath[1] == ':')
    {
        localPath = char.ToUpperInvariant(localPath[0]) + localPath[1..];
    }

    return new Uri(localPath).AbsoluteUri;
}
```

## 7. 线程安全模型

### 7.1 VolarLaneService

**DenofailureSnapshots**：使用 `ConcurrentDictionary` 保证线程安全

```csharp
private static readonly ConcurrentDictionary<string, DenoFailureSnapshot> DenoFailureSnapshots =
    new(StringComparer.OrdinalIgnoreCase);
```

**无状态方法**：所有公共方法都是无状态的，线程安全

### 7.2 其他车道

**无共享状态**：所有字段都是只读或无状态的，线程安全

## 8. 错误处理

### 8.1 Volar Deno 失败

**策略**：捕获所有异常，返回回退值，记录失败

```csharp
private async ValueTask<T> ExecuteDenoRequestAsync<T>(
    DocumentSnapshot document,
    string operation,
    T fallbackValue,
    Func<IDenoVolarHost, CancellationToken, ValueTask<T>> requestAsync,
    CancellationToken cancellationToken)
{
    var denoHost = _denoVolarHost;
    if (denoHost is null)
    {
        return fallbackValue;
    }

    return await ExecuteWithFailureCaptureAsync(
        operation,
        document.DocumentPath,
        fallbackValue,
        token => requestAsync(denoHost, token),
        cancellationToken);
}
```

**失败记录**：
- 输出到 stderr（可观察性）
- 更新 `DenoFailureSnapshots`（诊断）
- 不抛出异常（保持服务可用）

### 8.2 其他车道

**异常传播**：直接传播异常，由上层处理

## 9. 配置选项

### 9.1 构造函数参数

**JazorLaneService**：
- `JazorLspDocumentService documentService`（必需）

**RoslynLaneService**：
- `IJoltWorkspaceStore workspaceStore`（必需）
- `InProcRoslynCodeService? inProcCodeService`（可选，默认新建）

**VolarLaneService**：
- `IJoltWorkspaceStore workspaceStore`（必需）
- `IFrontendContextProvider? frontendContextProvider`（可选）
- `IVirtualDocumentRegistry? virtualDocumentRegistry`（可选）
- `IDenoVolarHost? denoVolarHost`（可选）
- `MarkupComponentBridgeService? markupComponentBridge`（可选，默认新建）

## 10. 与其他子系统的交互

### 10.1 与虚拟文档注册表交互

**VolarLaneService**：
- 获取投影文档（Jazor → Vue）
- 使用 `ProjectionMap` 映射位置

**其他车道**：不使用虚拟文档

### 10.2 与工作区存储交互

**RoslynLaneService**：
- 获取打开文档列表（工作区感知模式）

**VolarLaneService**：
- 通过 `MarkupComponentBridgeService` 间接使用

### 10.3 与组件桥接服务交互

**VolarLaneService**：
- 组件标签补全
- 未解析组件诊断
- 工作区组件扫描

**其他车道**：不使用组件桥接

## 11. 设计权衡

### 11.1 三车道 vs 单车道

**选择**：三车道架构

**原因**：
- Jazor 文档包含三种语言：Jazor、C#、Vue
- 每种语言需要专门的分析器
- 分离关注点，易于维护

**权衡**：
- 优势：专业化的分析器，更好的结果
- 劣势：复杂的协调和结果聚合

### 11.2 Roslyn 工作区感知 vs 非感知

**选择**：工作区感知模式优先

**原因**：
- 打开文档提供更准确的语义分析
- 支持跨文件引用（如 using 指令）
- 回退到非感知模式确保兼容性

**权衡**：
- 优势：更准确的智能感知
- 劣势：额外的异步开销

### 11.3 Volar 投影映射 vs 直接映射

**选择**：使用 `ProjectionMap` 进行位置映射

**原因**：
- Jazor → Vue 投影可能改变行号
- 需要准确的位置映射以支持跳转和诊断
- `ProjectionMap` 提供双向映射

**权衡**：
- 优势：准确的位置映射
- 劣势：额外的计算和内存开销

### 11.4 Deno 失败记录 vs 静默失败

**选择**：记录失败到 stderr 和内存快照

**原因**：
- 便于调试和监控
- 不影响 LSP 协议流
- 提供失败历史以供诊断

**权衡**：
- 优势：可观察性
- 劣势：可能的性能影响（最小化）
