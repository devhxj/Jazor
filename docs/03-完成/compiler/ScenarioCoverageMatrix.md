# Jazor Compiler 真实场景覆盖矩阵

> Status: 当前验收矩阵
> Updated: 2026-08-04
> Scope: `Jazor.Compiler`、RazorVue lowering 与 `compiler -> catalog -> emit -> Deno.host` 闭环

## 使用原则

这张表记录的是开发者可写的 C# 场景和对应的可复验入口，不用测试数量替代语义覆盖。

- `IOperation -> ESTree -> JS` 文本/AST 契约是主证据。
- 只有求值次数、控制流退出、异步顺序、事件回调和物化模块行为才使用 Deno.host。
- 每个闭环场景必须由 C# 源码驱动 compiler lowering；测试不得手拼被测 JavaScript。
- 未在表内声明的 runtime 形态不应从“覆盖率命中”推导为已支持。

## 场景矩阵

| 语义面 | 典型开发者场景 | 主证据 | 运行时证据 | 回归入口 |
|---|---|---|---|---|
| `SemanticWalker.Reference` | 字段、属性、方法组、CLR import、数组索引与 `Index`/`Range` | member/reference AST 与稳定 import 文本 | nullable receiver、materialized `Index`/`Range`、隐式 indexer 读写 | `SemanticWalkerReferenceTest`、`SemanticWalkerImplicitIndexerProtocolTests`、`SemanticWalkerRangeAndSizeOfTests` |
| `SemanticWalker.Pattern` | 常量、声明、类型、关系、属性、位置、list/slice 与 pattern switch | pattern scenario/protocol AST 和 JS 文本 | 仅在短路、绑定值或宿主形态无法由文本证明时执行 | `SemanticWalkerPatternTest`、`SemanticWalkerPatternScenarioTests`、`SemanticWalkerPatternProtocolTests` |
| `SemanticWalker.Loop` | `for` 初始化/更新、await update、`foreach` 解构、`while`/`do` | loop AST 与输出顺序 | await update、record/Map `foreach` 的 Deno 行为 | `SemanticWalkerLoopTest`、`AstConverterRuntimeClassScenarioTests` |
| `SemanticWalker.Using` | using declaration、表达式资源、return/throw、reverse disposal、`await using` | `try/finally` AST、接口/泛型选择与拒绝诊断 | 同步/异步 disposal 的退出和等待顺序 | `SemanticWalkerUsingLifetimeScenarioTests`、`AstConverterRuntimeClassScenarioTests` |
| Lambda / local binding | expression/block/anonymous delegate、`ref/out` callback、captured values、Roslyn transparent identifier、escaped keyword；`Expression<TDelegate>` 显式拒绝 | executable delegate 生成 function-body、shared ref/out return layout、合法且稳定的 binding AST/text；symbolic expression tree 不降级为箭头函数 | `ref/out` 写回、`let` query 的 transparent projection、escaped keyword lexical binding；expression-tree conversion failure | `AstConverterRefOutProtocolScenarioTests`、`SemanticWalkerBindingIdentifierTests`、`SemanticWalkerTranslatedQueryTests`、`SemanticWalkerExpressionTreeBoundaryTests` |
| Iterator artifact | module/runtime member/local `yield return`、`yield break`、`async IAsyncEnumerable<T>` | Roslyn operation tree 决定 `FunctionDeclaration` / `FunctionExpression` 的 generator/async 标志；不从返回类型或文本猜测 | 同步/异步迭代顺序与 nested function boundary | `SemanticWalkerDeclarationTest`、`AstConverterRuntimeClassScenarioTests` |
| LINQ query desugaring | `Enumerable` 的 `from`、`let`、`where`、`select`、multiple `from`、join/group continuation、left outer join、ordering、default-return terminals；`IQueryable` 明确拒绝 | `ITranslatedQueryOperation` wrapper 移除后的 bound Enumerable invocation/lambda AST 与 imports；Query provider 所需 expression tree 不模拟 | 仅在 projection/callback 链难以由文本证明时执行 Deno.host；Queryable 在 callback conversion 处失败 | `SemanticWalkerTranslatedQueryTests`、`SemanticWalkerTranslatedQuerySelectManyTests`、`SemanticWalkerTranslatedQueryJoinTests`、`SemanticWalkerTranslatedQueryGroupByTests`、`SemanticWalkerTranslatedQueryOrderingTests`、`SemanticWalkerEnumerableDefaultIfEmptyTests`、`SemanticWalkerExpressionTreeBoundaryTests` |
| 主分派与类型边界 | whitelist 命中、外部类型使用点拒绝、source origin、scope/import 合并 | boundary、whitelist、origin 与 import AST | 无需运行时的诊断以文本/异常为准 | `SemanticWalkerBoundaryTest`、`WhiteListLookupTests`、`SemanticWalkerSourceOriginTest` |
| `AstConverter` | 模块 export/import、字段初始值、runtime class、继承和构造函数选择 | module AST、导出名与 source map 文本 | class 属性、nullable/default、delegate、loop、using 等真实模块 | `AstConverterTests`、`AstConverterBoundaryScenarioTests`、`AstConverterRuntimeClassScenarioTests` |
| RazorVue compiler host | current component state/props、EventCallback、`@bind`、`StateHasChanged`、`InvokeAsync` | official Razor SG C# 到 render-function `.mjs` 文本 | 事件回写、异步 setter、nested runtime class receiver | `CurrentComponentSemanticWalkerHostTest`、`RazorSgOfficial*RuntimeTests`、`RazorVueSemanticMatrixInventoryTests` |
| catalog / emit / runtime | CLR modules、SDK consumer、最终 `.mjs` 与 Deno import map | manifest、catalog 和稳定模块 import | materialized `Index`/`Range`、implicit indexer、query-syntax `Join`/`GroupJoin`、外部 Razor SG consumer | `SdkIntegrationTests`、`ClrRuntimeCatalogReaderTests`、`RazorSgOfficialDenoRuntimeTestHost` |

## 关键闭环断言

### Materialized `Index` / `Range`

