# Jolt 文档类型系统

> 状态：已实现
> 定位：Jolt 内部文档版本管理和文本操作的核心类型，位于 `Jolt.Protocol.Documents` 命名空间

## 1. 文档定位

本文档描述 Jolt 的文档类型系统，包括文档版本管理、文本区间和文本变更。这些类型用于 LSP 文档同步、增量编译和文本操作。

**与契约层的关系**：
- **契约层**（`Jazor.VueContracts.Protocol`）：跨项目共享的 RPC 契约，包含 `DocumentSnapshot`
- **文档层**（`Jolt.Protocol.Documents`）：Jolt 内部文档类型，包含 `DocumentVersion`、`TextSpan`、`TextChange`

**命名空间差异原因**：
- `DocumentSnapshot` 在 RPC 协议中传输，需要与前端分析器共享
- `DocumentVersion`、`TextSpan`、`TextChange` 是 Jolt 内部实现细节，不需要跨项目共享

## 2. 核心类型

### 2.1 DocumentVersion (`DocumentVersion.cs`)

**职责**：表示文档的版本标识，提供强类型封装和验证。

**类型定义**：
```csharp
public readonly record struct DocumentVersion
{
    public DocumentVersion(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);
        Value = value;
    }

    public string Value { get; }

    public override string ToString() => Value;
}
```

**代码位置**：`src/Jolt/Protocol/Documents/DocumentVersion.cs`

**设计特点**：
- `record struct`：值类型，语义上不可变
- 构造函数验证：拒绝空白字符串
- `ToString()` 重写：直接返回 `Value`，便于日志输出

### 2.2 工厂方法

#### 2.2.1 Create(int version)

**职责**：从整数版本号创建 `DocumentVersion`，验证非负性。

**实现**：
```csharp
public static DocumentVersion Create(int version)
{
    if (!TryCreate(version, out var documentVersion))
    {
        throw new ArgumentOutOfRangeException(nameof(version), version, "Document version must be non-negative.");
    }

    return documentVersion;
}
```

**使用场景**：
```csharp
// LSP 文档版本通常从 1 开始递增
var version = DocumentVersion.Create(1);
var nextVersion = DocumentVersion.Create(version + 1);
```

**验证规则**：
- `version >= 0`：接受
- `version < 0`：抛出 `ArgumentOutOfRangeException`

#### 2.2.2 TryCreate(string? value, out DocumentVersion)

**职责**：安全地从字符串创建 `DocumentVersion`，避免异常。

**实现**：
```csharp
public static bool TryCreate(string? value, out DocumentVersion documentVersion)
{
    documentVersion = default;
    if (string.IsNullOrWhiteSpace(value))
    {
        return false;
    }

    documentVersion = new DocumentVersion(value);
    return true;
}
```

**使用场景**：
```csharp
// 从可选的版本字符串创建
if (DocumentVersion.TryCreate(versionString, out var version))
{
    // 使用 version
}
else
{
    // 处理无效版本
}
```

**验证规则**：
- `null` 或空白字符串：返回 `false`
- 非空白字符串：返回 `true` 并设置 `documentVersion`

#### 2.2.3 TryCreate(int version, out DocumentVersion)

**职责**：安全地从整数创建 `DocumentVersion`，避免异常。

**实现**：
```csharp
public static bool TryCreate(int version, out DocumentVersion documentVersion)
{
    documentVersion = default;
    if (version < 0)
    {
        return false;
    }

    documentVersion = new DocumentVersion(version.ToString(CultureInfo.InvariantCulture));
    return true;
}
```

**使用场景**：
```csharp
// 从可能无效的整数版本创建
if (DocumentVersion.TryCreate(versionNumber, out var version))
{
    // 使用 version
}
```

**验证规则**：
- `version < 0`：返回 `false`
- `version >= 0`：返回 `true` 并设置 `documentVersion`

