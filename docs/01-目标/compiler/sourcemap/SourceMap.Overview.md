# Jazor SourceMap 文档总览

> Status: 活跃参考
> Positioning: 当前 compiler SourceMap 文档集的总入口。
> Note: 本页服务于 broad compiler/source-origin/map 主线；若进入更窄的 Jolt 或 RazorVue active lane，应再结合对应状态页和执行计划判断。

## 1. 文档定位

本文档是 Jazor sourcemap 方案的总入口。

它不重复完整设计，只回答三个问题：

1. 现在 sourcemap 处于什么状态
2. 各份文档分别解决什么问题
3. 后续继续扩展与稳定化时建议按什么顺序阅读

## 2. 当前状态

当前 sourcemap 在项目中的状态是：

- compiler 侧 `SourceOrigin`、writer 侧 map 生成、emit 侧 `.mjs.map` 物化 baseline 已经落地
- broad sourcemap program 仍然保持保守范围，不把每个扩展点都立即抬成全量契约
- 与 RazorVue 相关的更窄 bundle / chaining lane 仍需按各自状态页与计划判断

当前共识是：

1. 不再把 sourcemap 描述成“尚未实现”，而是描述成“baseline 已有，后续继续巩固”
2. 继续控制范围，优先稳住模块级 map、origin 传播和输出确定性
3. broad compiler contract 仍以模块级 map 为主，不默认把 bundle chaining 提升成主线义务
4. 若进入 RazorVue / bundle 相关活跃 lane，应以当前状态页和计划文档为准

相关执行入口：

- [Compiler 当前状态](../../../03-完成/compiler/status.md)
- [Emit 当前状态](../../../03-完成/emit/status.md)
- [SourceMap 实施清单](../../../02-计划/compiler/SourceMap.ImplementationChecklist.md)

## 3. 核心结论

当前已经固定的主结论只有这几条：

1. sourcemap 采用三层方案：
   - `SourceOrigin`
   - `SourceMapBuilder`
   - emit 落盘
2. sourcemap 的目标是“源级调试体验”，不是还原 lowered JS
3. tuple / deconstruct / pattern / with / collection 这类 lowering 允许一源多目标
4. synthetic 节点不应主导调试体验
5. broad compiler contract 仍不默认要求 bundle map chaining

这条结论当前只适用于 broad sourcemap program 的保守范围判断。
若进入 RazorVue 相关 active lane，应以仓库级状态与当前执行计划为准。

## 4. 文档分工

### 4.1 快速结论

- [SourceMap.DecisionSummary.md](./SourceMap.DecisionSummary.md)

用途：

- 快速回看已经定下来的方向
- 无需重读完整设计时可先查阅

### 4.2 完整设计

- [SourceMap.Design.md](./SourceMap.Design.md)

用途：

- 查看分层、边界、目标、非目标
- 理解为什么要这样做

### 4.3 实施顺序

- [SourceMap.ImplementationChecklist.md](../../../02-计划/compiler/SourceMap.ImplementationChecklist.md)

用途：

- 继续扩展或收敛实现时按步骤推进
- 确认先改哪些层、哪些文件、哪些测试

### 4.4 易踩坑

- [SourceMap.Pitfalls.md](./SourceMap.Pitfalls.md)

用途：

- 开工前先看一遍
- 避免在 parser、optimizer、synthetic、路径、hash 这些点上返工

### 4.5 硬规则

- [SourceMap.HardRules.md](./SourceMap.HardRules.md)

用途：

- 解决“不能再临场决定”的问题
- 后续实现时作为评审准绳

## 5. 推荐阅读顺序

### 5.1 只想知道最终结论

按这个顺序：

1. [SourceMap.DecisionSummary.md](./SourceMap.DecisionSummary.md)
2. [SourceMap.HardRules.md](./SourceMap.HardRules.md)

### 5.2 准备继续扩展

按这个顺序：

1. [SourceMap.DecisionSummary.md](./SourceMap.DecisionSummary.md)
2. [SourceMap.Design.md](./SourceMap.Design.md)
3. [SourceMap.HardRules.md](./SourceMap.HardRules.md)
4. [SourceMap.Pitfalls.md](./SourceMap.Pitfalls.md)
5. [SourceMap.ImplementationChecklist.md](../../../02-计划/compiler/SourceMap.ImplementationChecklist.md)

### 5.3 做代码评审

按这个顺序：

1. [SourceMap.HardRules.md](./SourceMap.HardRules.md)
2. [SourceMap.Pitfalls.md](./SourceMap.Pitfalls.md)
3. [SourceMap.Design.md](./SourceMap.Design.md)

## 6. 实现前必须再次确认的事项

继续扩展覆盖前，至少要再次确认：

1. tuple / deconstruct lowering 是否已经稳定
2. `ToKnRECMAScript()` 输出格式是否稳定
3. parser 生成 AST 的来源策略是否仍与文档一致
4. optimizer / rewriter 是否会重建节点并需要传播 origin
5. sourcemap 的配置面是否已经准备好

## 7. 一句话结论

若仅需了解“现在该从哪里继续推进 sourcemap”，就从这里出发：

1. 先看 [SourceMap.DecisionSummary.md](./SourceMap.DecisionSummary.md)
2. 再看 [SourceMap.HardRules.md](./SourceMap.HardRules.md)
3. 真正落实施工时按 [SourceMap.ImplementationChecklist.md](../../../02-计划/compiler/SourceMap.ImplementationChecklist.md) 执行
