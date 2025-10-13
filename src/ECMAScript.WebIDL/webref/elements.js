import { listAll } from "@webref/elements";
import { addToArrayMap } from "../utils/map.js";
async function getInterfaceToElementMap() {
    const all = await listAll();
    const map = new Map();
    for (const item of Object.values(all)) {
        const { elements } = item;
        for (const element of elements) {
            if (!element.interface) {
                continue;
            }
            addToArrayMap(map, element.interface, element);
        }
    }
    return map;
}
export async function getInterfaceElementMergeData() {
    const data = { interfaces: { interface: {} } };
    const map = await getInterfaceToElementMap();
    for (const [key, elements] of map) {
        const namespace = key.startsWith("SVG")
            ? "SVG"
            : key.startsWith("MathML")
                ? "MathML"
                : undefined;
        data.interfaces.interface[key] = {
            element: elements.map((el) => ({
                namespace,
                name: el.name,
                deprecated: el.obsolete,
            })),
        };
    }
    return data;
}
//# sourceMappingURL=elements.js.map