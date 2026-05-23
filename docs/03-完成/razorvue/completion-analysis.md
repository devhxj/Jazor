# RazorVue 完成度与生产就绪评审

> 评审日期：2026-05-07  
> 最新更新：2026-05-22
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

## 2026-05-21 状态更新

本轮继续把 RazorVue consumer 接入链路从“能跑基本 demo”收紧到更真实的生产契约，重点落在 route template 与 consumer runtime 的一致性上：

1. `Jazor.Emit/RazorVueConsumerEntryCompiler` 的 route template -> Vue Router path 转换不再依赖手写 `{...}` 字符串拆分，而是正式切到 ASP.NET Core 官方 `Microsoft.AspNetCore.Routing.Template.TemplateParser`。这样 route grammar 的合法性与 segment/part 边界首先由 ASP.NET Core 官方 parser 决定，RazorVue 只负责“是否能诚实映射到 Vue Router path regex”这一步。
2. consumer route 支持面现已从原先的 pure literal / pure `{parameter}` / `{parameter?}` 扩到：受控 constraint（例如 `{id:int}`）、不含 optional separator 的 mixed/composite segment（例如 `post-{id}` / `post-{id:int}`），以及 catch-all（例如 `{*path}`）。
3. constraint 不再做“看起来像 constraint 就全吞掉”的弱支持，而是只对能稳定映射到 Vue Router regex path 的受控子集开放。当前已锁定的典型子集包括：`int`、`long`、`alpha`、`bool`、`required`、`length(...)` / `minlength(...)` / `maxlength(...)`、`regex(...)`。超出这一子集的 constraint 组合仍显式 fail-fast，而不是静默丢失 ASP.NET Core 约束语义。
4. whole-segment default value（如 `{id=42}`、`{id:int=42}`）现也已正式进入支持面。`RazorVueConsumerEntryCompiler` 不仅会把这类模板转换成 Vue Router optional path，还会把默认值写入 route metadata 的 `defaultParameterValues`；consumer runtime 在 route match 读取与 href 生成时会统一应用这份默认值合同，因此 `/examples` 与 `/examples/42` 都能落到 `id = "42"` 语义，而显式传入默认值时 href 仍会折叠为 `/examples`。
5. 当前继续显式拒绝的是：
   - default value 出现在 composite/mixed segment 内部，例如 `post-{id=42}`
   - 带 optional separator 的 composite/mixed segment，例如 `/files/{filename}.{ext?}`
   这里不是 parser 不会读，而是经真实 `vue-router` matcher / href-generation probe 校准后确认：这两类路径在当前 consumer/runtime 契约下不能诚实承载 ASP.NET Core 的参数提取与 URL 生成语义；继续 fail-fast 比“表面支持”更符合生产标准。
6. Playground consumer runtime 已去掉独立的简化 path matcher 和手写 `:id` 路径拼接规则。anchor interception 的 client-route 判定与 `resolveRouteHref(...)` 的 href 生成现在统一复用 `vue-router` matcher 语义，并在其外层叠加 generated route metadata 的 default-parameter contract，避免 generated `razorVueConsumerRoutes` 与真实 router 在 constrained/composite/catch-all/default-valued path 上发生语义漂移。
7. 新增 focused 回归已同时覆盖：
   - `Jazor.EmitTest` 中的 constrained / catch-all / composite / composite+constraint / optional composite separator
   - `Jazor.EmitTest` 中的 whole-segment default value / constrained default value / composite default-value rejection
   - `src/Playground/consumer/src/runtime-common.test.js` 中的 route metadata 归一化、复杂 path 匹配、default-parameter contract 与 href 生成
8. 当前 focused 验证已通过：
   - `dotnet test src/Jazor.EmitTest/Jazor.EmitTest.csproj --filter FullyQualifiedName~RazorVueConsumerEntryCompilerTests -v minimal`
   - `dotnet run --file src/Playground/consumer/scripts/run-deno.cs -- task test`
9. 同日，lifecycle 受控子集还补齐了一条真实继承链缺口：`ShouldRender` 不再只接受“当前类里直接 `return true;`”或“当前类里直接透传到 `ComponentBase.ShouldRender()`”，而是正式支持递归的安全 base-pass-through 链。例如抽象基类 `return true;`、派生 RazorVue 组件再 `return base.ShouldRender();`，现在 analyzer、generator、HMR 边界分类三层都能一致识别为受支持契约。
10. 这条扩面仍然刻意保持 fail-fast：若 base 链最终落到动态条件（例如 `return Value > 0;`）、源码不可分析、或出现递归/环引用，RazorVue 仍会把该 `ShouldRender` 链判为 unsupported，并继续要求 `FullReloadRequired`，不会因为表面上是 `base.ShouldRender()` 就静默放行。
11. 当前 focused RazorVue 回归已通过：
   - `dotnet test src/Jazor.RazorVue.Test/Jazor.RazorVue.Test.csproj --filter 'FullyQualifiedName~RazorVue_Misuse_PassThroughShouldRenderToSupportedBase_IsAccepted|FullyQualifiedName~RazorVue_Misuse_PassThroughShouldRenderToUnsupportedBase_ReportsJAZORVUE005|FullyQualifiedName~GenerateCatalog_WithPassThroughShouldRenderToSupportedBase_DoesNotReportJAZORVGA005_AndKeepsTemplateOnlyBoundary|FullyQualifiedName~GenerateCatalog_WithPassThroughShouldRenderToUnsupportedBase_DoesNotReportJAZORVGA005_AndKeepsFullReloadBoundary' -v minimal -p:UseSharedCompilation=false`
12. setup-side logic 同日还去掉了一条历史性的“helper 最多两层组合”人工限制。当前 contract 已重新校准为：同步 current-component helper 只要源码可分析、返回形状仍满足现有 setup lowering 合同、且下游 helper 也落在同一受控子集内，就会继续递归收集并 lower 到同一 setup scope；`FormatOuter -> FormatMiddle -> FormatInner` 这类三层及以上同步 helper 链现已正式支持。
13. 这条扩面没有放宽 async 语义边界。`async` helper、`Task` / `ValueTask` 返回 helper、以及 body 超出当前单表达式 / 单返回受控子集的 helper 仍然显式报 `UnsupportedSetupLogicLowering`；这里修掉的是“人工深度上限”，不是把 setup-side logic 扩成任意方法执行模型。
14. 当前 focused RazorVue 回归还补通过：
   - `dotnet test src/Jazor.RazorVue.Test/Jazor.RazorVue.Test.csproj --filter 'FullyQualifiedName~RazorVue_Pipeline_LowersThreeLevelHelperComposition|FullyQualifiedName~RazorVue_Pipeline_ThrowsCompilationIssueForAsyncInnerHelperMethod|FullyQualifiedName~GenerateCatalog_WithThreeLevelHelperComposition_GeneratesCatalogSource|FullyQualifiedName~GenerateCatalog_WithAsyncInnerHelper_ReportsStructuredDiagnostic' -v minimal -p:UseSharedCompilation=false`
