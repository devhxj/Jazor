# BCL 模块映射规则

本文档记录 C# BCL (Base Class Library) 类型到 JavaScript 的映射规则。

> **Jazor.CLR 的双重作用**：
> 1. **白名单来源**：为 Jazor 编译器提供白名单配置
> 2. **CLR Module 库**：被 Jazor 编译成 CLR module，供编译时引用

## 特性体系说明

### 体系一：[ECMAScript] 系列（用户代码标记）

用于开发者标记自己的 C# 代码，告诉 Jazor 编译器如何转换为 JavaScript。

| 特性 | 命名空间 | 用途 |
|------|----------|------|
| `[ECMAScriptModule]` | `ECMAScript` | 标记类生成 ES module |
| `[ECMAScript]` | `ECMAScript` | 标记可被编译器识别的资源型类型 |
| `[ECMAScriptIgnore]` | `ECMAScript` | 标记被编译器忽略的成员 |
| `[ECMAScriptInline]` | `ECMAScript` | 标记方法直接使用内联代码 |

**使用场景**：开发者在自己的项目中使用

### 体系二：[Jazor]（Jazor.CLR 内部使用）

用于 `Jazor.CLR` 项目内部，实现双重功能：

1. **白名单来源**：`Jazor.Compiler.Generator` 读取 `[Jazor]` 特性生成白名单
2. **CLR Module 编译**：`Jazor.CLR` 项目本身被编译成 CLR module 库，供编译时引用

| 特性 | 命名空间 | 用途 |
|------|----------|------|
| `[Jazor]` | `Jazor.Common` | 标记 BCL 类型的映射规则 |

**工作流程**：

```
Jazor.CLR 项目
     │
     ├── [Jazor] 标记 ───→ 白名单生成器 ───→ Analyzer 白名单
     │
     └── 编译 ───────────→ CLR Module 库 ───→ Compiler 引用
```

## 1. 模块声明规范

### 1.1 核心设计原则

**Jazor.CLR 将 BCL 类型扁平化为静态类静态方法**：

```
C# 实例方法 ──► 静态方法（实例作为第一个参数）
C# 静态方法 ──► 静态方法（保持原参数）
C# 属性 get ──► 静态方法（实例作为第一个参数）
C# 属性 set ──► 静态方法（实例作为第一个参数，值作为第二个参数）
```

### 1.2 基本结构

```csharp
[ECMAScriptModule]
[Jazor(Op.Import, "C#类型名", "模块路径")]
public static class XxxModule
{
    // 成员映射
}
```

### 1.3 模块声明示例

| C# 类型 | 模块声明 | 说明 |
|---------|----------|------|
| `bool` | `[Jazor(Op.Import, "bool", "System/BooleanModule.js")]` | 需要导入外部模块实现 |
| `object` | `[Jazor(Op.Import, "object", "System/ObjectModule.js")]` | 需要导入外部模块实现 |
| `Console` | `[Jazor(Op.Replace, "System.Console", "console")]` | 直接替换为 JavaScript 全局对象 |

## 2. Op 枚举说明

| Op 值 | 含义 | 用途 | 是否需要 `extern` | 示例 |
|-------|------|------|-------------------|------|
| `Discard` | 不支持，丢弃 | 该成员在 JavaScript 中不可用或不适用 | ✅ 需要 | `GetHashCode`, `GetTypeCode`, 控制台输入 |
| `Allowed` | 支持，无其他操作 | 允许调用，无特殊处理 | ✅ 需要 | 默认构造函数 |
| `Replace` | 支持，替换名称 | 将方法名替换为 JavaScript 原生方法名 | ✅ 需要 | `ToString` → `toString`, `WriteLine` → `log` |
| `Import` | 支持，模块导入 | C# 实现会被编译为 JS 导出方法 | ❌ **不需要**（必须有方法体） | `Parse`, `TryParse` |
| `Inline` | 支持，内联代码 | 直接嵌入 JavaScript 代码片段 | ✅ 需要 | `Equals` → `===`, `GetType` → `typeof` |

## 3. Op 使用原则

### 3.1 `extern` 关键字含义

**`extern` 表示该方法不需要实现或没有实现。**

**`extern + Op` 组合的具体含义**：

| 组合 | 含义 | 实际行为 |
|------|------|---------|
| `extern + Allowed` | JS 自有，无需处理 | JS 原生支持该语义，无需映射代码 |
| `extern + Replace` | JS 有同样语义的方法，但名称不同 | 调用 JS 原生方法，只需替换名称 |
| `extern + Inline` | 直接内联代码片段 | 替换为指定的内联表达式 |
| `extern + Discard` | 不支持 | 该成员在 JS 中无对应概念 |

