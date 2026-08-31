using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using TreeSitter;
using File = global::System.IO.File;
using Node = global::TreeSitter.Node;

internal static class TDesignComponentGenerator
{
    private const string Version = "1.20.5";

    public static void Run(string[] args)
    {
        var check = args is ["--check"];
        var report = args is ["--report"];
        if (!check && !report && args.Length != 0)
            throw new ArgumentException("Supported arguments: --check, --report.");

        var repoRoot = FindRepositoryRoot(Directory.GetCurrentDirectory());
        var projectRoot = Path.Combine(repoRoot, "src", "ECMAScript.TDesign");
        var snapshotRoot = Path.Combine(repoRoot, "src", "ECMAScript.Vue.Generator", "upstream", "tdesign-vue-next", Version);
        var contractsPath = Path.Combine(snapshotRoot, "contracts.json");
        var bindingsPath = Path.Combine(snapshotRoot, "bindings.json");
        var outputPath = Path.Combine(projectRoot, "TBasic.g.cs");

        if (!File.Exists(contractsPath) || !File.Exists(bindingsPath))
            throw new InvalidOperationException("Missing TDesign contracts. Run `tdesign snapshot` and `tdesign bindings` from ECMAScript.Vue.Generator first.");

        var contracts = ReadContracts(contractsPath);
        var bindings = ReadBindings(bindingsPath);
        using var typeScript = new Language("TypeScript");
        var typeCatalog = new TypeCatalog(snapshotRoot, typeScript);
        var candidates = contracts
            .Join(
                bindings,
                static contract => contract.Tag,
                static binding => binding.Tag,
                static (contract, binding) => new Component(contract, binding))
            .OrderBy(static component => component.Contract.AuthoringType, StringComparer.Ordinal)
            .ToArray();
        var attempts = MergeRuntimeComponents(candidates)
            .Select(component => GeneratedComponent.TryCreate(component, typeScript, typeCatalog, out var generated, out var failure)
                ? new GenerationAttempt(component, generated, null)
                : new GenerationAttempt(component, null, failure))
            .ToArray();
        var failures = attempts.Where(static attempt => attempt.Failure is not null).ToArray();
        if (report)
        {
            foreach (var failure in failures.OrderBy(static failure => failure.Component.Contract.AuthoringType, StringComparer.Ordinal))
                Console.WriteLine($"{failure.Component.Contract.AuthoringType}: {failure.Failure}");
            Console.WriteLine($"TDesign component mapping: {attempts.Length - failures.Length}/{attempts.Length} components.");
            return;
        }

        var components = attempts
            .Where(static attempt => attempt.Generated is not null)
            .Select(static attempt => attempt.Generated!)
            .ToArray();

        var source = Render(components);
        if (check)
        {
            if (!File.Exists(outputPath) || !string.Equals(File.ReadAllText(outputPath), source, StringComparison.Ordinal))
                throw new InvalidOperationException("Generated TDesign component slice is stale. Run `dotnet run --project src/ECMAScript.Vue.Generator -- tdesign components`.");

            Console.WriteLine($"TDesign basic component slice is current: {components.Length} components.");
        }
        else
        {
            File.WriteAllText(outputPath, source, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
            Console.WriteLine($"Generated {components.Length} strongly typed TDesign components: {outputPath}");
        }
    }

    static Contract[] ReadContracts(string path)
    {
        using var document = JsonDocument.Parse(File.ReadAllText(path));
        return document.RootElement.GetProperty("components").EnumerateArray()
            .Select(element => new Contract(
                element.GetProperty("tag").GetString()!,
                element.GetProperty("authoringType").GetString()!,
                element.GetProperty("props").EnumerateArray()
                    .Select(property => new Property(
                        property.GetProperty("name").GetString()!,
                        property.GetProperty("type").GetString()!,
                        property.GetProperty("source").GetString()!,
                        property.GetProperty("optional").GetBoolean()))
                    .ToArray(),
                element.GetProperty("events").EnumerateArray()
                    .Select(@event => new ComponentEvent(
                        @event.GetProperty("name").GetString()!,
                        @event.GetProperty("prop").GetString()!,
                        @event.GetProperty("type").GetString()!,
                        @event.GetProperty("source").GetString()!,
                        @event.GetProperty("optional").GetBoolean()))
                    .ToArray(),
                element.GetProperty("slots").EnumerateArray()
                    .Select(slot => new ComponentSlot(
                        slot.GetProperty("name").GetString()!,
                        slot.TryGetProperty("prop", out var property) ? property.GetString() : null,
                        slot.GetProperty("type").GetString()!,
                        slot.GetProperty("source").GetString()!))
                    .ToArray()))
            .ToArray();
    }

    static Binding[] ReadBindings(string path)
    {
        using var document = JsonDocument.Parse(File.ReadAllText(path));
        return document.RootElement.GetProperty("components").EnumerateArray()
            .Select(element => new Binding(
                element.GetProperty("tag").GetString()!,
                element.GetProperty("runtimeExport").GetString()!,
                element.GetProperty("propsDeclaration").GetString(),
                element.GetProperty("propsSource").GetString(),
                element.GetProperty("slots").EnumerateArray().Select(static slot => slot.GetString()!).ToHashSet(StringComparer.Ordinal)))
            .ToArray();
    }

    static Component[] MergeRuntimeComponents(IReadOnlyList<Component> components)
        => components
            .GroupBy(static component => component.Binding.RuntimeExport, StringComparer.Ordinal)
            .Select(group =>
            {
                var authoringTypes = group.Select(static component => component.Contract.AuthoringType).Distinct(StringComparer.Ordinal).ToArray();
                if (authoringTypes.Length != 1)
                    throw new InvalidOperationException(
                        $"Runtime export '{group.Key}' has incompatible TDesign authoring types: {string.Join(", ", authoringTypes)}.");

                var properties = group
                    .SelectMany(static component => component.Contract.Properties)
                    .GroupBy(static property => property.Name, StringComparer.Ordinal)
                    .Select(propertyGroup =>
                    {
                        var definitions = propertyGroup
                            .Select(static property => (property.Type, property.Optional))
                            .Distinct()
                            .ToArray();
                        if (definitions.Length != 1)
                            throw new InvalidOperationException(
                                $"Runtime export '{group.Key}' maps prop '{propertyGroup.Key}' to incompatible contracts.");
                        return new Property(propertyGroup.Key, definitions[0].Type, propertyGroup.First().SourcePath, definitions[0].Optional);
                    })
                    .OrderBy(static property => property.Name, StringComparer.Ordinal)
                    .ToArray();
                var first = group.First();
                var events = group
                    .SelectMany(static component => component.Contract.Events)
                    .GroupBy(static @event => @event.Property, StringComparer.Ordinal)
                    .Select(eventGroup =>
                    {
                        var definitions = eventGroup
                            .Select(static @event => (@event.Name, @event.Type, @event.Optional))
                            .Distinct()
                            .ToArray();
                        if (definitions.Length != 1)
                            throw new InvalidOperationException(
                                $"Runtime export '{group.Key}' maps event '{eventGroup.Key}' to incompatible contracts.");
                        var definition = definitions[0];
                        return new ComponentEvent(definition.Name, eventGroup.Key, definition.Type, eventGroup.First().SourcePath, definition.Optional);
                    })
                    .OrderBy(static @event => @event.Property, StringComparer.Ordinal)
                    .ToArray();
                var slots = group
                    .SelectMany(static component => component.Contract.Slots)
                    .GroupBy(static slot => slot.Name, StringComparer.Ordinal)
                    .Select(slotGroup =>
                    {
                        var definitions = slotGroup
                            .Select(static slot => (slot.Property, slot.Type))
                            .Distinct()
                            .ToArray();
                        if (definitions.Length != 1)
                            throw new InvalidOperationException(
                                $"Runtime export '{group.Key}' maps slot '{slotGroup.Key}' to incompatible contracts.");
                        var definition = definitions[0];
                        return new ComponentSlot(slotGroup.Key, definition.Property, definition.Type, slotGroup.First().SourcePath);
                    })
                    .OrderBy(static slot => slot.Name, StringComparer.Ordinal)
                    .ToArray();
                return new Component(
                    new Contract(first.Contract.Tag, first.Contract.AuthoringType, properties, events, slots),
                    first.Binding);
            })
            .OrderBy(static component => component.Contract.AuthoringType, StringComparer.Ordinal)
            .ToArray();

    static string Render(IReadOnlyList<GeneratedComponent> components)
    {
        var builder = new StringBuilder();
        builder.AppendLine("// <auto-generated />");
        builder.AppendLine("#nullable enable");
        builder.AppendLine();
        builder.AppendLine("using Microsoft.AspNetCore.Components;");
        builder.AppendLine();
        builder.AppendLine("namespace ECMAScript.TDesign;");

        foreach (var definition in CollectDefinitions(components))
        {
            builder.AppendLine();
            builder.Append(definition);
        }

        foreach (var component in components)
        {
            builder.AppendLine();
            builder.AppendLine($"[ECMAScript(\"tdesign-vue-next\", Transform.Component, \"{component.Component.Binding.RuntimeExport}\")]");
            var genericSuffix = component.TypeParameters.Length == 0
                ? string.Empty
                : "<" + string.Join(", ", component.TypeParameters.Select(static parameter => parameter.Name)) + ">";
            builder.AppendLine($"public {(component.TypeParameters.Length == 0 ? "sealed " : string.Empty)}class {component.Component.Contract.AuthoringType}{genericSuffix} : TContentComponentBase");
            builder.AppendLine("{");
            var memberCount = 0;
            foreach (var property in component.Properties)
            {
                var propertyName = property.CSharpName;
                var typeName = property.Type.Name + (property.Source.Optional || property.Type.IsNullable ? "?" : string.Empty);

                builder.AppendLine("    [Parameter]");
                if (!string.Equals(property.Source.Name, propertyName, StringComparison.Ordinal))
                    builder.AppendLine($"    [ECMAScriptName(\"{property.Source.Name}\")]");
                if (!property.Source.Optional && property.Type.IsReference && !property.Type.IsNullable)
                    builder.AppendLine("    [EditorRequired]");
                builder.AppendLine($"    public {typeName} {CSharpIdentifier.Escape(propertyName)} {{ get; set; }}{(!property.Source.Optional && property.Type.IsReference && !property.Type.IsNullable ? " = default!;" : string.Empty)}");
                builder.AppendLine();
                memberCount++;
            }

            foreach (var slot in component.Slots)
            {
                builder.AppendLine("    [Parameter]");
                if (!string.Equals(slot.Source.Name, slot.CSharpName, StringComparison.Ordinal))
                    builder.AppendLine($"    [ECMAScriptName(\"{slot.Source.Name}\")]");
                builder.AppendLine($"    public {slot.Type.Name}? {CSharpIdentifier.Escape(slot.CSharpName)} {{ get; set; }}");
                builder.AppendLine();
                memberCount++;
            }

            foreach (var @event in component.Events)
            {
                builder.AppendLine("    [Parameter]");
                if (!string.Equals(@event.ListenerRuntimeName, @event.CSharpName, StringComparison.Ordinal))
                    builder.AppendLine($"    [ECMAScriptName(\"{@event.ListenerRuntimeName}\")]");
                var callbackType = @event.PayloadType is null
                    ? "EventCallback"
                    : $"EventCallback<{@event.PayloadType.Name}{(@event.PayloadType.IsNullable ? "?" : string.Empty)}>";
                builder.AppendLine($"    public {callbackType} {@event.CSharpName} {{ get; set; }}");
                builder.AppendLine();
                memberCount++;
            }

            if (memberCount > 0)
                builder.Length -= Environment.NewLine.Length;
            builder.AppendLine("}");

            if (component.TypeParameters.Length > 0 && component.DefaultTypeArguments.Length == component.TypeParameters.Length)
            {
                builder.AppendLine();
                builder.AppendLine($"[ECMAScript(\"tdesign-vue-next\", Transform.Component, \"{component.Component.Binding.RuntimeExport}\")]");
                // Razor's component discovery cannot disambiguate a generic component and a
                // same-named closed alias. Keep the generated alias for assembly-internal
                // metadata compatibility, while typed Razor markup uses the generic component
                // directly (explicitly or through inference).
                builder.AppendLine($"internal sealed class {component.Component.Contract.AuthoringType} : {component.Component.Contract.AuthoringType}<{string.Join(", ", component.DefaultTypeArguments.Select(static argument => argument.Name))}>");
                builder.AppendLine("{");
                builder.AppendLine("}");
            }
        }

        if (components.Count > 0)
        {
            builder.AppendLine();
            builder.AppendLine("public static partial class TComponents");
            builder.AppendLine("{");
            foreach (var component in components)
            {
                builder.AppendLine($"    [ECMAScriptName(\"{component.Component.Binding.RuntimeExport}\")]");
                builder.AppendLine($"    public extern static ITDesignComponent {component.Component.Contract.AuthoringType} {{ get; }}");
                builder.AppendLine();
            }

            builder.Length -= Environment.NewLine.Length;
            builder.AppendLine("}");
            builder.AppendLine();
            builder.AppendLine("public sealed partial record TComponentRegistry");
            builder.AppendLine("{");
            foreach (var component in components)
            {
                builder.AppendLine($"    [Description(\"@#{component.Component.Binding.RuntimeExport}\")]");
                builder.AppendLine($"    public ITDesignComponent? {component.Component.Contract.AuthoringType} {{ get; init; }}");
                builder.AppendLine();
            }

            builder.Length -= Environment.NewLine.Length;
            builder.AppendLine("}");
        }

        return builder.ToString();
    }

    static IReadOnlyList<string> CollectDefinitions(IReadOnlyList<GeneratedComponent> components)
    {
        var definitions = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var definition in components.SelectMany(static component => component.Definitions))
        {
            var name = GetDefinitionName(definition);
            if (definitions.TryGetValue(name, out var existing) && !string.Equals(existing, definition, StringComparison.Ordinal))
                throw new InvalidOperationException($"TDesign generated definition '{name}' has incompatible declarations.");
            definitions[name] = definition;
        }

        return definitions.OrderBy(static pair => pair.Key, StringComparer.Ordinal).Select(static pair => pair.Value).ToArray();
    }

