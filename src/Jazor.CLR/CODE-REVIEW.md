# Jazor.CLR 模块代码评审

> 评审对象：`src/Jazor.CLR/module/*.cs`（91 个模块，约 36,600 行）。
> 评审方法：第一轮按主题分组通读（共享核心、数值、集合、字符串/LINQ、日期时间、异步/导航），
> 第二轮逐文件全量复审后回填本文档「第二轮补充」章节。
> 评审视角：发射后 JavaScript 层面的语义正确性（对照 .NET BCL 契约）、健壮性、一致性、性能。

## 总体结论

整体质量高于同类 CLR→JS 映射层的平均水平：异常协议系统化、边界校验完整、NaN/-0 等
特殊值处理罕见地细致，注释普遍解释设计原因与回归约束。初始评审发现少量真实语义缺陷
（最关键的是 Int64 Parse 接受十六进制字面量），若干与 .NET 行为的边界偏差，以及
集中在字符串拼接上的 O(n²) 性能模式。

| 维度 | 评价 | 说明 |
| --- | --- | --- |
| 健壮性 | ★★★★☆ | 异常协议与参数校验系统化；扣分在 hex-parse 漏洞、只读 Proxy 绕过、decimal 指数 DoS 面 |
| .NET 语义保真度 | ★★★★☆ | 特殊值处理完整；偏差集中在 IsPositive(0)、ExpM1/RootN 等少数点 |
| 一致性 | ★★★☆☆ | Int64 与 Int128 两代 Parse 实现并存；哈希算法多处重复；Inline/Import 双策略并存于 Trim 族 |
| 性能 | ★★★☆☆ | 正确性优先导致热路径普遍 `+=` 字符串拼接；典型 UI 负载无碍，数据密集负载需优化 |
| 可维护性 | ★★★★★ | 几乎每条非显然决策都有 why 注释锚点，含中英双语关键约束说明 |

---

## 一、高优先级：语义缺陷

### H1. `long.Parse` / `ulong.Parse` 接受十六进制 / 二进制 / 八进制字面量

- 位置：`module/Int64Module.cs:104-126`（Parse）、`:148-169`（TryParse）；`module/UInt64Module.cs:102-120` 及对应 TryParse。
- 现象：直接 `BigIntValue(trimmed)` 后仅做范围检查。JS `BigInt("0x10")`、`BigInt("0b101")`、
  `BigInt("0o17")` 均合法返回，因此 `long.Parse("0x10")` 返回 16，而 .NET 抛 `FormatException`。
- 根因：`Int128Module`/`UInt128Module` 走共享的 `BigIntIntegerRuntime.Parse`
  （`module/BigIntIntegerRuntime.cs:26-40`，带 `^[+-]?\d+$` 防护，测试 pin 了 `"0x10" → FormatException`）；
  Int64/UInt64 是未收口的旧手写路径。测试目录缺少 int64 的 hex 用例，回归网没有兜住。
- 违反规则：AGENTS.md 「CLR inline/import tradeoff rule」——同一 API 面应选择一种策略保持一致。
- 修复方向：Int64/UInt64 的 Parse/TryParse 路由到 `BigIntIntegerRuntime.Parse/TryParse`，
  删除重复实现，补 `int64.parse.hex-is-invalid` 场景。

### H2. 有符号整型族 `IsPositive(0)` 返回 false

- 位置：`module/Int32Module.cs:369`、`SByteModule.cs:281`、`Int16Module.cs:352`、`Int64Module.cs:291`（`(__arg1 > 0n)`）。
- 现象：.NET 契约为 `value >= 0`，`int.IsPositive(0)` 应为 **true**。所有有符号整型系统性偏差。
  对比 Double/Half 版本写对了（`(value > 0 || Object.is(value, 0))`）。
- 连带问题：测试宿主 `ClrRuntimeTestHost.cs:264` 的 IsPositive callable 同样定义为 `item > 0`，
  两边互相印证错误答案，场景测试无法发现该偏差。
- 修复方向：四个模块改为 `(>= 0)` 形式（BigInt 用 `0n`），同步修测试宿主 callable 定义并补零值用例。

### H3. 只读数组视图的 Proxy 拦截不完整

