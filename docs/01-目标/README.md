# 目标与设计

本目录记录 Jazor 各模块的产品定位、设计目标、职责边界和推荐实现方式。它回答“为什么存在”和“应当如何工作”，不承担当前构建状态或阶段性任务跟踪。

## 当前产品主线

Jazor 的核心产品链路是 C# 语义到 ECMAScript 模块的确定性转换；Razor-to-Vue 是基于该编译器和 Emit 能力构建的独立 opt-in 集成。

```text
C# / Razor
  -> Roslyn semantic model
  -> Jazor.Compiler
  -> ECMAScript AST
  -> .mjs / source map / manifest / bundle
```

Razor-to-Vue 的正式入口是 `Jazor.Vue`。它在官方 Razor Source Generator 完成后取得最终 `Compilation`，将生成的 `BuildRenderTree` 绑定并交给 `Jazor.Compiler` 降低。

## 模块索引

| 文档目录 | 责任范围 |
| --- | --- |
| `compiler/` | 编译器架构、转换管线和主线约束 |
| `compiler/semantic-walker/` | Roslyn 操作类型的降低规则 |
| `compiler/sourcemap/` | 源映射设计与物化约束 |
| `compiler/emit/` | 发射、物化、清单和打包 |
| `clr/` | CLR 映射声明与白名单生成链路 |
| `analyzer/` | 静态分析和编译期诊断 |
| `razorvue/` | Razor-to-Vue 目标、边界和实现落点 |
| `jazor.css/` | 结构化 CSS-in-JS 的产品定位、运行时合同与边界 |
| `ecmascript/` | ECMAScript 宿主契约和平台边界 |
| `ecmascript.vue3/` | Vue 3 类型绑定和 API 映射 |
| `ecmascript.pinia/` | Pinia 类型绑定和 API 映射 |
| `ecmascript.vueroute/` | Vue Router 类型绑定和 API 映射 |
| `jazor.admin/` | 管理后台壳层的目标设计 |
| `common/` | 共享契约、格式化和源映射支持 |
| `webidl/` | WebIDL 到 C# 绑定生成 |
| `tools/` | NuGet 打包和构建工具边界 |

历史探索、已冻结计划和不再指导当前实现的材料，不应与当前目标混合引用；相关内容统一归入 [遗弃材料](../05-遗弃/README.md)，或在原目录中明确标注为历史资料。

## 源码旁文档

与源码高度耦合的参考资料保留在对应项目中，例如 `src/Jazor.CLR/doc/`、`src/Jazor.Compiler/README.md` 和 `src/Jazor.Emit/README.md`。此类文档优先描述当前代码契约，仓库级设计文档负责解释模块之间的关系。
