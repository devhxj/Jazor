# LSP 协调 (Coordination)

> 状态：已实现
> 定位：Jolt LSP 的跨车道查询协调层

## 1. 文档定位

本文档描述 Jolt LSP 的协调器模式，用于协调多个车道的查询结果，特别是跨车道的引用、重命名和代码操作。

**相关文件**：
- `src/Jolt/Lsp/Coordination/ReferenceCoordinator.cs` (60行) - 引用协调器
- `src/Jolt/Lsp/Coordination/RenameCoordinator.cs` (60行) - 重命名协调器
- `src/Jolt/Lsp/Coordination/CodeActionCoordinator.cs` (85行) - 代码操作协调器
- `src/Jolt/Lsp/Coordination/MarkupBridgeFanoutCoordinator.cs` (142行) - 标记桥接扇出协调器
- `src/Jolt/Lsp/Coordination/MarkupComponentBridgeService.cs` (842行) - 标记组件桥接服务

## 2. 核心类型

### 2.1 ReferenceCoordinator

**文件位置**：`src/Jolt/Lsp/Coordination/ReferenceCoordinator.cs`

**职责**：协调跨车道的引用查询

**核心方法**：
```csharp
public async ValueTask<IReadOnlyList<LspLocation>> CoordinateAsync(
    DocumentSnapshot document,
    LspPosition position,
    bool includeDeclaration,
    ProjectionTarget projectionTarget,
    CancellationToken cancellationToken)
{
    var locations = new List<LspLocation>();
    foreach (var laneKind in _laneRouter.GetOrderedLanes(projectionTarget))
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (!_lanes.TryGetValue(laneKind, out var lane))
        {
            continue;
        }

        var laneLocations = await lane.GetReferencesAsync(
            document,
            position,
            includeDeclaration,
            projectionTarget,
            cancellationToken);
        if (laneLocations.Count > 0)
        {
            locations.AddRange(laneLocations);
        }
    }

    return await _markupBridgeFanout.CoordinateReferencesAsync(
        document,
        position,
        includeDeclaration,
        locations,
        cancellationToken);
}
```

**协调策略**：
1. 遍历所有车道（按路由器顺序）
2. 每个车道独立查询引用
3. 收集所有车道的引用
4. 扇出到标记桥接服务（查找 Jazor 文档中的组件标签引用）
5. 聚合和去重结果

### 2.2 RenameCoordinator

**文件位置**：`src/Jolt/Lsp/Coordination/RenameCoordinator.cs`

**职责**：协调跨车道的重命名操作

**核心方法**：
```csharp
public async ValueTask<LspWorkspaceEdit?> CoordinateAsync(
    DocumentSnapshot document,
    LspPosition position,
    string newName,
    ProjectionTarget projectionTarget,
    CancellationToken cancellationToken)
{
    var edits = new List<LspWorkspaceEdit>();
    foreach (var laneKind in _laneRouter.GetOrderedLanes(projectionTarget))
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (!_lanes.TryGetValue(laneKind, out var lane))
        {
            continue;
        }

        var edit = await lane.GetRenameAsync(
            document,
            position,
            newName,
            projectionTarget,
            cancellationToken);
        if (edit is not null)
        {
            edits.Add(edit);
        }
    }

    return await _markupBridgeFanout.CoordinateRenameAsync(
        document,
        position,
        newName,
        edits.Count == 0
            ? null
            : _resultAggregator.AggregateWorkspaceEdits(edits),
        cancellationToken);
}
```

**协调策略**：
1. 遍历所有车道（按路由器顺序）
2. 每个车道独立生成重命名编辑
3. 收集所有车道的编辑
4. 聚合工作区编辑
5. 扇出到标记桥接服务（查找 Jazor 文档中的组件标签重命名）
6. 再次聚合所有编辑

### 2.3 CodeActionCoordinator

**文件位置**：`src/Jolt/Lsp/Coordination/CodeActionCoordinator.cs`

