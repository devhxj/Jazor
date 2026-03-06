# Jazor.Compiler

## 项目概述

Jazor.Compiler 是 Jazor 项目的核心编译器模块，负责将 C# 代码（IOperation）转换为 JavaScript AST（Acornima ESTree）。

### 核心职责

| 组件 | 职责 |
|------|------|
| `AstConverter` | 类级别转换：C# 类 → ES6 Module |
| `SemanticWalker` | 操作级别转换：IOperation → JavaScript AST |
| `Sense` | 语义上下文枚举：标识当前操作的语义场景 |
| `SenseArgument` | 语义上下文参数：传递上下文信息，替代向上遍历 |
| `WalkerArgument` | 依赖项收集：变量声明、导入管理 |
| `TypeMapper` | 类型映射：C# 类型 → JavaScript 类型 |

### 项目状态

> 最后更新：2026-03-06 | 构建状态：✅ 成功 | 测试：533 个全部通过 (100%)

| 功能模块 | 完成状态 | 测试覆盖 |
|----------|----------|----------|
| 模式匹配 (Pattern) | ✅ 完成 | ✅ 完整 |
| 循环语句 (Loop) | ✅ 完成 | ✅ 完整 |
| Switch 语句/表达式 | ✅ 完成 | ✅ 完整 |
| 字符串插值 (String) | ✅ 完成 | ✅ 完整 |
| 异常处理 (TryCatch) | ✅ 完成 | ✅ 完整 |
| 元组 (Tuple) | ✅ 完成 | ✅ 完整 |
| 创建表达式 (Creation) | ✅ 完成 | ✅ 完整 |
| 引用操作 (Reference) | ✅ 完成 | ✅ 完整 |
| 变量声明 (Declaration) | ✅ 完成 | ✅ 完整 |
| 普通运算 (Ordinary) | ✅ 完成 | ✅ 完整 |
| 无效操作处理 (Invalid) | ✅ 完成 | ✅ 完整 |
| AST 优化器 (Optimizer) | ✅ 完成 | ✅ 完整 |

### 架构改进

| 改进项 | 状态 |
|--------|------|
| Sense 语义上下文 | ✅ 完成 |
| 移除向上遍历 | ✅ 完成 |
| PatternInput 上下文传递 | ✅ 完成 |
| 线程安全修复 | ✅ 完成 |
| 变量声明提升到块顶 | ✅ 完成 |
| 函数边界作用域隔离 | ✅ 完成 |

---

## 目录

