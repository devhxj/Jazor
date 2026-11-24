using ECMAScript;
using Wire = Npm.WireCells.Exports;
using Cell = Jolt.Generated.Npm.Exports.Cell_A357BBB40AF5<string>;

[ECMAScriptModule("shared-cell-producer-a.mjs")]
public static class SharedCellProducerA
{
    public static Cell Create(string value) => Wire.MakeCell(value);

    public static Cell Pass(Cell cell) => cell;
}
