namespace Jero.Core.Service;

/// <summary>
/// 微信小程序服务 🧩
/// </summary>
[ApiDescriptionSettings(Order = 240)]
public class SysWxOpenService : IDynamicApiController, ITransient
{
    private readonly SqlSugarRepository<SysWechatUser> _sysWechatUserRep;
    private readonly SysConfigService _sysConfigService;
    private readonly WechatApiClient _wechatApiClient;
    private readonly SysFileService _sysFileService;
    private readonly WechatApiClientFactory _wechatApiClientFactory;

    public SysWxOpenService(SqlSugarRepository<SysWechatUser> sysWechatUserRep,
        SysConfigService sysConfigService,
        WechatApiClientFactory wechatApiClientFactory,
        SysFileService sysFileService)
    {
        _sysWechatUserRep = sysWechatUserRep;
        _sysConfigService = sysConfigService;
        _wechatApiClient = wechatApiClientFactory.CreateWxOpenClient();
        _sysFileService = sysFileService;
        _wechatApiClientFactory = wechatApiClientFactory;
    }

    /// <summary>
    /// 获取微信用户OpenId 🔖
    /// </summary>
    /// <param name="input"></param>
    [AllowAnonymous]
    [DisplayName("获取微信用户OpenId")]
    public async Task<WxOpenIdOutput> GetWxOpenId([FromQuery] JsCode2SessionInput input)
    {
        var reqJsCode2Session = new SnsJsCode2SessionRequest()
        {
            JsCode = input.JsCode,
        };
        var resCode2Session = await _wechatApiClient.ExecuteSnsJsCode2SessionAsync(reqJsCode2Session);
        if (resCode2Session.ErrorCode != (int)WechatReturnCodeEnum.请求成功)
            throw Oops.Oh(resCode2Session.ErrorMessage + " " + resCode2Session.ErrorCode);

        var openId = resCode2Session.OpenId ?? throw Oops.Oh("微信小程序会话响应缺少 OpenId");
        var wxUser = await _sysWechatUserRep.GetFirstAsync(p => p.OpenId == openId);
        if (wxUser == null)
        {
            wxUser = new SysWechatUser
            {
                OpenId = openId,
                UnionId = resCode2Session.UnionId,
                SessionKey = resCode2Session.SessionKey,
                PlatformType = PlatformTypeEnum.微信小程序
            };
            wxUser = await _sysWechatUserRep.AsInsertable(wxUser).ExecuteReturnEntityAsync();
        }
        else
        {
            await _sysWechatUserRep.AsUpdateable(wxUser).IgnoreColumns(true).ExecuteCommandAsync();
        }

        return new WxOpenIdOutput
        {
            OpenId = openId
        };
    }

    /// <summary>
    /// 获取微信用户电话号码 🔖
    /// </summary>
    /// <param name="input"></param>
    [AllowAnonymous]
    [DisplayName("获取微信用户电话号码")]
    public async Task<WxPhoneOutput> GetWxPhone([FromQuery] WxPhoneInput input)
    {
        var accessToken = await GetCgibinToken();
        var reqUserPhoneNumber = new WxaBusinessGetUserPhoneNumberRequest()
        {
            Code = input.Code,
            AccessToken = accessToken,
        };
        var resUserPhoneNumber = await _wechatApiClient.ExecuteWxaBusinessGetUserPhoneNumberAsync(reqUserPhoneNumber);
        if (resUserPhoneNumber.ErrorCode != (int)WechatReturnCodeEnum.请求成功)
            throw Oops.Oh(resUserPhoneNumber.ErrorMessage + " " + resUserPhoneNumber.ErrorCode);

        var wxUser = await _sysWechatUserRep.GetFirstAsync(p => p.OpenId == input.OpenId);
        if (wxUser == null)
        {
            wxUser = new SysWechatUser
            {
                OpenId = input.OpenId,
                Mobile = resUserPhoneNumber.PhoneInfo?.PhoneNumber,
                PlatformType = PlatformTypeEnum.微信小程序
            };
            wxUser = await _sysWechatUserRep.AsInsertable(wxUser).ExecuteReturnEntityAsync();
        }
        else
        {
            wxUser.Mobile = resUserPhoneNumber.PhoneInfo?.PhoneNumber;
            await _sysWechatUserRep.AsUpdateable(wxUser).IgnoreColumns(true).ExecuteCommandAsync();
        }

        return new WxPhoneOutput
        {
            PhoneNumber = resUserPhoneNumber.PhoneInfo?.PhoneNumber ?? string.Empty
        };
    }

