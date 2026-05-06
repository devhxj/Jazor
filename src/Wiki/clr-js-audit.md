---
title: "CLR 生成 JS 语法与逻辑问题审计"
tags: ["clr", "generated-js", "audit", "compiler", "bug-tracking"]
created: 2026-05-06T02:31:12.923Z
updated: 2026-05-06T02:31:12.923Z
sources: []
links: []
category: debugging
confidence: medium
schemaVersion: 1
---

# CLR 生成 JS 语法与逻辑问题审计

# CLR 生成 JS 语法与逻辑问题审计

> 审计范围：`src/Wiki/jazor/System` 下 49 个 JS 模块文件
> 审计日期：2026-05-06
> 这些文件是 `jazor.clr` 模块编译器自动生成的 JavaScript 运行时代码

## 统计

| 严重度 | 数量 |
|--------|------|
| CRITICAL（运行时崩溃/完全错误） | 5 |
| HIGH（功能错误/语义偏差） | 11 |
| MEDIUM（已知限制/潜在问题） | 10 |

---

## CRITICAL — 运行时崩溃或完全错误行为

### C1. `BigInt.zero` / `BigInt.one` 不是标准 JavaScript

**影响文件：** Int64Module.js, UInt64Module.js, DecimalModule.js, RuntimeModule.js, DateTimeOffsetModule.js, TimeOnlyModule.js, TimeSpanModule.js, BigIntegerModule.js 等几乎所有使用 BigInt 的模块

**行号示例：** Int64Module.js:40,47,49; DecimalModule.js:186,187,188,252,341,498,564,578; RuntimeModule.js:56,67,128,141,149,296,326

**问题：** `BigInt.zero` 和 `BigInt.one` 不是 ECMAScript 标准属性，求值为 `undefined`。导致：
- 除零检查 `if (right === BigInt.zero)` 永远为 false，除零抛原始 `RangeError` 而非 `DivideByZeroException`
- Popcount 循环 `v > BigInt.zero` 为 `v > undefined`，永远 false，循环不执行
- 前导零计数 `v & BigInt.one` 抛 `TypeError: Cannot mix BigInt and other types`
- TryParse 返回 `[false, undefined]` 而非 `[false, 0n]`
- JDateTime 构造函数 `this.subMillisecondTicks = BigInt.zero` 设为 `undefined`

**修复：** 生成器级别修复，所有 `BigInt.zero` 替换为 `0n`，`BigInt.one` 替换为 `1n`

### C2. `DateTimeFormat` / `NumberFormat` / `Locale` / `DisplayNames` / `PropertyDescriptor` 未加 `Intl.` 前缀

**影响文件：** DateTimeModule.js, DateTimeOffsetModule.js, CultureInfoModule.js, RuntimeModule.js

**行号示例：** DateTimeModule.js:155; DateTimeOffsetModule.js:165,279,284; CultureInfoModule.js:40,68,484,511,526; RuntimeModule.js:58,69,80,130,141

**问题：**
- `new DateTimeFormat(locale, ...)` — 标准 JS 中不存在裸 `DateTimeFormat`，应为 `Intl.DateTimeFormat`
- `new NumberFormat()` — 应为 `Intl.NumberFormat`
- `new Locale(...)` — 应为 `Intl.Locale`
- `new DisplayNames(...)` — 应为 `Intl.DisplayNames`
- `new PropertyDescriptor(...)` — 标准中完全不存在，应为普通对象 `{ value, configurable, enumerable, writable }`

**影响：** 所有日期时间格式化、文化信息查询、运行时类型构造在标准 JS 环境中均抛 `ReferenceError`

**修复：** 生成器级别修复，所有 `Intl` API 引用必须加 `Intl.` 前缀；`PropertyDescriptor` 替换为对象字面量

### C3. CharModule 类型不一致 — Parse 返回 string，其他函数期望 number

**影响文件：** CharModule.js

**行号：** CharModule.js:17 (Parse), :8-9 (CompareTo), :29-30 (IsControl), :37-38 (IsDigit), :45-46 (IsLower), :53-54 (IsUpper), :61-62 (IsLetter), :69-70 (IsWhiteSpace), :73-75 (GetNumericValue), :108-110 (ConvertToUtf32)

**问题：** `_5ad63706a889c294`（StringModule 的 charAt wrapper）返回单字符 **string**。`Parse` 返回此 string，但 `CompareTo` 检查 `typeof value !== 'number'` 始终为 true → 总是抛异常。`IsDigit`/`IsLower`/`IsUpper` 等做 `c >= 97 && c <= 122`，字符串 `'a'` 被隐式转为 `NaN`，结果永远是 false。`GetNumericValue` 对所有输入返回 -1。

**修复：** 统一 char 表示为 number（`charCodeAt`）或统一为 string。建议使用 number 以匹配 C# char 语义

### C4. DoubleModule._aed2927097617729 未导出

**影响文件：** DecimalModule.js:446

