using Jero.Web.App.Components;
using Jero.Web.App.Shell;
using static ECMAScript.Global;

namespace Jero.Web.App.WechatPay;

[ECMAScriptModule("components/wechat-pay-page-state.js")]
public static class WechatPayPageModule
{
    public static WechatPayPageState Create() => new();

    public sealed class WechatPayPageState : IWechatPayContentState
    {
        private WechatPayView[] items = [];
        public WechatPayView[] rows { get; private set; } = [];
        private int currentPage = 1;
        private int pageSize = 50;
        private int total;
        private string keyword = string.Empty;
        private string startDate = string.Empty;
        private string endDate = string.Empty;
        private bool loading;
        private string? error;

        public bool detailDialogVisible { get; set; }
        public WechatPayView? detail { get; private set; }

        public bool auditDialogVisible { get; set; }
        public WechatPayView? audit { get; private set; }

        public bool createDialogVisible { get; set; }
        public bool creating { get; private set; }
        public string createDescription { get; set; } = string.Empty;
        public string createTotal { get; set; } = "100";
        public string createAttachment { get; set; } = string.Empty;
        public string createGoodsTag { get; set; } = string.Empty;
        public string createTags { get; set; } = string.Empty;
        public string createBusinessId { get; set; } = string.Empty;

        public bool qrDialogVisible { get; set; }
        public bool qrLoading { get; private set; }
        public string qrImage { get; private set; } = string.Empty;
        public string qrOrderNumber { get; private set; } = string.Empty;

        public bool refundListDialogVisible { get; set; }
        public bool refundListLoading { get; private set; }
        private WechatRefundView[] refundItems = [];
        public WechatRefundView[] refundRows { get; private set; } = [];

        public bool refundDialogVisible { get; set; }
        public bool refunding { get; private set; }
        public WechatPayView? refundOrder { get; private set; }
        public string refundReason { get; set; } = string.Empty;
        private string syncingTradeNumber = string.Empty;

        private bool CanCreate => ApplicationState.HasPermission("sysWechatPay:payTransactionNative");
        public bool CanListRefund => ApplicationState.HasPermission("sysWechatPay:listRefund");
        public bool CanRefund => ApplicationState.HasPermission("sysWechatPay:refundDomestic");
        public bool CanSync => ApplicationState.HasPermission("sysWechatPay:payInfoFromWechat");

        public void Initialize()
            => LoadPage();

        private void LoadPage()
        {
            loading = true;
            WechatPayClient.Page(new WechatPayPageInput
            {
                page = currentPage,
                pageSize = pageSize,
                keyword = keyword.Trim(),
                createTimeRange = DateRange()
            }).Then(outcome =>
            {
                loading = false;
                if (!outcome.Ok)
                {
                    items = [];
                    rows = [];
                    total = 0;
                    error = outcome.Error ?? "加载微信支付订单失败。";
                    return;
                }

                items = WechatPayClient.ReadItems(outcome.Data);
                rows = items;
                total = WechatPayClient.ReadTotal(outcome.Data);
                error = null;
            }, _ =>
            {
                loading = false;
                error = "加载微信支付订单失败，请检查网络后重试。";
            });
        }

        private string[] DateRange()
        {
            var start = startDate.Trim();
            var end = endDate.Trim();
            if (start.Length == 0 && end.Length == 0)
                return [];
            if (start.Length == 0)
                return [end, end];
            if (end.Length == 0)
                return [start, start];
            return [start, end];
        }

        private void Search()
        {
            currentPage = 1;
            LoadPage();
        }

        private void ResetSearch()
        {
            keyword = string.Empty;
            startDate = string.Empty;
            endDate = string.Empty;
            currentPage = 1;
            LoadPage();
        }

        private void ChangePage(Number value)
        {
            currentPage = (int)value;
            LoadPage();
        }

        private void ChangePageSize(Number value)
        {
            pageSize = (int)value;
            currentPage = 1;
            LoadPage();
        }

