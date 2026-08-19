using Acornima;
using Acornima.Ast;
using Jazor.Compiler;
using Jazor.ComplierTest;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;
using System.Collections.Immutable;
using System.Reflection;
using System.Threading;

namespace Jazor.CompilerTest;

[TestClass]
public sealed class CompilerCoverage98BoundaryTests
{
    [TestMethod]
    public async Task AstConverterRuntimeMemberMatrix_EmitsConstructorsPropertiesEventsAndErasedNestedTypes()
    {
        var compilation = CreateCompilation(
            """
            using System;
            public static class ModuleHost
            {
                public class Base
                {
                    public int Value;
                    public int Property { get; set; }
                    public event Action? Changed;
                    public Base() { }
                    public Base(int value) { Value = value; }
                    public void Raise() => Changed?.Invoke();
                }

                public sealed class Derived : Base
                {
                    public Derived() { }
                    public Derived(int value) : base(value) { }
                    public int Read() => Value;
                }

                public sealed class SingleConstructor
                {
                    public SingleConstructor(int value) { Value = value; }
                    public int Value { get; }
                }

                public sealed class ImplicitDerived : Base
                {
                    public int ReadBase() => Value;
                }

                public sealed class PrimaryConstructor(int value)
                {
                    private readonly int _value = value;
                    public int Read() => _value;
                }

                public enum ErasedKind { First, Second }
                public interface IContract { }
                public record ErasedRecord(int Value);
            }
            """);
        var tree = compilation.SyntaxTrees.Single();
        var model = compilation.GetSemanticModel(tree);
        var module = compilation.GetTypeByMetadataName("ModuleHost")!;
        var converter = new AstConverter(module, model);

        var converted = await converter.Convert();
        Assert.IsNotNull(converted);

        foreach (var runtimeType in module.GetTypeMembers().Where(static type => type.TypeKind == TypeKind.Class && !type.IsRecord))
        {
            var runtimeClass = converter.ConvertRuntimeClass(runtimeType);
            Assert.IsNotNull(runtimeClass);
            StringAssert.Contains(runtimeClass.ToKnRECMAScript(), "class", StringComparison.Ordinal);
        }
    }

    [TestMethod]
    public void AstConverterBoundaries_CoverModuleNameAndBodyNodeShapes()
    {
        var compilation = CreateCompilation(
            """
            public class ModuleHost
            {
                public int Field;
                public int Property { get; set; }
                public void Method() { }
                public record NestedRecord(int Value);
                public sealed class NestedClass { public int Value; }
                public struct NestedStruct { public int StructField; }

                public static void TestMethod(int seed)
                {
                    var local = seed;
                }
            }
            """);
        var module = compilation.GetTypeByMetadataName("ModuleHost")!;
        var converter = new AstConverter(module, compilation.GetSemanticModel(compilation.SyntaxTrees.Single()));

        var sourceName = GetPrivateStatic(
            typeof(AstConverter),
            "GetSourceDeclaredNameCandidate",
            typeof(ISymbol));
        var field = module.GetMembers("Field").OfType<IFieldSymbol>().Single();
        var property = module.GetMembers("Property").OfType<IPropertySymbol>().Single();
        var getter = property.GetMethod!;
        var method = module.GetMembers("Method").OfType<IMethodSymbol>().Single();
        var nested = module.GetTypeMembers("NestedClass").Single();
        var backing = module.GetMembers().OfType<IFieldSymbol>()
            .Single(static candidate => candidate.IsImplicitlyDeclared && candidate.AssociatedSymbol is IPropertySymbol);

        Assert.AreEqual("Field", sourceName.Invoke(null, [field]));
        Assert.AreEqual("Property", sourceName.Invoke(null, [getter]));
        Assert.AreEqual("Method", sourceName.Invoke(null, [method]));
        Assert.AreEqual("NestedClass", sourceName.Invoke(null, [nested]));
        Assert.IsNull(sourceName.Invoke(null, [backing]));
        Assert.AreEqual("", sourceName.Invoke(null, [module.ContainingNamespace]));

        var preferredName = GetPrivateStatic(
            typeof(AstConverter),
            "GetPreferredModuleDeclaredName",
            typeof(ISymbol),
            typeof(AstConverterModulePolicy),
            typeof(AstConverterProfile));
        Assert.AreEqual("Field", preferredName.Invoke(null, [field, AstConverterModulePolicy.Default, AstConverterProfile.Standard]));
        Assert.AreEqual("Method", preferredName.Invoke(null, [method, AstConverterModulePolicy.Default, AstConverterProfile.Standard]));
        Assert.AreEqual("NestedClass", preferredName.Invoke(null, [nested, AstConverterModulePolicy.Default, AstConverterProfile.Standard]));
        Assert.AreEqual("", preferredName.Invoke(null, [module.ContainingNamespace, AstConverterModulePolicy.Default, AstConverterProfile.Standard]));

        var materializeBody = GetPrivateStatic(
            typeof(AstConverter),
            "MaterializeFunctionBody",
            typeof(Node),
            typeof(SenseArgument),
            typeof(bool));
        Assert.IsNotNull(materializeBody.Invoke(null, [new FunctionBody(NodeList.Empty<Statement>(), strict: true), new SenseArgument(), false]));
        Assert.IsNotNull(materializeBody.Invoke(null, [new NestedBlockStatement(NodeList.Empty<Statement>()), new SenseArgument(), false]));
        Assert.IsNotNull(materializeBody.Invoke(null, [new ReturnStatement(null), new SenseArgument(), false]));
        Assert.IsNotNull(materializeBody.Invoke(null, [new Identifier("value"), new SenseArgument(), false]));
        var unsupported = Assert.Throws<TargetInvocationException>(() =>
            materializeBody.Invoke(null, [new VariableDeclarator(new Identifier("value"), null), new SenseArgument(), false]));
        Assert.IsInstanceOfType<InvalidOperationException>(unsupported.InnerException);

        var primaryStorage = GetPrivateInstance(
            typeof(AstConverter),
            "GetPrimaryConstructorParameterStorage",
            typeof(INamedTypeSymbol));
        Assert.IsEmpty((System.Collections.IEnumerable)primaryStorage.Invoke(converter, [module])!);

        var localNames = GetPrivateStatic(
            typeof(AstConverter),
            "BuildModuleLocalNames",
            typeof(INamedTypeSymbol),
            typeof(AstConverterModulePolicy));
        var names = (HashSet<string>)localNames.Invoke(null, [module, AstConverterModulePolicy.Default])!;
        Assert.IsTrue(names.Contains("local"));
        Assert.IsTrue(names.Contains("seed"));
        Assert.IsTrue(names.Contains("StructField"));

        var nestedPolicy = new IncludeNestedTypePolicy();
        var nestedNames = (HashSet<string>)localNames.Invoke(null, [module, nestedPolicy])!;
        Assert.IsFalse(nestedNames.Contains("NestedRecord"));

        var symbolName = GetPrivateInstance(typeof(AstConverter), "GetSymbolName", typeof(ISymbol));
        Assert.AreEqual("Property", symbolName.Invoke(converter, [getter]));
        Assert.AreEqual("Method", symbolName.Invoke(converter, [method]));
    }

    [TestMethod]
    public void SemanticWalkerBoundaries_CoverDefaultAndPatternHelperOutcomes()
    {
        var compilation = CreateCompilation(
            """
            public abstract class AbstractHost { }
            public sealed class PlainHost { }
            public sealed class TestClass
            {
                bool TestMethod(string text, int number)
                {
                    var empty = text is { };
                    var list = new[] { 1, 2 };
                    var listMatch = list is [1, .. var rest];
                    var value = number switch
                    {
                        0 => 1,
                        _ => 2
                    };
                    return empty && listMatch && value > 0;
                }
            }
            """);
        var tree = compilation.SyntaxTrees.Single();
        var model = compilation.GetSemanticModel(tree);
        var methodSyntax = tree.GetRoot().DescendantNodes()
            .OfType<MethodDeclarationSyntax>()
            .Single(static declaration => declaration.Identifier.ValueText == "TestMethod");
        var block = Assert.IsInstanceOfType<IBlockOperation>(model.GetOperation(methodSyntax.Body!));
        var walker = new SemanticWalker(true);

        var labeledSyntax = GetPrivateStatic(
            typeof(SemanticWalker),
            "HasUnmodeledLabeledBranchSyntax",
            typeof(SyntaxNode));
        Assert.IsFalse((bool)labeledSyntax.Invoke(null, [SyntaxFactory.ParseStatement("if (true) { }")])!);
        Assert.IsFalse((bool)labeledSyntax.Invoke(null, [SyntaxFactory.ParseStatement("break;")])!);
        Assert.IsTrue((bool)labeledSyntax.Invoke(null, [SyntaxFactory.ParseStatement("break label;")])!);
        Assert.IsTrue((bool)labeledSyntax.Invoke(null, [SyntaxFactory.ParseStatement("continue label;")])!);

        var enumZero = GetPrivateStatic(typeof(SemanticWalker), "CreateEnumUnderlyingZeroValue", typeof(ITypeSymbol));
        Assert.AreEqual(0, enumZero.Invoke(null, [compilation.GetSpecialType(SpecialType.System_Object)]));

        var supported = GetPrivateInstance(
            typeof(SemanticWalker),
            "IsDefaultValueTypeSupported",
            typeof(ITypeSymbol),
            typeof(Func<ITypeSymbol, bool>));
        var abstractType = compilation.GetTypeByMetadataName("AbstractHost")!;
        var plainType = compilation.GetTypeByMetadataName("PlainHost")!;
        var supportProbe = new Func<ITypeSymbol, bool>(static _ => false);
        Assert.IsTrue((bool)supported.Invoke(walker, [abstractType, supportProbe])!);
        Assert.IsFalse((bool)supported.Invoke(walker, [plainType, supportProbe])!);

        var defaultExpression = GetPrivateInstance(
            typeof(SemanticWalker),
            "GetDefaultValueTypeExpression",
            typeof(ITypeSymbol),
            typeof(SenseArgument),
            typeof(Func<string, Expression>));
        var fail = new Func<string, Expression>(static message => throw new InvalidOperationException(message));
        Assert.AreEqual("0", ((Expression)defaultExpression.Invoke(
            walker,
            [compilation.GetSpecialType(SpecialType.System_Int32), new SenseArgument(), fail])!).ToKnRECMAScript());

        var sliceLength = GetPrivateInstance(
            typeof(SemanticWalker),
            "BuildListPatternSliceLengthExpression",
            typeof(Expression),
            typeof(int),
            typeof(int));
        var length = new Identifier("length");
        Assert.AreSame(length, sliceLength.Invoke(walker, [length, 0, 0]));
        var sliced = (Expression)sliceLength.Invoke(walker, [new Identifier("length"), 1, 1])!;
        StringAssert.Contains(sliced.ToKnRECMAScript(), "length - 2", StringComparison.Ordinal);

        var operation = block.DescendantsAndSelf().OfType<IIsPatternOperation>().First();
        var visitPattern = typeof(SemanticWalker).GetMethod(
            "VisitIsPattern",
            BindingFlags.Instance | BindingFlags.Public)!;
        Assert.IsNotNull(visitPattern.Invoke(walker, [operation, new SenseArgument()]));
    }

    [TestMethod]
    public void SemanticWalkerHostBoundary_CoversEmptyExpressionStatementAndPropertyMutation()
    {
        var block = CreateBlock(
            """
            namespace ECMAScript
            {
                [global::System.AttributeUsage(global::System.AttributeTargets.Class, Inherited = false)]
                public sealed class ECMAScriptAttribute : global::System.Attribute { }
            }

            [ECMAScript.ECMAScript]
            public sealed class Host
            {
                public int Value { get; set; }
                public void Run() { }
            }

            public sealed class TestClass
            {
                void TestMethod(Host host)
                {
                    host.Value += 1;
                    host.Value++;
                    host.Run();
                }
            }
            """);
        var walker = new SemanticWalker(true)
        {
            Host = new EmptySequenceInvocationHost()
        };
        var node = walker.Visit(block, new SenseArgument());
        Assert.IsNotNull(node);
        _ = node!.ToKnRECMAScript();
    }

    [TestMethod]
    public void SemanticWalkerReferenceBoundaries_CoverModuleIndexerFieldAndRuntimeHostShapes()
    {
        var compilation = CreateCompilation(
            """
            namespace ECMAScript
            {
                [global::System.AttributeUsage(global::System.AttributeTargets.Class | global::System.AttributeTargets.Enum, Inherited = false)]
                public sealed class ECMAScriptAttribute : global::System.Attribute { }

                [global::System.AttributeUsage(global::System.AttributeTargets.Enum, Inherited = false)]
                public sealed class StringAttribute : global::System.Attribute { }
            }

            [ECMAScript.ECMAScript]
            public class ModuleHost
            {
                public const int Constant = 1;
                private int PrivateField;
                public int PublicField;
                public static int StaticValue { get; set; }
                public int Value { get; set; }
                public int this[int index]
                {
                    get => index;
                    set { }
                }

                public static void Static() { }

                public sealed class Child
                {
                    private int ChildField;
                }
            }
            """);
        var module = compilation.GetTypeByMetadataName("ModuleHost")!;
        var child = module.GetTypeMembers("Child").Single();
        var declaredNames = new Dictionary<ISymbol, string>(SymbolEqualityComparer.Default)
        {
            [module] = "ModuleHost",
            [child] = "Child"
        };
        var walker = new SemanticWalker(module, declaredNames);
        var value = module.GetMembers("Value").OfType<IPropertySymbol>().Single();
        var staticValue = module.GetMembers("StaticValue").OfType<IPropertySymbol>().Single();
        var indexer = module.GetMembers().OfType<IPropertySymbol>().Single(static property => property.IsIndexer);
        var constant = module.GetMembers("Constant").OfType<IFieldSymbol>().Single();
        var privateField = module.GetMembers("PrivateField").OfType<IFieldSymbol>().Single();
        var publicField = module.GetMembers("PublicField").OfType<IFieldSymbol>().Single();
        var childField = child.GetMembers("ChildField").OfType<IFieldSymbol>().Single();
        var staticMethod = module.GetMembers("Static").OfType<IMethodSymbol>().Single();

        var currentIndexer = GetPrivateInstance(typeof(SemanticWalker), "IsCurrentModuleRuntimeIndexer", typeof(IPropertySymbol));
        Assert.IsTrue((bool)currentIndexer.Invoke(walker, [indexer])!);
        Assert.IsFalse((bool)currentIndexer.Invoke(walker, [value])!);

        var getter = typeof(SemanticWalker).GetMethods(BindingFlags.Instance | BindingFlags.NonPublic)
            .Single(static method => method.Name == "TryBuildCurrentModuleIndexerGetterCall");
        var getterArgs = new object?[] { indexer, new Identifier("host"), new Expression[] { new NumericLiteral(0, "0") }, null };
        Assert.IsTrue((bool)getter.Invoke(walker, getterArgs)!);
        var getterFalseArgs = new object?[] { value, new Identifier("host"), Array.Empty<Expression>(), null };
        Assert.IsFalse((bool)getter.Invoke(walker, getterFalseArgs)!);

        var setter = typeof(SemanticWalker).GetMethods(BindingFlags.Instance | BindingFlags.NonPublic)
            .Single(static method => method.Name == "TryBuildCurrentModuleIndexerSetterCall");
        var setterArgs = new object?[] { indexer, new Identifier("host"), new Expression[] { new NumericLiteral(0, "0") }, new NumericLiteral(1, "1"), null };
        Assert.IsTrue((bool)setter.Invoke(walker, setterArgs)!);
        var setterFalseArgs = new object?[] { value, new Identifier("host"), Array.Empty<Expression>(), new NumericLiteral(1, "1"), null };
        Assert.IsFalse((bool)setter.Invoke(walker, setterFalseArgs)!);

        var fieldName = GetPrivateInstance(typeof(SemanticWalker), "GetFieldName", typeof(IFieldSymbol));
        Assert.AreEqual("1", ((Expression)fieldName.Invoke(walker, [constant])!).ToKnRECMAScript());
        Assert.AreEqual("PublicField", ((Expression)fieldName.Invoke(walker, [publicField])!).ToKnRECMAScript());

        var privateRuntimeField = GetPrivateInstance(typeof(SemanticWalker), "IsPrivateRuntimeClassField", typeof(IFieldSymbol));
        Assert.IsFalse((bool)privateRuntimeField.Invoke(walker, [privateField])!);
        Assert.IsTrue((bool)privateRuntimeField.Invoke(walker, [childField])!);

        var buildFieldAccess = GetPrivateInstance(
            typeof(SemanticWalker),
            "BuildFieldAccess",
            typeof(Expression),
            typeof(IFieldSymbol),
            typeof(string),
            typeof(bool));
        Assert.IsNotNull(buildFieldAccess.Invoke(walker, [new Identifier("host"), childField, "ChildField", false]));

        var extensionTarget = GetPrivateInstance(
            typeof(SemanticWalker),
            "TryBuildExtensionHostTarget",
            typeof(IMethodSymbol),
            typeof(Nullable<SenseArgument>));
        Assert.IsNotNull(extensionTarget.Invoke(walker, [staticMethod, null]));

        var normalize = GetPrivateInstance(
            typeof(SemanticWalker),
            "NormalizeRuntimeReceiverHostCallee",
            typeof(Expression),
            typeof(IMethodSymbol));
        Assert.IsNotNull(normalize.Invoke(walker, [
            new MemberExpression(new Identifier("ModuleHost"), new Identifier("Static"), computed: false, optional: false),
            staticMethod]));
    }

    [TestMethod]
    public void SemanticWalkerPatternSwitch_CoversPatternAndDefaultCaseBodies()
    {
        var block = CreateBlock(
            """
            public sealed class TestClass
            {
                void TestMethod(int value)
                {
                    switch (value)
                    {
                        case > 0:
                            return;
                        default:
                            return;
                    }
                }
            }
            """);

        var script = new SemanticWalker(true).Visit(block, new SenseArgument())?.ToKnRECMAScript();
        Assert.IsNotNull(script);
        StringAssert.Contains(script, "return", StringComparison.Ordinal);
    }

    [TestMethod]
    public void SemanticWalkerPatternSwitch_TypePatternWithDefaultUsesDefaultBodyPath()
    {
        var block = CreateBlock(
            """
            public sealed class TestClass
            {
                void TestMethod(object value)
                {
                    switch (value)
                    {
                        case int number:
                            return;
                        default:
                            return;
                    }
                }
            }
            """);

        var script = new SemanticWalker(true).Visit(block, new SenseArgument())?.ToKnRECMAScript();
        Assert.IsNotNull(script);
        StringAssert.Contains(script, "return", StringComparison.Ordinal);
    }

