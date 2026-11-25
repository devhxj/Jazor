namespace Jero.Core.Service;

public class JobDetailInput
{
    /// <summary>
    /// 作业Id
    /// </summary>
    public string JobId { get; set; } = string.Empty;
}

public class PageJobDetailInput : BasePageInput
{
    /// <summary>
    /// 作业Id
    /// </summary>
    public string JobId { get; set; } = string.Empty;
    /// <summary>
    /// 组名称
    /// </summary>
    public string GroupName { get; set; } = string.Empty;
    /// <summary>
    /// 描述信息
    /// </summary>
    public string Description { get; set; } = string.Empty;
}

public class AddJobDetailInput : SysJobDetail
{
    /// <summary>
    /// 作业Id
    /// </summary>
    [Required(ErrorMessage = "作业Id不能为空"), MinLength(2, ErrorMessage = "作业Id不能少于2个字符")]
    public override string JobId { get; set; } = string.Empty;
}

public class UpdateJobDetailInput : AddJobDetailInput
{
}

public class DeleteJobDetailInput : JobDetailInput
{
}