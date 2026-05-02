# ECMAScript.Vue3 Authoring 落地计划

> Status: 活跃计划
> Updated: 2026-05-02
> Positioning: 以 [ECMAScript.Vue3 平衡式目标设计](../../01-目标/ecmascript/vue3-balanced-design.md)、[ECMAScript.Vue3 API 覆盖矩阵](../../01-目标/ecmascript/vue3-api-coverage-matrix.md) 和 [ECMAScript.Vue3 映射细节设计](../../01-目标/ecmascript/vue3-mapping-details.md) 为边界，推进 `src/ECMAScript/Vue3.cs` 与相关 compiler lowering 的收口实施。

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

当前已经具备的能力：

- `VueObject` / `VueObject<TProps>` 已建立在通用 record structural lowering 上。
- `[Spread]` 已经是通用 record flattening 路线。
- 静态 `null` 省略已经存在于 structural lowering。
- `VueDirective`、`VuePlugin`、`VueSlots`、registry 已经具备基础 authoring surface。
- `PropOptions` / `EmitOptions` 已把 Vue runtime `props` / `emits` 的 object-form 显式声明纳入 record authoring surface，typed generic 不再作为自动 runtime declaration 的理由。
- `VueComponentOptions*` 不再携带历史 `[Props]` / `[Emits]` 推导特性，避免依赖同 key 成员排序压制推导这种脆弱行为。
- `H(...)` default-slot sugar 已从 `SemanticWalker` 的 Vue 命名分块迁移到通用 `ChildrenToSlotIntrinsic`，识别条件是 imported `h` + 同宿主 component / props / slot 合同。
- `H(...)` 已经具备 default-slot 路线，但 overload 家族仍偏大。

当前仍需要收口的不是“功能数量”，而是“边界质量”。

## 4. 实施分解

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

- 统一 `VueDictionary`、`VueObject`、registry、plugin options 的 string-key lowering。
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

#### B1. 保留并收敛 `VueObject.Class` 的 `Either`

**目标**

- 把 `Either` 作为合理的 bridge shape 保留下来，并收敛它的暴露面。
- 在未来 native union 更成熟时，保持迁移路径是机械的，而不是让现在的 surface 再长出一层更差的包装。

**接受标准**

- 常见 class authoring 仍能自然表达。
- `Either` 保持为清晰、稳定、可迁移的 bridge shape，而不是一层只能靠文档解释的临时 hack。
- 不再围绕 `Either` 再发明更差的替代包装。

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

- `VueKey` 作为 VNode `key` 的语义 bridge，覆盖 string / number-like / `Symbol`，避免 `Either<string, Number, Symbol>` 在 C# 中无法自然接收 `Key = 42` 的双重隐式转换问题。
- `VueObject.Key` / `VueObject.Ref` 补齐 render props 高频 convenience；`Ref` 先覆盖 named template ref key，并与 `UseTemplateRef<TElement>(key)` 配套。
- `VueEventHandlers` / `VueEventHandlers<TEvent>` 作为 `VueObject.Events` 的 `[Spread]` listener bag，解决 method group 不能直接赋给 `VueValue` indexer 的问题，同时保持事件 key 为最终 `onXxx`。
- `VueObject.Is` 覆盖 string customized built-in `is` special attribute；不使用 `Either<string, IVueComponent>`，因为 C# 不允许以 interface 为源/目标的自然用户定义转换，动态组件应直接使用 component-valued `H(...)`。

#### C2. `Dataset` / `Style` / class helper 收口

重点：

- 不引入 Vue-only runtime 污染；
- 优先复用 record、indexer、attribute、delegate 等现有 C# 能力。

#### C3. 文档与 README 对齐

重点：

- `src/ECMAScript/README.md`
- `docs/01-目标/ecmascript/*`
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
- composition provide/inject（string key 与 typed injection key 已落地）
- composition helpers（`useAttrs` / `useSlots` 基础 bag 与 typed projection、`useTemplateRef`、`useId` 已落地；`useModel` 底层 helper 已覆盖 ref + get/set options，完整 modifiers / v-model 协议后续设计）
- `defineAsyncComponent`（loader/options 核心路径已落地）
- `customRef`（factory + handlers 已落地）
- `toRef` / `toRefs`（normalization、source key、typed refs projection 已落地）
- `data` / `computed` / `methods` / `watch` / `props` / `emits` / `inheritAttrs` / `expose` / lifecycle callbacks / Options provide-inject 基础面 / mixins-extends 兼容面（component definition base option 已落地；explicit `PropNames` / `EmitNames` array-form 已落地；`PropOptions` / `EmitOptions` object-form validators/defaults 已落地；data 和 lifecycle 先覆盖无 `this` callback surface，computed 先覆盖 no-`this` getter/writable registry 与自定义 `VueProps` record，methods 先覆盖 no-`this` delegate registry 与自定义 `VueProps` record，watch 先覆盖 callback / cleanup callback / method-name / options object 基础形态，provide/inject 先覆盖 object/array 声明式形态，mixins/extends 只作为低层兼容 binding）
- built-in components（`Transition` / `TransitionGroup` / `KeepAlive` / `Teleport` / `Suspense` render-function binding 已落地）
- `withDirectives` / `withModifiers`（核心 render helper 已落地；后续只做 authoring convenience 优化）
- custom elements 核心 binding（`DefineCustomElement`、`VueCustomElementOptions`、`UseHost`、`UseShadowRoot` 已落地；完整 props/events/light DOM authoring 策略后续设计）

#### D3. P2/P3 分流

重点：

- Options API full surface、custom elements 的完整 authoring 策略先设计再实现。
- SFC、template、SSR renderer、custom renderer 保持 separate workstream。

## 5. 推荐执行顺序

1. A1
2. A2
3. A3
4. B1
5. B2
6. B3
7. B4
8. C1
9. C2
10. C3
11. D1
12. D2
13. D3

理由：

- 先减 compiler 硬编码，再收 surface。
- 先收合同，再补 ergonomics。
- 先让 public surface 说真话，再谈好用。
- 最后按覆盖矩阵补 API 面，避免把缺口补成新的 compiler 硬编码。

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

## 8. 完成定义

当以下条件满足时，这条计划可转入阶段性完成：

- `SemanticWalker` 不再以 Vue 示例名作为主要设计中心；
- `VueObject.Class` 保持为清晰的 bridge shape，并能在 native union 到来时平滑迁移；
- `VueSetupContext` 的读侧 bag 可实际使用；
- `H(...)` overload 家族收敛到可治理规模；
- `record` / `[Spread]` / static `null` 省略继续保持通用。

## 9. 参考

- [src/ECMAScript/README.md](../../../src/ECMAScript/README.md)
- [src/Jazor.Compiler/ImplementationPrinciples.md](../../../src/Jazor.Compiler/ImplementationPrinciples.md)
- [docs/01-目标/ecmascript/vue3-balanced-design.md](../../01-目标/ecmascript/vue3-balanced-design.md)
- [docs/01-目标/ecmascript/vue3-module-mapping-rules.md](../../01-目标/ecmascript/vue3-module-mapping-rules.md)
- [docs/01-目标/ecmascript/vue3-api-coverage-matrix.md](../../01-目标/ecmascript/vue3-api-coverage-matrix.md)
- [docs/01-目标/ecmascript/vue3-mapping-details.md](../../01-目标/ecmascript/vue3-mapping-details.md)
- [docs/01-目标/ecmascript/host-alignment.md](../../01-目标/ecmascript/host-alignment.md)
- [docs/02-计划/workstream-dashboard.md](../workstream-dashboard.md)
