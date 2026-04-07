# Jazor 文档中心

## 恢复工作的唯一路径

**从这里开始** → [工作流总览](./workstream-dashboard.md)

工作流总览会告诉你：
- 每个工作流现在在哪个阶段
- 下一步该做什么
- 哪些工作流可以并行，哪些有依赖关系

---

## 文档分层

| 目录 | 作用 | 什么时候看 |
|------|------|-----------|
| `workstream-dashboard.md` | 唯一的状态总览 | **每次恢复工作时先看这个** |
| `status/` | 每个工作流的详细状态快照 | 需要深入了解某个工作流时 |
| `architecture/` | 长期有效的架构和规范参考 | 需要查阅稳定参考时 |
| `archive/` | 历史材料和归档文档 | 查老账时 |
| `superpowers/plans/` | 执行级的详细计划文档 | 准备开始实施时 |
| `guides/` | 文档治理规则 | 维护文档时 |

---

## 文档治理规则

详见 [documentation-governance.md](./guides/documentation-governance.md)

---

## 子系统深度文档

有些子系统的文档直接放在源码旁边，不在 `docs/` 重复：

- [Jazor.Compiler 文档索引](../src/Jazor.Compiler/doc/README.md)
- [Jazor.Emit 文档索引](../src/Jazor.Emit/doc/README.md)

原则：repo-level 文档负责桥接和收口，子系统局部文档负责深度内容，已经成熟的局部文档集，不在顶层重复抄正文。
