# SemanticWalker.cs.Switch.cs 分析文档

## 1. 文件概述

**文件路径**: `core/SemanticWalker.cs.Switch.cs`

**职责**: 处理 switch 语句和表达式的转换。

**代码行数**: ~170 行

## 2. 核心设计

### 2.1 双路径转换策略

文件检测 switch 是否包含模式匹配，采用不同策略：

```csharp
public override Node? VisitSwitch(ISwitchOperation operation, WalkerArgument argument)
{
    var hasPatternCase = operation.Cases
        .Any(x=>x.Clauses.Any(y=>y.CaseKind == CaseKind.Pattern));

    if (hasPatternCase)
        return VisitSwitchPatternMatching(operation, argument);  // IIFE + if-else

    return VisitSwitchTraditional(operation, argument);  // 传统 switch
}
```

### 2.2 传统 switch 转换

```csharp
// C# 示例
switch (value) {
    case 1:
        DoOne();
        break;
    case 2:
        DoTwo();
        break;
    default:
        DoDefault();
        break;
}

// JavaScript 结果
switch (value) {
    case 1:
        doOne();
        break;
    case 2:
        doTwo();
        break;
    default:
        doDefault();
}
```

### 2.3 模式匹配 switch 转换

当包含模式匹配时，转换为 IIFE + if-else 链：

```csharp
// C# 示例
switch (obj) {
    case string s when s.Length > 0:
        Console.WriteLine(s);
        break;
    case int i:
        Console.WriteLine(i);
        break;
}

// JavaScript 结果（概念）
((obj) => {
    if (typeof obj === "string" && obj.length > 0) {
        console.log(obj);
        return;
    }
    if (typeof obj === "number") {
        console.log(obj);
        return;
    }
})(obj);
```

## 3. 方法详解

### 3.1 VisitSwitchTraditional

**处理流程**：
1. 转换 discriminant 表达式
2. 遍历每个 case 子句
3. 转换 case 条件和 body
4. 构建 SwitchCase 节点

**关键代码**：
```csharp
// 处理 default case
if (clause.CaseKind == CaseKind.Default)
    tests.Add(null);  // null 表示 default case

// 处理多个条件共享同一个 body
for (int i = 0; i < tests.Count; i++)
{
    var testExpr = tests[i];
    var statements = i == 0 ? consequent : [];  // 只有第一个有语句
    cases.Add(new SwitchCase(testExpr, NodeList.From(statements)));
}
```

### 3.2 VisitDefaultCaseClause

返回 `null`，实际处理在 `VisitSwitch` 中完成。

### 3.3 VisitSwitchCase

将 switch case 转换为块语句，用于模式匹配场景。

### 3.4 VisitSingleValueCaseClause

转换单值 case 子句的条件表达式。

## 4. 已知缺陷

### 4.1 高优先级缺陷

| 缺陷 | 影响 | 建议修复方案 |
|------|------|-------------|
| **模式匹配 switch 未完全实现** | `VisitSwitchPatternMatching` 未在当前文件中 | 实现完整 IIFE 生成逻辑 |
| **fallthrough 处理不完整** | C# 不支持 fallthrough 但 JS 需要 break | 确保每个 case 添加 break |

### 4.2 中优先级缺陷

| 缺陷 | 影响 | 建议修复方案 |
|------|------|-------------|
| **when 条件处理不明确** | 可能未正确处理 guard 条件 | 在模式匹配分支中明确处理 |

## 5. AST 节点映射

| C# 结构 | JavaScript AST | 备注 |
|---------|---------------|------|
| switch 语句 | `SwitchStatement` | 传统模式 |
| switch 表达式 | IIFE + if-else | 模式匹配模式 |
| case 子句 | `SwitchCase` | 含 test 和 consequent |
| default | `SwitchCase(test=null)` | test 为 null |

## 6. 测试覆盖

**当前状态**: ~80 个测试

**测试场景**：
- ✅ 传统常量 switch
- ✅ default case
- ✅ 多条件 case
- ✅ 模式匹配 switch（部分）

## 7. 相关文档

- [SemanticWalker.Pattern.md](./SemanticWalker.Pattern.md)
- [SemanticWalker.md](./SemanticWalker.md)

---

**最后更新**: 2026-03-03
