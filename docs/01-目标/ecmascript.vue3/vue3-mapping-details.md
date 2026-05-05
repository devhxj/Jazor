# ECMAScript.Vue3 映射细节设计

> Status: active target
> Updated: 2026-05-02
> Positioning: 补足 [ECMAScript.Vue3 平衡式目标设计](./vue3-balanced-design.md) 的细节合同，明确 `src/ECMAScript.Vue3/Vue3.cs` 每类 authoring surface 应该映射到什么 JavaScript 形态，以及 compiler 允许参与到什么程度。

## 1. 设计基线

Vue3 映射不是把 Vue 文档示例逐字镜像成 C# API，而是在 C# authoring 体验和最终 Vue runtime 形态之间取平衡。

平衡标准如下：

| 标准 | 设计取向 |
|------|----------|
| 最终 JS | 必须是 Vue 能直接消费的普通 `defineComponent(...)`、`h(...)`、props object、slots object、directive object、plugin object |
| C# authoring | 优先使用 record、overload、generic、delegate、attribute、nullable、IntelliSense |
| compiler 参与 | 只识别通用 lowering、基础绑定特性和少量稳定 host contract |
| Vue 专用逻辑 | 只能作为无法用通用能力表达时的最后手段，并且要有迁移退出路径 |
| union 表达 | 方法边界优先 overload；对象成员值或 normalization boundary 无法 overload 时使用命名 `[ECMAScriptUnion]` contract，未来可迁移到 C# native union |

这意味着：

- `VueObject`、`VueSlots`、`VueDirective`、`VuePlugin` 都应是 plain object authoring surface，不是运行时 CLR-like object。
- compiler 不应该知道 `Dataset` 需要加 `data-` 前缀，也不应该知道某个第三方库的属性格式。
- active public surface 不再以泛型 union wrapper 作为主路径；需要 bridge 时使用具名 contract。
- 外部扩展库不应依赖新的 Jazor compiler 特性来完成语义 lowering；语义应通过 `ECMAScript.Vue3` 暴露的公共 C# 合同表达。

## 2. Compiler 参与边界

| 能力 | 输入 | 输出 | 是否 Vue 专属 |
|------|------|------|----------------|
| module import | `[ECMAScript("npm:vue@3")]` + `[Description("@#name")]` | `import { name } from "npm:vue@3"` | 否，基础绑定 |
| record structural lowering | 任意 `record` object creation | object literal | 否 |
| static null omission | record 构造实参 / initializer 值为静态 `null` | 不生成该成员 | 否 |
| `[Spread]` | record 实例属性 | inline object members 或 `...expr` | 否 |
| string-key object literal | indexer / `Add(string, value)` | object property | 否 |
| `[Props]` / `[Emits]` | core host binding 内部属性 | inferred string array member | 否，基础绑定推导；不标注在 Vue3 public options 上 |
| `H(...)` default slot sugar | component + direct child | `{ default: () => child }` children object | 已迁移到 `ChildrenToSlotIntrinsic`；保持为稳定 children-to-slot contract |
| typed default slot validation | typed slot component + direct child | 诊断或允许 sugar | 由 `ChildrenToSlotIntrinsic` + typed slot contract 校验；保持最小通用 intrinsic |

`SemanticWalker` 不应继续扩张成 Vue API 目录表。新增 Vue surface 时，默认先问：

1. 能否用现有 `Description("@#...")` 做成员名映射？
2. 能否用 record structural lowering 表达 plain object？
3. 能否用 overload / generic / delegate 表达强类型？
4. 能否用 `[Spread]` 或 string-key object literal 表达对象任意性？
5. 如果必须新增 compiler 逻辑，它是否是通用能力而不是 Vue 名字特判？

## 3. Module 与导入映射

`Vue3` 静态类本身不应生成运行时对象。它是 `npm:vue@3` 的 C# host 投影。

| C# | JS |
|----|----|
| `Vue3.DefineComponent(...)` | `defineComponent(...)` |
| `Vue3.H(...)` | `h(...)` |
| `Vue3.CreateApp(...)` | `createApp(...)` |
| `Vue3.CreateSSRApp(...)` | `createSSRApp(...)` |
| `Vue3.Ref(...)` | `ref(...)` |
| `Vue3.Computed(...)` | `computed(...)` |
| `Vue3.Transition` | `Transition` |
| `Vue3.KeepAlive` | `KeepAlive` |
| `Vue3.Teleport` | `Teleport` |
| `Vue3.Suspense` | `Suspense` |
| `Vue3.OnMounted(...)` | `onMounted(...)` |

导入规则：

- 只导入实际使用的 Vue runtime member。
- 同一 member 只导入一次，并保持稳定 alias。
- `Vue3`、`IVNode`、`IVueComponent<T>`、`VueProps` 等类型只参与 C# 类型检查，不生成 JS 声明。

## 4. Object / Record 映射

所有 record authoring surface 都应走同一条 structural lowering：

```csharp
public sealed record ButtonProps : Vue3.VueProps
{
    [Description("@#label")]
    public string? Label { get; init; }
}

new ButtonProps { Label = "Save" }
```

目标 JS：

```js
{ label: "Save" }
```

### 4.1 成员生成规则

| C# 成员状态 | JS 输出 |
|-------------|---------|
| 未赋值 | 不生成 |
| 静态可证明为 `null` | 不生成 |
| 运行时可能为 `null` 的表达式 | 正常生成该表达式 |
| 非 `null` 常量或表达式 | 正常生成 |
| 显式空数组 / 空对象 | 正常生成 |

这条规则是通用 record 优化，不是 `VueObject` 专属协议。不要引入 `OptionalAttribute` 承担同一件事。

### 4.2 `[Spread]` 规则

`[Spread]` 表示“把该成员的对象形态展开到外层 object literal”。

| 输入 | 输出 |
|------|------|
| `[Spread] Props = new ButtonProps { Label = "Save" }` | `{ label: "Save" }` |
| `[Spread] Attrs = new VueDictionary { ["aria-label"] = "Save" }` | `{ "aria-label": "Save" }` |
| `[Spread] Raw = externalObject` | `{ ...externalObject }` |
| `[Spread] Dataset = null` 且静态可证明 | 不生成 |

约束：

- `[Spread]` 只能标在参与 structural lowering 的 record 实例属性上。
- `[Spread]` 不能和显式 JS property name 同时使用，因为一个表示 flatten，一个表示命名嵌套成员。
- 展开不做 Vue prefix / format 推断；`Dataset` 里的 key 必须已经是最终 key。
- 展开顺序是可观察语义，不能为了“美观”重排导致覆盖顺序变化。

### 4.3 object-literal key authoring

支持 string-key indexer / collection initializer，以及显式 `Symbol` key，是为了表达 JavaScript object key 的任意性。

```csharp
new Vue3.VueObject
{
    [".name"] = "some-name",
    ["^width"] = "100",
    ["data-user-id"] = "42"
}
```

目标 JS：

```js
{ ".name": "some-name", "^width": "100", "data-user-id": "42" }
```

```csharp
var countKey = Global.SymbolFn("count");
new Vue3.VueDictionary
{
    [countKey] = 1
}
```

目标 JS：

```js
{ [countKey]: 1 }
```

规则：

- key 是最终 emitted key，不做 PascalCase、kebab-case、`data-`、directive prefix 推断。
- string literal key 生成普通 object property；`Symbol` key 在 indexer / `Add(...)` contract 显式声明 `ECMAScript.Symbol` 时生成 computed property。
- 不能稳定翻译的 key 应诊断，而不是退回错误 JS。
- 重复 key 按 JS object literal 语义处理，后出现的属性覆盖先出现的属性。

## 5. Props 与 `VueObject`

### 5.1 `VueProps`

`VueProps` 是 typed props record 的基类。用户通过继承 record 声明组件 props：

```csharp
public sealed record CounterProps : Vue3.VueProps
{
    [Description("@#initialCount")]
    public int InitialCount { get; init; }
}
```

映射规则：

- object creation lower 成 plain props object。
- public instance property name 参与 C# typed props authoring；是否生成 Vue runtime `props` declaration 由 `Props` 显式决定。
- 属性值类型只用于 C# authoring 和 IntelliSense，运行时不生成 CLR 类型元数据。
- 如果需要 Vue runtime prop validators/defaults，使用显式 `VuePropOptions<T>` / `VuePropRegistry<T>` 或自定义 `VueProps` options record，不能把普通 typed props record 偷偷升级成 validator object。

### 5.2 `VueObject`

`VueObject` 是 `h(...)` props 和 root props 的便捷对象面。

