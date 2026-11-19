# 框架集成层

> 适用范围：Jazor 核心平台与框架特定产品方向之间的边界。

## 核心与集成的关系

Jazor 核心负责 C# 到 ECMAScript 的语义转换，不以 Vue、React 或任何 UI 框架为前提。框架集成层在核心之上处理框架特有的作者入口、绑定、组件投影和产物 framing，并调用核心编译器完成所有 C# 表达式、成员、调用、导入和宿主映射语义。

## 当前实现

Jolt 从 `.jazor` 模板和 npm 包的公开声明绑定组件，通过 Jazor 核心编译 C# 并生成 Vue 渲染模块。 [Jolt 作者指南](../03-guides/jolt-authoring.md)

基于 Razor Source Generator 的 `.razor` 集成由独立仓库维护。 [RazorVue](https://github.com/devhxj/RazorVue)

## 集成边界

1. C# 到 JavaScript 语义由 `Jazor.Compiler` 统一编译，集成层通过正式 translation hooks 使用该能力。
2. 集成层需要 C# lowering、类型映射、导入、符号绑定、临时名或 source origin 时，使用 `Jazor.Compiler` 的正式 translation hooks。
3. 框架专属语义归属对应集成项目，`AstConverterProfile` 与 `SemanticWalker` 保持通用核心职责。
4. 核心层提供可组合、强类型的扩展契约；集成层通过显式声明选择标准 lowering 或诊断路径。
5. 新方向在实施前定义作者输入、编译语义边界、最终 artifact、诊断与 source-map 合同，并以独立契约记录其演进。

手写 ECMAScript 类型绑定通过 `[ECMAScript("<specifier>")]` 声明模块路径，通过 `[ECMAScriptName]` 或 `[Description("@#...")]` 声明导出名，通过 `[Style]` 声明样式依赖。Jolt 的 npm 组件输入来自项目依赖的类型声明和 ESM 导出。

这一分层让核心平台可被多个框架方向复用，并保持通用 C# -> ECMAScript 能力的独立性。
