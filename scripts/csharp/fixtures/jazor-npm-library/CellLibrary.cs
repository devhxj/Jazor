using ECMAScript;
using Npm.WireCells;
using Cell = Jolt.Generated.Npm.Exports.Cell_A357BBB40AF5<string>;

[ECMAScriptModule("cell-library.mjs")]
public static class CellLibrary
{
    public static Cell Create(string value) => Exports.MakeCell(value);

    public static double? Weight(string name) => Exports.CellWeights()[name];
}
