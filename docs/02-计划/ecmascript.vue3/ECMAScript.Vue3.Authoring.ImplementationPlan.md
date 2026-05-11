# ECMAScript.Vue3 Authoring 过渡计划

> Status: 活跃计划
> Updated: 2026-05-05
> Positioning: 记录从 Vue3 Phase 1 到 Phase 2/3 的过渡状态。Phase 1 完成项保留为历史完成记录；当前活跃关注点是 Razor -> `H(...)` 规范层与后续 Jolt 工程化协同。

## 0. 三阶段总路线（H -> Razor->H -> Jolt）

`ECMAScript.Vue3` 按三阶段推进，防止把“库映射问题”和“工程化 authoring 问题”混在一个阶段里：

1. **Phase 1: H 函数映射层（已完成闭环）**
   - 建立 `H(...)` / `VueObject` / slot sugar / read-side bag 的规范面。
   - 以 C# 类型系统 + 通用 lowering 为主，不扩张 Vue 专项 compiler 分支。

2. **Phase 2: Razor -> H 规范层（当前主线）**
   - 目标是把 Razor authoring 稳定映射到 Phase 1 的 `H(...)` 规范层。
   - 重点是 canonical shape、诊断边界、语义一致性，而不是再扩张 `H(...)` overload。

3. **Phase 3: Jolt 工程化协同（规划）**
   - 在 Jolt 中承接 authoring、构建、调试的工程化闭环。
   - 仍保持 Vue3 为外部库：不把 Vue 命名语义反向注入 compiler 核心。

## 1. 计划目标

这条计划不再以“把 Vue 示例逐项补齐”为目标，而以“把 Vue authoring 收敛到 C# 原生语义 + 少量稳定 host contract”为目标：

1. 让公共 surface 尽量通过 `record`、`overload`、`generic`、`delegate`、`attribute` 表达。
2. 让 compiler 只保留稳定特性和通用 lowering，不再扩张 Vue 专用特路。
3. 让 `VueObject`、`VueSetupContext`、`H(...)`、registry 这些入口收敛到少量 canonical 形状。
4. 让所有 `null` / missing / optional / union 语义在公共 contract 上说真话。
5. 让每个 Vue3 authoring surface 都能对照映射细节文档解释其最终 JS 形态。
6. 让新增 Vue API 先对照覆盖矩阵确定优先级和归属，不把 SFC/SSR/custom renderer 等独立工作流塞进 `Vue3.cs`。

## 2. 边界约束

执行时必须保持以下硬规则：

- `record structural lowering`、`[Spread]`、静态 `null` 省略必须继续作为通用能力存在。
- compiler 只识别稳定 host contract，不围绕 `ECMAScript.Vue3` 名字本身增加散点硬编码。
- `Dataset` 不在 compiler 里做前缀推断。
- 不把 `OptionalAttribute` 或类似新语法当作当前问题的主解法。
- 能由 C# 类型系统承接的差异优先下沉到 helper type，而不是继续堆 compiler 分支。
- 新增或修改 Vue3 surface 前，必须先能在映射细节文档中落到明确的 C# 输入、JS 输出和诊断边界。
- 新增 Vue 官方 API 前，必须先在覆盖矩阵中标明状态、目标归属和优先级。

## 3. 当前基线

这里的“当前基线”表示进入 Phase 2 前已经稳定存在、可以直接复用的 contract，不再视为待设计项。

当前已经具备的能力：

