# SemanticWalker.cs.Invalid.cs 分析文档

## 1. 文件概述

**文件路径**: `core/SemanticWalker.cs.Invalid.cs`

**职责**: 处理 `IInvalidOperation`，提供从语法节点层面的备用转换路径。

**代码行数**: ~152 行

## 2. 设计思路

### 2.1 触发场景

`IInvalidOperation` 在以下情况出现：
- 编译器优化导致操作被折叠
- 上下文信息不完整
- 编译器内部中间状态

**注意**：在诊断器正常工作的情况下，理论上不应触发此方法。

### 2.2 回退策略

```
IOperation 层面转换失败
        │
        ▼
检测 IInvalidOperation
        │
        ▼
回退到 SyntaxNode 层面
        │
        ▼
基于语法节点类型转换
        │
        ▼
JavaScript AST
```

## 3. 方法详解

### 3.1 VisitInvalid

```csharp
public override Node? VisitInvalid(IInvalidOperation operation, WalkerArgument argument)
    => ConvertFromSyntaxNode(operation.Syntax);
```

### 3.2 ConvertFromSyntaxNode

基于 C# 语法节点类型进行模式匹配：

```csharp
private Node ConvertFromSyntaxNode(SyntaxNode node)
{
    var result = node switch
    {
        // 字面量
        LiteralExpressionSyntax lit => lit.Token.Value switch
        {
            null => Null,
            bool b => new BooleanLiteral(b, b.ToString().ToLower()),
            char c => new StringLiteral(c.ToString(), $"'{c}'"),
            string s => new StringLiteral(s, $"'{s}'"),
            int i => new NumericLiteral(i, i.ToString()),
            // ... 其他数值类型
        },

        // 标识符
        IdentifierNameSyntax id => new Identifier(id.Identifier.Text),

        // 成员访问
        MemberAccessExpressionSyntax ma => new MemberExpression(
            (Expression)ConvertFromSyntaxNode(ma.Expression),
            new Identifier(ma.Name.Identifier.Text),
            computed: false,
            optional: false),

        // 方法调用
        InvocationExpressionSyntax ie => new CallExpression(...),

        // 二元运算
        BinaryExpressionSyntax be => be.OperatorToken.Kind() switch
        {
            SyntaxKind.PlusToken => new NonLogicalBinaryExpression(Operator.Addition, ...),
            SyntaxKind.AmpersandAmpersandToken => new LogicalExpression(Operator.LogicalAnd, ...),
            // ...
        },

        // 更多语法节点类型...
    };

    return result ?? HandleTransformationFailure(node, $"Unsupported syntax node kind: {node.Kind()}.");
}
```

## 4. 支持的语法节点类型

| 语法节点 | 转换结果 |
|---------|---------|
| `LiteralExpressionSyntax` | 字面量 AST |
| `IdentifierNameSyntax` | `Identifier` |
| `ParenthesizedExpressionSyntax` | 解包内部表达式 |
| `InvocationExpressionSyntax` | `CallExpression` |
| `ObjectCreationExpressionSyntax` | `NewExpression` |
| `MemberAccessExpressionSyntax` | `MemberExpression` |
| `ConditionalAccessExpressionSyntax` | `ConditionalExpression` |
| `ElementAccessExpressionSyntax` | `MemberExpression` (computed) |
| `AssignmentExpressionSyntax` | `AssignmentExpression` |
| `ConditionalExpressionSyntax` | `ConditionalExpression` |
| `BinaryExpressionSyntax` | 对应二元表达式 |
| `PrefixUnaryExpressionSyntax` | 对应一元表达式 |
| `PostfixUnaryExpressionSyntax` | `UpdateExpression` |
| `CastExpressionSyntax` | 解包内部表达式 |
| `AwaitExpressionSyntax` | `AwaitExpression` |
| `TupleExpressionSyntax` | `SequenceExpression` |
| `DefaultExpressionSyntax` | `Null` |

## 5. 已知缺陷

### 5.1 高优先级缺陷

| 缺陷 | 影响 | 建议修复方案 |
|------|------|-------------|
| **语义信息丢失** | 无类型信息可能导致错误转换 | 尽量在 IOperation 层面处理 |
| **不支持所有语法节点** | 某些节点会抛出异常 | 扩展支持的语法节点类型 |

### 5.2 中优先级缺陷

| 缺陷 | 影响 | 建议修复方案 |
|------|------|-------------|
| **条件访问处理简化** | `a?.b?.c` 可能不完整 | 完善条件访问链处理 |
| **转换表达式未处理** | 强制类型转换被忽略 | 考虑运行时类型检查 |

## 6. 设计权衡

### 6.1 为什么需要此文件

**原因**：
- IOperation 是 Roslyn lowering 后的结果
- 某些表达式被优化或折叠
- 需要回退到原始语法保持语义

### 6.2 局限性

- 缺少类型信息
- 缺少语义分析结果
- 可能无法正确处理重载决策

## 7. 测试覆盖

**当前状态**: 有专门测试

**测试场景**：
- ✅ 字面量回退
- ✅ 成员访问回退
- ✅ 方法调用回退
- ✅ 运算符回退

## 8. 相关文档

- [SemanticWalker.md](./SemanticWalker.md)
- [rule.md](../rule.md)

---

**最后更新**: 2026-03-03
