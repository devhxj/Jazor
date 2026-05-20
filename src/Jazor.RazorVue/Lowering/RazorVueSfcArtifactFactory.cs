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
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Jazor.RazorVue.Lowering;

internal sealed class RazorVueSfcArtifactFactory : IRazorVueSfcArtifactLowerer
{
    private readonly IRazorVueTemplateFrontend _templateFrontend;
    private readonly RazorVueCanonicalHModelFactory _canonicalFactory;
    private readonly RazorVueSfcSemanticModelFactory _semanticFactory = new();
    private static readonly System.Threading.AsyncLocal<IReadOnlyDictionary<string, RazorVueSfcComponentImport>?> CurrentComponentImportMap = new();

    public RazorVueSfcArtifactFactory(IRazorVueTemplateFrontend templateFrontend)
    {
        _templateFrontend = templateFrontend ?? throw new ArgumentNullException(nameof(templateFrontend));
        _canonicalFactory = new RazorVueCanonicalHModelFactory(templateFrontend);
    }

    public VueSfcArtifact Lower(RazorVueCompilationContext context, RazorVueSemanticSnapshot snapshot)
    {
        if (context is null)
            throw new ArgumentNullException(nameof(context));
        if (snapshot is null)
            throw new ArgumentNullException(nameof(snapshot));

        var renderTree = _templateFrontend.CreateRenderTree(context, snapshot);
        var canonical = _canonicalFactory.Create(context, snapshot, renderTree);
        var semantic = _semanticFactory.Create(canonical);
        return CreateArtifact(context, snapshot, semantic);
    }