**设计决策**：
- 使用 `CultureInfo.InvariantCulture` 确保整数到字符串的转换一致性
- 避免 `ToString()` 的本地化差异（如某些文化的数字分隔符）

### 2.3 TextSpan (`TextSpan.cs`)

**职责**：表示文本中的区间，使用起始位置和长度。

**类型定义**：
```csharp
public readonly record struct TextSpan(int Start, int Length)
{
    public int End => Start + Math.Max(Length, 0);
}
```

**代码位置**：`src/Jolt/Protocol/Documents/TextSpan.cs`

**设计特点**：
- `record struct`：值类型，支持结构化比较
- **主构造函数**：C# 12 特性，简化属性定义
- **计算属性 `End`**：使用 `Math.Max(Length, 0)` 确保非负长度

**区间语义**：
- **闭开区间**：`[Start, End)`，即包含 `Start`，不包含 `End`
- **示例**：`TextSpan(5, 3)` 表示位置 5、6、7（共 3 个字符）

### 2.3.1 End 属性计算

```csharp
public int End => Start + Math.Max(Length, 0);
```

**使用 `Math.Max(Length, 0)` 的原因**：
- **防御性编程**：即使 `Length` 为负（理论上不应发生），`End` 也不会小于 `Start`
- **语义一致性**：空区间（`Length = 0`）的 `End = Start`，符合 `[Start, Start)` 的语义
- **避免溢出**：`Math.Max` 确保加法操作数非负，减少整数溢出风险

**示例计算**：
| Start | Length | End | 区间 |
|-------|--------|-----|------|
| 0 | 5 | 5 | `[0, 5)` |
| 10 | 0 | 10 | `[10, 10)` (空区间) |
| 5 | -1 | 5 | `[5, 5)` (Length 为负时视为 0) |

### 2.4 TextChange (`TextChange.cs`)

**职责**：表示文本变更，包含区间和新文本。

**类型定义**：
```csharp
public sealed record TextChange(TextSpan Span, string NewText);
```

**代码位置**：`src/Jolt/Protocol/Documents/TextChange.cs`

**设计特点**：
- `record`：引用类型，支持结构化比较和 `with` 表达式
- **不可变**：所有属性通过构造函数初始化
- **简洁**：单个字段定义，使用 C# 12 主构造函数

**变更语义**：
- **替换**：删除 `Span` 区间的旧文本，插入 `NewText`
- **删除**：`NewText` 为空字符串
- **插入**：`Span.Length` 为 0（空区间）

**示例**：
```csharp
// 替换：将位置 5-7 的文本替换为 "abc"
var replace = new TextChange(new TextSpan(5, 3), "abc");

// 删除：删除位置 10-15 的文本
var delete = new TextChange(new TextSpan(10, 5), "");

// 插入：在位置 20 插入 "xyz"
var insert = new TextChange(new TextSpan(20, 0), "xyz");
```

## 3. 核心算法

### 3.1 文本区间验证算法

**问题**：确保 `TextSpan` 的 `Start` 和 `Length` 组合合法

**算法**：
```csharp
// 1. 检查 Start 非负
if (span.Start < 0)
    throw new ArgumentOutOfRangeException("Start must be non-negative.");

// 2. 检查 Length 非负
if (span.Length < 0)
    throw new ArgumentOutOfRangeException("Length must be non-negative.");

// 3. 检查 End 不越界（如果知道文本长度）
if (span.End > textLength)
    throw new ArgumentOutOfRangeException("Span extends beyond text length.");
```

**当前实现**：
- `TextSpan` 本身不进行验证（构造函数无检查）
- 验证由使用者负责（如 LSP 层）

**权衡**：
- **优点**：轻量级，性能高
- **缺点**：可能创建非法区间（如 `Start = -1`）

### 3.2 文本变更应用算法

**问题**：将 `TextChange` 应用到源文本，生成新文本

