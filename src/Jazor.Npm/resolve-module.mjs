import fs from 'node:fs';
import path from 'node:path';
import { resolveRuntimeModule } from './runtime-module.mjs';

const [specifier, realpaths] = globalThis.__jazorArguments;
const matchesRealpath = ([file, expected]) => {
    try {
        return path.resolve(fs.realpathSync.native(file)) === expected;
    } catch (error) {
        if (error.code !== 'ENOENT' && error.code !== 'ENOTDIR')
            throw error;
        return false;
    }
};
try {
    const resolved = resolveRuntimeModule(specifier, import.meta.url, import.meta.resolve);
    console.log(JSON.stringify(Object.entries(JSON.parse(realpaths) ?? {}).every(matchesRealpath) ? resolved : null));
} catch (error) {
    if (error.code !== 'ERR_MODULE_NOT_FOUND' && error.code !== 'ERR_PACKAGE_PATH_NOT_EXPORTED')
        throw error;
    console.log('null');
}
