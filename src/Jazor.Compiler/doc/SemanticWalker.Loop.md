# SemanticWalker.cs.Loop.cs 分析文档

## 1. 文件概述

**文件路径**: `core/SemanticWalker.cs.Loop.cs`

**职责**: 处理循环语句的转换，包括 for、foreach、while、do-while。

**代码行数**: ~145 行

## 2. 支持的循环类型

### 2.1 VisitForEachLoop

```csharp
// C# 示例
foreach (var item in collection) {
    Console.WriteLine(item);
}

// JavaScript 结果
for (let item of collection) {
    console.log(item);
}
```

**转换规则**：
- 直接映射为 `for...of` 语句
- 支持 `@await` 标记异步迭代

### 2.2 VisitForLoop

```csharp
// C# 示例
for (int i = 0; i < 10; i++) {
    Console.WriteLine(i);
}

// JavaScript 结果
for (let i = 0; i < 10; i++) {
    console.log(i);
}
```

**处理要点**：
- `operation.Before` → 初始化部分
- `operation.Condition` → 条件部分
- `operation.AtLoopBottom` → 迭代部分

**特殊处理**：`AtLoopBottom` 可能有多个操作，组合为逗号表达式：

```csharp
// Roslyn lowering 可能拆分迭代表达式
// 例如: i += x + y → 先算临时变量，再执行加法赋值
if (operation.AtLoopBottom.Length > 1)
{
    var expressions = new List<Expression>();
    foreach (var atLoopBottomOp in operation.AtLoopBottom)
    {
        var expr = TranslateExpression(atLoopBottomOp, argument);
        expressions.Add(expr);
    }
    // 组合为逗号表达式
    updateExpression = new SequenceExpression(NodeList.From(expressions));
}
```

### 2.3 VisitWhileLoop

```csharp
// C# while 示例
while (condition) { ... }

// JavaScript 结果
while (condition) { ... }

// C# do-while 示例
do { ... } while (condition);

// JavaScript 结果
do { ... } while (condition);
```

**区分方式**：通过 `operation.ConditionIsTop` 判断：
- `true` → while 循环
- `false` → do-while 循环

## 3. AST 节点映射

| C# 循环 | JavaScript AST | 备注 |
|---------|---------------|------|
| `foreach` | `ForOfStatement` | 直接映射 |
| `for` | `ForStatement` | 初始化/条件/迭代分离 |
| `while` | `WhileStatement` | 条件在顶部 |
| `do-while` | `DoWhileStatement` | 条件在底部 |

## 4. 已知缺陷

### 4.1 中优先级缺陷

| 缺陷 | 影响 | 建议修复方案 |
|------|------|-------------|
| **foreach 不支持异步迭代标记** | `await foreach` 可能不正确 | 检查 `@await` 标记并设置 `@await: true` |
| **for 循环初始化多变量声明可能不完整** | 某些复杂声明可能失败 | 完善多声明处理逻辑 |

### 4.2 低优先级缺陷

| 缺陷 | 影响 | 建议修复方案 |
|------|------|-------------|
| **变量声明收集不完整** | 循环变量可能不在正确位置 | 统一变量声明处理 |

## 5. 测试覆盖

**当前状态**: ~50 个测试

**测试场景**：
- ✅ 基本 for 循环
- ✅ foreach 循环
- ✅ while 循环
- ✅ do-while 循环
- ✅ 多迭代表达式
- ✅ 嵌套循环

## 6. 相关文档

- [SemanticWalker.md](./SemanticWalker.md)
- [SemanticWalker.Declaration.md](./SemanticWalker.Declaration.md)

---

**最后更新**: 2026-03-03
