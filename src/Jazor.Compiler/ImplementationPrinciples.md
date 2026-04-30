# Jazor.Compiler 实现路线与决策原则

> Status: Active
> Updated: 2026-04-26
> Positioning: 从现有实现反推编译器路线、价值排序、硬边界与扩展判据。

## 1. 为什么写这份文档

`Jazor.Compiler` 现有文档已经覆盖了较多模块、能力和转换细节，但仍然缺一份专门回答下面这些问题的文档：

- 这个 compiler 真正把什么当成第一等公民？
- 当“完整语义等价”不可能时，优先保什么，允许丢什么？
- 为什么有些特性被擦除、有些特性走协议模拟、有些场景直接失败？
- 新增一个语法或宿主能力时，应该落到哪一层，以及按什么判据设计？

本文件不试图替代功能规格，也不试图成为测试清单。它的目标是记录 `Jazor.Compiler` 反复出现、且已经由实现与测试共同证明的设计立场。

## 2. 总体定位

`Jazor.Compiler` 不是“把任意 .NET 程序完整编译成 JavaScript”的通用 CLR-to-JS 编译器。

它更接近下面这个定义：

- 在受控输入域内工作；
- 以 Roslyn `IOperation` 语义为主输入；
- 以 ESTree 为核心输出；
- 在声明支持的边界内追求使用点可观察行为等价；
- 允许引入大量 synthetic lowering；
- 但要求宿主语义边界、命名、作用域、导入和调试锚点都保持确定且可解释。

换句话说，`Jazor.Compiler` 的目标既不是“尽量像 C#”，也不是“尽量像手写 JS”，而是三件事：

1. 使用点可观察行为
2. 宿主语义边界
3. 发射结果确定性

## 3. 三个第一等公民

### 3.1 使用点可观察行为

编译器首先要保证的是用户在使用点上能观察到的行为，而不是底层 runtime 结构。

这里的“可观察行为”主要包括：

- 求值顺序
- 副作用次数
- 最终结果值
- 分支路径
- 抛错/异常行为
- 调用前后可见的状态变化

### 3.2 宿主语义边界

编译器必须明确区分：

- 哪些用户代码允许进入编译域；
- 哪些外部类型和成员是已支持的宿主能力；
- 哪些语义必须通过 `WhiteList` / `Compile` 显式声明；
- 哪些能力不支持，必须直接失败。

边界的清晰度比“多支持一点点”更重要。没有边界，所有 lowering 最终都会被 fallback 污染。

### 3.3 发射结果确定性

同一份输入应尽量得到稳定、可复现、可比对的输出。

这里的确定性包括：

- lowering 临时名稳定
- import 绑定和 alias 稳定
- 作用域内名字不抖动
- 模块级导入顺序稳定
- sourcemap 锚点可预测

这不是测试附属品，而是 compiler 的产品属性。

## 4. 等价模型

“语义等价”在跨语言编译里不是单层概念。对 `Jazor.Compiler` 来说，应明确区分三层。

### 4.1 可观察行为等价

这是第一目标，也是必须优先保护的一层。

只要某个 lowering 可能破坏：

- 求值顺序
- 副作用次数
- 最终返回值
- 调用前后可见状态

就必须优先修正，即便代价是多引入若干 temp、`SequenceExpression` 或 IIFE。

### 4.2 使用点语义等价

当某个语言特性在当前支持边界内只需要保“可用行为”时，compiler 可以不保其完整 runtime 身份。

例如 tuple：

- 保投影
- 保解构
- 保比较
- 保视图 remap

但不保 `System.ValueTuple` 的 CLR runtime 身份。

### 4.3 运行时结构等价

这一层不是 `Jazor.Compiler` 的默认承诺。

只有当某个 runtime 结构对于目标平台语义同样重要，且保留成本合理时，才值得保留。否则优先考虑擦除或协议模拟。

## 5. 行为保真优先级

当“完整复刻”做不到时，保真优先级应按下面排序理解：

1. 求值顺序
2. 副作用次数
3. 最终结果
4. 使用点语义
5. 运行时结构身份

这条优先级解释了许多现有实现取舍：

- tuple 可以擦除
- `ref/out` 可以协议化
- conditional access 可以先缓存再 guard
- object initializer 可以包进 IIFE

## 6. 支持边界与失败策略

