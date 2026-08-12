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

`Jazor.React` 与 `Jazor.RazorReact` 仅代表可能的未来产品方向，不是当前已发布或已支持的 API。

## 当前实现

当前唯一已实现的框架集成是 `Jazor.RazorVue`：它以官方 Razor Source Generator 的最终 `Compilation` 为输入，绑定 `BuildRenderTree` 语义，并通过 Jazor 核心产生 Vue render-function `.mjs` 产物。具体边界见 [Razor-to-Vue](./razor-to-vue.md)。

## 不可跨越的边界

1. 集成层不得实现第二套 C# 到 JavaScript 编译器，也不得对 C# 表达式做字符串拼接。
2. 集成层需要 C# lowering、类型映射、导入、符号绑定、临时名或 source origin 时，必须使用 `Jazor.Compiler` 的正式 translation hooks。
3. 框架专属语义应留在对应集成项目中，不能作为 Vue、React 或其他产品模式塞入 `AstConverterProfile` 或 `SemanticWalker` 核心特例。
4. 核心层只提供可组合、强类型的扩展契约；未被集成层明确声明的行为走标准 lowering 或明确失败。
5. 新方向必须先定义其作者输入、编译语义边界、最终 artifact、诊断与 source-map 合同，再进入实现，不以兼容旧探索路线为目标。

外部组件库可用 `ECMAScript.Contract.LibraryComponentAttribute` 声明 ESM module specifier 与 named export。该特性只使共享分析器识别包装类型；Vue、React 等集成应使用各自的派生 attribute 或适配器约定，并独立拥有 import 解释和组件 rendering protocol。中性契约不得成为由核心猜测框架行为的入口。

这一分层使核心平台可被多个框架方向复用，同时避免某一个框架的历史协议污染通用 C# -> ECMAScript 能力。
