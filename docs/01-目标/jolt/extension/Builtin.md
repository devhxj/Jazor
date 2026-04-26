# 内置扩展

> Status: 活跃参考
> Positioning: Jolt 核心功能扩展，无需清单和加载流程

## 1. 文档定位

本文档描述 Jolt 扩展系统的内置扩展，包括组件代码操作、指令补全、结构诊断和工作区符号索引。内置扩展在编译时静态链接，无需 `extension.json` 清单，运行在主进程中，默认启用。核心实现在 `src/Jolt/Extensions/Builtin/` 目录。

## 2. 核心类型

### 2.1 BuiltinExtensionCatalog 内置扩展目录

**文件位置**: `src/Jolt/Extensions/Builtin/BuiltinExtensionCatalog.cs`

```csharp
internal static class BuiltinExtensionCatalog
{
    public static IReadOnlyList<IExtension> Create()
    {
        return
        [
            new StructureDiagnosticExtension(),
            new DirectiveCompletionExtension(),
            new ComponentCodeActionExtension(),
            new WorkspaceSymbolExtension()
        ];
    }
}
```

**加载时机**: Jolt 启动时通过 `ExtensionLoader.LoadBuiltinExtensionsAsync` 加载

## 3. 核心扩展

### 3.1 ComponentCodeActionExtension 组件代码操作

**文件位置**: `src/Jolt/Extensions/Builtin/ComponentCodeActionExtension.cs`

**元数据**:
```csharp
public ExtensionMetadata Metadata { get; } = new(
    Id: "builtin.component-code-action",
    Name: "Builtin Component Code Action",
    Version: "1.0.0",
    Description: "Provides component import quick fixes for unresolved template components.");
```

**Provider 配置**:
```csharp
public string Name => "BuiltinComponentCodeActionProvider";
public int Priority => 200;
```

**功能**: 为未解析的 Jazor/Vue 组件提供 `@module` 导入快速修复

#### 3.1.1 诊断检测

**触发条件**:
```csharp
private static bool IsMissingComponentDiagnostic(LspDiagnostic diagnostic)
{
    // 方法 1: 检查诊断代码
    if (string.Equals(diagnostic.Code, MissingComponentDiagnosticCode))
        return true;

    // 方法 2: 检查诊断源和消息内容
    if (!string.Equals(diagnostic.Source, "Jolt.Frontend"))
        return false;

    return diagnostic.Message.Contains("component", StringComparison.OrdinalIgnoreCase)
        && diagnostic.Message.Contains("resolve", StringComparison.OrdinalIgnoreCase);
}
```

**诊断代码**: `JAZORVUEFRONTEND001`

#### 3.1.2 组件名解析

**解析策略 1: 模板标签匹配**:
```csharp
private static bool TryResolveComponentName(
    string text,
    LspDiagnostic diagnostic,
    out string componentName)
{
    var diagnosticStartOffset = LspProtocolHelpers.GetOffset(text, diagnostic.Range.Start);
    var diagnosticEndOffset = LspProtocolHelpers.GetOffset(text, diagnostic.Range.End);

    foreach (Match match in JazorMarkupPatterns.ComponentTagPattern.Matches(text))
    {
        var nameGroup = match.Groups["name"];
        if (!nameGroup.Success)
            continue;

        if (RangesOverlap(
                match.Index,
                match.Length,
                diagnosticStartOffset,
                Math.Max(0, diagnosticEndOffset - diagnosticStartOffset)))
        {
            componentName = nameGroup.Value;
            return true;
        }
    }

    // ... 策略 2
}
```

**解析策略 2: 诊断消息引号提取**:
```csharp
private static readonly Regex QuotedComponentNamePattern = new(
    @"['""](?<name>[A-Z][A-Za-z0-9_]*)['""]",
    RegexOptions.Compiled);

var quotedComponentMatch = QuotedComponentNamePattern.Match(diagnostic.Message);
if (quotedComponentMatch.Success)
{
    componentName = quotedComponentMatch.Groups["name"].Value;
    return true;
}
```

#### 3.1.3 导入路径解析

**文件路径解析** (`TryResolveImportPath`):
```csharp
private static bool TryResolveImportPath(
    string documentPath,
    string componentName,
    out string importPath)
{
    // 尝试解析附近 Vue 组件
    if (JoltWorkspaceResolver.TryResolveNearbyVueComponent(
            documentPath,
            componentName,
            out _,
            out importPath))
    {
        return true;
    }

    importPath = string.Empty;
    return false;
}
```

