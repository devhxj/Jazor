using ECMAScript;
using ECMAScript.TDesign;
using ECMAScript.VuIcons;
using Microsoft.AspNetCore.Components;
using static ECMAScript.Vue;

namespace JazorAdmin.DemoClient;

[ECMAScriptModule("components/portal-page")]
public partial class PortalPage : ComponentBase, IVueComponent
{
    private bool loading = true;
    private bool authenticated;
    private string? error;
    private DemoSessionView? session;
    private ProtectedOverviewView? protectedOverview;

    protected override void OnInitialized()
        => LoadSession();

    private void LoadSession()
    {
        loading = true;
        error = null;
        PortalApiClient.GetSession().Then(result =>
        {
            loading = false;
            if (!result.Ok || result.Data is null)
            {
                authenticated = false;
                if (!result.Unauthorized)
                    error = result.Error ?? "The downstream session could not be loaded.";
                return;
            }

            authenticated = true;
            session = PortalApiClient.ToSession(result.Data);
            PortalApiClient.GetOverview().Then(ApplyOverview);
        });
    }

    private void ApplyOverview(PortalApiOutcome result)
    {
        if (!result.Ok || result.Data is null)
        {
            error = result.Error ?? "The protected platform API could not be reached.";
            return;
        }

        protectedOverview = PortalApiClient.ToOverview(result.Data);
    }
}
