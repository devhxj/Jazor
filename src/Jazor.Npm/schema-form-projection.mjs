import { readCommonFormMembers } from './common-form-members.mjs';
import { alternatives, singleCall } from './generic-callbacks.mjs';
import { dependentPathSetter, dependentPathTuple, supportsBracketPaths } from './dependent-path.mjs';
import { readPathTupleOptions } from './path-tuple-options.mjs';

export function readIndexedArrayContext(ts, checker, typeOf, vueRef, definition, signature) {
    const [slot] = signature.typeParameters ?? [];
    if (signature.typeParameters?.length !== 1 || !(slot.getDefault()?.flags & ts.TypeFlags.Unknown) ||
        slot.symbol.declarations?.some(declaration => declaration.constraint) ||
        signature.parameters.length !== 1 || signature.minArgumentCount !== 1 ||
        signature.thisParameter || checker.hasEffectiveRestParameter(signature))
        return;
    const string = checker.getStringType();
    // The original declaration must accept a direct string independently of T.
    // The caller uses this arm even if another input branch depends on T.
    if (!alternatives(typeOf(signature.parameters[0])).includes(string))
        return;
    const payload = { kind: 'typeParameter', name: slot.symbol.name };
    const read = type => type === slot ? payload :
        type === checker.getNumberType() ? { kind: 'number' } :
        type === checker.getStringType() ? { kind: 'string' } :
        type === checker.getBooleanType() ? { kind: 'boolean' } : undefined;
    const mutableElement = type => checker.isArrayType(type) &&
        checker.getPropertyOfType(type, 'push') && checker.getElementTypeOfArrayType(type);
    const memberType = (type, member) => checker.getTypeOfPropertyOfType(type, member.name);
    const properties = type => type.flags & ts.TypeFlags.Object && !type.getCallSignatures().length &&
        !type.getConstructSignatures().length && !checker.getIndexInfosOfType(type).length &&
        type.getProperties().every(member => !(member.flags & ts.SymbolFlags.Optional) &&
            member.declarations?.length === 1 && ts.isIdentifier(member.declarations[0].name))
        ? type.getProperties() : undefined;
    const member = (symbol, type, readOnly) => ({
        name: symbol.name, type, readOnly, definition: definition(symbol.declarations[0]),
    });

    const context = signature.getReturnType();
    const contextProperties = properties(context);
    if (!contextProperties)
        return;
    const refMembers = contextProperties.filter(symbol => vueRef(memberType(context, symbol))?.canonical);
    if (refMembers.length !== 1)
        return;
    const [entries] = refMembers;
    const ref = vueRef(memberType(context, entries));
    const item = !ref.name && ref.write === ref.element && mutableElement(ref.element);
    const entryProperties = item && properties(item);
    if (!entryProperties)
        return;
    const entry = [];
    for (const symbol of entryProperties) {
        const source = memberType(item, symbol);
        const branches = source.isUnion() && source.types.map(read);
        const type = read(source) ?? (branches && branches.length === 2 &&
            branches.every(branch => branch?.kind === 'string' || branch?.kind === 'number')
            ? { kind: 'union', variants: branches } : undefined);
        if (!type || !ts.isPropertySignature(symbol.declarations[0]))
            return;
        entry.push(member(symbol, type, type !== payload || Boolean(ts.getCombinedModifierFlags(symbol.declarations[0]) & ts.ModifierFlags.Readonly)));
    }
    if (entry.filter(property => property.type === payload).length !== 1)
        return;

    // Preserve the same slot in the entry and every mutation, including an
    // exact CLR array for replacement. The string path carries no inference.
    const members = [member(entries, { kind: 'ref', elementType: {
        kind: 'recordArray', elementType: { kind: 'object', members: entry },
    } }, true)];
    let slotInput = false;
    for (const symbol of contextProperties.filter(symbol => symbol !== entries)) {
        const [call] = singleCall(memberType(context, symbol));
        if (!call || call.typeParameters?.length || call.thisParameter || checker.hasEffectiveRestParameter(call) ||
            call.minArgumentCount !== call.parameters.length || !(call.getReturnType().flags & ts.TypeFlags.Void) ||
            !ts.isMethodSignature(symbol.declarations[0]))
            return;
        const parameters = [];
        for (const parameter of call.parameters) {
            const source = typeOf(parameter);
            const type = source === slot || source === checker.getNumberType() ? read(source) :
                mutableElement(source) === slot ? { kind: 'recordArray', elementType: payload } : undefined;
            if (!type)
                return;
            slotInput ||= type === payload || type.elementType === payload;
            parameters.push({ name: parameter.name, type, optional: false });
        }
        members.push(member(symbol, { kind: 'method', signatures: [{
            parameters, returnType: { kind: 'void' }, definition: definition(call.declaration),
        }] }, true));
    }
    if (slotInput)
        return { returnType: { kind: 'object', members }, typeParameters: [payload.name] };
}

