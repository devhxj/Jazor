# BCL 模块映射规则

本文档记录 C# BCL (Base Class Library) 类型到 JavaScript 的映射规则。

## 1. 模块声明规范

### 1.1 基本结构

```csharp
[ECMAScriptModule]
[Jazor(Op.Import, "C#类型名", "模块路径")]
public static class XxxModule
{
    // 成员映射
}
```

### 1.2 模块声明示例

| C# 类型 | 模块声明 | 说明 |
|---------|----------|------|
| `bool` | `[Jazor(Op.Import, "bool", "System/BooleanModule.js")]` | 需要导入外部模块实现 |
| `object` | `[Jazor(Op.Import, "object", "System/ObjectModule.js")]` | 需要导入外部模块实现 |
| `Console` | `[Jazor(Op.Replace, "System.Console", "console")]` | 直接替换为 JavaScript 全局对象 |

## 2. Op 枚举说明

| Op 值 | 含义 | 用途 | 示例 |
|-------|------|------|------|
| `Discard` | 不支持，丢弃 | 该成员在 JavaScript 中不可用或不适用 | `GetHashCode`, `GetTypeCode`, 控制台输入 |
| `Allowed` | 支持，无其他操作 | 允许调用，无特殊处理 | 默认构造函数 |
| `Replace` | 支持，替换名称 | 将方法名替换为 JavaScript 原生方法名 | `ToString` → `toString`, `WriteLine` → `log` |
| `Import` | 支持，模块导入 | 需要导入外部模块实现 | `Parse`, `TryParse` |
| `Inline` | 支持，内联代码 | 直接嵌入 JavaScript 代码片段 | `Equals` → `===`, `GetType` → `typeof` |

## 3. Op 使用原则

### 3.1 Op.Discard - 不支持的场景

使用场景：
- JavaScript 无对应概念（如 `GetHashCode`, `GetTypeCode`, `TypeCode`）
- 平台特定功能（如控制台输入、光标位置、窗口大小）
- JavaScript 运行时不支持（如 `Console.ReadLine`, `Console.ReadKey`）

```csharp
// 示例：JavaScript 没有 GetHashCode 概念
[Jazor(Op.Discard, "override object.GetHashCode()")]
public extern static Number _97891de43f43ceb4(object instance);

// 示例：浏览器控制台无法读取键盘输入
[Jazor(Op.Discard, "static System.Console.ReadLine()")]
public extern static string? _d665efe65ee40f12();
```

### 3.2 Op.Allowed - 无操作

使用场景：
- JavaScript 有直接等价的默认行为
- 默认构造函数（如 `new object()` → `{}`）

```csharp
// 示例：JavaScript 布尔值是原始类型，不需要构造函数
[Jazor(Op.Allowed, "bool.Boolean()")]
public extern static bool _2bd9618624257446();

// 示例：JavaScript 对象直接使用字面量创建
[Jazor(Op.Allowed, "object.Object()")]
public extern static object _4aea088b73a04a68();
```

### 3.3 Op.Replace - 方法名替换

使用场景：
- JavaScript 有原生对应方法，但名称不同
- 方法签名和语义一致

```csharp
// 示例：ToString 替换为 toString
[Jazor(Op.Replace, "override bool.ToString()", "toString")]
public extern static string _d48c2d39317daf8f(bool instance);

// 示例：Console.WriteLine 替换为 console.log
[Jazor(Op.Replace, "static System.Console.WriteLine()", "log")]
public extern static void _64a3c7e35feaa9f0();

// 示例：Console.Clear 替换为 console.clear
[Jazor(Op.Replace, "static System.Console.Clear()", "clear")]
public extern static void _7779d957d8f16481();
```

### 3.4 Op.Inline - 内联代码

使用场景：
- JavaScript 有对应操作符，直接内联表达式
- 简单逻辑，无需函数调用
- 类型检查操作（`typeof`, `instanceof`）

