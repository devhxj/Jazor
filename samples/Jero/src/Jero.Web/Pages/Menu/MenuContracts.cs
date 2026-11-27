namespace Jero.Web.Pages.Menu;

public sealed record MenuTableRow(
    string Id,
    string Title,
    int Type,
    string TypeText,
    string Path,
    string Component,
    string Permission,
    int OrderNo,
    int Status,
    bool HasChildren,
    bool Expanded,
    string Padding,
    string ToggleText,
    string Icon,
    string IconGlyph);
