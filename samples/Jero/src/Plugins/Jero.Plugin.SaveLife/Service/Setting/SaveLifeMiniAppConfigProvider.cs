


using SKIT.FlurlHttpClient;
using SKIT.FlurlHttpClient.Wechat.Api;
using SKIT.FlurlHttpClient.Wechat.Api.Models;
namespace Jero.Plugin.SaveLife.Service;

public interface ISaveLifeMiniAppConfigProvider
{
    Task<SaveLifeMiniAppConfigOutput> GetConfigAsync();

    Task<SaveLifeMiniAppConfigSecret> GetSecretAsync();

    Task<byte[]> GetQrCodeAsync();

    Task SaveConfigAsync(SaveLifeMiniAppConfigInput input);

    Task<bool> GetRecruitmentEnabledAsync();

    Task SaveRecruitmentEnabledAsync(bool enabled);

    Task<WxOpenIdOutput> GetOpenIdAsync(string jsCode);

    Task<WxPhoneOutput> GetPhoneAsync(string openId, string phoneCode);
}

public sealed class SaveLifeMiniAppConfigProvider : ISaveLifeMiniAppConfigProvider, ITransient
{
    private const string CacheKey = "save_life_miniapp_config";
    private const string QrCodeCacheKey = "save_life_miniapp_qr_code";
    private const string QrCodeFileName = "save-life-miniapp.png";
    private const string QrCodePagePath = "pages/index/index";
    private const string GroupCode = "SaveLifeMiniApp";
    private const string AppIdCode = "save_life_miniapp_app_id";
    private const string AppSecretCode = "save_life_miniapp_app_secret";
    private const string TokenCode = "save_life_miniapp_token";
    private const string EncodingAesKeyCode = "save_life_miniapp_encoding_aes_key";
    private const string QrImagePathCode = "save_life_miniapp_qr_image_path";
    private const string RecruitmentEnabledCode = "save_life_recruitment_enabled";
    private const string SecretPrefix = "sm4:";
    private const int MaxSecretBytes = 239;
    private const int InvalidJsCodeErrorCode = 40029;
    private const int UsedJsCodeErrorCode = 40163;

    private readonly SqlSugarRepository<SysConfig> _configRep;
    private readonly SysCacheService _cacheService;
    private readonly IHttpClientFactory _httpClientFactory;

    public SaveLifeMiniAppConfigProvider(
        SqlSugarRepository<SysConfig> configRep,
        SysCacheService cacheService,
        IHttpClientFactory httpClientFactory)
    {
        _configRep = configRep;
        _cacheService = cacheService;
        _httpClientFactory = httpClientFactory;
    }

    public async Task<SaveLifeMiniAppConfigOutput> GetConfigAsync()
    {
        var secret = await GetSecretAsync();
        return new SaveLifeMiniAppConfigOutput
        {
            AppId = secret.AppId,
            HasAppSecret = !string.IsNullOrWhiteSpace(secret.AppSecret),
            AppSecretMasked = MaskSecret(secret.AppSecret),
            Token = secret.Token,
            EncodingAesKey = secret.EncodingAesKey,
            QrImagePath = secret.QrImagePath,
            IsConfigured = !string.IsNullOrWhiteSpace(secret.AppId) && !string.IsNullOrWhiteSpace(secret.AppSecret),
            RecruitmentEnabled = secret.RecruitmentEnabled
        };
    }

    public async Task<bool> GetRecruitmentEnabledAsync()
    {
        var secret = await GetSecretAsync();
        return secret.RecruitmentEnabled;
    }

    public async Task<SaveLifeMiniAppConfigSecret> GetSecretAsync()
    {
        var cached = _cacheService.Get<SaveLifeMiniAppConfigSecret>(CacheKey);
        if (cached != null)
            return cached;

        var secret = await LoadSecretAsync();
        _cacheService.Set(CacheKey, secret, TimeSpan.FromMinutes(5));
        return secret;
    }

    public async Task SaveConfigAsync(SaveLifeMiniAppConfigInput input)
    {
        ValidateInput(input);

        var current = await LoadSecretAsync();
        var values = new Dictionary<string, string?>
        {
            [AppIdCode] = Normalize(input.AppId),
            [TokenCode] = Normalize(input.Token),
            [EncodingAesKeyCode] = Normalize(input.EncodingAesKey),
            [QrImagePathCode] = Normalize(input.QrImagePath)
        };

        var appSecret = Normalize(input.AppSecret);
        if (!string.IsNullOrWhiteSpace(appSecret))
            values[AppSecretCode] = ProtectSecret(appSecret);
        else if (!string.IsNullOrWhiteSpace(current.AppSecret))
            values[AppSecretCode] = ProtectSecret(current.AppSecret);

        foreach (var item in values)
        {
            await UpsertConfigAsync(item.Key, item.Value ?? string.Empty);
        }

        TryDeleteQrCodeFile(current.QrImagePath);
        _cacheService.Remove(CacheKey);
        _cacheService.Remove(QrCodeCacheKey);
    }