| C# 成员 | JS key | 规则 |
|---------|--------|------|
| `Is` | `is` | string customized built-in special attribute；动态组件直接用 component-valued `H(...)` |
| `Key` | `key` | `VueKey`，支持 string / number-like / `Symbol` |
| `Class` | `class` | 支持 string、string array、object form、mixed array；用 `VueClassValue` 表达 bridge union |
| `Style` | `style` | 接受 typed record 或 dictionary object；不展开 |
| `Ref` | `ref` | named template ref key，配合 `UseTemplateRef<TElement>(key)` |
| `Events` | flatten | `[Spread]`，事件 key 必须写最终 `onXxx` listener prop |
| `Id` | `id` | 普通 DOM/Vue prop |
| `Title` | `title` | 普通 DOM/Vue prop |
| `Attrs` | flatten | `[Spread]`，额外 fallthrough attrs |
| `Dataset` | flatten | `[Spread]`，key 必须写最终 `data-*` |
| `Raw` | flatten | `[Spread]`，无额外解释 |
| indexer | final key | 任意 object key |

上表描述的是结构成员与代表性 convenience member。`VueObject` 的内置 convenience attrs 不是“把 HTML 所有属性都搬成 C# property”，而是受以下边界约束：

- 只纳入高频、跨项目稳定、直接映射最终 JS key、类型单义、且不需要额外运行时协议的属性；
- `aria-*` 不进入一等 property，继续通过 `Attrs` 或 indexer 写最终 key；
- `data-*` 不进入一等 property，继续通过 `Dataset` 或 indexer 写最终 key；
- 长尾或项目特定属性不继续堆进 `VueObject`，而是走 typed props bag、`Attrs`、`Dataset`、`Raw` 或 indexer；
- 不为 convenience attrs 增加 prefix magic、format 推断或其他 Vue-only compiler 行为。

当前 `VueObject` 内置的原生 convenience attrs 主要覆盖：

- 基础 DOM attrs：`Id`、`Title`、`For`、`Role`、`Tabindex`；
- 表单/输入 attrs：`Name`、`Type`、`Placeholder`、`Value`、`Disabled`、`Checked`、`Readonly`、`Required`、`Multiple`、`Selected`、`Autocomplete`、`Autofocus`；
- 输入约束 attrs：`Min`、`Max`、`Step`、`Minlength`、`Maxlength`、`Pattern`、`Accept`、`Wrap`；
- 文本/媒体/链接 attrs：`Rows`、`Cols`、`Href`、`Target`、`Rel`、`Src`、`Alt`、`Action`、`Method`。

这些成员的目标是减少“为了标准元素 props 再自定义一个本地 `VueProps` record”的频率，而不是替代全部 bag/indexer authoring。

`VueObject<TProps>` 在 `VueObject` 之上增加：

| C# 成员 | JS key | 规则 |
|---------|--------|------|
| `Props` | flatten | `[Spread]` typed props bag |

示例：

```csharp
H(Child, new Vue3.VueObject<ChildProps>
{
    Props = new ChildProps { Title = "Welcome" },
    Is = "vue:child-panel",
    Key = 42,
    Class = new[] { "panel", "active" },
    Ref = "childPanel",
    Events = new Vue3.VueEventHandlers { ["onClick"] = Save },
    Attrs = new Vue3.VueDictionary { ["aria-label"] = "Welcome" },
    Dataset = new Vue3.VueDictionary { ["data-kind"] = "child" },
    ["role"] = "button"
})
```

目标 JS 形态：

```js
h(child, {
  title: "Welcome",
  is: "vue:child-panel",
  key: 42,
  class: ["panel", "active"],
  ref: "childPanel",
  onClick: save,
  "aria-label": "Welcome",
  "data-kind": "child",
  role: "button"
})
```

`aria-*` / `data-*` 以及其他长尾属性继续按最终 key authoring：

```csharp
H("button", new Vue3.VueObject
{
    Attrs = new Vue3.VueDictionary
    {
        ["aria-label"] = "Save"
    },
    Dataset = new Vue3.VueDictionary
    {
        ["data-kind"] = "primary"
    },
    ["enterkeyhint"] = "done"
})
```

### 5.3 `VueDictionary`、`VueValue` 与 `VueKey`

`VueDictionary<TValue>` 是 plain object authoring surface，不是 `Map`。

- `this[string key]` / `Add(string, value)` 生成普通 object property；
- `this[Symbol key]` / `Add(Symbol, value)` 生成 computed property；
- collection initializer 只是 C# authoring 便利，不生成 runtime `Add(...)` 调用。

`VueValue` 是常见 object value 的 bridge contract：

- primitive：`string`、`bool`、number-like、`BigInt`、`char`
- object：`VueProps`
- array：`VueValue[]`

`VueKey` 是 Vue VNode `key` 的专用 bridge contract。它只覆盖 Vue 官方接受的 `string | number | symbol` 语义，不退化为 `object`，也不回退到旧的泛型 union wrapper，因为 `Key = 42` 这类 authoring 需要自然接受 number-like 输入，而不是依赖多段隐式转换链。`VueKey` 直接提供 string、number-like 和 `Symbol` 的隐式转换，最终仍擦除为原始 JS 值。

设计约束：

- 方法参数边界如果 overload 能表达，优先 overload。
- object 成员值无法 overload 时，使用命名的 `[ECMAScriptUnion]` bridge type，例如 `VueValue`、`VueClassValue`、`VueComputedValue<TValue>`。
- 不引入只有“给旧的泛型 union wrapper 换个名字”但没有稳定公共语义的包装；命名 union 必须对应一个真实、可复用的 host contract。

### 5.4 `VueEventHandlers`

`VueEventHandlers` 是 render-function listener props 的 string-key bag：

```csharp
new Vue3.VueObject
{
    Events = new Vue3.VueEventHandlers
    {
        ["onClick"] = Save,
        ["onFocus"] = Vue3.WithModifiers(Focus, new[] { "stop" })
    }
}
```

输出是普通 props object：

```js
{
  onClick: save,
  onFocus: withModifiers(focus, ["stop"])
}
```

设计约束：

- key 必须是 Vue render function 的最终 listener prop，例如 `onClick`、`onUpdate:modelValue`；
- 不做 `Click -> onClick` 推断，因为那会变成 Vue-specific compiler 规则；
- `VueEventHandlers` 的值类型是 `Action`，解决 `["onClick"] = Save` 这种 method group 无法自然赋给 `VueValue` 的问题；
- `VueEventHandlers<TEvent>` 的值类型是 `VueEventHandler<TEvent>`，用于 `MouseEvent`、`KeyboardEvent` 等 typed payload；
- 如果需要完全任意值形态，仍可退回 `VueObject` indexer 或 `Raw` bag。

## 6. Component Definition 映射

`DefineComponent(...)` 的参数是 component options record，输出是 compile-time component identity。

| C# | JS |
|----|----|
| `VueComponentOptions` | `defineComponent({ props?: [...], emits?: [...], setup?, render? })` |
| `VueComponentOptions<TProps>` | `defineComponent({ props?: explicit, emits?: explicit, setup })` |
| `VueSlotComponentOptions<TSlots>` | `defineComponent({ props?: explicit, emits?: explicit, setup })` |
| `VueComponentOptions<TProps, TSlots>` | `defineComponent({ props?: explicit, emits?: explicit, setup })` |

### 6.1 Options 成员

| C# 成员 | JS key | 规则 |
|---------|--------|------|
| `Name` | `name` | 直接映射 |
| `Components` | `components` | registry object |
| `Directives` | `directives` | registry object |
| `InheritAttrs` | `inheritAttrs` | shared base option |
| `Expose` | `expose` | shared base option string array |
| `Provide` | `provide` | object-form provide，`VueProps` / `VueDictionary` |
| `Inject` | `inject` | array-form `string[]` 或 object-form `VueProps` |
| `Mixins` | `mixins` | low-level compatibility binding |
| `Extends` | `extends` | low-level compatibility binding |
| `Data` | `data` | no-`this` `VueDataCallback` |
| `BeforeCreate` | `beforeCreate` | no-`this` lifecycle callback |
| `Created` | `created` | no-`this` lifecycle callback |
| `BeforeMount` | `beforeMount` | no-`this` lifecycle callback |
| `Mounted` | `mounted` | no-`this` lifecycle callback |
| `BeforeUpdate` | `beforeUpdate` | no-`this` lifecycle callback |
| `Updated` | `updated` | no-`this` lifecycle callback |
| `BeforeUnmount` | `beforeUnmount` | no-`this` lifecycle callback |
| `Unmounted` | `unmounted` | no-`this` lifecycle callback |
| `Activated` | `activated` | no-`this` keep-alive lifecycle callback |
| `Deactivated` | `deactivated` | no-`this` keep-alive lifecycle callback |
| `ErrorCaptured` | `errorCaptured` | `VueErrorCapturedCallback` |
| `RenderTracked` | `renderTracked` | `VueDebuggerCallback` |
| `RenderTriggered` | `renderTriggered` | `VueDebuggerCallback` |
| `ServerPrefetch` | `serverPrefetch` | `VueServerPrefetchPromiseCallback` |
| `Props` | `props` | `string[]` array-form 或 `VueProps` object-form declaration |
| `Emits` | `emits` | `string[]` array-form 或 `VueProps` object-form declaration |
| `Setup` | `setup` | delegate/function |
| `Render` | `render` | delegate/function |

### 6.2 Runtime Props / Emits Declaration

