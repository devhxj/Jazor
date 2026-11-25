namespace Jero.Plugin.SaveLife;

/// <summary>
/// 医线生机账号注销处理记录。
/// 仅保留不含 OpenID、姓名或联系方式的审计信息。
/// </summary>
[SugarTable("sl_account_deletion_record", "医线生机账号注销处理记录")]
[SugarIndex("index_{table}_P", nameof(DeletedProfileId), OrderByType.Asc)]
public class SaveLifeAccountDeletionRecord : SaveLifeEntityBaseDel
{
    /// <summary>
    /// 已删除的用户档案 Id。
    /// </summary>
    [SugarColumn(ColumnDescription = "已删除用户档案 Id")]
    public long DeletedProfileId { get; set; }

    /// <summary>
    /// 注销申请时间。
    /// </summary>
    [SugarColumn(ColumnDescription = "注销申请时间")]
    public DateTime RequestedTime { get; set; }

    /// <summary>
    /// 注销处理完成时间。
    /// </summary>
    [SugarColumn(ColumnDescription = "注销处理完成时间")]
    public DateTime ProcessedTime { get; set; }

    /// <summary>
    /// 处理状态。
    /// </summary>
    [Required, MaxLength(32)]
    [SugarColumn(ColumnDescription = "处理状态", Length = 32)]
    public string Status { get; set; } = "completed";
}
