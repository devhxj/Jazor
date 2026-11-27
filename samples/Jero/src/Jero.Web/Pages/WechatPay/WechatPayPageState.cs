using Jero.Web.Application;
using Jero.Web.Infrastructure.Api.WechatPay;
using Jero.Web.Infrastructure.Browser;
using Jero.Web.Pages.Components;
using Jero.Web.Pages.Shell;
using static ECMAScript.Global;

namespace Jero.Web.Pages.WechatPay;

[ECMAScriptModule("components/wechat-pay-page-state.js")]
public static class WechatPayPageModule
{
    public static WechatPayPageState Create() => new();

    public sealed class WechatPayPageState : IWechatPayContentState
    {
        private WechatPayView[] _items = [];
        public WechatPayView[] Rows { get; private set; } = [];
        private int _currentPage = 1;
        private int _pageSize = 50;
        private int _total;
        private string _keyword = string.Empty;
        private string _startDate = string.Empty;
        private string _endDate = string.Empty;
        private bool _loading;
        private string? _error;

        public bool DetailDialogVisible { get; set; }
        public WechatPayView? Detail { get; private set; }

        public bool AuditDialogVisible { get; set; }
        public WechatPayView? Audit { get; private set; }

        public bool CreateDialogVisible { get; set; }
        public bool Creating { get; private set; }
        public string CreateDescription { get; set; } = string.Empty;
        public string CreateTotal { get; set; } = "100";
        public string CreateAttachment { get; set; } = string.Empty;
        public string CreateGoodsTag { get; set; } = string.Empty;
        public string CreateTags { get; set; } = string.Empty;
        public string CreateBusinessId { get; set; } = string.Empty;

        public bool QrDialogVisible { get; set; }
        public bool QrLoading { get; private set; }
        public string QrImage { get; private set; } = string.Empty;
        public string QrOrderNumber { get; private set; } = string.Empty;

        public bool RefundListDialogVisible { get; set; }
        public bool RefundListLoading { get; private set; }
        private WechatRefundView[] _refundItems = [];
        public WechatRefundView[] RefundRows { get; private set; } = [];

        public bool RefundDialogVisible { get; set; }
        public bool Refunding { get; private set; }
        public WechatPayView? RefundOrder { get; private set; }
        public string RefundReason { get; set; } = string.Empty;
        private string _syncingTradeNumber = string.Empty;

        private bool CanCreate => ApplicationState.HasPermission("sysWechatPay:payTransactionNative");
        public bool CanListRefund => ApplicationState.HasPermission("sysWechatPay:listRefund");
        public bool CanRefund => ApplicationState.HasPermission("sysWechatPay:refundDomestic");
        public bool CanSync => ApplicationState.HasPermission("sysWechatPay:payInfoFromWechat");

        public void Initialize()
            => LoadPage();

        private void LoadPage()
        {
            _loading = true;
            WechatPayClient.Page(new WechatPayPageInput
            {
                Page = _currentPage,
                PageSize = _pageSize,
                Keyword = _keyword.Trim(),
                CreateTimeRange = DateRange()
            }).Then(outcome =>
            {
                _loading = false;
                if (!outcome.Ok)
                {
                    _items = [];
                    Rows = [];
                    _total = 0;
                    _error = outcome.Error ?? "加载微信支付订单失败。";
                    return;
                }

                _items = WechatPayClient.ReadItems(outcome.Data);
                Rows = _items;
                _total = WechatPayClient.ReadTotal(outcome.Data);
                _error = null;
            }, _ =>
            {
                _loading = false;
                _error = "加载微信支付订单失败，请检查网络后重试。";
            });
        }

        private string[] DateRange()
        {
            var start = _startDate.Trim();
            var end = _endDate.Trim();
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
            _currentPage = 1;
            LoadPage();
        }