    /// <summary>
    /// 微信小程序登录OpenId 🔖
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [AllowAnonymous]
    [DisplayName("微信小程序登录OpenId")]
    public async Task<dynamic> WxOpenIdLogin(WxOpenIdLoginInput input)
    {
        var wxUser = await _sysWechatUserRep.GetFirstAsync(u => u.OpenId == input.OpenId);
        if (wxUser == null)
            throw Oops.Oh("微信小程序登录失败");

        var tokenExpire = await _sysConfigService.GetTokenExpire();
        return new
        {
            wxUser.Avatar,
            accessToken = JWTEncryption.Encrypt(new Dictionary<string, object>
            {
                { ClaimConst.UserId, wxUser.Id },
                { ClaimConst.RealName, wxUser.NickName ?? string.Empty },
                { ClaimConst.LoginMode, LoginModeEnum.APP },
            }, tokenExpire)
        };
    }

    /// <summary>
    /// 上传小程序头像
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [AllowAnonymous]
    [DisplayName("上传小程序头像")]
    public async Task<SysFile> UploadAvatar([FromForm] UploadAvatarInput input)
    {
        var file = input.File;
        if (file == null || file.Length <= 0)
            throw Oops.Oh(ErrorCodeEnum.D8000);

        var wxUser = await _sysWechatUserRep.GetFirstAsync(u => u.OpenId == input.OpenId);
        if (wxUser == null)
            throw Oops.Oh("未找到用户上传失败");

        var res = await _sysFileService.UploadFile(new UploadFileInput { File = file, FileType = input.FileType }, "upload/wechatAvatar");
        wxUser.Avatar = res.Url;
        await _sysWechatUserRep.AsUpdateable(wxUser).IgnoreColumns(true).ExecuteCommandAsync();

        return res;
    }

    /// <summary>
    /// 设置小程序用户昵称
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [AllowAnonymous]
    [HttpPost]
    public async Task SetNickName(SetNickNameInput input)
    {
        var wxUser = await _sysWechatUserRep.GetFirstAsync(u => u.OpenId == input.OpenId);
        if (wxUser == null)
            throw Oops.Oh("未找到用户信息设置失败");
        wxUser.NickName = input.NickName;
        await _sysWechatUserRep.AsUpdateable(wxUser).IgnoreColumns(true).ExecuteCommandAsync();
        return;
    }

    /// <summary>
    /// 获取小程序用户信息
    /// </summary>
    /// <param name="openid"></param>
    /// <returns></returns>
    [AllowAnonymous]
    public async Task<dynamic> GetUserInfo(string openid)
    {
        var wxUser = await _sysWechatUserRep.GetFirstAsync(u => u.OpenId == openid);
        if (wxUser == null)
            throw Oops.Oh("未找到用户信息获取失败");
        return new { nickName = wxUser.NickName, avator = wxUser.Avatar };
    }

    /// <summary>
    /// 获取订阅消息模板列表 🔖
    /// </summary>
    [DisplayName("获取订阅消息模板列表")]
    public async Task<dynamic?> GetMessageTemplateList()
    {
        var accessToken = await GetCgibinToken();
        var reqTemplate = new WxaApiNewTemplateGetTemplateRequest()
        {
            AccessToken = accessToken
        };
        var resTemplate = await _wechatApiClient.ExecuteWxaApiNewTemplateGetTemplateAsync(reqTemplate);
        if (resTemplate.ErrorCode != (int)WechatReturnCodeEnum.请求成功)
            throw Oops.Oh(resTemplate.ErrorMessage + " " + resTemplate.ErrorCode);

        return resTemplate.TemplateList;
    }

