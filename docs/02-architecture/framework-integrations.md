# 框架集成层

> 适用范围：Jazor 核心平台与框架特定产品方向之间的边界。

## 核心与集成的关系

Jazor 核心负责 C# 到 ECMAScript 的语义转换，不以 Vue、React 或任何 UI 框架为前提。框架集成层在核心之上处理框架特有的作者入口、绑定、组件投影和产物 framing，并调用核心编译器完成所有 C# 表达式、成员、调用、导入和宿主映射语义。

```text
Jazor core: C# / Roslyn IOperation -> ESTree -> ECMAScript modules
    ^
    |-- framework integration: framework-specific binding and artifact framing
            |-- current: Jazor.RazorVue
            |-- future candidates: Jazor.React, Jazor.RazorReact
```

`Jazor.React` 与 `Jazor.RazorReact` 记录潜在产品方向；当前公开 API 由 `Jazor.RazorVue` 组成。

## 当前实现

当前唯一已实现的框架集成是 `Jazor.RazorVue`：它以官方 Razor Source Generator 的最终 `Compilation` 为输入，绑定 `BuildRenderTree` 语义，并通过 Jazor 核心产出 Vue render-function `.mjs` 产物。具体边界见 [Razor-to-Vue](./razor-to-vue.md)。

## 集成边界

1. C# 到 JavaScript 语义由 `Jazor.Compiler` 统一编译，集成层通过正式 translation hooks 使用该能力。
2. 集成层需要 C# lowering、类型映射、导入、符号绑定、临时名或 source origin 时，使用 `Jazor.Compiler` 的正式 translation hooks。
3. 框架专属语义归属对应集成项目，`AstConverterProfile` 与 `SemanticWalker` 保持通用核心职责。
4. 核心层提供可组合、强类型的扩展契约；集成层通过显式声明选择标准 lowering 或诊断路径。
5. 新方向在实施前定义作者输入、编译语义边界、最终 artifact、诊断与 source-map 合同，并以独立契约记录其演进。

外部组件库使用 `[ECMAScript("<specifier>")]` 声明 ESM module specifier；导出名由 `[ECMAScriptName]` / `[Description("@#...")]` 给出，`default` 导出写成 `[ECMAScriptName("default")]`；样式边由可重复的 `[Style]` 声明。组件身份由约定判定：类型派生 `ComponentBase` 且实现对应 Vue marker，没有组件专用特性或参数。该特性描述静态 binding；Vue、React 等集成由各自适配器确认组件 marker 并拥有 rendering protocol。组件 Attribute 使用现行协议，引用方通过 lockstep 版本完成升级；核心依据显式 metadata 解析框架行为。

这一分层让核心平台可被多个框架方向复用，并保持通用 C# -> ECMAScript 能力的独立性。
