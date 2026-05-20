# RazorVue 完成度与生产就绪评审

> 评审日期：2026-05-07  
> 最新更新：2026-05-20
> 评审范围：`src/Jazor.RazorVue/`、`src/Jazor.Analyzer/RazorVue/`、`src/Jazor.Emit` 的 RazorVue 路径、`src/Jazor.RazorVue.Test/`、`src/Jazor.RazorVue.RazorIr.Test/`、`src/Jazor.EmitTest/` 的 RazorVue 切片、`src/Jolt.Test` 的 Volar/VueAnalysis/JazorVue 切片、`samples/RazorVue.TodoList/`  
> 基线说明：本评审基于当前工作区状态。当前工作区存在多处 RazorVue/emit/VueRoute 相关未提交修改，因此结论不等同于已发布包基线。

## 结论

RazorVue 不能继续按历史文档里的“100% 完成”口径描述。当前更准确的判断是：

- **核心语义与 lowering 主链：约 85% 完成**。组件发现、descriptor、Razor IR 优先模板前端、canonical H、SFC semantic model、SFC artifact、legacy/SFC 双 catalog、source-origin/hash/HMR 元数据等主干已经成型，并有大量单元测试覆盖。
- **库模式生产接入：关键 P0 已解除，但仍未完成最终上线门槛**。Razor SG tail 注入、当前 context 级接管判断、package payload 守卫、emit/SDK RazorVue 集成切片、TodoList sample 纯 Deno build/SSR/browser smoke、独立外部 NuGet `.razor` SFC consumer 与独立外部纯 Deno consumer 已通过；剩余重点是全量回归、支持矩阵和调试闭环。
- **Jolt 关联能力：局部可用，不能替代库模式验收**。Volar/VueAnalysis/virtual artifact 过滤测试通过，但这只证明 Jolt 相关局部协议和投影切片，不证明 RazorVue NuGet/SDK 生产消费链路闭合。

当前建议状态：

| 维度 | 状态 | 说明 |
|------|------|------|
| 核心库编译 | 通过 | `Jazor.RazorVue` 可构建 |
| RazorVue 单元测试 | 通过 | 1072 通过 |
| Razor IR 前端测试 | 通过 | 371 通过 |
| Jolt 关联过滤测试 | 通过 | 35 通过 |
| solution 构建 | 通过但有警告 | 1 个 Razor IR 测试项目 nullable 警告 |
| emit/SDK RazorVue 集成 | 通过 | RazorVue 过滤切片 45/45 通过 |
| NuGet payload | 通过当前守卫 | `Jazor` 包 analyzer/lib payload 不携带 Razor Compiler / Razor Utilities Shared |
| 生产发布判断 | **接近但未达最终上线标准** | SDK/package/emit/sample/纯 Deno SSR/browser/独立外部 .NET + Deno consumer 已闭合；全量回归、支持边界和调试闭环仍需最终确认 |

## 2026-05-09 状态更新

本轮已解除 2026-05-07 评审中的 emit/SDK P0 阻断：

```powershell
dotnet test src/Jazor.EmitTest/Jazor.EmitTest.csproj --filter "FullyQualifiedName~CreateLocalPackage_IncludesRazorVueAuthoringAssets" -v minimal -p:UseSharedCompilation=false
```

结果：1/1 通过。

```powershell
dotnet test src/Jazor.EmitTest/Jazor.EmitTest.csproj --filter "FullyQualifiedName~RazorVue" -v minimal -p:UseSharedCompilation=false
```

结果：45/45 通过。

补充结论：

