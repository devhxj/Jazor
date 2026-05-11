# ECMAScript.Vue3 平衡式目标设计

> Updated: 2026-05-02

## 1. 核心问题

现有实现已经证明了几件事：

- `record` structural lowering 可以作为通用对象字面量路线；
- `[Spread]` 可以作为通用 flattening 语法糖；
- 静态 `null` 省略可以作为通用优化；
- object-literal host 的 indexer / `Add(string, ...)` lowering 可以统一到一条路径。

真正不平衡的地方在于：

- default-slot sugar 已移出 `SemanticWalker` 的 Vue 命名分块，当前由通用 `ChildrenToSlotIntrinsic` 基于 imported `h` 和同宿主 slot contract 处理；
- `VueObject.Class` 已收敛为命名 native union `VueClassValue`；这类对象成员值上的 union 应继续走显式强类型 union contract，而不是回退到泛型 union wrapper 或再包一层无语义 wrapper；
- `VueSetupContext.Attrs` / `Slots` 已有真实 read-side bag，且补齐了 `VueAttributeListeners*` / `VueScopedSlots<TScope>` helper；后续应继续按使用反馈收敛 helper 族；
- `H(...)` overload 已完成第一轮 canonical 收敛（`IVNode` + `VueChild`），但仍需要持续约束后续新增 API 不回到示例镜像扩张；
- `Dataset`、`Style`、slot、directive value 这些高频面还缺少统一的 helper 形状。

## 2. 平衡标准

这条线以后按下面的标准设计：

| 保留在 compiler | 下沉到类型/特性 |
|------|------|
| 通用 record structural lowering | 框架名字硬编码 |
| `[Spread]` flattening | Vue-only prefix / format 魔法 |
| 静态 `null` 省略 | 新的 `OptionalAttribute` omission 协议 |
| canonical host contract 识别 | 按 docs 示例逐项镜像的特例 |
| 真实 lowering 所需的少量入口判定 | 没有稳定合同的临时捷径 |

额外约束：

- `null` 省略是通用 lowering 优化，不是 Vue 专属协议；
- 如果需要区分“缺失”和“显式 null”，应使用真实的专门类型或协议，而不是把 compiler 改成隐式 optional 系统；
- 能由 C# 类型系统表达的，优先用 `record`、`overload`、`generic`、`delegate`、`attribute`；
- 只有在 C# 自身表达力确实不够时，才增加一个小而明确的 helper type。

## 3. 新方案

### 3.1 compiler 侧

compiler 只保留四类 Vue 相关能力：

1. 通用结构化 record lowering。
2. 通用 `[Spread]` flattening。
3. 通用 object-literal host lowering。
4. 少量稳定 host contract 的识别。

这里的“少量稳定 host contract”应该是基于显式特性、稳定类型形状和统一 lowering 规则，而不是基于 `ECMAScript.Vue3` 这个名字本身到处硬编码。

这意味着：

- 不再为单一 Vue 示例增加专用 lowering 分支；
- 不再把 `dataset` 前缀、class 变体、slot 形状这些问题塞进 compiler 里的名字特判；
- 不再让 `SemanticWalker` 同时承担“语义 lowering + Vue API 目录表”的职责。

### 3.2 `VueObject` 侧

`VueObject` / `VueObject<TProps>` 应继续作为结构化值对象 authoring surface，而不是隐性运行时对象。

建议遵守：

- 继续依赖通用 record lowering；
- `Props` / `Attrs` / `Raw` / `Dataset` 保持对象形状语义，不引入 Vue-only compiler 分支；
- 字符串索引键继续走同一条 object-literal lowering 路线；
- `Dataset` 只接受显式最终键，不在 compiler 里做 `data-*` 推断；
- `Class` 这类对象成员值可以保留命名 union bridge，例如 `VueClassValue`；关键是保持 runtime 语义直接可读，不回退到泛型 union wrapper，也不要围绕它再发明更差的替代包装。

这里的方向不是“更多 magic”，而是“更少 leakage”。

### 3.3 `VueSetupContext` 侧

`VueSetupContext.Attrs` / `Slots` 应该是真实可读的 bag contract，而不是只有名字的空壳。

目标是：

- `VueAttributeBag` 提供最小但真实的读取面；
- `VueSlotBag` 提供最小但真实的 slot 读取/调用面；
- typed / untyped setup context 的职责边界清晰；
- 读侧 bag 和写侧 record/slot record 分离，不互相偷语义。

### 3.4 `H(...)` 侧

`H(...)` 可以继续作为 Vue authoring 的主入口。当前已按 canonical 家族收敛（element/component/props/slots/direct-child），后续不应再回到“把 Vue 文档里的每个组合都镜像一遍”的 overload 矩阵。

更合理的方向是：

- 保留少量语义不同的 canonical overload；
- 让 C# 侧的等价写法收敛到更少的入口形状；
- 让 helper type 承担真正有差异的 value shape；
- 用编译器识别稳定合同，而不是用更多外部库特路去补洞。

### 3.5 null 省略

structural-lowered record 的静态 `null` 省略应该继续作为通用优化：

- 主构造参数和 object initializer 成员如果能被 Roslyn 静态证明为 `null`，就不生成；
- 未赋值或静态 `null` 都可以被省略；
- 非字面量的运行时 `null` 仍按普通值成员生成；
- 这条规则对所有 record 生效，不单独为 `VueObject` 开口子。

## 4. 设计结论

这条线最终要得到的是：

- compiler 只认稳定特性，不认 Vue 名字本身；
- public surface 尽量用 C# 自己的类型系统说话；
- 真正需要桥接的 union 优先用命名 native `union` 类型表达；native union 无法保留精确 tagged projection 时使用 `[System.Runtime.CompilerServices.Union]` + `IUnion` fallback；
- 不用 `OptionalAttribute` 这种新语法去补当前已经能由 lowering 解决的问题；
- Vue 只作为被投影的 host contract，而不是 compiler 的特例命名空间。

## 5. 参考

- [ECMAScript.Vue3 映射细节设计](./vue3-mapping-details.md)
- [ECMAScript.Vue3 模块映射规则](./vue3-module-mapping-rules.md)
- [ECMAScript.Vue3 API 覆盖矩阵](./vue3-api-coverage-matrix.md)
- [src/ECMAScript.Vue3/Vue3.cs](../../../src/ECMAScript.Vue3/Vue3.cs)
- [src/Jazor.Compiler/core/ChildrenToSlotIntrinsic.cs](../../../src/Jazor.Compiler/core/ChildrenToSlotIntrinsic.cs)
- [src/Jazor.Compiler/core/SemanticWalker.cs.Creation.cs](../../../src/Jazor.Compiler/core/SemanticWalker.cs.Creation.cs)
- [src/Jazor.Compiler/ImplementationPrinciples.md](../../../src/Jazor.Compiler/ImplementationPrinciples.md)
