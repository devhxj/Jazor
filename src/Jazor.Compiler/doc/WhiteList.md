# WhiteList 分析文档

## 1. 文件概述

**文件路径**: `WhiteList.cs` 及其分文件

**职责**: 白名单核心，存储允许使用的类型和成员映射信息。

## 2. 核心设计思路

### 2.1 数据结构

```csharp
internal static partial class WhiteList
{
    // 类型白名单：类型全名 → 处理方式
    public static readonly Dictionary<string, WhiteListValue> Types;

    // 成员白名单：成员签名 → 处理方式
    public static readonly Dictionary<string, WhiteListValue> Members;
}
```

### 2.2 WhiteListValue 结构

```csharp
internal sealed class WhiteListValue
{
    public Op Op { get; }           // 操作类型
    public string? Value { get; }   // 映射值（方法名、内联代码等）
    public string? Path { get; }    // 模块路径（用于 Import）
}
```

### 2.3 Op 类型处理

| Op 类型 | 处理方式 | Value 含义 |
|---------|---------|-----------|
| `Alias` | 替换方法名 | JavaScript 方法名 |
| `Inline` | 内联表达式 | JavaScript 表达式模板 |
| `Import` | 模块调用 | 函数哈希名 |
| `Allowed` | 原生支持 | 无 |
| `Discard` | 不支持 | 无 |
| `Compile` | 编译器处理 | 无 |

### 2.4 生成机制

白名单数据由 `WhiteList.cs.Generate.cs` 自动生成：

```
Jazor.CLR.dll / ECMAScript.dll
        │
        ▼
[Jazor] 特性扫描
        │
        ▼
WhiteListGenerator
        │
        ▼
WhiteList.cs.Generate.cs
```

## 3. 使用流程

### 3.1 类型映射查询

```csharp
// 在 GetMapperType 中
if (WhiteList.Types.TryGetValue(displayName, out var entry) && entry.Op == Op.Alias)
{
    // 使用 entry.Value 作为 JavaScript 类型名
}
```

### 3.2 成员映射查询

```csharp
// 在 GetWhiteListExpression 中
if (WhiteList.Members.TryGetValue(displayString, out var entry))
{
    if (entry.Op == Op.Alias)
        alias = entry.Value!;
    else if (entry.Op == Op.Inline)
        // 解析并替换占位符
    else if (entry.Op == Op.Import)
        // 生成模块调用
}
```

## 4. 已知缺陷

### 4.1 高优先级缺陷

| 缺陷 | 影响 | 建议修复方案 |
|------|------|-------------|
| **白名单数据不一致** | 编译器和分析器可能使用不同版本 | 确保生成过程同步 |
| **缺少验证机制** | 无法验证白名单完整性 | 添加白名单验证测试 |

### 4.2 中优先级缺陷

| 缺陷 | 影响 | 建议修复方案 |
|------|------|-------------|
| **查询使用字符串比较** | 性能一般 | 使用 Symbol 比较或缓存 |
| **缺少版本管理** | 无法追踪变更 | 添加版本信息和变更日志 |

### 4.3 低优先级缺陷

| 缺陷 | 影响 | 建议修复方案 |
|------|------|-------------|
| **错误消息不友好** | 调试困难 | 添加详细的错误消息 |
| **缺少文档** | 难以理解映射规则 | 自动生成文档 |

## 5. 需完善内容

### 5.1 功能完善

- [ ] 添加白名单验证机制
- [ ] 支持运行时动态查询优化
- [ ] 添加白名单变更追踪

### 5.2 代码质量

- [ ] 添加单元测试
- [ ] 优化查询性能
- [ ] 改善错误消息

### 5.3 工具支持

- [ ] 添加白名单可视化工具
- [ ] 自动生成白名单文档
- [ ] 支持白名单差异比较

## 6. 相关文档

- [SemanticWalker.md](./SemanticWalker.md)
- [Jazor.CLR/rule.md](../../Jazor.CLR/rule.md)

---

**最后更新**: 2026-03-03
