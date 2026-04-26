# Source Map Service 子系统

> Status: 活跃参考
> Positioning: Jolt Source Map 管理层，提供 Source Map v3 解析、双向映射和源码查询

## 1. 文档定位

Source Map Service 子系统是 Jolt 调试和开发服务器支持的核心组件，负责管理编译输出与源代码之间的映射关系。该子系统位于 `src/Jolt/SourceMap/` 目录下，实现完整的 Source Map v3 规范，支持原始位置到生成位置的双向映射。

核心设计目标：
- 提供完整的 Source Map v3 支持
- 支持双向映射（原始位置 ↔ 生成位置）
- 线程安全的并发访问
- 高效的 VLQ 解码和映射查找
- 支持源码内容嵌入

## 2. 核心类型

### 2.1 `ISourceMapService` 接口

**文件位置**：`src/Jolt/SourceMap/ISourceMapService.cs`

Source Map 服务的公共接口：

```csharp
public interface ISourceMapService
{
    // 注册 Source Map（generatedPath → sourceMapJson）
    void Register(string generatedPath, string sourceMapJson);

    // 注销 Source Map
    void Unregister(string generatedPath);

    // 清除所有 Source Map
    void Clear();

    // 获取原始 Source Map JSON
    string? GetSourceMapJson(string generatedPath);

    // 生成位置 → 原始位置
    OriginalPosition? OriginalPositionFor(string generatedPath, int line, int column);

    // 原始位置 → 生成位置
    GeneratedPosition? GeneratedPositionFor(string sourcePath, int line, int column);

    // 获取源码内容
    string? GetSourceContent(string generatedPath, int sourceIndex);
}
```

**坐标系统**：
- **零基准**：行和列都从 0 开始
- **Source Map v3 兼容**：与标准规范完全一致

### 2.2 `OriginalPosition` 记录

**文件位置**：`src/Jolt/SourceMap/ISourceMapService.cs`（行 24-28）

原始位置的数据结构：

```csharp
public sealed record OriginalPosition(
    string SourcePath,    // 源文件路径
    int Line,             // 源文件行号（0-based）
    int Column,           // 源文件列号（0-based）
    int SourceIndex);     // 源文件索引（在 sources 数组中的位置）
```

### 2.3 `GeneratedPosition` 记录

**文件位置**：`src/Jolt/SourceMap/ISourceMapService.cs`（行 30-33）

生成位置的数据结构：

```csharp
public sealed record GeneratedPosition(
    string GeneratedPath, // 生成文件路径
    int Line,             // 生成文件行号（0-based）
    int Column);          // 生成文件列号（0-based）
```

### 2.4 `InMemorySourceMapService` 实现

**文件位置**：`src/Jolt/SourceMap/InMemorySourceMapService.cs`

内存中的 Source Map 服务实现：

```csharp
internal sealed class InMemorySourceMapService : ISourceMapService
{
    private const int MaxVlqDigitsPerValue = 7;
    private readonly Dictionary<string, RegisteredSourceMap> _maps =
        new(StringComparer.OrdinalIgnoreCase);
    private readonly Lock _gate = new();
}
```

