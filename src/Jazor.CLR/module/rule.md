# Boolean 类型模块映射规则

本文档记录 `System.Boolean` 类型从 C# 到 JavaScript 的映射规则。

## 1. 模块声明

```csharp
[ECMAScriptModule]
[Jazor(Op.Import, "bool", "System/BooleanModule.js")]
public static class BooleanModule
```

- `[ECMAScriptModule]` - 标记为 ECMAScript 模块
- `[Jazor(Op.Import, "bool", "System/BooleanModule.js")]` - 类型映射：`bool` → 导入 `System/BooleanModule.js` 模块

## 2. Op 枚举说明

| Op 值 | 含义 | 用途 |
|-------|------|------|
| `Discard` | 不支持，丢弃 | 该成员在 JavaScript 中不可用或不适用 |
| `Allowed` | 支持，无其他操作 | 允许调用，无特殊处理 |
| `Replace` | 支持，替换名称 | 将方法名替换为 JavaScript 原生方法名 |
| `Import` | 支持，模块导入 | 需要导入外部模块实现 |
| `Inline` | 支持，内联代码 | 直接嵌入 JavaScript 代码片段 |
| `Compile` | 支持，编译器特殊处理 | 编译器根据上下文生成代码 |

## 3. 静态字段映射

### 3.1 TrueString / FalseString

| C# 成员 | 映射方式 | JavaScript 结果 |
|---------|----------|----------------|
| `bool.TrueString` | `Op.Inline` → `"true"` | 字符串字面量 `"true"` |
| `bool.FalseString` | `Op.Inline` → `"false"` | 字符串字面量 `"false"` |

**示例**：

```csharp
// C#
var trueStr = bool.TrueString;
var falseStr = bool.FalseString;
```

```javascript
// JavaScript
let trueStr = "true";
let falseStr = "false";
```

## 4. 构造函数映射

| C# 成员 | 映射方式 | JavaScript 结果 |
|---------|----------|----------------|
| `bool.Boolean()` | `Op.Allowed` | 无操作（JavaScript 布尔值是原始类型） |

**说明**：JavaScript 中布尔值是原始类型，不需要构造函数。

## 5. 实例方法映射

### 5.1 GetHashCode

| C# 成员 | 映射方式 | 说明 |
|---------|----------|------|
| `override bool.GetHashCode()` | `Op.Discard` | JavaScript 无对应概念，丢弃 |

**原因**：JavaScript 没有统一的对象哈希码机制，布尔值的哈希码在 JS 中无意义。

### 5.2 ToString

| C# 成员 | 映射方式 | JavaScript 结果 |
|---------|----------|----------------|
| `override bool.ToString()` | `Op.Replace` → `toString` | 调用原生 `toString()` |
| `bool.ToString(IFormatProvider)` | `Op.Discard` | JavaScript 无格式提供者概念 |

**示例**：

```csharp
// C#
bool flag = true;
var str = flag.ToString();
```

```javascript
// JavaScript
let flag = true;
let str = flag.toString(); // "true"
```

### 5.3 TryFormat

| C# 成员 | 映射方式 | 说明 |
|---------|----------|------|
| `bool.TryFormat(Span<char>, out int)` | `Op.Discard` | JavaScript 无 Span 概念 |

### 5.4 Equals

| C# 成员 | 映射方式 | JavaScript 内联代码 |
|---------|----------|---------------------|
| `override bool.Equals(object)` | `Op.Inline` | `(@#{0} === @#{1})` |
| `bool.Equals(bool)` | `Op.Inline` | `(@#{0} === @#{1})` |

**占位符说明**：
- `@#{0}` - 第一个参数（实例）
- `@#{1}` - 第二个参数（比较对象）

**示例**：

```csharp
// C#
bool a = true;
bool b = false;
var result = a.Equals(b);
```

```javascript
// JavaScript
let a = true;
let b = false;
let result = (a === b); // false
```

### 5.5 CompareTo

| C# 成员 | 映射方式 | JavaScript 内联代码 |
|---------|----------|---------------------|
| `bool.CompareTo(object)` | `Op.Inline` | `(@#{0} === @#{1} ? 0 : (@#{0} ? 1 : -1))` |
| `bool.CompareTo(bool)` | `Op.Inline` | `(@#{0} === @#{1} ? 0 : (@#{0} ? 1 : -1))` |

**语义说明**：
- 相等返回 `0`
- `true > false`，所以 `true.CompareTo(false)` 返回 `1`
- `false < true`，所以 `false.CompareTo(true)` 返回 `-1`

**示例**：

```csharp
// C#
bool a = true;
bool b = false;
var result = a.CompareTo(b); // 1
```

```javascript
// JavaScript
let a = true;
let b = false;
let result = (a === b ? 0 : (a ? 1 : -1)); // 1
```

### 5.6 GetTypeCode

| C# 成员 | 映射方式 | 说明 |
|---------|----------|------|
| `bool.GetTypeCode()` | `Op.Discard` | JavaScript 无 TypeCode 概念 |

## 6. 静态方法映射

### 6.1 Parse

| C# 成员 | 映射方式 | 说明 |
|---------|----------|------|
| `static bool.Parse(string)` | `Op.Import` | 模块导入，有完整实现 |
| `static bool.Parse(ReadOnlySpan<char>)` | `Op.Import` | 模块导入，有完整实现 |

