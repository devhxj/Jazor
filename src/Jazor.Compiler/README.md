# Jazor.Compiler

> 定位：受控 C# 编译域到 ECMAScript 的核心 lowering 层。

`Jazor.Compiler` 以 Roslyn `IOperation` 为语义输入，以 Acornima ESTree 为中间表示，输出确定的 JavaScript module text、source-origin 和 `Jazor.Generated.ModuleCatalog` 记录。它不直接物化文件，也不为任何框架产品内置特判。

## 职责

- `AstConverter` 负责模块、类型、导入、导出和运行时成员类的结构转换。
- `SemanticWalker` 负责表达式和语句 lowering，以及运行时敏感使用点的最终验证。
- `ESGenerator` 负责 JavaScript 文本、source map 和 `ModuleCatalog` carrier；`.mjs`、import map 和 bundle 由 `Jazor.Emit` 物化。
- WhiteList 与 `Alias`、`Inline`、`Import`、`Compile` 映射连接 CLR/host API 和 compiler lowering。

## 稳定边界

- 目标是保持使用点可观察行为和确定性输出，不是复刻完整 CLR runtime identity。
- 支持范围由已绑定的 Roslyn 语义、白名单映射和测试共同定义；不支持的运行时敏感类型或成员必须显式失败。
- `Jazor.Analyzer` 可以在 erased positions 提前诊断，`SemanticWalker` 仍在真正 lowering 处做最终裁定。
- interface 只作为编译期契约，不发射 runtime declaration；enum、tuple、record、`ref/out` 等按照各自已实现的擦除或协议语义处理。
- temp 名、import alias、helper 名、声明排序和 source-origin 锚点都是长期输出契约。

## 扩展位置

| 问题 | 归属 |
| --- | --- |
| 输入域的前置诊断 | `Jazor.Analyzer` |
| 外部 API 映射 | `Jazor.CLR` 或 `ECMAScript` 标注与 WhiteList 生成 |
| 模块级结构 | `AstConverter` |
| C# 表达式、语句和使用点验证 | `SemanticWalker` |
| 文件输出与 bundle | `Jazor.Emit` |

复杂且依赖 temp、import、source-origin 或语句级协议的 host 语义，必须通过正式 compiler extension seam 实现，不能绕过 compiler 手拼 JavaScript 或 AST。

## 代码结构

- `AstConverter.cs`：模块和声明转换。
- `core/SemanticWalker.cs.*`：语义 lowering 的分区实现。
- `ESGenerator*.cs`：JavaScript writer、source map 与 `ModuleCatalog` carrier。
- `WhiteList.cs.*`：宿主映射消费；`WhiteList.cs.Generate.cs` 为生成结果。
- `ImplementationPrinciples.md`：支持边界与设计理由的权威说明。

## 验证

```bash
dotnet test src/Jazor.CompilerTest/Jazor.CompilerTest.csproj
dotnet run --file scripts/csharp/verify-compiler-coverage.cs
```

## 相关文档

- [ImplementationPrinciples.md](./ImplementationPrinciples.md)
- [Jazor.CompilerTest](../Jazor.CompilerTest/README.md)
- [编译器架构](../../docs/02-architecture/compiler.md)
- [产物管线](../../docs/02-architecture/artifact-pipeline.md)
