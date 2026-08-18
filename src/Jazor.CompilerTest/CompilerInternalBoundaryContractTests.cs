using Acornima;
using Acornima.Ast;
using Jazor.Common;
using Jazor.Compiler;
using Jazor.ComplierTest;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;
using System.Reflection;

namespace Jazor.CompilerTest;

[TestClass]
public sealed class CompilerInternalBoundaryContractTests
{
    [TestMethod]
    public void TupleLikeHostClassifier_DistinguishesTupleSyntaxUnderlyingCarrierAndOrdinaryTypes()
    {
        var compilation = CreateCompilation(
            """
            public sealed class Holder
            {
                public (int Count, string Label) Tuple;
            }
            """);
        var tuple = ((INamedTypeSymbol)compilation.GetTypeByMetadataName("Holder")!
            .GetMembers("Tuple")
            .OfType<IFieldSymbol>()
            .Single()
            .Type);
        var tupleCarrier = tuple.TupleUnderlyingType!;
        var stringType = compilation.GetSpecialType(SpecialType.System_String);
        var arrayType = compilation.CreateArrayTypeSymbol(compilation.GetSpecialType(SpecialType.System_Int32));
        var method = GetSemanticWalkerStaticMethod("IsTupleLikeHost");

        Assert.IsTrue(Invoke<bool>(method, null, tuple));
        Assert.IsTrue(Invoke<bool>(method, null, tupleCarrier));
        Assert.IsFalse(Invoke<bool>(method, null, stringType));
        Assert.IsFalse(Invoke<bool>(method, null, arrayType));
    }

    [TestMethod]
    public void SystemCarrierNameClassifiers_RejectShadowedNamesAndRetainOnlyClrHosts()
    {
        var compilation = CreateCompilation(
            """
            namespace Shadow
            {
                public struct Half { }
                public struct Index { }
                public struct Range { }
                public struct ValueTuple<T> { }
            }

            public sealed class Generic<Half>
            {
            }

            public sealed class Holder
            {
                public (int Count, string Label) Pair;
            }
            """);
        var isHalf = GetSemanticWalkerStaticMethod("IsSystemHalfType");
        var isIndex = GetSemanticWalkerStaticMethod("IsSystemIndexType");
        var isRange = GetSemanticWalkerStaticMethod("IsSystemRangeType");
        var isTupleLike = GetSemanticWalkerStaticMethod("IsTupleLikeHost");
        var genericParameter = compilation.GetTypeByMetadataName("Generic`1")!.TypeParameters.Single();
        var pair = (INamedTypeSymbol)((IFieldSymbol)compilation.GetTypeByMetadataName("Holder")!
            .GetMembers("Pair")
            .Single()).Type;
        var tupleCarrier = pair.TupleUnderlyingType!;

        Assert.IsTrue(Invoke<bool>(isHalf, null, compilation.GetTypeByMetadataName("System.Half")!));
        Assert.IsTrue(Invoke<bool>(isIndex, null, compilation.GetTypeByMetadataName("System.Index")!));
        Assert.IsTrue(Invoke<bool>(isRange, null, compilation.GetTypeByMetadataName("System.Range")!));
        Assert.IsTrue(Invoke<bool>(isTupleLike, null, pair));
        Assert.IsTrue(Invoke<bool>(isTupleLike, null, tupleCarrier));

        Assert.IsFalse(Invoke<bool>(isHalf, null, compilation.GetTypeByMetadataName("Shadow.Half")!));
        Assert.IsFalse(Invoke<bool>(isIndex, null, compilation.GetTypeByMetadataName("Shadow.Index")!));
        Assert.IsFalse(Invoke<bool>(isRange, null, compilation.GetTypeByMetadataName("Shadow.Range")!));
        Assert.IsFalse(Invoke<bool>(isTupleLike, null, compilation.GetTypeByMetadataName("Shadow.ValueTuple`1")!));
        Assert.IsFalse(Invoke<bool>(isHalf, null, genericParameter));
        Assert.IsFalse(Invoke<bool>(isIndex, null, genericParameter));
        Assert.IsFalse(Invoke<bool>(isRange, null, genericParameter));
        Assert.IsFalse(Invoke<bool>(isTupleLike, null, genericParameter));
        Assert.IsFalse(Invoke<bool>(isHalf, null, (object?)null));
        Assert.IsFalse(Invoke<bool>(isIndex, null, (object?)null));
        Assert.IsFalse(Invoke<bool>(isRange, null, (object?)null));
    }

    [TestMethod]
    public void CreationFallbackClassifiers_RestrictErrorVueDictionaryAndInheritedStructuralContracts()
    {
        var compilation = CreateCompilation(
            """
            namespace ECMAScript.Vue
            {
                public class VueDictionary
                {
                }

                public class VueDictionary<TValue>
                {
                }

                public sealed class DerivedDictionary : VueDictionary<int>
                {
                }
            }

            public sealed class ErrorLike
            {
            }

            public class BaseRecord
            {
                public int Value { get; }
            }

            public sealed class DerivedRecord : BaseRecord
            {
                public static int Missing { get; }

                public void BindValue(int value)
                {
                }

                public void BindMissing(int missing)
                {
                }
            }
            """);
        var isNativeError = GetSemanticWalkerStaticMethod("IsNativeErrorConstructorFallbackAllowed");
        var isVueDictionary = GetSemanticWalkerStaticMethod("IsVueDictionaryHostType");
        var resolveStructuralProperty = GetSemanticWalkerStaticMethod("ResolveStructuralRuntimeProperty");
        var exceptionType = compilation.GetTypeByMetadataName("System.Exception")!;
        var errorLike = compilation.GetTypeByMetadataName("ErrorLike")!;
        var dictionary = compilation.GetTypeByMetadataName("ECMAScript.Vue.VueDictionary")!;
        var genericDictionary = compilation.GetTypeByMetadataName("ECMAScript.Vue.VueDictionary`1")!;
        var derivedDictionary = compilation.GetTypeByMetadataName("ECMAScript.Vue.DerivedDictionary")!;
        var derivedRecord = compilation.GetTypeByMetadataName("DerivedRecord")!;
        var baseValue = compilation.GetTypeByMetadataName("BaseRecord")!
            .GetMembers("Value")
            .OfType<IPropertySymbol>()
            .Single();
        var valueParameter = derivedRecord.GetMembers("BindValue")
            .OfType<IMethodSymbol>()
            .Single()
            .Parameters
            .Single();
        var missingParameter = derivedRecord.GetMembers("BindMissing")
            .OfType<IMethodSymbol>()
            .Single()
            .Parameters
            .Single();

        Assert.IsTrue(Invoke<bool>(isNativeError, null, exceptionType, "Error"));
        Assert.IsTrue(Invoke<bool>(isNativeError, null, exceptionType, "TypeError"));
        Assert.IsFalse(Invoke<bool>(isNativeError, null, exceptionType, "RangeError"));
        Assert.IsFalse(Invoke<bool>(isNativeError, null, errorLike, "Error"));

        Assert.IsTrue(Invoke<bool>(isVueDictionary, null, dictionary));
        Assert.IsTrue(Invoke<bool>(isVueDictionary, null, genericDictionary.Construct(compilation.GetSpecialType(SpecialType.System_Int32))));
        Assert.IsTrue(Invoke<bool>(isVueDictionary, null, derivedDictionary));
        Assert.IsFalse(Invoke<bool>(isVueDictionary, null, errorLike));
        Assert.IsFalse(Invoke<bool>(isVueDictionary, null, genericDictionary.TypeParameters.Single()));

        var resolved = Invoke<IPropertySymbol?>(resolveStructuralProperty, null, derivedRecord, valueParameter);
        Assert.IsTrue(SymbolEqualityComparer.Default.Equals(baseValue, resolved));
        Assert.IsNull(Invoke<IPropertySymbol?>(resolveStructuralProperty, null, derivedRecord, missingParameter));
    }

    [TestMethod]
    public void RuntimeAliasAndMetadataHostClassifiers_UseBoundInheritanceAndTypeIdentity()
    {
        var compilation = CreateCompilation(
            """
            public interface IContract
            {
            }

            public class BaseHost
            {
            }

            public sealed class DerivedHost : BaseHost, IContract
            {
            }

            public sealed class OtherHost
            {
            }

            public enum State
            {
                None
            }

            public delegate void Callback();

            public sealed class Generic<T>
            {
            }

            public sealed class Holder
            {
                public (int Count, string Label) Pair;
            }
            """);
        var isRuntimeAliasAssignable = GetSemanticWalkerStaticMethod("IsRuntimeAliasAssignableToTarget");
        var isConcreteMetadataHost = GetSemanticWalkerStaticMethod("IsConcreteMetadataInteropHost");
        var eraseGenericArguments = GetSemanticWalkerStaticMethod("EraseGenericDisplayArguments");
        var baseHost = compilation.GetTypeByMetadataName("BaseHost")!;
        var derivedHost = compilation.GetTypeByMetadataName("DerivedHost")!;
        var otherHost = compilation.GetTypeByMetadataName("OtherHost")!;
        var contract = compilation.GetTypeByMetadataName("IContract")!;
        var genericParameter = compilation.GetTypeByMetadataName("Generic`1")!.TypeParameters.Single();
        var tuple = ((IFieldSymbol)compilation.GetTypeByMetadataName("Holder")!
            .GetMembers("Pair")
            .Single()).Type;

        Assert.IsTrue(Invoke<bool>(isRuntimeAliasAssignable, null, derivedHost, baseHost));
        Assert.IsTrue(Invoke<bool>(isRuntimeAliasAssignable, null, derivedHost, contract));
        Assert.IsFalse(Invoke<bool>(isRuntimeAliasAssignable, null, otherHost, baseHost));

        Assert.AreEqual("System.Collections.Generic.Dictionary", Invoke<string>(
            eraseGenericArguments,
            null,
            "System.Collections.Generic.Dictionary<System.String, System.Collections.Generic.List<System.Int32>>"));
        Assert.AreEqual("PlainName", Invoke<string>(eraseGenericArguments, null, "PlainName"));

        Assert.IsTrue(Invoke<bool>(isConcreteMetadataHost, null, derivedHost));
        Assert.IsFalse(Invoke<bool>(isConcreteMetadataHost, null, compilation.GetSpecialType(SpecialType.System_Int32)));
        Assert.IsFalse(Invoke<bool>(isConcreteMetadataHost, null, compilation.GetTypeByMetadataName("State")!));
        Assert.IsFalse(Invoke<bool>(isConcreteMetadataHost, null, compilation.GetTypeByMetadataName("Callback")!));
        Assert.IsFalse(Invoke<bool>(isConcreteMetadataHost, null, genericParameter));
        Assert.IsFalse(Invoke<bool>(isConcreteMetadataHost, null, tuple));
    }

