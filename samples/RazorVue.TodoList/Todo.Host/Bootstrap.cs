using System.ComponentModel;
using ECMAScript;
using static ECMAScript.Vue;

namespace Todo.Host;

/// <summary>Imports the generated TODOList root component from the emitted artifact graph.</summary>
[ECMAScript("components/todo-app.mjs")]
[Description("@#")]
internal static class TodoAppModule
{
#pragma warning disable CS0626 // The generated ECMAScript module supplies this export in the browser.
    [ECMAScriptName("default")]
    public extern static IVueComponent Default { get; }
#pragma warning restore CS0626
}

/// <summary>Evaluates styles and mounts the generated TODOList root component.</summary>
[ECMAScriptModule("app.mjs")]
public static class Bootstrap
{
    private static readonly bool started = Start();

    private static bool Start()
    {
        Todo.Library.TodoStyleSheet.EnsureLoaded();
        CreateApp(TodoAppModule.Default)
            .Provide(
                "jazor:service:Todo.Library.TodoBrowserService",
                new Todo.Library.TodoBrowserService { Label = "browser-provider" })
            .Mount("#app");
        return true;
    }
}
