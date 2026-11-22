import { alternatives, singleCall } from './generic-callbacks.mjs';
import { dependentPathSetter } from './dependent-path.mjs';

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

export function schemaFormProbes(ts, program, checker, source, aliases, callsOf, selected, typeOf, packageOf) {
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
    const callOf = type => singleCall(type)[0];
    const callOfOptional = type => callOf(checker.getNonNullableType(type));
    const promised = type => type.target?.getSymbol() === promise ? checker.getTypeArguments(type)[0] : undefined;
    const shape = (type, flag) => Boolean(type?.flags & flag);
    const exact = (type, parameter) => checker.getNonNullableType(type) === parameter;
    const partial = (type, parameter) => type === parameter ||
        shape(type, ts.TypeFlags.Conditional) && type.aliasTypeArguments?.[0] === parameter ||
        type.aliasTypeArguments?.length === 1 && type.aliasTypeArguments[0] === parameter;
    const forwards = (type, parameter) => alternatives(type).some(branch => partial(branch, parameter));
    const callsWithOne = call => call && call.minArgumentCount === 1 && !call.typeParameters?.length &&
        !call.thisParameter && !checker.hasEffectiveRestParameter(call);

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

    const probes = [];
    for (const alias of aliases) {
        if (!selected.has(alias.name))
            continue;
        const calls = callsOf(alias);
        const [call] = calls;
        if (calls.length !== 1 || call.thisParameter || checker.hasEffectiveRestParameter(call))
            continue;
        const ambient = call.typeParameters?.length === 2 && call.parameters.length === 0;
        if (!ambient && (call.typeParameters?.length !== 3 || call.parameters.length !== 1))
            continue;
        const output = distinct(call.typeParameters.filter(parameter => call.typeParameters.includes(parameter.getDefault())));
        if (!output)
            continue;
        const input = output.getDefault();
        if (!admitsPlainRecord(input) || !admitsPlainRecord(output))
            continue;
        const result = ambient ? checker.getNonNullableType(call.getReturnType()) : call.getReturnType();
        const values = distinct(matching(result, value => value === input));
        const submit = distinct(result.getProperties().map(member =>
            ({ member, signature: readSubmission(memberType(result, member), input, output) })).filter(match => match.signature));
        const update = distinct(matching(result, value => {
            const setter = callOf(value);
            return callsWithOne(setter) && forwards(typeOf(setter.parameters[0]), input) &&
                shape(setter.getReturnType(), ts.TypeFlags.Void) &&
                setter.parameters.slice(1).every(parameter => shape(checker.getNonNullableType(typeOf(parameter)), ts.TypeFlags.Boolean));
        }));
        if (!values || !submit || !update)
            continue;
        const pathSetters = result.getProperties().flatMap(member => {
            const signature = !optional(member) && dependentPathSetter(ts, checker,
                callOf(memberType(result, member)), input, typeOf);
            // The setter declaration owns its runtime path dialect, even through a re-export.
            const veeValidatePath = signature?.declaration && packageOf(signature.declaration) === 'vee-validate';
            return signature ? [{ member, signature, veeValidatePath }] : [];
        });
        const key = member => JSON.stringify(member.name);
        if (ambient) {
            if (!pathSetters.length || !call.typeParameters.every(parameter => parameter.getDefault()))
                continue;
            const setter = pathSetters[0].signature;
            const pathConstraint = signature => {
                const node = signature.declaration?.typeParameters?.[0]?.constraint;
                if (!node || !ts.isTypeReferenceNode(node) || node.typeArguments?.length !== 1)
                    return;
                const [slot] = signature.typeParameters;
                // Resolve the inherited signature's slot. The effective base constraint
                // can collapse different Path<T> instantiations to the same string domain.
                slot.getConstraint();
                return slot.constraint;
            };
            const route = pathConstraint(setter);
            const validators = result.getProperties().flatMap(member => {
                const validation = !optional(member) && callOf(memberType(result, member));
                const returned = validation && promised(validation.getReturnType());
                if (!returned || validation.thisParameter || checker.hasEffectiveRestParameter(validation))
                    return [];
                const validationRoute = validation.typeParameters?.length === 1 && pathConstraint(validation);
                const field = Boolean(route?.aliasSymbol && route.aliasTypeArguments?.[0] === input &&
                    validationRoute?.aliasSymbol === route.aliasSymbol && validationRoute.aliasTypeArguments?.[0] === input &&
                    validation.minArgumentCount === 1 &&
                    typeOf(validation.parameters[0]) === validation.typeParameters[0]);
                if (!field && (validation.minArgumentCount || validation.typeParameters?.length))
                    return [];
                const passed = distinct(matching(returned, (type, property) => !optional(property) && type === checker.getBooleanType()));
                const errors = field && distinct(matching(returned, (type, property) => !optional(property) &&
                    checker.isArrayType(type) && checker.getElementTypeOfArrayType(type) === checker.getStringType()));
                return passed && (!field || errors) ? [{ member, validation, passed, errors, field }] : [];
            });
            const expression = `() => {
                const form = packageExports[${JSON.stringify(alias.name)}]<{ joltInput: string }, { joltOutput: number }>();
                if (form == null) return;
                const current: { joltInput: string } = form[${key(values)}];
                form[${key(pathSetters[0].member)}]("joltInput", "next", false);
                ${validators.map(({ member, field, passed }) =>
                    `form[${key(member)}](${field ? '"joltInput"' : ''}).then(result => { const passed: boolean = result[${key(passed)}]; });`).join('\n')}
                return form;
            }`;
            probes.push({ name: alias.name, expression, ambient, values, pathSetters, validators, call });
            continue;
        }
        const schema = call.typeParameters.find(parameter => parameter !== input && parameter !== output);
        const schemaShape = distinct(alternatives(checker.getBaseConstraintOfType(schema))
            .map(type => readSchema(type, input, output)).filter(Boolean));
        if (!schemaShape)
            continue;
        const options = checker.getNonNullableType(typeOf(call.parameters[0]));
        const schemaOption = distinct(matching(options, value => alternatives(value).some(branch =>
            exact(branch, schemaShape.type) || referencesConditional(branch, schema, schemaShape.type))));
        const seed = distinct(matching(options, value => forwards(value, input)));
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
            const submit: (event?: Event) => Promise<void | undefined> = form[${key(submit.member)}]<void>(
                values => { const numberValue: number = values.joltOutput; },
                context => { const stringValue: string = context[${key(submit.signature.values)}].joltInput; }
            );
            const current: typeof input = form[${key(values)}];
            form[${key(update)}](input);
            return form;
        }`;
        probes.push({ name: alias.name, expression, schemaShape, schemaOption, seed, values, submit, update, pathSetters,
            updateCall: callOf(memberType(result, update)), call });
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
        return isEntry(checker.getTypeAtLocation(node));
    }

    return read(node, new Map(), readMap);
}

export function readSchemaFormProjections(ts, checker, probes, statements, diagnostics, definition, vueRef) {
    const results = new Map();
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
        if (diagnostics.some(diagnostic => diagnostic.start >= statement.pos && diagnostic.start < statement.end))
            continue;
        const arrow = statement.declarationList.declarations[0].initializer;
        const field = { kind: 'typeParameter', name: 'TField' };
        const pathParameter = (signature, veeValidatePath) => parameter(signature.parameters[0].name, {
            kind: 'propertyPath', elementType: input, variants: [field], veeValidatePath: veeValidatePath || undefined,
        });
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
        if (probe.ambient) {
            const validators = probe.validators.map(({ member: symbol, validation, passed, errors, field: fieldValidation }) => {
                const members = [member(passed, { kind: 'boolean' })];
                if (errors)
                    members.push(member(errors, { kind: 'array', elementType: string }));
                const result = callable('method', fieldValidation
                    ? [pathParameter(validation, probe.pathSetters[0].veeValidatePath)] : [],
                promise({ kind: 'object', members }), validation.declaration);
                if (fieldValidation)
                    result.signatures[0].typeParameters = [field.name];
                return member(symbol, result);
            });
            // Without a runtime argument to establish the provider, a correlated
            // form lookup may be absent. The caller supplies the DTO shape; fields
            // can still be absent before they have been registered or initialized.
            const context = { kind: 'object', members: [
                member(probe.values, { kind: 'partialRecord', elementType: input }), ...pathSetters, ...validators,
            ] };
            results.set(probe.name, { parameters: [], typeParameters: [input.name],
                definition: definition(probe.call.declaration), returnType: { kind: 'undefinedable', elementType: context } });
            continue;
        }
        const [form, submit] = arrow.body.statements.slice(0, 2).map(statement => statement.declarationList.declarations[0].initializer);
        const call = checker.getResolvedSignature(submit);
        const contextCallback = checker.getNonNullableType(checker.getTypeOfSymbol(call.parameters[1])).getCallSignatures()[0];
        const concreteContext = checker.getTypeOfSymbol(contextCallback.parameters[0]);
        const context = probe.submit.signature.context;
        const errorMaps = concreteContext.getProperties().filter(candidate => isInputErrorMap(checker.getTypeOfSymbol(candidate)));
        const errorMap = errorMaps.length === 1 && context.getProperty(errorMaps[0].name);
        if (!errorMap)
            continue;

        const formType = checker.getTypeAtLocation(form);
        const errorRefs = formType.getProperties().filter(candidate => {
            if (candidate.flags & ts.SymbolFlags.Optional)
                return false;
            const ref = vueRef(checker.getTypeOfSymbol(candidate));
            return ref?.name === 'VueComputedRef' && ref.readOnly && isInputErrorMap(ref.element);
        });
        const errorRef = errorRefs.length === 1 ? [member(errorRefs[0], {
            kind: 'ref', name: 'VueComputedRef', readOnly: true, elementType: errorTable,
        })] : [];
        const genericForm = probe.call.getReturnType();
        const isStringArray = entry =>
            checker.isArrayType(entry) && checker.getElementTypeOfArrayType(entry) === checker.getStringType();
        const errorBags = formType.getProperties().filter(candidate => {
            if (candidate.flags & ts.SymbolFlags.Optional)
                return false;
            const ref = vueRef(checker.getTypeOfSymbol(candidate));
            if (!ref || !isInputErrorMap(ref.element, isStringArray))
                return false;
            // A concrete input marker cannot distinguish fixed entries from values
            // whose type depends on the input DTO or the individual field.
            const property = genericForm.getProperty(candidate.name)?.declarations?.find(ts.isPropertySignature);
            const annotation = property?.type;
            if (!annotation || !ts.isTypeReferenceNode(annotation))
                return false;
            const genericRef = vueRef(checker.getTypeAtLocation(annotation));
            const [map] = annotation.typeArguments ?? [];
            return genericRef && map && checker.getTypeAtLocation(map) === genericRef.element &&
                hasFixedMappedEntries(ts, checker, map, isStringArray);
        });
        // Expose the live map as a read view, including when the declaration
        // uses Ref but the implementation computes the bag from field errors.
        const errorBag = errorBags.length === 1 ? [member(errorBags[0], {
            kind: 'ref', readOnly: true, elementType: {
                kind: 'readonlyDictionary', name: 'FormErrorBag', elementType: { kind: 'array', elementType: string },
            },
        })] : [];

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
            member(parse, callable('function', [parameter(parseCall.parameters[0].name, input)], promise(parseResult), parseCall.declaration)),
        ]);
        const handlers = probe.submit.signature;
        const success = callable('function', [parameter(handlers.success.parameters[0].name, output)], nothing, handlers.success.declaration);
        const failureContext = declaredObject(context, [
            member(handlers.values, input),
            member(errorMap, errorTable),
        ]);
        const failure = callable('function', [parameter(handlers.failure.parameters[0].name, failureContext)], nothing, handlers.failure.declaration);
        const onSuccess = parameter(handlers.call.parameters[0].name, success);
        const onFailure = parameter(handlers.call.parameters[1].name, failure);
        const submitResult = callable('function', [], promise(nothing));
        const submitMethods = {
            kind: 'method',
            signatures: [[onSuccess], [onSuccess, onFailure]].map(parameters => ({
                parameters, returnType: submitResult, definition: definition(handlers.call.declaration),
            })),
        };
        const options = {
            kind: 'object', members: [member(probe.seed, input), member(probe.schemaOption, schema)],
        };
        results.set(probe.name, {
            parameters: [parameter(probe.call.parameters[0].name, options)],
            typeParameters: [input.name, output.name],
            definition: definition(probe.call.declaration),
            returnType: { kind: 'object', members: [
                member(probe.values, input),
                ...errorRef,
                ...errorBag,
                member(probe.submit.member, submitMethods),
                member(probe.update, callable('method', [parameter(probe.updateCall.parameters[0].name, input)], nothing,
                    probe.updateCall.declaration)),
                ...pathSetters,
            ] },
        });
    }
    return results;
}
