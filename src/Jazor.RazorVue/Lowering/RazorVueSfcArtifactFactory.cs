using System.Collections.Immutable;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using Jazor.RazorVue.Artifacts;
using Jazor.RazorVue.Canonical;
using Jazor.RazorVue.Descriptor;
using Jazor.RazorVue.Extensibility;
using Jazor.RazorVue.RenderTree;
using Jazor.RazorVue.Sfc;
namespace Jazor.RazorVue.Lowering;

internal sealed class RazorVueSfcArtifactFactory : IRazorVueSfcArtifactLowerer
{
    private readonly RazorVueCanonicalHModelFactory _canonicalFactory;
    private readonly RazorVueSfcSemanticModelFactory _semanticFactory = new();
    private static readonly System.Threading.AsyncLocal<IReadOnlyDictionary<string, RazorVueSfcComponentImport>?> CurrentComponentImportMap = new();

    public RazorVueSfcArtifactFactory(IRazorVueTemplateFrontend templateFrontend)
    {
        _canonicalFactory = new RazorVueCanonicalHModelFactory(templateFrontend);
    }

    public VueSfcArtifact Lower(RazorVueCompilationContext context, RazorVueSemanticSnapshot snapshot)
    {
        if (context is null)
            throw new ArgumentNullException(nameof(context));
        if (snapshot is null)
            throw new ArgumentNullException(nameof(snapshot));

        var canonical = _canonicalFactory.Create(context, snapshot);
        var semantic = _semanticFactory.Create(canonical);
        return CreateArtifact(snapshot, semantic);
    }

    private static VueSfcArtifact CreateArtifact(
        RazorVueSemanticSnapshot snapshot,
        RazorVueSfcSemanticModel semantic)
    {
        var importMap = semantic.ComponentImports.ToDictionary(
            static item => item.ComponentKey,
            static item => item,
            StringComparer.Ordinal);
        CurrentComponentImportMap.Value = importMap;
        var bindingSiteMap = semantic.TemplateBlock.BindingSites.ToDictionary(
            static item => item.SitePath,
            static item => item.BindingName,
            StringComparer.Ordinal);
        var templateText = BuildTemplateBlockText(semantic.TemplateBlock.Template, bindingSiteMap);
        var scriptSetupText = BuildScriptSetupBlockText(
            snapshot,
            semantic,
            RazorVueAttributeMergeHelper.ContainsInvocation(templateText));
        CurrentComponentImportMap.Value = null;
        var styleBlocks = BuildStyleBlocks(semantic.StyleBlocks);
        var customBlocks = BuildCustomBlocks(semantic.CustomBlocks);
        var sfcText = BuildSfcText(templateText, scriptSetupText, styleBlocks, customBlocks);
        var sourceOrigins = semantic.SourceOrigins
            .AddRange(semantic.TemplateBlock.SourceOrigins)
            .AddRange(semantic.ScriptSetupBlock.SourceOrigins)
            .AddRange(styleBlocks.SelectMany(static block => block.SourceOrigins))
            .AddRange(customBlocks.SelectMany(static block => block.SourceOrigins));

        return new VueSfcArtifact(
            ComponentName: semantic.ComponentName,
            RelativeSfcPath: semantic.RelativeSfcPath,
            SfcText: sfcText,
            TemplateBlock: new VueSfcTemplateBlock(
                templateText,
                semantic.TemplateBlock.SourceOrigins),
            ScriptSetupBlock: new VueSfcScriptSetupBlock(
                scriptSetupText,
                Language: "ts",
                semantic.ScriptSetupBlock.SourceOrigins),
            StyleBlocks: styleBlocks,
            CustomBlocks: customBlocks,
            Imports: semantic.Imports,
            Styles: semantic.Styles,
            PluginRequirements: semantic.PluginRequirements,
            Identity: BuildIdentity(semantic, templateText, scriptSetupText, styleBlocks),
            Hints: semantic.Hints,
            SourceOrigins: sourceOrigins.Distinct().ToImmutableArray());
    }

    private static VueSfcArtifactIdentity BuildIdentity(
        RazorVueSfcSemanticModel semantic,
        string templateText,
        string scriptSetupText,
        ImmutableArray<VueSfcStyleBlock> styleBlocks)
        => new(
            ComponentId: semantic.ComponentFullName,
            ModuleId: semantic.RelativeSfcPath,
            DescriptorHash: ComputeSha256Hex(BuildDescriptorShape(semantic.Descriptor)),
            TemplateHash: ComputeSha256Hex(templateText),
            LogicHash: ComputeSha256Hex(scriptSetupText),
            StyleHash: ComputeSha256Hex(string.Join("\n---\n", styleBlocks.Select(static block => block.Text + "|" + (block.SourceFilePath ?? string.Empty)))),
            HmrBoundaryKind: ClassifyHmrBoundary(semantic));

