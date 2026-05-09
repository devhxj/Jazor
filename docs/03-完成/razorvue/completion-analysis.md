# RazorVue 完成度与生产就绪评审

> 评审日期：2026-05-07  
> 最新更新：2026-05-09
> 评审范围：`src/Jazor.RazorVue/`、`src/Jazor.Analyzer/RazorVue/`、`src/Jazor.Emit` 的 RazorVue 路径、`src/Jazor.RazorVue.Test/`、`src/Jazor.RazorVue.RazorIr.Test/`、`src/Jazor.EmitTest/` 的 RazorVue 切片、`src/Jolt.Test` 的 Volar/VueAnalysis/JazorVue 切片、`samples/RazorVue.TodoList/`  
> 基线说明：本评审基于当前工作区状态。当前工作区存在多处 RazorVue/emit/VueRoute 相关未提交修改，因此结论不等同于已发布包基线。

## 结论

RazorVue 不能继续按历史文档里的“100% 完成”口径描述。当前更准确的判断是：

- **核心语义与 lowering 主链：约 85% 完成**。组件发现、descriptor、Razor IR 优先模板前端、canonical H、SFC semantic model、SFC artifact、legacy/SFC 双 catalog、source-origin/hash/HMR 元数据等主干已经成型，并有大量单元测试覆盖。
- **库模式生产接入：关键 P0 已解除，但仍未完成最终上线门槛**。Razor SG tail 注入、当前 context 级接管判断、package payload 守卫、emit/SDK RazorVue 集成切片、TodoList sample/Vite build/SSR render smoke、独立外部 NuGet `.razor` SFC consumer 已通过；剩余重点是真实浏览器 smoke、全量回归、支持矩阵和调试闭环。
- **Jolt 关联能力：局部可用，不能替代库模式验收**。Volar/VueAnalysis/virtual artifact 过滤测试通过，但这只证明 Jolt 相关局部协议和投影切片，不证明 RazorVue NuGet/SDK 生产消费链路闭合。

当前建议状态：

| 维度 | 状态 | 说明 |
|------|------|------|
| 核心库编译 | 通过 | `Jazor.RazorVue` 可构建 |
| RazorVue 单元测试 | 通过 | 557 通过 |
| Razor IR 前端测试 | 通过 | 96 通过 |
| Jolt 关联过滤测试 | 通过 | 35 通过 |
| solution 构建 | 通过但有警告 | 1 个 Razor IR 测试项目 nullable 警告 |
| emit/SDK RazorVue 集成 | 通过 | RazorVue 过滤切片 45/45 通过 |
| NuGet payload | 通过当前守卫 | `Jazor` 包 analyzer/lib payload 不携带 Razor Compiler / Razor Utilities Shared |
| 生产发布判断 | **接近但未达最终上线标准** | SDK/package/emit/sample/Vite/SSR render/独立外部 .NET consumer 已闭合；浏览器 smoke、全量回归和支持边界仍需最终确认 |

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
4. `samples/RazorVue.TodoList/build-local.ps1` 已用本地 pack 的 `Jazor` / `ECMAScript.Vuetify` 包重新跑通；当前样例生成 2 个 SFC artifact、manifest、host requirements module 和 sidecar。
5. 生成的 `todo-app.vue` 已恢复完整嵌套 Vuetify 结构，包含 `VRow` / `VCol` / `VCard` / `VTextField` / `VList` / `VListItem`，并包含 `item.Title` / `item.IsDone` / `item.Category` / `item.IsPinned` 等 DTO 属性投影。
6. `samples/RazorVue.TodoList/todo-consumer` 已切到纯 Deno consumer 验证链；`npm run build` 通过，底层实际调用仓库内 bundled `deno.exe`，证明当前生成的 `.vue` 可以先经 Deno 侧 SFC 预编译，再通过 `deno bundle` 产出浏览器 JS/CSS。
7. 新增 `Build_LocalPackages_WithExternalRazorSgSfcConsumer_EmitsVueSfcArtifacts`，用独立临时 consumer 通过本地 NuGet 包消费 `Jazor` / `ECMAScript.Vuetify`，显式启用 `UseRazorSourceGenerator=true`、`JazorRazorVueEnableRazorSgIntegration=true`、`JazorRazorVueOutputMode=sfc`，并验证 `.razor` authoring 生成 `.vue`、manifest、host requirements module、source map 和 origins sidecar。
8. SFC template lowering 已修正 component literal prop 语义：组件非字符串 literal（例如 Vuetify `bool` / `number` props）输出为 Vue bound props，如 `:fluid="true"`、`:cols="12"`；字符串 literal 仍输出静态属性，如 `title="Inbox"`。
9. `samples/RazorVue.TodoList/todo-consumer` 已执行 `npm run smoke:ssr`，通过纯 Deno 的 Vue server renderer + Vuetify 对生成 SFC 做 runtime render smoke，且不再出现 `fluid` Boolean prop 类型 warning。
10. `samples/RazorVue.TodoList/todo-consumer` 已执行 `npm run smoke:bundle-api`，证明 `Deno.bundle()` 也能消费 Deno 预编译后的 RazorVue entry，产出 JS/CSS 与 source map。

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

