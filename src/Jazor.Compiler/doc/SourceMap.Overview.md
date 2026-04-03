# Jazor SourceMap 文档总览

## 1. 文档定位

本文档是 Jazor sourcemap 方案的总入口。

它不重复完整设计，只回答三个问题：

1. 现在 sourcemap 处于什么状态
2. 各份文档分别解决什么问题
3. 后续真正开工时建议按什么顺序阅读

## 2. 当前状态

当前 sourcemap 在项目中的状态是：

- 设计已冻结主方向
- 实现明确延后
- 现阶段只维护文档，不改编译器行为

当前共识是：

1. 先完成编译器主体
2. 再实现 sourcemap
3. sourcemap 第一阶段只做模块级 map

## 3. 核心结论

当前已经固定的主结论只有这几条：

1. sourcemap 采用三层方案：
   - `SourceOrigin`
   - `SourceMapBuilder`
   - emit 落盘
2. sourcemap 的目标是“源级调试体验”，不是还原 lowered JS
3. tuple / deconstruct / pattern / with / collection 这类 lowering 允许一源多目标
4. synthetic 节点不应主导调试体验
5. 第一阶段不做 bundle map chaining

## 4. 文档分工

### 4.1 快速结论

- [SourceMap.DecisionSummary.md](./SourceMap.DecisionSummary.md)

用途：

- 快速回看已经定下来的方向
- 不想重新读完整设计时先看它

### 4.2 完整设计

- [SourceMap.Design.md](./SourceMap.Design.md)

用途：

- 查看分层、边界、目标、非目标
- 理解为什么要这样做

### 4.3 实施顺序

- [SourceMap.ImplementationChecklist.md](./SourceMap.ImplementationChecklist.md)

用途：

- 真正开始实现时按步骤推进
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

### 5.2 准备真正开工

按这个顺序：

1. [SourceMap.DecisionSummary.md](./SourceMap.DecisionSummary.md)
2. [SourceMap.Design.md](./SourceMap.Design.md)
3. [SourceMap.HardRules.md](./SourceMap.HardRules.md)
4. [SourceMap.Pitfalls.md](./SourceMap.Pitfalls.md)
5. [SourceMap.ImplementationChecklist.md](./SourceMap.ImplementationChecklist.md)

### 5.3 做代码评审

按这个顺序：

1. [SourceMap.HardRules.md](./SourceMap.HardRules.md)
2. [SourceMap.Pitfalls.md](./SourceMap.Pitfalls.md)
3. [SourceMap.Design.md](./SourceMap.Design.md)

## 6. 实现前必须再次确认的事项

真正开始实现前，至少要再次确认：

1. tuple / deconstruct lowering 是否已经稳定
2. `ToKnRECMAScript()` 输出格式是否稳定
3. parser 生成 AST 的来源策略是否仍与文档一致
4. optimizer / rewriter 是否会重建节点并需要传播 origin
5. sourcemap 的配置面是否已经准备好

## 7. 一句话结论

如果你只是想知道“以后从哪里开始继续做 sourcemap”，就从这里出发：

1. 先看 [SourceMap.DecisionSummary.md](./SourceMap.DecisionSummary.md)
2. 再看 [SourceMap.HardRules.md](./SourceMap.HardRules.md)
3. 真开工时按 [SourceMap.ImplementationChecklist.md](./SourceMap.ImplementationChecklist.md) 执行
