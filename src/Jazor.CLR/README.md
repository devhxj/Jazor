# Jazor.CLR

## 项目概述

Jazor.CLR 是 Jazor 编译器项目的 CLR 运行时支持层。它使用 C# 编写（语法贴合 JavaScript）来实现 .NET 类型对应的 ES6 module，为 C# 到 JavaScript 的编译提供类型成员的 JavaScript 运行时实现。

### 核心职责

- **类型映射**：将 .NET 类型映射到 JavaScript 类型
- **成员实现**：提供类型成员（方法、属性）的 JavaScript 运行时实现
- **白名单机制**：通过 `[Jazor]` 特性与 Analyzer 和 Compiler 协同工作
- **模块导出**：标记可导出的模块供 JavaScript 使用

## 目录结构

```text
Jazor.CLR/
├── GlobalUsings.cs          # 全局 using 声明
├── Jazor.CLR.csproj         # 项目配置
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
│   ├── ...
│   └── VoidModule.cs
└── doc/                     # 文档目录（记录成员签名）
    ├── BooleanModule.md
    ├── ConsoleModule.md
    └── ObjectModule.md
```

## 核心概念

### 1. `[ECMAScriptModule]` 特性

标记类为可导出的 ES6 模块：

```csharp
[ECMAScriptModule]
public static class BooleanModule
{
    // ...
}
```

### 2. `[Jazor]` 特性

控制编译器对成员的处理方式，定义在 [JazorAttribute](../Jazor.Common/JazorAttribute.cs) 中。

#### 操作类型 (Op)

| Op 值 | 说明 | 用途 |
| :--- | :--- | :--- |
| `Discard` | 不支持，丢弃 | 标记不需要转换到 JavaScript 的成员 |
| `Allowed` | 支持，无其他操作 | 允许使用，按默认方式处理 |
| `Replace` | 支持，替换名称 | 用指定名称替换原成员名 |
| `Import` | 支持，作为模块导入 | 类上表示模块引用，方法上表示必须有实现 |
| `Inline` | 支持，内联代码 | 字符串表示内联调用的代码 |
| `Compile` | 支持，编译器特殊处理 | 属性/方法上的特殊处理标记 |

#### 用法示例

```csharp
// 类级别：整个类替换为 console
[Jazor(Op.Replace, "System.Console", "console")]
public static class ConsoleModule
{
    // 方法级别：替换为 log
    [Jazor(Op.Replace, "static System.Console.WriteLine(string)", "log")]
    public extern static void _19f2583beee4f7fb(object value);

    // 方法级别：丢弃，不转换
    [Jazor(Op.Discard, "static System.Console.Clear()")]
    public extern static void _7779d957d8f16481();
}
```

```csharp
// 类级别：导入模块
[ECMAScriptModule]
[Jazor(Op.Import, "bool", "System/BooleanModule.js")]
public static class BooleanModule
{
    // 方法级别：必须有实现（C# 代码）
    [Jazor(Op.Import, "static bool.Parse(string)")]
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
}
```

### 3. 方法签名命名规则

所有方法名采用混淆签名格式：`_` + SHA256 哈希值。

**生成规则**：`_` + SHA256Hash(成员名称)

**示例**：
```csharp
// 成员: static bool.Parse(string)
// 签名: _5dbf54319ebc8dfe
[Jazor(Op.Import, "static bool.Parse(string)")]
public static bool _5dbf54319ebc8dfe(string value)
{
    // ...
}
```

doc 目录中的文件记录了各模块成员的签名映射。

### 4. extern 方法

使用 `extern` 关键字声明外部实现：

```csharp
[Jazor(Op.Discard, "override bool.GetHashCode()")]
public extern static Number _80b6c29cc0038969(Boolean instance);
```

- `extern` 方法没有方法体
- 由编译器根据 Op 类型进行处理
- 可能被丢弃、替换或内联

## 模块分类

### 基础类型模块

| 模块 | .NET 类型 | JavaScript 类型 |
| :--- | :--- | :--- |
| `VoidModule` | `void` | `undefined` |
| `BooleanModule` | `bool` | `boolean` |
| `CharModule` | `char` | `string` |
| `ObjectModule` | `object` | `object` |

### 数值类型模块