    [TestMethod]
    public void StringEnumAndComputedAliasHelpers_PreserveAuthoredRuntimeNames()
    {
        var compilation = CreateCompilation(
            """
            namespace ECMAScript
            {
                [global::System.AttributeUsage(global::System.AttributeTargets.Enum)]
                public sealed class StringAttribute : global::System.Attribute
                {
                }

                [global::System.AttributeUsage(global::System.AttributeTargets.Field | global::System.AttributeTargets.Method | global::System.AttributeTargets.Property)]
                public sealed class ECMAScriptNameAttribute : global::System.Attribute
                {
                    public ECMAScriptNameAttribute(string value)
                    {
                    }
                }
            }

            [ECMAScript.String]
            public enum State
            {
                [ECMAScript.ECMAScriptName("ready-state")]
                Ready = 1,

                [global::System.ComponentModel.Description("@#busy-state")]
                Busy = 2,

                [global::System.ComponentModel.Description("configured-state")]
                Configured = 4,

                Plain = 3
            }

            public enum Ordinary
            {
                Value = 1
            }

            public sealed class Holder
            {
                public const int Constant = 1;

                public int Read()
                {
                    return 1;
                }
            }
            """);
        var state = compilation.GetTypeByMetadataName("State")!;
        var ordinary = compilation.GetTypeByMetadataName("Ordinary")!;
        var ready = state.GetMembers("Ready").OfType<IFieldSymbol>().Single();
        var busy = state.GetMembers("Busy").OfType<IFieldSymbol>().Single();
        var plain = state.GetMembers("Plain").OfType<IFieldSymbol>().Single();
        var configured = state.GetMembers("Configured").OfType<IFieldSymbol>().Single();
        var ordinaryValue = ordinary.GetMembers("Value").OfType<IFieldSymbol>().Single();
        var ordinaryConstant = compilation.GetTypeByMetadataName("Holder")!
            .GetMembers("Constant")
            .OfType<IFieldSymbol>()
            .Single();

        var literalMethod = typeof(SemanticWalker).GetMethod(
            "TryBuildStringEnumLiteral",
            BindingFlags.Static | BindingFlags.NonPublic)!;
        var valueMethod = typeof(SemanticWalker).GetMethod(
            "TryBuildStringEnumValueLiteral",
            BindingFlags.Static | BindingFlags.NonPublic)!;
        var literalTextMethod = typeof(SemanticWalker).GetMethod(
            "GetStringEnumLiteralText",
            BindingFlags.Static | BindingFlags.NonPublic)!;
        var parseAliasMethod = typeof(SemanticWalker).GetMethod(
            "TryParseExplicitComputedAliasProperty",
            BindingFlags.Static | BindingFlags.NonPublic)!;

        Assert.AreEqual("ready-state", Invoke<string>(literalTextMethod, null, ready));
        Assert.AreEqual("busy-state", Invoke<string>(literalTextMethod, null, busy));
        Assert.AreEqual("Configured", Invoke<string>(literalTextMethod, null, configured));
        Assert.AreEqual("Plain", Invoke<string>(literalTextMethod, null, plain));

        var readyArguments = new object?[] { ready, null };
        Assert.IsTrue((bool)literalMethod.Invoke(null, readyArguments)!);
        Assert.AreEqual("\"ready-state\"", ((Expression)readyArguments[1]!).ToKnRECMAScript());
        var ordinaryArguments = new object?[] { ordinaryValue, null };
        Assert.IsFalse((bool)literalMethod.Invoke(null, ordinaryArguments)!);
        var ordinaryFieldArguments = new object?[] { ordinaryConstant, null };
        Assert.IsFalse((bool)literalMethod.Invoke(null, ordinaryFieldArguments)!);

        var matchingArguments = new object?[] { state, 2, null };
        Assert.IsTrue((bool)valueMethod.Invoke(null, matchingArguments)!);
        Assert.AreEqual("\"busy-state\"", ((Expression)matchingArguments[2]!).ToKnRECMAScript());
        var nonMatchingArguments = new object?[] { state, 99, null };
        Assert.IsFalse((bool)valueMethod.Invoke(null, nonMatchingArguments)!);
        var ordinaryValueArguments = new object?[] { ordinary, 1, null };
        Assert.IsFalse((bool)valueMethod.Invoke(null, ordinaryValueArguments)!);

        AssertComputedAlias("[3]", "3", "3");
        AssertComputedAlias("[\"quoted\"]", "quoted", "\"quoted\"");
        AssertComputedAlias("[unknown]", null, null);
        AssertComputedAlias("[ab]", null, null);
        AssertComputedAlias("plain", null, null);

        void AssertComputedAlias(string authored, string? expectedKey, string? expectedExpression)
        {
            var arguments = new object?[] { authored, null, null };
            var result = (bool)parseAliasMethod.Invoke(null, arguments)!;
            Assert.AreEqual(expectedKey is not null, result, authored);
            if (expectedKey is null)
                return;

            Assert.AreEqual(expectedKey, arguments[2]);
            Assert.AreEqual(expectedExpression, ((Expression)arguments[1]!).ToKnRECMAScript());
        }
    }

    [TestMethod]
    public void RuntimeMemberHostResolver_PreservesBoundInstanceStaticAndCreationHosts()
    {
        var (compilation, block) = CreateBlock(
            """
            public sealed class Host
            {
                public int Value;
                public static int StaticValue;
                public int Property => Value;
                public static int StaticProperty => StaticValue;
                public int Method() => Value;
                public static int StaticMethod() => StaticValue;
            }

            public sealed class TestClass
            {
                void TestMethod()
                {
                    var host = new Host();
                    var instanceCall = host.Method();
                    var staticCall = Host.StaticMethod();
                    System.Func<int> methodGroup = host.Method;
                    var instanceProperty = host.Property;
                    var staticProperty = Host.StaticProperty;
                    var instanceField = host.Value;
                    var staticField = Host.StaticValue;
                    var created = new Host();
                    var scalar = 42;
                }
            }
            """);
        var host = compilation.GetTypeByMetadataName("Host")!;
        var resolver = GetSemanticWalkerStaticMethod("TryGetRuntimeMemberHostType");
        var operations = block.DescendantsAndSelf();

        AssertHost(operations.OfType<IInvocationOperation>().Single(static operation => operation.TargetMethod.Name == "Method"));
        AssertHost(operations.OfType<IInvocationOperation>().Single(static operation => operation.TargetMethod.Name == "StaticMethod"));
        AssertHost(operations.OfType<IMethodReferenceOperation>().Single());
        AssertHost(operations.OfType<IPropertyReferenceOperation>().Single(static operation => operation.Property.Name == "Property"));
        AssertHost(operations.OfType<IPropertyReferenceOperation>().Single(static operation => operation.Property.Name == "StaticProperty"));
        AssertHost(operations.OfType<IFieldReferenceOperation>().Single(static operation => operation.Field.Name == "Value"));
        AssertHost(operations.OfType<IFieldReferenceOperation>().Single(static operation => operation.Field.Name == "StaticValue"));
        AssertHost(operations.OfType<IObjectCreationOperation>().Last(static operation => operation.Type?.Name == "Host"));
        Assert.IsNull(Invoke<ITypeSymbol?>(resolver, null, operations.OfType<ILiteralOperation>().Single(static operation => operation.ConstantValue.Value is 42)));
        Assert.IsNull(Invoke<ITypeSymbol?>(resolver, null, (object?)null));

        void AssertHost(IOperation operation)
        {
            var result = Invoke<ITypeSymbol?>(resolver, null, operation);
            Assert.IsTrue(SymbolEqualityComparer.Default.Equals(host, result), operation.Kind.ToString());
        }
    }

    [TestMethod]
    public void SourceBoundaryResolver_WalksNestedSymbolsBackToTheirTopLevelType()
    {
        var compilation = CreateCompilation(
            """
            public sealed class Outer
            {
                public int Field;

                public sealed class Inner
                {
                    public void Execute(int value)
                    {
                    }
                }
            }
            """);
        var outer = compilation.GetTypeByMetadataName("Outer")!;
        var inner = outer.GetTypeMembers("Inner").Single();
        var field = outer.GetMembers("Field").OfType<IFieldSymbol>().Single();
        var parameter = inner.GetMembers("Execute").OfType<IMethodSymbol>().Single().Parameters.Single();
        var resolver = GetSemanticWalkerStaticMethod("GetTopMostContainingType");

        AssertTopLevel(outer);
        AssertTopLevel(inner);
        AssertTopLevel(field);
        AssertTopLevel(parameter);
        Assert.IsNull(Invoke<INamedTypeSymbol?>(resolver, null, (object?)null));

        void AssertTopLevel(ISymbol symbol)
            => Assert.IsTrue(SymbolEqualityComparer.Default.Equals(outer, Invoke<INamedTypeSymbol?>(resolver, null, symbol)));
    }

    [TestMethod]
    public void NativeMapSetEqualityContract_ClassifiesEverySupportedRuntimeCarrier()
    {
        var compilation = CreateCompilation(
            """
            using System;

            public enum State
            {
                None
            }

            public interface IContract
            {
            }

            public struct ValueStruct
            {
                public int Value;
            }

            public delegate void Callback();

            public record RecordValue(int Value);

            public sealed class CustomEquality : IEquatable<CustomEquality>
            {
                public bool Equals(CustomEquality? other) => true;
                public override bool Equals(object? other) => true;
                public override int GetHashCode() => 0;
            }

            public sealed class OverrideEquality
            {
                public override bool Equals(object? other) => true;
                public override int GetHashCode() => 0;
            }

            public sealed class PlainReference
            {
            }

            public sealed class Generic<T>
            {
            }

            public sealed class Holder
            {
                public (int Count, string Label) Tuple;
            }
            """);
        var method = typeof(SemanticWalker).GetMethod(
            "HasJsStableNativeMapSetEquality",
            BindingFlags.Instance | BindingFlags.NonPublic)!;
        var walker = new SemanticWalker(true);
        var intType = compilation.GetSpecialType(SpecialType.System_Int32);
        var nullableInt = compilation.GetSpecialType(SpecialType.System_Nullable_T).Construct(intType);
        var tuple = ((IFieldSymbol)compilation.GetTypeByMetadataName("Holder")!
            .GetMembers("Tuple")
            .Single()).Type;
        var genericParameter = compilation.GetTypeByMetadataName("Generic`1")!.TypeParameters.Single();

        AssertStable(intType);
        AssertStable(nullableInt);
        AssertStable(compilation.GetSpecialType(SpecialType.System_String));
        AssertStable(compilation.GetSpecialType(SpecialType.System_Boolean));
        AssertStable(compilation.GetSpecialType(SpecialType.System_Int64));
        AssertStable(compilation.GetTypeByMetadataName("State")!);
        AssertStable(compilation.CreateArrayTypeSymbol(intType));
        AssertStable(compilation.GetTypeByMetadataName("PlainReference")!);

        AssertUnstable(tuple, "Tuple");
        AssertUnstable(genericParameter, "Type-parameter");
        AssertUnstable(compilation.GetSpecialType(SpecialType.System_Object), "object");
        AssertUnstable(compilation.GetTypeByMetadataName("IContract")!, "Interface");
        AssertUnstable(compilation.GetTypeByMetadataName("ValueStruct")!, "Struct");
        AssertUnstable(compilation.GetTypeByMetadataName("Callback")!, "Delegate");
        AssertUnstable(compilation.GetTypeByMetadataName("RecordValue")!, "record");
        AssertUnstable(compilation.GetTypeByMetadataName("CustomEquality")!, "custom");
        AssertUnstable(compilation.GetTypeByMetadataName("OverrideEquality")!, "custom");

        void AssertStable(ITypeSymbol type)
        {
            var (stable, reason) = InvokeNativeMapSetEquality(method, walker, type);
            Assert.IsTrue(stable, type.ToDisplayString());
            Assert.AreEqual(string.Empty, reason, type.ToDisplayString());
        }

        void AssertUnstable(ITypeSymbol type, string reasonFragment)
        {
            var (stable, reason) = InvokeNativeMapSetEquality(method, walker, type);
            Assert.IsFalse(stable, type.ToDisplayString());
            StringAssert.Contains(reason, reasonFragment, StringComparison.OrdinalIgnoreCase);
        }
    }

