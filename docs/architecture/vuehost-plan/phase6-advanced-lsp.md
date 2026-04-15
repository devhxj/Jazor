# Phase 6: 高级 LSP 功能 — 详细实施计划

## 目标

实现完整的高级 LSP 功能：引用查找 (Find References)、重命名 (Rename)、代码操作 (Code Actions)、Document Symbol、Semantic Tokens、跨 Lane 符号身份合并、bridge supplement 完善。

**验收标准**:
- 在 `.jazor` 中引用组件/变量，"查找所有引用" 返回所有使用位置（跨 `.jazor`/`.vue`/`.ts`）
- 重命名组件/变量，所有引用位置同步更新
- 代码操作 (Quick Fix / Refactor) 在 `.jazor` 中可用
- Document Symbol 正确显示 `.jazor` 文件结构
- Semantic Tokens 提供语法高亮语义信息

---

## 一、LSP 能力矩阵回顾

### 1.1 阶段定义

| 阶段 | 覆盖范围 | 核心能力 |
|------|---------|---------|
| **P1** | 最小可用 | `.jazor` 诊断/补全/悬停/定义 + `.vue`/`.ts` 诊断/补全/悬停 |
| **P2** | 增强导航与重构 | 引用、重命名、Document Symbol |
| **P3** | 完整覆盖 | 代码操作、Semantic Tokens、Inlay Hints |

**Phase 6 目标**: 完成 P2 + 部分 P3 能力。

### 1.2 能力矩阵详细

| 文件类型 | 诊断 | 补全 | 悬停 | 定义 | 引用 | 重命名 | 代码操作 | Doc Symbol | Sem Tokens |
|---------|:----:|:----:|:----:|:----:|:----:|:------:|:--------:|:----------:|:----------:|
| `.jazor` | P1 ✅ | P1 ✅ | P1 ✅ | P1 ✅ | P2 | P2 | P2/P3 | P2 | P3 |
| `.vue` | P1 ✅ | P1 ✅ | P1 ✅ | P1 ✅ | P2 | P2 | P3 | P2 | P3 |
| `.ts` | P1 ✅ | P1 ✅ | P1 ✅ | P2 | P2 | P2 | P3 | P2 | P3 |
| `.js` | P2 | P2 | P1 ✅ | P2 | P2 | P3 | P3 | P2 | P3 |
| `.css` | P2 | P2 | P2 | P3 | — | P3 | P3 | P3 | — |
| `.html` | P2 | P2 | P2 | P3 | — | P3 | P3 | P3 | — |

---

## 二、核心架构

### 2.1 三 Lane 协调模型

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                              LSP Layer                                       │
│                                                                              │
│  ┌─────────────────────────────────────────────────────────────────────┐    │
│  │                    LspSession + Coordinators                        │    │
│  │                                                                     │    │
│  │  ┌──────────────────┐ ┌──────────────────┐ ┌──────────────────┐    │    │
│  │  │ReferenceCoordinator│ │RenameCoordinator │ │CodeActionCoordinator│   │    │
│  │  │                   │ │                  │ │                    │    │    │
│  │  │ - 跨 Lane 聚合    │ │ - 跨 Lane 同步   │ │ - 跨 Lane 提供     │    │    │
│  │  │ - Bridge 补充     │ │ - Bridge 补充    │ │ - Bridge 补充      │    │    │
│  │  └──────────────────┘ └──────────────────┘ └──────────────────┘    │    │
│  │                                                                     │    │
│  │  ┌──────────────────────────────────────────────────────────────┐  │    │
│  │  │              ProjectionMap (段级位置映射)                     │  │    │
│  │  │                                                              │  │    │
│  │  │  .jazor:15:10 ──▶ @code 投影片段:5:2    (RoslynLane)        │  │    │
│  │  │  .jazor:25:5  ──▶ 组件标签锚点          (VolarLane)         │  │    │
│  │  └──────────────────────────────────────────────────────────────┘  │    │
│  └─────────────────────────────────────────────────────────────────────┘    │
│                                      │                                       │
│          ┌───────────────────────────┼───────────────────────────┐          │
│          ▼                           ▼                           ▼          │
│  ┌───────────────┐          ┌───────────────┐          ┌───────────────┐   │
│  │  JazorLane    │          │  RoslynLane   │          │  VolarLane    │   │
│  │               │          │               │          │               │   │
│  │ - 结构解析    │          │ - C# 语义     │          │ - Vue/TS 语义 │   │
│  │ - 区域分类    │          │ - @code 块    │          │ - 组件解析    │   │
│  │ - 路由决策    │          │ - 导航/重命名  │          │ - 模板语义    │   │
│  └───────────────┘          └───────────────┘          └───────────────┘   │
│                                                                              │
└─────────────────────────────────────────────────────────────────────────────┘
```

### 2.2 关键约束

**来自 vuehost-capabilities.md**:

> **关键约束**：没有任何 Lane 直接向 IDE 发布结果。所有 Lane 输出都先进入 `LspSession`、`DocumentProjectionResolver`、`LspLaneRouter` 以及 shared coordinators，再由 LSP 层统一发送。

**双向桥接约束**:
- `.jazor/.cs -> VueHost bridge -> .vue/.ts/.js/.css/.html`
- `.vue/.ts/.js/.css/.html -> VueHost bridge -> .jazor/.cs`
- `definition / references / rename` 的 bridge supplement 必须集中在 session/coordinator 层

---

## 三、新增文件清单

```
src/Jazor.VueHost/
├── Lsp/                                # [已存在，扩展]
│   ├── Handlers/
│   │   ├── ReferencesHandler.cs        # [新增] textDocument/references
│   │   ├── RenameHandler.cs            # [新增] textDocument/rename
│   │   ├── CodeActionHandler.cs        # [新增] textDocument/codeAction
│   │   ├── DocumentSymbolHandler.cs    # [新增] textDocument/documentSymbol
│   │   ├── SemanticTokensHandler.cs    # [新增] textDocument/semanticTokens
│   │   └── PrepareRenameHandler.cs     # [新增] textDocument/prepareRename
│   │
│   ├── Coordinators/
│   │   ├── ReferenceCoordinator.cs     # [新增] 引用聚合协调器
│   │   ├── RenameCoordinator.cs        # [新增] 重命名协调器
│   │   ├── CodeActionCoordinator.cs    # [新增] 代码操作协调器
│   │   └── SymbolIdentityResolver.cs   # [新增] 跨 Lane 符号身份解析
│   │
│   ├── Bridge/
│   │   ├── IBridgeSymbol.cs            # [新增] 桥接符号接口
│   │   ├── MarkupBridgeSymbol.cs       # [已存在，扩展] 标记桥接符号
│   │   ├── ImportBridgeSymbol.cs       # [新增] 导入桥接符号
│   │   └── BridgeSupplementProvider.cs # [新增] Bridge 补充提供者
│   │
│   └── Projection/
│       ├── ProjectionMap.cs            # [已存在，增强] 段级映射
│       ├── ProjectionMapBuilder.cs     # [新增] 映射构建器
│       └── ProjectionEntry.cs          # [已存在] 映射条目
│
└── Workspace/
    └── WorkspaceSymbolIndex.cs         # [新增] 工作区符号索引
```

---

## 四、接口与类型定义

### 4.1 ReferenceCoordinator

```csharp
// Lsp/Coordinators/ReferenceCoordinator.cs
namespace Jazor.VueHost.Lsp.Coordinators;

/// <summary>
/// 引用查找协调器，负责跨 Lane 引用聚合和 Bridge 补充
/// </summary>
public sealed class ReferenceCoordinator
{
    private readonly LspSession _session;
    private readonly ProjectionMap _projectionMap;
    private readonly RoslynLane _roslynLane;
    private readonly VolarLane _volarLane;
    private readonly SymbolIdentityResolver _symbolResolver;
    private readonly BridgeSupplementProvider _bridgeProvider;
    
    public ReferenceCoordinator(
        LspSession session,
        ProjectionMap projectionMap,
        RoslynLane roslynLane,
        VolarLane volarLane,
        SymbolIdentityResolver symbolResolver,
        BridgeSupplementProvider bridgeProvider)
    {
        _session = session;
        _projectionMap = projectionMap;
        _roslynLane = roslynLane;
        _volarLane = volarLane;
        _symbolResolver = symbolResolver;
        _bridgeProvider = bridgeProvider;
    }
    
