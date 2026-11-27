
using Microsoft.Playwright;
using static Microsoft.Playwright.Assertions;
using Xunit;

namespace Jero.Test;

/// <summary>
/// 浏览器端到端测试基类
/// </summary>
public abstract class BaseTest : IAsyncLifetime
{
    private const int DefaultTimeoutMilliseconds = 30_000;
    private const string DefaultLoginAccount = "superJero";
    private const string DefaultLoginPassword = "Jero++010101";
    private const string DefaultLoginCaptcha = "0";

    private readonly BrowserHost _host;
    private IPlaywright? _playwright;
    private IBrowserContext? _context;
    private string? _profile;

    protected BaseTest(BrowserHost host) => _host = host;

    protected IPage Page { get; private set; } = null!;

    public async ValueTask InitializeAsync()
    {
        _profile = TestWorkspace.Create("browser-profile");

        try
        {
            _playwright = await Playwright.CreateAsync();
            _context = await _playwright.Chromium.LaunchPersistentContextAsync(_profile,
                new BrowserTypeLaunchPersistentContextOptions
                {
                    BaseURL = _host.BaseUrl,
                    ArtifactsDir = _profile,
                    DownloadsPath = _profile,
                    IgnoreHTTPSErrors = true,
                    ViewportSize = new ViewportSize { Width = 1920, Height = 1080 }
                });
            Page = _context.Pages.FirstOrDefault() ?? await _context.NewPageAsync();
        }
        catch
        {
            await DisposeAsync();
            throw;
        }
    }

    public async ValueTask DisposeAsync()
    {
        try
        {
            if (_context is not null)
                await _context.CloseAsync();
        }
        finally
        {
            _playwright?.Dispose();
            if (_profile is not null && Directory.Exists(_profile))
                Directory.Delete(_profile, recursive: true);
        }
    }

    /// <summary>
    /// 使用 token 或默认账号完成登录。
    /// </summary>
    protected async Task AuthenticateAsync()
    {
        if (_host.IsExternal && NormalizeOptionalText(Environment.GetEnvironmentVariable("JERO_TEST_LOGIN_TOKEN")) is { } loginToken)
        {
            await Page.AddInitScriptAsync("localStorage.setItem('jero.jazor.access-token', " +
                System.Text.Json.JsonSerializer.Serialize(loginToken) + ");");
            await GoToUrlAsync("/workspace");
            return;
        }

        await LoginAsync();
    }

    /// <summary>
    /// 用户账号密码登录。
    /// </summary>
    protected async Task LoginAsync(string? account = null, string? password = null)
    {
        await Page.RunAndWaitForResponseAsync(() => GoToUrlAsync("/login"),
            response => response.Ok && response.Url.EndsWith("/api/sysAuth/loginConfig", StringComparison.OrdinalIgnoreCase));

        var login = Page.Locator(".login-page");
        var accountInput = login.GetByRole(AriaRole.Textbox).First;
        var passwordInput = login.Locator("input[type=password]");

        await Expect(accountInput).ToBeVisibleAsync(new LocatorAssertionsToBeVisibleOptions
        {
            Timeout = DefaultTimeoutMilliseconds,
        });
        await accountInput.FillAsync(ResolveEnvironmentValue("JERO_TEST_LOGIN_ACCOUNT", account, DefaultLoginAccount));
        await passwordInput.FillAsync(ResolveEnvironmentValue("JERO_TEST_LOGIN_PASSWORD", password, DefaultLoginPassword));

        var captchaInput = login.GetByPlaceholder("输入右侧字符");
        if (await captchaInput.CountAsync() > 0 && await captchaInput.IsVisibleAsync())
        {
            await captchaInput.FillAsync(ResolveEnvironmentValue("JERO_TEST_LOGIN_CAPTCHA", null, DefaultLoginCaptcha));
        }

        await login.GetByRole(AriaRole.Button, new LocatorGetByRoleOptions
        {
            NameRegex = new System.Text.RegularExpressions.Regex(@"^登\s*录$"),
        }).ClickAsync();

        await accountInput.WaitForAsync(new LocatorWaitForOptions
        {
            State = WaitForSelectorState.Hidden,
            Timeout = DefaultTimeoutMilliseconds,
        });
        await WaitForAppReadyAsync();
    }

    /// <summary>
    /// 打开指定页面
    /// </summary>
    /// <param name="url"></param>
    protected async Task GoToUrlAsync(string url)
    {
        await Page.GotoAsync(NormalizeRoute(url), new PageGotoOptions
        {
            Timeout = DefaultTimeoutMilliseconds,
            WaitUntil = WaitUntilState.DOMContentLoaded,
        });
        await WaitForAppReadyAsync();
    }

    /// <summary>
    /// 等待文档和 SPA 首屏脚本进入可交互状态。
    /// </summary>
    protected async Task WaitForAppReadyAsync()
    {
        await Page.WaitForLoadStateAsync(LoadState.DOMContentLoaded, new PageWaitForLoadStateOptions
        {
            Timeout = DefaultTimeoutMilliseconds,
        });
        await Page.WaitForFunctionAsync(
            "() => document.readyState === 'interactive' || document.readyState === 'complete'",
            null,
            new PageWaitForFunctionOptions
            {
                Timeout = DefaultTimeoutMilliseconds,
            });
    }

    private static string NormalizeRoute(string url)
    {
        if (string.IsNullOrWhiteSpace(url))
        {
            return "/";
        }

        var trimmed = url.Trim();
        if (Uri.TryCreate(trimmed, UriKind.Absolute, out _))
        {
            return trimmed;
        }

        return $"/{trimmed.TrimStart('/')}";
    }

    private string ResolveEnvironmentValue(string name, string? overrideValue, string fallback)
    {
        return NormalizeOptionalText(overrideValue) ??
               (_host.IsExternal ? NormalizeOptionalText(Environment.GetEnvironmentVariable(name)) : null) ??
               fallback;
    }

    private static string? NormalizeOptionalText(string? value)
    {
        var trimmed = value?.Trim();
        return string.IsNullOrEmpty(trimmed) ? null : trimmed;
    }
}
