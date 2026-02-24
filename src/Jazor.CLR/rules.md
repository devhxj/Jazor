# ECMAScript.CLR 白名单映射规则文档

本文档定义了 C# 到 JavaScript 的白名单映射规则，用于指导 `ECMAScript.CLR` 模块中各模块类的配置。

## 基本原则

1. **优先使用 JS 原生方法**：尽可能使用 `WhiteListOp.Replace` 映射到 JavaScript 原生方法
2. **保持委托类型**：`Action<T>`、`Predicate<T>`、`Converter<TInput,TOutput>`、`Comparison<T>` 等委托参数应保持原类型，不要映射为 `object`
3. **按需 Import**：无法用 JS 原生方法映射的实现，使用 `WhiteListOp.Import` 引入外部实现
4. **慎用 Discard**：只有在 JavaScript 中完全没有对应语义的功能才使用 `WhiteListOp.Discard`

## WhiteListOp 枚举说明

| 枚举值 | 用途 | 使用场景 |
|--------|------|---------|
| `Allowed` | 允许类型使用 | 类型级别配置，无额外操作 |
| `Replace` | 替换为 JS 原生方法 | 直接映射到 JavaScript 原生方法 |
| `Import` | 导入外部实现 | 需要自定义实现的场景 |
| `Discard` | 丢弃不支持 | JS 中无法实现或语义差异过大 |
| `Equals` | 特殊相等比较 | 覆盖 Equals 方法 |
| `CompareTo` | 特殊大小比较 | 覆盖 CompareTo 方法 |

## 数组高阶方法映射规则

### C# Predicate<T> → JavaScript 回调函数

`Predicate<T>` 是返回 `bool` 的单参数委托，直接映射为 JavaScript 的回调函数：

| C# 方法 | JavaScript 方法 | WhiteListOp |
|---------|----------------|-------------|
| `static bool Exists<T>(T[], Predicate<T>)` | `Array.prototype.some()` | `Replace, "some"` |
| `static T? Find<T>(T[], Predicate<T>)` | `Array.prototype.find()` | `Replace, "find"` |
| `static Array<T> FindAll<T>(T[], Predicate<T>)` | `Array.prototype.filter()` | `Replace, "filter"` |
| `static Number FindIndex<T>(T[], Predicate<T>)` | `Array.prototype.findIndex()` | `Replace, "findIndex"` |
| `static T? FindLast<T>(T[], Predicate<T>)` | `Array.prototype.findLast()` | `Replace, "findLast"` (ES2023) |
| `static Number FindLastIndex<T>(T[], Predicate<T>)` | `Array.prototype.findLastIndex()` | `Replace, "findLastIndex"` (ES2023) |
| `static bool TrueForAll<T>(T[], Predicate<T>)` | `Array.prototype.every()` | `Replace, "every"` |

### C# Action<T> → JavaScript 回调函数

`Action<T>` 是无返回值的单参数委托：

| C# 方法 | JavaScript 方法 | WhiteListOp |
|---------|----------------|-------------|
| `static void ForEach<T>(T[], Action<T>)` | `Array.prototype.forEach()` | `Replace, "forEach"` |

### C# Converter<TInput, TOutput> → JavaScript 回调函数

`Converter<TInput, TOutput>` 是带返回值的单参数委托：

| C# 方法 | JavaScript 方法 | WhiteListOp |
|---------|----------------|-------------|
| `static TOutput[] ConvertAll<TInput, TOutput>(TInput[], Converter<TInput, TOutput>)` | `Array.prototype.map()` | `Replace, "map"` |

### 其他数组方法映射

| C# 方法 | JavaScript 方法 | WhiteListOp | 说明 |
|---------|----------------|-------------|------|
| `static void Fill<T>(T[], T)` | `Array.prototype.fill()` | `Replace, "fill"` | ES2016+ |
| `static void Reverse(System.Array)` | `Array.prototype.reverse()` | `Replace, "reverse"` | 仅无参数版本 |
| `static Number IndexOf(System.Array, object)` | `Array.prototype.indexOf()` | `Replace, "indexOf"` | 仅无参数版本 |
| `static Number LastIndexOf(System.Array, object)` | `Array.prototype.lastIndexOf()` | `Replace, "lastIndexOf"` | 仅无参数版本 |
| `Number Length.get` | `array.length` | `Replace, "length"` | 属性访问 |
| `Object Clone()` | `array.slice()` | `Replace, "slice"` | 浅拷贝 |

