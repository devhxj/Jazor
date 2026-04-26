# LSP 路由与聚合 (Routing and Aggregation)

> Status: 活跃参考
> Positioning: Jolt LSP 的请求路由和多车道结果协调层

## 1. 文档定位

本文档描述 Jolt LSP 的请求路由机制和多车道结果聚合策略。

**相关文件**：
- `src/Jolt/Lsp/Routing/LspLaneRouter.cs` (39行) - 车道路由器
- `src/Jolt/Lsp/Routing/DocumentRegionClassifier.cs` (266行) - 文档区域分类器
- `src/Jolt/Lsp/Routing/DocumentProjectionResolver.cs` (242行) - 文档投影解析器
- `src/Jolt/Lsp/Aggregation/LspResultAggregator.cs` (256行) - 结果聚合器

## 2. 核心类型

### 2.1 LspLaneRouter

**文件位置**：`src/Jolt/Lsp/Routing/LspLaneRouter.cs`

**职责**：根据文档类型和投影目标确定请求应路由到哪些车道

**核心字段**：
```csharp
private static readonly IReadOnlyList<LaneKind> JazorOnly = [LaneKind.Jazor];
private static readonly IReadOnlyList<LaneKind> JazorSemanticTokenLanes = [LaneKind.Volar, LaneKind.Roslyn];
private static readonly IReadOnlyList<LaneKind> VolarOnly = [LaneKind.Volar];
private static readonly IReadOnlyList<LaneKind> RoslynOnly = [LaneKind.Roslyn];
private static readonly IReadOnlyList<LaneKind> DiagnosticLanes = [LaneKind.Jazor, LaneKind.Roslyn, LaneKind.Volar];
```

**路由策略**：

```csharp
public IReadOnlyList<LaneKind> GetOrderedLanes(ProjectionTarget projectionTarget)
    => projectionTarget.LaneKind switch
    {
        LaneKind.Volar => VolarOnly,
        LaneKind.Roslyn => RoslynOnly,
        _ => JazorOnly
    };

public IReadOnlyList<LaneKind> GetDiagnosticLanes(DocumentSnapshot document)
    => document.DocumentKind switch
    {
        DocumentKind.Jazor => DiagnosticLanes,
        DocumentKind.CSharp => RoslynOnly,
        DocumentKind.Vue or DocumentKind.JavaScript or DocumentKind.TypeScript or DocumentKind.Css => VolarOnly,
        _ => JazorOnly
    };

public IReadOnlyList<LaneKind> GetSemanticTokenLanes(DocumentSnapshot document)
    => document.DocumentKind switch
    {
        DocumentKind.Jazor => JazorSemanticTokenLanes,
        DocumentKind.CSharp => RoslynOnly,
        DocumentKind.Vue or DocumentKind.JavaScript or DocumentKind.TypeScript or DocumentKind.Css => VolarOnly,
        _ => JazorOnly
    };
```

**设计特点**：
- 简单的基于文档类型的路由
- 诊断和语义令牌使用多车道聚合
- 其他操作使用单车道（基于投影目标）

### 2.2 DocumentRegionClassifier

**文件位置**：`src/Jolt/Lsp/Routing/DocumentRegionClassifier.cs` (266行)

**职责**：将 Jazor 文档中的光标位置分类为 Template/Code/Directive 区域

**核心方法**：
```csharp
public DocumentRegionKind Classify(string text, int offset)
{
    var clampedOffset = Math.Max(0, Math.Min(offset, text.Length));
    var templateRange = FindTagBlock(text, "<template", "</template>");
    if (InRange(templateRange, clampedOffset))
    {
        return DocumentRegionKind.Template;
    }

    var codeRange = FindCodeBlock(text, clampedOffset);
    if (InRange(codeRange, clampedOffset))
    {
        return DocumentRegionKind.Code;
    }

    var markupBoundary = GetMarkupBoundary(text);
    return clampedOffset < markupBoundary
        || (markupBoundary == text.Length && clampedOffset == markupBoundary)
        ? DocumentRegionKind.Directive
        : DocumentRegionKind.Template;
}
```

**区域类型**：
- **Template**：`<template>` 标签内或标记边界后的 HTML
- **Code**：`@code {}` 或 `@functions {}` 块内
- **Directive**：文件开头的 Razor 指令区域