- `System.Index` 与 `System.Range` 可跨 local、argument 与 return 使用。
- 数组以及具备 `Length`、`this[int]`、`Slice(int, int)` 的隐式 indexer 走 Roslyn 已绑定的协议，而不是猜测 JavaScript 对象形状。
- `Index` 的读取和复合赋值保持 receiver、getter、setter 的单次求值；`Range` 先计算 `(offset, length)`，再按目标协议投影为切片参数。
- `Enumerable.Take<TSource>(IEnumerable<TSource>, Range)` 复用同一 `JRange.GetOffsetAndLength` protocol：eager materialization 后按 checked offset/length 返回新 Array，支持 from-start/from-end、`Range.All`、inverted/越界失败和 source 不变性，而不伪造延迟 iterator。
- `SdkIntegrationTests.Build_LocalJazorPackage_StoredIndexAndRange_ExecutesMaterializedRuntimeOnDenoHost` 直接执行物化后的 catalog/runtime 模块，覆盖切片、复合赋值和越界异常。
- `SemanticWalkerEnumerableTakeRangeTests` 断言 C# range authoring 绑定、import 与 AST/text；`ClrRuntimeEnumerableTakeRangeScenarios` / `EnumerableTakeRangeRuntimeTests` 经 generated catalog 和 Deno.host 覆盖四类边界、异常与独立结果 carrier。

### Numeric terminal `Min` / `Max`

- non-nullable 与 nullable `IEnumerable<int>`、`IEnumerable<long>`、`IEnumerable<float>`、`IEnumerable<double>` 和 `IEnumerable<decimal>` 的 `Min` / `Max`，以及同结果类型 `Func<TSource, TResult>` selector overload，均通过 Roslyn 已绑定的精确签名进入 `EnumerableModule`，各自导入稳定 export；不按 JavaScript `Number` 或字符串进行跨 carrier 合并。
- `int` 保持 Number 比较，`long` 保持 BigInt，decimal 走现有 decimal 比较操作。所有已支持 overload 对 null source 抛出 `ArgumentNullException`；non-nullable 空 source 抛出 `InvalidOperationException`，direct nullable overload 的空/全 null source 返回 null。
- selector 按 source order 对每项恰好调用一次，并复用其结果类型专用 comparer：float/double 的 NaN 行为与当前 BCL 一致，`Min` 在观察到 NaN 时返回 NaN；`Max` 跳过 `NaN`，若所有非 null 值皆为 `NaN` 则保留 NaN。nullable selector 同样忽略 null 并在没有有效值时返回 null；decimal 仍经 decimal comparer 而非字符串排序。comparer overload 仍不在此切片内。
- `SemanticWalkerEnumerableMinMaxTests`、`SemanticWalkerEnumerableNullableMinMaxTests`、`SemanticWalkerEnumerableNumericSelectorTests` 与 `SemanticWalkerEnumerableNullableNumericSelectorTests` 验证 Roslyn-bound overload、import、lambda AST/text 与语法；`ClrRuntimeEnumerableMinMaxScenarios` / `EnumerableMinMaxRuntimeTests`、`ClrRuntimeEnumerableNullableMinMaxScenarios` / `EnumerableNullableMinMaxRuntimeTests`、`ClrRuntimeEnumerableNumericSelectorScenarios` / `EnumerableNumericSelectorRuntimeTests` 以及 `ClrRuntimeEnumerableNullableNumericSelectorScenarios` / `EnumerableNullableNumericSelectorRuntimeTests` 通过 generated catalog 和 Deno.host 验证 carrier、NaN、decimal 数值顺序/scale、null element、all-null、selector 顺序与 null source。

### Numeric terminal `Sum`

- 非 nullable 与 direct nullable `IEnumerable<int>`、`IEnumerable<long>`、`IEnumerable<float>`、`IEnumerable<double>`、`IEnumerable<decimal>` numeric `Sum` overload 通过 Roslyn 已绑定的精确签名进入 `EnumerableModule`，每个 carrier 导入独立稳定 export，不将数值宽度统一压缩为 JavaScript Number。
- `int` 与 `long` 在每次累加前分别检查 Int32 / Int64 边界，前者保留 Number、后者保留 BigInt。float 以 Number 累加并在返回时单次 `Math.fround`，double 直接保留 Number，二者均保持 `NaN` 传播。
- decimal 不使用 `+` 或字符串顺序近似，而是经已映射的 `decimal.Parse` 和 `decimal.Add` 维持 exact decimal carrier；空 source 对所有已支持 overload 返回相应零值，null source 抛出 `ArgumentNullException`。nullable direct 和 selector overload 都忽略 null，并在空/全 null source 返回带零值的 `T?`，仍检查 Int32 / Int64 / decimal 累加溢出；decimal selector Sum 保留参与项的最大 scale。同一五种 non-nullable 和 nullable `Func<TSource, TResult>` selector overload 复用精确 carrier core，逐项按 source order 恰好调用一次 selector；null selector 明确失败。
- `SemanticWalkerEnumerableSumTests`、`SemanticWalkerEnumerableNullableNumericTests`、`SemanticWalkerEnumerableNumericSelectorTests` 与 `SemanticWalkerEnumerableNullableNumericSelectorTests` 验证 Roslyn-bound overload、import、lambda AST/text 与语法；`ClrRuntimeEnumerableSumScenarios` / `EnumerableSumRuntimeTests`、`ClrRuntimeEnumerableNullableNumericScenarios` / `EnumerableNullableNumericRuntimeTests`、`ClrRuntimeEnumerableNumericSelectorScenarios` / `EnumerableNumericSelectorRuntimeTests` 以及 `ClrRuntimeEnumerableNullableNumericSelectorScenarios` / `EnumerableNullableNumericSelectorRuntimeTests` 经 generated catalog 和 Deno.host 覆盖 carrier、精度、`NaN`、null element、空 source、null source、selector 顺序与 overflow。

### Numeric terminal `Average`

