namespace Jero.Plugin.DingTalk;

public interface IDingTalkApi : IHttpDeclarative
{
    /// <summary>
    /// 获取企业内部应用的access_token
    /// </summary>
    /// <param name="appkey">应用的唯一标识key</param>
    /// <param name="appsecret"> 应用的密钥。AppKey和AppSecret可在钉钉开发者后台的应用详情页面获取。</param>
    /// <returns></returns>
    [Get("https://oapi.dingtalk.com/gettoken")]
    Task<GetDingTalkTokenOutput> GetDingTalkToken([QueryParam] string appkey, [QueryParam] string appsecret);

    /// <summary>
    /// 获取在职员工列表
    /// </summary>
    /// <param name="access_token">调用该接口的应用凭证</param>
    /// <param name="input"></param>
    /// <returns></returns>
    [Post("https://oapi.dingtalk.com/topapi/smartwork/hrm/employee/queryonjob")]
    Task<
        DingTalkBaseResponse<GetDingTalkCurrentEmployeesListOutput>
    > GetDingTalkCurrentEmployeesList(
        [QueryParam] string access_token,
        [Body(ContentType = "application/json", UseStringContent = true), Required]
            GetDingTalkCurrentEmployeesListInput input
    );

    /// <summary>
    /// 获取员工花名册字段信息
    /// </summary>
    /// <param name="access_token">调用该接口的应用凭证</param>
    /// <param name="input"></param>
    /// <returns></returns>
    [Post("https://oapi.dingtalk.com/topapi/smartwork/hrm/employee/v2/list")]
    Task<
        DingTalkBaseResponse<List<DingTalkEmpRosterFieldVo>>
    > GetDingTalkCurrentEmployeesRosterList(
        [QueryParam] string access_token,
        [Body(ContentType = "application/json", UseStringContent = true), Required]
            GetDingTalkCurrentEmployeesRosterListInput input
    );

    /// <summary>
    /// 发送钉钉互动卡片
    /// </summary>
    /// <param name="token">调用该接口的访问凭证</param>
    /// <param name="input"></param>
    /// <returns></returns>
    /// <remarks>
    /// 钉钉官方文档显示接口不再支持新应用接入, 已接入的应用可继续调用
    /// 推荐更新接口https://open.dingtalk.com/document/orgapp/create-and-deliver-cards?spm=ding_open_doc.document.0.0.67fc50988Pf0mc
    /// </remarks>
    [Post("https://api.dingtalk.com/v1.0/im/interactiveCards/send")]
    [Obsolete]
    Task<DingTalkSendInteractiveCardsOutput> DingTalkSendInteractiveCards(
        [Header("x-acs-dingtalk-access-token")] string token,
        [Body(ContentType = "application/json", UseStringContent = true)]
            DingTalkSendInteractiveCardsInput input
    );

    /// <summary>
    /// 获取钉钉卡片消息读取状态
    /// </summary>
    /// <param name="token"></param>
    /// <param name="input"></param>
    /// <returns></returns>
    [Get("https://api.dingtalk.com/v1.0/robot/oToMessages/readStatus")]
    Task<GetDingTalkCardMessageReadStatusOutput> GetDingTalkCardMessageReadStatus(
        [Header("x-acs-dingtalk-access-token")] string token,
        [QueryParam] GetDingTalkCardMessageReadStatusInput input
    );

    /// <summary>
    /// 获取角色列表
    /// </summary>
    /// <param name="access_token">调用该接口的应用凭证</param>
    /// <param name="input"></param>
    /// <returns></returns>
    [Post("https://oapi.dingtalk.com/topapi/role/list")]
    Task<DingTalkBaseResponse<DingTalkRoleListOutput>> GetDingTalkRoleList(
        [QueryParam] string access_token,
        [Body(ContentType = "application/json", UseStringContent = true), Required]
            GetDingTalkCurrentRoleListInput input
    );

    /// <summary>
    /// 获取指定角色的员工列表
    /// </summary>
    /// <param name="access_token">调用该接口的应用凭证</param>
    /// <param name="input"></param>
    /// <returns></returns>
    [Post("https://oapi.dingtalk.com/topapi/role/simplelist")]
    Task<DingTalkBaseResponse<DingTalkRoleSimplelistOutput>> GetDingTalkRoleSimplelist(
        [QueryParam] string access_token,
        [Body(ContentType = "application/json", UseStringContent = true), Required]
            GetDingTalkCurrentRoleSimplelistInput input
    );

    /// <summary>
    /// 创建并投放钉钉消息卡片
    /// </summary>
    /// <param name="token"></param>
    /// <param name="input"></param>
    /// <returns></returns>
    [Post("https://api.dingtalk.com/v1.0/card/instances/createAndDeliver")]
    Task<DingTalkCreateAndDeliverOutput> DingTalkCreateAndDeliver(
        [Header("x-acs-dingtalk-access-token")] string token,
        [Body(ContentType = "application/json", UseStringContent = true)]
            DingTalkCreateAndDeliverInput input
    );

    /// <summary>
    /// 获取部门列表列表
    /// </summary>
    /// <param name="access_token">调用该接口的应用凭证</param>
    /// <param name="input"></param>
    /// <returns></returns>
    [Post("https://oapi.dingtalk.com/topapi/v2/department/listsub")]
    Task<DingTalkBaseResponse<List<DingTalkDeptOutput>>> GetDingTalkDept(
        [QueryParam] string access_token,
        [Body(ContentType = "application/json", UseStringContent = true), Required]
            GetDingTalkDeptInput input
    );

    /// <summary>
    /// 发起审批实例
    /// </summary>
    /// <param name="token"></param>
    /// <param name="input"></param>
    /// <returns></returns>
    [Post("https://api.dingtalk.com/v1.0/workflow/processInstances")]
    Task<DingTalkWorkflowProcessInstancesOutput> DingTalkWorkflowProcessInstances(
        [Header("x-acs-dingtalk-access-token")] string token,
        [Body(ContentType = "application/json", UseStringContent = true), Required]
            DingTalkWorkflowProcessInstancesInput input
    );

    /// <summary>
    /// 查询审批实例
    /// </summary>
    /// <param name="token"></param>
    /// <param name="processInstanceId"></param>
    /// <returns></returns>
    [Get("https://api.dingtalk.com/v1.0/workflow/processInstances")]
    Task<DingTalkGetProcessInstancesOutput> GetProcessInstances(
        [Header("x-acs-dingtalk-access-token")] string token,
        [QueryParam] string processInstanceId
    );
}