```csharp
// 示例：Equals 内联为 === 操作符
[Jazor(Op.Inline, "override bool.Equals(object)", "(@#{0} === @#{1})")]
public extern static bool _97cc6572c33639b7(bool instance, object? obj);

// 示例：GetType 内联为 typeof 操作符
[Jazor(Op.Inline, "object.GetType()", "typeof @#{0}")]
public extern static string _393ae40d42f17afb(object instance);

// 示例：CompareTo 内联为条件表达式
[Jazor(Op.Inline, "bool.CompareTo(object)", "(@#{0} === @#{1} ? 0 : (@#{0} ? 1 : -1))")]
public extern static Number _f877237b160159b0(bool instance, object? obj);

// 示例：静态字段内联为字面量
[Jazor(Op.Inline, "static readonly bool.TrueString", "true")]
public extern static bool _49c57acefc093fcc();
```

**占位符说明**：
- `@#{0}` - 第一个参数（实例或第一个参数）
- `@#{1}` - 第二个参数
- 以此类推...

### 3.5 Op.Import - 模块导入

使用场景：
- 解析逻辑较复杂，需要完整的 JavaScript 实现
- JavaScript 没有直接对应的方法
- 需要额外的辅助函数

```csharp
// 示例：Parse 方法需要完整实现
[Jazor(Op.Import, "static bool.Parse(string)")]
public static bool _5dbf54319ebc8dfe(string? value)
{
    var str = value?.Trim()?.ToLower();
    if (str == "true")
        return true;
    else if (str == "false")
        return false;
    else
        throw new Error($"FormatException: String '{value}' was not recognized as a valid Boolean.");
}

// 示例：TryParse 返回数组模拟 out 参数
[Jazor(Op.Import, "static bool.TryParse(string, out bool)")]
public static Array<object?> _dada4bbdacd7aa19(string? value, bool result)
{
    var str = value?.Trim()?.ToLower();
    if (str == "true")
        return [true, true];
    else if (str == "false")
        return [true, false];
    return [false, false];
}
```

## 4. 类型映射表

### 4.1 基本类型映射

| C# 类型 | JavaScript 类型 | TypeMapper | 说明 |
|---------|-----------------|------------|------|
| `bool` | `boolean` | `Boolean` | 原始类型 |
| `bool?` | `boolean \| null` | - | 可空类型 |
| `char` | `string` | `String` | 单字符字符串 |
| `char[]` | `string` | `String` | 字符数组映射为字符串 |
| `string` | `string` | `String` | 原始类型 |
| `string?` | `string \| null` | - | 可空类型 |
| `object` | `object` | `Object` | 基类 |

### 4.2 数值类型映射

| C# 类型 | JavaScript 类型 | TypeMapper | 说明 |
|---------|-----------------|------------|------|
| `byte`, `sbyte` | `Number` | `Number` | 8位整数 |
| `short`, `ushort` | `Number` | `Number` | 16位整数 |
| `int`, `uint` | `Number` | `Number` | 32位整数 |
| `float`, `double` | `Number` | `Number` | 浮点数 |
| `decimal` | `Number` | `Number` | 十进制数 |
| `long`, `ulong` | `BigInt` | `BigInt` | 64位整数 |
| `Int128`, `UInt128` | `BigInt` | `BigInt` | 128位整数 |
| `BigInteger` | `BigInt` | `BigInt` | 任意精度整数 |

### 4.3 特殊类型映射

| C# 类型 | JavaScript 类型 | 说明 |
|---------|-----------------|------|
| `ReadOnlySpan<char>` | `Uint32Array` | 每个 Unicode 码点一个元素 |
| `Span<char>` | `Uint32Array` | 每个 Unicode 码点一个元素 |
| `System.Type` | `object` | JavaScript 无类型系统 |
| `System.TypeCode` | - | 丢弃 |

## 5. 方法命名规范

### 5.1 哈希命名

模块内的方法使用哈希值命名（如 `_5dbf54319ebc8dfe`），避免命名冲突。

```csharp
// 方法名是成员签名的哈希值
[Jazor(Op.Import, "static bool.Parse(string)")]
public static bool _5dbf54319ebc8dfe(string? value) { ... }
```

### 5.2 命名规则