    public async Task SaveRecruitmentEnabledAsync(bool enabled)
    {
        await UpsertConfigAsync(RecruitmentEnabledCode, enabled ? "true" : "false");
        _cacheService.Remove(CacheKey);
    }

    public async Task<byte[]> GetQrCodeAsync()
    {
        var cached = _cacheService.Get<byte[]>(QrCodeCacheKey);
        if (cached is { Length: > 0 })
            return cached;

        var secret = await GetSecretAsync();
        var filePath = ResolveQrCodeFilePath(secret.QrImagePath);
        if (!string.IsNullOrWhiteSpace(filePath) && File.Exists(filePath))
        {
            var fileBytes = await File.ReadAllBytesAsync(filePath);
            if (fileBytes.Length > 0)
            {
                _cacheService.Set(QrCodeCacheKey, fileBytes, TimeSpan.FromHours(1));
                return fileBytes;
            }
        }

        var client = await CreateClientAsync();
        var response = await client.ExecuteWxaGetWxaCodeAsync(new WxaGetWxaCodeRequest
        {
            PagePath = QrCodePagePath,
            Width = 430,
            EnvironmentVersion = "release"
        });

        if (!response.IsSuccessful())
            throw Oops.Oh($"微信小程序码生成失败：{response.ErrorMessage} {response.ErrorCode}").StatusCode(502);

        var bytes = response.GetRawBytes();
        if (bytes is not { Length: > 0 })
            throw Oops.Oh("微信小程序码生成失败：微信未返回图片数据").StatusCode(502);

        if (!string.IsNullOrWhiteSpace(filePath))
        {
            try
            {
                var directory = Path.GetDirectoryName(filePath);
                if (!string.IsNullOrWhiteSpace(directory))
                    Directory.CreateDirectory(directory);
                await File.WriteAllBytesAsync(filePath, bytes);
            }
            catch (IOException)
            {
                // 文件缓存失败不影响当前请求返回微信生成的图片。
            }
            catch (UnauthorizedAccessException)
            {
                // 文件缓存失败不影响当前请求返回微信生成的图片。
            }
        }

        _cacheService.Set(QrCodeCacheKey, bytes, TimeSpan.FromHours(1));
        return bytes;
    }

    public async Task<WxOpenIdOutput> GetOpenIdAsync(string jsCode)
    {
        var client = await CreateClientAsync();
        var secret = await GetSecretAsync();
        var response = await client.ExecuteSnsJsCode2SessionAsync(new SnsJsCode2SessionRequest
        {
            JsCode = jsCode
        });

        if (response.ErrorCode != (int)WechatReturnCodeEnum.请求成功)
            throw Oops.Oh(BuildOpenIdErrorMessage(response.ErrorCode, response.ErrorMessage, secret.AppId)).StatusCode(400);

        var openId = response.OpenId?.Trim();
        if (string.IsNullOrWhiteSpace(openId))
            throw Oops.Oh("微信登录未返回 openid").StatusCode(400);

        return new WxOpenIdOutput
        {
            OpenId = openId
        };
    }

    public async Task<WxPhoneOutput> GetPhoneAsync(string openId, string phoneCode)
    {
        var client = await CreateClientAsync();
        var accessToken = await GetAccessTokenAsync(client);
        var response = await client.ExecuteWxaBusinessGetUserPhoneNumberAsync(new WxaBusinessGetUserPhoneNumberRequest
        {
            AccessToken = accessToken,
            Code = phoneCode
        });

        if (response.ErrorCode != (int)WechatReturnCodeEnum.请求成功)
            throw Oops.Oh(response.ErrorMessage + " " + response.ErrorCode);

        var phoneNumber = response.PhoneInfo?.PhoneNumber?.Trim();
        if (string.IsNullOrWhiteSpace(phoneNumber))
            throw Oops.Oh("微信手机号授权未返回手机号").StatusCode(400);

        return new WxPhoneOutput
        {
            PhoneNumber = phoneNumber
        };
    }

