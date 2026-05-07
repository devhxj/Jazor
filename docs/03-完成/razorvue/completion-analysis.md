# RazorVue 完成度与生产就绪评审

> 评审日期：2026-05-07  
> 评审范围：`src/Jazor.RazorVue/`、`src/Jazor.Analyzer/RazorVue/`、`src/Jazor.Emit` 的 RazorVue 路径、`src/Jazor.RazorVue.Test/`、`src/Jazor.RazorVue.RazorIr.Test/`、`src/Jazor.EmitTest/` 的 RazorVue 切片、`src/Jolt.Test` 的 Volar/VueAnalysis/JazorVue 切片、`samples/RazorVue.TodoList/`  
> 基线说明：本评审基于当前工作区状态。当前工作区存在多处 RazorVue/emit/VueRoute 相关未提交修改，因此结论不等同于已发布包基线。

## 结论

RazorVue 不能继续按历史文档里的“100% 完成”口径描述。当前更准确的判断是：

- **核心语义与 lowering 主链：约 80% 完成**。组件发现、descriptor、Razor IR 优先模板前端、canonical H、SFC semantic model、SFC artifact、legacy/SFC 双 catalog、source-origin/hash/HMR 元数据等主干已经成型，并有大量单元测试覆盖。
- **库模式生产就绪：未达标**。真实 generator assembly、SDK 本地包、sample/host 构建、emit 物化相关 RazorVue 集成测试仍有失败，不能作为可生产发布能力声明。
- **Jolt 关联能力：局部可用，不能替代库模式验收**。Volar/VueAnalysis/virtual artifact 过滤测试通过，但这只证明 Jolt 相关局部协议和投影切片，不证明 RazorVue NuGet/SDK 生产消费链路闭合。

当前建议状态：

| 维度 | 状态 | 说明 |
|------|------|------|
| 核心库编译 | 通过 | `Jazor.RazorVue` 可构建 |
| RazorVue 单元测试 | 通过 | 549 通过 |
| Razor IR 前端测试 | 通过但有警告 | 76 通过，测试项目存在 nullable 警告 |
| Jolt 关联过滤测试 | 通过 | 35 通过 |
| solution 构建 | 通过但有警告 | 1 个 Razor IR 测试项目 nullable 警告 |
| emit/SDK RazorVue 集成 | **失败** | 43 个过滤测试中 8 失败 |
| 生产发布判断 | **不建议上线** | SDK/package/sample/emit 集成链路未过 |

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

### P0 阻断：emit/SDK 集成测试失败

本轮执行：

```powershell
dotnet test src/Jazor.EmitTest/Jazor.EmitTest.csproj --filter "FullyQualifiedName~RazorVue" -v minimal
```

结果：

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

这不是小问题：生产发布必须证明 NuGet 包、MSBuild props/targets、source generator、emit tool、consumer project 可以按文档闭环。当前这条链路没有通过。

### P0 阻断：authoring surface 文档/样例与真实类型合同漂移

当前 `RazorVueCompilationSymbols` 解析：

- `ECMAScript.Vue3+IVueComponent`
- `ECMAScript.Vue3+IVueLibraryComponent`

但 `samples/RazorVue.TodoList/Todo.Library/TodoApp.razor.cs` 仍使用：

```csharp
using ECMAScript;

public partial class TodoApp : ComponentBase, IVueComponent
```

除非消费项目显式添加：

```csharp
global using static ECMAScript.Vue3;
```

否则裸 `IVueComponent` 不应解析。核心测试里很多测试通过全局 `using static ECMAScript.Vue3` 补齐，但 sample 和 emit 集成测试没有全面同步。

生产前必须二选一：

