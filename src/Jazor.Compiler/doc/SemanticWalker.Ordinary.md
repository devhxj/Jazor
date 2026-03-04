# SemanticWalker.cs.Ordinary.cs 分析文档

## 1. 文件概述

**文件路径**: `core/SemanticWalker.cs.Ordinary.cs`

**职责**: 处理二元/一元运算、条件表达式等普通运算操作。

**代码行数**: ~500+ 行 (估计，基于完整输出)

## 2. 支持的运算类型

### 2.1 二元运算

| C# 运算符 | JavaScript 运算符 | AST 类型 |
|----------|------------------|---------|
| `+` | `+` | `NonLogicalBinaryExpression` |
| `-` | `-` | `NonLogicalBinaryExpression` |
| `*` | `*` | `NonLogicalBinaryExpression` |
| `/` | `/` | `NonLogicalBinaryExpression` |
| `%` | `%` | `NonLogicalBinaryExpression` |
| `==` | `===` | `NonLogicalBinaryExpression` |
| `!=` | `!==` | `NonLogicalBinaryExpression` |
| `&&` | `&&` | `LogicalExpression` |
| `||` | `||` | `LogicalExpression` |
| `??` | `??` | `LogicalExpression` |

### 2.2 一元运算

| C# 运算符 | JavaScript 运算符 | AST 类型 |
|----------|------------------|---------|
| `-` | `-` | `NonUpdateUnaryExpression` |
| `!` | `!` | `NonUpdateUnaryExpression` |
| `++` | `++` | `UpdateExpression` |
| `--` | `--` | `UpdateExpression` |
| `~` | `~` | `NonUpdateUnaryExpression` |

### 2.3 赋值运算

| C# 运算符 | JavaScript 运算符 |
|----------|------------------|
| `=` | `=` |
| `+=` | `+=` |
| `-=` | `-=` |
| `*=` | `*=` |
| `/=` | `/=` |
| `%=` | `%=` |
| `&=` | `&=` |
| `|=` | `|=` |
| `^=` | `^=` |
| `<<=` | `<<=` |
| `>>=` | `>>=` |
| `>>>=` | `>>>=` |

## 3. 核心方法

### 3.1 VisitBinaryOperator

处理二元运算：

```csharp
public override Node? VisitBinaryOperator(IBinaryOperation operation, WalkerArgument argument)
{
    var left = Translate<Expression>(operation.LeftOperand, argument);
    var right = Translate<Expression>(operation.RightOperand, argument);

    // 根据运算符类型选择 AST 节点
    // 逻辑运算符 -> LogicalExpression
    // 其他运算符 -> NonLogicalBinaryExpression
}
```

### 3.2 VisitUnaryOperator

处理一元运算：

```csharp
public override Node? VisitUnaryOperator(IUnaryOperation operation, WalkerArgument argument)
{
    var operand = Translate<Expression>(operation.Operand, argument);

    // 更新运算符 -> UpdateExpression
    // 其他运算符 -> NonUpdateUnaryExpression
}
```

### 3.3 VisitConditionalExpression

处理条件表达式：

```csharp
// C# 示例
condition ? trueValue : falseValue

// JavaScript 结果
condition ? trueValue : falseValue

public override Node? VisitConditionalExpression(IConditionalOperation operation, WalkerArgument argument)
{
    var test = Translate<Expression>(operation.Condition, argument);
    var consequent = Translate<Expression>(operation.WhenTrue, argument);
    var alternate = Translate<Expression>(operation.WhenFalse, argument);
    return new ConditionalExpression(test, consequent, alternate);
}
```

### 3.4 VisitCoalesce

处理空合并运算符：

```csharp
// C# 示例
value ?? defaultValue

// JavaScript 结果
value ?? defaultValue
```

## 4. 特殊处理

### 4.1 字符串拼接优化

```csharp
// 多个字符串拼接可能优化为模板字符串
"a" + b + "c" → `a${b}c`
```

### 4.2 比较运算符映射

```csharp
// C# == 映射为 JavaScript ===
// C# != 映射为 JavaScript !==
// 保证语义等价性
```

### 4.3 is 运算符转换

```csharp
// C# 示例
obj is string

// JavaScript 结果
typeof obj === "string"  // 或 instanceof
```

## 5. 已知缺陷

### 5.1 中优先级缺陷

| 缺陷 | 影响 | 建议修复方案 |
|------|------|-------------|
| **运算符优先级处理** | 复杂表达式可能需要括号 | 添加括号包裹逻辑 |
| **溢出检查未处理** | `checked` 块被忽略 | 设计溢出处理策略 |

### 5.2 低优先级缺陷

| 缺陷 | 影响 | 建议修复方案 |
|------|------|-------------|
| **字符串拼接优化不完整** | 可能生成冗长的 + 表达式 | 改进拼接检测算法 |

## 6. AST 节点映射

| C# 运算 | JavaScript AST | 备注 |
|---------|---------------|------|
| 逻辑运算 | `LogicalExpression` | &&, ||, ?? |
| 比较运算 | `NonLogicalBinaryExpression` | ===, !==, <, >, 等 |
| 算术运算 | `NonLogicalBinaryExpression` | +, -, *, /, % |
| 更新运算 | `UpdateExpression` | ++, -- |
| 其他一元 | `NonUpdateUnaryExpression` | !, -, ~ |
| 条件表达式 | `ConditionalExpression` | ? : |

## 7. 测试覆盖

**当前状态**: ~33 个测试

**测试场景**：
- ✅ 算术运算
- ✅ 比较运算
- ✅ 逻辑运算
- ✅ 一元运算
- ✅ 条件表达式
- ✅ 赋值运算

## 8. 相关文档

- [SemanticWalker.md](./SemanticWalker.md)
- [Optimizer.md](./Optimizer.md)

---

**最后更新**: 2026-03-03