| 模块 | .NET 类型 | JavaScript 类型 |
| :--- | :--- | :--- |
| `SByteModule` | `sbyte` | `number` |
| `ByteModule` | `byte` | `number` |
| `Int16Module` | `short` | `number` |
| `UInt16Module` | `ushort` | `number` |
| `Int32Module` | `int` | `number` |
| `UInt32Module` | `uint` | `number` |
| `SingleModule` | `float` | `number` |
| `DoubleModule` | `double` | `number` |
| `DecimalModule` | `decimal` | `number` |
| `Int64Module` | `long` | `bigint` |
| `UInt64Module` | `ulong` | `bigint` |
| `BigIntegerModule` | `BigInteger` | `bigint` |

### 日期时间模块

| 模块 | .NET 类型 | JavaScript 类型 |
| :--- | :--- | :--- |
| `DateTimeModule` | `DateTime` | `Date` |
| `DateTimeOffsetModule` | `DateTimeOffset` | `Date` |
| `DateOnlyModule` | `DateOnly` | `Date` |
| `TimeOnlyModule` | `TimeOnly` | `number` |
| `TimeSpanModule` | `TimeSpan` | `bigint` |

### 集合类型模块

| 模块 | .NET 类型 | JavaScript 类型 |
| :--- | :--- | :--- |
| `ArrayModule` | `Array<T>` | `Array` |
| `ListModule` | `List<T>` | `Array` |
| `DictionaryModule` | `Dictionary<K,V>` | `Map` |
| `HashSetModule` | `HashSet<T>` | `Set` |
| `ReadOnlyCollectionModule` | `ReadOnlyCollection<T>` | `readonly Array` |
| `ReadOnlyDictionaryModule` | `ReadOnlyDictionary<K,V>` | `readonly Map` |
| `ReadOnlySetModule` | `ReadOnlySet<T>` | `readonly Set` |

### 其他模块

| 模块 | 说明 |
| :--- | :--- |
| `StringModule` | 字符串操作 |
| `StringBuilderModule` | 字符串构建器 |
| `ConsoleModule` | 控制台输出 |
| `MathModule` | 数学运算 |
| `NullableModule` | 可空类型支持 |
| `ValueTupleModule` | 值元组 |
| `ExceptionModule` | 异常处理 |
| `ConditionalWeakTableModule` | 条件弱表 |
| `WeakReferenceModule` | 弱引用 |
| `CultureInfoModule` | 文化信息 |
| `GregorianCalendarModule` | 格里高利历 |

## 与其他项目的协作

### Jazor.CLR 在编译流程中的位置

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

## 类型映射说明

### C# 到 JavaScript 的类型映射

| C# 类型 | JavaScript 类型 | 说明 |
| :--- | :--- | :--- |
| `void` | `undefined` | 无返回值 |
| `bool` | `boolean` | 布尔值 |
| `char` | `string` | 单字符字符串 |
| `string` | `string` | 字符串 |
| `byte/sbyte/short/ushort/int/uint/float/double/decimal` | `number` | 浮点数 |
| `long/ulong/BigInteger` | `bigint` | 大整数 |
| `DateTime/DateTimeOffset/DateOnly` | `Date` | 日期对象 |
| `TimeOnly` | `number` | 时间（毫秒） |
| `TimeSpan` | `bigint` | 时间间隔（刻度） |
| `Array<T>/List<T>` | `Array` | 数组 |
| `Dictionary<K,V>` | `Map` | 映射 |
| `HashSet<T>` | `Set` | 集合 |
| `object` | `object` | 对象 |

### 特殊类型

- `Uint32Array` - 表示 `ReadOnlySpan<char>`，字符数组
- `Uint8Array` - 表示字节数组
- `Box<T>` - 表示 out/ref 参数的包装类型（当前实现，计划移除）
- `IArray<T>` - 表示数组接口

## out/ref 参数处理（计划中）

> **状态**: 设计阶段，当前仍使用 `Box<T>` 实现，待调整

### 设计思路

C# 的 out/ref 参数将通过返回数组的方式转换到 JavaScript：

1. 方法返回值和 out/ref 参数都放在数组中返回
2. 数组格式：`[返回值, out参数1, out参数2, ...]`
3. 使用临时变量和逗号表达式进行解包

### 转换示例

**C# 代码**:

```csharp
var a = "123";
var b = false;
if(b && Int32.TryParse(a, out b))
{
    Console.WriteLine(b);
}
```

**JavaScript 转换结果**:

