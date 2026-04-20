# Jazor SourceMap 决策摘要

## 1. 这份文档解决什么问题

这是一份短文档，只保留 sourcemap 方案里的最终结论，方便后续快速回看。

完整设计见：

- [SourceMap.Design.md](./SourceMap.Design.md)
- [SourceMap.ImplementationChecklist.md](./SourceMap.ImplementationChecklist.md)

## 2. 最终决策

### 2.1 现在先不实现

原因：

- 编译器主体还在继续完善
- tuple / pattern / collection / with 等 lowering 仍可能调整
- sourcemap 过早接入会放大维护成本

当前策略：

- 先把方案定下来
- 等编译器主体稳定后再实现

### 2.2 sourcemap 分三层实现

后续实现必须分三层：

1. `SourceOrigin` 标注层
   在 Acornima AST 节点上记录其对应的 C# 源位置。
2. `SourceMapBuilder` 构建层
   在输出 JavaScript 时，把 generated position 和 original position 编码成标准 source map。
3. Emit 落盘层
   写出 `.mjs`、`.mjs.map` 和 `sourceMappingURL`。

### 2.3 不在这些地方做

明确不采用以下方案：

1. 不在 `SemanticWalker` 里直接拼 `mappings`
2. 不在 emit 阶段从最终 JS 文本反推 source map
3. 不用注释或近似方案模拟 mapping
4. 不让 sourcemap 反向影响 lowering 语义

### 2.4 sourcemap 的标准

sourcemap 的目标不是"还原 lowered JS"，而是"保证源级调试体验"。

也就是说：

- 重点是断点、异常位置、单步体验
- 不是让每个 lowered token 都追到一个独立源点

### 2.5 lowering 统一规则

对 tuple、deconstruct、pattern、with、collection expression 等 lowering，统一采用这条规则：

1. 一个 C# 源节点映射到多个 JS 片段是允许的
2. 用户语义片段保留真实来源
3. 编译器临时变量和胶水节点标记为 synthetic
4. 调试体验优先回到用户写的代码，而不是临时 lowering 代码

### 2.6 tuple 的特别规则

tuple 在 Jazor 中是语法糖，不是 runtime 类型设计问题。

因此 sourcemap 也按同一标准处理：

- 强调源 tuple 视角
- 不强调运行时对象 key 细节
- temp 变量不应主导调试落点

### 2.7 第一阶段只做模块级 map

第一阶段明确只做：

1. 模块级 `.mjs.map`
2. 普通表达式主链路
3. tuple / deconstruct 主链路
4. emit 落盘

第一阶段明确不做：

1. bundle map chaining
2. token 级极致 mapping
3. 所有语法点一次性全覆盖

### 2.8 "全节点 sourcemap"边界

当前 active lane 里的"全节点"只覆盖 `SemanticWalker` 可输出 `Node` 的支持节点。
`NotSupport` 节点不强制提供 sourcemap。

补充规则：

1. `NotSupport`（抛错/返回 `null`）不纳入 sourcemap 保证
2. 由父节点统一消费、子节点自身不产出节点的场景（如 default case clause）不单独要求映射
3. `IImplicitIndexerReferenceOperation` 受 Roslyn 形态影响，采用条件覆盖（出现即断言）
4. `IAttributeOperation` 仅在实际产出 decorator 节点时要求附带 `SourceOrigin`

## 3. 实施顺序

后续实现时按这个顺序：

1. 新增 `SourceOrigin` 与 helper
2. 接入普通引用、赋值、调用、return
3. 接入 tuple / deconstruct
4. 新增 `ToKnRECMAScriptWithSourceMap(...)`
5. 扩 `ESGenerator`
6. 扩 `Jazor.Emit`
7. 最后补测试

## 4. 验收标准

只有同时满足以下条件，才算 sourcemap 第一阶段完成：

1. 模块级 `.mjs.map` 能稳定生成
2. 主要断点与异常位置能映射回 C# 源
3. tuple / deconstruct 的调试不会被 temp 节点主导
4. sourcemap 不改变现有 lowering 结果
5. 相关测试稳定通过

## 5. 一句话结论

Jazor 的 sourcemap 应该在"编译器主体稳定之后"，按"源来源标注 -> JS 输出建图 -> emit 落盘"三层方案实现，而不是提前混入当前仍在变化的 lowering 细节里。
