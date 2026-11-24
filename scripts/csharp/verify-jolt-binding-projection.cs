#:property AssemblyName=Jazor.NodeApiVerification
#:property CETCompat=false
#:property PublishAot=false
#:package Microsoft.CodeAnalysis.CSharp@5.12.0-1.26529.118
#:project ../../src/Jolt.Compiler/Jolt.Compiler.csproj

using System.Text.Json;
using Jazor.NodeHost;
using Jolt.Compiler;
using Jazor.Npm;
using Jolt.Npm;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;
using Microsoft.CodeAnalysis.Text;

var root = Path.GetFullPath(".");
var bindings = new[]
{
    (Library: "VueUse", Package: "@vueuse/core", Samples: new[] { "useCounter", "useMouse", "useOnline", "useStorage", "useVModel", "useUrlSearchParams", "eagerComputed", "invoke", "refThrottled", "throttledRef" }),
    (Library: "DateFns", Package: "date-fns", Samples: new[]
        { "addDays", "isBefore", "add", "sub", "formatDuration", "eachDayOfInterval", "clamp", "min", "max" }),
    (Library: "FloatingUi", Package: "@floating-ui/vue", Samples: new[] { "computePosition" }),
    (Library: "VeeValidate", Package: "vee-validate", Samples: new[] { "useIsFormDirty", "useSetFieldError", "useForm" }),
    (Library: "VueI18n", Package: "vue-i18n", Samples: new[] { "useI18n" }),
    (Library: "VueQuery", Package: "@tanstack/vue-query", Samples: new[] { "useIsFetching", "useQueryClient", "useQuery", "useMutation", "useInfiniteQuery" }),
    (Library: "Monaco", Package: "monaco-editor", Samples: new[] { "editor" })
};
var dependencies = new SortedDictionary<string, string>(StringComparer.Ordinal);
var fixture = Path.Combine(root, "scripts", "csharp", "fixtures", "jolt-binding-projection");
using var fixtureManifest = JsonDocument.Parse(File.ReadAllText(Path.Combine(fixture, "package.json")));
foreach (var binding in bindings)
{
    using var manifest = JsonDocument.Parse(File.ReadAllText(Path.Combine(root,
        "src", $"ECMAScript.{binding.Library}", "manifest.json")));
    var packages = manifest.RootElement.GetProperty("packages");
    foreach (var dependency in packages.EnumerateObject())
    {
        var expected = dependency.Value.GetProperty("version").GetString();
        var pinnedVersion = fixtureManifest.RootElement.GetProperty("dependencies");
        if (pinnedVersion.TryGetProperty(dependency.Name, out var actual))
            Require(actual.GetString() == expected, $"{dependency.Name} fixture version differs from the binding manifest.");
    }
    dependencies.Add(binding.Package, packages.GetProperty(binding.Package).GetProperty("version").GetString()!);
}