**顶层指令列表**：
```csharp
private static readonly string[] TopLevelDirectives =
[
    "@attribute", "@functions", "@implements", "@inherits", "@inject",
    "@layout", "@model", "@module", "@namespace", "@page",
    "@preservewhitespace", "@rendermode", "@typeparam", "@using", "@code"
];
```

**代码块指令列表**：
```csharp
private static readonly string[] CodeBlockDirectives =
[
    "@code",
    "@functions"
];
```

**标记边界算法**：
```csharp
private static int GetMarkupBoundary(string text)
{
    var lineStart = 0;
    while (lineStart < text.Length)
    {
        // 跳过分隔注释
        if (TrySkipPreambleDelimitedComment(text, lineStart, "/*", "*/", out var nextLineStart)
            || TrySkipPreambleDelimitedComment(text, lineStart, "@*", "*@", out nextLineStart))
        {
            lineStart = nextLineStart;
            continue;
        }

        // 跳过空白行和顶层指令
        var line = text.AsSpan(lineStart, lineEnd - lineStart);
        if (IsIgnorablePreambleLine(line))
        {
            lineStart = GetNextLineStart(text, lineEnd);
            continue;
        }

        return lineStart;
    }

    return text.Length;
}
```

### 2.3 DocumentProjectionResolver

**文件位置**：`src/Jolt/Lsp/Routing/DocumentProjectionResolver.cs` (242行)

**职责**：解析投影目标，确定请求应路由到哪个车道和哪个投影文档

**核心方法**：
```csharp
public async ValueTask<ProjectionTarget> ResolveAsync(
    DocumentSnapshot document,
    LspPosition position,
    CancellationToken cancellationToken)
{
    // 非 Jazor 文档直接路由
    if (document.DocumentKind != DocumentKind.Jazor)
    {
        if (document.DocumentKind == DocumentKind.CSharp)
        {
            return new ProjectionTarget(
                LaneKind.Roslyn,
                DocumentRegionKind.Code,
                document.DocumentPath,
                document.DocumentPath,
                position,
                null,
                IsProjected: false);
        }

        if (document.DocumentKind is DocumentKind.Vue or DocumentKind.JavaScript or DocumentKind.TypeScript or DocumentKind.Css)
        {
            return new ProjectionTarget(
                LaneKind.Volar,
                DocumentRegionKind.Unknown,
                document.DocumentPath,
                document.DocumentPath,
                position,
                null,
                IsProjected: false);
        }

        return new ProjectionTarget(
            LaneKind.Jazor,
            DocumentRegionKind.Unknown,
            document.DocumentPath,
            document.DocumentPath,
            position,
            IsProjected: false);
    }

    // Jazor 文档需要区域分类和投影解析
    var offset = LspProtocolHelpers.GetOffset(document.Text, position);
    var regionKind = _classifier.Classify(document.Text, offset);
    var virtualDocuments = await _virtualDocumentRegistry.GetBySourceDocumentAsync(
        document.DocumentPath,
        cancellationToken);

    // @module 指令位置特殊处理
    if (regionKind == DocumentRegionKind.Directive
        && IsModuleDirectivePosition(document.Text, offset))
    {
        return new ProjectionTarget(
            LaneKind.Jazor,
            regionKind,
            document.DocumentPath,
            document.DocumentPath,
            position,
            IsProjected: false);
    }

    // Code 区域尝试投影到 C#
    if (regionKind != DocumentRegionKind.Template
        && TryResolveProjectedTarget(
            document.Text,
            position,
            regionKind,
            LaneKind.Roslyn,
            FindCSharpProjection(virtualDocuments),
            out var projectedCodeTarget))
    {
        return projectedCodeTarget;
    }

    // Template 区域尝试投影到 Vue
    if (regionKind == DocumentRegionKind.Template)
    {
        var projectedDocument = FindPrimaryVueProjection(
            document.DocumentPath,
            virtualDocuments);
        if (TryResolveProjectedTarget(
            document.Text,
            position,
            regionKind,
            LaneKind.Volar,
            projectedDocument,
            out var projectedTemplateTarget))
        {
            return projectedTemplateTarget;
        }

        return new ProjectionTarget(
            LaneKind.Volar,
            regionKind,
            document.DocumentPath,
            document.DocumentPath,
            position,
            null,
            IsProjected: false);
    }

    // Code/Directive 区域默认到 Roslyn
    if (regionKind is DocumentRegionKind.Code or DocumentRegionKind.Directive)
    {
        return new ProjectionTarget(
            LaneKind.Roslyn,
            regionKind,
            document.DocumentPath,
            document.DocumentPath,
            position,
            null,
            IsProjected: false);
    }

    return new ProjectionTarget(
        LaneKind.Jazor,
        regionKind,
        document.DocumentPath,
        document.DocumentPath,
        position,
        IsProjected: false);
}
```

