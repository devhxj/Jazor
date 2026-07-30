using System.Numerics;

namespace Jazor.CLR.Test;

internal static class ClrRuntimeBigIntegerBinaryScenarios
{
    private const string ModulePath = "System/Numerics/BigIntegerModule.js";
    private static readonly BigInteger TwoPow64 = BigInteger.One << 64;

    public static IReadOnlyList<ClrRuntimeScenario> All { get; } =
    [
        Success("big-integer.bytes.ctor-empty", "System.Numerics.BigInteger.BigInteger(byte[])", [Bytes()], Big(0)),
        Success("big-integer.bytes.ctor-little-endian", "System.Numerics.BigInteger.BigInteger(byte[])", [Bytes(0, 1)], Big(256)),
        Success("big-integer.bytes.ctor-negative-one", "System.Numerics.BigInteger.BigInteger(byte[])", [Bytes(255)], Big(-1)),
        Success("big-integer.bytes.ctor-negative-boundary", "System.Numerics.BigInteger.BigInteger(byte[])", [Bytes(128)], Big(-128)),
        Success("big-integer.bytes.span-ctor-signed-little-endian", "System.Numerics.BigInteger.BigInteger(System.ReadOnlySpan<byte>, bool, bool)", [Bytes(0, 1), Bool(false), Bool(false)], Big(256)),
        Success("big-integer.bytes.span-ctor-signed-big-endian", "System.Numerics.BigInteger.BigInteger(System.ReadOnlySpan<byte>, bool, bool)", [Bytes(0, 1), Bool(false), Bool(true)], Big(1)),
        Success("big-integer.bytes.span-ctor-unsigned-high-bit", "System.Numerics.BigInteger.BigInteger(System.ReadOnlySpan<byte>, bool, bool)", [Bytes(128), Bool(true), Bool(false)], Big(128)),
        Success("big-integer.bytes.span-ctor-signed-negative-big-endian", "System.Numerics.BigInteger.BigInteger(System.ReadOnlySpan<byte>, bool, bool)", [Bytes(255, 127), Bool(false), Bool(true)], Big(-129)),
        Success("big-integer.bytes.span-ctor-nine-byte-big-endian", "System.Numerics.BigInteger.BigInteger(System.ReadOnlySpan<byte>, bool, bool)", [Bytes(1, 0, 0, 0, 0, 0, 0, 0, 0), Bool(false), Bool(true)], Big(TwoPow64)),
        Success("big-integer.bytes.to-array-zero", "System.Numerics.BigInteger.ToByteArray()", [Big(0)], Bytes(0)),
        Success("big-integer.bytes.to-array-positive-single-byte", "System.Numerics.BigInteger.ToByteArray()", [Big(127)], Bytes(127)),
        Success("big-integer.bytes.to-array-positive-sign-padding", "System.Numerics.BigInteger.ToByteArray()", [Big(128)], Bytes(128, 0)),
        Success("big-integer.bytes.to-array-positive-word", "System.Numerics.BigInteger.ToByteArray()", [Big(256)], Bytes(0, 1)),
        Success("big-integer.bytes.to-array-negative-one", "System.Numerics.BigInteger.ToByteArray()", [Big(-1)], Bytes(255)),
        Success("big-integer.bytes.to-array-negative-single-byte", "System.Numerics.BigInteger.ToByteArray()", [Big(-128)], Bytes(128)),
        Success("big-integer.bytes.to-array-negative-sign-extension", "System.Numerics.BigInteger.ToByteArray()", [Big(-129)], Bytes(127, 255)),
        Success("big-integer.bytes.to-array-negative-word", "System.Numerics.BigInteger.ToByteArray()", [Big(-256)], Bytes(0, 255)),
        Success("big-integer.bytes.to-array-nine-byte-positive", "System.Numerics.BigInteger.ToByteArray()", [Big(TwoPow64)], Bytes(0, 0, 0, 0, 0, 0, 0, 0, 1)),
        Success("big-integer.bytes.to-array-nine-byte-negative", "System.Numerics.BigInteger.ToByteArray()", [Big(-TwoPow64)], Bytes(0, 0, 0, 0, 0, 0, 0, 0, 255)),
        Success("big-integer.bytes.to-array-flags-unsigned", "System.Numerics.BigInteger.ToByteArray(bool, bool)", [Big(128), Bool(true), Bool(false)], Bytes(128)),
        Success("big-integer.bytes.to-array-flags-signed-big-endian", "System.Numerics.BigInteger.ToByteArray(bool, bool)", [Big(128), Bool(false), Bool(true)], Bytes(0, 128)),
        Success("big-integer.bytes.to-array-flags-unsigned-big-endian", "System.Numerics.BigInteger.ToByteArray(bool, bool)", [Big(256), Bool(true), Bool(true)], Bytes(1, 0)),
        Success("big-integer.bytes.to-array-flags-negative-big-endian", "System.Numerics.BigInteger.ToByteArray(bool, bool)", [Big(-129), Bool(false), Bool(true)], Bytes(255, 127)),
        Failure("big-integer.bytes.to-array-flags-negative-unsigned", "System.Numerics.BigInteger.ToByteArray(bool, bool)", [Big(-1), Bool(true), Bool(false)], "OverflowException"),
        Success("big-integer.bytes.count-zero-signed", "System.Numerics.BigInteger.GetByteCount(bool)", [Big(0), Bool(false)], Number(1)),
        Success("big-integer.bytes.count-zero-unsigned", "System.Numerics.BigInteger.GetByteCount(bool)", [Big(0), Bool(true)], Number(1)),
        Success("big-integer.bytes.count-positive-single-byte", "System.Numerics.BigInteger.GetByteCount(bool)", [Big(127), Bool(false)], Number(1)),
        Success("big-integer.bytes.count-positive-sign-padding", "System.Numerics.BigInteger.GetByteCount(bool)", [Big(128), Bool(false)], Number(2)),
        Success("big-integer.bytes.count-positive-unsigned", "System.Numerics.BigInteger.GetByteCount(bool)", [Big(255), Bool(true)], Number(1)),
        Success("big-integer.bytes.count-negative-single-byte", "System.Numerics.BigInteger.GetByteCount(bool)", [Big(-128), Bool(false)], Number(1)),
        Success("big-integer.bytes.count-negative-sign-extension", "System.Numerics.BigInteger.GetByteCount(bool)", [Big(-129), Bool(false)], Number(2)),
        Failure("big-integer.bytes.count-negative-unsigned", "System.Numerics.BigInteger.GetByteCount(bool)", [Big(-1), Bool(true)], "OverflowException"),
        SuccessWithArguments(
            "big-integer.bytes.try-write-signed-little-endian",
            "System.Numerics.BigInteger.TryWriteBytes(System.Span<byte>, out int, bool, bool)",
            [Big(128), Bytes(9, 9, 9), Number(0), Bool(false), Bool(false)],
            Array(Bool(true), Number(2)),
            [Big(128), Bytes(128, 0, 9), Number(0), Bool(false), Bool(false)]),
        SuccessWithArguments(
            "big-integer.bytes.try-write-signed-big-endian",
            "System.Numerics.BigInteger.TryWriteBytes(System.Span<byte>, out int, bool, bool)",
            [Big(-129), Bytes(9, 9, 9), Number(0), Bool(false), Bool(true)],
            Array(Bool(true), Number(2)),
            [Big(-129), Bytes(255, 127, 9), Number(0), Bool(false), Bool(true)]),
        SuccessWithArguments(
            "big-integer.bytes.try-write-insufficient-destination",
            "System.Numerics.BigInteger.TryWriteBytes(System.Span<byte>, out int, bool, bool)",
            [Big(128), Bytes(9), Number(0), Bool(false), Bool(false)],
            Array(Bool(false), Number(0)),
            [Big(128), Bytes(9), Number(0), Bool(false), Bool(false)]),
        Failure(
            "big-integer.bytes.try-write-negative-unsigned",
            "System.Numerics.BigInteger.TryWriteBytes(System.Span<byte>, out int, bool, bool)",
            [Big(-1), Bytes(9), Number(0), Bool(true), Bool(false)],
            "OverflowException"),
        Success("big-integer.rotate-left.zero", "static System.Numerics.BigInteger.RotateLeft(System.Numerics.BigInteger, int)", [Big(0), Number(7)], Big(0)),
        Success("big-integer.rotate-left.compact-positive", "static System.Numerics.BigInteger.RotateLeft(System.Numerics.BigInteger, int)", [Big(11), Number(1)], Big(22)),
        Success("big-integer.rotate-right.compact-positive", "static System.Numerics.BigInteger.RotateRight(System.Numerics.BigInteger, int)", [Big(11), Number(1)], Big(2147483653)),
        Success("big-integer.rotate-left.negative-amount", "static System.Numerics.BigInteger.RotateLeft(System.Numerics.BigInteger, int)", [Big(11), Number(-1)], Big(2147483653)),
        Success("big-integer.rotate-right.negative-amount", "static System.Numerics.BigInteger.RotateRight(System.Numerics.BigInteger, int)", [Big(11), Number(-1)], Big(22)),
        Success("big-integer.rotate-left.compact-negative", "static System.Numerics.BigInteger.RotateLeft(System.Numerics.BigInteger, int)", [Big(-3), Number(1)], Big(-5)),
        Success("big-integer.rotate-right.compact-negative", "static System.Numerics.BigInteger.RotateRight(System.Numerics.BigInteger, int)", [Big(-3), Number(1)], Big(-2)),
        Success("big-integer.rotate-left-word-sign-boundary", "static System.Numerics.BigInteger.RotateLeft(System.Numerics.BigInteger, int)", [Big(2147483648), Number(1)], Big(1)),
        Success("big-integer.rotate-right.multi-word", "static System.Numerics.BigInteger.RotateRight(System.Numerics.BigInteger, int)", [Big(4294967296), Number(1)], Big(2147483648)),
        Success("big-integer.rotate-left.multi-word-negative", "static System.Numerics.BigInteger.RotateLeft(System.Numerics.BigInteger, int)", [Big(-4294967296), Number(1)], Big(-8589934591)),
        Success("big-integer.rotate-right.multi-word-negative", "static System.Numerics.BigInteger.RotateRight(System.Numerics.BigInteger, int)", [Big(-4294967296), Number(1)], Big(9223372034707292160)),
        Success("big-integer.unsigned-shift.positive", "static System.Numerics.BigInteger.operator >>>(System.Numerics.BigInteger, int)", [Big(8), Number(1)], Big(4)),
        Success("big-integer.unsigned-shift.compact-negative", "static System.Numerics.BigInteger.operator >>>(System.Numerics.BigInteger, int)", [Big(-3), Number(1)], Big(9223372036854775806)),
        Success("big-integer.unsigned-shift.multi-word-negative", "static System.Numerics.BigInteger.operator >>>(System.Numerics.BigInteger, int)", [Big(-4294967296), Number(1)], Big(9223372034707292160)),
        Success("big-integer.unsigned-shift-negative-amount", "static System.Numerics.BigInteger.operator >>>(System.Numerics.BigInteger, int)", [Big(-3), Number(-1)], Big(-6)),
        Success("big-integer.unsigned-shift-beyond-width", "static System.Numerics.BigInteger.operator >>>(System.Numerics.BigInteger, int)", [Big(-3), Number(64)], Big(0))
    ];

