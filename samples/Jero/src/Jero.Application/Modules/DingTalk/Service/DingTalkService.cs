using Furion.FriendlyException;
using Microsoft.Extensions.Options;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq.Dynamic.Core;

namespace Jero.Application.Modules.DingTalk.Service;

/// <summary>
/// 钉钉服务 🧩
/// </summary>
[ApiDescriptionSettings(DingTalkConst.GroupName, Order = 100)]
public class DingTalkService : IDynamicApiController, IScoped
{
    private readonly IDingTalkApi _dingTalkApi;
    private readonly DingTalkOptions _dingTalkOptions;
    private readonly SqlSugarRepository<DingTalkWokerflowLog> _dingTalkWokerflowLogRep;

    public DingTalkService(
        IDingTalkApi dingTalkApi,
        IOptions<DingTalkOptions> dingTalkOptions,
        SqlSugarRepository<DingTalkWokerflowLog> dingTalkWokerflowLogRep
    )
    {
        _dingTalkApi = dingTalkApi;
        _dingTalkOptions = dingTalkOptions.Value;
        _dingTalkWokerflowLogRep = dingTalkWokerflowLogRep;
    }

    /// <summary>
    /// 获取企业内部应用的access_token
    /// </summary>
    /// <returns></returns>
    [DisplayName("获取企业内部应用的access_token")]
    public async Task<GetDingTalkTokenOutput> GetDingTalkToken()
    {
        var tokenRes = await _dingTalkApi.GetDingTalkToken(
            _dingTalkOptions.ClientId,
            _dingTalkOptions.ClientSecret
        );
        if (tokenRes.ErrCode != 0)
        {
            throw Oops.Oh(tokenRes.ErrMsg);
        }
        return tokenRes;
    }

    /// <summary>
    /// 获取在职员工列表 🔖
    /// </summary>
    /// <param name="access_token"></param>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpPost, DisplayName("获取在职员工列表")]
    public async Task<
        DingTalkBaseResponse<GetDingTalkCurrentEmployeesListOutput>
    > GetDingTalkCurrentEmployeesList(
        string access_token,
        [Required] GetDingTalkCurrentEmployeesListInput input
    )
    {
        return await _dingTalkApi.GetDingTalkCurrentEmployeesList(access_token, input);
    }

    /// <summary>
    /// 获取员工花名册字段信息 🔖
    /// </summary>
    /// <param name="access_token"></param>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpPost, DisplayName("获取员工花名册字段信息")]
    public async Task<
        DingTalkBaseResponse<List<DingTalkEmpRosterFieldVo>>
    > GetDingTalkCurrentEmployeesRosterList(
        string access_token,
        [Required] GetDingTalkCurrentEmployeesRosterListInput input
    )
    {
        return await _dingTalkApi.GetDingTalkCurrentEmployeesRosterList(access_token, input);
    }

    /// <summary>
    /// 发送钉钉互动卡片 🔖
    /// </summary>
    /// <param name="token"></param>
    /// <param name="input"></param>
    /// <returns></returns>
    [DisplayName("给指定用户发送钉钉互动卡片")]
    [Obsolete]
    public async Task<DingTalkSendInteractiveCardsOutput> DingTalkSendInteractiveCards(
        string token,
        DingTalkSendInteractiveCardsInput input
    )
    {
        return await _dingTalkApi.DingTalkSendInteractiveCards(token, input);
    }

    /// <summary>
    /// 创建并投放钉钉消息卡片 🔖
    /// </summary>
    /// <param name="token"></param>
    /// <param name="input"></param>
    /// <returns></returns>
    [DisplayName("给指定用户发送钉钉消息卡片")]
    public async Task<DingTalkCreateAndDeliverOutput> DingTalkCreateAndDeliver(
        string token,
        DingTalkCreateAndDeliverInput input
    )
    {
        return await _dingTalkApi.DingTalkCreateAndDeliver(token, input);
    }

    [DisplayName("用于发起OA审批实例")]
    public async Task<DingTalkWorkflowProcessInstancesOutput> DingTalkWorkflowProcessInstances(
        string token,
        DingTalkWorkflowProcessInstancesInput input
    )
    {
        var temp = await _dingTalkApi.DingTalkWorkflowProcessInstances(token, input);
        return temp;
    }

    [DisplayName("查询审批实例")]
    public async Task<DingTalkGetProcessInstancesOutput> DingTalkWorkflowProcessInstances(
        string token,
        string input
    )
    {
        var temp = await _dingTalkApi.GetProcessInstances(token, input);
        DingTalkWokerflowLog flow = await _dingTalkWokerflowLogRep.GetFirstAsync(t =>
            t.Status == "RUNNING" && t.instanceId == input
        );

        if ((flow != null) && (temp.Result.Status != flow.Status))
        {
            flow.Status = temp.Result.Status;
            flow.UpdateTime = DateTime.Now;
            flow.WorkflowId = temp.Result.BusinessId;
            flow.Result = temp.Result.Result;
            flow.taskId = temp.Result.Tasks.FirstOrDefault(t => t.Status == "RUNNING")?.TaskId;
            await _dingTalkWokerflowLogRep.UpdateAsync(flow);
        }
        return temp;
    }
}