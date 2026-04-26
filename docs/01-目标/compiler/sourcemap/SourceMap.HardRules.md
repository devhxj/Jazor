# Jazor SourceMap 硬规则补充

## 1. 文档定位

本文档用于补充 [SourceMap.Design.md](./SourceMap.Design.md) 中尚未固定为硬约束的实现规则。

前面的设计文档已经说明了方向、分层和实施顺序；本文档只做一件事：

把已落地 baseline 及其后续扩展时不能再临场决定的规则提前固定下来。

这些规则优先级高于后续实现阶段的局部便利性。

## 2. 适用范围

本文档适用于当前 sourcemap baseline 以及沿该 baseline 继续扩面时的实现约束。

当前 broad compiler contract 仍以第一阶段边界作为主范围：

1. 模块级 `.mjs.map`
2. 普通表达式主链路
3. tuple / deconstruct 主链路
4. emit 落盘

也就是说：

1. baseline 已经落地
2. 文档继续沿用第一阶段边界来固定主线规则
3. bundle chaining 等更窄 active lane 不自动并入这里的广义契约

## 3. 归属边界

### Rule 1. 第一阶段 sourcemap 归属于编译器主链路

第一阶段 sourcemap 定义为 `compiler-owned sourcemap`。

这意味着：

1. `SemanticWalker` / `AstConverter` 负责给 AST 节点标源来源
2. `Jazor.Compiler` 内部 writer / builder 负责生成 source map
3. `Jazor.Emit` 负责把 `.mjs` 与 `.mjs.map` 落盘

第一阶段不把 sourcemap 视为独立工具链能力，也不把它下放给 bundler 或 host 层处理。

## 4. Catalog 与产物协议

### Rule 2. `.mjs` 是 catalog 主协议，`.map` 不是强制核心协议

第一阶段必须保证：

- `.mjs` 仍然是 catalog 主产物

第一阶段不要求：

- `.map` 必须永久成为 catalog 核心协议的一部分

实现建议：

- 允许 catalog 携带 `.map`
- 但 `.map` 必须保持为可关闭、可裁剪能力

原因：

- `.mjs` 是运行时必需产物
- `.map` 是调试辅助产物
- 不应让调试辅助产物无条件放大 generator 体积与程序集体积

### Rule 3. `sourcesContent` 不能默认无条件强制内嵌

第一阶段必须保留 `sourcesContent` 的开关能力。

建议默认策略：

- Debug：默认开启
- Release：默认关闭或由显式开关控制

无论最终默认值如何，文档和实现都必须支持关闭 `sourcesContent`。

## 5. SourceOrigin 模型

### Rule 4. `SourceOrigin` 是“主来源模型”，不是“多来源模型”

第一阶段的 `SourceOrigin` 只表达一个节点的主要来源。

它不表达：

- 多个来源同时参与合成
- 多来源优先级链
- 来源集合

后续若要增强调试质量，可以在后续阶段扩展，但第一阶段不得提前为多来源模型增加复杂度。

### Rule 5. `SourceOrigin` 必须允许“无真实文件路径”的来源表示

并非所有源都保证有稳定物理文件路径。

因此第一阶段必须允许：

- 逻辑路径
- 伪路径
- 或明确的“无物理文件”表示

不能把 `SourcePath` 是否是本地文件绝对路径当作前提。

## 6. Parser 生成 AST 的统一规则

### Rule 6. parser 生成节点默认不宣称精确源位置

凡是通过 `Parser` 重新生成的 AST 节点，例如：

- inline template 骨架
- import declaration

第一阶段默认规则是：

1. parser 生成的模板骨架节点默认视为 synthetic 或弱来源节点
2. 替换进去的实参 AST 保留其原始来源
3. 整个表达式或声明的根节点，可以回挂到调用点或父级语句来源

不允许把 parser 生成节点伪装成精确的 C# 源位置映射。

## 7. AST 重建与优化合同

### Rule 7. 所有 AST 重建点都必须显式处理 origin

凡是会新建 AST 节点的阶段，例如：

- optimizer
- rewriter
- inline template 替换
- 未来的任何 AST 规范化步骤

必须满足以下二选一：

1. 显式传播已有 origin
2. 明确在该阶段之后重新挂 origin