    [TestMethod]
    public void NativeMapSetEqualitySurface_ExtractsKeyAndElementContractsOnly()
    {
        var compilation = CreateCompilation("public sealed class Marker { }");
        var method = GetSemanticWalkerStaticMethod("TryGetNativeMapSetEqualitySurface");
        var intType = compilation.GetSpecialType(SpecialType.System_Int32);
        var stringType = compilation.GetSpecialType(SpecialType.System_String);

        AssertSurface("System.Collections.Generic.Dictionary`2", intType, stringType, "key", intType);
        AssertSurface("System.Collections.Generic.IDictionary`2", intType, stringType, "key", intType);
        AssertSurface("System.Collections.ObjectModel.ReadOnlyDictionary`2", intType, stringType, "key", intType);
        AssertSurface("System.Collections.Generic.HashSet`1", intType, null, "element", intType);
        AssertSurface("System.Collections.Generic.ISet`1", intType, null, "element", intType);
        AssertSurface("System.Collections.ObjectModel.ReadOnlySet`1", intType, null, "element", intType);

        var noSurfaceArguments = new object?[] { stringType, null, null };
        Assert.IsFalse((bool)method.Invoke(null, noSurfaceArguments)!);
        Assert.IsNull(noSurfaceArguments[1]);
        Assert.AreEqual(string.Empty, noSurfaceArguments[2]);

        void AssertSurface(
            string metadataName,
            ITypeSymbol first,
            ITypeSymbol? second,
            string expectedRole,
            ITypeSymbol expectedEqualityType)
        {
            var definition = compilation.GetTypeByMetadataName(metadataName)!;
            var type = second is null
                ? definition.Construct(first)
                : definition.Construct(first, second);
            var arguments = new object?[] { type, null, null };

            Assert.IsTrue((bool)method.Invoke(null, arguments)!, metadataName);
            Assert.IsTrue(SymbolEqualityComparer.Default.Equals(expectedEqualityType, (ITypeSymbol)arguments[1]!));
            Assert.AreEqual(expectedRole, arguments[2]);
        }
    }

    [TestMethod]
    public void CompoundAssignmentOperatorTable_MapsEveryBoundCSharpOperatorAndRejectsUnknownKinds()
    {
        var method = GetSemanticWalkerStaticMethod("GetCompoundAssignmentOperators");
        var cases = new (BinaryOperatorKind Kind, (Operator Assignment, Operator Binary) Expected)[]
        {
            (BinaryOperatorKind.Add, (Operator.AdditionAssignment, Operator.Addition)),
            (BinaryOperatorKind.Subtract, (Operator.SubtractionAssignment, Operator.Subtraction)),
            (BinaryOperatorKind.Multiply, (Operator.MultiplicationAssignment, Operator.Multiplication)),
            (BinaryOperatorKind.Divide, (Operator.DivisionAssignment, Operator.Division)),
            (BinaryOperatorKind.Remainder, (Operator.RemainderAssignment, Operator.Remainder)),
            (BinaryOperatorKind.And, (Operator.BitwiseAndAssignment, Operator.BitwiseAnd)),
            (BinaryOperatorKind.Or, (Operator.BitwiseOrAssignment, Operator.BitwiseOr)),
            (BinaryOperatorKind.ExclusiveOr, (Operator.BitwiseXorAssignment, Operator.BitwiseXor)),
            (BinaryOperatorKind.LeftShift, (Operator.LeftShiftAssignment, Operator.LeftShift)),
            (BinaryOperatorKind.RightShift, (Operator.RightShiftAssignment, Operator.RightShift)),
            (BinaryOperatorKind.UnsignedRightShift, (Operator.UnsignedRightShiftAssignment, Operator.UnsignedRightShift))
        };

        foreach (var testCase in cases)
            Assert.AreEqual(testCase.Expected, Invoke<object>(method, null, testCase.Kind), testCase.Kind.ToString());

        var exception = Assert.Throws<TargetInvocationException>(() =>
            method.Invoke(null, [(BinaryOperatorKind)int.MaxValue]));
        Assert.IsInstanceOfType<InvalidOperationException>(exception.InnerException);
    }

    [TestMethod]
    public void TypeOfWhitelistCompilerHandler_RequiresExactlyOneExplicitExpression()
    {
        var compilation = CreateCompilation("public sealed class Marker { }");
        var walker = new SemanticWalker(true);
        var value = new Identifier("value");
        var result = walker.Compile_27d71701fd254382(
            compilation.GetTypeByMetadataName("Marker")!,
            new SenseArgument(),
            handler: null,
            args: [value],
            originOperation: null);

        Assert.AreEqual("typeof value", result?.ToKnRECMAScript());
        Assert.Throws<InvalidOperationException>(() => walker.Compile_27d71701fd254382(
            compilation.GetTypeByMetadataName("Marker")!,
            new SenseArgument(),
            handler: new Identifier("receiver"),
            args: [value],
            originOperation: null));
        Assert.Throws<InvalidOperationException>(() => walker.Compile_27d71701fd254382(
            compilation.GetTypeByMetadataName("Marker")!,
            new SenseArgument(),
            handler: null,
            args: [],
            originOperation: null));
        Assert.Throws<InvalidOperationException>(() => walker.Compile_27d71701fd254382(
            compilation.GetTypeByMetadataName("Marker")!,
            new SenseArgument(),
            handler: null,
            args: [null],
            originOperation: null));
        Assert.Throws<InvalidOperationException>(() => walker.Compile_27d71701fd254382(
            compilation.GetTypeByMetadataName("Marker")!,
            new SenseArgument(),
            handler: null,
            args: [value, new Identifier("other")],
            originOperation: null));
    }

    [TestMethod]
    public void ObjectInitializerMemberClassifier_RecognizesAssignmentsNestedMembersAndCollectionEntries()
    {
        var (_, block) = CreateBlock(
            """
            public sealed class Child
            {
                public int Value { get; set; }
            }

            public sealed class Holder
            {
                public int Count { get; set; }
                public Child Child { get; } = new();
            }

            public sealed class TestClass
            {
                static void TestMethod()
                {
                    var holder = new Holder { Count = 1, Child = { Value = 2 } };
                    var values = new System.Collections.Generic.List<int> { 3 };
                }
            }
            """);
        var holderInitializer = block.DescendantsAndSelf()
            .OfType<IObjectCreationOperation>()
            .Single(static operation => operation.Type?.Name == "Holder")
            .Initializer!;
        var collectionInitializer = block.DescendantsAndSelf()
            .OfType<IObjectCreationOperation>()
            .Single(static operation => operation.Type?.Name == "List")
            .Initializer!;
        var method = GetSemanticWalkerStaticMethod("GetObjectInitializerMemberSymbol");

        var count = Invoke<ISymbol?>(method, null, holderInitializer.Initializers[0]);
        var child = Invoke<ISymbol?>(method, null, holderInitializer.Initializers[1]);
        var add = Invoke<ISymbol?>(method, null, collectionInitializer.Initializers.Single());

        Assert.AreEqual("Count", count?.Name);
        Assert.AreEqual("Child", child?.Name);
        Assert.IsNull(add);
    }

    [TestMethod]
    public void TypeNameResolution_UsesImportsOnlyWhenAnImportContextIsAvailable()
    {
        var (compilation, _) = CreateBlock(
            """
            using System;

            namespace ECMAScript
            {
                [AttributeUsage(AttributeTargets.Class, Inherited = false)]
                public sealed class ECMAScriptModuleAttribute : Attribute
                {
                    public ECMAScriptModuleAttribute(string import) { }
                }
            }

            namespace Demo
            {
                [ECMAScript.ECMAScriptModule("./runtime.mjs")]
                public sealed class ExternalRuntime
                {
                }

                public sealed class TestClass
                {
                    void TestMethod()
                    {
                    }
                }
            }
            """);
        var externalRuntime = compilation.GetTypeByMetadataName("Demo.ExternalRuntime")!;
        var method = typeof(SemanticWalker).GetMethod(
            "BuildFullTypeName",
            BindingFlags.Instance | BindingFlags.NonPublic)!;
        var walker = new SemanticWalker(true);

        var withoutContext = Invoke<Expression?>(method, walker, externalRuntime, null);
        var importContext = new SenseArgument(UseImportAliases: true);
        var withContext = Invoke<Expression?>(method, walker, externalRuntime, importContext);

        Assert.AreEqual("ExternalRuntime", withoutContext?.ToKnRECMAScript());
        Assert.AreEqual("ExternalRuntime", withContext?.ToKnRECMAScript());
        var imports = importContext.FlushImportSpecifiers();
        Assert.HasCount(1, imports);
        Assert.AreEqual("./runtime.mjs", imports[0].Key);
    }

