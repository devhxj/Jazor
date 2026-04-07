# Jazor Compiler 主线状态（2026-04-06）

> Status: current status snapshot
> Positioning: Repository-level status bridge for the compiler mainline.

## 总结

`Jazor.Compiler` 仍然是当前仓库里最成熟的主干资产。

说得更直白点：

- 编译器主链路已经接近稳定主干
- 当前工作重点不是重做架构，而是维持主线闭环、控制边界扩张、给外围能力提供稳定依赖面
- 仓库级文档应该把 compiler 当成"稳定核心"，而不是"当前最混沌的探索区"

## 当前依据

- [2026-04-04-project-stage-assessment.md](./2026-04-04-project-stage-assessment.md)
- [src/Jazor.Compiler/README.md](../../src/Jazor.Compiler/README.md)
- [src/Jazor.Compiler/doc/README.md](../../src/Jazor.Compiler/doc/README.md)

## 当前状态判断

### 1. 主链路成熟度高

`AstConverter`、`SemanticWalker`、白名单和 generator 主链路已经有稳定参考价值了。

这也是为啥 repo-level 文档应该优先把 compiler deep-dive 当成长期参考入口，而不是把它和阶段性实施材料混成一层。

### 2. 当前更像"稳定化与收口"，不是"大规模重构期"

当前 compiler 线最重要的几件事是：

- 保持语义主线稳定
- 控制新增能力对主链路的扰动
- 给 RazorVue、SourceMap、Emit 这些下游 lane 提供稳定上游

### 3. 这轮已经补上对称的 compiler 状态入口

之前 repo-level 主要是总项目状态和 RazorVue 状态，导致 compiler 虽然成熟，但在仓库级工作流图里不够显眼。本状态页的作用就是把这条断链补起。

## 当前推荐入口

如果你准备处理 compiler 主线问题，建议按这个顺序：

1. [项目阶段评审](./2026-04-04-project-stage-assessment.md)
2. 本文档
3. [Compiler Architecture Bridge](../architecture/compiler/README.md)
4. [Jazor.Compiler 文档索引](../../src/Jazor.Compiler/doc/README.md)
5. 再按专题进入具体文档

## 当前执行重点

当前 compiler 主线更适合沿这三条原则推进：

- 主链路稳定优先于外围能力扩张
- 文档里区分 stable reference 和 phase materials
- 新能力应该复用 compiler 既有的稳定边界，而不是倒逼主线重构

## 当前缺口

- repo-level 虽然已经新增 compiler 状态快照，但还缺更细的执行镜像
- 执行导航之前主要由 RazorVue 文档拉动，compiler 的"稳定核心"角色才开始变得显式
- 还需要持续把 compiler 局部索引里的 active / historical 边界写清楚