15. 同日，setup-side logic 还补齐了一条此前真实缺口：current-component getter-bodied property 现在可以进入同一 setup lowering 主线。这里不是在 RazorVue 内部手拼 property 语义，而是把 property body 解析为 Roslyn `IOperation` 后继续交给 `RazorVueExpressionEmitter.EmitSetupExpression -> SemanticWalker -> Jazor.Compiler` 做 CLR-aware 表达式翻译；RazorVue 自己只负责 setup function framing、依赖排序和循环检测。
16. 当前正式支持的 property 子集是：expression-bodied property、getter accessor 中单个 `return` 的 property，以及它们之间的链式依赖；helper body 对这些 property 的引用会稳定 lower 为 `prefix()` / `basePrefix()` 这类 setup function 调用，并保持 compiler import/alias/reference 语义一致。
17. 同轮，setup-side logic 还把 declaration-initialized value-like property 纳入了正式受控支持面，但仍保持 compiler-owned lowering。对于当前组件内声明点初始化、且源码可证明保持 source-stable 的 property，RazorVue 现在会把它 lower 为 setup value binding；helper body 与 direct template expression 对这类 property 的引用会共用同一条 setup binding 主线，而不是继续拆成“helper 支持 / direct render 不支持”的裂缝语义。
18. 这条扩面同样刻意保持 fail-fast：getter property 链一旦形成循环依赖，会在编译期直接报 `UnsupportedSetupLogicLowering`；declaration-initialized value-like property / field 一旦后续再次出现可观察写入，也会直接报 `UnsupportedSetupLogicLowering`，不会静默沿首次初始化继续 hoist。当前仍明确不支持的是依赖构造/写入时序语义的 property、或 getter body 超出当前单表达式 / 单返回受控子集的场景。
19. 当前 focused RazorVue 回归继续补通过：
   - `dotnet test src/Jazor.RazorVue.Test/Jazor.RazorVue.Test.csproj --filter 'FullyQualifiedName~RazorVue_Snapshot_ContainsSupportedLogicProperties|FullyQualifiedName~RazorVue_Pipeline_LowersSupportedSetupPropertyAndHelperIntoSetupScope|FullyQualifiedName~RazorVue_Pipeline_LowersChainedSupportedSetupPropertiesIntoSetupScope|FullyQualifiedName~RazorVue_Pipeline_ThrowsCompilationIssueForCyclicSetupProperties|FullyQualifiedName~GenerateCatalog_WithSupportedSetupPropertyLowering_GeneratesCatalogSource' -v minimal -p:UseSharedCompilation=false`
20. 同一批 focused 回归现还补通过：
   - `dotnet test src/Jazor.RazorVue.Test/Jazor.RazorVue.Test.csproj --filter 'FullyQualifiedName~RazorVue_Snapshot_ClassifiesDeclarationInitializedSetupPropertyAsValueBinding|FullyQualifiedName~RazorVue_Pipeline_LowersDeclarationInitializedSetupPropertyAndHelperIntoSetupScope|FullyQualifiedName~RazorVue_Pipeline_LowersDeclarationInitializedSetupPropertyUsedDirectlyInTemplateExpression|FullyQualifiedName~RazorVue_Pipeline_ThrowsCompilationIssueForDeclarationInitializedSetupPropertyWithLaterWrites|FullyQualifiedName~RazorVue_Pipeline_ThrowsCompilationIssueForDeclarationInitializedSetupFieldWithLaterWrites|FullyQualifiedName~GenerateCatalog_WithDeclarationInitializedSetupPropertyLowering_GeneratesCatalogSource' -v minimal -p:UseSharedCompilation=false`
21. 同轮还补齐了一条 `SetParametersAsync` 的真实支持矩阵缺口：expression-bodied no-op 现在与 block-body no-op 对齐。例如 `public override Task SetParametersAsync(ParameterView parameters) => Task.CompletedTask;` 会被正式识别为“无运行时生命周期行为”，因此 generator / pipeline / HMR 边界都会保持 `TemplateOnly`，而不是继续把这种空实现误判为 unsupported。
22. 这条补齐没有放宽 `SetParametersAsync` 的执行模型。当前依然只接受 no-op、base pass-through，以及“base 后接单个受支持 `InvokeAsync` emit”这类可诚实映射到单个 Vue `watch` 的形态；重复 emit、额外 mutation、控制流或更一般的方法体仍显式 fail-fast。
23. 同轮，普通 lifecycle 的 base-pass-through 也补齐了一格此前遗漏的尾随 no-op 形态：例如 `await base.OnInitializedAsync(); return;` 这类“base 透传 + 空返回”的方法体，现在会与纯 pass-through 一样被识别为没有新增运行时行为，因此 generator / pipeline / HMR 边界会继续保持原有受支持语义，而不会因为尾随 `return;` 误判 unsupported。
24. 同轮还修正了一条 `SetParametersAsync` 的生产级保守边界：此前“派生组件 `return base.SetParametersAsync(parameters);`，但真正 base override 来自外部引用程序集、当前编译看不到源码”的场景，会被过于乐观地视为安全 pass-through。现在 analyzer / lowering / generator 已统一收紧为：只有直达 `ComponentBase.SetParametersAsync(...)` 默认实现，或源码可分析且同样受支持的 base 链，才继续接受；若最终落到外部无源码 override，则显式回到 `JAZORVUE006` / `FullReloadRequired`，不再把未知参数赋值语义误当成 no-op。
25. 当前 focused RazorVue 回归已补通过：
   - `dotnet test src/Jazor.RazorVue.Test/Jazor.RazorVue.Test.csproj --filter 'FullyQualifiedName~RazorVue_Misuse_BaseOnlySetParametersAsync_IsAccepted|FullyQualifiedName~RazorVue_Pipeline_ClassifiesTemplateOnlyBoundaryForBaseOnlySetParametersAsyncLifecycle|FullyQualifiedName~GenerateCatalog_WithBaseOnlySetParametersAsync_DoesNotReportJAZORVGA005_AndKeepsTemplateOnlyBoundary|FullyQualifiedName~RazorVue_Misuse_PassThroughSetParametersAsyncToExternalOverrideWithoutSource_ReportsJAZORVUE006|FullyQualifiedName~RazorVue_Pipeline_ClassifiesFullReloadBoundaryForSetParametersAsyncPassThroughToExternalOverrideWithoutSource|FullyQualifiedName~GenerateCatalog_WithSetParametersAsyncPassThroughToExternalOverrideWithoutSource_DoesNotReportJAZORVGA005_AndKeepsFullReloadBoundary' -v minimal -p:UseSharedCompilation=false`
26. 随后又补齐了一条 analyzer 与 lowering 的契约漂移：`SetParametersAsync` 的 expression-bodied no-op（例如 `public override Task SetParametersAsync(ParameterView parameters) => Task.CompletedTask;`）此前已经被 pipeline / generator 识别为 `TemplateOnly`，但 analyzer 仍会误报 `JAZORVUE006`。现在 analyzer 已对齐接受同一 no-op 子集，三层重新保持一致。
27. 当前 focused RazorVue 回归继续补通过：
   - `dotnet test src/Jazor.RazorVue.Test/Jazor.RazorVue.Test.csproj --filter 'FullyQualifiedName~RazorVue_Misuse_ExpressionBodiedNoOpSetParametersAsync_IsAccepted|FullyQualifiedName~RazorVue_Pipeline_ClassifiesTemplateOnlyBoundaryForExpressionBodiedNoOpSetParametersAsyncLifecycle|FullyQualifiedName~GenerateCatalog_WithExpressionBodiedNoOpSetParametersAsync_DoesNotReportJAZORVGA005_AndKeepsTemplateOnlyBoundary' -v minimal -p:UseSharedCompilation=false`