泛型 component options 的 `TProps` / `TSlots` 是 C# authoring contract。它们约束 `Setup(...)`、`VueSetupContext<TSlots>` 和 `H(...)` 调用，但不自动发明 Vue runtime `props` / `emits` 选项。

运行时声明通过同一个 canonical 成员显式选择形态：

| C# | JS | 使用场景 |
|----|----|----------|
| `Props = ["title"]` | `props: ["title"]` | 只需要 Vue array-form props |
| `Props = new MyPropOptions { ... }` | `props: { ... }` | 需要 runtime type/default/validator |
| `Emits = ["save"]` | `emits: ["save"]` | 只需要事件名声明 |
| `Emits = new VueEmitRegistry<T> { ... }` | `emits: { ... }` | 需要 emit validator |

`VuePropOptions<TValue>` 覆盖 Vue object-form prop declaration：

| C# 成员 | JS key | 规则 |
|---------|--------|------|
| `Type` | `type` | 单个 `VuePropType` 构造器，如 `VuePropType.String` -> `String` |
| `Types` | `type` | 构造器数组，可包含 `null` 表达 Vue nullable type form |
| `Required` | `required` | `bool` |
| `Default` | `default` | 字面默认值 |
| `DefaultFactory` | `default` | 无参 factory |
| `DefaultFactoryWithProps` | `default` | 接收 raw props 的 factory |
| `Validator` | `validator` | 单值 validator |
| `ValidatorWithProps` | `validator` | 接收 raw props 的 validator |

`VuePropRegistry<TValue>` 适合同一 value contract 的 string-key object-form props。异构 props 推荐声明自定义 `VueProps` record：

```csharp
public sealed record LabelPropOptions : VueProps
{
    [Description("@#label")]
    public VuePropOptions<string>? Label { get; init; }

    [Description("@#count")]
    public VuePropOptions<int>? Count { get; init; }
}
```

`VueEmitRegistry` / `VueEmitRegistry<T0...T3>` 覆盖 0 到 4 个 payload 的 object-form validator。超过 4 个 payload 时再按真实需求增加 overload，不使用 `object[]`。

`Props` / `Emits` 的 canonical public surface 通过 `VueNamesOrOptions` bridge 同时覆盖 array-form 与 object-form，并保留 `Props = ["title"]` / `Emits = ["save"]` 这类 collection-expression authoring。

### 6.3 Host-Level `[Props]` / `[Emits]`

`[Props]` / `[Emits]` 是 compiler 的基础绑定推导原语，不是 Vue3 外部 authoring API，也不应出现在 `VueComponentOptions*` 这类面向用户的 Vue public surface 上。

规则：

- `[Props]` 目标成员必须是 `string[]`，来源是指定 generic type argument 的 public instance properties。
- `[Emits]` 目标成员必须是 `string[]`，来源是指定 setup-like 成员中的稳定 emit 调用。
- 这两个特性只用于受控 host contract；外部库不应依赖它们扩展 Vue 语义。
- Vue runtime `props` / `emits` 必须通过 `Props` / `Emits` 显式声明，不能依赖推导成员被“碰巧跳过”。
- Vue3 public surface 新增功能时，优先使用显式 record 成员、overload、generic、delegate、`Description("@#...")` 和通用 record lowering。

### 6.4 Setup / Render

`Setup` delegate 映射到 Vue `setup` function。返回的 `VueRenderCallback` 映射为 render closure。

```csharp
Setup = (props, context) =>
{
    context.Emit("ready");
    return () => H("button", props.Label);
}
```

目标 JS 形态：

```js
setup: (props, context) => {
  context.emit("ready");
  return () => h("button", props.label);
}
```

如果 `Setup` 和 `Render` 同时显式设置，compiler 不应发明新语义；它们都按 object members 生成，由 Vue runtime 决定最终行为。是否增加 analyzer 提示应单独设计。

### 6.5 Options API Lifecycle

Options lifecycle hook 先覆盖不需要额外 compiler 协议的 direct callback surface：

```csharp
new VueComponentOptions
{
    Mounted = OnMounted,
    ErrorCaptured = CaptureError,
    RenderTracked = OnDebug,
    ServerPrefetch = Prefetch
}
```

目标 JS 形态：

```js
{
  mounted: onMounted,
  errorCaptured: captureError,
  renderTracked: onDebug,
  serverPrefetch: prefetch
}
```

设计约束：

- 这些成员放在 `VueComponentDefinition` 基类上，因此 `VueComponentOptions`、`VueComponentOptions<TProps>`、`VueSlotComponentOptions<TSlots>`、`VueComponentOptions<TProps,TSlots>` 共享同一套 hook。
- no-arg lifecycle hook 使用 `Action`，表达不依赖 Vue `this` 的回调；这符合 C# delegate authoring，也避免为了 `this` 绑定扩张 compiler。
- `ErrorCaptured` 使用 `VueErrorCapturedCallback`，保留返回 `false` 停止传播的 Vue 语义；void handler 可通过显式返回 `true`/`false` 的 callback 表达。
- `RenderTracked` / `RenderTriggered` 复用 `VueDebuggerCallback`，与 Composition API debug hook 事件类型一致。
- `ServerPrefetch` 使用 `VueServerPrefetchPromiseCallback`，表达 SSR promise hook。`PromiseResult` 形态仍由 Composition API `OnServerPrefetch(...)` overload 承接，避免在 option property 上制造多套同名 union。
- `this`-bound Options API authoring、`data(vm)`、`methods`、`computed`、`watch` 仍是后续设计面；当前不通过 hidden compiler magic 模拟 Vue instance `this`。

### 6.6 Options API Provide / Inject

Options composition 当前覆盖 object-form 与 function-form 两条主路径，并允许 symbol-key provide / inject source：

```csharp
var countKey = Global.SymbolFn("count");

new VueComponentOptions
{
    Provide = new VueDictionary
    {
        [countKey] = 1
    },
    Inject = new VueInjectRegistry<int>
    {
        ["count"] = countKey,
        ["optionalCount"] = new VueInjectOptions<int>
        {
            From = countKey,
            Default = 2
        }
    }
}
```

目标 JS 形态：

```js
{
  provide: { [countKey]: 1 },
  inject: {
    count: countKey,
    optionalCount: { from: countKey, default: 2 }
  }
}
```

设计约束：

- `Provide` 使用 `VueProps?`，既支持用户声明 typed record，也支持 `VueDictionary` 任意 key。
- `ProvideFactory` 使用 `VueDataCallback?`，直接映射 Vue 的 function-form `provide()`；需要 `this` 时复用 `BindThis<TThis>(VueThisDataCallback<TThis>)`，不再新增专门 compiler 特路。
- `Inject` 使用 `VueNamesOrOptions?`，覆盖官方 array form 和 object form，并保留 `Inject = ["feature"]` 这类 collection-expression authoring。
- object-form inject 可以直接使用 custom `VueProps` record，也可以使用 `VueInjectOptions<TValue>` / `VueInjectEntry<TValue>` / `VueInjectRegistry<TValue>` 表达 string key、typed `VueInjectionKey<TValue>`、raw `Symbol` source key、default literal 与 default factory。
- 更复杂的 `this`-bound Options API 长尾仍作为下一阶段设计项，但 `provide` 本身已经可以通过 `ProvideFactory + BindThis(...)` 收口；symbol-key `provide` / `inject` object form 不再要求额外 Vue 编译器特路。
- Composition API `Provide(...)` / `Inject(...)` 与 Options API `Provide` / `Inject` 是不同入口：前者是 setup-time helper，后者是 component option object member。

### 6.7 Options API Mixins / Extends

`Mixins` / `Extends` 只作为 Vue Options API 的低层兼容 binding：

```csharp
new VueComponentOptions
{
    Extends = new VueComponentOptions { Created = BaseCreated },
    Mixins = new VueComponentDefinition[]
    {
        new VueComponentOptions { Mounted = FocusMounted }
    }
}
```

目标 JS 形态：

```js
{
  extends: { created: baseCreated },
  mixins: [{ mounted: focusMounted }]
}
```

设计约束：

- 类型使用 `VueComponentDefinition`，复用已有 component options record，不复制一套 mixin-specific options。
- 这是 Vue runtime option merge 的投影，不是 C# 继承模型，也不改变 Jazor member-class inheritance 语义。
- 对新代码复用逻辑，仍优先 Composition API；该 surface 主要服务已有 Vue options objects 和库兼容。

### 6.8 Options API Data

`Data` 覆盖不依赖 Vue instance `this` 的 factory 形态：

```csharp
public sealed record LocalState : VueProps
{
    public int Count { get; init; }
}

new VueComponentOptions
{
    Data = CreateState
}

private static LocalState CreateState()
    => new LocalState { Count = 1 };
```

目标 JS 形态：

```js
{
  data: createState
}

function createState() {
  return { count: 1 };
}
```

设计约束：

