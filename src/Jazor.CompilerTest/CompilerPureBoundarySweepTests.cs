using Acornima;
using Acornima.Ast;
using Jazor.Compiler;
using Jazor.ComplierTest;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;
using System.Reflection;
using System.Text;

namespace Jazor.CompilerTest;

[TestClass]
public sealed class CompilerPureBoundarySweepTests
{
    [TestMethod]
    public void PureTypeAndNameHelpers_CoverScalarTupleAndGenericBoundaries()
    {
        var compilation = CreateCompilation(
            """
            public sealed class TestClass
            {
                void TestMethod()
                {
                    var tuple = (1, 2);
                }
            }
            """);
        var intType = compilation.GetSpecialType(SpecialType.System_Int32);
        var halfType = compilation.GetTypeByMetadataName("System.Half");
        var indexType = compilation.GetTypeByMetadataName("System.Index");
        var rangeType = compilation.GetTypeByMetadataName("System.Range");
        var tupleInitializer = compilation.SyntaxTrees.Single().GetRoot()
            .DescendantNodes()
            .OfType<VariableDeclaratorSyntax>()
            .Single()
            .Initializer!;
        var tupleType = compilation.GetSemanticModel(compilation.SyntaxTrees.Single())
            .GetOperation(tupleInitializer.Value)!.Type!;

        var half = GetPrivateStatic("IsSystemHalfType", typeof(ITypeSymbol));
        Assert.IsFalse((bool)half.Invoke(null, [intType])!);
        Assert.IsTrue((bool)half.Invoke(null, [halfType])!);

        var tupleLike = GetPrivateStatic("IsTupleLikeHost", typeof(ITypeSymbol));
        Assert.IsFalse((bool)tupleLike.Invoke(null, [intType])!);
        Assert.IsTrue((bool)tupleLike.Invoke(null, [tupleType])!);

        var index = GetPrivateStatic("IsSystemIndexType", typeof(ITypeSymbol));
        var range = GetPrivateStatic("IsSystemRangeType", typeof(ITypeSymbol));
        Assert.IsFalse((bool)index.Invoke(null, [intType])!);
        Assert.IsTrue((bool)index.Invoke(null, [indexType])!);
        Assert.IsFalse((bool)range.Invoke(null, [intType])!);
        Assert.IsTrue((bool)range.Invoke(null, [rangeType])!);

        var erase = GetPrivateStatic("EraseGenericDisplayArguments", typeof(string));
        Assert.AreEqual("System.Collections.Generic.List", erase.Invoke(null, ["System.Collections.Generic.List<System.Int32>"]));
        Assert.AreEqual("PlainName", erase.Invoke(null, ["PlainName"]));

        var rewrite = GetPrivateStatic(
            typeof(WhiteListLookup),
            "RewriteDeclaredGenericParameters",
            typeof(string),
            typeof(IReadOnlyDictionary<string, int>));
        var rewritten = rewrite.Invoke(
            null,
            [
                "T + Namespace.T + T2",
                new Dictionary<string, int>(StringComparer.Ordinal) { ["T"] = 0, ["T2"] = 1 }
            ]);
        Assert.AreEqual("{generic_parameter_0} + Namespace.T + {generic_parameter_1}", rewritten);
    }

    [TestMethod]
    public void OptimizerAndSourceMapHelpers_PreserveNullAndPathContracts()
    {
        var guard = GetPrivateStatic(
            typeof(Optimizer),
            "IsNonNullGuardFor",
            typeof(Expression),
            typeof(Expression));
        var value = new Identifier("value");
        var nullLiteral = new NullLiteral("null");
        var leftNull = new Acornima.Ast.NonLogicalBinaryExpression(Operator.Inequality, nullLiteral, value);
        var rightNull = new Acornima.Ast.NonLogicalBinaryExpression(Operator.StrictInequality, value, new NullLiteral("null"));
        var unrelated = new Acornima.Ast.NonLogicalBinaryExpression(Operator.Equality, value, new NullLiteral("null"));
        Assert.IsTrue((bool)guard.Invoke(null, [leftNull, value])!);
        Assert.IsTrue((bool)guard.Invoke(null, [rightNull, value])!);
        Assert.IsFalse((bool)guard.Invoke(null, [unrelated, value])!);

        var append = typeof(GeneratedSourceMapWriter).GetMethod(
            "AppendString",
            BindingFlags.Static | BindingFlags.NonPublic)!;
        var builder = new StringBuilder();
        append.Invoke(null, [builder, null]);
        Assert.AreEqual("null", builder.ToString());

        var normalize = typeof(SourceMapEmitter).GetMethod(
            "NormalizeSourcePath",
            BindingFlags.Static | BindingFlags.NonPublic)!;
        Assert.AreEqual("components/Button.cs", normalize.Invoke(null, ["./components\\Button.cs", null]));
        var root = Path.Combine(Path.GetTempPath(), "Jazor.CompilerPureBoundary", "root");
        var nested = Path.Combine(root, "nested", "Button.cs");
        Assert.AreEqual("nested/Button.cs", normalize.Invoke(null, [nested, root]));

        var sourceRoot = typeof(ESGenerator).GetMethod(
            "TryGetCompilationSourceRoot",
            BindingFlags.Static | BindingFlags.NonPublic)!;
        var compilation = CreateCompilation("public sealed class Empty { }");
        Assert.IsNull(sourceRoot.Invoke(null, [compilation]));
    }

    [TestMethod]
    public void ImportAndOriginHelpers_RejectUnknownShapesAndPreserveLocationFlags()
    {
        var createKey = typeof(ImportDeclarationFactory).GetMethod(
            "CreateSpecifierKey",
            BindingFlags.Static | BindingFlags.NonPublic)!;
        var invalidSpecifier = new ImportSpecifier(new NumericLiteral(1, "1"), new Identifier("local"));
        var exception = Assert.Throws<TargetInvocationException>(() => createKey.Invoke(null, [invalidSpecifier]));
        Assert.IsInstanceOfType<NotSupportedException>(exception.InnerException);

        var createOrigin = typeof(SemanticWalker)
            .GetMethods(BindingFlags.Static | BindingFlags.NonPublic)
            .Single(method => method.Name == "CreateOrigin" &&
                method.GetParameters().Length == 3 &&
                method.GetParameters()[0].ParameterType == typeof(Location));
        var tree = CSharpSyntaxTree.ParseText("public sealed class Origin { }", path: "contracts/Origin.cs");
        var origin = (SourceOrigin)createOrigin.Invoke(
            null,
            [tree.GetRoot().GetLocation(), true, "synthetic"]
        )!;
        Assert.AreEqual("contracts/Origin.cs", origin.SourcePath);
        Assert.IsTrue(origin.IsSynthetic);
        Assert.AreEqual("synthetic", origin.Name);

        var noPath = (SourceOrigin)createOrigin.Invoke(null, [Location.None, false, null])!;
        Assert.IsNull(noPath.SourcePath);
        Assert.IsFalse(noPath.IsSynthetic);
    }

