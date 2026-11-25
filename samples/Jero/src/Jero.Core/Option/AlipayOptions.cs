


using Aop.Api;
namespace Jero.Core;

/// <summary>
/// 支付宝支付配置选项
/// </summary>
public sealed class AlipayOptions : IConfigurableOptions
{
    /// <summary>
    /// 支付宝网关地址
    /// </summary>
    public string ServerUrl { get; init; } = string.Empty;
    /// <summary>
    /// 支付宝授权回调地址
    /// </summary>
    public string AuthUrl { get; init; } = string.Empty;
    /// <summary>
    /// 应用授权回调地址
    /// </summary>
    public string AppAuthUrl { get; init; } = string.Empty;
    /// <summary>
    /// 支付宝 websocket 服务地址
    /// </summary>
    public string WebsocketUrl { get; init; } = string.Empty;
    /// <summary>
    /// 应用回调地址
    /// </summary>
    public string NotifyUrl { get; init; } = string.Empty;
    /// <summary>
    /// 支付宝根证书存放路径
    /// </summary>
    public string RootCertPath { get; init; } = string.Empty;
    /// <summary>
    /// 支付宝商户账号列表
    /// </summary>
    public List<AlipayMerchantAccount> AccountList { get; init; } = [];
    /// <summary>
    /// 获取支付宝客户端
    /// </summary>
    /// <param name="account"></param>
    public DefaultAopClient GetClient(AlipayMerchantAccount account)
    {
        account = account ?? throw new Exception("未找到支付宝商户账号");
        string path = App.WebHostEnvironment.ContentRootPath;
        return new DefaultAopClient(new AlipayConfig
        {
            Format = "json",
            Charset = "UTF-8",
            ServerUrl = ServerUrl,
            AppId = account.AppId,
            SignType = account.SignType,
            PrivateKey = account.PrivateKey,
            EncryptKey = account.EncryptKey,
            RootCertPath = Path.Combine(path, RootCertPath),
            AppCertPath = Path.Combine(path, account.AppCertPath),
            AlipayPublicCertPath = Path.Combine(path, account.AlipayPublicCertPath)
        });
    }
}

/// <summary>
/// 支付宝商户账号信息
/// </summary>
public class AlipayMerchantAccount
{
    /// <summary>
    /// 配置Id
    /// </summary>
    public long Id { get; init; }

    /// <summary>
    /// 商户名称
    /// </summary>
    public string Name { get; init; } = string.Empty;
    /// <summary>
    /// 商户AppId
    /// </summary>
    public string AppId { get; init; } = string.Empty;
    /// <summary>
    /// 应用私钥
    /// </summary>
    public string PrivateKey { get; init; } = string.Empty;
    /// <summary>
    /// 从支付宝获取敏感信息时的加密密钥（可选）
    /// </summary>
    public string EncryptKey { get; init; } = string.Empty;
    /// <summary>
    /// 加密算法
    /// </summary>
    public string SignType { get; init; } = string.Empty;
    /// <summary>
    /// 应用公钥证书路径
    /// </summary>
    public string AppCertPath { get; init; } = string.Empty;
    /// <summary>
    /// 支付宝公钥证书路径
    /// </summary>
    public string AlipayPublicCertPath { get; init; } = string.Empty;
}