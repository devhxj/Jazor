


using Jero.Plugin.DingTalk;
using Furion.Schedule;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
namespace Jero.Plugin.Job;

/// <summary>
/// 同步钉钉角色job,自动同步触发器请在web页面按需求设置
/// </summary>
[JobDetail(
    "SyncWokerflowLogJob",
    Description = "同步钉钉审批状态",
    GroupName = "default",
    Concurrent = false
)]
[Daily(TriggerId = "SyncWokerflowLogTrigger", Description = "同步钉钉审批状态")]
public class SyncWokerflowLogJob : IJob
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IDingTalkApi _dingTalkApi;
    private readonly ILogger _logger;
    private readonly SqlSugarRepository<DingTalkDept> _dingTalkDeptRep;
    private readonly SqlSugarRepository<DingTalkWokerflowLog> _dingTalkWokerflowLogRep;

    public SyncWokerflowLogJob(
        IServiceScopeFactory scopeFactory,
        IDingTalkApi dingTalkApi,
        SqlSugarRepository<DingTalkDept> dingTalkDeptRep,
        SqlSugarRepository<DingTalkWokerflowLog> dingTalkWokerflowLogRep,
        ILoggerFactory loggerFactory
    )
    {
        _scopeFactory = scopeFactory;
        _dingTalkApi = dingTalkApi;
        _dingTalkDeptRep = dingTalkDeptRep;
        _dingTalkWokerflowLogRep = dingTalkWokerflowLogRep;
        _logger = loggerFactory.CreateLogger(CommonConst.SysLogCategoryName);
    }

    public async Task ExecuteAsync(JobExecutingContext context, CancellationToken stoppingToken)
    {
        using var serviceScope = _scopeFactory.CreateScope();
        var _dingTalkOptions = serviceScope.ServiceProvider.GetRequiredService<
            IOptions<DingTalkOptions>
        >();

        // 获取Token
        var tokenRes = await _dingTalkApi.GetDingTalkToken(
            _dingTalkOptions.Value.ClientId,
            _dingTalkOptions.Value.ClientSecret
        );
        if (tokenRes.ErrCode != 0)
            throw Oops.Oh(tokenRes.ErrMsg);

        var dingTalkDeptList = new List<DingTalkDept>();
        // 获取未完成审批列表
        List<DingTalkWokerflowLog> flow_list = await _dingTalkWokerflowLogRep.GetListAsync(t =>
            t.Status == "RUNNING"
        );
        List<DingTalkWokerflowLog> update_list = new List<DingTalkWokerflowLog>();
        if (flow_list?.Count > 0)
        {
            foreach (var item in flow_list)
            {
                var flow = await _dingTalkApi.GetProcessInstances(
                    tokenRes.AccessToken,
                    item.instanceId
                );
                if (flow.Result.Status != item.Status)
                {
                    item.Status = flow.Result.Status;
                    item.UpdateTime = DateTime.Now;
                    item.WorkflowId = flow.Result.BusinessId;
                    item.taskId = flow
                        .Result.Tasks.FirstOrDefault(t => t.Status == "RUNNING")
                        ?.TaskId;
                    update_list.Add(item);
                }
            }

            if (update_list.Count > 0)
            {
                await _dingTalkWokerflowLogRep.UpdateRangeAsync(update_list);
            }
            var originColor = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.WriteLine("【" + DateTime.Now + "】同步钉钉审批记录状态");
            Console.ForegroundColor = originColor;
        }
    }
}