- 非 nullable 与 direct nullable `IEnumerable<int>`、`IEnumerable<long>`、`IEnumerable<float>`、`IEnumerable<double>`、`IEnumerable<decimal>` numeric `Average` overload 通过 Roslyn 已绑定的精确签名进入 `EnumerableModule`，每个 numeric return contract 具有独立稳定 export。
- `int` 与 `long` 均以 checked Int64 BigInt accumulator 计算，随后显式转换到 Number 返回 double；因此 `int.MaxValue` pair 可平均，而两个 `long.MaxValue` 仍按 BCL 的累加语义溢出。空 source 抛出 `InvalidOperationException`，null source 抛出 `ArgumentNullException`。
- float / double 使用宽 Number accumulation 并传播 `NaN`，float 仅在 final result `Math.fround`。decimal 经 `decimal.Parse` / `decimal.Add` / `decimal.Divide` 映射保持 exact carrier；最终平均值可表示不会掩盖累加阶段的 decimal overflow。nullable direct 和 selector overload 忽略 null，且在没有有效元素时返回 null；`int?` / `long?` 维持 `double?` Number result，float? / double? / decimal? 保留各自 nullable result carrier。同一五种 non-nullable 和 nullable `Func<TSource, TResult>` selector overload 复用相同 accumulator 与返回 contract，逐项按 source order 恰好调用一次 selector；null selector 明确失败。
- `SemanticWalkerEnumerableAverageTests`、`SemanticWalkerEnumerableNullableNumericTests`、`SemanticWalkerEnumerableNumericSelectorTests` 与 `SemanticWalkerEnumerableNullableNumericSelectorTests` 验证 Roslyn-bound overload、import、lambda AST/text 与语法；`ClrRuntimeEnumerableAverageScenarios` / `EnumerableAverageRuntimeTests`、`ClrRuntimeEnumerableNullableNumericScenarios` / `EnumerableNullableNumericRuntimeTests`、`ClrRuntimeEnumerableNumericSelectorScenarios` / `EnumerableNumericSelectorRuntimeTests` 以及 `ClrRuntimeEnumerableNullableNumericSelectorScenarios` / `EnumerableNullableNumericSelectorRuntimeTests` 经 generated catalog 和 Deno.host 覆盖 fractional result、carrier、precision、`NaN`、null element、empty/null source、selector 顺序与 overflow。

### Materialized `GroupBy` result selectors

- `Enumerable.GroupBy<TSource, TKey, TResult>` 与 `GroupBy<TSource, TKey, TElement, TResult>` 通过 Roslyn 已绑定的精确 BCL signature 进入 `EnumerableModule.groupByResult` / `groupByElementResult`。用户侧 result lambda 继续接收 `IEnumerable<TSource>` 或 `IEnumerable<TElement>`；adapter 内部将它稳定投影为已有 `IGrouping<TKey, TElement>` Array carrier，而不是构造弱类型 wrapper。
- source key/element selectors 先按 source order 完整执行，并复用 `EqualityComparer<TKey>` bucket 与首 key metadata；只有全部 group 物化完成后才按 group 的首次出现顺序调用 result selector。null source、key/element/result selector 均在 source 枚举前明确失败，空 source 不调用 result selector，输入不变。
- `SemanticWalkerEnumerableGroupByResultTests` 断言两条 bound signature、nested lambda AST/text、imports 与 JavaScript 语法；`ClrRuntimeEnumerableGroupByScenarios` / `EnumerableGroupByRuntimeTests` 通过 generated catalog 和 Deno.host 验证 group result、element projection、selector phase order、null argument、空 source、首 key 及 source 不变性。

### Materialized `ToLookup`

- `Enumerable.ToLookup<TSource, TKey>` 与 element selector overload 进入同一 `GroupBy` equality/layout core；`ILookup<TKey, TElement>` 在 runtime 是带有 `GroupingT2Module` key metadata 的 outer Array，不使用 JavaScript `Map` 作为 CLR lookup。
- `ILookup.Count`、`Contains(TKey)`、indexer getter 均通过精确 member mapping 进入 `EnumerableModule`。查询使用 `EqualityComparer<TKey>`，因此 `NaN`、有符号零与 GroupBy/Join/Distinct 一致；缺失 key indexer 返回新的空 Array。当前不支持 comparer overload。
- `SemanticWalkerEnumerableLookupTests` 验证两条 ToLookup signature、ILookup type alias、Count/Contains/indexer imports 和 AST/text；`ClrRuntimeEnumerableLookupScenarios` / `EnumerableLookupRuntimeTests` 通过 generated catalog 和 Deno.host 覆盖 key/element selector、metadata、NaN/有符号零、缺失 key、null argument 和 source 不变性。

### Materialized LINQ `Join` / `GroupJoin`

- query syntax 经 Roslyn `ITranslatedQueryOperation` 展开后复用已绑定的 `Enumerable.Join` / `GroupJoin` invocation 与 lambda lowering；不另建 query-string 或 JavaScript fallback 路径。
- `Join` 的验证覆盖 outer source 顺序、inner 同 key 重复项顺序，以及无匹配 outer 项不产生结果。
- `GroupJoin` 的验证覆盖每个 outer 项均调用 result selector，无匹配项传入空分组。
- `SdkIntegrationTests.Build_LocalJazorPackage_QuerySyntaxWithCapturedLambda_ExecutesOnDenoHost` 用本地 SDK 包构建 consumer，断言 manifest 包含 `EnumerableModule` 与 `EqualityComparerT1Module`，再由 Deno 执行最终 materialized `.mjs`。

### Materialized Multiple `from`

- 双 `from` query 由 Roslyn 展开为带 result selector 的 `Enumerable.SelectMany`；where predicate、collection selector 与 result selector 都继续经过已绑定的匿名函数 lowering。
- `SdkIntegrationTests.Build_LocalJazorPackage_QuerySyntaxWithCapturedLambda_ExecutesOnDenoHost` 以 threshold 和 offset 捕获值验证过滤、内层展开、透明标识符投影、outer/inner 顺序及 source 不变性。

### Lambda `ref/out` 与合法 binding

- 匿名方法和 lambda 的 `ref` / `out` 参数与 local/module method 共用 `RefOutReturnProtocol`：非 void 按 `[result, ref1, ...]` 返回，void 按 `[ref1, ...]` 返回；delegate invoke 继续由既有 caller 端按同一索引写回。不会为 delegate 另建 JavaScript runtime 类型或数组以外的协议。
- Roslyn 为 query `let` 生成的 transparent identifier 以及 C# escaped keyword（例如 `@class`、`@await`）会通过 symbol + source span 的稳定 hash 投影成合法 JS binding；普通合法作者名称不改写，产物不依赖 checkout path。
- `AstConverterRefOutProtocolScenarioTests` 分别检查 AST return/caller write-back layout，并由 Deno.host 验证 anonymous `ref` return 与 lambda `out` 的最终结果。`SemanticWalkerBindingIdentifierTests` 跨两个不同 source path 比较输出，并由 Deno.host 验证 escaped keyword 的 lexical binding。