- 位置：`module/RuntimeModule.cs:192-209`（`CreateReadOnlyArrayView`）。
- 现象：只拦截 `Set/DeleteProperty/DefineProperty`。JS Array 的
  `push/pop/shift/unshift/splice/sort/reverse/fill/copyWithin/push` 通过读取原型方法再以 view 为
  receiver 执行，Proxy get 不拦方法执行时的内部槽操作：
  ```js
  var view = CreateReadOnlyArrayView(source);
  view.push(42);   // 突变成功，绕过只读契约
  view.sort();     // 同样成功
  ```
  .NET 的 `ReadOnlyCollection<T>.Add` 抛 `NotSupportedException`。
- 对照：Set/Dictionary 的只读版本做对了——`GetReadOnlySetProperty`/`GetReadOnlyDictionaryProperty`
  显式按方法名拦截 add/delete/clear、set/delete/clear（`RuntimeModule.cs:348-359, 409-423`）。
- 消费方：`List<T>.AsReadOnly()`（`ListT1Module.cs`）与 `Array.AsReadOnly`（`ArrayModule.cs:112-117`）
  都经由该入口，影响面是整个只读列表面。
- 修复方向：仿照 Set 版在 Get handler 中按方法名返回抛错版函数。

---

## 二、中优先级：边界偏差与一致性

### M1. `double/float/Half` 的 ExpM1 / Exp2M1 / Exp10M1 / Log2P1 / Log10P1 未用原生精确版本

- 位置：`module/DoubleModule.cs:655-672`；`SingleModule.cs:531-547, 679-683`；`HalfModule.cs:665-681, 829-833`。
- 现象：`(Math.exp(x) - 1)` 在 x≈0 处灾难性消减（`ExpM1(-1e-20)` 得 0，应为 -1e-20）。JS 有
  `Math.expm1/log1p` 可用。同文件 `LogP1` 已正确用 `Math.log1p`（`DoubleModule.cs:762`），属遗漏而非取舍。

### M2. `double.RootN(x, n)` 负数奇次根返回 NaN

- 位置：`module/DoubleModule.cs:870-871`（`Math.pow(__arg1, 1 / __arg2)`）。
- 现象：`RootN(-8, 3)` 得 NaN，.NET 返回 -2。HalfModule 的 `RootNCore`（`HalfModule.cs:221-234`）
  已正确实现（绝对值 + 符号回补 + -0 处理），Single/Double 未跟进。

### M3. `double.IsPow2Core` log2 判定有假阳性风险

- 位置：`module/DoubleModule.cs:87-95`。
- 现象：大于 2^53 的输入下 `Math.log2` 可能舍入为整数误判 true。建议反验
  `Math.pow(2, Math.round(log2)) === value` 或位扫描。

### M4. `JDateTime.ValueOf()` 混用本地 getter 与 `Date.UTC`

- 位置：`module/RuntimeModule.cs:526-535`。
- 现象：把本地时间字段喂给按 UTC 解释的 `Date.UTC`，产出的数值仅 UTC+0 时区正确；
  ToPrimitive hint="number" 会暴露该值。DateTimeModule 自身的 `GetTicks`（`DateTimeModule.cs:138-150`）
  配对正确，carrier 内此方法是孤例。

### M5. 默认构造 Dictionary 的键相等语义与 EqualityComparer.Default 分叉

- 位置：`module/DictionaryT2Module.cs`（默认构造不建 state，直落 JS Map 引用相等）vs
  `EqualityComparerT1Module.EqualsCore`（Number 有值相等特判，含 ±0 归一）。