var workRoot = Directory.CreateDirectory(Path.Combine(root, ".tmp"));
var ownsProject = args.Length == 0;
var project = args switch
{
    [] => workRoot.CreateSubdirectory($"jolt-binding-projection-{Guid.NewGuid():N}").FullName,
    [var existing] => Path.GetFullPath(existing),
    _ => throw new ArgumentException("Usage: verify-jolt-binding-projection.cs [existing-project-directory]")
};
var inventoryFile = Path.Combine(project, $".binding-inventory-{Guid.NewGuid():N}.mjs");
try
{
    if (ownsProject)
    {
        foreach (var file in new[] { "package.json", "pnpm-lock.yaml" })
            File.Copy(Path.Combine(fixture, file), Path.Combine(project, file));
    }
    var document = Path.Combine(project, "Probe.jazor");
    var store = Environment.GetEnvironmentVariable("JazorPnpmStoreDirectory") ?? Path.Combine(workRoot.FullName, "nodeapi-store");
    var provider = new NpmVueMetadataProvider(store);
    File.Copy(Path.Combine(fixture, "inventory.mjs"), inventoryFile);
    await NodeToolchain.RestoreAsync(project, store, CancellationToken.None);
    var allProjected = new List<NpmValueExport>();
    var authoredCount = 0;
    var projectedAuthoredCount = 0;
    var authoredArityCount = 0;
    var projectedArityCount = 0;
    var overloadOperations = new Dictionary<string, SortedSet<string>>(StringComparer.Ordinal);
    foreach (var binding in bindings)
    {
        var bindingDirectory = Path.Combine(root, "src", $"ECMAScript.{binding.Library}");
        var authored = ReadAuthoredExports(Path.Combine(bindingDirectory, "Api"));
        var authoredShapes = ReadAuthoredShapes(Path.Combine(bindingDirectory, "Types"));
        var specifier = ReadModuleSpecifier(Path.Combine(bindingDirectory, binding.Library + ".cs"));
        var inventory = JsonSerializer.Deserialize<Dictionary<string, DeclarationExport>>(await NodeModuleRunner.RunAsync(
            project, inventoryFile, [specifier]), JsonSerializerOptions.Web)!;
        var candidates = await provider.ListValueExportsAsync(document, specifier);
        var matching = authored.Keys.Intersect(candidates, StringComparer.Ordinal).ToArray();
        var projected = (await provider.ReadValuesAsync(document, specifier,
            matching.Concat(binding.Samples).Distinct(StringComparer.Ordinal).ToArray()))
            .ToDictionary(value => value.ExportName, StringComparer.Ordinal);
        allProjected.AddRange(projected.Values);
        var callable = matching.Count(name => projected.GetValueOrDefault(name)?.Signatures.Count > 0);
        var constants = matching.Count(name => projected.GetValueOrDefault(name)?.ConstantType is not null &&
            inventory.GetValueOrDefault(name)?.Kind != "namespace");
        var namespaces = matching.Count(name => projected.ContainsKey(name) &&
            inventory.GetValueOrDefault(name)?.Kind == "namespace");
        authoredCount += authored.Count;
        projectedAuthoredCount += matching.Count(projected.ContainsKey);
        Console.WriteLine($"{specifier} ({binding.Package}@{dependencies[binding.Package]})");
        Console.WriteLine($"  {authored.Count} mapped top-level names, {matching.Length} projection candidates, " +
            $"{callable} projected callables, {constants} projected constants, {namespaces} projected namespaces");
        Console.WriteLine($"  Declaration exports {inventory.Count} " +
            string.Join(", ", inventory.Values.GroupBy(declaration => declaration.Kind).OrderBy(group => group.Key)
                .Select(group => $"{group.Count()} {group.Key}")));
        Console.WriteLine($"  Missing declaration exports [{string.Join(", ", authored.Keys.Except(inventory.Keys))}]");
        Console.WriteLine($"  Excluded declaration exports [{string.Join(", ", authored.Keys.Intersect(inventory.Keys).Except(matching)
            .Select(name => $"{name} ({inventory[name].Kind})"))}]");
        foreach (var (name, members) in authored)
        {
            if (inventory.GetValueOrDefault(name) is not { Kind: "namespace" } declaration)
                continue;
            var memberNames = members.Select(member => member.InlineTarget)
                .OfType<string>().Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal).ToArray();
            var declared = memberNames.Intersect(declaration.Members, StringComparer.Ordinal).ToArray();
            var projectedMembers = projected.GetValueOrDefault(name)?.ConstantType?.Members ?? [];
            var projectedCalls = declared.Intersect(projectedMembers.Where(member => member.Type.Signatures is { Count: > 0 })
                .Select(member => member.Name), StringComparer.Ordinal).ToArray();
            Console.WriteLine($"  {name} mapped namespace calls {declared.Length}/{memberNames.Length} declared " +
                $"[{string.Join(", ", declared)}], {projectedCalls.Length}/{memberNames.Length} projected " +
                $"[{string.Join(", ", projectedCalls)}], missing declarations [{string.Join(", ", memberNames.Except(declared))}]");
        }
        Console.WriteLine($"  Unsupported projections [{string.Join(", ", matching.Except(projected.Keys))}]");
        Require(authored.Keys.All(projected.ContainsKey), $"Missing reference exports in {specifier}: " +
            string.Join(", ", authored.Keys.Except(projected.Keys)));
        var (expectedArities, supportedArities) = PrintArityCoverage(specifier, authored, projected, inventory);
        authoredArityCount += expectedArities;
        projectedArityCount += supportedArities;
        overloadOperations.Add(specifier, PrintReturnMemberCoverage(specifier, Path.Combine(bindingDirectory, "Types"), authored, projected, inventory));
        foreach (var (name, members) in authored)
        {
            Console.WriteLine($"  {name}");
            foreach (var member in members)
                Console.WriteLine($"    Authored   {member.Location} {member.Description}");
            if (!projected.TryGetValue(name, out var value))
            {
                Console.WriteLine("    Gap        no generated export");
                continue;
            }

            Require(value.Version == dependencies[binding.Package], $"{name} must resolve to the manifest version.");
            var isNamespace = inventory.GetValueOrDefault(name)?.Kind == "namespace";
            PrintProjection(value, isNamespace);
            if (!isNamespace && value.Signatures.Count == 0 && members.Any(member => member.Callable))
                Console.WriteLine("    Shape mismatch: authored method maps to an upstream constant.");
            if (value.Signatures.Count > 0 && members.Any(member => !member.Callable))
                Console.WriteLine("    Shape mismatch: authored value maps to an upstream callable.");
        }

        Console.WriteLine($"  Authored supporting types and members ({authoredShapes.Length}, source inventory)");
        foreach (var shape in authoredShapes)
            Console.WriteLine($"    {shape}");

        foreach (var sample in binding.Samples)
        {
            var value = projected.GetValueOrDefault(sample) ?? await provider.ReadValueAsync(document, specifier, sample);
            Require(value.Version == dependencies[binding.Package], $"{sample} must resolve to the manifest version.");
            Console.WriteLine($"  Sample {sample}");
            PrintProjection(value, inventory.GetValueOrDefault(sample)?.Kind == "namespace");
        }

        if (binding.Library == "VueUse")
            Require(projected.GetValueOrDefault("isClient") is { Signatures: [], ConstantType.Kind: "boolean" },
                "VueUse isClient must project as a boolean constant.");
        if (binding.Library == "Monaco")
            Require(projected.GetValueOrDefault("editor")?.ConstantType?.Members?.Any(member =>
                member.Name == "createWebWorker" && member.Type.Signatures?.Any(signature =>
                    signature.Parameters is [{ Type.Members: { } options }] && options.Any(option =>
                        option is { Name: "worker", Type: { Kind: "web", Name: "Worker" } })) == true) == true,
                "Monaco editor.createWebWorker must accept the WebIDL Worker in its options.");
    }

    var floatingDom = await provider.ReadValuesAsync(document, "@floating-ui/dom", ["computePosition", "offset", "arrow", "autoUpdate"]);
    var frenchLocale = await provider.ReadValueAsync(document, "date-fns/locale", "fr");
    Console.WriteLine($"{projectedAuthoredCount}/{authoredCount} distinct top-level names from the seven handwritten bindings have a projection. Each name counts once across its overloads.");
    Console.WriteLine($"{projectedArityCount}/{authoredArityCount} distinct callable paths and positional argument counts have a projection. Roslyn sample bindings are reported separately.");
    VerifyGeneratedFacades(project, allProjected, [.. floatingDom, frenchLocale], overloadOperations);
}
finally
{
    File.Delete(inventoryFile);
    if (ownsProject)
        Directory.Delete(project, recursive: true);
}

static string ReadModuleSpecifier(string file) => CSharpSyntaxTree.ParseText(File.ReadAllText(file)).GetRoot()
    .DescendantNodes().OfType<AttributeSyntax>()
    .Where(attribute => attribute.Name.ToString() == "ECMAScript")
    .Select(attribute => (attribute.ArgumentList?.Arguments.Single().Expression as LiteralExpressionSyntax)?.Token.ValueText)
    .OfType<string>().Single();

static string[] ReadAuthoredShapes(string directory)
{
    var shapes = new List<string>();
    if (!Directory.Exists(directory))
        return [];
    foreach (var file in Directory.EnumerateFiles(directory, "*.cs", SearchOption.AllDirectories).Order(StringComparer.Ordinal))
    {
        var relative = Path.GetRelativePath(Directory.GetCurrentDirectory(), file);
        var root = CSharpSyntaxTree.ParseText(File.ReadAllText(file), path: file).GetRoot();
        foreach (var member in root.DescendantNodes().OfType<MemberDeclarationSyntax>())
        {
            // Delegates, enum values and positional union/record parameters are
            // part of what a handwritten caller can rely on as well.
            if (member.Ancestors().OfType<BaseTypeDeclarationSyntax>().Any(type => !type.Modifiers.Any(SyntaxKind.PublicKeyword)) ||
                !(member.Modifiers.Any(SyntaxKind.PublicKeyword) || member is EnumMemberDeclarationSyntax ||
                  member.Parent is InterfaceDeclarationSyntax && member.Modifiers.Count == 0))
                continue;
            var declaration = member.WithAttributeLists(default);
            var header = declaration switch
            {
                TypeDeclarationSyntax type => type.WithMembers(default).NormalizeWhitespace().ToString().Split('\n', 2)[0],
                EnumDeclarationSyntax enumeration => "enum " + enumeration.Identifier.ValueText,
                _ => declaration.NormalizeWhitespace().ToString().Split('\n', 2)[0],
            };
            var owner = member.Parent is BaseTypeDeclarationSyntax containing ? containing.Identifier.ValueText + "." : "";
            shapes.Add($"{relative}:{member.GetLocation().GetLineSpan().StartLinePosition.Line + 1} {owner}{header.Trim()}");
        }
    }
    return shapes.ToArray();
}

