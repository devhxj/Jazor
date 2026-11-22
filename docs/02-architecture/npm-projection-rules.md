# Jolt npm 类型投影与值传递规范

本文规定从锁定的 TypeScript 公开声明生成 C# 的目标算法，适用于 `.jazor`、编辑器和普通 `.cs` 类库。Vue、Vue Router、Pinia、Pinia Testing 和 Vue Devtools 继续提供手写绑定。七个应用库提供调用语料，逐库的支持状态和未通过的场景记录在[验收计划](../04-roadmap/p3-vue-application-bindings-plan.md)。

## 输入、产物与职责

| 阶段 | 输入与决定 | 产物及使用者 |
| --- | --- | --- |
| 解析公开入口 | 用项目锁文件与包的 exports 解析实际 ESM 入口，用同一项目的 TypeScript checker 解析声明、别名、重导出和重载顺序。区分类型导出与运行时值。 | 导入用作者看到的 specifier 和导出名，类型身份用解析后的声明。 |
| 读取类型与关系 | 对每个公开调用签名建立泛型参数作用域，记录约束、默认值、各处引用，以及属性、索引、数组、回调、联合与宿主类型之间的关系。递归引用回到同一声明。 | 在现有 `NpmValueExport / NpmValueSignature / NpmValueType` 契约上传递已确定的 C# 形状。checker 的临时类型编号只在本次计算中使用。 |
| 决定作者可用的签名 | 根据值的方向选宿主类型，求出参数、回调和返回类型的共同约束，再决定是否需要额外重载。对同一导出的所有候选一起处理重载冲突。 | 组件和公开 DLL 使用同一套成员名、可用操作和类型关系。编辑器用同一份声明位置索引提供定义跳转。 |
| 编译具体调用 | Roslyn 绑定 C# 重载、类型实参、属性与实际参数。Jazor 根据已绑定的符号执行已有 Alias、Inline、Import 或 Compile 规则。 | 调用原 ESM 导出，保留接收者、对象身份、求值顺序和求值次数。Jolt CLI 写出组件模块，普通 C# 模块由 Jazor.Emit 物化，Vite 负责应用资源。 |

一个签名可以提供若干可用的调用形式。凡是 C# 允许构造的输入，所选 TS 重载必须接受。凡是公开可读取的结果，必须覆盖该调用可能返回的值。允许缩小输入域、只公开结果的一部分。遗漏必填输入会使这条调用形式不可用。发生在嵌套节点的问题要记录到相应路径，例如 `options.select → return`，便于定位声明或补充有类型的适配器。[现有元数据](../../src/Jolt.Npm/NpmValueExport.cs) · [Jazor 编译边界](../../src/Jazor.Compiler/ImplementationPrinciples.md)

## 值传递方向

以应用为观察点，**输入**表示应用给 npm 的值，**输出**表示 npm 给应用的值。沿类型结构递归，以下规则可组合任意层数。

| 所在位置 | 如何传递 | 对 C# 的影响 |
| --- | --- | --- |
| 模块函数、实例方法 | 参数为输入，返回为输出 | 输入对象要可构造，输出对象可只读，方法调用保留 JS 的 `this`。 |
| 函数值或回调 | 形参翻转外层方向，返回值沿用外层方向 | 应用传入回调时，回调形参是应用读到的值，回调结果必须是 npm 能接收的值。返回给应用的函数再翻转一次。 |
| 只读属性、只读数组、只读 ref、Promise | 元素沿用外层方向 | 直接读出相应类型，标准 Promise 映射 `IPromise<T>`，Vue ref 保持响应式身份。 |
| 可写属性、索引器、可变数组、可写 ref | 同时检查读和写，分别确定可读域及可写域 | 两者都满足才公开读写。若仅可证明读取，返回对象可公开只读视图。允许写入的集合必须能承受 npm 的合法回写。 |
| 初始化属性 | 创建时赋值按输入检查 | TS 的 `readonly` 仍允许构造时赋值。C# 可用 `required`、`init` 及可选属性表达构造契约。省略属性在 JS 对象中保持缺席。 |

例如 `transform<S, R>(value: S, select: (value: S) => R): R` 只需要两个类型参数。第一个实参和回调形参共享声明中的 `S`，回调结果与调用结果共享 `R`。若返回声明改为 `S`，公开返回类型也改为 `S`。下面两行消费的是同一个导出，C# 从回调推断出不同的返回类型。[独立消费者用例](../../scripts/csharp/verify-jolt-public-projection.cs)

```csharp
var input = new TransformInput("typed", 3);
string label = Fixture.Transform(input, value => value.Label);
double count = Fixture.Transform(input, value => value.Count);
```

## 类型表示与发射

