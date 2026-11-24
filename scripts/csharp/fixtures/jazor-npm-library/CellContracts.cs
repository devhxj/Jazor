using Wire = Npm.WireCells.Exports;

internal static class CellContracts
{
    internal const string Factory = nameof(Wire.MakeCell);
    internal const string Label = nameof(Wire.ReadLabel);
    internal const string Slots = nameof(Wire.DescribeSlots);
}