export function inputClosureSlot(ts, checker, typeOf, signature) {
    if (signature.typeParameters?.length !== 1)
        return;
    const [slot] = signature.typeParameters;
    const [closure] = singleCall(signature.getReturnType());
    if (!closure || !closure.minArgumentCount || closure.typeParameters?.length || closure.thisParameter ||
        checker.hasEffectiveRestParameter(closure) || !(closure.getReturnType().flags & ts.TypeFlags.Void) ||
        typeOf(closure.parameters[0]) !== slot)
        return;
    const constraint = checker.getBaseConstraintOfType(slot);
    if (!constraint || constraint.flags & ts.TypeFlags.Unknown)
        return { slot, record: false };
    const [index, ...others] = checker.getIndexInfosOfType(constraint);
    // An explicit C# record provides the same string-keyed object contract.
    // Narrower value or field constraints require a more specific carrier.
    if (!constraint.getProperties().length && !constraint.getCallSignatures().length &&
        !constraint.getConstructSignatures().length && index && !others.length &&
        index.keyType === checker.getStringType() && index.type.flags & (ts.TypeFlags.Unknown | ts.TypeFlags.Any))
        return { slot, record: true };
}

export function schemaFormProbes(ts, program, checker, source, aliases, callsOf, selected, typeOf, packageOf, vueRef, definition) {
    const standardType = name => {
        const symbol = checker.resolveName(name, source, ts.SymbolFlags.Type, false);
        return symbol?.declarations?.every(node => program.isSourceFileDefaultLibrary(node.getSourceFile())) ? symbol : null;
    };
    const promise = standardType('Promise');
    const event = standardType('Event');
    if (!promise || !event)
        return [];

    const memberType = (type, member) => checker.getTypeOfPropertyOfType(type, member.name);
    const optional = member => Boolean(member.flags & ts.SymbolFlags.Optional);
    const matching = (type, predicate) => type.getProperties().filter(member => predicate(memberType(type, member), member));
    const distinct = matches => matches.length === 1 ? matches[0] : undefined;
    const callOf = type => type && singleCall(type)[0];
    const callOfOptional = type => callOf(checker.getNonNullableType(type));
    const promised = type => type.target?.getSymbol() === promise ? checker.getTypeArguments(type)[0] : undefined;
    const shape = (type, flag) => Boolean(type?.flags & flag);
    const exact = (type, parameter) => checker.getNonNullableType(type) === parameter;
    function acceptsFullRecord(type, slot, annotation) {
        // Maps from paths to errors or touched flags also mention the DTO.
        // Only an identity mapping over the DTO's own keys can supply its values.
        if (alternatives(type).some(branch => {
            if (branch === slot)
                return true;
            const mapped = branch.declaration;
            if (branch.aliasTypeArguments?.length !== 1 || branch.aliasTypeArguments[0] !== slot ||
                !mapped || !ts.isMappedTypeNode(mapped) || mapped.nameType)
                return false;
            const keys = mapped.typeParameter.constraint;
            const value = mapped.type;
            return keys && ts.isTypeOperatorNode(keys) && keys.operator === ts.SyntaxKind.KeyOfKeyword &&
                value && ts.isIndexedAccessTypeNode(value) && ts.isTypeReferenceNode(value.indexType) &&
                checker.getTypeAtLocation(keys.type) === checker.getTypeAtLocation(value.objectType) &&
                checker.getSymbolAtLocation(mapped.typeParameter.name) === checker.getSymbolAtLocation(value.indexType.typeName) &&
                checker.isTypeAssignableTo(slot, branch);
        }))
            return true;
        // TypeScript cannot reduce PartialDeep<T> while T is open. Resolve the
        // published utility itself, including imported aliases, before applying
        // its full-record input contract. An arbitrary conditional alias may narrow T.
        return annotation && (ts.isUnionTypeNode(annotation) ? annotation.types : [annotation]).some(node => {
            if (!ts.isTypeReferenceNode(node) || node.typeArguments?.length !== 1)
                return false;
            let symbol = checker.getSymbolAtLocation(node.typeName);
            if (symbol?.flags & ts.SymbolFlags.Alias)
                symbol = checker.getAliasedSymbol(symbol);
            if (symbol?.name !== 'PartialDeep' || !symbol.declarations?.some(declaration =>
                ts.isTypeAliasDeclaration(declaration) && packageOf(declaration) === 'type-fest'))
                return false;
            // The checker retains the same conditional declaration through
            // instantiation, even when it changes the display alias to _PartialDeep.
            const target = checker.getTypeAtLocation(node).root;
            return target && alternatives(type).some(branch =>
                branch.root === target && branch.aliasTypeArguments?.[0] === slot);
        });
    }
    const initialMember = (options, input) => distinct(matching(options, (type, property) =>
        acceptsFullRecord(type, input, property.declarations?.find(ts.isPropertySignature)?.type)));
    const callsWithOne = call => call && call.minArgumentCount === 1 && !call.typeParameters?.length &&
        !call.thisParameter && !checker.hasEffectiveRestParameter(call);

    function pathConstraint(signature) {
        const node = signature.declaration?.typeParameters?.[0]?.constraint;
        if (!node || !ts.isTypeReferenceNode(node) || node.typeArguments?.length !== 1)
            return;
        const [slot] = signature.typeParameters;
        // An inherited constraint may reduce to string for two different DTOs.
        // Keep the instantiated alias and its actual root slot for comparison.
        slot.getConstraint();
        return slot.constraint;
    }

    function readValidators(result, input, pathSetters) {
        const setter = pathSetters[0];
        const route = setter && pathConstraint(setter.signature);
        return result.getProperties().flatMap(member => {
            const validation = !optional(member) && callOf(memberType(result, member));
            const returned = validation && promised(validation.getReturnType());
            if (!returned || validation.thisParameter || checker.hasEffectiveRestParameter(validation))
                return [];
            const validationRoute = validation.typeParameters?.length === 1 && pathConstraint(validation);
            const field = Boolean(route?.aliasSymbol && route.aliasTypeArguments?.[0] === input &&
                validationRoute?.aliasSymbol === route.aliasSymbol && validationRoute.aliasTypeArguments?.[0] === input &&
                validation.minArgumentCount === 1 && typeOf(validation.parameters[0]) === validation.typeParameters[0]);
            if (!field && (validation.minArgumentCount || validation.typeParameters?.length))
                return [];
            const passed = distinct(matching(returned, (type, property) => !optional(property) && type === checker.getBooleanType()));
            const errors = field && distinct(matching(returned, (type, property) => !optional(property) &&
                checker.isArrayType(type) && checker.getElementTypeOfArrayType(type) === checker.getStringType()));
            const veeValidatePath = field && validation.declaration && packageOf(validation.declaration) === 'vee-validate';
            return passed && (!field || errors) ? [{ member, validation, passed, errors, field, veeValidatePath,
                arrayIndices: field && supportsBracketPaths(ts, checker, validation) }] : [];
        });
    }

    function admitsPlainRecord(parameter) {
        const constraint = checker.getBaseConstraintOfType(parameter);
        if (!constraint || constraint.flags & (ts.TypeFlags.Unknown | ts.TypeFlags.NonPrimitive))
            return true;
        // The C# boundary admits author-defined plain records. A required or
        // restricted property on an upstream constraint cannot be enforced by VueProps.
        return Boolean(constraint.flags & ts.TypeFlags.Object) && !constraint.getProperties().length &&
            !constraint.getCallSignatures().length && !constraint.getConstructSignatures().length &&
            checker.getIndexInfosOfType(constraint).every(index => index.keyType.flags & ts.TypeFlags.String &&
                index.type.flags & (ts.TypeFlags.Any | ts.TypeFlags.Unknown));
    }

    function readSchema(type, input, output) {
        const tag = distinct(matching(type, (value, member) => value.isStringLiteral() && !optional(member)));
        const parse = distinct(matching(type, (value, member) => {
            const call = callOf(value);
            return !optional(member) && callsWithOne(call) && typeOf(call.parameters[0]) === input && promised(call.getReturnType());
        }));
        if (!tag || !parse || type.getProperties().some(member => member !== tag && member !== parse && !optional(member)))
            return;
        const parseCall = callOf(memberType(type, parse));
        const result = promised(parseCall.getReturnType());
        const value = distinct(matching(result, value => exact(value, output)));
        const messages = distinct(matching(result, (value, member) => !optional(member) && checker.isArrayType(value)));
        if (!value || !messages || result.getProperties().some(member => member !== value && member !== messages && !optional(member)))
            return;
        const error = checker.getElementTypeOfArrayType(memberType(result, messages));
        const path = distinct(matching(error, (value, member) => optional(member) && shape(checker.getNonNullableType(value), ts.TypeFlags.String)));
        const texts = distinct(matching(error, (value, member) => !optional(member) && checker.isArrayType(value) &&
            shape(checker.getElementTypeOfArrayType(value), ts.TypeFlags.String)));
        if (!path || !texts || error.getProperties().some(member => member !== path && member !== texts && !optional(member)))
            return;
        return { type, tag, parse, parseCall, result, value, messages, error, path, texts, literal: memberType(type, tag).value };
    }

    function readSubmission(type, input, output) {
        const call = callOf(type);
        if (call?.typeParameters?.length !== 1 || call.parameters.length !== 2 || call.minArgumentCount !== 1 ||
            call.thisParameter || checker.hasEffectiveRestParameter(call))
            return;
        const [result] = call.typeParameters;
        const [successSymbol, failureSymbol] = call.parameters;
        const success = callOf(typeOf(successSymbol));
        const failure = callOfOptional(typeOf(failureSymbol));
        if (!success?.parameters[0] || !callsWithOne(failure) || typeOf(success.parameters[0]) !== output ||
            success.getReturnType() !== result || !shape(failure.getReturnType(), ts.TypeFlags.Void))
            return;
        const handler = callOf(call.getReturnType());
        const returns = handler && promised(handler.getReturnType());
        if (!handler || handler.minArgumentCount !== 0 || handler.parameters.length !== 1 || handler.typeParameters?.length ||
            checker.getNonNullableType(typeOf(handler.parameters[0])).getSymbol() !== event ||
            !returns?.isUnion() || !returns.types.includes(result) || !returns.types.some(type => shape(type, ts.TypeFlags.Undefined)))
            return;
        const context = typeOf(failure.parameters[0]);
        const values = distinct(matching(context, value => value === input));
        if (!values)
            return;
        return { call, success, failure, context, values };
    }

    function referencesConditional(type, parameter, schema) {
        if (!(type.flags & ts.TypeFlags.Conditional))
            return false;
        // The pinned checker retains a conditional's check type and declaration.
        // Recognize the concrete schema arm even if a different arm is any.
        return type.checkType === parameter &&
            checker.getTypeAtLocation(type.root.node.trueType).getSymbol() === schema.getSymbol();
    }

    function readPathTuple(signature, input) {
        const path = dependentPathTuple(ts, checker, signature, input, typeOf);
        if (!path || signature.typeParameters.length > 3)
            return;
        const { valueSlot } = path;
        const tuple = signature.getReturnType();
        if (!checker.isTupleType(tuple) || tuple.target.minLength !== 2 || tuple.target.fixedLength !== 2 ||
            tuple.target.hasRestElement)
            return;
        const [model, attributes] = checker.getTypeArguments(tuple).map(vueRef);
        if (!model?.canonical || model.readOnly || model.element !== valueSlot || model.write !== valueSlot ||
            !attributes?.canonical)
            return;

        const extras = signature.typeParameters[2];
        const extrasDefault = extras?.getDefault();
        let base = attributes.element;
        if (extras) {
            // An omitted options bag can only establish the base props. The
            // extra slot's default may declare index keys but no required members.
            const record = extrasDefault && checker.getIndexInfosOfType(extrasDefault);
            const [index, ...otherIndexes] = record ?? [];
            if (!base.isIntersection() || base.types.length !== 2 || !base.types.includes(extras) ||
                !admitsPlainRecord(extras) || !index || otherIndexes.length ||
                index.keyType !== checker.getStringType() || !(index.type.flags & (ts.TypeFlags.Any | ts.TypeFlags.Unknown)) ||
                extrasDefault.getProperties().length || extrasDefault.getCallSignatures().length ||
                extrasDefault.getConstructSignatures().length)
                return;
            base = base.types.find(arm => arm !== extras);
        }
        if (!(base.flags & ts.TypeFlags.Object) || !base.getProperties().length ||
            base.getCallSignatures().length || base.getConstructSignatures().length || checker.getIndexInfosOfType(base).length)
            return;
        const props = base.getProperties().map(symbol => {
            const [declaration] = symbol.declarations ?? [];
            const call = callOf(memberType(base, symbol));
            if (optional(symbol) || !call || call.parameters.length || call.thisParameter || call.typeParameters?.length ||
                !(call.getReturnType().flags & ts.TypeFlags.Void) || symbol.declarations?.length !== 1 ||
                !ts.isPropertySignature(declaration) || !ts.isIdentifier(declaration.name))
                return;
            return { symbol, call };
        });
        if (props.every(Boolean))
            return { ...path, props, config: readPathTupleOptions(ts, checker, signature, typeOf, definition, valueSlot, extras) };
    }

    const probes = [];
    for (const alias of aliases) {
        if (!selected.has(alias.name))
            continue;
        const calls = callsOf(alias);
        const [call] = calls;
        if (calls.length !== 1 || call.thisParameter || checker.hasEffectiveRestParameter(call))
            continue;
        const slots = call.typeParameters ?? [];
        const ambient = slots.length === 2 && call.parameters.length === 0;
        const schemaFactory = slots.length === 3 && call.parameters.length === 1;
        const defaultsAfterInput = call.parameters.length === 1 && slots.length > 0 &&
            slots.slice(1).every(parameter => parameter.getDefault());
        if (!ambient && !schemaFactory && !defaultsAfterInput)
            continue;
        const outputSlot = distinct(slots.filter(parameter => slots.includes(parameter.getDefault())));
        if (!outputSlot && slots.length !== 1)
            continue;
        const input = outputSlot?.getDefault() ?? slots[0];
        const output = outputSlot ?? input;
        if (!admitsPlainRecord(input) || !admitsPlainRecord(output))
            continue;
        const result = ambient ? checker.getNonNullableType(call.getReturnType()) : call.getReturnType();
        const values = distinct(matching(result, value => value === input));
        if (!values)
            continue;
        const pathSetters = result.getProperties().flatMap(member => {
            const signature = !optional(member) && dependentPathSetter(ts, checker,
                callOf(memberType(result, member)), input, typeOf);
            // The setter declaration owns its runtime path dialect, even through a re-export.
            const veeValidatePath = signature?.declaration && packageOf(signature.declaration) === 'vee-validate';
            return signature ? [{ member, signature, veeValidatePath }] : [];
        });
        const pathTuples = result.getProperties().flatMap(member => {
            const signature = !optional(member) && callOf(memberType(result, member));
            const tuple = readPathTuple(signature, input);
            const veeValidatePath = signature?.declaration && packageOf(signature.declaration) === 'vee-validate';
            return tuple ? [{ member, signature, veeValidatePath, ...tuple }] : [];
        });
        if (!pathSetters.length && !schemaFactory)
            continue;
        const validators = readValidators(result, input, pathSetters);
        const key = member => JSON.stringify(member.name);
        const checkValidators = validators.map(({ member, field, passed, errors }) =>
            `form[${key(member)}](${field ? '"joltInput"' : ''}).then(result => {
                const passed: boolean = result[${key(passed)}];
                ${errors ? `const errors: string[] = result[${key(errors)}];` : ''}
            });`).join('\n');
        const formProbe = { name: alias.name, ambient, values, pathSetters, pathTuples, validators, call, result };
        const submit = distinct(result.getProperties().map(member =>
            ({ member, signature: readSubmission(memberType(result, member), input, output) })).filter(match => match.signature));
        const update = distinct(matching(result, (value, member) => {
            const setter = !optional(member) && callOf(value);
            return callsWithOne(setter) &&
                acceptsFullRecord(typeOf(setter.parameters[0]), input, setter.declaration?.parameters[0]?.type) &&
                shape(setter.getReturnType(), ts.TypeFlags.Void) && setter.parameters.slice(1).every(parameter =>
                    checker.getNonNullableType(typeOf(parameter)) === checker.getBooleanType());
        }));
        const updateCall = update && callOf(memberType(result, update));
        const checkSubmit = (outputField, outputType) => `const submit: (event?: Event) => Promise<void | undefined> = form[${key(submit.member)}]<void>(
            values => { const value: ${outputType} = values.${outputField}; },
            context => { const stringValue: string = context[${key(submit.signature.values)}].joltInput; }
        );`;
        const inputFactory = defaultsAfterInput && input === slots[0];
        if (pathSetters.length && (inputFactory || ambient && slots.every(parameter => parameter.getDefault()))) {
            // Both calls use the default output slot. The options overload
            // requires the complete DTO at the point of initialization.
            const argumentsText = ambient ? '{ joltInput: string }, { joltOutput: number }' : '{ joltInput: string }';
            const defaultSubmit = submit && (ambient || output === input || output.getDefault() === input) ? submit : undefined;
            const formExpression = seed => `(${seed ? 'initial: { joltInput: string }' : ''}) => {
                const form = packageExports[${JSON.stringify(alias.name)}]<${argumentsText}>(${seed ? `{ ${key(seed)}: initial }` : ''});
                ${ambient ? 'if (form == null) return;' : ''}
                const current: { joltInput: string } = form[${key(values)}];
                form[${key(pathSetters[0].member)}]("joltInput", "next");
                ${defaultSubmit ? ambient ? checkSubmit('joltOutput', 'number') : checkSubmit('joltInput', 'string') : ''}
                ${update ? `form[${key(update)}]({ joltInput: 'next' });` : ''}
                ${checkValidators}
                return form;
            }`;
            const factory = { ...formProbe, submit: defaultSubmit, update, updateCall };
            if (call.minArgumentCount === 0)
                probes.push({ ...factory, expression: formExpression() });
            if (inputFactory) {
                const options = checker.getNonNullableType(typeOf(call.parameters[0]));
                const seed = initialMember(options, input);
                if (seed && options.getProperties().every(property => property === seed || optional(property)))
                    probes.push({ ...factory, seed, expression: formExpression(seed) });
            }
        }
        if (!schemaFactory)
            continue;
        if (!submit || !update)
            continue;
        const schema = call.typeParameters.find(parameter => parameter !== input && parameter !== output);
        const schemaShape = distinct(alternatives(checker.getBaseConstraintOfType(schema))
            .map(type => readSchema(type, input, output)).filter(Boolean));
        if (!schemaShape)
            continue;
        const options = checker.getNonNullableType(typeOf(call.parameters[0]));
        const schemaOption = distinct(matching(options, value => alternatives(value).some(branch =>
            exact(branch, schemaShape.type) || referencesConditional(branch, schema, schemaShape.type))));
        const seed = initialMember(options, input);
        if (!schemaOption || !seed || options.getProperties().some(member => member !== schemaOption && member !== seed && !optional(member)))
            continue;
        const { tag, parse, value, messages, path, texts, literal } = schemaShape;
        const typeArguments = call.typeParameters.map(parameter => parameter === input ? 'typeof input' :
            parameter === output ? '{ joltOutput: number }' : 'typeof schema').join(', ');
        // Resolve a concrete invocation through the locked checker. Its marker
        // fields differ so swapping input and output slots fails this probe.
        const expression = `(input: { joltInput: string }, schema: {
            ${key(tag)}: ${JSON.stringify(literal)},
            ${key(parse)}: (values: { joltInput: string }) => Promise<{
                ${key(value)}: { joltOutput: number }, ${key(messages)}: { ${key(path)}?: string, ${key(texts)}: string[] }[]
            }>
        }) => {
            const form = packageExports[${JSON.stringify(alias.name)}]<${typeArguments}>({
                ${key(seed)}: input, ${key(schemaOption)}: schema
            });
            ${checkSubmit('joltOutput', 'number')}
            const current: typeof input = form[${key(values)}];
            form[${key(update)}](input);
            ${checkValidators}
            return form;
        }`;
        probes.push({ ...formProbe, expression, schemaShape, schemaOption, seed, submit, update, updateCall });
    }
    return probes;
}

