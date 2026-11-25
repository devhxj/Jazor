namespace Jero.Plugin.SaveLife;

/// <summary>
/// 医线生机用户收藏表
/// </summary>
[SugarTable("sl_favorite", "医线生机用户收藏表")]
[SugarIndex("index_{table}_U", nameof(ExternalUserId), OrderByType.Asc)]
[SugarIndex("index_{table}_O", nameof(ObjectType), OrderByType.Asc, nameof(ObjectId), OrderByType.Asc)]
public class SaveLifeFavorite : SaveLifeEntityBaseDel
{
    /// <summary>
    /// 外部用户 Id
    /// </summary>
    [Required, MaxLength(64)]
    [SugarColumn(ColumnDescription = "外部用户 Id", Length = 64)]
    public string ExternalUserId { get; set; } = string.Empty;

    /// <summary>
    /// 收藏对象类型 trial/knowledge
    /// </summary>
    [Required, MaxLength(32)]
    [SugarColumn(ColumnDescription = "收藏对象类型", Length = 32)]
    public string ObjectType { get; set; } = string.Empty;

    /// <summary>
    /// 收藏对象 Id
    /// </summary>
    [SugarColumn(ColumnDescription = "收藏对象 Id")]
    public long ObjectId { get; set; }
}