**问题：** `createDecimalFromNumber` 调用 `DoubleModule._aed2927097617729(value)`（对应 `Double.IsFinite`），但此函数未从 DoubleModule.js 导出。所有从 `double`/`float` 创建 `decimal` 的操作均抛 `TypeError`

**修复：** 将 `_aed2927097617729` 加入 DoubleModule 的 export 列表

### C5. StringModule.escapeRegexCharClassChar — String.fromCharCode(ch) 传入字符串

**影响文件：** StringModule.js:206

**问题：** `ch` 来自 `charAt()` 返回值（string 类型）。`String.fromCharCode(ch)` 期望数值 code point。字符串 `'a'` 被隐式转为 `NaN`，`String.fromCharCode(NaN)` 返回 `'\0'`。对于所有非特殊字符（非 `\ ] ^ -`），生成的正则字符类字符均为 `'\0'`（空字符），导致 `split` 函数在非 ASCII 字符上的行为完全错误

**修复：** fallthrough 分支应为 `return ch;`（ch 已经是正确的字符串字符）

---

## HIGH — 功能错误或语义偏差

### H1. Int64Module BigInt 旋转使用算术右移 — 负值旋转结果错误

**文件：** Int64Module.js:59,65

**问题：** C# `long.RotateLeft/Right` 将值视为无符号 64 位模式。JS BigInt `>>` 执行**算术**右移（符号扩展），没有无符号右移 `>>>`。对负值的旋转产生错误位模式

**修复：** 用掩码模拟逻辑右移：`((value >> amount) & ((1n << 64n) - 1n))`

### H2. Int64Module 旋转函数的 BigInt/Number 混合取模

**文件：** Int64Module.js:56,62

**问题：** `rotateAmount % 64` — 若 `rotateAmount` 是 BigInt，BigInt 与 number 取模抛 `TypeError`

**修复：** 使用 `rotateAmount % 64n`

### H3. CharModule.IsLetter 只检查小写 a-z

**文件：** CharModule.js:46

**问题：** 仅检查 `c >= 97 && c <= 122`（a-z），缺少 A-Z (65-90)。加上 C3 的类型问题，此函数对所有输入返回 false

### H4. CharModule.IsControl 缺少 C1 控制字符范围 (0x80-0x9F)

**文件：** CharModule.js:30

**问题：** 只检查 `c < 32 || c === 127`，缺少 0x7F-0x9F 范围。C# `char.IsControl` 包含 0x00-0x1F、0x7F-0x9F

**修复：** 改为 `c < 32 || (c >= 127 && c <= 159)`

### H5. ArrayModule.Resize — `[newSize]` 创建含单元素的数组

**文件：** ArrayModule.js:12

**问题：** `let newArray = [newSize]` 创建 `[5]`（含一个元素 5 的数组），而非长度为 5 的空数组

**修复：** 应为 `new Array(newSize)` 或 `Array(newSize).fill(undefined)`

### H6. BigInteger.Log/Log10/Log(base) 公式错误

**文件：** BigIntegerModule.js:110-112 (Log), :140 (Log10), :122-132 (Log with base)

**问题：**
- `Log` 对小数字使用未归一化尾数，如 `Log(10)` 计算为 `2 * Math.log(10)` 而非 `Math.log(10)`
- `Log10` 对 >15 位的数字公式错误，加了剩余位数而非归一化尾数
- `Log(value, Math.E)` 直接 `Math.log(Number(value))`，大 BigInt 全部精度丢失

### H7. BigInteger.LeadingZeroCount 总是返回零

**文件：** BigIntegerModule.js:361-365

**问题：** 非零值也无条件返回 `BigInt.zero`（即 `undefined`），实现完全缺失

### H8. TimeSpan multiplyByDouble / divideByDouble 大值精度丢失

**文件：** TimeSpanModule.js:67-71

**问题：** `Number(instance.ticks) * factor` 对 `|ticks| > 2^53` 的 TimeSpan 结果不正确。TimeSpan 最大 ticks 远超 `Number.MAX_SAFE_INTEGER`

### H9. ListT1Module.Sort 无比较器时使用 JS 默认字典序排序

**文件：** ListT1Module.js:333-334; ArrayModule.js:594,601 等

**问题：** `array.sort()` 对数字做字典序排序：`[10, 2, 1]` → `[1, 10, 2]` 而非 `[1, 2, 10]`。C# `List<T>.Sort()` 使用 `Comparer<T>.Default`

**修复：** 使用 `ComparerT1Module.compareObjectsCore` 作为默认比较器

### H10. ComparerT1Module NaN 排序语义与 .NET 不一致

**文件：** ComparerT1Module.js:14-17

**问题：** NaN 与数字比较时返回 -1（NaN < number），但 .NET `Comparer<double>.Default` 中 NaN 大于一切

### H11. DateTimeOffset getDateTimeInstantTicks — Unspecified kind 语义偏差

**文件：** DateTimeOffsetModule.js:96-101