| TypeScript 声明 | C# 作者表示与 JS 行为 |
| --- | --- |
| `boolean`、`number`、`string`、`void` | 分别为 `bool`、`double`、`string`、无返回值。C# 调用方可以使用已有的宿主数值转换。 |
| `bigint`、`symbol`、`unique symbol` | 目标载体分别是 `ECMAScript.BigInt` 和 `ECMAScript.Symbol`。要求特定 `unique symbol` 的输入还需携带该声明的身份。如果包导出这个值，可生成固定传入该导出的调用形式。 |
| 标准 Date、DOM/WebIDL 实例、Vue Ref、Computed、Promise | 解析源声明及实例化参数，复用宿主程序集的现有类型和继承关系。实例进入下一个函数或 ref 时保留 JS 身份。包内同名类型按自己的声明处理。 |
| 标准 Map、Set、RegExp 和类型化数组 | 优先复用 ECMAScript 的 `Map<TKey, TValue>`、`Set<T>`、`RegExp` 和相应类型化数组。只读视图仅开放原声明可用的操作。带索引签名的 JS 对象沿用字典规则，`Map.get` 和普通属性读取各自保留原协议。 |
| 对象、接口、交叉类型 | 描述数据的输入生成结构记录，按需区分只读结果与可构造输入。合并交叉成员时同时满足各组成声明。带行为、递归身份或需要回传的对象可生成宿主句柄。接口和类型参数在 JS 中擦除。 |
| 数组、元组、字典 | `ReadonlyArray<T>` 公开只读元素，允许写入的数组保留元素不变性。元组保留位置、长度及可选项。一个元组参数发射一个 JS 实参，展开参数依据 rest 声明处理。字典按键域和值域生成索引访问，缺键按声明及已知宿主读取协议处理。 |
| 有限字面量、枚举、联合 | 封闭字符串域保留每个 wire 值，数值枚举保留原值。互不可赋值的分支优先用 C# 原生 union。分支可相互赋值且需要精确选择时使用带标签的宿主契约。允许多种输入调用方式时可用重载。输出联合只允许读取各分支共有的成员，或依据已证明的判别值收窄。 |
| `null`、`undefined`、可省略 | 参数缺席、对象键缺席、值为 `null` 和读取时的 `undefined` 分开记录。末尾省略减少 JS 实参数量，具名后参前的空位发射 `undefined`，显式 `null` 保留。只读可缺省值可在 C# 观察为 `null`，用空值判断保留 `0`、`false` 与空字符串。若调用需区分两种空值，须有能表达该区别的输入契约。 |
| 泛型、索引访问、映射、条件及模板字面量类型 | 先按下一节解析其类型关系。封闭后回到本表，可保留的开放关系生成受约束泛型。仅凭 `any`、`unknown` 或任意字符串不能产生具体的结果元素、属性或插值键。 |
| `unknown`、`any`、`never` 与类型谓词 | 输入域允许任意值时，可公开能由现有宿主表示的输入。输出域未知时可只保留同一值的传递能力，具体成员操作需有声明。联合中的 `never` 分支可消去。类型谓词可公开布尔结果，进一步的 C# 类型收窄须有对应的宿主判别协议。 |
| 函数、重载、命名空间、构造签名 | 独立函数从 ESM 入口导入。对象方法、可调用对象和构造器分别保留绑定接收者、调用和 `new` 的协议。对照 Jazor 已有 lowering，只有可端到端发射的操作才进入公开契约。 |

同一命名类型跨调用传递时，比较解析后的包与声明身份、泛型实参、可读写成员和用途。例如 date-fns 的 `IntervalToDuration` 返回值可以直接交给接受同一时长结构的 `FormatDuration`。若接收方能写入比结果视图更宽的值，则不能据此共享可写类型。Monaco 的模型、DOM 元素及 Vue ref 通过同一宿主句柄传递。强类型转发通常只需要生成 C# 签名，JavaScript 无需复制对象。[共享类型与宿主发射](../../src/Jolt.Compiler/JoltNpmValueProjection.cs)

Jazor.CLR 的内部值载体由静态适配签名推导，继续由核心编译器管理。npm 投影采用公开的 JavaScript 宿主契约，例如 TypeScript 的 Date 对应 ECMAScript.Date。跨语言调用使用 Number 或 BigInt 时，整数位宽、溢出与窄化仍遵循 Jazor 的使用点规则。[宿主类型清单](../../src/Jolt.Npm/WebIdlCarriers.cs) · [CLR 类型映射原则](../../src/Jazor.Compiler/ImplementationPrinciples.md)

对于 `T | undefined`，先确定 `T` 的声明域。引用类型可映射为可空引用，值类型需要 `Nullable<T>` 及相应的值类型约束。无约束的 C# `T?` 在 `T = double` 时无法表示缺值。优先保留单一泛型入口，只对实际影响缺省读取的独立槽位划分类别。若读写须使用不同视图，可由可区分的选项类型或签名承载。单靠 `where` 和返回类型不能区分 C# 重载。独立槽位分别求解，公开签名按可观察差异合并，不预生成所有标量的笛卡儿积。

### 对象上的可选泛型字段

对于 `interface Draft<T> { seed: T; value?: T }`，生成的 `Value` 属性保持类型 `T`，`HasValue` 查询对象自身是否有 `value` 键。构造时只指定 `Seed` 会省略该键。写入 `0`、`false` 或 DTO 会创建该键，回传记录时保留原对象。读取缺席的字段会抛出带字段名的错误。作者可以用 `draft.HasValue ? draft.Value : draft.Seed` 提供默认值。若上游已有 `hasValue`，原成员保留 `HasValue`，存在性检查生成 `HasValue_2`。

当声明为 `value?: T | null` 时，字段有三种状态。先用 `HasValue` 判断是否提供，再读 `Value`。提供了显式 `null` 时读取结果为 `null`，有值时读 `Value.Value` 得到 `T`。其 C# 表示为 `ECMAScript.NullableValue<T>?`，初始化可直接赋 `T` 或 `null`，运行时仍传原始 JS 值。对于普通 `value?: T`，只要类型实参本身允许 `null`（例如 `Draft<double?>`），显式赋 `null` 同样会保留该键。

这条规则适用于命名泛型数据记录与有限键的 `Pick`、`Partial<Pick<…>>`。已定型的记录作为必填实参时可以提供泛型推断依据，即使它的所有字段都缺席。仅写 `new()` 仍需由其他实参或显式类型实参确定 `T`。上游把可选字段改为必填字段后，重新生成的契约会要求调用方初始化它。[声明及组件用例](../../src/Jolt.Npm/verify.cs) · [独立 DLL 与运行用例](../../scripts/csharp/verify-jolt-public-projection.cs) · [缓存验收](../../scripts/csharp/verify-jolt-projection-cache.cs)

## 泛型、依赖关系与重载选择

