
using Microsoft.Playwright;
using Xunit;

namespace Jero.Test.User;

public class UserTest : BaseTest
{
    public UserTest()
        : base(Environment.GetEnvironmentVariable("JERO_TEST_LOGIN_TOKEN"))
    {
    }

    public static bool BrowserTestsEnabled =>
        !string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("JERO_TEST_BASE_URL"));

    [Fact(Skip = "Set JERO_TEST_BASE_URL to an isolated running Jero host.", SkipUnless = nameof(BrowserTestsEnabled))]
    public async Task UserLogin_ValidCredentials_CompletesAuthentication()
    {
        await LoginAsync();

        await Expect(Page.GetByPlaceholder("请输入账号")).ToBeHiddenAsync(new LocatorAssertionsToBeHiddenOptions
        {
            Timeout = 30_000,
        });
    }

    [Fact(Skip = "Set JERO_TEST_BASE_URL to an isolated running Jero host.", SkipUnless = nameof(BrowserTestsEnabled))]
    public async Task AddUser_OpenForm_FillsBasicFields()
    {
        await AuthenticateAsync();
        await GoToUrlAsync("/#/system/user");

        await Expect(Page.GetByText("账号管理").First).ToBeVisibleAsync(new LocatorAssertionsToBeVisibleOptions
        {
            Timeout = 30_000,
        });

        await Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions
        {
            Name = "新增",
        }).First.ClickAsync();

        var dialog = Page.Locator(".user-form-dialog");
        await Expect(dialog).ToBeVisibleAsync(new LocatorAssertionsToBeVisibleOptions
        {
            Timeout = 30_000,
        });
        await Expect(dialog.GetByRole(AriaRole.Tab, new LocatorGetByRoleOptions
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