    /// <summary>
    /// 查找所有引用
    /// </summary>
    public async Task<Location[]> FindReferencesAsync(
        ReferenceParams parameters,
        CancellationToken cancellationToken)
    {
        var uri = parameters.TextDocument.Uri;
        var position = parameters.Position;
        
        // 1. 确定符号身份
        var symbolIdentity = await _symbolResolver.ResolveAsync(uri, position, cancellationToken);
        if (symbolIdentity == null)
            return Array.Empty<Location>();
        
        // 2. 根据文件类型路由
        var results = new List<Location>();
        
        if (uri.EndsWith(".jazor", StringComparison.OrdinalIgnoreCase))
        {
            // .jazor 文件：三 Lane 协作
            results.AddRange(await FindJazorReferencesAsync(
                uri, position, symbolIdentity, cancellationToken));
        }
        else if (uri.EndsWith(".vue", StringComparison.OrdinalIgnoreCase) ||
                 uri.EndsWith(".ts", StringComparison.OrdinalIgnoreCase) ||
                 uri.EndsWith(".js", StringComparison.OrdinalIgnoreCase))
        {
            // .vue/.ts/.js 文件：VolarLane + Bridge 补充
            results.AddRange(await FindVolarReferencesAsync(
                uri, position, symbolIdentity, cancellationToken));
        }
        else if (uri.EndsWith(".cs", StringComparison.OrdinalIgnoreCase))
        {
            // .cs 文件：RoslynLane + Bridge 补充
            results.AddRange(await FindRoslynReferencesAsync(
                uri, position, symbolIdentity, cancellationToken));
        }
        
        // 3. 去重和排序
        return DeduplicateAndSort(results);
    }
    
    private async Task<List<Location>> FindJazorReferencesAsync(
        string uri,
        Position position,
        SymbolIdentity symbol,
        CancellationToken cancellationToken)
    {
        var results = new List<Location>();
        
        // 2.1 确定光标所在区域
        var projection = _projectionMap.MapToTarget(uri, position);
        if (projection == null)
            return results;
        
        // 2.2 根据 Lane 类型分发
        switch (projection.TargetLane)
        {
            case LaneKind.Roslyn:
                // 在 @code 块中：查询 Roslyn 引用
                var roslynRefs = await _roslynLane.FindReferencesAsync(
                    projection.TargetUri,
                    projection.TargetPosition,
                    cancellationToken);
                
                // 映射回 .jazor
                foreach (var loc in roslynRefs)
                {
                    var mapped = _projectionMap.MapToSource(loc.Uri, loc.Range.Start);
                    if (mapped != null)
                        results.Add(new Location { Uri = mapped.SourceUri, Range = mapped.Range });
                }
                
                // Bridge 补充：查找同名组件/变量在前端的引用
                var bridgeRefs = await _bridgeProvider.FindBridgeReferencesAsync(
                    symbol, BridgeDirection.CSharpToFrontend, cancellationToken);
                results.AddRange(bridgeRefs);
                break;
                
            case LaneKind.Volar:
                // 在模板区中：查询 Volar 引用
                var volarRefs = await _volarLane.FindReferencesAsync(
                    uri, position, cancellationToken);
                results.AddRange(volarRefs);
                
                // Bridge 补充：查找同名符号在 C# 中的引用
                var csharpRefs = await _bridgeProvider.FindBridgeReferencesAsync(
                    symbol, BridgeDirection.FrontendToCSharp, cancellationToken);
                results.AddRange(csharpRefs);
                break;
        }
        
        return results;
    }
    
    private async Task<List<Location>> FindVolarReferencesAsync(
        string uri,
        Position position,
        SymbolIdentity symbol,
        CancellationToken cancellationToken)
    {
        var results = new List<Location>();
        
        // VolarLane 原生引用
        var volarRefs = await _volarLane.FindReferencesAsync(uri, position, cancellationToken);
        results.AddRange(volarRefs);
        
        // Bridge 补充：查找 .jazor 中的引用
        var bridgeRefs = await _bridgeProvider.FindBridgeReferencesAsync(
            symbol, BridgeDirection.FrontendToJazor, cancellationToken);
        results.AddRange(bridgeRefs);
        
        return results;
    }
    
    private async Task<List<Location>> FindRoslynReferencesAsync(
        string uri,
        Position position,
        SymbolIdentity symbol,
        CancellationToken cancellationToken)
    {
        var results = new List<Location>();
        
        // RoslynLane 原生引用
        var roslynRefs = await _roslynLane.FindReferencesAsync(uri, position, cancellationToken);
        results.AddRange(roslynRefs);
        
        // Bridge 补充：查找 .jazor 模板中的引用
        var bridgeRefs = await _bridgeProvider.FindBridgeReferencesAsync(
            symbol, BridgeDirection.CSharpToJazor, cancellationToken);
        results.AddRange(bridgeRefs);
        
        return results;
    }
    
    private static Location[] DeduplicateAndSort(List<Location> locations)
    {
        return locations
            .GroupBy(l => (l.Uri, l.Range.Start.Line, l.Range.Start.Character))
            .Select(g => g.First())
            .OrderBy(l => l.Uri)
            .ThenBy(l => l.Range.Start.Line)
            .ThenBy(l => l.Range.Start.Character)
            .ToArray();
    }
}

/// <summary>
/// 符号身份
/// </summary>
public sealed record SymbolIdentity
{
    /// <summary>
    /// 符号名称
    /// </summary>
    public required string Name { get; init; }
    
    /// <summary>
    /// 符号类型
    /// </summary>
    public required SymbolKind Kind { get; init; }
    
    /// <summary>
    /// 定义位置
    /// </summary>
    public required Location Definition { get; init; }
    
    /// <summary>
    /// 所属 Lane
    /// </summary>
    public LaneKind SourceLane { get; init; }
    
    /// <summary>
    /// 跨 Lane 桥接标识
    /// </summary>
    public string? BridgeId { get; init; }
}

public enum LaneKind
{
    Jazor,
    Roslyn,
    Volar
}

public enum BridgeDirection
{
    CSharpToFrontend,
    FrontendToCSharp,
    CSharpToJazor,
    FrontendToJazor
}
```

### 4.2 RenameCoordinator

```csharp
// Lsp/Coordinators/RenameCoordinator.cs
namespace Jazor.VueHost.Lsp.Coordinators;

/// <summary>
/// 重命名协调器，负责跨 Lane 重命名同步
/// </summary>
public sealed class RenameCoordinator
{
    private readonly LspSession _session;
    private readonly ProjectionMap _projectionMap;
    private readonly RoslynLane _roslynLane;
    private readonly VolarLane _volarLane;
    private readonly SymbolIdentityResolver _symbolResolver;
    private readonly BridgeSupplementProvider _bridgeProvider;
    
    public async Task<WorkspaceEdit?> RenameAsync(
        RenameParams parameters,
        CancellationToken cancellationToken)
    {
        var uri = parameters.TextDocument.Uri;
        var position = parameters.Position;
        var newName = parameters.NewName;
        
        // 1. 验证重命名可行性
        var prepareResult = await PrepareRenameAsync(uri, position, cancellationToken);
        if (!prepareResult.CanRename)
            return null;
        
        // 2. 获取符号身份
        var symbol = await _symbolResolver.ResolveAsync(uri, position, cancellationToken);
        if (symbol == null)
            return null;
        
        // 3. 收集所有编辑
        var edits = new Dictionary<DocumentUri, List<TextEdit>>();
        
        if (uri.EndsWith(".jazor", StringComparison.OrdinalIgnoreCase))
        {
            await CollectJazorRenamesAsync(edits, uri, position, symbol, newName, cancellationToken);
        }
        else if (uri.EndsWith(".vue", StringComparison.OrdinalIgnoreCase) ||
                 uri.EndsWith(".ts", StringComparison.OrdinalIgnoreCase))
        {
            await CollectVolarRenamesAsync(edits, uri, position, symbol, newName, cancellationToken);
        }
        else if (uri.EndsWith(".cs", StringComparison.OrdinalIgnoreCase))
        {
            await CollectRoslynRenamesAsync(edits, uri, position, symbol, newName, cancellationToken);
        }
        
        // 4. 构建 WorkspaceEdit
        return new WorkspaceEdit
        {
            Changes = edits.ToDictionary(
                kvp => kvp.Key,
                kvp => kvp.Value.ToArray())
        };
    }
    