static SortedDictionary<string, List<AuthoredMember>> ReadAuthoredExports(string directory)
{
    var exports = new SortedDictionary<string, List<AuthoredMember>>(StringComparer.Ordinal);
    foreach (var file in Directory.EnumerateFiles(directory, "*.cs").Order(StringComparer.Ordinal))
    {
        var root = CSharpSyntaxTree.ParseText(File.ReadAllText(file), path: file).GetRoot();
        foreach (var member in root.DescendantNodes().OfType<MemberDeclarationSyntax>())
        {
            if (!member.Modifiers.Any(SyntaxKind.PublicKeyword) || !member.Modifiers.Any(SyntaxKind.StaticKeyword))
                continue;

            var export = member.AttributeLists.SelectMany(list => list.Attributes)
                .Where(attribute => attribute.Name.ToString() == "Description")
                .Select(attribute => (attribute.ArgumentList?.Arguments.Single().Expression as LiteralExpressionSyntax)?.Token.ValueText)
                .SingleOrDefault(description => description is { Length: > 2 } && description.StartsWith("@#", StringComparison.Ordinal));
            if (export is null)
                continue;

            var shape = member switch
            {
                MethodDeclarationSyntax method => $"method {method.ReturnType} {method.Identifier}{method.TypeParameterList}{method.ParameterList.NormalizeWhitespace()}",
                PropertyDeclarationSyntax property => $"property {property.Type} {property.Identifier}",
                FieldDeclarationSyntax field => $"field {field.Declaration}",
                _ => throw new InvalidOperationException($"Unexpected binding declaration {member.Kind()}.")
            };
            var name = export[2..];
            var inline = member.AttributeLists.SelectMany(list => list.Attributes)
                .Where(attribute => attribute.Name.ToString() == "ECMAScriptInline")
                .Select(attribute => (attribute.ArgumentList?.Arguments.Single().Expression as LiteralExpressionSyntax)?.Token.ValueText)
                .OfType<string>().SingleOrDefault();
            var inlineTarget = inline is null ? null : (SyntaxFactory.ParseExpression(inline) as InvocationExpressionSyntax)?.Expression
                as MemberAccessExpressionSyntax;
            if (!exports.TryGetValue(name, out var members))
                exports.Add(name, members = []);
            var location = $"{Path.GetRelativePath(Directory.GetCurrentDirectory(), file)}:{member.GetLocation().GetLineSpan().StartLinePosition.Line + 1}";
            var parameters = (member as MethodDeclarationSyntax)?.ParameterList.Parameters;
            var required = parameters?.Count(parameter => parameter.Default is null && !parameter.Modifiers.Any(SyntaxKind.ParamsKeyword));
            var variadic = parameters?.Any(parameter => parameter.Modifiers.Any(SyntaxKind.ParamsKeyword)) == true;
            var returnType = member switch
            {
                MethodDeclarationSyntax method => method.ReturnType,
                PropertyDeclarationSyntax property => property.Type,
                FieldDeclarationSyntax field => field.Declaration.Type,
                _ => null
            };
            members.Add(new(shape, member is MethodDeclarationSyntax, inlineTarget?.Expression.ToString() == name
                ? inlineTarget.Name.Identifier.ValueText : null, location, required, parameters?.Count, variadic, returnType));
        }
    }
    Require(exports.Count > 0, $"No authored npm imports found in {directory}.");
    return exports;
}

static (int Expected, int Supported) PrintArityCoverage(
    string specifier,
    SortedDictionary<string, List<AuthoredMember>> authored,
    Dictionary<string, NpmValueExport> projected,
    Dictionary<string, DeclarationExport> inventory)
{
    int expected = 0, supported = 0;
    var shapeDifferences = new List<string>();
    foreach (var (name, members) in authored)
    {
        var value = projected.GetValueOrDefault(name);
        var isNamespace = inventory.GetValueOrDefault(name)?.Kind == "namespace" ||
            members.Any(member => member.InlineTarget is not null);
        foreach (var target in members.Where(member => member.Callable && (!isNamespace || member.InlineTarget is not null))
            .GroupBy(member => member.InlineTarget))
        {
            var path = target.Key is { } child ? $"{name}.{child}" : name;
            var signatures = isNamespace
                ? value?.ConstantType?.Members?.FirstOrDefault(member => member.Name == target.Key)?.Type.Signatures
                : value?.Signatures;
            foreach (var member in target.Where(member => member.Variadic))
                Console.WriteLine($"  Variadic {path} at {member.Location}. Counts through {member.Parameters} only.");
            var arities = target.SelectMany(member =>
                Enumerable.Range(member.Required!.Value, member.Parameters!.Value - member.Required.Value + 1)
                    .Select(count => (Count: count, member.Location)))
                .GroupBy(call => call.Count).OrderBy(group => group.Key);
            foreach (var arity in arities)
            {
                expected++;
                if (signatures?.Any(signature => CoversUpstreamArity(signature, arity.Key)) == true)
                    supported++;
                else if (specifier == "@vueuse/core" && name is ("isClient" or "isIOS" or "isWorker") &&
                    !isNamespace && arity.Key == 0 && value is { ConstantType.Kind: "boolean", Signatures: [] } &&
                    inventory.GetValueOrDefault(name)?.Kind == "value")
                {
                    // Preserve the handwritten callable denominator even though the locked
                    // VueUse declarations re-export these values from @vueuse/shared.
                    shapeDifferences.Add($"{path}(0) at {string.Join(", ", arity.Select(call => call.Location).Distinct())}");
                }
                else
                    throw new InvalidOperationException($"Missing argument count in {specifier}#{path}({arity.Key}) " +
                        $"at {string.Join(", ", arity.Select(call => call.Location).Distinct())}.");
            }
        }
    }
    Console.WriteLine($"  Positional argument counts {supported}/{expected}");
    if (shapeDifferences.Count > 0)
        Console.WriteLine($"  Upstream export shape differences ({shapeDifferences.Count}, boolean constants in @vueuse/shared) " +
            $"[{string.Join(", ", shapeDifferences)}]");
    return (expected, supported);
}

static void VerifyMissingReturnOperations(string specifier, SortedDictionary<string, SortedSet<string>> missing)
{
    foreach (var (operation, locations) in missing)
    {
        var reason = (specifier, operation) switch
        {
            ("@vueuse/core", "useTimeoutFn(2).isActive (read)" or "useTimeoutFn(2).pause (call)" or
                "useTimeoutFn(2).resume (call)") =>
                "@vueuse/shared/dist/index.d.ts declares UseTimeoutFnReturn as Stoppable<Parameters<CallbackFn> | []>. " +
                "The three handwritten members belong to Pausable.",
            ("vee-validate", "useFieldArray(1).path (read)") =>
                "vee-validate/dist/vee-validate.d.ts declares path on PrivateFieldArrayContext. " +
                "useFieldArray returns public FieldArrayContext.",
            _ => null
        };
        var location = $"{specifier}#{operation} at {string.Join(", ", locations)}";
        if (reason is null)
            throw new InvalidOperationException($"Unexpected missing first-level return operation {location}.");
        Console.WriteLine($"    {location}. {reason}");
    }
}

static bool CoversUpstreamArity(NpmValueSignature signature, int arity)
{
    // The inventory counts JS argument slots, including literals supplied by the facade.
    // A fixed slot after an omitted C# option still emits an undefined placeholder.
    return arity >= 0 && arity <= signature.Parameters.Count &&
        signature.Parameters.Skip(arity).All(parameter =>
            parameter.Optional && parameter.FixedBoolean is null && !parameter.FixedUndefined);
}

