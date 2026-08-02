using ECMAScript.Contract;
using Jazor.Common;
using Jazor.Compiler;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

// 生成器是仓库源码到编译器消费产物的单向刷新入口：先生成白名单，再生成 CLR runtime catalog。
// 生成文件属于产物，新增或修改映射必须回到 ECMAScript/Jazor.CLR 源码中的属性声明。
const string Split = $@"
		";

var repoRoot = SharedGeneration.FindRepositoryRoot();
var references = Basic.Reference.Assemblies.Net110.References.All
    .Add(MetadataReference.CreateFromFile(typeof(JazorAttribute).Assembly.Location));

GenerateWhiteListArtifacts(repoRoot, references);
ClrRuntimeCatalogEmitter.Generate(repoRoot, references);

Console.WriteLine("生成完成");

static void GenerateWhiteListArtifacts(string repoRoot, IEnumerable<MetadataReference> references)
{
    // 白名单扫描使用源码和 Roslyn symbol，而不是反射；这样生成结果与源声明、OriginalDefinition
    // 格式保持一致，也能在生成阶段发现无法解析的声明。
    var sourceFiles = SharedGeneration.GetWhiteListSourceRoots(repoRoot)
        .SelectMany(SharedGeneration.GetSourceFiles)
        .OrderBy(path => path, StringComparer.Ordinal)
        .ToArray();
    Console.WriteLine($"whitelist sourceFiles={sourceFiles.Length}");
    var syntaxTrees = sourceFiles
        .Select(SharedGeneration.CreateSyntaxTree)
        .ToArray();
    var compilation = CSharpCompilation.Create(
        "Jazor.SourceScan",
        syntaxTrees,
        [.. references],
        new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
    var types = new List<(string Op, string Member, string? Value, string? ModulePath)>();
    var members = new List<(string TypeName, string Op, string Member, string? Value, string? ModulePath)>();
    var adapterMethods = new List<(string TargetType, string TargetMember, IMethodSymbol Implementation)>();
    var seenTypes = new HashSet<string>(StringComparer.Ordinal);
    var seenMembers = new HashSet<string>(StringComparer.Ordinal);
    var attributedTypeSyntaxCount = 0;
    var attributedTypeSymbolCount = 0;

    foreach (var syntaxTree in syntaxTrees)
    {
        var semanticModel = compilation.GetSemanticModel(syntaxTree, ignoreAccessibility: true);
        var root = syntaxTree.GetRoot();
        foreach (var typeDeclaration in root.DescendantNodes().OfType<TypeDeclarationSyntax>())
        {
            var jazorAttr = SharedGeneration.FindAttribute(typeDeclaration.AttributeLists, "Jazor");
            if (jazorAttr is null)
                continue;

            attributedTypeSyntaxCount++;

            if (semanticModel.GetDeclaredSymbol(typeDeclaration) is not INamedTypeSymbol type)
            {
                Console.WriteLine($"whitelist missing symbol: {typeDeclaration.Identifier.ValueText} @ {syntaxTree.FilePath}");
                continue;
            }

            attributedTypeSymbolCount++;

            var (typeOp, typeMemberName, typeValue) = SharedGeneration.ReadJazorAttribute(jazorAttr, semanticModel);
            if (typeOp == nameof(Op.Discard))
                continue;

            var typeName = type.OriginalDefinition.ToDisplayString(Format.NameFormat);
            var modulePath = SharedGeneration.ReadModulePath(typeDeclaration.AttributeLists, semanticModel);
            var mappedTypeName = string.IsNullOrEmpty(typeMemberName) ? typeName : typeMemberName;

            if (typeOp != nameof(Op.Compile))
            {
                if (seenTypes.Add(mappedTypeName))
                    types.Add((typeOp, mappedTypeName, typeValue, modulePath));
            }

            foreach (var memberDeclaration in typeDeclaration.Members)
            {
                var attr = SharedGeneration.FindAttribute(memberDeclaration.AttributeLists, "Jazor");
                if (attr is null)
                    continue;

                var member = SharedGeneration.GetDeclaredSymbol(memberDeclaration, semanticModel);
                if (member is null)
                    continue;

                var (op, memberName, value) = SharedGeneration.ReadJazorAttribute(attr, semanticModel);
                if (op == nameof(Op.Discard))
                    continue;

                if ((op == nameof(Op.Compile) || op == nameof(Op.Inline)) && string.IsNullOrEmpty(memberName))
                {
                    memberName = member.OriginalDefinition.ToDisplayString(Format.NameFormat);
                    value ??= Format.HashName(memberName);
                }

                if (member.Kind == SymbolKind.Method && memberName is not null && value is null)
                    value = member.Name;

                if (member is IMethodSymbol implementationMethod &&
                    memberName is not null &&
                    type.IsStatic &&
                    type.ContainingNamespace?.ToDisplayString() == "Jazor.CLR")
                {
                    adapterMethods.Add((mappedTypeName, memberName, implementationMethod));
                }

                if (member is IPropertySymbol property)
                {
                    if (property.GetMethod is not null)
                    {
                        var getMemberName = property.GetMethod.OriginalDefinition.ToDisplayString(Format.NameFormat);
                        if (seenMembers.Add(getMemberName))
                            members.Add((typeName, op, getMemberName, value, modulePath));
                    }

                    if (property.SetMethod is not null)
                    {
                        var setMemberName = property.SetMethod.OriginalDefinition.ToDisplayString(Format.NameFormat);
                        if (seenMembers.Add(setMemberName))
                            members.Add((typeName, op, setMemberName, value, modulePath));
                    }
                }
                else if (memberName is not null && seenMembers.Add(memberName))
                {
                    members.Add((typeName, op, memberName, value, modulePath));
                }
            }
        }
    }

    var runtimeValueCarriers = InferRuntimeValueCarriers(
        compilation,
        types.Select(static entry => entry.Member).ToHashSet(StringComparer.Ordinal),
        adapterMethods);

    Console.WriteLine($"whitelist attributed types syntax={attributedTypeSyntaxCount}, symbols={attributedTypeSymbolCount}, types={types.Count}, members={members.Count}, runtimeCarriers={runtimeValueCarriers.Count}");

    var jazorCompilerDir = Path.Combine(repoRoot, "src", "Jazor.Compiler");
    var typesInit = string.Join(Split, types
        .OrderBy(x => x.Op)
        .Select(n => $"types[\"{n.Member}\"] = new(Op.{n.Op}{FormatValue(n.Op, n.Value)}{FormatRuntimeValueCarrier(runtimeValueCarriers.GetValueOrDefault(n.Member))});"));
    // The catalog is shared by the analyzer and compiler. Compile mappings additionally
    // generate dispatch slots below, but remain normal supported member entries here.
    var membersInit = string.Join(Split, members
        .Select(n => $"members[\"{n.Member}\"] = new(Op.{n.Op}{FormatValue(n.Op, n.Value)}{FormatModulePath(n.Op, n.ModulePath)});"));
    var compilesInit = string.Join("", members
        .Where(x => x.Op == nameof(Op.Compile))
        .GroupBy(x => x.Value, StringComparer.Ordinal)
        .Select(x => x.First())
        .Select(n => $@"
	/// <summary>
	/// {n.Member}
	/// </summary>
	/// <param name=""symbol""></param>
	/// <param name=""context""></param>
	/// <param name=""handler""></param>
	/// <param name=""args""></param>
	/// <param name=""originOperation""></param>
	/// <returns></returns>
	Expression? Compile{n.Value}(ISymbol symbol, SenseArgument context, Expression? handler, Expression?[] args, IOperation? originOperation);
"));
    var funssInit = string.Join(Environment.NewLine, members
        .Where(x => x.Op == nameof(Op.Compile))
        .Select(n => $"\t\tfuncs[\"{n.Member}\"] = Compile{n.Value};"));

    File.WriteAllText(
        Path.Combine(jazorCompilerDir, "WhiteList.cs.Generate.cs"),
        $@"// <auto-generated/>
using ECMAScript.Contract;
using System.Collections.Generic;

namespace Jazor.Compiler;

internal static partial class WhiteList
{{
	static partial void Generate(ref Dictionary<string, WhiteListValue> types, ref Dictionary<string, WhiteListValue> members)
	{{
		// 初始化类型
		{typesInit}
		
		// 初始化成员
		{membersInit}
	}}
}}
");

    File.WriteAllText(
        Path.Combine(jazorCompilerDir, "WhiteList.cs.Compile.cs"),
        $@"// <auto-generated/>
#nullable enable
using Acornima.Ast;
using ECMAScript.Contract;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Operations;

namespace Jazor.Compiler;

partial interface IWhiteList
{{{compilesInit}
}}
");

    File.WriteAllText(
        Path.Combine(jazorCompilerDir, "core", "SemanticWalker.cs.Generate.cs"),
        $@"// <auto-generated/>
#nullable enable
using Acornima;
using Acornima.Ast;
using System;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;
using System.Collections.Generic;
using System.Linq;

namespace Jazor.Compiler;

public partial class SemanticWalker
{{
	partial void Generate(ref Dictionary<string, Func<ISymbol, SenseArgument, Expression?, Expression?[], IOperation?, Expression?>> funcs)
	{{
		// 初始白名单特殊编译处理
{funssInit}
	}}
}}
");

    WhiteList.ReplaceForCurrentProcess(
        types.Select(entry => new KeyValuePair<string, WhiteListValue>(
            entry.Member,
            CreateWhiteListValue(entry.Op, entry.Value, entry.ModulePath, runtimeValueCarriers.GetValueOrDefault(entry.Member)))),
        members
            .Select(static entry => new KeyValuePair<string, WhiteListValue>(
                entry.Member,
                CreateWhiteListValue(entry.Op, entry.Value, entry.ModulePath))));
}

static string FormatValue(string op, string? str)
    => op == nameof(Op.Allowed) || str is null ? "" : $", \"{SharedGeneration.EscapeForCSharpStringLiteral(str)}\"";

static string FormatModulePath(string op, string? str)
    => op != nameof(Op.Import) || str is null ? "" : $", \"{SharedGeneration.EscapeForCSharpStringLiteral(str)}\"";

static string FormatRuntimeValueCarrier(RuntimeValueCarrierReference? carrier)
    => carrier is null
        ? ""
        : $", null, new(\"{SharedGeneration.EscapeForCSharpStringLiteral(carrier.Name)}\", \"{SharedGeneration.EscapeForCSharpStringLiteral(carrier.Path)}\")";

static WhiteListValue CreateWhiteListValue(
    string opName,
    string? value,
    string? modulePath,
    RuntimeValueCarrierReference? runtimeValueCarrier = null)
{
    var op = Enum.Parse<Op>(opName, ignoreCase: false);
    if (runtimeValueCarrier is not null)
        return new WhiteListValue(op, value, path: null, runtimeValueCarrier);

    return op switch
    {
        Op.Allowed => new WhiteListValue(op),
        Op.Import => new WhiteListValue(op, value, modulePath),
        _ => new WhiteListValue(op, value)
    };
}

static Dictionary<string, RuntimeValueCarrierReference> InferRuntimeValueCarriers(
    Compilation compilation,
    HashSet<string> mappedTypes,
    IEnumerable<(string TargetType, string TargetMember, IMethodSymbol Implementation)> adapters)
{
    var targetTypes = ResolveTargetTypes(compilation, mappedTypes);
    var targetMethods = targetTypes.ToDictionary(
        static pair => pair.Key,
        static pair => BuildTargetMethodLookup(pair.Value),
        StringComparer.Ordinal);
    var carrierSymbols = new Dictionary<string, INamedTypeSymbol>(StringComparer.Ordinal);

    foreach (var adapter in adapters)
    {
        if (!targetMethods.TryGetValue(adapter.TargetType, out var methods) ||
            !methods.TryGetValue(adapter.TargetMember, out var targetMethod))
        {
            continue;
        }

        foreach (var pair in AlignAdapterTypes(targetMethod, adapter.Implementation))
        {
            var targetName = pair.Target.OriginalDefinition.ToDisplayString(Format.NameFormat);
            if (!mappedTypes.Contains(targetName) ||
                !TryGetInternalRuntimeValueCarrier(pair.Implementation, out var carrier))
            {
                continue;
            }

            var originalCarrier = carrier.OriginalDefinition;
            if (carrierSymbols.TryGetValue(targetName, out var existing))
            {
                if (!SymbolEqualityComparer.Default.Equals(existing, originalCarrier))
                {
                    throw new InvalidOperationException(
                        $"Jazor.CLR adapter signatures map '{targetName}' to both " +
                        $"'{existing.ToDisplayString(Format.NameFormat)}' and " +
                        $"'{originalCarrier.ToDisplayString(Format.NameFormat)}'.");
                }

                continue;
            }

            carrierSymbols.Add(targetName, originalCarrier);
        }
    }

    return carrierSymbols.ToDictionary(
        static pair => pair.Key,
        static pair => new RuntimeValueCarrierReference(
            Util.GetConfigOrSymbolName(pair.Value),
            GetRuntimeModulePath(pair.Value)),
        StringComparer.Ordinal);
}

static Dictionary<string, INamedTypeSymbol> ResolveTargetTypes(
    Compilation compilation,
    HashSet<string> targetNames)
{
    var result = new Dictionary<string, INamedTypeSymbol>(StringComparer.Ordinal);

    VisitNamespace(compilation.GlobalNamespace);
    return result;

    void VisitNamespace(INamespaceSymbol @namespace)
    {
        foreach (var type in @namespace.GetTypeMembers())
            VisitType(type);

        foreach (var child in @namespace.GetNamespaceMembers())
            VisitNamespace(child);
    }

    void VisitType(INamedTypeSymbol type)
    {
        var original = type.OriginalDefinition;
        var displayName = original.ToDisplayString(Format.NameFormat);
        if (targetNames.Contains(displayName))
            result.TryAdd(displayName, original);

        foreach (var nested in type.GetTypeMembers())
            VisitType(nested);
    }
}

static Dictionary<string, IMethodSymbol> BuildTargetMethodLookup(INamedTypeSymbol type)
{
    var result = new Dictionary<string, IMethodSymbol>(StringComparer.Ordinal);
    foreach (var member in type.GetMembers())
    {
        if (member is IMethodSymbol method)
            Add(method);
        else if (member is IPropertySymbol property)
        {
            if (property.GetMethod is not null)
                Add(property.GetMethod);
            if (property.SetMethod is not null)
                Add(property.SetMethod);
        }
        else if (member is IEventSymbol @event)
        {
            if (@event.AddMethod is not null)
                Add(@event.AddMethod);
            if (@event.RemoveMethod is not null)
                Add(@event.RemoveMethod);
        }
    }

    return result;

    void Add(IMethodSymbol method)
        => result.TryAdd(method.OriginalDefinition.ToDisplayString(Format.NameFormat), method.OriginalDefinition);
}

static IEnumerable<(ITypeSymbol Target, ITypeSymbol Implementation)> AlignAdapterTypes(
    IMethodSymbol target,
    IMethodSymbol implementation)
{
    var parameterOffset = 0;
    if (target.MethodKind == MethodKind.Constructor)
    {
        if (!implementation.ReturnsVoid)
            yield return (target.ContainingType, implementation.ReturnType);
    }
    else
    {
        if (!target.IsStatic && implementation.Parameters.Length > 0)
        {
            yield return (target.ContainingType, implementation.Parameters[0].Type);
            parameterOffset = 1;
        }

        if (!target.ReturnsVoid && !implementation.ReturnsVoid)
            yield return (target.ReturnType, implementation.ReturnType);
    }

    var parameterCount = Math.Min(target.Parameters.Length, implementation.Parameters.Length - parameterOffset);
    for (var index = 0; index < parameterCount; index++)
        yield return (target.Parameters[index].Type, implementation.Parameters[index + parameterOffset].Type);
}

static bool TryGetInternalRuntimeValueCarrier(
    ITypeSymbol implementationType,
    out INamedTypeSymbol carrier)
{
    carrier = implementationType as INamedTypeSymbol ?? null!;
    if (carrier is null ||
        carrier.TypeKind != TypeKind.Class ||
        carrier.IsStatic ||
        carrier.DeclaringSyntaxReferences.Length == 0 ||
        carrier.ContainingNamespace?.ToDisplayString() != "Jazor.CLR")
    {
        return false;
    }

    return TryGetRuntimeModulePath(carrier, out _);
}

static string GetRuntimeModulePath(INamedTypeSymbol carrier)
{
    if (TryGetRuntimeModulePath(carrier, out var path))
        return path;

    throw new InvalidOperationException(
        $"Internal runtime carrier '{carrier.ToDisplayString(Format.NameFormat)}' is not contained in an ECMAScript module.");
}

static bool TryGetRuntimeModulePath(INamedTypeSymbol carrier, out string path)
{
    for (ITypeSymbol? current = carrier; current is not null; current = current.ContainingType)
    {
        var modulePath = Util.GetECMAScriptModuleImportPath(current);
        if (!string.IsNullOrWhiteSpace(modulePath))
        {
            path = modulePath!;
            return true;
        }
    }

    path = string.Empty;
    return false;
}