    /// <summary>
    /// 发送订阅消息 🔖
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [DisplayName("发送订阅消息")]
    public async Task<dynamic> SendSubscribeMessage(SendSubscribeMessageInput input)
    {
        var accessToken = await GetCgibinToken();
        var reqMessage = new CgibinMessageSubscribeSendRequest()
        {
            AccessToken = accessToken,
            TemplateId = input.TemplateId,
            ToUserOpenId = input.ToUserOpenId,
            Data = input.Data,
            MiniProgramState = input.MiniprogramState,
            Language = input.Language,
            MiniProgramPagePath = input.MiniProgramPagePath
        };
        var resMessage = await _wechatApiClient.ExecuteCgibinMessageSubscribeSendAsync(reqMessage);
        return resMessage;
    }

    /// <summary>
    /// 增加订阅消息模板 🔖
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [ApiDescriptionSettings(Name = "AddSubscribeMessageTemplate"), HttpPost]
    [DisplayName("增加订阅消息模板")]
    public async Task<dynamic> AddSubscribeMessageTemplate(AddSubscribeMessageTemplateInput input)
    {
        var accessToken = await GetCgibinToken();
        var reqMessage = new WxaApiNewTemplateAddTemplateRequest()
        {
            AccessToken = accessToken,
            TemplateTitleId = input.TemplateTitleId,
            KeyworkIdList = input.KeyworkIdList,
            SceneDescription = input.SceneDescription
        };
        var resTemplate = await _wechatApiClient.ExecuteWxaApiNewTemplateAddTemplateAsync(reqMessage);
        return resTemplate;
    }

    /// <summary>
    /// 生成带参数小程序二维码(总共生成的码数量限制为 100,000)
    /// </summary>
    /// <param name="input"> 扫码进入的小程序页面路径，最大长度 128 个字符，不能为空； eg: pages / index ? id = AY000001 </param>
    /// <returns></returns>
    [DisplayName("生成小程序二维码")]
    [ApiDescriptionSettings(Name = "GenerateQRImage")]
    public async Task<GenerateQRImageOutput> GenerateQRImageAsync(GenerateQRImageInput input)
    {
        GenerateQRImageOutput generateQRImageOutInput = new GenerateQRImageOutput();
        if (input.PagePath.IsNullOrEmpty())
        {
            generateQRImageOutInput.Message = $"生成失败 页面路径不能为空";
            return generateQRImageOutInput;
        }

        if (input.ImageName.IsNullOrEmpty())
        {
            input.ImageName = DateTime.Now.ToString("yyyyMMddHHmmss");
        }

        var accessToken = await GetCgibinToken();
        var request = new CgibinWxaappCreateWxaQrcodeRequest
        {
            AccessToken = accessToken,
            Path = input.PagePath,
            Width = input.Width
        };
        var response = await _wechatApiClient.ExecuteCgibinWxaappCreateWxaQrcodeAsync(request);

        if (response.IsSuccessful())
        {
            var QRImagePath = App.GetConfig<string>("Wechat:QRImagePath");
            var relativeImgPath = string.Empty;

            // 判断路径是绝对路径还是相对路径
            var isPathRooted = Path.IsPathRooted(QRImagePath);
            if (!isPathRooted)
            {
                // 相对路径
                relativeImgPath = string.IsNullOrEmpty(QRImagePath) ? Path.Combine("upload", "QRImage") : QRImagePath;
                QRImagePath = Path.Combine(App.WebHostEnvironment.WebRootPath, relativeImgPath);
            }

            //判断文件存放路径是否存在
            if (!Directory.Exists(QRImagePath))
            {
                Directory.CreateDirectory(QRImagePath);
            }
            // 将二维码图片数据保存为文件
            var fileName = $"{input.ImageName.ToUpper()}.png";
            var filePath = Path.Combine(QRImagePath, fileName);
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
            File.WriteAllBytes(filePath, response.GetRawBytes());

            generateQRImageOutInput.Success = true;
            generateQRImageOutInput.ImgPath = filePath;
            generateQRImageOutInput.RelativeImgPath = Path.Combine(relativeImgPath, fileName);
            generateQRImageOutInput.Message = "生成成功";
        }
        else
        {
            // 处理错误情况
            generateQRImageOutInput.Message = $"生成失败 错误代码：{response.ErrorCode}  错误描述：{response.ErrorMessage}";
        }
        return generateQRImageOutInput;
    }

