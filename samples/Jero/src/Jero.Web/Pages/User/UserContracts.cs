using System;
using Jero.Web.Pages.Components;
using static ECMAScript.Vue;

namespace Jero.Web.Pages.User;

public sealed record UserTableRow(
    string Id,
    string Account,
    string RealName,
    string Phone,
    string RoleName,
    string OrgName,
    string PosName,
    int OrderNo,
    string UpdateTime,
    string AvatarSource,
    string AccountTypeTag,
    string AccountTypeLabel,
    string StatusTag,
    string StatusText,
    string StatusConfirm,
    string StatusAction,
    bool IsChangingStatus,
    bool IsResetting,
    bool IsUnlocking,
    bool IsDeleting);

public sealed record UserTreeRow(string Id, string Name, string Path, string TenantId, int Depth, bool Disabled);
public sealed record UserRoleRow(string Id, string Name);
public sealed record UserExtOrganizationRow(string OrgId, string PosId);

public sealed record UserTableModel
{
    public required UserTableRow[] Rows { get; init; }
    public required bool CanSetStatus { get; init; }
    public required bool CanDetail { get; init; }
    public required bool HasActionColumn { get; init; }
    public required bool CanEdit { get; init; }
    public required bool CanGrantRole { get; init; }
    public required bool CanAdd { get; init; }
    public required bool CanResetPassword { get; init; }
    public required bool CanUnlockLogin { get; init; }
    public required bool CanDelete { get; init; }
    public required string StatusLoadingId { get; init; }
    public required string ResetLoadingId { get; init; }
    public required string UnlockLoadingId { get; init; }
    public required string DeletingId { get; init; }
    public required Action<string> SetStatus { get; init; }
    public required Action<string> OpenDetail { get; init; }
    public required Action<string> OpenEdit { get; init; }
    public required Action<string> OpenGrantRoles { get; init; }
    public required Action<string> OpenCopy { get; init; }
    public required Action<string> ResetPassword { get; init; }
    public required Action<string> UnlockLogin { get; init; }
    public required Action<string> Delete { get; init; }
}

