# Jazor.Compiler.Generator

`Jazor.Compiler.Generator` 是仓库内的白名单再生成工具。它扫描源码中的 `[Jazor(...)]` 与 `[ECMAScriptModule(...)]` 声明，刷新编译器消费的白名单和 `Op.Compile` 分发表。

## Responsibilities

- 扫描白名单声明源。
- 生成 `src/Jazor.Compiler/WhiteList.cs.Generate.cs`。
- 生成 `src/Jazor.Compiler/WhiteList.cs.Compile.cs`。
- 生成 `src/Jazor.Compiler/core/SemanticWalker.cs.Generate.cs`。

## Current Scan Roots

当前固定扫描这些源码树：

- `src/ECMAScript/`
- `src/Jazor.CLR/`
- `src/ECMAScript.Vue/`
- `src/ECMAScript.Vuetify/`

生成器按源码扫描工作，不依赖运行时反射装载这些项目。

## How It Works

1. 使用 Roslyn 解析扫描目录下的 `.cs` 文件。
2. 读取类型和成员上的 `[Jazor]` 声明。
3. 读取类型上的 `[ECMAScriptModule]` 模块路径。
4. 归一化成员签名，必要时生成稳定 hash 名。
5. 输出编译器白名单和 `Compile_*` 分发表。

## Generation Rules

- `Op.Discard` 会被过滤，不进入可消费白名单。
- `Op.Inline` / `Op.Compile` 的无名声明会自动按成员签名生成稳定 hash 名。
- 属性会展开成 getter / setter 两条成员白名单记录。
- `Op.Import` 会额外携带模块路径，供编译器后续导入收集使用。

## Dependency Model

- 项目引用只有 `ECMAScript.Contract`。
- 为了复用统一的签名格式与 hash 规则，生成器直接链接 `src/Jazor.Common/Format.cs` 源文件，而不是引用整个 `Jazor.Common` 程序集。
- 这样可以保持工具依赖面小，同时保证生成规则和运行时 lookup 规则一致。

## Run

在仓库根目录执行：

```powershell
dotnet run --project src/Jazor.Compiler.Generator/Jazor.Compiler.Generator.csproj
```

修改 `Jazor.CLR`、`ECMAScript`、`ECMAScript.Vue`、`ECMAScript.Vuetify` 中的 `[Jazor]` 声明后，都应该重新执行一次。

## Output Files

- `src/Jazor.Compiler/WhiteList.cs.Generate.cs`
- `src/Jazor.Compiler/WhiteList.cs.Compile.cs`
- `src/Jazor.Compiler/core/SemanticWalker.cs.Generate.cs`

这些文件都是生成产物，不应手改。

## Read Next

- [../ECMAScript.Contract/README.md](../ECMAScript.Contract/README.md)
- [../Jazor.CLR/README.md](../Jazor.CLR/README.md)
- [../Jazor.Compiler/README.md](../Jazor.Compiler/README.md)