    /// <summary>
    /// 生成二维码(获取不受限制的小程序码)
    /// </summary>
    /// <param name="input">入参</param>
    /// <returns></returns>
    [DisplayName("生成小程序二维码")]
    [ApiDescriptionSettings(Name = "GenerateQRImageUnlimit")]
    public async Task<GenerateQRImageOutput> GenerateQRImageUnlimitAsync(GenerateQRImageUnLimitInput input)
    {
        GenerateQRImageOutput generateQRImageOutInput = new GenerateQRImageOutput();
        if (input.PagePath.IsNullOrEmpty())
        {
            generateQRImageOutInput.Message = $"生成失败，页面路径不能为空";
            return generateQRImageOutInput;
        }

        if (input.Scene.Length > 32)
        {
            generateQRImageOutInput.Message = $"生成失败，携带的参数长度超过限制";
            return generateQRImageOutInput;
        }

        if (input.ImageName.IsNullOrEmpty())
        {
            input.ImageName = DateTime.Now.ToString("yyyyMMddHHmmss");
        }

        var accessToken = await GetCgibinToken();
        var request = new WxaGetWxaCodeRequest
        {
            AccessToken = accessToken,
            Width = input.Width,
            PagePath = input.PagePath,
        };
        var response = await _wechatApiClient.ExecuteWxaGetWxaCodeAsync(request);

        if (response.IsSuccessful())
        {
            var QRImagePath = App.GetConfig<string>("Wechat:QRImagePath");
            var relativeImgPath = string.Empty;

            // 判断路径是绝对路径还是相对路径
            var isPathRooted = Path.IsPathRooted(QRImagePath);
            if (!isPathRooted)
            {
                // 相对路径
                relativeImgPath = string.IsNullOrEmpty(QRImagePath) ? Path.Combine("upload", "QRImageUnLimit") : QRImagePath;
                QRImagePath = Path.Combine(App.WebHostEnvironment.WebRootPath, relativeImgPath);
            }

            //判断文件存放路径是否存在
            if (!Directory.Exists(QRImagePath))
            {
                Directory.CreateDirectory(QRImagePath);
            }
            // 将二维码图片数据保存为文件
            var fileName = $"{input.ImageName.ToUpper()}.png";
            var filePath = Path.Combine(QRImagePath, fileName);
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
            File.WriteAllBytes(filePath, response.GetRawBytes());

            generateQRImageOutInput.Success = true;
            generateQRImageOutInput.ImgPath = filePath;
            generateQRImageOutInput.RelativeImgPath = Path.Combine(relativeImgPath, fileName);
            generateQRImageOutInput.Message = "生成成功";
        }
        else
        {
            // 处理错误情况
            generateQRImageOutInput.Message = $"生成失败 错误代码：{response.ErrorCode}  错误描述：{response.ErrorMessage}";
        }
        return generateQRImageOutInput;
    }

    /// <summary>
    /// 获取Access_token
    /// </summary>
    private async Task<string> GetCgibinToken()
    {
        return await _wechatApiClientFactory.TryGetWxOpenAccessTokenAsync();
    }
}