1. [目录结构](#目录结构)
2. [核心转换架构](#核心转换架构)
3. [语义上下文设计](#语义上下文设计)
4. [变量声明与作用域](#变量声明与作用域)
5. [类型映射](#类型映射)
6. [Visit 方法规范](#visit-方法规范)
7. [AST 节点构造](#ast-节点构造)
8. [白名单机制](#白名单机制)
9. [不支持特性](#不支持特性)
10. [开发指南](#开发指南)

---

## 目录结构

```text
Jazor.Compiler/
├── GlobalUsings.cs              # 全局 using 声明
├── TypeMapper.cs                # 类型映射枚举
├── Sense.cs                     # 语义上下文枚举
├── SenseArgument.cs             # 语义上下文参数
├── WalkerArgument.cs            # 依赖项收集器
├── AstConverter.cs              # 类级别转换器
├── AstTransformationException.cs # 异常类型定义
├── Optimizer.cs                 # AST 优化器
├── ESGenerator.cs               # 增量源生成器
├── core/                        # SemanticWalker 分文件
│   ├── SemanticWalker.cs              # 主文件
│   ├── SemanticWalker.cs.Pattern.cs   # 模式匹配
│   ├── SemanticWalker.cs.Reference.cs # 引用操作
│   ├── SemanticWalker.cs.Loop.cs      # 循环语句
│   ├── SemanticWalker.cs.Switch.cs    # Switch
│   ├── SemanticWalker.cs.String.cs    # 字符串
│   ├── SemanticWalker.cs.TryCatch.cs  # 异常处理
│   ├── SemanticWalker.cs.Creation.cs  # 创建表达式
│   ├── SemanticWalker.cs.Tuple.cs     # 元组
│   ├── SemanticWalker.cs.Declaration.cs # 声明
│   ├── SemanticWalker.cs.Ordinary.cs  # 普通运算
│   ├── SemanticWalker.cs.Invalid.cs   # 无效操作
│   └── SemanticWalker.cs.NotSupport.cs # 不支持操作
├── rule.md                      # 开发规则文档
├── task.md                      # 任务追踪文档
└── README.md                    # 本文档
```

---

## 核心转换架构

### 两层转换架构

```
C# 源代码
    │
    ▼
┌─────────────────────────────────────┐
│  Roslyn 编译器                       │
│  生成 IOperation 操作树              │
└─────────────────────────────────────┘
    │
    ▼
┌─────────────────────────────────────┐
│  AstConverter (类级别)              │
│  - 静态字段 → const/let 变量         │
│  - 静态属性 → get/set 方法           │
│  - 静态方法 → 函数声明               │
│  - 嵌套类 → class 声明               │
│  - 枚举 → const 对象                 │
└─────────────────────────────────────┘
    │
    ▼
┌─────────────────────────────────────┐
│  SemanticWalker (操作级别)          │
│  - IOperation → Acornima AST        │
│  - 通过 SenseArgument 传递上下文     │
└─────────────────────────────────────┘
    │
    ▼
JavaScript AST (Acornima ESTree)
```

### 核心转换原则

1. **语义等价性**：确保 C# 和 JavaScript 之间的语义完全等价
2. **直接 AST 构造**：直接构造目标 AST 节点，禁止使用 Parser 解析
3. **不向上遍历**：所有上下文信息通过 `SenseArgument` 显式传递
4. **不可变性**：`SenseArgument` 是值类型，通过 `with` 语法创建新实例
5. **编译时优化**：利用 C# 强类型系统直接生成最简 AST

---

## 语义上下文设计

### 设计原则

**核心原则：不向上遍历操作树**

如果需要上下文信息，必须通过 `SenseArgument` 显式传递，而非通过 `operation.Parent` 向上遍历。

**优点**：
- 显式依赖，调用链清晰
- 避免向上遍历的性能开销
- 单个 Visit 方法可独立测试
- 单一职责，每个方法只处理当前操作

### Sense 枚举

```csharp
public enum Sense
{
    // ===== 通用 =====
    Any,                    // 默认值

    // ===== 赋值上下文 =====
    LeftValue,              // 左值（赋值目标）
    RightValue,             // 右值（赋值源）
    PropertyAssignment,     // 属性赋值
    Deconstruction,         // 解构赋值

    // ===== Block 上下文 =====
    FunctionBody,           // 函数体
    StaticBlock,            // 静态初始化块
    NestedBlock,            // 嵌套块
    CatchHandler,           // Catch 处理器

    // ===== 模式匹配上下文 =====
    PatternInput,           // 模式匹配输入
    PatternCase,            // Switch case 模式
    SwitchExpressionArm,    // Switch expression arm
    PropertySubpattern,     // 属性子模式
    PatternExpression,      // 模式表达式

    // ===== 引用上下文 =====
    PropertyRead,           // 属性读取
    PropertyWrite,          // 属性写入
    ContainingTypeInstance, // this
    ImplicitReceiver,       // 隐式接收者

    // ===== 创建上下文 =====
    ObjectInitializer,      // 对象初始化器
    CollectionInitializer,  // 集合初始化器

    // ===== 异常上下文 =====
    ThrowNew,               // 抛出新异常
    Rethrow,                // 重新抛出

    // ===== 声明上下文 =====
    OutParameter,           // Out 参数声明
    VariableDeclaration,    // 变量声明
    Argument,               // 方法参数

    // ===== 丢弃上下文 =====
    DiscardAssignment,      // 丢弃赋值
    DefaultValue,           // 默认值
}
```

### SenseArgument 结构体

```csharp
public readonly record struct SenseArgument(
    Sense Sense = Sense.Any,
    WalkerArgument? Depend = null,
    Expression? PatternInput = null,
    string? CatchExceptionVar = null,
    string? SwitchExpressionVar = null)
{
    public static readonly SenseArgument Default = new();
    public WalkerArgument DependOrNew => Depend ?? new WalkerArgument();

    // Sense 变更
    public SenseArgument With(Sense sense) => this with { Sense = sense };

    // 作用域隔离（共享 _specifiers，新 _declarators）
    public SenseArgument WithNewScope() => /* ... */;

    // 模式匹配上下文
    public SenseArgument WithPatternInput(Expression? input) => this with { PatternInput = input };

    // 异常处理上下文
    public SenseArgument WithCatchVar(string? varName) => this with { CatchExceptionVar = varName };

    // 组合设置
    public SenseArgument With(Sense sense, Expression patternInput) => /* ... */;
}
```

### 使用示例

```csharp
// 模式匹配：传递被测试的表达式
public override Node? VisitIsPattern(IIsPatternOperation operation, SenseArgument argument)
{
    var value = Translate<Expression>(operation.Value, argument);
    var patternContext = argument.WithPatternInput(value);
    return Visit(operation.Pattern, patternContext);
}

// 异常处理：传递异常参数名
private List<Statement> ExtractCatchClauseBody(ICatchClauseOperation operation, SenseArgument argument, Identifier? exceptionParam)
{
    // 隔离 catch 体作用域
    var catchContext = (exceptionParam is not null
        ? argument.WithCatchVar(exceptionParam.Name)
        : argument).WithNewScope();
    // ...
}

// Block 输出类型判断
public override Node? VisitBlock(IBlockOperation operation, SenseArgument argument)
{
    if (argument.Sense == Sense.FunctionBody)
        return new FunctionBody(statements, strict: true);
    return new NestedBlockStatement(statements);
}
```

---

## 变量声明与作用域

### 设计原则

**核心原则：函数边界必须隔离作用域**

凡是对应 JS 函数边界或独立作用域的地方，必须隔离 `_declarators`，共享 `_specifiers`：

| 数据 | 传播方向 | 函数边界行为 |
|------|---------|------------|
| `_declarators`（变量声明） | 向上冒泡到最近的块 | **不能**穿越函数边界 |
| `_specifiers`（import 声明） | 向上冒泡到模块顶层 | **必须**穿越函数边界 |

### 作用域隔离位置

| 方法 | 处理方式 |
|------|---------|
| `VisitBlock` | `WithNewScope()` + flush 移到循环后（声明提升到块顶） |
| `VisitLocalFunction` | `WithNewScope()` + 内部自行 flush |
| `VisitAnonymousFunction` | `WithNewScope()` + 内部自行 flush |
| `VisitTry` | try/catch/finally 各自 `WithNewScope()` |
| `VisitSwitchPatternMatching` | case 体 `WithNewScope()` |

### 示例：函数边界隔离

```csharp
public override Node? VisitAnonymousFunction(IAnonymousFunctionOperation operation, SenseArgument argument)
{
    // 隔离 _declarators，共享 _specifiers
    var bodyCtx = argument.WithNewScope();
    var pendingStatements = new List<Statement>();

    foreach (var stmt in operation.Body.Operations)
    {
        var node = Visit(stmt, bodyCtx);
        // ...
    }

    // 函数体内部自行处理变量声明
    var bodyStatements = new List<Statement>();
    if (bodyCtx.HasVarDeclarator)
    {
        var declarators = bodyCtx.FlushVarDeclarator();
        bodyStatements.Add(new VariableDeclaration(VariableDeclarationKind.Let, declarators));
    }
    bodyStatements.AddRange(pendingStatements);
    // ...
}
```

---

## 类型映射

### TypeMapper 枚举

```csharp
public enum TypeMapper
{
    Undefined, Null, Object, String, Boolean, Number,
    Date, BigInt, Array, Map, Set, Class, Unknown
}
```

### C# 到 JavaScript 类型映射表

| C# 类型 | JavaScript 类型 | TypeMapper |
|---------|----------------|------------|
| `object` | `object` | `Object` |
| `bool` | `boolean` | `Boolean` |
| `char`, `string` | `string` | `String` |
| `byte`, `short`, `int`, `float`, `double` | `number` | `Number` |
| `long`, `BigInteger`, `TimeSpan` | `bigint` | `BigInt` |
| `DateTime`, `DateTimeOffset` | `Date` | `Date` |
| `Array<T>`, `List<T>` | `Array` | `Array` |
| `Dictionary<K,V>` | `Map` | `Map` |
| `HashSet<T>` | `Set` | `Set` |

---

## Visit 方法规范

### 方法签名约定

```csharp
/// <summary>
/// 处理 {操作类型} 操作
/// C# 示例：{C# 代码示例}
/// 转换结果：{JavaScript 结果}
/// </summary>
public override Node? VisitXxx(IXxxOperation operation, SenseArgument argument)
```

### 错误处理

```csharp
return HandleTransformationFailure<Node>(operation, "Unsupported operation");
```

---

## AST 节点构造

### 节点类型选择

| JavaScript 操作 | Acornima 节点类型 |
|----------------|-------------------|
| 逻辑操作（&&、||、??） | `LogicalExpression` |
| 比较操作（==、!=、<、>） | `NonLogicalBinaryExpression` |
| 一元操作（!、-、typeof） | `NonUpdateUnaryExpression` |
| 更新操作（++、--） | `UpdateExpression` |

### 唯一名称生成

`GetUniqueName` 方法基于语法节点位置生成稳定的唯一变量名：

- 使用 `_testCache` 缓存机制确保相同位置返回相同名称
- 正常模式：基于 SHA256 哈希生成唯一名称
- 测试模式：返回带索引的固定名称 `v$0`, `v$1`... 便于测试验证

使用场景：
- 对象创建时的临时变量
- switch 表达式的输入变量
- try-catch 的异常参数
- 元组解构的临时变量

---

## 白名单机制

| Op 类型 | 处理方式 |
|---------|---------|
| `Alias` | 替换为 JavaScript 方法名 |
| `Inline` | 内联 JavaScript 表达式 |
| `Import` | 生成模块导入和调用 |
| `Allowed` | 直接使用 JavaScript 原生行为 |
| `Discard` | 不支持，抛出异常 |

---

## 不支持特性

| 操作类型 | 原因 |
|---------|------|
| 事件系统 | JavaScript 事件模型与 C# 多播事件根本不同 |
| 动态类型 | C# 动态绑定与 JavaScript 静态分派不可通约 |
| LINQ | LINQ 提供延迟执行、表达式树，JS 无对应构造 |
| `typeof`/`sizeof`/`addressof` | JavaScript 是安全语言，无底层操作 |
| `using`/`lock` | JS 无内置资源管理和线程同步机制 |

---

## 开发指南

### 添加新的转换支持

1. 在 `core/SemanticWalker.cs.{功能}.cs` 中添加 Visit 方法
2. 遵循方法签名约定和注释规范
3. 通过 `SenseArgument` 传递上下文，不要使用 `operation.Parent`
4. 添加对应的单元测试

### 测试规范

测试文件命名：`SemanticWalker{功能}Test.cs`

测试方法命名：`Visit_[PatternType]_[Scenario]`

```csharp
[TestMethod]
public void Visit_IsPattern_Constant() { }
```

### 待办任务

| 优先级 | 任务 | 状态 |
|--------|------|------|
| P1 | WalkerArgument 上下文优化 | ✅ 完成 |
| P1 | 变量声明位置优化 | ✅ 完成 |
| P1 | 函数边界作用域隔离 | ✅ 完成 |
| P2 | 测试覆盖率统计 | ⏳ 待执行 |
| P2 | 注释统一为 XML 文档格式 | ⏳ 待执行 |

---

## 依赖关系

- **Microsoft.CodeAnalysis (Roslyn)** - C# 编译器平台
- **Acornima** - JavaScript AST 库

---

## 文档资源

- [task.md](./task.md) - 任务追踪文档
- [CLAUDE.md](../../CLAUDE.md) - Jazor 项目整体开发规则

---

**最后更新**：2026-03-06
**文档版本**：v1.3
