import { webIdlProjection } from './web-idl-projection.mjs';
import { readCallableProtocol, readProtocolArray } from './callable-protocol-projection.mjs';
import { readSubmissionProjection } from './submission-projection.mjs';
import { readPropertyKeySignature } from './dependent-path.mjs';
import { readFiniteMappedRecord, readNamedRecord } from './named-record-projection.mjs';
import { finiteBooleanProbes, readFiniteBooleanSignatures } from './finite-boolean-projection.mjs';
import { pagedQueryProbes, readPagedQueries } from './paged-query-projection.mjs';
import { readQueryFetch } from './query-client-projection.mjs';
import { localeMessageMaps, readDynamicLocaleMessage, readLocaleMessageMethod, readStaticLocaleWriter, readStringMessageCall } from './locale-message-projection.mjs';
import { readStaticLocaleResources } from './static-locale-projection.mjs';
import { finiteNumberLiterals, namedNumberDomain } from './finite-number-domain.mjs';
import { createMessageReader } from './locale-message-grammar.mjs';
import { readQueryData } from './query-data-projection.mjs';
import fs from 'node:fs';
import { behaviorProbes, readBehaviorProjections } from './callback-behavior.mjs';
import { domEventProbes, readDomEventProjections } from './dom-event-projection.mjs';
import { asyncStateProbes, readAsyncStateProbes, readCorrelatedRefCall, refUpdaterProbes } from './correlated-ref-projection.mjs';
import { dateArguments, dateProjection, readDateProjection } from './date-specializations.mjs';
import { omittedOptionsProbes, vueComposableProjection } from './vue-composable-projection.mjs';
import { alternatives, callbackProbes, hasConditionalAnnotation, singleCall } from './generic-callbacks.mjs';
import { genericCallbackProjections, readGenericCallbackProjections } from './generic-callback-projections.mjs';
import { eventHookProbes, readEventHookProjections } from './event-hook-projection.mjs';
import { inputClosureSlot, readIndexedArrayContext, readSchemaFormProjections, schemaFormProbes } from './schema-form-projection.mjs';
import { packageChecker } from './package-checker.mjs';
import { pluginFactoryProbes, projectVuePlugin, readPluginFactory } from './vue-plugin.mjs';
import path from 'node:path';

const [typescript, moduleSpecifier, mode, query, webIdlCarriersJson, localeToolManifest] = globalThis.__jazorArguments;
const webIdlCarriers = JSON.parse(webIdlCarriersJson);
let staticMessageReader;
const needsLocaleTool = Symbol();
const exportName = mode === 'values' ? undefined : query;
let vueAvailable;
const { ts, modulePath, entry, options, host, snapshotInputs, source: initialSource,
    program: initialProgram, definition, replaceSource, packageIdentity } =
    await packageChecker(import.meta.url, typescript, moduleSpecifier, (ts, entry, options, host) => {
        vueAvailable = ts.resolveModuleName('vue', entry, options, host).resolvedModule;
        return `
            import * as packageExports from ${JSON.stringify(moduleSpecifier)};
            type JoltStringDictionary = Record<string, string>;
            ${vueAvailable ? "type JoltVueObjectPlugin = import('vue').ObjectPlugin<[]>;" : ''}
        `;
    }, (ts, modulePath, options, moduleType, readInput) => {
        const exists = fs.existsSync(modulePath);
        const ambiguousJs = exists && modulePath.endsWith('.js') && moduleType == null;
        const lexicalEsm = ambiguousJs &&
            ts.createSourceFile(modulePath, readInput(modulePath), options.target, true, ts.ScriptKind.JS)
                .statements.some(statement => ts.isImportDeclaration(statement) || ts.isExportDeclaration(statement) ||
                    ts.isExportAssignment(statement) && !statement.isExportEquals ||
                    statement.modifiers?.some(modifier => modifier.kind === ts.SyntaxKind.ExportKeyword));
        if (!exists ||
            !(modulePath.endsWith('.mjs') || modulePath.endsWith('.js') && (moduleType === 'module' || lexicalEsm)))
            throw new Error(`'${moduleSpecifier}' does not resolve to a public ESM file.`);
    });
let source = initialSource;
let program = initialProgram;
const formatFlags = ts.TypeFormatFlags.NoTruncation | ts.TypeFormatFlags.UseAliasDefinedOutsideCurrentScope;

let checker = program.getTypeChecker();
let web = webIdlProjection(ts, program, source, host, webIdlCarriers);
const domSymbol = name => web.domSymbol(name);
const getAliases = () => checker.getExportsOfModule(checker.getSymbolAtLocation(source.statements[0].moduleSpecifier));
let aliases = getAliases();
const unalias = symbol => symbol.flags & ts.SymbolFlags.Alias ? checker.getAliasedSymbol(symbol) : symbol;
const typeOf = symbol => checker.getTypeOfSymbolAtLocation(symbol, source);
const display = type => checker.typeToString(type, source, formatFlags);
const optional = symbol => Boolean(symbol.flags & ts.SymbolFlags.Optional);
const declarationPackageManifests = new Map();
const activeAugmentedRefs = new Set();
function recordIdentity(declaration, includeOwners = false) {
    const file = path.resolve(declaration.getSourceFile().fileName);
    const names = [declaration.name.text];
    for (let node = declaration.parent; node && !ts.isSourceFile(node); node = node.parent) {
        if (ts.isModuleDeclaration(node))
            names.unshift(node.name.text);
        else if (includeOwners && (ts.isInterfaceDeclaration(node) || ts.isTypeAliasDeclaration(node) ||
            ts.isVariableDeclaration(node) || ts.isFunctionDeclaration(node) || ts.isPropertySignature(node)))
            names.unshift(node.name?.getText() ?? '');
    }
    const scope = declarationPackage(file);
    return typeof scope?.manifest.version === 'string' ?
        JSON.stringify([scope.manifest.name, scope.manifest.version, path.relative(scope.directory, file).replaceAll(path.sep, '/'), ...names]) : undefined;
}

function declarationPackage(file) {
    // The checker resolves linked packages to their actual files. Follow the
    // declaration's package scope so moving the link or checkout keeps its identity.
    for (let directory = path.dirname(file); directory !== path.dirname(directory); directory = path.dirname(directory)) {
        if (!declarationPackageManifests.has(directory)) {
            const json = host.readFile(path.join(directory, 'package.json'));
            declarationPackageManifests.set(directory, json ? JSON.parse(json) : null);
        }
        const manifest = declarationPackageManifests.get(directory);
        if (typeof manifest?.name === 'string')
            return { manifest, directory };
    }
}

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

// Cold completion only needs to project callable exports that declare properties.
// Read that subset in one checker session, using the same rules as a real call.
const requestedValues = new Set(mode === 'values' ? JSON.parse(query) :
    mode === 'editor' ? aliases.filter(alias => {
        const symbol = valueSymbol(alias);
        if (!symbol)
            return false;
        const type = typeOf(symbol);
        return type.getCallSignatures().length && type.getProperties().length;
    }).map(alias => alias.name) : exportName ? [exportName] : []);
let dateTypes = dateArguments(ts, program, checker, source);
const callsOf = alias => {
    const symbol = valueSymbol(alias);
    return symbol ? typeOf(symbol).getCallSignatures() : [];
};
const initialPromise = standardLibrarySymbol('Promise');
const initialReadonlyArray = standardLibrarySymbol('ReadonlyArray');
let error = standardLibrarySymbol('Error');
const directValues = new Map(aliases.flatMap(alias => {
    if (!requestedValues.has(alias.name))
        return [];
    const projection = readDirectGenericSignature(callsOf(alias), initialPromise, initialReadonlyArray);
    return projection ? [[alias.name, projection]] : [];
}));
const probeValues = new Set([...requestedValues].filter(name => !directValues.has(name)));
const finiteBooleanCalls = finiteBooleanProbes(ts, checker, aliases, callsOf, probeValues, typeOf);
const projections = dateProjection(aliases, callsOf, probeValues, dateTypes.argumentsFor);
const callbackTypes = callbackProbes(ts, program, checker, aliases, callsOf, probeValues);
const validatorProbes = genericCallbackProjections(ts, program, checker, source, aliases, callsOf, probeValues, typeOf);
const hooks = eventHookProbes(aliases, callsOf, probeValues);
const formProbes = schemaFormProbes(ts, program, checker, source, aliases, callsOf, probeValues, typeOf,
    declaration => declarationPackage(path.resolve(declaration.getSourceFile().fileName))?.manifest.name, vueRef, definition);
const pagingProbes = pagedQueryProbes(ts, checker, aliases, callsOf, probeValues, typeOf);
// Resolve the exact zero-argument call when a composable declares defaults.
// Supplying options can infer different generics, so this result belongs only to ().
const defaultCalls = aliases.filter(alias => probeValues.has(alias.name) && valueSymbol(alias) &&
    typeOf(valueSymbol(alias)).getCallSignatures().some(signature => signature.minArgumentCount === 0 &&
        hasOmittedTypeArguments(signature) &&
        !signature.thisParameter && !checker.hasEffectiveRestParameter(signature)));
const pluginFactories = vueAvailable ? pluginFactoryProbes(ts, checker, aliases, callsOf, probeValues,
    typeOf, vueObjectPluginContract(), defaultCalls,
    checker.getDeclaredTypeOfSymbol(checker.resolveName('JoltStringDictionary', source, ts.SymbolFlags.Type, false))) : [];
const eventProbes = domEventProbes(ts, () => checker, aliases, callsOf, probeValues,
    typeOf, domSymbol, web.webType);
