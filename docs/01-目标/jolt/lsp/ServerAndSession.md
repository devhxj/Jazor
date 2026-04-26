# LSP Server and Session

> Status: 活跃参考
> Positioning: Jolt LSP 子系统的核心消息处理与协议实现层

## 1. 文档定位

本文档描述 Jolt LSP 服务器和会话的实现，包括标准输入/输出通信、消息分发、请求取消、错误处理和语言特性的提供。

**相关文件**：
- `src/Jolt/Lsp/StdioLspServer.cs` (402行) - 标准输入/输出 LSP 服务器
- `src/Jolt/Lsp/LspSession.cs` (885行) - LSP 会话核心逻辑
- `src/Jolt/Lsp/LspSession.TextAndFormatting.cs` - 文本同步和格式化
- `src/Jolt/Lsp/LspSession.WorkspaceFolders.cs` - 工作区文件夹管理
- `src/Jolt/Lsp/LspSession.ProviderIsolationAndRouting.cs` - 提供者隔离和车道路由

## 2. 核心类型

### 2.1 StdioLspServer

**职责**：管理 stdin/stdup 上的 LSP 消息协议

**核心字段**：
```csharp
private readonly LspSession _session;
private readonly Lock _requestGate = new();
private readonly Dictionary<string, CancellationTokenSource> _activeRequests;
private readonly HashSet<string> _pendingCancellationRequests;
```

**生命周期**：
1. 创建 `Channel<LRpcRequestMessage>` 用于消息队列
2. 读取循环：从 stdin 读取 JSON RPC 消息
3. 处理特殊消息：`$/cancelRequest` 和 `exit`
4. 工作循环：处理队列中的请求/通知
5. 优雅关闭：取消所有活动请求并清理

### 2.2 LspSession

**职责**：实现 LSP 协议的所有语言特性

**核心字段**：
```csharp
private readonly IJoltWorkspaceStore _workspaceStore;
private readonly IReadOnlyDictionary<LaneKind, ILspLane> _lanes;
private readonly ILspLaneRouter _laneRouter;
private readonly JazorProjectionService _projectionService;
private readonly DocumentProjectionResolver _projectionResolver;
private readonly LspResultAggregator _resultAggregator;
private readonly ReferenceCoordinator _referenceCoordinator;
private readonly RenameCoordinator _renameCoordinator;
private readonly CodeActionCoordinator _codeActionCoordinator;
```

**支持的 LSP 方法**：
- 初始化：`initialize`、`initialized`、`shutdown`
- 文本同步：`textDocument/didOpen`、`textDocument/didChange`、`textDocument/didClose`、`textDocument/didSave`
- 语言特性：`hover`、`completion`、`definition`、`references`、`rename`、`codeAction`、`semanticTokens`、`foldingRange`、`inlayHint`、`documentSymbol`、`signatureHelp`、`documentHighlight`、`typeHierarchy`、`callHierarchy`
- 工作区：`workspace/didChangeWatchedFiles`、`workspace/symbol`

## 3. 核心算法

### 3.1 请求取消机制

**文件位置**：`src/Jolt/Lsp/StdioLspServer.cs:48-52, 262-281`

**流程**：
1. 客户端发送 `$/cancelRequest` 通知，包含要取消的请求 ID
2. `CancelRequest` 方法提取请求 ID 并查找：
   - 如果请求正在执行：取消其 `CancellationTokenSource`
   - 如果请求尚未执行：添加到 `_pendingCancellationRequests`
3. 当请求开始执行时，检查是否已被标记为取消
4. 返回错误码 `-32800`（RequestCancelled）

**请求键生成**：
```csharp
private static string? CreateRequestKey(object? id)
{
    if (id is JsonElement jsonElement)
    {
        return jsonElement.ValueKind switch
        {
            JsonValueKind.String => "s:" + jsonElement.GetString(),
            JsonValueKind.Number => "n:" + jsonElement.GetRawText(),
            _ => "j:" + jsonElement.GetRawText()
        };
    }
    if (id is string text) return "s:" + text;
    return "o:" + id.ToString();
}
```