## 委托类型参数映射规则

### 保持原类型的委托

以下委托类型应**保持原类型**，不要映射为 `object`：

```csharp
// ✅ 正确
public extern static void _method<T>(Array<T> array, Action<T> action);
public extern static bool _method<T>(Array<T> array, Predicate<T> predicate);
public extern static TOutput[] _method<TInput, TOutput>(Array<TInput> array, Converter<TInput, TOutput> converter);
public extern static void _method<T>(Array<T> array, Comparison<T> comparison);

// ❌ 错误
public extern static void _method<T>(Array<T> array, object action);
public extern static bool _method<T>(Array<T> array, object predicate);
```

### 可映射为 object 的类型

以下接口/抽象类型由于 JavaScript 中没有直接对应，可以映射为 `object`：

- `IComparer<T>` - 比较器接口
- `IEqualityComparer<T>` - 相等比较器接口
- `IEnumerable<T>` - 枚举接口（可映射为 Iterable）

## 基础类型映射规则

| C# 类型 | JavaScript 类型 | 说明 |
|---------|----------------|------|
| `void` | `undefined` | 无返回值 |
| `bool` | `boolean` | 布尔值 |
| `byte`, `sbyte`, `short`, `ushort` | `Number` | 小整数 |
| `int`, `uint` | `Number` | 整数 |
| `long`, `ulong` | `BigInt` | 大整数 |
| `float`, `double`, `decimal` | `Number` | 浮点数 |
| `char` | `string` | 字符（单字符字符串） |
| `string` | `string` | 字符串 |
| `object` | `object` | 对象 |
| `Array<T>`, `T[]` | `Array` | 数组 |
| `DateTime` | `Date` | 日期 |
| `TimeSpan` | `object` / `BigInt` | 时间间隔 |

## 方法签名映射规则

### 实例方法 vs 静态方法

```csharp
// 实例方法
[WhiteList("System.Array.GetLength(int)", WhiteListOp.Discard)]
public extern static Number _method(System.Array instance, Number dimension);

// 静态方法
[WhiteList("static System.Array.Reverse(System.Array)", WhiteListOp.Replace, "reverse")]
public extern static void _method(object array);
```

### 属性 getter/setter

```csharp
// 属性 getter
[WhiteList("System.Array.Length.get", WhiteListOp.Replace, "length")]
public extern static Number _property(System.Array instance);

// 属性 setter
[WhiteList("System.Array.Length.set", WhiteListOp.Discard)]
public extern static void _property(System.Array instance, Number value);
```

### 泛型方法

```csharp
[WhiteList("static System.Array.Find<T>(T[], System.Predicate<T>)", WhiteListOp.Replace, "find")]
public extern static T? _method<T>(Array<T> array, Predicate<T> match);
```

## Import 实现规则

当无法使用 `Replace` 映射时，使用 `WhiteListOp.Import`：

```csharp
[WhiteList("static System.Array.Clear(System.Array, int, int)", WhiteListOp.Import, "System/ArrayModule.js")]
public extern static void _method(object array, Number index, Number length);
```

### 需要 Import 的场景

1. **语义差异大**：C# 和 JavaScript 的行为有本质差异
2. **复杂逻辑**：无法简单映射到单个 JS 方法
3. **需要额外实现**：需要组合多个 JS 操作

### Import 文件路径规则

```
System/ArrayModule.js       → src/ECMAScript.CLR/Output/System/ArrayModule.js
System/MathModule.js        → src/ECMAScript.CLR/Output/System/MathModule.js
System/StringModule.js      → src/ECMAScript.CLR/Output/System/StringModule.js
```

## Discard 使用规则

仅在以下场景使用 `WhiteListOp.Discard`：

1. **JavaScript 不支持**：
   - 多维数组操作（`GetLength(int dimension)`）
   - 数组创建（`CreateInstance`）
   - 线程同步（`SyncRoot`, `IsSynchronized`）

