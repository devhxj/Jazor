using System.Reflection;

namespace ECMAScript.Vben.Test;

[TestClass]
public sealed class VbenNavigationTargetResolverTests
{
    [TestMethod]
    public void Vben_NavigationTargetResolver_PathRoute_NormalizesHash()
    {
        var resolved = Resolve(new VbenRouteLocation
        {
            Path = "/ops",
            Hash = "logs"
        });

        Assert.IsNull(GetHref(resolved));

        var route = GetRoute(resolved);
        Assert.IsNotNull(route);
        Assert.AreEqual("/ops", route.Path);
        Assert.IsNull(route.Name);
        Assert.AreEqual("#logs", route.Hash);
    }

    [TestMethod]
    public void Vben_NavigationTargetResolver_PathAndNameRoute_PrefersPathVariant()
    {
        var resolved = Resolve(new VbenRouteLocation
        {
            Path = "/ops",
            Name = "ops.dashboard",
            Hash = "summary"
        });

        Assert.IsNull(GetHref(resolved));

        var route = GetRoute(resolved);
        Assert.IsNotNull(route);
        Assert.AreEqual("/ops", route.Path);
        Assert.IsNull(route.Name);
        Assert.AreEqual("#summary", route.Hash);
    }

    [TestMethod]
    public void Vben_NavigationTargetResolver_NameRoute_NormalizesHash()
    {
        var resolved = Resolve(new VbenRouteLocation
        {
            Name = "reports.daily",
            Hash = "summary"
        });

        Assert.IsNull(GetHref(resolved));

        var route = GetRoute(resolved);
        Assert.IsNotNull(route);
        Assert.IsNull(route.Path);
        Assert.AreEqual("reports.daily", route.Name);
        Assert.AreEqual("#summary", route.Hash);
    }

    [TestMethod]
    public void Vben_NavigationTargetResolver_HashOnlyRoute_RemainsNavigableRelativeTarget()
    {
        var resolved = Resolve(new VbenRouteLocation
        {
            Hash = "details"
        });

        Assert.IsNull(GetHref(resolved));

        var route = GetRoute(resolved);
        Assert.IsNotNull(route);
        Assert.IsNull(route.Path);
        Assert.IsNull(route.Name);
        Assert.AreEqual("#details", route.Hash);
    }

    [TestMethod]
    public void Vben_NavigationTargetResolver_WhitespaceHref_IsRejected()
    {
        var resolved = Resolve((VbenNavTarget)"   ");

        Assert.IsNull(GetHref(resolved));
        Assert.IsNull(GetRoute(resolved));
        Assert.IsFalse(GetIsNavigable(resolved));
    }

    private static object Resolve(VbenRouteLocation route)
        => Resolve((VbenNavTarget)route);

    private static object Resolve(VbenNavTarget target)
    {
        var resolverType = typeof(VbenAdminLayout).Assembly.GetType("ECMAScript.Vben.VbenNavigationTargetResolver");
        Assert.IsNotNull(resolverType);

        var resolveMethod = resolverType!.GetMethod("Resolve", BindingFlags.Public | BindingFlags.Static);
        Assert.IsNotNull(resolveMethod);

        return resolveMethod!.Invoke(null, [target])!;
    }

    private static string? GetHref(object resolved)
        => (string?)resolved.GetType().GetProperty("Href", BindingFlags.Public | BindingFlags.Instance)?.GetValue(resolved);

    private static VbenRouteLocation? GetRoute(object resolved)
        => (VbenRouteLocation?)resolved.GetType().GetProperty("Route", BindingFlags.Public | BindingFlags.Instance)?.GetValue(resolved);

    private static bool GetIsNavigable(object resolved)
        => (bool?)resolved.GetType().GetProperty("IsNavigable", BindingFlags.Public | BindingFlags.Instance)?.GetValue(resolved) ?? false;
}