    /// <summary>
    /// 准备重命名，验证重命名是否可行
    /// </summary>
    public async Task<PrepareRenameResult> PrepareRenameAsync(
        string uri,
        Position position,
        CancellationToken cancellationToken)
    {
        // 检查符号是否可重命名
        var symbol = await _symbolResolver.ResolveAsync(uri, position, cancellationToken);
        if (symbol == null)
        {
            return new PrepareRenameResult
            {
                CanRename = false,
                ErrorMessage = "No symbol found at this position"
            };
        }
        
        // 检查是否是只读符号（如内置类型）
        if (IsReadonlySymbol(symbol))
        {
            return new PrepareRenameResult
            {
                CanRename = false,
                ErrorMessage = "Cannot rename built-in symbol"
            };
        }
        
        // 检查是否是跨文件组件引用
        if (symbol.Kind == SymbolKind.Component && IsExternalComponent(symbol))
        {
            return new PrepareRenameResult
            {
                CanRename = false,
                ErrorMessage = "Cannot rename component from external library"
            };
        }
        
        return new PrepareRenameResult
        {
            CanRename = true,
            Range = symbol.Definition.Range,
            Placeholder = symbol.Name
        };
    }
    
    private async Task CollectJazorRenamesAsync(
        Dictionary<DocumentUri, List<TextEdit>> edits,
        string uri,
        Position position,
        SymbolIdentity symbol,
        string newName,
        CancellationToken cancellationToken)
    {
        var projection = _projectionMap.MapToTarget(uri, position);
        if (projection == null) return;
        
        switch (projection.TargetLane)
        {
            case LaneKind.Roslyn:
            {
                // Roslyn 重命名
                var roslynEdits = await _roslynLane.RenameAsync(
                    projection.TargetUri,
                    projection.TargetPosition,
                    newName,
                    cancellationToken);
                
                // 映射回 .jazor
                foreach (var (docUri, textEdits) in roslynEdits)
                {
                    // docUri 是投影片段 URI，需要映射回源
                    var sourceEdits = MapEditsToSource(docUri, textEdits);
                    AddEdits(edits, sourceEdits);
                }
                
                // Bridge 重命名：前端组件名
                var bridgeEdits = await _bridgeProvider.GetRenameEditsAsync(
                    symbol, newName, BridgeDirection.CSharpToFrontend, cancellationToken);
                AddEdits(edits, bridgeEdits);
                break;
            }
            
            case LaneKind.Volar:
            {
                // Volar 重命名
                var volarEdits = await _volarLane.RenameAsync(
                    uri, position, newName, cancellationToken);
                AddEdits(edits, volarEdits);
                
                // Bridge 重命名：C# 变量名
                var bridgeEdits = await _bridgeProvider.GetRenameEditsAsync(
                    symbol, newName, BridgeDirection.FrontendToCSharp, cancellationToken);
                AddEdits(edits, bridgeEdits);
                break;
            }
        }
    }
    
    private async Task CollectVolarRenamesAsync(
        Dictionary<DocumentUri, List<TextEdit>> edits,
        string uri,
        Position position,
        SymbolIdentity symbol,
        string newName,
        CancellationToken cancellationToken)
    {
        // Volar 重命名
        var volarEdits = await _volarLane.RenameAsync(uri, position, newName, cancellationToken);
        AddEdits(edits, volarEdits);
        
        // Bridge 重命名：.jazor 中的引用
        var bridgeEdits = await _bridgeProvider.GetRenameEditsAsync(
            symbol, newName, BridgeDirection.FrontendToJazor, cancellationToken);
        AddEdits(edits, bridgeEdits);
    }
    
    private async Task CollectRoslynRenamesAsync(
        Dictionary<DocumentUri, List<TextEdit>> edits,
        string uri,
        Position position,
        SymbolIdentity symbol,
        string newName,
        CancellationToken cancellationToken)
    {
        // Roslyn 重命名
        var roslynEdits = await _roslynLane.RenameAsync(uri, position, newName, cancellationToken);
        AddEdits(edits, roslynEdits);
        
        // Bridge 重命名：.jazor 模板中的引用
        var bridgeEdits = await _bridgeProvider.GetRenameEditsAsync(
            symbol, newName, BridgeDirection.CSharpToJazor, cancellationToken);
        AddEdits(edits, bridgeEdits);
    }
    
    private List<(DocumentUri Uri, TextEdit Edit)> MapEditsToSource(
        DocumentUri targetUri,
        IReadOnlyList<TextEdit> targetEdits)
    {
        var result = new List<(DocumentUri, TextEdit)>();
        
        foreach (var edit in targetEdits)
        {
            var sourcePosition = _projectionMap.MapToSource(targetUri, edit.Range.Start);
            if (sourcePosition != null)
            {
                result.Add((sourcePosition.SourceUri, new TextEdit
                {
                    Range = sourcePosition.Range,
                    NewText = edit.NewText
                }));
            }
        }
        
        return result;
    }
    
    private static void AddEdits(
        Dictionary<DocumentUri, List<TextEdit>> edits,
        IEnumerable<(DocumentUri Uri, TextEdit Edit)> newEdits)
    {
        foreach (var (uri, edit) in newEdits)
        {
            if (!edits.TryGetValue(uri, out var list))
            {
                list = new List<TextEdit>();
                edits[uri] = list;
            }
            list.Add(edit);
        }
    }
    
    private static bool IsReadonlySymbol(SymbolIdentity symbol)
    {
        // 内置类型、关键字等不可重命名
        return symbol.Kind == SymbolKind.Keyword ||
               symbol.Kind == SymbolKind.BuiltinType;
    }
    
    private static bool IsExternalComponent(SymbolIdentity symbol)
    {
        // node_modules 或外部库的组件不可重命名
        return symbol.Definition.Uri.Contains("node_modules");
    }
}

public sealed record PrepareRenameResult
{
    public bool CanRename { get; init; }
    public Range? Range { get; init; }
    public string? Placeholder { get; init; }
    public string? ErrorMessage { get; init; }
}
```

### 4.3 CodeActionCoordinator

```csharp
// Lsp/Coordinators/CodeActionCoordinator.cs
namespace Jazor.VueHost.Lsp.Coordinators;

/// <summary>
/// 代码操作协调器，负责跨 Lane CodeAction 聚合
/// </summary>
public sealed class CodeActionCoordinator
{
    private readonly LspSession _session;
    private readonly ProjectionMap _projectionMap;
    private readonly RoslynLane _roslynLane;
    private readonly VolarLane _volarLane;
    private readonly IEnumerable<ICodeActionProvider> _providers;
    
    public async Task<CommandOrCodeAction[]> GetCodeActionsAsync(
        CodeActionParams parameters,
        CancellationToken cancellationToken)
    {
        var uri = parameters.TextDocument.Uri;
        var range = parameters.Range;
        var context = parameters.Context;
        
        var results = new List<CommandOrCodeAction>();
        
        // 1. 内置 CodeAction 提供者
        foreach (var provider in _providers)
        {
            var actions = await provider.ProvideAsync(uri, range, context, cancellationToken);
            results.AddRange(actions);
        }
        
        // 2. 根据文件类型获取 Lane 提供的 CodeAction
        if (uri.EndsWith(".jazor", StringComparison.OrdinalIgnoreCase))
        {
            results.AddRange(await GetJazorCodeActionsAsync(uri, range, context, cancellationToken));
        }
        else if (uri.EndsWith(".vue", StringComparison.OrdinalIgnoreCase) ||
                 uri.EndsWith(".ts", StringComparison.OrdinalIgnoreCase))
        {
            results.AddRange(await GetVolarCodeActionsAsync(uri, range, context, cancellationToken));
        }
        else if (uri.EndsWith(".cs", StringComparison.OrdinalIgnoreCase))
        {
            results.AddRange(await GetRoslynCodeActionsAsync(uri, range, context, cancellationToken));
        }
        
        // 3. 过滤只包含请求的类型
        if (context.Only != null && context.Only.Length > 0)
        {
            results = results.Where(a => MatchesKind(a, context.Only)).ToList();
        }
        
        return results.ToArray();
    }
    
    public async Task<WorkspaceEdit?> ResolveCodeActionAsync(
        CodeAction codeAction,
        CancellationToken cancellationToken)
    {
        // 解析 CodeAction 的编辑
        if (codeAction.Data is string data)
        {
            // 根据数据解析实际编辑
            return await ResolveCodeActionDataAsync(data, cancellationToken);
        }
        
        return codeAction.Edit;
    }
    
    private async Task<List<CommandOrCodeAction>> GetJazorCodeActionsAsync(
        string uri,
        Range range,
        CodeActionContext context,
        CancellationToken cancellationToken)
    {
        var results = new List<CommandOrCodeAction>();
        
        // 确定范围涉及的 Lane
        var projections = _projectionMap.GetProjectionsInRange(uri, range);
        
        foreach (var projection in projections.DistinctBy(p => p.TargetLane))
        {
            switch (projection.TargetLane)
            {
                case LaneKind.Roslyn:
                {
                    // 映射范围到投影
                    var targetRange = _projectionMap.MapRangeToTarget(uri, range, projection.TargetUri);
                    if (targetRange != null)
                    {
                        var roslynActions = await _roslynLane.GetCodeActionsAsync(
                            projection.TargetUri, targetRange, context, cancellationToken);
                        
                        // 映射回 .jazor
                        results.AddRange(MapCodeActionsToSource(roslynActions, projection.TargetUri));
                    }
                    break;
                }
                
                case LaneKind.Volar:
                {
                    var volarActions = await _volarLane.GetCodeActionsAsync(
                        uri, range, context, cancellationToken);
                    results.AddRange(volarActions);
                    break;
                }
            }
        }
        
        // Jazor 特有的 CodeAction
        results.AddRange(await GetJazorSpecificCodeActionsAsync(uri, range, context, cancellationToken));
        
        return results;
    }
    
