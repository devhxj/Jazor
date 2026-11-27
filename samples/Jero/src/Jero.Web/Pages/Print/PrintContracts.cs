namespace Jero.Web.Pages.Print;

public sealed record PrintNumberProps(
    [property: ECMAScriptName("max")] double Max,
    [property: ECMAScriptName("precision")] double Precision) : ECMAScript.Vue.VueProps;

public sealed record PrintTableRow(
    string Id, string Name, int PrintType, int Status, int OrderNo, string ClientServiceAddress, string Remark);
