# ECMAScript

> Status: active reference
> Positioning: core JavaScript host projection assembly for Jazor module authoring.

`ECMAScript` 提供 Jazor 的基础 JavaScript 运行时投影，包括：

- 全局宿主表面（`Global.cs`）
- Vue 运行时 authoring surface（`Vue3.cs`）
- `Either<...>`、DOM、Web API 等核心契约

## Responsibilities

- 把 JavaScript / Web API 表面以稳定的 C# host contract 形式暴露出来。
- 保持 `ECMAScript("npm:...")` / `ECMAScriptModule("...")` 上声明的包 specifier 与模块路径原样进入 emitted JS。
- 为上层模块 authoring 提供 Vue `createApp` / `defineComponent` / `h` / `ref` / `computed` 等编译时可投影 API。

## Vue Authoring Surface

`Vue3.cs` 当前提供两条组件 authoring 通道：

1. `VueComponentOptions`
   - 面向无 props 的简单组件。
   - 支持 `Render` 或无参 `Setup`。

2. `VueComponentOptions<TProps>`
   - 面向 typed props 的组件。
   - `TProps` 主要用于 C# 的 `Setup` / `H(...)` 强类型 authoring，不自动发明 Vue runtime `props` 选项。
   - 运行时 `props` 合同需要显式写 `PropNames`（array-form）或 `PropOptions`（object-form validators/defaults）。
   - 运行时 `emits` 合同需要显式写 `EmitNames`（array-form）或 `EmitOptions`（object-form validators）。
   - `Setup` 形状为 `VueTypedSetupCallback<TProps>`，对应 Vue 的 `setup(props, context)`.

3. `VueComponentOptions<TProps, TSlots>`
   - 面向同时需要 typed props 和 typed slots 的组件。
   - `Setup` 形状为 `VueTypedSetupCallback<TProps, TSlots>`。
   - `context` 形状为 `VueSetupContext<TSlots>`，`context.Slots` 会以 `TSlots` 的 authoring contract 暴露出来。
   - `DefineComponent(...)` 返回 `IVueComponent<TProps, TSlots>`，`H(component, props, slots)` 可把读写两侧合同接起来。

4. `VueSlotComponentOptions<TSlots>`
   - 面向无 props、但需要 typed slots 的组件。
   - `Setup` 形状为 `VueTypedSlotSetupCallback<TSlots>`。
   - `DefineComponent(...)` 返回 `IVueSlotComponent<TSlots>`。
   - `H(component, slots)` 可直接走 typed slot-only authoring 路径，不需要占位 props 类型。

`PropOptions` / `EmitOptions` 是 object-form Vue runtime declaration surface：

- `VuePropOptions<TValue>` 覆盖 `type`、`required`、`default`、`validator`，支持 `VuePropType.String` / `Number` / `Boolean` 等构造器值；
- `VuePropRegistry<TValue>` 适合同一 value contract 的 string-key prop registry，异构 props 推荐声明自定义 `VueProps` record；
- `VueEmitRegistry` 与 `VueEmitRegistry<T0...T3>` 覆盖 0 到 4 个 payload 的 validator object-form；
- `PropNames` / `EmitNames` 仍用于简单 array-form 声明；
- 同一个 options object 上按约定只使用 `PropOptions` 或 `PropNames` 之一、`EmitOptions` 或 `EmitNames` 之一。

`ECMAScript.Contract.PropsAttribute` / `ECMAScript.Contract.EmitsAttribute` 仍是 compiler 的基础绑定推导原语，但不作为外部库 authoring surface，也不再标注在 `VueComponentOptions*` 上。Vue3 public API 优先通过显式 record 成员、overload、generic、delegate 和 `Description("@#...")` 表达最终 JS，避免把历史推导行为伪装成新设计。

`VueSetupContext` 当前暴露：

- `Emit(...)`
- `Expose(...)`
- `Attrs`
- `Slots`

`components` / `directives` 这两类注册表当前也走“直接可用 + 可继承扩展”的双轨 surface：

- `VueComponentRegistry` 可直接写成 `new VueComponentRegistry { ["ChildView"] = Child }`；
- `VueDirectiveRegistry` 可直接写成 `new VueDirectiveRegistry { ["Ripple"] = Ripple }`，本地 function shorthand 也可写成 `new VueDirectiveRegistry { { "Focus", ApplyFocus } }`；
- 外部库仍可继承它们暴露更强的属性面，例如 `VuetifyComponentRegistry` / `VuetifyDirectiveRegistry`；
- 因此常见场景下不再需要为了注册几个组件或指令额外定义一个自定义 registry record。

`VuePluginOptions` 也采用同样思路：

- `VuePluginOptions` 可直接写成 `new VuePluginOptions { ["feature"] = true }`；
- 外部库仍可继承它暴露更强的 typed options surface，例如 `VuetifyOptions`；
- 这让简单插件配置不必先定义一层专用 options record。