        public void OpenDetail(VueProps? value)
        {
            detail = Find(RowString(value, "id"));
            if (detail is not null)
                detailDialogVisible = true;
        }

        public void OpenAudit(VueProps? value)
        {
            audit = Find(RowString(value, "id"));
            if (audit is not null)
                auditDialogVisible = true;
        }

        private void OpenCreate()
        {
            if (!CanCreate)
                return;
            createDescription = string.Empty;
            createTotal = "100";
            createAttachment = string.Empty;
            createGoodsTag = string.Empty;
            createTags = string.Empty;
            createBusinessId = string.Empty;
            error = null;
            createDialogVisible = true;
        }

        public void CreateNative()
        {
            if (!CanCreate || creating)
                return;

            var description = createDescription.Trim();
            var amount = PositiveInt(createTotal);
            var businessId = OptionalPositiveId(createBusinessId);
            if (description.Length == 0)
            {
                error = "商品描述不能为空。";
                return;
            }
            if (amount is null)
            {
                error = "金额必须是 1 至 2147483647 之间的整数分值。";
                return;
            }
            if (createBusinessId.Trim().Length > 0 && businessId is null)
            {
                error = "业务 ID 必须是正整数。";
                return;
            }

            creating = true;
            WechatPayClient.CreateNative(new WechatPayCreateInput
            {
                description = description,
                total = amount.Value,
                attachment = createAttachment.Trim(),
                goodsTag = createGoodsTag.Trim(),
                tags = createTags.Trim(),
                businessId = businessId ?? "0"
            }).Then(outcome =>
            {
                creating = false;
                if (!outcome.Ok)
                {
                    error = outcome.Error ?? "创建 Native 支付单失败。";
                    return;
                }

                var result = WechatPayClient.ReadCreateResult(outcome.Data);
                createDialogVisible = false;
                error = null;
                ElementPlus.ElMessageMembers.Success("Native 支付单已创建");
                LoadPage();
                if (result.qrcodeUrl.Length > 0)
                    RenderQrCode(result.qrcodeUrl, result.outTradeNumber);
            }, _ =>
            {
                creating = false;
                error = "创建 Native 支付单失败，请检查网络后重试。";
            });
        }

        public void OpenQrCode(VueProps? value)
        {
            var row = Find(RowString(value, "id"));
            if (row is null || row.qrcodeContent.Length == 0)
            {
                error = "当前订单没有可用的付款二维码。";
                return;
            }
            RenderQrCode(row.qrcodeContent, row.outTradeNumber);
        }

        private void RenderQrCode(string content, string orderNumber)
        {
            qrLoading = true;
            qrImage = string.Empty;
            qrOrderNumber = orderNumber;
            qrDialogVisible = true;
            QrCodeGenerator.ToDataUrl(content, new QrCodeOptions
            {
                width = 320,
                margin = 1,
                errorCorrectionLevel = "M"
            }).Then(image =>
            {
                qrLoading = false;
                qrImage = image;
                error = null;
            }, _ =>
            {
                qrLoading = false;
                qrDialogVisible = false;
                error = "生成付款二维码失败，请稍后重试。";
            });
        }

        public void OpenRefunds(VueProps? value)
        {
            var row = Find(RowString(value, "id"));
            if (!CanListRefund || row is null || row.transactionId.Length == 0 || refundListLoading)
                return;

            refundListDialogVisible = true;
            refundListLoading = true;
            refundItems = [];
            refundRows = [];
            WechatPayClient.Refunds(row.transactionId).Then(outcome =>
            {
                refundListLoading = false;
                if (!outcome.Ok)
                {
                    error = outcome.Error ?? "加载退款记录失败。";
                    return;
                }

                refundItems = WechatPayClient.ReadRefunds(outcome.Data);
                refundRows = refundItems;
                error = null;
            }, _ =>
            {
                refundListLoading = false;
                error = "加载退款记录失败，请检查网络后重试。";
            });
        }