**职责**：协调跨车道的代码操作查询

**核心方法**：
```csharp
public async ValueTask<IReadOnlyList<LspCodeAction>> CoordinateAsync(
    DocumentSnapshot document,
    LspRange range,
    IReadOnlyList<LspDiagnostic> diagnostics,
    ProjectionTarget projectionTarget,
    CancellationToken cancellationToken)
{
    var actions = new List<LspCodeAction>();
    var laneKinds = GetOrderedLanes(projectionTarget, diagnostics);
    foreach (var laneKind in laneKinds)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (!_lanes.TryGetValue(laneKind, out var lane))
        {
            continue;
        }

        var laneActions = await lane.GetCodeActionsAsync(
            document,
            range,
            diagnostics,
            projectionTarget,
            cancellationToken);
        if (laneActions.Count > 0)
        {
            actions.AddRange(laneActions);
        }
    }

    return _resultAggregator.AggregateCodeActions(actions);
}
```

**协调策略**：
1. 根据投影目标和诊断来源确定车道列表
2. 每个车道独立生成代码操作
3. 收集所有车道的操作
4. 聚合和去重操作

**诊断来源检测**：
```csharp
private IReadOnlyList<LaneKind> GetOrderedLanes(
    ProjectionTarget projectionTarget,
    IReadOnlyList<LspDiagnostic> diagnostics)
{
    var laneKinds = _laneRouter.GetOrderedLanes(projectionTarget).ToList();
    if (ContainsVolarDiagnostic(diagnostics) && !laneKinds.Contains(LaneKind.Volar))
    {
        laneKinds.Add(LaneKind.Volar);
    }

    if (ContainsJazorDiagnostic(diagnostics) && !laneKinds.Contains(LaneKind.Jazor))
    {
        laneKinds.Add(LaneKind.Jazor);
    }

    return laneKinds;
}

private static bool ContainsVolarDiagnostic(IReadOnlyList<LspDiagnostic> diagnostics)
    => diagnostics.Any(diagnostic =>
        string.Equals(diagnostic.Source, "Jolt.Frontend", StringComparison.Ordinal)
        || string.Equals(diagnostic.Code, "JAZORVUEFRONTEND001", StringComparison.Ordinal)
        || string.Equals(diagnostic.Code, "JAZORVUEFRONTEND002", StringComparison.Ordinal));

private static bool ContainsJazorDiagnostic(IReadOnlyList<LspDiagnostic> diagnostics)
    => diagnostics.Any(diagnostic =>
        string.Equals(diagnostic.Source, "Jolt", StringComparison.Ordinal)
        || string.Equals(diagnostic.Code, "JAZORVUE001", StringComparison.Ordinal));
```

### 2.4 MarkupBridgeFanoutCoordinator

**文件位置**：`src/Jolt/Lsp/Coordination/MarkupBridgeFanoutCoordinator.cs`

**职责**：扇出模式的跨车道查询协调器

**核心方法**：

**定义协调**：
```csharp
public async ValueTask<IReadOnlyList<LspLocation>> CoordinateDefinitionAsync(
    DocumentSnapshot document,
    LspPosition position,
    IReadOnlyList<LspLocation> nativeLocations,
    bool allowMarkupFallback,
    CancellationToken cancellationToken)
{
    if (nativeLocations.Count > 0 || !allowMarkupFallback)
    {
        return _resultAggregator.AggregateLocations(nativeLocations);
    }

    var symbol = await _markupComponentBridge.ResolveBridgeSymbolAsync(
        document,
        position,
        locationHints: null,
        allowWorkspaceScan: true,
        cancellationToken);
    if (symbol is null)
    {
        return Array.Empty<LspLocation>();
    }

    return
    [
        new LspLocation
        {
            Uri = LspProtocolHelpers.ToDocumentUri(symbol.Value.AbsolutePath),
            Range = new LspRange
            {
                Start = new LspPosition { Line = 0, Character = 0 },
                End = new LspPosition { Line = 0, Character = 0 }
            }
        }
    ];
}
```