1. `CreateLocalPackage_IncludesRazorVueAuthoringAssets` 已同步当前 analyzer payload，包含 `0Harmony.dll`、`ECMAScript.dll`、`ECMAScript.Vue3.dll` 及其 pdb。
2. 包内容测试已加入负向守卫，禁止 `Razor.Compiler`、`Razor.Utilities.Shared`、`Microsoft.CodeAnalysis.Razor`、`Microsoft.AspNetCore.Razor.Language` 进入 `Jazor` 包 payload。
3. 之前 `--no-restore` 下出现的 `ECMAScript.Contract` predefined type / `System.Runtime` 错误已确认是 restore/assets 状态问题；带 restore 的 focused test 可稳定通过。
4. `samples/RazorVue.TodoList/build-local.cs` 已用本地 pack 的 `Jazor` / `ECMAScript.Vuetify` 包重新跑通；当前样例生成 2 个 SFC artifact、manifest、host requirements module 和 sidecar。
5. 生成的 `todo-app.vue` 已恢复完整嵌套 Vuetify 结构，包含 `VRow` / `VCol` / `VCard` / `VTextField` / `VList` / `VListItem`，并包含 `item.Title` / `item.IsDone` / `item.Category` / `item.IsPinned` 等 DTO 属性投影。
6. `samples/RazorVue.TodoList/todo-consumer` 已切到纯 Deno consumer 验证链；`dotnet run --file .\scripts\run-deno.cs -- task build` 通过，底层实际调用仓库内 bundled `deno.exe`，证明当前生成的 `.vue` 可以先经 Deno 侧 SFC 预编译，再通过 `deno bundle` 产出浏览器 JS/CSS。
7. 新增 `Build_LocalPackages_WithExternalRazorSgSfcConsumer_EmitsVueSfcArtifacts`，用独立临时 consumer 通过本地 NuGet 包消费 `Jazor` / `ECMAScript.Vuetify`，显式启用 `UseRazorSourceGenerator=true`、`JazorRazorVueEnableRazorSgIntegration=true`、`JazorRazorVueOutputMode=sfc`，并验证 `.razor` authoring 生成 `.vue`、manifest、host requirements module、source map 和 origins sidecar。
8. SFC template lowering 已修正 component literal prop 语义：组件非字符串 literal（例如 Vuetify `bool` / `number` props）输出为 Vue bound props，如 `:fluid="true"`、`:cols="12"`；字符串 literal 仍输出静态属性，如 `title="Inbox"`。
9. `samples/RazorVue.TodoList/todo-consumer` 已执行 `dotnet run --file .\scripts\run-deno.cs -- task smoke:ssr`，通过纯 Deno 的 Vue server renderer + Vuetify 对生成 SFC 做 runtime render smoke，且不再出现 `fluid` Boolean prop 类型 warning。
10. `samples/RazorVue.TodoList/todo-consumer` 已执行 `dotnet run --file .\scripts\run-deno.cs -- task smoke:bundle-api`，证明 `Deno.bundle()` 也能消费 Deno 预编译后的 RazorVue entry，产出 JS/CSS 与 source map。
11. `samples/RazorVue.TodoList/todo-consumer` 已移除 `package.json` / `package-lock.json` 包装层，仓库内正式 consumer 入口收口为 `deno.json` tasks + `scripts/run-deno.cs`。

## 2026-05-10 状态更新

本轮继续解除真实浏览器和独立外部 Deno consumer 阻断：

1. `todo-consumer` 新增 `smoke:browser`，使用纯 Deno 静态服务 + headless Edge/Chrome CDP 验证浏览器真实挂载，不引入 Vite/npm wrapper。
2. 浏览器 smoke 会失败于 console warning/error、runtime exception、网络加载失败、缺失生成 CSS/JS、缺失 Vuetify `.v-application` root、关键文本不可见或 TodoList 交互状态未更新。
3. `todo-consumer` 生成 `vue-feature-flags.mjs` 并作为 browser entry 第一依赖，显式设置 Vue esm-bundler feature flags，避免生产浏览器 smoke 中出现 Vue feature flag warning。
4. TodoList sample 根组件已提升为 `VApp` / `VMain` / `VContainer` 结构，浏览器 smoke 不再通过降低断言绕过 Vuetify app shell，而是验证真实 Vuetify application root。
5. `VCardTitle` authoring 合同已修正为默认 `ChildContent`，移除错误的 `Text` 参数。Vuetify 3.8.0 `VCardTitle` 是 simple functional 标题容器，不消费 `text` prop；此前 `<VCardTitle text="...">` 会导致标题在真实浏览器不可见。
6. `Build_LocalPackages_WithExternalRazorSgSfcConsumer_PureDenoPipeline_PassesInIsolatedWorkspace` 已覆盖独立临时 `.NET + Razor SG + SFC` consumer，再用独立纯 Deno consumer 执行 `test` task，包含 SSR smoke、`Deno.bundle()` smoke、browser build 和 browser smoke。
7. `Build_LocalPackages_RazorVueTodoListSample_PureDenoPipeline_PassesInIsolatedWorkspace` 已覆盖仓库 TodoList sample 的本地 package build、纯 Deno SSR、`Deno.bundle()`、browser build、browser smoke。

## 2026-05-20 状态更新

本轮继续收紧 typed `RenderFragment<T>` / typed slot 的正式合同，重点不是再加一层 bridge，而是把已有 lowering 主线做实：