1. **绑定开放参数。** 以声明及其作用域内的位置识别类型参数。别名解引用，同名的内层参数遮蔽外层。把参数引用递归穿过回调、Promise、ref、数组、命名类型实参和记录成员。已支持的直接传递形式归入这一条规则。带有具体类型、约束、默认值或条件运算的位置保留各自含义。
2. **代入与默认。** 先使用显式实参，再使用调用可推断的实参，最后使用适用的声明默认值。回调 `(input: S) => R` 可以独立决定 `R`。`Promise<R>`、`Ref<R>` 与 `R` 对应不同的作者操作。原生 Promise 会采用另一个 Promise 的最终结果，因此声明中直接嵌套的两层 Promise 不能承诺两次可观察的兑现。
3. **键与字段关联。** `K extends keyof T` 确定合法单键，`T[K]` 绑定同一记录的字段类型。有限具体键可生成字面量或类型化重载。对于应用自定义记录，可使用属性选择器表示 key 与值的关联。递归路径还须由声明或有类型的适配器给出分段和编码协议，数组索引、点分字段和整串字面键分别处理。
4. **条件与映射。** 对已知的有限判别值分别代入原声明，按 TS 的条件、映射修饰符和重载顺序求返回类型。动态判别值公开所有可能分支都允许的操作。开放条件仅在保留其相关关系或可证明各分支给出同一操作时开放。符号关系由 checker 提供，具体示例调用只验证所选封闭分支。
5. **决定 C# 入口。** 先保留所有可表达原重载的原始顺序及调用域，再按 C# 可区分的签名合并。参数类型、泛型元数和合法传参方式参与重载选择，返回类型与可空引用标注不能单独区分。C# 擦除后冲突时，结果相同可合并，返回依赖字面量时可生成带固定输入的命名入口。较早 TS 重载可能命中时，只公开已确定的共同结果。真实导出名优先。

**递归记录。** 处理 `Tree<T> { value: T; children: readonly Tree<T>[] }` 时先登记 `Tree` 的声明和形参。再次遇到同一声明，成员类型只携带声明身份和本次类型实参。由此 `root.Children[0].Value` 与 `root.Value` 都是 `T`，两个包导入同一声明时也使用同一个节点类型。相互引用的记录沿用这一规则。整条环路中若只传递、重排已有类型槽位或代入原始标量，可能访问到的实例化是有限的。`Tree<T>` 回到 `Tree<readonly T[]>` 会在每次遍历后继续增加一层类型，需要另行处理。能否从调用实参推断 `T`，依据可达的数据字段和回调返回决定。

**可省略的布尔判别。** 对 `select<B extends boolean = false>(enabled?: B): B extends true ? Detail : number`，分别解析 `select(true)`、`select(false)` 和 `select()`。前两种调用将字面量写回原位置。第三种调用让 checker 在省略实参的条件下推断返回类型，发射时保持原有的 JS 实参数量。若包把类型默认值改成 `true`，省略入口的返回类型和对应的源码缓存键随之改变。没有声明默认值时，省略调用可能产生联合结果，只有可忠实表示的结果才进入 C#。单签名的特化还需检查每个保留的参数，重载存在竞争时须按原声明顺序独立判定。

**具名后参与中间空位。** 以 `useStorage(key, initial, storage?, options?)` 为例，手写参考允许作者选择默认存储并自定义选项。对应的 C# 写法是 `UseStorage("count", 5d, options: new() { WriteDefaults = false })`，发射四个实参，第三位为 `undefined`。只写 key 和初值时发射两个实参。投影器要把每种允许省略的调用与上游声明的顺序、参数域和结果一起核对。较短重载与较长重载的结果不同时，应保留它们各自的调用范围，尤其要检查“第一个可选参数缺席、下一个参数存在”能命中哪条声明。该判断适用于具有相同可选参数结构的包。
6. **确定失败范围。** 未知的输出成员可留在 JS 对象上，其他有类型的成员继续可读。无法提供的必填参数、没有明确调用协议的签名或无法证明的读写关系，限制相应调用。缺少上游声明时可由项目提供窄 `.d.ts` 或有类型的 ESM 适配器，复用同一投影路径。

有限映射以 checker 实例化后的成员为准。例如 `Partial<Pick<T, K>>` 保留选中的键和值类型，并给每个结果成员加上可缺席标记。`Readonly` 作用于声明指定的层级。`A & B` 的同名可写属性必须同时满足两侧契约。若输入联合的不同分支分别要求 `{ x: number }` 与 `{ y: string }`，公开的构造方式应能保证至少满足其中一支。将二者合成所有属性都可省略的记录会错误接受 `{}`。

每次代入后按完整的可观察签名去重，包括泛型绑定、参数域、必填与可省略的位置、读写权限、返回契约以及 JS 参数排列。对于输入只是多种表示而结果相同的情况，先尝试一个闭合 union。只有委托推断、目标类型初始化或结果差异需要多个入口时再生成重载。这样两个互相独立的联合参数通常只需一个签名，单次 TS 选择得到的相关分支仍作为一个整体保存。

### 声明如何决定生成结果

对一个导出，先在每个原始重载的作用域内沿参数和返回类型收集约束，再决定生成哪种 C# 表示。下表用同一个泛型连接多个位置，说明每步需要保留下来的信息。

| 决策顺序 | 可用于决定结果的信息 | 举例 |
| --- | --- | --- |
| 1. 识别身份 | 解引用后的声明符号、泛型所属声明及槽位、标准库或宿主符号 | 两处都引用同一个 `S` 才共享类型参数。包内声明的 `Promise` 按该包自己的成员和调用协议解析。 |
| 2. 确定谁提供类型 | 显式类型实参、应用提供的实参、回调返回、TS 默认值 | `select: (s: S) => R` 可以从 lambda 推断 `R`。只出现在返回值且无默认值的 `R`，需要另有依据给出作者可用的返回类型。 |
| 3. 确定对值的操作 | 对同一槽位汇总输入、输出、可写位置、缺值及宿主身份 | `(v: V) => Promise<D>` 与两个可缺省的 `Ref` 分别要求保持 `V` 和 `D`。在可读可写 ref 上分别核对读取类型和 setter 接受的类型。 |
| 4. 选择调用形式 | 由 TS 重载顺序、默认值和有限判别决定的合法组合 | 分页选项不含 `select` 时保留默认数据形状。加入必填 `Select` 的选项记录，可将回调结果与返回对象的 `Data.Value` 绑定。 |
| 5. 生成并复用契约 | 规范化后的签名、命名类型和 Jazor 已支持的使用点操作 | 记录、委托、宿主句柄和参数属性进入生成源码。构建公开 DLL 后，另一项目可从成员元数据恢复原 ESM 导入及参数传递方式。 |

