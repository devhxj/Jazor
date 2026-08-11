# ECMAScript 显式命名迁移实施计划

> Status: completed (2026-08-10)
>
> Scope: Jazor.Compiler、Jazor.RazorVue、ECMAScript binding、binding generator、回归测试与生成 artifact。
>
> Decision source: [ECMAScript 名称解析规范](../../01-目标/compiler/ECMAScriptNamingPolicy.md)。

## 0. 完成记录

本迁移已收口。普通 C# 符号不再隐式从 PascalCase 转为 lowerCamelCase；Vue prop、listener 与 slot
也不再由 RazorVue 按约定推断名称。外部 JavaScript ABI 由声明端逐成员表达，CLR host 则继续由其
白名单 member mapping 表达。

| 区域 | 实施结果 |
| --- | --- |
| Compiler | 删除普通符号大小写 fallback 及其冲突预留；声明、引用、导入、导出和 source map 统一消费同一已解析名称。 |
| RazorVue | 删除 `Changed`、`OnX`、`ChildContent`、kebab-case、raw emit 等名称推断；只消费成员 metadata 或源名称。 |
| Binding | Vue3、VueRoute、Pinia、Style、Element Plus、TDesign 与 Vuetify 的外部 ABI 差异已写为成员级 metadata，并重新生成受控产物。Vuetify projection 固定为 4.1.8。 |
| CLR host | 未新增 CLR module/global alias 表；`ToString`、`Length` 等继续使用逐成员 `Jazor(Op.Alias, ...)`，`Op.Import.Value` 继续是 runtime export/import 的物理名称。 |
| Emit consumer | 外部 Deno 与 Netpack consumer 均只使用 NuGet 携带的 browser asset/import map；不探测、复制或要求前端 `node_modules`。NetPack 0.8.0 已通过该 smoke。 |

### 发布说明

这是 breaking artifact ABI 变更：未显式映射的 C# member 现在以其 Roslyn 源名称发射，例如 `TitleText`
不再发射为 `titleText`。自有 JavaScript consumer 必须改用新名称，或在 binding/组件成员上添加明确的
`Description("@#...")` 或 `ECMAScriptName`。本迁移不提供项目级开关、旧 key 双写或运行时 fallback。

## 1. 目标

本迁移取消所有普通 C# 符号的隐式 PascalCase 到 lowerCamelCase 转换。最终名称只可来自：

1. ECMAScriptName。
2. Description("@#name")。
3. 原始 Roslyn 符号名。
4. JavaScript 语法或稳定唯一性所必需的编译器合成名，例如 backing-field hash、重载后缀和 constructor helper。

第 4 项不属于命名风格转换。不得借由合成名、Vue convention 或兼容逻辑重新引入大小写 fallback。

迁移完成后，未标注的 TitleText 必须稳定发射为 TitleText；外部 ABI 若需要 titleText、
modelValue、onUpdate:modelValue、default、header-data 或任何其他名称，必须在 binding/组件声明中逐项写明。

## 2. 冻结的行为合同

| 使用面 | 最终名称来源 | 本迁移后禁止的行为 |
| --- | --- | --- |
| 普通类型、方法、属性、字段、事件 | 统一名称合同 | PascalCase 到 lowerCamelCase |
| RazorVue prop | 参数成员的统一名称合同 | 参数名自动变小驼峰 |
| RazorVue listener key | EventCallback 成员的统一名称合同 | OnX、Changed、raw emit 到 onX 的反推 |
| RazorVue slot key | RenderFragment 成员的统一名称合同 | ChildContent/DefaultContent、XContent、kebab-case 推断 |
| Vue raw emit | 上游组件库 runtime；可作为 generator schema 审计数据保存 | 暴露为 C# 特性、由 RazorVue 声明或反推 |
| 直接 render 参数与 slot map | 已解析的 parameter runtime-name map | 局部再次 lower-case 或 ChildContent 特判 |
| CLR host member dispatch | 对应成员的 `Jazor(Op.Alias/Inline/Import/Compile)` 白名单映射 | 用普通命名 fallback 或全局 alias 表猜测 JS 成员 |

Jazor 的 `EventCallback` 只表示调用方传入的 listener property。原始 Vue emit 是外部组件自身的
runtime 合同，当前 RazorVue 既不消费它，也不为当前组件自动写出 `emits`。一个外部 v-model binding 的完整
声明形态如下：

