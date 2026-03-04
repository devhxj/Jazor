# SemanticWalker.cs.Creation.cs 分析文档

## 1. 文件概述

**文件路径**: `core/SemanticWalker.cs.Creation.cs`

**职责**: 处理对象和数组创建操作的转换。

**代码行数**: ~422 行

## 2. 支持的创建类型

| C# 操作 | JavaScript 结果 |
|---------|----------------|
| `new MyClass()` | `new MyClass()` |
| `new List<int> { 1, 2 }` | `[1, 2]` |
| `new { Name = "John" }` | `{ name: "John" }` |
| `new int[5]` | `new Array(5)` |
| `new int[] { 1, 2, 3 }` | `[1, 2, 3]` |
| `new BigInteger(123)` | `BigInt(123)` |

## 3. 核心方法

### 3.1 BuildObjectCreation

**处理流程**：
1. 获取类型映射
2. 转换构造函数参数
3. 处理白名单映射
4. 处理初始化器

**关键代码**：
```csharp
// 特殊类型处理
var (mapper, typeName) = GetMapperType(operation.Type);
Expression expr = new NewExpression(callee, NodeList.From(arguments));

if (mapper == TypeMapper.BigInt)
    expr = new CallExpression(callee, NodeList.From(arguments), false);  // BigInt()
else if (mapper == TypeMapper.Array)
    expr = new ArrayExpression(NodeList.From<Expression?>(arguments));    // []
```

### 3.2 BuildObjectOrCollectionInitializer

处理带初始化器的对象创建，使用 IIFE 包装：

```csharp
// C# 示例
new MyClass { Prop1 = val1, Prop2 = val2 }

// JavaScript 结果
(() => {
    let _obj = new MyClass();
    _obj.prop1 = val1;
    _obj.prop2 = val2;
    return _obj;
})()
```

### 3.3 VisitAnonymousObjectCreation

```csharp
// C# 示例
new { Name = "John", Age = 25 }

// JavaScript 结果
{ name: "John", age: 25 }
```

### 3.4 VisitArrayCreation

```csharp
// 带初始化器
new int[] { 1, 2, 3 } → [1, 2, 3]

// 指定大小
new int[5] → new Array(5)

// 多维数组 - 不支持
new int[,] { {1,2}, {3,4} } → 抛出异常
```

### 3.5 VisitDelegateCreation

```csharp
// C# 示例
Action action = Method;

// JavaScript 结果
method.bind(this)  // 或直接引用
```

## 4. 初始化器处理

### 4.1 对象初始化器

```csharp
private List<Expression> BuildObjectCreationInitializer(
    Expression? obj,
    IObjectOrCollectionInitializerOperation initializers,
    WalkerArgument argument)
{
    // 处理 ISimpleAssignmentOperation
    // 处理 IMemberInitializerOperation
    // 处理 IInvocationOperation (集合添加方法)
}
```

### 4.2 嵌套对象初始化

```csharp
// C# 示例
new Person { Address = new Address { City = "NYC" } }

// 递归处理嵌套对象
if (simpleAssignmentOp.Value is IObjectCreationOperation subObjectCreationOp &&
    subObjectCreationOp.Initializer is not null)
{
    var sequenceExpr = BuildObjectCreation(left, subObjectCreationOp, argument);
}
```

## 5. 已知缺陷

### 5.1 高优先级缺陷

| 缺陷 | 影响 | 建议修复方案 |
|------|------|-------------|
| **多维数组不支持** | `new int[,]` 转换失败 | 设计替代方案或明确拒绝 |
| **集合初始化器方法调用不完整** | 复杂初始化可能失败 | 完善 Add 方法处理 |

### 5.2 中优先级缺陷

| 缺陷 | 影响 | 建议修复方案 |
|------|------|-------------|
| **IIFE 增加代码复杂度** | 生成代码可读性差 | 考虑优化为简单序列 |
| **泛型类型参数对象创建不完整** | `new T()` 可能失败 | 改进泛型处理 |

## 6. AST 节点映射

| C# 结构 | JavaScript AST | 备注 |
|---------|---------------|------|
| `new Class()` | `NewExpression` | 标准构造 |
| `new Class { }` | `CallExpression` (IIFE) | 带初始化器 |
| `new { }` | `ObjectExpression` | 匿名对象 |
| `new int[] { }` | `ArrayExpression` | 数组字面量 |
| `new int[n]` | `NewExpression` | 指定大小 |

## 7. 测试覆盖

**当前状态**: ~40 个测试

**测试场景**：
- ✅ 简单对象创建
- ✅ 带参数构造
- ✅ 对象初始化器
- ✅ 匿名对象
- ✅ 数组创建
- ✅ 集合初始化器

## 8. 相关文档

- [SemanticWalker.md](./SemanticWalker.md)
- [SemanticWalker.Reference.md](./SemanticWalker.Reference.md)

---

**最后更新**: 2026-03-03