- `VueDataCallback` 返回 `VueProps`，用户可以返回继承自 `VueProps` 的强类型 record；最终仍是 Vue 期望的 plain object。
- factory 必须返回 fresh object，是否复用对象实例属于用户代码语义，不由 compiler 额外检测。
- `data(vm)` 和依赖 `this` 的 Options API authoring 需要和 `methods`、`computed`、`watch` 的 this-binding 一起设计；当前不通过 compiler 注入 this。

### 6.9 Options API Computed

`Computed` 覆盖不依赖 Vue instance `this` 的 Options API computed object 形态：

```csharp
new VueComponentOptions
{
    Computed = new VueComputedRegistry<int>
    {
        { "doubled", ReadDoubled },
        { "plusOne", new VueWritableComputedOptions<int>
            {
                Get = ReadPlusOne,
                Set = WritePlusOne
            }
        }
    }
}
```

目标 JS 形态：

```js
{
  computed: { doubled: readDoubled, plusOne: { get: readPlusOne, set: writePlusOne } }
}
```

异构 computed values 使用自定义 `VueProps` record：

```csharp
public sealed record PanelComputed : VueProps
{
    public Func<int>? Count { get; init; }
    public VueWritableComputedOptions<string>? Label { get; init; }
}
```

设计约束：

- `VueComponentDefinition.Computed` 使用 `VueProps?`，避免为 Options API computed 引入 Vue-specific compiler 分支；普通 record object lowering 负责生成最终对象字面量。
- `VueComputedRegistry<TValue>` 只解决动态字符串 key 且同值类型的常见场景；它继承 `VueProps` 并通过 `Add(string, Func<TValue>)` / `Add(string, VueWritableComputedOptions<TValue>)` 支持 collection initializer，避免 method group 在 union 赋值上的 C# 绑定限制。
- indexer 类型现在收敛为命名 union `VueComputedValue<TValue>`，服务显式赋值和反射契约；推荐 authoring path 仍然是 collection initializer。
- 依赖 Vue instance `this` 的 computed getter/setter 使用 `BindThis<TThis,...>(VueThisFunc<...>/VueThisAction<...>)`；`BindThis` 通过 `ECMAScriptInline` 降级为 `function(){ return cb(this, ...arguments); }` 包装。

### 6.10 Options API Methods

`Methods` 覆盖不依赖 Vue instance `this` 的 Options API methods object 形态：

```csharp
new VueComponentOptions
{
    Methods = new VueMethodRegistry<Action>
    {
        { "reset", Reset },
        { "focus", Focus }
    }
}
```

目标 JS 形态：

```js
{
  methods: { reset: reset, focus: focus }
}
```

异构 method signatures 使用自定义 `VueProps` record：

```csharp
public sealed record PanelMethods : VueProps
{
    public Action? Reset { get; init; }
    public Func<string, bool>? Validate { get; init; }
}
```

设计约束：

- `VueComponentDefinition.Methods` 使用 `VueProps?`，保持普通 record object lowering，不引入 methods 专用 compiler 分支。
- `VueMethodRegistry<TDelegate>` 只解决动态字符串 key 且同 delegate 签名的场景；`TDelegate : Delegate` 保留 C# 方法组转换、参数类型和返回类型检查。
- collection initializer 通过 `Add(string, TDelegate)` 表达动态键；最终仍生成 methods 对象属性，不生成运行时 `Add(...)` 调用。
- Vue 会以 component public instance 作为 method `this` 调用目标；C# 侧通过 `BindThis<TThis,...>(VueThisAction<...>/VueThisFunc<...>)` 显式声明 this-contract，并保持调用点强类型。

### 6.11 Options API Watch

`Watch` 覆盖 Options API watch object 的基础声明式形态：

```csharp
new VueComponentOptions
{
    Watch = new VueWatchRegistry<int>
    {
        { "count", OnCountChanged },
        { "total", new VueWatchHandlerOptions<int>
            {
                Immediate = true,
                Deep = 1,
                Handler = OnTotalChanged
            }
        },
        { "legacy", "onLegacyChanged" }
    }
}
```

目标 JS 形态：

```js
{
  watch: {
    count: onCountChanged,
    total: { immediate: true, deep: 1, handler: onTotalChanged },
    legacy: "onLegacyChanged"
  }
}
```

支持的基础 value 形态：

- `Action<T, T>`：直接 watcher callback。
- `VueWatchCleanupCallback<T>`：带 cleanup registration 的 callback。
- `string`：按 Vue Options API 从同组件 `methods` 解析 method name。
- `VueWatchHandlerOptions<T>`：带 `immediate` / `deep` / `flush` / debug options 的 callback object。
- `VueWatchCleanupHandlerOptions<T>`：带 options 的 cleanup-aware callback object。
- `VueWatchNamedHandlerOptions`：带 options 的 method-name handler object。
- 上述任意形态的数组：通过 `VueWatchEntries<T>` 隐式转换覆盖 `string[]`、callback 数组、options 数组，以及 `VueWatchEntry<T>[]` 的 mixed array。

设计约束：

- `VueComponentDefinition.Watch` 使用 `VueProps?`，保持普通 object lowering；watch key 可以是普通属性名，也可以是 Vue 支持的简单点路径。
- `VueWatchRegistry<TValue>` 只解决动态字符串 key 且同 watched value 类型的场景；异构 watch sources 使用自定义 `VueProps` record。
- `VueWatchEntry<T>` / `VueWatchEntries<T>` 作为语法糖类型承接 array 场景复杂度；公开 indexer 使用 `VueWatchDeclaration<TValue>` 命名 union，兼容 method-group、options object 与 mixed array 的自然写法。
- 依赖 component instance `this` 的 handler 使用 `BindThis<TThis,...>`；watch cleanup handler 使用 `BindThis<TThis, TValue>(VueThisWatchCleanupCallback<TThis, TValue>)`。

## 7. Render / `H(...)` 映射

`H(...)` 是 `h(...)` 的强类型 overload surface。重载是 C# 优势，不是缺点；当前已按 canonical 家族收敛，后续新增能力应继续落在既有分类中。

### 7.1 Element `H`

| C# | JS |
|----|----|
| `H("div")` | `h("div")` |
| `H("div", "text")` | `h("div", "text")` |
| `H("div", child)` | `h("div", child)` |
| `H("div", children)` | `h("div", children)` |
| `H("div", props)` | `h("div", props)` |
| `H("div", props, children)` | `h("div", props, children)` |

element child 不做 default slot wrapping。

实现形状：

- direct child 参数统一分为 `IVNode`（节点值）和 `VueChild`（text/number/bool/`IVNode[]`）两类；
- `H("div", "text")`、`H("div", 1)`、`H("div", new IVNode[] { ... })` 通过 `VueChild` 隐式转换进入同一 canonical overload；
- compiler 仍输出普通 `h("div", child)`，不做 component slot 语义。

### 7.2 Component `H`

| C# | JS |
|----|----|
| `H(component)` | `h(component)` |
| `H(component, props)` | `h(component, props)` |
| `H(component, slots)` | `h(component, slots)` |
| `H(component, props, slots)` | `h(component, props, slots)` |
| `H(component, child)` | `h(component, { default: () => child })` |
| `H(component, props, child)` | `h(component, props, { default: () => child })` |

直接 child sugar 只对 component 生效，因为 Vue component children object 表示 slots。

实现形状：

- component direct child 同样统一分为 `IVNode` 和 `VueChild` 两类；
- `H(component, "body")` / `H(component, 1)` / `H(component, children)` 进入同一 direct-child sugar contract；
- typed component 的 props+child 形状同样保持 `TProps`/`VueObject<TProps>` + (`IVNode`/`VueChild`) 两条 canonical 边界。

### 7.3 Default slot sugar 与 IIFE

默认 slot sugar 的语义是：

```csharp
H(Child, child)
```

目标值对象形态是：

```js
h(childComponent, { default: () => child })
```

但如果 `component`、`props` 或 `child` 表达式有副作用，lowering 必须保证：

- 每个输入表达式只求值一次。
- 求值顺序与 C# 调用参数顺序一致。
- slot callback 不会把 child 表达式延迟到未来才求值，除非 C# authoring 本来就是显式 callback。

因此 compiler 可以在必要时生成 single-evaluation wrapper：

```js
((__component, __slot0) => h(__component, { default: () => __slot0 }))(makeComponent(), makeChild())
```

目标优化方向：

- 对 identifier、literal、已稳定临时变量等安全表达式，优先生成直接值对象形态。
- 只有在需要保护 evaluation order / side-effect count 时才生成 IIFE。
- 不允许为了“看起来像手写 JS”而重复求值。

### 7.4 Typed slot component 的 child sugar

当 component 类型带 `TSlots` 时，直接 child sugar 只能在以下条件成立时使用：

- slot contract 中恰好一个成员映射到 `default`。
- 该成员类型是返回 host `IVNode` 的 delegate。
- 该 delegate 不带参数；带参数 delegate 视为 scoped slot，不能由 direct child sugar 隐式满足。

否则必须使用显式 slots object：

```csharp
H(Child, new ChildSlots
{
    Default = () => H("span", "body")
})
```

诊断应说明缺少 default slot、default slot 重复、default slot 类型错误、或 scoped default slot 不能由直接 child 满足。

