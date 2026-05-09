# Projection Map - Coordinate Mapping Engine


## 1. 文档定位

ProjectionMap 是 Jolt 处理「坐标映射」的核心抽象。在 LSP 场景中，客户端需要在源文档（如 `.jazor`）和投影文档（如生成的 `.cs`）之间转换位置和范围信息。例如：

- **诊断消息映射**：Razor 编译器在 `.cs` 投影中报告错误，需要映射回 `.jazor` 源文档显示给用户
- **代码编辑映射**：用户在 `.jazor` 中编辑代码，需要同步到 `.cs` 投影触发 IntelliSense
- **重构操作映射**：用户在 `.jazor` 中执行重命名，需要在所有投影中执行相应操作

ProjectionMap 提供了高性能、双向、支持边界情况的坐标转换能力。

## 2. 核心类型

### 2.1 ProjectionSegment Record

**文件位置**：`src/Jolt/VirtualDocuments/Mapping/ProjectionSegment.cs`

```csharp
public sealed record ProjectionSegment
{
    public ProjectionSegment(
        int originalStart,      // 源文档起始偏移
        int originalLength,     // 源文档长度
        int projectedStart,     // 投影文档起始偏移
        int projectedLength,    // 投影文档长度
        bool isBidirectional = true)  // 是否支持双向映射

    public int OriginalStart { get; }
    public int OriginalLength { get; }
    public int ProjectedStart { get; }
    public int ProjectedLength { get; }
    public bool IsBidirectional { get; }

    // 计算属性：结束位置（不包含）
    public int OriginalEnd => checked(OriginalStart + OriginalLength);
    public int ProjectedEnd => checked(ProjectedStart + ProjectedLength);

    // 判断偏移量是否在段内
    public bool ContainsOriginalOffset(int offset)
        => offset >= OriginalStart && offset < OriginalEnd;

    public bool ContainsProjectedOffset(int offset)
        => offset >= ProjectedStart && offset < ProjectedEnd;
}
```

**设计说明**：
- **基础映射单元**：一个 `ProjectionSegment` 表示源文档和投影文档中的一个连续映射区域
- **双向映射标记**：`IsBidirectional` 控制是否支持反向映射（投影→源）
- **左闭右开区间**：使用 `[Start, End)` 半开区间，与字符串索引惯例一致
- **溢出检查**：使用 `checked` 关键字检测整数溢出

**使用示例**：
```csharp
// 场景：.jazor 文件的第 10-20 字符映射到 .cs 文件的第 15-30 字符
var segment = new ProjectionSegment(
    originalStart: 10,
    originalLength: 10,
    projectedStart: 15,
    projectedLength: 15,
    isBidirectional: true
);

// 检查偏移量是否在段内
segment.ContainsOriginalOffset(15);  // true
segment.ContainsOriginalOffset(20);  // false（20 是 OriginalEnd，不包含）
```

### 2.2 ProjectionMap 类

**文件位置**：`src/Jolt/VirtualDocuments/Mapping/ProjectionMap.cs`

```csharp
public sealed class ProjectionMap
{
    public ProjectionMap(
        string sourceDocumentPath,
        string projectedDocumentPath,
        IReadOnlyList<ProjectionSegment> segments)
    {
        SourceDocumentPath = sourceDocumentPath ?? throw new ArgumentNullException(nameof(sourceDocumentPath));
        ProjectedDocumentPath = projectedDocumentPath ?? throw new ArgumentNullException(nameof(projectedDocumentPath));
        Segments = ValidateSegments(segments);  // 验证排序和不重叠
    }

    public string SourceDocumentPath { get; }
    public string ProjectedDocumentPath { get; }
    public IReadOnlyList<ProjectionSegment> Segments { get; }

    // 核心映射方法（详见后续章节）
}
```

**设计说明**：
- **不可变性**：构造后所有属性不可变，线程安全
- **路径标识**：存储源文档和投影文档路径，用于调试和日志
- **段验证**：构造时验证段是否排序且不重叠（详见 2.3 节）

