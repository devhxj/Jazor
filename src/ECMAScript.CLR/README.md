# ECMAScript.CLR

Jazor 编译器的 CLR 运行时支持库，使用 C# 编写（但语法贴合 JavaScript）来实现 C# 类型对应的 ES6 module。

## 项目概述

ECMAScript.CLR 是 Jazor 项目的重要组成部分，负责提供：

1. **白名单定义**：声明哪些 C# 类型和成员可以被编译器使用
2. **JavaScript 运行时实现**：为白名单中的类型提供 JavaScript 等价实现
3. **类型映射**：将 C# 类型映射到 JavaScript 类型

### 设计目的

- 将 C# 类型的属性和方法统一转换成可导出的方法
- 提供类型成员的 JavaScript 运行时实现
- 通过白名单机制与 Analyzer 和 Compiler 协同工作

---

## 项目结构

```
ECMAScript.CLR/
├── 核心基础模块（已启用）
│   ├── CLRModule.cs              # CLR 运行时支持类型
│   ├── AssignModule.cs           # 基础类型白名单
│   ├── BooleanModule.cs          # bool 类型实现
│   ├── StringModule.cs           # string 类型实现
│   ├── ObjectModule.cs           # object 类型实现
│   ├── BigIntegerModule.cs       # BigInteger 类型实现
│   └── ValueTupleModule.cs       # ValueTuple 和 Tuple 实现
│
├── 数值类型模块（已禁用）
│   ├── SByteModule.cs            # sbyte (System.SByte)
│   ├── ByteModule.cs             # byte (System.Byte)
│   ├── Int16Module.cs            # short (System.Int16)
│   ├── UInt16Module.cs           # ushort (System.UInt16)
│   ├── Int32Module.cs            # int (System.Int32)
│   ├── UInt32Module.cs           # uint (System.UInt32)
│   ├── Int64Module.cs            # long (System.Int64)
│   ├── UInt64Module.cs           # ulong (System.UInt64)
│   ├── SingleModule.cs           # float (System.Single)
│   ├── DoubleModule.cs           # double (System.Double)
│   ├── DecimalModule.cs          # decimal (System.Decimal)
│   └── CharModule.cs             # char (System.Char)
│
├── 日期时间模块（已禁用）
│   ├── DateTimeModule.cs         # DateTime
│   ├── DateOnlyModule.cs         # DateOnly
│   ├── TimeOnlyModule.cs         # TimeOnly
│   ├── DateTimeOffsetModule.cs   # DateTimeOffset
│   ├── TimeSpanModule.cs         # TimeSpan
│   ├── GregorianCalendarModule.cs # GregorianCalendar
│   └── CultureInfoModule.cs      # CultureInfo
│
├── 集合类型模块（已禁用）
│   ├── ListModule.cs             # List<T>
│   ├── DictionaryModule.cs       # Dictionary<K,V>
│   ├── HashSetModule.cs          # HashSet<T>
│   ├── ReadOnlyCollectionModule.cs
│   ├── ReadOnlyDictionaryModule.cs
│   ├── ReadOnlySetModule.cs
│   └── ConditionalWeakTableModule.cs
│
└── 其他工具模块（已禁用）
    ├── StringBuilderModule.cs    # StringBuilder
    ├── NullableModule.cs         # Nullable<T>
    ├── WeakReferenceModule.cs    # WeakReference<T>
    └── ExceptionModule.cs        # Exception
```

---

## 白名单机制

### 核心特性

ECMAScript.CLR 使用三个自定义特性来定义白名单和 JavaScript 实现：

#### 1. `[ECMAScriptModule]`

标记类为可导出的 ES6 module。

```csharp
[ECMAScriptModule]
public static class BooleanModule
{
    // ...
}
```

**参数**：
- `Import`（可选）：指定导入路径

#### 2. `[WhiteList]`

声明类型或成员的白名单映射名称。

```csharp
[WhiteList("bool")]                           // 类型白名单
[WhiteList("override bool.GetHashCode()")]    // 成员白名单
[WhiteList("static bool.Parse(string)")]      // 方法白名单
```

**命名规则**：
- 类型白名单：使用完整类型名（如 `bool`, `string`, `System.Numerics.BigInteger`）
- 成员白名单：使用 `签名` 格式（如 `override bool.GetHashCode()`, `static bool.Parse(string)`）

#### 3. `[ECMAScriptLiteral]`

直接嵌入 JavaScript 代码片段。

```csharp
[ECMAScriptLiteral("@#{0} ? 1 : 0")]
public extern static Number BooleanGetHashCode(bool instance);
```