    [TestMethod]
    public void RuntimeNameResolvers_PreserveWhitelistImportConfiguredAndNestedModuleContracts()
    {
        var (compilation, block) = CreateBlock(
            """
            using System;

            [AttributeUsage(AttributeTargets.All, Inherited = false)]
            public sealed class JazorAttribute : Attribute
            {
                public JazorAttribute(int operation, string memberName, string runtimeName)
                {
                }
            }

            [AttributeUsage(AttributeTargets.All, Inherited = false)]
            public sealed class ECMAScriptNameAttribute : Attribute
            {
                public ECMAScriptNameAttribute(string name)
                {
                }
            }

            namespace ECMAScript
            {
                [AttributeUsage(AttributeTargets.Class, Inherited = false)]
                public sealed class ECMAScriptModuleAttribute : Attribute
                {
                    public ECMAScriptModuleAttribute(string import)
                    {
                    }
                }
            }

            namespace Demo
            {
                [ECMAScript.ECMAScriptModule("./runtime.mjs")]
                public static class RuntimeModule
                {
                    public sealed class NestedExport
                    {
                    }
                }

                public sealed class LocalOuter
                {
                    public sealed class LocalNested
                    {
                    }
                }

                public sealed class NamingHost
                {
                    [Jazor(3, "load", "loadRuntime")]
                    public static void Imported()
                    {
                    }

                    [ECMAScriptName("configured")]
                    public static void Configured()
                    {
                    }

                    public static void Plain()
                    {
                    }

                    void TestMethod()
                    {
                    }
                }
            }
            """);
        var host = compilation.GetTypeByMetadataName("Demo.NamingHost")!;
        var console = compilation.GetTypeByMetadataName("System.Console")!;
        var methodNameResolver = GetSemanticWalkerStaticMethod("GetMethodConfigOrWhiteListName");
        var typeAliasResolver = GetSemanticWalkerStaticMethod("TryGetTypeAliasFromWhiteList");
        var typeNameResolver = typeof(SemanticWalker).GetMethod(
            "BuildFullTypeName",
            BindingFlags.Instance | BindingFlags.NonPublic)!;
        var walker = new SemanticWalker(true);

        var writeLine = console.GetMembers("WriteLine")
            .OfType<IMethodSymbol>()
            .Single(method => method.Parameters.Length == 1 &&
                method.Parameters[0].Type.SpecialType == SpecialType.System_String);
        Assert.AreEqual("log", Invoke<string>(methodNameResolver, null, writeLine));
        Assert.AreEqual("loadRuntime", Invoke<string>(methodNameResolver, null, host.GetMembers("Imported").OfType<IMethodSymbol>().Single()));
        Assert.AreEqual("configured", Invoke<string>(methodNameResolver, null, host.GetMembers("Configured").OfType<IMethodSymbol>().Single()));
        Assert.AreEqual("Plain", Invoke<string>(methodNameResolver, null, host.GetMembers("Plain").OfType<IMethodSymbol>().Single()));

        Assert.AreEqual("console", Invoke<string?>(typeAliasResolver, null, "System.Console"));
        Assert.IsNull(Invoke<string?>(typeAliasResolver, null, "System.Nullable<T>"));
        Assert.IsNull(Invoke<string?>(typeAliasResolver, null, "Demo.Unknown"));

        var dictionary = compilation.GetTypeByMetadataName("System.Collections.Generic.Dictionary`2")!
            .Construct(
                compilation.GetSpecialType(SpecialType.System_Int32),
                compilation.GetSpecialType(SpecialType.System_String));
        Assert.AreEqual("Map", Invoke<Expression?>(typeNameResolver, walker, dictionary, null)?.ToKnRECMAScript());

        var nestedExport = compilation.GetTypeByMetadataName("Demo.RuntimeModule+NestedExport")!;
        var importContext = new SenseArgument(UseImportAliases: true);
        Assert.AreEqual("NestedExport", Invoke<Expression?>(typeNameResolver, walker, nestedExport, importContext)?.ToKnRECMAScript());
        var imports = importContext.FlushImportSpecifiers();
        Assert.HasCount(1, imports);
        Assert.AreEqual("./runtime.mjs", imports[0].Key);
        Assert.HasCount(1, imports[0].Value);

        var localNested = compilation.GetTypeByMetadataName("Demo.LocalOuter+LocalNested")!;
        Assert.AreEqual("LocalNested", Invoke<Expression?>(typeNameResolver, walker, localNested, null)?.ToKnRECMAScript());
    }

    [TestMethod]
    public void IntrinsicConversionClassifier_PassesOnlyErasedScalarAndIndexRangeBoundaries()
    {
        var (_, block) = CreateBlock(
            """
            public struct Token
            {
                public static implicit operator Token(int value) => default;
            }

            public sealed class TestClass
            {
                void TestMethod()
                {
                    int number = 1;
                    long integer64 = number;
                    int narrowed = (int)integer64;
                    double floating = number;
                    object boxedNumber = number;
                    System.Index index = number;
                    object boxedIndex = index;
                    System.Range range = 1..2;
                    object boxedRange = range;
                    Token token = number;
                }
            }
            """);
        var method = typeof(SemanticWalker).GetMethod(
            "CanPassThroughIntrinsicConversion",
            BindingFlags.Instance | BindingFlags.NonPublic)!;
        var walker = new SemanticWalker(true);
        var conversions = block.DescendantsAndSelf().OfType<IConversionOperation>().ToArray();

        AssertSpecialConversion(SpecialType.System_Int64, SpecialType.System_Int32, expected: true);
        AssertSpecialConversion(SpecialType.System_Int32, SpecialType.System_Int64, expected: true);
        AssertSpecialConversion(SpecialType.System_Double, SpecialType.System_Int32, expected: true);
        AssertSpecialConversion(SpecialType.System_Object, SpecialType.System_Int32, expected: false);
        AssertNamedTargetConversion("System.Index", SpecialType.System_Int32, expected: true);
        AssertNamedOperandConversion(SpecialType.System_Object, "System.Index", expected: true);
        AssertNamedOperandConversion(SpecialType.System_Object, "System.Range", expected: true);
        AssertNamedTargetConversion("Token", SpecialType.System_Int32, expected: false);

        void AssertSpecialConversion(SpecialType target, SpecialType operand, bool expected)
            => Assert.AreEqual(
                expected,
                Invoke<bool>(
                    method,
                    walker,
                    conversions.Single(conversion =>
                        conversion.Type?.SpecialType == target &&
                        conversion.Operand.Type?.SpecialType == operand)),
                $"{operand} -> {target}");

        void AssertNamedTargetConversion(string targetMetadataName, SpecialType operand, bool expected)
            => Assert.AreEqual(
                expected,
                Invoke<bool>(
                    method,
                    walker,
                    conversions.First(conversion =>
                        conversion.Type?.OriginalDefinition.ToDisplayString() == targetMetadataName &&
                        conversion.Operand.Type?.SpecialType == operand)),
                $"{operand} -> {targetMetadataName}");

        void AssertNamedOperandConversion(SpecialType target, string operandMetadataName, bool expected)
            => Assert.AreEqual(
                expected,
                Invoke<bool>(
                    method,
                    walker,
                    conversions.Single(conversion =>
                        conversion.Type?.SpecialType == target &&
                        conversion.Operand.Type?.OriginalDefinition.ToDisplayString() == operandMetadataName)),
                $"{operandMetadataName} -> {target}");
    }

    [TestMethod]
    public void ExtensionMethodGroups_PreserveMappedAndSourceStaticHosts()
    {
        var (_, runtimeBlock) = CreateBlock(
            """
            using System;

            namespace ECMAScript
            {
                [AttributeUsage(AttributeTargets.Class, Inherited = false)]
                public sealed class ECMAScriptAttribute : Attribute
                {
                }
            }

            [ECMAScript.ECMAScript]
            public sealed class Host
            {
            }

            [ECMAScript.ECMAScript]
            public static class TestClass
            {
                public static int RuntimeValue(this Host host) => 1;

                static void TestMethod()
                {
                    Host host = new();
                    Func<int> value = host.RuntimeValue;
                }
            }
            """);
        var (_, sourceBlock) = CreateBlock(
            """
            using System;

            public sealed class Host
            {
            }

            public static class TestClass
            {
                public static int LocalValue(this Host host) => 1;

                static void TestMethod()
                {
                    Host host = null!;
                    Func<int> value = host.LocalValue;
                }
            }
            """);

        var runtimeReference = runtimeBlock.DescendantsAndSelf().OfType<IMethodReferenceOperation>().Single();
        var sourceReference = sourceBlock.DescendantsAndSelf().OfType<IMethodReferenceOperation>().Single();
        var runtimeScript = new SemanticWalker(true).Visit(runtimeReference, new SenseArgument())?.ToKnRECMAScript();
        var sourceScript = new SemanticWalker(true).Visit(sourceReference, new SenseArgument())?.ToKnRECMAScript();

        Assert.IsNotNull(runtimeScript);
        Assert.IsNotNull(sourceScript);
        StringAssert.Contains(runtimeScript, "TestClass.RuntimeValue");
        StringAssert.Contains(sourceScript, "Host.LocalValue");
        _ = new Parser().ParseExpression($"({runtimeScript})");
        _ = new Parser().ParseExpression($"({sourceScript})");
    }

    [TestMethod]
    public void DirectExternalTypeSupport_RecognizesSourceModuleMarkerBaseAndWhitelistContracts()
    {
        var compilation = CreateCompilation(
            """
            using System;

            namespace ECMAScript
            {
                [AttributeUsage(AttributeTargets.Class, Inherited = false)]
                public sealed class ECMAScriptAttribute : Attribute
                {
                }
            }

            public sealed class Unmapped
            {
            }

            [ECMAScript.ECMAScript]
            public class Marked
            {
            }

            public sealed class Derived : Marked
            {
            }

            public static class Boundary
            {
                public sealed class Nested
                {
                }

                public sealed class Registered
                {
                }
            }
            """);
        var boundary = compilation.GetTypeByMetadataName("Boundary")!;
        var registered = boundary.GetTypeMembers("Registered").Single();
        var declaredNames = new Dictionary<ISymbol, string>(SymbolEqualityComparer.Default)
        {
            [registered] = "Registered"
        };
        var walker = new SemanticWalker(boundary, declaredNames);
        var method = typeof(SemanticWalker).GetMethods(BindingFlags.Instance | BindingFlags.NonPublic)
            .Single(candidate => candidate.Name == "IsDirectlySupportedExternalType" &&
                candidate.GetParameters() is var parameters &&
                parameters.Length == 2 &&
                parameters[0].ParameterType == typeof(ITypeSymbol));

        Assert.IsFalse(Invoke<bool>(method, walker, compilation.GetTypeByMetadataName("Unmapped")!, null));
        Assert.IsTrue(Invoke<bool>(method, walker, boundary.GetTypeMembers("Nested").Single(), boundary));
        Assert.IsTrue(Invoke<bool>(method, walker, registered, null));
        Assert.IsTrue(Invoke<bool>(method, walker, compilation.GetTypeByMetadataName("Marked")!, null));
        Assert.IsTrue(Invoke<bool>(method, walker, compilation.GetTypeByMetadataName("Derived")!, null));
        Assert.IsTrue(Invoke<bool>(method, walker, compilation.GetSpecialType(SpecialType.System_Int32), null));
    }

    [TestMethod]
    public void PrimaryConstructorInitializerDiscovery_ExcludesStaticAndUninitializedMembers()
    {
        var compilation = CreateCompilation(
            """
            public static class Module
            {
                public sealed class Profile(string value)
                {
                    private readonly string field = value;
                    public string Property { get; } = value;
                    public int Uninitialized;
                    public static int Counter = 0;
                }
            }
            """);
        var syntaxTree = compilation.SyntaxTrees.Single();
        var module = compilation.GetTypeByMetadataName("Module")!;
        var profile = module.GetTypeMembers("Profile").Single();
        var converter = new AstConverter(module, compilation.GetSemanticModel(syntaxTree));
        var method = typeof(AstConverter).GetMethod(
            "GetPrimaryConstructorInitializers",
            BindingFlags.Instance | BindingFlags.NonPublic)!;

        var initializers = method.Invoke(converter, [profile])!;
        var length = (int)initializers.GetType().GetProperty("Length")!.GetValue(initializers)!;

        Assert.AreEqual(2, length);
    }

