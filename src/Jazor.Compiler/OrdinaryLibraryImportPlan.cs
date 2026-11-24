using Acornima.Ast;
using ICSharpCode.Decompiler;
using ICSharpCode.Decompiler.CSharp;
using FullTypeName = ICSharpCode.Decompiler.TypeSystem.FullTypeName;
using ICSharpCode.Decompiler.Metadata;
using Jazor.Common;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;
using System.IO;
using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;
using System.Security.Cryptography;
using System.Text;
using System.Threading;

namespace Jazor.Compiler;

/// <summary>A locally resolved implementation selected for ordinary library compilation.</summary>
public sealed record LibraryImplementationReference(string AssemblyName, string ImplementationPath)
{
    /// <summary>Optional content identity supplied by the build's resolved implementation manifest.</summary>
    public string? ContentHash { get; init; }
}

/// <summary>
/// Owns recovered library bodies and the member closure shared by all roots of one compilation.
/// Consumer symbols remain bound to their original assemblies; recovered code never enters them.
/// </summary>
public sealed class OrdinaryLibraryImportPlan
{
    private sealed class RecoveredLibrary(
        IAssemblySymbol originalAssembly, CSharpCompilation compilation,
        string modulePath, string contentHash)
    {
        internal IAssemblySymbol OriginalAssembly { get; } = originalAssembly;
        internal CSharpCompilation Compilation { get; } = compilation;
        internal string ModulePath { get; } = modulePath;
        internal string ContentHash { get; } = contentHash;
        internal Dictionary<string, INamedTypeSymbol> Types { get; } = [];
        internal HashSet<ISymbol> Members { get; } = new(SymbolEqualityComparer.Default);
        internal HashSet<INamedTypeSymbol> RuntimeTypes { get; } = new(SymbolEqualityComparer.Default);
    }

    private readonly Compilation _consumer;
    private readonly ILookup<string, LibraryImplementationReference> _inputs;
    private readonly Dictionary<string, RecoveredLibrary> _libraries = new(StringComparer.Ordinal);
    private readonly Queue<ISymbol> _pending = new();
    private readonly CancellationToken _cancellationToken;
    private readonly RuntimeClassPrivateStorage _privateStorage;
    private readonly object _moduleGate = new();
    private IReadOnlyList<ImportedLibraryModule>? _modules;

    private OrdinaryLibraryImportPlan(Compilation consumer, IEnumerable<LibraryImplementationReference> references,
        RuntimeClassPrivateStorage privateStorage, CancellationToken cancellationToken)
    {
        _consumer = consumer;
        _inputs = references.Distinct().ToLookup(static reference => reference.AssemblyName, StringComparer.Ordinal);
        _cancellationToken = cancellationToken;
        _privateStorage = privateStorage;
    }

    /// <summary>Recover only members reached from the supplied, already bound source roots.</summary>
    public static OrdinaryLibraryImportPlan Create(Compilation compilation,
        IEnumerable<LibraryImplementationReference> references, IEnumerable<INamedTypeSymbol> roots,
        CancellationToken cancellationToken = default)
        => CreateCore(compilation, references, roots, RuntimeClassPrivateStorage.JavaScriptPrivateFields, cancellationToken);

    internal static OrdinaryLibraryImportPlan CreateCore(Compilation compilation,
        IEnumerable<LibraryImplementationReference> references, IEnumerable<INamedTypeSymbol> roots,
        RuntimeClassPrivateStorage privateStorage, CancellationToken cancellationToken)
    {
        var plan = new OrdinaryLibraryImportPlan(compilation, references, privateStorage, cancellationToken);
        foreach (var type in SourceTypeClosure.Collect(compilation, roots, cancellationToken: cancellationToken))
            foreach (var syntax in type.DeclaringSyntaxReferences)
                plan.CollectSyntax(compilation.GetSemanticModel(syntax.SyntaxTree), syntax.GetSyntax(cancellationToken));
        plan.Drain();
        plan.ValidateStaticInitialization();
        return plan;
    }

    /// <summary>Whether a concrete type has a selected, recovered implementation.</summary>
    public bool ContainsType(ITypeSymbol type)
        => type is INamedTypeSymbol named && TryGetLibrary(named, out var library) &&
           library.RuntimeTypes.Any(candidate => LibrarySymbolIdentity.TypeName(candidate) == LibrarySymbolIdentity.TypeName(named));