```javascript
let a = "123";
let b = false;
let $0;
if(b && ($0=_Int32Module_TryParse(a,b), b=$0[1], $0[0]))
{
    console.log(b);
}
```

### 逗号表达式解析

```javascript
($0=_Int32Module_TryParse(a,b), b=$0[1], $0[0])
```

执行顺序：

1. 调用 `_Int32Module_TryParse(a, b)`，返回数组 `[true, 123]`
2. 将数组赋值给临时变量 `$0`
3. 将 `$0[1]`（即 `123`）赋值给 `b`
4. 返回 `$0[0]`（即 `true`）作为整个表达式的值

### 模块方法签名调整

**当前实现**:

```csharp
[Jazor(Op.Discard, "static int.TryParse(string, out int)")]
public extern static bool _16e2a901535b765e(object s, Box<Number> result);
```

**计划调整为**:

```csharp
[Jazor(Op.Import, "static int.TryParse(string, out int)")]
public static object[] _16e2a901535b765e(object s, object _result)
{
    // C# 实现
    bool success = int.TryParse(s, out int result);
    return new object[] { success, result };
}
```

> **说明**: `_result` 参数仅用于类型推导和保持签名一致性，实际不使用。

### 多个 out/ref 参数示例

**C# 代码**:

```csharp
if (DivRem(10, 3, out int quotient, out int remainder))
{
    Console.WriteLine($"{quotient}, {remainder}");
}
```

**JavaScript 转换结果**:

```javascript
let $0;
if (($0=_DivRem(10, 3), quotient=$0[1], remainder=$0[2], $0[0]))
{
    console.log(`${quotient}, ${remainder}`);
}
```

**方法返回**: `[true, 3, 1]` (返回值, quotient, remainder)

### 待实现部分

1. **模块文件签名调整** - 所有带 out/ref 参数的方法
2. **SemanticWalker 处理逻辑** - 识别 out/ref 参数并生成转换代码
3. **临时变量生成** - 自动生成 `$0`, `$1`, ... 临时变量
4. **Box\<T\> 类型移除** - 不再需要包装类型

## 开发指南

### 添加新模块

1. 在 `module/` 目录创建新文件，如 `NewTypeModule.cs`
2. 添加 `[ECMAScriptModule]` 和 `[Jazor(Op.Import, ...)]` 特性
3. 为需要支持的成员添加方法声明
4. 根据需要实现方法或使用 `extern`

```csharp
namespace Jazor.CLR;

[ECMAScriptModule]
[Jazor(Op.Import, "System.YourType", "System/YourTypeModule.js")]
public static class YourTypeModule
{
    // 使用 C# 实现
    [Jazor(Op.Import, "static System.YourType.Parse(string)")]
    public static YourType _xxx(string value)
    {
        // 实现代码
    }

    // 使用 extern 声明
    [Jazor(Op.Discard, "System.YourType.UnsupportedMethod()")]
    public extern static void _yyy();
}
```

### 成员命名约定

`[Jazor]` 特性的 Member 参数使用 .NET 完整成员名格式：

- 静态方法：`static TypeName.MethodName(params)`
- 实例方法：`TypeName.MethodName(params)`
- 静态属性：`static TypeName.PropertyName.get` / `.set`
- 实例属性：`TypeName.PropertyName.get` / `.set`
- 重写方法：`override TypeName.MethodName(params)`
- 运算符：`static TypeName.operator +(Type, Type)`

### 代码风格

- 代码使用 C# 编写，但语法贴合 JavaScript
- 使用 JavaScript 类型名称（`Number`、`String`、`Boolean`、`Object` 等）
- 使用 JavaScript 运行时 API（如 `Error` 构造函数）

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

- `CS0626` - extern 方法没有特性
- `CS0824` - 构造函数标记为 extern
- `IDE0130` - 命名空间与文件夹不匹配
- `CA1822` - 成员可以标记为静态
- `IDE0060` - 未使用的参数
- `IDE1006` - 命名风格不符合规则（混淆签名）

## 依赖关系

- **ECMAScript** - 提供 ECMAScript AST 类型和 JavaScript 运行时类型
- **Jazor.Common** - 提供 `[Jazor]` 特性和 `Op` 枚举

## 文档资源

- [doc/](./doc/) - 各模块成员签名文档
- [CLAUDE.md](../../CLAUDE.md) - Jazor 项目整体开发规则
