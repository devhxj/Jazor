namespace Jero.Web.Pages.ApprovalFlow;

public sealed record ApprovalFlowTableRow(
    [property: ECMAScriptName("id")] string Id,
    string Name,
    string Code,
    string Remark,
    string Status,
    string StatusTag,
    string FormRoute,
    string FlowSummary);
public sealed record ApprovalFlowNodeRow(string Id, string Label, double X, double Y);