**注意**：
- `extern` 是 C# 语法的一部分，表示**外部实现**
- 在 `Jazor.CLR` 中，它明确表示**该代码不会作为 C# 运行**
- **`extern` 仅用于 `Discard`、`Allowed`、`Replace`、`Inline` 四种 Op 类型**
- **`Op.Import` 不使用 `extern`**，而是提供 C# 方法体，会被 Jazor 编译器编译为 JavaScript 导出方法
- 没有 `extern` 且带有 `[Jazor]` 特性的方法表示**该 C# 代码会被编译器编译为 JS**（`Op.Import` 的情况）

### 3.2 Op.Discard - 不支持的场景

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

### 3.3 Op.Allowed - 无操作

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

### 3.4 Op.Replace - 方法名替换

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

### 3.5 Op.Inline - 内联代码

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

**占位符替换详细规则**：

| 场景 | @#{0} | @#{1} | @#{2} | 说明 |
|------|-------|-------|-------|------|
| 实例方法 | 实例表达式 | 第一个显式参数 | 第二个显式参数 | 包含隐式 this |
| 静态方法 | 第一个参数 | 第二个参数 | 第三个参数 | 无 this |
| 扩展方法 | 被扩展对象 | 第一个显式参数 | 第二个显式参数 | 待确认 |

**重要**：占位符替换的是**已转换的 JavaScript 表达式**，不是原始标识符。

**`@#{n}` 替换的是参数表达式经过 AST 转换后的 JavaScript 代码**：

| 方法类型 | C# 签名 | 映射后 | @#{0} | @#{1} |
|---------|---------|--------|-------|-------|
| 实例方法 | `bool GetHashCode()` | `BooleanGetHashCode(bool instance)` | **实例表达式的 JS 代码** | - |
| 双参实例 | `bool Equals(object)` | `BooleanEquals(bool instance, object obj)` | 实例表达式 | obj 表达式 |
| 静态方法 | `static bool Parse(string)` | `BooleanParse(string value)` | value 表达式 | - |

**示例**：

```csharp
// C# 调用：a.GetHashCode()
// @#{0} 被替换为实例表达式 "a"
[Jazor(Op.Inline, "override bool.GetHashCode()", "@#{0} ? 1 : 0")]
public extern static Number _xxx(bool instance);
// 生成的 JS：a ? 1 : 0

// C# 调用：GetValue().Property.GetHashCode()
// @#{0} 被替换为实例表达式 "GetValue().Property"
// 生成的 JS：GetValue().Property ? 1 : 0
```

### 3.6 Op.Import - 模块导入

使用场景：
- 解析逻辑较复杂，需要完整的 JavaScript 实现
- JavaScript 没有直接对应的方法
- 需要额外的辅助函数
- **通过 C# 方法体提供实现，由 Jazor 编译器编译为 JavaScript**

```csharp
// 示例：Parse 方法需要完整实现
// 注意：Op.Import 不使用 extern，而是提供 C# 方法体
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

**Op.Import 的特点**：

- **不使用 `extern`**：与其他 Op 类型不同，Import 必须有 C# 方法体实现
- **编译时转换**：C# 代码会被 Jazor 编译器编译为 JavaScript 导出方法
- **白名单生成**：同时作为白名单来源，告诉编译器该方法可以被用户代码调用
- **双重作用**：既是实现定义，又是白名单配置

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

## 5. 成员命名格式规范（NameFormat）

成员名称使用 `Jazor.Name.Format.NameFormat` 进行统一格式化，在 Analyzer、Compiler 和 CLR 中保持一致。

**NameFormat 就是具体的规范格式**，无需额外转换。

### 5.1 方法签名

```
[修饰符] 返回类型.方法名(参数类型列表)

示例：
- static bool.Parse(string)
- override bool.ToString()
- bool.CompareTo(object)
- static bool.TryParse(string, out bool)
```

### 5.2 构造函数签名

```
类型.类型名(参数类型列表)

示例：
- bool.Boolean()
- bool.Boolean(bool)
```

### 5.3 属性签名

```
类型.get_属性名()
类型.set_属性名(参数类型)

示例：
- string.get_Length()
- string.set_Chars(int, char)
```

**映射规则**：
- 属性映射为 **2 个静态方法**（get 和 set）
- 实例属性的第一个参数是 **实例本身**
- get 方法：`PropertyGet(实例)`
- set 方法：`PropertySet(实例, 值)`

```csharp
// C# 属性：instance.Length
[Jazor(Op.Replace, "string.get_Length()", "length")]
public extern static int _xxx(string instance); // @#{0} = instance

// 生成的 JS：instance.length
```

### 5.4 字段签名

```
[修饰符] 类型.字段名

