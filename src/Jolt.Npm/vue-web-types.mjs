import path from 'node:path';

export function readWebTypes(ts, packageManifest, readInput) {
    const metadataPath = packageManifest?.metadata['web-types'];
    const byExport = new Map();
    if (typeof metadataPath !== 'string')
        return byExport;

    const file = path.resolve(path.dirname(packageManifest.path), metadataPath);
    const text = readInput(file);
    if (text === undefined)
        return byExport;
    const source = ts.parseJsonText(file, text);
    const property = (object, key) => object?.properties?.find(entry => entry.name?.text === key)?.initializer;
    const string = node => node && ts.isStringLiteral(node) ? node.text : undefined;
    const html = property(property(source.statements[0]?.expression, 'contributions'), 'html');
    for (const component of property(html, 'vue-components')?.elements ?? []) {
        const symbol = property(property(component, 'source'), 'symbol');
        if (!symbol || !ts.isStringLiteral(symbol))
            continue;
        const metadata = byExport.get(symbol.text) ?? { props: new Map(), slots: new Map() };
        byExport.set(symbol.text, metadata);
        metadata.description ??= string(property(component, 'description'));
        for (const prop of property(component, 'props')?.elements ?? []) {
            const name = string(property(prop, 'name'));
            const description = string(property(prop, 'description'));
            if (name && description && !metadata.props.has(name))
                metadata.props.set(name, description);
        }
        const { slots } = metadata;
        for (const slot of property(component, 'slots')?.elements ?? []) {
            const name = property(slot, 'name');
            if (!name || !ts.isStringLiteral(name) || slots.has(name.text))
                continue;
            const type = property(slot, 'type');
            const description = property(slot, 'description')?.text;
            // Web Types publish slot names independently of generated declarations.
            // Scope text is documentation until a checker can resolve its types.
            slots.set(name.text, {
                required: false,
                signatures: type ? [[{ name: 'scope', type: type.text ?? type.getText(source) }]] : [[]],
                definition: {
                    path: file,
                    start: name.getStart(source) + 1,
                    length: name.getWidth(source) - 2,
                    documentation: description ? { summary: description } : undefined,
                },
            });
        }
    }
    return byExport;
}