### 6.1 Compiler 是 fail-fast 的

`Jazor.Compiler` 不是 best-effort transpiler。

当外部类型、外部成员或某类运行时语义不在支持边界内时，编译器的默认行为应是：

- 明确拒绝
- 给出清晰错误
- 禁止静默退化成“看起来像 JS”的近似实现

特别是下面这类情况，不能回退成 raw JavaScript fallback：

- unsupported external member access
- unsupported runtime host invocation
- unsupported property/indexer mutation path
- unsupported pattern/runtime property probing

### 6.2 边界按使用点裁决，而不是按“类型出现”裁决

支持与否不由一个类型是否“出现过”决定，而由它是否在当前 lowering 点需要真实运行时语义决定。

因此：

- `List<Unsupported>` 可以允许
- `Task<Unsupported>` 可以允许
- `Dictionary<TKey, Unsupported>` 可以允许
- `Unsupported[]` 可以允许

但以下使用点应严格失败：

- `new Unsupported()`
- 需要 concrete lowering 的 `default(Unsupported)`
- 对 `Unsupported` 的静态/实例成员访问
- 需要 runtime-sensitive materialization 的场景

### 6.3 `Analyzer` 是前置收口，`SemanticWalker` 是最终裁决

职责不应描述为“Analyzer 负责所有合法性，SemanticWalker 只负责 lowering”。

更准确的说法是：

- `Analyzer` 负责尽早收紧输入域；
- `SemanticWalker` 负责在具体使用点做最终可落地裁决。

原因很直接：很多“能不能支持”只有在 lowering 现场才知道，因为它依赖：

- 当前语义位置
- 当前目标 AST 形状
- 当前宿主映射是否存在
- 当前是否要进入 runtime-sensitive lowering

## 7. 四类 lowering 路线

### 7.1 值擦除型 lowering

适用场景：

- 某个特性本质上可以被更简单的值组合替代；
- 用户真正依赖的是使用点行为，而不是 runtime 类型身份。

典型代表是 tuple。

tuple 在 Jazor 中应视为“可擦除的复合值”：

- 保位置语义
- 保目标视图 remap
- 保解构和比较结果
- 不保 CLR runtime 身份

### 7.2 协议模拟型 lowering

适用场景：

- 目标平台没有原生等价 runtime 机制；
- 但使用点行为仍然值得保留；
- 可以通过 caller/callee 双端一致协议保持行为闭环。

典型代表：

- `ref/out`
- 自定义 `Deconstruct`

这类 lowering 的重点不是“复刻底层模型”，而是定义一个清晰且内部一致的协议。

### 7.3 表达式闭包型 lowering

适用场景：

- 语义必须保留在表达式位置；
- 但正确实现它需要 statement 级能力；
- 或者需要先缓存再继续组合。

典型手法：

- `SequenceExpression`
- `ConditionalExpression`
- IIFE
- 局部 temp cache

典型场景：

- conditional access
- switch expression
- pattern matching 分支
- object initializer
- 带回写的调用表达式

### 7.4 宿主缝型 lowering

适用场景：

- 某个行为本质上不是纯 C# 语法问题，而是“外部 API 如何落到 JS”。

这类问题应优先落到：

- `Alias`
- `Inline`
- `Import`
- `Compile`

而不是散落在普通语义分支里。

## 8. 声明级对象模型路线

除了方法体内的表达式与语句 lowering，`Jazor.Compiler` 还必须对声明级对象模型做明确裁决。当前最关键的三类是：

- enum
- interface
- class inheritance
- record

### 8.1 record 的路线：结构化值对象，不保 nominal runtime identity

`record` 在当前编译器里应视为“结构化值对象声明”，而不是“语法更短的 class”。

这条路线的约束是：

- `record` 创建默认 lower 成对象字面量，而不是 `new Record(...)`
- `with` 表达式默认 lower 成对象 spread
- record 位置模式/属性模式默认按结构属性键匹配
- record 解构赋值默认按结构属性键展开，而不是依赖实例 `Deconstruct()` runtime protocol
- 模块层与成员层都不为 `record` 发射 runtime class 声明
- `Description` / `ECMAScriptName` 之类命名配置只影响结构属性键或编译期名称，不会把 `record` 抬升回 nominal class 语义

换句话说，`record` 保的是：

