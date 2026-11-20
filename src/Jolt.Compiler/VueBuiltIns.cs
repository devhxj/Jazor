using ECMAScript;

namespace Jolt.Compiler;

internal static class VueBuiltIns
{
    internal static readonly ComponentContract Teleport = new(nameof(Vue.Teleport), "vue",
        [
            new("to", "global::ECMAScript.Vue." + nameof(Vue.VueTeleportTarget), true),
            new("disabled", "bool", false),
        ], []) { IsVueBuiltIn = true };

    internal static readonly ComponentContract Transition = new(nameof(Vue.Transition), "vue",
        [
            new("name", "string", false),
            new("mode", "global::ECMAScript.Vue." + nameof(Vue.VueTransitionMode), false)
            {
                LiteralExpressions = new Dictionary<string, string>(StringComparer.Ordinal)
                {
                    ["in-out"] = "global::ECMAScript.Vue.VueTransitionMode." + nameof(Vue.VueTransitionMode.InOut),
                    ["out-in"] = "global::ECMAScript.Vue.VueTransitionMode." + nameof(Vue.VueTransitionMode.OutIn),
                },
            },
        ], [new("default", false)]) { IsVueBuiltIn = true };

    internal static IReadOnlyList<ComponentContract> Components { get; } = [Teleport, Transition];

    internal static ComponentContract? Find(string? name) => Components.FirstOrDefault(component => component.Name == name);
}
