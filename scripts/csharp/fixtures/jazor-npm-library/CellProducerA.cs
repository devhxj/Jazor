using ECMAScript;
using Wire = Npm.WireCells.Exports;
using Cell = Jolt.Generated.Npm.Exports.Cell_A357BBB40AF5<string>;

[ECMAScriptModule("cell-producer-a.mjs")]
public static class CellProducerA
{
    public static Cell Create(string value) => Wire.MakeCell(value);
}