1. `Jazor.Compiler` 新增 `SemanticWalkerHost.RewriteVariableDeclaratorPreorder(...)` seam，允许 RazorVue 在 imperative bridge 内改写局部 `RenderFragment` carrier 的 declarator，同时继续复用 `SemanticWalker` 完成 builder body 的 CLR-aware 翻译、引用绑定、import 收集和求值顺序保持。
2. imperative bridge 中的局部 typed `RenderFragment<T>` carrier 现在直接 lower 为最终 Vue slot callback：slot callback 自己创建 `__jazorCreateRenderContext(h)`，经 `Jazor.Compiler` / `SemanticWalker` 翻译 body，最后返回 `.finish()`。
3. 旧的中间 wrapper/helper 协议已经从这条 typed carrier 路径移除；该路径不再引入 `__jazorCreateRenderSlot`、`__jazorCreateContextualRenderSlot` 或 `__jazorInvokeRenderFragment`。
4. `RenderFragment` / `RenderFragment<T>` 的 canonical 检测统一改为 `OriginalDefinition.ToDisplayString(Jazor.Common.Format.NameFormat)` 精确判定。这里必须注意：`Format.NameFormat` 对 delegate 输出的是完整签名，而不是裸类型名；当前 canonical 名称分别是 `Microsoft.AspNetCore.Components.RenderFragment(Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder)` 与 `Microsoft.AspNetCore.Components.RenderFragment<TValue>(TValue)`。
5. Razor IR typed child-content/template-body 的 standalone imperative promotion 继续补齐 parity 与 pipeline 回归，覆盖 `if`、`while`、`do-while`、`foreach`/`for` + `break`/`continue`、`switch`、`try-catch-finally`、`using` / `using declaration`、`lock`、`return` / `throw` / mutation tail，以及局部 typed carrier 赋给组件 typed slot 的 imperative tail 保活场景。
6. imperative typed slot 的 factory-backed local/member carrier 已与 inline typed template 对齐：`RenderFragment<T>` 局部 carrier、当前组件 property/field carrier，以及 `CreateTemplate(Title)` 这类 fragment factory 返回值，都会直接 lower 成最终 Vue slot callback，不再残留 `__jazorCreateRenderSlot` / `__jazorCreateContextualRenderSlot` / `__jazorInvokeRenderFragment` 或中间 wrapper JS。
7. factory captured 参数不再通过额外 JS wrapper 传递，而是通过 canonical parameter alias 直接内联进最终 callback body；例如 `CreateTemplate(Title)` 中的 `title` 会直接 lower 为 `props.title`。
8. 当前组件 property/field carrier、局部 carrier、fragment factory 返回值三条线现在共用一条静态 carrier 解析链；component resolution、imperative runtime-usage / descriptor identity 收集，以及最终 imperative slot callback lowering 都会沿这条链继续追踪 nested builder body，因此 factory-backed typed slot 内部的 `OpenComponent<T>` 会稳定产出组件 import、imperative metadata 和 `enterComponent(...)` lowering。
9. handwritten `BuildRenderTree` 中局部 typed `RenderFragment<T>` carrier 的“先声明、再紧邻一次简单赋值”窄模式，现已补齐 mixed imperative 路径：即使声明/赋值出现在 declarative 前缀，而真正消费出现在后续 imperative segment，这条 carrier 仍会按同一静态合同被恢复，不会因为 segmentation 把初始化来源丢失。
10. 同一条局部 carrier 合同现在已在 BuildRenderTree template frontend / mixed imperative segmentation / pipeline lowering 三条线上统一收口：若“先声明、再紧邻一次简单赋值”的 local `RenderFragment` / `RenderFragment<T>` 在后续再次出现可观察写入，RazorVue 会显式 fail-fast，而不会继续静默沿第一次赋值恢复旧模板。
11. RazorVue 两套高频测试宿主现已补上 metadata reference 进程级缓存；fresh full `dotnet test -p:UseSharedCompilation=false` 不再因每个测试重复 `MetadataReference.CreateFromFile(...)` 而在 Roslyn metadata 装载阶段触发 OOM，验证基线回到可重复状态。
12. 静态 `MarkupString` local carrier 现已与 `RenderFragment` carrier 的 source-stable 窄模式进一步收敛：handwritten `BuildRenderTree` 与 Razor IR authored template / pipeline / SFC 路线都支持“先声明、再紧邻一次简单赋值”的 `MarkupString` local，再由 `AddContent(...)` 或 `@markup` 消费；若后续再次出现可观察写入，则统一 fail-fast，而不是回退成通用 assignment unsupported。
13. Razor IR authored template 下的本地 `RenderFragment` / `RenderFragment<T>` carrier 现在也已正式补齐这条 source-stable 窄模式：`RenderFragment<T> template; template = ...;` 可继续赋给组件 typed slot/template 参数并贯通 render tree / `.mjs` pipeline；本轮又补齐并锁定了 immediate-assignment 右侧来自 current-component member carrier 与受支持 fragment factory 返回值的变体，不再错误退回 imperative tail。若不是紧邻一次简单赋值，或后续再次出现可观察写入，则同样显式 fail-fast。
14. Razor IR authored template 中 direct untyped `RenderFragment` expression consumption 现已正式补齐：`@Template`、`@template` 这类 current-component member / source-stable local carrier 会直接还原为结构化 render subtree，不再重复输出同一模板体、把普通 member 误判成 slot outlet，或把 immediate-assignment local 错误退回 imperative tail。对于 `private RenderFragment Template => @<...>` 这类 property initializer，如果 Razor SG 生成后只剩 builder lambda 且 direct operation-level source mapping 缺失，当前实现会明确依赖 shared builder parser fallback，而不会再把这类 direct source-map 缺失误当成“功能不支持”。
15. Razor IR authored template 中 direct typed `RenderFragment<T>` invocation 现也已正式补齐：`@Template(42)`、`@template(42)` 这类 current-component member / source-stable local carrier，以及 `@CreateTemplate(Title)(42)`、`@CreateTemplate()(42)`、`@CreateTemplate(subtitle: Subtitle, title: Title)(42)` 这类直接调用当前组件 fragment factory 返回值的 authored 语法，都会直接还原为 typed fragment scope，并继续保留 factory/member captured-value scope，而不会退化成普通 invocation 表达式或在 canonical/SFC 阶段触发 unsupported member/property 错误；其中 named argument out-of-order 也会保留调用点求值顺序。
16. Razor IR authored template 中 direct typed slot outlet invocation 也已对齐 handwritten `BuildRenderTree`：`@Header(Count + 1)` 这类当前组件 `[Parameter] RenderFragment<T>?` slot source 会直接还原为带 argument 的 slot outlet，并最终稳定 lower 为 `<slot name="header" :value="(props.count + 1)" />`，而不会再退化成普通插值表达式。
17. Razor IR authored template code-block 中“局部 `RenderFragment` carrier + 同块 local function fragment factory 声明”现也已正式补齐：`@{ RenderFragment<int> template = CreateTemplate(Title); RenderFragment<int> CreateTemplate(string? title) => item => @<span>@title @item</span>; }` 这类 authored 形态会与 `@code` factory 保持同一 captured-value scope + typed fragment scope 语义，不再因为 local function 声明残片、尾随 `;` 或内部 `@<...>` 模板节点未消费而把它们泄漏成 render tree 根节点；这条修复同时把 local-function-authored template 节点消费从 syntax 级扩大到 operation-coverage 级 source range 标记，避免 Razor IR 边界切分差异再次漏网。
18. 在同一补齐面上，Razor IR authored template 现在也支持“template code-block 内 local function fragment factory 的 direct typed invocation”这一原生 authoring：`@{ RenderFragment<int> CreateTemplate(string? title) => item => @<span>@title @item</span>; } @CreateTemplate(Title)(42)` 会和当前组件 `@code` / member factory 一样直接还原为“外层 captured-value scope + 内层 typed fragment scope”，并贯通 render tree / parity / SFC 路线。为此，template code-block 前缀扫描现已把 pure local-function declaration block 视为已绑定声明前缀，而不会再把这类只含 local function 声明的 code-block 错误留成 unbound `CSharpCodeIntermediateNode`。