    private static HmrBoundaryKind ClassifyHmrBoundary(RazorVueSfcSemanticModel semantic)
    {
        if (semantic.TemplateBlock.Template.Children.IsDefaultOrEmpty)
            return HmrBoundaryKind.Unknown;

        if (!semantic.ScriptSetupBlock.LiftedBindings.IsDefaultOrEmpty ||
            !semantic.ScriptSetupBlock.Setup.RequiredFields.IsDefaultOrEmpty ||
            !semantic.ScriptSetupBlock.Setup.RequiredMethods.IsDefaultOrEmpty ||
            semantic.ScriptSetupBlock.Setup.Lifecycle.HasAnyHook)
        {
            return HmrBoundaryKind.LogicSafe;
        }

        return HmrBoundaryKind.TemplateOnly;
    }

    private static ImmutableArray<VueSfcStyleBlock> BuildStyleBlocks(ImmutableArray<RazorVueSfcStyleBlockModel> styleModels)
    {
        if (styleModels.IsDefaultOrEmpty)
            return ImmutableArray<VueSfcStyleBlock>.Empty;

        return styleModels.Select(static block => new VueSfcStyleBlock(
                Text: block.Text,
                IsScoped: block.IsScoped,
                ModuleName: block.ModuleName,
                Language: block.Language,
                SourceFilePath: block.SourceFilePath,
                SourceOrigins: block.SourceOrigins))
            .ToImmutableArray();
    }

    private static ImmutableArray<VueSfcCustomBlock> BuildCustomBlocks(ImmutableArray<RazorVueSfcCustomBlockModel> customBlockModels)
    {
        if (customBlockModels.IsDefaultOrEmpty)
            return ImmutableArray<VueSfcCustomBlock>.Empty;

        return customBlockModels.Select(static block => new VueSfcCustomBlock(
                Name: block.Name,
                Text: block.Text,
                Language: block.Language,
                Attributes: block.Attributes,
                SourceFilePath: block.SourceFilePath,
                SourceOrigins: block.SourceOrigins))
            .ToImmutableArray();
    }

    private static string BuildSfcText(
        string templateText,
        string scriptSetupText,
        ImmutableArray<VueSfcStyleBlock> styleBlocks,
        ImmutableArray<VueSfcCustomBlock> customBlocks)
    {
        var builder = new StringBuilder();
        builder.AppendLine("<template>");
        builder.Append(templateText);
        if (!templateText.EndsWith("\n", StringComparison.Ordinal))
            builder.AppendLine();
        builder.AppendLine("</template>");
        builder.AppendLine();
        builder.AppendLine("<script setup lang=\"ts\">");
        builder.Append(scriptSetupText);
        if (!scriptSetupText.EndsWith("\n", StringComparison.Ordinal))
            builder.AppendLine();
        builder.AppendLine("</script>");

        foreach (var styleBlock in styleBlocks)
        {
            builder.AppendLine();
            builder.Append("<style");
            if (styleBlock.IsScoped)
                builder.Append(" scoped");
            if (!string.IsNullOrWhiteSpace(styleBlock.ModuleName))
                builder.Append(" module=\"").Append(styleBlock.ModuleName).Append('"');
            if (!string.IsNullOrWhiteSpace(styleBlock.Language))
                builder.Append(" lang=\"").Append(styleBlock.Language).Append('"');
            if (!string.IsNullOrWhiteSpace(styleBlock.SourceFilePath))
                builder.Append(" src=\"").Append(styleBlock.SourceFilePath!.Replace("\\", "/")).Append('"');
            builder.AppendLine(">");
            builder.Append(styleBlock.Text);
            if (!styleBlock.Text.EndsWith("\n", StringComparison.Ordinal))
                builder.AppendLine();
            builder.AppendLine("</style>");
        }

        foreach (var customBlock in customBlocks)
        {
            builder.AppendLine();
            builder.Append('<').Append(customBlock.Name);
            if (!string.IsNullOrWhiteSpace(customBlock.Language))
                builder.Append(" lang=\"").Append(customBlock.Language).Append('"');
            foreach (var attribute in customBlock.Attributes)
            {
                builder.Append(' ').Append(attribute.Name);
                if (attribute.Value is not null)
                    builder.Append("=\"").Append(attribute.Value).Append('"');
            }

            builder.AppendLine(">");
            builder.Append(customBlock.Text);
            if (!customBlock.Text.EndsWith("\n", StringComparison.Ordinal))
                builder.AppendLine();
            builder.Append("</").Append(customBlock.Name).AppendLine(">");
        }

        return NormalizeLineEndings(builder.ToString());
    }

    private static string BuildTemplateBlockText(
        RazorVueCanonicalTemplateFragment template,
        IReadOnlyDictionary<string, string> bindingSiteMap)
    {
        var builder = new StringBuilder();
        for (var index = 0; index < template.Children.Length; index++)
            AppendTemplateNode(builder, template.Children[index], 0, bindingSiteMap, "root/child[" + index + "]");
        return NormalizeLineEndings(builder.ToString());
    }

