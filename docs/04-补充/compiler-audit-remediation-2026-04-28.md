# Compiler 审查整改状态（2026-04-28）

本文对应最近一轮编译器专项审查清单，给出当前整改状态与可追溯锚点（实现/测试）。

## 总览

- 状态结论：`Critical/Major` 项已落地；`Medium/Low` 关键风险已覆盖；负面测试面显著增强。
- 覆盖策略：优先补“拒绝路径”与“稳定性契约”测试，再补诊断可观测性与性能基线。

## Critical

### 1) NotSupport 路径无测试覆盖
- 状态：`已完成`
- 关键锚点：
  - 全量拒绝处理器反射回归：[src/Jazor.CompilerTest/SemanticWalkerNotSupportTest.cs:518](../../src/Jazor.CompilerTest/SemanticWalkerNotSupportTest.cs)
  - 典型拒绝路径样例（`lock`）：[src/Jazor.CompilerTest/SemanticWalkerNotSupportTest.cs:194](../../src/Jazor.CompilerTest/SemanticWalkerNotSupportTest.cs)

## Major

### 1) SemanticWalker 缺少 CancellationToken
- 状态：`已完成`
- 关键锚点：
  - Walker 持有 token： [src/Jazor.Compiler/core/SemanticWalker.cs:1176](../../src/Jazor.Compiler/core/SemanticWalker.cs)
  - Visit 入口取消检查： [src/Jazor.Compiler/core/SemanticWalker.cs:1277](../../src/Jazor.Compiler/core/SemanticWalker.cs)
  - 取消测试： [src/Jazor.CompilerTest/SemanticWalkerNotSupportTest.cs:415](../../src/Jazor.CompilerTest/SemanticWalkerNotSupportTest.cs)

### 2) comparer 面缺少端到端发射断言
- 状态：`已完成`
- 关键锚点：
  - comparer concrete + interface E2E： [src/Jazor.CompilerTest/AstConverterTests.cs:5974](../../src/Jazor.CompilerTest/AstConverterTests.cs)

### 3) Import 去重/顺序稳定性缺乏系统断言
- 状态：`已完成`
- 关键锚点：
  - 跨模块乱序使用时按 module path 稳定发射： [src/Jazor.CompilerTest/AstConverterTests.cs:5896](../../src/Jazor.CompilerTest/AstConverterTests.cs)

### 4) base/derived 发射顺序无断言
- 状态：`已完成`
- 关键锚点：
  - 三层继承乱序声明，验证 base-before-derived： [src/Jazor.CompilerTest/AstConverterTests.cs:788](../../src/Jazor.CompilerTest/AstConverterTests.cs)

### 5) var 推断拒绝路径未覆盖
- 状态：`已完成`
- 关键锚点：
  - `var` 推断到不支持外部类型拒绝： [src/Jazor.CompilerTest/SemanticWalkerDeclarationTest.cs:1653](../../src/Jazor.CompilerTest/SemanticWalkerDeclarationTest.cs)

### 6) 负面测试数量不足
- 状态：`显著缓解（持续项）`
- 说明：已新增 NotSupport 全量处理器元测试 + 多类边界负例；后续仍可继续按新增功能面扩充。

## Medium

### 1) checked/unchecked 表达式
- 状态：`已完成`
- 关键锚点： [src/Jazor.CompilerTest/SemanticWalkerOrdinaryTest.cs:2122](../../src/Jazor.CompilerTest/SemanticWalkerOrdinaryTest.cs)

### 2) nameof() 表达式
- 状态：`已完成`
- 关键锚点： [src/Jazor.CompilerTest/SemanticWalkerOrdinaryTest.cs:2096](../../src/Jazor.CompilerTest/SemanticWalkerOrdinaryTest.cs)

### 3) using (expr) 语句形式
- 状态：`已完成`
- 关键锚点： [src/Jazor.CompilerTest/SemanticWalkerNotSupportTest.cs:382](../../src/Jazor.CompilerTest/SemanticWalkerNotSupportTest.cs)

