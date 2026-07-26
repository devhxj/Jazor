# 公共契约与共享实现

> 对应源码：`src/ECMAScript.Contract/`、`src/Jazor.Common/`

## 为什么需要

迁移之后，仓库把“最小契约”和“可带依赖的共享实现”明确拆成了两层：

- `ECMAScript.Contract` 只保留最小、稳定、零依赖污染的契约。
- `Jazor.Common` 承载真正需要跨模块复用的实现、工具与 SourceMap 支撑。

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

## 解决什么问题

1. **避免命名空间污染**：依赖包和复杂共享实现不再进入 `ECMAScript.*` 契约层。
2. **统一共享实现**：格式化、SourceMap 和稳定命名规则不再分散在多个旧项目里。
3. **稳定跨模块协作**：Analyzer、Compiler、Emit、Generator 和 RazorVue adapter 共享同一套低层契约。

## 与其他项目的关系

```text
ECMAScript.Contract（最小契约）
    ├── Jazor.Compiler
    ├── Jazor.Analyzer
    ├── Jazor.Compiler.Generator
    └── Jazor.Emit

Jazor.Common（共享实现）
    ├── Jazor.Compiler
    ├── Jazor.Analyzer
    ├── Jazor.Emit
    └── Jazor.RazorVue

Jazor.RazorVue（RazorVue SG-result adapter）
    ├── Jazor.Analyzer
    └── Jazor.Compiler
```

## 边界规则

- 只要类型需要保持 **无外部依赖、低波动、跨程序集共享**，优先放 `ECMAScript.Contract`。
- 只要类型需要 **JSON + 通用 SourceMap 支撑**，放 `Jazor.Common`。
- 只要类型属于 **official Razor SG final-document adapter、generated C# binder 或 component selector**，放 `Jazor.RazorVue`。
- 不再为 RazorVue/Jolt 协议 DTO、旧 manifest/update-plan 模型保留公共共享位置；这些合同已经退役。