    [TestMethod]
    public void VueContractMetadataAndInitializerSymbols_PreserveDefaultsAndAuthoredOverrides()
    {
        var compilation = CreateCompilation(
            """
            namespace ECMAScript.Contract
            {
                [global::System.AttributeUsage(global::System.AttributeTargets.Property)]
                public sealed class PropsAttribute : global::System.Attribute
                {
                    public int TypeArgumentIndex { get; set; }
                    public string Extra { get; set; } = string.Empty;
                }

                [global::System.AttributeUsage(global::System.AttributeTargets.Property)]
                public sealed class EmitsAttribute : global::System.Attribute
                {
                    public string SourceMemberName { get; set; } = "Setup";
                    public string Extra { get; set; } = string.Empty;
                }
            }

            [global::System.AttributeUsage(global::System.AttributeTargets.Property)]
            public sealed class UnrelatedAttribute : global::System.Attribute
            {
            }

            public sealed class Options
            {
                [ECMAScript.Contract.Props]
                public string[] DefaultProps { get; set; } = [];

                [ECMAScript.Contract.Props(TypeArgumentIndex = 2, Extra = "ignored")]
                public string[] ShiftedProps { get; set; } = [];

                [ECMAScript.Contract.Emits]
                public string[] DefaultEmits { get; set; } = [];

                [ECMAScript.Contract.Emits(SourceMemberName = "Bootstrap", Extra = "ignored")]
                public string[] NamedEmits { get; set; } = [];

                [Unrelated]
                public int Plain { get; set; }
            }

            public sealed class TestClass
            {
                void TestMethod()
                {
                    var options = new Options
                    {
                        Plain = 1,
                        DefaultProps = ["name"]
                    };
                }
            }
            """);
        var options = compilation.GetTypeByMetadataName("Options")!;
        var defaultProps = options.GetMembers("DefaultProps").OfType<IPropertySymbol>().Single();
        var shiftedProps = options.GetMembers("ShiftedProps").OfType<IPropertySymbol>().Single();
        var defaultEmits = options.GetMembers("DefaultEmits").OfType<IPropertySymbol>().Single();
        var namedEmits = options.GetMembers("NamedEmits").OfType<IPropertySymbol>().Single();
        var plain = options.GetMembers("Plain").OfType<IPropertySymbol>().Single();

        var readProps = GetPrivateStatic(
            "TryReadPropsAttribute",
            typeof(AttributeData),
            typeof(int).MakeByRefType());
        var readEmits = GetPrivateStatic(
            "TryReadEmitsAttribute",
            typeof(AttributeData),
            typeof(string).MakeByRefType());
        var getProps = GetPrivateStatic(
            "TryGetPropsAttribute",
            typeof(IPropertySymbol),
            typeof(int).MakeByRefType());
        var getEmits = GetPrivateStatic(
            "TryGetEmitsAttribute",
            typeof(IPropertySymbol),
            typeof(string).MakeByRefType());

        var defaultPropsAttribute = defaultProps.GetAttributes().Single();
        var shiftedPropsAttribute = shiftedProps.GetAttributes().Single();
        var defaultEmitsAttribute = defaultEmits.GetAttributes().Single();
        var namedEmitsAttribute = namedEmits.GetAttributes().Single();
        var propsArguments = new object?[] { defaultPropsAttribute, null };
        Assert.IsTrue((bool)readProps.Invoke(null, propsArguments)!);
        Assert.AreEqual(0, propsArguments[1]);
        propsArguments = [shiftedPropsAttribute, null];
        Assert.IsTrue((bool)readProps.Invoke(null, propsArguments)!);
        Assert.AreEqual(2, propsArguments[1]);
        Assert.IsFalse((bool)readProps.Invoke(null, [plain.GetAttributes().Single(), null])!);

        var emitsArguments = new object?[] { defaultEmitsAttribute, null };
        Assert.IsTrue((bool)readEmits.Invoke(null, emitsArguments)!);
        Assert.AreEqual("Setup", emitsArguments[1]);
        emitsArguments = [namedEmitsAttribute, null];
        Assert.IsTrue((bool)readEmits.Invoke(null, emitsArguments)!);
        Assert.AreEqual("Bootstrap", emitsArguments[1]);
        Assert.IsFalse((bool)readEmits.Invoke(null, [plain.GetAttributes().Single(), null])!);

        var propsResult = new object?[] { defaultProps, null };
        Assert.IsTrue((bool)getProps.Invoke(null, propsResult)!);
        Assert.AreEqual(0, propsResult[1]);
        var emitsResult = new object?[] { namedEmits, null };
        Assert.IsTrue((bool)getEmits.Invoke(null, emitsResult)!);
        Assert.AreEqual("Bootstrap", emitsResult[1]);

        var syntaxTree = compilation.SyntaxTrees.Single();
        var model = compilation.GetSemanticModel(syntaxTree);
        var body = syntaxTree.GetRoot().DescendantNodes()
            .OfType<MethodDeclarationSyntax>()
            .Single(static method => method.Identifier.ValueText == "TestMethod")
            .Body!;
        var block = Assert.IsInstanceOfType<IBlockOperation>(model.GetOperation(body));
        var assignment = block.DescendantsAndSelf()
            .OfType<ISimpleAssignmentOperation>()
            .Single(static operation =>
                operation.Target is IMemberReferenceOperation { Member.Name: "Plain" });
        var literal = block.DescendantsAndSelf()
            .OfType<ILiteralOperation>()
            .Single(static operation => operation.ConstantValue.Value is 1);
        var memberSymbol = GetPrivateStatic("GetObjectInitializerMemberSymbol", typeof(IOperation));
        var memberName = GetPrivateStatic("GetObjectInitializerMemberName", typeof(IOperation));
        Assert.AreSame(assignment.Target switch
        {
            IMemberReferenceOperation memberReference => memberReference.Member,
            _ => null
        }, memberSymbol.Invoke(null, [assignment]));
        Assert.AreEqual("Plain", memberName.Invoke(null, [assignment]));
        Assert.IsNull(memberSymbol.Invoke(null, [literal]));
        Assert.AreEqual(string.Empty, memberName.Invoke(null, [literal]));
    }

    [TestMethod]
    public void StableNamesAndStorageHelpers_PreserveFallbackAndReuseContracts()
    {
        var compilation = CreateCompilation(
            """
            public sealed class Holder
            {
                private int Field;
                public int Property { get; set; }

                void TestMethod()
                {
                    var value = 1;
                }
            }
            """);
        var holder = compilation.GetTypeByMetadataName("Holder")!;
        var field = holder.GetMembers("Field").OfType<IFieldSymbol>().Single();
        var backingField = holder.GetMembers("<Property>k__BackingField").OfType<IFieldSymbol>().Single();
        Assert.AreEqual("$jazor$private$Field", RuntimeClassPrivateStorageNames.GetFieldStorageName(
            RuntimeClassPrivateStorage.ProxySafeMangledProperties,
            field,
            "Field"));
        Assert.AreEqual("Field", RuntimeClassPrivateStorageNames.GetFieldStorageName(
            RuntimeClassPrivateStorage.JavaScriptPrivateFields,
            field,
            "Field"));
        Assert.AreEqual(
            "$jazor$private$" + Jazor.Common.Format.HashName(
                backingField.AssociatedSymbol!.OriginalDefinition.ToDisplayString(Jazor.Common.Format.NameFormat)),
            RuntimeClassPrivateStorageNames.GetFieldStorageName(
                RuntimeClassPrivateStorage.ProxySafeMangledProperties,
                backingField,
                "Property"));

        var tree = compilation.SyntaxTrees.Single();
        var model = compilation.GetSemanticModel(tree);
        var methodBody = tree.GetRoot().DescendantNodes()
            .OfType<MethodDeclarationSyntax>()
            .Single(static method => method.Identifier.ValueText == "TestMethod")
            .Body!;
        var operation = model.GetOperation(methodBody)!;
        var session = new UniqueNameSession(operation, ScopeSite.RootFragment());
        var owner = new LoweringNameOwner("stable", "identity");
        var site = LoweringSite.CreationTemp();
        var first = session.RootScope.Allocate(owner, site);
        var second = session.RootScope.Allocate(owner, site);
        Assert.AreEqual(first, second);
    }

    [TestMethod]
    public void PureBoundaryHelpers_CoverNullishAndEscapingBranches()
    {
        var compilation = CreateCompilation(
            """
            public sealed class Custom
            {
            }

            public sealed class Holder
            {
                public int Value;
            }
            """);
        var intType = compilation.GetSpecialType(SpecialType.System_Int32);
        var customType = compilation.GetTypeByMetadataName("Custom")!;

        var half = GetPrivateStatic("IsSystemHalfType", typeof(ITypeSymbol));
        Assert.IsFalse((bool)half.Invoke(null, [null])!);
        var unwrapNullable = GetPrivateStatic("UnwrapNullableValueType", typeof(ITypeSymbol));
        Assert.AreSame(intType, unwrapNullable.Invoke(null, [intType]));
        var unwrapInterpolation = GetPrivateStatic("UnwrapNullableInterpolationType", typeof(ITypeSymbol));
        Assert.AreSame(customType, unwrapInterpolation.Invoke(null, [customType]));
        var tupleLike = GetPrivateStatic("IsTupleLikeHost", typeof(ITypeSymbol));
        var genericType = CSharpCompilation.Create(
            "GenericBoundary",
            [CSharpSyntaxTree.ParseText("public sealed class Generic<T> { }")],
            TestMetadataReferences.Net11,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary))
            .GetTypeByMetadataName("Generic`1")!
            .TypeParameters[0];
        Assert.IsFalse((bool)tupleLike.Invoke(null, [genericType])!);

        var findToString = GetPrivateStatic("FindParameterlessToStringMethod", typeof(INamedTypeSymbol));
        var toString = (IMethodSymbol)findToString.Invoke(null, [customType])!;
        Assert.AreEqual("ToString", toString.Name);

        var append = typeof(GeneratedSourceMapWriter).GetMethod(
            "AppendString",
            BindingFlags.Static | BindingFlags.NonPublic)!;
        var builder = new StringBuilder();
        append.Invoke(null, [builder, "\"\\\b\f\n\r\t\u0001text"]);
        Assert.AreEqual("\"\\\"\\\\\\b\\f\\n\\r\\t\\u0001text\"", builder.ToString());

        var trackingType = typeof(SourceMapEmitter).GetNestedType(
            "TrackingStringWriter",
            BindingFlags.NonPublic)!;
        var tracking = Activator.CreateInstance(trackingType)!;
        trackingType.GetMethod("Write", [typeof(string)])!.Invoke(tracking, ["a\r\nb\nc"]);
        Assert.AreEqual(2, trackingType.GetProperty("Line")!.GetValue(tracking));
        Assert.AreEqual(1, trackingType.GetProperty("Column")!.GetValue(tracking));