1. `samples/RazorVue.TodoList/build-local.ps1` 从本地 pack 出来的 `Jazor` / `ECMAScript.Vuetify` 包恢复并构建成功。
2. 构建产出 `.vue`、manifest、host requirements module 和 sidecar。
3. 不依赖仓库源码引用、不依赖本机全局缓存残留、不依赖旧 legacy `.mjs` 默认假设。
4. `todo-consumer` 纯 Deno build 能消费生成的 `.vue` 与 host requirements module。
5. `todo-consumer` SSR smoke 能通过 Deno 预编译后的 Vue module、Vue server renderer 和 Vuetify plugin 渲染 DTO 投影文本，并验证 host requirements 中包含 `vuetify` plugin 和 `vuetify/styles`。
6. `todo-consumer` `Deno.bundle()` smoke 已通过，说明同一套 Deno 预编译入口既能走 CLI bundle，也能走 runtime bundle API。
6. `todo-app.vue` 中 Vuetify Boolean / numeric props 已以 Vue binding 输出，避免把 `bool` / `number` 传成字符串导致运行时 prop warning。

### 已解除：独立外部 .NET NuGet/SFC consumer smoke

`Build_LocalPackages_WithExternalRazorSgSfcConsumer_EmitsVueSfcArtifacts` 已覆盖一个独立临时 consumer：

1. 不复用仓库 sample 源码。
2. 只通过本地 pack 的 `Jazor` / `ECMAScript.Vuetify` NuGet 包消费 RazorVue。
3. 使用 `Microsoft.NET.Sdk.Razor`、官方 Razor SG、`.razor + .razor.cs` authoring 和 SFC 输出模式。
4. 验证输出 `components/external-dashboard.vue`，并确认不会同时产出 legacy `external-dashboard.mjs`。
5. 验证 host requirements module、RazorVue manifest、SFC source map 和 origins sidecar。

仍未自动化的是“独立外部 Vite consumer build”。当前已有仓库内 TodoList Vite build 证明真实 Vue/Vuetify 工具链可消费生成 `.vue`，但独立临时 consumer 的 Vite build 还未纳入测试主链。

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
- SSR 目前已有 TodoList render smoke；hydration、浏览器挂载与交互仍是待验证生产能力。

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

最新结果：557 通过，0 失败，0 跳过。

```powershell
dotnet test src/Jazor.RazorVue.RazorIr.Test/Jazor.RazorVue.RazorIr.Test.csproj -v minimal -p:UseSharedCompilation=false
```

