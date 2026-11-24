extern alias First;
extern alias Second;

using ECMAScript;
using Local = Npm.WireCells.Exports;
using Cell = First::Jolt.Generated.Npm.Exports.Cell_A357BBB40AF5<string>;

[ECMAScriptModule("aliased-cell-consumer.mjs")]
public static class AliasedCellConsumer
{
    public static Cell Create(string value) => First::CellProducerA.Create(value);

    public static Cell Roundtrip(Cell input) => Second::CellProducerB.Echo(input);

    public static double Count() => Second::CellProducerB.Count();

    public static double? Weight(string name) => Local.CellWeights()[name];
}
