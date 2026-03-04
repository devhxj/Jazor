# SemanticWalker.cs.Reference.cs 分析文档

## 1. 文件概述

**文件路径**: `core/SemanticWalker.cs.Reference.cs`

**职责**: 处理字段、属性、方法引用和调用操作。

**代码行数**: ~584 行

## 2. 支持的引用类型

| C# 操作 | JavaScript 结果 |
|---------|----------------|
| 局部变量引用 | `localVar` |
| 参数引用 | `param` |
| 字段引用 | `obj.field` |
| 属性引用 | `obj.property` |
| 方法引用 | `obj.method` (可能需要 bind) |
| 方法调用 | `obj.method(args)` |
| 数组索引 | `array[index]` |
| 实例引用 (this) | `this` |

## 3. 核心方法

### 3.1 VisitLocalReference / VisitParameterReference

```csharp
public override Node? VisitLocalReference(ILocalReferenceOperation operation, WalkerArgument argument)
    => new Identifier(operation.Local.Name);

public override Node? VisitParameterReference(IParameterReferenceOperation operation, WalkerArgument argument)
    => new Identifier(operation.Parameter.Name);
```

### 3.2 VisitFieldReference

```csharp
public override Node? VisitFieldReference(IFieldReferenceOperation operation, WalkerArgument argument)
{
    // 静态常量字段特殊处理
    if (operation.Instance is null)
        return GetFieldName(operation, operation.Field);

    // 隐式接收者
    if (operation.Instance is IInstanceReferenceOperation instanceRef &&
        instanceRef.ReferenceKind == InstanceReferenceKind.ImplicitReceiver)
    {
        return GetFieldName(operation, operation.Field);
    }

    // 普通实例字段访问
    var expr = Translate<Expression>(operation.Instance, argument);
    return new MemberExpression(expr, new Identifier(fieldName), computed: false, optional: false);
}
```

### 3.3 VisitPropertyReference

```csharp
public override Node? VisitPropertyReference(IPropertyReferenceOperation operation, WalkerArgument argument)
{
    var instance = Translate<Expression>(operation.Instance, argument, null);

    // 白名单检查
    var mapperExpr = GetWhiteListExpression(operation.Property.GetMethod!, argument, [], instance, out var alias);
    if (mapperExpr is not null)
        return mapperExpr;

    // 静态属性
    if (operation.Property.IsStatic && operation.Property.ContainingType is not null)
    {
        var containing = BuildFullTypeName(operation.Property.ContainingType);
        return new MemberExpression(containing, property, computed: false, optional: false);
    }

    // 实例属性
    return new MemberExpression(instance, property, computed: false, optional: false);
}
```

### 3.4 VisitInvocation

处理方法调用，包括 ref/out 参数：

```csharp
public override Node? VisitInvocation(IInvocationOperation operation, WalkerArgument argument)
{
    var instance = Translate<Expression>(operation.Instance, argument, null);
    var refParas = new List<Expression>();
    var arguments = new List<Expression>();

    // 处理参数
    foreach (var arg in operation.Arguments)
    {
        var right = Translate<Expression>(arg.Value, argument);
        if (arg.Parameter?.RefKind is RefKind.Out or RefKind.Ref)
            refParas.Add(right);
        arguments.Add(right);
    }

    // 白名单检查
    var mapperExpr = GetWhiteListExpression(operation.TargetMethod, argument, arguments, instance, out var alias);
    if (mapperExpr is not null)
        return BuildInvExpr(hasReturn, mapperExpr, refParas, argument);

    // 构建调用
    var callExpr = new CallExpression(callee, NodeList.From(arguments), optional: false);
    return BuildInvExpr(hasReturn, callExpr, refParas, argument);
}
```

### 3.5 处理 ref/out 参数

```csharp
Expression BuildInvExpr(bool hasReturns, Expression expr, List<Expression> refs, WalkerArgument ctx)
{
    if (refs.Count > 0)
    {
        // 使用临时变量存储返回值
        // 返回值 + ref 参数值 组成数组
        // 生成逗号表达式
        var tempId = new Identifier(GetUniqueName(operation));
        var declarator = new VariableDeclarator(tempId, null);
        ctx.AddVarDeclarator(declarator, _recursionDepth);

        expressions.Add(new AssignmentExpression(Operator.Assignment, tempId, expr));
        // 从数组中提取 ref 参数值
        // ...
        return new SequenceExpression(NodeList.From(expressions));
    }
    return expr;
}
```

### 3.6 VisitArrayElementReference

处理数组索引和范围操作：

```csharp
public override Node? VisitArrayElementReference(IArrayElementReferenceOperation operation, WalkerArgument argument)
{
    if (operation.Indices.Length != 1)
        return HandleTransformationFailure<Node>(operation, "Multi-dimensional array access is not supported");

    // 从末尾索引: array[^1] → array[array.length - 1]
    // 范围操作: array[1..^4] → array.slice(1, array.length - 4)
    // 普通索引: array[0]
}
```

## 4. 特殊常量字段处理

GetFieldName 处理特殊常量：

```csharp
private Expression GetFieldName(IOperation includeOp, IFieldSymbol symbol)
{
    return (symbol.Name, symbol.ContainingType.SpecialType) switch
    {
        ("PositiveInfinity", SpecialType.System_Double) => new Identifier("Infinity"),
        ("NegativeInfinity", SpecialType.System_Double) => new Identifier("-Infinity"),
        ("NaN", SpecialType.System_Double) => new Identifier("NaN"),
        ("Epsilon", SpecialType.System_Double) => new MemberExpression(new Identifier("Number"), new Identifier("EPSILON"), ...),
        ("MaxValue", SpecialType.System_Double) => new MemberExpression(new Identifier("Number"), new Identifier("MAX_VALUE"), ...),
        // ...
    };
}
```

## 5. 已知缺陷

### 5.1 高优先级缺陷

| 缺陷 | 影响 | 建议修复方案 |
|------|------|-------------|
| **ref/out 参数处理复杂** | 生成代码冗长 | 优化 ref/out 处理逻辑 |
| **方法引用 bind 不完整** | this 绑定可能错误 | 完善 this 绑定检测 |

### 5.2 中优先级缺陷

| 缺陷 | 影响 | 建议修复方案 |
|------|------|-------------|
| **扩展方法处理** | 需要特殊处理 | 完善扩展方法支持 |
| **静态方法调用路径** | 可能生成冗长的路径 | 优化静态方法调用 |

## 6. AST 节点映射

| C# 操作 | JavaScript AST | 备注 |
|---------|---------------|------|
| 变量引用 | `Identifier` | 直接名称 |
| 字段/属性访问 | `MemberExpression` | computed=false |
| 方法调用 | `CallExpression` | callee + arguments |
| 数组索引 | `MemberExpression` | computed=true |
| 条件访问 | `MemberExpression` | optional=true |
| this | `ThisExpression` | - |

## 7. 测试覆盖

**当前状态**: ~50 个测试

**测试场景**：
- ✅ 局部变量引用
- ✅ 参数引用
- ✅ 字段引用
- ✅ 属性引用
- ✅ 方法调用
- ✅ 数组索引
- ✅ ref/out 参数

## 8. 相关文档

- [SemanticWalker.md](./SemanticWalker.md)
- [SemanticWalker.WhiteList.md](./SemanticWalker.WhiteList.md)

---

**最后更新**: 2026-03-03
