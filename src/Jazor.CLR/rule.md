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
5. [成员命名格式规范](#5-成员命名格式规范)
6. [方法命名规范](#6-方法命名规范)
7. [out/ref 参数处理](#7-outref-参数处理)
8. [可空类型的处理](#8-可空类型的处理)
9. [文档使用指南](#9-文档使用指南)
10. [类型模块示例](#10-类型模块示例)
11. [设计原则](#11-设计原则)
12. [注释规范](#12-注释规范)
13. [注意事项](#13-注意事项)
14. [模块分类](#14-模块分类)
15. [扩展内容](#15-扩展内容)

---

## 1. 特性体系说明

### 1.1 体系一：[ECMAScript] 系列（用户代码标记）

用于开发者标记自己的 C# 代码，告诉 Jazor 编译器如何转换为 JavaScript。

| 特性 | 命名空间 | 用途 |
|------|----------|------|
| `[ECMAScriptModule]` | `ECMAScript` | 标记类生成 ES module |
| `[ECMAScript]` | `ECMAScript` | 标记可被编译器识别的资源型类型 |
| `[ECMAScriptIgnore]` | `ECMAScript` | 标记被编译器忽略的成员 |
| `[ECMAScriptInline]` | `ECMAScript` | 标记方法直接使用内联代码（参数为完整函数代码） |

**编译器处理规则**：
Jazor 编译器只处理以下情况：
1. 必须是 `[ECMAScriptModule]` 标记的静态类
2. 有实现的方法（非 `extern`）
3. 带有 `[ECMAScriptInline]` 特性的 `extern` 方法（直接提取 inline rawFuncCode）

**使用场景**：开发者在自己的项目中使用

### 1.2 体系二：[Jazor]（Jazor.CLR 内部使用）

用于 `Jazor.CLR` 项目内部，实现双重功能：

1. **白名单来源**：`Jazor.Compiler.Generator` 读取 `[Jazor]` 特性生成白名单
2. **CLR Module 编译**：`Jazor.CLR` 项目本身被编译成 CLR module 库，供编译时引用
3. `[Jazor]` 特性构造方法：
   - 无参构造：`Op = Op.Compile`, `Member = string.Empty`, `Value = null`
   - 单字符串参数：`Op = Op.Inline`, `Member = string.Empty`, `Value = value`（value为内联代码）
   - 三参数：`(Op op, string member, string? value = null)` 完整配置

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

---

## 2. 模块声明规范

### 2.1 核心设计原则

**Jazor.CLR 将 BCL 类型扁平化为静态类静态方法**：

```
C# 实例方法 ──► 静态方法（实例作为第一个参数）
C# 静态方法 ──► 静态方法（保持原参数）
C# 属性 get ──► 静态方法（实例作为第一个参数）
C# 属性 set ──► 静态方法（实例作为第一个参数，值作为第二个参数）
```

### 2.2 基本结构

```csharp
[ECMAScriptModule]
[Jazor(Op.Import, "C#类型名", "模块路径")]
public static class XxxModule
{
    // 成员映射
}
```

**注意**：

- 模块路径标注在类上，表示内部方法引用的路径

### 2.3 模块声明示例

| C# 类型 | 模块声明 | 说明 |
|---------|----------|------|
| `bool` | `[Jazor(Op.Import, "bool", "System/BooleanModule.js")]` | 需要导入外部模块实现 |
| `object` | `[Jazor(Op.Import, "object", "System/ObjectModule.js")]` | 需要导入外部模块实现 |
| `Console` | `[Jazor(Op.Replace, "System.Console", "console")]` | 直接替换为 JavaScript 全局对象 |

### 2.4 模块路径规范

模块路径用于 `Op.Import` 类型，指定生成的 JavaScript 模块文件位置：

**路径格式**：

```csharp
[Jazor(Op.Import, "类型名", "模块路径")]
```

- **相对路径**：相对于编译输出目录
- **文件扩展名**：必须包含 `.js` 扩展名
- **目录分隔符**：使用 `/`（正斜杠），跨平台兼容

**命名空间映射规则**：

模块路径与 C# 命名空间保持一致：

| C# 命名空间 | 模块路径 |
|-------------|----------|
| `System` | `System/{类型名}Module.js` |
| `System.Collections.Generic` | `System/Collections/Generic/{类型名}Module.js` |
| `System.Linq` | `System/Linq/{类型名}Module.js` |

**文件命名规范**：

- **单类型模块**：`{类型名}Module.js`
  - 示例：`BooleanModule.js`, `StringModule.js`
- **嵌套类型**：使用 `+` 连接
  - 示例：`OuterClass+NestedClassModule.js`
- **泛型类型**：使用 `` `n `` 表示参数数量
  - 示例：`List`1Module.js`, `Dictionary`2Module.js`

**路径映射示例**：

| C# 类型 | 命名空间 | 模块路径 |
|---------|----------|----------|
| `bool` | (全局) | `System/BooleanModule.js` |
| `List<T>` | `System.Collections.Generic` | `System/Collections/Generic/ListModule.js` |
| `Dictionary<K,V>` | `System.Collections.Generic` | `System/Collections/Generic/DictionaryModule.js` |
| `Console` | `System` | `System/ConsoleModule.js` |

**特殊约定**：
- 路径分隔符统一使用 `/`（兼容所有平台）
- 扩展名必须是 `.js`
- 路径不包含版本号或文化信息

---

## 3. Op 枚举说明

### 3.1 `extern` 关键字含义

**`extern` 表示该方法不需要实现或没有实现。**

**`extern + Op` 组合的具体含义**：

| 组合 | 含义 | 实际行为 |
|------|------|---------|
| `extern + Allowed` | JS 自有，无需处理 | JS 原生支持该语义，无需映射代码 |
| `extern + Replace` | JS 有同样语义的方法，但名称不同 | 调用 JS 原生方法，只需替换名称 |
| `extern + Inline` | 直接内联代码片段 | 替换为指定的内联表达式 |
| `extern + Discard` | 不支持 | 该成员在 JS 中无对应概念 |
| `extern + Compile` | 编译器特殊处理 | 由编译器根据上下文生成代码 |

**注意**：
- `extern` 是 C# 语法的一部分，表示**外部实现**
- 在 `Jazor.CLR` 中，它明确表示**该代码不会作为 C# 运行**
- **`Discard`、`Allowed`、`Replace`、`Inline`、`Compile` 五种 Op 类型在方法上都有`extern`**
- **`Op.Import` 不使用 `extern`**，而是提供 C# 方法体，会被 Jazor 编译器编译为 JavaScript 导出方法

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

// 示例：Console.WriteLine 替换为 console.log（带参数版本）
[Jazor(Op.Replace, "static System.Console.WriteLine(string)", "log")]
public extern static void _19f2583beee4f7fb(string value);

// 示例：Console.WriteLine 无参版本
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
| 扩展方法 | 被扩展对象 | 第一个显式参数 | 第二个显式参数 | 与实例方法一致 |

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
public extern static Number _80b6c29cc0038969(bool instance);
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
// result 当作常规参数传入，因为可能会在代码中被使用
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

### 3.7 Op.Compile - 编译器特殊处理

使用场景：
- 编译器有内置的特殊处理逻辑
- 静态常量字段（如 `int.MaxValue`, `int.MinValue`）
- 需要编译器根据上下文生成不同代码的场景
- `[Jazor]` 无参构造函数就是 `Op.Compile`
- **编译时常量表达式** - 编译器直接内联常量值
- **特殊语法转换** - 需要编译器识别并生成特定代码模式

```csharp
// 示例：静态常量字段由编译器直接处理
[Jazor]
public extern static int _xxx();

// 示例：int.MaxValue 映射为 JavaScript 的 Number.MAX_SAFE_INTEGER
[Jazor(Op.Compile, "static int.MaxValue")]
public extern static int _intMaxValue();
// 生成的 JS：Number.MAX_SAFE_INTEGER

// 示例：int.MinValue 映射为 JavaScript 的 Number.MIN_SAFE_INTEGER
[Jazor(Op.Compile, "static int.MinValue")]
public extern static int _intMinValue();
// 生成的 JS：Number.MIN_SAFE_INTEGER
```

**Op.Compile 的典型使用场景**：

| 场景 | 说明 | 示例 |
|------|------|------|
| 编译期常量 | 由编译器直接替换为常量值 | `int.MaxValue` → `Number.MAX_SAFE_INTEGER` |
| 内置操作符 | 编译器识别并生成对应操作符 | `+` `-` `*` `/` 等 |
| 特殊类型转换 | 需要编译器处理类型转换逻辑 | 枚举转换、可空类型展开 |

### 3.8 Op 选择决策树

当决定使用哪个 Op 时，参考以下决策流程：

```
是否需要 JavaScript 实现？
├── 否 → JavaScript 原生支持？
│   ├── 是，名称相同 → Op.Allowed
│   ├── 是，名称不同 → Op.Replace
│   └── 否，概念不存在 → Op.Discard
├── 是，简单表达式 → Op.Inline
├── 是，复杂逻辑 → Op.Import
└── 编译器特殊处理 → Op.Compile
```

**决策示例**：

| 成员 | 决策路径 | 结果 |
|------|----------|------|
| `object.Object()` | 不需要实现 + JS 原生支持 | `Op.Allowed` |
| `bool.ToString()` | 不需要实现 + 名称不同 | `Op.Replace` → `toString` |
| `bool.Equals()` | 不需要实现 + 简单表达式 | `Op.Inline` → `(@#{0} === @#{1})` |
| `bool.Parse()` | 需要实现 + 复杂逻辑 | `Op.Import` |
| `int.MaxValue` | 编译器特殊处理 | `Op.Compile` |
| `GetHashCode()` | 概念不存在 | `Op.Discard` |

- **编译器内置**：具体行为由编译器代码决定
- **放置在类上**：等同于 `Allowed`
- **放置在属性/方法上**：触发编译器特殊处理逻辑
- **常量优化**：编译时常量会被直接内联到调用处

**Op.Compile 的典型使用场景**：

| 场景 | 说明 | 示例 |
|------|------|------|
| 编译期常量 | 由编译器直接替换为常量值 | `int.MaxValue` → `Number.MAX_SAFE_INTEGER` |
| 内置操作符 | 编译器识别并生成对应操作符 | `+` `-` `*` `/` 等 |
| 特殊类型转换 | 需要编译器处理类型转换逻辑 | 枚举转换、可空类型展开 |

**与 Op.Inline 的区别**：

| 特性 | Op.Inline | Op.Compile |
|------|-----------|------------|
| 处理时机 | 代码生成阶段 | 编译器内部处理 |
| 灵活性 | 固定代码模板 | 可动态生成代码 |
| 占位符 | 支持 `@#{n}` | 由编译器决定 |
| 适用场景 | 简单表达式替换 | 复杂逻辑、常量优化 |
| 特殊类型转换 | 需要编译器特殊处理的类型 | `object` 默认构造函数 |

### 3.8 Op 类型选择决策树

```
选择 Op 类型的决策流程：

1. JavaScript 是否有直接等价物？
   ├── 否 → 检查是否属于以下情况：
   │         ├── 无对应概念 → Op.Discard
   │         └── 需要复杂实现 → Op.Import
   └── 是 → 检查等价方式：
             ├── 同名同行为 → Op.Allowed
             ├── 名不同但行为同 → Op.Replace
             ├── 可用操作符/表达式内联 → Op.Inline
             └── 需要编译器特殊处理 → Op.Compile
```

---

## 4. 类型映射表

本节详细说明 C# 类型与 JavaScript 类型的映射关系。

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
| `ReadOnlySpan<char>` | `string` | 无 |
| `Span<char>` | `string` | 无 |
| `System.Type` | `object` | JavaScript 无类型系统 |
| `System.TypeCode` | - | 丢弃 |

### 4.4 异常类型映射

C# 异常类型映射为 JavaScript 的 `Error` 类及其子类。

#### 4.4.1 异常类型对照表

| C# 异常类型 | JavaScript 类型 | 说明 |
|-------------|-----------------|------|
| `System.Exception` | `Error` | 基类映射 |
| `System.SystemException` | `Error` | 系统异常基类 |
| `System.ArgumentException` | `Error` | 参数错误，通常使用特定消息 |
| `System.ArgumentNullException` | `TypeError` | 更具体的错误类型 |
| `System.ArgumentOutOfRangeException` | `RangeError` | 范围错误 |
| `System.InvalidOperationException` | `Error` | 运行时状态错误 |
| `System.NullReferenceException` | 自动处理 | 使用可选链 `?.` 避免 |
| `System.FormatException` | `Error` | 格式错误，消息包含详细信息 |
| `System.NotImplementedException` | `Error` | 未实现功能 |
| `System.NotSupportedException` | `Error` | 不支持的操作 |
| `System.IndexOutOfRangeException` | `RangeError` | 数组/列表索引越界 |
| `System.DivideByZeroException` | `Error` 或 `Infinity` | JavaScript 返回 Infinity |
| `System.OverflowException` | `Error` | 数值溢出 |

#### 4.4.2 异常抛出示例

```csharp
// C# 实现中使用 throw new Error()
[Jazor(Op.Import, "static bool.Parse(string)")]
public static bool _xxx(string? value)
{
    var str = value?.Trim()?.ToLower();
    if (str == "true")
        return true;
    else if (str == "false")
        return false;
    else
        throw new Error($"FormatException: String '{value}' was not recognized as a valid Boolean.");
}
```

**生成的 JavaScript**：

```javascript
export function _xxx(value) {
    const str = value?.trim()?.toLowerCase();
    if (str === "true")
        return true;
    else if (str === "false")
        return false;
    else
        throw new Error(`FormatException: String '${value}' was not recognized as a valid Boolean.`);
}
```

#### 4.4.3 异常处理注意事项

1. **自定义异常**：C# 自定义异常类映射为带有特定消息的 `Error`
2. **异常链**：JavaScript 不支持异常链（`InnerException`），需要手动拼接消息
3. **finally 块**：直接映射为 JavaScript 的 `finally` 块
4. **异常类型判断**：使用 `instanceof Error` 进行基础判断，具体类型通过消息内容识别

---

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

### 5.4 索引器签名

索引器（Indexer）的映射规则与属性完全一致，映射为 `get_Item` 和 `set_Item` 静态方法。

```
类型.get_Item(索引类型)
类型.set_Item(索引类型, 值类型)

示例：
- List`1.get_Item(int)
- List`1.set_Item(int, T)
- Dictionary`2.get_Item(TKey)
- Dictionary`2.set_Item(TKey, TValue)
```

**映射规则**：
- 索引器映射为 **2 个静态方法**（get_Item 和 set_Item）
- 实例索引器的第一个参数是 **实例本身**
- 第二个参数是 **索引**
- set 方法第三个参数是 **值**
- 使用 `@#{1}` 作为索引占位符

```csharp
// C# 索引器：list[0]
[Jazor(Op.Replace, "List`1.get_Item(int)", "@#{0}[@#{1}]")]
public extern static T _xxx(List<T> instance, int index);

// 生成的 JS：list[0]

// C# 索引器：dict[key] = value
[Jazor(Op.Replace, "Dictionary`2.set_Item(TKey, TValue)", "(@#{0}[@#{1}] = @#{2})")]
public extern static void _xxx(Dictionary<K, V> instance, K key, V value);

// 生成的 JS：dict[key] = value
```

### 5.5 字段签名

```
[修饰符] 类型.字段名

示例：
- static readonly bool.TrueString
- const int.MaxValue
```

**字段映射规则**：

| Op 类型 | 使用场景 | 示例 |
|---------|----------|------|
| `Inline` | 静态常量可直接内联为字面量 | `bool.TrueString` → `"true"` |
| `Compile` | 需要编译器特殊处理的常量 | `int.MaxValue` → `Number.MAX_SAFE_INTEGER` |

**注意**：
- 原则上实例字段不映射（应通过属性访问）
- 静态常量字段使用 `Op.Inline` 直接内联或 `Op.Compile` 交由编译器处理
- `readonly` 字段若在构造函数外初始化，可能需要 `Op.Import` 提供访问方法

### 5.6 泛型签名

```
使用 ` 标记泛型参数数量：
- List`1.Add(T)
- Dictionary`2.get_Item(TKey)
```

**注意**：泛型类型在 C# 中约束，在 JavaScript 中**类型擦除**：
- `List<T>` → JS 数组 `Array`
- `Dictionary<K,V>` → JS `Map`
- 泛型参数 `T`, `K`, `V` 在 JS 运行时不可见

### 5.7 可空类型签名

```
可空类型在签名中使用 `?` 标记：
- static bool.Parse(string?)
- bool.TryParse(string?, out bool)
```

**注意**：可空类型在 JavaScript 中**保持可空性**，需要运行时检查：
- `string?` → JS `string | null | undefined`
- 实现中使用可选链 `?.` 处理可空值

### 5.8 规范要点

- **完整类型名**：包含命名空间（如 `System.Boolean.Parse`）
- **修饰符标记**：`static`, `override`, `virtual`, `abstract` 等
- **参数类型**：包含 `out`, `ref`, `in`, `params` 等修饰符
- **泛型表示**：使用 `` `n `` 表示泛型参数数量

### 5.9 泛型方法签名

泛型方法的签名格式与泛型类型类似，使用 `` `n `` 标记泛型参数数量。

#### 5.9.1 签名格式

```
类型.`n.方法名[T](参数类型列表)

示例：
- System.Linq.Enumerable.`1.Where[T](System.Collections.Generic.IEnumerable{T}, System.Func{T,bool})
- System.Linq.Enumerable.`1.Select[T,TResult](System.Collections.Generic.IEnumerable{T}, System.Func{T,TResult})
```

#### 5.9.2 类型参数映射

| C# 泛型参数 | 签名表示 | 说明 |
|-------------|----------|------|
| `T` | `{T}` | 单类型参数 |
| `T, TResult` | `{T,TResult}` | 多类型参数用逗号分隔 |
| `where T : class` | 无 | 约束不体现在签名中 |

#### 5.9.3 示例

```csharp
// C# 泛型方法
public static IEnumerable<TResult> Select<TSource, TResult>(
    this IEnumerable<TSource> source,
    Func<TSource, TResult> selector)

// Jazor 签名
[Jazor(Op.Import, "System.Linq.Enumerable.`2.Select[TSource,TResult](System.Collections.Generic.IEnumerable{TSource}, System.Func{TSource,TResult})")]
public static Array _xxx(Array source, Function selector) { ... }
```

#### 5.9.4 注意事项

- 泛型方法在 JavaScript 中**类型擦除**，类型参数 `T` 在运行时不存在
- 类型约束（`where` 子句）仅在 C# 编译时检查，不体现在签名中
- 泛型方法的类型推断由 C# 编译器处理，Jazor 直接使用推断后的类型

---

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

### 6.3 哈希生成算法

方法名哈希基于**完整签名**生成，确保唯一性和稳定性。

#### 6.3.1 算法步骤

1. **构建完整签名字符串**：
   ```
   格式: {命名空间}.{类型名}.{成员签名}
   示例: System.Boolean.static bool.Parse(string)
   ```

2. **使用 SHA256 计算哈希**：
   ```csharp
   using var sha256 = SHA256.Create();
   var hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(signature));
   ```

3. **提取前 8 字节（16 位十六进制）**：
   ```csharp
   var sb = new StringBuilder("_");
   for (int i = 0; i < 8; i++)
       sb.Append(hashBytes[i].ToString("x2"));
   return sb.ToString();  // 示例: _5dbf54319ebc8dfe
   ```

#### 6.3.2 命名冲突处理

由于使用 16 位十六进制（64 位），碰撞概率极低（约 1/2^64）。如发生碰撞，可：
- 添加序号后缀（如 `_5dbf54319ebc8dfe_1`）
- 或使用更长的哈希值

#### 6.3.3 示例

| 成员签名 | 生成的哈希名 |
|----------|-------------|
| `static bool.Parse(string)` | `_5dbf54319ebc8dfe` |
| `override bool.ToString()` | `_d48c2d39317daf8f` |
| `static bool.TryParse(string, out bool)` | `_dada4bbdacd7aa19` |

#### 6.3.4 工具方法

```csharp
/// <summary>
/// 生成成员签名的哈希方法名
/// </summary>
public static string GenerateHashName(string signature)
{
    using var sha256 = SHA256.Create();
    var bytes = Encoding.UTF8.GetBytes(signature);
    var hash = sha256.ComputeHash(bytes);

    var sb = new StringBuilder("_");
    for (int i = 0; i < 8; i++)
        sb.Append(hash[i].ToString("x2"));

    return sb.ToString();
}
```

---

## 7. out/ref 参数处理

> **重要说明**：虽然 C# 中 `out` 和 `ref` 有明确的语义区别（`out` 必须赋值，`ref` 必须初始化），但 **Jazor 使用同种方式处理**它们。两者都通过返回数组模拟，在调用处统一使用逗号表达式解构。

### 7.1 返回数组模式

C# 的 out/ref 参数在 JavaScript 中通过返回数组模拟：

```csharp
// C# 方法签名
static bool TryParse(string, out bool result)
static void Modify(ref int value)

// JavaScript 返回值格式（两者相同）
[returnValue, refOutValue]
```

**out 和 ref 参数的占位符与普通参数一样处理**，特殊处理发生在**调用处**。

### 7.2 Jazor 中的统一处理方式

在 Jazor 中，`ref` 和 `out` 的处理方式**完全相同**：

| 特性 | C# 语义 | Jazor 处理方式 |
|------|---------|---------------|
| `out` | 必须赋值，不需要初始化 | 返回数组，调用处解构 |
| `ref` | 必须初始化，可以修改 | 返回数组，调用处解构 |

**原因**：
- JavaScript 没有引用传递的概念，无法区分 `ref` 和 `out`
- 两者都通过返回数组 + 调用处解构来实现参数传递
- 编译器不检查 `out` 是否赋值或 `ref` 是否初始化（由 C# 编译器在编译时保证）

### 7.3 定义处

```csharp
// out 参数示例
[Jazor(Op.Import, "static bool.TryParse(string, out bool)")]
public static Array<object?> _dada4bbdacd7aa19(string? value, bool result)
{
    // 返回 [success, result]
    return [true, parsedValue];
}

// ref 参数示例
[Jazor(Op.Import, "static void Increment(ref int)")]
public static Array<object?> _xxx(int value)
{
    // 返回 [void, incrementedValue]
    return [null, value + 1];
}
```

**注意**：
- 参数占位符 `@#{0}`、`@#{1}` 等对 `ref` 和 `out` 一视同仁
- 返回值统一使用 `Array<object?>`
- 数组第一个元素是方法返回值（void 时为 `null`），后续是 ref/out 参数值

### 7.4 调用处处理

```csharp
// C# 代码 - out 参数
var result = false;
if (bool.TryParse(input, out result))
{
    Console.WriteLine(result);
}

// C# 代码 - ref 参数
var counter = 0;
Increment(ref counter);
```

**生成的 JavaScript（两者结构相同）**：

```javascript
// out 参数
let result = false;
let $0;
if (($0 = TryParse(input, result), result = $0[1], $0[0])) {
    console.log(result);
}

// ref 参数
let counter = 0;
let $1;
$1 = Increment(counter), counter = $1[1];
```

**规则总结**：

| 方面 | 处理方式 |
|------|----------|
| **定义处占位符** | `@#{n}` 与普通参数相同，不区分 ref/out |
| **调用处生成** | 逗号表达式 `(_temp = method(args))[index]` |
| **参数赋值** | 从返回数组中按顺序取出值赋给变量 |
| **多个 ref/out** | 按签名中的顺序，依次从数组中取值（索引 1, 2, 3...）|
| **ref vs out** | Jazor 中**无区别**，统一处理 |

---

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
public static Array<object?> _xxx(string? value, bool result = false)
```

---

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

### 9.3 快速决策流程

```
开始实现新方法
       │
       ▼
  JavaScript 是否有
  原生对应方法？
   │         │
   是        否
   │         │
   ▼         ▼
方法名相同？  能否用简单
   │         表达式实现？
   │      │        │
   是     是       否
   │      │        │
   ▼      ▼        ▼
 Allowed Inline  Import
   │      │        │
   ▼      ▼        ▼
 完成    完成    完成
```

### 9.4 常见陷阱与解决方案

| 问题 | 原因 | 解决方案 |
|------|------|----------|
| 方法未被编译 | 使用了 `extern` 但 Op 不是 Import | `Import` 类型**不能**使用 `extern` |
| 白名单未生成 | 特性拼写错误 | 检查 `[Jazor]` 而非 `[JazorModule]` 等 |
| 占位符未替换 | `@#{n}` 格式错误 | 使用正确的占位符格式 `@#{0}`, `@#{1}` |
| 返回值类型错误 | 使用了具体泛型而非 `Array<object?>` | out/ref 参数方法必须返回 `Array<object?>` |
| 循环依赖 | 模块间相互引用 | 使用 `Op.Import` 并通过模块路径解耦 |

---

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

---

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

---

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

---

## 13. 注意事项

### 13.1 核心注意事项

1. **out/ref 参数处理**：C# 的 `out` 和 `ref` 参数在 Jazor 中**使用同种方式处理**，都通过返回数组模拟，格式为 `[returnValue, outParam1, outParam2, ...]`。虽然 C# 中两者有区别（`out` 必须赋值，`ref` 必须初始化），但 JavaScript 没有引用传递概念，因此 Jazor 统一使用返回数组 + 调用处解构的方式处理。

2. **ReadOnlySpan<char> 处理**：映射为 `string`

3. **方法命名**：模块内方法使用哈希值命名（如 `_5dbf54319ebc8dfe`），避免命名冲突。哈希基于完整签名生成，使用 SHA256 前 16 位。

4. **可空处理**：`string?` 参数需要使用可选链操作符 `?.` 处理空值

5. **类型系统差异**：
   - C# `GetType()` 返回 `Type` 对象
   - JavaScript `typeof` 返回类型字符串（`"object"`, `"string"`, `"number"` 等）

6. **Console.Write 行为差异**：
   - C# `Write` 不换行，`WriteLine` 换行
   - JavaScript `console.log` 总是换行
   - 两者语义不完全一致，但可接受

### 13.2 开发实践注意事项

7. **嵌套类型的模块路径**：嵌套类型（如 `OuterClass.InnerClass`）的模块路径使用 `+` 连接，例如 `OuterClass+InnerClassModule.js`

8. **泛型类型的模块路径**：泛型类型使用 `` `n `` 标记参数数量，例如 `List`1Module.js`

9. **方法重载处理**：JavaScript 不支持方法重载，因此不同重载在模块中必须有不同的哈希名

10. **partial 类支持**：C# 的 `partial` 类在 Jazor.CLR 模块中不支持，每个模块必须是完整的静态类

11. **循环引用处理**：避免在 `Op.Import` 方法实现中调用其他 `Op.Import` 方法，可能导致循环依赖问题

12. **字符串转义**：`Op.Inline` 中的内联代码如果包含引号，需要进行转义

---

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

## 15. 扩展内容

### 15.1 异步方法映射

C# 的 `async/await` 模式映射到 JavaScript 的 `Promise`：

| C# 特性 | JavaScript 对应 | 说明 |
|---------|-----------------|------|
| `async Task` | `async function` / `Promise` | 异步方法 |
| `async Task<T>` | `async function` 返回 `Promise<T>` | 带返回值的异步方法 |
| `await` | `await` | 直接映射 |
| `Task.WhenAll` | `Promise.all` | 并行等待 |
| `Task.WhenAny` | `Promise.race` | 竞争等待 |

**示例**：

```csharp
// C#
public static async Task<string> FetchDataAsync(string url)
{
    return await httpClient.GetStringAsync(url);
}
```

```javascript
// JavaScript
async function FetchDataAsync(url) {
    return await httpClient.getStringAsync(url);
}
```

### 15.2 枚举类型映射

C# 枚举映射为 JavaScript 对象常量：

```csharp
// C#
public enum DayOfWeek
{
    Sunday = 0,
    Monday = 1,
    Tuesday = 2,
    Wednesday = 3,
    Thursday = 4,
    Friday = 5,
    Saturday = 6
}
```

```javascript
// JavaScript
const DayOfWeek = {
    Sunday: 0,
    Monday: 1,
    Tuesday: 2,
    Wednesday: 3,
    Thursday: 4,
    Friday: 5,
    Saturday: 6
};
```

**枚举操作映射**：

| C# 操作 | JavaScript 结果 |
|---------|-----------------|
| `DayOfWeek.Monday` | `DayOfWeek.Monday` (值为 1) |
| `(DayOfWeek)1` | `1` (直接使用数值) |
| `value == DayOfWeek.Monday` | `value === DayOfWeek.Monday` |
| `Enum.GetName(typeof(DayOfWeek), 1)` | 需要辅助函数 |

### 15.3 数组处理

| C# 数组操作 | JavaScript 对应 | 说明 |
|-------------|-----------------|------|
| `new T[n]` | `new Array(n)` 或 `[...Array(n)]` | 固定大小数组 |
| `new T[] { a, b, c }` | `[a, b, c]` | 数组初始化器 |
| `new T[n, m]` | 嵌套数组 `Array(n).fill().map(() => Array(m))` | 多维数组 |
| `T[]` | `Array` | 一维数组 |
| `T[][]` | `Array<Array>` | 交错数组 |

### 15.4 泛型约束处理

泛型约束在 JavaScript 中**类型擦除**，仅保留编译时检查：

| C# 泛型约束 | JavaScript 处理 |
|-------------|-----------------|
| `where T : class` | 编译时检查，运行时忽略 |
| `where T : struct` | 编译时检查，运行时忽略 |
| `where T : new()` | 编译时检查，调用时创建对象 |
| `where T : BaseClass` | 编译时检查，运行时忽略 |
| `where T : ISomeInterface` | 编译时检查，运行时忽略 |

**注意**：运行时泛型参数 `T` 在 JavaScript 中被擦除，无法进行类型判断。

### 15.5 返回类型规范

对于包含 `out`/`ref` 参数的方法，返回值应统一使用 `Array<object?>` 类型：

```csharp
// 正确 ✅
[Jazor(Op.Import, "static bool.TryParse(string?, out bool)")]
public static Array<object?> _xxx(string? value, bool result)
{
    // 返回 [success, result]
    return [true, parsedValue];
}

// 错误 ❌ - 不要使用泛型 Array<T>
public static Array<bool> _xxx(string? value, bool result);

// 错误 ❌ - 不要使用元组语法
public static (bool, bool) _xxx(string? value, bool result = false);
```

**规范**：

- **必须使用 `Array<object?>` 作为返回类型**
- 返回数组元素顺序：`[返回值, out参数1, out参数2, ...]`
- `TryParse` 模式：`[success, parsedValue]`
- `ref` 参数与 `out` 参数处理方式相同

**原因**：

- JavaScript 数组是类型擦除的，无法在类型层面约束元素类型
- `Array<object?>` 可以容纳任意类型的返回值和参数
- 编译器会在调用处根据签名进行正确的类型转换

### 15.6 不支持的特性

以下 C# 特性在 JavaScript 中无对应概念，应使用 `Op.Discard`：

| 特性 | 原因 |
|------|------|
| 事件 (`event`) | JavaScript 使用回调/订阅模式，无多播事件 |
| 委托 (`delegate`) | JavaScript 只有函数引用 |
| `unsafe` 代码 | JavaScript 是安全语言，无指针操作 |
| `sizeof` | JavaScript 无固定内存布局 |
| `stackalloc` | JavaScript 无栈分配 |
| `ref struct` | JavaScript 无栈内存概念 |

---

## 16. 快速参考表

### 16.1 Op 类型速查表

| 场景 | Op 类型 | extern? | 示例 |
|------|---------|---------|------|
| JS 原生支持，无需处理 | `Allowed` | ✅ 是 | `object.Object()` |
| JS 有同名方法 | `Allowed` | ✅ 是 | `string.ToString()` → `toString()` |
| JS 有类似方法但名称不同 | `Replace` | ✅ 是 | `ToString()` → `toString` |
| 可用简单表达式实现 | `Inline` | ✅ 是 | `(@#{0} === @#{1})` |
| 需要完整实现 | `Import` | ❌ 否 | `bool.Parse(string)` |
| 编译器特殊处理 | `Compile` | ✅ 是 | `int.MaxValue` |
| 不支持 | `Discard` | ✅ 是 | `GetHashCode()` |

### 16.2 占位符速查表

| 方法类型 | @#{0} | @#{1} | @#{2} |
|----------|-------|-------|-------|
| 实例方法 | 实例 | 参数1 | 参数2 |
| 静态方法 | 参数1 | 参数2 | 参数3 |
| 扩展方法 | 被扩展对象 | 参数1 | 参数2 |

### 16.3 类型映射速查表

| C# 类型 | JS 类型 | 类型检查方式 |
|---------|---------|-------------|
| `bool` | `boolean` | `typeof x === "boolean"` |
| `string` | `string` | `typeof x === "string"` |
| `int`, `double` | `number` | `typeof x === "number"` |
| `long`, `BigInteger` | `bigint` | `typeof x === "bigint"` |
| `DateTime` | `Date` | `x instanceof Date` |
| `Array`, `List<T>` | `Array` | `Array.isArray(x)` |
| `object` | `object` | `typeof x === "object"` |
| 自定义 class | class | `x instanceof ClassName` |

### 16.4 常见签名模式

```csharp
// 实例方法
[Jazor(Op.Replace, "Type.MethodName(ParamType)", "jsMethodName")]
public extern static ReturnType _hash(Type instance, ParamType param);

// 静态方法
[Jazor(Op.Replace, "static Type.MethodName(ParamType)", "jsMethodName")]
public extern static ReturnType _hash(ParamType param);

// 属性 get
[Jazor(Op.Replace, "Type.get_PropertyName()", "jsPropertyName")]
public extern static PropertyType _hash(Type instance);

// 属性 set
[Jazor(Op.Replace, "Type.set_PropertyName(PropertyType)", "jsPropertyName")]
public extern static void _hash(Type instance, PropertyType value);

// 带 out 参数
[Jazor(Op.Import, "static Type.TryParse(string, out Type)")]
public static Array<object?> _hash(string? value, Type result)
{
    // 返回 [success, parsedValue]
    return [true, parsed];
}
```

---

## 17. 附录

### 17.1 术语表

| 术语 | 说明 |
|------|------|
| BCL | Base Class Library，.NET 基础类库 |
| CLR | Common Language Runtime，公共语言运行时 |
| ESTree | ECMAScript AST 标准 |
| TypeMapper | Jazor 内部类型映射枚举 |
| 类型擦除 | 泛型类型在运行时失去类型参数信息 |
| TDZ | Temporal Dead Zone，暂时性死区 |

### 17.2 相关文档

- [CLAUDE.md](../../CLAUDE.md) - 项目整体架构和转换思想
- [Jazor.Name/rule.md](../Jazor.Name/rule.md) - 命名规范详细说明
- `doc/` 目录 - 各类型模块详细文档

### 17.3 版本历史

| 版本 | 日期 | 变更说明 |
|------|------|----------|
| v3.1 | 2026-02-26 | 完善 Op.Compile 说明，添加快速参考表 |
| v3.0 | 2026-01-23 | 重构文档结构，完善类型映射规则 |
| v2.0 | 2025-12-15 | 添加泛型方法和异步方法映射 |
| v1.0 | 2025-11-01 | 初始版本 |

---

**文档维护者**：developerhan
**最后更新**：2026-02-26
**文档版本**：v3.2
