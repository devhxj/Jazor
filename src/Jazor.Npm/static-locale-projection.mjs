import { finiteNumberLiterals, namedLiteralDomain, namedNumberDomain } from './finite-number-domain.mjs';

export function readStaticLocaleResources(ts, checker, composer, composerSymbol, vueRef, definition, recordIdentity, readParameters, readWriter) {
    if (!composerSymbol || composer.target?.getSymbol() !== composerSymbol)
        return;
    const [resources] = checker.getTypeArguments(composer);
    const messageType = checker.getTypeOfPropertyOfType(composer, 'messages');
    const computed = messageType && vueRef(messageType);
    if (!closedObject(resources) || computed?.name !== 'VueComputedRef' || !computed.readOnly)
        return;

    const managedName = name => name.split(/[-_.]/).filter(Boolean)
        .map(part => part[0].toUpperCase() + part.slice(1)).join('');
    const managedIdentifier = name => /^\p{L}[\p{L}\p{Nd}]*$/u.test(managedName(name));
    const visibleField = name => name !== '__proto__' && /^[\p{L}_][\p{L}\p{Nd}_]*$/u.test(name);
    function distinctMembers(properties) {
        const names = new Map();
        for (const property of properties) {
            const name = managedName(property.name);
            names.set(name, (names.get(name) ?? 0) + 1);
        }
        return properties.filter(property => managedIdentifier(property.name) && names.get(managedName(property.name)) === 1);
    }

    function closedObject(type) {
        return type && !type.isUnionOrIntersection() && Boolean(type.flags & ts.TypeFlags.Object) &&
            !checker.isArrayType(type) && !checker.isTupleType(type) &&
            !type.getSymbol()?.declarations?.some(declaration => ts.isClassDeclaration(declaration) ||
                declaration.getSourceFile().hasNoDefaultLib) &&
            !checker.getIndexInfosOfType(type).length && !type.getCallSignatures().length &&
            !type.getConstructSignatures().length;
    }

    function readRecord(type, leaves, prefix, visiting) {
        if (!closedObject(type) || visiting.has(type))
            return;
        const next = new Set(visiting).add(type);
        const members = distinctMembers(type.getProperties()).flatMap(property => {
            if (!visibleField(property.name))
                return [];
            const original = checker.getTypeOfPropertyOfType(type, property.name);
            const optional = Boolean(property.flags & ts.SymbolFlags.Optional);
            const present = original.isUnion() ? original.types.filter(arm =>
                !(arm.flags & (ts.TypeFlags.Null | ts.TypeFlags.Undefined))) : [original];
            const value = present.length === 1 ? present[0] : checker.getNonNullableType(original);
            const nullable = present.length !== (original.isUnion() ? original.types.length : 1);
            const key = [...prefix, property.name];
            const literalTexts = present.length && present.every(arm =>
                arm.isStringLiteral() && !(arm.flags & ts.TypeFlags.EnumLiteral))
                ? present.map(arm => arm.value) : undefined;
            const kind = literalTexts || value.flags & ts.TypeFlags.StringLike ? 'string' :
                (value.flags & ts.TypeFlags.NumberLike || present.length &&
                    present.every(arm => arm.isNumberLiteral() && Number.isFinite(arm.value))) ? 'number' :
                value === checker.getBooleanType() || value.flags & ts.TypeFlags.BooleanLiteral ? 'boolean' : undefined;
            const projected = kind ? { kind } : readRecord(value, optional || nullable ? new Map() : leaves, key, next);
            if (!projected)
                return [];
            if (!optional && !nullable && literalTexts)
                leaves.set(key.join('.'), literalTexts);
            return [{ name: property.name, definition: definition(property.declarations?.[0]), type: projected,
                optional: optional || nullable, readOnly: true }];
        });
        return members.length ? { kind: 'object', members } : undefined;
    }

    function writeRecord(type, merge, visiting, path) {
        if (!closedObject(type) || visiting.has(type))
            return;
        const next = new Set(visiting).add(type);
        const properties = type.getProperties();
        const visible = new Set(distinctMembers(properties));
        const members = [];
        for (const property of properties) {
            const optional = Boolean(property.flags & ts.SymbolFlags.Optional);
            const original = checker.getTypeOfPropertyOfType(type, property.name);
            const arms = original.isUnion() ? original.types : [original];
            const present = arms.filter(arm => !(arm.flags & (ts.TypeFlags.Null | ts.TypeFlags.Undefined)));
            const value = checker.getNonNullableType(original);
            const nullable = arms.some(arm => arm.flags & ts.TypeFlags.Null);
            const canBeAbsent = optional || present.length !== arms.length;
            const fieldPath = [...path, property.name];
            // A missing or null child can be replaced outright by the merge.
            // Its replacement must carry every required field.
            const field = visible.has(property) && visibleField(property.name)
                ? writeScalar(value, present, property, fieldPath) ??
                    writeRecord(value, merge && !canBeAbsent, next, fieldPath) : undefined;
            if (!field) {
                if (!merge && !optional)
                    return;
                continue;
            }
            members.push({ name: property.name, optional: merge || optional,
                type: nullable ? { kind: 'nullable', elementType: field } : field,
                definition: definition(property.declarations?.[0]) });
        }
        if (members.length || !merge && !properties.length)
            return { kind: 'object', members };
    }

    function writeScalar(value, arms, property, path) {
        if (arms.some(arm => arm.flags & ts.TypeFlags.EnumLiteral))
            return;
        if (value === checker.getStringType())
            return { kind: 'string' };
        if (value === checker.getNumberType())
            return { kind: 'number' };
        if (value === checker.getBooleanType())
            return { kind: 'boolean' };
        if (value === checker.getTrueType() || value === checker.getFalseType())
            return { kind: 'web', name: value === checker.getTrueType() ? 'True' : 'False' };
        const declaration = property.declarations?.[0];
        const strings = arms.length && arms.every(arm => arm.isStringLiteral())
            ? arms.map(arm => arm.value).sort() : undefined;
        const numbers = strings ? undefined : finiteNumberLiterals(ts, arms);
        if (!strings && !numbers)
            return;
        const named = strings ? namedLiteralDomain(ts, value, definition, recordIdentity) :
            namedNumberDomain(ts, checker, value, declaration?.type, numbers, definition, recordIdentity);
        const domain = named ?? fieldDomain(declaration, path, strings ?? numbers);
        return strings ? { kind: 'stringEnum', ...domain, stringLiterals: strings } :
            { kind: 'numberLiterals', ...domain, numberLiterals: numbers };
    }

    function fieldDomain(declaration, path, literals) {
        const sourceIdentity = declaration && recordIdentity(declaration, true);
        const identity = [sourceIdentity, ...path];
        // A generic field can have distinct simultaneous instantiations. For
        // ordinary declarations a copy edit must preserve the field type name.
        for (let owner = declaration?.parent; owner && !ts.isSourceFile(owner); owner = owner.parent) {
            if (owner.typeParameters?.length) {
                identity.push(literals);
                break;
            }
        }
        return { name: path.map(managedName).join('') + 'Value', typeDefinition: definition(declaration),
            typeIdentity: sourceIdentity && JSON.stringify(identity) };
    }

    const locales = resources.getProperties();
    if (!locales.length)
        return;
    const texts = [];
    const visibleLocales = new Set(distinctMembers(locales));
    const writableLocales = [];
    const tables = locales.flatMap(locale => {
        const declared = checker.getTypeOfPropertyOfType(resources, locale.name);
        const actual = checker.getTypeOfPropertyOfType(computed.element, locale.name);
        const leaves = new Map();
        if (!actual || checker.getNonNullableType(declared) !== checker.getNonNullableType(actual))
            return [];
        texts.push({ leaves, table: checker.getNonNullableType(actual),
            required: checker.getNonNullableType(declared) === declared && !(locale.flags & ts.SymbolFlags.Optional) });
        const validName = visibleLocales.has(locale) && /^[A-Za-z][A-Za-z0-9]*(?:-[A-Za-z0-9]+)*$/.test(locale.name);
        const table = readRecord(checker.getNonNullableType(actual), leaves, [], new Set());
        if (validName)
            writableLocales.push({ locale, declared });
        return table && validName ?
            [{ name: locale.name, type: table, optional: true, readOnly: true,
            definition: definition(locale.declarations?.[0]) }] : [];
    });
    if (!tables.length && !writableLocales.length)
        return;

    const occupied = new Set(composer.getProperties().map(property => managedName(property.name)));
    const names = new Map();
    const variants = [{ kind: 'string' }, { kind: 'number' }, { kind: 'boolean' }];
    function isMissing(table, key) {
        for (const part of key.split('.')) {
            if (!closedObject(table))
                return false;
            if (!table.getProperty(part))
                return true;
            table = checker.getTypeOfPropertyOfType(table, part);
        }
        return false;
    }
    const keys = [...new Set(texts.flatMap(locale => [...locale.leaves.keys()]))].sort();
    const translations = keys.flatMap(key => {
        if (texts.length !== locales.length || !texts.some(locale => locale.required && locale.leaves.has(key)) ||
            texts.some(locale => !locale.leaves.has(key) && !isMissing(locale.table, key)))
            return [];
        const values = texts.flatMap(locale => locale.leaves.get(key) ?? []);
        const interpolation = readParameters(values);
        if (!interpolation)
            return [];
        const { names: named, listArity, plural } = interpolation;
        if (named.some(argument => !/^[\p{L}_][\p{L}\p{Nd}_-]*$/u.test(argument)) ||
            distinctMembers(named.map(name => ({ name }))).length !== named.length)
            return [];
        const parameters = named.length ? [{ name: 'named', optional: false, type: { kind: 'object',
            members: named.map(argument => ({ name: argument, optional: false, type: { kind: 'union', variants } })) } }] :
            Array.from({ length: listArity }, (_, index) => ({
                name: 'value' + index, optional: false, type: { kind: 'union', variants },
            }));
        if (plural)
            parameters.push({ name: 'plural', optional: false, type: { kind: 'number' } });
        const name = 't' + managedName(key);
        const normalized = managedName(name);
        if (occupied.has(normalized))
            return [];
        names.set(normalized, (names.get(normalized) ?? 0) + 1);
        return [{ name, jsName: 't', staticKey: key, ...(listArity ? { staticListArity: listArity } : {}), readOnly: true,
            type: { kind: 'method', signatures: [{ parameters, returnType: { kind: 'string' } }] } }];
    });
    const writers = ['setLocaleMessage', 'mergeLocaleMessage'].flatMap(methodName => {
        const method = checker.getTypeOfPropertyOfType(composer, methodName);
        const calls = method?.getCallSignatures() ?? [];
        const acceptsLocale = readWriter(calls, composerSymbol, methodName, resources);
        if (!acceptsLocale)
            return [];
        const merge = methodName === 'mergeLocaleMessage';
        const location = definition(calls[0].declaration);
        return writableLocales.flatMap(({ locale, declared }) => {
            if (!acceptsLocale(locale.name))
                return [];
            const value = checker.getNonNullableType(declared);
            const patch = merge && value === declared && !(locale.flags & ts.SymbolFlags.Optional);
            const type = writeRecord(value, patch, new Set(), [locale.name]);
            const name = methodName + managedName(locale.name);
            const normalized = managedName(name);
            if (!type || occupied.has(normalized))
                return [];
            names.set(normalized, (names.get(normalized) ?? 0) + 1);
            return [{ name, jsName: methodName, staticKey: locale.name, readOnly: true, definition: location,
                type: { kind: 'method', signatures: [{ parameters: [
                    { name: merge ? 'patch' : 'message', type, optional: false },
                ], returnType: { kind: 'void' }, definition: location }] } }];
        });
    });
    return { messages: tables.length ? { kind: 'ref', name: 'VueComputedRef', readOnly: true,
        elementType: { kind: 'object', members: tables } } : undefined,
        methods: [...translations, ...writers].filter(member => names.get(managedName(member.name)) === 1) };
}