`VuePlugin` 现在也补齐了 authoring surface，并且同时覆盖 Vue 原生的 object-form / function-form 两条使用路径：

- object-form plugin 可直接写成 `new VuePlugin { Install = InstallPlugin }`；
- typed object-form plugin 可直接写成 `new VuePlugin<MyPluginOptions> { Install = InstallPlugin }`；
- function-form plugin 可直接写成 `app.Use(InstallPlugin)`；
- typed function-form plugin 可直接写成 `app.Use<MyPluginOptions>(InstallPlugin, options)`；
- 现有 `VuePlugin` + `VuePluginOptions` fallback 仍保留，便于接住弱类型或外部导入 plugin 值；
- 外部库若需要 install-time typed options，也可以继承 `VuePlugin<TOptions>` 暴露更强的 surface；
- 像 `VuetifyPlugin` 这类“创建时即已消费 options”的 imported plugin，则仍可直接继承非泛型 `VuePlugin`。

`VueDirective` 当前也已经切到 direct object-form surface：

- object-form directive 可直接写成 `new VueDirective { Mounted = OnMounted }`；
- typed object-form directive 可直接写成 `new VueDirective<string> { Mounted = ApplyColor }`；
- `VueDirectiveBinding<TValue>` / `VueDirectiveUpdateBinding<TValue>` 会把 `binding.Value` / `binding.OldValue` 收敛到强类型；
- `VueDirectiveRegistry` 既可承载 object-form directive，也支持 collection-initializer 形式的本地 function shorthand，例如 `{ { "Focus", ApplyFocus } }`；
- typed object-form directive 仍可直接写成 `["Colorize"] = new VueDirective<string> { Mounted = ApplyColor }`；
- typed function shorthand 在 `VueDirectiveRegistry` 的 collection initializer 里如果直接写 method group，会受 C# 自身 overload-resolution 限制；这时写成 `(VueDirectiveFunction<string>)ApplyColor`，或先落到一个已类型化 delegate 变量，再放进 registry 即可；
- `VueDirectiveModifiers` 提供 `binding.Modifiers["primary"]` 这类直接修饰符读取；
- `app.Directive("name", ...)` 保留非泛型 surface，同时补了 `app.Directive<TValue>("name", directive)` 的 typed overload；
- `app.Directive("name", handler)` 现在支持 Vue 原生 function shorthand；
- typed function shorthand 也可用，但在 method-group 场景下一般写成 `app.Directive<string>("name", ApplyColor)` 更稳定；
- `app.Directive("name")` 的读取侧返回 `VueDirectiveValue`，用于承载“object-form directive 或 function shorthand”这层真实 Vue union contract；
- 外部库导入指令（如 `VuetifyDirective`）继续兼容这一基类 surface。

`H(...)` 的 component authoring 现在还支持显式 slot object：

- `H(tagOrComponent, singleVNodeChild)` 可直接表达单个 vnode child，不必再手动包 `IVNode[]`；
- `H(...)` 的常用 children authoring 已切到 overload-first：`string`、`Number`、`bool`、`IVNode`、`IVNode[]` 直接用 C# 重载表达，不再依赖 `Either<...>`；
- 当目标是 `component` 时，这些 child overload 都会按 default slot authoring 收敛；编译输出会保持 `component / props / child` 左到右、单次求值，再映射成标准 Vue slot 形状；
- 这层 default-slot sugar 不通过外部库侧 `[Jazor]` / `Op.Compile` 声明驱动，而是由编译器基于 imported `h` 与同宿主 component / props / slot 合同做内部识别；
- 因此识别边界不是 `ECMAScript.Vue3` 精确命名空间，而是稳定 host contract；外部基础绑定只要复用同样的 host 形状，也可以获得同一 default-slot lowering；
- 对 `string` / `number` / `bool` / `null` 等字面量 child，编译器可直接生成 `{ default: () => literal }`；对变量、属性读取、调用、数组/object 等可能改变求值时机或次数的 child，仍使用 IIFE 捕获值；
- 当目标是 `IVueComponent<TProps>` 时，implicit child/default-slot sugar 总是成立，因为它没有 typed slot contract 需要额外校验；
- 当目标是 `IVueSlotComponent<TSlots>` 或 `IVueComponent<TProps, TSlots>` 时，implicit child/default-slot sugar 只在 `TSlots` 暴露且仅暴露一个 parameterless default slot 时成立；缺少 default slot、声明了多个 default slot，或 default slot delegate 带参数时，必须回到显式 `H(component, slots)` / `H(component, props, slots)` authoring；
- typed default slot 通过 slot 属性最终映射名是否为 `default` 识别；推荐直接用 `Description("@#default")` 明确声明；
- `H(component, slots)`
- `H(component, props, slots)`
- 若 `component` 是 `IVueSlotComponent<TSlots>`，可直接使用 typed `H(component, slots)` overload；
- 若 `component` 是 `IVueComponent<TProps, TSlots>`，可直接使用 typed `H(component, props, slots)` overload；
- `slots` 使用 `VueSlots` record surface 建模；
- `VueSlots` 现在可直接写成 `new VueSlots { ["default"] = RenderBody }` 这类 parameterless slot bag；
- 若 slot 需要 scoped props，仍应定义 typed slot record，并使用带参数且返回 `IVNode` 的 delegate 显式建模；`VueSlotCallback<TScope>` 是推荐的内置 delegate，不是 compiler 唯一接受的类型名；
- 每个 slot 属性继续走 `Description("@#...")` 名称映射；
- slot 回调推荐使用 `VueSlotCallback` / `VueSlotCallback<TScope>`；外部基础 binding 也可使用自定义 delegate，只要返回同宿主 `IVNode`；
- 这层是“写侧” authoring contract，和 `VueSetupContext.Slots` 的“读侧”运行时 bag 保持分离。