**占位符语法**：
- `@#{0}`, `@#{1}`, ... : 表示方法参数的位置替换
- 示例：`[ECMAScriptLiteral("@#{0} + @#{1}")]` → 生成 `arg0 + arg1`

---

## 实现模式

### 模式 1：直接使用 ECMAScriptLiteral

适用于简单的 JavaScript 表达式：

```csharp
[WhiteList("override bool.GetHashCode()")]
[ECMAScriptLiteral("@#{0} ? 1 : 0")]
public extern static Number BooleanGetHashCode(bool instance);

[WhiteList("override bool.Equals(object)")]
[ECMAScriptLiteral("@#{0} === @#{1}")]
public extern static bool BooleanEquals(bool instance, Object? obj);
```

**特点**：
- 使用 `extern` 关键字声明外部实现
- JavaScript 代码直接嵌入生成的输出
- 适用于简单的、纯函数式转换

### 模式 2：使用 C# 实现复杂逻辑

适用于需要条件判断或复杂逻辑的场景：

```csharp
[WhiteList("static bool.Parse(string)")]
public static bool BooleanParse(string value)
{
    var str = value.Trim().ToLower();
    if (str == "true")
        return true;
    else if (str == "false")
        return false;
    else
        throw new Error($"FormatException: String '{value}' was not recognized as a valid Boolean.");
}
```

**特点**：
- 使用 C# 实现复杂逻辑
- 编译器会将 C# 代码转换为 JavaScript
- 支持完整的 C# 控制流语句

### 模式 3：标记为不支持

明确标记某些成员在 Jazor 中不支持：

```csharp
[WhiteList("bool.ToString(System.IFormatProvider)")]
[Obsolete("Not Support in Jazor", true)]
public extern static string BooleanToString2(bool instance, Intl.NumberFormat? provider);
```

**特点**：
- 使用 `[Obsolete]` 特性标记
- 第二个参数为 `true` 表示使用时会导致编译错误
- 白名单生成器会自动排除这些项

---

## 已启用模块详解

### CLRModule.cs

CLR 运行时支持类型，提供特殊类型封装。

```csharp
public sealed class OutValue<T>
{
    public T? Value { get; set; }
}

public sealed class RefValue<T>(T value)
{
    public T Value { get; set; } = value;
}
```

**用途**：
- `OutValue<T>`：封装 `out` 参数
- `RefValue<T>`：封装 `ref` 参数

### BooleanModule.cs

bool 类型的完整实现。

**白名单类型**：
```csharp
[WhiteList("bool")]
```

**支持的成员**：
| 成员 | JavaScript 实现 | 说明 |
|------|---------------|------|
| `static readonly bool.TrueString` | `'true'` | 字符串常量 |
| `static readonly bool.FalseString` | `'false'` | 字符串常量 |
| `override bool.GetHashCode()` | `@#{0} ? 1 : 0` | 哈希码 |
| `override bool.ToString()` | - | 转字符串 |
| `override bool.Equals(object)` | `@#{0} === @#{1}` | 相等比较 |
| `static bool.Parse(string)` | C# 实现 | 解析字符串 |

### StringModule.cs

string 类型的完整实现（超大文件，>25000 行）。

**白名单类型**：
```csharp
[WhiteList("string")]
```

**支持的功能**：
- 所有 string 实例方法（Substring, Trim, Replace, Split, ...）
- 所有 string 静态方法（Concat, Join, Format, IsNullOrEmpty, ...）
- 字符串操作（PadLeft, PadRight, Remove, Insert, ...）
- 搜索和比较（IndexOf, LastIndexOf, Contains, StartsWith, EndsWith, ...）
- 正则表达式相关方法

### ObjectModule.cs

object 类型的实现。

**白名单类型**：
```csharp
[WhiteList("object")]
```

**支持的成员**：
- `override object.GetHashCode()`
- `override object.ToString()`
- `override object.Equals(object)`
- `static object.Equals(object, object)`
- `static object.ReferenceEquals(object, object)`

### BigIntegerModule.cs

BigInteger 类型的完整实现（1444 行）。

**白名单类型**：
```csharp
[WhiteList("System.Numerics.BigInteger")]
```

**支持的功能**：
- 基本运算（Add, Subtract, Multiply, Divide, Remainder）
- 位运算（LeftShift, RightShift, BitwiseAnd, BitwiseOr, BitwiseXor）
- 数学运算（Log, Log10, Pow, ModPow, GCD）
- 位操作（RotateLeft, RotateRight, LeadingZeroCount, PopCount）
- 字节数组转换（ToByteArray, FromByteArray）
- 类型转换（Parse, ToString, TryParse）

### ValueTupleModule.cs

