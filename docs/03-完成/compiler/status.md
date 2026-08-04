# Jazor Compiler 主线状态（2026-08-04）

> Status: 当前状态快照
> Positioning: 仓库级编译器主线状态快照

## 总结

`Jazor.Compiler` 仍然是当前仓库里最成熟的主干资产。

当前可复验基线：

- `Jazor.CompilerTest`：8297 / 8297 通过
- `Jazor.Compiler` 行覆盖：15983 / 16593（96.32%）
- `Jazor.Compiler` 分支覆盖：6332 / 7029（90.08%）
- 验收入口：`dotnet run --file scripts/csharp/verify-compiler-coverage.cs`

coverage gate 会直接运行完整 compiler suite、读取本次 TRX 与 Cobertura，并对 8,000 个通过测试、95% 行覆盖和 90% 分支覆盖执行非零退出码约束；`coverlet.runsettings` 本身不承担阈值判断。

更具体而言：

- 编译器主链路已经接近稳定主干
- 当前工作重点不是重做架构，而是维持主线闭环、控制边界扩张、给外围能力提供稳定依赖面
- 仓库级文档应该把 compiler 当成"稳定核心"，而不是"当前最混沌的探索区"

更具体地说，当前 compiler 主线已经不是“很多能力还没定路线”，而是：

- 关键 runtime 边界已经明确
- 若干以前容易摇摆的语义已经从“目标路线”收口为“当前契约”
- 后续增量工作应优先遵守这些契约，而不是重新打开基础语义形态

## 当前状态判断

### 1. 主链路成熟度高

`AstConverter`、`SemanticWalker`、白名单和 generator 主链路已经有稳定参考价值了。

因此 repo-level 文档应该优先把 compiler deep-dive 当成长期参考入口，而不是把它和阶段性实施材料混成一层。

### 2. 当前更像"稳定化与收口"，不是"大规模重构期"

当前 compiler 线最重要的几件事是：

- 保持语义主线稳定
- 控制新增能力对主链路的扰动
- 给 RazorVue、SourceMap、Emit 这些下游 lane 提供稳定上游

### 3. 当前已收口的关键路线

这一轮明确下来的，不只是“支持了更多测试”，而是几条长期容易反复的路线已经固定：