    static string GetDefinitionName(string definition)
    {
        var match = Regex.Match(
            definition,
            @"public\s+(?:(?:sealed|readonly)\s+)*(?:class|record|struct|enum|union)\s+(?<name>[A-Za-z_][A-Za-z0-9_]*)|public\s+delegate\s+\S+\s+(?<delegate>[A-Za-z_][A-Za-z0-9_]*)",
            RegexOptions.CultureInvariant);
        if (!match.Success)
            throw new InvalidOperationException($"Unable to identify generated TDesign definition: {definition}");
        return match.Groups["name"].Success ? match.Groups["name"].Value : match.Groups["delegate"].Value;
    }

    // This is generator-owned ABI projection from the upstream event name. The
    // resulting listener key is always materialized as member metadata.
    static string GetListenerRuntimeName(string eventName)
    {
        if (eventName.Length == 0 ||
            eventName.StartsWith("on", StringComparison.Ordinal) &&
            eventName.Length > 2 &&
            char.IsUpper(eventName[2]))
        {
            return eventName;
        }

        var builder = new StringBuilder(eventName.Length + 2);
        builder.Append("on");
        var capitalizeNext = true;
        foreach (var character in eventName)
        {
            if (character == '-')
            {
                capitalizeNext = true;
                continue;
            }

            builder.Append(capitalizeNext ? char.ToUpperInvariant(character) : character);
            capitalizeNext = false;
        }

        return builder.ToString();
    }

    static string FindRepositoryRoot(string startDirectory)
    {
        for (var directory = new DirectoryInfo(Path.GetFullPath(startDirectory)); directory is not null; directory = directory.Parent)
        {
            if (File.Exists(Path.Combine(directory.FullName, "Jazor.slnx")))
                return directory.FullName;
        }

        throw new InvalidOperationException("Unable to locate Jazor.slnx.");
    }

    sealed record Contract(
        string Tag,
        string AuthoringType,
        Property[] Properties,
        ComponentEvent[] Events,
        ComponentSlot[] Slots);
    sealed record Binding(
        string Tag,
        string RuntimeExport,
        string? PropsDeclaration,
        string? PropsSource,
        HashSet<string> Slots);
    sealed record Component(Contract Contract, Binding Binding);
    sealed record Property(string Name, string Type, string SourcePath, bool Optional);
    sealed record ComponentEvent(string Name, string Property, string Type, string SourcePath, bool Optional);
    sealed record ComponentSlot(string Name, string? Property, string Type, string SourcePath);
    sealed record GenerationAttempt(Component Component, GeneratedComponent? Generated, string? Failure);

