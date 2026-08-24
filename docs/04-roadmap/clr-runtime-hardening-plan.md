# CLR Runtime 健壮性与性能强化计划

> 状态：规划中。本文保留现有 nominal carrier 路线，优先修复已经能复现的身份/构造缺口，再以基准数据决定是否实施性能候选。
>
> 审阅基线：已发布的 `v0.19.0` 与 2026-08-24 的开发工作树。源码位置以类型、成员和测试名为准，不把提交号或生成文件行号当成长期契约。
>
> 关联：[RazorVue Blazor CLR 类型支持计划](./blazor-clr-support-plan.md) 负责新增 Blazor API 切片；本文只强化既有 CLR runtime 表示、生成器护栏和消费边界，不扩大作者可调用的 API 面。`ECMAScript.Blazor` 只提供随 `Jazor.Vue` 交付的 Blazor mapping declaration；若映射需要 carrier/helper，实际 runtime 仍由 `Jazor.CLR` 提供。`Jazor` 核心包不因此引入新的 Blazor mapping API。

## 1. 已裁决的对象模型

运行时表示按可观察语义选择，不要求所有 CLR 类型落到同一种 JavaScript 形状：

| 类别 | 当前路线 | 运行时判别边界 |
| --- | --- | --- |
| 推断得到的 nominal carrier | 保留 `RuntimeModule.J*` class。当前包括日期/时间、`Index`/`Range`、`Queue`/`Stack`、Gregorian calendar 和 cancellation registration 等 carrier | 由 `[Jazor(...)]` 适配器签名推断 carrier，并发射 `instanceof J*`；精确到 carrier，不保证精确到唯一 CLR 类型 |
| 原生 browser carrier | 使用 `URL`、`AbortSignal`、`AbortController`、DOM event 等真实宿主对象 | 只承诺该原生 carrier 能证明的身份和成员面，不补 CLR 风格 tag |
| 擦除的集合 carrier | 使用原生 `Array`、`Map`、`Set` | 只区分运行时容器种类；泛型实参以及共享同一 carrier 的 CLR 集合类型不具备独立身份 |
| 无 nominal identity 的 host/state | 使用模块私有状态、closure、plain object 或 `WeakMap`，具体形状由所属模块决定 | 默认不提供 `is`/`as`/`typeof` 身份；只有实际使用场景证明需要时才引入 carrier |

保留现有 `J*` class 的理由是可观察行为，而不是“每个 CLR 类型都应有一个 class”。当前 `Equals(object)`、`CompareTo(object)`、`is`、`as` 和 `Enumerable.OfType<T>` 已使用推断得到的 carrier identity；把这些 carrier 改成无身份 object literal 会改变已有行为。另一方面，`Calendar` 与 `GregorianCalendar` 共用 `JGregorianCalendar`，泛型 carrier 也擦除类型实参，因此不能把 `instanceof J*` 描述成完整 CLR runtime identity。

本计划不再推进一次性的“大规模 object-layout 迁移”。任何后续布局改动必须先指出一个真实失败场景或可重复性能瓶颈，并证明不能由现有 carrier、原生宿主对象或模块私有状态解决。

## 2. 审阅结论与健壮性工作项

### 2.1 已确认的事实

