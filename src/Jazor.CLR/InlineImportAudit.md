# Inline / Import 复审记录

## 目标

这份文档只回答一个问题：

> 在当前 Jazor 编译器实现下，哪些 `Op.Import` 其实应该降回 `Op.Inline`，哪些应该继续保留 `Import`，哪些更适合未来走 `Op.Compile`。

这里的判断标准不是“尽量贴近 CLR 内部语义实现方式”，而是：

- 生成结果是否稳定等价
- 是否会给第三方观察语义引入歧义
- 是否会把本应在编译期完成的语法糖处理错误地下沉到运行时模块

本次复审覆盖：

- `src/Jazor.CLR/module/*.cs`
- `src/ECMAScript/internal/*.cs`

结论先写在前面：

- `Alias > Inline > Compile > Import`
- 能稳定写成 `Inline` 的，不要继续挂 `Import`
- 但不要为了“去 Import”而手写脆弱模板
- 只要涉及 tuple 运行时形状、复杂校验、循环、多步副作用、模块 helper 复用，就不要强行塞回 `Inline`

## 判定规则

### 1. 什么时候优先 `Inline`

满足以下条件时，优先 `Inline`：

- 最终可以表达成单个 JS expression
- 不依赖本模块私有 helper 或另一模块的运行时 helper
- 不需要循环
- 不需要显式 `throw`
- 不需要临时变量保存中间状态
- 不需要手写 tuple 运行时对象形状
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
- 这类逻辑更适合未来 `Compile` 直接产 AST

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
- 不能在 `Inline` 模板里手写 tuple 运行时对象形状
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

#### 3. 依赖循环或范围处理的集合方法

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

#### 4. `string.Compare` / `string.Format`

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

### 第二批：等 `Op.Compile` 接线后再处理

优先做：

1. 带 `throw` 的索引器 / `Add`
2. `HashSet.Add`
3. `MaxMagnitude` / `MinMagnitude`
4. `SinCos` / `SinCosPi`

## 当前结论

按“能用 `Inline` 就不要用 `Import`”这条规则看，当前最应该收缩的不是 `Parse`、`decimal`、集合范围算法，而是：

- `StringBuilder` 的几个简单成员
- `float` / `double` 的 typed `CompareTo` / `Equals`
- `CopySign`

而 tuple 结果构造、带异常分支的索引器、依赖临时值的表达式，则应明确归到未来 `Compile`，不要继续让 `Inline` 模板承担结构上不合适的职责。
