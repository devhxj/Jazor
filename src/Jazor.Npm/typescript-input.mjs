import { createHash } from 'node:crypto';
import { readFileSync } from 'node:fs';
import path from 'node:path';

export function readTypeScriptInput(file) {
    let bytes;
    try {
        bytes = readFileSync(file);
    } catch {
        return undefined;
    }
    // Hash the original bytes, then decode with the pinned ts.sys.readFile BOM rules.
    const hash = createHash('sha256').update(bytes).digest('hex').toUpperCase();
    let text;
    if (bytes[0] === 0xfe && bytes[1] === 0xff)
        text = bytes.subarray(2, bytes.length & ~1).swap16().toString('utf16le');
    else if (bytes[0] === 0xff && bytes[1] === 0xfe)
        text = bytes.toString('utf16le', 2);
    else {
        const hasBom = bytes[0] === 0xef && bytes[1] === 0xbb && bytes[2] === 0xbf;
        text = bytes.toString('utf8', hasBom ? 3 : 0);
    }
    return { path: path.resolve(file), hash, text };
}
