using ECMAScript;
using Cell = Jolt.Generated.Npm.Exports.Cell_A357BBB40AF5<string>;

[ECMAScriptModule("shared-cell-consumer.mjs")]
public static class SharedCellConsumer
{
    public static Cell Create(string value) => SharedCellProducerB.Pass(SharedCellProducerA.Create(value));

    public static Cell Pass(Cell cell) => SharedCellProducerB.Pass(SharedCellProducerA.Pass(cell));

    public static string Label(string value) => SharedCellProducerB.Label(SharedCellProducerA.Create(value));
}