**算法**：
```csharp
public static string ApplyChange(string source, TextChange change)
{
    // 1. 验证区间
    if (change.Span.Start < 0 || change.Span.Start > source.Length)
        throw new ArgumentOutOfRangeException("Span start out of range.");

    if (change.Span.End > source.Length)
        throw new ArgumentOutOfRangeException("Span end out of range.");

    // 2. 分割源文本
    var before = source.Substring(0, change.Span.Start);
    var after = source.Substring(change.Span.End);

    // 3. 拼接新文本
    return before + change.NewText + after;
}
```

**示例**：
```csharp
var source = "Hello, world!";
var change = new TextChange(new TextSpan(7, 5), "C#");
var result = ApplyChange(source, change);  // "Hello, C#!"
```

**性能考虑**：
- **字符串不可变**：每次变更创建新字符串
- **频繁变更场景**：考虑使用 `StringBuilder` 或增量文本表示（如 PIE 表）

**Jolt 当前场景**：
- LSP 增量同步：变更频率低（用户按键）
- 单次变更文本小：字符串拼接性能可接受

### 3.3 版本号递增算法

**问题**：从当前版本生成下一个版本号

**算法**：
```csharp
// 整数版本号递增
public static DocumentVersion Increment(DocumentVersion current)
{
    if (int.TryParse(current.Value, out var version))
    {
        return DocumentVersion.Create(version + 1);
    }

    // 非整数版本号：追加哈希或时间戳
    return DocumentVersion.Create($"{current.Value}-{GenerateShortHash()}");
}
```

**LSP 场景**：
- LSP 规范建议版本号从 1 开始递增
- 整数版本号：`1`, `2`, `3`, ...
- 服务器端维护递增计数器

**Jolt 实现**：
```csharp
// LspSession 中的版本管理
private int _version = 0;

public DocumentVersion NextVersion()
{
    _version++;
    return DocumentVersion.Create(_version);
}
```

### 3.4 文本区间重叠检测算法

**问题**：判断两个 `TextSpan` 是否重叠

**算法**：
```csharp
public static bool Overlaps(TextSpan a, TextSpan b)
{
    // 区间不重叠的条件：
    // a 在 b 左侧：a.End <= b.Start
    // a 在 b 右侧：a.Start >= b.End
    // 重叠 = !不重叠
    return a.Start < b.End && b.Start < a.End;
}
```

**使用场景**：
- LSP 文本变更：合并重叠的编辑
- 诊断信息：避免同一位置重复报告

**示例**：
```csharp
var a = new TextSpan(5, 10);   // [5, 15)
var b = new TextSpan(10, 5);   // [10, 15)

Overlaps(a, b);  // true (共享位置 10-14)

var c = new TextSpan(0, 5);    // [0, 5)
var d = new TextSpan(5, 10);   // [5, 15)

Overlaps(c, d);  // false (c.End == d.Start，闭开区间不重叠)
```

## 4. 线程安全模型

### 4.1 DocumentVersion 线程安全

**类型**：`readonly record struct`（值类型）

**线程安全保证**：
- **不可变**：`Value` 属性只读，构造后无法修改
- **值语义**：每次赋值创建副本，无共享状态
- **天然线程安全**：可自由跨线程传递

**示例**：
```csharp
// 线程 A
var version = DocumentVersion.Create(1);

// 线程 B（安全读取）
Console.WriteLine(version.Value);  // 无竞争条件
```

### 4.2 TextSpan 线程安全

**类型**：`readonly record struct`（值类型）

**线程安全保证**：
- **不可变**：`Start`、`Length` 只读，`End` 为计算属性（纯函数）
- **值语义**：每次赋值创建副本
- **天然线程安全**

**示例**：
```csharp
// 线程 A
var span = new TextSpan(0, 10);

// 线程 B（安全读取）
Console.WriteLine(span.End);  // 无竞争条件
```

### 4.3 TextChange 线程安全

