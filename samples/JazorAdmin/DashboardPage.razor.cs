// Supplies the real session-scoped data used by the administration overview.
// 为管理首页提供当前会话范围内的真实数据，不引入任何项目或报名领域模型。
using JazorAdmin.Features.Identity;

namespace JazorAdmin;

[ECMAScriptModule("./components/dashboard")]
public partial class DashboardPage : AppComponentBase, IVueContainerComponent
{
    [Parameter]
    public SessionResponse? Session { get; set; }

    [Parameter]
    public OrganizationSummary? SelectedOrganization { get; set; }
}
