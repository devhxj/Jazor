# Jazor.CLR

> 定位：CLR 类型和成员到 JavaScript runtime 的白名单映射层。

`Jazor.CLR` 通过 `[Jazor]` 声明可支持的 .NET API，并为需要运行时 helper 的映射提供 JavaScript 语义实现。它是映射事实的 producer；诊断、lowering 和文件输出分别属于 `Jazor.Analyzer`、`Jazor.Compiler` 与 `Jazor.Emit`。

## 工作方式

```text
Jazor.CLR module/*.cs
  -> Jazor.Compiler.Generator
  -> Jazor.Compiler/WhiteList.cs.Generate.cs
  -> Jazor.Analyzer 和 Jazor.Compiler
  -> 按需导入 System/...Module.js
```

| `Op` | 用途 |
| --- | --- |
| `Discard` | 明确不支持的 API |
| `Allowed` | 可直接承接的 JavaScript 语义 |
| `Alias` | 类型或成员名称映射 |
| `Inline` | 简短、稳定的单表达式映射 |
| `Import` | 需要 helper 或复杂控制流的运行时实现 |
| `Compile` | 极窄的 compiler 内部 AST hook |

`out` / `ref` 映射使用数组返回协议：索引 `0` 是正常返回值，后续元素依声明顺序承载回写值。

## 修改映射

1. 通过 `Jazor.CLR.Generator` 从当前 BCL 符号生成模块骨架和参考文档；不要手写成员签名、哈希或 `doc/*.md`。
2. 在 `module/*.cs` 的生成骨架上选择恰当的 `Op`，复杂语义使用 `Import`，保持 concrete/interface 可达路径的一致性。
3. 运行 `Jazor.Compiler.Generator` 刷新 `WhiteList.cs.Generate.cs`；该文件不能手工编辑。
4. 为白名单元数据补 `Jazor.CLR.Test` 覆盖，并为 compiler emission 补 `Jazor.CompilerTest` 覆盖。

## 验证

```bash
dotnet build src/Jazor.CLR/Jazor.CLR.csproj --no-restore -v minimal
dotnet test src/Jazor.CLR.Test/Jazor.CLR.Test.csproj
dotnet test src/Jazor.CompilerTest/Jazor.CompilerTest.csproj
dotnet run --project src/Jazor.Compiler.Generator/Jazor.Compiler.Generator.csproj -- --version 0.26.3
```

需要生成或核对新模块骨架时：

```bash
dotnet run --project src/Jazor.CLR.Generator/Jazor.CLR.Generator.csproj -- .tmp/clr-scaffold
```

## 相关文档

- [模块参考](./doc/)
- [Jazor.Compiler](../Jazor.Compiler/README.md)
- [编译器架构](../../docs/02-architecture/compiler.md)
- [平台与绑定](../../docs/02-architecture/platform-and-bindings.md)
