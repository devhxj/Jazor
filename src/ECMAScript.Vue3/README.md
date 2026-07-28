# ECMAScript.Vue3

`ECMAScript.Vue3` 是独立外部库项目，不属于 `ECMAScript` 平台内核模块。  
它作为“官方第一个外部库映射样例”，展示如何在不增加 compiler 框架白名单特路的前提下，通过 C# 类型系统与通用特性完成 Vue 3 绑定。

## Design Boundary

- 不依赖 `Jazor` 专属特性做外部库语义（除平台通用映射特性外）。
- 不在 compiler 中硬编码 `ECMAScript.Vue3` 命名空间规则。
- 运行时映射通过通用 `[ECMAScript]` / `[Description]` / `[ECMAScriptInline]` 等机制表达。

## File Layout (Partial Class Pattern)

- `Vue3.cs`
  - 入口文件，仅保留 `Vue3` 的模块映射特性与顶层委托/handle 类型。
- `Api/Vue3.Api.cs`
  - App/Component/CustomElement/Builtin Component/VNode Utility 等核心 API。
- `Api/Vue3.Api.Render.cs`
  - `BindThis(...)` 与 `H(...)` overload 家族（渲染构建面）。
- `Api/Vue3.Api.Reactivity.cs`
  - `reactive/ref/computed/watch/...` 响应式 API。
- `Api/Vue3.Api.Composition.cs`
  - `useAttrs/useSlots/useTemplateRef/useModel/useHost/...` 组合式 API。
- `Api/Vue3.Api.Lifecycle.cs`
  - `onMounted/onUpdated/onErrorCaptured/...` 生命周期与 scope/hook API。
- `Types/Vue3.Types.*.cs`
  - `Vue3` 嵌套类型分组（Core/Props/Component/Directive/PluginApp/Structural/ReactivityOptions）。

该拆分策略的目标是：

- 保持 API 可读性与发现性；
- 控制单文件复杂度；
- 为后续外部库（如状态库、路由库、UI 库）提供可复制模板。

## Mapping Rules (Summary)

- C# 类型系统优先：`record`/`generic`/`overload`/`nullable`/`delegate`。
- 名称映射默认遵循“Vue 官方 JS 名只做 C# 大小写投影”：
  - `createApp` -> `CreateApp`
  - `createSSRApp` -> `CreateSSRApp`
  - `defineComponent` -> `DefineComponent`
  - `watchEffect` -> `WatchEffect`
  - `onMounted` -> `OnMounted`
- 命名 native `union` 类型用于真实 union 桥接；方法边界优先 overload 体验。只有 native union 无法保留精确 tagged projection 语义时，才使用 `[System.Runtime.CompilerServices.Union]` + `IUnion` fallback。
- authoring 默认规则是“能直接赋值就直接赋值”：
  - 如果调用点已经持有强类型委托变量、类实例变量、typed record、或可直接命中的 union 分支值，应直接赋给目标宿主成员或参数；
  - 只有在 C# 语言本身无法把作者想写的字面量 / lambda / interface-typed source 形式直接绑定到目标 contract 时，才补显式 helper、命名 union bridge 或 collection-initializer `Add(...)` 入口；
  - 不要为了统一表面写法，把本来可以自然直赋的场景再机械包一层 helper。
- props/object 字面量遵循通用 record lowering，不额外引入 Vue 专用 compiler 特路。
- `VueObject` 承载 Vue 核心 props 与一组高频原生 HTML convenience attrs；长尾属性继续通过 `Attrs` / indexer / typed props bag 表达。
- `UseModel(...)` 返回 `VueModelRef<TValue>`，`.Value` 对应 `model.value`，modifiers 通过 `GetModifiers()` / `GetModifiers<T>()` 读取；named model 可通过 `VueModelName<TProps,TValue>` + `ModelName/ModelPropName/ModelUpdateEventName` helper 复用同一 typed contract，`setup(context)` 侧可直接 `context.Emit(model, value)` 发出对应 `update:*` 事件。
- Options API object-form inject 可通过 `VueInjectOptions<T>` / `VueInjectEntry<T>` / `VueInjectRegistry<T>` 表达；`VueDictionary` 现已支持 string / `Symbol` key object authoring，不新增 compiler 特路。
- `[Spread]` 等语法糖由通用属性机制驱动，不绑定 Vue 命名空间。

