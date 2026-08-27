# Jazor.Compiler.Generator

> 定位：白名单与 `Op.Compile` 分发表的仓库内再生成工具。

该项目扫描源码中的 `[Jazor(...)]` 与 `[ECMAScriptModule(...)]` 声明，刷新 compiler 消费的白名单和分发代码。

## 职责

- 生成 `src/Jazor.Compiler/WhiteList.cs.Generate.cs`。
- 生成 `src/Jazor.Compiler/WhiteList.cs.Compile.cs`。
- 生成 `src/Jazor.Compiler/core/SemanticWalker.cs.Generate.cs`。

## 扫描范围与规则

当前扫描 `src/ECMAScript/`、`src/Jazor.CLR/`、`src/ECMAScript.Vue/` 与 `src/ECMAScript.Vuetify/`，不依赖运行时反射装载这些项目。Blazor framework 的 CLR mapping 只来自 `src/Jazor.CLR/`；`ECMAScript.Blazor` 是随 `Jazor.Vue` 交付的可选标准 ECMAScript 模拟/投影扩展，不是 whitelist source-root，也不携带 CLR runtime module。

- 未显式提供成员字符串时，key 必须来自 `symbol.OriginalDefinition.ToDisplayString(Jazor.Common.Format.NameFormat)`。
- `Op.Discard` 不进入可消费白名单；属性会展开为 getter/setter 记录。
- `Op.Import` 保留模块路径，供 compiler 进行导入收集。
- 生成器不得私自改写 key；新的共享 fallback 或规范化必须落在公共实现并补生成器和 compiler 测试。

## 运行

```bash
dotnet run --project src/Jazor.Compiler.Generator/Jazor.Compiler.Generator.csproj
```

修改上述扫描范围内的 `[Jazor]` 声明后，必须重新执行生成器并提交生成文件。`WhiteList.cs.Generate.cs` 及相关输出不应手工编辑。

## 相关文档

- [ECMAScript.Contract](../ECMAScript.Contract/README.md)
- [Jazor.CLR](../Jazor.CLR/README.md)
- [Jazor.Compiler](../Jazor.Compiler/README.md)