    /// <summary>Whether the exact bound member belongs to the imported implementation closure.</summary>
    public bool ContainsMember(ISymbol member)
        => TryGetRecoveredMember(member, out var recovered, out var library) && library.Members.Contains(recovered);

    internal bool TryGetTypeBinding(ITypeSymbol type, out string modulePath, out string exportName)
    {
        modulePath = exportName = string.Empty;
        if (type is not INamedTypeSymbol named || !ContainsType(named))
            return false;
        var library = _libraries[named.ContainingAssembly.Identity.ToString()];
        modulePath = library.ModulePath;
        exportName = GetTypeExportName(named);
        return true;
    }

    internal string GetMemberName(ISymbol member)
    {
        if (!TryGetRecoveredMember(member, out var recovered, out _))
            throw new NotSupportedException($"Member '{member}' has no recovered implementation binding.");
        return recovered is IMethodSymbol { AssociatedSymbol: IPropertySymbol { IsIndexer: true } } accessor
            ? Util.GetMemberIndexerAccessorHelperName(accessor)
            : Util.GetConfigOrSymbolName(recovered);
    }

    internal bool RequiresConstructorSelector(IMethodSymbol constructor)
        => TryGetLibrary(constructor, out var library) && library.Members.OfType<IMethodSymbol>()
            .Count(member => member.MethodKind == MethodKind.Constructor &&
                LibrarySymbolIdentity.TypeName(member.ContainingType) == LibrarySymbolIdentity.TypeName(constructor.ContainingType)) > 1;

    internal string GetConstructorSelector(IMethodSymbol constructor)
        => TryGetRecoveredMember(constructor, out var recovered, out _)
            ? Util.GetMemberConstructorHelperName((IMethodSymbol)recovered)
            : throw new NotSupportedException($"Constructor '{constructor}' has no recovered implementation binding.");

    private static string GetTypeExportName(INamedTypeSymbol type)
        => "$lib_" + Format.HashName(LibrarySymbolIdentity.TypeName(type.OriginalDefinition));

    private bool TryGetLibrary(ISymbol symbol, out RecoveredLibrary library)
        => _libraries.TryGetValue(symbol.ContainingAssembly.Identity.ToString(), out library!);

    private bool TryGetRecoveredMember(ISymbol member, out ISymbol recovered, out RecoveredLibrary library)
    {
        recovered = null!;
        library = null!;
        if (member.ContainingType is null || !TryGetLibrary(member, out library!) ||
            !library.Types.TryGetValue(LibrarySymbolIdentity.TypeName(member.ContainingType.OriginalDefinition), out var type))
            return false;
        var key = LibrarySymbolIdentity.Member(member);
        recovered = type.GetMembers().FirstOrDefault(candidate => LibrarySymbolIdentity.Member(candidate) == key)!;
        return recovered is not null;
    }

    private void Add(ISymbol? member)
    {
        if (member is null || member.ContainingType is null ||
            member.ContainingType.TypeKind == TypeKind.Delegate || member.ContainingType.IsRecord ||
            !_inputs.Contains(member.ContainingAssembly.Name) || HasExistingImplementation(member))
            return;
        member = member.OriginalDefinition;
        var library = GetOrRecover(member.ContainingAssembly);
        if (!TryGetRecoveredMember(member, out var recovered, out _))
            throw new NotSupportedException($"Cannot match library member '{member.ToDisplayString()}' in implementation '{library.OriginalAssembly.Identity}'.");
        if (library.Members.Add(recovered))
        {
            AddRuntimeType(library, recovered.ContainingType);
            _pending.Enqueue(recovered);
            if (recovered is IMethodSymbol method) IncludeOverrides(library, method);
        }
    }

    private static bool HasExistingImplementation(ISymbol member)
    {
        // Resolved assets are candidates, never a request to recover CLR shims or producer
        // artifacts. Existing host/whitelist/catalog contracts keep their dispatch priority.
        return Util.HasECMAScriptSupportMarker(member) ||
            Util.HasECMAScriptSupportMarkerBaseType(member.ContainingType) ||
            WhiteList.Types.ContainsKey(member.ContainingType.OriginalDefinition.ToDisplayString(Format.NameFormat)) ||
            member.ContainingAssembly.GetTypeByMetadataName("Jazor.Generated.ModuleCatalog") is not null;
    }