~~~csharp
public sealed class ExternalInput : ComponentBase
{
    [Parameter, Description("@#modelValue")]
    public string? ModelValue { get; set; }

    [Parameter, Description("@#onUpdate:modelValue")]
    public EventCallback<string?> ModelValueChanged { get; set; }

    [Parameter, Description("@#default")]
    public RenderFragment? ChildContent { get; set; }
}
~~~

当 Description 已承载人类可读说明时，成员改用 ECMAScriptName。ECMAScriptName 的优先级高于
Description("@#...")；Description("@#") 继续只是名称解析边界，不是空别名。

RazorVue 保留识别 Parameter、EventCallback、RenderFragment、生成合法 object key、放置 h 的 props/slots
参数及检测重复 key 的职责。它不保留任何名称选择职责。

### 2.1 CLR host 映射边界

`ToString`、`Length`、`Count` 等 CLR 成员不是普通 C# 声明的命名问题，而是 CLR 语义投影到
JavaScript 宿主对象的问题。不得新建 CLR module 级的全局 alias 配置；现有逐成员的
`Jazor(Op.Alias, member, value)` 就是唯一的映射事实来源，例如：

~~~csharp
[Jazor(Op.Alias, "virtual object.ToString()", "toString")]
[Jazor(Op.Alias, "string.Length.get", "length")]
~~~

这种映射必须保留在拥有 CLR 语义的 module 源码中，并经 WhiteList generator 生成。不能以
`ToString -> toString` 或 `Length -> length` 的全局表替代它：不同 CLR 类型的同名成员可能需要
`Inline`、`Import` 或 `Compile` 才能保持格式化、异常和返回值语义；同一 CLR 概念也可能在不同
JavaScript carrier 上对应 `length`、`size` 或完全不同的实现。

`Op.Alias` 只改写 consumer 侧的成员访问，例如 `value.ToString()` 到 `value.toString()`；它不命名
CLR runtime module 的 producer export，也不应被 `ECMAScriptName` 重复声明。对于带非空 `Value` 的
`Jazor(Op.Import, member, value)`，该 `Value` 才是物理 runtime export 名称，CLR module 声明和跨 module
import 必须消费同一个值。不得把 `Op.Alias` 扩展成 export 重命名机制。

## 3. 明确不在范围内的事项

- 不增加项目级、程序集级或包级的 legacy naming 开关。
- 不为旧 artifact ABI 添加 fallback、双写 key 或运行时兼容层。
- 不改写 WhiteList 的持久化 key，不把 Op.Alias 当作普通成员命名策略。
- 不增加 CLR module 级的通用 alias 表，也不为已有 Jazor mapping 复制 ECMAScriptName。
- 不改变 backing-field hash、overload selector、constructor helper、import alias 或 source-map 的稳定算法。
- 不删除只用于 C# 局部变量、参数、WebIDL C# authoring 的大小写 helper；审计目标是最终 ECMAScript
  名称 consumer，而不是仓库内任何出现 ToCamelCase 的位置。

## 4. Gate G0：建立迁移清单

在改动任何 resolver 前，先生成一份可复核的名称清单。清单每一行至少包含：

| 字段 | 说明 |
| --- | --- |
| 项目与源文件 | 谁拥有声明 |
| Roslyn 符号 | 类型、成员及其 metadata display |
| 名称类别 | 普通 member、prop、listener、slot、合成名或 host alias；上游审计可另列 raw emit |
| 当前 artifact 名称 | 迁移前实际输出 |
| 目标 artifact 名称 | 迁移后显式或源名称 |
| 映射来源 | ECMAScriptName、Description、源名称或合成规则；上游 raw emit 仅作 schema 审计来源 |
| 保持 ABI 的动作 | 需要生成器、手写 binding、测试 snapshot 或无需修改 |

执行方法：

1. 在基线 revision 生成 compiler/RazorVue artifact，并保留稳定的名称 diff 输入。
2. 新增或扩展一个单文件 C# 审计入口到 scripts/csharp；不得使用 PowerShell。它以 Roslyn/反射读取已构建
   binding，并按项目、类型、成员排序输出可审阅结果。
3. 分别审计 Jazor.CLR、ECMAScript、ECMAScript.Vue3、VueRoute、Pinia、Style、Vuetify、Element Plus、
   TDesign 和 sample 的公开 ECMAScript surface。
4. 对每个 VueLibraryComponent 单独记录 prop key、listener key 与 slot key；上游 generator 可另列 raw emit
   供版本审计，但不得把它写回 C# 或交给 RazorVue 消费。