    private async Task<List<CommandOrCodeAction>> GetJazorSpecificCodeActionsAsync(
        string uri,
        Range range,
        CodeActionContext context,
        CancellationToken cancellationToken)
    {
        var results = new List<CommandOrCodeAction>();
        
        // 示例：添加缺失的导入
        // 示例：提取组件
        // 示例：生成 @code 属性
        
        // TODO: 实现具体的 Jazor CodeAction
        
        return results;
    }
    
    private Task<List<CommandOrCodeAction>> GetVolarCodeActionsAsync(
        string uri, Range range, CodeActionContext context, CancellationToken cancellationToken)
    {
        return _volarLane.GetCodeActionsAsync(uri, range, context, cancellationToken)
            .ContinueWith(t => t.Result.ToList(), cancellationToken);
    }
    
    private Task<List<CommandOrCodeAction>> GetRoslynCodeActionsAsync(
        string uri, Range range, CodeActionContext context, CancellationToken cancellationToken)
    {
        return _roslynLane.GetCodeActionsAsync(uri, range, context, cancellationToken)
            .ContinueWith(t => t.Result.ToList(), cancellationToken);
    }
    
    private IEnumerable<CommandOrCodeAction> MapCodeActionsToSource(
        IEnumerable<CommandOrCodeAction> actions,
        DocumentUri targetUri)
    {
        foreach (var action in actions)
        {
            if (action.CodeAction?.Edit?.Changes != null)
            {
                var mappedEdits = new Dictionary<DocumentUri, TextEdit[]>();
                
                foreach (var (uri, edits) in action.CodeAction.Edit.Changes)
                {
                    var mappedEditList = new List<TextEdit>();
                    foreach (var edit in edits)
                    {
                        var sourcePosition = _projectionMap.MapToSource(uri, edit.Range.Start);
                        if (sourcePosition != null)
                        {
                            mappedEditList.Add(new TextEdit
                            {
                                Range = sourcePosition.Range,
                                NewText = edit.NewText
                            });
                        }
                    }
                    if (mappedEditList.Count > 0)
                    {
                        mappedEdits[sourcePosition!.SourceUri] = mappedEditList.ToArray();
                    }
                }
                
                yield return new CommandOrCodeAction(new CodeAction
                {
                    Title = action.CodeAction.Title,
                    Kind = action.CodeAction.Kind,
                    Edit = new WorkspaceEdit { Changes = mappedEdits }
                });
            }
            else
            {
                yield return action;
            }
        }
    }
    
    private static bool MatchesKind(CommandOrCodeAction action, CodeActionKind[] kinds)
    {
        var kind = action.CodeAction?.Kind ?? CodeActionKind.QuickFix;
        return kinds.Any(k => kind.ToString().StartsWith(k.ToString()));
    }
    
    private Task<WorkspaceEdit?> ResolveCodeActionDataAsync(string data, CancellationToken cancellationToken)
    {
        // 解析序列化的 CodeAction 数据
        // TODO: 实现解析逻辑
        return Task.FromResult<WorkspaceEdit?>(null);
    }
}

/// <summary>
/// CodeAction 提供者接口
/// </summary>
public interface ICodeActionProvider
{
    Task<IReadOnlyList<CommandOrCodeAction>> ProvideAsync(
        DocumentUri uri,
        Range range,
        CodeActionContext context,
        CancellationToken cancellationToken);
}
```

### 4.4 SymbolIdentityResolver

```csharp
// Lsp/Coordinators/SymbolIdentityResolver.cs
namespace Jazor.VueHost.Lsp.Coordinators;

/// <summary>
/// 跨 Lane 符号身份解析器
/// </summary>
public sealed class SymbolIdentityResolver
{
    private readonly LspSession _session;
    private readonly ProjectionMap _projectionMap;
    private readonly RoslynLane _roslynLane;
    private readonly VolarLane _volarLane;
    private readonly MarkupBridgeSymbolCache _bridgeSymbolCache;
    
    /// <summary>
    /// 解析指定位置的符号身份
    /// </summary>
    public async Task<SymbolIdentity?> ResolveAsync(
        DocumentUri uri,
        Position position,
        CancellationToken cancellationToken)
    {
        // 1. 检查是否是 Bridge 符号
        var bridgeSymbol = await TryResolveBridgeSymbolAsync(uri, position, cancellationToken);
        if (bridgeSymbol != null)
            return bridgeSymbol;
        
        // 2. 根据文件类型解析
        if (uri.EndsWith(".jazor", StringComparison.OrdinalIgnoreCase))
        {
            return await ResolveJazorSymbolAsync(uri, position, cancellationToken);
        }
        else if (uri.EndsWith(".vue", StringComparison.OrdinalIgnoreCase) ||
                 uri.EndsWith(".ts", StringComparison.OrdinalIgnoreCase))
        {
            return await ResolveVolarSymbolAsync(uri, position, cancellationToken);
        }
        else if (uri.EndsWith(".cs", StringComparison.OrdinalIgnoreCase))
        {
            return await ResolveRoslynSymbolAsync(uri, position, cancellationToken);
        }
        
        return null;
    }
    
    private async Task<SymbolIdentity?> TryResolveBridgeSymbolAsync(
        DocumentUri uri,
        Position position,
        CancellationToken cancellationToken)
    {
        // 检查 MarkupBridgeSymbol 缓存
        var bridgeSymbol = _bridgeSymbolCache.FindByPosition(uri, position);
        if (bridgeSymbol != null)
        {
            return new SymbolIdentity
            {
                Name = bridgeSymbol.Name,
                Kind = bridgeSymbol.Kind,
                Definition = bridgeSymbol.Definition,
                SourceLane = LaneKind.Jazor, // Bridge 符号源自 Jazor
                BridgeId = bridgeSymbol.BridgeId
            };
        }
        
        return null;
    }
    
    private async Task<SymbolIdentity?> ResolveJazorSymbolAsync(
        DocumentUri uri,
        Position position,
        CancellationToken cancellationToken)
    {
        var projection = _projectionMap.MapToTarget(uri, position);
        if (projection == null)
            return null;
        
        switch (projection.TargetLane)
        {
            case LaneKind.Roslyn:
            {
                var roslynSymbol = await _roslynLane.GetSymbolAtPositionAsync(
                    projection.TargetUri, projection.TargetPosition, cancellationToken);
                
                if (roslynSymbol != null)
                {
                    return new SymbolIdentity
                    {
                        Name = roslynSymbol.Name,
                        Kind = MapRoslynSymbolKind(roslynSymbol.Kind),
                        Definition = new Location
                        {
                            Uri = uri,
                            Range = projection.SourceRange
                        },
                        SourceLane = LaneKind.Roslyn
                    };
                }
                break;
            }
            
            case LaneKind.Volar:
            {
                var volarSymbol = await _volarLane.GetSymbolAtPositionAsync(
                    uri, position, cancellationToken);
                
                if (volarSymbol != null)
                {
                    return new SymbolIdentity
                    {
                        Name = volarSymbol.Name,
                        Kind = MapVolarSymbolKind(volarSymbol.Kind),
                        Definition = volarSymbol.Definition,
                        SourceLane = LaneKind.Volar
                    };
                }
                break;
            }
        }
        
        return null;
    }
    
    private Task<SymbolIdentity?> ResolveVolarSymbolAsync(
        DocumentUri uri,
        Position position,
        CancellationToken cancellationToken)
    {
        return _volarLane.GetSymbolAtPositionAsync(uri, position, cancellationToken)
            .ContinueWith(t =>
            {
                var symbol = t.Result;
                return symbol == null ? null : new SymbolIdentity
                {
                    Name = symbol.Name,
                    Kind = MapVolarSymbolKind(symbol.Kind),
                    Definition = symbol.Definition,
                    SourceLane = LaneKind.Volar
                };
            }, cancellationToken);
    }
    
