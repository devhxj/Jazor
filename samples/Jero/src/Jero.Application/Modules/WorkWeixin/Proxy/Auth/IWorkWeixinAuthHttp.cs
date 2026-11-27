using Furion.HttpRemote;

namespace Jero.Application.Modules.WorkWeixin.Proxy.AppChat;

/// <summary>
/// 授权会话远程服务
/// </summary>
public interface IWorkWeixinAuthHttp : IHttpDeclarative
{
    /// <summary>
    /// 获取接口凭证
    /// </summary>
    /// <param name="corpId">企业ID</param>
    /// <param name="corpSecret">应用的凭证密钥</param>
    /// <returns></returns>
    /// <see href="https://developer.work.weixin.qq.com/document/path/91039"/>
    [Post("https://qyapi.weixin.qq.com/cgi-bin/gettoken")]
    Task<AuthAccessTokenHttpOutput> GetToken([QueryParam("corpid")] string corpId, [QueryParam("corpsecret")] string corpSecret);
}