    [TestMethod]
    public void SemanticWalkerPatternSwitch_CoversBreakAndRejectsContinueAcrossIife()
    {
        var breakBlock = CreateBlock(
            """
            public sealed class TestClass
            {
                void TestMethod(object value)
                {
                    switch (value)
                    {
                        case int number:
                            break;
                        default:
                            break;
                    }
                }
            }
            """);
        var script = new SemanticWalker(true).Visit(breakBlock, new SenseArgument())?.ToKnRECMAScript();
        Assert.IsNotNull(script);

        var continueBlock = CreateBlock(
            """
            public sealed class TestClass
            {
                void TestMethod(object value)
                {
                    while (true)
                    {
                        switch (value)
                        {
                            case int number:
                                continue;
                            default:
                                break;
                        }
                    }
                }
            }
            """);
        var exception = Assert.Throws<OperationTransformationException>(() =>
            new SemanticWalker(true).Visit(continueBlock, new SenseArgument()));
        StringAssert.Contains(exception.Message, "Continue statements inside pattern-matching switch", StringComparison.Ordinal);
    }

    [TestMethod]
    public void SemanticWalkerPatternHelpers_CoverReportCallbackAndTupleSourceKinds()
    {
        var block = CreateBlock(
            """
            public sealed class TestClass
            {
                private (int First, int Second) field;

                void TestMethod((int First, int Second) parameter)
                {
                    var local = parameter;
                    var tuple = (1, 2);
                    var fromField = field;
                    var fromThis = this.field;
                    var check = parameter is { };
                }
            }
            """);
        var recursive = block.DescendantsAndSelf().OfType<IRecursivePatternOperation>().FirstOrDefault();
        if (recursive is not null)
        {
            var reported = false;
            var patternReference = GetPrivateInstance(
                typeof(SemanticWalker),
                "GetPatternRefrence",
                typeof(IOperation),
                typeof(SenseArgument));
            var reportingWalker = new SemanticWalker((_, _) => reported = true);
            _ = Assert.Throws<TargetInvocationException>(() =>
                patternReference.Invoke(reportingWalker, [recursive, new SenseArgument()]));
            Assert.IsTrue(reported);
        }

        var cache = GetPrivateStatic(typeof(SemanticWalker), "ShouldCacheTupleSource", typeof(IOperation));
        foreach (var operation in block.DescendantsAndSelf().OfType<IOperation>())
        {
            if (operation is ILocalReferenceOperation or IParameterReferenceOperation or
                IFieldReferenceOperation or IInstanceReferenceOperation or ITupleOperation)
                _ = cache.Invoke(null, [operation]);
        }
    }

    [TestMethod]
    public void SemanticWalkerPatternBoundaries_CoverInterfaceFoldingAndSourceResolution()
    {
        var compilation = CreateCompilation(
            """
            using System;
            public interface IMarker { }
            public sealed class Impl : IMarker { }
            public sealed class PatternHost
            {
                void Test(object input)
                {
                    IMarker direct = new Impl();
                    var exact = direct is IMarker;
                    var cast = input as string;
                    int? nullable = 1;
                    var nullableCheck = nullable is IComparable;
                    var recursive = input is { };
                    IMarker reassigned = new Impl();
                    reassigned = input as IMarker;
                    var reassignedCheck = reassigned is IMarker;
                    switch (input)
                    {
                        case IMarker marker:
                            break;
                        default:
                            break;
                    }
                }
            }
            public sealed class GenericPattern<T, U>
                where U : IMarker
                where T : U
            {
                public bool Test(T value) => value is IMarker;
            }
            """);
        var tree = compilation.SyntaxTrees.Single();
        var model = compilation.GetSemanticModel(tree);
        var method = tree.GetRoot().DescendantNodes().OfType<MethodDeclarationSyntax>()
            .Single(static candidate => candidate.Identifier.ValueText == "Test" && candidate.Parent is ClassDeclarationSyntax type && type.Identifier.ValueText == "PatternHost");
        var body = Assert.IsInstanceOfType<IBlockOperation>(model.GetOperation(method.Body!));
        var walker = new SemanticWalker(true);
        var marker = compilation.GetTypeByMetadataName("IMarker")!;
        var impl = compilation.GetTypeByMetadataName("Impl")!;
        var nullableInt = compilation.GetSpecialType(SpecialType.System_Int32);
        var nullable = compilation.GetTypeByMetadataName("System.Nullable`1")!.Construct(nullableInt);

        var assignable = GetPrivateStatic(
            typeof(SemanticWalker),
            "IsRuntimeTypeAssignableToInterface",
            typeof(ITypeSymbol),
            typeof(ITypeSymbol));
        Assert.IsTrue((bool)assignable.Invoke(null, [marker, marker])!);
        Assert.IsTrue((bool)assignable.Invoke(null, [impl, marker])!);
        Assert.IsFalse((bool)assignable.Invoke(null, [impl, compilation.GetTypeByMetadataName("System.IDisposable")!])!);
        Assert.IsFalse((bool)assignable.Invoke(null, [compilation.GetSpecialType(SpecialType.System_String), marker])!);
        Assert.IsFalse((bool)assignable.Invoke(null, [nullable, marker])!);

        var resolveSource = GetPrivateStatic(typeof(SemanticWalker), "ResolveIsTypeSourceOperation", typeof(IOperation));
        var isTypes = body.Descendants().OfType<IIsTypeOperation>().ToArray();
        var isType = isTypes.Single(operation => operation.Syntax.ToString().Contains("direct is", StringComparison.Ordinal));
        var nullableIsType = isTypes.Single(operation => operation.Syntax.ToString().Contains("nullable is", StringComparison.Ordinal));
        var isPattern = body.Descendants().OfType<IIsPatternOperation>().First();
        var tryCast = body.Descendants().OfType<IConversionOperation>().FirstOrDefault(operation => operation.IsTryCast);
        Assert.AreSame(isType.ValueOperand, resolveSource.Invoke(null, [isType]));
        if (tryCast is not null)
            Assert.AreSame(tryCast.Operand, resolveSource.Invoke(null, [tryCast]));
        Assert.AreSame(isPattern.Value, resolveSource.Invoke(null, [isPattern.Pattern])) ;

        var resolveAssignment = GetPrivateInstance(
            typeof(SemanticWalker),
            "ResolveSingleAssignmentValueSource",
            typeof(IOperation),
            typeof(IOperation));
        var directReference = body.Descendants().OfType<ILocalReferenceOperation>()
            .First(reference => reference.Local.Name == "direct");
        Assert.IsNotNull(resolveAssignment.Invoke(walker, [directReference, isType]));
        var reassignedReference = body.Descendants().OfType<ILocalReferenceOperation>()
            .First(reference => reference.Local.Name == "reassigned");
        Assert.IsNotNull(resolveAssignment.Invoke(walker, [reassignedReference, isType]));

        var evaluate = typeof(SemanticWalker).GetMethod(
            "TryEvaluateCompileTimeErasedInterfaceIsTypeCheck",
            BindingFlags.Instance | BindingFlags.NonPublic)!;
        var exactArgs = new object?[] { isType, marker, null };
        Assert.IsTrue((bool)evaluate.Invoke(walker, exactArgs)!);
        var nullableArgs = new object?[] { nullableIsType, compilation.GetTypeByMetadataName("System.IComparable")!, null };
        Assert.IsTrue((bool)evaluate.Invoke(walker, nullableArgs)!);

        var genericType = compilation.GetTypeByMetadataName("GenericPattern`2")!;
        var genericParameter = genericType.TypeParameters[0];
        Assert.IsTrue((bool)assignable.Invoke(null, [genericParameter, marker])!);
    }

    [TestMethod]
    public void SemanticWalkerOperatorBoundaries_CoverCustomBooleanOperatorAndExternalPrimaryParameter()
    {
        var block = CreateBlock(
            """
            namespace ECMAScript
            {
                [global::System.AttributeUsage(global::System.AttributeTargets.Class, Inherited = false)]
                public sealed class ECMAScriptAttribute : global::System.Attribute { }
            }

            [ECMAScript.ECMAScript]
            public sealed class Flag
            {
                public static bool operator true(Flag value) => true;
                public static bool operator false(Flag value) => false;
            }

            public sealed class TestClass
            {
                void TestMethod(Flag value)
                {
                    if (value)
                        return;
                }
            }
            """);
        var script = new SemanticWalker(true).Visit(block, new SenseArgument())?.ToKnRECMAScript();
        Assert.IsNotNull(script);
        StringAssert.Contains(script, "!", StringComparison.Ordinal);

        var metadataCompilation = CreateCompilation("public sealed class Holder { }");
        var externalParameter = metadataCompilation.GetTypeByMetadataName("System.String")!
            .GetMembers("Concat")
            .OfType<IMethodSymbol>()
            .Single(static method => method.Parameters.Length == 2 &&
                method.Parameters.All(static parameter => parameter.Type.SpecialType == SpecialType.System_String))
            .Parameters[0];
        var walker = new SemanticWalker(true);
        var buildDefault = GetPrivateInstance(
            typeof(SemanticWalker),
            "BuildImplicitPrimaryConstructorParameterDefaultValue",
            typeof(IParameterSymbol),
            typeof(SenseArgument));
        var missingSource = Assert.Throws<TargetInvocationException>(() =>
            buildDefault.Invoke(walker, [externalParameter, new SenseArgument()]));
        Assert.IsInstanceOfType<InvalidOperationException>(missingSource.InnerException);
    }

    [TestMethod]
    public void SemanticWalkerMutationBoundaries_CoverNullOperandIndexTypesAndConditionalAccess()
    {
        var index = GetPrivateStatic(typeof(SemanticWalker), "IsSystemIndexType", typeof(ITypeSymbol));
        var range = GetPrivateStatic(typeof(SemanticWalker), "IsSystemRangeType", typeof(ITypeSymbol));
        Assert.IsFalse((bool)index.Invoke(null, [null])!);
        Assert.IsFalse((bool)range.Invoke(null, [null])!);

        var materialize = GetPrivateInstance(
            typeof(SemanticWalker),
            "MaterializePropertyMutationOperand",
            typeof(Expression),
            typeof(IOperation),
            typeof(SenseArgument),
            typeof(List<Expression>),
            typeof(string));
        var block = CreateBlock(
            """
            public sealed class TestClass
            {
                void TestMethod(string? value)
                {
                    var read = value?.Length;
                }
            }
            """);
        var operation = block.DescendantsAndSelf().First();
        Assert.IsNull(materialize.Invoke(new SemanticWalker(true), [null, operation, new SenseArgument(), new List<Expression>(), "null"]));

        var duplicateTarget = GetPrivateStatic(typeof(SemanticWalker), "CanDuplicateReadWriteTarget", typeof(Expression));
        Assert.IsTrue((bool)duplicateTarget.Invoke(null, [new Identifier("value")])!);
        Assert.IsTrue((bool)duplicateTarget.Invoke(null, [
            new MemberExpression(new Identifier("value"), new Identifier("Property"), computed: false, optional: false)])!);
        Assert.IsFalse((bool)duplicateTarget.Invoke(null, [
            new CallExpression(new Identifier("GetValue"), NodeList.Empty<Expression>(), optional: false)])!);

        var conditionalProperty = block.DescendantsAndSelf()
            .OfType<IPropertyReferenceOperation>()
            .Single(static property => property.Property.Name == "Length");
        var guard = GetPrivateInstance(
            typeof(SemanticWalker),
            "RequiresConditionalAccessNullishGuard",
            typeof(IPropertyReferenceOperation));
        _ = guard.Invoke(new SemanticWalker(true), [conditionalProperty]);
    }

    [TestMethod]
    public void SemanticWalkerReferenceBoundaries_CoverStringEnumMappingsAndRuntimeHostFallbacks()
    {
        var compilation = CreateCompilation(
            """
            using System.ComponentModel;
            namespace ECMAScript
            {
                [global::System.AttributeUsage(global::System.AttributeTargets.Class | global::System.AttributeTargets.Enum, Inherited = false)]
                public sealed class ECMAScriptAttribute : global::System.Attribute { }

                [global::System.AttributeUsage(global::System.AttributeTargets.Enum, Inherited = false)]
                public sealed class StringAttribute : global::System.Attribute { }

                [global::System.AttributeUsage(global::System.AttributeTargets.Field | global::System.AttributeTargets.Method | global::System.AttributeTargets.Property)]
                public sealed class ECMAScriptNameAttribute : global::System.Attribute
                {
                    public ECMAScriptNameAttribute(string name) { }
                }
            }

            [ECMAScript.String]
            public enum StringState
            {
                [ECMAScript.ECMAScriptName("ready-value")]
                Ready = 1,
                [Description("@#busy-value")]
                Busy = 2,
                [Description("ordinary-description")]
                Ordinary = 3,
                Fallback = 4
            }

            public enum PlainState
            {
                Value = 1
            }

            [ECMAScript.ECMAScript]
            public static class RuntimeHost
            {
                public static int Value { get; set; }
                public static int Field = 1;
                public static int Read(int value) => value;
            }
            """);

        var stringEnum = compilation.GetTypeByMetadataName("StringState")!;
        var plainEnum = compilation.GetTypeByMetadataName("PlainState")!;
        var host = compilation.GetTypeByMetadataName("RuntimeHost")!;
        var staticValue = host.GetMembers("Value").OfType<IPropertySymbol>().Single();
        var runtimeField = host.GetMembers("Field").OfType<IFieldSymbol>().Single();
        var literal = GetPrivateStatic(
            typeof(SemanticWalker),
            "TryBuildStringEnumLiteral",
            typeof(IFieldSymbol),
            typeof(Expression).MakeByRefType());
        foreach (var field in stringEnum.GetMembers().OfType<IFieldSymbol>())
        {
            var arguments = new object?[] { field, null };
            Assert.IsTrue((bool)literal.Invoke(null, arguments)!);
            Assert.IsInstanceOfType<Literal>(arguments[1]);
        }

        var plainField = plainEnum.GetMembers("Value").OfType<IFieldSymbol>().Single();
        Assert.IsFalse((bool)literal.Invoke(null, new object?[] { plainField, null })!);
        Assert.IsFalse((bool)literal.Invoke(null, new object?[] { runtimeField, null })!);

        var valueLiteral = GetPrivateStatic(
            typeof(SemanticWalker),
            "TryBuildStringEnumValueLiteral",
            typeof(INamedTypeSymbol),
            typeof(object),
            typeof(Expression).MakeByRefType());
        var mapped = new object?[] { stringEnum, 2, null };
        Assert.IsTrue((bool)valueLiteral.Invoke(null, mapped)!);
        var unmatched = new object?[] { stringEnum, 99, null };
        Assert.IsFalse((bool)valueLiteral.Invoke(null, unmatched)!);
        var zeroValue = new object?[] { stringEnum, 0, null };
        Assert.IsFalse((bool)valueLiteral.Invoke(null, zeroValue)!);
        var plain = new object?[] { plainEnum, 1, null };
        Assert.IsFalse((bool)valueLiteral.Invoke(null, plain)!);

        var method = host.GetMembers("Read").OfType<IMethodSymbol>().Single();
        var runtimeHost = GetPrivateInstance(
            typeof(SemanticWalker),
            "TryBuildRuntimeHostExpression",
            typeof(ITypeSymbol),
            typeof(Nullable<SenseArgument>));
        Assert.IsNotNull(runtimeHost.Invoke(new SemanticWalker(true), [host, null]));
        Assert.IsNotNull(runtimeHost.Invoke(new SemanticWalker(true), [plainEnum, null]));

        var extensionTarget = GetPrivateInstance(
            typeof(SemanticWalker),
            "TryBuildExtensionHostTarget",
            typeof(IMethodSymbol),
            typeof(Nullable<SenseArgument>));
        Assert.IsNotNull(extensionTarget.Invoke(new SemanticWalker(true), [method, null]));

        var preferredRuntimeStatic = GetPrivateInstance(
            typeof(SemanticWalker),
            "TryBuildPreferredRuntimeStaticMemberAccess",
            typeof(ISymbol),
            typeof(SyntaxNode),
            typeof(SemanticModel),
            typeof(string),
            typeof(Nullable<SenseArgument>),
            typeof(Expression).MakeByRefType());
        var preferredArguments = new object?[]
        {
            staticValue,
            staticValue.DeclaringSyntaxReferences.Single().GetSyntax(),
            compilation.GetSemanticModel(staticValue.DeclaringSyntaxReferences.Single().SyntaxTree),
            "Value",
            new SenseArgument(),
            null
        };
        _ = preferredRuntimeStatic.Invoke(new SemanticWalker(true), preferredArguments);
    }

    [TestMethod]
    public void SemanticWalkerReferenceBoundaries_CoverMethodGroupsAndNamedRuntimeArguments()
    {
        var block = CreateBlock(
            """
            namespace ECMAScript
            {
                [global::System.AttributeUsage(global::System.AttributeTargets.Class, Inherited = false)]
                public sealed class ECMAScriptAttribute : global::System.Attribute { }
            }

            [ECMAScript.ECMAScript]
            public static class RuntimeHost
            {
                public static int Optional(int first = 1, int second = 2) => first + second;
            }

            public sealed class TestClass
            {
                private int Instance(int value) => value;
                private static int Static(int value) => value;

                void TestMethod(int value)
                {
                    System.Func<int, int> instanceGroup = Instance;
                    System.Func<int, int> staticGroup = Static;
                    var first = instanceGroup(value);
                    var second = staticGroup(value);
                    var third = RuntimeHost.Optional(second: second);
                }
            }
            """);

        var host = new SemanticWalker(true)
        {
            Host = new ArgumentRewriteHost()
        };
        var script = host.Visit(block, new SenseArgument())?.ToKnRECMAScript();
        Assert.IsNotNull(script);
        StringAssert.Contains(script, "Optional", StringComparison.Ordinal);
    }

    [TestMethod]
    public void SemanticWalkerReferenceBoundaries_CoverRuntimeTokenAndTaggedUnionProjection()
    {
        var compilation = CreateCompilation(
            """
            using ECMAScript;

            namespace ECMAScript
            {
                [global::System.AttributeUsage(global::System.AttributeTargets.Class | global::System.AttributeTargets.Struct)]
                public sealed class ECMAScriptAttribute : global::System.Attribute { }
            }

            namespace System.Runtime.CompilerServices
            {
                [global::System.AttributeUsage(global::System.AttributeTargets.Struct)]
                public sealed class UnionAttribute : global::System.Attribute { }

                public interface IUnion
                {
                    object? Value { get; }
                }
            }

            [ECMAScript]
            [System.Runtime.CompilerServices.Union]
            public readonly struct Choice : System.Runtime.CompilerServices.IUnion
            {
                public Choice(string value) { }
                public object? Value => null;
                public string? AsText => null;
                public static implicit operator Choice(string value) => default;
            }

            public sealed class TestClass
            {
                void TestMethod()
                {
                    var value = 1;
                }
            }
            """);
        var block = CreateBlockFromCompilation(compilation, "TestClass", "TestMethod");
        var operation = block.Operations[0];
        var walker = new SemanticWalker(true);

        var runtimeToken = GetPrivateInstance(
            typeof(SemanticWalker),
            "BuildRuntimeTypeTokenExpression",
            typeof(IOperation),
            typeof(ITypeSymbol),
            typeof(SenseArgument));
        Assert.IsNotNull(runtimeToken.Invoke(
            walker,
            [operation, compilation.GetSpecialType(SpecialType.System_Void), new SenseArgument()]));

        var choice = compilation.GetTypeByMetadataName("Choice")!;
        var projection = choice.GetMembers("AsText").OfType<IPropertySymbol>().Single();
        var value = choice.GetMembers("Value").OfType<IPropertySymbol>().Single();
        var isProjection = GetPrivateStatic(
            typeof(SemanticWalker),
            "IsErasedUnionProjectionProperty",
            typeof(IPropertySymbol));
        Assert.IsTrue((bool)isProjection.Invoke(null, [projection])!);
        Assert.IsTrue((bool)isProjection.Invoke(null, [value])!);
    }

