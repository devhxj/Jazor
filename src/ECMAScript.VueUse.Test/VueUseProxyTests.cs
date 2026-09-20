using System.Reflection;
using ECMAScript;

#pragma warning disable CA1416

namespace ECMAScriptVueUseTest;

/// <summary>
/// Reflection checks for the VueUse host surface: import host, strongly typed shapes, curated
/// composable surface, and the repository rule that public parameters never degrade to object.
/// 宿主表面反射检查：import 宿主、强类型形状、策展 composable 表面与禁用 object 的仓库规则。
/// </summary>
[TestClass]
public sealed class VueUseProxyTests
{
    [TestMethod]
    public void VueUse_ImportHost_UsesTheCoreEntry()
    {
        var runtime = typeof(VueUse).GetCustomAttribute<ECMAScriptAttribute>();
        Assert.IsNotNull(runtime);
        Assert.AreEqual("@vueuse/core", runtime!.Import);
    }

    [TestMethod]
    public void VueUse_RuntimeShapes_DoNotExposeObjectAsParameters()
    {
        var methods = typeof(VueUse)
            .GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)
            .Where(static method => !method.IsSpecialName)
            .ToArray();

        Assert.IsTrue(methods.Length >= 69, $"Expected the curated surface of at least 69 composables, found {methods.Length}.");
        foreach (var method in methods)
        {
            foreach (var parameter in method.GetParameters())
                Assert.IsFalse(parameter.ParameterType == typeof(object), $"{method.Name}({parameter.Name}) must stay strongly typed.");

            Assert.IsFalse(method.ReturnType == typeof(object), $"{method.Name} return must stay strongly typed.");
        }
    }

    [TestMethod]
    public void VueUse_StaticApi_ExposesTheCuratedGroups()
    {
        var methods = typeof(VueUse)
            .GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)
            .Where(static method => !method.IsSpecialName)
            .Select(static method => method.Name)
            .ToHashSet(StringComparer.Ordinal);

        // One representative from each curated group must be present.
        foreach (var name in new[]
                 {
                     "UseMouse", "UseMouseInElement", "UseWindowSize", "UseWindowScroll", "UseElementSize",
                     "UseElementBounding", "UseElementVisibility", "UseIntersectionObserver", "UseResizeObserver",
                     "UseDocumentVisibility", "UseWindowFocus", "UseActiveElement", "UseMediaQuery", "UsePreferredDark",
                     "UsePreferredColorScheme", "UsePreferredLanguages", "UseOnline", "UseNetwork", "UseBattery",
                     "UseDevicePixelRatio", "UseFps", "UseIdle", "UsePageLeave",
                     "UseStorage", "UseLocalStorage", "UseSessionStorage", "UseClipboard", "UseDark", "UseColorMode",
                     "UseTitle", "UseFavicon", "UseFullscreen",
                     "UseNow", "UseTimestamp", "UseTimeAgo", "UseIntervalFn", "UseTimeoutFn", "UseRafFn",
                     "OnClickOutside", "OnKeyDown", "OnKeyStroke", "OnKeyUp", "OnLongPress", "UseEventListener",
                     "UseToggle", "UsePrevious", "UseDebounceFn", "UseThrottleFn", "CreateEventHook", "UseAsyncState",
                     "ComputedAsync", "ComputedEager", "IsClient", "IsDef", "IsIOS", "IsWorker",
                     "TryOnMounted", "TryOnScopeDispose", "TryOnUnmounted",
                     "UseBase64", "UseCountdown", "UseSupported", "UsePreferredReducedMotion", "UseCssVar",
                     "UseElementHover", "UseKeyModifier", "OnStartTyping", "UseSorted", "UseTextSelection"
                 })
            Assert.IsTrue(methods.Contains(name), $"Curated composable '{name}' is not bound.");
    }

    [TestMethod]
    public void VueUse_RefReturningComposables_UseTheVueRefContracts()
    {
        var methods = typeof(VueUse)
            .GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)
            .Where(static method => !method.IsSpecialName)
            .ToArray();

        static Type? Return(MethodInfo[] methods, string name) => methods.SingleOrDefault(method => method.Name == name)?.ReturnType;

        Assert.AreEqual(typeof(Vue.VueShallowRef<Element?>), Return(methods, "UseActiveElement"));
        Assert.AreEqual(typeof(Vue.VueComputedRef<bool>), Return(methods, "UseMediaQuery"));
        Assert.AreEqual(typeof(Vue.VueComputedRef<bool>), Return(methods, "UsePreferredDark"));
        Assert.AreEqual(typeof(Vue.VueReadonlyRef<bool>), Return(methods, "UseOnline"));
        Assert.AreEqual(typeof(Vue.VueShallowRef<Number>), Return(methods, "UseTimestamp"));
        Assert.AreEqual(typeof(Vue.VueShallowRef<Date>), Return(methods, "UseNow"));
        Assert.AreEqual(typeof(Vue.IVueRef<string>), Return(methods, "UseStorage"));
        Assert.AreEqual(typeof(Vue.IVueRef<bool>), Return(methods, "UseDark"));
        Assert.AreEqual(typeof(Vue.IVueRef<bool>), Return(methods, "UseToggle"));
    }

    [TestMethod]
    public void VueUse_DisposableControls_ReturnCleanupDelegates()
    {
        var methods = typeof(VueUse)
            .GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)
            .Where(static method => !method.IsSpecialName)
            .ToArray();

        foreach (var name in new[] { "OnClickOutside", "OnKeyDown", "OnKeyStroke", "OnKeyUp", "OnLongPress", "UseEventListener" })
        {
            var method = methods.Single(candidate => candidate.Name == name);
            Assert.AreEqual(typeof(Action), method.ReturnType, $"{name} must return a cleanup delegate.");
        }
    }

    [TestMethod]
    public void VueUse_StringEnums_MirrorUpstreamValues()
    {
        AssertStringEnumMember<VueUseMouseSourceType>("mouse", nameof(VueUseMouseSourceType.Mouse));
        AssertStringEnumMember<VueUseMouseSourceType>("touch", nameof(VueUseMouseSourceType.Touch));
        AssertStringEnumMember<VueUseMouseCoordType>("page", nameof(VueUseMouseCoordType.Page));
        AssertStringEnumMember<VueUseColorScheme>("no-preference", nameof(VueUseColorScheme.NoPreference));
        AssertStringEnumMember<VueUseColorMode>("auto", nameof(VueUseColorMode.Auto));
        AssertStringEnumMember<VueUseDocumentVisibility>("visible", nameof(VueUseDocumentVisibility.Visible));
        AssertStringEnumMember<VueUseNetworkEffectiveType>("slow-2g", nameof(VueUseNetworkEffectiveType.Slow2G));
        AssertStringEnumMember<VueUseNetworkEffectiveType>("4g", nameof(VueUseNetworkEffectiveType.FourG));
    }

    [TestMethod]
    public void VueUse_MaybeElementUnion_UsesNet11Contract()
    {
        var unionType = typeof(VueUseMaybeElement);
        Assert.IsNotNull(unionType.GetCustomAttribute<System.Runtime.CompilerServices.UnionAttribute>(), unionType.FullName);
        Assert.IsTrue(typeof(System.Runtime.CompilerServices.IUnion).IsAssignableFrom(unionType), unionType.FullName);
    }

    private static void AssertStringEnumMember<TEnum>(string javaScriptValue, string memberName)
        where TEnum : struct, Enum
    {
        Assert.IsNotNull(typeof(TEnum).GetCustomAttribute<ECMAScript.StringAttribute>(), typeof(TEnum).FullName);
        var description = typeof(TEnum).GetField(memberName)!
            .GetCustomAttribute<System.ComponentModel.DescriptionAttribute>()!.Description;
        Assert.AreEqual("@#" + javaScriptValue, description, $"{typeof(TEnum).Name}.{memberName}");
    }
}
