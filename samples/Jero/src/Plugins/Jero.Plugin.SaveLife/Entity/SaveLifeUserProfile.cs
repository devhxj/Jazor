namespace Jero.Plugin.SaveLife;

/// <summary>
/// 医线生机用户健康档案表
/// </summary>
[SugarTable("sl_user_profile", "医线生机用户健康档案表")]
[SugarIndex("index_{table}_P", nameof(Phone), OrderByType.Asc)]
public class SaveLifeUserProfile : SaveLifeEntityBaseDel
{
    /// <summary>
    /// 外部用户 Id
    /// </summary>
    [MaxLength(64)]
    [SugarColumn(ColumnDescription = "外部用户 Id", Length = 64)]
    public string? ExternalUserId { get; set; }

    /// <summary>
    /// 真实姓名
    /// </summary>
    [MaxLength(64)]
    [SugarColumn(ColumnDescription = "真实姓名", Length = 64)]
    public string? RealName { get; set; }

    /// <summary>
    /// 头像文件 Id
    /// </summary>
    [SugarColumn(ColumnDescription = "头像文件 Id")]
    public long? AvatarFileId { get; set; }

    /// <summary>
    /// 头像文件地址
    /// </summary>
    [MaxLength(1024)]
    [SugarColumn(ColumnDescription = "头像文件地址", Length = 1024)]
    public string? AvatarUrl { get; set; }

    /// <summary>
    /// 性别
    /// </summary>
    [MaxLength(16)]
    [SugarColumn(ColumnDescription = "性别", Length = 16)]
    public string? Gender { get; set; }

    /// <summary>
    /// 出生日期
    /// </summary>
    [SugarColumn(ColumnDescription = "出生日期")]
    public DateTime? BirthDate { get; set; }

    /// <summary>
    /// 手机号
    /// </summary>
    [MaxLength(32)]
    [SugarColumn(ColumnDescription = "手机号", Length = 32)]
    public string? Phone { get; set; }

    /// <summary>
    /// 邮箱
    /// </summary>
    [MaxLength(128)]
    [SugarColumn(ColumnDescription = "邮箱", Length = 128)]
    public string? Email { get; set; }

    /// <summary>
    /// 地址
    /// </summary>
    [MaxLength(256)]
    [SugarColumn(ColumnDescription = "地址", Length = 256)]
    public string? Address { get; set; }

    /// <summary>
    /// 民族
    /// </summary>
    [MaxLength(32)]
    [SugarColumn(ColumnDescription = "民族", Length = 32)]
    public string? Ethnicity { get; set; }

    /// <summary>
    /// 紧急联系人
    /// </summary>
    [MaxLength(64)]
    [SugarColumn(ColumnDescription = "紧急联系人", Length = 64)]
    public string? EmergencyContact { get; set; }

    /// <summary>
    /// 紧急联系电话
    /// </summary>
    [MaxLength(32)]
    [SugarColumn(ColumnDescription = "紧急联系电话", Length = 32)]
    public string? EmergencyPhone { get; set; }

    /// <summary>
    /// 既往病史
    /// </summary>
    [SugarColumn(ColumnDescription = "既往病史", ColumnDataType = StaticConfig.CodeFirst_BigString)]
    public string? MedicalHistory { get; set; }

    /// <summary>
    /// 过敏史
    /// </summary>
    [SugarColumn(ColumnDescription = "过敏史", ColumnDataType = StaticConfig.CodeFirst_BigString)]
    public string? Allergies { get; set; }
}
