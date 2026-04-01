# Transformation Roadmap

## 1. Purpose

本文档用于把 Jazor 当前语法转化体系拆成：

- 已闭环能力
- 基本闭环能力
- 未完全闭环能力
- 下一阶段建议动作

目标不是重复架构说明，而是为后续迭代提供一个可维护的路线图视图。

## 2. Current Status Snapshot

### 2.1 已闭环

- 宿主映射 `[Jazor(Op.*)]` 扫描
- `WhiteList.cs.Generate.cs` 自动生成
- `WhiteList.cs.Compile.cs` / `SemanticWalker.cs.Generate.cs` 基础设施生成
- `Jazor.Analyzer` 主输入约束
- `SemanticWalker` 主语义转换链路
- 编译测试回归主集

### 2.2 基本闭环

- `AstConverter` 模块级转换
- `Alias` / `Inline` / `Import` 白名单消费
- `Optimizer` 作为 AST 后处理节点存在
- 文档体系：总览、简化版、端到端说明、扩展规范

### 2.3 未完全闭环

- `ESGenerator` 真实 JavaScript 产物接回
- `ImportDeclaration` 最终落盘
- `Op.Compile` 复杂宿主语义体系化
- `Inline` 的 AST 模板化演进
- 更完整的泛型、继承、嵌套模块增强

## 3. Work Buckets

## 3.1 Bucket A: Output Closure

目标：把“AST 已生成”推进到“输出闭环稳定完成”。

范围：

- 打通 `ESGenerator -> AstConverter/SemanticWalker -> JS Writer`
- 把当前测试路径与真实生成路径统一到同一套 AST 输出链路
- 明确 `Optimizer` 的接入位置

完成标准：

- `ESGenerator` 不再输出占位 JavaScript
- 模块生成结果可直接用于真实输出
- 输出链路由测试覆盖

## 3.2 Bucket B: Import Closure

目标：把当前“只收集 import 规范”推进到“最终生成 import 声明”。

范围：

- `SenseArgument.MergeImportSpecifier` 的收集结果回填到模块顶层
- 明确 `AstConverter` / `ESGenerator` 谁负责最终 `ImportDeclaration`
- 保证 import 去重、命名稳定、顺序稳定

完成标准：

- `Op.Import` 能在最终 JavaScript 中稳定落盘
- import 声明具备测试覆盖

## 3.3 Bucket C: Host Semantics Upgrade

目标：减少字符串模板式 `Inline` 的结构风险。

范围：

- 保留现有 `[Jazor(Op.Inline, ...)]` 的声明方式
- 内部升级为 AST 模板 + 占位符替换
- 复杂宿主语义优先提升到 `Op.Compile`

完成标准：

- 结构性表达式不再依赖“先字符串替换再 parse”
- `Inline` 和 `Compile` 的边界在文档和代码中一致

## 3.4 Bucket D: Module Capability Upgrade

目标：增强模块层转换能力，但不破坏当前职责边界。

范围：

- 嵌套类
- 泛型模块能力
- 继承关系处理
- 更稳健的 partial / 多文件处理

完成标准：

- 新能力仍由 `AstConverter` 主导
- 不把模块级转换问题错误下沉到 `SemanticWalker`

## 4. Priority Recommendation

建议优先级：

1. Output Closure
2. Import Closure
3. Host Semantics Upgrade
4. Module Capability Upgrade

原因：

- 没有最终产物闭环，编译器主链路的工程价值受限
- import 不落盘会直接限制宿主模块能力
- `Inline` 的结构风险会随着宿主映射增长而放大
- 模块能力增强应放在主输出闭环之后

## 5. Near-Term Action List

下一阶段建议拆成以下动作：

1. 明确 `ESGenerator` 最终输出责任边界，并接回真实 JS 产物
2. 设计 `ImportDeclaration` 输出路径，打通 `Op.Import`
3. 落地 `Inline` AST 模板最小实现，优先替换高风险模板
4. 为 `Op.Compile` 建立统一实现约定与测试模板
5. 为模块层增强补充专项测试矩阵

## 6. Guard Rails

推进路线图时应坚持以下约束：

- 不手改生成白名单，修改回到 CLR/ECMAScript 标注源
- 不在 `ESGenerator` 中重新拼一套独立语义
- 不让 `Optimizer` 做改变语义的转换
- 不把模块级转换问题和语义级转换问题混到同一层处理
- 所有新能力必须先补测试，再扩展实现

## 7. Related Documents

- [ArchitectureOverview.md](./ArchitectureOverview.md)
- [ArchitectureOverview.Simplified.md](./ArchitectureOverview.Simplified.md)
- [SyntaxTransformationPipeline.md](./SyntaxTransformationPipeline.md)
- [TransformationClosureChecklist.md](./TransformationClosureChecklist.md)
- [InlineAstTemplateSpec.md](./InlineAstTemplateSpec.md)
- [WalkerExtensionSpec.md](./WalkerExtensionSpec.md)