const behaviorCalls = behaviorProbes(ts, checker, aliases, callsOf, probeValues, typeOf);
const omittedOptionCalls = omittedOptionsProbes(ts, checker, aliases, callsOf, probeValues, typeOf, dateTypes.isDate);
function vueObjectPluginContract() {
    const declaration = source.statements.find(node => ts.isTypeAliasDeclaration(node) && node.name.text === 'JoltVueObjectPlugin');
    return declaration && checker.getTypeAtLocation(declaration.type);
}
const elementRefCalls = aliases.filter(alias => probeValues.has(alias.name) &&
    callsOf(alias).length === 1 && elementRefParameters(callsOf(alias)[0]));
const stateProbes = [
    ...asyncStateProbes(checker, aliases, callsOf, probeValues, typeOf,
        checker.resolveName('Promise', source, ts.SymbolFlags.Type, false)),
    ...refUpdaterProbes(checker, aliases, callsOf, probeValues, typeOf, vueRef),
];
const probeExpressions = new Map();
const addProbes = (group, expression = probe => probe.expression) => {
    probeExpressions.set(group, group.map(expression));
};
addProbes(projections);
addProbes(finiteBooleanCalls);
addProbes(defaultCalls, alias => `packageExports[${JSON.stringify(alias.name)}]()`);
addProbes(callbackTypes);
addProbes(validatorProbes);
addProbes(hooks);
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
    replaceSource(source);
    program = ts.createProgram([entry], options, host, program);
    checker = program.getTypeChecker();
    web = webIdlProjection(ts, program, source, host, webIdlCarriers);
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
const eventHooks = readEventHookProjections(ts, checker, statementsByGroup.get(hooks), hooks,
    probeDiagnostics, standardLibrarySymbol('Promise'), definition);
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
let optionalValidatorValue;
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

const webTypes = web.webType;
const promise = standardLibrarySymbol('Promise');
const readonlyArray = standardLibrarySymbol('ReadonlyArray');
error = standardLibrarySymbol('Error');
const standardPromise = Boolean(promise);
function standardLibrarySymbol(name) {
    const symbol = checker.resolveName(name, source, ts.SymbolFlags.Type, false);
    return symbol?.declarations?.length &&
        symbol.declarations.every(node => program.isSourceFileDefaultLibrary(node.getSourceFile())) ? symbol : undefined;
}
const scalarElements = checker.getUnionType([checker.getBooleanType(), checker.getNumberType(), checker.getStringType()]);
const readonlyScalars = checker.createArrayType(scalarElements, true);
const objectType = checker.getTypeFromTypeNode(ts.factory.createKeywordTypeNode(ts.SyntaxKind.ObjectKeyword));
const readonlyData = checker.createArrayType(checker.getUnionType([scalarElements, objectType]), true);
const schemaForms = readSchemaFormProjections(ts, checker, formProbes,
    statementsByGroup.get(formProbes), probeDiagnostics, definition, vueRef);
const asyncStates = readAsyncStateProbes(ts, checker, stateProbes, statementsByGroup.get(stateProbes), probeDiagnostics, vueRef, readUnknownRefOutput, definition);
const pagedQueries = readPagedQueries(ts, checker, pagingProbes, statementsByGroup.get(pagingProbes),
    probeDiagnostics, vueRef, standardPromise && promise, definition, readPagedQueryKey, readObserverRef);

function readObserverRef(type) {
    const ref = vueRef(type);
    if (!ref)
        return;
    const element = ref.element;
    const values = alternatives(element);
    const errorValue = branch => error && branch.getSymbol() === error;
    const elementType = readRefStringDomain(element) ??
        (values.every(branch => branch.flags & ts.TypeFlags.BooleanLiteral) ? { kind: 'boolean' } :
            errorValue(element) ? { kind: 'error' } :
            values.some(errorValue) && values.every(branch => errorValue(branch) || branch.flags === ts.TypeFlags.Null)
                ? { kind: 'nullable', elementType: { kind: 'error' } } : undefined);
    if (elementType)
        return { kind: 'ref', readOnly: true, elementType };
}

function readPagedQueryKey(type, constraint = type) {
    if (!type)
        return;
    for (const candidate of [readonlyData, readonlyScalars]) {
        if (checker.isTypeAssignableTo(candidate, type) && checker.isTypeAssignableTo(candidate, constraint))
            return readType(candidate, new Set(), 'input', false);
    }
    const strings = checker.createArrayType(checker.getStringType());
    if (checker.isTypeAssignableTo(strings, type) && checker.isTypeAssignableTo(strings, constraint))
        return { kind: 'array', readOnly: true, elementType: { kind: 'string' } };
}

try {
    writeResult();
} catch (error) {
    if (error !== needsLocaleTool)
        throw error;
    console.log(JSON.stringify({ needsLocaleTool: true }));
}