开放签名的依据必须能随其允许的所有 C# 类型实参一起成立。原声明保留了类型参数身份时直接使用该关系。需要查询 TypeScript 的条件结果时，对所公开的封闭分支分别求值。若只有具体样本的结果一致，仅能作为这些样本的回归证据。后续将分页、表单等读取器中重复的槽位传递、数组元素和可缺省 ref 识别迁入公共规则时，以此决定能移除哪些专项分支。

## 调用链中的类型契约

| 公开声明中的事实 | 由规则得到的作者体验 | 边界与验收 |
| --- | --- | --- |
| Mutation 的回调接收 `TVariables` 并返回 `Promise<TData>`，响应对象分别暴露 `Ref<TData 或 undefined>` 与 `Ref<TVariables 或 undefined>` | 数字变量、布尔结果的调用，上传 `0` 后能读回可空数字 `0` 和可空布尔 `false`。两个泛型参数可独立换成记录。 | 更改结果类型只影响依赖该参数的位置。调用前的缺值与成功后的零值必须能区分。 |
| 分页选项中的 `select?: (data: InfiniteData<Page, Cursor>) => Result`，结果读取 `Result` | 省略回调沿用声明默认结果。选择原数据可以保留游标类型，选择页标题则应得到标题的类型。 | 默认结果的 `pageParams: unknown[]` 只能保留已知信息。显式选择的游标要从 C# 直接读出，并能拒绝错误的元素类型。 |
| 表单 setter 的路径和写入值由同一记录及同一个路径参数决定 | 目标写法为 `form.SetFieldValue(x => x.Details!.Count, 0, false)`。Roslyn 绑定数据属性，Jazor 将各段实际 JS 名编码为原 API 要求的路径，npm 完成更新。 | 验证选择器根与叶类型，拒绝计算 getter 和把字段拓宽为 `object` 后传任意值。含路径分隔符的属性别名须按声明协议编码。确保接收者和值各求值一次。 |

Vue I18n 的消息树可给出 `checkout.receipt` 这样的资源路径。叶子的类型为 `string` 时，同一声明可容纳 `Hello {guest}` 或 `Total {amount}`。要按消息约束插值名，需要消费者的静态资源文本或显式资源契约。运行时加载的文本沿用通用命名参数字典。[锁定包消费夹具](../../scripts/csharp/fixtures/jolt-binding-projection/I18nPlugin.jazor)

### 返回对象上的方法与异步取值

同一回调既能注册又能解除时，两个参数必须能接收同一个委托。对于把泛型载荷作为 rest 元组的事件钩子，先让 checker 以单元素元组 `[T]` 实例化，再检查订阅、退订和触发是否引用同一载荷。由此生成的 `CreateEventHook<string[]>()` 一次传一个数组。注册与退订直接传原回调，订阅句柄保留自己的接收者。上游触发结果是 `Promise<unknown[]>`，这一投影提供等待完成的 `IPromise`。[事件钩子读取规则](../../src/Jolt.Npm/event-hook-projection.mjs) · [消费用例](../../scripts/csharp/fixtures/jolt-npm-runtime/EventHook.jazor)

对于 `copy(text: string | (() => Promise<string | undefined>)): Promise<void>`，同一个调用位置允许传文本，也允许 npm 稍后调用取值函数。投影为两个实例方法重载。直接值和回调返回值必须属于声明允许的域。回调可以承诺总是兑现为字符串，Jazor 将回调本身传给 npm，调用时机由 npm 决定。只有原方法允许省略参数时才生成无参调用。通过方法组调用仍须保留原对象作为接收者。

若返回对象有 `te<Str extends string, Key extends SchemaKeys = SchemaKeys>(key: Str | Key, locale?: string): boolean`，第一个类型参数允许任何字符串。分别检查只给键、同时给键和 locale 的调用。每个参数前缀都要与可能先命中的重载核对结果类型。Vue I18n 用两个重载区分依默认语言回退查找和只在指定语言中查找。消息文本查找仍通过原来的 composer 完成。[SSR 验收](../../scripts/csharp/verify-jolt-i18n-plugin.cs) · [语言切换](../../scripts/csharp/fixtures/jolt-npm-runtime/verify-i18n.mjs)

### 结果中的状态、集合和宿主对象

| 上游关系 | 公开调用与边界 |
| --- | --- |
| 多个分支分别提供 `Ref<true>` 和 `Ref<false>`，或携带各自的状态字面量 | 通过原 ref 读取布尔值或状态枚举。分支相关的写入需要知道当前分支，公共视图只开放读取。若错误类型确实解析为标准库的 `Error` 或 `null`，`Error.Value?.Message` 在没有错误时为空，失败后能读到原异常消息。查询与提交分别保留各自的状态域。[客户端状态转换](../../scripts/csharp/fixtures/jolt-npm-runtime/verify-query.mjs) |
| `useFieldArray<T = unknown>(path)` 的返回把同一 `T` 用于条目、单值操作和替换数组 | `UseFieldArray<string>(path)` 的 `Fields.Value[i].Value`、`Push(value)`、`Update(i, value)` 和 `Replace(values)` 使用同一种元素类型。字符串路径与活动表单的元素类型由作者配对。省略类型实参时仍能读固定元数据或按位置移除，不能据此推断未知元素的类型。[表单数组](../../scripts/csharp/fixtures/jolt-npm-runtime/Vee.jazor) |
| 实例方法返回同一命名宿主句柄或 `null` | Monaco 的 `GetModel()` 保留可空句柄和模型身份，非空时可读只读字符串 `Id`。解除挂载和重新挂载都沿用原编辑器、模型与 worker。[Chromium 用例](../../scripts/csharp/fixtures/jolt-monaco-browser/App.jazor) |
| ref 的元素是同一具名字面量域，可能为 `null` 或 `undefined` | 两个鼠标来源使用同一个可空字符串枚举，保留声明允许的写入。网络状态缺席时，getter 将 `undefined` 映射到 `null`，仅在声明允许完整写入域时才公开 setter。[挂载验收](../../scripts/csharp/fixtures/jolt-npm-runtime/verify-events.mjs) |