function hasFixedMappedEntries(ts, checker, node, isEntry) {
    function read(node, bindings, readShape) {
        if (ts.isParenthesizedTypeNode(node))
            return read(node.type, bindings, readShape);
        if (!ts.isTypeReferenceNode(node))
            return readShape(node, bindings);
        let symbol = checker.getSymbolAtLocation(node.typeName);
        if (symbol?.flags & ts.SymbolFlags.Alias)
            symbol = checker.getAliasedSymbol(symbol);
        const bound = bindings.get(symbol);
        if (bound)
            return bound.node && read(bound.node, bound.bindings, readShape);
        const alias = symbol?.declarations?.find(ts.isTypeAliasDeclaration);
        if (!alias)
            return readShape(node, bindings);
        const next = new Map(alias.typeParameters?.map((parameter, index) => [
            checker.getSymbolAtLocation(parameter.name), { node: node.typeArguments?.[index], bindings },
        ]));
        return read(alias.type, next, readShape);
    }

    function readMap(node, bindings) {
        if (!node)
            return false;
        if (!ts.isMappedTypeNode(node))
            return false;
        const { type: entry, typeParameter } = node;
        if (!entry)
            return false;
        const keys = typeParameter.constraint;
        // Homomorphic wrappers such as Partial<T> forward the original values.
        if (ts.isIndexedAccessTypeNode(entry) && keys && ts.isTypeOperatorNode(keys) &&
            keys.operator === ts.SyntaxKind.KeyOfKeyword &&
            checker.getTypeAtLocation(keys.type) === checker.getTypeAtLocation(entry.objectType) &&
            ts.isTypeReferenceNode(entry.indexType) &&
            checker.getSymbolAtLocation(typeParameter.name) === checker.getSymbolAtLocation(entry.indexType.typeName))
            return read(entry.objectType, bindings, readMap);
        return read(entry, bindings, readEntry);
    }

    function readEntry(node, bindings) {
        if (ts.isArrayTypeNode(node))
            return read(node.elementType, bindings, child => checker.getTypeAtLocation(child) === checker.getStringType());
        if (ts.isUnionTypeNode(node)) {
            const present = node.types.filter(branch => !(checker.getTypeAtLocation(branch).flags &
                (ts.TypeFlags.Undefined | ts.TypeFlags.Null)));
            if (present.length === 1)
                return read(present[0], bindings, readEntry);
            return false;
        }
        return isEntry(checker.getNonNullableType(checker.getTypeAtLocation(node)));
    }

    return read(node, new Map(), readMap);
}