    private Task<SymbolIdentity?> ResolveRoslynSymbolAsync(
        DocumentUri uri,
        Position position,
        CancellationToken cancellationToken)
    {
        return _roslynLane.GetSymbolAtPositionAsync(uri, position, cancellationToken)
            .ContinueWith(t =>
            {
                var symbol = t.Result;
                return symbol == null ? null : new SymbolIdentity
                {
                    Name = symbol.Name,
                    Kind = MapRoslynSymbolKind(symbol.Kind),
                    Definition = new Location
                    {
                        Uri = uri,
                        Range = symbol.DefinitionSpan
                    },
                    SourceLane = LaneKind.Roslyn
                };
            }, cancellationToken);
    }
    
    private static SymbolKind MapRoslynSymbolKind(Microsoft.CodeAnalysis.SymbolKind kind)
    {
        return kind switch
        {
            Microsoft.CodeAnalysis.SymbolKind.Method => SymbolKind.Method,
            Microsoft.CodeAnalysis.SymbolKind.Property => SymbolKind.Property,
            Microsoft.CodeAnalysis.SymbolKind.Field => SymbolKind.Field,
            Microsoft.CodeAnalysis.SymbolKind.Event => SymbolKind.Event,
            Microsoft.CodeAnalysis.SymbolKind.NamedType => SymbolKind.Class,
            Microsoft.CodeAnalysis.SymbolKind.Parameter => SymbolKind.Parameter,
            Microsoft.CodeAnalysis.SymbolKind.Local => SymbolKind.Variable,
            _ => SymbolKind.Variable
        };
    }
    
    private static SymbolKind MapVolarSymbolKind(string kind)
    {
        return kind.ToLowerInvariant() switch
        {
            "function" => SymbolKind.Function,
            "class" => SymbolKind.Class,
            "interface" => SymbolKind.Interface,
            "variable" => SymbolKind.Variable,
            "constant" => SymbolKind.Constant,
            "property" => SymbolKind.Property,
            "method" => SymbolKind.Method,
            "component" => SymbolKind.Component,
            _ => SymbolKind.Variable
        };
    }
}

public enum SymbolKind
{
    File,
    Module,
    Namespace,
    Package,
    Class,
    Method,
    Property,
    Field,
    Constructor,
    Enum,
    Interface,
    Function,
    Variable,
    Constant,
    String,
    Number,
    Boolean,
    Array,
    Object,
    Key,
    Null,
    EnumMember,
    Struct,
    Event,
    Operator,
    TypeParameter,
    Component,
    Parameter,
    BuiltinType,
    Keyword
}
```

### 4.5 BridgeSupplementProvider

```csharp
// Lsp/Bridge/BridgeSupplementProvider.cs
namespace Jazor.VueHost.Lsp.Bridge;

/// <summary>
/// Bridge 补充提供者，负责跨 Lane 的符号引用和重命名补充
/// </summary>
public sealed class BridgeSupplementProvider
{
    private readonly MarkupBridgeSymbolCache _bridgeSymbolCache;
    private readonly VolarLane _volarLane;
    private readonly RoslynLane _roslynLane;
    
    /// <summary>
    /// 查找 Bridge 引用
    /// </summary>
    public async Task<List<Location>> FindBridgeReferencesAsync(
        SymbolIdentity symbol,
        BridgeDirection direction,
        CancellationToken cancellationToken)
    {
        var results = new List<Location>();
        
        switch (direction)
        {
            case BridgeDirection.CSharpToFrontend:
            {
                // C# 变量 → Vue 模板中的使用
                var bridgeSymbols = _bridgeSymbolCache.FindByCSharpSymbol(symbol.Name);
                foreach (var bridge in bridgeSymbols)
                {
                    results.AddRange(bridge.GetReferences());
                }
                break;
            }
            
            case BridgeDirection.FrontendToCSharp:
            {
                // Vue 组件/变量 → C# @code 中的定义
                var bridgeSymbols = _bridgeSymbolCache.FindByFrontendSymbol(symbol.Name);
                foreach (var bridge in bridgeSymbols)
                {
                    if (bridge.CSharpDefinition != null)
                        results.Add(bridge.CSharpDefinition);
                }
                break;
            }
            
            case BridgeDirection.CSharpToJazor:
            {
                // C# 定义 → .jazor 模板中的引用
                var bridgeSymbols = _bridgeSymbolCache.FindByCSharpSymbol(symbol.Name);
                foreach (var bridge in bridgeSymbols)
                {
                    results.AddRange(bridge.GetJazorReferences());
                }
                break;
            }
            
            case BridgeDirection.FrontendToJazor:
            {
                // Vue 组件 → .jazor 中的使用
                var bridgeSymbols = _bridgeSymbolCache.FindByFrontendSymbol(symbol.Name);
                foreach (var bridge in bridgeSymbols)
                {
                    results.AddRange(bridge.GetJazorReferences());
                }
                break;
            }
        }
        
        return results;
    }
    
    /// <summary>
    /// 获取 Bridge 重命名编辑
    /// </summary>
    public async Task<List<(DocumentUri Uri, TextEdit Edit)>> GetRenameEditsAsync(
        SymbolIdentity symbol,
        string newName,
        BridgeDirection direction,
        CancellationToken cancellationToken)
    {
        var results = new List<(DocumentUri, TextEdit)>();
        
        var bridgeSymbols = direction switch
        {
            BridgeDirection.CSharpToFrontend => 
                _bridgeSymbolCache.FindByCSharpSymbol(symbol.Name),
            BridgeDirection.FrontendToCSharp => 
                _bridgeSymbolCache.FindByFrontendSymbol(symbol.Name),
            BridgeDirection.CSharpToJazor => 
                _bridgeSymbolCache.FindByCSharpSymbol(symbol.Name),
            BridgeDirection.FrontendToJazor => 
                _bridgeSymbolCache.FindByFrontendSymbol(symbol.Name),
            _ => Array.Empty<MarkupBridgeSymbol>()
        };
        
        foreach (var bridge in bridgeSymbols)
        {
            var edits = bridge.GetRenameEdits(newName, direction);
            results.AddRange(edits);
        }
        
        return results;
    }
}
```

### 4.6 ProjectionMap 增强

```csharp
// Lsp/Projection/ProjectionMap.cs (增强)
namespace Jazor.VueHost.Lsp.Projection;

/// <summary>
/// 投影映射表，支持段级双向位置映射
/// </summary>
public sealed class ProjectionMap
{
    private readonly Dictionary<DocumentUri, List<ProjectionMapEntry>> _sourceToTarget = new();
    private readonly Dictionary<DocumentUri, List<ProjectionMapEntry>> _targetToSource = new();
    
    /// <summary>
    /// 添加映射条目
    /// </summary>
    public void AddEntry(ProjectionMapEntry entry)
    {
        // 正向索引
        if (!_sourceToTarget.TryGetValue(entry.SourceUri, out var sourceList))
        {
            sourceList = new List<ProjectionMapEntry>();
            _sourceToTarget[entry.SourceUri] = sourceList;
        }
        sourceList.Add(entry);
        
        // 逆向索引
        if (!_targetToSource.TryGetValue(entry.TargetUri, out var targetList))
        {
            targetList = new List<ProjectionMapEntry>();
            _targetToSource[entry.TargetUri] = targetList;
        }
        targetList.Add(entry);
    }
    
    /// <summary>
    /// 正向映射：源位置 → 目标
    /// </summary>
    public ProjectionMapEntry? MapToTarget(DocumentUri sourceUri, Position sourcePosition)
    {
        if (!_sourceToTarget.TryGetValue(sourceUri, out var entries))
            return null;
        
        // 找到包含该位置的最精确条目
        ProjectionMapEntry? bestMatch = null;
        var bestLength = int.MaxValue;
        
        foreach (var entry in entries)
        {
            if (entry.SourceRange.Contains(sourcePosition))
            {
                var length = entry.SourceRange.Length;
                if (length < bestLength)
                {
                    bestLength = length;
                    bestMatch = entry;
                }
            }
        }
        
        return bestMatch;
    }
    
    /// <summary>
    /// 逆向映射：目标位置 → 源
    /// </summary>
    public MappedPosition? MapToSource(DocumentUri targetUri, Position targetPosition)
    {
        if (!_targetToSource.TryGetValue(targetUri, out var entries))
            return null;
        
        foreach (var entry in entries)
        {
            if (entry.TargetRange.Contains(targetPosition))
            {
                // 计算偏移
                var offset = targetPosition - entry.TargetRange.Start;
                var sourcePosition = entry.SourceRange.Start + offset;
                
                return new MappedPosition
                {
                    SourceUri = entry.SourceUri,
                    Range = new Range
                    {
                        Start = sourcePosition,
                        End = sourcePosition // 单点
                    },
                    SourceRange = entry.SourceRange
                };
            }
        }
        
        return null;
    }
    
