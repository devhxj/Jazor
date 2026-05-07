# ECMAScript Vue 生态完成度与生产就绪诊断报告

> 评审日期：2026-05-07  
> 评审范围：`src/ECMAScript.Vue3/`、`src/ECMAScript.Pinia/`、`src/ECMAScript.Pinia.Testing/`、`src/ECMAScript.VueRoute/`、对应测试工程、`samples/ECMAScript.Pinia.Counter/`、`docs/01-目标/ecmascript.*`、`docs/02-计划/ecmascript.*`、`docs/03-完成/ecmascript.*`  
> 基线说明：本评审基于当前工作区状态。当前工作区存在 3 个未跟踪临时文件：`.tmp-lambda-union.cs`、`.tmp-null-add.cs`、`.tmp-overload-test.cs`；复核时另有 `src/Jolt.Test/JoltBuildTests.cs`、`src/Jolt/Build/DenoBuildImportMapGenerator.cs`、`src/Jolt/Build/DenoBundleRunner.cs` 的无关修改。本报告未使用也未修改这些文件。

## 结论

`ECMAScript.Vue3`、`ECMAScript.Pinia`、`ECMAScript.VueRoute` 都已经从平台内核中拆成独立外部库绑定线，模块结构、API/Types 分层和测试所有权整体方向正确。但当前不能把三者合并描述为“Vue 生态生产可用”。

更准确的状态判断：

| 模块 | 当前完成度判断 | 生产就绪判断 | 主要依据 |
|------|----------------|--------------|----------|
| `ECMAScript.Vue3` | 核心绑定约 80-85% | `production candidate`，但仍需更广回归 | 独立构建通过，Vue3 专项 compiler 测试 57/57 已收口；`Vue3.cs` 分层边界与 reactivity surface 断言已对齐 |
| `ECMAScript.Pinia` | 功能链路约 85-90% | `production candidate`，但仍需样例与跨线回归 | 独立构建通过，sample 端到端 smoke 通过，主测试工程 62/62 已收口 |
| `ECMAScript.Pinia.Testing` | 约 90% | 可随 Pinia 主包一起进入候选 | 独立构建通过，39 个测试全绿，sample smoke 覆盖 `@pinia/testing` runtime 路径 |
| `ECMAScript.VueRoute` | 高频 API 切片约 80-85% | 可作为 covered API beta，不能宣称全量生产 | 独立构建通过，94 个测试全绿；但缺少真实前端 runtime/package smoke，也缺少 `docs/03-完成/ecmascript.vueroute/status.md` |

当前最大风险已经从“Vue3/Pinia 基础合同测试红灯”转为**跨线生产回归还没跑完**。Pinia sample 已经证明一条真实消费链路可以跑通，Vue3/Pinia 的专项目录测试也已收口；下一步的生产标准重点应转到 RazorVue/emit/Jolt 联动、外部 consumer、以及 VueRoute 真实 runtime smoke。

## 已完成能力

### ECMAScript.Vue3

- `src/ECMAScript.Vue3/` 已独立为外部库项目，`RootNamespace=ECMAScript`，并通过 `ECMAScript("npm:vue@3")` 暴露 Vue 3 runtime 绑定。
- API 与类型文件已按 `Api/`、`Types/` 分层，`H(...)`、`CreateApp(...)`、`DefineComponent(...)`、reactivity、composition、lifecycle、directive、props、slots、component contract 等主要 surface 已成形。
- 文档域已拆分到 `docs/01-目标/ecmascript.vue3/`、`docs/02-计划/ecmascript.vue3/`、`docs/03-完成/ecmascript.vue3/`。
- 独立项目构建通过，说明当前源码本体可编译。

### ECMAScript.Pinia

- `src/ECMAScript.Pinia/` 已独立为外部库项目，API/Types 分层稳定，主测试已从 compiler 测试中拆到 `src/ECMAScript.Pinia.Test/`。
- 已覆盖 Pinia root lifecycle、option store、setup store、`storeToRefs`、hydration、HMR、plugin projection、Options API helpers、subscription、action listener、multi-root isolation 等大量高频路径。
- `src/ECMAScript.Pinia.Testing/` 已把 `@pinia/testing` 拆成独立绑定包，并覆盖 `createTestingPinia`、`TestingOptions`、`stubActions`、typed predicate、`createSpy`、testing plugin projection 等测试场景。
- `samples/ECMAScript.Pinia.Counter/verify-smoke.ps1` 本轮通过，验证了本地 Jazor 包、sample host、generated Pinia/testing modules、Vite production build、Vitest runtime/DOM。

### ECMAScript.VueRoute