- `tuple`：走表达式组合 lowering，保使用点行为，不保 `System.ValueTuple` runtime identity
- `ref/out`：走 caller/callee 协议模拟，保求值顺序、回写顺序和结果形态
- `enum`：声明擦除，使用点常量化，运行时按底层标量处理
- `interface`：只作为契约参与分析、投影和宿主查找，不发射 runtime artifact；erased interface `is` 仅在 Roslyn 可证明时折叠，`T : IContract` 保留非空判断，`T : struct, IContract` 折叠为 `true`
- `record`：固定走 structural lowering；创建、`with`、位置/属性模式与解构都按结构属性键处理，不保 nominal runtime identity
- iterator：module method、runtime member method 与 local function 都从实际 Roslyn operation tree 判定 generator；`yield return` / `yield break` 输出 `function*`，`async IAsyncEnumerable<T>` 输出 `async function*`。共享遍历会在 nested lambda/local function 处停止，避免子函数的 yield 错误改变外层函数形态
- `System.Index` / `System.Range`：允许作为真实 carrier 跨 local/argument 边界；数组及具备 `Length`、`this[int]` 和 `Slice(int, int)` 的隐式索引器可消费该 carrier。materialized `Index` 保留读写及复合赋值的单次 getter/setter 求值，materialized `Range` 将 `(offset, length)` 明确转换为 JavaScript `slice(start, endExclusive)` 或等价的 `(start, length)` 调用，并保留 carrier projection 单次求值与越界异常语义
- lambda / delegate：普通、async 和捕获 lambda 均通过匿名函数/委托创建 lowering 进入箭头函数；函数边界隔离局部声明，同时共享模块 import 收集
- field-like event：当前模块 non-record runtime member class 的非静态、非 virtual/override 字段式事件由私有调用列表、受控 add/remove helper 和 snapshot delegate 组成；直接实例方法组以未绑定 method 加 receiver 作为等价键，避免 JS `bind` 临时函数破坏 `-=`。snapshot 在 invoke 前复制当前调用列表，因此重复订阅、最后匹配移除、空事件 `?.Invoke(...)` 的参数短路，以及 handler 内增删订阅的当前/下一轮可见性均有 AST 与 Deno.host 回归。模块静态事件、custom accessor、virtual/override、带 by-ref 参数或返回的 delegate、delegate equality/combination 和 `IRaiseEventOperation` 仍明确拒绝。
- UTF-8 literal：`IUtf8StringOperation` 取 Roslyn 已解码的 C# 字符串并构造精确 UTF-8 byte `ArrayExpression`，通过既有 `ReadOnlySpan<byte> -> Array` carrier 传递；不发射 JavaScript 字符串、`TextEncoder`、BOM、隐式结束符或新的 typed-array identity。普通、转义、BMP、补充平面与 raw literal 均有 AST/text 回归。
- LINQ query：`ITranslatedQueryOperation` 只移除 Roslyn wrapper，复用绑定后的 `Enumerable.Where`、`Select`、`ToList`、`ToArray` AST intrinsic；`Skip` / `Take` 提供物化分页链路（非正 `Skip` 保留全部元素，非正 `Take` 为空）；`Any` / `All` 通过迭代立即短路，每个已观察元素只调用一次 predicate；`OrderBy` / `OrderByDescending` 走 CLR `Import` 的稳定物化排序，selector 每个元素只求值一次，并通过 `Comparer<T>` 默认比较；`ThenBy` / `ThenByDescending` 仅支持直接衔接当前 module 生成的 materialized order state，未知外部 `IOrderedEnumerable<T>` 明确失败；当前仍是受控 Array/IEnumerable 物化子集，不承诺延迟枚举、自定义 `IComparer<T>`、`Queryable` 或完整 LINQ provider 语义
- LINQ materialized equality slice：`SelectMany`、`Count`、`Contains`、`SequenceEqual`、`Concat`、`Append`、`Prepend`、`Chunk`、`Distinct`、`DistinctBy`、`Union`、`Except`、`Intersect`、`GroupBy`、`Join`、`GroupJoin`、`Order`、`OrderDescending`、`Reverse`、`ElementAt(int)`、`First`、`Last`、`Single`、`MinBy`、`MaxBy` 与 `Aggregate` 已进入绑定映射；集合与联接先以 `EqualityComparer<T>.GetHashCode` 收窄 bucket，再以 `Equals` 确认，保留源序、首次值和 `NaN`/有符号零规则，不退回 JavaScript `Set`/`Map` 键语义。`Reverse` 返回新的逆序 Array，不改变输入 source，并分别覆盖 `IEnumerable<T>` 和数组 overload；`SequenceEqual` 的显式 LINQ 调用绑定 `Enumerable.SequenceEqual`，SDK 默认 imports 下的数组实例调用则绑定 `ReadOnlySpan<T>.SequenceEqual(ReadOnlySpan<T>)`。两条路径均使用同一 Array carrier 的长度预判与同步默认相等比较，在首个不等项短路且不修改输入；`Concat` 先完整枚举 first 再枚举 second；`Append` 先枚举 source 再追加 element，`Prepend` 先写入 element 再枚举 source。三者均返回新的 Array carrier，不使用 JavaScript 数组原生 mutation 或 `Array.concat` 退化为数组专用协议；`Chunk` 顺序物化为独立 nested Array carriers，保留 tail chunk；`DistinctBy` 对每个枚举项调用 key selector 一次，并通过同一 comparer 合约保留每个 key 的首项；`Order` / `OrderDescending` 复用 default `Comparer<T>`、稳定排序与 existing `ThenBy` order-state，不开放 custom comparer；`MinBy` / `MaxBy` 对每项 selector 一次、default `Comparer<TKey>` 选值且保留首个并列 source 项；`ElementAt(int)` 对负 index 直接失败、对非负 index 在命中时立即停止枚举，越界明确失败；`ReadOnlySpan<T>` 仅是这个受控 Array view 的静态 alias，不引入地址、切片 identity 或生命周期模型。`First` / `Last` / `Single` 及 predicate overload 保留短路或完整遍历、空序列、无匹配和多项异常；`Aggregate` 覆盖无 seed、带 seed、带 result selector 的 accumulator 顺序与空 source 行为。`ElementAtOrDefault`、`FirstOrDefault` / `LastOrDefault` / `SingleOrDefault` 通过 compiler-owned closed-default protocol 在使用点发射 `default(T)`；无法安全发射未约束泛型默认值时明确失败，而不以 `null` 近似。该子集不承诺延迟枚举、自定义 `IEqualityComparer<T>`、自定义 `IComparer<T>`、`Queryable` 或 provider identity。SDK Razor 默认 imports 下的 array `Contains` 也会绑定 `System.MemoryExtensions.Contains(ReadOnlySpan<T>, T)`，其 Array carrier 同样使用默认 comparer。
- compiler-owned `Enumerable.Zip` 目前覆盖二源 tuple、二源 result selector 与三源 tuple 三个精确 BCL overload；IIFE 以 source 参数顺序创建和推进 JavaScript iterator，在任一 source 完成时停止，并在 `finally` 中反向调用可用 `return()`。tuple 输出继续使用 Roslyn tuple element 的 runtime 名字，不建立 `IEnumerator<T>` 白名单或 Array index shortcut。
- keyed aggregation：`Enumerable.CountBy` 与固定 seed/key seed selector 的 `AggregateBy` 通过精确 bound signature 复用 comparer-aware grouping bucket，不构造 generic `Dictionary`。它们保留首次 key representative、插入顺序、collision、`NaN` 与 signed-zero 规则；`CountBy` 在 Int32 overflow 前失败，key seed selector 仅对每个新 key 调用一次。结果为现有 `KeyValuePair<TKey, TValue>` 两槽 Array carrier，因此 `Key` / `Value` 与 foreach 解构不引入 wrapper object。
- 模块导出：固定只支持 named export；任何成员若解析到导出名 `default` 都应显式失败
- 成员类继承：支持同模块成员类的 JS-compatible 子集，真实输出 `extends` / `super(...)` / `super.member`
- 成员类构造函数重载：单真实 `constructor` + `$ctor_<hash>` helper + 已绑定构造函数 selector dispatcher
- 产品扩展：核心以 `AstConverterModulePolicy`、`SemanticWalkerHost`、`CompositeSemanticWalkerHost` 和 `SemanticInvocationLoweringContext` 提供强类型组合契约；RazorVue product lowering 与 Components catalog 已迁出核心

