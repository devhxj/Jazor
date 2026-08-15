using JazorAdmin.Features.Sso;

namespace JazorAdmin;

[ECMAScriptModule("components/sso-scope")]
public partial class SsoScopePage : AppComponentBase, IVueContainerComponent
{
    private bool loading = true;
    private string? error;
    private ScopeView[] scopes = [];
    private string? selectedId;
    private string name = string.Empty;
    private string displayName = string.Empty;
    private string description = string.Empty;
    private string resources = string.Empty;
    private bool deleteArmed;

    private bool IsNew => selectedId is null;

    protected override void OnInitialized() => Load();

    private void Load()
    {
        loading = true;
        error = null;
        ApiClient.GetScopes().Then(ApplyScopes);
    }

    private void ApplyScopes(ApiOutcome outcome)
    {
        loading = false;
        if (!outcome.Ok)
        {
            error = outcome.Error ?? L("Unable to load OpenID scopes.", "无法加载 OpenID Scope。");
            return;
        }

        scopes = ApiClient.ToScopes(outcome.Data);
        if (selectedId is not null)
        {
            var selected = scopes.FirstOrDefault(value => value.Id == selectedId);
            if (selected is not null)
            {
                Select(selected);
                return;
            }
        }
        if (scopes.Length > 0)
            Select(scopes[0]);
    }

    private void Select(ScopeView scope)
    {
        selectedId = scope.Id;
        name = scope.Name;
        displayName = scope.DisplayName;
        description = scope.Description ?? string.Empty;
        resources = Join(scope.Resources);
        deleteArmed = false;
    }

    private void NewScope()
    {
        selectedId = null;
        name = string.Empty;
        displayName = string.Empty;
        description = string.Empty;
        resources = string.Empty;
        deleteArmed = false;
        error = null;
    }

    private void Save()
    {
        var resourceValues = Split(resources);
        if (selectedId is null)
        {
            ApiClient.CreateScope(new ScopeCreate(name, displayName, description, resourceValues)).Then(ApplySaved);
        }
        else
        {
            ApiClient.UpdateScope(selectedId, new ScopeUpdate(displayName, description, resourceValues)).Then(ApplySaved);
        }
    }

    private void ApplySaved(ApiOutcome outcome)
    {
        if (!outcome.Ok)
        {
            error = outcome.Error ?? L("Unable to save the OpenID scope.", "无法保存 OpenID Scope。");
            return;
        }
        Load();
    }

    private void DeleteScope()
    {
        if (selectedId is null)
            return;
        if (!deleteArmed)
        {
            deleteArmed = true;
            return;
        }

        ApiClient.DeleteScope(selectedId).Then(outcome =>
        {
            if (!outcome.Ok)
            {
                error = outcome.Error ?? L("Unable to delete the OpenID scope.", "无法删除 OpenID Scope。");
                return;
            }
            NewScope();
            Load();
        });
    }

    private static string[] Split(string value)
        => value.Split([' ', '\r', '\n', ',', ';'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

    private static string Join(string[] values)
        => values.Length == 0 ? string.Empty : string.Join(" ", values);

    private static string Display(string[] values)
        => values.Length == 0 ? "-" : string.Join(", ", values);
}