    private static ClrRuntimeScenario Success(
        string id,
        string member,
        IReadOnlyList<ClrRuntimeValue> arguments,
        ClrRuntimeValue expected)
        => new(id, member, ModulePath, arguments, expected);

    private static ClrRuntimeScenario SuccessWithArguments(
        string id,
        string member,
        IReadOnlyList<ClrRuntimeValue> arguments,
        ClrRuntimeValue expected,
        IReadOnlyList<ClrRuntimeValue> expectedArguments)
        => new(id, member, ModulePath, arguments, expected, ExpectedArguments: expectedArguments);

    private static ClrRuntimeScenario Failure(
        string id,
        string member,
        IReadOnlyList<ClrRuntimeValue> arguments,
        string error)
        => new(id, member, ModulePath, arguments, ExpectedValue: null, ExpectedErrorContains: error);

    private static ClrRuntimeValue Big(long value) => ClrRuntimeValue.BigInt(value);
    private static ClrRuntimeValue Big(BigInteger value) => ClrRuntimeValue.BigInt(value);
    private static ClrRuntimeValue Number(double value) => ClrRuntimeValue.Number(value);
    private static ClrRuntimeValue Bool(bool value) => ClrRuntimeValue.Boolean(value);
    private static ClrRuntimeValue Array(params ClrRuntimeValue[] values) => ClrRuntimeValue.Array(values);
    private static ClrRuntimeValue Bytes(params int[] values)
        => ClrRuntimeValue.Array(values.Select(static value => ClrRuntimeValue.Number(value)).ToArray());
}
