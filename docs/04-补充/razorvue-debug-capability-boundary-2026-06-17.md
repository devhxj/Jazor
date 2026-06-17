# RazorVue 调试能力边界（2026-06-17）

本文响应 `docs/03-完成/razorvue/completion-analysis.md` 的生产就绪门槛：**“证明 `.razor → .vue → bundled JS` sourcemap 在真实浏览器调试中可用，或在上线说明中明确当前调试能力边界。”** 当前环境无法自动证明真实浏览器调试闭环，故本文采用后者：明确当前调试能力边界、已验证与未验证的环节、以及用户侧验收步骤。

## 产物链与 sourcemap 触点

RazorVue 的 `.razor → 浏览器可执行 JS` 链路有三个 sourcemap 触点：

| 环节 | 产物 | sourcemap 触点 | 当前状态 |
|------|------|----------------|----------|
| 1. `.razor` → RazorVue artifact | `jazor/<component>.vue` + `<component>.vue.map` + `<component>.origins.json` | RazorVue emit 写 `.vue.map`（`RazorVueStaticMarkupParser` / canonical H / SFC semantic model 的源码 origin）与 `.origins.json`（per-artifact source origin） | ✅ 已实现并有单元/集成测试 |
| 2. `.vue` → Deno bundle | `wwwroot/jazor/*.mjs` + `*.mjs.map` | Deno `bundle` 消费 `.vue` SFC 编译输出，生成 bundled JS + `.mjs.map` | ✅ 链路已通（SDK 集成测试 `Build_LocalPackages_*PureDenoPipeline_*` 通过） |
| 3. bundled JS → `.razor` 源码 | 浏览器 DevTools 调试 | DevTools 通过 `.mjs.map` 反向定位到 `.vue`，再由 `.vue.map` / `.origins.json` 回到 `.razor` | 🟡 **未在真实浏览器调试中验收**（本文档化边界） |

## 已验证

- **source origin 元数据**：`RazorVueSourceOrigin`（`src/Jazor.RazorVue/Artifacts/RazorVueSourceOrigin.cs`）记录 `OriginKind`（Component / Descriptor / Template / Logic / GeneratedRender）、源码文件路径、源码 span、生成位置、`MappingQuality`、`Provenance`。设计见 `docs/01-目标/razorvue/artifacts/SourceOrigin.md`。
- **`.map` 生成机制**：`SourceMapBuilder` 单元测试覆盖行映射、多行 span、零长度 origin 过滤、越界 origin 过滤、空生成跳过、多 segment 同行保留（`src/Jazor.EmitTest/SourceMapBuilderTests.cs`，7 个）。
- **`.origins.json` sidecar**：`RazorVueSfcModuleWriter_WritesArtifactsAndManifest`、`RazorVueModuleWriter_WritesAggregateManifest_WithPerAssemblyOrigins` 验证 per-artifact / per-assembly origin 写入。
- **端到端产物存在性**：`Build_LocalJazorPackage_WithSourceReferencedRazorVueSample_EmitsRazorVueOutputs` 验证 `.vue` + `.vue.map` + `.origins.json` + manifest 产物存在；`_SecondBuildWritesUpdatePlan` 验证增量更新计划。
- **Deno bundle 产物**：`Build_LocalPackages_WithExternalRazorSgSfcConsumer_PureDenoPipeline_PassesInIsolatedWorkspace`、`Build_LocalPackages_RazorVueTodoListSample_PureDenoPipeline_PassesInIsolatedWorkspace` 验证 `.vue` → Deno bundle → 浏览器 JS/CSS 链路通，且 `Deno.bundle()` 产出 JS + source map。
- **浏览器 smoke（功能层面）**：`samples/RazorVue.TodoList/Todo.Host/consumer/scripts/smoke-browser.ts` 通过 CDP 启动 headless 浏览器，验证 mount / 交互 / 控制台无错误，但**不验证 DevTools sourcemap 调试定位**。

## 未验证（当前边界）

1. **真实浏览器 DevTools sourcemap 调试闭环**：未在 Chrome/Edge DevTools Sources 面板中验收“在 `.razor` 源码行下断点 → 命中 → 调用栈 / 变量查看”这一闭环。
2. **诊断稳定回映射到 Razor 源位置**：Roslyn 诊断（`JAZORVGA0xx` / `JAZORVUE0xx`）的定位已由 SG 位置携带，但运行时异常 / stack trace 经 `.mjs.map` 反向回映射到 `.razor` 源位置的稳定性未验收。
3. **style / template / script block origin 粒度**：origin 粒度是否足以支撑实际逐行调试与增量更新未验收。

## 用户侧验收步骤（上线前推荐执行）

在真实 consumer 项目中执行以下步骤以闭合调试闭环：

1. `dotnet build`（触发 `JazorEmit` + `JazorConsumerBuild`），确认 `jazor/<component>.vue`、`<component>.vue.map`、`<component>.origins.json` 生成。
2. 运行 colocated consumer（`dotnet run --file scripts/run-deno.cs -- task build`），确认 `wwwroot/jazor/*.mjs` + `*.mjs.map` 生成。
3. 在 DevTools Sources 面板加载应用，确认 `.razor` 源文件出现（sourcemap 反向映射生效）。
4. 在 `.razor` 模板行与 `@code` 逻辑行分别下断点，触发对应交互，确认断点命中、调用栈与变量查看正常。
5. 触发一个运行时异常，确认 stack trace 能回映射到 `.razor` 源位置（而非停留在 bundled JS）。
6. 修改 `.razor`（template / logic / style 各一次），重新 build，确认 HMR / 增量更新按边界分类（TemplateOnly / LogicSafe / FullReloadRequired）生效，且 sourcemap 同步更新。

若任一步骤失败，记为调试闭环未达标，不应宣称完整生产可用。

## 维护规则

- 本文不追加逐次验收日志；验收过程留在 PR / commit 描述与 issue 跟踪中。
- 当真实浏览器调试闭环被验收后，将“未验证”三项移到“已验证”并更新本文日期；同时更新 `razorvue-support-matrix-2026-06-17.md` 第 10 节 sourcemap 行状态。
- sourcemap 机制变更（origin 粒度、mapping quality、provenance）须同步更新 `docs/01-目标/razorvue/artifacts/SourceOrigin.md` 与 `SourceMapBuilderTests`。