export function readSchemaFormProjections(ts, checker, probes, statements, diagnostics, definition, vueRef) {
    const results = new Map();
    const add = (name, signature) => {
        const signatures = results.get(name) ?? [];
        signatures.push(signature);
        results.set(name, signatures);
    };
    const string = { kind: 'string' };
    const nothing = { kind: 'void' };
    const input = { kind: 'recordTypeParameter', name: 'TInput' };
    const output = { kind: 'recordTypeParameter', name: 'TOutput' };
    const parameter = (name, type) => ({ name, type, optional: false });
    const member = (symbol, type, optional = false) =>
        ({ name: symbol.name, type, definition: definition(symbol.declarations?.[0]), optional, readOnly: true });
    const callable = (kind, parameters, returnType, node) =>
        ({ kind, signatures: [{ parameters, returnType, definition: definition(node) }] });
    const promise = elementType => ({ kind: 'promise', elementType });
    const declaredObject = (type, members) => ({
        kind: 'object', name: type.getSymbol()?.name, typeDefinition: definition(type.getSymbol()?.declarations?.[0]), members,
    });
    const errorTable = { kind: 'readonlyDictionary', name: 'FormErrors', elementType: string };
    const isInputErrorMap = (type, isEntry = value => Boolean(value.flags & ts.TypeFlags.String)) => {
        const entry = type.getProperty('joltInput');
        // A mapped path must resolve to the input marker, without also
        // accepting the output marker or unrelated keys through an index.
        return Boolean(entry?.flags & ts.SymbolFlags.Optional) &&
            !type.getProperty('joltOutput') && !checker.getIndexInfosOfType(type).length &&
            type.getProperties().every(property => Boolean(property.flags & ts.SymbolFlags.Optional) &&
                isEntry(checker.getNonNullableType(checker.getTypeOfSymbol(property))));
    };

    for (const [index, probe] of probes.entries()) {
        const statement = statements[index];
        const arrow = statement.declarationList.declarations[0].initializer;
        const body = arrow.body.statements;
        const validationStart = body.length - 1 - probe.validators.length;
        const validationChecks = body.slice(validationStart, -1);
        const updateCheck = !probe.schemaShape && probe.update ? body[validationStart - 1] : undefined;
        const submitCheck = probe.submit ? body[probe.schemaShape ? 1 : validationStart - 1 - Number(Boolean(updateCheck))] : undefined;
        const optionalChecks = [...validationChecks];
        if (updateCheck)
            optionalChecks.push(updateCheck);
        if (!probe.schemaShape && submitCheck)
            optionalChecks.push(submitCheck);
        const contains = (node, diagnostic) => diagnostic.start >= node.pos && diagnostic.start < node.end;
        // A failed optional member probe removes that capability without
        // invalidating the factory or other independently checked members.
        const failures = diagnostics.filter(diagnostic => contains(statement, diagnostic));
        if (failures.some(diagnostic => !optionalChecks.some(check => contains(check, diagnostic))))
            continue;
        const failed = check => check && failures.some(diagnostic => contains(check, diagnostic));
        const field = { kind: 'typeParameter', name: 'TField' };
        const pathParameter = (signature, veeValidatePath, arrayIndices) => parameter(signature.parameters[0].name, {
            kind: 'propertyPath', elementType: input, variants: [field], veeValidatePath: veeValidatePath || undefined,
            arrayIndices: arrayIndices || undefined,
        });
        // The recognized lookup splits a dotted path. Even where Path<T> also
        // admits brackets, that lookup alone cannot establish the setter's value.
        const pathSetters = probe.pathSetters.map(({ member: symbol, signature, veeValidatePath }) => member(symbol, {
            kind: 'method', signatures: [{
                parameters: [
                    pathParameter(signature, veeValidatePath),
                    parameter(signature.parameters[1].name, field),
                    ...signature.parameters.slice(2).map(({ name }) => ({ name, type: { kind: 'boolean' }, optional: true })),
                ],
                returnType: nothing, typeParameters: [field.name], definition: definition(signature.declaration),
            }],
        }));
        const pathTuples = probe.pathTuples.map(({ member: symbol, signature, veeValidatePath, arrayIndices, props, config }) => {
            const state = { name: 'Item1', readOnly: true, type: {
                kind: 'ref', name: 'VueSplitOptionalRef', readOnly: true,
                elementType: { kind: 'nullableSlot', elementType: field }, writeType: field,
            } };
            const tuple = elementType => ({ kind: 'tupleView', members: [
                state, { name: 'Item2', readOnly: true, type: { kind: 'ref', readOnly: true, elementType } },
            ] });
            const projected = {
                parameters: [pathParameter(signature, veeValidatePath, arrayIndices)],
                returnType: tuple({ kind: 'object', members: props.map(({ symbol, call }) =>
                    member(symbol, callable('function', [], nothing, call.declaration))) }),
                typeParameters: [field.name], definition: definition(signature.declaration),
            };
            const signatures = [projected];
            for (const { parameter, hasExtras, overloadPriority } of config ?? []) {
                const overload = { ...projected, parameters: [...projected.parameters, parameter], overloadPriority };
                if (hasExtras) {
                    // Callback props can replace any base event property. Keep
                    // the same returned ref and expose the selected extras DTO.
                    const extras = { kind: 'recordTypeParameter', name: 'TProps' };
                    signatures.push({ ...overload, typeParameters: [field.name, extras.name],
                        returnType: tuple(extras) });
                } else {
                    signatures.push(overload);
                }
            }
            return member(symbol, { kind: 'method', signatures });
        });
        const validators = probe.validators.flatMap(({ member: symbol, validation, passed, errors, field: fieldValidation,
            veeValidatePath, arrayIndices }, index) => {
            if (failed(validationChecks[index]))
                return [];
            const members = [member(passed, { kind: 'boolean' })];
            if (errors)
                members.push(member(errors, { kind: 'array', elementType: string }));
            const result = callable('method', fieldValidation ? [pathParameter(validation, veeValidatePath, arrayIndices)] : [],
                promise({ kind: 'object', members }), validation.declaration);
            if (fieldValidation)
                result.signatures[0].typeParameters = [field.name];
            return [member(symbol, result)];
        });
        const form = body[0].declarationList.declarations[0].initializer;
        const resolvedForm = checker.getTypeAtLocation(form);
        // The ambient probe guards an absent provider before reading its
        // fields. The initializer's type still carries the nullish arm.
        const formType = probe.ambient ? checker.getNonNullableType(resolvedForm) : resolvedForm;
        const isString = entry => entry === checker.getStringType();
        const isStringArray = entry =>
            checker.isArrayType(entry) && isString(checker.getElementTypeOfArrayType(entry));
        function findErrorTable(isEntry, computed) {
            const candidates = formType.getProperties().filter(candidate => {
                if (candidate.flags & ts.SymbolFlags.Optional)
                    return false;
                const ref = vueRef(checker.getTypeOfPropertyOfType(formType, candidate.name));
                if (!ref || computed && !(ref.name === 'VueComputedRef' && ref.readOnly) ||
                    !isInputErrorMap(ref.element, isEntry))
                    return false;
                // The marker identifies the key domain. Check the generic source as well:
                // its entry can be T[K] or conditional even when the marker yields string.
                const property = probe.result.getProperty(candidate.name)?.declarations?.find(ts.isPropertySignature);
                const annotation = property?.type;
                if (!annotation || !ts.isTypeReferenceNode(annotation))
                    return false;
                const genericRef = vueRef(checker.getTypeAtLocation(annotation));
                const [map] = annotation.typeArguments ?? [];
                return genericRef && map && checker.getTypeAtLocation(map) === genericRef.element &&
                    hasFixedMappedEntries(ts, checker, map, isEntry);
            });
            return candidates.length === 1 ? candidates[0] : undefined;
        }
        const firstErrorTable = findErrorTable(isString, true);
        const errorRef = firstErrorTable ? [member(firstErrorTable, {
            kind: 'ref', name: 'VueComputedRef', readOnly: true, elementType: errorTable,
        })] : [];
        const bag = findErrorTable(isStringArray, false);
        // Some packages implement the declared Ref as computed. Preserve the live read.
        const errorBag = bag ? [member(bag, {
            kind: 'ref', readOnly: true, elementType: {
                kind: 'readonlyDictionary', name: 'FormErrorBag', elementType: { kind: 'array', elementType: string },
            },
        })] : [];

        const partial = elementType => ({ kind: 'partialRecord', elementType });
        // Fields may unregister and remove their keys even after a full seed.
        // Raw/live values remain sparse until the schema produces its output.
        const values = partial(input);
        const state = [member(probe.values, values), ...errorRef, ...errorBag];
        const fields = [...pathSetters, ...pathTuples, ...validators];
        const pathSlot = probe.pathSetters[0]?.signature.typeParameters?.[0];
        pathSlot?.getConstraint();
        const additional = readCommonFormMembers(ts, checker, probe.result, formType, new Set([
            ...state.map(member => member.name), ...fields.map(member => member.name),
            probe.update?.name, probe.submit?.member.name,
        ]), {
            pathRoute: pathSlot?.constraint, veeValidatePath: probe.pathSetters[0]?.veeValidatePath,
            arrayIndices: probe.validators.some(validator => validator.field && validator.arrayIndices),
            vueRef, member, callable, isInputErrorMap, input,
        });
        let update;
        if (probe.update && !failed(updateCheck)) {
            const [patch, ...trailing] = probe.updateCall.parameters;
            const mutation = callable('method', [parameter(patch.name, input), ...(!probe.schemaShape ? trailing.map(flag => ({
                name: flag.name, type: { kind: 'boolean' }, optional: true,
            })) : [])], nothing, probe.updateCall.declaration);
            update = member(probe.update, mutation);
        }

        function submitMember(source, destination) {
            if (!submitCheck || failed(submitCheck))
                return;
            const handlers = probe.submit.signature;
            const invocation = submitCheck.declarationList.declarations[0].initializer;
            const call = checker.getResolvedSignature(invocation);
            const contextCallback = checker.getNonNullableType(checker.getTypeOfSymbol(call.parameters[1])).getCallSignatures()[0];
            const concreteContext = checker.getTypeOfSymbol(contextCallback.parameters[0]);
            const context = handlers.context;
            const errorMaps = concreteContext.getProperties().filter(candidate => {
                const type = checker.getTypeOfPropertyOfType(concreteContext, candidate.name);
                const annotation = context.getProperty(candidate.name)?.declarations?.find(ts.isPropertySignature)?.type;
                return isInputErrorMap(type, isString) && annotation &&
                    hasFixedMappedEntries(ts, checker, annotation, isString);
            });
            const errorMap = errorMaps.length === 1 && context.getProperty(errorMaps[0].name);
            if (!errorMap)
                return;
            const success = callable('function', [parameter(handlers.success.parameters[0].name, destination)], nothing, handlers.success.declaration);
            const failureContext = declaredObject(context, [member(handlers.values, source), member(errorMap, errorTable)]);
            const failure = callable('function', [parameter(handlers.failure.parameters[0].name, failureContext)], nothing, handlers.failure.declaration);
            const onSuccess = parameter(handlers.call.parameters[0].name, success);
            const onFailure = parameter(handlers.call.parameters[1].name, failure);
            const submitResult = callable('function', [], promise(nothing));
            return member(probe.submit.member, {
                kind: 'method', signatures: [[onSuccess], [onSuccess, onFailure]].map(parameters => ({
                    parameters, returnType: submitResult, definition: definition(handlers.call.declaration),
                })),
            });
        }

        if (!probe.schemaShape) {
            // Calling without initial values can leave fields absent. The
            // output of an injected provider has its own independent DTO.
            const sparse = members => {
                const context = { kind: 'object', members };
                return probe.ambient ? { kind: 'undefinedable', elementType: context } : context;
            };
            const parameters = probe.seed ? [parameter(probe.call.parameters[0].name,
                { kind: 'object', members: [member(probe.seed, input)] })] : [];
            const signature = { parameters, typeParameters: [input.name], definition: definition(probe.call.declaration) };
            const submit = probe.submit && submitMember(values, probe.ambient ? partial(output) : values);
            const core = [...state, ...(update ? [update] : []), ...fields, ...additional];
            add(probe.name, { ...signature, returnType: sparse(submit && !probe.ambient ? [...core, submit] : core) });
            if (submit && probe.ambient)
                add(probe.name, { ...signature, typeParameters: [input.name, output.name], returnType: sparse([...core, submit]) });
            continue;
        }

        const submit = submitMember(values, output);
        if (!submit)
            continue;

        const { type, tag, parse, parseCall, value, result, messages, error, path, texts, literal } = probe.schemaShape;
        const errors = { kind: 'recordArray', elementType: declaredObject(error, [
            member(path, { kind: 'undefinedable', elementType: string }, true),
            member(texts, { kind: 'array', elementType: string }),
        ]) };
        // Some schemas declare an optional output. An adapter that promises a
        // transformed submission must supply it even on its validation-error path.
        const parseResult = declaredObject(result, [member(value, output), member(messages, errors)]);
        parseResult.name = type.getSymbol()?.name + 'Result';
        const schema = declaredObject(type, [
            member(tag, { kind: 'stringEnum', name: type.getSymbol()?.name + 'Discriminator', stringLiterals: [literal] }),
            member(parse, callable('function', [parameter(parseCall.parameters[0].name, values)], promise(parseResult), parseCall.declaration)),
        ]);
        const options = {
            kind: 'object', members: [member(probe.seed, input), member(probe.schemaOption, schema)],
        };
        add(probe.name, {
            parameters: [parameter(probe.call.parameters[0].name, options)],
            typeParameters: [input.name, output.name],
            definition: definition(probe.call.declaration),
            returnType: { kind: 'object', members: [...state, submit, update, ...fields, ...additional] },
        });
    }
    return results;
}
