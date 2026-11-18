# Jolt for Visual Studio

This VSIX registers `.jazor` in Visual Studio 2026 18.12 or later on Windows x64. TextMate highlights markup and directives. The Jolt language service adds Roslyn symbol coloring and distinguishes resolved component tags and props from HTML. Completion uses C# symbol kinds and descriptions, lists component parameter types, and puts required parameters first.

Install the CLI and build the VSIX from the repository root.

```powershell
dotnet tool install --global Jazor.Jolt --version 2.1.0-preview.4
dotnet build editors/jolt-visual-studio/Jolt.VisualStudio.csproj -c Release
```

Close Visual Studio and double-click `editors/jolt-visual-studio/bin/Release/net472/Jolt.VisualStudio.vsix` to install it. Use **File > Open > Folder** to open a Jolt project, then open a `.jazor` document. In [Jolt Counter](../../samples/Jolt.Counter), try completion inside a C# expression, use **Go To Definition** on `CollapsibleRoot`, and edit a member name without saving to verify diagnostics.

With the current CLI candidate you can also open `.jazor` files through a solution project, use **Find All References** and **Rename** on C# members or local component parameters, and run **Format Document**. Component file rename uses the client's workspace file operation support. Use the candidate build with the matching VSIX to exercise these commands. [Verification](../../docs/04-roadmap/jolt-editor-implementation-plan.md#候选验收)

Install `jolt-2.1.0-preview.4.vsix` through **Extensions: Install from VSIX** in VS Code. The Visual Studio package has its own identifier and filename.