28. 本轮又修正了一条更隐蔽但更生产级的 no-op 语义漏洞：RazorVue 之前把 bare `default` 一概当成 lifecycle / `SetParametersAsync` 空实现，这对 `DisposeAsync() => default;` 这类 non-generic `ValueTask` 形态是成立的，但对 `protected override Task OnInitializedAsync() => default;` 并不成立，因为 `default(Task)` 实际是 `null`，不是 completed task。
29. 当前 analyzer / lowering / generator 已统一改为“按目标返回类型判定 no-op”：
   - `Task` 返回方法只接受真实 completed-task 形态，例如 `Task.CompletedTask`
   - non-generic `ValueTask` 返回方法继续接受 `default` / `default(ValueTask)` / `new ValueTask(...)` 包裹后的等价 no-op
   - 因此 `Task` lifecycle / `SetParametersAsync` 上的 `=> default` 现在会一致前置报错或进入 unsupported，而既有 `ValueTask DisposeAsync() => default;` 不会被误伤
30. 本轮 focused 回归已补通过：
   - `dotnet test src/Jazor.RazorVue.Test/Jazor.RazorVue.Test.csproj --filter 'FullyQualifiedName~RazorVue_Misuse_DefaultTaskOnInitializedAsync_ReportsJAZORVUE005|FullyQualifiedName~RazorVue_Misuse_DefaultValueTaskDisposeAsync_IsAccepted|FullyQualifiedName~RazorVue_Misuse_ConstantTrueShouldRender_IsAccepted|FullyQualifiedName~RazorVue_Misuse_ComponentBaseShouldRenderPassThrough_IsAccepted|FullyQualifiedName~RazorVue_Misuse_PassThroughShouldRenderToSupportedBase_IsAccepted|FullyQualifiedName~RazorVue_Misuse_PassThroughShouldRenderToUnsupportedBase_ReportsJAZORVUE005|FullyQualifiedName~RazorVue_Misuse_ExpressionBodiedNoOpSetParametersAsync_IsAccepted|FullyQualifiedName~RazorVue_Pipeline_ThrowsCompilationIssueForDefaultTaskOnInitializedAsyncLifecycle|FullyQualifiedName~GenerateCatalog_WithDefaultTaskOnInitializedAsyncLifecycle_ReportsJAZORVGA005|FullyQualifiedName~RazorVue_Pipeline_ClassifiesTemplateOnlyBoundaryForNoOpDisposeAsyncLifecycle|FullyQualifiedName~RazorVue_Pipeline_ClassifiesTemplateOnlyBoundaryForInheritedNoOpDisposeAsyncLifecycle' -v minimal -p:UseSharedCompilation=false`
31. 同一轮还补齐了一条此前真实存在的 lifecycle payload 支持裂缝：当前组件 source-stable value member 现在可以正式进入 lifecycle payload lowering，而不再只限 `[Parameter]` property。当前已锁定的受支持子集包括 declaration-initialized value-like property、declaration-initialized field，以及 getter-bodied property；它们会沿现有 setup/property/field lowering 主线进入 payload，而不是在 RazorVue 里另写一套私有 member-to-JS 拼接逻辑。
32. 随后这条 lifecycle payload 路线又继续补齐到一条受控 current-component helper/method-call 子集：只要 helper 仍是当前组件内、源码可分析、同步、非 `Task`/`ValueTask` 返回、调用点 arity 与签名完全一致，且 helper body 仍落在现有 setup helper lowering 合同内，lifecycle payload 现在也可以直接引用该 helper 调用；helper 体内部对 declaration-initialized property/field、getter-bodied property、以及其他同步 helper 的依赖，会继续并入同一 setup 依赖图，而不是在 lifecycle lowering 内另写一套私有调用语义。
33. module builder / SFC builder 同轮还修掉了一条真实初始化顺序风险：lifecycle lowering 现已先创建 lifecycle plan，并在计划阶段预收集 payload 触发的 setup property/field/method 依赖；这些 setup bindings/functions 会先发射，随后才注册 `watch(..., { immediate: true })` 与其他 lifecycle hook。这样 `OnParametersSet*` / `SetParametersAsync` 的 immediate watch 就不会在 setup binding 还未声明时提前闭包引用它们。
34. 这不是纯重构，而是生产级正确性修复。此前只要 lifecycle payload 依赖 setup member，就存在 `watch(..., { immediate: true })` 先于 setup binding 发射的 TDZ / 初始化顺序风险；当前 pipeline 与 render-function/SFC artifact 都已锁定“setup 先于 immediate watch/hook 注册”这一合同。
35. analyzer / lowering / generator 的 lifecycle 支持矩阵本轮也重新对齐：`RazorVueMisuseAnalyzer` 不再沿用旧的语法级近似规则猜测 lifecycle payload 是否受支持，而是构建同一 `RazorVueSemanticSnapshot` 并复用 lowering 侧的 support-shape 判定。这样 declaration-initialized property/field lifecycle payload 不再出现“pipeline 已支持、analyzer 仍先报 `JAZORVUE005`”的层间漂移。
36. 这条 helper payload 扩面仍刻意保持 fail-fast：`async` helper、`Task` / `ValueTask` 返回 helper、非精确 arity helper 调用、越出当前 setup lowering 合同的 helper body、mutable/later-written member，以及更宽的动态 payload 仍继续显式回到 `UnsupportedLifecycleLowering` / analyzer `JAZORVUE005`；这里关闭的是“受控 helper-call payload 不能进入 lifecycle lowering”的缺口，不是把 lifecycle lowering 扩成任意 setup helper 执行通道。
37. 当前 focused RazorVue 回归已补通过：
   - `dotnet test src/Jazor.RazorVue.Test/Jazor.RazorVue.Test.csproj --filter 'FullyQualifiedName~RazorVue_Pipeline_LowersDeclarationInitializedPropertyOnParametersSetLifecyclePayload_AndEmitsSetupBeforeWatch|FullyQualifiedName~RazorVue_Pipeline_LowersDeclarationInitializedFieldOnAfterRenderAsyncLifecyclePayload|FullyQualifiedName~RazorVue_Pipeline_LowersGetterBodiedPropertyOnInitializedLifecyclePayload|FullyQualifiedName~GenerateCatalog_WithDeclarationInitializedPropertyLifecyclePayload_LowersWithoutJAZORVGA005|FullyQualifiedName~GenerateCatalog_WithDeclarationInitializedFieldLifecyclePayload_LowersWithoutJAZORVGA005|FullyQualifiedName~RazorVue_SfcArtifactFactory_EmitsLifecycleSetupBindingsBeforeImmediateWatchRegistration|FullyQualifiedName~RazorVue_Misuse_DeclarationInitializedPropertyLifecyclePayload_IsAccepted|FullyQualifiedName~RazorVue_Misuse_DeclarationInitializedFieldLifecyclePayload_IsAccepted|FullyQualifiedName~RazorVue_Pipeline_LowersHelperCallOnParametersSetLifecyclePayload_AndEmitsSetupBeforeWatch|FullyQualifiedName~RazorVue_Pipeline_ThrowsCompilationIssueForAsyncHelperCallLifecyclePayloadOnInitialized|FullyQualifiedName~GenerateCatalog_WithHelperCallLifecyclePayload_LowersWithoutJAZORVGA005|FullyQualifiedName~GenerateCatalog_WithAsyncHelperCallLifecyclePayload_ReportsJAZORVGA005|FullyQualifiedName~RazorVue_SfcArtifactFactory_EmitsLifecycleHelperSetupBindingsBeforeImmediateWatchRegistration|FullyQualifiedName~RazorVue_Misuse_HelperCallLifecyclePayload_IsAccepted|FullyQualifiedName~RazorVue_Misuse_AsyncHelperCallLifecyclePayload_ReportsJAZORVUE005' -v minimal -p:UseSharedCompilation=false`