示例：
- static readonly bool.TrueString
- const int.MaxValue
```

**注意**：字段映射规则**待定**
- 原则上字段不映射
- 特殊字段（如 `int.MaxValue`）可能使用 `Op.Compile` 交给编译器处理

### 5.5 泛型签名

```
使用 ` 标记泛型参数数量：
- List`1.Add(T)
- Dictionary`2.get_Item(TKey)
```

**注意**：泛型类型在 C# 中约束，在 JavaScript 中**类型擦除**：
- `List<T>` → JS 数组 `Array`
- `Dictionary<K,V>` → JS `Map`
- 泛型参数 `T`, `K`, `V` 在 JS 运行时不可见

### 5.6 可空类型签名

```
可空类型在签名中使用 `?` 标记：
- static bool.Parse(string?)
- bool.TryParse(string?, out bool)
```

**注意**：可空类型在 JavaScript 中**保持可空性**，需要运行时检查：
- `string?` → JS `string | null | undefined`
- 实现中使用可选链 `?.` 处理可空值

### 5.7 规范要点

- **完整类型名**：包含命名空间（如 `System.Boolean.Parse`）
- **修饰符标记**：`static`, `override`, `virtual`, `abstract` 等
- **参数类型**：包含 `out`, `ref`, `in`, `params` 等修饰符
- **泛型表示**：使用 `` `n `` 表示泛型参数数量

## 6. 方法命名规范

### 6.1 哈希命名

模块内的方法使用哈希值命名（如 `_5dbf54319ebc8dfe`），避免命名冲突。

```csharp
// 方法名是成员签名的哈希值
[Jazor(Op.Import, "static bool.Parse(string)")]
public static bool _5dbf54319ebc8dfe(string? value) { ... }
```

### 6.2 命名规则

- 使用 `_` 前缀
- 16位十六进制哈希值
- 基于成员签名生成

### 6.3 参数命名建议（仅为可读性，不影响功能）

- 实例方法的第一个参数建议命名为 `instance`
- 静态方法的参数使用有意义的名称
- **实际占位符替换基于参数位置，与名称无关**

## 7. out/ref 参数处理

### 7.1 返回数组模式

C# 的 out 参数在 JavaScript 中通过返回数组模拟：

```csharp
// C# 方法签名
static bool TryParse(string, out bool result)

// JavaScript 返回值格式
[success, value]
```

**out 和 ref 参数的占位符与普通参数一样处理**，特殊处理发生在**调用处**。

**定义处**：

```csharp
[Jazor(Op.Import, "static bool.TryParse(string, out bool)")]
public extern static Array _xxx(string value, bool result);
// 注意：返回 Array 包含 [bool success, bool result]
// 参数 result 的占位符 @#{1} 与普通参数一样处理
```

**调用处处理**：

```csharp
// C# 调用代码
if (bool.TryParse(input, out bool result))
{
    Console.WriteLine(result);
}
```

**生成的 JavaScript**：

```javascript
// 编译器生成逗号表达式，从返回数组中解构
let _temp;
if ((_temp = TryParse(input, false))[0]) {
    let result = _temp[1];
    console.log(result);
}
```

**规则总结**：

| 方面 | 处理方式 |
|------|----------|
| **定义处占位符** | `@#{0}`=value, `@#{1}`=result，与普通参数相同 |
| **调用处生成** | 逗号表达式 `(_temp = method(args))[0]` |
| **out/ref 赋值** | 从返回数组中取出对应索引值赋给变量 |
| **多个 out/ref** | 按签名中的顺序，依次从数组中取值 |

### 7.2 使用示例

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

### 7.3 多个 out 参数

```csharp
// C# 方法签名
static bool TryFormat(Span<char>, out int charsWritten)

// JavaScript 返回值格式
[success, charsWritten] 或 [returnValue, outParam1, outParam2, ...]
```

## 8. 可空类型的处理

### 8.1 参数类型声明

在模块方法中使用可空类型（`string?`）保持与 C# 一致：

```csharp
// C# 方法签名：static bool Parse(string? value)
[Jazor(Op.Import, "static bool.Parse(string?)")]
public static bool _xxx(string? value)  // 使用 string? 保持语义
```

### 8.2 实现中使用可选链

```csharp
[Jazor(Op.Import, "static bool.Parse(string?)")]
public static bool _xxx(string? value)
{
    // 使用可选链处理可空值
    var str = value?.Trim()?.ToLower();
    // ...
}
```

### 8.3 签名中的可空标记

在 `[Jazor]` 签名中使用 `?` 标记可空类型：

```csharp
[Jazor(Op.Import, "static bool.TryParse(string?, out bool)")]
public static (bool, bool) _xxx(string? value, bool result = false)
```