static SortedSet<string> PrintReturnMemberCoverage(
    string specifier,
    string directory,
    SortedDictionary<string, List<AuthoredMember>> authored,
    Dictionary<string, NpmValueExport> projected,
    Dictionary<string, DeclarationExport> inventory)
{
    var trees = Directory.Exists(directory) ? Directory.EnumerateFiles(directory, "*.cs", SearchOption.AllDirectories)
        .Select(file => CSharpSyntaxTree.ParseText(File.ReadAllText(file),
            new CSharpParseOptions(LanguageVersion.Preview), path: file)).ToArray() : [];
    var hierarchy = CSharpCompilation.Create("AuthoredBindingTypes", trees);
    var types = trees.SelectMany(tree => tree.GetRoot().DescendantNodes().OfType<TypeDeclarationSyntax>())
        .Where(type => type.Parent is BaseNamespaceDeclarationSyntax or CompilationUnitSyntax &&
            type.Modifiers.Any(SyntaxKind.PublicKeyword))
        .ToLookup(type => (type.Identifier.ValueText, type.Arity));
    var localTypes = types.SelectMany(group => group).ToHashSet();
    var handles = projected.Values.SelectMany(value =>
        (value.ConstantType is { } constant ? [constant] : Enumerable.Empty<NpmValueType>())
        .Concat(value.Signatures.Select(signature => signature.ReturnType)))
        .SelectMany(ReturnTypes)
        .Where(type => type is { Kind: "handle", TypeDefinition: not null, Members.Count: > 0 })
        .ToLookup(type => (type.TypeDefinition, type.ProtocolKey, type.TypeIdentity));
    var present = new SortedSet<string>(StringComparer.Ordinal);
    var missing = new SortedDictionary<string, SortedSet<string>>(StringComparer.Ordinal);
    var overloadDependent = new SortedSet<string>(StringComparer.Ordinal);
    var unassessed = new SortedSet<string>(StringComparer.Ordinal);
    foreach (var (export, declarations) in authored)
    {
        foreach (var declaration in declarations)
        {
            var path = declaration.InlineTarget is { } child ? $"{export}.{child}" : export;
            var value = projected.GetValueOrDefault(export);
            var namespaceExport = inventory.GetValueOrDefault(export)?.Kind == "namespace";
            var type = declaration.ReturnType is NullableTypeSyntax nullable ? nullable.ElementType : declaration.ReturnType;
            var owners = type is SimpleNameSyntax name ? types[(name.Identifier.ValueText, name.Arity)].ToArray() : null;
            if (owners is not [{ } owner] || declaration.Variadic ||
                namespaceExport && declaration.InlineTarget is null)
            {
                unassessed.Add($"{path} ({declaration.ReturnType} at {declaration.Location})");
                continue;
            }

            var signatures = namespaceExport
                ? value?.ConstantType?.Members?.FirstOrDefault(member => member.Name == declaration.InlineTarget)?.Type.Signatures
                : value?.Signatures;
            var calls = declaration.Callable
                ? Enumerable.Range(declaration.Required!.Value, declaration.Parameters!.Value - declaration.Required.Value + 1)
                    .Select(arity => (Path: $"{path}({arity})", Returns: signatures?
                        .Where(signature => CoversUpstreamArity(signature, arity)).Select(signature => signature.ReturnType).ToArray()))
                : [(path, value?.ConstantType is { } constant ? [constant] : null)];

            foreach (var (callPath, returns) in calls)
            {
                var resolved = returns?.Select(ResolveResultMembers).ToArray();
                if (resolved is not { Length: > 0 } || resolved.Any(result =>
                    result.Kind is not ("object" or "handle" or "enrichedRef" or "callableHandle") ||
                    result.Members is null))
                {
                    unassessed.Add($"{callPath} ({owner.Identifier.ValueText} to {string.Join(" | ", returns?.Select(result => result.Kind) ?? ["unavailable"])})");
                    continue;
                }

                var inherited = InheritedMembers(owner);
                if (inherited.Unresolved)
                    unassessed.Add($"{callPath} ({owner.Identifier.ValueText} has a base outside local public Types)");
                foreach (var member in inherited.Members)
                {
                    var operations = member switch
                    {
                        MethodDeclarationSyntax => new[] { "call" },
                        PropertyDeclarationSyntax property => property.AccessorList?.Accessors
                            .Where(accessor => accessor.Modifiers.Count == 0 && !accessor.IsKind(SyntaxKind.InitAccessorDeclaration))
                            .Select(accessor => accessor.IsKind(SyntaxKind.GetAccessorDeclaration) ? "read" : "write").ToArray() ?? [],
                        _ => []
                    };
                    if (operations.Length == 0)
                        continue;
                    var alias = member.AttributeLists.SelectMany(list => list.Attributes)
                        .Where(attribute => attribute.Name.ToString() is "Description" or "DescriptionAttribute")
                        .Select(attribute => (attribute.ArgumentList?.Arguments.Single().Expression as LiteralExpressionSyntax)?.Token.ValueText)
                        .SingleOrDefault(text => text is { Length: > 2 } && text.StartsWith("@#", StringComparison.Ordinal));
                    if (alias is null)
                    {
                        unassessed.Add($"{callPath} ({owner.Identifier.ValueText} has a member without an explicit mapping)");
                        continue;
                    }
                    var nameOnWire = alias[2..];
                    var matches = resolved.Select(result => result.Members?.FirstOrDefault(projectedMember =>
                        projectedMember.Name == nameOnWire)).ToArray();
                    foreach (var operation in operations)
                    {
                        var memberPath = $"{callPath}.{nameOnWire} ({operation})";
                        // Multiple same-arity projections cannot be paired to C# overloads
                        // without checking argument domains. Count absence only if universal.
                        var supported = matches.Count(match => match is not null && (operation switch
                        {
                            "call" => match.Type.Signatures is { Count: > 0 },
                            "write" => !match.ReadOnly,
                            _ => match.Type.Kind != "method"
                        }));
                        // The ref's value lives on its generated base interface.
                        if (nameOnWire == "value" && operation != "call")
                            supported += resolved.Zip(matches).Count(pair => pair.Second is null &&
                                pair.First.Kind == "enrichedRef" &&
                                (operation == "read" || !pair.First.ReadOnly));
                        if (supported == 0)
                        {
                            if (!missing.TryGetValue(memberPath, out var locations))
                                missing.Add(memberPath, locations = new(StringComparer.Ordinal));
                            locations.Add(declaration.Location);
                        }
                        else if (supported == matches.Length)
                            present.Add(memberPath);
                        else
                            overloadDependent.Add(memberPath);
                    }
                }
            }
        }
    }
    Console.WriteLine($"  First-level return operation presence {present.Count}");
    Console.WriteLine($"  Missing first-level return operations ({missing.Count}) [{string.Join(", ", missing.Keys)}]");
    Console.WriteLine($"  Overload-dependent first-level operations ({overloadDependent.Count}) [{string.Join(", ", overloadDependent)}]");
    Console.WriteLine($"  Unassessed first-level returns ({unassessed.Count}) [{string.Join(", ", unassessed)}]");
    Console.WriteLine("  Scope covers explicitly mapped operations on public Types and their resolved local bases. Parameter domains and nested return operations are outside this comparison.");
    VerifyMissingReturnOperations(specifier, missing);
    return overloadDependent;

    NpmValueType ResolveResultMembers(NpmValueType type)
    {
        // Compare operations on a present result. Nullability remains part of the
        // C# facade and is exercised by the corresponding consumer.
        if (type is { Kind: "nullable" or "undefinedable", ElementType: { } element })
            return ResolveResultMembers(element);
        if (type is not { Kind: "handle", TypeDefinition: not null, Members: null })
            return type;
        var arguments = type.TypeArguments ?? [];
        // Concrete arguments can be compared across exports. Open type
        // parameter names have signature-local scope and cannot prove identity.
        if (arguments.Any(argument => ReturnTypes(argument).Any(nested =>
            nested.Kind is "typeParameter" or "recordTypeParameter")))
            return type;
        var key = JsonSerializer.Serialize(arguments);
        var candidates = handles[(type.TypeDefinition, type.ProtocolKey, type.TypeIdentity)]
            .Where(candidate => JsonSerializer.Serialize(candidate.TypeArguments ?? []) == key)
            .GroupBy(candidate => JsonSerializer.Serialize(candidate.Members), StringComparer.Ordinal)
            .Take(2).ToArray();
        return candidates is [var match] ? type with { Members = match.First().Members } : type;
    }

    (List<MemberDeclarationSyntax> Members, bool Unresolved) InheritedMembers(TypeDeclarationSyntax owner)
    {
        var members = new List<MemberDeclarationSyntax>();
        var visited = new HashSet<TypeDeclarationSyntax>();
        var unresolved = false;
        Visit(owner);
        return (members, unresolved);

        void Visit(TypeDeclarationSyntax current)
        {
            if (!visited.Add(current))
                return;
            members.AddRange(current.Members.Where(member => !member.Modifiers.Any(SyntaxKind.StaticKeyword) &&
                (member.Modifiers.Any(SyntaxKind.PublicKeyword) ||
                    current is InterfaceDeclarationSyntax && !member.Modifiers.Any(SyntaxKind.PrivateKeyword) &&
                    !member.Modifiers.Any(SyntaxKind.ProtectedKeyword) && !member.Modifiers.Any(SyntaxKind.InternalKeyword))));
            foreach (var baseType in current.BaseList?.Types ?? default)
            {
                var symbol = hierarchy.GetSemanticModel(current.SyntaxTree).GetTypeInfo(baseType.Type).Type as INamedTypeSymbol;
                var declarations = symbol?.OriginalDefinition.DeclaringSyntaxReferences
                    .Select(reference => reference.GetSyntax()).OfType<TypeDeclarationSyntax>().ToArray();
                if (declarations is [{ } declaration] && localTypes.Contains(declaration))
                    Visit(declaration);
                else
                    unresolved = true;
            }
        }
    }
}