    [TestMethod]
    public void DefaultValueFallbackContract_RequiresClosedSourceFallbackOverloadAndImportMapping()
    {
        var (compilation, block) = CreateBlock(
            """
            using System.Collections.Generic;

            public static class FallbackContracts
            {
                public static int Nongeneric() => 0;

                public static IEnumerable<T> Missing<T>(IEnumerable<T> source) => source;

                public static IEnumerable<T> Unmapped<T>(IEnumerable<T> source) => source;

                public static IEnumerable<T> Unmapped<T>(IEnumerable<T> source, T defaultValue) => source;
            }

            public sealed class TestClass
            {
                void TestMethod()
                {
                    IEnumerable<int> values = new List<int>();
                    _ = FallbackContracts.Nongeneric();
                    _ = FallbackContracts.Missing(values);
                    _ = FallbackContracts.Unmapped(values);
                }
            }
            """);
        var method = typeof(SemanticWalker).GetMethod(
            "BuildDefaultValueCall",
            BindingFlags.Instance | BindingFlags.NonPublic)!;
        var walker = new SemanticWalker(true);
        var invocations = block.DescendantsAndSelf().OfType<IInvocationOperation>().ToArray();

        AssertFailure("Nongeneric", "requires one bound TSource type argument");
        AssertFailure("Missing", "defaultValue) is required as the runtime fallback contract");
        AssertFailure("Unmapped", "must have an Import mapping");

        void AssertFailure(string methodName, string expectedMessage)
        {
            var invocation = invocations.Single(operation => operation.TargetMethod.Name == methodName);
            var exception = Assert.Throws<TargetInvocationException>(() => method.Invoke(
                walker,
                [invocation.TargetMethod, invocation, new SenseArgument(), Array.Empty<Expression>()]));

            StringAssert.Contains(exception.InnerException!.Message, expectedMessage);
        }
    }

    [TestMethod]
    public void HostSkippedVariableDeclaration_OnlySkipsEveryDeclaratorOwnedByTheHost()
    {
        var (_, block) = CreateBlock(
            """
            public sealed class TestClass
            {
                void TestMethod()
                {
                    int first = 1, second = 2;
                    var scalar = 3;
                }
            }
            """);
        var method = typeof(SemanticWalker).GetMethod(
            "IsHostSkippedVariableDeclaration",
            BindingFlags.Instance | BindingFlags.NonPublic)!;
        var group = block.Operations.OfType<IVariableDeclarationGroupOperation>().First();
        var declaration = group.Declarations.Single();
        var literal = block.DescendantsAndSelf().OfType<ILiteralOperation>()
            .Single(static operation => operation.ConstantValue.Value is 3);
        var skipAll = new SemanticWalker(true) { Host = new SkipAllVariableDeclaratorsHost() };
        var skipNone = new SemanticWalker(true) { Host = new SkipNoVariableDeclaratorsHost() };

        Assert.IsTrue(Invoke<bool>(method, skipAll, group, new SenseArgument()));
        Assert.IsFalse(Invoke<bool>(method, skipNone, group, new SenseArgument()));
        Assert.IsTrue(Invoke<bool>(method, skipAll, declaration, new SenseArgument()));
        Assert.IsFalse(Invoke<bool>(method, skipNone, declaration, new SenseArgument()));
        Assert.IsFalse(Invoke<bool>(method, skipAll, literal, new SenseArgument()));
    }

    [TestMethod]
    public void LoweringNameOwner_RequiresTheVisitScopedNamingSession()
    {
        var (_, block) = CreateBlock(
            """
            public sealed class TestClass
            {
                void TestMethod()
                {
                    var value = 1;
                }
            }
            """);
        var operation = block.DescendantsAndSelf().OfType<ILiteralOperation>()
            .Single(static candidate => candidate.ConstantValue.Value is 1);
        var method = typeof(SemanticWalker).GetMethod(
            "CreateLoweringNameOwner",
            BindingFlags.Instance | BindingFlags.NonPublic)!;

        var exception = Assert.Throws<TargetInvocationException>(() => method.Invoke(
            new SemanticWalker(true),
            [operation, LoweringSite.CreationTemp(), new SenseArgument()]));

        StringAssert.Contains(exception.InnerException!.Message, "会话尚未初始化");
    }

    [TestMethod]
    public void InitializerMemberNameResolvers_PreserveStructuralAndMappedMemberContracts()
    {
        var (_, block) = CreateBlock(
            """
            using System.Collections.Generic;

            public sealed record Structural
            {
                public int Value { get; init; }
            }

            public sealed class TestClass
            {
                void TestMethod()
                {
                    var structural = new Structural { Value = 1 };
                    var selected = structural.Value;
                    var map = new Dictionary<string, int>();
                    var mapped = map["key"];
                    map["key"] = 2;
                }
            }
            """);
        var accessResolver = typeof(SemanticWalker).GetMethod(
            "ResolveInitializerAccessMemberName",
            BindingFlags.Instance | BindingFlags.NonPublic)!;
        var assignmentResolver = typeof(SemanticWalker).GetMethod(
            "ResolveInitializerAssignmentMemberName",
            BindingFlags.Instance | BindingFlags.NonPublic)!;
        var walker = new SemanticWalker(true);
        var references = block.DescendantsAndSelf().OfType<IPropertyReferenceOperation>().ToArray();
        var structuralRead = references.Single(reference =>
            reference.Property.Name == "Value" &&
            reference.Parent is IVariableInitializerOperation);
        var structuralWrite = references.Single(reference =>
            reference.Property.Name == "Value" &&
            reference.Parent is ISimpleAssignmentOperation);
        var mappedRead = references.Single(reference =>
            reference.Property.IsIndexer &&
            reference.Parent is IVariableInitializerOperation);
        var mappedWrite = references.Single(reference =>
            reference.Property.IsIndexer &&
            reference.Parent is ISimpleAssignmentOperation);

        Assert.AreEqual("Value", Resolve(accessResolver, structuralRead, "member initializer access"));
        Assert.AreEqual("this[]", Resolve(accessResolver, mappedRead, "member initializer access"));
        Assert.AreEqual("Value", Resolve(assignmentResolver, structuralWrite, "object initializer property assignment"));
        Assert.AreEqual("this[]", Resolve(assignmentResolver, mappedWrite, "object initializer property assignment"));

        string Resolve(MethodInfo resolver, IPropertyReferenceOperation reference, string usage)
            => Invoke<string>(
                resolver,
                walker,
                reference,
                reference.Property,
                usage,
                reference.Instance!.Type!);
    }

    [TestMethod]
    public void MethodCallLowering_StaticReceiverPathsPreferExtensionHostAndRetainFallbackReceiver()
    {
        var (_, block) = CreateBlock(
            """
            namespace ECMAScript
            {
                [global::System.AttributeUsage(global::System.AttributeTargets.Class, Inherited = false)]
                public sealed class ECMAScriptAttribute : global::System.Attribute
                {
                }
            }

            [ECMAScript.ECMAScript]
            public sealed class RuntimeHost
            {
            }

            [ECMAScript.ECMAScript]
            public static class Extensions
            {
                public static int Read(this RuntimeHost host) => 1;
            }

            public sealed class TestClass
            {
                void TestMethod()
                {
                    RuntimeHost host = new();
                    _ = host.Read();
                    _ = global::System.Math.Abs(-1);
                }
            }
            """);
        var method = typeof(SemanticWalker).GetMethod(
            "BuildMethodCallExpression",
            BindingFlags.Instance | BindingFlags.NonPublic)!;
        var invocations = block.DescendantsAndSelf().OfType<IInvocationOperation>().ToArray();
        var extensionInvocation = invocations.Single(static invocation => invocation.TargetMethod.IsExtensionMethod);
        var staticInvocation = invocations.Single(static invocation =>
            invocation.TargetMethod.ContainingType.ToDisplayString() == "System.Math");
        var walker = new SemanticWalker(true);

        var extensionCall = Lower(
            extensionInvocation,
            new Identifier("host"),
            [new Identifier("host")]);
        var fallbackCall = Lower(
            staticInvocation,
            new Identifier("receiver"),
            [new NumericLiteral(1, "1")]);

        Assert.AreEqual("Extensions.Read(host)", extensionCall.ToKnRECMAScript());
        Assert.AreEqual("receiver.abs(1)", fallbackCall.ToKnRECMAScript());

        Expression Lower(
            IInvocationOperation invocation,
            Expression receiver,
            List<Expression> arguments)
            => Invoke<Expression>(
                method,
                walker,
                invocation,
                invocation.TargetMethod,
                invocation.Syntax,
                invocation.SemanticModel,
                receiver,
                arguments,
                new SenseArgument(),
                invocation.TargetMethod.ContainingType!,
                false,
                invocation);
    }

    [TestMethod]
    public void ObjectLiteralNumericKeyFactory_FormatsDecimalAndRejectsNonNumericValues()
    {
        var method = GetSemanticWalkerStaticMethod("TryCreateNumericLiteral");
        var decimalArguments = new object?[] { 12.5m, null };
        var unsupportedArguments = new object?[] { "12.5", null };

        Assert.IsTrue((bool)method.Invoke(null, decimalArguments)!);
        Assert.AreEqual("12.5", ((Expression)decimalArguments[1]!).ToKnRECMAScript());
        Assert.IsFalse((bool)method.Invoke(null, unsupportedArguments)!);
        Assert.IsNull(unsupportedArguments[1]);
    }

    [TestMethod]
    public void DynamicObjectKeyDiagnostic_IdentifiesKnownAndUnknownHostTypes()
    {
        var (compilation, block) = CreateBlock(
            """
            public sealed class TestClass
            {
                void TestMethod()
                {
                    var value = 42;
                }
            }
            """);
        var literal = block.DescendantsAndSelf().OfType<ILiteralOperation>()
            .Single(static operation => operation.ConstantValue.Value is 42);
        var method = typeof(SemanticWalker).GetMethod(
            "RejectUnsupportedDynamicObjectLiteralKey",
            BindingFlags.Instance | BindingFlags.NonPublic)!;
        var walker = new SemanticWalker(true);

        AssertDiagnostic(compilation.GetSpecialType(SpecialType.System_Int32), "int");
        AssertDiagnostic(null, "<unknown>");

        void AssertDiagnostic(ITypeSymbol? hostType, string expectedHost)
        {
            var exception = Assert.Throws<TargetInvocationException>(() =>
                method.Invoke(walker, [literal, hostType, "object initializer"]));
            var failure = Assert.IsInstanceOfType<OperationTransformationException>(exception.InnerException);
            StringAssert.Contains(failure.Message, expectedHost);
        }
    }

    [TestMethod]
    public void DeconstructionAssignments_PreserveInstanceAndStaticFieldWriteTargets()
    {
        var (_, block) = CreateBlock(
            """
            public sealed class TestClass
            {
                public sealed class Holder
                {
                    public int Value;
                    public static int Shared;
                }

                void TestMethod()
                {
                    var holder = new Holder();
                    (holder.Value, Holder.Shared) = (1, 2);
                }
            }
            """);
        var script = new SemanticWalker(true).Visit(block, new SenseArgument())?.ToKnRECMAScript();

        Assert.IsNotNull(script);
        StringAssert.Contains(script, "holder.Value = 1");
        StringAssert.Contains(script, "Holder.Shared = 2");
        _ = new Parser().ParseScript(script);
    }