最新结果：96 通过，0 失败，0 跳过。

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
pwsh ./samples/RazorVue.TodoList/build-local.ps1
```

最新结果：通过，`assemblies=15 catalogs=2 razorvueSfcCatalogs=1 modules=51 razorvueSfcArtifacts=2 written=3 skipped=51 deleted=0`。

```powershell
cd samples/RazorVue.TodoList/todo-consumer
npm run build
```

最新结果：通过，`deno bundle` 产出浏览器 JS/CSS。

```powershell
cd samples/RazorVue.TodoList/todo-consumer
npm run smoke:ssr
```

最新结果：通过，`RazorVue TodoList Deno SSR smoke passed.`，无 Vue prop 类型 warning。

```powershell
cd samples/RazorVue.TodoList/todo-consumer
npm run smoke:bundle-api
```

最新结果：通过，`Deno.bundle()` 产出 JS/CSS 与 linked source map。

```powershell
dotnet test src/Jazor.EmitTest/Jazor.EmitTest.csproj --filter "FullyQualifiedName~Build_LocalPackages_WithExternalRazorSgSfcConsumer_EmitsVueSfcArtifacts" -p:JazorIsolatedBaseOutputRoot="D:\repository\own\jazor\Jazor\.tmp\test-out\emit-external-sfc-3\" -p:JazorIsolatedBaseIntermediateOutputRoot="D:\repository\own\jazor\Jazor\.tmp\test-obj\emit-external-sfc-3\" /nr:false -p:UseSharedCompilation=false -v minimal
```

最新结果：1 通过，0 失败，0 跳过。

## 离上生产还差什么

### 必须完成

1. 补真实浏览器 smoke，至少覆盖 TodoList sample 的挂载与关键交互。
2. 将独立外部 consumer 的纯 Deno build 纳入可重复验证路径，避免只依赖仓库 sample consumer。
3. 统一 `IVueComponent` / `IVueLibraryComponent` authoring 合同，确保 tests、sample、README、SDK 集成样例完全一致。
4. 继续保持 NuGet 包 contents 合同：`Jazor` 包和 `ECMAScript.Vuetify` 包能被普通 consumer 无额外源码 hack 使用，且不携带 Razor Compiler / Razor Utilities Shared。
5. 全量回归：`Jazor.RazorVue.Test`、`Jazor.RazorVue.RazorIr.Test`、`Jazor.EmitTest` RazorVue 切片、`dotnet pack` 和 package payload guard 必须同时通过。

### 应该完成

1. 消除 Razor IR 测试项目 nullable 警告。
2. 在 docs 中替换旧的“默认 legacy / 单大 catalog / 100% 完成”表述，避免后续按过期计划推进。
3. 给支持矩阵补当前实际支持/unsupported 清单：Razor 语法、生命周期、setup 逻辑、slot、bind、事件、Vuetify 组件、source map、HMR。
4. 继续补真实浏览器 smoke，至少覆盖 TodoList sample 的挂载与关键交互。
5. 对 `RazorVuePipelineTests.cs`、`ESGeneratorTests.cs` 做测试文件分组，降低后续维护成本。

## 上线建议

当前不建议直接标记为通用生产可用。合理状态应调整为：

- **core ready candidate**：核心编译、IR/SFC lowering、descriptor/canonical/emit 模型和 SG tail 接入已进入候选状态。
- **library mode release candidate pending browser/support-matrix smoke**：SDK/package/emit/sample/Deno build 和独立外部 .NET consumer 已闭合，真实浏览器 smoke、独立外部 Deno 自动化和支持矩阵仍是上线前门槛。
- **not GA production ready**：在浏览器交互、支持矩阵和调试闭环完成前，不对外宣称完整生产可用。

下一轮评审的最低通过门槛：

1. 独立外部 Vite consumer build 可重复验证，或明确作为发布流水线步骤。
2. 真实浏览器 smoke 通过，证明 TodoList sample 可挂载且核心交互有效。
3. `dotnet build Jazor.slnx` 0 警告或所有剩余警告有明确豁免说明。
4. 文档、README、sample 的 authoring 写法与真实类型合同一致。
5. 支持/unsupported 矩阵与实际测试覆盖一致。