## 已完成能力

1. RazorVue 核心语义层已经从早期“只靠 BuildRenderTree 逆推模板”的方向明显前进到 IR 优先路线。
   - `RazorVuePreferredTemplateFrontend` 在有 `RazorDocumentPath` 时走 `RazorVueRazorIrTemplateFrontend`。
   - 只有源码手写 `BuildRenderTree` 才允许 fallback。
   - Razor 生成组件无法绑定 Razor 文档时会显式失败，而不是静默退回旧前端。

2. SFC lane 已经成为当前 generator 默认方向。
   - `Jazor.props` 默认 `JazorRazorVueOutputMode=sfc`。
   - `RazorVueGenerator.ResolveOutputMode(null)` 返回 `Sfc`。
   - SFC 模式下已经按每个 artifact 单独 `AddSource(...)`，再生成小型 `Jazor.Generated.RazorVueCatalog.g.cs` 聚合入口，旧计划里“仍是单大 catalog、默认仍是 legacy”的描述已经过期。

3. canonical/SFC artifact 边界已经建立。
   - `RazorVueCanonicalHModelFactory` 负责 canonical H 模型。
   - `RazorVueSfcSemanticModelFactory` 负责 template/script/style/custom block 语义。
   - `VueSfcArtifact` 带 `TemplateBlock`、`ScriptSetupBlock`、`StyleBlocks`、`CustomBlocks`、`StyleHash`、source origins 和 HMR boundary。