    private void IncludeOverrides(RecoveredLibrary library, IMethodSymbol method)
    {
        foreach (var type in library.RuntimeTypes.ToArray())
        foreach (var candidate in type.GetMembers().OfType<IMethodSymbol>())
        {
            for (var current = candidate.OverriddenMethod; current is not null; current = current.OverriddenMethod)
                if (SymbolEqualityComparer.Default.Equals(current.OriginalDefinition, method.OriginalDefinition))
                {
                    Add(candidate.AssociatedSymbol ?? candidate);
                    if (candidate.AssociatedSymbol is not null) Add(candidate);
                    break;
                }
        }
    }

    private void AddRuntimeType(RecoveredLibrary library, INamedTypeSymbol type)
    {
        if (!library.RuntimeTypes.Add(type.OriginalDefinition)) return;
        if (type.TypeKind != TypeKind.Class || type.IsRecord)
            throw new NotSupportedException($"Imported runtime type '{type}' is outside the ordinary class import slice.");
        // Field initializers execute even when the field is never read. Include storage and
        // initialization together; member pruning must not discard their observable effects.
        foreach (var field in type.GetMembers().OfType<IFieldSymbol>().Where(static field => !field.IsImplicitlyDeclared))
            if (library.Members.Add(field)) _pending.Enqueue(field);
        foreach (var constructor in type.StaticConstructors)
            if (library.Members.Add(constructor)) _pending.Enqueue(constructor);
        foreach (var property in type.GetMembers().OfType<IPropertySymbol>().Where(property =>
                     property.DeclaringSyntaxReferences.Any(reference => reference.GetSyntax() is PropertyDeclarationSyntax { Initializer: not null })))
            if (library.Members.Add(property)) _pending.Enqueue(property);
        if (type.BaseType is { SpecialType: not SpecialType.System_Object } baseType)
        {
            if (!SymbolEqualityComparer.Default.Equals(baseType.ContainingAssembly, library.Compilation.Assembly))
                throw new NotSupportedException($"Imported class '{type}' has unsupported external base '{baseType}'.");
            AddRuntimeType(library, baseType);
        }
        foreach (var method in library.Members.OfType<IMethodSymbol>().ToArray()) IncludeOverrides(library, method);
    }

    private void CollectSyntax(SemanticModel model, SyntaxNode syntax)
    {
        foreach (var node in syntax.DescendantNodesAndSelf())
        {
            _cancellationToken.ThrowIfCancellationRequested();
            // A nameof expression binds its operands for metadata only. Those
            // symbols are compile-time metadata, not calls into the component's CLR body.
            if (node.AncestorsAndSelf().Any(static ancestor => ancestor is AttributeSyntax or
                InvocationExpressionSyntax { Expression: IdentifierNameSyntax { Identifier.ValueText: "nameof" } })) continue;
            if (model.GetOperation(node, _cancellationToken) is not { } operation) continue;
            switch (operation)
            {
                case IInvocationOperation invocation: Add(invocation.TargetMethod.ReducedFrom ?? invocation.TargetMethod); break;
                case IObjectCreationOperation creation: Add(creation.Constructor); break;
                case IMethodReferenceOperation method: Add(method.Method); break;
                case IPropertyReferenceOperation property:
                    Add(property.Property);
                    var isWrite = property.Parent is ISimpleAssignmentOperation assignment && ReferenceEquals(assignment.Target, property);
                    var isReadWrite = property.Parent is ICompoundAssignmentOperation or IIncrementOrDecrementOperation;
                    if (!isWrite || isReadWrite) Add(property.Property.GetMethod);
                    if (isWrite || isReadWrite) Add(property.Property.SetMethod);
                    break;
                case IFieldReferenceOperation field when !field.Field.HasConstantValue: Add(field.Field); break;
            }
        }
    }