ValueTuple 和自定义 Tuple 类的实现。

**白名单类型**：
```csharp
[WhiteList("System.ValueTuple")]
```

**自定义 Tuple 类**：
```csharp
[Description("@#Tuple")]
public sealed class Tuple : Array<object?>
{
    // 支持命名元组：["name", value] 语法
    // 使用 Object.Freeze() 冻结对象
    // 实现 With() 方法用于不可变更新
}
```

**特点**：
- 支持 C# ValueTuple 到 JavaScript 对象的转换
- 支持命名元组元素
- 实现不可变更新语义

---

## 白名单生成流程

### 自动生成

ECMAScript.CLR 使用源生成器自动生成白名单文件。

**生成器**：`ECMAScript.Compiler/WhiteListGenerator.cs`

**输出文件**：`ECMAScript.Analyzer/WhiteList.cs`

**触发时机**：编译时自动运行

### 生成逻辑

```csharp
// 伪代码
foreach (var type in Assembly.GetTypes())
{
    if (type.HasCustomAttribute<ECMAScriptModuleAttribute>())
    {
        // 添加类型到白名单
        WhiteList.Types.Add(type.WhiteListName());

        foreach (var member in type.GetMembers())
        {
            if (member.HasCustomAttribute<WhiteListAttribute>())
            {
                // 排除标记为 [Obsolete] 的成员
                if (!member.HasCustomAttribute<ObsoleteAttribute>())
                {
                    // 添加成员到白名单
                    WhiteList.Members.Add(member.WhiteListName());
                }
            }
        }
    }
}
```

### 生成的 WhiteList.cs

```csharp
public static class WhiteList
{
    public static readonly HashSet<string> Types = new HashSet<string>
    {
        "void",
        "System.Nullable",
        "System.ValueTuple",
        "System.Array",
        "System.Numerics.BigInteger",
        "bool",
        "object",
        "string"
    };

    public static readonly HashSet<string> Members = new HashSet<string>
    {
        "override bool.GetHashCode()",
        "override bool.ToString()",
        "override bool.Equals(object)",
        "bool.Equals(bool)",
        "static bool.Parse(string)",
        // ... 更多成员
    };
}
```

---

## 协同工作流程

```
┌─────────────────────────────────────────────────────────────┐
│                    开发者编写代码                            │
│                  (C# 源代码)                                 │
└────────────────────┬────────────────────────────────────────┘
                     │
                     ▼
┌─────────────────────────────────────────────────────────────┐
│                  ECMAScript.CLR                              │
│  ┌─────────────────────────────────────────────────────┐   │
│  │ 1. 定义白名单（[WhiteList] 特性）                    │   │
│  │ 2. 提供 JavaScript 实现（ECMAScriptLiteral 或 C#）   │   │
│  └─────────────────────────────────────────────────────┘   │
│                     │                                       │
│                     │ 自动生成 WhiteList.cs                 │
│                     ▼                                       │
└─────────────────────────────────────────────────────────────┘
                     │
                     ▼
┌─────────────────────────────────────────────────────────────┐
│                  ECMAScript.Analyzer                          │
│  ┌─────────────────────────────────────────────────────┐   │
│  │ 1. 读取 WhiteList.Types 和 WhiteList.Members       │   │
│  │ 2. 检查用户代码使用的类型和成员                       │   │
│  │ 3. 拒绝使用未列入白名单的类型/成员                    │   │
│  └─────────────────────────────────────────────────────┘   │
└────────────────────┬────────────────────────────────────────┘
                     │
                     │ (通过白名单验证)
                     ▼
┌─────────────────────────────────────────────────────────────┐
│                  ECMAScript.Compiler                          │
│  ┌─────────────────────────────────────────────────────┐   │
│  │ 1. 根据白名单中的名称反查 ECMAScript.CLR 实现        │   │
│  │ 2. 使用 ECMAScriptLiteral 或转换 C# 代码            │   │
│  │ 3. 生成对应的 ESTree AST 节点                       │   │
│  └─────────────────────────────────────────────────────┘   │
└────────────────────┬────────────────────────────────────────┘
                     │
                     ▼
┌─────────────────────────────────────────────────────────────┐
│                    JavaScript 代码                           │
│                  (ES6 module)                                │
└─────────────────────────────────────────────────────────────┘
```

---

## 项目配置