        var collectorType = typeof(SourceMapEmitter).GetNestedType(
            "SourceMapCaptureCollector",
            BindingFlags.NonPublic)!;
        var collector = collectorType.GetConstructors(
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
            .Single()
            .Invoke([tracking, false, false]);
        var captures = collectorType.GetProperty("Captures")!;
        var positions = collectorType.GetProperty("NodePositions")!;
        Assert.IsInstanceOfType<InvalidOperationException>(
            Assert.Throws<TargetInvocationException>(() => captures.GetValue(collector)).InnerException);
        Assert.IsInstanceOfType<InvalidOperationException>(
            Assert.Throws<TargetInvocationException>(() => positions.GetValue(collector)).InnerException);

        var createKey = typeof(ImportDeclarationFactory).GetMethod(
            "CreateSpecifierKey",
            BindingFlags.Static | BindingFlags.NonPublic)!;
        _ = createKey.Invoke(null, [new ImportDefaultSpecifier(new Identifier("defaultLocal"))]);
        _ = createKey.Invoke(null, [new ImportNamespaceSpecifier(new Identifier("namespaceLocal"))]);
        _ = createKey.Invoke(null, [new ImportSpecifier(new StringLiteral("named", "\"named\""), new Identifier("local"))]);

        var guard = GetPrivateStatic(typeof(Optimizer), "IsNonNullGuardFor", typeof(Expression), typeof(Expression));
        var value = new Identifier("value");
        var strictLeft = new Acornima.Ast.NonLogicalBinaryExpression(
            Operator.StrictInequality,
            new NullLiteral("null"),
            value);
        Assert.IsTrue((bool)guard.Invoke(null, [strictLeft, value])!);
    }

    [TestMethod]
    public void NullableCompileBoundary_RejectsUnboundPropertyShape()
    {
        var walker = new SemanticWalker(true);
        Assert.Throws<InvalidOperationException>(() => walker.CompileNullableValue(
            null!,
            new SenseArgument(),
            new Identifier("value"),
            Array.Empty<Expression>(),
            null));
    }

    [TestMethod]
    public void TypeAndRuntimeMarkerHelpers_RejectShadowedAndUnannotatedSymbols()
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

            public struct Half { }
            public struct ValueTuple<T> { }

            public class Plain
            {
                public void Method() { }
                public int Property { get; set; }
            }
            """);
        var half = GetPrivateStatic("IsSystemHalfType", typeof(ITypeSymbol));
        var index = GetPrivateStatic("IsSystemIndexType", typeof(ITypeSymbol));
        var range = GetPrivateStatic("IsSystemRangeType", typeof(ITypeSymbol));
        var tuple = GetPrivateStatic("IsTupleLikeHost", typeof(ITypeSymbol));
        Assert.IsFalse((bool)half.Invoke(null, [compilation.GetTypeByMetadataName("Shadow.Half")!])!);
        Assert.IsFalse((bool)half.Invoke(null, [compilation.GetTypeByMetadataName("Half")!])!);
        Assert.IsFalse((bool)index.Invoke(null, [compilation.GetTypeByMetadataName("Shadow.Index")!])!);
        Assert.IsFalse((bool)range.Invoke(null, [compilation.GetTypeByMetadataName("Shadow.Range")!])!);
        Assert.IsFalse((bool)tuple.Invoke(null, [compilation.GetTypeByMetadataName("Shadow.ValueTuple`1")!])!);
        Assert.IsFalse((bool)tuple.Invoke(null, [compilation.GetTypeByMetadataName("ValueTuple`1")!])!);

        var plain = compilation.GetTypeByMetadataName("Plain")!;
        var methodSymbol = plain.GetMembers("Method").OfType<IMethodSymbol>().Single();
        var propertySymbol = plain.GetMembers("Property").OfType<IPropertySymbol>().Single();
        var runtimeMarker = GetPrivateStatic(typeof(Util), "IsRuntimeMarkerType", typeof(ISymbol));
        var moduleMarker = GetPrivateStatic(typeof(Util), "IsECMAScriptModuleType", typeof(ITypeSymbol));
        Assert.IsFalse((bool)runtimeMarker.Invoke(null, [null])!);
        Assert.IsFalse((bool)moduleMarker.Invoke(null, [null])!);

        var inlineTemplate = GetPrivateStatic(typeof(Util), "HasECMAScriptInlineTemplate", typeof(IMethodSymbol));
        Assert.IsFalse((bool)inlineTemplate.Invoke(null, [methodSymbol])!);
        var proxyProperty = GetPrivateStatic(typeof(Util), "IsECMAScriptRecordProxyProperty", typeof(IPropertySymbol));
        var proxyMethod = GetPrivateStatic(typeof(Util), "IsECMAScriptRecordProxyMethod", typeof(IMethodSymbol));
        Assert.IsFalse((bool)proxyProperty.Invoke(null, [propertySymbol])!);
        Assert.IsFalse((bool)proxyMethod.Invoke(null, [methodSymbol])!);
        Assert.IsFalse(Util.IsECMAScriptRecordProxyMember(null));

        var importMapping = GetPrivateStatic(
            typeof(Util),
            "TryGetJazorImportMapping",
            typeof(ISymbol),
            typeof(string).MakeByRefType(),
            typeof(string).MakeByRefType());
        var mappingArgs = new object?[] { methodSymbol, null, null };
        Assert.IsFalse((bool)importMapping.Invoke(null, mappingArgs)!);
        var metadata = (Util.JavaScriptNameMetadata)GetPrivateStatic(
            typeof(Util),
            "GetJavaScriptNameMetadata",
            typeof(ISymbol)).Invoke(null, [methodSymbol])!;
        Assert.IsFalse(metadata.HasECMAScriptNameAttribute);

        var computedKey = GetPrivateStatic("IsObjectLiteralComputedKeyType", typeof(ITypeSymbol));
        var numericKey = GetPrivateStatic("IsObjectLiteralNumericKeyType", typeof(ITypeSymbol));
        Assert.IsFalse((bool)computedKey.Invoke(null, [null])!);
        Assert.IsFalse((bool)numericKey.Invoke(null, [null])!);

        var guard = GetPrivateStatic(typeof(Optimizer), "IsNonNullGuardFor", typeof(Expression), typeof(Expression));
        var value = new Identifier("value");
        var unrelated = new Acornima.Ast.NonLogicalBinaryExpression(
            Operator.StrictInequality,
            new Identifier("left"),
            new Identifier("right"));
        Assert.IsFalse((bool)guard.Invoke(null, [unrelated, value])!);
    }

    [TestMethod]
    public void RuntimeHostAndLiteralHelpers_RejectNonRuntimeShapes()
    {
        var compilation = CreateCompilation(
            """
            public sealed class Plain
            {
                public int Value;
                public void Method() { }
                public static void StaticMethod() { }
            }

            public enum PlainEnum
            {
                Value = 1
            }

            namespace Other
            {
                public sealed class Exception
                {
                }
            }

            public sealed class Holder
            {
                void TestMethod()
                {
                    var value = 1;
                    var key = value;
                }
            }
            """);
        var plain = compilation.GetTypeByMetadataName("Plain")!;
        var otherException = compilation.GetTypeByMetadataName("Other.Exception")!;
        var methodSymbol = plain.GetMembers("Method").OfType<IMethodSymbol>().Single();
        var staticMethodSymbol = plain.GetMembers("StaticMethod").OfType<IMethodSymbol>().Single();
        var fieldSymbol = plain.GetMembers("Value").OfType<IFieldSymbol>().Single();
        var walker = new SemanticWalker(true);

        var nativeError = GetPrivateStatic(
            "IsNativeErrorConstructorFallbackAllowed",
            typeof(ITypeSymbol),
            typeof(string));
        Assert.IsFalse((bool)nativeError.Invoke(null, [plain, "Error"])!);
        Assert.IsFalse((bool)nativeError.Invoke(null, [plain, "Other"])!);
        Assert.IsFalse((bool)nativeError.Invoke(null, [otherException, "Error"])!);
        var systemException = compilation.GetTypeByMetadataName("System.Exception");
        if (systemException is not null)
            Assert.IsTrue((bool)nativeError.Invoke(null, [systemException, "Error"])!);

        var specialized = GetPrivateStatic("TryGetSpecializedRuntimeHostType", typeof(ITypeSymbol));
        Assert.IsNull(specialized.Invoke(null, [plain]));
        var listOfInt = compilation.GetTypeByMetadataName("System.Collections.Generic.List`1")!
            .Construct(compilation.GetSpecialType(SpecialType.System_Int32));
        Assert.IsNull(specialized.Invoke(null, [listOfInt]));

        var specializedCompilation = CreateEcmaCompilation(
            """
            using ECMAScript;

            [ECMAScript]
            public class TypedArray<T, TArray>
                where TArray : TypedArray<T, TArray>
            {
            }