## 3. 核心算法

### 3.1 段验证（ValidateSegments）

**文件位置**：`src/Jolt/VirtualDocuments/Mapping/ProjectionMap.cs:326-349`

```csharp
private static IReadOnlyList<ProjectionSegment> ValidateSegments(IReadOnlyList<ProjectionSegment> segments)
{
    ArgumentNullException.ThrowIfNull(segments);

    var copiedSegments = new ProjectionSegment[segments.Count];
    ProjectionSegment? previousSegment = null;
    for (var index = 0; index < segments.Count; index++)
    {
        var segment = segments[index]
            ?? throw new ArgumentException("Projection segments cannot contain null items.", nameof(segments));

        if (previousSegment is not null)
        {
            // 验证排序和不重叠
            if (segment.OriginalStart < previousSegment.OriginalStart
                || segment.OriginalStart < previousSegment.OriginalEnd)
            {
                throw new ArgumentException(
                    "Projection segments must be sorted and non-overlapping in source order.",
                    nameof(segments));
            }
        }

        copiedSegments[index] = segment;
        previousSegment = segment;
    }

    return copiedSegments;
}
```

**验证规则**：
1. **非空检查**：段列表和每个段都不能为 null
2. **排序要求**：`OriginalStart` 必须递增
3. **不重叠要求**：当前段的 `OriginalStart` 必须 ≥ 前一段的 `OriginalEnd`
4. **防御性拷贝**：将输入列表拷贝到数组，防止外部修改

**错误示例**：
```csharp
// 重叠段：会抛出异常
var invalidSegments = new[]
{
    new ProjectionSegment(0, 10, 0, 10),
    new ProjectionSegment(5, 10, 10, 20)  // OriginalStart=5 < 前一段的 OriginalEnd=10
};
```

### 3.2 源文档 → 投影文档映射

#### 3.2.1 位置映射（TryMapToProjectedPosition）

**文件位置**：`src/Jolt/VirtualDocuments/Mapping/ProjectionMap.cs:24-35`

```csharp
internal bool TryMapToProjectedPosition(
    string sourceText,
    LspPosition sourcePosition,
    string projectedText,
    out LspPosition projectedPosition)
{
    // 将 LSP 位置转换为偏移量
    var sourceOffset = LspProtocolHelpers.GetOffset(sourceText, sourcePosition);

    // 执行核心映射逻辑
    if (!TryMapToProjectedOffsetCore(sourceOffset, preferSegmentEnd: sourceOffset == sourceText.Length, out var projectedOffset))
    {
        projectedPosition = new LspPosition();
        return false;
    }

    // 将偏移量转换回 LSP 位置
    projectedPosition = LspProtocolHelpers.GetPosition(projectedText, projectedOffset);
    return true;
}
```

**设计说明**：
- **LSP 坐标转换**：使用 `LspProtocolHelpers` 在 `LspPosition`（行列号）和偏移量之间转换
- **边界处理**：当源偏移量等于文档长度时，设置 `preferSegmentEnd: true`（详见 3.4 节）

#### 3.2.2 范围映射（TryMapToProjectedRange）

**文件位置**：`src/Jolt/VirtualDocuments/Mapping/ProjectionMap.cs:37-58`

```csharp
internal bool TryMapToProjectedRange(
    string sourceText,
    LspRange sourceRange,
    string projectedText,
    [NotNullWhen(true)] out LspRange? projectedRange)
{
    // 分别映射起始和结束位置
    var sourceStartOffset = LspProtocolHelpers.GetOffset(sourceText, sourceRange.Start);
    var sourceEndOffset = LspProtocolHelpers.GetOffset(sourceText, sourceRange.End);

    if (!TryMapToProjectedOffsetCore(sourceStartOffset, preferSegmentEnd: false, out var projectedStartOffset)
        || !TryMapToProjectedOffsetCore(sourceEndOffset, preferSegmentEnd: true, out var projectedEndOffset))
    {
        projectedRange = null;
        return false;
    }

    projectedRange = new LspRange
    {
        Start = LspProtocolHelpers.GetPosition(projectedText, projectedStartOffset),
        End = LspProtocolHelpers.GetPosition(projectedText, projectedEndOffset)
    };
    return true;
}
```

