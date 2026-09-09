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
    public void RouteCatalog_UsesFallbacksAndBuildsNavigationShape()
    {
        var routes = new[]
        {
            new AdminRouteDefinition
            {
                Key = "home",
                Path = "/",
                Title = "Home",
                Children =
                [
                    new AdminRouteDefinition { Key = "settings", Path = "/settings", Title = "Settings", Disabled = true }
                ]
            },
            new AdminRouteDefinition { Key = "reports", Path = "/reports", Title = "Reports" }
        };

        Assert.AreEqual("reports", AdminRouteCatalog.Resolve(routes, "/missing", "reports").Key);
        Assert.AreEqual("home", AdminRouteCatalog.Resolve(routes, "/missing", "unknown").Key);

        var navigation = AdminRouteCatalog.BuildNavigation(routes).AsArray!;
        Assert.AreEqual(2, navigation.Length);
        Assert.AreEqual("home", navigation[0].Key);
        var children = navigation[0].Children?.AsArray;
        Assert.IsNotNull(children);
        Assert.AreEqual(1, children.Length);
        Assert.AreEqual("settings", children[0].Key);
    }

    [TestMethod]
    public void RouteCatalog_BuildsRootBreadcrumbAndSortsExpandedKeys()
    {
        var routes = new[]
        {
            new AdminRouteDefinition { Key = "a", Path = "/a", Title = "A" },
            new AdminRouteDefinition { Key = "b", Path = "/b", Title = "B" }
        };

        var root = new AdminBreadcrumbItem { Key = "root", Title = "Root" };
        var breadcrumbs = AdminRouteCatalog.BuildBreadcrumbs(routes, "b", root);
        var expanded = AdminRouteCatalog.BuildExpandedKeys(routes, "missing", new[] { "z", "a" });

        Assert.AreEqual(2, breadcrumbs.Length);
        Assert.AreEqual("root", breadcrumbs[0].Key);
        Assert.AreEqual("b", breadcrumbs[1].Key);
        CollectionAssert.AreEqual(new[] { "a", "z" }, expanded);
    }

    [TestMethod]
    public void RouteCatalog_BuildsNestedRecordsAndCatchAllRedirect()
    {
        var routes = new[]
        {
            new AdminRouteDefinition
            {
                Key = "root",
                Path = "/root",
                Children =
                [
                    new AdminRouteDefinition { Key = "child", Path = "/root/child" }
                ]
            }
        };

        var records = AdminRouteCatalog.BuildRouteRecords(routes, new TestVueComponent(), "root");
        Assert.AreEqual(3, records.Length);
        Assert.IsNotNull(records[0].AsSingleView);
        Assert.AreEqual("/root", records[0].AsSingleView!.Path);
        Assert.IsNotNull(records[1].AsSingleView);
        Assert.AreEqual("/root/child", records[1].AsSingleView!.Path);
        Assert.IsNotNull(records[2].AsRedirect);
        Assert.AreEqual("/:pathMatch(.*)*", records[2].AsRedirect!.Path);
    }

    private sealed class TestVueComponent : ECMAScript.Vue.IVueComponent
    {
    }

}