### 7.5 Render function helpers

这些 API 只需要基础 host binding，不应引入 Vue 专用 compiler 逻辑。

| C# | JS |
|----|----|
| `MergeProps(a, b)` | `mergeProps(a, b)` |
| `CloneVNode(vnode)` | `cloneVNode(vnode)` |
| `CloneVNode(vnode, props)` | `cloneVNode(vnode, props)` |
| `IsVNode(value)` | `isVNode(value)` |
| `ResolveComponent("Name")` | `resolveComponent("Name")` |
| `ResolveDirective("focus")` | `resolveDirective("focus")` |
| `WithDirectives(vnode, directives)` | `withDirectives(vnode, directives)` |
| `WithDirectives(vnode, d1, d2)` | `withDirectives(vnode, [d1, d2])` |
| `WithModifiers(handler, modifiers)` | `withModifiers(handler, modifiers)` |
| `WithModifiers(handler, "stop", "prevent")` | `withModifiers(handler, ["stop", "prevent"])` |

`IsVNode<T>(T value)` 使用泛型承接 Vue 官方的 unknown-like 输入，避免把 public surface 退化为 `object`。

`withDirectives` 使用 `VueDirectiveArguments` / `VueDirectiveArguments<TValue>` 表达 Vue 官方的 directive tuple。该 tuple host 映射到 JavaScript `Array`，所以输出是 Vue 可直接消费的 `[directive, value, arg, modifiers]` array shape，而不是 record object。`VueDirectiveModifierBag` 继承 `VueDictionary<bool>`，key 是最终 modifier 名，不做模板语法推断。`WithDirectives` 参数使用 `[PreserveParamsArray] params VueDirectiveArguments[]`，因此可以写 `WithDirectives(vnode, d1, d2)`，但 JS 始终保持 `withDirectives(vnode, [d1, d2])`，不会退化成 varargs。

`withModifiers` 保持官方 helper 形态：第二参数是 modifier name array。无 payload handler 使用 `Action`，有 payload handler 使用 `VueEventHandler<TEvent>`；普通 listener 优先放进 `VueObject.Events`，完全任意值形态仍可退回 `VueObject` string-key props。`WithModifiers` 参数使用 `[PreserveParamsArray] params string[]`，因此 C# 可以写 `WithModifiers(handler, "stop", "prevent")`，但 JS 仍稳定输出 `withModifiers(handler, ["stop", "prevent"])`，不会退化成 varargs。

### 7.6 Built-in components

Vue built-in components 在 render function 中按普通 component binding 使用，不引入模板语法：

| C# | JS |
|----|----|
| `H(Vue3.Transition, props, child)` | `h(Transition, props, { default: () => child })` |
| `H(Vue3.TransitionGroup, props, children)` | `h(TransitionGroup, props, { default: () => children })` |
| `H(Vue3.KeepAlive, props, child)` | `h(KeepAlive, props, { default: () => child })` |
| `H(Vue3.Teleport, props, child)` | `h(Teleport, props, { default: () => child })` |
| `H(Vue3.Suspense, props, slots)` | `h(Suspense, props, slots)` |

对应 props/slots contract：

- `VueTransitionProps` / `VueTransitionGroupProps` 覆盖 class、duration、type、hook 等 render-function props。
- `VueKeepAliveProps` 覆盖 `include` / `exclude` / `max`。
- `VueTeleportProps` 覆盖 `to` / `disabled` / `defer`。
- `VueSuspenseProps` + `VueSuspenseSlots` 覆盖 `timeout`、events、`default` / `fallback` slots。
- built-in component property 只是 `npm:vue@3` import binding，类型只用于 C# props/slots 智能感知。

## 8. Slots 映射

### 8.1 Write-side slots

`VueSlots` 是 slots object 的基类。

```csharp
public sealed record PanelSlots : Vue3.VueSlots
{
    [Description("@#header")]
    public VueSlotCallback Header { get; init; } = default!;

    [Description("@#default")]
    public VueSlotCallback Body { get; init; } = default!;
}
```

目标 JS：

```js
{ header: header, default: body }
```

规则：

- typed slot record 属性映射到 final slot key。
- `VueSlotCallback` 表示无 scope slot，是内置推荐 delegate。
- `VueSlotCallback<TScope>` 表示 scoped slot，是内置推荐 delegate。
- 外部基础 binding 可使用自定义 delegate；compiler 只要求 slot delegate 返回同宿主 `IVNode`，并通过参数数量区分 parameterless / scoped slot。
- `VueSlots["name"] = callback` 可表达任意 slot key。
- slot record 是 object literal，不生成运行时 slot class。

### 8.2 Read-side slots

`VueSetupContext.Slots` 是 runtime slots bag。

目标 contract：

| C# | JS |
|----|----|
| `context.Slots` | `context.slots` |
| `context.Slots["default"]` | `context.slots["default"]` |
| `context.Slots.Default()` 或 typed property | `context.slots.default()` |
| `context.Slots.Row(scope)` | `context.slots.row(scope)` |

当前 `VueSlotBag` 已补齐最小读取面：

- `VueSlotCallback? this[string key] { get; }`
- `Default` 读取入口映射到 `default`。
- typed `VueSetupContext<TSlots>.Slots` 返回 `TSlots`，属性访问按普通 member binding 映射。
- `VueScopedSlots<TScope>` 可作为 `UseSlots<T>()` typed projection，直接提供 scoped slot 的 `Default` + indexer 调用面。

读侧 bag 和写侧 slot record 是同一个 JS object shape，但 C# contract 不应混用：写侧强调 object literal authoring，读侧强调 runtime optional callable slots。

## 9. Setup Context 映射

| C# | JS | 规则 |
|----|----|------|
| `context.Attrs` | `context.attrs` | fallthrough attrs bag |
| `context.Slots` | `context.slots` | slots bag |
| `context.Emit("ready")` | `context.emit("ready")` | no payload |
| `context.Emit("update", value)` | `context.emit("update", value)` | one payload |
| `context.Emit("change", a, b)` | `context.emit("change", a, b)` | two payloads |
| `context.Emit("batch", a, b, c)` | `context.emit("batch", a, b, c)` | three payloads |
| `context.Emit("batch", a, b, c, d)` | `context.emit("batch", a, b, c, d)` | four payloads |
| `context.Expose(value)` | `context.expose(value)` | expose public instance shape |

`VueAttributeBag` 已覆盖的读取面：

- `VueValue? this[string key] { get; }`
- `Class` / `Style` / `Id` / `Title` convenience reads。
- 高频 attrs convenience reads：`For` / `Name` / `Type` / `Placeholder` / `Disabled` / `Readonly` / `Required` / `Tabindex` / `Role`。
- 不做 `data-*`、event listener、kebab-case 推断。

`UseAttrs<TAttrs>()` 的 typed projection 可进一步使用：

- `VueAttributeListeners`：arbitrary listener key -> `Action`；
- `VueAttributeListeners<TEvent>`：arbitrary listener key -> `VueEventHandler<TEvent>`。

`Emit(...)` 使用 overload 覆盖常见 0-4 payload，避免 `params object[]` 破坏强类型和 public no-`object` surface。官方 `$emit` / setup `emit` 支持事件名后跟额外参数；如果未来需要超过 4 个 payload，应继续按实际需求补 overload，而不是退回 loose params。

`Expose<TValue>` 接受 reference type。若传入 record object creation，record 先 lower 成 plain object，再作为 expose 参数。

## 10. App 映射

### 10.1 CreateApp / CreateSSRApp

| C# | JS |
|----|----|
| `CreateApp(component)` | `createApp(component)` |
| `CreateApp(component, props)` | `createApp(component, props)` |
| `CreateSSRApp(component)` | `createSSRApp(component)` |
| `CreateSSRApp(component, props)` | `createSSRApp(component, props)` |

typed root props：

- `TProps` 直接 lower 成 props object。
- `VueObject<TProps>` flatten `Props` 并允许 `key`、named `ref`、`class`、`style`、events、attrs、dataset、raw、indexer。
- root component 的 `IVueComponent<TProps>` / `IVueComponent<TProps,TSlots>` 只用于 C# 类型检查，不生成 runtime assertion。

### 10.2 VueApp instance

| C# | JS |
|----|----|
| `app.Version` | `app.version` |
| `app.Config` | `app.config` |
| `app.Mount("#app")` | `app.mount("#app")` |
| `app.Mount(element)` | `app.mount(element)` |
| `app.Unmount()` | `app.unmount()` |
| `app.OnUnmount(callback)` | `app.onUnmount(callback)` |
| `app.Use(plugin)` | `app.use(plugin)` |
| `app.Mixin(mixin)` | `app.mixin(mixin)` |
| `app.Component("Name", component)` | `app.component("Name", component)` |
| `app.Component("Name")` | `app.component("Name")` |
| `app.Directive("focus", directive)` | `app.directive("focus", directive)` |
| `app.Directive("focus")` | `app.directive("focus")` |
| `app.Provide("key", value)` | `app.provide("key", value)` |
| `app.RunWithContext(callback)` | `app.runWithContext(callback)` |