**类型**：`record`（引用类型）

**线程安全保证**：
- **不可变**：`Span`、`NewText` 只读
- **引用类型**：多个线程可访问同一实例
- **安全读取**：读取操作无竞争

**注意事项**：
- **字符串不可变**：`NewText` 为 `string`，天然线程安全
- **组合类型**：`TextChange` 包含 `TextSpan`（值类型）和 `string`（不可变），整体不可变

**示例**：
```csharp
// 线程 A
var change = new TextChange(new TextSpan(0, 5), "Hello");

// 线程 B（安全读取）
Console.WriteLine(change.NewText);  // 无竞争条件
```

### 4.4 并发修改场景

**场景**：多个线程尝试修改文档版本

**问题**：`DocumentVersion` 本身不可变，但版本递增需要协调

**解决方案**：
```csharp
public sealed class DocumentVersionCounter
{
    private int _counter = 0;
    private readonly object _lock = new object();

    public DocumentVersion NextVersion()
    {
        lock (_lock)
        {
            _counter++;
            return DocumentVersion.Create(_counter);
        }
    }
}
```

**或者使用无锁算法**：
```csharp
public sealed class DocumentVersionCounter
{
    private int _counter = 0;

    public DocumentVersion NextVersion()
    {
        return DocumentVersion.Create(Interlocked.Increment(ref _counter));
    }
}
```

## 5. 错误处理

### 5.1 DocumentVersion 验证错误

**错误类型**：`ArgumentOutOfRangeException`、`ArgumentException`

**场景 1：负整数版本**
```csharp
DocumentVersion.Create(-1);  // ArgumentOutOfRangeException
```

**场景 2：空白字符串版本**
```csharp
new DocumentVersion("");     // ArgumentException
new DocumentVersion("   ");  // ArgumentException
```

**处理策略**：
- **快速失败**：构造时立即验证
- **TryCreate 模式**：提供无异常的创建方式

### 5.2 TextSpan 边界错误

**错误类型**：`ArgumentOutOfRangeException`

**场景：区间越界**
```csharp
var text = "Hello";  // 长度 5
var span = new TextSpan(0, 10);  // End = 10，超出文本长度

// 使用时需要验证
if (span.End > text.Length)
    throw new ArgumentOutOfRangeException("Span extends beyond text length.");
```

**处理策略**：
- **延迟验证**：`TextSpan` 构造时不验证（性能）
- **使用时验证**：在应用变更时检查

### 5.3 TextChange 应用错误

**错误类型**：`ArgumentOutOfRangeException`

**场景：变更区间越界**
```csharp
var text = "Hello";
var change = new TextChange(new TextSpan(0, 10), "World");

ApplyChange(text, change);  // ArgumentOutOfRangeException
```

**处理策略**：
- **验证优先**：应用前检查区间合法性
- **友好错误**：明确指出 `Start`、`End` 和文本长度

## 6. 配置选项

### 6.1 版本号格式

**当前支持**：
- 整数版本号：`"1"`, `"2"`, `"3"`
- 字符串版本号：`"v1.0.0"`, `"abc123"`

**限制**：
- 非空白字符串
- 无格式验证（任何非空白字符串都接受）

**扩展方向**：
```csharp
public enum VersionFormat
{
    Integer,       // 仅整数：1, 2, 3
    Semantic,      // 语义化版本：1.0.0, 2.1.3
    Hash           // 哈希值：abc123def456
}

public static bool IsValidFormat(string value, VersionFormat format)
{
    return format switch
    {
        VersionFormat.Integer => int.TryParse(value, out var v) && v >= 0,
        VersionFormat.Semantic => Version.TryParse(value, out _),
        VersionFormat.Hash => value.All(char.IsLetterOrDigit) && value.Length >= 3,
        _ => false
    };
}
```

### 6.2 TextSpan 坐标系

**当前**：绝对偏移量（从文档开始的字节数/字符数）

