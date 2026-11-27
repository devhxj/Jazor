namespace Jero.Application.Modules.GoView.Service;

/// <summary>
/// GoView 新增项目
/// </summary>
public class GoViewProCreateInput
{
    /// <summary>
    /// 项目名称
    /// </summary>
    public string ProjectName { get; set; } = string.Empty;

    /// <summary>
    /// 项目备注
    /// </summary>
    public string Remarks { get; set; } = string.Empty;

    /// <summary>
    /// 预览图片url
    /// </summary>
    public string IndexImage { get; set; } = string.Empty;
}

/// <summary>
/// GoView 编辑项目
/// </summary>
public class GoViewProEditInput
{
    /// <summary>
    /// 项目Id
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// 项目名称
    /// </summary>
    public string ProjectName { get; set; } = string.Empty;

    /// <summary>
    /// 预览图片url
    /// </summary>
    public string IndexImage { get; set; } = string.Empty;
}

/// <summary>
/// GoView 修改项目发布状态
/// </summary>
public class GoViewProPublishInput
{
    /// <summary>
    /// 项目Id
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// 项目状态
    /// </summary>
    public GoViewProStateEnum StateEnum { get; set; }
}

/// <summary>
/// GoView 保存项目数据
/// </summary>
public class GoViewProSaveDataInput
{
    /// <summary>
    /// 项目Id
    /// </summary>
    public long ProjectId { get; set; }

    /// <summary>
    /// 项目内容
    /// </summary>
    public string Content { get; set; } = string.Empty;
}