    private static VueSfcArtifact CreateArtifact(
        RazorVueCompilationContext context,
        RazorVueSemanticSnapshot snapshot,
        RazorVueSfcSemanticModel semantic)
    {
        if (semantic.RenderMode == VueSfcArtifactRenderMode.RenderFunction)
            return CreateImperativeArtifact(context, snapshot, semantic);

        var importMap = semantic.ComponentImports.ToDictionary(
            static item => item.ComponentKey,
            static item => item,
            StringComparer.Ordinal);
        CurrentComponentImportMap.Value = importMap;
        try
        {
            var bindingSiteMap = semantic.TemplateBlock.BindingSites.ToDictionary(
                static item => item.SitePath,
                static item => item.TemplateExpressionText,
                StringComparer.Ordinal);
            var templateText = BuildTemplateBlockText(semantic.TemplateBlock.Template, bindingSiteMap);
            var scriptSetupText = BuildScriptSetupBlockText(
                snapshot,
                semantic,
                RazorVueAttributeMergeHelper.ContainsInvocation(templateText));
            var styleBlocks = BuildStyleBlocks(semantic.StyleBlocks);
            var customBlocks = BuildCustomBlocks(semantic.CustomBlocks);
            var sfcText = BuildSfcText(
                new VueSfcTemplateBlock(templateText, semantic.TemplateBlock.SourceOrigins),
                new VueSfcScriptSetupBlock(scriptSetupText, "ts", semantic.ScriptSetupBlock.SourceOrigins),
                new VueSfcScriptBlock(string.Empty, null, ImmutableArray<RazorVueSourceOrigin>.Empty),
                styleBlocks,
                customBlocks);
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
                ScriptBlock: new VueSfcScriptBlock(
                    string.Empty,
                    Language: null,
                    ImmutableArray<RazorVueSourceOrigin>.Empty),
                RenderMode: VueSfcArtifactRenderMode.Template,
                StyleBlocks: styleBlocks,
                CustomBlocks: customBlocks,
                RouteTemplates: semantic.Descriptor.RouteTemplates,
                Imports: semantic.Imports,
                Styles: semantic.Styles,
                PluginRequirements: semantic.PluginRequirements,
                Identity: BuildIdentity(semantic, templateText, scriptSetupText, styleBlocks),
                Hints: semantic.Hints,
                SourceOrigins: sourceOrigins.Distinct().ToImmutableArray());
        }
        finally
        {
            CurrentComponentImportMap.Value = null;
        }
    }

    private static VueSfcArtifact CreateImperativeArtifact(
        RazorVueCompilationContext context,
        RazorVueSemanticSnapshot snapshot,
        RazorVueSfcSemanticModel semantic)
    {
        if (semantic.ImperativeRootProgram is null)
            throw new InvalidOperationException("RazorVue imperative SFC artifact creation requires an imperative root program.");

        var descriptor = snapshot.Descriptor;
        var renderTree = semantic.ImperativeRootProgram.RenderTree;
        var resolvedComponents = RazorVueArtifactFactory.ResolveComponentsForCanonicalization(context, snapshot, renderTree);
        var imperativeResolvedComponents = NormalizeResolvedComponentsForSfc(resolvedComponents, descriptor.ImportSpecifier);
        var expressionEmitter = RazorVueArtifactFactory.CreateExpressionEmitterForCanonicalization(snapshot, imperativeResolvedComponents);
        var moduleCode = RazorVueImperativeSfcModuleBuilder.BuildModuleCode(
            snapshot,
            renderTree,
            expressionEmitter,
            imperativeResolvedComponents,
            out var compilerImports);
        var styleBlocks = ImmutableArray<VueSfcStyleBlock>.Empty;
        var customBlocks = ImmutableArray<VueSfcCustomBlock>.Empty;
        var scriptOrigins = snapshot.Origins.AddRange(expressionEmitter.CollectOrigins(renderTree)).Distinct().ToImmutableArray();
        var templateBlock = new VueSfcTemplateBlock(string.Empty, ImmutableArray<RazorVueSourceOrigin>.Empty);
        var scriptSetupBlock = new VueSfcScriptSetupBlock(string.Empty, null, ImmutableArray<RazorVueSourceOrigin>.Empty);
        var scriptBlock = new VueSfcScriptBlock(moduleCode, "ts", scriptOrigins);
        var sfcText = BuildSfcText(templateBlock, scriptSetupBlock, scriptBlock, styleBlocks, customBlocks);
        var relativeSfcPath = RazorVueSfcSemanticModelFactory.ChangeExtensionToVuePublic(
            RazorVueArtifactFactory.NormalizeRelativePathForCanonicalization(descriptor.ImportSpecifier));
        var sourceOrigins = snapshot.Origins
            .AddRange(expressionEmitter.CollectOrigins(renderTree))
            .Distinct()
            .ToImmutableArray();

        return new VueSfcArtifact(
            ComponentName: descriptor.Name,
            RelativeSfcPath: relativeSfcPath,
            SfcText: sfcText,
            TemplateBlock: templateBlock,
            ScriptSetupBlock: scriptSetupBlock,
            ScriptBlock: scriptBlock,
            RenderMode: VueSfcArtifactRenderMode.RenderFunction,
            StyleBlocks: styleBlocks,
            CustomBlocks: customBlocks,
            RouteTemplates: descriptor.RouteTemplates,
            Imports: BuildImperativeSfcImports(imperativeResolvedComponents, compilerImports),
            Styles: RazorVueArtifactFactory.BuildStylesForCanonicalization(descriptor, imperativeResolvedComponents),
            PluginRequirements: RazorVueArtifactFactory.BuildPluginRequirementsForCanonicalization(descriptor, imperativeResolvedComponents),
            Identity: BuildImperativeIdentity(snapshot, renderTree, expressionEmitter, relativeSfcPath, imperativeResolvedComponents),
            Hints: RazorVueArtifactFactory.BuildHintsForCanonicalization(snapshot, renderTree),
            SourceOrigins: sourceOrigins);
    }

    private static VueSfcArtifactIdentity BuildIdentity(
        RazorVueSfcSemanticModel semantic,
        string templateText,
        string scriptSetupText,
        ImmutableArray<VueSfcStyleBlock> styleBlocks)
        => new(
            ComponentId: semantic.ComponentFullName,
            ModuleId: semantic.RelativeSfcPath,
            DescriptorHash: ComputeSha256Hex(
                RazorVueDescriptorIdentityShapeBuilder.BuildForCanonicalTemplate(
                    semantic.Descriptor,
                    semantic.TemplateBlock.Template)),
            TemplateHash: ComputeSha256Hex(templateText),
            LogicHash: ComputeSha256Hex(scriptSetupText),
            StyleHash: ComputeSha256Hex(string.Join("\n---\n", styleBlocks.Select(static block => block.Text + "|" + (block.SourceFilePath ?? string.Empty)))),
            HmrBoundaryKind: ClassifyHmrBoundary(semantic));

    private static VueSfcArtifactIdentity BuildImperativeIdentity(
        RazorVueSemanticSnapshot snapshot,
        RazorVueRenderFragment renderTree,
        RazorVueExpressionEmitter expressionEmitter,
        string relativeSfcPath,
        ImmutableDictionary<string, VueComponentDescriptor> resolvedComponents)
    {
        var descriptor = snapshot.Descriptor;
        var descriptorShape = RazorVueDescriptorIdentityShapeBuilder.BuildForRenderTree(
            descriptor,
            snapshot.ComponentSymbol,
            snapshot.Compilation,
            renderTree,
            resolvedComponents);
        var templateShape = expressionEmitter.DescribeFragment(renderTree);
        var logicShape = RazorVueImperativeSfcModuleBuilder.BuildLogicShape(snapshot, renderTree, expressionEmitter);
        var boundaryKind = RazorVueImperativeSfcModuleBuilder.ClassifyHmrBoundary(renderTree, snapshot);

        return new VueSfcArtifactIdentity(
            ComponentId: descriptor.FullName,
            ModuleId: relativeSfcPath,
            DescriptorHash: ComputeSha256Hex(descriptorShape),
            TemplateHash: ComputeSha256Hex(templateShape),
            LogicHash: ComputeSha256Hex(logicShape),
            StyleHash: string.Empty,
            HmrBoundaryKind: boundaryKind);
    }

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

    private static ImmutableDictionary<string, VueComponentDescriptor> NormalizeResolvedComponentsForSfc(
        ImmutableDictionary<string, VueComponentDescriptor> resolvedComponents,
        string ownerImportSpecifier)
    {
        if (resolvedComponents.IsEmpty)
            return resolvedComponents;

        var ownerRelativeSfcPath = RazorVueSfcSemanticModelFactory.ChangeExtensionToVuePublic(
            RazorVueArtifactFactory.NormalizeRelativePathForCanonicalization(ownerImportSpecifier));
        var builder = ImmutableDictionary.CreateBuilder<string, VueComponentDescriptor>(StringComparer.Ordinal);
        foreach (var pair in resolvedComponents)
        {
            var descriptor = pair.Value with
            {
                ImportSpecifier = RazorVueSfcSemanticModelFactory.NormalizeSfcImportSpecifierPublic(
                    pair.Value.ImportSpecifier,
                    pair.Value.SourceKind,
                    ownerRelativeSfcPath)
            };

            builder[pair.Key] = descriptor;
        }

        return builder.ToImmutable();
    }

    private static ImmutableArray<string> BuildImperativeSfcImports(
        ImmutableDictionary<string, VueComponentDescriptor> resolvedComponents,
        ImmutableArray<RazorVueCompilerImportBinding> compilerImports)
    {
        var builder = ImmutableArray.CreateBuilder<string>();
        builder.Add("vue");
        builder.AddRange(RazorVueCompilerImportFormatter.CollectImportSources(compilerImports));
        builder.AddRange(
            resolvedComponents.Values
                .Select(static descriptor => descriptor.ImportSpecifier)
                .Where(static importSpecifier => !string.Equals(importSpecifier, "vue", StringComparison.Ordinal))
                .Distinct(StringComparer.Ordinal));
        return builder.Distinct(StringComparer.Ordinal).ToImmutableArray();
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
        VueSfcTemplateBlock templateBlock,
        VueSfcScriptSetupBlock scriptSetupBlock,
        VueSfcScriptBlock scriptBlock,
        ImmutableArray<VueSfcStyleBlock> styleBlocks,
        ImmutableArray<VueSfcCustomBlock> customBlocks)
    {
        var builder = new StringBuilder();
        if (!string.IsNullOrEmpty(templateBlock.Text))
        {
            builder.AppendLine("<template>");
            builder.Append(templateBlock.Text);
            if (!templateBlock.Text.EndsWith("\n", StringComparison.Ordinal))
                builder.AppendLine();
            builder.AppendLine("</template>");
        }

        if (!string.IsNullOrEmpty(scriptSetupBlock.Text))
        {
            if (builder.Length > 0)
                builder.AppendLine();
            builder.Append("<script setup");
            if (!string.IsNullOrWhiteSpace(scriptSetupBlock.Language))
                builder.Append(" lang=\"").Append(scriptSetupBlock.Language).Append('"');
            builder.AppendLine(">");
            builder.Append(scriptSetupBlock.Text);
            if (!scriptSetupBlock.Text.EndsWith("\n", StringComparison.Ordinal))
                builder.AppendLine();
            builder.AppendLine("</script>");
        }

        if (!string.IsNullOrEmpty(scriptBlock.Text))
        {
            if (builder.Length > 0)
                builder.AppendLine();
            builder.Append("<script");
            if (!string.IsNullOrWhiteSpace(scriptBlock.Language))
                builder.Append(" lang=\"").Append(scriptBlock.Language).Append('"');
            builder.AppendLine(">");
            builder.Append(scriptBlock.Text);
            if (!scriptBlock.Text.EndsWith("\n", StringComparison.Ordinal))
                builder.AppendLine();
            builder.AppendLine("</script>");
        }

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
        AppendTemplateFragment(builder, template, 0, bindingSiteMap, "root");
        return NormalizeLineEndings(builder.ToString());
    }

    private static void AppendTemplateFragment(
        StringBuilder builder,
        RazorVueCanonicalTemplateFragment fragment,
        int depth,
        IReadOnlyDictionary<string, string> bindingSiteMap,
        string pathPrefix,
        int templateScopeDepth = 0)
    {
        var openWrapperCount = 0;
        var currentTemplateScopeDepth = templateScopeDepth;
        for (var index = 0; index < fragment.Children.Length; index++)
        {
            if (fragment.Children[index] is RazorVueCanonicalLocalDeclarationNode localDeclaration)
            {
                var wrapperIndent = new string(' ', (depth + openWrapperCount) * 2);
                builder.Append(wrapperIndent)
                    .Append("<template v-for=\"(")
                    .Append(localDeclaration.LocalName)
                    .Append(") in [")
                    .Append(ResolveTemplateExpression(
                        localDeclaration.InitializerExpressionText,
                        localDeclaration.TemplateEncodability,
                        bindingSiteMap,
                        pathPrefix + "/child[" + index + "]/local",
                        currentTemplateScopeDepth))
                    .AppendLine("]\">");
                openWrapperCount++;
                currentTemplateScopeDepth++;
                continue;
            }

            if (fragment.Children[index] is RazorVueCanonicalTemplateScopeNode templateScope)
            {
                var wrapperIndent = new string(' ', (depth + openWrapperCount) * 2);
                builder.Append(wrapperIndent)
                    .Append("<template v-for=\"(")
                    .Append(templateScope.ScopeName)
                    .Append(") in [")
                    .Append(ResolveTemplateExpression(
                        templateScope.InitializerExpressionText,
                        templateScope.TemplateEncodability,
                        bindingSiteMap,
                        pathPrefix + "/child[" + index + "]/scopeInit",
                        currentTemplateScopeDepth))
                    .AppendLine("]\">");
                openWrapperCount++;
                AppendTemplateFragment(
                    builder,
                    templateScope.Children,
                    depth + openWrapperCount,
                    bindingSiteMap,
                    pathPrefix + "/child[" + index + "]/scope",
                    currentTemplateScopeDepth + 1);
                var closeIndent = new string(' ', (depth + openWrapperCount - 1) * 2);
                builder.Append(closeIndent).AppendLine("</template>");
                openWrapperCount--;
                continue;
            }

            AppendTemplateNode(
                builder,
                fragment.Children[index],
                depth + openWrapperCount,
                bindingSiteMap,
                pathPrefix + "/child[" + index + "]",
                currentTemplateScopeDepth);
        }

        for (var index = openWrapperCount - 1; index >= 0; index--)
        {
            var wrapperIndent = new string(' ', (depth + index) * 2);
            builder.Append(wrapperIndent).AppendLine("</template>");
        }
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
                AppendNodeKeyBinding(builder, element.Key, bindingSiteMap, path + "/key", templateScopeDepth);
                AppendAttributeBindings(builder, element.Attributes, bindingSiteMap, path + "/attrs", templateScopeDepth);
                if (element.Children.Children.IsDefaultOrEmpty)
                {
                    builder.AppendLine(" />");
                    return;
                }

                builder.AppendLine(">");
                AppendTemplateFragment(builder, element.Children, depth + 1, bindingSiteMap, path, templateScopeDepth);
                builder.Append(indent).Append("</").Append(element.TagName).AppendLine(">");
                return;

            case RazorVueCanonicalComponentNode component:
                var componentTagName = ResolveTemplateComponentTagName(component);
                builder.Append(indent).Append('<').Append(componentTagName);
                AppendNodeKeyBinding(builder, component.Key, bindingSiteMap, path + "/key", templateScopeDepth);
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

                AppendTemplateFragment(builder, component.Children, depth + 1, bindingSiteMap, path, templateScopeDepth);
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

            case RazorVueCanonicalLocalDeclarationNode:
                return;

            case RazorVueCanonicalTemplateScopeNode:
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
                AppendTemplateFragment(builder, conditional.WhenTrue, depth + 1, bindingSiteMap, path + "/whenTrue", templateScopeDepth);
                builder.Append(indent).AppendLine("</template>");
                if (!conditional.WhenFalse.Children.IsDefaultOrEmpty)
                {
                    builder.Append(indent).AppendLine("<template v-else>");
                    AppendTemplateFragment(builder, conditional.WhenFalse, depth + 1, bindingSiteMap, path + "/whenFalse", templateScopeDepth);
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
                AppendTemplateFragment(builder, loop.Body, depth + 1, bindingSiteMap, path + "/body", templateScopeDepth + 1);
                builder.Append(indent).AppendLine("</template>");
                return;

            case RazorVueCanonicalForNode loop:
                builder.Append(indent)
                    .Append("<template v-for=\"")
                    .Append(loop.VariableName)
                    .Append(" in ")
                    .Append(EscapeAttributeValue(BuildForIterableExpression(loop, bindingSiteMap, path, templateScopeDepth)))
                    .AppendLine("\">");
                AppendTemplateFragment(builder, loop.Body, depth + 1, bindingSiteMap, path + "/body", templateScopeDepth + 1);
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
            AppendTemplateFragment(
                builder,
                slot.Children,
                depth + 1,
                bindingSiteMap,
                path,
                string.IsNullOrWhiteSpace(slot.ParameterName) ? templateScopeDepth : templateScopeDepth + 1);
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
            var wrapParameterizedDefault = !string.IsNullOrWhiteSpace(slot.ParameterName);
            if (wrapParameterizedDefault)
            {
                builder.Append(indent)
                    .Append("<template #default=\"")
                    .Append(slot.ParameterName)
                    .AppendLine("\">");
            }

            AppendTemplateFragment(
                builder,
                slot.Children,
                wrapParameterizedDefault ? depth + 1 : depth,
                bindingSiteMap,
                path,
                wrapParameterizedDefault ? templateScopeDepth + 1 : templateScopeDepth);

            if (wrapParameterizedDefault)
                builder.Append(indent).AppendLine("</template>");

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

    private static void AppendNodeKeyBinding(
        StringBuilder builder,
        RazorVueCanonicalNodeKey? key,
        IReadOnlyDictionary<string, string> bindingSiteMap,
        string path,
        int templateScopeDepth)
    {
        if (key is null)
            return;

        var expressionText = ResolveTemplateExpression(
            key.ExpressionText,
            key.TemplateEncodability,
            bindingSiteMap,
            path,
            templateScopeDepth);

        builder.Append(" :key=\"")
            .Append(EscapeAttributeValue(expressionText))
            .Append('"');
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

        var expressionEmitter = new RazorVueExpressionEmitter(snapshot);
        builder.Append("const __jazorRawProps = defineProps<");
        builder.Append(GetPropsTypeLiteral(semantic.Descriptor));
        builder.AppendLine(">();");
        AppendNormalizedPropsBinding(builder, snapshot, expressionEmitter, semantic.Descriptor);
        builder.Append("const emit = defineEmits<");
        builder.Append(GetEmitTypeLiteral(semantic.Descriptor));
        builder.AppendLine(">();");
        if (semantic.ScriptSetupBlock.RequiresSlotsRuntime)
            builder.AppendLine("const slots = useSlots();");
        if (RazorVueForLoopLoweringSupport.ContainsForLoop(semantic.TemplateBlock.Template))
            RazorVueForLoopLoweringSupport.AppendForRangeHelper(builder, string.Empty);
        if (requiresAttributeMergeHelper)
            RazorVueAttributeMergeHelper.AppendHelper(builder, string.Empty);

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
        if (semantic.ScriptSetupBlock.RequiresSlotsRuntime)
            builder.Add("useSlots");
        return builder
            .Distinct(StringComparer.Ordinal)
            .ToImmutableArray();
    }

    private static void AppendNormalizedPropsBinding(
        StringBuilder builder,
        RazorVueSemanticSnapshot snapshot,
        RazorVueExpressionEmitter expressionEmitter,
        VueComponentDescriptor descriptor)
    {
        if (descriptor.Props.IsDefaultOrEmpty)
        {
            builder.AppendLine("const props = __jazorRawProps;");
            return;
        }

        var defaultEntries = descriptor.Props
            .Where(static prop => prop.DefaultSource != VuePropDefaultSource.None)
            .Select(prop => new DefaultPropEntry(
                prop.Name,
                LowerDefaultExpression(snapshot, expressionEmitter, prop)))
            .ToImmutableArray();

        if (defaultEntries.IsDefaultOrEmpty)
        {
            builder.AppendLine("const props = __jazorRawProps;");
            return;
        }

        builder.AppendLine("const __jazorPropDefaultCache = Object.create(null);");
        builder.AppendLine("const props = new Proxy(__jazorRawProps, {");
        builder.AppendLine("  get(target, key, receiver) {");
        builder.AppendLine("    if (typeof key === \"string\") {");
        foreach (var entry in defaultEntries)
        {
            builder.Append("      if (key === ")
                .Append(ToJavaScriptString(entry.PropName))
                .AppendLine(") {");
            builder.AppendLine("        const value = Reflect.get(target, key, receiver);");
            builder.AppendLine("        if (value !== undefined) return value;");
            builder.AppendLine("        if (Object.prototype.hasOwnProperty.call(__jazorPropDefaultCache, key)) return __jazorPropDefaultCache[key];");
            builder.Append("        const defaultValue = ")
                .Append(entry.ExpressionText)
                .AppendLine(";");
            builder.AppendLine("        __jazorPropDefaultCache[key] = defaultValue;");
            builder.AppendLine("        return defaultValue;");
            builder.AppendLine("      }");
        }
        builder.AppendLine("    }");
        builder.AppendLine("    return Reflect.get(target, key, receiver);");
        builder.AppendLine("  }");
        builder.AppendLine("});");
    }

    private static string LowerDefaultExpression(
        RazorVueSemanticSnapshot snapshot,
        RazorVueExpressionEmitter expressionEmitter,
        VuePropDescriptor prop)
    {
        if (prop.DefaultSource == VuePropDefaultSource.None || string.IsNullOrWhiteSpace(prop.DefaultExpression))
            throw new InvalidOperationException($"Prop '{prop.PublicName}' does not declare a default expression.");

        var propertySymbol = snapshot.ComponentSymbol
            .GetMembers(prop.PublicName)
            .OfType<IPropertySymbol>()
            .FirstOrDefault(static candidate => !candidate.IsStatic);
        if (propertySymbol is null ||
            propertySymbol.DeclaringSyntaxReferences.Length == 0 ||
            prop.DefaultSource != VuePropDefaultSource.PropertyInitializer)
        {
            return prop.DefaultExpression!;
        }

        foreach (var reference in propertySymbol.DeclaringSyntaxReferences)
        {
            if (reference.GetSyntax() is not PropertyDeclarationSyntax declaration ||
                declaration.Initializer?.Value is null)
            {
                continue;
            }

            var semanticModel = snapshot.Compilation.GetSemanticModel(declaration.SyntaxTree);
            if (!RazorVuePropertyInitializerHelper.TryGetNormalizedOperation(
                    semanticModel,
                    declaration.Initializer.Value,
                    out var operation))
            {
                continue;
            }

            return expressionEmitter.EmitSetupExpression(operation!);
        }

        return prop.DefaultExpression!;
    }

    private sealed record DefaultPropEntry(string PropName, string ExpressionText);

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
            case RazorVueSfcSetupBindingKind.LocalAlias:
                builder.Append("const ")
                    .Append(binding.Name)
                    .Append(" = ")
                    .Append(binding.ExpressionText)
                    .AppendLine(";");
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
        if (templateScopeDepth == 0 &&
            bindingSiteMap.TryGetValue(path, out var resolvedExpression))
        {
            return resolvedExpression;
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
            .Replace(">", "&gt;")
            .Replace("{{", "&#123;&#123;");

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
