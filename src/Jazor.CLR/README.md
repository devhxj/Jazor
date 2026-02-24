# ECMAScript.CLR

Jazor 编译器的 CLR 运行时支持库，使用 C# 编写（但语法贴合 JavaScript）来实现 C# 类型对应的 ES6 module。

## 项目概述

ECMAScript.CLR 是 Jazor 项目的重要组成部分，负责提供：

1. **白名单定义**：声明哪些 C# 类型和成员可以被编译器使用
2. **JavaScript 运行时实现**：为白名单中的类型提供 JavaScript 等价实现
3. **类型映射**：将 C# 类型映射到 JavaScript 类型
4. **模块导入**：定义 JavaScript 模块路径用于 ES6 import

### 设计目的

- 将 C# 类型的属性和方法统一转换成可导出的方法
- 提供类型成员的 JavaScript 运行时实现
- 通过白名单机制与 Analyzer 和 Compiler 协同工作
- 支持从外部 JavaScript 模块导入实现

---

## 项目结构

```
ECMAScript.CLR/
├── 核心基础模块
│   ├── AssignModule.cs           # 基础类型白名单定义（许可类型定义）
│   ├── BooleanModule.cs          # bool 类型实现（已调整）
│   ├── StringModule.cs           # string 类型实现（已调整）
│   ├── ObjectModule.cs           # object 类型实现（已调整）
│   ├── BigIntegerModule.cs       # BigInteger 类型实现（已调整）
│   ├── ValueTupleModule.cs       # ValueTuple 和 Tuple 实现
│   ├── ConsoleModule.cs          # System.Console 实现（已调整）
│   ├── MathModule.cs             # System.Math 实现（已调整）
│   └── ArrayModule.cs            # System.Array 实现（已调整）
│
├── 数值类型模块（已调整）
│   ├── SByteModule.cs            # sbyte (System.SByte) ✓
│   ├── ByteModule.cs             # byte (System.Byte) ✓
│   ├── Int16Module.cs            # short (System.Int16) ✓
│   ├── UInt16Module.cs           # ushort (System.UInt16) ✓
│   ├── Int32Module.cs            # int (System.Int32) ✓
│   ├── UInt32Module.cs           # uint (System.UInt32) ✓
│   ├── Int64Module.cs            # long (System.Int64) ✓
│   ├── UInt64Module.cs           # ulong (System.UInt64) ✓
│   ├── SingleModule.cs           # float (System.Single) ✓
│   ├── DoubleModule.cs           # double (System.Double) ✓
│   ├── DecimalModule.cs          # decimal (System.Decimal) ✓
│   └── CharModule.cs             # char (System.Char) ✓
│
├── 日期时间模块
│   ├── DateTimeModule.cs         # DateTime
│   ├── DateOnlyModule.cs         # DateOnly
│   ├── TimeOnlyModule.cs         # TimeOnly
│   ├── DateTimeOffsetModule.cs   # DateTimeOffset
│   ├── TimeSpanModule.cs         # TimeSpan
│   ├── GregorianCalendarModule.cs # GregorianCalendar
│   └── CultureInfoModule.cs      # CultureInfo
│
├── 集合类型模块
│   ├── ListModule.cs             # List<T>
│   ├── DictionaryModule.cs       # Dictionary<K,V>
│   ├── HashSetModule.cs          # HashSet<T>
│   ├── ReadOnlyCollectionModule.cs
│   ├── ReadOnlyDictionaryModule.cs
│   ├── ReadOnlySetModule.cs
│   └── ConditionalWeakTableModule.cs
│
└── 其他工具模块
    ├── StringBuilderModule.cs    # StringBuilder
    ├── NullableModule.cs         # Nullable<T>
    ├── WeakReferenceModule.cs    # WeakReference<T>
    └── ExceptionModule.cs        # Exception
```

> **注意**：所有模块均已启用，无禁用模块。

---

## 白名单机制

### 核心特性

ECMAScript.CLR 使用三个核心类型来定义白名单和 JavaScript 实现：

#### 1. `[ECMAScriptModule]`

标记类为可导出的 ES6 module。

```csharp
[ECMAScriptModule]
[WhiteList("bool", WhiteListOp.Allowed, null, "System/BooleanModule.js")]
public static class BooleanModule
{
    // ...
}
```

**参数**：
- `Import`（可选）：指定导入路径（通过 `WhiteListAttribute` 的 `path` 参数设置）

#### 2. `[WhiteList]`

声明类型或成员的白名单映射名称和处理方式。