**设计说明**：
- **独立映射**：起始和结束位置分别映射，支持跨段范围
- **非对称处理**：起始位置使用 `preferSegmentEnd: false`，结束位置使用 `preferSegmentEnd: true`

#### 3.2.3 核心映射算法（TryMapToProjectedOffsetCore）

**文件位置**：`src/Jolt/VirtualDocuments/Mapping/ProjectionMap.cs:102-133`

```csharp
private bool TryMapToProjectedOffsetCore(int sourceOffset, bool preferSegmentEnd, out int projectedOffset)
{
    // 1. 边界检查
    if (sourceOffset < 0)
    {
        projectedOffset = default;
        return false;
    }

    // 2. 优先尝试段尾映射（如果 preferSegmentEnd=true）
    if (preferSegmentEnd && TryMapBoundaryToProjectedOffset(sourceOffset, preferSegmentEnd: true, out projectedOffset))
    {
        return true;
    }

    // 3. 遍历所有段，查找包含源偏移量的段
    foreach (var segment in Segments)
    {
        if (!segment.IsBidirectional || !segment.ContainsOriginalOffset(sourceOffset))
        {
            continue;
        }

        // 线性插值映射：计算投影偏移量
        projectedOffset = segment.ProjectedStart + Math.Min(sourceOffset - segment.OriginalStart, segment.ProjectedLength);
        return true;
    }

    // 4. 回退到段首映射（如果 preferSegmentEnd=false）
    if (!preferSegmentEnd && TryMapBoundaryToProjectedOffset(sourceOffset, preferSegmentEnd: false, out projectedOffset))
    {
        return true;
    }

    // 5. 映射失败
    projectedOffset = default;
    return false;
}
```

**算法流程**：
1. **边界检查**：拒绝负偏移量
2. **优先段尾**：如果 `preferSegmentEnd=true`，先尝试匹配段的结束位置
3. **段内映射**：查找包含源偏移量的段，使用线性插值计算投影偏移量
4. **回退段首**：如果 `preferSegmentEnd=false`，尝试匹配段的起始位置
5. **失败返回**：无法映射时返回 false

**线性插值示例**：
```
源文档：  [0.........10.........20]
投影文档：[0.........15.........30]

如果 sourceOffset = 15：
projectedOffset = 15 + min(15 - 10, 15) = 15 + 5 = 20
```

### 3.3 投影文档 → 源文档映射

#### 3.3.1 位置和范围映射

**文件位置**：`src/Jolt/VirtualDocuments/Mapping/ProjectionMap.cs:60-94`

```csharp
// 位置映射（对称于 TryMapToProjectedPosition）
internal bool TryMapToOriginalPosition(string projectedText, LspPosition projectedPosition, string sourceText, out LspPosition originalPosition)
{
    var projectedOffset = LspProtocolHelpers.GetOffset(projectedText, projectedPosition);
    if (!TryMapToOriginalOffsetCore(projectedOffset, preferSegmentEnd: projectedOffset == projectedText.Length, out var originalOffset))
    {
        originalPosition = new LspPosition();
        return false;
    }

    originalPosition = LspProtocolHelpers.GetPosition(sourceText, originalOffset);
    return true;
}

// 范围映射（对称于 TryMapToProjectedRange）
internal bool TryMapToOriginalRange(string projectedText, LspRange projectedRange, string sourceText, [NotNullWhen(true)] out LspRange? originalRange)
{
    var projectedStartOffset = LspProtocolHelpers.GetOffset(projectedText, projectedRange.Start);
    var projectedEndOffset = LspProtocolHelpers.GetOffset(projectedText, projectedRange.End);
    if (!TryMapToOriginalOffsetCore(projectedStartOffset, preferSegmentEnd: false, out var originalStartOffset)
        || !TryMapToOriginalOffsetCore(projectedEndOffset, preferSegmentEnd: true, out var originalEndOffset))
    {
        originalRange = null;
        return false;
    }

    originalRange = new LspRange
    {
        Start = LspProtocolHelpers.GetPosition(sourceText, originalStartOffset),
        End = LspProtocolHelpers.GetPosition(sourceText, originalEndOffset)
    };
    return true;
}
```

