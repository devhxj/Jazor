export function readPropertyKeySignature(ts, checker, signatures, typeOf, definition, readRef, unwrapRef, includeOtherArities) {
    if (!signatures.some(call => call.typeParameters?.length >= 2 && call.parameters.length >= 2))
        return;
    const plain = signatures.length === 1 && signatures[0].typeParameters?.length === 2 &&
        signatures[0].minArgumentCount === signatures[0].parameters.length &&
        [2, 3].includes(signatures[0].parameters.length);
    const applicable = signatures.filter(call => acceptsTwoArguments(checker, call));
    if (!plain && !includeOtherArities && applicable.length !== signatures.length)
        return;
    const domains = applicable.map(call => propertyKeyDomain(ts, checker, call, typeOf, true));
    const [selected] = domains;
    const ref = selected && domains.every(domain => domain && samePropertyKeyDomain(domain, selected)) && selected;
    const root = { kind: 'typeParameter', name: 'TRecord' };
    const field = { kind: 'typeParameter', name: 'TValue' };
    const project = (domain, returnType, trailing, selectedField = field) => {
        const { call, object, key } = domain;
        const parameters = [
            { name: object.name, type: root, optional: false },
            { name: key.name, type: { kind: 'propertyKey', elementType: root, variants: [selectedField] }, optional: false },
            ...trailing,
        ];
        const typeParameters = selectedField === field ? [root.name, field.name] : [root.name];
        return { parameters, returnType, typeParameters, definition: definition(call.declaration) };
    };
    if (plain) {
        const [call] = signatures;
        const domain = propertyKeyDomain(ts, checker, call, typeOf, false);
        const value = call.parameters[2];
        const result = call.getReturnType();
        if (domain && domain.matchesIndex(value ? typeOf(value) : result) &&
            (!value || result.flags & ts.TypeFlags.Void))
            return [project(domain, value ? { kind: 'void' } : field,
                value ? [{ name: value.name, type: field, optional: false }] : [])];
    }
    const result = [];
    if (ref) {
        // Matching two-argument domains leave declaration order as the tiebreak.
        // Read the selected return through Vue's canonical host contract.
        const returnType = readRef(ref.call.getReturnType(), type => ref.matchesIndex(type) ? field : undefined);
        if (returnType?.elementType === field && !returnType.writeType)
            result.push(project(ref, returnType, []));
    }
    for (const call of signatures) {
        if (call.minArgumentCount > 3 || call.parameters.length < 3 && !checker.hasEffectiveRestParameter(call))
            continue;
        const parameter = call.parameters[2];
        const type = parameter && checker.getNonNullableType(typeOf(parameter));
        // Only skip domains proven disjoint from a JavaScript function.
        // Function and objects with optional properties can accept one
        // without declaring any call signatures of their own.
        const scalar = ts.TypeFlags.StringLike | ts.TypeFlags.NumberLike | ts.TypeFlags.BooleanLike |
            ts.TypeFlags.BigIntLike | ts.TypeFlags.ESSymbolLike | ts.TypeFlags.Null | ts.TypeFlags.Undefined |
            ts.TypeFlags.Void | ts.TypeFlags.Never;
        if (type && !checker.hasEffectiveRestParameter(call) &&
            (type.isUnion() ? type.types : [type]).every(arm => arm.flags & scalar))
            continue;
        const domain = propertyKeyDomain(ts, checker, call, typeOf, true, 3);
        const emit = domain && readPropertyEmit(ts, checker, domain, typeOf, definition, field);
        const trailingOptional = call.parameters.slice(3).every(parameter =>
            parameter.valueDeclaration && checker.isOptionalParameter(parameter.valueDeclaration));
        const returnType = emit && trailingOptional &&
            readRef(call.getReturnType(), type => domain.matchesIndex(type) ? field : undefined);
        // Prefer options for a target-typed new() when both overloads have three arguments.
        if (returnType?.elementType === field && !returnType.writeType)
            result.push({ ...project(domain, returnType, [emit]), overloadPriority: -1 });
        break;
    }
    // Direct options and options after a hidden slot have the same C#
    // arity. Prefer the direct call when both families can project.
    const directOptions = readPropertyOptions(ts, checker, signatures, 3, typeOf, definition, readRef, unwrapRef, field, project);
    const optionsAfterEmit = readPropertyOptions(ts, checker, signatures, 4, typeOf, definition, readRef, unwrapRef, field, project);
    result.push(...directOptions, ...optionsAfterEmit.filter(signature =>
        !directOptions.length || !signature.parameters[2].fixedUndefined));
    return result.length ? result : undefined;
}