返回 `VueApp` 的方法保持 chainable runtime app instance。

`app.Mixin(...)` 接受 `VueComponentDefinition`，因此可以复用 `VueComponentOptions` / typed component options 的 record lowering 生成普通 Vue options object。Vue 官方不推荐在应用代码中使用全局 mixin；该 API 只作为库兼容和迁移场景的低层 binding，不作为优先 authoring path。

### 10.3 App Config

`VueAppConfig` 是 `app.config` 的 typed projection，不是 record object creation surface。用户通过普通属性赋值修改 Vue runtime app config：

```csharp
app.Config.ErrorHandler = OnError;
app.Config.CompilerOptions.IsCustomElement = IsCustomElement;
app.Config.GlobalProperties["$name"] = "jazor";
app.Config.OptionMergeStrategies["route"] = MergeRoute;
```

目标 JS：

```js
app.config.errorHandler = onError;
app.config.compilerOptions.isCustomElement = isCustomElement;
app.config.globalProperties["$name"] = "jazor";
app.config.optionMergeStrategies["route"] = mergeRoute;
```

映射规则：

| C# | JS |
|----|----|
| `ErrorHandler` | `errorHandler` |
| `WarnHandler` | `warnHandler` |
| `Performance` | `performance` |
| `CompilerOptions.IsCustomElement` | `compilerOptions.isCustomElement` |
| `CompilerOptions.Whitespace` | `compilerOptions.whitespace` |
| `CompilerOptions.Delimiters` | `compilerOptions.delimiters` |
| `CompilerOptions.Comments` | `compilerOptions.comments` |
| `GlobalProperties[key]` | `globalProperties[key]` |
| `OptionMergeStrategies[key]` | `optionMergeStrategies[key]` |
| `IdPrefix` | `idPrefix` |
| `ThrowUnhandledErrorInProduction` | `throwUnhandledErrorInProduction` |

约束：

- handler 的 unknown-like value 使用 `VueValue`，不暴露 `object`。
- `GlobalProperties` value 使用 `VueValue` bridge contract；key 是最终 runtime key，不做 `$`、camelCase 或 kebab-case 推断。
- `OptionMergeStrategies` 使用 `VueOptionMergeFunction` delegate；merge value 仍保持 unknown-like `VueValue`。
- `CompilerOptions` 指 Vue runtime compiler config，只影响使用浏览器内模板编译器的 app，不代表 Jolt、SFC 或 emit pipeline 的编译配置。
- 该 surface 只依赖 `[Description("@#...")]`、属性访问、索引器和 delegate 映射，不引入 Vue 专用 compiler 分支。

## 11. Directive 映射

### 11.1 Object-form directive

`VueDirective` / `VueDirective<TValue>` lower 成 Vue directive object。

| C# 成员 | JS key |
|---------|--------|
| `Deep` | `deep` |
| `Created` | `created` |
| `BeforeMount` | `beforeMount` |
| `Mounted` | `mounted` |
| `BeforeUpdate` | `beforeUpdate` |
| `Updated` | `updated` |
| `BeforeUnmount` | `beforeUnmount` |
| `Unmounted` | `unmounted` |
| `GetSSRProps` | `getSSRProps` |

typed directive 只改变 C# binding value 类型，不改变 JS object shape。

### 11.2 Function shorthand

`VueDirectiveFunction` / `VueDirectiveFunction<TValue>` 直接作为 directive value：

```csharp
app.Directive("focus", ApplyFocus);
```

目标 JS：

```js
app.directive("focus", applyFocus);
```

registry 中同样允许 function shorthand：

```csharp
Directives = new Vue3.VueDirectiveRegistry
{
    { "focus", ApplyFocus }
}
```

目标 JS：

```js
directives: { focus: applyFocus }
```

### 11.3 Binding bag

| C# | JS |
|----|----|
| `binding.Value` | `binding.value` |
| `binding.OldValue` | `binding.oldValue` |
| `binding.Arg` | `binding.arg` |
| `binding.Modifiers` | `binding.modifiers` |
| `binding.Modifiers["primary"]` | `binding.modifiers["primary"]` |
| `binding.Instance` | `binding.instance` |
| `binding.Dir` | `binding.dir` |

typed binding `VueDirectiveBinding<TValue>` / `VueDirectiveUpdateBinding<TValue>` 只提供 C# 强类型读取，不生成 runtime conversion。

`VueDirectiveValue` 是 directive retrieval 的 union bridge，表示 object-form directive 或 function shorthand。未来 C# union 可替代它。

## 12. Plugin 映射

### 12.1 Object-form plugin

`VuePlugin` / `VuePlugin<TOptions>` lower 成 plugin object：

```csharp
new Vue3.VuePlugin
{
    Install = Install
}
```

目标 JS：

```js
{ install: install }
```

typed plugin 只改变 install callback 和 options 的 C# 类型，不改变 JS shape。

### 12.2 Function-form plugin

`VuePluginInstallCallback` / `VuePluginInstallCallback<TOptions>` 直接作为 plugin：

```csharp
app.Use(InstallFeature, new FeatureOptions { Enabled = true });
```

目标 JS：

```js
app.use(installFeature, { enabled: true });
```

### 12.3 Plugin options

`VuePluginOptions` 是 string-key record/dictionary object：

- typed plugin options 可以继承并声明强类型属性。
- arbitrary options 用 indexer 写最终 key。
- `null` 静态值按通用 record 规则省略。
- 不生成 plugin options runtime class。

## 13. Reactivity 与 Lifecycle 映射

这些 API 应只依赖基础 binding，不需要 Vue 专用 compiler logic。

| C# | JS |
|----|----|
| `Reactive(value)` | `reactive(value)` |
| `ShallowReactive(value)` | `shallowReactive(value)` |
| `Readonly(value)` | `readonly(value)` |
| `ShallowReadonly(value)` | `shallowReadonly(value)` |
| `Ref(value)` | `ref(value)` |
| `ShallowRef(value)` | `shallowRef(value)` |
| `TriggerRef(value)` | `triggerRef(value)` |
| `IsRef(value)` | `isRef(value)` |
| `Unref(value)` | `unref(value)` |
| `ToRef(value)` | `toRef(value)` |
| `ToRef(existingRef)` | `toRef(existingRef)` |
| `ToRef(getter)` | `toRef(getter)` |
| `ToRef(source, key)` | `toRef(source, key)` |
| `ToRef(source, key, defaultValue)` | `toRef(source, key, defaultValue)` |
| `ToRefs(source)` | `toRefs(source)` |
| `ToRefs<TRefs, TSource>(source)` | `toRefs(source)` |
| `IsProxy(value)` | `isProxy(value)` |
| `IsReactive(value)` | `isReactive(value)` |
| `IsReadonly(value)` | `isReadonly(value)` |
| `ToRaw(value)` | `toRaw(value)` |
| `MarkRaw(value)` | `markRaw(value)` |
| `Computed(getter)` | `computed(getter)` |
| `Computed(options)` | `computed({ get, set })` |
| `CustomRef(factory)` | `customRef(factory)` |
| `Watch(source, callback)` | `watch(source, callback)` |
| `Watch(source, callback, options)` | `watch(source, callback, options)` |
| `Watch(refSources, callback)` | `watch([refA, refB], callback)` |
| `Watch(readonlyRefSources, callback)` | `watch([computedA, computedB], callback)` |
| `Watch(getterSources, callback)` | `watch([getterA, getterB], callback)` |
| `WatchEffect(effect)` | `watchEffect(effect)` |
| `WatchEffect(effect, options)` | `watchEffect(effect, options)` |
| `WatchPostEffect(effect)` | `watchPostEffect(effect)` |
| `WatchSyncEffect(effect)` | `watchSyncEffect(effect)` |
| `OnWatcherCleanup(callback)` | `onWatcherCleanup(callback)` |
| `OnWatcherCleanup(callback, failSilently)` | `onWatcherCleanup(callback, failSilently)` |
| `ToValue(value)` | `toValue(value)` |
| `NextTick()` | `nextTick()` |
| `NextTick(callback)` | `nextTick(callback)` |
| `UseAttrs()` | `useAttrs()` |
| `UseSlots()` | `useSlots()` |
| `UseTemplateRef<TElement>(key)` | `useTemplateRef(key)` |
| `UseId()` | `useId()` |
| `Provide(key, value)` | `provide(key, value)` |
| `Provide(VueInjectionKey<T>, value)` | `provide(key, value)` |
| `Inject(key)` | `inject(key)` |
| `Inject(key, defaultValue)` | `inject(key, defaultValue)` |
| `Inject(key, defaultFactory, true)` | `inject(key, defaultFactory, true)` |
| `Inject(VueInjectionKey<T>, ...)` | `inject(key, ...)` |
| `HasInjectionContext()` | `hasInjectionContext()` |
| `EffectScope(detached)` | `effectScope(detached)` |
| `GetCurrentScope()` | `getCurrentScope()` |
| `OnScopeDispose(callback)` | `onScopeDispose(callback)` |
| `OnScopeDispose(callback, failSilently)` | `onScopeDispose(callback, failSilently)` |
| `Version` | `version` |
| `OnMounted(callback)` | `onMounted(callback)` |
| `OnBeforeMount(callback)` | `onBeforeMount(callback)` |
| `OnUnmounted(callback)` | `onUnmounted(callback)` |
| `OnBeforeUnmount(callback)` | `onBeforeUnmount(callback)` |
| `OnUpdated(callback)` | `onUpdated(callback)` |
| `OnBeforeUpdate(callback)` | `onBeforeUpdate(callback)` |
| `OnErrorCaptured(handler)` | `onErrorCaptured(handler)` |
| `OnActivated(callback)` | `onActivated(callback)` |
| `OnDeactivated(callback)` | `onDeactivated(callback)` |
| `OnRenderTracked(callback)` | `onRenderTracked(callback)` |
| `OnRenderTriggered(callback)` | `onRenderTriggered(callback)` |
| `OnServerPrefetch(callback)` | `onServerPrefetch(callback)` |
| `refValue.Value` | `refValue.value` |

