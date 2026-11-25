namespace Jero.Plugin.WorkWeixin.Proxy.AppChat;

/// <summary>
/// 部门远程调用服务
/// </summary>
public interface IDepartmentHttp : IHttpDeclarative
{
    /// <summary>
    /// 创建部门
    /// https://developer.work.weixin.qq.com/document/path/90205
    /// </summary>
    /// <param name="accessToken"></param>
    /// <param name="body"></param>
    /// <returns></returns>
    [Post("https://qyapi.weixin.qq.com/cgi-bin/department/create")]
    Task<BaseWorkIdOutput> Create([QueryParam("access_token")] string accessToken, [Body] DepartmentHttpInput body);

    /// <summary>
    /// 修改部门
    /// https://developer.work.weixin.qq.com/document/path/90206
    /// </summary>
    /// <param name="accessToken"></param>
    /// <param name="body"></param>
    /// <returns></returns>
    [Post("https://qyapi.weixin.qq.com/cgi-bin/department/update")]
    Task<BaseWorkOutput> Update([QueryParam("access_token")] string accessToken, [Body] DepartmentHttpInput body);

    /// <summary>
    /// 删除部门
    /// https://developer.work.weixin.qq.com/document/path/90207
    /// </summary>
    /// <param name="accessToken"></param>
    /// <param name="id"></param>
    /// <returns></returns>
    [Get("https://qyapi.weixin.qq.com/cgi-bin/department/delete")]
    Task<BaseWorkOutput> Delete([QueryParam("access_token")] string accessToken, [QueryParam] long id);

    /// <summary>
    /// 获取部门Id列表
    /// https://developer.work.weixin.qq.com/document/path/90208
    /// </summary>
    /// <param name="accessToken"></param>
    /// <param name="id"></param>
    /// <returns></returns>
    [Get("https://qyapi.weixin.qq.com/cgi-bin/department/simplelist")]
    Task<DepartmentIdOutput> SimpleList([QueryParam("access_token")] string accessToken, [QueryParam] long id);

    /// <summary>
    /// 获取部门详情
    /// https://developer.work.weixin.qq.com/document/path/90208
    /// </summary>
    /// <param name="accessToken"></param>
    /// <param name="id"></param>
    /// <returns></returns>
    [Get("https://qyapi.weixin.qq.com/cgi-bin/department/get")]
    Task<DepartmentOutput> Get([QueryParam("access_token")] string accessToken, [QueryParam] long id);
}