**投影目标解析**：
```csharp
private static bool TryResolveProjectedTarget(
    string sourceText,
    LspPosition sourcePosition,
    DocumentRegionKind regionKind,
    LaneKind laneKind,
    VirtualDocument? projectedDocument,
    out ProjectionTarget projectionTarget)
{
    if (projectedDocument is null
        || !projectedDocument.ProjectionMap.TryMapToProjectedPosition(
            sourceText,
            sourcePosition,
            projectedDocument.Text,
            out var projectedPosition))
    {
        projectionTarget = default!;
        return false;
    }

    projectionTarget = new ProjectionTarget(
        laneKind,
        regionKind,
        projectedDocument.Identity.ProjectedDocumentPath,
        projectedDocument.Identity.SourceDocumentPath,
        projectedPosition,
        null,
        IsProjected: true);
    return true;
}
```

**@module 指令检测**：
```csharp
private static bool IsModuleDirectivePosition(string text, int offset)
{
    var line = text.AsSpan(lineStart, lineEnd - lineStart).TrimStart();
    if (line.IsEmpty || line[0] != '@')
    {
        return false;
    }

    var directiveLength = 0;
    while (directiveLength < line.Length && !char.IsWhiteSpace(line[directiveLength]))
    {
        directiveLength++;
    }

    var directive = line[..directiveLength];
    var moduleDirective = "@module".AsSpan();
    return directive.Equals(moduleDirective, StringComparison.OrdinalIgnoreCase)
        || moduleDirective.StartsWith(directive, StringComparison.OrdinalIgnoreCase);
}
```

### 2.4 LspResultAggregator

**文件位置**：`src/Jolt/Lsp/Aggregation/LspResultAggregator.cs` (256行)

**职责**：去重和聚合多车道结果

**核心方法**：

**诊断聚合**：
```csharp
public IReadOnlyList<LspDiagnostic> AggregateDiagnostics(
    IReadOnlyList<LspDiagnostic> diagnostics)
{
    var seen = new HashSet<LspDiagnostic>(DiagnosticComparer);
    var aggregated = new List<LspDiagnostic>(diagnostics.Count);
    foreach (var diagnostic in diagnostics)
    {
        if (seen.Add(diagnostic))
        {
            aggregated.Add(diagnostic);
        }
    }
    return aggregated.ToArray();
}
```

**诊断比较器**：
```csharp
private sealed class LspDiagnosticComparer : IEqualityComparer<LspDiagnostic>
{
    public bool Equals(LspDiagnostic? x, LspDiagnostic? y)
        => ReferenceEquals(x, y)
            || (x is not null
                && y is not null
                && x.Range.Start.Line == y.Range.Start.Line
                && x.Range.Start.Character == y.Range.Start.Character
                && x.Range.End.Line == y.Range.End.Line
                && x.Range.End.Character == y.Range.End.Character
                && string.Equals(x.Code, y.Code, StringComparison.Ordinal)
                && string.Equals(x.Message, y.Message, StringComparison.Ordinal)
                && string.Equals(x.Source, y.Source, StringComparison.Ordinal));
}
```

**补全项聚合**：
```csharp
public IReadOnlyList<LspCompletionItem> AggregateCompletionItems(
    IReadOnlyList<LspCompletionItem> items)
{
    return items
        .GroupBy(static item => string.Join('|', item.Label, item.Kind, item.Detail), StringComparer.Ordinal)
        .Select(static group => group.First())
        .ToArray();
}
```

**位置聚合**：
```csharp
public IReadOnlyList<LspLocation> AggregateLocations(
    IReadOnlyList<LspLocation> locations)
{
    return locations
        .GroupBy(static location => string.Join(
            '|',
            location.Uri,
            location.Range.Start.Line,
            location.Range.Start.Character,
            location.Range.End.Line,
            location.Range.End.Character),
            StringComparer.Ordinal)
        .Select(static group => group.First())
        .ToArray();
}
```

**代码操作聚合**：
```csharp
public IReadOnlyList<LspCodeAction> AggregateCodeActions(
    IReadOnlyList<LspCodeAction> actions)
{
    return actions
        .GroupBy(
            static action => string.Join(
                '|',
                action.Title,
                action.Kind,
                GetWorkspaceEditSignature(action.Edit)),
            StringComparer.Ordinal)
        .Select(static group => group.First())
        .ToArray();
}
```

