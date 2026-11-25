namespace Jero.Web.App.Print;

public sealed record PrintNumberProps(double max, double precision) : ECMAScript.Vue.VueProps;

public sealed record PrintTableRow(
    string id, string name, int printType, int status, int orderNo, string clientServiceAddress, string remark);
