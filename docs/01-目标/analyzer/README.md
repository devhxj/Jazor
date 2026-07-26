# 静态代码分析器

> 对应源码：`src/Jazor.Analyzer/`

## 为什么需要

Jazor 的编译器并不接受任意 .NET 语义。很多错误如果等到 lowering 阶段才暴露，定位成本会很高。`Jazor.Analyzer` 的目标是把这类错误尽可能提前到 IDE 和编译入口处，同时为 RazorVue 注册当前 Razor SG hook/bootstrap 生成器。

## 解决什么问题

1. **白名单守门**：对 `ECMAScript` 标注代码做类型和成员入口诊断。
2. **更早的 erased-position 诊断**：对泛型实参、数组元素、局部推断、集合表达式等位置做更严格的入口检查。
3. **Razor SG hook/bootstrap 诊断**：在 official SG final-document 输入不可用时 fail-fast，而不是退回旧 Razor DR/IR、SFC 或 Vue RPC 路径。

## 实现思路

### ECMAScript 主线分析

```text
Jazor.CLR / ECMAScript / ECMAScript.Vue* / ECMAScript.Vuetify
    └── [Jazor(...)] producer 侧声明
            ↓
Jazor.Compiler.Generator
    └── 生成 WhiteList.cs.*
            ↓
Jazor.Compiler
    └── 暴露 WhiteList / WhiteListLookup
            ↓
Jazor.Analyzer
    └── 在编译入口处做更早、更严格的诊断
```

这里的分析器可以比编译器更严格，但最终 runtime-sensitive 拒绝点仍然在 `Jazor.Compiler` 的实际 lowering 现场。

### RazorVue SG hook/bootstrap

`Jazor.Analyzer` 当前只承载 RazorVue 所需的 Roslyn generator/bootstrap 宿主：

- 注册受控 Razor SG tail hook。
- 暴露 official SG final-document 输入缺失时的诊断。
- 保留历史命名空间 `Jazor.RazorVue.Analysis` 的程序集承载关系，避免包边界断裂。

它不再承载旧 RazorVue authoring 诊断、Razor DR/IR 语义模型、SFC catalog 生成或 Vue 分析 RPC 宿主。

## 关键约束

- 分析器负责 **尽早报错**，编译器负责 **最终使用点裁决**。
- erased positions 上允许分析器更严格，编译器不必机械追平。
- RazorVue 的生产输入是 official Razor SG generated C#，最终 lowering 入口在 `Jazor.Compiler` / `SemanticWalker`。
- RazorVue 的 Razor SDK final-document 绑定边界在 `src/Jazor.RazorVue/RazorSdk/`。
- `Jazor.Analyzer` 只保留 Roslyn analyzer/generator/bootstrap 责任，不重新拥有 RazorVue core lowering、descriptor、旧 SFC catalog 或 RPC 语义。

## 诊断面

- `JAZOR001` / `JAZOR002`：ECMAScript 主线白名单与 runtime alias 歧义。
- `JAZORVGA020`：Razor SG tail hook 无法提供 official final-document 输入时的 fail-fast 诊断。