2. **语义完全不同**：
   - `Resize` - C# 改变数组大小 vs JS 数组不可变长度
   - `BinarySearch` - C# 有序数组二分查找 vs JS 无此概念

3. **非核心功能**：
   - `GetEnumerator` - 迭代器功能
   - `AsReadOnly` - 只读包装器

## 数组方法映射速查表

### 完全支持（Replace）

| C# | JS | ES 版本 |
|----|----|--------|
| `Exists` | `some` | ES5 |
| `Find` | `find` | ES2015 |
| `FindAll` | `filter` | ES5 |
| `FindIndex` | `findIndex` | ES2015 |
| `FindLast` | `findLast` | ES2023 |
| `FindLastIndex` | `findLastIndex` | ES2023 |
| `ForEach` | `forEach` | ES5 |
| `TrueForAll` | `every` | ES5 |
| `ConvertAll` | `map` | ES5 |
| `Fill` | `fill` | ES2016 |
| `Reverse` | `reverse` | ES1 |
| `IndexOf` | `indexOf` | ES5 |
| `LastIndexOf` | `lastIndexOf` | ES5 |
| `Clone` | `slice` | ES1 |
| `Length` | `length` | ES1 |

### 需要实现（Import）

| C# | 说明 |
|----|------|
| `Clear` | 部分清空数组元素 |
| `Sort` | 排序（语义差异） |
| `Copy` | 数组复制 |
| `Resize` | 改变数组大小 |

### 不支持（Discard）

| C# | 原因 |
|----|------|
| `CreateInstance` | 多维数组创建 |
| `GetLength` | 多维数组长度 |
| `GetValue/SetValue` | 多维数组索引 |
| `BinarySearch` | 有序数组查找 |
| `AsReadOnly` | 只读包装器 |

## 最佳实践

### 1. 优先检查 ES 标准

在决定使用 `Replace` 前，检查目标 JavaScript ES 版本是否支持该方法。

### 2. 保持参数类型一致性

同一方法的不同重载应保持委托参数类型一致：

```csharp
// ✅ 正确 - 重载中保持 Predicate<T> 类型
[WhiteList("static System.Array.Find<T>(T[], Predicate<T>)", ...)]
public extern static T? _find<T>(Array<T> array, Predicate<T> match);

[WhiteList("static System.Array.Find<T>(T[], int, int, Predicate<T>)", ...)]
public extern static T? _findRange<T>(Array<T> array, Number start, Number count, Predicate<T> match);

// ❌ 错误 - 重载中类型不一致
[WhiteList("static System.Array.Find<T>(T[], Predicate<T>)", ...)]
public extern static T? _find<T>(Array<T> array, Predicate<T> match);

[WhiteList("static System.Array.Find<T>(T[], int, int, Predicate<T>)", ...)]
public extern static T? _findRange<T>(Array<T> array, Number start, Number count, object match);
```

### 3. 添加 XML 注释

为每个映射添加 C# XML 注释，说明对应的 JavaScript 方法：

```csharp
///<summary>Determines whether the specified array contains elements that match the conditions defined by the specified predicate.</summary>
///<remarks>Maps to JavaScript Array.prototype.some() method.</remarks>
[WhiteList("static System.Array.Exists<T>(T[], System.Predicate<T>)", WhiteListOp.Replace, "some")]
public extern static bool _3795c9344e3fe39f<T>(Array<T> array, Predicate<T> match);
```

### 4. 方法哈希命名

使用方法签名的 SHA256 哈希（前8位）作为方法名，避免冲突：

```csharp
public extern static bool _3795c9344e3fe39f<T>(Array<T> array, Predicate<T> match);
//                      ^^^^^^^^^^^^^^^^^ 方法签名哈希
```

## 版本兼容性说明

### ES2023+ 方法

以下方法需要 ES2023 或更高版本：
- `findLast`
- `findLastIndex`

### ES2016+ 方法

以下方法需要 ES2016 或更高版本：
- `fill`

如果项目需要支持更旧的 JavaScript 环境，这些方法应该使用 `Import` 提供降级实现。

---

**文档维护者**: developerhan
**最后更新**: 2025-02-05
**文档版本**: v1.0
