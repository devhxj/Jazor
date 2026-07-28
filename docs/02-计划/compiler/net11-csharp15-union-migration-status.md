# .NET 11 / C# 15 编译器兼容状态

> Status: 当前状态快照
> Date: 2026-07-28
> Scope: .NET 11 preview SDK、Roslyn 操作模型与 C# 15 union 合同

## 当前基线

- 解决方案按 `global.json` 固定的 .NET 11 preview SDK 构建。
- Roslyn 相关包与 SDK 编译器表面保持一致，避免分析器因版本不匹配而被跳过。
- union 优先使用 SDK 提供的 `System.Runtime.CompilerServices.UnionAttribute` 与 `IUnion`；只有语言原生 union 无法保持精确分支投影时，才使用带标签的显式 fallback。
- collection-expression 场景使用官方 `CollectionBuilderAttribute` 合同，不引入仓库自定义替代属性。
- 编译器与分析器共享对象字面量宿主判定，但分析器仍可在运行时敏感操作之前更早报告诊断。

## Razor-to-Vue 输入边界

当前 Razor-to-Vue 主线不读取 Razor IR，也不依赖 `RazorCodeDocument`、`RazorCSharpDocument` 或生成 C# 的二次解析。正式输入是官方 Razor Source Generator 完成后的最终 Roslyn `Compilation`：

```text
官方 Razor Source Generator
    -> 最终 Compilation
    -> Jazor.Vue Hook
    -> BuildRenderTree 绑定与 Roslyn IOperation
    -> Vue render-function .mjs
```

因此，RazorVue、Compiler 与 Emit 的当前验证应分别使用：

- `src/Jazor.RazorVue.Sg.Test`
- `src/Jazor.CompilerTest`
- `src/Jazor.EmitTest`

## Union 设计约束

- 分支之间互不赋值时，优先使用命名的原生 union。
- 分支存在继承关系、对象边界或 delegate/interface 精确投影要求时，使用带标签的 `[Union]` + `IUnion` 合同。
- 保留现有强类型赋值、集合构造、投影属性和必要的重载入口。
- 不以 `object`、开放泛型或无约束 fallback 扩大宿主 API。
- 仅在运行时语义确实需要时增加编译器特殊 lowering 或运行时保护。

## 验证入口

```text
dotnet build Jazor.slnx
dotnet test src/Jazor.CompilerTest/Jazor.CompilerTest.csproj
dotnet test src/Jazor.RazorVue.Sg.Test/Jazor.RazorVue.Sg.Test.csproj
dotnet test src/Jazor.EmitTest/Jazor.EmitTest.csproj
```

本页只记录当前 SDK 与语言合同。具体 lowering 原则以 `src/Jazor.Compiler/ImplementationPrinciples.md` 为准，具体物化行为以 `src/Jazor.Emit/README.md` 与 Emit 测试为准。
