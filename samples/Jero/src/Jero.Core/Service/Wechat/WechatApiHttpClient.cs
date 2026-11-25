



namespace Jero.Core.Service;

/// <summary>
/// 微信API客户端
/// </summary>
public partial class WechatApiClientFactory : ISingleton
{
    private readonly IHttpClientFactory _httpClientFactory;
    public readonly WechatOptions _wechatOptions;
    private readonly SysCacheService _sysCacheService;

    public WechatApiClientFactory(IHttpClientFactory httpClientFactory, IOptions<WechatOptions> wechatOptions, SysCacheService sysCacheService)
    {
        _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
        _wechatOptions = wechatOptions.Value ?? throw new ArgumentNullException(nameof(wechatOptions));
        _sysCacheService = sysCacheService;
    }

    /// <summary>
    /// 微信公众号
    /// </summary>
    /// <returns></returns>
    public WechatApiClient CreateWechatClient()
    {
        if (string.IsNullOrEmpty(_wechatOptions.WechatAppId) || string.IsNullOrEmpty(_wechatOptions.WechatAppSecret))
            throw Oops.Oh("微信公众号配置错误");

        var client = WechatApiClientBuilder.Create(new WechatApiClientOptions()
        {
            AppId = _wechatOptions.WechatAppId,
            AppSecret = _wechatOptions.WechatAppSecret,
            PushToken = _wechatOptions.WechatToken,
            PushEncodingAESKey = _wechatOptions.WechatEncodingAESKey,
        })
        .UseHttpClient(_httpClientFactory.CreateClient(), disposeClient: false) // 设置 HttpClient 不随客户端一同销毁
        .Build();

        client.Configure(config =>
        {
            var jsonSerializerOptions = SystemTextJsonSerializer.GetDefaultSerializerOptions();
            jsonSerializerOptions.WriteIndented = true;
            config.JsonSerializer = new SystemTextJsonSerializer(jsonSerializerOptions);
        });

        return client;
    }

    /// <summary>
    /// 微信小程序
    /// </summary>
    /// <returns></returns>
    public WechatApiClient CreateWxOpenClient()
    {
        if (string.IsNullOrEmpty(_wechatOptions.WxOpenAppId) || string.IsNullOrEmpty(_wechatOptions.WxOpenAppSecret))
            throw Oops.Oh("微信小程序配置错误");

        var client = WechatApiClientBuilder.Create(new WechatApiClientOptions()
        {
            AppId = _wechatOptions.WxOpenAppId,
            AppSecret = _wechatOptions.WxOpenAppSecret,
            PushToken = _wechatOptions.WxToken,
            PushEncodingAESKey = _wechatOptions.WxEncodingAESKey,
        })
        .UseHttpClient(_httpClientFactory.CreateClient(), disposeClient: false) // 设置 HttpClient 不随客户端一同销毁
        .Build();

        client.Configure(config =>
        {
            var jsonSerializerOptions = SystemTextJsonSerializer.GetDefaultSerializerOptions();
            jsonSerializerOptions.WriteIndented = true;
            config.JsonSerializer = new SystemTextJsonSerializer(jsonSerializerOptions);
        });

        return client;
    }

    /// <summary>
    /// 获取微信公众号AccessToken
    /// </summary>
    /// <returns></returns>
    public async Task<string> TryGetWechatAccessTokenAsync()
    {
        var cacheKey = $"WxAccessToken_{_wechatOptions.WechatAppId}";
        var cachedToken = _sysCacheService.Get<string>(cacheKey);
        if (!string.IsNullOrEmpty(cachedToken)) return cachedToken;

        if (!_sysCacheService.ExistKey(cacheKey) || string.IsNullOrEmpty(cachedToken))
        {
            var client = CreateWechatClient();
            var reqCgibinToken = new CgibinTokenRequest();
            var resCgibinToken = await client.ExecuteCgibinTokenAsync(reqCgibinToken);
            if (resCgibinToken.ErrorCode != (int)WechatReturnCodeEnum.请求成功)
                throw Oops.Oh(resCgibinToken.ErrorMessage + " " + resCgibinToken.ErrorCode);
            if (string.IsNullOrWhiteSpace(resCgibinToken.AccessToken)) throw Oops.Oh("微信公众号 AccessToken 为空");
            if (resCgibinToken.ExpiresIn <= 0) throw Oops.Oh("微信公众号 AccessToken 过期时间无效");

            var expireSeconds = Math.Max(1, resCgibinToken.ExpiresIn - 60);
            _sysCacheService.Set(cacheKey, resCgibinToken.AccessToken, TimeSpan.FromSeconds(expireSeconds));
            return resCgibinToken.AccessToken;
        }

        throw Oops.Oh("微信公众号 AccessToken 缓存无效");
    }

