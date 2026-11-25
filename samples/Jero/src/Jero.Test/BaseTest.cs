
using Microsoft.Playwright;
using Microsoft.Playwright.Xunit.v3;
using Xunit;

namespace Jero.Test;

/// <summary>
/// 浏览器端到端测试基类
/// </summary>
public abstract class BaseTest : PageTest
{
    private const int DefaultTimeoutMilliseconds = 30_000;
    private const string DefaultBaseUrl = "http://localhost:5095";
    private const string DefaultLoginAccount = "superJero";
    private const string DefaultLoginPassword = "Jero++010101";
    private const string DefaultLoginCaptcha = "0";

    private static readonly HttpClient AvailabilityClient = new()
    {
        Timeout = TimeSpan.FromSeconds(5),
    };

    private readonly string? _loginToken;

    protected BaseTest(string? loginToken = null)
    {
        BaseUrl = ResolveBaseUrl();
        _loginToken = NormalizeOptionalText(loginToken);
    }

    protected string BaseUrl { get; }

    public override BrowserNewContextOptions ContextOptions()
    {
        return new BrowserNewContextOptions
        {
            BaseURL = BaseUrl,
            IgnoreHTTPSErrors = true,
            ViewportSize = new ViewportSize
            {
                Width = 1920,
                Height = 1080,
            },
        };
    }

    /// <summary>
    /// 确认本地 Web 应用可访问。浏览器测试依赖外部宿主进程，未启动时跳过而不是报红。
    /// </summary>
    protected async Task EnsureApplicationAvailableAsync()
    {
        if (await IsApplicationAvailableAsync())
        {
            return;
        }

        Assert.Skip(
            $"Jero Web 应用未在 {BaseUrl} 响应，已跳过浏览器集成测试。请按 README 的开发命令构建并启动 Jero.Web，或通过 JERO_TEST_BASE_URL 指向已启动的测试环境。");
    }

    /// <summary>
    /// 使用 token 或默认账号完成登录。
    /// </summary>
    protected async Task AuthenticateAsync()
    {
        if (!string.IsNullOrWhiteSpace(_loginToken))
        {
            await GoToUrlAsync($"/#/login?token={Uri.EscapeDataString(_loginToken)}");
            return;
        }

        await LoginAsync();
    }

    /// <summary>
    /// 用户账号密码登录。
    /// </summary>
    protected async Task LoginAsync(string? account = null, string? password = null)
    {
        await EnsureApplicationAvailableAsync();
        await GoToUrlAsync("/#/login");

        var accountInput = Page.GetByPlaceholder("请输入账号");
        var passwordInput = Page.GetByPlaceholder("请输入密码");

        await Expect(accountInput).ToBeVisibleAsync(new LocatorAssertionsToBeVisibleOptions
        {
            Timeout = DefaultTimeoutMilliseconds,
        });
        await accountInput.FillAsync(ResolveEnvironmentValue("JERO_TEST_LOGIN_ACCOUNT", account, DefaultLoginAccount));
        await passwordInput.FillAsync(ResolveEnvironmentValue("JERO_TEST_LOGIN_PASSWORD", password, DefaultLoginPassword));

        var captchaInput = Page.GetByPlaceholder("请输入验证码").First;
        if (await captchaInput.CountAsync() > 0 && await captchaInput.IsVisibleAsync())
        {
            await captchaInput.FillAsync(ResolveEnvironmentValue("JERO_TEST_LOGIN_CAPTCHA", null, DefaultLoginCaptcha));
        }

        await Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions
        {
            Name = "登录",
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
        await EnsureApplicationAvailableAsync();
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

    private async Task<bool> IsApplicationAvailableAsync()
    {
        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, BaseUrl);
            using var response = await AvailabilityClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
            return true;
        }
        catch (Exception exception) when (exception is HttpRequestException or TaskCanceledException or OperationCanceledException)
        {
            return false;
        }
    }

    private static string ResolveBaseUrl()
    {
        var candidate = NormalizeOptionalText(Environment.GetEnvironmentVariable("JERO_TEST_BASE_URL")) ?? DefaultBaseUrl;
        if (!Uri.TryCreate(candidate, UriKind.Absolute, out var uri) ||
            (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
        {
            throw new InvalidOperationException(
                $"JERO_TEST_BASE_URL 必须是 http/https 绝对地址，当前值：{candidate}");
        }

        return candidate.TrimEnd('/');
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

        return trimmed.StartsWith("#/", StringComparison.Ordinal)
            ? $"/{trimmed}"
            : $"/{trimmed.TrimStart('/')}";
    }

    private static string ResolveEnvironmentValue(string name, string? overrideValue, string fallback)
    {
        return NormalizeOptionalText(overrideValue) ??
               NormalizeOptionalText(Environment.GetEnvironmentVariable(name)) ??
               fallback;
    }

    private static string? NormalizeOptionalText(string? value)
    {
        var trimmed = value?.Trim();
        return string.IsNullOrEmpty(trimmed) ? null : trimmed;
    }
}