| ID | 状态 | 结论与证据 | 后续动作 |
| --- | --- | --- | --- |
| R1 | **需修复** | `blazor-components.mjs` 的 `parseIsoDate` 为 `DateOnly`、`DateTime`、`DateTimeOffset` 手工拼装 plain object；`RazorSgStandardBlazorComponentRuntimeTests.TypedInputs_EmitValueDescriptorsForNumberDateAndEnum` 也把该结构写成了期望值。这些对象不是对应的 `J*` 实例，因而丢失 `is`/`as`、`Symbol.toPrimitive` 和依赖 carrier 的 object overload 语义 | 由 CLR-owned 强类型构造/helper 产生值；RazorVue 只传递输入描述和调用模块入口，不再维护第二份字段布局 |
| R2 | **已有行为，缺 compiler 护栏** | `DateTimeModule` 和 `DateTimeOffsetModule` 已为 `==`、`!=`、`<`、`<=`、`>`、`>=` 声明 `Op.Import`；生成白名单使用 `static System.DateTime.operator ...` 形式的 canonical key。`ClrRuntimeDateTimeScenarios` 与 `ClrRuntimeDateTimeOffsetScenarios` 已覆盖 helper 行为 | 增加 authored C# compiler emission 回归，证明比较运算符绑定到稳定 import，而不是退化成裸 JavaScript 运算符。此项不新增 API，也不改变现有 runtime helper |
| R3 | **需护栏** | `TryGetInternalRuntimeValueCarrier` 检查 `TypeKind.Class`、非 static、源码声明、`Jazor.CLR` 命名空间和 module path；但它没有像 `ClrRuntimeSelection` 一样排除 record。record 会走 structural lowering，不应被推断成可供 `instanceof` 的 runtime class | carrier inference 明确排除 `IsRecord`，并在 generator predicate 的可测试落点增加回归 |
| R4 | **需护栏** | 需要字符串/数值强制的六类 carrier（`JDateTime`、`JDateTimeOffset`、`JDateOnly`、`JTimeOnly`、`JTimeSpan`、`JGregorianCalendar`）在各构造路径安装 own `Symbol.toPrimitive`。安装逻辑分散，漏掉某个重载会只在该构造路径复现 | 对这六类 carrier 逐个覆盖所有构造路径，断言 own hook、default/string/number hint；不要求 `JIndex`、`JRange`、`JQueue` 等本来不需要该 hook 的 carrier 安装它 |
| R5 | **已知边界** | `JDateTime.ValueOf()` 返回毫秒级 `Number`，而 ticks、比较和文本可保留 100ns 余量。这是 JavaScript 数值投影，不是 C# `DateTime` 比较路径；比较运算符已经直接调用 CLR helper | 在 runtime carrier 的源码说明或 CLR 架构文档中记录“number hint 为毫秒精度”。没有真实消费场景前不改变投影，也不把亚毫秒 ticks 强塞进 `Number` |
| R6 | **已覆盖，按触及面维护** | carrier 字段名与 `Op.Alias` 的耦合已经由模块元数据测试覆盖，例如 `CancellationModuleWhitelistTests.CancellationTokenRegistrationMembers_ExposeTheUnregisterCredential` 固定 `Token.get -> signal` | 修改相关 `[Description("@#...")]` 或 Alias 时同步扩展原测试；不重复建立第二套全局字段注册表 |
| R7 | **接受并文档化** | carrier 判别精确到 carrier：`Calendar`/`GregorianCalendar` 共用一个 carrier，原生 `Array`/`Map`/`Set` 也承载多个 CLR 集合类型，泛型实参被擦除 | 保持现有行为，在支持文档中明确限制；不得据此新增 type tag、结构 shape test 或一类型一 carrier 的平行体系 |
| R8 | **测试基础设施债务** | `ClrRuntimeTestHost` 对 `JQueue`、`JStack`、`JIndex`、`JRange` 使用 `constructor.name` 分支编码测试值。类名变化可能让专用编码静默退化 | 仅在触及该宿主时收敛为一个显式、会对未知值失败的测试协议；它不是产品 runtime identity，也不能反向进入 compiler/carrier inference |

### 2.2 R1 的修复边界

R1 是当前唯一已经由生产路径证明的“双写布局”缺口，优先级高于性能工作。修复必须满足：

1. 先用标准 Blazor reference fixture 固化 `InputDate<TValue>` 对 `DateOnly`、`DateTime`、`DateTimeOffset`、nullable、无效输入和时区/offset 的行为；不能只把现有 plain object 换成 `new J*` 后沿用未经验证的值语义。
2. 日期解析、范围校验和 carrier 创建由 C# 编写的 `Jazor.CLR` module helper 负责；RazorVue `.mjs` 可以保留 DOM/Vue listener framing 和一次薄调用，但不得继续知道 `year`、`dayNumber`、`utcDateTime` 等内部字段集合。
3. 产出的值必须通过正常 CLR 成员调用、`is`/`as`、object overload 和字符串转换回归；不得只断言字段 deep-equal。
4. 这是 [Blazor CLR 支持计划](./blazor-clr-support-plan.md) 表单切片进入 Support 的前置条件。修复缺口本身不等于完整 `EditContext`/validation 已支持。
5. 本计划不顺带建立通用 JSON、IndexedDB、`postMessage` 或 `structuredClone` rehydration 框架；以后若新增这类入口，必须为该入口单独定义强类型重建协议。

### 2.3 R2 的更正

此前把白名单 key 误搜成 `System.DateTime.op_*`，从而得出“比较运算符未映射并显式失败”的结论。该结论是错误的：仓库的 canonical key 使用 `operator ==`、`operator <` 等 display 形式，且映射为 `Op.Import`；`DateOnly`、`TimeOnly`、`TimeSpan` 也沿用同一 contract。R2 的回归重点是曾被误判的 `DateTime`/`DateTimeOffset` authored path。正确的端到端路径是：

```text
authored C# d1 < d2
  -> Roslyn-bound operator symbol
  -> WhiteList canonical operator key
  -> DateTimeModule/DateTimeOffsetModule Import helper
  -> ticks-based comparison
```

