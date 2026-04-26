# Transformation Roadmap

## 1. Purpose

本文档用于把 Jazor 当前语法转化体系拆成：

- 稳定契约能力
- 主链已接通能力
- 仍需继续巩固或扩展的能力
- 下一阶段建议动作

目标不是重复架构说明，而是为后续迭代提供一个可维护的路线图视图。
这里的状态分层只描述当前实现收敛程度，不应被读成“永久完成”的宣告。

## 2. Current Status Snapshot

### 2.1 稳定契约

- 宿主映射 `[Jazor(Op.*)]` 扫描
- `WhiteList.cs.Generate.cs` 自动生成
- `WhiteList.cs.Compile.cs` / `SemanticWalker.cs.Generate.cs` 基础设施生成
- `Jazor.Analyzer` 主输入约束
- `SemanticWalker` 主语义转换链路
- 编译测试回归主集

### 2.2 主链已接通

- `AstConverter` 模块级转换
- `Alias` / `Inline` / `Import` 白名单消费
- import 收集、合并、模块头输出主链
- enum / interface declaration 擦除
- 成员类继承子集
- 成员类构造函数重载 dispatcher
- `Optimizer` 作为 AST 后处理节点存在
- 文档体系：总览、简化版、端到端说明、扩展规范

### 2.3 继续巩固 / 扩展点

- compiler catalog / emit 物化与 sourcemap 稳定契约继续巩固
- `Op.Compile` 复杂宿主语义 contract 继续扩展
- `Inline` AST 模板边界继续收紧
- 更完整的泛型、跨模块继承、更深层嵌套类型增强

## 3. Work Buckets

## 3.1 Bucket A: Catalog / Emit Contract

目标：把 compiler 产 catalog 与 emit 物化的边界巩固成稳定契约。

范围：

- 稳定 `ESGenerator -> AstConverter/SemanticWalker -> JS Writer -> ModuleCatalog`
- 明确 `Jazor.Emit` 如何消费 catalog / source map carriers 并物化文件
- 明确 `Optimizer` 的接入位置

完成标准：

- compiler catalog 与 emit 物化职责不再混写
- 模块生成结果可被 emit 确定性消费
- 输出链路由测试覆盖

## 3.2 Bucket B: Import Stability

目标：把当前已接通的 import 主链路继续巩固为稳定契约。

范围：

- 锁定 `SenseArgument` 收集、`AstConverter.MergeImports(...)` 合并、`BuildImportDeclarations()` 输出的职责边界
- 保证 import 去重、命名稳定、顺序稳定
- 为别名冲突、跨方法收集、跨成员共享绑定补专项回归

完成标准：

- import 声明输出具备稳定断言
- 别名冲突、跨方法收集和共享绑定具备专项回归覆盖

## 3.3 Bucket C: Host Semantics Upgrade

目标：稳定 `Inline` / `Compile` 分工，减少宿主映射的结构性风险。

范围：

- 保留现有 `[Jazor(Op.Inline, ...)]` 的声明方式
- 内部升级为 AST 模板 + 占位符替换
- 先把表达式级复杂宿主语义提升到 `Op.Compile`
- 需要 temp/import/source-origin 的语义，等 `Compile` contract 扩展后再接

完成标准：

- 结构性表达式不再依赖“先字符串替换再 parse”
- `Inline` 和 `Compile` 的边界在文档和代码中一致
- `Compile` 第一阶段只承载表达式级 hook，不越界承担完整 lowering

## 3.4 Bucket D: Module Capability Upgrade

目标：增强模块层转换能力，但不破坏当前职责边界。

范围：

- 更深层嵌套类
- 泛型模块能力
- 当前继承子集之外的边界
- 更稳健的 partial / 多文件处理

完成标准：

- 新能力仍由 `AstConverter` 主导
- 不把模块级转换问题错误下沉到 `SemanticWalker`

## 4. Priority Recommendation

建议优先级：

1. Catalog / Emit Contract
2. Import Stability
3. Host Semantics Upgrade
4. Module Capability Upgrade

原因：

- 没有清晰 catalog / emit 契约，编译器主链路的工程边界会持续漂移
- import 主链虽然已接通，但稳定性直接影响宿主模块能力和输出确定性
- `Inline` 的结构风险会随着宿主映射增长而放大
- 模块能力增强应放在 compiler/emit 契约与 import 稳定性之后

## 5. Near-Term Action List

下一阶段建议拆成以下动作：

1. 明确 compiler catalog 与 emit 物化责任边界
2. 为 import 绑定、去重、别名冲突和顺序稳定补专项断言
3. 继续把 `Inline` AST 模板边界写死，避免旧字符串逻辑回流
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

- [ArchitectureOverview.md](../../01-目标/compiler/ArchitectureOverview.md)
- [ArchitectureOverview.Simplified.md](../../01-目标/compiler/ArchitectureOverview.Simplified.md)
- [SyntaxTransformationPipeline.md](../../01-目标/compiler/SyntaxTransformationPipeline.md)
- [TransformationClosureChecklist.md](./TransformationClosureChecklist.md)
- [InlineAstTemplateSpec.md](../../01-目标/compiler/InlineAstTemplateSpec.md)
- [OpCompileSpec.md](../../01-目标/compiler/OpCompileSpec.md)
- [OpCompileImplementationChecklist.md](./OpCompileImplementationChecklist.md)
- [WalkerExtensionSpec.md](../../01-目标/compiler/WalkerExtensionSpec.md)
