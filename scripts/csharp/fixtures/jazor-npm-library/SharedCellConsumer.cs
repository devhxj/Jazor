using ECMAScript;
using System;
using Cell = Jolt.Generated.Npm.Exports.Cell_A357BBB40AF5<string>;
using Wire = Npm.WireCells.Exports;

[ECMAScriptModule("shared-cell-consumer.mjs")]
public static class SharedCellConsumer
{
    public static Cell Create(string value) => SharedCellProducerB.Pass(SharedCellProducerA.Create(value));

    public static Cell Pass(Cell cell) => SharedCellProducerB.Pass(SharedCellProducerA.Pass(cell));

    public static string Label(string value) => SharedCellProducerB.Label(SharedCellProducerA.Create(value));

    public static string OmitMiddleSlot() => Wire.DescribeSlots("first", third: "third");

    public static string ForwardMethodGroup()
    {
        Func<string, string, string, string> describe = Wire.DescribeSlots;
        return describe("first", "second", "third");
    }
}
