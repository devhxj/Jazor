import fs from 'node:fs';
import path from 'node:path';
import { fileURLToPath, pathToFileURL } from 'node:url';

const [typescript, moduleSpecifier, exportName] = globalThis.__jazorArguments;
const { default: ts } = await import(pathToFileURL(typescript));
const modulePath = fileURLToPath(import.meta.resolve(moduleSpecifier));
const root = path.dirname(path.dirname(fileURLToPath(import.meta.url)));
const entry = path.join(root, 'jolt-metadata.mts');
const options = {
    target: ts.ScriptTarget.ES2022,
    module: ts.ModuleKind.ESNext,
    moduleResolution: ts.ModuleResolutionKind.Bundler,
    strict: true,
    exactOptionalPropertyTypes: true,
    skipLibCheck: true,
    types: [],
};
const host = ts.createCompilerHost(options);
const formatFlags = ts.TypeFormatFlags.NoTruncation | ts.TypeFormatFlags.UseAliasDefinedOutsideCurrentScope;
// Vue publishes both public instance constructors and callable generic SFCs.
// Callable SFCs carry their public context on the returned VNode.
const source = ts.createSourceFile(entry, `
    import * as packageExports from ${JSON.stringify(moduleSpecifier)};
    import type { VNodeProps, AllowedComponentProps, ComponentCustomProps } from 'vue';
    type VueProps = keyof (VNodeProps & AllowedComponentProps & ComponentCustomProps);
    ${exportName === undefined ? '' : `
        type Component = (typeof packageExports)[${JSON.stringify(exportName)}];
        type Shape<C> = C extends abstract new (...args: any[]) => infer I
            ? I extends { $props: infer P; $emit: infer E; $slots: infer S }
                ? { props: P; emit: E; slots: S }
                : never
            : C extends (props: infer P, ...args: any[]) => { __ctx?: infer Context }
                ? NonNullable<Context> extends { emit: infer E; slots: infer S }
                    ? { props: P; emit: E; slots: S }
                    : never
                : never;
        type Instance = Shape<Component>;
        type Props = Omit<Instance['props'], VueProps>;
        type PropValues = Required<Props>;
        type Emit = Instance['emit'];
        type Slots = Instance['slots'];
    `}
`, options.target, true);
const inputs = new Set([modulePath]);
// Runtime conditions can change even when TypeScript follows the same types entry.
for (let directory = path.dirname(modulePath); directory !== path.dirname(directory); directory = path.dirname(directory)) {
    const manifest = path.join(directory, 'package.json');
    inputs.add(manifest);
    if (fs.existsSync(manifest) && JSON.parse(fs.readFileSync(manifest, 'utf8')).name)
        break;
}
const readFile = host.readFile.bind(host);
host.readFile = file => {
    const result = readFile(file);
    if (result !== undefined)
        inputs.add(path.resolve(file));
    return result;
};
const getSourceFile = host.getSourceFile.bind(host);
host.getSourceFile = (file, ...args) => file === entry ? source : getSourceFile(file, ...args);
const fileExists = host.fileExists.bind(host);
host.fileExists = file => file === entry || fileExists(file);
const program = ts.createProgram([entry], options, host);
const diagnostics = ts.getPreEmitDiagnostics(program);
if (diagnostics.length)
    throw new Error(ts.formatDiagnostics(diagnostics, host));

const checker = program.getTypeChecker();
const moduleSymbol = checker.getSymbolAtLocation(source.statements[0].moduleSpecifier);
const aliases = checker.getExportsOfModule(moduleSymbol);
const unalias = symbol => symbol.flags & ts.SymbolFlags.Alias ? checker.getAliasedSymbol(symbol) : symbol;
const typeOf = symbol => checker.getTypeOfSymbolAtLocation(symbol, source);
const display = type => checker.typeToString(type, source, formatFlags);
const optional = symbol => Boolean(symbol.flags & ts.SymbolFlags.Optional);
const definition = node => {
    if (!node)
        return null;
    const target = node.name ?? node;
    return {
        path: path.resolve(target.getSourceFile().fileName),
        start: target.getStart(),
        length: target.getWidth(),
    };
};
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

