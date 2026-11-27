using Jero.Web.Application;
using ECMAScript;
using static ECMAScript.Vue;

namespace Jero.Web.Pages.WechatPay;

[ECMAScript]
public interface IWechatPayContentState
{
    WechatPayView? Audit { get; }
    bool AuditDialogVisible { get; set; }
    bool CanListRefund { get; }
    bool CanRefund { get; }
    bool CanSync { get; }
    string CreateAttachment { get; set; }
    string CreateBusinessId { get; set; }
    string CreateDescription { get; set; }
    bool CreateDialogVisible { get; set; }
    string CreateGoodsTag { get; set; }
    void CreateNative();
    string CreateTags { get; set; }
    string CreateTotal { get; set; }
    bool Creating { get; }
    WechatPayView? Detail { get; }
    bool DetailDialogVisible { get; set; }
    string DisplayRowValueForView(VueProps? row, string key);
    string DisplayUserForView(string name, string id);
    string DisplayValueForView(string value);
    string FormatAmount(int fen);
    bool IsSyncing(VueProps? row);
    string NullableAmountForView(int? fen);
    void OpenAudit(VueProps? value);
    void OpenDetail(VueProps? value);
    void OpenQrCode(VueProps? value);
    void OpenRefund(VueProps? value);
    void OpenRefunds(VueProps? value);
    bool QrDialogVisible { get; set; }
    string QrImage { get; }
    bool QrLoading { get; }
    string QrOrderNumber { get; }
    void Refund();
    bool RefundDialogVisible { get; set; }
    bool Refunding { get; }
    bool RefundListDialogVisible { get; set; }
    bool RefundListLoading { get; }
    WechatPayView? RefundOrder { get; }
    string RefundReason { get; set; }
    WechatRefundView[] RefundRows { get; }
    string RowAmount(VueProps? row, string key);
    WechatPayView[] Rows { get; }
    string RowStateTag(VueProps? row);
    string RowStateText(VueProps? row);
    string RowStringForView(VueProps? row, string key);
    void SyncWechat(VueProps? value);
    string TradeStateTag(string state);
    string TradeStateText(string state);
}
