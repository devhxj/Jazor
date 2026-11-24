using ECMAScript;
using Wire = Npm.WireCells.Exports;
using Cell = Jolt.Generated.Npm.Exports.Cell_A357BBB40AF5<string>;

[ECMAScriptModule("cell-producer-b.mjs")]
public static class CellProducerB
{
    public static double Count() => Wire.CellCount();

    public static string Label(Cell cell) => Wire.ReadLabel(cell);

    public static TValue Echo<TValue>(TValue value) => Npm.CellOps.Exports.Apply(value, cell => cell);
}