    [TestMethod]
    public void AstConverterBoundaries_CoverConstructorAndNameCollisionBranches()
    {
        var compilation = CreateCompilation(
            """
            public static class ModuleHost
            {
                public class Base
                {
                    public Base() { }
                    public Base(int value) { }
                }

                public sealed class Derived : Base
                {
                    public Derived() : base() { }
                    public Derived(int value) : base(value) { }
                }

                public sealed class Primary(int seed) : Base()
                {
                    public int Field = seed;
                    public int Property { get; } = seed;
                }

                public sealed class Accessors
                {
                    public int Auto { get; set; }
                    public int ReadOnly => 1;
                    public int WriteOnly { set { Auto = value; } }
                    public Accessors() { }
                    public Accessors(int value) => Auto = value;
                }

                public sealed class ExpressionConstructor
                {
                    public int Value;
                    public ExpressionConstructor(int value) => Value = value;
                }
            }
            """);
        var tree = compilation.SyntaxTrees.Single();
        var model = compilation.GetSemanticModel(tree);
        var module = compilation.GetTypeByMetadataName("ModuleHost")!;
        var converter = new AstConverter(module, model);
        var derived = module.GetTypeMembers("Derived").Single();
        var baseType = module.GetTypeMembers("Base").Single();
        var primary = module.GetTypeMembers("Primary").Single();
        var accessors = module.GetTypeMembers("Accessors").Single();
        var expressionConstructor = module.GetTypeMembers("ExpressionConstructor").Single();

        var supportedBase = GetPrivateInstance(typeof(AstConverter), "GetSupportedMemberBaseType", typeof(INamedTypeSymbol));
        Assert.AreEqual("Base", ((INamedTypeSymbol)supportedBase.Invoke(converter, [derived])!).Name);
        var runtimeBase = GetPrivateInstance(typeof(AstConverter), "GetSupportedRuntimeClassBaseType", typeof(INamedTypeSymbol));
        Assert.IsNull(runtimeBase.Invoke(converter, [module]));

        var initializers = GetPrivateInstance(typeof(AstConverter), "GetPrimaryConstructorInitializers", typeof(INamedTypeSymbol));
        Assert.IsNotEmpty((System.Collections.IEnumerable)initializers.Invoke(converter, [primary])!);
        Assert.IsEmpty((System.Collections.IEnumerable)initializers.Invoke(converter, [derived])!);

        // Exercise all constructor cardinalities and base-class naming combinations used by
        // the runtime-class emitter (zero, one, and multiple explicit constructors).
        _ = converter.ConvertRuntimeClass(baseType).ToKnRECMAScript();
        _ = converter.ConvertRuntimeClass(derived).ToKnRECMAScript();
        _ = converter.ConvertRuntimeClass(primary).ToKnRECMAScript();
        _ = converter.ConvertRuntimeClass(accessors).ToKnRECMAScript();
        _ = converter.ConvertRuntimeClass(expressionConstructor).ToKnRECMAScript();

        var getBaseInvocation = GetPrivateInstance(
            typeof(AstConverter),
            "GetPrimaryConstructorBaseInvocation",
            typeof(ClassDeclarationSyntax),
            typeof(INamedTypeSymbol));
        var primarySyntax = tree.GetRoot().DescendantNodes().OfType<ClassDeclarationSyntax>()
            .Single(static declaration => declaration.Identifier.ValueText == "Primary");
        var baseFailure = Assert.Throws<TargetInvocationException>(() =>
            getBaseInvocation.Invoke(converter, [primarySyntax, null]));
        Assert.IsInstanceOfType<NotSupportedException>(baseFailure.InnerException);
        Assert.IsNotNull(getBaseInvocation.Invoke(converter, [primarySyntax, module]));

        var initializerArguments = GetPrivateInstance(
            typeof(AstConverter),
            "CreateConstructorInitializerArguments",
            typeof(ImmutableArray<ArgumentSyntax>),
            typeof(IMethodSymbol),
            typeof(CancellationToken));
        var derivedConstructor = derived.InstanceConstructors
            .Single(static ctor => !ctor.IsImplicitlyDeclared && ctor.Parameters.Length == 1);
        var derivedSyntax = tree.GetRoot().DescendantNodes().OfType<ConstructorDeclarationSyntax>()
            .Single(static declaration => declaration.Parent is ClassDeclarationSyntax type && type.Identifier.ValueText == "Derived" && declaration.ParameterList.Parameters.Count == 1);
        var baseInitializer = derivedSyntax.Initializer!;
        var baseModel = compilation.GetSemanticModel(tree);
        var baseConstructor = baseModel.GetSymbolInfo(baseInitializer).Symbol as IMethodSymbol;
        var arguments = baseInitializer.ArgumentList.Arguments.ToImmutableArray();
        Assert.IsNotNull(initializerArguments.Invoke(converter, [arguments, baseConstructor, CancellationToken.None]));

        var createLiteral = GetPrivateStatic(typeof(AstConverter), "CreateLiteralExpression", typeof(object));
        Assert.AreEqual("null", ((Expression)createLiteral.Invoke(null, [null])!).ToKnRECMAScript());
        Assert.AreEqual("true", ((Expression)createLiteral.Invoke(null, [true])!).ToKnRECMAScript());
        Assert.AreEqual("7", ((Expression)createLiteral.Invoke(null, [7])!).ToKnRECMAScript());
    }

    [TestMethod]
    public void AstConverterRuntimeClassBoundaries_CoverProxyStorageOverloadsAndPrimaryInitializers()
    {
        var compilation = CreateCompilation(
            """
            public static class ModuleHost
            {
                public class Base
                {
                    public Base() { }
                    public Base(int value) { }
                }

                public sealed class Derived : Base
                {
                    private int field;
                    public int Value { get; set; }
                    public Derived() : base() { }
                    public Derived(int value) : base(value) { }
                }

                public sealed class Primary(int seed)
                {
                    public int Field = seed;
                    public int Property { get; } = seed;
                }
            }
            """);
        var tree = compilation.SyntaxTrees.Single();
        var model = compilation.GetSemanticModel(tree);
        var module = compilation.GetTypeByMetadataName("ModuleHost")!;
        var derived = module.GetTypeMembers("Derived").Single();
        var primary = module.GetTypeMembers("Primary").Single();
        var converter = new AstConverter(module, model);

        var storage = GetPrivateInstance(
            typeof(AstConverter),
            "CreatePrivateStorageKey",
            typeof(IFieldSymbol),
            typeof(string));
        var field = derived.GetMembers("field").OfType<IFieldSymbol>().Single();
        Assert.IsInstanceOfType<PrivateIdentifier>(storage.Invoke(converter, [field, "field"]));
        var proxyConverter = new AstConverter(
            module,
            model,
            new AstConverterOptions(
                AstConverterProfile.Standard,
                RuntimeClassPrivateStorage: RuntimeClassPrivateStorage.ProxySafeMangledProperties));
        Assert.IsInstanceOfType<Identifier>(storage.Invoke(proxyConverter, [null, "synthetic"]));
        Assert.IsInstanceOfType<Identifier>(storage.Invoke(proxyConverter, [field, "field"]));

        var localNamesProperty = typeof(AstConverter).GetProperty(
            "ModuleLocalNames",
            BindingFlags.Instance | BindingFlags.NonPublic)!;
        Assert.IsNotNull(localNamesProperty.GetValue(converter));

        var declaration = converter.ConvertRuntimeClass(derived);
        var script = declaration.ToKnRECMAScript();
        StringAssert.Contains(script, "$ctor_", StringComparison.Ordinal);

        var getInitializers = GetPrivateInstance(
            typeof(AstConverter),
            "GetPrimaryConstructorInitializers",
            typeof(INamedTypeSymbol));
        var initializers = (System.Collections.IEnumerable)getInitializers.Invoke(converter, [primary])!;
        Assert.IsNotEmpty(initializers);
    }

    [TestMethod]
    public void CompilerNullBoundaryHelpers_CoverOptionalSymbolAndResourceContracts()
    {
        var half = GetPrivateStatic(typeof(SemanticWalker), "IsSystemHalfType", typeof(ITypeSymbol));
        var tupleLike = GetPrivateStatic(typeof(SemanticWalker), "IsTupleLikeHost", typeof(ITypeSymbol));
        Assert.IsFalse((bool)half.Invoke(null, [null])!);
        var tupleProbeCompilation = CreateCompilation("public sealed class Probe { }");
        Assert.IsFalse((bool)tupleLike.Invoke(null, [tupleProbeCompilation.GetTypeByMetadataName("Probe")!])!);

        var disposeByInterface = GetPrivateStatic(
            typeof(SemanticWalker),
            "TryResolveUsingDisposeMethodByInterface",
            typeof(ITypeSymbol),
            typeof(string),
            typeof(string),
            typeof(IMethodSymbol).MakeByRefType());
        var disposeArguments = new object?[] { null, "System.IDisposable", "Dispose", null };
        Assert.IsFalse((bool)disposeByInterface.Invoke(null, disposeArguments)!);

        var runtimeMarker = GetPrivateStatic(typeof(Util), "IsRuntimeMarkerType", typeof(ISymbol));
        var moduleMarker = GetPrivateStatic(typeof(Util), "IsECMAScriptModuleType", typeof(ITypeSymbol));
        Assert.IsFalse((bool)runtimeMarker.Invoke(null, [null])!);
        Assert.IsFalse((bool)moduleMarker.Invoke(null, [null])!);

        var compilation = CreateCompilation(
            """
            using System;
            using System.ComponentModel;

            [Description("@#configured")]
            public sealed class NamedHost
            {
                public (int First, int Second) Pair;
                public static void Overload(int value) { }
                public static void Overload(string value) { }
            }
            """);
        var host = compilation.GetTypeByMetadataName("NamedHost")!;
        var pair = host.GetMembers("Pair").OfType<IFieldSymbol>().Single();
        var tupleField = ((INamedTypeSymbol)pair.Type).GetMembers("First").OfType<IFieldSymbol>().Single();
        Assert.AreEqual("First", Util.GetConfigOrSymbolName(tupleField));

        var importMapping = GetPrivateStatic(
            typeof(Util),
            "TryGetJazorImportMapping",
            typeof(ISymbol),
            typeof(string).MakeByRefType(),
            typeof(string).MakeByRefType());
        var importArgs = new object?[] { host, null, null };
        Assert.IsFalse((bool)importMapping.Invoke(null, importArgs)!);

        var structural = GetPrivateInstance(typeof(SemanticWalker), "ShouldLowerStructurally", typeof(ITypeSymbol));
        Assert.IsFalse((bool)structural.Invoke(new SemanticWalker(true), [null])!);
        var initializerSymbol = GetPrivateStatic(typeof(SemanticWalker), "GetObjectInitializerMemberSymbol", typeof(IOperation));
        Assert.IsNull(initializerSymbol.Invoke(null, [null]));

        var collectionTarget = GetPrivateStatic(typeof(SemanticWalker), "GetCollectionElementTargetType", typeof(ITypeSymbol));
        var probe = CreateCompilation(
            """
            using System.Collections.Generic;
            public sealed class Probe
            {
                public int[] Array = [];
                public List<int> List = [];
            }
            """);
        var probeType = probe.GetTypeByMetadataName("Probe")!;
        var arrayType = probeType.GetMembers("Array").OfType<IFieldSymbol>().Single().Type;
        var listType = probeType.GetMembers("List").OfType<IFieldSymbol>().Single().Type;
        Assert.AreEqual("int", ((ITypeSymbol)collectionTarget.Invoke(null, [arrayType])!).ToDisplayString());
        Assert.AreEqual("int", ((ITypeSymbol)collectionTarget.Invoke(null, [listType])!).ToDisplayString());
        Assert.IsNull(collectionTarget.Invoke(null, [probe.GetSpecialType(SpecialType.System_Int32)]));

        var tupleCache = GetPrivateStatic(typeof(SemanticWalker), "ShouldCacheTupleSource", typeof(IOperation));
        var probeTree = probe.SyntaxTrees.Single();
        var probeModel = probe.GetSemanticModel(probeTree);
        var arrayInitializer = probeTree.GetRoot().DescendantNodes()
            .OfType<VariableDeclaratorSyntax>()
            .Single(static declarator => declarator.Identifier.ValueText == "Array")
            .Initializer!;
        var arrayOperation = probeModel.GetOperation(arrayInitializer.Value)!;
        var localBlock = CreateBlock(
            """
            public sealed class TestClass
            {
                void TestMethod(int value)
                {
                    var local = value;
                    var tuple = (value, value);
                    var copy = local;
                }
            }
            """);
        Assert.IsFalse((bool)tupleCache.Invoke(null, [localBlock.DescendantsAndSelf().OfType<ILocalReferenceOperation>().First()])!);
        Assert.IsFalse((bool)tupleCache.Invoke(null, [localBlock.DescendantsAndSelf().OfType<ITupleOperation>().First()])!);
        Assert.IsTrue((bool)tupleCache.Invoke(null, [arrayOperation])!);
    }

    [TestMethod]
    public void SemanticWalkerConstructionAndSupportBoundaries_CoverAlternateScopesAndTupleHosts()
    {
        var compilation = CreateCompilation(
            """
            public sealed class Host
            {
                public void TestMethod()
                {
                    var value = 1;
                }
            }
            """);
        var host = compilation.GetTypeByMetadataName("Host")!;
        _ = new SemanticWalker(host);
        _ = new SemanticWalker(host, CancellationToken.None);
        _ = new SemanticWalker((_, _) => { });
        _ = new SemanticWalker((_, _) => { }, CancellationToken.None);

        var tupleLike = GetPrivateStatic(typeof(SemanticWalker), "IsTupleLikeHost", typeof(ITypeSymbol));
        Assert.IsTrue((bool)tupleLike.Invoke(null, [compilation.GetTypeByMetadataName("System.ValueTuple`2")!])!);

        var operation = CreateBlockFromCompilation(compilation, "Host", "TestMethod");
        var ensureScope = GetPrivateInstance(
            typeof(SemanticWalker),
            "EnsureScopeContext",
            typeof(IOperation),
            typeof(SenseArgument),
            typeof(Nullable<ScopeSite>));
        var walker = new SemanticWalker(true);
        Assert.IsNotNull(ensureScope.Invoke(walker, [operation, new SenseArgument(), null]));

        var recursionField = typeof(SemanticWalker).GetField("_recursionDepth", BindingFlags.Instance | BindingFlags.NonPublic)!;
        recursionField.SetValue(walker, 2);
        var scopeFailure = Assert.Throws<TargetInvocationException>(() =>
            ensureScope.Invoke(walker, [operation, new SenseArgument(), null]));
        Assert.IsInstanceOfType<InvalidOperationException>(scopeFailure.InnerException);

        var createSyntax = GetPrivateStatic(
            typeof(SemanticWalker),
            "CreateSyntaxNodeTransformationException",
            typeof(SyntaxNode),
            typeof(string));
        var syntax = compilation.SyntaxTrees.Single().GetRoot();
        Assert.IsNotNull(createSyntax.Invoke(null, [syntax, "boundary"]));

        var isSupported = GetPrivateInstance(
            typeof(SemanticWalker),
            "IsSupportedExternalType",
            typeof(IOperation),
            typeof(ITypeSymbol));
        Assert.IsTrue((bool)isSupported.Invoke(new SemanticWalker(true), [operation, host])!);
    }

    [TestMethod]
    public void SemanticWalkerCreationHelpers_CoverKeyKindsAndReceiverCaching()
    {
        var compilation = CreateCompilation(
            """
            using System;
            namespace ECMAScript
            {
                public sealed class Symbol { }
                public sealed class Number { }
            }
            namespace Other
            {
                public sealed class Index { }
                public sealed class Range { }
            }
            public sealed class Holder
            {
                public int Value { get; set; }
                public Holder Child { get; } = new();
                public static Holder Create() => new();
                public void TestMethod()
                {
                    var value = new Holder { Value = 1 };
                    var nested = new Holder { Child = { Value = 2 } };
                    _ = value;
                    _ = nested;
                }
            }
            """);
        var holder = compilation.GetTypeByMetadataName("Holder")!;
        var computedKey = GetPrivateStatic(typeof(SemanticWalker), "IsObjectLiteralComputedKeyType", typeof(ITypeSymbol));
        var numericKey = GetPrivateStatic(typeof(SemanticWalker), "IsObjectLiteralNumericKeyType", typeof(ITypeSymbol));
        var symbolType = compilation.GetTypeByMetadataName("System.Object")!;
        Assert.IsFalse((bool)computedKey.Invoke(null, [null])!);
        Assert.IsFalse((bool)computedKey.Invoke(null, [symbolType])!);
        Assert.IsFalse((bool)numericKey.Invoke(null, [null])!);
        Assert.IsFalse((bool)numericKey.Invoke(null, [symbolType])!);
        Assert.IsTrue((bool)computedKey.Invoke(null, [compilation.GetTypeByMetadataName("ECMAScript.Symbol")!])!);
        Assert.IsTrue((bool)numericKey.Invoke(null, [compilation.GetTypeByMetadataName("ECMAScript.Number")!])!);

        var indexType = compilation.GetTypeByMetadataName("System.Index")!;
        var rangeType = compilation.GetTypeByMetadataName("System.Range")!;
        var isIndex = GetPrivateStatic(typeof(SemanticWalker), "IsSystemIndexType", typeof(ITypeSymbol));
        var isRange = GetPrivateStatic(typeof(SemanticWalker), "IsSystemRangeType", typeof(ITypeSymbol));
        Assert.IsTrue((bool)isIndex.Invoke(null, [indexType])!);
        Assert.IsTrue((bool)isRange.Invoke(null, [rangeType])!);
        Assert.IsFalse((bool)isIndex.Invoke(null, [compilation.GetTypeByMetadataName("Other.Index")!])!);
        Assert.IsFalse((bool)isRange.Invoke(null, [compilation.GetTypeByMetadataName("Other.Range")!])!);

        var block = CreateBlockFromCompilation(compilation, "Holder", "TestMethod");
        var nestedInitializer = block.DescendantsAndSelf().OfType<IMemberInitializerOperation>().Single();
        var receiver = GetPrivateInstance(
            typeof(SemanticWalker),
            "BuildMemberInitializerReceiver",
            typeof(IMemberInitializerOperation),
            typeof(Expression),
            typeof(SenseArgument));
        var walker = new SemanticWalker(true);
        Assert.IsNotNull(receiver.Invoke(walker, [nestedInitializer, new Identifier("owner"), new SenseArgument()]));

        var materialize = GetPrivateInstance(
            typeof(SemanticWalker),
            "MaterializeMemberInitializerReceiver",
            typeof(Expression),
            typeof(IOperation),
            typeof(SenseArgument),
            typeof(List<Expression>));
        var operation = block;
        var initializations = new List<Expression>();
        Assert.IsInstanceOfType<Identifier>(materialize.Invoke(walker, [new Identifier("owner"), operation, new SenseArgument(), initializations]));
        Assert.IsEmpty(initializations);
        var callInitializations = new List<Expression>();
        var cached = materialize.Invoke(walker, [new CallExpression(new Identifier("create"), NodeList.Empty<Expression>(), false), operation, new SenseArgument(), callInitializations]);
        Assert.IsInstanceOfType<Identifier>(cached);
        Assert.IsNotEmpty(callInitializations);
    }

