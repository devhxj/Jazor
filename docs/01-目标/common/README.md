# 公共契约与共享实现

> 对应源码：`src/ECMAScript.Contract/`、`src/Jazor.Common/`

## 为什么需要

迁移之后，仓库把“最小契约”和“可带依赖的共享实现”明确拆成了两层：

- `ECMAScript.Contract` 只保留最小、稳定、零依赖污染的契约。
- `Jazor.Common` 承载真正需要跨模块复用的实现、DTO 和工具。

这样可以避免把 Roslyn、JSON、emit、RazorVue 共享代码塞进 `ECMAScript.*` 命名空间，同时也避免各项目重复维护自己的格式化、协议和中间模型。

## 当前分层

### ECMAScript.Contract

最小契约层，保持无外部依赖：

- `JazorAttribute`
- `Op`
- `IUIComponent`
- `GlobalUsings` / `IsExternalInit`

其中 `JazorAttribute`、`Op` 当前都是仓库内部共享契约，通过 `InternalsVisibleTo` 在需要的程序集之间复用。

### Jazor.Common

共享实现层，允许带依赖：

- `Format`：统一签名格式和稳定 hash 命名
- `Jazor.Common.SourceMaps`：SourceMap 模型和写出
- `Jazor.Common.Emit`：manifest 与 emit 共享模型
- `Jazor.Common.VueContracts.*`：Jolt / 分析链路共享协议 DTO
- `Jazor.RazorVue*`：RazorVue 共享语义、描述符、lowering、artifact 模型

## 解决什么问题

1. **避免命名空间污染**：依赖包和复杂共享实现不再进入 `ECMAScript.*` 契约层。
2. **统一共享实现**：格式化、SourceMap、emit DTO、RazorVue 语义不再分散在多个旧项目里。
3. **稳定跨模块协作**：Analyzer、Compiler、Emit、Jolt、Generator 共享同一套低层契约和中间模型。

## 与其他项目的关系

```text
ECMAScript.Contract（最小契约）
    ├── Jazor.Compiler
    ├── Jazor.Analyzer
    ├── Jazor.Compiler.Generator
    ├── Jazor.Emit
    └── Jolt

Jazor.Common（共享实现）
    ├── Jazor.Compiler
    ├── Jazor.Analyzer
    ├── Jazor.Emit
    ├── Jolt
    └── ECMAScript.Vuetify
```

## 当前边界规则

- 只要类型需要保持 **无外部依赖、低波动、跨程序集共享**，优先放 `ECMAScript.Contract`。
- 只要类型需要 **Roslyn、JSON、SourceMap、协议 DTO、RazorVue 共享语义**，放 `Jazor.Common`。
- RazorVue 的对外命名空间仍可以叫 `Jazor.RazorVue`，但其物理实现已经并入 `Jazor.Common`。
