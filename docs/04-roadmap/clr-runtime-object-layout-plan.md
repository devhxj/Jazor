# CLR Runtime Object 布局调整计划

> 状态：拟议。本文定义后续 CLR runtime 迁移的目标布局；在相应行为、compiler、browser 和 Release package 门禁全部通过前，不改变任何已发布支持面的状态。
>
> 关联：[RazorVue Blazor CLR 类型支持计划](./blazor-clr-support-plan.md) 负责 Blazor API 切片。本计划只收敛这些切片及既有 CLR 模块在浏览器中的内部对象表示，不能借此扩大作者可调用的 API 面。

## 1. 决策

`Jazor.CLR` 不再把“每个 CLR 类型都需要一个命名 JavaScript class”当作默认实现路线。默认选择应为：

```text
已绑定的 CLR 成员
  -> [Jazor] / whitelist dispatch
  -> object 或已有浏览器原生对象
  -> 模块私有 WeakMap / closure 中的结构化状态
  -> C# 编写的 runtime helper
```

这里的 `object` 是浏览器值的 opaque transport，不是向作者暴露无约束动态 API；作者代码仍通过 Roslyn 已绑定的 CLR 成员、WebIDL binding 和 whitelist 使用它。成员分发继续由源 CLR symbol 决定，不能因为 adapter 实现参数改为 `object` 而退化成字符串反射。

命名 runtime class 只在以下条件同时成立时才允许保留或新增：

1. 已有浏览器内建对象、`object` 加 side-table、Array/Map/Set 或闭包都不能表达必要的可观察行为；
2. 该行为需要可从未知 `object` 精确识别的 nominal runtime identity；
3. 有独立的 compiler emission、runtime 和消费者场景证明该身份是产品契约，而不是实现便利；
4. 评审明确记录为何不能改为非 nominal 布局。

当前已有的 `RuntimeModule.J*` 类型不能因历史存在而自动满足这些条件。每一类都必须按本计划的迁移矩阵重新裁决。

## 2. 不变量

1. 不引入 `__jazorType`、字符串 tag、全局 type registry、名称约定 special case 或结构 shape test 来识别 CLR 类型。它们会平行于 Roslyn/whitelist 建立第二套类型系统。
2. `[Jazor]` adapter 签名仍是 CLR 类型到内部 nominal carrier 的唯一推导源。adapter 使用 `object`、匿名类型、record、原生 WebIDL 类型或 Array/Map/Set 时，不得生成 `RuntimeValueCarrier` metadata。
3. 对没有 nominal carrier 的 CLR 类型，未知 `object` 上的 `is T`、`as T`、bare type pattern 和 `typeof(T)` 必须由 compiler 明确拒绝，或仅在 Roslyn 已证明结果时折叠；绝不发射假阳性的 property/tag 判别。
4. `object.Get/Set/Invoke` 和 `Reflect.Get/Set/Apply` 仅用于 CLR runtime 内已经擦除形状的边界。静态形状已知时，优先继续使用强类型 WebIDL 或 CLR adapter；字段 key 必须集中在 layout helper，不能散落进每个业务 adapter。
5. 新领域逻辑仍以 C# 写入 `[ECMAScriptModule]`，由既有管道编译。此次迁移不得新增 hand-written `.mjs`，也不得把状态机迁回 RazorVue framing。
6. 匿名类型和 structural record 只描述内部数据布局；不依赖其 CLR equality、`with`、clone、`ToString()`、`GetHashCode()`、`typeof` 或 nominal pattern 语义。JavaScript 产物是普通 object literal。
7. 不因内部实现简化而把公共 C# API 弱化为新的 `object`/`object?` catch-all。`object` 可出现在 CLR adapter 实现签名和模块私有 helper，不能成为作者面的替代类型设计。

## 3. 可复用布局积木

| 情况 | 默认表示 | 私有状态位置 | 适用边界 |
| --- | --- | --- | --- |
| 一次性、只读内部 payload | `new { ... }` 匿名类型 | object 自身字段 | 不跨 helper 暴露匿名静态类型；不要求变更或类型判别。 |
| 跨多个 helper 的不可变结构状态 | `private sealed record State(...)` | `WeakMap<object, State>` 的 value 或局部值 | record 只走 structural lowering；只能通过已声明属性读取。 |
| 可释放句柄 | `new { dispose = (Action)(...) }` | closure 捕获幂等标志和资源 | 对齐现有 `IDisposableModule` 的动态 `dispose` 协议。 |
| 可变服务/宿主 | `Object.Create(null)` 或只暴露命令的匿名 object | `WeakMap<object, State>` | 对外只给已声明的 CLR 成员；状态不写入 URL、VNode 或作者可见 payload。 |
| 已有 JavaScript 容器 | `Array`、`Map`、`Set`、`AbortSignal`、`AbortController`、DOM object | 必要时 `WeakMap` side-table | 优先保留浏览器原生 identity，不再包一层 CLR class。 |
| 值需要 coercion hook | opaque object + `Object.DefineProperty(..., Symbol.ToPrimitive, ...)` | module-private `WeakMap` 或 object fields | hook 和格式化集中在 creator/helper；不以 type tag 识别对象。 |

