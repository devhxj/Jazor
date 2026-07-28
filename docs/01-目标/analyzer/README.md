# 静态分析器与生成器

> 对应源码：`src/Jazor.Analyzer/`

## 定位

`Jazor.Analyzer` 负责在编译早期诊断 ECMAScript authoring 中不受支持的类型、成员和运行时语义。它与 `Jazor.Compiler` 形成“提前诊断、使用点裁决”的两级支持边界。

Razor-to-Vue Hook 不属于基础 `Jazor` 包。Hook 由独立的 `Jazor.Vue` 包安装，具体实现位于 `Jazor.RazorVue.Generator`；这使不使用 Razor 的类库无需承担 Razor 扫描和生成器 Hook。

## 责任范围

1. 对标记为 ECMAScript authoring 的代码执行白名单和类型边界诊断。
2. 对泛型实参、数组元素、局部推断和集合表达式等擦除位置执行更早的入口检查。
3. 对运行时别名歧义、非法 `[Spread]` 使用等编译期问题提供明确诊断。
4. 与 Razor-to-Vue 包协同，但不拥有 Razor 组件 lowering、catalog 生成或 Vue 产物物化职责。

## 分层关系

```text
Jazor.CLR / ECMAScript module declarations
            -> whitelist generation
            -> Jazor.Compiler lookup
            -> Jazor.Analyzer early diagnostics

Jazor.Vue
            -> Jazor.RazorVue.Generator Hook
            -> final Compilation component binding
            -> Jazor.Compiler lowering
```

分析器可以比编译器更早、更严格地诊断闭合的外部类型；但实际运行时敏感的成员和类型裁决仍必须在 `Jazor.Compiler` 的使用点完成。

## 关键约束

- 不通过静默 JavaScript fallback 掩盖不支持的宿主语义。
- 不在分析器中复制 Razor 编译器已经完成的参数绑定和组件契约校验。
- Razor-to-Vue 的生产输入是官方 Razor Source Generator 完成后的最终 `Compilation`。
- `EnableRazorHostOutputs`、Razor DR/IR 和生成 C# 二次解析不属于生产输入边界。

## 主要诊断

| 诊断 | 含义 |
| --- | --- |
| `JAZOR001` | 不受支持的 ECMAScript 类型、成员或运行时语义 |
| `JAZOR002` | 运行时别名存在歧义 |
| `JAZOR003` / `JAZOR004` | 非法或冲突的 `[Spread]` 使用 |
| `JAZORVGA020` | Razor-to-Vue final Compilation 绑定失败 |
