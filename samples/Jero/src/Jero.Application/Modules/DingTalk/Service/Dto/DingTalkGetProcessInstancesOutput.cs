namespace Jero.Application.Modules.DingTalk;

public class DingTalkGetProcessInstancesOutput
{
    public ResultData Result { get; set; } = new();
    public bool Success { get; set; }
}

public class OperationRecord
{
    public DateTime? Date { get; set; }
    public string Result { get; set; } = string.Empty;
    public List<object> Images { get; set; } = []; // 图片可能是字符串 URL 或对象
    public string ShowName { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
}

// 表格行中的子项（用于 TableField 的解析）
public class TableRowItem
{
    public string BizAlias { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public string Key { get; set; } = string.Empty;
    public bool Mask { get; set; }
}

// 完整的一行表格数据
public class TableRow
{
    public List<TableRowItem> RowValue { get; set; } = [];
    public string RowNumber { get; set; } = string.Empty;
}

public class TaskItem
{
    public string Result { get; set; } = string.Empty;
    public string ActivityId { get; set; } = string.Empty;
    public string PcUrl { get; set; } = string.Empty;
    public DateTime? CreateTime { get; set; }
    public string MobileUrl { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public long TaskId { get; set; }
    public string Status { get; set; } = string.Empty;
}

public class ResultData
{
    public List<string> AttachedProcessInstanceIds { get; set; } = [];
    public string BusinessId { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string OriginatorDeptId { get; set; } = string.Empty;
    public List<OperationRecord> OperationRecords { get; set; } = [];
    public List<FormComponentValue> FormComponentValues { get; set; } = [];

    /// <summary>
    /// 审批结果 agree：同意 refuse：拒绝
    /// </summary>
    public string Result { get; set; } = string.Empty;

    public string BizAction { get; set; } = string.Empty;
    public DateTime? CreateTime { get; set; }
    public string OriginatorUserId { get; set; } = string.Empty;
    public List<TaskItem> Tasks { get; set; } = [];
    public string OriginatorDeptName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
}