        public void OpenRefund(VueProps? value)
        {
            var row = Find(RowString(value, "id"));
            if (!CanRefund || row is null || row.tradeState != "SUCCESS")
                return;
            refundOrder = row;
            refundReason = string.Empty;
            error = null;
            refundDialogVisible = true;
        }

        public void Refund()
        {
            var row = refundOrder;
            if (!CanRefund || refunding || row is null)
                return;
            if (row.outTradeNumber.Length == 0 || row.total <= 0)
            {
                error = "当前订单缺少退款所需的商户订单号或金额。";
                return;
            }

            refunding = true;
            WechatPayClient.Refund(new WechatPayRefundInput
            {
                tradeId = row.outTradeNumber,
                reason = refundReason.Trim(),
                refund = row.total,
                total = row.total
            }).Then(outcome =>
            {
                refunding = false;
                if (!outcome.Ok)
                {
                    error = outcome.Error ?? "提交全额退款失败。";
                    return;
                }

                refundDialogVisible = false;
                refundOrder = null;
                error = null;
                ElementPlus.ElMessageMembers.Success("全额退款申请已提交");
                LoadPage();
            }, _ =>
            {
                refunding = false;
                error = "提交全额退款失败，请检查网络后重试。";
            });
        }

        public void SyncWechat(VueProps? value)
        {
            var row = Find(RowString(value, "id"));
            if (!CanSync || row is null || row.outTradeNumber.Length == 0 || syncingTradeNumber.Length > 0)
                return;

            syncingTradeNumber = row.outTradeNumber;
            WechatPayClient.SyncFromWechat(row.outTradeNumber).Then(outcome =>
            {
                syncingTradeNumber = string.Empty;
                if (!outcome.Ok)
                {
                    error = outcome.Error ?? "同步微信支付状态失败。";
                    return;
                }

                var synced = WechatPayClient.ReadPayDetail(outcome.Data);
                error = null;
                ElementPlus.ElMessageMembers.Success("微信状态已同步：" + (synced is null ? "完成" : StateText(synced.tradeState)));
                LoadPage();
            }, _ =>
            {
                syncingTradeNumber = string.Empty;
                error = "同步微信支付状态失败，请检查网络后重试。";
            });
        }

        private WechatPayView? Find(string id)
        {
            for (var index = 0; index < items.Length; index++)
                if (items[index].id == id)
                    return items[index];
            return null;
        }

        private static string StateText(VueProps? row)
            => StateText(RowString(row, "tradeState"));

        private static string StateText(string state)
        {
            var normalized = state.ToUpperInvariant();
            if (normalized == "SUCCESS") return "完成";
            if (normalized == "REFUND") return "退款";
            if (normalized == "CLOSED") return "已关闭";
            if (normalized == "REVOKED") return "已撤销";
            if (normalized == "PAYERROR") return "支付失败";
            return normalized.Length == 0 ? "待支付" : normalized;
        }

        private static string StateTag(VueProps? row)
            => StateTag(RowString(row, "tradeState"));

        private static string StateTag(string state)
        {
            var normalized = state.ToUpperInvariant();
            if (normalized == "SUCCESS") return "success";
            if (normalized == "REFUND" || normalized == "PAYERROR") return "danger";
            if (normalized == "CLOSED" || normalized == "REVOKED") return "info";
            return "warning";
        }

        public bool IsSyncing(VueProps? row)
            => syncingTradeNumber.Length > 0 && syncingTradeNumber == RowString(row, "outTradeNumber");

        private static string Amount(VueProps? row, string key)
            => Amount(RowInt(row, key, 0));

        private static string Amount(int fen)
        {
            var yuan = fen / 100;
            var cents = fen % 100;
            return yuan + "." + (cents < 10 ? "0" : string.Empty) + cents;
        }

        private static string NullableAmount(int? fen)
            => fen is null ? "-" : Amount(fen.Value);