- `VueObject` / `VueObject<TProps>` 已建立在通用 record structural lowering 上。
- `[Spread]` 已经是通用 record flattening 路线。
- 静态 `null` 省略已经存在于 structural lowering。
- `VueDirective`、`VuePlugin`、`VueSlots`、registry 已经具备基础 authoring surface。
- `PropOptions` / `EmitOptions` 已把 Vue runtime `props` / `emits` 的 object-form 显式声明纳入 record authoring surface，typed generic 不再作为自动 runtime declaration 的理由。
- `VueComponentOptions*` 不再携带历史 `[Props]` / `[Emits]` 推导特性，避免依赖同 key 成员排序压制推导这种脆弱行为。
- `H(...)` default-slot sugar 已从 `SemanticWalker` 的 Vue 命名分块迁移到通用 `ChildrenToSlotIntrinsic`，识别条件是 imported `h` + 同宿主 component / props / slot 合同。
- `H(...)` 已按 element/component/props/slots/direct-child canonical 分类收口，并由 default-slot、typed-slot、host-like contract 与 single-evaluation 回归守护。

后续工作的重点不再是“功能数量”，而是“边界质量”。

## 4. Phase 1 完成记录（已闭环，保留作过渡参考）

### Phase A: Compiler Boundary Reduction

目标：先把 Vue 专用硬编码压回可解释的稳定 contract。

#### A1. 迁移 Vue 命名 lowering 分块

**Status**

- 已完成第一步：default-slot sugar 不再要求调用点方法来自 `ECMAScript.Vue3`，而是要求方法最终导入名为 `h`，并且同一 host type 暴露 `IVNode`、`IVueComponent*` / `IVueSlotComponent<TSlots>`、`VueProps` 等稳定合同。
- typed default slot 校验仍保留，但 slot callback 不再要求固定 `VueSlotCallback` 类型名；成员类型只要是返回同宿主 `IVNode` 的 delegate 即可，参数数量用于区分 parameterless / scoped slot。
- 已完成安全输出优化：literal child 直接生成 default slot object；非 literal child 仍使用 IIFE 捕获值，保持求值顺序、单次求值和变量快照语义。
- 已移除 `SemanticWalker.cs.Vue.cs`，对应逻辑现在位于 `ChildrenToSlotIntrinsic`，`SemanticWalker` 仅负责注入导入绑定、诊断和通用类型 helper。

**目标**

- 把对 `ECMAScript.Vue3` 名字本身的直接特判降到最少。
- 让 `H(...)`、slot、directive 相关 lowering 更依赖稳定 contract 而不是示例名字。
- 按映射细节文档把 default slot sugar 明确为 children-to-slot contract，而不是 Vue API 散点特例。

**接受标准**

- 新增 Vue authoring 场景时，不需要先在 compiler 里补一条新的 Vue-only 分支。
- 能通过已有通用 lowering 和少量 contract 识别表达的，不再加特判。
- default slot sugar 在保持单次求值和顺序语义的前提下，简单安全表达式可生成直接值对象。

#### A2. 收紧 object-literal host 路线

**目标**

- 统一 `VueDictionary`、`VueObject`、registry、plugin options 的 object-literal lowering；其中 string literal key 走普通 property，显式 `Symbol` key contract 走 computed property。
- 维持 `Add(string, ...)`、indexer、initializer 这三条路径的一致性。

**接受标准**

- 不支持的 string-key 形态不会静默变成错误 JS。
- 运行时 object-literal 语义与 authoring surface 不再分叉。

#### A3. 保持通用 record lowering 纯净

**目标**

- `[Spread]`、静态 `null` 省略继续作为通用 record 规则。
- 不为 `VueObject` 新增额外特殊规则。

**接受标准**

- 任意 structural-lowered record 都能共享同一套 spread/null 规则。
- `VueObject` 不再要求 compiler 独立开洞。

### Phase B: Public Surface Convergence

目标：让 public surface 更像 C#，而不是更像 Vue 示例集。

#### B1. 收敛 `VueObject.Class` 到命名 union contract

**Status**

- 已落地 `VueClassValue` 命名 union，并迁移到 native `union` 优先路径。
- active public surface 不再暴露旧泛型 union wrapper 或旧 marker。
- 旧兼容 marker 已移除；需要精确 tagged projection 的场景使用 `[System.Runtime.CompilerServices.Union]` + `IUnion` fallback。

