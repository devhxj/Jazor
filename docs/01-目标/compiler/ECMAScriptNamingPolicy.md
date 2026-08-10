# ECMAScript 名称解析规范

> Status: accepted and implemented (`v0.8.0`, 2026-08-10)
> Scope: `Jazor.Compiler`、`Jazor.RazorVue`、ECMAScript binding 声明与生成的 Vue render-function artifact。

## 1. 决策

Jazor 不再把 `PascalCase -> lowerCamelCase` 作为普通 C# 符号的默认 ECMAScript 命名规则。

默认情况下，用户声明的类型、方法、属性、字段和事件以其 C# 源名称参与 ECMAScript lowering。只有运行时 ABI 要求不同名称时，才在绑定该 Roslyn 符号的元数据上明确声明目标名称。RazorVue prop 也遵循同一规则，不保留独立的大小写推断。

这是一份已实施的合同。普通符号与 RazorVue 均不再保留自动 lower-camel fallback；绑定中的外部 ABI 差异已按成员显式声明。

## 2. 为什么采用显式映射

### 2.1 C# 是 authoring source of truth

Jazor 的输入是 C#，输出是由编译器生成的 ECMAScript artifact。对用户自己拥有的成员，源名称已经是唯一稳定、可由 Roslyn 绑定的名称。JavaScript 标识符允许 `PascalCase`，Vue render function 的 props 对象也会保留对象键的大小写，因此没有技术理由把每个普通成员再改写一次。

全局自动转小驼峰表面上节省了特性，但会额外制造一套隐式名称：作者读 C#、检查 emitted JS、定位 source map、阅读错误信息或跨模块排查时，都必须在脑中重复换算。这个成本发生在所有普通成员上，而不是只发生在真正的跨 ABI 边界。

### 2.2 外部 ABI 本来就必须精确描述

外部 JavaScript/Vue API 的真实名称并不只是大小写差异：`modelValue`、`$patch`、`aria-label`、`onUpdate:modelValue`、`class` 等都无法由通用 PascalCase 规则可靠恢复。即使保留自动转小驼峰，仍然需要为这些边界写映射。

把映射显式写在成员上，会增加有限的声明量，但名称差异是可见、可搜索、可审阅的合同，而不是调用者需要猜测的编译器行为。注解成本集中在 binding 作者和协议边界，而普通 C# authoring 不再承担持续的认知转换成本。

### 2.3 Vue 不要求所有 prop 使用小驼峰

Vue 生态通常把组件 prop 写成 lower camel case，但这是一种约定和外部组件 ABI，不是 Vue 对所有 prop 的强制语法要求。对于 Jazor 生成的 render function：

```js
h(MyComponent, { TitleText: "hello" })
```

只要 `MyComponent` 声明的运行时 prop 同样是 `TitleText`，该名称就可以正常工作。浏览器 HTML 属性大小写归一化的问题不适用于这里，因为 artifact 直接构造 JavaScript props 对象。

外部组件若声明 `modelValue`，则它的真实 ABI 仍必须严格使用 `modelValue`。这正是显式映射存在的原因，而不是恢复全局转换的理由。

## 3. 统一名称合同

### 3.1 普通成员的解析顺序

对普通 C# 声明，最终 ECMAScript 名称按以下顺序确定：

1. 有效的 `[ECMAScriptName("name")]`。
2. 有效的 `[Description("@#name")]`。
3. C# 源符号名称。
4. 编译器必须生成的稳定名字，例如自动属性 backing field 哈希、重载区分后缀和构造函数 helper 名称。

第 4 项不是命名风格转换。它只解决 JavaScript 语法或稳定唯一性要求，不能被用来重新引入大小写 fallback。

`Jazor(Op.Alias)` 仍是 CLR/宿主 member dispatch 的语义映射，按既有白名单消费顺序处理；它不等同于为用户声明重新选择默认命名策略。

### 3.2 `Description` 与 `ECMAScriptName` 的 authoring 约定

`DescriptionAttribute` 是 .NET 自带的通用特性。它尚未用于正常说明文本时，优先使用 `@#` 前缀承载 ECMAScript 名称：

```csharp
[Description("@#modelValue")]
public string? ModelValue { get; set; }
```

`@#` 用于把这类机器可消费的名称与普通的人类说明区分开。普通描述不参与命名：

```csharp
[Description("当前绑定值")]
public string? ModelValue { get; set; } // emitted name: ModelValue
```

`ECMAScriptName` 是不受 authoring 场景限制的专用名称特性，可以随时直接使用。当 `Description` 已经需要承载正常说明时，它尤其是必要的：

```csharp
[Description("当前绑定值")]
[ECMAScriptName("modelValue")]
public string? ModelValue { get; set; }
```