    [TestMethod]
    public void SemanticWalkerCreationHelpers_CoverEmitInferenceAndInitializerSymbolShapes()
    {
        var block = CreateBlock(
            """
            public sealed class EmitContext
            {
                public void Emit(string value) { }
            }
            public sealed class TestClass
            {
                static void EmitValue(EmitContext context) => context.Emit("value");
                void TestMethod()
                {
                    var context = new EmitContext();
                    context.Emit("direct");
                    System.Action<EmitContext> callback = EmitValue;
                    callback(context);
                }
            }
            """);
        var unwrap = GetPrivateStatic(typeof(SemanticWalker), "UnwrapEmitInferenceOperation", typeof(IOperation));
        foreach (var operation in block.DescendantsAndSelf().OfType<IOperation>())
        {
            if (operation is IConversionOperation or IDelegateCreationOperation)
                Assert.IsNotNull(unwrap.Invoke(null, [operation]));
        }

        var initializerSymbol = GetPrivateStatic(typeof(SemanticWalker), "GetObjectInitializerMemberSymbol", typeof(IOperation));
        var assignment = block.DescendantsAndSelf().OfType<ISimpleAssignmentOperation>().FirstOrDefault();
        if (assignment is not null)
            Assert.IsNotNull(initializerSymbol.Invoke(null, [assignment]));
        var invocation = block.DescendantsAndSelf().OfType<IInvocationOperation>().First();
        Assert.IsNull(initializerSymbol.Invoke(null, [invocation]));

        var findContext = GetPrivateStatic(
            typeof(SemanticWalker),
            "FindEmitContextParameter",
            typeof(IEnumerable<IParameterSymbol>));
        var method = block.DescendantsAndSelf().OfType<IInvocationOperation>().First().TargetMethod;
        Assert.IsNull(findContext.Invoke(null, [method.Parameters]));
    }

    [TestMethod]
    public void SemanticWalkerTupleBoundaries_CoverTupleComparisonAndNestedDeconstruction()
    {
        var block = CreateBlock(
            """
            public sealed class TestClass
            {
                void TestMethod((int First, int Second) source, (int First, int Second) target)
                {
                    var (first, second) = source;
                    (first, second) = target;
                    var equal = source == target;
                    var different = source != target;
                    _ = equal;
                    _ = different;
                }
            }
            """);
        var walker = new SemanticWalker(true);
        foreach (var operation in block.DescendantsAndSelf().OfType<ITupleBinaryOperation>())
            Assert.IsNotNull(walker.Visit(operation, new SenseArgument()));
        var deconstruction = block.DescendantsAndSelf().OfType<IDeconstructionAssignmentOperation>().ToArray();
        Assert.IsNotEmpty(deconstruction);
        foreach (var operation in deconstruction)
            Assert.IsNotNull(walker.Visit(operation, new SenseArgument()));
    }

    [TestMethod]
    public void SemanticWalkerStatementBoundaries_CoverUsingLoopAndInterpolationTailShapes()
    {
        var compilation = CreateCompilation(
            """
            using System;
            using System.Collections.Generic;

            namespace ECMAScript
            {
                [global::System.AttributeUsage(global::System.AttributeTargets.Class, Inherited = false)]
                public sealed class ECMAScriptAttribute : global::System.Attribute { }
            }

            [ECMAScript.ECMAScript]
            public sealed class Disposable : IDisposable
            {
                public void Dispose() { }
                public static void Dispose(int value) { }
            }

            public sealed class TestClass
            {
                void TestMethod(int value, string text)
                {
                    using (var resource = new Disposable())
                    {
                        _ = resource;
                    }

                    using var second = new Disposable();
                    foreach (var character in text)
                        _ = character;
                    for (var index = 0; index < value; index++)
                        _ = index;

                    var startsWithExpression = $"{value}suffix";
                    var endsWithExpression = $"prefix{value}";
                    _ = startsWithExpression + endsWithExpression;
                }

                void InvalidForInitializer()
                {
                    return;
                }
            }
            """);
        var block = CreateBlockFromCompilation(compilation, "TestClass", "TestMethod");
        var script = new SemanticWalker(true).Visit(block, new SenseArgument())?.ToKnRECMAScript();
        Assert.IsNotNull(script);
        StringAssert.Contains(script, "try", StringComparison.Ordinal);
        StringAssert.Contains(script, "split(\"\")", StringComparison.Ordinal);
        StringAssert.Contains(script, "suffix", StringComparison.Ordinal);

        var disposeType = compilation.GetTypeByMetadataName("Disposable")!;
        var tryResolveDispose = GetPrivateStatic(
            typeof(SemanticWalker),
            "TryResolveUsingDisposeMethod",
            typeof(ITypeSymbol),
            typeof(string),
            typeof(IMethodSymbol).MakeByRefType());
        var foundDispose = new object?[] { disposeType, "Dispose", null };
        Assert.IsTrue((bool)tryResolveDispose.Invoke(null, foundDispose)!);
        var missingDispose = new object?[] { disposeType, "Missing", null };
        Assert.IsFalse((bool)tryResolveDispose.Invoke(null, missingDispose)!);
        var typeParameter = compilation.GetTypeByMetadataName("System.Collections.Generic.List`1")!.TypeParameters[0];
        var nonNamedDispose = new object?[] { typeParameter, "Dispose", null };
        Assert.IsFalse((bool)tryResolveDispose.Invoke(null, nonNamedDispose)!);

        var interfaceImplementation = GetPrivateStatic(
            typeof(SemanticWalker),
            "TryResolveUsingDisposeInterfaceImplementation",
            typeof(ITypeSymbol),
            typeof(string),
            typeof(string),
            typeof(IMethodSymbol).MakeByRefType());
        var interfaceArguments = new object?[] { disposeType, "System.IDisposable", "Dispose", null };
        Assert.IsTrue((bool)interfaceImplementation.Invoke(null, interfaceArguments)!);
        var typeParameterArguments = new object?[] { typeParameter, "System.IDisposable", "Dispose", null };
        Assert.IsFalse((bool)interfaceImplementation.Invoke(null, typeParameterArguments)!);

        var invalidForBlock = CreateBlockFromCompilation(compilation, "TestClass", "InvalidForInitializer");
        var returnOperation = invalidForBlock.Operations.Single();
        var createInitializer = GetPrivateInstance(
            typeof(SemanticWalker),
            "CreateForLoopInitializer",
            typeof(IEnumerable<IOperation>),
            typeof(SenseArgument));
        var initializerFailure = Assert.Throws<TargetInvocationException>(() =>
            createInitializer.Invoke(new SemanticWalker(true), [new[] { returnOperation }, new SenseArgument()]));
        Assert.IsInstanceOfType<OperationTransformationException>(initializerFailure.InnerException);

        var compileDefault = GetPrivateInstance(
            typeof(SemanticWalker),
            "CompileEnumerableDefaultValueOverload",
            typeof(ISymbol),
            typeof(SenseArgument),
            typeof(Expression[]),
            typeof(IOperation));
        var enumerable = compilation.GetTypeByMetadataName("System.Linq.Enumerable")!;
        var defaultIfEmpty = enumerable.GetMembers("DefaultIfEmpty").OfType<IMethodSymbol>()
            .First(static method => method.Parameters.Length == 1);
        var defaultFailure = Assert.Throws<TargetInvocationException>(() =>
            compileDefault.Invoke(new SemanticWalker(true), [defaultIfEmpty, new SenseArgument(), Array.Empty<Expression>(), null]));
        Assert.IsInstanceOfType<InvalidOperationException>(defaultFailure.InnerException);
    }

    [TestMethod]
    public void SemanticWalkerPatternBoundaries_CoverSingleAssignmentCyclesAndSwitchSources()
    {
        var block = CreateBlock(
            """
            public sealed class TestClass
            {
                void TestMethod(int value)
                {
                    var first = value;
                    var second = first;
                    first = second;
                    var result = value switch
                    {
                        0 => 1,
                        _ => 2
                    };
                    switch (value)
                    {
                        case 0:
                            break;
                        default:
                            value++;
                            break;
                    }
                    _ = result;
                }
            }
            """);
        var localReferences = block.DescendantsAndSelf().OfType<ILocalReferenceOperation>().ToArray();
        var resolveSource = GetPrivateInstance(
            typeof(SemanticWalker),
            "ResolveSingleAssignmentValueSource",
            typeof(IOperation),
            typeof(IOperation));
        foreach (var localReference in localReferences)
            _ = resolveSource.Invoke(new SemanticWalker(true), [localReference, block]);

        var resolveIsSource = GetPrivateStatic(typeof(SemanticWalker), "ResolveIsTypeSourceOperation", typeof(IOperation));
        foreach (var operation in block.DescendantsAndSelf().OfType<IOperation>())
            _ = resolveIsSource.Invoke(null, [operation]);

        var switchCase = block.DescendantsAndSelf()
            .OfType<ISwitchOperation>()
            .Single()
            .Cases
            .First();
        var translateCase = GetPrivateInstance(
            typeof(SemanticWalker),
            "TranslatePatternSwitchCaseBodyStatements",
            typeof(IReadOnlyList<IOperation>),
            typeof(SenseArgument));
        _ = translateCase.Invoke(new SemanticWalker(true), [switchCase.Body, new SenseArgument()]);
    }

    [TestMethod]
    public void SemanticWalkerReferenceAndPatternBoundaries_CoverComputedAliasesAndDeterministicValues()
    {
        var parseAlias = GetPrivateStatic(
            typeof(SemanticWalker),
            "TryParseExplicitComputedAliasProperty",
            typeof(string),
            typeof(Expression).MakeByRefType(),
            typeof(string).MakeByRefType());
        foreach (var name in new[] { "[0]", "[\"data-value\"]", "['aria-label']", "plain", "[]", "[unknown]" })
        {
            var arguments = new object?[] { name, null, null };
            _ = parseAlias.Invoke(null, arguments);
        }

        var buildAlias = GetPrivateStatic(
            typeof(SemanticWalker),
            "TryBuildComputedAliasProperty",
            typeof(string),
            typeof(Expression).MakeByRefType());
        var identifier = new object?[] { "validName", null };
        Assert.IsFalse((bool)buildAlias.Invoke(null, identifier)!);
        var numeric = new object?[] { "[12]", null };
        Assert.IsTrue((bool)buildAlias.Invoke(null, numeric)!);
        var stringAlias = new object?[] { "[\"data-value\"]", null };
        Assert.IsTrue((bool)buildAlias.Invoke(null, stringAlias)!);

        var tupleCompilation = CreateCompilation(
            """
            public sealed class Holder
            {
                public (int First, int Second) Pair = (1, 2);

                void TestMethod()
                {
                    object? nil = null;
                    var objectValue = new Holder();
                    var arrayValue = new int[0];
                    int defaultValue = default;
                    var literalValue = 42;
                    var converted = (object)literalValue;
                    _ = (nil, objectValue, arrayValue, defaultValue, converted);
                }
            }
            """);
        var tree = tupleCompilation.SyntaxTrees.Single();
        var model = tupleCompilation.GetSemanticModel(tree);
        var method = tree.GetRoot().DescendantNodes()
            .OfType<MethodDeclarationSyntax>()
            .Single(static declaration => declaration.Identifier.ValueText == "TestMethod");
        var block = Assert.IsInstanceOfType<IBlockOperation>(model.GetOperation(method.Body!));
        var deterministic = GetPrivateInstance(
            typeof(SemanticWalker),
            "TryResolveDeterministicRuntimeValue",
            typeof(IOperation),
            typeof(ITypeSymbol).MakeByRefType(),
            typeof(bool).MakeByRefType());
        foreach (var operation in block.DescendantsAndSelf()
                     .Where(static operation => operation is IObjectCreationOperation or
                                                IArrayCreationOperation or
                                                IDefaultValueOperation or
                                                ILiteralOperation or
                                                IConversionOperation))
        {
            var arguments = new object?[] { operation, null, false };
            _ = deterministic.Invoke(new SemanticWalker(true), arguments);
        }

        var tupleShape = GetPrivateStatic(
            typeof(SemanticWalker),
            "HasSameTupleRuntimeShape",
            typeof(INamedTypeSymbol),
            typeof(INamedTypeSymbol));
        var pair = (INamedTypeSymbol)tupleCompilation.GetTypeByMetadataName("Holder")!
            .GetMembers("Pair")
            .OfType<IFieldSymbol>()
            .Single()
            .Type;
        var matchingTuple = (INamedTypeSymbol)model.Compilation.CreateTupleTypeSymbol(
            [tupleCompilation.GetSpecialType(SpecialType.System_Int32), tupleCompilation.GetSpecialType(SpecialType.System_Int32)],
            ["First", "Second"]);
        var renamedTuple = (INamedTypeSymbol)model.Compilation.CreateTupleTypeSymbol(
            [tupleCompilation.GetSpecialType(SpecialType.System_Int32), tupleCompilation.GetSpecialType(SpecialType.System_Int32)],
            ["Left", "Right"]);
        Assert.IsTrue((bool)tupleShape.Invoke(null, [pair, matchingTuple])!);
        Assert.IsFalse((bool)tupleShape.Invoke(null, [pair, renamedTuple])!);

        var pureChain = GetPrivateStatic(typeof(SemanticWalker), "IsPurePropertyAccessChain", typeof(Expression));
        Assert.IsTrue((bool)pureChain.Invoke(null, [new Identifier("value")])!);
        Assert.IsTrue((bool)pureChain.Invoke(null, [new ThisExpression()])!);
        Assert.IsTrue((bool)pureChain.Invoke(null, [new Super()])!);
        Assert.IsTrue((bool)pureChain.Invoke(null, [new MemberExpression(new Identifier("value"), new Identifier("Name"), false, false)])!);
        Assert.IsTrue((bool)pureChain.Invoke(null, [new MemberExpression(new Identifier("value"), new StringLiteral("data", "\"data\""), true, false)])!);
        Assert.IsFalse((bool)pureChain.Invoke(null, [new MemberExpression(new Identifier("value"), new Identifier("Name"), false, true)])!);
        Assert.IsTrue((bool)pureChain.Invoke(null, [new MemberExpression(new Identifier("value"), new NumericLiteral(1, "1"), true, false)])!);
        Assert.IsFalse((bool)pureChain.Invoke(null, [new MemberExpression(new Identifier("value"), new Identifier("key"), true, false)])!);
        Assert.IsFalse((bool)pureChain.Invoke(null, [new CallExpression(new Identifier("get"), NodeList.Empty<Expression>(), false)])!);
    }

    [TestMethod]
    public void CompilerCoverage98PatternAndMutationShapes_ExerciseNonNullFoldDeconstructionAndPropertyBridges()
    {
        var compilation = CreateCompilation(
            """
            using System;
            using System.Collections.Generic;

            public interface IMarker { }
            public sealed class Implementation : IMarker { }
            public sealed class PlainType { public int Length => 1; }

            public sealed class PatternAndMutationHost
            {
                private int _value;
                public int Value { get => _value; set => _value = value; }
                public int this[int index] { get => _value + index; set => _value = value; }

                public bool Test(IMarker parameter, object input, (int First, int Second) pair)
                {
                    var nonNullOnly = parameter is IMarker;
                    var alwaysTrue = new Implementation() is IMarker;
                    var alwaysFalse = new PlainType() is IMarker;
                    var defaultReference = default(string) is IMarker;
                    var typePattern = input is int number;
                    var recursive = input is { };
                    var listPattern = new[] { 1, 2, 3 } is [var first, .. var rest];
                    (Value, this[0]) = pair;
                    Value += 1;
                    this[0] += 1;
                    _ = (nonNullOnly, alwaysTrue, alwaysFalse, defaultReference, typePattern, recursive, listPattern);
                    return true;
                }
            }
            """);
        var tree = compilation.SyntaxTrees.Single();
        var model = compilation.GetSemanticModel(tree);
        var method = tree.GetRoot().DescendantNodes()
            .OfType<MethodDeclarationSyntax>()
            .Single(candidate => candidate.Identifier.ValueText == "Test");
        var body = Assert.IsInstanceOfType<IBlockOperation>(model.GetOperation(method.Body!));
        var walker = new SemanticWalker(true);

        var marker = compilation.GetTypeByMetadataName("IMarker")!;
        var typeChecks = body.DescendantsAndSelf().OfType<IIsTypeOperation>().ToArray();
        Assert.IsGreaterThanOrEqualTo(4, typeChecks.Length);
        var evaluate = typeof(SemanticWalker).GetMethod(
            "TryEvaluateCompileTimeErasedInterfaceIsTypeCheck",
            BindingFlags.Instance | BindingFlags.NonPublic)!;
        foreach (var check in typeChecks)
        {
            var arguments = new object?[] { check, marker, null };
            _ = evaluate.Invoke(walker, arguments);
        }

        // The test compilation intentionally uses a plain interface with no runtime carrier;
        // lowering it is supposed to reject rather than emit an unsound JavaScript type test.
        Assert.Throws<OperationTransformationException>(() => walker.Visit(body, new SenseArgument()));

        var patternOperations = body.DescendantsAndSelf().OfType<IIsPatternOperation>().ToArray();
        Assert.IsGreaterThanOrEqualTo(3, patternOperations.Length);
        var visitPattern = typeof(SemanticWalker).GetMethod(
            "VisitIsPattern",
            BindingFlags.Instance | BindingFlags.Public)!;
        foreach (var pattern in patternOperations)
            _ = visitPattern.Invoke(walker, [pattern, new SenseArgument()]);

        var resolveSource = GetPrivateStatic(typeof(SemanticWalker), "ResolveIsTypeSourceOperation", typeof(IOperation));
        foreach (var pattern in patternOperations)
            _ = resolveSource.Invoke(null, [pattern.Pattern]);

        var tupleAssignment = body.DescendantsAndSelf().OfType<IDeconstructionAssignmentOperation>().Single();
        var tupleVisit = typeof(SemanticWalker).GetMethod(
            "VisitDeconstructionAssignment",
            BindingFlags.Instance | BindingFlags.Public)!;
        Assert.IsNotNull(tupleVisit.Invoke(walker, [tupleAssignment, new SenseArgument()]));

        var mutation = body.DescendantsAndSelf().OfType<ICompoundAssignmentOperation>().ToArray();
        Assert.IsGreaterThanOrEqualTo(2, mutation.Length);
        foreach (var compound in mutation)
            Assert.IsNotNull(walker.Visit(compound, new SenseArgument()));
    }