    private void Drain()
    {
        while (_pending.Count > 0)
        {
            _cancellationToken.ThrowIfCancellationRequested();
            var member = _pending.Dequeue();
            var library = _libraries[member.ContainingAssembly.Identity.ToString()];
            foreach (var reference in member.DeclaringSyntaxReferences)
            {
                var syntax = reference.GetSyntax(_cancellationToken);
                if (member is IPropertySymbol)
                {
                    if (syntax is not PropertyDeclarationSyntax { Initializer: { } initializer }) continue;
                    syntax = initializer;
                }
                var model = library.Compilation.GetSemanticModel(syntax.SyntaxTree);
                var error = model.GetDiagnostics(syntax.Span, _cancellationToken)
                    .FirstOrDefault(static diagnostic => diagnostic.Severity == DiagnosticSeverity.Error);
                if (error is not null)
                    throw new NotSupportedException($"Cannot bind recovered library member '{member}': {error}");
                CollectSyntax(model, syntax);
            }
        }
    }

    private void ValidateStaticInitialization()
    {
        foreach (var library in _libraries.Values)
        foreach (var member in library.Members.Where(static member => member.IsStatic && member is IFieldSymbol or IPropertySymbol))
        foreach (var reference in member.DeclaringSyntaxReferences)
        {
            var syntax = reference.GetSyntax(_cancellationToken);
            var initializer = syntax switch
            {
                VariableDeclaratorSyntax variable => variable.Initializer,
                PropertyDeclarationSyntax property => property.Initializer,
                _ => null
            };
            if (initializer is null) continue;
            var pending = new Queue<SyntaxNode>();
            pending.Enqueue(initializer);
            var visited = new HashSet<ISymbol>(SymbolEqualityComparer.Default) { member };
            while (pending.Count > 0)
            {
                var body = pending.Dequeue();
                var model = library.Compilation.GetSemanticModel(body.SyntaxTree);
                foreach (var node in body.DescendantNodesAndSelf())
                {
                    var target = model.GetOperation(node, _cancellationToken) switch
                    {
                        IInvocationOperation invocation => (ISymbol)invocation.TargetMethod,
                        IObjectCreationOperation creation => creation.Constructor,
                        IFieldReferenceOperation field when !field.Field.HasConstantValue => field.Field,
                        IPropertyReferenceOperation property => property.Property.GetMethod,
                        _ => null
                    };
                    if (target?.ContainingAssembly is null || !_inputs.Contains(target.ContainingAssembly.Name) ||
                        HasExistingImplementation(target)) continue;
                    if (!target.ContainingAssembly.Identity.Equals(member.ContainingAssembly.Identity) ||
                        LibrarySymbolIdentity.TypeName(target.ContainingType) != LibrarySymbolIdentity.TypeName(member.ContainingType))
                        throw new NotSupportedException($"Imported static initializer '{member}' depends on another library type '{target.ContainingType}'. Cross-type initialization requires a lazy type-initialization protocol and is not supported by this import slice.");
                    if (!visited.Add(target.OriginalDefinition)) continue;
                    foreach (var declaration in target.DeclaringSyntaxReferences)
                        pending.Enqueue(declaration.GetSyntax(_cancellationToken));
                }
            }
        }
    }

