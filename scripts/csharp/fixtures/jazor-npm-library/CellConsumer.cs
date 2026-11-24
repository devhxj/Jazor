using ECMAScript;
using Cell = Jolt.Generated.Npm.Exports.Cell_A357BBB40AF5<string>;
using Operations = Npm.CellOps.Exports;

[ECMAScriptModule("cell-consumer.mjs")]
public static class CellConsumer
{
    public static Cell Read(string value) => CellLibrary.Create(value);

    public static Cell Direct(string value) => Npm.WireCells.Exports.MakeCell(value);

    public static Cell Roundtrip(Cell cell)
        => Operations.Apply(cell, value => value);

    public static string Select(Cell cell)
        => Operations.Apply(cell, value => value.Value + value.Label);

    public static double? Weight(string name) => CellLibrary.Weight(name);

    public static string PlainLabel() => Operations.Label("plain");

    public static string ConfiguredLabel()
        => Operations.Label(new Operations.LabelOptions { Message = "configured", Type = Operations.Type.Loud });

    public static string NumericLabel()
        => Operations.Label(new Operations.LabelOptions { Message = 7d, Type = Operations.Type.Plain });
}