    [TestMethod]
    public void SemanticWalkerInlineTemplateBoundaries_CoverPlaceholderValidationAndLegacySyntax()
    {
        var tryGet = GetPrivateStatic(
            typeof(SemanticWalker),
            "TryGetPlaceholderIndex",
            typeof(string),
            typeof(int).MakeByRefType());
        (bool Result, int Index) Invoke(string? value)
        {
            var arguments = new object?[] { value, -1 };
            var result = (bool)tryGet.Invoke(null, arguments)!;
            return (result, (int)arguments[1]!);
        }

        Assert.IsFalse(Invoke(null).Result);
        Assert.IsFalse(Invoke("arg1").Result);
        Assert.IsFalse(Invoke("__arg").Result);
        Assert.IsFalse(Invoke("__arg0").Result);
        Assert.IsFalse(Invoke("__arg1x").Result);
        var valid = Invoke("__arg2");
        Assert.IsTrue(valid.Result);
        Assert.AreEqual(1, valid.Index);
        Assert.Throws<TargetInvocationException>(() => Invoke("__arg999999999999999999999999"));

        var zeroBased = GetPrivateStatic(typeof(SemanticWalker), "IsZeroBasedPlaceholder", typeof(string));
        Assert.IsFalse((bool)zeroBased.Invoke(null, ["__arg"] )!);
        Assert.IsTrue((bool)zeroBased.Invoke(null, ["__arg0"])!);
        Assert.IsFalse((bool)zeroBased.Invoke(null, ["__arg0x"])!);

        var parse = GetPrivateStatic(typeof(SemanticWalker), "ParseInlineTemplate", typeof(string), typeof(string));
        Assert.Throws<TargetInvocationException>(() => parse.Invoke(null, ["legacy", "@#{0}"]));
        Assert.Throws<TargetInvocationException>(() => parse.Invoke(null, ["invalid", "("]));

        var instantiate = GetPrivateStatic(
            typeof(SemanticWalker),
            "InstantiateInlineTemplate",
            typeof(string),
            typeof(string),
            typeof(IReadOnlyList<Expression>),
            typeof(string),
            typeof(Identifier));
        var effectful = new CallExpression(new Identifier("getValue"), NodeList.Empty<Expression>(), optional: false);
        Assert.Throws<TargetInvocationException>(() => instantiate.Invoke(
            null,
            ["arity", "__arg2", new Expression[] { new Identifier("first") }, null, null]));
        _ = instantiate.Invoke(
            null,
            ["import", "foo + __arg1", new Expression[] { effectful }, "foo", new Identifier("fooAlias")]);
        _ = instantiate.Invoke(
            null,
            ["order", "__arg2 + __arg1", new Expression[] { effectful, effectful }, null, null]);
    }

    [TestMethod]
    public void SemanticWalkerReferenceBoundaries_CoverEnumerableIntrinsicGuards()
    {
        var compilation = CreateCompilation(
            """
            using System.Collections.Generic;
            using System.Linq;
            public static class EnumerableBoundaryHost
            {
                public static void TestMethod(IEnumerable<int> values)
                {
                    var array = values.ToArray();
                    var list = values.ToList();
                    var filtered = values.Where(value => value > 0);
                    var indexed = values.Where((value, index) => value > index);
                }
            }
            """);
        var block = CreateBlockFromCompilation(compilation, null, "TestMethod");
        var invocations = block.DescendantsAndSelf().OfType<IInvocationOperation>().ToArray();
        var toArray = invocations.Single(static invocation => invocation.TargetMethod.Name == "ToArray");
        var toList = invocations.Single(static invocation => invocation.TargetMethod.Name == "ToList");
        var whereCandidates = invocations
            .Where(static invocation => invocation.TargetMethod.Name == "Where")
            .OrderBy(static invocation => invocation.Arguments[1].Value.Syntax.Span.Length)
            .ToArray();
        var where = whereCandidates[0];
        var indexedWhere = whereCandidates[^1];
        var intrinsic = GetPrivateInstance(
            typeof(SemanticWalker),
            "TryBuildEnumerableArrayLikeIntrinsic",
            typeof(IMethodSymbol),
            typeof(List<Expression>),
            typeof(ITypeSymbol),
            typeof(SenseArgument),
            typeof(Expression).MakeByRefType());

        bool Invoke(IInvocationOperation operation, ITypeSymbol? sourceType, Expression source)
        {
            var arguments = new object?[]
            {
                operation.TargetMethod,
                new List<Expression> { source, new Identifier("callback") },
                sourceType,
                new SenseArgument(),
                null
            };
            return (bool)intrinsic.Invoke(new SemanticWalker(true), arguments)!;
        }

        Assert.IsTrue(Invoke(toArray, toArray.Arguments[0].Value.Type!, new ArrayExpression(NodeList.Empty<Expression?>())));
        Assert.IsTrue(Invoke(toList, toList.Arguments[0].Value.Type!, new Identifier("values")));
        Assert.IsTrue(Invoke(where, where.Arguments[0].Value.Type!, new Identifier("values")));
        Assert.IsTrue(Invoke(indexedWhere, indexedWhere.Arguments[0].Value.Type!, new Identifier("values")));
        Assert.IsTrue(Invoke(toArray, compilation.GetSpecialType(SpecialType.System_Int32), new Identifier("value")));
        Assert.IsTrue(Invoke(toArray, null, new Identifier("values")));
    }

    [TestMethod]
    public void SemanticWalkerCreationBoundaries_CoverInitializerSymbolsAndInferenceUnwrap()
    {
        var compilation = CreateCompilation(
            """
            using System;
            public sealed class CreationBoundaryHost
            {
                public int Value;
                public void Method() { }
                public static void TestMethod(int seed)
                {
                    var instance = new CreationBoundaryHost { Value = seed };
                    Action callback = StaticMethod;
                    object boxed = (object)seed;
                }
                private static void StaticMethod() { }
            }
            """);
        var block = CreateBlockFromCompilation(compilation, null, "TestMethod");
        var initializer = block.DescendantsAndSelf()
            .OfType<IObjectCreationOperation>()
            .Single()
            .Initializer!
            .Initializers
            .Single();
        var memberSymbol = GetPrivateStatic(
            typeof(SemanticWalker),
            "GetObjectInitializerMemberSymbol",
            typeof(IOperation));
        Assert.IsNotNull(memberSymbol.Invoke(null, [initializer]));
        Assert.IsNull(memberSymbol.Invoke(null, [null]));

        var memberName = GetPrivateStatic(
            typeof(SemanticWalker),
            "GetObjectInitializerMemberName",
            typeof(IOperation));
        Assert.AreEqual("Value", memberName.Invoke(null, [initializer]));

        var unwrap = GetPrivateStatic(
            typeof(SemanticWalker),
            "UnwrapEmitInferenceOperation",
            typeof(IOperation));
        var conversion = block.DescendantsAndSelf().OfType<IConversionOperation>().First();
        var delegateCreation = block.DescendantsAndSelf().OfType<IDelegateCreationOperation>().First();
        var parameterReference = block.DescendantsAndSelf().OfType<IParameterReferenceOperation>().First();
        Assert.AreSame(conversion.Operand, unwrap.Invoke(null, [conversion]));
        Assert.AreSame(delegateCreation.Target, unwrap.Invoke(null, [delegateCreation]));
        Assert.AreSame(parameterReference, unwrap.Invoke(null, [parameterReference]));
    }

    [TestMethod]
    public void AstConverterInheritanceBoundaries_CoverBaseResolutionAndConstructorSelection()
    {
        var compilation = CreateCompilation(
            """
            public static class InheritanceBoundaryModule
            {
                public class Base { public Base() { } public Base(int value = 1) { } }
                public class Derived : Base { public Derived() { } }
                public class Plain { }
            }
            """);
        var syntaxTree = compilation.SyntaxTrees.Single();
        var model = compilation.GetSemanticModel(syntaxTree);
        var module = compilation.GetTypeByMetadataName("InheritanceBoundaryModule")!;
        var converter = new AstConverter(module, model);
        var baseType = module.GetTypeMembers("Base").Single();
        var derivedType = module.GetTypeMembers("Derived").Single();
        var plainType = module.GetTypeMembers("Plain").Single();

        var memberBase = GetPrivateInstance(typeof(AstConverter), "GetSupportedMemberBaseType", typeof(INamedTypeSymbol));
        Assert.IsNull(memberBase.Invoke(converter, [plainType]));
        Assert.AreSame(baseType, memberBase.Invoke(converter, [derivedType]));

        var runtimeBase = GetPrivateInstance(typeof(AstConverter), "GetSupportedRuntimeClassBaseType", typeof(INamedTypeSymbol));
        Assert.IsNull(runtimeBase.Invoke(converter, [plainType]));
        Assert.AreSame(baseType, runtimeBase.Invoke(converter, [derivedType]));

        var hasMultiple = GetPrivateStatic(typeof(AstConverter), "HasMultipleExplicitConstructors", typeof(INamedTypeSymbol));
        Assert.IsTrue((bool)hasMultiple.Invoke(null, [baseType])!);
        Assert.IsFalse((bool)hasMultiple.Invoke(null, [plainType])!);

        var resolveImplicit = GetPrivateStatic(typeof(AstConverter), "ResolveImplicitBaseConstructor", typeof(INamedTypeSymbol));
        Assert.IsNotNull(resolveImplicit.Invoke(null, [baseType]));
        var implicitConstructor = GetPrivateStatic(typeof(AstConverter), "CreateImplicitBaseConstructor", typeof(INamedTypeSymbol));
        Assert.IsNotNull(implicitConstructor.Invoke(null, [baseType]));

        var prepareConstructor = GetPrivateInstance(
            typeof(AstConverter),
            "PrepareMemberConstructorLowering",
            typeof(IMethodSymbol),
            typeof(INamedTypeSymbol),
            typeof(CancellationToken));
        var plainConstructor = plainType.InstanceConstructors.Single();
        var prepareFailure = Assert.Throws<TargetInvocationException>(() =>
            prepareConstructor.Invoke(converter, [plainConstructor, plainType, CancellationToken.None]));
        Assert.IsInstanceOfType<NotSupportedException>(prepareFailure.InnerException);
    }

    [TestMethod]
    public void SemanticWalkerOrdinaryBoundaries_CoverIndexRangeAndReadWriteTargetShapes()
    {
        var compilation = CreateCompilation(
            """
            public sealed class OrdinaryBoundaryHost
            {
                public int Value;
                public int this[int index] { get => Value; set => Value = value; }
                public void TestMethod() { }
            }
            public sealed class Index { }
            public sealed class Range { }
            namespace Foo { public sealed class Index { } public sealed class Range { } public sealed class Other { } }
            namespace System { public sealed class Other { } }
            """);
        var indexType = compilation.GetTypeByMetadataName("System.Index")!;
        var rangeType = compilation.GetTypeByMetadataName("System.Range")!;
        var customIndexType = compilation.GetTypeByMetadataName("Foo.Index")!;
        var customRangeType = compilation.GetTypeByMetadataName("Foo.Range")!;
        var customOtherType = compilation.GetTypeByMetadataName("Foo.Other")!;
        var systemOtherType = compilation.GetTypeByMetadataName("System.Other")!;
        var globalIndexType = compilation.GetTypeByMetadataName("Index")!;
        var globalRangeType = compilation.GetTypeByMetadataName("Range")!;
        var intType = compilation.GetSpecialType(SpecialType.System_Int32);
        var isIndex = GetPrivateStatic(typeof(SemanticWalker), "IsSystemIndexType", typeof(ITypeSymbol));
        var isRange = GetPrivateStatic(typeof(SemanticWalker), "IsSystemRangeType", typeof(ITypeSymbol));
        Assert.IsFalse((bool)isIndex.Invoke(null, [null])!);
        Assert.IsFalse((bool)isIndex.Invoke(null, [intType])!);
        Assert.IsTrue((bool)isIndex.Invoke(null, [indexType])!);
        Assert.IsFalse((bool)isIndex.Invoke(null, [customIndexType])!);
        Assert.IsFalse((bool)isIndex.Invoke(null, [customOtherType])!);
        Assert.IsFalse((bool)isIndex.Invoke(null, [systemOtherType])!);
        Assert.IsFalse((bool)isIndex.Invoke(null, [globalIndexType])!);
        Assert.IsFalse((bool)isRange.Invoke(null, [null])!);
        Assert.IsFalse((bool)isRange.Invoke(null, [intType])!);
        Assert.IsTrue((bool)isRange.Invoke(null, [rangeType])!);
        Assert.IsFalse((bool)isRange.Invoke(null, [customRangeType])!);
        Assert.IsFalse((bool)isRange.Invoke(null, [customOtherType])!);
        Assert.IsFalse((bool)isRange.Invoke(null, [systemOtherType])!);
        Assert.IsFalse((bool)isRange.Invoke(null, [globalRangeType])!);

        var canDuplicate = GetPrivateStatic(typeof(SemanticWalker), "CanDuplicateReadWriteTarget", typeof(Expression));
        Assert.IsTrue((bool)canDuplicate.Invoke(null, [new Identifier("value")])!);
        Assert.IsTrue((bool)canDuplicate.Invoke(null, [new ThisExpression()])!);
        Assert.IsTrue((bool)canDuplicate.Invoke(null, [new MemberExpression(new Identifier("obj"), new Identifier("Value"), false, false)])!);
        Assert.IsTrue((bool)canDuplicate.Invoke(null, [new MemberExpression(new Identifier("obj"), new StringLiteral("Value", "\"Value\""), true, false)])!);
        Assert.IsFalse((bool)canDuplicate.Invoke(null, [new MemberExpression(new Identifier("obj"), new CallExpression(new Identifier("key"), NodeList.Empty<Expression>(), false), true, false)])!);
    }

    [TestMethod]
    public void UtilNamingBoundaries_CoverTupleImplicitFieldsAndEmptyAttributes()
    {
        var compilation = CreateCompilation(
            """
            using System;
            [Obsolete]
            public sealed class TupleNamingHost
            {
                public (int Left, int Right) Pair;
            }
            """);
        var host = compilation.GetTypeByMetadataName("TupleNamingHost")!;
        var pair = host.GetMembers("Pair").OfType<IFieldSymbol>().Single();
        var tuple = (INamedTypeSymbol)pair.Type;
        foreach (var element in tuple.TupleElements)
            Assert.IsFalse(string.IsNullOrEmpty(Util.GetConfigOrSymbolName(element)));

        var metadata = Util.GetJavaScriptNameMetadata(host);
        Assert.IsFalse(metadata.HasECMAScriptNameAttribute);
        Assert.IsNull(metadata.ECMAScriptName);
    }

    [TestMethod]
    public void UtilNamingBoundaries_CoverImportAndAuthoredNameAttributeShapes()
    {
        var compilation = CreateCompilation(
            """
            using System;
            using ECMAScript;
            [AttributeUsage(AttributeTargets.All, AllowMultiple = true)]
            public sealed class JazorAttribute : Attribute
            {
                public JazorAttribute() { }
                public JazorAttribute(int operation, string? member, string? runtime = null) { }
            }
            [AttributeUsage(AttributeTargets.All)]
            public sealed class OtherAttribute : Attribute { }
            [AttributeUsage(AttributeTargets.All)]
            public sealed class ECMAScriptNameAttribute : Attribute
            {
                public ECMAScriptNameAttribute(string? name) { }
            }
            [AttributeUsage(AttributeTargets.All)]
            public sealed class DescriptionAttribute : Attribute
            {
                public DescriptionAttribute(string? value) { }
            }
            namespace ECMAScript
            {
                [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
                public sealed class ECMAScriptAttribute : Attribute { }
                [AttributeUsage(AttributeTargets.Class)]
                public sealed class ECMAScriptModuleAttribute : Attribute
                {
                    public ECMAScriptModuleAttribute(string path) { }
                }
                [AttributeUsage(AttributeTargets.Method)]
                public sealed class ECMAScriptInlineAttribute : Attribute
                {
                    public ECMAScriptInlineAttribute(string template) { }
                }
            }
            [ECMAScript]
            public sealed class ImportShapes
            {
                [Other] public void Other() { }
                [Jazor] public void MissingArguments() { }
                [Jazor(2, "alias")] public void WrongOperation() { }
                [Jazor(3, null)] public void NullMember() { }
                [Jazor(3, "")] public void EmptyMember() { }
                [Jazor(3, "load")] public void ImportWithoutRuntime() { }
                [Jazor(3, "load", "loadRuntime")] public void ImportWithRuntime() { }
                [ECMAScriptName("  authored  "), Description("@#fallback")] public void Named() { }
                [ECMAScriptName(""), Description("@#description")] public void EmptyNamed() { }
                [Description("@#")] public void Boundary() { }
                [Description("ordinary")] public void OrdinaryDescription() { }
                [ECMAScriptInline("value")] public void InlineValue() { }
                [ECMAScriptInline(" ")] public void InlineBlank() { }
            }
            [ECMAScriptModule("./module-shapes")]
            public sealed class ModuleShapes
            {
                public void Overload(int value) { }
                public void Overload(string value) { }
                [ECMAScriptName("named-overload")] public void Overload(double value) { }
            }
            """);
        var host = compilation.GetTypeByMetadataName("ImportShapes")!;
        var importMapping = GetPrivateStatic(
            typeof(Util),
            "TryGetJazorImportMapping",
            typeof(ISymbol),
            typeof(string).MakeByRefType(),
            typeof(string).MakeByRefType());
        foreach (var method in host.GetMembers().OfType<IMethodSymbol>())
        {
            var args = new object?[] { method, null, null };
            _ = importMapping.Invoke(null, args);
            _ = Util.GetConfigOrSymbolName(method);
            _ = Util.TryGetJazorImportRuntimeName(method, out _);
        }

        var metadata = GetPrivateStatic(typeof(Util), "GetJavaScriptNameMetadata", typeof(ISymbol));
        foreach (var method in host.GetMembers().OfType<IMethodSymbol>())
            _ = metadata.Invoke(null, [method]);

        var moduleHost = compilation.GetTypeByMetadataName("ModuleShapes")!;
        foreach (var method in moduleHost.GetMembers().OfType<IMethodSymbol>())
            _ = Util.GetConfigOrSymbolName(method);

        var inline = GetPrivateStatic(typeof(Util), "HasECMAScriptInlineTemplate", typeof(IMethodSymbol));
        Assert.IsTrue((bool)inline.Invoke(null, [host.GetMembers("InlineValue").OfType<IMethodSymbol>().Single()])!);
        Assert.IsFalse((bool)inline.Invoke(null, [host.GetMembers("InlineBlank").OfType<IMethodSymbol>().Single()])!);
        Assert.IsFalse((bool)inline.Invoke(null, [host.GetMembers("Other").OfType<IMethodSymbol>().Single()])!);

        var runtimeMarker = GetPrivateStatic(typeof(Util), "IsRuntimeMarkerType", typeof(ISymbol));
        var moduleType = GetPrivateStatic(typeof(Util), "IsECMAScriptModuleType", typeof(ITypeSymbol));
        Assert.IsTrue((bool)runtimeMarker.Invoke(null, [host])!);
        Assert.IsFalse((bool)runtimeMarker.Invoke(null, [moduleHost])!);
        Assert.IsFalse((bool)runtimeMarker.Invoke(null, [null])!);
        Assert.IsTrue((bool)moduleType.Invoke(null, [moduleHost])!);
        Assert.IsFalse((bool)moduleType.Invoke(null, [host])!);
        Assert.IsFalse((bool)moduleType.Invoke(null, [null])!);

    }