**解析逻辑**: 搜索当前文档目录及父目录的 `{ComponentName}.vue` 文件

#### 3.1.4 导入语句生成

**插入位置确定**:
```csharp
private static (int InsertOffset, string NewText) DetermineInsertion(
    string text,
    string importLine,
    string newline)
{
    var importMatches = JazorImportDirectiveLocator.EnumerateDirectiveLines(text).ToArray();
    if (importMatches.Length == 0)
    {
        // 文件开头插入
        return (0, importLine + newline);
    }

    // 最后一个导入语句后插入
    var lastImportDirective = importMatches[^1];
    return (
        lastImportDirective.LineStartIndex + lastImportDirective.LineLength,
        newline + importLine);
}
```

**代码操作生成**:
```csharp
private static LspCodeAction CreateImportAction(
    DocumentSnapshot document,
    string componentName,
    string importPath)
{
    var text = document.Text;
    var newline = text.Contains("\r\n") ? "\r\n" : "\n";
    var importLine = $"@module {componentName} from \"{importPath}\"";
    var (insertOffset, newText) = DetermineInsertion(text, importLine, newline);

    var uri = LspProtocolHelpers.ToDocumentUri(document.DocumentPath);
    return new LspCodeAction
    {
        Title = $"Add @module for {componentName}",
        Kind = "quickfix",
        Edit = new LspWorkspaceEdit
        {
            Changes = new Dictionary<string, LspTextEdit[]>
            {
                [uri] =
                [
                    new LspTextEdit
                    {
                        Range = LspProtocolHelpers.ToRange(text, insertOffset, length: 0),
                        NewText = newText
                    }
                ]
            }
        }
    };
}
```

**示例输出**:
```csharp
// 输入文档
<div>
    <Button />
</div>

// 代码操作结果
@module Button from "./Button.vue"

<div>
    <Button />
</div>
```

### 3.2 StructureDiagnosticExtension 结构诊断

**文件位置**: `src/Jolt/Extensions/Builtin/StructureDiagnosticExtension.cs`

**功能**: 提供 Jazor/Vue 文件的结构性诊断（如未关闭标签、属性语法错误）

**元数据**:
```csharp
public ExtensionMetadata Metadata { get; } = new(
    Id: "builtin.structure-diagnostic",
    Name: "Builtin Structure Diagnostic",
    Version: "1.0.0",
    Description: "Provides structural diagnostics for Jazor/Vue files.");
```

**Provider 配置**:
```csharp
public string Name => "BuiltinStructureDiagnosticProvider";
public int Priority => 100;
```

**诊断类型**:
- 未关闭标签（`<div>` 缺少 `</div>`）
- 属性语法错误（`@bind="value"` 缺少 `value` 属性）
- 指令语法错误（`@foreach` 缺少 `in` 关键字）

### 3.3 DirectiveCompletionExtension 指令补全

**文件位置**: `src/Jolt/Extensions/Builtin/DirectiveCompletionExtension.cs`

**功能**: 提供 Jazor/Vue 指令的代码补全（如 `@foreach`, `@if`, `@bind`）

**元数据**:
```csharp
public ExtensionMetadata Metadata { get; } = new(
    Id: "builtin.directive-completion",
    Name: "Builtin Directive Completion",
    Version: "1.0.0",
    Description: "Provides code completion for Jazor/Vue directives.");
```

**Provider 配置**:
```csharp
public string Name => "BuiltinDirectiveCompletionProvider";
public int Priority => 1000;
```

**补全项**:
```csharp
@foreach (var item in items)
@if (condition)
@else if (condition)
@else
@bind("value")
@bind:value="Value"
@ref="reference"
@key="key"
@code { ... }
@functions { ... }
```

### 3.4 WorkspaceSymbolExtension 工作区符号

**文件位置**: `src/Jolt/Extensions/Builtin/WorkspaceSymbolExtension.cs`

**功能**: 提供工作区范围的符号搜索（类、方法、组件）

**元数据**:
```csharp
public ExtensionMetadata Metadata { get; } = new(
    Id: "builtin.workspace-symbol",
    Name: "Builtin Workspace Symbol",
    Version: "1.0.0",
    Description: "Provides workspace-wide symbol search for C#, Jazor, Vue, and JavaScript/TypeScript.");
```