### Module / runtime class iterator artifacts

- `yield return` 和 `yield break` 先在 `SemanticWalker` 形成 ESTree `YieldExpression` / `ReturnStatement`，再由共享 Roslyn operation traversal 向 module `FunctionDeclaration`、runtime member `FunctionExpression` 与 local function 传播 generator flag；因此同步 iterator 输出真实 `function*`，不再留下普通函数中的非法 `yield`。
- `async IAsyncEnumerable<T>` 同时保留 symbol 的 async flag，输出 `async function*`；同步和异步 iterator 都不依赖返回类型名称、注释或 JavaScript 文本后处理。
- traversal 在 anonymous/local function 边界停止，即子 iterator 的 `yield` 不会让包含它的普通 module/member method 变成 generator。`AstConverterRuntimeClassScenarioTests.ConvertModule_IteratorMethods_DeclareSyncAndAsyncGeneratorArtifacts` 断言 module/member AST/text 可解析，并通过 Deno.host 验证同步/异步序列顺序和该边界。

### `for` 控制变量闭包捕获

- C# `for` 声明的每个控制局部变量在整个循环内都是单一 lexical binding；JavaScript 的 `for (let ...)` 则为每轮迭代建立新的 binding。仅当 Roslyn `IForLoopOperation.Locals` 表明声明变量被嵌套 anonymous function 或 local function 捕获时，lowering 才将整个声明置于等价外层 block，并生成空初始化器的 `for`；未捕获循环仍保持普通 `for (let ...)` 产物。
- `AstConverterRuntimeClassScenarioTests.ConvertModule_ForCapturedControlVariable_PreservesSingleCSharpBindingOnDenoHost` 断言该 AST/text 结构，并由 Deno.host 验证 lambda、local function 与多控制变量回调均观察到循环结束后的最终控制变量值。`SemanticWalkerLoopTest` 保留普通 loop initializer 的精确文本回归。

### `Nullable<T>.Value`

- `Nullable<T>.Value` 通过 compiler-owned `Op.Compile` lowering 为 nullish guard：receiver 只求值一次，有值时直接返回其 erased carrier；`null` 或 JS 边界传入的 `undefined` 通过 AST 构造的 throw expression 抛出稳定 `InvalidOperationException` 消息，不以默认值或弱类型 sentinel 近似。
- `AstConverterRuntimeClassScenarioTests.ConvertModule_NullableValue_UsesSingleProbeAndThrowsOnEmptyCarrierOnDenoHost` 同时断言生成文本和 Deno.host 的有值、`null`、`undefined` 与 probe 计数行为。

### `Nullable<T>.GetValueOrDefault(defaultValue)`

- 带默认参数的 overload 使用 compiler-owned `Op.Compile` lowering，而不是 inline `??` 模板。它以 AST 构造参数化 IIFE，使 receiver 与 `defaultValue` 先按 C# 从左到右顺序各求值一次，再在 IIFE 内执行 nullish coalescing；因此 nullable 有值时也不会错误跳过 fallback 的副作用。
- `SemanticWalkerOrdinaryTest.Visit_Nullable_GetValueOrDefault_WithArg` 断言 IIFE 的 ESTree/text shape；`AstConverterRuntimeClassScenarioTests.ConvertModule_NullableGetValueOrDefaultWithDefault_EvaluatesFallbackBeforeCoalescingOnDenoHost` 通过有值、`null` 和 JS 边界 `undefined` carrier 验证 fallback eager、receiver-first 与单次求值。

### LINQ `let`

- `let` 由 Roslyn 展开为匿名结构投影、过滤与最终选择；`ITranslatedQueryOperation` 只移除 wrapper，后续仍复用 `Select` / `Where` / `ToArray` intrinsic 和匿名函数 lowering，不拼 query 文本或引入 query 特例 runtime。
- `SemanticWalkerTranslatedQueryTests.Visit_TranslatedQuery_LetWhereSelectToArray_PreservesProjectionChainOnDenoHost` 同时断言透明标识符不泄漏、输出可解析，并执行 `[1, 2, 3] -> [5, 7]` 的最终值链路。

### Materialized `Reverse`

- Roslyn 对数组扩展调用绑定 `Enumerable.Reverse<TSource>(TSource[])`；CLR mapping 使用显式 `reverseArray` export，不从 adapter C# 方法名猜测导出符号。
- `Reverse` 返回逆序的新 Array，输入 source 保持不变；`IEnumerable<TSource>` overload 在逆序前先物化 source。
- `SemanticWalkerEnumerableReverseTests` 断言绑定 overload、import 与输出 AST/text；`ClrRuntimeEnumerableReverseScenarios` 和 `SdkIntegrationTests.Build_LocalJazorPackage_QuerySyntaxWithCapturedLambda_ExecutesOnDenoHost` 覆盖 catalog 和 Deno.host 行为。

### Materialized `SequenceEqual`

- 显式 LINQ 路径绑定 `Enumerable.SequenceEqual<TSource>(IEnumerable<TSource>, IEnumerable<TSource>)`，并导入 `EnumerableModule`；SDK 默认 imports 下的 `array.SequenceEqual(other)` 则由 Roslyn 绑定 `ReadOnlySpan<T>.SequenceEqual<T>(ReadOnlySpan<T>)`，并导入 `MemoryExtensionsModule`。
- 两条 binding 都只接受已 materialize 的 `Array<T>` carrier。先比较长度，再按相同 index 调用默认 `EqualityComparer<T>`，因此 `NaN` 和有符号零遵循当前 CLR equality 合约，首个不等项立即返回 `false`，输入保持不变。
- `ReadOnlySpan<T>` 的 `Array` alias 只服务于明确映射的只读成员调用，不承诺地址、slice identity、stack-only 生命周期或任意 span API。
- `SemanticWalkerEnumerableSequenceEqualTests` 分别断言两条 Roslyn binding、import 与输出 AST/text；`ClrRuntimeEnumerableSequenceEqualScenarios` / `EnumerableSequenceEqualRuntimeTests` 覆盖 `Enumerable` runtime；`SdkIntegrationTests.Build_LocalJazorPackage_QuerySyntaxWithCapturedLambda_ExecutesOnDenoHost` 覆盖 SDK consumer materialization 和 Deno.host。