#### 3.3.2 核心反向映射算法（TryMapToOriginalOffsetCore）

**文件位置**：`src/Jolt/VirtualDocuments/Mapping/ProjectionMap.cs:135-166`

```csharp
private bool TryMapToOriginalOffsetCore(int projectedOffset, bool preferSegmentEnd, out int originalOffset)
{
    // 1. 边界检查
    if (projectedOffset < 0)
    {
        originalOffset = default;
        return false;
    }

    // 2. 优先尝试段尾映射
    if (preferSegmentEnd && TryMapBoundaryToOriginalOffset(projectedOffset, preferSegmentEnd: true, out originalOffset))
    {
        return true;
    }

    // 3. 遍历所有段，查找包含投影偏移量的段
    foreach (var segment in Segments)
    {
        if (!segment.IsBidirectional || !segment.ContainsProjectedOffset(projectedOffset))
        {
            continue;
        }

        // 线性插值映射：计算源偏移量
        originalOffset = segment.OriginalStart + Math.Min(projectedOffset - segment.ProjectedStart, segment.OriginalLength);
        return true;
    }

    // 4. 回退到段首映射
    if (!preferSegmentEnd && TryMapBoundaryToOriginalOffset(projectedOffset, preferSegmentEnd: false, out originalOffset))
    {
        return true;
    }

    // 5. 映射失败
    originalOffset = default;
    return false;
}
```

**对称性**：
- 与 `TryMapToProjectedOffsetCore` 完全对称
- 只需将 "源" 和 "投影" 互换，算法逻辑相同

### 3.4 边界处理策略

#### 3.4.1 preferSegmentEnd 参数

**用途**：控制当偏移量位于段边界时的映射优先级。

**场景示例**：
```
源文档：  [0.....段1.....10][间隙][20.....段2.....30]
投影文档：[0.....段1.....15][间隙][25.....段2.....40]

问题：sourceOffset = 10（段1 的结束位置）应该映射到哪里？
```

**preferSegmentEnd 的行为**：

| preferSegmentEnd | 优先匹配 | 回退匹配 | 典型用例 |
|------------------|----------|----------|----------|
| `true` | 段尾位置（如段1的 OriginalEnd=10 → ProjectedEnd=15） | 段首位置 | 范围的结束位置、文档末尾 |
| `false` | 段首位置（如段2的 OriginalStart=20 → ProjectedStart=25） | 段尾位置 | 范围的起始位置、文档开头 |

#### 3.4.2 边界映射算法

**源 → 投影边界映射**（`TryMapBoundaryToProjectedOffset`）：
**文件位置**：`src/Jolt/VirtualDocuments/Mapping/ProjectionMap.cs:180-210`

```csharp
private bool TryMapBoundaryToProjectedOffset(int sourceOffset, bool preferSegmentEnd, out int projectedOffset)
{
    if (preferSegmentEnd)
    {
        // 优先尝试匹配段的结束位置
        if (TryMapProjectedSegmentEnd(sourceOffset, out projectedOffset))
        {
            return true;
        }

        // 回退到匹配段的起始位置
        if (TryMapProjectedSegmentStart(sourceOffset, out projectedOffset))
        {
            return true;
        }

        projectedOffset = default;
        return false;
    }

    // 优先尝试匹配段的起始位置
    if (TryMapProjectedSegmentStart(sourceOffset, out projectedOffset))
    {
        return true;
    }

    // 回退到匹配段的结束位置
    if (TryMapProjectedSegmentEnd(sourceOffset, out projectedOffset))
    {
        return true;
    }

    projectedOffset = default;
    return false;
}
```