**设计特点**：
- 使用 `Lock` (C# 13) 确保线程安全
- 路径比较大小写不敏感
- 预解析 Source Map JSON 为内部结构

### 2.5 内部数据结构

**RegisteredSourceMap**：

```csharp
private readonly record struct RegisteredSourceMap(
    string GeneratedPath,                    // 生成文件路径
    string RawJson,                          // 原始 JSON
    IReadOnlyList<RegisteredSource> Sources, // 源文件列表
    IReadOnlyList<RegisteredSegment> Segments); // 映射段列表
```

**RegisteredSource**：

```csharp
private readonly record struct RegisteredSource(
    string Path,     // 源文件路径
    string? Content); // 源码内容（可能为 null）
```

**RegisteredSegment**：

```csharp
private readonly record struct RegisteredSegment(
    int GeneratedLine,   // 生成文件行号
    int GeneratedColumn, // 生成文件列号
    int SourceIndex,     // 源文件索引
    int SourceLine,      // 源文件行号
    int SourceColumn);   // 源文件列号
```

## 3. 核心算法

### 3.1 Source Map v3 JSON 解析

**方法**：`Parse(string generatedPath, string sourceMapJson)`

**文件位置**：`src/Jolt/SourceMap/InMemorySourceMapService.cs`（行 141-186）

**算法流程**：

```
解析 JSON (JsonDocument.Parse)
    ↓
提取 sources 数组
    ↓
提取 sourcesContent 数组（可选）
    ↓
验证数组长度一致性
    ↓
构建 RegisteredSource 列表
    ↓
提取 mappings 字符串
    ↓
解码 VLQ mappings (DecodeMappings)
    ↓
返回 RegisteredSourceMap
```

**JSON 结构解析**：

```csharp
using var document = JsonDocument.Parse(sourceMapJson);
var root = document.RootElement;

// 1. 解析 sources 数组
var sourcesArray = root.TryGetProperty("sources", out var sourcesElement)
    && sourcesElement.ValueKind == JsonValueKind.Array
    ? sourcesElement
    : default;

// 2. 解析 sourcesContent 数组（可选）
var sourcesContentArray = root.TryGetProperty("sourcesContent", out var contentElement)
    && contentElement.ValueKind == JsonValueKind.Array
    ? contentElement
    : default;

// 3. 验证长度一致性
var sourceCount = sourcesArray.ValueKind == JsonValueKind.Array
    ? sourcesArray.GetArrayLength()
    : 0;
if (sourcesContentArray.ValueKind == JsonValueKind.Array
    && sourcesContentArray.GetArrayLength() != sourceCount)
{
    throw new InvalidOperationException("Source map sourcesContent length must match sources length.");
}

// 4. 构建源文件列表
var sources = new List<RegisteredSource>(sourceCount);
if (sourcesArray.ValueKind == JsonValueKind.Array)
{
    for (var index = 0; index < sourcesArray.GetArrayLength(); index++)
    {
        var sourcePath = sourcesArray[index].GetString() ?? string.Empty;
        string? content = null;
        if (sourcesContentArray.ValueKind == JsonValueKind.Array
            && index < sourcesContentArray.GetArrayLength())
        {
            content = sourcesContentArray[index].ValueKind == JsonValueKind.Null
                ? null
                : sourcesContentArray[index].GetString();
        }
        sources.Add(new RegisteredSource(sourcePath, content));
    }
}

// 5. 解析 mappings
var mappings = root.TryGetProperty("mappings", out var mappingsElement)
    ? mappingsElement.GetString() ?? string.Empty
    : string.Empty;

return new RegisteredSourceMap(
    NormalizePath(generatedPath),
    sourceMapJson,
    sources.ToArray(),
    DecodeMappings(mappings));
```

### 3.2 VLQ 解码器

**方法**：`DecodeVlq(string mappings, ref int position)`

**文件位置**：`src/Jolt/SourceMap/InMemorySourceMapService.cs`（行 241-275）

**VLQ 编码规则**：

Source Map 使用 Base64 VLQ (Variable-Length Quantity) 编码来压缩映射数据：
- 每个 VLQ 数字使用一个或多个 Base64 字符
- 每个字符 6 位有效数据（5 位数据 + 1 位延续标志）
- 延续标志位（bit 5）：1 = 继续读取，0 = 最后一个字符
- 最低位（bit 0）是符号位：1 = 负数，0 = 正数

**解码算法**：

```csharp
private static int DecodeVlq(string mappings, ref int position)
{
    var result = 0;
    var shift = 0;
    var continuation = true;
    var digitCount = 0;

    while (continuation)
    {
        // 安全检查
        if (position >= mappings.Length)
        {
            throw new InvalidOperationException("Unexpected end of VLQ mapping.");
        }

        digitCount++;
        if (digitCount > MaxVlqDigitsPerValue)
        {
            throw new InvalidOperationException("VLQ mapping value exceeds the supported digit length.");
        }

        // 解码 Base64 字符
        var digit = DecodeBase64(mappings[position++]);

        // 检查延续标志
        continuation = (digit & 32) != 0;

        // 移除延续标志，保留 5 位数据
        digit &= 31;

        // 检查整数溢出
        if (shift >= 31 || digit > (int.MaxValue >> shift))
        {
            throw new InvalidOperationException("VLQ mapping value exceeds the supported integer range.");
        }

        // 累加数据位
        result += digit << shift;
        shift += 5;
    }

    // 解析符号
    var isNegative = (result & 1) == 1;
    result >>= 1;

    return isNegative ? -result : result;
}
```

**Base64 解码**：

```csharp
private static int DecodeBase64(char value)
    => value switch
    {
        >= 'A' and <= 'Z' => value - 'A',        // 0-25
        >= 'a' and <= 'z' => value - 'a' + 26,   // 26-51
        >= '0' and <= '9' => value - '0' + 52,   // 52-61
        '+' => 62,                               // 62
        '/' => 63,                               // 63
        _ => throw new InvalidOperationException($"Invalid base64 VLQ digit '{value}'.")
    };
```

**示例**：

假设 VLQ 编码为 "gkB"：

1. 解码 'g'：
   - Base64 值：16
   - 延续标志：16 & 32 = 0（停止）
   - 数据位：16 & 31 = 16
   - 累加：result = 16 << 0 = 16

2. 解码 'k'：
   - Base64 值：10
   - 延续标志：10 & 32 = 0（停止）
   - 数据位：10 & 31 = 10
   - 累加：result = 10 << 0 = 10

3. 解码 'B'：
   - Base64 值：1
   - 延续标志：1 & 32 = 0（停止）
   - 数据位：1 & 31 = 1
   - 符号位：1 & 1 = 1（负数）
   - 右移：1 >> 1 = 0
   - 结果：-0 = 0

### 3.3 Mappings 字符串解码

**方法**：`DecodeMappings(string mappings)`

**文件位置**：`src/Jolt/SourceMap/InMemorySourceMapService.cs`（行 188-239）

**Mappings 格式**：

```
generatedLine1: [segment1, segment2, ...]
generatedLine2: [segment3, segment4, ...]
...
```

- 段之间用 `,` 分隔
- 行之间用 `;` 分隔
- 每个段包含 1-5 个 VLQ 编码的值：
  1. 生成的列号（必需）
  2. 源文件索引（可选）
  3. 源文件行号（可选）
  4. 源文件列号（可选）
  5. 名称索引（可选，当前实现忽略）

**解码算法**：

```csharp
private static IReadOnlyList<RegisteredSegment> DecodeMappings(string mappings)
{
    var segments = new List<RegisteredSegment>();
    var generatedLine = 0;
    var previousGeneratedColumn = 0;
    var previousSourceIndex = 0;
    var previousSourceLine = 0;
    var previousSourceColumn = 0;
    var position = 0;

    while (position < mappings.Length)
    {
        var current = mappings[position];

        // 分号表示新行
        if (current == ';')
        {
            generatedLine++;
            previousGeneratedColumn = 0;
            position++;
            continue;
        }

        // 逗号表示跳过
        if (current == ',')
        {
            position++;
            continue;
        }

        // 1. 解码生成的列号（相对于上一个段）
        var generatedColumn = previousGeneratedColumn + DecodeVlq(mappings, ref position);
        previousGeneratedColumn = generatedColumn;

        // 检查是否为空段
        if (position >= mappings.Length || mappings[position] == ',' || mappings[position] == ';')
        {
            continue;
        }

        // 2. 解码源文件索引（相对于上一个段）
        var sourceIndex = previousSourceIndex + DecodeVlq(mappings, ref position);

        // 3. 解码源文件行号（相对于上一个段）
        var sourceLine = previousSourceLine + DecodeVlq(mappings, ref position);

        // 4. 解码源文件列号（相对于上一个段）
        var sourceColumn = previousSourceColumn + DecodeVlq(mappings, ref position);

        // 更新前一个值
        previousSourceIndex = sourceIndex;
        previousSourceLine = sourceLine;
        previousSourceColumn = sourceColumn;

        // 5. 跳过名称索引（当前实现不使用）
        if (position < mappings.Length && mappings[position] != ',' && mappings[position] != ';')
        {
            _ = DecodeVlq(mappings, ref position);
        }

        // 添加段到列表
        segments.Add(new RegisteredSegment(
            generatedLine,
            generatedColumn,
            sourceIndex,
            sourceLine,
            sourceColumn));
    }

    return segments;
}
```

**示例**：

假设 mappings 为 `"AAgBC,SAAsB"`：

1. 初始状态：
   - generatedLine = 0
   - previousGeneratedColumn = 0

2. 解码第一个段 "AAgBC"：
   - generatedColumn: 0 + 0 = 0
   - sourceIndex: 0 + 16 = 16
   - sourceLine: 0 + 11 = 11
   - sourceColumn: 0 + 1 = 1
   - 段：(0, 0, 16, 11, 1)

3. 解码第二个段 "SAAsB"：
   - generatedColumn: 0 + 18 = 18
   - sourceIndex: 16 + 0 = 16
   - sourceLine: 11 + 28 = 39
   - sourceColumn: 1 + 0 = 1
   - 段：(0, 18, 16, 39, 1)

### 3.4 原始位置查找

**方法**：`OriginalPositionFor(string generatedPath, int line, int column)`

**文件位置**：`src/Jolt/SourceMap/InMemorySourceMapService.cs`（行 53-74）

**算法流程**：

```
获取 Source Map（读锁）
    ↓
查找最后一个 ≤ (line, column) 的段
    ↓
验证段的源文件索引有效性
    ↓
返回 OriginalPosition
```

**查找算法**：

```csharp
public OriginalPosition? OriginalPositionFor(string generatedPath, int line, int column)
{
    ArgumentException.ThrowIfNullOrWhiteSpace(generatedPath);

    // 1. 获取 Source Map（读锁）
    RegisteredSourceMap sourceMap;
    lock (_gate)
    {
        if (!_maps.TryGetValue(NormalizePath(generatedPath), out sourceMap))
        {
            return null;
        }
    }

    // 2. 查找最后一个 ≤ (line, column) 的段
    var segment = FindLastSegmentAtOrBefore(sourceMap.Segments, line, column);
    if (!segment.HasValue
        || segment.Value.SourceIndex < 0
        || segment.Value.SourceIndex >= sourceMap.Sources.Count)
    {
        return null;
    }

    // 3. 返回原始位置
    var source = sourceMap.Sources[segment.Value.SourceIndex];
    return new OriginalPosition(
        source.Path,
        segment.Value.SourceLine,
        segment.Value.SourceColumn,
        segment.Value.SourceIndex);
}
```

**FindLastSegmentAtOrBefore 算法**：

```csharp
private static RegisteredSegment? FindLastSegmentAtOrBefore(
    IReadOnlyList<RegisteredSegment> segments,
    int generatedLine,
    int generatedColumn)
{
    RegisteredSegment? candidate = null;

    foreach (var segment in segments)
    {
        // 行号超过，停止
        if (segment.GeneratedLine > generatedLine)
        {
            break;
        }

        // 同行但列号超过，停止
        if (segment.GeneratedLine == generatedLine && segment.GeneratedColumn > generatedColumn)
        {
            break;
        }

        // 记录候选段
        candidate = segment;
    }

    return candidate;
}
```

**特点**：
- 线性搜索（假设段是有序的）
- 返回最后一个 ≤ 查询点的段
- O(n) 时间复杂度

### 3.5 生成位置查找（反向映射）

**方法**：`GeneratedPositionFor(string sourcePath, int line, int column)`

**文件位置**：`src/Jolt/SourceMap/InMemorySourceMapService.cs`（行 76-118）

**算法流程**：

```
获取所有 Source Map（读锁）
    ↓
遍历每个 Source Map
    ↓
遍历每个段
    ↓
检查段的源文件是否匹配
    ↓ 是
评分候选段
    ↓
选择最佳候选
    ↓
返回 GeneratedPosition
```

**路径匹配算法**：

```csharp
private static bool PathMatches(string left, string right)
{
    var normalizedLeft = NormalizePath(left);
    var normalizedRight = NormalizePath(right);

    // 1. 完全匹配（大小写不敏感）
    if (string.Equals(normalizedLeft, normalizedRight, StringComparison.OrdinalIgnoreCase))
    {
        return true;
    }

    // 2. 任一为空，不匹配
    if (string.IsNullOrWhiteSpace(normalizedLeft) || string.IsNullOrWhiteSpace(normalizedRight))
    {
        return false;
    }

    // 3. 后缀匹配（支持相对路径）
    return normalizedLeft.EndsWith("/" + normalizedRight, StringComparison.OrdinalIgnoreCase)
        || normalizedRight.EndsWith("/" + normalizedLeft, StringComparison.OrdinalIgnoreCase);
}
```

**候选段评分算法**：

```csharp
private readonly record struct Candidate(
    string GeneratedPath,
    int GeneratedLine,
    int GeneratedColumn,
    int LineDelta,      // 源文件行号差
    int ColumnDelta)    // 源文件列号差
    : IComparable<Candidate>
{
    public int CompareTo(Candidate other)
    {
        // 1. 精确行匹配优先
        var exactLineScore = LineDelta == 0 ? 0 : 1;
        var otherExactLineScore = other.LineDelta == 0 ? 0 : 1;
        var comparison = exactLineScore.CompareTo(otherExactLineScore);
        if (comparison != 0)
        {
            return comparison;
        }

        // 2. 行号差越小越好（向前优先）
        var forwardLinePenalty = LineDelta >= 0 ? LineDelta : int.MaxValue / 2 + Math.Abs(LineDelta);
        var otherForwardLinePenalty = other.LineDelta >= 0 ? other.LineDelta : int.MaxValue / 2 + Math.Abs(other.LineDelta);
        comparison = forwardLinePenalty.CompareTo(otherForwardLinePenalty);
        if (comparison != 0)
        {
            return comparison;
        }

        // 3. 列号差越小越好（向前优先）
        var forwardColumnPenalty = ColumnDelta >= 0 ? ColumnDelta : int.MaxValue / 2 + Math.Abs(ColumnDelta);
        var otherForwardColumnPenalty = other.ColumnDelta >= 0 ? other.ColumnDelta : int.MaxValue / 2 + Math.Abs(other.ColumnDelta);
        comparison = forwardColumnPenalty.CompareTo(otherForwardColumnPenalty);
        if (comparison != 0)
        {
            return comparison;
        }

        // 4. 生成行号越小越好（更靠前的代码）
        comparison = GeneratedLine.CompareTo(other.GeneratedLine);
        if (comparison != 0)
        {
            return comparison;
        }

        // 5. 生成列号越小越好
        return GeneratedColumn.CompareTo(other.GeneratedColumn);
    }
}
```

**评分启发式**：

| 优先级 | 标准 | 说明 |
|-------|------|------|
| 1 | 精确行匹配 | 行号差为 0 的候选优先 |
| 2 | 向前行差 | 向前的行差优于向后（负数惩罚） |
| 3 | 向前列差 | 向前的列差优于向后（负数惩罚） |
| 4 | 生成行号 | 更早的代码优先 |
| 5 | 生成列号 | 更早的列号优先 |

**主算法**：

```csharp
public GeneratedPosition? GeneratedPositionFor(string sourcePath, int line, int column)
{
    ArgumentException.ThrowIfNullOrWhiteSpace(sourcePath);

    // 1. 获取所有 Source Map（读锁）
    RegisteredSourceMap[] sourceMaps;
    lock (_gate)
    {
        sourceMaps = _maps.Values.ToArray();
    }

    // 2. 遍历所有 Source Map 和段
    Candidate? bestCandidate = null;
    foreach (var sourceMap in sourceMaps)
    {
        foreach (var segment in sourceMap.Segments)
        {
            // 验证源文件索引
            if (segment.SourceIndex < 0 || segment.SourceIndex >= sourceMap.Sources.Count)
            {
                continue;
            }

            // 检查路径匹配
            var source = sourceMap.Sources[segment.SourceIndex];
            if (!PathMatches(source.Path, sourcePath))
            {
                continue;
            }

            // 创建候选段并评分
            var candidate = new Candidate(
                sourceMap.GeneratedPath,
                segment.GeneratedLine,
                segment.GeneratedColumn,
                segment.SourceLine - line,
                segment.SourceColumn - column);

            if (bestCandidate is null || candidate.CompareTo(bestCandidate.Value) < 0)
            {
                bestCandidate = candidate;
            }
        }
    }

    // 3. 返回最佳候选
    return bestCandidate is null
        ? null
        : new GeneratedPosition(
            bestCandidate.Value.GeneratedPath,
            bestCandidate.Value.GeneratedLine,
            bestCandidate.Value.GeneratedColumn);
}
```

**特点**：
- O(n × m) 时间复杂度（n = Source Map 数量，m = 每个映射的段数）
- 全局搜索所有已注册的 Source Map
- 启发式评分选择最佳匹配

### 3.6 路径规范化

**方法**：`NormalizePath(string path)`

**文件位置**：`src/Jolt/SourceMap/InMemorySourceMapService.cs`（行 327-354）

**规范化规则**：

```csharp
private static string NormalizePath(string path)
{
    if (string.IsNullOrWhiteSpace(path))
    {
        return string.Empty;
    }

    var normalized = path.Trim();

    // 1. 解析 URI
    if (Uri.TryCreate(normalized, UriKind.Absolute, out var uri))
    {
        if (uri.IsFile)
        {
            normalized = uri.LocalPath;
        }
        else if (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps)
        {
            normalized = uri.AbsolutePath;
        }
    }

    // 2. 统一分隔符
    normalized = normalized.Replace('\\', '/');

    // 3. 移除 ./ 前缀
    while (normalized.StartsWith("./", StringComparison.Ordinal))
    {
        normalized = normalized[2..];
    }

    return normalized;
}
```

**示例**：

| 输入路径 | 规范化结果 |
|---------|----------|
| `C:\Project\src\file.js` | `C:/Project/src/file.js` |
| `./src/components/Button.vue` | `src/components/Button.vue` |
| `http://example.com/app.js` | `/app.js` |
| `file:///C:/Project/dist/bundle.js` | `C:/Project/dist/bundle.js` |

## 4. 线程安全模型

### 4.1 锁策略

**使用 `Lock` (C# 13)**：

```csharp
private readonly Lock _gate = new();
```

**读写操作**：

```csharp
// 读操作：使用读锁
public OriginalPosition? OriginalPositionFor(string generatedPath, int line, int column)
{
    RegisteredSourceMap sourceMap;
    lock (_gate)  // 使用锁（虽然当前是简单锁，未来可升级为读写锁）
    {
        if (!_maps.TryGetValue(NormalizePath(generatedPath), out sourceMap))
        {
            return null;
        }
    }
    // 在锁外执行计算
    var segment = FindLastSegmentAtOrBefore(sourceMap.Segments, line, column);
    // ...
}

// 写操作：使用写锁
public void Register(string generatedPath, string sourceMapJson)
{
    var parsed = Parse(generatedPath, sourceMapJson);
    lock (_gate)
    {
        _maps[NormalizePath(generatedPath)] = parsed;
    }
}
```

**特点**：
- 粗粒度锁（整个字典）
- 读操作在锁外执行计算
- 写操作在锁内完成

### 4.2 不变性保证

**RegisteredSourceMap 不可变**：

```csharp
private readonly record struct RegisteredSourceMap(
    string GeneratedPath,
    string RawJson,
    IReadOnlyList<RegisteredSource> Sources,
    IReadOnlyList<RegisteredSegment> Segments);
```

- 使用 `record struct` 确保不可变性
- 所有字段都是 `readonly`
- 列表类型是 `IReadOnlyList`

**好处**：
- 读操作无需锁（仅访问时需要锁）
- 无数据竞争
- 线程安全的快照

### 4.3 并发场景

| 操作类型 | 并发行为 | 保证 |
|---------|---------|------|
| 多个读操作 | 串行化（锁） | 最后一致 |
| 读 + 写操作 | 串行化（锁） | 强一致 |
| 多个写操作 | 串行化（锁） | 强一致 |

**性能考虑**：
- 当前使用简单锁（未区分读写）
- 读操作较多时可升级为 `ReaderWriterLockSlim`
- 计算密集型操作在锁外执行

## 5. 错误处理

### 5.1 参数验证

**空值检查**：

```csharp
ArgumentException.ThrowIfNullOrWhiteSpace(generatedPath);
ArgumentException.ThrowIfNullOrWhiteSpace(sourceMapJson);
ArgumentException.ThrowIfNullOrWhiteSpace(sourcePath);
```

**索引范围检查**：

```csharp
if (sourceIndex < 0 || sourceIndex >= sourceMap.Sources.Count)
{
    return null;
}
```

### 5.2 VLQ 解码错误

**异常类型**：`InvalidOperationException`

**错误场景**：

1. **意外的 VLQ 结束**：

```csharp
if (position >= mappings.Length)
{
    throw new InvalidOperationException("Unexpected end of VLQ mapping.");
}
```

2. **VLQ 数字长度超限**：

```csharp
if (digitCount > MaxVlqDigitsPerValue)
{
    throw new InvalidOperationException("VLQ mapping value exceeds the supported digit length.");
}
```

3. **整数溢出**：

```csharp
if (shift >= 31 || digit > (int.MaxValue >> shift))
{
    throw new InvalidOperationException("VLQ mapping value exceeds the supported integer range.");
}
```

4. **无效的 Base64 字符**：

```csharp
_ => throw new InvalidOperationException($"Invalid base64 VLQ digit '{value}'.")
```

### 5.3 JSON 解析错误

**sourcesContent 长度不一致**：

```csharp
if (sourcesContentArray.ValueKind == JsonValueKind.Array
    && sourcesContentArray.GetArrayLength() != sourceCount)
{
    throw new InvalidOperationException("Source map sourcesContent length must match sources length.");
}
```

**异常传播**：
- `JsonDocument.Parse` 可能抛出 `JsonException`
- 不捕获，向上传播

### 5.4 路径处理

**空白路径**：

```csharp
private static string NormalizePath(string path)
{
    if (string.IsNullOrWhiteSpace(path))
    {
        return string.Empty;
    }
    // ...
}
```

**URI 解析失败**：

```csharp
if (Uri.TryCreate(normalized, UriKind.Absolute, out var uri))
{
    // 成功
}
// 失败时继续处理为普通路径
```

## 6. 配置选项

### 6.1 VLQ 数字长度限制

```csharp
private const int MaxVlqDigitsPerValue = 7;
```

**影响**：
- 最多支持 7 个 Base64 字符的 VLQ 数字
- 最大值范围：±(2^35 - 1) ≈ ±343 亿

**原因**：
- 防止恶意构造的 Source Map 导致无限循环
- 足够大的范围覆盖实际使用场景

### 6.2 路径比较策略

**当前实现**：`StringComparer.OrdinalIgnoreCase`

**影响**：
- Windows 和 Linux 上路径匹配行为一致
- 大小写不敏感的路径查找

### 6.3 反向映射评分策略

**硬编码的优先级**：

1. 精确行匹配
2. 向前行差
3. 向前列差
4. 生成行号
5. 生成列号

**影响**：
- 不可配置的评分策略
- 适用于大多数调试场景
- 可能不适合特殊需求

## 7. 与其他子系统的交互

### 7.1 DevServer 交互

**HtmlTransformer** 使用 Source Map 服务：

```
DevServer 构建完成
    ↓
生成 Source Map JSON
    ↓
ISourceMapService.Register(generatedPath, sourceMapJson)
    ↓
存储到内存
```

### 7.2 Debug Protocol 交互

**DapRequestHandler** 使用 Source Map 服务：

```
Debug Adapter 收到断点请求（源文件位置）
    ↓
ISourceMapService.OriginalPositionFor(generatedPath, line, column)
    ↓
查找原始位置
    ↓
设置断点到正确位置
```

**堆栈跟踪转换**：

```
暂停执行（生成文件位置）
    ↓
ISourceMapService.GeneratedPositionFor(sourcePath, line, column)
    ↓
查找生成位置
    ↓
映射堆栈帧到源文件
```

### 7.3 LSP 服务交互

**LspSession** 使用 Source Map 服务：

```
LSP 客户端请求定义（生成文件位置）
    ↓
ISourceMapService.OriginalPositionFor(generatedPath, line, column)
    ↓
查找原始位置
    ↓
返回源文件定义
```

## 8. 设计权衡

### 8.1 反向映射算法

**当前选择**：全局搜索 + 启发式评分

**优点**：
- 支持多文件 Source Map 查找
- 不需要预先构建反向索引
- 内存占用小

**缺点**：
- O(n × m) 时间复杂度（慢）
- 启发式评分可能不准确
- 无法处理一对多映射

**适用场景**：
- Source Map 数量有限（< 100）
- 调试时的交互式查询
- 内存受限环境

**未来改进方向**：
- 构建反向索引（sourcePath → segments）
- 使用空间索引（如 R-tree）
- 缓存热门查询

### 8.2 线程安全策略

**当前选择**：简单锁（`Lock`）

**优点**：
- 实现简单
- 强一致性
- 低 bug 风险

**缺点**：
- 读操作互斥（性能瓶颈）
- 无法并发读取

**适用场景**：
- 读操作不频繁
- 写操作较少
- 简单的并发模型

**未来改进方向**：
- 升级到 `ReaderWriterLockSlim`
- 无锁读取（ImmutableDictionary）
- 分片锁

### 8.3 路径规范化策略

**当前选择**：存储时规范化，查询时也规范化

**优点**：
- 存储键始终一致
- 支持多种路径格式
- URI 友好

**缺点**：
- 可能丢失原始路径信息
- 规范化逻辑复杂

**适用场景**：
- 跨平台兼容性优先
- 支持多种输入格式
- 不需要原始路径

### 8.4 Source Map 存储策略

**当前选择**：内存存储（Dictionary）

**优点**：
- 极快的访问速度
- 无 I/O 开销
- 简单的生命周期管理

**缺点**：
- 进程重启后数据丢失
- 内存消耗随 Source Map 数量增长
- 无持久化历史

**适用场景**：
- Source Map 由外部系统管理
- 重启后可以重新生成
- 不需要跨会 persist

**未来改进方向**：
- 持久化到磁盘
- LRU 缓存策略
- 增量更新

### 8.5 名称索引支持

**当前选择**：忽略名称索引（第 5 个 VLQ 值）

**优点**：
- 简化实现
- 减少内存占用
- 覆盖主要使用场景

**缺点**：
- 无法支持变量名重命名
- 调试时变量名不匹配

**适用场景**：
- 仅需要位置映射
- 不需要变量名映射
- 简化实现

**未来改进方向**：
- 解析并存储 names 数组
- 返回名称信息
- 支持变量名查询

---

**文档维护者**：developerhan
**最后更新**：2026-04-21
**参考源文件**：
- `src/Jolt/SourceMap/ISourceMapService.cs`
- `src/Jolt/SourceMap/InMemorySourceMapService.cs`