因此本计划只补 compiler emission 与稳定 import 的缺失证据。删除或放宽 custom-operator gate、改为 `Op.Allowed`、或直接发射 JavaScript `==`/`<` 都应让回归失败。

## 3. 性能工作必须先测量

源码调用次数和代码行数不是性能证据。任何性能改动前，先用单文件 C# 脚本编排同一 SDK、同一 Deno/runtime、固定输入的 warm-up 与多轮测量，记录中位数及产物大小；可复用脚本放在 `scripts/csharp/`，不得新增 `.ps1`。

首轮基准至少分开测量：

- 构造大量 `JDateTime` / `JDateTimeOffset` 的成本；
- 重复读取 ticks、比较、相等和 hash 的热路径；
- 重复 `ToString()` 的热路径，以及大多数值没有亚毫秒余量的普通路径；
- runtime module 体积和每实例新增字段带来的成本。

在看到基线前，下列内容都只是候选，不是已批准实现：

| ID | 候选 | 可能收益 | 必须同时证明的代价 |
| --- | --- | --- | --- |
| P1 | 在 `JDateTime` 构造时缓存 ticks，让 `GetTicks(JDateTime)` 直读 | 比较、相等、hash、ticks 属性等重复调用不再反复读取 `Date` 分量并构造 `BigInt` | 每实例增加一个 `BigInt` 字段并提高构造成本；只有代表性 workload 的净收益达到预先记录的验收阈值才实施 |
| P2 | `JDateTime.ToString()` 对 `SubMillisecondTicks == 0` 使用等价快路径 | 普通毫秒精度值避免不必要的 BigInt 乘加 | 必须逐字符保持 7 位 fraction，且在真实热路径有可重复收益 |
| P3 | 缓存 `JDateTimeOffset` 的 offset 文本或拆分不变量 | 重复 `ToString()` 可减少 offset 分解 | 会让从不格式化的实例也承担字符串分配；没有“同实例重复格式化”证据时不实施 |
| P4 | 把 `Symbol.toPrimitive` 安装到 prototype | 可避免每实例 `Object.defineProperty` 和绑定函数 | 需要新的 compiler/runtime-class emission 能力并改变对象特性；不属于当前计划 |
| P5 | 取消 `Date` 防御性拷贝 | 减少构造时 `Date` 分配 | 会破坏 carrier 的逻辑不可变性；明确不实施 |

性能项只有在基准阈值于改动前写明、行为门禁全绿且没有把成本转移到更高频路径时才能合并。“生成代码更短”或“少调用了几个 helper”不构成验收。

## 4. 改动不得破坏的不变量

1. CLR 类型到内部 carrier 的关系只从 `[Jazor(...)]` 适配器签名与 Roslyn symbol 对齐推断；不增加显式 carrier attribute、名称规则、隐藏 tag、平行 registry 或结构 shape test。
2. `WhiteList.cs.Generate.cs` 是生成产物，persisted key 保持既有 canonical contract；修改 CLR whitelist 源后必须重新生成并提交结果。
3. 已推断的 nominal carrier 保留 class identity。多个 CLR 类型可以共用一个 carrier，泛型实参可以擦除；carrier 不得被当成 `typeof(T)` / `System.Type` token。
4. `DateTime`/`DateTimeOffset` 比较运算符继续通过 `Op.Import` helper 保留 ticks 语义，不得退化为 JavaScript 对象引用比较或数值强制。
5. 只有声明并需要 `ToPrimitive` 的 carrier 才要求 own hook；这些 carrier 的每个构造路径都必须安装它，default hint 继续走字符串分支。
6. 日期/时间值 carrier 在受支持 CLR 路径中保持逻辑不可变，并保留 `Date` 防御性拷贝。`JGregorianCalendar` 和 `JCancellationTokenRegistration` 等有意可变状态 carrier 不受“全部字段只读”的错误泛化约束。
7. 任何生产入口从外部表示创建 nominal carrier 时，必须调用 CLR-owned 构造/helper；不得在 RazorVue 或其他宿主层复制 carrier 字段布局。
8. 原生集合和共享 carrier 的 `is`/`as` 只具有 carrier 精度；文档必须诚实说明，不能用 type tag 假装获得更强身份。
9. 共用 CLR runtime 行为以及 Blazor 专属 runtime module/helper 都以 C# 写入 `Jazor.CLR` 并由现有管道编译；`ECMAScript.Blazor` 只持有 framework-to-carrier mapping declaration。手写 `.mjs` 只保留宿主生命周期、渲染 framing 和到 `Jazor.CLR` 模块入口的薄转发。
10. 本计划不新增 CLR API 映射；若某项必须扩大作者可用成员面，应移入独立 `MINOR` 能力切片。