### Materialized `Concat`

- `Enumerable.Concat<TSource>(IEnumerable<TSource>, IEnumerable<TSource>)` 通过 Roslyn 已绑定的扩展调用进入 `EnumerableModule.concat`，随后由现有 `ToArray` intrinsic 物化；不建立 query 或数组拼接的旁路 lowering。
- runtime 先完整枚举 first，再枚举 second，产出新的 Array carrier，输入 source 不变。实现不用 JavaScript `Array.concat`，因此不把当前受控 `IEnumerable<T>` 语义缩减为数组专用协议。
- `SemanticWalkerEnumerableConcatTests` 断言 bound signature、唯一 import 和 AST/text；`ClrRuntimeEnumerableConcatScenarios` / `EnumerableConcatRuntimeTests` 覆盖空序列、null 参数、generator 枚举顺序与 source 不变性；SDK integration 覆盖最终 consumer artifact。

### Materialized `Append` / `Prepend`

- `Enumerable.Append<TSource>(IEnumerable<TSource>, TSource)` 和 `Prepend<TSource>(IEnumerable<TSource>, TSource)` 由 Roslyn 已绑定的扩展调用分别进入 `EnumerableModule.append` / `prepend`，不把 C# 扩展调用重写为 JavaScript 数组原生 mutation。
- `Append` 先完整枚举 source 再写入 element；`Prepend` 先写入 element 再完整枚举 source。二者均返回新的 Array carrier，不改变输入；当前契约仍是 eager/materialized LINQ，不承诺 CLR iterator identity 或延迟执行。
- `SemanticWalkerEnumerableAppendPrependTests` 断言两个 bound signature、imports 和 AST/text；`ClrRuntimeEnumerableAppendPrependScenarios` / `EnumerableAppendPrependRuntimeTests` 覆盖空 source、null source、generator 枚举顺序与输入不变性；SDK integration 覆盖最终 consumer artifact。

### Materialized `SkipWhile` / `TakeWhile`

- `Enumerable.SkipWhile<TSource>` 与 `TakeWhile<TSource>` 的普通 predicate 和 indexed predicate overload 均通过 Roslyn-bound `Import` 进入 `EnumerableModule.skipWhile` / `skipWhileAt` / `takeWhile` / `takeWhileAt`，复用普通 lambda lowering。
- `SkipWhile` 对每个前缀 source 项调用 predicate，直到首个 false；该项和后续项按原顺序物化，且后续项不再调用 predicate。indexed overload 的 index 对应 predicate 实际观察到的原始 source offset。
- `TakeWhile` 对每个 source 项调用 predicate，首个 false 项不会进入结果且立即终止遍历；既有 `foreach -> for...of` lowering 会关闭可关闭 iterator。indexed overload 将失败项的 source index 传给 predicate，再终止。
- `SemanticWalkerEnumerableWhileTests` 验证四条 bound signature、imports、普通/带 index lambda AST/text 与语法；`ClrRuntimeEnumerableWhileScenarios` / `EnumerableWhileRuntimeTests` 经 generated catalog 和 Deno.host 验证 selector 停止、indexed source offset、early close、null 参数和输入不变性。

### Materialized source factories `Empty` / `Range` / `Repeat` / `AsEnumerable` / `Sequence`

- `Enumerable.Empty<TResult>()`、`Range(int, int)`、`Repeat<TResult>(TResult, int)`、`AsEnumerable<TSource>(IEnumerable<TSource>)` 和 `Sequence<T>(T, T, T)` 均通过 Roslyn-bound `Import` 进入 `EnumerableModule`，不建立 generic-string 或 array-special-case lowering。
- `Range` 使用 Number carrier 但保留 C# Int32 结果域：负 count 与超出 `Int32.MaxValue` 的末项明确失败，零 count 可与 `Int32.MaxValue` start 组合。`Repeat` 拒绝负 count，并在新 Array 每一项中保留原 element reference；`Empty` 每次提供独立空 carrier，避免 JavaScript 可变 Array 泄漏共享 CLR empty storage。
- `AsEnumerable` 是引用透传而不是物化或 null validation，因此同一 source carrier 与 null 输入均按原值返回。
- `Sequence` 每次生成新的三项 Array carrier，并保持三个已绑定参数的求值顺序和值/引用身份。
- `SemanticWalkerEnumerableFactoryTests` 验证五条 bound signatures、imports、组合链 AST/text 与语法；`ClrRuntimeEnumerableFactoryScenarios` / `EnumerableFactoryRuntimeTests` 经 generated catalog 和 Deno.host 验证范围边界、重复引用、empty carrier、source identity、null 透传与失败路径。

### Materialized `LongCount`

- `Enumerable.LongCount<TSource>(IEnumerable<TSource>)` 和 predicate overload 通过 Roslyn-bound `Import` 进入 `EnumerableModule.longCount` / `longCountWhere`；两条路径均沿用普通 lambda lowering，不将 `long` 降格为 JavaScript `Number`。
- 结果 carrier 是既有 `long -> BigInt` 映射。无 predicate overload 枚举每项一次；predicate overload 对每项按 source order 恰好调用一次并仅累加匹配项。null source 或 predicate 明确失败，source 不变。
- `Count` 同时保留 Int32 计数边界，在第 `Int32.MaxValue + 1` 个匹配项抛出 `OverflowException`；`LongCount` 以 BigInt 保留 Int64 上界，并在第 `Int64.MaxValue + 1` 个匹配项抛出同类错误。
- `SemanticWalkerEnumerableLongCountTests` 验证两条 bound signatures、BigInt path 的 imports、lambda AST/text 与语法；`ClrRuntimeEnumerableLongCountScenarios` / `EnumerableLongCountRuntimeTests` 通过 generated catalog 和 Deno.host 验证 carrier、顺序、null 行为和两种宽度 guard 的产物协议。

### Materialized `Index`