**扩展**：支持行列坐标
```csharp
public readonly record struct TextPosition(int Line, int Column);

public readonly record struct LineSpan(TextPosition Start, TextPosition End)
{
    public static LineSpan FromOffsets(string text, int startOffset, int endOffset)
    {
        // 将偏移量转换为行列
        var start = TextPosition.FromOffset(text, startOffset);
        var end = TextPosition.FromOffset(text, endOffset);
        return new LineSpan(start, end);
    }
}
```

### 6.3 TextChange 合并策略

**当前**：不支持自动合并

**扩展**：合并连续的文本变更
```csharp
public static TextChange Merge(TextChange a, TextChange b)
{
    // 检查变更是否连续
    if (a.Span.End != b.Span.Start)
        throw new InvalidOperationException("Changes are not adjacent.");

    // 合并区间和新文本
    var mergedSpan = new TextSpan(a.Span.Start, a.Span.Length + b.Span.Length);
    var mergedNewText = a.NewText + b.NewText;

    return new TextChange(mergedSpan, mergedNewText);
}
```

## 7. 与其他子系统的交互

### 7.1 与契约层交互

**DocumentSnapshot vs DocumentVersion**：
```csharp
// 契约层（跨项目）
public sealed class DocumentSnapshot
{
    public string? Version { get; }  // 可选字符串
}

// 文档层（Jolt 内部）
public readonly record struct DocumentVersion
{
    public string Value { get; }  // 验证过的版本字符串
}
```

**转换关系**：
```csharp
// DocumentSnapshot → DocumentVersion
if (DocumentVersion.TryCreate(snapshot.Version, out var version))
{
    // 使用强类型版本
}

// DocumentVersion → DocumentSnapshot
var snapshot = new DocumentSnapshot(
    path,
    kind,
    text,
    version.Value);  // 转换回字符串
```

### 7.2 与 LSP 集成

**LSP 文档版本**：
```csharp
// LSP 规范：版本号从 1 开始递增
// LspSession 使用 DocumentVersion 封装

public sealed class LspDocument
{
    private int _lspVersion = 0;
    private string _text;

    public DocumentVersion Version => DocumentVersion.Create(_lspVersion);

    public void ApplyChange(TextChange change)
    {
        _text = ApplyChange(_text, change);
        _lspVersion++;
    }
}
```

**LSP 文本同步**：
```csharp
// LSP 客户端发送增量变更
public sealed class DidChangeTextDocumentParams
{
    public TextDocumentItem textDocument { get; }
    public TextDocumentContentChangeEvent[] contentChanges { get; }
}

public sealed class TextDocumentContentChangeEvent
{
    public Range range { get; }           // 可选：null 表示全文档替换
    public int rangeLength { get; }       // 可选：区间长度
    public string text { get; }           // 新文本
}

// 转换为 Jolt TextChange
public static TextChange ToTextChange(TextDocumentContentChangeEvent lspChange)
{
    var span = lspChange.range is null
        ? new TextSpan(0, currentText.Length)  // 全文档替换
        : LspRangeToTextSpan(lspChange.range);

    return new TextChange(span, lspChange.text);
}
```

### 7.3 与编译器集成

**文档版本在编译中的作用**：
```csharp
public sealed class JazorCompiler
{
    private readonly Dictionary<string, (DocumentVersion Version, CompilationResult Result)> _cache;

    public CompilationResult Compile(DocumentSnapshot document)
    {
        // 检查缓存
        if (_cache.TryGetValue(document.DocumentPath, out var cached))
        {
            if (cached.Version.ToString() == document.Version)
            {
                return cached.Result;  // 版本匹配，使用缓存
            }
        }

        // 编译并缓存
        var result = CompileInternal(document);
        var version = DocumentVersion.TryCreate(document.Version, out var v) ? v : throw new ArgumentException();
        _cache[document.DocumentPath] = (version, result);
        return result;
    }
}
```

