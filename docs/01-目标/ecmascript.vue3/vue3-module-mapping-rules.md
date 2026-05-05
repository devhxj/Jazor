# ECMAScript.Vue3 模块映射规则

> Status: active target
> Updated: 2026-05-03
> Positioning: 规定 `ECMAScript.Vue3` host binding、用户生成模块、record/object lowering 与 Vue runtime import/export 之间的映射边界。

## 1. 目标

Vue3 映射的目标不是把 Vue 官方示例逐字翻译成 C# API，而是让 C# authoring 通过类型系统、record、overload、delegate 和 attribute 表达清楚，最终生成 Vue runtime 接受的标准 JavaScript。

这份规则约束四件事：

- `src/ECMAScript.Vue3/Vue3.cs` 如何声明外部 Vue module binding；
- 用户 `[ECMAScriptModule]` 模块如何生成 `.mjs` 和 named export；
- compiler 允许在哪些稳定通用规则上参与 lowering，哪些 Vue 细节必须留在 public C# surface 中表达。
- `ECMAScript.Vue3` 作为外部库样例时，如何组织可维护的 partial 文件目录。

## 2. 模块类型

### 2.1 外部 host module binding

外部 host module binding 是对已有 JavaScript module 的 C# 投影。例如：

```csharp
[ECMAScript("npm:vue@3")]
[Description("@#")]
public static class Vue3
{
    [Description("@#h")]
    public extern static IVNode H(string type);
}
```

规则：

- `[ECMAScript("npm:vue@3")]` 声明 import source；
- 静态成员通过 runtime name 映射为 named import；
- `Vue3` 静态类本身不生成运行时对象；
- 嵌套 marker type、interface、delegate 主要服务 C# 类型检查，不生成 JS 声明。

调用：

```csharp
Vue3.H("div");
```

应生成：

```js
import { h } from "npm:vue@3";
h("div");
```

### 2.2 用户生成 module

用户 module 是 Jazor 从 C# 静态类生成的 `.mjs`：

```csharp
[ECMAScriptModule("components/panel.mjs")]
public static class PanelModule
{
    public static IVueComponent Child = Vue3.DefineComponent(new VueComponentOptions
    {
        Name = "ChildView"
    });
}
```

规则：

- `[ECMAScriptModule("components/panel.mjs")]` 声明输出 module 路径；
- public static field / property / method 生成 named export；
- private member 只在当前 module 内可见；
- 不支持 default export；
- 当前 module 内引用自身成员不生成 import；
- 引用其他 module 的 public member 走 named import。

## 3. 名称映射

### 3.1 Runtime name 优先级

成员最终 JS 名称必须来自稳定配置：

- 优先使用 whitelist alias；
- 其次使用显式 name mapping attribute，例如 `[Description("@#h")]`；
- 否则使用 C# symbol name；
- 对 record property、slot property、props key 同样适用。

示例：

```csharp
[Description("@#class")]
public Either<string, string[], VueProps, VueValue[]>? Class { get; init; }
```

应生成对象键：

```js
{ class: value }
```

### 3.2 禁止 default import/export

模块边界统一使用 named import/export：

- member runtime name 解析为 `default` 时必须报错；
- 不通过 compiler fallback 偷偷生成 default import；
- 如果上游 JS 只有 default export，应在 binding 层显式设计 named bridge，而不是让调用点绕过规则。

### 3.3 Import alias 稳定性

compiler 可以为了避免本地名称冲突生成 import alias，但 alias 必须稳定：

- import source 按稳定顺序输出；
- 同一 export 的 local alias 不随遍历顺序漂移；
- source map 和 generated module 可复现。

## 4. Type-only 与 runtime value 边界

Vue3 中大量类型只参与 C# 语义，不生成 JS：

| C# shape | JS 形态 |
|----------|---------|
| `IVNode` | erased type contract |
| `IVueComponent` / `IVueComponent<TProps>` / `IVueComponent<TProps, TSlots>` | erased component value contract |
| `VueProps` / `VueSlots` base record | structural object contract |
| delegate | JS function shape contract |
| interface | erased contract |

注意：

- 类型参数是编译期约束，不自动生成 Vue runtime declaration；
- `VueComponentOptions<TProps>` 不因为 `TProps` 自动生成 `props`；
- runtime `props` / `emits` 必须通过 `Props` / `Emits` 显式声明。

## 5. Record 到 object literal

