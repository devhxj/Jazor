# Jazor.Compiler

## 项目概述

Jazor.Compiler 是 Jazor 项目的核心编译器模块，负责将 C# 代码（IOperation）转换为 JavaScript AST（Acornima ESTree）。

### 核心职责

| 组件 | 职责 |
|------|------|
| `AstConverter` | 类级别转换：C# 类 → ES6 Module |
| `SemanticWalker` | 操作级别转换：IOperation → JavaScript AST |
| `WalkerArgument` | 转换上下文：变量声明、导入管理 |
| `TypeMapper` | 类型映射：C# 类型 → JavaScript 类型 |

### 项目状态

> 最后更新：2026-03-03 | 构建状态：✅ 成功 | 测试：533 个全部通过 (100%)

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

---

## 目录

1. [目录结构](#目录结构)
2. [核心转换架构](#核心转换架构)
3. [类型映射](#类型映射)
4. [Visit 方法规范](#visit-方法规范)
5. [AST 节点构造](#ast-节点构造)
6. [白名单机制](#白名单机制)
7. [不支持特性](#不支持特性)
8. [开发指南](#开发指南)

---

## 目录结构

```text
Jazor.Compiler/
├── GlobalUsings.cs              # 全局 using 声明
├── TypeMapper.cs                # 类型映射枚举
├── WalkerArgument.cs            # 转换上下文参数
├── AstConverter.cs              # 类级别转换器
├── AstTransformationException.cs # 异常类型定义
├── Optimizer.cs                 # AST 优化器
├── ESGenerator.cs               # 增量源生成器
├── ExtensionNode.cs             # 扩展节点
├── WhiteList.cs                 # 白名单核心
├── WhiteList.cs.Compile.cs      # 白名单编译时生成
├── WhiteList.cs.Generate.cs     # 白名单生成逻辑
├── core/                        # SemanticWalker 分文件
│   ├── SemanticWalker.cs              # 主文件 - 类型映射、Translate、ConvertFromSyntaxNode
│   ├── SemanticWalker.cs.Pattern.cs   # 模式匹配 - 常量、类型、属性、关系、递归、列表、切片模式
│   ├── SemanticWalker.cs.Reference.cs # 引用操作 - 字段、属性、方法引用、数组索引
│   ├── SemanticWalker.cs.Loop.cs      # 循环语句 - for、foreach、while、do-while
│   ├── SemanticWalker.cs.Switch.cs    # Switch - switch 语句和表达式
│   ├── SemanticWalker.cs.String.cs    # 字符串 - 插值字符串（模板字符串）
│   ├── SemanticWalker.cs.TryCatch.cs  # 异常处理 - try-catch-finally
│   ├── SemanticWalker.cs.Creation.cs  # 创建表达式 - 对象/数组创建
│   ├── SemanticWalker.cs.Tuple.cs     # 元组 - 元组创建和解构
│   ├── SemanticWalker.cs.Declaration.cs # 声明 - 变量声明、局部函数
│   ├── SemanticWalker.cs.Ordinary.cs  # 普通运算 - 二元/一元运算、条件表达式
│   ├── SemanticWalker.cs.Invalid.cs   # 无效操作 - IInvalidOperation 处理
│   ├── SemanticWalker.cs.NotSupport.cs # 不支持操作 - 抛出异常
│   ├── SemanticWalker.cs.WhiteList.cs # 白名单处理
│   └── SemanticWalker.cs.Generate.cs  # 生成逻辑
├── rule.md                      # 开发规则文档
├── task.md                      # 任务追踪文档
└── readme.md                    # 本文档
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
│  - 变量声明、运算、控制流             │
│  - 模式匹配、字符串插值等             │
└─────────────────────────────────────┘
    │
    ▼
JavaScript AST (Acornima ESTree)
```

### AstConverter 转换规则

| C# 成员 | JavaScript 结果 | 导出规则 |
|---------|----------------|---------|
| `public` 静态字段 | `const/let name = value` | `export` |
| `internal` 静态字段 | `const/let name = value` | `export` |
| `private` 静态字段 | `const/let _name = value` | 不导出 |
| `public` 静态属性 | 字段 + get/set 函数 | `export` |
| `public` 静态方法 | `function name(...) { ... }` | `export` |
| 嵌套 `public` 类 | `class ClassName { ... }` | `export` |
| 枚举 | `const EnumName = { ... }` | `export` |

### 核心转换原则

1. **语义等价性**：确保 C# 和 JavaScript 之间的语义完全等价，禁止任何形式的简化处理
2. **直接 AST 构造**：必须直接构造目标 AST 节点，禁止使用 Parser 进行解析
3. **空值安全处理**：构造 AST 节点时必须先检查输入值是否为 null
4. **编译时优化**：利用 C# 强类型系统的编译时信息直接生成最简 AST
5. **方法复用原则**：优先复用现有的 Visit 方法，避免为相似语义创建多个独立生成方法

---

## 类型映射

### TypeMapper 枚举

```csharp
public enum TypeMapper
{
    Undefined,   // 未定义
    Null,        // null
    Object,      // object
    String,      // string
    Boolean,     // boolean
    Number,      // number
    Date,        // Date
    BigInt,      // bigint
    Array,       // Array
    Map,         // Map
    Set,         // Set
    Class,       // class
    Unknown      // 未知类型
}
```

### C# 到 JavaScript 类型映射表

#### 基础类型

| C# 类型 | JavaScript 类型 | TypeMapper | 类型检查方式 |
|---------|----------------|------------|-------------|
| `object` | `object` | `Object` | `typeof x === "object"` |
| `bool` | `boolean` | `Boolean` | `typeof x === "boolean"` |
| `char` | `string` | `String` | `typeof x === "string"` |
| `string` | `string` | `String` | `typeof x === "string"` |

#### 数值类型

| C# 类型 | JavaScript 类型 | TypeMapper |
|---------|----------------|------------|
| `byte`, `sbyte`, `short`, `ushort`, `int`, `uint` | `number` | `Number` |
| `float`, `double`, `decimal` | `number` | `Number` |
| `long`, `ulong`, `Int128`, `UInt128` | `bigint` | `BigInt` |
| `BigInteger`, `TimeSpan` | `bigint` | `BigInt` |

#### 日期时间类型

| C# 类型 | JavaScript 类型 | TypeMapper |
|---------|----------------|------------|
| `DateTime` | `Date` | `Date` |
| `DateTimeOffset` | `Date` | `Date` |
| `DateOnly`, `TimeOnly` | `Date`/`number` | `Date`/`Number` |

#### 集合类型

| C# 类型 | JavaScript 类型 | TypeMapper |
|---------|----------------|------------|
| `Array<T>`, `T[]` | `Array` | `Array` |
| `List<T>`, `IList<T>`, `IEnumerable<T>` | `Array` | `Array` |
| `Dictionary<K,V>`, `IDictionary<K,V>` | `Map` | `Map` |
| `HashSet<T>`, `ISet<T>` | `Set` | `Set` |

---

## Visit 方法规范

### 方法签名约定

```csharp
/// <summary>
/// 处理 {操作类型} 操作
/// C# 示例：
/// {C# 代码示例}
/// 转换结果：{JavaScript 结果}
/// </summary>
/// <param name="operation">当前访问的operation</param>
/// <param name="argument">用于存放当前operation内部需要的全局变量定义</param>
/// <returns>Acornima的ESTree的Node</returns>
public override Node? VisitXxx(IXxxOperation operation, WalkerArgument argument)
```

### WalkerArgument 使用规则

| 方法 | 用途 |
|------|------|
| `AddVarDeclarator(declarator, depth)` | 添加变量声明，使用深度+名称作为键 |
| `FlushVarDeclarator()` | 刷新并获取累积的变量声明列表 |
| `MergeImportSpecifier(modulePath, specifier)` | 添加导入声明 |
| `With(type, target)` | 创建新实例，复用变量声明，更新上下文表达式 |

### 错误处理策略

使用 `HandleTransformationFailure<T>` 方法统一处理转换失败：

```csharp
return HandleTransformationFailure<Node>(operation, "Unsupported operation");
```

异常类型：

| 异常类型 | 用途 |
|---------|------|
| `OperationTransformationException` | IOperation 转换失败 |
| `SymbolTransformationException` | 符号转换失败 |
| `SyntaxNodeTransformationException` | 语法节点转换失败 |

---

## AST 节点构造

### 节点类型选择

| JavaScript 操作 | Acornima 节点类型 |
|----------------|-------------------|
| 逻辑操作（&&、||、??） | `LogicalExpression` |
| 比较操作（==、!=、<、>） | `NonLogicalBinaryExpression` |
| 一元操作（!、-、typeof） | `NonUpdateUnaryExpression` |
| 更新操作（++、--） | `UpdateExpression` |

### 字面量构造规则

```csharp
// NullLiteral 必须提供 raw 参数
var nullLit = new NullLiteral("null");

// BooleanLiteral：第一个参数为 bool 值，第二个参数为 string 原始值
var trueLit = new BooleanLiteral(true, "true");

// StringLiteral 必须提供原始值参数
var strLit = new StringLiteral("hello", "'hello'");

// 使用 CookedToRaw 方法处理转义字符
var escaped = CookedToRaw("hello\nworld");
```

### 唯一名称生成

`GetUniqueName` 方法用于生成稳定的唯一变量名称：

**使用场景**：
- 对象创建时的临时变量
- switch 表达式的输入变量
- try-catch 的异常参数
- 元组解构的临时变量

---

## 白名单机制

### 白名单查询流程

```
IMemberReferenceOperation
        │
        ▼
┌───────────────────────────────┐
│  GetWhiteListSymbol           │
│  获取成员符号                   │
└───────────────────────────────┘
        │
        ▼
┌───────────────────────────────┐
│  WhiteList.Members.TryGetValue│
│  查询白名单                     │
└───────────────────────────────┘
        │
        ├── Op.Alias → 替换方法名
        ├── Op.Inline → 内联代码
        ├── Op.Import → 导入模块调用
        └── Op.Discard → 不支持
```

### Op 类型处理

| Op 类型 | 处理方式 |
|---------|---------|
| `Alias` | 替换为 JavaScript 方法名 |
| `Inline` | 内联 JavaScript 表达式 |
| `Import` | 生成模块导入和调用 |
| `Allowed` | 直接使用 JavaScript 原生行为 |
| `Discard` | 不支持，抛出异常 |
| `Compile` | 编译器特殊处理 |

---

## 不支持特性

### 不支持的操作类型

| 操作类型 | 原因 |
|---------|------|
| **事件系统** | JavaScript 事件模型与 C# 多播事件模型根本不同 |
| `IRaiseEventOperation`, `IEventReferenceOperation`, `IEventAssignmentOperation` | 事件相关操作 |
| **动态类型** | C# 动态绑定语义与 JavaScript 静态分派模型不可通约 |
| `IDynamicObjectCreationOperation`, `IDynamicMemberReferenceOperation` 等 | 动态相关操作 |
| **LINQ** | LINQ 提供延迟执行、表达式树，JavaScript 没有对应构造 |
| `ITranslatedQueryOperation` | LINQ 查询表达式 |
| **类型和内存操作** | JavaScript 是安全语言，没有这些底层操作 |
| `ITypeOfOperation`, `ISizeOfOperation`, `IAddressOfOperation` | 类型/内存操作 |
| **资源管理** | JavaScript 没有内置的资源管理机制 |
| `IUsingOperation`, `IUsingDeclarationOperation` | using 语句/声明 |
| **线程同步** | JavaScript 是单线程语言 |
| `ILockOperation` | lock 语句 |

---

## 开发指南

### 添加新的转换支持

1. 在 `core/SemanticWalker.cs.{功能}.cs` 中添加对应的 Visit 方法
2. 遵循方法签名约定和注释规范
3. 使用 `Translate` 方法族进行类型安全转换
4. 添加对应的单元测试

### 测试规范

测试文件命名：`SemanticWalker{功能}Test.cs`

测试方法命名：`Visit_[PatternType]_[Scenario]`

```csharp
[TestMethod]
public void Visit_IsPattern_Constant() { }

[TestMethod]
public void Visit_SwitchExpression_Basic() { }
```

### 待办任务

| 优先级 | 任务 | 状态 |
|--------|------|------|
| P1 | WalkerArgument 上下文优化 | ⏳ 待评估 |
| P1 | 变量声明位置优化 | ⏳ 待评估 |
| P2 | 测试覆盖率统计 | ⏳ 待执行 |
| P2 | 注释统一为 XML 文档格式 | ⏳ 待执行 |
| P3 | 性能优化评估 | ⏳ 待评估 |

---

## 依赖关系

- **Microsoft.CodeAnalysis (Roslyn)** - C# 编译器平台，提供 IOperation API
- **Acornima** - JavaScript AST 库，提供 ESTree 节点类型
- **Jazor.Common** - 提供公共类型和工具

---

## 文档资源

- [rule.md](./rule.md) - 详细开发规则文档
- [task.md](./task.md) - 任务追踪文档
- [CLAUDE.md](../../CLAUDE.md) - Jazor 项目整体开发规则
- [Jazor.CLR/readme.md](../Jazor.CLR/readme.md) - CLR 模块文档

---

**文档维护者**：developerhan
**最后更新**：2026-03-03
**文档版本**：v1.0
