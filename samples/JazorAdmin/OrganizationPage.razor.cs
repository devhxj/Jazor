// Loads and mutates organization-scoped structure and membership state through the typed API bridge.
// 通过强类型 API bridge 加载并修改组织范围内的架构与成员状态。
using ECMAScript.TDesign;
using JazorAdmin.Features.Organizations;
using Microsoft.AspNetCore.Components;

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

    // 成员表列：成员列组合名称与邮箱，操作列进入角色编辑。
    private TPrimaryTableCol<OrganizationMemberResponse>[] MemberColumns =>
    [
        new() { Title = (TPrimaryTableColTitle<OrganizationMemberResponse>)L("Member", "成员"), Cell = (TPrimaryTableColCell<OrganizationMemberResponse>)((RenderFragment<TPrimaryTableCellParams<OrganizationMemberResponse>>)(context => builder =>
            {
        builder.OpenElement(0, "div");
        builder.OpenElement(1, "strong");
        builder.AddContent(2, context.Row.DisplayName);
        builder.CloseElement();
        builder.OpenElement(3, "span");
        builder.AddContent(4, context.Row.Email);
        builder.CloseElement();
        builder.CloseElement();
            })) },
        new() { Title = (TPrimaryTableColTitle<OrganizationMemberResponse>)L("Roles", "角色"), Cell = (TPrimaryTableColCell<OrganizationMemberResponse>)((RenderFragment<TPrimaryTableCellParams<OrganizationMemberResponse>>)(context => builder =>
            {
        builder.AddContent(0, GetRoleNames(context.Row.Roles));
            })) },
        new() { Title = (TPrimaryTableColTitle<OrganizationMemberResponse>)L("Actions", "操作"), Cell = (TPrimaryTableColCell<OrganizationMemberResponse>)((RenderFragment<TPrimaryTableCellParams<OrganizationMemberResponse>>)(context => builder =>
            {
        builder.OpenComponent<TButton>(0);
        builder.AddComponentParameter(1, nameof(TButton.Variant), TButtonVariantValue.Text);
        builder.AddComponentParameter(2, nameof(TButton.Size), TSizeEnum.Small);
        builder.AddComponentParameter(3, "data-organization-command", "edit-roles");
        builder.AddComponentParameter(4, nameof(TButton.OnClick),
            EventCallback.Factory.Create(this, () => SelectMember(context.Row)));
        builder.AddComponentParameter(5, nameof(TContentComponentBase.ChildContent),
            (RenderFragment)(child => child.AddContent(0, L("Edit roles", "编辑角色"))));
        builder.CloseComponent();
            })) }
    ];

    private TTableRowClassNameValue<OrganizationMemberResponse> SelectedMemberRowClassName
        => (TTableRowClassNameValueOption2<OrganizationMemberResponse>)SelectedMemberRowClass;

    private TClassName SelectedMemberRowClass(TRowClassNameParams<OrganizationMemberResponse> parameters)
        => parameters.Row.MembershipId == editingMembershipId ? (TClassName)"ja-table-row-selected" : (TClassName)string.Empty;

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
            ApiClient.GetOrganization(OrganizationId).Then(ApplyOrganization);
            return;
        }

        ApiClient.GetMembers(OrganizationId).Then(ApplyMembers);
        ApiClient.GetRoles(OrganizationId).Then(ApplyRoles);
    }

    private void ApplyOrganization(ApiOutcome outcome)
    {
        loading = false;
        if (!outcome.Ok || outcome.Data is null)
        {
            error = outcome.Error ?? L("Unable to load the organization.", "无法加载该组织。");
            return;
        }

        organization = ApiClient.ToOrganizationDetail(outcome.Data);
    }

    private void ApplyMembers(ApiOutcome outcome)
    {
        loading = false;
        if (!outcome.Ok)
        {
            error = outcome.Error ?? L("Unable to load organization members.", "无法加载组织成员。");
            return;
        }

        members = ApiClient.ToMembers(outcome.Data);
    }

    private void ApplyRoles(ApiOutcome outcome)
    {
        if (!outcome.Ok)
        {
            error = outcome.Error ?? L("Unable to load organization roles.", "无法加载组织角色。");
            return;
        }

        roles = ApiClient.ToRoles(outcome.Data);
    }

    private void CreateChild()
    {
        if (OrganizationId is null || Text.Normalize(childCode) is null || Text.Normalize(childDisplayName) is null)
            return;

        ApiClient.CreateChildOrganization(OrganizationId, childCode, childDisplayName).Then(outcome =>
        {
            if (!outcome.Ok)
            {
                error = outcome.Error ?? L("Unable to create the child organization.", "无法创建下级组织。");
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
        ApiClient.CreateMember(OrganizationId, memberEmail, roleIds).Then(outcome =>
        {
            if (!outcome.Ok)
            {
                error = outcome.Error ?? L("Unable to add the organization member.", "无法添加组织成员。");
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

        ApiClient.ReplaceMemberRoles(OrganizationId, editingMembershipId, editingMemberRoleIds).Then(outcome =>
        {
            if (!outcome.Ok)
            {
                error = outcome.Error ?? L("Unable to update member roles.", "无法更新成员角色。");
                return;
            }

            Load();
        });
    }

    private string GetRoleNames(OrganizationRoleResponse[] values)
        => values.Length == 0 ? L("No roles", "暂无角色") : string.Join(", ", values.Select(role => role.DisplayName));
}