字段工厂的初值会影响回调实际读到的值。带初值和校验器的 `UseField<T>` 将同一类型贯穿初值、回调和可写 ref。省略初值时，回调参数和字段读取可以表示缺席，`SetValue` 仍接收具体的 `T`。调用者只传路径和 options 时，生成的 JS 在规则参数的位置传 `undefined`。注入表单上下文时，DTO 来自调用者的显式类型参数。选择器沿 DTO 的映射属性名读取已注册字段，数值缺席读为 `null`，运行时没有注入源时上下文本身也可为空。[独立字段调用](../../scripts/csharp/verify-jolt-public-projection.cs) · [表单上下文](../../scripts/csharp/fixtures/jolt-npm-runtime/FormScope.jazor)

### 扩展 Vue ref 的身份与转发

`type Meter<T> = Ref<T> & { replace(value: T): T }` 同时声明了 Vue 引用、附加方法以及两者共享的类型参数。返回和参数位置解析到同一命名声明和同一实参时，两个模块应引用同一个 C# 类型。组件或类库把它传给下一个 npm 函数时保留原对象。只接受原始 ref 的函数仍可通过 Vue 接口接收它。若接收方还要求附加成员，C# 实参须来自相应的扩展类型；含不同泛型实参的可写引用也要分别检查。成员的 TS 源位置用于编辑器导航，包、版本、包内声明和实例化决定类型身份。

### 开放字符串键的读写

遇到 `T extends Record<string, any> = D` 且返回 `T` 时，先确定这次调用实际选择了哪个 `T`。省略所有能推断 `T` 的参数后，若 checker 确认取默认类型 `D`，结果就按 `D` 的完整索引值域生成。此时调用者没有提供能证明自定义 DTO 的实参。显式传入独立于 `T` 的模式枚举时，同样需要检查它选择的重载。

VueUse 的 URL 参数默认值域是 `Record<string, string | string[]>`。同一个键在 `?tag=vue` 中读出字符串，在 `?tag=vue&tag=jolt` 中读出数组，删除后读到 `null`。索引读取跟踪当前缺席的键，并区分对象自有字段和原型成员。可写声明生成 `Set(key, value)` 和 `Delete(key)`，分别发射属性赋值与 JS 的 `delete`。这样缺席时的读取类型可以包含 `null`，而写入值保持上游的非空域。读取的数组保留身份，修改元素后 VueUse 会同步更新 URL。[挂载验收](../../scripts/csharp/fixtures/jolt-npm-runtime/verify-url-params.mjs)

## 结构稳定的代码缓存

项目保留两层缓存。第一层按项目、模块、导出选择和工具版本缓存 checker 元数据，验证锁文件、实际解析入口和参与解析的声明。第二层按归一化的导出选择、映射后的签名和生成工具身份缓存 C# 源码。组件与公开类库模式各有对应源码，每个消费者重新绑定自己的 Roslyn 符号。[元数据缓存](../../src/Jolt.Npm/NpmVueMetadataProvider.Cache.cs) · [代码缓存](../../src/Jolt.Compiler/JoltNpmProjectionCache.cs)

| 变化 | 应观察到的行为 |
| --- | --- |
| 编辑模板、调整导出请求顺序、重启 CLI 或 LSP | 相同调用契约复用原 C#。稳定排序适用于无序集合，TS 重载顺序、元组位置、泛型绑定次序和有语义的参数顺序须保留。 |
| TS 注释移动 | 更新定义位置，已有源码与声明索引重新绑定。绝对路径和行列用于导航，可影响公共类型名的包内声明身份参与源码键。 |
| 改变返回槽位、读写权限、缺省策略、枚举值、固定参数或宿主类型 | 新语义进入缓存键。不同实例化、规则版本或 ABI 将产生不同代码条目，后续构建复用新条目。 |
| 多进程同时生成、输出目录丢失 | 同一源码键由先完成的进程原子发布。其他进程核对相同内容后直接复用，编辑器可以继续读取该文件。删除公开输出后，下次构建可以从缓存恢复 `Bindings.g.cs`。普通类库将它编入 DLL，消费者通过程序集中的成员属性恢复 ESM 入口与 JS 名称。 |

按请求的导出和可达类型投影。在同一 checker 实例内共享声明及其已计算的方向和实例化，递归处复用引用。只有必须在具体实参处决定的调用才需要额外的 checker 解析。常规类型传递只产生 C# 作者契约，运行时继续直接使用 npm 值。

## 泛型实例方法与缓存值

从 npm 取得的实例由原 JavaScript 对象持有状态。投影一个实例方法时，按实参数量依次考察声明中的重载，保留实例接收者。下面四组调用共用同一缓存值类型的判断，适用于 Vue Query 的 `QueryClient` 以及声明具有相同关系的其他实例。

| 声明关系 | 可生成的调用 | 使用点的约束 |
| --- | --- | --- |
| 输入键可接受普通只读数组，返回 `T \| undefined` | 按作者给定的 `T` 读取。带标签的重载须先证明普通数组落入使用 `T` 的分支。 | 同一查询键的读写类型由应用约定。查询键的字符串和数字本身不能确定记录结构。缺值读作 `null`，保留 `0`、`false`、空字符串及原记录。 |
| 输入接受值或接收旧值的 updater，结果仍为同一槽位 | 分别推导直接写入和回调更新。检查 `NoInfer` 所包裹的槽位和默认值，确保回调所读、所写与结果对应。 | 上游可把 `undefined` 交给 updater。若开放放弃更新的写法，须将此操作传回上游为 `undefined`。直接写入 `null` 有不同的 JS 语义。 |
| 选项中的回调产出 `Promise<T>`，方法结果为 `Promise<R>` | 当 `R` 与 `T` 是同一槽位，或未提供的 `R` 默认绑定到 `T` 时，提供返回 `IPromise<T>` 的入口。 | 按实际字段名生成键和回调，保留所有必填输入。若之前的重载对同一选项有不同结果，需要先解决重载选择。 |
| 实例方法允许省略过滤对象，类型参数均有默认值，结果为 `void` | 分别生成无参调用和已确定字段类型的过滤对象调用。 | 无参时发射零个 JS 实参。按键精确删除时保留 `exact`，有相同前缀的其他条目应继续可读。 |