        private static string DisplayRowValue(VueProps? row, string key)
            => DisplayValue(RowString(row, key));

        private static string DisplayValue(string value)
            => value.Length == 0 ? "-" : value;

        private static string DisplayUser(string name, string id)
        {
            if (name.Length > 0 && id.Length > 0)
                return name + " (" + id + ")";
            if (name.Length > 0)
                return name;
            return DisplayValue(id);
        }

        private static int? PositiveInt(string value)
        {
            var normalized = value.Trim();
            if (normalized.Length == 0)
                return null;
            var number = (double)NumberValue(normalized);
            if (!IsFinite(number) || number < 1 || number > 2147483647 || number % 1 != 0)
                return null;
            return (int)number;
        }

        private static string? OptionalPositiveId(string value)
        {
            var normalized = value.Trim();
            if (normalized.Length == 0)
                return null;
            var number = (double)NumberValue(normalized);
            return IsFinite(number) && number > 0 && number % 1 == 0 ? normalized : null;
        }

        private static string RowString(VueProps? row, string key)
        {
            if (row is null)
                return string.Empty;
            var value = Reflect.Get(row, key);
            if (value is null)
                return string.Empty;
            var result = StringValue(value);
            return result == "null" || result == "undefined" ? string.Empty : result;
        }

        private static int RowInt(VueProps? row, string key, int fallback)
        {
            if (row is null)
                return fallback;
            var value = Reflect.Get(row, key);
            return value is null ? fallback : (int)NumberValue(value);
        }

        public string RowStateText(VueProps? row) => StateText(row);
        public string TradeStateText(string state) => StateText(state);
        public string RowStateTag(VueProps? row) => StateTag(row);
        public string TradeStateTag(string state) => StateTag(state);
        public string RowAmount(VueProps? row, string key) => Amount(row, key);
        public string FormatAmount(int fen) => Amount(fen);
        public string NullableAmountForView(int? fen) => NullableAmount(fen);
        public string DisplayRowValueForView(VueProps? row, string key) => DisplayRowValue(row, key);
        public string DisplayValueForView(string value) => DisplayValue(value);
        public string DisplayUserForView(string name, string id) => DisplayUser(name, id);
        public string RowStringForView(VueProps? row, string key) => RowString(row, key);

        public WechatPayPageProps CreateProps() => new()
        {
            SetKeyword = value => keyword = value,
            SetStartDate = value => startDate = value,
            SetEndDate = value => endDate = value,
            DismissError = () => error = null,
            Search = Search,
            ResetSearch = ResetSearch,
            OpenCreate = OpenCreate,
            LoadPage = LoadPage,
            ChangePage = value => ChangePage(value),
            ChangePageSize = value => ChangePageSize(value),
            CanCreate = CanCreate,
            CurrentPage = currentPage,
            PageSize = pageSize,
            Total = total,
            Keyword = keyword,
            StartDate = startDate,
            EndDate = endDate,
            Loading = loading,
            Error = error,
        };
    }
}

public sealed record WechatPayPageProps : VueProps
{
    public required Action<string> SetKeyword { get; init; }
    public required Action<string> SetStartDate { get; init; }
    public required Action<string> SetEndDate { get; init; }
    public required Action DismissError { get; init; }
    public required Action Search { get; init; }
    public required Action ResetSearch { get; init; }
    public required Action OpenCreate { get; init; }
    public required Action LoadPage { get; init; }
    public required Action<double> ChangePage { get; init; }
    public required Action<double> ChangePageSize { get; init; }
    public required bool CanCreate { get; init; }
    public required int CurrentPage { get; init; }
    public required int PageSize { get; init; }
    public required int Total { get; init; }
    public required string Keyword { get; init; }
    public required string StartDate { get; init; }
    public required string EndDate { get; init; }
    public required bool Loading { get; init; }
    public required string? Error { get; init; }
}
