using ECMAScript;
using ECMAScript.Vuetify;
using static ECMAScript.Vue;

namespace Wiki;

[ECMAScriptModule("main.mjs")]
public static class AppModule
{
    private static readonly VuetifyOptions VuetifyConfiguration = new()
    {
        Components = new VuetifyComponentRegistry
        {
            VBtn = VuetifyComponents.VBtn,
            VCard = VuetifyComponents.VCard,
            VTextField = VuetifyComponents.VTextField
        },
        Directives = new VuetifyDirectiveRegistry
        {
            Ripple = VuetifyDirectives.Ripple
        },
        Theme = new VuetifyThemeOptions
        {
            DefaultTheme = "light"
        },
        Display = new VuetifyDisplayOptions
        {
            MobileBreakpoint = "md"
        }
    };

    private static readonly RegExp VisibilityPattern = Global.RegExp(@"^\s*(?:public|private|protected|internal)\s+", "gm");
    private static readonly RegExp StaticPattern = Global.RegExp(@"^\s*static\s+", "gm");
    private static readonly RegExp TypedLocalPattern = Global.RegExp(@"^(\s*)(?:var|bool|byte|sbyte|short|ushort|int|uint|long|ulong|float|double|decimal|string|char|object)\s+([A-Za-z_][\w]*)\s*=", "gm");
    private static readonly RegExp TypedMethodPattern = Global.RegExp(@"^(\s*)([A-Za-z_][\w<>,\[\]\?]*)\s+([A-Za-z_][\w]*)\s*\(([^)]*)\)(\s*\{?)", "gm");
    private static readonly RegExp EmptyLinePattern = Global.RegExp(@"\n{3,}", "g");

    private static HTMLTextAreaElement? _input;
    private static HTMLElement? _output;
    private static readonly bool Initialized = Initialize();

    private static bool Initialize()
    {
        Boot();
        return true;
    }

    public static void Boot()
    {
        var app = Vue.CreateApp(WikiHomeModule.Component);
        app.Use(Vuetify.CreateVuetify(VuetifyConfiguration));
        app.Mount("#app");

        if (Global.Document.GetElementById("cs-input") is not HTMLTextAreaElement input ||
            Global.Document.GetElementById("js-output") is not HTMLElement output)
            return;

        _input = input;
        _output = output;

        input.AddEventListener("input", new EventListenerLiteral { HandleEvent = OnInputChanged }, false);
        RenderPreview();
    }

    private static void OnInputChanged(Event @event)
    {
        _ = @event;
        RenderPreview();
    }

    private static void RenderPreview()
    {
        if (_input is null || _output is null)
            return;

        var normalized = NormalizeInput(_input.Value);
        if (normalized.Length == 0)
        {
            _output.TextContent = "// Input is empty.";
            return;
        }

        var body = ConvertPreviewSource(normalized);

        _output.TextContent =
            "// jazor.wiki live preview\n" +
            "// This is a fast browser-side preview for authoring feedback.\n" +
            body;
    }

    private static string NormalizeInput(string value)
        => value.Replace("\r\n", "\n").Replace("\r", "\n").TrimEnd();

    private static string ConvertPreviewSource(string source)
    {
        var text = source;
        text = text.Replace(VisibilityPattern, string.Empty);
        text = text.Replace(StaticPattern, string.Empty);
        text = text.Replace("Console.WriteLine", "console.log");
        text = text.Replace(TypedLocalPattern, "$1let $2 =");
        text = text.Replace(TypedMethodPattern, "$1function $3($4)$5");
        text = text.Replace(EmptyLinePattern, "\n\n");
        return text;
    }
}
