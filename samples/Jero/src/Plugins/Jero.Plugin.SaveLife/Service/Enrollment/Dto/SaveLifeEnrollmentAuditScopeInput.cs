namespace Jero.Plugin.SaveLife.Service;

public class SaveLifeEnrollmentAuditScopeConfigOutput
{
    public List<string> DiseaseAreas { get; set; } = new();

    public List<SaveLifeEnrollmentAuditRoleScopeOutput> Roles { get; set; } = new();
}

public class SaveLifeEnrollmentAuditRoleScopeOutput
{
    public long RoleId { get; set; }

    public string RoleName { get; set; } = string.Empty;

    public string? RoleCode { get; set; }

    public List<string> DiseaseAreas { get; set; } = new();
}

public class SaveSaveLifeEnrollmentAuditRoleScopeInput
{
    [Range(1, long.MaxValue, ErrorMessage = "角色Id不正确")]
    public long RoleId { get; set; }

    /// <summary>
    /// 空集合表示移除该角色的全部审核领域。
    /// </summary>
    public List<string> DiseaseAreas { get; set; } = new();
}

/// <summary>
/// 当前后台用户可处理的报名领域范围。
/// </summary>
public sealed class SaveLifeEnrollmentAuditAccessScope
{
    public static SaveLifeEnrollmentAuditAccessScope Unrestricted { get; } = new(true, []);

    public static SaveLifeEnrollmentAuditAccessScope Empty { get; } = new(false, []);

    public SaveLifeEnrollmentAuditAccessScope(bool isUnrestricted, IReadOnlyCollection<string> diseaseAreas)
    {
        IsUnrestricted = isUnrestricted;
        DiseaseAreas = diseaseAreas;
    }

    public bool IsUnrestricted { get; }

    public IReadOnlyCollection<string> DiseaseAreas { get; }

    public bool Includes(string? diseaseArea)
    {
        if (IsUnrestricted)
            return true;

        var normalized = diseaseArea?.Trim();
        return !string.IsNullOrWhiteSpace(normalized) &&
            DiseaseAreas.Contains(normalized, StringComparer.OrdinalIgnoreCase);
    }
}

public interface ISaveLifeEnrollmentAuditScopeAccessService
{
    Task<SaveLifeEnrollmentAuditAccessScope> GetCurrentAccessScopeAsync();
}