    private async Task<SaveLifeMiniAppConfigSecret> LoadSecretAsync()
    {
        var configs = await _configRep.AsQueryable()
            .Where(u => u.GroupCode == GroupCode)
            .ToListAsync();
        var values = configs
            .Where(u => !string.IsNullOrWhiteSpace(u.Code))
            .ToDictionary(u => u.Code!, u => u.Value, StringComparer.OrdinalIgnoreCase);

        return new SaveLifeMiniAppConfigSecret
        {
            AppId = ReadString(values, AppIdCode, null),
            AppSecret = ReadSecret(values, AppSecretCode, null),
            Token = ReadString(values, TokenCode, null),
            EncodingAesKey = ReadString(values, EncodingAesKeyCode, null),
            QrImagePath = ReadString(values, QrImagePathCode, null),
            RecruitmentEnabled = ReadBoolean(values, RecruitmentEnabledCode, true)
        };
    }

    private static string? ResolveQrCodeFilePath(string? configuredPath)
    {
        var path = Normalize(configuredPath);
        if (string.IsNullOrWhiteSpace(path))
            return null;

        if (!Path.IsPathRooted(path))
        {
            var webRoot = App.WebHostEnvironment.WebRootPath ?? App.WebHostEnvironment.ContentRootPath;
            path = Path.Combine(webRoot, path);
        }

        return string.Equals(Path.GetExtension(path), ".png", StringComparison.OrdinalIgnoreCase)
            ? path
            : Path.Combine(path, QrCodeFileName);
    }

    private static void TryDeleteQrCodeFile(string? configuredPath)
    {
        var filePath = ResolveQrCodeFilePath(configuredPath);
        if (string.IsNullOrWhiteSpace(filePath))
            return;

        try
        {
            if (File.Exists(filePath))
                File.Delete(filePath);
        }
        catch (IOException)
        {
            // A stale disk cache must not prevent configuration from being saved.
        }
        catch (UnauthorizedAccessException)
        {
            // A stale disk cache must not prevent configuration from being saved.
        }
    }

    private async Task<WechatApiClient> CreateClientAsync()
    {
        var secret = await GetSecretAsync();
        if (string.IsNullOrWhiteSpace(secret.AppId) || string.IsNullOrWhiteSpace(secret.AppSecret))
            throw Oops.Oh("微信小程序配置未完成，请先在医线生机小程序设置中配置 AppId 和 AppSecret").StatusCode(400);

        var client = WechatApiClientBuilder.Create(new WechatApiClientOptions
        {
            AppId = secret.AppId,
            AppSecret = secret.AppSecret,
            PushToken = secret.Token,
            PushEncodingAESKey = secret.EncodingAesKey
        })
            .UseHttpClient(_httpClientFactory.CreateClient(SaveLifeConst.MiniAppHttpClientName), disposeClient: false)
            .Build();

        client.Configure(config =>
        {
            var settings = SystemTextJsonSerializer.GetDefaultSerializerOptions();
            settings.WriteIndented = true;
            config.JsonSerializer = new SystemTextJsonSerializer(settings);
        });

        return client;
    }

    private async Task<string> GetAccessTokenAsync(WechatApiClient client)
    {
        var secret = await GetSecretAsync();
        var cacheKey = $"SaveLifeWxOpenAccessToken_{secret.AppId}";
        var cached = _cacheService.Get<string>(cacheKey);
        if (!string.IsNullOrWhiteSpace(cached))
            return cached;

        var response = await client.ExecuteCgibinTokenAsync(new CgibinTokenRequest());
        if (response.ErrorCode != (int)WechatReturnCodeEnum.请求成功)
            throw Oops.Oh(response.ErrorMessage + " " + response.ErrorCode);

        var accessToken = response.AccessToken;
        if (string.IsNullOrWhiteSpace(accessToken))
            throw Oops.Oh("微信接口未返回有效的 access_token");

        _cacheService.Set(cacheKey, accessToken, TimeSpan.FromSeconds(response.ExpiresIn - 60));
        return accessToken;
    }

    private async Task UpsertConfigAsync(string code, string value)
    {
        var entity = await _configRep.GetFirstAsync(u => u.Code == code);
        if (entity == null)
        {
            await _configRep.InsertAsync(new SysConfig
            {
                Name = GetConfigName(code),
                Code = code,
                Value = Limit(value, 512),
                SysFlag = YesNoEnum.N,
                GroupCode = GroupCode,
                OrderNo = GetConfigOrder(code),
                Remark = "医线生机小程序全局配置",
                CreateTime = DateTime.Now
            });
            return;
        }

        entity.Value = Limit(value, 512);
        entity.GroupCode = GroupCode;
        await _configRep.AsUpdateable(entity).IgnoreColumns(true).ExecuteCommandAsync();
    }

