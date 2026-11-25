namespace Jero.Plugin.WorkWeixin.Proxy.AppChat;

/// <summary>
/// 群聊会话远程调用服务
/// </summary>
public interface IWorkWeixinAppChatHttp : IHttpDeclarative
{
    /// <summary>
    /// 创建群聊会话
    /// </summary>
    /// <param name="accessToken"></param>
    /// <param name="body"></param>
    /// <returns></returns>
    /// <see href="https://developer.work.weixin.qq.com/document/path/90245"/>
    [Post("https://qyapi.weixin.qq.com/cgi-bin/appchat/create")]
    Task<CreatAppChatOutput> Create([QueryParam("access_token")] string accessToken, [Body] CreatAppChatInput body);

    /// <summary>
    /// 修改群聊会话
    /// </summary>
    /// <param name="accessToken"></param>
    /// <param name="body"></param>
    /// <returns></returns>
    /// <see href="https://developer.work.weixin.qq.com/document/path/98913"/>
    [Post("https://qyapi.weixin.qq.com/cgi-bin/appchat/update")]
    Task<CreatAppChatOutput> Update([QueryParam("access_token")] string accessToken, [Body] UpdateAppChatInput body);

    /// <summary>
    /// 获取群聊会话
    /// </summary>
    /// <param name="accessToken"></param>
    /// <param name="chatId"></param>
    /// <returns></returns>
    /// <see href="https://developer.work.weixin.qq.com/document/path/98914"/>
    [Get("https://qyapi.weixin.qq.com/cgi-bin/appchat/get")]
    Task<CreatAppChatOutput> Get([QueryParam("access_token")] string accessToken, [QueryParam("chatid")] string chatId);

    /// <summary>
    /// 应用推送消息
    /// </summary>
    /// <param name="accessToken"></param>
    /// <param name="body"></param>
    /// <returns></returns>
    /// <see href="https://developer.work.weixin.qq.com/document/path/90248"/>
    [Post("https://qyapi.weixin.qq.com/cgi-bin/appchat/send")]
    Task<BaseWorkOutput> Send([QueryParam("access_token")] string accessToken, [Body] SendBaseAppChatInput body);
}