这层合同的目标是让 emitted JS 维持标准 Vue 3 组件形状，而不是引入 Jazor 私有运行时包装。

### `VueObject` / `VueObject<TProps>`

除组件定义对象外，`Vue3.cs` 还提供了面向 `h(...)` props / root props authoring 的 record surface：

- `VueObject`
- `VueObject<TProps>`
- `VueDictionary`
- `VueValue`
- `VueKey`

它们本质上不是新的 runtime shape，而是普通 `record` structural lowering 的便捷 authoring contract：

- `VueObject` 继承自 `VueDictionary`，因此既保留 record -> plain JS object lowering，也可以直接写 `["key"] = value`；
- `Is` 覆盖 Vue customized built-in element 的 string `is` special attribute；动态组件直接使用 component-valued `H(...)` overload；
- `Key` 使用 `VueKey` 覆盖 Vue VNode `key` 的 string / number-like / `Symbol` contract；
- `Class`、`Style`、`Ref`、`Id`、`Title` 提供常用 Vue/DOM props 的强类型入口，其中 `Ref` 是 named template ref key；
- `Events` 通过 `[Spread]` 扁平化 `VueEventHandlers` / `VueEventHandlers<TEvent>`，事件 key 直接写最终 `onXxx` listener prop，避免 compiler 做 `Click -> onClick` 推断；
- `Attrs`、`Dataset`、`Raw` 通过 `[Spread]` 直接扁平化进当前对象；
- `VueObject<TProps>.Props` 也通过 `[Spread]` 扁平化 typed props bag；
- `VueDictionary` 提供常用的非泛型字典入口，`VueValue` 提供统一的字典值契约；
- `Dataset` 当前不提供额外 prefix/format 规则，属性名应直接映射到最终 `data-*` key，例如 `Description("@#data-user-id")`。

也就是说，`VueObject` 不是“编译器里的 Vue 特路”，而是建立在通用 record structural lowering 之上的 authoring sugar。

### `[Spread]` 语法糖

`ECMAScript.SpreadAttribute` 是通用 record property flattening 标记，不是 Vue 专属特性。

当前规则是：

- 仅对参与 structural lowering 的 record 实例属性生效；
- 若被标记成员 lower 后是 object literal，会内联展开其成员；
- 若不是 object literal，则生成标准 JS `...expr`；
- 不能与显式 JS 属性命名约定同时使用，因为两者语义自相矛盾；
- 这层能力的目标是“把一个 record 成员 flatten 到外层对象”，而不是引入新的 runtime 类型系统。

### record 的静态 `null` 省略

对 structural-lowered record，编译器会做一个刻意的静态优化：

- 若 record 主构造参数的实参能被 Roslyn 静态证明为 `null`，该成员不生成；
- 若 record object initializer 的赋值值能被 Roslyn 静态证明为 `null`，该成员不生成；
- 这条规则只针对静态可证明的 `null`，不会把一般运行时 `null` 检查扩展成隐式省略协议。

因此：

- 未赋值或静态 `null` 的 authoring 成员可以被干净省略；
- 非常量 `null` 流值仍会按普通值成员生成；
- 这条优化适用于通用 record structural lowering，不是 `VueObject` 专属行为。

## Boundaries

- `ECMAScript` 不承载 RazorVue 描述符提取、组件目录生成或 Roslyn 分析逻辑；这些能力位于 `Jazor.Common` / `Jazor.Analyzer`。
- `ECMAScript` 不负责产物物化；`.mjs` / source map / manifest 输出由 `Jazor.Emit` 负责。
- `ECMAScript` 不应引入与命名空间无关的额外依赖污染其公共 host surface。

## Key Files

- `Global.cs`: JavaScript 全局对象与基础函数投影。
- `Vue3.cs`: Vue 运行时 authoring 合同。
- `internal/Either.cs`: JS union-like host contract。

## Read Next

- [../Jazor.Common/README.md](../Jazor.Common/README.md)
- [../ECMAScript.Vuetify/README.md](../ECMAScript.Vuetify/README.md)
- [../../docs/01-目标/razorvue/README.md](../../docs/01-目标/razorvue/README.md)