    /// <summary>
    /// 获取范围内的所有投影
    /// </summary>
    public IReadOnlyList<ProjectionMapEntry> GetProjectionsInRange(
        DocumentUri sourceUri,
        Range range)
    {
        if (!_sourceToTarget.TryGetValue(sourceUri, out var entries))
            return Array.Empty<ProjectionMapEntry>();
        
        return entries
            .Where(e => e.SourceRange.Overlaps(range))
            .ToList();
    }
    
    /// <summary>
    /// 映射范围到目标
    /// </summary>
    public Range? MapRangeToTarget(
        DocumentUri sourceUri,
        Range sourceRange,
        DocumentUri targetUri)
    {
        var entries = GetProjectionsInRange(sourceUri, sourceRange);
        var targetEntry = entries.FirstOrDefault(e => e.TargetUri == targetUri);
        
        if (targetEntry == null)
            return null;
        
        // 计算目标范围
        var startOffset = sourceRange.Start - targetEntry.SourceRange.Start;
        var endOffset = sourceRange.End - targetEntry.SourceRange.Start;
        
        return new Range
        {
            Start = targetEntry.TargetRange.Start + startOffset,
            End = targetEntry.TargetRange.Start + endOffset
        };
    }
    
    /// <summary>
    /// 清除指定源文件的映射
    /// </summary>
    public void Clear(DocumentUri sourceUri)
    {
        if (_sourceToTarget.TryGetValue(sourceUri, out var entries))
        {
            foreach (var entry in entries)
            {
                if (_targetToSource.TryGetValue(entry.TargetUri, out var targetEntries))
                {
                    targetEntries.RemoveAll(e => e.SourceUri == sourceUri);
                }
            }
            
            _sourceToTarget.Remove(sourceUri);
        }
    }
}

/// <summary>
/// 投影映射条目
/// </summary>
public sealed record ProjectionMapEntry
{
    /// <summary>
    /// 源文件 URI（.jazor）
    /// </summary>
    public required DocumentUri SourceUri { get; init; }
    
    /// <summary>
    /// 源范围
    /// </summary>
    public required Range SourceRange { get; init; }
    
    /// <summary>
    /// 目标文件 URI（投影片段）
    /// </summary>
    public required DocumentUri TargetUri { get; init; }
    
    /// <summary>
    /// 目标范围
    /// </summary>
    public required Range TargetRange { get; init; }
    
    /// <summary>
    /// 目标 Lane
    /// </summary>
    public required LaneKind TargetLane { get; init; }
    
    /// <summary>
    /// 映射类型
    /// </summary>
    public ProjectionKind Kind { get; init; }
}

public enum ProjectionKind
{
    /// <summary>
    /// @code 块 → Roslyn C# 投影
    /// </summary>
    CodeBlock,
    
    /// <summary>
    /// @functions 块 → Roslyn C# 投影
    /// </summary>
    FunctionsBlock,
    
    /// <summary>
    /// 组件标签 → Volar 锚点
    /// </summary>
    ComponentTag,
    
    /// <summary>
    /// 指令 → Volar 锚点
    /// </summary>
    Directive,
    
    /// <summary>
    /// 模板内容 → Volar 锚点
    /// </summary>
    Template
}

public sealed record MappedPosition
{
    public required DocumentUri SourceUri { get; init; }
    public required Range Range { get; init; }
    public Range? SourceRange { get; init; }
}
```

### 4.7 DocumentSymbolHandler

```csharp
// Lsp/Handlers/DocumentSymbolHandler.cs
namespace Jazor.VueHost.Lsp.Handlers;

/// <summary>
/// Document Symbol 处理器
/// </summary>
public sealed class DocumentSymbolHandler
{
    private readonly LspSession _session;
    private readonly ProjectionMap _projectionMap;
    private readonly RoslynLane _roslynLane;
    private readonly VolarLane _volarLane;
    
    public async Task<DocumentSymbol[]> GetDocumentSymbolsAsync(
        DocumentSymbolParams parameters,
        CancellationToken cancellationToken)
    {
        var uri = parameters.TextDocument.Uri;
        var results = new List<DocumentSymbol>();
        
        if (uri.EndsWith(".jazor", StringComparison.OrdinalIgnoreCase))
        {
            return await GetJazorDocumentSymbolsAsync(uri, cancellationToken);
        }
        else if (uri.EndsWith(".vue", StringComparison.OrdinalIgnoreCase))
        {
            return await _volarLane.GetDocumentSymbolsAsync(uri, cancellationToken);
        }
        else if (uri.EndsWith(".ts", StringComparison.OrdinalIgnoreCase) ||
                 uri.EndsWith(".js", StringComparison.OrdinalIgnoreCase))
        {
            return await _volarLane.GetDocumentSymbolsAsync(uri, cancellationToken);
        }
        else if (uri.EndsWith(".cs", StringComparison.OrdinalIgnoreCase))
        {
            return await _roslynLane.GetDocumentSymbolsAsync(uri, cancellationToken);
        }
        
        return Array.Empty<DocumentSymbol>();
    }
    
    private async Task<DocumentSymbol[]> GetJazorDocumentSymbolsAsync(
        DocumentUri uri,
        CancellationToken cancellationToken)
    {
        var symbols = new List<DocumentSymbol>();
        
        // 1. 结构级符号（JazorLane 提供）
        var structureSymbols = await GetStructureSymbolsAsync(uri);
        symbols.AddRange(structureSymbols);
        
        // 2. @code 块符号（RoslynLane 提供）
        var codeBlockSymbols = await GetCodeBlockSymbolsAsync(uri, cancellationToken);
        symbols.AddRange(codeBlockSymbols);
        
        // 3. 模板符号（VolarLane 提供）
        var templateSymbols = await GetTemplateSymbolsAsync(uri, cancellationToken);
        symbols.AddRange(templateSymbols);
        
        // 构建层级结构
        return BuildSymbolTree(symbols);
    }
    
    private async Task<List<DocumentSymbol>> GetStructureSymbolsAsync(DocumentUri uri)
    {
        // 解析 .jazor 文件结构
        var document = await _session.GetDocumentAsync(uri);
        if (document == null)
            return new List<DocumentSymbol>();
        
        var symbols = new List<DocumentSymbol>();
        
        // @code 块
        var codeBlocks = document.GetCodeBlocks();
        foreach (var block in codeBlocks)
        {
            symbols.Add(new DocumentSymbol
            {
                Name = "@code",
                Kind = SymbolKind.Module,
                Range = block.Range,
                SelectionRange = block.Range,
                Children = new List<DocumentSymbol>()
            });
        }
        
        // @functions 块
        var functionsBlocks = document.GetFunctionsBlocks();
        foreach (var block in functionsBlocks)
        {
            symbols.Add(new DocumentSymbol
            {
                Name = "@functions",
                Kind = SymbolKind.Module,
                Range = block.Range,
                SelectionRange = block.Range,
                Children = new List<DocumentSymbol>()
            });
        }
        
        // 组件标签
        var components = document.GetComponents();
        foreach (var component in components)
        {
            symbols.Add(new DocumentSymbol
            {
                Name = component.TagName,
                Kind = SymbolKind.Class,
                Range = component.Range,
                SelectionRange = component.Range
            });
        }
        
        return symbols;
    }
    
    private async Task<List<DocumentSymbol>> GetCodeBlockSymbolsAsync(
        DocumentUri uri,
        CancellationToken cancellationToken)
    {
        var symbols = new List<DocumentSymbol>();
        
        // 获取投影到 Roslyn 的条目
        var projections = _projectionMap.GetProjectionsBySource(uri, LaneKind.Roslyn);
        
        foreach (var projection in projections)
        {
            var roslynSymbols = await _roslynLane.GetDocumentSymbolsAsync(
                projection.TargetUri, cancellationToken);
            
            // 映射回源文档
            foreach (var symbol in roslynSymbols)
            {
                var mappedSymbol = MapSymbolToSource(symbol, projection);
                if (mappedSymbol != null)
                    symbols.Add(mappedSymbol);
            }
        }
        
        return symbols;
    }
    
    private async Task<List<DocumentSymbol>> GetTemplateSymbolsAsync(
        DocumentUri uri,
        CancellationToken cancellationToken)
    {
        // VolarLane 提供模板符号
        var volarSymbols = await _volarLane.GetDocumentSymbolsAsync(uri, cancellationToken);
        return volarSymbols.ToList();
    }
    
