# Jazor Compiler 主线状态（2026-04-06）

> Status: current status snapshot
> Positioning: Repository-level status snapshot for the compiler mainline.

## 总结

`Jazor.Compiler` 仍然是当前仓库里最成熟的主干资产。

说得更直白点：

- 编译器主链路已经接近稳定主干
- 当前工作重点不是重做架构，而是维持主线闭环、控制边界扩张、给外围能力提供稳定依赖面
- 仓库级文档应该把 compiler 当成"稳定核心"，而不是"当前最混沌的探索区"

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

## 下一步行动

### 1. Output closure

**目标**：压实 `ESGenerator -> catalog -> output` 闭环

**具体行动**：
- 避免测试链路和真实输出链路继续分裂
- 让 generator 输出路径和真实构建产物路径保持一致

**参考文档**：
- [TransformationRoadmap.md](../../src/Jazor.Compiler/doc/TransformationRoadmap.md)
- [TransformationClosureChecklist.md](../../src/Jazor.Compiler/doc/TransformationClosureChecklist.md)

### 2. Import closure

**目标**：让 import 从收集阶段进到稳定落盘阶段

**具体行动**：
- 保持 import 命名、去重和顺序稳定
- 确保 import 语句生成的确定性

**参考文档**：
- [TransformationRoadmap.md](../../src/Jazor.Compiler/doc/TransformationRoadmap.md)

### 3. Host semantics seam

**目标**：稳定 `Inline` / `Compile` 分工

**具体行动**：
- 莫让宿主语义扩张又跑回来破坏 compiler 主线边界
- 保持 `Inline` 和 `Compile` 的职责清晰分离

**参考文档**：
- [InlineAstTemplateSpec.md](../../src/Jazor.Compiler/doc/InlineAstTemplateSpec.md)
- [OpCompileSpec.md](../../src/Jazor.Compiler/doc/OpCompileSpec.md)
- [OpCompileImplementationChecklist.md](../../src/Jazor.Compiler/doc/OpCompileImplementationChecklist.md)

## 深度文档

- [Compiler Architecture Bridge](../architecture/compiler/README.md)
- [Jazor.Compiler 文档索引](../../src/Jazor.Compiler/doc/README.md)
- [Jazor.Compiler README](../../src/Jazor.Compiler/README.md)

## 当前缺口

- Repo-level 虽然已经新增 compiler 状态快照，但还缺更细的执行镜像
- 执行导航之前主要由 RazorVue 文档拉动，compiler 的"稳定核心"角色才开始变得显式
- 还需要持续把 compiler 局部索引里的 active / historical 边界写清楚
