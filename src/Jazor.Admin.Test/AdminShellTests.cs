using Jazor.Admin;

namespace Jazor.Admin.Test;

[TestClass]
public sealed class AdminShellTests
{
    [TestMethod]
    public void EmptyRouteCatalog_ResolvesWithoutIndexFailure()
    {
        var routes = Array.Empty<AdminRouteDefinition>();

        var resolved = AdminRouteCatalog.Resolve(routes, "/missing", "dashboard");
        var records = AdminRouteCatalog.BuildRouteRecords(routes, null!, "dashboard");

        Assert.AreEqual("dashboard", resolved.Key);
        Assert.AreEqual(0, records.Length);
        Assert.AreEqual(0, AdminRouteCatalog.BuildNavigation(routes).AsArray?.Length ?? 0);
    }

    [TestMethod]
    public void RouteCatalog_PreservesNestedSelectionAndExpandsAncestors()
    {
        var routes = new[]
        {
            new AdminRouteDefinition
            {
                Key = "root",
                Title = "Root",
                Children =
                [
                    new AdminRouteDefinition { Key = "child", Path = "/child", Title = "Child" }
                ]
            }
        };

        var breadcrumbs = AdminRouteCatalog.BuildBreadcrumbs(routes, "child");
        var expanded = AdminRouteCatalog.BuildExpandedKeys(routes, "child", null);

        Assert.AreEqual(2, breadcrumbs.Length);
        Assert.AreEqual("root", breadcrumbs[0].Key);
        Assert.AreEqual("child", breadcrumbs[1].Key);
        CollectionAssert.AreEqual(new[] { "root" }, expanded);
    }

    [TestMethod]
    public void NavigationTarget_PrefersRouteAndTrimsHrefFallback()
    {
        var route = AdminNavigationTargetResolver.Resolve(" /legacy ", (RouteLocationRaw)"/dashboard");
        var href = AdminNavigationTargetResolver.Resolve("  /legacy  ", null);
        var empty = AdminNavigationTargetResolver.Resolve("   ", null);

        Assert.IsTrue(route.HasRoute);
        Assert.IsFalse(route.HasHref);
        Assert.AreEqual("/legacy", href.Href);
        Assert.IsFalse(empty.IsNavigable);
    }
}
