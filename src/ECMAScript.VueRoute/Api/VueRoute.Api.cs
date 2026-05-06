using System.ComponentModel;

namespace ECMAScript;

public static partial class VueRoute
{
	[Description("@#createRouter")]
	public extern static Router CreateRouter(RouterOptions options);

	[Description("@#createWebHistory")]
	public extern static RouterHistory CreateWebHistory();

	[Description("@#createWebHistory")]
	public extern static RouterHistory CreateWebHistory(string basePath);

	[Description("@#createWebHashHistory")]
	public extern static RouterHistory CreateWebHashHistory();

	[Description("@#createWebHashHistory")]
	public extern static RouterHistory CreateWebHashHistory(string basePath);

	[Description("@#createMemoryHistory")]
	public extern static RouterHistory CreateMemoryHistory();

	[Description("@#createMemoryHistory")]
	public extern static RouterHistory CreateMemoryHistory(string basePath);

	[Description("@#useRouter")]
	public extern static Router UseRouter();

	[Description("@#useRoute")]
	public extern static RouteLocationNormalizedLoaded UseRoute();

	[Description("@#useLink")]
	public extern static UseLinkResult UseLink(UseLinkOptions options);

	[Description("@#onBeforeRouteLeave")]
	public extern static void OnBeforeRouteLeave(NavigationGuardHandler guard);

	[Description("@#onBeforeRouteUpdate")]
	public extern static void OnBeforeRouteUpdate(NavigationGuardHandler guard);

	[Description("@#isNavigationFailure")]
	public extern static bool IsNavigationFailure(Error error);

	[Description("@#isNavigationFailure")]
	public extern static bool IsNavigationFailure(Error error, NavigationFailureType type);

	[Description("@#RouterLink")]
	public extern static Vue3.IVueComponent<RouterLinkProps, RouterLinkSlots> RouterLink { get; }

	[Description("@#RouterView")]
	public extern static Vue3.IVueComponent<RouterViewProps, RouterViewSlots> RouterView { get; }
}
