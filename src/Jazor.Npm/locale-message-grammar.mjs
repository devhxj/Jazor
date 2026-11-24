import { createRequire } from 'node:module';

// NodeTypes is an erased const enum in the locked message-compiler declaration.
const NodeType = { Resource: 0, Plural: 1, Message: 2, Text: 3, Named: 4, List: 5, Literal: 9 };

export function createMessageReader(toolManifest) {
    const require = createRequire(toolManifest);
    const { createParser } = require('@intlify/message-compiler');
    let invalid;
    const parser = createParser({ location: false, onError: () => { invalid = true; } });
    return messages => {
        if (!messages.length)
            return;
        const names = new Set();
        const indices = new Set();
        let plural = false;
        for (const message of messages) {
            invalid = false;
            const resource = parser.parse(message);
            // The parser recovers after errors. Extract a signature only from
            // complete messages and never from a recovered AST.
            if (invalid || resource.type !== NodeType.Resource)
                return;
            const body = resource.body;
            const isPlural = body.type === NodeType.Plural;
            plural ||= isPlural;
            for (const branch of isPlural ? body.cases : [body]) {
                if (branch.type !== NodeType.Message)
                    return;
                for (const item of branch.items) {
                    if (item.type === NodeType.Named) {
                        names.add(item.key);
                    } else if (item.type === NodeType.List) {
                        if (!Number.isSafeInteger(item.index) || item.index < 0)
                            return;
                        indices.add(item.index);
                    } else if (item.type !== NodeType.Text && item.type !== NodeType.Literal) {
                        return;
                    }
                }
            }
        }
        if (plural) {
            // Vue I18n injects both values whenever the call supplies a plural
            // number, including when the current locale has an ordinary message.
            names.delete('count');
            names.delete('n');
        }
        const positions = [...indices].sort((left, right) => left - right);
        // Validate contiguity with memory proportional to the message text.
        if (names.size && positions.length || positions.some((index, position) => index !== position))
            return;
        return { names: [...names].sort(), listArity: positions.length, plural };
    };
}