**Provider 配置**:
```csharp
public string Name => "BuiltinWorkspaceSymbolProvider";
public int Priority => 100;
```

**符号提取逻辑**:
```csharp
private static IReadOnlyList<LspWorkspaceSymbol> ExtractSymbols(DocumentSnapshot document)
{
    var symbols = new List<LspWorkspaceSymbol>();
    var uri = LspProtocolHelpers.ToDocumentUri(document.DocumentPath);
    var containerName = Path.GetFileNameWithoutExtension(document.DocumentPath);

    switch (document.DocumentKind)
    {
        case DocumentKind.CSharp:
            AddSymbolsFromPattern(document.Text, uri, containerName, CSharpTypePattern, kind: 5, symbols);
            AddSymbolsFromPattern(document.Text, uri, containerName, CSharpMethodPattern, kind: 6, symbols);
            break;

        case DocumentKind.Jazor:
        case DocumentKind.Vue:
            AddSymbolsFromPattern(document.Text, uri, containerName, TagComponentPattern, kind: 5, symbols);
            AddSymbolsFromPattern(document.Text, uri, containerName, CSharpMethodPattern, kind: 6, symbols);
            break;

        case DocumentKind.JavaScript:
        case DocumentKind.TypeScript:
            AddSymbolsFromPattern(document.Text, uri, containerName, JavaScriptExportPattern, kind: 12, symbols);
            break;
    }

    return symbols
        .GroupBy(static symbol => string.Join('|', symbol.Name, symbol.Kind, symbol.Location.Uri, symbol.Location.Range.Start.Line))
        .Select(static group => group.First())
        .ToArray();
}
```

**正则表达式模式**:
```csharp
// C# 类型: class/record/interface/struct Name
private static readonly Regex CSharpTypePattern = new(
    @"\b(class|record|interface|struct)\s+(?<name>[A-Za-z_][A-Za-z0-9_]*)",
    RegexOptions.Compiled);

// C# 方法: public/private/... Name(...)
private static readonly Regex CSharpMethodPattern = new(
    @"\b(?:public|private|protected|internal|static|virtual|override|async|sealed|partial|\s)+[\w<>\[\]\.\?,]+\s+(?<name>[A-Za-z_][A-Za-z0-9_]*)\s*\(",
    RegexOptions.Compiled);

// Vue 组件: <ComponentName>
private static readonly Regex TagComponentPattern = new(
    @"<(?<name>[A-Z][A-Za-z0-9_]*)\b",
    RegexOptions.Compiled);

// JavaScript 导出: export function/class/const Name
private static readonly Regex JavaScriptExportPattern = new(
    @"\bexport\s+(?:default\s+)?(?:async\s+)?(?:function|class|const|let|var)\s+(?<name>[A-Za-z_][A-Za-z0-9_]*)",
    RegexOptions.Compiled);
```

**搜索和过滤**:
```csharp
public IReadOnlyList<LspWorkspaceSymbol> Search(
    string query,
    IReadOnlyList<DocumentSnapshot> openDocuments,
    int maxResults = 256)
{
    Refresh(openDocuments);  // 更新缓存
    var normalizedQuery = query?.Trim() ?? string.Empty;

    List<LspWorkspaceSymbol> symbols;
    lock (_gate)
    {
        symbols = _symbolsByDocumentPath.Values
            .SelectMany(static entry => entry.Symbols)
            .ToList();
    }

    var filtered = string.IsNullOrWhiteSpace(normalizedQuery)
        ? symbols
        : symbols
            .Where(symbol =>
                symbol.Name.Contains(normalizedQuery, StringComparison.OrdinalIgnoreCase)
                || (symbol.ContainerName?.Contains(normalizedQuery, StringComparison.OrdinalIgnoreCase) ?? false))
            .ToList();

    return filtered
        .OrderBy(static symbol => symbol.Name, StringComparer.OrdinalIgnoreCase)
        .ThenBy(static symbol => symbol.Location.Uri, StringComparer.OrdinalIgnoreCase)
        .ThenBy(static symbol => symbol.Location.Range.Start.Line)
        .Take(maxResults)
        .ToArray();
}
```