更新回调的输入缺值与输出缺值方向相反。公开 DLL 在回调参数上保存这个约定。编译器在调用点捕获委托一次，调用它时把旧值的 `undefined` 转为 `null`，把作者返回的 `null` 转回 `undefined`。普通直接调用、具名实参重排和已绑定的方法组均使用这一规则。已存的记录按原 JS 对象回传，直接值的重载接受标量或结构记录，普通 C# 记录可以由 updater 返回。[回调与缺值规则](../../src/Jazor.Compiler/core/SemanticWalker.cs.Argument.cs) · [公开类库使用](../../scripts/csharp/verify-jolt-public-projection.cs)

对于默认 schema 为 `never` 的条件声明，先求出接收者的实际泛型实参，再检查默认选中分支和可写域。Vue I18n 的动态消息树允许公开为 `VueProps` 并回传给另一个 locale。若声明给索引值或某个键指定了更窄的类型，写入面也要相应收窄。普通字符串 locale 对应的值可以是空对象，因此仅凭 locale 无法给调用方提供固定成员。[声明判定](../../src/Jolt.Npm/locale-message-projection.mjs) · [替换、合并与读取](../../scripts/csharp/verify-jolt-i18n-plugin.cs)

库的事件声明也决定回调参数。`IEvent<void>` 可以由 C# 的无参 `Action` 订阅。布局事件的回调读取上游给出的尺寸，查询布局时可使用同一具名只读结构。两种事件都使用上游返回的订阅句柄释放资源。Monaco 执行编辑器命令时还需传齐 `source`、`handlerId` 和 `payload`，最后一个声明为 `unknown` 的实参可用结构记录提供。[Chromium 事件与命令验证](../../scripts/csharp/fixtures/jolt-monaco-browser/main.js)

## 实施与验收

对单一调用签名，如果必填实参能确定全部开放类型参数，读取器直接生成投影。回调的返回值可提供推断依据，回调形参可从其他实参获得上下文类型。可表示的尾部可选参数按原顺序保留。例如 `mapItems<T, U>(items: readonly T[], select: (item: T) => U): readonly U[]` 保留输入元素 `T`，并从回调推断结果元素 `U`。VueUse 的 `refThrottled<T>(value: Ref<T>, delay?, trailing?, leading?)` 和它的别名也由这条规则处理。只传入 ref 与显式传入零延迟各自遵循上游行为。[直接签名读取](../../src/Jolt.Npm/metadata.mjs) · [VueUse 运行用例](../../scripts/csharp/fixtures/jolt-npm-runtime/Getters.jazor)

命名接口或对象类型别名的声明形参和导出函数的形参各有独立作用域。例如 `swap<A,B>(v: Pair<A,B>): Pair<B,A>` 只生成一个 `Pair` 声明，两处使用分别排列实参。具有必填数据属性的泛型记录可用于构造、读取和跨包回传，包含通过只读数组引用自身的树和相互引用且类型实参有限的记录。可写字段生成 `required ... { get; set; }`，只读字段生成 `required ... { get; init; }`。链接包和重导出按类型所属包名、版本、声明文件的包内路径和限定名称确定共享身份。结果对象可以直接传给另一导出，普通类库与组件都能读取对方写入的字段。[命名记录读取](../../src/Jolt.Npm/named-record-projection.mjs) · [公开 DLL 消费](../../scripts/csharp/verify-jolt-public-projection.cs)

直接传递的 Vue 容器从 Vue 自身的类型声明识别。`Ref<T>` 的读写域相同时可在两侧写入，`Readonly<Ref<T>>` 提供只读视图，`ComputedRef<T>` 保留 computed 宿主身份。带有不同 setter 类型的 ref 由专门的读写规则检查。将同一可写 ref 通过两个 npm 函数分别取得可写和只读视图，向前者写入的数值可以从后者读出。两个视图在 JavaScript 中指向同一对象。[宿主与泛型回归](../../src/Jolt.Npm/verify.cs)

Vue 的 `Ref<R, W>` 分开声明 getter 和 setter。两侧类型可独立表示时，公开的 `Value` 读出 `R`，`WriteValue` 允许写入 `W`。若所有读出的值都能合法写回，`Value` 也保留 setter。两种写法都修改同一个 JS ref 的 `.value`。例如读字符串、写数字的 ref，给 `WriteValue` 赋 `0` 合法，给 `Value` 赋字符串触发 C# 诊断。[泛型声明回归](../../src/Jolt.Npm/verify.cs) · [跨包 ref 用例](../../scripts/csharp/verify-jolt-public-projection.cs)

对 `Ref<T> & { … }` 的结果，附加字段和 ref 的 `value` 属于同一个 JS 对象。先确定交叉类型里的 Vue 宿主及 `T` 的完整读写域，再生成能访问这些字段的具名宿主类型。VueUse 的 `useColorMode()` 是一项对照用例，手写调用能读写 `Value`、读写 `Store.Value`，还能读取 `System.Value` 和 `State.Value`。锁定声明的默认 `Value` 可取 `auto`、`light`、`dark`，`System.Value` 和 `State.Value` 只有后两者。挂载后在 C# 给 `Value` 赋 `Dark`，`Store.Value` 随之更新，HTML 根元素的主题属性变为 `dark`。[手写参考](../../src/ECMAScript.VueUse/Types/VueUse.Types.Returns.cs) · [运行用例](../../scripts/csharp/fixtures/jolt-npm-runtime/verify-color-mode.mjs)