`ECMAScriptNameAttribute` 允许标注所有 C# attribute target，包括原生 `event`。它不是 `Description` 被占用后的受限 fallback；优先使用 `Description("@#...")` 只是为了在无需正常说明时复用 .NET 自带特性、减少专用注解，并不禁止作者直接选择 `ECMAScriptName`。

`ECMAScriptName` 是 compiler 消费的纯元数据，不标记为 browser-only API；应用该特性不会引入平台兼容性警告或运行时调用。

`AttributeTargets.All` 取消的是 C# authoring 限制，不会为原本没有 emitted-name 消费点的目标虚构 lowering 语义。特性只有在对应符号进入名称解析路径时才影响 artifact；assembly、return value 等目标若没有名称消费者，仅允许合法标注，不产生额外运行时行为。

解析冲突时，`ECMAScriptName` 优先于 `Description("@#...")`。这条优先级用于消除歧义，不改变日常 authoring 的推荐顺序。

`[Description("@#")]` 是名称解析边界，不表示空名称，也不能作为普通别名使用。它使当前符号及其外层宿主不再继续参与需要拼接的名称解析；这一语义必须保留。

空白名称特性不是有效 authoring 合同。迁移时应审计现有空白 `ECMAScriptName` 与 `Description` 组合的兼容行为，不要把偶然的实现细节当作新规则。

### 3.3 所有 consumer 必须共用解析结果

下列路径必须对同一个 Roslyn 符号得到同一个最终名称：

- `AstConverter` 的声明、导出和对象结构 key；
- `SemanticWalker` 的成员访问、赋值、调用和 import binding；
- RazorVue 对普通 `[Parameter]` prop 的 runtime key；
- record 的结构属性 key、模式匹配和 `with` lowering；
- 跨模块引用、冲突预留和 source-map 可观察的 emitted binding。

RazorVue 不得在通用解析结果之后，再私下执行 `PascalCase -> lowerCamelCase`。同样，模块级保留名收集不能为了旧 fallback 同时保留一个人为转换后的候选名称。

## 4. RazorVue 与 Vue 协议

### 4.1 普通 prop

普通 `[Parameter]` prop 的 runtime key 就是统一名称合同的结果：

```csharp
[Parameter]
public string? TitleText { get; set; }
// emitted prop key: TitleText

[Parameter]
[Description("@#modelValue")]
public string? ModelValue { get; set; }
// emitted prop key: modelValue
```

面向外部组件库时，binding 必须声明该组件实际要求的名称；面向 Jazor 自有组件时，不应仅为了模仿 JavaScript 风格而添加映射。

### 4.2 Vue 协议名称也是显式 ABI

Vue 的协议 key 不是 RazorVue 可以从 C# 成员名补全的第二套命名规则。RazorVue 只负责识别已绑定的
`[Parameter]`、`EventCallback` 与 `RenderFragment`，并将其放进正确的 `h(...)` 参数位置；它不负责选择
prop、listener 或 slot 的名称。

- `EventCallback` 的 listener property key 精确采用该成员的统一名称解析结果。不存在
  `ValueChanged -> onUpdate:value`、`OnClick -> onClick`、`OnSave -> onSave` 或由 raw emit 反推 listener 的规则。
- `RenderFragment` 的 slot key 精确采用该成员的统一名称解析结果。不存在
  `ChildContent` / `DefaultContent -> default`、`XContent -> x` 或 PascalCase 到 kebab-case 的规则。
- `EventCallback` 不会使 RazorVue 为当前组件生成 `defineComponent({ emits: ... })`。Jazor 的当前组件
  callback 直接调用 listener prop；外部组件的 raw Vue emit 仍由该组件库自身实现，Jazor binding 只声明
  调用方必须传入的 listener key，例如 `onUpdate:modelValue` 或 `onClick:close`。上游 raw emit 名可以保留在
  generator 的冻结 schema 中用于审计，但不构成 C# 特性或 RazorVue runtime 合同。
- 当前以固定字符串构造 `default` 的 imperative children-to-slot intrinsic 也属于本次审计范围。
  它必须改为消费显式 slot contract，或要求作者传入显式 slots object，不能保留隐藏的默认 slot 命名。

这不删除 `EventCallback` 或 `RenderFragment` 的结构语义。它只将名称选择完全交回 binding/组件作者，
使所有路径只消费已经解析的明确名称。

## 5. 适用与不适用范围

