# Jolt for VS Code

## Install

Install the Jolt CLI and download `jolt-2.2.0.vsix` from the same release. In VS Code, select **Extensions: Install from VSIX**. [Jolt release](https://github.com/devhxj/Jazor/releases/tag/v2.2.0)

```sh
dotnet tool install --global Jazor.Jolt --version 2.2.0
```

Open the project folder and then a `.jazor` file. In Jolt Counter, try completion in `@code` and go to definition on `CollapsibleRoot`. [Counter sample](../../samples/Jolt.Counter/README.md)

If the server fails to start with `spawn jolt ENOENT`, run `jolt --help` in a terminal to verify the CLI installation, then restart VS Code. The Windows client checks for `jolt.cmd` in the .NET global tool directory and can resolve it through PATH. Linux uses `jolt` from PATH. Set `jolt.serverPath` to the absolute path of a packaged CLI executable when you use a separate installation. [Jolt CLI setup](../../docs/03-guides/jolt-authoring.md#安装并运行)

## Develop the extension

Set `jolt.serverPath` to `Jolt.LanguageServer.dll` for a source checkout. Use the packaged apphost to exercise npm tooling on Windows.

Restore client dependencies with the repository Node host and build the language server from the repository root.

```sh
dotnet run --file scripts/csharp/restore-node-project.cs -- editors/jolt
dotnet build src/Jolt.LanguageServer
code --extensionDevelopmentPath=editors/jolt samples/Jolt.Counter
```

The standalone DLL lives under `src/Jolt.LanguageServer/bin/Debug/net11.0/` after the build. Open `App.jazor` to use Roslyn member completion, hover, signature help and definition navigation in C# regions, and npm tag and attribute suggestions in markup. Initial npm metadata loading uses the project's package manifest and lockfile.

The stdio regression can run without an editor.

```sh
dotnet run --file src/Jolt.LanguageServer/verify.cs
```

To exercise completion, references, rename, undo and formatting in a separate VS Code extension host, pass `--extensionTestsPath` with the absolute path to `editors/jolt/verify.cjs`. The test workspace needs a `Counter.jazor` containing `<p>@Count</p>` on the first line and `@code { private int Count = 1; }` on the second. Use a separate `--user-data-dir` and `--extensions-dir` for the test host. Set `JOLT_TEST_INSTALLED=1` to exercise discovery of an installed CLI, or `JOLT_TEST_SERVER` to select a candidate apphost.