### 项目文件 (ECMAScript.CLR.csproj)

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <LangVersion>preview</LangVersion>
    <Nullable>enable</Nullable>
    <NoWarn>CS0626,CS0824,IDE0130,CA1822,IDE0060</NoWarn>
    <AllowUnsafeBlocks>False</AllowUnsafeBlocks>
  </PropertyGroup>

  <ItemGroup>
    <ProjectReference Include="..\ECMAScript\ECMAScript.csproj" />
  </ItemGroup>

  <!-- 禁用的模块 -->
  <ItemGroup>
    <Compile Remove="SByteModule.cs" />
    <Compile Remove="ByteModule.cs" />
    <Compile Remove="Int16Module.cs" />
    <!-- ... 更多已禁用模块 -->
  </ItemGroup>
</Project>
```

### 全局引用 (GlobalUsings.cs)

```csharp
global using System;
global using System.Collections.Generic;
global using System.Globalization;
global using ECMAScript;
global using static ECMAScript.CLRModule;
global using static ECMAScript.Global;
```

---

## 开发指南

### 添加新模块

1. **创建模块文件**：
```csharp
namespace ECMAScript;

[ECMAScriptModule]
[WhiteList("YourNamespace.YourType")]
public static class YourTypeModule
{
    // 使用 ECMAScriptLiteral 实现
    [WhiteList("static YourType.Method(string)")]
    [ECMAScriptLiteral("console.log(@#{0})")]
    public extern static void Method(string value);

    // 使用 C# 实现
    [WhiteList("static YourType.Parse(string)")]
    public static YourType Parse(string value)
    {
        // 复杂逻辑
        return new YourType();
    }
}
```

2. **在项目文件中启用**（如果模块被禁用）：
```xml
<ItemGroup>
  <Compile Remove="" />  <!-- 移除禁用配置 -->
</ItemGroup>
```

3. **编译项目**：
```bash
dotnet build src/ECMAScript.CLR
```

4. **白名单自动更新**：
   - `WhiteList.cs` 会在编译时自动生成
   - 位置：`src/ECMAScript.Analyzer/WhiteList.cs`

### 命名约定

| 类型 | 约定 | 示例 |
|------|------|------|
| 类型白名单 | 完整类型名 | `bool`, `System.Numerics.BigInteger` |
| 静态方法 | `static Type.Method(params)` | `static bool.Parse(string)` |
| 实例方法 | `Type.Method(params)` | `bool.ToString()` |
| 重写方法 | `override Type.Method(params)` | `override bool.GetHashCode()` |
| 静态字段 | `static readonly Type.Field` | `static readonly bool.TrueString` |
| 实例属性 | `Type.Property` | `string.Length` |

---

## 类型映射表

| C# 类型 | JavaScript 类型 | 模块 |
|---------|-----------------|------|
| `void` | `undefined` | AssignModule |
| `bool` | `boolean` | BooleanModule |
| `string` | `string` | StringModule |
| `object` | `object` | ObjectModule |
| `int` | `number` | Int32Module (禁用) |
| `long` | `bigint` | Int64Module (禁用) |
| `BigInteger` | `bigint` | BigIntegerModule |
| `DateTime` | `Date` | DateTimeModule (禁用) |
| `TimeSpan` | `bigint` | TimeSpanModule (禁用) |
| `List<T>` | `Array` | ListModule (禁用) |
| `Dictionary<K,V>` | `Map` | DictionaryModule (禁用) |
| `HashSet<T>` | `Set` | HashSetModule (禁用) |
| `(T1, T2)` | `{Item1, Item2}` | ValueTupleModule |

---

## 注意事项

### 当前限制

1. **已禁用模块**：大部分数值类型、日期时间类型、集合类型模块已被禁用
2. **渐进式开发**：项目处于渐进式开发状态，核心基础设施已建立，但大部分类型模块的实现还未启用

### 编译警告

项目配置了以下警告忽略：
- `CS0626`：类、结构、接口成员没有外部实现
- `CS0824`：extern 方法没有特性
- `IDE0130`：命名空间与文件夹结构不匹配
- `CA1822`：成员可以声明为静态
- `IDE0060`：未使用的参数

这些警告是正常的，因为：
- `extern` 方法由 `[ECMAScriptLiteral]` 提供实现
- 命名空间结构是按照功能组织而非文件夹结构

---

## 相关文件

- **特性定义**：`src/ECMAScript/attribute/`
  - `ECMAScriptModuleAttribute.cs`
  - `WhiteListKeyAttribute.cs`
  - `ECMAScriptLiteralAttribute.cs`

- **白名单生成器**：`src/ECMAScript.Compiler/WhiteListGenerator.cs`

- **生成的白名单**：`src/ECMAScript.Analyzer/WhiteList.cs`

---

## 许可证

本项目遵循 Jazor 项目的许可证。

---

**文档维护者**：Claude Code
**最后更新**：2026-01-27
**文档版本**：v1.0