### 3.2 提供者隔离机制

**文件位置**：`src/Jolt/Lsp/LspSession.ProviderIsolationAndRouting.cs:19-179`

**目的**：防止失败的外部提供者（如扩展）反复超时，影响用户体验

**算法**：
1. **失败记录**：每次提供者失败时，增加连续失败计数
2. **隔离阈值**：当连续失败达到阈值（默认2次）时，隔离该提供者
3. **隔离期**：被隔离的提供者在指定期间（默认10秒）内被跳过
4. **恢复**：隔离期过后或成功执行后重置失败计数

**状态结构**：
```csharp
private readonly record struct ProviderIsolationState(
    int ConsecutiveFailureCount,
    DateTimeOffset? IsolatedUntil);
```

**使用示例**：
```csharp
if (TryGetProviderIsolationWindow(capability, providerName, out var remaining))
{
    _extensionRegistry.ReportProviderInvocation(..., Skipped: true, ...);
    return ProviderInvocationResult<TResult>.Isolated();
}
```

### 3.3 文本同步流程

**文件位置**：`src/Jolt/Lsp/LspSession.cs:632-698`

**didOpen**：
1. 解析 `DidOpenTextDocumentParams`
2. 创建 `DocumentSnapshot`（包含路径、类型、文本、版本）
3. 更新工作区存储：`UpsertDocumentAsync`
4. 使工作区解析器缓存失效：`JoltWorkspaceResolver.InvalidatePath`
5. 更新投影状态：`UpdateProjectionStateAsync`
6. 发布诊断：`PublishDiagnosticsAsync`
7. 刷新其他打开的 Jazor 文档的诊断

**didChange**：
1. 解析 `DidChangeTextDocumentParams`
2. 应用内容更改（取最后一个内容更改）
3. 更新文档快照
4. 使缓存失效
5. 更新投影状态
6. 发布诊断
7. 刷新相关文档
8. 通知工作区文档更改（用于 HMR）

**didClose**：
1. 从工作区存储中移除文档
2. 使缓存失效
3. 清理虚拟文档注册表
4. 发布空诊断（清除客户端诊断）

## 4. 线程安全模型

### 4.1 StdioLspServer

**锁策略**：
- `_requestGate`：保护 `_activeRequests` 和 `_pendingCancellationRequests` 的访问
- 单一写入者/单一读取者 Channel：确保消息顺序处理

**并发模型**：
- 读取循环和工作循环并发执行
- 请求处理按顺序进行（Channel 单一读取者）
- 请求取消通过 `CancellationToken` 实现

### 4.2 LspSession

**锁策略**：
- `_providerIsolationGate`：保护提供者隔离状态字典
- `_workspaceFoldersGate`：保护工作区文件夹字典

**无状态设计**：
- 每个请求/通知处理都是独立的
- 工作区存储 (`IJoltWorkspaceStore`) 负责自己的线程安全

## 5. 错误处理

### 5.1 LspRequestException

**用途**：表示可预期的 LSP 请求错误（如无效参数）

**错误码**：
- `-32602` (InvalidParams)：参数无效
- `-32603` (InternalError)：内部错误
- `-32800` (RequestCancelled)：请求被取消

**处理策略**：
```csharp
catch (LspRequestException ex)
{
    response = new LspResponseMessage
    {
        Id = request.Id,
        Error = new LspResponseError
        {
            Code = ex.ErrorCode,
            Message = ex.Message
        }
    };
}
```

### 5.2 通用异常处理

**通知处理**：捕获所有异常，记录到 stderr，继续运行
```csharp
catch (Exception exception)
{
    WriteServerWarning("lspNotificationFailed", message.Method, exception);
    continue; // 保持服务器循环运行
}
```

**请求处理**：区分 `LspRequestException` 和通用异常
```csharp
catch (Exception ex)
{
    response = new LspResponseMessage
    {
        Id = request.Id,
        Error = new LspResponseError
        {
            Code = -32603,
            Message = ex.Message
        }
    };
}
```