    /// <summary>
    /// 获取微信小程序AccessToken
    /// </summary>
    /// <returns></returns>
    public async Task<string> TryGetWxOpenAccessTokenAsync()
    {
        var cacheKey = $"WxAccessToken_{_wechatOptions.WxOpenAppId}";
        var cachedToken = _sysCacheService.Get<string>(cacheKey);
        if (!string.IsNullOrEmpty(cachedToken)) return cachedToken;

        if (!_sysCacheService.ExistKey(cacheKey) || string.IsNullOrEmpty(cachedToken))
        {
            var client = CreateWxOpenClient();
            var reqCgibinToken = new CgibinTokenRequest();
            var resCgibinToken = await client.ExecuteCgibinTokenAsync(reqCgibinToken);
            if (resCgibinToken.ErrorCode != (int)WechatReturnCodeEnum.请求成功)
                throw Oops.Oh(resCgibinToken.ErrorMessage + " " + resCgibinToken.ErrorCode);
            if (string.IsNullOrWhiteSpace(resCgibinToken.AccessToken)) throw Oops.Oh("微信小程序 AccessToken 为空");
            if (resCgibinToken.ExpiresIn <= 0) throw Oops.Oh("微信小程序 AccessToken 过期时间无效");

            var expireSeconds = Math.Max(1, resCgibinToken.ExpiresIn - 60);
            _sysCacheService.Set(cacheKey, resCgibinToken.AccessToken, TimeSpan.FromSeconds(expireSeconds));
            return resCgibinToken.AccessToken;
        }

        throw Oops.Oh("微信小程序 AccessToken 缓存无效");
    }

    /// <summary>
    /// 检查微信公众号AccessToken
    /// </summary>
    /// <returns></returns>
    public async Task CheckWechatAccessTokenAsync()
    {
        if (string.IsNullOrEmpty(_wechatOptions.WechatAppId) || string.IsNullOrEmpty(_wechatOptions.WechatAppSecret)) return;

        var req = new CgibinOpenApiQuotaGetRequest
        {
            AccessToken = await TryGetWechatAccessTokenAsync(),
            CgiPath = "/cgi-bin/token"
        };
        var client = CreateWechatClient();
        var res = await client.ExecuteCgibinOpenApiQuotaGetAsync(req);

        var originColor = Console.ForegroundColor;
        if (res.ErrorCode != (int)WechatReturnCodeEnum.请求成功)
        {
            _sysCacheService.Remove($"WxAccessToken_{_wechatOptions.WechatAppId}");
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("【" + DateTime.Now + "】" + _wechatOptions.WxOpenAppId + " 微信公众号令牌 无效");
        }
        else
        {
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine("【" + DateTime.Now + "】" + _wechatOptions.WxOpenAppId + " 微信公众号令牌 有效");
        }
        Console.ForegroundColor = originColor;
    }

    /// <summary>
    /// 检查微信小程序AccessToken
    /// </summary>
    /// <returns></returns>
    public async Task CheckWxOpenAccessTokenAsync()
    {
        if (string.IsNullOrEmpty(_wechatOptions.WxOpenAppId) || string.IsNullOrEmpty(_wechatOptions.WxOpenAppSecret)) return;

        var req = new CgibinOpenApiQuotaGetRequest
        {
            AccessToken = await TryGetWxOpenAccessTokenAsync(),
            CgiPath = "/cgi-bin/token"
        };
        var client = CreateWxOpenClient();
        var res = await client.ExecuteCgibinOpenApiQuotaGetAsync(req);

        var originColor = Console.ForegroundColor;
        if (res.ErrorCode != (int)WechatReturnCodeEnum.请求成功)
        {
            _sysCacheService.Remove($"WxAccessToken_{_wechatOptions.WxOpenAppId}");
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("【" + DateTime.Now + "】" + _wechatOptions.WxOpenAppId + " 微信小程序令牌 无效");
        }
        else
        {
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine("【" + DateTime.Now + "】" + _wechatOptions.WxOpenAppId + " 微信小程序令牌 有效");
        }
        Console.ForegroundColor = originColor;
    }

    /// <summary>
    /// 获取微信JS接口临时票据jsapi_ticket
    /// </summary>
    /// <returns></returns>
    public async Task<string> TryGetWechatJsApiTicketAsync()
    {
        var cacheKey = $"WxJsApiTicket_{_wechatOptions.WechatAppId}";
        var cachedTicket = _sysCacheService.Get<string>(cacheKey);
        if (!string.IsNullOrEmpty(cachedTicket)) return cachedTicket;

        if (!_sysCacheService.ExistKey(cacheKey) || string.IsNullOrEmpty(cachedTicket))
        {
            var accessToken = await TryGetWechatAccessTokenAsync();
            var client = CreateWechatClient();
            var request = new CgibinTicketGetTicketRequest()
            {
                AccessToken = accessToken
            };
            var response = await client.ExecuteCgibinTicketGetTicketAsync(request);
            if (!response.IsSuccessful())
                throw Oops.Oh(response.ErrorMessage + " " + response.ErrorCode);
            if (string.IsNullOrWhiteSpace(response.Ticket)) throw Oops.Oh("微信公众号 JSApiTicket 为空");
            if (response.ExpiresIn <= 0) throw Oops.Oh("微信公众号 JSApiTicket 过期时间无效");

            var expireSeconds = Math.Max(1, response.ExpiresIn - 60);
            _sysCacheService.Set(cacheKey, response.Ticket, TimeSpan.FromSeconds(expireSeconds));
            return response.Ticket;
        }

        throw Oops.Oh("微信公众号 JSApiTicket 缓存无效");
    }
}