5. 把每个未标注但依赖旧输出的外部 ABI 分配给一个 binding owner。普通应用代码不补注解，预期改为源名称。

G0 通过条件：

- 没有“靠搜索结果猜测”的剩余项。
- 每个外部 ABI 差异都有确定的显式 metadata 写入点。
- 所有 legacy 输出变化都被分类为“有意的应用 ABI 改变”或“必须由 binding 保持”。

## 5. 实施阶段

### P1：先写新合同的回归测试

先增加会在旧实现上失败的测试，再改生产逻辑。

Compiler 单元测试覆盖：

1. 未标注的 type/member、property、field、method、event、record property、tuple field 保留原始大小写。
2. ECMAScriptName、Description("@#name")、普通 Description、空白 ECMAScriptName、Description("@#") 的优先级与边界。
3. 显式名称中的冒号、连字符、美元符号、保留字和非 identifier key 走合法 AST key，而不是字符串拼接。
4. overload hash、backing field hash、constructor helper、indexer helper 和 import alias 不变。
5. declaration、member access、assignment、invocation、record projection、object creation、export/import 和 source map
   对同一 symbol 消费同一最终名称。
6. AstConverter 的冲突预留不再人为保留小驼峰候选；加入相邻局部名不被错误改名的回归。
7. CLR host 专项：验证 `ToString -> toString`、`Length/Count -> length` 等既有 Alias 仍只影响调用侧；验证带
   显式 `Op.Import.Value` 的 runtime helper 在 module export 与跨 module import 两端都使用该值，且 Alias
   不会重命名 export。

RazorVue 单元测试覆盖：

1. 未标注的 TitleText、ValueChanged、OnSave、ChildContent、DefaultContent、HeaderContent 都原样输出。
2. 显式 modelValue、onUpdate:modelValue、default、带连字符 slot 和带冒号 listener 精确输出。
3. prop/listener 共享重复域，slot 独立重复域；检测使用已解析 key，而非 C# 名或 convention 名。
4. 派生 Parameter 隐藏、直接 render、CurrentComponentSemanticWalkerHost、descriptor catalog 和 bracket member
   access 都消费同一个名称 map。
5. 移除 VueDescriptorNaming、TryGetModelUpdateEventName、TryGetConventionalEventName 的旧测试，替换为
   “没有推断”断言。

### P2：先补齐生成 binding 的显式 metadata

这一阶段在旧 fallback 仍存在时完成，使外部 ABI 已由声明保护，再移除 resolver。

Element Plus：

1. prop 的是否标注判断由“是否等于 lower camel”改为“是否等于 C# property source name”。
2. slot 的是否标注判断由 Content/kebab convention 改为同一精确比较；默认 slot 也必须产生 default 映射。
3. 每个 EventCallback 属性写出精确 listener key；不得生成类级 raw emit 特性。
4. metadata model 可以保存上游 raw emit 与 listener key 用于审计，但 C# generated source 只消费 listener key，
   不允许生成结果依赖 RazorVue 反推。
5. 更新 ElContentComponentBase 的 ChildContent 为明确 default 映射，并重新生成 ElementPlus.Components.generated.cs。

TDesign：

1. 删除“conventional event name”及 “camel case 相同则省略”判断。
2. MappedProperty、MappedSlot、MappedEvent 分别携带最终 runtime name；Render 始终按 source-name 差异发射
   ECMAScriptName。
3. EventCallback 只在生成 C# 中显式落地 listener key；raw emit 留在上游 contract，不生成 C# descriptor。
4. 更新 TContentComponentBase 的 ChildContent 映射，重新生成 bindings/components，并用 --check 守护。

Vuetify：

现有 `VuetifyCatalogGenerator` 只扫描手写 `V*.cs` 并生成 export/registry catalog；它不是组件 contract
生成器，无法接管 prop、slot、listener 与 emit 的 ABI。因此不能把它当作本迁移的完成状态，必须升级为
Vuetify contract generator。

1. 建立受版本控制的 Vuetify contract schema，作为组件 C# 声明、component export 与 runtime member name 的唯一
   输入。schema 必须按 component/prop/slot/listener/emit 建模，至少保存 C# 名、完整 C# 类型形状、继承关系、
   XML 文档、Parameter 选项、runtime key、raw emit name 与必要的保留特性；不得把名称藏在 C# casing 规则里。