**引用协调**：
```csharp
public async ValueTask<IReadOnlyList<LspLocation>> CoordinateReferencesAsync(
    DocumentSnapshot document,
    LspPosition position,
    bool includeDeclaration,
    IReadOnlyList<LspLocation> nativeLocations,
    CancellationToken cancellationToken)
{
    var locations = nativeLocations.ToList();
    var symbol = await _markupComponentBridge.ResolveBridgeSymbolAsync(
        document,
        position,
        includeDeclaration ? nativeLocations : null,
        allowWorkspaceScan: true,
        cancellationToken);
    if (symbol is not null)
    {
        locations.AddRange(await _markupComponentBridge.FindJazorReferencesAsync(
            document,
            symbol.Value.ComponentName,
            symbol.Value.AbsolutePath,
            includeDeclaration,
            cancellationToken));
    }

    return _resultAggregator.AggregateLocations(locations);
}
```

**重命名协调**：
```csharp
public async ValueTask<LspWorkspaceEdit?> CoordinateRenameAsync(
    DocumentSnapshot document,
    LspPosition position,
    string newName,
    LspWorkspaceEdit? nativeEdit,
    CancellationToken cancellationToken)
{
    var edits = new List<LspWorkspaceEdit>();
    if (nativeEdit is not null)
    {
        edits.Add(nativeEdit);
    }

    var symbol = await _markupComponentBridge.ResolveBridgeSymbolAsync(
        document,
        position,
        locationHints: null,
        allowWorkspaceScan: true,
        cancellationToken);
    if (symbol is not null)
    {
        var changes = await _markupComponentBridge.FindJazorRenameChangesAsync(
            document,
            symbol.Value.ComponentName,
            symbol.Value.AbsolutePath,
            newName,
            cancellationToken);
        if (changes.Count > 0)
        {
            edits.Add(new LspWorkspaceEdit
            {
                Changes = changes
            });
        }
    }

    return _resultAggregator.AggregateWorkspaceEdits(edits);
}
```

## 3. 核心算法

### 3.1 引用查找算法

**文件位置**：`src/Jolt/Lsp/Coordination/MarkupComponentBridgeService.cs:300-333`

**步骤**：
1. 解析光标位置的符号（组件标签名称）
2. 查找组件定义（tracked/nearby/workspace scan）
3. 收集候选文档（打开文档 + 工作区扫描）
4. 在候选文档中查找组件标签匹配
5. 去重和排序结果

**实现**：
```csharp
public async ValueTask<IReadOnlyList<LspLocation>> FindJazorReferencesAsync(
    DocumentSnapshot document,
    string componentName,
    string? declarationDocumentPath,
    bool includeDeclaration,
    CancellationToken cancellationToken)
{
    var locations = new List<LspLocation>();
    if (includeDeclaration && !string.IsNullOrWhiteSpace(declarationDocumentPath))
    {
        locations.Add(new LspLocation
        {
            Uri = LspProtocolHelpers.ToDocumentUri(declarationDocumentPath),
            Range = new LspRange
            {
                Start = new LspPosition { Line = 0, Character = 0 },
                End = new LspPosition { Line = 0, Character = 0 }
            }
        });
    }

    var candidateDocuments = await GetJazorReferenceCandidateDocumentsAsync(
        document,
        declarationDocumentPath,
        cancellationToken);
    foreach (var candidateDocument in candidateDocuments)
    {
        locations.AddRange(FindComponentTagLocations(candidateDocument, componentName));
    }

    return locations
        .GroupBy(static location =>
            $"{location.Uri}:{location.Range.Start.Line}:{location.Range.Start.Character}:{location.Range.End.Line}:{location.Range.End.Character}",
            StringComparer.Ordinal)
        .Select(static group => group.First())
        .ToArray();
}
```

### 3.2 重命名编辑生成算法