- 属性 shape
- 位置/属性匹配行为
- `with` 的非破坏性更新语义

而不保：

- `instanceof Record`
- 模块级/成员级 runtime class 声明
- 默认的 nominal runtime type identity

如果用户需要普通 class 的运行时语义，应显式写 `class`，而不是依赖“带名字的 record”“带配置特性的 record”或其他隐式升级路径。

它们不能再被视为“以后再看”的零散实现点，因为三者直接决定模块级输出模型是否自洽。

### 8.1 enum：编译期值域类型，运行时擦除为底层常量

enum 的路线应定义为：

- 默认不发射独立的 JS runtime declaration；
- 定义只作为编译期值域类型存在；
- 使用点统一降级为底层标量常量或标量表达式；
- enum typed local/field/parameter/return 在运行时都只是标量；
- 不提供 CLR enum runtime 身份；
- 不提供 reverse-map 风格的 JS 枚举对象。

也就是说，推荐模型不是“保一个模块级枚举对象再在运行时引用它”，而是：

- enum declaration -> compile-time only domain declaration
- enum member usage -> scalar literal / scalar expression
- enum typed runtime value -> scalar runtime value

这条路线的好处是：

- 与 tuple 一样，把重点放在使用点行为，而不是 runtime 身份；
- 与比较、赋值、`switch`、`default(enum)`、`Flags` 位运算等高频语义天然一致；
- 避免为了少量名字语义去伪造一个其实并不稳定的 runtime enum object；
- 让模块级对象模型更清晰，避免把声明级 artifact 和使用点语义混在一起。

需要额外写清楚的边界：

- `Flags` 只是 bitmask 值域，不是特殊 runtime 对象；
- `E.A`、显式/隐式到底层数值类型的转换、`default(E)`、nullable enum 的值语义，都应按底层标量处理；
- `typeof(E)`、反射、装箱身份、`System.Enum` 家族 API、按名字取值/格式化等能力，不应被默认视为仍然成立；
- 如果未来确实要支持“按名字看 enum”的能力，应通过显式 host seam 或编译期元数据映射引入，而不是反向恢复一个伪 CLR enum object；
- 对于超出 JS `Number` 安全整数范围的底层枚举值，编译器不能继续假装“普通 number 就够了”；
- 这类场景必须二选一：
  - 升级到精确的 `BigInt` 枚举路线；
  - 或显式拒绝，直到存在稳定实现。

如果当前实现里仍然存在 `Object.freeze({...})` 式的枚举声明输出，它只能被视为过渡态，而不是长期路线本身。

### 8.2 interface：只作为契约，不发射 runtime artifact

interface 的路线应定义为：

- 一律不发射 JS runtime declaration；
- 一律不生成 JS interface object；
- 默认只作为编译期契约存在。

它在 compiler 中仍然可以参与两类事情：

1. 约束与合法性判断
2. 宿主语义投影与白名单/实现面查找

因此，interface 的正确理解不是“尚未支持输出”，而是：

- 它本来就不是主要输出 artifact；
- 它是 contract / projection seam。

这条路线同时解释了为什么：

- 某些 `IEnumerable<T>` / `IList<T>` / runtime host 接口能参与 lowering；
- 但 nested interface declaration 本身仍然不应被输出成 JS 声明。

### 8.3 继承：支持受控的 JS-compatible 子集，超出子集显式失败

JavaScript 原生支持 `class extends`、`super(...)` 和 prototype dispatch。  
因此对于与 JS 语义足够接近的 C# class inheritance，compiler 的目标不应是永久拒绝，而应是支持一个清晰、受控的子集。

当前已经落地、并应视为正式路线一部分的子集是：

- 同一模块成员类之间的单继承；
- declaration emission 必须先基类、后派生类，即使源码书写顺序相反；
- `extends`；
- `: base(args)` -> `super(...)`；
- 派生类未显式声明构造函数时，合成 `constructor() { super(); }`，避免静默擦除基类链；
- `base.Method(...)` -> `super.Method(...)`；
- `base.Property` / `base.Property = value` -> `super.Property` / `super.Property = value`；
- `base.Method` 方法组引用可通过局部 forwarder 保留“调用基类实现”的语义；
- override 后的普通实例方法沿 JS prototype dispatch 运行。

当前必须继续显式拒绝的路径是：

