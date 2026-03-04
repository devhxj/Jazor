# Jazor.CLR

## 项目概述

Jazor.CLR 是 Jazor 编译器项目的 CLR 运行时支持层，使用 C# 编写（语法贴合 JavaScript）来实现 .NET 类型对应的 ES6 module，为 C# 到 JavaScript 的编译提供类型成员的 JavaScript 运行时实现。

### 核心职责

- **类型映射**：将 .NET 类型映射到 JavaScript 类型
- **成员实现**：提供类型成员（方法、属性）的 JavaScript 运行时实现
- **白名单机制**：通过 `[Jazor]` 特性与 Analyzer 和 Compiler 协同工作
- **模块导出**：标记可导出的模块供 JavaScript 使用

### 项目状态

> 最后更新：2026-03-03 | 构建状态：✅ 成功 (0 warnings, 0 errors)

| 指标 | 状态 | 完成度 |
|------|------|--------|
| 模块总数 | 39 | - |
| ✅ 完善模块 (9/10) | 27 | 69% |
| ⚠️ 部分完善 (7-8/10) | 12 | 31% |
| 🔴 需完善 (< 7/10) | 0 | 0% |

**任务完成状态**：
- P0（紧急）：5/5 ✅ 全部完成
- P1（高优先级）：5/5 ✅ 全部完成
- P2（中优先级）：5/5 ✅ 全部完成
- P3（低优先级）：4/4 ✅ 全部完成
- P4（不常用模块）：7/7 ✅ 全部完成

**总体完成度**：100%

---

## 目录

