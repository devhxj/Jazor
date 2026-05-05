# ECMAScript.Vue3 状态（2026-05-05）

> Status: 当前状态快照
> Positioning: `src/ECMAScript.Vue3/` 外部库线的仓库级状态快照
> Scope: API/Types 分层、文档域拆分、外部库映射边界与基础验证基线

## 总结

`ECMAScript.Vue3` 已从 `ECMAScript` 平台内核中完成项目与文档层面的独立化：

- 源码项目已独立为 `src/ECMAScript.Vue3/`；
- `Vue3` public surface 已按 `Api/` 与 `Types/` 分层拆分；
- 文档已从 `docs/*/ecmascript/` 拆分为独立 `docs/*/ecmascript.vue3/` 目录；
- 编译器侧 Vue 专名散点硬编码的 Phase 1 收口已完成，后续继续按稳定 host contract 约束推进。

当前更准确的状态是：**结构化收口已完成，Phase 1 已完成闭环，当前主线已经切到 Phase 2**。

## 三阶段进度（H -> Razor->H -> Jolt）

1. **Phase 1: H 函数映射层**
  - 状态：完成。
  - 结果：`src/ECMAScript.Vue3/` 独立化、`Api/` + `Types/` 分层、default-slot sugar 迁移到通用 `ChildrenToSlotIntrinsic`、文档域拆分完成；`VueModelRef` modifiers、Options inject helper、read-side attrs bag、spread ordering/null omission、`VueDictionary` symbol-key object authoring / Options provide-inject symbol 路径，以及 named-model `useModel` / typed `update:*` emit contract 已全部收口，并由 AST / proxy 回归守护。

2. **Phase 2: Razor -> H 规范层**
   - 状态：进行中。
   - 当前结果：RazorVue `h(...)` lowering 已完成第一轮 canonical 对齐，最小 arity / 无多余 `null` 占位 / component direct-child default-slot 形态已与 Phase 1 contract 收口。
   - 当前结果（新增）：RazorVue 库模式的 SFC emit/materialisation contract 已落地，`VueSfcArtifact`、catalog reader、`.vue` writer、manifest/style-hash diff 已进入真实代码和回归测试。
   - 当前结果（新增）：`RazorVueCanonicalHComponentModel` / `RazorVueSfcSemanticModel` / SFC lowerer 已进入真实代码；control-flow、attribute、interpolation 的 lifted binding 消费、setup/lifecycle shared lowering、component import/path 闭环、typed slot outlet argument 都已有聚焦回归守护。
   - 当前目标：继续把 Razor authoring 的 diagnostics 边界压到与手写 `H(...)` 一致，补齐 child component callable scoped slot forwarding 等剩余 slot parity，然后再把 library mode 主工件默认切到 design-time 生成的 `.vue` SFC。

3. **Phase 3: Jolt 工程化协同**
   - 状态：规划中。
   - 当前目标：在不引入 Vue compiler 特路的前提下，承接 authoring/build/debug 工程化闭环。

## 当前状态判断

### 1. 模块结构已稳定

- 入口壳文件：`src/ECMAScript.Vue3/Vue3.cs`
- API 分层：`src/ECMAScript.Vue3/Api/Vue3.Api*.cs`
- 类型分层：`src/ECMAScript.Vue3/Types/Vue3.Types.*.cs`
- 项目命名空间：`ECMAScript.Vue3.csproj` 显式 `RootNamespace=ECMAScript`

这套结构已可作为后续官方外部库样例模板。

### 2. 文档域已独立

- 目标文档：`docs/01-目标/ecmascript.vue3/`
- 计划文档：`docs/02-计划/ecmascript.vue3/`
- 状态文档：`docs/03-完成/ecmascript.vue3/`

`ecmascript` 目录已回归平台内核定位，不再混放 Vue3 专项材料。

### 3. 验证基线可用

针对当前拆分已形成稳定回归基线：

- `src/ECMAScript.Vue3/ECMAScript.Vue3.csproj` 可独立构建通过
- `src/Wiki/Wiki.csproj` 可在当前链路下构建通过
- `EcmaScriptVueProxyTests` 回归通过
- `EcmaScriptVue3LayoutGuardTests` 已建立，约束 `Api/` + `Types/` 分层、`Vue3.cs` 壳文件边界、`ECMAScript.Vue3.csproj` 元数据约束、文档拆分边界，以及 `src/ECMAScript/` 不回流 `Vue3` 实现文件
- `useModel` / read-side bag / object-literal ordering / inject helper / direct-child single-evaluation contract 的聚焦 AST 回归已补齐
- `useModel` named-model authoring 已收口到 `VueModelName<TProps,TValue>` + `ModelName/ModelPropName/ModelUpdateEventName` typed contract，不再只能依赖裸字符串
- `VueSetupContext.Emit(model, value)` 已补齐 typed `update:*` emit helper，model prop / emit / useModel 三处可复用同一 contract

## 下一步行动（Phase 2 主线）

1. Razor -> `H(...)` canonical lowering  
   已完成最小 arity / `null` 占位清理；下一步继续保证 Razor authoring 其它路径都落到与手写 `H(...)` 相同的 props / children / slots contract，重点补齐 child component callable scoped slot forwarding 与负例边界。

2. design-time SFC compiler 主链切换  
   emit 侧 `.vue` contract、canonical H model、SFC semantic model 都已就绪；下一步是切 generator/catalog topology 与默认 output mode，禁止 render fallback。

3. diagnostics 与 contract 对齐  
   保持 Razor 产物与手写 Vue3 authoring 在 object-literal / slot / attrs / props 等边界上的同一诊断语义。

4. 外部库模板化  
   已完成第一轮 layout/project/doc-split guard；下一步继续把 `ECMAScript.Vue3` 的 proxy guard 与文档分层规则沉淀为后续外部库样板。

## 参考

- [ECMAScript.Vue3 模块映射规则](../../01-目标/ecmascript.vue3/vue3-module-mapping-rules.md)
- [ECMAScript.Vue3 映射细节设计](../../01-目标/ecmascript.vue3/vue3-mapping-details.md)
- [ECMAScript.Vue3 API 覆盖矩阵](../../01-目标/ecmascript.vue3/vue3-api-coverage-matrix.md)
- [ECMAScript.Vue3 落地计划](../../02-计划/ecmascript.vue3/ECMAScript.Vue3.Authoring.ImplementationPlan.md)
- [RazorVue 库模式 Design-Time SFC 方案](../../02-计划/ecmascript.vue3/RazorVue.LibraryMode.DesignTimeSfcPlan.md)
- [src/ECMAScript.Vue3/README.md](../../../src/ECMAScript.Vue3/README.md)