```csharp
// 类型白名单 - 指定模块路径
[WhiteList("bool", WhiteListOp.Allowed, null, "System/BooleanModule.js")]

// 成员白名单 - 丢弃（不导入）
[WhiteList("override bool.GetHashCode()", WhiteListOp.Discard)]

// 成员白名单 - 替换为 JavaScript 原生方法
[WhiteList("override bool.ToString()", WhiteListOp.Replace, "toString")]

// 成员白名单 - 从 C# 实现导入
[WhiteList("static bool.Parse(string)", WhiteListOp.Import)]
```

**参数**：
- `member`：白名单名称（使用 ECMAScript.Common.Util.NameFormat 格式化）
- `op`：处理方式（`WhiteListOp` 枚举）
- `value`：当 `op` 是 `Replace` 时，指定替换的 JavaScript 方法名
- `path`：当是类名时，指定 JavaScript 模块路径

#### 3. `WhiteListOp` 枚举

定义白名单成员的处理方式：

| 值 | 说明 | 用途 |
|-----|------|------|
| `Discard` | 不支持，丢弃 | 不导入该成员 |
| `Allowed` | 支持，无其他操作 | 仅标记为允许使用 |
| `Replace` | 支持，替换名称 | 替换为 JavaScript 原生方法 |
| `Import` | 支持，作为模块导入 | 从 C# 实现导入逻辑 |
| `Equals` | 特殊处理，判断相等 | 用于 `Equals` 方法 |
| `CompareTo` | 特殊处理，比较大小 | 用于 `CompareTo` 方法 |

---

## 实现模式

### 模式 1：替换为 JavaScript 原生方法

适用于可以映射到 JavaScript 原生方法的场景：

```csharp
[WhiteList("override bool.ToString()", WhiteListOp.Replace, "toString")]
public extern static string _d48c2d39317daf8f(Boolean instance);

[WhiteList("static System.Math.Abs(int)", WhiteListOp.Replace, "abs")]
public extern static Number _0aaf1073fc70e405(Number value);

[WhiteList("static System.Console.Write(string)", WhiteListOp.Replace, "log")]
public extern static void _89898d51245a9c64(object value);
```

**特点**：
- 使用 `extern` 关键字声明外部实现
- `WhiteListOp.Replace` 指定替换的 JavaScript 方法名
- 编译器直接生成对 JavaScript 原生方法的调用

### 模式 2：从 C# 实现导入

适用于需要条件判断或复杂逻辑的场景：

```csharp
[WhiteList("static bool.Parse(string)", WhiteListOp.Import)]
public static bool _5dbf54319ebc8dfe(string value)
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
- 使用 `WhiteListOp.Import` 标记
- 编译器会将 C# 代码转换为 JavaScript
- 支持完整的 C# 控制流语句

### 模式 3：标记为不支持

明确标记某些成员在 Jazor 中不支持：

```csharp
[WhiteList("bool.ToString(System.IFormatProvider)", WhiteListOp.Discard)]
public extern static string _6e30cb91da447de8(Boolean instance, Intl.NumberFormat? provider);
```

**特点**：
- 使用 `WhiteListOp.Discard` 标记
- 白名单生成器会自动排除这些项

---

## 已启用模块详解

### AssignModule.cs

基础类型白名单定义，使用 record 类型声明。

```csharp
[WhiteList("void", WhiteListOp.Allowed)]
public record VoidModule;

