namespace Jero.Web.App.Menu;

public sealed record MenuTableRow(
    string id,
    string title,
    int type,
    string typeText,
    string path,
    string component,
    string permission,
    int orderNo,
    int status,
    bool hasChildren,
    bool expanded,
    string padding,
    string toggleText,
    string icon,
    string iconGlyph);