**缓存机制**:
```csharp
private void Refresh(IReadOnlyList<DocumentSnapshot> openDocuments)
{
    var normalizedOpenPaths = openDocuments
        .Select(static document => Path.GetFullPath(document.DocumentPath))
        .ToHashSet(StringComparer.OrdinalIgnoreCase);

    lock (_gate)
    {
        // 移除已关闭文档的符号
        foreach (var stalePath in _symbolsByDocumentPath.Keys
                     .Where(path => !normalizedOpenPaths.Contains(path))
                     .ToArray())
        {
            _symbolsByDocumentPath.Remove(stalePath);
        }

        // 更新已打开文档的符号
        foreach (var openDocument in openDocuments)
        {
            var fullPath = Path.GetFullPath(openDocument.DocumentPath);
            var fingerprint = CreateFingerprint(openDocument);

            if (_symbolsByDocumentPath.TryGetValue(fullPath, out var existing)
                && string.Equals(existing.Fingerprint, fingerprint, StringComparison.Ordinal))
            {
                continue;  // 未修改，跳过
            }

            _symbolsByDocumentPath[fullPath] = new IndexedDocumentSymbols(
                fullPath,
                fingerprint,
                ExtractSymbols(openDocument));
        }
    }
}
```

**指纹生成**:
```csharp
private static string CreateFingerprint(DocumentSnapshot document)
{
    var version = document.Version?.Trim();
    if (!string.IsNullOrWhiteSpace(version))
        return "version:" + version;

    // 回退到文本哈希
    var textBytes = Encoding.UTF8.GetBytes(document.Text);
    var textHash = Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(textBytes));
    return "text:" + textHash;
}
```

## 4. Provider 接口汇总

**文件位置**: `src/Jolt/Extensions/Lsp*.cs`（11 个文件）

### 4.1 ILspDiagnosticProvider 诊断

```csharp
internal interface ILspDiagnosticProvider
{
    string Name { get; }
    int Priority { get; }
    ValueTask<IReadOnlyList<LspDiagnostic>> ProvideDiagnosticsAsync(
        LspDiagnosticProviderContext context,
        CancellationToken cancellationToken);
}
```

**上下文**:
```csharp
internal sealed record LspDiagnosticProviderContext(
    DocumentSnapshot Document,
    CancellationToken CancellationToken);
```

### 4.2 ILspCodeActionProvider 代码操作

```csharp
internal interface ILspCodeActionProvider
{
    string Name { get; }
    int Priority { get; }
    ValueTask<IReadOnlyList<LspCodeAction>> ProvideCodeActionsAsync(
        LspCodeActionProviderContext context,
        CancellationToken cancellationToken);
}
```

**上下文**:
```csharp
internal sealed record LspCodeActionProviderContext(
    DocumentSnapshot Document,
    IReadOnlyList<LspDiagnostic> Diagnostics,
    LspRange? Selection,
    CancellationToken CancellationToken);
```

### 4.3 ILspHoverProvider 悬停提示

```csharp
internal interface ILspHoverProvider
{
    string Name { get; }
    int Priority { get; }
    ValueTask<LspHoverResult?> ProvideHoverAsync(
        LspHoverProviderContext context,
        CancellationToken cancellationToken);
}
```

**上下文**:
```csharp
internal sealed record LspHoverProviderContext(
    DocumentSnapshot Document,
    LspPosition Position,
    LspHoverResult? ExistingHover,
    CancellationToken CancellationToken);
```

### 4.4 ILspCompletionProvider 补全

```csharp
internal interface ILspCompletionProvider
{
    string Name { get; }
    int Priority { get; }
    ValueTask<IReadOnlyList<LspCompletionItem>> ProvideCompletionItemsAsync(
        LspCompletionProviderContext context,
        CancellationToken cancellationToken);
}
```

**上下文**:
```csharp
internal sealed record LspCompletionProviderContext(
    DocumentSnapshot Document,
    LspPosition Position,
    LspCompletionTriggerKind TriggerKind,
    string TriggerCharacter,
    CancellationToken CancellationToken);
```

### 4.5 ILspDocumentSymbolProvider 文档符号

```csharp
internal interface ILspDocumentSymbolProvider
{
    string Name { get; }
    int Priority { get; }
    ValueTask<IReadOnlyList<LspDocumentSymbol>> ProvideDocumentSymbolsAsync(
        LspDocumentSymbolProviderContext context,
        CancellationToken cancellationToken);
}
```

**上下文**:
```csharp
internal sealed record LspDocumentSymbolProviderContext(
    DocumentSnapshot Document,
    CancellationToken CancellationToken);
```