- `base.Field`；
- `this(...)` 构造函数链；
- 外部基类；
- 需要独立协议设计的构造函数初始化器能力，例如 named/ref/out 参数传递以及其他非常规参数绑定；
- 任何仍依赖 CLR 运行时身份或 metadata shape 的继承相关语义。

这里要特别记录一个实现约束：

- 只要 `base` receiver 还没有独立 lowering，绝不能把它复用成 `this`。

原因是当前实例引用 lowering 默认天然偏向 `this` 语义。  
如果在 `base` 路径上偷用同一路径，会把“调用基类实现”静默降级成“调用当前覆盖后的实现”，这是错误语义。

同时也要区分成员种类：

- base method / accessor 与 JS prototype 语义相近，可以走 `super`；
- base field 与当前 member-class field lowering 不相近，不能假装 `super.Field` 就等价。

当前 member-class field 的运行时形态是实例自有状态，而不是原型上的 descriptor。  
JS 的 `super.Field` 查找的是基类原型链，不会自然命中这类实例字段。  
因此在 base field 还没有单独协议前，显式失败比生成错误 JavaScript 更正确。

需要明确排除或单独设计的内容：

- 多继承
- interface runtime emission
- event 体系
- 依赖 CLR 元数据身份的 abstract/runtime 检查
- 与 JS 无直接对位的可见性或运行时约束

最重要的一条工程规则是：

- 在继承模型没有真正接通前，不能静默擦除继承。

也就是说，如果某个成员类声明了非 `object` 基类，但当前场景不在上述已支持子集内，就必须直接失败，而不是生成 `superClass: null`、省略 `super(...)`、或者把 `base` 悄悄变成 `this` 的错误 class shape。

### 8.4 成员类构造函数重载：单 `constructor` + 稳定 helper + 按参数个数分派

成员类构造函数重载不能照搬普通方法重载路线。  
原因不是“签名 hash 不够”，而是 JavaScript class runtime shape 只允许一个真实 `constructor`。

因此当前正式路线应定义为：

- 运行时始终只发射一个真实 `constructor`；
- 每个 C# 实例构造函数 body 下降为一个稳定命名的 helper method，例如 `$ctor_<hash>`；
- 真实 `constructor` 负责分派，不直接承载多套 body；
- 分派键当前限定为 `arguments.length`；
- optional parameter 的省略值在命中分支后按该 overload 自身的默认值补齐；
- 对派生类，每个分支各自先执行对应的 `super(...)`，再进入 helper body；
- helper / dispatcher 的插入位置保持稳定，跟随源码中第一处显式构造函数的位置，不额外把构造函数重排到 class 顶部。

这条路线支持的子集必须收紧为：

- overload 集合按“可接受参数个数区间”两两不重叠；
- 也就是每个 overload 的 `[requiredCount, totalCount]` 区间并集必须无冲突；
- 同 arity 重载直接失败；
- optional parameter 导致的区间重叠同样直接失败。

当前必须继续显式拒绝的路径是：

- `this(...)` 构造函数链；
- `ref/out/in/params` 参与的构造函数重载分派；
- 需要按类型、命名参数或其他 CLR 规则进一步判别的 overload 集合；
- 外部基类上的构造函数协议模拟。

这条设计的重点不是“尽量像手写 JS”，而是：

- 单一 JS class runtime shape 保持合法；
- 调用点 observable 的 overload 选择结果保持确定；
- 默认值补齐、`super(...)` 调用、body 执行顺序保持稳定；
- helper 名称和 class element 顺序保持确定性，不受遍历偶然性影响。

## 9. `ref/out` 的路线定义

`ref/out` 在 `Jazor.Compiler` 中不应被定义为“地址语义复刻”，而应被定义为“调用协议模拟”。

核心原则：

- callee 端负责把返回值和回写值打包；
- caller 端负责按稳定槽位解包并回写；
- 整个调用仍然可以参与外层表达式组合；
- 内部协议可以 synthetic，但必须全链路一致。

这里要保的是：

- 实参求值顺序
- `ref` 初值传入
- `out` 结果回写
- 回写发生在调用之后、后续消费之前

不保的是：

- CLR 地址语义本体
- 引用存储模型本体

## 10. tuple 的路线定义

tuple 在 `Jazor.Compiler` 中不应被定义为“runtime tuple 模拟”，而应被定义为“值擦除型 lowering”。

