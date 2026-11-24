import { packageChecker } from './package-checker.mjs';
import { contractTypes, readComponent } from './vue-contract.mjs';
import { webIdlProjection } from './web-idl-projection.mjs';

const [typescript, moduleSpecifier, exportName, webIdlCarriersJson] = globalThis.__jazorArguments;
const { ts, source, program, host, inputs, modulePath, realpaths, definition, packageIdentity } =
    await packageChecker(import.meta.url, typescript, moduleSpecifier, () => `
        import * as packageExports from ${JSON.stringify(moduleSpecifier)};
        import type { VNodeProps, AllowedComponentProps, ComponentCustomProps } from 'vue';
        type VueProps = keyof (VNodeProps & AllowedComponentProps & ComponentCustomProps);
        ${exportName ? `type Component = (typeof packageExports)[${JSON.stringify(exportName)}];
            ${contractTypes('Component')}` : ''}
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
if (exportName) {
    const symbol = aliases.find(symbol => symbol.name === exportName);
    if (!symbol || !isComponent(symbol))
        throw new Error(`'${moduleSpecifier}' has no supported Vue component export named '${exportName}'.`);
    const { webType } = webIdlProjection(ts, program, source, host, JSON.parse(webIdlCarriersJson));
    result = { component: readComponent(unalias(symbol), { ts, source, checker, typeOf, display, optional, definition,
        identity: packageIdentity(), exportName, webType }) };
} else {
    result = { exports: aliases.filter(isComponent).map(symbol => symbol.name).sort() };
}
console.log(JSON.stringify({ ...result, inputs: [...inputs], modulePath, realpaths: Object.fromEntries(realpaths) }));
