# BCL 模块映射规则

本文档记录 C# BCL (Base Class Library) 类型到 JavaScript 的映射规则。

> **Jazor.CLR 的双重作用**：
> 1. **白名单来源**：为 Jazor 编译器提供白名单配置
> 2. **CLR Module 库**：被 Jazor 编译成 CLR module，供编译时引用
---

## 目录

1. [特性体系说明](#1-特性体系说明)
2. [模块声明规范](#2-模块声明规范)
3. [Op 枚举说明](#3-op-枚举说明)
4. [类型映射表](#4-类型映射表)
5. [命名格式规范](#5-命名格式规范)
6. [out/ref 参数处理](#6-outref-参数处理)
7. [快速参考表](#7-快速参考表)
8. [附录](#8-附录)

---

## 1. 特性体系说明

### 1.1 [ECMAScript] 系列（用户代码标记）

| 特性 | 用途 |
|------|------|
| `[ECMAScriptModule]` | 标记类生成 ES module |
| `[ECMAScript]` | 标记可被编译器识别的资源型类型 |
| `[ECMAScriptIgnore]` | 标记被编译器忽略的成员 |
| `[ECMAScriptInline]` | 标记方法直接使用内联代码 |
| `[ECMAScriptName]` | 标记编译时别称（优先级 > [Description]） |

**编译器处理规则**：只处理 `[ECMAScriptModule]` 标记的静态类中有实现的方法，或带 `[ECMAScriptInline]` 的 extern 方法。

### 1.2 [Jazor] 系列（Jazor.CLR 内部）

用于白名单生成和 CLR Module 编译双重功能。

**构造方法**：
- 无参：`Op = Op.Compile`
- 单字符串：`Op = Op.Inline, Value = 内联代码`
- 三参数：`(Op op, string member, string? value = null)`

**工作流程**：

```
Jazor.CLR 项目
     │
     ├── [Jazor] 标记 ───→ 白名单生成器 ───→ Analyzer 白名单
     │
     └── 编译 ───────────→ CLR Module 库 ───→ Compiler 引用
```

---

## 2. 模块声明规范

### 2.1 扁平化原则

```
C# 实例方法 ──► 静态方法（实例作为第一个参数）
C# 静态方法 ──► 静态方法（保持原参数）
C# 属性 get ──► 静态方法（实例作为第一个参数）
C# 属性 set ──► 静态方法（实例 + 值作为参数）
```

**注意**：

- doc文件夹中已经定义好了BCL类型被扁平化的member及对应hash和注释
- module文件夹是根据doc文件生成的BCLModule，严格按照doc中指定的member及对应hash
- `BooleanModule.md`和`BooleanModule.cs`是样板参考，在使用本rule.md时不要参考其他模块
- 本rule.md中使用的member和_hash都是代指，一切member和hash以doc文档为准

### 2.2 基本结构

```csharp
[ECMAScriptModule]
[Jazor(Op.Import, "C#类型名", "模块路径")]
public static class XxxModule { }
```

### 2.3 模块路径规范

| C# 类型 | 模块声明 | 说明 |
|---------|----------|------|
| `bool` | `[Jazor(Op.Import, "bool", "System/BooleanModule.js")]` | 导入外部模块 |
| `Console` | `[Jazor(Op.Replace, "System.Console", "console")]` | 替换为全局对象 |

**路径命名规则**：
- 命名空间映射：`System.Collections.Generic` → `System/Collections/Generic/`
- 文件命名：`{类型名}Module.js`
- 泛型类型：`` `n `` 表示参数数量，如 `List`1Module.js`
- 嵌套类型：使用 `+` 连接，如 `Outer+InnerModule.js`

---

## 3. Op 枚举说明

### 3.1 Op 类型概览

| Op 类型 | extern? | 使用场景 |
|---------|---------|----------|
| `Discard` | ✅ | JavaScript 无对应概念，丢弃 |
| `Allowed` | ✅ | JavaScript 原生支持，无需处理 |
| `Replace` | ✅ | JS 有同名语义方法但名称不同 |
| `Inline` | ✅ | 简单表达式可直接内联 |
| `Import` | ❌ | 需要完整 JavaScript 实现 |
| `Compile` | ✅ | 编译器特殊处理 |

**`extern` 含义**：方法不需要 C# 实现（由 JS 原生或内联代码提供）。**只有 `Import` 不使用 `extern`**。

### 3.2 Op.Discard - 不支持

JavaScript 无对应概念（如 `GetHashCode`, `Console.ReadLine`）。

```csharp
[Jazor(Op.Discard, "override object.GetHashCode()")]
public extern static Number _hash(object instance);
```

### 3.3 Op.Allowed - 无操作

JavaScript 原生支持，默认行为正确（如默认构造函数）。

```csharp
[Jazor(Op.Allowed, "bool.Boolean()")]  // JS 布尔是原始类型
public extern static bool _hash();
```

### 3.4 Op.Replace - 方法名替换

JS 有原生方法但名称不同。

```csharp
[Jazor(Op.Replace, "override bool.ToString()", "toString")]
public extern static string _hash(bool instance);
// 生成：instance.toString()
```

### 3.5 Op.Inline - 内联代码

用占位符模板直接生成 JavaScript 表达式。

**占位符规则**：

| 方法类型 | @#{0} | @#{1} | @#{2} |
|----------|-------|-------|-------|
| 实例方法 | 实例 | 参数1 | 参数2 |
| 静态方法 | 参数1 | 参数2 | 参数3 |

```csharp
// Equals → 严格相等
[Jazor(Op.Inline, "override bool.Equals(object)", "(@#{0} === @#{1})")]
public extern static bool _hash(bool instance, object? obj);

// 静态字段 → 字面量
[Jazor(Op.Inline, "static readonly bool.TrueString", "true")]
public extern static bool _hash();
```

### 3.6 Op.Import - 模块导入

需要完整 JavaScript 实现，**不使用 `extern`**，提供 C# 方法体。

```csharp
[Jazor(Op.Import, "static bool.Parse(string)")]
public static bool _hash(string? value)
{
    var str = value?.Trim()?.ToLower();
    if (str == "true") return true;
    if (str == "false") return false;
    throw new Error($"FormatException: String '{value}' was not recognized as a valid Boolean.");
}
```
**注意**：

- Op.Import 必须使用 JavaScript 原生实现，禁止调用被映射的 C# 方法（避免循环调用）
- Op.Import 实现的方法体必须健壮，不能简写。

### 3.7 Op.Compile - 编译器特殊处理

编译器有内置特殊逻辑，如常量内联、特殊类型转换。

| 特性 | Op.Inline | Op.Compile |
|------|-----------|------------|
| 处理时机 | 代码生成阶段 | 编译器内部处理 |
| 占位符 | 支持 `@#{n}` | 由编译器决定 |
| 适用场景 | 简单表达式替换 | 复杂逻辑、编译器内置处理 |

### 3.8 Op 选择决策

```
JS 有原生对应？
├── 是，名称相同 → Allowed
├── 是，名称不同 → Replace
└── 否 → JS 有概念？
    ├── 否 → Discard
    ├── 简单表达式 → Inline
    ├── 复杂逻辑 → Import
    └── 编译器处理 → Compile
```

---

## 4. 类型映射表

### 4.1 基本类型映射

| C# 类型 | JavaScript 类型 | TypeMapper | 备注 |
|---------|-----------------|------------|------|
| `bool` | `boolean` | `Boolean` | 原始类型 |
| `char` | `string` | `String` | 单字符字符串 |
| `string` | `string` | `String` | 原始类型 |
| `object` | `object` | `Object` | 基类 |

### 4.2 数值类型映射

| C# 类型 | JS 类型 | TypeMapper |
|---------|---------|------------|
| `byte`, `sbyte`, `short`, `ushort`, `int`, `uint`, `float`, `double`, `decimal` | `Number` | `Number` |
| `long`, `ulong`, `Int128`, `UInt128`, `BigInteger`, `TimeSpan` | `BigInt` | `BigInt` |
| `DateTime`, `DateTimeOffset`, `DateOnly`, `TimeOnly` | `Date` | `Date` |

### 4.3 集合类型映射

| C# 类型 | JS 类型 | TypeMapper |
|---------|---------|------------|
| `Array<T>`, `List<T>`, `IList<T>`, `IEnumerable<T>` | `Array` | `Array` |
| `Dictionary<K,V>`, `IDictionary<K,V>` | `Map` | `Map` |
| `HashSet<T>`, `ISet<T>` | `Set` | `Set` |

### 4.4 特殊类型映射

| C# 类型 | JS 类型 | 说明 |
|---------|---------|------|
| `ReadOnlySpan<char>`, `Span<char>` | `string` | 无 |
| `System.Type` | `object` | JS 无类型系统 |
| `System.TypeCode` | - | 丢弃 |

### 4.5 异常类型映射

| C# 异常 | JS 类型 | C# 异常 | JS 类型 |
|---------|---------|---------|---------|
| `Exception`, `SystemException`, `ArgumentException` | `Error` | `ArgumentNullException` | `TypeError` |
| `ArgumentOutOfRangeException`, `IndexOutOfRangeException` | `RangeError` | `DivideByZeroException` | `Error`/`Infinity` |

**异常处理注意**：JS 不支持异常链，`InnerException` 需手动拼接消息。

---

## 5. 命名格式规范

### 5.1 成员签名格式

**方法签名**：`[修饰符] 返回类型.方法名(参数类型列表)`

```
static bool.Parse(string)
override bool.ToString()
bool.CompareTo(object)
static bool.TryParse(string, out bool)
```

**构造函数签名**：`类型.类型名(参数类型列表)`

```
bool.Boolean()
bool.Boolean(bool)
```

**属性签名**：映射为 get/set 静态方法

```
string.get_Length()          → instance.length
string.set_Chars(int, char)  → instance[index] = value
```

**索引器签名**：映射为 get_Item/set_Item

```
List`1.get_Item(int)                  → list[index]
Dictionary`2.set_Item(TKey, TValue)   → dict[key] = value
```

**字段签名**：`[修饰符] 类型.字段名`

```
static readonly bool.TrueString
const int.MaxValue
```

### 5.2 泛型与可空

**泛型表示**：使用 `` `n `` 标记参数数量

```
List`1.Add(T)
Dictionary`2.get_Item(TKey)
System.Linq.Enumerable.`1.Where[T](IEnumerable{T}, Func{T,bool})
```

**可空表示**：使用 `?` 标记

```
static bool.Parse(string?)
bool.TryParse(string?, out bool)
```

### 5.3 方法哈希命名

模块内方法使用哈希值命名（`_` + 16位十六进制），基于完整签名生成，确保唯一性。

**算法**：SHA256 签名 → 取前 8 字节 → 转 16 位十六进制

```csharp
public static string GenerateHashName(string signature)
{
    using var sha256 = SHA256.Create();
    var hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(signature));
    var sb = new StringBuilder("_");
    for (int i = 0; i < 8; i++)
        sb.Append(hash[i].ToString("x2"));
    return sb.ToString();  // 示例: _5dbf54319ebc8dfe
}
```

| 成员签名 | 哈希名 |
|----------|--------|
| `static bool.Parse(string)` | `_5dbf54319ebc8dfe` |
| `override bool.ToString()` | `_d48c2d39317daf8f` |

### 5.4 注释规范

每个映射方法应包含 XML 注释：

```csharp
/// <summary>
/// C#: obj.GetType()
/// JS: typeof obj
/// 注意：JS typeof 返回类型字符串
/// </summary>
[Jazor(Op.Inline, "object.GetType()", "typeof @#{0}")]
public extern static string _hash(object instance);
```

---

## 6. out/ref 参数处理

> **重要**：虽然 C# 中 `out` 和 `ref` 有语义区别，但 **Jazor 使用同种方式处理**：返回数组模拟，调用处解构。

### 6.1 返回数组模式

```csharp
// C# 签名
static bool TryParse(string, out bool result)
static void Modify(ref int value)

// JS 返回值格式（两者相同）
[returnValue, refOutValue]
```

### 6.2 定义处示例

```csharp
// out 参数
[Jazor(Op.Import, "static bool.TryParse(string, out bool)")]
public static Array<object?> _hash(string? value, bool result)
{
    return [true, parsedValue];  // [success, result]
}

// ref 参数
[Jazor(Op.Import, "static void Increment(ref int)")]
public static Array<object?> _hash(int value)
{
    return [null, value + 1];  // [void, incrementedValue]
}
```

### 6.3 调用处生成

```csharp
// C# 代码
if (bool.TryParse(input, out result)) { }

// JS 生成
let $0;
if (($0 = _hash(input, result), result = $0[1], $0[0])) { }
```

**规则**：
- 返回值统一使用 `Array<object?>`
- 数组格式：`[返回值, out/ref参数1, out/ref参数2, ...]`
- 占位符 `@#{n}` 与普通参数相同

### 6.4 可空类型处理

在签名和实现中保持可空语义：

```csharp
[Jazor(Op.Import, "static bool.Parse(string?)")]
public static bool _hash(string? value)
{
    var str = value?.Trim()?.ToLower();  // 使用可选链
    // ...
}
```

---

## 7. 快速参考表

### 7.1 Op 类型速查表

| 场景 | Op 类型 | extern? | 示例 |
|------|---------|---------|------|
| JS 原生支持，无需处理 | `Allowed` | ✅ | `object.Object()` |
| JS 有类似方法但名称不同 | `Replace` | ✅ | `ToString()` → `toString` |
| 可用简单表达式实现 | `Inline` | ✅ | `(@#{0} === @#{1})` |
| 需要完整实现 | `Import` | ❌ | `bool.Parse(string)` |
| 编译器特殊处理 | `Compile` | ✅ | 特殊类型转换 |
| 不支持 | `Discard` | ✅ | `GetHashCode()` |

### 7.2 占位符速查表

| 方法类型 | @#{0} | @#{1} | @#{2} |
|----------|-------|-------|-------|
| 实例方法 | 实例 | 参数1 | 参数2 |
| 静态方法 | 参数1 | 参数2 | 参数3 |
| 扩展方法 | 被扩展对象 | 参数1 | 参数2 |

### 7.3 类型映射速查表

| C# 类型 | JS 类型 | 类型检查方式 |
|---------|---------|-------------|
| `bool` | `boolean` | `typeof x === "boolean"` |
| `string` | `string` | `typeof x === "string"` |
| `int`, `double` | `number` | `typeof x === "number"` |
| `long`, `BigInteger` | `bigint` | `typeof x === "bigint"` |
| `DateTime` | `Date` | `x instanceof Date` |
| `Array`, `List<T>` | `Array` | `Array.isArray(x)` |
| `object` | `object` | `typeof x === "object"` |

### 7.4 常见签名模式

```csharp
// 实例方法
[Jazor(Op.Replace, "Type.MethodName(ParamType)", "jsMethodName")]
public extern static ReturnType _hash(Type instance, ParamType param);

// 静态方法
[Jazor(Op.Replace, "static Type.MethodName(ParamType)", "jsMethodName")]
public extern static ReturnType _hash(ParamType param);

// 属性 get/set
[Jazor(Op.Replace, "Type.get_PropertyName()", "jsPropertyName")]
[Jazor(Op.Replace, "Type.set_PropertyName(PropertyType)", "jsPropertyName")]

// 带 out 参数
[Jazor(Op.Import, "static Type.TryParse(string, out Type)")]
public static Array<object?> _hash(string? value, Type result)
{
    return [true, parsed];  // [success, value]
}
```

### 7.5 常见陷阱与解决方案

| 问题 | 原因 | 解决方案 |
|------|------|----------|
| 方法未被编译 | `extern` 但 Op 不是 Import | `Import` 类型**不能**使用 `extern` |
| 白名单未生成 | 特性拼写错误 | 检查 `[Jazor]` 而非 `[JazorModule]` |
| 占位符未替换 | `@#{n}` 格式错误 | 使用正确格式 `@#{0}`, `@#{1}` |
| 返回值类型错误 | 使用泛型而非 `Array<object?>` | out/ref 方法必须返回 `Array<object?>` |

---

## 8. 附录

### 8.1 术语表

| 术语 | 说明 |
|------|------|
| BCL | Base Class Library，.NET 基础类库 |
| CLR | Common Language Runtime，公共语言运行时 |
| ESTree | ECMAScript AST 标准 |
| TypeMapper | Jazor 内部类型映射枚举 |
| 类型擦除 | 泛型类型在运行时失去类型参数信息 |

### 8.2 类型模块示例

**Boolean 类型**：

| C# 成员 | Op | JavaScript 结果 |
|---------|-----|-----------------|
| `static bool.TrueString` | Inline | `"true"` |
| `bool.Boolean()` | Allowed | 无操作 |
| `override bool.ToString()` | Replace | `instance.toString()` |
| `override bool.Equals(object)` | Inline | `(a === b)` |
| `static bool.Parse(string)` | Import | 模块函数调用 |
| `static bool.TryParse(string, out bool)` | Import | 返回 `[success, value]` |

**Int32 类型**：

| C# 成员 | Op | JavaScript 结果 |
|---------|-----|-----------------|
| `static int.MaxValue` | Inline | `2147483647` |
| `static int.Parse(string)` | Import | 模块函数，带验证 |
| `static int.Max(int, int)` | Replace | `Math.max(a, b)` |

**String 类型**：

| C# 成员 | Op | JavaScript 结果 |
|---------|-----|-----------------|
| `string.get_Length()` | Replace | `str.length` |
| `string.Contains(string)` | Replace | `str.includes(value)` |
| `string.Trim()` | Replace | `str.trim()` |
| `static string.IsNullOrEmpty(string)` | Inline | `!value` |

### 8.3 设计原则

1. **GetHashCode 处理差异**：`bool/object.GetHashCode()` → `Discard`（JS 无哈希码机制）；`string.GetHashCode()` → `Import`（有实际用途）
2. **ToString 使用 Replace**：JS 原生支持 `toString()`，直接调用原生方法效率更高
3. **Equals 使用 Inline**：`===` 与 C# `Equals` 语义一致，内联避免函数调用开销
4. **Parse/TryParse 使用 Import**：解析逻辑复杂，需完整 JS 实现

### 8.4 注意事项

1. **out/ref 统一处理**：返回数组 `[returnValue, outParam1, ...]`，调用处解构
2. **方法命名**：使用 `_` + 16位哈希，基于完整签名生成
3. **可空处理**：`string?` 使用可选链 `?.` 处理空值
4. **类型系统差异**：C# `GetType()` 返回 `Type`；JS `typeof` 返回字符串
5. **Console 差异**：C# `Write` 不换行，JS `console.log` 总是换行
6. **嵌套类型路径**：使用 `+` 连接，如 `Outer+InnerModule.js`
7. **泛型类型路径**：使用 `` `n `` 标记参数数量，如 `List`1Module.js`
8. **方法重载**：JS 不支持重载，不同重载需不同哈希名
9. **循环引用**：避免 `Op.Import` 方法互相调用

### 8.5 不支持的特性

| 特性 | 原因 |
|------|------|
| 事件 (`event`) | JS 使用回调/订阅模式，无多播事件 |
| 委托 (`delegate`) | JS 只有函数引用 |
| `unsafe` 代码 | JS 是安全语言，无指针操作 |
| `sizeof`/`stackalloc` | JS 无固定内存布局和栈分配 |

### 8.6 相关文档

- [CLAUDE.md](../../CLAUDE.md) - 项目整体架构和转换思想
- [Jazor.Name/rule.md](../Jazor.Name/rule.md) - 命名规范详细说明
- `doc/` 目录 - 各类型模块详细文档

---

**文档维护者**：developerhan
**最后更新**：2026-02-27
**文档版本**：v4.0