`Object.Create(null)` 只用于确实不应继承 `Object.prototype` 的 service/host；普通值 payload 优先匿名 object。需要跨多个 helper 保留字段类型时，使用 module-private structural record 作为 C# 源内 schema，而不是将 record 变回 runtime class。

## 4. 类型判别与 compiler 合同

当前 `RuntimeValueCarrier` 只服务于 runtime type check 和 `as` lowering，并不是 `[Jazor]` 成员分发的前提。object 化后的成员调用仍按已绑定 CLR member 进入对应 module；改变的是下列 nominal 能力：

| 使用点 | 迁移后的规则 |
| --- | --- |
| `date.Year`、`calendar.AddMonths(...)`、`registration.Dispose()` | 正常映射到其 `[Jazor]` member；adapter receiver 可为 `object` 或已有 host 类型。 |
| 静态类型已知的转换/模式 | 仅在 Roslyn 能证明结果和单次求值后折叠；否则仍按一般使用点规则处理。 |
| `object value; value is DateTime` | 明确失败，不根据字段、`constructor.name` 或 tag 猜测。 |
| `value as DateTime` | 明确失败，不生成“看似成功”的 raw JavaScript conversion。 |
| `typeof(DateTime)` | 保持不支持；内部 object layout 不是 `System.Type` token。 |
| native carrier 类型，例如 `AbortSignal`、`URL`、DOM event | 继续使用浏览器原生、可证明的判别，不受本计划影响。 |

执行时要同步调整三处测试/生成契约：

1. `Jazor.Compiler.Generator` 只从与目标 CLR member 对齐的 adapter 签名中识别 source-declared、非 record 的 `Jazor.CLR` runtime class 并推导 `RuntimeValueCarrier`；对 record 明确排除，避免其先被推导、后在 runtime emission 缺构造器。
2. `WhiteList.cs.Generate.cs` 中迁移类型的 type alias 不再携带 `RuntimeValueCarrierReference`；member key、`Op` 和 module path 仍由原有 `[Jazor]` 声明生成。
3. `SemanticWalker` 测试必须把旧的 `instanceof J*` 断言替换为“已知结果折叠”或“unknown-object 使用点失败”断言；不能仅删除测试而放宽诊断。

## 5. 迁移矩阵

| 组 | 当前/计划中的 class carrier | 目标布局 | 优先级 | 关键验收 |
| --- | --- | --- | --- | --- |
| 生命周期 handle | 计划中的 `JBrowserSubscription`、`JRuntimeLifetime` | `{ dispose }` 闭包句柄；lifetime object 作为 `WeakMap<object, LifetimeState>` key | P0 | listener 只注册一次、重复 dispose 无副作用、unmount 清除全部 listener。 |
| 导航 host | 计划中的 `NavigationRuntimeHost` | opaque `object` + `WeakMap` state；只暴露已映射 navigation 命令 | P0 | `popstate`/`hashchange` 所有权、取消/commit/replay、refresh callback 与原计划行为一致。 |
| 取消注册 | `JCancellationTokenRegistration` | opaque object + `WeakMap<object, RegistrationState>`；可选 `dispose` 命令复用注销核心 | P1 | 已取消 token 的同步回调、`Unregister` 返回值、重复 `Dispose`、abort 后状态。 |
| 小型值协议 | `JIndex`、`JRange` | 匿名 object / structural record；`IndexState`、`RangeState` layout helper | P2 | `^0`、边界、切片、`GetOffsetAndLength`、越界错误；unknown-object type test 失败。 |
| 可变 calendar | `JGregorianCalendar` | opaque object + `WeakMap<object, CalendarState>` | P2 | `CalendarType`/`TwoDigitYearMax` 读写、Calendar/GregorianCalendar 共享支持面、所有日期运算。 |
| 容器 | `JQueue<T>`、`JStack<T>` | 原生 `Array<T>`；Queue head/容量等私有 metadata 转入 `WeakMap` | P3 | FIFO/LIFO、枚举、容量、clear、copy、异常、与 Array identity 的 side-table 清理。 |
| 时间值 | `JDateTime`、`JDateTimeOffset`、`JDateOnly`、`JTimeOnly`、`JTimeSpan` | opaque object + `WeakMap<object, ...State>`；creator 集中安装 `Symbol.ToPrimitive` | P4 | tick 精度、kind/offset、格式化、比较、Calendar/DateOnly/TimeOnly 相互转换、跨 module 调用及无 tag type-test 边界。 |

P0 的类型尚未作为生产 class 落地时，禁止先创建它们再立即迁移；直接按目标 object layout 实现。P1-P4 只在上一阶段的行为和 compiler 边界都通过后推进。

## 6. 实施阶段

### C0：建立迁移护栏