function readPropertyEmit(ts, checker, domain, typeOf, definition, field) {
    const parameter = domain.call.parameters[2];
    const type = checker.getNonNullableType(typeOf(parameter));
    const [call, ...others] = type.getCallSignatures();
    if (!call || others.length || call.thisParameter || call.typeParameters?.length ||
        call.parameters.length !== 2 || type.getProperties().length || type.getConstructSignatures().length ||
        checker.getIndexInfosOfType(type).length || call.getReturnType() !== checker.getVoidType())
        return;

    const [name, value] = call.parameters;
    const nameType = typeOf(name);
    const string = checker.getStringType();
    if (nameType !== string && (!domain.call.typeParameters?.includes(nameType) ||
        checker.getBaseConstraintOfType(nameType) !== string))
        return;
    const restCallback = checker.hasEffectiveRestParameter(call);
    const valueType = typeOf(value);
    let payload = field;
    if (restCallback) {
        const [item] = checker.isArrayType(valueType) ? checker.getTypeArguments(valueType) : [];
        if (call.minArgumentCount !== 1 || !value.valueDeclaration?.dotDotDotToken ||
            !item || !(item.flags & (ts.TypeFlags.Any | ts.TypeFlags.Unknown)))
            return;
        payload = { kind: 'recordArray', elementType: { kind: 'undefinedable', elementType: { kind: 'unknownValue' } } };
    } else if (call.minArgumentCount !== 2 || !domain.matchesIndex(valueType)) {
        return;
    }
    return { name: parameter.name, optional: false, ...(restCallback ? { restCallback } : {}),
        type: { kind: 'function', signatures: [{
            parameters: [{ name: name.name, type: { kind: 'string' }, optional: false },
                { name: value.name, type: payload, optional: false }],
            returnType: { kind: 'void' }, definition: definition(call.declaration),
        }] },
    };
}

function samePropertyKeyDomain(first, second) {
    return first.objectBound === second.objectBound && first.stringKeys === second.stringKeys &&
        first.call.declaration?.parent === second.call.declaration?.parent;
}

