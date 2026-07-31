using ECMAScript;
using Jazor.Compiler;
using Jazor.RazorVue.RazorSdk;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Jazor.RazorVue.Sg.Test;

[TestClass]
public sealed class CurrentComponentStateDefaultInitializerTests
{
    private static readonly Lazy<IReadOnlyDictionary<string, ITypeSymbol>> Symbols = new(CreateSymbols);

    public static IEnumerable<TestDataRow<StateDefaultCase>> Cases
        => StateDefaultCaseCatalog.All.Select(static testCase => new TestDataRow<StateDefaultCase>(testCase)
        {
            DisplayName = testCase.Id
        });

    [TestMethod]
    public void ScenarioCatalog_HasUniqueIdsDimensionsAndSymbolCoverage()
    {
        var cases = StateDefaultCaseCatalog.All;

        Assert.IsNotEmpty(cases);
        Assert.HasCount(cases.Count, cases.Select(static testCase => testCase.Id).Distinct(StringComparer.Ordinal));
        Assert.HasCount(cases.Count, cases.Select(static testCase => testCase.SymbolName).Distinct(StringComparer.Ordinal));
        Assert.IsTrue(cases.All(static testCase => testCase.Id.StartsWith("state-default.", StringComparison.Ordinal)));
        Assert.IsTrue(cases.All(static testCase => !string.IsNullOrWhiteSpace(testCase.Dimension)));
        CollectionAssert.AreEquivalent(
            Symbols.Value.Keys.ToArray(),
            cases.Select(static testCase => testCase.SymbolName).ToArray());
    }

    [TestMethod]
    [DynamicData(nameof(Cases))]
    public void CreateExpression_MatchesScenarioContract(StateDefaultCase testCase)
    {
        var type = Symbols.Value[testCase.SymbolName];

        switch (testCase.Expectation)
        {
            case StateDefaultExpectation.JavaScript expected:
            {
                var expression = CurrentComponentStateDefaultInitializer.CreateExpression(type);
                Assert.AreEqual(expected.Text, expression.ToKnRECMAScript(), testCase.Id);
                break;
            }

            case StateDefaultExpectation.Unsupported:
            {
                var exception = Assert.Throws<NotSupportedException>(
                    () => CurrentComponentStateDefaultInitializer.CreateExpression(type));
                StringAssert.Contains(
                    exception.Message,
                    type.OriginalDefinition.ToDisplayString(Jazor.Common.Format.NameFormat),
                    testCase.Id);
                StringAssert.Contains(exception.Message, "requires an explicit initializer", testCase.Id);
                break;
            }

            default:
                Assert.Fail($"Unknown state-default expectation for '{testCase.Id}'.");
                break;
        }
    }

    [TestMethod]
    public void CreateExpression_NullType_ThrowsArgumentNullException()
    {
        var exception = Assert.Throws<ArgumentNullException>(
            () => CurrentComponentStateDefaultInitializer.CreateExpression(null!));

        Assert.AreEqual("type", exception.ParamName);
    }

    private static IReadOnlyDictionary<string, ITypeSymbol> CreateSymbols()
    {
        const string source = """
            using System;
            using System.Numerics;
            using ECMAScript;

            public sealed class ReferenceClass;
            public interface IReference;
            public delegate void Callback();
            public struct ValueStruct { public int Value; }
            public readonly record struct ValueRecord(int Value);

            public enum Int32Enum : int { None }
            public enum Int64Enum : long { None }
            public enum UInt64Enum : ulong { None }

            [String]
            public enum StringEnum { None }

            public sealed class StateDefaultTypes<T>
            {
                public object Object = null!;
                public string String = null!;
                public ReferenceClass Class = null!;
                public IReference Interface = null!;
                public int[] Array = null!;
                public Callback Delegate = null!;
                public int? NullableInt32;
                public bool Boolean;
                public char Char;
                public sbyte SByte;
                public byte Byte;
                public short Int16;
                public ushort UInt16;
                public int Int32;
                public uint UInt32;
                public float Single;
                public double Double;
                public decimal Decimal;
                public Half Half;
                public long Int64;
                public ulong UInt64;
                public Int128 Int128;
                public UInt128 UInt128;
                public BigInteger BigInteger;
                public Int32Enum Int32Enum;
                public Int64Enum Int64Enum;
                public UInt64Enum UInt64Enum;
                public T TypeParameter = default!;
                public ValueStruct ValueStruct;
                public ValueRecord ValueRecord;
                public DateTime DateTime;
                public Guid Guid;
                public (int Number, string Text) Tuple;
                public StringEnum StringEnum;
            }
            """;

        var compilation = CSharpCompilation.Create(
            "CurrentComponentStateDefaultInitializerTests",
            [CSharpSyntaxTree.ParseText(source, TestMetadataReferences.PreviewParseOptions)],
            TestMetadataReferences.Net11.Add(MetadataReference.CreateFromFile(typeof(StringAttribute).Assembly.Location)),
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        var errors = compilation.GetDiagnostics()
            .Where(static diagnostic => diagnostic.Severity == DiagnosticSeverity.Error)
            .ToArray();
        if (errors.Length > 0)
        {
            throw new InvalidOperationException(
                string.Join(Environment.NewLine, errors.Select(static diagnostic => diagnostic.ToString())));
        }

        var container = compilation.GetTypeByMetadataName("StateDefaultTypes`1")
            ?? throw new InvalidOperationException("StateDefaultTypes<T> was not compiled.");
        return container.GetMembers()
            .OfType<IFieldSymbol>()
            .ToDictionary(static field => field.Name, static field => field.Type, StringComparer.Ordinal);
    }
}

public abstract record StateDefaultExpectation
{
    public sealed record JavaScript(string Text) : StateDefaultExpectation;