[WhiteList("System.Nullable", WhiteListOp.Allowed)]
public record NullableModule;
```

### ConsoleModule.cs

System.Console 的完整实现，映射到 JavaScript 的 `console` 对象。

**白名单类型**：
```csharp
[WhiteList("System.Console", WhiteListOp.Replace, "console")]
```

**支持的成员**：
- `Write`/`WriteLine` → `console.log`
- 不支持控制台相关的方法（如 `BackgroundColor`, `CursorVisible` 等）

### MathModule.cs

System.Math 的完整实现，映射到 JavaScript 的 `Math` 对象。

**白名单类型**：
```csharp
[WhiteList("System.Math", WhiteListOp.Allowed, null, "System/MathModule.js")]
```

**支持的成员**：
| C# 方法 | JavaScript 方法 |
|---------|-----------------|
| `Abs` | `Math.abs` |
| `Acos` | `Math.acos` |
| `Asin` | `Math.asin` |
| `Atan` | `Math.atan` |
| `Atan2` | `Math.atan2` |
| `Ceiling` | `Math.ceil` |
| `Cos` | `Math.cos` |
| `Exp` | `Math.exp` |
| `Floor` | `Math.floor` |
| `Log` | `Math.log` |
| `Max` | `Math.max` |
| `Min` | `Math.min` |
| `Pow` | `Math.pow` |
| `Round` | `Math.round` |
| `Sin` | `Math.sin` |
| `Sqrt` | `Math.sqrt` |
| `Tan` | `Math.tan` |
| `Truncate` | `Math.trunc` |

### ArrayModule.cs

System.Array 的完整实现，提供数组操作方法。

**白名单类型**：
```csharp
[WhiteList("System.Array", WhiteListOp.Allowed, null, "System/ArrayModule.js")]
```

**支持的功能**：
- 数组属性：`Length`, `LongLength`, `Rank`
- 数组创建：`CreateInstance`, `Empty`
- 数组操作：`Copy`, `Clear`, `Clone`, `Resize`
- 数组搜索：`IndexOf`, `LastIndexOf`, `BinarySearch`, `Find`, `FindIndex`
- 数组排序：`Sort`, `Reverse`
- 数组转换：`ConvertAll`

### BooleanModule.cs

bool 类型的完整实现。

**白名单类型**：
```csharp
[WhiteList("bool", WhiteListOp.Allowed, null, "System/BooleanModule.js")]
```

**支持的成员**：
| 成员 | WhiteListOp | JavaScript 实现 |
|------|------------|----------------|
| `override bool.ToString()` | `Replace` | `toString` |
| `override bool.Equals(object)` | `Equals` | `===` |
| `bool.Equals(bool)` | `Equals` | `===` |
| `bool.CompareTo(object)` | `CompareTo` | - |
| `bool.CompareTo(bool)` | `CompareTo` | - |
| `static bool.Parse(string)` | `Import` | C# 实现 |
| `static bool.TryParse(string, out bool)` | `Import` | C# 实现 |

### StringModule.cs

string 类型的完整实现。

**白名单类型**：
```csharp
[WhiteList("string", WhiteListOp.Allowed, null, "System/StringModule.js")]
```

**支持的功能**：
- 字符串属性：`Length`
- 字符串操作：`Substring`, `Trim`, `Replace`, `Split`, `PadLeft`, `PadRight`
- 字符串搜索：`IndexOf`, `LastIndexOf`, `Contains`, `StartsWith`, `EndsWith`
- 字符串比较：`Compare`, `CompareTo`, `Equals`
- 字符串转换：`ToLower`, `ToUpper`, `ToString`
- 字符串拼接：`Concat`, `Join`, `Format`

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

BigInteger 类型的完整实现。

**白名单类型**：
```csharp
[WhiteList("System.Numerics.BigInteger")]
```

**支持的功能**：
- 基本运算：`Add`, `Subtract`, `Multiply`, `Divide`, `Remainder`
- 位运算：`LeftShift`, `RightShift`, `BitwiseAnd`, `BitwiseOr`, `BitwiseXor`
- 数学运算：`Log`, `Log10`, `Pow`, `ModPow`, `GCD`
- 位操作：`RotateLeft`, `RotateRight`, `LeadingZeroCount`, `PopCount`
- 字节数组转换：`ToByteArray`, `FromByteArray`
- 类型转换：`Parse`, `ToString`, `TryParse`

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

---

## 方法名编码规则

所有模块方法名都使用 SHA256 哈希编码，确保唯一性：

```csharp
// 示例方法名
public extern static Number _80b6c29cc0038969(Boolean instance);  // GetHashCode
public extern static string _d48c2d39317daf8f(Boolean instance);  // ToString
public extern static bool _97cc6572c33639b7(Boolean instance, Object? obj);  // Equals
```

**编码目的**：
- 避免与方法签名的命名冲突
- 确保生成的 JavaScript 代码中方法名唯一
- 便于工具链处理和优化

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
    if (type.HasCustomAttribute<WhiteListAttribute>())
    {
        var whiteListAttr = type.GetCustomAttribute<WhiteListAttribute>();

        // 添加类型到白名单
        if (whiteListAttr.Op != WhiteListOp.Discard)
        {
            WhiteList.Types.Add(whiteListAttr.Member);
        }

        foreach (var member in type.GetMembers())
        {
            if (member.HasCustomAttribute<WhiteListAttribute>())
            {
                var memberAttr = member.GetCustomAttribute<WhiteListAttribute>();

                // 添加成员到白名单（排除 Discard）
                if (memberAttr.Op != WhiteListOp.Discard)
                {
                    WhiteList.Members.Add(memberAttr.Member);
                }
            }
        }
    }
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
│  │ 2. 指定处理方式（WhiteListOp 枚举）                  │   │
│  │ 3. 提供 JavaScript 实现（Replace/Import）            │   │
│  │ 4. 指定模块路径（path 参数）                         │   │
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
│  │ 2. 根据 WhiteListOp 处理：                           │   │
│  │    - Replace: 生成对 JavaScript 原生方法的调用       │   │
│  │    - Import: 转换 C# 实现为 JavaScript              │   │
│  │    - Equals: 生成 === 比较                           │   │
│  │    - CompareTo: 生成比较逻辑                         │   │
│  │ 3. 生成对应的 ESTree AST 节点                        │   │
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
    <ProjectReference Include="..\ECMAScript.Common\ECMAScript.Common.csproj" />
  </ItemGroup>
</Project>
```