### 5.3 观察可观测性事件

**文件位置**：`src/Jolt/Lsp/StdioLspServer.cs:381-401`

所有错误和警告事件以 JSON 格式输出到 stderr：
```json
{
  "eventType": "lspNotificationFailed",
  "method": "textDocument/didChange",
  "errorType": "System.InvalidOperationException",
  "message": "Invalid LSP params payload",
  "timestamp": "2026-04-21T12:34:56.789Z"
}
```

## 6. 配置选项

### 6.1 构造函数参数

```csharp
public LspSession(
    IJoltWorkspaceStore workspaceStore,
    IEnumerable<ILspLane> lanes,
    ILspLaneRouter laneRouter,
    LspMessageWriter writer,
    JazorProjectionService projectionService,
    IVirtualDocumentRegistry virtualDocumentRegistry,
    DocumentProjectionResolver projectionResolver,
    LspResultAggregator resultAggregator,
    MarkupBridgeFanoutCoordinator markupBridgeFanoutCoordinator,
    ReferenceCoordinator referenceCoordinator,
    RenameCoordinator renameCoordinator,
    CodeActionCoordinator codeActionCoordinator,
    IWorkspaceDocumentChangeSink? workspaceDocumentChangeSink = null,
    IExtensionRegistry? extensionRegistry = null,
    TimeSpan? extensionProviderTimeout = null,
    int extensionProviderIsolationFailureThreshold = 2,
    TimeSpan? extensionProviderIsolationDuration = null)
```

**默认值**：
- `extensionProviderTimeout`: 2 秒
- `extensionProviderIsolationFailureThreshold`: 2 次失败
- `extensionProviderIsolationDuration`: 10 秒

### 6.2 工作区文件夹管理

**初始化**（`initialize` 请求）：
```csharp
private void ApplyInitializeWorkspaceFolders(LspInitializeParams? parameters)
{
    var workspaceFolders = (parameters?.WorkspaceFolders ?? [])
        .Where(folder => !string.IsNullOrWhiteSpace(folder.Uri))
        .Select(CloneWorkspaceFolder)
        .ToArray();

    if (workspaceFolders.Length == 0)
    {
        var fallbackRootUri = parameters?.RootUri;
        // 使用 RootPath 作为后备
    }

    lock (_workspaceFoldersGate)
    {
        _workspaceFoldersByUri.Clear();
        foreach (var folder in workspaceFolders)
        {
            _workspaceFoldersByUri[folder.Uri] = folder;
        }
    }
}
```

**动态更改**（`workspace/didChangeWorkspaceFolders`）：
```csharp
private void ApplyWorkspaceFolderChanges(LspWorkspaceFoldersChangeEvent changeEvent)
{
    lock (_workspaceFoldersGate)
    {
        foreach (var removed in changeEvent.Removed ?? [])
        {
            _workspaceFoldersByUri.Remove(removed.Uri);
        }
        foreach (var added in changeEvent.Added ?? [])
        {
            _workspaceFoldersByUri[added.Uri] = CloneWorkspaceFolder(added);
        }
    }
}
```

## 7. 与其他子系统的交互

### 7.1 工作区存储 (IJoltWorkspaceStore)

**用途**：管理文档状态和打开文档列表

**交互**：
- `didOpen/didChange/didClose`：更新文档存储
- `GetDocumentAsync`：获取文档进行 LSP 操作
- `GetOpenDocumentsAsync`：获取所有打开文档（随后按 owning project 过滤后再做跨文档引用和诊断刷新）

**作用域规则**：
- 跨文档引用只在 owning project 内展开
- 诊断刷新只重算 owning project 的受影响文档
- 找不到 `.slnx` 时，项目级发现必须返回英文错误，不得静默降级为全工作区扫描

### 7.2 虚拟文档注册表 (IVirtualDocumentRegistry)

**用途**：管理投影文档（Jazor → Vue/C#）

