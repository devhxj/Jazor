# Jazor SourceMap 决策摘要

## 1. 要解决的问题

这是一份短文档，只保留 sourcemap 方案里的最终结论，方便后续快速回看。

完整设计见：

- [SourceMap.Design.md](./SourceMap.Design.md)
- [SourceMap.ImplementationChecklist.md](../../../02-计划/compiler/SourceMap.ImplementationChecklist.md)

## 2. 最终决策

### 2.1 baseline 已落地，后续重点转向稳定化

当前事实：

- `SourceOrigin` baseline 已落地
- `ToKnRECMAScriptWithSourceMap(...)` 与 writer 侧 map 生成已接入主链路
- `ESGenerator` / `Jazor.Emit` 已能携带并物化模块级 `.mjs.map`

当前策略：

- 不再把 sourcemap 描述成“尚未实现”
- 继续把 temp 名、import alias、synthetic 节点和 map 内容稳定成长期契约
- 继续按语法域扩展覆盖面，而不是一次性追求全量精细映射

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

换言之：

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

### 2.7 broad compiler contract 仍以模块级 map 为主

当前主线已经做到：

1. 模块级 `.mjs.map`
2. 普通表达式主链路
3. tuple / deconstruct 主链路
4. emit 落盘

broad compiler contract 仍不默认要求：

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

## 3. 后续推进顺序

后续继续推进时按这个顺序：

1. 继续锁定 `SourceOrigin` 传播与 synthetic 标注规则
2. 扩展普通引用、赋值、调用之外的语法域覆盖
3. 补 tuple / pattern / collection / initializer 等高风险 lowering 回归
4. 继续锁定 `ToKnRECMAScriptWithSourceMap(...)` 与普通 writer 的文本一致性
5. 继续锁定 `ESGenerator` catalog 与 `Jazor.Emit` 物化协议
6. 最后补更细粒度结构测试和失败路径测试

## 4. 后续扩展验收标准

后续新增覆盖面时，至少应同时满足以下条件：

1. 模块级 `.mjs.map` 能稳定生成
2. 主要断点与异常位置能映射回 C# 源
3. tuple / deconstruct 的调试不会被 temp 节点主导
4. sourcemap 不改变现有 lowering 结果
5. 相关测试稳定通过

## 5. 一句话结论

Jazor 的 sourcemap 当前已经按“源来源标注 -> JS 输出建图 -> emit 落盘”三层方案落地主线 baseline；后续工作的重点不再是“要不要实现”，而是把覆盖面、稳定性和契约边界继续巩固。