if (exportName === undefined) {
    const exports = aliases.filter(isComponent).map(symbol => symbol.name).sort();
    console.log(JSON.stringify({ exports, inputs: [...inputs] }));
} else {
    const symbol = aliases.find(symbol => symbol.name === exportName);
    if (!symbol || !isComponent(symbol))
        throw new Error(`'${moduleSpecifier}' has no supported Vue component export named '${exportName}'.`);
    const exportSymbol = unalias(symbol);
    const component = readComponent(exportSymbol);
    console.log(JSON.stringify({ component, inputs: [...inputs] }));
}

function readComponent(exportSymbol) {
    const types = Object.fromEntries(source.statements.filter(ts.isTypeAliasDeclaration)
        .map(alias => [alias.name.text, checker.getTypeAtLocation(alias.type)]));
    const describeParameter = (name, type) => {
        const members = type.flags & ts.TypeFlags.Object && !checker.isArrayType(type) && !checker.isTupleType(type)
            ? type.getProperties()
            : [];
        return {
            name,
            type: display(type),
            properties: members.length ? Object.fromEntries(members.map(member => [member.name, display(typeOf(member))])) : null,
        };
    };
    const parameters = (signature, first = 0) => signature.parameters.slice(first).flatMap(parameter => {
        const type = typeOf(parameter);
        const declaration = parameter.valueDeclaration ?? parameter.declarations?.[0];
        // Vue encodes emit payloads as rest tuples. Expand fixed tuples so each
        // call argument retains its own checked type and declaration label.
        const isRest = declaration?.dotDotDotToken ||
            (signature.parameters.at(-1) === parameter && checker.hasEffectiveRestParameter(signature));
        if (isRest && checker.isTupleType(type) && type.target.elementFlags.every(flag => flag === ts.ElementFlags.Required)) {
            return checker.getTypeArguments(type).map((element, index) =>
                describeParameter(type.target.labeledElementDeclarations?.[index]?.name.getText() ?? `arg${index + 1}`, element));
        }
        return [describeParameter(parameter.name, type)];
    });
    const events = Object.create(null);
    for (const signature of types.Emit.getCallSignatures()) {
        const [event] = signature.parameters;
        const eventType = event && typeOf(event);
        const names = eventType?.isUnion() ? eventType.types : [eventType];
        for (const name of names.filter(type => type?.isStringLiteral())) {
            const item = events[name.value] ??= { signatures: [], definition: null };
            item.signatures.push(parameters(signature, 1));
            const listener = types.Props.getProperty(listenerName(name.value));
            item.definition ??= definition(listener?.declarations?.[0] ?? signature.declaration);
        }
    }
    const listeners = new Set(Object.keys(events).map(listenerName));
    const props = Object.fromEntries(types.Props.getProperties()
        .filter(prop => !listeners.has(prop.name))
        .map(prop => {
            const type = checker.getTypeOfPropertyOfType(types.PropValues, prop.name);
            const branches = type.isUnion() ? type.types : [type];
            const values = branches.filter(branch => branch.isStringLiteral()).map(branch => branch.value);
            return [prop.name, {
                type: display(type),
                required: !optional(prop),
                literalValues: values,
                definition: definition(prop.declarations?.[0]),
            }];
        }));
    const slots = Object.fromEntries(types.Slots.getProperties().map(slot => [slot.name, {
        required: !optional(slot),
        signatures: checker.getNonNullableType(typeOf(slot)).getCallSignatures().map(signature => parameters(signature)),
        definition: definition(slot.declarations?.[0]),
    }]));

    const resolution = ts.resolveModuleName(moduleSpecifier, entry, options, host).resolvedModule;
    if (!resolution)
        throw new Error(`Cannot resolve TypeScript declarations for '${moduleSpecifier}'.`);
    const packageId = resolution.packageId;
    if (!packageId)
        throw new Error(`Cannot resolve npm package metadata for '${moduleSpecifier}'.`);
    return {
        moduleSpecifier, exportName, packageName: packageId.name, version: packageId.version,
        modulePath, declarationPath: path.resolve(resolution.resolvedFileName),
        definition: definition(exportSymbol.valueDeclaration ?? exportSymbol.declarations?.[0]),
        props, events, slots,
    };
}

function listenerName(name) {
    return `on${name.charAt(0).toUpperCase()}${name.slice(1)}`;
}