38. lifecycle payload 在 `OnAfterRender*` 的 `firstRender` 参数上，这轮又补齐了一条 compiler-owned fallback。当前如果 payload 实际引用 `firstRender`，且表达式形状仍落在受控子集内，RazorVue 会把该参数别名到 `currentFirstRender` 后，继续交由 `EmitSetupExpression -> SemanticWalker -> Jazor.Compiler` 完成 CLR-aware lowering，而不是在 RazorVue 内部继续堆新的手写 payload 分支。
39. 这条 fallback 当前已经锁定的真实支持形态又继续扩到：`(bool)firstRender`、`object.Equals(firstRender, true)`、`object.Equals((bool)firstRender, true)`、`firstRender.Equals(true)`、`firstRender == true`、`bool? alias = firstRender; alias ?? false` 这一类 source-stable nullable-bool local carrier、`firstRender is true/false`、`firstRender is not true/false`、`firstRender is true or false`、`firstRender is true and not false`、`firstRender is bool`、`firstRender is object`、直接 against `firstRender` 的 declaration-pattern（例如 `firstRender is bool ready && ready`）、`firstRender switch { ... }`，以及继续满足 setup helper lowering 合同的受控 helper-call payload，例如 `Normalize(firstRender)`。
40. after-render hook 自身仍保持原有 snapshot 协议：先 `const currentFirstRender = firstRender`，再翻转 `firstRender = false`；其中 `object.Equals(firstRender, true)` / `object.Equals((bool)firstRender, true)` / `firstRender.Equals(true)` 当前都会稳定 lower 为 `currentFirstRender === true`，`firstRender == true` 会稳定 lower 为 `(currentFirstRender === true)`，`alias ?? false` 会稳定 lower 为 `currentFirstRender ?? false`，`firstRender is bool ready && ready` 这一类 declaration-pattern 也会继续沿 `Jazor.Compiler` 主链 lower，保留 pattern local 绑定与复用，而 `firstRender is true/false` / `is bool` 这类 pattern 仍会落到对应 JS predicate。

## 2026-05-22 状态更新

本轮继续把 RazorVue 里仍残留的手写调用拼接收回到更一致的调用语义主线，重点是 lifecycle/setup/render 共用的 helper-call 参数绑定：

1. `RazorVueExpressionEmitter` 中 lifecycle payload、current-component helper、lifecycle local function，以及 rewrite 路径里原先基于 `string.Join(invocation.Arguments...)` 的手写调用重写，现已统一切到 shared invocation binder。
2. 这条 binder 不再假设 “`invocation.Arguments` 已按形参顺序排好”。它会显式区分“调用点源码求值顺序”和“最终 helper 调用形参落位顺序”，因此 named argument out-of-order 现在会稳定保留左到右求值顺序，再按声明顺序把值落到真正的调用槽位。
3. 同一条 shared binder 现也正式覆盖 omitted optional default 与按 Roslyn 绑定成单数组形参的 `params` 调用；这里没有再额外私造 RazorVue 参数协议，而是直接复用 Roslyn `IInvocationOperation.Arguments` 的已绑定结果。
4. `OnAfterRender*` / `firstRender` payload 下，这组 helper-call 现在也能与 after-render snapshot 协议稳定叠加：发射结果会继续 against `currentFirstRender`，而不是把 named/optional/params 包装层错误地退回到未快照的 `firstRender` 或错误的实参落位。
5. 这次修复还顺手补齐了 lifecycle 参数子表达式的 `PreludeBindings` / `UsesFirstRender` 聚合，因此子参数若触发 compiler-owned lifecycle fallback，也不会再在 helper-call 包装层把 prelude alias 或 `currentFirstRender` 使用标记丢掉。
6. 当前 focused 回归已通过：
   - `dotnet test src/Jazor.RazorVue.Test/Jazor.RazorVue.Test.csproj --filter 'FullyQualifiedName~HelperCallWithOmittedOptionalFirstRenderPayloadOnAfterRenderAsyncLifecycle|FullyQualifiedName~HelperCallWithParamsFirstRenderPayloadOnAfterRenderAsyncLifecycle|FullyQualifiedName~HelperCallWithNamedArgumentsOutOfDeclarationOrderFirstRenderPayloadOnAfterRenderAsyncLifecycle|FullyQualifiedName~GenerateCatalog_WithHelperCallOmittedOptionalFirstRenderPayloadOnAfterRenderAsyncLifecycle_LowersWithoutJAZORVGA005|FullyQualifiedName~GenerateCatalog_WithHelperCallParamsFirstRenderPayloadOnAfterRenderAsyncLifecycle_LowersWithoutJAZORVGA005|FullyQualifiedName~GenerateCatalog_WithHelperCallNamedArgumentsOutOfDeclarationOrderFirstRenderPayloadOnAfterRenderAsyncLifecycle_LowersWithoutJAZORVGA005|FullyQualifiedName~RazorVue_Pipeline_LowersCurrentComponentRenderHelperMethodWithNamedArgumentsOutOfDeclarationOrder_PreservingCallSiteEvaluationOrder|FullyQualifiedName~RazorVue_Pipeline_LowersBuildRenderTreeLocalFunctionHelperWithNamedArgumentsOutOfDeclarationOrder_PreservingCallSiteEvaluationOrder' -v minimal`