**目标**

- 把对象成员值上的 union 收敛到具名 host contract，而不是继续暴露泛型 union wrapper。
- 在未来 native union 更成熟时，保持迁移路径是机械的，而不是让现在的 surface 再长出一层更差的包装。

**接受标准**

- 常见 class authoring 仍能自然表达。
- `VueClassValue` 保持为清晰、稳定、可迁移的 bridge shape，而不是一层只能靠文档解释的临时 hack。
- 不再围绕旧的泛型 union wrapper 再发明更差的替代包装。

#### B2. 补齐 `VueSetupContext` 读侧 bag

**Status**

- 已落地 `VueAttributeBag` indexer / `Class` / `Style` / `Id` / `Title`。
- 已落地 `VueSlotBag` indexer / `Default`。

**目标**

- `Attrs` / `Slots` 变成真实 bag contract。
- 与映射细节文档中的 read-side bag 规则对齐。

**接受标准**

- `VueAttributeBag`、`VueSlotBag` 至少具备最小但真实的读取/调用面。
- typed / untyped context 的职责边界清楚。

#### B3. 收敛 `H(...)` overload 家族

**目标**

- 保留少量 canonical overload；
- 让等价写法收敛，而不是继续扩张。
- 按 element/component/props/slots/direct-child sugar 分类治理 overload。

**接受标准**

- `H(...)` 不再继续被当成 Vue docs 的镜像容器。
- overload 数量可治理、可维护。
- 每个 overload 都能在映射细节文档中归入一个 canonical 分类。

#### B4. 让 directive / emit / slot 的 union 说真话

**目标**

- `binding.Dir`、emit payload、slot return shape 都按真实合同表达。

**接受标准**

- missing / union / possibly-missing 不靠文档假装。
- 必要时用 helper type 表达，不靠 compiler 隐式修补。

### Phase C: Ergonomic Tuning

目标：在边界收口后，再做少量高频 ergonomic 提升。

#### C1. `VueValue` 与常用 helper 深化

重点：

- 只补真实常用值形态；
- 不引入错误隐式转换；
- 让 `VueObject` / `VueDictionary` 更好用。

已落地：

- `VueKey` 作为 VNode `key` 的语义 bridge，覆盖 string / number-like / `Symbol`，避免旧的泛型 union 形态在 C# 中无法自然接收 `Key = 42` 的双重隐式转换问题。
- `VueObject.Key` / `VueObject.Ref` 补齐 render props 高频 convenience；`Ref` 先覆盖 named template ref key，并与 `UseTemplateRef<TElement>(key)` 配套。
- `VueEventHandlers` / `VueEventHandlers<TEvent>` 作为 `VueObject.Events` 的 `[Spread]` listener bag，解决 method group 不能直接赋给 `VueValue` indexer 的问题，同时保持事件 key 为最终 `onXxx`。
- `VueObject.Is` 覆盖 string customized built-in `is` special attribute；不使用泛型 union wrapper 承接 `string | component`，因为 C# 不允许以 interface 为源/目标的自然用户定义转换，动态组件应直接使用 component-valued `H(...)`。

#### C2. `Dataset` / `Style` / class helper 收口

重点：

- 不引入 Vue-only runtime 污染；
- 优先复用 record、indexer、attribute、delegate 等现有 C# 能力。

#### C3. 文档与 README 对齐

重点：

- `src/ECMAScript.Vue3/README.md`
- `docs/01-目标/ecmascript.vue3/*`
- 本计划文档自身

### Phase D: Vue API Coverage Completion

目标：按覆盖矩阵补齐不需要 compiler Vue 特路的官方 API 面。

#### D1. P0 低风险 host binding

重点：

- 优先补 `vue3-api-coverage-matrix.md` 中的 P0 项。
- 仅使用基础 binding、delegate、record、overload。
- 不新增 Vue-specific compiler 分支。