static IEnumerable<NpmValueType> ReturnTypes(NpmValueType type)
{
    yield return type;
    foreach (var child in (type.Members ?? []).Select(member => member.Type)
        .Concat((type.Signatures ?? []).Select(signature => signature.ReturnType))
        .Concat(type.TypeArguments ?? []).Concat(type.Variants ?? [])
        .Concat(new[] { type.ElementType, type.WriteType }.OfType<NpmValueType>()))
        foreach (var nested in ReturnTypes(child))
            yield return nested;
}

static void PrintProjection(NpmValueExport value, bool isNamespace)
{
    if (value.ConstantType is { } constant)
    {
        if (isNamespace)
            Console.WriteLine($"    Projected  namespace members [{string.Join(", ", constant.Members!.Select(member => member.Name))}]");
        else
            Console.WriteLine($"    Projected  constant {DescribeType(constant)}");
    }
    foreach (var signature in value.Signatures)
        Console.WriteLine($"    Projected  callable {DescribeSignature(signature)}");
}

static string DescribeSignature(NpmValueSignature signature) =>
    (signature.TypeParameters is { Count: > 0 } names ? $"<{string.Join(", ", names)}>" : "") +
    $"({string.Join(", ", signature.Parameters.Select(parameter =>
        parameter.FixedUndefined ? $"{parameter.Name} = undefined" :
        parameter.FixedBoolean is { } literal ? $"{parameter.Name} = {(literal ? "true" : "false")}" :
            $"{parameter.Name}{(parameter.Optional ? "?" : "")} {DescribeType(parameter.Type)}"))}) -> {DescribeType(signature.ReturnType)}";

static string DescribeType(NpmValueType type)
{
    if (type.Kind is "ref" or "enrichedRef" && type.ElementType is { } read)
    {
        var name = type.Name ?? (type.ReadOnly && type.WriteType is null ? "ReadonlyRef" : "Ref");
        var slots = type.WriteType is { } write
            ? $"{DescribeType(read)}, {DescribeType(write)}" : DescribeType(read);
        var extras = type.Members is { Count: > 0 } membersOnRef
            ? " & { " + string.Join(", ", membersOnRef.Select(member => $"{member.Name} {DescribeType(member.Type)}")) + " }"
            : "";
        return $"{name}<{slots}>{extras}";
    }
    if (type.Kind == "handle")
        return type.TypeArguments is { Count: > 0 } arguments
            ? $"{type.Name}<{string.Join(", ", arguments.Select(DescribeType))}>"
            : type.Name!;
    if (type.Variants is { } variants)
        return string.Join(" | ", variants.Select(DescribeType));
    if (type.ElementType is { } element)
        return $"{type.Kind}<{DescribeType(element)}>";
    if (type.Members is { } members)
        return $"{{ {string.Join(", ", members.Select(member =>
            $"{member.Name}{(member.Optional ? "?" : "")} {DescribeType(member.Type)}"))} }}";
    if (type.Signatures is { } signatures)
        return string.Join(" | ", signatures.Select(DescribeSignature));
    return type.Name ?? type.Kind;
}