    sealed record MappedProperty(Property Source, MappedType Type, string CSharpName);
    sealed record MappedSlot(ComponentSlot Source, MappedType Type, string CSharpName);
    sealed record MappedEvent(ComponentEvent Source, MappedType? PayloadType, string CSharpName)
    {
        public string ListenerRuntimeName => GetListenerRuntimeName(Source.Name);
    }
    sealed record GeneratedComponent(
        Component Component,
        MappedProperty[] Properties,
        MappedSlot[] Slots,
        MappedEvent[] Events,
        string[] Definitions,
        TypeParameter[] TypeParameters,
        MappedType[] DefaultTypeArguments)
    {
        public static bool TryCreate(
            Component component,
            Language typeScript,
            TypeCatalog typeCatalog,
            out GeneratedComponent generated,
            out string? failure)
        {
            var typeParameters = ResolveComponentTypeParameters(component, typeCatalog);
            var mapper = new TypeMapper(typeScript, typeCatalog, typeParameters);
            var properties = new List<MappedProperty>();
            var slots = new List<MappedSlot>();
            var events = new List<MappedEvent>();
            var eventProperties = component.Contract.Events
                .Select(static @event => @event.Property)
                .ToHashSet(StringComparer.Ordinal);
            var slotsByProperty = component.Contract.Slots
                .Where(static slot => slot.Property is not null)
                .GroupBy(static slot => slot.Property!, StringComparer.Ordinal)
                .ToDictionary(static group => group.Key, static group => group.First(), StringComparer.Ordinal);
            foreach (var property in component.Contract.Properties.Where(static property =>
                         property.Name is not ("default" or "class" or "style")))
            {
                if (eventProperties.Contains(property.Name))
                    continue;

                // TComponentBase owns Vue's universal class/style bindings. Emitting the upstream
                // prop again creates two C# parameters that both lower to the same Vue name.
                // Keep the richer VueClassValue/VueStyleValue base contract instead.

                // Named upstream declarations own their concise T* name. Anonymous prop
                // shapes receive a Value suffix, preventing collisions such as CalendarCell
                // (a declaration) versus Calendar.cell (a union-valued prop).
                var typeName = component.Contract.AuthoringType + ToCSharpName(property.Name) + "Value";
                var sourceType = property.SourcePath == "helper/web-types.json"
                    ? NormalizeWebType(property.Type)
                    : property.Type;
                if (TryGetTNodeBranch(sourceType, out var tNodeType))
                {
                    var slotSource = slotsByProperty.GetValueOrDefault(property.Name) ??
                        new ComponentSlot(property.Name, property.Name, sourceType, property.SourcePath);
                    if (!TryMapSlot(mapper, slotSource, tNodeType, component.Contract.AuthoringType, out var slot, out failure))
                    {
                        generated = default!;
                        return false;
                    }

                    if (!slots.Any(candidate => string.Equals(candidate.Source.Name, slot.Source.Name, StringComparison.Ordinal)))
                        slots.Add(slot);

                    sourceType = RemoveTNodeBranches(sourceType);
                    if (string.IsNullOrWhiteSpace(sourceType))
                        continue;
                }
                else if (slotsByProperty.TryGetValue(property.Name, out var aliasedSlotSource))
                {
                    // Some upstream aliases, such as FooterButton, contain TNode internally.
                    // contracts.json has already identified the prop as a slot, so preserve its
                    // Razor slot surface instead of treating the alias as a value-only prop.
                    if (!TryMapSlot(mapper, aliasedSlotSource, "TNode", component.Contract.AuthoringType, out var slot, out failure))
                    {
                        generated = default!;
                        return false;
                    }

                    if (!slots.Any(candidate => string.Equals(candidate.Source.Name, slot.Source.Name, StringComparison.Ordinal)))
                        slots.Add(slot);
                }

                if (!mapper.TryMap(sourceType, typeName, property.SourcePath, out var type))
                {
                    generated = default!;
                    failure = $"{property.Name} ({property.SourcePath}): {property.Type} [{mapper.LastFailure}]";
                    return false;
                }

                var hasSlot = slotsByProperty.ContainsKey(property.Name) || TryGetTNodeBranch(property.Type, out _);
                properties.Add(new MappedProperty(
                    property,
                    type,
                    ToPascalCase(property.Name) + (hasSlot ? "Value" : string.Empty)));
            }

            foreach (var slotSource in component.Contract.Slots.Where(static slot => slot.Property is null && slot.Name != "default"))
            {
                var slotType = TryGetTNodeBranch(slotSource.Type, out var tNodeType) ? tNodeType : slotSource.Type;
                if (!TryMapSlot(mapper, slotSource, slotType, component.Contract.AuthoringType, out var slot, out failure))
                {
                    generated = default!;
                    return false;
                }

                if (!slots.Any(candidate => string.Equals(candidate.Source.Name, slot.Source.Name, StringComparison.Ordinal)))
                    slots.Add(slot);
            }

            foreach (var @event in component.Contract.Events)
            {
                var typeName = component.Contract.AuthoringType + ToPascalCase(@event.Property[2..]) + "Event";
                if (!mapper.TryMapCallbackParameters(@event.Type, typeName, @event.SourcePath, out var parameters))
                {
                    generated = default!;
                    failure = $"event {@event.Property} ({@event.SourcePath}): {@event.Type} [{mapper.LastFailure}]";
                    return false;
                }

                // EventCallback<T> models one JavaScript callback argument. TDesign emits
                // events as (value, context, ...); Vue does not pack those arguments into an
                // object, so a synthetic payload record would incorrectly lower value.Value
                // to value.value at runtime. The first argument is the truthful callback payload.
                MappedType? payloadType = parameters.Length == 0
                    ? null
                    : parameters[0].Type with
                    {
                        IsNullable = parameters[0].Optional || parameters[0].Type.IsNullable
                    };
                events.Add(new MappedEvent(@event, payloadType, ToPascalCase(@event.Property)));
            }

            var defaults = new List<MappedType>();
            foreach (var parameter in typeParameters)
            {
                if (parameter.DefaultSource is null || component.Binding.PropsSource is null ||
                    !mapper.TryMap(
                        parameter.DefaultSource,
                        component.Contract.AuthoringType + parameter.Name + "Default",
                        component.Binding.PropsSource,
                        out var defaultType))
                {
                    generated = default!;
                    failure = $"generic parameter {parameter.Name}: default type '{parameter.DefaultSource ?? "missing"}' is not mappable [{mapper.LastFailure}]";
                    return false;
                }

                defaults.Add(defaultType);
            }

            generated = new GeneratedComponent(
                component,
                properties.ToArray(),
                slots.ToArray(),
                events.ToArray(),
                mapper.Definitions.ToArray(),
                typeParameters,
                defaults.ToArray());
            failure = null;
            return true;
        }

        private static TypeParameter[] ResolveComponentTypeParameters(Component component, TypeCatalog catalog)
        {
            if (component.Binding.PropsDeclaration is null || component.Binding.PropsSource is null)
                return [];

            var colon = component.Binding.PropsDeclaration.IndexOf(':');
            var declarationName = colon < 0
                ? component.Binding.PropsDeclaration
                : component.Binding.PropsDeclaration[(colon + 1)..];
            return catalog.TryResolve(component.Binding.PropsSource, declarationName, out var declaration)
                ? declaration.TypeParameters
                : [];
        }

        private static bool TryMapSlot(
            TypeMapper mapper,
            ComponentSlot source,
            string tNodeType,
            string componentName,
            out MappedSlot slot,
            out string? failure)
        {
            var typeName = componentName + ToPascalCase(source.Name) + "Slot";
            if (!mapper.TryMap(tNodeType, typeName, source.SourcePath, out var mapped) ||
                !mapped.Name.StartsWith("RenderFragment", StringComparison.Ordinal))
            {
                slot = default!;
                failure = $"slot {source.Name} ({source.SourcePath}): {source.Type} [{mapper.LastFailure ?? "does not map to RenderFragment"}]";
                return false;
            }

            slot = new MappedSlot(source, mapped, ToSlotPropertyName(source.Name));
            failure = null;
            return true;
        }

        private static bool TryGetTNodeBranch(string source, out string tNodeType)
        {
            tNodeType = SplitTopLevel(source, '|')
                .FirstOrDefault(static branch => branch.TrimStart().StartsWith("TNode", StringComparison.Ordinal) &&
                    branch.TrimStart().AsSpan("TNode".Length).TrimStart() is var tail && (tail.IsEmpty || tail[0] == '<'))
                ?.Trim() ?? string.Empty;
            return tNodeType.Length > 0;
        }

        private static string RemoveTNodeBranches(string source)
            => string.Join(" | ", SplitTopLevel(source, '|')
                .Where(branch => !TryGetTNodeBranch(branch, out _)));

        private static IEnumerable<string> SplitTopLevel(string source, char separator)
        {
            var start = 0;
            var depth = 0;
            for (var index = 0; index < source.Length; index++)
            {
                depth += source[index] switch { '<' or '(' or '[' or '{' => 1, '>' or ')' or ']' or '}' => -1, _ => 0 };
                if (source[index] != separator || depth != 0)
                    continue;
                yield return source[start..index].Trim();
                start = index + 1;
            }
            yield return source[start..].Trim();
        }

        private static string ToSlotPropertyName(string slotName)
        {
            var name = ToPascalCase(slotName);
            return name switch
            {
                "Content" => "ContentSlot",
                _ when name.EndsWith("Content", StringComparison.Ordinal) => name,
                _ => name + "Content"
            };
        }

        private static string ToPascalCase(string name)
        {
            var builder = new StringBuilder(name.Length);
            var upper = true;
            foreach (var character in name)
            {
                if (character is '-' or '_')
                {
                    upper = true;
                    continue;
                }

                builder.Append(upper ? char.ToUpperInvariant(character) : character);
                upper = false;
            }

            return builder.ToString() switch
            {
                "Params" => "Parameters",
                "Event" => "EventData",
                _ => builder.ToString()
            };
        }

        private static string ToCSharpName(string name)
        {
            var builder = new StringBuilder(name.Length);
            var upper = true;
            foreach (var character in name)
            {
                if (character is '-' or '_')
                {
                    upper = true;
                    continue;
                }

                builder.Append(upper ? char.ToUpperInvariant(character) : character);
                upper = false;
            }

            return builder.ToString();
        }

        private static string NormalizeWebType(string source)
        {
            var branches = source.Split('|', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
            if (branches.Length < 2 || branches.Any(static branch => !Regex.IsMatch(branch, @"^[a-z][a-z0-9-]*$")))
                return source;

            var predefined = new HashSet<string>(["boolean", "number", "string", "object", "unknown"], StringComparer.Ordinal);
            return string.Join(" | ", branches.Select(branch => predefined.Contains(branch) ? branch : $"'{branch}'"));
        }
    }

    sealed record MappedType(string Name, bool IsReference, bool IsNullable = false);
    sealed record MappedParameter(string SourceName, string CSharpName, MappedType Type, bool Optional);
    sealed record MappedShapeProperty(string SourceName, string CSharpName, MappedType Type, bool Optional);
    sealed record MappedShape(string[] TypeParameters, MappedShapeProperty[] Properties);

    static class CSharpIdentifier
    {
        private static readonly HashSet<string> Keywords = new(StringComparer.Ordinal)
    {
        "abstract", "as", "base", "bool", "break", "byte", "case", "catch", "char", "checked", "class", "const",
        "continue", "decimal", "default", "delegate", "do", "double", "else", "enum", "event", "explicit", "extern",
        "false", "finally", "fixed", "float", "for", "foreach", "goto", "if", "implicit", "in", "int", "interface",
        "internal", "is", "lock", "long", "namespace", "new", "null", "object", "operator", "out", "override", "params",
        "private", "protected", "public", "readonly", "ref", "remove", "return", "sbyte", "sealed", "short", "sizeof",
        "stackalloc", "static", "string", "struct", "switch", "this", "throw", "true", "try", "typeof", "uint", "ulong",
        "unchecked", "unsafe", "ushort", "using", "virtual", "void", "volatile", "while",
        "add", "alias", "and", "ascending", "async", "await", "by", "descending", "dynamic", "equals", "file", "from",
        "get", "global", "group", "init", "into", "join", "let", "managed", "nameof", "not", "notnull", "on", "or",
        "orderby", "partial", "record", "required", "select", "set", "unmanaged", "value", "var", "when", "where", "with", "yield"
    };

        public static string Escape(string name)
            => Keywords.Contains(name) ? "@" + name : name;
    }

    enum TypeScriptDeclarationKind
    {
        Interface,
        TypeAlias,
        Class
    }

    sealed record TypeScriptDeclaration(
        string Name,
        string SourcePath,
        TypeScriptDeclarationKind Kind,
        string Body,
        string Definition,
        string[] BaseTypes,
        TypeParameter[] TypeParameters);

    sealed record TypeParameter(string Name, string? DefaultSource);
    sealed record ImportedType(string ExportName, string[] Targets);

    /// <summary>
    /// Indexes frozen declaration syntax by its real source path. Type lookup is source-aware
    /// so generated contracts cannot accidentally bind a same-named declaration from another module.
    /// </summary>
    sealed class TypeCatalog
    {
        private readonly Dictionary<string, TypeScriptDeclaration[]> _byName;
        private readonly Dictionary<(string SourcePath, string Name), ImportedType> _imports;

        public TypeCatalog(string snapshotRoot, Language typeScript)
        {
            var declarations = new List<TypeScriptDeclaration>();
            var imports = new Dictionary<(string SourcePath, string Name), ImportedType>();
            foreach (var path in Directory.GetFiles(snapshotRoot, "*.d.ts", SearchOption.AllDirectories))
            {
                var sourcePath = Path.GetRelativePath(snapshotRoot, path).Replace('\\', '/');
                var source = File.ReadAllText(path);
                foreach (var import in ReadImports(sourcePath, source))
                    imports[import.Key] = import.Value;
                using var parser = new Parser(typeScript);
                using var tree = parser.Parse(source)
                    ?? throw new InvalidOperationException($"Unable to parse TypeScript declaration: {path}");
                foreach (var declaration in EnumerateDeclarations(tree.RootNode))
                {
                    var name = declaration.GetChildForField("name")?.Text;
                    if (string.IsNullOrWhiteSpace(name))
                        throw new InvalidOperationException($"TypeScript declaration without a name: {path}");

                    var body = declaration.GetChildForField("body")?.Text ?? string.Empty;
                    var definition = declaration.Type == "type_alias_declaration"
                        ? ExtractAliasDefinition(declaration.Text)
                        : string.Empty;
                    declarations.Add(new TypeScriptDeclaration(
                        name,
                        sourcePath,
                        declaration.Type switch
                        {
                            "interface_declaration" => TypeScriptDeclarationKind.Interface,
                            "class_declaration" => TypeScriptDeclarationKind.Class,
                            _ => TypeScriptDeclarationKind.TypeAlias
                        },
                        body,
                        definition,
                        declaration.Type == "interface_declaration" ? ReadInterfaceBases(declaration.Text) : [],
                        ReadTypeParameters(declaration)));
                }
            }

            _byName = declarations
                .GroupBy(static declaration => declaration.Name, StringComparer.Ordinal)
                .ToDictionary(
                    static group => group.Key,
                    static group => group.OrderBy(static declaration => declaration.SourcePath, StringComparer.Ordinal).ToArray(),
                    StringComparer.Ordinal);
            _imports = imports;
        }

        private static IEnumerable<Node> EnumerateDeclarations(Node node)
        {
            if (node.Type is "interface_declaration" or "type_alias_declaration" or "class_declaration")
            {
                yield return node;
                yield break;
            }

            foreach (var child in node.NamedChildren)
            {
                foreach (var declaration in EnumerateDeclarations(child))
                    yield return declaration;
            }
        }

        public bool TryResolve(string sourcePath, string name, out TypeScriptDeclaration declaration)
        {
            var importedType = _imports.GetValueOrDefault((sourcePath, name));
            var exportName = importedType?.ExportName ?? name;
            if (!_byName.TryGetValue(exportName, out var candidates))
            {
                declaration = default!;
                return false;
            }

            if (importedType is not null)
            {
                var importTargets = importedType.Targets;
                var imported = candidates.Where(candidate => importTargets.Contains(candidate.SourcePath, StringComparer.Ordinal)).ToArray();
                if (imported.Length == 1)
                {
                    declaration = imported[0];
                    return true;
                }

                // A package facade can import and immediately re-export a declaration.
                // Follow that bound import rather than treating the facade as a source
                // declaration itself (validator/es/lib/isEmail is this exact shape).
                var forwardedTargets = importTargets
                    .SelectMany(target => _imports.TryGetValue((target, exportName), out var forwarded)
                        ? forwarded.Targets
                        : [])
                    .Distinct(StringComparer.Ordinal)
                    .ToArray();
                var forwardedCandidates = candidates
                    .Where(candidate => forwardedTargets.Contains(candidate.SourcePath, StringComparer.Ordinal))
                    .ToArray();
                if (forwardedCandidates.Length == 1)
                {
                    declaration = forwardedCandidates[0];
                    return true;
                }

                // Third-party packages commonly expose their root through lib/index.d.ts
                // or esm/index.d.ts and re-export its sibling declarations. Prefer the
                // first concrete entrypoint directory in package resolution order.
                foreach (var importTarget in importTargets)
                {
                    var separator = importTarget.LastIndexOf('/');
                    if (separator < 0)
                        continue;
                    var directory = importTarget[..separator] + "/";
                    var entrypointCandidate = candidates
                        .Where(candidate => candidate.SourcePath.StartsWith(directory, StringComparison.Ordinal))
                        .ToArray();
                    if (entrypointCandidate.Length == 1)
                    {
                        declaration = entrypointCandidate[0];
                        return true;
                    }
                }

                // Directory imports usually enter through index.d.ts, which may re-export
                // a declaration from type.d.ts/types.d.ts. Preserve the imported module
                // boundary instead of falling back to an unrelated same-named helper.
                var importedModules = importTargets.Select(GetModule).Distinct(StringComparer.Ordinal).ToArray();
                var moduleCandidates = candidates
                    .Where(candidate => importedModules.Contains(GetModule(candidate.SourcePath), StringComparer.Ordinal))
                    .ToArray();
                if (moduleCandidates.Length == 1)
                {
                    declaration = moduleCandidates[0];
                    return true;
                }

                var publicTypeCandidate = moduleCandidates
                    .Where(static candidate => candidate.SourcePath.EndsWith("/types.d.ts", StringComparison.Ordinal))
                    .ToArray();
                if (publicTypeCandidate.Length == 1)
                {
                    declaration = publicTypeCandidate[0];
                    return true;
                }
            }

            var exact = candidates.FirstOrDefault(candidate =>
                string.Equals(candidate.SourcePath, sourcePath, StringComparison.Ordinal));
            if (exact is not null)
            {
                declaration = exact;
                return true;
            }

            var sourceModule = GetModule(sourcePath);
            var local = candidates.Where(candidate => string.Equals(GetModule(candidate.SourcePath), sourceModule, StringComparison.Ordinal)).ToArray();
            if (local.Length == 1)
            {
                declaration = local[0];
                return true;
            }

            var common = candidates.Where(static candidate => candidate.SourcePath == "es/common.d.ts").ToArray();
            if (common.Length == 1)
            {
                declaration = common[0];
                return true;
            }

            if (candidates.Length == 1)
            {
                declaration = candidates[0];
                return true;
            }

            declaration = default!;
            return false;
        }

        private static IEnumerable<KeyValuePair<(string SourcePath, string Name), ImportedType>> ReadImports(
            string sourcePath,
            string source)
        {
            foreach (Match import in Regex.Matches(
                         source,
                         @"import\s+(?:type\s+)?(?:[A-Za-z_$][A-Za-z0-9_$]*\s*,\s*)?\{(?<names>[^}]+)\}\s+from\s+['""](?<module>[^'""]+)['""]\s*;?",
                         RegexOptions.CultureInvariant | RegexOptions.Multiline))
            {
                var targets = ResolveImportTargets(sourcePath, import.Groups["module"].Value).ToArray();
                if (targets.Length == 0)
                    continue;

                foreach (var importedName in import.Groups["names"].Value.Split(','))
                {
                    var parts = importedName.Trim().Split(" as ", StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length is 0 or > 2)
                        continue;
                    yield return new KeyValuePair<(string SourcePath, string Name), ImportedType>(
                        (sourcePath, parts[^1]),
                        new ImportedType(parts[0], targets));
                }
            }
        }

        private static IEnumerable<string> ResolveImportTargets(string sourcePath, string module)
        {
            const string selfPackagePrefix = "tdesign-vue-next/";
            if (module.StartsWith(selfPackagePrefix, StringComparison.Ordinal))
            {
                var selfTarget = module[selfPackagePrefix.Length..];
                yield return selfTarget + ".d.ts";
                yield return selfTarget + "/index.d.ts";
                yield break;
            }

            if (!module.StartsWith('.', StringComparison.Ordinal))
            {
                var externalSegments = module.Split('/', StringSplitOptions.RemoveEmptyEntries);
                var packageLength = module.StartsWith('@') ? 2 : 1;
                if (externalSegments.Length < packageLength)
                    yield break;

                var package = string.Join('/', externalSegments.Take(packageLength));
                var remainder = externalSegments.Skip(packageLength).ToArray();
                var externalTarget = "external/" + package + (remainder.Length == 0 ? string.Empty : "/" + string.Join('/', remainder));
                yield return externalTarget + ".d.ts";
                yield return externalTarget + "/index.d.ts";
                if (remainder.Length == 0)
                {
                    yield return externalTarget + "/lib/index.d.ts";
                    yield return externalTarget + "/esm/index.d.ts";
                }
                yield break;
            }

            var parts = sourcePath.Split('/').TakeWhile(static part => part.Length > 0).ToList();
            if (parts.Count > 0)
                parts.RemoveAt(parts.Count - 1);
            foreach (var segment in module.Split('/', StringSplitOptions.RemoveEmptyEntries))
            {
                if (segment == ".")
                    continue;
                if (segment == "..")
                {
                    if (parts.Count > 0)
                        parts.RemoveAt(parts.Count - 1);
                    continue;
                }
                parts.Add(segment);
            }

            var target = string.Join('/', parts);
            yield return target + ".d.ts";
            yield return target + "/index.d.ts";
        }

        private static string GetModule(string sourcePath)
        {
            const string prefix = "es/";
            if (!sourcePath.StartsWith(prefix, StringComparison.Ordinal))
                return string.Empty;

            var remainder = sourcePath[prefix.Length..];
            var slash = remainder.IndexOf('/');
            return slash < 0 ? string.Empty : remainder[..slash];
        }

        private static string ExtractAliasDefinition(string declaration)
        {
            var depth = 0;
            for (var index = 0; index < declaration.Length; index++)
            {
                depth += declaration[index] switch
                {
                    '<' or '(' or '[' or '{' => 1,
                    '>' or ')' or ']' or '}' => -1,
                    _ => 0
                };
                if (declaration[index] == '=' && depth == 0)
                    return declaration[(index + 1)..].Trim().TrimEnd(';');
            }

            throw new InvalidOperationException($"Unable to find TypeScript alias assignment: {declaration}");
        }

        private static string[] ReadInterfaceBases(string declaration)
        {
            var bodyStart = FindTopLevelCharacter(declaration, '{');
            if (bodyStart < 0)
                return [];

            var header = declaration[..bodyStart];
            var extendsIndex = FindTopLevelKeyword(header, "extends");
            if (extendsIndex < 0)
                return [];

            var bases = header[(extendsIndex + "extends".Length)..].Trim();
            var values = new List<string>();
            var start = 0;
            var depth = 0;
            for (var index = 0; index < bases.Length; index++)
            {
                switch (bases[index])
                {
                    case '<':
                    case '(':
                    case '[':
                        depth++;
                        break;
                    case '>':
                    case ')':
                    case ']':
                        depth--;
                        break;
                    case ',' when depth == 0:
                        values.Add(bases[start..index].Trim());
                        start = index + 1;
                        break;
                }
            }

            values.Add(bases[start..].Trim());
            return values.Where(static value => value.Length > 0).ToArray();
        }

        private static int FindTopLevelCharacter(string value, char sought)
        {
            var depth = 0;
            for (var index = 0; index < value.Length; index++)
            {
                switch (value[index])
                {
                    case '<':
                    case '(':
                    case '[':
                    case '{':
                        if (value[index] == sought && depth == 0)
                            return index;
                        depth++;
                        break;
                    case '>':
                    case ')':
                    case ']':
                    case '}':
                        depth--;
                        break;
                }
            }

            return -1;
        }

        private static int FindTopLevelKeyword(string value, string keyword)
        {
            var depth = 0;
            for (var index = 0; index <= value.Length - keyword.Length; index++)
            {
                switch (value[index])
                {
                    case '<':
                    case '(':
                    case '[':
                        depth++;
                        continue;
                    case '>':
                    case ')':
                    case ']':
                        depth--;
                        continue;
                }

                if (depth == 0 && value.AsSpan(index).StartsWith(keyword, StringComparison.Ordinal) &&
                    (index == 0 || !char.IsLetterOrDigit(value[index - 1])) &&
                    (index + keyword.Length == value.Length || !char.IsLetterOrDigit(value[index + keyword.Length])))
                    return index;
            }

            return -1;
        }

        public string GetInterfaceSource(TypeScriptDeclaration declaration)
        {
            if (declaration.Kind != TypeScriptDeclarationKind.Interface || declaration.BaseTypes.Length == 0)
                return declaration.Body;

            return ExpandInterfaceSource(declaration, declaration.TypeParameters.Select(static parameter => parameter.Name).ToArray(), []);
        }

        private string ExpandInterfaceSource(
            TypeScriptDeclaration declaration,
            IReadOnlyList<string> typeArguments,
            HashSet<(string SourcePath, string Name)> expansionStack)
        {
            var key = (declaration.SourcePath, declaration.Name);
            if (!expansionStack.Add(key))
                return ApplyTypeArguments(declaration.Body, declaration.TypeParameters, typeArguments);

            try
            {
                var members = new List<string>();
                var retainedBases = new List<string>();
                foreach (var baseType in declaration.BaseTypes)
                {
                    var substitutedBase = ApplyTypeArguments(baseType, declaration.TypeParameters, typeArguments);
                    var baseName = GetTypeReferenceName(substitutedBase);
                    if (baseName is not null && TryResolve(declaration.SourcePath, baseName, out var baseDeclaration) &&
                        baseDeclaration.Kind == TypeScriptDeclarationKind.Interface)
                    {
                        var baseArguments = GetTypeReferenceArguments(substitutedBase);
                        var baseSource = ExpandInterfaceSource(baseDeclaration, baseArguments, expansionStack);
                        if (TryExtractObjectMembers(baseSource, out var baseMembers))
                        {
                            members.Add(baseMembers);
                            continue;
                        }
                    }

                    // DOM and Vue runtime bases are opaque host values here. The generated
                    // record keeps the TDesign-declared structural members without inventing
                    // invalid C# record inheritance from those host classes.
                    retainedBases.Add(substitutedBase);
                }

                if (TryExtractObjectMembers(ApplyTypeArguments(declaration.Body, declaration.TypeParameters, typeArguments), out var ownMembers))
                    members.Add(ownMembers);
                else
                    return declaration.Body;

                var body = "{" + string.Join(Environment.NewLine, members.Where(static member => member.Length > 0)) + "}";
                return retainedBases.Count == 0
                    ? body
                    : string.Join(" & ", retainedBases.Append(body));
            }
            finally
            {
                expansionStack.Remove(key);
            }
        }

        private static string ApplyTypeArguments(
            string source,
            IReadOnlyList<TypeParameter> parameters,
            IReadOnlyList<string> arguments)
        {
            for (var index = 0; index < parameters.Count && index < arguments.Count; index++)
                source = Regex.Replace(source, $@"\b{Regex.Escape(parameters[index].Name)}\b", arguments[index]);
            return source;
        }

        private static bool TryExtractObjectMembers(string source, out string members)
        {
            source = source.Trim();
            if (source.Length >= 2 && source[0] == '{' && source[^1] == '}')
            {
                members = source[1..^1];
                return true;
            }

            members = string.Empty;
            return false;
        }

        private static string? GetTypeReferenceName(string source)
        {
            source = source.Trim();
            var genericStart = source.IndexOf('<');
            var name = genericStart < 0 ? source : source[..genericStart];
            return Regex.IsMatch(name, "^[A-Za-z_$][A-Za-z0-9_$]*$", RegexOptions.CultureInvariant) ? name : null;
        }

        private static string[] GetTypeReferenceArguments(string source)
        {
            var start = source.IndexOf('<');
            if (start < 0)
                return [];

            var end = source.LastIndexOf('>');
            if (end <= start)
                return [];

            return SplitTopLevel(source[(start + 1)..end], ',').ToArray();
        }

        private static IEnumerable<string> SplitTopLevel(string value, char separator)
        {
            var start = 0;
            var depth = 0;
            for (var index = 0; index < value.Length; index++)
            {
                switch (value[index])
                {
                    case '<':
                    case '(':
                    case '[':
                    case '{':
                        depth++;
                        break;
                    case '>':
                    case ')':
                    case ']':
                    case '}':
                        depth--;
                        break;
                    default:
                        if (value[index] == separator && depth == 0)
                        {
                            yield return value[start..index].Trim();
                            start = index + 1;
                        }
                        break;
                }
            }

            yield return value[start..].Trim();
        }

        private static TypeParameter[] ReadTypeParameters(Node declaration)
        {
            var parameters = declaration.NamedChildren.FirstOrDefault(static node => node.Type == "type_parameters");
            if (parameters is null)
                return [];

            return parameters.NamedChildren
                .Where(static node => node.Type == "type_parameter")
                .Select(parameter =>
                {
                    var name = parameter.NamedChildren.FirstOrDefault(static node => node.Type == "type_identifier")?.Text;
                    if (string.IsNullOrWhiteSpace(name))
                        throw new InvalidOperationException($"TypeScript type parameter has no name: {parameter.Text}");
                    var equals = IndexOfTopLevel(parameter.Text, '=');
                    return new TypeParameter(
                        name,
                        equals < 0 ? null : parameter.Text[(equals + 1)..].Trim());
                })
                .ToArray();
        }

        private static int IndexOfTopLevel(string text, char value)
        {
            var depth = 0;
            for (var index = 0; index < text.Length; index++)
            {
                switch (text[index])
                {
                    case '<':
                    case '(':
                    case '[':
                    case '{':
                        depth++;
                        break;
                    case '>':
                    case ')':
                    case ']':
                    case '}':
                        depth--;
                        break;
                    default:
                        if (text[index] == value && depth == 0)
                            return index;
                        break;
                }
            }

            return -1;
        }
    }

    sealed class TypeMapper(
        Language language,
        TypeCatalog typeCatalog,
        IReadOnlyList<TypeParameter> rootTypeParameters)
    {
        private readonly Dictionary<string, string> _definitions = new(StringComparer.Ordinal);
        private readonly Dictionary<string, MappedShape> _shapes = new(StringComparer.Ordinal);
        private readonly Dictionary<(string SourcePath, string Name), MappedType> _openNamedTypes = [];
        private readonly HashSet<(string SourcePath, string Name)> _resolvingNames = [];
        private readonly Dictionary<string, MappedType> _typeParameters = rootTypeParameters
            .ToDictionary(
                static parameter => parameter.Name,
                static parameter => new MappedType(parameter.Name, IsReference: false),
                StringComparer.Ordinal);
        // Anonymous definitions created while mapping a generic component must carry
        // that component's parameters; otherwise their fields leak unbound symbols.
        private string[] _definitionTypeParameters = rootTypeParameters.Select(static parameter => parameter.Name).ToArray();
        private string _sourcePath = string.Empty;

        public IReadOnlyCollection<string> Definitions => _definitions.Values;

        public string? LastFailure { get; private set; }

        public bool TryMap(string source, string suggestedName, string sourcePath, out MappedType type)
        {
            LastFailure = null;
            var previousSourcePath = _sourcePath;
            _sourcePath = sourcePath;
            using var parser = new Parser(language);
            using var tree = parser.Parse($"type Probe = {source};");
            var declaration = tree?.RootNode.NamedChildren.FirstOrDefault(static node => node.Type == "type_alias_declaration");
            var node = declaration?.NamedChildren.Skip(1).FirstOrDefault();
            if (node is null)
            {
                LastFailure = "Tree-sitter could not produce a type node";
                type = default!;
                _sourcePath = previousSourcePath;
                return false;
            }

            var mapped = TryMapNode(node, suggestedName, out type);
            if (!mapped && LastFailure is null)
                LastFailure = $"unsupported TypeScript node '{node.Type}' ({node.Text})";
            _sourcePath = previousSourcePath;
            return mapped;
        }

        public bool TryMapCallbackParameters(
            string source,
            string suggestedName,
            string sourcePath,
            out MappedParameter[] mappedParameters)
        {
            LastFailure = null;
            var previousSourcePath = _sourcePath;
            _sourcePath = sourcePath;
            try
            {
                using var parser = new Parser(language);
                using var tree = parser.Parse($"type Probe = {source};");
                var declaration = tree?.RootNode.NamedChildren.FirstOrDefault(static node => node.Type == "type_alias_declaration");
                var function = declaration?.NamedChildren.LastOrDefault();
                while (function?.Type == "parenthesized_type")
                    function = function.NamedChildren.SingleOrDefault();
                var parameters = function?.NamedChildren.FirstOrDefault(static child => child.Type == "formal_parameters");
                if (function?.Type != "function_type" || parameters is null)
                {
                    mappedParameters = [];
                    LastFailure = $"event contract is not a function type ({source})";
                    return false;
                }

                var mapped = new List<MappedParameter>();
                foreach (var parameter in parameters.NamedChildren)
                {
                    if (parameter.Type is not ("required_parameter" or "optional_parameter"))
                    {
                        mappedParameters = [];
                        LastFailure = $"unsupported event parameter '{parameter.Type}' ({parameter.Text})";
                        return false;
                    }

                    var name = parameter.GetChildForField("name")?.Text ??
                        parameter.NamedChildren.FirstOrDefault(static child => child.Type == "identifier")?.Text;
                    var annotation = GetTypeNode(parameter);
                    if (string.IsNullOrWhiteSpace(name) || annotation is null ||
                        !TryMapNode(annotation, suggestedName + ToCSharpName(name), out var parameterType))
                    {
                        mappedParameters = [];
                        LastFailure ??= $"event parameter '{parameter.Text}' has no mappable type";
                        return false;
                    }

                    mapped.Add(new MappedParameter(
                        name,
                        ToCSharpName(name),
                        parameterType,
                        parameter.Type == "optional_parameter"));
                }

                mappedParameters = mapped.ToArray();
                return true;
            }
            finally
            {
                _sourcePath = previousSourcePath;
            }
        }

        private bool TryMapNode(Node node, string suggestedName, out MappedType type)
        {
            if (node.Type is "parenthesized_type" or "type_annotation" or "opting_type_annotation")
                return TryMapNode(node.NamedChildren.Single(), suggestedName, out type);

            if (node.Type == "predefined_type")
            {
                type = node.Text switch
                {
                    "boolean" => new MappedType("bool", IsReference: false),
                    "number" => new MappedType("Number", IsReference: false),
                    "string" => new MappedType("string", IsReference: true),
                    "bigint" => new MappedType("BigInt", IsReference: false),
                    _ => default!
                };
                if (type is not null)
                    return true;

                if (node.Text is "any" or "unknown")
                    return AddJsonValue(out type);
                if (node.Text == "object")
                    return AddJsonObject(out type);
                return false;
            }

            if (node.Type == "literal_type")
            {
                if (TryGetStringLiteral(node, out var literal))
                {
                    AddStringEnum(suggestedName, [literal]);
                    type = new MappedType(suggestedName, IsReference: false);
                    return true;
                }

                if (double.TryParse(node.Text, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out _))
                {
                    type = new MappedType("Number", IsReference: false);
                    return true;
                }

                if (node.Text is "true" or "false")
                {
                    type = new MappedType("bool", IsReference: false);
                    return true;
                }
            }

            if (node.Type == "type_identifier" && node.Text == "TNode")
            {
                type = new MappedType("RenderFragment", IsReference: true);
                return true;
            }

            if (node.Type == "type_identifier" && node.Text == "Object")
                return AddJsonObject(out type);

            if (node.Type == "nested_type_identifier" && node.Text == "JSX.Element")
            {
                type = new MappedType("RenderFragment", IsReference: true);
                return true;
            }

            if (node.Type == "type_identifier")
                return TryMapNamedType(node.Text, suggestedName, out type);

            if (node.Type == "union_type")
                return TryMapUnion(node, suggestedName, out type);

            if (node.Type == "intersection_type")
                return TryMapIntersection(node, suggestedName, out type);

            if (node.Type == "array_type")
            {
                if (!TryMapNode(node.NamedChildren.Single(), suggestedName + "Item", out var item))
                {
                    type = default!;
                    return false;
                }

                type = new MappedType(item.Name + "[]", IsReference: true, item.IsNullable);
                return true;
            }

            if (node.Type == "generic_type")
                return TryMapGeneric(node, suggestedName, out type);

            if (node.Type == "object_type")
                return TryMapObject(node, suggestedName, out type);

            if (node.Type == "function_type")
                return TryMapFunction(node, suggestedName, out type);

            if (node.Type == "tuple_type")
                return TryMapTuple(node, suggestedName, out type);

            if (node.Type == "mapped_type")
                return TryMapMappedType(node, suggestedName, out type);

            if (node.Type == "lookup_type")
                return TryMapLookupType(node, suggestedName, out type);

            if (node.Type == "literal_type" && TryGetStringLiteral(node, out var stringLiteral))
            {
                AddStringEnum(suggestedName, [stringLiteral]);
                type = new MappedType(suggestedName, IsReference: false);
                return true;
            }

            type = default!;
            LastFailure ??= $"unsupported TypeScript node '{node.Type}' ({node.Text})";
            return false;
        }

        private bool TryMapNamedType(string name, string suggestedName, out MappedType type)
        {
            if (_typeParameters.TryGetValue(name, out var typeParameter))
            {
                type = typeParameter!;
                return true;
            }

            if (name == "ClassName")
                return AddClassName(out type);

            if (name == "ComponentType")
            {
                type = new MappedType("IVueComponent", IsReference: true);
                return true;
            }

            if (name == "Function")
            {
                AddDefinition(
                    "TCallback",
                    $"[ECMAScript]{Environment.NewLine}public delegate void TCallback();{Environment.NewLine}");
                type = new MappedType("TCallback", IsReference: true);
                return true;
            }

            if (name == "Sortable" && _sourcePath.StartsWith("external/sortablejs/", StringComparison.Ordinal))
            {
                // Sortable's class declaration is only referenced as a callback handle
                // in TDesign's options contract. Its callable runtime API is not a prop.
                AddDefinition(
                    "TSortable",
                    $"[ECMAScript]{Environment.NewLine}public sealed record TSortable : VueProps{Environment.NewLine}{{{Environment.NewLine}}}{Environment.NewLine}");
                type = new MappedType("TSortable", IsReference: true);
                return true;
            }

            if (name == "Dayjs" && _sourcePath.StartsWith("es/", StringComparison.Ordinal))
            {
                AddDefinition(
                    "TDayjs",
                    $"[ECMAScript]{Environment.NewLine}public sealed record TDayjs : VueProps{Environment.NewLine}{{{Environment.NewLine}}}{Environment.NewLine}");
                type = new MappedType("TDayjs", IsReference: true);
                return true;
            }

            if (TryMapPlatformType(name, out type))
                return true;

            if (!typeCatalog.TryResolve(_sourcePath, name, out var declaration))
            {
                type = default!;
                LastFailure ??= $"cannot resolve named TypeScript declaration '{name}' from {_sourcePath}";
                return false;
            }

            return TryMapNamedDeclaration(declaration, [], suggestedName, out type);
        }

        private bool TryMapNamedDeclaration(
            TypeScriptDeclaration declaration,
            IReadOnlyList<Node> argumentNodes,
            string suggestedName,
            out MappedType type)
        {
            var mappedArguments = new List<MappedType>();
            if (argumentNodes.Count > 0)
            {
                if (argumentNodes.Count != declaration.TypeParameters.Length)
                {
                    type = default!;
                    LastFailure ??= $"generic declaration '{declaration.Name}' requires {declaration.TypeParameters.Length} type arguments";
                    return false;
                }

                for (var index = 0; index < argumentNodes.Count; index++)
                {
                    if (!TryMapNode(argumentNodes[index], suggestedName + declaration.TypeParameters[index].Name, out var argument))
                    {
                        type = default!;
                        return false;
                    }

                    mappedArguments.Add(argument);
                }
            }
            else
            {
                // A default argument belongs to the declaration contract, not to the
                // generic component currently requesting it. Do not capture the outer
                // component parameters in helper definitions for the default value.
                var previousDefinitionParameters = _definitionTypeParameters;
                _definitionTypeParameters = [];
                try
                {
                    foreach (var parameter in declaration.TypeParameters)
                    {
                        if (parameter.DefaultSource is null ||
                            !TryMap(parameter.DefaultSource, suggestedName + parameter.Name + "Default", declaration.SourcePath, out var argument))
                        {
                            type = default!;
                            LastFailure ??= $"generic declaration '{declaration.Name}' requires an explicit argument for {parameter.Name}";
                            return false;
                        }

                        mappedArguments.Add(argument);
                    }
                }
                finally
                {
                    _definitionTypeParameters = previousDefinitionParameters;
                }
            }

            var key = (declaration.SourcePath, declaration.Name);
            if (!_openNamedTypes.TryGetValue(key, out var openType))
            {
                if (_resolvingNames.Contains(key))
                {
                    type = new MappedType(
                        CloseGeneric(ToGeneratedTypeName(declaration.Name), declaration.TypeParameters, mappedArguments),
                        IsReference: true);
                    return true;
                }

                _resolvingNames.Add(key);
                var previousParameters = _typeParameters.ToArray();
                var previousDefinitionParameters = _definitionTypeParameters;
                try
                {
                    _typeParameters.Clear();
                    foreach (var parameter in declaration.TypeParameters)
                        _typeParameters.Add(parameter.Name, new MappedType(parameter.Name, IsReference: false));
                    _definitionTypeParameters = declaration.TypeParameters.Select(static parameter => parameter.Name).ToArray();

                    var csharpName = ToGeneratedTypeName(declaration.Name);
                    if (declaration.Kind == TypeScriptDeclarationKind.Class)
                    {
                        // Runtime classes in declaration-only implementation details are
                        // passed by reference. Model their handle without inventing APIs.
                        AddDefinition(
                            csharpName,
                            $"[ECMAScript]{Environment.NewLine}public sealed record {DeclaredName(csharpName)} : VueProps{Environment.NewLine}{{{Environment.NewLine}}}{Environment.NewLine}");
                        openType = new MappedType(DeclaredName(csharpName), IsReference: true);
                    }
                    else
                    {
                        var source = declaration.Kind == TypeScriptDeclarationKind.Interface
                            ? typeCatalog.GetInterfaceSource(declaration)
                            : declaration.Definition;
                        if (!TryMap(source, csharpName, declaration.SourcePath, out openType))
                        {
                            type = default!;
                            LastFailure ??= $"unable to map declaration '{declaration.Name}' from {declaration.SourcePath}";
                            return false;
                        }
                    }

                    _openNamedTypes.Add(key, openType);
                }
                finally
                {
                    _typeParameters.Clear();
                    foreach (var parameter in previousParameters)
                        _typeParameters.Add(parameter.Key, parameter.Value);
                    _definitionTypeParameters = previousDefinitionParameters;
                    _resolvingNames.Remove(key);
                }
            }

            type = declaration.TypeParameters.Length == 0
                ? openType
                : openType with
                {
                    Name = CloseGeneric(openType.Name, declaration.TypeParameters, mappedArguments)
                };
            return true;
        }

        private static string CloseGeneric(
            string openName,
            IReadOnlyList<TypeParameter> parameters,
            IReadOnlyList<MappedType> arguments)
        {
            if (parameters.Count == 0)
                return openName;

            if (!openName.Contains('<', StringComparison.Ordinal))
                return openName + "<" + string.Join(", ", arguments.Select(static argument => argument.Name)) + ">";

            var result = openName;
            for (var index = 0; index < parameters.Count; index++)
            {
                result = Regex.Replace(
                    result,
                    $@"\b{Regex.Escape(parameters[index].Name)}\b",
                    arguments[index].Name);
            }

            return result;
        }

        private static bool TryMapPlatformType(string name, out MappedType type)
        {
            type = name switch
            {
                "bigint" => new MappedType("BigInt", IsReference: false),
                // Browser File is exposed as ECMAScript.JazorFile to avoid colliding with
                // System.IO.File in consumer projects; its JavaScript ABI remains File.
                // 浏览器 File 在作者侧使用 ECMAScript.JazorFile，避免与 consumer 项目的
                // System.IO.File 冲突；JavaScript ABI 仍然是 File。
                "File" => new MappedType("JazorFile", IsReference: true),
                "Document" => new MappedType("JazorDocument", IsReference: true),
                "Window" => new MappedType("JazorWindow", IsReference: true),
                "Event" => new MappedType("JazorEvent", IsReference: true),
                "History" => new MappedType("JazorHistory", IsReference: true),
                "Location" => new MappedType("JazorLocation", IsReference: true),
                "Date" or "Blob" or "FormData" or "HTMLElement" or "Element" or
                    "MouseEvent" or "KeyboardEvent" or "WheelEvent" or "DragEvent" or "ProgressEvent" or
                    "TouchEvent" or "FocusEvent" or "ClipboardEvent" or "InputEvent" or "CompositionEvent" or
                    "TransitionEvent" or "DataTransfer" or "XMLHttpRequest" or "Error" or "RegExp" => new MappedType(name, IsReference: true),
                _ => default!
            };
            return type is not null;
        }

        private static string ToGeneratedTypeName(string sourceName)
            => sourceName.StartsWith("Td", StringComparison.Ordinal) ||
               sourceName.Length > 1 && sourceName[0] == 'T' && char.IsUpper(sourceName[1])
                ? sourceName
                : "T" + sourceName;

        private string DeclaredName(string name)
            => _definitionTypeParameters.Length == 0
                ? name
                : name + "<" + string.Join(", ", _definitionTypeParameters) + ">";

        private bool TryMapUnion(Node node, string suggestedName, out MappedType type)
        {
            var branches = FlattenUnion(node).ToArray();
            var nullable = branches.Any(IsNullish);
            var nonNullish = branches.Where(branch => !IsNullish(branch)).ToArray();
            if (nonNullish.Length == 0)
            {
                type = default!;
                return false;
            }

            if (nonNullish.All(branch => TryGetStringLiteral(branch, out _)))
            {
                AddStringEnum(suggestedName, nonNullish.Select(branch =>
                {
                    TryGetStringLiteral(branch, out var value);
                    return value;
                }).ToArray());
                type = new MappedType(suggestedName, IsReference: false, nullable);
                return true;
            }

            var mapped = new List<MappedType>();
            for (var index = 0; index < nonNullish.Length; index++)
            {
                if (!TryMapNode(nonNullish[index], suggestedName + "Option" + (index + 1), out var branch))
                {
                    type = default!;
                    return false;
                }

                if (!mapped.Any(candidate => string.Equals(candidate.Name, branch.Name, StringComparison.Ordinal)))
                    mapped.Add(branch);
            }

            if (mapped.Count == 1)
            {
                type = mapped[0] with { IsNullable = nullable || mapped[0].IsNullable };
                return true;
            }

            if (mapped.Any(static branch => branch.IsNullable))
            {
                type = default!;
                return false;
            }

            AddNativeUnion(suggestedName, mapped);
            type = new MappedType(DeclaredName(suggestedName), IsReference: false, nullable);
            return true;
        }

        private bool TryMapIntersection(Node node, string suggestedName, out MappedType type)
        {
            var branches = node.NamedChildren.Where(static branch => !IsOpenExtension(branch)).ToArray();
            if (branches.Length == 0)
                return AddObjectShape(suggestedName, out type);
            if (branches.Length == 1)
                return TryMapNode(branches[0], suggestedName, out type);

            var objectBranch = branches.SingleOrDefault(static branch => branch.Type == "object_type");
            var inheritedBranches = branches.Where(static branch => branch.Type != "object_type").ToArray();
            if (objectBranch is not null && inheritedBranches.Length > 0)
            {
                var inheritedProperties = new List<MappedShapeProperty>();
                for (var index = 0; index < inheritedBranches.Length; index++)
                {
                    if (!TryMapNode(inheritedBranches[index], suggestedName + "Base" + (index + 1), out var mappedBase))
                    {
                        type = default!;
                        return false;
                    }

                    if (TryGetShape(mappedBase, out var properties))
                    {
                        inheritedProperties.AddRange(properties);
                    }
                }

                // TypeScript interface extension is structural. Flattening avoids record
                // inheritance from sealed Pick/Omit projections and keeps members visible
                // in the generated C# authoring contract.
                return TryMapObject(
                    objectBranch,
                    suggestedName,
                    out type,
                    baseType: null,
                    flattenedBaseProperties: inheritedProperties.Count == 0 ? null : inheritedProperties);
            }

            type = default!;
            LastFailure ??= $"intersection requires an object body and structural bases ({node.Text})";
            return false;
        }

        private bool TryMapTuple(Node node, string suggestedName, out MappedType type)
        {
            var values = new List<MappedType>();
            for (var index = 0; index < node.NamedChildren.Count; index++)
            {
                if (!TryMapNode(node.NamedChildren[index], suggestedName + "Item" + (index + 1), out var value))
                {
                    type = default!;
                    return false;
                }

                values.Add(value);
            }

            if (values.Count is < 1 or > 7)
            {
                type = default!;
                LastFailure ??= $"tuple arity {values.Count} is outside the supported C# tuple contract";
                return false;
            }

            type = new MappedType(
                "(" + string.Join(", ", values.Select((value, index) =>
                    value.Name + (value.IsNullable ? "?" : string.Empty) + " Item" + (index + 1))) + ")",
                IsReference: false);
            return true;
        }

        private bool TryMapMappedType(Node node, string suggestedName, out MappedType type)
        {
            var annotation = GetTypeNode(node);
            if (annotation is null || !TryMapNode(annotation, suggestedName + "Value", out var value))
            {
                type = default!;
                LastFailure ??= $"mapped type has no mappable value ({node.Text})";
                return false;
            }

            AddDefinition(
                suggestedName,
                $"[ECMAScript]{Environment.NewLine}public sealed record {DeclaredName(suggestedName)} : VueDictionary<{value.Name}>{Environment.NewLine}{{{Environment.NewLine}}}{Environment.NewLine}");
            type = new MappedType(DeclaredName(suggestedName), IsReference: true);
            return true;
        }

        private bool TryMapLookupType(Node node, string suggestedName, out MappedType type)
        {
            var baseType = node.NamedChildren.FirstOrDefault();
            var member = node.NamedChildren.Skip(1).FirstOrDefault();
            if (baseType?.Type == "type_identifier" && member is not null && TryGetStringLiteral(member, out var memberName) &&
                typeCatalog.TryResolve(_sourcePath, baseType.Text, out var memberDeclaration) &&
                TryFindPropertyType(memberDeclaration, memberName, out var propertyType))
            {
                return TryMap(propertyType, suggestedName, memberDeclaration.SourcePath, out type);
            }

            if (baseType?.Type == "type_identifier" && node.Text.EndsWith("[number]", StringComparison.Ordinal) &&
                typeCatalog.TryResolve(_sourcePath, baseType.Text, out var declaration))
            {
                using var parser = new Parser(language);
                using var tree = parser.Parse($"type Probe = {declaration.Definition};");
                var tuple = tree?.RootNode.NamedChildren
                    .FirstOrDefault(static candidate => candidate.Type == "type_alias_declaration")?
                    .NamedChildren
                    .LastOrDefault();
                if (tuple?.Type == "tuple_type" && tuple.NamedChildren.All(branch => TryGetStringLiteral(branch, out _)))
                {
                    var values = tuple.NamedChildren.Select(branch =>
                    {
                        TryGetStringLiteral(branch, out var value);
                        return value;
                    }).ToArray();
                    AddStringEnum(suggestedName, values);
                    type = new MappedType(suggestedName, IsReference: false);
                    return true;
                }
            }

            type = default!;
            LastFailure ??= $"unsupported lookup type '{node.Text}'";
            return false;
        }

        private bool TryFindPropertyType(
            TypeScriptDeclaration declaration,
            string propertyName,
            out string propertyType)
        {
            var source = declaration.Kind == TypeScriptDeclarationKind.Interface
                ? declaration.Body
                : declaration.Definition;
            using var parser = new Parser(language);
            using var tree = parser.Parse($"type Probe = {source};");
            var objectType = tree?.RootNode.NamedChildren
                .FirstOrDefault(static candidate => candidate.Type == "type_alias_declaration")?
                .NamedChildren
                .LastOrDefault();
            var property = objectType?.NamedChildren.FirstOrDefault(candidate =>
                candidate.Type == "property_signature" &&
                string.Equals(candidate.GetChildForField("name")?.Text.Trim('\'', '"'), propertyName, StringComparison.Ordinal));
            var annotation = property is null ? null : GetTypeNode(property);
            propertyType = annotation?.Text ?? string.Empty;
            return annotation is not null;
        }

        private bool TryMapGeneric(Node node, string suggestedName, out MappedType type)
        {
            var identifier = node.NamedChildren.FirstOrDefault(static child => child.Type == "type_identifier")?.Text;
            var arguments = node.NamedChildren.FirstOrDefault(static child => child.Type == "type_arguments")?.NamedChildren.ToArray();
            if (identifier is null || arguments is null)
            {
                type = default!;
                return false;
            }

            if (identifier is "Array" or "ReadonlyArray")
            {
                if (arguments.Length != 1 || !TryMapNode(arguments[0], suggestedName + "Item", out var item))
                {
                    type = default!;
                    return false;
                }

                type = new MappedType(item.Name + "[]", IsReference: true, item.IsNullable);
                return true;
            }

            if (identifier == "TNode")
            {
                if (arguments.Length == 0)
                {
                    type = new MappedType("RenderFragment", IsReference: true);
                    return true;
                }

                if (arguments.Length == 1 && TryMapNode(arguments[0], suggestedName + "Context", out var context))
                {
                    type = new MappedType($"RenderFragment<{context.Name}>", IsReference: true, context.IsNullable);
                    return true;
                }
            }

            if (identifier is "Promise" or "IPromise")
            {
                if (arguments.Length == 1 && TryMapNode(arguments[0], suggestedName + "Result", out var result))
                {
                    type = new MappedType($"IPromise<{result.Name}>", IsReference: true, result.IsNullable);
                    return true;
                }

                type = default!;
                LastFailure ??= $"{identifier} requires one result type";
                return false;
            }

            if (identifier == "Record")
            {
                if (arguments.Length == 2 && TryMapNode(arguments[1], suggestedName + "Value", out var value))
                {
                    type = new MappedType($"VueDictionary<{value.Name}>", IsReference: true, value.IsNullable);
                    return true;
                }

                type = default!;
                LastFailure ??= "Record requires key and value type arguments";
                return false;
            }

            if (identifier == "Partial")
            {
                if (arguments.Length == 1 && TryMapNode(arguments[0], suggestedName + "Partial", out type))
                    return true;

                type = default!;
                LastFailure ??= "Partial requires one object type argument";
                return false;
            }

            if (identifier is "Pick" or "Omit")
            {
                return TryMapProjection(identifier, arguments, suggestedName, out type);
            }

            if (typeCatalog.TryResolve(_sourcePath, identifier, out var declaration))
                return TryMapNamedDeclaration(declaration, arguments, suggestedName, out type);

            type = default!;
            LastFailure ??= $"cannot resolve generic TypeScript declaration '{identifier}' from {_sourcePath}";
            return false;
        }

        private bool TryMapProjection(
            string utility,
            IReadOnlyList<Node> arguments,
            string suggestedName,
            out MappedType type)
        {
            if (arguments.Count != 2 ||
                !TryMapNode(arguments[0], suggestedName + "Source", out var sourceType) ||
                !TryReadProjectionKeys(arguments[1], out var keys) ||
                !TryGetShape(sourceType, out var sourceProperties))
            {
                type = default!;
                LastFailure ??= $"{utility} requires a resolvable object shape and string literal keys";
                return false;
            }

            var sourceByName = sourceProperties.ToDictionary(static property => property.SourceName, StringComparer.Ordinal);
            if (utility == "Pick" && keys.Any(key => !sourceByName.ContainsKey(key)))
            {
                type = default!;
                LastFailure ??= $"Pick references keys absent from {sourceType.Name}: {string.Join(", ", keys.Where(key => !sourceByName.ContainsKey(key)))}";
                return false;
            }

            var projected = sourceProperties
                .Where(property => utility == "Pick" ? keys.Contains(property.SourceName) : !keys.Contains(property.SourceName))
                .ToArray();
            AddProjectedShape(suggestedName, projected);
            type = new MappedType(DeclaredName(suggestedName), IsReference: true);
            return true;
        }

        private bool TryReadProjectionKeys(Node node, out HashSet<string> keys)
        {
            keys = new HashSet<string>(StringComparer.Ordinal);
            if (TryGetStringLiteral(node, out var key))
            {
                keys.Add(key);
                return true;
            }

            if (node.Type == "union_type")
            {
                foreach (var branch in FlattenUnion(node))
                {
                    if (!TryGetStringLiteral(branch, out key))
                    {
                        keys.Clear();
                        return false;
                    }

                    keys.Add(key);
                }

                return keys.Count > 0;
            }

            if (node.Type != "type_identifier" || !typeCatalog.TryResolve(_sourcePath, node.Text, out var declaration) ||
                declaration.Kind != TypeScriptDeclarationKind.TypeAlias)
                return false;

            var previousSourcePath = _sourcePath;
            _sourcePath = declaration.SourcePath;
            try
            {
                using var parser = new Parser(language);
                using var tree = parser.Parse($"type Probe = {declaration.Definition};");
                var value = tree?.RootNode.NamedChildren
                    .FirstOrDefault(static candidate => candidate.Type == "type_alias_declaration")?
                    .NamedChildren
                    .LastOrDefault();
                return value is not null && TryReadProjectionKeys(value, out keys);
            }
            finally
            {
                _sourcePath = previousSourcePath;
            }
        }

        private bool TryGetShape(MappedType type, out MappedShapeProperty[] properties)
        {
            var genericStart = type.Name.IndexOf('<');
            var name = genericStart < 0 ? type.Name : type.Name[..genericStart];
            if (!_shapes.TryGetValue(name, out var shape))
            {
                properties = [];
                return false;
            }

            if (shape.TypeParameters.Length == 0)
            {
                properties = shape.Properties;
                return true;
            }

            var arguments = genericStart < 0
                ? []
                : SplitTypeArguments(type.Name[(genericStart + 1)..^1]).ToArray();
            if (arguments.Length != shape.TypeParameters.Length)
            {
                properties = [];
                return false;
            }

            properties = shape.Properties.Select(property => property with
            {
                Type = property.Type with
                {
                    Name = ReplaceTypeParameters(property.Type.Name, shape.TypeParameters, arguments)
                }
            }).ToArray();
            return true;
        }

        private void AddProjectedShape(string name, IReadOnlyList<MappedShapeProperty> properties)
        {
            var builder = new StringBuilder();
            builder.AppendLine("[ECMAScript]");
            builder.AppendLine($"public sealed record {DeclaredName(name)} : VueProps");
            builder.AppendLine("{");
            foreach (var property in properties)
            {
                if (!string.Equals(property.SourceName, property.CSharpName, StringComparison.Ordinal))
                    builder.AppendLine($"    [ECMAScriptName(\"{property.SourceName}\")]");
                if (!property.Optional && property.Type.IsReference && !property.Type.IsNullable)
                    builder.AppendLine("    [EditorRequired]");
                builder.AppendLine($"    public {property.Type.Name}{(property.Optional || property.Type.IsNullable ? "?" : string.Empty)} {CSharpIdentifier.Escape(property.CSharpName)} {{ get; init; }}{(!property.Optional && !property.Type.IsNullable ? " = default!;" : string.Empty)}");
                builder.AppendLine();
            }

            if (properties.Count > 0)
                builder.Length -= Environment.NewLine.Length;
            builder.AppendLine("}");
            AddDefinition(name, builder.ToString());
            _shapes[name] = new MappedShape(_definitionTypeParameters, properties.ToArray());
        }

        private static IEnumerable<string> SplitTypeArguments(string text)
        {
            var start = 0;
            var depth = 0;
            for (var index = 0; index < text.Length; index++)
            {
                depth += text[index] switch { '<' or '(' or '[' or '{' => 1, '>' or ')' or ']' or '}' => -1, _ => 0 };
                if (text[index] != ',' || depth != 0)
                    continue;
                yield return text[start..index].Trim();
                start = index + 1;
            }

            yield return text[start..].Trim();
        }

        private static string ReplaceTypeParameters(string value, IReadOnlyList<string> parameters, IReadOnlyList<string> arguments)
        {
            for (var index = 0; index < parameters.Count; index++)
                value = Regex.Replace(value, $@"\b{Regex.Escape(parameters[index])}\b", arguments[index]);
            return value;
        }

        private bool TryMapObject(
            Node node,
            string suggestedName,
            out MappedType type,
            MappedType? baseType = null,
            IReadOnlyList<MappedShapeProperty>? flattenedBaseProperties = null)
        {
            var properties = new List<MappedShapeProperty>();
            MappedType? indexValue = null;
            var hasOpenIndex = false;
            foreach (var property in node.NamedChildren)
            {
                if (property.Type == "comment")
                    continue;

                if (property.Type == "index_signature")
                {
                    var indexAnnotation = GetTypeNode(property);
                    // `any` only appears in TDesign's open extension dictionaries. The
                    // declared named fields remain strongly typed; exposing it as object
                    // or VueValue would weaken every component that composes the shape.
                    if (indexAnnotation?.Text is "any" or "unknown")
                    {
                        hasOpenIndex = true;
                        continue;
                    }
                    if (indexAnnotation is null || !TryMapNode(indexAnnotation, suggestedName + "Index", out var value))
                    {
                        type = default!;
                        return false;
                    }

                    indexValue = value;
                    continue;
                }

                if (property.Type == "mapped_type" && properties.Count == 0 && indexValue is null)
                    return TryMapMappedType(property, suggestedName, out type);

                if (property.Type != "property_signature")
                {
                    type = default!;
                    LastFailure ??= $"unsupported object member '{property.Type}' ({property.Text})";
                    return false;
                }

                var sourceName = property.GetChildForField("name")?.Text.Trim('\'', '"');
                var annotation = GetTypeNode(property);
                if (string.IsNullOrWhiteSpace(sourceName) || annotation is null ||
                    !TryMapNode(annotation, suggestedName + ToCSharpName(sourceName), out var propertyType))
                {
                    type = default!;
                    return false;
                }

                var optional = property.Children.Any(static child => child.Type == "?");
                properties.Add(new MappedShapeProperty(sourceName, ToCSharpName(sourceName), propertyType, optional));
            }

            var ownProperties = properties
                .GroupBy(static property => property.SourceName, StringComparer.Ordinal)
                .Select(static group => group.Last())
                .ToArray();

            if (hasOpenIndex && ownProperties.Length == 0 && indexValue is null && baseType is null)
                return AddJsonObject(out type);

            var inheritedProperties = flattenedBaseProperties ?? (baseType is not null && TryGetShape(baseType, out var inherited)
                ? inherited
                : []);
            var allProperties = inheritedProperties
                .Concat(ownProperties)
                .GroupBy(static property => property.SourceName, StringComparer.Ordinal)
                .Select(static group => group.Last())
                .ToArray();
            var declaredProperties = baseType is null && flattenedBaseProperties is not null
                ? allProperties
                : ownProperties;

            var builder = new StringBuilder();
            builder.AppendLine("[ECMAScript]");
            builder.AppendLine($"public record {DeclaredName(suggestedName)} : {baseType?.Name ?? (indexValue is null ? "VueProps" : $"VueDictionary<{indexValue.Name}>")}");
            builder.AppendLine("{");
            foreach (var property in declaredProperties)
            {
                var requiresInitialization = !property.Optional && !property.Type.IsNullable &&
                    (property.Type.IsReference || _definitionTypeParameters.Contains(property.Type.Name, StringComparer.Ordinal));
                if (!string.Equals(property.SourceName, property.CSharpName, StringComparison.Ordinal))
                    builder.AppendLine($"    [ECMAScriptName(\"{property.SourceName}\")]");
                if (!property.Optional && property.Type.IsReference && !property.Type.IsNullable)
                    builder.AppendLine("    [EditorRequired]");
                builder.AppendLine($"    public {property.Type.Name}{(property.Optional || property.Type.IsNullable ? "?" : string.Empty)} {CSharpIdentifier.Escape(property.CSharpName)} {{ get; init; }}{(requiresInitialization ? " = default!;" : string.Empty)}");
                builder.AppendLine();
            }

            if (declaredProperties.Any())
                builder.Length -= Environment.NewLine.Length;
            builder.AppendLine("}");
            AddDefinition(suggestedName, builder.ToString());
            _shapes[suggestedName] = new MappedShape(_definitionTypeParameters, allProperties);
            type = new MappedType(DeclaredName(suggestedName), IsReference: true);
            return true;
        }

        private bool AddObjectShape(string suggestedName, out MappedType type)
        {
            AddDefinition(
                suggestedName,
                $"[ECMAScript]{Environment.NewLine}public sealed record {DeclaredName(suggestedName)} : VueProps{Environment.NewLine}{{{Environment.NewLine}}}{Environment.NewLine}");
            _shapes[suggestedName] = new MappedShape(_definitionTypeParameters, []);
            type = new MappedType(DeclaredName(suggestedName), IsReference: true);
            return true;
        }

        private bool AddJsonObject(out MappedType type)
        {
            AddJsonDefinitions();
            type = new MappedType("TJsonObject", IsReference: true);
            return true;
        }

        private bool AddJsonValue(out MappedType type)
        {
            AddJsonDefinitions();
            type = new MappedType("TJsonValue", IsReference: false);
            return true;
        }

        private void AddJsonDefinitions()
        {
            AddDefinition(
                "TJsonValue",
                $"[ECMAScript]{Environment.NewLine}public readonly union TJsonValue(bool, Number, string, TJsonValue[], TJsonObject){Environment.NewLine}{{{Environment.NewLine}}}{Environment.NewLine}");
            AddDefinition(
                "TJsonObject",
                $"[ECMAScript]{Environment.NewLine}public sealed record TJsonObject : VueDictionary<TJsonValue>{Environment.NewLine}{{{Environment.NewLine}}}{Environment.NewLine}");
        }

        private bool AddClassName(out MappedType type)
        {
            const string className = "TClassName";
            const string dictionary = "TClassNameDictionary";
            AddDefinition(
                dictionary,
                $"[ECMAScript]{Environment.NewLine}public sealed record {dictionary} : VueDictionary<bool>{Environment.NewLine}{{{Environment.NewLine}}}{Environment.NewLine}");
            AddDefinition(
                className,
                $"[ECMAScript]{Environment.NewLine}public readonly union {className}({dictionary}, {className}[], string){Environment.NewLine}{{{Environment.NewLine}}}{Environment.NewLine}");
            type = new MappedType(className, IsReference: false);
            return true;
        }

        private bool TryMapFunction(Node node, string suggestedName, out MappedType type)
        {
            var parameters = node.NamedChildren.FirstOrDefault(static child => child.Type == "formal_parameters");
            var returnNode = node.NamedChildren.LastOrDefault();
            if (parameters is null)
            {
                type = default!;
                LastFailure ??= $"function type has no formal parameter node ({node.Text})";
                return false;
            }

            if (returnNode is null || !TryMapReturn(returnNode, suggestedName + "Result", out var returnType))
            {
                type = default!;
                LastFailure ??= $"function return type is unsupported ({returnNode?.Text ?? "missing"})";
                return false;
            }

            var mappedParameters = new List<string>();
            foreach (var parameter in parameters.NamedChildren)
            {
                if (parameter.Type is not ("required_parameter" or "optional_parameter"))
                {
                    type = default!;
                    LastFailure ??= $"unsupported function parameter '{parameter.Type}' ({parameter.Text})";
                    return false;
                }

                var name = parameter.GetChildForField("name")?.Text ??
                    parameter.NamedChildren.FirstOrDefault(static child => child.Type == "identifier")?.Text;
                if (string.Equals(name, "this", StringComparison.Ordinal) ||
                    parameter.Text.TrimStart().StartsWith("this:", StringComparison.Ordinal))
                {
                    // TypeScript's `this: T` is a compile-time receiver annotation, not
                    // a JavaScript callback argument and therefore has no C# delegate slot.
                    continue;
                }

                var annotation = GetTypeNode(parameter);
                if (string.IsNullOrWhiteSpace(name) || annotation is null ||
                    !TryMapNode(annotation, suggestedName + ToCSharpName(name), out var parameterType))
                {
                    type = default!;
                    LastFailure ??= $"function parameter '{parameter.Text}' has no mappable type";
                    return false;
                }

                var optional = parameter.Type == "optional_parameter";
                mappedParameters.Add($"{parameterType.Name}{(optional || parameterType.IsNullable ? "?" : string.Empty)} {CSharpIdentifier.Escape(name)}{(optional ? " = default" : string.Empty)}");
            }

            AddDefinition(suggestedName, $"[ECMAScript]{Environment.NewLine}public delegate {returnType} {DeclaredName(suggestedName)}({string.Join(", ", mappedParameters)});{Environment.NewLine}");
            type = new MappedType(DeclaredName(suggestedName), IsReference: true);
            return true;
        }

        private bool TryMapReturn(Node node, string suggestedName, out string type)
        {
            if (node.Type == "predefined_type" && node.Text == "void")
            {
                type = "void";
                return true;
            }

            if (TryMapNode(node, suggestedName, out var result))
            {
                type = result.Name + (result.IsNullable ? "?" : string.Empty);
                return true;
            }

            type = string.Empty;
            return false;
        }

        private void AddNativeUnion(string name, IReadOnlyList<MappedType> branches)
        {
            if (HasAssignableBranchOverlap(branches))
            {
                AddTaggedUnion(name, branches);
                return;
            }

            AddDefinition(
                name,
                $"[ECMAScript]{Environment.NewLine}public readonly union {DeclaredName(name)}({string.Join(", ", branches.Select(static branch => branch.Name))}){Environment.NewLine}{{{Environment.NewLine}}}{Environment.NewLine}");
        }

        private static bool HasAssignableBranchOverlap(IReadOnlyList<MappedType> branches)
        {
            for (var leftIndex = 0; leftIndex < branches.Count; leftIndex++)
            {
                if (!TryResolveRuntimeType(branches[leftIndex].Name, out var left))
                    continue;

                for (var rightIndex = 0; rightIndex < branches.Count; rightIndex++)
                {
                    if (leftIndex == rightIndex || !TryResolveRuntimeType(branches[rightIndex].Name, out var right))
                        continue;

                    if (left.IsAssignableFrom(right))
                        return true;
                }
            }

            return false;
        }

#pragma warning disable IL2026, IL3050 // This source generator executes on the maintainer's full build host, never in a trimmed application.
        private static bool TryResolveRuntimeType(string typeName, out Type type)
        {
            typeName = typeName.TrimEnd('?');
            if (typeName.EndsWith("[]", StringComparison.Ordinal) &&
                TryResolveRuntimeType(typeName[..^2], out var elementType))
            {
                type = elementType.MakeArrayType();
                return true;
            }

            if (typeName.Contains('<', StringComparison.Ordinal))
            {
                type = default!;
                return false;
            }

            type = typeof(ECMAScript.Element).Assembly.GetType($"ECMAScript.{typeName}", throwOnError: false)!;
            return type is not null;
        }
#pragma warning restore IL2026, IL3050

        private void AddTaggedUnion(string name, IReadOnlyList<MappedType> branches)
        {
            var typeName = DeclaredName(name);
            var genericStart = typeName.IndexOf('<');
            var constructorName = genericStart < 0 ? typeName : typeName[..genericStart];
            var members = branches
                .Select((branch, index) => new TaggedUnionBranch(branch, index + 1, GetTaggedUnionMemberName(branch.Name, index + 1)))
                .ToArray();
            var builder = new StringBuilder();
            builder.AppendLine("[ECMAScript]");
            builder.AppendLine("[Union]");
            builder.AppendLine($"public readonly struct {typeName} : IUnion");
            builder.AppendLine("{");
            builder.AppendLine("    // Native unions cannot retain an exact branch when one source type inherits another.");
            builder.AppendLine("    // 保留显式 tag，避免派生 DOM 类型同时命中基类 AsX 投影。");
            builder.AppendLine("    private readonly byte _kind;");
            foreach (var member in members)
                builder.AppendLine($"    private readonly {member.Type.Name}? _value{member.Index};");
            builder.AppendLine();

            foreach (var member in members)
            {
                builder.AppendLine($"    public {constructorName}({member.Type.Name} value)");
                builder.AppendLine("    {");
                builder.AppendLine($"        _kind = {member.Index};");
                foreach (var field in members)
                    builder.AppendLine($"        _value{field.Index} = {(field.Index == member.Index ? "value" : "default")};");
                builder.AppendLine("    }");
                builder.AppendLine();
            }

            foreach (var member in members)
                builder.AppendLine($"    public {member.Type.Name}? As{member.MemberName} => _kind == {member.Index} ? _value{member.Index} : default;");
            builder.AppendLine();
            builder.AppendLine("    public object? Value => _kind switch");
            builder.AppendLine("    {");
            foreach (var member in members)
                builder.AppendLine($"        {member.Index} => As{member.MemberName},");
            builder.AppendLine("        _ => default");
            builder.AppendLine("    };");
            builder.AppendLine();

            foreach (var member in members.Where(static member => !IsInterface(member.Type.Name)))
            {
                builder.AppendLine($"    public static implicit operator {typeName}({member.Type.Name} value)");
                builder.AppendLine("        => new(value);");
                builder.AppendLine();
            }

            builder.AppendLine("}");
            AddDefinition(name, builder.ToString());
        }

        private static string GetTaggedUnionMemberName(string typeName, int index)
        {
            var identifier = typeName
                .Replace("[]", "Array", StringComparison.Ordinal)
                .Replace("?", string.Empty, StringComparison.Ordinal);
            var genericStart = identifier.IndexOf('<');
            if (genericStart >= 0)
                identifier = identifier[..genericStart];
            identifier = ToCSharpName(identifier);
            return string.IsNullOrEmpty(identifier) ? $"Value{index}" : identifier;
        }

        private static bool IsInterface(string typeName)
            => TryResolveRuntimeType(typeName, out var type) && type.IsInterface;

        private sealed record TaggedUnionBranch(MappedType Type, int Index, string MemberName);

        private void AddStringEnum(string name, IReadOnlyList<string> values)
        {
            var builder = new StringBuilder();
            builder.AppendLine("[ECMAScript]");
            builder.AppendLine("[String]");
            // String literal domains do not depend on an enclosing generic parameter.
            // Emitting enum<T> is illegal C# and would invent a runtime distinction.
            builder.AppendLine($"public enum {name}");
            builder.AppendLine("{");
            var allocated = new HashSet<string>(StringComparer.Ordinal);
            foreach (var value in values.Distinct(StringComparer.Ordinal))
            {
                var member = ToCSharpName(value);
                if (member.Length == 0 || !char.IsLetter(member[0]))
                    member = "Value" + member;
                var original = member;
                var suffix = 2;
                while (!allocated.Add(member))
                    member = original + suffix++;
                builder.AppendLine($"    [Description(\"@#{value}\")]");
                builder.AppendLine($"    {CSharpIdentifier.Escape(member)},");
            }
            builder.AppendLine("}");
            AddDefinition(name, builder.ToString());
        }

        private void AddDefinition(string name, string source)
        {
            if (_definitions.TryGetValue(name, out var existing) && !string.Equals(existing, source, StringComparison.Ordinal))
                throw new InvalidOperationException($"TDesign generated type name collision: {name}");
            _definitions[name] = source;
        }

        private static bool TryGetStringLiteral(Node node, out string value)
        {
            var text = node.Type == "literal_type" ? node.NamedChildren.SingleOrDefault()?.Text : null;
            if (text is { Length: >= 2 } && text[0] is '\'' or '"' && text[^1] == text[0])
            {
                value = text[1..^1];
                return true;
            }

            value = string.Empty;
            return false;
        }

        private static bool IsNullish(Node node)
            => node.Text is "null" or "undefined" or "void";

        private static bool IsOpenExtension(Node node)
        {
            if (node.Type == "type_identifier" && node.Text == "PlainObject")
                return true;

            return node.Type == "generic_type" &&
                node.NamedChildren.FirstOrDefault(static child => child.Type == "type_identifier")?.Text == "Record" &&
                node.NamedChildren.FirstOrDefault(static child => child.Type == "type_arguments")?.NamedChildren.LastOrDefault()?.Text is "any" or "unknown";
        }

        private static IEnumerable<Node> FlattenUnion(Node node)
        {
            if (node.Type != "union_type")
            {
                yield return node;
                yield break;
            }

            foreach (var child in node.NamedChildren)
            {
                foreach (var branch in FlattenUnion(child))
                    yield return branch;
            }
        }

        private static string ToCSharpName(string name)
        {
            var builder = new StringBuilder(name.Length);
            var upper = true;
            foreach (var character in name)
            {
                if (!char.IsLetterOrDigit(character))
                {
                    upper = true;
                    continue;
                }

                builder.Append(upper ? char.ToUpperInvariant(character) : character);
                upper = false;
            }

            return builder.ToString() switch
            {
                // `Clone` is synthesized by C# records. Preserve the JavaScript name
                // through ECMAScriptName while keeping the authored C# surface legal.
                "Clone" => "CloneElement",
                _ => builder.ToString()
            };
        }

        private static Node? GetTypeNode(Node node)
        {
            var type = node.GetChildForField("type");
            if (type is not null)
                return type.Type == "type_annotation" ? type.NamedChildren.SingleOrDefault() : type;

            return node.NamedChildren
                .FirstOrDefault(static child => child.Type == "type_annotation")?
                .NamedChildren
                .SingleOrDefault();
        }

    }

}