            [ECMAScript]
            public sealed class Uint8Array : TypedArray<byte, Uint8Array>
            {
            }
            """);
        var typedArray = specializedCompilation.GetTypeByMetadataName("TypedArray`2")!
            .Construct(
                specializedCompilation.GetSpecialType(SpecialType.System_Byte),
                specializedCompilation.GetTypeByMetadataName("Uint8Array")!);
        var specializedType = (ITypeSymbol)specialized.Invoke(null, [typedArray])!;
        Assert.AreEqual("Uint8Array", specializedType.Name);

        var genericParameter = compilation.GetTypeByMetadataName("Holder")!
            .GetMembers("TestMethod")
            .OfType<IMethodSymbol>()
            .Single()
            .TypeParameters
            .FirstOrDefault();
        if (genericParameter is not null)
            Assert.IsNull(specialized.Invoke(null, [genericParameter]));
        else
        {
            var genericCompilation = CreateCompilation("public sealed class Generic<T> { }");
            genericParameter = genericCompilation.GetTypeByMetadataName("Generic`1")!.TypeParameters[0];
            Assert.IsNull(specialized.Invoke(null, [genericParameter]));
        }
        var runtimeHost = GetPrivateInstance(
            "TryBuildRuntimeHostExpression",
            typeof(ITypeSymbol),
            typeof(Nullable<SenseArgument>));
        Assert.IsNotNull(runtimeHost.Invoke(walker, [plain, null]));
        Assert.IsNotNull(runtimeHost.Invoke(walker, [genericParameter, null]));
        var extensionTarget = GetPrivateInstance(
            "TryBuildExtensionHostTarget",
            typeof(IMethodSymbol),
            typeof(Nullable<SenseArgument>));
        Assert.IsNull(extensionTarget.Invoke(walker, [methodSymbol, null]));
        Assert.IsNull(extensionTarget.Invoke(walker, [staticMethodSymbol, null]));
        var globalType = compilation.GetTypeByMetadataName("ECMAScript.Global");
        var globalMethod = globalType?.GetMembers("NumberValue").OfType<IMethodSymbol>().FirstOrDefault();
        if (globalMethod is not null)
            Assert.IsNull(extensionTarget.Invoke(walker, [globalMethod, null]));

        var normalize = GetPrivateInstance(
            "NormalizeRuntimeReceiverHostCallee",
            typeof(Expression),
            typeof(IMethodSymbol));
        var member = new MemberExpression(
            new Identifier("different"),
            new Identifier("Method"),
            computed: false,
            optional: false);
        Assert.AreSame(member, normalize.Invoke(walker, [member, methodSymbol]));
        var matchingMember = new MemberExpression(
            new Identifier("Plain"),
            new Identifier("Method"),
            computed: false,
            optional: false);
        Assert.IsNotNull(normalize.Invoke(walker, [matchingMember, methodSymbol]));

        var enumLiteral = GetPrivateStatic(
            "TryBuildStringEnumLiteral",
            typeof(IFieldSymbol),
            typeof(Expression).MakeByRefType());
        var enumField = compilation.GetTypeByMetadataName("PlainEnum")!
            .GetMembers("Value")
            .OfType<IFieldSymbol>()
            .Single();
        var enumArgs = new object?[] { enumField, null };
        Assert.IsFalse((bool)enumLiteral.Invoke(null, enumArgs)!);
        var plainFieldArgs = new object?[] { fieldSymbol, null };
        Assert.IsFalse((bool)enumLiteral.Invoke(null, plainFieldArgs)!);

        var tree = compilation.SyntaxTrees.Single();
        var model = compilation.GetSemanticModel(tree);
        var initializer = tree.GetRoot().DescendantNodes()
            .OfType<VariableDeclaratorSyntax>()
            .Single(static declarator => declarator.Identifier.ValueText == "value")
            .Initializer!;
        var literalOperation = model.GetOperation(initializer.Value)!;
        var variableReference = tree.GetRoot().DescendantNodes()
            .OfType<IdentifierNameSyntax>()
            .Single(static identifier => identifier.Identifier.ValueText == "value");
        var variableOperation = model.GetOperation(variableReference)!;
        var numericKey = GetPrivateStatic(
            "TryCreateNumericObjectPropertyKey",
            typeof(IOperation),
            typeof(ITypeSymbol),
            typeof(Expression).MakeByRefType(),
            typeof(bool).MakeByRefType());
        var numericArgs = new object?[]
        {
            literalOperation,
            compilation.GetSpecialType(SpecialType.System_Int32),
            null,
            false
        };
        Assert.IsFalse((bool)numericKey.Invoke(walker, numericArgs)!);
        var numberType = compilation.GetTypeByMetadataName("ECMAScript.Number");
        if (numberType is not null)
        {
            numericArgs[0] = literalOperation;
            numericArgs[1] = numberType;
            Assert.IsTrue((bool)numericKey.Invoke(null, numericArgs)!);
            numericArgs[0] = variableOperation;
            numericArgs[1] = numberType;
            Assert.IsFalse((bool)numericKey.Invoke(null, numericArgs)!);
        }
    }

    [TestMethod]
    public void RuntimeAndUsingBoundaryHelpers_CoverUnboundReceiverAndDisposeShapes()
    {
        var compilation = CreateCompilation(
            """
            using System;

            public sealed class Generic<T>
            {
            }

            public sealed class DisposableGeneric<T>
                where T : IDisposable
            {
            }

