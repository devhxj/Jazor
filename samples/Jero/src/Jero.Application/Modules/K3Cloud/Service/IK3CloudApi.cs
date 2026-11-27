using Furion.HttpRemote;

namespace Jero.Application.Modules.K3Cloud.Service;

/// <summary>
/// 金蝶云星空ERP接口
/// </summary>
[HttpClientName("K3Cloud")]
public interface IK3CloudApi : IHttpDeclarative
{
    /// <summary>
    /// 验证用户
    /// </summary>
    /// <param name="input"></param>
    /// <param name="action"></param>
    /// <returns></returns>
    [Post("Kingdee.BOS.WebApi.ServicesStub.AuthService.ValidateUser.common.kdsvc")]
    Task<K3CloudLoginOutput> ValidateUser([Body] K3CloudLoginInput input, Action<HttpClient, HttpResponseMessage>? action = null);

    /// <summary>
    /// 保存表单
    /// </summary>
    /// <param name="input"></param>
    /// <param name="action"></param>
    /// <returns></returns>
    [Post("Kingdee.BOS.WebApi.ServicesStub.DynamicFormService.Save.common.kdsvc")]
    Task<K3CloudPushResultOutput> Save<T>([Body] K3CloudBaeInput<T> input, Action<HttpClient, HttpRequestMessage>? action = null);

    /// <summary>
    /// 提交表单
    /// </summary>
    /// <param name="input"></param>
    /// <param name="action"></param>
    /// <returns></returns>
    [Post("Kingdee.BOS.WebApi.ServicesStub.DynamicFormService.Submit.common.kdsvc")]
    Task<K3CloudPushResultOutput> Submit<T>([Body] K3CloudBaeInput<T> input, Action<HttpClient, HttpRequestMessage>? action = null);

    /// <summary>
    /// 审核表单
    /// </summary>
    /// <param name="input"></param>
    /// <param name="action"></param>
    /// <returns></returns>
    [Post("Kingdee.BOS.WebApi.ServicesStub.DynamicFormService.Audit.common.kdsvc")]
    Task<K3CloudPushResultOutput> Audit<T>([Body] K3CloudBaeInput<T> input, Action<HttpClient, HttpRequestMessage>? action = null);
}