### 4) Inline 模板边界负面测试
- 状态：`已完成`
- 关键锚点：
  - Legacy 占位符拒绝： [src/Jazor.CompilerTest/SemanticWalkerInlineTemplateTest.cs:40](../../src/Jazor.CompilerTest/SemanticWalkerInlineTemplateTest.cs)
  - 零位占位符拒绝： [src/Jazor.CompilerTest/SemanticWalkerInlineTemplateTest.cs:60](../../src/Jazor.CompilerTest/SemanticWalkerInlineTemplateTest.cs)
  - 非法表达式拒绝： [src/Jazor.CompilerTest/SemanticWalkerInlineTemplateTest.cs:70](../../src/Jazor.CompilerTest/SemanticWalkerInlineTemplateTest.cs)
  - 稀疏高位占位符参数不足拒绝： [src/Jazor.CompilerTest/SemanticWalkerInlineTemplateTest.cs:96](../../src/Jazor.CompilerTest/SemanticWalkerInlineTemplateTest.cs)
  - 解析失败包装： [src/Jazor.Compiler/core/SemanticWalker.cs.InlineTemplate.cs:81](../../src/Jazor.Compiler/core/SemanticWalker.cs.InlineTemplate.cs)

### 5) 属性索引器 lowering（非常规路径）
- 状态：`已完成（拒绝路径）`
- 关键锚点：
  - 多参数索引器读取拒绝： [src/Jazor.CompilerTest/SemanticWalkerReferenceTest.cs:3430](../../src/Jazor.CompilerTest/SemanticWalkerReferenceTest.cs)
  - 多参数索引器写入拒绝： [src/Jazor.CompilerTest/SemanticWalkerReferenceTest.cs:3473](../../src/Jazor.CompilerTest/SemanticWalkerReferenceTest.cs)

### 6) 构造函数重载冲突诊断信息不足
- 状态：`已完成`
- 关键锚点：
  - 冲突文案增强（参数个数 + 两侧签名）： [src/Jazor.Compiler/AstConverter.cs:928](../../src/Jazor.Compiler/AstConverter.cs)
  - 同参数个数冲突断言： [src/Jazor.CompilerTest/AstConverterTests.cs:664](../../src/Jazor.CompilerTest/AstConverterTests.cs)
  - 可选参数重叠冲突断言： [src/Jazor.CompilerTest/AstConverterTests.cs:694](../../src/Jazor.CompilerTest/AstConverterTests.cs)

### 7) HandleTransformationFailure 在 _report=null 可观测性风险
- 状态：`已完成`
- 关键锚点：
  - 统一异常位置元数据附加： [src/Jazor.Compiler/core/SemanticWalker.cs:1689](../../src/Jazor.Compiler/core/SemanticWalker.cs)
  - 操作异常元数据测试： [src/Jazor.CompilerTest/SemanticWalkerNotSupportTest.cs:436](../../src/Jazor.CompilerTest/SemanticWalkerNotSupportTest.cs)
  - 语法异常元数据测试： [src/Jazor.CompilerTest/SemanticWalkerNotSupportTest.cs:476](../../src/Jazor.CompilerTest/SemanticWalkerNotSupportTest.cs)

### 8) InlineTemplate Parser 实例未复用
- 状态：`已完成`
- 说明：模板按签名缓存，parse 一次后复用 AST 实例化路径；相关负面分支已补齐。

## Low

### 1) RegexOptions.Compiled 可移除
- 状态：`已完成`
- 关键锚点： [src/Jazor.Compiler/core/SemanticWalker.cs.InlineTemplate.cs:16](../../src/Jazor.Compiler/core/SemanticWalker.cs.InlineTemplate.cs)

### 2) goto label 拒绝路径
- 状态：`已完成`
- 关键锚点： [src/Jazor.CompilerTest/SemanticWalkerOrdinaryTest.cs:367](../../src/Jazor.CompilerTest/SemanticWalkerOrdinaryTest.cs)

### 3) SourceMap 大文件边界性能
- 状态：`已完成（基线测试）`
- 关键锚点： [src/Jazor.CompilerTest/SemanticWalkerSourceMapEmissionTest.cs:884](../../src/Jazor.CompilerTest/SemanticWalkerSourceMapEmissionTest.cs)

### 4) CRLF 行尾 SourceMap 锚点
- 状态：`已完成`
- 关键锚点： [src/Jazor.CompilerTest/SemanticWalkerSourceMapEmissionTest.cs:812](../../src/Jazor.CompilerTest/SemanticWalkerSourceMapEmissionTest.cs)

## 本轮回归结果（摘要）

- `Jazor.CompilerTest` 全量：`1623/1623` 通过
- `Jolt.Test` 全量：`774/774` 通过
- `Jazor.CLR.Test` 全量：`55/55` 通过
- `Jazor.EmitTest` 全量：`61/61` 通过
- `Jazor.RazorVue.Test` 全量：`430/430` 通过

