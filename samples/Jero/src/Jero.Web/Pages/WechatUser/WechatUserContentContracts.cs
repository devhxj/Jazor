using Jero.Web.Application;
using ECMAScript;
using static ECMAScript.Vue;

namespace Jero.Web.Pages.WechatUser;

[ECMAScript]
public interface IWechatUserContentState
{
    string AvatarSourceForView(VueProps? row);
    bool CanDelete { get; }
    bool CanEdit { get; }
    void Delete(VueProps? value);
    string DeletingId { get; }
    string DisplayRowValueForView(VueProps? row, string key);
    string GenderTagForView(VueProps? row);
    string GenderTextForView(VueProps? row);
    bool HasActionColumn { get; }
    bool IsDeletingRow(VueProps? row);
    void OpenAudit(VueProps? value);
    void OpenEdit(VueProps? value);
    string PlatformLabel(VueProps? row);
    string PlatformTag(VueProps? row);
    WechatUserView[] Rows { get; }
}