**工作区编辑聚合**：
```csharp
public LspWorkspaceEdit? AggregateWorkspaceEdits(
    IReadOnlyList<LspWorkspaceEdit> edits)
{
    var mergedChanges = new Dictionary<string, List<LspTextEdit>>(StringComparer.Ordinal);
    foreach (var edit in edits)
    {
        foreach (var change in edit.Changes)
        {
            if (!mergedChanges.TryGetValue(change.Key, out var bucket))
            {
                bucket = [];
                mergedChanges.Add(change.Key, bucket);
            }
            bucket.AddRange(change.Value);
        }
    }

    return new LspWorkspaceEdit
    {
        Changes = mergedChanges.ToDictionary(
            static entry => entry.Key,
            static entry => entry.Value
                .GroupBy(static edit => string.Join(
                    '|',
                    edit.Range.Start.Line,
                    edit.Range.Start.Character,
                    edit.Range.End.Line,
                    edit.Range.End.Character,
                    edit.NewText),
                    StringComparer.Ordinal)
                .Select(static group => group.First())
                .OrderByDescending(static edit => edit.Range.Start.Line)
                .ThenByDescending(static edit => edit.Range.Start.Character)
                .ToArray(),
            StringComparer.Ordinal)
    };
}
```

## 3. 核心算法

### 3.1 文档区域分类算法

**文件位置**：`src/Jolt/Lsp/Routing/DocumentRegionClassifier.cs:32-52`

**步骤**：
1. 检查是否在 `<template>` 标签内 → Template
2. 检查是否在 `@code {}` 或 `@functions {}` 块内 → Code
3. 计算标记边界（跳过注释、空白行、顶层指令）
4. 如果位置在标记边界前 → Directive
5. 否则 → Template

**标记边界计算**：
```csharp
private static int GetMarkupBoundary(string text)
{
    var lineStart = 0;
    while (lineStart < text.Length)
    {
        // 跳过分隔注释 /* */ 或 @* *@
        if (TrySkipPreambleDelimitedComment(...))
        {
            lineStart = nextLineStart;
            continue;
        }

        // 跳过空白行和顶层指令
        if (IsIgnorablePreambleLine(line))
        {
            lineStart = GetNextLineStart(text, lineEnd);
            continue;
        }

        return lineStart;
    }
    return text.Length;
}
```

### 3.2 投影目标解析算法

**文件位置**：`src/Jolt/Lsp/Routing/DocumentProjectionResolver.cs:20-141`

**步骤**：
1. 非 Jazor 文档：直接返回对应车道
2. Jazor 文档：
   - 分类光标位置所在区域
   - 获取虚拟文档列表
   - @module 指令位置 → Jazor 车道
   - Code 区域且存在 C# 投影 → Roslyn 车道（投影）
   - Template 区域且存在 Vue 投影 → Volar 车道（投影）
   - Template 区域无投影 → Volar 车道（非投影）
   - Code/Directive 区域无投影 → Roslyn 车道（非投影）
   - 其他 → Jazor 车道

**投影映射查找**：
```csharp
private static bool TryResolveProjectedTarget(
    string sourceText,
    LspPosition sourcePosition,
    DocumentRegionKind regionKind,
    LaneKind laneKind,
    VirtualDocument? projectedDocument,
    out ProjectionTarget projectionTarget)
{
    if (projectedDocument is null
        || !projectedDocument.ProjectionMap.TryMapToProjectedPosition(
            sourceText,
            sourcePosition,
            projectedDocument.Text,
            out var projectedPosition))
    {
        projectionTarget = default!;
        return false;
    }

    projectionTarget = new ProjectionTarget(
        laneKind,
        regionKind,
        projectedDocument.Identity.ProjectedDocumentPath,
        projectedDocument.Identity.SourceDocumentPath,
        projectedPosition,
        null,
        IsProjected: true);
    return true;
}
```

### 3.3 结果去重算法

**通用策略**：基于关键字段分组，取每组第一个

**诊断关键字段**：Range + Code + Message + Source

**补全项关键字段**：Label + Kind + Detail

**位置关键字段**：Uri + Range

**工作区编辑关键字段**：DocumentUri + Range + NewText

**排序策略**：
- 位置和文档高亮：按位置排序（行优先，字符次之）
- 工作区编辑：按位置降序排序（从后向前应用）

