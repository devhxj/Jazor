using ECMAScript.Contract;
using Jazor.Common;
using Jazor.Compiler;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

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

            if (typeOp != nameof(Op.Compile))
            {
                var memberName = string.IsNullOrEmpty(typeMemberName) ? typeName : typeMemberName;
                if (seenTypes.Add(memberName))
                    types.Add((typeOp, memberName, typeValue, modulePath));
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

    Console.WriteLine($"whitelist attributed types syntax={attributedTypeSyntaxCount}, symbols={attributedTypeSymbolCount}, types={types.Count}, members={members.Count}");

    var jazorCompilerDir = Path.Combine(repoRoot, "src", "Jazor.Compiler");
    var typesInit = string.Join(Split, types
        .OrderBy(x => x.Op)
        .Select(n => $"types[\"{n.Member}\"] = new(Op.{n.Op}{FormatValue(n.Op, n.Value)});"));
    var membersInit = string.Join(Split, members
        .Where(x => x.Op != nameof(Op.Compile))
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
        types.Select(static entry => new KeyValuePair<string, WhiteListValue>(
            entry.Member,
            CreateWhiteListValue(entry.Op, entry.Value, entry.ModulePath))),
        members
            .Where(static entry => entry.Op != nameof(Op.Compile))
            .Select(static entry => new KeyValuePair<string, WhiteListValue>(
                entry.Member,
                CreateWhiteListValue(entry.Op, entry.Value, entry.ModulePath))));
}

static string FormatValue(string op, string? str)
    => op == nameof(Op.Allowed) || str is null ? "" : $", \"{SharedGeneration.EscapeForCSharpStringLiteral(str)}\"";

static string FormatModulePath(string op, string? str)
    => op != nameof(Op.Import) || str is null ? "" : $", \"{SharedGeneration.EscapeForCSharpStringLiteral(str)}\"";

static WhiteListValue CreateWhiteListValue(string opName, string? value, string? modulePath)
{
    var op = Enum.Parse<Op>(opName, ignoreCase: false);
    return op switch
    {
        Op.Allowed => new WhiteListValue(op),
        Op.Import => new WhiteListValue(op, value, modulePath),
        _ => new WhiteListValue(op, value)
    };
}