    private static void AppendTemplateNode(
        StringBuilder builder,
        RazorVueCanonicalTemplateNode node,
        int depth,
        IReadOnlyDictionary<string, string> bindingSiteMap,
        string path,
        int templateScopeDepth = 0)
    {
        var indent = new string(' ', depth * 2);
        switch (node)
        {
            case RazorVueCanonicalElementNode element:
                builder.Append(indent).Append('<').Append(element.TagName);
                AppendAttributeBindings(builder, element.Attributes, bindingSiteMap, path + "/attrs", templateScopeDepth);
                if (element.Children.Children.IsDefaultOrEmpty)
                {
                    builder.AppendLine(" />");
                    return;
                }

                builder.AppendLine(">");
                for (var index = 0; index < element.Children.Children.Length; index++)
                    AppendTemplateNode(builder, element.Children.Children[index], depth + 1, bindingSiteMap, path + "/child[" + index + "]", templateScopeDepth);
                builder.Append(indent).Append("</").Append(element.TagName).AppendLine(">");
                return;

            case RazorVueCanonicalComponentNode component:
                var componentTagName = ResolveTemplateComponentTagName(component);
                builder.Append(indent).Append('<').Append(componentTagName);
                AppendAttributeBindings(builder, component.Attributes, bindingSiteMap, path + "/attrs", templateScopeDepth);
                if (component.Children.Children.IsDefaultOrEmpty && component.Slots.IsDefaultOrEmpty)
                {
                    builder.AppendLine(" />");
                    return;
                }

                builder.AppendLine(">");
                var slotOrdinal = 0;
                foreach (var slot in component.Slots.Where(static slot => !slot.IsDefault))
                {
                    AppendNamedSlot(builder, slot, depth + 1, bindingSiteMap, path + "/slots/slot[" + slotOrdinal + "]", templateScopeDepth);
                    slotOrdinal++;
                }

                foreach (var slot in component.Slots.Where(static slot => slot.IsDefault))
                {
                    AppendDefaultSlot(builder, slot, depth + 1, bindingSiteMap, path + "/slots/slot[" + slotOrdinal + "]", templateScopeDepth);
                    slotOrdinal++;
                }

                for (var index = 0; index < component.Children.Children.Length; index++)
                    AppendTemplateNode(builder, component.Children.Children[index], depth + 1, bindingSiteMap, path + "/child[" + index + "]", templateScopeDepth);
                builder.Append(indent).Append("</").Append(componentTagName).AppendLine(">");
                return;

            case RazorVueCanonicalTextNode text:
                builder.Append(indent).AppendLine(EscapeTemplateText(text.Text));
                return;

            case RazorVueCanonicalInterpolationNode interpolation:
                builder.Append(indent)
                    .Append("{{ ")
                    .Append(ResolveTemplateExpression(interpolation.ExpressionText, interpolation.TemplateEncodability, bindingSiteMap, path, templateScopeDepth))
                    .AppendLine(" }}");
                return;

            case RazorVueCanonicalSlotOutletNode slotOutlet:
                builder.Append(indent).Append("<slot");
                if (!string.Equals(slotOutlet.SlotName, "default", StringComparison.Ordinal))
                    builder.Append(" name=\"").Append(slotOutlet.SlotName).Append('"');
                if (!string.IsNullOrWhiteSpace(slotOutlet.ArgumentExpressionText))
                    builder.Append(" :value=\"")
                        .Append(EscapeAttributeValue(ResolveTemplateExpression(slotOutlet.ArgumentExpressionText!, slotOutlet.TemplateEncodability, bindingSiteMap, path + "/slotArg", templateScopeDepth)))
                        .Append('"');
                builder.AppendLine(" />");
                return;

            case RazorVueCanonicalConditionalNode conditional:
                builder.Append(indent)
                    .Append("<template v-if=\"")
                    .Append(EscapeAttributeValue(ResolveTemplateExpression(conditional.ConditionExpressionText, conditional.TemplateEncodability, bindingSiteMap, path + "/if", templateScopeDepth)))
                    .AppendLine("\">");
                for (var index = 0; index < conditional.WhenTrue.Children.Length; index++)
                    AppendTemplateNode(builder, conditional.WhenTrue.Children[index], depth + 1, bindingSiteMap, path + "/whenTrue/child[" + index + "]", templateScopeDepth);
                builder.Append(indent).AppendLine("</template>");
                if (!conditional.WhenFalse.Children.IsDefaultOrEmpty)
                {
                    builder.Append(indent).AppendLine("<template v-else>");
                    for (var index = 0; index < conditional.WhenFalse.Children.Length; index++)
                        AppendTemplateNode(builder, conditional.WhenFalse.Children[index], depth + 1, bindingSiteMap, path + "/whenFalse/child[" + index + "]", templateScopeDepth);
                    builder.Append(indent).AppendLine("</template>");
                }

                return;

            case RazorVueCanonicalForEachNode loop:
                builder.Append(indent)
                    .Append("<template v-for=\"")
                    .Append(loop.ItemName)
                    .Append(" in ")
                    .Append(EscapeAttributeValue(ResolveTemplateExpression(loop.SourceExpressionText, loop.TemplateEncodability, bindingSiteMap, path + "/forEach", templateScopeDepth)))
                    .AppendLine("\">");
                for (var index = 0; index < loop.Body.Children.Length; index++)
                    AppendTemplateNode(builder, loop.Body.Children[index], depth + 1, bindingSiteMap, path + "/body/child[" + index + "]", templateScopeDepth + 1);
                builder.Append(indent).AppendLine("</template>");
                return;

            case RazorVueCanonicalForNode loop:
                builder.Append(indent)
                    .Append("<template v-for=\"")
                    .Append(loop.VariableName)
                    .Append(" in ")
                    .Append(EscapeAttributeValue(BuildForIterableExpression(loop, bindingSiteMap, path, templateScopeDepth)))
                    .AppendLine("\">");
                for (var index = 0; index < loop.Body.Children.Length; index++)
                    AppendTemplateNode(builder, loop.Body.Children[index], depth + 1, bindingSiteMap, path + "/body/child[" + index + "]", templateScopeDepth + 1);
                builder.Append(indent).AppendLine("</template>");
                return;

            default:
                throw new NotSupportedException($"Unsupported canonical template node '{node.NodeKind}'.");
        }
    }

