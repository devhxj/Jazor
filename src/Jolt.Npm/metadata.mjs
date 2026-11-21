import { readCallableProtocol, readProtocolArray } from './callable-protocol-projection.mjs';
import { readSubmissionProjection } from './submission-projection.mjs';
import { pagedQueryProbes, readPagedQueries } from './paged-query-projection.mjs';
import fs from 'node:fs';
import { behaviorProbes, readBehaviorProjections } from './callback-behavior.mjs';
import { domEventProbes, readDomEventProjections } from './dom-event-projection.mjs';
import { asyncStateProbes, readAsyncStateProbes, readCorrelatedRefCall, refUpdaterProbes } from './correlated-ref-projection.mjs';
import { dateArguments, dateProjection, readDateProjection } from './date-specializations.mjs';
import { omittedOptionsProbes, vueComposableProjection } from './vue-composable-projection.mjs';
import { alternatives, callbackProbes, hasConditionalAnnotation, singleCall } from './generic-callbacks.mjs';
import { genericCallbackProjections, readGenericCallbackProjections } from './generic-callback-projections.mjs';
import { inputClosureSlot, readSchemaFormProjections, schemaFormProbes } from './schema-form-projection.mjs';
import { contractTypes, readComponent } from './vue-contract.mjs';
import { pluginFactoryProbes, projectVuePlugin, readPluginFactory } from './vue-plugin.mjs';
import path from 'node:path';
import { fileURLToPath, pathToFileURL } from 'node:url';