        private void ResetSearch()
        {
            _keyword = string.Empty;
            _startDate = string.Empty;
            _endDate = string.Empty;
            _currentPage = 1;
            LoadPage();
        }

        private void ChangePage(Number value)
        {
            _currentPage = (int)value;
            LoadPage();
        }

        private void ChangePageSize(Number value)
        {
            _pageSize = (int)value;
            _currentPage = 1;
            LoadPage();
        }

        public void OpenDetail(VueProps? value)
        {
            Detail = Find(RowString(value, "id"));
            if (Detail is not null)
                DetailDialogVisible = true;
        }

        public void OpenAudit(VueProps? value)
        {
            Audit = Find(RowString(value, "id"));
            if (Audit is not null)
                AuditDialogVisible = true;
        }

        private void OpenCreate()
        {
            if (!CanCreate)
                return;
            CreateDescription = string.Empty;
            CreateTotal = "100";
            CreateAttachment = string.Empty;
            CreateGoodsTag = string.Empty;
            CreateTags = string.Empty;
            CreateBusinessId = string.Empty;
            _error = null;
            CreateDialogVisible = true;
        }

        public void CreateNative()
        {
            if (!CanCreate || Creating)
                return;

            var description = CreateDescription.Trim();
            var amount = PositiveInt(CreateTotal);
            var businessId = OptionalPositiveId(CreateBusinessId);
            if (description.Length == 0)
            {
                _error = "商品描述不能为空。";
                return;
            }
            if (amount is null)
            {
                _error = "金额必须是 1 至 2147483647 之间的整数分值。";
                return;
            }
            if (CreateBusinessId.Trim().Length > 0 && businessId is null)
            {
                _error = "业务 ID 必须是正整数。";
                return;
            }

            Creating = true;
            WechatPayClient.CreateNative(new WechatPayCreateInput
            {
                Description = description,
                Total = amount.Value,
                Attachment = CreateAttachment.Trim(),
                GoodsTag = CreateGoodsTag.Trim(),
                Tags = CreateTags.Trim(),
                BusinessId = businessId ?? "0"
            }).Then(outcome =>
            {
                Creating = false;
                if (!outcome.Ok)
                {
                    _error = outcome.Error ?? "创建 Native 支付单失败。";
                    return;
                }

                var result = WechatPayClient.ReadCreateResult(outcome.Data);
                CreateDialogVisible = false;
                _error = null;
                ElementPlus.ElMessageMembers.Success("Native 支付单已创建");
                LoadPage();
                if (result.QrcodeUrl.Length > 0)
                    RenderQrCode(result.QrcodeUrl, result.OutTradeNumber);
            }, _ =>
            {
                Creating = false;
                _error = "创建 Native 支付单失败，请检查网络后重试。";
            });
        }

        public void OpenQrCode(VueProps? value)
        {
            var row = Find(RowString(value, "id"));
            if (row is null || row.QrcodeContent.Length == 0)
            {
                _error = "当前订单没有可用的付款二维码。";
                return;
            }
            RenderQrCode(row.QrcodeContent, row.OutTradeNumber);
        }

        private void RenderQrCode(string content, string orderNumber)
        {
            QrLoading = true;
            QrImage = string.Empty;
            QrOrderNumber = orderNumber;
            QrDialogVisible = true;
            QrCodeGenerator.ToDataUrl(content, new QrCodeOptions
            {
                Width = 320,
                Margin = 1,
                ErrorCorrectionLevel = "M"
            }).Then(image =>
            {
                QrLoading = false;
                QrImage = image;
                _error = null;
            }, _ =>
            {
                QrLoading = false;
                QrDialogVisible = false;
                _error = "生成付款二维码失败，请稍后重试。";
            });
        }