**交互**：
```csharp
private async ValueTask UpdateProjectionStateAsync(
    DocumentSnapshot document,
    CancellationToken cancellationToken)
{
    if (document.DocumentKind != DocumentKind.Jazor)
    {
        return;
    }

    var virtualDocuments = await _projectionService.ProjectAsync(document, cancellationToken);
    await _virtualDocumentRegistry.UpsertAsync(virtualDocuments, cancellationToken);
}
```

**注意**：投影和刷新只应针对 owning project 内的文档集合；不要把 sibling project 的文档混进同一次诊断或 HMR 更新。

### 7.3 车道路由 (ILspLaneRouter)

**用途**：根据文档类型和区域确定请求应路由到哪些车道

**交互**：
```csharp
private IReadOnlyList<ILspLane> GetOrderedLanes(ProjectionTarget projectionTarget)
{
    var orderedLanes = new List<ILspLane>();
    foreach (var laneKind in _laneRouter.GetOrderedLanes(projectionTarget))
    {
        if (_lanes.TryGetValue(laneKind, out var lane))
        {
            orderedLanes.Add(lane);
        }
    }
    return orderedLanes;
}
```

### 7.4 结果聚合器 (LspResultAggregator)

**用途**：去重和合并多车道结果

**交互**：
```csharp
private async ValueTask PublishDiagnosticsAsync(
    DocumentSnapshot document,
    CancellationToken cancellationToken,
    IReadOnlyList<LspDiagnostic>? diagnostics = null)
{
    diagnostics ??= await CollectDiagnosticsAsync(document, cancellationToken);
    diagnostics = _resultAggregator.AggregateDiagnostics(diagnostics);
    // 发布到客户端
}
```

### 7.5 工作区文档更改接收器 (IWorkspaceDocumentChangeSink)

**用途**：通知 DevServer HMR 系统文档更改

**交互**：
```csharp
await _workspaceDocumentChangeSink.OnWorkspaceDocumentChangedAsync(
    document,
    openDocuments,
    cancellationToken);
```

**错误处理**：HMR 协调失败不影响 LSP 核心功能

## 8. 设计权衡

### 8.1 消息队列 vs 直接处理

**选择**：使用 Channel 消息队列

**原因**：
- 解耦读取和处理逻辑
- 允许 `$/cancelRequest` 在请求执行前处理
- 提供背压机制（无界 Channel 可能导致内存问题）

**权衡**：
- 优势：更好的取消支持，清晰的职责分离
- 劣势：额外的内存分配和上下文切换

### 8.2 提供者隔离 vs 快速失败

**选择**：实现提供者隔离机制

**原因**：
- 外部扩展可能频繁超时（如网络请求）
- 反复超时严重影响用户体验
- 隔离期后自动恢复，无需手动干预

**权衡**：
- 优势：保护用户体验，自动恢复
- 劣势：隔离期间可能错过有效的提供者响应

### 8.3 工作区文件夹锁定 vs 无锁

**选择**：使用 `Lock` 保护工作区文件夹字典

**原因**：
- 工作区文件夹可能动态更改
- 需要确保读取和更新的一致性
- 字典访问频率不高，锁开销可接受

**权衡**：
- 优势：简单、安全
- 劣势：可能成为瓶颈（如果频繁访问）

### 8.4 诊断刷新策略

**选择**：Jazor 文档更改时刷新所有打开的 Jazor 文档诊断

**原因**：
- Jazor 支持跨文档引用（如组件标签）
- 一个文档的更改可能影响其他文档的诊断
- 用户期望看到所有相关错误的实时更新

**权衡**：
- 优势：确保诊断的一致性
- 劣势：大量打开文档时可能影响性能

**优化**：仅刷新 Jazor 文档，跳过其他类型文档

### 8.5 观察可观测性 vs 静默失败

**选择**：所有错误和警告输出到 stderr

**原因**：
- 便于调试和监控
- 不影响 stdout 上的 JSON RPC 协议
- 客户端可以单独捕获和分析

**权衡**：
- 优势：易于观察，不影响协议
- 劣势：可能产生大量输出（需要日志轮转）
