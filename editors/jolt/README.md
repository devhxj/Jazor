# Jolt for VS Code

Install the matching `Jazor.Jolt` .NET tool, then download the `.vsix` from the [GitHub release](https://github.com/devhxj/Jazor/releases) and select **Extensions → Install from VSIX** in VS Code.

The client opens `.jazor` files as Jolt documents and starts the Jolt language server over stdio. On Windows it looks in the .NET global tool directory for `jolt.cmd`, then on PATH. Set `jolt.serverPath` to the packaged Jolt CLI executable or `Jolt.LanguageServer.dll` when developing from checkout. Use the packaged apphost to exercise npm tooling on Windows.

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
