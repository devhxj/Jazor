# SemanticWalker.cs.Declaration.cs 分析文档

## 1. 文件概述

**文件路径**: `core/SemanticWalker.cs.Declaration.cs`

**职责**: 处理变量声明和初始化器的转换。

**代码行数**: ~140 行

## 2. 支持的声明类型

| C# 操作 | JavaScript 结果 |
|---------|----------------|
| `int x = 5;` | `let x = 5;` |
| `int x = 5, y = 10;` | `let x = 5, y = 10;` |
| `out var result` | 临时变量声明 |
| 数组初始化器 | `[1, 2, 3]` |
| 字段初始化器 | 直接返回初始化值 |

## 3. 方法详解

### 3.1 VisitVariableDeclarator

```csharp
public override Node? VisitVariableDeclarator(IVariableDeclaratorOperation operation, WalkerArgument argument)
{
    var identifier = new Identifier(operation.Symbol.Name);
    var init = Translate<Expression>(operation.Initializer, argument, null);
    return new VariableDeclarator(identifier, init);
}
```

### 3.2 VisitVariableDeclaration

```csharp
// C# 示例
int x = 5, y = 10;

// JavaScript 结果
let x = 5, y = 10;

public override Node? VisitVariableDeclaration(IVariableDeclarationOperation operation, WalkerArgument argument)
{
    var declarators = new List<VariableDeclarator>();
    foreach (var declarator in operation.Declarators)
        Translate(declarators, declarator, argument);

    return new VariableDeclaration(VariableDeclarationKind.Let, NodeList.From(declarators));
}
```

### 3.3 VisitVariableDeclarationGroup

处理多声明组：

```csharp
// C# 示例
int a = 1, b = 2, c;

// 注意：通常 Declarations 只有一个元素
public override Node? VisitVariableDeclarationGroup(IVariableDeclarationGroupOperation operation, WalkerArgument argument)
{
    var declarators = new List<VariableDeclarator>();
    foreach (var declaration in operation.Declarations)
        foreach (var declarator in declaration.Declarators)
            Translate(declarators, declarator, argument);

    return new VariableDeclaration(VariableDeclarationKind.Let, NodeList.From(declarators));
}
```

### 3.4 VisitDeclarationExpression

处理 `out var` 声明：

```csharp
// C# 示例
if (int.TryParse(input, out var result)) { ... }

// 处理逻辑
public override Node? VisitDeclarationExpression(IDeclarationExpressionOperation operation, WalkerArgument argument)
{
    var expr = Translate<Expression>(operation.Expression, argument);
    if (operation.Parent is IArgumentOperation)
    {
        var declarator = new VariableDeclarator(expr, null);
        argument.AddVarDeclarator(declarator, _recursionDepth);
    }
    return expr;
}
```

### 3.5 VisitArrayInitializer

```csharp
// C# 示例
new int[] { 1, 2, 3, 4, 5 }

// JavaScript 结果
[1, 2, 3, 4, 5]

public override Node? VisitArrayInitializer(IArrayInitializerOperation operation, WalkerArgument argument)
{
    var elements = new List<Expression?>();
    foreach (var element in operation.ElementValues)
    {
        Translate(elements, element, argument, null);
    }
    return new ArrayExpression(NodeList.From(elements));
}
```

## 4. 变量声明位置问题

### 4.1 当前行为

变量声明被分散插入到各个 statement 之间：

```csharp
// 生成结果
statement1;
let tempVar;
statement2;
```

### 4.2 问题

- 与 JavaScript 最佳实践不一致
- 可能存在 TDZ（暂时性死区）问题

### 4.3 建议

收集所有声明并集中在块开头。

## 5. 已知缺陷

### 5.1 中优先级缺陷

| 缺陷 | 影响 | 建议修复方案 |
|------|------|-------------|
| **声明位置分散** | 生成代码可读性差 | 收集声明并集中在块开头 |
| **const vs let 未区分** | 统一使用 let | 根据是否可变选择 const/let |

### 5.2 低优先级缺陷

| 缺陷 | 影响 | 建议修复方案 |
|------|------|-------------|
| **using 声明不支持** | 明确拒绝 | 在 NotSupport 中已处理 |

## 6. AST 节点映射

| C# 结构 | JavaScript AST | 备注 |
|---------|---------------|------|
| 变量声明 | `VariableDeclaration` | kind = let |
| 变量声明符 | `VariableDeclarator` | id + init |
| 数组初始化器 | `ArrayExpression` | 元素列表 |
| out var 声明 | `VariableDeclarator` | 添加到 argument |

## 7. 测试覆盖

**当前状态**: ~30 个测试

**测试场景**：
- ✅ 单变量声明
- ✅ 多变量声明
- ✅ 带初始化器声明
- ✅ out var 声明
- ✅ 数组初始化器

## 8. 相关文档

- [SemanticWalker.md](./SemanticWalker.md)
- [WalkerArgument.md](./WalkerArgument.md)

---

**最后更新**: 2026-03-03