**增量编译**：
```csharp
public sealed class IncrementalCompiler
{
    public CompilationResult ApplyChange(TextChange change, DocumentVersion oldVersion)
    {
        // 检查变更范围
        if (IsSyntaxChangeOnly(change))
        {
            // 仅重新分析受影响的节点
            return ReparsePartial(change.Span, oldVersion);
        }
        else
        {
            // 类型或语义变更，完整重新编译
            return FullRecompile();
        }
    }
}
```

### 7.4 与诊断系统集成

**TextSpan 在诊断中的应用**：
```csharp
public sealed class DiagnosticBuilder
{
    public DiagnosticRecord CreateError(
        string message,
        TextSpan span,
        string documentPath)
    {
        return new DiagnosticRecord(
            id: Guid.NewGuid().ToString(),
            severity: DiagnosticSeverityKind.Error,
            message: message,
            documentPath: documentPath,
            start: span.Start,
            length: span.Length);
    }
}
```

**SourceMap 生成**：
```csharp
public sealed class SourceMapBuilder
{
    public void AddMapping(
        TextSpan sourceSpan,
        TextSpan generatedSpan,
        string sourcePath,
        string generatedPath)
    {
        _maps.Add(new SourceMapDescriptor(
            sourcePath: sourcePath,
            generatedPath: generatedPath,
            sourceStart: sourceSpan.Start,
            sourceLength: sourceSpan.Length,
            generatedStart: generatedSpan.Start,
            generatedLength: generatedSpan.Length));
    }
}
```

## 8. 设计权衡

### 8.1 值类型 vs 引用类型

**选择**：
- `DocumentVersion`、`TextSpan`：值类型（`record struct`）
- `TextChange`：引用类型（`record`）

**选择原因**：

**DocumentVersion（值类型）**：
- **大小小**：单个字符串引用（8 字节）
- **比较语义**：值比较（版本号相等性）
- **无继承**：不需要多态

**TextSpan（值类型）**：
- **大小小**：两个整数（8 字节）
- **比较语义**：值比较（区间相等性）
- **频繁复制**：LSP 增量同步中大量使用

**TextChange（引用类型）**：
- **大小较大**：包含 `TextSpan`（8 字节）+ `string` 引用（8 字节）= 16 字段
- **包含引用类型**：`string NewText`（引用类型）
- **语义复杂**：变更语义需要相等性和哈希码（`record` 提供）

**权衡**：
- 值类型：无 GC 压力，但频繁复制可能开销大
- 引用类型：GC 压力，但传递效率高（仅复制引用）

### 8.2 record vs class/struct

**选择**：所有类型使用 `record`（`record struct` 或 `record class`）

**优点**：
- **简洁**：主构造函数自动生成属性
- **结构化比较**：基于值的相等性（`Equals`、`GetHashCode`）
- **with 表达式**：支持非破坏性修改（`record class`）

**示例**：
```csharp
// 自动生成属性和构造函数
public readonly record struct TextSpan(int Start, int Length);

// 自动生成 Equals、GetHashCode
var a = new TextSpan(0, 10);
var b = new TextSpan(0, 10);
Console.WriteLine(a == b);  // true（值比较）

// with 表达式（record class）
var change1 = new TextChange(new TextSpan(0, 5), "Hello");
var change2 = change1 with { NewText = "World" };  // 非破坏性修改
```

**权衡**：
- 不可变性：`record` 默认不可变（符合文档类型的需求）
- 性能：`record struct` 避免分配，`record class` 有 GC 开销

### 8.3 主构造函数 vs 传统构造函数

**选择**：主构造函数（C# 12）

**传统方式**：
```csharp
public readonly record struct TextSpan : IEquatable<TextSpan>
{
    public int Start { get; }
    public int Length { get; }

    public TextSpan(int start, int length)
    {
        Start = start;
        Length = length;
    }

    public int End => Start + Math.Max(Length, 0);
}
```