function readPropertyOptions(ts, checker, signatures, arity, typeOf, definition, readRef, unwrapRef, field, project) {
    const candidates = signatures.filter(call => call.minArgumentCount <= arity &&
        (call.parameters.length >= arity || checker.hasEffectiveRestParameter(call)));
    const domains = candidates.map(call => propertyKeyDomain(ts, checker, call, typeOf, true, arity));
    const [first] = domains;
    if (!first || domains.some(domain => !domain || !samePropertyKeyDomain(first, domain)))
        return [];

    const optional = member => Boolean(member.flags & ts.SymbolFlags.Optional);
    const withoutUndefined = type => type.isUnion()
        ? checker.getUnionType(type.types.filter(branch => !(branch.flags & ts.TypeFlags.Undefined)))
        : type;
    const optionMemberType = (record, member) => {
        const type = checker.getTypeOfPropertyOfType(record, member.name);
        return optional(member) ? withoutUndefined(type) : type;
    };
    const optionsIndex = arity - 1;
    const options = domains.map(({ call }) => {
        const parameter = call.parameters[optionsIndex];
        return checker.isOptionalParameter(parameter.valueDeclaration)
            ? withoutUndefined(typeOf(parameter)) : typeOf(parameter);
    });
    if (options.some(option => !(option.flags & ts.TypeFlags.Object) || option.getCallSignatures().length ||
        option.getConstructSignatures().length || checker.getIndexInfosOfType(option).length))
        return [];

    // A literal shared across the overload family proves which branch an
    // explicitly supplied options object selects. The omitted call is separate.
    const literalFlag = (option, name) => {
        const member = option.getProperty(name);
        const type = member && optionMemberType(option, member);
        return type === checker.getTrueType() ? true : type === checker.getFalseType() ? false : undefined;
    };
    const flag = options[0].getProperties().find(member => {
        if (!/^[a-zA-Z][a-zA-Z0-9_]*$/.test(member.name))
            return false;
        const values = options.map(option => literalFlag(option, member.name));
        return values.every(value => value !== undefined) && new Set(values).size === values.length;
    })?.name;
    if (options.length > 1 && !flag)
        return [];

    return domains.flatMap((domain, index) => {
        const { call, matchesIndex } = domain;
        // An inferred generic parameter can make a competing overload
        // applicable. Keep it in the family before checking our own hole.
        const canOmitEmit = arity === 3 ||
            checker.isTypeAssignableTo(checker.getUndefinedType(), typeOf(call.parameters[2]));
        const type = call.getReturnType();
        let unwrapped = false;
        const returnType = readRef(type, element => {
            if (matchesIndex(element))
                return field;
            const candidate = unwrapRef(type, element);
            if (candidate && matchesIndex(candidate)) {
                unwrapped = true;
                return field;
            }
        });
        if (returnType?.elementType !== field || returnType.writeType)
            return [];

        const fixedBoolean = flag && literalFlag(options[index], flag);
        const suffix = fixedBoolean === true ? 'With' + flag[0].toUpperCase() + flag.slice(1) + 'True' : undefined;
        const fields = unwrapped ? [{ kind: 'string' }, { kind: 'number' }, { kind: 'boolean' }] : [field];
        const projected = fields.flatMap(selected => {
            const record = options[index];
            const members = [];
            const callbackVariants = [];
            for (const member of record.getProperties()) {
                const [declaration] = member.declarations ?? [];
                if (member.declarations?.length !== 1 || !ts.isPropertySignature(declaration) ||
                    !ts.isIdentifier(declaration.name) && !ts.isStringLiteral(declaration.name))
                    return [];

                const omitted = optional(member);
                const memberType = optionMemberType(record, member);
                let value = readDependentOptionValue(ts, checker, typeOf, definition, matchesIndex, selected,
                    memberType, true);
                if (member.name === flag)
                    value = { kind: 'boolean' };
                else if (omitted && value === selected && selected === field)
                    value = { kind: 'optionalSlot', elementType: selected };
                if (!value) {
                    if (!omitted)
                        return [];
                    continue;
                }
                members.push({ name: member.name, type: value, optional: omitted, readOnly: false,
                    definition: definition(declaration), ...(member.name === flag ? { fixedBoolean } : {}) });

                // The computed model supplies a defined prop to clone. The passive
                // watcher can invoke clone when an optional prop becomes undefined.
                if (omitted && returnType.name === 'VueWritableComputedRef' && value.kind === 'boolean' &&
                    memberType.isUnion() && memberType.types.length === 3) {
                    const arm = memberType.types.find(branch =>
                        branch !== checker.getTrueType() && branch !== checker.getFalseType());
                    const callback = readDependentOptionValue(ts, checker, typeOf, definition, matchesIndex, selected, arm);
                    const signature = callback?.signatures?.[0];
                    if (signature?.parameters.length === 1 && signature.parameters[0].type === selected &&
                        signature.returnType === selected && /^[a-zA-Z][a-zA-Z0-9_]*$/.test(member.name)) {
                        callbackVariants.push({
                            index: members.length - 1,
                            type: callback,
                            suffix: 'With' + member.name[0].toUpperCase() + member.name.slice(1) + 'Callback',
                        });
                    }
                }
            }
            if (!members.some(member => member.name !== flag))
                return [];
            const overload = (members, managedNameSuffix, emit) => ({
                ...project(domain, { ...returnType, elementType: selected }, [...emit, {
                    name: call.parameters[optionsIndex].name, type: { kind: 'object', members }, optional: false,
                }], selected),
                ...(managedNameSuffix ? { managedNameSuffix } : {}),
            });
            return propertyEmitArguments(selected).flatMap(emit => [
                overload(members, suffix, emit),
                ...callbackVariants.map(variant => overload(members.map((member, index) =>
                    index === variant.index ? { ...member, type: variant.type } : member),
                (suffix ?? '') + variant.suffix, emit)),
            ]);
        });
        if (unwrapped && fixedBoolean === true && !returnType.name && !returnType.readOnly &&
            options[index].getProperties().every(member => member.name === flag || optional(member))) {
            const recordOptions = options[index];
            // Independent scalar settings (such as watching nested mutations)
            // carry across DTOs. A clone callback or indexed default depends on
            // the payload, and the scalar overload's approximation cannot be reused.
            const members = recordOptions.getProperties().flatMap(member => {
                const type = optionMemberType(recordOptions, member);
                const kind = type === checker.getBooleanType() || member.name === flag ? 'boolean' :
                    type === checker.getStringType() ? 'string' : type === checker.getNumberType() ? 'number' : undefined;
                return kind ? [{ name: member.name, type: { kind }, optional: optional(member), readOnly: false,
                    definition: definition(member.valueDeclaration), ...(member.name === flag ? { fixedBoolean } : {}) }] : [];
            });
            for (const emit of propertyEmitArguments(field)) {
                const trailing = [...emit, { name: call.parameters[optionsIndex].name, optional: false, type: {
                    kind: 'object', members,
                } }];
                const record = project(domain, returnType, trailing);
                record.parameters[1].type.requirePlainRecord = true;
                projected.push({ ...record, managedNameSuffix: suffix + 'Record' });
                // A consumer-owned record with ref properties has different raw
                // and observed shapes. Carry the source as a key schema only.
                const view = project(domain, { kind: 'ref', readOnly: true,
                    elementType: { kind: 'unwrappedRecord', elementType: field } }, trailing);
                view.parameters[1].type.requireRecordView = true;
                projected.push({ ...view, referenceTypeParameters: [field.name], managedNameSuffix: suffix + 'RecordView' });
            }
        }
        return projected;

        function propertyEmitArguments(selected) {
            if (arity === 3)
                return [[]];
            const choices = [];
            if (canOmitEmit)
                choices.push([{ name: call.parameters[2].name, type: { kind: 'void' }, optional: false, fixedUndefined: true }]);
            const emit = readPropertyEmit(ts, checker, domain, typeOf, definition, selected);
            if (emit)
                choices.push([emit]);
            return choices;
        }
    });
}