    [TestMethod]
    public void DeconstructionAssignments_RejectExtensionAndStructProtocolsWithoutRuntimeCarriers()
    {
        var (_, structBlock) = CreateBlock(
            """
            public sealed class TestClass
            {
                public struct Value
                {
                    public void Deconstruct(out int first, out int second)
                    {
                        first = 1;
                        second = 2;
                    }
                }

                void TestMethod()
                {
                    var value = new Value();
                    var (first, second) = value;
                }
            }
            """);
        var (_, extensionBlock) = CreateBlock(
            """
            public sealed class TestClass
            {
                public sealed class Value
                {
                }

                void TestMethod()
                {
                    var value = new Value();
                    var (first, second) = value;
                }
            }

            public static class Extensions
            {
                public static void Deconstruct(this TestClass.Value value, out int first, out int second)
                {
                    first = 1;
                    second = 2;
                }
            }
            """);

        var structFailure = Assert.Throws<OperationTransformationException>(() =>
            new SemanticWalker(true).Visit(structBlock, new SenseArgument()));
        var extensionFailure = Assert.Throws<OperationTransformationException>(() =>
            new SemanticWalker(true).Visit(extensionBlock, new SenseArgument()));

        StringAssert.Contains(structFailure.Message, "Custom Deconstruct on struct type");
        StringAssert.Contains(extensionFailure.Message, "Extension Deconstruct method");
    }

    [TestMethod]
    public void MultiParameterIndexerAssignment_RejectsRawJavaScriptWriteFallback()
    {
        var (_, block) = CreateBlock(
            """
            public sealed class TestClass
            {
                public sealed class Matrix
                {
                    public int this[int row, int column]
                    {
                        get => 0;
                        set { }
                    }
                }

                void TestMethod()
                {
                    var matrix = new Matrix();
                    matrix[0, 1] = 2;
                }
            }
            """);

        var exception = Assert.Throws<OperationTransformationException>(() =>
            new SemanticWalker(true).Visit(block, new SenseArgument()));

        StringAssert.Contains(exception.Message, "only supports a single translated index argument");
    }

    [TestMethod]
    public void MemberBackingFieldNaming_UsesTheAutoPropertySymbolWhenAvailableAndStableFallbackOtherwise()
    {
        var (compilation, _) = CreateBlock(
            """
            public static class ModuleHost
            {
                public sealed class RuntimeHost
                {
                    public int Auto { get; set; }

                    public int Manual
                    {
                        get => 1;
                        set { }
                    }
                }

                static void TestMethod()
                {
                }
            }
            """);
        var module = compilation.GetTypeByMetadataName("ModuleHost")!;
        var runtimeHost = module.GetTypeMembers("RuntimeHost").Single();
        var auto = runtimeHost.GetMembers("Auto").OfType<IPropertySymbol>().Single();
        var manual = runtimeHost.GetMembers("Manual").OfType<IPropertySymbol>().Single();
        var converter = new AstConverter(module, compilation.GetSemanticModel(compilation.SyntaxTrees.Single()));
        var method = typeof(AstConverter).GetMethod(
            "GetMemberBackingFieldName",
            BindingFlags.Instance | BindingFlags.NonPublic)!;

        Assert.AreEqual(
            Format.HashName(auto.OriginalDefinition.ToDisplayString(Format.NameFormat)),
            Invoke<string>(method, converter, auto));
        Assert.AreEqual(
            Format.HashName(manual.OriginalDefinition.ToDisplayString(Format.NameFormat)),
            Invoke<string>(method, converter, manual));
    }

    [TestMethod]
    public void WhiteListMethodKey_ReducedExternExtensionKeepsCanonicalExternStaticModifierOrder()
    {
        var compilation = CreateCompilation(
            """
            namespace LookupTests;

            public static class TextExtensions
            {
                public static extern int Measure(this string value, in int offset);
            }

            public sealed class Consumer
            {
                public int Read(string value)
                {
                    var offset = 2;
                    return value.Measure(in offset);
                }
            }
            """);
        var syntaxTree = compilation.SyntaxTrees.Single();
        var model = compilation.GetSemanticModel(syntaxTree);
        var invocation = syntaxTree.GetRoot()
            .DescendantNodes()
            .OfType<InvocationExpressionSyntax>()
            .Single();
        var reducedMethod = Assert.IsInstanceOfType<IMethodSymbol>(model.GetSymbolInfo(invocation).Symbol);
        var lookupType = typeof(SemanticWalker).Assembly.GetType("Jazor.Compiler.WhiteListLookup", throwOnError: true)!;
        var method = lookupType.GetMethod(
            "TryBuildMethodWhiteListKey",
            BindingFlags.Static | BindingFlags.NonPublic)!;

        var key = Invoke<string>(method, null, reducedMethod);

        Assert.AreEqual("extern static LookupTests.TextExtensions.Measure(string, in int)", key);
    }

    [TestMethod]
    public void NullableAndBindingNameHelpers_PreserveRuntimeTypeAndGenerateStableFallbackIdentifiers()
    {
        var compilation = CreateCompilation(
            """
            public sealed class Holder
            {
                public int Read()
                {
                    var @class = 1;
                    return @class;
                }
            }
            """);
        var unwrapNullable = GetSemanticWalkerStaticMethod("UnwrapNullableValueType");
        var getBindingName = GetSemanticWalkerStaticMethod("GetJavaScriptBindingName");
        var intType = compilation.GetSpecialType(SpecialType.System_Int32);
        var nullableInt = compilation.GetSpecialType(SpecialType.System_Nullable_T).Construct(intType);
        var intArray = compilation.CreateArrayTypeSymbol(intType);
        var syntaxTree = compilation.SyntaxTrees.Single();
        var model = compilation.GetSemanticModel(syntaxTree);
        var escapedKeyword = model.GetDeclaredSymbol(syntaxTree.GetRoot()
            .DescendantNodes()
            .OfType<VariableDeclaratorSyntax>()
            .Single())!;

        Assert.IsTrue(SymbolEqualityComparer.Default.Equals(
            intType,
            Invoke<ITypeSymbol>(unwrapNullable, null, nullableInt)));
        Assert.IsTrue(SymbolEqualityComparer.Default.Equals(
            intType,
            Invoke<ITypeSymbol>(unwrapNullable, null, intType)));
        Assert.IsTrue(SymbolEqualityComparer.Default.Equals(
            intArray,
            Invoke<ITypeSymbol>(unwrapNullable, null, intArray)));

        var escapedKeywordBinding = Invoke<string>(getBindingName, null, escapedKeyword);
        var globalNamespaceBinding = Invoke<string>(getBindingName, null, compilation.GlobalNamespace);
        Assert.IsTrue(escapedKeywordBinding.StartsWith("__binding$", StringComparison.Ordinal));
        Assert.IsTrue(globalNamespaceBinding.StartsWith("__binding$", StringComparison.Ordinal));
        Assert.AreNotEqual(escapedKeywordBinding, globalNamespaceBinding);
    }

    [TestMethod]
    public void CustomOperatorFallbackPolicy_UsesRuntimeMappingBeforeMarkerFallback()
    {
        var (compilation, block) = CreateBlock(
            """
            using System.Collections.Generic;
            using System.Linq;

            namespace ECMAScript
            {
                [global::System.AttributeUsage(global::System.AttributeTargets.Class, Inherited = false)]
                public sealed class ECMAScriptAttribute : global::System.Attribute
                {
                }
            }

            public sealed record Structural(int Value)
            {
                public static Structural operator +(Structural left, Structural right)
                    => new(left.Value + right.Value);
            }

            [ECMAScript.ECMAScript]
            public sealed class MarkedRuntime
            {
                public static MarkedRuntime operator +(MarkedRuntime left, MarkedRuntime right) => left;
            }

            public sealed class TestClass
            {
                void TestMethod()
                {
                    var values = new[] { 1 };
                    _ = values.ElementAtOrDefault(0);
                    var map = new Dictionary<string, int>();
                    map.Add("key", 1);
                    _ = new Structural(1) + new Structural(2);
                }
            }
            """);
        var method = typeof(SemanticWalker).GetMethod(
            "IsPassThroughCustomOperatorFallbackAllowed",
            BindingFlags.Instance | BindingFlags.NonPublic)!;
        var walker = new SemanticWalker(true);
        var invocations = block.DescendantsAndSelf().OfType<IInvocationOperation>().ToArray();
        var enumerableCompile = invocations.Single(static invocation =>
            invocation.TargetMethod.ContainingType.ToDisplayString() == "System.Linq.Enumerable").TargetMethod;
        var dictionaryImport = invocations.Single(static invocation =>
            invocation.TargetMethod.ContainingType.OriginalDefinition.ToDisplayString() ==
            "System.Collections.Generic.Dictionary<TKey, TValue>").TargetMethod;
        var structuralOperator = block.DescendantsAndSelf().OfType<IBinaryOperation>()
            .Single(static operation => operation.OperatorMethod is not null)
            .OperatorMethod!;
        var markedOperator = compilation.GetTypeByMetadataName("MarkedRuntime")!
            .GetMembers("op_Addition")
            .OfType<IMethodSymbol>()
            .Single();

        Assert.IsTrue(Invoke<bool>(method, walker, enumerableCompile));
        Assert.IsFalse(Invoke<bool>(method, walker, dictionaryImport));
        Assert.IsFalse(Invoke<bool>(method, walker, structuralOperator));
        Assert.IsTrue(Invoke<bool>(method, walker, markedOperator));
    }

    [TestMethod]
    public void ObjectAndMemberInitializers_LowerStructuralPropertyFieldAndCollectionContracts()
    {
        var (_, block) = CreateBlock(
            """
            using System.Collections.Generic;

            public sealed record Child
            {
                public int Value { get; set; }
                public int Field;
            }

            public sealed record Holder
            {
                public int Count { get; set; }
                public int Field;
                public Child Child { get; } = new();
                public Child FieldChild = new();
                public Child Replacement { get; set; } = new();
            }

            public sealed class TestClass
            {
                void TestMethod()
                {
                    var holder = new Holder
                    {
                        Count = 1,
                        Field = 2,
                        Child = { Value = 3 },
                        FieldChild = { Field = 4 },
                        Replacement = new Child { Value = 5 }
                    };
                    var values = new List<int> { 6 };
                }
            }
            """);
        var initializers = block.DescendantsAndSelf()
            .OfType<IObjectCreationOperation>()
            .Where(static operation => operation.Initializer is not null)
            .ToArray();
        var holderCreation = initializers.Single(static operation => operation.Type?.Name == "Holder");
        var listCreation = initializers.Single(static operation => operation.Type?.Name == "List");
        var memberInitializers = holderCreation.Initializer!.Initializers
            .OfType<IMemberInitializerOperation>()
            .ToArray();
        var buildInitializer = typeof(SemanticWalker).GetMethod(
            "BuildObjectCreationInitializer",
            BindingFlags.Instance | BindingFlags.NonPublic)!;
        var buildMemberReceiver = typeof(SemanticWalker).GetMethod(
            "BuildMemberInitializerReceiver",
            BindingFlags.Instance | BindingFlags.NonPublic)!;
        var walker = new SemanticWalker(true);

        var holderExpressions = Invoke<List<Expression>>(
            buildInitializer,
            walker,
            new Identifier("holder"),
            holderCreation.Initializer,
            new SenseArgument());
        var listExpressions = Invoke<List<Expression>>(
            buildInitializer,
            walker,
            new Identifier("values"),
            listCreation.Initializer,
            new SenseArgument());
        var propertyReceiver = Invoke<Expression>(
            buildMemberReceiver,
            walker,
            memberInitializers.Single(static initializer =>
                initializer.InitializedMember is IPropertyReferenceOperation { Property.Name: "Child" }),
            new Identifier("holder"),
            new SenseArgument());
        var fieldReceiver = Invoke<Expression>(
            buildMemberReceiver,
            walker,
            memberInitializers.Single(static initializer =>
                initializer.InitializedMember is IFieldReferenceOperation { Field.Name: "FieldChild" }),
            new Identifier("holder"),
            new SenseArgument());

        var holderScript = string.Join("; ", holderExpressions.Select(static expression => expression.ToKnRECMAScript()));
        StringAssert.Contains(holderScript, "Count = 1");
        StringAssert.Contains(holderScript, "Field = 2");
        StringAssert.Contains(holderScript, "Value = 3");
        StringAssert.Contains(holderScript, "Replacement");
        Assert.HasCount(1, listExpressions);
        StringAssert.Contains(listExpressions[0].ToKnRECMAScript(), "(");
        StringAssert.Contains(propertyReceiver.ToKnRECMAScript(), "Child");
        StringAssert.Contains(fieldReceiver.ToKnRECMAScript(), "FieldChild");
    }

