// Loads and mutates organization-scoped structure and membership state through the typed API bridge.
// 通过强类型 API bridge 加载并修改组织范围内的架构与成员状态。
using JazorAdmin.Frontend;
using JazorAdmin.Features.Organizations;

namespace JazorAdmin;

[String]
public enum OrganizationView
{
    [Description("@#structure")]
    Structure,

    [Description("@#members")]
    Members
}

[ECMAScriptModule("components/organization.mjs")]
public partial class OrganizationPage : AppComponentBase, IVueContainerComponent
{
    [Parameter]
    public string? OrganizationId { get; set; }

    [Parameter]
    public OrganizationView View { get; set; }

    private bool loading = true;
    private string? error;
    private OrganizationDetailResponse? organization;
    private OrganizationMemberResponse[] members = [];
    private OrganizationRoleResponse[] roles = [];
    private string childCode = string.Empty;
    private string childDisplayName = string.Empty;
    private string memberEmail = string.Empty;
    private string newMemberRoleId = string.Empty;
    private string? editingMembershipId;
    private string[] editingMemberRoleIds = [];

    protected override void OnParametersSet()
    {
        // RazorVue runs this for the initial props and its props watcher. Keep route-driven data
        // refresh in the Razor lifecycle instead of constructor-only Vue registrations. RazorVue 会在初始参数和参数变化时调用此方法，不能在构造函数中注册 Vue watch。
        Load();
    }

    private void Load()
    {
        if (OrganizationId is null)
        {
            loading = false;
            return;
        }

        loading = true;
        error = null;
        if (View == OrganizationView.Structure)
        {
            JazorAdminApiClient.GetOrganization(OrganizationId).Then(ApplyOrganization);
            return;
        }

        JazorAdminApiClient.GetMembers(OrganizationId).Then(ApplyMembers);
        JazorAdminApiClient.GetRoles(OrganizationId).Then(ApplyRoles);
    }

    private void ApplyOrganization(AdminApiOutcome outcome)
    {
        loading = false;
        if (!outcome.Ok || outcome.Data is null)
        {
            error = outcome.Error ?? "Unable to load the organization.";
            return;
        }

        organization = JazorAdminApiClient.ToOrganizationDetail(outcome.Data);
    }

    private void ApplyMembers(AdminApiOutcome outcome)
    {
        loading = false;
        if (!outcome.Ok)
        {
            error = outcome.Error ?? "Unable to load organization members.";
            return;
        }

        members = JazorAdminApiClient.ToMembers(outcome.Data);
    }

    private void ApplyRoles(AdminApiOutcome outcome)
    {
        if (!outcome.Ok)
        {
            error = outcome.Error ?? "Unable to load organization roles.";
            return;
        }

        roles = JazorAdminApiClient.ToRoles(outcome.Data);
    }

    private void CreateChild()
    {
        if (OrganizationId is null || Text.Normalize(childCode) is null || Text.Normalize(childDisplayName) is null)
            return;

        JazorAdminApiClient.CreateChildOrganization(OrganizationId, childCode, childDisplayName).Then(outcome =>
        {
            if (!outcome.Ok)
            {
                error = outcome.Error ?? "Unable to create the child organization.";
                return;
            }

            childCode = string.Empty;
            childDisplayName = string.Empty;
            Load();
        });
    }

    private void CreateMember()
    {
        if (OrganizationId is null || Text.Normalize(memberEmail) is null)
            return;

        var roleIds = string.IsNullOrWhiteSpace(newMemberRoleId) ? new string[0] : new[] { newMemberRoleId };
        JazorAdminApiClient.CreateMember(OrganizationId, memberEmail, roleIds).Then(outcome =>
        {
            if (!outcome.Ok)
            {
                error = outcome.Error ?? "Unable to add the organization member.";
                return;
            }

            memberEmail = string.Empty;
            newMemberRoleId = string.Empty;
            Load();
        });
    }

    private void SelectMember(OrganizationMemberResponse member)
    {
        editingMembershipId = member.MembershipId;
        editingMemberRoleIds = member.Roles.Select(role => role.Id).ToArray();
    }

    private bool HasEditedMemberRole(string roleId)
        => editingMemberRoleIds.Contains(roleId);

    private void ToggleEditedMemberRole(string roleId)
    {
        var selected = editingMemberRoleIds.Where(id => id != roleId).ToList();
        if (!editingMemberRoleIds.Contains(roleId))
            selected.Add(roleId);
        editingMemberRoleIds = selected.ToArray();
    }

    private void SaveMemberRoles()
    {
        if (OrganizationId is null || editingMembershipId is null)
            return;

        JazorAdminApiClient.ReplaceMemberRoles(OrganizationId, editingMembershipId, editingMemberRoleIds).Then(outcome =>
        {
            if (!outcome.Ok)
            {
                error = outcome.Error ?? "Unable to update member roles.";
                return;
            }

            Load();
        });
    }

    private static string GetRoleNames(OrganizationRoleResponse[] values)
        => values.Length == 0 ? "No roles" : string.Join(", ", values.Select(role => role.DisplayName));
}