4. emit 侧已有 legacy 与 SFC 双 catalog 读取和物化能力。
   - `ModuleCollector` 能识别 legacy catalog 与 SFC catalog，并禁止 mixed legacy/SFC 同次 emit。
   - `RazorVueSfcCatalogReader`、`RazorVueSfcModuleWriter`、`RazorVueSfcManifestFactory` 已存在。
   - manifest diff/update-plan 也已具备 SFC 相关模型入口。

5. 测试覆盖量较高。
   - `src/Jazor.RazorVue.Test` 和 `src/Jazor.RazorVue.RazorIr.Test` 合计当前约 625 个 `[TestMethod]`。
   - 已覆盖 descriptor、pipeline、render tree、canonical/SFC、Razor IR、parity、analyzer/generator 等多个层次。
   - 其中 Razor IR typed child-content/template-body 切片已覆盖 standalone imperative body 的 `if`、`while`、`do-while`、`foreach`+`break/continue`、`for`+`break/continue`、`switch`、`try-catch-finally`、`using` / `using declaration`、`lock`、`throw tail`、`return tail`、mutation tail 等回归，并补上了 render-function SFC 产物验证。

## 主要缺口

### 已解除：emit/SDK 集成测试失败

2026-05-07 评审时执行：

```powershell
dotnet test src/Jazor.EmitTest/Jazor.EmitTest.csproj --filter "FullyQualifiedName~RazorVue" -v minimal
```

历史结果：

- 通过：35
- 失败：8
- 总计：43

失败集中在：

- `RazorVueCatalogReader_ReadsGeneratedCatalogFromRealGeneratorAssembly`
- `RazorVueSfcCatalogReader_ReadsGeneratedCatalogFromRealGeneratorAssembly`
- `CreateLocalPackage_IncludesRazorVueAuthoringAssets`
- `Build_LocalJazorPackage_RazorVueAuthoring_EmitsManifestAndHostRequirementsModule`
- `Build_LocalPackages_WithPackagedRazorVueVuetify_EmitsRazorVueBundleAndSidecars`
- `Build_LocalPackages_WithPackagedCustomRazorVueLibrary_EmitsRazorVueBundleAndSidecars`
- `Build_LocalJazorPackage_WithSourceReferencedRazorVueSample_EmitsRazorVueOutputs`
- `Build_LocalJazorPackage_WithSourceReferencedRazorVueSample_SecondBuildWritesUpdatePlan`

主要错误形态：

- 测试/样例源码找不到 `IVueComponent` / `IVueLibraryComponent`。
- 测试源码仍 `using ECMAScript.VueContract;` 后裸用 `IVueComponent`，但当前真实类型是 `ECMAScript.Vue3.IVueComponent` / `ECMAScript.Vue3.IVueLibraryComponent`。
- 本地包内容断言仍按旧 entry 列表，实际包内容已经变化。
- 多个 SDK 集成测试仍期待 legacy `.mjs` 产物路径，而当前默认已经切到 SFC。

该阻断已在 2026-05-09 解除。当前 RazorVue emit/SDK 切片已经做到 45/45 通过，package contents 断言也已经同步到当前 analyzer payload，并增加 Razor Compiler / Razor Utilities Shared 负向守卫。

### 已解除：TodoList sample / Deno build / SSR consumer smoke

当前已把最新包与 SG tail 注入链通过仓库内 TodoList 样例重新跑完：

1. `samples/RazorVue.TodoList/build-local.cs` 从本地 pack 出来的 `Jazor` / `ECMAScript.Vuetify` 包恢复并构建成功。
2. 构建产出 `.vue`、manifest、host requirements module 和 sidecar。
3. 不依赖仓库源码引用、不依赖本机全局缓存残留、不依赖旧 legacy `.mjs` 默认假设。
4. `todo-consumer` 纯 Deno build 能消费生成的 `.vue` 与 host requirements module。
5. `todo-consumer` SSR smoke 能通过 Deno 预编译后的 Vue module、Vue server renderer 和 Vuetify plugin 渲染 DTO 投影文本，并验证 host requirements 中包含 `vuetify` plugin 和 `vuetify/styles`。
6. `todo-consumer` `Deno.bundle()` smoke 已通过，说明同一套 Deno 预编译入口既能走 CLI bundle，也能走 runtime bundle API。
7. `todo-consumer` browser smoke 已通过，证明生成 SFC 经纯 Deno build 后可在真实浏览器中挂载、加载 CSS/JS、建立 Vuetify `.v-application` root，并完成 TodoList 核心交互。
8. `todo-app.vue` 中 Vuetify Boolean / numeric props 已以 Vue binding 输出，避免把 `bool` / `number` 传成字符串导致运行时 prop warning。