function readDependentOptionValue(ts, checker, typeOf, definition, matchesIndex, field, type, allowSubset = false) {
    const payload = candidate => {
        if (matchesIndex(candidate))
            return field;
        if (candidate === checker.getStringType())
            return { kind: 'string' };
        if (candidate === checker.getNumberType())
            return { kind: 'number' };
        if (candidate === checker.getBooleanType())
            return { kind: 'boolean' };
    };
    const scalar = payload(type);
    if (scalar)
        return scalar;
    if (allowSubset && type.isUnion()) {
        const defined = type.types.filter(branch => !(branch.flags & ts.TypeFlags.Undefined));
        if (defined.length === 1)
            return readDependentOptionValue(ts, checker, typeOf, definition, matchesIndex, field, defined[0]);
        // The caller controls this field. Preserve a supported scalar arm
        // without restricting the parameter types of a JavaScript callback.
        if (defined.includes(checker.getTrueType()) && defined.includes(checker.getFalseType()))
            return { kind: 'boolean' };
    }
    const [call, ...others] = type.getCallSignatures();
    if (!call || others.length || call.thisParameter || call.typeParameters?.length ||
        call.parameters.length !== call.minArgumentCount || checker.hasEffectiveRestParameter(call) ||
        type.getProperties().length || type.getConstructSignatures().length ||
        checker.getIndexInfosOfType(type).length || checker.getTypePredicateOfSignature(call))
        return;
    const parameters = call.parameters.map(parameter => ({
        name: parameter.name, type: payload(typeOf(parameter)), optional: false,
    }));
    const result = call.getReturnType();
    const returnType = result.flags & ts.TypeFlags.Void ? { kind: 'void' } : payload(result);
    if (returnType && parameters.every(parameter => parameter.type))
        return { kind: 'function', signatures: [{ parameters, returnType, definition: definition(call.declaration) }] };
}

function acceptsTwoArguments(checker, call) {
    return call.minArgumentCount <= 2 && (call.parameters.length >= 2 || checker.hasEffectiveRestParameter(call));
}

function propertyKeyDomain(ts, checker, call, typeOf, allowOmission, optionsArity) {
    const [object, key] = call.parameters;
    if (!object || !key || call.thisParameter || checker.hasEffectiveRestParameter(call) ||
        optionsArity && (call.parameters.length < optionsArity || !call.parameters[optionsArity - 1].valueDeclaration) ||
        allowOmission && !optionsArity && call.parameters.slice(2).some(parameter => !parameter.valueDeclaration ||
            !checker.isOptionalParameter(parameter.valueDeclaration) ||
            unparenthesized(parameter.valueDeclaration.type)?.kind === ts.SyntaxKind.LiteralType))
        return;
    const rootSlot = typeOf(object);
    const keySlot = allowOmission && key.valueDeclaration?.type ?
        checker.getTypeAtLocation(key.valueDeclaration.type) : typeOf(key);
    if (rootSlot === keySlot || !call.typeParameters?.includes(rootSlot) || !call.typeParameters.includes(keySlot))
        return;

    function unparenthesized(node) {
        return node && ts.isParenthesizedTypeNode(node) ? unparenthesized(node.type) : node;
    }
    const constraint = slot => unparenthesized(slot.symbol.declarations?.find(ts.isTypeParameterDeclaration)?.constraint);
    const rootConstraint = constraint(rootSlot);
    if (rootConstraint && rootConstraint.kind !== ts.SyntaxKind.ObjectKeyword)
        return;

    const isRootKey = node => {
        node = unparenthesized(node);
        return ts.isTypeOperatorNode(node) && node.operator === ts.SyntaxKind.KeyOfKeyword &&
            checker.getTypeAtLocation(node.type) === rootSlot;
    };
    const keyConstraint = constraint(keySlot);
    const stringKeys = keyConstraint && ts.isIntersectionTypeNode(keyConstraint) &&
        keyConstraint.types.length === 2 && keyConstraint.types.some(isRootKey) &&
        keyConstraint.types.some(node => unparenthesized(node).kind === ts.SyntaxKind.StringKeyword);
    if (!keyConstraint || !isRootKey(keyConstraint) && !stringKeys)
        return;

    // The checker must bind both sides of T[K] to the same slots as the actual
    // object and key parameters. Similar constraints or assignability do not prove it.
    const matchesIndex = indexed => Boolean(indexed.flags & ts.TypeFlags.IndexedAccess) &&
        indexed.objectType === rootSlot && indexed.indexType === keySlot;
    return { call, object, key, objectBound: Boolean(rootConstraint), stringKeys: Boolean(stringKeys), matchesIndex };
}

