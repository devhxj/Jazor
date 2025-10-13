import { readFile } from "fs/promises";
export async function tryReadFile(path) {
    try {
        return await readFile(path, "utf-8");
    }
    catch (err) {
        if (err.code !== "ENOENT") {
            throw err;
        }
    }
}
//# sourceMappingURL=fs.js.map