### 已解除：独立外部 .NET NuGet/SFC + 纯 Deno consumer smoke

`Build_LocalPackages_WithExternalRazorSgSfcConsumer_EmitsVueSfcArtifacts` 已覆盖一个独立临时 consumer：

1. 不复用仓库 sample 源码。
2. 只通过本地 pack 的 `Jazor` / `ECMAScript.Vuetify` NuGet 包消费 RazorVue。
3. 使用 `Microsoft.NET.Sdk.Razor`、官方 Razor SG、`.razor + .razor.cs` authoring 和 SFC 输出模式。
4. 验证输出 `components/external-dashboard.vue`，并确认不会同时产出 legacy `external-dashboard.mjs`。
5. 验证 host requirements module、RazorVue manifest、SFC source map 和 origins sidecar。

后续 `Build_LocalPackages_WithExternalRazorSgSfcConsumer_PureDenoPipeline_PassesInIsolatedWorkspace` 已把独立临时纯 Deno consumer 纳入自动化主链：同一测试在独立 `.NET` consumer 生成 `.vue` 后，复制纯 Deno consumer 脚本，执行 SSR smoke、`Deno.bundle()` smoke、browser build 和 browser smoke，避免只依赖仓库 sample consumer。

### P0：authoring surface 合同需要用 sample / README 再确认

当前 `RazorVueCompilationSymbols` 解析的正式入口仍是：

1. `ECMAScript.Vue3+IVueComponent`
2. `ECMAScript.Vue3+IVueLibraryComponent`

emit 集成测试已经收口，但生产前还必须确认 sample、README 和用户文档使用同一套 authoring 写法。不能出现测试靠 `global using static ECMAScript.Vue3` 通过，而 sample/文档仍展示旧 `using ECMAScript;` + 裸 `IVueComponent` 的漂移。

### P1：Razor IR 路线仍有过渡依赖

IR 前端已经是正确方向，但不是完全独立的 Razor 语义前端：

- 结构主要来自 Razor IR。
- 表达式仍通过 generated C# / Roslyn operation resolver 回映射。
- 当前这是合理过渡，因为表达式 lowering 复用 Roslyn 语义；但生产文档应明确这是当前边界，不应宣称已完全摆脱 generated `BuildRenderTree` 依赖。

### P1：支持面仍是安全子集，不是完整 Razor/Vue 语义

当前实现对不支持场景倾向 fail-fast，这是正确选择。但生产宣传必须明确支持边界：

- 生命周期/setup 逻辑仍是受控子集。
- `ShouldRender`、`SetParametersAsync`、复杂字段/方法、复杂局部变量、复杂 slot/lifted binding 等仍有显式 unsupported/fail-fast 路径。
- HMR 目前主要是身份、hash 和边界分类元数据，不是完整运行时热替换能力。
- SSR、浏览器挂载与 TodoList 核心交互已有自动化 smoke；hydration 和真实 sourcemap 调试闭环仍是待验证生产能力。

### P1：source map/source-origin 仍偏元数据与 sidecar，生产调试闭环未充分证明

RazorVue artifact 已保留 source origins，emit 能写 `.map` / `.origins.json` 类 sidecar。但生产就绪还需要证明：

- `.razor -> .vue -> bundled JS` 的 sourcemap 链在真实浏览器调试中可用。
- 诊断能稳定回映射到 Razor 源位置。
- style/template/script block 的 origin 粒度能支撑实际调试和增量更新。

### P2：测试/实现体量已经进入维护风险区

当前 `src/Jazor.RazorVue.Test/RazorVuePipelineTests.cs`、`ESGeneratorTests.cs` 都接近或超过万行级别，`Jazor.RazorVue` 内部也有多个 700-1300 行热点文件。短期不阻断生产，但后续扩功能时会影响定位和 review。

## 本轮验证证据

```powershell
dotnet build src/Jazor.RazorVue/Jazor.RazorVue.csproj -v minimal
```

结果：通过，0 警告，0 错误。

```powershell
dotnet test src/Jazor.RazorVue.Test/Jazor.RazorVue.Test.csproj -v minimal -p:UseSharedCompilation=false
```