41. 这轮又继续补上了一段以前明确缺失的 after-render 深链场景，但路线仍然是 compiler-owned，而不是 RazorVue 继续手写拼装：`new ReadyEnvelope(new ReadyState(firstRender)).State.Value` 这一类直接 structural source-data-carrier 深链、以及 `var readyEnvelopes = new List<ReadyEnvelope> { ... }; readyEnvelopes[1].State.Value` 这一类 source-stable structural local/list carrier，现在会通过 `SemanticWalker.AllowStructuralSourceDataCarrierLowering` 进入受控 structural lowering，发成普通 object literal shape，并在需要时继续保留既有 CLR helper（例如 `List<T>.this[int].get`）语义。
42. 这里的边界依旧是生产级保守边界，不是“允许任意源码类型 new 出来再靠 RazorVue 拼 JS class”。新增支持只覆盖可证明为 pure-data carrier 的 source-declared class/struct：它们没有 nominal runtime identity，`typeof` / bare `is Type` / 依赖运行时类型对象的路径仍然不开放；开放的是 object creation、member-chain、结构属性访问与对应 pattern/deconstruction 可以诚实擦除成结构值的那一段。
43. 这条 compiler-owned `firstRender` fallback 随后又继续补齐了三格此前测试仍按“应报错”记录的真实能力：helper-returned deep member-chain（例如 `BuildEnvelope(firstRender).State.Value`）、direct structural property-pattern（例如 `new ReadyEnvelope(new ReadyState(firstRender)) is { State.Value: true }`）、以及 helper-returned structural property-pattern（例如 `BuildEnvelope(firstRender) is { State.Value: true }`）现在也已正式支持。它们都不是 RazorVue 手拼语义，而是继续沿 `EmitSetupExpression -> SemanticWalker` 主线完成 structural helper/property-pattern lowering，其中 pattern 场景会保留 compiler-owned 单次求值 temp，避免 helper 或 object-creation 被重复求值。
44. 这条 compiler-owned `firstRender` fallback 同步又补齐了三格此前仍按“应报错”记录的真实能力：direct structural member payload（例如 `new ReadyState(firstRender).Value`）、object-initializer structural deep member-chain（例如 `new ReadyEnvelope { State = new ReadyState(firstRender) }.State.Value`）、以及 tuple-carried structural deep member-chain（例如 `(firstRender, new ReadyState(firstRender)).Item2.Value`）现在也已正式支持。它们继续沿 `EmitSetupExpression -> SemanticWalker` 主线完成 lowering，没有在 RazorVue 内另造 CLR/type/member 语义；其中 tuple 路径仍保留当前编译器 tuple runtime-shape 合同，字段名取当前静态视图（例如 `.item2.value`）。
45. 这条 compiler-owned `firstRender` fallback 本轮又继续纠正了 4 格此前仍被测试/文档记成“应报错”的真实能力：`firstRender.ToString().Length > 0` 这一类 chained expression、`new ReadyEnvelope(new ReadyState(firstRender)).State.Value.Equals(true)` 这一类 structural deep-member equals、`BuildReady(firstRender).Value.Equals(true)` 这一类 helper-returned equals、以及 `(new ReadyEnvelope { State = new ReadyState(firstRender) }.State?.Value) ?? false` 这一类 null-conditional + coalesced structural payload，现在都已沿 `EmitSetupExpression -> SemanticWalker` 主线正式支持。
46. 随后这条 `firstRender` compiler-owned fallback 又补齐了 3 格之前真实缺失、但现在已 focused 回归锁定为正式支持的 payload：source-stable tuple deconstruction local（例如 `var pair = (firstRender, new ReadyState(firstRender)); var (_, readyState) = pair; readyState.Value`）、source-stable local function（例如 `bool NormalizeReady(bool value) => value; NormalizeReady(firstRender)`）、以及 source-stable local lambda / delegate local（例如 `Func<bool, bool> normalizeReady = static value => value; normalizeReady(firstRender)`）。这些场景都不是 RazorVue 再造私有 JS 协议，而是先在 lifecycle prelude 中发射稳定 alias，再继续交回 `EmitSetupExpression -> SemanticWalker -> Jazor.Compiler` 完成 CLR-aware lowering。
47. 因此当前仍显式 fail-fast 的重点剩余项进一步收窄为：`async` local helper / local lambda、`Task` / `ValueTask` 返回 local helper、无法从源码稳定恢复初始化器或声明前缀的 callable local、非精确 arity 的 current-component helper/local helper 调用、依赖额外 source-stable object boxing/local carrier 的 declaration-pattern / pattern-var，以及更宽 dataflow 形状。这里继续遵守“只有现有 compiler-owned lowering 真能承载的 firstRender 表达式才开放”，不发明 RazorVue 私有语义。

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
9. handwritten `BuildRenderTree` 中局部 typed `RenderFragment<T>` carrier 的“先声明、再在同一线性局部声明前缀内完成一次简单赋值”窄模式，现已补齐 mixed imperative 路径：即使声明/赋值出现在 declarative 前缀，而真正消费出现在后续 imperative segment，这条 carrier 仍会按同一静态合同被恢复，不会因为 segmentation 把初始化来源丢失；本轮进一步锁定了 `RenderFragment<int> template; var revision = 0; template = CreateTemplate(Title);` 这一 sibling-local declaration 变体。
10. 同一条局部 carrier 合同现在已在 BuildRenderTree template frontend / mixed imperative segmentation / pipeline lowering 三条线上统一收口：若“先声明、再在同一线性局部声明前缀内完成一次简单赋值”的 local `RenderFragment` / `RenderFragment<T>` 在后续再次出现可观察写入，RazorVue 会显式 fail-fast，而不会继续静默沿第一次赋值恢复旧模板。
11. RazorVue 两套高频测试宿主现已补上 metadata reference 进程级缓存；fresh full `dotnet test -p:UseSharedCompilation=false` 不再因每个测试重复 `MetadataReference.CreateFromFile(...)` 而在 Roslyn metadata 装载阶段触发 OOM，验证基线回到可重复状态。
12. 静态 `MarkupString` local carrier 现已与 `RenderFragment` carrier 的 source-stable 窄模式进一步收敛：handwritten `BuildRenderTree` 与 Razor IR authored template / pipeline / SFC 路线都支持“先声明、再在同一线性局部声明前缀内完成一次简单赋值”的 `MarkupString` local，再由 `AddContent(...)` 或 `@markup` 消费；本轮显式补齐了 sibling local declaration 变体，例如 `MarkupString markup; var revision = 0; markup = ...;`。若后续再次出现可观察写入，则统一 fail-fast，而不是回退成通用 assignment unsupported。
13. Razor IR authored template 下的本地 `RenderFragment` / `RenderFragment<T>` carrier 现在也已正式补齐这条 source-stable 窄模式：`RenderFragment<T> template; template = ...;` 可继续赋给组件 typed slot/template 参数并贯通 render tree / `.mjs` pipeline；本轮又补齐并锁定了 declaration-prefix widening，即允许在赋值前继续出现 sibling local declarations，同时保持 immediate-assignment 右侧来自 current-component member carrier 与受支持 fragment factory 返回值的变体，不再错误退回 imperative tail。若不能在同一线性局部声明前缀内完成这次赋值，或后续再次出现可观察写入，则同样显式 fail-fast。
14. Razor IR authored template 中 direct untyped `RenderFragment` expression consumption 现已正式补齐：`@Template`、`@template` 这类 current-component member / source-stable local carrier 会直接还原为结构化 render subtree，不再重复输出同一模板体、把普通 member 误判成 slot outlet，或把 immediate-assignment local 错误退回 imperative tail。对于 `private RenderFragment Template => @<...>` 这类 property initializer，如果 Razor SG 生成后只剩 builder lambda 且 direct operation-level source mapping 缺失，当前实现会明确依赖 shared builder parser fallback，而不会再把这类 direct source-map 缺失误当成“功能不支持”。
15. Razor IR authored template 中 direct typed `RenderFragment<T>` invocation 现也已正式补齐：`@Template(42)`、`@template(42)` 这类 current-component member / source-stable local carrier，以及 `@CreateTemplate(Title)(42)`、`@CreateTemplate()(42)`、`@CreateTemplate(subtitle: Subtitle, title: Title)(42)` 这类直接调用当前组件 fragment factory 返回值的 authored 语法，都会直接还原为 typed fragment scope，并继续保留 factory/member captured-value scope，而不会退化成普通 invocation 表达式或在 canonical/SFC 阶段触发 unsupported member/property 错误；其中 named argument out-of-order 也会保留调用点求值顺序。
16. Razor IR authored template 中 direct typed slot outlet invocation 也已对齐 handwritten `BuildRenderTree`：`@Header(Count + 1)` 这类当前组件 `[Parameter] RenderFragment<T>?` slot source 会直接还原为带 argument 的 slot outlet，并最终稳定 lower 为 `<slot name="header" :value="(props.count + 1)" />`，而不会再退化成普通插值表达式。
17. Razor IR authored template code-block 中“局部 `RenderFragment` carrier + 同块 local function fragment factory 声明”现也已正式补齐：`@{ RenderFragment<int> template = CreateTemplate(Title); RenderFragment<int> CreateTemplate(string? title) => item => @<span>@title @item</span>; }` 这类 authored 形态会与 `@code` factory 保持同一 captured-value scope + typed fragment scope 语义，不再因为 local function 声明残片、尾随 `;` 或内部 `@<...>` 模板节点未消费而把它们泄漏成 render tree 根节点；这条修复同时把 local-function-authored template 节点消费从 syntax 级扩大到 operation-coverage 级 source range 标记，避免 Razor IR 边界切分差异再次漏网。
18. 在同一补齐面上，Razor IR authored template 现在也支持“template code-block 内 local function fragment factory 的 direct typed invocation”这一原生 authoring：`@{ RenderFragment<int> CreateTemplate(string? title) => item => @<span>@title @item</span>; } @CreateTemplate(Title)(42)` 会和当前组件 `@code` / member factory 一样直接还原为“外层 captured-value scope + 内层 typed fragment scope”，并贯通 render tree / parity / SFC 路线。为此，template code-block 前缀扫描现已把 pure local-function declaration block 视为已绑定声明前缀，而不会再把这类只含 local function 声明的 code-block 错误留成 unbound `CSharpCodeIntermediateNode`。
19. 同一条 Razor IR authored template expression 路径现已显式锁定 direct untyped fragment factory consumption：`@CreateTemplate(Title)` 以及 `@{ RenderFragment CreateTemplate(string? title) => @<section><span>@title</span><p>ok</p></section>; } @CreateTemplate(Title)` 这类当前组件 method / template code-block local function factory 直接消费返回值的 authored 语法，都会还原为“外层 captured-value scope + 内层结构化 render subtree”，并已通过 render tree / parity / SFC 回归覆盖。该能力本轮确认是已有 lowering 合同可达但此前未被测试与文档显式锁定，而非新增 wrapper 协议或旁路实现。
20. 在同一 untyped direct-factory 路径上，zero-argument 与 named-argument out-of-order 变体也已正式锁定：`@CreateTemplate()` 与 `@CreateTemplate(subtitle: Subtitle, title: Title)` 会和 typed 版本一样保留“按调用点求值顺序嵌套 captured scope + 内层结构化 render subtree”的语义，并已补齐 render tree / parity / SFC 回归。这再次证明当前缺的主要是支持矩阵显式化，而不是再造新的 lowering 分支。
21. 同一条 untyped direct-expression 语义现还补齐并锁定了“factory-backed member/local carrier 再直接消费”的 authored 形态：`private RenderFragment Template => CreateTemplate(Title); @Template` 与 `RenderFragment template; template = CreateTemplate(Title); @template` 这两类组合不会在 property/local 这一层把 factory captured scope 丢掉，而是继续还原为“外层 captured scope + 内层结构化 render subtree”，并已通过 render tree / parity / SFC 回归固定下来。
22. count-style `for` 的“动态步长表达式”边界本轮也已重新校准并锁定：`i += GetStep()` 与 `i = i + GetStep()` 这类 authored form 当前主线早已支持，并贯通 Razor IR render tree、preferred frontend parity、pipeline `.mjs`、canonical/SFC semantic 与 `.vue` artifact。真实合同不是“step 必须静态简单”，而是“iterator 结构必须仍能归一到现有 count-style 协议，且步进表达式按进入 range helper 前单次求值处理”。
23. 随后又补齐了一条此前仍存在的 frontend 过早拒绝缺口：多 iterator / 非 count-style `for` 不再一律等同于“整体 unsupported”。当前如果 `for` 头无法归一到 `RazorVueForNode` / `__jazorVueForRange(...)` 这条声明式 count-style 合同，但整个循环本身仍落在现有同步 imperative render artifact contract 内，例如 `for (var index = 0; index < Count; index++, total++)`，RazorVue 现在会把它直接切到 `RazorVueImperativeBlockNode` / render-context imperative bridge / render-function `.vue` 主线，而不是继续在 frontend 上报旧的 count-style unsupported。
24. 这条扩面没有放宽 count-style runtime helper 协议本身。多 iterator、`i = i * step`、`i = Next(i)` 这类形态仍不会被错误塞回 `__jazorVueForRange(...)`；它们只是改为“能由现有同步 imperative render 主线诚实承载时就走 imperative”，而不是继续被声明式 analyzer 误伤。真正仍未支持的是需要逐轮重算步进的新 runtime 协议，以及 async imperative render contract。
25. 同类 frontend 分流缺口本轮还在 Razor IR direct `@foreach` 上补齐了一格：此前 direct `@foreach` 主要只在 body 可结构化时稳定保留 `RazorVueForEachNode`，一旦循环体里出现 `break` / `continue` 等 imperative-only 语义，就可能在 frontend 阶段提前掉回 unsupported。现在这类 authored form 已与非 count-style `for` 保持同一处理原则：能 declarative 就 declarative，不能 declarative 但仍落在现有同步 imperative render contract 内时，直接切到 `RazorVueImperativeBlockNode` / render-context imperative bridge / render-function `.vue` 主线，而不是把结构化失败误当成整体不支持 `foreach`。
23. handwritten `BuildRenderTree` 静态 `AddMarkupContent(...)` string local carrier 现也已与既有 source-stable 合同对齐：除 direct literal、`const string`、只读 member、以及“private mutable + 无后续写入” member 外，普通 declaration-initialized `string` local 与 `string markup; markup = "...";` 这类在同一线性局部声明前缀内完成一次简单赋值的 local 现在也会被稳定识别为 compile-time provable static markup，并贯通 render tree / pipeline / SFC / imperative bridge；本轮补齐的 sibling-local declaration 变体同样纳入支持。若该 local 后续再次出现可观察写入，则会显式报 source-stable fail-fast，而不是退回成泛化的 `AddMarkupContent(...)` unsupported。
24. 静态 markup 的 factory-backed carrier 本轮已补齐到一个更完整但仍严格受控的子集：current-component method / local function 只要返回值本身仍可源码还原为 compile-time provable static `string` / `MarkupString`，现在即可直接被 `AddMarkupContent(...)`、`AddContent(..., MarkupString)`、Razor IR 静态 `MarkupString` 表达式位，以及 imperative render bridge 消费，并贯通 render tree / `.mjs` pipeline / SFC artifact；对带普通按值参数、omitted optional default，以及按 Roslyn 绑定为单数组形参的 `params` 调用的 factory，shared helper 现都会额外提取调用点 captured bindings，并按实参左到右求值顺序把它们包成 template scope / IIFE 后再落到最终静态 subtree，因此不会再因为“返回值看起来静态”而把调用点求值语义错误擦除。当前仍明确不支持的是递归、`ref/out/in`、实参与形参无法按当前合同绑定、或返回值本身已不再可静态证明的 static-markup factory。
25. 同一条静态-markup 解析链现也补齐并锁定了“factory-backed member/local carrier 再消费”的组合形态：`private string HeroMarkup => CreateMarkup(); builder.AddMarkupContent(..., HeroMarkup);`、`MarkupString markup; markup = CreateMarkup(); builder.AddContent(..., markup);`、`builder.AddMarkupContent(..., CreateMarkup(Title));`、`builder.AddContent(..., CreateMarkup(Title));`，以及 imperative bridge 中的等价 property/local 组合，都会继续沿 source-stable local / controlled member / static-factory 的同一递归链还原最终静态 subtree，并在需要时保留调用点 captured-binding scope，而不会在 property/local 这一层丢失 factory-return 解析上下文并误报 unsupported。这次修复是 shared helper 的递归上下文补全与 captured-binding 传播，不是新增独立 lowering 分支。
26. Razor authored root template code-block 与 typed child-content/template-body 在“local declaration + imperative tail”上的真实语义差异本轮也已校准并锁定：typed child-content/template body 仍保留“声明式 local 前缀 + imperative tail”分裂结构，而 root template 里像 `@{ var localTitle = Title; _count++; } <section>@localTitle @_count</section>`、`@{ var localTitle = Title; if (Hide) { return; } } <section>@localTitle</section>`、以及 `@{ var localTitle = Title; var index = 0; while (index < Count) { <section>@localTitle @index</section>; index++; } } <footer>@localTitle @index</footer>` 这类 authored form 当前都会整体提升为单个 imperative render block，并贯通 render tree / parity / `.mjs` / render-function `.vue`。这不是功能缺口，而是 root 级求值顺序与 local 可见性保留的既有实现选择，文档此前只是没有把该差异说清楚。

