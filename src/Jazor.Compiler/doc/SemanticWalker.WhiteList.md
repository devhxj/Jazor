# SemanticWalker.cs.WhiteList.cs 分析文档

## 1. 文件概述

**文件路径**: `core/SemanticWalker.cs.WhiteList.cs`

**职责**: 白名单处理的核心接口和实现。

**代码行数**: ~130 行

## 2. 设计思路

### 2.1 白名单的作用

白名单用于：
1. 定义允许转换的 C# 类型和成员
2. 映射 C# 方法到 JavaScript 实现
3. 控制转换的行为方式

### 2.2 IWhiteList 接口

```csharp
partial interface IWhiteList { }
```

这是一个标记接口，`SemanticWalker` 实现它以获得白名单访问能力。

### 2.3 WhiteListValue 结构

```csharp
internal sealed class WhiteListValue
{
    public Op Op { get; }           // 操作类型
    public string? Value { get; }   // 映射值
    public string? Path { get; }    // 模块路径
}
```

## 3. Op 类型处理

| Op 类型 | 用途 | Value 含义 |
|---------|------|-----------|
| `Alias` | 方法别名映射 | JavaScript 方法名 |
| `Inline` | 内联代码 | JavaScript 表达式模板 |
| `Import` | 模块导入 | 函数哈希名 |
| `Allowed` | 原生支持 | 无 |
| `Discard` | 不支持 | 无 |
| `Compile` | 编译器特殊处理 | 无 |

## 4. 核心实现

### 4.1 GetWhiteListSymbol

从成员引用中获取用于白名单查询的符号：

```csharp
private static ISymbol GetWhiteListSymbol(IMemberReferenceOperation operation, bool isRead = true)
{
    if (operation is IPropertyReferenceOperation propertyReferenceOp)
    {
        // 属性引用：根据读写返回 get/set 方法
        if (isRead && propertyReferenceOp.Property.GetMethod is not null)
            return propertyReferenceOp.Property.GetMethod;
        else if (!isRead && propertyReferenceOp.Property.SetMethod is not null)
            return propertyReferenceOp.Property.SetMethod;
    }
    return operation.Member;
}
```

### 4.2 GetWhiteListExpression

根据白名单条目生成表达式：

```csharp
private static Expression? GetWhiteListExpression(
    ISymbol symbol,
    WalkerArgument context,
    List<Expression> arguments,
    out string? alias)
{
    var displayString = symbol.OriginalDefinition.ToDisplayString(Format.NameFormat);
    if (WhiteList.Members.TryGetValue(displayString, out var entry))
    {
        if (entry.Op == Op.Alias)
            alias = entry.Value!;
        else if (entry.Op == Op.Inline)
        {
            // 替换占位符 @{n}
            var raw = entry.Value!;
            for (var i = 0; i < arguments.Count; i++)
                raw = raw.Replace($"@#{{{i}}}", arguments[i].ToKnRECMAScript());
            return _parser.ParseExpression(raw, null, true);
        }
        else if (entry.Op == Op.Import)
        {
            // 生成模块导入调用
            var id = new Identifier(entry.Value!);
            context.MergeImportSpecifier(entry.Value!, new ImportSpecifier(id));
            return new CallExpression(id, NodeList.From(arguments), optional: false);
        }
    }
    return null;
}
```

### 4.3 使用 Parser 解析内联代码的设计决策

**为什么必须使用 Parser**：

对于 `Op.Inline` 类型的白名单条目，使用 Parser 解析内联代码是**必要的设计选择**，而非缺陷。原因如下：

1. **模板复杂性**：内联代码模板可能包含任意复杂的 JavaScript 表达式，如：
   - 条件表达式：`a ? b : c`
   - 逻辑运算：`a && b || c`
   - 函数调用：`Math.max(a, b)`
   - 嵌套结构：`(a + b) * (c - d)`

2. **维护成本**：为每种可能的 AST 结构编写直接构造代码会导致：
   - 代码量爆炸
   - 维护困难
   - 容易出错

3. **运行时安全**：Parser 提供了：
   - 语法验证
   - 错误定位
   - 标准 AST 生成

**性能考量**：内联代码模板在白名单中是预定义的、有限的，Parser 的开销在可接受范围内。

## 5. 已知缺陷

### 5.1 中优先级缺陷

| 缺陷 | 影响 | 建议修复方案 |
|------|------|-------------|
| **白名单查询使用字符串比较** | 性能一般 | 使用符号缓存或预编译映射 |
| **导入声明未实际生成** | `MergeImportSpecifier` 收集但未使用 | 在 AstConverter 中生成 ImportDeclaration |

## 6. 白名单查询流程

```
IMemberReferenceOperation
        │
        ▼
GetWhiteListSymbol
获取成员符号
        │
        ▼
WhiteList.Members.TryGetValue
查询白名单
        │
        ├── Op.Alias → 替换方法名
        ├── Op.Inline → 内联代码
        ├── Op.Import → 模块调用
        ├── Op.Allowed → 原生支持
        └── Op.Discard → 不支持
```

## 7. 与其他组件的关系

```
┌─────────────────────────────────────────────┐
│           Jazor.CLR (定义白名单)              │
└─────────────────────────────────────────────┘
                    │
                    ▼
┌─────────────────────────────────────────────┐
│     WhiteListGenerator (自动生成)            │
└─────────────────────────────────────────────┘
                    │
                    ▼
┌─────────────────────────────────────────────┐
│  WhiteList.cs.Generate.cs (生成的白名单数据) │
└─────────────────────────────────────────────┘
                    │
                    ▼
┌─────────────────────────────────────────────┐
│   SemanticWalker (使用白名单进行转换)         │
└─────────────────────────────────────────────┘
```

## 8. 测试覆盖

**当前状态**: 白名单处理测试分散在各功能测试中

**建议添加的测试**：
- 别名映射测试
- 内联代码测试
- 导入生成测试

## 9. 相关文档

- [WhiteList.md](./WhiteList.md)
- [SemanticWalker.md](./SemanticWalker.md)
- [SemanticWalker.Reference.md](./SemanticWalker.Reference.md)

---

**最后更新**: 2026-03-04