最新结果：1072 通过，0 失败，0 跳过。

```powershell
dotnet test src/Jazor.RazorVue.RazorIr.Test/Jazor.RazorVue.RazorIr.Test.csproj -v minimal -p:UseSharedCompilation=false
```

最新结果：371 通过，0 失败，0 跳过。

```powershell
dotnet test src/Jolt.Test/Jolt.Test.csproj --filter "FullyQualifiedName~Volar|FullyQualifiedName~VueAnalysis|FullyQualifiedName~VirtualArtifact|FullyQualifiedName~JazorVue" -v minimal
```

结果：35 通过，0 失败，0 跳过。

```powershell
dotnet build Jazor.slnx -v minimal
```

结果：通过，1 个 nullable 警告，0 错误。

```powershell
dotnet test src/Jazor.EmitTest/Jazor.EmitTest.csproj --filter "FullyQualifiedName~RazorVue" -v minimal -p:UseSharedCompilation=false
```

最新结果：45 通过，0 失败，0 跳过。

```powershell
dotnet test src/Jazor.RazorVue.Test/Jazor.RazorVue.Test.csproj --filter "FullyQualifiedName~GenerateCatalog_WithOfficialRazorSgDocument_TailBridge" -p:JazorIsolatedBaseOutputRoot="D:\repository\own\jazor\Jazor\.tmp\test-out\razorvue-tailbridge-4\" -p:JazorIsolatedBaseIntermediateOutputRoot="D:\repository\own\jazor\Jazor\.tmp\test-obj\razorvue-tailbridge-4\" /nr:false -p:UseSharedCompilation=false -v minimal
```

最新结果：3 通过，0 失败，0 跳过。

```powershell
dotnet test src/Jazor.RazorVue.RazorIr.Test/Jazor.RazorVue.RazorIr.Test.csproj --filter "FullyQualifiedName~RazorVueRazorIrTemplateFrontendTests" -p:JazorIsolatedBaseOutputRoot="D:\repository\own\jazor\Jazor\.tmp\test-out\razorir-frontend-1\" -p:JazorIsolatedBaseIntermediateOutputRoot="D:\repository\own\jazor\Jazor\.tmp\test-obj\razorir-frontend-1\" /nr:false -p:UseSharedCompilation=false -v minimal
```

最新结果：28 通过，0 失败，0 跳过；仍有 `RazorEngineFeatureSpikeTests.cs(152,30)` nullable 警告。

```powershell
dotnet test src/Jazor.RazorVue.Test/Jazor.RazorVue.Test.csproj --filter "FullyQualifiedName~RazorVue_SfcArtifactFactory|FullyQualifiedName~GenerateCatalog_WithOfficialRazorSgDocument_TailBridge" -p:JazorIsolatedBaseOutputRoot="D:\repository\own\jazor\Jazor\.tmp\test-out\razorvue-sfc-tail-1\" -p:JazorIsolatedBaseIntermediateOutputRoot="D:\repository\own\jazor\Jazor\.tmp\test-obj\razorvue-sfc-tail-1\" /nr:false -p:UseSharedCompilation=false -v minimal
```

最新结果：31 通过，0 失败，0 跳过；新增覆盖 component literal 非字符串 props 输出为 Vue bound props。

```powershell
dotnet run --file ./samples/RazorVue.TodoList/build-local.cs
```

最新结果：通过，`assemblies=15 catalogs=2 razorvueSfcCatalogs=1 modules=51 razorvueSfcArtifacts=2 written=3 skipped=51 deleted=0`。

```powershell
cd samples/RazorVue.TodoList/todo-consumer
dotnet run --file .\scripts\run-deno.cs -- task build
```

最新结果：通过，`deno bundle` 产出浏览器 JS/CSS。

```powershell
cd samples/RazorVue.TodoList/todo-consumer
dotnet run --file .\scripts\run-deno.cs -- task smoke:ssr
```

最新结果：通过，`RazorVue TodoList Deno SSR smoke passed.`，无 Vue prop 类型 warning。

```powershell
cd samples/RazorVue.TodoList/todo-consumer
dotnet run --file .\scripts\run-deno.cs -- task smoke:bundle-api
```

最新结果：通过，`Deno.bundle()` 产出 JS/CSS 与 linked source map。

```powershell
cd samples/RazorVue.TodoList/todo-consumer
dotnet run --file .\scripts\run-deno.cs -- task smoke:browser
```

最新结果：通过，`RazorVue browser smoke passed.`，覆盖真实浏览器挂载、生成 CSS/JS 加载、Vuetify `.v-application` root、关键文本可见、TodoList 交互状态更新，以及 console warning/error / runtime exception / network failure 负向守卫。