// Recognize the declaration's key traversal and indexed lookup by symbol.
// TypeScript's checker often erases an alias after conditional instantiation.
export function dependentPathSetter(ts, checker, call, root, typeOf) {
    if (call?.typeParameters?.length !== 1 || call.parameters.length < 2 || call.parameters.length > 3 ||
        call.minArgumentCount !== 2 || call.thisParameter || checker.hasEffectiveRestParameter(call) ||
        !(call.getReturnType().flags & ts.TypeFlags.Void) ||
        call.parameters.slice(2).some(parameter => checker.getNonNullableType(typeOf(parameter)) !== checker.getBooleanType()))
        return;

    const [key, value] = call.parameters;
    const [slot] = call.typeParameters;
    if (typeOf(key) === slot && readPathDeclaration(ts, checker, call)?.matchesLookup(root, value.valueDeclaration?.type, typeOf(value)))
        return call;
}

export function dependentPathTuple(ts, checker, call, root, typeOf) {
    const [path, value] = call?.typeParameters ?? [];
    const [parameter] = call?.parameters ?? [];
    const lookup = call?.declaration?.typeParameters?.[1]?.default;
    if (!value || !lookup || value.symbol.declarations?.some(declaration => declaration.constraint) ||
        call.minArgumentCount !== 1 || call.thisParameter || checker.hasEffectiveRestParameter(call))
        return;
    const parameterType = typeOf(parameter);
    if (parameterType !== path && !(parameterType.isUnion() && parameterType.types.includes(path)))
        return;
    const declaration = readPathDeclaration(ts, checker, call);
    if (declaration?.matchesLookup(root, lookup, value.getDefault()))
        return { valueSlot: value, arrayIndices: declaration.supportsBracketPaths() };
}

export function supportsBracketPaths(ts, checker, signature) {
    return readPathDeclaration(ts, checker, signature)?.supportsBracketPaths() === true;
}

