import { packageChecker } from './package-checker.mjs';
import { contractHelpers, contractAliases, readComponent } from './vue-contract.mjs';
import { webIdlProjection } from './web-idl-projection.mjs';
import { readWebTypes } from './vue-web-types.mjs';

const [typescript, moduleSpecifier, exportNamesJson, webIdlCarriersJson] = globalThis.__jazorArguments;
const exportNames = JSON.parse(exportNamesJson);
// Keep per-component aliases in separate scopes while sharing the package and Vue imports.
const { ts, source, program, host, snapshotInputs, definition, packageIdentity, packageManifest, readInput } =
    await packageChecker(import.meta.url, typescript, moduleSpecifier, () => `
        import * as packageExports from ${JSON.stringify(moduleSpecifier)};
        import type { VNodeProps, AllowedComponentProps, ComponentCustomProps } from 'vue';
        type VueProps = keyof (VNodeProps & AllowedComponentProps & ComponentCustomProps);
        ${exportNames.length ? contractHelpers : ''}
        ${exportNames.map((name, index) => `namespace Component${index} {
            ${contractAliases(`(typeof packageExports)[${JSON.stringify(name)}]`)}
        }`).join('\n')}
    `);
const checker = program.getTypeChecker();
const formatFlags = ts.TypeFormatFlags.NoTruncation | ts.TypeFormatFlags.UseAliasDefinedOutsideCurrentScope;
const aliases = checker.getExportsOfModule(checker.getSymbolAtLocation(source.statements[0].moduleSpecifier));
const unalias = symbol => symbol.flags & ts.SymbolFlags.Alias ? checker.getAliasedSymbol(symbol) : symbol;
const typeOf = symbol => checker.getTypeOfSymbolAtLocation(symbol, source);
const display = type => checker.typeToString(type, source, formatFlags);
const optional = symbol => Boolean(symbol.flags & ts.SymbolFlags.Optional);
const isComponent = symbol => {
    const type = typeOf(unalias(symbol));
    return type.getConstructSignatures().some(signature => {
        const instance = signature.getReturnType();
        return ['$props', '$emit', '$slots'].every(name => instance.getProperty(name));
    }) || type.getCallSignatures().some(signature => {
        const context = signature.getReturnType().getProperty('__ctx');
        return context && ['props', 'emit', 'slots'].every(name => checker.getNonNullableType(typeOf(context)).getProperty(name));
    });
};
let result;
if (exportNames.length) {
    const exportsByName = new Map(aliases.map(symbol => [symbol.name, symbol]));
    const scopes = source.statements.filter(ts.isModuleDeclaration);
    const { webType } = webIdlProjection(ts, program, source, host, JSON.parse(webIdlCarriersJson));
    const identity = packageIdentity();
    const webTypes = readWebTypes(ts, packageManifest, readInput);
    // Keep authored export names as values so snapshot path-field names remain unambiguous.
    result = { components: exportNames.map((exportName, index) => {
        try {
            const symbol = exportsByName.get(exportName);
            if (!symbol || !isComponent(symbol))
                throw new Error(`'${moduleSpecifier}' has no supported Vue component export named '${exportName}'.`);
            const component = readComponent(unalias(symbol), { ts, source, checker, typeOf, display, optional, definition,
                identity, exportName, webType, scope: scopes[index].body });
            const metadata = webTypes.get(symbol.name);
            if (metadata) {
                // Descriptions supplement the checker result. Navigation stays on the original declaration.
                addDescription(component.definition, metadata.description);
                for (const [name, prop] of Object.entries(component.props)) {
                    const kebab = name.replace(/[A-Z]/g, letter => '-' + letter.toLowerCase());
                    addDescription(prop.definition, metadata.props.get(name) ?? metadata.props.get(kebab));
                }
                for (const [name, slot] of metadata.slots)
                    component.slots[name] ??= slot;
            }
            return { exportName, result: { component } };
        } catch (error) {
            return { exportName, result: { error: error.message } };
        }
    }) };
} else {
    result = { exports: aliases.filter(isComponent).map(symbol => symbol.name).sort() };
}
console.log(JSON.stringify({ ...result, ...snapshotInputs() }));

function addDescription(definition, summary) {
    if (definition && summary?.trim() && !definition.documentation?.summary)
        definition.documentation = { ...definition.documentation, summary };
}