1. [目录结构](#目录结构)
2. [核心概念](#核心概念)
3. [Op 枚举详解](#op-枚举详解)
4. [模块分类](#模块分类)
5. [类型映射](#类型映射)
6. [out/ref 参数处理](#outref-参数处理)
7. [白名单机制](#白名单机制)
8. [开发指南](#开发指南)
9. [错误处理与调试](#错误处理与调试)
10. [快速参考](#快速参考)

## 目录结构

```text
Jazor.CLR/
├── GlobalUsings.cs          # 全局 using 声明
├── Jazor.CLR.csproj         # 项目配置
├── rule.md                  # 开发规则文档
├── task.md                  # 任务完成状态
├── readme.md                # 本文档
├── module/                  # 模块实现目录
│   ├── ArrayModule.cs
│   ├── BigIntegerModule.cs
│   ├── BooleanModule.cs
│   ├── CharModule.cs
│   ├── ConsoleModule.cs
│   ├── DateTimeModule.cs
│   ├── DictionaryModule.cs
│   ├── ListModule.cs
│   ├── StringModule.cs
│   └── ...（共39个模块）
└── doc/                     # 文档目录（记录成员签名）
    ├── BooleanModule.md
    ├── ConsoleModule.md
    └── ObjectModule.md
```

## 核心概念

### [Jazor] 特性

控制编译器对成员的处理方式，定义在 [JazorAttribute](../Jazor.Common/JazorAttribute.cs) 中。

**构造方法**：

```csharp
// 无参：Op = Op.Compile（编译器特殊处理）
[Jazor]

// 单字符串：Op = Op.Inline（内联代码）
[Jazor("内联代码模板")]

// 三参数：完整指定（Jazor.CLR 专用）
[Jazor(Op op, string member, string? value = null)]
```

**参数说明**：

- `op`：操作类型（见 [Op 枚举详解](#op-枚举详解)）
- `member`：完整的 C# 成员签名，用于白名单生成和哈希计算
- `value`：
  - `Op.Alias`：JavaScript 方法名（如 `toString`）
  - `Op.Inline`：内联代码模板（如 `(@#{0} === @#{1})`）

### [ECMAScriptModule] 特性

标记类为可导出的 ES6 模块：

```csharp
[ECMAScriptModule]
[Jazor(Op.Import, "bool", "System/BooleanModule.js")]
public static class BooleanModule
{
    // ...
}
```

### 方法签名命名规则

所有方法名采用哈希签名格式：`_` + 16位十六进制。

**重要**：哈希值由 `doc/` 目录下的文档预先定义，开发者必须使用文档中指定的哈希值，**切勿自行计算**。

```csharp
// 成员: static bool.Parse(string)
// 签名: _5dbf54319ebc8dfe（来自 doc/BooleanModule.md）
[Jazor(Op.Import, "static bool.Parse(string)")]
public static bool _5dbf54319ebc8dfe(string value)
{
    // ...
}
```

---

## Op 枚举详解

### Op 类型概览

| Op 类型 | extern? | 编译到 CLR module? | 白名单行为 | 说明 |
|---------|---------|-------------------|-----------|------|
| `Discard` | ✅ | ❌ | 记录但标记为不支持 | JavaScript 无对应概念 |
| `Allowed` | ✅ | ❌ | 记录为允许 | JavaScript 原生支持，无需处理 |
| `Alias` | ✅ | ❌ | 记录为允许 | JS 有类似方法但名称不同 |
| `Inline` | ✅ | ❌ | 记录为允许 | 简单表达式可直接内联 |
| `Import` | ❌ | ✅ | 记录为允许 | 需要完整 JavaScript 实现 |
| `Compile` | ✅ | ❌ | 记录为允许 | 编译器特殊处理 |

**核心原则**：
- `extern` = 方法不需要 C# 实现，只在白名单中标记
- **只有 `Op.Import` 方法会被编译到 CLR module**（因为有方法体）

### Op 类型选择决策

```
JS 有原生对应？
├── 是，名称相同 → Allowed
├── 是，名称不同 → Alias
└── 否 → JS 有概念？
    ├── 否 → Discard
    ├── 简单表达式 → Inline
    ├── 复杂逻辑 → Import
    └── 编译器处理 → Compile
```

### 详细示例

#### Op.Discard - 不支持

JavaScript 无对应概念：

```csharp
// JavaScript 无哈希码机制
[Jazor(Op.Discard, "override bool.GetHashCode()")]
public extern static Number _80b6c29cc0038969(bool instance);
```

#### Op.Allowed - 无操作

JavaScript 原生支持，默认行为正确：

```csharp
// 布尔默认构造（JS 布尔是原始类型）
[Jazor(Op.Allowed, "bool.Boolean()")]
public extern static bool _2bd9618624257446();
```

#### Op.Alias - 方法名替换

JS 有原生方法但名称不同：

```csharp
[Jazor(Op.Alias, "override bool.ToString()", "toString")]
public extern static string _d48c2d39317daf8f(bool instance);
// 编译时直接生成：instance.toString()
```

#### Op.Inline - 内联代码

用占位符模板生成 JavaScript 表达式：

```csharp
// Equals → 严格相等
[Jazor(Op.Inline, "override bool.Equals(object)", "(@#{0} === @#{1})")]
public extern static bool _hash(bool instance, object? obj);
```

**占位符规则**：

| 方法类型 | @#{0} | @#{1} | @#{2} |
|----------|-------|-------|-------|
| 实例方法 | 实例 | 参数1 | 参数2 |
| 静态方法 | 参数1 | 参数2 | 参数3 |
| 扩展方法 | 被扩展对象 | 参数1 | 参数2 |

#### Op.Import - 模块导入

需要完整 JavaScript 实现，**必须有方法体**：

```csharp
[Jazor(Op.Import, "static bool.Parse(string)")]
public static bool _5dbf54319ebc8dfe(string? value)
{
    var str = value?.Trim()?.ToLower();
    if (str == "true") return true;
    if (str == "false") return false;
    throw new Error($"FormatException: String '{value}' was not recognized as a valid Boolean.");
}
```

**注意**：
- 必须使用 JavaScript 原生实现，尽量调用映射后的方法
- 避免调用 C# 原生方法如 `int.Parse`，而是使用 `ParseInt` 和 `Number` 类型

## 模块分类

### 基础类型模块

| 模块 | .NET 类型 | JavaScript 类型 | 质量 |
| :--- | :--- | :--- | :--- |
| `VoidModule` | `void` | `undefined` | ✅ |
| `BooleanModule` | `bool` | `boolean` | ✅ 9/10 |
| `CharModule` | `char` | `string` | ⚠️ 8/10 |
| `ObjectModule` | `object` | `object` | ⚠️ 8/10 |

### 数值类型模块

| 模块 | .NET 类型 | JavaScript 类型 | 质量 |
| :--- | :--- | :--- | :--- |
| `SByteModule` | `sbyte` | `number` | ⚠️ 8/10 |
| `ByteModule` | `byte` | `number` | ⚠️ 8/10 |
| `Int16Module` | `short` | `number` | ✅ 9/10 |
| `UInt16Module` | `ushort` | `number` | ✅ 9/10 |
| `Int32Module` | `int` | `number` | ⚠️ 8/10 |
| `UInt32Module` | `uint` | `number` | ✅ 9/10 |
| `SingleModule` | `float` | `number` | ✅ 9/10 |
| `DoubleModule` | `double` | `number` | ✅ 9/10 |
| `DecimalModule` | `decimal` | `number` | ✅ 9/10 |
| `Int64Module` | `long` | `bigint` | ⚠️ 8/10 |
| `UInt64Module` | `ulong` | `bigint` | ✅ 9/10 |
| `BigIntegerModule` | `BigInteger` | `bigint` | ⚠️ 8/10 |

### 日期时间模块

| 模块 | .NET 类型 | JavaScript 类型 | 质量 |
| :--- | :--- | :--- | :--- |
| `DateTimeModule` | `DateTime` | `Date` | ✅ 9/10 |
| `DateTimeOffsetModule` | `DateTimeOffset` | `Date` | ✅ 9/10 |
| `DateOnlyModule` | `DateOnly` | `Date` | ✅ 9/10 |
| `TimeOnlyModule` | `TimeOnly` | `number` | ✅ 9/10 |
| `TimeSpanModule` | `TimeSpan` | `bigint` | ✅ 9/10 |

### 集合类型模块

| 模块 | .NET 类型 | JavaScript 类型 | 质量 |
| :--- | :--- | :--- | :--- |
| `ArrayModule` | `Array<T>` | `Array` | ✅ 9/10 |
| `ListModule` | `List<T>` | `Array` | ✅ 9/10 |
| `DictionaryModule` | `Dictionary<K,V>` | `Map` | ✅ 9/10 |
| `HashSetModule` | `HashSet<T>` | `Set` | ⚠️ 8/10 |
| `ReadOnlyCollectionModule` | `ReadOnlyCollection<T>` | `readonly Array` | ✅ 9/10 |
| `ReadOnlyDictionaryModule` | `ReadOnlyDictionary<K,V>` | `readonly Map` | ✅ 9/10 |
| `ReadOnlySetModule` | `ReadOnlySet<T>` | `readonly Set` | ✅ 9/10 |

### 其他模块

| 模块 | 说明 | 质量 |
| :--- | :--- | :--- |
| `StringModule` | 字符串操作 | ✅ 9/10 |
| `StringBuilderModule` | 字符串构建器 | ⚠️ 8/10 |
| `ConsoleModule` | 控制台输出 | ✅ 9/10 |
| `MathModule` | 数学运算 | ✅ 9/10 |
| `NullableModule` | 可空类型支持 | ⚠️ 8/10 |
| `ValueTupleModule` | 值元组 | ✅ 9/10 |
| `ExceptionModule` | 异常处理 | ✅ 9/10 |
| `ConditionalWeakTableModule` | 条件弱表 | ✅ 9/10 |
| `WeakReferenceModule` | 弱引用 | ✅ 9/10 |
| `CultureInfoModule` | 文化信息 | ✅ 9/10 |
| `GregorianCalendarModule` | 格里高利历 | ✅ 9/10 |

## 类型映射

### C# 到 JavaScript 的类型映射

| C# 类型 | JavaScript 类型 | 类型检查方式 |
| :--- | :--- | :--- |
| `void` | `undefined` | - |
| `bool` | `boolean` | `typeof x === "boolean"` |
| `char` | `string` | `typeof x === "string"` |
| `string` | `string` | `typeof x === "string"` |
| `byte/sbyte/short/ushort/int/uint/float/double/decimal` | `number` | `typeof x === "number"` |
| `long/ulong/BigInteger` | `bigint` | `typeof x === "bigint"` |
| `DateTime/DateTimeOffset/DateOnly` | `Date` | `x instanceof Date` |
| `TimeOnly` | `number` | `typeof x === "number"` |
| `TimeSpan` | `bigint` | `typeof x === "bigint"` |
| `Array<T>/List<T>` | `Array` | `Array.isArray(x)` |
| `Dictionary<K,V>` | `Map` | `x instanceof Map` |
| `HashSet<T>` | `Set` | `x instanceof Set` |
| `object` | `object` | `typeof x === "object"` |

### 参数类型映射（C# → Jazor.CLR）

在模块方法签名中，C# 类型需要映射为 Jazor.CLR 定义的 JavaScript 类型：

| C# 类型 | Jazor.CLR 类型 |
|---------|---------------|
| `bool` | `bool` |
| `int`, `uint`, `short`, `ushort`, `byte`, `sbyte`, `float`, `double`, `decimal` | `Number` |
| `long`, `ulong`, `Int128`, `UInt128`, `BigInteger` | `BigInt` |
| `char`, `string` | `string` |
| `DateTime`, `DateTimeOffset`, `DateOnly`, `TimeOnly` | `Date` |
| `List<T>`, `IList<T>`, `IEnumerable<T>`, `T[]` | `Array<T>` |
| `Dictionary<K,V>`, `IDictionary<K,V>` | `Map<TKey, TValue>` |
| `HashSet<T>`, `ISet<T>` | `Set<T>` |
| `object` | `object` |
| `void` | `void` |

### 特殊类型

| C# 类型 | JS 类型 | 说明 |
|---------|---------|------|
| `ReadOnlySpan<char>`, `Span<char>` | `string` | 无需特殊处理 |
| `System.Type` | `object` | JS 无类型系统 |
| `System.Guid` | `string` | UUID 字符串格式 |
| `System.Version` | `string` | 版本字符串 |

### 异常类型映射

| C# 异常 | JS 类型 |
|---------|---------|
| `Exception`, `SystemException`, `ArgumentException` | `Error` |
| `ArgumentNullException` | `TypeError` |
| `ArgumentOutOfRangeException`, `IndexOutOfRangeException` | `RangeError` |
| `FormatException` | `SyntaxError` |

## out/ref 参数处理

> **重要**：C# 中 `out` 和 `ref` 使用同种方式处理：返回数组模拟，调用处解构。

### 返回数组模式

```csharp
// C# 签名
static bool TryParse(string, out bool result)

// JS 返回值格式
[returnValue, outValue]
```

**数组格式规则**：
- 索引 0：方法返回值（void 方法为 `null`）
- 索引 1+：按声明顺序的 out/ref 参数值

### 定义处示例

```csharp
// 成员：static bool.TryParse(string, out bool)
// 签名：_dada4bbdacd7aa19
[Jazor(Op.Import, "static bool.TryParse(string, out bool)")]
public static Array<object?> _dada4bbdacd7aa19(string? value, bool result)
{
    var str = value?.Trim()?.ToLower();
    if (str == "true")
        return [true, true];   // [返回值, out参数]
    else if (str == "false")
        return [true, false];  // [返回值, out参数]

    return [false, false];     // 解析失败
}
```

**重要注意事项**：
1. 返回类型必须是 `Array<object?>`，不能使用泛型数组
2. out/ref 参数在方法签名中只声明类型和名称，不使用 `out`/`ref` 关键字修饰
3. 数组长度 = 1（返回值）+ out/ref 参数数量

### 调用处生成

```csharp
// C# 代码
if (bool.TryParse(input, out result)) { }

// JS 生成
let $0;
if (($0 = _hash(input, result), result = $0[1], $0[0])) { }
```

**生成规则**：
1. 编译器自动生成临时变量（如 `$0`）
2. 先调用方法，结果存入临时变量
3. 从数组中提取 out/ref 参数值，赋回原变量
4. 使用返回值（索引 0）作为表达式的最终值

### 多个 out/ref 参数

```csharp
// C# 签名：static bool TryParse(string, out int value1, out int value2)
// JS 返回：[returnValue, value1, value2]

// 调用处生成
let $0;
if (($0 = _hash(input, value1, value2), value1 = $0[1], value2 = $0[2], $0[0])) { }
```

## 白名单机制

### 工作流程

```
Jazor.CLR 项目
     │
     ├── [Jazor] 标记 ───→ 白名单生成器 ───→ Analyzer 白名单
     │                                              │
     │                                              ↓
     │                                    编译时类型/成员检查
     │
     └── 编译 ───────────→ CLR Module 库 ───→ Compiler 引用
```

### 与其他项目的协作

```text
用户代码编写
       ↓
┌──────────────────┐
│  Analyzer 阶段   │  检查使用的类型和成员是否在白名单中
└────────┬─────────┘
         ↓ (通过白名单验证)
┌──────────────────┐
│ Compiler 阶段    │  根据白名单中的名称反查 Jazor.CLR 实现
└────────┬─────────┘
         ↓
┌──────────────────┐
│  Jazor.CLR       │  提供 JavaScript 运行时实现
└────────┬─────────┘
         ↓ (生成对应的 ESTree node)
    JavaScript 代码
```

### 白名单同步

白名单由源生成器自动生成：

1. 扫描 Jazor.CLR 中的 `[Jazor]` 特性
2. 生成白名单代码到 `Jazor.Compiler/WhiteList.cs.Generate.cs`
3. 同步到 Jazor.Analyzer 项目

---

## 开发指南

### 添加新模块

1. 在 `module/` 目录创建新文件，如 `NewTypeModule.cs`
2. 添加 `[ECMAScriptModule]` 和 `[Jazor(Op.Import, ...)]` 特性
3. 查阅 `doc/` 目录获取成员签名和哈希值
4. 根据需要实现方法或使用 `extern`

```csharp
namespace Jazor.CLR;

[ECMAScriptModule]
[Jazor(Op.Import, "System.YourType", "System/YourTypeModule.js")]
public static class YourTypeModule
{
    // 从 doc/YourTypeModule.md 获取签名和哈希值

    // 使用 C# 实现（Op.Import 必须有方法体）
    [Jazor(Op.Import, "static System.YourType.Parse(string)")]
    public static YourType _xxx(string value)
    {
        // JavaScript 实现
    }

    // 使用 extern 声明（非 Import 类型）
    [Jazor(Op.Discard, "System.YourType.UnsupportedMethod()")]
    public extern static void _yyy();
}
```

### 成员命名约定

`[Jazor]` 特性的 Member 参数使用 .NET 完整成员名格式：

| 成员类型 | 格式 |
|---------|------|
| 静态方法 | `static TypeName.MethodName(params)` |
| 实例方法 | `TypeName.MethodName(params)` |
| 静态属性 | `static TypeName.PropertyName.get` / `.set` |
| 实例属性 | `TypeName.PropertyName.get` / `.set` |
| 重写方法 | `override TypeName.MethodName(params)` |
| 运算符 | `static TypeName.operator +(Type, Type)` |
| 构造函数 | `TypeName.TypeName(params)` |
| 字段 | `static readonly TypeName.FieldName` |

### 模块路径规范

| C# 类型 | 模块声明 | 说明 |
|---------|----------|------|
| `bool` | `[Jazor(Op.Import, "bool", "System/BooleanModule.js")]` | 基本类型别名 |
| `Int32` | `[Jazor(Op.Import, "int", "System/Int32Module.js")]` | 基本类型关键字 |
| `DateTime` | `[Jazor(Op.Import, "System.DateTime", "System/DateTimeModule.js")]` | 完整类型名 |
| `Console` | `[Jazor(Op.Alias, "System.Console", "console")]` | 替换为全局对象 |

**路径命名规则**：
- 命名空间映射：`System.Collections.Generic` → `System/Collections/Generic/`
- 文件命名：`{类型名}Module.js`
- 泛型类型：`` `n `` 表示参数数量，如 `List`1Module.js`
- 嵌套类型：使用 `+` 连接，如 `Outer+InnerModule.js`

### 代码风格

- 代码使用 C# 编写，但语法贴合 JavaScript
- 使用 JavaScript 类型名称（`Number`、`String`、`Boolean`、`Object` 等）
- 使用 JavaScript 运行时 API（如 `Error` 构造函数）
- 避免调用 C# 原生方法，使用映射后的 JavaScript 方法

## 错误处理与调试

### 常见错误场景

| 错误信息 | 原因 | 解决方案 |
|---------|------|----------|
| `Type 'X' is not in whitelist` | 类型未在白名单中 | 检查类型是否被 `[Jazor]` 标记 |
| `Member 'X' is not in whitelist` | 成员未在白名单中 | 检查成员签名是否正确标记 |
| `Hash mismatch for member 'X'` | 方法哈希名与签名不匹配 | 使用 doc 文档中指定的正确哈希值 |
| `Method not compiled` | `extern` 方法但 Op 不是 Import | Import 类型**不能**使用 `extern` |

### extern 使用规则

| Op 类型 | extern? | 原因 |
|---------|---------|------|
| `Discard` | ✅ | 不需要实现，标记为不支持 |
| `Allowed` | ✅ | JS 原生支持，无需额外代码 |
| `Alias` | ✅ | JS 原生方法，只需改名 |
| `Inline` | ✅ | 内联代码提供实现 |
| `Import` | ❌ | 需要完整的 C# 方法体实现 |
| `Compile` | ✅ | 编译器内部处理 |

### 常见陷阱与解决方案

| 问题 | 原因 | 解决方案 |
|------|------|----------|
| 方法体未被编译到 CLR module | `extern` 方法只有白名单标记作用 | 只有 `Op.Import` 需要方法体，其他用 `extern` |
| 占位符未替换 | `@#{n}` 格式错误 | 使用正确格式 `@#{0}`, `@#{1}` |
| 返回值类型错误 | 使用泛型而非 `Array<object?>` | out/ref 方法必须返回 `Array<object?>` |
| 循环引用 | Import 方法互相调用 | 避免模块间循环依赖 |
| 类型映射错误 | 参数类型不正确 | 参考 GlobalUsings.cs 中的类型定义 |

---

## 快速参考

### Op 类型速查表

| 场景 | Op 类型 | extern? | 编译到 CLR module? |
|------|---------|---------|-------------------|
| JS 原生支持，无需处理 | `Allowed` | ✅ | ❌ |
| JS 有类似方法但名称不同 | `Alias` | ✅ | ❌ |
| 可用简单表达式实现 | `Inline` | ✅ | ❌ |
| 需要完整实现 | `Import` | ❌ | ✅ |
| 编译器特殊处理 | `Compile` | ✅ | ❌ |
| 不支持 | `Discard` | ✅ | ❌ |

### 占位符速查表

| 方法类型 | @#{0} | @#{1} | @#{2} |
|----------|-------|-------|-------|
| 实例方法 | 实例 | 参数1 | 参数2 |
| 静态方法 | 参数1 | 参数2 | 参数3 |
| 扩展方法 | 被扩展对象 | 参数1 | 参数2 |

### 常见成员 Op 选择指南

**基础类型成员**：

| 成员类型 | 推荐 Op | 原因 |
|---------|---------|------|
| `ToString()` | `Alias` → `toString` | JS 原生方法 |
| `Equals(object)` | `Inline` → `===` | 简单比较 |
| `GetHashCode()` | `Discard` | JS 无哈希码概念 |
| `Parse(string)` | `Import` | 需验证和异常处理 |
| `TryParse(string, out T)` | `Import` | 需返回数组 |
| 静态常量（MaxValue等） | `Inline` | 字面量内联 |
| 带 IFormatProvider 重载 | `Discard` | JS 无格式化区域概念 |

**集合类型成员**：

| 成员类型 | 推荐 Op | 原因 |
|---------|---------|------|
| 构造函数 `new List()` | `Inline` → `[]` 或 `new Map()` | 简单构造 |
| `Count` / `Length` | `Alias` → `size` / `length` | 属性名不同 |
| `Add(item)` | `Alias` → `push` (Array) 或 `Import` (Dictionary) | Array 直接替换，Dictionary 需检查重复 |
| `Contains(item)` | `Alias` → `includes` / `has` | 方法名不同 |
| 索引器 `this[i]` | `Inline` → `arr[i]` | 直接访问 |

---

## 项目配置

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <LangVersion>14.0</LangVersion>
    <Nullable>enable</Nullable>
    <NoWarn>CS0626,CS0824,IDE0130,CA1822,IDE0060,IDE1006</NoWarn>
    <AllowUnsafeBlocks>False</AllowUnsafeBlocks>
  </PropertyGroup>

  <ItemGroup>
    <ProjectReference Include="..\ECMAScript\ECMAScript.csproj" />
  </ItemGroup>
</Project>
```

### 警告说明

| 警告 | 说明 |
|------|------|
| `CS0626` | extern 方法没有特性 |
| `CS0824` | 构造函数标记为 extern |
| `IDE0130` | 命名空间与文件夹不匹配 |
| `CA1822` | 成员可以标记为静态 |
| `IDE0060` | 未使用的参数 |
| `IDE1006` | 命名风格不符合规则（哈希签名） |

---

## 依赖关系

- **ECMAScript** - 提供 ECMAScript AST 类型和 JavaScript 运行时类型
- **Jazor.Common** - 提供 `[Jazor]` 特性和 `Op` 枚举

---

## 文档资源

- [rule.md](./rule.md) - 详细开发规则文档
- [task.md](./task.md) - 任务完成状态
- [doc/](./doc/) - 各模块成员签名文档
- [CLAUDE.md](../../CLAUDE.md) - Jazor 项目整体开发规则

---

**文档维护者**：developerhan
**最后更新**：2026-03-03
**文档版本**：v4.1