            public sealed class DisposableResource : IDisposable
            {
                public void Dispose()
                {
                }
            }
            """);
        var intType = compilation.GetSpecialType(SpecialType.System_Int32);
        var typeParameter = compilation.GetTypeByMetadataName("Generic`1")!.TypeParameters[0];
        var disposableTypeParameter = compilation.GetTypeByMetadataName("DisposableGeneric`1")!.TypeParameters[0];
        var disposableResource = compilation.GetTypeByMetadataName("DisposableResource")!;

        var disposeByInterface = GetPrivateStatic(
            "TryResolveUsingDisposeMethodByInterface",
            typeof(ITypeSymbol),
            typeof(string),
            typeof(string),
            typeof(IMethodSymbol).MakeByRefType());
        var interfaceArgs = new object?[] { intType, "System.IDisposable", "Dispose", null };
        Assert.IsFalse((bool)disposeByInterface.Invoke(null, interfaceArgs)!);
        interfaceArgs = [disposableResource, "System.IDisposable", "Dispose", null];
        Assert.IsTrue((bool)disposeByInterface.Invoke(null, interfaceArgs)!);

        var disposeByConstraint = GetPrivateStatic(
            "TryResolveUsingTypeParameterDisposeMethod",
            typeof(ITypeParameterSymbol),
            typeof(string),
            typeof(string),
            typeof(IMethodSymbol).MakeByRefType());
        var constraintArgs = new object?[] { typeParameter, "System.IDisposable", "Dispose", null };
        Assert.IsFalse((bool)disposeByConstraint.Invoke(null, constraintArgs)!);
        constraintArgs = [disposableTypeParameter, "System.IDisposable", "Dispose", null];
        Assert.IsTrue((bool)disposeByConstraint.Invoke(null, constraintArgs)!);

        var ecmaCompilation = CSharpCompilation.Create(
            "CompilerPureBoundaryEcma_" + Guid.NewGuid().ToString("N"),
            [CSharpSyntaxTree.ParseText(
                "using ECMAScript; public sealed class Host { public void Run() { } }",
                TestMetadataReferences.PreviewParseOptions)],
            TestMetadataReferences.Net11.Add(
                MetadataReference.CreateFromFile(typeof(ECMAScript.Global).Assembly.Location))
                .Add(MetadataReference.CreateFromFile(typeof(ECMAScript.ECMAScriptAttribute).Assembly.Location)),
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        var errors = ecmaCompilation.GetDiagnostics()
            .Where(static diagnostic => diagnostic.Severity == DiagnosticSeverity.Error)
            .ToArray();
        Assert.IsEmpty(errors, string.Join(Environment.NewLine, errors.Select(static error => error.ToString())));

        var global = ecmaCompilation.GetTypeByMetadataName("ECMAScript.Global")!;
        var numberFn = global.GetMembers("NumberValue")
            .OfType<IMethodSymbol>()
            .Single(static method => method.Parameters.Length == 0);
        var extensionTarget = GetPrivateInstance(
            "TryBuildExtensionHostTarget",
            typeof(IMethodSymbol),
            typeof(Nullable<SenseArgument>));
        Assert.IsNull(extensionTarget.Invoke(new SemanticWalker(true), [numberFn, null]));

        var extensionMethod = ecmaCompilation.GlobalNamespace
            .GetNamespaceMembers()
            .SelectMany(static space => space.GetTypeMembers())
            .SelectMany(static type => type.GetMembers().OfType<IMethodSymbol>())
            .FirstOrDefault(static method => method.Name == "AbsFn" && method.IsStatic);
        if (extensionMethod is not null)
            Assert.IsNull(extensionTarget.Invoke(new SemanticWalker(true), [extensionMethod, null]));

        var intToString = intType.GetMembers("ToString")
            .OfType<IMethodSymbol>()
            .Single(static method => method.Parameters.Length == 1 &&
                method.Parameters[0].Type.SpecialType == SpecialType.System_String);
        var hexIntrinsic = GetPrivateStatic(
            "TryBuildIntegerHexToStringIntrinsic",
            typeof(IMethodSymbol),
            typeof(Expression),
            typeof(IReadOnlyList<Expression>),
            typeof(Expression).MakeByRefType());
        var invalidFormatArgs = new object?[]
        {
            intToString,
            new Identifier("value"),
            new Expression[] { new StringLiteral("D", "\"D\"") },
            null
        };
        Assert.IsFalse((bool)hexIntrinsic.Invoke(null, invalidFormatArgs)!);
    }

    [TestMethod]
    public void ConverterAndScopePrivateHelpers_HandleFallbackShapes()
    {
        var importedName = GetPrivateStatic(
            typeof(AstConverter),
            "GetImportedSpecifierName",
            typeof(ImportDeclarationSpecifier));
        Assert.AreEqual(
            "named",
            importedName.Invoke(null, [new ImportSpecifier(new Identifier("named"), new Identifier("local"))]));
        Assert.AreEqual(
            "named-string",
            importedName.Invoke(
                null,
                [new ImportSpecifier(new StringLiteral("named-string", "\"named-string\""), new Identifier("local"))]));
        Assert.AreEqual("default", importedName.Invoke(null, [new ImportDefaultSpecifier(new Identifier("local"))]));
        Assert.AreEqual("*", importedName.Invoke(null, [new ImportNamespaceSpecifier(new Identifier("local"))]));
        var fallbackSpecifier = new ImportSpecifier(
            new NumericLiteral(1, "1"),
            new Identifier("local"));
        Assert.AreEqual("local", importedName.Invoke(null, [fallbackSpecifier]));

        var compilation = CreateCompilation(
            """
            public sealed class Holder
            {
                public int Field;
                public const int Constant = 1;
                public int Initialized = 1;
            }
            """);
        var tree = compilation.SyntaxTrees.Single();
        var model = compilation.GetSemanticModel(tree);
        var holder = compilation.GetTypeByMetadataName("Holder")!;
        var converter = new AstConverter(holder, model);
        var defaultValue = GetPrivateInstance(
            typeof(AstConverter),
            "GetMemberFieldDefaultValue",
            typeof(IFieldSymbol));
        var field = holder.GetMembers("Field").OfType<IFieldSymbol>().Single();
        Assert.IsNotNull(defaultValue.Invoke(converter, [field]));
        var constant = holder.GetMembers("Constant").OfType<IFieldSymbol>().Single();
        Assert.IsNotNull(defaultValue.Invoke(converter, [constant]));

        var materialize = GetPrivateStatic(
            "MaterializeScopedStatements",
            typeof(SenseArgument),
            typeof(IEnumerable<Statement>));
        var empty = (List<Statement>)materialize.Invoke(
            null,
            [new SenseArgument(), Array.Empty<Statement>()])!;
        Assert.IsEmpty(empty);
        var withDeclarator = new SenseArgument();
        withDeclarator.AddVarDeclarator(
            new VariableDeclarator(new Identifier("value"), new NumericLiteral(1, "1")),
            depth: 0);
        var materialized = (List<Statement>)materialize.Invoke(
            null,
            [withDeclarator, Array.Empty<Statement>()])!;
        Assert.HasCount(1, materialized);
    }

    [TestMethod]
    public void UsingHostBoundary_RecognizesSkippedVariableGroupAndDeclarationOperations()
    {
        var compilation = CreateCompilation(
            """
            public sealed class TestClass
            {
                void TestMethod()
                {
                    var value = 1;
                }
            }
            """);
        var tree = compilation.SyntaxTrees.Single();
        var model = compilation.GetSemanticModel(tree);
        var body = tree.GetRoot().DescendantNodes()
            .OfType<MethodDeclarationSyntax>()
            .Single(static method => method.Identifier.ValueText == "TestMethod")
            .Body!;
        var block = Assert.IsInstanceOfType<IBlockOperation>(model.GetOperation(body));
        var group = block.DescendantsAndSelf()
            .OfType<IVariableDeclarationGroupOperation>()
            .Single();
        var declaration = group.Declarations.Single();

        var walker = new SemanticWalker(true)
        {
            Host = new SkipVariableHost()
        };
        var method = GetPrivateInstance(
            "IsHostSkippedVariableDeclaration",
            typeof(IOperation),
            typeof(SenseArgument));

        Assert.IsTrue((bool)method.Invoke(walker, [group, new SenseArgument()])!);
        Assert.IsTrue((bool)method.Invoke(walker, [declaration, new SenseArgument()])!);

        var ordinaryWalker = new SemanticWalker(true);
        Assert.IsFalse((bool)method.Invoke(ordinaryWalker, [group, new SenseArgument()])!);
        Assert.IsFalse((bool)method.Invoke(ordinaryWalker, [declaration, new SenseArgument()])!);
    }

    [TestMethod]
    public void JavaScriptPropertyNameHelpers_CoverQuotedAndInvalidIdentifierForms()
    {
        var parse = GetPrivateStatic(
            "TryParseExplicitComputedAliasProperty",
            typeof(string),
            typeof(Expression).MakeByRefType(),
            typeof(string).MakeByRefType());
        foreach (var authored in new[] { "['single-quoted']", "[\"double-quoted\"]", "[1]", "[-1]", "[unquoted]", "[]" })
        {
            var arguments = new object?[] { authored, null, null };
            _ = parse.Invoke(null, arguments);
        }

        var identifier = GetPrivateStatic("IsJavaScriptIdentifierName", typeof(string));
        Assert.IsTrue((bool)identifier.Invoke(null, ["alpha_1$beta"])!);
        Assert.IsFalse((bool)identifier.Invoke(null, [""])!);
        Assert.IsFalse((bool)identifier.Invoke(null, ["9startsWithDigit"])!);
        Assert.IsFalse((bool)identifier.Invoke(null, ["contains-dash"])!);
    }

    [TestMethod]
    public void PatternBoundaryHelpers_CoverReferenceFallbackAndInterfaceProofShapes()
    {
        var compilation = CreateEcmaCompilation(
            """
            using System;
            using ECMAScript;

            [ECMAScript]
            public interface IMarker
            {
            }

            public class MarkerBase : IMarker
            {
            }

            public sealed class Generic<T>
                where T : MarkerBase
            {
            }

            public sealed class GenericPlain<T>
                where T : Plain
            {
            }

            public class Plain
            {
            }

            public sealed record RecordValue(int Value);

