# SourceMap 状态（2026-07-28）

> Status: 当前状态快照
> Positioning: SourceMap 在 Compiler、catalog 与 Emit 之间的状态与验收边界

## 总结

SourceMap 已进入可用基线阶段：编译器为可输出语义节点携带来源信息，`ESGenerator` 将 source-map carrier 写入 module catalog，`Jazor.Emit` 负责 `.mjs.map` 物化及 `sourceMappingURL` 关联。当前工作的重点是稳定性、覆盖范围和 Bundle 场景的验证，不是重新设计输入管线。

## 当前职责

```text
SemanticWalker
    -> SourceOrigin
    -> ESGenerator module catalog
    -> Jazor.Emit .mjs.map
    -> browser debugger
```

- `Jazor.Compiler` 负责语义节点与源位置的关联。
- `Jazor.Common` 提供共享 SourceMap 模型和格式化支持。
- `Jazor.Emit` 负责模块、source map、manifest 与 Bundle 的写出。
- RazorVue 只提供 Razor 组件 lowering 所需的源位置承接，不拥有独立的 SourceMap 协议。

## 已具备能力

- 模块级 `.mjs.map` 输出。
- 普通表达式、引用、赋值、调用与 `return` 的来源关联。
- tuple、deconstruct 等已支持语义节点的来源关联。
- module catalog 到 Emit 文件的稳定传递。
- source map 文件与 `sourceMappingURL` 的一致性检查。

## 明确边界

“全节点”仅指已支持且实际产出 ECMAScript AST 节点的语义操作。明确不支持的操作、由父节点统一处理的 operation，以及当前不产出 decorator 节点的属性路径，不伪造 SourceMap 映射。

Bundle source-map chaining 属于 Emit 的后续增强项。它必须建立在 module-level map 稳定、输入来源确定且输出路径可复现的基础上，不改变 Compiler 的 lowering 责任。

## 验证入口

- `src/Jazor.CompilerTest/SemanticWalkerSourceOriginTest.cs`
- `src/Jazor.CompilerTest/SemanticWalkerSourceMapEmissionTest.cs`
- `src/Jazor.CompilerTest/ESGeneratorSourceMapCatalogTest.cs`
- `src/Jazor.EmitTest/StaticModuleSourceMapTests.cs`
- [SourceMap 设计](../../01-目标/compiler/sourcemap/SourceMap.Design.md)
- [SourceMap 实施清单](../../02-计划/compiler/SourceMap.ImplementationChecklist.md)

## 当前缺口

- 扩大多 fixture 的确定性与性能证据。
- 继续验证 RazorVue 产物在真实浏览器调试器中的来源追踪。
- 在不破坏模块级映射的前提下完善 Bundle map chaining。