## 4. 线程安全模型

### 4.1 无状态设计

**LspLaneRouter**：所有字段都是静态只读数组，线程安全

**DocumentRegionClassifier**：无状态类，所有方法都是纯函数

**DocumentProjectionResolver**：字段都是只读的，方法都是异步无状态

**LspResultAggregator**：无状态类，所有方法都是纯函数

### 4.2 并发访问

**安全性**：所有类型都是线程安全的，可以并发调用

**无共享状态**：不需要锁或其他同步机制

## 5. 错误处理

### 5.1 参数验证

**DocumentRegionClassifier**：
- 边界检查：`Math.Clamp(offset, 0, text.Length)`

**DocumentProjectionResolver**：
- 空值检查：`ArgumentNullException.ThrowIfNull`
- 取消令牌检查：`cancellationToken.ThrowIfCancellationRequested()`

**LspResultAggregator**：
- 空值检查：`ArgumentNullException.ThrowIfNull`

### 5.2 异常传播

**策略**：所有异常直接传播到上层处理

**理由**：
- 路由和聚合是核心逻辑，不应吞没异常
- 上层（LspSession）有统一的异常处理机制

## 6. 配置选项

### 6.1 构造函数参数

**LspLaneRouter**：无构造函数参数（静态类）

**DocumentRegionClassifier**：
```csharp
public DocumentRegionClassifier(
    DocumentRegionClassifier classifier,
    IVirtualDocumentRegistry virtualDocumentRegistry)
```

**DocumentProjectionResolver**：
```csharp
public DocumentProjectionResolver(
    DocumentRegionClassifier classifier,
    IVirtualDocumentRegistry virtualDocumentRegistry)
```

**LspResultAggregator**：无构造函数参数（无状态类）

## 7. 与其他子系统的交互

### 7.1 与虚拟文档注册表交互

**DocumentProjectionResolver**：
- 获取源文档的所有投影文档
- 查找特定类型的投影（C#/Vue）
- 使用 `ProjectionMap` 映射位置

### 7.2 与车道路由器交互

**LspSession**：
- 调用 `GetOrderedLanes` 获取车道列表
- 调用 `GetDiagnosticLanes` 获取诊断车道列表
- 调用 `GetSemanticTokenLanes` 获取语义令牌车道列表

### 7.3 与结果聚合器交互

**LspSession**：
- 聚合诊断：`AggregateDiagnostics`
- 聚合补全项：`AggregateCompletionItems`
- 聚合位置：`AggregateLocations`
- 聚合工作区编辑：`AggregateWorkspaceEdits`

**协调器**：
- ReferenceCoordinator：聚合引用位置
- RenameCoordinator：聚合重命名编辑
- CodeActionCoordinator：聚合代码操作

## 8. 设计权衡

### 8.1 简单路由 vs 复杂路由

**选择**：基于文档类型的简单路由

**原因**：
- Jazor 文档的三种语言区域已经明确
- 投影目标已经包含车道信息
- 简单的 switch 表达式易于理解和维护

**权衡**：
- 优势：简单、高效、易维护
- 劣势：不够灵活（但当前需求不需要）

### 8.2 区域分类优先级

**选择**：Template > Code > Directive

**原因**：
- `<template>` 标签最明确
- Code 块有明确的开始和结束标记
- Directive 是兜底情况

**权衡**：
- 优势：明确的优先级，避免歧义
- 劣势：可能需要多次查找（可接受）

### 8.3 诊断去重策略

**选择**：基于 Range + Code + Message + Source

**原因**：
- 同一位置的不同诊断应保留
- 相同诊断可能来自不同车道（需要去重）
- Source 字段区分诊断来源（Jolt/Roslyn/Volar）

**权衡**：
- 优势：精确的去重，保留有价值的信息
- 劣势：可能遗漏相似但不完全相同的诊断

### 8.4 工作区编辑排序

**选择**：按位置降序排序（从后向前）

**原因**：
- 后面的编辑不影响前面的位置
- 避免位置偏移问题

**权衡**：
- 优势：正确的编辑顺序
- 劣势：需要排序（O(n log n)，可接受）

### 8.5 投影失败回退

**选择**：投影失败时回退到源文档

**原因**：
- 投影可能不存在（如文档刚打开）
- 投影映射可能失败（如语法错误）
- 回退到源文档保证功能可用

**权衡**：
- 优势：鲁棒性，降级可用
- 劣势：可能丢失精度（可接受的权衡）
