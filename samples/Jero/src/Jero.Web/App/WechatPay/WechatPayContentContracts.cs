using ECMAScript;
using static ECMAScript.Vue;

namespace Jero.Web.App.WechatPay;

[ECMAScript]
public interface IWechatPayContentState
{
    WechatPayView? audit { get; }
    bool auditDialogVisible { get; set; }
    bool CanListRefund { get; }
    bool CanRefund { get; }
    bool CanSync { get; }
    string createAttachment { get; set; }
    string createBusinessId { get; set; }
    string createDescription { get; set; }
    bool createDialogVisible { get; set; }
    string createGoodsTag { get; set; }
    void CreateNative();
    string createTags { get; set; }
    string createTotal { get; set; }
    bool creating { get; }
    WechatPayView? detail { get; }
    bool detailDialogVisible { get; set; }
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
    bool qrDialogVisible { get; set; }
    string qrImage { get; }
    bool qrLoading { get; }
    string qrOrderNumber { get; }
    void Refund();
    bool refundDialogVisible { get; set; }
    bool refunding { get; }
    bool refundListDialogVisible { get; set; }
    bool refundListLoading { get; }
    WechatPayView? refundOrder { get; }
    string refundReason { get; set; }
    WechatRefundView[] refundRows { get; }
    string RowAmount(VueProps? row, string key);
    WechatPayView[] rows { get; }
    string RowStateTag(VueProps? row);
    string RowStateText(VueProps? row);
    string RowStringForView(VueProps? row, string key);
    void SyncWechat(VueProps? value);
    string TradeStateTag(string state);
    string TradeStateText(string state);
}

public sealed record WechatPayView : VueProps
{
    public string id { get; init; } = string.Empty;
    public string merchantId { get; init; } = string.Empty;
    public string appId { get; init; } = string.Empty;
    public string subMerchantId { get; init; } = string.Empty;
    public string subAppId { get; init; } = string.Empty;
    public string outTradeNumber { get; init; } = string.Empty;
    public string transactionId { get; init; } = string.Empty;
    public string tradeType { get; init; } = string.Empty;
    public string tradeState { get; init; } = string.Empty;
    public string tradeStateDescription { get; init; } = string.Empty;
    public string bankType { get; init; } = string.Empty;
    public int total { get; init; }
    public int? payerTotal { get; init; }
    public string successTime { get; init; } = string.Empty;
    public string expireTime { get; init; } = string.Empty;
    public string description { get; init; } = string.Empty;
    public string scene { get; init; } = string.Empty;
    public string attachment { get; init; } = string.Empty;
    public string goodsTag { get; init; } = string.Empty;
    public string settlement { get; init; } = string.Empty;
    public string notifyUrl { get; init; } = string.Empty;
    public string remark { get; init; } = string.Empty;
    public string openId { get; init; } = string.Empty;
    public string subOpenId { get; init; } = string.Empty;
    public string tags { get; init; } = string.Empty;
    public string businessId { get; init; } = string.Empty;
    public string qrcodeContent { get; init; } = string.Empty;
    public string createTime { get; init; } = string.Empty;
    public string createUserId { get; init; } = string.Empty;
    public string createUserName { get; init; } = string.Empty;
    public string updateTime { get; init; } = string.Empty;
    public string updateUserId { get; init; } = string.Empty;
    public string updateUserName { get; init; } = string.Empty;
}

public sealed record WechatRefundView : VueProps
{
    public string id { get; init; } = string.Empty;
    public string wechatPayId { get; init; } = string.Empty;
    public string outRefundNumber { get; init; } = string.Empty;
    public string transactionId { get; init; } = string.Empty;
    public string reason { get; init; } = string.Empty;
    public string channel { get; init; } = string.Empty;
    public string userReceivedAccount { get; init; } = string.Empty;
    public string tradeState { get; init; } = string.Empty;
    public string tradeStateDescription { get; init; } = string.Empty;
    public int refund { get; init; }
    public string successTime { get; init; } = string.Empty;
    public string notifyUrl { get; init; } = string.Empty;
    public string remark { get; init; } = string.Empty;
    public string createTime { get; init; } = string.Empty;
    public string createUserId { get; init; } = string.Empty;
    public string createUserName { get; init; } = string.Empty;
    public string updateTime { get; init; } = string.Empty;
    public string updateUserId { get; init; } = string.Empty;
    public string updateUserName { get; init; } = string.Empty;
}
