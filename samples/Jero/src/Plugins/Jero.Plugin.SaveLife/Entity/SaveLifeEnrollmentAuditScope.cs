namespace Jero.Plugin.SaveLife;

/// <summary>
/// 医线生机报名审核角色领域配置表。
/// </summary>
[SugarTable("sl_enrollment_audit_scope", "医线生机报名审核角色领域配置表")]
[SugarIndex("uq_{table}_RD", nameof(RoleId), OrderByType.Asc, nameof(DiseaseArea), OrderByType.Asc, true)]
public class SaveLifeEnrollmentAuditScope : SaveLifeEntityBaseDel
{
    /// <summary>
    /// 系统角色 Id。
    /// </summary>
    [SugarColumn(ColumnDescription = "系统角色 Id")]
    public long RoleId { get; set; }

    /// <summary>
    /// 可审核的项目疾病领域。
    /// </summary>
    [Required, MaxLength(64)]
    [SugarColumn(ColumnDescription = "项目疾病领域", Length = 64)]
    public string DiseaseArea { get; set; } = string.Empty;
}