    private static void AppendNamedSlot(
        StringBuilder builder,
        RazorVueCanonicalSlotBinding slot,
        int depth,
        IReadOnlyDictionary<string, string> bindingSiteMap,
        string path,
        int templateScopeDepth)
    {
        var indent = new string(' ', depth * 2);
        builder.Append(indent).Append("<template ");
        AppendSlotDirectiveTarget(builder, slot.SlotName);
        if (!string.IsNullOrWhiteSpace(slot.ParameterName))
            builder.Append("=\"").Append(slot.ParameterName).Append('"');
        builder.AppendLine(">");
        if (!slot.Children.Children.IsDefaultOrEmpty)
        {
            for (var index = 0; index < slot.Children.Children.Length; index++)
            {
                AppendTemplateNode(
                    builder,
                    slot.Children.Children[index],
                    depth + 1,
                    bindingSiteMap,
                    path + "/child[" + index + "]",
                    string.IsNullOrWhiteSpace(slot.ParameterName) ? templateScopeDepth : templateScopeDepth + 1);
            }
        }
        else if (slot.ValueKind == RazorVueCanonicalSlotValueKind.ForwardedSlot)
        {
            builder.Append(indent)
                .Append("  ")
                .Append("<slot");
            if (!string.Equals(slot.ForwardedSlotName, "default", StringComparison.Ordinal))
                builder.Append(" name=\"").Append(slot.ForwardedSlotName).Append('"');
            if (!string.IsNullOrWhiteSpace(slot.ParameterName))
                builder.Append(" v-bind=\"").Append(slot.ParameterName).Append('"');
            builder.AppendLine(" />");
        }
        else if (!string.IsNullOrWhiteSpace(slot.ValueExpressionText))
        {
            builder.Append(indent)
                .Append("  ")
                .Append("{{ ")
                .Append(ResolveTemplateExpression(
                    slot.ValueExpressionText!,
                    slot.TemplateEncodability,
                    bindingSiteMap,
                    path,
                    string.IsNullOrWhiteSpace(slot.ParameterName) ? templateScopeDepth : templateScopeDepth + 1))
                .AppendLine(" }}");
        }
        builder.Append(indent).AppendLine("</template>");
    }

    private static void AppendSlotDirectiveTarget(StringBuilder builder, string slotName)
    {
        if (IsSimpleDirectiveArgument(slotName))
        {
            builder.Append('#').Append(slotName);
            return;
        }

        builder.Append("#[`")
            .Append(EscapeDynamicDirectiveString(slotName))
            .Append("`]");
    }

    private static bool IsSimpleDirectiveArgument(string value)
    {
        if (string.IsNullOrEmpty(value))
            return false;

        for (var index = 0; index < value.Length; index++)
        {
            var character = value[index];
            if (character is not (>= 'A' and <= 'Z') &&
                character is not (>= 'a' and <= 'z') &&
                character is not (>= '0' and <= '9') &&
                character != '_' &&
                character != '-' &&
                character != '$')
            {
                return false;
            }
        }

        return true;
    }

    private static string EscapeDynamicDirectiveString(string value)
        => (value ?? string.Empty)
            .Replace("\\", "\\\\")
            .Replace("`", "\\`")
            .Replace("${", "\\${");

