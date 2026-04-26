# 标记桥接 (Markup Bridge)

> Status: 活跃参考
> Positioning: Jazor LSP 的 Razor/Vue 组件标签桥接服务

## 1. 文档定位

本文档描述标记组件桥接服务，用于解析 Razor/Vue 组件标签到 Vue 文件，并提供跨文档的引用查找、重命名和补全功能。

## 目录

- [1-文档定位](#1-文档定位)
- [2-核心类型](#2-核心类型)
- [3-核心算法](#3-核心算法)
- [4-jazorlspdocumentservice](#4-jazorlspdocumentservice)
- [5-线程安全模型](#5-线程安全模型)
- [6-错误处理](#6-错误处理)
- [7-配置选项](#7-配置选项)
- [8-与其他子系统的交互](#8-与其他子系统的交互)
- [9-设计权衡](#9-设计权衡)

**相关文件**：
- `src/Jolt/Lsp/Coordination/MarkupComponentBridgeService.cs` (842行) - 标记组件桥接服务
- `src/Jolt/Lsp/JazorLspDocumentService.cs` (822行) - Jazor LSP 文档服务

## 2. 核心类型

### 2.1 MarkupComponentBridgeService

**文件位置**：`src/Jolt/Lsp/Coordination/MarkupComponentBridgeService.cs`

**职责**：桥接 Razor/Vue 组件标签到 Vue 文件定义

**核心字段**：
```csharp
private readonly IJoltWorkspaceStore _workspaceStore;
```

**核心记录类型**：
```csharp
internal readonly record struct MarkupComponentSymbol(
    string ComponentName,
    LspRange Range);

internal readonly record struct MarkupBridgeSymbol(
    string ComponentName,
    string AbsolutePath,
    string ImportPath);

internal readonly record struct MarkupComponentResolution(
    string ComponentName,
    string AbsolutePath,
    string ImportPath);

internal readonly record struct ResolvedVueComponent(
    string ComponentName,
    string AbsolutePath,
    string ImportPath);

internal readonly record struct ImportedComponentSymbol(
    string LocalName,
    string ImportPath);
```

## 3. 核心算法

### 3.1 组件标签符号查找

**文件位置**：`src/Jolt/Lsp/Coordination/MarkupComponentBridgeService.cs:21-44`

**目的**：在文本中查找光标位置的组件标签符号

**正则表达式**：
```csharp
// JazorMarkupPatterns.ComponentTagPattern
@"</?(?<name>[A-Z][A-Za-z0-9_]*)\b"
```

**实现**：
```csharp
public bool TryFindComponentTagSymbol(string text, LspPosition position, out MarkupComponentSymbol symbol)
{
    var offset = LspProtocolHelpers.GetOffset(text, position);
    foreach (Match match in JazorMarkupPatterns.ComponentTagPattern.Matches(text))
    {
        var group = match.Groups["name"];
        if (offset < group.Index || offset > group.Index + group.Length)
        {
            continue;
        }

        symbol = new MarkupComponentSymbol(
            group.Value,
            new LspRange
            {
                Start = LspProtocolHelpers.GetPosition(text, group.Index),
                End = LspProtocolHelpers.GetPosition(text, group.Index + group.Length)
            });
        return true;
    }

    symbol = default;
    return false;
}
```

### 3.2 组件符号解析

**文件位置**：`src/Jolt/Lsp/Coordination/MarkupComponentBridgeService.cs:46-75`

**目的**：解析组件标签到 Vue 文件定义

**优先级顺序**：
1. **Tracked Nearby**：打开文档中查找 nearby Vue 组件
2. **Nearby**：文件系统 nearby 查找
3. **Tracked**：打开文档中查找绝对路径匹配
4. **Workspace Scan**：工作区扫描（如果允许）

**实现**：
```csharp
public async ValueTask<MarkupBridgeSymbol?> ResolveBridgeSymbolAsync(
    string documentPath,
    string componentName,
    bool allowWorkspaceScan,
    CancellationToken cancellationToken)
{
    var openDocuments = await _workspaceStore.GetOpenDocumentsAsync(cancellationToken);

    // 1. Tracked nearby
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

    // 2. Nearby
    if (JoltWorkspaceResolver.TryResolveNearbyVueComponent(
        documentPath,
        componentName,
        out var componentPath,
        out var importPath))
    {
        return new MarkupBridgeSymbol(componentName, componentPath, importPath);
    }

    // 3. Tracked
    if (JoltWorkspaceResolver.TryResolveTrackedVueComponent(
        documentPath,
        componentName,
        openDocuments,
        out var tracked))
    {
        return new MarkupBridgeSymbol(tracked.ComponentName, tracked.AbsolutePath, tracked.ImportPath);
    }

    // 4. Workspace scan
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

### 3.3 导入组件符号解析

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

**导入绑定解析**：
```csharp
private static IEnumerable<ImportBindingCandidate> EnumerateImportBindings(Group clauseGroup)
{
    var clause = clauseGroup.Value;

    // Default import
    var defaultMatch = Regex.Match(clause, @"^\s*(?<name>[A-Za-z_$][A-Za-z0-9_$]*)");
    if (defaultMatch.Success && defaultMatch.Groups["name"] is { Success: true } defaultGroup)
    {
        yield return new ImportBindingCandidate(
            defaultGroup.Value,
            clauseGroup.Index + defaultGroup.Index,
            clauseGroup.Index + defaultGroup.Index + defaultGroup.Length);
    }

    // Namespace import
    var namespaceMatch = Regex.Match(clause, @"\*\s+as\s+(?<name>[A-Za-z_$][A-Za-z0-9_$]*)");
    if (namespaceMatch.Success && namespaceMatch.Groups["name"] is { Success: true } namespaceGroup)
    {
        yield return new ImportBindingCandidate(
            namespaceGroup.Value,
            clauseGroup.Index + namespaceGroup.Index,
            clauseGroup.Index + namespaceGroup.Index + namespaceGroup.Length);
    }

    // Named imports
    var namedClauseMatch = Regex.Match(clause, @"\{(?<names>[^}]+)\}");
    if (!namedClauseMatch.Success || namedClauseMatch.Groups["names"] is not { Success: true } namesGroup)
    {
        yield break;
    }

    foreach (Match nameMatch in Regex.Matches(
                 namesGroup.Value,
                 @"(?<imported>[A-Za-z_$][A-Za-z0-9_$]*)(?:\s+as\s+(?<local>[A-Za-z_$][A-Za-z0-9_$]*))?"))
    {
        var localGroup = nameMatch.Groups["local"];
        var importedGroup = nameMatch.Groups["imported"];
        var effectiveGroup = localGroup.Success ? localGroup : importedGroup;
        if (!effectiveGroup.Success)
        {
            continue;
        }

        yield return new ImportBindingCandidate(
            effectiveGroup.Value,
            clauseGroup.Index + namesGroup.Index + effectiveGroup.Index,
            clauseGroup.Index + namesGroup.Index + effectiveGroup.Index + effectiveGroup.Length);
    }
}
```

### 3.4 组件标签位置查找

**文件位置**：`src/Jolt/Lsp/Coordination/MarkupComponentBridgeService.cs:424-449`

**目的**：在文档中查找所有匹配的组件标签位置

**实现**：
```csharp
private static IReadOnlyList<LspLocation> FindComponentTagLocations(
    DocumentSnapshot document,
    string componentName)
{
    var locations = new List<LspLocation>();
    foreach (Match match in JazorMarkupPatterns.ComponentTagPattern.Matches(document.Text))
    {
        var group = match.Groups["name"];
        if (!string.Equals(group.Value, componentName, StringComparison.Ordinal))
        {
            continue;
        }

        locations.Add(new LspLocation
        {
            Uri = LspProtocolHelpers.ToDocumentUri(document.DocumentPath),
            Range = new LspRange
            {
                Start = LspProtocolHelpers.GetPosition(document.Text, group.Index),
                End = LspProtocolHelpers.GetPosition(document.Text, group.Index + group.Length)
            }
        });
    }

    return locations;
}
```

### 3.5 候选文档收集

**文件位置**：`src/Jolt/Lsp/Coordination/MarkupComponentBridgeService.cs:363-422`

**目的**：收集引用查找的候选文档

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

    // 1. 打开的 Jazor 文档
    foreach (var openDocument in openDocuments.Where(static candidate => candidate.DocumentKind == DocumentKind.Jazor))
    {
        AddDocumentCandidate(openDocument, documents, seen);
    }

    // 2. 当前文档
    if (document.DocumentKind is DocumentKind.Jazor or DocumentKind.Vue)
    {
        AddDocumentCandidate(document, documents, seen);
    }

    // 3. 工作区扫描
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

**文件系统扫描**：
```csharp
private static async ValueTask AddDocumentsFromDirectoryAsync(
    string directory,
    string searchPattern,
    IReadOnlyList<DocumentSnapshot> openDocuments,
    List<DocumentSnapshot> documents,
    HashSet<string> seen,
    CancellationToken cancellationToken)
{
    if (!Directory.Exists(directory))
    {
        return;
    }

    var openDocumentsByPath = openDocuments
        .GroupBy(
            static document => JoltWorkspaceResolver.NormalizePath(document.DocumentPath),
            StringComparer.OrdinalIgnoreCase)
        .ToDictionary(
            static group => group.Key,
            static group => group.First(),
            StringComparer.OrdinalIgnoreCase);

    foreach (var filePath in JoltWorkspaceResolver.EnumerateWorkspaceFiles(
        new[] { directory },
        searchPattern,
        cancellationToken))
    {
        cancellationToken.ThrowIfCancellationRequested();
        var normalizedPath = JoltWorkspaceResolver.NormalizePath(filePath);

        // 优先使用打开文档
        if (openDocumentsByPath.TryGetValue(normalizedPath, out var openDocument))
        {
            AddDocumentCandidate(openDocument, documents, seen);
            continue;
        }

        // 跳过已处理文档
        if (seen.Contains(normalizedPath))
        {
            continue;
        }

        // 读取磁盘文档
        try
        {
            var documentKind = JoltWorkspaceResolver.MapDocumentKind(filePath);
            if (documentKind is not (DocumentKind.Jazor or DocumentKind.Vue))
            {
                continue;
            }

            documents.Add(new DocumentSnapshot(
                normalizedPath,
                documentKind,
                await File.ReadAllTextAsync(filePath, cancellationToken),
                version: null));
            seen.Add(normalizedPath);
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
    }
}
```

### 3.6 Hover 信息生成

**文件位置**：`src/Jolt/Lsp/Coordination/MarkupComponentBridgeService.cs:272-298`

**目的**：为组件标签生成 Hover 信息

**实现**：
```csharp
public async ValueTask<LspHoverResult?> GetHoverAsync(
    DocumentSnapshot document,
    LspPosition position,
    bool allowWorkspaceScan,
    CancellationToken cancellationToken)
{
    if (!TryFindComponentTagSymbol(document.Text, position, out var symbol))
    {
        return null;
    }

    var resolvedComponent = await ResolveComponentAsync(
        document.DocumentPath,
        symbol.ComponentName,
        allowWorkspaceScan,
        cancellationToken);
    if (resolvedComponent is null)
    {
        return null;
    }

    return new LspHoverResult
    {
        Contents = new LspMarkupContent
        {
            Kind = "markdown",
            Value = $"`{symbol.ComponentName}` resolved from Razor/Volar markup to `{resolvedComponent.Value.ImportPath}`\n\nkind: `VueComponent`"
        },
        Range = symbol.Range
    };
}
```

## 4. JazorLspDocumentService

**文件位置**：`src/Jolt/Lsp/JazorLspDocumentService.cs` (822行)

**职责**：提供 Jazor 文档的 LSP 服务

### 4.1 诊断

**文件位置**：`src/Jolt/Lsp/JazorLspDocumentService.cs:32-63`

**实现**：
```csharp
public async ValueTask<IReadOnlyList<LspDiagnostic>> GetDiagnosticsAsync(
    DocumentSnapshot document,
    CancellationToken cancellationToken)
{
    var response = await AnalyzeAsync(document, cancellationToken);
    var diagnostics = response.Diagnostics
        .Where(diagnostic => string.Equals(
            NormalizeDocumentPath(diagnostic.DocumentPath),
            NormalizeDocumentPath(document.DocumentPath),
            StringComparison.OrdinalIgnoreCase))
        .Select(diagnostic => new LspDiagnostic
        {
            Range = LspProtocolHelpers.ToRange(document.Text, diagnostic.Start, diagnostic.Length),
            Severity = diagnostic.Severity switch
            {
                DiagnosticSeverityKind.Error => 1,
                DiagnosticSeverityKind.Warning => 2,
                _ => 3
            },
            Code = diagnostic.Id,
            Source = "Jolt",
            Message = diagnostic.Message
        })
        .ToList();

    return diagnostics
        .GroupBy(
            static diagnostic => $"{diagnostic.Code}:{GetRangeKey(diagnostic.Range)}:{diagnostic.Message}",
            StringComparer.Ordinal)
        .Select(static group => group.First())
        .ToArray();
}
```

### 4.2 补全

**文件位置**：`src/Jolt/Lsp/JazorLspDocumentService.cs:187-233`

**实现**：
```csharp
private async ValueTask<IReadOnlyList<LspCompletionItem>> GetCompletionItemsCoreAsync(
    DocumentSnapshot document,
    LspPosition position,
    CancellationToken cancellationToken)
{
    cancellationToken.ThrowIfCancellationRequested();

    var offset = LspProtocolHelpers.GetOffset(document.Text, position);
    var prefix = document.Text[..Math.Min(offset, document.Text.Length)];
    var items = new List<LspCompletionItem>();

    if (TryGetTagCompletionPrefix(prefix, out var tagPrefix))
    {
        var seenLabels = new HashSet<string>(items.Select(static item => item.Label), StringComparer.Ordinal);
        foreach (var suggestion in await _markupComponentBridge.GetComponentSuggestionsAsync(
                     document.DocumentPath,
                     allowWorkspaceScan: true,
                     cancellationToken))
        {
            if (!suggestion.ComponentName.StartsWith(tagPrefix, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            if (!seenLabels.Add(suggestion.ComponentName))
            {
                continue;
            }

            items.Add(new LspCompletionItem
            {
                Label = suggestion.ComponentName,
                Kind = 7,
                Detail = suggestion.ImportPath,
                Documentation = $"Vue component available to `.jazor` from `{suggestion.ImportPath}`."
            });
        }
    }

    return items;
}
```

**标签补全前缀检测**：
```csharp
private static bool TryGetTagCompletionPrefix(string prefix, out string tagPrefix)
{
    var match = TagCompletionPrefixPattern.Match(prefix);
    if (!match.Success)
    {
        tagPrefix = string.Empty;
        return false;
    }

    tagPrefix = match.Groups["name"].Value;
    return true;
}

// TagCompletionPrefixPattern: @"</?(?<name>[A-Za-z0-9_]*)$"
```

### 4.3 语义令牌

**文件位置**：`src/Jolt/Lsp/JazorLspDocumentService.cs:235-265`

**实现**：
```csharp
public ValueTask<IReadOnlyList<LspSemanticToken>> GetSemanticTokensAsync(
    DocumentSnapshot document,
    CancellationToken cancellationToken)
{
    cancellationToken.ThrowIfCancellationRequested();

    if (document.DocumentKind != DocumentKind.Jazor)
    {
        return ValueTask.FromResult<IReadOnlyList<LspSemanticToken>>(Array.Empty<LspSemanticToken>());
    }

    var parsed = _parser.Parse(document.DocumentPath, document.Text);
    var tokens = new List<LspSemanticToken>();

    // Template wrapper tags
    AddTemplateWrapperTokens(document.Text, tokens);

    // @code directive keyword
    AddCodeDirectiveTokens(document.Text, tokens);

    // Import directives
    AddImportDirectiveTokens(document.Text, tokens);

    // Component tags (PascalCase) in template region
    if (parsed.TemplateStartIndex >= 0 && parsed.TemplateLength > 0)
    {
        AddComponentTagTokens(document.Text, parsed, tokens);
    }

    return ValueTask.FromResult<IReadOnlyList<LspSemanticToken>>(tokens);
}
```

**组件标签令牌**：
```csharp
private static void AddComponentTagTokens(
    string text,
    JazorVueDocument parsed,
    List<LspSemanticToken> tokens)
{
    foreach (Match match in TagPattern.Matches(parsed.Template))
    {
        var group = match.Groups["name"];
        if (!group.Success)
        {
            continue;
        }

        var sourceIndex = parsed.TemplateStartIndex + group.Index;
        var pos = LspProtocolHelpers.GetPosition(text, sourceIndex);
        tokens.Add(new LspSemanticToken
        {
            Line = pos.Line,
            Character = pos.Character,
            Length = group.Length,
            TokenType = "class",
            TokenModifiers = ["declaration"]
        });
    }
}
```

### 4.4 代码操作

**文件位置**：`src/Jolt/Lsp/JazorLspDocumentService.cs:508-562`

**私有方法修复**：
```csharp
if (TryFindPrivateMethodModifierForDiagnostic(document, diagnostics, out var privateMethodModifier))
{
    actions.Add(new LspCodeAction
    {
        Title = "Make method public for bridge lowering",
        Kind = "quickfix",
        Edit = new LspWorkspaceEdit
        {
            Changes = new Dictionary<string, LspTextEdit[]>
            {
                [LspProtocolHelpers.ToDocumentUri(document.DocumentPath)] =
                [
                    new LspTextEdit
                    {
                        Range = LspProtocolHelpers.ToRange(document.Text, privateMethodModifier.Index, privateMethodModifier.Length),
                        NewText = "public"
                    }
                ]
            }
        }
    });
}
```

**遗留导入指令修复**：
```csharp
var legacyDirectiveDiagnostics = diagnostics
    .Where(static diagnostic => string.Equals(diagnostic.Code, LegacyImportDirectiveCatalog.DiagnosticCode, StringComparison.Ordinal))
    .ToArray();
if (legacyDirectiveDiagnostics.Length > 0)
{
    var rangeKeys = legacyDirectiveDiagnostics
        .Select(static diagnostic => GetRangeKey(diagnostic.Range))
        .ToHashSet(StringComparer.Ordinal);
    actions.AddRange(CreateLegacyImportDirectiveCodeActions(document, rangeKeys));
}
```

## 5. 线程安全模型

### 5.1 MarkupComponentBridgeService

**工作区存储访问**：通过 `IJoltWorkspaceStore`（由实现保证线程安全）

**无共享状态**：所有方法都是独立的，不共享可变状态

### 5.2 JazorLspDocumentService

**字段**：
- `_analysisClient`：只读接口（线程安全）
- `_markupComponentBridge`：只读接口（线程安全）
- `_parser`：无状态类（线程安全）
- `_fallbackAnalysisService`：无状态类（线程安全）

**无共享状态**：所有方法都是独立的，线程安全

## 6. 错误处理

### 6.1 文件系统错误

**策略**：捕获并忽略文件系统异常

**理由**：
- 文件可能被删除或移动
- 不应因为单个文件错误而失败整个操作
- 继续处理其他文件

### 6.2 解析错误

**策略**：返回空值或空集合

**理由**：
- 解析失败不应阻止其他功能
- 上层可以处理空结果

## 7. 配置选项

### 7.1 构造函数参数

**MarkupComponentBridgeService**：
```csharp
public MarkupComponentBridgeService(IJoltWorkspaceStore workspaceStore)
```

**JazorLspDocumentService**：
```csharp
public JazorLspDocumentService(
    IJoltWorkspaceStore workspaceStore,
    IVueAnalysisClient analysisClient,
    MarkupComponentBridgeService? markupComponentBridge = null)
```

### 7.2 工作区扫描控制

**参数**：`allowWorkspaceScan`

**默认值**：
- Jazor 文档：`true`
- Vue 文档：`false`

## 8. 与其他子系统的交互

### 8.1 与工作区存储交互

**用途**：
- 获取打开文档列表
- 确定工作区搜索根目录
- 扫描工作区文件

### 8.2 与分析客户端交互

**JazorLspDocumentService**：
- 分析 Jazor 文档
- 获取诊断信息

### 8.3 与组件桥接服务交互

**JazorLspDocumentService**：
- 组件标签补全
- 组件定义跳转
- 组件引用查找
- 组件重命名

## 9. 设计权衡

### 9.1 组件解析优先级

**选择**：Tracked Nearby > Nearby > Tracked > Workspace Scan

**原因**：
- Tracked nearby 最快且最准确
- Nearby 提供文件系统查找
- Tracked 提供打开文档查找
- Workspace Scan 提供完整查找（但最慢）

**权衡**：
- 优势：性能和完整性的平衡
- 劣势：多次查找（可接受）

### 9.2 JS Trivia Masking

**选择**：实现完整的 JS trivia masking

**原因**：
- 避免注释和字符串干扰 import 解析
- 提供准确的导入绑定解析
- 支持复杂的 import 语句

**权衡**：
- 优势：准确性、完整性
- 劣势：实现复杂度（可接受）

### 9.3 文件系统扫描策略

**选择**：优先使用打开文档，回退到文件系统

**原因**：
- 打开文档提供最新信息
- 文件系统扫描提供完整性
- 去重避免重复处理

**权衡**：
- 优势：性能和完整性的平衡
- 劣势：可能扫描大量文件（可配置）

### 9.4 代码操作生成

**选择**：提供私有方法修复和遗留导入指令修复

**原因**：
- 帮助用户快速修复常见问题
- 提高开发体验
- 支持渐进式迁移

**权衡**：
- 优势：用户体验
- 劣势：额外的代码复杂度（可接受）