#### D2. P1 小型 helper surface

重点：

- `VueAppConfig`（核心路径已落地）
- `app.mixin`（低层兼容 binding 已落地；官方不推荐作为应用 authoring path）
- `VueWatchOptions` / `VueWatchEffectOptions`（核心路径、debugger event options、reactive object source、同类 multi-source watch 已落地）
- writable computed options（已落地）
- `VueEffectScope`（已落地）
- composition provide/inject（string key、typed injection key、Options object-form symbol-key source / provide authoring 已落地）
- composition helpers（`useAttrs` / `useSlots` 基础 bag 与 typed projection、`useTemplateRef`、`useId` 已落地；`useModel` 已覆盖 typed ref + get/set options + modifiers projection，并补入 `VueModelName<TProps,TValue>` + `ModelName/ModelPropName/ModelUpdateEventName` named-model contract，以及 `VueSetupContext.Emit(model, value)` typed update emit helper；higher-level v-model 协议后续设计）
- `defineAsyncComponent`（loader/options 核心路径已落地）
- `customRef`（factory + handlers 已落地）
- `toRef` / `toRefs`（normalization、source key、typed refs projection 已落地）
- `data` / `computed` / `methods` / `watch` / `props` / `emits` / `inheritAttrs` / `expose` / lifecycle callbacks / Options provide-inject 基础面 / mixins-extends 兼容面（component definition base option 已落地；explicit `PropNames` / `EmitNames` array-form 已落地；`PropOptions` / `EmitOptions` object-form validators/defaults 已落地；data 和 lifecycle 先覆盖无 `this` callback surface，computed 先覆盖 no-`this` getter/writable registry 与自定义 `VueProps` record，methods 先覆盖 no-`this` delegate registry 与自定义 `VueProps` record，watch 先覆盖 callback / cleanup callback / method-name / options object 基础形态，provide/inject 已覆盖 object/array 声明式形态、provide function-form、inject object-form source/default/factory helper，以及 `VueDictionary` symbol-key object authoring，mixins/extends 只作为低层兼容 binding）
- built-in components（`Transition` / `TransitionGroup` / `KeepAlive` / `Teleport` / `Suspense` render-function binding 已落地）
- `withDirectives` / `withModifiers`（核心 render helper 已落地；后续只做 authoring convenience 优化）
- custom elements 核心 binding（`DefineCustomElement`、`VueCustomElementOptions`、`UseHost`、`UseShadowRoot` 已落地；完整 props/events/light DOM authoring 策略后续设计）

#### D3. P2/P3 分流

重点：

- Options API full surface、custom elements 的完整 authoring 策略先设计再实现。
- SFC、template、SSR renderer、custom renderer 保持 separate workstream。

## 5. 当前主线执行顺序（Phase 2 / Phase 3）

1. Razor -> `H(...)` canonical lowering
2. 基于 canonical `H(...)` 的 RazorVue 库模式 design-time SFC artifact 方案落地
3. Razor 产物与手写 `H(...)` diagnostics 对齐
4. 外部库 layout/proxy/doc guard 模板化
5. higher-level `v-model` convenience 设计
6. Options API 长尾与 custom elements authoring 策略
7. Phase 3 的 Jolt 工程化协同

理由：

- Phase 1 已完成，后续不应再把 canonical `H(...)` / object-literal / read-side bag 当作待设计项反复回填。
- 下一阶段的核心是承接已完成 contract，而不是继续扩张 compiler Vue 特路。
- 先把 Razor/Jolt 等上层工作流压到 Phase 1 contract 上，再讨论更高层 ergonomics。
- 其中 RazorVue `h(...)` 发射已完成最小 arity 对齐：`h(component)` / `h(component, props)` / `h(component, slots)` / `h(element, child)` 等 canonical 形态不再携带多余 `null` 占位。
- 若库模式主工件改为 `.vue` SFC，则 SFC 生成必须视为 Phase 2 的直接下游，而不是独立于 canonical `H(...)` 的第二套模板语义；详见 [RazorVue 库模式 Design-Time SFC 方案](./RazorVue.LibraryMode.DesignTimeSfcPlan.md)。

