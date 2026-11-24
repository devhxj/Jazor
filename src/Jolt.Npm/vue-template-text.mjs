import path from 'node:path';
import { createHash } from 'node:crypto';
import { createRequire } from 'node:module';
import { readTypeScriptInput } from './typescript-input.mjs';

export function templateTextSignature(typescript, source, definitionEnd) {
    const require = createRequire(path.resolve(typescript, '../../../package.json'));
    const vuePath = require.resolve('@vue/language-core');
    const vue = require(vuePath);
    const compiler = createRequire(vuePath)('@vue/compiler-dom');
    const { descriptor, errors } = vue.parse(source);
    const template = descriptor.template;
    if (errors.length || !template || template.__src || (template.lang && template.lang !== 'html'))
        return null;

    let invalid = false;
    const ast = compiler.parse(template.content, {
        comments: true,
        expressionPlugins: ['typescript'],
        onError: () => { invalid = true; },
    });
    if (invalid)
        return null;

    // Only remove literal text past the last exported definition. Preserve every
    // expression, directive, comment and tag, as well as mapped definition offsets.
    const parts = [];
    const offset = template.loc.start.offset;
    let cursor = 0;
    visit(ast);
    parts.push(source.slice(cursor));
    return createHash('sha256').update(JSON.stringify(parts)).digest('hex');

    function visit(node) {
        if (node.type === compiler.NodeTypes.TEXT && node.content.trim()) {
            const start = offset + node.loc.start.offset;
            if (start >= definitionEnd) {
                parts.push(source.slice(cursor, start));
                cursor = offset + node.loc.end.offset;
            }
        }
        for (const child of node.children ?? [])
            visit(child);
    }
}

if (globalThis.__jazorArguments?.length === 2) {
    const [typescript, inputs] = globalThis.__jazorArguments;
    console.log(JSON.stringify(JSON.parse(inputs).map(({ path, fingerprint, definitionEnd }) => {
        const input = readTypeScriptInput(path);
        return input?.hash === fingerprint ? templateTextSignature(typescript, input.text, definitionEnd) : null;
    })));
}