- `src/ECMAScript.VueRoute/` 已独立为外部库项目，API/Types 分层存在。
- 已覆盖 Vue Router 4 高频 authoring surface：`createRouter`、web/hash/memory history、`useRouter`、`useRoute`、`useLink`、`RouterLink`、`RouterView`、导航守卫、route record、location/query/params、navigation failure。
- `src/ECMAScript.VueRoute.Test/` 已形成独立测试工程，compiler boundary、proxy、layout guard 合计 94 个测试本轮全绿。
- 目标文档和覆盖矩阵已存在，且明确不追求逐项镜像 Vue Router TypeScript 全量类型系统。

## 主要缺口

### 已收口：Vue3 专项 compiler 测试红灯

本轮执行：

```powershell
dotnet test src/Jazor.CompilerTest/Jazor.CompilerTest.csproj --filter "FullyQualifiedName~EcmaScriptVue3|FullyQualifiedName~EcmaScriptVueProxy" -v minimal -p:UseSharedCompilation=false
```

最新结果：

- 通过：57
- 失败：0
- 总计：57

失败项集中在三类合同漂移：

| 失败测试 | 问题形态 | 影响 |
|----------|----------|------|
| `Vue3_ShellFile_RemainsHostAttributeEntryPointOnly` | 测试要求 `Vue3.cs` 只保留 host attribute entrypoint，但当前 `src/ECMAScript.Vue3/Vue3.cs` 里有 `IVueComponent` / `IVueLibraryComponent` marker contract | Vue3 模块分层规则与真实代码不一致 |
| `Vue_P0CoverageBindings_ExposeStronglyTypedRuntimeSurface` | 强类型 runtime surface 断言找不到匹配方法，例如测试期望 `TriggerRef<T>(IVueRef<T>)`，当前实现为 `TriggerRef<T>(VueShallowRef<T>)` | API contract 未裁决或测试过期 |
| `Vue_P1ReactivityBindings_ExposeStronglyTypedHelperSurface` | reactivity helper 断言与当前实现不一致，例如 `Computed(...)` 返回类型期望与真实 `VueComputedRef<T>` / `VueWritableComputedRef<T>` 路线不一致 | public API 语义口径不稳定 |
| `Convert_ClassUsingEcmaScriptVueProxy_GeneratesVueImportsFromNameAttributes` | 动态编译测试中的 `ECMAScript.VueContract` 命名空间无法解析 | Vue authoring namespace/reference 合同漂移 |

已完成的收口动作：

- `IVueComponent` / `IVueLibraryComponent` 已移回 `Types/` 分层，恢复 `Vue3.cs` 壳文件只保留 entrypoint attribute 的结构边界。
- `TriggerRef(...)` 的合同按官方 shallow-ref 语义守护，不再把任意 `IVueRef<T>` 错当成可触发 surface。
- `Computed(...)` / `ToRef(getter)` 的断言已改为守护更精确的 `VueWritableComputedRef<T>` / `VueComputedRef<T>` 返回类型，而不是回退到过宽 contract。
- 动态编译测试已补齐 `ECMAScript.VueContract` metadata reference，恢复真实 consumer 编译环境。

这部分不再是当前生产阻断项。

### 已收口：Pinia 主测试工程红灯

本轮执行：

```powershell
dotnet test src/ECMAScript.Pinia.Test/ECMAScript.Pinia.Test.csproj -v minimal -p:UseSharedCompilation=false
```

最新结果：

- 通过：62
- 失败：0
- 总计：62

失败全部集中在 `EcmaScriptPiniaImportTests` 的动态编译源码，错误形态一致：

```text
error CS0234: 命名空间“ECMAScript”中不存在类型或命名空间名“VueContract”
```

已完成的收口动作：

- Pinia import/lowering 动态编译测试已补齐 `ECMAScript.VueContract` metadata reference。
- 这证明前一轮 11 个失败不是 Pinia runtime/binding 本体损坏，而是测试编译闭包没有模拟真实 package 依赖图。

这部分不再是当前生产阻断项。

### P1：Vue authoring namespace 合同仍需进一步显式化

同一个问题同时出现在 Vue3 compiler 测试、Pinia 主测试，以及此前 RazorVue 评审里的 SDK/sample 集成失败：旧代码路径仍依赖 `ECMAScript.VueContract` 或裸 `IVueComponent`，而当前真实类型解析主要落在 `ECMAScript.Vue3` / `using static ECMAScript.Vue3`。

必须二选一：

- 固定新合同：官方 authoring 必须显式 `using static ECMAScript.Vue3;`，并清理所有 `ECMAScript.VueContract` 旧 using、测试源码、sample、README。
- 提供兼容入口：让旧的 `ECMAScript.VueContract` / 裸 `IVueComponent` 在普通 consumer 中稳定可用，并把兼容层纳入 package 与测试。