    public sealed record Unsupported : StateDefaultExpectation;
}

public sealed record StateDefaultCase(
    string Id,
    string Dimension,
    string SymbolName,
    StateDefaultExpectation Expectation);

internal static class StateDefaultCaseCatalog
{
    private static readonly StateDefaultExpectation Null = new StateDefaultExpectation.JavaScript("null");
    private static readonly StateDefaultExpectation NumberZero = new StateDefaultExpectation.JavaScript("0");
    private static readonly StateDefaultExpectation BigIntZero = new StateDefaultExpectation.JavaScript("0n");
    private static readonly StateDefaultExpectation Unsupported = new StateDefaultExpectation.Unsupported();

    public static IReadOnlyList<StateDefaultCase> All { get; } =
    [
        Case("state-default.reference.object", "reference-type", "Object", Null),
        Case("state-default.reference.string", "reference-type", "String", Null),
        Case("state-default.reference.class", "reference-type", "Class", Null),
        Case("state-default.reference.interface", "reference-type", "Interface", Null),
        Case("state-default.reference.array", "reference-type", "Array", Null),
        Case("state-default.reference.delegate", "reference-type", "Delegate", Null),
        Case("state-default.nullable.int32", "nullable-value-type", "NullableInt32", Null),
        Case("state-default.scalar.boolean", "boolean-scalar", "Boolean", new StateDefaultExpectation.JavaScript("false")),
        Case("state-default.scalar.char", "character-scalar", "Char", new StateDefaultExpectation.JavaScript("\"\\0\"")),
        Case("state-default.number.sbyte", "number-scalar", "SByte", NumberZero),
        Case("state-default.number.byte", "number-scalar", "Byte", NumberZero),
        Case("state-default.number.int16", "number-scalar", "Int16", NumberZero),
        Case("state-default.number.uint16", "number-scalar", "UInt16", NumberZero),
        Case("state-default.number.int32", "number-scalar", "Int32", NumberZero),
        Case("state-default.number.uint32", "number-scalar", "UInt32", NumberZero),
        Case("state-default.number.single", "number-scalar", "Single", NumberZero),
        Case("state-default.number.double", "number-scalar", "Double", NumberZero),
        Case("state-default.number.decimal", "number-scalar", "Decimal", NumberZero),
        Case("state-default.number.half", "half-scalar", "Half", NumberZero),
        Case("state-default.bigint.int64", "bigint-scalar", "Int64", BigIntZero),
        Case("state-default.bigint.uint64", "bigint-scalar", "UInt64", BigIntZero),
        Case("state-default.bigint.int128", "bigint-mapped-value-type", "Int128", BigIntZero),
        Case("state-default.bigint.uint128", "bigint-mapped-value-type", "UInt128", BigIntZero),
        Case("state-default.bigint.biginteger", "bigint-mapped-value-type", "BigInteger", BigIntZero),
        Case("state-default.enum.int32", "numeric-enum", "Int32Enum", NumberZero),
        Case("state-default.enum.int64", "bigint-enum", "Int64Enum", BigIntZero),
        Case("state-default.enum.uint64", "bigint-enum", "UInt64Enum", BigIntZero),
        Case("state-default.unsupported.type-parameter", "unsupported-open-type", "TypeParameter", Unsupported),
        Case("state-default.unsupported.struct", "unsupported-value-shape", "ValueStruct", Unsupported),
        Case("state-default.unsupported.record-struct", "unsupported-value-shape", "ValueRecord", Unsupported),
        Case("state-default.unsupported.datetime", "unsupported-runtime-carrier", "DateTime", Unsupported),
        Case("state-default.unsupported.guid", "unsupported-value-shape", "Guid", Unsupported),
        Case("state-default.unsupported.tuple", "unsupported-erased-composition", "Tuple", Unsupported),
        Case("state-default.unsupported.string-enum", "unsupported-string-enum", "StringEnum", Unsupported)
    ];

    private static StateDefaultCase Case(
        string id,
        string dimension,
        string symbolName,
        StateDefaultExpectation expectation)
        => new(id, dimension, symbolName, expectation);
}