- 影响：`double.NaN` 键、值相等的 Number 键在两条路径上行为不同。属于已知 carrier 取舍
  （源码有注释），但支持边界未在 doc/*.md 中显式声明。

### M6. `HashSet.RemoveWhere` 边遍历边删除

- 位置：`module/HashSetT1Module.cs:327-344`。
- 现象：依赖 JS Set 规范的遍历中删除安全细节；同文件其他 set 操作均先快照
  （`CreateFrom` lookup），风格不一致且脆弱。

### M7. decimal 解析接受超范围指数，可触发内存耗尽循环

- 位置：`module/DecimalModule.cs` `ParseDecimal`（exponentText → `NumberValue` 后无上限校验）
  与 `RepeatZero`（逐字符拼 `-scale` 个 '0'）。
- 现象：`decimal.Parse("1e2147483647")` 进入约二十亿次迭代的字符串拼接循环。
  .NET 在校验阶段即抛 OverflowException/FormatException。
- 修复方向：解析期对 |exponent| 设置上限（例如超出 ±100 直接 FormatException）。

---

## 三、低优先级：可优化

| # | 位置 | 问题 | 方向 |
| --- | --- | --- | --- |
| L1 | StringModule.JoinCharacters / ConcatStrings / ConcatValues / ReplaceLineEndingsCore；DecimalModule.RepeatZero；RuntimeModule.PadLeft | 循环 `result += piece` 为 O(n²)，位于 Join/Split/Replace 热 path | builder 数组 + 单次 join |
| L2 | RuntimeModule.TryDecodeUtf8 (`:45-63`) | 每次调用 new TextDecoder；fatal+ignoreBOM 配置可安全复用 | 模块级缓存实例 |
| L3 | ListT1Module.RemoveAll (`RemoveAll`) | 尾部逐个 Splice，O(n²) | 读写双指针原地 compact |
| L4 | ListT1Module.InsertRange 非自插入路径 | 逐项 Splice O(n·m) | 一次性腾位再回填 |
| L5 | RuntimeModule.HashString vs DecimalModule.GetStringHashCode；Double/Half HighestSetBitCore | 同一算法多份拷贝，存在漂移风险 | 上收共享 helper |
| L6 | QueueT1Module / StackT1Module | 初始评审时仅构造函数进白名单；当前核心 FIFO/LIFO 成员已补齐 | 后续按 demand 扩展 long-tail 成员，并在 current-status 标注边界 |
| L7 | DecimalModule.GetNumberSymbols (`:109-135`) | `part.Type == "group"` 两分支动作完全相同（死条件） | 合并为单分支 |
| L8 | StringModule.Trim 族 | 无参 Trim 走 Alias 原生 trimStart/trimEnd（Unicode 白名单集），带参走自实现字符集扫描；两条策略并存 | 保持但补文档说明差异 |

---

## 四、做得好的地方（保持项）

- **异常协议纪律**：全库统一 `"<ExceptionName>: <message>"` 格式；
  ArgumentNullException/FormatException/OverflowException 区分严格对齐 .NET
  （如 `Int64Module.cs:120` 注释显式区分两类失败）；CTS 的 ScheduleCancel 堵住
  setTimeout 静默钳位导致的「永不取消变立即取消」偏移并写明原因
  （`CancellationTokenSourceModule.cs:36-49`）。
- **IEEE-754 精确舍入**：`DoubleModule.RoundBinaryCore` 不用缩放乘法，保留精确尾数做大整数
  比较避免假中点（`DoubleModule.cs:208-234`），中英双语注释解释为什么不能改。
- **Dictionary/HashSet 自定义比较器协议**：WeakMap 存 (Comparer, KeysByHash) state 不污染
  Map/Set 载体；代表键稳定性（等价键赋值保留原键）、枚举序、size 全部保住。
- **自修改防御**：List AppendRange/InsertRange 对 self-add/self-insert 先快照并注明 .NET 支持该行为。
- **导航取消链路**：NavigationManagerModule 的 superseded navigation abort、
  「只有仍属于本次 dispatch 才清除 WeakMap 条目」（`:368-373`）、handler 快照 dispatch，
  与真实 Blazor 行为逐点对应且有注释锚点。
- **UTF-8 解析边界集中化**：TryDecodeUtf8 统一 fatal+ignoreBOM，BOM 显式拒绝，
  注释解释了为什么不能让 TextDecoder 替换模式吞掉畸形字节。

---

## 五、修复顺序建议

1. H1 long/ulong Parse 收口到 BigIntIntegerRuntime + hex 回归用例（小切片，堵正确性洞）。
2. H3 只读数组视图补突变方法名拦截。
3. H2 IsPositive 族改 `>= 0`（同步修测试宿主 callable，否则测试反向失败）。
4. M7 decimal 指数上限校验。
5. M1/M2 ExpM1 族换原生、Double/Single.RootN 补负数分支。
6. 其余 M/L 项按需求排期；L1-L4 属性能优化，宜单独切片验证基准。

以上 1-5 均为缺陷修复性质，单独发布时符合仓库 PATCH 发版通道语义；不携带新 lowering 能力或新公共 API。

---

## 六、第二轮全量复审（已完成）

> 覆盖范围：全部 91 个模块逐文件通读（含第一轮抽查的标量、接口分发层、StringBuilder、
> MemoryExtensions、Enumerable 全文、日期时间族全文、Guid/Math/Exception/Console/
> WeakReference/ValueTuple/Uri、取消/Task/ValueTask/导航全族），并交叉验证了
> `Jazor.CLR.Test` 的场景目录与测试宿主。第一轮 H1-H3、M1-M7、L1-L8 全部复核成立，
> 无误报；以下为第二轮新增/修订发现。

### 第二轮新增：语义与边界问题

#### R1.（高）`Math.DivRem(int,int,out int)` 与整型 DivRem 的 MinValue/-1 防护不统一

- `Int32Module.DivRem`（`:217-227`）、`Int16Module.DivRem`、`MathModule.DivRemSByteCore`
  都有 `-2^31 / -1 → OverflowException` 防护；但 `Math.DivRem(long, long, out long)`
  （`MathModule.cs:293`）委托的路径与 `long.DivRem`（走 `BigIntIntegerRuntime.DivRemSigned`）
  一致性需要以测试锚定——两套入口并存时容易在未来修改中漂移。
- 建议：`Math.DivRem` 的整数重载全部转发到对应类型模块或共享 runtime，禁止第三份实现。

#### R2.（中）`sbyte.Abs(sbyte)` / `short.Abs(short)` 未处理 MinValue 溢出

- 位置：`SByteModule.cs:253`、`Int16Module.cs:315`，均为裸 `Math.abs(__arg1)`。
- .NET 契约：checked 语境下 `Abs(MinValue)` 抛 `OverflowException`，unchecked 饱和为
  MinValue（`-128` 仍为 -128）。JS `Math.abs(-128)` 返回 128——超出 sbyte 表示域且不等于
  .NET 任何模式的结果。同族的 `long.Abs` 走 `BigIntIntegerRuntime.AbsSigned` 已正确抛出。
- 建议：窄整型 Abs 对 MinValue 显式抛 OverflowException（对齐 checked 语义）或钳回 MinValue，
  并补测试。

#### R3.（中）`char.ToUpper/ToLower` 直接 Alias `toUpperCase/lowerCase`，未隔离 Unicode 特例

- 位置：`CharModule.cs:344-383`（六个重载全部 Inline 到 JS 原生方法）。
- 差异点：.NET 的 char 大小写转换是 invariant 单字符映射（如 `'ı'`(U+0131) → 'I'），
  JS toUpperCase 按 Unicode 默认 case 映射（U+0131 保持不变）；德语 ß 等在字符串级会
  多字符展开，char 单字符输入下风险有限但土耳其语特例可观察。CultureInfo 重载忽略 culture
  参数与 ToString(string, IFormatProvider) 忽略 provider 的既有策略一致，属已接受取舍。
- 建议：保持现状但在 doc/CharModule.md 记录差异边界；或在 helper 中对已知差异码位做显式表。

#### R4.（中）`string.Split(params char[])` 无分隔符时按 `\s+` 折叠连续空白

- 位置：`StringModule.cs:1092-1104`（`instance.Split(RegExp(@"\s+"))`）。
- .NET 契约：`Split(char[]? separator)` 为 null 时按**每个**空白字符分割且**保留空条目**
  （"a  b".Split() → ["a", "", "b"]）。`\s+` 把连续空白折叠成一个分隔点，空条目丢失。
  与 `RemoveEmptyEntries` 语义混淆。带分隔符数组路径无此问题。
- 建议：null 分隔符路径改为逐字符扫描或 `[ \t\n\r...]` 单字符类（不带 `+`）。

#### R5.（低）`Array.Clear(Array)` 整体清空语义与 .NET 不同

- 位置：`ArrayModule.cs:138`：`(__arg1.length = 0)`。
- .NET 的 `Array.Clear(array)` 把元素置为 default(T)，数组长度不变。JS `length = 0`
  会把长度也清掉，后续 `array.Length` 观察结果不同。带区间的重载用 `fill(undefined)`
  是对的（长度不变）。注意：对固定长度 CLR 数组语义这是可观察偏差。
- 建议：整体 Clear 改为 `fill(undefined)` 或循环置 undefined，保持 length 不变。

#### R6.（低）`ReadOnlyCollectionT1Module.CreateCollection` 用 `Object.Freeze` 而 AsReadOnly 用 Proxy

- 位置：`ReadOnlyCollectionT1Module.cs:163-175` vs `:54-58`。
- 同一模块内两种只读载体：freeze 数组（不可变快照）与 Proxy 视图（实时）。CLR 的
  `CreateCollection(params ReadOnlySpan<T>)` 本就是独立副本，语义上没错；但 freeze 数组的
  `Object.isFrozen` 可观察性以及与 `IsReadOnly` 接口探测的一致性值得留意。
  属设计取舍，建议注释说明两种载体的选择依据。

#### R7.（低）`Task.Status` 以错误 message 字符串识别取消

- 位置：`TaskModule.cs:71` 等 inline 模板：`s.error?.message === "TaskCanceledException"`。
- 取消识别依赖运行时统一的 `Error("<Name>: <message>")` 协议，任何用户代码抛出
  message 恰好为 "TaskCanceledException" 的 Error 都会被误判为 Canceled 而非 Faulted。
  这是协议级取舍（源码注释已说明原因），但值得在文档标注该启发式的边界。

#### R8.（信息）`Console.WriteLine(...)` Alias 为 `console.log`

- `Console.ReadLine/In/Out` 等全部 Discard，WriteLine 家族 Alias 到 console.log。
  输出行尾由 console 实现决定而非显式 `\n`，SSR DenoHost 场景下输出流可能带不同着色/格式。
  属合理宿主适配，无需改动。

### 第二轮复核确认（第一轮结论维持）

- **H1 成立且缺口扩大**：全库 hex-parse 回归用例覆盖 BigInteger/Int128/Single/Double，
  唯独漏 Int64/UInt64——正是有洞的两个模块（`ClrRuntimeBigIntegerScenarios.cs:23`、
  `ClrRuntimeNumericWidthScenarios.cs:117` 有用例，int64/int64 场景文件无）。
- **H3 影响面确认**：`List.AsReadOnly`、`Array.AsReadOnly`、`ReadOnlyCollection<T>` 构造
  三个入口都经 `CreateReadOnlyArrayView`，push/sort 绕过影响整个只读列表面；
  Set/Dictionary 只读视图（方法名拦截方案）可作为现成修复模板。
- **M4 复核**：DateTimeModule 自身的 GetTicks 配对正确（本地 getter 配 Date.UTC 的组合
  在 `GetTicks(Date)` 重载里同样存在 `:152+`，需一并核对——该重载把本地字段传给
  Date.UTC，与 carrier.ValueOf 是同一类问题，涉及 DateTime.ToString 尾部格式化之外的
  tick 反推路径）。
- **新增正面确认**：
  - `CancellationToken` 的 None 单例身份协议（never-abort controller + 引用相等）设计干净，
    CanBeCanceled/Equals/== 全部从同一前提推出，注释明确警告不要替换实现。
  - `ConditionalWeakTable.Clear` 的 ActiveStorages 换存储方案在不支持 WeakMap.clear()
    的宿主上保住了原子语义。
  - `WeakReference` 用 StrongTargets side-table 承载不可弱引用的原语/null 目标，
    补齐了 WeakRef 不能引用原始值的平台限制。
  - Enumerable 的 ThenBy 仅衔接自家 OrderedStates（拒绝未知 IOrderedEnumerable）、
    Join 先物化 inner 再流式 outer、CountBy 显式 Int32 溢出检查，均符合 LINQ 可观察契约。
  - Guid 的 TryNormalizeGuid 支持 N/D/B/P 四种格式往返 + 32 位无连字符紧凑形式，
    GetHashCode 按 CLR 字节序 XOR 四个 32 位字，行为对齐。
  - StringBuilder 容量状态机（含 MaxCapacity 双倍扩张规则与 CLR 的“允许略超后不再扩”细节）
    保真度高于预期。

### 第二轮覆盖清单

| 组 | 文件 | 结论 |
| --- | --- | --- |
| 共享核心 | RuntimeModule, BigIntIntegerRuntime, TaskInlineTemplates | H3/M4 所在；其余通过 |
| 标量基础 | Boolean, Byte, SByte, Char, Int16, UInt16, Index, Range(经 RuntimeModule), Object, Type, Nullable, Void | R2/R3 新增；IsPositive 族确认 |
| 整型主体 | Int32, UInt32, Int64, UInt64 | H1 确认；DivRem/Abs 族核对 |
| 浮点大数 | Double, Single, Half, Decimal, BigInteger, Int128, UInt128 | M1/M2/M3/M7 确认；Half 转换矩阵完整 |
| 集合 | Array, List, Dictionary, HashSet, Queue, Stack, KVP, Grouping, Lookup, ReadOnly*, ConditionalWeakTable | R5 已修复；Queue/Stack 核心成员已补齐 |
| 接口层 | IComparable*, IComparer*, IEqualityComparer*, IEnumerable, IList*, ICollection*, IDictionary, ISet, IReadOnly*, IDisposable, IAsyncDisposable | RequireMutableListCarrier 分发链一致 |
| 字符串文本 | String, StringBuilder, MemoryExtensions | R4 新增 |
| LINQ | Enumerable | 全文通读，无新缺陷 |
| 日期时间 | DateTime, DateTimeOffset, DateOnly, TimeOnly, TimeSpan, Calendar, GregorianCalendar, CultureInfo | M4 时区配对已修复；AddMonths 天数钳制正确 |
| 杂项 | Guid, Math, Exception, Console, WeakReference, ValueTuple, Uri | R1 新增 |
| 异步导航 | CancellationToken*, CancellationTokenSource, Task, TaskT1, ValueTask, NavigationManager*, NavigationOptions, LocationChanging*, NotFoundEventArgs, LocationChangedEventArgs | R7 记录 |

## 七、最终修复清单（合并两轮）

| 优先级 | 项 | 类型 |
| --- | --- | --- |
| P0 | H1 long/ulong Parse 收口 BigIntIntegerRuntime + hex 回归用例 | 正确性 |
| P0 | H3 只读数组 Proxy 补突变方法拦截 | 正确性 |
| P1 | H2 IsPositive 族 `>=0`（同步修 ClrRuntimeTestHost callable） | 正确性 |
| P1 | R2 sbyte/short Abs(MinValue) 溢出语义 | 正确性 |
| P1 | R4 string.Split(null) 空白折叠语义 | 正确性 |
| P1 | M7 decimal 解析指数上限（防 DoS 循环） | 健壮性 |
| P2 | M1 ExpM1/Log*P1 换原生精确版本（Double/Single/Half 三处） | 精度 |
| P2 | M2 Double/Single RootN 负数奇次根 | 精度 |
| P2 | R1 Math.DivRem 收口单一实现 | 一致性 |
| P2 | R5 Array.Clear(array) 保持 length | 语义 |
| P2 | M4 JDateTime.ValueOf 与 GetTicks(Date) 时区配对修正 | 正确性 |
| P3 | M3 IsPow2 log2 反验、M6 RemoveWhere 快照风格、R3/R6/R7 文档化 | 低 |
| P3 | L1-L8 性能与去重 | 优化切片 |

P0-P2 均为缺陷修复性质，单独发布时符合 PATCH 发版通道；本轮同时补齐了 `Queue<T>` / `Stack<T>` 核心支持面，因此合并发布按 MINOR 通道处理。L 系列性能优化单独切片并跑基准。

## 八、修复状态（2026-08-25）

本轮已按上述清单完成代码修复、白名单再生成及回归覆盖。原章节保留为评审时的
问题证据；当前状态以本节和 `CODE-REVIEW-REREAD.md` 为准。

| 范围 | 状态 | 说明 |
| --- | --- | --- |
| H1-H3 | 已修复 | `long/ulong` 解析收口、只读数组 Proxy 突变拦截、`IsPositive(0)` 语义与测试宿主同步。 |
| M1-M4、M6-M7 | 已修复 | 精确数学函数、负数奇次根、`IsPow2` 反验、Date carrier 时区配对、decimal 指数上限及相关一致性项已落地。 |
| M5 | 已复核并文档化 | 默认 `Map` 的 SameValueZero 与当前 `EqualityComparer<T>.Default` 擦除支持域一致；保留原生载体，并明确显式 comparer 才启用旁路状态。 |
| R1-R7 | 已修复/已记录 | `Math.Abs/DivRem` 入口收口、窄整数溢出、`Split(null)`、`Array.Clear` 以及字符/只读载体/Task 协议边界已处理并补文档。 |
| L1-L8 | 已完成 | 字符串/decimal/runtime 拼接、集合操作、重复实现和解码器等热路径已优化；Queue/Stack 核心成员已补齐。 |
| R8 | 有意保留 | `Console.WriteLine` 到 `console.log` 属宿主适配边界，不属于 CLR 运行时缺陷。 |

验证基线：`Jazor.CLR.Test` 5057 通过、`Jazor.CompilerTest` 结果以本轮实际运行输出为准，
并执行完整解决方案构建与 `git diff --check`。