    [TestMethod]
    public void PropertyWriteTarget_PreservesInstanceIndexerStaticAndUnqualifiedForms()
    {
        var (_, block) = CreateBlock(
            """
            public sealed class Holder
            {
                public int Value { get; set; }
                public static int Shared { get; set; }
                public int this[int index]
                {
                    get => 0;
                    set { }
                }
            }

            public sealed class TestClass
            {
                void TestMethod()
                {
                    var holder = new Holder();
                    holder.Value = 1;
                    holder[2] = 3;
                    Holder.Shared = 4;
                }
            }
            """);
        var method = typeof(SemanticWalker).GetMethod(
            "BuildPropertyWriteTarget",
            BindingFlags.Instance | BindingFlags.NonPublic)!;
        var references = block.DescendantsAndSelf().OfType<IPropertyReferenceOperation>().ToArray();
        var walker = new SemanticWalker(true);

        var instance = references.Single(static reference => reference.Property.Name == "Value");
        var indexer = references.Single(static reference => reference.Property.IsIndexer);
        var @static = references.Single(static reference => reference.Property.Name == "Shared");

        Assert.AreEqual("holder.Value", Lower(instance, new Identifier("holder"), []));
        Assert.AreEqual("holder[2]", Lower(indexer, new Identifier("holder"), [new NumericLiteral(2, "2")]));
        Assert.AreEqual("Holder.Shared", Lower(@static, null, []));

        string Lower(IPropertyReferenceOperation reference, Expression? instanceExpression, List<Expression> arguments)
            => Invoke<Expression>(method, walker, reference, new SenseArgument(), instanceExpression, arguments)
                .ToKnRECMAScript();
    }

    [TestMethod]
    public void VariableDeclaratorHostPreorder_ClaimsTheDeclarationBeforeDefaultLowering()
    {
        var (_, block) = CreateBlock(
            """
            public sealed class TestClass
            {
                void TestMethod()
                {
                    var value = 1;
                }
            }
            """);
        var declarator = block.DescendantsAndSelf().OfType<IVariableDeclaratorOperation>().Single();
        var walker = new SemanticWalker(true) { Host = new PreorderVariableDeclaratorHost() };

        var lowered = Assert.IsInstanceOfType<VariableDeclarator>(
            walker.Visit(declarator, new SenseArgument()));

        Assert.AreEqual("hostOwned", Assert.IsInstanceOfType<Identifier>(lowered.Id).Name);
        Assert.AreEqual("7", lowered.Init?.ToKnRECMAScript());
    }

    [TestMethod]
    public void ImportedSpecifierNameResolver_RecognizesDefaultNamespaceAndNamedImportShapes()
    {
        var method = typeof(AstConverter).GetMethod(
            "GetImportedSpecifierName",
            BindingFlags.Static | BindingFlags.NonPublic)!;

        Assert.AreEqual("default", Invoke<string>(
            method,
            null,
            new ImportDefaultSpecifier(new Identifier("component"))));
        Assert.AreEqual("*", Invoke<string>(
            method,
            null,
            new ImportNamespaceSpecifier(new Identifier("runtime"))));
        Assert.AreEqual("feature", Invoke<string>(
            method,
            null,
            new ImportSpecifier(
                new Identifier("feature"),
                new Identifier("featureLocal"))));
        Assert.AreEqual("feature-name", Invoke<string>(
            method,
            null,
            new ImportSpecifier(
                new StringLiteral("feature-name", "\"feature-name\""),
                new Identifier("featureName"))));
    }

    [TestMethod]
    public void PrimaryConstructorBaseInitializer_RequiresABoundSupportedBaseType()
    {
        var compilation = CreateCompilation(
            """
            public class Base(int value)
            {
            }

            public class Derived(int value) : Base(value)
            {
            }
            """);
        var syntaxTree = compilation.SyntaxTrees.Single();
        var derivedDeclaration = syntaxTree.GetRoot()
            .DescendantNodes()
            .OfType<ClassDeclarationSyntax>()
            .Single(static declaration => declaration.Identifier.ValueText == "Derived");
        var baseType = compilation.GetTypeByMetadataName("Base")!;
        var derivedType = compilation.GetTypeByMetadataName("Derived")!;
        var converter = new AstConverter(derivedType, compilation.GetSemanticModel(syntaxTree));
        var method = typeof(AstConverter).GetMethod(
            "GetPrimaryConstructorBaseInvocation",
            BindingFlags.Instance | BindingFlags.NonPublic)!;

        var invocation = ((System.Collections.Immutable.ImmutableArray<ArgumentSyntax> Arguments, IMethodSymbol? Constructor))
            method.Invoke(converter, [derivedDeclaration, baseType])!;
        Assert.HasCount(1, invocation.Arguments);
        Assert.IsNotNull(invocation.Constructor);
        Assert.Throws<TargetInvocationException>(() => method.Invoke(converter, [derivedDeclaration, null]));
    }

    [TestMethod]
    public void LiteralFactory_RejectsUnsupportedRuntimeValuesInsteadOfStringifyingThem()
    {
        var method = typeof(AstConverter).GetMethod(
            "CreateLiteralExpression",
            BindingFlags.Static | BindingFlags.NonPublic)!;

        var exception = Assert.Throws<TargetInvocationException>(() =>
            method.Invoke(null, [Guid.Empty]));

        StringAssert.Contains(exception.InnerException!.Message, "Unsupported literal type");
    }

    [TestMethod]
    public void AwaitAndIncrementClassifiers_UseBoundOperationKindsAndMappedCarriers()
    {
        var (_, awaitBlock) = CreateBlock(
            """
            using System.Threading.Tasks;

            public sealed class TestClass
            {
                async Task TestMethod()
                {
                    await Task.CompletedTask;
                    var value = 1;
                }
            }
            """);
        var (_, incrementBlock) = CreateBlock(
            """
            public struct CustomCounter
            {
                public static CustomCounter operator ++(CustomCounter value) => value;
            }

            public sealed class TestClass
            {
                void TestMethod()
                {
                    var number = 0;
                    number++;
                    var custom = default(CustomCounter);
                    custom++;
                }
            }
            """);
        var containsAwait = GetSemanticWalkerStaticMethod("ContainsAwaitOperation");
        var canPassIncrement = GetSemanticWalkerStaticMethod("CanPassThroughIntrinsicIncrementOrDecrement");
        var literal = awaitBlock.DescendantsAndSelf().OfType<ILiteralOperation>()
            .Single(static operation => operation.ConstantValue.Value is 1);
        var increments = incrementBlock.DescendantsAndSelf()
            .OfType<IIncrementOrDecrementOperation>()
            .ToArray();

        Assert.IsTrue(Invoke<bool>(containsAwait, null, awaitBlock));
        Assert.IsFalse(Invoke<bool>(containsAwait, null, literal));
        Assert.IsTrue(Invoke<bool>(canPassIncrement, null, increments.Single(static operation =>
            operation.Target.Type?.SpecialType == SpecialType.System_Int32)));
        Assert.IsFalse(Invoke<bool>(canPassIncrement, null, increments.Single(static operation =>
            operation.Target.Type?.Name == "CustomCounter")));
    }

    [TestMethod]
    public void ListPatternLengthPurity_RecognizesArrayLengthMappingWithAndWithoutIntrinsicFastPath()
    {
        var (_, block) = CreateBlock(
            """
            public sealed class TestClass
            {
                bool TestMethod(int[] values)
                {
                    return values is [1, ..];
                }
            }
            """);
        var operation = block.DescendantsAndSelf().OfType<IListPatternOperation>().Single();
        var method = typeof(SemanticWalker).GetMethod(
            "IsPureListPatternLengthAccess",
            BindingFlags.Instance | BindingFlags.NonPublic)!;
        var walker = new SemanticWalker(true);

        Assert.IsTrue(Invoke<bool>(method, walker, operation, true));
        Assert.IsTrue(Invoke<bool>(method, walker, operation, false));
    }

    [TestMethod]
    public void ObjectAndMemberInitializers_RejectUnmappedNominalMemberFallbacks()
    {
        var (_, block) = CreateBlock(
            """
            public sealed class Child
            {
                public int Value { get; set; }
                public int Field;
            }

            public sealed class Holder
            {
                public int Value { get; set; }
                public Child Child { get; } = new();
                public Child FieldChild = new();
            }

            public sealed class Collector : System.Collections.IEnumerable
            {
                public void Add(int value)
                {
                }

                public System.Collections.IEnumerator GetEnumerator()
                    => System.Array.Empty<int>().GetEnumerator();
            }

            public sealed class TestClass
            {
                void TestMethod()
                {
                    var holder = new Holder
                    {
                        Value = 1,
                        Child = { Value = 2 },
                        FieldChild = { Field = 3 }
                    };
                    var collector = new Collector { 4 };
                }
            }
            """);
        var creations = block.DescendantsAndSelf()
            .OfType<IObjectCreationOperation>()
            .Where(static operation => operation.Initializer is not null)
            .ToArray();
        var holder = creations.Single(static operation => operation.Type?.Name == "Holder");
        var collector = creations.Single(static operation => operation.Type?.Name == "Collector");
        var memberInitializers = holder.Initializer!.Initializers
            .OfType<IMemberInitializerOperation>()
            .ToArray();
        var buildInitializer = typeof(SemanticWalker).GetMethod(
            "BuildObjectCreationInitializer",
            BindingFlags.Instance | BindingFlags.NonPublic)!;
        var buildMemberReceiver = typeof(SemanticWalker).GetMethod(
            "BuildMemberInitializerReceiver",
            BindingFlags.Instance | BindingFlags.NonPublic)!;
        var walker = new SemanticWalker(true) { Host = new InstanceReferenceProjectionHost() };

        AssertUnsupported(() => buildInitializer.Invoke(
            walker,
            [new Identifier("holder"), holder.Initializer, new SenseArgument()]));
        AssertUnsupported(() => buildInitializer.Invoke(
            walker,
            [new Identifier("collector"), collector.Initializer, new SenseArgument()]));
        AssertUnsupported(() => buildMemberReceiver.Invoke(
            walker,
            [
                memberInitializers.Single(static initializer =>
                    initializer.InitializedMember is IPropertyReferenceOperation),
                new Identifier("holder"),
                new SenseArgument()
            ]));
        AssertUnsupported(() => buildMemberReceiver.Invoke(
            walker,
            [
                memberInitializers.Single(static initializer =>
                    initializer.InitializedMember is IFieldReferenceOperation),
                new Identifier("holder"),
                new SenseArgument()
            ]));

        static void AssertUnsupported(Action action)
        {
            var exception = Assert.Throws<TargetInvocationException>(action);
            Assert.IsInstanceOfType<OperationTransformationException>(exception.InnerException);
        }
    }

