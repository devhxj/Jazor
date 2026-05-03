# ECMAScript.Vue3 状态（2026-05-03）

> Status: 当前状态快照
> Positioning: `src/ECMAScript.Vue3/` 外部库线的仓库级状态快照
> Scope: API/Types 分层、文档域拆分、外部库映射边界与基础验证基线

## 总结

`ECMAScript.Vue3` 已从 `ECMAScript` 平台内核中完成项目与文档层面的独立化：

- 源码项目已独立为 `src/ECMAScript.Vue3/`；
- `Vue3` public surface 已按 `Api/` 与 `Types/` 分层拆分；
- 文档已从 `docs/*/ecmascript/` 拆分为独立 `docs/*/ecmascript.vue3/` 目录；
- 编译器侧 Vue 专名散点硬编码已进入收口阶段，按稳定 host contract 约束推进。

当前更准确的状态是：**结构化收口已完成，Phase 1 的核心 authoring surface 也已完成一轮高质量硬化**。

## 三阶段进度（H -> Razor->H -> Jolt）

1. **Phase 1: H 函数映射层**
   - 状态：基础收口完成，并在 2026-05-03 补完关键 hardening。
   - 结果：`src/ECMAScript.Vue3/` 独立化、`Api/` + `Types/` 分层、default-slot sugar 迁移到通用 `ChildrenToSlotIntrinsic`、文档域拆分完成；`VueModelRef` modifiers、Options inject helper、read-side attrs bag、spread ordering/null omission 回归已补齐。

2. **Phase 2: Razor -> H 规范层**
   - 状态：启动中。
   - 当前目标：收敛 `H(...)` canonical 分类、统一 object-literal/dictionary 路线、巩固 read-side bag 诊断边界。

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
- `EcmaScriptVue3LayoutGuardTests` 已建立，约束 `Api/` + `Types/` 分层与 `Vue3.cs` 壳文件边界
- `useModel` / read-side bag / object-literal ordering / inject helper 的聚焦 AST 回归已补齐

## 下一步行动（Phase 2 主线）

1. 覆盖矩阵驱动补齐  
   以 `vue3-api-coverage-matrix.md` 为优先级清单，补齐不依赖 Vue 专项 compiler 分支的 API 面。

2. Host contract 继续收口  
   继续减少 compiler 侧与 `ECMAScript.Vue3` 名称耦合，保持“通用 contract + 通用 lowering”路线。

3. 目录守护测试扩展  
   视新增外部库需要，把 `ecmascript.vue3` 的分层守护规则抽象为可复用测试模板。

## 参考

- [ECMAScript.Vue3 模块映射规则](../../01-目标/ecmascript.vue3/vue3-module-mapping-rules.md)
- [ECMAScript.Vue3 映射细节设计](../../01-目标/ecmascript.vue3/vue3-mapping-details.md)
- [ECMAScript.Vue3 API 覆盖矩阵](../../01-目标/ecmascript.vue3/vue3-api-coverage-matrix.md)
- [ECMAScript.Vue3 落地计划](../../02-计划/ecmascript.vue3/ECMAScript.Vue3.Authoring.ImplementationPlan.md)
- [src/ECMAScript.Vue3/README.md](../../../src/ECMAScript.Vue3/README.md)