核心原则：

- tuple 视图按使用点和目标位置映射；
- tuple projection 可以重构对象形状；
- tuple compare 可以递归展开为逐槽位比较；
- tuple deconstruct 可以拆成赋值序列；
- 复杂源值宁可缓存一次，也不能重复求值。

这里要保的是：

- 位置语义
- 目标字段名落地
- 解构结果
- 比较结果

不保的是：

- `System.ValueTuple` runtime 身份
- 反射形态
- CLR 布局与装箱细节

## 11. synthetic node 与 synthetic semantics 的边界

### 10.1 synthetic node 是允许且必要的

为了正确 lowering，编译器可以并且应该引入大量 synthetic 节点，例如：

- temp identifier
- synthetic catch 参数
- IIFE
- sequence 包装
- packed return array
- tuple/cache helper

### 10.2 synthetic user-facing semantics 不允许

虽然 synthetic node 可以很多，但不应引入模糊或偷偷改变用户边界的 synthetic semantics。

允许的事情：

- 为了正确求值而插 temp
- 为了表达式位置而包 IIFE
- 为了 `ref/out` 协议而返回数组

不允许的事情：

- unsupported external member 假装可以普通访问
- unsupported runtime host 假装可以普通调用
- 通过静默 fallback 发明一套看似合理但未经声明的用户可见语义

## 12. `WhiteList` 与 `Compile` 的分工

### 11.1 `WhiteList` 绑定的是宿主语义声明，不是字符串替换

`WhiteList` 的本质不是“映射表文本替换”，而是 compiler 与宿主语义之间的正式缝。

匹配应围绕下面这些东西展开：

- `OriginalDefinition`
- override / reduced extension 链
- structural generic matching
- API 家族，而非单个偶然的绑定实例

### 11.2 `Compile` 是高阶出口，不是兜底补丁

`Compile` 的地位应高于传统 `Alias/Inline/Import`。

推荐理解是：

- `Alias`: 简单名字映射
- `Inline`: 局部、稳定、可模板化的表达式替换
- `Import`: 外部 helper/module 接缝
- `Compile`: 需要 AST 级决策、上下文参与、局部协议或复杂结构构造的宿主语义出口

如果某个宿主能力继续停留在 `Inline` 会导致：

- AST 结构脆弱
- sourcemap 难以维持
- 调试体验变差
- 规则组合性下降

那么它就应该升级到 `Compile`。

## 13. 作用域与命名模型

### 12.1 作用域按 JS 发射规则重建

`Jazor.Compiler` 不是在保留 C# 原树结构，而是在按 JS 发射规则重建作用域。

关键对象：

- `SenseArgument`
- `EmissionScopeContext`
- `UniqueNameSession`
- `ScopeSite`
- `LoweringSite`

这些设施共同表达的不是“当前语法在哪”，而是：

- 当前 lowering 属于哪个作用域；
- 当前 temp 应该落在什么边界内；
- 当前名字是否会与其它 lowering 位点冲突。

### 12.2 稳定命名依赖语义位点，而非访问顺序

lowering 名称不应依赖遍历顺序、递增计数器或偶然上下文。

稳定命名应由三部分共同决定：

- lowering owner 的语义身份
- 当前 emission scope
- 当前 lowering site

因此临时名不是“为了测试方便造出来的字符串”，而是 compiler 确定性的一部分。

### 12.3 import alias 稳定性是模块契约

模块级导入状态应在整个模块转换过程中共享。

这样可以保证：

- 同一外部符号在不同成员中得到同一本地名；
- 名称冲突时 alias 分配稳定；
- import dedupe 和排序稳定；
- 输出不因访问顺序不同而抖动。

## 14. “不向上遍历”的真实含义

`Jazor.Compiler` 的主原则应是：

- 默认通过 `SenseArgument` 显式传递语义上下文；
- 禁止依赖父链做隐式语义控制流；
- 允许少量局部、封闭、纯结构性的父节点查询。

也就是说，这条规则是工程纪律，不是形式洁癖。

允许的例外应满足：

- 只读取局部结构信息；
- 不把父链当作隐式业务规则系统；
- 不把重要语义判定偷偷转移到 `operation.Parent` 链上。

## 15. sourcemap 与 source-origin 的定位

