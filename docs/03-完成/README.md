# 完成

> Status: 活跃参考
> Positioning: `03-完成` 分类入口，桥接当前状态快照、评审结果和必要的历史审计材料。
> Note: 目录名“完成”保留历史习惯，不表示对应工作流已经终局完成；优先把各子目录里的 `status.md` 读成当前快照，把 testing / obsolete / audit 类材料读成历史资料。

评审结果、深度分析报告、各工作流的状态快照。每个子目录对应项目中的一个核心模块。

## 按项目结构索引

| 目录 | 对应源码 | 内容 |
|------|---------|------|
| `jolt/` | `src/Jolt/` | 完成度分析报告 + 状态快照 |
| `razorvue/` | `src/Jazor.RazorVue/` | 完成度分析报告 |
| `compiler/` | `src/Jazor.Compiler/` | 状态快照 + 历史测试审计 |
| `emit/` | `src/Jazor.Emit/` | 状态快照 |
| `sourcemap/` | 跨模块 SourceMap | 状态快照 |

## 推荐入口

- compiler 当前状态 → [compiler/status.md](./compiler/status.md)
- jolt 当前状态 → [jolt/status.md](./jolt/status.md)
- emit 当前状态 → [emit/status.md](./emit/status.md)