    [TestMethod]
    public void SemanticWalkerReferenceBoundaries_CoverStaticHostCompatibilityAndSourceOverrides()
    {
        var compilation = CreateCompilation(
            """
            namespace ECMAScript
            {
                [global::System.AttributeUsage(global::System.AttributeTargets.Class, Inherited = false)]
                public sealed class ECMAScriptAttribute : global::System.Attribute { }
            }

            [ECMAScript.ECMAScript]
            public class RuntimeHost
            {
                public static int Read(int value) => value;
            }

            public sealed class Caller
            {
                private static int TestMethod()
                {
                    return RuntimeHost.Read(1);
                }
            }
            """);

        var tree = compilation.SyntaxTrees.Single();
        var model = compilation.GetSemanticModel(tree);
        var invocation = tree.GetRoot().DescendantNodes().OfType<InvocationExpressionSyntax>().Single();
        var invocationOperation = Assert.IsInstanceOfType<IInvocationOperation>(model.GetOperation(invocation));
        var preferred = GetPrivateInstance(
            typeof(SemanticWalker),
            "TryBuildPreferredRuntimeStaticMemberAccess",
            typeof(ISymbol),
            typeof(SyntaxNode),
            typeof(SemanticModel),
            typeof(string),
            typeof(Nullable<SenseArgument>),
            typeof(Expression).MakeByRefType());
        var preferredArguments = new object?[]
        {
            invocationOperation.TargetMethod,
            invocation,
            model,
            "Read",
            new SenseArgument(),
            null
        };
        Assert.IsTrue((bool)preferred.Invoke(new SemanticWalker(true), preferredArguments)!);
        Assert.IsNotNull(preferredArguments[5]);

        var compatibilityCompilation = CreateCompilation(
            """
            public interface IRuntimeContract { }
            public class RuntimeBase { }
            public sealed class RuntimeDerived : RuntimeBase, IRuntimeContract { }
            public sealed class Unrelated { }
            """);
        var derived = compatibilityCompilation.GetTypeByMetadataName("RuntimeDerived")!;
        var runtimeBase = compatibilityCompilation.GetTypeByMetadataName("RuntimeBase")!;
        var contract = compatibilityCompilation.GetTypeByMetadataName("IRuntimeContract")!;
        var unrelated = compatibilityCompilation.GetTypeByMetadataName("Unrelated")!;
        var compatible = GetPrivateStatic(
            typeof(SemanticWalker),
            "IsStaticHostOverrideCompatible",
            typeof(INamedTypeSymbol),
            typeof(ITypeSymbol));
        Assert.IsTrue((bool)compatible.Invoke(null, [derived, derived])!);
        Assert.IsTrue((bool)compatible.Invoke(null, [derived, runtimeBase])!);
        Assert.IsTrue((bool)compatible.Invoke(null, [derived, contract])!);
        Assert.IsFalse((bool)compatible.Invoke(null, [derived, unrelated])!);
    }

    [TestMethod]
    public void SemanticWalkerPatternBoundaries_RejectContinueAcrossPatternSwitchIife()
    {
        var block = CreateBlock(
            """
            public sealed class PatternLoopHost
            {
                private static void TestMethod(object value)
                {
                    while (true)
                    {
                        switch (value)
                        {
                            case int:
                                continue;
                            default:
                                break;
                        }
                    }
                }
            }
            """);

        var failure = Assert.Throws<OperationTransformationException>(() =>
            new SemanticWalker(true).Visit(block, new SenseArgument()));
        StringAssert.Contains(failure.Message, "Continue statements inside pattern-matching switch", StringComparison.Ordinal);
    }

    [TestMethod]
    public void SemanticWalkerReferenceBoundaries_UseEnumerableIntrinsicAfterClrMappingMiss()
    {
        var block = CreateBlock(
            """
            using System.Collections.Generic;
            using System.Linq;

            public sealed class EnumerableHost
            {
                private static void TestMethod(IEnumerable<int> values)
                {
                    var result = values.ToArray();
                    _ = result;
                }
            }
            """);
        var invocation = block.DescendantsAndSelf()
            .OfType<IInvocationOperation>()
            .Single(static operation => operation.TargetMethod.Name == "ToArray");
        var intrinsic = GetPrivateInstance(
            typeof(SemanticWalker),
            "TryBuildIntrinsicMethodInvocation",
            typeof(IInvocationOperation),
            typeof(IMethodSymbol),
            typeof(Expression),
            typeof(List<Expression>),
            typeof(SenseArgument),
            typeof(Expression).MakeByRefType());
        var arguments = new List<Expression> { new Identifier("values") };
        var invokeArguments = new object?[]
        {
            invocation,
            invocation.TargetMethod,
            new Identifier("values"),
            arguments,
            new SenseArgument(),
            null
        };
        Assert.IsTrue((bool)intrinsic.Invoke(new SemanticWalker(true), invokeArguments)!);
        Assert.IsNotNull(invokeArguments[5]);
    }

    [TestMethod]
    public void SemanticWalkerPatternBoundaries_KeepNonNullGuardForStaticallyAssignableInterface()
    {
        var block = CreateBlock(
            """
            using System;
            public sealed class RuntimeValue : IDisposable
            {
                public void Dispose() { }
            }

            public sealed class PatternInterfaceHost
            {
                private static bool TestMethod(RuntimeValue value)
                {
                    return value is IDisposable;
                }
            }
            """);

        var expression = new SemanticWalker(true).Visit(block, new SenseArgument());
        Assert.IsNotNull(expression);
        StringAssert.Contains(expression!.ToKnRECMAScript(), "!= null", StringComparison.Ordinal);
    }

    [TestMethod]
    public void SemanticWalkerReferenceAndPatternBoundaries_CoverAliasAndStaticDecisionShapes()
    {
        var compilation = CreateCompilation(
            """
            public interface IFoo { }
            public class Foo : IFoo { }
            public sealed class PatternBoundaryHost
            {
                private static bool Test(int value, string text)
                {
                    var local = value;
                    var exact = value is { };
                    var reference = text is { };
                    return local > 0 && exact && reference;
                }

                private static void Generic<T, U, V>() where T : U where U : IFoo where V : Foo { }
            }
            """);

        var parseAlias = GetPrivateStatic(
            typeof(SemanticWalker),
            "TryParseExplicitComputedAliasProperty",
            typeof(string),
            typeof(Expression).MakeByRefType(),
            typeof(string).MakeByRefType());
        static (bool Result, Expression? Property, string Key) ParseAlias(MethodInfo method, string value)
        {
            var arguments = new object?[] { value, null, null };
            var result = (bool)method.Invoke(null, arguments)!;
            return (result, (Expression?)arguments[1], (string)arguments[2]!);
        }

        var numeric = ParseAlias(parseAlias, "[12]");
        Assert.IsTrue(numeric.Result);
        Assert.IsInstanceOfType<NumericLiteral>(numeric.Property);
        Assert.AreEqual("12", numeric.Key);
        var doubleQuoted = ParseAlias(parseAlias, "[\"name\"]");
        Assert.IsTrue(doubleQuoted.Result);
        Assert.AreEqual("name", doubleQuoted.Key);
        var singleQuoted = ParseAlias(parseAlias, "['name']");
        Assert.IsTrue(singleQuoted.Result);
        Assert.AreEqual("name", singleQuoted.Key);
        Assert.IsFalse(ParseAlias(parseAlias, "[name]").Result);
        Assert.IsFalse(ParseAlias(parseAlias, "name").Result);
        Assert.IsFalse(ParseAlias(parseAlias, "[]").Result);

        var buildAlias = GetPrivateStatic(
            typeof(SemanticWalker),
            "TryBuildComputedAliasProperty",
            typeof(string),
            typeof(Expression).MakeByRefType());
        var identifierArguments = new object?[] { "name", null };
        Assert.IsFalse((bool)buildAlias.Invoke(null, identifierArguments)!);
        var spacedArguments = new object?[] { "name with space", null };
        Assert.IsTrue((bool)buildAlias.Invoke(null, spacedArguments)!);
        Assert.IsInstanceOfType<StringLiteral>(spacedArguments[1]);

        var model = compilation.GetSemanticModel(compilation.SyntaxTrees.Single());
        var methodSyntax = compilation.SyntaxTrees.Single().GetRoot().DescendantNodes()
            .OfType<MethodDeclarationSyntax>()
            .Single(static method => method.Identifier.ValueText == "Test");
        var body = Assert.IsInstanceOfType<IBlockOperation>(model.GetOperation(methodSyntax.Body!));
        var recursivePatterns = body.DescendantsAndSelf()
            .OfType<IIsPatternOperation>()
            .Select(static operation => operation.Pattern)
            .OfType<IRecursivePatternOperation>()
            .ToArray();
        Assert.HasCount(2, recursivePatterns);
        var recursiveFallback = GetPrivateStatic(
            typeof(SemanticWalker),
            "BuildRecursivePatternFallbackMatch",
            typeof(IRecursivePatternOperation),
            typeof(Expression));
        foreach (var pattern in recursivePatterns)
            Assert.IsNotNull(recursiveFallback.Invoke(null, [pattern, new Identifier("value")]));

        var canUseIn = GetPrivateStatic(
            typeof(SemanticWalker),
            "CanUseInOperatorForPropertyExistenceCheck",
            typeof(ITypeSymbol));
        Assert.IsFalse((bool)canUseIn.Invoke(null, [compilation.GetSpecialType(SpecialType.System_String)])!);
        Assert.IsFalse((bool)canUseIn.Invoke(null, [compilation.GetSpecialType(SpecialType.System_Int32)])!);
        Assert.IsFalse((bool)canUseIn.Invoke(null, [compilation.GetSpecialType(SpecialType.System_Boolean)])!);
        Assert.IsTrue((bool)canUseIn.Invoke(null, [compilation.GetTypeByMetadataName("PatternBoundaryHost")!])!);

        var genericMethod = compilation.GetTypeByMetadataName("PatternBoundaryHost")!
            .GetMembers("Generic").OfType<IMethodSymbol>().Single();
        var assignableToInterface = GetPrivateStatic(
            typeof(SemanticWalker),
            "IsRuntimeTypeAssignableToInterface",
            typeof(ITypeSymbol),
            typeof(ITypeSymbol));
        var fooInterface = compilation.GetTypeByMetadataName("IFoo")!;
        Assert.IsTrue((bool)assignableToInterface.Invoke(null, [genericMethod.TypeParameters[0], fooInterface])!);
        Assert.IsTrue((bool)assignableToInterface.Invoke(null, [genericMethod.TypeParameters[1], fooInterface])!);
        Assert.IsTrue((bool)assignableToInterface.Invoke(null, [genericMethod.TypeParameters[2], fooInterface])!);
        Assert.IsFalse((bool)assignableToInterface.Invoke(null, [compilation.GetSpecialType(SpecialType.System_Int32), fooInterface])!);

        var resolvePatternSource = GetPrivateStatic(
            typeof(SemanticWalker),
            "ResolveIsTypeSourceOperation",
            typeof(IOperation));
        var patternOperation = body.DescendantsAndSelf().OfType<IIsPatternOperation>().First();
        Assert.IsNull(resolvePatternSource.Invoke(null, [patternOperation]));
        Assert.IsNotNull(resolvePatternSource.Invoke(null, [patternOperation.Pattern]));

        var resolveLocal = GetPrivateStatic(
            typeof(SemanticWalker),
            "TryResolveSingleAssignmentLocalInitializer",
            typeof(ILocalReferenceOperation),
            typeof(IOperation),
            typeof(IOperation).MakeByRefType());
        var localReference = body.DescendantsAndSelf().OfType<ILocalReferenceOperation>()
            .Single(reference => reference.Local.Name == "local");
        var useSite = body.DescendantsAndSelf().OfType<IReturnOperation>().Single();
        var localArguments = new object?[] { localReference, useSite, null };
        Assert.IsTrue((bool)resolveLocal.Invoke(null, localArguments)!);
        Assert.IsNotNull(localArguments[2]);
    }

    [TestMethod]
    public void SemanticWalkerCoverage98Boundaries_CoverDeconstructAndPatternRejectionFamilies()
    {
        var structDeconstruct = CreateBlock(
            """
            public struct StructPoint
            {
                public void Deconstruct(out int x, out int y)
                {
                    x = 1;
                    y = 2;
                }
            }

            public sealed class StructDeconstructHost
            {
                static void TestMethod(StructPoint point)
                {
                    var (x, y) = point;
                }
            }
            """);
        var structFailure = Assert.Throws<OperationTransformationException>(() =>
            new SemanticWalker(true).Visit(structDeconstruct, new SenseArgument()));
        StringAssert.Contains(structFailure.Message, "Custom Deconstruct on struct type", StringComparison.Ordinal);

        var extensionDeconstruct = CreateBlock(
            """
            public sealed class ExtensionPoint { }

            public static class ExtensionPointExtensions
            {
                public static void Deconstruct(this ExtensionPoint point, out int x, out int y)
                {
                    x = 1;
                    y = 2;
                }
            }

            public sealed class ExtensionDeconstructHost
            {
                static void TestMethod(ExtensionPoint point)
                {
                    var (x, y) = point;
                }
            }
            """);
        var extensionFailure = Assert.Throws<OperationTransformationException>(() =>
            new SemanticWalker(true).Visit(extensionDeconstruct, new SenseArgument()));
        StringAssert.Contains(extensionFailure.Message, "Extension Deconstruct method", StringComparison.Ordinal);

        var labeledSwitch = CreateBlock(
            """
            public sealed class LabeledPatternHost
            {
                static void TestMethod(object value)
                {
                    switch (value)
                    {
                        case int number:
                            goto default;
                        default:
                            break;
                    }
                }
            }
            """);
        var labeledFailure = Assert.Throws<OperationTransformationException>(() =>
            new SemanticWalker(true).Visit(labeledSwitch, new SenseArgument()));
        StringAssert.Contains(labeledFailure.Message, "Goto statements are not supported", StringComparison.Ordinal);

        var valuePatternCompilation = CreateCompilation(
            """
            public sealed class ValuePatternHost
            {
                static int TestMethod(int value)
                {
                    return value is int { } ? 1 : 0;
                }
            }
            """);
        var valuePatternBlock = CreateBlockFromCompilation(valuePatternCompilation, null, "TestMethod");
        var valuePattern = valuePatternBlock.DescendantsAndSelf()
            .OfType<IRecursivePatternOperation>()
            .Single();
        var recursiveFallback = GetPrivateStatic(
            typeof(SemanticWalker),
            "BuildRecursivePatternFallbackMatch",
            typeof(IRecursivePatternOperation),
            typeof(Expression));
        Assert.AreEqual("true", ((Expression)recursiveFallback.Invoke(null, [valuePattern, new Identifier("value")])!).ToKnRECMAScript());
    }

    [TestMethod]
    public void SemanticWalkerCoverage98Boundaries_DistinguishOrdinaryAndExtensionRuntimeHosts()
    {
        var compilation = CreateCompilation(
            """
            namespace ECMAScript
            {
                [global::System.AttributeUsage(global::System.AttributeTargets.Class, Inherited = false)]
                public sealed class ECMAScriptAttribute : global::System.Attribute { }
            }

            [ECMAScript.ECMAScript]
            public sealed class RuntimeHosts
            {
                public int Instance(int value) => value;
                public static int Normal(int value) => value;
            }

            public static class RuntimeHostExtensions
            {
                public static int Extension(this RuntimeHosts host, int value) => value;
            }
            """);
        var host = compilation.GetTypeByMetadataName("RuntimeHosts")!;
        var walker = new SemanticWalker(true);
        var buildExtensionHost = GetPrivateInstance(
            typeof(SemanticWalker),
            "TryBuildExtensionHostTarget",
            typeof(IMethodSymbol),
            typeof(Nullable<SenseArgument>));
        var instance = host.GetMembers("Instance").OfType<IMethodSymbol>().Single();
        var normal = host.GetMembers("Normal").OfType<IMethodSymbol>().Single();
        var extension = compilation.GetTypeByMetadataName("RuntimeHostExtensions")!
            .GetMembers("Extension").OfType<IMethodSymbol>().Single();
        Assert.IsNull(buildExtensionHost.Invoke(walker, [instance, null]));
        Assert.IsNotNull(buildExtensionHost.Invoke(walker, [normal, null]));
        _ = buildExtensionHost.Invoke(walker, [extension, null]);
    }