### 14.1 source-origin 是 lowering 产物的一部分

“这个节点来自哪里”不是 emit 末端再猜出来的，而是在 lowering 时显式附着到 AST 节点上。

因此：

- lowering 不只是产出 AST shape；
- lowering 同时产出调试锚点。

### 14.2 sourcemap 服务源观察点，不服务还原 lowered 细节

sourcemap 的目标不是隐藏 lowering，也不是把 synthetic 节点伪装成原生存在。

它服务的是：

- 源级调试体验
- 断点与观察点对齐
- “这个生成位置应该归属于哪个真实源语义”的判断

因此：

- synthetic node 可以存在；
- synthetic origin 不应污染真实源锚点；
- child origin 应优先于 parent origin；
- 降级后的内部细节不要求被完整还原为源结构。

## 16. Optimizer 的边界

`Optimizer` 只能做语义保守的清理，不能承担主语义修复责任。

这意味着：

- 关键行为必须在 lowering 阶段定对；
- optimizer 只能做可证明安全的局部整理；
- 不能指望 optimizer 去补 `SemanticWalker` 的语义债。

换句话说，`Optimizer` 不是第二套编译器。

## 17. 直接构 AST 是主路

主 lowering 路径应坚持直接构造目标 AST。

Parser 可以存在，但只能作为少数受控语法孤岛的工具，例如：

- import declaration/specifier 语法片段
- inline template 的局部实例化

它不应成为主编译策略。否则：

- 语义约束会后移；
- AST 结构控制会减弱；
- deterministic 和 sourcemap 维护成本会上升。

## 18. 新增能力时的决策树

新增一个语法或能力时，应按下面顺序判断：

1. 它是在扩展输入域，还是在扩展已有输入域中的 lowering？
2. 它是用户代码合法性问题，还是宿主映射问题？
3. 它是模块/类型 shape 问题，还是方法体内语义问题？
4. 它是否要求真实 runtime 结构，还是可擦除？
5. 它是否需要 caller/callee 协议模拟？
6. 它是否必须保持表达式位置？
7. 它是否可能导致重复求值或副作用次数变化？
8. 它是否应该进入 `Compile`，而不是继续停留在 `Inline`？
9. 它是否会破坏稳定命名、稳定导入或 sourcemap 锚点？
10. 如果不支持，是否应该明确失败，而不是静默 fallback？

根据回答落点应大致是：

- 输入域与前置约束：`Analyzer`
- 宿主语义声明：`WhiteList` / `Compile`
- 模块级结构：`AstConverter`
- 方法体语义：`SemanticWalker`
- 后处理的小幅清理：`Optimizer`

## 19. 未来改动的检查清单

在提交任何 compiler 语义改动前，至少应追问下面这些问题：

- 这个改动保住了求值顺序吗？
- 这个改动会增加或减少副作用次数吗？
- 这个改动是否引入了新的隐式 fallback？
- 这个改动是否把应失败的场景错误地放宽了？
- 这个改动是否把应允许的 erased 场景错误地收紧了？
- 这个改动是否需要稳定 temp，但目前名字依赖遍历顺序？
- 这个改动是否把复杂宿主语义错误地塞进了 `Inline`？
- 这个改动是否破坏了 import alias 的模块级一致性？
- 这个改动是否插入了 synthetic node，但没有处理 source-origin？
- 这个改动是否把应由 lowering 负责的工作推给了 optimizer？
- 这个改动是否又把 enum 推回成 runtime declaration/object，而不是编译期值域类型？
- 这个改动是否把 interface 错误地推成了 runtime artifact？
- 这个改动是否在继承未打通时静默擦除了 `extends` / `super` 语义？

## 20. 非目标

下列目标不应被误认为当前 compiler 的核心追求：

- 完整 CLR runtime 模拟
- 任意 C# 语法的无条件支持
- 最少 AST 节点数
- 看起来最像人手写的 JavaScript
- 通过 parser 驱动主转换流程
- 通过静默 fallback 提升表面兼容率

## 21. 一句话总纲

`Jazor.Compiler` 的核心路线可以压缩成一句话：

在受控输入域内，优先保证使用点可观察行为、宿主语义边界和发射确定性；当完整 runtime 结构不值得保留时，允许擦除或协议模拟，但不允许静默伪造用户可见语义。