        public void OpenRefunds(VueProps? value)
        {
            var row = Find(RowString(value, "id"));
            if (!CanListRefund || row is null || row.TransactionId.Length == 0 || RefundListLoading)
                return;

            RefundListDialogVisible = true;
            RefundListLoading = true;
            _refundItems = [];
            RefundRows = [];
            WechatPayClient.Refunds(row.TransactionId).Then(outcome =>
            {
                RefundListLoading = false;
                if (!outcome.Ok)
                {
                    _error = outcome.Error ?? "加载退款记录失败。";
                    return;
                }

                _refundItems = WechatPayClient.ReadRefunds(outcome.Data);
                RefundRows = _refundItems;
                _error = null;
            }, _ =>
            {
                RefundListLoading = false;
                _error = "加载退款记录失败，请检查网络后重试。";
            });
        }

        public void OpenRefund(VueProps? value)
        {
            var row = Find(RowString(value, "id"));
            if (!CanRefund || row is null || row.TradeState != "SUCCESS")
                return;
            RefundOrder = row;
            RefundReason = string.Empty;
            _error = null;
            RefundDialogVisible = true;
        }

        public void Refund()
        {
            var row = RefundOrder;
            if (!CanRefund || Refunding || row is null)
                return;
            if (row.OutTradeNumber.Length == 0 || row.Total <= 0)
            {
                _error = "当前订单缺少退款所需的商户订单号或金额。";
                return;
            }

            Refunding = true;
            WechatPayClient.Refund(new WechatPayRefundInput
            {
                TradeId = row.OutTradeNumber,
                Reason = RefundReason.Trim(),
                Refund = row.Total,
                Total = row.Total
            }).Then(outcome =>
            {
                Refunding = false;
                if (!outcome.Ok)
                {
                    _error = outcome.Error ?? "提交全额退款失败。";
                    return;
                }

                RefundDialogVisible = false;
                RefundOrder = null;
                _error = null;
                ElementPlus.ElMessageMembers.Success("全额退款申请已提交");
                LoadPage();
            }, _ =>
            {
                Refunding = false;
                _error = "提交全额退款失败，请检查网络后重试。";
            });
        }

        public void SyncWechat(VueProps? value)
        {
            var row = Find(RowString(value, "id"));
            if (!CanSync || row is null || row.OutTradeNumber.Length == 0 || _syncingTradeNumber.Length > 0)
                return;

            _syncingTradeNumber = row.OutTradeNumber;
            WechatPayClient.SyncFromWechat(row.OutTradeNumber).Then(outcome =>
            {
                _syncingTradeNumber = string.Empty;
                if (!outcome.Ok)
                {
                    _error = outcome.Error ?? "同步微信支付状态失败。";
                    return;
                }

                var synced = WechatPayClient.ReadPayDetail(outcome.Data);
                _error = null;
                ElementPlus.ElMessageMembers.Success("微信状态已同步：" + (synced is null ? "完成" : StateText(synced.TradeState)));
                LoadPage();
            }, _ =>
            {
                _syncingTradeNumber = string.Empty;
                _error = "同步微信支付状态失败，请检查网络后重试。";
            });
        }

        private WechatPayView? Find(string id)
        {
            for (var index = 0; index < _items.Length; index++)
                if (_items[index].Id == id)
                    return _items[index];
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
            => _syncingTradeNumber.Length > 0 && _syncingTradeNumber == RowString(row, "outTradeNumber");

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
            SetKeyword = value => _keyword = value,
            SetStartDate = value => _startDate = value,
            SetEndDate = value => _endDate = value,
            DismissError = () => _error = null,
            Search = Search,
            ResetSearch = ResetSearch,
            OpenCreate = OpenCreate,
            LoadPage = LoadPage,
            ChangePage = value => ChangePage(value),
            ChangePageSize = value => ChangePageSize(value),
            CanCreate = CanCreate,
            CurrentPage = _currentPage,
            PageSize = _pageSize,
            Total = _total,
            Keyword = _keyword,
            StartDate = _startDate,
            EndDate = _endDate,
            Loading = _loading,
            Error = _error,
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
