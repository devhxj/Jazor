# Jolt for Visual Studio

This VSIX registers `.jazor` in Visual Studio 2026 (18.12 through 18.x) on Windows x64. TextMate highlights markup and directives. The Jolt language service adds Roslyn symbol coloring and distinguishes resolved component tags and props from HTML. Completion uses C# symbol kinds and descriptions, lists component parameter types, and puts required parameters first.

Install or update the existing `Jolt.VisualStudio.vsix` with Visual Studio closed. The migration candidate includes a C# editor component in this same installer. In an ordinary C# project that references the candidate Jazor NuGet package, F12 on a projected npm call opens the generated C# method. The navigation action in Quick Info opens the original TypeScript declaration at its line and column. These operations use the C# workspace and the project's Jazor projection.

For `.jazor` authoring, install the matching Jolt CLI as well. The released 2.6.0 tools are available from the release page. [Jolt release](https://github.com/devhxj/Jazor/releases/tag/v2.6.0)

```powershell
dotnet tool install --global Jazor.Jolt --version 2.6.0
```

Use **File > Open > Folder** to open a Jolt project, then open a `.jazor` document. In Jolt Counter, try completion inside a C# expression, use **Go To Definition** on `CollapsibleRoot`, and edit a member name without saving to verify diagnostics. [Jolt Counter](../../samples/Jolt.Counter)

You can also open `.jazor` files through a solution project, use **Find All References** and **Rename** on C# members or local component parameters, and run **Format Document**. Component file rename uses the client's workspace file operation support. [Editor capabilities](../../docs/03-guides/jolt-authoring.md#组件参数插槽和重构)

Build the VSIX from the repository root with `dotnet build editors/jolt-visual-studio/Jolt.VisualStudio.csproj -c Release`. The output is `editors/jolt-visual-studio/bin/Release/net472/Jolt.VisualStudio.vsix`.

If the language server fails to start, verify that `jolt --help` runs and that the global tool installation created `%USERPROFILE%\.dotnet\tools\jolt.cmd` (or the corresponding directory under `DOTNET_CLI_HOME`). Restart Visual Studio after installing the CLI. [Jolt CLI setup](../../docs/03-guides/jolt-authoring.md#安装并运行)