**主构造函数**：
```csharp
public readonly record struct TextSpan(int Start, int Length)
{
    public int End => Start + Math.Max(Length, 0);
}
```

**优点**：
- **简洁**：减少 50% 代码量
- **明确**：参数即属性，一目了然
- **类型安全**：编译器生成不可变属性

**权衡**：
- 验证逻辑需要在构造函数体中添加：
```csharp
public readonly record struct TextSpan(int Start, int Length)
{
    public TextSpan(int start, int length) : this()
    {
        if (start < 0)
            throw new ArgumentOutOfRangeException(nameof(start));
        if (length < 0)
            throw new ArgumentOutOfRangeException(nameof(length));

        Start = start;
        Length = length;
    }

    public int End => Start + Math.Max(Length, 0);
}
```

### 8.4 闭开区间 vs 闭闭区间

**选择**：闭开区间 `[Start, End)`

**方案 A**：闭开区间（当前实现）
```csharp
public int End => Start + Length;
// TextSpan(0, 5) 表示位置 0, 1, 2, 3, 4
```

**方案 B**：闭闭区间 `[Start, End]`
```csharp
// TextSpan(0, 4) 表示位置 0, 1, 2, 3, 4
```

**选择原因**：
- **一致性**：.NET、JavaScript、LSP 都使用闭开区间
- **长度直观**：`Length = End - Start`
- **空区间自然表示**：`TextSpan(5, 0)` = `[5, 5)`（空）

**权衡**：
- 闭闭区间在用户界面更直观（如文本编辑器的行列选择）
- 但底层实现通常使用闭开区间

### 8.5 绝对偏移量 vs 行列坐标

**选择**：绝对偏移量

**方案 A**：绝对偏移量（当前实现）
```csharp
public readonly record struct TextSpan(int Start, int Length)
```

**方案 B**：行列坐标
```csharp
public readonly record struct TextSpan(LinePosition Start, LinePosition Length)

public readonly record struct LinePosition(int Line, int Character)
```

**选择原因**：
- **简单**：单维度，无需行列转换
- **性能**：O(1) 定位，无需换行符查找
- **LSP 兼容**：LSP `Position` 使用行列，但 `Range` 内部可转换为偏移量

**权衡**：
- 行列坐标对用户更友好（错误消息显示）
- 但编译器内部使用偏移量更高效

**转换示例**：
```csharp
public static LinePosition ToLinePosition(string text, int offset)
{
    var line = 0;
    var character = 0;

    for (int i = 0; i < offset && i < text.Length; i++)
    {
        if (text[i] == '\n')
        {
            line++;
            character = 0;
        }
        else
        {
            character++;
        }
    }

    return new LinePosition(line, character);
}
```

### 8.6 延迟验证 vs 即时验证

**选择**：混合策略

**DocumentVersion**：即时验证（构造函数）
```csharp
public DocumentVersion(string value)
{
    ArgumentException.ThrowIfNullOrWhiteSpace(value);  // 立即验证
    Value = value;
}
```

**TextSpan**：延迟验证（使用时）
```csharp
public readonly record struct TextSpan(int Start, int Length)
{
    // 构造函数不验证
}

// 使用时验证
public static void Validate(TextSpan span, string text)
{
    if (span.Start < 0 || span.Start > text.Length)
        throw new ArgumentOutOfRangeException("Start out of range.");
    if (span.End > text.Length)
        throw new ArgumentOutOfRangeException("End out of range.");
}
```

**选择原因**：
- **DocumentVersion**：版本号无效是逻辑错误，应立即失败
- **TextSpan**：区间合法性与上下文相关（文档长度），延迟验证更灵活

**权衡**：
- 即时验证：快速失败，但可能误报（如尚未知道文档长度）
- 延迟验证：灵活，但错误可能传播到更深层