public sealed record UserPageModel
{
    public required UserTableModel Table { get; init; }
    public required UserDraft Draft { get; init; }
    public required bool TenantLoading { get; init; }
    public required ConfigurationOption[] TenantOptions { get; init; }
    public required ConfigurationOption[] EditorOrganizationOptions { get; init; }
    public required ConfigurationOption[] PositionOptions { get; init; }
    public required ConfigurationOption[] LanguageOptions { get; init; }
    public required ConfigurationOption[] NationOptions { get; init; }
    public required ConfigurationOption[] AccountTypeOptions { get; init; }
    public required ConfigurationOption[] CardTypeOptions { get; init; }
    public required ConfigurationOption[] GenderOptions { get; init; }
    public required ConfigurationOption[] CultureLevelOptions { get; init; }
    public required Action<string> TenantChanged { get; init; }
    public required string TenantId { get; init; }
    public required Action<string> SetDraftAccountType { get; init; }
    public required string DraftAccountType { get; init; }
    public required Action<string> SetDraftStatus { get; init; }
    public required ConfigurationOption[] StatusOptions { get; init; }
    public required string DraftStatus { get; init; }
    public required Action<int, string?> ChangeExtOrganization { get; init; }
    public required Action<int, string?> ChangeExtPosition { get; init; }
    public required Action<string> SetDraftCardType { get; init; }
    public required string DraftCardType { get; init; }
    public required Action<string> SetDraftSex { get; init; }
    public required string DraftSex { get; init; }
    public required Action<string> SetDraftCultureLevel { get; init; }
    public required string DraftCultureLevel { get; init; }
    public required Action<double?> SetDraftOrderNo { get; init; }
    public required Action<double?> SetDraftAge { get; init; }
    public required Action<string?> SetAccount { get; init; }
    public required Action<string?> SetRealName { get; init; }
    public required Action<string?> SetPosName { get; init; }
    public required Action<string?> SetPhone { get; init; }
    public required Action<bool> SetEditDialogVisible { get; init; }
    public required Action<bool> SetDetailDialogVisible { get; init; }
    public required Action<bool> SetGrantDialogVisible { get; init; }
    public required Action<bool> SetPasswordDialogVisible { get; init; }
    public required Action CloseEditDialogVisible { get; init; }
    public required Action CloseGrantDialogVisible { get; init; }
    public required Action ClosePasswordDialogVisible { get; init; }
    public required Action<int> RemoveExtOrganization { get; init; }
    public required Action Search { get; init; }
    public required Action ResetSearch { get; init; }
    public required Action OpenCreate { get; init; }
    public required Action Refresh { get; init; }
    public required Action AddExtOrganization { get; init; }
    public required Action Save { get; init; }
    public required Action SaveGrantRoles { get; init; }
    public required Action DismissError { get; init; }
    public required Action<double> ChangePage { get; init; }
    public required Action<double> ChangePageSize { get; init; }
    public required bool Editing { get; init; }
    public required string DetailAccount { get; init; }
    public required string DetailRealName { get; init; }
    public required string DetailNickName { get; init; }
    public required string DetailPhone { get; init; }
    public required string DetailEmail { get; init; }
    public required string DetailRoleName { get; init; }
    public required string DetailOrgName { get; init; }
    public required string DetailPosName { get; init; }
    public required string DetailJobNum { get; init; }
    public required string DetailLangCode { get; init; }
    public required string DetailLastLoginIp { get; init; }
    public required string DetailLastLoginAddress { get; init; }
    public required string DetailLastLoginTime { get; init; }
    public required string DetailLastLoginDevice { get; init; }
    public required string DetailCreateTime { get; init; }
    public required string DetailUpdateTime { get; init; }
    public required string DetailRemark { get; init; }
    public required string DetailCreateUserName { get; init; }
    public required string DetailUpdateUserName { get; init; }
    public required string DetailAccountType { get; init; }
    public required string DetailStatus { get; init; }
    public required string GrantAccount { get; init; }
    public required string GrantRealName { get; init; }
    public required string? Error { get; init; }
    public required string Account { get; init; }
    public required string RealName { get; init; }
    public required string PosName { get; init; }
    public required string Phone { get; init; }
    public required bool Loading { get; init; }
    public required bool OrgLoading { get; init; }
    public required string SelectedOrgId { get; init; }
    public required string SelectedOrgName { get; init; }
    public required int CurrentPage { get; init; }
    public required int PageSize { get; init; }
    public required int Total { get; init; }
    public required bool EditDialogVisible { get; init; }
    public required string EditDialogTitle { get; init; }
    public required string EditTab { get; init; }
    public required bool Saving { get; init; }
    public required bool DependencyLoading { get; init; }
    public required bool DependencyFailed { get; init; }
    public required string[] EditRoleIds { get; init; }
    public required string[] GrantRoleIds { get; init; }
    public required bool DetailDialogVisible { get; init; }
    public required bool GrantDialogVisible { get; init; }
    public required bool GrantLoading { get; init; }
    public required bool Granting { get; init; }
    public required bool GrantFailed { get; init; }
    public required bool PasswordDialogVisible { get; init; }
    public required string ResetPasswordValue { get; init; }
    public required bool CanEditDraftRoles { get; init; }
    public required bool IsSuperAdmin { get; init; }
    public required UserTreeRow[] OrgTreeItems { get; init; }
    public required UserRoleRow[] RoleOptions { get; init; }
    public required UserRoleRow[] GrantRoleOptions { get; init; }
    public required UserExtOrganizationRow[] EditExtOrganizations { get; init; }
    public required double[] PageSizes { get; init; }
    public required Action<string, string, string> SelectOrganization { get; init; }
    public required Action<string> SelectEditTab { get; init; }
    public required Action<string> ToggleEditRole { get; init; }
    public required Action<string> ToggleGrantRole { get; init; }
}

public sealed record UserDraft : VueProps
{
    public string Id { get; set; } = string.Empty;
    public string TenantId { get; set; } = string.Empty;
    public string OrgId { get; set; } = string.Empty;
    public string Account { get; set; } = string.Empty;
    public string RealName { get; set; } = string.Empty;
    public string NickName { get; set; } = string.Empty;
    public int Sex { get; set; } = 1;
    public int Age { get; set; }
    public string Birthday { get; set; } = string.Empty;
    public string Nation { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public int CardType { get; set; }
    public string IdCardNum { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public int CultureLevel { get; set; } = 5;
    public string PoliticalOutlook { get; set; } = string.Empty;
    public string College { get; set; } = string.Empty;
    public string OfficePhone { get; set; } = string.Empty;
    public string EmergencyContact { get; set; } = string.Empty;
    public string EmergencyPhone { get; set; } = string.Empty;
    public string EmergencyAddress { get; set; } = string.Empty;
    public string Introduction { get; set; } = string.Empty;
    public int OrderNo { get; set; } = 100;
    public int Status { get; set; } = 1;
    public string Remark { get; set; } = string.Empty;
    public int AccountType { get; set; } = 777;
    public string PosId { get; set; } = string.Empty;
    public string JobNum { get; set; } = string.Empty;
    public string JoinDate { get; set; } = string.Empty;
    public string LangCode { get; set; } = "zh-CN";
    public string Homepage { get; set; } = string.Empty;
    public string DomainAccount { get; set; } = string.Empty;
}
