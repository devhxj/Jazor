import fs from 'node:fs';
import { contractTypes, readComponent } from './vue-contract.mjs';
import path from 'node:path';
import { fileURLToPath, pathToFileURL } from 'node:url';

const [typescript, moduleSpecifier, mode, query] = globalThis.__jazorArguments;
const { default: ts } = await import(pathToFileURL(typescript));
const modulePath = fileURLToPath(import.meta.resolve(moduleSpecifier));
const isValue = mode !== 'component';
const exportName = mode === 'values' ? undefined : query;
const root = path.resolve(path.dirname(fileURLToPath(import.meta.url)), '../..');
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
    ${isValue ? '' : `
        import type { VNodeProps, AllowedComponentProps, ComponentCustomProps } from 'vue';
        type VueProps = keyof (VNodeProps & AllowedComponentProps & ComponentCustomProps);
    `}
    ${isValue || !exportName ? '' : `
        type Component = (typeof packageExports)[${JSON.stringify(exportName)}];
        ${contractTypes("Component")}
    `}
`, options.target, true);
const inputs = new Set([modulePath]);
// Runtime conditions can change even when TypeScript follows the same types entry.
let moduleType;
for (let directory = path.dirname(modulePath); directory !== path.dirname(directory); directory = path.dirname(directory)) {
    const manifest = path.join(directory, 'package.json');
    inputs.add(manifest);
    if (!fs.existsSync(manifest))
        continue;
    const metadata = JSON.parse(fs.readFileSync(manifest, 'utf8'));
    moduleType ??= metadata.type ?? 'commonjs';
    if (metadata.name)
        break;
}
if (isValue && (!fs.existsSync(modulePath) ||
    !(modulePath.endsWith('.mjs') || modulePath.endsWith('.js') && moduleType === 'module')))
    throw new Error(`'${moduleSpecifier}' does not resolve to a public ESM file.`);
const readFile = host.readFile.bind(host);
host.readFile = file => {
    const result = readFile(file);
    if (result !== undefined)
        inputs.add(path.resolve(file));
    return result;
};
const getSourceFile = host.getSourceFile.bind(host);
// TypeScript passes normalized slashes even when the host constructs a Windows path.
const entryKey = host.getCanonicalFileName(path.normalize(entry));
const isEntry = file => host.getCanonicalFileName(path.normalize(file)) === entryKey;
host.getSourceFile = (file, ...args) => isEntry(file) ? source : getSourceFile(file, ...args);
const fileExists = host.fileExists.bind(host);
host.fileExists = file => isEntry(file) || fileExists(file);
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

const valueSymbol = alias => {
    if (alias.flags & ts.SymbolFlags.Alias && checker.getTypeOnlyAliasDeclaration(alias))
        return undefined;
    const symbol = unalias(alias);
    if (symbol.flags & ts.SymbolFlags.Value && symbol.valueDeclaration)
        return symbol;
};
const isValueCandidate = alias => {
    const symbol = valueSymbol(alias);
    return symbol && (typeOf(symbol).getCallSignatures().length || scalar(typeOf(symbol), true) && isConstant(symbol.valueDeclaration));
};

class UnsupportedValueError extends Error {}

if (mode === 'values') {
    const requested = new Set(JSON.parse(query));
    const values = [];
    for (const alias of aliases) {
        if (!requested.has(alias.name))
            continue;
        try {
            values.push(readValue(alias));
        } catch (error) {
            if (!(error instanceof UnsupportedValueError))
                throw error;
        }
    }
    console.log(JSON.stringify({ values, exports: valueCandidates(), inputs: [...inputs] }));
} else if (!exportName) {
    const exports = isValue ? valueCandidates() : aliases.filter(isComponent).map(symbol => symbol.name).sort();
    console.log(JSON.stringify({ exports, inputs: [...inputs] }));
} else {
    const symbol = aliases.find(symbol => symbol.name === exportName);
    if (isValue) {
        if (!symbol)
            throw new Error(`'${moduleSpecifier}' has no export named '${exportName}'.`);
        const value = readValue(symbol);
        console.log(JSON.stringify({ value, exports: valueCandidates(), inputs: [...inputs] }));
    } else {
        if (!symbol || !isComponent(symbol))
            throw new Error(`'${moduleSpecifier}' has no supported Vue component export named '${exportName}'.`);
        const component = readComponent(unalias(symbol), { ts, source, checker, typeOf, display, optional, definition, identity: packageIdentity(), exportName });
        console.log(JSON.stringify({ component, inputs: [...inputs] }));
    }
}

function valueCandidates() {
    return aliases.filter(isValueCandidate).map(symbol => symbol.name).sort();
}

function scalar(type, literals = false) {
    const flags = type.flags;
    if (flags & ts.TypeFlags.String || literals && flags & ts.TypeFlags.StringLiteral)
        return 'string';
    if (flags & ts.TypeFlags.Number || literals && flags & ts.TypeFlags.NumberLiteral)
        return 'number';
    if (flags & ts.TypeFlags.Boolean || literals && flags & ts.TypeFlags.BooleanLiteral)
        return 'boolean';
}

function isConstant(node) {
    // An anonymous default scalar is represented by an export assignment.
    return ts.isVariableDeclaration(node) && Boolean(node.parent.flags & ts.NodeFlags.Const) ||
        ts.isExportAssignment(node) && !node.isExportEquals;
}

function readValue(alias) {
    const symbol = valueSymbol(alias);
    const unsupported = detail => {
        throw new UnsupportedValueError(`Unsupported npm value export '${alias.name}' from '${moduleSpecifier}': ${detail}.`);
    };
    if (!symbol)
        unsupported('expected a runtime value');
    const type = typeOf(symbol);
    const definitionNode = symbol.valueDeclaration;
    const calls = type.getCallSignatures();
    const identity = { ...packageIdentity(), exportName: alias.name };
    if (calls.length) {
        const signatures = calls.map(signature => readSignature(signature, new Set(), unsupported));
        return { ...identity, definition: definition(definitionNode), signatures, constantType: null };
    }
    const kind = scalar(type, true);
    if (!kind || !isConstant(definitionNode))
        unsupported(`expected a function or scalar constant, found '${display(type)}'`);
    return { ...identity, definition: definition(definitionNode), signatures: [], constantType: { kind } };
}

function readSignature(signature, ancestors, unsupported) {
    if (signature.typeParameters?.length || checker.hasEffectiveRestParameter(signature) || signature.thisParameter)
        unsupported(`generic, rest or explicit this signature '${checker.signatureToString(signature)}'`);
    const parameters = signature.parameters.map((parameter, index) => {
        const isOptional = index >= signature.minArgumentCount;
        const type = typeOf(parameter);
        return {
            name: parameter.name,
            type: readType(readParameterType(type, isOptional), ancestors, unsupported),
            optional: isOptional,
        };
    });
    const result = signature.getReturnType();
    return {
        parameters,
        returnType: result.flags & ts.TypeFlags.Void ? { kind: 'void' } : readType(result, ancestors, unsupported),
        definition: definition(signature.declaration),
    };
}

function omitUndefined(type) {
    if (!type.isUnion())
        return type;
    const rest = type.types.filter(branch => !(branch.flags & ts.TypeFlags.Undefined));
    if (rest.length === 1)
        return rest[0];
    if (rest.length === 2 && rest.every(branch => branch.flags & ts.TypeFlags.BooleanLiteral))
        return checker.getBooleanType();
    return type;
}

function readParameterType(type, isOptional) {
    // Optional parameters can flatten Vue's MaybeRef alias into its scalar
    // and ref branches. Project the scalar branch for C# input.
    if (type.isUnion()) {
        const branches = isOptional ? type.types.filter(branch => !(branch.flags & ts.TypeFlags.Undefined)) : type.types;
        const element = branches.find(branch => scalar(branch));
        if (element && branches.every(branch => branch === element || vueRef(branch)?.element === element))
            return element;
    }
    return isOptional ? omitUndefined(type) : type;
}

function vueRef(type) {
    if (type.isUnion()) {
        const branches = type.types.map(vueRef);
        const first = branches[0];
        if (first && branches.every(branch => branch && branch.element === first.element &&
            branch.readOnly === first.readOnly && branch.distinct === first.distinct))
            return first;
        return;
    }
    if (!(type.flags & ts.TypeFlags.Object) && !type.isIntersection())
        return;
    const value = type.getProperty('value');
    // Vue's own unique-symbol brand follows Ref through derived interfaces and
    // mapped types such as Readonly<Ref<T>>.
    const brand = type.getProperties().find(member => member.declarations?.some(node =>
        ts.isComputedPropertyName(node.name) && node.name.expression.getText() === 'RefSymbol' &&
        node.getSourceFile().fileName.replaceAll('\\', '/').includes('/node_modules/@vue/reactivity/')));
    if (!value || !brand)
        return;
    const element = typeOf(value);
    const readOnly = Boolean(ts.getCheckFlags(value) & ts.CheckFlags.Readonly) ||
        !value.declarations?.some(ts.isSetAccessorDeclaration);
    const [read, write = read] = type.objectFlags & ts.ObjectFlags.Reference ? checker.getTypeArguments(type) : [];
    return { element, readOnly, distinct: !readOnly && read && read !== write };
}

function readType(type, ancestors, unsupported) {
    const kind = scalar(type);
    if (kind)
        return { kind };
    const ref = vueRef(type);
    if (ref) {
        if (ref.distinct || ancestors.has(type))
            unsupported(`Vue ref '${display(type)}'`);
        return {
            kind: 'ref', readOnly: ref.readOnly,
            elementType: readType(ref.element, new Set(ancestors).add(type), unsupported),
        };
    }
    if (!(type.flags & ts.TypeFlags.Object) || ancestors.has(type) || checker.isArrayType(type) ||
        checker.isTupleType(type) || checker.getIndexInfosOfType(type).length || type.getConstructSignatures().length)
        unsupported(`type '${display(type)}'`);
    const nested = new Set(ancestors).add(type);
    const calls = type.getCallSignatures();
    const properties = type.getProperties();
    if (calls.length === 1 && !properties.length)
        return { kind: 'function', signatures: calls.map(signature => readSignature(signature, nested, unsupported)) };
    if (calls.length || !properties.length || type.getSymbol()?.flags & ts.SymbolFlags.Class)
        unsupported(`type '${display(type)}'`);
    const members = properties.map(member => {
        const declarations = member.declarations ?? [];
        if (!declarations.length || declarations.some(node => node.name && ts.isComputedPropertyName(node.name)))
            unsupported(`member '${member.name}' of '${display(type)}'`);
        const memberType = typeOf(member);
        const isOptional = optional(member);
        const readOnly = Boolean(ts.getCheckFlags(member) & ts.CheckFlags.Readonly) ||
            declarations.some(node => ts.getCombinedModifierFlags(node) & ts.ModifierFlags.Readonly) ||
            declarations.some(ts.isGetAccessorDeclaration) && !declarations.some(ts.isSetAccessorDeclaration);
        return {
            name: member.name,
            type: readType(isOptional ? omitUndefined(memberType) : memberType, nested, unsupported),
            optional: isOptional,
            readOnly,
            definition: definition(declarations[0]),
        };
    });
    return { kind: 'object', members };
}

function packageIdentity() {
    const resolution = ts.resolveModuleName(moduleSpecifier, entry, options, host).resolvedModule;
    if (!resolution)
        throw new Error(`Cannot resolve TypeScript declarations for '${moduleSpecifier}'.`);
    const packageId = resolution.packageId;
    if (!packageId)
        throw new Error(`Cannot resolve npm package metadata for '${moduleSpecifier}'.`);
    if (isValue && !/\.d\.(?:ts|mts)$/.test(resolution.resolvedFileName))
        throw new Error(`'${moduleSpecifier}' must publish TypeScript declarations for its ESM entry.`);
    return {
        moduleSpecifier, packageName: packageId.name, version: packageId.version,
        modulePath, declarationPath: path.resolve(resolution.resolvedFileName),
    };
}
