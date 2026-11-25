namespace Jero.Core.Service;

/// <summary>
/// 微信公众号服务 🧩
/// </summary>
[ApiDescriptionSettings(Order = 230)]
public class SysWechatService : IDynamicApiController, ITransient
{
    private readonly SqlSugarRepository<SysWechatUser> _sysWechatUserRep;
    private readonly SysConfigService _sysConfigService;
    private readonly WechatApiClientFactory _wechatApiClientFactory;
    private readonly WechatApiClient _wechatApiClient;

    public SysWechatService(SqlSugarRepository<SysWechatUser> sysWechatUserRep,
        SysConfigService sysConfigService,
        WechatApiClientFactory wechatApiClientFactory,
        SysCacheService sysCacheService)
    {
        _sysWechatUserRep = sysWechatUserRep;
        _sysConfigService = sysConfigService;
        _wechatApiClientFactory = wechatApiClientFactory;
        _wechatApiClient = wechatApiClientFactory.CreateWechatClient();
    }

    /// <summary>
    /// 生成网页授权Url 🔖
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [AllowAnonymous]
    [DisplayName("生成网页授权Url")]
    public string GenAuthUrl(GenAuthUrlInput input)
    {
        return _wechatApiClient.GenerateParameterizedUrlForConnectOAuth2Authorize(input.RedirectUrl, input.Scope, input.State);
    }

    /// <summary>
    /// 获取微信用户OpenId 🔖
    /// </summary>
    /// <param name="input"></param>
    [AllowAnonymous]
    [DisplayName("获取微信用户OpenId")]
    public async Task<string> SnsOAuth2([FromQuery] WechatOAuth2Input input)
    {
        var reqOAuth2 = new SnsOAuth2AccessTokenRequest()
        {
            Code = input.Code,
        };
        var resOAuth2 = await _wechatApiClient.ExecuteSnsOAuth2AccessTokenAsync(reqOAuth2);
        if (resOAuth2.ErrorCode != (int)WechatReturnCodeEnum.请求成功)
            throw Oops.Oh(resOAuth2.ErrorMessage + " " + resOAuth2.ErrorCode);

        var openId = resOAuth2.OpenId ?? throw Oops.Oh("微信 OAuth 响应缺少 OpenId");
        var wxUser = await _sysWechatUserRep.GetFirstAsync(p => p.OpenId == openId);
        if (wxUser == null)
        {
            var reqUserInfo = new SnsUserInfoRequest()
            {
                OpenId = openId,
                AccessToken = resOAuth2.AccessToken,
            };
            var resUserInfo = await _wechatApiClient.ExecuteSnsUserInfoAsync(reqUserInfo);
            wxUser = resUserInfo.Adapt<SysWechatUser>();
            wxUser.Avatar = resUserInfo.HeadImageUrl;
            wxUser.NickName = resUserInfo.Nickname;
            wxUser.OpenId = openId;
            wxUser.UnionId = resOAuth2.UnionId;
            wxUser.AccessToken = resOAuth2.AccessToken;
            wxUser.RefreshToken = resOAuth2.RefreshToken;
            wxUser = await _sysWechatUserRep.AsInsertable(wxUser).ExecuteReturnEntityAsync();
        }
        else
        {
            wxUser.AccessToken = resOAuth2.AccessToken;
            wxUser.RefreshToken = resOAuth2.RefreshToken;
            await _sysWechatUserRep.AsUpdateable(wxUser).IgnoreColumns(true).ExecuteCommandAsync();
        }

        return openId;
    }

