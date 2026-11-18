# Jolt for Visual Studio

This VSIX registers `.jazor` in Visual Studio 2026 18.12 or later on Windows x64. TextMate highlights markup and directives. The Jolt language service adds Roslyn symbol coloring and distinguishes resolved component tags and props from HTML. Completion uses C# symbol kinds and descriptions, lists component parameter types, and puts required parameters first.

Install the CLI and close Visual Studio before opening `Jolt.VisualStudio.vsix`. Download the VSIX from the matching GitHub release. [Jolt preview](https://github.com/devhxj/Jazor/releases/tag/v2.1.0-preview.5)

```powershell
dotnet tool install --global Jazor.Jolt --version 2.1.0-preview.5
```

Use **File > Open > Folder** to open a Jolt project, then open a `.jazor` document. In Jolt Counter, try completion inside a C# expression, use **Go To Definition** on `CollapsibleRoot`, and edit a member name without saving to verify diagnostics. [Jolt Counter](../../samples/Jolt.Counter)

You can also open `.jazor` files through a solution project, use **Find All References** and **Rename** on C# members or local component parameters, and run **Format Document**. Component file rename uses the client's workspace file operation support. [Verification](../../docs/04-roadmap/jolt-editor-implementation-plan.md#候选验收)

Build the VSIX from the repository root with `dotnet build editors/jolt-visual-studio/Jolt.VisualStudio.csproj -c Release`. The output is `editors/jolt-visual-studio/bin/Release/net472/Jolt.VisualStudio.vsix`.

Install `jolt-2.1.0-preview.5.vsix` through **Extensions: Install from VSIX** in VS Code.