record 是 Vue object/value authoring 的主路径：

```csharp
new VueObject
{
    Class = "primary",
    Id = "save",
    Attrs = new VueDictionary { ["aria-label"] = "Save" }
}
```

映射规则：

- structural-lowered record 生成 object literal；
- property name 使用 runtime name；
- 未赋值或静态 `null` 字面量省略；
- 非字面量 runtime null 不静态猜测；
- `[Spread]` property 展开到当前 object；
- indexer / `Add(string, value)` / initializer 应保持同一 string-key object-literal 语义。

`VueObject` 不应获得 Vue-only compiler 特路。它只是普通 record + dictionary + `[Spread]` 的组合。

同时，`VueObject` 的 convenience member 也必须保持克制：

- 只收高频、直接映射最终 key、类型单义的属性；
- `aria-*` 保持在 `Attrs` / indexer；
- `data-*` 保持在 `Dataset` / indexer；
- 长尾与项目特定属性保持在 typed props bag 或 bag/indexer 路线；
- 不把“少写几个字符”的诉求演化成新的 prefix magic 或 compiler 特路。

## 6. `[Spread]` 规则

`[Spread]` 是通用 record flattening 规则，不是 Vue 专属规则：

```csharp
public record VueObject<TProps> : VueObject
    where TProps : VueProps
{
    [Spread]
    public TProps? Props { get; init; }
}
```

规则：

- 静态 `null` spread member 不生成；
- object literal spread 保持声明顺序和覆盖语义；
- typed props、attrs、dataset、raw、events 都用同一套展开规则；
- `Dataset` 不做 `data-*` 前缀推断，调用者应写最终 key；
- 不为 `Style`、`Class`、`Dataset` 增加 compiler prefix/format magic。

## 7. `H(...)` 与 children-to-slot

`H(...)` 是 Vue render authoring 主入口，但不是 compiler 中的 Vue API 目录表。

当前 `H(...)` overload 家族已按 canonical 分类收敛：

- element：`H(type)` / `H(type, child)` / `H(type, props)` / `H(type, props, child)`；
- component：`H(component)` / `H(component, child)` / `H(component, slots)` / `H(component, props)` / `H(component, props, child|slots)`；
- typed component：在同一分类下叠加 `TProps`、`TSlots`、`VueObject<TProps>`；
- direct child 统一使用 `IVNode`（节点值）与 `VueChild`（text/number/bool/`IVNode[]`）两条边界。

### 7.1 Element

element child 不做 default slot wrapping：

```csharp
H("div", child)
```

生成：

```js
h("div", child)
```

### 7.2 Component direct child

component direct child 映射为 default slot：

```csharp
H(component, child)
```

生成：

```js
h(component, { default: () => child })
```

component + props + child：

```csharp
H(component, props, child)
```

生成：

```js
h(component, props, { default: () => child })
```

### 7.3 Intrinsic 边界

default-slot sugar 当前由 `ChildrenToSlotIntrinsic` 处理，识别条件是：

- 调用方法最终 runtime name 是 `h`；
- 方法来自可导入 host module；
- 同一 host type 暴露稳定 contract，例如 `IVNode`、`VueChild`、`IVueComponent*`、`IVueSlotComponent<TSlots>`、`VueProps`；
- typed slot 的 default slot 能由返回同宿主 `IVNode` 的 delegate 表达。

这不是 `ECMAScript.Vue3` 命名空间特判。外部类似 host 只要满足同一稳定 contract，也应走同一 lowering。

### 7.4 Evaluation rule

- literal child 可以直接生成 slot object；
- 非 literal child 必须保留单次求值和快照语义；
- 需要 IIFE 时允许生成 IIFE，不为了“更像手写 JS”破坏求值顺序或副作用次数。

## 8. Props 与 emits

runtime `props` / `emits` 声明必须显式：

```csharp
new VueComponentOptions<CounterProps>
{
    Props = ["value"],
    Emits = ["update:value"],
    Setup = Setup
}
```

规则：

- generic type 提供 authoring 类型约束；
- runtime declaration 由显式 property 生成；
- `Props` / `Emits` 分别同时覆盖 array-form 与 object-form；
- `[Props]` / `[Emits]` 只保留为底层 compiler host contract，不作为 Vue3 public authoring API 推广。

## 9. Dictionary 与 arbitrary key

任意 key 通过 dictionary-like object authoring 表达：