## 2026-05-22 状态更新

本轮继续收紧 handwritten `BuildRenderTree` render-helper 的真实支持边界，重点不是“放开任意 caller frame 协议”，而是把 compiler-owned/canonicalizable 子集做成可验证的生产合同：

1. current-component / local render helper 在“恰好一个 `RenderTreeBuilder` 参数 + 额外普通按值参数”场景下，除既有 self-contained fragment 外，现已正式支持 caller-owned open node 的受控 mutation 子集：
   - `AddAttribute(...)`
   - `SetKey(...)`
   - `AddMultipleAttributes(...)`
2. 这条扩面没有退回 ad hoc JS 拼接，也没有把 helper 参数直接在 frontend 阶段粗暴改写成调用点表达式。render tree 仍保留“helper 形参引用 + 节点级 captured binding”合同，再由 canonical / H / SFC 统一承接单次求值、作用域与 setup-binding 语义。
3. self-contained extra-parameter helper 的主路径同时回到统一的外层 template-scope 包裹语义；没有为了支持 caller-owned mutation 而把整段 helper body 打散成节点级替换，避免普通子节点 / 局部模板作用域 / loop-scope 的 helper 参数语义发生漂移。
4. caller-owned mutation helper 当前是显式受控子集，而不是完整 caller frame 协议：
   - 允许只读/只写当前 caller-owned node props surface 的 mutation
   - 允许在 helper 内临时进入子 frame 生成 child subtree，只要最终回到进入时的同一 caller-owned open node/frame
   - 仍显式拒绝 `OpenElement` / `OpenComponent` / `CloseElement` / `CloseComponent` / `OpenRegion` / `CloseRegion`
   - helper 结束后若 frame depth 或 active open node 与进入时不一致，会直接 fail-fast