static void VerifyGeneratedFacades(string project, List<NpmValueExport> primary, IReadOnlyList<NpmValueExport> additional,
    Dictionary<string, SortedSet<string>> overloadOperations)
{
    var projected = primary.Concat(additional).ToArray();
    const string source = """
        @using VueUse = Npm.Vueuse.Core.Exports
        @using Dates = Npm.DateFns.Exports
        @using Floating = Npm.FloatingUi.Vue.Exports
        @using Vee = Npm.VeeValidate.Exports
        @using Query = Npm.Tanstack.VueQuery.Exports
        @using I18n = Npm.VueI18n.Exports
        @using Monaco = Npm.MonacoEditor.Editor.EditorApiJs.Exports
        <p>@Bound()</p>
        @code {
            private bool Bound() {
                Monaco.Editor.SetTheme("vs-dark");
                Monaco.Editor.RemoveAllMarkers("test");
                bool dirty = Vee.UseIsFormDirty().Value;
                var form = Vee.UseForm();
                bool submitting = form.IsSubmitting.Value;
                double submitCount = form.SubmitCount.Value;
                var setError = Vee.UseSetFieldError("email");
                setError("Invalid");
                setError(new string[] { "First", "Second" });
                double fetching = Query.UseIsFetching().Value;
                double clientFetching = Query.UseQueryClient().IsFetching();
                var i18n = I18n.UseI18n();
                string locale = i18n.Locale.Value;
                var counter = VueUse.UseCounter(0);
                var mouse = VueUse.UseMouse();
                var online = VueUse.UseOnline();
                var storage = VueUse.UseStorage("projection-probe", "default");
                var start = new ECMAScript.Date(2024, 0, 1);
                var end = Dates.AddDays(start, 1);
                var duration = new Dates.Duration { Days = 2 };
                ECMAScript.Date later = Dates.Add(end, duration);
                ECMAScript.Date earlier = Dates.Sub(later, duration);
                string label = Dates.FormatDuration(duration);
                var days = Dates.EachDayOfInterval(new() { Start = start, End = later });
                ECMAScript.Date clamped = Dates.Clamp(earlier, new() { Start = start, End = later });
                var choices = new Dates.DateOrNumberOrString[] { start, 1700000000000d };
                ECMAScript.Date min = Dates.Min(choices);
                ECMAScript.Date max = Dates.Max(choices);
                double x = mouse.X.Value;
                double previous = counter.Count.Value;
                return VueUse.IsClient && !VueUse.IsIOS && !VueUse.IsWorker &&
                    online.Value && storage.Value == label && x >= 0 &&
                    counter.Inc() > previous && Dates.IsBefore(start, earlier) &&
                    days.Length > 0 && Dates.IsBefore(min, max) && Dates.IsBefore(days[0], clamped) &&
                    dirty && !submitting && submitCount >= 0 && fetching == clientFetching &&
                    i18n.AvailableLocales.Length > 0 && locale.Length > 0;
            }
            private void CheckPosition(ECMAScript.Element reference, ECMAScript.HTMLElement floating) {
                Floating.ComputePosition(reference, floating).Then(position => {
                    double x = position.X;
                });
            }
        }
        """;
    var runtime = Path.Combine("scripts", "csharp", "fixtures", "jolt-npm-runtime");
    var sources = new List<JoltSource> { new("Probe.jazor", source) };
    // Jolt interprets source paths as component namespaces, so use fixture filenames here.
    foreach (var file in new[] {
        "App.jazor", "Floating.jazor", "Query.jazor", "Vee.jazor", "Submit.jazor", "FormScope.jazor",
        "Events.jazor", "EventHook.jazor", "Getters.jazor", "Behavior.jazor", "Storage.jazor",
        "Model.jazor", "UrlParams.jazor", "ColorMode.jazor", "Clipboard.jazor" })
        sources.Add(new(file, File.ReadAllText(Path.Combine(runtime, file))));
    var fixtures = Path.GetDirectoryName(runtime)!;
    foreach (var file in new[] { "I18nPlugin.jazor", "FloatingVue.jazor", "AsyncVueRefs.jazor" })
        sources.Add(new(file, File.ReadAllText(Path.Combine(fixtures, "jolt-binding-projection", file))));
    sources.Add(new("MonacoEditor.jazor", File.ReadAllText(Path.Combine(fixtures, "jolt-monaco-browser", "App.jazor"))));
    var analysis = JoltProjectCompiler.Analyze(sources, new() { NpmValues = projected });
    var analysisErrors = analysis.Diagnostics.Where(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error).ToArray();
    Require(analysisErrors.Length == 0, $"Representative Jolt sources failed analysis.{Environment.NewLine}" +
        string.Join(Environment.NewLine, analysisErrors.Select(diagnostic => diagnostic.ToString())));
    Require(analysis.Documents.Count == sources.Count, "Every representative Jolt source must reach Roslyn.");
    var compilation = (CSharpCompilation)analysis.Documents[0].SemanticModel.Compilation;
    Require(compilation.SyntaxTrees.Any(tree => tree.FilePath.EndsWith("__JoltNpmValues.g.cs", StringComparison.Ordinal)),
        "The pooled npm metadata must provide a generated C# facade.");

    var errors = compilation.GetDiagnostics().Where(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error).ToLookup(
        diagnostic => diagnostic.Location.SourceTree?.FilePath.EndsWith("__JoltNpmValues.g.cs", StringComparison.Ordinal) == true);
    Require(!errors.Contains(true), $"npm value facade compilation failed.{Environment.NewLine}" +
        string.Join(Environment.NewLine, errors[true]));
    foreach (var export in projected)
    {
        var host = compilation.GetTypeByMetadataName($"{NpmNames.NamespaceFor(export.ModuleSpecifier)}.Exports");
        var name = NpmNames.ValueNameFor(export.ExportName);
        Require(export.Signatures.Count > 0
            ? export.Signatures.Any(signature => host?.GetMembers(name + signature.ManagedNameSuffix)
                .OfType<IMethodSymbol>().Any(method => method.DeclaredAccessibility == Accessibility.Public) == true)
            : host?.GetMembers(name).OfType<IPropertySymbol>().Any(property => property.DeclaredAccessibility == Accessibility.Public) == true,
            $"The compiled facade is missing the projected declaration for {export.ModuleSpecifier}#{export.ExportName}.");
    }
    Require(!errors.Contains(false), $"Representative typed calls failed.{Environment.NewLine}" +
        string.Join(Environment.NewLine, errors[false]));
    // Rebind the same author expressions against the emitted public metadata.
    // This catches differences in accessibility, constraints and overload resolution
    // that compiling the component and its synthesized facade together cannot expose.
    var componentTree = compilation.SyntaxTrees.Single(tree => tree.FilePath == "__JoltNpmValues.g.cs");
    var publicSource = NpmBindings.GenerateCachedSource(project, [], projected);
    var publicTree = CSharpSyntaxTree.ParseText(publicSource, (CSharpParseOptions)componentTree.Options, "Bindings.g.cs");
    var library = CSharpCompilation.Create("SevenLibraryBindings", [publicTree], compilation.References,
        new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, generalDiagnosticOption: ReportDiagnostic.Error));
    using var assembly = new MemoryStream();
    var emitted = library.Emit(assembly, options: new Microsoft.CodeAnalysis.Emit.EmitOptions(metadataOnly: true));
    Require(emitted.Success, string.Join(Environment.NewLine, emitted.Diagnostics));
    assembly.Position = 0;
    var consumerPath = Path.Combine(fixtures, "jolt-binding-projection", "CorrelatedConsumer.cs");
    var consumerTree = CSharpSyntaxTree.ParseText(File.ReadAllText(consumerPath), (CSharpParseOptions)componentTree.Options, consumerPath);
    var consumer = compilation.RemoveSyntaxTrees(componentTree).AddSyntaxTrees(consumerTree)
        .AddReferences(MetadataReference.CreateFromStream(assembly));
    foreach (var export in projected)
    {
        var facade = consumer.GetTypeByMetadataName($"{NpmNames.NamespaceFor(export.ModuleSpecifier)}.Exports");
        Require(facade?.GetMembers().Any(member => member.DeclaredAccessibility == Accessibility.Public &&
            (export.Signatures.Count > 0 ? member is IMethodSymbol : member is IPropertySymbol) &&
            HasStringAttribute(member, "ECMAScript.ECMAScriptAttribute", export.ModuleSpecifier) &&
            HasStringAttribute(member, "ECMAScript.ECMAScriptNameAttribute", export.ExportName)) == true,
            $"The public binding assembly is missing the projected declaration for {export.ModuleSpecifier}#{export.ExportName}.");
    }
    VerifyOverloadOperations(consumer, (CSharpParseOptions)componentTree.Options,
        Path.Combine(fixtures, "jolt-binding-projection", "OverloadOperationConsumer.cs"), overloadOperations);
    var consumerErrors = consumer.GetDiagnostics().Where(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error).ToArray();
    Require(consumerErrors.Length == 0, $"Calls against the public binding assembly failed.{Environment.NewLine}" +
        string.Join(Environment.NewLine, consumerErrors.Select(diagnostic => diagnostic.ToString())));
    Console.WriteLine($"All {sources.Count} authoring fixtures and the C# correlated consumer also compiled against the public binding assembly.");
    var correlatedModel = consumer.GetSemanticModel(consumerTree);
    var correlatedNames = new HashSet<string>(["SetFieldValue", "Variables", "PageParams", "UseStorage"], StringComparer.Ordinal);
    foreach (var expression in consumerTree.GetRoot().DescendantNodes().OfType<MemberAccessExpressionSyntax>())
    {
        var name = expression.Name.Identifier.ValueText;
        if (correlatedNames.Contains(name) &&
            correlatedModel.GetSymbolInfo(expression).Symbol is { ContainingAssembly.Name: "SevenLibraryBindings" } symbol)
        {
            correlatedNames.Remove(name);
            var line = expression.GetLocation().GetLineSpan().StartLinePosition.Line + 1;
            Console.WriteLine($"  {symbol.ToDisplayString()} <= {Path.GetFileName(consumerPath)}:{line}");
        }
    }
    Require(correlatedNames.Count == 0, $"Missing public assembly consumer evidence for {string.Join(", ", correlatedNames)}.");
    ReportCallEvidence(analysis);
    Console.WriteLine($"Roslyn compiled the pooled facade for {primary.Count} npm exports across the seven libraries " +
        $"and {additional.Count} additional exports from {string.Join(", ", additional.Select(value => value.ModuleSpecifier).Distinct())}.");
}