    [TestMethod]
    public void ImportBindingContext_HandlesAbsentOptionalBindingCollectionsDeterministically()
    {
        var currentModuleWithoutBindings = new SenseArgument(UseImportAliases: true)
            .WithImportContext(
                [],
                [],
                [],
                "./current.mjs",
                null!);
        var importBindings = new Dictionary<string, string>(StringComparer.Ordinal);
        var importWithoutOptionalCollections = new SenseArgument(UseImportAliases: true)
            .WithImportContext(
                importBindings,
                null!,
                null!,
                currentModuleImportPath: null,
                currentModuleBindings: []);

        Assert.AreEqual(
            "externalHelper",
            currentModuleWithoutBindings.BindImportSpecifier("./current", "externalHelper").Name);
        Assert.AreEqual(
            "helper",
            importWithoutOptionalCollections.BindImportSpecifier("runtime", "helper").Name);
        Assert.AreEqual("helper", importBindings["runtime\0helper"]);
    }

    [TestMethod]
    public void PatternAndListBoundAccess_RejectUnmappedNominalMemberFallbacks()
    {
        var (_, block) = CreateBlock(
            """
            public sealed class Container
            {
                public int Length => 1;
                public int this[int index] => index;
            }

            public sealed class Plain
            {
                public int Value { get; set; }
            }

            public sealed class TestClass
            {
                bool TestMethod(Container input, Plain plain)
                {
                    return input is [1] && plain is { Value: 1 };
                }
            }
            """);
        var listPattern = block.DescendantsAndSelf().OfType<IListPatternOperation>().Single();
        var patternMember = Assert.IsInstanceOfType<IMemberReferenceOperation>(block.DescendantsAndSelf()
            .OfType<IPropertySubpatternOperation>()
            .Single()
            .Member);
        var buildListAccess = typeof(SemanticWalker).GetMethod(
            "BuildListPatternBoundAccess",
            BindingFlags.Instance | BindingFlags.NonPublic)!;
        var buildPatternAccess = typeof(SemanticWalker).GetMethod(
            "BuildPatternMemberAccess",
            BindingFlags.Instance | BindingFlags.NonPublic)!;
        var walker = new SemanticWalker(true);

        AssertUnsupported(() => buildListAccess.Invoke(
            walker,
            [
                listPattern,
                listPattern.LengthSymbol!,
                new Identifier("input"),
                new List<Expression>(),
                new SenseArgument(),
                "list pattern length",
                null
            ]));
        AssertUnsupported(() => buildListAccess.Invoke(
            walker,
            [
                listPattern,
                listPattern.IndexerSymbol!,
                new Identifier("input"),
                new List<Expression> { new NumericLiteral(0, "0") },
                new SenseArgument(),
                "list pattern indexer",
                null
            ]));
        AssertUnsupported(() => buildPatternAccess.Invoke(
            walker,
            [patternMember, new Identifier("plain"), new SenseArgument(), null]));

        static void AssertUnsupported(Action action)
        {
            var exception = Assert.Throws<TargetInvocationException>(action);
            Assert.IsInstanceOfType<OperationTransformationException>(exception.InnerException);
        }
    }

    [TestMethod]
    public void AstConverter_ModuleDeclaredNameFallbacks_RemainDeterministicAcrossPolicyAndProfileBoundaries()
    {
        var compilation = CreateCompilation(
            """
            public static class ModuleHost
            {
                public static int Field;

                public static void Work()
                {
                    var local = 1;
                }

                public static int Value { get; set; }
            }

            public record RuntimeRecord(int Value);
            """);
        var tree = compilation.SyntaxTrees.Single();
        var model = compilation.GetSemanticModel(tree);
        var module = compilation.GetTypeByMetadataName("ModuleHost")!;
        var runtimeRecord = compilation.GetTypeByMetadataName("RuntimeRecord")!;
        var work = module.GetMembers("Work").OfType<IMethodSymbol>().Single();
        var value = module.GetMembers("Value").OfType<IPropertySymbol>().Single();
        var backingField = module.GetMembers().OfType<IFieldSymbol>()
            .Single(static field => field.AssociatedSymbol is IPropertySymbol);
        var policy = new InvalidPreferredModuleNamePolicy();
        var chooseName = typeof(AstConverter).GetMethod(
            "ChooseModuleDeclaredName",
            BindingFlags.Static | BindingFlags.NonPublic)!;
        var preferredName = typeof(AstConverter).GetMethod(
            "GetPreferredModuleDeclaredName",
            BindingFlags.Static | BindingFlags.NonPublic)!;
        var sourceName = typeof(AstConverter).GetMethod(
            "GetSourceDeclaredNameCandidate",
            BindingFlags.Static | BindingFlags.NonPublic)!;
        var buildLocalNames = typeof(AstConverter).GetMethod(
            "BuildModuleLocalNames",
            BindingFlags.Static | BindingFlags.NonPublic)!;
        var isAllowedAccessibility = typeof(AstConverter).GetMethod(
            "IsAllowedTopLevelAccessibility",
            BindingFlags.Instance | BindingFlags.NonPublic)!;
        var getPrimaryConstructorStorage = typeof(AstConverter).GetMethod(
            "GetPrimaryConstructorParameterStorage",
            BindingFlags.Instance | BindingFlags.NonPublic)!;

        Assert.AreEqual(
            "invalid-module-name",
            Invoke<string>(preferredName, null, work, policy, AstConverterProfile.ClrRuntime));
        Assert.AreEqual("Work", Invoke<string>(sourceName, null, work));
        Assert.AreEqual("Value", Invoke<string>(sourceName, null, value.GetMethod!));
        Assert.IsNull(Invoke<string?>(sourceName, null, backingField));
        Assert.AreEqual("ModuleHost", Invoke<string>(sourceName, null, module));

        var localNames = Invoke<HashSet<string>>(
            buildLocalNames,
            null,
            module,
            AstConverterModulePolicy.Default);
        Assert.IsTrue(localNames.Contains("local"));
        Assert.IsFalse(localNames.Contains("Field"));

        Assert.AreEqual(
            "Work",
            Invoke<string>(
                chooseName,
                null,
                work,
                new HashSet<string>(StringComparer.Ordinal),
                new HashSet<string>(StringComparer.Ordinal),
                policy,
                AstConverterProfile.Standard));

        var hashAlias = "m$" + Format.HashName(
            work.OriginalDefinition.ToDisplayString(Format.NameFormat)).TrimStart('_');
        Assert.AreEqual(
            hashAlias + "$1",
            Invoke<string>(
                chooseName,
                null,
                work,
                new HashSet<string>(StringComparer.Ordinal) { hashAlias },
                new HashSet<string>(StringComparer.Ordinal) { "Work" },
                policy,
                AstConverterProfile.Standard));

        var standardConverter = new AstConverter(module, model);
        var clrConverter = new AstConverter(
            module,
            model,
            new AstConverterOptions(AstConverterProfile.ClrRuntime));
        Assert.IsTrue(Invoke<bool>(isAllowedAccessibility, standardConverter, Accessibility.Public));
        Assert.IsFalse(Invoke<bool>(isAllowedAccessibility, standardConverter, Accessibility.Internal));
        Assert.IsTrue(Invoke<bool>(isAllowedAccessibility, clrConverter, Accessibility.Internal));
        Assert.IsFalse(Invoke<bool>(isAllowedAccessibility, clrConverter, Accessibility.Private));

        // Runtime-class primary-constructor capture is intentionally limited to ordinary class
        // declarations; a record's positional declaration must not allocate private class slots.
        Assert.IsEmpty((System.Collections.IEnumerable)getPrimaryConstructorStorage.Invoke(standardConverter, [runtimeRecord])!);
    }

    private static CSharpCompilation CreateCompilation(string source)
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(source, TestMetadataReferences.PreviewParseOptions);
        var compilation = CSharpCompilation.Create(
            "CompilerInternalBoundaryContracts_" + Guid.NewGuid().ToString("N"),
            [syntaxTree],
            TestMetadataReferences.Net11,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        var errors = compilation.GetDiagnostics()
            .Where(static diagnostic => diagnostic.Severity == DiagnosticSeverity.Error)
            .ToArray();
        Assert.IsEmpty(errors, string.Join(Environment.NewLine, errors.Select(static error => error.ToString())));
        return compilation;
    }

    private static (CSharpCompilation Compilation, IBlockOperation Block) CreateBlock(string source)
    {
        var compilation = CreateCompilation(source);
        var syntaxTree = compilation.SyntaxTrees.Single();
        var model = compilation.GetSemanticModel(syntaxTree);
        var method = syntaxTree.GetRoot()
            .DescendantNodes()
            .OfType<MethodDeclarationSyntax>()
            .Single(static declaration => declaration.Identifier.ValueText == "TestMethod");
        return (compilation, Assert.IsInstanceOfType<IBlockOperation>(model.GetOperation(method.Body!)));
    }

    private static MethodInfo GetSemanticWalkerStaticMethod(string name)
        => typeof(SemanticWalker).GetMethod(
            name,
            BindingFlags.Static | BindingFlags.NonPublic)!;

    private static T Invoke<T>(MethodInfo method, object? target, params object?[] arguments)
        => (T)method.Invoke(target, arguments)!;

    private static (bool Stable, string Reason) InvokeNativeMapSetEquality(
        MethodInfo method,
        SemanticWalker walker,
        ITypeSymbol type)
    {
        var arguments = new object?[] { type, null };
        var stable = (bool)method.Invoke(walker, arguments)!;
        return (stable, (string)arguments[1]!);
    }

    private sealed class SkipAllVariableDeclaratorsHost : SemanticWalkerHost
    {
        public override bool ShouldSkipVariableDeclarator(IVariableDeclaratorOperation operation, SenseArgument argument)
            => true;
    }

    private sealed class SkipNoVariableDeclaratorsHost : SemanticWalkerHost
    {
        public override bool ShouldSkipVariableDeclarator(IVariableDeclaratorOperation operation, SenseArgument argument)
            => false;
    }

    private sealed class PreorderVariableDeclaratorHost : SemanticWalkerHost
    {
        public override VariableDeclarator? RewriteVariableDeclaratorPreorder(
            IVariableDeclaratorOperation operation,
            SenseArgument argument)
            => new(new Identifier("hostOwned"), new NumericLiteral(7, "7"));
    }

    private sealed class InstanceReferenceProjectionHost : SemanticWalkerHost
    {
        public override Expression? RewriteInstanceReference(IInstanceReferenceOperation operation, SenseArgument argument)
            => new Identifier("receiver");
    }

    private sealed class InvalidPreferredModuleNamePolicy : AstConverterModulePolicy
    {
        public override string? GetPreferredModuleDeclaredName(ISymbol symbol)
            => "invalid-module-name";
    }
}
