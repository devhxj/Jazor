import fs from 'node:fs';
import path from 'node:path';
import { createRequire } from 'node:module';
import { fileURLToPath } from 'node:url';

export function resolveRuntimeModule(specifier, entryUrl, resolve, inputs, readText = file => fs.readFileSync(file, 'utf8')) {
    // Only bare package roots participate in the bundler's legacy module-field lookup.
    if (/^(?:@[^/]+\/)?[^@/.:][^/:]*$/.test(specifier)) {
        for (const directory of createRequire(entryUrl).resolve.paths(specifier) ?? []) {
            const root = path.join(directory, specifier);
            const manifest = path.join(root, 'package.json');
            inputs?.add(manifest);
            if (!fs.existsSync(manifest))
                continue;
            const metadata = JSON.parse(readText(manifest));
            // An exports map is authoritative even when it explicitly hides the root.
            if (metadata.exports == null && typeof metadata.module === 'string' && metadata.module.length > 0) {
                const candidate = path.resolve(root, metadata.module);
                const relative = path.relative(root, candidate);
                if (relative && relative !== '..' && !relative.startsWith('..' + path.sep) && !path.isAbsolute(relative)) {
                    inputs?.add(candidate);
                    if (fs.statSync(candidate, { throwIfNoEntry: false })?.isFile())
                        return fs.realpathSync.native(candidate);
                }
            }
            break;
        }
    }
    return fileURLToPath(resolve(specifier));
}
