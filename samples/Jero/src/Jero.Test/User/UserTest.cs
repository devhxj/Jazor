
using Microsoft.Playwright;
using static Microsoft.Playwright.Assertions;
using Xunit;

namespace Jero.Test.User;

public class UserTest(BrowserHost host) : BaseTest(host), IClassFixture<BrowserHost>
{
    [Fact]
    public async Task UserLogin_ValidCredentials_CompletesAuthentication()
    {
        await LoginAsync();

        await Expect(Page.Locator(".jero-shell__main")).ToBeVisibleAsync(new LocatorAssertionsToBeVisibleOptions
        {
            Timeout = 30_000,
        });
    }

    [Fact]
    public async Task AddUser_OpenForm_FillsBasicFields()
    {
        await AuthenticateAsync();
        await GoToUrlAsync("/user");

        await Expect(Page.GetByText("账号管理").First).ToBeVisibleAsync(new LocatorAssertionsToBeVisibleOptions
        {
            Timeout = 30_000,
        });

        await Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions
        {
            Name = "新增",
        }).First.ClickAsync();

        var dialog = Page.GetByRole(AriaRole.Dialog);
        await Expect(dialog).ToBeVisibleAsync(new LocatorAssertionsToBeVisibleOptions
        {
            Timeout = 30_000,
        });
        await Expect(dialog.GetByRole(AriaRole.Button, new LocatorGetByRoleOptions
        {
            Name = "基础信息",
        })).ToBeVisibleAsync();

        var account = $"pw{DateTime.UtcNow:MMddHHmmssfff}";
        var phone = "17396157893";
        var realName = "测试1";

        var accountInput = dialog.Locator("input[placeholder='账号名称']");
        var phoneInput = dialog.Locator("input[placeholder='手机号码']");
        var realNameInput = dialog.Locator("input[placeholder='真实姓名']");

        await accountInput.FillAsync(account);
        await phoneInput.FillAsync(phone);
        await realNameInput.FillAsync(realName);

        await Expect(accountInput).ToHaveValueAsync(account);
        await Expect(phoneInput).ToHaveValueAsync(phone);
        await Expect(realNameInput).ToHaveValueAsync(realName);
    }
}