这意味着 compiler 主线现在已经有一套更清晰的“什么必须保、什么可以擦除、什么必须显式失败”的规则，而不是继续在“尽量长得像手写 JS”上摇摆。

### 4. Import 与模块头链路已闭环

之前 import 还是“收集多、落盘少”的风险点。当前这部分已经进入稳定状态：

- `SemanticWalker` 收集 import specifier
- `SenseArgument` 上浮导入分组
- `AstConverter` 合并、去重并稳定排序
- 模块头生成 `ImportDeclaration`

所以 import 不再是当前 compiler 的核心缺口，后续重点更多是保持确定性，不是重新打通主链。

### 5. 这轮已经补上对称的 compiler 状态入口

之前 repo-level 主要是总项目状态和 RazorVue 状态，导致 compiler 虽然成熟，但在仓库级工作流图里不够显眼。本状态页的作用就是把这条断链补全。

## 下一步行动

### 1. Catalog / emit contract stability

**目标**：巩固 `compiler -> catalog -> emit` 边界与物化契约

**具体行动**：
- 已覆盖外部消费者的官方 Razor SG -> VueRenderCatalog -> `Jazor.Emit` -> 最终 `.mjs` -> bundled DenoHost 闭环；同一 Counter consumer 同时验证 DOM 与 descriptor 子组件显式 async `@bind:set` 的相对模块 import、`update:modelValue` 回写，以及 setter 内部先规范化 state 后完成持久化的顺序，另有异步点击后的条件内容切换，且产物不得回退到 render-context / `.vue` 协议。Razor SDK 禁止 `:set` 与 `:after` 组合，后续逻辑由显式 setter 承担。
- 避免文档把 compiler 产 catalog 和 emit 写文件混成一个未定义阶段
- 让 catalog、模块文本与最终物化产物的关系保持一致