- 把官方 authoring 合同固定为 `using static ECMAScript.Vue3;`，并更新所有 sample、测试、README。
- 或提供稳定兼容入口，让 `using ECMAScript;` + 裸 `IVueComponent` 在消费项目中可用。

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
- SSR/hydration 目前是 hints/预留，不是已验证生产能力。

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
dotnet test src/Jazor.RazorVue.Test/Jazor.RazorVue.Test.csproj -v minimal
```

结果：549 通过，0 失败，0 跳过。

```powershell
dotnet test src/Jazor.RazorVue.RazorIr.Test/Jazor.RazorVue.RazorIr.Test.csproj -v minimal
```

结果：76 通过，0 失败，0 跳过；存在 `RazorEngineFeatureSpikeTests.cs(151,30)` nullable 警告。

```powershell
dotnet test src/Jolt.Test/Jolt.Test.csproj --filter "FullyQualifiedName~Volar|FullyQualifiedName~VueAnalysis|FullyQualifiedName~VirtualArtifact|FullyQualifiedName~JazorVue" -v minimal
```

结果：35 通过，0 失败，0 跳过。

```powershell
dotnet build Jazor.slnx -v minimal
```

结果：通过，1 个 nullable 警告，0 错误。

```powershell
dotnet test src/Jazor.EmitTest/Jazor.EmitTest.csproj --filter "FullyQualifiedName~RazorVue" -v minimal
```

结果：35 通过，8 失败，0 跳过。

## 离上生产还差什么

### 必须完成

1. 修复 `Jazor.EmitTest` RazorVue 过滤切片，做到 43/43 通过。
2. 统一 `IVueComponent` / `IVueLibraryComponent` authoring 合同，更新 tests、sample、README、SDK 集成样例。
3. 重新验证 `samples/RazorVue.TodoList/build-local.ps1` 可从空本地包缓存跑通，并生成 `.vue`、manifest、host requirements module。
4. 清理旧 legacy `.mjs` 默认假设，测试和文档必须与 SFC 默认模式一致；legacy 只作为显式兼容模式验证。
5. 明确 NuGet 包 contents 合同，更新 `CreateLocalPackage_IncludesRazorVueAuthoringAssets` 的期望，并确保 `Jazor` 包和 `ECMAScript.Vuetify` 包能被普通 consumer 无额外源码 hack 使用。
6. 建立一个最小生产 smoke test：本地 pack `Jazor` + `ECMAScript.Vuetify`，新建外部 consumer，`dotnet build` 后由 Vite/Vue 消费生成 `.vue`，至少执行一次前端 build。

### 应该完成

1. 消除 Razor IR 测试项目 nullable 警告。
2. 在 docs 中替换旧的“默认 legacy / 单大 catalog / 100% 完成”表述，避免后续按过期计划推进。
3. 给支持矩阵补当前实际支持/unsupported 清单：Razor 语法、生命周期、setup 逻辑、slot、bind、事件、Vuetify 组件、source map、HMR。
4. 补真实浏览器或 Vite build 验证，至少覆盖 TodoList sample。
5. 对 `RazorVuePipelineTests.cs`、`ESGeneratorTests.cs` 做测试文件分组，降低后续维护成本。

## 上线建议

当前不建议把 RazorVue 标记为生产可用。合理的发布标签应是：

- **core preview**：核心编译、IR/SFC lowering、descriptor/canonical/emit 模型已可评估。
- **library mode integration preview**：SDK/emit/sample 正在收口，不能承诺普通消费项目一键可用。
- **not production ready**：在 emit/SDK 集成切片全绿、sample 端到端通过、authoring surface 固定前，不进入生产发布。

下一轮评审的最低通过门槛：

1. `dotnet test src/Jazor.EmitTest/Jazor.EmitTest.csproj --filter "FullyQualifiedName~RazorVue"` 全绿。
2. `samples/RazorVue.TodoList/build-local.ps1` 全绿。
3. `dotnet build Jazor.slnx` 0 警告或所有剩余警告有明确豁免说明。
4. 新建外部 consumer 项目验证包消费，不依赖仓库源码引用。
5. 文档、README、sample 的 authoring 写法与真实类型合同一致。