- `Enumerable.Index<TSource>(IEnumerable<TSource>)` 通过 Roslyn-bound `Import` 进入 `EnumerableModule.index`，返回当前 tuple lowering 已定义的 `(int Index, TSource Item)` value composition，不创建新的 iterator 或 tuple runtime 类型。
- 模块按 source order 一次性枚举并物化 `Array`；tuple 的 C# `Index` / `Item` 元素名按统一 runtime naming 投影为 `index` / `item`。source 为 null 或 index 超过 Int32 域时明确失败。
- `SemanticWalkerEnumerableIndexTests` 验证 bound signature、唯一 import 与后续普通 `Select` tuple projection；`ClrRuntimeEnumerableIndexScenarios` / `EnumerableIndexRuntimeTests` 通过 generated catalog 和 Deno.host 验证 source order、tuple shape、empty source 和 null 边界。

### Compiler-owned `DefaultIfEmpty`

- `Enumerable.DefaultIfEmpty<TSource>(source)` 不是由 JavaScript runtime 猜测 erased `TSource` 的默认值。它经 `Op.Compile` 在 Roslyn 已绑定的闭合调用点生成 `default(TSource)`，再复用 `Enumerable.DefaultIfEmpty<TSource>(source, defaultValue)` 的 `Import` runtime contract。
- 因此值类型、引用类型和具备 `class` 约束的泛型都遵循已有 `BuildDefaultValueExpression` 语义；不能安全生成 CLR 默认值的未约束泛型仍在 compiler 使用点明确失败，而不是用 `null` 近似。
- 当该无参 overload 作为静态方法组传给一个已绑定的一元 delegate 时，compiler 发射等价箭头函数并复用同一 fallback call；不会把未绑定方法组伪装成 runtime 的 partial application。
- runtime helper 仅一次物化 source：非空时按原顺序返回新 Array，空时写入已由 compiler 提供的 fallback。该路径是 query-syntax left outer join 的 `GroupJoin -> SelectMany -> DefaultIfEmpty` 中间步骤，不建立 query-string 或局部 JavaScript fallback。
- `SemanticWalkerEnumerableDefaultIfEmptyTests` 断言闭合默认值、唯一 import 和左外连接的 bound lowering；`ClrRuntimeEnumerableDefaultIfEmptyScenarios` 通过 catalog 在 Deno.host 验证空、非空和 null source。

### Compiler-owned Default-return Terminals

- `FirstOrDefault`、`LastOrDefault`、`SingleOrDefault` 的无 fallback overload 及 predicate overload 复用同一个 Compile helper：以 Roslyn 已绑定的 closed `TSource` 生成 `default(TSource)`，并调用各自 `(.., defaultValue)` 的 `Import` runtime contract。
- static method group 也按其已绑定 delegate 参数列表降为箭头函数，再进入相同 helper；这保留默认值的使用点类型语义，不把 CLR generic method group 变成 runtime partial application。
- `FirstOrDefault` 在第一个 source 项或第一个 predicate match 后停止；`LastOrDefault` 观察完整 source 并保留最终值；`SingleOrDefault` 在第二项或第二个 match 出现时按 CLR 失败。空 source / 无 match 返回 compiler 提供的 fallback。
- `SemanticWalkerEnumerableDefaultIfEmptyTests` 断言六个 terminal import、值类型默认值、predicate 调用与 module AST/text；`ClrRuntimeEnumerableDefaultTerminalScenarios` 通过 catalog 与 Deno.host 验证空值、顺序、无匹配、唯一值与多匹配错误。

### Terminal `ElementAt(int / Index)` and compiler-owned `ElementAtOrDefault(int / Index)`

- `Enumerable.ElementAt<TSource>(IEnumerable<TSource>, int)` 与 `ElementAt<TSource>(IEnumerable<TSource>, Index)` 通过各自 Roslyn-bound signature 分别进入 `EnumerableModule.elementAt` / `elementAtIndex`，不把两个 overload 归并为数组下标协议。
- int 负 index 在枚举前以 `ArgumentOutOfRangeException` 失败；非负 int 与 from-start Index 顺序枚举至目标项后立即返回。from-end Index 完整观察 source，并用有界环形 tail buffer 返回目标项；`^0`、两侧越界和空 source 均失败，输入 source 不变。
- `Enumerable.ElementAtOrDefault<TSource>(IEnumerable<TSource>, int)` 走独立 `Op.Compile`：在 Roslyn 已绑定的闭合 `TSource` 调用点生成 `default(TSource)`，并用单次求值 IIFE 以 `for...of` 枚举。负 index、空 source 和越界返回该默认值，命中时立即返回；静态方法组同样降为绑定后的箭头函数。未约束泛型在无法安全发射默认值时明确失败，而不以 `null` 近似。
- `ElementAtOrDefault<TSource>(IEnumerable<TSource>, Index)` 复用同一 Compile hook，并按 Roslyn 绑定的第二参数类型选择 traversal protocol。compiler 只通过 `System.Index.IsFromEnd` / `Value` 白名单 getter 读取 Index，不识别 `JIndex` 字段或结构；from-start 路径保持早停，from-end 路径完整观察 source，并以 O(min(source length, index value)) 空间的环形尾缓冲返回目标项。`Index.End` (`^0`) 不枚举 unknown source，空 source 或越界返回 closed default。
- `SemanticWalkerEnumerableElementAtTests` 断言 int / Index bound signature、Import/Compile AST/text、stored Index、`^1`、静态方法组和 generic default boundary；Deno.host + 生成 CLR catalog 验证 from-start 早停、from-end 完整遍历、ring wrap、`^0`、空序列、越界与 null source。`ClrRuntimeEnumerableElementAtScenarios` / `EnumerableElementAtRuntimeTests` 同时覆盖 throwing `ElementAt(int / Index)` runtime imports。

### Materialized tail paging `SkipLast` / `TakeLast`

- `Enumerable.SkipLast<TSource>(IEnumerable<TSource>, int)` 与 `TakeLast<TSource>(IEnumerable<TSource>, int)` 经 Roslyn-bound `Import` 进入 `EnumerableModule.skipLast` / `takeLast`，不把 source 预设为 Array，也不以 JavaScript `slice(-count)` 替代 enumerable traversal。
- 正 count 使用固定大小环形 tail buffer：`SkipLast` 在 buffer 满后输出最旧项，`TakeLast` 在 source 完整观察后按 ring 顺序物化尾项。两者空间均为 O(min(source length, count))；输入不变。
- `TakeLast(count <= 0)` 返回空 Array 且不枚举 source；`SkipLast(count <= 0)` 在本模块 eager materialization 契约下完整物化 source。null source 明确失败。
- `SemanticWalkerEnumerableSkipTakeLastTests` 验证 bound signatures、imports、pipeline AST/text 和可解析 JavaScript；`ClrRuntimeEnumerableSkipTakeLastScenarios` / `EnumerableSkipTakeLastRuntimeTests` 通过 generated catalog 与 Deno.host 覆盖顺序、满 buffer、零/负 count、越界 count、null 与 source 不变性。

