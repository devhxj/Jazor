// Loads and updates resource-operation grants for roles in the selected organization.
// 加载并更新当前组织中角色的资源操作授权集合。
using JazorAdmin.Features.Organizations;

namespace JazorAdmin;

[String]
public enum AccessControlView
{
    [Description("@#roles")]
    Roles,

    [Description("@#resources")]
    Resources
}

[ECMAScriptModule("components/access-control.mjs")]
public partial class AccessControlPage : AppComponentBase, IVueContainerComponent
{
    [Parameter]
    public string? OrganizationId { get; set; }

    [Parameter]
    public AccessControlView View { get; set; }

    private bool loading = true;
    private string? error;
    private OrganizationRoleResponse[] roles = [];
    private ResourceOperationResponse[] resourceOperations = [];
    private string? selectedRoleId;
    private string[] grants = [];
    private string roleCode = string.Empty;
    private string roleDisplayName = string.Empty;

    protected override void OnParametersSet()
    {
        // Organization and view are router-fed props. OnParametersSet covers both first render and
        // subsequent route changes. OrganizationId 与 View 由路由传入，此生命周期覆盖首帧和后续路由变更。
        Load();
    }

    private OrganizationRoleResponse? SelectedRole
        => roles.FirstOrDefault(role => role.Id == selectedRoleId);

    private void Load()
    {
        if (OrganizationId is null)
        {
            loading = false;
            return;
        }

        loading = true;
        error = null;
        ApiClient.GetAuthorizationResources(OrganizationId).Then(ApplyResourceOperations);
        if (View == AccessControlView.Roles)
            ApiClient.GetRoles(OrganizationId).Then(ApplyRoles);
    }

    private void ApplyResourceOperations(ApiOutcome outcome)
    {
        loading = false;
        if (!outcome.Ok)
        {
            error = outcome.Error ?? "Unable to load resource operations.";
            return;
        }

        resourceOperations = ApiClient.ToResourceOperations(outcome.Data);
    }

    private void ApplyRoles(ApiOutcome outcome)
    {
        if (!outcome.Ok)
        {
            error = outcome.Error ?? "Unable to load organization roles.";
            return;
        }

        roles = ApiClient.ToRoles(outcome.Data);
        if (selectedRoleId is null && roles.Length > 0)
            SelectRole(roles[0]);
    }

    private void SelectRole(OrganizationRoleResponse role)
    {
        selectedRoleId = role.Id;
        if (OrganizationId is null)
            return;

        ApiClient.GetRoleGrants(OrganizationId, role.Id).Then(outcome =>
        {
            if (!outcome.Ok)
            {
                error = outcome.Error ?? "Unable to load role grants.";
                return;
            }

            grants = ApiClient.ToGrantKeys(outcome.Data);
        });
    }

    private void CreateRole()
    {
        if (OrganizationId is null || Text.Normalize(roleCode) is null || Text.Normalize(roleDisplayName) is null)
            return;

        ApiClient.CreateRole(OrganizationId, roleCode, roleDisplayName).Then(outcome =>
        {
            if (!outcome.Ok)
            {
                error = outcome.Error ?? "Unable to create the organization role.";
                return;
            }

            roleCode = string.Empty;
            roleDisplayName = string.Empty;
            Load();
        });
    }

    private bool HasGrant(ResourceOperationResponse operation)
        => grants.Contains(GetGrantKey(operation));

    private void ToggleGrant(ResourceOperationResponse operation)
    {
        var key = GetGrantKey(operation);
        var selected = grants.Where(grant => grant != key).ToList();
        if (!grants.Contains(key))
            selected.Add(key);
        grants = selected.ToArray();
    }

    private void SaveRoleGrants()
    {
        if (OrganizationId is null || selectedRoleId is null)
            return;

        ApiClient.ReplaceRoleGrants(OrganizationId, selectedRoleId, grants).Then(outcome =>
        {
            if (!outcome.Ok)
            {
                error = outcome.Error ?? "Unable to update role grants.";
                return;
            }

            SelectRole(SelectedRole!);
        });
    }

    private static string GetGrantKey(ResourceOperationResponse operation)
        => operation.Resource + ":" + operation.Operation;
}