static void VerifyOverloadOperations(CSharpCompilation consumer, CSharpParseOptions options, string path,
    Dictionary<string, SortedSet<string>> inventory)
{
    var tree = CSharpSyntaxTree.ParseText(File.ReadAllText(path), options, path);
    var compilation = consumer.AddSyntaxTrees(tree);
    var model = compilation.GetSemanticModel(tree);
    var evidence = new Dictionary<(string Module, string Key), SortedSet<string>>();
    foreach (var method in tree.GetRoot().DescendantNodes().OfType<MethodDeclarationSyntax>())
    {
        var declaration = method.DescendantNodes().OfType<VariableDeclaratorSyntax>()
            .FirstOrDefault(node => node.Identifier.ValueText == "result");
        if (declaration?.Initializer?.Value is not InvocationExpressionSyntax invocation ||
            model.GetOperation(invocation) is not IInvocationOperation selected ||
            selected.TargetMethod is not { ContainingAssembly.Name: "SevenLibraryBindings" } factory)
            continue;
        var receiver = model.GetDeclaredSymbol(declaration);
        var module = ReadAttribute(factory, "ECMAScript.ECMAScriptAttribute")!;
        var export = ReadAttribute(factory, "ECMAScript.ECMAScriptNameAttribute");
        var group = $"{export}({invocation.ArgumentList.Arguments.Count})";
        foreach (var expression in method.DescendantNodes().OfType<MemberAccessExpressionSyntax>())
        {
            if (!SymbolEqualityComparer.Default.Equals(model.GetSymbolInfo(expression.Expression).Symbol, receiver) ||
                model.GetSymbolInfo(expression).Symbol is not { ContainingAssembly.Name: "SevenLibraryBindings" } member ||
                ReadAttribute(member, "System.ComponentModel.DescriptionAttribute") is not { } alias ||
                !alias.StartsWith("@#", StringComparison.Ordinal))
                continue;
            var operation = expression.Parent switch
            {
                InvocationExpressionSyntax call when call.Expression == expression &&
                    model.GetOperation(call) is IInvocationOperation => "call",
                AssignmentExpressionSyntax assignment when assignment.Left == expression => "write",
                _ when member is IPropertySymbol => "read",
                _ => null
            };
            if (operation is null)
                continue;
            var key = (module, $"{group}.{alias[2..]} ({operation})");
            if (!evidence.TryGetValue(key, out var selectedSymbols))
                evidence.Add(key, selectedSymbols = new(StringComparer.Ordinal));
            selectedSymbols.Add($"{method.Identifier.ValueText} selected {factory.ToDisplayString()} and {member.ToDisplayString()}");
        }
    }

    var uncovered = new List<string>();
    // Changes in the overload inventory can move operations into the common
    // surface. Preserve the initial 62 operations as regression targets.
    var baselineKeys = new Dictionary<string, string[]>(StringComparer.Ordinal)
    {
        ["useField(1)"] = ["name (read)", "setValue (call)", "value (read)"],
        ["useFieldArray(1)"] = ["insert (call)", "prepend (call)", "push (call)", "replace (call)", "update (call)"],
        ["useForm(0)"] = ["defineField (call)", "errorBag (read)", "handleSubmit (call)", "isSubmitting (read)",
            "isValidating (read)", "meta (read)", "name (read)", "resetForm (call)", "setErrors (call)",
            "setFieldError (call)", "setFieldTouched (call)", "setFieldValue (call)", "setValues (call)",
            "submitCount (read)", "validateField (call)", "values (read)"],
    };
    var baseline = baselineKeys.SelectMany(group => group.Value.Select(key => $"{group.Key}.{key}"))
        .Concat(new[] { "useField(2)", "useField(3)" }.SelectMany(group => baselineKeys["useField(1)"].Select(key => $"{group}.{key}")))
        .Concat(new[] { "useForm(1)", "useFormContext(0)" }.SelectMany(group => baselineKeys["useForm(0)"].Select(key => $"{group}.{key}")));
    foreach (var (module, keys) in inventory)
    {
        var required = module == "vee-validate" ? keys.Union(baseline) : keys;
        var group = required.GroupBy(key => key[..key.IndexOf('.')]);
        Console.WriteLine($"  {module} public Roslyn overload-dependent coverage");
        foreach (var operations in group)
        {
            var missing = operations.Where(key => !evidence.ContainsKey((module, key))).ToArray();
            Console.WriteLine($"    {operations.Key} {operations.Count() - missing.Length}/{operations.Count()}");
            foreach (var key in operations.Except(missing))
                Console.WriteLine($"      {key}{(keys.Contains(key) ? "" : " [common surface]")} [{string.Join(" | ", evidence[(module, key)])}]");
            uncovered.AddRange(missing.Select(key => $"{module} {key}"));
        }
    }
    var errors = model.GetDiagnostics().Where(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error).ToArray();
    Require(uncovered.Count == 0 && errors.Length == 0,
        $"Missing public overload operation evidence [{string.Join(", ", uncovered)}].{Environment.NewLine}" +
        string.Join(Environment.NewLine, errors.Select(diagnostic => diagnostic.ToString())));

    var source = File.ReadAllText(path);
    foreach (var (original, replacement, code) in new[]
    {
        ("""result.SetValue("new", false)""", "result.SetValue(1, false)", "CS1503"),
        ("""result.Replace([""])""", "result.Replace([1])", "CS0029"),
        ("""(Input value) => value.Email, "invalid" """.TrimEnd(), """(Input value) => value.Absent, "invalid" """.TrimEnd(), "CS1061"),
        ("""(Input value) => value.Email, "invalid" """.TrimEnd(), "(Input value) => value.Email, 1", "CS1503"),
        ("""InitialValues = new("initial", 0)""", """InitialValues = new("initial", "zero")""", "CS1503"),
    })
    {
        var offset = source.IndexOf(original, StringComparison.Ordinal);
        Require(offset >= 0, $"Public overload negative probe could not find {original}.");
        var changed = source.Remove(offset, original.Length).Insert(offset, replacement);
        var invalidTree = CSharpSyntaxTree.ParseText(changed, options, path);
        var invalid = compilation.ReplaceSyntaxTree(tree, invalidTree).GetSemanticModel(invalidTree);
        var span = new TextSpan(offset, replacement.Length);
        Require(invalid.GetDiagnostics().Any(diagnostic => diagnostic.Id == code &&
            diagnostic.Location.SourceSpan.IntersectsWith(span)), $"Expected {code} from {replacement}.");
        Console.WriteLine($"  Public Roslyn negative probe {code} for {replacement}");
    }

    static string? ReadAttribute(ISymbol symbol, string attribute) =>
        symbol.GetAttributes().SingleOrDefault(data => data.AttributeClass?.ToDisplayString() == attribute)
            ?.ConstructorArguments.FirstOrDefault().Value as string;
}

