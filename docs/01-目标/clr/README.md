# CLR 运行时支持与白名单机制

> 对应源码：`src/Jazor.CLR/`、`src/Jazor.CLR.Generator/`、`src/Jazor.Compiler.Generator/`

## 为什么需要

C# 到 JavaScript 的转换不是“看到什么 .NET API 都直接翻译”。只有那些已经被明确映射过、并且有稳定 JavaScript 语义承接的类型和成员，才允许进入编译域。`Jazor.CLR` 就是这套 producer 侧事实的声明源。

## 解决什么问题

1. **声明支持边界**：通过 `[Jazor(...)]` 明确声明哪些类型/成员被支持，以及采用 `Allowed` / `Alias` / `Inline` / `Import` / `Compile` 中哪一种策略。
2. **提供运行时实现**：对 `Op.Import` 这类需要 helper 或复杂语义的成员，提供真实 JavaScript 语义实现。
3. **自动生成白名单**：让编译器和分析器消费同一份由源码再生成出来的事实表，而不是手写维护。

## 大致实现思路

### 三层协作

```text
Jazor.CLR（producer 侧声明）
  用 [ECMAScriptModule] 声明模块路径
  用 [Jazor(...)] 声明类型和成员映射
       ↓
Jazor.Compiler.Generator / Jazor.CLR.Generator
  扫描源码，生成编译器可消费的白名单与绑定产物
       ↓
Jazor.Analyzer / Jazor.Compiler
  前者做更早诊断，后者在 lowering 时做最终裁决
```

### 模块示例

```csharp
[ECMAScriptModule("System/BooleanModule.js")]
[Jazor(Op.Alias, "bool", "Boolean")]
public static class BooleanModule
{
    [Jazor(Op.Inline, "override bool.GetHashCode()", "__arg1 ? 1 : 0")]
    public static extern Number BooleanGetHashCode(bool instance);
}
```

## 当前边界

- `Jazor.CLR` 是 **producer 侧事实来源**，不是 emit 或 bundling 层。
- 白名单再生成工具在 `src/Jazor.Compiler.Generator/`。
- 签名格式和稳定 hash 命名使用 `Jazor.Common.Format` 统一规则。

## 关键规则

- `Op.Discard` 表示显式不支持。
- `Op.Import` 适合复杂 helper 与异常协议。
- `Op.Inline` 只承接稳定单表达式模板。
- `Op.Compile` 只保留给极窄的编译器内部钩子。

## 参考文档

- `src/Jazor.CLR/doc/*.md`
- `docs/01-目标/compiler/WhiteList.md`
- `src/Jazor.CLR/README.md`
