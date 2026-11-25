using Jero.Core;
using Xunit;

namespace Jero.Test.Utils;

public class DefaultHomeMenuUtilTests
{
    [Fact]
    public void FindDefaultHomeMenu_ConfiguredPath_ReturnsMatchingMenuWithoutDependingOnTitle()
    {
        var menus = new[]
        {
            new SysMenu { Id = 1, Pid = 0, Title = "医线生机", Path = "/save-life", Name = "saveLife", Type = MenuTypeEnum.Dir },
            new SysMenu { Id = 2, Pid = 1, Title = "仪表盘", Path = "/save-life/dashboard", Name = "saveLifeDashboard", Type = MenuTypeEnum.Menu },
            new SysMenu { Id = 3, Pid = 0, Title = "工作台", Path = "/dashboard", Name = "dashboard", Type = MenuTypeEnum.Dir },
            new SysMenu { Id = 4, Pid = 3, Title = "工作台", Path = DefaultHomePathUtil.FallbackHomePath, Name = "home", Type = MenuTypeEnum.Menu },
        };

        var menu = DefaultHomeMenuUtil.FindDefaultHomeMenu(menus, " /save-life/dashboard?tab=today ");

        Assert.NotNull(menu);
        Assert.Equal(2, menu.Id);
    }

    [Fact]
    public void GetDefaultHomeMenuTree_ConfiguredPath_ReturnsAncestorsMenuAndDescendants()
    {
        var menus = new[]
        {
            new SysMenu { Id = 1, Pid = 0, Title = "医线生机", Path = "/save-life", Type = MenuTypeEnum.Dir },
            new SysMenu { Id = 2, Pid = 1, Title = "仪表盘", Path = "/save-life/dashboard", Type = MenuTypeEnum.Menu },
            new SysMenu { Id = 3, Pid = 2, Title = "查询", Permission = "saveLifeDashboard:page", Type = MenuTypeEnum.Btn },
        };

        var menuIds = DefaultHomeMenuUtil
            .GetDefaultHomeMenuTree(menus, "/save-life/dashboard")
            .Select(u => u.Id)
            .ToList();

        Assert.Equal([1, 2, 3], menuIds);
    }

    [Fact]
    public void FindDefaultHomeMenu_MissingConfiguredPath_FallsBackToFallbackHomePath()
    {
        var menus = new[]
        {
            new SysMenu { Id = 1, Pid = 0, Title = "医线生机", Path = "/save-life", Type = MenuTypeEnum.Dir },
            new SysMenu { Id = 2, Pid = 1, Title = "仪表盘", Path = "/save-life/dashboard", Type = MenuTypeEnum.Menu },
            new SysMenu { Id = 3, Pid = 0, Title = "控制台", Path = "/dashboard", Name = "dashboard", Type = MenuTypeEnum.Dir },
            new SysMenu { Id = 4, Pid = 3, Title = "首页", Path = DefaultHomePathUtil.FallbackHomePath, Name = "home", Type = MenuTypeEnum.Menu },
        };

        var menu = DefaultHomeMenuUtil.FindDefaultHomeMenu(menus, "/missing/home");

        Assert.NotNull(menu);
        Assert.Equal(4, menu.Id);
    }

    [Fact]
    public void ApplyDefaultHomeAffix_MovesAffixToConfiguredHomeMenu()
    {
        var fallbackHome = new SysMenu
        {
            Id = 1,
            Title = "工作台",
            Path = DefaultHomePathUtil.FallbackHomePath,
            IsAffix = true,
            Type = MenuTypeEnum.Menu,
        };
        var configuredHome = new SysMenu
        {
            Id = 2,
            Title = "仪表盘",
            Path = "/save-life/dashboard",
            IsAffix = false,
            Type = MenuTypeEnum.Menu,
        };
        var menus = new[] { fallbackHome, configuredHome };

        DefaultHomeMenuUtil.ApplyDefaultHomeAffix(menus, "/save-life/dashboard");

        Assert.False(fallbackHome.IsAffix);
        Assert.True(configuredHome.IsAffix);
    }
}