const [typescript, moduleSpecifier, mode, query, webIdlCarriersJson] = globalThis.__jazorArguments;
const webIdlCarriers = JSON.parse(webIdlCarriersJson);
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
const defaultLibraryDirectory = path.dirname(ts.getDefaultLibFilePath(options));
const realpaths = new Map();
const realpath = host.realpath.bind(host);
// A package link can move while the original target and the files probed
// through the link still have the same contents.
host.realpath = file => {
    const resolved = realpath(file);
    realpaths.set(path.resolve(file), path.resolve(resolved));
    return resolved;
};
const vueAvailable = isValue && ts.resolveModuleName('vue', entry, options, host).resolvedModule;
const formatFlags = ts.TypeFormatFlags.NoTruncation | ts.TypeFormatFlags.UseAliasDefinedOutsideCurrentScope;
// Vue publishes both public instance constructors and callable generic SFCs.
// Callable SFCs carry their public context on the returned VNode.
let source = ts.createSourceFile(entry, `
    import * as packageExports from ${JSON.stringify(moduleSpecifier)};
    ${isValue ? 'type JoltStringDictionary = Record<string, string>;' : ''}
    ${vueAvailable ? "type JoltVueObjectPlugin = import('vue').ObjectPlugin<[]>;" : ''}
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
    if (moduleType === undefined)
        moduleType = metadata.type ?? null;
    if (metadata.name)
        break;
}
const exists = fs.existsSync(modulePath);
const ambiguousJs = exists && modulePath.endsWith('.js') && moduleType == null;
const lexicalEsm = isValue && ambiguousJs &&
    ts.createSourceFile(modulePath, fs.readFileSync(modulePath, 'utf8'), options.target, true, ts.ScriptKind.JS)
        .statements.some(statement => ts.isImportDeclaration(statement) || ts.isExportDeclaration(statement) ||
            ts.isExportAssignment(statement) && !statement.isExportEquals ||
            statement.modifiers?.some(modifier => modifier.kind === ts.SyntaxKind.ExportKeyword));
if (isValue && (!exists ||
    !(modulePath.endsWith('.mjs') || modulePath.endsWith('.js') && (moduleType === 'module' || lexicalEsm))))
    throw new Error(`'${moduleSpecifier}' does not resolve to a public ESM file.`);
const readFile = host.readFile.bind(host);
host.readFile = file => {
    inputs.add(path.resolve(file));
    const result = readFile(file);
    return result;
};
const getSourceFile = host.getSourceFile.bind(host);
// TypeScript passes normalized slashes even when the host constructs a Windows path.
const entryKey = host.getCanonicalFileName(path.normalize(entry));
const isEntry = file => host.getCanonicalFileName(path.normalize(file)) === entryKey;
host.getSourceFile = (file, ...args) => isEntry(file) ? source : getSourceFile(file, ...args);
const fileExists = host.fileExists.bind(host);
host.fileExists = file => {
    if (isEntry(file))
        return true;
    inputs.add(path.resolve(file));
    return fileExists(file);
};
const directoryExists = host.directoryExists.bind(host);
host.directoryExists = directory => {
    inputs.add(path.resolve(directory));
    return directoryExists(directory);
};
let program = ts.createProgram([entry], options, host);
const diagnostics = ts.getPreEmitDiagnostics(program);
if (diagnostics.length)
    throw new Error(ts.formatDiagnostics(diagnostics, host));

let checker = program.getTypeChecker();
const getAliases = () => checker.getExportsOfModule(checker.getSymbolAtLocation(source.statements[0].moduleSpecifier));
let aliases = getAliases();
const unalias = symbol => symbol.flags & ts.SymbolFlags.Alias ? checker.getAliasedSymbol(symbol) : symbol;
const typeOf = symbol => checker.getTypeOfSymbolAtLocation(symbol, source);
const display = type => checker.typeToString(type, source, formatFlags);
const optional = symbol => Boolean(symbol.flags & ts.SymbolFlags.Optional);
const definition = node => {
    if (!node || isEntry(node.getSourceFile().fileName))
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
    return symbol && (symbol.flags & ts.SymbolFlags.ValueModule ||
        typeOf(symbol).getCallSignatures().length || isConstant(symbol.valueDeclaration) &&
        (scalar(typeOf(symbol), true) || readVuePlugin(typeOf(symbol)) || namedCallableRecord(typeOf(symbol)) ||
            undefinedableConstant(typeOf(symbol))));
};
const undefinedableConstant = (type, unsupported = detail => { throw new UnsupportedValueError(detail); }) => {
    if (!type.isUnion() || !type.types.some(part => part.flags & ts.TypeFlags.Undefined))
        return;
    const projected = attempt(() => readType(type, new Set(), 'output', false, unsupported)).value;
    if (projected?.kind === 'undefinedable')
        return projected;
};

const requestedValues = mode === 'values' ? new Set(JSON.parse(query)) : new Set(exportName ? [exportName] : []);
let dateTypes = dateArguments(ts, program, checker, source);
const callsOf = alias => {
    const symbol = valueSymbol(alias);
    return symbol ? typeOf(symbol).getCallSignatures() : [];
};
const projections = isValue ? dateProjection(aliases, callsOf, requestedValues, dateTypes.argumentsFor) : [];
const callbackTypes = isValue ? callbackProbes(ts, program, checker, aliases, callsOf, requestedValues) : [];
const validatorProbes = isValue ? genericCallbackProjections(ts, program, checker, source, aliases, callsOf, requestedValues, typeOf) : [];
const formProbes = isValue ? schemaFormProbes(ts, program, checker, source, aliases, callsOf, requestedValues, typeOf) : [];
const pagingProbes = isValue ? pagedQueryProbes(aliases, callsOf, requestedValues, typeOf) : [];
// Resolve the exact zero-argument call when a composable declares defaults.
// Supplying options can infer different generics, so this result belongs only to ().
const defaultCalls = aliases.filter(alias => requestedValues.has(alias.name) && valueSymbol(alias) &&
    typeOf(valueSymbol(alias)).getCallSignatures().some(signature => signature.minArgumentCount === 0 &&
        hasOmittedTypeArguments(signature) &&
        !signature.thisParameter && !checker.hasEffectiveRestParameter(signature)));
const pluginFactories = vueAvailable ? pluginFactoryProbes(ts, checker, aliases, callsOf, requestedValues,
    typeOf, vueObjectPluginContract(), defaultCalls,
    checker.getDeclaredTypeOfSymbol(checker.resolveName('JoltStringDictionary', source, ts.SymbolFlags.Type, false))) : [];
const eventProbes = isValue ? domEventProbes(ts, () => checker, aliases, callsOf, requestedValues,
    typeOf, domSymbol, bindWebTypes()) : [];
const behaviorCalls = isValue ? behaviorProbes(ts, checker, aliases, callsOf, requestedValues, typeOf) : [];
const omittedOptionCalls = isValue ? omittedOptionsProbes(ts, checker, aliases, callsOf, requestedValues, typeOf, dateTypes.isDate) : [];
function vueObjectPluginContract() {
    const declaration = source.statements.find(node => ts.isTypeAliasDeclaration(node) && node.name.text === 'JoltVueObjectPlugin');
    return declaration && checker.getTypeAtLocation(declaration.type);
}
const elementRefCalls = isValue ? aliases.filter(alias => requestedValues.has(alias.name) &&
    callsOf(alias).length === 1 && elementRefParameters(callsOf(alias)[0])) : [];
const stateProbes = isValue ? [
    ...asyncStateProbes(checker, aliases, callsOf, requestedValues, typeOf,
        checker.resolveName('Promise', source, ts.SymbolFlags.Type, false)),
    ...refUpdaterProbes(checker, aliases, callsOf, requestedValues, typeOf, vueRef),
] : [];
const probeExpressions = new Map();
const addProbes = (group, expression = probe => probe.expression) => {
    probeExpressions.set(group, group.map(expression));
};
addProbes(projections);
addProbes(defaultCalls, alias => `packageExports[${JSON.stringify(alias.name)}]()`);
addProbes(callbackTypes);
addProbes(validatorProbes);
addProbes(pluginFactories);
addProbes(eventProbes);
addProbes(behaviorCalls);
addProbes(omittedOptionCalls);
addProbes(stateProbes);
addProbes(formProbes);
addProbes(pagingProbes);
addProbes(elementRefCalls, alias => {
    const names = elementRefParameters(callsOf(alias)[0]);
    const args = names.map(name => `{} as Readonly<import('vue').Ref<${name} | null>>`);
    return `packageExports[${JSON.stringify(alias.name)}](${args.join(', ')})`;
});
const expressions = [...probeExpressions.values()].flat();
let projectedStatements = [];
if (expressions.length) {
    const statementCount = source.statements.length;
    const projectedText = expressions.map((expression, index) => `const projection${index} = ${expression};`).join('\n');
    const behaviorMarkers = behaviorCalls.length ? ['JoltCallback_TValue', 'JoltCallback_TResult'] : [];
    const stateMarkers = stateProbes.some(probe => probe.state) ? ['JoltAsyncStateValue'] : [];
    const markerNames = [...new Set([...callbackTypes.flatMap(probe => probe.names), ...behaviorMarkers, ...stateMarkers])];
    const markers = markerNames
        .map(name => `declare class ${name} { private ${name}: never; }`).join('\n');
    source = ts.createSourceFile(entry, `${source.text}\n${projectedText}\n${markers}`, options.target, true);
    program = ts.createProgram([entry], options, host, program);
    checker = program.getTypeChecker();
    aliases = getAliases();
    dateTypes = dateArguments(ts, program, checker, source);
    projectedStatements = source.statements.slice(statementCount);
}
// Each checker pass resolves the statements for its own probes. Offsets depend
// only on the registration order, including groups empty for this package.
const statementsByGroup = new Map();
let start = 0;
for (const [group, expressions] of probeExpressions) {
    statementsByGroup.set(group, projectedStatements.slice(start, start + expressions.length));
    start += expressions.length;
}
const { isDate, argumentsFor } = dateTypes;
const probeDiagnostics = expressions.length ? program.getSemanticDiagnostics(source) : [];
const dateSpecializations = readDateProjection(checker, statementsByGroup.get(projections), projections, argumentsFor, probeDiagnostics);
const validators = readGenericCallbackProjections(checker,
    statementsByGroup.get(validatorProbes), validatorProbes, probeDiagnostics);
const resolvedElementRefCalls = new Map();
for (const [index, alias] of elementRefCalls.entries()) {
    const statement = statementsByGroup.get(elementRefCalls)[index];
    if (probeDiagnostics.some(diagnostic => diagnostic.start >= statement.pos && diagnostic.start < statement.end))
        continue;
    const signature = checker.getResolvedSignature(statement.declarationList.declarations[0].initializer);
    const original = signature?.declaration && checker.getSignatureFromDeclaration(signature.declaration);
    const names = original && elementRefParameters(original);
    if (names && !signature.typeParameters?.length)
        resolvedElementRefCalls.set(alias.name, { signature, names });
}
const resolvedPluginFactories = new Map();
for (const [index, probe] of pluginFactories.entries()) {
    const statement = statementsByGroup.get(pluginFactories)[index];
    if (!probeDiagnostics.some(diagnostic => diagnostic.start >= statement.pos && diagnostic.start < statement.end))
        resolvedPluginFactories.set(JSON.stringify([probe.name, probe.fixed ?? null]),
            { probe, signature: checker.getResolvedSignature(statement.declarationList.declarations[0].initializer) });
}
let validatorTypeParameter;
const callbackSignatures = new Map();
let projectingCallbackOptions = false;
const callbackMarkers = new Map([...new Set([...callbackTypes.flatMap(probe => probe.names),
    ...(behaviorCalls.length ? ['JoltCallback_TValue', 'JoltCallback_TResult'] : [])])].map(name =>
    [checker.resolveName(name, source, ts.SymbolFlags.Type, false), name.replace('JoltCallback_', '')]));
for (const [index, probe] of callbackTypes.entries()) {
    const statement = statementsByGroup.get(callbackTypes)[index];
    if (probeDiagnostics.some(diagnostic => diagnostic.start >= statement.pos && diagnostic.start < statement.end))
        continue;
    const signatures = checker.getTypeAtLocation(statement.declarationList.declarations[0].initializer).getCallSignatures();
    const signature = signatures[probe.ordinal];
    if (!signature || signature.typeParameters?.length)
        continue;
    const entries = callbackSignatures.get(probe.exportName) ?? [];
    entries.push({ ...probe, signature, earlierSignatures: signatures.slice(0, probe.ordinal) });
    callbackSignatures.set(probe.exportName, entries);
}
const resolvedDefaults = new Map();
for (const [index, alias] of defaultCalls.entries()) {
    const statement = statementsByGroup.get(defaultCalls)[index];
    if (probeDiagnostics.some(diagnostic => diagnostic.start >= statement.pos && diagnostic.start < statement.end))
        continue;
    const signature = checker.getResolvedSignature(statement.declarationList.declarations[0].initializer);
    const original = signature?.declaration && checker.getSignatureFromDeclaration(signature.declaration);
    if (original && hasOmittedTypeArguments(original) &&
        !original.thisParameter && !checker.hasEffectiveRestParameter(original))
        resolvedDefaults.set(alias.name, signature);
}

function hasOmittedTypeArguments(signature) {
    return signature.typeParameters?.length && signature.typeParameters.every(parameter => parameter.getDefault()) ||
        vueComposableProjection(ts, checker, typeOf, vueRef, omitUndefined).constrainedRefDefault(signature);
}

class UnsupportedValueError extends Error {}

const vuePluginContract = vueObjectPluginContract();

function readVuePlugin(type) {
    return projectVuePlugin(ts, checker, vuePluginContract, type);
}

const webTypes = bindWebTypes();
const promise = checker.resolveName('Promise', source, ts.SymbolFlags.Type, false);
const standardPromise = promise?.declarations?.length &&
    promise.declarations.every(node => program.isSourceFileDefaultLibrary(node.getSourceFile()));
const scalarElements = checker.getUnionType([checker.getBooleanType(), checker.getNumberType(), checker.getStringType()]);
const readonlyScalars = checker.createArrayType(scalarElements, true);
const objectType = checker.getTypeFromTypeNode(ts.factory.createKeywordTypeNode(ts.SyntaxKind.ObjectKeyword));
const readonlyData = checker.createArrayType(checker.getUnionType([scalarElements, objectType]), true);
const schemaForms = readSchemaFormProjections(ts, checker, formProbes,
    statementsByGroup.get(formProbes), probeDiagnostics, definition, vueRef);
const asyncStates = readAsyncStateProbes(ts, checker, stateProbes, statementsByGroup.get(stateProbes), probeDiagnostics, vueRef, definition);
const pagedQueries = readPagedQueries(ts, checker, pagingProbes, statementsByGroup.get(pagingProbes),
    probeDiagnostics, vueRef, standardPromise && promise, definition, readPagedQueryKey);

function readPagedQueryKey(type) {
    for (const candidate of [readonlyData, readonlyScalars]) {
        if (checker.isTypeAssignableTo(candidate, type))
            return readType(candidate, new Set(), 'input', false);
    }
    if (checker.isTypeAssignableTo(checker.createArrayType(checker.getStringType()), type))
        return { kind: 'array', readOnly: true, elementType: { kind: 'string' } };
}

if (mode === 'values') {
    const values = [];
    const errors = new Map(Array.from(requestedValues, name =>
        [name, `'${moduleSpecifier}' has no export named '${name}'.`]));
    for (const alias of aliases) {
        if (!requestedValues.has(alias.name))
            continue;
        try {
            values.push(readValue(alias));
            errors.delete(alias.name);
        } catch (error) {
            if (!(error instanceof UnsupportedValueError))
                throw error;
            errors.set(alias.name, error.message);
        }
    }
    console.log(JSON.stringify({ values, errors: Object.fromEntries(errors), exports: valueCandidates(), inputs: [...inputs], modulePath, realpaths: Object.fromEntries(realpaths) }));
} else if (!exportName) {
    const exports = isValue ? valueCandidates() : aliases.filter(isComponent).map(symbol => symbol.name).sort();
    console.log(JSON.stringify({ exports, inputs: [...inputs], modulePath, realpaths: Object.fromEntries(realpaths) }));
} else {
    const symbol = aliases.find(symbol => symbol.name === exportName);
    if (isValue) {
        if (!symbol)
            throw new Error(`'${moduleSpecifier}' has no export named '${exportName}'.`);
        const value = readValue(symbol);
        console.log(JSON.stringify({ value, exports: valueCandidates(), inputs: [...inputs], modulePath, realpaths: Object.fromEntries(realpaths) }));
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

function numericEnum(type) {
    const symbol = type.getSymbol();
    const declaration = symbol?.declarations?.length === 1 && symbol.declarations[0];
    if (!declaration || !ts.isEnumDeclaration(declaration) ||
        checker.getDeclaredTypeOfSymbol(symbol) !== type)
        return;
    const enumMembers = declaration.members.map(member => ({
        name: ts.isIdentifier(member.name) || ts.isStringLiteral(member.name) ? member.name.text : undefined,
        value: checker.getConstantValue(member),
    }));
    if (enumMembers.length && enumMembers.every(member => member.name !== undefined && Number.isInteger(member.value) &&
        member.value >= -2147483648 && member.value <= 2147483647))
        return { kind: 'enum', name: symbol.name, typeDefinition: definition(declaration), enumMembers };
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
        const pagedQuery = pagedQueries.get(alias.name);
        if (pagedQuery)
            return { ...identity, definition: definition(definitionNode), signatures: [pagedQuery], constantType: null };
        const submission = readSubmissionProjection(ts, checker, calls, typeOf, standardPromise && promise, domSymbol, definition);
        if (submission)
            return { ...identity, definition: definition(definitionNode), signatures: [submission], constantType: null };
        const eventSignatures = readDomEventProjections(checker, eventProbes, alias.name,
            statementsByGroup.get(eventProbes), probeDiagnostics, readType, readReturn, definition, unsupported);
        if (eventSignatures.length)
            return { ...identity, definition: definition(definitionNode), signatures: eventSignatures, constantType: null };
        const behavior = attempt(() => readBehaviorProjections(ts, checker, behaviorCalls,
            statementsByGroup.get(behaviorCalls), probeDiagnostics, calls, alias.name,
            readType, readReturn, definition, unsupported, standardPromise && promise)).value;
        if (behavior?.length)
            return { ...identity, definition: definition(definitionNode), signatures: behavior, constantType: null };
        const factory = resolvedPluginFactories.get(JSON.stringify([alias.name, null]));
        const readPlugin = candidate => {
            const earlierReturns = [];
            const members = candidate.probe.members.map(member => member.name === candidate.probe.mode
                ? { ...member, fixedBoolean: candidate.probe.fixed } : member);
            const options = [{ type: { kind: 'object', members } }];
            for (const call of calls) {
                if (call.declaration === candidate.signature?.declaration)
                    break;
                if (mayMatch(call, options))
                    earlierReturns.push(call.getReturnType());
            }
            return readPluginFactory(ts, checker, candidate, readVuePlugin, definition,
                type => attempt(() => readType(type, new Set(), 'output', true, unsupported)).value,
                name => name.replace(/^[a-z]/, letter => letter.toUpperCase()), earlierReturns);
        };
        const plugin = factory?.probe.plugin && readPlugin(factory);
        if (plugin) {
            const signatures = [plugin];
            for (const fixed of [false, true]) {
                const specialized = resolvedPluginFactories.get(JSON.stringify([alias.name, fixed]));
                const projected = specialized && readPlugin(specialized);
                if (projected)
                    signatures.push(projected);
            }
            return { ...identity, definition: definition(definitionNode), signatures, constantType: null };
        }
        const specialized = callbackSignatures.get(alias.name)?.flatMap(probe => {
            projectingCallbackOptions = true;
            try {
                return [false, true].flatMap(asynchronous => {
                    const projected = attempt(() => readCallbackOptions(probe, asynchronous, unsupported)).value;
                    return projected ? [{ projected, probe }] : [];
                });
            } finally {
                projectingCallbackOptions = false;
            }
        });
        const validationSignatures = validators.get(alias.name)?.flatMap(candidate => {
            if (calls[0].declaration !== candidate.declaration)
                return [];
            const projected = attempt(() => readGenericValidator(candidate, unsupported)).value;
            return projected ? [projected] : [];
        });
        const bestCallback = specialized?.sort((left, right) =>
            left.projected.typeParameters.length - right.projected.typeParameters.length)[0];
        const schemaForm = schemaForms.get(alias.name);
        const correlatedRefs = standardPromise && readCorrelatedRefCall(checker, calls, typeOf, vueRef, promise, definition);
        const omittedOptions = attempt(() => readOmittedOptions(alias.name, unsupported)).value;
        const defaultCall = resolvedDefaults.get(alias.name);
        const readOrdinary = () => readSignatures(dateSpecializations.signatures(alias.name, calls), unsupported, defaultCall);
        const signatures = bestCallback && calls.every(call => call.typeParameters?.length)
            ? specialized.filter(entry => entry.probe === bestCallback.probe).map(entry => entry.projected) : validationSignatures?.length
            ? [...(attempt(readOrdinary).value ?? []), ...validationSignatures]
            : resolvedElementRefCalls.has(alias.name) ? readElementRefCall(resolvedElementRefCalls.get(alias.name), unsupported)
            : correlatedRefs?.length ? correlatedRefs
            : asyncStates.has(alias.name) ? asyncStates.get(alias.name)
            : schemaForm && !defaultCall ? []
            : omittedOptions?.length ? omittedOptions
            : readOrdinary();
        if (schemaForm)
            signatures.push(schemaForm);
        if (factory && !factory.probe.plugin) {
            const { probe, signature } = factory;
            const result = attempt(() => readReturn(signature, new Set(), 'output', true, unsupported)).value;
            if (result)
                signatures.push({
                    parameters: [{ name: signature.parameters[0].name, optional: false,
                        type: { kind: 'object', members: probe.members } }],
                    returnType: result, definition: definition(signature.declaration),
                });
        }
        return { ...identity, definition: definition(definitionNode), signatures, constantType: null };
    }
    if (symbol.flags & ts.SymbolFlags.ValueModule) {
        const members = checker.getExportsOfModule(symbol).flatMap(alias => {
            const member = valueSymbol(alias);
            if (!member || !(member.flags & ts.SymbolFlags.Function))
                return [];
            const projected = attempt(() => readSignatures(typeOf(member).getCallSignatures(), unsupported, undefined, new Set(), true)).value;
            return projected ? [methodMember(alias.name, projected)] : [];
        });
        if (!members.length)
            unsupported('namespace has no supported methods');
        return { ...identity, definition: definition(definitionNode), signatures: [], constantType: { kind: 'object', members } };
    }
    const plugin = isConstant(definitionNode) && readVuePlugin(type);
    if (plugin)
        return { ...identity, definition: definition(definitionNode), signatures: [], constantType: plugin };
    const record = isConstant(definitionNode) && namedCallableRecord(type);
    if (record)
        return { ...identity, definition: definition(definitionNode), signatures: [], constantType: record };
    const projected = isConstant(definitionNode) && undefinedableConstant(type, unsupported);
    if (projected)
        return { ...identity, definition: definition(definitionNode), signatures: [], constantType: projected };
    const kind = scalar(type, true);
    if (!kind || !isConstant(definitionNode))
        unsupported(`expected a function or scalar constant, found '${display(type)}'`);
    return { ...identity, definition: definition(definitionNode), signatures: [], constantType: { kind } };
}

function checkSignature(signature, unsupported, permittedTypeParameters) {
    if ((signature.typeParameters?.length && !permittedTypeParameters) || checker.hasEffectiveRestParameter(signature) || signature.thisParameter)
        unsupported(`generic, rest or explicit this signature '${checker.signatureToString(signature)}'`);
}

function readOmittedOptions(exportName, unsupported) {
    const result = [];
    const literals = [];
    for (const [index, probe] of omittedOptionCalls.entries()) {
        if (probe.name !== exportName)
            continue;
        const statement = statementsByGroup.get(omittedOptionCalls)[index];
        if (probeDiagnostics.some(diagnostic => diagnostic.start >= statement.pos && diagnostic.start < statement.end))
            continue;
        const call = statement.declarationList.declarations[0].initializer;
        const signature = checker.getResolvedSignature(call);
        if (!signature || !vueRef(signature.getReturnType()))
            continue;
        const name = signature.parameters[0].name;
        const location = definition(signature.declaration);
        const returnType = readReturn(signature, new Set(), 'output', true, unsupported);
        if (probe.domain) {
            literals.push({ name, location, returnType, domain: probe.domain });
            continue;
        }
        const type = readType(checker.getTypeAtLocation(call.arguments[0]), new Set(), 'input', false, unsupported);
        result.push({ parameters: [{ name, type }], returnType, definition: location });
    }
    if (literals.length) {
        const [{ name, location, returnType, domain }] = literals;
        if (literals.length === domain.length && literals.every(entry => shapeKey(entry.returnType) === shapeKey(returnType)))
            result.push({ parameters: [{ name, type: { kind: 'stringEnum', stringLiterals: domain } }], returnType, definition: location });
    }
    return result;
}

function methodMember(name, signatures) {
    return { name, type: { kind: 'method', signatures }, readOnly: true, definition: signatures[0].definition };
}

function readProtocolAlias(type, annotation) {
    if (type.isUnion())
        return;
    const annotated = annotation && ts.isTypeReferenceNode(annotation) &&
        !annotation.typeArguments?.length && checker.getSymbolAtLocation(annotation.typeName);
    const named = annotated && unalias(annotated);
    const direct = named ?? type.aliasSymbol;
    if (direct?.declarations?.length === 1 && direct.declarations.some(node =>
        ts.isTypeAliasDeclaration(node) && !node.typeParameters?.length) &&
        type.getProperties().some(member => !optional(member) &&
            checker.getTypeOfPropertyOfType(type, member.name)?.getCallSignatures().length)) {
        const location = definition(direct.declarations[0]);
        if (location)
            return { kind: 'handle', name: direct.name, typeDefinition: location };
    }
    for (const member of type.getProperties()) {
        if (optional(member) || !checker.getTypeOfPropertyOfType(type, member.name)?.getCallSignatures().length)
            continue;
        for (const node of member.declarations ?? []) {
            let parent = node.parent;
            while (parent && !ts.isSourceFile(parent) && !ts.isTypeAliasDeclaration(parent))
                parent = parent.parent;
            if (!parent || !ts.isTypeAliasDeclaration(parent) || parent.typeParameters?.length)
                continue;
            const symbol = checker.getSymbolAtLocation(parent.name);
            const location = definition(parent);
            // A mapped alias may have several checker instances. An explicit return
            // annotation names the declaration even when the instance differs.
            if (symbol && location && (annotation ? named === symbol :
                checker.isTypeAssignableTo(type, checker.getDeclaredTypeOfSymbol(symbol)) &&
                checker.isTypeAssignableTo(checker.getDeclaredTypeOfSymbol(symbol), type)))
                return { kind: 'handle', name: symbol.name, typeDefinition: location };
        }
    }
}

function namedCallableRecord(type) {
    const symbol = type.getSymbol();
    const declaration = symbol?.declarations?.length === 1 && symbol.declarations[0];
    if (!declaration || !ts.isInterfaceDeclaration(declaration) || declaration.typeParameters?.length ||
        type.getCallSignatures().length || type.getConstructSignatures().length ||
        !type.getProperties().some(member => !optional(member) &&
            member.declarations?.every(ts.isPropertySignature) &&
            checker.getTypeOfPropertyOfType(type, member.name)?.getCallSignatures().length))
        return;
    const location = definition(declaration);
    if (location)
        return { kind: 'handle', name: symbol.name, typeDefinition: location };
}

function callableRecordView(type, position) {
    // A mapped view accepts the original declared receiver unchanged. Require
    // both the mapper's declared source and TS assignability to the view.
    if (!(type.objectFlags & ts.ObjectFlags.Mapped))
        return;
    const source = type.aliasTypeArguments?.[0];
    if (source && checker.isTypeAssignableTo(source, type) &&
        (position === 'input' || checker.isTypeAssignableTo(type, source)))
        return namedCallableRecord(source);
}

function readHandle(type, ancestors, position, unsupported) {
    if (type.isUnion())
        return;
    const symbol = type.getSymbol();
    const declaration = symbol?.declarations?.find(node =>
        ts.isInterfaceDeclaration(node) || ts.isClassDeclaration(node) || ts.isTypeAliasDeclaration(node) && !node.typeParameters?.length);
    if (!declaration || !definition(declaration))
        return;
    const genericParameters = declaration.typeParameters?.map(parameter => parameter.name.text);
    const argumentsList = genericParameters && checker.getTypeArguments(type);
    if (argumentsList && (position !== 'output' || !argumentsList.every(parameter =>
        parameter.flags & ts.TypeFlags.TypeParameter && ancestors.has(parameter))))
        return;
    const members = type.getProperties();
    const methods = members.filter(member => !optional(member) &&
        member.declarations?.every(node => (ts.isMethodSignature(node) || ts.isMethodDeclaration(node)) &&
            (ts.isIdentifier(node.name) || ts.isStringLiteral(node.name)) &&
            !(ts.getCombinedModifierFlags(node) & (ts.ModifierFlags.Private | ts.ModifierFlags.Protected))));
    const callableFields = members.filter(member => !optional(member) && member.declarations?.every(node =>
        ts.isPropertySignature(node) && (ts.isIdentifier(node.name) || ts.isStringLiteral(node.name)) &&
        node.modifiers?.some(modifier => modifier.kind === ts.SyntaxKind.ReadonlyKeyword)) &&
        checker.getTypeOfPropertyOfType(type, member.name)?.getCallSignatures().length);
    if (!methods.length && !callableFields.length)
        return namedCallableRecord(type);
    const identity = { kind: 'handle', name: symbol.name, typeDefinition: definition(declaration) };
    if (argumentsList) {
        identity.typeArguments = argumentsList.map(parameter => ({ kind: 'typeParameter', name: parameter.symbol.name }));
        identity.typeParameters = identity.typeArguments.map(parameter => parameter.name);
    }
    // A handle is supplied by JavaScript. Carry its declaration identity
    // through parameters and recursive results without constructing an object.
    if (position !== 'output' || ancestors.has(type))
        return identity;
    const nested = new Set(ancestors).add(type);
    const projected = [];
    for (const member of [...methods, ...callableFields]) {
        const callable = checker.getTypeOfPropertyOfType(type, member.name);
        const calls = callable?.getCallSignatures() ?? [];
        if (calls.length === 1) {
            const subscription = readSubscription(calls[0], nested, unsupported);
            if (subscription) {
                projected.push(methodMember(member.name, [subscription]));
                continue;
            }
        }
        if (!methods.includes(member))
            continue;
        if (ts.isClassDeclaration(declaration) && calls.length === 1) {
            const [signature] = calls;
            const result = signature.getReturnType();
            // Omitted arguments need no C# representation. The checker
            // confirms the complete invocation uses defaults and has a fixed
            // fulfillment type from TypeScript's standard library.
            if (!signature.minArgumentCount && !signature.thisParameter &&
                !checker.hasEffectiveRestParameter(signature) &&
                (!signature.typeParameters?.length || signature.typeParameters.every(parameter => parameter.getDefault())) &&
                standardPromise && result.target?.getSymbol() === promise &&
                checker.getTypeArguments(result)[0]?.flags & ts.TypeFlags.Void) {
                projected.push(methodMember(member.name, readVoidPromiseMethod(signature, nested, unsupported)));
                continue;
            }
        }
        if (!calls.some(signature => {
            const result = signature.getReturnType();
            return result.flags & ts.TypeFlags.Void || scalar(result) || genericParameters;
        }))
            continue;
        const signatures = attempt(() => readSignatures(calls, unsupported, undefined, nested)).value;
        if (signatures)
            projected.push(methodMember(member.name, signatures));
    }
    for (const member of members) {
        if (optional(member) || !member.declarations?.some(node =>
            ts.isPropertySignature(node) && node.modifiers?.some(modifier => modifier.kind === ts.SyntaxKind.ReadonlyKeyword)))
            continue;
        const result = checker.getTypeOfPropertyOfType(type, member.name);
        if (!result?.getSymbol()?.declarations?.some(ts.isClassDeclaration))
            continue;
        const propertyType = readHandle(result, nested, 'input', unsupported);
        if (propertyType)
            projected.push({ name: member.name, type: propertyType, readOnly: true, definition: definition(member.declarations[0]) });
    }
    return projected.length ? { ...identity, members: projected } : undefined;
}

function readVoidPromiseMethod(signature, ancestors, unsupported) {
    const returnType = { kind: 'promise', elementType: { kind: 'void' } };
    const location = definition(signature.declaration);
    const signatures = [{ parameters: [], returnType, definition: location }];
    const parameters = [];
    for (const parameter of signature.parameters) {
        const argument = omitUndefined(typeOf(parameter));
        const choices = alternatives(argument).filter(branch => branch.flags & ts.TypeFlags.Object &&
            !branch.getCallSignatures().length && !branch.getConstructSignatures().length);
        // An options value goes through unchanged. Do not combine properties
        // from alternative object contracts into a synthetic input.
        if (choices.length !== 1)
            break;
        const projected = attempt(() => readType(choices[0], ancestors, 'input', true, unsupported,
            signature.typeParameters ?? [])).value;
        if (projected?.kind !== 'object')
            break;
        parameters.push({ name: parameter.name, type: projected, optional: false });
        signatures.push({ parameters: [...parameters], returnType, definition: location });
    }
    return signatures;
}

function readMethodOption(type, typeParameters, ancestors, unsupported) {
    for (const branch of alternatives(type)) {
        const generic = typeParameters.includes(branch);
        const candidate = generic ? branch.getDefault() : branch;
        if (!candidate || !checker.isArrayType(candidate) || candidate.target?.getSymbol()?.name !== 'ReadonlyArray')
            continue;
        const [element] = checker.getTypeArguments(candidate);
        if (!(element.flags & ts.TypeFlags.Unknown))
            continue;
        // The host may only read this input. Prove the entire heterogeneous
        // array is assignable, including any constraint on the method parameter.
        const constraint = generic ? checker.getBaseConstraintOfType(branch) : undefined;
        for (const items of [readonlyData, readonlyScalars]) {
            if (checker.isTypeAssignableTo(items, candidate) && (!constraint || checker.isTypeAssignableTo(items, constraint)))
                return readType(items, ancestors, 'input', false, unsupported);
        }
    }
}

function readSubscription(signature, ancestors, unsupported) {
    if (signature.minArgumentCount !== 1 || signature.typeParameters?.length ||
        signature.thisParameter || checker.hasEffectiveRestParameter(signature))
        return;
    const listener = typeOf(signature.parameters[0]);
    const calls = listener.getCallSignatures();
    if (calls.length !== 1 || listener.getProperties().length)
        return;
    const [callback] = calls;
    if (!(callback.getReturnType().flags & (ts.TypeFlags.Void | ts.TypeFlags.Any)) ||
        callback.minArgumentCount !== callback.parameters.length)
        return;
    const result = signature.getReturnType();
    // A named lifecycle handle preserves the identity of the subscription
    // returned by JavaScript. The library calls the listener directly.
    if (!result.getProperties().some(member => !optional(member) &&
        checker.getTypeOfPropertyOfType(result, member.name)?.getCallSignatures().some(call =>
            call.parameters.length === 0 && call.getReturnType().flags & ts.TypeFlags.Void)))
        return;
    return attempt(() => {
        checkSignature(callback, unsupported);
        const handle = readHandle(result, ancestors, 'output', unsupported);
        if (!handle?.members?.length)
            return;
        const parameters = callback.parameters.map(parameter => ({
            name: parameter.name, type: readType(typeOf(parameter), ancestors, 'output', true, unsupported), optional: false,
        }));
        const listenerType = { kind: 'function', signatures: [{ parameters, returnType: { kind: 'void' } }] };
        return { parameters: [{ name: signature.parameters[0].name, type: listenerType, optional: false }],
            returnType: handle, definition: definition(signature.declaration) };
    }).value;
}

function attempt(read) {
    try {
        return { value: read() };
    } catch (error) {
        if (!(error instanceof UnsupportedValueError))
            throw error;
        return { error };
    }
}

function shapeKey(shape) {
    return JSON.stringify(shape, function (key, value) {
        if (key === 'definition' || key === 'typeDefinition' && this.kind !== 'handle' && this.kind !== 'enum' ||
            key === 'members' && this.kind === 'handle' || key === 'name' && this.kind === 'object')
            return undefined;
        return value;
    });
}

function parameterTypeKey(type) {
    // C# overload resolution ignores reference nullability annotations.
    if (type.kind === 'nullable' && !['number', 'boolean'].includes(type.elementType.kind))
        return parameterTypeKey(type.elementType);
    if (type.elementType)
        return { ...type, elementType: parameterTypeKey(type.elementType) };
    return type;
}

function readSignatures(signatures, unsupported, defaultCall, ancestors = new Set(), allowGeneric = false) {
    const composable = vueComposableProjection(ts, checker, typeOf, vueRef, omitUndefined);
    const result = [];
    const seen = new Set();
    const earlier = [];
    const callbackSlot = signatures[0] && forwardedPromiseSlot(signatures[0]);
    const forwardsPromise = typeof callbackSlot === 'number' &&
        signatures.slice(1).every(candidate => forwardedPromiseSlot(candidate) === callbackSlot);
    const forwardsReadonlyRef = signatures.every(composable.forwardsReadonlyRef);
    let failure;
    for (const signature of signatures) {
        const closureInput = signatures.length === 1 && inputClosureSlot(ts, checker, typeOf, signature);
        const slot = signature.typeParameters?.[0];
        const forwardsValue = signatures.length === 1 && signature.typeParameters?.length === 1 &&
            signature.minArgumentCount === 1 && signature.parameters.length === 1 && !checker.getBaseConstraintOfType(slot) &&
            typeOf(signature.parameters[0]) === slot && signature.getReturnType() === slot;
        const predicate = checker.getTypePredicateOfSignature(signature);
        const genericPredicate = signatures.length === 1 && signature.typeParameters?.length === 1 &&
            signature.parameters.length === 1 && !checker.getBaseConstraintOfType(slot) &&
            predicate?.kind === ts.TypePredicateKind.Identifier && predicate.parameterIndex === 0 &&
            predicate.type === slot && omitUndefined(typeOf(signature.parameters[0])) === slot;
        // An unconstrained result-only slot can remain erased if every exposed
        // member has a concrete type independent of that slot. Required inputs
        // still pass the normal projection checks and cannot accept an open T.
        const erasedResult = signatures.length === 1 && signature.typeParameters?.length &&
            signature.typeParameters.every(parameter => !checker.getBaseConstraintOfType(parameter) &&
                (!parameter.getDefault() || parameter.getDefault().flags & ts.TypeFlags.Unknown));
        if (signature.typeParameters?.length && defaultCall?.declaration === signature.declaration) {
            const resultType = attempt(() => readReturn(defaultCall, new Set(), 'output', true, unsupported));
            if (resultType.value && !seen.has('[]')) {
                result.push({ parameters: [], returnType: resultType.value, definition: definition(signature.declaration) });
                seen.add('[]');
            }
        }
        const partialRecord = signature.parameters.length === 0 && signature.typeParameters?.length === 1 &&
            partialRecordSlot(vueRef(signature.getReturnType())?.element ?? signature.getReturnType()) === slot;
        const contextualComputed = signatures.length === 1 && composable.contextualComputed(signature);
        const forwardsParameters = forwardsPromise || forwardsReadonlyRef || closureInput || forwardsValue || genericPredicate || partialRecord || contextualComputed;
        const typeParameters = forwardsParameters ? signature.typeParameters : allowGeneric && signature.typeParameters?.every(parameter =>
            checker.getBaseConstraintOfType(parameter)?.flags & ts.TypeFlags.NonPrimitive)
            ? signature.typeParameters : undefined;
        const context = typeParameters ? new Set([...ancestors, ...typeParameters]) : ancestors;
        const projection = attempt(() => {
            checkSignature(signature, unsupported, typeParameters || erasedResult);
            let result = readReturn(signature, context, 'output', true, unsupported);
            if (contextualComputed) {
                // The caller chooses the value type. The selector alone supplies
                // no evidence that a matching value is present in Vue context.
                result = { ...result, elementType: { kind: 'undefinedable', elementType: { kind: 'typeParameter', name: slot.symbol.name } } };
            }
            if (closureInput?.record) {
                const [call] = result.signatures;
                const [first, ...rest] = call.parameters;
                result = { ...result, signatures: [{ ...call, parameters: [
                    { ...first, type: { ...first.type, kind: 'recordTypeParameter' } }, ...rest,
                ] }] };
            }
            if (typeParameters && !forwardsParameters && !result.typeArguments?.length)
                unsupported('generic namespace function must return a typed handle');
            return result;
        });
        if (projection.value) {
            let prefixes = [[]];
            let varied = false;
            const optionalArities = [];
            // A cleanup factory can participate in a method-group conversion.
            // C# delegate binding requires the actual supplied arity.
            let useExplicitArities = signatures.length > 1 || isVoidThunk(signature.getReturnType()) || genericPredicate;
            const definitionNode = definition(signature.declaration);
            const returnKey = shapeKey(projection.value);
            const add = parameters => {
                // With several TS overloads, exposing an omitted argument as a C# default
                // can accidentally select a different result. Check each injected arity.
                if (earlier.some(previous => previous.returnKey !== returnKey &&
                    mayMatch(previous.signature, parameters)))
                    return;
                const key = shapeKey(parameters.map(parameter => parameterTypeKey(parameter.type)));
                if (seen.has(key))
                    return;
                seen.add(key);
                const projected = { parameters, returnType: projection.value, definition: definitionNode,
                    typeParameters: genericPredicate && !parameters.length ? undefined : typeParameters?.map(parameter => parameter.symbol.name) };
                result.push(projected);
                if (contextualComputed && parameters.length) {
                    const [selector] = parameters;
                    // C# distinguishes `in` selectors from by-value selectors, allowing both
                    // generic constraints. The string selector also covers the omitted JS argument.
                    result.push({ ...projected, valueTypeParameters: projected.typeParameters,
                        parameters: [{ ...selector, readOnlyReference: true, optional: selector.type.kind === 'string' }] });
                }
            };
            for (let index = 0; index <= signature.parameters.length; index++) {
                const isOptional = index >= signature.minArgumentCount;
                if (index === signature.parameters.length)
                    break;
                if (isOptional)
                    optionalArities.push(...prefixes);
                const parameter = signature.parameters[index];
                const type = isOptional ? omitUndefined(typeOf(parameter)) : typeOf(parameter);
                const predicateInputs = dateTypes.predicateInputs(signature, index, type, signatures.length === 1);
                const getterInput = composable.unknownGetter(type, projection.value);
                const projected = predicateInputs ? { value: predicateInputs } : getterInput ? { value: [getterInput] } :
                    attempt(() => readInputBranches(type, context, true, unsupported, forwardsReadonlyRef));
                if (!projected.value) {
                    failure ??= projected.error;
                    if (!isOptional)
                        prefixes = [];
                    break;
                }
                // Keep one varying union per signature. Further unions can use
                // a representative accepted value without a Cartesian product.
                const choices = varied ? [preferredInput(projected.value)] : projected.value;
                if (isOptional && choices.length > 1)
                    useExplicitArities = true;
                varied ||= choices.length > 1;
                prefixes = prefixes.flatMap(prefix => choices.map(type => [
                    ...prefix, { name: parameter.name, type, optional: isOptional },
                ]));
            }
            if (useExplicitArities)
                [...optionalArities, ...prefixes].forEach(parameters =>
                    add(parameters.map(parameter => ({ ...parameter, optional: false }))));
            else
                prefixes.forEach(add);
        } else {
            failure ??= projection.error;
        }
        earlier.push({ signature, returnKey: projection.value && shapeKey(projection.value) });
    }
    if (!result.length) {
        if (failure)
            throw failure;
        unsupported('no unambiguous supported signature');
    }
    return result;
}

// A higher-order async function can carry the caller's payload directly.
// Compare checker symbols so a package-local thenable or another generic slot
// cannot acquire the standard Promise<T> contract through its spelling.
function forwardedPromiseSlot(signature) {
    if (!standardPromise || signature.typeParameters?.length !== 1)
        return;
    const [parameter] = signature.typeParameters;
    const constraint = checker.getBaseConstraintOfType(parameter);
    if (constraint && !(constraint.flags & ts.TypeFlags.Unknown))
        return;
    const carries = type => type.target?.getSymbol() === promise && checker.getTypeArguments(type)[0] === parameter;
    if (!carries(signature.getReturnType()))
        return;
    for (const [index, argument] of signature.parameters.slice(0, signature.minArgumentCount).entries()) {
        const callback = typeOf(argument);
        const calls = callback.getCallSignatures();
        if (!callback.getProperties().length && calls.length === 1 && !calls[0].typeParameters?.length &&
            carries(calls[0].getReturnType()))
            return index;
    }
}

function mayMatch(signature, parameters) {
    return parameters.length >= signature.minArgumentCount &&
        (parameters.length <= signature.parameters.length || checker.hasEffectiveRestParameter(signature)) &&
        parameters.every(({ type }, index) => {
            const parameter = signature.parameters[index];
            return !parameter || checker.hasEffectiveRestParameter(signature) && index >= signature.parameters.length - 1 ||
                mayAccept(typeOf(parameter), type);
        });
}

function mayAccept(original, projected) {
    if (original.isUnion())
        return original.types.some(branch => mayAccept(branch, projected));
    if (projected.kind === 'stringEnum' && original.flags & ts.TypeFlags.StringLike)
        return !(original.flags & ts.TypeFlags.StringLiteral) || projected.stringLiterals.includes(original.value);
    if (projected.kind === 'enum' && original.flags & ts.TypeFlags.NumberLike)
        return !(original.flags & ts.TypeFlags.NumberLiteral) || projected.enumMembers.some(member => member.value === original.value);
    if (projected.kind === 'nullable')
        return Boolean(original.flags & (ts.TypeFlags.Null | ts.TypeFlags.Any | ts.TypeFlags.Unknown | ts.TypeFlags.TypeParameter)) ||
            mayAccept(original, projected.elementType);
    const primitive = scalar(original, true);
    if (primitive)
        return primitive === projected.kind;
    if (original.flags & (ts.TypeFlags.Null | ts.TypeFlags.Undefined | ts.TypeFlags.Void | ts.TypeFlags.Never))
        return false;
    if (['string', 'number', 'boolean'].includes(projected.kind) &&
        (original.getCallSignatures().length || vueRef(original)))
        return false;
    if (projected.kind === 'object' && original.flags & ts.TypeFlags.Object)
        // Input records only emit their declared JS keys. A missing required key
        // is enough to prove that this record cannot select the earlier overload.
        return original.getProperties().every(member => optional(member) || member.name.startsWith('__@') ||
            projected.members.some(candidate => {
                if (candidate.name !== member.name)
                    return false;
                const memberType = checker.getTypeOfPropertyOfType(original, member.name);
                if (candidate.fixedBoolean !== undefined)
                    return checker.isTypeAssignableTo(candidate.fixedBoolean ? checker.getTrueType() : checker.getFalseType(), memberType);
                return mayAccept(memberType, candidate.type);
            }));
    return true;
}

function readGenericValidator({ signature, declaration }, unsupported) {
    const [generic] = signature.typeParameters;
    validatorTypeParameter = generic;
    try {
        const parameters = signature.parameters.map(parameter => ({
            name: parameter.name, type: readType(typeOf(parameter), new Set(), 'input', true, unsupported), optional: false,
        }));
        const returnType = readReturn(signature, new Set(), 'output', true, unsupported);
        return { parameters, returnType, typeParameters: [generic.symbol.name], definition: definition(declaration) };
    } finally {
        validatorTypeParameter = undefined;
    }
}

function readReturn(signature, ancestors, position, partial, unsupported) {
    const result = signature.getReturnType();
    const protocol = position === 'output' && readProtocolAlias(result, signature.declaration?.type);
    if (protocol) {
        const projection = attempt(() => readType(result, ancestors, position, true, unsupported)).value;
        return projection?.members?.length ? { ...protocol, members: projection.members } : protocol;
    }
    return result.flags & ts.TypeFlags.Void ? { kind: 'void' } : readType(result, ancestors, position, partial, unsupported);
}

function readCallbackOptions(probe, asynchronous, unsupported) {
    const { signature, member, names } = probe;
    const options = alternatives(typeOf(signature.parameters[0])).find(branch => branch.getProperty(member));
    const callback = alternatives(checker.getTypeOfPropertyOfType(options, member))
        .flatMap(singleCall).find(call => !call.typeParameters?.length);
    if (!callback)
        unsupported('expected a typed options callback');
    const result = callback.getReturnType();
    // Promise<T> must resolve to the same T as the synchronous callback.
    // Separate records let C# infer the result from a correctly typed callback.
    const synchronous = alternatives(result).find(branch => callbackMarkers.has(branch.getSymbol()));
    const promised = alternatives(result).find(branch => standardPromise && branch.target?.getSymbol() === promise);
    if (asynchronous && (!synchronous || !promised || checker.getTypeArguments(promised)[0] !== synchronous))
        unsupported('expected promise and synchronous arms with the same callback result');
    const payload = asynchronous ? promised : synchronous ?? result;
    const parameters = [];
    for (const parameter of callback.parameters) {
        const type = typeOf(parameter);
        if (!callbackMarkers.has(type.getSymbol()))
            break;
        parameters.push({ name: parameter.name, type: readType(type, new Set(), 'output', false, unsupported), optional: false });
    }
    const members = [];
    for (const property of options.getProperties()) {
        if (property.name === member) {
            members.push({ name: member, optional: false, readOnly: false, definition: definition(property.declarations?.[0]),
                type: { kind: 'function', signatures: [{ parameters, returnType: readType(payload, new Set(), 'input', false, unsupported) }] } });
            continue;
        }
        const original = omitUndefined(checker.getTypeOfPropertyOfType(options, property.name));
        const type = attempt(() => readType(original, new Set(), 'input', true, unsupported)).value ??
            alternatives(original).filter(branch => checker.isArrayType(branch) || scalar(branch))
                .map(branch => attempt(() => readType(branch, new Set(), 'input', false, unsupported)).value)
                .find(Boolean);
        if (type)
            members.push({ name: property.name, type, optional: optional(property), readOnly: false,
                definition: definition(property.declarations?.[0]) });
        else if (!optional(property))
            unsupported(`required callback option '${property.name}'`);
    }
    const returnType = readReturn(signature, new Set(), 'output', true, unsupported);
    const resultName = probe.result.replace('JoltCallback_', '');
    if (!returnType.members?.some(member => member.type.kind === 'ref' &&
        member.type.elementType?.kind === 'undefinedable' && member.type.elementType.elementType?.name === resultName))
        unsupported('expected a result ref that carries the callback return type');
    const parametersToProject = [{ name: asynchronous ? 'asyncOptions' : signature.parameters[0].name,
        optional: false, type: { kind: 'object', members } }];
    if (probe.earlierSignatures.some(candidate => mayMatch(candidate, parametersToProject) &&
        !checker.isTypeAssignableTo(candidate.getReturnType(), signature.getReturnType())))
        unsupported('an earlier callback overload can return an incompatible type');
    return { parameters: parametersToProject,
        typeParameters: names.map(name => name.replace('JoltCallback_', '')),
        scalarResultTypeParameter: resultName,
        referenceTypeParameters: names.filter(name => returnType.members.some(member =>
            member.type.elementType?.kind === 'undefinedable' &&
            member.type.elementType.elementType?.name === name.replace('JoltCallback_', '')))
            .map(name => name.replace('JoltCallback_', '')),
        returnType,
        definition: definition(signature.declaration) };
}

function readSignature(signature, ancestors, position, unsupported, partial = false) {
    checkSignature(signature, unsupported);
    const parameters = [];
    for (const [index, parameter] of signature.parameters.entries()) {
        const isOptional = index >= signature.minArgumentCount;
        const type = typeOf(parameter);
        // The JavaScript package can omit an optional callback argument.
        if (position === 'input' && isOptional)
            unsupported(`optional callback parameter '${parameter.name}'`);
        const input = isOptional ? omitUndefined(type) : type;
        const projected = attempt(() => ({
            name: parameter.name,
            type: position === 'output'
                ? readCallableInput(input, ancestors, unsupported, partial)
                : readType(input, ancestors, 'output', false, unsupported),
            optional: isOptional,
        }));
        if (!projected.value) {
            if (position === 'output' && isOptional && (validatorTypeParameter || partial))
                break;
            throw projected.error;
        }
        parameters.push(projected.value);
    }
    return {
        parameters,
        returnType: readReturn(signature, ancestors, position, position === 'output' && (partial || Boolean(validatorTypeParameter)), unsupported),
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
    return rest.length ? checker.getUnionType(rest) : type;
}

function readCallableInput(type, ancestors, unsupported, partial = false) {
    const whole = attempt(() => readType(type, ancestors, 'input', partial, unsupported));
    if (whole.value)
        return whole.value;
    // The C# caller supplies the argument, so it may use a supported subset.
    // Removing the undefined arm does not change the upstream parameter arity.
    const present = omitUndefined(type);
    const union = readScalarUnion(present, true);
    if (union)
        return union;
    if (present.isUnion()) {
        const boolean = checker.isTypeAssignableTo(checker.getBooleanType(), present);
        const branches = present.types.filter(branch => !boolean || !(branch.flags & ts.TypeFlags.BooleanLiteral))
            .map(branch => attempt(() => readType(branch, ancestors, 'input', false, unsupported)).value);
        if (boolean)
            branches.push({ kind: 'boolean' });
        // A returned delegate has one Invoke signature. A plain object and a
        // scalar are disjoint JS values and can share one erased C# union.
        if (branches.length === 2 && branches.some(branch => branch?.kind === 'dictionary') &&
            branches.some(branch => ['boolean', 'number', 'string'].includes(branch?.kind)))
            return { kind: 'union', variants: branches };
    }
    throw whole.error;
}

function readScalarCalls(signatures) {
    // A string key fixes K independently of the remaining argument domains.
    const projected = new Map();
    for (const signature of signatures) {
        if ((signature.typeParameters?.length ?? 0) > 1 || !signature.parameters.length ||
            signature.minArgumentCount !== signature.parameters.length ||
            signature.thisParameter || checker.hasEffectiveRestParameter(signature))
            continue;
        const generic = signature.typeParameters?.[0];
        const constraint = generic && checker.getBaseConstraintOfType(generic);
        const types = signature.parameters.map(typeOf);
        const first = types[0];
        const genericKey = generic && (first === generic || first.isUnion() && first.types.includes(generic));
        const inputs = types.map((type, index) => {
            const kind = index === 0 && generic
                ? genericKey && constraint && scalar(constraint) : scalar(type) ?? (isDate(type) ? 'date' : undefined);
            if (kind)
                return { kind };
            const union = !generic && readScalarUnion(type);
            if (union)
                return union;
            const indexes = checker.getIndexInfosOfType(type);
            if (index === 0 || indexes.length !== 1 || indexes[0].keyType !== checker.getStringType())
                return;
            const dictionary = checker.resolveName('JoltStringDictionary', source, ts.SymbolFlags.Type, false);
            // Preserve the upstream overload by checking the whole carrier domain.
            if (checker.isTypeAssignableTo(checker.getDeclaredTypeOfSymbol(dictionary), type))
                return { kind: 'dictionary', elementType: { kind: 'string' } };
        });
        const output = scalar(signature.getReturnType());
        if (!output || inputs.some(input => !input))
            continue;
        const parameters = signature.parameters.map((parameter, index) => ({
            name: parameter.name, type: inputs[index], optional: false,
        }));
        // A literal or generic overload may be chosen for part of a C# scalar
        // domain. Every overlapping signature must agree on the result.
        if (signatures.some(other => other !== signature && mayMatch(other, parameters) &&
            (other.thisParameter || scalar(other.getReturnType(), true) !== output)))
            continue;
        const key = shapeKey(inputs);
        if (!projected.has(key))
            projected.set(key, { parameters, returnType: { kind: output }, definition: definition(signature.declaration) });
    }
    return [...projected.values()];
}

function readInputBranches(type, ancestors, partial, unsupported, splitReactive = false) {
    // For an exact value/ref/getter union, each emitted overload passes the
    // original argument through. Keep the narrower member projection inside
    // option records where C# cannot spell a lambda-to-union conversion.
    const whole = attempt(() => readType(type, ancestors, 'input', partial, unsupported));
    if (whole.value && (!splitReactive || !type.isUnion()) && !vueInputValue(type))
        return [whole.value];
    if (!type.isUnion())
        throw whole.error;
    const branches = type.types
        .map(branch => attempt(() => readType(branch, ancestors, 'input', partial, unsupported)).value)
        .filter(Boolean);
    if (!whole.value && branches.length && type.types.every(branch =>
        (branch.flags & ts.TypeFlags.Object || branch.isIntersection()) &&
        !branch.getCallSignatures().length && !vueRef(branch)))
        // C# can construct one options branch without combining mutually exclusive keys.
        return [branches.find(branch => branch.members?.length) ?? branches[0]];
    if (!branches.length)
        throw whole.error;
    return [...new Map(branches.map(branch => [shapeKey(branch), branch])).values()];
}

function preferredInput(branches) {
    return branches.find(branch => branch.kind === 'date') ?? branches[0];
}

function vueRef(type) {
    if (type.isUnion()) {
        const branches = type.types.map(vueRef);
        const first = branches[0];
        if (first && branches.every(branch => branch && branch.element === first.element))
            return {
                element: first.element,
                readOnly: branches.some(branch => branch.readOnly),
                write: branches.every(branch => branch.write === first.write) ? first.write : undefined,
                plain: branches.every(branch => branch.plain),
                name: branches.every(branch => branch.name === first.name) ? first.name : undefined,
            };
        if (projectingCallbackOptions && branches.every(Boolean)) {
            const elements = branches.map(branch => branch.element);
            if (elements.some(element => callbackMarkers.has(element.getSymbol())) &&
                elements.some(element => element.flags & ts.TypeFlags.Undefined))
                return { element: checker.getUnionType(elements), readOnly: true, plain: false };
        }
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
    const element = checker.getTypeOfPropertyOfType(type, 'value');
    const readOnly = Boolean(ts.getCheckFlags(value) & ts.CheckFlags.Readonly) ||
        !value.declarations?.some(ts.isSetAccessorDeclaration);
    // Only the canonical Ref<T, S> declaration gives these arguments meaning.
    // A downstream interface or mapped type may have unrelated type parameters.
    const declaration = brand.declarations.find(node => ts.isInterfaceDeclaration(node.parent))?.parent;
    const refSymbol = declaration && checker.getSymbolAtLocation(declaration.name);
    const vueModule = declaration && checker.getSymbolAtLocation(declaration.getSourceFile());
    const vueExports = vueModule ? checker.getExportsOfModule(vueModule) : [];
    const computed = vueExports.find(symbol => symbol.name === 'ComputedRef');
    const writableComputed = vueExports.find(symbol => symbol.name === 'WritableComputedRef');
    const shallow = vueExports.find(symbol => symbol.name === 'ShallowRef');
    const isShallow = shallow && type.aliasSymbol === shallow;
    const reference = refSymbol && (type.target?.getSymbol() === refSymbol ? type :
        isShallow && type.isIntersection() ? type.types.find(branch => branch.target?.getSymbol() === refSymbol) : undefined);
    // WritableComputedRef<T, S> keeps Ref's asymmetric setter through
    // BaseComputedRef. Only Vue's own declaration has that parameter contract.
    const isWritableComputed = writableComputed && type.target?.getSymbol() === writableComputed;
    const [read, write = read] = reference || isWritableComputed
        ? checker.getTypeArguments(reference ?? type) : [];
    const name = computed && type.target?.getSymbol() === computed ? 'VueComputedRef' :
        isWritableComputed ? 'VueWritableComputedRef' : isShallow ? 'VueShallowRef' : undefined;
    const plain = type.getProperties().every(member => member === value || member === brand || optional(member));
    return { element, readOnly, write: read === element ? write : undefined, plain, name };
}

function elementRefParameters(signature) {
    if (!signature.minArgumentCount || signature.thisParameter || checker.hasEffectiveRestParameter(signature))
        return;
    const names = signature.parameters.slice(0, signature.minArgumentCount).map(parameter => {
        const type = typeOf(parameter);
        const ref = !type.isUnion() && vueRef(type);
        if (!ref?.readOnly || !ref.plain)
            return;
        const arms = alternatives(ref.element);
        if (!arms.some(arm => arm.flags & ts.TypeFlags.Null))
            return;
        for (const arm of arms) {
            const candidate = signature.typeParameters?.includes(arm) ? arm.getDefault() : arm;
            for (const name of ['Element', 'HTMLElement', 'SVGElement']) {
                const symbol = domSymbol(name);
                if (symbol && alternatives(candidate).some(type => type.getSymbol() === symbol))
                    return name;
            }
        }
    });
    // Keep the existing non-generic path for already representable refs.
    return names.every(Boolean) && (signature.typeParameters?.length ||
        signature.parameters.slice(0, signature.minArgumentCount).some(parameter =>
            alternatives(vueRef(typeOf(parameter)).element).length > 2)) ? names : undefined;
}

function readElementRefCall({ signature, names }, unsupported) {
    const parameters = names.map((name, index) => ({
        name: signature.parameters[index].name, optional: false,
        type: { kind: 'ref', readOnly: true, elementType: { kind: 'nullable', elementType: { kind: 'web', name } } },
    }));
    const returnType = readReturn(signature, new Set(), 'output', true, unsupported);
    const location = definition(signature.declaration);
    const signatures = [{ parameters, returnType, definition: location }];
    const options = signature.parameters[names.length];
    if (!options)
        return signatures;
    const type = omitUndefined(typeOf(options));
    if (!(type.flags & ts.TypeFlags.Object) || checker.getIndexInfosOfType(type).length ||
        type.getCallSignatures().length || type.getConstructSignatures().length)
        return signatures;
    const members = [];
    for (const member of type.getProperties()) {
        if (!optional(member))
            return signatures;
        const input = omitUndefined(checker.getTypeOfPropertyOfType(type, member.name));
        const array = readProtocolArray(checker, ts, readProtocolAlias, input);
        if (array) {
            members.push({ name: member.name, optional: true, readOnly: false, type: array,
                definition: definition(member.declarations?.[0]) });
            continue;
        }
        const [call] = singleCall(input);
        if (!call || input.getProperties().length || call.typeParameters?.length || call.thisParameter ||
            checker.hasEffectiveRestParameter(call) || call.minArgumentCount !== names.length + 1 ||
            call.parameters.length !== names.length + 1)
            continue;
        const args = call.parameters.map(typeOf);
        if (!names.every((name, index) => args[index].getSymbol() === domSymbol(name)) ||
            !isVoidThunk(args.at(-1)) || !isVoidThunk(call.getReturnType()))
            continue;
        const callback = readSignature(call, new Set(), 'input', unsupported);
        members.push({ name: member.name, optional: true, readOnly: false,
            type: { kind: 'function', signatures: [callback] }, definition: definition(member.declarations?.[0]) });
    }
    if (members.length)
        signatures.push({ parameters: [...parameters, { name: options.name, optional: false, type: { kind: 'object', members } }],
            returnType, definition: location });
    return signatures;
}

function isVoidThunk(type) {
    const [call] = singleCall(type);
    return call && !type.getProperties().length && !call.parameters.length && !call.thisParameter &&
        !call.typeParameters?.length && Boolean(call.getReturnType().flags & ts.TypeFlags.Void);
}

function vueInputValue(type) {
    if (!type.isUnion())
        return;
    // Boolean literals become separate union arms when a mapped options
    // type adds a ref alternative. The same domain still accepts a plain bool.
    const boolean = checker.getBooleanType();
    if (type.types.some(branch => vueRef(branch)) &&
        checker.isTypeAssignableTo(boolean, type))
        return boolean;
    const element = type.types.find(branch => scalar(branch) || webTypes(branch));
    if (element && type.types.every(branch => {
        const calls = branch.getCallSignatures();
        return branch === element || vueRef(branch)?.element === element ||
            calls.length === 1 && !calls[0].parameters.length && calls[0].getReturnType() === element;
    }))
        return element;
    // The direct DOM arm is accepted without unwrapping a Vue ref or
    // invoking a getter. Require that the other object arms are Vue refs
    // or zero-argument getters that return the same wider DOM domain.
    if (element && webTypes(element) && type.types.some(branch => {
        const ref = vueRef(branch);
        return ref?.readOnly && checker.isTypeAssignableTo(element, ref.element);
    }))
        return element;
}

function readScalarUnion(type, includeArrays = false) {
    if (!type.isUnion())
        return;
    const variants = new Map();
    let hasArray = false;
    for (const branch of type.types) {
        if (branch.flags & ts.TypeFlags.BooleanLiteral)
            continue;
        const kind = scalar(branch) ?? (isDate(branch) ? 'date' : undefined);
        let variant = kind && { kind };
        if (!variant && includeArrays && checker.isArrayType(branch) &&
            branch.target?.getSymbol()?.name !== 'ReadonlyArray') {
            const [element] = checker.getTypeArguments(branch);
            const elementKind = scalar(element);
            // Two array branches can be assignable through CLR array
            // covariance. One array branch is disjoint from the scalar arms.
            if (elementKind && !hasArray) {
                variant = { kind: 'array', readOnly: false, elementType: { kind: elementKind } };
                hasArray = true;
            }
        }
        if (!variant)
            return;
        variants.set(variant.kind, variant);
    }
    if (type.types.some(branch => branch.flags & ts.TypeFlags.BooleanLiteral)) {
        // TypeScript flattens boolean into its two literals in a wider union.
        if (!checker.isTypeAssignableTo(checker.getBooleanType(), type))
            return;
        variants.set('boolean', { kind: 'boolean' });
    }
    if (variants.size >= 2)
        return { kind: 'union', variants: [...variants.keys()].sort().map(kind => variants.get(kind)) };
}

function partialRecordSlot(type) {
    const alias = type.aliasSymbol;
    const [slot] = type.aliasTypeArguments ?? [];
    if (alias?.name !== 'Partial' || !alias.declarations?.every(node => program.isSourceFileDefaultLibrary(node.getSourceFile())) ||
        !(slot?.flags & ts.TypeFlags.TypeParameter))
        return;
    const constraint = checker.getBaseConstraintOfType(slot);
    const indexes = constraint && checker.getIndexInfosOfType(constraint);
    if (indexes?.length === 1 && !constraint.getProperties().length &&
        indexes[0].keyType === checker.getStringType() && indexes[0].type.flags & (ts.TypeFlags.Any | ts.TypeFlags.Unknown))
        return slot;
}

function readType(type, ancestors, position, partial, unsupported, methodParameters) {
    const partialSlot = position === 'output' && partialRecordSlot(type);
    if (partialSlot && ancestors.has(partialSlot))
        return { kind: 'partialRecord', elementType: { kind: 'recordTypeParameter', name: partialSlot.symbol.name } };
    if (position !== 'invariant') {
        const protocol = readCallableProtocol(checker, ts, optional, definition, typeOf, type);
        if (protocol)
            return protocol;
    }
    if (type === validatorTypeParameter)
        return { kind: 'typeParameter', name: type.symbol.name };
    if (type.flags & ts.TypeFlags.TypeParameter && ancestors.has(type))
        return { kind: 'typeParameter', name: type.symbol.name };
    const marker = callbackMarkers.get(type.getSymbol());
    if (marker)
        return { kind: 'typeParameter', name: marker };
    const enumeration = numericEnum(type);
    if (enumeration)
        return enumeration;
    const kind = scalar(type, position === 'output');
    if (kind)
        return { kind };
    if (isDate(type))
        return { kind: 'date' };
    const webType = webTypes(type);
    if (webType)
        return webType;
    const callableView = callableRecordView(type, position);
    if (callableView)
        return callableView;
    const isPromise = standardPromise && type.target?.getSymbol() === promise;
    if (!isPromise) {
        const handle = readHandle(type, ancestors, position, unsupported);
        if (handle)
            return handle;
    }
    const nested = new Set(ancestors).add(type);
    if (ancestors.has(type))
        unsupported(`recursive type '${display(type)}'`);
    if (isPromise) {
        const [element] = checker.getTypeArguments(type);
        // Fulfillment is observed by the receiver. Keep the promise's
        // direction for its payload, even inside a callback parameter.
        const elementType = element.flags & ts.TypeFlags.Void ? { kind: 'void' } :
            readType(element, nested, position, partial && position === 'output', unsupported);
        return { kind: 'promise', elementType };
    }
    if (type.isIntersection()) {
        // TypeScript preserves T & {} in some non-null input aliases.
        const branches = type.types.filter(branch => branch.getProperties().length ||
            !branch.getSymbol()?.declarations?.every(node => ts.isTypeLiteralNode(node) && !node.members.length));
        if (branches.length === 1)
            return readType(branches[0], nested, position, partial, unsupported);
    }
    if (type.isUnion()) {
        if (validatorTypeParameter && position === 'input') {
            const union = readScalarUnion(type);
            if (union)
                return union;
        }
        if (position === 'output' && type.types.some(branch => branch.flags & ts.TypeFlags.Undefined)) {
            const element = omitUndefined(type);
            const name = callbackMarkers.get(element.getSymbol()) ??
                (ancestors.has(element) && element.flags & ts.TypeFlags.TypeParameter ? element.symbol.name : undefined);
            if (name)
                return { kind: 'undefinedable', elementType: { kind: 'typeParameter', name } };
            if (scalar(element) || webTypes(element) || isDate(element))
                return { kind: 'undefinedable', elementType: readType(element, nested, position, partial, unsupported) };
            const nonNull = element.isUnion() && element.types.filter(arm => !(arm.flags & ts.TypeFlags.Null));
            if (nonNull?.length === 1 && (scalar(nonNull[0]) || webTypes(nonNull[0]) || isDate(nonNull[0])))
                return { kind: 'undefinedable', elementType: readType(nonNull[0], nested, position, partial, unsupported) };
        }
        const members = type.types.filter(branch => !(branch.flags & ts.TypeFlags.Null));
        if (members.length < type.types.length) {
            const element = members.length === 1 ? members[0] :
                members.length === 2 && members.every(branch => branch.flags & ts.TypeFlags.BooleanLiteral) ? checker.getBooleanType() : undefined;
            if (element)
                return { kind: 'nullable', elementType: readType(element, nested, position, partial, unsupported) };
        }
        if (position === 'output') {
            const family = scalar(type.types[0], true);
            if (family && type.types.every(branch => scalar(branch, true) === family))
                return { kind: family };
        }
        if (position === 'input' && partial) {
            const date = ancestors.size && type.types.find(isDate);
            // A constructible input record may select the Date arm of a
            // Date-or-scalar property. Both reads and writes must agree for refs.
            if (date && type.types.every(branch => branch === date ||
                scalar(branch) || branch.isIntersection() && branch.types.some(type => scalar(type))))
                return { kind: 'date' };
            const immediateHost = type.types.find(branch => webTypes(branch));
            if (immediateHost && type.types.every(branch => branch === immediateHost ||
                standardPromise && branch.target?.getSymbol() === promise &&
                checker.getTypeArguments(branch)[0] === immediateHost))
                return webTypes(immediateHost);
            // MaybeRefOrGetter<T> accepts T unchanged. A scalar/DOM value
            // can be passed directly without manufacturing a ref or closure.
            const element = vueInputValue(type);
            if (element)
                return readType(element, nested, position, partial, unsupported);
        }
    }
    const ref = vueRef(type);
    if (ref) {
        const exactWrite = ref.write === ref.element;
        if (position !== 'output' && (!ref.plain || !ref.readOnly && !exactWrite))
            unsupported(`Vue ref '${display(type)}'`);
        // A returned ref may accept more on write than it returns on read.
        // C# still only offers the read domain for writes on that ref.
        const readOnly = ref.readOnly || !exactWrite &&
            (!ref.write || !checker.isTypeAssignableTo(ref.element, ref.write));
        const element = attempt(() => readType(ref.element, nested, readOnly ? position : 'invariant', false, unsupported));
        if (!element.value) {
            if (position === 'output')
                return { kind: 'ref', readOnly: true, elementType: readType(ref.element, nested, position, partial, unsupported) };
            throw element.error;
        }
        return {
            kind: 'ref', readOnly,
            name: position === 'output' && (readOnly ? ref.name === 'VueComputedRef' :
                ref.name === 'VueShallowRef' || ref.name === 'VueWritableComputedRef')
                ? ref.name : undefined,
            elementType: element.value,
        };
    }
    if (checker.isArrayType(type)) {
        const readOnly = type.target?.getSymbol()?.name === 'ReadonlyArray';
        const [element] = checker.getTypeArguments(type);
        // A readonly Date-or-scalar input can read an array of Dates. Mutable
        // arrays stay invariant because the JavaScript function can write them.
        const date = readOnly && position === 'input' && element.isUnion() &&
            element.types.find(isDate);
        const input = date && element.types.every(branch => branch === date || scalar(branch)) ? date : element;
        // Preserve every writable branch. A Date[] would admit reads of the
        // wrong type after JavaScript writes a string or a number into it.
        // Vue may wrap each element in a ref. Admit plain C# records only if
        // the declared readonly array accepts arbitrary JS objects in that slot.
        let union;
        if (!readOnly) {
            union = readScalarUnion(element);
        } else if (position === 'input' && checker.isTypeAssignableTo(readonlyScalars, type)) {
            union = readScalarUnion(scalarElements);
            if (checker.isTypeAssignableTo(readonlyData, type))
                union.variants.push({ kind: 'plainRecord' });
        }
        if (!readOnly && partial && position !== 'invariant' &&
            element.getSymbol()?.declarations?.some(ts.isInterfaceDeclaration)) {
            const item = attempt(() => readType(element, nested, position, true, unsupported)).value;
            // Input array literals and output views both carry the original
            // JavaScript array. Keep constructible records on the input side.
            if (item?.kind === 'object' && item.typeDefinition)
                return { kind: position === 'input' ? 'recordArray' : 'array', readOnly, elementType: item };
            // An open element can still expose stable declared metadata.
            // Never allow replacement of erased entries or writes to the source array.
            if (position === 'output' && item?.kind === 'object')
                return { kind: 'array', readOnly: true, elementType: {
                    ...item, members: item.members.map(member => ({ ...member, readOnly: true })),
                } };
        }
        if (position === 'input' && !readOnly && !union && element.isUnion()) {
            const present = element.types.filter(branch => !(branch.flags & (ts.TypeFlags.Null | ts.TypeFlags.Undefined)) &&
                (!(branch.flags & ts.TypeFlags.BooleanLiteral) || branch.intrinsicName !== 'false'));
            if (present.length === 1 && present.length < element.types.length) {
                const handle = readHandle(present[0], nested, 'input', unsupported) ?? readProtocolAlias(present[0]);
                if (handle)
                    return { kind: 'inputArray', elementType: handle };
            }
        }
        return {
            kind: 'array', readOnly,
            elementType: union ?? readType(input, nested, readOnly ? position : 'invariant', false, unsupported),
        };
    }
    const objectIntersection = (position === 'output' || position === 'input' && partial) && type.isIntersection() &&
        type.types.every(branch => (branch.flags & ts.TypeFlags.Object) &&
            !(branch.getSymbol()?.flags & ts.SymbolFlags.Class) &&
            (position === 'output' || !branch.getCallSignatures().length && !branch.getConstructSignatures().length));
    const objectUnion = projectingCallbackOptions && position === 'output' && partial && type.isUnion() &&
        type.types.every(branch => branch.flags & ts.TypeFlags.Object || branch.isIntersection());
    const indexes = checker.getIndexInfosOfType(type);
    if (indexes.length === 1 && indexes[0].keyType === checker.getStringType() &&
        !type.getProperties().length && !type.getCallSignatures().length && !type.getConstructSignatures().length) {
        if (position === 'input') {
            // A string-keyed object can omit any key. Preserve the complete
            // defined element domain when a sparse entry also admits undefined.
            const element = omitUndefined(indexes[0].type);
            const elementType = readScalarUnion(element, true) ?? readType(element, nested, 'invariant', false, unsupported);
            return { kind: 'dictionary', elementType };
        }
        // A sparse string index has a concrete read domain even for a defaulted
        // generic call. There is no reason to promise writes through that view.
        if (position === 'output' && omitUndefined(indexes[0].type) === checker.getStringType())
            return { kind: 'readonlyDictionary', elementType: { kind: 'string' } };
    }
    if ((!(type.flags & ts.TypeFlags.Object) && !objectIntersection && !objectUnion) ||
        checker.isTupleType(type) || (position !== 'output' && indexes.length) ||
        type.getConstructSignatures().length)
        unsupported(`type '${display(type)}'`);
    const calls = type.getCallSignatures();
    const properties = type.getProperties();
    if (projectingCallbackOptions && position === 'output' && partial && !properties.length && calls.length === 1) {
        const [signature] = calls;
        const restTuple = signature.parameters.length === 1 &&
            (checker.hasEffectiveRestParameter(signature) || signature.declaration?.parameters[0]?.dotDotDotToken) &&
            typeOf(signature.parameters[0]);
        if (restTuple && checker.isTupleType(restTuple)) {
            // A tuple rest parameter can have a fixed required prefix.
            // Keep that prefix and omit only trailing optional slots.
            const args = checker.getTypeArguments(restTuple);
            const required = restTuple.target.minLength;
            const flags = restTuple.target.elementFlags;
            if (required > 0 && flags.slice(0, required).every(flag => flag === ts.ElementFlags.Required) &&
                flags.slice(required).every(flag => flag === ts.ElementFlags.Optional)) {
                const projected = attempt(() => ({
                    parameters: args.slice(0, required).map((argument, index) => ({
                        name: restTuple.target.labeledElementDeclarations?.[index]?.name?.getText() ?? `arg${index}`,
                        optional: false, type: readType(argument, nested, 'input', false, unsupported),
                    })),
                    returnType: readReturn(signature, nested, 'output', true, unsupported),
                })).value;
                if (projected)
                    return { kind: 'function', signatures: [projected] };
            }
        }
        // TS permits omitting unconstrained generic arguments in some
        // instantiations. The exposed C# variable carrier still supplies it.
        const count = signature.parameters.reduce((count, parameter, index) =>
            callbackMarkers.has(omitUndefined(typeOf(parameter)).getSymbol()) ? Math.max(count, index + 1) : count,
        signature.minArgumentCount);
        if (!signature.typeParameters?.length && !signature.thisParameter &&
            !checker.hasEffectiveRestParameter(signature) && count <= signature.parameters.length) {
            const projected = attempt(() => ({
                parameters: signature.parameters.slice(0, count).map(parameter => ({
                    name: parameter.name, optional: false,
                    type: readType(omitUndefined(typeOf(parameter)), nested, 'input', false, unsupported),
                })),
                returnType: readReturn(signature, nested, 'output', true, unsupported),
            })).value;
            if (projected)
                return { kind: 'function', signatures: [projected] };
        }
    }
    if (position === 'output' && !properties.length && calls.some(signature => signature.typeParameters?.length)) {
        const scalars = readScalarCalls(calls);
        if (scalars.length)
            return { kind: scalars.length === 1 ? 'function' : 'method', signatures: scalars };
    }
    if (calls.length === 1 && !properties.length && position !== 'invariant')
        return { kind: 'function', signatures: calls.map(signature => readSignature(signature, nested, position, unsupported, partial)) };
    if (type.getSymbol()?.flags & ts.SymbolFlags.Class && position === 'output' && partial) {
        const members = [];
        for (const member of properties) {
            const declarations = member.declarations ?? [];
            if (optional(member) || !declarations.length || !declarations.every(node => ts.isMethodDeclaration(node) &&
                (ts.isIdentifier(node.name) || ts.isStringLiteral(node.name)) &&
                !(ts.getCombinedModifierFlags(node) & (ts.ModifierFlags.Private | ts.ModifierFlags.Protected))))
                continue;
            const signatures = checker.getTypeOfPropertyOfType(type, member.name).getCallSignatures();
            if (signatures.length !== 1)
                continue;
            const [signature] = signatures;
            // With no required arguments, the caller can use the declaration's
            // default call. Only expose fixed return domains at this boundary.
            if (signature.minArgumentCount || signature.thisParameter || checker.hasEffectiveRestParameter(signature))
                continue;
            const result = signature.getReturnType();
            const kind = result.flags & ts.TypeFlags.Void ? 'void' : scalar(result);
            if (!kind)
                continue;
            const location = definition(signature.declaration);
            members.push({ name: member.name, type: { kind: 'method', signatures: [
                { parameters: [], returnType: { kind }, definition: location },
            ] }, readOnly: true, definition: location });
        }
        if (members.length)
            return { kind: 'object', members };
    }
    if (calls.length || !properties.length || position === 'invariant' || type.getSymbol()?.flags & ts.SymbolFlags.Class)
        unsupported(`type '${display(type)}'`);
    const members = [];
    for (const member of properties) {
        const declarations = member.declarations ?? [];
        // Mapped types can synthesize ordinary output properties with no source
        // declaration. Expose those as reads without a definition target.
        const synthetic = !declarations.length;
        if (synthetic && (position !== 'output' || member.name.startsWith('__@')) ||
            declarations.some(node => node.name && ts.isComputedPropertyName(node.name)))
            unsupported(`member '${member.name}' of '${display(type)}'`);
        const memberType = checker.getTypeOfPropertyOfType(type, member.name);
        if (projectingCallbackOptions && position === 'output' &&
            hasConditionalAnnotation(ts, checker, type, member))
            continue;
        const isOptional = optional(member);
        // An unconstrained C# T? cannot represent undefined when T is a value type.
        if (validatorTypeParameter && position === 'output' && partial && isOptional &&
            omitUndefined(memberType) === validatorTypeParameter)
            continue;
        let readOnly = synthetic || Boolean(ts.getCheckFlags(member) & ts.CheckFlags.Readonly) ||
            declarations.some(node => ts.getCombinedModifierFlags(node) & ts.ModifierFlags.Readonly) ||
            declarations.some(ts.isGetAccessorDeclaration) && !declarations.some(ts.isSetAccessorDeclaration);
        const actualType = isOptional ? omitUndefined(memberType) : memberType;
        // The nullable getter normalizes absence to null. Keep a setter only
        // if that same null is legal on the TypeScript write side.
        if (position === 'output' && isOptional &&
            !(actualType.flags & ts.TypeFlags.Null || actualType.isUnion() && actualType.types.some(type => type.flags & ts.TypeFlags.Null)))
            readOnly = true;
        const memberPosition = position === 'input' || readOnly ? position : 'invariant';
        // A property initializer has a target type. A closed string domain can
        // therefore use the existing Jazor string-enum lowering without widening writes.
        let projected;
        if (position === 'input' && actualType.isUnion() &&
            actualType.types.every(branch => branch.flags === ts.TypeFlags.StringLiteral)) {
            const annotation = declarations.find(node => node.type && ts.isTypeReferenceNode(node.type))?.type;
            const name = annotation && !annotation.typeArguments?.length ?
                (ts.isQualifiedName(annotation.typeName) ? annotation.typeName.right : annotation.typeName).text : member.name;
            projected = { value: { kind: 'stringEnum', name, stringLiterals: actualType.types.map(branch => branch.value).sort() } };
        } else if (position === 'input' && actualType.isUnion() && actualType.types.every(branch =>
            branch.flags === ts.TypeFlags.NumberLiteral && Number.isInteger(branch.value) &&
            branch.value >= -2147483648 && branch.value <= 2147483647)) {
            // Preserve each literal in the C# input domain and emit its actual JS number.
            const annotation = declarations.find(node => node.type && ts.isTypeReferenceNode(node.type))?.type;
            const name = actualType.aliasSymbol?.name ?? (annotation && !annotation.typeArguments?.length ?
                (ts.isQualifiedName(annotation.typeName) ? annotation.typeName.right : annotation.typeName).text : member.name);
            projected = { value: { kind: 'enum', name,
                enumMembers: actualType.types.map(branch => ({
                    name: branch.value < 0 ? `Negative${-branch.value}` : `Value${branch.value}`, value: branch.value,
                })),
            } };
        } else {
            projected = attempt(() => readType(actualType, nested, memberPosition, partial, unsupported));
        }
        if (!projected.value && position === 'input' && partial)
            projected.value = readMethodOption(actualType, methodParameters ?? [], nested, unsupported);
        if (!projected.value && position === 'output' && !readOnly) {
            projected = attempt(() => readType(actualType, nested, 'output', partial, unsupported));
            readOnly = true;
        }
        if (!projected.value) {
            // Optional inputs can be left absent. A result can expose a subset of
            // its fields because this projection never constructs that result.
            if (partial && (position === 'output' || position === 'input' && isOptional))
                continue;
            throw projected.error;
        }
        members.push({
            name: member.name,
            type: projected.value,
            optional: isOptional,
            readOnly,
            definition: definition(declarations[0]),
        });
    }
    if (!members.length)
        unsupported(`type '${display(type)}'`);
    const shape = { kind: 'object', members };
    if (position !== 'input' && position !== 'output')
        return shape;
    // A local alias of an instantiated interface still refers to the same
    // checker target as the interface spelling used by the next export.
    const target = type.target?.getSymbol();
    const namedTarget = target?.flags & ts.SymbolFlags.Interface ? target : undefined;
    const declarations = (namedTarget ?? type.aliasSymbol ?? type.getSymbol())?.declarations;
    const [declaration] = declarations ?? [];
    if (declarations?.length !== 1 || !(ts.isSourceFile(declaration.parent) || ts.isModuleBlock(declaration.parent)) ||
        !(ts.isInterfaceDeclaration(declaration) || ts.isTypeAliasDeclaration(declaration)))
        return shape;
    const named = { ...shape, name: declaration.name.text, typeDefinition: definition(declaration) };
    const parameters = declaration.typeParameters;
    if (!parameters?.length)
        return named;
    const args = namedTarget ? checker.getTypeArguments(type) : type.aliasTypeArguments;
    if (args?.length !== parameters.length)
        return shape;
    // Use the original type arguments for identity. An input member may expose
    // only a subset of a union, and a phantom parameter may have no member at all.
    if (parameters.every((p, index) => p.default && checker.getTypeFromTypeNode(p.default) === args[index]))
        return named;
    const typeArguments = args.map(argument => {
        const primitive = scalar(argument);
        return primitive ? { kind: primitive } : isDate(argument) ? { kind: 'date' } :
            webTypes(argument) ?? readScalarUnion(argument);
    });
    return typeArguments.every(Boolean) ? { ...named, typeArguments } : shape;
}

function domSymbol(name) {
    const symbol = checker.resolveName(name, source, ts.SymbolFlags.Type, false);
    const declarations = symbol?.declarations ?? [];
    if (declarations.some(node => path.basename(node.getSourceFile().fileName) === 'lib.dom.d.ts') &&
        declarations.every(isDefaultLibraryDeclaration))
        return symbol;
}

function isDefaultLibraryDeclaration(node) {
    const file = node.getSourceFile();
    return program.isSourceFileDefaultLibrary(file) &&
        host.getCanonicalFileName(path.dirname(path.normalize(file.fileName))) ===
        host.getCanonicalFileName(path.normalize(defaultLibraryDirectory));
}

function bindWebTypes() {
    const carriers = new Map();
    return type => {
        if (!(type.flags & ts.TypeFlags.Object) || !(type.objectFlags & ts.ObjectFlags.Interface) &&
            !(type.objectFlags & ts.ObjectFlags.Reference))
            return;
        const symbol = type.getSymbol();
        if (!symbol)
            return;
        if (!carriers.has(symbol)) {
            const name = Object.hasOwn(webIdlCarriers, symbol.name) && webIdlCarriers[symbol.name];
            // A generic interface or global augmentation can promise members
            // outside the fixed CLR carrier. Preserve the TS declaration boundary.
            const unmodified = candidate => candidate?.declarations?.some(ts.isInterfaceDeclaration) &&
                candidate.declarations.every(node => isDefaultLibraryDeclaration(node) &&
                    (ts.isVariableDeclaration(node) || ts.isInterfaceDeclaration(node)));
            const standard = name && domSymbol(symbol.name) === symbol && unmodified(symbol) &&
                !symbol.declarations.some(node => ts.isInterfaceDeclaration(node) && node.typeParameters?.length) &&
                !type.getConstructSignatures().length &&
                // Generic standard-library mixins (Worker's MessageEventTarget<Worker>)
                // are part of the fixed instance contract, even though the carrier itself is closed.
                (type.getBaseTypes() ?? []).every(function standardBase(base) {
                    return unmodified(base.getSymbol()) && (base.getBaseTypes() ?? []).every(standardBase);
                });
            carriers.set(symbol, standard ? { kind: 'web', name } : undefined);
        }
        return carriers.get(symbol);
    };
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