## Authoring Contract

- `IVueComponent` / `IVueLibraryComponent` 的 canonical 类型定义在 `ECMAScript.Vue3`。
- `ECMAScript.VueContract` 只承载 RazorVue library-mode 相关的 attribute / descriptor contract，例如 `VueLibraryComponentAttribute`、`VuePropKind`、`VueEmitKind`、`VueComponentFlags`。
- RazorVue authoring 走显式按需导入，不通过 `Jazor` NuGet 包对 consumer 做全局 marker 注入。
- 如果组件作者要直接使用 `IVueComponent` / `IVueLibraryComponent` 简名，应该在文件或共享 global usings 中显式添加：

```csharp
using static ECMAScript.Vue3;
```

- 如果 authoring 需要完整 Vue 3 API surface，例如 `H(...)`、`DefineComponent(...)`、`Ref(...)`、`Computed(...)`、`OnMounted(...)`，同样应显式 `using static ECMAScript.Vue3;` 或显式使用 `ECMAScript.Vue3` 成员。

### Naming Policy

- 对应 Vue 官方公开 API 的绑定名，默认只允许大小写变化，不额外改词、不重排缩写。
- 只有在 C# 关键字冲突、宿主已有同名核心类型/成员、或语义会明显误导时，才允许引入 helper 名、前后缀或包装类型。
- 引入偏离官方名的 helper 时，必须满足两点：
  - 如果该 surface 仍然直接对应某个 Vue runtime 成员，则 runtime 映射通过 `Description("@#...")` 直接指向原始 Vue API 名；
  - README 或测试中要明确说明偏离原因，例如语义 helper、typed contract helper，或命名冲突规避。
- 命名冲突的处理优先级：
  - 先保留原始词根；
  - 再只追加最小区分后缀/前缀；
  - 除非必要，不发明新的替代词。

### Allowed Deviations

- compile-time / inline helper：
  - `BindThis(...)`
  - `ModelName(...)`
  - `ModelPropName(...)`
  - `ModelUpdateEventName(...)`
  - 这些 API 只服务于 C# authoring 或 inline lowering，不是 Vue 官方公开 runtime API，因此不声明 `Description("@#...")` 直映射。
- C# options authoring surface：
  - `ProvideFactory`
  - `Props`
  - `Emits`
  - `provide` 因为与 object-form / function-form 共用同一个 runtime key，仍保留 `Provide` / `ProvideFactory` 的最小语义分流；`props` / `emits` / Options API `inject` 则通过 `VueNamesOrOptions` bridge 收口到 canonical 成员，同时保留 `Props = ["title"]` / `Emits = ["save"]` / `Inject = ["feature"]` 这类 collection-expression authoring。
- type / delegate naming：
  - `VueDirectiveSSRPropsCallback`
  - 这类类型名不直接等于 Vue JS 标识符，但其成员侧仍应保持官方 API 词根和缩写，例如 `getSSRProps` -> `GetSSRProps`。
- canonical options authoring：
  - `Props` / `Emits` / `Inject` 通过 `VueNamesOrOptions` 同时覆盖 array-form 与 object-form；
  - `Props = ["title"]` / `Emits = ["save"]` / `Inject = ["feature"]` 仍保持 collection-expression authoring 体验；
  - 一旦 public surface 直接对应 Vue 官方 API，就只保留 canonical 名称，不继续维护旧命名兼容层。

## Delivery Boundary

1. `ECMAScript.Vue3` 负责 Vue 3 host binding 与 `H(...)` authoring contract。
2. `Jazor.Vue` 负责显式启用 Razor-to-Vue Hook；RazorVue 负责组件绑定与 render-function lowering。
3. `Jazor.Emit` 负责 `.mjs`、SourceMap、manifest 与 production Bundle 的物化。
4. 本项目不负责 Razor 语法解析、Razor SG Hook、SFC 编译、开发服务器或浏览器 Bundle。