    private static void AppendDefaultSlot(
        StringBuilder builder,
        RazorVueCanonicalSlotBinding slot,
        int depth,
        IReadOnlyDictionary<string, string> bindingSiteMap,
        string path,
        int templateScopeDepth)
    {
        var indent = new string(' ', depth * 2);
        if (!slot.Children.Children.IsDefaultOrEmpty)
        {
            for (var index = 0; index < slot.Children.Children.Length; index++)
            {
                AppendTemplateNode(
                    builder,
                    slot.Children.Children[index],
                    depth,
                    bindingSiteMap,
                    path + "/child[" + index + "]",
                    string.IsNullOrWhiteSpace(slot.ParameterName) ? templateScopeDepth : templateScopeDepth + 1);
            }

            return;
        }

        if (slot.ValueKind == RazorVueCanonicalSlotValueKind.ForwardedSlot)
        {
            builder.Append(indent).Append("<slot");
            if (!string.IsNullOrWhiteSpace(slot.ParameterName))
                builder.Append(" v-bind=\"").Append(slot.ParameterName).Append('"');
            builder.AppendLine(" />");
            return;
        }

        if (!string.IsNullOrWhiteSpace(slot.ValueExpressionText))
        {
            builder.Append(indent)
                .Append("{{ ")
                .Append(ResolveTemplateExpression(
                    slot.ValueExpressionText!,
                    slot.TemplateEncodability,
                    bindingSiteMap,
                    path,
                    string.IsNullOrWhiteSpace(slot.ParameterName) ? templateScopeDepth : templateScopeDepth + 1))
                .AppendLine(" }}");
        }
    }

    private static void AppendAttributeBindings(
        StringBuilder builder,
        ImmutableArray<RazorVueCanonicalAttributeEntry> attributes,
        IReadOnlyDictionary<string, string> bindingSiteMap,
        string pathPrefix,
        int templateScopeDepth)
    {
        if (RequiresMergedAttributeBinding(attributes))
        {
            AppendMergedAttributeBinding(builder, attributes, bindingSiteMap, pathPrefix, templateScopeDepth);
            return;
        }

        for (var index = 0; index < attributes.Length; index++)
        {
            switch (attributes[index])
            {
                case RazorVueCanonicalAttributeBinding attribute:
                    if (attribute.ExpressionText is null)
                    {
                        builder.Append(' ').Append(attribute.Name);
                        continue;
                    }

                    if (attribute.BindingKind == RazorVueExpressionBindingKind.Literal &&
                        attribute.TemplateEncodability == RazorVueTemplateEncodability.DirectTemplate &&
                        attribute.AttributeKind != RazorVueCanonicalAttributeKind.ComponentEvent)
                    {
                        if (CanEmitStaticLiteralAttribute(attribute))
                        {
                            builder.Append(' ')
                                .Append(attribute.Name)
                                .Append("=\"")
                                .Append(EscapeAttributeValue(attribute.ExpressionText.Trim('"')))
                                .Append('"');
                        }
                        else
                        {
                            builder.Append(" :")
                                .Append(attribute.Name)
                                .Append("=\"")
                                .Append(EscapeAttributeValue(attribute.ExpressionText))
                                .Append('"');
                        }

                        continue;
                    }

                    var expressionText = ResolveTemplateExpression(
                        attribute.ExpressionText,
                        attribute.TemplateEncodability,
                        bindingSiteMap,
                        pathPrefix + "/attr[" + index + "]",
                        templateScopeDepth);
                    if (attribute.AttributeKind == RazorVueCanonicalAttributeKind.ComponentEvent)
                    {
                        builder.Append(" @")
                            .Append(attribute.Name)
                            .Append("=\"")
                            .Append(EscapeAttributeValue(expressionText))
                            .Append('"');
                    }
                    else
                    {
                        builder.Append(" :")
                            .Append(attribute.Name)
                            .Append("=\"")
                            .Append(EscapeAttributeValue(expressionText))
                            .Append('"');
                    }
                    break;
                case RazorVueCanonicalAttributeSpreadBinding spread:
                    var spreadExpression = ResolveTemplateExpression(
                        spread.ExpressionText,
                        spread.TemplateEncodability,
                        bindingSiteMap,
                        pathPrefix + "/attr[" + index + "]/spread",
                        templateScopeDepth);
                    builder.Append(" v-bind=\"")
                        .Append(EscapeAttributeValue(spreadExpression))
                        .Append('"');
                    break;
            }
        }
    }

    private static bool CanEmitStaticLiteralAttribute(RazorVueCanonicalAttributeBinding attribute)
        => attribute.AttributeKind == RazorVueCanonicalAttributeKind.HtmlAttribute ||
           attribute.LiteralValueKind is RazorVueLiteralValueKind.String;