5. H lowering 对这条 captured-binding 合同也补齐了稳定规约：
   - 当节点表达式只是单个 helper 形参 identity 引用时，会折叠回直接调用点实参，例如 `"class": props.title`、`"key": props.title`
   - `AddMultipleAttributes(...)` 仍沿既有 Blazor-style `__jazorVueMergeAttributes(...)` 主线 lower，而不是为了“看起来更短”绕开 merge 合同
   - 当同一 helper 调用既修改 caller-owned node，又向该 node 追加 child emission 时，lowering 不会再对同一 helper 实参做多次节点级内联求值；render tree 会保留 invocation-scoped replay，再由 imperative render bridge 统一重放
   - 当 caller-owned open node 是 component，default-slot 也进入同一 replay contract：implicit `ChildContent` assignment 与 ambient default-slot child 都会保留为 component-slot 语义，再由 imperative render path 发射成 `setComponentParameter("ChildContent", ...)`；不会再把 default-slot subtree 错误 materialize 成 component raw children / `append(...)`
   - 同一条 component replay contract 现也已显式锁定到 named slot / typed slot：`Header` 这类命名 slot 会发射成 `setComponentParameter("Header", () => ...)`，`ItemTemplate` 这类 typed/scoped slot 会发射成 `setComponentParameter("ItemTemplate", (item) => ...)`，并保留 helper captured-value scope 与 slot context 参数
   - 但这并不意味着“component caller-owned helper 一碰 slot 就必须走 imperative”。如果 helper 内只是把当前组件 `[Parameter] RenderFragment...` 继续 forward 给子组件 slot，且该形状仍可诚实 canonicalize，那么当前主线会保留 declarative forwarded-slot lowering，例如 `itemTemplate: (context) => slots.itemTemplate ? slots.itemTemplate(context) : null`；不会因为 helper 抽取而无谓退化到 render-function
6. 这条 mixed mutation + child emission 路线当前是刻意走 render-function / imperative render contract，而不是继续强塞回 template：
   - canonical model 会把 scoped replay 视为 imperative root program
   - SFC artifact 会显式切到 `RenderMode = RenderFunction`
   - imperative emitter 会按 ordered replay operations 重放 open node，而不是重新读取 declarative attrs/children 造成 helper 参数越界或重复求值
7. 当前 focused 回归已补齐并通过：
   - render tree frontend: current-component / local helper 的 caller-owned `AddAttribute`、`SetKey`、`AddMultipleAttributes`
   - pipeline/H output: current-component / local helper 的 caller-owned `AddAttribute`
   - pipeline/H output: current-component helper 的 caller-owned `SetKey`、`AddMultipleAttributes`
   - canonical / SFC / pipeline: current-component helper 的 caller-owned `AddAttribute + child emission`
   - render tree / canonical / SFC / pipeline: current-component helper 的 caller-owned implicit default-slot assignment
   - render tree / canonical / SFC / pipeline: current-component helper 的 caller-owned ambient default-slot child
   - render tree / canonical / SFC / pipeline: current-component helper 的 caller-owned named slot / typed slot assignment
   - render tree / pipeline: `BuildRenderTree` local function helper 的 caller-owned named slot / typed slot assignment
   - render tree / canonical / pipeline: current-component helper 的 caller-owned typed/named slot forwarding 继续保持 declarative forwarded-slot lowering，而不是被误提升为 imperative root
   - 与既有 extra-parameter helper 主路径回归一起验证，确保 self-contained helper 参数作用域未被这次修复回归破坏