### 4.6 ILspSignatureHelpProvider 签名帮助

```csharp
internal interface ILspSignatureHelpProvider
{
    string Name { get; }
    int Priority { get; }
    ValueTask<LspSignatureHelp?> ProvideSignatureHelpAsync(
        LspSignatureHelpProviderContext context,
        CancellationToken cancellationToken);
}
```

**上下文**:
```csharp
internal sealed record LspSignatureHelpProviderContext(
    DocumentSnapshot Document,
    LspPosition Position,
    LspSignatureHelpTriggerKind TriggerKind,
    string TriggerCharacter,
    bool IsRetrigger,
    LspSignatureHelp? ExistingSignatureHelp,
    CancellationToken CancellationToken);
```

### 4.7 ILspInlayHintProvider 内联提示

```csharp
internal interface ILspInlayHintProvider
{
    string Name { get; }
    int Priority { get; }
    ValueTask<IReadOnlyList<LspInlayHint>> ProvideInlayHintsAsync(
        LspInlayHintProviderContext context,
        CancellationToken cancellationToken);
}
```

**上下文**:
```csharp
internal sealed record LspInlayHintProviderContext(
    DocumentSnapshot Document,
    LspRange Range,
    CancellationToken CancellationToken);
```

### 4.8 ILspWorkspaceSymbolProvider 工作区符号

```csharp
internal interface ILspWorkspaceSymbolProvider
{
    string Name { get; }
    int Priority { get; }
    ValueTask<IReadOnlyList<LspWorkspaceSymbol>> ProvideWorkspaceSymbolsAsync(
        LspWorkspaceSymbolProviderContext context,
        CancellationToken cancellationToken);
}
```

**上下文**:
```csharp
internal sealed record LspWorkspaceSymbolProviderContext(
    string Query,
    IReadOnlyList<DocumentSnapshot> OpenDocuments,
    IReadOnlyList<LspWorkspaceSymbol> ExistingSymbols,
    int MaxResults,
    CancellationToken CancellationToken);
```

### 4.9 ILspFoldingRangeProvider 折叠范围

```csharp
internal interface ILspFoldingRangeProvider
{
    string Name { get; }
    int Priority { get; }
    ValueTask<IReadOnlyList<LspFoldingRange>> ProvideFoldingRangesAsync(
        LspFoldingRangeProviderContext context,
        CancellationToken cancellationToken);
}
```

**上下文**:
```csharp
internal sealed record LspFoldingRangeProviderContext(
    DocumentSnapshot Document,
    CancellationToken CancellationToken);
```

### 4.10 ILspReferenceProvider 引用查找

```csharp
internal interface ILspReferenceProvider
{
    string Name { get; }
    int Priority { get; }
    ValueTask<IReadOnlyList<LspLocation>> ProvideReferencesAsync(
        LspReferenceProviderContext context,
        CancellationToken cancellationToken);
}
```

**上下文**:
```csharp
internal sealed record LspReferenceProviderContext(
    DocumentSnapshot Document,
    LspPosition Position,
    IReadOnlyList<LspLocation> ExistingLocations,
    bool IncludeDeclaration,
    CancellationToken CancellationToken);
```

### 4.11 ILspRenameProvider 重命名

```csharp
internal interface ILspRenameProvider
{
    string Name { get; }
    int Priority { get; }
    ValueTask<LspWorkspaceEdit?> ProvideRenameAsync(
        LspRenameProviderContext context,
        CancellationToken cancellationToken);
}
```

**上下文**:
```csharp
internal sealed record LspRenameProviderContext(
    DocumentSnapshot Document,
    LspPosition Position,
    string NewName,
    LspWorkspaceEdit? ExistingEdit,
    CancellationToken CancellationToken);
```

## 5. 线程安全模型

### 5.1 WorkspaceSymbolIndex

**锁策略**:
```csharp
private readonly Lock _gate = new();
private readonly Dictionary<string, IndexedDocumentSymbols> _symbolsByDocumentPath = new(StringComparer.OrdinalIgnoreCase);
```

**保护范围**:
- `_symbolsByDocumentPath` 字典（添加/移除/更新）

**无锁操作**:
- 符号提取（纯函数）
- 搜索过滤（返回新列表）

### 5.2 其他内置扩展

**无状态设计**:
- 所有内置扩展为无状态类
- Provider 方法为纯函数（输入→输出）
- 无共享状态，无锁需求