    private RecoveredLibrary GetOrRecover(IAssemblySymbol assembly)
    {
        if (_libraries.TryGetValue(assembly.Identity.ToString(), out var existing)) return existing;
        // Portable hosts may carry several RID implementations for backend-only assemblies.
        // Require an unambiguous asset only when the bound JS member needs recovery.
        var candidates = _inputs[assembly.Name];
        var count = candidates.Count();
        if (count != 1)
            throw new NotSupportedException($"Ordinary library '{assembly.Name}' requires exactly one resolved implementation asset for the selected TFM/RID; found {count}.");
        var input = candidates.Single();
        // These are explicit, already restored local build inputs; no assembly probing or
        // network recovery is allowed during analysis. The build manifest tracks content.
#pragma warning disable RS1035
        var bytes = File.ReadAllBytes(input.ImplementationPath);
#pragma warning restore RS1035
        using var sha = SHA256.Create();
        var contentHash = BitConverter.ToString(sha.ComputeHash(bytes)).Replace("-", "").ToLowerInvariant();
        if (input.ContentHash is { Length: > 0 } expectedHash &&
            !string.Equals(contentHash, expectedHash, StringComparison.OrdinalIgnoreCase))
            throw new NotSupportedException($"Library '{assembly.Identity}' changed after implementation inputs were prepared. Rebuild to refresh the import manifest.");
        var implementation = MetadataReference.CreateFromImage(bytes);
        var metadataCompilation = CSharpCompilation.Create("Jazor.Import.Metadata", references: [implementation]);
        var actualAssembly = (IAssemblySymbol)metadataCompilation.GetAssemblyOrModuleSymbol(implementation)!;
        if (!actualAssembly.Identity.Equals(assembly.Identity))
            throw new NotSupportedException($"Implementation '{input.ImplementationPath}' has identity '{actualAssembly.Identity}', expected '{assembly.Identity}'.");
        using var stream = new MemoryStream(bytes);
        using var pe = new PEReader(stream);
        var metadata = pe.GetMetadataReader();
        if (metadata.GetAssemblyDefinition().GetCustomAttributes().Any(handle => IsReferenceAssemblyAttribute(metadata, handle)))
            throw new NotSupportedException($"Library '{assembly.Identity}' has only a reference assembly; select its matching implementation asset.");
        var settings = new DecompilerSettings { ThrowOnAssemblyResolveErrors = false };
        using var decompilerImage = new PEFile(input.ImplementationPath, new MemoryStream(bytes), PEStreamOptions.PrefetchEntireImage);
        var decompiler = new CSharpDecompiler(decompilerImage, new LibraryAssemblyResolver(_consumer), settings) { CancellationToken = _cancellationToken };
        var trees = new List<SyntaxTree>();
        foreach (var handle in metadata.TypeDefinitions)
        {
            var type = metadata.GetTypeDefinition(handle);
            if (!type.GetDeclaringType().IsNil || metadata.GetString(type.Name) == "<Module>") continue;
            if (CSharpDecompiler.MemberIsHidden(decompilerImage, handle, settings)) continue;
            var name = metadata.GetString(type.Namespace);
            name = (name.Length == 0 ? "" : name + ".") + metadata.GetString(type.Name);
            var source = decompiler.DecompileTypeAsString(new FullTypeName(name));
            var path = "jazor-recovered/" + assembly.Name + "/" + name.Replace('`', '_') + ".cs";
            trees.Add(CSharpSyntaxTree.ParseText(source, CSharpParseOptions.Default.WithLanguageVersion(Microsoft.CodeAnalysis.CSharp.LanguageVersion.Preview), path, Encoding.UTF8, _cancellationToken));
        }
        // Use the same reference graph but do not replace the consumer's bound symbols. The
        // implementation compilation owns recovered bodies and preserves assembly identity.
        var references = _consumer.References.Where(reference =>
            _consumer.GetAssemblyOrModuleSymbol(reference) is not IAssemblySymbol candidate || !candidate.Identity.Equals(assembly.Identity));
        trees.Add(CSharpSyntaxTree.ParseText(
            "[assembly: System.Reflection.AssemblyVersion(\"" + assembly.Identity.Version + "\")]",
            CSharpParseOptions.Default.WithLanguageVersion(Microsoft.CodeAnalysis.CSharp.LanguageVersion.Preview),
            "jazor-recovered/" + assembly.Name + "/assembly-identity.cs"));
        var recovered = CSharpCompilation.Create(assembly.Name, trees, references,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, allowUnsafe: true,
                metadataImportOptions: MetadataImportOptions.All).WithCryptoPublicKey(assembly.Identity.PublicKey));
        var result = new RecoveredLibrary(assembly, recovered, "_jazor/libraries/" + assembly.Name + "-" + contentHash.Substring(0, 16) + ".mjs", contentHash);
        foreach (var type in EnumerateTypes(recovered.Assembly.GlobalNamespace))
            result.Types.Add(LibrarySymbolIdentity.TypeName(type), type);
        _libraries.Add(assembly.Identity.ToString(), result);
        return result;
    }

    private static IEnumerable<INamedTypeSymbol> EnumerateTypes(INamespaceOrTypeSymbol owner)
    {
        foreach (var member in owner.GetMembers())
        {
            if (member is INamedTypeSymbol type) yield return type;
            if (member is INamespaceOrTypeSymbol container)
                foreach (var child in EnumerateTypes(container)) yield return child;
        }
    }

    private static bool IsReferenceAssemblyAttribute(MetadataReader metadata, CustomAttributeHandle handle)
    {
        var attribute = metadata.GetCustomAttribute(handle);
        if (attribute.Constructor.Kind != HandleKind.MemberReference) return false;
        var constructor = metadata.GetMemberReference((MemberReferenceHandle)attribute.Constructor);
        return constructor.Parent.Kind == HandleKind.TypeReference &&
            metadata.GetString(metadata.GetTypeReference((TypeReferenceHandle)constructor.Parent).Name) == "ReferenceAssemblyAttribute";
    }

    /// <summary>Emit one shared module for every selected implementation assembly.</summary>
    public IReadOnlyList<ImportedLibraryModule> BuildModules(CancellationToken cancellationToken = default)
    {
        lock (_moduleGate)
            return _modules ??= BuildModulesCore(cancellationToken);
    }

    private IReadOnlyList<ImportedLibraryModule> BuildModulesCore(CancellationToken cancellationToken)
    {
        var result = new List<ImportedLibraryModule>();
        foreach (var library in _libraries.Values.OrderBy(static item => item.ModulePath, StringComparer.Ordinal))
        {
            var rootTree = CSharpSyntaxTree.ParseText("public static class __JazorLibraryRoot {}",
                CSharpParseOptions.Default.WithLanguageVersion(Microsoft.CodeAnalysis.CSharp.LanguageVersion.Preview), path: "jazor-recovered/root.cs");
            var compilation = library.Compilation.AddSyntaxTrees(rootTree);
            var root = compilation.GetTypeByMetadataName("__JazorLibraryRoot")!;
            // Symbols are rebound after adding a tree. Selection uses structural identity, not
            // reference equality, while the runtime constructor name is shared by every caller.
            var types = library.RuntimeTypes.Select(type => compilation.GetTypeByMetadataName(LibrarySymbolIdentity.TypeName(type))!)
                .OrderBy(static type => LibrarySymbolIdentity.TypeName(type), StringComparer.Ordinal).ToArray();
            var selected = new HashSet<string>(library.Members.Select(member => LibrarySymbolIdentity.TypeName(member.ContainingType) + ":" + LibrarySymbolIdentity.Member(member)), StringComparer.Ordinal);
            var names = types.ToDictionary<INamedTypeSymbol, ISymbol, string>(static type => type, GetTypeExportName, SymbolEqualityComparer.Default);
            var converter = new AstConverter(root, compilation.GetSemanticModel(rootTree),
                new AstConverterOptions(AstConverterProfile.Standard, DeclaredNames: names, CurrentModuleOutputPath: library.ModulePath,
                    RuntimeClassPrivateStorage: _privateStorage)
                {
                    LibraryImports = this,
                    RuntimeMemberFilter = member => selected.Contains(LibrarySymbolIdentity.TypeName(member.ContainingType) + ":" + LibrarySymbolIdentity.Member(member))
                });
            var module = converter.ConvertLibraryModule(types, cancellationToken);
            var artifact = module.ToKnRECMAScriptWithSourceMap(library.ModulePath, true,
                readSourceContent: path => compilation.SyntaxTrees.FirstOrDefault(tree => tree.FilePath == path)?.GetText(cancellationToken).ToString());
            result.Add(new ImportedLibraryModule(library.ModulePath, artifact, converter.ModuleCatalogImportPaths.ToArray(),
                converter.ProjectSourceImportKeys.Except(converter.ModuleCatalogImportPaths, StringComparer.Ordinal)
                    .Concat(module.Body.OfType<ImportDeclaration>().Select(static declaration => declaration.Source.Value)
                        .Where(static source => !source.StartsWith(".", StringComparison.Ordinal) && !source.StartsWith("/", StringComparison.Ordinal)))
                    .Distinct(StringComparer.Ordinal).OrderBy(static source => source, StringComparer.Ordinal).ToArray()));
        }
        return result;
    }
}

/// <summary>A shared module and accurate recovered-source debugging artifact.</summary>
public sealed record ImportedLibraryModule(string RelativePath, GeneratedJavaScriptArtifact Artifact, IReadOnlyList<string> Dependencies,
    IReadOnlyList<string> PackageImports);
