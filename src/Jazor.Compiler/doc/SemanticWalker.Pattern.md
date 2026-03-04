# SemanticWalker.cs.Pattern.cs 分析文档

## 1. 文件概述

**文件路径**: `core/SemanticWalker.cs.Pattern.cs`

**职责**: 处理所有 C# 模式匹配操作的转换，将 IPatternOperation 转换为 JavaScript 条件表达式。

**代码行数**: ~800+ 行

## 2. 模式类型支持

### 2.1 支持的模式类型

| 模式类型 | IOperation | 转换结果 |
|---------|-----------|---------|
| 常量模式 | `IConstantPatternOperation` | `value === constant` |
| 类型模式 | `ITypePatternOperation` | `typeof/instanceof` 检查 |
| 声明模式 | `IDeclarationPatternOperation` | 类型检查 + 变量声明 |
| 关系模式 | `IRelationalPatternOperation` | `value >/>=/</<=/===/!==` |
| 取反模式 | `INegatedPatternOperation` | `!(pattern)` |
| 二元模式 | `IBinaryPatternOperation` | `pattern &&/\|\| pattern` |
| 递归模式 | `IRecursivePatternOperation` | 属性/位置解构检查 |
| 列表模式 | `IListPatternOperation` | 数组长度 + 元素检查 |
| 切片模式 | `ISlicePatternOperation` | 数组切片 + 剩余元素检查 |
| 丢弃模式 | `IDiscardPatternOperation` | `true` |

### 2.2 模式上下文处理

模式可出现在以下上下文中：

```
IIsPatternOperation           // expr is pattern
IPatternCaseClauseOperation   // switch case pattern
ISwitchExpressionArmOperation // switch expression arm
```

## 3. 核心方法分析

### 3.1 VisitIsPattern

```csharp
public override Node? VisitIsPattern(IIsPatternOperation operation, WalkerArgument argument)
{
    var expr = Translate<Expression>(operation.Pattern, argument);
    return Optimizer.OptimizeLogical(expr);
}
```

**特点**：
- 直接转换 Pattern 部分
- 使用 Optimizer 优化冗余逻辑表达式

### 3.2 模式转换核心逻辑

**常量模式**：
```csharp
// C#: value is 42
// JS: value === 42
```

**类型模式**：
```csharp
// C#: obj is string
// JS: typeof obj === "string"

// C#: obj is DateTime
// JS: obj instanceof Date
```

**关系模式**：
```csharp
// C#: value is > 0
// JS: value > 0

// C#: value is >= 10 and <= 100
// JS: value >= 10 && value <= 100
```

**递归模式**：
```csharp
// C#: obj is Person("John", 18)
// JS: obj instanceof Person && obj.Name === "John" && obj.Age === 18
```

**列表模式**：
```csharp
// C#: list is [1, 2, ..]
// JS: Array.isArray(list) && list.length >= 2 && list[0] === 1 && list[1] === 2
```

## 4. 已知缺陷

### 4.1 高优先级缺陷

| 缺陷 | 影响 | 建议修复方案 |
|------|------|-------------|
| **依赖向上遍历查找输入表达式** | 可测试性差，需要完整操作树 | 通过 WalkerArgument.Context 传入 |
| **ExtractPatternReference 复杂度高** | 难以维护和测试 | 重构为独立的模式上下文服务 |

### 4.2 中优先级缺陷

| 缺陷 | 影响 | 建议修复方案 |
|------|------|-------------|
| **列表模式生成的代码冗长** | 性能和可读性问题 | 优化生成更简洁的检查链 |
| **切片模式边界情况处理不完整** | 某些边界情况可能失败 | 添加更多边界测试用例 |

### 4.3 低优先级缺陷

| 缺陷 | 影响 | 建议修复方案 |
|------|------|-------------|
| **模式变量声明位置分散** | 生成代码可读性差 | 统一变量声明位置 |

## 5. 设计权衡

### 5.1 当前设计

**优点**：
- 完整支持所有 C# 模式类型
- 生成的代码语义等价

**缺点**：
- 复杂度高，难以独立测试
- 向上遍历操作树增加开销

### 5.2 改进方向

| 改进 | 优先级 | 风险 |
|------|--------|------|
| 通过 WalkerArgument 传入上下文 | P1 | 中 |
| 提取模式上下文服务 | P2 | 低 |
| 优化生成代码 | P3 | 低 |

## 6. 测试覆盖

**当前状态**: ~150 个测试

**测试场景**：
- ✅ 常量模式
- ✅ 类型模式
- ✅ 关系模式
- ✅ 逻辑模式 (and/or/not)
- ✅ 递归模式
- ✅ 列表模式
- ✅ 切片模式
- ✅ 声明模式

## 7. 相关文档

- [SemanticWalker.md](./SemanticWalker.md)
- [WalkerArgument.md](./WalkerArgument.md)
- [Optimizer.md](./Optimizer.md)

---

**最后更新**: 2026-03-03