**文件位置**：`src/Jolt/Lsp/Coordination/MarkupComponentBridgeService.cs:335-361`

**步骤**：
1. 解析光标位置的符号（组件标签名称）
2. 查找组件定义
3. 收集候选文档
4. 在候选文档中查找组件标签匹配
5. 生成重命名编辑（按位置降序排序）

**实现**：
```csharp
public async ValueTask<Dictionary<string, LspTextEdit[]>> FindJazorRenameChangesAsync(
    DocumentSnapshot document,
    string componentName,
    string? declarationDocumentPath,
    string newName,
    CancellationToken cancellationToken)
{
    var candidateDocuments = await GetJazorReferenceCandidateDocumentsAsync(
        document,
        declarationDocumentPath,
        cancellationToken);
    var changes = new Dictionary<string, LspTextEdit[]>(StringComparer.Ordinal);
    foreach (var candidateDocument in candidateDocuments)
    {
        var edits = FindComponentTagLocations(candidateDocument, componentName)
            .Select(location => new LspTextEdit
            {
                Range = location.Range,
                NewText = newName
            })
            .OrderByDescending(edit => LspProtocolHelpers.GetOffset(candidateDocument.Text, edit.Range.Start))
            .ToArray();
        if (edits.Length > 0)
        {
            changes[LspProtocolHelpers.ToDocumentUri(candidateDocument.DocumentPath)] = edits;
        }
    }

    return changes;
}
```

### 3.3 候选文档收集算法

**文件位置**：`src/Jolt/Lsp/Coordination/MarkupComponentBridgeService.cs:397-422`

**步骤**：
1. 收集打开的 Jazor 文档
2. 添加当前文档（如果是 Jazor 或 Vue）
3. 确定工作区搜索根目录
4. 递归扫描 `.jazor` 文件
5. 去重（基于规范化路径）

**实现**：
```csharp
private async ValueTask<IReadOnlyList<DocumentSnapshot>> GetJazorReferenceCandidateDocumentsAsync(
    DocumentSnapshot document,
    string? declarationDocumentPath,
    CancellationToken cancellationToken)
{
    var openDocuments = await _workspaceStore.GetOpenDocumentsAsync(cancellationToken);
    var documents = new List<DocumentSnapshot>();
    var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

    foreach (var openDocument in openDocuments.Where(static candidate => candidate.DocumentKind == DocumentKind.Jazor))
    {
        AddDocumentCandidate(openDocument, documents, seen);
    }

    if (document.DocumentKind is DocumentKind.Jazor or DocumentKind.Vue)
    {
        AddDocumentCandidate(document, documents, seen);
    }

    foreach (var directory in JoltWorkspaceResolver.GetWorkspaceSearchRoots(
        document.DocumentPath,
        declarationDocumentPath,
        openDocuments))
    {
        await AddDocumentsFromDirectoryAsync(
            directory,
            "*.jazor",
            openDocuments,
            documents,
            seen,
            cancellationToken);
    }

    return documents;
}
```

### 3.4 组件解析算法

**文件位置**：`src/Jolt/Lsp/Coordination/MarkupComponentBridgeService.cs:46-75`

**优先级**：
1. Tracked nearby（打开文档中查找 nearby Vue 组件）
2. Nearby（文件系统 nearby 查找）
3. Tracked（打开文档中查找绝对路径匹配）
4. Workspace scan（工作区扫描，如果允许）