2. schema 中每个 prop、slot 与 listener 都必须有精确 runtime key，即使它恰好等于 C# 名；每个 emit 都必须有
   精确 raw emit name。listener key 与 raw emit name 是两个独立字段；runtime generator 不得由 `Changed`、
   `On`、`Content`、camel-case、kebab-case 或 `update:` 规则在 RazorVue 中相互推导。bootstrap schema importer
   可以一次性记录既有 ABI，但不能成为运行时 fallback。
3. 扩展/替换 `VuetifyCatalogGenerator`，使一次 `vuetify` 生成同时产出 component contract 与 export/registry
   catalog。runtime key 与 C# 名不同时，生成的 property 必须实际带上 `ECMAScriptName` 或 `Description("@#...")`；
   相同则保留源名这一统一默认。生成器还必须校验固定版本的 web-types、bundle export 与 contract schema；不得生成
   VueLibraryEmit 或另设 RazorVue 私有 mapping 表绕开成员 metadata。
4. 为保留已有的强类型 authoring 表面，generator 必须消费结构化类型信息或 Roslyn `TypeSyntax`，而不是将所有
   prop 降级为 `object`、字符串类型名或 runtime `any`。复杂 union、slot context、共享 base type、attribute
   capture 和非声明式辅助方法拆入 generated contract 与 hand-written partial extension 的明确边界，不能重复声明
   同一个 Parameter property。
5. 迁移时先用只读 Roslyn importer 从现有 114 个 contract 提取候选 schema 和 ABI 清单，再人工审阅并写回
   显式 name/emit 字段；该 importer 只用于建立输入，不能有 `--apply` 式的批量源文件改写模式。随后将原
   `V*.cs` 的声明所有权移至生成产物，手写文件仅保留共享类型、slot context 和确有行为的 partial extension。
6. catalog 也必须从同一个 schema 生成，不能再反向扫描 generated C# 作为事实来源。`vuetify --check` 必须同时
   比较所有 generated contracts 与 catalog，并验证 schema 的 component export、member key、emit 与生成属性一一
   对应。
7. 为 generator 单独增加 schema validation、deterministic snapshot、C# parse/compile 与 metadata reflection
   测试；为 VBtn、VTextField、VDialog、VDataTable 和至少一个 labs component 覆盖 model、普通 emit、default
   slot、named/scoped slot 与含冒号 listener key 的真实 artifact。

其他 binding：

1. 对 ECMAScript.Vue3、VueRoute、Pinia、Style 和 CLR host 的外部 JavaScript member 完成同一审计。
2. Op.Alias/Inline/Import 已经拥有宿主 mapping 的成员只补回归，不复制为普通 ECMAScriptName，也不另建
   CLR module alias 配置。Alias 只验证 consumer dispatch；带显式 Value 的 Import 还必须验证 producer export
   与跨 module import 使用同一 runtime 名称。

P2 的通过条件是：重新生成所有受控产物后，Element Plus、TDesign 与 Vuetify 的外部组件 prop、slot、listener、
artifact ABI 都不依赖任何 RazorVue 或 compiler casing fallback；raw emit 仅保留为上游 runtime/schema 审计数据；
Vuetify 的 component contract、catalog、shim 与 manifest 均由同一 schema/固定版本输入生成。

### P3：删除 compiler 全局 fallback

改动面：

- src/Jazor.Compiler/Util.cs：GetConfigOrSymbolName 直接使用源符号名，再保留既有 overload/backing-field
  稳定规则；删除 ShouldUseJsMemberNamingFallback 和 ConvertPascalCaseIdentifierToJsNaming。
- src/Jazor.Compiler/AstConverter.cs：删除为 camelized local candidate 预留的名称。
- 审计 SemanticWalker、AstConverter、Jazor.Compiler.Generator 的每个 GetConfigOrSymbolName consumer；只改仍在
  局部重写名称的代码，不重写 host dispatch 的既有 Alias/Import 行为。尤其 `Op.Import.Value` 是 module
  export/import 的显式名称来源，不能因移除 casing fallback 而退回到 adapter C# 方法名。

此阶段必须先运行 compiler regression，再更新所有预期 artifact。仅应用/测试模块中未标注成员的大小写变化
视为有意 ABI 变化；binding 的任何意外变化都回到 P2 修正，不得在 compiler 恢复 fallback。

### P4：删除 RazorVue 命名推断

改动面：

- LibraryComponentConventions：prop、slot、listener 统一直接消费 resolved member name；删除 raw emit descriptor
  查找与 GetEmitRuntimeName。