```csharp
new VueDictionary
{
    [".name"] = "some-name",
    ["^width"] = "100",
    ["data-kind"] = "primary"
}
```

规则：

- string key 必须是静态可确定字符串；
- 只有声明了显式 `Symbol` key contract 的 host 才允许 symbol-key object authoring，例如 `this[Symbol]` 或 `Add(Symbol, ...)`；不是所有 string-key object host 都自动支持 `Symbol`；
- 显式 `Symbol` key contract 生成 computed property；
- unsupported dynamic key 必须诊断，不静默生成错误 JS；
- 值类型要由 dictionary/helper type 真实表达；
- listener key 如 `onClick` 应优先通过 `VueEventHandlers` 或 `VueAttributeListeners*` 这类 callable-aware helper 表达，不依赖不存在的隐式转换。

## 10. External library 规则

外部库应依赖公共 C# surface 和基础 binding attribute，而不是依赖 compiler 私有特路：

- 可以声明 host module、runtime name、record、delegate、overload；
- 可以复用 `VueObject`、`VueProps`、`VueSlots` 等公共 contract；
- 不应要求新增 Vue namespace hardcoding；
- 不应通过 `Op.Compiler` 解决普通 authoring convenience；
- 只有真正需要上下文敏感 AST 协议的 host 行为，才考虑 compiler intrinsic。

## 11. 禁止项

这些不属于 Vue3 module mapping：

- `.vue` SFC authoring；
- template directive 语法；
- `<script setup>` compiler macros；
- SSR renderer API；
- custom renderer API；
- dataset/class/style prefix magic；
- 为单个 Vue 示例新增 `SemanticWalker` 分支；
- 为了输出更短 JS 牺牲 evaluation order、side-effect count 或 usage-site diagnostics。

## 12. 模块目录规范（外部库样例）

`ECMAScript.Vue3` 采用“壳文件 + 分层 partial”目录规范：

- `src/ECMAScript.Vue3/Vue3.cs`
  - 仅保留 host module 映射入口（例如 `[ECMAScript("npm:vue@3")]`）。
  - 保留顶层共享 delegate / handle 等公共契约类型。
- `src/ECMAScript.Vue3/Api/`
  - 放 `Vue3.Api*.cs`，只承载 `Vue3` 静态 API 成员（`extern static`）。
- `src/ECMAScript.Vue3/Types/`
  - 放 `Vue3.Types.*.cs`，承载 `Vue3` 嵌套类型（props/options/directive/plugin/app 等）。

约束：

- 不把 `Api` 与 `Types` 混在同一文件继续膨胀；
- 不把 `Vue3.cs` 回流成“超大单文件”；
- 只允许通用映射特性作为入口硬编码，不在 compiler 中加 `ECMAScript.Vue3` 名称特判。

## 13. 变更检查清单

新增或修改 Vue3 映射时必须检查：

- 是否能用 record / overload / generic / delegate / nullable / attribute 表达；
- 是否需要 runtime import，如果需要，是否是 named import；
- 是否会生成 default export/import，如果会，必须改设计；
- 是否误把 type-only contract 变成 runtime object；
- 是否保持 record structural lowering 的通用性；
- 是否引入了 Vue-only compiler 分支；
- 是否有 compiler emission 测试和 public reflection/proxy 测试；
- 是否更新 `vue3-api-coverage-matrix.md` 与 `vue3-mapping-details.md`。

## 14. 参考

- [ECMAScript.Vue3 平衡式目标设计](./vue3-balanced-design.md)
- [ECMAScript.Vue3 映射细节设计](./vue3-mapping-details.md)
- [ECMAScript.Vue3 API 覆盖矩阵](./vue3-api-coverage-matrix.md)
- [src/ECMAScript.Vue3/Vue3.cs](../../../src/ECMAScript.Vue3/Vue3.cs)
- [src/ECMAScript.Vue3/Api](../../../src/ECMAScript.Vue3/Api)
- [src/ECMAScript.Vue3/Types](../../../src/ECMAScript.Vue3/Types)
- [src/Jazor.Compiler/core/ChildrenToSlotIntrinsic.cs](../../../src/Jazor.Compiler/core/ChildrenToSlotIntrinsic.cs)
- [src/Jazor.Compiler/ImplementationPrinciples.md](../../../src/Jazor.Compiler/ImplementationPrinciples.md)

