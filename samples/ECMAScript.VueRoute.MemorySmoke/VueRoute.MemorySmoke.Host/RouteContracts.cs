using System.ComponentModel;
using ECMAScript;

namespace VueRoute.MemorySmoke.Host;

[Description("@#")]
public sealed record DetailRouteProps : ECMAScript.Vue.VueProps
{
    [Description("@#id")]
    public string Id { get; init; } = "";

    [Description("@#source")]
    public string Source { get; init; } = "";
}

[Description("@#")]
public sealed record QueryEchoProps : ECMAScript.Vue.VueProps
{
    [Description("@#tab")]
    public string Tab { get; init; } = "";
}