**段首匹配**（`TryMapProjectedSegmentStart`）：
**文件位置**：`src/Jolt/VirtualDocuments/Mapping/ProjectionMap.cs:244-262`

```csharp
private bool TryMapProjectedSegmentStart(int sourceOffset, out int projectedOffset)
{
    foreach (var segment in Segments)
    {
        if (!segment.IsBidirectional)
        {
            continue;
        }

        if (segment.OriginalStart == sourceOffset)
        {
            projectedOffset = segment.ProjectedStart;
            return true;
        }
    }

    projectedOffset = default;
    return false;
}
```

**段尾匹配**（`TryMapProjectedSegmentEnd`）：
**文件位置**：`src/Jolt/VirtualDocuments/Mapping/ProjectionMap.cs:264-283`

```csharp
private bool TryMapProjectedSegmentEnd(int sourceOffset, out int projectedOffset)
{
    // 反向遍历，优先匹配最后一个段的结束位置
    for (var index = Segments.Count - 1; index >= 0; index--)
    {
        var segment = Segments[index];
        if (!segment.IsBidirectional)
        {
            continue;
        }

        if (segment.OriginalEnd == sourceOffset)
        {
            projectedOffset = segment.ProjectedEnd;
            return true;
        }
    }

    projectedOffset = default;
    return false;
}
```

**设计说明**：
- **段尾反向遍历**：处理段边界重叠时，优先匹配最后一个段
- **段首正向遍历**：处理段边界重叠时，优先匹配第一个段
- **双向支持**：只处理 `IsBidirectional=true` 的段

### 3.5 特殊工厂方法：CreateWholeDocument

**文件位置**：`src/Jolt/VirtualDocuments/Mapping/ProjectionMap.cs:168-178`

```csharp
public static ProjectionMap CreateWholeDocument(
    string sourceDocumentPath,
    string projectedDocumentPath,
    int sourceLength,
    int projectedLength)
    => new(
        sourceDocumentPath,
        projectedDocumentPath,
        [
            new ProjectionSegment(0, sourceLength, 0, projectedLength)
        ]);
```

**用途**：创建一个"恒等投影"，即整个文档一对一映射。

**使用场景**：
- 源文档和投影文档内容完全相同（如纯文本复制）
- 用于初始化或测试

**示例**：
```csharp
// 场景：创建一个 100 字符源文档到 100 字符投影文档的恒等映射
var map = ProjectionMap.CreateWholeDocument(
    "/src/App.jazor",
    "/src/App.jazor.cs",
    sourceLength: 100,
    projectedLength: 100
);

// 等价于：
var map = new ProjectionMap(
    "/src/App.jazor",
    "/src/App.jazor.cs",
    [new ProjectionSegment(0, 100, 0, 100)]
);
```

## 4. 线程安全模型

### 4.1 不可变设计

- **不可变对象**：`ProjectionMap` 的所有属性都是只读的
- **线程安全**：不可变对象天然线程安全，无需锁
- **值语义**：每次更新都创建新 `ProjectionMap` 实例

### 4.2 使用模式

```csharp
// 更新模式：替换整个 ProjectionMap
var oldMap = virtualDocument.ProjectionMap;
var newMap = new ProjectionMap(
    oldMap.SourceDocumentPath,
    oldMap.ProjectedDocumentPath,
    newSegments
);
var newDocument = new VirtualDocument(
    virtualDocument.Identity,
    virtualDocument.Text,
    newMap,
    virtualDocument.Version
);
```

## 5. 错误处理

### 5.1 构造时验证