这一层当前已不再阻断 Vue3/Pinia 专项测试，但仍然影响更大范围的 RazorVue/sample/SDK 文档一致性。它应该作为下一轮跨线生产收口项，而不是继续留成隐含假设。

### P1：Pinia 功能链路强，现已进入跨线验证阶段

`samples/ECMAScript.Pinia.Counter/verify-smoke.ps1` 本轮通过：

- 本地 pack `Jazor.0.1.17.nupkg` 成功。
- `Pinia.Counter.Host` rebuild 成功，0 警告，0 错误。
- 生成模块断言通过，覆盖 `pinia`、`@pinia/testing`、manifest、host app。
- Vite production build 成功，26 个模块 transformed。
- Vitest 通过：3 个 test files，23 个 tests。

这说明 Pinia 已经不再停留在“sample 强、单测红”的状态，而是可以进入生产候选。但工程发布仍不能只看 sample smoke；还需要更大范围的 solution/emit/RazorVue 联动验证。

### P1：VueRoute 只完成高频切片，缺少真实 runtime/package smoke

VueRoute 当前测试全绿，但验证层级仍偏 compiler/import/proxy/layout：

- 没有类似 `samples/ECMAScript.Pinia.Counter/verify-smoke.ps1` 的 VueRoute 真实前端 sample smoke。
- 没有证明生成的 router module 可被 Vite/Vue Router runtime 消费并完成 navigation/guard/RouterLink/RouterView DOM 断言。
- 覆盖矩阵明确 typed routes、route props 全细分、redirect callback typing、async component typing 仍是部分覆盖。

结论应写成“Vue Router 高频 API 绑定 beta 可用”，而不是“VueRoute 全量生产可用”。

### P1：状态文档需要按当前验证结果更新

现有状态文档需要校正：

- `docs/03-完成/ecmascript.vue3/status.md` 应更新为 Vue3 专项 compiler 测试 57/57 已通过，并记录 `Vue3.cs` 分层边界已恢复。
- `docs/03-完成/ecmascript.pinia/status.md` 应更新为 Pinia 主测试 62/62 已通过，并说明失败根因是动态编译测试引用闭包缺失而不是主包功能回退。
- `docs/03-完成/ecmascript.vueroute/status.md` 当前不存在，VueRoute 只有目标文档和覆盖矩阵，缺少完成状态快照。

建议尽快把这三个状态页补齐/更新，否则文档会继续落后于当前真实基线。

### P2：sample smoke 会改动已跟踪生成 manifest

本轮执行 Pinia smoke 后，`samples/ECMAScript.Pinia.Counter/Pinia.Counter.Host/wwwroot/jazor/jazor-manifest.json` 被改为 Debug assembly path 和新的 `GeneratedAtUtc`。本报告已把该改动恢复。

这不是功能阻断，但会影响开发体验和 CI 可重复性。建议让 smoke 写入隔离输出目录，或避免把带时间戳/本机配置路径的生成 manifest 作为固定跟踪产物。

### P2：并行构建共享中间产物容易触发文件锁

本轮最初并行构建 Vue3/Pinia/Pinia.Testing/VueRoute 时，共享 `src/ECMAScript/obj/Debug/net10.0/ECMAScript.dll` 触发 `CS2012` 文件锁。随后执行 `dotnet build-server shutdown` 并用顺序 `-p:UseSharedCompilation=false` 重跑后，四个库构建全部通过。

该问题不计为库功能缺陷，但后续自动化评审应遵守仓库规则：并行测试/构建时使用独立 `BaseOutputPath`，不要让多条 lane 争用同一个 `obj`。

## 本轮验证证据

```powershell
dotnet build src/ECMAScript.Vue3/ECMAScript.Vue3.csproj -v minimal -p:UseSharedCompilation=false
```

结果：通过，0 警告，0 错误。

```powershell
dotnet build src/ECMAScript.Pinia/ECMAScript.Pinia.csproj -v minimal -p:UseSharedCompilation=false
```

结果：通过，0 警告，0 错误。

```powershell
dotnet build src/ECMAScript.Pinia.Testing/ECMAScript.Pinia.Testing.csproj -v minimal -p:UseSharedCompilation=false
```

结果：通过，0 警告，0 错误。

```powershell
dotnet build src/ECMAScript.VueRoute/ECMAScript.VueRoute.csproj -v minimal -p:UseSharedCompilation=false
```

结果：通过，0 警告，0 错误。

```powershell
dotnet test src/Jazor.CompilerTest/Jazor.CompilerTest.csproj --filter "FullyQualifiedName~EcmaScriptVue3|FullyQualifiedName~EcmaScriptVueProxy" -v minimal -p:UseSharedCompilation=false
```