    private static bool RequiresMergedAttributeBinding(ImmutableArray<RazorVueCanonicalAttributeEntry> attributes)
    {
        if (attributes.IsDefaultOrEmpty)
            return false;

        var seenKeys = new HashSet<string>(StringComparer.Ordinal);
        foreach (var attributeEntry in attributes)
        {
            if (attributeEntry is RazorVueCanonicalAttributeSpreadBinding)
                return true;

            var attribute = (RazorVueCanonicalAttributeBinding)attributeEntry;
            if (!seenKeys.Add(GetMergedAttributeObjectKey(attribute)))
                return true;
        }

        return false;
    }

    private static void AppendMergedAttributeBinding(
        StringBuilder builder,
        ImmutableArray<RazorVueCanonicalAttributeEntry> attributes,
        IReadOnlyDictionary<string, string> bindingSiteMap,
        string pathPrefix,
        int templateScopeDepth)
    {
        var segments = new List<string>();
        var objectEntries = new List<string>();

        for (var index = 0; index < attributes.Length; index++)
        {
            switch (attributes[index])
            {
                case RazorVueCanonicalAttributeBinding attribute:
                    var expressionText = attribute.ExpressionText is null
                        ? "true"
                        : ResolveTemplateExpression(
                            attribute.ExpressionText,
                            attribute.TemplateEncodability,
                            bindingSiteMap,
                            pathPrefix + "/attr[" + index + "]",
                            templateScopeDepth);
                    objectEntries.Add(ToJavaScriptString(GetMergedAttributeObjectKey(attribute)) + ": " + expressionText);
                    break;

                case RazorVueCanonicalAttributeSpreadBinding spread:
                    FlushMergedAttributeObjectEntries(segments, objectEntries);
                    var spreadExpression = ResolveTemplateExpression(
                        spread.ExpressionText,
                        spread.TemplateEncodability,
                        bindingSiteMap,
                        pathPrefix + "/attr[" + index + "]/spread",
                        templateScopeDepth);
                    segments.Add(spreadExpression);
                    break;
            }
        }

        FlushMergedAttributeObjectEntries(segments, objectEntries);
        builder.Append(" v-bind=\"")
            .Append(EscapeAttributeValue(RazorVueAttributeMergeHelper.BuildInvocation(segments)))
            .Append('"');
    }

    private static void FlushMergedAttributeObjectEntries(List<string> segments, List<string> objectEntries)
    {
        if (objectEntries.Count == 0)
            return;

        segments.Add("{ " + string.Join(", ", objectEntries) + " }");
        objectEntries.Clear();
    }

    private static string GetMergedAttributeObjectKey(RazorVueCanonicalAttributeBinding attribute)
        => attribute.AttributeKind == RazorVueCanonicalAttributeKind.ComponentEvent
            ? ToVueEventHandlerName(attribute.Name)
            : attribute.Name;

    private static string ToVueEventHandlerName(string eventName)
    {
        if (string.IsNullOrEmpty(eventName))
            return "on";

        if (IsVueEventHandlerName(eventName))
            return eventName;

        return "on" + char.ToUpperInvariant(eventName[0]) + eventName.Substring(1);
    }

    private static bool IsVueEventHandlerName(string eventName)
    {
        if (!eventName.StartsWith("on", StringComparison.Ordinal) || eventName.Length <= 2)
            return false;

        var marker = eventName[2];
        return char.IsUpper(marker) || marker == ':';
    }

    private static string BuildScriptSetupBlockText(
        RazorVueSemanticSnapshot snapshot,
        RazorVueSfcSemanticModel semantic,
        bool requiresAttributeMergeHelper)
    {
        var builder = new StringBuilder();

        var vueImports = BuildVueImports(snapshot, semantic);
        if (vueImports.Length > 0)
            builder.Append("import { ").Append(string.Join(", ", vueImports)).AppendLine(" } from \"vue\";");

        RazorVueCompilerImportFormatter.AppendImportStatements(builder, semantic.CompilerImports);
        AppendComponentImports(builder, semantic);

        if (builder.Length > 0)
            builder.AppendLine();

        builder.Append("const props = defineProps<");
        builder.Append(GetPropsTypeLiteral(semantic.Descriptor));
        builder.AppendLine(">();");
        builder.Append("const emit = defineEmits<");
        builder.Append(GetEmitTypeLiteral(semantic.Descriptor));
        builder.AppendLine(">();");
        if (RazorVueForLoopLoweringSupport.ContainsForLoop(semantic.TemplateBlock.Template))
            RazorVueForLoopLoweringSupport.AppendForRangeHelper(builder, string.Empty);
        if (requiresAttributeMergeHelper)
            RazorVueAttributeMergeHelper.AppendHelper(builder, string.Empty);

        var expressionEmitter = new RazorVueExpressionEmitter(snapshot);
        RazorVueSetupAndLifecycleLoweringSupport.AppendLifecycleLowering(builder, snapshot, string.Empty);
        RazorVueSetupAndLifecycleLoweringSupport.AppendSetupLogicLowering(
            builder,
            snapshot,
            expressionEmitter,
            semantic.ScriptSetupBlock.Setup.RequiredFields,
            semantic.ScriptSetupBlock.Setup.RequiredMethods,
            string.Empty);

        foreach (var binding in semantic.ScriptSetupBlock.LiftedBindings)
        {
            AppendLiftedBinding(builder, binding);
        }

        return NormalizeLineEndings(builder.ToString());
    }