    private DocumentSymbol? MapSymbolToSource(DocumentSymbol symbol, ProjectionMapEntry projection)
    {
        // 映射范围
        var sourceStart = _projectionMap.MapToSource(projection.TargetUri, symbol.Range.Start);
        var sourceEnd = _projectionMap.MapToSource(projection.TargetUri, symbol.Range.End);
        
        if (sourceStart == null || sourceEnd == null)
            return null;
        
        return new DocumentSymbol
        {
            Name = symbol.Name,
            Kind = symbol.Kind,
            Range = new Range
            {
                Start = sourceStart.Range.Start,
                End = sourceEnd.Range.Start
            },
            SelectionRange = symbol.SelectionRange,
            Children = symbol.Children?.Select(c => MapSymbolToSource(c, projection))
                .Where(c => c != null)
                .Cast<DocumentSymbol>()
                .ToList()
        };
    }
    
    private static DocumentSymbol[] BuildSymbolTree(List<DocumentSymbol> symbols)
    {
        // 按范围构建层级
        var roots = new List<DocumentSymbol>();
        var sorted = symbols.OrderBy(s => s.Range.Start.Line)
                           .ThenBy(s => s.Range.Start.Character)
                           .ToList();
        
        foreach (var symbol in sorted)
        {
            // 查找父节点
            var parent = FindParent(roots, symbol);
            if (parent != null)
            {
                parent.Children ??= new List<DocumentSymbol>();
                parent.Children.Add(symbol);
            }
            else
            {
                roots.Add(symbol);
            }
        }
        
        return roots.ToArray();
    }
    
    private static DocumentSymbol? FindParent(List<DocumentSymbol> candidates, DocumentSymbol symbol)
    {
        foreach (var candidate in candidates)
        {
            if (candidate.Range.Contains(symbol.Range) && candidate.Range != symbol.Range)
            {
                var child = FindParent(candidate.Children ?? new List<DocumentSymbol>(), symbol);
                return child ?? candidate;
            }
        }
        
        return null;
    }
}
```

### 4.8 SemanticTokensHandler

```csharp
// Lsp/Handlers/SemanticTokensHandler.cs
namespace Jazor.VueHost.Lsp.Handlers;

/// <summary>
/// Semantic Tokens 处理器
/// </summary>
public sealed class SemanticTokensHandler
{
    private readonly LspSession _session;
    private readonly ProjectionMap _projectionMap;
    private readonly VolarLane _volarLane;
    
    public async Task<SemanticTokens?> GetSemanticTokensAsync(
        SemanticTokensParams parameters,
        CancellationToken cancellationToken)
    {
        var uri = parameters.TextDocument.Uri;
        
        if (uri.EndsWith(".jazor", StringComparison.OrdinalIgnoreCase))
        {
            return await GetJazorSemanticTokensAsync(uri, cancellationToken);
        }
        else if (uri.EndsWith(".vue", StringComparison.OrdinalIgnoreCase) ||
                 uri.EndsWith(".ts", StringComparison.OrdinalIgnoreCase) ||
                 uri.EndsWith(".js", StringComparison.OrdinalIgnoreCase))
        {
            return await _volarLane.GetSemanticTokensAsync(uri, cancellationToken);
        }
        
        return null;
    }
    
    private async Task<SemanticTokens?> GetJazorSemanticTokensAsync(
        DocumentUri uri,
        CancellationToken cancellationToken)
    {
        var tokens = new List<int>();
        var document = await _session.GetDocumentAsync(uri);
        if (document == null)
            return null;
        
        // 1. 结构级 token（指令、标记）
        var structureTokens = GetStructureTokens(document);
        
        // 2. Volar 提供 Vue 模板 token
        var volarTokens = await _volarLane.GetSemanticTokensAsync(uri, cancellationToken);
        
        // 合并 token
        var allTokens = MergeTokens(structureTokens, volarTokens);
        
        return new SemanticTokens
        {
            Data = EncodeTokens(allTokens)
        };
    }
    
    private List<SemanticToken> GetStructureTokens(JazorDocument document)
    {
        var tokens = new List<SemanticToken>();
        
        // @code 指令
        foreach (var directive in document.GetDirectives())
        {
            tokens.Add(new SemanticToken
            {
                Line = directive.Range.Start.Line,
                Column = directive.Range.Start.Character,
                Length = directive.Name.Length,
                TokenType = SemanticTokenType.Keyword,
                Modifiers = SemanticTokenModifiers.Static
            });
        }
        
        // 组件标签
        foreach (var component in document.GetComponents())
        {
            // 开始标签
            tokens.Add(new SemanticToken
            {
                Line = component.OpenTagRange.Start.Line,
                Column = component.OpenTagRange.Start.Character,
                Length = component.TagName.Length,
                TokenType = SemanticTokenType.Class,
                Modifiers = SemanticTokenModifiers.None
            });
            
            // 结束标签
            if (component.CloseTagRange != null)
            {
                tokens.Add(new SemanticToken
                {
                    Line = component.CloseTagRange.Start.Line,
                    Column = component.CloseTagRange.Start.Character,
                    Length = component.TagName.Length,
                    TokenType = SemanticTokenType.Class,
                    Modifiers = SemanticTokenModifiers.None
                });
            }
        }
        
        return tokens;
    }
    
    private List<SemanticToken> MergeTokens(
        List<SemanticToken> left,
        SemanticTokens? right)
    {
        var result = new List<SemanticToken>(left);
        
        if (right?.Data != null)
        {
            var rightTokens = DecodeTokens(right.Data);
            result.AddRange(rightTokens);
        }
        
        return result;
    }
    
    private static int[] EncodeTokens(List<SemanticToken> tokens)
    {
        var data = new List<int>();
        var prevLine = 0;
        var prevColumn = 0;
        
        foreach (var token in tokens.OrderBy(t => t.Line).ThenBy(t => t.Column))
        {
            var deltaLine = token.Line - prevLine;
            var deltaColumn = token.Line == prevLine 
                ? token.Column - prevColumn 
                : token.Column;
            
            data.Add(deltaLine);
            data.Add(deltaColumn);
            data.Add(token.Length);
            data.Add((int)token.TokenType);
            data.Add((int)token.Modifiers);
            
            prevLine = token.Line;
            prevColumn = token.Column;
        }
        
        return data.ToArray();
    }
    
    private static List<SemanticToken> DecodeTokens(int[] data)
    {
        var tokens = new List<SemanticToken>();
        var line = 0;
        var column = 0;
        
        for (var i = 0; i < data.Length; i += 5)
        {
            line += data[i];
            column = i == 0 || data[i] > 0 ? data[i + 1] : column + data[i + 1];
            
            tokens.Add(new SemanticToken
            {
                Line = line,
                Column = column,
                Length = data[i + 2],
                TokenType = (SemanticTokenType)data[i + 3],
                Modifiers = (SemanticTokenModifiers)data[i + 4]
            });
        }
        
        return tokens;
    }
}

public sealed record SemanticToken
{
    public int Line { get; init; }
    public int Column { get; init; }
    public int Length { get; init; }
    public SemanticTokenType TokenType { get; init; }
    public SemanticTokenModifiers Modifiers { get; init; }
}

public enum SemanticTokenType
{
    Class = 0,
    Enum = 1,
    EnumMember = 2,
    Event = 3,
    Function = 4,
    Interface = 5,
    Keyword = 6,
    Method = 7,
    Module = 8,
    Namespace = 9,
    Property = 10,
    Struct = 11,
    TypeParameter = 12,
    Variable = 13,
    Parameter = 14,
    Component = 15
}

[Flags]
public enum SemanticTokenModifiers
{
    None = 0,
    Static = 1,
    ReadOnly = 2,
    Deprecated = 4,
    Abstract = 8,
    Async = 16,
    Modification = 32,
    Documentation = 64,
    DefaultLibrary = 128
}
```

---

## 五、LSP Handler 实现

### 5.1 ReferencesHandler

```csharp
// Lsp/Handlers/ReferencesHandler.cs
public sealed class ReferencesHandler : IJsonRpcRequestHandler<ReferenceParams, Location[]>
{
    private readonly ReferenceCoordinator _coordinator;
    
    public ReferencesHandler(ReferenceCoordinator coordinator)
    {
        _coordinator = coordinator;
    }
    
    public async Task<Location[]> Handle(
        ReferenceParams parameters,
        CancellationToken cancellationToken)
    {
        return await _coordinator.FindReferencesAsync(parameters, cancellationToken);
    }
}
```

### 5.2 RenameHandler

```csharp
// Lsp/Handlers/RenameHandler.cs
public sealed class RenameHandler : IJsonRpcRequestHandler<RenameParams, WorkspaceEdit?>
{
    private readonly RenameCoordinator _coordinator;
    