- 删除 ToDefaultRuntimeName、ToKebabCase、ToEmitName、TryGetModelUpdateEventName、
  TryGetConventionalEventName 与 VueDescriptorNaming 的消费点和死代码。
- RenderEmitter.ComponentFrame：parameter map 只在 runtime name 与 source property name 不同时记录；删除
  NormalizeSlotName 的 ChildContent 特判和 NormalizeDirectComponentParameterName 的 lower-case fallback。
- VueModuleBuilder、CurrentComponentSemanticWalkerHost、direct-render slot rewriter 与 descriptor extraction
  只传递已解析 map，并保持非 identifier key 的 computed access。
- ChildrenToSlotIntrinsic：移除硬编码 default key 的隐式 child overload lowering，改为要求显式 slots object
  或由明确 slot member metadata 提供 key。Razor SG 已绑定的 RenderFragment Parameter 仍可工作，但它必须从
  参数的 explicit default 映射取得 key。

P4 是名称语义的原子切换点：不得留下 “先尝试显式，再按 Vue convention” 的兼容分支。

### P5：更新 artifact 与集成测试

1. 更新 compiler snapshots、Razor SG generated C# fixture、descriptor catalog、.mjs 和 source-map 预期。
2. 为一个自有组件证明未标注 PascalCase prop/slot/listener 可端到端运行。
3. 为 Element Plus、TDesign、Vuetify 各选择至少一个 model、普通 emit、default slot、named slot 与特殊 key
   真实 runtime case，证明旧 ABI 由显式 metadata 保持。
4. 运行时测试应断言 props/slots/listener 的实际对象 key，并断言当前组件不会因 EventCallback 自动生成 `emits`；
   不只断言生成文本。
5. 检查 importer、module alias、manifest、bundle 和 HMR catalog 的名称稳定性，避免无关输出漂移。

### P6：收口与发布门槛

1. 将所有旧转换 helper、过时测试名称和文档叙述删除或迁为历史说明。
2. 更新 ECMAScriptNamingPolicy、组件封装原则、generator README 和 package release note。
3. 审查 git diff：除了计划列出的 generated artifacts、binding metadata、tests 和 docs 外，不接收无关格式化。
4. 本迁移作为 breaking artifact ABI 记录在 release note；不提供 global switch 或 silent compatibility fallback。

## 6. 重点文件清单

