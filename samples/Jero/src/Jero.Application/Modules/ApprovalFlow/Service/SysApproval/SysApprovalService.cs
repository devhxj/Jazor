using SqlSugar;
using Microsoft.AspNetCore.Http;

namespace Jero.Application.Modules.ApprovalFlow.Service;

public class SysApprovalService : ITransient
{
    private readonly SqlSugarRepository<ApprovalFlowRecord> _approvalFlowRep;
    private readonly SqlSugarRepository<ApprovalFormRecord> _approvalFormRep;
    private readonly ApprovalFlowService _approvalFlowService;

    public SysApprovalService(SqlSugarRepository<ApprovalFlowRecord> approvalFlowRep, SqlSugarRepository<ApprovalFormRecord> approvalFormRep, ApprovalFlowService approvalFlowService)
    {
        _approvalFlowRep = approvalFlowRep;
        _approvalFormRep = approvalFormRep;
        _approvalFlowService = approvalFlowService;
    }

    /// <summary>
    /// 匹配审批流程
    /// </summary>
    /// <param name="context"></param>
    /// <returns></returns>
    [NonAction]
    public async Task MatchApproval(HttpContext context)
    {
        var request = context.Request;
        var response = context.Response;

        var path = request.Path.ToString().Split("/");

        var method = request.Method;
        var qs = request.QueryString;
        var h = request.Headers;
        var b = request.Body;

        var requestHeaders = request.Headers;
        var responseHeaders = response.Headers;

        var serviceName = path[1];
        if (serviceName.StartsWith("api"))
        {
            if (path.Length > 3)
            {
                var funcName = path[2];
                var typeName = path[3];

                var list = await _approvalFlowService.FormRoutes();
                if (list.Any(u => u.Contains(funcName) && u.Contains(typeName)))
                {
                    var approvalFlow = new ApprovalFlowRecord
                    {
                        FormName = funcName,
                        CreateTime = DateTime.Now,
                    };

                    // 判断是否需要审批
                    await _approvalFlowRep.InsertAsync(approvalFlow);

                    var approvalForm = new ApprovalFormRecord
                    {
                        FlowId = approvalFlow.Id,
                        FormName = funcName,
                        FormType = typeName,
                        CreateTime = DateTime.Now,
                    };

                    // 判断是否需要审批
                    await _approvalFormRep.InsertAsync(approvalForm);
                }
            }
        }

        await Task.CompletedTask;
    }
}