### 全局引用 (GlobalUsings.cs)

```csharp
global using System;
global using System.Collections.Generic;
global using System.Globalization;
global using ECMAScript;
global using ECMAScript.Common;
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
[WhiteList("YourNamespace.YourType", WhiteListOp.Allowed, null, "System/YourTypeModule.js")]
public static class YourTypeModule
{
    // 替换为 JavaScript 原生方法
    [WhiteList("static YourType.Method(string)", WhiteListOp.Replace, "methodName")]
    public extern static void _hash1234(string value);

    // 从 C# 实现导入
    [WhiteList("static YourType.Parse(string)", WhiteListOp.Import)]
    public static YourType _hash5678(string value)
    {
        // 复杂逻辑
        return new YourType();
    }

    // 不支持
    [WhiteList("YourType.Unsupported()", WhiteListOp.Discard)]
    public extern static void _hashabcd();
}
```

2. **编译项目**：
```bash
dotnet build src/ECMAScript.CLR
```

3. **白名单自动更新**：
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

| C# 类型 | JavaScript 类型 | WhiteListOp | 模块 |
|---------|-----------------|-------------|------|
| `void` | `undefined` | `Allowed` | AssignModule |
| `bool` | `boolean` | `Allowed` | BooleanModule |
| `string` | `string` | `Allowed` | StringModule |
| `object` | `object` | `Allowed` | ObjectModule |
| `int` | `number` | - | Int32Module |
| `long` | `bigint` | - | Int64Module |
| `BigInteger` | `bigint` | `Allowed` | BigIntegerModule |
| `DateTime` | `Date` | - | DateTimeModule |
| `TimeSpan` | `bigint` | - | TimeSpanModule |
| `List<T>` | `Array` | - | ListModule |
| `Dictionary<K,V>` | `Map` | - | DictionaryModule |
| `HashSet<T>` | `Set` | - | HashSetModule |
| `(T1, T2)` | `{Item1, Item2}` | `Allowed` | ValueTupleModule |
| `System.Console` | `console` | `Replace` | ConsoleModule |
| `System.Math` | `Math` | `Allowed` | MathModule |
| `System.Array` | `Array` | `Allowed` | ArrayModule |

---

## 注意事项

### 编译警告

项目配置了以下警告忽略：
- `CS0626`：类、结构、接口成员没有外部实现
- `CS0824`：extern 方法没有特性
- `IDE0130`：命名空间与文件夹结构不匹配
- `CA1822`：成员可以声明为静态
- `IDE0060`：未使用的参数

这些警告是正常的，因为：
- `extern` 方法由 `WhiteListOp.Replace` 或 `WhiteListOp.Import` 提供实现
- 命名空间结构是按照功能组织而非文件夹结构

### 方法名编码

所有方法名都使用 SHA256 哈希编码，这是正常的：
- 确保唯一性
- 避免命名冲突
- 便于工具链处理

---

## 相关文件

- **特性定义**：`src/ECMAScript.Common/`
  - `WhiteListKeyAttribute.cs` - 白名单特性
  - `WhiteListOp.cs` - 白名单操作枚举
  - `WhiteList.cs` - 生成的白名单

- **特性定义**：`src/ECMAScript/attribute/`
  - `ECMAScriptModuleAttribute.cs` - 模块特性

- **白名单生成器**：`src/ECMAScript.Compiler/WhiteListGenerator.cs`

- **生成的白名单**：`src/ECMAScript.Analyzer/WhiteList.cs`

---

## 许可证

本项目遵循 Jazor 项目的许可证。

---

**文档维护者**：developerhan
**最后更新**：2026-02-04
**文档版本**：v3.0