**参考文档**：
- [TransformationRoadmap.md](../../02-计划/compiler/TransformationRoadmap.md)
- [TransformationClosureChecklist.md](../../02-计划/compiler/TransformationClosureChecklist.md)

### 2. Host semantics seam

**目标**：稳定 `Inline` / `Compile` 分工

**具体行动**：
- `Nullable<T>.Value` 作为 compiler-owned `Op.Compile` lowering：通过 AST 构造短路 nullish guard，保证 receiver 单次求值，并在 `null` / `undefined` carrier 上抛出稳定的 `InvalidOperationException` 语义。
- `Nullable<T>.GetValueOrDefault(defaultValue)` 同样通过 compiler-owned `Op.Compile` lowering：参数化 AST IIFE 先完成 receiver 与默认参数的从左到右 eager evaluation，再执行 nullish fallback，避免 inline `??` 错误跳过默认参数副作用。
- ECMAScript runtime `params` 默认映射为 JavaScript rest arguments；显式 `[PreserveParamsArray]` 则保留为单个数组实参，保护 Vue `withModifiers` 等 runtime array contract。
- C# 14 `field` 属性在成员运行时类中使用合法的 JavaScript private slot；回归同时校验 Acornima AST 可解析与 Deno.host 的 setter/getter 读写语义。
- 避免宿主语义扩张又跑回来破坏 compiler 主线边界
- 保持 `Inline` 和 `Compile` 的职责清晰分离

**参考文档**：
- [InlineAstTemplateSpec.md](../../01-目标/compiler/InlineAstTemplateSpec.md)
- [OpCompileSpec.md](../../01-目标/compiler/OpCompileSpec.md)
- [OpCompileImplementationChecklist.md](../../02-计划/compiler/OpCompileImplementationChecklist.md)

### 3. Source origin / sourcemap stability

**目标**：把“稳定 emission”从测试便利提升为持续契约

**具体行动**：
- 保持 temp 名、import alias、source-origin 锚点稳定
- 避免 traversal-order 影响输出
- 让 SourceMap 与真实输出链继续对齐，而不是只在测试链路里成立

**参考文档**：
- [SourceMap.Design.md](../../01-目标/compiler/sourcemap/SourceMap.Design.md)
- [SourceMap.Overview.md](../../01-目标/compiler/sourcemap/SourceMap.Overview.md)

## 深度文档

- [Compiler Architecture Bridge](../../01-目标/compiler/architecture.md)
- [真实场景覆盖矩阵](ScenarioCoverageMatrix.md)
- [Compiler 文档索引](../../01-目标/compiler/README.md)
- [ImplementationPrinciples.md](../../../src/Jazor.Compiler/ImplementationPrinciples.md)
- [Jazor.Compiler README](../../../src/Jazor.Compiler/README.md)

## 当前缺口

- output / emit / sourcemap 侧仍需继续扩展真实构建闭环的场景覆盖；当前外部 Razor Counter 已具备 DOM binder、子组件 explicit model setter、catalog import 与事件状态的 DenoHost 基线
- 宿主语义扩张仍然可能反向污染 compiler 边界，需要持续约束
- 还需要继续把 compiler 局部文档里的 active / historical 边界写清楚，避免旧阶段表述回流成“当前事实”
