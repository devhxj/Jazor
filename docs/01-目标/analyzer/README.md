# 静态代码分析器

> 对应源码：`src/Jazor.Analyzer/`

## 为什么需要

Jazor 的编译器并不接受任意 .NET 语义。很多错误如果等到 lowering 阶段才暴露，定位成本会很高。`Jazor.Analyzer` 的目标就是把这类错误尽可能提前到 IDE 和编译入口处，同时为 RazorVue 提供编译时分析与增量生成器宿主。

## 解决什么问题

1. **白名单守门**：对 `ECMAScript` 标注代码做类型和成员入口诊断。
2. **更早的 erased-position 诊断**：对泛型实参、数组元素、局部推断、集合表达式等位置做更严格的入口检查。
3. **RazorVue 编译时分析**：对组件 authoring、旧指令迁移、库组件声明等做专项诊断。
4. **RazorVue catalog 生成**：在编译时生成 RazorVue catalog。

## 大致实现思路

### ECMAScript 主线分析

```text
Jazor.CLR / ECMAScript / ECMAScript.Vue / ECMAScript.Vuetify
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

### RazorVue 分析与生成

`Jazor.Analyzer` 现在同时承载原 `Jazor.RazorVue.Analysis` 的职责：

- RazorVue authoring 诊断
- 增量生成器
- 兼容分析 RPC 的进程内/stdio 宿主

对外命名空间仍保留 `Jazor.RazorVue.Analysis`，但物理程序集已经是 `Jazor.Analyzer`。

## 关键约束

- 分析器负责 **尽早报错**，编译器负责 **最终使用点裁决**。
- erased positions 上允许分析器更严格，编译器不必机械追平。
- RazorVue 共享语义不在这里定义，而在 `src/Jazor.Common/RazorVue/`。

## 诊断面

- `JAZOR001` / `JAZOR002`：ECMAScript 主线白名单与 runtime alias 歧义
- `JAZORVUE*`：RazorVue authoring / 旧指令迁移
- `JAZORVGA*`：RazorVue generator / catalog / library component 规则
