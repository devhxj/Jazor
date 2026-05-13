#!/usr/bin/env dotnet run
#:package DenoHost.Core@2.7.14
#:package DenoHost.Runtime.win-x64@2.7.14

using DenoHost.Core;

var consumerRoot = Path.GetDirectoryName(Path.GetDirectoryName(GetScriptPath()))
    ?? throw new InvalidOperationException("Cannot determine RazorVue.TodoList consumer root.");

await Deno.Execute(
    new DenoExecuteBaseOptions
    {
        WorkingDirectory = consumerRoot
    },
    args);

static string GetScriptPath([System.Runtime.CompilerServices.CallerFilePath] string path = "")
    => path;