| 区域 | 首要文件 | 工作 |
| --- | --- | --- |
| 统一 resolver | src/Jazor.Compiler/Util.cs | 删除普通 member casing fallback，保留合成稳定名 |
| 模块命名 | src/Jazor.Compiler/AstConverter.cs；SemanticWalker.cs.Reference.cs | 删除人为 camel 冲突候选，审计声明/引用一致性 |
| RazorVue resolver | src/Jazor.RazorVue/RazorSdk/LibraryComponentConventions.cs | 只返回显式或源名称 |
| RazorVue direct render | RenderEmitter.cs；VueModuleBuilder.cs；CurrentComponentSemanticWalkerHost.cs | 只消费 name map，保留结构 lowering |
| imperative slots | ChildrenToSlotIntrinsic.cs | 消除硬编码 default 命名 |
| Element Plus | ElementPlusGenerator.cs；ElementPlusComponentBase.cs | 生成完整 metadata 并再生生成文件 |
| TDesign | TDesignComponentGenerator.cs；TDesignBindingGenerator.cs；TDesignComponentBase.cs | 生成完整 metadata 并再生生成文件 |
| Vuetify | ECMAScript.Vuetify contract schema、generated contracts；VuetifyCatalogGenerator.cs | 从同一显式 schema 生成 component contract、emit metadata 与 catalog |
| CLR host | Jazor.CLR/module/*.cs；WhiteList.cs.Generate.cs；AstConverter.cs；SemanticWalker.cs.Reference.cs | 保持逐成员 Alias/Inline/Import 语义；Import.Value 统一 module export/import 名称 |
| compiler tests | UtilBoundaryScenarioTests.cs；UtilSymbolNamingScenarioTests.cs；module/projection tests | 新默认与合成名不变性 |
| RazorVue tests | LibraryComponentConventionsTests.cs；RenderEmitterPrivateContractTests.cs；RazorSgComponentMemberClosureTests.cs；direct-render/slot/runtime suites | 无推断与端到端 ABI |

## 7. 验证矩阵

| Gate | 最小验证 | 完成证据 |
| --- | --- | --- |
| G0 | 审计清单完整，外部 ABI owner 已分配 | 已完成：所有仍依赖 JS ABI 的 binding 均回写为成员级 metadata，普通应用 member 改为源名称。 |
| G1 | compiler 命名单元测试；显式特殊 key AST 测试 | `Jazor.CompilerTest`: 10318/10318 passed。 |
| G2 | Element Plus、TDesign、Vuetify generator 重新生成与 --check；Vuetify schema/metadata 一致性审计 | Element Plus 111 components/2 directives、TDesign 120 documented bindings/118 basic components、Vuetify 114 components (4.1.8) 均 `--check` 通过。 |
| G3 | RazorVue convention、descriptor、direct render、slot、official Razor SG focused suites | `Jazor.RazorVue.Sg.Test`: 4684/4684 passed。 |
| G4 | 外部 binding runtime smoke：model、emit、default/named slot、特殊 key | `verify-vue-binding-coverage.cs` passed；`Jazor.EmitTest`: 142/142 passed、0 skipped，含无前端 `node_modules` 的 DenoHost runtime 与 Netpack package consumer smoke。 |
| G5 | 全仓库 test-dotnet 通过，generated artifact diff 已审阅 | `test-dotnet.cs` passed：Compiler 10318、CLR 4744、Style 28、Pinia 68、Pinia Testing 39、VueRoute 102、Razor SG 4684、Emit 142。 |

建议命令顺序：

~~~text
dotnet restore Jazor.slnx
dotnet run --project src/ECMAScript.Vue.Generator -- elementplus
dotnet run --project src/ECMAScript.Vue.Generator -- tdesign bindings
dotnet run --project src/ECMAScript.Vue.Generator -- tdesign components
dotnet run --project src/ECMAScript.Vue.Generator -- vuetify
dotnet run --project src/ECMAScript.Vue.Generator -- elementplus --check
dotnet run --project src/ECMAScript.Vue.Generator -- tdesign bindings --check
dotnet run --project src/ECMAScript.Vue.Generator -- tdesign components --check
dotnet run --project src/ECMAScript.Vue.Generator -- vuetify --check
dotnet test src/Jazor.CompilerTest/Jazor.CompilerTest.csproj
dotnet test src/Jazor.RazorVue.Sg.Test/Jazor.RazorVue.Sg.Test.csproj
dotnet run --file scripts/csharp/verify-vue-binding-coverage.cs
dotnet run --file scripts/csharp/test-dotnet.cs
~~~

若并行执行 test lane，按仓库规则隔离 BaseOutputPath；成功 build 后优先使用 --no-build 做 focused rerun。

## 8. 风险与防线

| 风险 | 防线 |
| --- | --- |
| 外部 Vue ABI 静默变化 | P2 先完成 metadata，P5 做真实 runtime key 断言 |
| listener/raw emit 混淆 | C# 与 RazorVue 只保留 listener key；raw emit 限定在上游 schema 审计，禁止互相转换 helper |
| ChildContent 仍被某条 direct path 特判 | 对 RenderEmitter、slot intrinsic、parameter map 做静态搜索和负向测试 |
| 生成器重新省略 metadata | --check 加上 generated-source assertion，比较 source name 而非 convention name |
| 误伤 CLR host / WhiteList | Alias/Inline/Import 专项 regression，不修改 persisted whitelist key |
| Alias 被误用为 module export 名 | CLR catalog 与跨 module import 断言带显式 Import.Value 的同名绑定；不新增全局 alias 配置 |
| source-map 或 import alias 漂移 | P1/P5 对稳定输出建立断言，变更须逐项解释 |
| 工作区存在并行 HMR/emit 改动 | 仅触碰本计划列出的文件；每次提交前按 path 审查 diff |

## 9. 完成定义

以下条件同时满足才可标记完成：

1. 代码中不存在普通 PascalCase 到 lowerCamelCase fallback，也不存在 Vue 名称 convention fallback。
2. 未标注的普通 member、prop、listener、slot 都保留其 Roslyn 源名称。
3. 所有外部 ABI 差异由成员级名称 metadata 明确表达；raw emit 不暴露为 C# 或 RazorVue 特性。
4. Description("@#") 边界、ECMAScriptName 优先级、event target 支持、合成稳定名和 WhiteList key 合同未回归。
5. 所有 G0-G5 验证通过，且 release note 明确这是 breaking artifact ABI 改动。