**问题：** Unspecified kind 使用 `dateTime.date.getTime()`（UTC 时间戳），但 C# 将 Unspecified 视为本地时间

---

## MEDIUM — 已知限制或潜在问题

### M1. CharModule.IsWhiteSpace 不完整

**文件：** CharModule.js:69-70

**问题：** 仅检查空格/Tab/LF/CR/FF，缺少 VT(0x0B)、NEL(0x85)、NBSP(0xA0) 等 Unicode 空白

### M2. ByteModule/UInt16Module/UInt32Module Parse 负数时抛出错误异常类型

**文件：** ByteModule.js:44; UInt16Module.js:43; UInt32Module.js:42

**问题：** C# 对负数输入抛 `OverflowException`，生成代码抛 `FormatException`

### M3. DecimalModule 死代码分支 — getNumberStylesValue/getMidpointRoundingValue

**文件：** DecimalModule.js:117-122, 476-481

**问题：** 两个连续 `typeof style === "number"` 检查，第二个永远不可达

### M4. DateTimeModule exported object 重复键

**文件：** DateTimeModule.js:1721-1723 (createFromTicks), 1728 (getTicks)

**问题：** `createFromTicks` 和 `getTicks` 在 export object 中出现两次，后者覆盖前者

### M5. StringModule exported object 重复键

**文件：** StringModule.js:622

**问题：** `buildSplitCharClassPattern` 出现两次，第二次映射到 `BuildSplitCharClassPattern`

### M6. DateTimeModule UTC suffix 使用 Local kind

**文件：** DateTimeModule.js:1329

**问题：** 当输入以 'Z' 结尾时，返回 `get_DateTimeKindLocal()` 而非 `get_DateTimeKindUtc()`

### M7. HashSet/Dictionary 使用 JS Set/Map 忽略自定义比较器

**文件：** DictionaryT2Module.js:7; HashSetT1Module.js:112

**问题：** `Map.has()`/`Set.has()` 使用 `SameValueZero`，对象按引用比较。C# 的自定义 `IEqualityComparer` 被静默忽略

### M8. ComparerT1 混合类型比较使用 toString() 回退

**文件：** ComparerT1Module.js:40-48

**问题：** 不同类型比较回退到 `x.toString()` 字典序。C# `Comparer<object>.Default` 对不可比较类型抛异常。普通对象均为 `"[object Object]"` 被视为相等

### M9. DictionaryCarrierRuntime / SetCarrierRuntime 未导入

**文件：** IDictionaryT2Module.js:13; ISetT1Module.js:5; ReadOnlyDictionaryT2Module.js:10; ReadOnlySetT1Module.js:6

**问题：** 假设为全局变量/运行时绑定，若模块加载时不存在则抛 `ReferenceError`

### M10. BigInteger.ModPow 缺少负值归一化

**文件：** BigIntegerModule.js:160

**问题：** `value % modulus` 在 JS BigInt 中可能为负，但未归一化到 `[0, modulus)`。C# `BigInteger.ModPow` 对负输入有归一化

---

## 按模块分布

| 模块 | CRITICAL | HIGH | MEDIUM |
|------|----------|------|--------|
| BigInt 相关（Int64/UInt64/Decimal/BigInteger） | 1 (BigInt.zero) | 4 (rotate, Log, LeadingZero) | 1 (ModPow) |
| CharModule | 1 (类型不一致) | 2 (IsLetter, IsControl) | 1 (IsWhiteSpace) |
| StringModule | 1 (fromCharCode) | 0 | 1 (重复键) |
| Runtime/DateTime 相关 | 1 (Intl/PropertyDescriptor) | 1 (DateTimeOffset kind) | 2 (重复键, UTC kind) |
| ArrayModule | 0 | 1 (Resize) | 0 |
| Collections | 0 | 2 (Sort fallback, NaN sort) | 3 (Map/Set比较器, toString回退, Runtime未导入) |
| DoubleModule | 1 (未导出) | 0 | 0 |
| 整数 Parse (Byte/UInt16/UInt32) | 0 | 0 | 1 (异常类型) |

---

## 根因分析

### 生成器级别需修复（影响多文件）

1. **`BigInt.zero` → `0n`**：影响 10+ 模块，50+ 处引用
2. **Intl API 前缀**：`DateTimeFormat` → `Intl.DateTimeFormat`，`NumberFormat` → `Intl.NumberFormat`，`Locale` → `Intl.Locale`，`DisplayNames` → `Intl.DisplayNames`
3. **PropertyDescriptor → 对象字面量**：RuntimeModule 中所有类型构造
4. **char 表示统一**：整个 char 处理链需要统一为 number 或 string
5. **Export 重复键**：同名 camelCase/PascalCase 函数的导出合并策略

### 单文件修复

- StringModule.js escapeRegexCharClassChar fallthrough
- ArrayModule.js Resize 数组初始化
- DoubleModule.js 导出列表
- Int64Module.js 旋转实现