| 错误类型 | 触发条件 | 异常信息 |
|----------|----------|----------|
| `ArgumentNullException` | `sourceDocumentPath` 或 `projectedDocumentPath` 为 null | "Value cannot be null. (Parameter 'sourceDocumentPath')" |
| `ArgumentNullException` | `segments` 为 null | "Value cannot be null. (Parameter 'segments')" |
| `ArgumentException` | 段列表包含 null | "Projection segments cannot contain null items." |
| `ArgumentException` | 段未排序或重叠 | "Projection segments must be sorted and non-overlapping in source order." |

### 5.2 运行时映射失败

映射方法使用 `Try*` 模式：
- 返回 `bool` 表示成功或失败
- 使用 `out` 参数输出结果
- 失败时输出默认值（`default`）

**失败场景**：
- 偏移量为负数
- 偏移量不属于任何映射段（且无法匹配边界）
- 段标记为 `IsBidirectional=false`

## 6. 配置选项

当前实现无运行时配置选项，所有行为在构造时确定：
- **段列表**：构造时传入，之后不可变
- **双向支持**：通过 `ProjectionSegment.IsBidirectional` 控制
- **边界优先级**：通过 `preferSegmentEnd` 参数动态控制

## 7. 与其他子系统的交互

### 7.1 LSP 协议层

- **坐标转换**：`LspProtocolHelpers` 提供行列号与偏移量的转换
- **位置/范围类型**：使用 `LspPosition` 和 `LspRange` 类型

### 7.2 Virtual Documents 子系统

- **组合关系**：`VirtualDocument` 持有 `ProjectionMap` 实例
- **生命周期**：`ProjectionMap` 随 `VirtualDocument` 一起创建和销毁

### 7.3 文档生成器

- **段生成**：文档生成器负责创建 `ProjectionSegment` 列表
- **映射构建**：文档生成器根据源文档和投影文档的语法结构构建映射关系

## 8. 设计权衡

### 8.1 线性搜索 vs. 二分查找

**当前选择**：线性搜索（`foreach` 遍历段）
- **优点**：实现简单，支持非均匀段分布
- **缺点**：最坏时间复杂度 O(n)
- **适用场景**：段数量较少（通常 < 100），线性搜索足够快

**潜在优化**：
```csharp
// 使用二分查找优化（未来改进）
private int FindSegmentIndex(int sourceOffset)
{
    var left = 0;
    var right = Segments.Count - 1;
    while (left <= right)
    {
        var mid = (left + right) / 2;
        var segment = Segments[mid];
        if (sourceOffset < segment.OriginalStart)
        {
            right = mid - 1;
        }
        else if (sourceOffset >= segment.OriginalEnd)
        {
            left = mid + 1;
        }
        else
        {
            return mid;  // 找到包含偏移量的段
        }
    }
    return -1;  // 未找到
}
```

### 8.2 段验证时机

**当前选择**：构造时一次性验证
- **优点**：早期发现错误，避免运行时检查
- **缺点**：构造性能稍有开销
- **适用场景**：段数量少，验证开销可忽略

**替代方案**：延迟验证（首次使用时）
- **优点**：构造更快
- **缺点**：错误发现延迟，调试困难

### 8.3 边界映射的复杂度

**当前选择**：支持 `preferSegmentEnd` 参数的四阶段映射逻辑
- **优点**：精确控制边界行为，支持复杂场景
- **缺点**：逻辑复杂，理解成本高
- **适用场景**：LSP 边界情况多，需要精确控制

**简化方案**：
- 始终优先匹配段内，失败时返回 false
- 缺点：无法处理段边界情况

### 8.4 IsBidirectional 的必要性

**当前选择**：每个段独立标记是否支持双向映射
- **优点**：灵活性高，支持单向投影（如只读生成内容）
- **缺点**：增加状态管理复杂度
- **适用场景**：某些投影内容可能不需要反向映射（如编译器生成的元数据）

**简化方案**：
- 所有段都支持双向映射
- 缺点：无法表示单向关系

## 9. 性能特征

### 9.1 时间复杂度