**JavaScript 实现逻辑**：

```javascript
// BooleanModule.js
export function Parse(value) {
    let str = value?.trim()?.toLowerCase();
    if (str === "true")
        return true;
    else if (str === "false")
        return false;
    else
        throw new Error(`FormatException: String '${value}' was not recognized as a valid Boolean.`);
}
```

**示例**：

```csharp
// C#
var result = bool.Parse("True");   // true
var result2 = bool.Parse("false"); // false
```

```javascript
// JavaScript
let result = Parse("True");   // true
let result2 = Parse("false"); // false
```

### 6.2 TryParse

| C# 成员 | 映射方式 | JavaScript 返回值 |
|---------|----------|-------------------|
| `static bool.TryParse(string, out bool)` | `Op.Import` | `[success, value]` 数组 |
| `static bool.TryParse(ReadOnlySpan<char>, out bool)` | `Op.Import` | `[success, value]` 数组 |

**out 参数处理**：C# 的 out 参数在 JavaScript 中通过返回数组模拟。

**JavaScript 实现逻辑**：

```javascript
// BooleanModule.js
export function TryParse(value, result) {
    let str = value?.trim()?.toLowerCase();
    if (str === "true")
        return [true, true];
    else if (str === "false")
        return [true, false];
    return [false, false];
}
```

**示例**：

```csharp
// C#
if (bool.TryParse(input, out bool result))
{
    Console.WriteLine(result);
}
```

```javascript
// JavaScript
let [success, result] = TryParse(input, false);
if (success) {
    console.log(result);
}
```

## 7. 类型映射表

| C# 类型 | JavaScript 类型 | 备注 |
|---------|-----------------|------|
| `bool` | `boolean` | 原始类型 |
| `bool?` | `boolean \| null` | 可空类型 |
| `Number`（模块内） | `number` | 用于返回 int 的方法 |

## 8. 设计原则

### 8.1 为什么 GetHashCode 被丢弃？

- JavaScript 没有统一的 `GetHashCode` 机制
- 布尔值的哈希码（0 或 1）在 JavaScript 中无实际用途
- `Map` 和 `Set` 使用引用相等或值相等，不需要哈希码

### 8.2 为什么 ToString 使用 Replace 而非 Inline？

- JavaScript 布尔值原生支持 `toString()` 方法
- `true.toString()` 和 `false.toString()` 与 C# 语义一致
- 使用 `Replace` 可以直接调用原生方法，效率更高

### 8.3 为什么 Equals 和 CompareTo 使用 Inline？

- `Equals`：JavaScript 的严格相等 `===` 与 C# 的 `bool.Equals` 语义完全一致
- `CompareTo`：JavaScript 没有原生的布尔比较方法，需要自定义逻辑
- 内联代码避免了额外的函数调用开销

### 8.4 Parse/TryParse 为什么使用 Import？

- 解析逻辑较复杂（空格处理、大小写处理、错误处理）
- 需要完整的 JavaScript 实现
- 使用 `Import` 保持代码清晰，便于维护

## 9. 完整映射汇总表

| C# 成员 | Op | 替换/内联值 | JavaScript 结果 |
|---------|-----|-------------|-----------------|
| `static readonly bool.TrueString` | Inline | `"true"` | `"true"` |
| `static readonly bool.FalseString` | Inline | `"false"` | `"false"` |
| `bool.Boolean()` | Allowed | - | 无操作 |
| `override bool.GetHashCode()` | Discard | - | 不支持 |
| `override bool.ToString()` | Replace | `toString` | `instance.toString()` |
| `bool.ToString(IFormatProvider)` | Discard | - | 不支持 |
| `bool.TryFormat(...)` | Discard | - | 不支持 |
| `override bool.Equals(object)` | Inline | `(@#{0} === @#{1})` | `(a === b)` |
| `bool.Equals(bool)` | Inline | `(@#{0} === @#{1})` | `(a === b)` |
| `bool.CompareTo(object)` | Inline | `(@#{0} === @#{1} ? 0 : (@#{0} ? 1 : -1))` | 内联表达式 |
| `bool.CompareTo(bool)` | Inline | `(@#{0} === @#{1} ? 0 : (@#{0} ? 1 : -1))` | 内联表达式 |
| `static bool.Parse(string)` | Import | - | 模块函数调用 |
| `static bool.Parse(ReadOnlySpan<char>)` | Import | - | 模块函数调用 |
| `static bool.TryParse(string, out bool)` | Import | - | 返回 `[success, value]` |
| `static bool.TryParse(ReadOnlySpan<char>, out bool)` | Import | - | 返回 `[success, value]` |
| `bool.GetTypeCode()` | Discard | - | 不支持 |

## 10. 注意事项

1. **out 参数处理**：C# 的 out 参数在 JavaScript 中通过返回数组模拟，返回值格式为 `[success, value]`

2. **ReadOnlySpan<char> 处理**：映射为 `Uint32Array`，每个元素是一个 Unicode 码点

3. **方法命名**：模块内方法使用哈希值命名（如 `_5dbf54319ebc8dfe`），避免命名冲突

4. **可空处理**：`string?` 参数需要使用可选链操作符 `?.` 处理空值