    private static void ValidateInput(SaveLifeMiniAppConfigInput input)
    {
        ValidateLength(input.AppId, 128, "AppId 不能超过 128 个字符");
        ValidateSecret(input.AppSecret, "AppSecret 不能超过 240 个字符");
        ValidateLength(input.Token, 128, "Token 不能超过 128 个字符");
        ValidateLength(input.EncodingAesKey, 128, "EncodingAESKey 不能超过 128 个字符");
        ValidateLength(input.QrImagePath, 512, "二维码保存路径不能超过 512 个字符");
    }

    private static string? ReadString(Dictionary<string, string?> values, string code, string? defaultValue)
    {
        return values.TryGetValue(code, out var value) && !string.IsNullOrWhiteSpace(value)
            ? value.Trim()
            : Normalize(defaultValue);
    }

    private static string? ReadSecret(Dictionary<string, string?> values, string code, string? defaultValue)
    {
        var value = ReadString(values, code, null);
        if (string.IsNullOrWhiteSpace(value))
            return Normalize(defaultValue);

        return UnprotectSecret(value) ?? Normalize(defaultValue);
    }

    private static string? Normalize(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }

    private static string Limit(string value, int maxLength)
    {
        return value.Length <= maxLength ? value : value[..maxLength];
    }

    private static string ProtectSecret(string value)
    {
        try
        {
            return $"{SecretPrefix}{CryptogramUtil.SM4EncryptECB(value)}";
        }
        catch
        {
            throw Oops.Oh("小程序密钥加密失败").StatusCode(500);
        }
    }

    private static string? UnprotectSecret(string value)
    {
        var normalized = Normalize(value);
        if (string.IsNullOrWhiteSpace(normalized))
            return null;

        if (!normalized.StartsWith(SecretPrefix, StringComparison.OrdinalIgnoreCase))
            return normalized;

        try
        {
            return CryptogramUtil.SM4DecryptECB(normalized[SecretPrefix.Length..]);
        }
        catch
        {
            return null;
        }
    }

    private static string? MaskSecret(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        var trimmed = value.Trim();
        if (trimmed.Length <= 8)
            return "****";

        return $"{trimmed[..4]}****{trimmed[^4..]}";
    }

    private static string BuildOpenIdErrorMessage(int errorCode, string? errorMessage, string? appId)
    {
        var maskedAppId = MaskSecret(appId) ?? "未配置";
        return errorCode switch
        {
            InvalidJsCodeErrorCode => $"微信登录 code 无效，请确认微信开发者工具项目 AppId 与医线生机小程序设置 AppId 一致。当前后端 AppId：{maskedAppId}，code 只能使用一次且会过期。微信返回：{errorMessage} {errorCode}",
            UsedJsCodeErrorCode => $"微信登录 code 已被使用，请重新触发微信登录认证。当前后端 AppId：{maskedAppId}，微信返回：{errorMessage} {errorCode}",
            _ => $"{errorMessage} {errorCode}"
        };
    }

    private static void ValidateSecret(string? value, string message)
    {
        var normalized = Normalize(value);
        if (!string.IsNullOrWhiteSpace(normalized) && Encoding.UTF8.GetByteCount(normalized) > MaxSecretBytes)
            throw Oops.Oh(message).StatusCode(400);
    }

    private static void ValidateLength(string? value, int maxLength, string message)
    {
        var normalized = Normalize(value);
        if (!string.IsNullOrWhiteSpace(normalized) && normalized.Length > maxLength)
            throw Oops.Oh(message).StatusCode(400);
    }

    private static string GetConfigName(string code)
    {
        return code switch
        {
            AppIdCode => "医线生机小程序 AppId",
            AppSecretCode => "医线生机小程序 AppSecret",
            TokenCode => "医线生机小程序 Token",
            EncodingAesKeyCode => "医线生机小程序 EncodingAESKey",
            QrImagePathCode => "医线生机小程序二维码保存路径",
            RecruitmentEnabledCode => "医线生机项目招募开关",
            _ => code
        };
    }

    private static int GetConfigOrder(string code)
    {
        return code switch
        {
            AppIdCode => 10,
            AppSecretCode => 20,
            TokenCode => 30,
            EncodingAesKeyCode => 40,
            QrImagePathCode => 50,
            RecruitmentEnabledCode => 60,
            _ => 999
        };
    }

    private static bool ReadBoolean(Dictionary<string, string?> values, string code, bool defaultValue)
    {
        if (!values.TryGetValue(code, out var value) || string.IsNullOrWhiteSpace(value))
            return defaultValue;

        return bool.TryParse(value.Trim(), out var parsed) ? parsed : defaultValue;
    }
}
