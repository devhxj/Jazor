# Jazor.CompilerTest

> 定位：`Jazor.Compiler` 的主回归测试项目。

测试关注 C# 使用点的可观察行为、模块输出契约和 source map 相关确定性，而不是以测试数量证明“支持全部 C# 语法”。

## 覆盖范围

| 分层 | 关注点 | 典型文件 |
| --- | --- | --- |
| 模块转换 | import/export、成员类、继承、构造函数和稳定命名 | `AstConverter*Tests.cs` |
| 语义 lowering | 表达式、控制流、模式匹配、集合、tuple、`ref/out` 与边界失败 | `SemanticWalker*Test.cs` |
| 输出基础设施 | helper、alias、optimizer 和白名单查找 | `UniqueNameAllocatorTests.cs`、`WhiteListLookupCompatibilityTests.cs` |
| SourceMap 与 descriptor | source-origin、map、descriptor node 和 materialization 交接 | `*SourceMap*Tests.cs`、`ESGenerator*Tests.cs` |

涉及真实运行时顺序、异步回调、unmount 或浏览器交互的场景，可补 DenoHost 或样例级验证；其余情况优先断言 `IOperation -> ESTree -> JavaScript` 的结构和文本契约。

## 编写原则

- 先说明作者可写的行为契约，再选择输出断言。
- 修改 host mapping、source map 或稳定命名时，同时覆盖成功路径和受控失败路径。
- 跨 `AstConverter` 与 `SemanticWalker` 的能力分别补模块级和语义级回归。
- 新增 synthetic temp、dispatcher 或 helper 时，证明其名称与输出顺序稳定。

## 运行

```bash
dotnet test src/Jazor.CompilerTest/Jazor.CompilerTest.csproj
dotnet test src/Jazor.CompilerTest/Jazor.CompilerTest.csproj --filter SemanticWalkerReferenceTest
dotnet test src/Jazor.CompilerTest/Jazor.CompilerTest.csproj --settings src/Jazor.CompilerTest/coverlet.runsettings --collect:"XPlat Code Coverage"
dotnet run --file scripts/csharp/verify-compiler-coverage.cs
```

正式覆盖率门槛由 `verify-compiler-coverage.cs` 统一执行；`coverlet.runsettings` 仅定义采集范围。

## 相关文档

- [Jazor.Compiler](../Jazor.Compiler/README.md)
- [编译器实现原则](../Jazor.Compiler/ImplementationPrinciples.md)
- [开发与测试](../../docs/03-guides/development-and-testing.md)