    [TestMethod]
    public void SemanticWalkerUsingBoundaries_CoverDisposeResolutionFamilies()
    {
        var compilation = CreateCompilation(
            """
            using System;
            public sealed class DisposableHost : IDisposable
            {
                public void Dispose() { }
                public static void StaticDispose() { }
            }
            public sealed class PlainHost { }
            public sealed class GenericUsingHost
            {
                private static void Use<T>() where T : IDisposable { }
                private static void UseDeclaration()
                {
                    using var resource = new DisposableHost();
                }
            }
            """);
        var disposable = compilation.GetTypeByMetadataName("DisposableHost")!;
        var plain = compilation.GetTypeByMetadataName("PlainHost")!;
        var resolveDirect = GetPrivateStatic(
            typeof(SemanticWalker),
            "TryResolveUsingDisposeMethod",
            typeof(ITypeSymbol),
            typeof(string),
            typeof(IMethodSymbol).MakeByRefType());
        var directArguments = new object?[] { disposable, "Dispose", null };
        Assert.IsTrue((bool)resolveDirect.Invoke(null, directArguments)!);
        Assert.IsNotNull(directArguments[2]);
        var missingArguments = new object?[] { plain, "Dispose", null };
        Assert.IsFalse((bool)resolveDirect.Invoke(null, missingArguments)!);

        var resolveInterface = GetPrivateStatic(
            typeof(SemanticWalker),
            "TryResolveUsingDisposeMethodByInterface",
            typeof(ITypeSymbol),
            typeof(string),
            typeof(string),
            typeof(IMethodSymbol).MakeByRefType());
        var interfaceArguments = new object?[] { disposable, "System.IDisposable", "Dispose", null };
        Assert.IsTrue((bool)resolveInterface.Invoke(null, interfaceArguments)!);
        var missingInterfaceArguments = new object?[] { plain, "System.IDisposable", "Dispose", null };
        Assert.IsFalse((bool)resolveInterface.Invoke(null, missingInterfaceArguments)!);

        var resolveImplementation = GetPrivateStatic(
            typeof(SemanticWalker),
            "TryResolveUsingDisposeInterfaceImplementation",
            typeof(ITypeSymbol),
            typeof(string),
            typeof(string),
            typeof(IMethodSymbol).MakeByRefType());
        var implementationArguments = new object?[] { disposable, "System.IDisposable", "Dispose", null };
        Assert.IsTrue((bool)resolveImplementation.Invoke(null, implementationArguments)!);

        var genericUsing = compilation.GetTypeByMetadataName("GenericUsingHost")!
            .GetMembers("Use").OfType<IMethodSymbol>().Single();
        var resolveTypeParameter = GetPrivateStatic(
            typeof(SemanticWalker),
            "TryResolveUsingTypeParameterDisposeMethod",
            typeof(ITypeParameterSymbol),
            typeof(string),
            typeof(string),
            typeof(IMethodSymbol).MakeByRefType());
        var typeParameterArguments = new object?[]
        {
            genericUsing.TypeParameters[0], "System.IDisposable", "Dispose", null
        };
        Assert.IsTrue((bool)resolveTypeParameter.Invoke(null, typeParameterArguments)!);

        var translateRange = GetPrivateInstance(
            typeof(SemanticWalker),
            "TranslateOperationsRangeToStatements",
            typeof(IReadOnlyList<IOperation>),
            typeof(int),
            typeof(SenseArgument));
        Assert.IsEmpty((System.Collections.IEnumerable)translateRange.Invoke(
            new SemanticWalker(true), [Array.Empty<IOperation>(), 0, new SenseArgument()])!);

        var usingDeclarationMethod = compilation.GetTypeByMetadataName("GenericUsingHost")!
            .GetMembers("UseDeclaration").OfType<IMethodSymbol>().Single();
        var usingDeclarationTree = usingDeclarationMethod.DeclaringSyntaxReferences.Single().SyntaxTree;
        var usingDeclarationModel = compilation.GetSemanticModel(usingDeclarationTree);
        var usingDeclarationSyntax = (MethodDeclarationSyntax)usingDeclarationMethod.DeclaringSyntaxReferences.Single().GetSyntax();
        var usingDeclarationBody = Assert.IsInstanceOfType<IBlockOperation>(
            usingDeclarationModel.GetOperation(usingDeclarationSyntax.Body!));
        var usingDeclaration = usingDeclarationBody.Operations.OfType<IUsingDeclarationOperation>().Single();
        Assert.Throws<OperationTransformationException>(() =>
            new SemanticWalker(true).VisitUsingDeclaration(usingDeclaration, new SenseArgument()));

        var disposalKind = GetPrivateStatic(typeof(SemanticWalker), "GetUsingDisposalKind", typeof(bool));
        Assert.AreEqual("Synchronous", disposalKind.Invoke(null, [false])!.ToString());
        Assert.AreEqual("Asynchronous", disposalKind.Invoke(null, [true])!.ToString());

        var canReuse = GetPrivateStatic(typeof(SemanticWalker), "CanReuseUsingResourceExpression", typeof(Expression));
        Assert.IsTrue((bool)canReuse.Invoke(null, [new Identifier("resource")])!);
        Assert.IsTrue((bool)canReuse.Invoke(null, [new ThisExpression()])!);
        Assert.IsTrue((bool)canReuse.Invoke(null, [new Super()])!);
        Assert.IsFalse((bool)canReuse.Invoke(null, [new CallExpression(new Identifier("factory"), NodeList.Empty<Expression>(), optional: false)])!);
    }

    [TestMethod]
    public void SemanticWalkerCreationBoundaries_RejectTypeParameterConstructionAndDynamicKeys()
    {
        var compilation = CreateCompilation(
            """
            public sealed class CreationBoundaryHost
            {
                private static T Create<T>() where T : new()
                {
                    return new T();
                }
            }
            """);
        var tree = compilation.SyntaxTrees.Single();
        var model = compilation.GetSemanticModel(tree);
        var method = tree.GetRoot().DescendantNodes()
            .OfType<MethodDeclarationSyntax>()
            .Single(static declaration => declaration.Identifier.ValueText == "Create");
        var body = Assert.IsInstanceOfType<IBlockOperation>(model.GetOperation(method.Body!));
        var typeParameterCreation = body.DescendantsAndSelf().OfType<ITypeParameterObjectCreationOperation>().Single();
        var failure = Assert.Throws<OperationTransformationException>(() =>
            new SemanticWalker(true).Visit(typeParameterCreation, new SenseArgument()));
        StringAssert.Contains(failure.Message, "new T()", StringComparison.Ordinal);

        var rejectDynamicKey = GetPrivateInstance(
            typeof(SemanticWalker),
            "RejectUnsupportedDynamicObjectLiteralKey",
            typeof(IOperation),
            typeof(ITypeSymbol),
            typeof(string));
        var operation = body.Operations[0];
        var reflectionFailure = Assert.Throws<TargetInvocationException>(() =>
            rejectDynamicKey.Invoke(
                new SemanticWalker(true),
                [operation, compilation.GetTypeByMetadataName("CreationBoundaryHost"), "coverage"]));
        Assert.IsInstanceOfType<OperationTransformationException>(reflectionFailure.InnerException);

        var createNumericLiteral = GetPrivateStatic(
            typeof(SemanticWalker),
            "TryCreateNumericLiteral",
            typeof(object),
            typeof(Expression).MakeByRefType());
        foreach (var value in new object[]
        {
            (byte)1, (sbyte)2, (short)3, (ushort)4, 5, (uint)6, 1.5f, 2.5d, 3.5m
        })
        {
            var numericArguments = new object?[] { value, null };
            Assert.IsTrue((bool)createNumericLiteral.Invoke(null, numericArguments)!);
            Assert.IsInstanceOfType<NumericLiteral>(numericArguments[1]);
        }

        var unsupportedNumericArguments = new object?[] { "not numeric", null };
        Assert.IsFalse((bool)createNumericLiteral.Invoke(null, unsupportedNumericArguments)!);
    }

    [TestMethod]
    public void SemanticWalkerReferenceBoundaries_CoverProxyStorageAndComputedAliasShapes()
    {
        var compilation = CreateCompilation(
            """
            public sealed class Outer
            {
                public int Value { get; set; }
                public int this[int index]
                {
                    get => index;
                    set { }
                }

                public sealed class Child
                {
                    private int Hidden;
                }
            }
            """);
        var outer = compilation.GetTypeByMetadataName("Outer")!;
        var child = outer.GetTypeMembers("Child").Single();
        var hidden = child.GetMembers("Hidden").OfType<IFieldSymbol>().Single();
        var value = outer.GetMembers("Value").OfType<IPropertySymbol>().Single();
        var indexer = outer.GetMembers().OfType<IPropertySymbol>().Single(static property => property.IsIndexer);
        var declaredNames = new Dictionary<ISymbol, string>(SymbolEqualityComparer.Default)
        {
            [outer] = "Outer",
            [child] = "Child"
        };

        var proxyWalker = new SemanticWalker(
            outer,
            declaredNames,
            CancellationToken.None,
            RuntimeClassPrivateStorage.ProxySafeMangledProperties);
        var buildFieldAccess = GetPrivateInstance(
            typeof(SemanticWalker),
            "BuildFieldAccess",
            typeof(Expression),
            typeof(IFieldSymbol),
            typeof(string),
            typeof(bool));
        var proxyField = (Expression)buildFieldAccess.Invoke(
            proxyWalker,
            [new Identifier("instance"), hidden, "Hidden", false])!;
        StringAssert.Contains(proxyField.ToKnRECMAScript(), "$jazor$private$", StringComparison.Ordinal);

        var isIndexer = GetPrivateInstance(
            typeof(SemanticWalker),
            "IsCurrentModuleRuntimeIndexer",
            typeof(IPropertySymbol));
        Assert.IsTrue((bool)isIndexer.Invoke(proxyWalker, [indexer])!);
        Assert.IsFalse((bool)isIndexer.Invoke(proxyWalker, [value])!);

        var parseAlias = GetPrivateStatic(
            typeof(SemanticWalker),
            "TryParseExplicitComputedAliasProperty",
            typeof(string),
            typeof(Expression).MakeByRefType(),
            typeof(string).MakeByRefType());
        foreach (var (text, expected) in new[]
        {
            ("[12]", "12"),
            ("[+1]", "1"),
            ("[-1]", "-1"),
            ("[\"title\"]", "title"),
            ("['label']", "label"),
            ("[\"\"]", string.Empty)
        })
        {
            var arguments = new object?[] { text, null, null };
            Assert.IsTrue((bool)parseAlias.Invoke(null, arguments)!);
            Assert.AreEqual(expected, arguments[2]);
        }

        foreach (var text in new[]
        {
            "", "[]", "[ ]", "[name]", "[1.0]",
            "[\"unterminated", "[\"mixed']", "['mixed\"]"
        })
        {
            var arguments = new object?[] { text, null, null };
            Assert.IsFalse((bool)parseAlias.Invoke(null, arguments)!, text);
        }
    }

    [TestMethod]
    public void SemanticWalkerCreationBoundaries_CoverConstructorKeyAndEmitResolutionFamilies()
    {
        var compilation = CreateCompilation(
            """
            namespace ECMAScript
            {
                public sealed class Number { }
            }

            public sealed class EmitContext
            {
                public void Emit(string value) { }
            }

            public sealed class ExceptionChild : System.Exception { }

            public sealed class Holder
            {
                public int Value { get; set; }
                public Holder Child { get; } = new();

                public static void EmitValue(EmitContext context) { }

                public void Test(int value, uint unsignedValue)
                {
                    var numeric = 12;
                    var text = "12";
                    var nested = new Holder { Child = { Value = numeric } };
                    var upper = value.ToString("X");
                    var lower = value.ToString("x");
                    var ordinary = value.ToString("D");
                    _ = (unsignedValue, text, nested, upper, lower, ordinary);
                }
            }
            """);
        var tree = compilation.SyntaxTrees.Single();
        var model = compilation.GetSemanticModel(tree);
        var methodSyntax = tree.GetRoot().DescendantNodes()
            .OfType<MethodDeclarationSyntax>()
            .Single(static method => method.Identifier.ValueText == "Test");
        var block = Assert.IsInstanceOfType<IBlockOperation>(model.GetOperation(methodSyntax.Body!));
        var holder = compilation.GetTypeByMetadataName("Holder")!;

        var nativeError = GetPrivateStatic(
            typeof(SemanticWalker),
            "IsNativeErrorConstructorFallbackAllowed",
            typeof(ITypeSymbol),
            typeof(string));
        var exceptionChild = compilation.GetTypeByMetadataName("ExceptionChild")!;
        Assert.IsTrue((bool)nativeError.Invoke(null, [exceptionChild, "Error"])!);
        Assert.IsTrue((bool)nativeError.Invoke(null, [exceptionChild, "TypeError"])!);
        Assert.IsFalse((bool)nativeError.Invoke(null, [exceptionChild, "Other"])!);
        Assert.IsFalse((bool)nativeError.Invoke(null, [holder, "Error"])!);

        var numericKey = GetPrivateStatic(
            typeof(SemanticWalker),
            "TryCreateNumericObjectPropertyKey",
            typeof(IOperation),
            typeof(ITypeSymbol),
            typeof(Expression).MakeByRefType(),
            typeof(bool).MakeByRefType());
        var numericOperation = block.DescendantsAndSelf()
            .OfType<ILiteralOperation>()
            .Single(operation => Equals(operation.ConstantValue.Value, 12));
        var textOperation = block.DescendantsAndSelf()
            .OfType<ILiteralOperation>()
            .Single(operation => Equals(operation.ConstantValue.Value, "12"));
        var numericArguments = new object?[] { numericOperation, compilation.GetTypeByMetadataName("ECMAScript.Number")!, null, false };
        Assert.IsTrue((bool)numericKey.Invoke(null, numericArguments)!);
        Assert.AreEqual("12", ((Expression)numericArguments[2]!).ToKnRECMAScript());
        var wrongTypeArguments = new object?[] { numericOperation, compilation.GetSpecialType(SpecialType.System_Int32), null, false };
        Assert.IsFalse((bool)numericKey.Invoke(null, wrongTypeArguments)!);
        var nonNumericArguments = new object?[] { textOperation, compilation.GetTypeByMetadataName("ECMAScript.Number")!, null, false };
        Assert.IsFalse((bool)numericKey.Invoke(null, nonNumericArguments)!);

        var hexIntrinsic = GetPrivateStatic(
            typeof(SemanticWalker),
            "TryBuildIntegerHexToStringIntrinsic",
            typeof(IMethodSymbol),
            typeof(Expression),
            typeof(IReadOnlyList<Expression>),
            typeof(Expression).MakeByRefType());
        var intToString = block.DescendantsAndSelf()
            .OfType<IInvocationOperation>()
            .Where(static invocation => invocation.TargetMethod.Name == nameof(object.ToString))
            .Where(static invocation => invocation.TargetMethod.Parameters.Length == 1)
            .ToArray();
        foreach (var invocation in intToString)
        {
            var format = ((ILiteralOperation)invocation.Arguments[0].Value).ConstantValue.Value?.ToString();
            var arguments = new object?[]
            {
                invocation.TargetMethod,
                new Identifier("value"),
                new Expression[] { new StringLiteral(format!, $"\"{format}\"") },
                null
            };
            var lowered = (bool)hexIntrinsic.Invoke(null, arguments)!;
            if (format is "X" or "x")
                Assert.IsTrue(lowered);
            else
                Assert.IsFalse(lowered);
        }

        var initializerSymbol = GetPrivateStatic(
            typeof(SemanticWalker),
            "GetObjectInitializerMemberSymbol",
            typeof(IOperation));
        var nestedInitializer = block.DescendantsAndSelf().OfType<IMemberInitializerOperation>().Single();
        Assert.IsNotNull(initializerSymbol.Invoke(null, [nestedInitializer]));

        var findContext = GetPrivateStatic(
            typeof(SemanticWalker),
            "FindEmitContextParameter",
            typeof(IEnumerable<IParameterSymbol>));
        var emitMethod = holder.GetMembers("EmitValue").OfType<IMethodSymbol>().Single();
        Assert.IsNotNull(findContext.Invoke(null, [emitMethod.Parameters]));
        var intMethod = holder.GetMembers("Test").OfType<IMethodSymbol>().Single();
        Assert.IsNull(findContext.Invoke(null, [intMethod.Parameters]));

        var emitInvocation = GetPrivateStatic(
            typeof(SemanticWalker),
            "IsEmitContextInvocation",
            typeof(IOperation),
            typeof(IParameterSymbol));
        Assert.IsFalse((bool)emitInvocation.Invoke(null, [null, emitMethod.Parameters[0]])!);

        var numericNullTypeArguments = new object?[] { numericOperation, null, null, false };
        Assert.IsFalse((bool)numericKey.Invoke(null, numericNullTypeArguments)!);
    }

    [TestMethod]
    public void SemanticWalkerOperationShapes_TranslateOrRejectExplicitlyAcrossRuntimeSensitiveForms()
    {
        var compilation = CreateCompilation(
            """
            using System;
            using System.Collections.Generic;

            namespace ECMAScript
            {
                [global::System.AttributeUsage(global::System.AttributeTargets.Class, Inherited = false)]
                public sealed class ECMAScriptAttribute : global::System.Attribute { }
            }

            [ECMAScript.ECMAScript]
            public static class RuntimeHost
            {
                public static int Value { get; set; }
                public static int Read(int value) => value;
            }

            public sealed class Disposable : IDisposable
            {
                public void Dispose() { }
            }

            public sealed class Shape
            {
                public int Value { get; set; }
                public int this[int index]
                {
                    get => index;
                    set => Value = value;
                }

                public void Deconstruct(out int value, out int doubled)
                {
                    value = Value;
                    doubled = Value * 2;
                }

                public int Read() => Value;
            }

            public sealed class TestClass
            {
                void TestMethod(int input, string? text, Shape shape, int[] items)
                {
                    var created = new Shape { Value = input };
                    var list = new List<int> { input, input + 1 };
                    var map = new Dictionary<string, int> { ["value"] = input };
                    var tuple = (input, input + 1);
                    var (first, second) = tuple;
                    (first, second) = tuple;
                    var (fromShape, doubled) = shape;
                    shape.Value += input;
                    shape[input] = input;
                    var pattern = input is > 0 and < 100;
                    var typePattern = shape is Shape { Value: > 0 };
                    var switchValue = input switch { 0 => "zero", _ => text ?? "other" };
                    if (text is { Length: > 0 } matched)
                        switchValue = matched;
                    foreach (var item in items)
                        _ = item;
                    for (var index = 0; index < items.Length; index++)
                        _ = items[index];
                    using (var resource = new Disposable())
                        _ = resource;
                    var hostValue = RuntimeHost.Value;
                    var hostRead = RuntimeHost.Read(input);
                    var method = shape.Read;
                    var optional = shape?.Value;
                    var ranged = items[1..^1];
                    var interpolated = $"{input}:{switchValue}";
                    _ = (created, list, map, fromShape, doubled, pattern, typePattern, hostValue, hostRead, method, optional, ranged, interpolated);
                }
            }
            """);

        var block = CreateBlockFromCompilation(compilation, "TestClass", "TestMethod");
        var operations = block.DescendantsAndSelf()
            .Where(static operation => operation is not IBlockOperation)
            .ToArray();
        var translated = 0;
        var explicitFailures = 0;
        foreach (var operation in operations)
        {
            try
            {
                var node = new SemanticWalker(true).Visit(operation, new SenseArgument());
                if (node is not null)
                {
                    _ = node.ToKnRECMAScript();
                    translated++;
                }
            }
            catch (OperationTransformationException)
            {
                explicitFailures++;
            }
            catch (NotSupportedException)
            {
                explicitFailures++;
            }
            catch (InvalidOperationException)
            {
                explicitFailures++;
            }
        }

        Assert.IsGreaterThan(20, translated);
        Assert.IsGreaterThan(0, explicitFailures);
    }