function writeResult() {
    if (mode === 'values' || mode === 'editor') {
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
        console.log(JSON.stringify({ values, errors: Object.fromEntries(errors), exports: valueCandidates(), ...snapshotInputs() }));
    } else if (!exportName) {
        const exports = valueCandidates();
        console.log(JSON.stringify({ exports, ...snapshotInputs() }));
    } else {
        const symbol = aliases.find(symbol => symbol.name === exportName);
        if (!symbol)
            throw new Error(`'${moduleSpecifier}' has no export named '${exportName}'.`);
        const value = readValue(symbol);
        console.log(JSON.stringify({ value, exports: valueCandidates(), ...snapshotInputs() }));
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
    const value = readValueCore(alias);
    const type = typeOf(valueSymbol(alias));
    if (!type.getCallSignatures().length)
        return value;
    const attachedMembers = type.getProperties().flatMap(member => {
        if (member.name.startsWith('__@'))
            return [];
        const memberType = checker.getTypeOfPropertyOfType(type, member.name);
        const declaration = definition(member.valueDeclaration ?? member.declarations?.[0]);
        const unsupported = detail => {
            throw new UnsupportedValueError(detail);
        };
        const calls = memberType.getCallSignatures();
        if (!calls.length) {
            if (optional(member) && omitUndefined(memberType).getCallSignatures().length)
                return [];
            const projected = attempt(() => readType(memberType, new Set(), 'output', true, unsupported)).value;
            return projected ? [{ name: member.name, type: projected, optional: optional(member), definition: declaration, readOnly: true }] : [];
        }
        if (optional(member) || calls.some(call => call.thisParameter))
            return [];
        const signatures = attempt(() => readSignatures(calls, unsupported, undefined, new Set(), true, true)).value;
        return signatures?.length ? [{
            ...methodMember(member.name, signatures),
            definition: declaration ?? signatures[0].definition,
        }] : [];
    });
    return attachedMembers.length ? { ...value, attachedMembers } : value;
}

function readValueCore(alias) {
    const symbol = valueSymbol(alias);
    const unsupported = (detail, position, node) => {
        const declaration = node && definition(node) ? node : symbol?.valueDeclaration ?? alias.declarations?.[0];
        const location = definition(declaration);
        const place = location && declaration.getSourceFile().getLineAndCharacterOfPosition(location.start);
        throw new UnsupportedValueError(`Unsupported npm value export '${alias.name}' from '${moduleSpecifier}': ${detail}.` +
            (position ? ` Failed at ${position}.` : '') +
            (place ? ` TypeScript declaration: ${path.relative(path.dirname(entry), location.path).replaceAll(path.sep, '/')}(${place.line + 1},${place.character + 1}).` : ''));
    };
    if (!symbol)
        unsupported('expected a runtime value');
    const type = typeOf(symbol);
    const definitionNode = symbol.valueDeclaration;
    const calls = type.getCallSignatures();
    const identity = { ...packageIdentity(), exportName: alias.name };
    if (!/\.d\.(?:ts|mts)$/.test(identity.declarationPath))
        throw new Error(`'${moduleSpecifier}' must publish TypeScript declarations for its ESM entry.`);
    if (calls.length) {
        if (directValues.has(alias.name))
            return { ...identity, definition: definition(definitionNode), signatures: directValues.get(alias.name), constantType: null };
        const finiteBoolean = attempt(() => readFiniteBooleanSignatures(ts, checker, finiteBooleanCalls,
            statementsByGroup.get(finiteBooleanCalls), probeDiagnostics, alias.name, calls[0], typeOf, omitUndefined,
            type => attempt(() => readType(type, new Set(), 'input', false, unsupported)).value,
            call => attempt(() => readReturn(call, new Set(), 'output', false, unsupported)).value, definition)).value;
        if (finiteBoolean)
            return { ...identity, definition: definition(definitionNode), signatures: finiteBoolean, constantType: null };
        const pagedQuery = pagedQueries.get(alias.name);
        if (pagedQuery)
            return { ...identity, definition: definition(definitionNode), signatures: pagedQuery, constantType: null };
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
        const formSignatures = schemaForms.get(alias.name);
        const correlatedRefs = standardPromise && readCorrelatedRefCall(checker, calls, typeOf, vueRef, promise, definition);
        const omittedOptions = attempt(() => readOmittedOptions(alias.name, unsupported)).value;
        const defaultCall = resolvedDefaults.get(alias.name);
        const readOrdinary = () => readSignatures(dateSpecializations.signatures(alias.name, calls), unsupported, defaultCall);
        const ambientForm = formSignatures?.some(signature => signature.returnType.kind === 'undefinedable');
        const signatures = bestCallback && calls.every(call => call.typeParameters?.length)
            ? specialized.filter(entry => entry.probe === bestCallback.probe).map(entry => entry.projected) : validationSignatures?.length
            ? [...(attempt(readOrdinary).value ?? []), ...validationSignatures]
            : resolvedElementRefCalls.has(alias.name) ? readElementRefCall(resolvedElementRefCalls.get(alias.name), unsupported)
            : correlatedRefs?.length ? correlatedRefs
            : eventHooks.has(alias.name) ? [eventHooks.get(alias.name)]
            : asyncStates.has(alias.name) ? asyncStates.get(alias.name)
            : ambientForm ? (attempt(readOrdinary).value ?? []).filter(signature =>
                signature.parameters.length === 0 && !signature.typeParameters?.length).map(signature => ({
                ...signature,
                // An ambient lookup has the same provider requirement at either overload.
                returnType: signature.returnType.kind === 'undefinedable' ? signature.returnType :
                    { kind: 'undefinedable', elementType: signature.returnType },
            }))
            : formSignatures && !defaultCall ? []
            : omittedOptions?.length ? omittedOptions.some(signature => isSparseDictionary(signature.returnType))
                ? [...(attempt(readOrdinary).value ?? []), ...omittedOptions] : omittedOptions
            : readOrdinary();
        if (formSignatures)
            signatures.push(...formSignatures);
        if (factory && !factory.probe.plugin &&
            !signatures.some(signature => signature.returnType.kind === 'enrichedRef' && signature.parameters.length === 1)) {
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
            if (!member || !(member.flags & ts.SymbolFlags.Function) && !isConstant(member.valueDeclaration))
                return [];
            const calls = typeOf(member).getCallSignatures();
            if (!calls.length)
                return [];
            const projected = attempt(() => readSignatures(calls, unsupported, undefined, new Set(), true)).value;
            return projected?.length ? [methodMember(alias.name, projected)] : [];
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

function isSparseDictionary(type) {
    return type.kind === 'readonlyDictionary' || type.kind === 'writableDictionary';
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
        if (!signature || !probe.sparseDefault && !vueRef(signature.getReturnType()))
            continue;
        const name = signature.parameters[0].name;
        const location = definition(signature.declaration);
        const returnType = attempt(() => readReturn(signature, new Set(), 'output', true, unsupported)).value;
        if (!returnType || probe.sparseDefault && !isSparseDictionary(returnType))
            continue;
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
        const localeMessage = readLocaleMessageMethod(ts, checker, calls, definition) ??
            readDynamicLocaleMessage(ts, checker, calls, definition);
        if (localeMessage) {
            projected.push(methodMember(member.name, [localeMessage]));
            continue;
        }
        const data = readQueryData(ts, checker, calls, definition, readPagedQueryKey);
        if (data) {
            projected.push(methodMember(member.name, data));
            continue;
        }
        const fetch = readQueryFetch(ts, checker, calls, promise, definition, readPagedQueryKey);
        if (fetch) {
            projected.push(methodMember(member.name, [fetch]));
            continue;
        }
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
            if (result.flags & ts.TypeFlags.Void || scalar(result) || genericParameters)
                return true;
            // A zero-argument snapshot getter and an event carrying the same
            // readonly interface can share the existing named result projection.
            if (!signature.parameters.length && !signature.typeParameters?.length &&
                result.getSymbol()?.declarations?.some(ts.isInterfaceDeclaration)) {
                const fields = result.getProperties();
                if (fields.length && fields.every(field => field.declarations?.every(node =>
                    ts.isPropertySignature(node) && ts.getCombinedModifierFlags(node) & ts.ModifierFlags.Readonly)))
                    return true;
            }
            // Establish the inner handle's declaration identity without walking
            // its members. The normal signature reader can then project the
            // nullable return and validate its arguments at the same boundary.
            if (!result.isUnion() || result.types.length !== 2 || !result.types.some(branch => branch.flags & ts.TypeFlags.Null))
                return false;
            const target = result.types.find(branch => !(branch.flags & ts.TypeFlags.Null));
            return readHandle(target, nested, 'input', unsupported)?.kind === 'handle';
        }))
            continue;
        const signatures = attempt(() => readSignatures(calls, unsupported, undefined, nested, false, true)).value;
        if (signatures)
            projected.push(methodMember(member.name, signatures));
    }
    for (const member of members) {
        if (optional(member) || !member.declarations?.some(node =>
            ts.isPropertySignature(node) && node.modifiers?.some(modifier => modifier.kind === ts.SyntaxKind.ReadonlyKeyword)))
            continue;
        const result = checker.getTypeOfPropertyOfType(type, member.name);
        const kind = result && scalar(result);
        if (kind) {
            projected.push({ name: member.name, type: { kind }, readOnly: true, definition: definition(member.declarations[0]) });
            continue;
        }
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
    // IEvent<void> in Monaco still declares one parameter. A zero-argument
    // listener ignores that JS argument and retains the normal subscription handle.
    const noPayload = callback.parameters.length === 1 && typeOf(callback.parameters[0]).flags === ts.TypeFlags.Void;
    if (!(callback.getReturnType().flags & (ts.TypeFlags.Void | ts.TypeFlags.Any)) ||
        !noPayload && callback.minArgumentCount !== callback.parameters.length)
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
        const parameters = noPayload ? [] : callback.parameters.map(parameter => ({
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
        if (key === 'documentation' || key === 'documentationFromCallableOwner' || key === 'definition' ||
            key === 'typeDefinition' && this.kind !== 'handle' && this.kind !== 'enum' ||
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

function readSignatures(signatures, unsupported, defaultCall, ancestors = new Set(), allowGeneric = false, hostMethod = false) {
    const direct = readDirectGenericSignature(signatures, promise, readonlyArray);
    if (direct)
        return direct;
    const indexedRefs = readIndexedPropertySignature(signatures, true);
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
        if (signatures.length > 1 && signature.minArgumentCount === signature.parameters.length) {
            const directOverloads = readDirectGenericSignature([signature], promise, readonlyArray);
            // A directly inferred overload can retain its generic slots when no
            // earlier declaration accepts its full argument list.
            if (directOverloads?.every(projected => !earlier.some(previous =>
                mayMatch(previous.signature, projected.parameters)))) {
                for (const projected of directOverloads) {
                    const key = shapeKey(projected.parameters.map(parameter => parameterTypeKey(parameter.type)));
                    if (!seen.has(key)) {
                        seen.add(key);
                        result.push(projected);
                    }
                }
                earlier.push({ signature, returnKey: shapeKey(directOverloads[0].returnType) });
                continue;
            }
        }
        const closureInput = signatures.length === 1 && inputClosureSlot(ts, checker, typeOf, signature);
        const slot = signature.typeParameters?.[0];
        const predicate = checker.getTypePredicateOfSignature(signature);
        const genericPredicate = signatures.length === 1 && signature.typeParameters?.length === 1 &&
            signature.parameters.length === 1 && !checker.getBaseConstraintOfType(slot) &&
            predicate?.kind === ts.TypePredicateKind.Identifier && predicate.parameterIndex === 0 &&
            predicate.type === slot && omitUndefined(typeOf(signature.parameters[0])) === slot;
        // An unconstrained result-only slot can remain erased if every exposed
        // member has a concrete type independent of that slot. Required inputs
        // still pass the normal projection checks and cannot accept an open T.
        const erasedResult = signatures.length === 1 && signature.typeParameters?.length &&
            signature.typeParameters.every(parameter => !hasDeclaredConstraint(parameter) &&
                (!parameter.getDefault() || parameter.getDefault().flags & ts.TypeFlags.Unknown));
        // A host method with an exact void result can use its TypeScript defaults
        // when filters are omitted. Supplied fields still need an accepted input domain.
        const defaultedVoidMethod = hostMethod && signature.getReturnType().flags === ts.TypeFlags.Void &&
            !signature.minArgumentCount && signature.typeParameters?.length &&
            signature.typeParameters.every(parameter => parameter.getDefault());
        if (signature.typeParameters?.length && defaultCall?.declaration === signature.declaration) {
            const resultType = attempt(() => readReturn(defaultCall, new Set(), 'output', true, unsupported));
            if (resultType.value && !seen.has('[]')) {
                result.push({ parameters: [], returnType: resultType.value, definition: definition(signature.declaration) });
                seen.add('[]');
            }
            if (resultType.value?.kind === 'enrichedRef' && signatures.length === 1 &&
                defaultCall.parameters.length === 1 && signature.typeParameters.every(parameter => parameter.getDefault())) {
                // The checker instantiated the omitted signature with the declared
                // defaults. Supplying only options accepted by that instantiation
                // also satisfies the upstream function with the same type arguments.
                const [parameter] = defaultCall.parameters;
                const options = attempt(() =>
                    readType(omitUndefined(typeOf(parameter)), new Set(), 'input', true, unsupported)).value;
                if (options?.kind === 'object')
                    result.push({
                        parameters: [{ name: parameter.name, type: options, optional: false }],
                        returnType: resultType.value,
                        definition: definition(signature.declaration),
                    });
            }
        }
        const partialRecord = signature.parameters.length === 0 && signature.typeParameters?.length === 1 &&
            partialRecordSlot(vueRef(signature.getReturnType())?.element ?? signature.getReturnType()) === slot;
        const contextualComputed = signatures.length === 1 && composable.contextualComputed(signature);
        const forwardsParameters = forwardsPromise || forwardsReadonlyRef || closureInput || genericPredicate || partialRecord || contextualComputed;
        const typeParameters = forwardsParameters ? signature.typeParameters : allowGeneric && signature.typeParameters?.every(parameter =>
            checker.getBaseConstraintOfType(parameter)?.flags & ts.TypeFlags.NonPrimitive)
            ? signature.typeParameters : undefined;
        const context = typeParameters ? new Set([...ancestors, ...typeParameters]) : ancestors;
        const projection = attempt(() => {
            checkSignature(signature, unsupported, typeParameters || erasedResult || defaultedVoidMethod);
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
            let useExplicitArities = signatures.length > 1 || isVoidThunk(signature.getReturnType()) || genericPredicate || defaultedVoidMethod;
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
                // An exact-void host command can consume a record through an unknown
                // parameter without exposing any unchecked value to the C# caller.
                const recordInput = hostMethod && signature.getReturnType().flags === ts.TypeFlags.Void &&
                    type.flags === ts.TypeFlags.Unknown;
                const projected = recordInput ? { value: [{ kind: 'plainRecord' }] } :
                    predicateInputs ? { value: predicateInputs } : getterInput ? { value: [getterInput] } :
                    attempt(() => readInputBranches(type, context, true, (detail, position, node) =>
                        unsupported(detail, `parameter '${parameter.name}'` + (position ? ` → ${position}` : ''),
                            node ?? parameter.valueDeclaration ?? signature.declaration), forwardsReadonlyRef,
                        defaultedVoidMethod ? signature.typeParameters : undefined));
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
            if (useExplicitArities) {
                const competing = earlier.filter(previous => previous.returnKey !== returnKey);
                optionalArities.forEach(parameters => add(parameters.map(parameter => ({ ...parameter, optional: false }))));
                for (const parameters of prefixes) {
                    const optionalCount = parameters.filter(parameter => parameter.optional).length;
                    // Preserve shorter overloads for delegate binding. The full overload
                    // can additionally accept named arguments if each newly exposed call
                    // stays within the same TypeScript return contract.
                    const useDefaults = signatures.length > 1 &&
                        optionalCount > 1 &&
                        prefixes.every(other => other === parameters || other.every((parameter, index) =>
                            !parameter.optional || shapeKey(parameter.type) === shapeKey(parameters[index].type))) &&
                        competing.every(previous => !mayMatchWithOmittedArguments(previous.signature, parameters)) &&
                        !result.some(previous => previous.parameters.filter(parameter => parameter.optional).length > 1 &&
                            previous.parameters.some(parameter => parameter.optional && parameters.some(candidate =>
                                candidate.optional && candidate.name === parameter.name)) &&
                            mayMatchWithOmittedArguments(signature, previous.parameters.filter(parameter => !parameter.optional)));
                    add(useDefaults ? parameters : parameters.map(parameter => ({ ...parameter, optional: false })));
                }
            }
            else
                prefixes.forEach(add);
        } else {
            failure ??= projection.error;
        }
        earlier.push({ signature, returnKey: projection.value && shapeKey(projection.value) });
    }
    if (signatures.length === 1 && result.length && result.every(signature => !signature.typeParameters?.length)) {
        const indexedArray = readIndexedArrayContext(ts, checker, typeOf, vueRef, definition, signatures[0]);
        if (indexedArray)
            return [...result, ...result.filter(signature => signature.parameters.length === 1)
                .map(signature => ({ ...signature, ...indexedArray }))];
    }
    if (indexedRefs)
        return [...indexedRefs, ...result];
    if (!result.length) {
        if (failure)
            throw failure;
        unsupported('no unambiguous supported signature');
    }
    return result;
}

function hasDeclaredConstraint(slot) {
    // U extends T can have no effective base constraint when T is unconstrained.
    return slot.symbol.declarations?.some(node => ts.isTypeParameterDeclaration(node) && node.constraint) ||
        checker.getBaseConstraintOfType(slot);
}

function readIndexedPropertySignature(signatures, includeOtherArities = false) {
    return readPropertyKeySignature(ts, checker, signatures, typeOf, definition,
        (type, readElement) => readDirectVueRef(type, 'output', readElement, true), vueUnwrapArgument, includeOtherArities);
}

function vueUnwrapArgument(ref, element) {
    // The alias must be the one exported beside the canonical Vue Ref
    // declaration. A package may declare a different transformation with the same name.
    const declaration = ref.target?.getSymbol()?.declarations?.[0];
    const module = declaration && checker.getSymbolAtLocation(declaration.getSourceFile());
    const unwrap = module && checker.getExportsOfModule(module).find(symbol => symbol.name === 'UnwrapRef');
    if (unwrap && element.aliasSymbol === unwrap && element.aliasTypeArguments?.length === 1)
        return element.aliasTypeArguments[0];
}

function readDirectGenericSignature(signatures, promiseSymbol, readonlyArraySymbol) {
    const propertyKey = readIndexedPropertySignature(signatures);
    if (propertyKey)
        return propertyKey;
    const [signature] = signatures;
    if (signatures.length !== 1 || !signature.typeParameters?.length ||
        signature.thisParameter || checker.getTypePredicateOfSignature(signature) ||
        checker.hasEffectiveRestParameter(signature))
        return;
    const slots = new Set(signature.typeParameters);
    // An explicit unknown bound admits the same values as an omitted bound.
    // Check the written constraint too because `U extends T` can lack an effective base constraint.
    if (signature.typeParameters.some(slot => hasDeclaredConstraint(slot) &&
        !slot.symbol.declarations?.every(node => ts.isTypeParameterDeclaration(node) && node.constraint &&
            checker.getTypeAtLocation(node.constraint).flags & ts.TypeFlags.Unknown)))
        return;

    const inferred = new Set();
    const active = new Set();
    const activeRecords = new Map();
    function read(type, suppliesValue, position, permitsVoid = false, scope = slots, evidence = inferred, annotation) {
        if (scope.has(type)) {
            if (suppliesValue)
                evidence.add(type);
            return { kind: 'typeParameter', name: type.symbol.name };
        }
        const kind = scalar(type) || (permitsVoid && type.flags & ts.TypeFlags.Void ? 'void' : undefined);
        if (kind)
            return { kind };
        const namedRecord = referenceOnly => readNamedRecord(ts, checker, program, type, suppliesValue, activeRecords,
            (child, supplies, childScope = scope, childEvidence = evidence) =>
                read(child, supplies, 'invariant', false, childScope, childEvidence),
            child => scope.has(child) || scalar(child), definition, recordIdentity, referenceOnly);
        if (active.has(type))
            return namedRecord(true);
        active.add(type);
        let projection;
        if (promiseSymbol && type.target?.getSymbol() === promiseSymbol) {
            const elementType = read(checker.getTypeArguments(type)[0], suppliesValue, position, true, scope, evidence);
            // Native promises assimilate another promise instead of
            // producing it as an independently observable fulfillment.
            if (elementType && elementType.kind !== 'promise')
                projection = { kind: 'promise', elementType };
        } else if (readonlyArraySymbol && checker.isArrayType(type) && type.target?.getSymbol() === readonlyArraySymbol) {
            const elementType = read(checker.getTypeArguments(type)[0], suppliesValue, position, false, scope, evidence);
            if (elementType)
                projection = { kind: 'array', readOnly: true, elementType };
        } else if (type.getCallSignatures().length) {
            const calls = type.getCallSignatures();
            const [call] = calls;
            // A lambda's parameters receive contextual types. Only slots
            // supplied directly or through callback results drive inference.
            if (calls.length === 1 && !type.getProperties().length && !checker.getIndexInfosOfType(type).length &&
                !type.getConstructSignatures().length &&
                !call.thisParameter && !call.typeParameters?.length && !checker.getTypePredicateOfSignature(call) &&
                !checker.hasEffectiveRestParameter(call) && call.minArgumentCount === call.parameters.length) {
                const parameterPosition = position === 'invariant' ? position : position === 'input' ? 'output' : 'input';
                const parameters = readParameters(call, false, parameterPosition, scope, evidence);
                const returnType = parameters && read(call.getReturnType(), suppliesValue, position, true, scope, evidence);
                if (returnType)
                    projection = { kind: 'function', signatures: [{ parameters, returnType, definition: definition(call.declaration) }] };
            }
        } else {
            projection = readDirectVueRef(type, position,
                (element, usePosition) => read(element, suppliesValue, usePosition, false, scope, evidence)) ??
                readAugmentedVueRef(type,
                    argument => read(argument, suppliesValue, 'invariant', false, scope, evidence),
                    (member, usePosition, permitsVoid, formals) =>
                        read(member, false, usePosition, permitsVoid, formals, new Set())) ??
                (position !== 'invariant' ? readFiniteMappedRecord(ts, checker, program, type, suppliesValue,
                    (child, supplies) => read(child, supplies, 'invariant', false, scope, evidence),
                    omitUndefined, definition, recordIdentity, annotation) : undefined) ??
                namedRecord();
        }
        active.delete(type);
        return projection;
    }
    function readParameters(call, suppliesValue, position, scope, evidence) {
        const parameters = [];
        for (const [index, parameter] of call.parameters.entries()) {
            const optional = index >= call.minArgumentCount;
            // Omitting every trailing option must still leave enough evidence
            // for the required arguments to determine all generic slots.
            const declared = typeOf(parameter);
            const type = read(optional ? omitUndefined(declared) : declared, suppliesValue && !optional, position, false, scope, evidence,
                parameter.valueDeclaration?.type);
            if (!type)
                return;
            parameters.push({ name: parameter.name, type, optional });
        }
        return parameters;
    }
    const parameters = readParameters(signature, true, 'input', slots);
    const returnType = parameters && inferred.size === slots.size &&
        read(signature.getReturnType(), false, 'output', true, slots, inferred, signature.declaration?.type);
    if (!returnType)
        return;
    const projection = { parameters, returnType, typeParameters: signature.typeParameters.map(slot => slot.symbol.name),
        definition: definition(signature.declaration) };
    const arrayInputs = parameters.filter(parameter => parameter.type.kind === 'array' && parameter.type.readOnly);
    if (arrayInputs.length !== 1)
        return [projection];
    // A managed array and an already projected host array both erase to the original JS array.
    const [array] = arrayInputs;
    return [projection, { ...projection, parameters: parameters.map(parameter => parameter === array
        ? { ...parameter, type: { kind: 'recordArray', elementType: array.type.elementType } } : parameter) }];
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

function mayMatchWithOmittedArguments(signature, parameters) {
    // A missing middle parameter is an explicit undefined in the JS call.
    // A missing suffix shortens its argument list.
    for (let length = signature.minArgumentCount; length <= parameters.length; length++) {
        if (length > signature.parameters.length && !checker.hasEffectiveRestParameter(signature) ||
            parameters.slice(length).some(parameter => !parameter.optional))
            continue;
        if (parameters.slice(0, length).every(({ type, optional }, index) => {
            if (checker.hasEffectiveRestParameter(signature) && index >= signature.parameters.length - 1)
                return true;
            const target = typeOf(signature.parameters[index]);
            return mayAccept(target, type) || optional && checker.isTypeAssignableTo(checker.getUndefinedType(), target);
        }))
            return true;
    }
    return false;
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
        return projected.kind === 'typeParameter' || primitive === projected.kind;
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

function readGenericValidator({ signature, declaration, optionalValue, optionalCallback, omittedRules, managedNameSuffix }, unsupported) {
    const [generic] = signature.typeParameters;
    validatorTypeParameter = generic;
    optionalValidatorValue = optionalValue;
    try {
        const parameters = signature.parameters.map((parameter, index) => ({
            name: parameter.name, type: readType(typeOf(parameter), new Set(), 'input', true, unsupported), optional: false,
            ...(optionalCallback && index === 1 ? { optionalCallback: true } : {}),
        }));
        if (omittedRules)
            parameters.splice(1, 0, { name: 'rules', type: { kind: 'string' }, optional: false, fixedUndefined: true });
        const returnType = readReturn(signature, new Set(), 'output', true, unsupported);
        return { parameters, returnType, typeParameters: [generic.symbol.name], managedNameSuffix, definition: definition(declaration) };
    } finally {
        validatorTypeParameter = undefined;
        optionalValidatorValue = undefined;
    }
}

function readReturn(signature, ancestors, position, partial, unsupported) {
    const result = signature.getReturnType();
    const protocol = position === 'output' && readProtocolAlias(result, signature.declaration?.type);
    if (protocol) {
        const projection = attempt(() => readType(result, ancestors, position, true, unsupported)).value;
        if (projection?.kind === 'enrichedRef' && projection.typeIdentity)
            return projection;
        return projection?.members?.length ? { ...protocol, members: projection.members } : protocol;
    }
    return result.flags & ts.TypeFlags.Void ? { kind: 'void' } : readType(result, ancestors, position, partial,
        (detail, nested, node) => unsupported(detail, 'return type' + (nested ? ` → ${nested}` : ''),
            node ?? signature.declaration?.type ?? signature.declaration));
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
        const memberType = checker.getTypeOfPropertyOfType(options, property.name);
        if (!memberType)
            unsupported(`callback option '${property.name}' has no public property type`);
        const original = omitUndefined(memberType);
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
        if (branches.length === 2 && branches.some(branch => ['dictionary', 'inputDictionary'].includes(branch?.kind)) &&
            branches.some(branch => ['boolean', 'number', 'string'].includes(branch?.kind)))
            return { kind: 'union', variants: branches };
    }
    throw whole.error;
}

function readScalarCalls(signatures) {
    // A string key fixes K independently of the remaining argument domains.
    const projected = new Map();
    for (const signature of signatures) {
        if (!signature.parameters.length || !signature.minArgumentCount ||
            signature.typeParameters?.slice(1).some(parameter => !parameter.getDefault()) ||
            signature.thisParameter || checker.hasEffectiveRestParameter(signature))
            continue;
        const generic = signature.typeParameters?.[0];
        // An intermediate type parameter can default to a narrower literal.
        // Only a direct scalar constraint admits the whole C# input domain.
        const constraint = generic?.getConstraint();
        const types = signature.parameters.map((parameter, index) =>
            index >= signature.minArgumentCount ? omitUndefined(typeOf(parameter)) : typeOf(parameter));
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
        if (!output)
            continue;
        // Defaults for uninferred schema keys and an omitted locale can select a
        // different TS overload. Check overlap separately for each callable prefix.
        for (let count = signature.minArgumentCount; count <= inputs.length; count++) {
            const supplied = inputs.slice(0, count);
            if (supplied.some(input => !input))
                break;
            const parameters = supplied.map((type, index) => ({
                name: signature.parameters[index].name, type, optional: false,
            }));
            if (signatures.some(other => other !== signature && mayMatch(other, parameters) &&
                (other.thisParameter || scalar(other.getReturnType(), true) !== output)))
                continue;
            const key = shapeKey(supplied);
            if (!projected.has(key))
                projected.set(key, { parameters, returnType: { kind: output }, definition: definition(signature.declaration) });
        }
    }
    return [...projected.values()];
}

function readInputBranches(type, ancestors, partial, unsupported, splitReactive = false, methodParameters) {
    // For an exact value/ref/getter union, each emitted overload passes the
    // original argument through. Keep the narrower member projection inside
    // option records where C# cannot spell a lambda-to-union conversion.
    const whole = attempt(() => readType(type, ancestors, 'input', partial, unsupported, methodParameters));
    if (whole.value && (!splitReactive || !type.isUnion()) && !vueInputValue(type))
        return [whole.value];
    if (!type.isUnion())
        throw whole.error;
    const branches = type.types
        .map(branch => attempt(() => readType(branch, ancestors, 'input', partial, unsupported, methodParameters)).value)
        .filter(Boolean);
    // TypeScript flattens boolean into literal arms within a reactive union.
    // Preserve the accepted whole boolean when neither literal can be emitted alone.
    if (whole.value?.kind === 'boolean' && !branches.some(branch => branch.kind === 'boolean'))
        branches.unshift(whole.value);
    if (!whole.value && branches.length && type.types.every(branch =>
        (branch.flags & ts.TypeFlags.Object || branch.isIntersection()) &&
        !branch.getCallSignatures().length && !vueRef(branch)))
        // C# can construct one options branch without combining mutually exclusive keys.
        return [branches.find(branch => branch.members?.length) ?? branches[0]];
    if (!branches.length)
        throw whole.error;
    // VueWritableComputedRef<T> already implements IVueRef<T>. A MaybeRef
    // union (or an equivalent alias) needs only one overload for that domain.
    const writableElements = new Set(branches.filter(branch => branch.kind === 'ref' &&
        !branch.name && !branch.readOnly && !branch.writeType).map(branch => shapeKey(branch.elementType)));
    const distinct = branches.filter(branch => branch.kind !== 'ref' || branch.name !== 'VueWritableComputedRef' ||
        branch.readOnly || branch.writeType || !writableElements.has(shapeKey(branch.elementType)));
    return [...new Map(distinct.map(branch => [shapeKey(branch), branch])).values()];
}

function preferredInput(branches) {
    return branches.find(branch => branch.kind === 'date') ?? branches[0];
}

function readDirectVueRef(type, position, readElement, canonicalOnly = false) {
    // Direct signature discovery runs before callback-specific union projection is initialized.
    const ref = !type.isUnion() && vueRef(type);
    if (!ref || type.getCallSignatures().length || type.getConstructSignatures().length ||
        checker.getIndexInfosOfType(type).length)
        return;
    const computed = ref.name === 'VueComputedRef';
    if (canonicalOnly && !ref.canonical && !computed)
        return;
    const unknown = position === 'output' && readUnknownRefOutput(ref);
    if (unknown)
        return unknown;
    if (computed && (!ref.readOnly || checker.getTypeArguments(type)[0] !== ref.element ||
        type.getProperties().some(member => !optional(member) && member.declarations?.some(node =>
            node.getSourceFile() !== type.target.getSymbol().declarations[0].getSourceFile()))))
        return;
    const asymmetric = readAsymmetricVueRef(ref, position, readElement);
    if (asymmetric)
        return asymmetric;
    const writableComputed = ref.canonical && ref.name === 'VueWritableComputedRef' &&
        !ref.readOnly && ref.write === ref.element;
    if (!computed && !writableComputed && (!ref.plain || ref.name || !ref.readOnly && ref.write !== ref.element))
        return;
    // A writable ref exposes the same element to both caller and callee.
    const readOnly = computed || ref.readOnly;
    const elementType = readElement(ref.element, readOnly ? position : 'invariant');
    if (elementType)
        return { kind: 'ref', readOnly, name: computed || writableComputed ? ref.name : undefined, elementType };
}

function readRefStringDomain(type, absence = 0) {
    if (!type.isUnion())
        return;
    const choices = type.types.filter(branch => !(branch.flags & absence));
    if (choices.length < 2 || !choices.every(branch => branch.flags === ts.TypeFlags.StringLiteral))
        return;
    const stringLiterals = choices.map(branch => branch.value).sort();
    const names = stringLiterals.map(value => value.replace(/[^a-zA-Z0-9]+/g, ' ').split(' ')
        .filter(Boolean).map(part => part[0].toUpperCase() + part.slice(1)).join(''));
    const declaration = type.aliasSymbol?.declarations?.[0];
    const name = type.aliasSymbol?.name ?? `StringChoiceOf${names.join('Or')}`;
    const identity = declaration && recordIdentity(declaration);
    // A closed generic alias has one source declaration but can yield several
    // different writable domains. A ref may only share an enum for the same literals.
    const typeIdentity = identity && declaration.typeParameters?.length
        ? JSON.stringify([identity, stringLiterals]) : identity;
    return { kind: 'stringEnum', name, stringLiterals, typeDefinition: definition(declaration), typeIdentity };
}

function readNullishRefStringDomain(type, ref) {
    const mapped = type.aliasSymbol;
    const source = mapped?.name === 'Readonly' && mapped.declarations?.every(declaration =>
        program.isSourceFileDefaultLibrary(declaration.getSourceFile())) && type.aliasTypeArguments?.length === 1
        ? vueRef(type.aliasTypeArguments[0]) : undefined;
    const exact = ref.write === ref.element && (ref.canonical || ref.name === 'VueShallowRef');
    if (!exact && !(ref.readOnly && source?.name === 'VueShallowRef' &&
        source.element === ref.element && source.write === source.element))
        return;
    const branches = alternatives(ref.element);
    const hasUndefined = branches.some(branch => branch.flags & ts.TypeFlags.Undefined);
    const hasNull = branches.some(branch => branch.flags & ts.TypeFlags.Null);
    if (!(hasUndefined || hasNull) || hasUndefined && !ref.readOnly)
        return;
    const elementType = readRefStringDomain(ref.element, ts.TypeFlags.Null | ts.TypeFlags.Undefined);
    return elementType && { kind: hasUndefined ? 'undefinedable' : 'nullable', elementType };
}

function readAugmentedVueRef(type, readArgument, readMember) {
    const marked = !type.isUnion() && vueRef(type);
    if (!marked || marked.canonical || marked.name)
        return;
    const target = type.target?.getSymbol() ?? type.getSymbol();
    const isInterface = Boolean(target?.flags & ts.SymbolFlags.Interface);
    let source = isInterface ? target : type.aliasSymbol;
    let actualTypes = isInterface ? checker.getTypeArguments(type) : type.aliasTypeArguments ?? [];
    // A closed alias of an instantiated intersection preserves the alias
    // spelling. Retrieve its referenced declaration and actual arguments.
    const visitedAliases = new Set();
    while (source?.declarations?.length === 1 && !visitedAliases.has(source) && ts.isTypeAliasDeclaration(source.declarations[0]) &&
        !source.declarations[0].typeParameters?.length && ts.isTypeReferenceNode(source.declarations[0].type)) {
        visitedAliases.add(source);
        const reference = source.declarations[0].type;
        const symbol = checker.getSymbolAtLocation(reference.typeName);
        const referenced = symbol && unalias(symbol);
        if (!referenced || referenced === source)
            break;
        source = referenced;
        actualTypes = reference.typeArguments?.map(argument => checker.getTypeFromTypeNode(argument)) ?? [];
        if (actualTypes.length)
            break;
    }
    const [declaration] = source?.declarations ?? [];
    if (source?.declarations?.length !== 1 || activeAugmentedRefs.has(source) ||
        !(ts.isSourceFile(declaration.parent) || ts.isModuleBlock(declaration.parent)) ||
        !(ts.isInterfaceDeclaration(declaration) || ts.isTypeAliasDeclaration(declaration)) ||
        declaration.typeParameters?.some(parameter => parameter.constraint))
        return;
    const location = definition(declaration);
    if (!location)
        return;

    // Share the declaration's template across closed and open use sites. An
    // input must carry the same full required contract as a projected result.
    const template = checker.getDeclaredTypeOfSymbol(source);
    const base = augmentedRefBase(template);
    if (!base || template.getCallSignatures().length || template.getConstructSignatures().length ||
        checker.getIndexInfosOfType(template).length)
        return;
    const formalTypes = (declaration.typeParameters ?? []).map(parameter => checker.getTypeAtLocation(parameter.name));
    const formals = new Set(formalTypes);
    if (actualTypes.length !== formalTypes.length)
        return;

    activeAugmentedRefs.add(source);
    try {
        const argumentsList = actualTypes.map(readArgument);
        if (argumentsList.some(argument => !argument))
            return;
        const read = (member, position = 'invariant', permitsVoid = false) =>
            readRefStringDomain(member) ?? readMember(member, position, permitsVoid, formals);
        const ref = readDirectVueRef(base, 'invariant', read, true);
        if (!ref || ref.name)
            return;
        const baseNames = new Set(base.getProperties().map(member => member.name));
        const members = [];
        for (const member of template.getProperties()) {
            if (baseNames.has(member.name))
                continue;
            const [node] = member.declarations ?? [];
            if (!node || !node.name || !ts.isIdentifier(node.name) && !ts.isStringLiteral(node.name)) {
                if (optional(member))
                    continue;
                return;
            }
            const memberType = typeOf(member);
            const isOptional = optional(member);
            const readOnly = isOptional || Boolean(ts.getCheckFlags(member) & ts.CheckFlags.Readonly) ||
                member.declarations.some(entry => ts.getCombinedModifierFlags(entry) & ts.ModifierFlags.Readonly);
            if (member.declarations.every(ts.isMethodSignature) && !isOptional) {
                const calls = memberType.getCallSignatures();
                const [call] = calls;
                if (calls.length !== 1 || call.typeParameters?.length || call.thisParameter ||
                    checker.hasEffectiveRestParameter(call) || checker.getTypePredicateOfSignature(call) ||
                    call.minArgumentCount !== call.parameters.length)
                    return;
                const parameters = call.parameters.map(parameter => ({
                    name: parameter.name, type: read(typeOf(parameter), 'input'), optional: false,
                }));
                const returnType = read(call.getReturnType(), 'output', true);
                if (!returnType || parameters.some(parameter => !parameter.type))
                    return;
                members.push(methodMember(member.name, [{ parameters, returnType, definition: definition(node) }]));
                continue;
            }
            const payload = isOptional ? omitUndefined(memberType) : memberType;
            const propertyType = member.declarations.every(ts.isPropertySignature) &&
                (!isOptional || !formals.size) && read(payload, readOnly ? 'output' : 'invariant');
            if (!propertyType) {
                if (isOptional)
                    continue;
                return;
            }
            members.push({ name: member.name, type: propertyType, optional: isOptional,
                readOnly, definition: definition(node) });
        }
        if (!members.length)
            return;
        return { ...ref, kind: 'enrichedRef', name: source.name, typeDefinition: location, typeIdentity: recordIdentity(declaration),
            ...(formals.size && { typeParameters: formalTypes.map(slot => slot.symbol.name), typeArguments: argumentsList }), members };
    } finally {
        activeAugmentedRefs.delete(source);
    }
}

function augmentedRefBase(template) {
    const branches = template.isIntersection() ? template.types : template.getBaseTypes() ?? [];
    const bases = branches.filter(branch => {
        const ref = vueRef(branch);
        return ref?.canonical && !ref.name;
    });
    if (bases.length !== 1)
        return;
    const [base] = bases;
    // A same-named property from another branch can change the Vue setter or
    // brand. Verify each inherited symbol still has the original declarations.
    const properties = new Map(template.getProperties().map(member => [member.name, member]));
    if (base.getProperties().some(member => {
        const declarations = properties.get(member.name)?.declarations;
        return declarations?.length !== member.declarations?.length ||
            !member.declarations?.every(node => declarations?.includes(node));
    }))
        return;
    return base;
}

function readEnrichedVueRef(type, readRef, readMembers, partial, unsupported) {
    if (!type.isIntersection() || type.getCallSignatures().length || type.getConstructSignatures().length ||
        checker.getIndexInfosOfType(type).length)
        return;
    const bases = type.types.filter(branch => vueRef(branch)?.canonical);
    if (bases.length !== 1)
        return;
    const [base] = bases;
    const baseNames = new Set(base.getProperties().map(member => member.name));
    // Preserve the declared getter and setter only if no other intersection
    // constituent overrides either of them or any part of the Vue identity.
    if (type.types.some(branch => branch !== base && branch.getProperties().some(member => baseNames.has(member.name))))
        return;
    // Vue's optional shallow marker is an internal symbol, not an exposed
    // property. Leave an otherwise plain shallow ref on its existing path.
    const additions = type.getProperties().filter(member => !baseNames.has(member.name) &&
        !(optional(member) && member.declarations?.every(node => ts.isComputedPropertyName(node.name))));
    if (!additions.length)
        return;
    const projected = readRef(base);
    if (projected?.kind !== 'ref' || ['VueOptionalRef', 'VueShallowRef'].includes(projected.name))
        return;
    const members = readMembers(additions, partial, unsupported);
    if (!members.length)
        return;
    return { ...projected, kind: 'enrichedRef', members, typeDefinition: definition(type.aliasSymbol?.declarations?.[0]) };
}

function readAsymmetricVueRef(ref, position, read) {
    if (!ref.canonical || ref.readOnly || !ref.write || ref.write === ref.element)
        return;
    const branches = ref.write.isUnion() ? ref.write.types : [ref.write];
    const admitsUndefined = branches.some(branch => branch.flags & ts.TypeFlags.Undefined);
    const admitsNull = branches.some(branch => branch.flags & ts.TypeFlags.Null);
    const values = branches.filter(branch => !(branch.flags & (ts.TypeFlags.Null | ts.TypeFlags.Undefined)));
    if (!values.length)
        return;
    const value = admitsNull || admitsUndefined ? omitUndefined(checker.getUnionType(values)) : ref.write;
    // Keep the payload carriers identical across input and output uses.
    // A narrowed read/write shape is unsafe if the projected ref travels back.
    const elementType = read(ref.element, 'invariant');
    if (admitsUndefined) {
        // In a removable ref, writing null can make a subsequent read nullish
        // even when the authored getter exposes only the matching payload.
        if (position !== 'output' || ref.name || !admitsNull || value !== ref.element || !elementType)
            return;
        const nullable = { kind: elementType.kind === 'typeParameter' ? 'nullableSlot' : 'nullable', elementType };
        return { kind: 'ref', name: 'VueOptionalRef', elementType: nullable, readOnly: false };
    }
    const payload = elementType && read(value, 'invariant');
    if (!payload)
        return;
    const writeType = admitsNull ? { kind: payload.kind === 'typeParameter' ? 'nullableSlot' : 'nullable', elementType: payload } : payload;
    return {
        kind: 'ref', name: ref.name, elementType, writeType,
        readOnly: !checker.isTypeAssignableTo(ref.element, admitsNull ? checker.getUnionType([...values, checker.getNullType()]) : value),
    };
}

function readUnknownRefOutput(ref) {
    if (ref?.canonical && !ref.name && ref.element.flags === ts.TypeFlags.Unknown)
        // Keep the original ref. Unknown values can only be consumed through checked reads.
        return { kind: 'ref', readOnly: true, elementType: {
            kind: 'undefinedable', elementType: { kind: 'unknownValue' },
        } };
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
        if (branches.every(Boolean)) {
            const elements = branches.map(branch => branch.element);
            const values = elements.flatMap(alternatives);
            if ([ts.TypeFlags.BooleanLiteral, ts.TypeFlags.StringLiteral].some(family =>
                values.every(value => value.flags === family)))
                return { element: checker.getUnionType(values), readOnly: true, plain: false };
            if (error && values.some(value => value.getSymbol() === error) &&
                values.every(value => value.getSymbol() === error || value.flags === ts.TypeFlags.Null))
                return { element: checker.getUnionType(values), readOnly: true, plain: false };
            if (projectingCallbackOptions && elements.some(element => callbackMarkers.has(element.getSymbol())) &&
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
    const isShallow = shallow && (type.aliasSymbol === shallow || type.isIntersection() &&
        type.getProperties().some(member => member.declarations?.some(node =>
            node.getSourceFile() === declaration.getSourceFile() &&
            ts.isComputedPropertyName(node.name) && node.name.expression.getText() === 'ShallowRefMarker')));
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
    const canonical = type.target?.getSymbol() === refSymbol && plain || isWritableComputed &&
        type.getProperties().every(member => optional(member) || member.declarations?.every(node =>
            node.getSourceFile() === declaration.getSourceFile()));
    return { element, readOnly, write: read === element ? write : undefined, plain, name, canonical };
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
    if (position === 'output' && type.isIntersection() && type.types.length === 2) {
        const brandIndex = type.types.findIndex(branch => {
            const declaration = branch.getSymbol()?.declarations?.[0];
            return declaration && ts.isClassDeclaration(declaration) &&
                declaration.name?.text === 'ShallowReactiveBrandClass' &&
                declaration.getSourceFile().fileName.replaceAll('\\', '/').includes('/node_modules/@vue/reactivity/');
        });
        // The private Vue brand has no runtime property. The shallow proxy
        // still exposes the original payload's member types, including refs.
        if (brandIndex >= 0)
            return readType(type.types[1 - brandIndex], ancestors, position, partial, unsupported, methodParameters);
    }
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
    if (optionalValidatorValue && type.isUnion() && omitUndefined(type) === validatorTypeParameter)
        return { kind: 'nullableSlot', elementType: { kind: 'typeParameter', name: validatorTypeParameter.symbol.name } };
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
    // The host binding exposes JavaScript Error members on observer reads.
    // A structurally compatible package type can have a different contract.
    if (position === 'output' && error && type.getSymbol() === error)
        return { kind: 'error' };
    const webType = webTypes(type);
    if (webType)
        return webType;
    const callableView = callableRecordView(type, position);
    if (callableView)
        return callableView;
    if (!ancestors.has(type)) {
        const augmented = readAugmentedVueRef(type,
            argument => attempt(() => readType(argument, ancestors, 'invariant', false, unsupported)).value,
            (member, usePosition, permitsVoid, formals) =>
                permitsVoid && member.flags & ts.TypeFlags.Void ? { kind: 'void' } :
                    attempt(() => readType(member, formals, usePosition, false, unsupported)).value);
        if (augmented)
            return augmented;
    }
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
        if (position === 'output') {
            const enriched = readEnrichedVueRef(type,
                base => attempt(() => readType(base, nested, position, partial, unsupported)).value,
                members => readObjectMembers(type, members, nested, position, partial, unsupported), partial, unsupported);
            if (enriched)
                return enriched;
        }
    }
    if (type.isUnion()) {
        const indexedUnion = readClosedStringArrayMapUnion(type);
        if (indexedUnion)
            return indexedUnion;
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
        const unknown = position === 'output' && readUnknownRefOutput(ref);
        if (unknown)
            return unknown;
        const asymmetric = readAsymmetricVueRef(ref, position,
            (element, usePosition) => attempt(() => readType(element, nested, usePosition, false, unsupported)).value);
        if (asymmetric)
            return asymmetric;
        const exactWrite = ref.write === ref.element;
        // Ordinary refs do not satisfy Vue's computed identity or setter contract.
        const writableComputed = ref.canonical && ref.name === 'VueWritableComputedRef' && !ref.readOnly && exactWrite;
        if (position !== 'output' && !writableComputed && (!ref.plain || !ref.readOnly && !exactWrite))
            unsupported(`Vue ref '${display(type)}'`);
        // A returned ref may accept more on write than it returns on read.
        // C# still only offers the read domain for writes on that ref.
        const readOnly = ref.readOnly || !exactWrite &&
            (!ref.write || !checker.isTypeAssignableTo(ref.element, ref.write));
        const exactDomain = position === 'output' && readNullishRefStringDomain(type, ref) ||
            exactWrite && (ref.canonical || ref.name === 'VueComputedRef') && readRefStringDomain(ref.element);
        const readDomain = readOnly && position === 'output' && readObserverRef(type)?.elementType;
        const element = exactDomain || readDomain ? { value: exactDomain || readDomain } :
            attempt(() => readType(ref.element, nested, readOnly ? position : 'invariant', false, unsupported));
        if (!element.value) {
            const mapped = ref.element.declaration;
            let alias = mapped?.parent;
            while (alias && ts.isConditionalTypeNode(alias))
                alias = alias.parent;
            const vueMappedRecord = alias && ts.isTypeAliasDeclaration(alias) && alias.name.text === 'UnwrapRefSimple' &&
                alias.getSourceFile() === type.target?.getSymbol()?.declarations?.[0]?.getSourceFile();
            // The instantiated homomorphic mapping retains the original DTO in modifiersType.
            const original = ref.canonical && (vueUnwrapArgument(type, ref.element) ||
                vueMappedRecord && ref.element.modifiersType);
            const unwrappedRecord = original || ref.canonical && vueMappedRecord;
            if (position === 'output' && !readOnly && (ref.name === 'VueShallowRef' && exactWrite || unwrappedRecord)) {
                const view = attempt(() => readType(ref.element, nested, position, false, unsupported)).value;
                if (view?.kind === 'object') {
                    // Vue's setter accepts either this exact unwrapped view or its original DTO.
                    // Keep the C# write carrier separate and require the complete input shape.
                    if (original && original !== ref.element && ref.write?.isUnion() && ref.write.types.length === 2 &&
                        ref.write.types.includes(original) && ref.write.types.includes(ref.element)) {
                        const writeType = attempt(() => readType(original, nested, 'input', false, unsupported)).value;
                        if (writeType?.kind === 'object' || writeType?.kind === 'namedRecord')
                            return { kind: 'ref', readOnly: false, elementType: view, writeType };
                    }
                    return { kind: 'ref', readOnly: false, name: ref.name, elementType: view };
                }
            }
            if (position === 'output')
                return { kind: 'ref', readOnly: true, elementType: readType(ref.element, nested, position, partial, unsupported) };
            throw element.error;
        }
        return {
            kind: 'ref', readOnly,
            name: writableComputed || position === 'output' && (readOnly ? ref.name === 'VueComputedRef' :
                ref.name === 'VueShallowRef') ? ref.name : undefined,
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
            return { kind: 'inputDictionary', elementType };
        }
        // An open index reads sparsely, independently of the declared write domain.
        if (position === 'output') {
            const defined = omitUndefined(indexes[0].type);
            const kind = scalar(defined);
            const elementType = kind ? { kind } : readScalarUnion(defined, true);
            if (elementType)
                return { kind: indexes[0].isReadonly || defined !== indexes[0].type
                    ? 'readonlyDictionary' : 'writableDictionary', elementType };
        }
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
    const members = readObjectMembers(type, properties, nested, position, partial, unsupported, methodParameters);
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

function readClosedStringArrayMapUnion(type) {
    if (type.types.length !== 4 || !type.types.includes(checker.getStringType()) ||
        !type.types.includes(checker.getFalseType()))
        return;
    const mutableStringArray = value => checker.isArrayType(value) &&
        value.target?.getSymbol()?.name === 'Array' && checker.getTypeArguments(value)[0] === checker.getStringType();
    if (!type.types.some(mutableStringArray))
        return;
    const map = type.types.find(branch => {
        if (!(branch.flags & ts.TypeFlags.Object) || branch.getProperties().length ||
            branch.getCallSignatures().length || branch.getConstructSignatures().length)
            return false;
        const indexes = checker.getIndexInfosOfType(branch);
        return indexes.length === 1 && indexes[0].keyType === checker.getStringType() &&
            !indexes[0].isReadonly && mutableStringArray(indexes[0].type);
    });
    if (map) {
        const alias = type.aliasSymbol;
        const declaration = alias?.declarations?.find(ts.isTypeAliasDeclaration);
        return { kind: 'stringArrayMapUnion', name: declaration && alias.name,
            typeDefinition: definition(declaration), typeIdentity: declaration && recordIdentity(declaration) };
    }
}

// Keep a value and its lazy async supplier as separate C# overloads. A lambda
// needs a delegate target, and the package controls when (or whether) it runs.
function readAsyncSupplierMethod(type, ancestors, partial, unsupported) {
    const [call] = singleCall(type);
    if (!standardPromise || !call || call.parameters.length !== 1 || call.typeParameters?.length ||
        call.thisParameter || checker.hasEffectiveRestParameter(call) || type.getProperties().length)
        return;
    const [parameter] = call.parameters;
    const choices = omitUndefined(typeOf(parameter));
    if (!choices.isUnion())
        return;
    const supplier = choices.types.find(branch => branch.getCallSignatures().length);
    const [getter] = supplier ? singleCall(supplier) : [];
    if (!getter || getter.parameters.length || getter.typeParameters?.length || getter.thisParameter ||
        supplier.getProperties().length || supplier.getConstructSignatures().length || checker.getIndexInfosOfType(supplier).length)
        return;
    const promised = getter.getReturnType();
    if (promised.target?.getSymbol() !== promise)
        return;
    const eager = checker.getUnionType(choices.types.filter(branch => branch !== supplier));
    const kind = scalar(eager);
    const [payload] = checker.getTypeArguments(promised);
    if (!kind || omitUndefined(payload) !== eager)
        return;

    const element = { kind };
    const lazy = { kind: 'function', signatures: [{ parameters: [], returnType: { kind: 'promise', elementType: element } }] };
    const returnType = readReturn(call, ancestors, 'output', partial, unsupported);
    const parameters = [element, lazy].map(type => [{ name: parameter.name, optional: false, type }]);
    if (call.minArgumentCount === 0)
        parameters.unshift([]);
    return { kind: 'method', signatures: parameters.map(parameters => ({
        parameters, returnType, definition: definition(call.declaration),
    })) };
}

function readStaticMessageParameters(messages) {
    if (!localeToolManifest)
        throw needsLocaleTool;
    staticMessageReader ??= createMessageReader(localeToolManifest);
    return staticMessageReader(messages);
}

function composerExport(type) {
    const candidate = type.target?.getSymbol();
    if (candidate?.name !== 'Composer')
        return;
    for (const declaration of candidate.declarations ?? []) {
        const file = declaration.getSourceFile();
        const resolution = ts.resolveModuleName('vue-i18n', file.fileName, options, host).resolvedModule;
        if (resolution?.packageId?.name !== 'vue-i18n')
            continue;
        const module = program.getSourceFile(resolution.resolvedFileName);
        const symbol = module && checker.getSymbolAtLocation(module);
        if (symbol && checker.getExportsOfModule(symbol).some(exported =>
            exported.name === 'Composer' && unalias(exported) === candidate))
            return candidate;
    }
}

function readObjectMembers(type, properties, nested, position, partial, unsupported, methodParameters) {
    const members = [];
    const localeMaps = position === 'output' ? localeMessageMaps(ts, checker, properties, definition) : new Set();
    const composer = position === 'output' ? readStaticLocaleResources(ts, checker, type,
        composerExport(type), vueRef, definition, recordIdentity, readStaticMessageParameters,
        (calls, symbol, name, resources) => readStaticLocaleWriter(ts, checker, calls, symbol, name, resources)) : undefined;
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
        if (position === 'output' && optionalValidatorValue?.includes(member.name) &&
            vueRef(actualType)?.element === validatorTypeParameter) {
            // VeeValidate fields without initial values read undefined. SetValue
            // still exposes the declared payload type on the write side.
            const slot = { kind: 'typeParameter', name: validatorTypeParameter.symbol.name };
            members.push({ name: member.name, readOnly: true, definition: definition(declarations[0]),
                type: { kind: 'ref', name: 'VueReadonlyOptionalRef', readOnly: true,
                    elementType: { kind: 'nullableSlot', elementType: slot } } });
            continue;
        }
        if (position === 'output' && validatorTypeParameter && !isOptional && actualType.isUnion() &&
            actualType.types.includes(checker.getStringType()) &&
            actualType.types.every(branch => branch === checker.getStringType() ||
                vueRef(branch)?.element === checker.getStringType())) {
            // Preserve both declared branches while exposing only the ref read.
            members.push({ name: member.name, readOnly: true, definition: definition(declarations[0]),
                type: { kind: 'union', variants: [{ kind: 'string' },
                    { kind: 'ref', readOnly: true, elementType: { kind: 'string' } }] } });
            continue;
        }
        // The nullable getter normalizes absence to null. Keep a setter only
        // if that same null is legal on the TypeScript write side.
        if (position === 'output' && isOptional &&
            !(actualType.flags & ts.TypeFlags.Null || actualType.isUnion() && actualType.types.some(type => type.flags & ts.TypeFlags.Null)))
            readOnly = true;
        const memberPosition = position === 'input' || readOnly ? position : 'invariant';
        // A property initializer has a target type. A closed string domain can
        // therefore use the existing Jazor string-enum lowering without widening writes.
        let projected;
        if (position === 'output' && !isOptional) {
            // The broad locale writers can replace required resources with
            // arbitrary records. Typed Composer reads rely on those leaves.
            if (composer && (member.name === 'setLocaleMessage' || member.name === 'mergeLocaleMessage'))
                continue;
            if (readOnly && composer?.messages && member.name === 'messages') {
                members.push({ name: member.name, type: composer.messages, readOnly: true,
                    definition: definition(declarations[0]) });
                continue;
            }
            if (readOnly && declarations.some(declaration => localeMaps.has(declaration))) {
                const messages = vueRef(actualType)?.name === 'VueComputedRef' && readDirectVueRef(actualType, position, element => {
                    const indexes = checker.getIndexInfosOfType(element);
                    if (element.objectFlags & ts.ObjectFlags.Mapped &&
                        indexes.every(index => index.keyType === checker.getStringType() && index.type.flags & ts.TypeFlags.Object) &&
                        element.getProperties().every(property => typeOf(property).flags & ts.TypeFlags.Object))
                        return { kind: 'readonlyDictionary', elementType: { kind: 'plainRecord' } };
                });
                if (messages) {
                    members.push({ name: member.name, type: messages, readOnly: true, definition: definition(declarations[0]) });
                    continue;
                }
            }
            const localeMessage = declarations.every(ts.isMethodSignature) &&
                readLocaleMessageMethod(ts, checker, actualType.getCallSignatures(), definition);
            const dynamicMessage = readDynamicLocaleMessage(ts, checker, actualType.getCallSignatures(), definition);
            if (localeMessage || dynamicMessage) {
                members.push(methodMember(member.name, [localeMessage || dynamicMessage]));
                continue;
            }
            const stringMessage = readStringMessageCall(ts, checker, actualType.getCallSignatures(), definition);
            if (stringMessage) {
                members.push(methodMember(member.name, [stringMessage]));
                continue;
            }
            const method = attempt(() => readAsyncSupplierMethod(actualType, nested, partial, unsupported)).value;
            if (method) {
                members.push({ name: member.name, type: method, readOnly: true, definition: definition(declarations[0]) });
                continue;
            }
        }
        const numbers = position === 'input' && actualType.isUnion() && finiteNumberLiterals(ts, actualType.types);
        if (position === 'input' && actualType.isUnion() &&
            actualType.types.every(branch => branch.flags === ts.TypeFlags.StringLiteral)) {
            const annotation = declarations.find(node => node.type && ts.isTypeReferenceNode(node.type))?.type;
            const name = annotation && !annotation.typeArguments?.length ?
                (ts.isQualifiedName(annotation.typeName) ? annotation.typeName.right : annotation.typeName).text : member.name;
            projected = { value: { kind: 'stringEnum', name, stringLiterals: actualType.types.map(branch => branch.value).sort() } };
        } else if (numbers) {
            // Preserve each literal in the C# input domain and emit its actual JS number.
            const annotation = declarations.find(node => node.type && ts.isTypeReferenceNode(node.type))?.type;
            const name = actualType.aliasSymbol?.name ?? (annotation && !annotation.typeArguments?.length ?
                (ts.isQualifiedName(annotation.typeName) ? annotation.typeName.right : annotation.typeName).text : member.name);
            const domain = namedNumberDomain(ts, checker, actualType, annotation, numbers, definition, recordIdentity);
            projected = { value: { kind: 'numberLiterals', name,
                ...domain, numberLiterals: numbers } };
        } else {
            projected = attempt(() => readType(actualType, nested, memberPosition, partial, unsupported));
        }
        if (!projected.value && position === 'input' && partial)
            projected.value = readMethodOption(actualType, methodParameters ?? [], nested, unsupported);
        if (!projected.value && position === 'input' && partial && actualType.isUnion()) {
            // An input initializer can supply supported scalar arms while
            // retaining the original field's requiredness.
            const scalarArms = actualType.types.filter(branch => scalar(branch));
            const boolean = checker.getBooleanType();
            if (checker.isTypeAssignableTo(boolean, actualType) && !scalarArms.includes(boolean))
                scalarArms.push(boolean);
            if (scalarArms.length === 1)
                projected.value = { kind: scalar(scalarArms[0]) };
            else if (scalarArms.length > 1)
                projected.value = readScalarUnion(checker.getUnionType(scalarArms));
        }
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
    if (composer)
        members.push(...composer.methods);
    return members;
}