| 操作 | 时间复杂度 | 说明 |
|------|-----------|------|
| 构造函数 | O(n) | 验证段排序和不重叠 |
| TryMapToProjectedPosition | O(n) | 遍历段查找匹配 |
| TryMapToProjectedRange | O(n) | 两次位置映射 |
| TryMapToOriginalPosition | O(n) | 遍历段查找匹配 |
| TryMapToOriginalRange | O(n) | 两次位置映射 |
| CreateWholeDocument | O(1) | 直接构造单个段 |

### 9.2 空间复杂度

- **存储**：O(n)，n 为段数量
- **拷贝**：构造时拷贝段列表，避免外部修改

### 9.3 优化建议

1. **二分查找**：对于大量段（>100），使用二分查找替代线性搜索
2. **段缓存**：缓存最近使用的段，提高命中率
3. **延迟计算**：`OriginalEnd` 和 `ProjectedEnd` 是计算属性，可考虑缓存

## 10. 使用示例

### 10.1 基础映射

```csharp
// 场景：.jazor 文件的前 50 字符映射到 .cs 文件的前 100 字符
var segments = new[]
{
    new ProjectionSegment(0, 50, 0, 100)
};
var map = new ProjectionMap("/src/App.jazor", "/src/App.jazor.cs", segments);

// 映射位置
if (map.TryMapToProjectedOffset(25, out var projectedOffset))
{
    Console.WriteLine($"源偏移 25 → 投影偏移 {projectedOffset}");  // 输出：50
}
```

### 10.2 多段映射

```csharp
// 场景：.jazor 文件有两个映射区域（中间有间隙）
var segments = new[]
{
    new ProjectionSegment(0, 20, 0, 30),     // 第一段
    new ProjectionSegment(30, 20, 50, 30)    // 第二段（源文档 30-50，投影文档 50-80）
};
var map = new ProjectionMap("/src/App.jazor", "/src/App.jazor.cs", segments);

// 映射第一段内的位置
map.TryMapToProjectedOffset(10, out var p1);  // p1 = 15

// 映射第二段内的位置
map.TryMapToProjectedOffset(40, out var p2);  // p2 = 60

// 映射间隙位置（失败）
map.TryMapToProjectedOffset(25, out var p3);  // p3 = false
```

### 10.3 边界映射

```csharp
var segments = new[]
{
    new ProjectionSegment(0, 10, 0, 15),
    new ProjectionSegment(20, 10, 25, 15)
};
var map = new ProjectionMap("/src/App.jazor", "/src/App.jazor.cs", segments);

// 映射段边界（preferSegmentEnd=true）
map.TryMapToProjectedOffsetCore(10, preferSegmentEnd: true, out var p1);
// p1 = 15（第一段的 ProjectedEnd）

// 映射段边界（preferSegmentEnd=false）
map.TryMapToProjectedOffsetCore(10, preferSegmentEnd: false, out var p2);
// p2 = 25（第二段的 ProjectedStart）
```

### 10.4 反向映射

```csharp
var segments = new[]
{
    new ProjectionSegment(0, 50, 0, 100, isBidirectional: true)
};
var map = new ProjectionMap("/src/App.jazor", "/src/App.jazor.cs", segments);

// 投影 → 源
if (map.TryMapToOriginalOffset(50, out var originalOffset))
{
    Console.WriteLine($"投影偏移 50 → 源偏移 {originalOffset}");  // 输出：25
}
```

## 11. 未来扩展方向

### 11.1 可能的改进

1. **二分查找优化**：对大量段使用二分查找，提高性能
2. **段压缩**：自动合并相邻的段，减少内存占用
3. **缓存层**：缓存频繁映射的位置，避免重复计算
4. **增量更新**：支持增量添加段，而非重建整个 `ProjectionMap`

### 11.2 扩展点

- **自定义段比较器**：当前固定使用 `OriginalStart` 排序，可支持自定义排序
- **映射策略**：当前固定线性插值，可支持非线性映射
- **验证器**：当前固定验证排序和不重叠，可支持更复杂的验证规则