### Materialized `DistinctBy`

- `Enumerable.DistinctBy<TSource, TKey>(IEnumerable<TSource>, Func<TSource, TKey>)` 经 Roslyn 已绑定的 invocation 和普通 lambda lowering 进入 `EnumerableModule.distinctBy`，没有为 key selector 建立 query 或 JavaScript special case。
- 每个被枚举的 source 项恰好调用一次 key selector，按 `EqualityComparer<TKey>.GetHashCode` 建 bucket 再以 `Equals` 确认，因此保留每个 key 的首个 source 项，并与既有集合/联接路径共享 collision、`NaN` 和有符号零语义。
- `SemanticWalkerEnumerableDistinctByTests` 断言 bound signature、lambda AST/text、import 和语法可解析性；`ClrRuntimeEnumerableDistinctByScenarios` / `EnumerableDistinctByRuntimeTests` 覆盖空值、selector 调用次数、首次项和 equality contract；SDK integration 覆盖最终 consumer artifact。

### Materialized key-set `UnionBy` / `ExceptBy` / `IntersectBy`

- `Enumerable.UnionBy<TSource, TKey>`、`ExceptBy<TSource, TKey>` 和 `IntersectBy<TSource, TKey>` 分别经 Roslyn-bound `Import` 进入 `EnumerableModule.unionBy` / `exceptBy` / `intersectBy`；每个 path 都沿用普通 lambda lowering，不引入 query-string 或按样例重写。
- 三者都以 `EqualityComparer<TKey>.GetHashCode` bucket 加 `Equals` 确认键等价，因此与 `DistinctBy`、`Join` 和既有集合运算共享 collision、`NaN`、有符号零语义。`UnionBy` 先完整观察 first 再观察 second，保留每个 key 的首个 source representative；每个 source 项 selector 恰好执行一次。
- `ExceptBy` 与 `IntersectBy` 都先完整观察第二序列的 key 集合，再开始观察 first。前者把已存在 key 与已输出 key 统一在同一集合中，按 first 顺序输出唯一未排除项；后者移除已命中 key，按 first 顺序保留每个匹配 key 的首项。输入不变。
- `SemanticWalkerEnumerableSetByTests` 验证三条 bound signature、imports、selector AST/text 与 JavaScript 语法；`ClrRuntimeEnumerableSetByScenarios` / `EnumerableSetByRuntimeTests` 经 generated catalog 和 Deno.host 覆盖键代表、第二序列先观察、selector 次数、重复键、数值相等性、null 参数与输入不变性。

### Compiler-owned materialized `Zip`

- `Enumerable.Zip<TFirst, TSecond>`、带 `Func<TFirst, TSecond, TResult>` selector 的 overload，以及 `Zip<TFirst, TSecond, TThird>` 均通过 exact Roslyn-bound `Op.Compile` 进入 compiler-owned ESTree protocol。它不暴露或白名单化 `IEnumerator<T>`，也不预物化为 Array index loop。
- IIFE 按已绑定 source 参数顺序创建 `[Symbol.iterator]()`，每轮也按该顺序推进；任一 iterator 完成后立即返回当前 Array。`finally` 以相反顺序调用可用 `return()`，因此二源和三源 Zip 的 generator 提前终止、异常和资源关闭顺序都不会被 Array shortcut 擦除。
- 无 selector overload 用 Roslyn tuple element 的当前运行时名字构造二元或三元 object；带 selector overload 直接调用已 lower 的 callback。静态方法组复用 `VisitMethodReference` 已创建的 delegate proxy，而不是再返回一层函数。
- `SemanticWalkerEnumerableZipTests` 验证三条 bound signature、二源/三源 iterator AST、tuple shape、selector 与静态方法组；Deno.host 观察 iterator 创建、推进、empty-first、最短三源截断、reverse close、selector 次数和各 source 的 null guard。

### Materialized `Order` / `OrderDescending`

- `Enumerable.Order<T>(IEnumerable<T>)` 与 `OrderDescending<T>(IEnumerable<T>)` 通过实际 Roslyn `T` generic signature 分别映射到 `EnumerableModule.order` / `orderDescending`，并复用 `OrderByCore` 的 default `Comparer<T>` 路径与 stable `OrderedStates` protocol。
- 排序返回新的 Array，不改变 source；默认 comparer、稳定 tie-break 与由该模块产生的 `ThenBy` / `ThenByDescending` continuation 都保持既有排序契约。自定义 `IComparer<T>` overload 不在本次支持面，不能由任意 JavaScript function 冒充。
- `SemanticWalkerEnumerableOrderTests` 断言两个 bound signature、imports 和 AST/text；`ClrRuntimeReadOnlyCollectionScenarios` / `EnumerableOrderRuntimeTests` 覆盖默认 comparer、null source、升降序和输入不变性；SDK integration 覆盖最终 consumer artifact。

### Terminal `MinBy` / `MaxBy`

- `Enumerable.MinBy<TSource, TKey>(IEnumerable<TSource>, Func<TSource, TKey>)` 与 `MaxBy` 经 Roslyn 已绑定的 invocation 和普通 lambda lowering 分别进入 `EnumerableModule.minBy` / `maxBy`，不引入 selector-specific JS rewrite。
- source 按原顺序枚举，每项 selector 恰好调用一次，default `Comparer<TKey>` 只在首项后参与比较；并列 key 不替换候选，因此保留第一个 source 项。空 source、null source 和 null selector 明确失败，输入不变。
- `SemanticWalkerEnumerableMinMaxByTests` 断言 bound signature、imports 与 lambda AST/text；`ClrRuntimeEnumerableMinMaxByScenarios` / `EnumerableMinMaxByRuntimeTests` 覆盖选择、空值、空 source、selector 次数和 first-tie；SDK integration 覆盖最终 consumer artifact。自定义 `IComparer<TKey>` overload 仍不映射。