## 6. 错误处理

### 6.1 诊断提取失败

**ComponentCodeActionExtension**:
- 正则表达式匹配失败 → 跳过诊断
- 路径解析失败 → 跳过代码操作
- 异常静默捕获 → 不影响其他诊断

### 6.2 符号提取失败

**WorkspaceSymbolExtension**:
- 正则表达式抛出异常 → 返回空符号列表
- 文档不可解析 → 跳过文档
- 缓存更新失败 → 使用旧缓存

## 7. 配置选项

**内置扩展无配置选项**:
- 硬编码优先级（`Priority`）
- 无需 `extension.json` 清单
- 无法禁用（除非修改 `BuiltinExtensionCatalog`）

**潜在扩展**:
- 添加 `DisabledExtensionIds` 过滤内置扩展
- 支持配置文件覆盖优先级
- 支持热重载内置扩展

## 8. 与其他子系统的交互

### 8.1 与 LSP 系统的交互

**Provider 调用链**:
```
LspSession → ExtensionRegistry.GetLspXxxProviders()
         → 内置扩展 Provider
         → 用户扩展 Provider
         → 聚合结果
```

**优先级排序**:
- 按 `Priority` 降序排序
- 同优先级按 `Name` 字典序排序
- 内置扩展优先级通常为 100-1000

### 8.2 与 Workspace 的交互

**WorkspaceSymbolExtension 依赖**:
- `JoltWorkspaceResolver.TryResolveNearbyVueComponent`（解析组件路径）
- `JazorImportDirectiveLocator`（定位导入语句位置）

### 8.3 与 DevServer 的交互

**热重载支持**:
- 内置扩展在主进程中，无法卸载
- 配置变更需要重启 Jolt 进程
- 未来可支持可收集加载上下文

## 9. 设计权衡

### 9.1 内置扩展 vs 用户扩展

**内置扩展优势**:
- 零配置（无需清单）
- 高性能（无 IPC 开销）
- 可靠性（编译时类型检查）

**内置扩展劣势**:
- 无法禁用（硬编码）
- 无法更新（需要重新编译）
- 版本耦合（与 Jolt 主版本绑定）

**用户扩展优势**:
- 灵活性（第三方开发）
- 可禁用（配置控制）
- 独立版本（独立发布）

**当前策略**: 混合模式
- 核心功能内置（诊断、补全、代码操作）
- 高级功能用户扩展（如 LSP 代理、自定义语言支持）

### 9.2 正则表达式 vs AST 解析

**正则表达式优势**:
- 简单快速（无需完整解析）
- 容错性好（语法错误仍可提取符号）
- 跨语言（统一模式）

**正则表达式劣势**:
- 不精确（可能误报）
- 有限语义（无法区分类型/命名空间）
- 维护成本（复杂正则难以理解）

**AST 解析优势**:
- 精确（语义正确）
- 丰富语义（类型、作用域、继承）
- 可扩展（易于添加新特性）

**AST 解析劣势**:
- 复杂（需要完整解析器）
- 慢（构建 AST 开销）
- 脆弱（语法错误导致解析失败）

**当前选择**: 正则表达式
- 满足快速搜索场景
- 容错性好（部分语法错误不影响）
- 轻量级实现

**改进方向**: 混合模式
- 正则表达式预过滤
- AST 精确验证（需要时）
- 缓存 AST 结果

### 9.3 缓存策略：版本 vs 哈希

**版本号指纹**:
```csharp
var version = document.Version?.Trim();
if (!string.IsNullOrWhiteSpace(version))
    return "version:" + version;
```

**文本哈希指纹**:
```csharp
var textBytes = Encoding.UTF8.GetBytes(document.Text);
var textHash = Convert.ToHexString(SHA256.HashData(textBytes));
return "text:" + textHash;
```

**版本号优势**:
- 快速（无需哈希计算）
- 精确（编辑器版本递增）

**版本号劣势**:
- 依赖编辑器集成（LSP 版本号）
- 不一致（不同编辑器行为不同）

**文本哈希优势**:
- 通用（不依赖编辑器）
- 准确（内容变更必检测）

**文本哈希劣势**:
- 慢（SHA256 计算开销）
- 过度敏感（空白变更触发重新提取）

**当前策略**: 版本号优先，哈希回退
- 利用 LSP 版本号优化性能
- 回退到哈希保证兼容性
