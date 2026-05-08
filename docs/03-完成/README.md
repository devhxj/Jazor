# 完成

> Status: 活跃参考
> Positioning: `03-完成` 分类入口，桥接当前状态快照、评审结果和必要的历史审计材料。
> Note: 目录名“完成”保留历史习惯，不表示对应工作流已经终局完成；优先把各子目录里的 `status.md` 读成当前快照，把 testing / obsolete / audit 类材料读成历史资料。

评审结果、深度分析报告、状态快照与阶段性完成材料。

## 按项目结构索引

| 目录 | 对应源码 | 内容 |
|------|---------|------|
| `jolt/` | `src/Jolt/` | 完成度分析、问题清单、状态快照 |
| `razorvue/` | RazorVue 线路 | 阶段性完成材料；当前物理实现已迁到 `src/Jazor.RazorVue/`（含 `RazorSdk/`）、`src/Jazor.Analyzer/RazorVue/`、`src/ECMAScript.Vuetify/` |
| `compiler/` | `src/Jazor.Compiler/` | 当前状态快照 + 历史测试审计 |
| `emit/` | `src/Jazor.Emit/` | 当前状态快照 |
| `sourcemap/` | `src/Jazor.Common/SourceMaps/` + `src/Jazor.Emit/SourceMaps/` | SourceMap 状态快照 |
| `ecmascript.vue3/` | `src/ECMAScript.Vue3/` | 外部库 Vue3 线的状态快照 |
| `ecmascript.pinia/` | `src/ECMAScript.Pinia/` | 外部库 Pinia 线的状态快照 |
| `wiki/` | `src/Wiki/` | 传统 ASP.NET Core 宿主线的状态快照 |

## 推荐入口

- compiler 当前状态 → [compiler/status.md](./compiler/status.md)
- jolt 当前状态 → [jolt/status.md](./jolt/status.md)
- emit 当前状态 → [emit/status.md](./emit/status.md)
- ECMAScript.Vue3 当前状态 → [ecmascript.vue3/status.md](./ecmascript.vue3/status.md)
- ECMAScript.Pinia 当前状态 → [ecmascript.pinia/status.md](./ecmascript.pinia/status.md)
- Wiki 当前状态 → [wiki/status.md](./wiki/status.md)