结果：57 通过，0 失败，0 跳过。

```powershell
dotnet test src/ECMAScript.Pinia.Test/ECMAScript.Pinia.Test.csproj -v minimal -p:UseSharedCompilation=false
```

结果：62 通过，0 失败，0 跳过。

```powershell
dotnet test src/ECMAScript.Pinia.Testing.Test/ECMAScript.Pinia.Testing.Test.csproj -v minimal -p:UseSharedCompilation=false
```

结果：39 通过，0 失败，0 跳过。

```powershell
dotnet test src/ECMAScript.VueRoute.Test/ECMAScript.VueRoute.Test.csproj -v minimal -p:UseSharedCompilation=false
```

结果：94 通过，0 失败，0 跳过。

```powershell
pwsh samples/ECMAScript.Pinia.Counter/verify-smoke.ps1
```

结果：通过。本地 package pack、sample host rebuild、generated module 断言、Vite build、Vitest runtime/DOM 全部通过；Vitest 结果为 3 个 test files、23 个 tests 全绿。

## 离上生产还差什么

### 必须完成

1. 把 `ECMAScript.VueContract`、`ECMAScript.Vue3`、`using static ECMAScript.Vue3`、`IVueComponent` / `IVueLibraryComponent` 的 authoring 合同写入正式 README/status，而不是只存在于实现与测试。
2. 同步更新 Vue3、Pinia、RazorVue 相关 sample、README、SDK 集成样例，避免旧命名空间继续出现在真实消费代码里。
3. 重新跑 Pinia sample smoke，并确保执行后 `git status` 不产生 sample generated manifest 脏改。
4. 给 VueRoute 增加真实 package/consumer smoke，至少覆盖 Vite build、router creation、navigation、guard、`RouterLink` / `RouterView` runtime DOM 断言。
5. 为 VueRoute 补 `docs/03-完成/ecmascript.vueroute/status.md`，并把覆盖矩阵中的“部分覆盖/暂不覆盖”同步到生产边界说明。
6. 跑覆盖 Vue3/Pinia/Pinia Testing/VueRoute/RazorVue/emit 的跨线发布回归，确认这次合同修复没有把问题转移到 RazorVue/emit 端。

### 应该完成

1. 更新 `docs/03-完成/ecmascript.vue3/status.md` 和 `docs/03-完成/ecmascript.pinia/status.md`，把修复后的新基线写进去。
2. 把 Vue3 API surface 断言改成更聚焦的合同测试，避免“测试要旧签名、实现走新签名”的长期歧义。
3. 把 Pinia smoke 的输出目录隔离出来，不再修改已跟踪 `wwwroot/jazor` manifest。
4. 给三条外部库线建立统一发布门槛：独立 build、独立 test、sample smoke、package consumer smoke、文档状态更新。
5. 全量运行 `pwsh ./scripts/test-dotnet.ps1` 或至少运行覆盖 Vue3/Pinia/Pinia Testing/VueRoute/RazorVue/emit 的发布相关切片。

## 上线建议

当前建议发布标签：

| 模块 | 建议标签 | 说明 |
|------|----------|------|
| `ECMAScript.Vue3` | `production candidate` | 专项构建与测试已收口，下一步看跨线回归与外部 consumer |
| `ECMAScript.Pinia` | `production candidate` | sample、主测试都已收口，下一步看更大范围集成回归 |
| `ECMAScript.Pinia.Testing` | `preview candidate` | 独立测试和 sample smoke 均通过，但应随 Pinia 主包一起验收 |
| `ECMAScript.VueRoute` | `covered API beta` | 高频 API 测试全绿，但缺少前端 runtime smoke 和完成状态文档 |

下一轮评审的最低通过门槛：

1. `dotnet test src/Jazor.CompilerTest/Jazor.CompilerTest.csproj --filter "FullyQualifiedName~EcmaScriptVue3|FullyQualifiedName~EcmaScriptVueProxy"` 保持全绿。
2. `dotnet test src/ECMAScript.Pinia.Test/ECMAScript.Pinia.Test.csproj` 保持全绿。
3. `dotnet test src/ECMAScript.Pinia.Testing.Test/ECMAScript.Pinia.Testing.Test.csproj` 全绿。
4. `dotnet test src/ECMAScript.VueRoute.Test/ECMAScript.VueRoute.Test.csproj` 全绿。
5. `samples/ECMAScript.Pinia.Counter/verify-smoke.ps1` 全绿且不留下工作区脏改。
6. 新增 VueRoute sample smoke 全绿。
7. 三条线的 `docs/03-完成` 状态文档与真实测试结果一致。