| 场景 | 默认行为 | 名称不同的处理 |
| --- | --- | --- |
| 用户声明的 C# 方法、属性、字段、事件 | 保留源名称 | 成员级 `Description("@#...")` 或不受场景限制的 `ECMAScriptName` |
| 用户声明的 RazorVue prop | 保留源名称 | 同上 |
| 自有 Vue 组件 | 声明端与调用端使用同一解析结果 | 仅在对外 ABI 需要时映射 |
| 外部 Vue/JS 组件 binding | 以实际 ABI 为准 | 显式映射每个不同名称 |
| CLR runtime host | 保持白名单 `Alias` / `Inline` / `Import` / `Compile` 语义 | 不用普通 fallback 代替宿主映射 |
| 编译器合成成员 | 使用确定性、无冲突的合成名 | 不套用用户命名风格规则 |

本规范不引入项目级或程序集级的“自动 lower camel”开关。此类开关会让同一符号的 artifact ABI 依赖不可见的构建环境，并使跨模块、包引用和缓存产物难以稳定推断。

## 6. 迁移计划

### 6.1 编译器

1. 在 `Util.GetConfigOrSymbolName` 中移除普通成员的 `ConvertPascalCaseIdentifierToJsNaming` fallback，保留显式名称、稳定 backing field 名和重载/构造函数的唯一性规则。
2. 删除或改写仅为旧 fallback 服务的保留名候选，例如同时预留 `localName` 与其小驼峰版本的逻辑。
3. 审计 `AstConverter`、`SemanticWalker` 与 module import/export 路径，保证它们只消费统一解析结果，不能局部再次改写名称。
4. 保持 whitelist key、`Op.Alias` 和 host dispatch 的既有语义，不把该迁移扩散为白名单持久化 key 重写。

### 6.2 RazorVue

1. `LibraryComponentConventions.GetPropRuntimeName` 直接消费统一解析结果；取消其任何独立 lower-camel 默认值。
2. 更新 parameter runtime-name map 的“默认名”比较逻辑，使未映射的 PascalCase 参数不被错误视为已转换的名称。
3. 审计 model update、listener、slot 与组件 framing 路径：最终 key 只能来自成员 metadata 或源名称；删除由 `Changed`、`OnX`、`Content`、大小写或 raw emit 反推名称的逻辑，并删除 `EventCallback` 自动 `emits` 输出。
4. 对外组件 binding 中原先依赖自动转换的 prop、listener、slot 全部补齐显式 metadata；若成员已有正常 `Description`，名称改用 `ECMAScriptName`。raw emit 只可作为上游 generator schema 的审计字段保留。

### 6.3 生成 binding 与兼容性

组件库 generated source 必须从其上游 API 元数据生成显式名称映射，不能手工批量修改生成文件。手写 binding 只为实际不同名称添加元数据。

此迁移会改变未映射成员的 emitted JavaScript 名称，因此属于 artifact ABI 变更。执行前必须枚举公共 module export、外部组件 prop、listener、slot 和跨模块调用；无法同时升级的 consumer 需要在旧名称上保留显式映射，而不是恢复全局 fallback。

## 7. 验收标准

- 未标注的 `TitleText`、`Execute`、`Total`、`ValueChanged` 分别发射为同名基准，而不是 `titleText`、`execute`、`total`、`valueChanged`。
- `[Description("@#modelValue")]` 精确发射 `modelValue`；普通 `Description("说明")` 不改变名称。
- 同时存在有效 `ECMAScriptName` 与 `Description("@#...")` 时，前者获胜。
- `[Description("@#")]` 仍只作为名称解析边界，不能生成空标识符或空属性 key。
- RazorVue 未映射 prop 与 C# 源名称一致；外部 `modelValue`、`class`、`onUpdate:modelValue` 等显式映射仍精确发射。
- RazorVue 不会从 `ChildContent`、`DefaultContent`、`ValueChanged`、`OnX`、`Content` 或任何 casing/kebab-case 规则选择名称。
- record、对象创建、成员访问、模块 export/import、派生参数和 source-map 相关路径对同一符号使用同一最终名称。
- 不增加全局命名开关、字符串拼接 fallback 或第二套 RazorVue 名称转换器。

## 8. 相关文档

- [SyntaxTransformationPipeline.md](./SyntaxTransformationPipeline.md) - compiler 主链路与名称消费点
- [ModuleConversionSpec.md](./ModuleConversionSpec.md) - 模块成员名称与导出规范
- [../razorvue/组件封装原则.md](../razorvue/组件封装原则.md) - 组件 binding 的 authoring 合同
- [../../../src/Jazor.Compiler/ImplementationPrinciples.md](../../../src/Jazor.Compiler/ImplementationPrinciples.md) - compiler 实现边界与稳定性原则
- [../../02-计划/compiler/ECMAScriptNamingMigrationPlan.md](../../02-计划/compiler/ECMAScriptNamingMigrationPlan.md) - 本合同的迁移执行清单与 Gate