设计约束：

- `IVueRef<T>` / `VueReadonlyRef<T>` 是 runtime ref object 投影。
- `VueWatchHandle` 是 runtime watch handle 投影，C# 显式提供 `Stop()` / `Pause()` / `Resume()`，分别映射到 `stop()` / `pause()` / `resume()`。
- `VueWatchOptions` / `VueWatchEffectOptions` 是 plain object options；`Flush` 使用 ECMAScript enum string literal 映射到 `"pre"` / `"post"` / `"sync"`。
- `Watch` source 覆盖 getter、writable ref、readonly ref、reactive object；reactive object source 使用 `where TSource : class` 表达，不接受 primitive plain value。
- multi-source watch 覆盖同类型 `IVueRef<T>[]`、`VueReadonlyRef<T>[]`、`Func<T>[]`；callback 使用 `VueWatchSourcesCallback<T>` / `VueWatchSourcesCleanupCallback<T>` 接收 new/old value arrays。
- 异构 multi-source watch 不使用 public `object`，也不依赖 C# 不支持的接口隐式转换；需要异构强类型 payload 时，用 getter source 返回 typed record / tuple projection。
- `OnTrack` / `OnTrigger` 使用 `VueDebuggerCallback`，事件对象为 `VueDebuggerEvent`，unknown-like 值成员使用 `VueValue`。
- `OnErrorCaptured` 提供 void handler 与 bool callback 两条 overload；返回 `false` 的语义由 Vue runtime 处理。
- `OnServerPrefetch` 同时接受显式 `IPromise` callback 与 compiler-lowered `PromiseResult` callback。
- `VueCustomRefFactory<T>` 接收 `track` / `trigger` 两个 `Action`，返回 `VueCustomRefHandlers<T>`，其 `Get` / `Set` 直接降成 `{ get, set }`。
- `NextTick()` 与 `NextTick(Action)` 都返回 `PromiseResult`，分别对应 Vue promise form 与 callback form。
- `UseAttrs()` / `UseSlots()` 复用 `VueAttributeBag` / `VueSlotBag`，与 `setup(context)` 的 read-side bag 类型保持一致。
- `UseAttrs<TAttrs>() where TAttrs : VueProps` / `UseSlots<TSlots>() where TSlots : VueSlots` 是静态 typed projection，运行时仍分别调用 `useAttrs()` / `useSlots()`，不生成类型转换或包装对象。
- `UseTemplateRef<TElement>(key)` 返回 `VueReadonlyRef<TElement?>`，表达挂载前/卸载后可能为 `null` 的 Vue template ref 生命周期。
- `UseId()` 返回 string，用于 SSR-safe 的 app-local unique id。
- `UseModel<TValue>(props,key)` / `UseModel<TValue>(props,key,options)` 映射到底层 `useModel(props,key[,options])` helper；调用者必须显式声明对应 prop 与 `update:*` emit，避免 compiler 自动发明 v-model 协议。
- `UseModel<TProps,TValue>(props,model)` / `UseModel<TProps,TValue>(props,model,options)` 复用同一底层 helper，但把 prop key 收敛到 `VueModelName<TProps,TValue>` typed contract。
- `VueModelOptions<TValue>` 覆盖 get/set transform。
- `UseModel(...)` 返回 `VueModelRef<TValue>`：`.Value` 继续映射到 `model.value`，`GetModifiers()` / `GetModifiers<TModifiers>()` 通过 `ECMAScriptInline("__arg1[1]")` 读取官方 tuple-like modifiers bag，而不新增 compiler 特路。
- `ModelName<TProps,TValue>()` / `ModelName<TProps,TValue>(string key)` 分别覆盖 default model (`modelValue`) 与 named model；`ModelPropName(model)` / `ModelUpdateEventName(model)` 用于把同一 typed contract 复用到 `Props` / `Emits` 声明，减少重复字符串。
- `VueSetupContext.Emit(model, value)` 通过 instance-inline helper 映射到 `context.emit(\`update:${model}\`, value)`，把同一 `VueModelName<TProps,TValue>` contract 继续复用到 typed update emit。
- `ToRef` 使用 overload 区分 value/ref/getter normalization 与 object property ref；字符串 key 必须是最终 runtime key，C# 不从 string key 反推属性类型。
- `ToRef<TValue>(VueDictionary<TValue>, string)` 覆盖字典对象 key path；强类型 record path 使用 `ToRef<TSource,TValue>(source,key)` 明确 value contract。
- `ToRefs(source)` 返回 `VueRefs<TSource>`，提供 `IVueRef<VueValue>? this[string key]` 的兜底读取面。
- 用户需要 IntelliSense 时声明 `abstract class MyRefs : VueRefs<MyState>` 并用 `ToRefs<MyRefs, MyState>(state)`；这只是静态 projection，JS 仍是同一个 `toRefs(state)` 调用。
- `toRefs()` 只为调用时可枚举属性生成 refs；需要为可能缺失的属性建立 ref 时，应使用 `ToRef(source,key,defaultValue)` 或 `ToRef(source,key)`。
- `VueInjectionKey<T>` 是 Symbol 的泛型 phantom contract，通过 `Symbol -> VueInjectionKey<T>` 隐式转换保留 JS runtime key，同时约束 `Provide` / `Inject` 的值类型。
- `.Value` 只做 `value` member remap，不做自动 unwrapping。
- `Reactive<T>` / `Readonly<T>` 的 `where T : class` 表达 Vue object proxy 输入，不生成 runtime type guard。
- `IsRef<T>`、`IsVNode<T>` 这类 runtime predicate 使用泛型承接任意静态输入，避免 public API 暴露 `object`。
- mixed-source watch array 已通过 `VueWatchEntry<T>[]` 覆盖；调用端需显式写出数组元素目标类型，避免隐式退化为 `object[]`。

## 14. Async Component 映射

| C# authoring | JS output |
|--------------|-----------|
| `DefineAsyncComponent(loader)` | `defineAsyncComponent(loader)` |
| `DefineAsyncComponent(options)` | `defineAsyncComponent(options)` |
| `VueAsyncComponentOptions.Loader` | `loader` |
| `VueAsyncComponentOptions.LoadingComponent` | `loadingComponent` |
| `VueAsyncComponentOptions.ErrorComponent` | `errorComponent` |
| `VueAsyncComponentOptions.Delay` | `delay` |
| `VueAsyncComponentOptions.Timeout` | `timeout` |
| `VueAsyncComponentOptions.Suspensible` | `suspensible` |
| `VueAsyncComponentOptions.OnError` | `onError` |

设计约束：

- 非泛型 `VueAsyncComponentLoader` / `VueAsyncComponentOptions` 返回 `IVueComponent`，对应 Vue 官方 loader / options 核心形态，并保证 `DefineAsyncComponent(Load)` 方法组调用不二义。
- 泛型 `VueAsyncComponentLoader<TComponent>` / `VueAsyncComponentOptions<TComponent>` 保留 `IVueComponent<TProps>`、`IVueSlotComponent<TSlots>`、`IVueComponent<TProps,TSlots>` 等 typed component contract；typed async component 通过 options form 进入，避免 direct-loader 重载与非泛型 loader 争抢方法组绑定。
- loader 使用 `IPromise<TComponent>` 表达 JS Promise，不引入 compiler 特路；后续如果动态 import 有更完整 host surface，可只替换 loader 实现，不改变 `defineAsyncComponent` contract。

## 15. Custom Elements 映射

| C# authoring | JS output |
|--------------|-----------|
| `DefineCustomElement(options)` | `defineCustomElement(options)` |
| `DefineCustomElement(options, customElementOptions)` | `defineCustomElement(options, customElementOptions)` |
| `VueCustomElementComponentOptions*`（单参数 merged options） | `defineCustomElement({ ...componentOptions, ...customElementOptions })` |
| `VueCustomElementOptions.Styles` | `styles` |
| `VueCustomElementOptions.ConfigureApp` | `configureApp` |
| `VueCustomElementOptions.ShadowRoot` | `shadowRoot` |
| `VueCustomElementOptions.ShadowRootOptions` | `shadowRootOptions` |
| `VueCustomElementOptions.Nonce` | `nonce` |
| `UseHost()` | `useHost()` |
| `UseHost<THost>()` | `useHost()`（typed projection） |
| `UseShadowRoot()` | `useShadowRoot()` |