    public async Task<WorkspaceEdit?> Handle(
        RenameParams parameters,
        CancellationToken cancellationToken)
    {
        return await _coordinator.RenameAsync(parameters, cancellationToken);
    }
}

public sealed class PrepareRenameHandler : IJsonRpcRequestHandler<TextDocumentPositionParams, PrepareRenameResult>
{
    private readonly RenameCoordinator _coordinator;
    
    public async Task<PrepareRenameResult> Handle(
        TextDocumentPositionParams parameters,
        CancellationToken cancellationToken)
    {
        return await _coordinator.PrepareRenameAsync(
            parameters.TextDocument.Uri,
            parameters.Position,
            cancellationToken);
    }
}
```

### 5.3 CodeActionHandler

```csharp
// Lsp/Handlers/CodeActionHandler.cs
public sealed class CodeActionHandler : 
    IJsonRpcRequestHandler<CodeActionParams, CommandOrCodeAction[]>,
    IJsonRpcRequestHandler<ResolveCodeActionParams, CodeAction>
{
    private readonly CodeActionCoordinator _coordinator;
    
    public async Task<CommandOrCodeAction[]> Handle(
        CodeActionParams parameters,
        CancellationToken cancellationToken)
    {
        return await _coordinator.GetCodeActionsAsync(parameters, cancellationToken);
    }
    
    public async Task<CodeAction> Handle(
        ResolveCodeActionParams parameters,
        CancellationToken cancellationToken)
    {
        var edit = await _coordinator.ResolveCodeActionAsync(
            parameters.CodeAction, cancellationToken);
        
        return parameters.CodeAction with { Edit = edit };
    }
}
```

---

## 六、LSP 能力注册

```csharp
// Lsp/LspCapabilities.cs
public static class LspCapabilities
{
    public static ServerCapabilities GetCapabilities()
    {
        return new ServerCapabilities
        {
            TextDocumentSync = new TextDocumentSyncOptions
            {
                OpenClose = true,
                Change = TextDocumentSyncKind.Incremental
            },
            
            // P1 能力
            CompletionProvider = new CompletionOptions
            {
                TriggerCharacters = new[] { ".", "<", "@", " ", "(" },
                ResolveProvider = true
            },
            HoverProvider = true,
            DefinitionProvider = true,
            DiagnosticProvider = new DiagnosticOptions
            {
                InterFileDependencies = true,
                WorkspaceDiagnostics = false
            },
            
            // P2 能力 (Phase 6)
            ReferencesProvider = new ReferencesOptions
            {
                WorkDoneProgress = true
            },
            RenameProvider = new RenameOptions
            {
                PrepareProvider = true,
                WorkDoneProgress = true
            },
            DocumentSymbolProvider = new DocumentSymbolOptions
            {
                WorkDoneProgress = true,
                Label = "Jazor"
            },
            CodeActionProvider = new CodeActionOptions
            {
                CodeActionKinds = new[]
                {
                    CodeActionKind.QuickFix,
                    CodeActionKind.Refactor,
                    CodeActionKind.RefactorExtract,
                    CodeActionKind.RefactorInline,
                    CodeActionKind.RefactorRewrite,
                    CodeActionKind.Source,
                    CodeActionKind.SourceOrganizeImports
                },
                ResolveProvider = true,
                WorkDoneProgress = true
            },
            
            // P3 能力 (Phase 6 部分)
            SemanticTokensProvider = new SemanticTokensOptions
            {
                Legend = new SemanticTokensLegend
                {
                    TokenTypes = Enum.GetNames(typeof(SemanticTokenType)),
                    TokenModifiers = Enum.GetNames(typeof(SemanticTokenModifiers))
                },
                Full = new SemanticTokensFullOptions { Delta = true },
                Range = true
            }
        };
    }
}
```

---

## 七、实施步骤（严格顺序）

### Step 1: ProjectionMap 增强

**产出文件**:
- 增强 `Lsp/Projection/ProjectionMap.cs` — 添加段级映射支持
- 新增 `Lsp/Projection/ProjectionMapBuilder.cs`

**依赖**: Phase 1 的基础 ProjectionMap

**测试**:
- 段级正向映射正确
- 段级逆向映射正确
- 范围映射正确
- 多段重叠情况处理

**退出标准**: ProjectionMap 支持段级双向映射，单元测试通过。

---

### Step 2: SymbolIdentityResolver

**产出文件**:
- 新增 `Lsp/Coordinators/SymbolIdentityResolver.cs`

**依赖**: Step 1, Phase 1 的 Lane 实现

**测试**:
- 解析 C# 符号
- 解析 Vue 组件符号
- 解析 Bridge 符号

**退出标准**: 可正确识别光标位置的符号身份。

---

### Step 3: ReferenceCoordinator + ReferencesHandler

**产出文件**:
- 新增 `Lsp/Coordinators/ReferenceCoordinator.cs`
- 新增 `Lsp/Handlers/ReferencesHandler.cs`
- 新增 `Lsp/Bridge/BridgeSupplementProvider.cs`

**依赖**: Step 1, Step 2

**测试**:
- .jazor 中 @code 变量的引用查找
- .jazor 中组件标签的引用查找
- 跨 Lane 引用聚合
- Bridge 补充引用

**退出标准**: "查找所有引用" 功能可用，跨 Lane 结果正确。

---

### Step 4: RenameCoordinator + RenameHandler

**产出文件**:
- 新增 `Lsp/Coordinators/RenameCoordinator.cs`
- 新增 `Lsp/Handlers/RenameHandler.cs`
- 新增 `Lsp/Handlers/PrepareRenameHandler.cs`

**依赖**: Step 3

**测试**:
- PrepareRename 验证
- .jazor 中变量重命名
- .jazor 中组件重命名
- 跨 Lane 重命名同步

**退出标准**: 重命名功能可用，所有引用位置同步更新。

---

### Step 5: DocumentSymbolHandler

**产出文件**:
- 新增 `Lsp/Handlers/DocumentSymbolHandler.cs`

**依赖**: Step 1

**测试**:
- .jazor 文件结构符号
- @code 块内符号
- 组件标签符号
- 层级结构正确

**退出标准**: Document Symbol 功能可用，符号层级正确。

---

### Step 6: CodeActionCoordinator + CodeActionHandler

**产出文件**:
- 新增 `Lsp/Coordinators/CodeActionCoordinator.cs`
- 新增 `Lsp/Handlers/CodeActionHandler.cs`
- 新增 `Lsp/Bridge/ICodeActionProvider.cs`

**依赖**: Step 3, Step 4

**测试**:
- Quick Fix 显示
- Refactor 显示
- CodeAction 执行
- 跨 Lane CodeAction

**退出标准**: 代码操作功能可用，Quick Fix 和 Refactor 正确执行。

---

### Step 7: SemanticTokensHandler

**产出文件**:
- 新增 `Lsp/Handlers/SemanticTokensHandler.cs`

**依赖**: VolarLane 语义 token 支持

**测试**:
- .jazor 语义 token
- .vue/.ts 语义 token（转发）
- Token 类型正确
- Token 修饰符正确

**退出标准**: Semantic Tokens 功能可用，语法高亮增强。

---

### Step 8: 能力注册与集成测试

**产出文件**:
- 修改 `Lsp/LspCapabilities.cs`
- 修改 `Program.cs` — 注册 handler

**测试**:
- 完整 LSP 能力声明
- 所有 handler 注册
- 端到端测试

**退出标准**: VS Code 可连接并使用所有 P2 能力。

---

## 八、风险与缓解

| 风险 | 影响 | 缓解措施 |
|------|------|---------|
| **ProjectionMap 精度不足** | 诊断漂移、重命名损坏 | 渐进增强精度；标注置信度 |
| **Bridge 符号身份不一致** | 引用/重命名遗漏 | 统一符号身份模型；缓存一致性检查 |
| **跨 Lane 性能问题** | 响应延迟 | 并行查询；增量计算 |
| **重命名损坏源码** | 用户代码丢失 | 预览模式；撤销支持 |
| **VolarLane 不稳定** | 前端能力丢失 | 静默降级；自动重连 |
| **CodeAction 冲突** | 编辑冲突 | 合并策略；用户确认 |

---

## 九、后续优化方向

### 9.1 Inlay Hints

- 参数名提示
- 类型注解提示
- 隐式导入提示

### 9.2 Call Hierarchy

- 调用者查找
- 被调用者查找
- 跨 Lane 调用链

### 9.3 Type Hierarchy

- 类型继承关系
- 接口实现查找

### 9.4 Folding Range

- @code 块折叠
- 组件标签折叠
- 模板区域折叠

---

**文档维护者**: developerhan  
**最后更新**: 2026-04-15  
**文档版本**: v1.0