    [TestMethod]
    public void CompilerBoundaryHelpers_CoverReverseEvaluationAndFailureProtocols()
    {
        var compilation = CreateCompilation(
            """
            using System;
            using System.Collections.Generic;
            using System.Linq;

            public sealed class Disposable : IDisposable
            {
                public void Dispose() { }
            }

            public sealed class BoundaryHost
            {
                private static void TestMethod(IEnumerable<int> values, object? value)
                {
                    for (var index = 0; index < 1; index++)
                    {
                        Func<int> captured = () => index;
                        _ = captured();
                    }

                    for (var other = 0; other < 1; other++)
                        _ = other;

                    using var resource = new Disposable();
                    var interpolated = $"value:{value}";
                    var array = values.ToArray();
                    _ = (resource, interpolated, array);
                }

                private static void Unconstrained<T>()
                {
                    _ = typeof(T);
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

        var capturedLoop = GetPrivateStatic(typeof(SemanticWalker), "HasCapturedForControlVariable", typeof(IForLoopOperation));
        var loopResults = block.DescendantsAndSelf().OfType<IForLoopOperation>()
            .Select(loop => (bool)capturedLoop.Invoke(null, [loop])!)
            .ToArray();
        Assert.IsTrue(loopResults.Any(static result => result));
        Assert.IsTrue(loopResults.Any(static result => !result));

        var interpolation = block.DescendantsAndSelf().OfType<IInterpolationOperation>().Single();
        var rejectInterpolation = GetPrivateInstance(
            typeof(SemanticWalker),
            "RejectUnsupportedInterpolationRuntimeType",
            typeof(IInterpolationOperation),
            typeof(ITypeSymbol));
        Assert.Throws<TargetInvocationException>(() => rejectInterpolation.Invoke(
            walker,
            [interpolation, compilation.GetSpecialType(SpecialType.System_Object)]));
        var genericMethod = compilation.GetTypeByMetadataName("BoundaryHost")!
            .GetMembers("Unconstrained").OfType<IMethodSymbol>().Single();
        Assert.Throws<TargetInvocationException>(() => rejectInterpolation.Invoke(
            walker,
            [interpolation, genericMethod.TypeParameters[0]]));

        var parseInline = GetPrivateStatic(typeof(SemanticWalker), "ParseInlineTemplate", typeof(string), typeof(string));
        var parsed = parseInline.Invoke(null, ["coverage-boundary", "__arg2 + __arg1"])!;
        var requiresBoundary = typeof(SemanticWalker)
            .GetMethods(BindingFlags.Static | BindingFlags.NonPublic)
            .Single(static method => method.Name == "RequiresInlineEvaluationBoundary");
        var effectful = new CallExpression(new Identifier("read"), NodeList.Empty<Expression>(), optional: false);
        _ = requiresBoundary.Invoke(null, [parsed, new Expression[] { effectful, new Identifier("plain") }]);
        _ = requiresBoundary.Invoke(null, [parsed, new Expression[] { new Identifier("first"), new Identifier("second") }]);

        var enumerable = compilation.GetTypeByMetadataName("System.Linq.Enumerable")!;
        var defaultMethods = enumerable.GetMembers("DefaultIfEmpty").OfType<IMethodSymbol>().ToArray();
        var defaultFallback = GetPrivateStatic(
            typeof(SemanticWalker),
            "IsDefaultValueFallbackOverload",
            typeof(IMethodSymbol),
            typeof(IMethodSymbol));
        var fallbackResults = new List<bool>();
        foreach (var method in defaultMethods)
        foreach (var candidate in defaultMethods)
            fallbackResults.Add((bool)defaultFallback.Invoke(null, [method, candidate])!);
        Assert.IsTrue(fallbackResults.Any(static result => result));
        Assert.IsTrue(fallbackResults.Any(static result => !result));

        var toArray = enumerable.GetMembers("ToArray").OfType<IMethodSymbol>().Single();
        var skip = enumerable.GetMembers("Skip").OfType<IMethodSymbol>().Single();
        var invocation = block.DescendantsAndSelf().OfType<IInvocationOperation>()
            .Single(static operation => operation.TargetMethod.Name == "ToArray");
        var compileArrayLike = typeof(SemanticWalker).GetMethod(
            "CompileEnumerableArrayLike",
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
            binder: null,
            [typeof(ISymbol), typeof(SenseArgument), typeof(Expression), typeof(Expression[]), typeof(IOperation)],
            modifiers: null)!;
        Assert.Throws<TargetInvocationException>(() => compileArrayLike.Invoke(
            walker,
            [skip, new SenseArgument(), null, Array.Empty<Expression>(), invocation]));
        _ = toArray;

        var writerType = typeof(SourceMapEmitter).GetNestedType("TrackingStringWriter", BindingFlags.NonPublic)!;
        var writer = Activator.CreateInstance(
            writerType,
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
            binder: null,
            args: null,
            culture: null)!;
        writerType.GetMethod("Write", [typeof(char)])!.Invoke(writer, ['\r']);
        writerType.GetMethod("Write", [typeof(char)])!.Invoke(writer, ['\n']);
        writerType.GetMethod("Write", [typeof(string)])!.Invoke(writer, [null]);
        writerType.GetMethod("Write", [typeof(string)])!.Invoke(writer, ["tail"]);
    }

    [TestMethod]
    public void SemanticWalkerCoverage98OperationMatrix_PreservesOrRejectsComplexCSharpShapesExplicitly()
    {
        var compilation = CreateCompilation(
            """
            using System;
            using System.Collections.Generic;

            public sealed class MatrixHost
            {
                public int Field;
                public int Property { get; set; }
                public MatrixHost Child { get; } = new();

                public void Deconstruct(out int first, out int second)
                {
                    first = Field;
                    second = Property;
                }

                private static MatrixHost CreateHost() => new();

                private static T CreateGeneric<T>() where T : new() => new T();

                private void TestMethod(MatrixHost? candidate, int[] values, object value, int seed)
                {
                    var exact = values is [1, .. var middle, 3];
                    var empty = values is [];
                    var all = values is [.. var copied];
                    var chars = "ok" is ['o', 'k'];
                    var recursive = candidate is { Property: > 0, Child: { Field: >= 0 } };
                    var interfaceMatch = value is IComparable;
                    var optional = candidate?.Property;
                    var fromEnd = values[^1];
                    var range = values[1..^1];

                    var initialized = new MatrixHost
                    {
                        Field = seed,
                        Property = seed + 1,
                        Child = { Field = CreateHost().Field, Property = seed + 2 }
                    };
                    var list = new List<int> { seed, seed + 1 };
                    var map = new Dictionary<string, int> { ["seed"] = seed };

                    (Field, Property) = (seed, seed + 1);
                    var (first, second) = initialized;
                    (Field, Property) = initialized;
                    var generic = CreateGeneric<MatrixHost>();
                    Property += seed;
                    Property++;
                    _ = (exact, empty, all, chars, recursive, interfaceMatch, optional, fromEnd, range,
                        list, map, first, second, generic);
                }
            }
            """);
        var block = CreateBlockFromCompilation(compilation, "MatrixHost", "TestMethod");
        var translated = 0;
        var rejected = 0;

        foreach (var operation in block.DescendantsAndSelf().Where(static operation => operation is not IBlockOperation))
        {
            try
            {
                var node = new SemanticWalker(true).Visit(operation, new SenseArgument());
                if (node is not null)
                {
                    _ = node.ToKnRECMAScript();
                    translated++;
                }
            }
            catch (OperationTransformationException)
            {
                rejected++;
            }
            catch (NotSupportedException)
            {
                rejected++;
            }
            catch (InvalidOperationException)
            {
                rejected++;
            }
        }

        Assert.IsGreaterThan(35, translated);
        Assert.IsGreaterThan(0, rejected);
    }

    [TestMethod]
    public void SemanticWalkerCoverage98PrivateHelpers_ResolvePatternSourcesAndStructuralTargets()
    {
        var compilation = CreateCompilation(
            """
            using System;

            public interface IContract { }
            public sealed class ImplementsContract : IContract { }
            public sealed class PlainType { }
            public sealed class StructuralTarget
            {
                public StructuralTarget(int value) { }
            }

            public sealed class PatternHelperHost
            {
                private void TestMethod(object value, ImplementsContract implementation)
                {
                    var stable = implementation;
                    var reassigned = implementation;
                    reassigned = new ImplementsContract();
                    _ = stable is IContract;
                    _ = reassigned is IContract;
                    _ = value as string;
                    _ = value switch { IContract => 1, _ => 0 };
                }

                private static void GenericConstraint<T>() where T : IContract { }
            }
            """);
        var host = compilation.GetTypeByMetadataName("PatternHelperHost")!;
        var contract = compilation.GetTypeByMetadataName("IContract")!;
        var implements = compilation.GetTypeByMetadataName("ImplementsContract")!;
        var plain = compilation.GetTypeByMetadataName("PlainType")!;
        var structural = compilation.GetTypeByMetadataName("StructuralTarget")!;
        var block = CreateBlockFromCompilation(compilation, "PatternHelperHost", "TestMethod");
        var walker = new SemanticWalker(true);

        var assignable = GetPrivateStatic(
            typeof(SemanticWalker),
            "IsRuntimeTypeAssignableToInterface",
            typeof(ITypeSymbol),
            typeof(ITypeSymbol));
        Assert.IsTrue((bool)assignable.Invoke(null, [implements, contract])!);
        Assert.IsFalse((bool)assignable.Invoke(null, [plain, contract])!);
        var genericParameter = host.GetMembers("GenericConstraint")
            .OfType<IMethodSymbol>()
            .Single()
            .TypeParameters[0];
        Assert.IsTrue((bool)assignable.Invoke(null, [genericParameter, contract])!);

        var resolveSource = GetPrivateStatic(typeof(SemanticWalker), "ResolveIsTypeSourceOperation", typeof(IOperation));
        var typeChecks = block.DescendantsAndSelf().OfType<IIsTypeOperation>().ToArray();
        Assert.AreEqual(2, typeChecks.Length);
        foreach (var typeCheck in typeChecks)
            Assert.IsNotNull(resolveSource.Invoke(null, [typeCheck]));
        var tryCast = block.DescendantsAndSelf().OfType<IConversionOperation>()
            .Single(static operation => operation.IsTryCast);
        Assert.IsNotNull(resolveSource.Invoke(null, [tryCast]));
        var switchPattern = block.DescendantsAndSelf().OfType<ITypePatternOperation>()
            .Single(static operation => operation.MatchedType?.Name == "IContract" && operation.Parent is not IIsPatternOperation);
        Assert.IsNotNull(resolveSource.Invoke(null, [switchPattern]));

        var resolveInitializer = GetPrivateStatic(
            typeof(SemanticWalker),
            "TryResolveSingleAssignmentLocalInitializer",
            typeof(ILocalReferenceOperation),
            typeof(IOperation),
            typeof(IOperation).MakeByRefType());
        foreach (var typeCheck in typeChecks)
        {
            var local = Assert.IsInstanceOfType<ILocalReferenceOperation>(typeCheck.ValueOperand);
            var arguments = new object?[] { local, typeCheck, null };
            var resolved = (bool)resolveInitializer.Invoke(null, arguments)!;
            Assert.AreEqual(local.Local.Name == "stable", resolved);
        }

        var pureChain = GetPrivateStatic(typeof(SemanticWalker), "IsPurePropertyAccessChain", typeof(Expression));
        Assert.IsTrue((bool)pureChain.Invoke(null, [new Identifier("value")])!);
        Assert.IsTrue((bool)pureChain.Invoke(null, [
            new MemberExpression(new Identifier("value"), new Identifier("member"), computed: false, optional: false)])!);
        Assert.IsTrue((bool)pureChain.Invoke(null, [
            new MemberExpression(new Identifier("value"), new NumericLiteral(0, "0"), computed: true, optional: false)])!);
        Assert.IsFalse((bool)pureChain.Invoke(null, [
            new MemberExpression(new Identifier("value"), new Identifier("member"), computed: false, optional: true)])!);
        Assert.IsFalse((bool)pureChain.Invoke(null, [
            new MemberExpression(new Identifier("value"), new CallExpression(new Identifier("key"), NodeList.Empty<Expression>(), false), computed: true, optional: false)])!);

        var structuralProperty = GetPrivateInstance(
            typeof(SemanticWalker),
            "TryGetStructuralRuntimeProperty",
            typeof(INamedTypeSymbol),
            typeof(int),
            typeof(string).MakeByRefType(),
            typeof(ITypeSymbol).MakeByRefType());
        var first = new object?[] { structural, 0, null, null };
        Assert.IsTrue((bool)structuralProperty.Invoke(walker, first)!);
        Assert.AreEqual("value", first[2]);
        var absent = new object?[] { structural, 1, null, null };
        Assert.IsFalse((bool)structuralProperty.Invoke(walker, absent)!);
    }

    [TestMethod]
    public void SemanticWalkerAuthoringShapes_CoversEnumerableAndCompoundIndexedMutation()
    {
        var block = CreateBlock(
            """
            using System.Collections.Generic;
            using System.Linq;

            public sealed class TestClass
            {
                public int[] Values { get; set; } = new[] { 1, 2 };
                public int Position { get; set; }

                private TestClass GetSelf() => this;

                void TestMethod(IEnumerable<int> values, List<int> list, int[] array)
                {
                    var fromEnumerable = values.ToArray();
                    var listCopy = values.ToList();
                    var listArray = list.ToArray();
                    var arrayList = array.ToList();
                    Values[GetSelf().Position] += fromEnumerable.Length;
                    GetSelf().Values[Position] += listCopy.Count;
                }
            }
            """);

        var script = new SemanticWalker(true).Visit(block, new SenseArgument())?.ToKnRECMAScript();
        Assert.IsNotNull(script);
        StringAssert.Contains(script, "Array.from", StringComparison.Ordinal);
        StringAssert.Contains(script, "MarkAsMutableListCarrier", StringComparison.Ordinal);
        _ = new Parser().ParseScript(script!);
    }

    [TestMethod]
    public void SemanticWalkerAuthoringShapes_CoversNestedPropertyPatternsAndInitializers()
    {
        var block = CreateBlock(
            """
            namespace ECMAScript
            {
                [global::System.AttributeUsage(global::System.AttributeTargets.Class)]
                public sealed class ECMAScriptAttribute : global::System.Attribute { }
            }

            [ECMAScript.ECMAScript]
            public sealed class Node
            {
                public int Value { get; set; }
                public Node? Child { get; set; }
            }

            [ECMAScript.ECMAScript]
            public sealed class Holder
            {
                public Node Child { get; set; } = new();
                public Node[] Nodes { get; set; } = new[] { new Node() };
            }

            public sealed class TestClass
            {
                void TestMethod(Node? node)
                {
                    var holder = new Holder { Child = { Value = 1 }, Nodes = { [0] = { Value = 2 } } };
                    var matches = node is { Child: { Value: 1 } };
                    var matchesValue = node is { Value: > 0 };
                }
            }
            """);

        var script = new SemanticWalker(true).Visit(block, new SenseArgument())?.ToKnRECMAScript();
        Assert.IsNotNull(script);
        StringAssert.Contains(script, "v$0.Child.Value = 1", StringComparison.Ordinal);
        StringAssert.Contains(script, "v$0.Nodes[0].Value = 2", StringComparison.Ordinal);
        StringAssert.Contains(script, "\"Child\" in node", StringComparison.Ordinal);
        StringAssert.Contains(script, "\"Value\" in node", StringComparison.Ordinal);
        _ = new Parser().ParseScript(script!);
    }

    private static IBlockOperation CreateBlock(string source)
    {
        var compilation = CreateCompilation(source);
        return CreateBlockFromCompilation(compilation, null, "TestMethod");
    }

    private static IBlockOperation CreateBlockFromCompilation(CSharpCompilation compilation, string? containingTypeName, string methodName)
    {
        var syntaxTree = compilation.SyntaxTrees.Single();
        var method = syntaxTree.GetRoot()
            .DescendantNodes()
            .OfType<MethodDeclarationSyntax>()
            .Single(declaration =>
                declaration.Identifier.ValueText == methodName &&
                (containingTypeName is null || declaration.Parent is ClassDeclarationSyntax type && type.Identifier.ValueText == containingTypeName));
        return Assert.IsInstanceOfType<IBlockOperation>(
            compilation.GetSemanticModel(syntaxTree).GetOperation(method.Body!));
    }

    private static CSharpCompilation CreateCompilation(string source)
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(source, TestMetadataReferences.PreviewParseOptions);
        var compilation = CSharpCompilation.Create(
            "CompilerCoverage98Boundary_" + Guid.NewGuid().ToString("N"),
            [syntaxTree],
            TestMetadataReferences.Net11,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        var errors = compilation.GetDiagnostics()
            .Where(static diagnostic => diagnostic.Severity == DiagnosticSeverity.Error)
            .ToArray();
        Assert.IsEmpty(errors, string.Join(Environment.NewLine, errors.Select(static error => error.ToString())));
        return compilation;
    }

    private static MethodInfo GetPrivateStatic(Type owner, string name, params Type[] parameterTypes)
        => owner.GetMethod(name, BindingFlags.Static | BindingFlags.NonPublic, binder: null, parameterTypes, modifiers: null)
            ?? throw new MissingMethodException(owner.FullName, name);

    private static MethodInfo GetPrivateInstance(Type owner, string name, params Type[] parameterTypes)
        => owner.GetMethod(name, BindingFlags.Instance | BindingFlags.NonPublic, binder: null, parameterTypes, modifiers: null)
            ?? throw new MissingMethodException(owner.FullName, name);

    private sealed class EmptySequenceInvocationHost : SemanticWalkerHost
    {
        public override Expression? RewriteInvocationPreorder(IInvocationOperation operation, SenseArgument argument)
            => new SequenceExpression(NodeList.Empty<Expression>());
    }

    private sealed class ArgumentRewriteHost : SemanticWalkerHost
    {
        public override Expression? RewriteInvocationArgumentPreorder(
            IInvocationOperation operation,
            IArgumentOperation argument,
            int argumentIndex,
            SenseArgument context)
            => argumentIndex == 0 ? new Identifier("rewritten") : null;
    }

    private sealed class IncludeNestedTypePolicy : AstConverterModulePolicy
    {
        public override IEnumerable<INamedTypeSymbol> EnumerateModuleTypes(INamedTypeSymbol moduleType)
        {
            yield return moduleType;
            foreach (var nested in moduleType.GetTypeMembers())
                yield return nested;
        }
    }
}