设计约束：

- `DefineCustomElement` 返回 WebIDL `CustomElementConstructor`，可直接进入 `CustomElementRegistry.Define(...)`。
- component authoring 仍复用 `VueComponentDefinition` / `VueComponentOptions<T...>`，不复制一套 Vue component options。
- custom-element-only 配置既可以通过第二参数 `VueCustomElementOptions` 承接，也可以通过单参数 merged options（`VueCustomElementComponentOptions*`）承接。
- `ConfigureApp` 使用 `VueCustomElementConfigureAppCallback(VueApp app)`，继续复用现有 `VueApp` surface。
- `ShadowRootOptions` 直接复用 WebIDL `ShadowRootInit`，不为 Vue custom elements 复制 DOM 标准类型。
- `UseHost()` 当前返回 `HTMLElement?`，这是 `VueElement | null` 在现有 WebIDL surface 下的稳定 DOM 上界；不臆造 VueElement runtime 类型。
- `UseHost<THost>()` 是 typed projection，保持 runtime 调用形态不变（仍是 `useHost()`）。
- `UseShadowRoot()` 返回 `ShadowRoot?`，表达 `shadowRoot: false` 或非 custom-element setup context 下可能为 `null`。
- CE props/events/light DOM 策略：props/emits 继续复用 component options 的 `Props` / `Emits`；light DOM 通过 `ShadowRoot = false` 显式表达。

## 16. Registry 映射

| Registry | 用途 | JS key source |
|----------|------|---------------|
| `VueComponentRegistry` | component option `components` | final component registration name |
| `VueDirectiveRegistry` | component option `directives` | final directive registration name |
| `VueSlots` | slots object | final slot name |
| `VuePluginOptions` | plugin options | final option key |
| `VueDictionary<T>` | generic object bag | final property key |

统一规则：

- string indexer / `Add(string, value)` lower 成 object property；`Symbol` indexer / `Add(Symbol, value)` lower 成 computed property。
- key 是最终 JS key，不做框架专属命名加工。
- collection initializer 只是一种 C# authoring 便利，不能生成 runtime `Add(...)` 调用。

## 17. Diagnostics

必须诊断而不是静默生成错误 JS 的场景：

| 场景 | 诊断要求 |
|------|----------|
| `[Spread]` 标在非 record 实例属性、static、indexer 上 | analyzer error |
| `[Spread]` 与显式 JS property name 同时存在 | analyzer error |
| `[Props]` 目标不是 `string[]` | compiler error |
| `[Props]` 无法解析 generic source type | compiler error |
| `[Emits]` 目标不是 `string[]` | compiler error |
| `[Emits]` 无法分析 setup callback | compiler error，并提示显式 `Emits` |
| `[Emits]` 遇到非字面量 event name | compiler error，并提示显式 `Emits` |
| object literal key 无法稳定翻译 | compiler error |
| typed default slot sugar 缺少 default slot | compiler error |
| typed default slot sugar default slot 重复 | compiler error |
| typed default slot sugar default slot 类型不是返回 host `IVNode` 的 delegate | compiler error |
| scoped default slot 被 direct child sugar 满足 | compiler error |
| unsupported external member 被调用 | compiler usage-site error |

诊断信息应命名源 C# symbol、目标 contract 和可行替代写法。

## 18. 当前实现基线

| 区域 | 当前状态 | 目标 |
|------|----------|------|
| `ChildrenToSlotIntrinsic` | 已从 `SemanticWalker.cs.Vue.cs` 迁出，基于 imported `h` 和同宿主 slot contract 识别 | 继续保持为最小 intrinsic；后续不要回到 Vue 命名空间特判 |
| default slot sugar | literal child 直接生成值对象；其他 child 使用 single-evaluation IIFE；component/props/child 单次求值与顺序已由回归锁定 | 后续可继续扩大静态安全表达式集合，但不得改变求值时机、次数或变量快照语义 |
| `VueAttributeBag` | indexer / class / style / id / title / for / name / type / placeholder / disabled / readonly / required / tabindex / role 读取面已落地 | callable listener bridge 已由 `VueAttributeListeners` / `VueAttributeListeners<TEvent>` 补齐；继续按真实需求扩展 |
| `VueSlotBag` | indexer / default 读取入口已落地 | scoped read-side helper 已由 `VueScopedSlots<TScope>` 作为 typed projection 补齐 |
| `H(...)` overload | 已按 element/component/props/slots/direct-child canonical 分类收敛（direct-child 使用 `IVNode` + `VueChild`） | 后续新增 API 继续复用现有分类，不再按示例组合膨胀 |
| `VueValue` callable/listener values | arbitrary dictionary key 仍主要用于普通属性值 | listener key 继续优先走 `VueEventHandlers` 或 `VueAttributeListeners*` callable bridge，避免依赖不存在的隐式转换 |
| object-literal host dynamic key 诊断 | 已覆盖 `VueObject` / `VueDictionary` / `VueDirectiveRegistry` / `VuePluginOptions` / `VuePropRegistry` / `VueEmitRegistry` / `VueComputedRegistry` / `VueMethodRegistry` / `VueWatchRegistry` 的 indexer / `Add(...)` 路线；string literal 始终允许，symbol-key 只在 host 显式声明 `Symbol` key contract 时允许 | 后续新增 object-literal host 必须复用同一路径并补齐 string dynamic-key 负向回归；若新增 symbol-key host，还必须补齐 symbol-key 正向回归 |
| `VueObject` | 已走 record + spread + dictionary 路线 | 保持通用 lowering，不再加 Vue-only 规则 |
| directive / plugin | 基础 object/function form 已存在 | 补齐文档、测试和 typed retrieval 细节 |
| custom elements | 核心 runtime binding + merged options authoring 已覆盖 | 后续只需在真实业务场景下补充更多 typed convenience，不再依赖 compiler 特路 |

## 19. 测试矩阵

| 分类 | 必测场景 |
|------|----------|
| object lowering | typed props、`VueObject` common members、indexer 任意 key、static `null` omission、runtime null 保留 |
| spread | typed props spread、attrs spread、dataset final key、raw external object spread、spread ordering |
| component options | `DefineComponent` untyped、typed props、typed slots、typed props+slots、显式 array/object-form props/emits |
| `H` element | no props、props、direct child (`IVNode` + `VueChild`) |
| `H` component | props、slots、props+slots、direct child default slot sugar (`IVNode` + `VueChild`)、typed slot validation |
| setup context | `attrs` read、untyped slots read、typed slots read、emit overloads、expose record |
| app | `createApp` / `createSSRApp` root props、`mount`、`use`、`component`、`directive`、`provide` |
| directive | object-form、typed object-form、function shorthand、registry indexer、registry collection initializer、binding/modifiers reads |
| plugin | object-form、typed object-form、function-form、typed options、string-key options |
| custom elements | `defineCustomElement` component options、CE-only options、`useHost`、`useShadowRoot`、`CustomElementConstructor` 返回类型 |
| reactivity | ref `.Value` mapping、computed、watch、watchEffect、lifecycle imports |
| diagnostics | spread misuse、props/emits misuse、invalid default slot sugar、unsupported dynamic object key |

## 20. Non-goals

- 不引入 `.vue` SFC authoring。
- 不把 Vue template 指令语法搬进 compiler。
- 不为 `Dataset`、directive prefix、Vuetify props 增加 compiler prefix/format 魔法。
- 不把 `VueProps` 变成 Vue runtime prop validator object。
- 不为了更像手写 JS 牺牲 evaluation order、side-effect count 或 usage-site 诊断。
- 不让外部库通过新的 compiler-only 特性绕过 `ECMAScript.Vue3` 公共合同。

## 21. 参考

- [ECMAScript.Vue3 API 覆盖矩阵](./vue3-api-coverage-matrix.md)
- [ECMAScript.Vue3 模块映射规则](./vue3-module-mapping-rules.md)
- [src/ECMAScript.Vue3/Vue3.cs](../../../src/ECMAScript.Vue3/Vue3.cs)
- [src/ECMAScript/attribute/SpreadAttribute.cs](../../../src/ECMAScript/attribute/SpreadAttribute.cs)
- [src/Jazor.Compiler/core/SemanticWalker.cs.Creation.cs](../../../src/Jazor.Compiler/core/SemanticWalker.cs.Creation.cs)
- [src/Jazor.Compiler/core/ChildrenToSlotIntrinsic.cs](../../../src/Jazor.Compiler/core/ChildrenToSlotIntrinsic.cs)
- [src/Jazor.Compiler/ImplementationPrinciples.md](../../../src/Jazor.Compiler/ImplementationPrinciples.md)

