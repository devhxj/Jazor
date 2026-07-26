# 完成

> Status: 活跃参考
> Positioning: `03-完成` 分类入口，桥接当前状态快照、评审结果和必要的历史审计材料。
> Note: 目录名“完成”保留历史习惯，不表示对应工作流已经终局完成；`jolt/status.md` 是明确的历史快照，其余状态也需结合当前源码和测试确认。

评审结果、深度分析报告、状态快照与阶段性完成材料。

## 按项目结构索引

| 目录 | 对应源码 | 内容 |
|------|---------|------|
| `jolt/` | Git 基线 `d68aecbb00b23aa35735c9a269b2e987c7815b05` | Jolt 历史完成度、问题清单和验证快照；当前无对应源码 |
| `razorvue/` | RazorVue 线路 | 历史/阶段性完成材料；pre-G0 Razor IR/SFC/Jolt 结论不得当作当前转型事实，当前实现以 `src/Jazor.RazorVue/README.md` 与 WBS 为准 |
| `compiler/` | `src/Jazor.Compiler/` | 当前状态快照 + 历史测试审计 |
| `emit/` | `src/Jazor.Emit/` | 当前状态快照 |
| `sourcemap/` | `src/Jazor.Common/SourceMaps/` + `src/Jazor.Emit/SourceMaps/` | SourceMap 状态快照 |
| `ecmascript.vue3/` | `src/ECMAScript.Vue3/` | 外部库 Vue3 线的状态快照 |
| `ecmascript.pinia/` | `src/ECMAScript.Pinia/` | 外部库 Pinia 线的状态快照 |
| `ecmascript.vben/` | `src/ECMAScript.Vben/` | 后台壳层抽象的历史/阶段性状态；已删除测试项目和旧 SFC pipeline 结论只作迁移参考 |
| `razorvue/` 内相关状态材料 | `src/Jazor.RazorVue/` + `src/ECMAScript.VueContract/` + `src/ECMAScript.Vben/` | 组件发现/descriptor 与通用 `VueProp` / `VueSlot` authoring 收口状态；以当前实现与聚焦测试为准，不再把 `VueLibraryProp` / `VueLibrarySlot` 兼容别名视为现行契约 |
| `wiki/` | `src/Wiki/` | 传统 ASP.NET Core 宿主线的状态快照 |

## 推荐入口

- compiler 当前状态 → [compiler/status.md](./compiler/status.md)
- Jolt 历史状态快照 → [jolt/status.md](./jolt/status.md)
- emit 当前状态 → [emit/status.md](./emit/status.md)
- ECMAScript.Vue3 当前状态 → [ecmascript.vue3/status.md](./ecmascript.vue3/status.md)
- ECMAScript.Pinia 当前状态 → [ecmascript.pinia/status.md](./ecmascript.pinia/status.md)
- ECMAScript.Vben 当前状态 → [ecmascript.vben/status.md](./ecmascript.vben/status.md)
- Wiki 当前状态 → [wiki/status.md](./wiki/status.md)
