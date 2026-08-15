# 开发与测试

> 面向：Jazor 仓库维护者与贡献者。

## 环境与构建

使用仓库 [global.json](../../global.json) 指定的 .NET SDK。在仓库根目录执行：

```bash
dotnet restore Jazor.slnx
dotnet build Jazor.slnx
dotnet run --file scripts/csharp/test-dotnet.cs
```

`Jazor.slnx` 是解决方案入口。仓库自动化采用 `scripts/csharp/` 下的单文件 C# 程序，不新增 PowerShell build、test、publish 或诊断 wrapper。

## 聚焦测试

| 领域 | 命令 |
| --- | --- |
| 编译器 | `dotnet test src/Jazor.CompilerTest/Jazor.CompilerTest.csproj` |
| Razor-to-Vue 集成 | `dotnet test src/Jazor.RazorVue.Sg.Test/Jazor.RazorVue.Sg.Test.csproj` |
| Emit 与 bundle | `dotnet test src/Jazor.EmitTest/Jazor.EmitTest.csproj` |
| CLR 映射 | `dotnet test src/Jazor.CLR.Test/Jazor.CLR.Test.csproj` |
| Vue Devtools binding | `dotnet test src/ECMAScript.Vue.Devtools.Test/ECMAScript.Vue.Devtools.Test.csproj` |
| Vue Data UI binding | `dotnet test src/ECMAScript.VueDataUi.Test/ECMAScript.VueDataUi.Test.csproj` |
| Vu Icons binding | `dotnet test src/ECMAScript.VuIcons.Test/ECMAScript.VuIcons.Test.csproj` |
| 单个编译器类别 | `dotnet test src/Jazor.CompilerTest/Jazor.CompilerTest.csproj --filter "SemanticWalkerPatternTest"` |

并行运行多个 `dotnet test` lane 时，应使用独立 `BaseOutputPath`；聚焦回归构建成功后优先使用 `--no-build`。测试创建的临时目录、端口、管道和进程标识必须隔离并在结束时清理。

## 覆盖率门槛

| 范围 | 最低要求 | 验证入口 |
| --- | --- | --- |
| 核心编译器 | 10,000 个通过场景、98% 行覆盖率、96% 分支覆盖率 | `dotnet run --file scripts/csharp/verify-compiler-coverage.cs` |
| Razor-to-Vue | 4,000 个通过场景、90% 行覆盖率、96% 分支覆盖率 | `dotnet run --file scripts/csharp/verify-razorvue-coverage.cs` |
| Vue 生态绑定 | 每个目标 90% 已审计公共绑定契约 | `dotnet run --file scripts/csharp/verify-vue-binding-coverage.cs` |

门槛是验证规则，不等同于任一历史报告中的固定通过数量。对当前结果的判断应运行相应脚本或测试命令。

## 改动边界

- 修改 `Jazor.CLR` 白名单来源后，运行 `Jazor.Compiler.Generator` 并提交重新生成的 `WhiteList.cs.Generate.cs`；不得手工修改生成文件。
- 新增编译语义时，优先保持求值顺序、副作用次数和最终结果，随后补充聚焦回归。
- RazorVue 的 C# 语义必须使用 `Jazor.Compiler` translation hooks；不要在集成层拼接 JavaScript 或重建 AST 语义。
- `Jazor.Admin` 是库，`samples/JazorAdmin` 是示例；测试与文档应分别说明它们的职责。

项目内脚本、测试说明和特殊验证路径见 [scripts/csharp README](../../scripts/csharp/README.md)。