    /// <summary>
    /// 微信用户登录OpenId 🔖
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [AllowAnonymous]
    [DisplayName("微信用户登录OpenId")]
    public async Task<dynamic> OpenIdLogin(WechatUserLogin input)
    {
        var wxUser = await _sysWechatUserRep.GetFirstAsync(p => p.OpenId == input.OpenId);
        if (wxUser == null)
            throw Oops.Oh("微信用户登录OpenId错误");

        var tokenExpire = await _sysConfigService.GetTokenExpire();
        return new
        {
            wxUser.Avatar,
            accessToken = JWTEncryption.Encrypt(new Dictionary<string, object>
            {
                { ClaimConst.UserId, wxUser.Id },
                { ClaimConst.NickName, wxUser.NickName ?? string.Empty },
                { ClaimConst.LoginMode, LoginModeEnum.APP },
            }, tokenExpire)
        };
    }

    /// <summary>
    /// 获取配置签名参数(wx.config) 🔖
    /// </summary>
    /// <returns></returns>
    [DisplayName("获取配置签名参数(wx.config)")]
    public async Task<dynamic> GenConfigPara(SignatureInput input)
    {
        string ticket = await _wechatApiClientFactory.TryGetWechatJsApiTicketAsync();
        return _wechatApiClient.GenerateParametersForJSSDKConfig(ticket, input.Url);
    }

    /// <summary>
    /// 获取模板列表 🔖
    /// </summary>
    [DisplayName("获取模板列表")]
    public async Task<dynamic?> GetMessageTemplateList()
    {
        var accessToken = await GetCgibinToken();
        var reqTemplate = new CgibinTemplateGetAllPrivateTemplateRequest()
        {
            AccessToken = accessToken
        };
        var resTemplate = await _wechatApiClient.ExecuteCgibinTemplateGetAllPrivateTemplateAsync(reqTemplate);
        if (resTemplate.ErrorCode != (int)WechatReturnCodeEnum.请求成功)
            throw Oops.Oh(resTemplate.ErrorMessage + " " + resTemplate.ErrorCode);

        return resTemplate.TemplateList;
    }

    /// <summary>
    /// 发送模板消息 🔖
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [DisplayName("发送模板消息")]
    public async Task<dynamic> SendTemplateMessage(MessageTemplateSendInput input)
    {
        var dataInfo = input.Data.ToDictionary(k => k.Key, k => k.Value);
        var messageData = new Dictionary<string, CgibinMessageTemplateSendRequest.Types.DataItem>();
        foreach (var item in dataInfo)
        {
            var value = item.Value?.Value ?? throw Oops.Oh($"微信模板字段 {item.Key} 缺少值");
            messageData.Add(item.Key, new CgibinMessageTemplateSendRequest.Types.DataItem { Value = value.ToString() ?? string.Empty });
        }

        var accessToken = await GetCgibinToken();
        var reqMessage = new CgibinMessageTemplateSendRequest()
        {
            AccessToken = accessToken,
            TemplateId = input.TemplateId,
            ToUserOpenId = input.ToUserOpenId,
            Url = input.Url,
            MiniProgram = new CgibinMessageTemplateSendRequest.Types.MiniProgram
            {
                AppId = _wechatApiClientFactory._wechatOptions.WxOpenAppId,
                PagePath = input.MiniProgramPagePath,
            },
            Data = messageData
        };
        var resMessage = await _wechatApiClient.ExecuteCgibinMessageTemplateSendAsync(reqMessage);
        return resMessage;
    }

    /// <summary>
    /// 删除模板 🔖
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [ApiDescriptionSettings(Name = "DeleteMessageTemplate"), HttpPost]
    [DisplayName("删除模板")]
    public async Task<dynamic> DeleteMessageTemplate(DeleteMessageTemplateInput input)
    {
        var accessToken = await GetCgibinToken();
        var reqMessage = new CgibinTemplateDeletePrivateTemplateRequest()
        {
            AccessToken = accessToken,
            TemplateId = input.TemplateId
        };
        var resTemplate = await _wechatApiClient.ExecuteCgibinTemplateDeletePrivateTemplateAsync(reqMessage);
        return resTemplate;
    }

    /// <summary>
    /// 获取Access_token
    /// </summary>
    [NonAction]
    public async Task<string> GetCgibinToken()
    {
        return await _wechatApiClientFactory.TryGetWechatAccessTokenAsync();
    }
}
