namespace Jero.Web.App.ApprovalFlow;

public sealed record ApprovalFlowTableRow(string id, string name, string code, string remark,
    string status, string statusTag, string formRoute, string flowSummary);
public sealed record ApprovalFlowNodeRow(string id, string label, double x, double y);