当两个 npm 模块分别返回和接收 `Ref<SwitchState>`，且二者引用同一个字符串字面量类型别名时，一起生成这两个模块的投影会得到一个共享的 C# 枚举。身份包含声明所在包、版本和包内位置。调用者可将取得的 ref 直接交给另一个包，原 JS ref 和允许写入的取值域都保留下来。只选择消费模块的独立生成也包含它所需的枚举。[跨模块生成测试](../../src/Jolt.Compiler.Test/NpmHandleTests.cs) · [公开 DLL 调用](../../scripts/csharp/verify-jolt-public-projection.cs)

若参数要求 Vue 的 `WritableComputedRef<R, W>`，还须保留 computed 的声明身份。读写域相同时复用手写的 `VueWritableComputedRef<T>`。读写域不同时使用双泛型 computed 类型，按同一条可赋值规则决定能否用 `Value` 写回。这样，手写的 `Vue.Computed` 和从 npm 得到的可写 computed 都能传给 `forward<R,W>(value: WritableComputedRef<R,W>)`。普通 `Ref` 即使值类型相同也会在传参处产生诊断。各次调用都使用同一 JS 对象。[Vue 宿主类型](../../src/ECMAScript.Vue/Types/Vue.Types.Structural.cs) · [生成签名](../../src/Jolt.Compiler/JoltNpmValueProjection.cs)

对于返回的普通 `Ref<R, R | null | undefined>`，只要两处 `R` 相同，就用一个允许读写空值的 `Value`。锁定的 VueUse 15.0.0 中，给 `useStorage` 的返回值写入 `null` 后，同步读取也会得到 `null`，因此数值存储的读取类型为 `double?`。JS 的 `undefined` 在 getter 处归一化为 C# 的 `null`。将 ref 作为函数输入时，还要能够接受库写入的完整值域。[存储操作](../../scripts/csharp/fixtures/jolt-npm-runtime/Storage.jazor) · [客户端断言](../../scripts/csharp/fixtures/jolt-npm-runtime/verify-storage.mjs)

当标准 `Pick` 或 `Partial<Pick<…>>` 能解析出有限字段时，投影保留各字段的泛型值、只读性与可缺席性。例如 `type CountPatch<T> = Partial<Pick<Source<T>, "count">>` 中的 `count?: number`，C# 可用 `new() { Count = 0 }` 和 `new()` 分别提供显式零或省略该键。读取结果的 `Count` 为 `double?`。同一模块的两个导出若使用同一命名映射、相同类型实参及兼容字段，可直接将取回的映射对象传给接收方。对直接引用类型参数的可选字段，沿用前述存在性检查。开放键集与可写 ref 内的映射视图仍需确定读写表示。[有限映射回归](../../src/Jolt.Npm/verify.cs) · [跨程序集使用](../../scripts/csharp/verify-jolt-public-projection.cs)

第二批已覆盖一个选项记录和一个结果记录之间的两个独立类型参数。生成器为可缺省的值类型槽位加上 `struct` 约束，使 `T?` 在生成的 DLL 中仍是 `Nullable<T>`。例如 Vue Query 的 `MutateAsync(0)` 完成后，同一次调用可读到 `Variables.Value == 0`、`Data.Value == false`。这两个属性在调用前都可读为 `null`。[签名特化](../../src/Jolt.Compiler/JoltNpmValueProjection.cs) · [真实包调用](../../scripts/csharp/fixtures/jolt-npm-runtime/Query.jazor)

第三批已覆盖分页选择结果和表单记录的连续属性路径。分页选择器的参数保留页与游标类型，其返回的 DTO 成为结果 ref 的读取类型。通过回调和 ref 共享的类型参数可以识别改名为 `projectView` 的选择器；分页上下文仍使用 `queryKey`、`pages` 等成员约定。缺省值规则也适用于选择结果为 `bool` 的实际查询，SSR 和客户端都能区分加载前的 `null` 与加载后的 `false`。[声明回归](../../src/Jolt.Npm/verify.cs) · [运行夹具](../../scripts/csharp/fixtures/jolt-npm-runtime/Query.jazor)

普通对象的单键访问由 `K extends keyof T` 与同一声明的 `T[K]` 决定。公开 getter 的结果与 setter 的值都使用选中字段的 C# 类型。对于实际名为 `pricing.total` 的字段，`Fixture.Assign(entry, x => x.FlatTotal, 0)` 发射字符串实参 `"pricing.total"`，由 npm 使用完整键索引对象。此入口只接受一段结构属性，普通 C# 记录可直接传入。[声明读取](../../src/Jolt.Npm/dependent-path.mjs) · [消费用例](../../scripts/csharp/verify-jolt-public-projection.cs)

当 `T[K]` 出现在返回的 Vue ref 中时，先解析宿主类型，再把该槽位绑定到所选属性。例如 VueUse 的两个 `useVModel` 重载都接受 props 和显式 key。两者在这两个实参上接受相同的记录和键域，省略余下参数时按上游顺序选择第一个，返回 `WritableComputedRef<P[K]>`。投影器检查键的约束、根记录与结果槽位，以及可能接收这组实参的其他重载。针对特定键的早期重载会使通用入口失去依据。[单键投影](../../src/Jolt.Npm/dependent-path.mjs)

同一调用中的选项要与键一起求解。以锁定的 VueUse 声明为例，选择 `Amount` 后，`defaultValue?: P[K]` 的输入、`shouldEmit?: (v: P[K]) => boolean` 的回调形参和 computed 的读写值都使用该属性的类型。属性值仅在运行时发生改变，C# 推断依赖的是选择器的返回类型。对其他包的同形声明，根、键与索引按 checker 符号对应，字段名来自实际选项声明。

