# SemanticWalker.cs.TryCatch.cs 分析文档

## 1. 文件概述

**文件路径**: `core/SemanticWalker.cs.TryCatch.cs`

**职责**: 处理 try-catch-finally 语句的转换。

**代码行数**: ~235 行

## 2. 核心转换

### 2.1 基本结构

```csharp
// C# 示例
try {
    RiskyOperation();
} catch (Exception ex) {
    HandleError(ex);
} finally {
    Cleanup();
}

// JavaScript 结果
try {
    riskyOperation();
} catch (ex) {
    handleError(ex);
} finally {
    cleanup();
}
```

### 2.2 多 catch 处理

JavaScript 只支持单个 catch，需要合并多个 catch 并使用 instanceof 分发：

```csharp
// C# 示例
try { ... }
catch (IOException ex) { HandleIO(ex); }
catch (ArgumentException ex) { HandleArg(ex); }

// JavaScript 结果
try { ... }
catch (_ex) {
    if (_ex instanceof IOException) {
        const ex = _ex;
        handleIO(ex);
    } else if (_ex instanceof ArgumentException) {
        const ex = _ex;
        handleArg(ex);
    }
}
```

### 2.3 when 条件处理

```csharp
// C# 示例
catch (Exception ex) when (ex.Message.Contains("error")) {
    HandleError(ex);
}

// JavaScript 结果
catch (ex) {
    if (!(ex.message.includes("error"))) throw ex;
    handleError(ex);
}
```

## 3. 方法详解

### 3.1 VisitTry

**处理流程**：
1. 转换 body 语句
2. 处理 catch 子句（单 catch vs 多 catch）
3. 处理 finally 块
4. 构建 `TryStatement`

**多 catch 处理关键代码**：
```csharp
if (operation.Catches.Length > 1)
{
    var tryParam = new Identifier(GetUniqueName(operation));
    foreach (var @catch in operation.Catches)
    {
        var (_, typeName) = GetMapperType(@catch.ExceptionType);
        var test = new NonLogicalBinaryExpression(Operator.InstanceOf, tryParam, right);
        alternates.Add(new IfStatement(test, body, null));
    }
    handler = new CatchClause(tryParam, catchBody);
}
```

### 3.2 ExtractCatchClauseParam

从异常声明中提取变量名：

```csharp
// 支持的声明类型
switch (operation.ExceptionDeclarationOrExpression)
{
    case ILocalReferenceOperation localRef:
        param = new Identifier(localRef.Local.Name);
        break;
    case IParameterReferenceOperation paramRef:
        param = new Identifier(paramRef.Parameter.Name);
        break;
    case IVariableDeclaratorOperation varDeclarator:
        param = new Identifier(varDeclarator.Symbol.Name);
        break;
}
```

### 3.3 ExtractCatchClauseBody

处理 when 条件和 catch body：

```csharp
// when 条件过滤器
if (operation.Filter is not null)
{
    var filterExpr = TranslateExpression(operation.Filter, argument);
    var notFilter = new NonUpdateUnaryExpression(Operator.LogicalNot, filterExpr);
    var throwStmt = new ThrowStatement(throwExpr);
    var filterCheck = new IfStatement(notFilter, throwStmt, null);
    bodyStatements.Add(filterCheck);
}
```

### 3.4 VisitThrow

处理 throw 语句，支持重新抛出：

```csharp
// C#: throw;
// JS: throw ex;  (使用外层 catch 的参数)

if (operation.Exception is null)
{
    // 重新抛出，需要从外层 catch 获取异常变量
    if (@try.Catches.Length == 1)
    {
        var param = ExtractCatchClauseParam(@try.Catches[0]);
        expr = param;
    }
}
```

## 4. 已知缺陷

### 4.1 中优先级缺陷

| 缺陷 | 影响 | 建议修复方案 |
|------|------|-------------|
| **异常类型映射不完整** | 某些类型可能 instanceof 失败 | 使用白名单映射异常类型 |
| **when 条件中的变量作用域** | 可能引用错误变量 | 确保 when 条件使用正确的异常参数 |

### 4.2 低优先级缺陷

| 缺陷 | 影响 | 建议修复方案 |
|------|------|-------------|
| **重新抛出参数查找复杂** | 代码可读性差 | 封装为专门的方法 |

## 5. AST 节点映射

| C# 结构 | JavaScript AST | 备注 |
|---------|---------------|------|
| try | `TryStatement` | 包含 block, handler, finalizer |
| catch | `CatchClause` | 包含 param 和 body |
| finally | `NestedBlockStatement` | 作为 finalizer |
| throw | `ThrowStatement` | 支持重新抛出 |

## 6. 测试覆盖

**当前状态**: ~40 个测试

**测试场景**：
- ✅ 简单 try-catch
- ✅ try-catch-finally
- ✅ 多 catch 子句
- ✅ when 条件
- ✅ throw 语句
- ✅ 重新抛出

## 7. 相关文档

- [SemanticWalker.md](./SemanticWalker.md)

---

**最后更新**: 2026-03-03
