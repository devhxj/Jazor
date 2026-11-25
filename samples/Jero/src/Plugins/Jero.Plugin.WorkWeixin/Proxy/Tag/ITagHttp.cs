namespace Jero.Plugin.WorkWeixin.Proxy;

/// <summary>
/// 标签远程调用服务
/// </summary>
public interface ITagHttp : IHttpDeclarative
{
    /// <summary>
    /// 创建标签
    /// https://developer.work.weixin.qq.com/document/path/90210
    /// </summary>
    /// <param name="accessToken"></param>
    /// <param name="body"></param>
    /// <returns></returns>
    [Post("https://qyapi.weixin.qq.com/cgi-bin/tag/create")]
    Task<BaseWorkIdOutput> Create([QueryParam("access_token")] string accessToken, [Body] TagHttpInput body);

    /// <summary>
    /// 更新标签名字
    /// https://developer.work.weixin.qq.com/document/path/90211
    /// </summary>
    /// <param name="accessToken"></param>
    /// <param name="body"></param>
    /// <returns></returns>
    [Post("https://qyapi.weixin.qq.com/cgi-bin/tag/update")]
    Task<TagIdHttpOutput> Update([QueryParam("access_token")] string accessToken, [Body] TagHttpInput body);

    /// <summary>
    /// 删除标签
    /// https://developer.work.weixin.qq.com/document/path/90212
    /// </summary>
    /// <param name="accessToken"></param>
    /// <param name="tagId"></param>
    /// <returns></returns>
    [Get("https://qyapi.weixin.qq.com/cgi-bin/tag/delete")]
    Task<BaseWorkOutput> Delete([QueryParam("access_token")] string accessToken, [QueryParam("tagid")] long tagId);

    /// <summary>
    /// 获取标签详情
    /// https://developer.work.weixin.qq.com/document/path/90213
    /// </summary>
    /// <param name="accessToken"></param>
    /// <param name="tagId"></param>
    /// <returns></returns>
    [Get("https://qyapi.weixin.qq.com/cgi-bin/tag/get")]
    Task<DepartmentOutput> Get([QueryParam("access_token")] string accessToken, [QueryParam("tagid")] long tagId);

    /// <summary>
    /// 增加标签成员
    /// https://developer.work.weixin.qq.com/document/path/90214
    /// </summary>
    /// <param name="accessToken"></param>
    /// <param name="body"></param>
    /// <returns></returns>
    [Post("https://qyapi.weixin.qq.com/cgi-bin/tag/addtagusers")]
    Task<DepartmentOutput> AddTagUsers([QueryParam("access_token")] string accessToken, [Body] TagUsersTagInput body);

    /// <summary>
    /// 删除标签成员
    /// https://developer.work.weixin.qq.com/document/path/90215
    /// </summary>
    /// <param name="accessToken"></param>
    /// <param name="body"></param>
    /// <returns></returns>
    [Post("https://qyapi.weixin.qq.com/cgi-bin/tag/deltagusers")]
    Task<DepartmentOutput> DelTagUsers([QueryParam("access_token")] string accessToken, [Body] TagUsersTagInput body);

    /// <summary>
    /// 获取标签列表
    /// https://developer.work.weixin.qq.com/document/path/90216
    /// </summary>
    /// <param name="accessToken"></param>
    /// <returns></returns>
    [Get("https://qyapi.weixin.qq.com/cgi-bin/tag/list")]
    Task<TagListHttpOutput> List([QueryParam("access_token")] string accessToken);
}