- 使用 `_` 前缀
- 16位十六进制哈希值
- 基于成员签名生成

## 6. out 参数处理

### 6.1 返回数组模式

C# 的 out 参数在 JavaScript 中通过返回数组模拟：

```csharp
// C# 方法签名
static bool TryParse(string, out bool result)

// JavaScript 返回值格式
[success, value]
```

### 6.2 使用示例

```csharp
// C# 代码
if (bool.TryParse(input, out bool result))
{
    Console.WriteLine(result);
}
```

```javascript
// JavaScript 代码
let [success, result] = TryParse(input, false);
if (success) {
    console.log(result);
}
```

### 6.3 多个 out 参数

```csharp
// C# 方法签名
static bool TryFormat(Span<char>, out int charsWritten)

// JavaScript 返回值格式
[success, charsWritten] 或 [returnValue, outParam1, outParam2, ...]
```

## 7. 类型模块示例

### 7.1 Boolean 类型

| C# 成员 | Op | 替换/内联值 | JavaScript 结果 |
|---------|-----|-------------|-----------------|
| `static readonly bool.TrueString` | Inline | `"true"` | 字符串字面量 `"true"` |
| `static readonly bool.FalseString` | Inline | `"false"` | 字符串字面量 `"false"` |
| `bool.Boolean()` | Allowed | - | 无操作 |
| `override bool.GetHashCode()` | Discard | - | 不支持 |
| `override bool.ToString()` | Replace | `toString` | `instance.toString()` |
| `bool.ToString(IFormatProvider)` | Discard | - | 不支持 |
| `override bool.Equals(object)` | Inline | `(@#{0} === @#{1})` | `(a === b)` |
| `bool.Equals(bool)` | Inline | `(@#{0} === @#{1})` | `(a === b)` |
| `bool.CompareTo(object)` | Inline | `(@#{0} === @#{1} ? 0 : (@#{0} ? 1 : -1))` | 条件表达式 |
| `static bool.Parse(string)` | Import | - | 模块函数调用 |
| `static bool.TryParse(string, out bool)` | Import | - | 返回 `[success, value]` |

### 7.2 Object 类型

| C# 成员 | Op | 替换/内联值 | JavaScript 结果 |
|---------|-----|-------------|-----------------|
| `object.GetType()` | Inline | `typeof @#{0}` | `typeof obj` |
| `object.Object()` | Allowed | - | 无操作 |
| `virtual object.ToString()` | Replace | `toString` | `obj.toString()` |
| `virtual object.Equals(object)` | Inline | `(@#{0} === @#{1})` | `(a === b)` |
| `static object.Equals(object, object)` | Inline | `(@#{0} === @#{1})` | `(a === b)` |
| `static object.ReferenceEquals(object, object)` | Inline | `(@#{0} === @#{1})` | `(a === b)` |
| `virtual object.GetHashCode()` | Discard | - | 不支持 |

### 7.3 Console 类型

| C# 成员 | Op | 替换值 | 说明 |
|---------|-----|--------|------|
| `static Console.WriteLine()` | Replace | `log` | `console.log()` |
| `static Console.WriteLine(T)` | Replace | `log` | `console.log(value)` |
| `static Console.Write(T)` | Replace | `log` | `console.log(value)` |
| `static Console.Clear()` | Replace | `clear` | `console.clear()` |
| `static Console.ReadLine()` | Discard | - | 浏览器不支持 |
| `static Console.ReadKey()` | Discard | - | 浏览器不支持 |
| 所有属性（In, Out, Error 等） | Discard | - | 浏览器不支持 |
| 所有缓冲区/窗口方法 | Discard | - | 浏览器不支持 |

## 8. 设计原则

### 8.1 为什么 GetHashCode 被丢弃？

- JavaScript 没有统一的 `GetHashCode` 机制
- `Map` 和 `Set` 使用引用相等或值相等，不需要哈希码
- 布尔值的哈希码（0 或 1）在 JavaScript 中无实际用途

### 8.2 为什么 ToString 使用 Replace 而非 Inline？