### 8.7 字符串版本 vs 整数版本

**选择**：字符串版本（`string Value`）

**方案 A**：字符串版本（当前实现）
```csharp
public readonly record struct DocumentVersion
{
    public string Value { get; }
}
```

**方案 B**：整数版本
```csharp
public readonly record struct DocumentVersion
{
    public int Value { get; }
}
```

**选择原因**：
- **灵活性**：支持整数、语义化版本、哈希值
- **LSP 兼容**：LSP 版本号为整数，但转换为字符串无损（`"1"`, `"2"`）
- **扩展性**：未来支持版本格式（`"v1.0.0"`, `"abc123"`）

**权衡**：
- 字符串比较：`"2"` < `"10"`（字典序），需要解析为整数比较
- 但 LSP 版本号通常为连续整数，字典序即数值序

**比较实现**：
```csharp
public static int Compare(DocumentVersion a, DocumentVersion b)
{
    // 尝试整数比较
    if (int.TryParse(a.Value, out var aInt) && int.TryParse(b.Value, out var bInt))
    {
        return aInt.CompareTo(bInt);
    }

    // 回退到字符串比较
    return string.Compare(a.Value, b.Value, StringComparison.Ordinal);
}
```

---

## 附录

### A. 完整类型清单

| 类型 | 文件 | 用途 |
|------|------|------|
| `DocumentVersion` | `DocumentVersion.cs` | 文档版本标识 |
| `TextSpan` | `TextSpan.cs` | 文本区间 |
| `TextChange` | `TextChange.cs` | 文本变更 |

### B. 使用示例

**文档版本管理**：
```csharp
// 创建版本
var v1 = DocumentVersion.Create(1);
var v2 = DocumentVersion.Create(2);

// 安全创建
if (DocumentVersion.TryCreate(versionString, out var version))
{
    Console.WriteLine($"Valid version: {version}");
}

// 转换为字符串
var versionString = version.Value;
```

**文本区间操作**：
```csharp
// 创建区间
var span = new TextSpan(0, 10);

// 计算结束位置
var end = span.End;  // 10

// 检查重叠
var a = new TextSpan(0, 10);
var b = new TextSpan(5, 10);
var overlaps = a.Start < b.End && b.Start < a.End;  // true
```

**文本变更应用**：
```csharp
// 创建变更
var change = new TextChange(new TextSpan(7, 5), "C#");

// 应用变更
var text = "Hello, world!";
var result = ApplyChange(text, change);  // "Hello, C#!"

// 链式变更
var change1 = new TextChange(new TextSpan(0, 5), "Hi");
var change2 = new TextChange(new TextSpan(3, 6), "Earth");
var result2 = ApplyChange(ApplyChange(text, change1), change2);  // "Hi, Earth!"
```

### C. 性能特征

| 类型 | 大小 | 可变 | 线程安全 | GC 压力 |
|------|------|------|----------|---------|
| `DocumentVersion` | 8 字节（引用） | 否 | 是 | 低（值类型） |
| `TextSpan` | 8 字节 | 否 | 是 | 低（值类型） |
| `TextChange` | 16 字节（引用） | 否 | 是 | 中（引用类型） |

**内存分配**：
```csharp
// 值类型：栈分配或内联
var span = new TextSpan(0, 10);  // 栈分配

// 引用类型：堆分配
var change = new TextChange(span, "Hello");  // 堆分配

// 字符串：不可变，共享
var a = new TextChange(span, "Hello");
var b = new TextChange(span, "Hello");  // 共享同一个 "Hello" 字符串
```

### D. 相关文档

- `Contracts.md` - RPC 契约类型（`DocumentSnapshot`）
- `RpcTransport.md` - RPC 传输层实现
- LSP 规范：Text Document Synchronization（版本号管理）

---

**文档维护者**：developerhan
**最后更新**：2026-04-21
**文档版本**：v1.0