### Materialized `Chunk`

- `Enumerable.Chunk<TSource>(IEnumerable<TSource>, int)` 通过 Roslyn 已绑定的 invocation 进入 `EnumerableModule.chunk`，结果保留为 typed nested Array，不建立 iterator wrapper 或后处理字符串协议。
- source 顺序枚举，每个满 size chunk 和最终不足 size 的 tail 都使用独立 Array carrier；输入 source 不变。null source 与小于一的 size 明确失败。
- `SemanticWalkerEnumerableChunkTests` 断言 bound signature、nested Array materialization、import 和 AST/text；`ClrRuntimeEnumerableChunkScenarios` / `EnumerableChunkRuntimeTests` 覆盖 normal/exact/tail groups、size 错误、generator 顺序和 chunk 独立性；SDK integration 覆盖最终 `int[][]` consumer artifact。

### Terminal `First` / `Last` / `Single`

- `First` / `Last` / `Single` 及其 predicate overload 通过 Roslyn 已绑定的 `Enumerable` invocation 进入 CLR `Import`，不引入 query 或 lambda 特例。
- `First(predicate)` 在第一个匹配项停止；`Last(predicate)` 按 source 顺序观察全部元素并保留最后一个匹配项；空 source 或无匹配分别抛出明确的 `InvalidOperationException`。
- `Single(predicate)` 必须遍历至 source 结束以确认唯一匹配，但第二个匹配出现时立即失败；普通 `Single` 在第二个 source 项时失败。
- `SemanticWalkerEnumerableTerminalTests` 验证 bound signatures、import 与 lambda AST/text；`ClrRuntimeEnumerableTerminalScenarios` 覆盖正常、空值、空序列、无匹配和多项；`EnumerableTerminalRuntimeTests` 与 SDK integration 在 Deno.host 验证求值顺序和最终 consumer artifact。
- `FirstOrDefault` / `LastOrDefault` / `SingleOrDefault` 已通过 compiler-owned default-value protocol 支持无 predicate 与 predicate overload；`ElementAtOrDefault(int / Index)` 以相同 closed-default 原则分别完成 from-start early-stop 与 from-end tail traversal lowering。

### Materialized `Aggregate`

- `Aggregate` 的无 seed、带 seed、带 result selector 三种 overload 均通过 Roslyn 已绑定的 `Enumerable` invocation 与 CLR `Import` 进入通用 lambda lowering。
- 无 seed overload 将第一个 source 项作为 accumulator，空 source 显式失败；带 seed overload 对空 source 直接返回 seed；带 result selector overload 在折叠结束后恰好调用一次 selector，包括空 source。
- `SemanticWalkerEnumerableAggregateTests` 验证三个 bound signatures、import 与 accumulator/result-selector lambda AST/text；`ClrRuntimeEnumerableAggregateScenarios` 覆盖正常、空 source 和参数错误；`EnumerableAggregateRuntimeTests` 与 SDK integration 在 Deno.host 验证 callback 调用顺序和最终 consumer artifact。

### Materialized `CountBy` / `AggregateBy`

- `Enumerable.CountBy<TSource, TKey>` 和固定 seed / key seed selector 的两条 `AggregateBy<TSource, TKey, TAccumulate>` overload 均通过 Roslyn 已绑定的精确 signature 进入 `EnumerableModule`。它们不构造 `Dictionary<TKey, TValue>`，而是复用 `GroupBy` / `Join` 的 hash bucket 加 `IEqualityComparer<TKey>.Equals` 协议。
- hash 只缩小候选组，comparer 决定相等性；因此结果保留每个等价类的首次 key representative、首次出现顺序、collision、`NaN` 与有符号零契约。`CountBy` 对每项 selector 恰好一次，并在每组计数超过 Int32 域前失败。固定 seed 的 reducer 在每组首项调用 `func(seed, item)`；key seed selector 只在每个新 key 的首项前调用一次。
- 输出统一物化为 `KeyValuePair<TKey, TValue>` 的两槽 `[key, value]` Array carrier。普通 `Key` / `Value` 访问和 `foreach (var (key, value) in entries)` 都沿用同一结构投影，不新建 wrapper object。
- `SemanticWalkerEnumerableAggregateByTests` 验证三条 bound signature、import、entry `Key` / `Value`、显式 `KeyValuePair` 构造及 foreach 解构的 AST/text；`ClrRuntimeEnumerableAggregateByScenarios` / `EnumerableAggregateByRuntimeTests` 通过 generated catalog 和 Deno.host 验证 comparer、selector/reducer 顺序、seed 次数、null 参数、Int32 guard 和输入不变性。

### RazorVue 产物边界

- 生产输入是官方 Razor Source Generator C#，输出是 Vue render-function `.mjs`。
- 不允许回退到 Razor IR、SFC `.vue`、render-context 或 wrapper marker 协议。
- compiler-owned C# 语义必须经过 `SemanticWalker` hooks；RazorVue 只负责 Vue artifact framing 与显式 host projection。

## 当前门禁

本矩阵对应的 compiler 基线由以下命令复验：

```text
dotnet run --file scripts/csharp/verify-compiler-coverage.cs -- --no-build --no-restore
```

2026-08-04 当前结果：

- `Jazor.CompilerTest`: 8289 / 8289 passed
- `Jazor.CLR.Test`: 4744 / 4744 passed
- line coverage: 96.28% (15575 / 16176)
- branch coverage: 90.03% (6233 / 6923)

覆盖率门禁是最低保证，不替代上述场景证据。新增 operation、CLR mapping、RazorVue host protocol 或 emit 行为时，必须补充到相应行，并先运行其聚焦回归。

## 明确边界

此矩阵不将下列项目表示为“已完整支持”：dynamic、CLR event runtime model、LINQ provider/`Queryable`、unsafe/pointer/function pointer、custom interpolated-string handler、UTF-8 literal、inline array、完整反射/type identity、外部基类与 `this(...)` 构造链。

完整原因与 rejection 入口见 [SemanticWalker.NotSupport.md](../../01-目标/compiler/semantic-walker/SemanticWalker.NotSupport.md) 和 [Jazor.Compiler README](../../../src/Jazor.Compiler/README.md)。
