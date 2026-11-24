using ECMAScript;
using Wire = Npm.WireCells.Exports;
using Cell = Jolt.Generated.Npm.Exports.Cell_A357BBB40AF5<string>;

[ECMAScriptModule("shared-cell-producer-b.mjs")]
public static class SharedCellProducerB
{
    public static Cell Pass(Cell cell) => cell;

    public static string Label(Cell cell) => Wire.ReadLabel(cell);
}
