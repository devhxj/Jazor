# Inline / Import 复审记录

## 目录

- [目标](#目标)
- [判定规则](#判定规则)
- [复审结果](#复审结果)
- [ECMAScript/internal 结果](#ecmascriptinternal-结果)
- [推荐实施顺序](#推荐实施顺序)
- [当前结论](#当前结论)

## 目标

这份文档只回答一个问题：

> 在当前 Jazor 编译器实现下，哪些 `Op.Import` 其实应该降回 `Op.Inline`，哪些应该继续保留 `Import`，哪些更适合未来走 `Op.Compile`。

这里的判断标准不是“尽量贴近 CLR 内部语义实现方式”，而是：

- 生成结果是否稳定等价
- 是否会给第三方观察语义引入歧义
- 是否会把本应在编译期完成的语法糖处理错误地下沉到运行时模块

还有一条实现纪律：

- `WhiteList.cs.Generate.cs` 是生成产物，只能通过 `Jazor.Compiler.Generator` 刷新
- 不要手改 generated 白名单来“先让测试通过”

本次复审覆盖：

- `src/Jazor.CLR/module/*.cs`
- `src/ECMAScript/internal/*.cs`

结论先写在前面：

- `Allowed/Alias > Inline > Import`
- `Compile` 只保留给编译器内部必须直接接管的少数特例
- 能稳定写成 `Inline` 的，不要继续挂 `Import`
- 但不要为了“去 Import”而手写脆弱模板
- 只要涉及 tuple 对象协议/投影规则、复杂校验、循环、多步副作用、模块 helper 复用，就不要强行塞回 `Inline`

## 判定规则

### 1. 什么时候优先 `Inline`

满足以下条件时，优先 `Inline`：

- 最终可以表达成单个 JS expression
- 不依赖本模块私有 helper 或另一模块的运行时 helper
- 不需要循环
- 不需要显式 `throw`
- 不需要临时变量保存中间状态
- 不需要手写 tuple 对象协议
- 不会改变 `toString` / `toJSON` / 第三方可观察语义

补充说明：

- 当前 `Inline` 走的是 `Acornima` 的 `ParseExpression(...)`，不是旧的字符串拼接方案，所以 `sequence expression`、`spread` 等合法 expression 语法都可以作为候选。
- 但“语法上能写”不等于“工程上应该写”。可读性和稳定性仍然是第一约束。

### 2. 什么时候更适合 `Compile`

以下场景不要硬塞 `Inline`，更适合未来 `Op.Compile`：

- 仍然属于表达式级改写，但 `Inline` 模板会变成很难维护的多层条件表达式
- 需要表达 `throw` 分支
- 需要 tuple 构造，但不应该在白名单模板里手写 tuple 对象布局
- 需要引入一次性临时值以保证求值顺序或避免重复求值

### 3. 什么时候必须保留 `Import`

以下场景继续保留 `Import`：

- 需要循环或多步逻辑
- 需要运行时 helper
- 需要 `Parse` / `TryParse` / `Format` 这类完整协议
- 需要 `out` 返回包或多返回值协议
- 需要跨模块复用另一实现
- 需要异常消息、边界检查、范围校验

## 复审结果

### A. 高置信 `Import -> Inline`

这些成员已经满足“稳定单表达式”的要求，继续保留 `Import` 的收益不高。

#### 1. `StringBuilderModule`

文件：`src/Jazor.CLR/module/StringBuilderModule.cs`

- `System.Text.StringBuilder.StringBuilder(string)`
  - 当前实现只是把 `string?` 拆成字符数组
  - 当前循环按 UTF-16 code unit 工作
  - `("".split("")) == []`，`"😀".split("")` 也会保留两个代理项，和当前循环行为一致
  - 推荐模板：`(__arg1 ?? '').split('')`

- `System.Text.StringBuilder.Clear()`
  - 当前仅清空数组并返回自身
  - 推荐模板：`(__arg1.length = 0, __arg1)`

- `System.Text.StringBuilder.Append(string)`
  - 当前仅在 `value != null` 时逐字符 `push`
  - 推荐模板：`(__arg1.push(...(__arg2 ?? '').split('')), __arg1)`

- `System.Text.StringBuilder.AppendLine()`
  - 当前仅 `push('\n')` 并返回自身
  - 推荐模板：`(__arg1.push('\n'), __arg1)`

- `System.Text.StringBuilder.AppendLine(string)`
  - 当前仅先追加字符串，再追加换行，再返回自身
  - 推荐模板：`(__arg1.push(...(__arg2 ?? '').split('')), __arg1.push('\n'), __arg1)`

这组成员是本轮最适合先收缩的目标。

#### 2. `SingleModule`

文件：`src/Jazor.CLR/module/SingleModule.cs`

- `float.CompareTo(float)`
  - 当前只委托 `CompareCore`
  - 推荐模板：
    - `(__arg1 < __arg2 ? -1 : (__arg1 > __arg2 ? 1 : (isNaN(__arg1) ? (isNaN(__arg2) ? 0 : -1) : (isNaN(__arg2) ? 1 : 0))))`

- `float.Equals(float)`
  - 当前只委托 `AreEqualCore`
  - 推荐模板：
    - `((isNaN(__arg1) || isNaN(__arg2)) ? (isNaN(__arg1) && isNaN(__arg2)) : (!(__arg1 < __arg2) && !(__arg1 > __arg2)))`

- `static float.CopySign(float, float)`
  - 与 `double.CopySign(double, double)` 形状相同
  - 需要保留 `-0` 语义
  - 推荐模板：
    - `((__arg2 < 0 || Object.is(__arg2, -0)) ? -Math.abs(__arg1) : Math.abs(__arg1))`

`object` 重载不要一起收缩；它们还包含类型检查和异常分支。

#### 3. `DoubleModule`

文件：`src/Jazor.CLR/module/DoubleModule.cs`

- `double.CompareTo(double)`
  - 形态与 `float.CompareTo(float)` 相同
  - 推荐直接复用同一模板

- `double.Equals(double)`
  - 形态与 `float.Equals(float)` 相同
  - 推荐直接复用同一模板

- `static double.CopySign(double, double)`
  - 当前只委托 `CopySignCore`
  - 推荐模板：
    - `((__arg2 < 0 || Object.is(__arg2, -0)) ? -Math.abs(__arg1) : Math.abs(__arg1))`

#### 4. `MathModule`

文件：`src/Jazor.CLR/module/MathModule.cs`

- `static System.Math.CopySign(double, double)`
  - 逻辑与 `DoubleModule` 中的 `double.CopySign(double, double)` 一致
  - 可以收缩到同一模板

#### 5. `Int32Module` / `Int64Module`

文件：

- `src/Jazor.CLR/module/Int32Module.cs`
- `src/Jazor.CLR/module/Int64Module.cs`

- `static int.CopySign(int, int)`
  - `int` 不存在 `-0`
  - 可直接写成：
    - `(__arg2 < 0 ? -Math.abs(__arg1) : Math.abs(__arg1))`

- `static long.CopySign(long, long)`
  - `BigInt` 同样不存在 `-0`
  - 只需要按目标符号选择绝对值
  - 可直接写成：
    - `(__arg2 < 0n ? (__arg1 < 0n ? __arg1 : -__arg1) : (__arg1 < 0n ? -__arg1 : __arg1))`

#### 6. `Int16Module` / `UInt16Module`

文件：

- `src/Jazor.CLR/module/Int16Module.cs`
- `src/Jazor.CLR/module/UInt16Module.cs`

这两个模块里还有一批此前停在 `Discard` 的 16 位整数简单 intrinsic，本质都属于稳定表达式：

- `short.IsPow2(short)` / `ushort.IsPow2(ushort)`
- `short.Log2(short)` / `ushort.Log2(ushort)`
- `short.Clamp(short, short, short)` / `ushort.Clamp(ushort, ushort, ushort)`
- `short.Max(short, short)` / `ushort.Max(ushort, ushort)`
- `short.Min(short, short)` / `ushort.Min(ushort, ushort)`
- `short.Sign(short)` / `ushort.Sign(ushort)`
- `short.Abs(short)`
- `short.IsEvenInteger(short)` / `ushort.IsEvenInteger(ushort)`
- `short.IsOddInteger(short)` / `ushort.IsOddInteger(ushort)`
- `short.IsNegative(short)`
- `short.IsPositive(short)`
- `short.CopySign(short, short)`

这批都不需要运行时 helper、循环或异常分支，应该直接落回 `Inline`。

#### 7. `SByteModule` / `UInt32Module` / `UInt64Module`

文件：

- `src/Jazor.CLR/module/SByteModule.cs`
- `src/Jazor.CLR/module/UInt32Module.cs`
- `src/Jazor.CLR/module/UInt64Module.cs`

这三组成员与上面的 16 位整数 intrinsic 属于同一类问题，只是覆盖了 8 位、32 位和 `BigInt` 形态的无符号 64 位：

- `sbyte.IsPow2(sbyte)` / `uint.IsPow2(uint)` / `ulong.IsPow2(ulong)`
- `sbyte.Log2(sbyte)` / `uint.Log2(uint)` / `ulong.Log2(ulong)`
- `sbyte.Clamp(sbyte, sbyte, sbyte)` / `uint.Clamp(uint, uint, uint)` / `ulong.Clamp(ulong, ulong, ulong)`
- `sbyte.Max(sbyte, sbyte)` / `uint.Max(uint, uint)` / `ulong.Max(ulong, ulong)`
- `sbyte.Min(sbyte, sbyte)` / `uint.Min(uint, uint)` / `ulong.Min(ulong, ulong)`
- `sbyte.Sign(sbyte)` / `uint.Sign(uint)` / `ulong.Sign(ulong)`
- `sbyte.Abs(sbyte)`
- `sbyte.IsEvenInteger(sbyte)` / `uint.IsEvenInteger(uint)` / `ulong.IsEvenInteger(ulong)`
- `sbyte.IsOddInteger(sbyte)` / `uint.IsOddInteger(uint)` / `ulong.IsOddInteger(ulong)`
- `sbyte.IsNegative(sbyte)`
- `sbyte.IsPositive(sbyte)`
- `sbyte.CopySign(sbyte, sbyte)`

判断理由不变：

- 都能稳定收敛为单个表达式
- 不需要 helper、循环、异常或 tuple 形状
- `ulong` 虽然走 `BigInt`，但这批仍然只是比较、模运算、位运算和简单条件表达式

因此这批也应该直接用 `Inline`，不要继续保留 `Import` 或 `Discard`。

#### 8. `Int64Module` / `MathModule` 的 BigInt 整数简单成员

文件：

- `src/Jazor.CLR/module/Int64Module.cs`
- `src/Jazor.CLR/module/MathModule.cs`

这批成员虽然目标类型是 `BigInt`，但语义仍然只是简单比较和边界裁剪：

- `long.Clamp(long, long, long)`
- `long.Sign(long)`
- `Math.Clamp(long, long, long)` / `Math.Clamp(ulong, ulong, ulong)`
- `Math.Max(long, long)` / `Math.Max(ulong, ulong)`
- `Math.Min(long, long)` / `Math.Min(ulong, ulong)`
- `Math.Sign(long)`

它们和前面已经收口的 `ulong.Clamp/Max/Min/Sign`、`long.CopySign/Max/Min/Abs` 属于同一层级：

- 无 helper
- 无异常协议
- 无循环
- 无临时值
- 单个条件表达式即可稳定表达

因此这批也应直接落回 `Inline`，不应继续停留在 `Import` 或 `Discard`。

#### 9. `BigIntegerModule` 的简单静态 intrinsic

文件：

- `src/Jazor.CLR/module/BigIntegerModule.cs`

这批成员虽然声明在 `System.Numerics.BigInteger` 上，但当前运行时映射就是 JS `BigInt`：

- `BigInteger.Abs(BigInteger)`
- `BigInteger.CopySign(BigInteger, BigInteger)`
- `BigInteger.Max(BigInteger, BigInteger)`
- `BigInteger.Min(BigInteger, BigInteger)`
- `BigInteger.IsEvenInteger(BigInteger)`
- `BigInteger.IsNegative(BigInteger)`
- `BigInteger.IsOddInteger(BigInteger)`
- `BigInteger.IsPositive(BigInteger)`

它们都满足同一套 producer 侧判断：

- 单个表达式可稳定表达
- 不需要 helper、异常协议或临时值
- BigInt 不存在 `-0`，`CopySign` 可直接按符号位切换绝对值

所以这批也应优先落到 `Inline`，而不是继续停在 `Discard`。

### B. 更适合未来 `Import -> Compile`

这些成员不是不能做编译期改写，而是不适合继续用裸 `Inline` 模板承载。

#### 1. 需要 `throw` 分支的索引器 / 成员

文件：

- `src/Jazor.CLR/module/ListModule.cs`
- `src/Jazor.CLR/module/StringModule.cs`
- `src/Jazor.CLR/module/DictionaryModule.cs`

典型成员：

- `System.Collections.Generic.List<T>.this[int].get`
- `string.this[int].get`
- `System.Collections.Generic.Dictionary<TKey, TValue>.this[TKey].get`
- `System.Collections.Generic.Dictionary<TKey, TValue>.Add(TKey, TValue)`

原因：

- 本质仍是表达式级改写
- 但包含边界检查或存在性检查
- 若强行改成 `Inline`，会退化成 IIFE 或非常难读的条件表达式
- `Compile` 主分发虽然已经接好，但第一阶段 contract 仍缺少稳定的“throw 作为表达式分支”约定
- 在当前 contract 下硬迁移，只会把问题换成 IIFE 包装或语句级特判
- 这类逻辑更适合未来扩完 contract 后，再由 `Compile` 直接产 AST

#### 2. 需要一次性临时值的表达式

文件：

- `src/Jazor.CLR/module/HashSetModule.cs`
- `src/Jazor.CLR/module/DoubleModule.cs`
- `src/Jazor.CLR/module/MathModule.cs`

典型成员：

- `System.Collections.Generic.HashSet<T>.Add(T)`
- `static double.MaxMagnitude(double, double)`
- `static double.MinMagnitude(double, double)`
- `static System.Math.MaxMagnitude(double, double)`
- `static System.Math.MinMagnitude(double, double)`

原因：

- `HashSet.Add(T)` 需要保存旧 `size`
- `MaxMagnitude` / `MinMagnitude` 仍是表达式级逻辑，但 NaN、绝对值比较、tie-break 组合起来后，`Inline` 可读性明显变差
- 这类逻辑更适合 `Compile`，而不是继续膨胀模板字符串

#### 3. tuple 结果构造

文件：

- `src/Jazor.CLR/module/DoubleModule.cs`
- `src/Jazor.CLR/module/MathModule.cs`

典型成员：

- `static double.SinCos(double)`
- `static double.SinCosPi(double)`
- `static System.Math.SinCos(double)`

原因：

- 当前 tuple 在 Jazor 中是编译期语法糖
- 不能在 `Inline` 模板里手写 tuple 对象协议
- 这会把 tuple lowering 规则复制到白名单模板里，后续极难维护
- 这类成员应该保留 `Import`，或者等 `Compile` 接管 tuple 构造后再迁移

### C. 继续保留 `Import`

以下成员继续保留 `Import` 是正确的。

#### 1. `Parse` / `TryParse`

文件：

- `src/Jazor.CLR/module/BooleanModule.cs`
- `src/Jazor.CLR/module/SByteModule.cs`
- `src/Jazor.CLR/module/Int16Module.cs`
- `src/Jazor.CLR/module/SingleModule.cs`
- `src/Jazor.CLR/module/DoubleModule.cs`
- 以及其他数值 / 时间模块

原因：

- 有校验
- 有异常
- 有范围检查
- 有 `out` 返回协议

这些不是 `Inline` 的目标域。

#### 2. `decimal` 包装层

文件：

- `src/Jazor.CLR/module/MathModule.cs`

典型成员：

- `Math.Abs(decimal)`
- `Math.Clamp(decimal, decimal, decimal)`
- `Math.Ceiling(decimal)`
- `Math.Floor(decimal)`
- `Math.Max(decimal, decimal)`
- `Math.Min(decimal, decimal)`
- `Math.Round(...)`
- `Math.Sign(decimal)`
- `Math.Truncate(decimal)`

原因：

- 当前只是桥接到 `DecimalModule`
- 如果改成 `Inline`，就必须在模板里显式引用另一模块 helper
- 这会把模块边界和具体 helper 名暴露到模板层，稳定性不好

所以这批继续保留 `Import`。

#### 3. 浮点 / 整数 magnitude helper

文件：

- `src/Jazor.CLR/module/SingleModule.cs`
- `src/Jazor.CLR/module/DoubleModule.cs`
- `src/Jazor.CLR/module/MathModule.cs`
- `src/Jazor.CLR/module/Int16Module.cs`
- `src/Jazor.CLR/module/Int32Module.cs`
- `src/Jazor.CLR/module/Int64Module.cs`
- `src/Jazor.CLR/module/BigIntegerModule.cs`

典型成员：

- `float.MaxMagnitude` / `float.MinMagnitude`
- `float.MaxMagnitudeNumber` / `float.MinMagnitudeNumber`
- `double.MaxMagnitude` / `double.MinMagnitude`
- `double.MaxMagnitudeNumber` / `double.MinMagnitudeNumber`
- `System.Math.MaxMagnitude(double, double)` / `System.Math.MinMagnitude(double, double)`
- `short/int/long/BigInteger.MaxMagnitude` / `MinMagnitude`

原因：

- 这批成员虽然“理论上能写成表达式”，但真实语义包含绝对值比较、tie-break 和 `NaN` / `±0` 细节
- 浮点族里，`MaxMagnitude(-0, +0)` 需要返回 `+0`，`MinMagnitude(-0, +0)` 需要返回 `-0`
- `MaxMagnitudeNumber` / `MinMagnitudeNumber` 还要在 `NaN` 输入时切到“返回另一侧”的协议
- 把这套规则硬塞成长 `Inline` 模板只会增加脆弱性，不利于后续审查

因此这批更适合保留成小型 `Import` helper，而不是为了减少 import 数量继续压成模板字符串。

#### 4. 依赖循环或范围处理的集合方法

文件：

- `src/Jazor.CLR/module/ListModule.cs`
- `src/Jazor.CLR/module/HashSetModule.cs`

典型成员：

- `List<T>.CopyTo(...)`
- `List<T>.FindIndex(...)` 范围重载
- `List<T>.FindLast(...)`
- `List<T>.InsertRange(...)`
- `List<T>.RemoveAll(...)`
- `List<T>.Reverse(int, int)`
- `List<T>.Sort(int, int, IComparer<T>)`
- `HashSet<T>.UnionWith(...)`
- `HashSet<T>.IntersectWith(...)`
- `HashSet<T>.ExceptWith(...)`
- `HashSet<T>.SymmetricExceptWith(...)`
- `HashSet<T>.IsSubsetOf(...)`
- `HashSet<T>.SetEquals(...)`

原因：

- 本身就是循环或多步逻辑
- 有些场景若用 spread 代替循环，还会引入大输入下的参数展开风险

这批不应为了追求“少 Import”而回退。

#### 5. `string.Compare` / `string.Format`

文件：

- `src/Jazor.CLR/module/StringModule.cs`

原因：

- `string.Compare` 仍包含 `null` 规则和条件分支
- `string.Format` 包含替换流程，`params` 版本还带循环

这批继续保留 `Import` 更稳。

## ECMAScript/internal 结果

本轮检索没有发现 `src/ECMAScript/internal/*.cs` 中存在 `Op.Import` 成员。

因此“Inline 优先于 Import”的整改重点仍然在 `Jazor.CLR/module`。

## 推荐实施顺序

### 第一批：直接收缩到 `Inline`

优先做：

1. `StringBuilderModule` 的 5 个成员
2. `SingleModule` / `DoubleModule` 的 typed `CompareTo` / `Equals`
3. `MathModule` / `DoubleModule` 的 `CopySign`

原因：

- 不依赖 `Op.Compile`
- 风险窄
- 收益直接
- 规则示范性强

当前状态：

- 已实现
- 已重新运行 `Jazor.Compiler.Generator`
- 已通过 `Jazor.CLR` build
- 已通过 `Jazor.CompilerTest` 全量测试

### 第二批：等 `Op.Compile` contract 扩展后再处理

优先做：

1. 带 `throw` 的索引器 / `Add`
2. `HashSet.Add`
3. `MaxMagnitude` / `MinMagnitude`
4. `SinCos` / `SinCosPi`

当前状态：

- `Op.Compile` 主分发已经接入
- 第一阶段真实条目和测试已经落地
- 浮点 / 整型 magnitude 家族里，`SingleModule` / `DoubleModule` / `Int16Module` 已按“稳定优先”提前落地为 `Import` helper，并补了针对 `NaN` / `±0` / tie-break 的刻画测试
- 第二批剩余阻塞点不是“还没接线”，而是 contract 还不能稳定承载带 `throw` / temp / tuple 对象协议的表达式

## 当前结论

按“能用 `Inline` 就不要用 `Import`”这条规则看，当前最应该收缩的不是 `Parse`、`decimal`、集合范围算法，而是：

- `StringBuilder` 的几个简单成员
- `float` / `double` 的 typed `CompareTo` / `Equals`
- `CopySign`

而 tuple 结果构造、带异常分支的索引器、依赖临时值的表达式，则应明确归到未来 `Compile`，不要继续让 `Inline` 模板承担结构上不合适的职责。

补充复核结论：

- 看起来是“一行 `Import`”的不一定就是 `Inline` 候选
- `ReadOnlySpan` 重载转发、时间类型 runtime wrapper 构造、跨模块 helper 桥接，这三类通常仍应保留 `Import`

#### 10. BigIntegerModule 的 Compare / Equals / ToString 简单成员

文件：

- `src/Jazor.CLR/module/BigIntegerModule.cs`
- `src/Jazor.Compiler/core/SemanticWalker.cs.Reference.cs`

收缩内容：

- `BigInteger.Compare(BigInteger, BigInteger)` -> `Inline`
- `BigInteger.CompareTo(long/ulong/BigInteger)` -> `Inline`
- `BigInteger.Equals(object/long/ulong/BigInteger)` -> `Inline`
- `BigInteger.ToString()` -> `Alias`

保留内容：

- `BigInteger.CompareTo(object)` 继续保留 `Import`

原因：

- typed compare/equality 都是稳定单表达式
- `ToString()` 只是运行时成员别名，没有必要保留编译器内特判
- `CompareTo(object)` 仍需要 `null` / 类型检查，不能冒进塞进 `Inline`

附带修正：

- 删除 `SemanticWalker` 中针对 `BigInteger.CompareTo/Equals/ToString` 的宽泛 intrinsic 特判
- 原特判会把 `CompareTo(object)` 也误降级成 typed compare 形状，语义过宽，不应继续保留

#### 11. BigIntegerModule 的纯算术静态成员

文件：

- `src/Jazor.CLR/module/BigIntegerModule.cs`

收缩内容：

- `BigInteger.Add(BigInteger, BigInteger)` -> `Inline`
- `BigInteger.Subtract(BigInteger, BigInteger)` -> `Inline`
- `BigInteger.Multiply(BigInteger, BigInteger)` -> `Inline`
- `BigInteger.Negate(BigInteger)` -> `Inline`

本轮刻意不动：

- `BigInteger.Divide(BigInteger, BigInteger)`
- `BigInteger.Remainder(BigInteger, BigInteger)`
- `BigInteger.DivRem(...)`

原因：

- `+ / - / * / unary -` 都是稳定单表达式，没有 helper、异常协议或临时值
- `Divide/Remainder` 虽然表面上也像单表达式，但除零异常协议和 runtime 错误形状还值得单独核对
- 先收最无争议的一层，保持“稳定优先”

#### 12. BigIntegerModule 的除法族成员

文件：

- `src/Jazor.CLR/module/BigIntegerModule.cs`

收缩内容：

- `BigInteger.Divide(BigInteger, BigInteger)` -> `Inline`
- `BigInteger.Remainder(BigInteger, BigInteger)` -> `Inline`
- `BigInteger.DivRem(BigInteger, BigInteger, out BigInteger)` -> `Import`

保留内容：

- `BigInteger.DivRem(BigInteger, BigInteger)` 继续保留现有 `Import`

原因：

- `Divide/Remainder` 与已允许的 `BigInteger.operator /`、`BigInteger.operator %` 属于同一底层运行时语义，直接落表达式最稳定
- `DivRem(out ...)` 需要 `[returnValue, outValue]` 回写约定，不适合硬塞进 `Inline`
- 这里继续沿用 BigInteger 现有的 JS BigInt 除零错误形状，即 `RangeError("Division by zero")`，不额外引入与当前模块不一致的异常包装

#### 13. BigIntegerModule 的 MaxMagnitude / MinMagnitude

文件：

- `src/Jazor.CLR/module/BigIntegerModule.cs`

收缩内容：

- `BigInteger.MaxMagnitude(BigInteger, BigInteger)` -> `Import`
- `BigInteger.MinMagnitude(BigInteger, BigInteger)` -> `Import`

原因：

- 真实 .NET 语义是“先比较绝对值；绝对值相同再按数值大小决胜”
- 这类规则虽然能勉强写成 `Inline`，但会变成重复绝对值展开的长条件表达式，可读性和稳定性都变差
- 保留成小型 helper 更稳，也更容易和运行时语义刻画测试对齐

#### 14. Int64Module 的 MaxMagnitude / MinMagnitude tie-break 修正

文件：

- `src/Jazor.CLR/module/Int64Module.cs`

修正内容：

- `long.MaxMagnitude(long, long)` 绝对值相同时改为返回数值更大的那个
- `long.MinMagnitude(long, long)` 绝对值相同时改为返回数值更小的那个

原因：

- 旧实现对 `|x| == |y|` 的情况直接偏向左值，这和真实 .NET 语义不一致
- `long` 当前仍保留为 `Import` helper，修这里最小、最稳，不需要调整编译器消费路径
