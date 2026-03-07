# Jazor.Compiler 开发规则文档

本文档定义了 Jazor.Compiler 项目的开发规则，指导 C# 到 JavaScript AST 的转换实现。

> **Jazor.Compiler 的作用**：将 C# 代码（IOperation）转换为 JavaScript AST（Acornima ESTree）。
> **核心组件**：AstConverter（类级别转换）、SemanticWalker（操作级别转换）。

---

## 目录

1. [项目概述](#1-项目概述)
2. [核心转换架构](#2-核心转换架构)
3. [文件组织规范](#3-文件组织规范)
4. [类型映射规范](#4-类型映射规范)
5. [Visit 方法规范](#5-visit-方法规范)
6. [AST 节点构造规范](#6-ast-节点构造规范)
7. [白名单机制](#7-白名单机制)
8. [特性转换规范](#8-特性转换规范)
9. [Translate 方法族](#9-translate-方法族)
10. [不支持特性清单](#10-不支持特性清单)
11. [测试规范](#11-测试规范)

---

## 1. 项目概述

### 1.1 Compiler 在 Jazor 中的定位

Jazor.Compiler 是 Jazor 项目的核心编译器模块，负责：

1. **C# 到 JavaScript AST 转换**：将 Roslyn IOperation 转换为 Acornima ESTree 节点
2. **ES6 Module 生成**：将 C# 静态类转换为 ES6 模块
3. **白名单验证**：确保只使用允许的类型和成员

### 1.2 核心职责

| 组件 | 职责 |
|------|------|
| `AstConverter` | 类级别转换：C# 类 → ES6 Module |
| `SemanticWalker` | 操作级别转换：IOperation → JavaScript AST |
| `WalkerArgument` | 转换上下文：变量声明、导入管理 |
| `TypeMapper` | 类型映射：C# 类型 → JavaScript 类型 |

### 1.3 核心转换原则

1. **语义等价性**：确保 C# 和 JavaScript 之间的语义完全等价，禁止任何形式的简化处理
2. **直接 AST 构造**：必须直接构造目标 AST 节点，禁止使用 Parser 进行解析
3. **空值安全处理**：构造 AST 节点时必须先检查输入值是否为 null
4. **编译时优化**：利用 C# 强类型系统的编译时信息直接生成最简 AST
5. **方法复用原则**：优先复用现有的 Visit 方法，避免为相似语义创建多个独立生成方法

---

## 2. 核心转换架构

### 2.1 两层转换架构

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

### 2.2 AstConverter 转换规则

| C# 成员 | JavaScript 结果 | 导出规则 |
|---------|----------------|---------|
| `public` 静态字段 | `const/let name = value` | `export` |
| `internal` 静态字段 | `const/let name = value` | `export` |
| `private` 静态字段 | `const/let _name = value` | 不导出 |
| `public` 静态属性 | 字段 + get/set 函数 | `export` |
| `public` 静态方法 | `function name(...) { ... }` | `export` |
| 嵌套 `public` 类 | `class ClassName { ... }` | `export` |
| 枚举 | `const EnumName = { ... }` | `export` |

### 2.3 SemanticWalker 转换范围

支持将以下内容转换为 JavaScript AST：

- 方法体、静态字段初始值
- 属性 getter/setter
- 构造函数初始值设定项
- 局部函数、匿名函数/Lambda

---

## 3. 文件组织规范

### 3.1 SemanticWalker 分文件命名

SemanticWalker 采用分文件组织，位于 `core/` 目录下，每个文件负责特定类型的操作转换：

| 文件 | 职责 |
|------|------|
| `core/SemanticWalker.cs` | 主文件 - 类型映射、Translate、ConvertFromSyntaxNode |
| `core/SemanticWalker.cs.Pattern.cs` | 模式匹配 - IsPattern、常量、类型、属性、关系、递归、列表、切片模式 |
| `core/SemanticWalker.cs.Reference.cs` | 引用操作 - 字段、属性、方法引用、数组索引 |
| `core/SemanticWalker.cs.Loop.cs` | 循环语句 - for、foreach、while、do-while |
| `core/SemanticWalker.cs.Switch.cs` | Switch 语句 - switch 语句和表达式 |
| `core/SemanticWalker.cs.String.cs` | 字符串 - 插值字符串（模板字符串） |
| `core/SemanticWalker.cs.TryCatch.cs` | 异常处理 - try-catch-finally |
| `core/SemanticWalker.cs.Creation.cs` | 创建表达式 - 对象/数组创建 |
| `core/SemanticWalker.cs.Tuple.cs` | 元组 - 元组创建和解构 |
| `core/SemanticWalker.cs.Declaration.cs` | 声明 - 变量声明、局部函数 |
| `core/SemanticWalker.cs.Ordinary.cs` | 普通运算 - 二元/一元运算、条件表达式 |
| `core/SemanticWalker.cs.Invalid.cs` | 无效操作 - IInvalidOperation 处理（语法节点回退） |
| `core/SemanticWalker.cs.NotSupport.cs` | 不支持操作 - 抛出异常的不支持特性 |
| `core/SemanticWalker.cs.WhiteList.cs` | 白名单处理 - 白名单查询和应用 |
| `core/SemanticWalker.cs.Generate.cs` | 生成逻辑 - 白名单相关生成 |

### 3.2 分文件命名规则

- 格式：`core/SemanticWalker.cs.{功能}.cs`
- 位于 `core/` 子目录下
- 使用 partial class 分离不同功能
- 每个分文件专注单一职责

### 3.3 测试文件命名

- 格式：`SemanticWalker{功能}Test.cs`
- 与源文件一一对应
- 测试方法命名：`Visit_[PatternType]_[Scenario]`

---

## 4. 类型映射规范

### 4.1 TypeMapper 枚举定义

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

### 4.2 C# 到 JavaScript 类型映射表

#### 基础类型映射

| C# 类型 | JavaScript 类型 | TypeMapper | 类型检查方式 |
|---------|----------------|------------|-------------|
| `object` | `object` | `Object` | `typeof x === "object"` |
| `bool` | `boolean` | `Boolean` | `typeof x === "boolean"` |
| `char` | `string` | `String` | `typeof x === "string"` |
| `string` | `string` | `String` | `typeof x === "string"` |

#### 数值类型映射

| C# 类型 | JavaScript 类型 | TypeMapper |
|---------|----------------|------------|
| `byte`, `sbyte`, `short`, `ushort`, `int`, `uint` | `number` | `Number` |
| `float`, `double`, `decimal` | `number` | `Number` |
| `long`, `ulong`, `Int128`, `UInt128` | `bigint` | `BigInt` |
| `BigInteger`, `TimeSpan` | `bigint` | `BigInt` |

#### 日期时间类型映射

| C# 类型 | JavaScript 类型 | TypeMapper |
|---------|----------------|------------|
| `DateTime` | `Date` | `Date` |
| `DateTimeOffset` | `Date` | `Date` |
| `DateOnly`, `TimeOnly` | `Date`/`number` | `Date`/`Number` |

#### 集合类型映射

| C# 类型 | JavaScript 类型 | TypeMapper |
|---------|----------------|------------|
| `Array<T>`, `T[]` | `Array` | `Array` |
| `List<T>`, `IList<T>`, `IEnumerable<T>` | `Array` | `Array` |
| `Dictionary<K,V>`, `IDictionary<K,V>` | `Map` | `Map` |
| `HashSet<T>`, `ISet<T>` | `Set` | `Set` |

### 4.3 GetMapperType 方法规则

类型映射优先级：

1. **元组和匿名类型** → `Object`
2. **SpecialType 检查** → 基础类型映射
3. **TypeKind 检查** → Array、Enum
4. **显示名称检查** → 特殊类型（DateTimeOffset、BigInteger 等）
5. **白名单别名检查** → 自定义类型映射
6. **自定义 class/struct** → `Class`

---

## 5. Visit 方法规范

### 5.1 方法签名约定

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

### 5.2 WalkerArgument 使用规则

`WalkerArgument` 用于在遍历过程中传递上下文信息：

| 方法 | 用途 |
|------|------|
| `AddVarDeclarator(declarator, depth)` | 添加变量声明，使用深度+名称作为键 |
| `FlushVarDeclarator()` | 刷新并获取累积的变量声明列表 |
| `MergeImportSpecifier(modulePath, specifier)` | 添加导入声明 |
| `With(type, target)` | 创建新实例，复用变量声明，更新上下文表达式 |

### 5.3 错误处理策略

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

### 5.4 递归深度控制

```csharp
public override Node? Visit(IOperation? operation, WalkerArgument argument)
{
    if (operation is null)
        return null;

    _recursionDepth++;
    try
    {
        EnsureSufficientExecutionStack(_recursionDepth);
        return operation.Accept(this, argument);
    }
    finally
    {
        _recursionDepth--;
    }
}
```

---

## 6. AST 节点构造规范

### 6.1 节点类型选择

| JavaScript 操作 | Acornima 节点类型 |
|----------------|-------------------|
| 逻辑操作（&&、||、??） | `LogicalExpression` |
| 比较操作（==、!=、<、>） | `NonLogicalBinaryExpression` |
| 一元操作（!、-、typeof） | `NonUpdateUnaryExpression` |
| 更新操作（++、--） | `UpdateExpression` |

### 6.2 字面量构造规则

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

### 6.3 唯一名称生成

`GetUniqueName` 方法用于生成稳定的唯一变量名称：

```csharp
private string GetUniqueName(IOperation operation, string? prefix = null)
{
    var key = $"{syntaxTree.FilePath}${operation.Syntax.Kind()}${sourceSpan.Start}${sourceSpan.End}${operation.Kind}${prefix}";
    var name = Format.HashName(key);

    // 测试模式返回固定名称
    if (_test)
        return $"v${index}";

    return name;
}
```

**使用场景**：
- 对象创建时的临时变量
- switch 表达式的输入变量
- try-catch 的异常参数
- 元组解构的临时变量

---

## 7. 白名单机制

### 7.1 白名单接口

```csharp
public interface IWhiteList
{
    // 编译器生成白名单查询实现
}
```

### 7.2 白名单查询流程

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

### 7.3 Op 类型处理

| Op 类型 | 处理方式 |
|---------|---------|
| `Alias` | 替换为 JavaScript 方法名 |
| `Inline` | 内联 JavaScript 表达式 |
| `Import` | 生成模块导入和调用 |
| `Allowed` | 直接使用 JavaScript 原生行为 |
| `Discard` | 不支持，抛出异常 |
| `Compile` | 编译器特殊处理 |

---

## 8. 特性转换规范

### 8.1 IECMAScript 接口约定

`IECMAScript` 是一个标记接口，用于标识需要转换为 JavaScript Decorator 的 C# 特性：

```csharp
namespace ECMAScript;

public interface IECMAScript { }
```

**约定规则**：
- 接口名称固定为 `IECMAScript`（不包含命名空间）
- 只检查接口名称，不检查完整限定名
- 这是一个约定，而非强制继承关系

### 8.2 特性转换流程

```
IAttributeOperation
        │
        ▼
┌───────────────────────────────┐
│  检查是否实现 IECMAScript 接口  │
│  creationOp.Type?.AllInterfaces│
│    .Any(i => i.Name == "IECMAScript")│
└───────────────────────────────┘
        │
        ├── 未实现 → 返回 null（忽略特性）
        │
        ▼ 实现了 IECMAScript
┌───────────────────────────────┐
│  获取特性名称，移除 Attribute 后缀│
└───────────────────────────────┘
        │
        ▼
┌───────────────────────────────┐
│  通过 IObjectCreationOperation │
│  .Arguments 获取参数            │
│  使用 Visit 转换参数值          │
└───────────────────────────────┘
        │
        ▼
┌───────────────────────────────┐
│  构建 Decorator 表达式          │
│  @Decorator / @Decorator(args) │
└───────────────────────────────┘
```

### 8.3 参数转换规则

特性参数限制为编译时常量：

| 参数类型 | 转换方式 |
|---------|---------|
| 基本类型字面量 | 直接转换为 JavaScript 字面量 |
| 字符串 | `StringLiteral` |
| 枚举值 | 转换为枚举的数值或名称 |
| `typeof(Type)` | 根据类型映射转换 |
| null | `NullLiteral` |
| 数组 | `ArrayExpression` |

### 8.4 命名参数处理

通过语法节点的 `NameEquals` 判断是否为命名参数：

```csharp
if (syntaxArg.NameEquals is not null)
{
    // 命名参数：PropertyName = value
    var key = new Identifier(syntaxArg.NameEquals.Name.Identifier.Text);
    namedProps.Add(new ObjectProperty(...));
}
else
{
    // 位置参数
    positionalArgs.Add(valueExpr);
}
```

### 8.5 Decorator 输出格式

| 参数情况 | JavaScript 输出 |
|---------|----------------|
| 无参数 | `@Decorator` |
| 只有位置参数 | `@Decorator(arg1, arg2)` |
| 只有命名参数 | `@Decorator({ prop: value })` |
| 混合参数 | `@Decorator(arg1, { prop: value })` |

---

## 9. Translate 方法族

### 8.1 Translate 方法类型

| 方法签名 | 用途 | 失败行为 |
|---------|------|---------|
| `Translate<T>(IOperation, WalkerArgument)` | 强制转换为指定类型 | 抛出异常 |
| `Translate<T>(IOperation?, WalkerArgument, T?)` | 可选转换，允许默认值 | 返回默认值 |
| `Translate<T>(ICollection<T>, IOperation?, WalkerArgument)` | 集合转换，跳过失败项 | 记录错误但继续 |
| `TranslateExpression(IOperation, WalkerArgument)` | 专门转换为 Expression | 抛出异常 |

### 8.2 使用示例

```csharp
// 强制转换 - 必须成功
var expr = Translate<Expression>(operation.Value, argument);

// 可选转换 - 允许默认值
var expr = Translate<Expression>(operation.Value, argument, null);

// 集合转换 - 跳过失败项
Translate(elements, element, argument, null);
```

---

## 10. 不支持特性清单

### 10.1 不支持的操作类型

| 操作类型 | 原因 |
|---------|------|
| **事件系统** | JavaScript 事件模型与 C# 多播事件模型根本不同 |
| `IRaiseEventOperation` | 事件触发 |
| `IEventReferenceOperation` | 事件引用 |
| `IEventAssignmentOperation` | 事件赋值（+=/-=） |
| **动态类型** | C# 动态绑定语义与 JavaScript 静态分派模型不可通约 |
| `IDynamicObjectCreationOperation` | 动态对象创建 |
| `IDynamicMemberReferenceOperation` | 动态成员引用 |
| `IDynamicInvocationOperation` | 动态方法调用 |
| `IDynamicIndexerAccessOperation` | 动态索引器访问 |
| **LINQ** | LINQ 提供延迟执行、表达式树，JavaScript 没有对应构造 |
| `ITranslatedQueryOperation` | LINQ 查询表达式 |
| **编译器内部操作** | 不对应具体 C# 语法 |
| `IStopOperation`, `IEndOperation` | 编译器内部标记 |
| `IMethodBodyOperation`, `IConstructorBodyOperation` | 方法体操作 |
| `ICaughtExceptionOperation` | 捕获异常操作 |
| `IStaticLocalInitializationSemaphoreOperation` | 静态本地初始化信号量 |
| `IFlowAnonymousFunctionOperation`, `IFlowCaptureOperation`, `IFlowCaptureReferenceOperation` | 数据流分析操作 |
| **类型和内存操作** | JavaScript 是安全语言，没有这些底层操作 |
| `ITypeOfOperation` | typeof 操作符（C# 获取类型 vs JavaScript 获取值类型） |
| `ISizeOfOperation` | sizeof 操作符 |
| `IAddressOfOperation` | 取地址运算符 |
| **资源管理** | JavaScript 没有内置的资源管理机制 |
| `IUsingOperation`, `IUsingDeclarationOperation` | using 语句/声明 |
| **线程同步** | JavaScript 是单线程语言 |
| `ILockOperation` | lock 语句 |
| **VB.NET 特有功能** | JavaScript 没有对应语法 |
| `IForToLoopOperation` | For-To 循环 |
| `IReDimOperation`, `IReDimClauseOperation` | ReDim 操作 |
| `IRangeCaseClauseOperation`, `IRelationalCaseClauseOperation` | 范围/关系 case 子句 |
| **其他不支持的功能** | |
| `IInterpolatedStringHandlerCreationOperation`, `IInterpolatedStringAppendOperation` | 插值字符串处理器 |
| `IFunctionPointerInvocationOperation` | 函数指针调用 |
| `IUtf8StringOperation` | UTF-8 字符串 |
| `IInlineArrayAccessOperation` | 内联数组访问 |
| `IRangeOperation`（独立） | 独立的范围操作（在数组切片中支持） |

### 10.2 不支持时的错误信息格式

```csharp
public override Node? VisitXxx(IXxxOperation operation, WalkerArgument argument)
    => HandleTransformationFailure<Node>(operation, "{操作类型} operations are not supported in JavaScript conversion: {具体原因}");
```

---

## 11. 测试规范

### 11.1 测试文件组织

测试项目使用 MSTest 框架，按功能模块组织：

| 测试文件 | 测试范围 |
|----------|----------|
| `SemanticWalkerPatternTest.cs` | 模式匹配转换测试 |
| `SemanticWalkerLoopTest.cs` | 循环语句转换测试 |
| `SemanticWalkerSwitchTest.cs` | Switch 语句/表达式测试 |
| `SemanticWalkerStringTest.cs` | 字符串插值测试 |
| `SemanticWalkerTryCatchTest.cs` | 异常处理测试 |
| `SemanticWalkerDeclarationTest.cs` | 变量声明测试 |
| `SemanticWalkerOrdinaryTest.cs` | 普通运算测试 |
| `SemanticWalkerReferenceTest.cs` | 引用操作测试 |
| `SemanticWalkerCreationTest.cs` | 创建表达式测试 |
| `SemanticWalkerTupleTest.cs` | 元组测试 |
| `SemanticWalkerInvalidTest.cs` | 无效操作测试 |
| `AstConverterTests.cs` | AstConverter 测试 |
| `OptimizerTest.cs` | 优化器测试 |

### 11.2 测试方法命名约定

```csharp
// 格式：Visit_[PatternType]_[Scenario]
[TestMethod]
public void Visit_IsPattern_Constant() { }

[TestMethod]
public void Visit_SwitchExpression_Basic() { }
```

### 11.3 测试辅助方法

```csharp
// 编译代码并获取 Roslyn 代码块
protected IBlockOperation GetBlockOperation(string code)
{
    // 编译并返回 IBlockOperation
}
```

### 11.4 测试覆盖要求

- 每个 Visit 方法必须有对应的单元测试
- 测试场景包括正常转换和异常情况
- 验证 AST 结构正确性和语义等价性

---

## 附录

### A. 相关文件清单

| 文件 | 功能 |
|------|------|
| `AstConverter.cs` | 类级别转换器 |
| `SemanticWalker.cs` 及分文件 | 操作级别转换器 |
| `WalkerArgument.cs` | 转换上下文参数 |
| `TypeMapper.cs` | 类型映射枚举 |
| `WhiteList.cs` | 白名单核心 |
| `AstTransformationException.cs` | 异常类型定义 |
| `Optimizer.cs` | AST 优化器 |
| `ESGenerator.cs` | 增量源生成器 |

### B. 技术依赖

- Microsoft.CodeAnalysis（Roslyn）
- Acornima（JavaScript AST 库）
- .NET 10.0 运行时环境

### C. 相关文档

- [CLAUDE.md](../../CLAUDE.md) - 项目整体架构和转换思想
- [Jazor.CLR/rule.md](../Jazor.CLR/rule.md) - CLR 模块开发规则

---

**文档维护者**：developerhan
**最后更新**：2026-03-03
**文档版本**：v1.0
