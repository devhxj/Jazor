# Compiler 主线执行桥接

> Status: active plan
> Positioning: Repository-level execution bridge for the compiler mainline.

## 目标

把 compiler 主线从“深度文档很多”桥接成“当前维护者晓得先看啥、先补啥”。

## 当前执行重点

### 1. Output closure

优先看：

- [TransformationRoadmap.md](../../src/Jazor.Compiler/doc/TransformationRoadmap.md)
- [TransformationClosureChecklist.md](../../src/Jazor.Compiler/doc/TransformationClosureChecklist.md)

当前目标：

- 压实 `ESGenerator -> catalog -> output` 闭环
- 避免测试链路和真实输出链路继续分裂

### 2. Import closure

优先看：

- [TransformationRoadmap.md](../../src/Jazor.Compiler/doc/TransformationRoadmap.md)

当前目标：

- 让 import 从收集阶段进到稳定落盘阶段
- 保持 import 命名、去重和顺序稳定

### 3. Host semantics seam

优先看：

- [InlineAstTemplateSpec.md](../../src/Jazor.Compiler/doc/InlineAstTemplateSpec.md)
- [OpCompileSpec.md](../../src/Jazor.Compiler/doc/OpCompileSpec.md)
- [OpCompileImplementationChecklist.md](../../src/Jazor.Compiler/doc/OpCompileImplementationChecklist.md)

当前目标：

- 稳定 `Inline` / `Compile` 分工
- 莫让宿主语义扩张又跑回来破坏 compiler 主线边界

## 建议阅读顺序

1. [Jazor Compiler 主线状态（2026-04-06）](../status/2026-04-06-compiler-mainline-status.md)
2. [TransformationRoadmap.md](../../src/Jazor.Compiler/doc/TransformationRoadmap.md)
3. [Compiler Architecture Bridge](../architecture/compiler/README.md)
4. 再按具体问题进入专题文档

## 当前非目标

- 不把所有 cross-cutting work 都重新收回 compiler 视角
- 不用 repo-level bridge 取代 `src/Jazor.Compiler/doc/` 这套深度文档集

## 下一步维护要求

如果 compiler 主线的执行重点发生变化，至少同步更新：

1. [Jazor Compiler 主线状态（2026-04-06）](../status/2026-04-06-compiler-mainline-status.md)
2. [项目执行导航](./project-execution-index.md)