**实现**：
```csharp
public async ValueTask<MarkupBridgeSymbol?> ResolveBridgeSymbolAsync(
    string documentPath,
    string componentName,
    bool allowWorkspaceScan,
    CancellationToken cancellationToken)
{
    var openDocuments = await _workspaceStore.GetOpenDocumentsAsync(cancellationToken);
    if (JoltWorkspaceResolver.TryResolveTrackedNearbyVueComponent(
        documentPath,
        componentName,
        openDocuments,
        out var trackedNearby))
    {
        return new MarkupBridgeSymbol(
            trackedNearby.ComponentName,
            trackedNearby.AbsolutePath,
            trackedNearby.ImportPath);
    }

    if (JoltWorkspaceResolver.TryResolveNearbyVueComponent(
        documentPath,
        componentName,
        out var componentPath,
        out var importPath))
    {
        return new MarkupBridgeSymbol(componentName, componentPath, importPath);
    }

    if (JoltWorkspaceResolver.TryResolveTrackedVueComponent(
        documentPath,
        componentName,
        openDocuments,
        out var tracked))
    {
        return new MarkupBridgeSymbol(tracked.ComponentName, tracked.AbsolutePath, tracked.ImportPath);
    }

    if (allowWorkspaceScan
        && JoltWorkspaceResolver.ResolveWorkspaceVueComponent(
            documentPath,
            componentName,
            openDocuments,
            cancellationToken) is { } workspaceResolved)
    {
        return new MarkupBridgeSymbol(
            workspaceResolved.ComponentName,
            workspaceResolved.AbsolutePath,
            workspaceResolved.ImportPath);
    }

    return null;
}
```

### 3.5 导入组件符号解析算法

**文件位置**：`src/Jolt/Lsp/Coordination/MarkupComponentBridgeService.cs:227-270`

**目的**：解析 JavaScript import 语句中的组件符号

**步骤**：
1. 使用正则表达式匹配 import 语句
2. 使用 JS trivia masking 避免注释和字符串干扰
3. 解析导入绑定（default/namespace/named）
4. 查找光标位置的导入绑定
5. 解析导入路径到绝对路径

**正则表达式**：
```csharp
private static readonly Regex ScriptImportPattern = new(
    @"^\s*import\s+(?<clause>.+?)\s+from\s+[""'](?<path>[^""']+)[""']",
    RegexOptions.Compiled | RegexOptions.Multiline);
```

**JS Trivia Masking**：
```csharp
private static string MaskJavaScriptTrivia(string text)
{
    var buffer = text.ToCharArray();
    for (var index = 0; index < text.Length; index++)
    {
        if (text[index] == '/' && index + 1 < text.Length)
        {
            if (text[index + 1] == '/')
            {
                index = MaskLineComment(text, buffer, index);
                continue;
            }

            if (text[index + 1] == '*')
            {
                index = MaskBlockComment(text, buffer, index);
                continue;
            }
        }

        if (text[index] is '\'' or '"')
        {
            index = MaskQuotedLiteral(text, buffer, index, text[index], preserveDelimiters: true);
            continue;
        }

        if (text[index] == '`')
        {
            index = MaskQuotedLiteral(text, buffer, index, '`', preserveDelimiters: false);
        }
    }

    return new string(buffer);
}
```

## 4. 线程安全模型

### 4.1 协调器

**无状态设计**：所有协调器都是无状态的，线程安全

**字段**：
- `_lanes`：只读字典（线程安全）
- `_laneRouter`：只读接口（线程安全）
- `_resultAggregator`：无状态类（线程安全）
- `_markupComponentBridge`：有状态（通过 `IJoltWorkspaceStore`）

### 4.2 MarkupComponentBridgeService

**工作区存储访问**：通过 `IJoltWorkspaceStore`（由实现保证线程安全）

**无共享状态**：所有方法都是独立的，不共享可变状态

## 5. 错误处理

### 5.1 取消支持

**所有协调器**：支持 `CancellationToken`

**模式**：
```csharp
cancellationToken.ThrowIfCancellationRequested();
```

### 5.2 异常传播

**策略**：所有异常直接传播到上层处理

**理由**：
- 协调器是逻辑层，不应吞没异常
- 上层（LspSession）有统一的异常处理机制

### 5.3 文件系统错误

**策略**：捕获并忽略文件系统异常

**实现**：
```csharp
try
{
    documents.Add(new DocumentSnapshot(...));
}
catch (FileNotFoundException)
{
}
catch (DirectoryNotFoundException)
{
}
catch (IOException)
{
}
catch (UnauthorizedAccessException)
{
}
```

**理由**：
- 文件可能被删除或移动
- 不应因为单个文件错误而失败整个操作
- 继续处理其他文件

## 6. 配置选项

### 6.1 构造函数参数

**ReferenceCoordinator**：
```csharp
public ReferenceCoordinator(
    IReadOnlyDictionary<LaneKind, ILspLane> lanes,
    ILspLaneRouter laneRouter,
    MarkupBridgeFanoutCoordinator markupBridgeFanout)
