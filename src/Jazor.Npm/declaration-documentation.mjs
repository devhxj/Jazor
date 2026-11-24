export function declarationDocumentation(ts, declaration) {
    const summaries = [];
    const descriptions = [];
    const returns = [];
    const remarks = [];
    const examples = [];
    const deprecated = [];
    const parameters = new Map();
    const typeParameters = new Map();
    // Use the declaration's JSDoc nodes. Symbol documentation can combine
    // multiple declarations and would lose overload-specific descriptions.
    const text = comment => (typeof comment === 'string' ? comment : comment?.map(part => {
        if (part.kind === ts.SyntaxKind.JSDocText) return part.text;
        const target = part.name?.getText() ?? '';
        const label = part.text.trim().replace(/^\|\s*/, '');
        // TypeScript represents the scheme and the rest of an authored URL
        // separately. Join those parts while keeping the URL as plain text.
        return label.startsWith('://') ? target + label : label || target;
    }).join(''))?.trim();
    const parameterText = comment => text(comment)?.replace(/^-\s+/, '');
    const append = (parts, comment) => {
        const value = text(comment);
        if (value) parts.push(value);
    };
    function readTag(tag) {
        switch (tag.tagName.text) {
            case 'summary':
                append(summaries, tag.comment);
                break;
            case 'description':
            case 'desc':
                append(descriptions, tag.comment);
                break;
            case 'param':
            case 'arg':
            case 'argument': {
                const value = parameterText(tag.comment);
                if (value && ts.isJSDocParameterTag(tag))
                    parameters.set(tag.name.getText(), value);
                break;
            }
            case 'template':
            case 'typeParam':
            case 'typeparam': {
                const value = parameterText(tag.comment);
                if (value && ts.isJSDocTemplateTag(tag)) {
                    for (const parameter of tag.typeParameters)
                        typeParameters.set(parameter.name.text, value);
                } else if (value) {
                    const description = /^(\S+)\s+(?:-\s+)?([\s\S]+)$/.exec(value);
                    if (description)
                        typeParameters.set(description[1], description[2]);
                }
                break;
            }
            case 'return':
            case 'returns':
                append(returns, tag.comment);
                break;
            case 'remarks':
                append(remarks, tag.comment);
                break;
            case 'example':
                append(examples, tag.comment);
                break;
            case 'deprecated':
                deprecated.push(text(tag.comment) ?? '');
                break;
        }
    }
    for (const node of ts.getJSDocCommentsAndTags(declaration)) {
        if (ts.isJSDoc(node)) {
            append(summaries, node.comment);
            for (const tag of node.tags ?? [])
                readTag(tag);
        } else {
            readTag(node);
        }
    }

    const documentation = {};
    if (!summaries.length && descriptions.length)
        summaries.push(...descriptions.splice(0));
    const summary = [...new Set(summaries)].join('\n\n');
    for (const description of descriptions) {
        // Date-fns and similar declarations repeat @summary at the start of
        // @description. Keep the additional details as remarks.
        const detail = description === summary ? '' : summary && description.startsWith(summary + '\n')
            ? description.slice(summary.length).trim() : description;
        if (detail) remarks.unshift(detail);
    }
    if (summary) documentation.summary = summary;
    if (parameters.size) documentation.parameters = Object.fromEntries(parameters);
    if (typeParameters.size) documentation.typeParameters = Object.fromEntries(typeParameters);
    if (returns.length) documentation.returns = returns.join('\n\n');
    if (remarks.length) documentation.remarks = remarks.join('\n\n');
    if (examples.length) documentation.examples = examples;
    if (deprecated.length) documentation.deprecated = deprecated.filter(Boolean).join('\n\n');
    return Object.keys(documentation).length ? documentation : undefined;
}