static bool HasStringAttribute(ISymbol symbol, string attribute, string value) =>
    symbol.GetAttributes().Any(candidate => candidate.AttributeClass?.ToDisplayString() == attribute &&
        candidate.ConstructorArguments is [{ Value: string argument }] && argument == value);

static void ReportCallEvidence(JoltAnalysis analysis)
{
    const string facadePath = "__JoltNpmValues.g.cs";
    var used = new Dictionary<int, List<string>>();
    var hasQueryEvidence = false;
    var hasVeeEvidence = false;
    foreach (var document in analysis.Documents)
    {
        var source = SourceText.From(document.Document.Source);
        var model = document.SemanticModel;
        var root = model.SyntaxTree.GetRoot();
        foreach (var expression in root.DescendantNodes().OfType<ExpressionSyntax>())
        {
            var member = model.GetOperation(expression) switch
            {
                IInvocationOperation invocation => (ISymbol)invocation.TargetMethod,
                IMethodReferenceOperation reference => reference.Method,
                IPropertyReferenceOperation property => property.Property,
                IFieldReferenceOperation field when field.Field.ContainingType.TypeKind == TypeKind.Enum => field.Field,
                _ => null,
            };
            var declaration = member?.OriginalDefinition.DeclaringSyntaxReferences
                .SingleOrDefault(reference => reference.SyntaxTree.FilePath == facadePath);
            if (declaration is null)
                continue;
            if (!used.TryGetValue(declaration.Span.Start, out var locations))
                used.Add(declaration.Span.Start, locations = []);
            var anchor = document.Spans.Where(span => span.Generated.Contains(expression.SpanStart))
                .OrderBy(span => span.Generated.Length).FirstOrDefault();
            var sourceLine = anchor is not null && anchor.Generated.Length == anchor.Source.Length
                ? source.Lines.GetLineFromPosition(anchor.Source.Start + expression.SpanStart - anchor.Generated.Start)
                : (TextLine?)null;
            var location = sourceLine is null
                ? $"{model.SyntaxTree.FilePath}:{expression.GetLocation().GetLineSpan().StartLinePosition.Line + 1}"
                : $"{document.Document.Path}:{sourceLine.Value.LineNumber + 1}";
            var file = Path.GetFileName(document.Document.Path);
            hasQueryEvidence |= file == "Query.jazor" && member!.Name == "MutateAsync" &&
                sourceLine?.ToString().Contains(".MutateAsync(", StringComparison.Ordinal) == true;
            hasVeeEvidence |= file == "Vee.jazor" && member!.Name == "Validate" &&
                sourceLine?.ToString().Contains(".Validate(", StringComparison.Ordinal) == true;
            var text = expression.ToString().Split('\n')[0].TrimEnd();
            locations.Add($"{location} {text}");
        }
    }

    var compilation = analysis.Documents[0].SemanticModel.Compilation;
    var tree = compilation.SyntaxTrees.Single(tree => tree.FilePath == facadePath);
    var facade = compilation.GetSemanticModel(tree);
    Console.WriteLine("Generated overload and member evidence from the compiled probes");
    var exercised = 0;
    var unexercised = 0;
    foreach (var declaration in tree.GetRoot().DescendantNodes().OfType<MemberDeclarationSyntax>())
    {
        if (declaration is not (MethodDeclarationSyntax or PropertyDeclarationSyntax or IndexerDeclarationSyntax or EnumMemberDeclarationSyntax))
            continue;
        var member = facade.GetDeclaredSymbol(declaration);
        if (member is null)
            continue;
        var display = member.ToDisplayString(SymbolDisplayFormat.CSharpErrorMessageFormat);
        if (used.TryGetValue(declaration.SpanStart, out var hits))
        {
            exercised++;
            Console.WriteLine($"  Compiled    {display} [{string.Join(" | ", hits.Distinct(StringComparer.Ordinal))}]");
        }
        else
        {
            unexercised++;
            Console.WriteLine($"  Unexercised {display}");
        }
    }
    Console.WriteLine($"Generated methods, properties, indexers and enum members exercised {exercised}, unexercised {unexercised}.");
    Require(hasQueryEvidence, "Query.jazor must bind MutateAsync to a generated declaration and report its source line.");
    Require(hasVeeEvidence, "Vee.jazor must bind Validate to a generated declaration and report its source line.");
}

static void Require(bool condition, string message)
{
    if (!condition)
        throw new InvalidOperationException(message);
}

sealed record DeclarationExport(string Kind, string[] Members);
sealed record AuthoredMember(string Description, bool Callable, string? InlineTarget, string Location,
    int? Required, int? Parameters, bool Variadic, TypeSyntax? ReturnType);