不允许 silent drop origin。

### Rule 8. 第一阶段必须固定“origin 挂载顺序”

第一阶段实现前，必须明确：

- origin 在优化前挂，还是优化后挂

一旦选定，不允许不同语法域各自决定。

建议：

- 以最终待输出 AST 为准
- 保证最终 AST 节点上的 origin 是完整可用的

## 8. Builder fallback 规则

### Rule 9. builder 遇到无 origin 节点时，不得伪造精确映射

第一阶段统一 fallback 规则：

1. 优先使用当前节点自身的 origin
2. 若当前节点无 origin，可退到已知父级语句来源
3. 若无法可靠退到父级，则跳过该节点映射
4. 必要时发出诊断或调试日志

不允许：

- 继承上一条 mapping 作为默认来源
- 为无来源节点伪造看似精确的 line/column

## 9. 最小颗粒度与优先级

### Rule 10. 第一阶段的最小颗粒度标准是“两级映射”

对已纳入第一阶段范围的语法域，最少必须保证：

1. 语句根节点可映射
2. 关键值表达式可映射

第一阶段不要求所有子节点都具备细粒度来源。

### Rule 11. 冲突映射时，优先选择开发者最容易理解的源语法片段

当某个 generated 节点可以合理对应多个来源时，优先级如下：

1. 开发者最可能设置断点、理解语义的源语法片段
2. 父级语句
3. lowering runtime 细节

换句话说：

- 优先还原“源代码体验”
- 不优先暴露“lowered 对象/临时变量体验”

## 10. Writer 一致性

### Rule 12. 带 map 与不带 map 的 JS 文本必须完全一致

同一份 AST：

- 普通 writer 生成的 JS
- sourcemap writer 生成的 JS

必须完全一致。

如果出现差异，默认视为 bug，而不是可接受实现差异。

这条规则高于实现便利性。

## 11. 失败与诊断

### Rule 13. JS 产物优先，map 可以独立降级失败

第一阶段默认退化策略：

1. JavaScript 产物成功优先
2. sourcemap 可以单独失败
3. sourcemap 失败不能 silent ignore
4. 必须提供可见诊断

除非未来另有明确决策，第一阶段不因为 map 构建失败而让整个模块 JS 输出失败。

### Rule 14. sourcemap 是可诊断能力，不是静默 best-effort

后续实现应预留诊断通道，用于报告：

- map 构建失败
- 路径规范化失败
- 关键节点无来源
- 配置冲突

## 12. 路径规则

### Rule 15. `sources` 的根路径定义必须唯一

第一阶段不允许使用“看情况决定”的相对路径规则。

必须明确唯一根路径语义，例如：

- 相对 project directory
- 或统一的逻辑 source root

一旦选定，测试与实现必须使用同一规则。

### Rule 16. `sourceMappingURL` 不得写绝对路径

第一阶段 `.mjs` 尾部的 `sourceMappingURL` 必须是相对文件名或相对路径。

不得输出本机绝对路径。

## 13. Bundle 边界

### Rule 17. 第一阶段 bundle 产物不承诺源级调试保真

第一阶段必须明确对外声明：

1. module output 追求 sourcemap 保真
2. bundle output 不承诺源级调试保真

在未实现 map chaining 之前，不允许把 bundle 产物描述成与 module output 同等级调试体验。

## 14. 测试规则

### Rule 18. 必须提供最小 source map 断言工具

第一阶段测试不能只做：

- 文件存在断言
- 整串 `mappings` 文本硬比较

必须提供至少一个最小断言工具，用于验证：

1. source map JSON 合法
2. `sources` / `mappings` 结构可解析
3. 某些关键 generated position 能映射回预期 original position

## 15. 结论

本文档固定的不是 sourcemap 的所有实现细节，而是第一阶段不能再模糊处理的规则。

后续继续扩展、评审或校验实现边界时：

- 设计方向以 [SourceMap.Design.md](./SourceMap.Design.md) 为准
- 实施顺序以 [SourceMap.ImplementationChecklist.md](../../../02-计划/compiler/SourceMap.ImplementationChecklist.md) 为准
- 风险规避以 [SourceMap.Pitfalls.md](./SourceMap.Pitfalls.md) 为准
- 硬约束以本文档为准