    private static ImmutableArray<string> BuildVueImports(
        RazorVueSemanticSnapshot snapshot,
        RazorVueSfcSemanticModel semantic)
    {
        var builder = RazorVueSetupAndLifecycleLoweringSupport
            .CollectVueRuntimeImports(snapshot)
            .ToBuilder();
        if (semantic.ScriptSetupBlock.LiftedBindings.Any(static binding => binding.BindingKind == RazorVueSfcSetupBindingKind.Computed))
            builder.Add("computed");
        return builder
            .Distinct(StringComparer.Ordinal)
            .ToImmutableArray();
    }

    private static void AppendLiftedBinding(StringBuilder builder, RazorVueSfcSetupBinding binding)
    {
        switch (binding.BindingKind)
        {
            case RazorVueSfcSetupBindingKind.Computed:
                builder.Append("const ")
                    .Append(binding.Name)
                    .Append(" = computed(() => ")
                    .Append(binding.ExpressionText)
                    .AppendLine(");");
                return;
            default:
                builder.Append("const ")
                    .Append(binding.Name)
                    .Append(" = ")
                    .Append(binding.ExpressionText)
                    .AppendLine(";");
                return;
        }
    }

    private static string ResolveTemplateExpression(
        string expressionText,
        RazorVueTemplateEncodability templateEncodability,
        IReadOnlyDictionary<string, string> bindingSiteMap,
        string path,
        int templateScopeDepth)
    {
        if (templateEncodability == RazorVueTemplateEncodability.TemplateViaSetupBinding &&
            templateScopeDepth == 0)
        {
            if (!bindingSiteMap.TryGetValue(path, out var bindingName))
                throw new InvalidOperationException("RazorVue SFC template binding site map drifted out of sync with template emission at '" + path + "'.");

            return bindingName;
        }

        return expressionText;
    }

    private static string BuildForIterableExpression(
        RazorVueCanonicalForNode loop,
        IReadOnlyDictionary<string, string> bindingSiteMap,
        string path,
        int templateScopeDepth)
    {
        var initial = ResolveTemplateExpression(
            loop.InitialValueExpressionText,
            loop.TemplateEncodability,
            bindingSiteMap,
            path + "/for/init",
            templateScopeDepth);
        var limit = ResolveTemplateExpression(
            loop.LimitValueExpressionText,
            loop.TemplateEncodability,
            bindingSiteMap,
            path + "/for/limit",
            templateScopeDepth);
        var step = loop.StepValueExpressionText is null
            ? null
            : ResolveTemplateExpression(
                loop.StepValueExpressionText,
                loop.TemplateEncodability,
                bindingSiteMap,
                path + "/for/step",
                templateScopeDepth);

        return "__jazorVueForRange(" +
               initial + ", " +
               limit + ", " +
               ToJavaScriptString(RazorVueForLoopLoweringSupport.GetForConditionOperator(loop.ConditionKind)) + ", " +
               ToJavaScriptString(RazorVueForLoopLoweringSupport.GetForStepOperator(loop.StepKind)) + ", " +
               (step ?? "null") + ")";
    }

    private static string GetPropsTypeLiteral(VueComponentDescriptor descriptor)
    {
        if (descriptor.Props.IsDefaultOrEmpty)
            return "{ }";

        var members = descriptor.Props
            .OrderBy(static prop => prop.Name, StringComparer.Ordinal)
            .Select(static prop => prop.Name + (prop.Required ? ": any" : "?: any"));
        return "{ " + string.Join("; ", members) + " }";
    }

    private static string GetEmitTypeLiteral(VueComponentDescriptor descriptor)
    {
        if (descriptor.Emits.IsDefaultOrEmpty)
            return "{ }";

        var members = descriptor.Emits
            .OrderBy(static emit => emit.Name, StringComparer.Ordinal)
            .Select(static emit => "(event: \"" + emit.Name + "\", payload?: any): void");
        return "{ " + string.Join("; ", members) + " }";
    }