- JavaScript 布尔值和对象原生支持 `toString()` 方法
- `true.toString()` 和 `false.toString()` 与 C# 语义一致
- 使用 `Replace` 可以直接调用原生方法，效率更高

### 8.3 为什么 Equals 使用 Inline？

- JavaScript 的严格相等 `===` 与 C# 的 `Equals` 语义完全一致
- 内联代码避免了额外的函数调用开销
- 对于引用类型，`===` 比较引用，与 `Object.Equals` 语义一致

### 8.4 Parse/TryParse 为什么使用 Import？

- 解析逻辑较复杂（空格处理、大小写处理、错误处理）
- 需要完整的 JavaScript 实现
- 使用 `Import` 保持代码清晰，便于维护

### 8.5 Console 的属性和方法为什么大多丢弃？

- JavaScript 运行在浏览器或 Node.js 环境
- 浏览器控制台没有输入功能（`ReadLine`, `ReadKey`）
- 浏览器控制台没有光标、窗口、缓冲区概念
- 只保留输出功能（`Write`, `WriteLine`, `Clear`）

## 9. 注释规范

每个映射方法应包含 XML 注释，说明：
1. C# 原始签名
2. JavaScript 转换结果
3. 特殊说明（如类型映射、语义差异等）

```csharp
/// <summary>
/// C#: obj.GetType()
/// JS: typeof obj
/// 注意：JavaScript 的 typeof 返回类型字符串（如 "object", "string", "number" 等）
/// </summary>
[Jazor(Op.Inline, "object.GetType()", "typeof @#{0}")]
public extern static string _393ae40d42f17afb(object instance);
```

## 10. 注意事项

1. **out 参数处理**：C# 的 out 参数在 JavaScript 中通过返回数组模拟，返回值格式为 `[success, value]` 或 `[returnValue, outParam1, outParam2, ...]`

2. **ReadOnlySpan<char> 处理**：映射为 `Uint32Array`，每个元素是一个 Unicode 码点

3. **方法命名**：模块内方法使用哈希值命名（如 `_5dbf54319ebc8dfe`），避免命名冲突

4. **可空处理**：`string?` 参数需要使用可选链操作符 `?.` 处理空值

5. **类型系统差异**：
   - C# `GetType()` 返回 `Type` 对象
   - JavaScript `typeof` 返回类型字符串（`"object"`, `"string"`, `"number"` 等）

6. **Console.Write 行为差异**：
   - C# `Write` 不换行，`WriteLine` 换行
   - JavaScript `console.log` 总是换行
   - 两者语义不完全一致，但可接受

## 11. 模块分类

### 11.1 基本类型模块

- `BooleanModule` - `bool` 类型映射
- `CharModule` - `char` 类型映射
- `StringModule` - `string` 类型映射
- `ObjectModule` - `object` 类型映射

### 11.2 数值类型模块

- `SByteModule`, `ByteModule` - 8位整数
- `Int16Module`, `UInt16Module` - 16位整数
- `Int32Module`, `UInt32Module` - 32位整数
- `Int64Module`, `UInt64Module` - 64位整数
- `SingleModule`, `DoubleModule` - 浮点数
- `DecimalModule` - 十进制数
- `BigIntegerModule` - 任意精度整数

### 11.3 日期时间模块

- `DateTimeModule` - `DateTime` 类型
- `DateTimeOffsetModule` - `DateTimeOffset` 类型
- `DateOnlyModule` - `DateOnly` 类型
- `TimeOnlyModule` - `TimeOnly` 类型
- `TimeSpanModule` - `TimeSpan` 类型

### 11.4 集合类型模块

- `ArrayModule` - `Array` 类型
- `ListModule` - `List<T>` 类型
- `DictionaryModule` - `Dictionary<K,V>` 类型
- `HashSetModule` - `HashSet<T>` 类型

### 11.5 其他模块

- `ConsoleModule` - `Console` 类型
- `ConvertModule` - `Convert` 类型
- `MathModule` - `Math` 类型
- `CultureInfoModule` - `CultureInfo` 类型

---

**文档维护者**：developerhan
**最后更新**：2026-02-25
**文档版本**：v4.0