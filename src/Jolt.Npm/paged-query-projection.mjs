const optionNames = ['queryKey', 'initialPageParam', 'queryFn', 'getNextPageParam'];

export function pagedQueryProbes(aliases, callsOf, requested, typeOf) {
    return aliases.flatMap(alias => {
        if (!requested.has(alias.name) || !callsOf(alias).some(call =>
            call.typeParameters?.length && call.minArgumentCount === 1 && call.parameters[0] &&
            branches(typeOf(call.parameters[0])).some(options =>
                optionNames.every(name => options.getProperty(name)))))
            return [];
        const member = JSON.stringify(alias.name);
        return [['string', 'number', "''", '0'], ['number', 'string', '0', "''"]].flatMap(([cursor, page, seed, value]) =>
            ["['jolt', 1, true, { value: 'x' }] as readonly (string | number | boolean | object)[]", "['jolt']"].map(key => ({
                name: alias.name,
                cursor,
                expression: `packageExports[${member}]({
                    queryKey: ${key},
                    initialPageParam: ${seed} as ${cursor},
                    queryFn: async () => (${value} as ${page}),
                    getNextPageParam: () => (${seed} as ${cursor}),
                })`,
            })));
    });
}

export function readPagedQueries(ts, checker, probes, statements, diagnostics, vueRef, promise, definition, readKey) {
    const result = new Map();
    if (!promise || !probes.length)
        return result;
    const scalarTypes = { string: checker.getStringType(), number: checker.getNumberType() };
    const bool = checker.getBooleanType();
    const type = (target, name) => target && checker.getTypeOfPropertyOfType(target, name);
    const same = (left, right) => left && right &&
        checker.isTypeAssignableTo(left, right) && checker.isTypeAssignableTo(right, left) &&
        !(left.flags & ts.TypeFlags.Any);
    const arrayOf = (target, element) => target && checker.isArrayType(target) &&
        same(checker.getTypeArguments(target)[0], element);
    const promiseOf = (target, element) => target.target?.getSymbol() === promise &&
        checker.getTypeArguments(target)[0] === element;
    const absent = target => Boolean(target.flags & (ts.TypeFlags.Null | ts.TypeFlags.Undefined));
    const callback = (options, name, arity, acceptsSkipToken = false) => {
        const variants = branches(type(options, name));
        const callable = variants.filter(variant => variant.getCallSignatures().length);
        if (callable.length !== 1 || !variants.every(variant => callable.includes(variant) ||
            vueRef(variant) || absent(variant) || acceptsSkipToken && variant.flags & ts.TypeFlags.UniqueESSymbol))
            return;
        const calls = callable[0].getCallSignatures();
        const [call] = calls;
        if (calls.length === 1 && call.parameters.length === arity && call.minArgumentCount === arity &&
            !call.thisParameter && !call.typeParameters?.length && !checker.hasEffectiveRestParameter(call))
            return call;
    };

    const witnesses = new Map();
    for (const [index, probe] of probes.entries()) {
        const cursor = scalarTypes[probe.cursor];
        const page = scalarTypes[probe.cursor === 'string' ? 'number' : 'string'];
        const promisedPage = target => promiseOf(target, page);
        const statement = statements[index];
        if (diagnostics.some(diagnostic => diagnostic.start >= statement.pos && diagnostic.start < statement.end))
            continue;
        const call = checker.getResolvedSignature(statement.declarationList.declarations[0].initializer);
        if (!call?.declaration)
            continue;
        const original = checker.getSignatureFromDeclaration(call.declaration);
        const originalOptions = branches(checker.getTypeOfSymbol(original.parameters[0]));
        // A primitive witness alone cannot distinguish a correlated generic
        // from a fixed primitive or a generic constrained to that primitive.
        const openPair = originalOptions.map(options => {
            const next = callback(options, 'getNextPageParam', 4);
            const query = callback(options, 'queryFn', 1, true);
            if (!next || !query)
                return;
            const [genericPage, genericPages, genericCursor, genericCursors] =
                next.parameters.map(parameter => checker.getTypeOfSymbol(parameter));
            const queryOutcomes = branches(query.getReturnType());
            const nextOutcomes = branches(next.getReturnType());
            if (genericPage !== genericCursor && [genericPage, genericCursor].every(parameter =>
                original.typeParameters?.includes(parameter) && !checker.getBaseConstraintOfType(parameter)) &&
                queryOutcomes.some(branch => promiseOf(branch, genericPage)) &&
                queryOutcomes.every(branch => branch === genericPage || promiseOf(branch, genericPage)) &&
                nextOutcomes.includes(genericCursor) && nextOutcomes.some(branch => branch.flags & ts.TypeFlags.Null) &&
                nextOutcomes.every(branch => branch === genericCursor || absent(branch)) &&
                arrayOf(genericPages, genericPage) && arrayOf(genericCursors, genericCursor))
                return { genericPage, genericCursor };
        }).find(Boolean);
        if (!openPair)
            continue;
        const originalData = branches(type(original.getReturnType(), 'data'))
            .flatMap(branch => branches(vueRef(branch)?.element))
            .filter(branch => !absent(branch))
            .map(branch => original.typeParameters.includes(branch)
                ? checker.getDefaultFromTypeParameter(branch) : branch);
        const projectsOriginalArray = (name, parameter) => originalData.length > 0 &&
            originalData.every(branch => {
                const property = type(branch, name);
                return property && checker.isArrayType(property) && checker.getTypeArguments(property)[0] === parameter;
            });
        // A conditional result can look identical for all scalar witnesses.
        // Require an exact open generic element before exposing Pages to C#.
        if (!projectsOriginalArray('pages', openPair.genericPage))
            continue;
        const candidates = branches(checker.getTypeOfSymbol(call.parameters[0]));
        const options = candidates.find(candidate =>
            branches(type(candidate, 'initialPageParam')).some(branch => same(branch, cursor)) &&
            checker.isTypeAssignableTo(checker.createArrayType(checker.getStringType()), type(candidate, 'queryKey')) &&
            type(candidate, 'queryFn') && type(candidate, 'getNextPageParam'));
        if (!options)
            continue;
        const queryKey = readKey(type(options, 'queryKey'));
        if (!queryKey)
            continue;
        const query = callback(options, 'queryFn', 1, true);
        const next = callback(options, 'getNextPageParam', 4);
        if (!query || !next)
            continue;
        const context = checker.getTypeOfSymbol(query.parameters[0]);
        const nextTypes = next.parameters.map(parameter => checker.getTypeOfSymbol(parameter));
        const outcomes = branches(next.getReturnType());
        if (!same(type(context, 'pageParam'), cursor) || !branches(query.getReturnType()).some(promisedPage) ||
            !branches(query.getReturnType()).every(branch => branch === page || promisedPage(branch)) ||
            nextTypes[0] !== page || !arrayOf(nextTypes[1], page) ||
            nextTypes[2] !== cursor || !arrayOf(nextTypes[3], cursor) ||
            !outcomes.includes(cursor) || !outcomes.some(branch => branch.flags & ts.TypeFlags.Null) ||
            !outcomes.every(branch => branch === cursor || absent(branch)))
            continue;

        const returned = call.getReturnType();
        const data = type(returned, 'data');
        const dataRefs = branches(data).map(vueRef);
        if (!dataRefs.length || !dataRefs.every(Boolean))
            continue;
        const dataAlternatives = dataRefs.flatMap(ref => branches(ref.element));
        const [dataValue] = dataAlternatives.filter(branch => !absent(branch));
        const mayBeAbsent = dataAlternatives.some(branch => branch.flags & ts.TypeFlags.Undefined);
        if (!dataValue || !arrayOf(type(dataValue, 'pages'), page) ||
            !dataAlternatives.every(branch => branch === dataValue || branch.flags & ts.TypeFlags.Undefined))
            continue;

        const pageType = { kind: 'typeParameter', name: 'TPage' };
        const cursorType = { kind: 'typeParameter', name: 'TPageParam' };
        const member = (name, type, origin) => ({
            name, type, readOnly: true, definition: definition(origin?.getProperty(name)?.valueDeclaration),
        });
        const argument = (name, type) => ({ name, type, optional: false });
        const array = elementType => ({ kind: 'array', elementType, readOnly: true });
        const record = members => ({ kind: 'object', members });
        const promiseResult = elementType => ({ kind: 'promise', elementType });
        const handler = (parameters, returnType) => ({ kind: 'function', signatures: [{ parameters, returnType }] });
        const pageMembers = [member('pages', array(pageType), dataValue)];
        if (projectsOriginalArray('pageParams', openPair.genericCursor) && arrayOf(type(dataValue, 'pageParams'), cursor))
            pageMembers.push(member('pageParams', array(cursorType), dataValue));
        const pageData = record(pageMembers);
        const resultMembers = [member('data', { kind: 'ref', readOnly: true, elementType: mayBeAbsent
            ? { kind: 'undefinedable', elementType: pageData } : pageData }, returned)];
        const hasNextPage = type(returned, 'hasNextPage');
        if (hasNextPage && vueRef(hasNextPage)?.element === bool)
            resultMembers.push(member('hasNextPage', { kind: 'ref', readOnly: true, elementType: { kind: 'boolean' } }, returned));
        for (const name of ['fetchNextPage', 'refetch']) {
            const calls = type(returned, name)?.getCallSignatures();
            if (calls?.length === 1 && calls[0].minArgumentCount === 0 &&
                calls[0].getReturnType().target?.getSymbol() === promise)
                resultMembers.push(member(name, { kind: 'method', signatures: [
                    { parameters: [], returnType: promiseResult({ kind: 'void' }) },
                ] }, returned));
        }
        const input = record([
            member('queryKey', queryKey, options),
            member('initialPageParam', cursorType, options),
            member('queryFn', handler([argument(query.parameters[0].name, record([
                member('pageParam', cursorType, context),
            ]))], promiseResult(pageType)), options),
            member('getNextPageParam', handler(next.parameters.map((parameter, index) =>
                argument(parameter.name, [pageType, array(pageType), cursorType, array(cursorType)][index])),
            { kind: 'nullable', elementType: cursorType }), options),
        ]);
        const projection = {
            parameters: [argument(call.parameters[0].name, input)],
            returnType: record(resultMembers),
            typeParameters: [pageType.name, cursorType.name],
            // C# needs separate scalar overloads for a nullable value-type cursor.
            scalarResultTypeParameter: cursorType.name,
            definition: definition(call.declaration),
        };
        const key = JSON.stringify([probe.name, queryKey]);
        const earlier = witnesses.get(key);
        // Swapping the two witnesses rules out fixed types in mapped inputs
        // or in the result, including a fixed page inside the return ref.
        if (!result.has(probe.name) && earlier?.cursor !== probe.cursor &&
            JSON.stringify(earlier?.projection) === JSON.stringify(projection))
            result.set(probe.name, projection);
        witnesses.set(key, { cursor: probe.cursor, projection });
    }
    return result;
}

function branches(type) {
    return type?.isUnion() ? type.types : type ? [type] : [];
}