```powershell
dotnet test src/Jazor.EmitTest/Jazor.EmitTest.csproj --filter "FullyQualifiedName~Build_LocalPackages_WithExternalRazorSgSfcConsumer_EmitsVueSfcArtifacts" -p:JazorIsolatedBaseOutputRoot="D:\repository\own\jazor\Jazor\.tmp\test-out\emit-external-sfc-3\" -p:JazorIsolatedBaseIntermediateOutputRoot="D:\repository\own\jazor\Jazor\.tmp\test-obj\emit-external-sfc-3\" /nr:false -p:UseSharedCompilation=false -v minimal
```

最新结果：1 通过，0 失败，0 跳过。

```powershell
dotnet test src/Jazor.EmitTest/Jazor.EmitTest.csproj --filter "FullyQualifiedName~Build_LocalPackages_WithExternalRazorSgSfcConsumer_PureDenoPipeline_PassesInIsolatedWorkspace|FullyQualifiedName~Build_LocalPackages_RazorVueTodoListSample_PureDenoPipeline_PassesInIsolatedWorkspace" -v minimal -p:UseSharedCompilation=false -m:1
```

最新结果：2 通过，0 失败，0 跳过。

```powershell
dotnet test src/Jazor.RazorVue.Test/Jazor.RazorVue.Test.csproj --filter "FullyQualifiedName~RazorVue_Pipeline_LowersVuetifyLayoutComposition|FullyQualifiedName~RazorVue_SfcArtifactFactory_LowersVuetifyCardTitle_DefaultSlot" -v minimal -p:UseSharedCompilation=false -m:1
```

最新结果：2 通过，0 失败，0 跳过；新增覆盖 `VCardTitle` 默认 slot contract，避免重新生成无效 `text` prop。

## 离上生产还差什么

### 必须完成

1. 统一 `IVueComponent` / `IVueLibraryComponent` authoring 合同，确保 tests、sample、README、SDK 集成样例完全一致。
2. 继续保持 NuGet 包 contents 合同：`Jazor` 包和 `ECMAScript.Vuetify` 包能被普通 consumer 无额外源码 hack 使用，且不携带 Razor Compiler / Razor Utilities Shared。
3. 全量回归：`Jazor.RazorVue.Test`、`Jazor.RazorVue.RazorIr.Test`、`Jazor.EmitTest` RazorVue 切片、`dotnet pack` 和 package payload guard 必须同时通过。
4. 补完整支持/unsupported 矩阵并与测试覆盖对齐，尤其是 Razor 语法、生命周期/setup 逻辑、slot、bind、事件、Vuetify 组件、source map、HMR。
5. 证明 `.razor -> .vue -> bundled JS` sourcemap 在真实浏览器调试中可用，或在上线说明中明确当前调试能力边界。

### 应该完成

1. 消除 Razor IR 测试项目 nullable 警告。
2. 在 docs 中替换旧的“默认 legacy / 单大 catalog / 100% 完成”表述，避免后续按过期计划推进。
3. 给支持矩阵补当前实际支持/unsupported 清单：Razor 语法、生命周期、setup 逻辑、slot、bind、事件、Vuetify 组件、source map、HMR。
4. 将 browser smoke 接入更上层 CI/发布流水线，避免只在 focused integration test 中执行。
5. 对 `RazorVuePipelineTests.cs`、`ESGeneratorTests.cs` 做测试文件分组，降低后续维护成本。

## 上线建议

当前不建议直接标记为通用生产可用。合理状态应调整为：

- **core ready candidate**：核心编译、IR/SFC lowering、descriptor/canonical/emit 模型和 SG tail 接入已进入候选状态。
- **library mode release candidate pending support-matrix/debug smoke**：SDK/package/emit/sample/pure-Deno build、真实浏览器 smoke、独立外部 .NET + Deno consumer 已闭合，支持矩阵、全量回归和调试闭环仍是上线前门槛。
- **not GA production ready**：在浏览器交互、支持矩阵和调试闭环完成前，不对外宣称完整生产可用。

下一轮评审的最低通过门槛：

1. `dotnet build Jazor.slnx` 0 警告或所有剩余警告有明确豁免说明。
2. 文档、README、sample 的 authoring 写法与真实类型合同一致。
3. 支持/unsupported 矩阵与实际测试覆盖一致。
4. browser smoke 和 pure Deno consumer 集成测试进入发布流水线。
5. sourcemap/source-origin 的真实浏览器调试链路有验收记录。