1. 为每个候选 `J*` 建立 ledger：对应 CLR types、所有 `[Jazor]` adapter receiver/return/parameter、直接字段访问、runtime scenario、compiler type-pattern test、bundle consumer。
2. 在 generator/whitelist 测试中固定“`object` adapter 不推导 carrier、record 不推导 carrier”的规则。
3. 在 compiler 中固定 unknown-object nominal check 的失败诊断；诊断必须指出源静态类型、目标 CLR type 以及没有 runtime identity 的原因。
4. 更新 test host 的 value encoder：以可观察字段/行为编码，不再通过 `constructor.name === "J..."` 识别正确性。

### C1：先实现无 nominal identity 的 Blazor runtime 对象

1. 按 [Blazor CLR 类型支持计划](./blazor-clr-support-plan.md) 实现 navigation lifetime、browser subscription 和 host 时，直接使用 object/closure/WeakMap。
2. `blazor-routing.mjs` 只创建、释放并 provide host；listener、状态机和 private layout 均留在 C# CLR module。
3. 复用现有 `IDisposableModule` 的 `dispose` 探测，不为 subscription 建立新的 CLR wrapper type 或 whitelist entry。

### C2：取消与简单值

1. 先迁移 `JCancellationTokenRegistration`，确认 `CancellationToken`/`CancellationTokenSource` 仍各自使用原生 `AbortSignal`/`AbortController`。
2. 迁移 `JIndex`/`JRange`，将 property names 和 offset calculation 集中为 helper，避免 compiler 读取 object 内部字段。
3. 迁移 `JGregorianCalendar`，以 side-table 保留可变 state；Calendar 与 GregorianCalendar 必须继续走同一 core。

### C3：原生集合

1. 将 Stack 映射为原生 Array；将 Queue 映射为 Array 加私有 head state，不能把 `head` 泄漏为作者可依赖字段。
2. 逐一替换构造器、enumeration、copy 和 mutation adapter；保留既有求值顺序、容量错误和 comparer 行为。
3. 只有所有 Queue/Stack consumer 已不再 import `JQueue`/`JStack` 后，删除 class 与 carrier metadata。

### C4：时间值

1. 先抽取每个类型唯一 creator/getter layout helper，再替换各日期模块的直接 `J*` 字段访问；禁止在 Calendar、URI、格式化和 parsing 模块各自读取 raw key。
2. 保持日期复制、tick 精度、`DateTimeKind`、`DateTimeOffset` offset、`DateOnly` 无时区语义、`TimeSpan` Int64 边界以及 `Symbol.ToPrimitive` hook。
3. 逐类型移除 generated carrier metadata 和 `instanceof` 测试，最后删除 `RuntimeModule` class 定义。

### C5：收尾与发布

1. `rg` 检查不存在已迁移类型的 `RuntimeModule.J*` adapter signature、import、`instanceof` 或 `constructor.name` test branch。
2. 重新生成 whitelist，运行 CLR/runtime、compiler、Razor SG/browser、emit/package 对应门禁。
3. 更新 `blazor-clr-support-plan.md` 中的最终实现归属、`current-status.md` 和 CHANGELOG。该变更可能收紧 previously emitted unknown-object pattern 行为，不能作为 PATCH 发布；按当时版本通道记录迁移说明。

## 7. 逐阶段验收

| 维度 | 每一迁移组必须证明的事项 |
| --- | --- |
| C# / whitelist | 原有 CLR member 仍可绑定；新 adapter 不引入宽松 public API；生成 whitelist 的 key、Op 和 path 可预测。 |
| compiler emission | 正常成员调用不出现 `J*` class import；unknown-object nominal test 不会退化为 shape/tag；import/临时名/source origin 稳定。 |
| runtime 行为 | 既有 `Jazor.CLR.Test` scenario 对结果、错误、求值顺序、回调次数、async 时机和 dispose 幂等性全部通过。 |
| Blazor browser | 受影响的导航、事件、ElementReference、表单或认证切片在真实 browser 中验证；SSR/hydration 仅在该切片原本承诺时验证。 |
| package | 未使用切片不引入无关模块；已使用切片的 Release bundle 具有完整 closure；不新增手写 `.mjs`。 |
| 可维护性 | 状态 layout 有唯一 owner/helper；没有散落 raw key、没有 `__jazorType`、没有 duplicate state machine。 |

## 8. 非目标

- 不把任意浏览器 object 自动视为任意 CLR type。
- 不恢复完整 CLR boxing、`System.Type`、reflection、record value equality 或 runtime inheritance。
- 不以 object/动态访问替换作者可见的强类型 WebIDL 或 Blazor API。
- 不为了保留 `is/as` 而引入 tag、Proxy、全局 registry 或 per-type wrapper。
- 不在 RazorVue runtime `.mjs` 实现 CLR state、日期计算、导航决策或 listener ownership。

## 9. 完成定义

本计划完成不以“`J*` 类数量减少”作为唯一标准，而以以下结果为准：默认 CLR runtime 表示已经是原生对象或 `object` 加私有结构状态；保留的每一个 nominal class 都有书面理由和端到端 type-identity 测试；所有不具备 identity 的类型在 `is/as/typeof` 使用点稳定失败或经 Roslyn 折叠；现有 Blazor 切片的用户可观察行为、source map 和 Release artifact contract 不回归。
