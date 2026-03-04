# SemanticWalker.cs.String.cs 分析文档

## 1. 文件概述

**文件路径**: `core/SemanticWalker.cs.String.cs`

**职责**: 处理字符串插值，将 C# 插值字符串转换为 JavaScript 模板字符串。

**代码行数**: ~189 行

## 2. 核心转换

### 2.1 插值字符串转模板字符串

```csharp
// C# 示例
$"Hello {name}, you have {count} messages."

// JavaScript 结果
`Hello ${name}, you have ${count} messages.`
```

### 2.2 AST 节点映射

| C# 类型 | JavaScript AST | 用途 |
|---------|---------------|------|
| `IInterpolatedStringOperation` | `TemplateLiteral` | 完整插值字符串 |
| `IInterpolatedStringTextOperation` | `TemplateElement` | 静态文本部分 |
| `IInterpolationOperation` | `Expression` | 动态表达式部分 |
| `IInterpolatedStringAdditionOperation` | `TemplateLiteral` | 编译器生成的拼接 |

## 3. 方法详解

### 3.1 VisitInterpolatedString

**处理流程**：
1. 遍历 `operation.Parts`
2. 文本部分 → `TemplateElement`
3. 表达式部分 → 转换为 Expression
4. 确保 quasi 和 expression 数量关系正确
5. 构建 `TemplateLiteral`

**关键逻辑**：
```csharp
// 核心逻辑：确保表达式前有一个 quasi
if (quasis.Count == expressions.Count)
{
    quasis.Add(new TemplateElement(TemplateValue.From("", ""), tail: false));
}
```

**优化**：如果无表达式，返回更简单的 `StringLiteral`：
```csharp
if (expressions.Count == 0 && quasis.Count == 1)
{
    var cookedValue = quasis[0].Value.Cooked ?? "";
    return new StringLiteral(cookedValue, $"'{cookedValue}'");
}
```

### 3.2 VisitInterpolatedStringAddition

处理编译器生成的二叉树结构：

```csharp
// 编译器可能将 "a{b}c{d}e" 表示为：
// Addition(Addition(Addition("a", b), "c"), Addition(d, "e"))

void Collect(IOperation? node)
{
    switch (node)
    {
        case IInterpolatedStringAdditionOperation add:
            Collect(add.Left);   // 递归展开
            Collect(add.Right);
            break;
        case ILiteralOperation { ConstantValue: { HasValue: true, Value: string cookedValue } }:
            quasis.Add(new TemplateElement(...));
            break;
        default:
            Translate(exprs, node, argument);  // 动态表达式
            break;
    }
}
```

### 3.3 CookedToRaw 方法

处理转义字符：

```csharp
string CookedToRaw(string cooked)
{
    var sb = new StringBuilder(cooked.Length);
    foreach (var c in cooked)
    {
        switch (c)
        {
            case '`': sb.Append("\\`"); break;
            case '\\': sb.Append("\\\\"); break;
            case '$': sb.Append("\\$"); break;
            case '\r': sb.Append("\\r"); break;
            case '\n': sb.Append("\\n"); break;
            case '\t': sb.Append("\\t"); break;
            default: sb.Append(c); break;
        }
    }
    return sb.ToString();
}
```

## 4. 已知缺陷

### 4.1 中优先级缺陷

| 缺陷 | 影响 | 建议修复方案 |
|------|------|-------------|
| **格式化说明符未处理** | `{value:F2}` 格式丢失 | 解析格式说明符并生成对应代码 |
| **CultureInfo 未考虑** | 区域性格式化被忽略 | 添加区域性感知处理 |

### 4.2 低优先级缺陷

| 缺陷 | 影响 | 建议修复方案 |
|------|------|-------------|
| **复杂嵌套插值** | 可能有边界情况失败 | 添加更多测试用例 |

## 5. 转换示例

### 5.1 简单插值

```csharp
// C#
$"Hello {name}!"

// JavaScript
`Hello ${name}!`
```

### 5.2 表达式插值

```csharp
// C#
$"Value: {x + y}"

// JavaScript
`Value: ${x + y}`
```

### 5.3 连续表达式

```csharp
// C#
$"{a}{b}{c}"

// JavaScript
`${a}${b}${c}`
```

### 5.4 纯文本

```csharp
// C#
$"Hello World"

// JavaScript (优化为 StringLiteral)
'Hello World'
```

## 6. 测试覆盖

**当前状态**: ~30 个测试

**测试场景**：
- ✅ 简单插值
- ✅ 多表达式插值
- ✅ 表达式插值
- ✅ 连续表达式
- ✅ 纯文本优化

## 7. 相关文档

- [SemanticWalker.md](./SemanticWalker.md)

---

**最后更新**: 2026-03-03