            public sealed class TestClass
            {
                bool TestMethod(string text, object value, int number)
                {
                    var empty = text is { };
                    var typed = value is IMarker;
                    var impossible = new Plain() is IComparable;
                    var combined = number is > 0 and < 10;
                    var property = text is { Length: 1 };
                    var defaultText = default(string);
                    var defaultNumber = default(int);
                    var defaultNullable = default(int?);
                    var valueType = number is { };
                    return empty || typed || impossible || combined || property ||
                        defaultText is not null || defaultNumber is 0 || defaultNullable is null || valueType;
                }
            }
            """);
        var tree = compilation.SyntaxTrees.Single();
        var model = compilation.GetSemanticModel(tree);
        var methodSyntax = tree.GetRoot().DescendantNodes()
            .OfType<MethodDeclarationSyntax>()
            .Single(static method => method.Identifier.ValueText == "TestMethod");
        var block = Assert.IsInstanceOfType<IBlockOperation>(model.GetOperation(methodSyntax.Body!));
        var walker = new SemanticWalker(true);
        var recursive = block.DescendantsAndSelf().OfType<IRecursivePatternOperation>().First();
        var binary = block.DescendantsAndSelf().OfType<IBinaryPatternOperation>().FirstOrDefault();
        if (binary is not null)
        {
            var visitBinary = typeof(SemanticWalker).GetMethod(
                "VisitBinaryPattern",
                BindingFlags.Instance | BindingFlags.Public)!;
            Assert.Throws<TargetInvocationException>(() => visitBinary.Invoke(walker, [binary, new SenseArgument()]));
        }
        var nestedConstant = block.DescendantsAndSelf().OfType<IConstantPatternOperation>().FirstOrDefault();

        var patternReference = GetPrivateInstance(
            "GetPatternRefrence",
            typeof(IOperation),
            typeof(SenseArgument));
        var missingInput = Assert.Throws<TargetInvocationException>(() =>
            patternReference.Invoke(walker, [recursive, new SenseArgument()]));
        Assert.IsInstanceOfType<InvalidOperationException>(missingInput.InnerException);

        var fallback = GetPrivateStatic(
            "BuildRecursivePatternFallbackMatch",
            typeof(IRecursivePatternOperation),
            typeof(Expression));
        var fallbackExpression = (Expression)fallback.Invoke(
            null,
            [recursive, new Identifier("text")])!;
        StringAssert.Contains(fallbackExpression.ToKnRECMAScript(), "!= null", StringComparison.Ordinal);
        var valueTypeRecursive = block.DescendantsAndSelf()
            .OfType<IRecursivePatternOperation>()
            .Single(static operation => operation.MatchedType?.SpecialType == SpecialType.System_Int32);
        var valueTypeFallback = (Expression)fallback.Invoke(
            null,
            [valueTypeRecursive, new Identifier("number")])!;
        Assert.AreEqual("true", valueTypeFallback.ToKnRECMAScript());

        var resolveInputType = GetPrivateStatic("ResolvePatternInputStaticType", typeof(IOperation));
        Assert.IsNull(resolveInputType.Invoke(null, [block]));
        Assert.AreEqual(
            recursive.InputType,
            resolveInputType.Invoke(null, [recursive]));

        if (nestedConstant is not null)
        {
            var resolveSource = GetPrivateStatic("ResolveIsTypeSourceOperation", typeof(IOperation));
            Assert.IsNull(resolveSource.Invoke(null, [nestedConstant]));
        }

        var defaultOperation = block.DescendantsAndSelf().OfType<IDefaultValueOperation>()
            .Single(static operation => operation.Type?.SpecialType == SpecialType.System_String);
        var deterministic = typeof(SemanticWalker)
            .GetMethods(BindingFlags.Instance | BindingFlags.NonPublic)
            .Single(static method => method.Name == "TryResolveDeterministicRuntimeValue");
        var deterministicArgs = new object?[] { defaultOperation, null, false };
        Assert.IsTrue((bool)deterministic.Invoke(walker, deterministicArgs)!);
        Assert.IsNull(deterministicArgs[1]);
        Assert.IsFalse((bool)deterministicArgs[2]!);
        var valueTypeDefault = block.DescendantsAndSelf().OfType<IDefaultValueOperation>()
            .Single(static operation => operation.Type?.SpecialType == SpecialType.System_Int32);
        var valueTypeDefaultArgs = new object?[] { valueTypeDefault, null, false };
        Assert.IsTrue((bool)deterministic.Invoke(walker, valueTypeDefaultArgs)!);
        Assert.AreEqual(compilation.GetSpecialType(SpecialType.System_Int32), valueTypeDefaultArgs[1]);
        Assert.IsTrue((bool)valueTypeDefaultArgs[2]!);

        var unsupportedMessage = GetPrivateInstance(
            "BuildUnsupportedErasedInterfaceIsTypeCheckMessage",
            typeof(IOperation),
            typeof(ITypeSymbol));
        var interfaceType = compilation.GetTypeByMetadataName("IMarker")!;
        var message = (string)unsupportedMessage.Invoke(walker, [block, interfaceType])!;
        StringAssert.Contains(message, "source type is unknown", StringComparison.Ordinal);

        var purePropertyChain = GetPrivateStatic("IsPurePropertyAccessChain", typeof(Expression));
        Assert.IsTrue((bool)purePropertyChain.Invoke(null, [new Identifier("value")])!);
        Assert.IsFalse((bool)purePropertyChain.Invoke(null, [new MemberExpression(new Identifier("value"), new Identifier("property"), computed: true, optional: false)])!);
        Assert.IsTrue((bool)purePropertyChain.Invoke(
            null,
            [new MemberExpression(
                new Identifier("value"),
                new StringLiteral("property", "\"property\""),
                computed: true,
                optional: false)])!);
        Assert.IsTrue((bool)purePropertyChain.Invoke(
            null,
            [new MemberExpression(
                new Identifier("value"),
                new Identifier("property"),
                computed: false,
                optional: false)])!);

        var evaluateInterface = typeof(SemanticWalker)
            .GetMethods(BindingFlags.Instance | BindingFlags.NonPublic)
            .Single(static method => method.Name == "TryEvaluateCompileTimeErasedInterfaceIsTypeCheck");
        var evaluateParameters = new object?[] { block, interfaceType, null };
        Assert.IsFalse((bool)evaluateInterface.Invoke(walker, evaluateParameters)!);

        var impossibleIsType = block.DescendantsAndSelf()
            .OfType<IIsTypeOperation>()
            .Single(static operation => operation.ValueOperand.Type?.Name == "Plain");
        var comparableType = compilation.GetTypeByMetadataName("System.IComparable")!;
        var createTypeMatch = GetPrivateInstance(
            "CreateTypeMatchExpr",
            typeof(IOperation),
            typeof(ITypeSymbol),
            typeof(Expression),
            typeof(SenseArgument));
        var foldedFalse = (Expression)createTypeMatch.Invoke(
            walker,
            [impossibleIsType, comparableType, new Identifier("value"), new SenseArgument()])!;
        Assert.AreEqual("false", foldedFalse.ToKnRECMAScript());

        var assignable = GetPrivateStatic(
            "IsRuntimeTypeAssignableToInterface",
            typeof(ITypeSymbol),
            typeof(ITypeSymbol));
        var markerBase = compilation.GetTypeByMetadataName("MarkerBase")!;
        var genericParameter = compilation.GetTypeByMetadataName("Generic`1")!.TypeParameters[0];
        var genericPlainParameter = compilation.GetTypeByMetadataName("GenericPlain`1")!.TypeParameters[0];
        Assert.IsTrue((bool)assignable.Invoke(null, [genericParameter, interfaceType])!);
        Assert.IsFalse((bool)assignable.Invoke(null, [genericPlainParameter, interfaceType])!);
        Assert.IsFalse((bool)assignable.Invoke(null, [markerBase, compilation.GetSpecialType(SpecialType.System_IDisposable)])!);

        var classMatch = GetPrivateInstance(
            "BuildClassTypeMatch",
            typeof(IOperation),
            typeof(ITypeSymbol),
            typeof(Expression),
            typeof(string),
            typeof(Nullable<SenseArgument>));
        var structuralException = Assert.Throws<TargetInvocationException>(() =>
            classMatch.Invoke(
                walker,
                [block, compilation.GetTypeByMetadataName("RecordValue")!, new Identifier("value"), "RecordValue", null]));
        Assert.IsInstanceOfType<OperationTransformationException>(structuralException.InnerException);

        var declaredIdentifier = GetPrivateInstance(
            "CreatePatternDeclaredSymbolIdentifier",
            typeof(ISymbol),
            typeof(IOperation),
            typeof(SenseArgument));
        var parameter = compilation.GetTypeByMetadataName("TestClass")!
            .GetMembers("TestMethod")
            .OfType<IMethodSymbol>()
            .Single()
            .Parameters
            .First();
        var declared = (Identifier)declaredIdentifier.Invoke(
            walker,
            [parameter, recursive, new SenseArgument()])!;
        Assert.AreEqual(parameter.Name, declared.Name);
    }

    [TestMethod]
    public void ListPatternFallbackHelpers_CoverCustomLengthIndexerAndSliceBranches()
    {
        var compilation = CreateCompilation(
            """
            public sealed class Buffer
            {
                public int Length => 4;

                public int this[int index] => index;

                public int[] Slice(int start, int length) => [];
            }

            public sealed class TestClass
            {
                bool TestMethod(Buffer buffer)
                {
                    return buffer is [1, .. var rest];
                }
            }
            """);
        var tree = compilation.SyntaxTrees.Single();
        var model = compilation.GetSemanticModel(tree);
        var methodSyntax = tree.GetRoot().DescendantNodes()
            .OfType<MethodDeclarationSyntax>()
            .Single(static method => method.Identifier.ValueText == "TestMethod");
        var block = Assert.IsInstanceOfType<IBlockOperation>(model.GetOperation(methodSyntax.Body!));
        var listPattern = block.DescendantsAndSelf().OfType<IListPatternOperation>().Single();
        var slicePattern = listPattern.Patterns.OfType<ISlicePatternOperation>().Single();
        var walker = new SemanticWalker(true);
        var target = new Identifier("buffer");

        var lengthAccess = GetPrivateInstance(
            "BuildListPatternLengthAccess",
            typeof(IListPatternOperation),
            typeof(Expression),
            typeof(SenseArgument),
            typeof(bool),
            typeof(ITypeSymbol));
        Assert.Throws<TargetInvocationException>(() => lengthAccess.Invoke(
            walker,
            [listPattern, target, new SenseArgument(), false, listPattern.InputType]));

        var indexAccess = GetPrivateInstance(
            "BuildListPatternIndexerAccess",
            typeof(IListPatternOperation),
            typeof(Expression),
            typeof(Expression),
            typeof(SenseArgument),
            typeof(bool),
            typeof(ITypeSymbol));
        Assert.Throws<TargetInvocationException>(() => indexAccess.Invoke(
            walker,
            [listPattern, target, new NumericLiteral(0, "0"), new SenseArgument(), false, listPattern.InputType]));

        var sliceAccess = GetPrivateInstance(
            "BuildListPatternSliceAccess",
            typeof(IListPatternOperation),
            typeof(ISlicePatternOperation),
            typeof(Expression),
            typeof(Expression),
            typeof(int),
            typeof(SenseArgument),
            typeof(bool),
            typeof(ITypeSymbol));
        Assert.Throws<TargetInvocationException>(() => sliceAccess.Invoke(
            walker,
            [listPattern, slicePattern, target, new MemberExpression(target, new Identifier("length"), false, false), 1, new SenseArgument(), false, listPattern.InputType]));
    }

    [TestMethod]
    public void SymbolNameMetadataHelpers_CoverDescriptionBoundariesAndBackingFieldShapes()
    {
        var compilation = CreateEcmaCompilation(
            """
            using System;
            using System.ComponentModel;
            using ECMAScript;