function readPathDeclaration(ts, checker, call) {
    const [slot] = call.typeParameters ?? [];
    const [slotSyntax] = call.declaration?.typeParameters ?? [];
    const slotSymbol = slotSyntax && checker.getSymbolAtLocation(slotSyntax.name);
    if (!slotSymbol)
        return;

    const bare = node => ts.isParenthesizedTypeNode(node) ? bare(node.type) : node;
    const symbol = node => {
        const name = node && ts.isTypeReferenceNode(node) ? node.typeName : node;
        let resolved = name && checker.getSymbolAtLocation(name);
        if (resolved?.flags & ts.SymbolFlags.Alias)
            resolved = checker.getAliasedSymbol(resolved);
        return resolved;
    };
    const alias = node => node && ts.isTypeReferenceNode(node) &&
        symbol(node)?.declarations?.find(ts.isTypeAliasDeclaration);
    const isSymbol = (node, expected) => symbol(node) === expected;
    const branches = node => !node ? [] : ts.isUnionTypeNode(node) || ts.isIntersectionTypeNode(node) ? node.types : [node];
    const keyof = (node, record) => ts.isTypeOperatorNode(node) && node.operator === ts.SyntaxKind.KeyOfKeyword &&
        isSymbol(node.type, record);
    const isIntersection = (node, first, second) => ts.isIntersectionTypeNode(node) && node.types.length === 2 &&
        (first(node.types[0]) && second(node.types[1]) || second(node.types[0]) && first(node.types[1]));
    const isKind = kind => node => node?.kind === kind;
    const isString = isKind(ts.SyntaxKind.StringKeyword);
    const isNever = isKind(ts.SyntaxKind.NeverKeyword);
    const isTrue = node => ts.isLiteralTypeNode(node) && node.literal.kind === ts.SyntaxKind.TrueKeyword;
    const isFalse = node => ts.isLiteralTypeNode(node) && node.literal.kind === ts.SyntaxKind.FalseKeyword;
    const stringKeys = (node, record) => isIntersection(node, part => keyof(part, record), isString);
    const indexes = (node, record, key) => node && ts.isIndexedAccessTypeNode(node) &&
        isSymbol(node.objectType, record) && isSymbol(node.indexType, key);
    const ref = (node, target, args) => node && ts.isTypeReferenceNode(node) && symbol(node) === target &&
        node.typeArguments?.length === args.length && args.every((matches, index) => matches(node.typeArguments[index]));
    const bound = slotSyntax.constraint;
    const route = alias(bound);
    const declaredRoot = symbol(bound?.typeArguments?.[0]);
    if (route?.typeParameters?.length !== 1 || bound.typeArguments?.length !== 1 || !declaredRoot)
        return;

    const matchesEither = (node, first, second) => ts.isUnionTypeNode(node) && node.types.length === 2 &&
        (first(node.types[0]) && second(node.types[1]) || second(node.types[0]) && first(node.types[1]));
    const templateKey = (node, key, prefix) => ts.isTemplateLiteralTypeNode(node) && node.head.text === prefix &&
        node.templateSpans.length === 1 && isSymbol(node.templateSpans[0].type, key) && node.templateSpans[0].literal.text === '';
    const localParameters = declaration => declaration.typeParameters?.map(parameter => symbol(parameter.name)) ?? [];
    const mappedKeys = (node, record) => ts.isIndexedAccessTypeNode(node) && stringKeys(node.indexType, record) &&
        ts.isMappedTypeNode(node.objectType) && !node.objectType.nameType && node.objectType.type &&
        stringKeys(node.objectType.typeParameter.constraint, record) ? node.objectType : undefined;

    function directRoute(record, body) {
        const mapped = mappedKeys(body, record);
        if (!mapped)
            return false;
        const key = symbol(mapped.typeParameter.name);
        return matchesEither(mapped.type, node => isSymbol(node, key), node => {
            if (!ts.isTemplateLiteralTypeNode(node) || node.head.text !== '' || node.templateSpans.length !== 2)
                return false;
            const [head, tail] = node.templateSpans;
            return isSymbol(head.type, key) && head.literal.text === '.' && tail.literal.text === '' &&
                ref(tail.type, symbol(route.name), [value => indexes(value, record, key)]);
        });
    }

    const isStringLiteral = (node, text) => ts.isLiteralTypeNode(node) && ts.isStringLiteral(node.literal) && node.literal.text === text;
    const standardType = (node, name) => {
        const type = symbol(node);
        return type?.name === name && type.declarations?.every(declaration => declaration.getSourceFile().hasNoDefaultLib);
    };
    const any = isKind(ts.SyntaxKind.AnyKeyword);
    const isObjectRecord = node => standardType(node, 'Record') && ref(node, symbol(node), [isString, any]);
    const isArray = node => standardType(node, 'ReadonlyArray') && ref(node, symbol(node), [any]);

    function isTerminalType(node) {
        node = bare(node);
        if (ts.isUnionTypeNode(node))
            return node.types.every(isTerminalType);
        switch (node.kind) {
            case ts.SyntaxKind.UndefinedKeyword:
            case ts.SyntaxKind.StringKeyword:
            case ts.SyntaxKind.NumberKeyword:
            case ts.SyntaxKind.BigIntKeyword:
            case ts.SyntaxKind.BooleanKeyword:
            case ts.SyntaxKind.SymbolKeyword:
                return true;
            default:
                return ts.isLiteralTypeNode(node) && node.literal.kind === ts.SyntaxKind.NullKeyword ||
                    ['Date', 'File', 'FileList'].some(name => standardType(node, name)) ||
                    Boolean(alias(node)?.type && !node.typeArguments?.length && isTerminalType(alias(node).type));
        }
    }

    function arrayRoute(node, current, visited, helper) {
        const number = node => checker.getTypeAtLocation(node) === checker.getNumberType();
        const withCurrent = node => matchesEither(node, part => isSymbol(part, visited), part => isSymbol(part, current));

        // Check the array branch of the tuple conditional. The CLR selector only
        // exposes SZ arrays, so the tuple branch cannot affect these paths.
        let branch = bare(node);
        if (ts.isConditionalTypeNode(branch) && isTrue(branch.extendsType)) {
            const test = alias(branch.checkType);
            const [parameter] = test ? localParameters(test) : [];
            const condition = test && bare(test.type);
            if (condition && ts.isConditionalTypeNode(condition) && ref(branch.checkType, symbol(test.name), [part => isSymbol(part, current)]) &&
                number(condition.checkType) && ts.isIndexedAccessTypeNode(condition.extendsType) &&
                isSymbol(condition.extendsType.objectType, parameter) && isStringLiteral(condition.extendsType.indexType, 'length') &&
                isFalse(condition.trueType) && isTrue(condition.falseType))
                branch = bare(branch.falseType);
        }

        const renderer = alias(branch);
        if (renderer) {
            const [items, seen] = localParameters(renderer);
            if (!ref(branch, symbol(renderer.name), [part => isSymbol(part, current), withCurrent]))
                return false;
            return rendersIndices(bare(renderer.type), items, part => isSymbol(part, seen));
        }
        return rendersIndices(branch, current, withCurrent);

        function rendersIndices(node, items, traversal) {
            const element = part => ts.isIndexedAccessTypeNode(part) && isSymbol(part.objectType, items) && number(part.indexType);
            const prefix = part => ts.isTemplateLiteralTypeNode(part) && part.head.text === '[' &&
                number(part.templateSpans[0].type) && part.templateSpans[0].literal.text === ']';
            return matchesEither(node, part => prefix(part) && part.templateSpans.length === 1,
                part => prefix(part) && part.templateSpans.length === 2 && part.templateSpans[1].literal.text === '' &&
                    ref(part.templateSpans[1].type, helper, [element, traversal, isFalse]));
        }
    }

    function wrappedRoute(record, body) {
        // The wrapper distributes over its record, then enters the same helper
        // with a root flag. Read only the branch for plain object records.
        if (!ts.isConditionalTypeNode(body) || !isSymbol(body.checkType, record) || !any(body.extendsType) || !isNever(body.falseType))
            return false;
        let helper;
        if (!isIntersection(body.trueType, isString, node => {
            helper = alias(node);
            return helper && ref(node, symbol(helper.name), [part => isSymbol(part, record), part => isSymbol(part, record), isTrue]);
        }))
            return false;

        const [current, visited, first] = localParameters(helper);
        const terminal = bare(helper.type);
        if (helper.typeParameters?.length !== 3 || !ts.isConditionalTypeNode(terminal) ||
            !isSymbol(terminal.checkType, current) || !isTerminalType(terminal.extendsType))
            return false;
        const array = bare(terminal.falseType);
        if (!ts.isConditionalTypeNode(array) || !isSymbol(array.checkType, current) || !isArray(array.extendsType))
            return false;
        const object = bare(array.falseType);
        if (!ts.isConditionalTypeNode(object) || !isSymbol(object.checkType, current) || !isObjectRecord(object.extendsType) ||
            !isStringLiteral(object.falseType, ''))
            return false;
        const objectRoute = alias(object.trueType);
        if (!objectRoute || !ref(object.trueType, symbol(objectRoute.name), [node => isSymbol(node, current),
            node => matchesEither(node, part => isSymbol(part, visited), part => isSymbol(part, current)),
            node => isSymbol(node, first)]))
            return false;

        const [nested, seen, root] = localParameters(objectRoute);
        const mapped = mappedKeys(bare(objectRoute.type), nested);
        const key = mapped && symbol(mapped.typeParameter.name);
        const choice = mapped?.type;
        if (objectRoute.typeParameters?.length !== 3 || !choice || !ts.isConditionalTypeNode(choice) ||
            !isSymbol(choice.checkType, root) || !isTrue(choice.extendsType))
            return false;

        function routeBranches(node, prefix) {
            return matchesEither(node, part => templateKey(part, key, prefix), part => {
                if (!ts.isTemplateLiteralTypeNode(part) || part.head.text !== prefix || part.templateSpans.length !== 2)
                    return false;
                const [head, tail] = part.templateSpans;
                return isSymbol(head.type, key) && head.literal.text === '' && tail.literal.text === '' &&
                    ref(tail.type, symbol(helper.name), [value => indexes(value, nested, key), value => isSymbol(value, seen), isFalse]);
            });
        }
        if (!routeBranches(choice.trueType, '') || !routeBranches(choice.falseType, '.'))
            return false;
        return () => supportsArrayBranch(array.trueType, current, visited, helper);
    }

    function supportsArrayBranch(arrayBranch, current, visited, helper) {
        const indexed = bare(arrayBranch);
        // Selectors may walk recursive DTOs. Only expose bracket paths when
        // the upstream grammar has the same visited-type cutoff.
        if (!ts.isConditionalTypeNode(indexed) || !isTrue(indexed.checkType) || !isNever(indexed.trueType))
            return false;
        const guard = alias(indexed.extendsType);
        const [candidate, target] = guard ? localParameters(guard) : [];
        const proof = guard && bare(guard.type);
        // The guard distributes over the visited types. Its equality branch
        // prunes repeated array types, which the selector must respect.
        if (!proof || !ref(indexed.extendsType, symbol(guard.name), [part => isSymbol(part, visited), part => isSymbol(part, current)]) ||
            !ts.isConditionalTypeNode(proof) || !isSymbol(proof.checkType, candidate) || !isSymbol(proof.extendsType, target) ||
            !isNever(proof.falseType))
            return false;
        const onEqual = bare(proof.trueType);
        if (!ts.isConditionalTypeNode(onEqual) || !isTrue(onEqual.extendsType) || !isTrue(onEqual.trueType) ||
            !isNever(onEqual.falseType))
            return false;
        const equality = alias(onEqual.checkType);
        const [left, right] = equality ? localParameters(equality) : [];
        const test = equality && bare(equality.type);
        const comparison = test && ts.isConditionalTypeNode(test) ? bare(test.trueType) : test;
        const compare = (node, operand) => {
            node = bare(node);
            if (!ts.isFunctionTypeNode(node) || node.parameters.length || node.typeParameters?.length !== 1 ||
                node.typeParameters[0].constraint || node.typeParameters[0].default)
                return false;
            const body = bare(node.type);
            return ts.isConditionalTypeNode(body) && isSymbol(body.checkType, symbol(node.typeParameters[0].name)) &&
                isSymbol(body.extendsType, operand) && checker.getTypeAtLocation(body.trueType).value === 1 &&
                checker.getTypeAtLocation(body.falseType).value === 2;
        };
        if (!test || !ref(onEqual.checkType, symbol(equality.name), [part => isSymbol(part, candidate), part => isSymbol(part, target)]) ||
            !ts.isConditionalTypeNode(test) || !isSymbol(test.checkType, left) || !isSymbol(test.extendsType, right) ||
            !isFalse(test.falseType) || !ts.isConditionalTypeNode(comparison) ||
            !compare(comparison.checkType, left) || !compare(comparison.extendsType, right) ||
            !isTrue(comparison.trueType) || !isFalse(comparison.falseType))
            return false;
        return arrayRoute(indexed.falseType, current, visited, symbol(helper.name));
    }

    function matchesLookup(root, lookupNode, lookupType) {
        const lookup = alias(lookupNode);
        if (!root?.symbol || !lookup || (declaredRoot !== root.symbol &&
            (lookupType?.aliasTypeArguments?.[0] !== root || lookupType?.aliasTypeArguments?.[1] !== slot)) ||
            !ref(lookupNode, symbol(lookup.name), [node => isSymbol(node, declaredRoot), node => isSymbol(node, slotSymbol)]))
            return false;
        const parameters = lookup.typeParameters;
        if (parameters?.length !== 2 || lookupNode.typeArguments.length !== 2)
            return false;
        const [record, path] = parameters.map(parameter => symbol(parameter.name));
        const lookupSymbol = symbol(lookup.name);
        const routeSymbol = symbol(route.name);
        const checksRoute = (node, target) => ref(node, routeSymbol, [target]);
        if (!branches(parameters[1].constraint).some(node => checksRoute(node, argument => isSymbol(argument, record))))
            return false;

        let selector = bare(lookup.type);
        if (ts.isConditionalTypeNode(selector) && isSymbol(selector.checkType, record) &&
            selector.extendsType.kind === ts.SyntaxKind.AnyKeyword && selector.falseType.kind === ts.SyntaxKind.NeverKeyword)
            selector = bare(selector.trueType);
        if (!ts.isConditionalTypeNode(selector) || !isSymbol(selector.checkType, path) || !ts.isTemplateLiteralTypeNode(selector.extendsType))
            return false;
        const [first, rest, ...others] = selector.extendsType.templateSpans;
        if (others.length || selector.extendsType.head.text !== '' || first?.literal.text !== '.' || rest?.literal.text !== '' ||
            !ts.isInferTypeNode(first.type) || !ts.isInferTypeNode(rest.type))
            return false;
        const firstKey = symbol(first.type.typeParameter.name);
        const tailKey = symbol(rest.type.typeParameter.name);
        const headBranch = bare(selector.trueType);
        const leafBranch = bare(selector.falseType);
        if (!ts.isConditionalTypeNode(headBranch) || !isSymbol(headBranch.checkType, firstKey) ||
            !keyof(headBranch.extendsType, record) || !ts.isConditionalTypeNode(leafBranch) ||
            !isSymbol(leafBranch.checkType, path) || !keyof(leafBranch.extendsType, record) ||
            !indexes(leafBranch.trueType, record, path))
            return false;

        const nestedRecord = node => indexes(node, record, firstKey);
        const recursive = (node, tail) => ref(node, lookupSymbol, [nestedRecord, tail]);
        const step = bare(headBranch.trueType);
        if (ts.isConditionalTypeNode(step))
            return isSymbol(step.checkType, tailKey) && checksRoute(step.extendsType, nestedRecord) &&
                recursive(step.trueType, node => isSymbol(node, tailKey)) && isNever(step.falseType);

        return recursive(step, node => isIntersection(node, part => isSymbol(part, tailKey), part => checksRoute(part, nestedRecord)));
    }

    const [record] = localParameters(route);
    const body = bare(route.type);
    if (directRoute(record, body))
        return { matchesLookup, supportsBracketPaths: () => false };
    const supportsBracketPaths = wrappedRoute(record, body);
    if (supportsBracketPaths)
        return { matchesLookup, supportsBracketPaths };
}