## 9. 文档使用指南

### 9.1 目录结构说明

```
Jazor.CLR/
├── module/     # 模块实现代码（C# 编写，待完善）
├── doc/        # 参考文档（各类型映射的详细说明）
└── rule.md     # 本规则文档
```

### 9.2 使用建议

1. **实现新模块时**：
   - 优先参考本 rule.md 的通用规则
   - 参考 doc/ 目录下对应类型的详细说明
   - 不直接参考 module/ 目录（因为正在完善中）

2. **验证实现时**：
   - 检查入参和返回值类型映射是否正确
   - 检查 `[Jazor]` 特性参数配置是否合理
   - 检查 Op 类型选择是否合适

## 10. 类型模块示例

### 10.1 Boolean 类型

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

### 10.2 Object 类型

| C# 成员 | Op | 替换/内联值 | JavaScript 结果 |
|---------|-----|-------------|-----------------|
| `object.GetType()` | Inline | `typeof @#{0}` | `typeof obj` |
| `object.Object()` | Allowed | - | 无操作 |
| `virtual object.ToString()` | Replace | `toString` | `obj.toString()` |
| `virtual object.Equals(object)` | Inline | `(@#{0} === @#{1})` | `(a === b)` |
| `static object.Equals(object, object)` | Inline | `(@#{0} === @#{1})` | `(a === b)` |
| `static object.ReferenceEquals(object, object)` | Inline | `(@#{0} === @#{1})` | `(a === b)` |
| `virtual object.GetHashCode()` | Discard | - | 不支持 |

### 10.3 Console 类型

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

## 11. 设计原则

### 11.1 为什么 GetHashCode 被丢弃？

- JavaScript 没有统一的 `GetHashCode` 机制
- `Map` 和 `Set` 使用引用相等或值相等，不需要哈希码
- 布尔值的哈希码（0 或 1）在 JavaScript 中无实际用途

### 11.2 为什么 ToString 使用 Replace 而非 Inline？

- JavaScript 布尔值和对象原生支持 `toString()` 方法
- `true.toString()` 和 `false.toString()` 与 C# 语义一致
- 使用 `Replace` 可以直接调用原生方法，效率更高

### 11.3 为什么 Equals 使用 Inline？

- JavaScript 的严格相等 `===` 与 C# 的 `Equals` 语义完全一致
- 内联代码避免了额外的函数调用开销
- 对于引用类型，`===` 比较引用，与 `Object.Equals` 语义一致

### 11.4 Parse/TryParse 为什么使用 Import？

- 解析逻辑较复杂（空格处理、大小写处理、错误处理）
- 需要完整的 JavaScript 实现
- 使用 `Import` 保持代码清晰，便于维护

### 11.5 Console 的属性和方法为什么大多丢弃？

- JavaScript 运行在浏览器或 Node.js 环境
- 浏览器控制台没有输入功能（`ReadLine`, `ReadKey`）
- 浏览器控制台没有光标、窗口、缓冲区概念
- 只保留输出功能（`Write`, `WriteLine`, `Clear`）

## 12. 注释规范

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

## 13. 注意事项

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

## 14. 模块分类

### 14.1 基本类型模块

- `BooleanModule` - `bool` 类型映射
- `CharModule` - `char` 类型映射
- `StringModule` - `string` 类型映射
- `ObjectModule` - `object` 类型映射

### 14.2 数值类型模块

- `SByteModule`, `ByteModule` - 8位整数
- `Int16Module`, `UInt16Module` - 16位整数
- `Int32Module`, `UInt32Module` - 32位整数
- `Int64Module`, `UInt64Module` - 64位整数
- `SingleModule`, `DoubleModule` - 浮点数
- `DecimalModule` - 十进制数
- `BigIntegerModule` - 任意精度整数

### 14.3 日期时间模块

- `DateTimeModule` - `DateTime` 类型
- `DateTimeOffsetModule` - `DateTimeOffset` 类型
- `DateOnlyModule` - `DateOnly` 类型
- `TimeOnlyModule` - `TimeOnly` 类型
- `TimeSpanModule` - `TimeSpan` 类型

### 14.4 集合类型模块

- `ArrayModule` - `Array` 类型
- `ListModule` - `List<T>` 类型
- `DictionaryModule` - `Dictionary<K,V>` 类型
- `HashSetModule` - `HashSet<T>` 类型

### 14.5 其他模块

- `ConsoleModule` - `Console` 类型
- `ConvertModule` - `Convert` 类型
- `MathModule` - `Math` 类型
- `CultureInfoModule` - `CultureInfo` 类型

---

**文档维护者**：developerhan
**最后更新**：2026-02-25
**文档版本**：v5.0