    private static string BuildDescriptorShape(VueComponentDescriptor descriptor)
    {
        var builder = new StringBuilder();
        builder.AppendLine(descriptor.FullName);
        builder.AppendLine(descriptor.SourceKind.ToString());
        builder.AppendLine(descriptor.ImportSpecifier);
        builder.AppendLine(descriptor.ExportName);
        builder.AppendLine("flags:" + descriptor.Flags);
        foreach (var prop in descriptor.Props.OrderBy(static item => item.PublicName, StringComparer.Ordinal))
            builder.AppendLine(prop.PublicName + "|" + prop.Name + "|" + prop.TypeName + "|" + prop.Required + "|" + prop.AcceptsBinding + "|" + (prop.DefaultExpression ?? string.Empty) + "|" + prop.Kind + "|" + prop.CaptureUnmatchedValues);
        foreach (var emit in descriptor.Emits.OrderBy(static item => item.RazorAlias, StringComparer.Ordinal))
            builder.AppendLine(emit.RazorAlias + "|" + emit.Name + "|" + emit.PayloadTypeName + "|" + emit.Kind);
        foreach (var slot in descriptor.Slots.OrderBy(static item => item.Name, StringComparer.Ordinal))
            builder.AppendLine(slot.PublicName + "|" + slot.Name + "|" + slot.IsDefault + "|" + slot.Required + "|" + string.Join(",", slot.Parameters.Select(static parameter => parameter.Name + ":" + parameter.TypeName)));
        foreach (var pluginRequirement in descriptor.PluginRequirements.OrderBy(static item => item, StringComparer.Ordinal))
            builder.AppendLine("plugin:" + pluginRequirement);
        return builder.ToString();
    }

    private static string CreateImportAlias(string importSpecifier)
    {
        var builder = new StringBuilder("module");
        foreach (var ch in importSpecifier)
        {
            if (char.IsLetterOrDigit(ch))
                builder.Append(ch);
        }

        return builder.ToString();
    }

    private static string CreateComponentAlias(string componentName)
        => componentName + "Component";

    private static string EscapeTemplateText(string text)
        => text.Replace("&", "&amp;")
            .Replace("<", "&lt;")
            .Replace(">", "&gt;");

    private static string EscapeAttributeValue(string value)
        => value.Replace("&", "&amp;")
            .Replace("\"", "&quot;")
            .Replace("<", "&lt;")
            .Replace(">", "&gt;");

    private static string ResolveTemplateComponentTagName(RazorVueCanonicalComponentNode component)
    {
        if (CurrentComponentImportMap.Value is not null &&
            CurrentComponentImportMap.Value.TryGetValue(component.ComponentFullName, out var import))
        {
            return import.TemplateTagName;
        }

        if (component.ResolvedDescriptor is null)
            return component.ResolutionName;

        return component.ResolvedDescriptor.SourceKind == VueComponentSourceKind.LibraryComponent
            ? component.ResolvedDescriptor.ExportName
            : CreateComponentAlias(component.ComponentName);
    }

    private static void AppendComponentImports(StringBuilder builder, RazorVueSfcSemanticModel semantic)
    {
        var groups = semantic.ComponentImports
            .Where(static component => component.ImportKind != RazorVueSfcComponentImportKind.None &&
                                       !string.IsNullOrWhiteSpace(component.ImportSpecifier))
            .OrderBy(static component => component.TemplateTagName, StringComparer.Ordinal)
            .GroupBy(static component => component.ImportSpecifier!, StringComparer.Ordinal);

        foreach (var group in groups)
        {
            var components = group.ToImmutableArray();
            var namedImports = components
                .Where(static component => component.ImportKind == RazorVueSfcComponentImportKind.Named)
                .Select(static component => component.ExportName + " as " + component.LocalBindingName)
                .ToImmutableArray();

            foreach (var component in components.Where(static component => component.ImportKind == RazorVueSfcComponentImportKind.Default))
            {
                builder.Append("import ")
                    .Append(component.LocalBindingName)
                    .Append(" from ")
                    .Append(ToJavaScriptString(component.ImportSpecifier!))
                    .AppendLine(";");
            }

            if (!namedImports.IsDefaultOrEmpty)
            {
                builder.Append("import { ")
                    .Append(string.Join(", ", namedImports))
                    .Append(" } from ")
                    .Append(ToJavaScriptString(group.Key))
                    .AppendLine(";");
            }
        }
    }


    private static string NormalizeLineEndings(string text)
    {
        if (string.IsNullOrEmpty(text))
            return text;

        return text.Replace("\r\n", "\n").Replace("\r", "\n");
    }

    private static string ToLowerCamelCase(string value)
    {
        if (string.IsNullOrEmpty(value))
            return value;
        if (value.Length == 1)
            return char.ToLowerInvariant(value[0]).ToString();
        if (char.IsUpper(value[0]) && char.IsUpper(value[1]))
            return value;
        return char.ToLowerInvariant(value[0]) + value.Substring(1);
    }

    private static string ComputeSha256Hex(string content)
    {
        using var sha = SHA256.Create();
        var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(content ?? string.Empty));
        var builder = new StringBuilder(bytes.Length * 2);
        foreach (var item in bytes)
            builder.Append(item.ToString("X2", CultureInfo.InvariantCulture));
        return builder.ToString();
    }

    private static string ToJavaScriptString(string value)
        => "\"" + (value ?? string.Empty)
            .Replace("\\", "\\\\")
            .Replace("\"", "\\\"")
            .Replace("\r", "\\r")
            .Replace("\n", "\\n") + "\"";

}
