# 计划

WBS、里程碑、阶段拆分，以及各工作流的当前执行进度。

## 文件索引

| 文件 | 说明 |
|------|------|
| `workstream-dashboard.md` | 全局工作流总览（依赖、并行策略、执行门槛） |
| `jazor-component-runtime-plan-2026-07-06.md` | 当前前端 authoring/runtime 主线：基于官方 Razor SG 与 ASP.NET Core Components 语义移植的 Jazor Component Runtime 计划 |
| `razorvue-blazor-component-directive-support-plan-2026-06-23.md` | RazorVue Blazor component 指令支持补齐计划，覆盖 `@bind`、`@typeparam`、metadata 指令和 host/runtime-only 指令诊断 |

## 当前主线定位

当前前端 authoring/runtime 主线切换为 Jazor Component Runtime：只有显式标注 `[ECMAScriptModule]` 的标准 `.razor` 组件进入该链路；`.razor` 仍由官方 Razor Source Generator 生成组件 C#，Jazor.Compiler 编译为 ES module，`@jazor/runtime` 执行 Razor render tree 与浏览器 DOM 更新。

RazorVue 和 Jolt 不再作为主线推进：

- RazorVue 保留为 Vue artifact 旁路、历史探索和可选互操作参考。
- Jolt 保留为开发期宿主、LSP、DevServer、HMR、build/debug 经验旁路；Runtime 合同稳定前不承载新的组件执行模型。

## 按项目结构索引

| 目录 | 对应源码 | 内容 |
|------|---------|------|
| `ecmascript/` | `src/ECMAScript/` | ECMAScript 平台内核相关执行级计划 |
| `ecmascript.vue3/` | `src/ECMAScript.Vue3/` | ECMAScript.Vue3 外部库 authoring surface 的执行级落地计划 |
| `ecmascript.vben/` | `src/ECMAScript.Vben/` | 后台壳层核心、首个 UI 适配闭环与后续扩展顺序的执行级计划 |
| `ecmascript.pinia/` | `src/ECMAScript.Pinia/` | ECMAScript.Pinia 外部库 authoring surface 的执行级收口清单 |
| `ecmascript.vuetify/` | `src/ECMAScript.Vuetify/` | ECMAScript.Vuetify 作为 Vuetify 代理层与 RazorVue authoring 层的执行级收口清单与组件覆盖矩阵 |
| `wiki/` | `src/Wiki/` | `jazor.wiki` sample 的阶段划分、收口计划与产品化分流边界 |
| `jolt/` | `src/Jolt/` | Phase 计划、切片实施文档、运行模式收口 |
| `csx/` | 待创建 `src/Jazor.CSX/` | Jazor CSX Frontend 独立路线计划：TSX-like `.jazor` 输入、`.jsx` 输出、shadow C# 和 compiler 复用边界 |
| `jolt/razorvue-implementation/` | RazorVue 迁移材料（当前按 RazorVue 独立线路理解） | RazorVue 模板前端迁移、HMR/桥接边界与历史交叉材料；当前不应自动解读为 `Jolt` 生产代码所有权 |
| `compiler/` | `src/Jazor.Compiler/` | 编译管线实施清单、转换路线图、SourceMap 实施清单 |