| 实参 | 生成规则 | 发射与验证 |
| --- | --- | --- |
| 显式 key、选项含默认值或回调 | 选项的索引值绑定到选择器。区分不提供默认值、显式 `0` 与显式 `false`。 | 选择器发射为单一 JS 键。保留原实参位置，例如 `useVModel(props, "amount", undefined, options)`。 |
| 选项决定重载 | 只有判别字段能随构造的选项确定时，才为该分支提供固定判别值的签名。省略整个选项时仍按 TS 重载顺序选取结果。 | 默认 computed 和显式 passive 的返回值分别绑定到各自宿主。回调返回 `false` 的事件不得到达父组件。 |
| `Ref<UnwrapRef<P[K]>>` | 同时检查索引来自当前键及 `UnwrapRef` 来自 Vue。字符串、布尔和数值字段使用标量重载。对于封闭的嵌套数据记录，使用点逐层检查 Vue 深层解包是否保留原 C# 属性类型。 | passive 返回普通 ref。验证父组件替换整个记录后，子组件能读取新值，子组件赋值只向父组件回传一次。若记录里嵌有 ref，诊断指出具体字段。 |

```csharp
var amount = VueUse.UseVModel(props, x => x.Amount, new()
{
    DefaultValue = 0d,
    ShouldEmit = value => value >= 0,
});
var draft = VueUse.UseVModelWithPassiveTrue(props, x => x.Amount, new() { DefaultValue = 0d });
```

对于普通记录，`UseVModelWithPassiveTrueRecord(props, x => x.Record, new())` 使用上游的 `passive: true`。目前可用的载荷是非泛型的密封记录类，属性由嵌套的同类记录及 `string`、`double`、`bool` 组成。父组件替换记录后，`draft.Value.Info.Label` 可以直接读到新字符串。若 `Info` 内含 Vue ref，Vue 会解包这个成员，原 DTO 的成员类型已经不同，需要给读取和写入分别建模。克隆选项在默认 computed 入口可使用 `UseVModelWithCloneCallback`。显式 `emit` 的锁定参数为 `(name: Name, ...args: any[]) => void`，单凭声明只能推出事件名的约束。[组件用例](../../scripts/csharp/fixtures/jolt-npm-runtime/Model.jazor) · [使用点验证](../../src/Jazor.Compiler/PropertyPathSelector.cs)

同一规则遇到 `ComputedRef<P[K]>` 会返回只读的 `VueComputedRef<TValue>`。若所选属性依赖存在性检查或需要把缺席的 JS 值转换为 C# 的 `null`，通过 ref 的间接读取会绕过该属性的 getter。编译器在选择器处给出诊断，作者可改为直接读取该属性。该约束随投影写入公开 DLL。[使用点检查](../../src/Jazor.Compiler/PropertyPathSelector.cs) · [元数据回归](../../src/Jolt.Npm/verify.cs)

表单读取器识别两种已验证的递归路径声明。对于匹配的表单，Jazor 使用 Roslyn 的属性符号生成路径实参。内联选择器可使用嵌套字段的实际 JS 名。顶层字面键 `invoice.total` 编码为 VeeValidate 的 `[invoice.total]`，顶层 `invoice[label]` 直接传递。嵌套段中的分隔符、数组下标以及会被上游解释为下标的字面键仍需独立契约。普通记录的单键 API 则按完整键名索引。写入非空引用字段时也检查实际值的可空流状态。[公共消费用例](../../scripts/csharp/fixtures/jolt-binding-projection/CorrelatedConsumer.cs) · [路径使用点](../../src/Jazor.Compiler/PropertyPathSelector.cs)

若返回声明为 `B extends true ? Detail : number`，且唯一的布尔泛型 `B extends boolean` 对应一个独立的判别参数，读取器分别以 `true` 和 `false` 实例化同一个 TS 签名。对 `selectMode(label, enabled, count)`，C# 的 `SelectModeWithEnabledTrue(label, count)` 返回详情对象，`SelectModeWithEnabledFalse(label, count)` 返回数值。程序集元数据携带布尔值和原参数位置，直接调用和方法组转换都能调用原导出。判别参数可省略且后面都是可选参数时，原方法名还可表示调用到判别参数之前就结束，返回该省略方式的确切类型。其他实参参与判别值推断时，需要先证明这些实参和结果的关联。真实导出名优先，特化名称全部被占用时会给出诊断。[布尔条件投影](../../src/Jolt.Npm/finite-boolean-projection.mjs) · [消费用例](../../scripts/csharp/verify-jolt-public-projection.cs)

| 批次 | 一次实现的规则 | 交付条件 |
| --- | --- | --- |
| 1. 开放泛型的直接传递 | 在公共签名读取路径处理独立类型参数穿过函数值和标准 Promise 的可表达子集。覆盖普通输入、回调输入、回调返回与导出结果。 | 同一规则处理重命名声明和不同槽位排列。生成的 C# 可推断类型，跨程序集消费能运行。约束、未知结果和条件类型保持原声明边界。 |
| 2. 容器、可写性与缺省 | 将已有 ref、数组、记录和异步结果投影接到同一个泛型作用域和方向规则上，统一计算读写和可空约束。 | VueUse、Vue Query、Floating UI 共用规则。验证 mutation 的两个可缺省结果、ref 身份和数组回写。 |
| 3. 依赖键与条件返回 | 按符号求解键、路径、回调选择结果和有限判别。把当前专项读取器里相同的关系迁移到公共规则。 | VeeValidate 路径与值类型相关，分页默认和显式选择有各自结果，I18n 两种模式保持准确。 |
| 4. 七库整体验收与替换 | 对照每个手写调用所用的参数、可读写成员和下一次调用，合并可被通用规则完整覆盖的专项分支。 | 七库组件与 DLL 使用同一批 C# 调用。执行涉及 SSR、客户端响应式更新、资源清理及 Monaco worker 的真实场景，列出仍缺的操作。 |

每批按规则运行同一组声明测试、Roslyn 正负消费、JS 行为和缓存验证。改名覆盖相同结构，近似声明覆盖语义变化。覆盖率分开记录导出、实际可绑定的调用链和已运行的场景，定位未覆盖的参数关系或操作。[声明门禁](../../src/Jolt.Npm/verify.cs) · [独立类库](../../scripts/csharp/verify-jolt-public-projection.cs) · [七库对照](../../scripts/csharp/verify-jolt-binding-projection.cs) · [缓存验收](../../scripts/csharp/verify-jolt-projection-cache.cs)