## 5. 实施阶段

| 阶段 | 内容 | 出口条件 |
| --- | --- | --- |
| H0 | 更正 operator 基线；为 `DateTime`/`DateTimeOffset` authored C# 比较补 compiler import 回归；把 carrier 精度和 number-hint 精度写入稳定文档 | 生成代码调用既有 import helper；CLR runtime comparison 场景继续通过；没有新增 API |
| H1 | 修复 `blazor-components.mjs` 日期 plain-object 构造；先补 reference oracle，再改为 CLR-owned 构造/helper | `InputDate` 产生真实 carrier；成员访问、比较/identity、字符串转换、invalid/nullable/offset 行为通过 Razor SG runtime 与真实浏览器验证 |
| H2 | carrier inference 排除 record；为需要 `ToPrimitive` 的 carrier 覆盖全部构造路径 | 生成器输出级回归能阻止 record 获得 `RuntimeValueCarrier`；故意漏掉任一 hook 时对应测试失败。优先扩展现有 `WhiteListLookupTests` 的生成结果断言；若 predicate 本身无法从现有测试入口覆盖，再提取共享纯函数或使用最小输入/输出 probe，不把不存在的 generator test project 当成前提 |
| H3 | 建立 CLR runtime 性能基线，给 P1-P3 分别做 go/no-go 裁决 | 基线、阈值和结果可复现；没有达到阈值的候选记录为“不实施”，不提交无收益复杂度 |
| H4 | 仅在触及 `ClrRuntimeTestHost` 时收敛按 class-name 编码的测试债务 | 专用 carrier 编码不会静默落入 generic object 分支；该协议仍只存在于测试层 |

依赖关系只有一条硬约束：H1 是 Blazor 表单日期切片进入 Support 的前置条件。H0 与 H2 可独立完成；H3 必须先完成基准再修改性能代码；H4 不阻塞产品能力。

## 6. 验收与验证入口

健壮性改动按触及面运行：

```bash
dotnet run --project src/Jazor.Compiler.Generator/Jazor.Compiler.Generator.csproj
dotnet run --file scripts/csharp/test-dotnet.cs -- --project clr
dotnet run --file scripts/csharp/test-dotnet.cs -- --project compiler
dotnet run --file scripts/csharp/test-dotnet.cs -- --project razor-sg
```

- H1 还必须进入真实浏览器与 isolated Release package consumer；Deno 中伪造的 `window`/`history`/DOM 只能算 module runtime 证据，不能替代 BrowserSmoke 或 PackageConsumer。
- 修改 compiler 或 RazorVue 时分别运行 [当前状态](./current-status.md) 指定的 `verify-compiler-coverage.cs` / `verify-razorvue-coverage.cs`；只有触及 WebIDL/Vue 公开 binding 时才追加 `verify-vue-binding-coverage.cs`。
- 修改 CLR 映射时更新 module source，并通过 `Jazor.CLR.Generator` 的既有流程重新生成/核对 `src/Jazor.CLR/doc/`；不手改生成骨架或生成文档来掩盖实现漂移。
- 性能项必须附同一环境、同一输入的基线/候选结果；runtime module 自身产物允许按设计变化，消费者可观察结果、import 稳定性和 source-map 锚点不得变化。

## 7. 非目标

- 不把现有 nominal carrier 批量改成 DTO、object literal、原生 `Map` 或带约定 type key 的容器。
- 不为 `structuredClone`、JSON、IndexedDB、worker/realm 传输预建通用 rehydration 框架。
- 不因理论上的外部 JavaScript 篡改而改变 `Symbol.toPrimitive` 的 `Configurable`；没有受支持场景和回归证据时不做预防性 hardening。
- 不取消 `Date` 防御性拷贝，不抽返回 `object` 的 carrier 公共基类，不把 carrier 暴露为 `System.Type`。
- 不实现 prototype symbol-member emission、完整 CLR runtime identity 或一类型一 carrier。
- 不新增手写 runtime `.mjs` 领域逻辑，不新增 `.ps1` 自动化。
- 不扩大作者可调用的 CLR/Blazor API 面；新增能力归 [Blazor CLR 支持计划](./blazor-clr-support-plan.md) 或其他独立路线图。
- 不把 Blazor framework mapping declaration 复制回 `Jazor.CLR`；其声明归属和随 `Jazor.Vue` 的交付由 [Blazor CLR 支持计划](./blazor-clr-support-plan.md) §2.1 规定，实际 runtime module/helper 仍由 `Jazor.CLR` 唯一承载。