## 5.1 Phase 2 / Phase 3 前置门槛

进入 Phase 2（Razor -> H）前至少满足：

- `H(...)` canonical 分类文档稳定（element / component / props / slots / direct-child sugar）。
- object-literal / dictionary 路线（indexer / `Add(string, ...)` / initializer）具备统一语义和诊断边界。
- read-side bag（`VueAttributeBag` / `VueSlotBag`）可覆盖 Razor 映射的基础读取场景。

进入 Phase 3（Jolt）前至少满足：

- Razor -> H 产物在 compiler 输出语义上可预测、可诊断、可回归测试。
- Jolt 只承接工程化闭环，不接管 Vue3 API 语义定义。
- Vue3 相关语义变化能回流到 `ECMAScript.Vue3` 文档与测试，而不是散落到 Jolt 私有逻辑。

## 6. 验证要求

每个切片至少满足：

- `src/Jazor.CompilerTest/EcmaScriptVueProxyTests.cs`
- `src/Jazor.CompilerTest/AstConverterTests.cs`
- 聚焦回归通过：

```powershell
dotnet test src/Jazor.CompilerTest/Jazor.CompilerTest.csproj --filter "FullyQualifiedName~EcmaScriptVueProxyTests|FullyQualifiedName~AstConverterTests" -v minimal
```

## 7. 风险与控制

| 风险 | 影响 | 控制方式 |
|------|------|---------|
| compiler 继续按 Vue 示例堆硬编码 | 高 | 先做 Phase A，约束新增入口 |
| public surface 继续把 union 暴露给一般作者 | 高 | 先做 Phase B1/B2/B4 |
| overload 继续膨胀 | 中 | 先收 canonical shape，再补 helper |
| 文档领先实现 | 中 | 每个切片完成后同步 README |

## 8. Phase 1 完成定义（历史完成记录，已满足）

以下条件已经满足，因此 Phase 1 现已转入完成状态：

- `SemanticWalker` 不再以 Vue 示例名作为主要设计中心；
- `VueObject.Class` 已收敛为清晰的命名 union bridge shape，并能在 native union 到来时平滑迁移；
- `VueSetupContext` 的读侧 bag 可实际使用；
- `H(...)` overload 家族已收敛到 canonical 分类，并由回归测试守护；
- `record` / `[Spread]` / static `null` 省略继续保持通用。

## 9. 参考

- [src/ECMAScript.Vue3/README.md](../../../src/ECMAScript.Vue3/README.md)
- [RazorVue.LibraryMode.DesignTimeSfcPlan.md](./RazorVue.LibraryMode.DesignTimeSfcPlan.md)
- [src/Jazor.Compiler/ImplementationPrinciples.md](../../../src/Jazor.Compiler/ImplementationPrinciples.md)
- [docs/01-目标/ecmascript.vue3/vue3-balanced-design.md](../../01-目标/ecmascript.vue3/vue3-balanced-design.md)
- [docs/01-目标/ecmascript.vue3/vue3-module-mapping-rules.md](../../01-目标/ecmascript.vue3/vue3-module-mapping-rules.md)
- [docs/01-目标/ecmascript.vue3/vue3-api-coverage-matrix.md](../../01-目标/ecmascript.vue3/vue3-api-coverage-matrix.md)
- [docs/01-目标/ecmascript.vue3/vue3-mapping-details.md](../../01-目标/ecmascript.vue3/vue3-mapping-details.md)
- [docs/01-目标/ecmascript/host-alignment.md](../../01-目标/ecmascript/host-alignment.md)
- [docs/02-计划/workstream-dashboard.md](../workstream-dashboard.md)