8. 当前 focused 验证已通过：
   - `dotnet test src/Jazor.RazorVue.Test/Jazor.RazorVue.Test.csproj --filter 'FullyQualifiedName~CreateRenderTree_WithBuildRenderTreeLocalFunctionHelperRequiringExtraParametersAndCallerOwnedAttributeMutation_PreservesOpenElementAttributes|FullyQualifiedName~CreateRenderTree_WithCurrentComponentRenderHelperMethodRequiringExtraParametersAndCallerOwnedAttributeMutation_PreservesOpenElementAttributes|FullyQualifiedName~CreateRenderTree_WithCurrentComponentRenderHelperMethodRequiringExtraParametersAndCallerOwnedSetKey_PreservesOpenElementKey|FullyQualifiedName~CreateRenderTree_WithCurrentComponentRenderHelperMethodRequiringExtraParametersAndCallerOwnedAddMultipleAttributes_PreservesOpenElementSpread|FullyQualifiedName~RazorVue_Pipeline_LowersCurrentComponentRenderHelperWithExtraParameterAndCallerOwnedAttributeMutation|FullyQualifiedName~RazorVue_Pipeline_LowersBuildRenderTreeLocalFunctionHelperWithExtraParameterAndCallerOwnedAttributeMutation|FullyQualifiedName~RazorVue_Pipeline_LowersCurrentComponentRenderHelperWithExtraParameterAndCallerOwnedSetKey|FullyQualifiedName~RazorVue_Pipeline_LowersCurrentComponentRenderHelperWithExtraParameterAndCallerOwnedAddMultipleAttributes|FullyQualifiedName~CreateRenderTree_WithBuildRenderTreeLocalFunctionHelperRequiringExtraParameters_ProducesStructuredNodes|FullyQualifiedName~CreateRenderTree_WithCurrentComponentRenderHelperMethodRequiringExtraParameters_ProducesStructuredNodes|FullyQualifiedName~RazorVue_Pipeline_LowersCurrentComponentRenderHelperMethodWithExtraParameters|FullyQualifiedName~RazorVue_Pipeline_LowersBuildRenderTreeLocalFunctionHelperWithExtraParameters' -v minimal`
   - `dotnet test src/Jazor.RazorVue.Test/Jazor.RazorVue.Test.csproj --filter 'FullyQualifiedName~CreateRenderTree_WithCurrentComponentRenderHelperMethodRequiringExtraParametersAndCallerOwnedAttributeMutationPlusChildEmission_PreservesOpenElementShape|FullyQualifiedName~CreateRenderTree_WithBuildRenderTreeLocalFunctionHelperRequiringExtraParametersAndCallerOwnedAttributeMutationPlusChildEmission_PreservesOpenElementShape|FullyQualifiedName~RazorVue_CanonicalModelFactory_CreatesImperativeRootProgram_ForCurrentComponentRenderHelperWithExtraParameterAndCallerOwnedAttributeMutationPlusChildEmission|FullyQualifiedName~RazorVue_SfcArtifactFactory_WithCurrentComponentRenderHelperExtraParameterAndCallerOwnedAttributeMutationPlusChildEmission_LowersRenderFunctionVueSfc|FullyQualifiedName~RazorVue_Pipeline_LowersCurrentComponentRenderHelperWithExtraParameterAndCallerOwnedAttributeMutationPlusChildEmission_IntoRenderFunction' -v minimal`
   - `dotnet test src/Jazor.RazorVue.Test/Jazor.RazorVue.Test.csproj --filter 'FullyQualifiedName~CreateRenderTree_WithCurrentComponentRenderHelperMethodRequiringExtraParametersAndCallerOwnedImplicitDefaultSlotAssignment_PreservesOpenComponentDefaultSlotShape|FullyQualifiedName~CreateRenderTree_WithCurrentComponentRenderHelperMethodRequiringExtraParametersAndCallerOwnedAmbientDefaultSlotChild_PreservesOpenComponentDefaultSlotShape|FullyQualifiedName~RazorVue_CanonicalModelFactory_CreatesImperativeRootProgram_ForCurrentComponentRenderHelperWithExtraParameterAndCallerOwnedImplicitDefaultSlotAssignment|FullyQualifiedName~RazorVue_CanonicalModelFactory_CreatesImperativeRootProgram_ForCurrentComponentRenderHelperWithExtraParameterAndCallerOwnedAmbientDefaultSlotChild|FullyQualifiedName~RazorVue_SfcArtifactFactory_WithCurrentComponentRenderHelperExtraParameterAndCallerOwnedImplicitDefaultSlotAssignment_LowersRenderFunctionVueSfc|FullyQualifiedName~RazorVue_SfcArtifactFactory_WithCurrentComponentRenderHelperExtraParameterAndCallerOwnedAmbientDefaultSlotChild_LowersRenderFunctionVueSfc|FullyQualifiedName~RazorVue_Pipeline_LowersCurrentComponentRenderHelperWithExtraParameterAndCallerOwnedImplicitDefaultSlotAssignment_IntoRenderFunction|FullyQualifiedName~RazorVue_Pipeline_LowersCurrentComponentRenderHelperWithExtraParameterAndCallerOwnedAmbientDefaultSlotChild_IntoRenderFunction' -v minimal`
   - `dotnet test src/Jazor.RazorVue.Test/Jazor.RazorVue.Test.csproj --filter 'FullyQualifiedName~CreateRenderTree_WithCurrentComponentRenderHelperMethodRequiringExtraParametersAndCallerOwnedNamedAndTypedSlotAssignments_PreservesOpenComponentSlotShape|FullyQualifiedName~CreateRenderTree_WithBuildRenderTreeLocalFunctionHelperRequiringExtraParametersAndCallerOwnedNamedAndTypedSlotAssignments_PreservesOpenComponentSlotShape|FullyQualifiedName~RazorVue_CanonicalModelFactory_CreatesImperativeRootProgram_ForCurrentComponentRenderHelperWithExtraParameterAndCallerOwnedNamedAndTypedSlotAssignments|FullyQualifiedName~RazorVue_SfcArtifactFactory_WithCurrentComponentRenderHelperExtraParameterAndCallerOwnedNamedAndTypedSlotAssignments_LowersRenderFunctionVueSfc|FullyQualifiedName~RazorVue_Pipeline_LowersCurrentComponentRenderHelperWithExtraParameterAndCallerOwnedNamedAndTypedSlotAssignments_IntoRenderFunction|FullyQualifiedName~RazorVue_Pipeline_LowersBuildRenderTreeLocalFunctionHelperWithExtraParameterAndCallerOwnedNamedAndTypedSlotAssignments_IntoRenderFunction' -v minimal`
   - `dotnet test src/Jazor.RazorVue.Test/Jazor.RazorVue.Test.csproj --filter 'FullyQualifiedName~CreateRenderTree_WithCurrentComponentRenderHelperMethodAndCallerOwnedScopedSlotForwarding_PreservesForwardedSlotAttribute|FullyQualifiedName~CreateRenderTree_WithCurrentComponentRenderHelperMethodAndCallerOwnedNamedSlotForwardingViaAddComponentParameter_PreservesForwardedSlotAttribute|FullyQualifiedName~RazorVue_CanonicalModelFactory_MapsCurrentComponentRenderHelperAndCallerOwnedScopedSlotForwarding_ToForwardedSlotBinding|FullyQualifiedName~RazorVue_Pipeline_LowersCurrentComponentRenderHelperAndCallerOwnedScopedSlotForwarding_Declaratively|FullyQualifiedName~RazorVue_Pipeline_LowersCurrentComponentRenderHelperAndCallerOwnedNamedSlotForwardingViaAddComponentParameter_Declaratively' -v minimal`

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
- `ShouldRender`、`SetParametersAsync`、复杂字段/方法、复杂局部变量、复杂 slot/lifted binding 等仍有显式 unsupported/fail-fast 路径；其中 `ShouldRender` 目前已支持“`return true;` / `ComponentBase` pass-through / 递归安全 base-pass-through 链”这一受控子集，但仍不支持任意动态条件。
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