```

**RenameCoordinator**：
```csharp
public RenameCoordinator(
    IReadOnlyDictionary<LaneKind, ILspLane> lanes,
    ILspLaneRouter laneRouter,
    LspResultAggregator resultAggregator,
    MarkupBridgeFanoutCoordinator markupBridgeFanout)
```

**CodeActionCoordinator**：
```csharp
public CodeActionCoordinator(
    IReadOnlyDictionary<LaneKind, ILspLane> lanes,
    ILspLaneRouter laneRouter,
    LspResultAggregator resultAggregator)
```

**MarkupBridgeFanoutCoordinator**：
```csharp
public MarkupBridgeFanoutCoordinator(
    MarkupComponentBridgeService markupComponentBridge,
    LspResultAggregator resultAggregator)
```

**MarkupComponentBridgeService**：
```csharp
public MarkupComponentBridgeService(IJoltWorkspaceStore workspaceStore)
```

### 6.2 工作区扫描控制

**参数**：`allowWorkspaceScan`

**用途**：控制是否扫描整个工作区查找组件

**默认值**：
- Jazor 文档：`true`（允许工作区扫描）
- Vue 文档：`false`（不允许工作区扫描）

**理由**：
- Jazor 文档需要查找所有可能的组件
- Vue 文档通常只查找导入的组件

## 7. 与其他子系统的交互

### 7.1 与工作区存储交互

**MarkupComponentBridgeService**：
- 获取打开文档列表
- 确定工作区搜索根目录
- 扫描工作区文件

### 7.2 与车道路由器交互

**协调器**：
- 获取车道路由顺序
- 确定请求应发送到哪些车道

### 7.3 与结果聚合器交互

**协调器**：
- 聚合多车道结果
- 去重和排序结果

### 7.4 与组件桥接服务交互

**MarkupBridgeFanoutCoordinator**：
- 扇出查询到组件桥接服务
- 合并原生结果和桥接结果

## 8. 设计权衡

### 8.1 扇出模式 vs 直接查询

**选择**：扇出模式（先查询原生车道，再查询桥接服务）

**原因**：
- 原生车道提供快速、准确的结果
- 桥接服务提供额外的跨文档引用
- 分离关注点，易于维护

**权衡**：
- 优势：灵活性、可扩展性
- 劣势：多次查询（可接受）

### 8.2 工作区扫描 vs 打开文档

**选择**：优先使用打开文档，允许工作区扫描

**原因**：
- 打开文档提供最新、最准确的信息
- 工作区扫描提供完整的引用查找
- 用户可以选择是否启用工作区扫描

**权衡**：
- 优势：完整性、性能平衡
- 劣势：可能扫描大量文件（可配置）

### 8.3 文件系统错误处理

**选择**：捕获并忽略文件系统错误

**原因**：
- 文件可能被删除或移动
- 不应因为单个文件错误而失败整个操作
- 继续处理其他文件

**权衡**：
- 优势：鲁棒性
- 劣势：可能遗漏部分结果（可接受）

### 8.4 JS Trivia Masking

**选择**：实现完整的 JS trivia masking

**原因**：
- 避免注释和字符串干扰 import 解析
- 提供准确的导入绑定解析
- 支持复杂的 import 语句

**权衡**：
- 优势：准确性、完整性
- 劣势：实现复杂度（可接受）