            [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method | AttributeTargets.Property)]
            public sealed class DummyAttribute : Attribute
            {
            }

            [ECMAScript]
            [Description("@#")]
            public sealed class Host
            {
                [ECMAScriptName("")]
                public int Blank { get; set; }

                [Description("@#display-name")]
                public int Described { get; set; }

                public int Plain { get; set; }

                public (int First, int Second) Pair;

                [ECMAScriptName("configured")]
                public static void Overload(int value) { }

                public static void Overload(string value) { }
            }

            [ECMAScript]
            [Description("@#")]
            public sealed record RecordHost
            {
                [Description("not an ECMAScript name")]
                public int GetterOnly { get; } = 1;

                [Description("not an inline template")]
                public static void DecoratedButNotInline() { }
            }

            [Dummy]
            [Description("plain CLR host")]
            public static class PlainHost
            {
                public static void Unique() { }

                public static void LocalContainer()
                {
                    void Local() { }
                    Local();
                }
            }

            [ECMAScriptModule("runtime/host.mjs")]
            public static class ModuleHost
            {
                [ECMAScriptName("configured")]
                public static void Overload(int value) { }

                public static void Overload(string value) { }
            }
            """);
        var host = compilation.GetTypeByMetadataName("Host")!;
        var recordHost = compilation.GetTypeByMetadataName("RecordHost")!;
        var plainHost = compilation.GetTypeByMetadataName("PlainHost")!;
        var blank = host.GetMembers("Blank").OfType<IPropertySymbol>().Single();
        var described = host.GetMembers("Described").OfType<IPropertySymbol>().Single();
        var plain = host.GetMembers("Plain").OfType<IPropertySymbol>().Single();
        var pair = host.GetMembers("Pair").OfType<IFieldSymbol>().Single();
        var backing = host.GetMembers().OfType<IFieldSymbol>()
            .Single(static field => field.IsImplicitlyDeclared && field.AssociatedSymbol?.Name == "Blank");
        var tupleField = ((INamedTypeSymbol)pair.Type).GetMembers("First")
            .OfType<IFieldSymbol>()
            .Single();

        var metadata = GetPrivateStatic(typeof(Util), "GetJavaScriptNameMetadata", typeof(ISymbol));
        var blankMetadata = (Util.JavaScriptNameMetadata)metadata.Invoke(null, [blank])!;
        var describedMetadata = (Util.JavaScriptNameMetadata)metadata.Invoke(null, [described])!;
        Assert.IsTrue(blankMetadata.HasECMAScriptNameAttribute);
        Assert.IsFalse(describedMetadata.HasDescriptionBoundary);
        Assert.AreEqual("display-name", describedMetadata.DescriptionName);

        Assert.AreEqual(
            Jazor.Common.Format.HashName(blank.OriginalDefinition.ToDisplayString(Jazor.Common.Format.NameFormat)),
            Util.GetConfigOrSymbolName(backing));
        Assert.AreEqual(tupleField.Name, Util.GetConfigOrSymbolName(tupleField));
        Assert.AreEqual("Plain", Util.GetConfigOrSymbolName(plain));

        var runtimeMarker = GetPrivateStatic(typeof(Util), "IsRuntimeMarkerType", typeof(ISymbol));
        var moduleMarker = GetPrivateStatic(typeof(Util), "IsECMAScriptModuleType", typeof(ITypeSymbol));
        Assert.IsTrue((bool)runtimeMarker.Invoke(null, [host])!);
        Assert.IsFalse((bool)runtimeMarker.Invoke(null, [plain])!);
        Assert.IsFalse((bool)runtimeMarker.Invoke(null, [plainHost])!);
        Assert.IsTrue((bool)moduleMarker.Invoke(null, [compilation.GetTypeByMetadataName("ModuleHost")!])!);
        Assert.IsFalse((bool)moduleMarker.Invoke(null, [host])!);
        Assert.IsFalse((bool)moduleMarker.Invoke(null, [plainHost])!);

        var getterOnly = recordHost.GetMembers("GetterOnly").OfType<IPropertySymbol>().Single();
        Assert.IsFalse(Util.IsECMAScriptRecordProxyMember(getterOnly));
        var inlineTemplate = GetPrivateStatic(typeof(Util), "HasECMAScriptInlineTemplate", typeof(IMethodSymbol));
        var decoratedButNotInline = recordHost.GetMembers("DecoratedButNotInline").OfType<IMethodSymbol>().Single();
        Assert.IsFalse((bool)inlineTemplate.Invoke(null, [decoratedButNotInline])!);

        var module = compilation.GetTypeByMetadataName("ModuleHost")!;
        var moduleOverloads = module.GetMembers("Overload").OfType<IMethodSymbol>().ToArray();
        Assert.AreEqual("Overload", Util.GetConfigOrSymbolName(moduleOverloads.Single(static method => method.Parameters[0].Type.SpecialType == SpecialType.System_String)));
        var unique = plainHost.GetMembers("Unique").OfType<IMethodSymbol>().Single();
        Assert.AreEqual("Unique", Util.GetConfigOrSymbolName(unique));
        var appendOverload = GetPrivateStatic(
            typeof(Util),
            "AppendMethodOverloadSuffixIfNeeded",
            typeof(ISymbol),
            typeof(string));
        Assert.AreEqual("Unique", appendOverload.Invoke(null, [unique, "Unique"]));
        var tree = compilation.SyntaxTrees.Single();
        var localSyntax = tree.GetRoot().DescendantNodes().OfType<LocalFunctionStatementSyntax>().Single();
        var local = compilation.GetSemanticModel(tree).GetDeclaredSymbol(localSyntax)!;
        Assert.AreEqual("Local", Util.GetConfigOrSymbolName(local));
        Assert.AreEqual("Local", appendOverload.Invoke(null, [local, "Local"]));
    }

    [TestMethod]
    public void ReferenceBoundaryHelpers_CoverNonRuntimeParamsAndHostFallbacks()
    {
        var compilation = CreateEcmaCompilation(
            """
            using ECMAScript;

            [ECMAScript]
            public static class RuntimeHost
            {
                public static void Static() { }
            }

            public class PlainHost
            {
                public int Value { get; set; }

                public int this[int index] => index;

                public static void Expand(params int[] values) { }
            }

            public class GenericHost<T, THost>
                where THost : GenericHost<T, THost>
            {
            }

            public sealed class PlainSpecializedHost : GenericHost<int, PlainSpecializedHost>
            {
            }

            [ECMAScript]
            public class MarkedGenericHost<T, THost>
                where THost : MarkedGenericHost<T, THost>
            {
            }

            public sealed class MarkedSpecializedHost : MarkedGenericHost<int, MarkedSpecializedHost>
            {
            }

            [ECMAScript.String]
            public enum StringState
            {
                Ready = 1
            }

            public enum OrdinaryState
            {
                Value = 1
            }
            """);
        var plainHost = compilation.GetTypeByMetadataName("PlainHost")!;
        var plainExpand = plainHost.GetMembers("Expand").OfType<IMethodSymbol>().Single();
        var value = plainHost.GetMembers("Value").OfType<IPropertySymbol>().Single();
        var indexer = plainHost.GetMembers().OfType<IPropertySymbol>().Single(static property => property.IsIndexer);
        var runtimeHost = compilation.GetTypeByMetadataName("RuntimeHost")!;
        var runtimeStatic = runtimeHost.GetMembers("Static").OfType<IMethodSymbol>().Single();
        var ordinaryValue = compilation.GetTypeByMetadataName("OrdinaryState")!
            .GetMembers("Value").OfType<IFieldSymbol>().Single();

        var paramsExpansion = GetPrivateStatic(
            "TryExpandEcmascriptParamsArgument",
            typeof(IMethodSymbol),
            typeof(IParameterSymbol),
            typeof(Expression),
            typeof(List<Expression>));
        var destination = new List<Expression>();
        Assert.IsFalse((bool)paramsExpansion.Invoke(
            null,
            [plainExpand, plainExpand.Parameters[0], new Identifier("values"), destination])!);
        Assert.IsEmpty(destination);
        Assert.IsFalse((bool)paramsExpansion.Invoke(
            null,
            [runtimeStatic, null, new Identifier("values"), destination])!);

        var stringEnumLiteral = GetPrivateStatic(
            "TryBuildStringEnumLiteral",
            typeof(IFieldSymbol),
            typeof(Expression).MakeByRefType());
        var literalArguments = new object?[] { ordinaryValue, null };
        Assert.IsFalse((bool)stringEnumLiteral.Invoke(null, literalArguments)!);

        var currentIndexer = GetPrivateInstance("IsCurrentModuleRuntimeIndexer", typeof(IPropertySymbol));
        Assert.IsFalse((bool)currentIndexer.Invoke(new SemanticWalker(true), [value])!);
        Assert.IsFalse((bool)currentIndexer.Invoke(new SemanticWalker(true), [indexer])!);
        var moduleWalker = new SemanticWalker(
            plainHost,
            new Dictionary<ISymbol, string>(SymbolEqualityComparer.Default)
            {
                [plainHost] = "PlainHost"
            });
        Assert.IsTrue((bool)currentIndexer.Invoke(moduleWalker, [indexer])!);

        var extensionTarget = GetPrivateInstance(
            "TryBuildExtensionHostTarget",
            typeof(IMethodSymbol),
            typeof(SenseArgument));
        Assert.IsNotNull(extensionTarget.Invoke(new SemanticWalker(true), [runtimeStatic, null]));
        Assert.IsNull(extensionTarget.Invoke(new SemanticWalker(true), [plainExpand, null]));

        var specializedHost = GetPrivateStatic(
            "TryGetSpecializedRuntimeHostType",
            typeof(ITypeSymbol));
        var specializedBase = compilation.GetTypeByMetadataName("PlainSpecializedHost")!.BaseType!;
        Assert.IsNull(specializedHost.Invoke(null, [specializedBase]));
        var runtimeHostExpression = GetPrivateInstance(
            "TryBuildRuntimeHostExpression",
            typeof(ITypeSymbol),
            typeof(SenseArgument));
        var markedSpecializedBase = compilation.GetTypeByMetadataName("MarkedSpecializedHost")!.BaseType!;
        Assert.IsNotNull(specializedHost.Invoke(null, [markedSpecializedBase]));
        Assert.IsNotNull(runtimeHostExpression.Invoke(
            new SemanticWalker(true),
            [markedSpecializedBase, null]));

        var normalize = GetPrivateInstance(
            "NormalizeRuntimeReceiverHostCallee",
            typeof(Expression),
            typeof(IMethodSymbol));
        var identifier = new Identifier("callee");
        Assert.AreSame(identifier, normalize.Invoke(new SemanticWalker(true), [identifier, runtimeStatic]));
        var memberCallee = new MemberExpression(
            new Identifier("RuntimeHost"),
            new Identifier("Static"),
            computed: false,
            optional: false);
        Assert.IsInstanceOfType<MemberExpression>(normalize.Invoke(new SemanticWalker(true), [memberCallee, runtimeStatic]));
    }

    [TestMethod]
    public void ReferenceBoundaryHelpers_CoverUnmatchedStringEnumAndPlainLiteralFallback()
    {
        var compilation = CreateEcmaCompilation(
            """
            using ECMAScript;

            [ECMAScript.String]
            public enum StringState
            {
                Ready = 1,
                Busy = 2
            }

            public enum OrdinaryState
            {
                Value = 1
            }
            """);
        var stringEnum = compilation.GetTypeByMetadataName("StringState")!;
        var ordinaryField = compilation.GetTypeByMetadataName("OrdinaryState")!
            .GetMembers("Value")
            .OfType<IFieldSymbol>()
            .Single();

        var getLiteralText = GetPrivateStatic(
            "GetStringEnumLiteralText",
            typeof(IFieldSymbol));
        Assert.AreEqual("Value", getLiteralText.Invoke(null, [ordinaryField]));

        var buildValueLiteral = GetPrivateStatic(
            "TryBuildStringEnumValueLiteral",
            typeof(INamedTypeSymbol),
            typeof(object),
            typeof(Expression).MakeByRefType());
        var arguments = new object?[] { stringEnum, 999, null };
        Assert.IsFalse((bool)buildValueLiteral.Invoke(null, arguments)!);
        Assert.IsNull(arguments[2]);

        var indexCompilation = CreateCompilation(
            """
            public static class IndexHost
            {
                public static int[] Slice(int[] values, System.Index start)
                    => values[start..];
            }
            """);
        var indexTree = indexCompilation.SyntaxTrees.Single();
        var indexModel = indexCompilation.GetSemanticModel(indexTree);
        var range = indexTree.GetRoot().DescendantNodes()
            .OfType<RangeExpressionSyntax>()
            .Single();
        var indexOperation = indexModel.GetOperation(range.LeftOperand!)!;
        var rangeLength = GetPrivateStatic("RequiresArrayRangeBoundaryLength", typeof(IOperation));
        var indexerLength = GetPrivateStatic("RequiresImplicitIndexerLengthAccess", typeof(IOperation));
        Assert.IsTrue((bool)rangeLength.Invoke(null, [indexOperation])!);
        Assert.IsTrue((bool)indexerLength.Invoke(null, [indexOperation])!);

        var implicitIndexCompilation = CreateCompilation(
            """
            public static class ImplicitIndexHost
            {
                public static int[] Slice(int[] values)
                    => values[1..];
            }
            """);
        var implicitTree = implicitIndexCompilation.SyntaxTrees.Single();
        var implicitModel = implicitIndexCompilation.GetSemanticModel(implicitTree);
        var implicitRange = implicitTree.GetRoot().DescendantNodes()
            .OfType<RangeExpressionSyntax>()
            .Single();
        var implicitIndexOperation = implicitModel.GetOperation(implicitRange.LeftOperand!)!;
        Assert.IsFalse((bool)rangeLength.Invoke(null, [implicitIndexOperation])!);
        Assert.IsFalse((bool)indexerLength.Invoke(null, [implicitIndexOperation])!);
    }

    private static MethodInfo GetPrivateStatic(string name, params Type[] parameterTypes)
        => GetPrivateStatic(typeof(SemanticWalker), name, parameterTypes);

    private static MethodInfo GetPrivateInstance(string name, params Type[] parameterTypes)
        => typeof(SemanticWalker).GetMethod(
            name,
            BindingFlags.Instance | BindingFlags.NonPublic,
            binder: null,
            types: parameterTypes,
            modifiers: null)!;

    private static MethodInfo GetPrivateStatic(Type owner, string name, params Type[] parameterTypes)
        => owner.GetMethod(
            name,
            BindingFlags.Static | BindingFlags.NonPublic,
            binder: null,
            types: parameterTypes,
            modifiers: null)!;

    private static MethodInfo GetPrivateInstance(Type owner, string name, params Type[] parameterTypes)
        => owner.GetMethod(
            name,
            BindingFlags.Instance | BindingFlags.NonPublic,
            binder: null,
            types: parameterTypes,
            modifiers: null)!;

    private static CSharpCompilation CreateCompilation(string source)
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(source, TestMetadataReferences.PreviewParseOptions);
        var compilation = CSharpCompilation.Create(
            "CompilerPureBoundarySweep_" + Guid.NewGuid().ToString("N"),
            [syntaxTree],
            TestMetadataReferences.Net11,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        var errors = compilation.GetDiagnostics()
            .Where(static diagnostic => diagnostic.Severity == DiagnosticSeverity.Error)
            .ToArray();
        Assert.IsEmpty(errors, string.Join(Environment.NewLine, errors.Select(static error => error.ToString())));
        return compilation;
    }

    private static CSharpCompilation CreateEcmaCompilation(string source)
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(source, TestMetadataReferences.PreviewParseOptions);
        var compilation = CSharpCompilation.Create(
            "CompilerPureBoundaryEcma_" + Guid.NewGuid().ToString("N"),
            [syntaxTree],
            TestMetadataReferences.Net11.Add(
                MetadataReference.CreateFromFile(typeof(ECMAScript.Global).Assembly.Location))
                .Add(MetadataReference.CreateFromFile(typeof(ECMAScript.ECMAScriptAttribute).Assembly.Location)),
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        var errors = compilation.GetDiagnostics()
            .Where(static diagnostic => diagnostic.Severity == DiagnosticSeverity.Error)
            .ToArray();
        Assert.IsEmpty(errors, string.Join(Environment.NewLine, errors.Select(static error => error.ToString())));
        return compilation;
    }

    private sealed class SkipVariableHost : SemanticWalkerHost
    {
        public override bool ShouldSkipVariableDeclarator(
            IVariableDeclaratorOperation operation,
            SenseArgument argument)
            => true;
    }
}
