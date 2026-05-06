# 计划

WBS、里程碑、阶段拆分，以及各工作流的当前执行进度。

## 文件索引

| 文件 | 说明 |
|------|------|
| `workstream-dashboard.md` | 全局工作流总览（依赖、并行策略、执行门槛） |

## 按项目结构索引

| 目录 | 对应源码 | 内容 |
|------|---------|------|
| `ecmascript/` | `src/ECMAScript/` | ECMAScript 平台内核相关执行级计划 |
| `ecmascript.vue3/` | `src/ECMAScript.Vue3/` | ECMAScript.Vue3 外部库 authoring surface 的执行级落地计划 |
| `wiki/` | `src/Wiki/` | `jazor.wiki` sample 的阶段划分、收口计划与产品化分流边界 |
| `jolt/` | `src/Jolt/` | Phase 计划、切片实施文档、运行模式收口 |
| `jolt/razorvue-implementation/` | RazorVue 迁移材料（当前按 RazorVue 独立线路理解） | RazorVue 模板前端迁移、HMR/桥接边界与历史交叉材料；当前不应自动解读为 `Jolt` 生产代码所有权 |
| `compiler/` | `src/Jazor.Compiler/` | 编译管线实施清单、转换路线图、SourceMap 实施清单 |
