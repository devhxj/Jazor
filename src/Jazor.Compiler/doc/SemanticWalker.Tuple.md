# SemanticWalker.cs.Tuple.cs 分析文档

## 1. 文件概述

**文件路径**: `core/SemanticWalker.cs.Tuple.cs`

**职责**: 处理元组创建、解构和比较操作。

**代码行数**: ~560 行

## 2. 核心功能

### 2.1 元组创建 (VisitTuple)

元组使用 JavaScript 对象模拟：

```csharp
// C# 示例
(1, "hello", true)
var tuple = (x, y);
(double Sum, int Count) t2 = (4.5, 3);

// JavaScript 结果
{ Item1: 1, Item2: "hello", Item3: true }
{ x: x, y: y }
{ Sum: 4.5, Count: 3 }
```

### 2.2 解构赋值 (VisitDeconstructionAssignment)

```csharp
// C# 示例
var (name, age) = GetPerson();
(int x, int y) = tuple;

// JavaScript 结果
let _temp = getPerson();
let name = _temp.name;
let age = _temp.age;
```

**支持的解构场景**：
- 元组解构
- 嵌套元组解构
- 自定义 Deconstruct 方法
- 带方法调用的解构

### 2.3 元组比较 (VisitTupleBinaryOperator)

```csharp
// C# 示例
(a, b) == (c, d)
(x, y) != (1, 2)

// JavaScript 结果
a === c && b === d
x !== 1 || y !== 2
```

## 3. 方法详解

### 3.1 VisitTuple

```csharp
public override Node? VisitTuple(ITupleOperation operation, WalkerArgument argument)
{
    var nodes = new List<Node>();
    var tupleType = (INamedTypeSymbol)operation.NaturalType!;
    for (var index = 0; index < operation.Elements.Length; index++)
    {
        var fieldName = tupleType.TupleElements[index].Name;  // 支持命名元组
        var element = operation.Elements[index];
        var key = new Identifier(fieldName);
        var value = Translate<Expression>(element, argument);
        nodes.Add(new ObjectProperty(PropertyKind.Init, key, value, ...));
    }
    return new ObjectExpression(NodeList.From(nodes));
}
```

### 3.2 Deconstruct (嵌套方法)

处理复杂的解构逻辑：

```csharp
void Deconstruct(IOperation target, ITypeSymbol valueType, object value, List<Expression> exprs)
{
    // 1. 元组类型解构
    if (valueType.IsTupleType && target is ITupleOperation or IDeclarationExpressionOperation)
    {
        // 递归处理每个元素
        for (var index = 0; index < tupleTarget.Elements.Length; index++)
        {
            // 声明新变量或赋值给现有变量
            // 处理丢弃操作
            // 处理嵌套元组
        }
    }
    // 2. 自定义 Deconstruct 方法
    else if (valueType.TypeKind == TypeKind.Class)
    {
        // 调用 Deconstruct 方法
        // 从返回数组中取值
    }
}
```

### 3.3 BuildTupleBinaryExpression

递归构建元组比较表达式：

```csharp
private Expression? BuildTupleBinaryExpression(
    (object Target, ITypeSymbol Type) left,
    (object Target, ITypeSymbol Type) right,
    bool isEq,
    WalkerArgument argument)
{
    // 处理方法调用结果缓存
    if (left.Target is IInvocationOperation leftInvocation)
    {
        leftExpr = new Identifier(GetUniqueName(leftInvocation));
        // 添加临时变量声明
    }

    // 递归处理嵌套元组
    if (leftField.Type.IsTupleType)
    {
        var subResult = BuildTupleBinaryExpression(subLeft, subRight, isEq, argument);
    }
}
```

## 4. 已知缺陷

### 4.1 高优先级缺陷

| 缺陷 | 影响 | 建议修复方案 |
|------|------|-------------|
| **解构赋值复杂度高** | 代码难以维护 | 重构为独立的解构服务 |
| **自定义 Deconstruct 处理不完整** | 某些场景可能失败 | 完善 Deconstruct 方法查找和调用 |

### 4.2 中优先级缺陷

| 缺陷 | 影响 | 建议修复方案 |
|------|------|-------------|
| **临时变量命名可能冲突** | 复杂嵌套时可能冲突 | 使用更安全的命名策略 |
| **元组比较对方法调用的缓存** | 重复计算 | 已实现缓存，但需验证 |

## 5. AST 节点映射

| C# 结构 | JavaScript AST | 备注 |
|---------|---------------|------|
| `(a, b)` | `ObjectExpression` | 属性为 Item1, Item2 或命名 |
| `var (a, b) = tuple` | 多个赋值语句 | 使用逗号表达式 |
| `tuple1 == tuple2` | `LogicalExpression` | and/or 连接的比较 |
| `_` (丢弃) | `null` 或忽略 | 在解构中跳过 |

## 6. 测试覆盖

**当前状态**: ~30 个测试

**测试场景**：
- ✅ 元组创建
- ✅ 命名元组
- ✅ 简单解构
- ✅ 嵌套解构
- ✅ 元组比较
- ✅ 丢弃模式

## 7. 相关文档

- [SemanticWalker.Pattern.md](./SemanticWalker.Pattern.md)
- [SemanticWalker.md](./SemanticWalker.md)

---

**最后更新**: 2026-03-03
